#if !NET40
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Dasync.Collections;
using Mighty.ConnectionProviders;
using Mighty.Plugins;

namespace Mighty.DataContracts
{
    static internal class ConcurrentDictionaryExtensions
    {
        /// <summary>
        /// Adds a key/value pair to the <see cref="ConcurrentDictionary{TKey, TValue}"/> by using the specified function, 
        /// if the key does not already exist.
        /// </summary>
        /// <returns>
        /// The value for the key. This will be either the existing value for the key if the key is already in the dictionary,
        /// or the new value for the key as returned by asyncValueFactory if the key was not in the dictionary.
        /// </returns>
        public static async Task<TResult> GetOrAddAsync<TKey, TResult>(
            this ConcurrentDictionary<TKey, TResult> dict,
            TKey key, Func<TKey, Task<TResult>> asyncValueFactory)
        {
            if (dict.TryGetValue(key, out TResult resultingValue))
            {
                return resultingValue;
            }
            var newValue = await asyncValueFactory(key);
            return dict.GetOrAdd(key, newValue);
        }
    }

    /// <summary>
    /// Cache table meta data so we don't do loads of unecessary lookups
    /// </summary>
    public sealed partial class TableMetaDataStore
    {
        /// <summary>
        /// Async cache hits
        /// </summary>
        public int AsyncCacheHits { get; private set; }

        /// <summary>
        /// Async cache misses
        /// </summary>
        public int AsyncCacheMisses { get; private set; }

        internal async Task<IEnumerable<dynamic>> GetAsync(
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
            AsyncCacheHits++;
            return await store.GetOrAddAsync<TableMetaDataKey, IEnumerable<dynamic>>(key, async k => {
                CacheHits--;
                CacheMisses++;
                AsyncCacheHits--;
                AsyncCacheMisses++;
                return await LoadTableMetaDataAsync(IsGeneric, k, Mighty, connection).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        private async Task<IEnumerable<dynamic>> LoadTableMetaDataAsync(bool isGeneric, TableMetaDataKey key, dynamic Mighty, DbConnection connection)
        {
            var sql = key.Plugin.BuildTableMetaDataQuery(key.BareTableName, key.TableOwner);
            IEnumerable<dynamic> unprocessedMetaData;
            dynamic db = Mighty;
            if (isGeneric)
            {
                // we need a dynamic query, so on the generic version we create a new dynamic DB object with the same connection info
                db = new MightyOrm(connectionProvider: new PresetsConnectionProvider(connection == null ? key.ConnectionString : null, key.Factory, key.Plugin.GetType()));
            }
            unprocessedMetaData = await ((IAsyncEnumerable<dynamic>)(await db.QueryAsync(sql, connection, key.BareTableName, key.TableOwner).ConfigureAwait(false))).ToListAsync().ConfigureAwait(false);
            var postProcessedMetaData = key.Plugin.PostProcessTableMetaData(unprocessedMetaData);
            if (postProcessedMetaData.Count == 0)
            {
                throw new InvalidOperationException($"Cannot find any meta-data for table {(key.TableOwner == null ? "" : $"{key.TableOwner }.")}{key.BareTableName} in database");
            }
            return FilterTableMetaData(key, postProcessedMetaData);
        }
    }
}
#endif
