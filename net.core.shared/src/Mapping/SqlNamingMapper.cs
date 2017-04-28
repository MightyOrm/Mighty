namespace Mighty.Mapping
{
	// Using class not interface to allow for later extensions; and not an abstract class because we can define
	// a sensible default implementation.
	public class SqlNamingMapper
	{
		// instruct the microORM whether to use case-invariant mapping between db names and class names
		virtual public bool UseCaseInsensitiveMapping { get; protected set; } = true;
		// You could override this to establish, for example, the convention of using _ to separate schema/owner from table (just replace "_" with "." and return!)
		virtual public string GetTableName(string className) { return className; }
		// field and class provided to help with naming
		virtual public string GetColumnName(string className, string fieldName) { return fieldName; }
		virtual public string QuoteDatabaseName(string name) { return name; }
		// If this is not overridden then no primary key will be defined by default
		virtual public string GetPrimaryKeyName(string className) { return null; }
	}
}