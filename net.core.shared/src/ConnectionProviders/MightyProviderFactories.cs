using System;
using System.Data;
using System.Data.Common;
using System.Reflection;

using Mighty.DatabasePlugins;

namespace Mighty.ConnectionProviders
{
	internal class MightyProviderFactories
	{
		private const string INSTANCE_FIELD_NAME = "Instance";

		internal static DbProviderFactory GetFactory(string providerName)
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

		private static string GetProviderFactoryClassName(string providerName)
		{
			string loweredProviderName = providerName.ToLowerInvariant();
			string result = MySQL.GetProviderFactoryClassName(loweredProviderName);
			if (result == null) result = Oracle.GetProviderFactoryClassName(loweredProviderName);
			if (result == null) result = PostgreSQL.GetProviderFactoryClassName(loweredProviderName);
			if (result == null) result = SQLite.GetProviderFactoryClassName(loweredProviderName);
			if (result == null) result = SQLServer.GetProviderFactoryClassName(loweredProviderName);
			if (result == null) throw new InvalidOperationException("Unknown database provider: " + providerName);
			return result;
		}

		internal static SupportedDatabase GetSupportedDatabase(string providerName)
		{
			string loweredProviderName = providerName.ToLowerInvariant();
			if (MySQL.GetProviderFactoryClassName(loweredProviderName) != null) return SupportedDatabase.MySQL;
			else if (Oracle.GetProviderFactoryClassName(loweredProviderName) != null) return SupportedDatabase.Oracle;
			else if (PostgreSQL.GetProviderFactoryClassName(loweredProviderName) != null) return SupportedDatabase.PostgreSQL;
			else if (SQLite.GetProviderFactoryClassName(loweredProviderName) != null) return SupportedDatabase.SQLite;
			else if (SQLServer.GetProviderFactoryClassName(loweredProviderName) != null) return SupportedDatabase.SQLServer;
			else throw new InvalidOperationException("Unknown database provider: " + providerName);
		}
	}
}