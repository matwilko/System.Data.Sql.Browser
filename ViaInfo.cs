using System.Diagnostics.CodeAnalysis;

namespace System.Data.Sql
{
    /// <summary>
    /// Virtual Interface Architecture information for a SQL Server instance
    /// </summary>
    public sealed class ViaInfo
    {
        /// <summary>
        /// The NetBIOS name of a machine where the server resides
        /// </summary>
        public string NetBios { get; }
        
        /// <summary>
        /// The VIA network interface card (NIC) identifier
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Nic")]
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
