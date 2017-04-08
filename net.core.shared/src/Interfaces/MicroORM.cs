using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Mighty.Interfaces
{
	// NEW new:
	//	- Clean support for Single with columns
	//	- Compound PKs
	//	- Cleaner support for sequences (incl. one less DB round-trip on sequence-based insert)
	//	- With the new inner loop this really might be faster than Massive too. 'Kinell.
	//  - True support for ulong for those ADO.NET providers which use it (MySQL...)
	// To Add:
	//  - Firebird(?)
	//  - Generics(??)

	// Abstract class 'interface' for the ORM and ADO.NET Data Access Wrapper methods.
	// Uses abstract class, not interface, because the semantics of interface means it can never have anything added to it!
	// (See ... MS document about DB classes; SO post about intefaces)
	//
	// Notes:
	//	- Any params type argument is ALWAYS last (it must be...)
	//	- DbConnection is always last (or last before any params), except in the Single-with-columns overload, where it needs to be where it is
	//	  to play the very useful dual role of also disambiguating calls to this overload from calls to the simpler overload without columns.
	//	- ALL database parameters (i.e. everything sent to the DB via args, inParams or ioParams) is ALWAYS passed in as a true database
	//	  parameter under all circumstances - so can never be used for direct SQL injection. In general (i.e. assuming
	//	  you aren't building SQL from the value yourself, anywhere) strings, etc., which are passed in will NOT need any escaping.
	//
	abstract public partial class MicroORM
	{
#region Properties
		// initialise table name from class name, but only if we are a *subclass* of MightyORM(!)
		// throw exception if attempt to use it when not set
		// NB this may have a dot in to specify owner/schema, and then needs splitting by us, but ONLY when getting information schema
		private string _TableName;
		// this implementation should go in the actual class; and these definitions should be abstract
		// and I think we should access the table name via a method, so that the user can see when this is null without an exception
		// no, via a protected property called ProtectedTableName with the same backing field!, that's nicer
		public string TableName {
			get
			{
				if (_TableName == null)
				{
					throw new InvalidOperationException("No TableName available; this can be passed to the MightyORM constructor or automatically inferred from the class name of any sub-class of MighyORM");
				}
				return _TableName;
			}
			protected set
			{
				_TableName = value;
			}
		}
		// probably similar exception processing for PrimaryKeyList, PrimaryKeyString and DefaultColumns...
		// I think we just need private Get methods (!)
		abstract public string PrimaryKeyString { get; protected set; }
		abstract public List<string> PrimaryKeyList { get; protected set; }
		abstract public string DefaultColumns { get; protected set; }

		// We don't need this... but it might be sensible to make it visible; which leaves the question should it be the raw one passed in, or the processed one?
		// Probably the raw one; the processed one will be visible (I think? yes) in any connections sent out.
		abstract public string ConnectionString { get; protected set; }

		// We need the table info so we can instantiate from form submit (or any other namevaluecollection-ish thing, via ToExpando),
		// filtering to match columns; needs to buffer itself
		abstract public IEnumerable<dynamic> TableInfo { get; protected set; }

		// some reasonable use-cases don't require this, can help you see whether or not this trip to the database has been made for you
		abstract public bool HasLoadedTableInfo { get; protected set; }
#endregion

		// We can implement NewItem() and ColumnDefault()
		// NB *VERY* useful for better PK handling; ColumnDefault needs to do buffering - actually, it doesn't because we may end up passing the very same object out twice
		abstract public object ColumnDefault(string column);

		// Will instantiate item from superset, only including columns which match the table schema
		// (read once from the database), (optionally) setting default values for any non-present columns.
		// If called with no args, will create a fully populated prototype.
		// NB You do NOT need to use this - you can create new items to pass in to Mighty more or less however you want!
		// (Of course, you do need to make sure that YOU don't pass in columns which aren't in the underlying table, or this will throw errors,
		// but whether you call this method to ensure that is up to you.)
		// (Any fields specified as PK will contain null or default; DB defined defaults for all other columns will be noticed, interpreted and applied where possible.)
		abstract public dynamic NewItem(object superset = null, bool addNonPresentAsDefaults = true);

		// NB MUST return object not int because of MySQL ulong return type
		// Can I *advertise* correct support for DBs with long types? (i.e. am I sure it does it?)
		abstract public object Count(string columns = "*", string where = null,
			params object[] args);
		abstract public object Count(string columns = "*", string where = null,
			DbConnection connection = null,
			params object[] args);

		// Use this also for MAX, MIN, SUM, AVG (basically it's scalar on current table)
		abstract public object Aggregate(string expression, string where = null,
			params object[] args);
		abstract public object Aggregate(string expression, string where = null,
			DbConnection connection = null,
			params object[] args);

		// ORM: Single from our table
		abstract public dynamic Single(object key, string columns = null,
			DbConnection connection = null);

		// I think there really are tricky problems with  this, aren't there?
		// It's a problem because we've already told the user that they can set the columns,
		// and now we're asking them to set them again; and not only that, it's getting in the
		// way of the easy-to-use args-based api.
		// We have to include columns (becasue I always want to use it), but the default HAS to
		// be null or "" so that we don't automatically overwrite the columns they've already specified.
		abstract public dynamic Single(string where,
			params object[] args);
		// THAT is it........ :-))))))
		// DbConnection coming before columns spec is really useful, as it avoids ambiguity between a column spec and a first string arg
		abstract public dynamic Single(string where,
			DbConnection connection = null,
			string columns = null,
			params object[] args);
		
		// WithParams version just in case; allows transactions for a start
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

		// ORM version (there is also a data wrapper version)
		// You must provider orderBy, except you don't have to as it will order by PK if you don't (or exception if there is no PK defined)
		// columns (currently?) not first, as it's an override to something we (may) have already provided in the constructor...
		abstract public dynamic Paged(string orderBy = null, string where = null,
			string columns = null,
			int pageSize = 20, int currentPage = 1,
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
		abstract public int Save(DbConnection connection, params object[] items);
		
		abstract public int Insert(params object[] items);
		abstract public int Insert(DbConnection connection, params object[] items);

		abstract public int Update(params object[] items);
		abstract public int Update(DbConnection connection, params object[] items);

		// apply all fields which are present in item to the row matching key
		abstract public int UpdateFrom(object partialItem, object key);
		abstract public int UpdateFrom(object partialItem, object key,
			DbConnection connection);

		// apply all fields which are present in item to all rows matching where clause
		// for safety you MUST specify the where clause yourself (use "1=1" to update all rows)
		abstract public int UpdateFrom(object partialItem, string where,
			params object[] args);
		abstract public int UpdateFrom(object partialItem, string where,
			DbConnection connection,
			params object[] args);

		// delete item from table; what about deleting by object? (maybe key can be pk OR expando containing pk? no)
		// also why the f does this fetch the item back before deleting it, when it's by PK? sod it, let the user
		// fetch it; only delete by item, and only if (there's a PK and) the item contains the PK. that means
		// the user has prefetched it. Good.
		// I prefer this:
		// delete one or more items
		abstract public int Delete(params object[] items);
		abstract public int Delete(DbConnection connection, params object[] items);
		abstract public int DeleteByKey(params object[] keys);
		abstract public int DeleteByKey(DbConnection connection, params object[] keys);
		// for safety you MUST specify the where clause yourself (use "1=1" to delete all rows)
		abstract public int Delete(string where,
			params object[] args);
		abstract public int Delete(string where,
			DbConnection connection,
			params object[] args);

#region User hooks
		// false => do nothing with this object but continue with the list
		virtual public bool Inserting(dynamic item) { return true; }
		virtual public void Inserted(dynamic item) {}
		// false => do nothing with this object but continue with the list
		virtual public bool Updating(dynamic item) { return true; }
		virtual public void Updated(dynamic item) {}
		// false => do nothing with this object but continue with the list
		virtual public bool Deleting(dynamic item) { return true; }
		virtual public void Deleted(dynamic item) {}

		// You could override this to establish, for example, the convention of using _ to separate schema/owner from table (just replace "_" with "." and return!)
		virtual public string CreateTableNameFromClassName(string className) { return className; }
#endregion
	}
}
