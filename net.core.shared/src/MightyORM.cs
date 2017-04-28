using System;
using System.Collections.Generic;
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
using Mighty.DatabasePlugins;
using Mighty.Interfaces;
using Mighty.Mapping;
using Mighty.Parameters;
using Mighty.Validation;

namespace Mighty
{
	// In order to support generics, the nice ;) dynamic version now a sub-class of the generic version
	// (though of course it is really a super class)
	public class MightyORM : MightyORM<dynamic>
	{
		// ctor - disallow, or just ignore?, sequence spec when we have multiple PKs
		public MightyORM(string connectionStringOrName = null,
						 string table = null,
						 string primaryKey = null,
						 string sequence = null,
						 string columns = null,
						 Validator validator = null,
						 SqlNamingMapper mapper = null,
						 ConnectionProvider connectionProvider = null)
		{
			if (mapper == null)
			{
				mapper = new SqlNamingMapper();
			}
			string tableClassName = null;
			if (table != null)
			{
				TableName = table;
			}
			else
			{
				// Class-based table name override for dynamic MightyORM
				
				var me = this.GetType();
				// leave table name unset if we are not a true sub-class;
				// this test enforces strict sub-class (i.e. does not pass for an instance of the class itself)
#if NETFRAMEWORK
				if (me.IsSubclassOf(typeof(MightyORM)))
#else
				if (me.GetTypeInfo().IsSubclassOf(typeof(MightyORM)))
#endif
				{
					tableClassName = me.Name;
					TableName = mapper.GetTableName(tableClassName);
				}
			}
			Init(connectionStringOrName, primaryKey, sequence, columns, validator, mapper, connectionProvider, tableClassName);
		}

#region Convenience factory
		// mini-factory for non-table specific access
		// (equivalent to a constructor call)
		// <remarks>static, so can't be defined anywhere but here</remarks>
		new static public MightyORM DB(string connectionStringOrName = null)
		{
			return new MightyORM(connectionStringOrName);
		}
#endregion
	}

	public class MightyORM<T> : MicroORM<T>, IPluginCallback where T: new()
	{
		// Only properties with a non-trivial implementation are here, the rest are in the MicroORM abstract class.
#region Properties
		protected IEnumerable<dynamic> _TableInfo;
		override public IEnumerable<dynamic> TableInfo
		{
			// TO DO: Might as well lock-initialize properly
			get
			{
				if (_TableInfo == null)
				{
					string tableName = TableName;
					string owner = null;
					int i = tableName.LastIndexOf('.');
					if (i >= 0)
					{
						owner = tableName.Substring(0, i);
						tableName = tableName.Substring(i + 1);
					}
					var sql = _plugin.BuildTableInfoQuery(owner, tableName);
					_TableInfo = _plugin.NormalizeTableInfo(Query(sql));
				}
				return _TableInfo;
			}
		}

		protected Dictionary<string, PropertyInfo> columnNameToPropertyInfo;
#endregion

#region Constructor
		/// <summary>
		/// <see cref="MightyORM"> constructor.
		/// </summary>
		/// <param name="connectionStringOrName">Connection string with support for additional, non-standard
		/// "ProviderName=" property</param>
		/// <param name="primaryKey">Primary key field name; or comma separated list of names for compound PK</param>
		/// <param name="sequence">Optional sequence name for PK inserts on sequence-based DBs; optionally override
		/// identity retrieval function for identity-based DBs (e.g. specify "@@IDENTITY" for SQL CE); as a special case
		/// send an empty string (i.e. not the default value of null) to turn off identity support on identity-based DBs.</param>
		/// <param name="columns">Default column list</param>
		/// <param name="validator">Optional validator</param>
		/// <param name="mapper">Optional C# &lt;-&gt; SQL name mapper</param>
		/// <param name="connectionProvider">Optional connection provider (only needed for providers not yet known to MightyORM)</param>
		/// <param name="propertyBindingFlags">Specify which properties should be managed by the ORM</param>
		public MightyORM(string connectionStringOrName = null,
						 string primaryKey = null,
						 string sequence = null,
						 string columns = null,
						 Validator validator = null,
						 SqlNamingMapper mapper = null,
						 ConnectionProvider connectionProvider = null,
						 BindingFlags propertyBindingFlags = BindingFlags.Instance | BindingFlags.Public)
		{
			if (mapper == null)
			{
				mapper = new SqlNamingMapper();
			}

			// Table name for MightyORM<T>
			string tableClassName = typeof(T).Name;
			TableName = mapper.GetTableName(tableClassName);

			Init(connectionStringOrName, primaryKey, sequence, columns, validator, mapper, connectionProvider, tableClassName);

			columnNameToPropertyInfo = new Dictionary<string, PropertyInfo>();
			foreach (var info in typeof(T).GetProperties(propertyBindingFlags))
			{
				var columnName = mapper.GetColumnName(tableClassName, info.Name);
				if (mapper.UseCaseInsensitiveMapping)
				{
					columnName = columnName.ToLowerInvariant();
				}
				columnNameToPropertyInfo.Add(columnName, info);
			}
		}
#endregion

#region Shared initialiser
		// sequence is for sequence-based databases (Oracle, PostgreSQL) - there is no default sequence, specify either null or empty string to disable and manually specify your PK values;
		// for non-sequence-based databases, in unusual cases, you may specify this to specify an alternative key retrieval function
		// (e.g. for example to use @@IDENTITY instead of SCOPE_IDENTITY(), in the case of SQL Server CE)
		// primaryKeyFields is a comma separated list; if it has more than one column, you cannot specify sequence or keyRetrievalFunction
		// (if neither sequence nor keyRetrievalFunction are set (which is always the case for compound primary keys), you MUST specify non-null, non-default values for every column in your primary key
		// before saving an object)
		public void Init(string connectionStringOrName,
						 string primaryKey,
						 string sequence,
						 string columns,
						 Validator validator,
						 SqlNamingMapper mapper,
						 ConnectionProvider connectionProvider,
						 string tableClassName)
		{
			if (connectionProvider == null)
			{
#if NETFRAMEWORK
				connectionProvider = new ConfigFileConnectionProvider().Init(connectionStringOrName);
				if (connectionProvider.ConnectionString == null)
#endif
				{
					connectionProvider = new PureConnectionStringProvider()
#if NETFRAMEWORK
						.UsedAfterConfigFile()
#endif
						.Init(connectionStringOrName);
				}
			}
			else
			{
				connectionProvider.Init(connectionStringOrName);
			}

			ConnectionString = connectionProvider.ConnectionString;
			Factory = connectionProvider.ProviderFactoryInstance;
			Type pluginType = connectionProvider.DatabasePluginType;
			_plugin = (DatabasePlugin)Activator.CreateInstance(pluginType, false);
			_plugin.mighty = (IPluginCallback)this;

			if (primaryKey == null && tableClassName != null)
			{
				primaryKey = mapper.GetPrimaryKeyName(tableClassName);
			}
			PrimaryKeyFields = primaryKey;
			if (primaryKey != null)
			{
				PrimaryKeyList = primaryKey.Split(',').Select(k => k.Trim()).ToList();
			}
			DefaultColumns = columns ?? "*";
			Validator = validator;
			Mapper = mapper;
			// After all this, SequenceNameOrIdentityFn is only non-null if we really are expecting to use it
			// (which entails exactly one PK)
			if (!_plugin.IsSequenceBased)
			{
				if (PrimaryKeyList.Count != 1)
				{
					SequenceNameOrIdentityFn = null;
				}
				else
				{
					// empty string on identity-based DB specifies that PK is manually controlled
					if (sequence == "") SequenceNameOrIdentityFn = null;
					// other non-null value overrides default identity retrieval fn (e.g. use "@@IDENTITY" on SQL CE)
					else if (sequence != null) SequenceNameOrIdentityFn = sequence;
					// default fn
					else SequenceNameOrIdentityFn = _plugin.IdentityRetrievalFunction;
				}
			}
			else
			{
				// NB on identity-based DBs using an identity on the PK is the default mode of operation (i.e. unless
				// empty string is specified in 'sequence'; or unless there is > 1 primary key), whereas on sequence-based
				// DBs NOT having a sequence is the default (i.e. unless a specific sequence is passed in).
				if (PrimaryKeyList.Count != 1)
				{
					throw new InvalidOperationException("Sequence may only be specified for tables with a single primary key");
				}
				SequenceNameOrIdentityFn = mapper.QuoteDatabaseName(sequence);
			}
		}
#endregion

#region Convenience factory
		// mini-factory for non-table specific access
		// (equivalent to a constructor call)
		// <remarks>static, so can't be defined anywhere but here</remarks>
		static public MightyORM<T> DB(string connectionStringOrName = null)
		{
			return new MightyORM<T>(connectionStringOrName);
		}
#endregion

		// Only methods with a non-trivial implementation are here, the rest are in the MicroORM abstract class.
#region MircoORM interace
		// In theory COUNT expression could vary across SQL variants, in practice it doesn't.
		override public object Count(string columns = "*", string where = null,
			DbConnection connection = null,
			params object[] args)
		{
			var expression = string.Format("COUNT({0})", columns);
			return Aggregate(expression, where, connection, args);
		}

		// This just lets you pass in the aggregate expressions of your SQL variant, but SUM, AVG, MIN, MAX are supported on all.
		override public object Aggregate(string expression, string where = null,
			DbConnection connection = null,
			params object[] args)
		{
			return ScalarWithParams(_plugin.BuildSelect(expression, CheckTableName(), where),
				connection: connection, args: args);
		}

		// You do NOT have to use this - you can create new items to pass into the microORM more or less however you want.
		// The main convenience provided here is to automatically strip out any input which does not match your column names.
		// TO DO: This is slightly dodgy because it does not get the values from the DB itself - it is possible that with the
		// correct select we can get the DB to send us the values.
		override public T NewFrom(object nameValues = null, bool addNonPresentAsDefaults = true)
		{
			var item = new ExpandoObject();
			var newItemDictionary = item.AsDictionary();
			var parameters = new NameValueTypeEnumerator(nameValues);
			// drive the loop by the actual column names
			foreach (var columnInfo in TableInfo)
			{
				string columnName = columnInfo.COLUMN_NAME;
				object userValue = null;
				foreach (var paramInfo in parameters)
				{
					if (paramInfo.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase))
					{
						userValue = paramInfo.Value;
						break;
					}
				}
				if (userValue != null)
				{
					newItemDictionary.Add(columnName, userValue);
				}
				else if (addNonPresentAsDefaults)
				{
					newItemDictionary.Add(columnName, GetColumnDefault(columnName));
				}
			}
			// ********** TO DO **********
			return default(T); //(T)item;
		}

		// Update from fields in the item sent in. If PK has been specified, any primary key fields in the
		// item are ignored (this is an update, not an insert!). However the item is not filtered to remove fields
		// not in the table. If you need that, call <see cref="NewFrom"/>(<see cref="partialItem"/>, false) first.
		override public int UpdateUsing(object partialItem, string where,
			DbConnection connection,
			params object[] args)
		{
			var values = new StringBuilder();
			var parameters = new NameValueTypeEnumerator(partialItem);
			var filteredItem = new ExpandoObject();
			var toDict = filteredItem.AsDictionary();
			int i = 0;
			foreach (var paramInfo in parameters)
			{
				if (!IsKey(paramInfo.Name))
				{
					if (i > 0) values.Append(", ");
					values.Append(paramInfo.Name).Append(" = ").Append(_plugin.PrefixParameterName(paramInfo.Name));
					i++;

					toDict.Add(paramInfo.Name, paramInfo.Value);
				}
			}
			var sql = _plugin.BuildUpdate(CheckTableName(), values.ToString(), where);
			return ExecuteWithParams(sql, args: args, inParams: filteredItem, connection: connection);
		}

		override public int DeleteByKey(DbConnection connection, params object[] keys)
		{
			int sum = 0;
			foreach (var key in keys)
			{
				var sql = _plugin.BuildDelete(CheckTableName(), WhereForKeys());
				sum += Execute(sql, key);
			}
			return sum;
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
			var sql = _plugin.BuildDelete(CheckTableName(), where);
			return Execute(sql, connection, args);
		}

		/// <summary>
		/// Get the meta-data for a single column
		/// </summary>
		/// <param name="column">Column name</param>
		/// <param name="ExceptionOnAbsent">If true throw an exception if there is no such column, otherwise return null.</param>
		/// <returns></returns>
		override public dynamic GetColumnInfo(string column, bool ExceptionOnAbsent = true)
		{
			var info = TableInfo.Select(c => column.Equals(c.COLUMN_NAME, StringComparison.OrdinalIgnoreCase));
			if (ExceptionOnAbsent && info == null)
			{
				throw new InvalidOperationException("Cannot find table info for column name " + column);
			}
			return column;
		}

		/// <summary>
		/// Get the default value for a column.
		/// </summary>
		/// <param name="columnName"></param>
		/// <returns></returns>
		/// <remarks>
		/// Although it might look more efficient, GetColumnDefault should not do buffering, as we don't
		/// want to pass out the same actual object more than once.
		/// TO DO: Should this actually be used for checking whether PKs are at their default values?
		/// I would say probably not.
		/// </remarks>
		override public object GetColumnDefault(string columnName)
		{
			var columnInfo = GetColumnInfo(columnName);
			return _plugin.GetColumnDefault(columnInfo);
		}

		/// <summary>
		/// Return array of key values from passed in key values.
		/// Raise exception if the wrong number of keys are provided.
		/// The wrapping of a single item into an array which this does would happen automatically anyway
		/// in C# params handling, so this code is only required for the exception checking.
		/// </summary>
		/// <param name="key">The key value or values</param>
		/// <returns></returns>
		override protected object[] KeyValuesFromKey(object key)
		{
			if (key == null) throw new ArgumentNullException(nameof(key));
			var okey = key as object[];
			if (okey == null) okey = new object[] { key };
			if (okey.Length != PrimaryKeyList.Count)
			{
				throw new InvalidOperationException(okey.Length + " key values provided, " + PrimaryKeyList.Count + "expected");
			}
			return okey;
		}

		private string _whereForKeys;

		/// <summary>
		/// Return a WHERE clause with auto-named parameters for the primary keys
		/// </summary>
		/// <returns></returns>
		override protected string WhereForKeys()
		{
			if (_whereForKeys == null)
			{
				if (PrimaryKeyList == null || PrimaryKeyList.Count == 0)
				{
					throw new InvalidOperationException("No primary key field(s) have been specified");
				}
				var sb = new StringBuilder();
				int i = 0;
				foreach (var keyName in PrimaryKeyList)
				{
					if (i > 0) sb.Append(" AND ");
					sb.Append(keyName).Append(" = ").Append(_plugin.PrefixParameterName(i++.ToString()));
				}
				_whereForKeys = sb.ToString();
			}
			return _whereForKeys;
		}

		/// <summary>
		/// Return comma-separated list of primary key fields, raising an exception if there are none.
		/// </summary>
		/// <returns></returns>
		override protected string CheckPrimaryKeyFields()
		{
			if (string.IsNullOrEmpty(PrimaryKeyFields))
			{
					throw new InvalidOperationException("No primary key field(s) have been specified");
			}
			return PrimaryKeyFields;
		}

		/// <summary>
		/// Return current table name, raising an exception if there isn't one.
		/// </summary>
		/// <returns></returns>
		override protected string CheckTableName()
		{
			if (string.IsNullOrEmpty(TableName))
			{
				throw new InvalidOperationException("No table name has been specified");
			}
			return TableName;
		}

		// In new version, null or default value for type in PK will save as new, as well as no PK field
		// only we don't know what the pk type is... but we do after getting the schema, and we should just use = to compare without worrying too much about types
		// is checking whether every item is valid before any saving - which is good - and not the same as checking
		// something at inserting/updating time; still if we're going to use a transaction ANYWAY, and this does.... hmmm... no: rollback is EXPENSIVE
		// returns the sum of the number of rows affected;
		// *** insert WILL set the PK field, as long as the object was an expando in the first place (could upgrade that; to set PK
		// in Expando OR in settable property of correct name)
		// *** we can assume that it is NEVER valid for the user to specify the PK value manually - though they can of course specify the pkFieldName,
		// and the pkSequence, for those databases which work that way; I strongly suspect we should be able to shove the sequence select into ONE round
		// trip to the DB, as well.
		// (Of course, this would mean that there would be no such thing as an ORM provided update, for a table without a PK. You know what? It *is* valid to
		// set - and update based on - a compound PK. Which means it must BE valid to set a non-compound PK.)
		// I think we want primaryKeySequence (for dbs which use that; defaults to no sequence) and primaryKeyRetrievalFunction (for dbs which use that; defaults to
		// correct default to DB, but may be set to null). If both are null, you can still have a (potentially compound) PK.
		// We can use INSERT seqname.nextval and then SELECT seqname.currval in Oracle.
		// And INSERT nextval('seqname') and then currval('seqname') (or just lastval()) in PostgreSQL.
		// (if neither primaryKeySequence nor primaryKeyRetrievalFunction are set (which is always the case for compound primary keys), you MUST specify non-null, non-default values for every column in your primary key
		// before saving an object)
		// *** okay, shite, how do we know if a compound key object is an insert or an update? I think we just provide Save, which is auto, but can't work for manual primary keys,
		// and Insert and Update, which will do what they say on the tin, and which can.

		// Save cannot be used with manually controlled primary keys (which includes compound primary keys), as the microORM cannot tell apart an insert from an update in this case
		// but I think this can just be an exception, as we really don't need to worry most users about it.
		// exception can check whether we are compound; or whether we may be sequence, but just not set; or whether we have retrieval fn intentionally overridden to empty string;
		// and give different messages.

		/// <summary>
		/// Perform CRUD action for the item or items in the params list.
		/// For insert only, the PK of the first item is returned.
		/// For all others, the number of items affected is returned.
		/// </summary>
		/// <param name="action">The ORM action</param>
		/// <param name="connection">The DbConnection</param>
		/// <param name="items">The item or items</param>
		/// <returns></returns>
		override internal object ActionOnItems(ORMAction action, DbConnection connection, params object[] items)
		{
			object pk = null;
			int count = 0;
			int affected = 0;
			if (Validator != null) Validator.PrevalidateAllActions(action, items);
			foreach (var item in items)
			{
				if (Validator == null || Validator.PerformingAction(action, item))
				{
					var _pk = ActionOnItem(action, item, connection);
					if (count == 0)
					{
						pk = _pk;
					}
					if (Validator != null) Validator.PerformedAction(action, item);
					affected++;
				}
				count++;
			}
			if (action == ORMAction.Insert) return pk;
			else return affected;
		}
#endregion

		// Only methods with a non-trivial implementation are here, the rest are in the DataAccessWrapper abstract class.
#region DataAccessWrapper interface
		/// <summary>
		/// Creates a new DbConnection. You do not normally need to call this! (MightyORM normally manages its own
		/// connections. Create a connection here and pass it on to other MightyORM commands only in non-standard
		/// cases where you need to explicitly manage transactions or share connections, e.g. when using explicit
		/// cursors).
		/// </summary>
		/// <returns></returns>
		override public DbConnection OpenConnection()
		{
			var connection = Factory.CreateConnection();
			if (connection != null)
			{
				connection.ConnectionString = ConnectionString;
				connection.Open();
			}
			return connection;
		}

		/// <summary>
		/// Execute DbCommand
		/// </summary>
		/// <param name="command">The command</param>
		/// <param name="connection">Optional DbConnection to use</param>
		/// <returns></returns>
		override public dynamic Execute(DbCommand command,
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
		/// <param name="columns">The SELECT columns</param>
		/// <param name="tablesAndJoins">The FROM tables and joins</param>
		/// <param name="orderBy">The ORDER BY clause</param>
		/// <param name="where">The WHERE clause</param>
		/// <param name="pageSize">Page size</param>
		/// <param name="currentPage">Current page</param>
		/// <param name="connection">Optional DbConnection to use</param>
		/// <param name="args">Optional parameters to the SQL</param>
		/// <returns></returns>
		override public dynamic PagedFromSelect(string columns, string tablesAndJoins, string orderBy, string where = null,
			int pageSize = 20, int currentPage = 1,
			DbConnection connection = null,
			params object[] args)
		{
			int limit = pageSize;
			int offset = (currentPage - 1) * pageSize;
			var sql = _plugin.BuildPagingQuery(columns, tablesAndJoins, orderBy, where, limit, offset);
			var resultSets = QueryMultiple(sql);
			dynamic result = new ExpandoObject();
			result.TotalCount = resultSets.First().First().TotalCount;
			result.Items = resultSets.Last();
			return result;
		}

		/// <summary>
		/// Create command, setting any provider specific features which we assume elsewhere.
		/// </summary>
		/// <param name="sql">The command text</param>
		/// <returns></returns>
		internal DbCommand CreateCommand(string sql)
		{
			var command = Factory.CreateCommand();
			_plugin.SetProviderSpecificCommandProperties(command);
			command.CommandText = sql;
			return command;
		}

		/// <summary>
		/// Create command with named, typed, directional parameters.
		/// </summary>
		override public DbCommand CreateCommandWithParams(string sql,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null, bool isProcedure = false,
			params object[] args)
		{
			var command = CreateCommand(sql);
			if (isProcedure) command.CommandType = CommandType.StoredProcedure;
			AddParams(command, args);
			AddNamedParams(command, inParams, ParameterDirection.Input);
			AddNamedParams(command, outParams, ParameterDirection.Output);
			AddNamedParams(command, ioParams, ParameterDirection.InputOutput);
			AddNamedParams(command, returnParams, ParameterDirection.ReturnValue);
			return command;
		}

		/// <summary>
		/// Put all output and return parameter values into an expando.
		/// Due to ADO.NET limitations, should only be called after disposing of any associated reader.
		/// </summary>
		/// <param name="cmd">The command</param>
		/// <returns></returns>
		override public dynamic ResultsAsExpando(DbCommand cmd)
		{
			dynamic e = new ExpandoObject();
			var resultDictionary = e.AsDictionary();
			for (int i = 0; i < cmd.Parameters.Count; i++)
			{
				var param = cmd.Parameters[i];
				if (param.Direction != ParameterDirection.Input)
				{
					var name = _plugin.DeprefixParameterName(param.ParameterName, cmd);
					var value = _plugin.GetValue(param);
					resultDictionary.Add(name, value == DBNull.Value ? null : value);
				}
			}
			return e;
		}

		/// <summary>
		/// Return all matching items.
		/// </summary>
		/// <remarks>TO DO(?): May require LIMIT (although I think this was really mainly for Single support on Massive)</remarks>
		override public IEnumerable<T> AllWithParams(
			string where = null, string orderBy = null, string columns = null,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			CommandBehavior behavior = CommandBehavior.Default,
			DbConnection connection = null,
			params object[] args)
		{
			if (columns == null)
			{
				columns = DefaultColumns;
			}
			var sql = _plugin.BuildSelect(columns, CheckTableName(), where, orderBy);
			return QueryNWithParams<T>(sql,
				inParams, outParams, ioParams, returnParams,
				behavior: behavior, connection: connection, args: args);
		}

		/// <summary>
		/// Yield return values for Query or QueryMultiple.
		/// Use with &lt;T&gt; for single or &lt;IEnumberable&lt;T&gt;&gt; for multiple.
		/// </summary>
		override protected IEnumerable<X> QueryNWithParams<X>(string sql = null, object inParams = null, object outParams = null, object ioParams = null, object returnParams = null, bool isProcedure = false, DbCommand command = null, CommandBehavior behavior = CommandBehavior.Default, DbConnection connection = null, params object[] args)
		{
			if (behavior == CommandBehavior.Default && typeof(X) == typeof(T))
			{
				behavior = CommandBehavior.SingleResult;
			}
			// using applied only to local connection
			using (var localConn = (connection == null ? OpenConnection() : null))
			{
				if (command != null)
				{
					command.Connection = connection ?? localConn;
				}
				else
				{
					command = CreateCommandWithParams(sql, inParams, outParams, ioParams, returnParams, isProcedure, connection ?? localConn, args);
				}
				// manage wrapping transaction if required, and if we have not been passed an incoming connection
				// in which case assume user can/should manage it themselves
				using (var trans = ((connection == null
#if NETFRAMEWORK
					// TransactionScope support
					&& Transaction.Current == null
#endif
					&& _plugin.RequiresWrappingTransaction(command)) ? localConn.BeginTransaction() : null))
				{
					using (var rdr = _plugin.ExecuteDereferencingReader(command, behavior, connection ?? localConn))
					{
						if (typeof(X) == typeof(IEnumerable<T>))
						{
							// query multiple pattern
							do
							{
								// cast is required because compiler doesn't see that we've just checked this!
								yield return (X)YieldReturnRows(rdr);
							}
							while (rdr.NextResult());
						}
						else
						{
							YieldReturnRows(rdr);
						}
					}
					if (trans != null) trans.Commit();
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
		/// <returns>The PK of the inserted item, iff a new auto-generated PK value is available.</returns>
		/// <remarks>
		/// It *is* technically possibly (by writing to private backing fields) to change the field value in anonymously
		/// typed objects - http://stackoverflow.com/a/30242237/795690 - and bizarrely VB supports writing to fields in
		/// anonymously typed objects natively even though C# doesn't - http://stackoverflow.com/a/9065678/795690 (which
		/// sounds as if it means that if this part of the library was written in VB then doing this would be officially
		/// supported? not quite sure, that assumes that the different implementations of anonymous types can co-exist)
		/// </remarks>
		private object ActionOnItem(ORMAction action, object item, DbConnection connection)
		{
			int nKeys = 0;
			int nDefaultKeyValues = 0;
			// TO DO(?): Only create and append to these lists conditional upon potential need
			List<string> insertNames = new List<string>();
			List<string> insertValues = new List<string>(); // list of param names, not actual values
			List<string> updateNameValuePairs = new List<string>();
			List<string> whereNameValuePairs = new List<string>();
			var count = 0;
			foreach (var nvt in new NameValueTypeEnumerator(item))
			{
				var name = nvt.Name;
				var value = nvt.Value;
				var prefixedName = _plugin.PrefixParameterName(name);
				if (IsKey(name))
				{
					nKeys++;					
					if (value == null || value == nvt.Type.GetDefaultValue())
					{
						nDefaultKeyValues++;
					}

					if (SequenceNameOrIdentityFn == null)
					{
						insertNames.Add(name);
						insertValues.Add(prefixedName);
					}
					else
					{
						if (_plugin.IsSequenceBased)
						{
							insertNames.Add(name);
							insertValues.Add(string.Format(_plugin.BuildNextval(SequenceNameOrIdentityFn)));
						}
					}

					whereNameValuePairs.Add(name);
					whereNameValuePairs.Add(" = ");
					whereNameValuePairs.Add(prefixedName);
				}
				else
				{
					insertNames.Add(name);
					insertValues.Add(prefixedName);

					updateNameValuePairs.Add(name);
					updateNameValuePairs.Add(" = ");
					updateNameValuePairs.Add(prefixedName);
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
			if (action == ORMAction.Save)
			{
				if (nKeys > 0 && nDefaultKeyValues == 0)
				{
					action = ORMAction.Update;
				}
				else
				{
					action = ORMAction.Insert;
				}
			}
			switch (action)
			{
				case ORMAction.Update:
					command = CreateUpdateCommand(item, updateNameValuePairs, whereNameValuePairs);
					break;
					
				case ORMAction.Insert:
					// TO DO: Hang on, we've got a different check here from SequenceNameOrIdentityFn != null;
					// either one or other is right, or else some exceptions should be thrown if they come apart.
					command = CreateInsertCommand(item, insertNames, insertValues, nDefaultKeyValues > 0 ? PkFilter.NoKeys : PkFilter.DoNotFilter);
					break;
					
				case ORMAction.Delete:
					command = CreateDeleteCommand(item, whereNameValuePairs);
					break;
					
				default:
					throw new InvalidOperationException("Internal error, incorrect " + nameof(ORMAction) + "=" + action + " at action choice in " + nameof(ActionOnItem));
			}
			command.Connection = connection;
			if (action == ORMAction.Insert && SequenceNameOrIdentityFn != null)
			{
				var pk = command.ExecuteScalar();
				WriteNewPKToItem(item, pk);
				return pk;
			}
			else
			{
				int n = command.ExecuteNonQuery();
				// should this be checked? is it reasonable for this to be zero sometimes?
				if (n != 1)
				{
					throw new InvalidOperationException("Could not " + action + " item");
				}
				return null;
			}
		}

		/// <summary>
		/// Create update command
		/// </summary>
		/// <param name="item"></param>
		/// <param name="updateNameValuePairs"></param>
		/// <param name="whereNameValuePairs"></param>
		/// <returns></returns>
		private DbCommand CreateUpdateCommand(object item, List<string> updateNameValuePairs, List<string> whereNameValuePairs)
		{
			string sql = _plugin.BuildUpdate(TableName, string.Join(", ", updateNameValuePairs), string.Join(" AND ", whereNameValuePairs));
			return CreateCommandWithParams(sql, inParams: item);
		}

		/// <summary>
		/// Create insert command
		/// </summary>
		/// <param name="item"></param>
		/// <param name="insertNames"></param>
		/// <param name="insertValues"></param>
		/// <param name="pkFilter"></param>
		/// <returns></returns>
		private DbCommand CreateInsertCommand(object item, List<string> insertNames, List<string> insertValues, PkFilter pkFilter)
		{
			string sql = _plugin.BuildInsert(TableName, string.Join(", ", insertNames), string.Join(", ", insertValues));
			if (SequenceNameOrIdentityFn != null)
			{
				sql += ";\r\n" +
					   "SELECT " +
					   (_plugin.IsSequenceBased ? _plugin.BuildCurrval(SequenceNameOrIdentityFn) : SequenceNameOrIdentityFn) +
					   _plugin.FromNoTable() + ";";
			}
			var command = CreateCommand(sql);
			AddNamedParams(command, item, pkFilter: pkFilter);
			return command;
		}

		/// <summary>
		/// Create delete command
		/// </summary>
		/// <param name="item"></param>
		/// <param name="whereNameValuePairs"></param>
		/// <returns></returns>
		private DbCommand CreateDeleteCommand(object item, List<string> whereNameValuePairs)
		{
			string sql = _plugin.BuildDelete(TableName,string.Join(" AND ", whereNameValuePairs));
			var command = CreateCommand(sql);
			AddNamedParams(command, item, pkFilter: PkFilter.KeysOnly);
			return command;
		}

		/// <summary>
		/// Write new PK value into item.
		/// The PK field is a) created if not present and b) filled with the new PK value, where this is possible
		/// (e.g. fields can't be created on POCOs, and property values can't be set on immutable items such as
		/// anonymously typed objects).
		/// </summary>
		/// <param name="item">The item to modify</param>
		/// <param name="pk">The PK value (PK may be int or long depending on the current database)</param>
		private void WriteNewPKToItem(object item, object pk)
		{
		}

		/// <summary>
		/// Is the string passed in the name of a PK field?
		/// </summary>
		/// <param name="fieldName">The name to check</param>
		/// <returns></returns>
		internal bool IsKey(string fieldName)
		{
			return PrimaryKeyList.Any(key => key.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
		}
#endregion

#region Parameters
		/// <summary>
		/// Add a parameter to a command
		/// </summary>
		/// <param name="cmd">The command</param>
		/// <param name="value">The value</param>
		/// <param name="name">Optional parameter name</param>
		/// <param name="direction">Optional parameter direction</param>
		/// <param name="type">Optional parameter type (for typed NULL support)</param>
		internal void AddParam(DbCommand cmd, object value, string name = null, ParameterDirection direction = ParameterDirection.Input, Type type = null)
		{
			var p = cmd.CreateParameter();
			if (name == string.Empty)
			{
				if (!_plugin.SetAnonymousParameter(p))
				{
					throw new InvalidOperationException("Current ADO.NET provider does not support anonymous parameters");
				}
			}
			else
			{
				p.ParameterName = _plugin.PrefixParameterName(name ?? cmd.Parameters.Count.ToString(), cmd);
			}
			_plugin.SetDirection(p, direction);
			if (value == null)
			{
				if (type != null)
				{
					_plugin.SetValue(p, type.GetDefaultValue());
					// explicitly lock type and size to the values which ADO.NET has just implicitly assigned
					// (when only implictly assigned, setting Value to DBNull.Value later on causes these to reset, in at least the Npgsql and SQL Server providers)
					p.DbType = p.DbType;
					p.Size = p.Size;
				}
				// Some ADO.NET providers completely ignore the parameter DbType when deciding on the .NET type for return values, others do not
				else if(direction != ParameterDirection.Input && !_plugin.IgnoresOutputTypes(p))
				{
					throw new InvalidOperationException("Parameter \"" + p.ParameterName + "\" - on this ADO.NET provider all output, input-output and return parameters require non-null value or fully typed property, to allow correct SQL parameter type to be inferred");
				}
				p.Value = DBNull.Value;
			}
			else
			{
				var cursor = value as Cursor;
				if (cursor != null)
				{
					// Placeholder cursor ref; we only need the value if passing in a cursor by value
					// doesn't work on Postgres.
					if (!_plugin.SetCursor(p, cursor.Value))
					{
						throw new InvalidOperationException("ADO.NET provider does not support cursors");
					}
				}
				else
				{
					// Note - the passed in parameter value can be a real cursor ref, this works - at least in Oracle
					_plugin.SetValue(p, value);
				}
			}
			cmd.Parameters.Add(p);
		}

		/// <summary>
		/// Add auto-named parameters from an array of parameter values (normally would have been passed in to microORM
		/// using C# params syntax)
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="args"></param>
		internal void AddParams(DbCommand cmd, params object[] args)
		{
			if (args == null)
			{
				return;
			}
			foreach (var value in args)
			{
				AddParam(cmd, value);
			}
		}

		/// <summary>
		/// Optional control whether to add only or no PKs when created parameters from object.
		/// </summary>
		internal enum PkFilter
		{
			DoNotFilter,
			KeysOnly,
			NoKeys
		}

		/// <summary>
		/// Add named, typed directional params to DbCommand.
		/// </summary>
		/// <param name="cmd">The command</param>
		/// <param name="nameValuePairs">Parameters to add (POCO, anonymous type, NameValueCollection, ExpandoObject, etc.) </param>
		/// <param name="direction">Parameter direction</param>
		/// <param name="pkFilter">Optional PK filter control</param>
		internal void AddNamedParams(DbCommand cmd, object nameValuePairs, ParameterDirection direction = ParameterDirection.Input, PkFilter pkFilter = PkFilter.DoNotFilter)
		{
			if (nameValuePairs == null)
			{
				return;
			}
			foreach (var paramInfo in new NameValueTypeEnumerator(nameValuePairs))
			{
				if (pkFilter == PkFilter.DoNotFilter || (IsKey(paramInfo.Name) == (pkFilter == PkFilter.KeysOnly)))
				{
					AddParam(cmd, paramInfo.Value, paramInfo.Name, direction, paramInfo.Type);
				}
			}
		}
#endregion

#region DbDataReader
		/// <summary>
		/// Reasonably fast inner loop to yield-return objects of the required type from the DbDataReader.
		/// </summary>
		/// <param name="reader">The reader</param>
		/// <returns></returns>
		virtual internal IEnumerable<T> YieldReturnRows(DbDataReader reader)
		{
			if (reader.Read())
			{
				bool useExpando = (typeof(T) == typeof(object));

				int fieldCount = reader.FieldCount;
				object[] rowValues = new object[fieldCount];

				// this is for dynamic support
				string[] columnNames = null;
				// this is for generic<T> support
				PropertyInfo[] propertyInfo = null;

				if (useExpando) columnNames = new string[fieldCount];
				else propertyInfo = new PropertyInfo[fieldCount];

				// for generic, we need array of properties to set; we find this
				// from fieldNames array, using a look up from lowered name -> property
				for (int i = 0; i < fieldCount; i++)
				{
					var columnName = reader.GetName(i);
					if (useExpando)
					{
						// For dynamics, create fields using the case that comes back from the database
						// TO DO: Test how this is working now in Oracle
						columnNames[i] = columnName;
					}
					else
					{
						if (Mapper.UseCaseInsensitiveMapping)
						{
							columnName = columnName.ToLowerInvariant();
						}
						propertyInfo[i] = columnNameToPropertyInfo[columnName];
					}
				}
				do
				{
					reader.GetValues(rowValues);
					if (useExpando)
					{
						dynamic e = new ExpandoObject();
						IDictionary<string, object> d = e.AsDictionary();
						for (int i = 0; i < fieldCount; i++)
						{
							var v = rowValues[i];
							d.Add(columnNames[i], v == DBNull.Value ? null : v);
						}
						yield return e;
					}
					else
					{
						T t = new T();
						for (int i = 0; i < fieldCount; i++)
						{
							var v = rowValues[i];
							propertyInfo[i].SetValue(t, v == DBNull.Value ? null : v, null);
						}
						yield return t;
					}
				} while (reader.Read());
			}
		}

		/// <summary>
		/// Will be needed for async support.
		/// Keep this in sync with the method above.
		/// </summary>
		/// <param name="reader"></param>
		/// <returns></returns>
		virtual internal IEnumerable<T> ReturnRows(DbDataReader reader)
		{
			throw new NotImplementedException();
		}
#endregion
	}
}