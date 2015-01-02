namespace System.Data.Sql
{
    /// <summary>
    /// Banyan VINES information for a SQL Server Instance
    /// </summary>
    public sealed class BanyanVinesInfo
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
        /// The Banyan VINES parameters
        /// </summary>
        public BanyanVinesParameters Parameters { get; }

        internal BanyanVinesInfo(string item, string group, string paramItem, string paramGroup, string paramOrg)
        {
            Item = item;
            Group = group;
            Parameters = new BanyanVinesParameters(paramItem, paramGroup, paramOrg);
        }
    }
}
