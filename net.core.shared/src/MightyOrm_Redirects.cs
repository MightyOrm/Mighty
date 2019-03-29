using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

using MightyOrm.Mocking;
using MightyOrm.Mapping;
using MightyOrm.Plugins;
using MightyOrm.Profiling;
using MightyOrm.Validation;

/// <summary>
/// MightyOrm_Redirects.cs holds methods in Mighty than can be very simply defined in terms of other methods.
/// </summary>
namespace MightyOrm
{
    public partial class MightyOrm<T> : MightyOrmMockable<T> where T : class, new()
    {
        #region Non-table specific methods
        override public IEnumerable<T> Query(DbCommand command,
            DbConnection connection = null)
        {
            return QueryNWithParams<T>(command: command, connection: connection);
        }

        override public T Single(DbCommand command,
            DbConnection connection = null)
        {
            return QueryNWithParams<T>(command: command, connection: connection).FirstOrDefault();
        }

        // no connection, easy args
        override public IEnumerable<T> Query(string sql,
            params object[] args)
        {
            return QueryNWithParams<T>(sql, args: args);
        }

        override public T SingleFromQuery(string sql,
            params object[] args)
        {
            return QueryNWithParams<T>(sql, args: args).FirstOrDefault();
        }

        override public IEnumerable<T> Query(string sql,
            DbConnection connection,
            params object[] args)
        {
            return QueryNWithParams<T>(sql, connection: connection, args: args);
        }

        override public T SingleFromQuery(string sql,
            DbConnection connection,
            params object[] args)
        {
            return QueryNWithParams<T>(sql, connection: connection, args: args).FirstOrDefault();
        }

        override public IEnumerable<T> QueryWithParams(string sql,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            return QueryNWithParams<T>(sql,
                inParams, outParams, ioParams, returnParams,
                connection: connection, args: args);
        }

        override public T SingleFromQueryWithParams(string sql,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            return QueryNWithParams<T>(sql,
                inParams, outParams, ioParams, returnParams,
                connection: connection, args: args).FirstOrDefault();
        }

        override public IEnumerable<T> QueryFromProcedure(string spName,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            return QueryNWithParams<T>(spName,
                inParams, outParams, ioParams, returnParams,
                isProcedure: true,
                connection: connection, args: args);
        }

        override public T SingleFromProcedure(string spName,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            return QueryNWithParams<T>(spName,
                inParams, outParams, ioParams, returnParams,
                isProcedure: true,
                connection: connection, args: args).FirstOrDefault();
        }

        override public IEnumerable<IEnumerable<T>> QueryMultiple(DbCommand command,
            DbConnection connection = null)
        {
            return QueryNWithParams<IEnumerable<T>>(command: command, connection: connection);
        }

        // no connection, easy args
        override public IEnumerable<IEnumerable<T>> QueryMultiple(string sql,
            params object[] args)
        {
            return QueryNWithParams<IEnumerable<T>>(sql, args: args);
        }

        override public IEnumerable<IEnumerable<T>> QueryMultiple(string sql,
            DbConnection connection,
            params object[] args)
        {
            return QueryNWithParams<IEnumerable<T>>(sql, connection: connection, args: args);
        }

        override public IEnumerable<IEnumerable<T>> QueryMultipleWithParams(string sql,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            return QueryNWithParams<IEnumerable<T>>(sql,
                inParams, outParams, ioParams, returnParams,
                connection: connection, args: args);
        }

        override public IEnumerable<IEnumerable<T>> QueryMultipleFromProcedure(string spName,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            return QueryNWithParams<IEnumerable<T>>(spName,
                inParams, outParams, ioParams, returnParams,
                isProcedure: true,
                connection: connection, args: args);
        }

        // no connection, easy args
        override public int Execute(string sql,
            params object[] args)
        {
            var command = CreateCommandWithParams(sql, args: args);
            return Execute(command);
        }

        override public int Execute(string sql,
            DbConnection connection,
            params object[] args)
        {
            var command = CreateCommandWithParams(sql, args: args);
            return Execute(command, connection);
        }

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
        override public dynamic ExecuteWithParams(string sql,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            var command = CreateCommandWithParams(sql,
            inParams, outParams, ioParams, returnParams,
            args: args);
            Execute(command, connection);
            return ResultsAsExpando(command);
        }

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
        override public dynamic ExecuteAsProcedure(string spName,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            var command = CreateCommandWithParams(spName,
            inParams, outParams, ioParams, returnParams,
            isProcedure: true,
            args: args);
            Execute(command, connection);
            return ResultsAsExpando(command);
        }

        // no connection, easy args
        override public object Scalar(string sql,
            params object[] args)
        {
            var command = CreateCommand(sql, args);
            return Scalar(command);
        }

        override public object Scalar(string sql,
            DbConnection connection,
            params object[] args)
        {
            var command = CreateCommand(sql, args);
            return Scalar(command, connection);
        }

        override public object ScalarWithParams(string sql,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            var command = CreateCommandWithParams(sql,
            inParams, outParams, ioParams, returnParams,
            args: args);
            return Scalar(command, connection);
        }

        override public object ScalarFromProcedure(string spName,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            var command = CreateCommandWithParams(spName,
            inParams, outParams, ioParams, returnParams,
            isProcedure: true,
            args: args);
            return Scalar(command, connection);
        }

        override public DbCommand CreateCommand(string sql,
            params object[] args)
        {
            return CreateCommandWithParams(sql, args: args);
        }

        override public DbCommand CreateCommand(string sql,
            DbConnection connection,
            params object[] args)
        {
            return CreateCommandWithParams(sql, args: args);
        }

        override protected IEnumerable<X> QueryNWithParams<X>(string sql = null, object inParams = null, object outParams = null, object ioParams = null, object returnParams = null, bool isProcedure = false, CommandBehavior behavior = CommandBehavior.Default, DbConnection connection = null, params object[] args)
        {
            var command = CreateCommandWithParams(sql, inParams, outParams, ioParams, returnParams, isProcedure, null, args);
            return QueryNWithParams<X>(command, behavior, connection);
        }
        #endregion

        #region Table specific methods
        /// <summary>
        /// Perform scalar operation on the current table (use for SUM, MAX, MIN, AVG, etc.)
        /// </summary>
        /// <param name="expression">Scalar expression</param>
        /// <param name="where">Optional where clause</param>
        /// <param name="connection">Optional connection</param>
        /// <param name="args">Parameters</param>
        /// <returns></returns>
        override public object Aggregate(string expression, string where = null,
            DbConnection connection = null,
            params object[] args)
        {
            return AggregateWithParams(expression, where, connection: connection, args: args);
        }

        /// <summary>
        /// Get a single object from the current table by primary key value
        /// </summary>
        /// <param name="key">Single key (or any reasonable multi-value item for compound keys)</param>
        /// <param name="columns">Optional columns to retrieve</param>
        /// <param name="connection">Optional connection</param>
        /// <returns></returns>
        override public T Single(object key, string columns = null,
            DbConnection connection = null)
        {
            return Single(WhereForKeys(), connection, columns, KeyValuesFromKey(key));
        }

        /// <summary>
        /// Get a single object from the current table with where specification.
        /// </summary>
        /// <param name="where">Where clause</param>
        /// <param name="args">Optional auto-named params</param>
        /// <returns></returns>
        /// <remarks>
        /// 'Easy-calling' version, optional args straight after where.
        /// </remarks>
        override public T Single(string where,
            params object[] args)
        {
            return SingleWithParams(where, args: args);
        }

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
        override public T Single(string where,
            DbConnection connection = null,
            string orderBy = null,
            string columns = null,
            params object[] args)
        {
            return SingleWithParams(where, orderBy, columns, connection: connection, args: args);
        }

        // WithParams version just in case; allows transactions for a start
        override public T SingleWithParams(string where, string orderBy = null, string columns = null,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            return AllWithParams(
                where, orderBy, columns, 1,
                inParams, outParams, ioParams, returnParams,
                connection,
                args).FirstOrDefault();
        }

        // ORM
        override public IEnumerable<T> All(
            string where = null, string orderBy = null, string columns = null, int limit = 0,
            params object[] args)
        {
            return AllWithParams(where, orderBy, columns, limit, args: args);
        }

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
        /// <see cref="columns"/> parameter is not placed first because it's an override to something we may have alread provided in the constructor
        /// (so we don't want the user to have to non-fluently re-type it, or else type null, every time).
        /// </remarks>
        override public PagedResults<T> Paged(string where = null, string orderBy = null,
            string columns = null,
            int pageSize = 20, int currentPage = 1,
            DbConnection connection = null,
            params object[] args)
        {
            return PagedFromSelect(columns, CheckGetTableName(), where, orderBy ?? CheckGetPrimaryKeyFields(), pageSize, currentPage, connection, args);
        }

        /// <summary>
        /// Save one or more items using params style arguments
        /// </summary>
        /// <param name="items">The items</param>
        /// <returns></returns>
        override public int Save(params object[] items)
        {
            return ActionOnItems(OrmAction.Save, null, items);
        }

        /// <summary>
        /// Save one or more items using pre-specified DbConnection
        /// </summary>
        /// <param name="connection">The connection</param>
        /// <param name="items">The items</param>
        /// <returns></returns>
        override public int Save(DbConnection connection, params object[] items)
        {
            return ActionOnItems(OrmAction.Save, connection, items);
        }

        /// <summary>
        /// Save array or other IEnumerable of items
        /// </summary>
        /// <param name="items">The items</param>
        /// <returns></returns>
        override public int Save(IEnumerable<object> items)
        {
            return ActionOnItems(OrmAction.Save, null, items);
        }

        /// <summary>
        /// Save array or other IEnumerable of items using pre-specified DbConnection
        /// </summary>
        /// <param name="connection">The connection</param>
        /// <param name="items">The items</param>
        /// <returns></returns>
        override public int Save(DbConnection connection, IEnumerable<object> items)
        {
            return ActionOnItems(OrmAction.Save, connection, items);
        }

        /// <summary>
        /// Insert single item, returning the item sent in but with PK populated.
        /// If you need all fields populated (i.e. you want to get back DB default values for non-PK fields), please create the item using New() before inserting it.
        /// </summary>
        /// <param name="items">The item to insert, in any reasonable format (for MightyOrm&lt;T&gt; this includes, but is not limited to, in instance of type T)</param>
        /// <returns>The inserted item</returns>
        override public T Insert(object item)
        {
            T insertedItem;
            ActionOnItems(OrmAction.Insert, null, new object[] { item }, out insertedItem);
            return insertedItem;
        }

        /// <summary>
        /// Insert one or more items using params style arguments
        /// </summary>
        /// <param name="items">The items</param>
        /// <returns>The number of rows inserted</returns>
        override public int Insert(params object[] items)
        {
            return ActionOnItems(OrmAction.Insert, null, items);
        }

        /// <summary>
        /// Insert one or more items using pre-specified DbConnection
        /// </summary>
        /// <param name="connection">The connection</param>
        /// <param name="items">The items</param>
        /// <returns>The number of rows inserted</returns>
        override public int Insert(DbConnection connection, params object[] items)
        {
            return ActionOnItems(OrmAction.Insert, connection, items);
        }

        /// <summary>
        /// Insert array or other IEnumerable of items
        /// </summary>
        /// <param name="items">The items</param>
        /// <returns>The number of rows inserted</returns>
        override public int Insert(IEnumerable<object> items)
        {
            return ActionOnItems(OrmAction.Insert, null, items);
        }

        /// <summary>
        /// Insert array or other IEnumerable of items using pre-specified DbConnection
        /// </summary>
        /// <param name="connection">The connection</param>
        /// <param name="items">The items</param>
        /// <returns>The number of rows inserted</returns>
        override public int Insert(DbConnection connection, IEnumerable<object> items)
        {
            return ActionOnItems(OrmAction.Insert, connection, items);
        }

        /// <summary>
        /// Update one or more items using params style arguments
        /// </summary>
        /// <param name="items">The items</param>
        /// <returns></returns>
        override public int Update(params object[] items)
        {
            return ActionOnItems(OrmAction.Update, null, items);
        }

        /// <summary>
        /// Update one or more items using pre-specified DbConnection
        /// </summary>
        /// <param name="connection">The connection</param>
        /// <param name="items">The items</param>
        /// <returns></returns>
        override public int Update(DbConnection connection, params object[] items)
        {
            return ActionOnItems(OrmAction.Update, connection, items);
        }

        /// <summary>
        /// Update array or other IEnumerable of items
        /// </summary>
        /// <param name="items">The items</param>
        /// <returns></returns>
        override public int Update(IEnumerable<object> items)
        {
            return ActionOnItems(OrmAction.Update, null, items);
        }

        /// <summary>
        /// Update array or other IEnumerable of items using pre-specified DbConnection
        /// </summary>
        /// <param name="connection">The connection</param>
        /// <param name="items">The items</param>
        /// <returns></returns>
        override public int Update(DbConnection connection, IEnumerable<object> items)
        {
            return ActionOnItems(OrmAction.Update, connection, items);
        }

        /// <summary>
        /// Delete one or more items using params style arguments
        /// </summary>
        /// <param name="items">The items</param>
        /// <returns></returns>
        override public int Delete(params object[] items)
        {
            return ActionOnItems(OrmAction.Delete, null, items);
        }

        /// <summary>
        /// Delete one or more items using pre-specified DbConnection
        /// </summary>
        /// <param name="connection">The connection</param>
        /// <param name="items">The items</param>
        /// <returns></returns>
        override public int Delete(DbConnection connection, params object[] items)
        {
            return ActionOnItems(OrmAction.Delete, connection, items);
        }

        /// <summary>
        /// Delete array or other IEnumerable of items
        /// </summary>
        /// <param name="items">The items</param>
        /// <returns></returns>
        override public int Delete(IEnumerable<object> items)
        {
            return ActionOnItems(OrmAction.Delete, null, items);
        }

        /// <summary>
        /// Delete array or other IEnumerable of items using pre-specified DbConnection
        /// </summary>
        /// <param name="connection">The connection</param>
        /// <param name="items">The items</param>
        /// <returns></returns>
        override public int Delete(DbConnection connection, IEnumerable<object> items)
        {
            return ActionOnItems(OrmAction.Delete, connection, items);
        }

        override public T New()
        {
            return NewFrom();
        }

        /// <summary>
        /// Apply all fields which are present in item to the row matching key.
        /// We *don't* filter by available columns - call with <see cref="CreateFrom"/>(<see cref="partialItem"/>) to do that.
        /// </summary>
        /// <param name="partialItem"></param>
        /// <param name="key"></param>
        override public int UpdateUsing(object partialItem, object key)
        {
            return UpdateUsing(partialItem, key, null);
        }

        /// <summary>
        /// Apply all fields which are present in item to the row matching key.
        /// We *don't* filter by available columns - call with <see cref="CreateFrom"/>(<see cref="partialItem"/>) to do that.
        /// </summary>
        /// <param name="partialItem"></param>
        /// <param name="key"></param>
        /// <param name="connection"></param>
        override public int UpdateUsing(object partialItem, object key,
            DbConnection connection)
        {
            return UpdateUsing(partialItem, WhereForKeys(), KeyValuesFromKey(key));
        }

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
        override public int UpdateUsing(object partialItem, string where,
            params object[] args)
        {
            return UpdateUsing(partialItem, where, null, args);
        }

        /// <summary>
        /// Delete rows from ORM table based on WHERE clause.
        /// </summary>
        /// <param name="where">
        /// Non-optional where clause.
        /// Specify "1=1" if you are sure that you want to delete all rows.</param>
        /// <param name="args">Optional auto-named parameters for the WHERE clause</param>
        /// <returns></returns>
        override public int Delete(string where,
            params object[] args)
        {
            return Delete(where, null, args);
        }

        internal int ActionOnItems(OrmAction action, DbConnection connection, IEnumerable<object> items)
        {
            T insertedItem;
            return ActionOnItems(action, connection, items, out insertedItem);
        }
        #endregion
    }
}
