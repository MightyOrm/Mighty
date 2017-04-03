using System;
using System.Data;
using System.Data.Common;

namespace Mighty
{
	// There is no need to make these extension public (access modifiers on extension methods are relative to the package they are defined in,
	// not relative to the package which they extend); making them public turns them into utilty methods provided as part of the microORM.
	public static partial class PublicExtensions
	{
		internal static IEnumerable<dynamic> YieldResult(this DbDataReader rdr)
		{
			while(rdr.Read())
			{
				yield return rdr.RecordToExpando();
			}
		}

		/// <remarks>
		/// This supports all the types listed in ADO.NET DbParameter type-inference documentation https://msdn.microsoft.com/en-us/library/yy6y35y8(v=vs.110).aspx , except for byte[] and Object.
		/// Although this method supports all these types, the various ADO.NET providers do not:
		/// None of the providers support DbType.UInt16/32/64; Oracle and Postgres do not support DbType.Guid or DbType.Boolean.
		/// Setting DbParameter DbType or Value to one of the per-provider non-supported types will produce an ArgumentException
		/// (immediately on Postgres and Oracle, at DbCommand execution time on SQL Server).
		/// The per-database method DbParameter.SetValue is the place to add code to convert these non-supported types to supported types.
		///
		/// Not sure whether this should be public...?
		/// </remarks>
		public static object CreateInstance(this Type type)
		{
			Type underlying = Nullable.GetUnderlyingType(type);
			if(underlying != null)
			{
				return Activator.CreateInstance(underlying);
			}
#if COREFX
			if(type.GetTypeInfo().IsValueType)
#else
			if(type.IsValueType)
#endif
			{
				return Activator.CreateInstance(type);
			}
			if(type == typeof(string))
			{
				return "";
			}
			throw new InvalidOperationException("CreateInstance does not support type " + type);
		}

		public static void SetRuntimeEnumProperty(this object o, string enumPropertyName, string enumStringValue, bool throwException = true)
		{
			// Both the property lines can be simpler in .NET 4.5
			PropertyInfo pinfoEnumProperty = o.GetType().GetProperties().Where(property => property.Name == enumPropertyName).FirstOrDefault();
			if(pinfoEnumProperty == null && throwException == false)
			{
				return;
			}
			pinfoEnumProperty.SetValue(o, Enum.Parse(pinfoEnumProperty.PropertyType, enumStringValue), null);
		}

		public static string GetRuntimeEnumProperty(this object o, string enumPropertyName)
		{
			// Both these lines can be simpler in .NET 4.5
			PropertyInfo pinfoEnumProperty = o.GetType().GetProperties().Where(property => property.Name == enumPropertyName).FirstOrDefault();
			return pinfoEnumProperty == null ? null : pinfoEnumProperty.GetValue(o, null).ToString();
		}

		// Not sure whether this is useful or not... syntax is slightly nicer, and saves a little typing, even though functionality is obviously very simple.
		// Presumably compiler removes any apparent inefficiency.
		public static IDictionary<string, object> ToDictionary(this ExpandoObject object)
		{
			return (IDictionary<string, object>)object;
		}
	}
}