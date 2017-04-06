using System;
using System.Data;
using System.Data.Common;

namespace Mighty.DatabasePlugins
{
	internal class MySQL : DatabasePlugin
	{
#region Provider support
		// we must use new because there are no overrides on static methods
		// e.g. http://stackoverflow.com/q/7839691
		new static internal string GetProviderFactoryClassName(string loweredProviderName)
		{
			switch (loweredProviderName)
			{
				case "mysql.data.mysqlclient":
					// older/beta qualified class name on COREFX was:
					//return "MySql.Data.MySqlClient.MySqlClientFactory, MySql.Data.Core";
					return "MySql.Data.MySqlClient.MySqlClientFactory, MySql.Data";

				case "devart.data.mysql":
					return "Devart.Data.MySql.MySqlProviderFactory";

				default:
					return null;
			}
		}
#endregion

#region SQL
		// Build a single query which returns two result sets: a scalar of the total count followed by
		// a normal result set of the page of items.
		// This really does vary per DB and can't be a standard virtual method which most things share.
		override public string BuildPagingQuery(string columns, string tablesAndJoins, string orderBy, string where = null,
			int pageSize = 1, int currentPage = 20)
		{
			throw new NotImplementedException();
		}
#endregion

#region Table info
		// owner is for owner/schema, will be null if none was specified
		// This really does vary per DB and can't be a standard virtual method which most things share.
		override public string BuildTableInfoQuery(string owner, string tableName)
		{
			throw new NotImplementedException();
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

#region DbCommand
		override public DbDataReader ExecuteDereferencingReader(DbCommand cmd, DbConnection conn)
		{
			throw new NotImplementedException();
		}
		override public bool RequiresWrappingTransaction(DbCommand cmd)
		{
			throw new NotImplementedException();
		}
#endregion

#region DbParameter
		override public void SetDirection(DbParameter p, ParameterDirection direction)
		{
			throw new NotImplementedException();
		}
		override public void SetValue(DbParameter p, object value)
		{
			throw new NotImplementedException();
		}
		override public object GetValue(DbParameter p)
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