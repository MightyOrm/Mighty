using System;
using System.Data;
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

#region SQL
		// Build a single query which returns two result sets: a scalar of the total count followed by
		// a normal result set of the page of items.
		// This really does vary per DB and can't be a standard virtual method which most things share.
		override public string BuildPagingQuery(string columns, string tablesAndJoins, string orderBy, string where,
			int limit, int offset)
		{
			throw new NotImplementedException();
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
		// Needs to know whether this is for use in DbParameter name (cmd=null) or for escaping within the SQL fragment itself,
		// and if it is for a DbParameter whether it is used for a stored procedure or for a SQL fragment.
		override public string PrefixParameterName(string rawName, DbCommand cmd = null)
		{
			throw new NotImplementedException();
		}
		// Will always be from a DbParameter, but needs to know whether it was used for
		// a stored procedure or for a SQL fragment.
		override public string DeprefixParameterName(string dbParamName, DbCommand cmd)
		{
			throw new NotImplementedException();
		}
#endregion

#region DbParameter
		override public void SetValue(DbParameter p, object value)
		{
			throw new NotImplementedException();
		}
		override public object GetValue(DbParameter p)
		{
			throw new NotImplementedException();
		}
		override public void SetDirection(DbParameter p, ParameterDirection direction)
		{
			throw new NotImplementedException();
		}
		override public bool SetCursor(DbParameter p, object value)
		{
			throw new NotImplementedException();
		}
		override public bool IsCursor(DbParameter p)
		{
			throw new NotImplementedException();
		}
		override public bool SetAnonymousParameter(DbParameter p)
		{
			throw new NotImplementedException();
		}
		override public bool IgnoresOutputTypes(DbParameter p)
		{
			throw new NotImplementedException();
		}
#endregion
	}
}