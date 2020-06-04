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

        /// <remarks>
        ///  - fluent API
        ///  - I think that we do want to find (and remove if present) the provider name in the connection string, even if
        ///    the user has passed in a providerName which will then override it; note that the ConfigFileConnectionProvider
        ///    has already been tried at this point (.NET Framework only)
        /// </remarks>
        override public ConnectionProvider Init(string connectionString, string providerName)
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

            // overwrite the work we've just done with the passed in value if present (see remarks at start of method)
            _providerName = providerName ?? _providerName;

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