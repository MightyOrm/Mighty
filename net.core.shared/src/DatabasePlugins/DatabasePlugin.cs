using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Mighty.DatabasePlugins
{
	abstract public class DatabasePlugin
	{
		protected const string CRLF = "\r\n";
		// This is a weird column name, but it is a column name, it gets escaped in the DBs which need it.
		internal const string ROWCOL = "ROW_NUMBER()";
		
		// the instance which we are pluged in to
		public MightyORM mighty { get; internal set; }

#region Provider support
		// Returns the provider factory class name for the known provider(s) for this DB;
		// should simply return null if the plugin does not know that it can support the
		// named provider.
		//
		// There is no C# syntax to enforce sub-classes of DatabasePlugin to provide a static method with this name,
		// but they must do so (failure to do so results in a runtime exception).
		//
		// If you wan't to create a new plugin for unknown provider for a known database, subclass the existing plugin
		// for that database and provide your own implementation of just this method. Then either call
		// <see cref="DatabasePluginManager.RegisterPlugin"/> to register the plugin for use with extended connection
		// strings, or pass it to the MightyORM constructor using your own sub-class of <see cref="ConnectionProvider"/>.
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
		virtual public string BuildSelect(string columns, string tableName, string where, string orderBy = null)
		{
			return string.Format("SELECT {0} FROM {1}{2}{3};",
				columns, tableName, mighty.Thingify("WHERE", where), mighty.Thingify("ORDER BY", orderBy));
		}

		// is the same for every (currently supported?) database
		virtual public string BuildDelete(string tableName, string where)
		{
			return string.Format("DELETE FROM {0}{1};",
				tableName, mighty.Thingify("WHERE", where));
		}

		// is the same for every (currently supported?) database
		virtual public string BuildInsert(string tableName, string columns, string values)
		{
			return string.Format("INSERT {0} ({1}) VALUES {2};",
				tableName, columns, values);
		}

		// is the same for every (currently supported?) database
		virtual public string BuildUpdate(string tableName, string values, string where)
		{
			return string.Format("UPDATE {0} SET {1}{2};",
				tableName, values, mighty.Thingify("WHERE", where));
		}

		// Build a single query which returns two result sets: a scalar of the total count followed by
		// a normal result set of the page of items.
		// This really does vary per DB and can't be a standard virtual method which most things share.
		abstract public string BuildPagingQuery(string columns, string tablesAndJoins, string orderBy, string where,
			int limit, int offset);
#endregion

#region Table info
		// Owner is for owner/schema, will be null if none was specified by the user.
		// This is exactly the same on MySQL, PostgreSQL and SQL Server, override on the others.
		virtual public string BuildTableInfoQuery(string owner, string tableName)
		{
			return string.Format("SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = {0}{1}",
				tableName,
				owner == null ? "": string.Format(" AND TABLE_SCHEMA = {1}", owner));
		}

		// If the table info comes in the semi-standard INFORMATION_SCHEMA format (which it does, though from a
		// differently name table, on Oracle as well as on the above three) then we don't need to override this.
		virtual public IEnumerable<dynamic> NormaliseTableInfo(IEnumerable<dynamic> results) { return results; }
#endregion

#region Prefix/deprefix parameters
		// Needs to know whether this is for use in DbParameter name (cmd=null) or for escaping within the SQL fragment itself,
		// and if it is for a DbParameter whether it is used for a stored procedure or for a SQL fragment.
		abstract public string PrefixParameterName(string rawName, DbCommand cmd = null);

		// Will always be from a DbParameter, but needs to know whether it was used for
		// a stored procedure or for a SQL fragment.
		virtual public string DeprefixParameterName(string dbParamName, DbCommand cmd) { return dbParamName; }
#endregion

#region DbParameter
		// Set Value (and implicitly DbType) for single parameter, adding support for provider unsupported types, etc.
		virtual public void SetValue(DbParameter p, object value)
		{
			p.Value = value;
			var valueAsString = value as string;
			if(valueAsString != null)
			{
				p.Size = valueAsString.Length > 4000 ? -1 : 4000;
			}
		}

		// Get the output Value from single parameter, adding support for provider unsupported types, etc.
		virtual public object GetValue(DbParameter p) { return p.Value; }

		// Set ParameterDirection for single parameter, correcting for unexpected handling in specific ADO.NET providers.
		virtual public void SetDirection(DbParameter p, ParameterDirection direction) { p.Direction = direction; }
		
		// Set the parameter to DB specific cursor type.
		// Return false if not supported on this provider.
		virtual public bool SetCursor(DbParameter p, object value) { return false; }

		// Return true iff this parameter is of DB specific cursor type.
		virtual public bool IsCursor(DbParameter p) { return false; }

		// Set anonymous DbParameter.
		// Return false if not supported on this provider.
		virtual public bool SetAnonymousParameter(DbParameter p) { return false; }

		// Return true iff this ADO.NET provider ignores output parameter types when generating output data types.
		// (To avoid forcing the user to have to provide these types if they would not have had to do so when programming
		// against this provider directly.)
		virtual public bool IgnoresOutputTypes(DbParameter p) { return false; }
#endregion

#region Npgsql cursor dereferencing
		virtual public DbDataReader ExecuteDereferencingReader(DbCommand cmd, DbConnection conn)
		{
			return cmd.ExecuteReader();
		}

		virtual public bool RequiresWrappingTransaction(DbCommand cmd)
		{
			return false;
		}
#endregion
	}
}