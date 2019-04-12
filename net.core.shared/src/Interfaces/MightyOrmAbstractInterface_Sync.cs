using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

using Mighty.Plugins;
using Mighty.Mapping;
using Mighty.Profiling;
using Mighty.Validation;

// <summary>
// TO DO: Not sure about putting this in a separate namespace, but maybe best to hide the mockable version?
// </summary>
namespace Mighty.Interfaces
{
	abstract public partial class MightyOrmAbstractInterface<T>
	{
        // 'Interface' for the general purpose data access wrapper methods (i.e. the ones which can be used
        // even if no table has been specified).
        // All versions which simply redirect to other versions are defined here, not in the main class.
        #region Non-table specific methods
        /// <summary>
        /// Creates a new DbConnection. You do not normally need to call this! (MightyOrm normally manages its own
        /// connections. Create a connection here and pass it on to other MightyOrm commands only in non-standard use
        /// cases where you need to explicitly manage transactions or share connections, e.g. when using explicit cursors.)
        /// </summary>
        /// <returns></returns>
        abstract public DbConnection OpenConnection();

		abstract public IEnumerable<T> Query(DbCommand command,
			DbConnection connection = null);

        /// <summary>
        /// Get single item returned by database command.
        /// (To ensure consistent behaviour use <see cref="CreateCommand(string, object[])"/> or
        /// <see cref="CreateCommand(string, DbConnection, object[])"/> to create commands passed in to Mighty.)
        /// </summary>
        /// <param name="command">Database command</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        abstract public T Single(DbCommand command,
			DbConnection connection = null);

        // no connection, easy args

        /// <summary>
        /// Get <see cref="IEnumerable{T}"/> of items returned by SQL query
        /// </summary>
        /// <param name="sql">SQL</param>
		/// <param name="args">Auto-numbered parameter values for SQL</param>
        /// <returns></returns>
        abstract public IEnumerable<T> Query(string sql,
			params object[] args);

        /// <summary>
        /// Get single item returned by SQL query
        /// </summary>
        /// <param name="sql">SQL</param>
		/// <param name="args">Auto-numbered parameter values for SQL</param>
        /// <returns></returns>
		abstract public T SingleFromQuery(string sql,
			params object[] args);

        /// <summary>
        /// Get <see cref="IEnumerable{T}"/> of items returned by SQL query
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="connection">The connection to use</param>
		/// <param name="args">Auto-numbered parameter values for SQL</param>
        /// <returns></returns>
		abstract public IEnumerable<T> Query(string sql,
			DbConnection connection,
			params object[] args);

        /// <summary>
        /// Get single item returned by SQL query
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="connection">The connection to use</param>
		/// <param name="args">Auto-numbered parameter values for SQL</param>
        /// <returns></returns>
		abstract public T SingleFromQuery(string sql,
			DbConnection connection,
			params object[] args);

		abstract public IEnumerable<T> QueryWithParams(string sql,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args);

		abstract public T SingleFromQueryWithParams(string sql,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args);

		abstract public IEnumerable<T> QueryFromProcedure(string spName,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args);

		abstract public T SingleFromProcedure(string spName,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args);

		abstract public IEnumerable<IEnumerable<T>> QueryMultiple(DbCommand command,
			DbConnection connection = null);

		// no connection, easy args
		abstract public IEnumerable<IEnumerable<T>> QueryMultiple(string sql,
			params object[] args);

		abstract public IEnumerable<IEnumerable<T>> QueryMultiple(string sql,
			DbConnection connection,
			params object[] args);

		abstract public IEnumerable<IEnumerable<T>> QueryMultipleWithParams(string sql,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args);

		abstract public IEnumerable<IEnumerable<T>> QueryMultipleFromProcedure(string spName,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args);

		abstract public int Execute(DbCommand command,
			DbConnection connection = null);

		// no connection, easy args
		abstract public int Execute(string sql,
			params object[] args);

		abstract public int Execute(string sql,
			DbConnection connection,
			params object[] args);

		/// <summary>
		/// Execute command with parameters
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="inParams"></param>
		/// <param name="outParams"></param>
		/// <param name="ioParams"></param>
		/// <param name="returnParams"></param>
		/// <param name="connection">Optional connection to use</param>
		/// <param name="args">Auto-numbered parameter values for WHERE clause</param>
		/// <returns>The results of all non-input parameters</returns>
		abstract public dynamic ExecuteWithParams(string sql,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args);

		/// <summary>
		/// Execute stored procedure with parameters
		/// </summary>
		/// <param name="spName"></param>
		/// <param name="inParams"></param>
		/// <param name="outParams"></param>
		/// <param name="ioParams"></param>
		/// <param name="returnParams"></param>
		/// <param name="connection">Optional connection to use</param>
		/// <param name="args">Auto-numbered parameter values for WHERE clause</param>
		/// <returns>The results of all non-input parameters</returns>
		abstract public dynamic ExecuteProcedure(string spName,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args);

		abstract public object Scalar(DbCommand command,
			DbConnection connection = null);

		// no connection, easy args
		abstract public object Scalar(string sql,
			params object[] args);

		abstract public object Scalar(string sql,
			DbConnection connection,
			params object[] args);

		abstract public object ScalarWithParams(string sql,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args);

		abstract public object ScalarFromProcedure(string spName,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args);

        /// <summary>
        /// Return paged results from arbitrary select statement.
        /// </summary>
        /// <param name="columns">Column spec</param>
        /// <param name="tableNameOrJoinSpec">Single table name, or join specification</param>
        /// <param name="orderBy">ORDER BY clause</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="currentPage">Current page</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <returns>The result of the paged query. Result properties are Items, TotalPages, and TotalRecords.</returns>
        /// <remarks>
        /// In this one instance, because of the connection to the underlying logic of these queries, the user
        /// can pass "SELECT columns" instead of columns.
        /// TO DO: Possibly cancel the above, it makes no sense from a UI pov!
        /// </remarks>
		abstract public PagedResults<T> PagedFromSelect(
            string tableNameOrJoinSpec,
            string orderBy,
            string columns = null,
            string where = null,
            int pageSize = 20, int currentPage = 1,
			DbConnection connection = null,
			params object[] args);

		abstract protected IEnumerable<X> QueryNWithParams<X>(string sql = null, object inParams = null, object outParams = null, object ioParams = null, object returnParams = null, bool isProcedure = false, CommandBehavior behavior = CommandBehavior.Default, DbConnection connection = null, params object[] args);

		abstract protected IEnumerable<X> QueryNWithParams<X>(DbCommand command, CommandBehavior behavior = CommandBehavior.Default, DbConnection connection = null, DbDataReader outerReader = null);
        #endregion

        #region Table specific methods
        /// <summary>
        /// Perform COUNT on current table.
        /// </summary>
        /// <param name="where">WHERE clause</param>
        /// <param name="columns">Columns (defaults to *, but can be specified, e.g., to count non-nulls in a given field)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <returns></returns>
        abstract public object Count(
            string where = null,
            string columns = "*",
			DbConnection connection = null,
			params object[] args);

        /// <summary>
        /// Perform COUNT on current table.
        /// </summary>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="columns">Columns (defaults to *, but can be specified, e.g., to count non-nulls in a given field)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        abstract public object Count(
            object whereParams = null,
            string columns = "*",
            DbConnection connection = null);

        /// <summary>
        /// Get MAX of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <returns></returns>
        abstract public object Max(
            string columns,
            string where = null,
            DbConnection connection = null,
            params object[] args);

        /// <summary>
        /// Get MAX of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        abstract public object Max(
            string columns,
            object whereParams = null,
            DbConnection connection = null);

        /// <summary>
        /// Get MIN of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <returns></returns>
        abstract public object Min(
            string columns,
            string where = null,
            DbConnection connection = null,
            params object[] args);

        /// <summary>
        /// Get MIN of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        abstract public object Min(
            string columns,
            object whereParams = null,
            DbConnection connection = null);

        /// <summary>
        /// Get SUM of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <returns></returns>
        abstract public object Sum(
            string columns,
            string where = null,
            DbConnection connection = null,
            params object[] args);

        /// <summary>
        /// Get SUM of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        abstract public object Sum(
            string columns,
            object whereParams = null,
            DbConnection connection = null);

        /// <summary>
        /// Get AVG of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <returns></returns>
        abstract public object Avg(
            string columns,
            string where = null,
            DbConnection connection = null,
            params object[] args);

        /// <summary>
        /// Get AVG of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        abstract public object Avg(
            string columns,
            object whereParams = null,
            DbConnection connection = null);

        /// <summary>
        /// Perform aggregate operation on the current table (use for SUM, MAX, MIN, AVG, etc.)
        /// </summary>
        /// <param name="function">Aggregate function</param>
        /// <param name="columns">Columns for aggregate function</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <returns></returns>
        abstract public object Aggregate(string function, string columns, string where = null,
			DbConnection connection = null,
			params object[] args);

        /// <summary>
        /// Perform aggregate operation on the current table (use for SUM, MAX, MIN, AVG, etc.)
        /// </summary>
        /// <param name="function">Aggregate function</param>
        /// <param name="columns">Columns for aggregate function</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        abstract public object Aggregate(string function, string columns, object whereParams = null,
            DbConnection connection = null);

        /// <summary>
        /// Perform aggregate operation on the current table (use for SUM, MAX, MIN, AVG, etc.), with support for named params.
        /// </summary>
        /// <param name="function">Aggregate function</param>
        /// <param name="columns">Columns for aggregate function</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="inParams">Input parameters</param>
        /// <param name="outParams">Output parameters</param>
        /// <param name="ioParams">Input-output parameters</param>
        /// <param name="returnParams">Return parameters</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <returns></returns>
        abstract public object AggregateWithParams(string function, string columns, string where = null,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args);

        /// <summary>
        /// Get single object from the current table using primary key or name-value specification.
        /// </summary>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="columns">List of columns to return</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        abstract public T Single(object whereParams, string columns = null,
            DbConnection connection = null);

        /// <summary>
        /// Get a single object from the current table with where specification.
        /// </summary>
        /// <param name="where">WHERE clause</param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <returns></returns>
        /// <remarks>
        /// 'Easy-calling' version, optional args straight after where.
        /// </remarks>
        abstract public T Single(string where,
			params object[] args);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="where"></param>
		/// <param name="connection">Optional connection to use</param>
		/// <param name="orderBy"></param>
		/// <param name="columns"></param>
		/// <param name="args">Auto-numbered parameter values for WHERE clause</param>
		/// <returns></returns>
		/// <remarks>
		/// DbConnection coming early (not just before args) in this one case is really useful, as it avoids ambiguity between
		/// the `columns` and `orderBy` strings and optional string args.
		/// </remarks>
		abstract public T Single(string where,
			DbConnection connection = null,
			string orderBy = null,
			string columns = null,
			params object[] args);

		// WithParams version just in case; allows transactions for a start
		abstract public T SingleWithParams(string where, string orderBy = null, string columns = null,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args);

        // ORM
        abstract public IEnumerable<T> All(
            string where = null, string orderBy = null, string columns = null, int limit = 0,
            params object[] args);

        abstract public IEnumerable<T> All(
            object whereParams = null, string orderBy = null, string columns = null, int limit = 0);

        abstract public IEnumerable<T> AllWithParams(
			string where = null, string orderBy = null, string columns = null, int limit = 0,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args);

		/// <summary>
		/// Table-specific paging; there is also a data wrapper version of paging <see cref="PagedFromSelect"/>.
		/// </summary>
		/// <param name="orderBy">You may provide orderBy, if you don't it will try to order by PK and will produce an exception if there is no PK defined.</param>
		/// <param name="where">WHERE clause</param>
		/// <param name="columns">Columns to return</param>
		/// <param name="pageSize">Page size</param>
		/// <param name="currentPage">Current page</param>
		/// <param name="connection">Optional connection to use</param>
		/// <param name="args">Auto-numbered parameter values for WHERE clause</param>
		/// <returns>The result of the paged query. Result properties are Items, TotalPages, and TotalRecords.</returns>
		/// <remarks>
		/// `columns` parameter is not placed first because it's an override to something we may have alread provided in the constructor,
		/// so we don't want the user to have to non-fluently re-type it, or else type null, every time.
		/// </remarks>
		abstract public PagedResults<T> Paged(
            string orderBy = null,
            string columns = null,
            string where = null,
			int pageSize = 20, int currentPage = 1,
			DbConnection connection = null,
			params object[] args);

		/// <summary>
		/// Save one or more items using params style arguments
		/// </summary>
		/// <param name="items">The items</param>
		/// <returns></returns>
		abstract public int Save(params object[] items);

		/// <summary>
		/// Save one or more items using pre-specified DbConnection
		/// </summary>
		/// <param name="connection">The connection to use</param>
		/// <param name="items">The items</param>
		/// <returns></returns>
		abstract public int Save(DbConnection connection, params object[] items);

		/// <summary>
		/// Save array or other IEnumerable of items
		/// </summary>
		/// <param name="items">The items</param>
		/// <returns></returns>
		abstract public int Save(IEnumerable<object> items);

		/// <summary>
		/// Save array or other IEnumerable of items using pre-specified DbConnection
		/// </summary>
		/// <param name="connection">The connection to use</param>
		/// <param name="items">The items</param>
		/// <returns></returns>
		abstract public int Save(DbConnection connection, IEnumerable<object> items);

		/// <summary>
		/// Insert single item, returning the item sent in but with PK populated.
		/// If you need all fields populated (i.e. you want to get back DB default values for non-PK fields), please create the item using New() before inserting it.
		/// </summary>
		/// <param name="item">The item to insert, in any reasonable format (for MightyOrm&lt;T&gt; this includes, but is not limited to, in instance of type T)</param>
		/// <returns>The inserted item</returns>
		abstract public T Insert(object item);

		/// <summary>
		/// Insert one or more items using params style arguments
		/// </summary>
		/// <param name="items">The items</param>
		/// <returns>The number of rows inserted</returns>
		abstract public int Insert(params object[] items);

		/// <summary>
		/// Insert one or more items using pre-specified DbConnection
		/// </summary>
		/// <param name="connection">The connection to use</param>
		/// <param name="items">The items</param>
		/// <returns>The number of rows inserted</returns>
		abstract public int Insert(DbConnection connection, params object[] items);

		/// <summary>
		/// Insert array or other IEnumerable of items
		/// </summary>
		/// <param name="items">The items</param>
		/// <returns>The number of rows inserted</returns>
		abstract public int Insert(IEnumerable<object> items);

		/// <summary>
		/// Insert array or other IEnumerable of items using pre-specified DbConnection
		/// </summary>
		/// <param name="connection">The connection to use</param>
		/// <param name="items">The items</param>
		/// <returns>The number of rows inserted</returns>
		abstract public int Insert(DbConnection connection, IEnumerable<object> items);

		/// <summary>
		/// Update one or more items using params style arguments
		/// </summary>
		/// <param name="items">The items</param>
		/// <returns></returns>
		abstract public int Update(params object[] items);

		/// <summary>
		/// Update one or more items using pre-specified DbConnection
		/// </summary>
		/// <param name="connection">The connection to use</param>
		/// <param name="items">The items</param>
		/// <returns></returns>
		abstract public int Update(DbConnection connection, params object[] items);

		/// <summary>
		/// Update array or other IEnumerable of items
		/// </summary>
		/// <param name="items">The items</param>
		/// <returns></returns>
		abstract public int Update(IEnumerable<object> items);

		/// <summary>
		/// Update array or other IEnumerable of items using pre-specified DbConnection
		/// </summary>
		/// <param name="connection">The connection to use</param>
		/// <param name="items">The items</param>
		/// <returns></returns>
		abstract public int Update(DbConnection connection, IEnumerable<object> items);

        /// <summary>
        /// Delete one or more items using params style arguments.
        /// Each argument may be (or contain) a value (or values) only, in which case
        /// it specifies the primary key value(s) of the item to delete, or it can be any object containing name-values pairs in which case
        /// it should contain fields with names matching the primary key(s) whose values will specify the item to delete (but it may contain
        /// other fields as well which will be ignored here).
        /// </summary>
        /// <param name="items">The items</param>
        /// <returns>The number of items affected</returns>
		abstract public int Delete(params object[] items);

        /// <summary>
        /// Delete one or more items using params style arguments.
        /// Each argument may be (or contain) a value (or values) only, in which case
        /// it specifies the primary key value(s) of the item to delete, or it can be any object containing name-values pairs in which case
        /// it should contain fields with names matching the primary key(s) whose values will specify the item to delete (but it may contain
        /// other fields as well which will be ignored here).
        /// </summary>
        /// <param name="items">The items</param>
        /// <param name="connection">The connection to use</param>
        /// <returns>The number of items affected</returns>
		abstract public int Delete(DbConnection connection, params object[] items);

        /// <summary>
        /// Delete an array or other <see cref="IEnumerable"/> of items.
        /// Each argument may be (or contain) a value (or values) only, in which case
        /// it specifies the primary key value(s) of the item to delete, or it can be any object containing name-values pairs in which case
        /// it should contain fields with names matching the primary key(s) whose values will specify the item to delete (but it may contain
        /// other fields as well which will be ignored here).
        /// </summary>
        /// <param name="items">The items</param>
        /// <returns>The number of items affected</returns>
		abstract public int Delete(IEnumerable<object> items);

        /// <summary>
        /// Delete an array or other <see cref="IEnumerable"/> of items.
        /// Each argument may be (or contain) a value (or values) only, in which case
        /// it specifies the primary key value(s) of the item to delete, or it can be any object containing name-values pairs in which case
        /// it should contain fields with names matching the primary key(s) whose values will specify the item to delete (but it may contain
        /// other fields as well which will be ignored here).
        /// </summary>
        /// <param name="items">The items</param>
        /// <param name="connection">The connection to use</param>
        /// <returns>The number of items affected</returns>
		abstract public int Delete(DbConnection connection, IEnumerable<object> items);

        /// <summary>
        /// Update the row(s) specified by the primary key(s) or WHERE values sent in using the values from the item sent in.
        /// If `primaryKeyFields` has been specified on the current Mighty instance then any primary key fields in the item are ignored.
        /// The item is not filtered to remove fields not in the table, if you need that you can call <see cref="NewFrom"/> with first parameter `partialItem` and second parameter `false` first.
        /// </summary>
        /// <param name="partialItem">Item containing values to update with</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
		abstract public int UpdateUsing(object partialItem, object whereParams);

        /// <summary>
        /// Update the row(s) specified by the primary key(s) or WHERE values sent in using the values from the item sent in.
        /// If `primaryKeyFields` has been specified on the current Mighty instance then any primary key fields in the item are ignored.
        /// The item is not filtered to remove fields not in the table, if you need that you can call <see cref="NewFrom"/> with first parameter `partialItem` and second parameter `false` first.
        /// </summary>
        /// <param name="partialItem">Item containing values to update with</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
		abstract public int UpdateUsing(object partialItem, object whereParams,
			DbConnection connection);

        /// <summary>
        /// Update all items matching WHERE clause using fields from the item sent in.
        /// If `primaryKeyFields` has been specified on the current Mighty instance then any primary key fields in the item are ignored.
        /// The item is not filtered to remove fields not in the table, if you need that you can call <see cref="NewFrom"/> with first parameter `partialItem` and second parameter `false` first.
        /// </summary>
        /// <param name="partialItem">Item containing values to update with</param>
        /// <param name="where">WHERE clause specifying which rows to update</param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
		abstract public int UpdateUsing(object partialItem, string where,
			params object[] args);

        /// <summary>
        /// Update all items matching WHERE clause using fields from the item sent in.
        /// If `primaryKeyFields` has been specified on the current Mighty instance then any primary key fields in the item are ignored.
        /// The item is not filtered to remove fields not in the table, if you need that you can call <see cref="NewFrom"/> with first parameter `partialItem` and second parameter `false` first.
        /// </summary>
        /// <param name="partialItem">Item containing values to update with</param>
        /// <param name="where">WHERE clause specifying which rows to update</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        abstract public int UpdateUsing(object partialItem, string where,
			DbConnection connection,
			params object[] args);

        /// <summary>
        /// Delete one or more items based on a WHERE clause.
        /// </summary>
        /// <param name="where">
        /// Non-optional WHERE clause.
        /// Specify "1=1" if you are sure that you want to delete all rows.
        /// </param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <returns>The number of items affected</returns>
        abstract public int Delete(string where,
			params object[] args);

        /// <summary>
        /// Delete one or more items based on a WHERE clause.
        /// </summary>
        /// <param name="where">
        /// Non-optional WHERE clause.
        /// Specify "1=1" if you are sure that you want to delete all rows.
        /// </param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <param name="connection">The connection to use</param>
        /// <returns>The number of items affected</returns>
        abstract public int Delete(string where,
			DbConnection connection,
			params object[] args);

#if KEY_VALUES
		// kv pair stuff for dropdowns - a method to convert IEnumerable<T> to kv pair
		abstract public IDictionary<string, string> KeyValues(string orderBy = "");
#endif
#endregion
	}
}
