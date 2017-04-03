using System;

namespace Mighty.Plugin
{
	internal class SqlServer : DatabasePlugin
	{
		internal override string GetProviderFactoryClassName(string loweredProviderName)
		{
			switch (loweredProviderName)
			{
				case "system.data.sqlclient":
					return "System.Data.SqlClient.SqlClientFactory";

				default:
					return null;
			}
		}
	}
}