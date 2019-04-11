using System;
using System.Reflection;

namespace Mighty.Mapping
{
	public class NullMapper : SqlNamingMapper
	{

	}

	/// <summary>
	/// Class to allow mapping between C# names and SQL names.
	/// </summary>
	/// <remarks>
	/// Using class not interface to allow for later extensions; and not an abstract class because we can define a sensible default implementation.
	/// </remarks>
	abstract public class SqlNamingMapper
	{
		/// <summary>
		/// Instruct the microORM whether or not to use case-insensitive mapping when mapping between db names and class names.
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
		/// Get database column name for C# field name. By default the unmodified property name is used.
		/// </summary>
		/// <param name="classType"></param>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		/// <remarks>The field can be from an ExpandoObject, so it might not have a PropertyInfo - which probably means we need typed and untyped mappers</remarks>
		virtual public string GetColumnNameFromPropertyName(Type classType, string propertyName) { return propertyName; }

		/// <summary>
		/// Get primary key field from class. Note that the C# field name (or names) should be returned.
		/// </summary>
		/// <param name="className"></param>
		/// <returns></returns>
		/// <remarks>TO DO: should be sent type, so it can look at namespace, etc.</remarks>
		virtual public string GetPrimaryKeyFieldFromClassName(string className) { return null; }

		/// <summary>
		/// Database specific quoting.
		/// You could handle quoting here or in <see cref="GetTableNameFromClassName(string)"/> and <see cref="GetColumnNameFromPropertyName(Type, string)"/>, but not both.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		/// <remarks>
		/// TO DO: needs virtual method which splits the name at the dots then rejoins it, with single overrideable method to quote the individual parts
		/// </remarks>
		virtual public string QuoteDatabaseIdentifier(string id) { return id; }
	}
}