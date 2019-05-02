using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

using Mighty.ConnectionProviders;
using Mighty.Plugins;

namespace Mighty.DataContracts
{
    /// <summary>
    /// Cache table meta data so we don't do loads of unecessary lookups
    /// </summary>
    public sealed class TableMetaDataStore
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
        private TableMetaDataStore()
        {
            Flush();
        }

        /// <summary>
        /// The store
        /// </summary>
        private Dictionary<TableMetaDataKey, IEnumerable<dynamic>> store;

        /// <summary>
        /// Cache hits
        /// </summary>
        public int CacheHits { get; private set; }

        /// <summary>
        /// Cache hits
        /// </summary>
        public int CacheMisses { get; private set; }

        /// <summary>
        /// Remove all stored table meta-data
        /// </summary>
        public void Flush()
        {
            store = new Dictionary<TableMetaDataKey, IEnumerable<dynamic>>();
        }

        internal IEnumerable<dynamic> Get(
            bool IsDynamic, PluginBase Plugin, DbProviderFactory Factory, string ConnectionString,
            string BareTableName, string TableOwner, ColumnsContract ColumnsContract, object Mighty
            )
        {
            // IsDynamic does not need to be in the key, because it determines how the data is
            // fetched (do we need to create a new, dynamic instance?), but not what is fetched.
            TableMetaDataKey key = new TableMetaDataKey(
                Plugin, Factory, ConnectionString,
                BareTableName, TableOwner, ColumnsContract
            );
            IEnumerable<dynamic> value;
            if (store.TryGetValue(key, out value))
            {
                CacheHits++;
            }
            else
            {
                CacheMisses++;
                value = LoadTableMetaData(IsDynamic, key, Mighty);
                store.Add(key, value);
            }
            return value;
        }

        private IEnumerable<dynamic> LoadTableMetaData(bool IsDynamic, TableMetaDataKey key, object Mighty)
        {
            var sql = key.Plugin.BuildTableMetaDataQuery(key.BareTableName, key.TableOwner);
            IEnumerable<dynamic> unprocessedMetaData;
            dynamic db = Mighty;
            if (!IsDynamic)
            {
                // we need a dynamic query, so on the generic version we create a new dynamic DB object with the same connection info
                db = new MightyOrm(connectionProvider: new PresetsConnectionProvider(key.ConnectionString, key.Factory, key.Plugin.GetType()));
            }
            unprocessedMetaData = (IEnumerable<dynamic>)db.Query(sql, key.BareTableName, key.TableOwner);
            var postProcessedMetaData = key.Plugin.PostProcessTableMetaData(unprocessedMetaData);
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
                columnInfo.IS_MIGHTY_COLUMN = key.ColumnsContract.IsMightyColumn(columnInfo.COLUMN_NAME);
            }
            return tableMetaData;
        }
    }
}
