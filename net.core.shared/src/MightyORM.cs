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
			PrimaryKeyString = primaryKey;
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

#region MircoORM interace
		// NB MUST return object not int because of MySQL ulong return type.
		// Note also: it is worth passing in something other than "*"; COUNT over any
		// column which can contain null COUNTS only the non-null values.
		override public object Count(string columns = "*", string where = null,
			params object[] args)
		{
			return Count(columns, where, null, args);
		}
		override public object Count(string columns = "*", string where = null,
			DbConnection connection = null,
			params object[] args)
		{
			var expression = string.Format("COUNT({0})", columns);
			return Aggregate(expression, where, connection, args);
		}
		// Use this for MAX, MIN, SUM, AVG (basically it's scalar on current table)
		override public object Aggregate(string expression, string where = null,
			params object[] args)
		{
			return Aggregate(expression, where, null, args);
		}
		override public object Aggregate(string expression, string where = null,
			DbConnection connection = null,
			params object[] args)
		{
			return ScalarWithParams(_plugin.BuildSelect(expression, CheckTableName(), where),
				connection: connection, args: args);
		}

		// ORM: Single from our table
		override public dynamic Single(object key, string columns = null,
			DbConnection connection = null)
		{
			return Single(WhereForKey(), connection, columns, KeysFromKey(key));
		}
		override public dynamic Single(string where,
			params object[] args)
		{
			return Single(where, null, null, args);
		}
		// THAT is it........ :-))))))
		// DbConnection coming before columns spec is really useful, as it avoids ambiguity between a column spec and a first string arg
		override public dynamic Single(string where,
			DbConnection connection = null,
			string columns = null,
			params object[] args)
		{
			return SingleWithParams(where, columns, connection: connection, args: args);
		}
		
		// WithParams version just in case; allows transactions for a start
		override public dynamic SingleWithParams(string where, string columns = null,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args)
		{
			return AllWithParams(CommandBehavior.SingleRow,
				null, null, columns,
				inParams, outParams, ioParams, returnParams,
				connection,
				args).FirstOrDefault();
		}

		// ORM
		override public IEnumerable<dynamic> All(
			string where = null, string orderBy = null, string columns = null,
			params object[] args)
		{
			return AllWithParams(where, orderBy, columns, args: args);
		}

		override public IEnumerable<dynamic> AllWithParams(
			string where = null, string orderBy = null, string columns = null,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args)
		{
			return AllWithParams(CommandBehavior.Default,
				where, orderBy, columns,
				inParams, outParams, ioParams, returnParams,
				connection,
				args);		
		}

		// ORM version (there is also a data wrapper version)
		// You must provider orderBy, except you don't have to as it will order by PK if you don't (or exception if there is no PK defined)
		// columns (currently?) not first, as it's an override to something we (may) have already provided in the constructor...
		override public dynamic Paged(string orderBy = null, string where = null,
			string columns = null,
			int pageSize = 20, int currentPage = 1,
			DbConnection connection = null,
			params object[] args)
		{
			return PagedFromSelect(columns, CheckTableName(), orderBy, where, pageSize, currentPage, connection, args);
		}

		// save (insert or update) one or more items
		override public int Save(params object[] items)
		{
			return Save(null, items);
		}
		override public int Save(DbConnection connection, params object[] items)
		{
			return Action(ORMAction.Save, connection, items);
		}
		
		override public int Insert(params object[] items)
		{
			return Insert(null, items);
		}
		override public int Insert(DbConnection connection, params object[] items)
		{
			return Action(ORMAction.Insert, connection, items);
		}

		override public int Update(params object[] items)
		{
			return Update(null, items);
		}
		override public int Update(DbConnection connection, params object[] items)
		{
			return Action(ORMAction.Update, connection, items);
		}

		// Apply all fields which are present in item to the row matching key.
		// We *don't* filter by available columns - call with <see cref="CreateFrom"/>(<see cref="partialItem"/>) to do that.
		override public int UpdateFrom(object partialItem, object key)
		{
			return UpdateFrom(partialItem, key, null);
		}
		override public int UpdateFrom(object partialItem, object key,
			DbConnection connection)
		{
			return UpdateFrom(partialItem, WhereForKey(), KeysFromKey(key));
		}

		// apply all fields which are present in item to all rows matching where clause
		// for safety you MUST specify the where clause yourself (use "1=1" to update all rows)
		// this removes/ignores any PK fields from the action; keeps auto-named params for args,
		// and uses named params for the update feilds.
		override public int UpdateFrom(object partialItem, string where,
			params object[] args)
		{
			return UpdateFrom(partialItem, where, null, args);
		}
		override public int UpdateFrom(object partialItem, string where,
			DbConnection connection,
			params object[] args)
		{
			var values = new StringBuilder();
			var fromDict = partialItem.ToExpando().AsDictionary();
			var filteredItem = new ExpandoObject();
			var toDict = filteredItem.AsDictionary();
			int i = 0;
			foreach (var fieldName in fromDict)
			{
				if (!PrimaryKeyList.Any(key => key.Equals(fieldName, StringComparison.OrdinalIgnoreCase)))
				{
					if (i > 0) values.Append(", ");
					values.Append(fieldName).Append(" = ").Append(_plugin.PrefixParameterName(fieldName));
					i++;

					toDict.Add(fieldName, fromDict[fieldName]);
				}
			}
			var sql = _plugin.BuildUpdate(CheckTableName(), values.ToString(), where);
			return ExecuteWithParams(sql, args: args, inParams: filteredItem, connection: connection);
		}

		override public int Delete(params object[] items)
		{
			// ambiguous with other overloads otherwise
			return Delete(connection: null, items: items);
		}
		override public int Delete(DbConnection connection, params object[] items)
		{
			return Action(ORMAction.Delete, connection, items);
		}

		override public int DeleteByKey(params object[] keys)
		{
			return DeleteByKey(null, keys);
		}
		override public int DeleteByKey(DbConnection connection, params object[] keys)
		{
			int sum = 0;
			foreach (var key in keys)
			{
				var sql = _plugin.BuildDelete(CheckTableName(), WhereForKey());
				sum += Execute(sql, key);
			}
			return sum;
		}

		// for safety you MUST specify the where clause yourself (use "1=1" to delete all rows)
		override public int Delete(string where,
			params object[] args)
		{
			return Delete(where, null, args);
		}
		override public int Delete(string where,
			DbConnection connection,
			params object[] args)
		{
			var command = _plugin.BuildDelete(CheckTableName(), where);
			return Execute(command, connection, args);
		}
			
		// We can implement NewItem() and ColumnDefault()
		// NB *VERY* useful for better PK handling; ColumnDefault needs to do buffering - actually, it doesn't because we may end up passing the very same object out twice
		override public object ColumnDefault(string column)
		{
			throw new NotImplementedException();
		}

		// NB You do NOT have to use this - you can create new items to pass in to Mighty more or less however you want.
		// The main convenience provided here is to automatically strip out any input which does not match your column names.
		override public dynamic CreateFrom(object nameValues = null, bool addNonPresentAsDefaults = true)
		{
			throw new NotImplementedException();
		}
#endregion

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

		override public IEnumerable<dynamic> Query(DbCommand command,
			DbConnection connection = null)
		{
			return QueryNWithParams<dynamic>(command: command, connection: connection);
		}
		// no connection, easy args
		override public IEnumerable<dynamic> Query(string sql,
			params object[] args)
		{
			return QueryNWithParams<dynamic>(sql, args: args);
		}
		override public IEnumerable<dynamic> QueryWithParams(string sql,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args)
		{
			return QueryNWithParams<dynamic>(sql,
				inParams, outParams, ioParams, returnParams,
				connection: connection, args: args);
		}
		override public IEnumerable<dynamic> QueryFromProcedure(string spName,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args)
		{
			return QueryNWithParams<dynamic>(spName,
				inParams, outParams, ioParams, returnParams,
				isProcedure: true,
				connection: connection, args: args);
		}

		override public IEnumerable<IEnumerable<dynamic>> QueryMultiple(DbCommand command,
			DbConnection connection = null)
		{
			return QueryNWithParams<IEnumerable<dynamic>>(command: command, connection: connection);
		}
		// no connection, easy args
		override public IEnumerable<IEnumerable<dynamic>> QueryMultiple(string sql,
			params object[] args)
		{
			return QueryNWithParams<IEnumerable<dynamic>>(sql, args: args);
		}
		override public IEnumerable<IEnumerable<dynamic>> QueryMultipleWithParams(string sql,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args)
		{
			return QueryNWithParams<IEnumerable<dynamic>>(sql,
				inParams, outParams, ioParams, returnParams,
				connection: connection, args: args);
		}
		override public IEnumerable<IEnumerable<dynamic>> QueryMultipleFromProcedure(string spName,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args)
		{
			return QueryNWithParams<IEnumerable<dynamic>>(spName,
				inParams, outParams, ioParams, returnParams,
				isProcedure: true,
				connection: connection, args: args);
		}

		override public int Execute(DbCommand command,
			DbConnection connection = null)
		{
			// using only applied to local connection
			using (var localConn = ((connection == null) ? OpenConnection() : null))
			{
				command.Connection = connection ?? localConn;
				return command.ExecuteNonQuery();
			}
		}
		// no connection, easy args
		override public int Execute(string sql,
			params object[] args)
		{
			return ExecuteWithParams(sql, args: args);
		}
		// COULD add a RowCount class, like Cursor, to pick out the rowcount if required
		override public dynamic ExecuteWithParams(string sql,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args)
		{
			var command = CreateCommandWithParams(sql,
			inParams, outParams, ioParams, returnParams,
			args: args);
			return Execute(command, connection);
		}
		override public dynamic ExecuteAsProcedure(string spName,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args)
		{
			var command = CreateCommandWithParams(spName,
			inParams, outParams, ioParams, returnParams,
			isProcedure: true,
			args: args);
			return Execute(command, connection);
		}

		override public object Scalar(DbCommand command,
			DbConnection connection = null)
		{
			// using only applied to local connection
			using (var localConn = ((connection == null) ? OpenConnection() : null))
			{
				command.Connection = connection ?? localConn;
				return command.ExecuteScalar();
			}
		}
		// no connection, easy args
		override public object Scalar(string sql,
			params object[] args)
		{
			var command = CreateCommand(sql, args);
			return Scalar(command);
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

		// You must provide orderBy for a paged query; where is optional.
		// In this one instance, because of the connection to the underlying logic of these queries, the user
		// can pass "SELECT columns" instead of columns.
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

		// note 1: no <see cref="DbConnection"/> param to either of these, because the connection for a command to use
		// is always passed in to the action which uses it, or else created by the microORM on the fly
		// note 2: some API calls of the microORM take command objects, you are recommended to pass in commands created
		// by these methods, as certain provider specific command properties are set by Massive on some providers, so
		// your results may vary if you pass in a command not constructed here.
		override public DbCommand CreateCommand(string sql,
			params object[] args)
		{
			return CreateCommand(sql, args: args);
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
		// This is not required, in that passing a single key via args will do this anyway.
		// It's just for exception checking.
		protected object[] KeysFromKey(object key)
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

		private string _whereForKey;
		protected string WhereForKey()
		{
			if (_whereForKey == null)
			{
				if (PrimaryKeyList.Count == 0)
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
				_whereForKey = sb.ToString();
			}
			return _whereForKey;
		}

		protected string CheckTableName()
		{
			if (string.IsNullOrEmpty(TableName))
			{
				throw new InvalidOperationException("No table name has been specified");
			}
			return TableName;
		}

		private int Action(ORMAction action, DbConnection connection, params object[] items)
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

		private IEnumerable<dynamic> AllWithParams(
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

		private IEnumerable<T> QueryNWithParams<T>(string sql = null, object inParams = null, object outParams = null, object ioParams = null, object returnParams = null, bool isProcedure = false, DbCommand command = null, CommandBehavior behavior = CommandBehavior.Default, DbConnection connection = null, params object[] args)
		{
			if (behavior == CommandBehavior.Default && typeof(T) != typeof(IEnumerable<dynamic>))
			{
				behavior = CommandBehavior.SingleResult;
			}
			// using only applied to local connection
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

		public dynamic ResultsAsExpando(DbCommand cmd)
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
#endregion
	}
}