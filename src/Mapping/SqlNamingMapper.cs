using System;
using System.Linq;
using System.Reflection;

namespace Mighty.Mapping
{
    /// <summary>
    /// Pass an instance of this class to the constructor of <see cref="MightyOrm"/> in order to
    /// map between C# field names and SQL column names.
    /// If you're not (yet) used to <see cref="Action"/>/<see cref="Func{T, TResult}"/> syntax in C#, you may find
    /// slightly harder to set up this mapper than if it had just been a class with methods you can override (see
    /// Mighty documentation for examples). One reason for doing it like this is that Mighty can then do much more
    /// aggressive and successful caching of its data contracts, by checking whether the mapping functions (not just
    /// the whole mapper) are the same.
    /// </summary>
    public class SqlNamingMapper
    {
        /// <summary>
        /// Return false whatever type is sent in.
        /// </summary>
        /// <remarks>
        /// We need to be able to identify this one and tell users that they should not change it for dynamic <see cref="MightyOrm"/>.
        /// </remarks>
        public static readonly Func<Type, bool> CaseInsensitiveColumnMapping = (t) => false;

        /// <summary>
        /// Should <see cref="MightyOrm{T}"/> be case sensitive when matching returned data to class properties?
        /// Provided the data item type in case you need it.
        /// Default is to return <c>false</c> since many databases are case insensitive and use different case conventions from C#, by default.
        /// Not applicable to dynamic <see cref="MightyOrm"/> (use e.g. `columns: "description AS Description"` in the constructor instead).
        /// The type passed in is the <see cref="MightyOrm{Type}"/> generic type.
        /// </summary>
        public Func<Type, bool> CaseSensitiveColumnMapping { get; protected set; } = CaseInsensitiveColumnMapping;

        /// <summary>
        /// Function to get database table name from the data item type.
        /// Default is to return <see cref="Type"/>.Name unmodified.
        /// The type passed in is the <see cref="MightyOrm"/> subclass type for dynamic instances of Mighty
        /// (or null if this is not a subclass), and is the generic type for generic instances of <see cref="MightyOrm{T}"/>.
        /// </summary>
        public Func<Type, string> TableName { get; protected set; } = (t) => t.Name;

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
        /// Not applicable to dynamic <see cref="MightyOrm"/> (use e.g. `columns: "film_id AS FilmID"` in the constructor instead).
        /// The type passed in is the <see cref="MightyOrm{Type}"/> generic type.
        /// </summary>
        public Func<Type, string, string> ColumnName { get; protected set; } = IdentityColumnMapping;

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
        /// Cannot be used with dynamic <see cref="MightyOrm"/>.
        /// The type passed in is the <see cref="MightyOrm{Type}"/> generic type.
        /// </summary>
        public Func<Type, string, bool> IgnoreColumn { get; protected set; } = NeverIgnoreColumn;

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
        /// Cannot be used with dynamic <see cref="MightyOrm"/>.
        /// The type passed in is the <see cref="MightyOrm{Type}"/> generic type.
        /// </summary>
        public Func<Type, string, DataDirection> ColumnDataDirection { get; protected set; } = ColumnDataDirectionUnspecified;

        /// <summary>
        /// Function to get primary key field name(s) from the data item type and field or property name.
        /// The exact C# field/property name(s) should be returned and not database column names (where these are different).
        /// The default behaviour is to return <c>null</c> which results in no primary keys being specified in this way -
        /// they may still be specified using the <see cref="MightyOrm"/> `keys` constructor parameter.
        /// Applicable to dynamic and non-dynamic instances of Mighty.
        /// The type passed in is the <see cref="MightyOrm"/> subclass type for dynamic instances of Mighty
        /// (or null if this is not a subclass), and is the generic type for generic instances of <see cref="MightyOrm{T}"/>.
        /// </summary>
        public Func<Type, string> PrimaryKeyFieldNames { get; protected set; } = (t) => null;

        /// <summary>
        /// Function to get the sequence from the data item type.
        /// Generally only applicable to sequence-based databases (Oracle and Postgres), except in the rare case where
        /// you may need to override the default identity function on identity-based databases (see Mighty documentation).
        /// Applicable to dynamic and non-dynamic instances of Mighty.
        /// The type passed in is the <see cref="MightyOrm"/> subclass type for dynamic instances of Mighty
        /// (or null if this is not a subclass), and is the generic type for generic instances of <see cref="MightyOrm{T}"/>.
        /// </summary>
        public Func<Type, string> SequenceName { get; protected set; } = (t) =>  null;

        /// <summary>
        /// Function to perform database specific identifier quoting (such as "name" -> "[name]" or "name" -> "'name'").
        /// Default is to return the passed in string unmodified.
        /// You should handle quoting identifiers here only, or in <see cref="TableName"/> and <see cref="ColumnName"/> only, but not both.
        /// </summary>
        /// <remarks>
        /// TO DO: Might be useful to provide additional method which splits the name at the dots then rejoins it, with single overrideable method to quote the individual parts
        /// </remarks>
        public Func<string, string> QuotedDatabaseIdentifier { get; protected set; } = (id) => id;

        /// <summary>
        /// Useful alias which maps one or more field names in a comma separated list using the <see cref="SqlNamingMapper.ColumnName"/> function.
        /// Useful to help convert one more more field/property names to column names, when creating SQL fragments
        /// (including the `columns` parameter) to pass in to <see cref="MightyOrm"/>.
        /// </summary>
        /// <param name="classType">The class type</param>
        /// <param name="fieldNames">The property name</param>
        /// <returns></returns>
        public string Map(Type classType, string fieldNames)
        {
            return string.Join(", ", fieldNames.Split(',').Select(n => n.Trim()).Select(n => ColumnName(classType, n)));
        }

        /// <summary>
        /// Parameterless constructor (overriding class can use protected setters)
        /// </summary>
        protected SqlNamingMapper() { }

        /// <summary>
        /// Constructor
        /// </summary>
        public SqlNamingMapper(
            Func<Type, bool> caseSensitiveColumnMapping = null,
            Func<Type, string> tableName = null,
            Func<Type, string, string> columnName = null,
            Func<Type, string> primaryKeyFieldNames = null,
            Func<Type, string> sequenceName = null,
            Func<string, string> quotedDatabaseIdentifier = null)
        {
            if (caseSensitiveColumnMapping != null) CaseSensitiveColumnMapping = caseSensitiveColumnMapping;
            if (tableName != null) TableName = tableName;
            if (columnName != null) ColumnName = columnName;
            if (primaryKeyFieldNames != null) PrimaryKeyFieldNames = primaryKeyFieldNames;
            if (sequenceName != null) SequenceName = sequenceName;
            if (quotedDatabaseIdentifier != null) QuotedDatabaseIdentifier = quotedDatabaseIdentifier;
        }
    }
}