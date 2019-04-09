using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

using Mighty.Mocking;
using Mighty.Mapping;
using Mighty.Plugins;
using Mighty.Profiling;
using Mighty.Validation;

/// <summary>
/// MightyOrm_Redirects.cs holds methods in Mighty than can be very simply defined in terms of other methods.
/// </summary>
namespace Mighty
{
	public partial class MightyOrm<T> : MightyOrmAbstractInterface<T> where T : class, new()
	{
		#region Non-table specific methods
		override public DbCommand CreateCommand(string sql,
			params object[] args)
		{
			return CreateCommandWithParams(sql, args: args);
		}

		override public DbCommand CreateCommand(string sql,
			DbConnection connection,
			params object[] args)
		{
			return CreateCommandWithParams(sql, args: args);
		}
		#endregion

		#region Table specific methods
		override public T New()
		{
			return NewFrom();
		}
		#endregion
	}
}
