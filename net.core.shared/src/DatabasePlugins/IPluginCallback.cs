using System.Data.Common;

namespace Mighty.DatabasePlugins
{
	// TO DO: I'd like to make this internal - I think I nearly can
	public interface IPluginCallback
	{
		bool NpgsqlAutoDereferenceCursors { get; }
		int NpgsqlAutoDereferenceFetchSize { get; }
		DbCommand CreateCommand(string sql, params object[] args);
	}
}