using System;
using System.Data.Common;

namespace Mighty.ConnectionProviders
{
	public class PresetsConnectionProvider : ConnectionProvider
	{
		public PresetsConnectionProvider(string connectionString, DbProviderFactory providerFactoryInstance, Type databasePluginType)
		{
			ConnectionString = connectionString;
			ProviderFactoryInstance = providerFactoryInstance;
			DatabasePluginType = databasePluginType;
		}

		// fluent API
		override public ConnectionProvider Init(string connectionString)
		{
			if (connectionString != null)
			{
				// we have to ignore this - setting a GlobalConnectionString will result in that being passed in to here -
				// and we don't want to disable a general purpose connection provider from possibly taking a connection string
				// (so we don't want Mighty to not try to use GlobalConnectionString just because we have a connectionProvider, I don't think)
				////throw new InvalidOperationException($"{nameof(PresetsConnectionProvider)} does not support non-null {nameof(connectionString)}");
			}
			return this;
		}
	}
}