using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mighty.Profiling
{
    /// <summary>
    /// Inject this if you need to inject something for no SQL Profiling.
    /// </summary>
    public class NullProfiler : SqlProfiler
    {
        /// <summary>
        /// Just return the factory.
        /// </summary>
        /// <param name="factory">The factory to wrap</param>
        /// <returns></returns>
        override public DbProviderFactory Wrap(DbProviderFactory factory)
        {
            return factory;
        }

        /// <summary>
        /// Just return the connection.
        /// </summary>
        /// <param name="connection">The connection to wrap</param>
        /// <returns></returns>
        override public DbConnection Wrap(DbConnection connection)
        {
            return connection;
        }

        /// <summary>
        /// Just return the command.
        /// </summary>
        /// <param name="command">The command to wrap</param>
        /// <returns></returns>
        override public DbCommand Wrap(DbCommand command)
        {
            return command;
        }
    }
}
