using System;
using System.Data;
using System.Data.Common;

namespace Mighty.DatabasePlugins
{
	internal class MySQL : DatabasePlugin
	{
#region Provider support
		// we must use new because there are no overrides on static methods, see e.g. http://stackoverflow.com/q/7839691
		new static internal string GetProviderFactoryClassName(string loweredProviderName)
		{
			switch (loweredProviderName)
			{
				case "mysql.data.mysqlclient":
					// older/beta qualified class name on COREFX was:
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
		override public string BuildPagingQuery(string columns, string tablesAndJoins, string orderBy, string where,
			int limit, int offset)
		{
			throw new NotImplementedException();
		}
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
			p.Value = value;
			var valueAsString = value as string;
			if (valueAsString != null)
			{
				// let the query optimizer have a fixed size to work with for reasonable-sized strings
				p.Size = valueAsString.Length > 4000 ? -1 : 4000;
				return;
			}
			var valueAsBool = value as bool?;
			if (valueAsBool != null)
			{
				// this is required for our bool fix-up for Oracle/MySQL, and does not change a thing on Devart
				p.DbType = DbType.Boolean;
				return;
			}
			var valueAsSByte = value as sbyte?;
			if (valueAsSByte != null)
			{
				// we have to set this to what it already is at this point, to ensure they know we don't want it to change
				p.SetRuntimeEnumProperty("MySqlType", "TinyInt", false);
				return;
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