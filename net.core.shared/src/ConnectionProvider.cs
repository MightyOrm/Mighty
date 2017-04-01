using System;

namespace Mighty
{
	public abstract class ConnectionProvider
	{
		public DbProviderFactory ProviderFactory { get; protected set; }
		public SupportedDatabase SupportedDatabase { get; protected set; }
		public string connectionString { get; protected set; }
        // fluent API, must return itself at the end
        abstract public ConnectionProvider Init(string connectionString);
	}

#if !COREFX
	internal class ConfigFileConnectionProvider : ConnectionProvider
	{
		override public void Init(string connectionStringName)
		{
			ConnectionStringSettings connectionStringSettings = GetConnectionStringSettings(connectionStringName);
			if (connectionStringSettings != null)
			{
				connectionString = connectionStringSettings.connectionString;
				string providerName = connectionStringSettings.providerName;
				if (providerName != null)
				{
					SupportedDatabase = GetSupportedDatabaseFromProviderName(providerName);
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
#endif

	internal class PureConnectionStringProvider : ConnectionProvider
	{
        internal bool _usedAsOverride;

        internal public ConnectionProvider UsedAsOverride()
        {
            _usedAsOverride = true;
            return this;
        }

		override public ConnectionProvider Init(string connectionString)
		{
			string providerName = null;
			var extraMessage = _usedAsOverride ? " (and is not a valid connection string name)" : "";
			StringBuilder connectionString = new StringBuilder();
			try
			{
				foreach (var configPair in connectionString.Split(';'))
				{
					if (!string.IsNullOrEmpty(configPair))
					{
						var keyValuePair = configPair.Split('=');
						if ("providername".Equals(keyValuePair[0], StringComparison.OrdinalIgnoreCase))
						{
							providerName = keyValuePair[1];
						}
						else
						{
							connectionString.Append(configPair);
							connectionString.Append(";");
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException("Cannot parse as connection string \"" + connectionString + "\"" + extraMessage, ex);
			}
			if (providerName == null)
			{
				throw new InvalidOperationException("Cannot find providerName=... in connection string passed to DynamicModel" + extraMessage);
			}
			SupportedDatabase = ProviderFactories.GetSupportedDb(providerName);
			ProviderFactory = ProviderFactories.GetFactory(providerName);
			ConnectionString = connectionString.ToString();
            return this;
		}
	}
}