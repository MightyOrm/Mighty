///////////////////////////////////////////////////////////////////////////////////////////////////
// At the moment, the methods in this class are the same as, or only _very_ lightly modified from
// methods in Massive, which is released under the same New BSD License as MightyORM.
//
// Massive is copyright (c) 2009-2017 various contributors.
// All rights reserved.
// See for sourcecode, full history and contributors list: https://github.com/FransBouma/Massive
//
///////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Mighty
{
	static public partial class ObjectExtensions
	{
		public static dynamic RecordToExpando(this DbDataReader reader)
		{
			dynamic e = new ExpandoObject();
			var d = (IDictionary<string, object>)e;
			object[] values = new object[reader.FieldCount];
			reader.GetValues(values);
			for(int i = 0; i < values.Length; i++)
			{
				var v = values[i];
				d.Add(reader.GetName(i), DBNull.Value.Equals(v) ? null : v);
			}
			return e;
		}
		
		public static dynamic ToExpando(this object o)
		{
			if(o is ExpandoObject)
			{
				return o;
			}
			var result = new ExpandoObject();
			var d = (IDictionary<string, object>)result; //work with the Expando as a Dictionary
#if !COREFX
			if(o.GetType() == typeof(NameValueCollection) || o.GetType().GetTypeInfo().IsSubclassOf(typeof(NameValueCollection)))
			{
				var nv = (NameValueCollection)o;
				nv.Cast<string>().Select(key => new KeyValuePair<string, object>(key, nv[key])).ToList().ForEach(i => d.Add(i));
			}
			else
#endif
			{
				var props = o.GetType().GetProperties();
				foreach(var item in props)
				{
					d.Add(item.Name, item.GetValue(o, null));
				}
			}
			return result;
		}
	}
}