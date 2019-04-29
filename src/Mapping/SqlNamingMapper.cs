using System;
using System.Linq;
using System.Reflection;

namespace Mighty.Mapping
{
    /// <summary>
    /// Extension methods for <see cref="SqlNamingMapper"/>.
    /// </summary>
    static public partial class ObjectExtensions
    {
        /// <summary>
        /// Useful alias which maps one or more field names in a comma separated list
        /// using <see cref="SqlNamingMapper.GetColumnName(Type, string)"/>.
        /// This should be useful to help create SQL fragments to pass in to <see cref="MightyOrm{T}"/>.
        /// </summary>
        /// <param name="mapper">The <see cref="SqlNamingMapper"/></param>
        /// <param name="classType">The class type</param>
        /// <param name="fieldNames">The property name</param>
        /// <returns></returns>
        static public string Map(this SqlNamingMapper mapper, Type classType, string fieldNames)
        {
            return string.Join(", ", fieldNames.Split(',').Select(n => n.Trim()).Select(n => mapper.GetColumnName(classType, n)));
        }
    }

    /// <summary>
    /// Implement this abstract class and pass an instance of it to the constructor of <see cref="MightyOrm"/> in order to get Mighty
    /// to do mapping between C# field names and SQL column names.
    /// NB In order for Mighty to be able to cache mappings between data classes and the database, you should make
    /// just one instance of each custom data contract which you define and then re-use it.
    /// </summary>
    /// <remarks>
    /// TO DO: If I make this use functions instead of being done by overriding, then we can make caching work
    /// even if the user creates multiple instances, as long as they use the very same functions (by defining
    /// hascode and equals for this class), which is much more likely to be done correctly, and much more correct.
    /// </remarks>
    abstract public class SqlNamingMapper
    {
        /// <summary>
        /// Instruct Mighty whether or not to use case-insensitive mapping when mapping between db names and class names.
        /// Defaults is to return <c>false</c> (typically DBs use different case conventions from C#).
        /// </summary>
        abstract public bool UseCaseSensitiveMapping();

        /// <summary>
        /// Get database table name from C# class type.
        /// Default is to return <see cref="Type"/>.Name unmodified.
        /// </summary>
        /// <param name="type">The class type (for dynamic instances of Mighty this is the type of
        /// the user sub-class of <see cref="MightyOrm"/>, if any, or else this won't be called;
        /// for generic instances of <see cref="MightyOrm{T}"/> it is the generic type)</param>
        /// <returns></returns>
        abstract public string GetTableName(Type type);

        /// <summary>
        /// Get database column name from C# field or property name.
        /// Default is to return <paramref name="name"/> unmodified.
        /// Incoming data in Mighty can come from any name-value collection, so <see cref="MemberInfo"/>
        /// cannot always be provided and is left out to ensure consistent mapping.
        /// </summary>
        /// <param name="type">The class type (for dynamic instances of Mighty, this is the type of
        /// the user sub-class of <see cref="MightyOrm"/>, if any, or else null;
        /// for generic instances of <see cref="MightyOrm{T}"/> it is the generic type)</param>
        /// <param name="name">The property name</param>
        /// <returns></returns>
        abstract public string GetColumnName(Type type, string name);

        /// <summary>
        /// Get primary key field name(s) from C# class type.
        /// Note that exact C# field/property name(s) should be returned and not database column names (where these are different).
        /// The default behaviour is to return <c>null</c> for no primary keys specified this way -
        /// in which case they may still be specified in the `keys` constructor parameter.
        /// </summary>
        /// <param name="type">The class type (for dynamic instances of Mighty, this is the type of
        /// the user sub-class of <see cref="MightyOrm"/>, if any, or else this won't be called;
        /// for generic instances of <see cref="MightyOrm{T}"/> it is the generic type)</param>
        /// <returns></returns>
        abstract public string GetPrimaryKeyFieldNames(Type type);

        /// <summary>
        /// Get the sequence from the class type. Generally only applicable to sequence-based databases (Oracle and Postgres),
        /// except in the rare case where you may need to override the default identity function on identity-based
        /// databases (see Mighty documentation).
        /// </summary>
        /// <param name="type">The class type (for dynamic instances of Mighty, this is the type of
        /// the user sub-class of <see cref="MightyOrm"/>, if any, or else this won't be called;
        /// for generic instances of <see cref="MightyOrm{T}"/> it is the generic type)</param>
        /// <returns></returns>
        abstract public string GetSequenceName(Type type);

        /// <summary>
        /// Perform database specific quoting (such as "name" -> "[name]" or "name" -> "'name'").
        /// Default is to return <paramref name="id"/> unmodified.
        /// You should handle quoting identifiers here only, or in <see cref="GetTableName(Type)"/> and <see cref="GetColumnName(Type, string)"/>, but not both.
        /// </summary>
        /// <param name="id">The name to be quoted</param>
        /// <returns></returns>
        /// <remarks>
        /// TO DO: needs abstract method which splits the name at the dots then rejoins it, with single overrideable method to quote the individual parts
        /// </remarks>
        abstract public string QuoteDatabaseIdentifier(string id);
    }
}