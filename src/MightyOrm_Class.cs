#pragma warning disable IDE0079
#pragma warning disable IDE0057
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
using Mighty.DataContracts;
using Mighty.Plugins;
using Mighty.Interfaces;
using Mighty.Mapping;
using Mighty.Parameters;
using Mighty.Profiling;
using Mighty.Validation;

namespace Mighty
{
    /// <summary>
    /// In order to most simply support generics, the dynamic version of Mighty has to be a sub-class of the generic version, but of course the dynamic version is still the nicest version to use! :)
    /// </summary>
    public class MightyOrm : MightyOrm<dynamic>
    {
        #region Constructor
#if KEY_VALUES
        /// <summary>
        /// Constructor for dynamic instances of <see cref="MightyOrm"/>.
        /// </summary>
        /// <param name="connectionString">
        /// Connection string, with additional Mighty-specific support for non-standard "ProviderName=" property
        /// within the connection string itself.
        /// On .NET Framework (but not .NET Core) this can instead be a connection string name, in which case the
        /// connection string itself and provider name are looked up in the ConnectionStrings section of the .config file.
        /// </param>
        /// <param name="tableName">Table name</param>
        /// <param name="primaryKeys">Either single primary key member name, or comma separated list of names for compound PK</param>
        /// <param name="valueField">Value member name, for lookup tables</param>
        /// <param name="sequence">Optional sequence name for PK inserts on sequence-based DBs; or, optionally override
        /// identity retrieval function for identity-based DBs (e.g. specify "@@IDENTITY" here for SQL Server CE). As a special case,
        /// send an empty string (i.e. not the default value of null) to turn off identity support on identity-based DBs.</param>
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
                         string primaryKeys = null,
                         string valueField = null,
                         string sequence = null,
                         string columns = null,
                         Validator validator = null,
                         SqlNamingMapper mapper = null,
                         DataProfiler profiler = null,
                         ConnectionProvider connectionProvider = null)
        {
            Init(connectionString, tableName, primaryKeys,
                valueField,
                sequence, columns, validator, mapper, profiler, connectionProvider);
        }
#else
        /// <summary>
        /// Constructor for dynamic instances of <see cref="MightyOrm"/>.
        /// </summary>
        /// <param name="connectionString">
        /// Connection string, with additional Mighty-specific support for non-standard "ProviderName=" property
        /// within the connection string itself.
        /// On .NET Framework (but not .NET Core) this can instead be a connection string name, in which case the
        /// connection string itself and provider name are looked up in the ConnectionStrings section of the .config file.
        /// </param>
        /// <param name="tableName">Table name</param>
        /// <param name="primaryKeys">Either single primary key member name, or comma separated list of names for compound PK</param>
        /// <param name="sequence">Optional sequence name for PK inserts on sequence-based DBs; or, optionally override
        /// identity retrieval function for identity-based DBs (e.g. specify "@@IDENTITY" here for SQL Server CE). As a special case,
        /// send an empty string (i.e. not the default value of null) to turn off identity support on identity-based DBs.</param>
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
                         string primaryKeys = null,
                         string sequence = null,
                         string columns = null,
                         Validator validator = null,
                         SqlNamingMapper mapper = null,
                         DataProfiler profiler = null,
                         ConnectionProvider connectionProvider = null)
        {
            Init(connectionString, tableName, primaryKeys,
                sequence, columns, validator, mapper, profiler, connectionProvider);
        }
#endif
        #endregion

        #region Convenience factory
        /// <summary>
        /// Return a new non-table specific instances of <see cref="MightyOrm"/> (equivalent to a constructor call).
        /// </summary>
        /// <param name="connectionString">
        /// Connection string, with additional Mighty-specific support for non-standard "ProviderName=" property
        /// within the connection string itself.
        /// On .NET Framework (but not .NET Core) this can instead be a connection string name, in which case the
        /// connection string itself and provider name are looked up in the ConnectionStrings section of the .config file.
        /// </param>
        /// <returns></returns>
        /// <remarks>
        /// Static, so can't be made part of any kind of interface, even though we want this on the generic and dynamic versions.
        /// I think this requires new because of the conflict with the MightyOrm&lt;T&gt; version.
        /// TO DO: check.
        /// </remarks>
        new static public MightyOrm Open(string connectionString = null)
        {
            return new MightyOrm(connectionString);
        }
        #endregion

    }

    /// <summary>
    /// Strongly typed MightyOrm instance.
    /// </summary>
    /// <typeparam name="T">The generic type for items returned by this instance</typeparam>
    public partial class MightyOrm<T> : MightyOrmAbstractInterface<T> where T : class, new()
    {
        #region Constructor
#if KEY_VALUES
        /// <summary>
        /// Constructor for strongly typed instances of <see cref="MightyOrm{T}"/>.
        /// </summary>
        /// <param name="connectionString">
        /// Connection string, with additional Mighty-specific support for non-standard "ProviderName=" property
        /// within the connection string itself.
        /// On .NET Framework (but not .NET Core) this can instead be a connection string name, in which case the
        /// connection string itself and provider name are looked up in the ConnectionStrings section of the .config file.
        /// </param>
        /// <param name="tableName">Override the table name (defaults to using T class name)</param>
        /// <param name="primaryKeys">Either single primary key member name, or comma separated list of names for compound PK</param>
        /// <param name="valueField">Value member name, for lookup tables</param>
        /// <param name="sequence">Optional sequence name for PK inserts on sequence-based DBs; or, optionally override
        /// identity retrieval function for identity-based DBs (e.g. specify "@@IDENTITY" here for SQL Server CE). As a special case,
        /// send an empty string (i.e. not the default value of null) to turn off identity support on identity-based DBs.</param>
        /// <param name="columns">Default column list</param>
        /// <param name="validator">Optional validator</param>
        /// <param name="mapper">Optional C# &lt;-&gt; SQL name mapper</param>
        /// <param name="profiler">Optional SQL profiler</param>
        /// <param name="connectionProvider">Optional connection provider (only needed for providers not yet known to MightyOrm)</param>
        public MightyOrm(string connectionString = null,
                         string tableName = null,
                         string primaryKeys = null,
                         string valueField = null,
                         string sequence = null,
                         string columns = null,
                         Validator validator = null,
                         SqlNamingMapper mapper = null,
                         DataProfiler profiler = null,
                         ConnectionProvider connectionProvider = null)
        {
            // If this has been called as part of constructing MightyOrm (non-generic), then return immediately and let that constructor do all the work
            if (this is MightyOrm) return;
            IsGeneric = true;
            Init(connectionString, tableName, primaryKeys,
                valueField,
                sequence, columns, validator, mapper, profiler, connectionProvider);
        }
#else
        /// <summary>
        /// Constructor for strongly typed instances of <see cref="MightyOrm{T}"/>.
        /// </summary>
        /// <param name="connectionString">
        /// Connection string, with additional Mighty-specific support for non-standard "ProviderName=" property
        /// within the connection string itself.
        /// On .NET Framework (but not .NET Core) this can instead be a connection string name, in which case the
        /// connection string itself and provider name are looked up in the ConnectionStrings section of the .config file.
        /// </param>
        /// <param name="table">Override the table name (defaults to using T class name)</param>
        /// <param name="primaryKeys">Either single primary key member name, or comma separated list of names for compound PK</param>
        /// <param name="sequence">Optional sequence name for PK inserts on sequence-based DBs; or, optionally override
        /// identity retrieval function for identity-based DBs (e.g. specify "@@IDENTITY" here for SQL Server CE). As a special case,
        /// send an empty string (i.e. not the default value of null) to turn off identity support on identity-based DBs.</param>
        /// <param name="columns">Default column list</param>
        /// <param name="validator">Optional validator</param>
        /// <param name="mapper">Optional C# &lt;-&gt; SQL name mapper</param>
        /// <param name="profiler">Optional SQL profiler</param>
        /// <param name="connectionProvider">Optional connection provider (only needed for providers not yet known to MightyOrm)</param>
        public MightyOrm(string connectionString = null,
                         string table = null,
                         string primaryKeys = null,
                         string sequence = null,
                         string columns = null,
                         Validator validator = null,
                         SqlNamingMapper mapper = null,
                         DataProfiler profiler = null,
                         ConnectionProvider connectionProvider = null)
        {
            // If this has been called as part of constructing MightyOrm (non-generic), then return immediately and let that constructor do all the work
            if (this is MightyOrm) return;
            IsGeneric = true;
            Init(connectionString, table, primaryKeys,
                sequence, columns, validator, mapper, profiler, connectionProvider);
        }
#endif
        #endregion

        #region Convenience factory
        /// <summary>
        /// Return a new non-table specific instances of <see cref="MightyOrm{T}"/> (equivalent to a constructor call).
        /// </summary>
        /// <param name="connectionString">
        /// Connection string, with additional Mighty-specific support for non-standard "ProviderName=" property
        /// within the connection string itself.
        /// On .NET Framework (but not .NET Core) this can instead be a connection string name, in which case the
        /// connection string itself and provider name are looked up in the ConnectionStrings section of the .config file.
        /// </param>
        /// <returns></returns>
        /// <remarks>Static, so can't be defined anywhere but here.</remarks>
        static public MightyOrm<T> Open(string connectionString = null)
        {
            return new MightyOrm<T>(connectionString);
        }
        #endregion

        #region Shared initialiser
        // sequence is for sequence-based databases (Oracle, PostgreSQL); there is no default sequence, specify either null or empty string to disable and manually specify your PK values;
        // for non-sequence-based databases, in unusual cases, you may specify this to specify an alternative key retrieval function
        // (e.g. for example to use @@IDENTITY instead of SCOPE_IDENTITY(), in the case of SQL Server CE)
        // keys is a comma separated list; if it has more than one column, you cannot specify sequence or keyRetrievalFunction
        // (if neither sequence nor keyRetrievalFunction are set (which is always the case for compound primary keys), you MUST specify non-null, non-default values for every column in your primary key
        // before saving an object)
        internal void Init(string xconnectionString,
                         string tableName,
                         string primaryKeys,
#if KEY_VALUES
                         string valueField,
#endif
                         string sequence,
                         string columns,
                         Validator xvalidator,
                         SqlNamingMapper xmapper,
                         DataProfiler xprofiler,
                         ConnectionProvider connectionProvider)
        {
            // Use the passed in item, followed by the user global default for the specific generic type, followed by
            // the user global default for untyped Mighty, followed by the default default.
            // (The second and third of these refer to the same value for dynamically typed Mighty, but not for generically typed Mighty.)
            // A null connectionString still makes sense in .NET Framework, where ConfigFileConnectionProvider will then
            // use the first user connection string from the .config file.
            string intialConnectionString = xconnectionString ?? GlobalConnectionString ?? MightyOrm.GlobalConnectionString ?? null;
            Validator = xvalidator ?? GlobalValidator ?? MightyOrm.GlobalValidator ?? new NullValidator();
            DataProfiler = xprofiler ?? GlobalDataProfiler ?? MightyOrm.GlobalDataProfiler ?? new DataProfiler();
            SqlNamingMapper = xmapper ?? GlobalSqlNamingMapper ?? MightyOrm.GlobalSqlNamingMapper ?? new SqlNamingMapper();

            // Use the user global default for the specific generic type, followed by the user global default for untyped Mighty,
            // followed by the default default.
            // Note that these are not passed in to the constructor, but unlike the above four these two do have public setters.
            NpgsqlAutoDereferenceCursors = GlobalNpgsqlAutoDereferenceCursors ?? MightyOrm.GlobalNpgsqlAutoDereferenceCursors ?? true;
            NpgsqlAutoDereferenceFetchSize = GlobalNpgsqlAutoDereferenceFetchSize ?? MightyOrm.GlobalNpgsqlAutoDereferenceFetchSize ?? 10000;

            SetupConnection(intialConnectionString, connectionProvider);

            Type mappingClass;
            if (IsGeneric)
            {
                mappingClass = typeof(T);
            }
            else
            {
                mappingClass = this.GetType();
            }

            // Get reflected column mapping info for this type + everything else which matters (from cache if possible)
            // (columns passed in here are only ever used if the auto-mapping settings imply that they are field/prop names)
            DataContract = DataContractStore.Instance.Get(IsGeneric, mappingClass, columns, SqlNamingMapper);

            DefaultColumns = DataContract.Map(AutoMap.Columns, columns) ?? DataContract.ReadColumns ?? "*";

            // This stuff is just recalculated, not cached
            SetTableNameAndOwner(DataContract, tableName);
            PrimaryKeyInfo = new Keys.PrimaryKeyInfo(IsGeneric, DataContract, Plugin, mappingClass, SqlNamingMapper, primaryKeys, sequence);
#if KEY_VALUES
            ValueColumn = DataContract.Map(AutoMap.Value, valueField);
#endif

            // Init for lazy load of table meta-data (from cache if possible; only if needed)
            InitTableMetaDataLazyLoader();

#if DYNAMIC_METHODS
            // Add dynamic method support (mainly for compatibility with Massive)
            // TO DO: This line probably shouldn't be here, as it's so intimately tied to code in DynamicMethodProvider
            DynamicObjectWrapper = new DynamicMethodProvider<T>(this);
#endif
        }

        /// <summary>
        /// Set table name, and from that bare table name and table owner.
        /// </summary>
        /// <param name="dataContract">The class data contract (may include a table name override from attributes or mapper)</param>
        /// <param name="tableName">The table name from the constructor</param>
        private void SetTableNameAndOwner(DataContract dataContract, string tableName)
        {
            if (tableName != null)
            {
                // this line allows an empty string table name in the constructor to be used to force not using any table name
                // TO DO: Test. (Not sure exactly why I thought this was worth doing, but no table name is handled cleanly, so I guess why not?)
                tableName = tableName == "" ? null : tableName;
            }
            else
            {
                // this will still be null if neither attributes nor mapper have overridden their default behaviour
                tableName = dataContract.Key.DatabaseTableSettings.TableName;

                // if not specified any other way, use data item type class name
                if (tableName == null && dataContract.Key.DataItemType != typeof(MightyOrm))
                {
                    tableName = dataContract.Key.DataItemType.Name;
                }
            }

            if (tableName != null)
            {
                TableName = tableName;
                int i = tableName.LastIndexOf('.');
                if (i >= 0)
                {
                    // leave this at null if there is no table owner part of TableName
                    TableOwner = tableName.Substring(0, i);
                }
                // this will always be non-null if TableName is
                BareTableName = tableName.Substring(i + 1);
            }
        }

        private void SetupConnection(string connectionString, ConnectionProvider connectionProvider)
        {
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
            Factory = DataProfiler.FactoryWrapping(Factory);
            Type pluginType = connectionProvider.DatabasePluginType;
            Plugin = (PluginBase)Activator.CreateInstance(pluginType, false);
            Plugin.Mighty = this;
        }
        #endregion

        // Only properties with a non-trivial implementation are here, the rest are in the MightyOrm_Properties file.
        #region Properties
        /// <summary>
        /// Lazy initialiser
        /// </summary>
        private Lazy<IEnumerable<dynamic>> _TableMetaDataLazy;

#if NET40
        /// <summary>
        /// Table meta data (filtered to only contain columns specific to generic type T, or to constructor `columns`, if either is present).
        /// </summary>
        /// <remarks>
        /// Note that this does a synchronous database SELECT on first access, and the result is then cached.
        /// Non-locking caching is used: a cached result will be returned after the first such SELECT to complete has finished.
        /// </remarks>
        override public IEnumerable<dynamic> TableMetaData { get { return _TableMetaDataLazy.Value; } }
#else
        /// <summary>
        /// Table meta data (filtered to only contain columns specific to generic type T, or to constructor `columns`, if either is present).
        /// </summary>
        /// <remarks>
        /// Note that this does a synchronous database SELECT on first access, and the result is then cached.
        /// Use <see cref="GetTableMetaDataAsync()"/> for async acccess.
        /// Non-locking caching is used: a cached result will be returned after the first such SELECT to complete has finished.
        /// </remarks>
        override public IEnumerable<dynamic> TableMetaData { get { return _TableMetaDataLazy.Value; } }
#endif

        private void InitTableMetaDataLazyLoader()
        {
            _TableMetaDataLazy = new Lazy<IEnumerable<dynamic>>(() =>
            {
                if (string.IsNullOrEmpty(ConnectionString))
                {
#if NET40
                    throw new InvalidOperationException($"Cannot use {nameof(MightyOrm.TableMetaData)} property on instance of Mighty with no available connection string; provide connection string or use {nameof(MightyOrm.GetTableMetaData)} method instead");
#else
                    throw new InvalidOperationException($"Cannot use {nameof(MightyOrm.TableMetaData)} property on instance of Mighty with no available connection string; provide connection string or use {nameof(MightyOrm.GetTableMetaData)} or {nameof(MightyOrm.GetTableMetaDataAsync)} methods instead");
#endif
                }
                return TableMetaDataStore.Instance.Get(
                    IsGeneric, Plugin, Factory, null,
                    BareTableName, TableOwner, DataContract,
                    this);
            });
        }
        #endregion

        // Only methods with a non-trivial implementation are here, the rest are in the MightyOrm_Redirects file.
        #region MicroORM interface
#if KEY_VALUES
        /// <summary>
        /// Return value column, raising an exception if not specified.
        /// </summary>
        /// <returns></returns>
        string CheckGetValueColumn(string partialMessage)
        {
            if (string.IsNullOrEmpty(ValueColumn))
            {
                throw new InvalidOperationException($"{nameof(ValueColumn)} is required{partialMessage}");
            }
            return ValueColumn;
        }
#endif

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

        internal void ExceptionOnDbConnectionOrNull(object item)
        {
            if (item is null)
            {
                throw new InvalidOperationException($"Incorrect method overload used? Found null object instead of data item. If correct method overload used, remove null data items from list before call as they specify no action and trigger this warning!");
            }
            if (item is DbConnection)
            {
                throw new InvalidOperationException($"Incorrect method overload has been used: found {nameof(DbConnection)} object instead of data item");
            }
        }

        /// <summary>
        /// Checks that every item in the list is valid for the action to be undertaken.
        /// Normally you should not need to override this, but override <see cref="Validator.ValidateForAction"/>
        /// or <see cref="Validator.Validate"/> instead.
        /// </summary>
        /// <param name="action">The ORM action</param>
        /// <param name="items">The list of items. (Can be T, dynamic, or anything else with suitable name-value (and optional type) data in it.)</param>
        virtual internal void ValidateAction(IEnumerable<object> items, OrmAction action)
        {
            if (Validator.PrevalidationType == Prevalidation.Off)
            {
                return;
            }
            // Intention of non-shared error list is thread safety
            List<object> Errors = new List<object>();
            bool valid = true;
            foreach (var item in items)
            {
                // best not to pass this to any validator
                ExceptionOnDbConnectionOrNull(item);

                int oldCount = Errors.Count;
                Validator.ValidateForAction(action, item, o => Errors.Add(o));
                if (Errors.Count > oldCount)
                {
                    valid = false;
                    if (Validator.PrevalidationType == Prevalidation.Lazy) break;
                }
            }
            if (valid == false || Errors.Count > 0)
            {
                throw new ValidationException(Errors, "Prevalidation failed for one or more items for " + action);
            }
        }

        /// <summary>
        /// Is the passed in item valid against the current validator?
        /// </summary>
        /// <param name="item">The item</param>
        /// <param name="action">Optional action type (defaults to <see cref="OrmAction.Save"/>)</param>
        /// <returns></returns>
        override public List<object> IsValid(object item, OrmAction action = OrmAction.Save)
        {
            List<object> Errors = new List<object>();
            if (Validator != null)
            {
                Validator.ValidateForAction(action, item, o => Errors.Add(o));
            }
            return Errors;
        }

        /// <summary>
        /// True only if the passed in object contains field(s) matching the named primary key(s) of the current table.
        /// </summary>
        /// <param name="item">Item to check</param>
        /// <returns></returns>
        override public bool HasPrimaryKey(object item)
        {
            return PrimaryKeyInfo.HasPrimaryKey(item);
        }

        /// <summary>
        /// Return primary key for item, as a single object for simple PK, or as object[] for compound PK.
        /// </summary>
        /// <param name="item">The item</param>
        /// <param name="alwaysArray">If true return object[] of 1 item, even for simple PK</param>
        /// <returns></returns>
        override public object GetPrimaryKey(object item, bool alwaysArray = false)
        {
            return PrimaryKeyInfo.GetPrimaryKey(item, alwaysArray);
        }
        #endregion

        // Only methods with a non-trivial implementation are here, the rest are in the DataAccessWrapper abstract class.
        #region DataAccessWrapper interface
        /// <summary>
        /// Create command, setting any provider specific features which we assume elsewhere.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <returns></returns>
        internal DbCommand CreateCommand(string sql)
        {
            var command = Factory.CreateCommand();
            command = DataProfiler.CommandWrapping(command);
            Plugin.SetProviderSpecificCommandProperties(command);
            command.CommandText = sql;
            return command;
        }

        /// <summary>
        /// Create a general-purpose <see cref="DbCommand"/> with named parameters ready for use with Mighty.
        /// Manually creating commands is an advanced use-case; standard Mighty methods create and dispose
        /// of required <see cref="DbCommand"/> and <see cref="DbConnection"/> objects for you.
        /// You should use one of the variants of <see cref="CreateCommand(string, object[])"/>
        /// for all commands passed in to Mighty, since on some providers this sets provider specific properties which are needed to ensure expected behaviour with Mighty.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="inParams">Named input parameters</param>
        /// <param name="outParams">Named output parameters</param>
        /// <param name="ioParams">Named input-output parameters</param>
        /// <param name="returnParams">Named return parameters</param>
        /// <param name="isProcedure">Is the SQL a stored procedure name (with optional argument spec) only?</param>
        /// <param name="connection">The connection to use</param>
        /// <param name="args">Auto-numbered parameters for the SQL</param>
        /// <returns></returns>
        override public DbCommand CreateCommandWithParams(string sql,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null, bool isProcedure = false,
            DbConnection connection = null,
            params object[] args)
        {
            return CreateCommandWithParamsAndRowCountCheck(sql,
                inParams, outParams, ioParams, returnParams, isProcedure,
                connection,
                args).Item1;
        }

// We are getting "IDE0059 Value assigned to 'hasRowCountParams' is never used" for this method
// (at least in VS 2019 preview), but this is simply wrong; the value is returned and used.
#pragma warning disable IDE0059 // Value assigned is never used
        /// <summary>
        /// Create command with named, typed, directional parameters.
        /// </summary>
        protected Tuple<DbCommand, bool> CreateCommandWithParamsAndRowCountCheck(string sql,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null, bool isProcedure = false,
            DbConnection connection = null,
            params object[] args)
        {
            bool hasRowCountParams = false;
            var command = CreateCommand(sql);
            command.Connection = connection;
            if (isProcedure) command.CommandType = CommandType.StoredProcedure;
            AddParams(command, args);
            AddNamedParams(command, inParams, ParameterDirection.Input);
            hasRowCountParams = AddNamedParams(command, outParams, ParameterDirection.Output);
            AddNamedParams(command, ioParams, ParameterDirection.InputOutput);
            AddNamedParams(command, returnParams, ParameterDirection.ReturnValue);
            return new Tuple<DbCommand, bool>(command, hasRowCountParams);
        }
#pragma warning restore IDE0059

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
                    if (Plugin.IsCursor(param)) value = new Cursor(value);
                    resultDictionary.Add(name, value == DBNull.Value ? null : value);
                }
            }
            return e;
        }

        /// <summary>
        /// Add Execute results for <see cref="RowCount"/> parameters.
        /// </summary>
        /// <param name="rowCount">The row count</param>
        /// <param name="outParams">The list of output parameters</param>
        /// <param name="results">The results object to add to</param>
        protected void AppendRowCountResults(int rowCount, object outParams, dynamic results)
        {
            var dictionary = ((ExpandoObject)results).ToDictionary();
            foreach (var paramInfo in new NameValueTypeEnumerator(DataContract, outParams, ParameterDirection.Input))
            {
                if (paramInfo.Value is RowCount)
                {
                    dictionary.Add(paramInfo.Name, rowCount);
                }
            }
        }
        #endregion

        #region ORM actions
        /// <summary>
        /// Create update command
        /// </summary>
        /// <param name="item">The item which contains the update values</param>
        /// <param name="updateNameValuePairs">The columns to update (with values as SQL params)</param>
        /// <param name="whereNameValuePairs">The columns which specify what to update (with values as SQL params)</param>
        /// <returns></returns>
        private DbCommand CreateUpdateCommand(object item, List<string> updateNameValuePairs, List<string> whereNameValuePairs)
        {
            string sql = Plugin.BuildUpdate(TableName, string.Join(", ", updateNameValuePairs), string.Join(" AND ", whereNameValuePairs));
            return CreateCommandWithParams(sql, inParams: item);
        }

        /// <summary>
        /// Create insert command
        /// </summary>
        /// <param name="item">The item containing the update values</param>
        /// <param name="insertNames">The names of the columns to update</param>
        /// <param name="insertValues">The values (as SQL parameters) of the columns to update</param>
        /// <param name="pkFilter">The PK filter setting</param>
        /// <returns></returns>
        private DbCommand CreateInsertCommand(object item, List<string> insertNames, List<string> insertValues, PkFilter pkFilter)
        {
            string sql = Plugin.BuildInsert(TableName, string.Join(", ", insertNames), string.Join(", ", insertValues));
            if (PrimaryKeyInfo.SequenceNameOrIdentityFunction != null)
            {
                sql += ";\r\n" +
                       (Plugin.IsSequenceBased ? Plugin.BuildCurrvalSelect(PrimaryKeyInfo.SequenceNameOrIdentityFunction) : string.Format("SELECT {0}", PrimaryKeyInfo.SequenceNameOrIdentityFunction)) +
                       ";";
            }
            var command = CreateCommand(sql);
            AddNamedParams(command, item, pkFilter: pkFilter);
            if (PrimaryKeyInfo.SequenceNameOrIdentityFunction != null)
            {
                Plugin.FixupInsertCommand(command);
            }
            return command;
        }

        /// <summary>
        /// Create delete command
        /// </summary>
        /// <param name="item">The item containg the param values</param>
        /// <param name="whereNameValuePairs">The column names (and values as SQL params) specifying what to delete</param>
        /// <returns></returns>
        private DbCommand CreateDeleteCommand(object item, List<string> whereNameValuePairs)
        {
            string sql = Plugin.BuildDelete(TableName, string.Join(" AND ", whereNameValuePairs));
            var command = CreateCommand(sql);
            AddNamedParams(command, item, pkFilter: PkFilter.KeysOnly);
            return command;
        }

        /// <summary>
        /// Create the <see cref="DbCommand"/> for an ORM action on an item
        /// </summary>
        /// <param name="originalAction">The action</param>
        /// <param name="item">The item</param>
        /// <param name="revisedAction">The original action unless that was <see cref="OrmAction.Save"/>, in which case this will become <see cref="OrmAction.Insert"/> or <see cref="OrmAction.Update"/></param>
        /// <returns></returns>
        private DbCommand CreateActionCommand(OrmAction originalAction, object item, out OrmAction revisedAction)
        {
            DbCommand command;

            int nKeys = 0;
            int nDefaultKeyValues = 0;

            List<string> insertNames = null;
            List<string> insertValues = null; // param names only, the params pass the actual values
            List<string> updateNameValuePairs = null;
            List<string> whereNameValuePairs = null;

            if (originalAction == OrmAction.Insert || originalAction == OrmAction.Save)
            {
                insertNames = new List<string>();
                insertValues = new List<string>();
            }
            if (originalAction == OrmAction.Update || originalAction == OrmAction.Save)
            {
                updateNameValuePairs = new List<string>();
            }
            if (originalAction == OrmAction.Delete || originalAction == OrmAction.Update || originalAction == OrmAction.Save)
            {
                whereNameValuePairs = new List<string>();
            }

            var argsItem = new ExpandoObject();
            var argsItemDict = argsItem.ToDictionary();
            var count = 0;

            foreach (var nvt in new NameValueTypeEnumerator(DataContract, item, action: originalAction))
            {
                var name = nvt.Name;
                if (name == string.Empty)
                {
                    name = PrimaryKeyInfo.CheckGetKeyName(count, "Too many values trying to map value-only object to primary key list");
                }
                else
                {
                    name = DataContract.Map(name);
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
                if (nvt.Name == null || PrimaryKeyInfo.IsKey(name))
                {
                    nKeys++;
                    if (value == null || value.Equals(nvt.Type.GetDefaultValue()))
                    {
                        nDefaultKeyValues++;
                    }

                    if (originalAction == OrmAction.Insert || originalAction == OrmAction.Save)
                    {
                        if (PrimaryKeyInfo.SequenceNameOrIdentityFunction == null)
                        {
                            insertNames.Add(name);
                            insertValues.Add(paramName);
                        }
                        else
                        {
                            if (Plugin.IsSequenceBased)
                            {
                                insertNames.Add(name);
                                insertValues.Add(string.Format(Plugin.BuildNextval(PrimaryKeyInfo.SequenceNameOrIdentityFunction)));
                            }
                        }
                    }

                    if (originalAction == OrmAction.Delete || originalAction == OrmAction.Update || originalAction == OrmAction.Save)
                    {
                        whereNameValuePairs.Add(string.Format("{0} = {1}", name, paramName));
                    }
                }
                else
                {
                    if (originalAction == OrmAction.Insert || originalAction == OrmAction.Save)
                    {
                        insertNames.Add(name);
                        insertValues.Add(paramName);
                    }

                    if (originalAction == OrmAction.Update || originalAction == OrmAction.Save)
                    {
                        updateNameValuePairs.Add(string.Format("{0} = {1}", name, paramName));
                    }
                }
                count++;
            }

            if (nKeys > 0)
            {
                if (nKeys != PrimaryKeyInfo.Count)
                {
                    throw new InvalidOperationException("All or no primary key fields must be present in item for " + originalAction);
                }
                if (nDefaultKeyValues > 0 && nDefaultKeyValues != nKeys)
                {
                    throw new InvalidOperationException("All or no primary key fields must start with their default values in item for " + originalAction);
                }
            }
            if (originalAction == OrmAction.Save)
            {
                if (nKeys > 0 && nDefaultKeyValues == 0)
                {
                    revisedAction = OrmAction.Update;
                }
                else
                {
                    revisedAction = OrmAction.Insert;
                }
            }
            else
            {
                revisedAction = originalAction;
            }

            switch (revisedAction)
            {
                case OrmAction.Update:
                    command = CreateUpdateCommand(argsItem, updateNameValuePairs, whereNameValuePairs);
                    break;

                case OrmAction.Insert:
                    if (PrimaryKeyInfo.SequenceNameOrIdentityFunction != null && Plugin.IsSequenceBased)
                    {
                        // our copy of SequenceNameOrIdentityFunction is only ever non-null when there is a non-compound PK
                        insertNames.Add(PrimaryKeyInfo.PrimaryKeyColumn);
                        // TO DO: Should there be two places for BuildNextval? (See above.) Why?
                        insertValues.Add(Plugin.BuildNextval(PrimaryKeyInfo.SequenceNameOrIdentityFunction));
                    }
                    // TO DO: Hang on, we've got a different check here from SequenceNameOrIdentityFunction != null;
                    // either one or other is right, or else some exceptions should be thrown if they come apart.
                    command = CreateInsertCommand(argsItem, insertNames, insertValues, nDefaultKeyValues > 0 ? PkFilter.NoKeys : PkFilter.DoNotFilter);
                    break;

                case OrmAction.Delete:
                    command = CreateDeleteCommand(argsItem, whereNameValuePairs);
                    break;

                default:
                    // use 'Exception' for strictly internal/should not happen/our fault exceptions
                    throw new Exception("incorrect " + nameof(OrmAction) + "=" + originalAction + " at action choice in " + nameof(ActionOnItem));
            }

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
        /// <returns>The modified item</returns>
        private object UpsertItemPK(object item, object pk, bool createIfNeeded)
        {
            // Write PK back to ExpandoObject if we can
            if (item is ExpandoObject)
            {
                var dict = ((ExpandoObject)item).ToDictionary();
                dict[PrimaryKeyInfo.PrimaryKeyColumn] = pk;
                return item;
            }
            // Write PK back to NameValueCollection if we can
            if (item is NameValueCollection)
            {
                ((NameValueCollection)item)[PrimaryKeyInfo.PrimaryKeyColumn] = pk.ToString();
                return item;
            }
            // Write PK back to POCO of type T if we can
            if (DataContract.IsManagedGenericType(item) && PrimaryKeyInfo.PrimaryKeyMemberInfo != null)
            {
                if (PrimaryKeyInfo.PrimaryKeyMemberInfo is FieldInfo)
                {
                    ((FieldInfo)PrimaryKeyInfo.PrimaryKeyMemberInfo).SetValue(item, pk);
                }
                else
                {
                    ((PropertyInfo)PrimaryKeyInfo.PrimaryKeyMemberInfo).SetValue(item, pk);
                }
                return item;
            }
            if (createIfNeeded)
            {
                // Convert POCO to expando
                var result = item.ToExpando();
                var dict = result.ToDictionary();
                dict[PrimaryKeyInfo.PrimaryKeyMemberName] = pk;
                return result;
            }
            return null;
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
                if (value is Cursor)
                {
                    // Placeholder cursor ref; we only need the value if passing in a cursor by value
                    // doesn't work on Postgres. (TO DO: What? Is this out of date?)
                    if (!Plugin.SetCursor(p, ((Cursor)value).CursorRef))
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
        /// Add auto-named parameters to a command from an array of parameter values (which typically would have been
        /// passed in to Mighty using C# parameter syntax)
        /// </summary>
        /// <param name="cmd">The command</param>
        /// <param name="args">Auto-numbered input parameters</param>
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
        /// <param name="nameValuePairs">Parameters to add (POCO, anonymous type, NameValueCollection, ExpandoObject, etc.)</param>
        /// <param name="direction">Parameter direction</param>
        /// <param name="pkFilter">Optional PK filter control</param>
        internal bool AddNamedParams(DbCommand cmd, object nameValuePairs, ParameterDirection direction = ParameterDirection.Input, PkFilter pkFilter = PkFilter.DoNotFilter)
        {
            if (nameValuePairs is DbConnection)
            {
                throw new InvalidOperationException($"Object of type {nameof(DbConnection)} found instead of {direction.ToString().ToLowerInvariant()} parameters; you need to supply missing nulls or use the `connection:` named parameter to put your connection parameter in the right place");
            }
            if (nameValuePairs == null)
            {
                // We want to return quickly in this case
                return false;
            }
            bool containsRowCount = false;
            foreach (var paramInfo in new NameValueTypeEnumerator(DataContract, nameValuePairs, direction))
            {
                if (pkFilter == PkFilter.DoNotFilter || (PrimaryKeyInfo.IsKey(paramInfo.Name) == (pkFilter == PkFilter.KeysOnly)))
                {
                    if (paramInfo.Value is RowCount)
                    {
                        if (direction != ParameterDirection.Output)
                        {
                            throw new InvalidOperationException($"${direction} ${nameof(RowCount)} parameter ${paramInfo.Name} is invalid, ${nameof(RowCount)} can only be used for ${ParameterDirection.Output} direction parameters");
                        }
                        containsRowCount = true;
                    }
                    else
                    {
                        AddParam(cmd, paramInfo.Value, paramInfo.Name, direction, paramInfo.Type);
                    }
                }
            }
            return containsRowCount;
        }

        /// <summary>
        /// Produce WHERE clause and inParams or args from either name-value collection or primary key value-only collection
        /// </summary>
        /// <param name="whereParams">Name-value or value-only params</param>
        /// <returns>WHERE; inParams; args</returns>
        internal Tuple<string, object, object[]> GetWhereSpecFromWhereParams(object whereParams)
        {
            var wherePredicates = new List<string>();
            var nameValueArgs = new ExpandoObject();
            var nameValueDictionary = nameValueArgs.ToDictionary();

            var enumerator = new NameValueTypeEnumerator(DataContract, whereParams);

            // If no value names in the whereParams, map the values to the primary key(s)
            if (!enumerator.HasNames())
            {
                return new Tuple<string, object, object[]>(PrimaryKeyInfo.WhereForKeys(), null, PrimaryKeyInfo.KeyValuesFromKey(whereParams));
            }

            // Use (mapped) names as column names and values as values
            foreach (var paramInfo in enumerator)
            {
                string name = SqlNamingMapper.ColumnNameMapping(typeof(T), paramInfo.Name);
                wherePredicates.Add(string.Format("{0} = {1}", name, Plugin.PrefixParameterName(name)));
                nameValueDictionary.Add(name, paramInfo.Value);
            }

            var whereClause = string.Empty;
            if (wherePredicates.Count > 0)
            {
                whereClause = " WHERE " + string.Join(" AND ", wherePredicates);
            }

            return new Tuple<string, object, object[]>(whereClause, nameValueArgs, null);
        }
        #endregion
    }
}