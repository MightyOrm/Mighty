#if !NET40
using System.Collections.Generic;
using Dasync.Collections;
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
        /// <summary>
        /// Get <see cref="IEnumerable{T}"/> of items returned by database command.
        /// </summary>
        /// <param name="command">The command to execute</param>
        /// <param name="connection">The connection to use</param>
        /// <returns></returns>
        override public async Task<IAsyncEnumerable<T>> QueryAsync(
            DbCommand command,
            DbConnection connection = null)
        {
            return await QueryNWithParamsAsync<T>(command: command, connection: connection);
        }

        /// <summary>
        /// Get <see cref="IEnumerable{T}"/> of items returned by database command.
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="command">The command to execute</param>
        /// <param name="connection">The connection to use</param>
        /// <returns></returns>
        override public async Task<IAsyncEnumerable<T>> QueryAsync(
            CancellationToken cancellationToken,
            DbCommand command,
            DbConnection connection = null)
        {
            return await QueryNWithParamsAsync<T>(command: command, cancellationToken: cancellationToken, connection: connection);
        }

        /// <summary>
        /// Get <see cref="IEnumerable{T}"/> of items returned by SQL query.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="args">Auto-numbered parameter values for SQL</param>
        /// <returns></returns>
        /// <remarks>
        /// 'Easy-calling' version, optional args straight after SQL.
        /// </remarks>
        override public async Task<IAsyncEnumerable<T>> QueryAsync(string sql,
            params object[] args)
        {
            return await QueryNWithParamsAsync<T>(sql, args: args);
        }

        /// <summary>
        /// Get <see cref="IEnumerable{T}"/> of items returned by SQL query.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="args">Auto-numbered parameter values for SQL</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        /// <remarks>
        /// 'Easy-calling' version, optional args straight after SQL.
        /// </remarks>
        override public async Task<IAsyncEnumerable<T>> QueryAsync(
            CancellationToken cancellationToken,
            string sql,
            params object[] args)
        {
            return await QueryNWithParamsAsync<T>(sql, cancellationToken: cancellationToken, args: args);
        }

        /// <summary>
        /// Get single item from query.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        /// <remarks>
        /// 'Easy-calling' version, optional args straight after SQL.
        /// </remarks>
        override public async Task<T> SingleFromQueryAsync(string sql,
            params object[] args)
        {
            return await
                (await QueryNWithParamsAsync<T>(sql, args: args).ConfigureAwait(false))
                .FirstOrDefaultAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Get single item from query.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        /// <remarks>
        /// 'Easy-calling' version, optional args straight after SQL.
        /// </remarks>
        override public async Task<T> SingleFromQueryAsync(
            CancellationToken cancellationToken,
            string sql,
            params object[] args)
        {
            return await
                (await QueryNWithParamsAsync<T>(sql, cancellationToken: cancellationToken, args: args).ConfigureAwait(false))
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Get <see cref="IEnumerable{T}"/> of items returned by SQL query.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="connection">The connection to use</param>
        /// <param name="args">Auto-numbered parameter values for SQL</param>
        /// <returns></returns>
        override public async Task<IAsyncEnumerable<T>> QueryAsync(string sql,
            DbConnection connection,
            params object[] args)
        {
            return await QueryNWithParamsAsync<T>(sql, connection: connection, args: args);
        }

        /// <summary>
        /// Get <see cref="IEnumerable{T}"/> of items returned by SQL query.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="connection">The connection to use</param>
        /// <param name="args">Auto-numbered parameter values for SQL</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        override public async Task<IAsyncEnumerable<T>> QueryAsync(
            CancellationToken cancellationToken,
            string sql,
            DbConnection connection,
            params object[] args)
        {
            return await QueryNWithParamsAsync<T>(sql, connection: connection, cancellationToken: cancellationToken, args: args);
        }

        /// <summary>
        /// Get single item from query.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="connection">The connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        override public async Task<T> SingleFromQueryAsync(string sql,
            DbConnection connection,
            params object[] args)
        {
            return await
                (await QueryNWithParamsAsync<T>(sql, connection: connection, args: args).ConfigureAwait(false))
                .FirstOrDefaultAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Get single item from query.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="connection">The connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        override public async Task<T> SingleFromQueryAsync(
            CancellationToken cancellationToken,
            string sql,
            DbConnection connection,
            params object[] args)
        {
            return await
                (await QueryNWithParamsAsync<T>(
                    sql,
                    connection: connection,
                    cancellationToken: cancellationToken,
                    args: args).ConfigureAwait(false))
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Get <see cref="IEnumerable{T}"/> of items returned by SQL query with support for named parameters.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="inParams">Named input parameters</param>
        /// <param name="outParams">Named output parameters</param>
        /// <param name="ioParams">Named input-output parameters</param>
        /// <param name="returnParams">Named return parameters</param>
        /// <param name="connection">The connection to use</param>
        /// <param name="args">Auto-numbered parameter values for SQL</param>
        /// <returns></returns>
        override public async Task<IAsyncEnumerable<T>> QueryWithParamsAsync(string sql,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await QueryNWithParamsAsync<T>(sql,
                inParams, outParams, ioParams, returnParams,
                connection: connection, args: args);
        }

        /// <summary>
        /// Get <see cref="IEnumerable{T}"/> of items returned by SQL query with support for named parameters.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="inParams">Named input parameters</param>
        /// <param name="outParams">Named output parameters</param>
        /// <param name="ioParams">Named input-output parameters</param>
        /// <param name="returnParams">Named return parameters</param>
        /// <param name="connection">The connection to use</param>
        /// <param name="args">Auto-numbered parameter values for SQL</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        override public async Task<IAsyncEnumerable<T>> QueryWithParamsAsync(
            CancellationToken cancellationToken,
            string sql,
            object inParams = null,
            object outParams = null,
            object ioParams = null,
            object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await QueryNWithParamsAsync<T>(
                sql,
                inParams, outParams, ioParams, returnParams,
                connection: connection,
                cancellationToken: cancellationToken,
                args: args);
        }

        /// <summary>
        /// Get single item from query with support for named parameters.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="inParams">Named input parameters</param>
        /// <param name="outParams">Named output parameters</param>
        /// <param name="ioParams">Named input-output parameters</param>
        /// <param name="returnParams">Named return parameters</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
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

        /// <summary>
        /// Get single item from query with support for named parameters.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="inParams">Named input parameters</param>
        /// <param name="outParams">Named output parameters</param>
        /// <param name="ioParams">Named input-output parameters</param>
        /// <param name="returnParams">Named return parameters</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        override public async Task<T> SingleFromQueryWithParamsAsync(
            CancellationToken cancellationToken,
            string sql,
            object inParams = null,
            object outParams = null,
            object ioParams = null,
            object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await
                (await QueryNWithParamsAsync<T>(
                    sql,
                    inParams, outParams, ioParams, returnParams,
                    connection: connection,
                    cancellationToken: cancellationToken,
                    args: args).ConfigureAwait(false))
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Get <see cref="IEnumerable{T}"/> of items from stored procedure call with support for named parameters.
        /// </summary>
        /// <param name="spName">Stored procedure name</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="inParams">Named input parameters</param>
        /// <param name="outParams">Named output parameters</param>
        /// <param name="ioParams">Named input-output parameters</param>
        /// <param name="returnParams">Named return parameters</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
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

        /// <summary>
        /// Get <see cref="IEnumerable{T}"/> of items from stored procedure call with support for named parameters.
        /// </summary>
        /// <param name="spName">Stored procedure name</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="inParams">Named input parameters</param>
        /// <param name="outParams">Named output parameters</param>
        /// <param name="ioParams">Named input-output parameters</param>
        /// <param name="returnParams">Named return parameters</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        override public async Task<IAsyncEnumerable<T>> QueryFromProcedureAsync(
            CancellationToken cancellationToken,
            string spName,
            object inParams = null,
            object outParams = null,
            object ioParams = null,
            object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await QueryNWithParamsAsync<T>(
                spName,
                inParams, outParams, ioParams, returnParams,
                isProcedure: true,
                connection: connection, cancellationToken: cancellationToken,
                args: args);
        }

        /// <summary>
        /// Get single item from stored procedure call with support for named parameters.
        /// </summary>
        /// <param name="spName">Stored procedure name</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="inParams">Named input parameters</param>
        /// <param name="outParams">Named output parameters</param>
        /// <param name="ioParams">Named input-output parameters</param>
        /// <param name="returnParams">Named return parameters</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
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

        /// <summary>
        /// Get single item from stored procedure call with support for named parameters.
        /// </summary>
        /// <param name="spName">Stored procedure name</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="inParams">Named input parameters</param>
        /// <param name="outParams">Named output parameters</param>
        /// <param name="ioParams">Named input-output parameters</param>
        /// <param name="returnParams">Named return parameters</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        override public async Task<T> SingleFromProcedureAsync(
            CancellationToken cancellationToken,
            string spName,
            object inParams = null,
            object outParams = null,
            object ioParams = null,
            object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await
                (await QueryNWithParamsAsync<T>(
                    spName,
                    inParams, outParams, ioParams, returnParams,
                    isProcedure: true,
                    connection: connection,
                    cancellationToken: cancellationToken,
                    args: args).ConfigureAwait(false))
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Get <see cref="IEnumerable{R}"/> of result sets, each of which is itself an <see cref="IEnumerable{T}"/> of items, returned by database command.
        /// </summary>
        /// <param name="command">The command to execute</param>
        /// <param name="connection">The connection to use</param>
        /// <returns></returns>
        override public async Task<IAsyncEnumerable<IAsyncEnumerable<T>>> QueryMultipleAsync(DbCommand command,
            DbConnection connection = null)
        {
            return await QueryNWithParamsAsync<IAsyncEnumerable<T>>(command: command, connection: connection);
        }

        /// <summary>
        /// Get <see cref="IEnumerable{R}"/> of result sets, each of which is itself an <see cref="IEnumerable{T}"/> of items, returned by database command.
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="command">The command to execute</param>
        /// <param name="connection">The connection to use</param>
        /// <returns></returns>
        override public async Task<IAsyncEnumerable<IAsyncEnumerable<T>>> QueryMultipleAsync(
            CancellationToken cancellationToken,
            DbCommand command,
            DbConnection connection = null)
        {
            return await QueryNWithParamsAsync<IAsyncEnumerable<T>>(command: command, cancellationToken: cancellationToken, connection: connection);
        }

        /// <summary>
        /// Get <see cref="IEnumerable{R}"/> of result sets, each of which is itself an <see cref="IEnumerable{T}"/> of items, returned by SQL query.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="args">Auto-numbered parameter values for SQL</param>
        /// <returns></returns>
        /// <remarks>
        /// 'Easy-calling' version, optional args straight after SQL.
        /// </remarks>
        override public async Task<IAsyncEnumerable<IAsyncEnumerable<T>>> QueryMultipleAsync(string sql,
            params object[] args)
        {
            return await QueryNWithParamsAsync<IAsyncEnumerable<T>>(sql, args: args);
        }

        /// <summary>
        /// Get <see cref="IEnumerable{R}"/> of result sets, each of which is itself an <see cref="IEnumerable{T}"/> of items, returned by SQL query.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="args">Auto-numbered parameter values for SQL</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        /// <remarks>
        /// 'Easy-calling' version, optional args straight after SQL.
        /// </remarks>
        override public async Task<IAsyncEnumerable<IAsyncEnumerable<T>>> QueryMultipleAsync(
            CancellationToken cancellationToken,
            string sql,
            params object[] args)
        {
            return await QueryNWithParamsAsync<IAsyncEnumerable<T>>(sql, cancellationToken: cancellationToken, args: args);
        }

        /// <summary>
        /// Get <see cref="IEnumerable{R}"/> of result sets, each of which is itself an <see cref="IEnumerable{T}"/> of items, returned by SQL query.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="connection">The connection to use</param>
        /// <param name="args">Auto-numbered parameter values for SQL</param>
        /// <returns></returns>
        override public async Task<IAsyncEnumerable<IAsyncEnumerable<T>>> QueryMultipleAsync(string sql,
            DbConnection connection,
            params object[] args)
        {
            return await QueryNWithParamsAsync<IAsyncEnumerable<T>>(sql, connection: connection, args: args);
        }

        /// <summary>
        /// Get <see cref="IEnumerable{R}"/> of result sets, each of which is itself an <see cref="IEnumerable{T}"/> of items, returned by SQL query.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="connection">The connection to use</param>
        /// <param name="args">Auto-numbered parameter values for SQL</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        override public async Task<IAsyncEnumerable<IAsyncEnumerable<T>>> QueryMultipleAsync(
            CancellationToken cancellationToken,
            string sql,
            DbConnection connection,
            params object[] args)
        {
            return await QueryNWithParamsAsync<IAsyncEnumerable<T>>(sql, connection: connection, cancellationToken: cancellationToken, args: args);
        }

        /// <summary>
        /// Get <see cref="IEnumerable{R}"/> of result sets, each of which is itself an <see cref="IEnumerable{T}"/> of items, returned by SQL query with support for named parameters.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="inParams">Named input parameters</param>
        /// <param name="outParams">Named output parameters</param>
        /// <param name="ioParams">Named input-output parameters</param>
        /// <param name="returnParams">Named return parameters</param>
        /// <param name="connection">The connection to use</param>
        /// <param name="args">Auto-numbered parameter values for SQL</param>
        /// <returns></returns>
        override public async Task<IAsyncEnumerable<IAsyncEnumerable<T>>> QueryMultipleWithParamsAsync(string sql,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await QueryNWithParamsAsync<IAsyncEnumerable<T>>(sql,
                inParams, outParams, ioParams, returnParams,
                connection: connection, args: args);
        }

        /// <summary>
        /// Get <see cref="IEnumerable{R}"/> of result sets, each of which is itself an <see cref="IEnumerable{T}"/> of items, returned by SQL query with support for named parameters.
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="sql">The command SQL</param>
        /// <param name="inParams">Named input parameters</param>
        /// <param name="outParams">Named output parameters</param>
        /// <param name="ioParams">Named input-output parameters</param>
        /// <param name="returnParams">Named return parameters</param>
        /// <param name="connection">The connection to use</param>
        /// <param name="args">Auto-numbered parameter values for SQL</param>
        /// <returns></returns>
        override public async Task<IAsyncEnumerable<IAsyncEnumerable<T>>> QueryMultipleWithParamsAsync(
            CancellationToken cancellationToken,
            string sql,
            object inParams = null,
            object outParams = null,
            object ioParams = null,
            object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await QueryNWithParamsAsync<IAsyncEnumerable<T>>(
                sql,
                inParams, outParams, ioParams, returnParams,
                connection: connection,
                cancellationToken: cancellationToken,
                args: args);
        }

        /// <summary>
        /// Get <see cref="IEnumerable{R}"/> of result sets, each of which is itself an <see cref="IEnumerable{T}"/> of items, from stored procedure call with support for named parameters.
        /// </summary>
        /// <param name="spName">Stored procedure name</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="inParams">Named input parameters</param>
        /// <param name="outParams">Named output parameters</param>
        /// <param name="ioParams">Named input-output parameters</param>
        /// <param name="returnParams">Named return parameters</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
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

        /// <summary>
        /// Get <see cref="IEnumerable{R}"/> of result sets, each of which is itself an <see cref="IEnumerable{T}"/> of items, from stored procedure call with support for named parameters.
        /// </summary>
        /// <param name="spName">Stored procedure name</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="inParams">Named input parameters</param>
        /// <param name="outParams">Named output parameters</param>
        /// <param name="ioParams">Named input-output parameters</param>
        /// <param name="returnParams">Named return parameters</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        override public async Task<IAsyncEnumerable<IAsyncEnumerable<T>>> QueryMultipleFromProcedureAsync(
            CancellationToken cancellationToken,
            string spName,
            object inParams = null,
            object outParams = null,
            object ioParams = null,
            object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await QueryNWithParamsAsync<IAsyncEnumerable<T>>(
                spName,
                inParams, outParams, ioParams, returnParams,
                isProcedure: true,
                connection: connection,
                cancellationToken: cancellationToken,
                args: args);
        }

        /// <summary>
        /// Execute SQL query.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="args">Auto-numbered parameters for the SQL</param>
        /// <returns>The number of rows affected</returns>
        /// <remarks>
        /// 'Easy-calling' version, optional args straight after SQL.
        /// </remarks>
        override public async Task<int> ExecuteAsync(string sql,
            params object[] args)
        {
            using (var command = CreateCommandWithParams(sql, args: args))
            {
                return await ExecuteAsync(command).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Execute SQL query.
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="sql">The command SQL</param>
        /// <param name="args">Auto-numbered parameters for the SQL</param>
        /// <returns>The number of rows affected</returns>
        /// <remarks>
        /// 'Easy-calling' version, optional args straight after SQL.
        /// </remarks>
        override public async Task<int> ExecuteAsync(
            CancellationToken cancellationToken,
            string sql,
            params object[] args)
        {
            using (var command = CreateCommandWithParams(sql, args: args))
            {
                return await ExecuteAsync(cancellationToken, command).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Execute SQL query.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered parameters for the SQL</param>
        /// <returns>The number of rows affected</returns>
        override public async Task<int> ExecuteAsync(string sql,
            DbConnection connection,
            params object[] args)
        {
            using (var command = CreateCommandWithParams(sql, connection: connection, args: args))
            {
                return await ExecuteAsync(command, connection).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Execute SQL query.
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="sql">The command SQL</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered parameters for the SQL</param>
        /// <returns>The number of rows affected</returns>
        override public async Task<int> ExecuteAsync(
            CancellationToken cancellationToken,
            string sql,
            DbConnection connection,
            params object[] args)
        {
            using (var command = CreateCommandWithParams(sql, connection: connection, args: args))
            {
                return await ExecuteAsync(cancellationToken, command, connection).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Execute SQL query with support for named parameters.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="inParams">Named input parameters</param>
        /// <param name="outParams">Named output parameters</param>
        /// <param name="ioParams">Named input-output parameters</param>
        /// <param name="returnParams">Named return parameters</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns>A dynamic object containing the names and output values of all output, input-output and return parameters</returns>
        override public async Task<dynamic> ExecuteWithParamsAsync(string sql,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await ExecuteWithParamsAsync(
                CancellationToken.None,
                sql,
                inParams, outParams, ioParams, returnParams,
                connection,
                args).ConfigureAwait(false);
        }

        /// <summary>
        /// Execute SQL query with support for named parameters.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="inParams">Named input parameters</param>
        /// <param name="outParams">Named output parameters</param>
        /// <param name="ioParams">Named input-output parameters</param>
        /// <param name="returnParams">Named return parameters</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns>A dynamic object containing the names and output values of all output, input-output and return parameters</returns>
        override public async Task<dynamic> ExecuteWithParamsAsync(
            CancellationToken cancellationToken,
            string sql,
            object inParams = null,
            object outParams = null,
            object ioParams = null,
            object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            var retval = CreateCommandWithParamsAndRowCountCheck(sql,
                inParams, outParams, ioParams, returnParams,
                connection: connection,
                args: args);
            using (retval.Item1)
            {
                var rowCount = await ExecuteAsync(cancellationToken, retval.Item1, connection).ConfigureAwait(false);
                var results = ResultsAsExpando(retval.Item1);
                if (retval.Item2)
                {
                    AppendRowCountResults(rowCount, outParams, results);
                }
                return results;
            }
        }

        /// <summary>
        /// Execute stored procedure with support for named parameters.
        /// </summary>
        /// <param name="spName">Stored procedure name</param>
        /// <param name="inParams">Named input parameters</param>
        /// <param name="outParams">Named output parameters</param>
        /// <param name="ioParams">Named input-output parameters</param>
        /// <param name="returnParams">Named return parameters</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns>A dynamic object containing the names and output values of all output, input-output and return parameters</returns>
        override public async Task<dynamic> ExecuteProcedureAsync(string spName,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await ExecuteProcedureAsync(
                CancellationToken.None,
                spName,
                inParams, outParams, ioParams, returnParams,
                connection,
                args);
        }

        /// <summary>
        /// Execute stored procedure with support for named parameters.
        /// </summary>
        /// <param name="spName">Stored procedure name</param>
        /// <param name="inParams">Named input parameters</param>
        /// <param name="outParams">Named output parameters</param>
        /// <param name="ioParams">Named input-output parameters</param>
        /// <param name="returnParams">Named return parameters</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns>A dynamic object containing the names and output values of all output, input-output and return parameters</returns>
        override public async Task<dynamic> ExecuteProcedureAsync(
            CancellationToken cancellationToken,
            string spName,
            object inParams = null,
            object outParams = null,
            object ioParams = null,
            object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            var retval = CreateCommandWithParamsAndRowCountCheck(spName,
                inParams, outParams, ioParams, returnParams,
                isProcedure: true,
                connection: connection,
                args: args);
            using (retval.Item1)
            {
                var rowCount = await ExecuteAsync(cancellationToken, retval.Item1, connection).ConfigureAwait(false);
                var results = ResultsAsExpando(retval.Item1);
                if (retval.Item2)
                {
                    AppendRowCountResults(rowCount, outParams, results);
                }
                return results;
            }
        }

        /// <summary>
        /// Return scalar result (value of first or only column from first or only row) from SQL query.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        /// <remarks>
        /// 'Easy-calling' version, optional args straight after SQL.
        /// </remarks>
        override public async Task<object> ScalarAsync(string sql,
            params object[] args)
        {
            using (var command = CreateCommand(sql, args))
            {
                return await ScalarAsync(command).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Return scalar result (value of first or only column from first or only row) from SQL query.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        /// <remarks>
        /// 'Easy-calling' version, optional args straight after SQL.
        /// </remarks>
        override public async Task<object> ScalarAsync(
            CancellationToken cancellationToken,
            string sql,
            params object[] args)
        {
            using (var command = CreateCommand(sql, args))
            {
                return await ScalarAsync(cancellationToken, command).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Return scalar result (value of first or only column from first or only row) from SQL query.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        override public async Task<object> ScalarAsync(string sql,
            DbConnection connection,
            params object[] args)
        {
            using (var command = CreateCommand(sql, args))
            {
                return await ScalarAsync(command, connection).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Return scalar result (value of first or only column from first or only row) from SQL query.
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="sql">The command SQL</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        override public async Task<object> ScalarAsync(
            CancellationToken cancellationToken,
            string sql,
            DbConnection connection,
            params object[] args)
        {
            using (var command = CreateCommand(sql, args))
            {
                return await ScalarAsync(cancellationToken, command, connection).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Return scalar result (value of first or only column from first or only row) from SQL query with support for named parameters.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="inParams">Named input parameters</param>
        /// <param name="outParams">Named output parameters</param>
        /// <param name="ioParams">Named input-output parameters</param>
        /// <param name="returnParams">Named return parameters</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        override public async Task<object> ScalarWithParamsAsync(string sql,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            using (var command = CreateCommandWithParams(sql,
                inParams, outParams, ioParams, returnParams,
                connection: connection,
                args: args))
            {
                return await ScalarAsync(command, connection).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Return scalar result (value of first or only column from first or only row) from SQL query with support for named parameters.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="inParams">Named input parameters</param>
        /// <param name="outParams">Named output parameters</param>
        /// <param name="ioParams">Named input-output parameters</param>
        /// <param name="returnParams">Named return parameters</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        override public async Task<object> ScalarWithParamsAsync(
            CancellationToken cancellationToken,
            string sql,
            object inParams = null,
            object outParams = null,
            object ioParams = null,
            object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            using (var command = CreateCommandWithParams(sql,
                inParams, outParams, ioParams, returnParams,
                connection: connection,
                args: args))
            {
                return await ScalarAsync(cancellationToken, command, connection).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Return scalar result (value of first or only column from first or only row) from stored procedure with support for named parameters.
        /// </summary>
        /// <param name="spName">Stored procedure name</param>
        /// <param name="inParams">Named input parameters</param>
        /// <param name="outParams">Named output parameters</param>
        /// <param name="ioParams">Named input-output parameters</param>
        /// <param name="returnParams">Named return parameters</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        override public async Task<object> ScalarFromProcedureAsync(string spName,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            using (var command = CreateCommandWithParams(spName,
                inParams, outParams, ioParams, returnParams,
                isProcedure: true,
                connection: connection,
                args: args))
            {
                return await ScalarAsync(command, connection).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Return scalar result (value of first or only column from first or only row) from stored procedure with support for named parameters.
        /// </summary>
        /// <param name="spName">Stored procedure name</param>
        /// <param name="inParams">Named input parameters</param>
        /// <param name="outParams">Named output parameters</param>
        /// <param name="ioParams">Named input-output parameters</param>
        /// <param name="returnParams">Named return parameters</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        override public async Task<object> ScalarFromProcedureAsync(
            CancellationToken cancellationToken,
            string spName,
            object inParams = null,
            object outParams = null,
            object ioParams = null,
            object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            using (var command = CreateCommandWithParams(spName,
                inParams, outParams, ioParams, returnParams,
                isProcedure: true,
                connection: connection,
                args: args))
            {
                return await ScalarAsync(cancellationToken, command, connection).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Yield return values for single or multiple resultsets.
        /// </summary>
        /// <typeparam name="X">Use with <typeparamref name="T"/> for single or <see cref="IEnumerable{T}"/> for multiple</typeparam>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="sql">The command SQL</param>
        /// <param name="inParams">Named input parameters</param>
        /// <param name="outParams">Named output parameters</param>
        /// <param name="ioParams">Named input-output parameters</param>
        /// <param name="returnParams">Named return parameters</param>
        /// <param name="isProcedure">Is the SQL a stored procedure name (with optional argument spec) only?</param>
        /// <param name="behavior">The command behaviour</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered parameters for the SQL</param>
        /// <returns></returns>
        override protected async Task<IAsyncEnumerable<X>> QueryNWithParamsAsync<X>(
            string sql,
            object inParams = null,
            object outParams = null,
            object ioParams = null,
            object returnParams = null,
            bool isProcedure = false,
            CommandBehavior behavior = CommandBehavior.Default,
            DbConnection connection = null,
            CancellationToken cancellationToken = default,
            params object[] args)
        {
            var command = CreateCommandWithParams(sql, inParams, outParams, ioParams, returnParams, isProcedure, connection, args);
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
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        override public async Task<object> CountAsync(
            string where = null,
            string columns = "*",
            DbConnection connection = null,
            params object[] args)
        {
            return await AggregateWithParamsAsync(CancellationToken.None, "COUNT", columns, where, connection, args: args).ConfigureAwait(false);
        }

        /// <summary>
        /// Perform COUNT on current table.
        /// </summary>
        /// <param name="where">WHERE clause</param>
        /// <param name="columns">Columns (defaults to *, but can be specified, e.g., to count non-nulls in a given field)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        override public async Task<object> CountAsync(
            CancellationToken cancellationToken,
            string where = null,
            string columns = "*",
            DbConnection connection = null,
            params object[] args)
        {
            return await AggregateWithParamsAsync(cancellationToken, "COUNT", columns, where, connection, args: args).ConfigureAwait(false);
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
            return await AggregateAsync(CancellationToken.None, "COUNT", columns, whereParams, connection).ConfigureAwait(false);
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
            return await AggregateAsync(cancellationToken, "COUNT", columns, whereParams, connection).ConfigureAwait(false);
        }

        /// <summary>
        /// Get MAX of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        override public async Task<object> MaxAsync(
            string columns,
            string where = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await AggregateWithParamsAsync(CancellationToken.None, "MAX", columns, where, connection, args: args).ConfigureAwait(false);
        }

        /// <summary>
        /// Get MAX of column on current table.
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="columns">Columns</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        override public async Task<object> MaxAsync(
            CancellationToken cancellationToken,
            string columns,
            string where = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await AggregateWithParamsAsync(cancellationToken, "MAX", columns, where, connection, args: args).ConfigureAwait(false);
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
            return await AggregateAsync(CancellationToken.None, "MAX", columns, whereParams, connection).ConfigureAwait(false);
        }

        /// <summary>
        /// Get MAX of column on current table.
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="columns">Columns</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        override public async Task<object> MaxAsync(
            CancellationToken cancellationToken,
            string columns,
            object whereParams = null,
            DbConnection connection = null)
        {
            return await AggregateAsync(cancellationToken, "MAX", columns, whereParams, connection).ConfigureAwait(false);
        }

        /// <summary>
        /// Get MIN of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        override public async Task<object> MinAsync(
            string columns,
            string where = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await AggregateWithParamsAsync(CancellationToken.None, "MIN", columns, where, connection, args: args).ConfigureAwait(false);
        }

        /// <summary>
        /// Get MIN of column on current table.
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="columns">Columns</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        override public async Task<object> MinAsync(
            CancellationToken cancellationToken,
            string columns,
            string where = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await AggregateWithParamsAsync(cancellationToken, "MIN", columns, where, connection, args: args).ConfigureAwait(false);
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
            return await AggregateAsync(CancellationToken.None, "MIN", columns, whereParams, connection).ConfigureAwait(false);
        }

        /// <summary>
        /// Get MIN of column on current table.
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="columns">Columns</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        override public async Task<object> MinAsync(
            CancellationToken cancellationToken,
            string columns,
            object whereParams = null,
            DbConnection connection = null)
        {
            return await AggregateAsync(cancellationToken, "MIN", columns, whereParams, connection).ConfigureAwait(false);
        }

        /// <summary>
        /// Get SUM of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        override public async Task<object> SumAsync(
            string columns,
            string where = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await AggregateWithParamsAsync(CancellationToken.None, "SUM", columns, where, connection, args: args).ConfigureAwait(false);
        }

        /// <summary>
        /// Get SUM of column on current table.
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="columns">Columns</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        override public async Task<object> SumAsync(
            CancellationToken cancellationToken,
            string columns,
            string where = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await AggregateWithParamsAsync(cancellationToken, "SUM", columns, where, connection, args: args).ConfigureAwait(false);
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
            return await AggregateAsync(CancellationToken.None, "SUM", columns, whereParams, connection).ConfigureAwait(false);
        }

        /// <summary>
        /// Get SUM of column on current table.
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="columns">Columns</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        override public async Task<object> SumAsync(
            CancellationToken cancellationToken,
            string columns,
            object whereParams = null,
            DbConnection connection = null)
        {
            return await AggregateAsync(cancellationToken, "SUM", columns, whereParams, connection).ConfigureAwait(false);
        }

        /// <summary>
        /// Get AVG of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        override public async Task<object> AvgAsync(
            string columns,
            string where = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await AggregateWithParamsAsync(CancellationToken.None, "AVG", columns, where, connection, args: args).ConfigureAwait(false);
        }

        /// <summary>
        /// Get AVG of column on current table.
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="columns">Columns</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        override public async Task<object> AvgAsync(
            CancellationToken cancellationToken,
            string columns,
            string where = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await AggregateWithParamsAsync(cancellationToken, "AVG", columns, where, connection, args: args).ConfigureAwait(false);
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
            return await AggregateAsync(CancellationToken.None, "AVG", columns, whereParams, connection).ConfigureAwait(false);
        }

        /// <summary>
        /// Get AVG of column on current table.
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="columns">Columns</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        override public async Task<object> AvgAsync(
            CancellationToken cancellationToken,
            string columns,
            object whereParams = null,
            DbConnection connection = null)
        {
            return await AggregateAsync(cancellationToken, "AVG", columns, whereParams, connection).ConfigureAwait(false);
        }

        /// <summary>
        /// Perform aggregate operation on the current table (use for SUM, MAX, MIN, AVG, etc.)
        /// </summary>
        /// <param name="function">Aggregate function</param>
        /// <param name="columns">Columns for aggregate function</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        override public async Task<object> AggregateAsync(
            string function,
            string columns,
            string where = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await AggregateWithParamsAsync(CancellationToken.None, function, columns, where, connection: connection, args: args).ConfigureAwait(false);
        }

        /// <summary>
        /// Perform aggregate operation on the current table (use for SUM, MAX, MIN, AVG, etc.)
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="function">Aggregate function</param>
        /// <param name="columns">Columns for aggregate function</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        override public async Task<object> AggregateAsync(
            CancellationToken cancellationToken,
            string function,
            string columns,
            string where = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await AggregateWithParamsAsync(cancellationToken, function, columns, where, connection: connection, args: args).ConfigureAwait(false);
        }

        /// <summary>
        /// Perform aggregate operation on the current table (use for SUM, MAX, MIN, AVG, etc.)
        /// </summary>
        /// <param name="function">Aggregate function</param>
        /// <param name="columns">Columns for aggregate function</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        override public async Task<object> AggregateAsync(
            string function,
            string columns,
            object whereParams = null,
            DbConnection connection = null)
        {
            return await AggregateAsync(CancellationToken.None, function, columns, whereParams, connection: connection).ConfigureAwait(false);
        }

        /// <summary>
        /// Perform aggregate operation on the current table (use for SUM, MAX, MIN, AVG, etc.)
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="function">Aggregate function</param>
        /// <param name="columns">Columns for aggregate function</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        override public async Task<object> AggregateAsync(
            CancellationToken cancellationToken,
            string function,
            string columns,
            object whereParams = null,
            DbConnection connection = null)
        {
            Tuple<string, object, object[]> retval = GetWhereSpecFromWhereParams(whereParams);
            return await AggregateWithParamsAsync(
                cancellationToken,
                function, columns,
                where: retval.Item1, inParams: retval.Item2, args: retval.Item3,
                connection: connection);
        }

        /// <summary>
        /// Get single item returned by database command.
        /// </summary>
        /// <param name="command">The command to execute</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        override public async Task<T> SingleAsync(
            DbCommand command,
            DbConnection connection = null)
        {
            return await
                (await QueryNWithParamsAsync<T>(
                    command: command, connection: connection).ConfigureAwait(false))
                .FirstOrDefaultAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Get single item returned by database command.
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="command">The command to execute</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        override public async Task<T> SingleAsync(
            CancellationToken cancellationToken,
            DbCommand command,
            DbConnection connection = null)
        {
            return await
                (await QueryNWithParamsAsync<T>(
                    command: command,
                    cancellationToken: cancellationToken,
                    connection: connection).ConfigureAwait(false))
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Get single item from the current table using primary key or name-value where specification.
        /// </summary>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="columns">List of columns to return</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        override public async Task<T> SingleAsync(
            object whereParams,
            string columns = null,
            DbConnection connection = null)
        {
            return await SingleAsync(CancellationToken.None, whereParams, columns, connection).ConfigureAwait(false);
        }

        /// <summary>
        /// Get single item from the current table using primary key or name-value where specification.
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="columns">List of columns to return</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        override public async Task<T> SingleAsync(
            CancellationToken cancellationToken,
            object whereParams,
            string columns = null,
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
        /// Get single item from the current table using WHERE specification.
        /// </summary>
        /// <param name="where">WHERE clause</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        /// <remarks>
        /// 'Easy-calling' version, optional args straight after where.
        /// </remarks>
        override public async Task<T> SingleAsync(
            string where,
            params object[] args)
        {
            return await SingleWithParamsAsync(where, args: args).ConfigureAwait(false);
        }

        /// <summary>
        /// Get single item from the current table using WHERE specification.
        /// </summary>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">The connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        override public async Task<T> SingleAsync(
            string where,
            DbConnection connection,
            params object[] args)
        {
            return await SingleWithParamsAsync(where, args: args).ConfigureAwait(false);
        }

        /// <summary>
        /// Get single item from the current table using WHERE specification.
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="where">WHERE clause</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        /// <remarks>
        /// 'Easy-calling' version, optional args straight after where.
        /// </remarks>
        override public async Task<T> SingleAsync(
            CancellationToken cancellationToken,
            string where,
            params object[] args)
        {
            return await SingleWithParamsAsync(cancellationToken, where, args: args).ConfigureAwait(false);
        }

        /// <summary>
        /// Get single item from the current table using WHERE specification.
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">The connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        override public async Task<T> SingleAsync(
            CancellationToken cancellationToken,
            string where,
            DbConnection connection,
            params object[] args)
        {
            return await SingleWithParamsAsync(where, args: args).ConfigureAwait(false);
        }

        /// <summary>
        /// Get single item from the current table using WHERE specification.
        /// </summary>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="orderBy">ORDER BY clause</param>
        /// <param name="columns">Comma separated list of columns to return or "*"</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        /// <remarks>
        /// DbConnection coming early (not just before args) in this one case is really useful, as it avoids ambiguity between
        /// the `columns` and `orderBy` strings and optional string args.
        /// </remarks>
        override public async Task<T> SingleAsync(
            string where,
            DbConnection connection = null,
            string orderBy = null,
            string columns = null,
            params object[] args)
        {
            return await SingleWithParamsAsync(where, orderBy, columns, connection: connection, args: args).ConfigureAwait(false);
        }

        /// <summary>
        /// Get single item from the current table using WHERE specification.
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="orderBy">ORDER BY clause</param>
        /// <param name="columns">Comma separated list of columns to return or "*"</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        /// <remarks>
        /// DbConnection coming early (not just before args) in this one case is really useful, as it avoids ambiguity between
        /// the `columns` and `orderBy` strings and optional string args.
        /// </remarks>
        override public async Task<T> SingleAsync(
            CancellationToken cancellationToken,
            string where,
            DbConnection connection = null,
            string orderBy = null,
            string columns = null,
            params object[] args)
        {
            return await SingleWithParamsAsync(cancellationToken, where, orderBy, columns, connection: connection, args: args).ConfigureAwait(false);
        }

        /// <summary>
        /// Get single item from the current table using WHERE specification with support for named parameters.
        /// </summary>
        /// <param name="where">WHERE clause</param>
        /// <param name="orderBy">ORDER BY clause</param>
        /// <param name="columns">Columns to return</param>
        /// <param name="inParams">Named input parameters</param>
        /// <param name="outParams">Named output parameters</param>
        /// <param name="ioParams">Named input-output parameters</param>
        /// <param name="returnParams">Named return parameters</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        override public async Task<T> SingleWithParamsAsync(
            string where,
            string orderBy = null,
            string columns = null,
            object inParams = null,
            object outParams = null,
            object ioParams = null,
            object returnParams = null,
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

        /// <summary>
        /// Get single item from the current table using WHERE specification with support for named parameters.
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="where">WHERE clause</param>
        /// <param name="orderBy">ORDER BY clause</param>
        /// <param name="columns">Columns to return</param>
        /// <param name="inParams">Named input parameters</param>
        /// <param name="outParams">Named output parameters</param>
        /// <param name="ioParams">Named input-output parameters</param>
        /// <param name="returnParams">Named return parameters</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        override public async Task<T> SingleWithParamsAsync(
            CancellationToken cancellationToken,
            string where,
            string orderBy = null,
            string columns = null,
            object inParams = null,
            object outParams = null,
            object ioParams = null,
            object returnParams = null,
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

        /// <summary>
        /// Get <see cref="IEnumerable{T}"/> of items from the current table with WHERE and TOP/LIMIT specification.
        /// </summary>
        /// <param name="where">WHERE clause</param>
        /// <param name="orderBy">ORDER BY clause</param>
        /// <param name="columns">Columns to return</param>
        /// <param name="limit">Maximum number of items to return</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        override public async Task<IAsyncEnumerable<T>> AllAsync(
            string where = null,
            string orderBy = null,
            string columns = null,
            int limit = 0,
            params object[] args)
        {
            return await AllWithParamsAsync(where, orderBy, columns, limit, args: args).ConfigureAwait(false);
        }

        /// <summary>
        /// Get <see cref="IEnumerable{T}"/> of items from the current table with WHERE and TOP/LIMIT specification.
        /// </summary>
        /// <param name="where">WHERE clause</param>
        /// <param name="orderBy">ORDER BY clause</param>
        /// <param name="columns">Columns to return</param>
        /// <param name="limit">Maximum number of items to return</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        override public async Task<IAsyncEnumerable<T>> AllAsync(
            CancellationToken cancellationToken,
            string where = null,
            string orderBy = null,
            string columns = null,
            int limit = 0,
            params object[] args)
        {
            return await AllWithParamsAsync(cancellationToken, where, orderBy, columns, limit, args: args).ConfigureAwait(false);
        }

        /// <summary>
        /// Get <see cref="IEnumerable{T}"/> of items from the current table with primary key or name-value where specification and TOP/LIMIT specification.
        /// </summary>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="orderBy">ORDER BY clause</param>
        /// <param name="columns">Columns to return</param>
        /// <param name="limit">Maximum number of items to return</param>
        /// <returns></returns>
        override public async Task<IAsyncEnumerable<T>> AllAsync(
            object whereParams = null,
            string orderBy = null,
            string columns = null,
            int limit = 0)
        {
            return await AllAsync(CancellationToken.None, whereParams, orderBy, columns, limit);
        }

        /// <summary>
        /// Get <see cref="IEnumerable{T}"/> of items from the current table with primary key or name-value where specification and TOP/LIMIT specification.
        /// </summary>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="orderBy">ORDER BY clause</param>
        /// <param name="columns">Columns to return</param>
        /// <param name="limit">Maximum number of items to return</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        override public async Task<IAsyncEnumerable<T>> AllAsync(
            CancellationToken cancellationToken,
            object whereParams = null,
            string orderBy = null,
            string columns = null,
            int limit = 0)
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
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns>The result of the paged query. Result properties are Items, TotalPages, and TotalRecords.</returns>
        /// <remarks>
        /// <paramref name="columns"/> parameter is not placed first because it's an override to something we may have alread provided in the constructor
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
            return await PagedWithParamsAsync(
                orderBy,
                columns,
                where,
                pageSize, currentPage,
                connection: connection,
                args: args).ConfigureAwait(false);
        }

        /// <summary>
        /// Table-specific paging; there is also a data wrapper version of paging <see cref="PagedFromSelect"/>.
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="orderBy">You may provide orderBy, if you don't it will try to order by PK and will produce an exception if there is no PK defined.</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="columns">Columns to return</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="currentPage">Current page</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns>The result of the paged query. Result properties are Items, TotalPages, and TotalRecords.</returns>
        /// <remarks>
        /// <paramref name="columns"/> parameter is not placed first because it's an override to something we may have alread provided in the constructor
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
            return await PagedWithParamsAsync(
                cancellationToken,
                orderBy,
                columns,
                where,
                pageSize, currentPage,
                connection: connection,
                args: args).ConfigureAwait(false);
        }

        /// <summary>
        /// Return paged results from arbitrary select statement.
        /// </summary>
        /// <param name="columns">Column spec</param>
        /// <param name="tableNameOrJoinSpec">A table name, or a complete join specification (i.e. anything you can SELECT FROM in SQL)</param>
        /// <param name="orderBy">ORDER BY clause</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="currentPage">Current page</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns>The result of the paged query. Result properties are Items, TotalPages, and TotalRecords.</returns>
        /// <remarks>
        /// In this one instance, because of the connection to the underlying logic of these queries, the user
        /// can pass "SELECT columns" instead of columns.
        /// TO DO: Possibly Possibly cancel the above, it makes no sense from a UI pov!
        /// </remarks>
        override public async Task<PagedResults<T>> PagedFromSelectAsync(
            string tableNameOrJoinSpec,
            string orderBy,
            string columns = null,
            string where = null,
            int pageSize = 20, int currentPage = 1,
            DbConnection connection = null,
            params object[] args)
        {
            return await PagedFromSelectWithParamsAsync(
                CancellationToken.None,
                tableNameOrJoinSpec,
                orderBy,
                columns,
                where,
                pageSize,
                currentPage,
                connection: connection,
                args: args).ConfigureAwait(false);
        }

        /// <summary>
        /// Return paged results from arbitrary select statement.
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="tableNameOrJoinSpec">A table name, or a complete join specification (i.e. anything you can SELECT FROM in SQL)</param>
        /// <param name="orderBy">ORDER BY clause</param>
        /// <param name="columns">Column spec</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="currentPage">Current page</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns>The result of the paged query. Result properties are Items, TotalPages, and TotalRecords.</returns>
        /// <remarks>
        /// In this one instance, because of the connection to the underlying logic of these queries, the user
        /// can pass "SELECT columns" instead of columns.
        /// TO DO: Possibly Possibly cancel the above, it makes no sense from a UI pov!
        /// </remarks>
        override public async Task<PagedResults<T>> PagedFromSelectAsync(
            CancellationToken cancellationToken,
            string tableNameOrJoinSpec,
            string orderBy,
            string columns = null,
            string where = null,
            int pageSize = 20, int currentPage = 1,
            DbConnection connection = null,
            params object[] args)
        {
            return await PagedFromSelectWithParamsAsync(
                cancellationToken,
                tableNameOrJoinSpec,
                orderBy,
                columns,
                where,
                pageSize, currentPage,
                connection: connection,
                args: args).ConfigureAwait(false);
        }

        /// <summary>
        /// Table-specific paging with support for named parameters; there is also a data wrapper version of paging <see cref="PagedFromSelect"/>.
        /// </summary>
        /// <param name="orderBy">You may provide orderBy, if you don't it will try to order by PK and will produce an exception if there is no PK defined.</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="columns">Columns to return</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="currentPage">Current page</param>
        /// <param name="inParams">Named input parameters</param>
        /// <param name="outParams">Named output parameters</param>
        /// <param name="ioParams">Named input-output parameters</param>
        /// <param name="returnParams">Named return parameters</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns>The result of the paged query. Result properties are Items, TotalPages, and TotalRecords.</returns>
        /// <remarks>
        /// <paramref name="columns"/> parameter is not placed first because it's an override to something we may have alread provided in the constructor
        /// (so we don't want the user to have to non-fluently re-type it, or else type null, every time).
        /// </remarks>
        override public async Task<PagedResults<T>> PagedWithParamsAsync(
            string orderBy = null,
            string columns = null,
            string where = null,
            int pageSize = 20,
            int currentPage = 1,
            object inParams = null,
            object outParams = null,
            object ioParams = null,
            object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await PagedWithParamsAsync(
                CancellationToken.None,
                orderBy,
                columns,
                where,
                pageSize,
                currentPage,
                inParams,
                outParams,
                ioParams,
                returnParams,
                connection,
                args).ConfigureAwait(false);
        }

        /// <summary>
        /// Table-specific paging with support for named parameters; there is also a data wrapper version of paging <see cref="PagedFromSelect"/>.
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="orderBy">You may provide orderBy, if you don't it will try to order by PK and will produce an exception if there is no PK defined.</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="columns">Columns to return</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="currentPage">Current page</param>
        /// <param name="inParams">Named input parameters</param>
        /// <param name="outParams">Named output parameters</param>
        /// <param name="ioParams">Named input-output parameters</param>
        /// <param name="returnParams">Named return parameters</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns>The result of the paged query. Result properties are Items, TotalPages, and TotalRecords.</returns>
        /// <remarks>
        /// <paramref name="columns"/> parameter is not placed first because it's an override to something we may have alread provided in the constructor
        /// (so we don't want the user to have to non-fluently re-type it, or else type null, every time).
        /// </remarks>
        override public async Task<PagedResults<T>> PagedWithParamsAsync(
            CancellationToken cancellationToken,
            string orderBy = null,
            string columns = null,
            string where = null,
            int pageSize = 20,
            int currentPage = 1,
            object inParams = null,
            object outParams = null,
            object ioParams = null,
            object returnParams = null,
            DbConnection connection = null,
            params object[] args)
        {
            return await PagedFromSelectWithParamsAsync(
                cancellationToken,
                CheckGetTableName(),
                orderBy ?? PrimaryKeyInfo.CheckGetPrimaryKeyColumns(),
                columns,
                where,
                pageSize, currentPage,
                inParams,
                outParams,
                ioParams,
                returnParams,
                connection,
                args).ConfigureAwait(false);
        }

        /// <summary>
        /// Save one or more items.
        /// 'Save' means
        /// objects with missing or default primary keys are inserted
        /// and objects with non-default primary keys are updated.
        /// </summary>
        /// <param name="items">The items</param>
        /// <returns></returns>
        override public async Task<int> SaveAsync(
            params object[] items)
        {
            return (await ActionOnItemsWithOutputAsync(OrmAction.Save, null, items).ConfigureAwait(false)).Item1;
        }

        /// <summary>
        /// Save one or more items.
        /// 'Save' means
        /// objects with missing or default primary keys are inserted
        /// and objects with non-default primary keys are updated.
        /// </summary>
        /// <param name="connection">The connection to use</param>
        /// <param name="items">The items</param>
        /// <returns></returns>
        override public async Task<int> SaveAsync(
            DbConnection connection,
            params object[] items)
        {
            return (await ActionOnItemsWithOutputAsync(OrmAction.Save, connection, items).ConfigureAwait(false)).Item1;
        }

        /// <summary>
        /// Save one or more items.
        /// 'Save' means
        /// objects with missing or default primary keys are inserted
        /// and objects with non-default primary keys are updated.
        /// </summary>
        /// <param name="items">The items</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        override public async Task<int> SaveAsync(
            CancellationToken cancellationToken,
            params object[] items)
        {
            return (await ActionOnItemsWithOutputAsync(OrmAction.Save, null, items, cancellationToken).ConfigureAwait(false)).Item1;
        }

        /// <summary>
        /// Save one or more items.
        /// 'Save' means
        /// objects with missing or default primary keys are inserted
        /// and objects with non-default primary keys are updated.
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="connection">The connection to use</param>
        /// <param name="items">The items</param>
        /// <returns></returns>
        override public async Task<int> SaveAsync(
            CancellationToken cancellationToken,
            DbConnection connection,
            params object[] items)
        {
            return (await ActionOnItemsWithOutputAsync(OrmAction.Save, connection, items, cancellationToken).ConfigureAwait(false)).Item1;
        }

        /// <summary>
        /// Save array or other <see cref="IEnumerable"/> of items.
        /// 'Save' means
        /// objects with missing or default primary keys are inserted
        /// and objects with non-default primary keys are updated.
        /// </summary>
        /// <param name="items">The items</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        override public async Task<int> SaveAsync(
            IEnumerable<object> items,
            DbConnection connection = null)
        {
            return (await ActionOnItemsWithOutputAsync(OrmAction.Save, connection, items).ConfigureAwait(false)).Item1;
        }

        /// <summary>
        /// Save array or other <see cref="IEnumerable"/> of items.
        /// 'Save' means
        /// objects with missing or default primary keys are inserted
        /// and objects with non-default primary keys are updated.
        /// </summary>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="items">The items</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        override public async Task<int> SaveAsync(
            CancellationToken cancellationToken,
            IEnumerable<object> items,
            DbConnection connection = null)
        {
            return (await ActionOnItemsWithOutputAsync(OrmAction.Save, connection, items, cancellationToken).ConfigureAwait(false)).Item1;
        }

        /// <summary>
        /// Insert single item.
        /// Call <see cref="New"/> before insert if you need to pre-populate your inserted items with any defined database column defaults.
        /// </summary>
        /// <param name="item">The item to insert, in any reasonable format (for MightyOrm&lt;T&gt; this includes, but is not limited to, in instance of type T)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns>The item sent in but with the primary key populated</returns>
        override public async Task<T> InsertAsync(
            object item,
            DbConnection connection = null)
        {
            return (await ActionOnItemsAsync(OrmAction.Insert, null, new object[] { item }).ConfigureAwait(false)).FirstOrDefault();
        }

        /// <summary>
        /// Insert single item.
        /// Call <see cref="New"/> before insert if you need to pre-populate your inserted items with any defined database column defaults.
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="item">The item to insert, in any reasonable format (for MightyOrm&lt;T&gt; this includes, but is not limited to, in instance of type T)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns>The item sent in but with the primary key populated</returns>
        override public async Task<T> InsertAsync(
            CancellationToken cancellationToken,
            object item,
            DbConnection connection = null)
        {
            return (await ActionOnItemsAsync(OrmAction.Insert, null, new object[] { item }, cancellationToken).ConfigureAwait(false)).FirstOrDefault();
        }

        /// <summary>
        /// Insert one or more items.
        /// Call <see cref="New"/> before insert if you need to pre-populate your inserted items with any defined database column defaults.
        /// </summary>
        /// <param name="items">The items</param>
        /// <returns>The items sent in but with the primary keys populated</returns>
        override public async Task<List<T>> InsertAsync(
            params object[] items)
        {
            return await ActionOnItemsAsync(OrmAction.Insert, null, items).ConfigureAwait(false);
        }

        /// <summary>
        /// Insert one or more items.
        /// Call <see cref="New"/> before insert if you need to pre-populate your inserted items with any defined database column defaults.
        /// </summary>
        /// <param name="connection">The connection to use</param>
        /// <param name="items">The items</param>
        /// <returns>The items sent in but with the primary keys populated</returns>
        override public async Task<List<T>> InsertAsync(
            DbConnection connection,
            params object[] items)
        {
            return await ActionOnItemsAsync(OrmAction.Insert, connection, items).ConfigureAwait(false);
        }

        /// <summary>
        /// Insert one or more items.
        /// Call <see cref="New"/> before insert if you need to pre-populate your inserted items with any defined database column defaults.
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="items">The items</param>
        /// <returns>The items sent in but with the primary keys populated</returns>
        override public async Task<List<T>> InsertAsync(
            CancellationToken cancellationToken,
            params object[] items)
        {
            return await ActionOnItemsAsync(OrmAction.Insert, null, items, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Insert one or more items.
        /// Call <see cref="New"/> before insert if you need to pre-populate your inserted items with any defined database column defaults.
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="connection">The connection to use</param>
        /// <param name="items">The items</param>
        /// <returns>The items sent in but with the primary keys populated</returns>
        override public async Task<List<T>> InsertAsync(
            CancellationToken cancellationToken,
            DbConnection connection,
            params object[] items)
        {
            return await ActionOnItemsAsync(OrmAction.Insert, connection, items, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Insert array or other <see cref="IEnumerable"/> of items.
        /// Call <see cref="New"/> before insert if you need to pre-populate your inserted items with any defined database column defaults.
        /// </summary>
        /// <param name="items">The items</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns>The items sent in but with the primary keys populated</returns>
        override public async Task<List<T>> InsertAsync(
            IEnumerable<object> items,
            DbConnection connection = null)
        {
            return await ActionOnItemsAsync(OrmAction.Insert, connection, items).ConfigureAwait(false);
        }

        /// <summary>
        /// Insert array or other <see cref="IEnumerable"/> of items.
        /// Call <see cref="New"/> before insert if you need to pre-populate your inserted items with any defined database column defaults.
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="items">The items</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns>The items sent in but with the primary keys populated</returns>
        override public async Task<List<T>> InsertAsync(
            CancellationToken cancellationToken,
            IEnumerable<object> items,
            DbConnection connection = null)
        {
            return await ActionOnItemsAsync(OrmAction.Insert, connection, items, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Update one or more items.
        /// </summary>
        /// <param name="items">The items</param>
        /// <returns></returns>
        override public async Task<int> UpdateAsync(
            params object[] items)
        {
            return (await ActionOnItemsWithOutputAsync(OrmAction.Update, null, items).ConfigureAwait(false)).Item1;
        }

        /// <summary>
        /// Update one or more items.
        /// </summary>
        /// <param name="connection">The connection to use</param>
        /// <param name="items">The items</param>
        /// <returns></returns>
        override public async Task<int> UpdateAsync(
            DbConnection connection,
            params object[] items)
        {
            return (await ActionOnItemsWithOutputAsync(OrmAction.Update, connection, items).ConfigureAwait(false)).Item1;
        }

        /// <summary>
        /// Update one or more items.
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="items">The items</param>
        /// <returns></returns>
        override public async Task<int> UpdateAsync(
            CancellationToken cancellationToken,
            params object[] items)
        {
            return (await ActionOnItemsWithOutputAsync(OrmAction.Update, null, items, cancellationToken).ConfigureAwait(false)).Item1;
        }

        /// <summary>
        /// Update one or more items.
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="connection">The connection to use</param>
        /// <param name="items">The items</param>
        /// <returns></returns>
        override public async Task<int> UpdateAsync(
            CancellationToken cancellationToken,
            DbConnection connection,
            params object[] items)
        {
            return (await ActionOnItemsWithOutputAsync(OrmAction.Update, connection, items, cancellationToken).ConfigureAwait(false)).Item1;
        }

        /// <summary>
        /// Update array or other <see cref="IEnumerable"/> of items.
        /// </summary>
        /// <param name="items">The items</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        override public async Task<int> UpdateAsync(
            IEnumerable<object> items,
            DbConnection connection = null)
        {
            return (await ActionOnItemsWithOutputAsync(OrmAction.Update, connection, items).ConfigureAwait(false)).Item1;
        }

        /// <summary>
        /// Update array or other <see cref="IEnumerable"/> of items.
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="items">The items</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        override public async Task<int> UpdateAsync(
            CancellationToken cancellationToken,
            IEnumerable<object> items,
            DbConnection connection = null)
        {
            return (await ActionOnItemsWithOutputAsync(OrmAction.Update, connection, items, cancellationToken).ConfigureAwait(false)).Item1;
        }

        /// <summary>
        /// Delete one or more items.
        /// Each argument may be (or contain) a value (or values) only, in which case
        /// it specifies the primary key value(s) of the item to delete, or it can be any object containing name-values pairs in which case
        /// it should contain fields with names matching the primary key(s) whose values will specify the item to delete (but it may contain
        /// other fields as well which will be ignored here).
        /// </summary>
        /// <param name="items">The items</param>
        /// <returns>The number of items affected</returns>
        override public async Task<int> DeleteAsync(
            params object[] items)
        {
            return (await ActionOnItemsWithOutputAsync(OrmAction.Delete, null, items).ConfigureAwait(false)).Item1;
        }

        /// <summary>
        /// Delete one or more items.
        /// Each argument may be (or contain) a value (or values) only, in which case
        /// it specifies the primary key value(s) of the item to delete, or it can be any object containing name-values pairs in which case
        /// it should contain fields with names matching the primary key(s) whose values will specify the item to delete (but it may contain
        /// other fields as well which will be ignored here).
        /// </summary>
        /// <param name="items">The items</param>
        /// <param name="connection">The connection to use</param>
        /// <returns>The number of items affected</returns>
        override public async Task<int> DeleteAsync(
            DbConnection connection,
            params object[] items)
        {
            return (await ActionOnItemsWithOutputAsync(OrmAction.Delete, connection, items).ConfigureAwait(false)).Item1;
        }

        /// <summary>
        /// Delete one or more items.
        /// Each argument may be (or contain) a value (or values) only, in which case
        /// it specifies the primary key value(s) of the item to delete, or it can be any object containing name-values pairs in which case
        /// it should contain fields with names matching the primary key(s) whose values will specify the item to delete (but it may contain
        /// other fields as well which will be ignored here).
        /// </summary>
        /// <param name="items">The items</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns>The number of items affected</returns>
        override public async Task<int> DeleteAsync(
            CancellationToken cancellationToken,
            params object[] items)
        {
            return (await ActionOnItemsWithOutputAsync(OrmAction.Delete, null, items, cancellationToken).ConfigureAwait(false)).Item1;
        }

        /// <summary>
        /// Delete one or more items.
        /// Each argument may be (or contain) a value (or values) only, in which case
        /// it specifies the primary key value(s) of the item to delete, or it can be any object containing name-values pairs in which case
        /// it should contain fields with names matching the primary key(s) whose values will specify the item to delete (but it may contain
        /// other fields as well which will be ignored here).
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="connection">The connection to use</param>
        /// <param name="items">The items</param>
        /// <returns>The number of items affected</returns>
        override public async Task<int> DeleteAsync(
            CancellationToken cancellationToken,
            DbConnection connection,
            params object[] items)
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
        /// <param name="connection">Optional connection to use</param>
        /// <returns>The number of items affected</returns>
        override public async Task<int> DeleteAsync(
            IEnumerable<object> items,
            DbConnection connection = null)
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
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="items">The items</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns>The number of items affected</returns>
        override public async Task<int> DeleteAsync(
            CancellationToken cancellationToken,
            IEnumerable<object> items,
            DbConnection connection = null)
        {
            return (await ActionOnItemsWithOutputAsync(OrmAction.Delete, connection, items, cancellationToken).ConfigureAwait(false)).Item1;
        }

        /// <summary>
        /// Update the row(s) specified by the primary key(s) or WHERE values sent in using the values from the item sent in.
        /// If `keys` has been specified on the current Mighty instance then any primary key fields in the item are ignored.
        /// The item is not filtered to remove fields not in the table, if you need that you can call <see cref="New"/> with first parameter `partialItem` and second parameter `false` first.
        /// </summary>
        /// <param name="partialItem">Item containing values to update with</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
        override public async Task<int> UpdateUsingAsync(
            object partialItem,
            object whereParams,
            DbConnection connection = null)
        {
            return await UpdateUsingAsync(CancellationToken.None, partialItem, whereParams, connection).ConfigureAwait(false);
        }

        /// <summary>
        /// Update the row(s) specified by the primary key(s) or WHERE values sent in using the values from the item sent in.
        /// If `keys` has been specified on the current Mighty instance then any primary key fields in the item are ignored.
        /// The item is not filtered to remove fields not in the table, if you need that you can call <see cref="New"/> with first parameter `partialItem` and second parameter `false` first.
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="partialItem">Item containing values to update with</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
        override public async Task<int> UpdateUsingAsync(
            CancellationToken cancellationToken,
            object partialItem,
            object whereParams,
            DbConnection connection = null)
        {
            Tuple<string, object, object[]> retval = GetWhereSpecFromWhereParams(whereParams);
            return await UpdateUsingWithParamsAsync(
                cancellationToken,
                partialItem,
                where: retval.Item1, inParams: retval.Item2, args: retval.Item3,
                connection: connection).ConfigureAwait(false);
        }

        /// <summary>
        /// Update all items matching WHERE clause using fields from the item sent in.
        /// If `keys` has been specified on the current Mighty instance then any primary key fields in the item are ignored.
        /// The item is not filtered to remove fields not in the table, if you need that you can call <see cref="New"/> with first parameter `partialItem` and second parameter `false` first.
        /// </summary>
        /// <param name="partialItem">Item containing values to update with</param>
        /// <param name="where">WHERE clause specifying which rows to update</param>
        /// <param name="args">Auto-numbered input parameters</param>
        override public async Task<int> UpdateUsingAsync(
            object partialItem,
            string where,
            params object[] args)
        {
            return await UpdateUsingWithParamsAsync(
                CancellationToken.None,
                partialItem,
                where,
                null,
                null,
                args).ConfigureAwait(false);
        }

        /// <summary>
        /// Update all items matching WHERE clause using fields from the item sent in.
        /// If `keys` has been specified on the current Mighty instance then any primary key fields in the item are ignored.
        /// The item is not filtered to remove fields not in the table, if you need that you can call <see cref="New"/> with first parameter `partialItem` and second parameter `false` first.
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="partialItem">Item containing values to update with</param>
        /// <param name="where">WHERE clause specifying which rows to update</param>
        /// <param name="args">Auto-numbered input parameters</param>
        override public async Task<int> UpdateUsingAsync(
            CancellationToken cancellationToken,
            object partialItem,
            string where,
            params object[] args)
        {
            return await UpdateUsingWithParamsAsync(
                cancellationToken,
                partialItem,
                where,
                null,
                args).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete one or more items based on a WHERE clause.
        /// </summary>
        /// <param name="where">
        /// Non-optional WHERE clause.
        /// Specify "1=1" if you are sure that you want to delete all rows.
        /// </param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns>The number of items affected</returns>
        override public async Task<int> DeleteAsync(
            string where,
            params object[] args)
        {
            return await DeleteAsync(where, null, args).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete one or more items based on a WHERE clause.
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="where">
        /// Non-optional WHERE clause.
        /// Specify "1=1" if you are sure that you want to delete all rows.
        /// </param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns>The number of items affected</returns>
        override public async Task<int> DeleteAsync(
            CancellationToken cancellationToken,
            string where,
            params object[] args)
        {
            return await DeleteAsync(cancellationToken, where, null, args).ConfigureAwait(false);
        }
        #endregion
    }
}
#endif