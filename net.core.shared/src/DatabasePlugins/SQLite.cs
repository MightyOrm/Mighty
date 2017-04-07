using System.Data.Common;

namespace Mighty.DatabasePlugins
{
	internal class SQLite : DatabasePlugin
	{
#region Provider support
		// we must use new because there are no overrides on static methods, see e.g. http://stackoverflow.com/q/7839691
		new static internal string GetProviderFactoryClassName(string loweredProviderName)
		{
			switch (loweredProviderName)
			{
				case "system.data.sqlite":
					return "System.Data.SQLite.SQLiteFactory";

				case "microsoft.data.sqlite":
					return "Microsoft.Data.Sqlite.SqliteFactory";

				default:
					return null;
			}
		}
#endregion

#region Table info
		override public string BuildTableInfoQuery(string owner, string tableName)
		{
			// SQLite does not have schema/owner
			return string.Format("PRAGMA table_info({0})", tableName);
		}
#endregion

#region Prefix/deprefix parameters
		override public string PrefixParameterName(string rawName, DbCommand cmd = null)
		{
			return (cmd != null) ? rawName : ("@" + rawName);
		}
#endregion
	}
}