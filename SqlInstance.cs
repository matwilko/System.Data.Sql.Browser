﻿/*
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
using System.Globalization;

namespace System.Data.Sql
{
    /// <summary>
    /// Provides information about a specific SQL Server instance
    /// </summary>
    public sealed class SqlInstance
    {
        /// <summary>
        /// The name of the server.
        /// </summary>
        public string ServerName { get; }

        /// <summary>
        /// The name of the server instance.
        /// </summary>
        public string InstanceName { get; }

        /// <summary>
        /// Whether or not the server is part of a cluster.
        /// </summary>
        public bool IsClustered { get; }

        /// <summary>
        /// The version of the server instance.
        /// </summary>
        public Version Version { get; }

        /// <summary>
        /// The pipe name that the server can be reached on, if any.
        /// </summary>
        public string NamedPipe { get; }

        /// <summary>
        /// The TCP port that the server can be reached on, if any.
        /// </summary>
        public int? TcpPort { get; }

        /// <summary>
        /// The name of the computer to connect to for RPC-based connections.
        /// </summary>
        public string RpcName { get; }

        /// <summary>
        /// The SPX service name of the server.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Spx")]
        public string SpxName { get; }

        /// <summary>
        /// The AppleTalk service object name.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Adsp")]
        public string AdspName { get; }

        /// <summary>
        /// The Banyan VINES information for the server instance
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Bv")]
        public BanyanVinesInfo BVInfo { get; }

        /// <summary>
        /// The Virtual Interface Architecture configuration for the server instance
        /// </summary>
        public ViaInfo ViaInfo { get; }

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

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Cyclo-complexity is high, but method itself is simple enough to read and understand")]
        private SqlInstance(string str)
        {
            var strings = str.Split(';');
            for (int i = 0; i < strings.Length; i += 2)
            {
                if (i + 1 > strings.Length)
                {
                    throw new FormatException("Last parameter (" + strings[i] + ") from instance has value truncated");
                }

                switch (strings[i])
                {
                    case "ServerName":
                        ServerName = strings[i + 1];
                        break;

                    case "InstanceName":
                        InstanceName = strings[i + 1];
                        break;

                    case "IsClustered":
                        IsClustered = strings[i + 1] == "Yes";
                        break;

                    case "Version":
                        Version = Version.Parse(strings[i + 1]);
                        break;

                    case "np":
                        NamedPipe = strings[i + 1];
                        break;

                    case "tcp":
                        TcpPort = int.Parse(strings[i + 1], NumberStyles.Integer, NumberFormatInfo.InvariantInfo);
                        break;

                    case "via":
                        ViaInfo = ProcessVia(strings[i+1]);
                        break;

                    case "rpc":
                        RpcName = strings[i + 1];
                        break;

                    case "spx":
                        SpxName = strings[i + 1];
                        break;

                    case "adsp":
                        AdspName = strings[i + 1];
                        break;

                    case "bv":
                        BVInfo = ProcessBanyanVines(strings, ref i);
                        break;
                }
            }
        }

        private static ViaInfo ProcessVia(string viaString)
        {
            var values = viaString.Split(',', ':');
            var netbios = values[0];
            var nic = values[1];
            var port = int.Parse(values[2], NumberStyles.Integer, NumberFormatInfo.InvariantInfo);

            return new ViaInfo(netbios, nic, port);
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "bv")]
        private static BanyanVinesInfo ProcessBanyanVines(string[] strings, ref int i)
        {
            // Check all parts are actually present before end of array
            if (i + 5 > strings.Length - 1)
            {
                throw new FormatException("Last parameter (bv) from instance has values truncated");
            }

            var item = strings[i + 1];
            var group = strings[i + 2];
            var paramItem = strings[i + 3];
            var paramGroup = strings[i + 4];
            var paramOrg = strings[i + 5];
            
            i += 4;
            return new BanyanVinesInfo(item, group, paramItem, paramGroup, paramOrg);
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
