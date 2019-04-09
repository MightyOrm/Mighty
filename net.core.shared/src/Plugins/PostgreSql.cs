using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Collections.Generic;
using System.Linq;

using Mighty.Npgsql;

namespace Mighty.Plugins
{
	internal partial class PostgreSql : PluginBase
	{
		#region Provider support
		// we must use new because there are no overrides on static methods, see e.g. http://stackoverflow.com/q/7839691
		new static internal string GetProviderFactoryClassName(string loweredProviderName)
		{
			switch (loweredProviderName)
			{
				case "npgsql":
					return "Npgsql.NpgsqlFactory";

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

		#region Table info
		override public IEnumerable<dynamic> PostProcessTableMetaData(IEnumerable<dynamic> rawTableMetaData)
		{
			List<dynamic> results = new List<object>();
			foreach (ExpandoObject columnInfo in rawTableMetaData)
			{
				var newInfo = new ExpandoObject();
				var dict = newInfo.ToDictionary();
				foreach (var pair in columnInfo)
				{
					dict.Add(pair.Key.ToUpperInvariant(), pair.Value);
				}
				results.Add(newInfo);
			}
			return results;
		}
		#endregion

		#region Keys and sequences
		override public bool IsSequenceBased { get; protected set; } = true;
		override public string BuildNextval(string sequence) { return string.Format("nextval('{0}')", sequence); }
		override public string BuildCurrvalSelect(string sequence) { return string.Format("SELECT currval('{0}')", sequence); }
		#endregion

		#region Prefix/deprefix parameters
		override public string PrefixParameterName(string rawName, DbCommand cmd = null)
		{
			return (cmd != null) ? rawName : (":" + rawName);
		}
		#endregion

		#region DbParameter
		override public void SetDirection(DbParameter p, ParameterDirection direction)
		{
			// PostgreSQL/Npgsql specific fix: if used, return params always return unchanged; return values must
			// be accessed using output params instead (presumably because of how PostgreSQL internally treates
			// return parameters, which are not really different from output parameters on that DB)
			p.Direction = (direction == ParameterDirection.ReturnValue) ? ParameterDirection.Output : direction;
		}

		override public bool SetCursor(DbParameter p, object value)
		{
			p.SetRuntimeEnumProperty("NpgsqlDbType", "Refcursor");
			p.Value = value;
			return true;
		}

		override public bool IsCursor(DbParameter p)
		{
			return p.GetRuntimeEnumProperty("NpgsqlDbType") == "Refcursor";
		}

		override public bool SetAnonymousParameter(DbParameter p)
		{
			// pretty simple! but assume in principle more could be needed in some other provider
			p.ParameterName = "";
			return true;
		}

		override public bool IgnoresOutputTypes(DbParameter p)
		{
			return true;
		}
		#endregion

		#region Npgsql cursor dereferencing
		/// <summary>
		/// Dereference cursors in more or less the way which used to be supported within Npgsql itself, only now considerably improved from that removed, partial support.
		/// </summary>
		/// <param name="cmd">The command.</param>
		/// <param name="Connection">The connection - required for deferencing.</param>
		/// <param name="db">The parent MightyOrm (or subclass) - required to get at the factory for deferencing and config vaules.</param>
		/// <returns>The reader, dereferenced if needed.</returns>
		/// <remarks>
		/// https://github.com/npgsql/npgsql/issues/438
		/// http://stackoverflow.com/questions/42292341/
		/// </remarks>
		override public DbDataReader ExecuteDereferencingReader(DbCommand cmd, CommandBehavior behavior, DbConnection Connection)
		{
			// We can never restrict the parent read to do LESS than the hint provided - because we might
			// not be dereferencing it, but just using it; but we can always restrict to the hint provided,
			// because the first cursor (if any) MUST always be in the first row of the first result.
			var reader = cmd.ExecuteReader(behavior); // var reader = Execute(behavior);

			// Remarks: Do not consider dereferencing if no returned columns are cursors, but if just some are cursors then follow the pre-existing convention set by
			// the Oracle drivers and dereference what we can. The rest of the pattern is that we only ever try to dereference on Query and Scalar, never on Execute.
			if (Mighty.NpgsqlAutoDereferenceCursors && NpgsqlDereferencingReader.CanDereference(reader))
			{
				// Passes <see cref="CommandBehavior"/> to dereferencing reader, which uses it where it can
				// (e.g. to dereference only the first cursor, or only the first row of the first cursor)
				var newReader = new NpgsqlDereferencingReader(reader, behavior, Connection, Mighty);
				newReader.Init();
				return newReader;
			}

			return reader;
		}

		/// <summary>
		/// Returns true if this command requires a wrapping transaction.
		/// </summary>
		/// <param name="cmd">The command.</param>
		/// <param name="db">The dynamic model, to access config params.</param>
		/// <returns>true if it requires a wrapping transaction</returns>
		/// <remarks>
		/// Only relevant to Postgres cursor dereferencing and in this case we also do some relevant pre-processing of the command.
		/// </remarks>
		override public bool RequiresWrappingTransaction(DbCommand cmd)
		{
			if (!Mighty.NpgsqlAutoDereferenceCursors)
			{
				// Do not request wrapping transaction if auto-dereferencing is off
				return false;
			}
			// If we've got cursor parameters these are actually just placeholders to kick off cursor support (i.e. the wrapping transaction); we need to remove them before executing the command.
			bool isCursorCommand = false;
			cmd.Parameters.Cast<DbParameter>().Where(p => IsCursor(p)).ToList().ForEach(p => { isCursorCommand = true; cmd.Parameters.Remove(p); });
			return isCursorCommand;
		}
		#endregion
	}
}