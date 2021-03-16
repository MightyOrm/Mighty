using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Mighty.ConnectionProviders;
using Mighty.Plugins;

namespace Mighty.DataContracts
{
    /// <summary>
    /// Cache table meta data so we don't do loads of unecessary lookups
    /// </summary>
    public sealed partial class TableMetaDataStore
    {
        // Singleton pattern: https://csharpindepth.com/Articles/Singleton#lazy

        /// <summary>
        /// Lazy initialiser
        /// </summary>
        private static readonly Lazy<TableMetaDataStore> lazy = new Lazy<TableMetaDataStore>(() => new TableMetaDataStore());

        /// <summary>
        /// Singleton instance
        /// </summary>
        public static TableMetaDataStore Instance { get { return lazy.Value; } }

        /// <summary>
        /// Private constructor
        /// </summary>
        private TableMetaDataStore() {}

        /// <summary>
        /// The store
        /// </summary>
        private readonly ConcurrentDictionary<TableMetaDataKey, IEnumerable<dynamic>> store = new ConcurrentDictionary<TableMetaDataKey, IEnumerable<dynamic>>();

        /// <summary>
        /// Cache size
        /// </summary>
        public int CacheSize { get { return store.Count; } }

        /// <summary>
        /// Cache hits
        /// </summary>
        public int CacheHits { get; private set; }

        /// <summary>
        /// Sync cache hits
        /// </summary>
        public int SyncCacheHits { get; private set; }

        /// <summary>
        /// Cache misses
        /// </summary>
        public int CacheMisses { get; private set; }

        /// <summary>
        /// Sync cache misses
        /// </summary>
        public int SyncCacheMisses { get; private set; }

        /// <summary>
        /// Remove all stored table meta-data
        /// </summary>
        public void Flush()
        {
            store.Clear();
        }

        internal IEnumerable<dynamic> Get(
            bool IsGeneric, PluginBase Plugin, DbProviderFactory Factory, DbConnection connection,
            string BareTableName, string TableOwner, DataContract DataContract, dynamic Mighty
            )
        {
            string connectionString =
                connection == null ?
                    Mighty.ConnectionString :
                    connection.ConnectionString;

            if (connectionString == null)
            {
                throw new Exception($"No {nameof(DbConnection)} and no local or global connection string available when fetching table metadata");
            }

            // !IsGeneric does not need to be in the key, because it determines how the data is
            // fetched (do we need to create a new, dynamic instance?), but not what is fetched.
            TableMetaDataKey key = new TableMetaDataKey(
                Plugin, Factory, connectionString,
                BareTableName, TableOwner, DataContract
            );
            CacheHits++;
            SyncCacheHits++;
            return store.GetOrAdd(key, k => {
                CacheHits--;
                CacheMisses++;
                SyncCacheHits--;
                SyncCacheMisses++;
                return LoadTableMetaData(IsGeneric, k, Mighty, connection);
            });
        }

        private IEnumerable<dynamic> LoadTableMetaData(bool isGeneric, TableMetaDataKey key, dynamic Mighty, DbConnection connection)
        {
            var sql = key.Plugin.BuildTableMetaDataQuery(key.BareTableName, key.TableOwner);
            IEnumerable<dynamic> unprocessedMetaData;
            dynamic db = Mighty;
            if (isGeneric)
            {
                // we need a dynamic query, so on the generic version we create a new dynamic DB object with the same connection info
                db = new MightyOrm(connectionProvider: new PresetsConnectionProvider(connection == null ? key.ConnectionString : null, key.Factory, key.Plugin.GetType()));
            }
            unprocessedMetaData = (IEnumerable<dynamic>)db.Query(sql, connection, key.BareTableName, key.TableOwner);
            var postProcessedMetaData = key.Plugin.PostProcessTableMetaData(unprocessedMetaData);
            if (postProcessedMetaData.Count == 0)
            {
                throw new InvalidOperationException($"Cannot find any meta-data for table {(key.TableOwner == null ? "" : $"{key.TableOwner }.")}{key.BareTableName} in database");
            }
            return FilterTableMetaData(key, postProcessedMetaData);
        }

        /// <summary>
        /// We drive creating new objects by the table meta-data list, but we only want to add columns which are actually
        /// specified for this instance of Mighty
        /// </summary>
        /// <param name="key">The info needed to create the meta-data</param>
        /// <param name="tableMetaData">The table meta-data</param>
        /// <returns></returns>
        private IEnumerable<dynamic> FilterTableMetaData(TableMetaDataKey key, IEnumerable<dynamic> tableMetaData)
        {
            foreach (var columnInfo in tableMetaData)
            {
                columnInfo.IS_MIGHTY_COLUMN = key.DataContract.IsMightyColumn(columnInfo.COLUMN_NAME);
            }
            return tableMetaData;
        }
    }
}
