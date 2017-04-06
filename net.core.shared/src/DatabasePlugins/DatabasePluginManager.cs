using System;
using System.Data;
using System.Collections.Generic;
using System.Reflection;

namespace Mighty.DatabasePlugins
{
	public class DatabasePluginManager
	{
		// for locking access to the list of default + registered database plugins
		private static ConnectionState _initState; // closed (default value), connecting, open
        private static List<Type> _installedPluginTypes;
        private static object _lockobj = new object();

		// Thread-safe initialization based on Microsoft DbProviderFactories reference 
		// https://referencesource.microsoft.com/#System.Data/System/Data/Common/DbProviderFactories.cs
		// This method doesn't need to be public, but it seems friendly/useful to make it so!
        static public List<Type> GetInstalledPluginTypes() {
            Initialize();
            return _installedPluginTypes;
        }

		// register a new database plugin for use with PureConnectionStringProvider
		// (If are going to pass the type of your DatabasePlugin via your own subclass of ConnectionProvider
		// you do not need to register it here; this registers unknown plugins for use with the ProviderName
		// in ConnectionString feature.)
		// <remarks>This can be tested by registering an existing plugin with a silly name!</remarks>
		static public void RegisterPlugin(Type pluginType)
		{
			GetInstalledPluginTypes().Add(pluginType);
		}
 
        static private void Initialize() {
            if (ConnectionState.Open != _initState) {
                lock (_lockobj) {
                    switch(_initState) {
                    case ConnectionState.Closed:
						// only relevant if the same thread which has the lock can recurse back into here while we
						// are initialising (any other thread can only see Closed or Open)
                        _initState = ConnectionState.Connecting;
                        try {
							AssembleDefaultPlugins();
                        }
                        finally {
							// try-catch ensures that even after exception we register that Initialize has been done once
                            _initState = ConnectionState.Open;
                        }
                        break;
                    case ConnectionState.Connecting:
                    case ConnectionState.Open:
                        break;
                    default:
						throw new Exception("unexpected state");
                    }
                }
            }
        }

		static private void AssembleDefaultPlugins()
		{
			_installedPluginTypes = new List<Type>();
			var targetType = typeof(DatabasePlugin);
			// Seems to be the best way to do this in .NET Core 1.1?
			var assembly = Assembly.Load(new AssemblyName(targetType.AssemblyQualifiedName));
			foreach (var type in assembly.GetTypes())
			{
				if (targetType.IsAssignableFrom(type))
				{
					_installedPluginTypes.Add(type);
				}
			}
		}
	}
}
