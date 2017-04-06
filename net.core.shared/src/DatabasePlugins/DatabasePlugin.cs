using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Mighty.DatabasePlugins
{
	abstract public class DatabasePlugin
	{
		// the instance which we are pluged in to
		public MightyORM mighty { get; internal set; }

#region Provider support
		// Returns the provider factory class name for the known provider(s) for this DB.
		//
		// There is no C# syntax to enforce sub-classes of DatabasePlugin to provide a static method with this name,
		// but they must do so.
		//
		// If you wan't to plug in a new provider for a known database, simply subclass the plugin of
		// that database and provide a different implementation of this method. Then, either call DatabasePluginManager.RegisterPlugin
		// to use it with extended connection strings, or else pass to the MightyORM constructor via your own
		// sub-class of ConnectionProvider.
		//
		static public string GetProviderFactoryClassName(string providerName)
		{
			// NB because of the way static methods work in C#, this method can never be found and called from
			// a sub-class.
			throw new NotImplementedException(string.Format("{0} should only ever be called on sub-classes of {1}",
				nameof(GetProviderFactoryClassName), typeof(DatabasePlugin)));
		}
#endregion

#region SQL
		// is the same for every (currently supported?) database
		virtual public string BuildSelect(string columns, string tableName, string where, string orderBy)
		{
			return string.Format("SELECT {0} FROM {1}{2}{3}",
				columns, tableName, mighty.Thingify("WHERE", where), mighty.Thingify("ORDER BY", orderBy));
		}

		// is the same for every (currently supported?) database
		virtual public string BuildDelete(string tableName, string where)
		{
			return string.Format("DELETE FROM {0}{1}",
				tableName, mighty.Thingify("WHERE", where));
		}

		// is the same for every (currently supported?) database
		virtual public string BuildInsert(string tableName, string columns, string values)
		{
			return string.Format("INSERT {0} ({1}) VALUES {2}",
				tableName, columns, values);
		}

		// is the same for every (currently supported?) database
		virtual public string BuildUpdate(string tableName, string values, string where)
		{
			return string.Format("UPDATE {0} SET {1}{2}",
				tableName, values, mighty.Thingify("WHERE", where));
		}

		// Build a single query which returns two result sets: a scalar of the total count followed by
		// a normal result set of the page of items.
		// This really does vary per DB and can't be a standard virtual method which most things share.
		abstract public string BuildPagingQuery(string columns, string tablesAndJoins, string orderBy, string where = null,
			int pageSize = 1, int currentPage = 20);
#endregion

#region Table info
		// owner is for owner/schema, will be null if none was specified
		// This really does vary per DB and can't be a standard virtual method which most things share.
		abstract public string BuildTableInfoQuery(string owner, string tableName);

		// if the table info comes from the semi-standard INFORMATION_SCHEMA table then we don't need to override this 
		virtual public IEnumerable<dynamic> NormaliseTableInfo(IEnumerable<dynamic> results) { return results; }
#endregion

#region Prefix/deprefix parameters
		// Needs to know whether this is for use in DbParameter name (cmd=null) or for escaping within the SQL fragment itself,
		// and if it is for a DbParameter whether it is used for a stored procedure or for a SQL fragment.
		abstract public string PrefixParameterName(string rawName, DbCommand cmd = null);
		// Will always be from a DbParameter, but needs to know whether it was used for
		// a stored procedure or for a SQL fragment.
		abstract public string DeprefixParameterName(string dbParamName, DbCommand cmd);
#endregion

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