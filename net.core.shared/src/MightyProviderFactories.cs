using System;
using System.Data;
using System.Data.Common;

using Mighty.Plugin;

namespace Mighty
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
			string result = MySql.GetProviderFactoryClassName(loweredProviderName);
			if (result == null) result = Oracle.GetProviderFactoryClassName(loweredProviderName);
			if (result == null) result = PostgreSql.GetProviderFactoryClassName(loweredProviderName);
			if (result == null) result = Sqlite.GetProviderFactoryClassName(loweredProviderName);
			if (result == null) result = SqlServer.GetProviderFactoryClassName(loweredProviderName);
			if (result == null) throw new InvalidOperationException("Unknown database provider: " + providerName);
			return result;
		}

		internal static SupportedDb GetSupportedDb(string providerName)
		{
			string loweredProviderName = providerName.ToLowerInvariant();
			if (MySql.GetProviderFactoryClassNameName(loweredProviderName)) return SupportedDb.MySql;
			else if (Oracle.GetProviderFactoryClassNameName(loweredProviderName)) return SupportedDb.Oracle;
			else if (PostgreSql.GetProviderFactoryClassNameName(loweredProviderName)) return SupportedDb.PostgreSql;
			else if (Sqlite.GetProviderFactoryClassNameName(loweredProviderName)) return SupportedDb.Sqlite;
			else if (SqlServer.GetProviderFactoryClassNameName(loweredProviderName)) return SupportedDb.SqlServer;
			else throw new InvalidOperationException("Unknown database provider: " + providerName);
		}
	}
}