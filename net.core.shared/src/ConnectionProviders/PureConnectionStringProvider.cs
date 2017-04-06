using System;
using System.Text;

namespace Mighty.ConnectionProviders
{
	internal class PureConnectionStringProvider : ConnectionProvider
	{
		internal bool _usedAfterConfigFile;

		// fluent API
		internal ConnectionProvider UsedAfterConfigFile()
		{
			_usedAfterConfigFile = true;
			return this;
		}

		// fluent API
		override public ConnectionProvider Init(string connectionString)
		{
			string providerName = null;
			var extraMessage = _usedAfterConfigFile ? " (and is not a valid connection string name)" : "";
			StringBuilder ConnectionString = new StringBuilder();
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
							ConnectionString.Append(configPair);
							ConnectionString.Append(";");
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
				throw new InvalidOperationException("Cannot find providerName=... in connection string passed to MightyORM" + extraMessage);
			}
			DatabasePluginType = MightyProviderFactories.GetDatabasePluginAsType(providerName);
			ProviderFactoryInstance = MightyProviderFactories.GetFactory(providerName);
			this.ConnectionString = ConnectionString.ToString();
			return this;
		}
	}
}