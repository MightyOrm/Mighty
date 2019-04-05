#if !NET40
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Mighty.Plugins
{
    abstract internal partial class PluginBase
    {
        #region Npgsql cursor dereferencing
        virtual public async Task<DbDataReader> ExecuteDereferencingReaderAsync(DbCommand cmd, CommandBehavior behavior, DbConnection conn, CancellationToken cancellationToken)
        {
            return await cmd.ExecuteReaderAsync(behavior, cancellationToken).ConfigureAwait(false);
        }
        #endregion
    }
}
#endif