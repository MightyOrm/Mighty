#if !NET40
using System.Collections.Generic;
using System.Collections.Async;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Mighty.Interfaces;
using Mighty.Mapping;
using Mighty.Plugins;
using Mighty.Profiling;
using Mighty.Validation;
using System;
using System.Dynamic;
using System.Collections;

// <summary>
// MightyOrm_Redirects.cs holds methods in Mighty than can be very simply defined in terms of other methods.
// </summary>
namespace Mighty
{
    public partial class MightyOrm<T> : MightyOrmAbstractInterface<T> where T : class, new()
    {
        #region Non-table specific methods
        override public async Task<IAsyncEnumerable<T>> QueryAsync(DbCommand command,
            DbConnection connection = null)
        {
            return await QueryNWithParamsAsync<T>(command: command, connection: connection);
        }
        override public async Task<IAsyncEnumerable<T>> QueryAsync(DbCommand command,
            CancellationToken cancellationToken,
            DbConnection connection = null)
        {
            return await QueryNWithParamsAsync<T>(command: command, cancellationToken: cancellationToken, connection: connection);
        }

        override public async Task<T> SingleAsync(DbCommand command,
            DbConnection connection = null)
        {
            return await
                (await QueryNWithParamsAsync<T>(
                    command: command, connection: connection).ConfigureAwait(false))
                .FirstOrDefaultAsync().ConfigureAwait(false);
        }
        override public async Task<T> SingleAsync(DbCommand command,
            CancellationToken cancellationToken,
            DbConnection connection = null)
        {
            return await
                (await QueryNWithParamsAsync<T>(
                    command: command,
                    cancellationToken: cancellationToken,
                    connection: connection).ConfigureAwait(false))
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        // no connection, easy args
        override public async Task<IAsyncEnumerable<T>> QueryAsync(string sql,
            params object[] args)
        {
            return await QueryNWithParamsAsync<T>(sql, args: args);
        }
        override public async Task<IAsyncEnumerable<T>> QueryAsync(string sql,
            CancellationToken cancellationToken,
            params object[] args)
        {
            return await QueryNWithParamsAsync<T>(cancellationToken, sql, args: args);
        }

        override public async Task<T> SingleFromQueryAsync(string sql,
            params object[] args)
        {
            return await
                (await QueryNWithParamsAsync<T>(sql, args: args).ConfigureAwait(false))
                .FirstOrDefaultAsync().ConfigureAwait(false);
        }
        override public async Task<T> SingleFromQueryAsync(string sql,
            CancellationToken cancellationToken,
            params object[] args)
        {
            return await
                (await QueryNWithParamsAsync<T>(cancellationToken, sql, args: args).ConfigureAwait(false))
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        override public async Task<IAsyncEnumerable<T>> QueryAsync(string sql,
            DbConnection connection,
            params object[] args)
        {
            return await QueryNWithParamsAsync<T>(sql, connection: connection, args: args);
        }
        override public async Task<IAsyncEnumerable<T>> QueryAsync(string sql,
            DbConnection connection,
            CancellationToken cancellationToken,
            params object[] args)
        {
            return await QueryNWithParamsAsync<T>(cancellationToken, sql, connection: connection, args: args);
        }

        override public async Task<T> SingleFromQueryAsync(string sql,
            DbConnection connection,
            params object[] args)
        {
            return await
                (await QueryNWithParamsAsync<T>(sql, connection: connection, args: args).ConfigureAwait(false))
                .FirstOrDefaultAsync().ConfigureAwait(false);
        }
        override public async Task<T> SingleFromQueryAsync(string sql,
            DbConnection connection,
            CancellationToken cancellationToken,
            params object[] args)
        {
            return await
                (await QueryNWithParamsAsync<T>(
                    cancellationToken,
                    sql,
                    connection: connection,
                    args: args).ConfigureAwait(false))
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        override public async Task<IAsyncEnumerable<T>> QueryWithParamsAsync(string sql,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await QueryNWithParamsAsync<T>(sql,
                inParams, outParams, ioParams, returnParams,
                connection: connection, args: args);
        }
        override public async Task<IAsyncEnumerable<T>> QueryWithParamsAsync(string sql,
            CancellationToken cancellationToken,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await QueryNWithParamsAsync<T>(cancellationToken, sql,
                inParams, outParams, ioParams, returnParams,
                connection: connection, args: args);
        }

        override public async Task<T> SingleFromQueryWithParamsAsync(string sql,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await
                (await QueryNWithParamsAsync<T>(
                    sql, inParams, outParams, ioParams, returnParams,
                    connection: connection, args: args).ConfigureAwait(false))
                .FirstOrDefaultAsync().ConfigureAwait(false);
        }
        override public async Task<T> SingleFromQueryWithParamsAsync(string sql,
            CancellationToken cancellationToken,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await
                (await QueryNWithParamsAsync<T>(
                    cancellationToken, sql,
                    inParams, outParams, ioParams, returnParams,
                    connection: connection, args: args).ConfigureAwait(false))
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        override public async Task<IAsyncEnumerable<T>> QueryFromProcedureAsync(string spName,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await QueryNWithParamsAsync<T>(spName,
                inParams, outParams, ioParams, returnParams,
                isProcedure: true,
                connection: connection, args: args);
        }
        override public async Task<IAsyncEnumerable<T>> QueryFromProcedureAsync(string spName,
            CancellationToken cancellationToken,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await QueryNWithParamsAsync<T>(cancellationToken, spName,
                inParams, outParams, ioParams, returnParams,
                isProcedure: true,
                connection: connection, args: args);
        }

        override public async Task<T> SingleFromProcedureAsync(string spName,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await
                (await QueryNWithParamsAsync<T>(
                    spName,
                    inParams, outParams, ioParams, returnParams,
                    isProcedure: true,
                    connection: connection, args: args).ConfigureAwait(false))
                .FirstOrDefaultAsync().ConfigureAwait(false);
        }
        override public async Task<T> SingleFromProcedureAsync(string spName,
            CancellationToken cancellationToken,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await
                (await QueryNWithParamsAsync<T>(
                    cancellationToken, spName,
                    inParams, outParams, ioParams, returnParams,
                    isProcedure: true,
                    connection: connection, args: args).ConfigureAwait(false))
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        override public async Task<IAsyncEnumerable<IAsyncEnumerable<T>>> QueryMultipleAsync(DbCommand command,
            DbConnection connection = null)
        {
            return await QueryNWithParamsAsync<IAsyncEnumerable<T>>(command: command, connection: connection);
        }
        override public async Task<IAsyncEnumerable<IAsyncEnumerable<T>>> QueryMultipleAsync(DbCommand command,
            CancellationToken cancellationToken,
            DbConnection connection = null)
        {
            return await QueryNWithParamsAsync<IAsyncEnumerable<T>>(command: command, cancellationToken: cancellationToken, connection: connection);
        }

        // no connection, easy args
        override public async Task<IAsyncEnumerable<IAsyncEnumerable<T>>> QueryMultipleAsync(string sql,
            params object[] args)
        {
            return await QueryNWithParamsAsync<IAsyncEnumerable<T>>(sql, args: args);
        }
        override public async Task<IAsyncEnumerable<IAsyncEnumerable<T>>> QueryMultipleAsync(string sql,
            CancellationToken cancellationToken,
            params object[] args)
        {
            return await QueryNWithParamsAsync<IAsyncEnumerable<T>>(cancellationToken, sql, args: args);
        }

        override public async Task<IAsyncEnumerable<IAsyncEnumerable<T>>> QueryMultipleAsync(string sql,
            DbConnection connection,
            params object[] args)
        {
            return await QueryNWithParamsAsync<IAsyncEnumerable<T>>(sql, connection: connection, args: args);
        }
        override public async Task<IAsyncEnumerable<IAsyncEnumerable<T>>> QueryMultipleAsync(string sql,
            DbConnection connection,
            CancellationToken cancellationToken,
            params object[] args)
        {
            return await QueryNWithParamsAsync<IAsyncEnumerable<T>>(cancellationToken, sql, connection: connection, args: args);
        }

        override public async Task<IAsyncEnumerable<IAsyncEnumerable<T>>> QueryMultipleWithParamsAsync(string sql,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await QueryNWithParamsAsync<IAsyncEnumerable<T>>(sql,
                inParams, outParams, ioParams, returnParams,
                connection: connection, args: args);
        }
        override public async Task<IAsyncEnumerable<IAsyncEnumerable<T>>> QueryMultipleWithParamsAsync(string sql,
            CancellationToken cancellationToken,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await QueryNWithParamsAsync<IAsyncEnumerable<T>>(cancellationToken, sql,
                inParams, outParams, ioParams, returnParams,
                connection: connection, args: args);
        }

        override public async Task<IAsyncEnumerable<IAsyncEnumerable<T>>> QueryMultipleFromProcedureAsync(string spName,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await QueryNWithParamsAsync<IAsyncEnumerable<T>>(spName,
                inParams, outParams, ioParams, returnParams,
                isProcedure: true,
                connection: connection, args: args);
        }
        override public async Task<IAsyncEnumerable<IAsyncEnumerable<T>>> QueryMultipleFromProcedureAsync(string spName,
            CancellationToken cancellationToken,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await QueryNWithParamsAsync<IAsyncEnumerable<T>>(cancellationToken, spName,
                inParams, outParams, ioParams, returnParams,
                isProcedure: true,
                connection: connection, args: args);
        }

        // no connection, easy args
        override public async Task<int> ExecuteAsync(string sql,
            params object[] args)
        {
            using (var command = CreateCommandWithParams(sql, args: args))
            {
                return await ExecuteAsync(command).ConfigureAwait(false);
            }
        }
        override public async Task<int> ExecuteAsync(string sql,
            CancellationToken cancellationToken,
            params object[] args)
        {
            using (var command = CreateCommandWithParams(sql, args: args))
            {
                return await ExecuteAsync(command, cancellationToken).ConfigureAwait(false);
            }
        }

        override public async Task<int> ExecuteAsync(string sql,
            DbConnection connection,
            params object[] args)
        {
            using (var command = CreateCommandWithParams(sql, args: args))
            {
                return await ExecuteAsync(command, connection).ConfigureAwait(false);
            }
        }
        override public async Task<int> ExecuteAsync(string sql,
            DbConnection connection,
            CancellationToken cancellationToken,
            params object[] args)
        {
            using (var command = CreateCommandWithParams(sql, args: args))
            {
                return await ExecuteAsync(command, cancellationToken, connection).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Execute command with parameters
        /// </summary>
        /// <param name="sql">The command SQL (with optional DB-native parameter placeholders)</param>
        /// <param name="inParams">Named input parameters</param>
        /// <param name="outParams">Named output parameters</param>
        /// <param name="ioParams">Named input-output parameters</param>
        /// <param name="returnParams">Named return parameters</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <returns>The results of all non-input parameters</returns>
        override public async Task<dynamic> ExecuteWithParamsAsync(string sql,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await ExecuteWithParamsAsync(sql,
                CancellationToken.None,
                inParams, outParams, ioParams, returnParams,
                connection,
                args).ConfigureAwait(false);
        }

        /// <summary>
        /// Execute command with parameters
        /// </summary>
        /// <param name="sql">The command SQL (with optional DB-native parameter placeholders)</param>
        /// <param name="inParams">Named input parameters</param>
        /// <param name="outParams">Named output parameters</param>
        /// <param name="ioParams">Named input-output parameters</param>
        /// <param name="returnParams">Named return parameters</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns>The results of all non-input parameters</returns>
        override public async Task<dynamic> ExecuteWithParamsAsync(string sql,
            CancellationToken cancellationToken,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            var retval = CreateCommandWithParamsAndRowCountCheck(sql,
                inParams, outParams, ioParams, returnParams,
                args: args);
            using (retval.Item1)
            {
                var rowCount = await ExecuteAsync(retval.Item1, connection).ConfigureAwait(false);
                var results = ResultsAsExpando(retval.Item1);
                if (retval.Item2)
                {
                    AppendRowCountResults(rowCount, outParams, results);
                }
                return results;
            }
        }

        /// <summary>
        /// Execute stored procedure with parameters
        /// </summary>
        /// <param name="spName">Stored procedure name</param>
        /// <param name="inParams">Named input parameters</param>
        /// <param name="outParams">Named output parameters</param>
        /// <param name="ioParams">Named input-output parameters</param>
        /// <param name="returnParams">Named return parameters</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <returns>The results of all non-input parameters</returns>
        override public async Task<dynamic> ExecuteProcedureAsync(string spName,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await ExecuteProcedureAsync(spName,
                CancellationToken.None,
                inParams, outParams, ioParams, returnParams,
                connection,
                args);
        }

        /// <summary>
        /// Execute stored procedure with parameters
        /// </summary>
        /// <param name="spName">Stored procedure name</param>
        /// <param name="inParams">Named input parameters</param>
        /// <param name="outParams">Named output parameters</param>
        /// <param name="ioParams">Named input-output parameters</param>
        /// <param name="returnParams">Named return parameters</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns>The results of all non-input parameters</returns>
        override public async Task<dynamic> ExecuteProcedureAsync(string spName,
            CancellationToken cancellationToken,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            var retval = CreateCommandWithParamsAndRowCountCheck(spName,
                inParams, outParams, ioParams, returnParams,
                isProcedure: true,
                args: args);
            using (retval.Item1)
            {
                var rowCount = await ExecuteAsync(retval.Item1, cancellationToken, connection).ConfigureAwait(false);
                var results = ResultsAsExpando(retval.Item1);
                if (retval.Item2)
                {
                    AppendRowCountResults(rowCount, outParams, results);
                }
                return results;
            }
        }

        // no connection, easy args
        override public async Task<object> ScalarAsync(string sql,
            params object[] args)
        {
            using (var command = CreateCommand(sql, args))
            {
                return await ScalarAsync(command).ConfigureAwait(false);
            }
        }
        override public async Task<object> ScalarAsync(string sql,
            CancellationToken cancellationToken,
            params object[] args)
        {
            using (var command = CreateCommand(sql, args))
            {
                return await ScalarAsync(command, cancellationToken).ConfigureAwait(false);
            }
        }

        override public async Task<object> ScalarAsync(string sql,
            DbConnection connection,
            params object[] args)
        {
            using (var command = CreateCommand(sql, args))
            {
                return await ScalarAsync(command, connection).ConfigureAwait(false);
            }
        }
        override public async Task<object> ScalarAsync(string sql,
            DbConnection connection,
            CancellationToken cancellationToken,
            params object[] args)
        {
            using (var command = CreateCommand(sql, args))
            {
                return await ScalarAsync(command, cancellationToken, connection).ConfigureAwait(false);
            }
        }

        override public async Task<object> ScalarWithParamsAsync(string sql,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            using (var command = CreateCommandWithParams(sql,
                inParams, outParams, ioParams, returnParams,
                args: args))
            {
                return await ScalarAsync(command, connection).ConfigureAwait(false);
            }
        }
        override public async Task<object> ScalarWithParamsAsync(string sql,
            CancellationToken cancellationToken,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            using (var command = CreateCommandWithParams(sql,
                inParams, outParams, ioParams, returnParams,
                args: args))
            {
                return await ScalarAsync(command, cancellationToken, connection).ConfigureAwait(false);
            }
        }

        override public async Task<object> ScalarFromProcedureAsync(string spName,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            using (var command = CreateCommandWithParams(spName,
                inParams, outParams, ioParams, returnParams,
                isProcedure: true,
                args: args))
            {
                return await ScalarAsync(command, connection).ConfigureAwait(false);
            }
        }
        override public async Task<object> ScalarFromProcedureAsync(string spName,
            CancellationToken cancellationToken,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            using (var command = CreateCommandWithParams(spName,
                inParams, outParams, ioParams, returnParams,
                isProcedure: true,
                args: args))
            {
                return await ScalarAsync(command, cancellationToken, connection).ConfigureAwait(false);
            }
        }

        override protected async Task<IAsyncEnumerable<X>> QueryNWithParamsAsync<X>(string sql = null, object inParams = null, object outParams = null, object ioParams = null, object returnParams = null, bool isProcedure = false, CommandBehavior behavior = CommandBehavior.Default, DbConnection connection = null, params object[] args)
        {
            var command = CreateCommandWithParams(sql, inParams, outParams, ioParams, returnParams, isProcedure, null, args);
            return await QueryNWithParamsAsync<X>(command, behavior, connection);
        }
        override protected async Task<IAsyncEnumerable<X>> QueryNWithParamsAsync<X>(CancellationToken cancellationToken, string sql = null, object inParams = null, object outParams = null, object ioParams = null, object returnParams = null, bool isProcedure = false, CommandBehavior behavior = CommandBehavior.Default, DbConnection connection = null, params object[] args)
        {
            var command = CreateCommandWithParams(sql, inParams, outParams, ioParams, returnParams, isProcedure, null, args);
            return await QueryNWithParamsAsync<X>(command, cancellationToken, behavior, connection);
        }
        #endregion

        #region Table specific methods
        // In theory COUNT expression could vary across SQL variants, in practice it doesn't.

        /// <summary>
        /// Perform COUNT on current table.
        /// </summary>
        /// <param name="where">WHERE clause</param>
        /// <param name="columns">Columns (defaults to *, but can be specified, e.g., to count non-nulls in a given field)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <returns></returns>
        override public async Task<object> CountAsync(
            string where = null,
            string columns = "*",
            DbConnection connection = null,
            params object[] args)
        {
            return await AggregateWithParamsAsync("COUNT", columns, CancellationToken.None, where, connection, args: args).ConfigureAwait(false);
        }

        /// <summary>
        /// Perform COUNT on current table.
        /// </summary>
        /// <param name="where">WHERE clause</param>
        /// <param name="columns">Columns (defaults to *, but can be specified, e.g., to count non-nulls in a given field)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        override public async Task<object> CountAsync(
            CancellationToken cancellationToken,
            string where = null,
            string columns = "*",
            DbConnection connection = null,
            params object[] args)
        {
            return await AggregateWithParamsAsync("COUNT", columns, cancellationToken, where, connection, args: args).ConfigureAwait(false);
        }

        /// <summary>
        /// Perform COUNT on current table.
        /// </summary>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="columns">Columns (defaults to *, but can be specified, e.g., to count non-nulls in a given field)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        override public async Task<object> CountAsync(
            object whereParams = null,
            string columns = "*",
            DbConnection connection = null)
        {
            return await AggregateAsync("COUNT", columns, CancellationToken.None, whereParams, connection).ConfigureAwait(false);
        }

        /// <summary>
        /// Perform COUNT on current table.
        /// </summary>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="columns">Columns (defaults to *, but can be specified, e.g., to count non-nulls in a given field)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        override public async Task<object> CountAsync(
            CancellationToken cancellationToken,
            object whereParams = null,
            string columns = "*",
            DbConnection connection = null)
        {
            return await AggregateAsync("COUNT", columns, cancellationToken, whereParams, connection).ConfigureAwait(false);
        }

        /// <summary>
        /// Get MAX of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <returns></returns>
        override public async Task<object> MaxAsync(
            string columns,
            string where = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await AggregateWithParamsAsync("MAX", columns, CancellationToken.None, where, connection, args: args).ConfigureAwait(false);
        }

        /// <summary>
        /// Get MAX of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        override public async Task<object> MaxAsync(
            string columns,
            CancellationToken cancellationToken,
            string where = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await AggregateWithParamsAsync("MAX", columns, cancellationToken, where, connection, args: args).ConfigureAwait(false);
        }

        /// <summary>
        /// Get MAX of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        override public async Task<object> MaxAsync(
            string columns,
            object whereParams = null,
            DbConnection connection = null)
        {
            return await AggregateAsync("MAX", columns, CancellationToken.None, whereParams, connection).ConfigureAwait(false);
        }

        /// <summary>
        /// Get MAX of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        override public async Task<object> MaxAsync(
            string columns,
            CancellationToken cancellationToken,
            object whereParams = null,
            DbConnection connection = null)
        {
            return await AggregateAsync("MAX", columns, cancellationToken, whereParams, connection).ConfigureAwait(false);
        }

        /// <summary>
        /// Get MIN of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <returns></returns>
        override public async Task<object> MinAsync(
            string columns,
            string where = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await AggregateWithParamsAsync("MIN", columns, CancellationToken.None, where, connection, args: args).ConfigureAwait(false);
        }

        /// <summary>
        /// Get MIN of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        override public async Task<object> MinAsync(
            string columns,
            CancellationToken cancellationToken,
            string where = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await AggregateWithParamsAsync("MIN", columns, cancellationToken, where, connection, args: args).ConfigureAwait(false);
        }

        /// <summary>
        /// Get MIN of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        override public async Task<object> MinAsync(
            string columns,
            object whereParams = null,
            DbConnection connection = null)
        {
            return await AggregateAsync("MIN", columns, CancellationToken.None, whereParams, connection).ConfigureAwait(false);
        }

        /// <summary>
        /// Get MIN of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        override public async Task<object> MinAsync(
            string columns,
            CancellationToken cancellationToken,
            object whereParams = null,
            DbConnection connection = null)
        {
            return await AggregateAsync("MIN", columns, cancellationToken, whereParams, connection).ConfigureAwait(false);
        }

        /// <summary>
        /// Get SUM of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <returns></returns>
        override public async Task<object> SumAsync(
            string columns,
            string where = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await AggregateWithParamsAsync("SUM", columns, CancellationToken.None, where, connection, args: args).ConfigureAwait(false);
        }

        /// <summary>
        /// Get SUM of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        override public async Task<object> SumAsync(
            string columns,
            CancellationToken cancellationToken,
            string where = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await AggregateWithParamsAsync("SUM", columns, cancellationToken, where, connection, args: args).ConfigureAwait(false);
        }

        /// <summary>
        /// Get SUM of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        override public async Task<object> SumAsync(
            string columns,
            object whereParams = null,
            DbConnection connection = null)
        {
            return await AggregateAsync("SUM", columns, CancellationToken.None, whereParams, connection).ConfigureAwait(false);
        }

        /// <summary>
        /// Get SUM of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        override public async Task<object> SumAsync(
            string columns,
            CancellationToken cancellationToken,
            object whereParams = null,
            DbConnection connection = null)
        {
            return await AggregateAsync("SUM", columns, cancellationToken, whereParams, connection).ConfigureAwait(false);
        }

        /// <summary>
        /// Get AVG of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <returns></returns>
        override public async Task<object> AvgAsync(
            string columns,
            string where = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await AggregateWithParamsAsync("AVG", columns, CancellationToken.None, where, connection, args: args).ConfigureAwait(false);
        }

        /// <summary>
        /// Get AVG of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        override public async Task<object> AvgAsync(
            string columns,
            CancellationToken cancellationToken,
            string where = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await AggregateWithParamsAsync("AVG", columns, cancellationToken, where, connection, args: args).ConfigureAwait(false);
        }

        /// <summary>
        /// Get AVG of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        override public async Task<object> AvgAsync(
            string columns,
            object whereParams = null,
            DbConnection connection = null)
        {
            return await AggregateAsync("AVG", columns, CancellationToken.None, whereParams, connection).ConfigureAwait(false);
        }

        /// <summary>
        /// Get AVG of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        override public async Task<object> AvgAsync(
            string columns,
            CancellationToken cancellationToken,
            object whereParams = null,
            DbConnection connection = null)
        {
            return await AggregateAsync("AVG", columns, cancellationToken, whereParams, connection).ConfigureAwait(false);
        }

        /// <summary>
        /// Perform aggregate operation on the current table (use for SUM, MAX, MIN, AVG, etc.)
        /// </summary>
        /// <param name="function">Aggregate function</param>
        /// <param name="columns">Columns for aggregate function</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <returns></returns>
        override public async Task<object> AggregateAsync(string function, string columns, string where = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await AggregateWithParamsAsync(function, columns, CancellationToken.None, where, connection: connection, args: args).ConfigureAwait(false);
        }

        /// <summary>
        /// Perform aggregate operation on the current table (use for SUM, MAX, MIN, AVG, etc.)
        /// </summary>
        /// <param name="function">Aggregate function</param>
        /// <param name="columns">Columns for aggregate function</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        override public async Task<object> AggregateAsync(string function, string columns, CancellationToken cancellationToken, string where = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await AggregateWithParamsAsync(function, columns, cancellationToken, where, connection: connection, args: args).ConfigureAwait(false);
        }

        /// <summary>
        /// Perform aggregate operation on the current table (use for SUM, MAX, MIN, AVG, etc.)
        /// </summary>
        /// <param name="function">Aggregate function</param>
        /// <param name="columns">Columns for aggregate function</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        override public async Task<object> AggregateAsync(string function, string columns, object whereParams = null,
            DbConnection connection = null)
        {
            return await AggregateAsync(function, columns, CancellationToken.None, whereParams, connection: connection).ConfigureAwait(false);
        }

        /// <summary>
        /// Perform aggregate operation on the current table (use for SUM, MAX, MIN, AVG, etc.)
        /// </summary>
        /// <param name="function">Aggregate function</param>
        /// <param name="columns">Columns for aggregate function</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        override public async Task<object> AggregateAsync(string function, string columns, CancellationToken cancellationToken, object whereParams = null,
            DbConnection connection = null)
        {
            Tuple<string, object, object[]> retval = GetWhereSpecFromWhereParams(whereParams);
            return await AggregateWithParamsAsync(
                function, columns,
                where: retval.Item1, inParams: retval.Item2, args: retval.Item3,
                connection: connection, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Get single object from the current table using primary key or name-value specification.
        /// </summary>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="columns">List of columns to return</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        override public async Task<T> SingleAsync(object whereParams, string columns = null,
            DbConnection connection = null)
        {
            return await SingleAsync(whereParams, CancellationToken.None, columns, connection).ConfigureAwait(false);
        }

        /// <summary>
        /// Get single object from the current table using primary key or name-value specification.
        /// </summary>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="columns">List of columns to return</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        override public async Task<T> SingleAsync(object whereParams, CancellationToken cancellationToken, string columns = null,
            DbConnection connection = null)
        {
            Tuple<string, object, object[]> retval = GetWhereSpecFromWhereParams(whereParams);
            return await
                (await AllWithParamsAsync(
                    where: retval.Item1, inParams: retval.Item2, args: retval.Item3, columns: columns, limit: 1,
                    connection: connection, cancellationToken: cancellationToken).ConfigureAwait(false))
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Get a single object from the current table with where specification.
        /// </summary>
        /// <param name="where">WHERE clause</param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <returns></returns>
        /// <remarks>
        /// 'Easy-calling' version, optional args straight after where.
        /// </remarks>
        override public async Task<T> SingleAsync(string where,
            params object[] args)
        {
            return await SingleWithParamsAsync(where, args: args).ConfigureAwait(false);
        }
        override public async Task<T> SingleAsync(string where,
            CancellationToken cancellationToken,
            params object[] args)
        {
            return await SingleWithParamsAsync(where, cancellationToken, args: args).ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="orderBy">ORDER BY clause</param>
        /// <param name="columns">Comma separated list of columns to return or "*"</param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <returns></returns>
        /// <remarks>
        /// DbConnection coming early (not just before args) in this one case is really useful, as it avoids ambiguity between
        /// the `columns` and `orderBy` strings and optional string args.
        /// </remarks>
        override public async Task<T> SingleAsync(string where,
            DbConnection connection = null,
            string orderBy = null,
            string columns = null,
            params object[] args)
        {
            return await SingleWithParamsAsync(where, orderBy, columns, connection: connection, args: args).ConfigureAwait(false);
        }
        override public async Task<T> SingleAsync(string where,
            CancellationToken cancellationToken,
            DbConnection connection = null,
            string orderBy = null,
            string columns = null,
            params object[] args)
        {
            return await SingleWithParamsAsync(where, cancellationToken, orderBy, columns, connection: connection, args: args).ConfigureAwait(false);
        }

        // WithParams version just in case; allows transactions for a start
        override public async Task<T> SingleWithParamsAsync(string where, string orderBy = null, string columns = null,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await
                (await AllWithParamsAsync(
                    where, orderBy, columns, 1,
                    inParams, outParams, ioParams, returnParams,
                    connection,
                    args).ConfigureAwait(false))
                .FirstOrDefaultAsync().ConfigureAwait(false);
        }
        override public async Task<T> SingleWithParamsAsync(string where, CancellationToken cancellationToken, string orderBy = null, string columns = null,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await
                (await AllWithParamsAsync(
                    cancellationToken,
                    where, orderBy, columns, 1,
                    inParams, outParams, ioParams, returnParams,
                    connection,
                    args).ConfigureAwait(false))
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        // ORM
        override public async Task<IAsyncEnumerable<T>> AllAsync(
            string where = null, string orderBy = null, string columns = null, int limit = 0,
            params object[] args)
        {
            return await AllWithParamsAsync(where, orderBy, columns, limit, args: args).ConfigureAwait(false);
        }
        override public async Task<IAsyncEnumerable<T>> AllAsync(
            CancellationToken cancellationToken,
            string where = null, string orderBy = null, string columns = null, int limit = 0,
            params object[] args)
        {
            return await AllWithParamsAsync(cancellationToken, where, orderBy, columns, limit, args: args).ConfigureAwait(false);
        }

        override public async Task<IAsyncEnumerable<T>> AllAsync(
            object whereParams = null, string orderBy = null, string columns = null, int limit = 0)
        {
            return await AllAsync(CancellationToken.None, whereParams, orderBy, columns, limit);
        }
        override public async Task<IAsyncEnumerable<T>> AllAsync(
            CancellationToken cancellationToken,
            object whereParams = null, string orderBy = null, string columns = null, int limit = 0)
        {
            Tuple<string, object, object[]> retval = GetWhereSpecFromWhereParams(whereParams);
            if (retval.Item3 != null)
            {
                throw new InvalidOperationException($"{nameof(whereParams)} in {nameof(AllAsync)}(...) should contain names and values but it contained values only. If you want to get a single item by its primary key use {nameof(SingleAsync)}(...) instead.");
            }
            return await AllWithParamsAsync(
                cancellationToken,
                where: retval.Item1, inParams: retval.Item2,
                orderBy: orderBy, columns: columns, limit: limit);
        }

        /// <summary>
        /// Table-specific paging; there is also a data wrapper version of paging <see cref="PagedFromSelect"/>.
        /// </summary>
        /// <param name="orderBy">You may provide orderBy, if you don't it will try to order by PK and will produce an exception if there is no PK defined.</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="columns">Columns to return</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="currentPage">Current page</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <returns>The result of the paged query. Result properties are Items, TotalPages, and TotalRecords.</returns>
        /// <remarks>
        /// `columns` parameter is not placed first because it's an override to something we may have alread provided in the constructor
        /// (so we don't want the user to have to non-fluently re-type it, or else type null, every time).
        /// </remarks>
        override public async Task<PagedResults<T>> PagedAsync(
            string orderBy = null,
            string columns = null,
            string where = null,
            int pageSize = 20, int currentPage = 1,
            DbConnection connection = null,
            params object[] args)
        {
            return await PagedFromSelectAsync(
                CheckGetTableName(),
                orderBy ?? CheckGetPrimaryKeyFields(),
                columns,
                where,
                pageSize, currentPage,
                connection,
                args).ConfigureAwait(false);
        }

        /// <summary>
        /// Table-specific paging; there is also a data wrapper version of paging <see cref="PagedFromSelect"/>.
        /// </summary>
        /// <param name="orderBy">You may provide orderBy, if you don't it will try to order by PK and will produce an exception if there is no PK defined.</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="columns">Columns to return</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="currentPage">Current page</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns>The result of the paged query. Result properties are Items, TotalPages, and TotalRecords.</returns>
        /// <remarks>
        /// `columns` parameter is not placed first because it's an override to something we may have alread provided in the constructor
        /// (so we don't want the user to have to non-fluently re-type it, or else type null, every time).
        /// </remarks>
        override public async Task<PagedResults<T>> PagedAsync(
            CancellationToken cancellationToken,
            string orderBy = null,
            string columns = null,
            string where = null,
            int pageSize = 20, int currentPage = 1,
            DbConnection connection = null,
            params object[] args)
        {
            return await PagedFromSelectAsync(
                CheckGetTableName(),
                orderBy ?? CheckGetPrimaryKeyFields(),
                cancellationToken,
                columns,
                where,
                pageSize, currentPage,
                connection,
                args).ConfigureAwait(false);
        }

        /// <summary>
        /// Save one or more items using params style arguments.
        /// 'Save' means
        /// objects with missing (applies to dynamic only) or default primary keys are inserted
        /// and objects with non-default primary keys are updated.
        /// </summary>
        /// <param name="items">The items</param>
        /// <returns></returns>
        override public async Task<int> SaveAsync(params object[] items)
        {
            return (await ActionOnItemsWithOutputAsync(OrmAction.Save, null, items).ConfigureAwait(false)).Item1;
        }

        /// <summary>
        /// Save one or more items using params style arguments.
        /// 'Save' means
        /// objects with missing (applies to dynamic only) or default primary keys are inserted
        /// and objects with non-default primary keys are updated.
        /// </summary>
        /// <param name="items">The items</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        override public async Task<int> SaveAsync(CancellationToken cancellationToken, params object[] items)
        {
            return (await ActionOnItemsWithOutputAsync(OrmAction.Save, null, items, cancellationToken).ConfigureAwait(false)).Item1;
        }

        /// <summary>
        /// Save one or more items using params style arguments.
        /// 'Save' means
        /// objects with missing (applies to dynamic only) or default primary keys are inserted
        /// and objects with non-default primary keys are updated.
        /// </summary>
        /// <param name="connection">The connection to use</param>
        /// <param name="items">The items</param>
        /// <returns></returns>
        override public async Task<int> SaveAsync(DbConnection connection, params object[] items)
        {
            return (await ActionOnItemsWithOutputAsync(OrmAction.Save, connection, items).ConfigureAwait(false)).Item1;
        }

        /// <summary>
        /// Save one or more items using params style arguments.
        /// 'Save' means
        /// objects with missing (applies to dynamic only) or default primary keys are inserted
        /// and objects with non-default primary keys are updated.
        /// </summary>
        /// <param name="connection">The connection to use</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="items">The items</param>
        /// <returns></returns>
        override public async Task<int> SaveAsync(DbConnection connection, CancellationToken cancellationToken, params object[] items)
        {
            return (await ActionOnItemsWithOutputAsync(OrmAction.Save, connection, items, cancellationToken).ConfigureAwait(false)).Item1;
        }

        /// <summary>
        /// Save array or other <see cref="IEnumerable"/> of items.
        /// 'Save' means
        /// objects with missing (applies to dynamic only) or default primary keys are inserted
        /// and objects with non-default primary keys are updated.
        /// </summary>
        /// <param name="items">The items</param>
        /// <returns></returns>
        override public async Task<int> SaveAsync(IEnumerable<object> items)
        {
            return (await ActionOnItemsWithOutputAsync(OrmAction.Save, null, items).ConfigureAwait(false)).Item1;
        }

        /// <summary>
        /// Save array or other <see cref="IEnumerable"/> of items.
        /// 'Save' means
        /// objects with missing (applies to dynamic only) or default primary keys are inserted
        /// and objects with non-default primary keys are updated.
        /// </summary>
        /// <param name="items">The items</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        override public async Task<int> SaveAsync(IEnumerable<object> items, CancellationToken cancellationToken)
        {
            return (await ActionOnItemsWithOutputAsync(OrmAction.Save, null, items, cancellationToken).ConfigureAwait(false)).Item1;
        }

        /// <summary>
        /// Save array or other <see cref="IEnumerable"/> of items.
        /// 'Save' means
        /// objects with missing (applies to dynamic only) or default primary keys are inserted
        /// and objects with non-default primary keys are updated.
        /// </summary>
        /// <param name="connection">The connection to use</param>
        /// <param name="items">The items</param>
        /// <returns></returns>
        override public async Task<int> SaveAsync(DbConnection connection, IEnumerable<object> items)
        {
            return (await ActionOnItemsWithOutputAsync(OrmAction.Save, connection, items).ConfigureAwait(false)).Item1;
        }

        /// <summary>
        /// Save array or other <see cref="IEnumerable"/> of items.
        /// 'Save' means
        /// objects with missing (applies to dynamic only) or default primary keys are inserted
        /// and objects with non-default primary keys are updated.
        /// </summary>
        /// <param name="connection">The connection to use</param>
        /// <param name="items">The items</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        override public async Task<int> SaveAsync(DbConnection connection, IEnumerable<object> items, CancellationToken cancellationToken)
        {
            return (await ActionOnItemsWithOutputAsync(OrmAction.Save, connection, items, cancellationToken).ConfigureAwait(false)).Item1;
        }

        /// <summary>
        /// Insert single item, returning the item sent in but with PK populated.
        /// If you need all fields populated (i.e. you want to get back DB default values for non-PK fields), please create the item using New() before inserting it.
        /// </summary>
        /// <param name="item">The item to insert, in any reasonable format (for MightyOrm&lt;T&gt; this includes, but is not limited to, in instance of type T)</param>
        /// <returns>The inserted item</returns>
        override public async Task<T> InsertAsync(object item)
        {
            return (await ActionOnItemsAsync(OrmAction.Insert, null, new object[] { item }).ConfigureAwait(false)).FirstOrDefault();
        }

        /// <summary>
        /// Insert single item, returning the item sent in but with PK populated.
        /// If you need all fields populated (i.e. you want to get back DB default values for non-PK fields), please create the item using New() before inserting it.
        /// </summary>
        /// <param name="item">The item to insert, in any reasonable format (for MightyOrm&lt;T&gt; this includes, but is not limited to, in instance of type T)</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns>The inserted item</returns>
        override public async Task<T> InsertAsync(object item, CancellationToken cancellationToken)
        {
            return (await ActionOnItemsAsync(OrmAction.Insert, null, new object[] { item }, cancellationToken).ConfigureAwait(false)).FirstOrDefault();
        }

        /// <summary>
        /// Insert one or more items using params style arguments.
        /// </summary>
        /// <param name="items">The items</param>
        /// <returns>The number of rows inserted</returns>
        override public async Task<IEnumerable<T>> InsertAsync(params object[] items)
        {
            return await ActionOnItemsAsync(OrmAction.Insert, null, items).ConfigureAwait(false);
        }
        override public async Task<IEnumerable<T>> InsertAsync(CancellationToken cancellationToken, params object[] items)
        {
            return await ActionOnItemsAsync(OrmAction.Insert, null, items, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Insert one or more items using params style arguments.
        /// </summary>
        /// <param name="connection">The connection to use</param>
        /// <param name="items">The items</param>
        /// <returns>The number of rows inserted</returns>
        override public async Task<IEnumerable<T>> InsertAsync(DbConnection connection, params object[] items)
        {
            return await ActionOnItemsAsync(OrmAction.Insert, connection, items).ConfigureAwait(false);
        }
        override public async Task<IEnumerable<T>> InsertAsync(DbConnection connection, CancellationToken cancellationToken, params object[] items)
        {
            return await ActionOnItemsAsync(OrmAction.Insert, connection, items, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Insert array or other <see cref="IEnumerable"/> of items.
        /// </summary>
        /// <param name="items">The items</param>
        /// <returns>The number of rows inserted</returns>
        override public async Task<IEnumerable<T>> InsertAsync(IEnumerable<object> items)
        {
            return await ActionOnItemsAsync(OrmAction.Insert, null, items).ConfigureAwait(false);
        }
        override public async Task<IEnumerable<T>> InsertAsync(IEnumerable<object> items, CancellationToken cancellationToken)
        {
            return await ActionOnItemsAsync(OrmAction.Insert, null, items, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Insert array or other <see cref="IEnumerable"/> of items.
        /// </summary>
        /// <param name="connection">The connection to use</param>
        /// <param name="items">The items</param>
        /// <returns>The number of rows inserted</returns>
        override public async Task<IEnumerable<T>> InsertAsync(DbConnection connection, IEnumerable<object> items)
        {
            return await ActionOnItemsAsync(OrmAction.Insert, connection, items).ConfigureAwait(false);
        }
        override public async Task<IEnumerable<T>> InsertAsync(DbConnection connection, IEnumerable<object> items, CancellationToken cancellationToken)
        {
            return await ActionOnItemsAsync(OrmAction.Insert, connection, items, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Update one or more items using params style arguments.
        /// </summary>
        /// <param name="items">The items</param>
        /// <returns></returns>
        override public async Task<int> UpdateAsync(params object[] items)
        {
            return (await ActionOnItemsWithOutputAsync(OrmAction.Update, null, items).ConfigureAwait(false)).Item1;
        }
        override public async Task<int> UpdateAsync(CancellationToken cancellationToken, params object[] items)
        {
            return (await ActionOnItemsWithOutputAsync(OrmAction.Update, null, items, cancellationToken).ConfigureAwait(false)).Item1;
        }

        /// <summary>
        /// Update one or more items using params style arguments.
        /// </summary>
        /// <param name="connection">The connection to use</param>
        /// <param name="items">The items</param>
        /// <returns></returns>
        override public async Task<int> UpdateAsync(DbConnection connection, params object[] items)
        {
            return (await ActionOnItemsWithOutputAsync(OrmAction.Update, connection, items).ConfigureAwait(false)).Item1;
        }
        override public async Task<int> UpdateAsync(DbConnection connection, CancellationToken cancellationToken, params object[] items)
        {
            return (await ActionOnItemsWithOutputAsync(OrmAction.Update, connection, items, cancellationToken).ConfigureAwait(false)).Item1;
        }

        /// <summary>
        /// Update array or other <see cref="IEnumerable"/> of items.
        /// </summary>
        /// <param name="items">The items</param>
        /// <returns></returns>
        override public async Task<int> UpdateAsync(IEnumerable<object> items)
        {
            return (await ActionOnItemsWithOutputAsync(OrmAction.Update, null, items).ConfigureAwait(false)).Item1;
        }
        override public async Task<int> UpdateAsync(IEnumerable<object> items, CancellationToken cancellationToken)
        {
            return (await ActionOnItemsWithOutputAsync(OrmAction.Update, null, items, cancellationToken).ConfigureAwait(false)).Item1;
        }

        /// <summary>
        /// Update array or other <see cref="IEnumerable"/> of items.
        /// </summary>
        /// <param name="connection">The connection to use</param>
        /// <param name="items">The items</param>
        /// <returns></returns>
        override public async Task<int> UpdateAsync(DbConnection connection, IEnumerable<object> items)
        {
            return (await ActionOnItemsWithOutputAsync(OrmAction.Update, connection, items).ConfigureAwait(false)).Item1;
        }
        override public async Task<int> UpdateAsync(DbConnection connection, IEnumerable<object> items, CancellationToken cancellationToken)
        {
            return (await ActionOnItemsWithOutputAsync(OrmAction.Update, connection, items, cancellationToken).ConfigureAwait(false)).Item1;
        }

        /// <summary>
        /// Delete one or more items using params style arguments.
        /// Each argument may be (or contain) a value (or values) only, in which case
        /// it specifies the primary key value(s) of the item to delete, or it can be any object containing name-values pairs in which case
        /// it should contain fields with names matching the primary key(s) whose values will specify the item to delete (but it may contain
        /// other fields as well which will be ignored here).
        /// </summary>
        /// <param name="items">The items</param>
        /// <returns>The number of items affected</returns>
        override public async Task<int> DeleteAsync(params object[] items)
        {
            return (await ActionOnItemsWithOutputAsync(OrmAction.Delete, null, items).ConfigureAwait(false)).Item1;
        }

        /// <summary>
        /// Delete one or more items using params style arguments.
        /// Each argument may be (or contain) a value (or values) only, in which case
        /// it specifies the primary key value(s) of the item to delete, or it can be any object containing name-values pairs in which case
        /// it should contain fields with names matching the primary key(s) whose values will specify the item to delete (but it may contain
        /// other fields as well which will be ignored here).
        /// </summary>
        /// <param name="items">The items</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns>The number of items affected</returns>
        override public async Task<int> DeleteAsync(CancellationToken cancellationToken, params object[] items)
        {
            return (await ActionOnItemsWithOutputAsync(OrmAction.Delete, null, items, cancellationToken).ConfigureAwait(false)).Item1;
        }

        /// <summary>
        /// Delete one or more items using params style arguments.
        /// Each argument may be (or contain) a value (or values) only, in which case
        /// it specifies the primary key value(s) of the item to delete, or it can be any object containing name-values pairs in which case
        /// it should contain fields with names matching the primary key(s) whose values will specify the item to delete (but it may contain
        /// other fields as well which will be ignored here).
        /// </summary>
        /// <param name="items">The items</param>
        /// <param name="connection">The connection to use</param>
        /// <returns>The number of items affected</returns>
        override public async Task<int> DeleteAsync(DbConnection connection, params object[] items)
        {
            return (await ActionOnItemsWithOutputAsync(OrmAction.Delete, connection, items).ConfigureAwait(false)).Item1;
        }

        /// <summary>
        /// Delete one or more items using params style arguments.
        /// Each argument may be (or contain) a value (or values) only, in which case
        /// it specifies the primary key value(s) of the item to delete, or it can be any object containing name-values pairs in which case
        /// it should contain fields with names matching the primary key(s) whose values will specify the item to delete (but it may contain
        /// other fields as well which will be ignored here).
        /// </summary>
        /// <param name="items">The items</param>
        /// <param name="connection">The connection to use</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns>The number of items affected</returns>
        override public async Task<int> DeleteAsync(DbConnection connection, CancellationToken cancellationToken, params object[] items)
        {
            return (await ActionOnItemsWithOutputAsync(OrmAction.Delete, connection, items, cancellationToken).ConfigureAwait(false)).Item1;
        }

        /// <summary>
        /// Delete an array or other <see cref="IEnumerable"/> of items.
        /// Each argument may be (or contain) a value (or values) only, in which case
        /// it specifies the primary key value(s) of the item to delete, or it can be any object containing name-values pairs in which case
        /// it should contain fields with names matching the primary key(s) whose values will specify the item to delete (but it may contain
        /// other fields as well which will be ignored here).
        /// </summary>
        /// <param name="items">The items</param>
        /// <returns>The number of items affected</returns>
        override public async Task<int> DeleteAsync(IEnumerable<object> items)
        {
            return (await ActionOnItemsWithOutputAsync(OrmAction.Delete, null, items).ConfigureAwait(false)).Item1;
        }

        /// <summary>
        /// Delete an array or other <see cref="IEnumerable"/> of items.
        /// Each argument may be (or contain) a value (or values) only, in which case
        /// it specifies the primary key value(s) of the item to delete, or it can be any object containing name-values pairs in which case
        /// it should contain fields with names matching the primary key(s) whose values will specify the item to delete (but it may contain
        /// other fields as well which will be ignored here).
        /// </summary>
        /// <param name="items">The items</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns>The number of items affected</returns>
        override public async Task<int> DeleteAsync(IEnumerable<object> items, CancellationToken cancellationToken)
        {
            return (await ActionOnItemsWithOutputAsync(OrmAction.Delete, null, items, cancellationToken).ConfigureAwait(false)).Item1;
        }

        /// <summary>
        /// Delete an array or other <see cref="IEnumerable"/> of items.
        /// Each argument may be (or contain) a value (or values) only, in which case
        /// it specifies the primary key value(s) of the item to delete, or it can be any object containing name-values pairs in which case
        /// it should contain fields with names matching the primary key(s) whose values will specify the item to delete (but it may contain
        /// other fields as well which will be ignored here).
        /// </summary>
        /// <param name="items">The items</param>
        /// <param name="connection">The connection to use</param>
        /// <returns>The number of items affected</returns>
        override public async Task<int> DeleteAsync(DbConnection connection, IEnumerable<object> items)
        {
            return (await ActionOnItemsWithOutputAsync(OrmAction.Delete, connection, items).ConfigureAwait(false)).Item1;
        }

        /// <summary>
        /// Delete an array or other <see cref="IEnumerable"/> of items.
        /// Each argument may be (or contain) a value (or values) only, in which case
        /// it specifies the primary key value(s) of the item to delete, or it can be any object containing name-values pairs in which case
        /// it should contain fields with names matching the primary key(s) whose values will specify the item to delete (but it may contain
        /// other fields as well which will be ignored here).
        /// </summary>
        /// <param name="items">The items</param>
        /// <param name="connection">The connection to use</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns>The number of items affected</returns>
        override public async Task<int> DeleteAsync(DbConnection connection, IEnumerable<object> items, CancellationToken cancellationToken)
        {
            return (await ActionOnItemsWithOutputAsync(OrmAction.Delete, connection, items, cancellationToken).ConfigureAwait(false)).Item1;
        }

        /// <summary>
        /// Update the row(s) specified by the primary key(s) or WHERE values sent in using the values from the item sent in.
        /// If `primaryKeyFields` has been specified on the current Mighty instance then any primary key fields in the item are ignored.
        /// The item is not filtered to remove fields not in the table, if you need that you can call <see cref="NewFrom"/> with first parameter `partialItem` and second parameter `false` first.
        /// </summary>
        /// <param name="partialItem">Item containing values to update with</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        override public async Task<int> UpdateUsingAsync(object partialItem, object whereParams)
        {
            return await UpdateUsingAsync(partialItem, whereParams, null, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Update the row(s) specified by the primary key(s) or WHERE values sent in using the values from the item sent in.
        /// If `primaryKeyFields` has been specified on the current Mighty instance then any primary key fields in the item are ignored.
        /// The item is not filtered to remove fields not in the table, if you need that you can call <see cref="NewFrom"/> with first parameter `partialItem` and second parameter `false` first.
        /// </summary>
        /// <param name="partialItem">Item containing values to update with</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        override public async Task<int> UpdateUsingAsync(object partialItem, object whereParams, CancellationToken cancellationToken)
        {
            return await UpdateUsingAsync(partialItem, whereParams, null, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Update the row(s) specified by the primary key(s) or WHERE values sent in using the values from the item sent in.
        /// If `primaryKeyFields` has been specified on the current Mighty instance then any primary key fields in the item are ignored.
        /// The item is not filtered to remove fields not in the table, if you need that you can call <see cref="NewFrom"/> with first parameter `partialItem` and second parameter `false` first.
        /// </summary>
        /// <param name="partialItem">Item containing values to update with</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
        override public async Task<int> UpdateUsingAsync(object partialItem, object whereParams,
            DbConnection connection)
        {
            return await UpdateUsingAsync(partialItem, whereParams, connection, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Update the row(s) specified by the primary key(s) or WHERE values sent in using the values from the item sent in.
        /// If `primaryKeyFields` has been specified on the current Mighty instance then any primary key fields in the item are ignored.
        /// The item is not filtered to remove fields not in the table, if you need that you can call <see cref="NewFrom"/> with first parameter `partialItem` and second parameter `false` first.
        /// </summary>
        /// <param name="partialItem">Item containing values to update with</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        override public async Task<int> UpdateUsingAsync(object partialItem, object whereParams,
            DbConnection connection, CancellationToken cancellationToken)
        {
            Tuple<string, object, object[]> retval = GetWhereSpecFromWhereParams(whereParams);
            return await UpdateUsingWithParamsAsync(
                partialItem,
                where: retval.Item1, inParams: retval.Item2, args: retval.Item3,
                connection: connection, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Update all items matching WHERE clause using fields from the item sent in.
        /// If `primaryKeyFields` has been specified on the current Mighty instance then any primary key fields in the item are ignored.
        /// The item is not filtered to remove fields not in the table, if you need that you can call <see cref="NewFrom"/> with first parameter `partialItem` and second parameter `false` first.
        /// </summary>
        /// <param name="partialItem">Item containing values to update with</param>
        /// <param name="where">WHERE clause specifying which rows to update</param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        override public async Task<int> UpdateUsingAsync(object partialItem, string where,
            params object[] args)
        {
            return await UpdateUsingWithParamsAsync(
                partialItem,
                where,
                null,
                cancellationToken: CancellationToken.None,
                args).ConfigureAwait(false);
        }

        /// <summary>
        /// Update all items matching WHERE clause using fields from the item sent in.
        /// If `primaryKeyFields` has been specified on the current Mighty instance then any primary key fields in the item are ignored.
        /// The item is not filtered to remove fields not in the table, if you need that you can call <see cref="NewFrom"/> with first parameter `partialItem` and second parameter `false` first.
        /// </summary>
        /// <param name="partialItem">Item containing values to update with</param>
        /// <param name="where">WHERE clause specifying which rows to update</param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        override public async Task<int> UpdateUsingAsync(object partialItem, string where,
            CancellationToken cancellationToken,
            params object[] args)
        {
            return await UpdateUsingWithParamsAsync(
                partialItem,
                where,
                null,
                cancellationToken,
                args).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete one or more items based on a WHERE clause.
        /// </summary>
        /// <param name="where">
        /// Non-optional WHERE clause.
        /// Specify "1=1" if you are sure that you want to delete all rows.
        /// </param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <returns>The number of items affected</returns>
        override public async Task<int> DeleteAsync(string where,
            params object[] args)
        {
            return await DeleteAsync(where, null, args).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete one or more items based on a WHERE clause.
        /// </summary>
        /// <param name="where">
        /// Non-optional WHERE clause.
        /// Specify "1=1" if you are sure that you want to delete all rows.
        /// </param>
        /// <param name="args">Auto-numbered parameter values for WHERE clause</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns>The number of items affected</returns>
        override public async Task<int> DeleteAsync(string where,
            CancellationToken cancellationToken,
            params object[] args)
        {
            return await DeleteAsync(where, null, cancellationToken, args).ConfigureAwait(false);
        }
        #endregion
    }
}
#endif