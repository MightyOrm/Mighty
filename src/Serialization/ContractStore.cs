using System;
using System.Collections.Generic;
using System.Text;

using Mighty.Interfaces;

namespace Mighty.Serialization
{
    /// <summary>
    /// <see cref="MightyDataContract"/> store.
    /// Designed to be used as a singleton instance.
    /// </summary>
    internal class ContractStore
    {
        private readonly Dictionary<MightyStoreKey, MightyDataContract> store;

        /// <summary>
        /// Constructore
        /// </summary>
        internal ContractStore()
        {
            store = new Dictionary<MightyStoreKey, MightyDataContract>();
        }

        /// <summary>
        /// Get (from store, or creating the first time it is needed) data contract for key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal MightyDataContract Get(MightyStoreKey key)
        {
            MightyDataContract value;

            if (!store.TryGetValue(key, out value))
            {
                value = new MightyDataContract(key);
                store.Add(key, value);
            }

            return value;
        }
    }
}
