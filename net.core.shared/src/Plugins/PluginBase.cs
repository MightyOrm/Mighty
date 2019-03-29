using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Linq;

namespace Mighty.Plugins
{
	/// <summary>
	/// Abstract class for database plugins; we're trying to put as much shared code as possible in here, while
	/// maintaining reasonable readability.
	/// </summary>
	abstract internal class PluginBase
	{
		protected const string CRLF = "\r\n";

		/// <summary>
		/// The instance which we are plugged in to (as dynamic to avoid having to dynamically type everything about the database plugin classes)
		/// </summary>
		public dynamic Mighty { get; set; }

		#region Provider support
		/// <summary>
		/// Returns the provider factory class name for the known provider(s) for this DB;
		/// should simply return null if the plugin does not know that it can support the
		/// named provider.
		///
		/// There is no C# syntax to enforce sub-classes of DatabasePlugin to provide a static method with this name,
		/// but they must do so (failure to do so results in a runtime exception).
		///
		/// If you wan't to create a plugin for an unknown provider for a known database, subclass the existing plugin
		/// for that database and provide your own implementation of just this method. Then either call
		/// <see cref="DatabasePluginManager.RegisterPlugin"/> to register the plugin for use with extended connection
		/// strings, or pass it to the MightyOrm constructor using your own sub-class of <see cref="ConnectionProvider"/>.
		/// </summary>
		/// <param name="loweredProviderName"></param>
		/// <returns></returns>
		static public string GetProviderFactoryClassName(string loweredProviderName)
		{
			// NB because of the way static methods work in C#, this method can never be found and called from
			// a sub-class.
			throw new InvalidOperationException(string.Format("{0} should only ever be called on sub-classes of {1}",
				nameof(GetProviderFactoryClassName), typeof(PluginBase)));
		}
		#endregion

		#region SQL
		/// <summary>
		/// SELECT pattern, using either LIMIT or TOP
		/// </summary>
		/// <param name="columns"></param>
		/// <param name="tableName"></param>
		/// <param name="where"></param>
		/// <param name="orderBy"></param>
		/// <param name="limit"></param>
		/// <returns></returns>
		/// <remarks>
		/// It makes sense to handle this separately from paging, because the semantics of LIMIT/TOP are simpler than
		/// the semantics of LIMIT OFFSET/ROW_NUMBER() queries, in particular a pure LIMIT/TOP query doesn't require
		/// an explicit ORDER BY.
		/// </remarks>
		abstract public string BuildSelect(string columns, string tableName, string where, string orderBy = null, int limit = 0);

		/// <summary>
		/// TOP SELECT pattern
		/// </summary>
		/// <param name="columns"></param>
		/// <param name="tableName"></param>
		/// <param name="where"></param>
		/// <param name="orderBy"></param>
		/// <param name="limit"></param>
		/// <returns></returns>
		protected string BuildTopSelect(string columns, string tableName, string where, string orderBy = null, int limit = 0)
		{
			return string.Format("SELECT{4} {0} FROM {1}{2}{3}",
				columns, tableName, where.Thingify("WHERE"), orderBy.Thingify("ORDER BY"), limit == 0 ? "" : limit.ToString().Thingify("TOP"));
		}

		/// <summary>
		/// LIMIT SELECT pattern
		/// </summary>
		/// <param name="columns"></param>
		/// <param name="tableName"></param>
		/// <param name="where"></param>
		/// <param name="orderBy"></param>
		/// <param name="limit"></param>
		/// <returns></returns>
		protected string BuildLimitSelect(string columns, string tableName, string where, string orderBy = null, int limit = 0)
		{
			return string.Format("SELECT {0} FROM {1}{2}{3}{4}",
				columns, tableName, where.Thingify("WHERE"), orderBy.Thingify("ORDER BY"), limit == 0 ? "" : limit.ToString().Thingify("LIMIT"));
		}

		/// <summary>
		/// Is the same for every (currently supported?) database
		/// </summary>
		/// <param name="tableName"></param>
		/// <param name="where"></param>
		/// <returns></returns>
		virtual public string BuildDelete(string tableName, string where)
		{
			return string.Format("DELETE FROM {0}{1}",
				tableName, where.Compulsify("WHERE", "DELETE"));
		}

		/// <summary>
		/// Is the same for every (currently supported?) database
		/// </summary>
		/// <param name="tableName"></param>
		/// <param name="columns"></param>
		/// <param name="values"></param>
		/// <returns></returns>
		virtual public string BuildInsert(string tableName, string columns, string values)
		{
			return string.Format("INSERT INTO {0} ({1}) VALUES ({2})",
				tableName, columns, values);
		}

		/// <summary>
		/// Is the same for every (currently supported?) database
		/// </summary>
		/// <param name="tableName"></param>
		/// <param name="values"></param>
		/// <param name="where"></param>
		/// <returns></returns>
		virtual public string BuildUpdate(string tableName, string values, string where)
		{
			return string.Format("UPDATE {0} SET {1}{2}",
				tableName, values, where.Compulsify("WHERE", "UPDATE"));
		}

		/// <summary>
		/// Build a single query which returns two result sets: a scalar of the total count followed by
		/// a normal result set of the page of items.
		/// Default to the LIMIT OFFSET pattern, which works exactly the same in all DBs which support it.
		/// </summary>
		/// <param name="columns"></param>
		/// <param name="tablesAndJoins"></param>
		/// <param name="where"></param>
		/// <param name="orderBy">Order by is required</param>
		/// <param name="limit"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		/// <remarks>
		/// Has to be done as two round-trips to the DB for one main reason:
		/// 1) The items are done using the standard yield return delayed execution, so we don't want to
		///	   start the reader until the results are needed, but we do want the count straight away.
		/// Less importantly
		/// 2) It is difficult (though possible now, using Oracle's automatic dereferencing and the cursor
		///	   support in the microORM) to get at the results of multiple selects from one DB call
		///	   on Oracle.
		/// </remarks>
		abstract public dynamic BuildPagingQueryPair(string columns, string tablesAndJoins, string where, string orderBy,
			int limit, int offset);

		/// <summary>
		/// Utility method to provide LIMIT-OFFSET paging pattern.
		/// </summary>
		/// <param name="columns"></param>
		/// <param name="tablesAndJoins"></param>
		/// <param name="where"></param>
		/// <param name="orderBy"></param>
		/// <param name="limit"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		protected dynamic BuildLimitOffsetPagingQueryPair(string columns, string tablesAndJoins, string where, string orderBy,
			int limit, int offset)
		{
			dynamic result = new ExpandoObject();
			tablesAndJoins = tablesAndJoins.Unthingify("FROM");
			result.CountQuery = BuildSelect("COUNT(*) AS TotalCount", tablesAndJoins, where);
			result.PagingQuery = string.Format("SELECT {0} FROM {1}{2} {3} LIMIT {4}{5}",
				columns.Unthingify("SELECT"),
				tablesAndJoins,
				where == null ? "" : string.Format(" {0}", where.Thingify("WHERE")),
				orderBy.Compulsify("ORDER BY", "paged select"),
				limit,
				offset > 0 ? string.Format(" OFFSET {0}", offset) : ""
			);
			return result;
		}

		/// <summary>
		/// Utility method to provide the ROW_NUMBER() paging pattern; contrary to popular belief, *exactly* the same
		/// pattern can be used on Oracle and SQL Server.
		/// </summary>
		/// <param name="columns"></param>
		/// <param name="tablesAndJoins"></param>
		/// <param name="where"></param>
		/// <param name="orderBy">Order by is required</param>
		/// <param name="limit"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		/// <remarks>Unavoidably (without significant SQL parsing, which we do not do) adds column RowNumber to the results, which does not happen on LIMIT/OFFSET DBs</remarks>
		protected dynamic BuildRowNumberPagingQueryPair(string columns, string tablesAndJoins, string where, string orderBy, int limit, int offset)
		{
			dynamic result = new ExpandoObject();
			tablesAndJoins = tablesAndJoins.Unthingify("FROM");
			result.CountQuery = BuildSelect("COUNT(*) AS TotalCount", tablesAndJoins, where);
			// we have to use t_.* in the outer select as columns may refer to table names or aliases which are only in scope in the inner select
			result.PagingQuery = string.Format("SELECT t_.*" + CRLF +
								 "FROM" + CRLF +
								 "(" + CRLF +
								 "    SELECT ROW_NUMBER() OVER ({3}) RowNumber, {0}" + CRLF +
								 "    FROM {1}" + CRLF +
								 "{2}" +
								 ") t_" + CRLF +
								 "WHERE {5}RowNumber < {4}" + CRLF +
								 "ORDER BY RowNumber",
				FixStarColumns(tablesAndJoins, columns),
				tablesAndJoins,
				where == null ? "" : string.Format("    {0}" + CRLF, where.Thingify("WHERE")),
				orderBy.Compulsify("ORDER BY", "paged select"),
				offset + limit + 1,
				offset > 0 ? string.Format("RowNumber > {0} AND ", offset) : ""
			);
			return result;
		}

		/// <summary>
		/// Adds table name qualifer to * column spec so that basic paging query can work on Oracle (and does no harm on SQL Server).
		/// Throws exception for * colmns spec combined with join query (which will not work even on SQL Server).
		/// </summary>
		/// <remarks>
		/// Adding the qualifier to a bare table name is not required, but works, on SQL Server
		/// Having * on a join query won't work on SQL server, even though the syntax is valid, because it complains about multiple appearance of the join column.
		/// Therefore this call makes sense on both DBs.
		/// </remarks>
		protected string FixStarColumns(string tableName, string columns)
		{
			// This is checking for multiple table names by checking for spaces, so it will complain when it shouldn't in some corner cases (e.g. a quoted table
			// name with a space in), but can be worked round even then.
			// TO DO: How?
			if (columns == "*")
			{
				if (tableName.Any(Char.IsWhiteSpace))
				{
					throw new InvalidOperationException("To query from joined tables you must specify the columns explicitly (not *)");
				}
				columns = string.Format("{0}.{1}", tableName, columns);
			}
			return columns;
		}

		/// <summary>
		/// Required for Oracle only
		/// </summary>
		/// <param name="command"></param>
		virtual public void FixupInsertCommand(DbCommand command) { }
		#endregion

		#region Table info
		/// <summary>
		/// This is exactly the same on MySQL, PostgreSQL and SQL Server, override on the others.
		/// </summary>
		/// <param name="tableName">Table name</param>
		/// <param name="tableOwner">Table owner/schema, will be null if none was specified by the user</param>
		/// <returns></returns>
		virtual public string BuildTableMetaDataQuery(string tableName, string tableOwner)
		{
			return string.Format("SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = {0}{1}",
				PrefixParameterName("0"),
				tableOwner != null ? string.Format(" AND TABLE_SCHEMA = {0}", PrefixParameterName("1")) : "");
		}

		/// <summary>
		/// If the table info comes in the semi-standard INFORMATION_SCHEMA format (which it does, though from a
		/// differently named table, on Oracle as well as on the above three) then we don't need to override this;
		/// however, this DOES need ToList, as it is converting from delayed execution to something ready to use.
		/// </summary>
		/// <param name="results"></param>
		/// <returns></returns>
		virtual public IEnumerable<dynamic> PostProcessTableMetaData(IEnumerable<dynamic> results)
		{
			return results.ToList();
		}

		/// <summary>
		/// Get default value for a column - was done as a plugin method, but now the same for everything.
		/// </summary>
		/// <param name="columnInfo"></param>
		/// <returns></returns>
		/// <remarks>
		/// Not DB-specific, and not trivial... should move into <see cref="MightyOrm"/>.
		/// </remarks>
		virtual public object GetColumnDefault(dynamic columnInfo)
		{
			string defaultValue = columnInfo.COLUMN_DEFAULT;
			if (defaultValue == null)
			{
				return null;
			}
			object result = null;
			if (defaultValue.StartsWith("(") && defaultValue.EndsWith(")"))
			{
				defaultValue = defaultValue.Substring(1, defaultValue.Length - 2);
			}
			switch (defaultValue.ToUpperInvariant())
			{
				case "CURRENT_DATE":
					result = DateTime.Now.Date;
					break;

				case "CURRENT_TIME":
					result = DateTime.Now.TimeOfDay;
					break;

				case "CURRENT_TIMESTAMP":
					result = DateTime.Now;
					break;

				case "SYSDATE":
					result = DateTime.Now;
					break;

				case "GETDATE()":
					result = DateTime.Now;
					break;

				case "NEWID()":
					result = Guid.NewGuid().ToString();
					break;
			}
			if (result == null)
			{
				if (defaultValue.StartsWith("(") && defaultValue.EndsWith(")"))
				{
					defaultValue = defaultValue.Substring(1, defaultValue.Length - 2);
				}
				if (!((ExpandoObject)columnInfo).ToDictionary().ContainsKey("NUMERIC_SCALE"))
				{
					string DATA_TYPE = columnInfo.DATA_TYPE;
					if (DATA_TYPE.Contains(",")) result = float.Parse(defaultValue);
					else if (DATA_TYPE.Contains("(")) result = defaultValue;
					else result = int.Parse(defaultValue);
				}
				else if (columnInfo.NUMERIC_SCALE == null)
				{
					if (columnInfo.DATA_TYPE == "bit") result = (defaultValue == "1");
					else result = defaultValue;
				}
				else if (columnInfo.NUMERIC_SCALE == 0) result = int.Parse(defaultValue);
				else result = float.Parse(defaultValue);
			}
#if DEBUG
			if (result == null) throw new Exception(string.Format("Unknown defaultValue={0}", defaultValue)); ////
#endif
			return result;
		}
		#endregion

		#region Keys and sequences
		virtual public bool IsSequenceBased { get; protected set; } = false;
		virtual public string BuildNextval(string sequence) => throw new NotImplementedException();
		virtual public string BuildCurrvalSelect(string sequence) => throw new NotImplementedException();
		virtual public string IdentityRetrievalFunction { get; protected set; }
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
			if (valueAsString != null)
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