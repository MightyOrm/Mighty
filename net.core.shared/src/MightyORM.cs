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
using Mighty.DatabasePlugins;
using Mighty.Interfaces;
using Mighty.Mapping;
using Mighty.Parameters;
using Mighty.Profiling;
using Mighty.Validation;

namespace Mighty
{
	/// <summary>
	/// In order to support generics, the dynamic version now a sub-class of the generic version, though of course it is still the nicest version.
	/// </summary>
	public class MightyORM : MightyORM<dynamic>
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
		/// <param name="valueField">Specify the value field, for lookup tables</param>
		/// <param name="columns">Default column list</param>
		/// <param name="validator">Optional validator</param>
		/// <param name="mapper">Optional C# &lt;-&gt; SQL name mapper</param>
		/// <param name="profiler">Optional SQL profiler</param>
		/// <param name="connectionProvider">Optional connection provider (only needed for providers not yet known to MightyORM)</param>
		/// <remarks>
		/// What about the SQL Profiler? Should this (really) go into here as a parameter?
		/// </remarks>
		public MightyORM(string connectionString = null,
						 string tableName = null,
						 string primaryKeyField = null,
						 string valueField = null,
						 string sequence = null,
						 string columns = null,
						 Validator validator = null,
						 SqlNamingMapper mapper = null,
						 Profiler profiler = null,
						 ConnectionProvider connectionProvider = null)
		{
			// Subclass-based table name override for dynamic version of MightyORM
			string tableClassName = null;

			var me = this.GetType();
			// leave table name unset if we are not a true sub-class; this test enforces strict sub-class (i.e. does not pass for an instance of the class itself)
			if (me
#if !NETFRAMEWORK
				.GetTypeInfo()
#endif
				.IsSubclassOf(typeof(MightyORM)))
			{
				tableClassName = me.Name;
			}
			Init(connectionString, tableName, tableClassName, primaryKeyField, valueField, sequence, columns, validator, mapper, profiler, connectionProvider);
		}
#endregion

#region Convenience factory
		/// <summary>
		/// Mini-factory for non-table specific access (equivalent to a constructor call)
		/// </summary>
		/// <param name="connectionStringOrName"></param>
		/// <returns></returns>
		/// <remarks>Static, so can't be made part of any kind of interface</remarks>
		new static public MightyORM DB(string connectionStringOrName = null)
		{
			return new MightyORM(connectionStringOrName);
		}
#endregion
	}

	public partial class MightyORM<T> : MicroORM<T> where T : class, new()
	{
		#region Constructor
		/// <summary>
		/// Strongly typed MightyORM constructor
		/// </summary>
		/// <param name="connectionString">
		/// Connection string with support for additional, non-standard "ProviderName=" property.
		/// On .NET Framework but not .NET Core this can also, optionally, be a config file connection string name (in which case the provider name is specified
		/// as an additional config file attribute next to the connection string).
		/// </param>
		/// <param name="tableName">Override the table name (defaults to using T class name)</param>
		/// <param name="primaryKeyField">Primary key field name; or comma separated list of names for compound PK</param>
		/// <param name="valueField">Specify the value field, for lookup tables</param>
		/// <param name="sequence">Optional sequence name for PK inserts on sequence-based DBs; or, optionally override
		/// identity retrieval function for identity-based DBs (e.g. specify "@@IDENTITY" here for SQL Server CE). As a special case,
		/// send an empty string (i.e. not the default value of null) to turn off identity support on identity-based DBs.</param>
		/// <param name="columns">Default column list</param>
		/// <param name="validator">Optional validator</param>
		/// <param name="mapper">Optional C# &lt;-&gt; SQL name mapper</param>
		/// <param name="profiler">Optional SQL profiler</param>
		/// <param name="connectionProvider">Optional connection provider (only needed for providers not yet known to MightyORM)</param>
		/// <param name="propertyBindingFlags">Specify which properties should be managed by the ORM</param>
		public MightyORM(string connectionString = null,
						 string tableName = null,
						 string primaryKeyField = null,
						 string valueField = null,
						 string sequence = null,
						 string columns = null,
						 Validator validator = null,
						 SqlNamingMapper mapper = null,
						 Profiler profiler = null,
						 ConnectionProvider connectionProvider = null,
						 BindingFlags propertyBindingFlags = BindingFlags.Instance | BindingFlags.Public)
		{
			// If this has been called as part of constructing MightyORM (non-generic), then return immediately and let that constructor do all the work
			if (this is MightyORM) return;

			// Table name for MightyORM<T> is taken from type T not from a constructor argument; use SqlNamingMapper to override it.
			string tableClassName = typeof(T).Name;

			Init(connectionString, tableName, tableClassName, primaryKeyField, valueField, sequence, columns, validator, mapper, profiler, connectionProvider);

			// For generic version only, store the column names defined by the generic type
			columnNameToPropertyInfo = new Dictionary<string, PropertyInfo>(Mapper.UseCaseInsensitiveMapping ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
			foreach (var prop in typeof(T).GetProperties(propertyBindingFlags))
			{
				var columnName = Mapper.GetColumnNameFromPropertyName(tableClassName, prop.Name);
				if (Mapper.UseCaseInsensitiveMapping)
				{
					columnName = columnName.ToLowerInvariant();
				}
				columnNameToPropertyInfo.Add(columnName, prop);
			}

			// SequenceNameOrIdentityFn is only left at non-null when there is a single PK
			if (SequenceNameOrIdentityFn != null)
			{
				pkProperty = columnNameToPropertyInfo[PrimaryKeyFields];
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

#region Shared initialiser
		// sequence is for sequence-based databases (Oracle, PostgreSQL); there is no default sequence, specify either null or empty string to disable and manually specify your PK values;
		// for non-sequence-based databases, in unusual cases, you may specify this to specify an alternative key retrieval function
		// (e.g. for example to use @@IDENTITY instead of SCOPE_IDENTITY(), in the case of SQL Server CE)
		// primaryKeyFields is a comma separated list; if it has more than one column, you cannot specify sequence or keyRetrievalFunction
		// (if neither sequence nor keyRetrievalFunction are set (which is always the case for compound primary keys), you MUST specify non-null, non-default values for every column in your primary key
		// before saving an object)
		//
		// TO DO: disallow, or just ignore?, sequence spec when we have multiple PKs
		//
		public void Init(string connectionString,
						 string tableName,
						 string tableClassName,
						 string primaryKeyField,
						 string valueField,
						 string sequence,
						 string columns,
						 Validator validator,
						 SqlNamingMapper mapper,
						 Profiler profiler,
						 ConnectionProvider connectionProvider)
		{
			ValueField = valueField;

			if (mapper == null)
			{
				mapper = new SqlNamingMapper();
			}

			if (tableName != null)
			{
				// an empty string can be used to force not using the sub-class name
				TableName = tableName == "" ? null : tableName;
			}
			else if (tableClassName != null)
			{
				TableName = mapper.GetTableNameFromClassName(tableClassName);
			}

			if (TableName != null)
			{
				int i = TableName.LastIndexOf('.');
				if (i >= 0)
				{
					TableOwner = TableName.Substring(0, i);
					BareTableName = TableName.Substring(i + 1);
				}
				else
				{
					BareTableName = TableName;
				}
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
			Type pluginType = connectionProvider.DatabasePluginType;
			Plugin = (DatabasePlugin)Activator.CreateInstance(pluginType, false);
			Plugin.Mighty = this;

			if (primaryKeyField == null && TableName != null)
			{
				primaryKeyField = mapper.GetPrimaryKeyNameFromClassName(TableName);
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
			DefaultColumns = columns ?? "*";
			Validator = validator ?? new Validator();
			Profiler = profiler ?? new Profiler();
			Mapper = mapper;
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
				SequenceNameOrIdentityFn = mapper.QuoteDatabaseIdentifier(sequence);
			}

			DynamicObjectWrapper = new DynamicObjectWrapper<T>(this);
		}
#endregion

		// Only properties with a non-trivial implementation are here, the rest are in the MicroORM abstract class.
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
		private void LoadTableMetaData()
		{
			var sql = Plugin.BuildTableMetaDataQuery(BareTableName, TableOwner);
			IEnumerable<dynamic> unprocessedMetaData;
			dynamic db = this;
			if (typeof(T) != typeof(object))
			{
				// we need a dynamic query, so on the generic version we create a new dynamic DB object with the same connection info
				db = new MightyORM(connectionProvider: new PresetsConnectionProvider(ConnectionString, Factory, Plugin.GetType()));
			}
			unprocessedMetaData = (IEnumerable<dynamic>)db.Query(sql, BareTableName, TableOwner);
			_TableMetaData = Plugin.PostProcessTableMetaData(unprocessedMetaData);
		}

		// fields for thread-safe initialization of TableMetaData
		// (done once or less per instance of MightyORM, so not static)
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
								LoadTableMetaData();
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

		// Only methods with a non-trivial implementation are here, the rest are in the MicroORM abstract class.
#region MircoORM interace
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
		override public object AggregateWithParams(string expression, string where = null,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args)
		{
			return ScalarWithParams(Plugin.BuildSelect(expression, CheckGetTableName(), where),
				inParams, outParams, ioParams, returnParams,
				connection, args);
		}

		// You do not have to use this - you can create new items to pass into the microORM more or less however you want.
		// The main convenience provided here is to automatically strip out any input which does not match your column names.
		// TO DO: This may be slightly dodgy because it does not get the values from the DB itself - it is possible that with the
		// correct select we can get the DB to send us the values.
		override public T NewFrom(object nameValues = null, bool addNonPresentAsDefaults = true)
		{
			object item;
			IDictionary<string, object> newItemDictionary = null;
			if (typeof(T) == typeof(object))
			{
				item = new ExpandoObject();
				newItemDictionary = ((ExpandoObject)item).AsDictionary();
			}
			else
			{
				item = new T();
			}
			var nvtEnumerator = new NameValueTypeEnumerator(nameValues);
			// drive the loop by the actual column names
			foreach (var columnInfo in TableMetaData)
			{
				string columnName = columnInfo.COLUMN_NAME;
				PropertyInfo prop = null;
				if (typeof(T) != typeof(object))
				{
					columnNameToPropertyInfo.TryGetValue(columnName, out prop);
					if (prop == null) continue;
				}
				object value = null;
				foreach (var nvtInfo in nvtEnumerator)
				{
					if (nvtInfo.Name.Equals(columnName, Mapper.UseCaseInsensitiveMapping ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
					{
						value = nvtInfo.Value;
						break;
					}
				}
				if (value == null)
				{
					if (!addNonPresentAsDefaults) continue;
					value = GetColumnDefault(columnName);
				}
				if (value != null)
				{
					if (prop != null) prop.SetValue(item, value.ChangeType(prop.PropertyType));
					else newItemDictionary.Add(columnName, value);
				}
			}
			return (T)item;
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
		/// TO DO: Should this actually be used for checking whether PKs are at their default values?
		/// I would say probably not.
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
		override protected string CheckGetPrimaryKeyFields()
		{
			if (string.IsNullOrEmpty(PrimaryKeyFields))
			{
				throw new InvalidOperationException("No primary key field(s) have been specified");
			}
			return PrimaryKeyFields;
		}

		/// <summary>
		/// Return comma-separated list of primary key fields, raising an exception if there are none.
		/// </summary>
		/// <returns></returns>
		override protected string CheckGetValueField(string message)
		{
			if (string.IsNullOrEmpty(ValueField))
			{
				throw new InvalidOperationException(message);
			}
			return ValueField;
		}

		/// <summary>
		/// Return the single (non-compound) primary key name, with meaningful exception if there isn't one.
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		override protected string CheckGetKeyName(string message)
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
		override protected string CheckGetKeyName(int i, string message)
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
		override protected string CheckGetTableName()
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
		override internal int ActionOnItems(ORMAction action, DbConnection connection, IEnumerable<object> items, out T insertedItem)
		{
			insertedItem = null;
			int count = 0;
			int affected = 0;
			Prevalidate(items, action);
			foreach (var item in items)
			{
				if (Validator.PerformingAction(item, action))
				{
					var _inserted = ActionOnItem(action, item, connection);
					if (count == 0)
					{
						insertedItem = _inserted;
					}
					Validator.PerformedAction(item, action);
					affected++;
				}
				count++;
			}
			return affected;
		}


		/// <summary>
		/// Checks that every item in the list is valid for the action to be undertaken.
		/// Normally you should not need to override this, but override <see cref="IsValidForAction" /> instead.
		/// </summary>
		/// <param name="action">The ORM action</param>
		/// <param name="items">The list of items. (Can be T, dynamic, or anything else with suitable name-value (and optional type) data in it.)</param>
		virtual internal void Prevalidate(IEnumerable<object> items, ORMAction action)
		{
			if (Validator.AutoPrevalidation == AutoPrevalidation.Off)
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
					if (Validator.AutoPrevalidation == AutoPrevalidation.TestToFirstFailure) break;
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
		override public List<object> IsValid(object item, ORMAction action = ORMAction.Save)
		{
			List<object> Errors = new List<object>();
			if (Validator != null)
			{
				Validator.ValidateForAction(item, action, Errors);
			}
			return Errors;
		}

		/// <summary>
		/// True iff input object has a named field the PK (or PKs for compound primary keys)
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
		/// <returns></returns>
		/// <remarks>
		/// TO DO: Add test for compound PK
		/// </remarks>
		override public object GetPrimaryKey(object item, bool alwaysArray = false)
		{
			var pks = new ExpandoObject();
			var pkDictionary = pks.AsDictionary();
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

		/// <summary>
		/// This will return a string/string dictionary which can be bound directly to dropdowns etc http://stackoverflow.com/q/805595/
		/// </summary>
		override public IDictionary<string, string> KeyValues(string orderBy = "")
		{
			string foo = string.Format(" to call {0}, please provide one in your constructor", nameof(KeyValues));
			string valueField = CheckGetValueField(string.Format("ValueField is required{0}", foo));
			string primaryKeyField = CheckGetKeyName(string.Format("A single primary key must be specified{0}", foo));
			var results = All(orderBy: orderBy, columns: string.Format("{0}, {1}", primaryKeyField, valueField)).Cast<IDictionary<string, object>>();
			return results.ToDictionary(item => item[primaryKeyField].ToString(), item => item[valueField].ToString());
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
		/// TO DO: Cancel the above!
		/// </remarks>
		override public PagedResults<T> PagedFromSelect(string columns, string tablesAndJoins, string where, string orderBy,
			int pageSize = 20, int currentPage = 1,
			DbConnection connection = null,
			params object[] args)
		{
			int limit = pageSize;
			int offset = (currentPage - 1) * pageSize;
			if (columns == null) columns = DefaultColumns;
			var pagingQueryPair = Plugin.BuildPagingQueryPair(columns, tablesAndJoins, where, orderBy, limit, offset);
			var result = new PagedResults<T>();
			result.TotalRecords = Convert.ToInt32(Scalar(pagingQueryPair.CountQuery));
			result.TotalPages = (result.TotalRecords + pageSize - 1) / pageSize;
			result.Items = Query(pagingQueryPair.PagingQuery);
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
			var resultDictionary = e.AsDictionary();
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
		/// <remarks>TO DO(?): May require LIMIT (although I think this was really mainly for Single support on Massive)</remarks>
		override public IEnumerable<T> AllWithParams(
			string where = null, string orderBy = null, string columns = null, int limit = 0,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args)
		{
			if (columns == null)
			{
				columns = DefaultColumns;
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
			if (behavior == CommandBehavior.Default && typeof(X) == typeof(T))
			{
				// (= single result set, not single row...)
				behavior = CommandBehavior.SingleResult;
			}
			// using applied only to local connection
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

							if (useReader.Read())
							{
								bool useExpando = (typeof(T) == typeof(object));

								int fieldCount = useReader.FieldCount;
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
									var columnName = useReader.GetName(i);
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
										// leaves as null if no match
										columnNameToPropertyInfo.TryGetValue(columnName, out propertyInfo[i]);
									}
								}
								do
								{
									useReader.GetValues(rowValues);
									if (useExpando)
									{
										ExpandoObject e = new ExpandoObject();
										IDictionary<string, object> d = e.AsDictionary();
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
								} while (useReader.Read());
							}
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
		private T ActionOnItem(ORMAction action, object item, DbConnection connection)
		{
			int nKeys = 0;
			int nDefaultKeyValues = 0;
			// TO DO(?): Only create and append to these lists conditional upon potential need
			List<string> insertNames = new List<string>();
			List<string> insertValues = new List<string>(); // list of param names, not actual values
			List<string> updateNameValuePairs = new List<string>();
			List<string> whereNameValuePairs = new List<string>();
			var argsItem = new ExpandoObject();
			var argsItemDict = argsItem.AsDictionary();
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
					// Sending NULL in the SQL, and not in a param, is *required* for obscure cases (such as SQL Server Image) where the column will not accept a varchar NULL param
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
					command = CreateUpdateCommand(argsItem, updateNameValuePairs, whereNameValuePairs);
					break;

				case ORMAction.Insert:
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

				case ORMAction.Delete:
					command = CreateDeleteCommand(argsItem, whereNameValuePairs);
					break;

				default:
					// use 'Exception' for strictly internal/should not happen/our fault exceptions
					throw new Exception("incorrect " + nameof(ORMAction) + "=" + action + " at action choice in " + nameof(ActionOnItem));
			}
			command.Connection = connection;
			if (action == ORMAction.Insert && SequenceNameOrIdentityFn != null)
			{
				// All DBs return a massive size for their identity by default, we are normalising to int
				var pk = Convert.ToInt32(Scalar(command));
				var result = UpsertItemPK(item, pk);
				// return value not used if we were originally a Save (so in that case user
				// must send in a mutable object if they want to see the updated PK)
				return (T)result;
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
			Plugin.FixupPagingCommand(command);
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
		private object UpsertItemPK(object item, object pk)
		{
			if (typeof(T) == typeof(object))
			{
				var itemAsExpando = item as ExpandoObject;
				if (itemAsExpando != null)
				{
					var dict = itemAsExpando.AsDictionary();
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
				// non-conflicting local scope
				{
					// Convert POCO to expando
					var result = item.ToExpando();
					var dict = result.AsDictionary();
					dict[PrimaryKeyFields] = pk;
					return result;
				}
			}
			else
			{
				var itemT = item as T;
				if (itemT == null)
				{
					itemT = NewFrom(item);
				}
				pkProperty.SetValue(itemT, pk);
				return itemT;
			}
		}

		/// <summary>
		/// Is the string passed in the name of a PK field?
		/// </summary>
		/// <param name="fieldName">The name to check</param>
		/// <param name="canonicalKeyName">Return the canonical key name</param>
		/// <returns></returns>
		internal bool IsKey(string fieldName, out string canonicalKeyName)
		{
			canonicalKeyName = null;
			foreach (var key in PrimaryKeyList)
			{
				if (key.Equals(fieldName, StringComparison.OrdinalIgnoreCase))
				{
					canonicalKeyName = fieldName;
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Is the string passed in the name of a PK field?
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