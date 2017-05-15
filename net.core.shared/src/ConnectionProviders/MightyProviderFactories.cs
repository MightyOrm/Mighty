using System;
using System.Data.Common;
using System.Reflection;

using Mighty.DatabasePlugins;

namespace Mighty.ConnectionProviders
{
	// A replacement for System.Data.Common.DbProvider factories!
	//
	// we really need to look at some kind of mocking (both in terms of whether all public methods on MightyORM should be virtual, to help this; okay, actually,
	// that's trivial, isn't it? if it's virtual, you can make a mock version which does what you want; HOWEVER - if we have correctly wrapped everything in
	// abstract class interfaces then we don't need to do that, as they can implement the interfaces instead (and they don't even need to fart around with our
	// constructor).
	//
	// But then why I thought of it here, in terms of Frans Bouma's approach to testing.
	// It would be *really* nice to develop some kind of test wrapper to factories (how on earth do you do that?) which really does trap the very
	// final point before action on every type of db command, and prints out the SQL and params which it's going to send.
	//
	internal class MightyProviderFactories
	{
		private const string INSTANCE_FIELD_NAME = "Instance";

#if !NETFRAMEWORK
		static internal DbProviderFactory GetFactory(string providerName)
		{
			string assemblyName = null;
			var factoryClass = GetProviderFactoryClassName(providerName);
			string[] elements = factoryClass.Split(',');
			string factoryClassName = elements[0];
			if (elements.Length > 1)
			{
				assemblyName = elements[1];
			}
			else
			{
				assemblyName = factoryClassName.Substring(0, factoryClassName.LastIndexOf("."));
			}
			var assemblyNameClass = new AssemblyName(assemblyName);
			Type type = Assembly.Load(assemblyNameClass).GetType(factoryClassName);
			var f = type.GetField(INSTANCE_FIELD_NAME);
			if (f == null)
			{
				throw new FieldAccessException("No '" + INSTANCE_FIELD_NAME + "' field found in intended DbProviderFactory class '" + factoryClassName + "'");
			}
			return (DbProviderFactory)f.GetValue(null);
		}
#endif

		static private object GetProviderFactoryClassNameOrDatabasePluginType(string providerName, bool returnClassType)
		{
			string loweredProviderName = providerName.ToLowerInvariant();
			foreach (var type in DatabasePluginManager.GetInstalledPluginTypes())
			{
				// The DatabasePlugin subclass must implement this static GetProviderFactoryClassName method and will
				// throw a reasonably meaningful exception here if not; unfortunately there is no syntax to enforce this
				// at complile time.
				var methodName = nameof(DatabasePlugin.GetProviderFactoryClassName);
				var method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic);
				if (method == null)
				{
					throw new Exception(type + " does not implement static internal " + methodName);
				}
				string className = (string)method.Invoke(null, new object[] { loweredProviderName });
				if (className != null)
				{
					if (returnClassType) return type;
					else return className;
				}
			}
			throw new InvalidOperationException("Unknown database provider: " + providerName);
		}

		static private string GetProviderFactoryClassName(string providerName)
		{
			return (string)GetProviderFactoryClassNameOrDatabasePluginType(providerName, false);
		}

		static internal Type GetDatabasePluginAsType(string providerName)
		{
			return (Type)GetProviderFactoryClassNameOrDatabasePluginType(providerName, true);
		}
	}
}