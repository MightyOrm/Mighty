#if NETFRAMEWORK
using System;
using System.Configuration;
using System.Data.Common;

namespace Mighty.ConnectionProviders
{
    internal class ConfigFileConnectionProvider : ConnectionProvider
    {
        // fluent API
        override public ConnectionProvider Init(string connectionStringName, string providerName)
        {
            ConnectionStringSettings connectionStringSettings = GetConnectionStringSettings(connectionStringName);
            if (connectionStringSettings != null)
            {
                ConnectionString = connectionStringSettings.ConnectionString;
                string finalProviderName = providerName ?? connectionStringSettings.ProviderName;
                if (finalProviderName != null)
                {
                    DatabasePluginType = MightyProviderFactories.GetDatabasePluginAsType(finalProviderName);
                    ProviderFactoryInstance = DbProviderFactories.GetFactory(finalProviderName);
                }
            }
            return this;
        }

        // null may be passed in, to request the first non-Machine.config connection string, if there is one
        private ConnectionStringSettings GetConnectionStringSettings(string connectionStringName)
        {
            ConnectionStringSettings connectionStringSettings;
            if (connectionStringName == null)
            {
                // http://stackoverflow.com/a/4681754/
                var machineConfigCount = ConfigurationManager.OpenMachineConfiguration().ConnectionStrings.ConnectionStrings.Count;
                if (ConfigurationManager.ConnectionStrings.Count <= machineConfigCount)
                {
                    throw new InvalidOperationException("No user-configured connection string available");
                }
                connectionStringSettings = ConfigurationManager.ConnectionStrings[machineConfigCount];
            }
            else
            {
                // Result will be null if there is no such connection string name;
                // MightyOrm constructor will then switch to using the PureConnectionStringProvider instead
                connectionStringSettings = ConfigurationManager.ConnectionStrings[connectionStringName];
            }
            return connectionStringSettings;
        }
    }
}
#endif
