using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Mighty.Interfaces
{

	// Abstract class 'interface' for the ADO.NET Data Access Wrapper methods (i.e. the ones which can be used even if no table
	// has been specified).
	// Uses abstract class, not interface, because the semantics of interface means it can never have anything added to it!
	// (See ... MS document about DB classes; SO post about intefaces)
	abstract public partial class MicroORM //DataAccessWrapper
	{
#region Properties
		abstract public IEnumerable<dynamic> TableInfo { get; }
#endregion

		// All versions which simply redirect to other versions are defined here, not in the main class.
#region DataAccessWrapper
		abstract public DbConnection OpenConnection();

		virtual public IEnumerable<dynamic> Query(DbCommand command,
			DbConnection connection = null)
		{
			return QueryNWithParams<dynamic>(command: command, connection: connection);
		}

		// no connection, easy args
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

		// no connection, easy args
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
			return ExecuteWithParams(sql, args: args);
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
			return Execute(command, connection);
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
			return Execute(command, connection);
		}

		abstract public object Scalar(DbCommand command,
			DbConnection connection = null);

		// no connection, easy args
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

		// You must provide orderBy for a paged query; where is optional.
		// In this one instance, because of the connection to the underlying logic of these queries, the user
		// can pass "SELECT columns" instead of columns.
		abstract public dynamic PagedFromSelect(string columns, string tablesAndJoins, string orderBy, string where = null,
			int pageSize = 20, int currentPage = 1,
			DbConnection connection = null,
			params object[] args);

		// note 1: no <see cref="DbConnection"/> param to either of these, because the connection for a command to use
		// is always passed in to the action which uses it, or else created by the microORM on the fly
		// note 2: some API calls of the microORM take command objects, you are recommended to pass in commands created
		// by these methods, as certain provider specific command properties are set by Massive on some providers, so
		// your results may vary if you pass in a command not constructed here.
		virtual public DbCommand CreateCommand(string sql,
			params object[] args)
		{
			return CreateCommandWithParams(sql, args: args);
		}

		abstract public DbCommand CreateCommandWithParams(string sql,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null, bool isProcedure = false,
			params object[] args);

		// Add kv pair stuff for dropdowns? Maybe, at max, provide a method to convert IEnumerable<dynamic> to kv pair.
		// ...

		abstract public dynamic ResultsAsExpando(DbCommand cmd);

		abstract protected IEnumerable<dynamic> AllWithParams(
			CommandBehavior behavior,
			string where = null, string orderBy = null, string columns = null,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args);

		abstract protected IEnumerable<T> QueryNWithParams<T>(string sql = null, object inParams = null, object outParams = null, object ioParams = null, object returnParams = null, bool isProcedure = false, DbCommand command = null, CommandBehavior behavior = CommandBehavior.Default, DbConnection connection = null, params object[] args);
#endregion
	}
}
