using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace System.Data.Sql.Browser
{
    public static class SqlServerBrowser
    {
        private const int SqlServerBrowserPort = 1434;
        private const int Timeout = 3000;

        private static UdpClient Client => new UdpClient
        {
            DontFragment = true,
            ExclusiveAddressUse = true,
            MulticastLoopback = false,
            Client = {ReceiveTimeout = Timeout}
        };

        /// <summary>
        /// Searches for all available instances in the subnet.
        /// </summary>
        /// <remarks>
        /// The returned enumerable uses delayed execution, that is that the query is only sent when enumeration begins (and is sent everytime enumeration occurs!)
        /// Additionally, instance information will be yielded as it is available from other hosts piecemeal, the enumerable will not wait on all hosts to return information and timeout.
        /// </remarks>
        /// <returns>A delayed execution enumerable that yields SQL Server instance information from the local network.</returns>
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
        /// <param name="ipAddresses">The remote hosts to query for instances.</param>
        /// <returns>A delayed execution enumerable that yields SQL Server instance information from the local network.</returns>
        public static IEnumerable<SqlInstance> GetInstancesOn(params IPAddress[] ipAddresses)
        {
            using (var client = Client)
            {
                var datagram = Messages.ClientUnicastEx();
                foreach (var address in ipAddresses)
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
                catch (Exception)
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
    }
}
