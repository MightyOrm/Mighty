using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Mighty.DatabasePlugins
{
	internal class MySQL //: DatabasePlugin
	{
		/*override*/ static internal string GetProviderFactoryClassName(string loweredProviderName)
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