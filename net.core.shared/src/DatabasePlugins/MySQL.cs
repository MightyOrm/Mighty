namespace Mighty.DatabasePlugins
{
	internal class MySQL //: DatabasePlugin
	{
		/*override*/ static internal string GetProviderFactoryClassName(string loweredProviderName)
		{
			switch (loweredProviderName)
			{
				case "mysql.data.mysqlclient":
#if true//COREFX
					// was needed for older/beta version; can remove this conditional code change since
					// they're now the same
					//return "MySql.Data.MySqlClient.MySqlClientFactory, MySql.Data.Core";
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