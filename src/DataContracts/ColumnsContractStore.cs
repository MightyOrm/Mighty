using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

using Mighty.Mapping;
using Mighty.Plugins;

namespace Mighty.DataContracts
{
    /// <summary>
    /// <see cref="ColumnsContract"/> store.
    /// Designed to be used as a singleton instance.
    /// </summary>
    public sealed class ColumnsContractStore
    {
        // Singleton pattern: https://csharpindepth.com/Articles/Singleton#lazy

        /// <summary>
        /// Lazy initialiser
        /// </summary>
        private static readonly Lazy<ColumnsContractStore> lazy = new Lazy<ColumnsContractStore>(() => new ColumnsContractStore());

        /// <summary>
        /// Singleton instance
        /// </summary>
        public static ColumnsContractStore Instance { get { return lazy.Value; } }

        /// <summary>
        /// Private constructor
        /// </summary>
        private ColumnsContractStore() { }

        /// <summary>
        /// The store
        /// </summary>
        private readonly Dictionary<ColumnsContractKey, ColumnsContract> store = new Dictionary<ColumnsContractKey, ColumnsContract>();

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
        /// <param name="mapper"></param>
        /// <returns></returns>
        internal ColumnsContract Get(
            bool IsDynamic, PluginBase Plugin, DbProviderFactory Factory, string ConnectionString,
            Type type, SqlNamingMapper mapper)
        {
            ColumnsContractKey key = new ColumnsContractKey(IsDynamic, type, mapper);
            ColumnsContract value;
            if (store.TryGetValue(key, out value))
            {
                CacheHits++;
            }
            else
            {
                CacheMisses++;
                value = new ColumnsContract(key);
                store.Add(key, value);
            }
            return value;
        }
    }
}
