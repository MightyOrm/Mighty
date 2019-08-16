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
            const string ProviderName = "ProviderName";
            string _providerName = null;
#if NETFRAMEWORK
            var extraMessage = _usedAfterConfigFile ? " (and is not a valid connection string name)" : "";
#endif
            StringBuilder _connectionString = new StringBuilder();
            if (connectionString != null)
            {
                try
                {
                    foreach (var configPair in connectionString.Split(';'))
                    {
                        if (!string.IsNullOrEmpty(configPair))
                        {
                            var keyValuePair = configPair.Split('=');
                            if (ProviderName.Equals(keyValuePair[0], StringComparison.OrdinalIgnoreCase))
                            {
                                _providerName = keyValuePair[1];
                            }
                            else
                            {
                                _connectionString.Append(configPair);
                                _connectionString.Append(";");
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
            }
            if (_providerName == null)
            {
                throw new InvalidOperationException($"Cannot find {ProviderName}=... in connection string passed to MightyOrm"
#if NETFRAMEWORK
                    + extraMessage
#endif
                    );
            }
            DatabasePluginType = MightyProviderFactories.GetDatabasePluginAsType(_providerName);
#if NETFRAMEWORK
            ProviderFactoryInstance = DbProviderFactories.GetFactory(_providerName);
#else
            ProviderFactoryInstance = MightyProviderFactories.GetFactory(_providerName);
#endif
            if (_connectionString.Length > 0)
                this.ConnectionString = _connectionString.ToString();

            return this;
        }
    }
}