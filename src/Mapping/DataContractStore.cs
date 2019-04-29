using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

using Mighty.Plugins;

namespace Mighty.Mapping
{
    /// <summary>
    /// <see cref="DataContract"/> store.
    /// Designed to be used as a singleton instance.
    /// </summary>
    static class DataContractStore
    {
        private static readonly Dictionary<DataContractStoreKey, DataContract> store = new Dictionary<DataContractStoreKey, DataContract>();

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
        static internal DataContract Get(
            bool IsDynamic, PluginBase Plugin, DbProviderFactory Factory, string ConnectionString,
            Type type, string columns, SqlNamingMapper mapper)
        {
            DataContractStoreKey key = new DataContractStoreKey(
                IsDynamic, Plugin, Factory, ConnectionString,
                type, columns, mapper);
            DataContract value;
            if (!store.TryGetValue(key, out value))
            {
                value = new DataContract(key);
                store.Add(key, value);
            }
            return value;
        }
    }
}
