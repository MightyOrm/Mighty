using System.Reflection;

namespace MightyTests
{
	static internal class ObjectExtensions
	{
#if NET40
		internal static void SetValue(this PropertyInfo prop, object obj, object value)
		{
			prop.SetValue(obj, value, null);
		}

		internal static object GetValue(this PropertyInfo prop, object obj)
		{
			return prop.GetValue(obj, null);
		}
#endif
	}
}
