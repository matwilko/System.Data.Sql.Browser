namespace System.Data.Sql
{
    /// <summary>
    /// Banyan VINES parameter information for a SQL Server instance
    /// </summary>
    public sealed class BanyanVinesParameters
    {
        /// <summary>
        /// The Banyan VINES item name
        /// </summary>
        public string Item { get; }

        /// <summary>
        /// The Banyan VINES group name
        /// </summary>
        public string Group { get; }

        /// <summary>
        /// The Banyan VINES organization name
        /// </summary>
        public string Organization { get; }

        internal BanyanVinesParameters(string item, string group, string organization)
        {
            Item = item;
            Group = group;
            Organization = organization;
        }
    }
}
