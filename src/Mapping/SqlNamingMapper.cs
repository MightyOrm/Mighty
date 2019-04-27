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
    /// </summary>
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
        /// This method is passed
        /// the generic type T from subclasses or instances of <see cref="MightyOrm{T}"/>,
        /// the subclass from strict subclasses of <see cref="MightyOrm"/>, and not called otherwise.
        /// </summary>
        /// <param name="type">The class type</param>
        /// <returns></returns>
        /// <remarks>TO DO: should be sent type, so it can look at namespace, etc.</remarks>
        abstract public string GetTableName(Type type);

        /// <summary>
        /// Get database column name from C# field or property name.
        /// Default is to return <paramref name="name"/> unmodified.
        /// This method is passed
        /// the generic type T from subclasses or instances of <see cref="MightyOrm{T}"/>,
        /// the subclass from strict subclasses of <see cref="MightyOrm"/>, and not called otherwise.
        /// </summary>
        /// <param name="type">The class type</param>
        /// <param name="member">The field or property</param>
        /// <param name="name">The property name</param>
        /// <returns></returns>
        /// <remarks>The field can be from an ExpandoObject, so it might not have a PropertyInfo - which probably means we need typed and untyped mappers</remarks>
        abstract public string GetColumnName(Type type, MemberInfo member, string name);

        /// <summary>
        /// Derive the primary key field name(s) from C# class type.
        /// Note that C# field/property name(s) should be returned and not database column names (if these are different).
        /// Default is to return <c>null</c>.
        /// This method is passed
        /// the generic type T from subclasses or instances of <see cref="MightyOrm{T}"/>,
        /// the subclass from strict subclasses of <see cref="MightyOrm"/>, and not called otherwise.
        /// </summary>
        /// <param name="type">The class type</param>
        /// <returns></returns>
        abstract public string GetPrimaryKeysFromClassType(Type type);

        /// <summary>
        /// Get the sequence from the class type. Only applicable to sequence-based databases (Oracle and Postgres),
        /// or if you need to override the default identity function (see Mighty documentation).
        /// </summary>
        /// <param name="type">The class type</param>
        /// <returns></returns>
        abstract public string GetSequenceFromClassType(Type type);

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