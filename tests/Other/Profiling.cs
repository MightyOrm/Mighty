using System;
using NUnit.Framework;
using Mighty.Profiling;
using SqlProfiler.Simple;

using MightyTests.Profiling;

namespace Mighty.XAllTests.SqlProfiling
{
    [TestFixture]
    public class Profiling
    {
        [Test]
        public void ExecuteDbDataReaderCount()
        {
#if NETFRAMEWORK
#if !NET40
            Assert.AreEqual(1434, ((MightyTestsSqlProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteDbDataReader]);
#else
            Assert.AreEqual(725, ((MightyTestsSqlProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteDbDataReader]);
#endif
#elif NETCOREAPP
#if !(NETCOREAPP1_0 || NETCOREAPP1_1)
#if DISABLE_DEVART
            Assert.AreEqual(1214, ((MightyTestsSqlProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteDbDataReader]);
#else
            Assert.AreEqual(1351, ((MightyTestsSqlProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteDbDataReader]);
#endif
#else
#if DISABLE_DEVART
            Assert.AreEqual(1143, ((MightyTestsSqlProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteDbDataReader]);
#else
            Assert.AreEqual(1279, ((MightyTestsSqlProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteDbDataReader]);
#endif
#endif
#endif
        }

        [Test]
        public void ExecuteNonQueryCount()
        {
#if NETFRAMEWORK
#if !NET40
            Assert.AreEqual(341, ((MightyTestsSqlProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteNonQuery]);
#else
            Assert.AreEqual(176, ((MightyTestsSqlProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteNonQuery]);
#endif
#elif NETCOREAPP
#if !(NETCOREAPP1_0 || NETCOREAPP1_1)
#if DISABLE_DEVART
            Assert.AreEqual(222, ((MightyTestsSqlProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteNonQuery]);
#else
            Assert.AreEqual(268, ((MightyTestsSqlProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteNonQuery]);
#endif
#else
#if DISABLE_DEVART
            Assert.AreEqual(152, ((MightyTestsSqlProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteNonQuery]);
#else
            Assert.AreEqual(198, ((MightyTestsSqlProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteNonQuery]);
#endif
#endif
#endif
        }

        [Test]
        public void ExecuteScalarCount()
        {
#if NETFRAMEWORK
#if !NET40
            Assert.AreEqual(425, ((MightyTestsSqlProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteScalar]);
#else
            Assert.AreEqual(214, ((MightyTestsSqlProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteScalar]);
#endif
#elif NETCOREAPP
#if !(NETCOREAPP1_0 || NETCOREAPP1_1)
#if DISABLE_DEVART
            Assert.AreEqual(283, ((MightyTestsSqlProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteScalar]);
#else
            Assert.AreEqual(385, ((MightyTestsSqlProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteScalar]);
#endif
#else
#if DISABLE_DEVART
            Assert.AreEqual(241, ((MightyTestsSqlProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteScalar]);
#else
            Assert.AreEqual(343, ((MightyTestsSqlProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteScalar]);
#endif
#endif
#endif
        }

    }

}

namespace Mighty.AllTests.SqlProfiling
{
    [TestFixture]
    public class Profiling
    {
        [Test]
        public void AddProfiling()
        {
            MightyOrm.GlobalDataProfiler = new MightyTestsSqlProfiler();
        }
    }
}
