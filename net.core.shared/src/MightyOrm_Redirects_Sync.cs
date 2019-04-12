using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

using Mighty.Interfaces;
using Mighty.Mapping;
using Mighty.Plugins;
using Mighty.Profiling;
using Mighty.Validation;
using System;
using System.Dynamic;

// <summary>
// MightyOrm_Redirects.cs holds methods in Mighty than can be very simply defined in terms of other methods.
// </summary>
namespace Mighty
{
	public partial class MightyOrm<T> : MightyOrmAbstractInterface<T> where T : class, new()
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
            using (var command = CreateCommandWithParams(sql, args: args))
            {
                return Execute(command);
            }
		}

		override public int Execute(string sql,
			DbConnection connection,
			params object[] args)
		{
            using (var command = CreateCommandWithParams(sql, args: args))
            {
                return Execute(command, connection);
            }
		}

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
		override public dynamic ExecuteWithParams(string sql,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args)
		{
            var retval = CreateCommandWithParamsAndRowCountCheck(sql,
                inParams, outParams, ioParams, returnParams,
                args: args);
            using (retval.Item1)
            {
                var rowCount = Execute(retval.Item1, connection);
                var results = ResultsAsExpando(retval.Item1);
                if (retval.Item2)
                {
                    AppendRowCountResults(rowCount, outParams, results);
                }
                return results;
            }
		}

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
		override public dynamic ExecuteProcedure(string spName,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args)
		{
            var retval = CreateCommandWithParamsAndRowCountCheck(spName,
                inParams, outParams, ioParams, returnParams,
                isProcedure: true,
                args: args);
            using (retval.Item1)
            {
                var rowCount = Execute(retval.Item1, connection);
                var results = ResultsAsExpando(retval.Item1);
                if (retval.Item2)
                {
                    AppendRowCountResults(rowCount, outParams, results);
                }
                return results;
            }
        }

        // no connection, easy args
        override public object Scalar(string sql,
			params object[] args)
		{
            using (var command = CreateCommand(sql, args))
            {
                return Scalar(command);
            }
		}

		override public object Scalar(string sql,
			DbConnection connection,
			params object[] args)
		{
            using (var command = CreateCommand(sql, args))
            {
                return Scalar(command, connection);
            }
		}

		override public object ScalarWithParams(string sql,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args)
		{
            using (var command = CreateCommandWithParams(sql,
            inParams, outParams, ioParams, returnParams,
            args: args))
            {
                return Scalar(command, connection);
            }
		}

		override public object ScalarFromProcedure(string spName,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args)
		{
            using (var command = CreateCommandWithParams(spName,
            inParams, outParams, ioParams, returnParams,
            isProcedure: true,
            args: args))
            {
                return Scalar(command, connection);
            }
		}

		override protected IEnumerable<X> QueryNWithParams<X>(string sql = null, object inParams = null, object outParams = null, object ioParams = null, object returnParams = null, bool isProcedure = false, CommandBehavior behavior = CommandBehavior.Default, DbConnection connection = null, params object[] args)
		{
            var command = CreateCommandWithParams(sql, inParams, outParams, ioParams, returnParams, isProcedure, null, args);
            return QueryNWithParams<X>(command, behavior, connection);
		}
        #endregion

        #region Table specific methods
        // In theory COUNT expression could vary across SQL variants, in practice it doesn't.

        /// <summary>
        /// Perform COUNT on current table.
        /// </summary>
		/// <param name="where">WHERE clause</param>
        /// <param name="columns">Columns (defaults to *, but can be specified, e.g., to count non-nulls in a given field)</param>
		/// <param name="connection">Optional connection to use</param>
		/// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <returns></returns>
        override public object Count(
            string where = null,
            string columns = "*",
            DbConnection connection = null,
            params object[] args)
        {
            return AggregateWithParams("COUNT", columns, where, connection, args: args);
        }

        /// <summary>
        /// Perform COUNT on current table.
        /// </summary>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="columns">Columns (defaults to *, but can be specified, e.g., to count non-nulls in a given field)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        override public object Count(
            object whereParams = null,
            string columns = "*",
            DbConnection connection = null)
        {
            return Aggregate("COUNT", columns, whereParams, connection);
        }

        /// <summary>
        /// Get MAX of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
		/// <param name="where">WHERE clause</param>
		/// <param name="connection">Optional connection to use</param>
		/// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <returns></returns>
        override public object Max(
            string columns,
            string where = null,
            DbConnection connection = null,
            params object[] args)
        {
            return AggregateWithParams("MAX", columns, where, connection, args: args);
        }

        /// <summary>
        /// Get MAX of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        override public object Max(
            string columns,
            object whereParams = null,
            DbConnection connection = null)
        {
            return Aggregate("MAX", columns, whereParams, connection);
        }

        /// <summary>
        /// Get MIN of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
		/// <param name="where">WHERE clause</param>
		/// <param name="connection">Optional connection to use</param>
		/// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <returns></returns>
        override public object Min(
            string columns,
            string where = null,
            DbConnection connection = null,
            params object[] args)
        {
            return AggregateWithParams("MIN", columns, where, connection, args: args);
        }

        /// <summary>
        /// Get MIN of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        override public object Min(
            string columns,
            object whereParams = null,
            DbConnection connection = null)
        {
            return Aggregate("MIN", columns, whereParams, connection);
        }

        /// <summary>
        /// Get SUM of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
		/// <param name="where">WHERE clause</param>
		/// <param name="connection">Optional connection to use</param>
		/// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <returns></returns>
        override public object Sum(
            string columns,
            string where = null,
            DbConnection connection = null,
            params object[] args)
        {
            return AggregateWithParams("SUM", columns, where, connection, args: args);
        }

        /// <summary>
        /// Get SUM of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        override public object Sum(
            string columns,
            object whereParams = null,
            DbConnection connection = null)
        {
            return Aggregate("SUM", columns, whereParams, connection);
        }

        /// <summary>
        /// Get AVG of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
		/// <param name="where">WHERE clause</param>
		/// <param name="connection">Optional connection to use</param>
		/// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <returns></returns>
        override public object Avg(
            string columns,
            string where = null,
            DbConnection connection = null,
            params object[] args)
        {
            return AggregateWithParams("AVG", columns, where, connection, args: args);
        }

        /// <summary>
        /// Get AVG of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        override public object Avg(
            string columns,
            object whereParams = null,
            DbConnection connection = null)
        {
            return Aggregate("AVG", columns, whereParams, connection);
        }

        /// <summary>
        /// Perform aggregate operation on the current table (use for SUM, MAX, MIN, AVG, etc.)
        /// </summary>
        /// <param name="function">Aggregate function</param>
        /// <param name="columns">Columns for aggregate function</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <returns></returns>
        override public object Aggregate(string function, string columns, string where = null,
			DbConnection connection = null,
			params object[] args)
		{
			return AggregateWithParams(function, columns, where, connection: connection, args: args);
		}

        /// <summary>
        /// Perform aggregate operation on the current table (use for SUM, MAX, MIN, AVG, etc.)
        /// </summary>
        /// <param name="function">Aggregate function</param>
        /// <param name="columns">Columns for aggregate function</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        override public object Aggregate(string function, string columns, object whereParams = null,
            DbConnection connection = null)
        {
            Tuple<string, object, object[]> retval = GetWhereSpecFromWhereParams(whereParams);
            return AggregateWithParams(
                function, columns,
                where: retval.Item1, inParams: retval.Item2, args: retval.Item3,
                connection: connection);
        }

        /// <summary>
        /// Get single object from the current table using primary key or name-value specification.
        /// </summary>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="columns">List of columns to return</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        override public T Single(object whereParams, string columns = null,
            DbConnection connection = null)
        {
            Tuple<string, object, object[]> retval = GetWhereSpecFromWhereParams(whereParams);
            return AllWithParams(
                    where: retval.Item1, inParams: retval.Item2, args: retval.Item3, columns: columns, limit: 1,
                    connection: connection)
                .FirstOrDefault();
        }

        /// <summary>
        /// Get a single object from the current table with where specification.
        /// </summary>
        /// <param name="where">WHERE clause</param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
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
		/// Get single item from current table
		/// </summary>
		/// <param name="where">WHERE clause</param>
		/// <param name="connection">Optional connection to use</param>
		/// <param name="orderBy">ORDER BY clause</param>
		/// <param name="columns">Columns to return</param>
		/// <param name="args">Auto-numbered parameter values for WHERE clause</param>
		/// <returns></returns>
		/// <remarks>
		/// DbConnection coming early (not just before args) in this one case is really useful, as it avoids ambiguity between
		/// the `columns` and `orderBy` strings and optional string args.
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

        override public IEnumerable<T> All(
            object whereParams = null, string orderBy = null, string columns = null, int limit = 0)
        {
            Tuple<string, object, object[]> retval = GetWhereSpecFromWhereParams(whereParams);
            if (retval.Item3 != null)
            {
                throw new InvalidOperationException($"{nameof(whereParams)} in {nameof(All)}(...) should contain names and values but it contained values only. If you want to get a single item by its primary key use {nameof(Single)}(...) instead.");
            }
            return AllWithParams(
                where: retval.Item1, inParams: retval.Item2,
                orderBy: orderBy, columns: columns, limit: limit);
        }

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
        /// `columns` parameter is not placed first because it's an override to something we may have already provided in the constructor
        /// (so we don't want the user to have to non-fluently re-type it, or else type null, every time).
        /// </remarks>
        override public PagedResults<T> Paged(
            string orderBy = null,
            string columns = null,
            string where = null,
			int pageSize = 20, int currentPage = 1,
			DbConnection connection = null,
			params object[] args)
		{
			return PagedFromSelect(CheckGetTableName(), orderBy ?? CheckGetPrimaryKeyFields(), columns, where, pageSize, currentPage, connection, args);
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
		/// <param name="connection">The connection to use</param>
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
		/// <param name="connection">The connection to use</param>
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
		/// <param name="item">The item to insert, in any reasonable format (for MightyOrm&lt;T&gt; this includes, but is not limited to, in instance of type T)</param>
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
		/// <param name="connection">The connection to use</param>
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
		/// <param name="connection">The connection to use</param>
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
		/// <param name="connection">The connection to use</param>
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
		/// <param name="connection">The connection to use</param>
		/// <param name="items">The items</param>
		/// <returns></returns>
		override public int Update(DbConnection connection, IEnumerable<object> items)
		{
			return ActionOnItems(OrmAction.Update, connection, items);
		}

        /// <summary>
        /// Delete one or more items using params style arguments.
        /// Each argument may be (or contain) a value (or values) only, in which case
        /// it specifies the primary key value(s) of the item to delete, or it can be any object containing name-values pairs in which case
        /// it should contain fields with names matching the primary key(s) whose values will specify the item to delete (but it may contain
        /// other fields as well which will be ignored here).
        /// </summary>
        /// <param name="items">The items</param>
        /// <returns>The number of items affected</returns>
        override public int Delete(params object[] items)
		{
			return ActionOnItems(OrmAction.Delete, null, items);
		}

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
		override public int Delete(DbConnection connection, params object[] items)
		{
			return ActionOnItems(OrmAction.Delete, connection, items);
		}

        /// <summary>
        /// Delete an array or other <see cref="IEnumerable"/> of items.
        /// Each argument may be (or contain) a value (or values) only, in which case
        /// it specifies the primary key value(s) of the item to delete, or it can be any object containing name-values pairs in which case
        /// it should contain fields with names matching the primary key(s) whose values will specify the item to delete (but it may contain
        /// other fields as well which will be ignored here).
        /// </summary>
        /// <param name="items">The items</param>
        /// <returns>The number of items affected</returns>
		override public int Delete(IEnumerable<object> items)
		{
			return ActionOnItems(OrmAction.Delete, null, items);
		}

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
		override public int Delete(DbConnection connection, IEnumerable<object> items)
		{
			return ActionOnItems(OrmAction.Delete, connection, items);
		}

        /// <summary>
        /// Update the row(s) specified by the primary key(s) or WHERE values sent in using the values from the item sent in.
        /// If `primaryKeyFields` has been specified on the current Mighty instance then any primary key fields in the item are ignored.
        /// The item is not filtered to remove fields not in the table, if you need that you can call <see cref="NewFrom"/> with first parameter `partialItem` and second parameter `false` first.
        /// </summary>
        /// <param name="partialItem">Item containing values to update with</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
		override public int UpdateUsing(object partialItem, object whereParams)
		{
			return UpdateUsing(partialItem, whereParams, null);
		}

        /// <summary>
        /// Update the row(s) specified by the primary key(s) or WHERE values sent in using the values from the item sent in.
        /// If `primaryKeyFields` has been specified on the current Mighty instance then any primary key fields in the item are ignored.
        /// The item is not filtered to remove fields not in the table, if you need that you can call <see cref="NewFrom"/> with first parameter `partialItem` and second parameter `false` first.
        /// </summary>
        /// <param name="partialItem">Item containing values to update with</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
		override public int UpdateUsing(object partialItem, object whereParams,
			DbConnection connection)
		{
            Tuple<string, object, object[]> retval = GetWhereSpecFromWhereParams(whereParams);
            return UpdateUsingWithParams(partialItem,
                where: retval.Item1, inParams: retval.Item2, args: retval.Item3,
                connection: connection);
		}

        /// <summary>
        /// Update all items matching WHERE clause using fields from the item sent in.
        /// If `primaryKeyFields` has been specified on the current Mighty instance then any primary key fields in the item are ignored.
        /// The item is not filtered to remove fields not in the table, if you need that you can call <see cref="NewFrom"/> with first parameter `partialItem` and second parameter `false` first.
        /// </summary>
        /// <param name="partialItem">Item containing values to update with</param>
        /// <param name="where">WHERE clause specifying which rows to update</param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
		override public int UpdateUsing(object partialItem, string where,
			params object[] args)
		{
			return UpdateUsing(partialItem, where, null, args);
		}

        /// <summary>
        /// Delete one or more items based on a WHERE clause.
        /// </summary>
        /// <param name="where">
        /// Non-optional WHERE clause.
        /// Specify "1=1" if you are sure that you want to delete all rows.
        /// </param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <returns>The number of items affected</returns>
        override public int Delete(string where,
			params object[] args)
		{
			return Delete(where, null, args);
		}

#pragma warning disable IDE0059 // Value assigned is never used
        internal int ActionOnItems(OrmAction action, DbConnection connection, IEnumerable<object> items)
		{
			T insertedItem;
			return ActionOnItems(action, connection, items, out insertedItem);
		}
#pragma warning restore IDE0059
        #endregion
    }
}
