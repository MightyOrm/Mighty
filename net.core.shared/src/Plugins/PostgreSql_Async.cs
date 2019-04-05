#if !NET40
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

using Mighty.Npgsql;
using System.Threading;

namespace Mighty.Plugins
{
    internal partial class PostgreSql : PluginBase
    {
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
        override public async Task<DbDataReader> ExecuteDereferencingReaderAsync(DbCommand cmd, CommandBehavior behavior, DbConnection Connection, CancellationToken cancellationToken)
        {
            // We can never restrict the parent read to do LESS than the hint provided - because we might
            // not be dereferencing it, but just using it; but we can always restrict to the hint provided,
            // because the first cursor (if any) MUST always be in the first row of the first result.
            var reader = await cmd.ExecuteReaderAsync(behavior, cancellationToken).ConfigureAwait(false); // var reader = Execute(behavior);

            // Remarks: Do not consider dereferencing if no returned columns are cursors, but if just some are cursors then follow the pre-existing convention set by
            // the Oracle drivers and dereference what we can. The rest of the pattern is that we only ever try to dereference on Query and Scalar, never on Execute.
            if (Mighty.NpgsqlAutoDereferenceCursors && NpgsqlDereferencingReader.CanDereference(reader))
            {
                // Passes <see cref="CommandBehavior"/> to dereferencing reader, which uses it where it can
                // (e.g. to dereference only the first cursor, or only the first row of the first cursor)
                var newReader = new NpgsqlDereferencingReader(reader, behavior, Connection, Mighty);
                await newReader.InitAsync(cancellationToken);
                return newReader;
            }

            return reader;
        }
    }
}
#endif