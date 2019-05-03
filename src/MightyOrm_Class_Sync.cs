using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Linq;
using System.Text;
#if NETFRAMEWORK
using System.Transactions;
#endif

using Mighty.ConnectionProviders;
using Mighty.DataContracts;
using Mighty.Interfaces;
using Mighty.Parameters;
using Mighty.Profiling;
using Mighty.Validation;

namespace Mighty
{
    public partial class MightyOrm<T> : MightyOrmAbstractInterface<T> where T : class, new()
    {
        // Only methods with a non-trivial implementation are here, the rest are in the MightyOrm_Redirects_Sync file.
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
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        /// <remarks>
        /// This only lets you pass in the aggregate expressions of your SQL variant, but SUM, AVG, MIN, MAX are supported on all.
        /// </remarks>
        /// <remarks>
        /// This is very close to a 'redirect' method, but couldn't have been in the abstract interface before because of the plugin access.
        /// </remarks>
        override public object AggregateWithParams(string function, string columns, string where = null,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            return ScalarWithParams(Plugin.BuildSelect(string.Format("{0}({1})", function, columns), CheckGetTableName(), where),
                inParams, outParams, ioParams, returnParams,
                connection, args);
        }

        /// <summary>
        /// Update all items matching WHERE clause using fields from the item sent in.
        /// If `keys` has been specified on the current Mighty instance then any primary key fields in the item are ignored.
        /// The item is not filtered to remove fields not in the table, if you need that you can call <see cref="New"/> with first parameter `partialItem` and second parameter `false` first.
        /// </summary>
        /// <param name="partialItem">Item containing values to update with</param>
        /// <param name="where">WHERE clause specifying which rows to update</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        override public int UpdateUsing(object partialItem, string where,
            DbConnection connection,
            params object[] args)
        {
            return UpdateUsingWithParams(partialItem, where,
                connection,
                null,
                args);
        }

        /// <summary>
        /// Update all items matching WHERE clause using fields from the item sent in.
        /// If `keys` has been specified on the current Mighty instance then any primary key fields in the item are ignored.
        /// The item is not filtered to remove fields not in the table, if you need that you can call <see cref="New"/> with first parameter `partialItem` and second parameter `false` first.
        /// </summary>
        /// <param name="partialItem">Item containing values to update with</param>
        /// <param name="where">WHERE clause specifying which rows to update</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="inParams">Named input parameters</param>
        /// <param name="args">Auto-numbered input parameters</param>
        protected int UpdateUsingWithParams(object partialItem, string where,
            DbConnection connection,
            object inParams,
            params object[] args)
        {
            var updateValues = new StringBuilder();
            var partialItemParameters = new NameValueTypeEnumerator(this, partialItem);
            // TO DO: Test that this combinedInputParams approach works
            var combinedInputParams = inParams?.ToExpando() ?? new ExpandoObject();
            var toDict = combinedInputParams.ToDictionary();
            int i = 0;
            foreach (var paramInfo in partialItemParameters)
            {
                if (!PrimaryKeys.IsKey(paramInfo.Name))
                {
                    if (i > 0) updateValues.Append(", ");
                    updateValues.Append(paramInfo.Name).Append(" = ").Append(Plugin.PrefixParameterName(paramInfo.Name));
                    i++;

                    toDict.Add(paramInfo.Name, paramInfo.Value);
                }
            }
            var sql = Plugin.BuildUpdate(CheckGetTableName(), updateValues.ToString(), where);
            var retval = ExecuteWithParams(sql, args: args, inParams: combinedInputParams, outParams: new { __rowcount = new RowCount() }, connection: connection);
            return retval.__rowcount;
        }

        /// <summary>
        /// Delete one or more items based on a WHERE clause.
        /// </summary>
        /// <param name="where">
        /// Non-optional WHERE clause.
        /// Specify "1=1" if you are sure that you want to delete all rows.
        /// </param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <param name="connection">The connection to use</param>
        /// <returns>The number of items affected</returns>
        override public int Delete(string where,
            DbConnection connection,
            params object[] args)
        {
            var sql = Plugin.BuildDelete(CheckGetTableName(), where);
            return Execute(sql, connection, args);
        }

        /// <summary>
        /// Perform CRUD action for the item(s) in the params list.
        /// An <see cref="IEnumerable{T}"/> of *modified* items is returned; the modification is to update the primary key to the correct new value for inserted items.
        /// If the input item does not support field writes/inserts as needed then an <see cref="ExpandoObject"/> corresponding to the updated item is returned instead.
        /// </summary>
        /// <param name="action">The ORM action</param>
        /// <param name="connection">The connection to use</param>
        /// <param name="items">The item or items</param>
        /// <returns>The list of modified items</returns>
        /// <remarks>Here and in <see cref="UpsertItemPK"/> we always return the modified original object where possible</remarks>
        internal IEnumerable<T> ActionOnItems(OrmAction action, DbConnection connection, IEnumerable<object> items)
        {
            return ActionOnItemsWithOutput(action, connection, items).Item2;
        }

        /// <summary>
        /// Perform CRUD action for the item(s) in the params list.
        /// An <see cref="IEnumerable{T}"/> of *modified* items is returned; the modification is to update the primary key to the correct new value for inserted items.
        /// If the input item does not support field writes/inserts as needed then an <see cref="ExpandoObject"/> corresponding to the updated item is returned instead.
        /// </summary>
        /// <param name="action">The ORM action</param>
        /// <param name="connection">The connection to use</param>
        /// <param name="items">The item or items</param>
        /// <returns>The list of modified items</returns>
        /// <remarks>Here and in <see cref="UpsertItemPK"/> we always return the modified original object where possible</remarks>
        internal Tuple<int, IEnumerable<T>> ActionOnItemsWithOutput(OrmAction action, DbConnection connection, IEnumerable<object> items)
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
                    object result;
                    affected += ActionOnItem(out result, action, item, connection);
                    if (action == OrmAction.Insert)
                    {
                        var modified = result ?? item;
                        if (IsGeneric && !(modified is T))
                        {
                            modified = New(modified, false);
                        }
                        modifiedItems.Add((T)modified);
                    }
                    Validator.HasPerformedAction(item, action);
                }
                count++;
            }
            return new Tuple<int, IEnumerable<T>>(affected, modifiedItems);
        }

#if KEY_VALUES
        /// <summary>
        /// Returns a string-string dictionary which can be directly bound to ASP.NET dropdowns etc. (see https://stackoverflow.com/a/805610/795690).
        /// </summary>
        /// <param name="orderBy">Order by, defaults to primary key</param>
        /// <returns></returns>
        override public IDictionary<string, string> KeyValues(string orderBy = null)
        {
            if (IsGeneric)
            {
                // TO DO: Make sure this works even when there is mapping
                var db = new MightyOrm(null, TableName, PrimaryKeys.FieldNames, ValueColumn, connectionProvider: new PresetsConnectionProvider(ConnectionString, Factory, Plugin.GetType()));
                return db.KeyValues(orderBy);
            }
            string partialMessage = $" to call {nameof(KeyValues)}, please provide one in your constructor";
            string valueColumn = CheckGetValueColumn(partialMessage);
            string pkColumn = PrimaryKeys.CheckGetKeyColumn(partialMessage);
            // this cast here casts the IEnumerable of ExpandoObject's to an IEnumerable of string-object dictionaries
            // (since each ExpandoObject can be cast like that)
            var results = All(orderBy: orderBy ?? pkColumn, columns: $"{pkColumn}, {valueColumn}").Cast<IDictionary<string, object>>();
            return results.ToDictionary(item => item[pkColumn].ToString(), item => item[valueColumn].ToString());
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
        override public DbConnection OpenConnection()
        {
            var connection = Factory.CreateConnection();
            connection = DataProfiler.ConnectionWrapping(connection);
            connection.ConnectionString = ConnectionString;
            connection.Open();
            return connection;
        }

        /// <summary>
        /// Execute database command.
        /// </summary>
        /// <param name="command">The command to execute</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns>The number of rows affected</returns>
        override public int Execute(DbCommand command,
            DbConnection connection = null)
        {
            // using applied only to local connection
            using (var localConn = ((connection == null) ? OpenConnection() : null))
            {
                command.Connection = connection ?? localConn;
                return command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Return scalar result (value of first or only column from first or only row) from database command.
        /// </summary>
        /// <param name="command">The command to execute</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        override public object Scalar(DbCommand command,
            DbConnection connection = null)
        {
            // using applied only to local connection
            using (var localConn = ((connection == null) ? OpenConnection() : null))
            {
                command.Connection = connection ?? localConn;
                return command.ExecuteScalar();
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
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns>The result of the paged query. Result properties are Items, TotalPages, and TotalRecords.</returns>
        /// <remarks>
        /// In this one instance, because of the connection to the underlying logic of these queries, the user
        /// can pass "SELECT columns" instead of columns.
        /// TO DO: Possibly cancel the above, it makes no sense from a UI pov!
        /// </remarks>
        override public PagedResults<T> PagedFromSelect(
            string tableNameOrJoinSpec,
            string orderBy,
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
            result.TotalRecords = Convert.ToInt32(Scalar(pagingQueryPair.CountQuery));
            result.TotalPages = (result.TotalRecords + pageSize - 1) / pageSize;
            result.Items = Query(pagingQueryPair.PagingQuery);
            return result;
        }

        /// <summary>
        /// Get <see cref="IEnumerable{T}"/> of items from the current table with WHERE and TOP/LIMIT specification and support for named parameters.
        /// </summary>
        /// <param name="where">WHERE clause</param>
        /// <param name="orderBy">ORDER BY clause</param>
        /// <param name="columns">Columns to return</param>
        /// <param name="limit">Maximum number of items to return</param>
        /// <param name="inParams">Named input parameters</param>
        /// <param name="outParams">Named output parameters</param>
        /// <param name="ioParams">Named input-output parameters</param>
        /// <param name="returnParams">Named return parameters</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        override public IEnumerable<T> AllWithParams(
            string where = null, string orderBy = null, string columns = null, int limit = 0,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            if (columns == null) columns = Columns;
            var sql = Plugin.BuildSelect(columns, CheckGetTableName(), where, orderBy, limit);
            return QueryNWithParams<T>(sql,
                inParams, outParams, ioParams, returnParams,
                behavior: limit == 1 ? CommandBehavior.SingleRow : CommandBehavior.Default, connection: connection, args: args);
        }

        /// <summary>
        /// Yield return values for single or multiple resultsets.
        /// </summary>
        /// <typeparam name="X">Use with <typeparamref name="T"/> for single or <see cref="IEnumerable{T}"/> for multiple</typeparam>
        /// <param name="command">The command to execute</param>
        /// <param name="behavior">The command behaviour</param>
        /// <param name="connection">Optional conneciton to use</param>
        /// <param name="outerReader">The outer reader when this is a call to the inner reader in QueryMultiple</param>
        /// <returns></returns>
        override protected IEnumerable<X> QueryNWithParams<X>(DbCommand command, CommandBehavior behavior = CommandBehavior.Default, DbConnection connection = null, DbDataReader outerReader = null)
        {
            using (command)
            {
                if (behavior == CommandBehavior.Default && typeof(X) == typeof(T))
                {
                    // (= single result set, not single row...)
                    behavior = CommandBehavior.SingleResult;
                }
                // using is applied only to locally generated connection
                using (var localConn = (connection == null ? OpenConnection() : null))
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
                        using (var reader = (outerReader == null ? Plugin.ExecuteDereferencingReader(command, behavior, connection ?? localConn) : null))
                        {
                            if (typeof(X) == typeof(IEnumerable<T>))
                            {
                                // query multiple pattern
                                do
                                {
                                    // cast is required because compiler doesn't see that we've just checked that X is IEnumerable<T>
                                    // first three params carefully chosen so as to avoid lots of checks about outerReader in the code above in this method
                                    yield return (X)QueryNWithParams<T>(null, (CommandBehavior)(-1), connection ?? localConn, reader);
                                }
                                while (reader.NextResult());
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
                                    ColumnsContractMemberInfo[] memberInfo = null;

                                    if (!IsGeneric) columnNames = new string[fieldCount];
                                    else memberInfo = new ColumnsContractMemberInfo[fieldCount];

                                    // for generic, we need array of properties to set; we find this
                                    // from fieldNames array, using a look up from lowered name -> property
                                    for (int i = 0; i < fieldCount; i++)
                                    {
                                        var columnName = useReader.GetName(i);
                                        if (string.IsNullOrEmpty(columnName))
                                        {
                                            throw new InvalidOperationException("Cannot autopopulate from anonymous column");
                                        }
                                        if (!IsGeneric)
                                        {
                                            // For dynamics, create fields using the case that comes back from the database
                                            // TO DO: Test how this is working now in Oracle
                                            columnNames[i] = columnName;
                                        }
                                        else
                                        {
                                            // leaves as null if no match
                                            ColumnsContract.TryGetDataMemberInfo(columnName, out memberInfo[i], DataDirection.Read);
                                        }
                                    }
                                    while (useReader.Read())
                                    {
                                        useReader.GetValues(rowValues);
                                        if (!IsGeneric)
                                        {
                                            ExpandoObject e = new ExpandoObject();
                                            IDictionary<string, object> d = e.ToDictionary();
                                            for (int i = 0; i < fieldCount; i++)
                                            {
                                                var v = rowValues[i];
                                                d.Add(columnNames[i], v == DBNull.Value ? null : v);
                                            }
                                            yield return (X)(object)e;
                                        }
                                        else
                                        {
                                            T t = new T();
                                            for (int i = 0; i < fieldCount; i++)
                                            {
                                                var v = rowValues[i];
                                                memberInfo[i]?.SetValue(t, v == DBNull.Value ? null : v);
                                            }
                                            yield return (X)(object)t;
                                        }
                                    }
                                }
                            }
                        }
                        if (trans != null) trans.Commit();
                    }
                }
            }
        }
        #endregion

        #region ORM actions
        /// <summary>
        /// Save, Insert, Update or Delete an item.
        /// Save means: update item if PK field or fields are present and at non-default values, insert otherwise.
        /// On inserting an item with a single PK and a sequence/identity the PK field of the item itself is
        /// a) created if not present and b) filled with the new PK value, where this is possible (examples of cases
        /// where not possible are: fields can't be created on POCOs, property values can't be set on immutable items
        /// such as anonymously typed objects).
        /// </summary>
        /// <param name="modified">The modified item with PK added, if <see cref="OrmAction.Insert"/></param>
        /// <param name="originalAction">Save, Insert, Update or Delete</param>
        /// <param name="item">item</param>
        /// <param name="connection">The connection to use</param>
        /// <returns>The number of items affected</returns>
        /// <remarks>
        /// It *is* technically possibly (by writing to private backing fields) to change the field value in anonymously
        /// typed objects - http://stackoverflow.com/a/30242237/795690 - and bizarrely VB supports writing to fields in
        /// anonymously typed objects natively even though C# doesn't - http://stackoverflow.com/a/9065678/795690 (which
        /// sounds as if it means that if this part of the library was written in VB then doing this would be officially
        /// supported? not quite sure, that assumes that the different implementations of anonymous types can co-exist)
        /// </remarks>
        private int ActionOnItem(out object modified, OrmAction originalAction, object item, DbConnection connection)
        {
            OrmAction revisedAction;
            DbCommand command = CreateActionCommand(originalAction, item, out revisedAction);
            command.Connection = connection;
            if (revisedAction == OrmAction.Insert && PrimaryKeys.SequenceNameOrIdentityFunction != null)
            {
                // *All* DBs return a huge sized number for their identity by default, following Massive we are normalising to int
                var pk = Convert.ToInt32(Scalar(command));
                modified = UpsertItemPK(
                    item, pk,
                    // Don't create clone items on Save as these will then be discarded; but do still update the PK if clone not required
                    originalAction == OrmAction.Insert);
                return 1;
            }
            else
            {
                modified = null;
                return Execute(command);
            }
        }
        #endregion
    }
}