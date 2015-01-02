namespace System.Data.Sql
{
    public sealed class ViaInfo
    {
        /// <summary>
        /// The NetBIOS name of a machine where the server resides
        /// </summary>
        public string NetBios { get; }

        /// <summary>
        /// The VIA network interface card (NIC) identifier
        /// </summary>
        public string Nic { get; }

        /// <summary>
        /// The VIA NIC's port
        /// </summary>
        public int Port { get; }

        internal ViaInfo(string netBios, string nic, int port)
        {
            NetBios = netBios;
            Nic = nic;
            Port = port;
        }
    }
}
