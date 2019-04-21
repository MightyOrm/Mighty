#if !NET40
using System;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Mighty.Plugins;

namespace Mighty.Npgsql
{
    internal partial class NpgsqlDereferencingReader : DbDataReader, IDisposable
    {
        /// <summary>
        /// Initialise the reader
        /// </summary>
        /// <returns></returns>
        public async Task InitAsync(CancellationToken cancellationToken)
        {
            // Behavior is only saved to be used here in this Init method; we don't need to check or enforce it again
            // elsewhere since the logic below is already enforcing it.
            // For SingleRow we additionally rely on the user to only read one row and then dispose of everything.
            bool earlyQuit = (Behavior == CommandBehavior.SingleResult || Behavior == CommandBehavior.SingleRow);

            // we're saving all the cursors from the original reader here, then disposing of it
            // (we've already checked in CanDereference that there are at least some cursors)
            using (originalReader)
            {
                // Supports 1x1 1xN Nx1 and NXM patterns of cursor data.
                // If just some values are cursors we follow the pre-existing pattern set by the Oracle drivers, and dereference what we can.
                while (await originalReader.ReadAsync(cancellationToken))
                {
                    for (int i = 0; i < originalReader.FieldCount; i++)
                    {
                        if (originalReader.GetDataTypeName(i) == "refcursor")
                        {
                            // cursor name can potentially contain " so stop that breaking us
                            // TO DO: document how/why
                            Cursors.Add(originalReader.GetString(i).Replace(@"""", @""""""));
                            if (earlyQuit) break;
                        }
                    }
                    if (earlyQuit) break;
                }
            }

            // initialize
            await NextResultAsync(cancellationToken);
        }

        /// <summary>
        /// Fetch next N rows from current cursor.
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/>.</param>
        /// <param name="closePreviousSQL">SQL to prepend, to close the previous cursor in a single round trip (optional).</param>
        private async Task FetchNextNFromCursorAsync(CancellationToken cancellationToken, string closePreviousSQL = "")
        {
            // close and dispose previous fetch reader for this cursor
            if (fetchReader != null && !fetchReader.IsClosed)
            {
                fetchReader.Dispose();
            }
            // fetch next n from cursor;
            // optionally close previous cursor;
            // iff we're fetching all, we can close this cursor in this command
            using (var fetchCmd = CreateCommand(closePreviousSQL + FetchSQL() + (FetchSize <= 0 ? CloseSQL() : ""), Connection)) // new NpgsqlCommand(..., Connection);
            {
                // NpgsqlCommand.ExecuteReaderAsync is ignoring its CancellationToken
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }
                fetchReader = await fetchCmd.ExecuteReaderAsync(CommandBehavior.SingleResult, cancellationToken);
            }
            Count = 0;
        }

        public override async Task<bool> NextResultAsync(CancellationToken cancellationToken)
        {
            var closeSql = CloseCursor(Index >= Cursors.Count);
            if (Index >= Cursors.Count)
            {
                return false;
            }
            Cursor = Cursors[Index++];
            await FetchNextNFromCursorAsync(cancellationToken, closeSql);
            return true;
        }

        public override async Task<bool> ReadAsync(CancellationToken cancellationToken)
        {
            if (fetchReader != null)
            {
                // NpgsqlDataReader.ReadAsync is ignoring its CancellationToken
                // TO DO: Report the test we've already done on this to Npgsql (make simplified stand-alone test first)
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }
                bool cursorHasNextRow = await fetchReader.ReadAsync(cancellationToken);
                if (cursorHasNextRow)
                {
                    Count++;
                    return true;
                }
                // if we did FETCH ALL or if rows expired before requested count, there is nothing more to fetch on this cursor
                if (FetchSize <= 0 || Count < FetchSize)
                {
                    return false;
                }
            }
            // if rows expired at requested count, there may or may not be more rows
            await FetchNextNFromCursorAsync(cancellationToken);
            // recursive self-call
            return await ReadAsync(cancellationToken);
        }
    }
}
#endif