using System;
using System.Data.Common;
using System.Reflection;

using Mighty.DatabasePlugins;

namespace Mighty.ConnectionProviders
{
	internal class MightyProviderFactories
	{
		private const string INSTANCE_FIELD_NAME = "Instance";

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
				throw new NotImplementedException("No " + INSTANCE_FIELD_NAME + " field/property found in intended DbProviderFactory class '" + factoryClassName + "'");
			}
			return (DbProviderFactory)f.GetValue(null);
		}

		// less readable but it loops through the enum, so you just need to add a new class with the
		// right name and add its name to the enum, and nothing else
		static private object GetProviderFactoryEnumOrClassName(string providerName, bool returnClassName)
		{
			string loweredProviderName = providerName.ToLowerInvariant();
			foreach (var db in Enum.GetValues(typeof(SupportedDatabase)))
			{
				Type type = DatabasePlugin.GetPluginType((SupportedDatabase)db);
				// string name = <type>.GetProviderFactoryClassName(loweredProviderName);
				string className = (string)type.GetMethod(nameof(DatabasePlugin.GetProviderFactoryClassName)).Invoke(null, new object[] { loweredProviderName });
				if (className != null)
				{
					if (returnClassName) return db;
					else return className;
				}
			}
			throw new InvalidOperationException("Unknown database provider: " + providerName);
		}

		static private string GetProviderFactoryClassName(string providerName)
		{
			return (string)GetProviderFactoryEnumOrClassName(providerName, false);
		}

		static internal SupportedDatabase GetSupportedDatabase(string providerName)
		{
			return (SupportedDatabase)GetProviderFactoryEnumOrClassName(providerName, true);
		}
	}
}