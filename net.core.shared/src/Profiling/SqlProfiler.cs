using System.Data.Common;

namespace Mighty.Profiling
{
	public class NullProfiler : SqlProfiler
	{
	}

	abstract public class SqlProfiler
	{
		/// <summary>
		/// Override here to wrap at the factory level.
		/// </summary>
		/// <param name="factory"></param>
		/// <returns></returns>
		virtual public DbProviderFactory Wrap(DbProviderFactory factory)
		{
			return factory;
		}

		/// <summary>
		/// Override here to wrap at the connection level.
		/// </summary>
		/// <param name="connection"></param>
		/// <returns></returns>
		virtual public DbConnection Wrap(DbConnection connection)
		{
			return connection;
		}

		/// <summary>
		/// Override here to wrap at the command level.
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>
		virtual public DbCommand Wrap(DbCommand command)
		{
			return command;
		}
	}
}