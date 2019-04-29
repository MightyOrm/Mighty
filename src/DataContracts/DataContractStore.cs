using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

using Mighty.Mapping;
using Mighty.Plugins;

namespace Mighty.DataContracts
{
    /// <summary>
    /// <see cref="DataContract"/> store.
    /// Designed to be used as a singleton instance.
    /// </summary>
    public sealed class DataContractStore
    {
        // Singleton pattern: https://csharpindepth.com/Articles/Singleton#lazy

        /// <summary>
        /// Lazy initialiser
        /// </summary>
        private static readonly Lazy<DataContractStore> lazy = new Lazy<DataContractStore>(() => new DataContractStore());

        /// <summary>
        /// Singleton instance
        /// </summary>
        public static DataContractStore Instance { get { return lazy.Value; } }

        /// <summary>
        /// Private constructor
        /// </summary>
        private DataContractStore() { }

        /// <summary>
        /// The store
        /// </summary>
        private readonly Dictionary<DataContractKey, DataContract> store = new Dictionary<DataContractKey, DataContract>();

        /// <summary>
        /// Cache hits
        /// </summary>
        public int CacheHits { get; private set; }

        /// <summary>
        /// Cache hits
        /// </summary>
        public int CacheMisses { get; private set; }

        /// <summary>
        /// Get (from store, or creating the first time it is needed) data contract for the type, columns spec and data mapper.
        /// </summary>
        /// <param name="IsDynamic"></param>
        /// <param name="Plugin"></param>
        /// <param name="Factory"></param>
        /// <param name="ConnectionString"></param>
        /// <param name="type"></param>
        /// <param name="columns"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        internal DataContract Get(
            bool IsDynamic, PluginBase Plugin, DbProviderFactory Factory, string ConnectionString,
            Type type, string columns, SqlNamingMapper mapper)
        {
            DataContractKey key = new DataContractKey(IsDynamic, type, columns, mapper);
            DataContract value;
            if (store.TryGetValue(key, out value))
            {
                CacheHits++;
            }
            else
            {
                CacheMisses++;
                value = new DataContract(key);
                store.Add(key, value);
            }
            return value;
        }
    }
}
