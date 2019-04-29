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
    public sealed class MetaDataStore
    {
        // Singleton pattern: https://csharpindepth.com/Articles/Singleton#lazy

        /// <summary>
        /// Lazy initialiser
        /// </summary>
        private static readonly Lazy<MetaDataStore> lazy = new Lazy<MetaDataStore>(() => new MetaDataStore());

        /// <summary>
        /// Singleton instance
        /// </summary>
        public static MetaDataStore Instance { get { return lazy.Value; } }

        /// <summary>
        /// Private constructor
        /// </summary>
        private MetaDataStore()
        {
            Flush();
        }

        /// <summary>
        /// The store
        /// </summary>
        private Dictionary<MetaDataKey, IEnumerable<dynamic>> store;

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
            store = new Dictionary<MetaDataKey, IEnumerable<dynamic>>();
        }

        internal IEnumerable<dynamic> Get(
            bool IsDynamic, PluginBase Plugin, DbProviderFactory Factory, string ConnectionString,
            string BareTableName, string TableOwner, DataContract DataContract, object Mighty
            )
        {
            MetaDataKey key = new MetaDataKey(
                IsDynamic, Plugin, Factory, ConnectionString,
                BareTableName, TableOwner, DataContract
                );
            IEnumerable<dynamic> value;
            if (store.TryGetValue(key, out value))
            {
                CacheHits++;
            }
            else
            {
                CacheMisses++;
                value = LoadTableMetaData(key, Mighty);
                store.Add(key, value);
            }
            return value;
        }

        // Thread-safe initialization based on Microsoft DbProviderFactories reference 
        // https://referencesource.microsoft.com/#System.Data/System/Data/Common/DbProviderFactories.cs

        // called within the lock
        private IEnumerable<dynamic> LoadTableMetaData(MetaDataKey key, object Mighty)
        {
            var sql = key.Plugin.BuildTableMetaDataQuery(key.BareTableName, key.TableOwner);
            IEnumerable<dynamic> unprocessedMetaData;
            dynamic db = Mighty;
            if (!key.IsDynamic)
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
        private IEnumerable<dynamic> FilterTableMetaData(MetaDataKey key, IEnumerable<dynamic> tableMetaData)
        {
            foreach (var columnInfo in tableMetaData)
            {
                columnInfo.IS_MIGHTY_COLUMN = key.DataContract.IsMightyColumn(columnInfo.COLUMN_NAME);
            }
            return tableMetaData;
        }
    }
}
