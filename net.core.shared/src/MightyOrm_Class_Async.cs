#if !NET40
using System;
using System.Collections.Async;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
#if NETFRAMEWORK
using System.Transactions;
#endif

using Mighty.ConnectionProviders;
using Mighty.Plugins;
using Mighty.Interfaces;
using Mighty.Mapping;
using Mighty.Parameters;
using Mighty.Profiling;
using Mighty.Validation;
using System.Threading.Tasks;

namespace Mighty
{
    /// <summary>
    /// Strongly typed MightyOrm instance.
    /// </summary>
    /// <typeparam name="T">The generic type for items returned by this instance</typeparam>
    public partial class MightyOrm<T> : MightyOrmAbstractInterface<T> where T : class, new()
    {
        // Only methods with a non-trivial implementation are here, the rest are in the MightyOrm_Redirects_Async file.
        #region MircoORM interface
        /// <summary>
        /// Perform aggregate operation on the current table (use for SUM, MAX, MIN, AVG, etc.), with support for named params.
        /// </summary>
        /// <param name="function">Aggregate function</param>
        /// <param name="columns">Columns for aggregate function</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="inParams">Named input parameters</param>
        /// <param name="outParams">Named output parameters</param>
        /// <param name="ioParams">Named input-output parameters</param>
        /// <param name="returnParams">Named return parameters</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <returns></returns>
        /// <remarks>
        /// This only lets you pass in the aggregate expressions of your SQL variant, but SUM, AVG, MIN, MAX are supported on all.
        /// </remarks>
        /// <remarks>
        /// This is very close to a 'redirect' method, but couldn't have been in the abstract interface before because of the plugin access.
        /// </remarks>
        override public async Task<object> AggregateWithParamsAsync(string function, string columns, string where = null,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await AggregateWithParamsAsync(function, columns, CancellationToken.None, where,
                inParams, outParams, ioParams, returnParams,
                connection,
                args: args);
        }

        /// <summary>
        /// Perform aggregate operation on the current table (use for SUM, MAX, MIN, AVG, etc.), with support for named params.
        /// </summary>
        /// <param name="function">Aggregate function</param>
        /// <param name="columns">Columns for aggregate function</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="inParams">Named input parameters</param>
        /// <param name="outParams">Named output parameters</param>
        /// <param name="ioParams">Named input-output parameters</param>
        /// <param name="returnParams">Named return parameters</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        /// <remarks>
        /// This only lets you pass in the aggregate expressions of your SQL variant, but SUM, AVG, MIN, MAX are supported on all.
        /// </remarks>
        /// <remarks>
        /// This is very close to a 'redirect' method, but couldn't have been in the abstract interface before because of the plugin access.
        /// </remarks>
        override public async Task<object> AggregateWithParamsAsync(string function, string columns, CancellationToken cancellationToken, string where = null,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await ScalarWithParamsAsync(Plugin.BuildSelect(string.Format("{0}({1})", function, columns), CheckGetTableName(), where),
                cancellationToken,
                inParams, outParams, ioParams, returnParams,
                connection, args).ConfigureAwait(false);
        }

        /// <summary>
        /// Update all items matching WHERE clause using fields from the item sent in.
        /// If `primaryKeyFields` has been specified on the current Mighty instance then any primary key fields in the item are ignored.
        /// The item is not filtered to remove fields not in the table, if you need that you can call <see cref="NewFrom"/> with first parameter `partialItem` and second parameter `false` first.
        /// </summary>
        /// <param name="partialItem">Item containing values to update with</param>
        /// <param name="where">WHERE clause specifying which rows to update</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        override public async Task<int> UpdateUsingAsync(object partialItem, string where,
            DbConnection connection,
            params object[] args)
        {
            return await UpdateUsingWithParamsAsync(
                partialItem,
                where,
                connection,
                CancellationToken.None,
                null,
                args);
        }

        /// <summary>
        /// Update all items matching WHERE clause using fields from the item sent in.
        /// If `primaryKeyFields` has been specified on the current Mighty instance then any primary key fields in the item are ignored.
        /// The item is not filtered to remove fields not in the table, if you need that you can call <see cref="NewFrom"/> with first parameter `partialItem` and second parameter `false` first.
        /// </summary>
        /// <param name="partialItem">Item containing values to update with</param>
        /// <param name="where">WHERE clause specifying which rows to update</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        override public async Task<int> UpdateUsingAsync(object partialItem, string where,
            DbConnection connection,
            CancellationToken cancellationToken,
            params object[] args)
        {
            return await UpdateUsingWithParamsAsync(
                partialItem,
                where,
                connection,
                cancellationToken,
                null,
                args);
        }

        /// <summary>
        /// Update all items matching WHERE clause using fields from the item sent in.
        /// If `primaryKeyFields` has been specified on the current Mighty instance then any primary key fields in the item are ignored.
        /// The item is not filtered to remove fields not in the table, if you need that you can call <see cref="NewFrom"/> with first parameter `partialItem` and second parameter `false` first.
        /// </summary>
        /// <param name="partialItem">Item containing values to update with</param>
        /// <param name="where">WHERE clause specifying which rows to update</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="inParams">Named input parameters</param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        protected async Task<int> UpdateUsingWithParamsAsync(
            object partialItem,
            string where,
            DbConnection connection,
            CancellationToken cancellationToken,
            object inParams,
            params object[] args)
        {
            var setValues = new StringBuilder();
            var partialItemParameters = new NameValueTypeEnumerator(partialItem);
            // TO DO: Test that this combinedInputParams approach works
            var combinedInputParams = inParams?.ToExpando() ?? new ExpandoObject();
            var toDict = combinedInputParams.ToDictionary();
            int i = 0;
            foreach (var paramInfo in partialItemParameters)
            {
                if (!IsKey(paramInfo.Name))
                {
                    if (i > 0) setValues.Append(", ");
                    setValues.Append(paramInfo.Name).Append(" = ").Append(Plugin.PrefixParameterName(paramInfo.Name));
                    i++;

                    toDict.Add(paramInfo.Name, paramInfo.Value);
                }
            }
            var sql = Plugin.BuildUpdate(CheckGetTableName(), setValues.ToString(), where);
            var retval = await ExecuteWithParamsAsync(sql, cancellationToken, args: args, inParams: combinedInputParams, outParams: new { __rowcount = new RowCount() }, connection: connection).ConfigureAwait(false);
            return retval.__rowcount;
        }

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
        override public async Task<int> DeleteAsync(string where,
            DbConnection connection,
            params object[] args)
        {
            return await DeleteAsync(where,
                connection,
                CancellationToken.None,
                args: args);
        }

        /// <summary>
        /// Delete one or more items based on a WHERE clause.
        /// </summary>
        /// <param name="where">
        /// Non-optional WHERE clause.
        /// Specify "1=1" if you are sure that you want to delete all rows.
        /// </param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <param name="connection">The connection to use</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns>The number of items affected</returns>
        override public async Task<int> DeleteAsync(string where,
            DbConnection connection,
            CancellationToken cancellationToken,
            params object[] args)
        {
            var sql = Plugin.BuildDelete(CheckGetTableName(), where);
            return await ExecuteAsync(sql, connection, cancellationToken, args).ConfigureAwait(false);
        }

        /// <summary>
        /// Perform CRUD action for the item(s) in the params list.
        /// An <see cref="IEnumerable{T}"/> of *modified* items is returned; the modification is to update the primary key to the correct new value for inserted items.
        /// If the input item does not support field writes/inserts as needed then an <see cref="ExpandoObject"/> corresponding to the updated item is returned instead.
        /// </summary>
        /// <param name="action">The ORM action</param>
        /// <param name="connection">The connection to use</param>
        /// <param name="items">The item or items</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns>The list of modified items</returns>
        /// <remarks>Here and in <see cref="UpsertItemPK"/> we always return the modified original object where possible</remarks>
        internal async Task<IEnumerable<T>> ActionOnItemsAsync(OrmAction action, DbConnection connection, IEnumerable<object> items, CancellationToken cancellationToken = default)
        {
            return (await ActionOnItemsWithOutputAsync(action, connection, items, cancellationToken).ConfigureAwait(false)).Item2;
        }

        /// <summary>
        /// Perform CRUD action for the item(s) in the params list.
        /// An <see cref="IEnumerable{T}"/> of *modified* items is returned; the modification is to update the primary key to the correct new value for inserted items.
        /// If the input item does not support field writes/inserts as needed then an <see cref="ExpandoObject"/> corresponding to the updated item is returned instead.
        /// </summary>
        /// <param name="action">The ORM action</param>
        /// <param name="connection">The connection to use</param>
        /// <param name="items">The item or items</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns>The list of modified items</returns>
        /// <remarks>Here and in <see cref="UpsertItemPK"/> we always return the modified original object where possible</remarks>
        internal async Task<Tuple<int, IEnumerable<T>>> ActionOnItemsWithOutputAsync(OrmAction action, DbConnection connection, IEnumerable<object> items, CancellationToken cancellationToken = default)
        {
            List<T> modifiedItems = null;
            if (action == OrmAction.Insert)
            {
                modifiedItems = new List<T>();
            }
            int count = 0;
            int affected = 0;
            ValidateAction(items, action);
            foreach (var item in items)
            {
                if (Validator.ShouldPerformAction(item, action))
                {
                    var result = await ActionOnItemAsync(action, item, connection, cancellationToken).ConfigureAwait(false);
                    affected += result.Item1;
                    if (action == OrmAction.Insert)
                    {
                        var modified = result.Item2 ?? item;
                        if (!UseExpando)
                        {
                            var resultT = modified as T;
                            if (resultT == null)
                            {
                                resultT = NewFrom(modified, false);
                            }
                            modified = resultT;
                        }
                        modifiedItems.Add((T)modified);
                    }
                    Validator.HasPerformedAction(item, action);
                }
                count++;
            }
            return new Tuple<int, IEnumerable<T>>(affected, modifiedItems);
        }

        // TO DO: We should still be supporting this
#if KEY_VALUES
        /// <summary>
        /// Returns a string/string dictionary which can be bound directly to dropdowns etc http://stackoverflow.com/q/805595/
        /// </summary>
        override public async Task<IDictionary<string, string>> KeyValuesAsync(string orderBy = "")
        {
            string foo = string.Format(" to call {0}, please provide one in your constructor", nameof(KeyValues));
            string valueField = CheckGetValueColumn(string.Format("ValueField is required{0}", foo));
            string primaryKeyFields = CheckGetKeyName(string.Format("A single primary key must be specified{0}", foo));
            var results = (await AllAsync(orderBy: orderBy, columns: string.Format("{0}, {1}", primaryKeyFields, valueField)).ConfigureAwait(false)).Cast<IDictionary<string, object>>();
            return results.ToDictionary(item => item[primaryKeyFields].ToString(), item => item[valueField].ToString());
        }
#endif
#endregion

        // Only methods with a non-trivial implementation are here, the rest are in the DataAccessWrapper abstract class.
#region DataAccessWrapper interface
        /// <summary>
        /// Creates a new DbConnection. You do not normally need to call this! (MightyOrm normally manages its own
        /// connections. Create a connection here and pass it on to other MightyOrm commands only in non-standard use
        /// cases where you need to explicitly manage transactions or share connections, e.g. when using explicit cursors.)
        /// </summary>
        /// <returns></returns>
        override public async Task<DbConnection> OpenConnectionAsync()
        {
            return await OpenConnectionAsync(CancellationToken.None);
        }

        /// <summary>
        /// Creates a new DbConnection. You do not normally need to call this! (MightyOrm normally manages its own
        /// connections. Create a connection here and pass it on to other MightyOrm commands only in non-standard use
        /// cases where you need to explicitly manage transactions or share connections, e.g. when using explicit cursors.)
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        override public async Task<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken)
        {
            var connection = Factory.CreateConnection();
            connection = SqlProfiler.Wrap(connection);
            connection.ConnectionString = ConnectionString;
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            return connection;
        }

        /// <summary>
        /// Execute DbCommand
        /// </summary>
        /// <param name="command">The command</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        override public async Task<int> ExecuteAsync(DbCommand command,
            DbConnection connection = null)
        {
            return await ExecuteAsync(command, CancellationToken.None, connection);
        }
        override public async Task<int> ExecuteAsync(DbCommand command,
            CancellationToken cancellationToken,
            DbConnection connection = null)
        {
            // using applied only to local connection
            using (var localConn = ((connection == null) ? await OpenConnectionAsync(cancellationToken).ConfigureAwait(false) : null))
            {
                command.Connection = connection ?? localConn;
                return await command.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Return scalar from DbCommand
        /// </summary>
        /// <param name="command">The command</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        override public async Task<object> ScalarAsync(DbCommand command,
            DbConnection connection = null)
        {
            return await ScalarAsync(command, CancellationToken.None, connection);
        }
        override public async Task<object> ScalarAsync(DbCommand command,
            CancellationToken cancellationToken,
            DbConnection connection = null)
        {
            // using applied only to local connection
            using (var localConn = ((connection == null) ? await OpenConnectionAsync(cancellationToken).ConfigureAwait(false) : null))
            {
                command.Connection = connection ?? localConn;
                return await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Return paged results from arbitrary select statement.
        /// </summary>
        /// <param name="columns">Column spec</param>
        /// <param name="tableNameOrJoinSpec">A table name, or a complete join specification (i.e. anything you can SELECT FROM in SQL)</param>
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
        /// TO DO: Possibly Possibly cancel the above, it makes no sense from a UI pov!
        /// </remarks>
        override public async Task<PagedResults<T>> PagedFromSelectAsync(
            string tableNameOrJoinSpec,
            string orderBy,
            string columns = null,
            string where = null,
            int pageSize = 20, int currentPage = 1,
            DbConnection connection = null,
            params object[] args)
        {
            return await PagedFromSelectAsync(tableNameOrJoinSpec, orderBy,
                CancellationToken.None,
                columns,
                where,
                pageSize, currentPage,
                connection,
                args: args);
        }

        /// <summary>
        /// Return paged results from arbitrary select statement.
        /// </summary>
        /// <param name="columns">Column spec</param>
        /// <param name="tableNameOrJoinSpec">A table name, or a complete join specification (i.e. anything you can SELECT FROM in SQL)</param>
        /// <param name="orderBy">ORDER BY clause</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="currentPage">Current page</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns>The result of the paged query. Result properties are Items, TotalPages, and TotalRecords.</returns>
        /// <remarks>
        /// In this one instance, because of the connection to the underlying logic of these queries, the user
        /// can pass "SELECT columns" instead of columns.
        /// TO DO: Possibly Possibly cancel the above, it makes no sense from a UI pov!
        /// </remarks>
        override public async Task<PagedResults<T>> PagedFromSelectAsync(
            string tableNameOrJoinSpec,
            string orderBy,
            CancellationToken cancellationToken,
            string columns = null,
            string where = null,
            int pageSize = 20, int currentPage = 1,
            DbConnection connection = null,
            params object[] args)
        {
            int limit = pageSize;
            int offset = (currentPage - 1) * pageSize;
            if (columns == null) columns = Columns;
            var pagingQueryPair = Plugin.BuildPagingQueryPair(columns, tableNameOrJoinSpec, orderBy, where, limit, offset);
            var result = new PagedResults<T>();
            result.TotalRecords = Convert.ToInt32(await ScalarAsync(pagingQueryPair.CountQuery, cancellationToken).ConfigureAwait(false));
            result.TotalPages = (result.TotalRecords + pageSize - 1) / pageSize;
            var items = await QueryAsync(pagingQueryPair.PagingQuery, cancellationToken).ConfigureAwait(false);
            result.Items = await items.ToListAsync(cancellationToken).ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Return all matching items.
        /// </summary>
        override public async Task<IAsyncEnumerable<T>> AllWithParamsAsync(
            string where = null, string orderBy = null, string columns = null, int limit = 0,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await AllWithParamsAsync(
                CancellationToken.None,
                where, orderBy, columns, limit,
                inParams, outParams, ioParams, returnParams,
                connection,
                args: args);
        }
        override public async Task<IAsyncEnumerable<T>> AllWithParamsAsync(
            CancellationToken cancellationToken,
            string where = null, string orderBy = null, string columns = null, int limit = 0,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            if (columns == null)
            {
                columns = Columns;
            }
            var sql = Plugin.BuildSelect(columns, CheckGetTableName(), where, orderBy, limit);
            return await QueryNWithParamsAsync<T>(sql,
                inParams, outParams, ioParams, returnParams,
                behavior: limit == 1 ? CommandBehavior.SingleRow : CommandBehavior.Default, connection: connection, cancellationToken: cancellationToken, args: args);
        }

#pragma warning disable CS1998
        /// <summary>
        /// Yield return values for single or multiple resultsets.
        /// </summary>
        /// <typeparam name="X">Use with <typeparamref name="T"/> for single or <see cref="IEnumerable{T}"/> for multiple</typeparam>
        /// <param name="command">The command to execute</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="behavior">The command behaviour</param>
        /// <param name="connection">Optional conneciton to use</param>
        /// <param name="outerReader">The outer reader when this is a call to the inner reader in QueryMultiple</param>
        /// <returns></returns>
        override protected async Task<IAsyncEnumerable<X>> QueryNWithParamsAsync<X>(DbCommand command, CancellationToken cancellationToken = default, CommandBehavior behavior = CommandBehavior.Default, DbConnection connection = null, DbDataReader outerReader = null)
        {
            return new AsyncEnumerable<X>(async yield => {
                using (command)
                {
                    if (behavior == CommandBehavior.Default && typeof(X) == typeof(T))
                    {
                        // (= single result set, not single row...)
                        behavior = CommandBehavior.SingleResult;
                    }
                    // using is applied only to locally generated connection
                    using (var localConn = (connection == null ? await OpenConnectionAsync(cancellationToken).ConfigureAwait(false) : null))
                    {
                        if (command != null)
                        {
                            command.Connection = connection ?? localConn;
                        }
                        // manage wrapping transaction if required, and if we have not been passed an incoming connection
                        // in which case assume user can/should manage it themselves
                        using (var trans = (connection == null
#if NETFRAMEWORK
                            // TransactionScope support
                            && Transaction.Current == null
#endif
                            && Plugin.RequiresWrappingTransaction(command) ? localConn.BeginTransaction() : null))
                        {
                            using (var reader = (outerReader == null ? await Plugin.ExecuteDereferencingReaderAsync(command, behavior, connection ?? localConn, cancellationToken).ConfigureAwait(false) : null))
                            {
                                if (typeof(X) == typeof(IAsyncEnumerable<T>))
                                {
                                    // query multiple pattern
                                    do
                                    {
                                        // cast is required because compiler doesn't see that we've just checked that X is IEnumerable<T>
                                        // first three params carefully chosen so as to avoid lots of checks about outerReader in the code above in this method
                                        var next = (X)(await QueryNWithParamsAsync<T>(null, cancellationToken, (CommandBehavior)(-1), connection ?? localConn, reader).ConfigureAwait(false));
                                        // yield.ReturnAsync does not take a cancellation token (it would have nothing to do
                                        // with it except pass it back to the caller who provided it in the first place, if it did)
                                        await yield.ReturnAsync(next).ConfigureAwait(false);
                                    }
                                    while (await reader.NextResultAsync(cancellationToken).ConfigureAwait(false));
                                }
                                else
                                {
                                    // Reasonably fast inner loop to yield-return objects of the required type from the DbDataReader.
                                    //
                                    // Used to be a separate function YieldReturnRows(), called here or within the loop above; but you can't do a yield return
                                    // for an outer function in an inner function (nor inside a delegate), so we're using recursion to avoid duplicating this
                                    // entire inner loop.
                                    //
                                    DbDataReader useReader = outerReader ?? reader;

                                    if (useReader.HasRows)
                                    {
                                        int fieldCount = useReader.FieldCount;
                                        object[] rowValues = new object[fieldCount];

                                        // this is for dynamic support
                                        string[] columnNames = null;
                                        // this is for generic<T> support
                                        PropertyInfo[] propertyInfo = null;

                                        if (UseExpando) columnNames = new string[fieldCount];
                                        else propertyInfo = new PropertyInfo[fieldCount];

                                        // for generic, we need array of properties to set; we find this
                                        // from fieldNames array, using a look up from lowered name -> property
                                        for (int i = 0; i < fieldCount; i++)
                                        {
                                            var columnName = useReader.GetName(i);
                                            if (string.IsNullOrEmpty(columnName))
                                            {
                                                throw new InvalidOperationException("Cannot autopopulate from anonymous column");
                                            }
                                            if (UseExpando)
                                            {
                                                // For dynamics, create fields using the case that comes back from the database
                                                // TO DO: Test how this is working now in Oracle
                                                columnNames[i] = columnName;
                                            }
                                            else
                                            {
                                                // leaves as null if no match
                                                columnNameToPropertyInfo.TryGetValue(columnName, out propertyInfo[i]);
                                            }
                                        }
                                        while (await useReader.ReadAsync(cancellationToken).ConfigureAwait(false))
                                        {
                                            useReader.GetValues(rowValues);
                                            if (UseExpando)
                                            {
                                                ExpandoObject e = new ExpandoObject();
                                                IDictionary<string, object> d = e.ToDictionary();
                                                for (int i = 0; i < fieldCount; i++)
                                                {
                                                    var v = rowValues[i];
                                                    d.Add(columnNames[i], v == DBNull.Value ? null : v);
                                                }
                                                await yield.ReturnAsync((X)(object)e).ConfigureAwait(false);
                                            }
                                            else
                                            {
                                                T t = new T();
                                                for (int i = 0; i < fieldCount; i++)
                                                {
                                                    var v = rowValues[i];
                                                    if (propertyInfo[i] != null)
                                                    {
                                                        propertyInfo[i].SetValue(t, v == DBNull.Value ? null : v.ChangeType(propertyInfo[i].PropertyType));
                                                    }
                                                }
                                                await yield.ReturnAsync((X)(object)t).ConfigureAwait(false);
                                            }
                                        }
                                    }
                                }
                            }
                            if (trans != null) trans.Commit();
                        }
                    }
                }
            });
        }
#pragma warning restore CS1998
        #endregion

        #region ORM actions
        /// <summary>
        /// Save, Insert, Update or Delete an item.
        /// Save means: update item if PK field or fields are present and at non-default values, insert otherwise.
        /// On inserting an item with a single PK and a sequence/identity 1) the PK of the new item is returned;
        /// 2) the PK field of the item itself is a) created if not present and b) filled with the new PK value,
        /// where this is possible (e.g. fields can't be created on POCOs, property values can't be set on immutable
        /// items such as anonymously typed objects).
        /// </summary>
        /// <param name="originalAction">Save, Insert, Update or Delete</param>
        /// <param name="item">item</param>
        /// <param name="connection">The connection to use</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns>The PK of the inserted item, iff a new auto-generated PK value is available.</returns>
        /// <remarks>
        /// It *is* technically possibly (by writing to private backing fields) to change the field value in anonymously
        /// typed objects - http://stackoverflow.com/a/30242237/795690 - and bizarrely VB supports writing to fields in
        /// anonymously typed objects natively even though C# doesn't - http://stackoverflow.com/a/9065678/795690 (which
        /// sounds as if it means that if this part of the library was written in VB then doing this would be officially
        /// supported? not quite sure, that assumes that the different implementations of anonymous types can co-exist)
        /// </remarks>
        private async Task<Tuple<int, object>> ActionOnItemAsync(OrmAction originalAction, object item, DbConnection connection,
            CancellationToken cancellationToken = default)
        {
            OrmAction revisedAction;
            DbCommand command = CreateActionCommand(originalAction, item, out revisedAction);
            command.Connection = connection;
            if (revisedAction == OrmAction.Insert && SequenceNameOrIdentityFunction != null)
            {
                // *All* DBs return a huge sized number for their identity by default, following Massive we are normalising to int
                var pk = Convert.ToInt32(await ScalarAsync(command, cancellationToken).ConfigureAwait(false));
                var result = UpsertItemPK(
                    item, pk,
                    // No point creating clone items on Save as these will then be discarded
                    originalAction == OrmAction.Insert
                    );
                return new Tuple<int, object>(1, result);
            }
            else
            {
                int n = await ExecuteAsync(command, cancellationToken).ConfigureAwait(false);
                return new Tuple<int, object>(n, null);
            }
        }
#endregion
    }
}
#endif