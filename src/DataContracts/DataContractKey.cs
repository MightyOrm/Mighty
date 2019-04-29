using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

using Mighty.Mapping;
using Mighty.Plugins;

namespace Mighty.DataContracts
{
    /// <summary>
    /// A data contract key (unique identifier);
    /// all of the values on which a <see cref="DataContract"/> depends.
    /// </summary>
    public class DataContractKey
    {
        /// <summary>
        /// Is this a dynamically typed instance of <see cref="MightyOrm"/>?
        /// </summary>
        public readonly bool IsDynamic;

        /// <summary>
        /// The data item type
        /// </summary>
        public readonly Type type;

        /// <summary>
        /// The database columns to access
        /// </summary>
        public readonly string columns;

        /// <summary>
        /// The mapper
        /// </summary>
        public readonly SqlNamingMapper mapper;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="IsDynamic"></param>
        /// <param name="type"></param>
        /// <param name="columns"></param>
        /// <param name="mapper"></param>
        internal DataContractKey(bool IsDynamic, Type type, string columns, SqlNamingMapper mapper)
        {
            this.IsDynamic = IsDynamic;
            this.type = type;
            this.columns = columns;
            this.mapper = mapper;
        }

        /// <summary>
        /// Get the hash code for this key
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            var h =
                (IsDynamic ? 1 : 0) ^
                (type?.GetHashCode() ?? 0) ^
                (columns?.GetHashCode() ?? 0) ^
                (mapper.GetType()?.GetHashCode() ?? 0);
            return h;
        }

        /// <summary>
        /// Define equality between keys
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var other = obj as DataContractKey;
            if (other == null) return false;
            var y =
                IsDynamic == other.IsDynamic &&
                type == other.type &&
                columns == other.columns &&
                mapper == other.mapper;
            return y;
        }
    }
}
