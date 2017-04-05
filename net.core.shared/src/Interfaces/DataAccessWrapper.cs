using System.Collections.Generic;
using System.Data.Common;

namespace Mighty.Interfaces
{

	// Abstract class 'interface' for the ADO.NET Data Access Wrapper methods (i.e. the ones which can be used even if no table has been specified).
	// Uses abstract class, not interface, because the semantics of interface means it can never have anything added to it!
	// (See ... MS document about DB classes; SO post about intefaces)
	abstract public class DataAccessWrapper
	{
		abstract public DbConnection OpenConnection();

		abstract public IEnumerable<dynamic> Query(DbCommand command,
			DbConnection connection = null);
		// no connection, easy args
		abstract public IEnumerable<dynamic> Query(string sql,
			params object[] args);
		abstract public IEnumerable<dynamic> QueryWithParams(string sql,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args);
		abstract public IEnumerable<dynamic> QueryFromProcedure(string spName,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args);

		abstract public IEnumerable<IEnumerable<dynamic>> QueryMultiple(DbCommand command,
			DbConnection connection = null);
		// no connection, easy args
		abstract public IEnumerable<IEnumerable<dynamic>> QueryMultiple(string sql,
			params object[] args);
		abstract public IEnumerable<IEnumerable<dynamic>> QueryMultipleWithParams(string sql,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args);
		abstract public IEnumerable<IEnumerable<dynamic>> QueryMultipleFromProcedure(string spName,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args);

		abstract public int Execute(DbCommand command,
			DbConnection connection = null);
		// no connection, easy args
		abstract public int Execute(string sql,
			params object[] args);
		// COULD add a RowCount class, like Cursor, to pick out the rowcount if required
		abstract public dynamic ExecuteWithParams(string sql,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args);
		abstract public dynamic ExecuteAsProcedure(string spName,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args);

		abstract public object Scalar(DbCommand command,
			DbConnection connection = null);
		// no connection, easy args
		abstract public object Scalar(string sql,
			params object[] args);
		abstract public object ScalarWithParams(string sql,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args);
		abstract public object ScalarFromProcedure(string spName,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args);

		// You must provide orderBy for a paged query; where is optional.
		abstract public dynamic PagedFromSelect(string columns, string tablesAndJoins, string orderBy, string where = null,
			int pageSize = 20, int currentPage = 1,
			DbConnection connection = null,
			params object[] args);
		abstract public dynamic PagedFromProcedure(string spName,
			int pageSize = 20, int currentPage = 1,
			DbConnection connection = null,
			params object[] args);


		abstract public DbCommand CreateCommand(string sql,
			DbConnection conn = null, // do we need (no) or want (not sure) this, here? it is a prime purpose of a command to have a connection, so why not?
			params object[] args);
		abstract public DbCommand CreateCommandWithParams(string sql,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null, bool isProcedure = false,
			DbConnection connection = null,
			params object[] args);

		// kv pair stuff for dropdowns, but it's not obvious you want your dropdown list in kv pair...
		// it's a lot of extra code for this - you could add to kvpairs (whatever it's called) as
		// an extension of IEnumerable<dynamic> ... if you can. That means almost no extra code.
		// it is very easy for the user to do this conversion themselves

		// BASICALLY DONE THE BELOW, I THINK:
		
		// create item from form post, only filling in fields which are in the schema - not bad!
		// (but the form post namevaluecollection is not in NET CORE1.1 anyway ... so what are they doing?
		// no form posts per se in MVC, but what about that way I was reading back from a form, for files?)
		// Oh bollocks, it was left out by mistake and a I can have it:
		// https://github.com/dotnet/corefx/issues/10338

		//For folks that hit missing types from one of these packages after upgrading to Microsoft.NETCore.UniversalWindowsPlatform they can reference the packages directly as follows.
		//"System.Collections.NonGeneric": "4.0.1",
		//"System.Collections.Specialized": "4.0.1", ****
		//"System.Threading.Overlapped": "4.0.1",
		//"System.Xml.XmlDocument": "4.0.1"
	}
}
