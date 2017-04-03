using System;

namespace Mighty.ConnectionProviders
{
	internal class PureConnectionStringProvider : ConnectionProvider
	{
		internal bool _usedAfterConfigFile;

		// fluent API
		internal public ConnectionProvider UsedAfterConfigFile()
		{
			_usedAfterConfigFile = true;
			return this;
		}

		// fluent API
		override public ConnectionProvider Init(string connectionString)
		{
			string providerName = null;
			var extraMessage = _usedAfterConfigFile ? " (and is not a valid connection string name)" : "";
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