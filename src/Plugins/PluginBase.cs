using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Linq;

namespace Mighty.Plugins
{
    /// <summary>
    /// Implement this abstract class in order to add support for a new database type to Mighty.
    /// </summary>
    /// <remarks>
    /// We're trying to put as much shared code as possible in here, while
    /// maintaining reasonable readability.
    /// </remarks>
    abstract public partial class PluginBase
    {
        /// <summary>
        /// CRLF for use in generating SQL (all SQL is generated in Windows CRLF format on all platforms, currently)
        /// </summary>
        protected const string CRLF = "\r\n";

        /// <summary>
        /// The instance which we are plugged in to (as dynamic to avoid having to dynamically type everything about the database plugin classes)
        /// </summary>
        public dynamic Mighty { get; set; }

        #region Provider support
#pragma warning disable IDE0060 // Remove unused parameter
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
        /// <see cref="PluginManager.RegisterPlugin"/> to register the plugin for use with extended connection
        /// strings, or pass it to the MightyOrm constructor using your own sub-class of <see cref="ConnectionProviders.ConnectionProvider"/>.
        /// </summary>
        /// <param name="loweredProviderName">The provider name, converted to lower case</param>
        /// <returns></returns>
        static public string GetProviderFactoryClassName(string loweredProviderName)
        {
            // NB because of the way static methods work in C#, this method can never be found and called from
            // a sub-class.
            throw new InvalidOperationException(string.Format("{0} should only ever be called on sub-classes of {1}",
                nameof(GetProviderFactoryClassName), typeof(PluginBase)));
        }
#pragma warning restore IDE0060
        #endregion

        #region SQL
        /// <summary>
        /// SELECT pattern, using either LIMIT or TOP
        /// </summary>
        /// <param name="columns">Comma separated list of columns to return or "*"</param>
        /// <param name="tableName">The table name</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="orderBy">ORDER BY clause</param>
        /// <param name="limit">The maximum number of rows to return</param>
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
        /// <param name="columns">Comma separated list of columns to return or "*"</param>
        /// <param name="tableName">The table name</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="orderBy">ORDER BY clause</param>
        /// <param name="limit">The maximum number of rows to return</param>
        /// <returns></returns>
        protected string BuildTopSelect(string columns, string tableName, string where, string orderBy = null, int limit = 0)
        {
            return string.Format("SELECT{4} {0} FROM {1}{2}{3}",
                columns, tableName, where.Thingify("WHERE"), orderBy.Thingify("ORDER BY"), limit == 0 ? "" : limit.ToString().Thingify("TOP"));
        }

        /// <summary>
        /// LIMIT SELECT pattern
        /// </summary>
        /// <param name="columns">Comma separated list of columns to return or "*"</param>
        /// <param name="tableName">The table name</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="orderBy">ORDER BY clause</param>
        /// <param name="limit">The maximum number of rows to return</param>
        /// <returns></returns>
        protected string BuildLimitSelect(string columns, string tableName, string where, string orderBy = null, int limit = 0)
        {
            return string.Format("SELECT {0} FROM {1}{2}{3}{4}",
                columns, tableName, where.Thingify("WHERE"), orderBy.Thingify("ORDER BY"), limit == 0 ? "" : limit.ToString().Thingify("LIMIT"));
        }

        /// <summary>
        /// Is the same for every (currently supported?) database
        /// </summary>
        /// <param name="tableName">The table name</param>
        /// <param name="where">WHERE clause</param>
        /// <returns></returns>
        virtual public string BuildDelete(string tableName, string where)
        {
            return string.Format("DELETE FROM {0}{1}",
                tableName, where.Compulsify("WHERE", "DELETE"));
        }

        /// <summary>
        /// Is the same for every (currently supported?) database
        /// </summary>
        /// <param name="tableName">The table name</param>
        /// <param name="columns">Comma separated list of columns to return or "*"</param>
        /// <param name="values">The values (as SQL parameters)</param>
        /// <returns></returns>
        virtual public string BuildInsert(string tableName, string columns, string values)
        {
            return string.Format("INSERT INTO {0} ({1}) VALUES ({2})",
                tableName, columns, values);
        }

        /// <summary>
        /// Is the same for every (currently supported?) database
        /// </summary>
        /// <param name="tableName">The table name</param>
        /// <param name="values">The values (as SQL parameters)</param>
        /// <param name="where">WHERE clause</param>
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
        /// <param name="columns">Comma separated list of columns to return or "*"</param>
        /// <param name="tableNameOrJoinSpec">A table name, or a complete join specification (i.e. anything you can SELECT FROM in SQL)</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="orderBy">Order by is required</param>
        /// <param name="limit">The maximum number of rows to return (i.e. the page size)</param>
        /// <param name="offset">The starting row offset for the page</param>
        /// <returns></returns>
        /// <remarks>
        /// Has to be done as two round-trips to the DB for one main reason:
        /// 1) The items are done using the standard yield return delayed execution, so we don't want to
        ///       start the reader until the results are needed, but we do want the count straight away.
        /// Less importantly
        /// 2) It is difficult (though possible now, using Oracle's automatic dereferencing and the cursor
        ///       support in Mighty) to get at the results of multiple selects from one DB call
        ///       on Oracle.
        /// </remarks>
        abstract public PagingQueryPair BuildPagingQueryPair(string columns, string tableNameOrJoinSpec, string orderBy, string where,
            int limit, int offset);

        /// <summary>
        /// Utility method to provide LIMIT-OFFSET paging pattern.
        /// </summary>
        /// <param name="columns">Comma separated list of columns to return or "*"</param>
        /// <param name="tableNameOrJoinSpec">A table name, or a complete join specification (i.e. anything you can SELECT FROM in SQL)</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="orderBy">ORDER BY clause</param>
        /// <param name="limit">The maximum number of rows to return (i.e. the page size)</param>
        /// <param name="offset">The starting row offset for the page</param>
        /// <returns></returns>
        protected PagingQueryPair BuildLimitOffsetPagingQueryPair(string columns, string tableNameOrJoinSpec, string orderBy, string where,
            int limit, int offset)
        {
            tableNameOrJoinSpec = tableNameOrJoinSpec.Unthingify("FROM");
            return new PagingQueryPair()
            {
                CountQuery = BuildSelect("COUNT(*) AS TotalCount", tableNameOrJoinSpec, where),
                PagingQuery = string.Format("SELECT {0} FROM {1}{2}{3} LIMIT {4}{5}",
                                    columns.Unthingify("SELECT"),
                                    tableNameOrJoinSpec,
                                    where == null ? "" : where.Thingify("WHERE"),
                                    orderBy.Compulsify("ORDER BY", "paged select"),
                                    limit,
                                    offset > 0 ? string.Format(" OFFSET {0}", offset) : "")
            };
        }

        /// <summary>
        /// Utility method to provide the ROW_NUMBER() paging pattern; contrary to popular belief, *exactly* the same
        /// pattern can be used on Oracle and SQL Server.
        /// </summary>
        /// <param name="columns">Comma separated list of columns to return or "*"</param>
        /// <param name="tableNameOrJoinSpec">A table name, or a complete join specification (i.e. anything you can SELECT FROM in SQL)</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="orderBy">Order by is required</param>
        /// <param name="limit">The maximum number of rows to return (i.e. the page size)</param>
        /// <param name="offset">The starting row offset for the page</param>
        /// <returns></returns>
        /// <remarks>Unavoidably (without significant SQL parsing, which we do not do) adds column RowNumber to the results, which does not happen on LIMIT/OFFSET DBs</remarks>
        protected PagingQueryPair BuildRowNumberPagingQueryPair(string columns, string tableNameOrJoinSpec, string orderBy, string where, int limit, int offset)
        {
            tableNameOrJoinSpec = tableNameOrJoinSpec.Unthingify("FROM");
            return new PagingQueryPair()
            {
                CountQuery = BuildSelect("COUNT(*) AS TotalCount", tableNameOrJoinSpec, where),
                // we have to use t_.* in the outer select as columns may refer to table names or aliases which are only in scope in the inner select
                PagingQuery = string.Format("SELECT t_.*" + CRLF +
                                     "FROM" + CRLF +
                                     "(" + CRLF +
                                     "    SELECT ROW_NUMBER() OVER ({3}) RowNumber, {0}" + CRLF +
                                     "    FROM {1}" + CRLF +
                                     "{2}" +
                                     ") t_" + CRLF +
                                     "WHERE {5}RowNumber < {4}" + CRLF +
                                     "ORDER BY RowNumber",
                    FixStarColumns(tableNameOrJoinSpec, columns),
                    tableNameOrJoinSpec,
                    where == null ? "" : string.Format("    {0}" + CRLF, where.Thingify("WHERE", addSpace: false)),
                    orderBy.Compulsify("ORDER BY", "paged select", addSpace: false),
                    offset + limit + 1,
                    offset > 0 ? string.Format("RowNumber > {0} AND ", offset) : ""
                )
            };
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
                    throw new InvalidOperationException("To query from joined tables you have to specify the columns explicitly not with *");
                }
                columns = string.Format("{0}.{1}", tableName, columns);
            }
            return columns;
        }

        /// <summary>
        /// Required for Oracle only
        /// </summary>
        /// <param name="command">The command to execute</param>
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
        /// <param name="results">The unprocessed table meta-data</param>
        /// <returns></returns>
        /// <remarks>
        /// TO DO: Just make the inner conversion function part of the plugin, not the loop.
        /// </remarks>
        virtual public List<dynamic> PostProcessTableMetaData(IEnumerable<dynamic> results)
        {
            return results.ToList();
        }

        /// <summary>
        /// Get default value for a column - was done as a plugin method, but now the same for everything.
        /// </summary>
        /// <param name="columnInfo">The column info (which was collected from the database the first time that table meta-data was required)</param>
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
            // In live code we just want null rather then unrunnable if we encounter a default value which we cannot handle
            if (result == null) throw new Exception(string.Format("Unknown defaultValue={0}", defaultValue)); ////
#endif
            return result;
        }
        #endregion

        #region Keys and sequences
        /// <summary>
        /// Is this sequence based? If not then identity based.
        /// </summary>
        virtual public bool IsSequenceBased { get; protected set; } = false;

        /// <summary>
        /// Build an SQL fragment which references the next value from the named sequence
        /// </summary>
        /// <param name="sequence">The sequence</param>
        /// <returns></returns>
        virtual public string BuildNextval(string sequence) => throw new NotImplementedException();

        /// <summary>
        /// Build SQL to select the current value from the named sequence
        /// </summary>
        /// <param name="sequence">The sequence</param>
        /// <returns></returns>
        virtual public string BuildCurrvalSelect(string sequence) => throw new NotImplementedException();

        /// <summary>
        /// Return the SQL fragment which retrieves the identity for the last inserted row
        /// </summary>
        virtual public string IdentityRetrievalFunction { get; protected set; }
        #endregion

        #region DbCommand
        /// <summary>
        /// Set any provider specific properties which are required to make this database perform as expected by Mighty.
        /// </summary>
        /// <param name="command">The command to execute</param>
        virtual public void SetProviderSpecificCommandProperties(DbCommand command) { }
        #endregion

        #region Prefix/deprefix parameters
        /// <summary>
        /// Prefix a database parameter name.
        /// </summary>
        /// <param name="rawName">The unprefixed parameter name</param>
        /// <param name="cmd">
        /// The database command,
        /// which is required because this method
        /// needs to know whether this is for use in DbParameter name (cmd=null) or for escaping within the SQL fragment itself,
        /// and if it is for a DbParameter whether it is used for a stored procedure or for an SQL fragment.
        /// </param>
        /// <returns></returns>
        abstract public string PrefixParameterName(string rawName, DbCommand cmd = null);

        /// <summary>
        /// Deprefix a database parameter name.
        /// </summary>
        /// <param name="dbParamName">The prefixed parameter name</param>
        /// <param name="cmd">
        /// The database command,
        /// which is required because this although method
        /// will always be from a DbParameter, it needs to know whether it was used for
        /// a stored procedure or for an SQL fragment.
        /// </param>
        /// <returns></returns>
        virtual public string DeprefixParameterName(string dbParamName, DbCommand cmd) { return dbParamName; }
        #endregion

        #region DbParameter
        /// <summary>
        /// Set the <see cref="DbParameter.Value"/> (and implicitly <see cref="DbParameter.DbType"/>) for single parameter, adding support for provider unsupported types, etc.
        /// </summary>
        /// <param name="p">The <see cref="DbParameter"/></param>
        /// <param name="value">The value to set</param>
        virtual public void SetValue(DbParameter p, object value)
        {
            p.Value = value;
            var valueAsString = value as string;
            if (valueAsString != null)
            {
                p.Size = valueAsString.Length > 4000 ? -1 : 4000;
            }
        }

        /// <summary>
        /// Get the output Value from single parameter, adding support for provider unsupported types, etc.
        /// </summary>
        /// <param name="p">The parameter</param>
        /// <returns></returns>
        virtual public object GetValue(DbParameter p) { return p.Value; }

        /// <summary>
        /// Set the ParameterDirection for single parameter, correcting for unexpected handling in specific ADO.NET providers.
        /// </summary>
        /// <param name="p">The parameter</param>
        /// <param name="direction">The required parameter direction</param>
        virtual public void SetDirection(DbParameter p, ParameterDirection direction) { p.Direction = direction; }

        /// <summary>
        /// Set the parameter to DB specific cursor type.
        /// Return false if not supported on this provider.
        /// </summary>
        /// <param name="p">The parameter</param>
        /// <param name="value">The value (a db-specific cursor reference; typically a string containing a cursor id of some sort)</param>
        /// <returns></returns>
        virtual public bool SetCursor(DbParameter p, object value) { return false; }

        /// <summary>
        /// Return true iff this parameter is of the DB specific cursor type.
        /// </summary>
        /// <param name="p">The parameter</param>
        /// <returns></returns>
        virtual public bool IsCursor(DbParameter p) { return false; }

        /// <summary>
        /// Set an anonymous <see cref="DbParameter"/>.
        /// Return false if not supported on this provider.
        /// </summary>
        /// <param name="p">The parameter</param>
        /// <returns></returns>
        virtual public bool SetAnonymousParameter(DbParameter p) { return false; }

        /// <summary>
        /// Return true iff this ADO.NET provider ignores output parameter types when generating output data types, for a given parameter.
        /// (To avoid forcing the user to have to provide these types if they would not have had to do so when programming
        /// against this provider directly.)
        /// </summary>
        /// <param name="p">The parameter</param>
        /// <returns></returns>
        virtual public bool IgnoresOutputTypes(DbParameter p) { return false; }
        #endregion

        #region Npgsql cursor dereferencing
        /// <summary>
        /// For non-Npgsql, this just does <see cref="DbCommand.ExecuteReader(CommandBehavior)"/>.
        /// For Npgql this (optionally, depending on the value of<see cref="MightyOrm{T}.NpgsqlAutoDereferenceCursors"/>) returns a new <see cref="DbDataReader"/> which de-references
        /// all cursors returned by the original reader, iteratively returning those results instead.
        /// </summary>
        /// <param name="cmd">The original command</param>
        /// <param name="behavior">The command behaviour</param>
        /// <param name="conn">The connection to use</param>
        /// <returns></returns>
        virtual public DbDataReader ExecuteDereferencingReader(DbCommand cmd, CommandBehavior behavior, DbConnection conn)
        {
            return cmd.ExecuteReader(behavior);
        }

#if (NETCOREAPP || NETSTANDARD)
        /// <summary>
        /// Does this command require a wrapping transaction? This is required for some cursor-specific commands on some databases.
        /// If required Mighty will only create a new transaction if a user transaction is not already in place.
        /// </summary>
        /// <param name="cmd">The command to check</param>
        /// <returns></returns>
#else
        /// <summary>
        /// Does this command require a wrapping transaction? This is required for some cursor-specific commands on some databases.
        /// If required Mighty will only create a new transaction if a user transaction or <see cref="System.Transactions.TransactionScope"/> is not already in place.
        /// </summary>
        /// <param name="cmd">The command to check</param>
        /// <returns></returns>
#endif
        virtual public bool RequiresWrappingTransaction(DbCommand cmd)
        {
            return false;
        }
        #endregion
    }
}