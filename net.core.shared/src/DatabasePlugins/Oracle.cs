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
		override public string BuildPagingQuery(string columns, string tablesAndJoins, string orderBy, string where,
			int limit, int offset)
		{
			return BuildRowNumberPagingQuery(columns, tablesAndJoins, orderBy, where, limit, offset);
		}
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

#region Keys and sequences
		override public bool IsSequenceBased { get; protected set; } = true;
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
			// Oracle exceptions on Guid parameter - set it via string
			if (value is Guid)
			{
				p.Value = value.ToString();
				p.Size = 36;
				return;
			}
			base.SetValue(p, value);
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