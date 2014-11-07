using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace System.Data.Sql
{
    public static class Browser
    {
        private const int SqlServerBrowserPort = 1434;
        private const int Timeout = 3000;



        /// <summary>
        /// Searches for all available instances in the subnet.
        /// </summary>
        /// <remarks>
        /// The returned enumerable uses delayed execution, that is that the query is only sent when enumeration begins (and is sent everytime enumeration occurs!)
        /// Additionally, instance information will be yielded as it is available from other hosts piecemeal, the enumerable will not wait on all hosts to return information and timeout.
        /// </remarks>
        /// <returns>A delayed execution enumerable that yields SQL Server instance information from the local network.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Operation requires long delay waiting for network timeout")]
        public static IEnumerable<SqlInstance> GetInstances()
        {
            var client = Client;
            Client.EnableBroadcast = true;

            using (client)
            {
                var endpoint = new IPEndPoint(IPAddress.Broadcast, SqlServerBrowserPort);
                var datagram = Messages.ClientBroadcastEx();
                client.Send(datagram, datagram.Length, endpoint);

                // Could return directly, but then we lose the deferred execution benefits!
                foreach (var instance in ProcessIncomingInstanceData(client, 255))
                {
                    yield return instance;
                }
            }
        }

        /// <summary>
        /// Searches for all available instances at the specified IP Addresses.
        /// </summary>
        /// <remarks>
        /// The returned enumerable uses delayed execution, that is that the query is only sent when enumeration begins (and is sent everytime enumeration occurs!)
        /// Additionally, instance information will be yielded as it is available from other hosts piecemeal, the enumerable will not wait on all hosts to return information and timeout.
        /// </remarks>
        /// <param name="addresses">The remote hosts to query for instances.</param>
        /// <returns>A delayed execution enumerable that yields SQL Server instance information from the local network.</returns>
        public static IEnumerable<SqlInstance> GetInstancesOn(params IPAddress[] addresses)
        {
            using (var client = Client)
            {
                var datagram = Messages.ClientUnicastEx();
                foreach (var address in addresses)
                {
                    var endpoint = new IPEndPoint(address, SqlServerBrowserPort);
                    client.Send(datagram, datagram.Length, endpoint);
                }

                // Could return directly, but then we lose the deferred execution benefits!
                foreach (var instance in ProcessIncomingInstanceData(client, 255))
                {
                    yield return instance;
                }
            }
        }

        /// <summary>
        /// Get information about a specific instance
        /// </summary>
        /// <param name="address">The host on which the instance is running</param>
        /// <param name="instanceName">The name of the instance</param>
        /// <returns>The SQL Server instance information</returns>
        /// <exception cref="InvalidOperationException">Thrown if instance does not exist on target server</exception>
        public static SqlInstance GetInstance(IPAddress address, string instanceName)
        {
            using (var client = Client)
            {
                var datagram = Messages.ClientUnicastInstance(instanceName);
                var endpoint = new IPEndPoint(address, SqlServerBrowserPort);
                client.Connect(endpoint);
                client.Send(datagram, datagram.Length, endpoint);

                try
                {
                    return ProcessIncomingInstanceData(client, 1).Single();
                }
                catch (InvalidOperationException ex)
                {
                    if (ex.Message != "Sequence contains no elements")
                    {
                        throw;
                    }

                    throw new InvalidOperationException("Instance does not exist on target server");
                }
            }
        }

        /// <summary>
        /// Obtains the Dedicated Administrator Connection port number for an instance
        /// </summary>
        /// <param name="address">The host on which the instance is running</param>
        /// <param name="instanceName">The name of the instance</param>
        /// <returns>The port number of the DAC</returns>
        /// <exception cref="InvalidOperationException">Thrown if instance does not exist on target server, or the DAC for that instance is not available</exception>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
        public static int GetDacPort(IPAddress address, string instanceName)
        {
            using (var client = Client)
            {
                var datagram = Messages.ClientUnicastDac(instanceName);
                var endpoint = new IPEndPoint(address, SqlServerBrowserPort);

                client.Connect(endpoint);
                client.Send(datagram, datagram.Length);

                IPEndPoint remoteEndpoint = null;
                try
                {
                    var response = client.Receive(ref remoteEndpoint);
                    return Messages.ServerResponseDac(response);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode != SocketError.TimedOut)
                    {
                        throw;
                    }

                    throw new InvalidOperationException("Instance does not exist on target server, or the DAC is not available.");
                }
                
            }
        }
        
        private static IEnumerable<SqlInstance> ProcessIncomingInstanceData(UdpClient client, int expectedClients)
        {
            IPEndPoint endPoint = null;

            for (int i = 0; i < expectedClients; i++)
            {
                byte[] bytes;
                try
                {
                    bytes = client.Receive(ref endPoint);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode != SocketError.TimedOut)
                    {
                        throw;
                    }

                    yield break;
                }

                var instanceData = Messages.ServerResponse(bytes)
                    .Split(new[] {";;"}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(SqlInstance.Parse);

                foreach (var instance in instanceData)
                {
                    yield return instance;
                }
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "All consumers obtain client in using block")]
        private static UdpClient Client => new UdpClient
        {
            DontFragment = true,
            ExclusiveAddressUse = true,
            MulticastLoopback = false,
            Client = {ReceiveTimeout = Timeout}
        };
    }
}
