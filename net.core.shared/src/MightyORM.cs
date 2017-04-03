using System;
using System.Text;

namespace Mighty
{
	public class MightyORM : MicroORM, AdoNetDataAccessWrapper
	{
		protected string _connectionString;
		protected DbProviderFactory _factory;

		// should be properties
		public string Table; // NB this may have a dot in, and then needs splitting, but ONLY when getting information schema
		public string PrimaryKeyField;
		public string Columns;

		// pkSequence is for sequence-based databases
		public MightyORM(string connectionStringOrName = null, string table = null, string primaryKeyField = null, string primaryKeySequence = null, string columns = null, ConnectionProvider connectionProvider = null)
		{
			if (connectionProvider == null)
			{
#if !COREFX
				connectionProvider = new ConfigFileConnectionProvider().Init(connectionStringOrName);
				if (connectionProvider.connectionString == null)
#endif
				{
					connectionProvider = new PureConnectionStringProvider()
#if !COREFX
						.UsedAfterConfigFile()
#endif
						.Init(connectionStringOrName);
				}
			}
			else
			{
				connectionProvider.Init(connectionStringOrName);
			}

			_connectionString = connectionProvider.connectionString;
			_factory = connectionProvider.providerFactory;
			Table = table;
			PrimaryKeyField = primaryKeyField;
			Columns = columns;
		}

		private IDatabasePlugin GetPlugin(SupportedDatabase supportedDatabase)
		{
			var pluginClassName = "Mighty.Plugin." + supportedDatabase.ToString();
			var type = Type.GetType(pluginClassName);
			if(type == null)
			{
				throw new NotImplementedException("Cannot find type " + pluginClassName);
			}
			var plugin = (IDatabasePlugin)Activator.CreateInstance(type, false);
			plugin._dynamicModel = this;
			return plugin;
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
					command = CreateCommandWithParams(sql, args, inParams, outParams, ioParams, returnParams, isProcedure, connection ?? localConn);
				}
				// manage wrapping transaction if required, and if we have not been passed an incoming connection
				using (var trans = ((connection == null
#if !COREFX
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
								yield return (T)rdr.YieldResult();
							}
							while (rdr.NextResult());
						}
						else
						{
							// query pattern
							while (rdr.Read())
							{
								yield return rdr.RecordToExpando();
							}
						}
					}
					if (trans != null) trans.Commit();
				}
			}
		}

		public dynamic ResultsAsExpando(DbCommand cmd)
		{
			dynamic result = new ExpandoObject();
			var resultDictionary = (IDictionary<string, object>)result;
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
			return result;
		}

		private string BuildScalar(string expression)
		{
			return string.Format("SELECT {0} FROM {1}", expression, Table)
		}
	}
}