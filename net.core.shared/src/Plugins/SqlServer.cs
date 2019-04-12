using System;
using System.Data.Common;

namespace Mighty.Plugins
{
	internal class SqlServer : PluginBase
	{
		#region Provider support
		// we must use new because there are no overrides on static methods, see e.g. http://stackoverflow.com/q/7839691
		new static public string GetProviderFactoryClassName(string loweredProviderName)
		{
			switch (loweredProviderName)
			{
				case "system.data.sqlclient":
					return "System.Data.SqlClient.SqlClientFactory";

				default:
					return null;
			}
		}
		#endregion

		#region SQL
		override public string BuildSelect(string columns, string tableName, string where, string orderBy = null, int limit = 0)
		{
			return BuildTopSelect(columns, tableName, where, orderBy, limit);
		}

		override public PagingQueryPair BuildPagingQueryPair(string columns, string tableNameOrJoinSpec, string orderBy, string where,
			int limit, int offset)
		{
			return BuildRowNumberPagingQueryPair(columns, tableNameOrJoinSpec, orderBy, where, limit, offset);
		}
		#endregion

		#region Keys and sequences
		override public string IdentityRetrievalFunction { get; protected set; } = "SCOPE_IDENTITY()";
		#endregion

		#region Prefix/deprefix parameters
		override public string PrefixParameterName(string rawName, DbCommand cmd = null)
		{
			return (cmd != null) ? rawName : ("@" + rawName);
		}
		#endregion
	}
}