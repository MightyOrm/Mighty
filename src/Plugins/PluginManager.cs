using System;
using System.Data;
using System.Collections.Generic;
using System.Reflection;

namespace Mighty.Plugins
{
    /// <summary>
    /// Register new plugins and review the registered plugins which provide Mighty support for different databases.
    /// </summary>
    public class PluginManager
    {
        /// <summary>
        /// Installed database plugin types.
        /// </summary>
        /// <remarks>
        /// I think this property doesn't need to be public, but it seems friendly/useful to make it be.
        /// </remarks>
        /// <returns></returns>
        static public List<Type> InstalledPluginTypes
        {
            get
            {
                Initialize();
                return _installedPluginTypes;
            }
        }

        /// <summary>
        /// Register a new database plugin for use with the <see cref="ConnectionProviders.PureConnectionStringProvider"/>.
        /// (NB If you plan to pass the type of your own database plugin via your own subclass of
        /// <see cref="ConnectionProviders.ConnectionProvider"/> then you do not need to register here;
        /// this registers unknown plugins for use with Mighty's "ProviderName=..." in ConnectionString feature.)
        /// <remarks>TO DO: This approach can be tested by registering an existing plugin with a silly name...</remarks>
        /// </summary>
        /// <param name="pluginType">The plugin type to register, must be a sub-class of <see cref="PluginBase"/></param>
        static public void RegisterPlugin(Type pluginType)
        {
            // no incorrect type exception here - a perfectly meaningful exception will be thrown as soon as Mighty tries to use this
            InstalledPluginTypes.Add(pluginType);
        }

        /// <summary>
        /// Use reflection to find the available plugins; only call this from inside the thread-safe initializer.
        /// </summary>
        static private void AssembleDefaultPlugins()
        {
            _installedPluginTypes = new List<Type>();
            var targetType = typeof(PluginBase);
#if (NETCOREAPP || NETSTANDARD)
            // Seems to be the best way to do this in .NET Core 1.1?
            var assemblyParts = targetType.AssemblyQualifiedName.Split(',');
            var assembly = Assembly.Load(new AssemblyName(assemblyParts[1]));
#else
            var assembly = Assembly.GetExecutingAssembly();
#endif
            foreach (var type in assembly.GetTypes())
            {
                if (targetType != type && targetType.IsAssignableFrom(type))
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
        private static readonly object _lockobj = new object();

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
