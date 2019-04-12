using System.Data.Common;

namespace Mighty.Profiling
{
    /// <summary>
    /// Implement this abstract class and pass it to the constructor of <see cref="MightyOrm"/> in order to trap System.Data.Common operations at any or all of the
    /// <see cref="DbProviderFactory"/>, <see cref="DbConnection"/> or <see cref="DbCommand"/> levels.
    /// </summary>
	abstract public class SqlProfiler
	{
		/// <summary>
		/// Override here to wrap at the factory level.
		/// </summary>
		/// <param name="factory">The factory</param>
		/// <returns></returns>
		virtual public DbProviderFactory Wrap(DbProviderFactory factory)
		{
			return factory;
		}

		/// <summary>
		/// Override here to wrap at the connection level.
		/// </summary>
		/// <param name="connection">Optional connection to use</param>
		/// <returns></returns>
		virtual public DbConnection Wrap(DbConnection connection)
		{
			return connection;
		}

        /// <summary>
        /// Override here to wrap at the command level.
        /// </summary>
        /// <param name="command">The command</param>
        /// <returns></returns>
        virtual public DbCommand Wrap(DbCommand command)
		{
			return command;
		}
	}
}