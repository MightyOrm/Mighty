using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Linq;
using System.Reflection;

using Mighty.ConnectionProviders;
using Mighty.DatabasePlugins;
//using Mighty.Interfaces;

namespace Mighty
{
	public partial class MightyORM
		// (- wait till we're ready to actually implement! -)
		// : MicroORM
		// , DataAccessWrapper
		// , NpgsqlCursorController
	{
		protected string _connectionString;
		protected DbProviderFactory _factory;
		internal DatabasePlugin _plugin = null;

		// these should all be properties
		// initialise table name from class name, but only if not == MicroORM(!); get, set, throw
		// exception if attempt to use it when not set
		public string TableName; // NB this may have a dot in to specify owner/schema, and then needs splitting by us, but ONLY when getting information schema
		public string PrimaryKeyString; // un-separated PK(s)
		public List<string> PrimaryKeyList; // separated PK(s)
		public string DefaultColumns;

		// primaryKeySequence is for sequence-based databases (Oracle, PostgreSQL) - there is no default, specify either null or empty string to disable and manually specify your PK values;
		// primaryKeyRetrievalFunction is for non-sequence based databases (MySQL, SQL Server, SQLite) - defaults to default for DB, specify empty string to disable and manually specify your PK values;
		// primaryKeyFields is a comma separated list; if it has more than one column, you cannot specify primaryKeySequence or primaryKeyRetrievalFunction
		// (if neither primaryKeySequence nor primaryKeyRetrievalFunction are set (which is always the case for compound primary keys), you MUST specify non-null, non-default values for every column in your primary key
		// before saving an object)
		public MightyORM(string connectionStringOrName = null, string tableName = null, string primaryKeyFields = null, string primaryKeySequence = null, string primaryKeyRetrievalFunction = null, string defaultColumns = null, ConnectionProvider connectionProvider = null)
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

			_connectionString = connectionProvider.ConnectionString;
			_factory = connectionProvider.ProviderFactoryInstance;
			Type pluginType = connectionProvider.DatabasePluginType;
			_plugin = (DatabasePlugin)Activator.CreateInstance(pluginType, false);
			_plugin.mighty = this;

			if (tableName != null)
			{
				TableName = tableName;
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
			PrimaryKeyString = primaryKeyFields;
			PrimaryKeyList = primaryKeyFields.Split(',').Select(k => k.Trim()).ToList();
			DefaultColumns = defaultColumns;
		}

		// mini-factory for non-table specific access
		static public MightyORM DB(string connectionStringOrName = null)
		{
			return new MightyORM(connectionStringOrName);
		}

		public string Thingify(string thing, string sql, bool addLeadingSpace = true)
		{
			if (string.IsNullOrWhiteSpace(thing)) return string.Empty;
			throw new NotImplementedException();
		}

		public string Unthingify(string thing, string sql)
		{
			if (string.IsNullOrWhiteSpace(thing)) return string.Empty;
			throw new NotImplementedException();
		}

		private IEnumerable<T> QueryNWithParams<T>(string sql, object args, object inParams = null, object outParams = null, object ioParams = null, object returnParams = null, bool isProcedure = false, DbConnection connection = null, DbCommand command = null)
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

		// The ones which really are the same cross-db don't need to be put into the plugin classes; plugin can be extended if required at a future point.
		private string BuildScalar(string expression)
		{
			return string.Format("SELECT {0} FROM {1}", expression, TableName);
		}

		public string CreateTableNameFromClassName(string className)
		{
			return className;
		}

#region Not Implemented - TEMP
		public DbConnection OpenConnection()
		{
			throw new NotImplementedException();
		}

		public DbCommand CreateCommand(string sql, DbConnection connection = null)
		{
			throw new NotImplementedException();
		}

		public DbCommand CreateCommandWithParams(string sql,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null, bool isProcedure = false,
			DbConnection connection = null,
			params object[] args)
		{
			throw new NotImplementedException();
		}
#endregion

#region Would be NpgsqlCursorController abstract class interface if we had multiple inheritance
		public bool NpgsqlAutoDereferenceCursors { get; set; } = true;
		public int NpgsqlAutoDereferenceFetchSize { get; set; } = 10000;
#endregion
	}
}