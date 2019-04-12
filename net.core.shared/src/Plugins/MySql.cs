using System;
using System.Data;
using System.Data.Common;

namespace Mighty.Plugins
{
	internal class MySql : PluginBase
	{
		#region Provider support
		// we must use new because there are no overrides on static methods, see e.g. http://stackoverflow.com/q/7839691
		new static public string GetProviderFactoryClassName(string loweredProviderName)
		{
			switch (loweredProviderName)
			{
				case "mysql.data.mysqlclient":
					// older/beta qualified class name on .NET Core was:
					//return "MySql.Data.MySqlClient.MySqlClientFactory, MySql.Data.Core";
					return "MySql.Data.MySqlClient.MySqlClientFactory, MySql.Data";

				case "devart.data.mysql":
					return "Devart.Data.MySql.MySqlProviderFactory";

				default:
					return null;
			}
		}
		#endregion

		#region SQL
		override public string BuildSelect(string columns, string tableName, string where, string orderBy = null, int limit = 0)
		{
			return BuildLimitSelect(columns, tableName, where, orderBy, limit);
		}

		override public PagingQueryPair BuildPagingQueryPair(string columns, string tableNameOrJoinSpec, string orderBy, string where,
			int limit, int offset)
		{
			return BuildLimitOffsetPagingQueryPair(columns, tableNameOrJoinSpec, orderBy, where, limit, offset);
		}
		#endregion

		#region Keys and sequences
		override public string IdentityRetrievalFunction { get; protected set; } = "LAST_INSERT_ID()";
		#endregion

		#region Prefix/deprefix parameters
		override public string PrefixParameterName(string rawName, DbCommand cmd = null)
		{
			return (cmd != null && cmd.CommandType == CommandType.StoredProcedure) ? rawName : ("@" + rawName);
		}

		override public string DeprefixParameterName(string dbParamName, DbCommand cmd)
		{
			return cmd.CommandType == CommandType.StoredProcedure ? dbParamName : dbParamName.Substring(1);
		}
		#endregion

		#region DbParameter
		override public void SetValue(DbParameter p, object value)
		{
			base.SetValue(p, value);
			if (value is string) return;
			var valueAsBool = value as bool?;
			if (valueAsBool != null)
			{
				// this is required for our bool fix-up for Oracle/MySQL, and does not change a thing on Devart
				p.DbType = DbType.Boolean;
			}
			var valueAsSByte = value as sbyte?;
			if (valueAsSByte != null)
			{
				// we have to set this to what it already is at this point, so that they know we really want to use the auto-assigned type
				p.SetRuntimeEnumProperty("MySqlType", "TinyInt", false);
			}
		}

		override public object GetValue(DbParameter p)
		{
			object value = p.Value;
			if (value == DBNull.Value)
			{
				return value;
			}
			if (p.DbType == DbType.Boolean)
			{
				return (1 == Convert.ToInt32(value));
			}
			if (p.GetRuntimeEnumProperty("MySqlType") == "Bit")
			{
				return (1 == Convert.ToInt32(value));
			}
			if (p.GetRuntimeEnumProperty("MySqlType") == "TinyInt")
			{
				return Convert.ToSByte(value);
			}
			return value;
		}
		#endregion
	}
}