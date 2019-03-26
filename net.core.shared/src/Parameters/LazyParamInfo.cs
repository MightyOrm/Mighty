using System;

namespace MightyOrm.Parameters
{
	internal class LazyNameValueTypeInfo
	{
		/// <summary>
		/// The parameter name.
		/// </summary>
		/// <returns></returns>
		internal string Name { get; }

		private Func<object> _lazyValue;
		private object _value;
		/// <summary>
		/// The parameter value; lazy evaluated.
		/// </summary>
		/// <returns></returns>
		internal object Value
		{
			get
			{
				if (_value == null)
				{
					_value = _lazyValue();
				}
				return _value;
			}
		}

		private Type _type;
		/// <summary>
		/// The parameter type; can be explicitly specified or lazily evaluated from the parameter value.
		/// </summary>
		/// <returns></returns>
		internal Type Type
		{
			get
			{
				if (_type == null && _value != null)
				{
					_type = _value.GetType();
				}
				return _type;
			}
		}

		/// <summary>
		/// Object to help with unpacking names, values and types from arbitrary objects or collections.
		/// </summary>
		/// <param name="name">Name</param>
		/// <param name="lazyValue">Function to lazily evaluate value</param>
		/// <param name="type">Type (if available)</param>
		internal LazyNameValueTypeInfo(string name, Func<object> lazyValue, Type type = null)
		{
			Name = name;
			_lazyValue = lazyValue;
			_type = type;
		}
	}
}