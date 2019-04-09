using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

using Mighty.Plugins;
using Mighty.Mapping;
using Mighty.Profiling;
using Mighty.Validation;

/// <summary>
/// TO DO: Not sure about putting this in a separate namespace, but maybe best to hide the mockable version?
/// </summary>
namespace Mighty.Mocking
{
	abstract public partial class MightyOrmMockable<T>
	{
		// 'Interface' for the general purpose data access wrapper methods (i.e. the ones which can be used
		// even if no table has been specified).
		// All versions which simply redirect to other versions are defined here, not in the main class.
		#region Non-table specific methods
		abstract public DbConnection OpenConnection();

		abstract public IEnumerable<T> Query(DbCommand command,
			DbConnection connection = null);

		abstract public T Single(DbCommand command,
			DbConnection connection = null);

		// no connection, easy args
		abstract public IEnumerable<T> Query(string sql,
			params object[] args);

		abstract public T SingleFromQuery(string sql,
			params object[] args);

		abstract public IEnumerable<T> Query(string sql,
			DbConnection connection,
			params object[] args);

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
		/// <param name="connection"></param>
		/// <param name="args"></param>
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
		/// <param name="connection"></param>
		/// <param name="args"></param>
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

		abstract public PagedResults<T> PagedFromSelect(string columns, string tableNameOrJoinSpec, string orderBy, string where,
			int pageSize = 20, int currentPage = 1,
			DbConnection connection = null,
			params object[] args);

		abstract protected IEnumerable<X> QueryNWithParams<X>(string sql = null, object inParams = null, object outParams = null, object ioParams = null, object returnParams = null, bool isProcedure = false, CommandBehavior behavior = CommandBehavior.Default, DbConnection connection = null, params object[] args);

		abstract protected IEnumerable<X> QueryNWithParams<X>(DbCommand command, CommandBehavior behavior = CommandBehavior.Default, DbConnection connection = null, DbDataReader outerReader = null);
		#endregion

		#region Table specific methods
		/// <summary>
		/// Perform COUNT on current table
		/// </summary>
		/// <param name="columns">Columns (defaults to *, but can be specified, e.g., to count non-nulls in a given field)</param>
		/// <param name="where">Optional where clause</param>
		/// <param name="connection">Optional connection</param>
		/// <param name="args">Args</param>
		/// <returns></returns>
		abstract public object Count(string columns = "*", string where = null,
			DbConnection connection = null,
			params object[] args);

		/// <summary>
		/// Perform scalar operation on the current table (use for SUM, MAX, MIN, AVG, etc.)
		/// </summary>
		/// <param name="expression">Scalar expression</param>
		/// <param name="where">Optional where clause</param>
		/// <param name="connection">Optional connection</param>
		/// <param name="args">Parameters</param>
		/// <returns></returns>
		abstract public object Aggregate(string expression, string where = null,
			DbConnection connection = null,
			params object[] args);

		/// <summary>
		/// Perform scalar operation on the current table (use for SUM, MAX, MIN, AVG, etc.), with support for named params.
		/// </summary>
		/// <param name="expression">Scalar expression</param>
		/// <param name="where">Optional where clause</param>
		/// <param name="inParams">Optional input parameters</param>
		/// <param name="outParams">Optional output parameters</param>
		/// <param name="ioParams">Optional input-output parameters</param>
		/// <param name="returnParams">Optional return parameters</param>
		/// <param name="connection">Optional connection</param>
		/// <param name="args">Optional auto-named input parameters</param>
		/// <returns></returns>
		abstract public object AggregateWithParams(string expression, string where = null,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args);

		/// <summary>
		/// Get a single object from the current table by primary key value
		/// </summary>
		/// <param name="key">Single key (or any reasonable multi-value item for compound keys)</param>
		/// <param name="columns">Optional columns to retrieve</param>
		/// <param name="connection">Optional connection</param>
		/// <returns></returns>
		abstract public T Single(object key, string columns = null,
			DbConnection connection = null);

		/// <summary>
		/// Get a single object from the current table with where specification.
		/// </summary>
		/// <param name="where">Where clause</param>
		/// <param name="args">Optional auto-named params</param>
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
		/// <param name="connection"></param>
		/// <param name="orderBy"></param>
		/// <param name="columns"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		/// <remarks>
		/// DbConnection coming early (not just before args) in this one case is really useful, as it avoids ambiguity between
		/// the <see cref="columns" /> and <see cref="orderBy" /> strings and optional string args.
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

		abstract public IEnumerable<T> AllWithParams(
			string where = null, string orderBy = null, string columns = null, int limit = 0,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args);

		/// <summary>
		/// Table-specific paging; there is also a data wrapper version of paging <see cref="PagedFromSelect"/>.
		/// </summary>
		/// <param name="orderBy">You may provide orderBy, if you don't it will try to order by PK and will produce an exception if there is no PK defined.</param>
		/// <param name="where"></param>
		/// <param name="columns"></param>
		/// <param name="pageSize"></param>
		/// <param name="currentPage"></param>
		/// <param name="connection"></param>
		/// <param name="args"></param>
		/// <returns>The result of the paged query. Result properties are Items, TotalPages, and TotalRecords.</returns>
		/// <remarks>
		/// `columns` parameter is not placed first because it's an override to something we may have alread provided in the constructor,
		/// so we don't want the user to have to non-fluently re-type it, or else type null, every time.
		/// </remarks>
		abstract public PagedResults<T> Paged(
            string orderBy = null,
            string where = null,
			string columns = null,
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
		/// <param name="connection">The connection</param>
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
		/// <param name="connection">The connection</param>
		/// <param name="items">The items</param>
		/// <returns></returns>
		abstract public int Save(DbConnection connection, IEnumerable<object> items);

		/// <summary>
		/// Insert single item, returning the item sent in but with PK populated.
		/// If you need all fields populated (i.e. you want to get back DB default values for non-PK fields), please create the item using New() before inserting it.
		/// </summary>
		/// <param name="items">The item to insert, in any reasonable format (for MightyOrm&lt;T&gt; this includes, but is not limited to, in instance of type T)</param>
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
		/// <param name="connection">The connection</param>
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
		/// <param name="connection">The connection</param>
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
		/// <param name="connection">The connection</param>
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
		/// <param name="connection">The connection</param>
		/// <param name="items">The items</param>
		/// <returns></returns>
		abstract public int Update(DbConnection connection, IEnumerable<object> items);

		/// <summary>
		/// Delete one or more items using params style arguments
		/// </summary>
		/// <param name="items">The items</param>
		/// <returns></returns>
		abstract public int Delete(params object[] items);

		/// <summary>
		/// Delete one or more items using pre-specified DbConnection
		/// </summary>
		/// <param name="connection">The connection</param>
		/// <param name="items">The items</param>
		/// <returns></returns>
		abstract public int Delete(DbConnection connection, params object[] items);

		/// <summary>
		/// Delete array or other IEnumerable of items
		/// </summary>
		/// <param name="items">The items</param>
		/// <returns></returns>
		abstract public int Delete(IEnumerable<object> items);

		/// <summary>
		/// Delete array or other IEnumerable of items using pre-specified DbConnection
		/// </summary>
		/// <param name="connection">The connection</param>
		/// <param name="items">The items</param>
		/// <returns></returns>
		abstract public int Delete(DbConnection connection, IEnumerable<object> items);

		/// <summary>
		/// Apply all fields which are present in item to the row matching key.
		/// We *don't* filter by available columns - call with <see cref="CreateFrom"/>(<see cref="partialItem"/>) to do that.
		/// </summary>
		/// <param name="partialItem"></param>
		/// <param name="key"></param>
		abstract public int UpdateUsing(object partialItem, object key);

		/// <summary>
		/// Apply all fields which are present in item to the row matching key.
		/// We *don't* filter by available columns - call with <see cref="CreateFrom"/>(<see cref="partialItem"/>) to do that.
		/// </summary>
		/// <param name="partialItem"></param>
		/// <param name="key"></param>
		/// <param name="connection"></param>
		abstract public int UpdateUsing(object partialItem, object key,
			DbConnection connection);

		/// <summary>
		/// Apply all fields which are present in item to all rows matching where clause
		/// for safety you MUST specify the where clause yourself (use "1=1" to update all rows)/
		/// This removes/ignores any PK fields from the action; keeps auto-named params for args,
		/// and uses named params for the update feilds.
		/// </summary>
		/// <param name="partialItem"></param>
		/// <param name="where"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		abstract public int UpdateUsing(object partialItem, string where,
			params object[] args);

		/// <summary>
		/// Update from fields in the item sent in. If PK has been specified, any primary key fields in the
		/// item are ignored (this is an update, not an insert!). However the item is not filtered to remove fields
		/// not in the table. If you need that, call <see cref="NewFrom"/>(<see cref="partialItem"/>, false) first.
		/// </summary>
		/// <param name="partialItem"></param>
		/// <param name="where"></param>
		/// <param name="connection"></param>
		/// <param name="args"></param>
		abstract public int UpdateUsing(object partialItem, string where,
			DbConnection connection,
			params object[] args);

		/// <summary>
		/// Delete rows from ORM table based on WHERE clause.
		/// </summary>
		/// <param name="where">
		/// Non-optional where clause.
		/// Specify "1=1" if you are sure that you want to delete all rows.</param>
		/// <param name="args">Optional auto-named parameters for the WHERE clause</param>
		/// <returns></returns>
		abstract public int Delete(string where,
			params object[] args);

		abstract public int Delete(string where,
			DbConnection connection,
			params object[] args);

		// kv pair stuff for dropdowns - a method to convert IEnumerable<T> to kv pair
		abstract public IDictionary<string, string> KeyValues(string orderBy = "");
		#endregion
	}
}
