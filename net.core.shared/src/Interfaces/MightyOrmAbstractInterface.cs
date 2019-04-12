using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

using Mighty.Plugins;
using Mighty.Mapping;
using Mighty.Profiling;
using Mighty.Validation;

// <summary>
// TO DO: Not sure about putting this in a separate namespace, but maybe best to hide the mockable version?
// </summary>
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
	// NB Mighty is dynamic-focussed, so even when you are using MightyOrm<T> instead of MightyOrm (which is like MightyOrm<dynamic>), the
	// T determines the output type, but not the input type (which can be of type T, but can also be any of the various arbitrary objects
	// which the micro-ORM supports, with appropriately named fields).

    /// <summary>
    /// Abstract interface for all features of <see cref="MightyOrm{T}"/>, provided for injection and mocking.
    /// </summary>
    /// <typeparam name="T"></typeparam>
	abstract public partial class MightyOrmAbstractInterface<T>
	{
        // In C# (though not all languages, see discussion here https://stackoverflow.com/a/11271938/795690)
        // constructors cannot be overridden, and therefore cannot be defined in abstract classes.

        #region Npgsql cursor dereferencing
        // Abstract class 'interface' for Npgsql cursor control additions.
        // These should ideally be contributed back to Npgsql ([ref]()), but for now are added to MightyOrm.
        // (Note: unfortunately it looks far from trivial to set up a full Npgsql build environment in order to create
        // a properly constructed and tested PR for that project. Which is not to say it won't be done at some point.)

        /// <summary>
        /// Should we auto-dereference cursors when using the Npgsql ADO.NET driver? (See Mighty documentation.)
        /// </summary>
        abstract public bool NpgsqlAutoDereferenceCursors { get; set; }

		/// <summary>
		/// How many rows at a time should we fetch if auto-dereferencing cursors on the Npgsql ADO.NET driver? (Default value 10,000.) (See Mighty documentation.)
		/// </summary>
		abstract public int NpgsqlAutoDereferenceFetchSize { get; set; }
		#endregion

		#region Properties
		/// <summary>
		/// Connection string
		/// </summary>
		abstract public string ConnectionString { get; protected set; }

		/// <summary>
		/// Validator
		/// </summary>
		abstract public Validator Validator { get; protected set; }

		/// <summary>
		/// C# &lt;=&gt; SQL mapper
		/// </summary>
		abstract public SqlNamingMapper SqlMapper { get; protected set; }

		/// <summary>
		/// Optional SQL profiler
		/// </summary>
		abstract public SqlProfiler SqlProfiler { get; protected set; }

		/// <summary>
		/// Table name (null if non-table-specific instance)
		/// </summary>
		abstract public string TableName { get; protected set; }

		/// <summary>
		/// Table owner/schema (null if not specified)
		/// </summary>
		abstract public string TableOwner { get; protected set; }

		/// <summary>
		/// Bare table name (without owner/schema part)
		/// </summary>
		abstract public string BareTableName { get; protected set; }

		/// <summary>
		/// Primary key field or fields (no mapping applied)
		/// </summary>
		abstract public string PrimaryKeyFields { get; protected set; }

		/// <summary>
		/// Separated, lowered primary key fields (no mapping applied)
		/// </summary>
		abstract public List<string> PrimaryKeyList { get; protected set; }

		/// <summary>
		/// All columns in one string, or "*" (mapping, if any, already applied)
		/// </summary>
		abstract public string Columns { get; protected set; }

		/// <summary>
		/// Separated column names, in a list (mapping, if any, already applied)
		/// </summary>
		abstract public List<string> ColumnList { get; protected set; }

		/// <summary>
		/// Sequence name or identity retrieval function (always null for compound PK)
		/// </summary>
		abstract public string SequenceNameOrIdentityFunction { get; protected set; }

#if KEY_VALUES
		/// <summary>
		/// Column from which value is retrieved by <see cref="KeyValues"/>
		/// </summary>
		abstract public string ValueColumn { get; protected set; }
#endif

		/// <summary>
		/// Table meta data (filtered to be only for columns specified by the generic type T, or by consturctor `columns`, if present)
		/// </summary>
		abstract public IEnumerable<dynamic> TableMetaData { get; }
#endregion

		// 'Interface' for the general purpose data access wrapper methods (i.e. the ones which can be used
		// even if no table has been specified).
		// All versions which simply redirect to other versions are defined here, not in the main class.
#region Non-table specific methods
		abstract public DbCommand CreateCommand(string sql,
			params object[] args);

		abstract public DbCommand CreateCommand(string sql,
			DbConnection connection,
			params object[] args);

		abstract public DbCommand CreateCommandWithParams(string sql,
			object inParams = null, object outParams = null, object ioParams = null, object returnParams = null, bool isProcedure = false,
			DbConnection connection = null,
			params object[] args);

		abstract public dynamic ResultsAsExpando(DbCommand cmd);
#endregion

#region Table specific methods
		abstract public T New();

		abstract public T NewFrom(object nameValues = null, bool addNonPresentAsDefaults = true);

		abstract public dynamic GetColumnInfo(string column, bool ExceptionOnAbsent = true);

		abstract public object GetColumnDefault(string columnName);

		abstract public List<object> IsValid(object item, OrmAction action = OrmAction.Save);

		abstract public bool HasPrimaryKey(object item);

		abstract public object GetPrimaryKey(object item, bool alwaysArray = false);
#endregion
	}
}
