namespace System.Data.Sql
{
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
        public string Organisation { get; }

        internal BanyanVinesParameters(string item, string group, string organisation)
        {
            Item = item;
            Group = group;
            Organisation = organisation;
        }
    }
}
