using System;
using System.Data;
using System.Collections.Generic;
using System.Reflection;

namespace Mighty.DatabasePlugins
{
	public class DatabasePluginManager
	{
		// I think this method doesn't need to be public, but it seems friendly/useful to make it be
        static public List<Type> GetInstalledPluginTypes() {
            Initialize();
            return _installedPluginTypes;
        }

		// Register a new database plugin for use with PureConnectionStringProvider
		// (If are going to pass the type of your own DatabasePlugin via your own subclass of
		// ConnectionProvider you do not need to register here; this registers unknown plugins
		// for use with the ProviderName in ConnectionString feature.)
		// <remarks>This approach can be tested by registering an existing plugin with a silly name..!</remarks>
		static public void RegisterPlugin(Type pluginType)
		{
			// no incorrect type exception here - a perfectly meaningful exception will be thrown as soon as Mighty tries to use this
			GetInstalledPluginTypes().Add(pluginType);
		}

		// Use reflection to find the plugins; only call this from inside the thread-safe initializer
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

#region Thread-safe initializer 
		// Thread-safe initialization based on Microsoft DbProviderFactories reference 
		// https://referencesource.microsoft.com/#System.Data/System/Data/Common/DbProviderFactories.cs
		
		// fields for thread safe access to the list of default + registered database plugins
		private static ConnectionState _initState; // closed (default value), connecting, open
        private static List<Type> _installedPluginTypes;
        private static object _lockobj = new object();

        static private void Initialize()
		{
			// MS code (re-)uses database connection states
            if (_initState != ConnectionState.Open)
			{
                lock (_lockobj)
				{
                    switch (_initState)
					{
						case ConnectionState.Closed:
							// 'Connecting' state only relevant if the thread which has the lock can recurse back into here
							// while we are initialising (any other thread can only see Closed or Open)
							_initState = ConnectionState.Connecting;
							try
							{
								AssembleDefaultPlugins();
							}
							finally
							{
								// try-finally ensures that even after exception we register that Initialize has been called
								// (the exception is still thrown after the finally code has happened)
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
#endregion
	}
}
