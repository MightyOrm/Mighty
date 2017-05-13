using System.Data.Common;

namespace Mighty.Profiling
{
	public class SqlProfiler
	{
		virtual public DbCommand Wrap(DbCommand command)
		{
			return command;
		}

		virtual public DbProviderFactory Wrap(DbProviderFactory factory)
		{
			return factory;
		}
	}
}