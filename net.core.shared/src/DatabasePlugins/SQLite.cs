using System.Data.Common;
using System.Collections.Generic;

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

		override public IEnumerable<dynamic> NormalizeTableInfo(IEnumerable<dynamic> rawTableInfo)
		{
			// TO DO: TEST that this and the other ones are working
			// NB various null checks removed in this one, were these needed?
			var result = new List<dynamic>();
			foreach (var row in rawTableInfo)
			{
				var rowAsDictionary = row.AsDictionary();
				result.Add(new
				{
					// Taken from Massive - see CREDITS file
					COLUMN_NAME = rowAsDictionary["name"].ToString(),
					DATA_TYPE = rowAsDictionary["type"].ToString(),
					IS_NULLABLE = rowAsDictionary["notnull"].ToString() == "0" ? "NO" : "YES",
					COLUMN_DEFAULT = rowAsDictionary["dflt_value"] ?? string.Empty,
				});
			}
			return result;
		}
#endregion

#region Keys and sequences
		override public string KeyRetrievalFunction { get; protected set; } = "LAST_INSERT_ROWID()";
#endregion

#region Prefix/deprefix parameters
		override public string PrefixParameterName(string rawName, DbCommand cmd = null)
		{
			return (cmd != null) ? rawName : ("@" + rawName);
		}
#endregion
	}
}