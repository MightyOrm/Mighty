using System;
using System.Data;
using System.Data.Common;

namespace Mighty.DatabasePlugins
{
	internal class SQLServer : DatabasePlugin
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
		override public string BuildPagingQuery(string columns, string tablesAndJoins, string orderBy, string where,
			int limit, int offset)
		{
			throw new NotImplementedException();
		}
		// works with QUOTED_IDENTIFIER OFF OR ON;
		// t outer table name does not conflict with any use of t table name in inner SELECT
		// we need to call the column ROW_NUMBER() and then remove it from any results, if we are going to be consistent across DBs;
		// note that first number is offset, and second number is offset + limit + 1
		// SELECT t.*
		// FROM
		// (
		// 	   SELECT ROW_NUMBER() OVER (ORDER BY t.Color DESC, t.ProductNumber) AS [ROW_NUMBER()], t.*
		// 	   FROM Production.Product AS t
		// 	   WHERE t.ProductNumber LIKE '%R%'
		// ) t
		// WHERE [ROW_NUMBER()] > 10 AND [ROW_NUMBER()] < 21
		// ORDER BY [ROW_NUMBER()]
#endregion

#region Prefix/deprefix parameters
		// Needs to know whether this is for use in DbParameter name (cmd=null) or for escaping within the SQL fragment itself,
		// and if it is for a DbParameter whether it is used for a stored procedure or for a SQL fragment.
		override public string PrefixParameterName(string rawName, DbCommand cmd = null)
		{
			throw new NotImplementedException();
		}
		// Will always be from a DbParameter, but needs to know whether it was used for
		// a stored procedure or for a SQL fragment.
		override public string DeprefixParameterName(string dbParamName, DbCommand cmd)
		{
			throw new NotImplementedException();
		}
#endregion

#region DbParameter
		override public void SetValue(DbParameter p, object value)
		{
			throw new NotImplementedException();
		}
		override public object GetValue(DbParameter p)
		{
			throw new NotImplementedException();
		}
		override public void SetDirection(DbParameter p, ParameterDirection direction)
		{
			throw new NotImplementedException();
		}
		override public bool SetCursor(DbParameter p, object value)
		{
			throw new NotImplementedException();
		}
		override public bool IsCursor(DbParameter p)
		{
			throw new NotImplementedException();
		}
		override public bool SetAnonymousParameter(DbParameter p)
		{
			throw new NotImplementedException();
		}
		override public bool IgnoresOutputTypes(DbParameter p)
		{
			throw new NotImplementedException();
		}
#endregion
	}
}