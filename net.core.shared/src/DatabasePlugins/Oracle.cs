using System;
using System.Data.Common;

namespace Mighty.DatabasePlugins
{
	internal class Oracle : DatabasePlugin
	{
#region Provider support
		// we must use new because there are no overrides on static methods, see e.g. http://stackoverflow.com/q/7839691
		new static internal string GetProviderFactoryClassName(string loweredProviderName)
		{
			switch (loweredProviderName)
			{
				case "oracle.manageddataaccess.client":
					return "Oracle.ManagedDataAccess.Client.OracleClientFactory";

				case "oracle.dataaccess.client":
					return "Oracle.DataAccess.Client.OracleClientFactory";

				default:
					return null;
			}
		}
#endregion

#region SQL
		// Build a single query which returns two result sets: a scalar of the total count followed by
		// a normal result set of the page of items.
		// This really does vary per DB and can't be a standard virtual method which most things share.
		override public string BuildPagingQuery(string columns, string tablesAndJoins, string orderBy, string where,
			int limit, int offset)
		{
			throw new NotImplementedException();
		}

		// I think the SELECT in Oracle in Massive is WRONG, and we need to use this SQL if we want to limit things.
		// (By the way, we probably don't, as we can just use the single result hint, can't we?)

		// t outer table name does not conflict with any use of t table name in inner SELECT;
		// we need to call the column ROW_NUMBER() and then remove it from any results, if we are going to be consistent across DBs - but maybe we don't need to be;
		// note that first number is offset, and second number is offset + limit + 1
		// SELECT t.*
		// FROM
		// (
		// 		SELECT ROW_NUMBER() OVER (ORDER BY t.Salary DESC, t.Employee_ID) "ROW_NUMBER()", t.*
		// 		FROM employees t
		// 		WHERE t.last_name LIKE '%i%'
		// ) t
		// WHERE "ROW_NUMBER()" > 10 AND "ROW_NUMBER()" < 21
		// ORDER BY "ROW_NUMBER()";
#endregion

#region Table info
		// owner is for owner/schema, will be null if none was specified
		// This really does vary per DB and can't be a standard virtual method which most things share.
		override public string BuildTableInfoQuery(string owner, string tableName)
		{
			return string.Format("SELECT * FROM USER_TAB_COLUMNS WHERE TABLE_NAME = {0}{1}",
				tableName,
				owner == null ? "": string.Format(" AND OWNER = {1}", owner));
		}
#endregion

#region Prefix/deprefix parameters
		override public string PrefixParameterName(string rawName, DbCommand cmd = null)
		{
			return (cmd != null) ? rawName : (":" + rawName);
		}
#endregion

#region DbParameter
		override public void SetValue(DbParameter p, object value)
		{
			if (value is Guid)
			{
				p.Value = value.ToString();
				p.Size = 36;
			}
			else
			{
				p.Value = value;
				var valueAsString = value as string;
				if(valueAsString != null)
				{
					// let the query optimizer have a fixed size to work with for reasonable-sized strings
					p.Size = valueAsString.Length > 4000 ? -1 : 4000;
				}
			}
		}

		override public bool SetCursor(DbParameter p, object value)
		{
			p.SetRuntimeEnumProperty("OracleDbType", "RefCursor");
			p.Value = value;
			return true;
		}

		override public bool IsCursor(DbParameter p)
		{
			return p.GetRuntimeEnumProperty("OracleDbType") == "RefCursor";
		}
#endregion
	}
}