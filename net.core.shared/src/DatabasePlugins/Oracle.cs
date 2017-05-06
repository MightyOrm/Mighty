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
		override public string BuildSelect(string columns, string tableName, string where, string orderBy = null, int limit = 0)
		{
			string sql = BuildTopSelect(columns, tableName, where, orderBy);
			if (limit == 0)
			{
				return sql;
			}
			else
			{
				// This seems incorrect, because it breaches the notion that ORDER BY is not necessarily preserved from an inner query,
				// but it also seems to work (e.g. no dissenting voices at http://stackoverflow.com/q/2498035/), otherwise we could
				// build a full row_numer() paging query, but that itself can't be done correctly without adding a row number column
				// to the results.
				return string.Format("SELECT * FROM ({0}) WHERE ROWNUM <= {1}", sql, limit);
			}
		}

		override public string BuildPagingQuery(string columns, string tablesAndJoins, string orderBy, string where,
			int limit, int offset)
		{
			return BuildRowNumberPagingQuery(columns, tablesAndJoins, orderBy, where, limit, offset);
		}
		#endregion

		#region Table info
		// owner is for owner/schema, will be null if none was specified
		// This really does vary per DB and can't be a standard virtual method which most things share.
		override public string BuildTableMetaDataQuery(bool addOwner)
		{
			return string.Format("SELECT * FROM USER_TAB_COLUMNS WHERE TABLE_NAME = {0}{1}",
				PrefixParameterName("0"),
				addOwner ? string.Format(" AND OWNER = {1}", PrefixParameterName("0")) : "");
		}

		override public object GetColumnDefault(dynamic columnInfo)
		{
			// This code from Massive - see CREDITS file
			string defaultValue = columnInfo.COLUMN_DEFAULT;
			if (string.IsNullOrEmpty(defaultValue))
			{
				return null;
			}
			dynamic result;
			switch (defaultValue)
			{
				case "SYSDATE":
				case "(SYSDATE)":
					result = DateTime.Now;
					break;
				default:
					result = defaultValue.Replace("(", "").Replace(")", "");
					break;
			}
			return result;
		}
		#endregion

		#region Keys and sequences
		override public bool IsSequenceBased { get; protected set; } = true;
		override public string BuildNextval(string sequence) { return string.Format("{0}.nextval", sequence); }
		override public string BuildCurrval(string sequence) { return string.Format("{0}.currval", sequence); }
		virtual public string NoTable() { return " FROM DUAL"; }
		#endregion

		#region DbCommand
		override public void SetProviderSpecificCommandProperties(DbCommand command)
		{
			// These two settings and their comments taken direct from Massive - see CREDITS file
			((dynamic)command).BindByName = true;   // keep true as the default as otherwise ODP.NET won't bind the parameters by name but by location.
			((dynamic)command).InitialLONGFetchSize = -1;   // this is the ideal value, it obtains the LONG value in one go.
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