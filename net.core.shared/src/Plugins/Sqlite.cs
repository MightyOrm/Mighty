using System;
using System.Data.Common;
using System.Dynamic;
using System.Collections.Generic;

namespace MightyOrm.Plugins
{
	internal class Sqlite : PluginBase
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

		#region SQL
		override public string BuildSelect(string columns, string tableName, string where, string orderBy = null, int limit = 0)
		{
			return BuildLimitSelect(columns, tableName, where, orderBy, limit);
		}

		override public dynamic BuildPagingQueryPair(string columns, string tablesAndJoins, string where, string orderBy,
			int limit, int offset)
		{
			return BuildLimitOffsetPagingQueryPair(columns, tablesAndJoins, where, orderBy, limit, offset);
		}
		#endregion

		#region Table info
		override public string BuildTableMetaDataQuery(string tableName, string tableOwner)
		{
			// does not work with params (not even the inner part)
			return string.Format("PRAGMA {1}table_info({0})", tableName, tableOwner != null ? string.Format("{0}.", tableOwner) : "");
		}

		override public IEnumerable<dynamic> PostProcessTableMetaData(IEnumerable<dynamic> rawTableMetaData)
		{
			var results = new List<dynamic>();
			foreach (dynamic row in rawTableMetaData)
			{
				row.COLUMN_NAME = row.name;
				row.DATA_TYPE = row.type;
				row.COLUMN_DEFAULT = row.dflt_value;
				results.Add(row);
			}
			return results;
		}
		#endregion

		#region Keys and sequences
		override public string IdentityRetrievalFunction { get; protected set; } = "LAST_INSERT_ROWID()";
		#endregion

		#region Prefix/deprefix parameters
		override public string PrefixParameterName(string rawName, DbCommand cmd = null)
		{
			return (cmd != null) ? rawName : ("@" + rawName);
		}
		#endregion
	}
}