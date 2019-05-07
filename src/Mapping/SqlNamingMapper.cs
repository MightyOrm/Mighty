using System;
using System.Dynamic;
using System.Linq;
using System.Reflection;

using Mighty.DataContracts;
using Mighty.Interfaces;

namespace Mighty.Mapping
{
    /// <summary>
    /// Useful object extensions
    /// </summary>
    static public partial class ObjectExtensions
    {
        /// <summary>
        /// Utility method to create <see cref="SqlNamingMapper"/> naming maps, chainable in fluent syntax.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="memberName"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        static public string Map(this string from, string memberName, string columnName)
        {
            if (string.Equals(from, memberName, StringComparison.OrdinalIgnoreCase)) return columnName;
            else return from;
        }
    }
    /// <summary>
    /// Pass an instance of this class to the constructor of <see cref="MightyOrm"/> in order to
    /// map between C# field names and SQL column names.
    /// If you're not (yet) used to <see cref="Action"/>/<see cref="Func{T, TResult}"/> syntax in C#, you may find
    /// slightly harder to set up this mapper than if it had just been a class with methods you can override (see
    /// Mighty documentation for examples). One reason for doing it like this is that Mighty can then do much more
    /// aggressive and successful caching of its data contracts, by checking whether the mapping functions (not just
    /// the whole mapper) are the same.
    /// </summary>
    public class SqlNamingMapper : SqlNamingMapperAbstractInterface
    {
        #region Table-only features (not needed in column mapping conract)
        /// <summary>
        /// Function to get database table name from the data item type.
        /// Default is to return <c>null</c> in order to not override the table name.
        /// The type passed in is the class or subclass type for dynamic instances of <see cref="MightyOrm"/>
        /// and is the generic type T for generic instances of <see cref="MightyOrm{T}"/>.
        /// </summary>
        override public Func<Type, string> TableNameMapping { get; protected set; } = (t) => null;

        /// <summary>
        /// Function to get primary key field name(s) from the data item type and field or property name.
        /// The exact C# field/property name(s) should be returned and not database column names (where these are different).
        /// The default behaviour is to return <c>null</c> which results in no primary keys being specified in this way -
        /// they may still be specified using the <see cref="MightyOrm"/> `keys` constructor parameter.
        /// The type passed in is the class or subclass type for dynamic instances of <see cref="MightyOrm"/>
        /// and is the generic type T for generic instances of <see cref="MightyOrm{T}"/>.
        /// </summary>
        override public Func<Type, string> GetPrimaryKeyFieldNames { get; protected set; } = (t) => null;

        /// <summary>
        /// Function to get the sequence from the data item type.
        /// Generally only applicable to sequence-based databases (Oracle and Postgres), except in the rare case where
        /// you may need to override the default identity function on identity-based databases (see Mighty documentation).
        /// The type passed in is the class or subclass type for dynamic instances of <see cref="MightyOrm"/>
        /// and is the generic type T for generic instances of <see cref="MightyOrm{T}"/>.
        /// </summary>
        override public Func<Type, string> GetSequenceName { get; protected set; } = (t) => null;
        #endregion

        #region Table-column features (needed in column mapping conract)
        /// <summary>
        /// Return <see cref="AutoMap.On"/> whatever type is sent in.
        /// </summary>
        /// <remarks>
        /// We may need to be able to identify this one and see if it has changed?
        /// </remarks>
        public static readonly Func<Type, AutoMap> AlwaysAutoMap = (t) => AutoMap.On;

        /// <summary>
        /// Specify whether Mighty should automatically remap any `keys`, `columns` and `orderBy` inputs it receives if one or more column names have been remapped.
        /// Default is to return <see cref="AutoMap.On"/>.
        /// The type passed in is the class or subclass type for dynamic instances of <see cref="MightyOrm"/>
        /// and is the generic type T for generic instances of <see cref="MightyOrm{T}"/>.
        /// </summary>
        override public Func<Type, AutoMap> AutoMapAfterColumnRename { get; protected set; } = AlwaysAutoMap;

        /// <summary>
        /// Return <c>false</c> whatever type is sent in.
        /// </summary>
        /// <remarks>
        /// We need to be able to identify this one and tell users that they should not change it for dynamic <see cref="MightyOrm"/>.
        /// </remarks>
        public static readonly Func<Type, bool> UseCaseInsensitiveColumnMapping = (t) => false;

        /// <summary>
        /// Should <see cref="MightyOrm{T}"/> be case sensitive when matching returned data to class properties?
        /// Provided the data item type in case you need it.
        /// Default is to return <c>false</c> since many databases are case insensitive and use different case conventions from C#, by default.
        /// The type passed in is the class or subclass type for dynamic instances of <see cref="MightyOrm"/>
        /// and is the generic type T for generic instances of <see cref="MightyOrm{T}"/>.
        /// </summary>
        override public Func<Type, bool> CaseSensitiveColumns { get; protected set; } = UseCaseInsensitiveColumnMapping;
        #endregion

        #region Column-level features
        /// <summary>
        /// Identity column mapping.
        /// Takes data item type and field or property name, and returns field or property name unmodified.
        /// </summary>
        /// <remarks>
        /// We need to be able to identify this one and tell users that they should not change it for dynamic <see cref="MightyOrm"/>.
        /// </remarks>
        public static readonly Func<Type, string, string> IdentityColumnMapping = (t, n) => n;

        /// <summary>
        /// Function to get database column name from the data item type and field or property name.
        /// Default is to return name unmodified.
        /// Since incoming data in Mighty can come from any name-value collection, <see cref="MemberInfo"/>
        /// cannot always be provided and is left out to ensure consistent mapping.
        /// The type passed in is the class or subclass type for dynamic instances of <see cref="MightyOrm"/>
        /// and is the generic type T for generic instances of <see cref="MightyOrm{T}"/>.
        /// </summary>
        override public Func<Type, string, string> ColumnNameMapping { get; protected set; } = IdentityColumnMapping;

        /// <summary>
        /// Never ignore column.
        /// Takes data item type and field or property name, and always returns <c>false</c>.
        /// </summary>
        /// <remarks>
        /// We need to be able to identify this one and tell users that they should not change it for dynamic <see cref="MightyOrm"/>.
        /// </remarks>
        public static readonly Func<Type, string, bool> NeverIgnoreColumn = (t, n) => false;

        /// <summary>
        /// Function to determine whether to ignore database column based on the data item type and field or property name.
        /// Default is to return <c>false</c> for do not ignore.
        /// Since incoming data in Mighty can come from any name-value collection, <see cref="MemberInfo"/>
        /// cannot always be provided and is left out to ensure consistent mapping.
        /// The type passed in is the class or subclass type for dynamic instances of <see cref="MightyOrm"/>
        /// and is the generic type T for generic instances of <see cref="MightyOrm{T}"/>.
        /// </summary>
        override public Func<Type, string, bool> IgnoreColumn { get; protected set; } = NeverIgnoreColumn;

        /// <summary>
        /// Leave column data direction unspecified.
        /// Takes data item type and field or property name, and always returns <c>0</c>.
        /// </summary>
        /// <remarks>
        /// We need to be able to identify this one and tell users that they should not change it for dynamic <see cref="MightyOrm"/>.
        /// </remarks>
        public static readonly Func<Type, string, DataDirection> ColumnDataDirectionUnspecified = (t, n) => 0;

        /// <summary>
        /// Function to determine column data direction based on the data item type and field or property name.
        /// Default is to return <c>0</c> to leave direction unspecified.
        /// Since incoming data in Mighty can come from any name-value collection, <see cref="MemberInfo"/>
        /// cannot always be provided and is left out to ensure consistent mapping.
        /// The type passed in is the class or subclass type for dynamic instances of <see cref="MightyOrm"/>
        /// and is the generic type T for generic instances of <see cref="MightyOrm{T}"/>.
        /// </summary>
        override public Func<Type, string, DataDirection> ColumnDataDirection { get; protected set; } = ColumnDataDirectionUnspecified;
        #endregion

        #region Id mapping
        /// <summary>
        /// Function to perform database specific identifier quoting (such as "name" -> "[name]" or "name" -> "'name'").
        /// Default is to return the passed in string unmodified.
        /// You should handle quoting identifiers here only, or in <see cref="TableNameMapping"/> and <see cref="ColumnNameMapping"/> only, but not both.
        /// </summary>
        /// <remarks>
        /// TO DO: Might be useful to provide additional method which splits the name at the dots then rejoins it, with single overrideable method to quote the individual parts
        /// </remarks>
        override public Func<string, string> GetQuotedDatabaseIdentifier { get; protected set; } = (id) => id;
        #endregion

        #region Mapping utility method
        /// <summary>
        /// Useful alias which maps one or more comma-separated field names to comma-separated column names.
        /// Use this on mapped but not auto-mapped instances of <see cref="MightyOrm"/> and <see cref="MightyOrm{T}"/> to
        /// create the correct values to pass in for the `keys` parameter (applies to the constructor only), and for SQL
        /// fragments including the `columns` (constructor and methods) and `orderBy` (methods only) parameters.
        /// (Note: Use `new <see cref="SqlNamingMapper"/>()` as the mapper from which to call this method if you
        /// want to use this to apply attribute-based mapping but are not using any convention-based
        /// mapping of your own, i.e. are not using your own sub-class <see cref="SqlNamingMapper"/>.)
        /// </summary>
        /// <param name="classType">The item class type for the instance of Mighty (pass generic type T for generic instances of <see cref="MightyOrm{T}"/>;
        /// pass type of class or sub-class of <see cref="MightyOrm"/> itself for dynamic instances)</param>
        /// <param name="fieldNames">A comma-separated list of field names to be mapped to database column names</param>
        /// <param name="columns">This parameter is only required for auto-mapped dynamic instances of Mighty; in that case pass in the same `columns` value here which is passed to the constructor of <see cref="MightyOrm"/></param>
        /// <returns></returns>
        /// <remarks>
        /// It would be incorrect to throw an exception here even if the contract says that all auto-mapping is already being applied:
        ///   - It's true that it would be wrong for the user to apply this method to anything which will also be auto-mapped
        ///   - But here and there Mighty takes in SQL fragments other than `keys`, `columns` and `orderBy`, and the user
        ///     may still validly need this method then
        /// </remarks>
        override public string Map(Type classType, string fieldNames, string columns = null)
        {
            // check that we're legal
            if (classType == null)
            {
                // we cannot do the mapping without this
                throw new ArgumentNullException(nameof(classType));
            }

            if (classType == typeof(object) ||
                classType == typeof(ExpandoObject))
            {
                throw new InvalidOperationException(
                    $"To use {nameof(SqlNamingMapper)}.{nameof(SqlNamingMapper.Map)} with dyanamic instances of {nameof(MightyOrm)} pass the type of the class or user subclass of {nameof(MightyOrm)} that you are using, do not pass typeof(object) or typeof(ExpandoObject)");
            }

            // return null (and don't waste time looking up the contract) if we're null
            if (fieldNames == null)
            {
                return null;
            }

            // get the contract
            bool IsGeneric = (classType == typeof(MightyOrm) ||
                              classType
#if !NETFRAMEWORK
                              .GetTypeInfo()
#endif
                              .IsSubclassOf(typeof(MightyOrm)));

            DataContract dataContract = DataContractStore.Instance.Get(IsGeneric, classType, columns, this);

            // do the mapping
            return dataContract.Map(AutoMap.On, fieldNames);
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Parameterless constructor (overriding class can use protected setters)
        /// </summary>
        protected SqlNamingMapper() { }

        /// <summary>
        /// Constructor
        /// </summary>
        public SqlNamingMapper(
            Func<Type, bool> caseSensitiveColumns = null,
            Func<Type, string> tableNameMapping = null,
            Func<Type, string, string> columnNameMapping = null,
            Func<Type, string> getPrimaryKeyFieldNames = null,
            Func<Type, string> getSequenceName = null,
            Func<string, string> getQuotedDatabaseIdentifier = null)
        {
            if (caseSensitiveColumns != null) CaseSensitiveColumns = caseSensitiveColumns;
            if (tableNameMapping != null) TableNameMapping = tableNameMapping;
            if (columnNameMapping != null) ColumnNameMapping = columnNameMapping;
            if (getPrimaryKeyFieldNames != null) GetPrimaryKeyFieldNames = getPrimaryKeyFieldNames;
            if (getSequenceName != null) GetSequenceName = getSequenceName;
            if (getQuotedDatabaseIdentifier != null) GetQuotedDatabaseIdentifier = getQuotedDatabaseIdentifier;
        }
        #endregion
    }
}