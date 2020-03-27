using System;
using System.Collections.Concurrent;

using Mighty.Mapping;

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
        private readonly ConcurrentDictionary<DataContractKey, DataContract> store = new ConcurrentDictionary<DataContractKey, DataContract>();

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
        /// <param name="IsGeneric"></param>
        /// <param name="type"></param>
        /// <param name="columns"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        /// <remarks>
        /// In theory, mapping depends on Plugin, Factory, and ConnectionString as well;
        /// in practice, including those would make it much harder to provide the very useful
        /// <see cref="DataContract.Map(string)"/> feature.
        /// I think it seems (more or less?) reasonable to suppose that any one class will only
        /// be read from and written one database with one mapping at a time? In fact, since
        /// Mighty only supports one mapping per class, maybe this is effectively enforced anyway?
        /// </remarks>
        internal DataContract Get(bool IsGeneric, Type type, string columns, SqlNamingMapper mapper)
        {
            DataContractKey key = new DataContractKey(IsGeneric, type, columns, mapper);
            CacheHits++;
            return store.GetOrAdd(key, k => {
                CacheHits--;
                CacheMisses++;
                return new DataContract(k);
            });
        }
    }
}
