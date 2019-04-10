#if !NET40
using System.Collections.Generic;
using System.Collections.Async;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Mighty.Mocking;
using Mighty.Mapping;
using Mighty.Plugins;
using Mighty.Profiling;
using Mighty.Validation;
using System;
using System.Dynamic;

/// <summary>
/// MightyOrm_Redirects.cs holds methods in Mighty than can be very simply defined in terms of other methods.
/// </summary>
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
		/// <param name="sql"></param>
		/// <param name="inParams"></param>
		/// <param name="outParams"></param>
		/// <param name="ioParams"></param>
		/// <param name="returnParams"></param>
		/// <param name="connection"></param>
		/// <param name="args"></param>
		/// <returns>The results of all non-input parameters</returns>
		override public async Task<dynamic> ExecuteWithParamsAsync(string sql,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args)
		{
            using (var command = CreateCommandWithParams(sql,
                inParams, outParams, ioParams, returnParams,
                args: args))
            {
                await ExecuteAsync(command, connection).ConfigureAwait(false);
                return ResultsAsExpando(command);
            }
		}
		override public async Task<dynamic> ExecuteWithParamsAsync(string sql,
			CancellationToken cancellationToken,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args)
		{
            using (var command = CreateCommandWithParams(sql,
                inParams, outParams, ioParams, returnParams,
                args: args))
            {
                await ExecuteAsync(command, cancellationToken, connection).ConfigureAwait(false);
                return ResultsAsExpando(command);
            }
		}

		/// <summary>
		/// Execute stored procedure with parameters
		/// </summary>
		/// <param name="spName"></param>
		/// <param name="inParams"></param>
		/// <param name="outParams"></param>
		/// <param name="ioParams"></param>
		/// <param name="returnParams"></param>
		/// <param name="connection"></param>
		/// <param name="args"></param>
		/// <returns>The results of all non-input parameters</returns>
		override public async Task<dynamic> ExecuteProcedureAsync(string spName,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args)
		{
            using (var command = CreateCommandWithParams(spName,
            inParams, outParams, ioParams, returnParams,
            isProcedure: true,
            args: args))
            {
                await ExecuteAsync(command, connection).ConfigureAwait(false);
                return ResultsAsExpando(command);
            }
		}
		override public async Task<dynamic> ExecuteProcedureAsync(string spName,
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
                await ExecuteAsync(command, cancellationToken, connection).ConfigureAwait(false);
                return ResultsAsExpando(command);
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
		/// <summary>
		/// Perform scalar operation on the current table (use for SUM, MAX, MIN, AVG, etc.)
		/// </summary>
		/// <param name="expression">Scalar expression</param>
		/// <param name="where">Optional where clause</param>
		/// <param name="connection">Optional connection to use</param>
		/// <param name="args">Parameters</param>
		/// <returns></returns>
		override public async Task<object> AggregateAsync(string expression, string where = null,
			DbConnection connection = null,
			params object[] args)
		{
			return await AggregateWithParamsAsync(expression, where, connection: connection, args: args).ConfigureAwait(false);
		}
		override public async Task<object> AggregateAsync(string expression, CancellationToken cancellationToken, string where = null,
			DbConnection connection = null,
			params object[] args)
		{
			return await AggregateWithParamsAsync(expression, cancellationToken, where, connection: connection, args: args).ConfigureAwait(false);
		}

        /// <summary>
        /// Get single object from the current table using primary key or name-value specification.
        /// </summary>
        /// <param name="whereParams">Value(s) which are mapped to the table's primary key(s), or named field(s) which are mapped to the named column(s)</param>
        /// <param name="columns">Optional list of columns to retrieve</param>
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
        /// <param name="whereParams">Value(s) which are mapped to the table's primary key(s), or named field(s) which are mapped to the named column(s)</param>
        /// <param name="columns">Optional list of columns to retrieve</param>
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
        /// <param name="where">Where clause</param>
        /// <param name="args">Optional auto-named params</param>
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
		/// <param name="where"></param>
		/// <param name="connection"></param>
		/// <param name="orderBy"></param>
		/// <param name="columns"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		/// <remarks>
		/// DbConnection coming early (not just before args) in this one case is really useful, as it avoids ambiguity between
		/// the <see cref="columns" /> and <see cref="orderBy" /> strings and optional string args.
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

		/// <summary>
		/// Table-specific paging; there is also a data wrapper version of paging <see cref="PagedFromSelect"/>.
		/// </summary>
		/// <param name="orderBy">You may provide orderBy, if you don't it will try to order by PK and will produce an exception if there is no PK defined.</param>
		/// <param name="where"></param>
		/// <param name="columns"></param>
		/// <param name="pageSize"></param>
		/// <param name="currentPage"></param>
		/// <param name="connection"></param>
		/// <param name="args"></param>
		/// <returns>The result of the paged query. Result properties are Items, TotalPages, and TotalRecords.</returns>
		/// <remarks>
		/// <see cref="columns"/> parameter is not placed first because it's an override to something we may have alread provided in the constructor
		/// (so we don't want the user to have to non-fluently re-type it, or else type null, every time).
		/// </remarks>
		override public async Task<PagedResults<T>> PagedAsync(
            string orderBy = null,
            string where = null,
			string columns = null,
			int pageSize = 20, int currentPage = 1,
			DbConnection connection = null,
			params object[] args)
		{
			return await PagedFromSelectAsync(columns, CheckGetTableName(), orderBy ?? CheckGetPrimaryKeyFields(), where, pageSize, currentPage, connection, args).ConfigureAwait(false);
		}
		override public async Task<PagedResults<T>> PagedAsync(
            CancellationToken cancellationToken,
            string orderBy = null,
            string where = null,
			string columns = null,
			int pageSize = 20, int currentPage = 1,
			DbConnection connection = null,
			params object[] args)
		{
			return await PagedFromSelectAsync(columns, CheckGetTableName(), orderBy ?? CheckGetPrimaryKeyFields(), where, cancellationToken, pageSize, currentPage, connection, args).ConfigureAwait(false);
		}

		/// <summary>
		/// Save one or more items using params style arguments
		/// </summary>
		/// <param name="items">The items</param>
		/// <returns></returns>
		override public async Task<int> SaveAsync(params object[] items)
		{
			return await ActionOnItemsAsync(OrmAction.Save, null, items).ConfigureAwait(false);
		}
		override public async Task<int> SaveAsync(CancellationToken cancellationToken, params object[] items)
		{
			return await ActionOnItemsAsync(OrmAction.Save, null, items, cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		/// Save one or more items using pre-specified <see cref="DbConnection"/>
		/// </summary>
		/// <param name="connection">The connection</param>
		/// <param name="items">The items</param>
		/// <returns></returns>
		override public async Task<int> SaveAsync(DbConnection connection, params object[] items)
		{
			return await ActionOnItemsAsync(OrmAction.Save, connection, items).ConfigureAwait(false);
		}
		override public async Task<int> SaveAsync(DbConnection connection, CancellationToken cancellationToken, params object[] items)
		{
			return await ActionOnItemsAsync(OrmAction.Save, connection, items, cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		/// Save array or other <see cref="IEnumerable"/> of items
		/// </summary>
		/// <param name="items">The items</param>
		/// <returns></returns>
		override public async Task<int> SaveAsync(IEnumerable<object> items)
		{
			return await ActionOnItemsAsync(OrmAction.Save, null, items).ConfigureAwait(false);
		}
		override public async Task<int> SaveAsync(IEnumerable<object> items, CancellationToken cancellationToken)
		{
			return await ActionOnItemsAsync(OrmAction.Save, null, items, cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		/// Save array or other <see cref="IEnumerable"/> of items using pre-specified <see cref="DbConnection"/>
		/// </summary>
		/// <param name="connection">The connection</param>
		/// <param name="items">The items</param>
		/// <returns></returns>
		override public async Task<int> SaveAsync(DbConnection connection, IEnumerable<object> items)
		{
			return await ActionOnItemsAsync(OrmAction.Save, connection, items).ConfigureAwait(false);
		}
		override public async Task<int> SaveAsync(DbConnection connection, IEnumerable<object> items, CancellationToken cancellationToken)
		{
			return await ActionOnItemsAsync(OrmAction.Save, connection, items, cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		/// Insert single item, returning the item sent in but with PK populated.
		/// If you need all fields populated (i.e. you want to get back DB default values for non-PK fields), please create the item using New() before inserting it.
		/// </summary>
		/// <param name="items">The item to insert, in any reasonable format (for MightyOrm&lt;T&gt; this includes, but is not limited to, in instance of type T)</param>
		/// <returns>The inserted item</returns>
		override public async Task<T> InsertAsync(object item)
		{
			return (await ActionOnItemsWithOutputAsync(OrmAction.Insert, null, new object[] { item }).ConfigureAwait(false)).Item2;
		}
		override public async Task<T> InsertAsync(object item, CancellationToken cancellationToken)
		{
			return (await ActionOnItemsWithOutputAsync(OrmAction.Insert, null, new object[] { item }, cancellationToken).ConfigureAwait(false)).Item2;
		}

		/// <summary>
		/// Insert one or more items using params style arguments
		/// </summary>
		/// <param name="items">The items</param>
		/// <returns>The number of rows inserted</returns>
		override public async Task<int> InsertAsync(params object[] items)
		{
			return await ActionOnItemsAsync(OrmAction.Insert, null, items).ConfigureAwait(false);
		}
		override public async Task<int> InsertAsync(CancellationToken cancellationToken, params object[] items)
		{
			return await ActionOnItemsAsync(OrmAction.Insert, null, items, cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		/// Insert one or more items using pre-specified <see cref="DbConnection"/>
		/// </summary>
		/// <param name="connection">The connection</param>
		/// <param name="items">The items</param>
		/// <returns>The number of rows inserted</returns>
		override public async Task<int> InsertAsync(DbConnection connection, params object[] items)
		{
			return await ActionOnItemsAsync(OrmAction.Insert, connection, items).ConfigureAwait(false);
		}
		override public async Task<int> InsertAsync(DbConnection connection, CancellationToken cancellationToken, params object[] items)
		{
			return await ActionOnItemsAsync(OrmAction.Insert, connection, items, cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		/// Insert array or other <see cref="IEnumerable"/> of items
		/// </summary>
		/// <param name="items">The items</param>
		/// <returns>The number of rows inserted</returns>
		override public async Task<int> InsertAsync(IEnumerable<object> items)
		{
			return await ActionOnItemsAsync(OrmAction.Insert, null, items).ConfigureAwait(false);
		}
		override public async Task<int> InsertAsync(IEnumerable<object> items, CancellationToken cancellationToken)
		{
			return await ActionOnItemsAsync(OrmAction.Insert, null, items, cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		/// Insert array or other <see cref="IEnumerable"/> of items using pre-specified <see cref="DbConnection"/>
		/// </summary>
		/// <param name="connection">The connection</param>
		/// <param name="items">The items</param>
		/// <returns>The number of rows inserted</returns>
		override public async Task<int> InsertAsync(DbConnection connection, IEnumerable<object> items)
		{
			return await ActionOnItemsAsync(OrmAction.Insert, connection, items).ConfigureAwait(false);
		}
		override public async Task<int> InsertAsync(DbConnection connection, IEnumerable<object> items, CancellationToken cancellationToken)
		{
			return await ActionOnItemsAsync(OrmAction.Insert, connection, items, cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		/// Update one or more items using params style arguments
		/// </summary>
		/// <param name="items">The items</param>
		/// <returns></returns>
		override public async Task<int> UpdateAsync(params object[] items)
		{
			return await ActionOnItemsAsync(OrmAction.Update, null, items).ConfigureAwait(false);
		}
		override public async Task<int> UpdateAsync(CancellationToken cancellationToken, params object[] items)
		{
			return await ActionOnItemsAsync(OrmAction.Update, null, items, cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		/// Update one or more items using pre-specified <see cref="DbConnection"/>
		/// </summary>
		/// <param name="connection">The connection</param>
		/// <param name="items">The items</param>
		/// <returns></returns>
		override public async Task<int> UpdateAsync(DbConnection connection, params object[] items)
		{
			return await ActionOnItemsAsync(OrmAction.Update, connection, items).ConfigureAwait(false);
		}
		override public async Task<int> UpdateAsync(DbConnection connection, CancellationToken cancellationToken, params object[] items)
		{
			return await ActionOnItemsAsync(OrmAction.Update, connection, items, cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		/// Update array or other <see cref="IEnumerable"/> of items
		/// </summary>
		/// <param name="items">The items</param>
		/// <returns></returns>
		override public async Task<int> UpdateAsync(IEnumerable<object> items)
		{
			return await ActionOnItemsAsync(OrmAction.Update, null, items).ConfigureAwait(false);
		}
		override public async Task<int> UpdateAsync(IEnumerable<object> items, CancellationToken cancellationToken)
		{
			return await ActionOnItemsAsync(OrmAction.Update, null, items, cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		/// Update array or other <see cref="IEnumerable"/> of items using pre-specified <see cref="DbConnection"/>
		/// </summary>
		/// <param name="connection">The connection</param>
		/// <param name="items">The items</param>
		/// <returns></returns>
		override public async Task<int> UpdateAsync(DbConnection connection, IEnumerable<object> items)
		{
			return await ActionOnItemsAsync(OrmAction.Update, connection, items).ConfigureAwait(false);
		}
		override public async Task<int> UpdateAsync(DbConnection connection, IEnumerable<object> items, CancellationToken cancellationToken)
		{
			return await ActionOnItemsAsync(OrmAction.Update, connection, items, cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		/// Delete one or more items using params style arguments
		/// </summary>
		/// <param name="items">The items</param>
		/// <returns></returns>
		override public async Task<int> DeleteAsync(params object[] items)
		{
			return await ActionOnItemsAsync(OrmAction.Delete, null, items).ConfigureAwait(false);
		}
		override public async Task<int> DeleteAsync(CancellationToken cancellationToken, params object[] items)
		{
			return await ActionOnItemsAsync(OrmAction.Delete, null, items, cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		/// Delete one or more items using pre-specified <see cref="DbConnection"/>
		/// </summary>
		/// <param name="connection">The connection</param>
		/// <param name="items">The items</param>
		/// <returns></returns>
		override public async Task<int> DeleteAsync(DbConnection connection, params object[] items)
		{
			return await ActionOnItemsAsync(OrmAction.Delete, connection, items).ConfigureAwait(false);
		}
		override public async Task<int> DeleteAsync(DbConnection connection, CancellationToken cancellationToken, params object[] items)
		{
			return await ActionOnItemsAsync(OrmAction.Delete, connection, items, cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		/// Delete array or other <see cref="IEnumerable"/> of items
		/// </summary>
		/// <param name="items">The items</param>
		/// <returns></returns>
		override public async Task<int> DeleteAsync(IEnumerable<object> items)
		{
			return await ActionOnItemsAsync(OrmAction.Delete, null, items).ConfigureAwait(false);
		}
		override public async Task<int> DeleteAsync(IEnumerable<object> items, CancellationToken cancellationToken)
		{
			return await ActionOnItemsAsync(OrmAction.Delete, null, items, cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		/// Delete array or other <see cref="IEnumerable"/> of items using pre-specified <see cref="DbConnection"/>
		/// </summary>
		/// <param name="connection">The connection</param>
		/// <param name="items">The items</param>
		/// <returns></returns>
		override public async Task<int> DeleteAsync(DbConnection connection, IEnumerable<object> items)
		{
			return await ActionOnItemsAsync(OrmAction.Delete, connection, items).ConfigureAwait(false);
		}
		override public async Task<int> DeleteAsync(DbConnection connection, IEnumerable<object> items, CancellationToken cancellationToken)
		{
			return await ActionOnItemsAsync(OrmAction.Delete, connection, items, cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		/// Apply all fields which are present in item to the row matching key.
		/// We *don't* filter by available columns - call with <see cref="CreateFrom"/>(<see cref="partialItem"/>) to do that.
		/// </summary>
		/// <param name="partialItem"></param>
		/// <param name="key"></param>
		override public async Task<int> UpdateUsingAsync(object partialItem, object key)
		{
			return await UpdateUsingAsync(partialItem, key, null).ConfigureAwait(false);
		}
		override public async Task<int> UpdateUsingAsync(object partialItem, object key, CancellationToken cancellationToken)
		{
			return await UpdateUsingAsync(partialItem, key, null, cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		/// Apply all fields which are present in item to the row matching key.
		/// We *don't* filter by available columns - call with <see cref="CreateFrom"/>(<see cref="partialItem"/>) to do that.
		/// </summary>
		/// <param name="partialItem"></param>
		/// <param name="key"></param>
		/// <param name="connection"></param>
		override public async Task<int> UpdateUsingAsync(object partialItem, object key,
			DbConnection connection)
		{
			return await UpdateUsingAsync(partialItem, WhereForKeys(), connection, args: KeyValuesFromKey(key)).ConfigureAwait(false);
		}
		override public async Task<int> UpdateUsingAsync(object partialItem, object key,
			DbConnection connection, CancellationToken cancellationToken)
		{
			return await UpdateUsingAsync(partialItem, WhereForKeys(), connection, cancellationToken, args: KeyValuesFromKey(key)).ConfigureAwait(false);
		}

		/// <summary>
		/// Apply all fields which are present in item to all rows matching where clause
		/// for safety you MUST specify the where clause yourself (use "1=1" to update all rows)/
		/// This removes/ignores any PK fields from the action; keeps auto-named params for args,
		/// and uses named params for the update feilds.
		/// </summary>
		/// <param name="partialItem"></param>
		/// <param name="where"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		override public async Task<int> UpdateUsingAsync(object partialItem, string where,
			params object[] args)
		{
			return await UpdateUsingAsync(partialItem, where, null, args).ConfigureAwait(false);
		}
		override public async Task<int> UpdateUsingAsync(object partialItem, string where,
			CancellationToken cancellationToken,
			params object[] args)
		{
			return await UpdateUsingAsync(partialItem, where, null, cancellationToken, args).ConfigureAwait(false);
		}

		/// <summary>
		/// Delete rows from ORM table based on WHERE clause.
		/// </summary>
		/// <param name="where">
		/// Non-optional where clause.
		/// Specify "1=1" if you are sure that you want to delete all rows.</param>
		/// <param name="args">Optional auto-named parameters for the WHERE clause</param>
		/// <returns></returns>
		override public async Task<int> DeleteAsync(string where,
			params object[] args)
		{
			return await DeleteAsync(where, null, args).ConfigureAwait(false);
		}
		override public async Task<int> DeleteAsync(string where,
			CancellationToken cancellationToken,
			params object[] args)
		{
			return await DeleteAsync(where, null, cancellationToken, args).ConfigureAwait(false);
		}

		internal async Task<int> ActionOnItemsAsync(OrmAction action, DbConnection connection, IEnumerable<object> items)
		{
			return (await ActionOnItemsWithOutputAsync(action, connection, items).ConfigureAwait(false)).Item1;
		}
		internal async Task<int> ActionOnItemsAsync(OrmAction action, DbConnection connection, IEnumerable<object> items, CancellationToken cancellationToken)
		{
			return (await ActionOnItemsWithOutputAsync(action, connection, items, cancellationToken).ConfigureAwait(false)).Item1;
		}
		#endregion
	}
}
#endif