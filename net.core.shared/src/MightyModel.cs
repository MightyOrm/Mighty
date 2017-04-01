using System;

namespace Mighty
{
    public class MightyModel
    {
        public MightyModel(string connectionString = null, string tableName = null, string pkName = null, ConnectionProvider connectionProvider = null)
        {
			if (connectionProvider == null)
			{
#if !COREFX
				connectionProvider = new ConfigFileConnectionProvider().Init(connectionStringOrName);
				if (connectionProvider.connectionString == null)
#endif
				{
					connectionProvider = new PureConnectionStringProvider().UsedAsOverride().Init(connectionStringOrName);
				}
			}
            else
            {
                connectionProvider.Init(connectionStringOrName);
            }

			_connectionString = connectionProvider.connectionString;
			_factory = connectionProvider.providerFactory;
            
        }
    }
}