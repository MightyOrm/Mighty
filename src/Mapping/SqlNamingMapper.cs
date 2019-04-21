using System;
using System.Reflection;

namespace Mighty.Mapping
{
    /// <summary>
    /// Implement this abstract class and pass an instance of it to the constructor of <see cref="MightyOrm"/> in order to get Mighty
    /// to do mapping between C# field names and SQL column names.
    /// All methods have sensible default implementations, so it is up to you what you override.
    /// </summary>
    abstract public class SqlNamingMapper
    {
        /// <summary>
        /// Instruct Mighty whether or not to use case-insensitive mapping when mapping between db names and class names.
        /// Defaults to true (typically DBs use different case conventions from C#).
        /// </summary>
        virtual public bool UseCaseInsensitiveMapping { get; protected set; } = true;

        /// <summary>
        /// You could override this to establish, for example, the convention of using _ to separate schema/owner from table (just replace "_" with
        /// "." and return), or maybe to use the C# namespace structure for the same purpose. By default the unmodified class name is used.
        /// </summary>
        /// <param name="className">Will be sent the class name of your data class</param>
        /// <returns></returns>
        /// <remarks>TO DO: should be sent type, so it can look at namespace, etc.</remarks>
        virtual public string GetTableNameFromClassName(string className) { return className; }

        /// <summary>
        /// Get database column name from C# property name. By default the unmodified property name is used.
        /// </summary>
        /// <param name="classType">The class type</param>
        /// <param name="propertyName">The property name</param>
        /// <returns></returns>
        /// <remarks>The field can be from an ExpandoObject, so it might not have a PropertyInfo - which probably means we need typed and untyped mappers</remarks>
        virtual public string GetColumnNameFromPropertyName(Type classType, string propertyName) { return propertyName; }

        /// <summary>
        /// Automatically derive the primary key field name(s) from an items C# class name.
        /// Note that the relevant C# property name(s) should be returned (and not the database column names
        /// if these are also mapped).
        /// </summary>
        /// <param name="className">The class name</param>
        /// <returns></returns>
        /// <remarks>TO DO: should be sent type, so it can look at namespace, etc.</remarks>
        virtual public string GetPrimaryKeyFieldFromClassName(string className) { return null; }

        /// <summary>
        /// Perform database specific quoting (such as name -> [name] or name -> 'name').
        /// You might handle quoting here or in <see cref="GetTableNameFromClassName(string)"/> and <see cref="GetColumnNameFromPropertyName(Type, string)"/>, but not both.
        /// </summary>
        /// <param name="id">The name to be quoted</param>
        /// <returns></returns>
        /// <remarks>
        /// TO DO: needs virtual method which splits the name at the dots then rejoins it, with single overrideable method to quote the individual parts
        /// </remarks>
        virtual public string QuoteDatabaseIdentifier(string id) { return id; }
    }
}