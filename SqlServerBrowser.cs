/*
Copyright 2014 Matthew Wilkinson

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace System.Data.Sql
{
    /// <summary>
    /// Provides methods that query SQL Browser on remote hosts to obtain instance or DAC information
    /// </summary>
    public static class Browser
    {
        private const int SqlServerBrowserPort = 1434;

        /// <summary>
        /// The default timeout period for any requests made via this class
        /// </summary>
        public static int DefaultTimeout => 3000;

        /// <summary>
        /// Searches for all available instances in the subnet.
        /// </summary>
        /// <remarks>
        /// The returned enumerable uses delayed execution, that is that the query is only sent when enumeration begins (and is sent everytime enumeration occurs!)
        /// Additionally, instance information will be yielded as it is available from other hosts piecemeal, the enumerable will not wait on all hosts to return information and timeout.
        /// </remarks>
        /// <param name="timeout">The number of milliseconds to wait for more hosts to respond</param>
        /// <returns>A delayed execution enumerable that yields SQL Server instance information from the local network.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Operation requires long delay waiting for network timeout")]
        public static IEnumerable<SqlInstance> GetInstances(int timeout)
        {
            using (var client = Client)
            {
                client.EnableBroadcast = true;
                client.Client.ReceiveTimeout = timeout;

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
        /// Searches for all available instances in the subnet.
        /// </summary>
        /// <remarks>
        /// The returned enumerable uses delayed execution, that is that the query is only sent when enumeration begins (and is sent everytime enumeration occurs!)
        /// Additionally, instance information will be yielded as it is available from other hosts piecemeal, the enumerable will not wait on all hosts to return information and timeout.
        /// </remarks>
        /// <returns>A delayed execution enumerable that yields SQL Server instance information from the local network.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "Operation requires long delay waiting for network timeout")]
        public static IEnumerable<SqlInstance> GetInstances()
        {
            return GetInstances(DefaultTimeout);
        }

        /// <summary>
        /// Searches for all available instances at the specified IP Addresses.
        /// </summary>
        /// <remarks>
        /// The returned enumerable uses delayed execution, that is that the query is only sent when enumeration begins (and is sent everytime enumeration occurs!)
        /// Additionally, instance information will be yielded as it is available from other hosts piecemeal, the enumerable will not wait on all hosts to return information and timeout.
        /// </remarks>
        /// <param name="addresses">The remote hosts to query for instances.</param>
        /// <param name="timeout">The number of milliseconds to wait for more hosts to respond</param>
        /// <returns>A delayed execution enumerable that yields SQL Server instance information from the local network.</returns>
        public static IEnumerable<SqlInstance> GetInstancesOn(int timeout, IEnumerable<IPAddress> addresses)
        {
            using (var client = Client)
            {
                client.Client.ReceiveTimeout = timeout;

                var datagram = Messages.ClientUnicastEx();
                foreach (var endpoint in addresses.Select(address => new IPEndPoint(address, SqlServerBrowserPort)))
                {
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
        /// Searches for all available instances at the specified IP Addresses.
        /// </summary>
        /// <remarks>
        /// The returned enumerable uses delayed execution, that is that the query is only sent when enumeration begins (and is sent everytime enumeration occurs!)
        /// Additionally, instance information will be yielded as it is available from other hosts piecemeal, the enumerable will not wait on all hosts to return information and timeout.
        /// </remarks>
        /// <param name="addresses">The remote hosts to query for instances.</param>
        /// <param name="timeout">The number of milliseconds to wait for more hosts to respond</param>
        /// <returns>A delayed execution enumerable that yields SQL Server instance information from the local network.</returns>
        public static IEnumerable<SqlInstance> GetInstancesOn(int timeout, params IPAddress[] addresses)
        {
            return GetInstancesOn(timeout, addresses.AsEnumerable());
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
        public static IEnumerable<SqlInstance> GetInstancesOn(IEnumerable<IPAddress> addresses)
        {
            return GetInstancesOn(DefaultTimeout, addresses);
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
            return GetInstancesOn(DefaultTimeout, addresses);
        }

        /// <summary>
        /// Get information about a specific instance
        /// </summary>
        /// <param name="address">The host on which the instance is running</param>
        /// <param name="instanceName">The name of the instance</param>
        /// <param name="timeout">The number of milliseconds to wait for the host to respond</param>
        /// <returns>The SQL Server instance information</returns>
        /// <exception cref="InvalidOperationException">Thrown if instance does not exist on target server</exception>
        public static SqlInstance GetInstance(IPAddress address, string instanceName, int timeout)
        {
            using (var client = Client)
            {
                client.Client.ReceiveTimeout = timeout;

                var datagram = Messages.ClientUnicastInstance(instanceName);
                var endpoint = new IPEndPoint(address, SqlServerBrowserPort);
                client.Connect(endpoint);
                client.Send(datagram, datagram.Length, endpoint);

                try
                {
                    return ProcessIncomingInstanceData(client, 1).Single();
                }
                catch (InvalidOperationException ex) if (ex.Message == "Sequence contains no elements")
                {
                    throw new InvalidOperationException("Instance does not exist on target server");
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
            return GetInstance(address, instanceName, DefaultTimeout);
        }

        /// <summary>
        /// Obtains the Dedicated Administrator Connection port number for an instance
        /// </summary>
        /// <param name="address">The host on which the instance is running</param>
        /// <param name="instanceName">The name of the instance</param>
        /// <param name="timeout">The number of milliseconds to wait for the host to respond</param>
        /// <returns>The port number of the DAC</returns>
        /// <exception cref="InvalidOperationException">Thrown if instance does not exist on target server, or the DAC for that instance is not available</exception>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
        public static int GetDacPort(IPAddress address, string instanceName, int timeout)
        {
            using (var client = Client)
            {
                client.Client.ReceiveTimeout = timeout;

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
                catch (SocketException ex) if (ex.SocketErrorCode == SocketError.TimedOut)
                {
                    throw new InvalidOperationException("Instance does not exist on target server, or the DAC is not available.");
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
            return GetDacPort(address, instanceName, DefaultTimeout);
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
                catch (SocketException ex) if (ex.SocketErrorCode != SocketError.TimedOut)
                {
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
            Client = {ReceiveTimeout = DefaultTimeout}
        };
    }
}
