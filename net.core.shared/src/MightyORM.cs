using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Text;

using Mighty.ConnectionProviders;
using Mighty.DatabasePlugins;

namespace Mighty
{
	public partial class MightyORM // (- wait till we're ready to actually implement! -) : API.MicroORM
	{
		protected string _connectionString;
		protected DbProviderFactory _factory;

		// these should all be properties
		// initialise table name from class name, but only if not == MicroORM(!); get, set, throw
		// exception if attempt to use it when not set
		public string Table; // NB this may have a dot in to specify owner/schema, and then needs splitting by us, but ONLY when getting information schema
		public string PrimaryKeyFields;
		public string Columns;

		// primaryKeySequence is for sequence-based databases (Oracle, PostgreSQL) - there is no default, specify either null or empty string to disable and manually specify your PK values;
		// primaryKeyRetrievalFunction is for non-sequence based databases (MySQL, SQL Server, SQLite) - defaults to default for DB, specify empty string to disable and manually specify your PK values;
		// primaryKeyFields is a comma separated list; if it has more than one column, you cannot specify primaryKeySequence or primaryKeyRetrievalFunction
		// (if neither primaryKeySequence nor primaryKeyRetrievalFunction are set (which is always the case for compound primary keys), you MUST specify non-null, non-default values for every column in your primary key
		// before saving an object)
		// *** okay, shite, how do we know if a compound key object is an insert or an update?)
		public MightyORM(string connectionStringOrName = null, string table = null, string primaryKeyFields = null, string primaryKeySequence = null, string primaryKeyRetrievalFunction = null, string defaultColumns = null, ConnectionProvider connectionProvider = null)
		{
			if (connectionProvider == null)
			{
#if !COREFX
				connectionProvider = new ConfigFileConnectionProvider().Init(connectionStringOrName);
				if (connectionProvider.ConnectionString == null)
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

			_connectionString = connectionProvider.ConnectionString;
			_factory = connectionProvider.ProviderFactory;
			Table = table;
			PrimaryKeyFields = primaryKeyFields; // More
			Columns = defaultColumns;
		}

		// mini-factory for non-table specific access
		public static MightyORM DB(string connectionStringOrName = null)
		{
			return new MightyORM(connectionStringOrName);
		}

		private IDatabasePlugin GetPlugin(SupportedDatabase supportedDatabase)
		{
			var pluginClassName = "Mighty.Plugins." + supportedDatabase.ToString();
			var type = Type.GetType(pluginClassName);
			if (type == null)
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