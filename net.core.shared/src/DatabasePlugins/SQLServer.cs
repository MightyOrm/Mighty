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
			string CountQuery = BuildSelect("COUNT(*)", mighty.Unthingify("FROM", tablesAndJoins), where);

			// works with QUOTED_IDENTIFIER OFF or ON
			//
			// 't' outer table name will not conflict with any use of 't' table name in inner SELECT
			//
			// the idea is to to call the column ROW_NUMBER() and then remove it from any results, if we are going to be
			// consistent across DBs - but maybe we don't need to be;
			//
			string PagingQuery =
				string.Format("SELECT t.*" + CRLF +
							  "FROM" + CRLF +
							  "(" + CRLF +
							  "    SELECT ROW_NUMBER() OVER ({3}) [" + ROWCOL + "], {0}" + CRLF +
							  "    FROM {1}" + CRLF +
							  "{2}" +
							  ") t" + CRLF +
							  "WHERE {5}[" + ROWCOL + "] < {4}" + CRLF +
							  "ORDER BY [" + ROWCOL + "];",
					mighty.Unthingify("SELECT", columns),
					mighty.Unthingify("FROM", tablesAndJoins),
					where == null ? "" : string.Format("    {0}" + CRLF, mighty.Thingify("WHERE", where)),
					mighty.Thingify("ORDER BY", orderBy),
					limit + 1,
					offset > 0 ? string.Format("{0} > {1} AND ", "{0}", offset) : ""
				);
			return CountQuery + CRLF + PagingQuery;
		}
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
		override public string PrefixParameterName(string rawName, DbCommand cmd = null)
		{
			return (cmd != null) ? rawName : ("@" + rawName);
		}
#endregion
	}
}