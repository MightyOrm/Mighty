using System;

namespace Mighty.Parameters
{
	internal class LazyParamInfo
	{
		internal string Name { get; }
		private Func<object> _lazyValue;
		internal object Value { get {return _lazyValue();} }
		internal Type Type { get; }

		internal LazyParamInfo(string name, Func<object> lazyValue, Type type = null)
		{
			Name = name;
			_lazyValue = lazyValue;
			Type = type;
		}
	}
}