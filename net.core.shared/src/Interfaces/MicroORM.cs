using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

using Mighty.DatabasePlugins;
using Mighty.Validation;

namespace Mighty.Interfaces
{
	// NEW new:
	//	- Clean support for Single with columns
	//	- Compound PKs
	//	- Cleaner support for sequences (incl. one less DB round-trip on sequence-based insert)
	//	- With the new inner loop this really might be faster than Massive too. 'Kinell.
	//  - True support for ulong for those ADO.NET providers which use it (MySQL...) [CHECK THIS!!]
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
		virtual public string ConnectionString { get; protected set; }
		virtual public DbProviderFactory Factory { get; protected set; }
		virtual public DatabasePlugin _plugin { get; protected set; }
		virtual public Validator _validator { get; protected set; }

		virtual public string TableName { get; protected set; } // NB this may have a dot in to specify owner/schema, and then needs splitting by us, but ONLY when getting information schema
		virtual public string PrimaryKeyFields { get; protected set; } // un-separated PK(s)
		virtual public List<string> PrimaryKeyList { get; protected set; } // separated, lowered PK(s)
		virtual public string DefaultColumns { get; protected set; }
		
		abstract public IEnumerable<dynamic> TableInfo { get; }
#endregion

#region User hooks
		// You could override this to establish, for example, the convention of using _ to separate schema/owner from table (just replace "_" with "." and return!)
		virtual public string CreateTableNameFromClassName(string className) { return className; }
#endregion

#region MircoORM interface
		// NB MUST return object not int because of MySQL ulong return type.
		// Note also: it is worth passing in something other than "*"; COUNT over any
		// column which can contain null COUNTS only the non-null values.
		virtual public object Count(string columns = "*", string where = null,
			params object[] args)
		{
			return Count(columns, where, null, args);
		}

		abstract public object Count(string columns = "*", string where = null,
			DbConnection connection = null,
			params object[] args);

		// Use this for MAX, MIN, SUM, AVG (basically it's scalar on current table)
		virtual public object Aggregate(string expression, string where = null,
			params object[] args)
		{
			return Aggregate(expression, where, null, args);
		}

		abstract public object Aggregate(string expression, string where = null,
			DbConnection connection = null,
			params object[] args);

		// ORM: Single from our table
		virtual public dynamic Single(object key, string columns = null,
			DbConnection connection = null)
		{
			return Single(WhereForKeys(), connection, columns, KeyValuesFromKey(key));
		}

		virtual public dynamic Single(string where,
			params object[] args)
		{
			return Single(where, null, null, args);
		}

		// THAT is it........ :-))))))
		// DbConnection coming before columns spec is really useful, as it avoids ambiguity between a column spec and a first string arg
		virtual public dynamic Single(string where,
			DbConnection connection = null,
			string columns = null,
			params object[] args)
		{
			return SingleWithParams(where, columns, connection: connection, args: args);
		}
		
		// WithParams version just in case; allows transactions for a start
		virtual public dynamic SingleWithParams(string where, string columns = null,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args)
		{
			return AllWithParams(CommandBehavior.SingleRow,
				null, null, columns,
				inParams, outParams, ioParams, returnParams,
				connection,
				args).FirstOrDefault();
		}

		// ORM
		virtual public IEnumerable<dynamic> All(
			string where = null, string orderBy = null, string columns = null,
			params object[] args)
		{
			return AllWithParams(where, orderBy, columns, args: args);
		}

		virtual public IEnumerable<dynamic> AllWithParams(
			string where = null, string orderBy = null, string columns = null,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args)
		{
			return AllWithParams(CommandBehavior.Default,
				where, orderBy, columns,
				inParams, outParams, ioParams, returnParams,
				connection,
				args);		
		}

		// ORM version (there is also a data wrapper version).
		// You may provide orderBy, if you don't it will try to order by PK (and will produce an exception if there is no PK defined).
		// <see cref="columns"/> parameter not placed first, as it's an override to something we may have already
		// provided in the constructor...
		virtual public dynamic Paged(string orderBy = null, string where = null,
			string columns = null,
			int pageSize = 20, int currentPage = 1,
			DbConnection connection = null,
			params object[] args)
		{
			return PagedFromSelect(columns, CheckTableName(), orderBy ?? CheckPrimaryKeyFields(), where, pageSize, currentPage, connection, args);
		}

		// save (insert or update) one or more items
		virtual public int Save(params object[] items)
		{
			return Save(null, items);
		}

		virtual public int Save(DbConnection connection, params object[] items)
		{
			return Action(ORMAction.Save, connection, items);
		}
		
		virtual public int Insert(params object[] items)
		{
			return Insert(null, items);
		}

		virtual public int Insert(DbConnection connection, params object[] items)
		{
			return Action(ORMAction.Insert, connection, items);
		}

		virtual public int Update(params object[] items)
		{
			return Update(null, items);
		}

		virtual public int Update(DbConnection connection, params object[] items)
		{
			return Action(ORMAction.Update, connection, items);
		}

		virtual public int Delete(params object[] items)
		{
			return Delete((DbConnection)null, items);
		}

		virtual public int Delete(DbConnection connection, params object[] items)
		{
			return Action(ORMAction.Delete, connection, items);
		}

		virtual  public dynamic New()
		{
			return NewFrom();
		}

		abstract public dynamic NewFrom(object nameValues = null, bool addNonPresentAsDefaults = true);

		// Apply all fields which are present in item to the row matching key.
		// We *don't* filter by available columns - call with <see cref="CreateFrom"/>(<see cref="partialItem"/>) to do that.
		virtual public int UpdateFrom(object partialItem, object key)
		{
			return UpdateFrom(partialItem, key, null);
		}

		virtual public int UpdateFrom(object partialItem, object key,
			DbConnection connection)
		{
			return UpdateFrom(partialItem, WhereForKeys(), KeyValuesFromKey(key));
		}

		// apply all fields which are present in item to all rows matching where clause
		// for safety you MUST specify the where clause yourself (use "1=1" to update all rows)
		// this removes/ignores any PK fields from the action; keeps auto-named params for args,
		// and uses named params for the update feilds.
		virtual public int UpdateFrom(object partialItem, string where,
			params object[] args)
		{
			return UpdateFrom(partialItem, where, null, args);
		}

		abstract public int UpdateFrom(object partialItem, string where,
			DbConnection connection,
			params object[] args);

		virtual public int DeleteByKey(params object[] keys)
		{
			return DeleteByKey(null, keys);
		}

		abstract public int DeleteByKey(DbConnection connection, params object[] keys);

		// for safety you MUST specify the where clause yourself (use "1=1" to delete all rows)
		virtual public int Delete(string where,
			params object[] args)
		{
			return Delete(where, null, args);
		}

		abstract public int Delete(string where,
			DbConnection connection,
			params object[] args);

		abstract public bool IsKey(string fieldName);

		abstract public dynamic ColumnInfo(string column, bool ExceptionOnAbsent = true);

		abstract public dynamic GetColumnDefault(string columnName);

		abstract protected object[] KeyValuesFromKey(object key);

		abstract protected string WhereForKeys();

		abstract protected string CheckPrimaryKeyFields();

		abstract protected string CheckTableName();

		abstract public int Action(ORMAction action, DbConnection connection, params object[] items);
#endregion
	}
}
