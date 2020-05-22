using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

using Mighty.DataContracts;
using Mighty.Keys;
using Mighty.Mapping;
using Mighty.Profiling;
using Mighty.Validation;

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
    // (See ... MS document about DB classes; SO post about intefaces)
    //
    // Rules for argument positions:
    //    - Any params type argument is always last (it has to be)
    //    - In any method signature with no optional parameters before params args, then a new overload is created
    //      with a non-optional DbConnection just before the params args
    //         o *** Except in the Single-with-columns overload, where DbConnection being where it is plays the very
    //               useful dual role of also disambiguating calls to this overload from calls to the simpler overload
    //               without columns.
    //    - In any method signature with some optional parameters, then DbConnection is just added as another optional
    //      parameter, just before params args

    //    - When adding CancellationToken, we just always make two variants, one without CancellationToken (i.e. same
    //      signature as the Sync version), and one with CancellationToken as the compulsory first argument
    //
    // All database parameters (i.e. everything sent to the DB via args, inParams or ioParams) are always passed in as true database
    // parameters under all circumstances - they are never interpolated into SQL - so they can never be used for _direct_ SQL injection.
    // So assuming you aren't building any SQL to execute yourself within the DB, from the values passed in, then strings etc. which are
    // passed in will not need any escaping to be safe.
    //
    // NB Mighty is dynamic-focussed, so even when you are using MightyOrm<T> instead of MightyOrm (which is like MightyOrm<dynamic>), the
    // T determines the output type, but not the input type (which can be of type T, but can also be any of the various arbitrary objects
    // which the micro-ORM supports, with appropriately named fields).

    /// <summary>
    /// Abstract interface for all features of <see cref="MightyOrm{T}"/>, provided for injection and mocking.
    /// </summary>
    /// <typeparam name="T">The generic type for items returned by this instance</typeparam>
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

        #region Sql Server auto-join commands to any transaction
        /// <summary>
        /// Should we automatically enlist all commands to any transaction on any connection provided?
        /// SQL Server does not do this automatically even though other ADO.NET providers do.
        /// (Default value true.)
        /// </summary>
        abstract public bool SqlServerAutoEnlistCommandsToTransactions { get; set; }
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
        abstract public SqlNamingMapper SqlNamingMapper { get; protected set; }

        /// <summary>
        /// Optional SQL profiler
        /// </summary>
        abstract public DataProfiler DataProfiler { get; protected set; }

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
        /// Keys and sequence
        /// </summary>
        abstract public PrimaryKeyInfo PrimaryKeyInfo { get; protected set; }

#if KEY_VALUES
        /// <summary>
        /// Column from which value is retrieved by <see cref="KeyValues"/>
        /// </summary>
        abstract public string ValueColumn { get; protected set; }
#endif

        /// <summary>
        /// A data contract for the current item type, specified columns and case-sensitivity
        /// </summary>
        abstract public DataContract DataContract { get; protected set; }

        /// <summary>
        /// The default set of columns to use for queries
        /// </summary>
        abstract public string DefaultColumns { get; protected set; }

        /// <summary>
        /// true for generic instance; false if dynamically typed instance
        /// </summary>
        abstract public bool IsGeneric { get; protected set; }

        /// <summary>
        /// Table meta data (filtered to be only for columns specified by the generic type T, or by consturctor `columns`, if present)
        /// </summary>
        abstract public IEnumerable<dynamic> TableMetaData { get; }
#endregion

        // 'Interface' for the general purpose data access wrapper methods (i.e. the ones which can be used
        // even if no table has been specified).
        // All versions which simply redirect to other versions are defined here, not in the main class.
#region Non-table specific methods
        /// <summary>
        /// Create a <see cref="DbCommand"/> ready for use with Mighty.
        /// Manually creating commands is an advanced use-case; standard Mighty methods create and dispose
        /// of required <see cref="DbCommand"/> and <see cref="DbConnection"/> objects for you.
        /// You should use one of the variants of <see cref="CreateCommand(string, object[])"/>
        /// for all commands passed in to Mighty, since on some providers this sets provider specific properties which are needed to ensure expected behaviour with Mighty.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="args">Auto-numbered parameters for the SQL</param>
        /// <returns></returns>
        abstract public DbCommand CreateCommand(string sql,
            params object[] args);

        /// <summary>
        /// Create a <see cref="DbCommand"/> ready for use with Mighty.
        /// Manually creating commands is an advanced use-case; standard Mighty methods create and dispose
        /// of required <see cref="DbCommand"/> and <see cref="DbConnection"/> objects for you.
        /// You should use one of the variants of <see cref="CreateCommand(string, object[])"/>
        /// for all commands passed in to Mighty, since on some providers this sets provider specific properties which are needed to ensure expected behaviour with Mighty.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="connection">The connection to use</param>
        /// <param name="args">Auto-numbered parameters for the SQL</param>
        /// <returns></returns>
        abstract public DbCommand CreateCommand(string sql,
            DbConnection connection,
            params object[] args);

        /// <summary>
        /// Create a general-purpose <see cref="DbCommand"/> with named parameters ready for use with Mighty.
        /// Manually creating commands is an advanced use-case; standard Mighty methods create and dispose
        /// of required <see cref="DbCommand"/> and <see cref="DbConnection"/> objects for you.
        /// You should use one of the variants of <see cref="CreateCommand(string, object[])"/>
        /// for all commands passed in to Mighty, since on some providers this sets provider specific properties which are needed to ensure expected behaviour with Mighty.
        /// </summary>
        /// <param name="sql">The command SQL</param>
        /// <param name="inParams">Named input parameters</param>
        /// <param name="outParams">Named output parameters</param>
        /// <param name="ioParams">Named input-output parameters</param>
        /// <param name="returnParams">Named return parameters</param>
        /// <param name="isProcedure">Is the SQL a stored procedure name (with optional argument spec) only?</param>
        /// <param name="connection">The connection to use</param>
        /// <param name="args">Auto-numbered parameters for the SQL</param>
        /// <returns></returns>
        abstract public DbCommand CreateCommandWithParams(string sql,
            object inParams = null, object outParams = null, object ioParams = null, object returnParams = null, bool isProcedure = false,
            DbConnection connection = null,
            params object[] args);

        /// <summary>
        /// Put all output and return parameter values into an expando.
        /// Due to ADO.NET limitations, should only be called after disposing of any associated reader.
        /// </summary>
        /// <param name="cmd">The command</param>
        /// <returns></returns>
        abstract public dynamic ResultsAsExpando(DbCommand cmd);
#endregion

#region Table specific methods
        /// <summary>
        /// Return a new item populated with defaults which correctly reflect the defaults of the current database table, if any.
        /// </summary>
        /// <param name="nameValues">Optional name-value collection from which to initialise some or all of the fields</param>
        /// <param name="addNonPresentAsDefaults">
        /// When true also include default values for fields not present in <paramref name="nameValues"/>
        /// but which exist in the defined list of columns for the current table in Mighty
        /// </param>
        /// <returns></returns>
        abstract public T New(object nameValues = null, bool addNonPresentAsDefaults = true);

        /// <summary>
        /// Get the meta-data for a single column
        /// </summary>
        /// <param name="column">Column name</param>
        /// <param name="ExceptionOnAbsent">If true throw an exception if there is no such column, otherwise return null.</param>
        /// <returns></returns>
        abstract public dynamic GetColumnInfo(string column, bool ExceptionOnAbsent = true);

        /// <summary>
        /// Get the default value for a column.
        /// </summary>
        /// <param name="columnName">The column name</param>
        /// <returns></returns>
        /// <remarks>
        /// Although it might look more efficient, GetColumnDefault should not do buffering, as we don't
        /// want to pass out the same actual object more than once.
        /// </remarks>
        abstract public object GetColumnDefault(string columnName);

        /// <summary>
        /// Is the passed in item valid against the current validator?
        /// </summary>
        /// <param name="item">The item</param>
        /// <param name="action">Optional action type (defaults to <see cref="OrmAction.Save"/>)</param>
        /// <returns></returns>
        abstract public List<object> IsValid(object item, OrmAction action = OrmAction.Save);

        /// <summary>
        /// True only if the passed in object contains (a) field(s) matching the named primary key(s) of the current table.
        /// </summary>
        /// <param name="item">Item to check</param>
        /// <returns></returns>
        abstract public bool HasPrimaryKey(object item);

        /// <summary>
        /// Return primary key for item, as a single object for simple PK, or as object[] for compound PK.
        /// </summary>
        /// <param name="item">The item</param>
        /// <param name="alwaysArray">If true return object[] of 1 item, even for simple PK</param>
        /// <returns></returns>
        abstract public object GetPrimaryKey(object item, bool alwaysArray = false);
#endregion
    }
}
