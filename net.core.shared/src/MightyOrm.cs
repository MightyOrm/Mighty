using System;
using System.Collections.Async;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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
using System.Threading.Tasks;

namespace Mighty
{
	/// <summary>
	/// In order to most simply support generics, the dynamic version of Mighty has to be a sub-class of the generic version, but of course the dynamic version is still the nicest version to use! :)
	/// </summary>
	public class MightyOrm : MightyOrm<dynamic>
	{
		#region Constructor
		/// <summary>
		/// Constructor for pure dynamic version.
		/// </summary>
		/// <param name="connectionString">
		/// Connection string with support for additional, non-standard "ProviderName=" property.
		/// On .NET Framework but not .NET Core this can also, optionally, be a config file connection string name (in which case the provider name is specified
		/// as an additional config file attribute next to the connection string).
		/// </param>
		/// <param name="tableName">Table name</param>
		/// <param name="primaryKeyField">Primary key field name; or comma separated list of names for compound PK</param>
		/// <param name="sequence">Optional sequence name for PK inserts on sequence-based DBs; or, optionally override
		/// identity retrieval function for identity-based DBs (e.g. specify "@@IDENTITY" here for SQL Server CE). As a special case,
		/// send an empty string (i.e. not the default value of null) to turn off identity support on identity-based DBs.</param>
		/// <param name="valueColumn">Specify the value field, for lookup tables</param>
		/// <param name="columns">Default column list</param>
		/// <param name="validator">Optional validator</param>
		/// <param name="mapper">Optional C# &lt;-&gt; SQL name mapper</param>
		/// <param name="profiler">Optional SQL profiler</param>
		/// <param name="connectionProvider">Optional connection provider (only needed for providers not yet known to MightyOrm)</param>
		/// <remarks>
		/// What about the SQL Profiler? Should this (really) go into here as a parameter?
		/// ALL column names in the above are pre-mapped C# names, not post-mapped SQL names, where you have a mapper which makes them different.
		/// </remarks>
		public MightyOrm(string connectionString = null,
						 string tableName = null,
						 string primaryKeyField = null,
						 string valueColumn = null,
						 string sequence = null,
						 string columns = null,
						 Validator validator = null,
						 SqlNamingMapper mapper = null,
						 SqlProfiler profiler = null,
						 ConnectionProvider connectionProvider = null)
		{
			UseExpando = true;

			// Subclass-based table name override for dynamic version of MightyOrm
			string tableClassName = null;

			var me = this.GetType();
			// leave table name unset if we are not a true sub-class; this test enforces strict sub-class (i.e. does not pass for an instance of the class itself)
			if (me
#if !NETFRAMEWORK
				.GetTypeInfo()
#endif
				.IsSubclassOf(typeof(MightyOrm)))
			{
				tableClassName = me.Name;
			}
			Init(connectionString, tableName, tableClassName, primaryKeyField, valueColumn, sequence, columns, validator, mapper, profiler, 0, connectionProvider);
		}
#endregion

#region Convenience factory
		/// <summary>
		/// Mini-factory for non-table specific access (equivalent to a constructor call)
		/// </summary>
		/// <param name="connectionStringOrName"></param>
		/// <returns></returns>
		/// <remarks>
		/// Static, so can't be made part of any kind of interface, even though we want this on the generic and dynamic versions.
		/// I think this requires new because of the conflict with the MightyOrm&lt;T&gt; version.
		/// TO DO: check.
		/// </remarks>
		new static public MightyOrm DB(string connectionStringOrName = null)
		{
			return new MightyOrm(connectionStringOrName);
		}
#endregion

	}

	public partial class MightyOrm<T> : MightyOrmMockable<T> where T : class, new()
	{
		#region Constructor
		/// <summary>
		/// Strongly typed MightyOrm constructor
		/// </summary>
		/// <param name="connectionString">
		/// Connection string with support for additional, non-standard "ProviderName=" property.
		/// On .NET Framework but not .NET Core this can also, optionally, be a config file connection string name (in which case the provider name is specified
		/// as an additional config file attribute next to the connection string).
		/// </param>
		/// <param name="tableName">Override the table name (defaults to using T class name)</param>
		/// <param name="primaryKeyField">Primary key field name; or comma separated list of names for compound PK</param>
		/// <param name="valueColumn">Specify the value field, for lookup tables</param>
		/// <param name="sequence">Optional sequence name for PK inserts on sequence-based DBs; or, optionally override
		/// identity retrieval function for identity-based DBs (e.g. specify "@@IDENTITY" here for SQL Server CE). As a special case,
		/// send an empty string (i.e. not the default value of null) to turn off identity support on identity-based DBs.</param>
		/// <param name="columns">Default column list (specifies C# names rather than SQL names, if you have defined a mapper)</param>
		/// <param name="validator">Optional validator</param>
		/// <param name="mapper">Optional C# &lt;-&gt; SQL name mapper</param>
		/// <param name="profiler">Optional SQL profiler</param>
		/// <param name="connectionProvider">Optional connection provider (only needed for providers not yet known to MightyOrm)</param>
		/// <param name="propertyBindingFlags">Specify which properties should be managed by the ORM</param>
		public MightyOrm(string connectionString = null,
						 string tableName = null,
						 string primaryKeyField = null,
						 string valueColumn = null,
						 string sequence = null,
						 string columns = null,
						 Validator validator = null,
						 SqlNamingMapper mapper = null,
						 SqlProfiler profiler = null,
						 ConnectionProvider connectionProvider = null,
						 BindingFlags propertyBindingFlags = BindingFlags.Instance | BindingFlags.Public)
		{
			// If this has been called as part of constructing MightyOrm (non-generic), then return immediately and let that constructor do all the work
			if (this is MightyOrm) return;

			// Table name for MightyOrm<T> is taken from type T not from a constructor argument; use SqlNamingMapper to override it.
			string tableClassName = typeof(T).Name;

			Init(connectionString, tableName, tableClassName, primaryKeyField, valueColumn, sequence, columns, validator, mapper, profiler, propertyBindingFlags, connectionProvider);
		}
		#endregion

		#region Convenience factory
		// mini-factory for non-table specific access
		// (equivalent to a constructor call)
		// <remarks>static, so can't be defined anywhere but here</remarks>
		static public MightyOrm<T> DB(string connectionStringOrName = null)
		{
			return new MightyOrm<T>(connectionStringOrName);
		}
#endregion

#region Shared initialiser
		// sequence is for sequence-based databases (Oracle, PostgreSQL); there is no default sequence, specify either null or empty string to disable and manually specify your PK values;
		// for non-sequence-based databases, in unusual cases, you may specify this to specify an alternative key retrieval function
		// (e.g. for example to use @@IDENTITY instead of SCOPE_IDENTITY(), in the case of SQL Server CE)
		// primaryKeyFields is a comma separated list; if it has more than one column, you cannot specify sequence or keyRetrievalFunction
		// (if neither sequence nor keyRetrievalFunction are set (which is always the case for compound primary keys), you MUST specify non-null, non-default values for every column in your primary key
		// before saving an object)
		internal void Init(string connectionString,
						 string tableName,
						 string tableClassName,
						 string primaryKeyField,
						 string valueColumn,
						 string sequence,
						 string columns,
						 Validator xvalidator,
						 SqlNamingMapper xmapper,
						 SqlProfiler xprofiler,
						 BindingFlags propertyBindingFlags,
						 ConnectionProvider connectionProvider)
		{
			// Slightly hacky, works round the fact that static items are not shared between differently typed classes of the same generic type:
			// use passed-in item; followed by global item for this particular generic class (if specified); followed by global item for the dynamic class
			// (which is intended to be the place you should use, if specifying one of these globally), followed by null object.
			// (A null connectionString still makes sense in .NET Framework, where ConfigFileConnectionProvider will then use the first user connectionString from app.Config)
			connectionString = connectionString ?? GlobalConnectionString ?? MightyOrm.GlobalConnectionString ?? null;
			Validator = xvalidator ?? GlobalValidator ?? MightyOrm.GlobalValidator ?? new NullValidator();
			SqlProfiler = xprofiler ?? GlobalSqlProfiler ?? MightyOrm.GlobalSqlProfiler ?? new NullProfiler();
			SqlMapper = xmapper ?? GlobalSqlMapper ?? MightyOrm.GlobalSqlMapper ?? new NullMapper();

			if (!UseExpando)
			{
				InitialiseTypeProperties(propertyBindingFlags);
			}

			if (tableName != null)
			{
				// an empty string can be used to force not using the sub-class name
				TableName = tableName == "" ? null : tableName;
			}
			else if (tableClassName != null)
			{
				TableName = SqlMapper.GetTableNameFromClassName(tableClassName);
			}

			if (TableName != null)
			{
				int i = TableName.LastIndexOf('.');
				if (i >= 0)
				{
					TableOwner = TableName.Substring(0, i);
				}
				BareTableName = TableName.Substring(i + 1);
			}

			if (connectionProvider == null)
			{
#if NETFRAMEWORK
				// try using the string sent in as a connection string name from the config file; revert to pure connection string if it is not there
				connectionProvider = new ConfigFileConnectionProvider().Init(connectionString);
				if (connectionProvider.ConnectionString == null)
#endif
				{
					connectionProvider = new PureConnectionStringProvider()
#if NETFRAMEWORK
						.UsedAfterConfigFile()
#endif
						.Init(connectionString);
				}
			}
			else
			{
				connectionProvider.Init(connectionString);
			}

			ConnectionString = connectionProvider.ConnectionString;
			Factory = connectionProvider.ProviderFactoryInstance;
			Factory = SqlProfiler.Wrap(Factory);
			Type pluginType = connectionProvider.DatabasePluginType;
			Plugin = (PluginBase)Activator.CreateInstance(pluginType, false);
			Plugin.Mighty = this;

			if (primaryKeyField == null && TableName != null)
			{
				primaryKeyField = SqlMapper.GetPrimaryKeyFieldFromClassName(TableName);
			}
			PrimaryKeyFields = primaryKeyField;
			if (primaryKeyField == null)
			{
				PrimaryKeyList = new List<string>();
			}
			else
			{
				PrimaryKeyList = primaryKeyField.Split(',').Select(k => k.Trim()).ToList();
			}
			if (columns == null || columns == "*")
			{
				// If generic, columns have been set to type columns already
				if (UseExpando) Columns = "*";
			}
			else
			{
				ColumnList = columns.Split(',').Select(column => SqlMapper.GetColumnNameFromPropertyName(typeof(T), column)).ToList();
				Columns = columns == null || columns == "*" ? "*" : string.Join(",", ColumnList);
			}
			ValueColumn = string.IsNullOrEmpty(valueColumn) ? null : SqlMapper.GetColumnNameFromPropertyName(typeof(T), valueColumn);
			// After all this, SequenceNameOrIdentityFn is only non-null if we really are expecting to use it
			// (which entails exactly one PK)
			if (!Plugin.IsSequenceBased)
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
					else SequenceNameOrIdentityFn = Plugin.IdentityRetrievalFunction;
				}
			}
			else if (sequence != null)
			{
				// NB on identity-based DBs using an identity on the PK is the default mode of operation (i.e. unless
				// empty string is specified in 'sequence'; or unless there is > 1 primary key), whereas on sequence-based
				// DBs NOT having a sequence is the default (i.e. unless a specific sequence is passed in).
				if (PrimaryKeyList.Count != 1)
				{
					throw new InvalidOperationException("Sequence may only be specified for tables with a single primary key");
				}
				SequenceNameOrIdentityFn = SqlMapper.QuoteDatabaseIdentifier(sequence);
			}

#if DYNAMIC_METHODS
			// Add dynamic method support (mainly for compatibility with Massive)
			// TO DO: This line shouldn't be here, as it's so intimately tied to code in DynamicMethodProvider
			DynamicObjectWrapper = new DynamicMethodProvider<T>(this);
#endif
		}

		protected void InitialiseTypeProperties(BindingFlags propertyBindingFlags)
		{
			// For generic version only, store the column names defined by the generic type
			columnNameToPropertyInfo = new Dictionary<string, PropertyInfo>(SqlMapper.UseCaseInsensitiveMapping ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
			ColumnList = new List<string>();
			foreach (var prop in typeof(T).GetProperties(propertyBindingFlags))
			{
				var columnName = SqlMapper.GetColumnNameFromPropertyName(typeof(T), prop.Name);
				columnNameToPropertyInfo.Add(columnName, prop);
				ColumnList.Add(columnName);
			}
			Columns = string.Join(",", ColumnList);

			// SequenceNameOrIdentityFn is only left at non-null when there is a single PK
			if (SequenceNameOrIdentityFn != null)
			{
				pkProperty = columnNameToPropertyInfo[PrimaryKeyFields];
			}
		}
#endregion

		// Only properties with a non-trivial implementation are here, the rest are in the MicroOrm abstract class.
#region Properties
		protected IEnumerable<dynamic> _TableMetaData;
		override public IEnumerable<dynamic> TableMetaData
		{
			get
			{
				InitializeTableMetaData();
				return _TableMetaData;
			}
		}

		protected Dictionary<string, PropertyInfo> columnNameToPropertyInfo;
		protected PropertyInfo pkProperty;
#endregion

#region Thread-safe initializer for table meta-data
		// Thread-safe initialization based on Microsoft DbProviderFactories reference 
		// https://referencesource.microsoft.com/#System.Data/System/Data/Common/DbProviderFactories.cs

		// called within the lock
		// TO DO: What is being called inside here is really async!
		private async Task LoadTableMetaData()
		{
			var sql = Plugin.BuildTableMetaDataQuery(BareTableName, TableOwner);
			IAsyncEnumerable<dynamic> unprocessedMetaData;
			dynamic db = this;
			if (!UseExpando)
			{
				// we need a dynamic query, so on the generic version we create a new dynamic DB object with the same connection info
				db = new MightyOrm(connectionProvider: new PresetsConnectionProvider(ConnectionString, Factory, Plugin.GetType()));
			}
			unprocessedMetaData = await db.QueryAsync(sql, BareTableName, TableOwner);
			var postProcessedMetaData = await Plugin.PostProcessTableMetaDataAsync(unprocessedMetaData);
			_TableMetaData = FilterTableMetaData(postProcessedMetaData);
		}

		/// <summary>
		/// We drive creating new objects by this list, but we only want to add columns
		/// </summary>
		/// <param name="tableMetaData"></param>
		/// <returns></returns>
		private IEnumerable<dynamic> FilterTableMetaData(IEnumerable<dynamic> tableMetaData)
		{
			foreach (var columnInfo in tableMetaData)
			{
				string cname = columnInfo.COLUMN_NAME;
				columnInfo.IS_MIGHTY_COLUMN = ColumnList == null ? true : ColumnList.Any(columnName => columnName == cname);
			}
			return tableMetaData;
		}

		// fields for thread-safe initialization of TableMetaData
		// (done once or less per instance of MightyOrm, so not static)
		private ConnectionState _initState; // closed (default value), connecting, open
		private object _lockobj = new object();

		private void InitializeTableMetaData()
		{
			// MS code (re-)uses database connection states
			if (_initState != ConnectionState.Open)
			{
				lock (_lockobj)
				{
					switch (_initState)
					{
						case ConnectionState.Closed:
							// 'Connecting' state only relevant if the thread which has the lock can recurse back into here
							// while we are initialising (any other thread can only see Closed or Open)
							_initState = ConnectionState.Connecting;
							try
							{
								// TO DO: This is technically NOT OK - it is sync over async.
								// We definitely want sync over async (at least I think we do, here...), but we do need
								// to be sure (how?) that it's being done safely.
								LoadTableMetaData().Wait();
							}
							finally
							{
								// try-finally ensures that even after exception we register that Initialize has been called, and don't keep retrying
								// (the exception is of course still thrown after the finally code has happened)
								_initState = ConnectionState.Open;
							}
							break;

						case ConnectionState.Connecting:
						case ConnectionState.Open:
							break;

						default:
							throw new Exception("unexpected state");
					}
				}
			}
		}
#endregion

		// Only methods with a non-trivial implementation are here, the rest are in the MicroOrm abstract class.
#region MircoORM interface
		// In theory COUNT expression could vary across SQL variants, in practice it doesn't.
		override public async Task<object> CountAsync(string columns = "*", string where = null,
			DbConnection connection = null,
			params object[] args)
		{
			var expression = string.Format("COUNT({0})", columns);
			return await AggregateWithParamsAsync(expression, where, connection, args: args).ConfigureAwait(false);
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
		override public async Task<object> AggregateWithParamsAsync(string expression, string where = null,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args)
		{
			return await ScalarWithParamsAsync(Plugin.BuildSelect(expression, CheckGetTableName(), where),
				inParams, outParams, ioParams, returnParams,
				connection, args).ConfigureAwait(false);
		}

		/// <summary>
		/// Make a new item from the passed-in name-value collection.
		/// </summary>
		/// <param name="nameValues"></param>
		/// <param name="addNonPresentAsDefaults"></param>
		/// <returns></returns>
		override public T NewFrom(object nameValues = null, bool addNonPresentAsDefaults = true)
		{
			var nvtEnumerator = new NameValueTypeEnumerator(nameValues);
			Dictionary<string, object> columnNameToValue = new Dictionary<string, object>();
			foreach (var nvtInfo in nvtEnumerator)
			{
				string columnName = SqlMapper.GetColumnNameFromPropertyName(typeof(T), nvtInfo.Name);
				PropertyInfo columnInfo;
				if (!columnNameToPropertyInfo.TryGetValue(columnName, out columnInfo)) continue;
				columnNameToValue.Add(columnName, nvtInfo.Value);
			}
			object item;
			IDictionary<string, object> newItemDictionary = null;
			if (UseExpando)
			{
				item = new ExpandoObject();
				newItemDictionary = ((ExpandoObject)item).ToDictionary();
			}
			else
			{
				item = new T();
			}
			// drive the loop by the actual column names
			foreach (var columnInfo in TableMetaData)
			{
				if (!columnInfo.IS_MIGHTY_COLUMN) continue;
				string columnName = columnInfo.COLUMN_NAME;
				PropertyInfo prop = null;
				if (!UseExpando) prop = columnNameToPropertyInfo[columnName]; // meta-data already filtered to props only
				object value;
				if (!columnNameToValue.TryGetValue(columnName, out value))
				{
					if (!addNonPresentAsDefaults) continue;
					value = Plugin.GetColumnDefault(columnInfo);
				}
				if (value != null)
				{
					if (prop != null) prop.SetValue(item, value.ChangeType(prop.PropertyType));
					else newItemDictionary.Add(columnName, value);
				}
			}
			return (T)item;
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
		override public async Task<int> UpdateUsingAsync(object partialItem, string where,
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
			return await ExecuteWithParamsAsync(sql, args: args, inParams: filteredItem, connection: connection).ConfigureAwait(false);
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
		override public async Task<int> DeleteAsync(string where,
			DbConnection connection,
			params object[] args)
		{
			var sql = Plugin.BuildDelete(CheckGetTableName(), where);
			return await ExecuteAsync(sql, connection, args).ConfigureAwait(false);
		}

		/// <summary>
		/// Get the meta-data for a single column
		/// </summary>
		/// <param name="column">Column name</param>
		/// <param name="ExceptionOnAbsent">If true throw an exception if there is no such column, otherwise return null.</param>
		/// <returns></returns>
		override public dynamic GetColumnInfo(string column, bool ExceptionOnAbsent = true)
		{
			var info = TableMetaData.Where(c => column.Equals(c.COLUMN_NAME, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
			if (ExceptionOnAbsent && info == null)
			{
				throw new InvalidOperationException("Cannot find table info for column name " + column);
			}
			return info;
		}

		/// <summary>
		/// Get the default value for a column.
		/// </summary>
		/// <param name="columnName"></param>
		/// <returns></returns>
		/// <remarks>
		/// Although it might look more efficient, GetColumnDefault should not do buffering, as we don't
		/// want to pass out the same actual object more than once.
		/// </remarks>
		override public object GetColumnDefault(string columnName)
		{
			var columnInfo = GetColumnInfo(columnName);
			return Plugin.GetColumnDefault(columnInfo);
		}

		/// <summary>
		/// Return array of key values from passed in key values.
		/// Raise exception if the wrong number of keys are provided.
		/// The wrapping of a single item into an array which this does would happen automatically anyway
		/// in C# params handling, so this code is only required for the exception checking.
		/// </summary>
		/// <param name="key">The key value or values</param>
		/// <returns></returns>
		protected object[] KeyValuesFromKey(object key)
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
		protected string WhereForKeys()
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
					sb.Append(keyName).Append(" = ").Append(Plugin.PrefixParameterName(i++.ToString()));
				}
				_whereForKeys = sb.ToString();
			}
			return _whereForKeys;
		}

		/// <summary>
		/// Return comma-separated list of primary key fields, raising an exception if there are none.
		/// </summary>
		/// <returns></returns>
		protected string CheckGetPrimaryKeyFields()
		{
			if (string.IsNullOrEmpty(PrimaryKeyFields))
			{
				throw new InvalidOperationException("No primary key field(s) have been specified");
			}
			return PrimaryKeyFields;
		}

		/// <summary>
		/// Return value column, raising an exception if not specified.
		/// </summary>
		/// <returns></returns>
		string CheckGetValueColumn(string message)
		{
			if (string.IsNullOrEmpty(ValueColumn))
			{
				throw new InvalidOperationException(message);
			}
			return ValueColumn;
		}

		/// <summary>
		/// Return the single (non-compound) primary key name, with meaningful exception if there isn't one.
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		protected string CheckGetKeyName(string message)
		{
			if (PrimaryKeyList.Count != 1)
			{
				throw new InvalidOperationException(message);
			}
			return PrimaryKeyList[0];
		}

		/// <summary>
		/// Return ith primary key name, with meaningful exception if too many requested.
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		protected string CheckGetKeyName(int i, string message)
		{
			if (i >= PrimaryKeyList.Count)
			{
				throw new InvalidOperationException(message);
			}
			return PrimaryKeyList[i];
		}

		/// <summary>
		/// Return current table name, raising an exception if there isn't one.
		/// </summary>
		/// <returns></returns>
		protected string CheckGetTableName()
		{
			if (string.IsNullOrEmpty(TableName))
			{
				throw new InvalidOperationException("No table name has been specified");
			}
			return TableName;
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
		internal async Task<Tuple<int, T>> ActionOnItemsWithOutputAsync(OrmAction action, DbConnection connection, IEnumerable<object> items)
		{
			T insertedItem = null;
			int count = 0;
			int affected = 0;
			Prevalidate(items, action);
			foreach (var item in items)
			{
				if (Validator.PerformingAction(item, action))
				{
					var _inserted = await ActionOnItemAsync(action, item, connection, count).ConfigureAwait(false);
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
			return new Tuple<int, T>(affected, insertedItem);
		}


		/// <summary>
		/// Checks that every item in the list is valid for the action to be undertaken.
		/// Normally you should not need to override this, but override <see cref="IsValidForAction" /> instead.
		/// </summary>
		/// <param name="action">The ORM action</param>
		/// <param name="items">The list of items. (Can be T, dynamic, or anything else with suitable name-value (and optional type) data in it.)</param>
		virtual internal void Prevalidate(IEnumerable<object> items, OrmAction action)
		{
			if (Validator.Prevalidation == Prevalidation.Off)
			{
				return;
			}
			// Intention of non-shared error list is thread safety
			List<object> Errors = new List<object>();
			bool valid = true;
			foreach (var item in items)
			{
				int oldCount = Errors.Count;
				Validator.ValidateForAction(item, action, Errors);
				if (Errors.Count > oldCount)
				{
					valid = false;
					if (Validator.Prevalidation == Prevalidation.Lazy) break;
				}
			}
			if (valid == false || Errors.Count > 0)
			{
				throw new ValidationException(Errors, "Prevalidation failed for one or more items for " + action);
			}
		}

		/// <summary>
		/// Is the passed in item valid against the current validator for the specified ORMAction?
		/// </summary>
		/// <param name="item">The item</param>
		/// <param name="action">Optional action type (defaults to Save)</param>
		/// <returns></returns>
		override public List<object> IsValid(object item, OrmAction action = OrmAction.Save)
		{
			List<object> Errors = new List<object>();
			if (Validator != null)
			{
				Validator.ValidateForAction(item, action, Errors);
			}
			return Errors;
		}

		/// <summary>
		/// True iff input object has a named field matching the PK name (or PKs for compound primary keys)
		/// </summary>
		/// <param name="item">Item to check</param>
		/// <returns></returns>
		override public bool HasPrimaryKey(object item)
		{
			int count = 0;
			foreach (var info in new NameValueTypeEnumerator(item))
			{
				if (IsKey(info.Name)) count++;
			}
			return count == PrimaryKeyList.Count;
		}

		/// <summary>
		/// Return primary key for item, as simple object for simple PK, or as object[] for compound PK.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="alwaysArray">If true return object[] of 1 item, even for simple PK</param>
		/// <returns></returns>
		override public object GetPrimaryKey(object item, bool alwaysArray = false)
		{
			var pks = new ExpandoObject();
			var pkDictionary = pks.ToDictionary();
			foreach (var info in new NameValueTypeEnumerator(item))
			{
				string canonicalKeyName;
				if (IsKey(info.Name, out canonicalKeyName)) pkDictionary.Add(canonicalKeyName, info.Value);
			}
			if (pkDictionary.Count != PrimaryKeyList.Count)
			{
				throw new InvalidOperationException("PK field(s) not present in object");
			}
			// re-arrange to specified order
			var retval = new List<object>();
			foreach (var key in PrimaryKeyList)
			{
				retval.Add(pkDictionary[key]);
			}
			var array = retval.ToArray();
			if (array.Length == 1 && !alwaysArray)
			{
				return array[0];
			}
			return array;
		}

		// TO DO: We should still be supporting this
#if KEY_VALUES
		/// <summary>
		/// Returns a string/string dictionary which can be bound directly to dropdowns etc http://stackoverflow.com/q/805595/
		/// </summary>
		override public async Task<IDictionary<string, string>> KeyValuesAsync(string orderBy = "")
		{
			string foo = string.Format(" to call {0}, please provide one in your constructor", nameof(KeyValues));
			string valueField = CheckGetValueColumn(string.Format("ValueField is required{0}", foo));
			string primaryKeyField = CheckGetKeyName(string.Format("A single primary key must be specified{0}", foo));
			var results = (await AllAsync(orderBy: orderBy, columns: string.Format("{0}, {1}", primaryKeyField, valueField)).ConfigureAwait(false)).Cast<IDictionary<string, object>>();
			return results.ToDictionary(item => item[primaryKeyField].ToString(), item => item[valueField].ToString());
		}
#endif
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
		override public async Task<DbConnection> OpenConnectionAsync()
		{
			var connection = Factory.CreateConnection();
			connection = SqlProfiler.Wrap(connection);
			connection.ConnectionString = ConnectionString;
			await connection.OpenAsync().ConfigureAwait(false);
			return connection;
		}

		/// <summary>
		/// Execute DbCommand
		/// </summary>
		/// <param name="command">The command</param>
		/// <param name="connection">Optional DbConnection to use</param>
		/// <returns></returns>
		override public async Task<int> ExecuteAsync(DbCommand command,
			DbConnection connection = null)
		{
			// using applied only to local connection
			using (var localConn = ((connection == null) ? await OpenConnectionAsync().ConfigureAwait(false) : null))
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
		override public async Task<object> ScalarAsync(DbCommand command,
			DbConnection connection = null)
		{
			// using applied only to local connection
			using (var localConn = ((connection == null) ? await OpenConnectionAsync().ConfigureAwait(false) : null))
			{
				command.Connection = connection ?? localConn;
				return await command.ExecuteScalarAsync().ConfigureAwait(false);
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
		override public async Task<PagedResults<T>> PagedFromSelectAsync(string columns, string tablesAndJoins, string where, string orderBy,
			int pageSize = 20, int currentPage = 1,
			DbConnection connection = null,
			params object[] args)
		{
			int limit = pageSize;
			int offset = (currentPage - 1) * pageSize;
			if (columns == null) columns = Columns;
			var pagingQueryPair = Plugin.BuildPagingQueryPair(columns, tablesAndJoins, where, orderBy, limit, offset);
			var result = new PagedResults<T>();
			result.TotalRecords = Convert.ToInt32(await ScalarAsync(pagingQueryPair.CountQuery).ConfigureAwait(false));
			result.TotalPages = (result.TotalRecords + pageSize - 1) / pageSize;
			var items = await QueryAsync(pagingQueryPair.PagingQuery).ConfigureAwait(false);
			result.Items = await items.ToListAsync().ConfigureAwait(false);
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
			command = SqlProfiler.Wrap(command);
			Plugin.SetProviderSpecificCommandProperties(command);
			command.CommandText = sql;
			return command;
		}

		/// <summary>
		/// Create command with named, typed, directional parameters.
		/// </summary>
		override public DbCommand CreateCommandWithParams(string sql,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null, bool isProcedure = false,
			DbConnection connection = null,
			params object[] args)
		{
			var command = CreateCommand(sql);
			command.Connection = connection;
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
			var e = new ExpandoObject();
			var resultDictionary = e.ToDictionary();
			for (int i = 0; i < cmd.Parameters.Count; i++)
			{
				var param = cmd.Parameters[i];
				if (param.Direction != ParameterDirection.Input)
				{
					var name = Plugin.DeprefixParameterName(param.ParameterName, cmd);
					var value = Plugin.GetValue(param);
					resultDictionary.Add(name, value == DBNull.Value ? null : value);
				}
			}
			return e;
		}

		/// <summary>
		/// Return all matching items.
		/// </summary>
		override public async Task<IAsyncEnumerable<T>> AllWithParamsAsync(
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
			return await QueryNWithParamsAsync<T>(sql,
				inParams, outParams, ioParams, returnParams,
				behavior: limit == 1 ? CommandBehavior.SingleRow : CommandBehavior.Default, connection: connection, args: args);
		}

		/// <summary>
		/// Yield return values for Query or QueryMultiple.
		/// Use with &lt;T&gt; for single or &lt;IEnumerable&lt;T&gt;&gt; for multiple.
		/// </summary>
		override protected async Task<IAsyncEnumerable<X>> QueryNWithParamsAsync<X>(DbCommand command, CommandBehavior behavior = CommandBehavior.Default, DbConnection connection = null, DbDataReader outerReader = null)
		{
			return new AsyncEnumerable<X>(async yield => {
				if (behavior == CommandBehavior.Default && typeof(X) == typeof(T))
				{
					// (= single result set, not single row...)
					behavior = CommandBehavior.SingleResult;
				}
				// using is applied only to locally generated connection
				using (var localConn = (connection == null ? await OpenConnectionAsync().ConfigureAwait(false) : null))
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
						using (var reader = (outerReader == null ? await Plugin.ExecuteDereferencingReaderAsync(command, behavior, connection ?? localConn).ConfigureAwait(false) : null))
						{
							if (typeof(X) == typeof(IAsyncEnumerable<T>))
							{
								// query multiple pattern
								do
								{
									// cast is required because compiler doesn't see that we've just checked that X is IEnumerable<T>
									// first three params carefully chosen so as to avoid lots of checks about outerReader in the code above in this method
									var next = (X)(await QueryNWithParamsAsync<T>(null, (CommandBehavior)(-1), connection ?? localConn, reader).ConfigureAwait(false));
									await yield.ReturnAsync(next).ConfigureAwait(false);
								}
								while (await reader.NextResultAsync().ConfigureAwait(false));
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
									while (await useReader.ReadAsync().ConfigureAwait(false))
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
											await yield.ReturnAsync((X)(object)e).ConfigureAwait(false);
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
											await yield.ReturnAsync((X)(object)t).ConfigureAwait(false);
										}
									}
								}
							}
						}
						if (trans != null) trans.Commit();
					}
				}
			});
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
		private async Task<object> ActionOnItemAsync(OrmAction action, object item, DbConnection connection, int outerCount)
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
					throw new Exception("incorrect " + nameof(OrmAction) + "=" + action + " at action choice in " + nameof(ActionOnItemAsync));
			}
			command.Connection = connection;
			if (action == OrmAction.Insert && SequenceNameOrIdentityFn != null)
			{
				// *All* DBs return a huge sized number for their identity by default, following Massive we are normalising to int
				var pk = Convert.ToInt32(await ScalarAsync(command).ConfigureAwait(false));
				var result = UpsertItemPK(item, pk, originalAction == OrmAction.Insert && outerCount == 0);
				return result;
			}
			else
			{
				int n = await ExecuteAsync(command).ConfigureAwait(false);
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
			string sql = Plugin.BuildUpdate(TableName, string.Join(", ", updateNameValuePairs), string.Join(" AND ", whereNameValuePairs));
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
			string sql = Plugin.BuildInsert(TableName, string.Join(", ", insertNames), string.Join(", ", insertValues));
			if (SequenceNameOrIdentityFn != null)
			{
				sql += ";\r\n" +
					   (Plugin.IsSequenceBased ? Plugin.BuildCurrvalSelect(SequenceNameOrIdentityFn) : string.Format("SELECT {0}", SequenceNameOrIdentityFn)) +
					   ";";
			}
			var command = CreateCommand(sql);
			AddNamedParams(command, item, pkFilter: pkFilter);
			Plugin.FixupInsertCommand(command);
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
			string sql = Plugin.BuildDelete(TableName, string.Join(" AND ", whereNameValuePairs));
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
		/// <param name="createIfNeeded">Writing back the value to a mutable item is worth it, but creating a
		/// replacement item for an immutable item other than the first, on a true insert, isn't</param>
		/// <returns></returns>
		private object UpsertItemPK(object item, object pk, bool createIfNeeded)
		{
			var itemAsExpando = item as ExpandoObject;
			if (itemAsExpando != null)
			{
				var dict = itemAsExpando.ToDictionary();
				dict[PrimaryKeyFields] = pk;
				return item;
			}
			var nvc = item as NameValueCollection;
			if (nvc != null)
			{
				nvc[PrimaryKeyFields] = pk.ToString();
				return item;
			}
			// Try to write back field to arbitrary POCO
			var pkProp = item.GetType().GetProperty(PrimaryKeyFields);
			if (pkProp != null && pkProp.CanWrite)
			{
				pkProp.SetValue(item, pk);
				return item;
			}
			if (createIfNeeded)
			{
				// Convert POCO to expando
				var result = item.ToExpando();
				var dict = result.ToDictionary();
				dict[PrimaryKeyFields] = pk;
				return result;
			}
			return null;
		}

		/// <summary>
		/// Is this the name of a PK field?
		/// </summary>
		/// <param name="fieldName">The name to check</param>
		/// <param name="canonicalKeyName">Returns the canonical key name, i.e. as specified in <see cref="MightyOrm"/> constructor</param>
		/// <returns></returns>
		internal bool IsKey(string fieldName, out string canonicalKeyName)
		{
			canonicalKeyName = null;
			foreach (var key in PrimaryKeyList)
			{
				if (key.Equals(fieldName, SqlMapper.UseCaseInsensitiveMapping ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
				{
					canonicalKeyName = key;
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Is this the name of a PK field?
		/// </summary>
		/// <param name="fieldName">The name to check</param>
		/// <returns></returns>
		internal bool IsKey(string fieldName)
		{
			string canonicalKeyName;
			return IsKey(fieldName, out canonicalKeyName);
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
				if (!Plugin.SetAnonymousParameter(p))
				{
					throw new InvalidOperationException("Current ADO.NET provider does not support anonymous parameters");
				}
			}
			else
			{
				p.ParameterName = Plugin.PrefixParameterName(name ?? cmd.Parameters.Count.ToString(), cmd);
			}
			Plugin.SetDirection(p, direction);
			if (value == null)
			{
				if (type != null)
				{
					Plugin.SetValue(p, type.GetDefaultValue());
					// explicitly lock type and size to the values which ADO.NET has just implicitly assigned
					// (when only implictly assigned, setting Value to DBNull.Value later on causes these to reset, in at least the Npgsql and SQL Server providers)
					p.DbType = p.DbType;
					p.Size = p.Size;
				}
				// Some ADO.NET providers completely ignore the parameter DbType when deciding on the .NET type for return values, others do not
				else if (direction != ParameterDirection.Input && !Plugin.IgnoresOutputTypes(p))
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
					if (!Plugin.SetCursor(p, cursor.Value))
					{
						throw new InvalidOperationException("ADO.NET provider does not support cursors");
					}
				}
				else
				{
					// Note - the passed in parameter value can be a real cursor ref, this works - at least in Oracle
					Plugin.SetValue(p, value);
				}
			}
			cmd.Parameters.Add(p);
		}

		/// <summary>
		/// Add auto-named parameters from an array of parameter values (normally would have been passed in to microORM
		/// using C# parameter syntax)
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
		/// Optionally control whether to add only the PKs or only not the PKs, when creating parameters from object
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
			foreach (var paramInfo in new NameValueTypeEnumerator(nameValuePairs, direction))
			{
				if (pkFilter == PkFilter.DoNotFilter || (IsKey(paramInfo.Name) == (pkFilter == PkFilter.KeysOnly)))
				{
					AddParam(cmd, paramInfo.Value, paramInfo.Name, direction, paramInfo.Type);
				}
			}
		}
#endregion
	}
}