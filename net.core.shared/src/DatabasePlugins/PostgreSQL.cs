using System.Data;
using System.Data.Common;
using System.Linq;

namespace Mighty.DatabasePlugins
{
	internal class PostgreSQL : DatabasePlugin
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
		/// <param name="db">The parent MightyORM (or subclass) - required to get at the factory for deferencing and config vaules.</param>
		/// <returns>The reader, dereferenced if needed.</returns>
		/// <remarks>
		/// https://github.com/npgsql/npgsql/issues/438
		/// http://stackoverflow.com/questions/42292341/
		/// </remarks>
		override public DbDataReader ExecuteDereferencingReader(DbCommand cmd, DbConnection Connection)
		{
			var reader = cmd.ExecuteReader(); // var reader = Execute(behavior);

			// Remarks: Do not consider dereferencing if no returned columns are cursors, but if just some are cursors then follow the pre-existing convention set by
			// the Oracle drivers and dereference what we can. The rest of the pattern is that we only ever try to dereference on Query and Scalar, never on Execute.
			if (mighty.NpgsqlAutoDereferenceCursors && NpgsqlDereferencingReader.CanDereference(reader))
			{
				return new NpgsqlDereferencingReader(reader, Connection, mighty);
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
			if (!mighty.NpgsqlAutoDereferenceCursors)
			{
				// Do not request wrapping transaction if auto-dereferencing is off
				return false;
			}
			// If we've got cursor parameters these are actually just placeholders to kick off cursor support (i.e. the wrapping transaction); we need to remove them before executing the command.
			bool isCursorCommand = false;
			cmd.Parameters.Cast<DbParameter>().Where(p => mighty._plugin.IsCursor(p)).ToList().ForEach(p => { isCursorCommand = true; cmd.Parameters.Remove(p); });
			return isCursorCommand;
		}
#endregion
	}
}