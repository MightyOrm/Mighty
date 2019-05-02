using System;
using System.Data.Common;

namespace Mighty.Profiling
{
    /// <summary>
    /// Pass an instance of this class to the constructor of <see cref="MightyOrm"/> in order to intercept
    /// System.Data.Common operations at any or all of the
    /// <see cref="DbProviderFactory"/>, <see cref="DbConnection"/> or <see cref="DbCommand"/> levels.
    /// </summary>
    public class DataProfiler
    {
        /// <summary>
        /// Provide your own function here to wrap at the factory level.
        /// </summary>
        public Func<DbProviderFactory, DbProviderFactory> FactoryWrapping { get; protected set; } = (i) => i;

        /// <summary>
        /// Provide your own function here to wrap at the connection level.
        /// </summary>
        public Func<DbConnection, DbConnection> ConnectionWrapping { get; protected set; } = (i) => i;

        /// <summary>
        /// Provide your own function here to wrap at the command level.
        /// </summary>
        public Func<DbCommand, DbCommand> CommandWrapping { get; protected set; } = (i) => i;

        /// <summary>
        /// Parameterless constructor (overriding class can use protected setters)
        /// </summary>
        /// 
        protected DataProfiler() { }

        /// <summary>
        /// Constructor
        /// </summary>
        public DataProfiler(
            Func<DbProviderFactory, DbProviderFactory> factoryWrapping = null,
            Func<DbConnection, DbConnection> connectionWrapping = null,
            Func<DbCommand, DbCommand> commandWrapping = null)
        {
            if (factoryWrapping != null) FactoryWrapping = factoryWrapping;
            if (connectionWrapping != null) ConnectionWrapping = connectionWrapping;
            if (commandWrapping != null) CommandWrapping = commandWrapping;
        }
    }
}