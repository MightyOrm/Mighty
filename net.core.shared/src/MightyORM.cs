using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;

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
		public MightyORM(string connectionStringOrName = null,
						 string table = null,
						 string primaryKey = null,
						 string sequence = null,
						 string columns = null,
						 Validator validator = null,
						 Mapper mapper = null,
						 ConnectionProvider connectionProvider = null)
		{
			if (mapper == null)
			{
				mapper = new Mapper();
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
				if (me.GetTypeInfo().IsSubclassOf(typeof(MightyORM)))
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
		// sequence is for sequence-based databases (Oracle, PostgreSQL) - there is no default, specify either null or empty string to disable and manually specify your PK values;
		// keyRetrievalFunction is for non-sequence based databases (MySQL, SQL Server, SQLite) - defaults to default for DB, specify empty string to disable and manually specify your PK values;
		// primaryKeyFields is a comma separated list; if it has more than one column, you cannot specify sequence or keyRetrievalFunction
		// (if neither sequence nor keyRetrievalFunction are set (which is always the case for compound primary keys), you MUST specify non-null, non-default values for every column in your primary key
		// before saving an object)
		public MightyORM(string connectionStringOrName = null,
						 string primaryKey = null,
						 string sequence = null,
						 string columns = null,
						 Validator validator = null,
						 Mapper mapper = null,
						 ConnectionProvider connectionProvider = null,
						 BindingFlags propertyBindingFlags = BindingFlags.Instance | BindingFlags.Public)
		{
			if (mapper == null)
			{
				mapper = new Mapper();
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
						 Mapper mapper,
						 ConnectionProvider connectionProvider,
						 string tableClassName)
		{
			if (connectionProvider == null)
			{
#if !true//COREFX
				connectionProvider = new ConfigFileConnectionProvider().Init(connectionStringOrName);
				if (connectionProvider.ConnectionString == null)
#endif
				{
					connectionProvider = new PureConnectionStringProvider()
#if !true//COREFX
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
		override public T NewFrom(object nameValues = null, bool addNonPresentAsDefaults = true)
		{
			var item = new ExpandoObject();
			var newItemDictionary = item.AsDictionary();
			var parameters = new ParamEnumerator(nameValues);
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
		override public int UpdateFrom(object partialItem, string where,
			DbConnection connection,
			params object[] args)
		{
			var values = new StringBuilder();
			var parameters = new ParamEnumerator(partialItem);
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

		override public int Delete(string where,
			DbConnection connection,
			params object[] args)
		{
			var sql = _plugin.BuildDelete(CheckTableName(), where);
			return Execute(sql, connection, args);
		}

		override public dynamic GetColumnInfo(string column, bool ExceptionOnAbsent = true)
		{
			var info = TableInfo.Select(c => column.Equals(c.COLUMN_NAME, StringComparison.OrdinalIgnoreCase));
			if (ExceptionOnAbsent && info == null)
			{
				throw new InvalidOperationException("Cannot find table info for column name " + column);
			}
			return column;
		}
	
		// We can implement NewItem() and GetColumnDefault()
		// NB *VERY* useful for better PK handling; GetColumnDefault needs to do buffering - actually, it doesn't because
		// otherwise we may end up passing the very same object out twice
		override public object GetColumnDefault(string columnName)
		{
			var columnInfo = GetColumnInfo(columnName);
			return _plugin.GetColumnDefault(columnInfo);
		}

		// This is not required, in that passing a single key via args will do this anyway.
		// It's just for exception checking.
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

		override protected string CheckPrimaryKeyFields()
		{
			if (string.IsNullOrEmpty(PrimaryKeyFields))
			{
					throw new InvalidOperationException("No primary key field(s) have been specified");
			}
			return PrimaryKeyFields;
		}

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
		override internal int Action(ORMAction action, DbConnection connection, params object[] items)
		{
			int sum = 0;
			if (Validator != null) Validator.PrevalidateActions(action, items);
			foreach (var item in items)
			{
				if (Validator == null || Validator.PerformingAction(action, item))
				{
					switch (action)
					{
						case ORMAction.Delete:
							sum += Delete(item, connection);
							break;

						default:
							sum += SaveItem(action, item, connection);
							break;
					}
					//throw new NotImplementedException();

					if (Validator != null) Validator.PerformedAction(action, item);
				}
			}
			return sum;
		}
#endregion

		// Only methods with a non-trivial implementation are here, the rest are in the DataAccessWrapper abstract class.
#region DataAccessWrapper interface
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

		override public DbCommand CreateCommandWithParams(string sql,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null, bool isProcedure = false,
			params object[] args)
		{
			var command = Factory.CreateCommand();
			_plugin.SetProviderSpecificCommandProperties(command);
			command.CommandText = sql;
			if (isProcedure) command.CommandType = CommandType.StoredProcedure;
			AddParams(command, args);
			AddNamedParams(command, inParams, ParameterDirection.Input);
			AddNamedParams(command, outParams, ParameterDirection.Output);
			AddNamedParams(command, ioParams, ParameterDirection.InputOutput);
			AddNamedParams(command, returnParams, ParameterDirection.ReturnValue);
			return command;
		}

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

		// Call with either T for single or IEnumberable<T> for multiple
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
#if !true//COREFX
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
		private int Delete(object item, DbConnection connection)
		{
			throw new NotImplementedException();
		}

		private int SaveItem(ORMAction action, object item, DbConnection connection)
		{
			if (!CheckHasKeys(item) || CheckHasNullOrDefaultKeys(item))
			{
				return InsertItem(item, connection);
			}
			else
			{
				return UpdateItem(item, connection);
			}
		}

		private int InsertItem(object item, DbConnection connection)
		{
			throw new NotImplementedException();
		}

		private int UpdateItem(object item, DbConnection connection)
		{
			throw new NotImplementedException();
		}

		private bool CheckHasKeys(object item)
		{
			throw new NotImplementedException();
		}

		private bool CheckHasNullOrDefaultKeys(object item)
		{
			throw new NotImplementedException();
		}

		internal bool IsKey(string fieldName)
		{
			return PrimaryKeyList.Any(key => key.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
		}
#endregion

#region Parameters
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
					_plugin.SetValue(p, type.CreateInstance());
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

		// add auto-named parameters from an array of items (presumably passed in to microORM using C# params)
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

		internal void AddNamedParams(DbCommand cmd, object nameValuePairs, ParameterDirection direction = ParameterDirection.Input)
		{
			if (nameValuePairs == null)
			{
				return;
			}
			foreach (var paramInfo in new ParamEnumerator(nameValuePairs))
			{
				AddParam(cmd, paramInfo.Value, paramInfo.Name, direction, paramInfo.Type);
			}
		}
#endregion

#region DbDataReader
		// will need to keep this in sync with the unbuffered version below (once we are implementing both)
		virtual internal IEnumerable<T> YieldReturnRows(DbDataReader reader)
		{
			if (reader.Read())
			{
				bool useExpando = (typeof(T) == typeof(object));

				int fieldCount = reader.FieldCount;
				object[] rowValues = new object[fieldCount];

				// this is for expando
				string[] columnNames = null;
				// this is for generic
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
						// use the case that comes back from the database
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
							propertyInfo[i].SetValue(t, v == DBNull.Value ? null : v);
						}
						yield return t;
					}
				} while (reader.Read());
			}
		}
		
		// (will be needed for async support)
		// keep this in sync with the method above
		virtual internal IEnumerable<T> ReturnRows(DbDataReader reader)
		{
			throw new NotImplementedException();
		}
#endregion
	}
}