#if false //!COREFX
using System;
using System.Configuration;
using System.Data;
using System.Data.Common;

namespace Mighty.ConnectionProviders
{
	internal class ConfigFileConnectionProvider : ConnectionProvider
	{
		// fluent API
		override public ConnectionProvider Init(string connectionStringName)
		{
			ConnectionStringSettings connectionStringSettings = GetConnectionStringSettings(connectionStringName);
			if (connectionStringSettings != null)
			{
				connectionString = connectionStringSettings.connectionString;
				string providerName = connectionStringSettings.providerName;
				if (providerName != null)
				{
					DatabasePluginType = MightyProviderFactories.GetDatabasePluginAsType(providerName);
					ProviderFactory = DbProviderFactories.GetFactory(providerName);
				}
			}
			return this;
		}

		private ConnectionStringSettings GetConnectionStringSettings(string connectionStringName)
		{
			ConnectionStringSettings connectionStringSettings = null;
			if (connectionStringName == null)
			{
				// http://stackoverflow.com/a/4681754/
				var machineConfigCount = System.Configuration.ConfigurationManager.OpenMachineConfiguration().ConnectionStrings.ConnectionStrings.Count;
				if (ConfigurationManager.ConnectionStrings.Count <= machineConfigCount)
				{
					throw new InvalidOperationException("No user-configured connection string available");
				}
				connectionStringSettings = ConfigurationManager.ConnectionStrings[machineConfigCount];
			}
			else
			{
				// may be null if there is no such connection string name; Massive will switch to using the pure connection string provider
				connectionStringSettings = ConfigurationManager.ConnectionStrings[connectionStringName];
			}
			return connectionStringSettings;
		}
	}
}
#endif
