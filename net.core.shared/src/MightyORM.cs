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
using Mighty.Validation;

namespace Mighty
{
	public partial class MightyORM : MicroORM
		// (- wait till we're ready to actually implement! -)
		// : MicroORM
		// , DataAccessWrapper
		// , NpgsqlCursorController
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
				// leave table name unset if we are not a true sub-class
				// test enforces strict sub-class, does not pass for an instance of the class itself
				if (me.GetTypeInfo().IsSubclassOf(typeof(MightyORM)))
				{
					TableName = CreateTableNameFromClassName(me.Name);
				}
			}
			PrimaryKeyString = primaryKey;
			PrimaryKeyList = primaryKey.Split(',').Select(k => k.Trim()).ToList();
			DefaultColumns = columns;
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
		// NB MUST return object not int because of MySQL ulong return type
		override public object Count(string columns = "*", string where = null,
			params object[] args)
		{
			return Count(columns, where, null, args);
		}
		override public object Count(string columns = "*", string where = null,
			DbConnection connection = null,
			params object[] args)
		{
			return Aggregate("COUNT(*)", where, connection, args);
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
			throw new NotImplementedException();
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
		override public int UpdateFrom(object partialItem, string where,
			params object[] args)
		{
			return UpdateFrom(partialItem, where, null, args);
		}
		override public int UpdateFrom(object partialItem, string where,
			DbConnection connection,
			params object[] args)
		{
			throw new NotImplementedException();
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
			throw new NotImplementedException();
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
			throw new NotImplementedException();
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
			throw new NotImplementedException();
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
				true,
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
				true,
				connection: connection, args: args);
		}

		override public int Execute(DbCommand command,
			DbConnection connection = null)
		{
			throw new NotImplementedException();
		}
		// no connection, easy args
		override public int Execute(string sql,
			params object[] args)
		{
			throw new NotImplementedException();
		}
		// COULD add a RowCount class, like Cursor, to pick out the rowcount if required
		override public dynamic ExecuteWithParams(string sql,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args)
		{
			throw new NotImplementedException();
		}
		override public dynamic ExecuteAsProcedure(string spName,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args)
		{
			throw new NotImplementedException();
		}

		override public object Scalar(DbCommand command,
			DbConnection connection = null)
		{
			throw new NotImplementedException();
		}
		// no connection, easy args
		override public object Scalar(string sql,
			params object[] args)
		{
			throw new NotImplementedException();
		}
		override public object ScalarWithParams(string sql,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args)
		{
			throw new NotImplementedException();
		}
		override public object ScalarFromProcedure(string spName,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args)
		{
			throw new NotImplementedException();
		}

		// You must provide orderBy for a paged query; where is optional.
		override public dynamic PagedFromSelect(string columns, string tablesAndJoins, string orderBy, string where = null,
			int pageSize = 20, int currentPage = 1,
			DbConnection connection = null,
			params object[] args)
		{
			throw new NotImplementedException();
		}

		// note: no <see cref="DbConnection"/> param to either of these, because the connection for a command to use
		// is always passed in to the action which uses it, or else created by the microORM on the fly
		override public DbCommand CreateCommand(string sql,
			params object[] args)
		{
			return CreateCommand(sql, args: args);
		}
		override public DbCommand CreateCommandWithParams(string sql,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null, bool isProcedure = false,
			params object[] args)
		{
			throw new NotImplementedException();
		}
#endregion

#region Utility methods
		internal string Unthingify(string thing, string sql)
		{
			return Thingify(thing, sql, false);
		}

		internal string Thingify(string thing, string sql, bool yes = true)
		{
			if (sql == null) return string.Empty;
			sql = sql.Trim();
			if (sql == string.Empty) return string.Empty;
			if (sql.Length > thing.Length &&
				sql.StartsWith(thing, StringComparison.OrdinalIgnoreCase) &&
				string.IsNullOrWhiteSpace(sql.Substring(thing.Length, 1)))
			{
				return yes ? sql.Substring(thing.Length + 1).Trim() : sql;
			}
			else
			{
				return yes ? sql : string.Format("{0} {1}", thing, sql.Trim());
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
					sb.Append(keyName).Append(" = ").Append(_plugin.PrefixParameterName(i.ToString()));
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
			CommandBehavior behaviour,
			string where = null, string orderBy = null, string columns = null,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args)
		{
			throw new NotImplementedException();
		}

		private IEnumerable<T> QueryNWithParams<T>(string sql = null, object inParams = null, object outParams = null, object ioParams = null, object returnParams = null, bool isProcedure = false, DbCommand command = null, DbConnection connection = null, params object[] args)
		{
			using (var localConn = (connection == null ? OpenConnection() : null))
			{
				if (command != null)
				{
					command.Connection = localConn;
				}
				else
				{
					command = CreateCommandWithParams(sql, inParams, outParams, ioParams, returnParams, isProcedure, connection ?? localConn, args);
				}
				// manage wrapping transaction if required, and if we have not been passed an incoming connection
				using (var trans = ((connection == null
#if !true//COREFX
					// TransactionScope support
					&& Transaction.Current == null
#endif
					&& _plugin.RequiresWrappingTransaction(command)) ? localConn.BeginTransaction() : null))
				{
					// TO DO: Apply single result hint when appropriate
					// (since all the cursors we might dereference come in the first result set, we can do this even
					// if we are dereferencing PostgreSQL cursors)
					using (var rdr = _plugin.ExecuteDereferencingReader(command, connection ?? localConn))
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