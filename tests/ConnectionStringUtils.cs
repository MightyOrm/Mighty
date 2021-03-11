using System;
#if !NETCOREAPP
using System.Configuration;
#endif

using NUnit.Framework;

namespace MightyTests
{
    internal class ConnectionStringUtils
    {
        /// <summary>
        /// Check that connection string is indeed required for OpenConnection in Mighty instance with no connection string
        /// </summary>
        /// <param name="db"></param>
        internal static void CheckConnectionStringRequiredForOpenConnection(dynamic db)
        {
            if (!string.IsNullOrEmpty(db.ConnectionString))
            {
                Assert.Fail("db.ConnectionString should be missing in this test!");
            }
            Assert.Throws<InvalidOperationException>(() => db.OpenConnection(), $"{nameof(db.OpenConnection)} did not throw {nameof(InvalidOperationException)}");
        }

#if !NET40
        /// <summary>
        /// Check that connection string is indeed required for OpenConnectionAsync in Mighty instance with no connection string
        /// </summary>
        /// <param name="db"></param>
        internal static
#if NETCOREAPP1_0
            async
#endif

            void CheckConnectionStringRequiredForOpenConnectionAsync(dynamic db)
        {
            if (!string.IsNullOrEmpty(db.ConnectionString))
            {
                Assert.Fail("db.ConnectionString should be missing in this test!");
            }
            string failMsg = $"{nameof(db.OpenConnectionAsync)} did not throw {nameof(InvalidOperationException)}";
#if NETCOREAPP1_0
            bool okay = false;
            try
            {
                await db.OpenConnectionAsync();
            }
            catch (InvalidOperationException)
            {
                okay = true;
            }
            if (!okay) Assert.Fail(failMsg);
#else
            Assert.ThrowsAsync<InvalidOperationException>(async () => await db.OpenConnectionAsync(), failMsg);
#endif
        }
#endif

        /// <summary>
        /// Reverse the admitted mess around here, to extract the raw connection string
        /// </summary>
        /// <param name="BaseConnectionString"></param>
        /// <param name="ProviderName"></param>
        /// <returns></returns>
        internal static string GetConnectionString(string BaseConnectionString, string ProviderName)
        {
#if NETCOREAPP
            if (BaseConnectionString.Contains(";providerName={0}")) return BaseConnectionString.Replace(";providerName={0}", "");
            else if (BaseConnectionString.Contains(";ProviderName={0}")) return BaseConnectionString.Replace(";ProviderName={0}", "");
            else throw new Exception("Cannot find ProviderName to replace");
#else
            string name = string.Format(BaseConnectionString, ProviderName);
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[name];
            if (settings == null) throw new Exception($"No ConfigurationManager connection string for connection string name \"{name}\"");
            return settings.ConnectionString;
#endif
        }

    }
}
