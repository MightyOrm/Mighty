using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

using Mighty.Plugins;

namespace Mighty.Mapping
{
    /// <summary>
    /// A data contract key (unique identifier);
    /// all of the values on which a <see cref="DataContract"/> depends.
    /// </summary>
    public class DataContractStoreKey
    {
        /// <summary>
        /// Is this a dynamically typed instance of <see cref="MightyOrm"/>?
        /// </summary>
        public readonly bool IsDynamic;

        /// <summary>
        /// The database plugin in use
        /// </summary>
        public readonly PluginBase Plugin;

        /// <summary>
        /// The db provider factory
        /// </summary>
        public readonly DbProviderFactory Factory;

        /// <summary>
        /// The connection string
        /// </summary>
        public string ConnectionString;

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
        /// <param name="Plugin"></param>
        /// <param name="Factory"></param>
        /// <param name="ConnectionString"></param>
        /// <param name="type"></param>
        /// <param name="columns"></param>
        /// <param name="mapper"></param>
        internal DataContractStoreKey(
            bool IsDynamic, PluginBase Plugin, DbProviderFactory Factory, string ConnectionString,
            Type type, string columns, SqlNamingMapper mapper)
        {
            this.IsDynamic = IsDynamic;
            this.Plugin = Plugin;
            this.Factory = Factory;
            this.ConnectionString = ConnectionString;
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
            return
                (IsDynamic ? 1 : 0) ^
                (Plugin?.GetHashCode() ?? 0) ^
                (Factory?.GetHashCode() ?? 0) ^
                (ConnectionString?.GetHashCode() ?? 0) ^
                (type?.GetHashCode() ?? 0) ^
                (columns?.GetHashCode() ?? 0) ^
                (mapper?.GetHashCode() ?? 0);
        }

        /// <summary>
        /// Define equality between keys
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var other = obj as DataContractStoreKey;
            if (other == null) return false;
            return
                IsDynamic == other.IsDynamic &&
                Plugin == other.Plugin &&
                Factory == other.Factory &&
                ConnectionString == other.ConnectionString &&
                type == other.type &&
                columns == other.columns &&
                mapper == other.mapper;
        }
    }
}
