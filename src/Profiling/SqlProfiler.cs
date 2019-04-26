using System.Data.Common;

namespace Mighty.Profiling
{
    /// <summary>
    /// Implement this abstract class and pass an instance of it to the constructor of <see cref="MightyOrm"/> in order to trap System.Data.Common operations at any or all of the
    /// <see cref="DbProviderFactory"/>, <see cref="DbConnection"/> or <see cref="DbCommand"/> levels.
    /// </summary>
    abstract public class SqlProfiler
    {
        /// <summary>
        /// Override here to wrap at the factory level.
        /// </summary>
        /// <param name="factory">The factory to wrap</param>
        /// <returns></returns>
        abstract public DbProviderFactory Wrap(DbProviderFactory factory);

        /// <summary>
        /// Override here to wrap at the connection level.
        /// </summary>
        /// <param name="connection">The connection to wrap</param>
        /// <returns></returns>
        abstract public DbConnection Wrap(DbConnection connection);

        /// <summary>
        /// Override here to wrap at the command level.
        /// </summary>
        /// <param name="command">The command to wrap</param>
        /// <returns></returns>
        abstract public DbCommand Wrap(DbCommand command);
    }
}