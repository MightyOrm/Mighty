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
        /// <param name="IsGeneric"></param>
        /// <param name="type"></param>
        /// <param name="columns"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        /// <remarks>
        /// In theory, mapping depends on "PluginBase Plugin, DbProviderFactory Factory, string ConnectionString," as well - 
        /// in practice, including those would make it much harder (or impossible?) to provide the very useful
        /// <see cref="SqlNamingMapper.Map(Type, string, string)"/> feature.
        /// I think it does seem (more or less?) reasonable to suppose that any one class will only be written to one database at a time?
        /// So, TO DO:, at least we can *document* that caching works like this.
        /// </remarks>
        internal DataContract Get(bool IsGeneric, Type type, string columns, SqlNamingMapper mapper)
        {
            DataContractKey key = new DataContractKey(IsGeneric, type, columns, mapper);
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
