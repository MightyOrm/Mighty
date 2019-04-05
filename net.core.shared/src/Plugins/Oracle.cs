using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Mighty.Plugins
{
	internal class Oracle : PluginBase
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

		override public PagingQueryPair BuildPagingQueryPair(string columns, string tablesAndJoins, string where, string orderBy,
			int limit, int offset)
		{
			return BuildRowNumberPagingQueryPair(columns, tablesAndJoins, where, orderBy, limit, offset);
		}

		// Build an insert command which - even on Oracle - can insert and return the new PK in a single round-trip to the database.
		// (TO DO: check whether we are actually getting the Oracle API to auto-generate a second round-trip for us? Either
		// way, we need some special case for Oracle, and this is as good as any.)
		override public void FixupInsertCommand(DbCommand command)
		{
			command.CommandText = string.Format("BEGIN\r\n{0}\r\nEND;", command.CommandText);
			// Add cursor, which will be automatically dereferenced by the Oracle data access layer
			Mighty.AddNamedParams(command, new { pk___ = new Cursor() }, ParameterDirection.Output);
		}
		#endregion

		#region Table info
		// owner is for owner/schema, will be null if none was specified
		// This really does vary per DB and can't be a standard virtual method which most things share.
		override public string BuildTableMetaDataQuery(string tableName, string tableOwner)
		{
			return string.Format("SELECT * FROM {0}_TAB_COLUMNS WHERE TABLE_NAME = {1}{2}",
				tableOwner != null ? "ALL" : "USER",
				PrefixParameterName("0"),
				tableOwner != null ? string.Format(" AND OWNER = {0}", PrefixParameterName("1")) : "");
		}

		override public IEnumerable<dynamic> PostProcessTableMetaData(IEnumerable<dynamic> rawTableMetaData)
		{
			List<dynamic> results = new List<object>();
			foreach (dynamic columnInfo in rawTableMetaData)
			{
				columnInfo.NUMERIC_SCALE = columnInfo.DATA_SCALE;
				columnInfo.COLUMN_DEFAULT = columnInfo.DATA_DEFAULT;
				results.Add(columnInfo);
			}
			return results;
		}
		#endregion

		#region Keys and sequences
		override public bool IsSequenceBased { get; protected set; } = true;
		override public string BuildNextval(string sequence) { return string.Format("{0}.nextval", sequence); }
		// With cursor so that we can use Oracle built-in automatic dereferencing and make a single round trip to the DB
		override public string BuildCurrvalSelect(string sequence) { return string.Format("OPEN :pk___ FOR SELECT {0}.currval FROM DUAL", sequence); }
		#endregion

		#region DbCommand
		override public void SetProviderSpecificCommandProperties(DbCommand command)
		{
			// these setting values and their comments are taken directly from Massive - see CREDITS file;
			// the approach to setting them via a dynamic is different from what used to be in Massive
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