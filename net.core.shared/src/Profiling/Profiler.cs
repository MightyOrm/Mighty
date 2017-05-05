using System.Data.Common;

namespace Mighty.Profiling
{
	public class Profiler
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