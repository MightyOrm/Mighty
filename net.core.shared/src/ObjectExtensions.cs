using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

using Mighty.Parameters;

namespace Mighty
{
	// There is no need to make these extensions public (note that access modifiers on extension methods are relative to the package they are defined in,
	// not relative to the package which they extend); making some of them public turns them into utilty methods which are provided as part of the microORM.
	static public partial class ObjectExtensions
	{
		static public dynamic ToExpando(this object o)
		{
			var oAsExpando = o as ExpandoObject;
			if (oAsExpando != null) return oAsExpando;
			var result = new ExpandoObject();
			var dict = result.AsDictionary();
			foreach (var info in new NameValueTypeEnumerator(o))
			{
				dict.Add(info.Name, info.Value);
			}
			return result;
		}

		// Not sure whether this is really useful or not... syntax is nicer and saves a little typing, even though functionality is obviously very simple.
		// Hopefully compiler removes any apparent inefficiency.
		static public IDictionary<string, object> AsDictionary(this ExpandoObject o)
		{
			return (IDictionary<string, object>)o;
		}

		static internal string Unthingify(this string sql, string thing)
		{
			return Thingify(sql, thing, false);
		}

		static internal string Thingify(this string sql, string thing, bool yes = true)
		{
			if (sql == null) return string.Empty;
			sql = sql.Trim();
			if (sql == string.Empty) return string.Empty;
			if (sql.Length > thing.Length &&
				sql.StartsWith(thing, StringComparison.OrdinalIgnoreCase) &&
				string.IsNullOrWhiteSpace(sql.Substring(thing.Length, 1)))
			{
				return yes ? string.Format(" {0}", sql) : sql.Substring(thing.Length + 1).Trim();
			}
			else
			{
				return yes ? string.Format(" {0} {1}", thing, sql) : sql;
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
		static internal object GetDefaultValue(this Type type)
		{
			Type underlying = Nullable.GetUnderlyingType(type);
			if(underlying != null)
			{
				return Activator.CreateInstance(underlying);
			}
#if NETFRAMEWORK
			if (type.IsValueType)
#else
			if (type.GetTypeInfo().IsValueType)
#endif
			{
				return Activator.CreateInstance(type);
			}
			if (type == typeof(string))
			{
				return "";
			}
			throw new InvalidOperationException(nameof(GetDefaultValue) + " does not support Type=" + type);
		}

		static internal void SetRuntimeEnumProperty(this object o, string enumPropertyName, string enumStringValue, bool throwException = true)
		{
			// Both the property lines can be simpler in .NET 4.5
			PropertyInfo pinfoEnumProperty = o.GetType().GetProperties().Where(property => property.Name == enumPropertyName).FirstOrDefault();
			if(pinfoEnumProperty == null && throwException == false)
			{
				return;
			}
			pinfoEnumProperty.SetValue(o, Enum.Parse(pinfoEnumProperty.PropertyType, enumStringValue), null);
		}

		static internal string GetRuntimeEnumProperty(this object o, string enumPropertyName)
		{
			// Both these lines can be simpler in .NET 4.5
			PropertyInfo pinfoEnumProperty = o.GetType().GetProperties().Where(property => property.Name == enumPropertyName).FirstOrDefault();
			return pinfoEnumProperty == null ? null : pinfoEnumProperty.GetValue(o, null).ToString();
		}
	}
}