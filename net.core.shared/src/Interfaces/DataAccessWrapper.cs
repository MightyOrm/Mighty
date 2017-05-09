using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Mighty.Interfaces
{

	// Abstract class 'interface' for the ADO.NET Data Access Wrapper methods (i.e. the ones which can be used even if no table
	// has been specified).
	// Uses abstract class, not interface, because the semantics of interface means it can never have anything added to it!
	// (See ... MS document about DB classes; SO post about intefaces)
	abstract public partial class MicroORM<T> //DataAccessWrapper
	{
		// All versions which simply redirect to other versions are defined here, not in the main class.
		#region DataAccessWrapper
		abstract public DbConnection OpenConnection();

		virtual public IEnumerable<dynamic> Query(DbCommand command,
			DbConnection connection = null)
		{
			return QueryNWithParams<dynamic>(command: command, connection: connection);
		}

		// no connection, easy args (use WithParams version for connection)
		virtual public IEnumerable<dynamic> Query(string sql,
			params object[] args)
		{
			return QueryNWithParams<dynamic>(sql, args: args);
		}

		virtual public IEnumerable<dynamic> QueryWithParams(string sql,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args)
		{
			return QueryNWithParams<dynamic>(sql,
				inParams, outParams, ioParams, returnParams,
				connection: connection, args: args);
		}

		virtual public IEnumerable<dynamic> QueryFromProcedure(string spName,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args)
		{
			return QueryNWithParams<dynamic>(spName,
				inParams, outParams, ioParams, returnParams,
				isProcedure: true,
				connection: connection, args: args);
		}

		virtual public IEnumerable<IEnumerable<dynamic>> QueryMultiple(DbCommand command,
			DbConnection connection = null)
		{
			return QueryNWithParams<IEnumerable<dynamic>>(command: command, connection: connection);
		}

		// no connection, easy args (use WithParams version for connection)
		virtual public IEnumerable<IEnumerable<dynamic>> QueryMultiple(string sql,
			params object[] args)
		{
			return QueryNWithParams<IEnumerable<dynamic>>(sql, args: args);
		}

		virtual public IEnumerable<IEnumerable<dynamic>> QueryMultipleWithParams(string sql,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args)
		{
			return QueryNWithParams<IEnumerable<dynamic>>(sql,
				inParams, outParams, ioParams, returnParams,
				connection: connection, args: args);
		}

		virtual public IEnumerable<IEnumerable<dynamic>> QueryMultipleFromProcedure(string spName,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args)
		{
			return QueryNWithParams<IEnumerable<dynamic>>(spName,
				inParams, outParams, ioParams, returnParams,
				isProcedure: true,
				connection: connection, args: args);
		}

		abstract public int Execute(DbCommand command,
			DbConnection connection = null);

		// no connection, easy args
		virtual public int Execute(string sql,
			params object[] args)
		{
			var command = CreateCommandWithParams(sql, args: args);
			return Execute(command);
		}

		virtual public int Execute(string sql,
			DbConnection connection,
			params object[] args)
		{
			var command = CreateCommandWithParams(sql, args: args);
			return Execute(command, connection);
		}

		// COULD add a RowCount class, like Cursor, to pick out the rowcount if required
		virtual public dynamic ExecuteWithParams(string sql,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args)
		{
			var command = CreateCommandWithParams(sql,
			inParams, outParams, ioParams, returnParams,
			args: args);
			Execute(command, connection);
			return ResultsAsExpando(command);
		}

		virtual public dynamic ExecuteAsProcedure(string spName,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args)
		{
			var command = CreateCommandWithParams(spName,
			inParams, outParams, ioParams, returnParams,
			isProcedure: true,
			args: args);
			Execute(command, connection);
			return ResultsAsExpando(command);
		}

		abstract public object Scalar(DbCommand command,
			DbConnection connection = null);

		// no connection, easy args (use WithParams version for connection)
		virtual public object Scalar(string sql,
			params object[] args)
		{
			var command = CreateCommand(sql, args);
			return Scalar(command);
		}

		virtual public object ScalarWithParams(string sql,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args)
		{
			var command = CreateCommandWithParams(sql,
			inParams, outParams, ioParams, returnParams,
			args: args);
			return Scalar(command, connection);
		}

		virtual public object ScalarFromProcedure(string spName,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args)
		{
			var command = CreateCommandWithParams(spName,
			inParams, outParams, ioParams, returnParams,
			isProcedure: true,
			args: args);
			return Scalar(command, connection);
		}

		abstract public dynamic PagedFromSelect(string columns, string tablesAndJoins, string where, string orderBy,
			int pageSize = 20, int currentPage = 1,
			DbConnection connection = null,
			params object[] args);

		// Some API calls of the microORM take command objects, you are recommended to pass in commands created
		// by these methods, as certain provider specific command properties are set by Massive on some providers, so
		// your results may vary if you pass in a command not constructed here.
		virtual public DbCommand CreateCommand(string sql,
			params object[] args)
		{
			return CreateCommandWithParams(sql, args: args);
		}

		virtual public DbCommand CreateCommand(string sql,
			DbConnection connection,
			params object[] args)
		{
			return CreateCommandWithParams(sql, args: args);
		}

		abstract public DbCommand CreateCommandWithParams(string sql,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null, bool isProcedure = false,
			DbConnection connection = null,
			params object[] args);

		// Add kv pair stuff for dropdowns? Maybe, at max, provide a method to convert IEnumerable<T> to kv pair.
		// ...

		abstract public dynamic ResultsAsExpando(DbCommand cmd);

		abstract protected IEnumerable<X> QueryNWithParams<X>(string sql = null, object inParams = null, object outParams = null, object ioParams = null, object returnParams = null, bool isProcedure = false, DbCommand command = null, CommandBehavior behavior = CommandBehavior.Default, DbConnection connection = null, params object[] args);
		#endregion
	}
}
