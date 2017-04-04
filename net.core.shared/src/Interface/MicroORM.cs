using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Mighty.Interface
{
	// Abstract class 'interface' for ORM and ADO.NET Data Access Wrapper methods.
	// Uses abstract class, not interface, because the semantics of interface means it can never have anything added to it!
	// (See ... MS document about DB classes; SO post about intefaces)
	public abstract class MicroORM
	{
		// We need the schema so we can instantiate from form submit (or any other namevaluecollection-ish thing, via ToExpando),
		// filtering to match columns; needs to buffer result
		public IEnumerable<dynamic> TableInfo {get, set}

		// We can implement prototype and defaultvalue(column)
		// NB *VERY* useful for better PK handling; needs to do some buffering
		public object ColumnDefault(string column);

		// Will instantiate item from superset, only including columns which match the table schema
		// (read once from the database), (optionally) setting default values for any non-present columns
		public dynamic CreateItemFrom(object superset, bool addNonPresentAsDefaults = true);

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

		// NB MUST return object because of MySQL ulong
		// Can I *advertise* correct support for DBs with long types? (i.e. am I sure it does it?)
		abstract public object Count(string columns = "*", string where = null,
			params object[] args);

		// Use this also for MAX, MIN, SUM, AVG (basically it's scalar on current table)
		abstract public object Aggregate(string expression, string where = null,
			params object[] args);


		// ORM: Single from our table
		abstract public dynamic Single(object key, string columns = null);

		// I think there really are tricky problems with  this, aren't there?
		// It's a problem because we've already told the user that they can set the columns,
		// and now we're asking them to set them again; and not only that, it's getting in the
		// way of the easy-to-use params-based api.
		// We have to include columns, but the default HAS to be null or "*"so that we don't
		// automatically overwrite the columns they've already specified.
		abstract public dynamic Single(string where, string columns = null, params object[] args);		
		
		// WithParams version just in case, allows transaction for a start
		abstract public dynamic SingleWithParams(string where, string columns = null,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args);


		// ORM
		abstract public IEnumerable<dynamic> All(
			string where = null, string orderBy = null, int limit = 0, string columns = null,
			params object[] args);
		abstract public IEnumerable<dynamic> AllWithParams(
			string where = null, string orderBy = null, int limit = 0, string columns = null,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args);


		// non-ORM (NB columns is only used in generating SQL, so makes no sense on either of these)
		abstract public dynamic Paged(string sql,
			int pageSize = 20, int currentPage = 1,
			DbConnection connection = null,
			params object[] args);
		abstract public dynamic PagedFromProcedure(string spName,
			int pageSize = 20, int currentPage = 1,
			DbConnection connection = null,
			params object[] args);

		// ORM
		abstract public dynamic Paged(string where = null, string orderBy = null,
			string columns = null, int pageSize = 20, int currentPage = 1,
			DbConnection connection = null,
			params object[] args);


		// does update OR insert, per item
		// in my NEW version, null or default value for type in PK will save as new, as well as no PK field
		// only we don't know what the pk type is... but we do after getting the schema, and we should just use = to compare without worrying too much about types
		// is checking whether every item is valid before any saving - which is good - and not the same as checking
		// something at inserting/updating time; still if we're going to use a transaction ANYWAY, and this does.... hmmm... no: rollback is EXPENSIVE
		// returns the sum of the number of rows affected;
		// *** insert WILL set the PK field, as long as the object was an expando in the first place (could upgrade that; to set PK
		// in Expando OR in settable property of correct name)
		// *** we can assume that it is NEVER valid for the user to specify the PK value manually - though they can of course specify the pkFieldName,
		// and the pkSequence, for those databases which work that way; I strongly suspect we should be able to shove the sequence select into ONE round
		// trip to the DB, as well.
		// (Of course, this would mean that there would be no such thing as an ORM provided update, for a table without a PK. You know what? It *is* valid to
		// set - and update based on - a compound PK. Which means it must BE valid to set a non-compound PK.)
		// I think we want primaryKeySequence (for dbs which use that; defaults to no sequence) and primaryKeyRetrievalFunction (for dbs which use that; defaults to
		// correct default to DB, but may be set to null). If both are null, you can still have a (potentially compound) PK.
		// We can use INSERT seqname.nextval and then SELECT seqname.currval in Oracle.
		// And INSERT nextval('seqname') and then currval('seqname') (or just lastval()) in PostgreSQL.
		// (if neither primaryKeySequence nor primaryKeyRetrievalFunction are set (which is always the case for compound primary keys), you MUST specify non-null, non-default values for every column in your primary key
		// before saving an object)
		// *** okay, shite, how do we know if a compound key object is an insert or an update? I think we just provide Save, which is auto, but can't work for manual primary keys,
		// and Insert and Update, which will do what they say on the tin, and which can.

		// Cannot be used with manually controlled primary keys (which includes compound primary keys), as the microORM cannot tell apart an insert from an update in this case
		// but I think this can just be an exception, as we really don't need to worry most users about it.
		// exception can check whether we are compound; or whether we may be sequence, but just not set; or whether we have retrieval fn intentionally overridden to empty string;
		// and give different messages.

		// save/insert/update one or more items
		abstract public int Save(params object[] items);
		
		abstract public int Insert(params object[] items);
		abstract public int Update(params object[] items);

		// apply all fields which are present in item to the row matching key
		abstract public int UpdateFrom(object partialItem, object key);

		// apply all fields which are present in item to all rows matching where clause
		// for safety you MUST specify the where clause yourself (use "1=1" to update all rows)
		abstract public int UpdateFrom(object partialItem, string where, params object[] args);

		// delete item from table; what about deleting by object? (maybe key can be pk OR expando containing pk? no)
		// also why the f does this fetch the item back before deleting it, when it's by PK? sod it, let the user
		// fetch it; only delete by item, and only if (there's a PK and) the item contains the PK. that means
		// the user has prefetched it. Good.
		// I prefer this:
		// delete one or more items
		abstract public int Delete(params object[] items);
		abstract public int DeleteKey(params object[] keys);
		// for safety you MUST specify the where clause yourself (use "1=1" to delete all rows)
		abstract public int Delete(string where, params object[] args);

		// We also have validation, called on each object to be updated, before any saves, if a validator was passed in
		//...

		/// Hooks; false return => do nothing with this object but continue with the list
		public bool Inserting(dynamic item) { return true; }
		public void Inserted(dynamic item) {}
		public bool Updating(dynamic item) { return true; }
		public void Updated(dynamic item) {}
		public bool Deleting(dynamic item) { return true; }
		public void Deleted(dynamic item) {};

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

		public bool NpgsqlAutoDereferenceCursors { get; set; } = true;
		public int NpgsqlAutoDereferenceFetchSize { get; set; } = 10000;
	}
}
