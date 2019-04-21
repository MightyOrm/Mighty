#if !NET40
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Mighty.Plugins
{
    abstract public partial class PluginBase
    {
        #region Npgsql cursor dereferencing
        /// <summary>
        /// For non-Npgsql, this just does <see cref="DbCommand.ExecuteReader(CommandBehavior)"/>.
        /// For Npgql this (optionally, depending on the value of<see cref="MightyOrm{T}.NpgsqlAutoDereferenceCursors"/>) returns a new <see cref="DbDataReader"/> which de-references
        /// all cursors returned by the original reader, iteratively returning those results instead.
        /// </summary>
        /// <param name="cmd">The original command</param>
        /// <param name="behavior">The command behaviour</param>
        /// <param name="conn">The connection to use</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        virtual public async Task<DbDataReader> ExecuteDereferencingReaderAsync(DbCommand cmd, CommandBehavior behavior, DbConnection conn, CancellationToken cancellationToken)
        {
            return await cmd.ExecuteReaderAsync(behavior, cancellationToken).ConfigureAwait(false);
        }
        #endregion
    }
}
#endif