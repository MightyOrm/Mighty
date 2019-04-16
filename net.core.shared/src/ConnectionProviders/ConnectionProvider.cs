using System;
using System.Data.Common;

using Mighty.Plugins;

namespace Mighty.ConnectionProviders
{
    /// <summary>
    /// Implement this abstract class and then register the <see cref="Type"/> of your implementation with <see cref="PluginManager"/>
    /// in order to provide support for any currently unsupported database type in Mighty.
    /// </summary>
    abstract public class ConnectionProvider
    {
        /// <summary>
        /// Specify the <see cref="DbProviderFactory"/> which database access going via this connection provider should use.
        /// </summary>
        public DbProviderFactory ProviderFactoryInstance { get; protected set; }

        /// <summary>
        /// Specify which database plugin class to use, for all the various bits of Mighty which have been found to
        /// vary between database providers. This must be an instance of <see cref="PluginBase"/>).
        /// </summary>
        public Type DatabasePluginType { get; protected set; }

        /// <summary>
        /// Specify the connection string to use when instantiating a <see cref="DbConnection"/> via this connection provider;
        /// this might typically be (or be derived from) the connection string provided to the constructor of this class.
        /// </summary>
        public string ConnectionString { get; protected set; }

        /// <summary>
        /// Fluent API, must return `this` at the end. It should set all three public properties of this class.
        /// (You may ignore connectionStringOrName input here if appropriate, in which case you
        /// would pass null as the connectionStringOrName value to the <see cref="MightyOrm"/> constructor.)
        /// </summary>
        /// <param name="connectionStringOrName">
        /// A connection provider will typically either be passed either a connection string
        /// or a name by which a connection string can be looked up elsewhere. (But note that your
        /// connection provider can just ignore this input parameter if it needs to.)
        /// </param>
        /// <returns></returns>
        abstract public ConnectionProvider Init(string connectionStringOrName);
    }
}