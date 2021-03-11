#if !NET40
using System.Collections;
using Dasync.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

using Mighty.Plugins;
using Mighty.Mapping;
using Mighty.Profiling;
using Mighty.Validation;
using System.Threading;

// <summary>
// TO DO: Not sure about putting this in a separate namespace, but maybe best to hide the mockable version?
// </summary>
namespace Mighty.Interfaces
{
    // NEW new:
    //    - Clean support for Single with columns
    //    - Compound PKs
    //    - Cleaner support for sequences (incl. one less DB round-trip on sequence-based insert)
    //    - With the new inner loop this really might be faster than Massive too. 'Kinell.
    //  - True support for ulong for those ADO.NET providers which use it (MySQL...) [CHECK THIS!!]
    //  - Generics(!)
    // To Add:
    //  - Firebird(?)
    // We:
    //  - Solve the problem of default values (https://samsaffron.com/archive/2012/01/16/that-annoying-insert-problem-getting-data-into-the-db-using-dapper)
    //      by ignoring them at Insert(), but by populating them (in a slightly fake, but working, way) on New()
    //    - Are genuinely cross DB, unlike Dapper Rainbow (and possibly unlike other bits of Dapper?)
    //  - Have a true System.Data hiding interface - you just don't use it *at all* unless you need transactions,
    //      in which case you use exactly enough of it to manage your transactions, and no more.
    //    - Have an (arguably) nicer/simpler interface to parameter directions and output values than Dapper.

    // Abstract class 'interface' for the ORM and ADO.NET Data Access Wrapper methods.
    // Uses abstract class, not interface, because the semantics of interface mean it can never have anything added to it!
    // (See ... MS document about DB classes; SO post about interfaces)
    //
    // Notes:
    //    - Any params type argument is always last (it has to be)
    //    - DbConnection is always last (or last before a params argument, if any), except in the Single-with-columns overload, where it needs to be where
    //      it is to play the very useful dual role of also disambiguating calls to this overload from calls to the simpler overload without columns.
    //    - All database parameters (i.e. everything sent to the DB via args, inParams or ioParams) are always passed in as true database
    //      parameters under all circumstances - they are never interpolated into SQL - so they can never be used for _direct_ SQL injection.
    //      So assuming you aren't building any SQL to execute yourself within the DB, from the values passed in, then strings etc. which are
    //      passed in will not need any escaping to be safe.
    //
    // NB Mighty is dynamic-focussed, so even when you are using MightyOrm<T> instead of MightyOrm (which is like MightyOrm<dynamic>), the
    // T determines the output type, but not the input type (which can be of type T, but can also be any of the various arbitrary objects
    // which the micro-ORM supports, with appropriately named fields).
    abstract public partial class MightyOrmAbstractInterface<T>
    {
        // We are always adding optional `CancellationToken` after all compulsory params and just
        // before any optional or `params` params.
        // Also, even though it is quite a lot more work, we are providing versions with and without
        // CancellationToken, so that the async interface to our `args` variants stays nice if you are not
        // passing a CancellationToken.

        // 'Interface' for the general purpose data access wrapper methods (i.e. the ones which can be used
        // even if no table has been specified).
        // All versions which simply redirect to other versions are defined here, not in the main class.
        #region Non-table specific methods
        /// <summary>
        /// Creates a new DbConnection. You do not normally need to call this! (MightyOrm normally manages its own
        /// connections. Create a connection here and pass it on to other MightyOrm commands only in non-standard use
        /// cases where you need to explicitly manage transactions or share connections, e.g. when using explicit cursors.)
        /// </summary>
        /// <returns></returns>
        abstract public Task<DbConnection> OpenConnectionAsync();

        /// <summary>
        /// Creates a new DbConnection. You do not normally need to call this! (MightyOrm normally manages its own
        /// connections. Create a connection here and pass it on to other MightyOrm commands only in non-standard use
        /// cases where you need to explicitly manage transactions or share connections, e.g. when using explicit cursors.)
        /// </summary>
        /// <param name="connectionString">Connection string to use</param>
        /// <returns></returns>
        abstract public Task<DbConnection> OpenConnectionAsync(string connectionString);

        /// <summary>
        /// Creates a new DbConnection. You do not normally need to call this! (MightyOrm normally manages its own
        /// connections. Create a connection here and pass it on to other MightyOrm commands only in non-standard use
        /// cases where you need to explicitly manage transactions or share connections, e.g. when using explicit cursors.)
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        abstract public Task<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Creates a new DbConnection. You do not normally need to call this! (MightyOrm normally manages its own
        /// connections. Create a connection here and pass it on to other MightyOrm commands only in non-standard use
        /// cases where you need to explicitly manage transactions or share connections, e.g. when using explicit cursors.)
        /// </summary>
        /// <param name="connectionString">Connection string to use</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        abstract public Task<DbConnection> OpenConnectionAsync(string connectionString, CancellationToken cancellationToken);

        /// <summary>
        /// Get <see cref="IEnumerable{T}"/> of items returned by database command.
        /// </summary>
        /// <param name="command">The command to execute</param>
        /// <param name="connection">The connection to use</param>
        /// <returns></returns>
        abstract public Task<IAsyncEnumerable<T>> QueryAsync(DbCommand command,
            DbConnection connection = null);

        /// <summary>
        /// Get <see cref="IEnumerable{T}"/> of items returned by database command.
        /// </summary>
        /// <param name="command">The command to execute</param>
        /// <param name="connection">The connection to use</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        abstract public Task<IAsyncEnumerable<T>> QueryAsync(DbCommand command,
            CancellationToken cancellationToken,
            DbConnection connection = null);

        /// <summary>
        /// Get single item returned by database command.
        /// </summary>
        /// <param name="command">The command to execute</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        abstract public Task<T> SingleAsync(DbCommand command,
            DbConnection connection = null);

        /// <summary>
        /// Get single item returned by database command.
        /// </summary>
        /// <param name="command">The command to execute</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        abstract public Task<T> SingleAsync(DbCommand command,
            CancellationToken cancellationToken,
            DbConnection connection = null);

        /// <summary>
        /// Get <see cref="IEnumerable{T}"/> of items returned by SQL query.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="args">Auto-numbered parameter values for SQL</param>
        /// <returns></returns>
        /// <remarks>
        /// 'Easy-calling' version, optional args straight after SQL.
        /// </remarks>
        abstract public Task<IAsyncEnumerable<T>> QueryAsync(string sql,
            params object[] args);

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
        abstract public Task<IAsyncEnumerable<T>> QueryAsync(string sql,
            CancellationToken cancellationToken,
            params object[] args);

        /// <summary>
        /// Get single item from query.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        /// <remarks>
        /// 'Easy-calling' version, optional args straight after SQL.
        /// </remarks>
        abstract public Task<T> SingleFromQueryAsync(string sql,
            params object[] args);

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
        abstract public Task<T> SingleFromQueryAsync(string sql,
            CancellationToken cancellationToken,
            params object[] args);

        /// <summary>
        /// Get <see cref="IEnumerable{T}"/> of items returned by SQL query.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="connection">The connection to use</param>
        /// <param name="args">Auto-numbered parameter values for SQL</param>
        /// <returns></returns>
        abstract public Task<IAsyncEnumerable<T>> QueryAsync(string sql,
            DbConnection connection,
            params object[] args);

        /// <summary>
        /// Get <see cref="IEnumerable{T}"/> of items returned by SQL query.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="connection">The connection to use</param>
        /// <param name="args">Auto-numbered parameter values for SQL</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        abstract public Task<IAsyncEnumerable<T>> QueryAsync(string sql,
            DbConnection connection,
            CancellationToken cancellationToken,
            params object[] args);

        /// <summary>
        /// Get single item from query.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="connection">The connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        abstract public Task<T> SingleFromQueryAsync(string sql,
            DbConnection connection,
            params object[] args);

        /// <summary>
        /// Get single item from query.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="connection">The connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        abstract public Task<T> SingleFromQueryAsync(string sql,
            DbConnection connection,
            CancellationToken cancellationToken,
            params object[] args);

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
        abstract public Task<IAsyncEnumerable<T>> QueryWithParamsAsync(string sql,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args);

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
        abstract public Task<IAsyncEnumerable<T>> QueryWithParamsAsync(string sql,
            CancellationToken cancellationToken,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args);

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
        abstract public Task<T> SingleFromQueryWithParamsAsync(string sql,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args);

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
        abstract public Task<T> SingleFromQueryWithParamsAsync(string sql,
            CancellationToken cancellationToken,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args);

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
        abstract public Task<IAsyncEnumerable<T>> QueryFromProcedureAsync(string spName,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args);

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
        abstract public Task<IAsyncEnumerable<T>> QueryFromProcedureAsync(string spName,
            CancellationToken cancellationToken,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args);

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
        abstract public Task<T> SingleFromProcedureAsync(string spName,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args);

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
        abstract public Task<T> SingleFromProcedureAsync(string spName,
            CancellationToken cancellationToken,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args);

        /// <summary>
        /// Get <see cref="IEnumerable{R}"/> of result sets, each of which is itself an <see cref="IEnumerable{T}"/> of items, returned by database command.
        /// </summary>
        /// <param name="command">The command to execute</param>
        /// <param name="connection">The connection to use</param>
        /// <returns></returns>
        abstract public Task<IAsyncEnumerable<IAsyncEnumerable<T>>> QueryMultipleAsync(DbCommand command,
            DbConnection connection = null);

        /// <summary>
        /// Get <see cref="IEnumerable{R}"/> of result sets, each of which is itself an <see cref="IEnumerable{T}"/> of items, returned by database command.
        /// </summary>
        /// <param name="command">The command to execute</param>
        /// <param name="connection">The connection to use</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        abstract public Task<IAsyncEnumerable<IAsyncEnumerable<T>>> QueryMultipleAsync(DbCommand command,
            CancellationToken cancellationToken,
            DbConnection connection = null);

        /// <summary>
        /// Get <see cref="IEnumerable{R}"/> of result sets, each of which is itself an <see cref="IEnumerable{T}"/> of items, returned by SQL query.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="args">Auto-numbered parameter values for SQL</param>
        /// <returns></returns>
        /// <remarks>
        /// 'Easy-calling' version, optional args straight after SQL.
        /// </remarks>
        abstract public Task<IAsyncEnumerable<IAsyncEnumerable<T>>> QueryMultipleAsync(string sql,
            params object[] args);

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
        abstract public Task<IAsyncEnumerable<IAsyncEnumerable<T>>> QueryMultipleAsync(string sql,
            CancellationToken cancellationToken,
            params object[] args);

        /// <summary>
        /// Get <see cref="IEnumerable{R}"/> of result sets, each of which is itself an <see cref="IEnumerable{T}"/> of items, returned by SQL query.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="connection">The connection to use</param>
        /// <param name="args">Auto-numbered parameter values for SQL</param>
        /// <returns></returns>
        abstract public Task<IAsyncEnumerable<IAsyncEnumerable<T>>> QueryMultipleAsync(string sql,
            DbConnection connection,
            params object[] args);

        /// <summary>
        /// Get <see cref="IEnumerable{R}"/> of result sets, each of which is itself an <see cref="IEnumerable{T}"/> of items, returned by SQL query.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="connection">The connection to use</param>
        /// <param name="args">Auto-numbered parameter values for SQL</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        abstract public Task<IAsyncEnumerable<IAsyncEnumerable<T>>> QueryMultipleAsync(string sql,
            DbConnection connection,
            CancellationToken cancellationToken,
            params object[] args);

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
        abstract public Task<IAsyncEnumerable<IAsyncEnumerable<T>>> QueryMultipleWithParamsAsync(string sql,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args);

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
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        abstract public Task<IAsyncEnumerable<IAsyncEnumerable<T>>> QueryMultipleWithParamsAsync(string sql,
            CancellationToken cancellationToken,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args);

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
        abstract public Task<IAsyncEnumerable<IAsyncEnumerable<T>>> QueryMultipleFromProcedureAsync(string spName,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args);

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
        abstract public Task<IAsyncEnumerable<IAsyncEnumerable<T>>> QueryMultipleFromProcedureAsync(string spName,
            CancellationToken cancellationToken,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args);

        /// <summary>
        /// Execute database command.
        /// </summary>
        /// <param name="command">The command to execute</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns>The number of rows affected</returns>
        abstract public Task<int> ExecuteAsync(DbCommand command,
            DbConnection connection = null);

        /// <summary>
        /// Execute database command.
        /// </summary>
        /// <param name="command">The command to execute</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns>The number of rows affected</returns>
        abstract public Task<int> ExecuteAsync(DbCommand command,
            CancellationToken cancellationToken,
            DbConnection connection = null);

        /// <summary>
        /// Execute SQL query.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="args">Auto-numbered parameters for the SQL</param>
        /// <returns>The number of rows affected</returns>
        /// <remarks>
        /// 'Easy-calling' version, optional args straight after SQL.
        /// </remarks>
        abstract public Task<int> ExecuteAsync(string sql,
            params object[] args);

        /// <summary>
        /// Execute SQL query.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="args">Auto-numbered parameters for the SQL</param>
        /// <returns>The number of rows affected</returns>
        /// <remarks>
        /// 'Easy-calling' version, optional args straight after SQL.
        /// </remarks>
        abstract public Task<int> ExecuteAsync(string sql,
            CancellationToken cancellationToken,
            params object[] args);

        /// <summary>
        /// Execute SQL query.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered parameters for the SQL</param>
        /// <returns>The number of rows affected</returns>
        abstract public Task<int> ExecuteAsync(string sql,
            DbConnection connection,
            params object[] args);

        /// <summary>
        /// Execute SQL query.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="args">Auto-numbered parameters for the SQL</param>
        /// <returns>The number of rows affected</returns>
        abstract public Task<int> ExecuteAsync(string sql,
            DbConnection connection,
            CancellationToken cancellationToken,
            params object[] args);

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
        abstract public Task<dynamic> ExecuteWithParamsAsync(string sql,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args);

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
        abstract public Task<dynamic> ExecuteWithParamsAsync(string sql,
            CancellationToken cancellationToken,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args);

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
        abstract public Task<dynamic> ExecuteProcedureAsync(string spName,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args);

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
        abstract public Task<dynamic> ExecuteProcedureAsync(string spName,
            CancellationToken cancellationToken,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args);

        /// <summary>
        /// Return scalar result (value of first or only column from first or only row) from database command.
        /// </summary>
        /// <param name="command">The command to execute</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        abstract public Task<object> ScalarAsync(DbCommand command,
            DbConnection connection = null);

        /// <summary>
        /// Return scalar result (value of first or only column from first or only row) from database command.
        /// </summary>
        /// <param name="command">The command to execute</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        abstract public Task<object> ScalarAsync(DbCommand command,
            CancellationToken cancellationToken,
            DbConnection connection = null);

        /// <summary>
        /// Return scalar result (value of first or only column from first or only row) from SQL query.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        /// <remarks>
        /// 'Easy-calling' version, optional args straight after SQL.
        /// </remarks>
        abstract public Task<object> ScalarAsync(string sql,
            params object[] args);

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
        abstract public Task<object> ScalarAsync(string sql,
            CancellationToken cancellationToken,
            params object[] args);

        /// <summary>
        /// Return scalar result (value of first or only column from first or only row) from SQL query.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        abstract public Task<object> ScalarAsync(string sql,
            DbConnection connection,
            params object[] args);

        /// <summary>
        /// Return scalar result (value of first or only column from first or only row) from SQL query.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        abstract public Task<object> ScalarAsync(string sql,
            DbConnection connection,
            CancellationToken cancellationToken,
            params object[] args);

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
        abstract public Task<object> ScalarWithParamsAsync(string sql,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args);

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
        abstract public Task<object> ScalarWithParamsAsync(string sql,
            CancellationToken cancellationToken,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args);

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
        abstract public Task<object> ScalarFromProcedureAsync(string spName,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args);

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
        abstract public Task<object> ScalarFromProcedureAsync(string spName,
            CancellationToken cancellationToken,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args);

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
        abstract public Task<PagedResults<T>> PagedFromSelectAsync(
            string tableNameOrJoinSpec,
            string orderBy,
            string columns = null,
            string where = null,
            int pageSize = 20, int currentPage = 1,
            DbConnection connection = null,
            params object[] args);

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
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns>The result of the paged query. Result properties are Items, TotalPages, and TotalRecords.</returns>
        /// <remarks>
        /// In this one instance, because of the connection to the underlying logic of these queries, the user
        /// can pass "SELECT columns" instead of columns.
        /// TO DO: Possibly Possibly cancel the above, it makes no sense from a UI pov!
        /// </remarks>
        abstract public Task<PagedResults<T>> PagedFromSelectAsync(
            string tableNameOrJoinSpec,
            string orderBy,
            CancellationToken cancellationToken,
            string columns = null,
            string where = null,
            int pageSize = 20, int currentPage = 1,
            DbConnection connection = null,
            params object[] args);


        /// <summary>
        /// Yield return values for single or multiple resultsets.
        /// </summary>
        /// <typeparam name="X">Use with <typeparamref name="T"/> for single or <see cref="IEnumerable{T}"/> for multiple</typeparam>
        /// <param name="sql">The command SQL</param>
        /// <param name="inParams">Named input parameters</param>
        /// <param name="outParams">Named output parameters</param>
        /// <param name="ioParams">Named input-output parameters</param>
        /// <param name="returnParams">Named return parameters</param>
        /// <param name="isProcedure">Is the SQL a stored procedure name (with optional argument spec) only?</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="behavior">The command behaviour</param>
        /// <param name="connection">Optional conneciton to use</param>
        /// <param name="args">Auto-numbered parameters for the SQL</param>
        /// <returns></returns>
        abstract protected Task<IAsyncEnumerable<X>> QueryNWithParamsAsync<X>(string sql = null, object inParams = null, object outParams = null, object ioParams = null, object returnParams = null, bool isProcedure = false, CommandBehavior behavior = CommandBehavior.Default, DbConnection connection = null, CancellationToken cancellationToken = default, params object[] args);

        /// <summary>
        /// Yield return values for single or multiple resultsets.
        /// </summary>
        /// <typeparam name="X">Use with <typeparamref name="T"/> for single or <see cref="IEnumerable{T}"/> for multiple</typeparam>
        /// <param name="command">The command to execute</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="behavior">The command behaviour</param>
        /// <param name="connection">Optional conneciton to use</param>
        /// <param name="outerReader">The outer reader when this is a call to the inner reader in QueryMultiple</param>
        /// <returns></returns>
        abstract protected Task<IAsyncEnumerable<X>> QueryNWithParamsAsync<X>(DbCommand command, CancellationToken cancellationToken = default, CommandBehavior behavior = CommandBehavior.Default, DbConnection connection = null, DbDataReader outerReader = null);
        #endregion

        #region Table specific methods
        /// <summary>
        /// Perform COUNT on current table.
        /// </summary>
        /// <param name="where">WHERE clause</param>
        /// <param name="columns">Columns (defaults to *, but can be specified, e.g., to count non-nulls in a given field)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        abstract public Task<object> CountAsync(
            string where = null,
            string columns = "*",
            DbConnection connection = null,
            params object[] args);

        /// <summary>
        /// Perform COUNT on current table.
        /// </summary>
        /// <param name="where">WHERE clause</param>
        /// <param name="columns">Columns (defaults to *, but can be specified, e.g., to count non-nulls in a given field)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <param name="cancellationToken">See <see cref="CancellationToken"/></param>
        /// <returns></returns>
        abstract public Task<object> CountAsync(
            CancellationToken cancellationToken,
            string where = null,
            string columns = "*",
            DbConnection connection = null,
            params object[] args);

        /// <summary>
        /// Perform COUNT on current table.
        /// </summary>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="columns">Columns (defaults to *, but can be specified, e.g., to count non-nulls in a given field)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        abstract public Task<object> CountAsync(
            object whereParams = null,
            string columns = "*",
            DbConnection connection = null);

        /// <summary>
        /// Perform COUNT on current table.
        /// </summary>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="columns">Columns (defaults to *, but can be specified, e.g., to count non-nulls in a given field)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="cancellationToken">See <see cref="CancellationToken"/></param>
        /// <returns></returns>
        abstract public Task<object> CountAsync(
            CancellationToken cancellationToken,
            object whereParams = null,
            string columns = "*",
            DbConnection connection = null);

        /// <summary>
        /// Get MAX of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        abstract public Task<object> MaxAsync(
            string columns,
            string where = null,
            DbConnection connection = null,
            params object[] args);

        /// <summary>
        /// Get MAX of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <param name="cancellationToken">See <see cref="CancellationToken"/></param>
        /// <returns></returns>
        abstract public Task<object> MaxAsync(
            string columns,
            CancellationToken cancellationToken,
            string where = null,
            DbConnection connection = null,
            params object[] args);

        /// <summary>
        /// Get MAX of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        abstract public Task<object> MaxAsync(
            string columns,
            object whereParams = null,
            DbConnection connection = null);

        /// <summary>
        /// Get MAX of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="cancellationToken">See <see cref="CancellationToken"/></param>
        /// <returns></returns>
        abstract public Task<object> MaxAsync(
            string columns,
            CancellationToken cancellationToken,
            object whereParams = null,
            DbConnection connection = null);

        /// <summary>
        /// Get MIN of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        abstract public Task<object> MinAsync(
            string columns,
            string where = null,
            DbConnection connection = null,
            params object[] args);

        /// <summary>
        /// Get MIN of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <param name="cancellationToken">See <see cref="CancellationToken"/></param>
        /// <returns></returns>
        abstract public Task<object> MinAsync(
            string columns,
            CancellationToken cancellationToken,
            string where = null,
            DbConnection connection = null,
            params object[] args);

        /// <summary>
        /// Get MIN of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        abstract public Task<object> MinAsync(
            string columns,
            object whereParams = null,
            DbConnection connection = null);

        /// <summary>
        /// Get MIN of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="cancellationToken">See <see cref="CancellationToken"/></param>
        /// <returns></returns>
        abstract public Task<object> MinAsync(
            string columns,
            CancellationToken cancellationToken,
            object whereParams = null,
            DbConnection connection = null);

        /// <summary>
        /// Get SUM of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        abstract public Task<object> SumAsync(
            string columns,
            string where = null,
            DbConnection connection = null,
            params object[] args);

        /// <summary>
        /// Get SUM of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <param name="cancellationToken">See <see cref="CancellationToken"/></param>
        /// <returns></returns>
        abstract public Task<object> SumAsync(
            string columns,
            CancellationToken cancellationToken,
            string where = null,
            DbConnection connection = null,
            params object[] args);

        /// <summary>
        /// Get SUM of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        abstract public Task<object> SumAsync(
            string columns,
            object whereParams = null,
            DbConnection connection = null);

        /// <summary>
        /// Get SUM of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="cancellationToken">See <see cref="CancellationToken"/></param>
        /// <returns></returns>
        abstract public Task<object> SumAsync(
            string columns,
            CancellationToken cancellationToken,
            object whereParams = null,
            DbConnection connection = null);

        /// <summary>
        /// Get AVG of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        abstract public Task<object> AvgAsync(
            string columns,
            string where = null,
            DbConnection connection = null,
            params object[] args);

        /// <summary>
        /// Get AVG of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <param name="cancellationToken">See <see cref="CancellationToken"/></param>
        /// <returns></returns>
        abstract public Task<object> AvgAsync(
            string columns,
            CancellationToken cancellationToken,
            string where = null,
            DbConnection connection = null,
            params object[] args);

        /// <summary>
        /// Get AVG of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        abstract public Task<object> AvgAsync(
            string columns,
            object whereParams = null,
            DbConnection connection = null);

        /// <summary>
        /// Get AVG of column on current table.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="cancellationToken">See <see cref="CancellationToken"/></param>
        /// <returns></returns>
        abstract public Task<object> AvgAsync(
            string columns,
            CancellationToken cancellationToken,
            object whereParams = null,
            DbConnection connection = null);

        /// <summary>
        /// Perform aggregate operation on the current table (use for SUM, MAX, MIN, AVG, etc.)
        /// </summary>
        /// <param name="function">Aggregate function</param>
        /// <param name="columns">Columns for aggregate function</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        abstract public Task<object> AggregateAsync(string function, string columns, string where = null,
            DbConnection connection = null,
            params object[] args);

        /// <summary>
        /// Perform aggregate operation on the current table (use for SUM, MAX, MIN, AVG, etc.)
        /// </summary>
        /// <param name="function">Aggregate function</param>
        /// <param name="columns">Columns for aggregate function</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <param name="cancellationToken">See <see cref="CancellationToken"/></param>
        /// <returns></returns>
        abstract public Task<object> AggregateAsync(string function, string columns, CancellationToken cancellationToken, string where = null,
            DbConnection connection = null,
            params object[] args);

        /// <summary>
        /// Perform aggregate operation on the current table (use for SUM, MAX, MIN, AVG, etc.)
        /// </summary>
        /// <param name="function">Aggregate function</param>
        /// <param name="columns">Columns for aggregate function</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        abstract public Task<object> AggregateAsync(string function, string columns, object whereParams = null,
            DbConnection connection = null);

        /// <summary>
        /// Perform aggregate operation on the current table (use for SUM, MAX, MIN, AVG, etc.)
        /// </summary>
        /// <param name="function">Aggregate function</param>
        /// <param name="columns">Columns for aggregate function</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="cancellationToken">See <see cref="CancellationToken"/></param>
        /// <returns></returns>
        abstract public Task<object> AggregateAsync(string function, string columns, CancellationToken cancellationToken, object whereParams = null,
            DbConnection connection = null);

        /// <summary>
        /// Perform aggregate operation on the current table (use for SUM, MAX, MIN, AVG, etc.), with support for named params.
        /// </summary>
        /// <param name="function">Aggregate function</param>
        /// <param name="columns">Columns for aggregate function</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="inParams">Named input parameters</param>
        /// <param name="outParams">Named output parameters</param>
        /// <param name="ioParams">Named input-output parameters</param>
        /// <param name="returnParams">Named return parameters</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        abstract public Task<object> AggregateWithParamsAsync(string function, string columns = "*", string where = null,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args);

        /// <summary>
        /// Perform aggregate operation on the current table (use for SUM, MAX, MIN, AVG, etc.), with support for named params.
        /// </summary>
        /// <param name="function">Aggregate function</param>
        /// <param name="columns">Columns for aggregate function</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="inParams">Named input parameters</param>
        /// <param name="outParams">Named output parameters</param>
        /// <param name="ioParams">Named input-output parameters</param>
        /// <param name="returnParams">Named return parameters</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        abstract public Task<object> AggregateWithParamsAsync(string function, string columns, CancellationToken cancellationToken, string where = null,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args);

        /// <summary>
        /// Get single item from the current table using primary key or name-value where specification.
        /// </summary>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="columns">List of columns to return</param>
        /// <param name="connection">Optional connection to use</param>
        /// <returns></returns>
        abstract public Task<T> SingleAsync(object whereParams, string columns = null,
            DbConnection connection = null);

        /// <summary>
        /// Get single item from the current table using primary key or name-value where specification.
        /// </summary>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="columns">List of columns to return</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        abstract public Task<T> SingleAsync(object whereParams, CancellationToken cancellationToken, string columns = null,
            DbConnection connection = null);

        /// <summary>
        /// Get single item from the current table using WHERE specification.
        /// </summary>
        /// <param name="where">WHERE clause</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        /// <remarks>
        /// 'Easy-calling' version, optional args straight after where.
        /// </remarks>
        abstract public Task<T> SingleAsync(string where,
            params object[] args);

        /// <summary>
        /// Get single item from the current table using WHERE specification.
        /// </summary>
        /// <param name="where">WHERE clause</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        /// <remarks>
        /// 'Easy-calling' version, optional args straight after where.
        /// </remarks>
        abstract public Task<T> SingleAsync(string where,
            CancellationToken cancellationToken,
            params object[] args);

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
        abstract public Task<T> SingleAsync(string where,
            DbConnection connection = null,
            string orderBy = null,
            string columns = null,
            params object[] args);

        /// <summary>
        /// Get single item from the current table using WHERE specification.
        /// </summary>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="orderBy">ORDER BY clause</param>
        /// <param name="columns">Comma separated list of columns to return or "*"</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        /// <remarks>
        /// DbConnection coming early (not just before args) in this one case is really useful, as it avoids ambiguity between
        /// the `columns` and `orderBy` strings and optional string args.
        /// </remarks>
        abstract public Task<T> SingleAsync(string where,
            CancellationToken cancellationToken,
            DbConnection connection = null,
            string orderBy = null,
            string columns = null,
            params object[] args);

        /// <summary>
        /// Get single item from the current table using WHERE specification with support for named parameters.
        /// </summary>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="orderBy">ORDER BY clause</param>
        /// <param name="columns">Columns to return</param>
        /// <param name="inParams">Named input parameters</param>
        /// <param name="outParams">Named output parameters</param>
        /// <param name="ioParams">Named input-output parameters</param>
        /// <param name="returnParams">Named return parameters</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        abstract public Task<T> SingleWithParamsAsync(string where, string orderBy = null, string columns = null,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args);

        /// <summary>
        /// Get single item from the current table using WHERE specification with support for named parameters.
        /// </summary>
        /// <param name="where">WHERE clause</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="orderBy">ORDER BY clause</param>
        /// <param name="columns">Columns to return</param>
        /// <param name="inParams">Named input parameters</param>
        /// <param name="outParams">Named output parameters</param>
        /// <param name="ioParams">Named input-output parameters</param>
        /// <param name="returnParams">Named return parameters</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        abstract public Task<T> SingleWithParamsAsync(string where, CancellationToken cancellationToken, string orderBy = null, string columns = null,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args);

        /// <summary>
        /// Get <see cref="IEnumerable{T}"/> of items from the current table with WHERE and TOP/LIMIT specification.
        /// </summary>
        /// <param name="where">WHERE clause</param>
        /// <param name="orderBy">ORDER BY clause</param>
        /// <param name="columns">Columns to return</param>
        /// <param name="limit">Maximum number of items to return</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        abstract public Task<IAsyncEnumerable<T>> AllAsync(
            string where = null, string orderBy = null, string columns = null, int limit = 0,
            params object[] args);

        /// <summary>
        /// Get <see cref="IEnumerable{T}"/> of items from the current table with WHERE and TOP/LIMIT specification.
        /// </summary>
        /// <param name="connection">The connection to use</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="orderBy">ORDER BY clause</param>
        /// <param name="columns">Columns to return</param>
        /// <param name="limit">Maximum number of items to return</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        abstract public Task<IAsyncEnumerable<T>> AllAsync(
            DbConnection connection,
            string where = null, string orderBy = null, string columns = null, int limit = 0,
            params object[] args);

        /// <summary>
        /// Get <see cref="IEnumerable{T}"/> of items from the current table with WHERE and TOP/LIMIT specification.
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="where">WHERE clause</param>
        /// <param name="orderBy">ORDER BY clause</param>
        /// <param name="columns">Columns to return</param>
        /// <param name="limit">Maximum number of items to return</param>
        /// <param name="args">Auto-numbered input parameters</param>.
        /// <returns></returns>
        abstract public Task<IAsyncEnumerable<T>> AllAsync(
            CancellationToken cancellationToken,
            string where = null, string orderBy = null, string columns = null, int limit = 0,
            params object[] args);

        /// <summary>
        /// Get <see cref="IEnumerable{T}"/> of items from the current table with WHERE and TOP/LIMIT specification.
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="connection">The connection to use</param>
        /// <param name="where">WHERE clause</param>
        /// <param name="orderBy">ORDER BY clause</param>
        /// <param name="columns">Columns to return</param>
        /// <param name="limit">Maximum number of items to return</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        abstract public Task<IAsyncEnumerable<T>> AllAsync(
            CancellationToken cancellationToken,
            DbConnection connection,
            string where = null, string orderBy = null, string columns = null, int limit = 0,
            params object[] args);

        /// <summary>
        /// Get <see cref="IEnumerable{T}"/> of items from the current table with primary key or name-value where specification and TOP/LIMIT specification.
        /// </summary>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="orderBy">ORDER BY clause</param>
        /// <param name="columns">Columns to return</param>
        /// <param name="limit">Maximum number of items to return</param>
        /// <returns></returns>
        abstract public Task<IAsyncEnumerable<T>> AllAsync(
            object whereParams = null, string orderBy = null, string columns = null, int limit = 0);

        /// <summary>
        /// Get <see cref="IEnumerable{T}"/> of items from the current table with primary key or name-value where specification and TOP/LIMIT specification.
        /// </summary>
        /// <param name="connection">The connection to use</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="orderBy">ORDER BY clause</param>
        /// <param name="columns">Columns to return</param>
        /// <param name="limit">Maximum number of items to return</param>
        /// <returns></returns>
        abstract public Task<IAsyncEnumerable<T>> AllAsync(
            DbConnection connection,
            object whereParams = null, string orderBy = null, string columns = null, int limit = 0);

        /// <summary>
        /// Get <see cref="IEnumerable{T}"/> of items from the current table with primary key or name-value where specification and TOP/LIMIT specification.
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="orderBy">ORDER BY clause</param>
        /// <param name="columns">Columns to return</param>
        /// <param name="limit">Maximum number of items to return</param>
        /// <returns></returns>
        abstract public Task<IAsyncEnumerable<T>> AllAsync(
            CancellationToken cancellationToken,
            object whereParams = null, string orderBy = null, string columns = null, int limit = 0);

        /// <summary>
        /// Get <see cref="IEnumerable{T}"/> of items from the current table with primary key or name-value where specification and TOP/LIMIT specification.
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="connection">The connection to use</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="orderBy">ORDER BY clause</param>
        /// <param name="columns">Columns to return</param>
        /// <param name="limit">Maximum number of items to return</param>
        /// <returns></returns>
        abstract public Task<IAsyncEnumerable<T>> AllAsync(
            CancellationToken cancellationToken,
            DbConnection connection,
            object whereParams = null, string orderBy = null, string columns = null, int limit = 0);

        /// <summary>
        /// Get <see cref="IEnumerable{T}"/> of items from the current table with WHERE and TOP/LIMIT specification and support for named parameters.
        /// </summary>
        /// <param name="where">WHERE clause</param>
        /// <param name="orderBy">ORDER BY clause</param>
        /// <param name="columns">Columns to return</param>
        /// <param name="limit">Maximum number of items to return</param>
        /// <param name="inParams">Named input parameters</param>
        /// <param name="outParams">Named output parameters</param>
        /// <param name="ioParams">Named input-output parameters</param>
        /// <param name="returnParams">Named return parameters</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns></returns>
        abstract public Task<IAsyncEnumerable<T>> AllWithParamsAsync(
            string where = null, string orderBy = null, string columns = null, int limit = 0,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args);

        /// <summary>
        /// Get <see cref="IEnumerable{T}"/> of items from the current table with WHERE and TOP/LIMIT specification and support for named parameters.
        /// </summary>
        /// <param name="where">WHERE clause</param>
        /// <param name="orderBy">ORDER BY clause</param>
        /// <param name="columns">Columns to return</param>
        /// <param name="limit">Maximum number of items to return</param>
        /// <param name="inParams">Named input parameters</param>
        /// <param name="outParams">Named output parameters</param>
        /// <param name="ioParams">Named input-output parameters</param>
        /// <param name="returnParams">Named return parameters</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        abstract public Task<IAsyncEnumerable<T>> AllWithParamsAsync(
            CancellationToken cancellationToken,
            string where = null, string orderBy = null, string columns = null, int limit = 0,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
            DbConnection connection = null,
            params object[] args);

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
        /// `columns` parameter is not placed first because it's an override to something we may have alread provided in the constructor
        /// (so we don't want the user to have to non-fluently re-type it, or else type null, every time).
        /// </remarks>
        abstract public Task<PagedResults<T>> PagedAsync(
            string orderBy = null,
            string columns = null,
            string where = null,
            int pageSize = 20, int currentPage = 1,
            DbConnection connection = null,
            params object[] args);

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
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns>The result of the paged query. Result properties are Items, TotalPages, and TotalRecords.</returns>
        /// <remarks>
        /// `columns` parameter is not placed first because it's an override to something we may have alread provided in the constructor
        /// (so we don't want the user to have to non-fluently re-type it, or else type null, every time).
        /// </remarks>
        abstract public Task<PagedResults<T>> PagedAsync(CancellationToken cancellationToken, string orderBy = null,
            string columns = null,
            string where = null,
            int pageSize = 20, int currentPage = 1,
            DbConnection connection = null,
            params object[] args);

        /// <summary>
        /// Save one or more items.
        /// 'Save' means
        /// objects with missing or default primary keys are inserted
        /// and objects with non-default primary keys are updated.
        /// </summary>
        /// <param name="items">The items</param>
        /// <returns></returns>
        abstract public Task<int> SaveAsync(params object[] items);

        /// <summary>
        /// Save one or more items.
        /// 'Save' means
        /// objects with missing or default primary keys are inserted
        /// and objects with non-default primary keys are updated.
        /// </summary>
        /// <param name="items">The items</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        abstract public Task<int> SaveAsync(CancellationToken cancellationToken, params object[] items);

        /// <summary>
        /// Save one or more items.
        /// 'Save' means
        /// objects with missing or default primary keys are inserted
        /// and objects with non-default primary keys are updated.
        /// </summary>
        /// <param name="connection">The connection to use</param>
        /// <param name="items">The items</param>
        /// <returns></returns>
        abstract public Task<int> SaveAsync(DbConnection connection, params object[] items);

        /// <summary>
        /// Save one or more items.
        /// 'Save' means
        /// objects with missing or default primary keys are inserted
        /// and objects with non-default primary keys are updated.
        /// </summary>
        /// <param name="connection">The connection to use</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="items">The items</param>
        /// <returns></returns>
        abstract public Task<int> SaveAsync(DbConnection connection, CancellationToken cancellationToken, params object[] items);

        /// <summary>
        /// Save array or other <see cref="IEnumerable"/> of items.
        /// 'Save' means
        /// objects with missing or default primary keys are inserted
        /// and objects with non-default primary keys are updated.
        /// </summary>
        /// <param name="items">The items</param>
        /// <returns></returns>
        abstract public Task<int> SaveAsync(IEnumerable<object> items);

        /// <summary>
        /// Save array or other <see cref="IEnumerable"/> of items.
        /// 'Save' means
        /// objects with missing or default primary keys are inserted
        /// and objects with non-default primary keys are updated.
        /// </summary>
        /// <param name="items">The items</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        abstract public Task<int> SaveAsync(IEnumerable<object> items, CancellationToken cancellationToken);

        /// <summary>
        /// Save array or other <see cref="IEnumerable"/> of items.
        /// 'Save' means
        /// objects with missing or default primary keys are inserted
        /// and objects with non-default primary keys are updated.
        /// </summary>
        /// <param name="connection">The connection to use</param>
        /// <param name="items">The items</param>
        /// <returns></returns>
        abstract public Task<int> SaveAsync(DbConnection connection, IEnumerable<object> items);

        /// <summary>
        /// Save array or other <see cref="IEnumerable"/> of items.
        /// 'Save' means
        /// objects with missing or default primary keys are inserted
        /// and objects with non-default primary keys are updated.
        /// </summary>
        /// <param name="connection">The connection to use</param>
        /// <param name="items">The items</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        abstract public Task<int> SaveAsync(DbConnection connection, IEnumerable<object> items, CancellationToken cancellationToken);

        /// <summary>
        /// Insert single item.
        /// Call <see cref="New"/> before insert if you need to pre-populate your inserted items with any defined database column defaults.
        /// </summary>
        /// <param name="item">The item to insert, in any reasonable format (for MightyOrm&lt;T&gt; this includes, but is not limited to, in instance of type T)</param>
        /// <returns>The item sent in but with the primary key populated</returns>
        abstract public Task<T> InsertAsync(object item);

        /// <summary>
        /// Insert single item.
        /// Call <see cref="New"/> before insert if you need to pre-populate your inserted items with any defined database column defaults.
        /// </summary>
        /// <param name="item">The item to insert, in any reasonable format (for MightyOrm&lt;T&gt; this includes, but is not limited to, in instance of type T)</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns>The item sent in but with the primary key populated</returns>
        abstract public Task<T> InsertAsync(object item, CancellationToken cancellationToken);

        /// <summary>
        /// Insert single item.
        /// Call <see cref="New"/> before insert if you need to pre-populate your inserted items with any defined database column defaults.
        /// </summary>
        /// <param name="connection">The connection to use</param>
        /// <param name="item">The item to insert, in any reasonable format (for MightyOrm&lt;T&gt; this includes, but is not limited to, in instance of type T)</param>
        /// <returns>The item sent in but with the primary key populated</returns>
        abstract public Task<T> InsertAsync(object item, DbConnection connection);

        /// <summary>
        /// Insert single item.
        /// Call <see cref="New"/> before insert if you need to pre-populate your inserted items with any defined database column defaults.
        /// </summary>
        /// <param name="connection">The connection to use</param>
        /// <param name="item">The item to insert, in any reasonable format (for MightyOrm&lt;T&gt; this includes, but is not limited to, in instance of type T)</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns>The item sent in but with the primary key populated</returns>
        abstract public Task<T> InsertAsync(object item, DbConnection connection, CancellationToken cancellationToken);

        /// <summary>
        /// Insert one or more items.
        /// Call <see cref="New"/> before insert if you need to pre-populate your inserted items with any defined database column defaults.
        /// </summary>
        /// <param name="items">The items</param>
        /// <returns>The items sent in but with the primary keys populated</returns>
        abstract public Task<IEnumerable<T>> InsertAsync(params object[] items);

        /// <summary>
        /// Insert one or more items.
        /// Call <see cref="New"/> before insert if you need to pre-populate your inserted items with any defined database column defaults.
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="items">The items</param>
        /// <returns>The items sent in but with the primary keys populated</returns>
        abstract public Task<IEnumerable<T>> InsertAsync(CancellationToken cancellationToken, params object[] items);

        /// <summary>
        /// Insert one or more items.
        /// Call <see cref="New"/> before insert if you need to pre-populate your inserted items with any defined database column defaults.
        /// </summary>
        /// <param name="connection">The connection to use</param>
        /// <param name="items">The items</param>
        /// <returns>The items sent in but with the primary keys populated</returns>
        abstract public Task<IEnumerable<T>> InsertAsync(DbConnection connection, params object[] items);

        /// <summary>
        /// Insert one or more items.
        /// Call <see cref="New"/> before insert if you need to pre-populate your inserted items with any defined database column defaults.
        /// </summary>
        /// <param name="connection">The connection to use</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="items">The items</param>
        /// <returns>The items sent in but with the primary keys populated</returns>
        abstract public Task<IEnumerable<T>> InsertAsync(DbConnection connection, CancellationToken cancellationToken, params object[] items);

        /// <summary>
        /// Insert array or other <see cref="IEnumerable"/> of items.
        /// Call <see cref="New"/> before insert if you need to pre-populate your inserted items with any defined database column defaults.
        /// </summary>
        /// <param name="items">The items</param>
        /// <returns>The items sent in but with the primary keys populated</returns>
        abstract public Task<IEnumerable<T>> InsertAsync(IEnumerable<object> items);

        /// <summary>
        /// Insert array or other <see cref="IEnumerable"/> of items.
        /// Call <see cref="New"/> before insert if you need to pre-populate your inserted items with any defined database column defaults.
        /// </summary>
        /// <param name="items">The items</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns>The items sent in but with the primary keys populated</returns>
        abstract public Task<IEnumerable<T>> InsertAsync(IEnumerable<object> items, CancellationToken cancellationToken);

        /// <summary>
        /// Insert array or other <see cref="IEnumerable"/> of items.
        /// Call <see cref="New"/> before insert if you need to pre-populate your inserted items with any defined database column defaults.
        /// </summary>
        /// <param name="connection">The connection to use</param>
        /// <param name="items">The items</param>
        /// <returns>The items sent in but with the primary keys populated</returns>
        abstract public Task<IEnumerable<T>> InsertAsync(DbConnection connection, IEnumerable<object> items);

        /// <summary>
        /// Insert array or other <see cref="IEnumerable"/> of items.
        /// Call <see cref="New"/> before insert if you need to pre-populate your inserted items with any defined database column defaults.
        /// </summary>
        /// <param name="connection">The connection to use</param>
        /// <param name="items">The items</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns>The items sent in but with the primary keys populated</returns>
        abstract public Task<IEnumerable<T>> InsertAsync(DbConnection connection, IEnumerable<object> items, CancellationToken cancellationToken);

        /// <summary>
        /// Update single item.
        /// </summary>
        /// <param name="item">The item</param>
        /// <returns></returns>
        abstract public Task<int> UpdateAsync(object item);

        /// <summary>
        /// Update single item.
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="item">The item</param>
        /// <returns></returns>
        abstract public Task<int> UpdateAsync(object item, CancellationToken cancellationToken);

        /// <summary>
        /// Update single item.
        /// </summary>
        /// <param name="connection">The connection to use</param>
        /// <param name="item">The item</param>
        /// <returns></returns>
        abstract public Task<int> UpdateAsync(object item, DbConnection connection);

        /// <summary>
        /// Update single item.
        /// </summary>
        /// <param name="connection">The connection to use</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="item">The item</param>
        /// <returns></returns>
        abstract public Task<int> UpdateAsync(object item, DbConnection connection, CancellationToken cancellationToken);

        /// <summary>
        /// Update one or more items.
        /// </summary>
        /// <param name="items">The items</param>
        /// <returns></returns>
        abstract public Task<int> UpdateAsync(params object[] items);

        /// <summary>
        /// Update one or more items.
        /// </summary>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="items">The items</param>
        /// <returns></returns>
        abstract public Task<int> UpdateAsync(CancellationToken cancellationToken, params object[] items);

        /// <summary>
        /// Update one or more items.
        /// </summary>
        /// <param name="connection">The connection to use</param>
        /// <param name="items">The items</param>
        /// <returns></returns>
        abstract public Task<int> UpdateAsync(DbConnection connection, params object[] items);

        /// <summary>
        /// Update one or more items.
        /// </summary>
        /// <param name="connection">The connection to use</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <param name="items">The items</param>
        /// <returns></returns>
        abstract public Task<int> UpdateAsync(DbConnection connection, CancellationToken cancellationToken, params object[] items);

        /// <summary>
        /// Update array or other <see cref="IEnumerable"/> of items.
        /// </summary>
        /// <param name="items">The items</param>
        /// <returns></returns>
        abstract public Task<int> UpdateAsync(IEnumerable<object> items);

        /// <summary>
        /// Update array or other <see cref="IEnumerable"/> of items.
        /// </summary>
        /// <param name="items">The items</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        abstract public Task<int> UpdateAsync(IEnumerable<object> items, CancellationToken cancellationToken);

        /// <summary>
        /// Update array or other <see cref="IEnumerable"/> of items.
        /// </summary>
        /// <param name="connection">The connection to use</param>
        /// <param name="items">The items</param>
        /// <returns></returns>
        abstract public Task<int> UpdateAsync(DbConnection connection, IEnumerable<object> items);

        /// <summary>
        /// Update array or other <see cref="IEnumerable"/> of items.
        /// </summary>
        /// <param name="connection">The connection to use</param>
        /// <param name="items">The items</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        abstract public Task<int> UpdateAsync(DbConnection connection, IEnumerable<object> items, CancellationToken cancellationToken);

        /// <summary>
        /// Delete one or more items.
        /// Each argument may be (or contain) a value (or values) only, in which case
        /// it specifies the primary key value(s) of the item to delete, or it can be any object containing name-values pairs in which case
        /// it should contain fields with names matching the primary key(s) whose values will specify the item to delete (but it may contain
        /// other fields as well which will be ignored here).
        /// </summary>
        /// <param name="items">The items</param>
        /// <returns>The number of items affected</returns>
        abstract public Task<int> DeleteAsync(params object[] items);

        /// <summary>
        /// Delete one or more items.
        /// Delete an array or other <see cref="IEnumerable"/> of items.
        /// Each argument may be (or contain) a value (or values) only, in which case
        /// it specifies the primary key value(s) of the item to delete, or it can be any object containing name-values pairs in which case
        /// it should contain fields with names matching the primary key(s) whose values will specify the item to delete (but it may contain
        /// other fields as well which will be ignored here).
        /// </summary>
        /// <param name="items">The items</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns>The number of items affected</returns>
        abstract public Task<int> DeleteAsync(CancellationToken cancellationToken, params object[] items);

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
        abstract public Task<int> DeleteAsync(DbConnection connection, params object[] items);

        /// <summary>
        /// Delete one or more items.
        /// Each argument may be (or contain) a value (or values) only, in which case
        /// it specifies the primary key value(s) of the item to delete, or it can be any object containing name-values pairs in which case
        /// it should contain fields with names matching the primary key(s) whose values will specify the item to delete (but it may contain
        /// other fields as well which will be ignored here).
        /// </summary>
        /// <param name="items">The items</param>
        /// <param name="connection">The connection to use</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns>The number of items affected</returns>
        abstract public Task<int> DeleteAsync(DbConnection connection, CancellationToken cancellationToken, params object[] items);

        /// <summary>
        /// Delete an array or other <see cref="IEnumerable"/> of items.
        /// Each argument may be (or contain) a value (or values) only, in which case
        /// it specifies the primary key value(s) of the item to delete, or it can be any object containing name-values pairs in which case
        /// it should contain fields with names matching the primary key(s) whose values will specify the item to delete (but it may contain
        /// other fields as well which will be ignored here).
        /// </summary>
        /// <param name="items">The items</param>
        /// <returns>The number of items affected</returns>
        abstract public Task<int> DeleteAsync(IEnumerable<object> items);

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
        abstract public Task<int> DeleteAsync(IEnumerable<object> items, CancellationToken cancellationToken);

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
        abstract public Task<int> DeleteAsync(DbConnection connection, IEnumerable<object> items);

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
        abstract public Task<int> DeleteAsync(DbConnection connection, IEnumerable<object> items, CancellationToken cancellationToken);

        /// <summary>
        /// Update the row(s) specified by the primary key(s) or WHERE values sent in using the values from the item sent in.
        /// If `keys` has been specified on the current Mighty instance then any primary key fields in the item are ignored.
        /// The item is not filtered to remove fields not in the table, if you need that you can call <see cref="New"/> with first parameter `partialItem` and second parameter `false` first.
        /// </summary>
        /// <param name="partialItem">Item containing values to update with</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        abstract public Task<int> UpdateUsingAsync(object partialItem, object whereParams);

        /// <summary>
        /// Update the row(s) specified by the primary key(s) or WHERE values sent in using the values from the item sent in.
        /// If `keys` has been specified on the current Mighty instance then any primary key fields in the item are ignored.
        /// The item is not filtered to remove fields not in the table, if you need that you can call <see cref="New"/> with first parameter `partialItem` and second parameter `false` first.
        /// </summary>
        /// <param name="partialItem">Item containing values to update with</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        abstract public Task<int> UpdateUsingAsync(object partialItem, object whereParams, CancellationToken cancellationToken);

        /// <summary>
        /// Update the row(s) specified by the primary key(s) or WHERE values sent in using the values from the item sent in.
        /// If `keys` has been specified on the current Mighty instance then any primary key fields in the item are ignored.
        /// The item is not filtered to remove fields not in the table, if you need that you can call <see cref="New"/> with first parameter `partialItem` and second parameter `false` first.
        /// </summary>
        /// <param name="partialItem">Item containing values to update with</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
        abstract public Task<int> UpdateUsingAsync(object partialItem, object whereParams,
            DbConnection connection);

        /// <summary>
        /// Update the row(s) specified by the primary key(s) or WHERE values sent in using the values from the item sent in.
        /// If `keys` has been specified on the current Mighty instance then any primary key fields in the item are ignored.
        /// The item is not filtered to remove fields not in the table, if you need that you can call <see cref="New"/> with first parameter `partialItem` and second parameter `false` first.
        /// </summary>
        /// <param name="partialItem">Item containing values to update with</param>
        /// <param name="whereParams">Value(s) to be mapped to the table's primary key(s), or object containing named value(s) to be mapped to the matching named column(s)</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        abstract public Task<int> UpdateUsingAsync(object partialItem, object whereParams,
            DbConnection connection, CancellationToken cancellationToken);

        /// <summary>
        /// Update all items matching WHERE clause using fields from the item sent in.
        /// If `keys` has been specified on the current Mighty instance then any primary key fields in the item are ignored.
        /// The item is not filtered to remove fields not in the table, if you need that you can call <see cref="New"/> with first parameter `partialItem` and second parameter `false` first.
        /// </summary>
        /// <param name="partialItem">Item containing values to update with</param>
        /// <param name="where">WHERE clause specifying which rows to update</param>
        /// <param name="args">Auto-numbered input parameters</param>
        abstract public Task<int> UpdateUsingAsync(object partialItem, string where,
            params object[] args);

        /// <summary>
        /// Update all items matching WHERE clause using fields from the item sent in.
        /// If `keys` has been specified on the current Mighty instance then any primary key fields in the item are ignored.
        /// The item is not filtered to remove fields not in the table, if you need that you can call <see cref="New"/> with first parameter `partialItem` and second parameter `false` first.
        /// </summary>
        /// <param name="partialItem">Item containing values to update with</param>
        /// <param name="where">WHERE clause specifying which rows to update</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        abstract public Task<int> UpdateUsingAsync(object partialItem, string where,
            CancellationToken cancellationToken,
            params object[] args);

        /// <summary>
        /// Update all items matching WHERE clause using fields from the item sent in.
        /// If `keys` has been specified on the current Mighty instance then any primary key fields in the item are ignored.
        /// The item is not filtered to remove fields not in the table, if you need that you can call <see cref="New"/> with first parameter `partialItem` and second parameter `false` first.
        /// </summary>
        /// <param name="partialItem">Item containing values to update with</param>
        /// <param name="where">WHERE clause specifying which rows to update</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        abstract public Task<int> UpdateUsingAsync(object partialItem, string where,
            DbConnection connection,
            params object[] args);

        /// <summary>
        /// Update all items matching WHERE clause using fields from the item sent in.
        /// If `keys` has been specified on the current Mighty instance then any primary key fields in the item are ignored.
        /// The item is not filtered to remove fields not in the table, if you need that you can call <see cref="New"/> with first parameter `partialItem` and second parameter `false` first.
        /// </summary>
        /// <param name="partialItem">Item containing values to update with</param>
        /// <param name="where">WHERE clause specifying which rows to update</param>
        /// <param name="connection">Optional connection to use</param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        abstract public Task<int> UpdateUsingAsync(object partialItem, string where,
            DbConnection connection,
            CancellationToken cancellationToken,
            params object[] args);

        /// <summary>
        /// Delete one or more items based on a WHERE clause.
        /// </summary>
        /// <param name="where">
        /// Non-optional WHERE clause.
        /// Specify "1=1" if you are sure that you want to delete all rows.
        /// </param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <returns>The number of items affected</returns>
        abstract public Task<int> DeleteAsync(string where,
            params object[] args);

        /// <summary>
        /// Delete one or more items based on a WHERE clause.
        /// </summary>
        /// <param name="where">
        /// Non-optional WHERE clause.
        /// Specify "1=1" if you are sure that you want to delete all rows.
        /// </param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns>The number of items affected</returns>
        abstract public Task<int> DeleteAsync(string where,
            CancellationToken cancellationToken,
            params object[] args);

        /// <summary>
        /// Delete one or more items based on a WHERE clause.
        /// </summary>
        /// <param name="where">
        /// Non-optional WHERE clause.
        /// Specify "1=1" if you are sure that you want to delete all rows.
        /// </param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <param name="connection">The connection to use</param>
        /// <returns>The number of items affected</returns>
        abstract public Task<int> DeleteAsync(string where,
            DbConnection connection,
            params object[] args);

        /// <summary>
        /// Delete one or more items based on a WHERE clause.
        /// </summary>
        /// <param name="where">
        /// Non-optional WHERE clause.
        /// Specify "1=1" if you are sure that you want to delete all rows.
        /// </param>
        /// <param name="args">Auto-numbered input parameters</param>
        /// <param name="connection">The connection to use</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns>The number of items affected</returns>
        abstract public Task<int> DeleteAsync(string where,
            DbConnection connection,
            CancellationToken cancellationToken,
            params object[] args);

#if KEY_VALUES
        /// <summary>
        /// Returns a string-string dictionary which can be directly bound to ASP.NET dropdowns etc. (see https://stackoverflow.com/a/805610/795690).
        /// </summary>
        /// <param name="orderBy">Order by, defaults to primary key</param>
        /// <returns></returns>
        abstract public Task<IDictionary<string, string>> KeyValuesAsync(string orderBy = null);

        /// <summary>
        /// Returns a string-string dictionary which can be directly bound to ASP.NET dropdowns etc. (see https://stackoverflow.com/a/805610/795690).
        /// </summary>
        /// <param name="orderBy">Order by, defaults to primary key</param>
        /// <param name="cancellationToken">Async <see cref="CancellationToken"/></param>
        /// <returns></returns>
        abstract public Task<IDictionary<string, string>> KeyValuesAsync(CancellationToken cancellationToken, string orderBy = null);
#endif
        #endregion
    }
}
#endif