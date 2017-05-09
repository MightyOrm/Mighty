using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Dynamic;
using System.Reflection;

namespace Mighty.Parameters
{
	/// <remarks>
	/// <see cref="NameValueCollection"/> *is* supported in .NET Core 1.1, but got a bit lost:
	/// https://github.com/dotnet/corefx/issues/10338
	/// For folks that hit missing types from one of these packages after upgrading to Microsoft.NETCore.UniversalWindowsPlatform they can reference the packages directly as follows.
	/// "System.Collections.NonGeneric": "4.0.1",
	/// "System.Collections.Specialized": "4.0.1", ****
	/// "System.Threading.Overlapped": "4.0.1",
	/// "System.Xml.XmlDocument": "4.0.1"
	/// </remarks>
	internal class NameValueTypeEnumerator : IEnumerable<LazyNameValueTypeInfo>, IEnumerable
	{
		private object _o;
		private ParameterDirection _direction;

		internal ParameterInfo Current { get; set; }

		// We don't default to output parameters as such, but we do default to complaining
		// if object[] is passed in in any context except for directional parameters with
		// the direction at Input (see <see cref="GetEnumerator" />)
		internal NameValueTypeEnumerator(object o, ParameterDirection direction = ParameterDirection.Output)
		{
			_o = o;
			_direction = direction;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<LazyNameValueTypeInfo> GetEnumerator()
		{
			// enumerate nothing if null
			if (_o == null)
			{
				yield break;
			}

			var o = _o as ExpandoObject;
			if (o != null)
			{
				foreach (var pair in o)
				{
					yield return new LazyNameValueTypeInfo(pair.Key, () => pair.Value);
				}
				yield break;
			}

			var nvc = _o as NameValueCollection;
			if (nvc != null)
			{
				foreach (string name in nvc)
				{
					yield return new LazyNameValueTypeInfo(name, () =>  nvc[name]);
				}
				yield break;
			}

			// possible support for Newtonsoft JObject here...

			// Convert non-class objects to value array
			object[] valueArray = null;
			var type = _o.GetType();
			if (!type
#if !NETFRAMEWORK
				.GetTypeInfo()
#endif
				.IsClass)
			{
				valueArray = new object[] { _o };
			}
			else
			{
				valueArray = _o as object[];
			}

			// This is for adding anonymous parameters (this will cause an exception later on, in AddParam, if used on
			// a DB which doesn't support it; even on DBs which do support it, it only makes sense on input parameters).
			// NB This is not the same thing as the auto-named parameters added by AddParams() which also uses object[],
			// but with a different meaning (namely the values for auto-named params @0, @1 or :0, :1 etc.).
			// Value only processing now used to support value-only collections of PK values when performing ORMAction on an item.
			if (valueArray != null)
			{
				if (_direction != ParameterDirection.Input)
				{
					throw new InvalidOperationException("object[] arguments supported for input parameters only");
				}
				// anonymous parameters from array
				foreach (var value in valueArray)
				{
					// string.Empty not null is needed in AddParam
					yield return new LazyNameValueTypeInfo(string.Empty, () => value);
				}
				yield break;
			}

			// names, values and types from properties of anonymous object or POCOs
			foreach (PropertyInfo property in _o.GetType().GetProperties())
			{
				yield return new LazyNameValueTypeInfo(property.Name, () => property.GetValue(_o, null), property.PropertyType);
			}
		}
	}
}