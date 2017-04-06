using System.Data;
using System.Data.Common;

namespace Mighty.DatabasePlugins
{
	abstract public class DatabasePlugin
	{
		public MightyORM mighty { get; internal set; }

		abstract public string GetProviderFactoryClassName(string providerName);

		virtual public string BuildSelect(string columns, string tableName, string where, string orderBy)
		{
			return string.Format("SELECT {0} FROM {1}{2}{3}",
				columns, tableName, mighty.Thingify("WHERE", where), mighty.Thingify("ORDER BY", orderBy));
		}

		virtual public string BuildDelete(string tableName, string where)
		{
			return string.Format("DELETE FROM {0}{1}",
				tableName, mighty.Thingify("WHERE", where));
		}

		virtual public string BuildInsert(string tableName, string columns, string values)
		{
			return string.Format("INSERT {0} ({1}) VALUES {2}",
				tableName, columns, values);
		}

		virtual public string BuildUpdate(string tableName, string values, string where)
		{
			return string.Format("UPDATE {0} SET {1}{2}",
				tableName, values, mighty.Thingify("WHERE", where));
		}

		abstract public string BuildPagingQuery()

		// Needs to know whether this is for a DbParameter name (cmd=null) or for escaping within the SQL fragment itself,
		// and if it is for a DbParameter whether it is used for a stored procedure or for a SQL fragment.
		abstract public string PrefixParameterName(string rawName, DbCommand cmd = null);
		// Will always be from a DbParameter, but needs to know whether it was used for
		// a stored procedure or for a SQL fragment.
		abstract public string DeprefixParameterName(string dbParamName, DbCommand cmd);

#region DbCommand
		abstract public DbDataReader ExecuteDereferencingReader(DbCommand cmd, DbConnection conn);
		abstract public bool RequiresWrappingTransaction(DbCommand cmd);
#endregion

#region DbParameter
		abstract public void SetDirection(DbParameter p, ParameterDirection direction);
		abstract public void SetValue(DbParameter p, object value);
		abstract public object GetValue(DbParameter p);
		abstract public bool SetCursor(DbParameter p, object value);
		abstract public bool IsCursor(DbParameter p);
		abstract public bool SetAnonymousParameter(DbParameter p);
		abstract public bool IgnoresOutputTypes(DbParameter p);
#endregion
	}
}