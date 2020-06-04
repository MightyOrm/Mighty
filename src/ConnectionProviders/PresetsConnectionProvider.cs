using System;
using System.Data.Common;

namespace Mighty.ConnectionProviders
{
    internal class PresetsConnectionProvider : ConnectionProvider
    {
        public PresetsConnectionProvider(string connectionString, DbProviderFactory providerFactoryInstance, Type databasePluginType)
        {
            ConnectionString = connectionString;
            ProviderFactoryInstance = providerFactoryInstance;
            DatabasePluginType = databasePluginType;
        }

        /// <remarks>
        ///  - fluent API
        ///  - this is an internal class used when we need to create a new, related instance of Mighty for internal use only, in the odd
        ///    occasional situation where the current instance can't quite do what is needed, and so the exact settings required are
        ///    passed in to the constructor; if the user has set global connection string or provider name settings then these will be
        ///    passed in to this Init call, but should just be ignored
        /// </remarks>
        override public ConnectionProvider Init(string connectionString, string providerName)
        {
            return this;
        }
    }
}