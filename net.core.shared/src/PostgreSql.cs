using System;

namespace Mighty
{
    internal class PostgreSql : DatabasePlugin
    {
        internal override string GetProviderFactoryClassName(string loweredProviderName)
        {
			switch (loweredProviderName)
			{
				case "npgsql":
					return "Npgsql.NpgsqlFactory";

				default:
                    return null;
			}
        }
    }
}