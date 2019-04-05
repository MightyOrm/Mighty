using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
#if NETFRAMEWORK
using System.Transactions;
#endif

using Mighty.ConnectionProviders;
using Mighty.Plugins;
using Mighty.Mocking;
using Mighty.Mapping;
using Mighty.Parameters;
using Mighty.Profiling;
using Mighty.Validation;

namespace Mighty
{
	public partial class MightyOrm<T> : MightyOrmMockable<T> where T : class, new()
	{
		// Only methods with a non-trivial implementation are here, the rest are in the MicroOrm abstract class.
		#region MircoORM interface
		// In theory COUNT expression could vary across SQL variants, in practice it doesn't.
		override public object Count(string columns = "*", string where = null,
			DbConnection connection = null,
			params object[] args)
		{
			var expression = string.Format("COUNT({0})", columns);
			return AggregateWithParams(expression, where, connection, args: args);
		}

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
		/// <remarks>
		/// This only lets you pass in the aggregate expressions of your SQL variant, but SUM, AVG, MIN, MAX are supported on all.
		/// </remarks>
		/// <remarks>
		/// This is very close to a 'redirect' method, but couldn't have been in the abstract interface before because of the plugin access.
		/// </remarks>
		override public object AggregateWithParams(string expression, string where = null,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args)
		{
			return ScalarWithParams(Plugin.BuildSelect(expression, CheckGetTableName(), where),
				inParams, outParams, ioParams, returnParams,
				connection, args);
		}

		/// <summary>
		/// Update from fields in the item sent in. If PK has been specified, any primary key fields in the
		/// item are ignored (this is an update, not an insert!). However the item is not filtered to remove fields
		/// not in the table. If you need that, call <see cref="NewFrom"/>(<see cref="partialItem"/>, false) first.
		/// </summary>
		/// <param name="partialItem"></param>
		/// <param name="where"></param>
		/// <param name="connection"></param>
		/// <param name="args"></param>
		override public int UpdateUsing(object partialItem, string where,
			DbConnection connection,
			params object[] args)
		{
			var values = new StringBuilder();
			var parameters = new NameValueTypeEnumerator(partialItem);
			var filteredItem = new ExpandoObject();
			var toDict = filteredItem.ToDictionary();
			int i = 0;
			foreach (var paramInfo in parameters)
			{
				if (!IsKey(paramInfo.Name))
				{
					if (i > 0) values.Append(", ");
					values.Append(paramInfo.Name).Append(" = ").Append(Plugin.PrefixParameterName(paramInfo.Name));
					i++;

					toDict.Add(paramInfo.Name, paramInfo.Value);
				}
			}
			var sql = Plugin.BuildUpdate(CheckGetTableName(), values.ToString(), where);
			return ExecuteWithParams(sql, args: args, inParams: filteredItem, connection: connection);
		}

		/// <summary>
		/// Delete rows from ORM table based on WHERE clause.
		/// </summary>
		/// <param name="where">
		/// Non-optional where clause.
		/// Specify "1=1" if you are sure that you want to delete all rows.</param>
		/// <param name="connection">The DbConnection to use</param>
		/// <param name="args">Optional auto-named parameters for the WHERE clause</param>
		/// <returns></returns>
		override public int Delete(string where,
			DbConnection connection,
			params object[] args)
		{
			var sql = Plugin.BuildDelete(CheckGetTableName(), where);
			return Execute(sql, connection, args);
		}

		/// <summary>
		/// Perform CRUD action for the item or items in the params list.
		/// For insert only, the PK of the first item is returned.
		/// For all others, the number of items affected is returned.
		/// </summary>
		/// <param name="action">The ORM action</param>
		/// <param name="connection">The DbConnection</param>
		/// <param name="items">The item or items</param>
		/// <returns></returns>
		/// <remarks>Here and in <see cref="UpsertItemPK"/> we always return the modified original object, where possible</remarks>
		internal int ActionOnItems(OrmAction action, DbConnection connection, IEnumerable<object> items, out T insertedItem)
		{
			insertedItem = null;
			int count = 0;
			int affected = 0;
			Prevalidate(items, action);
			foreach (var item in items)
			{
				if (Validator.PerformingAction(item, action))
				{
					var _inserted = ActionOnItem(action, item, connection, count);
					if (count == 0 && _inserted != null && action == OrmAction.Insert)
					{
						if (!UseExpando)
						{
							var resultT = _inserted as T;
							if (resultT == null)
							{
								resultT = NewFrom(_inserted, false);
							}
							_inserted = resultT;
						}
						insertedItem = (T)_inserted;
					}
					Validator.PerformedAction(item, action);
					affected++;
				}
				count++;
			}
			return affected;
		}

		/// <summary>
		/// Returns a string/string dictionary which can be bound directly to dropdowns etc http://stackoverflow.com/q/805595/
		/// </summary>
		override public IDictionary<string, string> KeyValues(string orderBy = "")
		{
			string foo = string.Format(" to call {0}, please provide one in your constructor", nameof(KeyValues));
			string valueField = CheckGetValueColumn(string.Format("ValueField is required{0}", foo));
			string primaryKeyField = CheckGetKeyName(string.Format("A single primary key must be specified{0}", foo));
			var results = All(orderBy: orderBy, columns: string.Format("{0}, {1}", primaryKeyField, valueField)).Cast<IDictionary<string, object>>();
			return results.ToDictionary(item => item[primaryKeyField].ToString(), item => item[valueField].ToString());
		}
		#endregion

		// Only methods with a non-trivial implementation are here, the rest are in the DataAccessWrapper abstract class.
		#region DataAccessWrapper interface
		/// <summary>
		/// Creates a new DbConnection. You do not normally need to call this! (MightyOrm normally manages its own
		/// connections. Create a connection here and pass it on to other MightyOrm commands only in non-standard
		/// cases where you need to explicitly manage transactions or share connections, e.g. when using explicit
		/// cursors).
		/// </summary>
		/// <returns></returns>
		override public DbConnection OpenConnection()
		{
			var connection = Factory.CreateConnection();
			connection = SqlProfiler.Wrap(connection);
			connection.ConnectionString = ConnectionString;
			connection.Open();
			return connection;
		}

		/// <summary>
		/// Execute DbCommand
		/// </summary>
		/// <param name="command">The command</param>
		/// <param name="connection">Optional DbConnection to use</param>
		/// <returns></returns>
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
		/// Return scalar from DbCommand
		/// </summary>
		/// <param name="command">The command</param>
		/// <param name="connection">Optional DbConnection to use</param>
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
		/// <param name="tablesAndJoins">Single table name, or join specification</param>
		/// <param name="where">Optional</param>
		/// <param name="orderBy">Required</param>
		/// <param name="pageSize"></param>
		/// <param name="currentPage"></param>
		/// <param name="connection"></param>
		/// <param name="args"></param>
		/// <returns>The result of the paged query. Result properties are Items, TotalPages, and TotalRecords.</returns>
		/// <remarks>
		/// In this one instance, because of the connection to the underlying logic of these queries, the user
		/// can pass "SELECT columns" instead of columns.
		/// TO DO: Cancel the above, it makes no sense from a UI pov!
		/// </remarks>
		override public PagedResults<T> PagedFromSelect(string columns, string tablesAndJoins, string where, string orderBy,
			int pageSize = 20, int currentPage = 1,
			DbConnection connection = null,
			params object[] args)
		{
			int limit = pageSize;
			int offset = (currentPage - 1) * pageSize;
			if (columns == null) columns = Columns;
			var pagingQueryPair = Plugin.BuildPagingQueryPair(columns, tablesAndJoins, where, orderBy, limit, offset);
			var result = new PagedResults<T>();
			result.TotalRecords = Convert.ToInt32(Scalar(pagingQueryPair.CountQuery));
			result.TotalPages = (result.TotalRecords + pageSize - 1) / pageSize;
			result.Items = Query(pagingQueryPair.PagingQuery);
			return result;
		}

		/// <summary>
		/// Return all matching items.
		/// </summary>
		override public IEnumerable<T> AllWithParams(
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
			return QueryNWithParams<T>(sql,
				inParams, outParams, ioParams, returnParams,
				behavior: limit == 1 ? CommandBehavior.SingleRow : CommandBehavior.Default, connection: connection, args: args);
		}

		/// <summary>
		/// Yield return values for Query or QueryMultiple.
		/// Use with &lt;T&gt; for single or &lt;IEnumerable&lt;T&gt;&gt; for multiple.
		/// </summary>
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
                                    while (useReader.Read())
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
                                            yield return (X)(object)e;
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
		/// On inserting an item with a single PK and a sequence/identity 1) the PK of the new item is returned;
		/// 2) the PK field of the item itself is a) created if not present and b) filled with the new PK value,
		/// where this is possible (e.g. fields can't be created on POCOs, property values can't be set on immutable
		/// items such as anonymously typed objects).
		/// </summary>
		/// <param name="action">Save, Insert, Update or Delete</param>
		/// <param name="item">item</param>
		/// <param name="connection">connection to use</param>
		/// <param name="outerCount">when zero we are on the first item in the loop</param>
		/// <returns>The PK of the inserted item, iff a new auto-generated PK value is available.</returns>
		/// <remarks>
		/// It *is* technically possibly (by writing to private backing fields) to change the field value in anonymously
		/// typed objects - http://stackoverflow.com/a/30242237/795690 - and bizarrely VB supports writing to fields in
		/// anonymously typed objects natively even though C# doesn't - http://stackoverflow.com/a/9065678/795690 (which
		/// sounds as if it means that if this part of the library was written in VB then doing this would be officially
		/// supported? not quite sure, that assumes that the different implementations of anonymous types can co-exist)
		/// </remarks>
		private object ActionOnItem(OrmAction action, object item, DbConnection connection, int outerCount)
		{
			int nKeys = 0;
			int nDefaultKeyValues = 0;
			// TO DO(?): Only create and append to these lists conditional upon potential need
			List<string> insertNames = new List<string>();
			List<string> insertValues = new List<string>(); // list of param names, not actual values
			List<string> updateNameValuePairs = new List<string>();
			List<string> whereNameValuePairs = new List<string>();
			var argsItem = new ExpandoObject();
			var argsItemDict = argsItem.ToDictionary();
			var count = 0;
			foreach (var nvt in new NameValueTypeEnumerator(item, action: action))
			{
				var name = nvt.Name;
				if (name == string.Empty)
				{
					name = CheckGetKeyName(count, "Too many values trying to map value-only object to primary key list");
				}
				var value = nvt.Value;
				string paramName;
				if (value == null)
				{
					// Sending NULL in the SQL and not in a param is required to support obscure cases (such as the SQL Server IMAGE type)
					// where the column refuses to cast from the default VARCHAR NULL param which is created when a parameter is null.
					paramName = "NULL";
				}
				else
				{
					paramName = Plugin.PrefixParameterName(name);
					argsItemDict.Add(name, value);
				}
				if (nvt.Name == null || IsKey(name))
				{
					nKeys++;
					if (value == null || value == nvt.Type.GetDefaultValue())
					{
						nDefaultKeyValues++;
					}

					if (SequenceNameOrIdentityFn == null)
					{
						insertNames.Add(name);
						insertValues.Add(paramName);
					}
					else
					{
						if (Plugin.IsSequenceBased)
						{
							insertNames.Add(name);
							insertValues.Add(string.Format(Plugin.BuildNextval(SequenceNameOrIdentityFn)));
						}
					}

					whereNameValuePairs.Add(string.Format("{0} = {1}", name, paramName));
				}
				else
				{
					insertNames.Add(name);
					insertValues.Add(paramName);

					updateNameValuePairs.Add(string.Format("{0} = {1}", name, paramName));
				}
				count++;
			}
			if (nKeys > 0)
			{
				if (nKeys != this.PrimaryKeyList.Count)
				{
					throw new InvalidOperationException("All or no primary key fields must be present in item for " + action);
				}
				if (nDefaultKeyValues > 0 && nDefaultKeyValues != nKeys)
				{
					throw new InvalidOperationException("All or no primary key fields must start with their default values in item for " + action);
				}
			}
			DbCommand command = null;
			OrmAction originalAction = action;
			if (action == OrmAction.Save)
			{
				if (nKeys > 0 && nDefaultKeyValues == 0)
				{
					action = OrmAction.Update;
				}
				else
				{
					action = OrmAction.Insert;
				}
			}
			switch (action)
			{
				case OrmAction.Update:
					command = CreateUpdateCommand(argsItem, updateNameValuePairs, whereNameValuePairs);
					break;

				case OrmAction.Insert:
					if (SequenceNameOrIdentityFn != null && Plugin.IsSequenceBased)
					{
						// our copy of SequenceNameOrIdentityFn is only ever non-null when there is a non-compound PK
						insertNames.Add(PrimaryKeyFields);
						// TO DO: Should there be two places for BuildNextval? (See above.) Why?
						insertValues.Add(Plugin.BuildNextval(SequenceNameOrIdentityFn));
					}
					// TO DO: Hang on, we've got a different check here from SequenceNameOrIdentityFn != null;
					// either one or other is right, or else some exceptions should be thrown if they come apart.
					command = CreateInsertCommand(argsItem, insertNames, insertValues, nDefaultKeyValues > 0 ? PkFilter.NoKeys : PkFilter.DoNotFilter);
					break;

				case OrmAction.Delete:
					command = CreateDeleteCommand(argsItem, whereNameValuePairs);
					break;

				default:
					// use 'Exception' for strictly internal/should not happen/our fault exceptions
					throw new Exception("incorrect " + nameof(OrmAction) + "=" + action + " at action choice in " + nameof(ActionOnItem));
			}
			command.Connection = connection;
			if (action == OrmAction.Insert && SequenceNameOrIdentityFn != null)
			{
				// *All* DBs return a huge sized number for their identity by default, following Massive we are normalising to int
				var pk = Convert.ToInt32(Scalar(command));
				var result = UpsertItemPK(item, pk, originalAction == OrmAction.Insert && outerCount == 0);
				return result;
			}
			else
			{
				int n = Execute(command);
				// should this be checked? is it reasonable for this to be zero sometimes?
				if (n != 1)
				{
					throw new InvalidOperationException("Could not " + action + " item");
				}
				return null;
			}
		}
		#endregion
	}
}