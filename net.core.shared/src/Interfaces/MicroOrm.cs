using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

using Mighty.DatabasePlugins;
using Mighty.Mapping;
using Mighty.Profiling;
using Mighty.Validation;

namespace Mighty.Interfaces
{
	// NEW new:
	//	- Clean support for Single with columns
	//	- Compound PKs
	//	- Cleaner support for sequences (incl. one less DB round-trip on sequence-based insert)
	//	- With the new inner loop this really might be faster than Massive too. 'Kinell.
	//  - True support for ulong for those ADO.NET providers which use it (MySQL...) [CHECK THIS!!]
	//  - Generics(!)
	// To Add:
	//  - Firebird(?)
	// We:
	//  - Solve the problem of default values (https://samsaffron.com/archive/2012/01/16/that-annoying-insert-problem-getting-data-into-the-db-using-dapper)
	//	  by ignoring them at Insert(), but by populating them (in a slightly fake, but working, way) on New()
	//	- Are genuinely cross DB, unlike Dapper Rainbow (and possibly unlike other bits of Dapper?)
	//  - Have a true System.Data hiding interface - you just don't use it *at all* unless you need transactions,
	//	  in which case you use exactly enough of it to manage your transactions, and no more.
	//	- Have an (arguably) nicer/simpler interface to parameter directions and output values than Dapper.

	// Abstract class 'interface' for the ORM and ADO.NET Data Access Wrapper methods.
	// Uses abstract class, not interface, because the semantics of interface mean it can never have anything added to it!
	// (See ... MS document about DB classes; SO post about intefaces)
	//
	// Notes:
	//	- Any params type argument is always last (it has to be)
	//	- DbConnection is always last (or last before a params argument, if any), except in the Single-with-columns overload, where it needs to be where
	//	  it is to play the very useful dual role of also disambiguating calls to this overload from calls to the simpler overload without columns.
	//	- All database parameters (i.e. everything sent to the DB via args, inParams or ioParams) are always passed in as true database
	//	  parameters under all circumstances - they are never interpolated into SQL - so they can never be used for _direct_ SQL injection.
	//	  So assuming you aren't building any SQL to execute yourself within the DB, from the values passed in, then strings etc. which are
	//	  passed in will not need any escaping to be safe.
	//
	// NB MicroORM is dynamic-focussed, so even when you are using MightyORM<T> instead of MightyORM (which is like MightyORM<dynamic>), the
	// T determines the output type, but not the input type (which can be of type T, but can also be any of the various arbitrary objects
	// which the microORM supports, with appropriately named fields).
	abstract public partial class MicroORM<T>
	{
		#region Properties
		/// <summary>
		/// Connection string
		/// </summary>
		virtual public string ConnectionString { get; protected set; }

		/// <summary>
		/// ADO.NET provider factory
		/// </summary>
		virtual public DbProviderFactory Factory { get; protected set; }

		/// <summary>
		/// Plugin
		/// </summary>
		virtual internal DatabasePlugin Plugin { get; set; }

		/// <summary>
		/// Allows setting a global validator
		/// </summary>
		static public Validator GlobalValidator { get; set; }

		/// <summary>
		/// Validator
		/// </summary>
		virtual public Validator Validator { get; protected set; }

		/// <summary>
		/// Allows setting a global sql mapper
		/// </summary>
		static public SqlNamingMapper GlobalSqlMapper { get; set; }

		/// <summary>
		/// C# &lt;=&gt; SQL mapper
		/// </summary>
		virtual public SqlNamingMapper SqlMapper { get; protected set; }

		/// <summary>
		/// Allows setting a global SQL profiler
		/// </summary>
		static public SqlProfiler GlobalSqlProfiler { get; set; }

		/// <summary>
		/// Optional SQL profiler
		/// </summary>
		virtual public SqlProfiler SqlProfiler { get; protected set; }

		/// <summary>
		/// Table name (null if non-table-specific instance)
		/// </summary>
		virtual public string TableName { get; protected set; }

		/// <summary>
		/// Table owner/schema (null if not specified)
		/// </summary>
		virtual public string TableOwner { get; protected set; }

		/// <summary>
		/// Bare table name (without owner/schema part)
		/// </summary>
		virtual public string BareTableName { get; protected set; }

		/// <summary>
		/// Primary key field or fields (no mapping applied)
		/// </summary>
		virtual public string PrimaryKeyFields { get; protected set; }

		/// <summary>
		/// Separated, lowered primary key fields (no mapping applied)
		/// </summary>
		virtual public List<string> PrimaryKeyList { get; protected set; }

		/// <summary>
		/// All columns in one string, or "*" (mapping, if any, already applied)
		/// </summary>
		virtual public string Columns { get; protected set; }

		/// <summary>
		/// Separated column names, in a list (mapping, if any, already applied)
		/// </summary>
		virtual public List<string> ColumnList { get; protected set; }

		/// <summary>
		/// Sequence name or identity retrieval fn. (always null for compound PK)
		/// </summary>
		virtual public string SequenceNameOrIdentityFn { get; protected set; }

		/// <summary>
		/// Column from which value is retrieved by <see cref="KeyValues"/>
		/// </summary>
		virtual public string ValueColumn { get; protected set; }

		/// <summary>
		/// true for dynamic instantiation; false if generically typed instantiation
		/// </summary>
		virtual internal bool UseExpando { get; set; }

		/// <summary>
		/// Table meta data (filtered to be only for columns specified by the generic type T, or by <see cref="columns"/>, where present)
		/// </summary>
		abstract public IEnumerable<dynamic> TableMetaData { get; }
		#endregion

		#region MircoORM interface
		/// <summary>
		/// Perform COUNT on current table
		/// </summary>
		/// <param name="columns">Columns (defaults to *, but can be specified, e.g., to count non-nulls in a given field)</param>
		/// <param name="where">Optional where clause</param>
		/// <param name="connection">Optional connection</param>
		/// <param name="args">Args</param>
		/// <returns></returns>
		abstract public object Count(string columns = "*", string where = null,
			DbConnection connection = null,
			params object[] args);

		/// <summary>
		/// Perform scalar operation on the current table (use for SUM, MAX, MIN, AVG, etc.)
		/// </summary>
		/// <param name="expression">Scalar expression</param>
		/// <param name="where">Optional where clause</param>
		/// <param name="connection">Optional connection</param>
		/// <param name="args">Parameters</param>
		/// <returns></returns>
		virtual public object Aggregate(string expression, string where = null,
			DbConnection connection = null,
			params object[] args)
		{
			return AggregateWithParams(expression, where, connection: connection, args: args);
		}

		/// <summary>
		/// Perform scalar operation on the current table (use for SUM, MAX, MIN, AVG, etc.), with support for named params.
		/// </summary>
		/// <param name="expression">Scalar expression</param>
		/// <param name="where">Optional where clause</param>
		/// <param name="inParams">Optional input parameters</param>
		/// <param name="outParams">Optional output parameters</param>
		/// <param name="ioParams">Optional input-output parameters</param>
		/// <param name="returnParams">Optional return parameters</param>
		/// <param name="connection">Optional connection</param>
		/// <param name="args">Optional auto-named input parameters</param>
		/// <returns></returns>
		abstract public object AggregateWithParams(string expression, string where = null,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args);

		/// <summary>
		/// Get a single object from the current table by primary key value
		/// </summary>
		/// <param name="key">Single key (or any reasonable multi-value item for compound keys)</param>
		/// <param name="columns">Optional columns to retrieve</param>
		/// <param name="connection">Optional connection</param>
		/// <returns></returns>
		virtual public T Single(object key, string columns = null,
			DbConnection connection = null)
		{
			return Single(WhereForKeys(), connection, columns, KeyValuesFromKey(key));
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
		virtual public T Single(string where,
			params object[] args)
		{
			return SingleWithParams(where, args: args);
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
		virtual public T Single(string where,
			DbConnection connection = null,
			string orderBy = null,
			string columns = null,
			params object[] args)
		{
			return SingleWithParams(where, orderBy, columns, connection: connection, args: args);
		}

		// WithParams version just in case; allows transactions for a start
		virtual public T SingleWithParams(string where, string orderBy = null, string columns = null,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args)
		{
			return AllWithParams(
				where, orderBy, columns, 1,
				inParams, outParams, ioParams, returnParams,
				connection,
				args).FirstOrDefault();
		}

		// ORM
		virtual public IEnumerable<T> All(
			string where = null, string orderBy = null, string columns = null, int limit = 0,
			params object[] args)
		{
			return AllWithParams(where, orderBy, columns, limit, args: args);
		}

		abstract public IEnumerable<T> AllWithParams(
			string where = null, string orderBy = null, string columns = null, int limit = 0,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null,
			DbConnection connection = null,
			params object[] args);

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
		virtual public PagedResults<T> Paged(string where = null, string orderBy = null,
			string columns = null,
			int pageSize = 20, int currentPage = 1,
			DbConnection connection = null,
			params object[] args)
		{
			return PagedFromSelect(columns, CheckGetTableName(), where, orderBy ?? CheckGetPrimaryKeyFields(), pageSize, currentPage, connection, args);
		}

		/// <summary>
		/// Save one or more items using params style arguments
		/// </summary>
		/// <param name="items">The items</param>
		/// <returns></returns>
		virtual public int Save(params object[] items)
		{
			return ActionOnItems(ORMAction.Save, null, items);
		}

		/// <summary>
		/// Save one or more items using pre-specified DbConnection
		/// </summary>
		/// <param name="connection">The connection</param>
		/// <param name="items">The items</param>
		/// <returns></returns>
		virtual public int Save(DbConnection connection, params object[] items)
		{
			return ActionOnItems(ORMAction.Save, connection, items);
		}

		/// <summary>
		/// Save array or other IEnumerable of items
		/// </summary>
		/// <param name="items">The items</param>
		/// <returns></returns>
		virtual public int Save(IEnumerable<object> items)
		{
			return ActionOnItems(ORMAction.Save, null, items);
		}

		/// <summary>
		/// Save array or other IEnumerable of items using pre-specified DbConnection
		/// </summary>
		/// <param name="connection">The connection</param>
		/// <param name="items">The items</param>
		/// <returns></returns>
		virtual public int Save(DbConnection connection, IEnumerable<object> items)
		{
			return ActionOnItems(ORMAction.Save, connection, items);
		}

		/// <summary>
		/// Insert single item, returning the item sent in but with PK populated.
		/// If you need all fields populated (i.e. you want to get back DB default values for non-PK fields), please create the item using New() before inserting it.
		/// </summary>
		/// <param name="items">The item to insert, in any reasonable format (for MightyORM&lt;T&gt; this includes, but is not limited to, in instance of type T)</param>
		/// <returns>The inserted item</returns>
		virtual public T Insert(object item)
		{
			T insertedItem;
			ActionOnItems(ORMAction.Insert, null, new object[] { item }, out insertedItem);
			return insertedItem;
		}

		/// <summary>
		/// Insert one or more items using params style arguments
		/// </summary>
		/// <param name="items">The items</param>
		/// <returns>The number of rows inserted</returns>
		virtual public int Insert(params object[] items)
		{
			return ActionOnItems(ORMAction.Insert, null, items);
		}

		/// <summary>
		/// Insert one or more items using pre-specified DbConnection
		/// </summary>
		/// <param name="connection">The connection</param>
		/// <param name="items">The items</param>
		/// <returns>The number of rows inserted</returns>
		virtual public int Insert(DbConnection connection, params object[] items)
		{
			return ActionOnItems(ORMAction.Insert, connection, items);
		}

		/// <summary>
		/// Insert array or other IEnumerable of items
		/// </summary>
		/// <param name="items">The items</param>
		/// <returns>The number of rows inserted</returns>
		virtual public int Insert(IEnumerable<object> items)
		{
			return ActionOnItems(ORMAction.Insert, null, items);
		}

		/// <summary>
		/// Insert array or other IEnumerable of items using pre-specified DbConnection
		/// </summary>
		/// <param name="connection">The connection</param>
		/// <param name="items">The items</param>
		/// <returns>The number of rows inserted</returns>
		virtual public int Insert(DbConnection connection, IEnumerable<object> items)
		{
			return ActionOnItems(ORMAction.Insert, connection, items);
		}

		/// <summary>
		/// Update one or more items using params style arguments
		/// </summary>
		/// <param name="items">The items</param>
		/// <returns></returns>
		virtual public int Update(params object[] items)
		{
			return ActionOnItems(ORMAction.Update, null, items);
		}

		/// <summary>
		/// Update one or more items using pre-specified DbConnection
		/// </summary>
		/// <param name="connection">The connection</param>
		/// <param name="items">The items</param>
		/// <returns></returns>
		virtual public int Update(DbConnection connection, params object[] items)
		{
			return ActionOnItems(ORMAction.Update, connection, items);
		}

		/// <summary>
		/// Update array or other IEnumerable of items
		/// </summary>
		/// <param name="items">The items</param>
		/// <returns></returns>
		virtual public int Update(IEnumerable<object> items)
		{
			return ActionOnItems(ORMAction.Update, null, items);
		}

		/// <summary>
		/// Update array or other IEnumerable of items using pre-specified DbConnection
		/// </summary>
		/// <param name="connection">The connection</param>
		/// <param name="items">The items</param>
		/// <returns></returns>
		virtual public int Update(DbConnection connection, IEnumerable<object> items)
		{
			return ActionOnItems(ORMAction.Update, connection, items);
		}

		/// <summary>
		/// Delete one or more items using params style arguments
		/// </summary>
		/// <param name="items">The items</param>
		/// <returns></returns>
		virtual public int Delete(params object[] items)
		{
			return ActionOnItems(ORMAction.Delete, null, items);
		}

		/// <summary>
		/// Delete one or more items using pre-specified DbConnection
		/// </summary>
		/// <param name="connection">The connection</param>
		/// <param name="items">The items</param>
		/// <returns></returns>
		virtual public int Delete(DbConnection connection, params object[] items)
		{
			return ActionOnItems(ORMAction.Delete, connection, items);
		}

		/// <summary>
		/// Delete array or other IEnumerable of items
		/// </summary>
		/// <param name="items">The items</param>
		/// <returns></returns>
		virtual public int Delete(IEnumerable<object> items)
		{
			return ActionOnItems(ORMAction.Delete, null, items);
		}

		/// <summary>
		/// Delete array or other IEnumerable of items using pre-specified DbConnection
		/// </summary>
		/// <param name="connection">The connection</param>
		/// <param name="items">The items</param>
		/// <returns></returns>
		virtual public int Delete(DbConnection connection, IEnumerable<object> items)
		{
			return ActionOnItems(ORMAction.Delete, connection, items);
		}

		virtual public T New()
		{
			return NewFrom();
		}

		abstract public T NewFrom(object nameValues = null, bool addNonPresentAsDefaults = true);

		// Apply all fields which are present in item to the row matching key.
		// We *don't* filter by available columns - call with <see cref="CreateFrom"/>(<see cref="partialItem"/>) to do that.
		virtual public int UpdateUsing(object partialItem, object key)
		{
			return UpdateUsing(partialItem, key, null);
		}

		virtual public int UpdateUsing(object partialItem, object key,
			DbConnection connection)
		{
			return UpdateUsing(partialItem, WhereForKeys(), KeyValuesFromKey(key));
		}

		// apply all fields which are present in item to all rows matching where clause
		// for safety you MUST specify the where clause yourself (use "1=1" to update all rows)
		// this removes/ignores any PK fields from the action; keeps auto-named params for args,
		// and uses named params for the update feilds.
		virtual public int UpdateUsing(object partialItem, string where,
			params object[] args)
		{
			return UpdateUsing(partialItem, where, null, args);
		}

		abstract public int UpdateUsing(object partialItem, string where,
			DbConnection connection,
			params object[] args);

		/// <summary>
		/// Delete rows from ORM table based on WHERE clause.
		/// </summary>
		/// <param name="where">
		/// Non-optional where clause.
		/// Specify "1=1" if you are sure that you want to delete all rows.</param>
		/// <param name="args">Optional auto-named parameters for the WHERE clause</param>
		/// <returns></returns>
		virtual public int Delete(string where,
			params object[] args)
		{
			return Delete(where, null, args);
		}

		abstract public int Delete(string where,
			DbConnection connection,
			params object[] args);

		abstract public dynamic GetColumnInfo(string column, bool ExceptionOnAbsent = true);

		abstract public object GetColumnDefault(string columnName);

		// These protected versions need to be in the interface so that they can be called from the method implementations above
		abstract protected object[] KeyValuesFromKey(object key);

		abstract protected string WhereForKeys();

		abstract protected string CheckGetPrimaryKeyFields();

		abstract protected string CheckGetValueColumn(string message);

		abstract protected string CheckGetKeyName(string message);

		abstract protected string CheckGetKeyName(int i, string message);

		abstract protected string CheckGetTableName();

		virtual internal int ActionOnItems(ORMAction action, DbConnection connection, IEnumerable<object> items)
		{
			T insertedItem;
			return ActionOnItems(action, connection, items, out insertedItem);
		}

		abstract internal int ActionOnItems(ORMAction action, DbConnection connection, IEnumerable<object> items, out T insertedItem);

		abstract public List<object> IsValid(object item, ORMAction action = ORMAction.Save);

		abstract public bool HasPrimaryKey(object item);

		abstract public object GetPrimaryKey(object item, bool alwaysArray = false);

		abstract public IDictionary<string, string> KeyValues(string orderBy = "");
		#endregion
	}
}
