using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Mighty.DatabasePlugins
{
	// Abstract class for database plugins; we're trying to put as much shared code as possible in here, while
	// maintaining reasonable readability.
	abstract public class DatabasePlugin
	{
		protected const string CRLF = "\r\n";
		
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
		// If you wan't to create a new plugin for an unknown provider for a known database, subclass the existing plugin
		// for that database and provide your own implementation of just this method. Then either call
		// <see cref="DatabasePluginManager.RegisterPlugin"/> to register the plugin for use with extended connection
		// strings, or pass it to the MightyORM constructor using your own sub-class of <see cref="ConnectionProvider"/>.
		//
		static public string GetProviderFactoryClassName(string providerName)
		{
			// NB because of the way static methods work in C#, this method can never be found and called from
			// a sub-class.
			throw new InvalidOperationException(string.Format("{0} should only ever be called on sub-classes of {1}",
				nameof(GetProviderFactoryClassName), typeof(DatabasePlugin)));
		}
#endregion

#region SQL
		// is the same for every (currently supported?) database
		virtual public string BuildSelect(string columns, string tableName, string where, string orderBy = null)
		{
			return string.Format("SELECT {0} FROM {1}{2}{3};",
				columns, tableName, where.Thingify("WHERE"), orderBy.Thingify("ORDERBY"));
		}

		// is the same for every (currently supported?) database
		virtual public string BuildDelete(string tableName, string where)
		{
			return string.Format("DELETE FROM {0}{1};",
				tableName, where.Thingify("WHERE"));
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
				tableName, values, where.Thingify("WHERE"));
		}

		// Build a single query which returns two result sets: a scalar of the total count followed by
		// a normal result set of the page of items.
		// Default to the LIMIT OFFSET pattern, which works exactly the same in all DBs which support it.
		virtual public string BuildPagingQuery(string columns, string tablesAndJoins, string orderBy, string where,
			int limit, int offset)
		{
			string tj = tablesAndJoins.Unthingify("FROM");
			string CountQuery = BuildSelect("COUNT(*) AS TotalCount", tj, where);
			string PagingQuery =
				string.Format("SELECT {0} FROM {1}{2} ORDER BY {3} LIMIT {4}{5}",
					columns.Unthingify("SELECT"),
					tj,
					where == null ? "" : string.Format(" WHERE {0}" + CRLF, where.Unthingify("WHERE")),
					orderBy.Unthingify("ORDERBY"),
					limit,
					offset > 0 ? string.Format(" OFFSET {0}", offset) : ""
				);
			return CountQuery + CRLF + PagingQuery;
		}

		// Utility method to provide the ROW_NUMBER() paging pattern; contrary to popular belief, *exactly* the same
		// pattern can be used on Oracle and SQL Server.
		protected string BuildRowNumberPagingQuery(string columns, string tablesAndJoins, string orderBy, string where,
			int limit, int offset)
		{
			string tj = tablesAndJoins.Unthingify("FROM");
			string CountQuery = BuildSelect("COUNT(*) AS TotalCount", tj, where);
			// 't_' outer query alias does not conflict with any use of 't_' table/query alias in user SELECT.
			string PagingQuery =
				string.Format("SELECT t_.*" + CRLF +
							  "FROM" + CRLF +
							  "(" + CRLF +
							  "    SELECT ROW_NUMBER() OVER (ORDER BY {3}) RowNum, {0}" + CRLF +
							  "    FROM {1}" + CRLF +
							  "{2}" +
							  ") t_" + CRLF +
							  "WHERE {5}RowNum < {4}" + CRLF +
							  "ORDER BY RowNum;",
					columns.Unthingify("SELECT"),
					tj,
					where == null ? "" : string.Format("    WHERE {0}" + CRLF, where.Unthingify("WHERE")),
					orderBy.Unthingify("ORDERBY"),
					limit + 1,
					offset > 0 ? string.Format("RowNum > {0} AND ", offset) : ""
				);
			return CountQuery + CRLF + PagingQuery;
		}
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
		virtual public IEnumerable<dynamic> NormalizeTableInfo(IEnumerable<dynamic> results) { return results; }

		// TO DO: accessibility?
		abstract public object GetColumnDefault(dynamic columnInfo);
#endregion

#region Keys and sequences
		virtual public bool IsSequenceBased { get; protected set; } = false;
		virtual public string KeyRetrievalFunction { get; protected set; }
#endregion

#region DbCommand
		virtual public void SetProviderSpecificCommandProperties(DbCommand command) { }
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
		virtual public DbDataReader ExecuteDereferencingReader(DbCommand cmd, CommandBehavior behavior, DbConnection conn)
		{
			return cmd.ExecuteReader(behavior);
		}

		virtual public bool RequiresWrappingTransaction(DbCommand cmd)
		{
			return false;
		}
#endregion
	}
}