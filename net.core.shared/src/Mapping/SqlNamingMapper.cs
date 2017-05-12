namespace Mighty.Mapping
{
	// Using class not interface to allow for later extensions; and not an abstract class because we can define
	// a sensible default implementation.
	public class SqlNamingMapper
	{
		// instruct the microORM whether to use case-invariant mapping between db names and class names
		virtual public bool UseCaseInsensitiveMapping { get; protected set; } = true;
		// You could override this to establish, for example, the convention of using _ to separate schema/owner from table (just replace "_" with "." and return!)
		virtual public string GetTableNameFromClassName(string className) { return className; }
		// field and class provided to help with naming
		virtual public string GetColumnNameFromPropertyName(string className, string propertyName) { return propertyName; }
		// If this is not overridden then no primary key will be defined by default
		virtual public string GetPrimaryKeyNameFromClassName(string className) { return null; }
		// TO DO: virtual method to split the name at the dots rejoin it, with single overrideable method to quote the individual parts
		virtual public string QuoteDatabaseIdentifier(string id) { return id; }
	}
}