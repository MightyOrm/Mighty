using System;

namespace Mighty.Plugin
{
	internal class MySql : DatabasePlugin
	{
		internal override string GetProviderFactoryClassName(string loweredProviderName)
		{
			switch (loweredProviderName)
			{
				case "mysql.data.mysqlclient":
#if COREFX
					//return "MySql.Data.MySqlClient.MySqlClientFactory, MySql.Data.Core"; // older/beta version
					return "MySql.Data.MySqlClient.MySqlClientFactory, MySql.Data";
#else
					return "MySql.Data.MySqlClient.MySqlClientFactory, MySql.Data";
#endif

				case "devart.data.mysql":
					return "Devart.Data.MySql.MySqlProviderFactory";

				default:
					return null;
			}
		}
	}
}