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
    public class MetaDataKey
    {
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
        /// BareTableName
        /// </summary>
        public string BareTableName;

        /// <summary>
        /// TableOwner
        /// </summary>
        public string TableOwner;

        /// <summary>
        /// DataContract
        /// </summary>
        public DataContract DataContract;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Plugin"></param>
        /// <param name="Factory"></param>
        /// <param name="ConnectionString"></param>
        /// <param name="BareTableName"></param>
        /// <param name="TableOwner"></param>
        /// <param name="DataContract"></param>
        internal MetaDataKey(
            PluginBase Plugin, DbProviderFactory Factory, string ConnectionString,
            string BareTableName, string TableOwner, DataContract DataContract
            )
        {
            this.Plugin = Plugin;
            this.Factory = Factory;
            this.ConnectionString = ConnectionString;
            this.BareTableName = BareTableName;
            this.TableOwner = TableOwner;
            this.DataContract = DataContract;
        }

        /// <summary>
        /// Get the hash code for this key
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Other plugins of the same type are the same plugin, but user data mappers of the
        /// same type are not necessarily the same data mapper.
        /// </remarks>
        public override int GetHashCode()
        {
            var h =
                (Plugin.GetType()?.GetHashCode() ?? 0) ^
                (Factory?.GetHashCode() ?? 0) ^
                (ConnectionString?.GetHashCode() ?? 0) ^
                (BareTableName?.GetHashCode() ?? 0) ^
                (TableOwner?.GetHashCode() ?? 0) ^
                (DataContract?.GetHashCode() ?? 0);
            return h;
        }

        /// <summary>
        /// Define equality between keys
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        /// <remarks>
        /// Other plugins of the same type are the same plugin, but user data mappers of the
        /// same type are not necessarily the same data mapper.
        /// </remarks>
        public override bool Equals(object obj)
        {
            var other = obj as MetaDataKey;
            if (other == null) return false;
            var y =
                Plugin.GetType() == other.Plugin.GetType() &&
                Factory == other.Factory &&
                ConnectionString == other.ConnectionString &&
                BareTableName == other.BareTableName &&
                TableOwner == other.TableOwner &&
                DataContract == other.DataContract;
            return y;
        }
    }
}
