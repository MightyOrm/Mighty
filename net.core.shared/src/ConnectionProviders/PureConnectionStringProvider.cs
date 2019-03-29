using System;
#if NETFRAMEWORK
using System.Data.Common;
#endif
using System.Text;

namespace Mighty.ConnectionProviders
{
	internal class PureConnectionStringProvider : ConnectionProvider
	{
#if NETFRAMEWORK
		internal bool _usedAfterConfigFile;

		// fluent API
		internal ConnectionProvider UsedAfterConfigFile()
		{
			_usedAfterConfigFile = true;
			return this;
		}
#endif

		// fluent API
		override public ConnectionProvider Init(string connectionString)
		{
			string providerName = null;
#if NETFRAMEWORK
			var extraMessage = _usedAfterConfigFile ? " (and is not a valid connection string name)" : "";
#endif
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
				throw new InvalidOperationException("Cannot parse as connection string \"" + connectionString + "\""
#if NETFRAMEWORK
					+ extraMessage
#endif
					, ex);
			}
			if (providerName == null)
			{
				throw new InvalidOperationException("Cannot find ProviderName=... in connection string passed to MightyOrm"
#if NETFRAMEWORK
					+ extraMessage
#endif
					);
			}
			DatabasePluginType = MightyProviderFactories.GetDatabasePluginAsType(providerName);
#if NETFRAMEWORK
			ProviderFactoryInstance = DbProviderFactories.GetFactory(providerName);
#else
			ProviderFactoryInstance = MightyProviderFactories.GetFactory(providerName);
#endif
			this.ConnectionString = ConnectionString.ToString();
			return this;
		}
	}
}