using System;
using System.Data.Common;

namespace MightyOrm.ConnectionProviders
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
				throw new NotImplementedException();
			}
			return this;
		}
	}
}