using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace System.Data.Sql
{
    /// <summary>
    /// Provides information about a specific SQL Server instance
    /// </summary>
    public sealed class SqlInstance
    {
        private readonly string serverName;
        private readonly string instanceName;
        private readonly bool isClustered;
        private readonly Version version;
        private readonly string namedPipe;
        private readonly int? tcpPort;
        private readonly string rpcName;
        private readonly string spxName;
        private readonly string adspName;

        /// <summary>
        /// The name of the server.
        /// </summary>
        public string ServerName => serverName;

        /// <summary>
        /// The name of the server instance.
        /// </summary>
        public string InstanceName => instanceName;

        /// <summary>
        /// Whether or not the server is part of a cluster.
        /// </summary>
        public bool IsClustered => isClustered;

        /// <summary>
        /// The version of the server instance.
        /// </summary>
        public Version Version => version;

        /// <summary>
        /// The pipe name that the server can be reached on, if any.
        /// </summary>
        public string NamedPipe => namedPipe;

        /// <summary>
        /// The TCP port that the server can be reached on, if any.
        /// </summary>
        public int? TcpPort => tcpPort;
        
        /// <summary>
        /// The name of the computer to connect to for RPC-based connections.
        /// </summary>
        public string RpcName => rpcName;

        /// <summary>
        /// The SPX service name of the server.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Spx")]
        public string SpxName => spxName;

        /// <summary>
        /// The AppleTalk service object name.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Adsp")]
        public string AdspName => adspName;

        /// <summary>
        /// The human readable version of the instance.
        /// </summary>
        public string SqlServerVersion => SqlServerVersions.ContainsKey(Version)
            ? SqlServerVersions[Version]
            : "Unknown";

        internal static SqlInstance Parse(string str)
        {
            return new SqlInstance(str);
        }

        private SqlInstance(string str)
        {
            var strings = str.Split(';');
            for (int i = 0; i < strings.Length; i += 2)
            {
                switch (strings[i])
                {
                    case "ServerName":
                        serverName = strings[i + 1];
                        break;

                    case "InstanceName":
                        instanceName = strings[i + 1];
                        break;

                    case "IsClustered":
                        isClustered = strings[i + 1] == "Yes";
                        break;

                    case "Version":
                        version = Version.Parse(strings[i + 1]);
                        break;

                    case "np":
                        namedPipe = strings[i + 1];
                        break;

                    case "tcp":
                        tcpPort = int.Parse(strings[i + 1], NumberStyles.Integer, NumberFormatInfo.InvariantInfo);
                        break;
                        
                    // TODO: VIA_INFO

                    case "rpc":
                        rpcName = strings[i + 1];
                        break;

                    case "spx":
                        spxName = strings[i + 1];
                        break;

                    case "adsp":
                        adspName = strings[i + 1];
                        break;

                    case "bv":
                        // TODO: BV_INFO
                        i += 4;
                        break;
                }
            }
        }

        private static readonly IDictionary<Version, string> SqlServerVersions = new Dictionary<Version, string>
        {
            {new Version(12, 0, 2000, 80), "SQL Server 2014 RTM"},
            {new Version(11, 0, 5058, 0), "SQL Server 2012 Service Pack 2"},
            {new Version(11, 0, 3000, 0), "SQL Server 2012 Service Pack 1"},
            {new Version(11, 0, 2100, 60), "SQL Server 2012 RTM"},
            {new Version(10, 50, 6000, 34), "SQL Server 2008 R2 Service Pack 3"},
            {new Version(10, 50, 4000, 0), "SQL Server 2008 R2 Service Pack 2"},
            {new Version(10, 50, 2500, 0), "SQL Server 2008 R2 Service Pack 1"},
            {new Version(10, 50, 1600, 1), "SQL Server 2008 R2 RTM"},
            {new Version(10, 0, 5500, 34), "SQL Server 2008 Service Pack 3"},
            {new Version(10, 0, 4000, 0), "SQL Server 2008 Service Pack 2"},
            {new Version(10, 0, 2531, 0), "SQL Server 2008 Service Pack 1"},
            {new Version(10, 0, 1600, 22), "SQL Server 2008 RTM"},
            {new Version(9, 0, 5000, 0), "SQL Server 2005 Service Pack 4"},
            {new Version(9, 0, 4035, 0), "SQL Server 2005 Service Pack 3"},
            {new Version(9, 0, 3042, 0), "SQL Server 2005 Service Pack 2"},
            {new Version(9, 0, 2047, 0), "SQL Server 2005 Service Pack 1"},
            {new Version(9, 0, 1399, 0), "SQL Server 2005 RTM"},
            {new Version(8, 0, 2039, 0), "SQL Server 2000 Service Pack 4"},
            {new Version(8, 0, 760, 0), "SQL Server 2000 Service Pack 3"},
            {new Version(8, 0, 534, 0), "SQL Server 2000 Service Pack 2"},
            {new Version(8, 0, 384, 0), "SQL Server 2000 Service Pack 1"},
            {new Version(8, 0, 194, 0), "SQL Server 2000 RTM"},
        };
    }
}
