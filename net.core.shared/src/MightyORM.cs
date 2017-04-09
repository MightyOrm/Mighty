using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;

using Mighty.ConnectionProviders;
using Mighty.DatabasePlugins;
using Mighty.Interfaces;
using Mighty.Validation;

namespace Mighty
{
	public partial class MightyORM : MicroORM
	{
#region Constructors
		// sequence is for sequence-based databases (Oracle, PostgreSQL) - there is no default, specify either null or empty string to disable and manually specify your PK values;
		// keyRetrievalFunction is for non-sequence based databases (MySQL, SQL Server, SQLite) - defaults to default for DB, specify empty string to disable and manually specify your PK values;
		// primaryKeyFields is a comma separated list; if it has more than one column, you cannot specify sequence or keyRetrievalFunction
		// (if neither sequence nor keyRetrievalFunction are set (which is always the case for compound primary keys), you MUST specify non-null, non-default values for every column in your primary key
		// before saving an object)
		public MightyORM(string connectionStringOrName = null,
						 string table = null, string primaryKey = null,
						 string sequence = null,
						 string columns = null,
						 Validator validator = null,
						 ConnectionProvider connectionProvider = null)
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
			_plugin.mighty = this;

			if (table != null)
			{
				TableName = table;
			}
			else
			{
				var me = this.GetType();
				// leave table name unset if we are not a true sub-class;
				// test enforces strict sub-class, does not pass for an instance of the class itself
				if (me.GetTypeInfo().IsSubclassOf(typeof(MightyORM)))
				{
					TableName = CreateTableNameFromClassName(me.Name);
				}
			}
			PrimaryKeyFields = primaryKey;
			PrimaryKeyList = primaryKey.Split(',').Select(k => k.Trim()).ToList();
			DefaultColumns = columns ?? "*";
			_validator = validator;
		}
#endregion

#region Convenience factory
		// mini-factory for non-table specific access
		// (equivalent to a constructor call)
		static public MightyORM DB(string connectionStringOrName = null)
		{
			return new MightyORM(connectionStringOrName);
		}
#endregion

		// All the elements of this interface which are purely defined in terms of other elements are implemented
		// in the abstract class, not here.
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

		// You do NOT have to use this - you can create new items to pass in the microORM more or less however you want.
		// The main convenience provided here is to automatically strip out any input which does not match your column names.
		override public dynamic NewFrom(object nameValues = null, bool addNonPresentAsDefaults = true)
		{
			var item = new ExpandoObject();
			var newItemDictionary = item.AsDictionary();
			var userDictionary = nameValues.ToExpando().AsDictionary();
			// drive the loop by the actual column names
			foreach (var columnInfo in TableInfo)
			{
				string columnName = columnInfo.COLUMN_NAME;
				object userValue = null;
				foreach (var pair in userDictionary)
				{
					if (pair.Key.Equals(columnName, StringComparison.OrdinalIgnoreCase))
					{
						userValue = pair.Value;
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
			return item;
		}

		// Update from fields in the item sent in. If PK has been specified, any primary key fields in the
		// item are ignored (this is an update, not an insert!). However the item is not filtered to remove fields
		// not in the table. If you need that, call <see cref="NewFrom"/>(<see cref="partialItem"/>, false) first.
		override public int UpdateFrom(object partialItem, string where,
			DbConnection connection,
			params object[] args)
		{
			var values = new StringBuilder();
			var userDictionary = partialItem.ToExpando().AsDictionary();
			var filteredItem = new ExpandoObject();
			var toDict = filteredItem.AsDictionary();
			int i = 0;
			foreach (var pair in userDictionary)
			{
				if (!IsKey(pair.Key))
				{
					if (i > 0) values.Append(", ");
					values.Append(pair.Key).Append(" = ").Append(_plugin.PrefixParameterName(pair.Key));
					i++;

					toDict.Add(pair.Key, pair.Value);
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

		override public bool IsKey(string fieldName)
		{
			return PrimaryKeyList.Any(key => key.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
		}

		override public dynamic ColumnInfo(string column, bool ExceptionOnAbsent = true)
		{
			var info = TableInfo.Select(c => column.Equals(c.COLUMN_NAME, StringComparison.OrdinalIgnoreCase));
			if (ExceptionOnAbsent && info == null)
			{
				throw new InvalidOperationException("Cannot find table info for column name " + column);
			}
			return column;
		}
	
		// We can implement NewItem() and GetColumnDefault()
		// NB *VERY* useful for better PK handling; GetColumnDefault needs to do buffering - actually, it doesn't because we may end up passing the very same object out twice
		override public dynamic GetColumnDefault(string columnName)
		{
			var columnInfo = ColumnInfo(columnName);
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

		// Cannot be used with manually controlled primary keys (which includes compound primary keys), as the microORM cannot tell apart an insert from an update in this case
		// but I think this can just be an exception, as we really don't need to worry most users about it.
		// exception can check whether we are compound; or whether we may be sequence, but just not set; or whether we have retrieval fn intentionally overridden to empty string;
		// and give different messages.
		override public int Action(ORMAction action, DbConnection connection, params object[] items)
		{
			int sum = 0;
			if (_validator != null) _validator.PrevalidateActions(action, items);
			foreach (var item in items)
			{
				if (_validator == null || _validator.PerformingAction(action, item))
				{
					//throw new NotImplementedException();

					if (_validator != null) _validator.PerformedAction(action, item);
				}
			}
			return sum;
		}
#endregion

		// All the elements of this interface which are purely defined in terms of other elements are implemented
		// in the abstract class, not here.
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

		override protected IEnumerable<dynamic> AllWithParams(
			CommandBehavior behavior,
			string where = null, string orderBy = null, string columns = null,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args)
		{
			if (columns == null)
			{
				columns = DefaultColumns;
			}
			var sql = _plugin.BuildSelect(columns, CheckTableName(), where, orderBy);
			return QueryNWithParams<dynamic>(sql,
				inParams, outParams, ioParams, returnParams,
				behavior: behavior, connection: connection, args: args);
		}

		override protected IEnumerable<T> QueryNWithParams<T>(string sql = null, object inParams = null, object outParams = null, object ioParams = null, object returnParams = null, bool isProcedure = false, DbCommand command = null, CommandBehavior behavior = CommandBehavior.Default, DbConnection connection = null, params object[] args)
		{
			if (behavior == CommandBehavior.Default && typeof(T) != typeof(IEnumerable<dynamic>))
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
						if (typeof(T) == typeof(IEnumerable<dynamic>))
						{
							// query multiple pattern
							do
							{
								// cast is required because compiler doesn't see that we've just checked this!
								yield return (T)rdr.YieldReturnExpandos();
							}
							while (rdr.NextResult());
						}
						else
						{
							rdr.YieldReturnExpandos();
						}
					}
					if (trans != null) trans.Commit();
				}
			}
		}
#endregion

#region Parameters
		public void AddNamedParam(DbCommand cmd, object value, string name = null, ParameterDirection direction = ParameterDirection.Input, Type type = null)
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

		public void AddParams(DbCommand cmd, params object[] args)
		{
			if (args == null)
			{
				return;
			}
			foreach (var item in args)
			{
				AddNamedParam(cmd, item);
			}
		}

		/// <remarks>
		/// <see cref="NameValueCollection"/> *is* supported in .NET Core 1.1, but got a bit lost:
		/// https://github.com/dotnet/corefx/issues/10338
		/// For folks that hit missing types from one of these packages after upgrading to Microsoft.NETCore.UniversalWindowsPlatform they can reference the packages directly as follows.
		/// "System.Collections.NonGeneric": "4.0.1",
		/// "System.Collections.Specialized": "4.0.1", ****
		/// "System.Threading.Overlapped": "4.0.1",
		/// "System.Xml.XmlDocument": "4.0.1"
		/// </remarks>
		public void AddNamedParams(DbCommand cmd, object nameValuePairs, ParameterDirection direction = ParameterDirection.Input)
		{
			if (nameValuePairs == null)
			{
				return;
			}

			object[] values = nameValuePairs as object[];
			if(values != null)
			{
				if (direction != ParameterDirection.Input)
				{
					throw new InvalidOperationException("object[] arguments supported for input parameters only");
				}
				// anonymous parameters from array
				foreach (var value in values)
				{
					AddNamedParam(cmd, value, string.Empty);
				}
				return;
			}

			var nvp = nameValuePairs as ExpandoObject;
			if (nvp != null)
			{
				foreach(var pair in nvp.AsDictionary())
				{
					AddNamedParam(cmd, pair.Value, pair.Key, direction);
				}
				return;
			}

			if (nameValuePairs.GetType() == typeof(NameValueCollection) || nameValuePairs.GetType().GetTypeInfo().IsSubclassOf(typeof(NameValueCollection)))
			{
				var argsCollection = (NameValueCollection)nameValuePairs;
				foreach(string name in argsCollection)
				{
					AddNamedParam(cmd, argsCollection[name], name);
				}
				return;
			}

			// names, values and types from properties of anonymous object or POCO
			foreach (PropertyInfo property in nameValuePairs.GetType().GetProperties())
			{
				// Extra null in GetValue() required for .NET backwards compatibility
				AddNamedParam(cmd, property.GetValue(nameValuePairs, null), property.Name, direction, property.PropertyType);
			}
		}
#endregion

#region Implementation
		protected IEnumerable<dynamic> _TableInfo;
		override public IEnumerable<dynamic> TableInfo
		{
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
#endregion
	}
}