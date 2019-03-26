using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Dynamic;
using System.Reflection;

using MightyOrm.Validation;

namespace MightyOrm.Parameters
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
		private ParameterDirection? _direction;
		private OrmAction? _action;

		internal ParameterInfo Current { get; set; }

		internal NameValueTypeEnumerator(object o, ParameterDirection? direction = null, OrmAction? action = null)
		{
			_o = o;
			_direction = direction;
			_action = action;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<LazyNameValueTypeInfo> GetEnumerator()
		{
			// enumerate empty list if null
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

			// This is for adding anonymous parameters (which will cause a different exception later on, in AddParam, if used on
			// a DB which doesn't support it; even on DBs which do support it, it only makes sense on input parameters).
			// NB This is not the same thing as the auto-named parameters added by AddParams(), which also use object[]
			// but with a different meaning (namely the values for the auto-named params @0, @1 or :0, :1 etc.).
			// Value-only processing now also used to support value-only collections of PK values when performing ORMAction on an item.
			if (valueArray != null)
			{
				string msg = null;
				if (_action != null)
				{
					if (_action != OrmAction.Delete)
					{
						msg = "Value-only collections not supported for action " + _action;
						if (_action == OrmAction.Update)
						{
							msg += "; use Update(item), not Update(item, pk)";
						}
					}
				}
				else
				{
					if (_direction != ParameterDirection.Input)
					{
						msg = "object[] arguments supported for input parameters only";
					}
				}
				if (msg != null)
				{
					throw new InvalidOperationException(msg);
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
				yield return new LazyNameValueTypeInfo(property.Name, () => property.GetValue(_o), property.PropertyType);
			}
		}
	}
}