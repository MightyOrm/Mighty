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
            return BaseConnectionString.Replace(";providerName={0}", "");
#else
            return ConfigurationManager.ConnectionStrings[string.Format(BaseConnectionString, ProviderName)].ConnectionString;
#endif
        }

    }
}
