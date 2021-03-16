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
            Assert.AreEqual(1444, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteDbDataReader]);
#else
            Assert.AreEqual(726, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteDbDataReader]);
#endif
#elif NETCOREAPP
#if !(NETCOREAPP1_0 || NETCOREAPP1_1)
#if DISABLE_DEVART
            Assert.AreEqual(1222, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteDbDataReader]);
#else
            Assert.AreEqual(1362, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteDbDataReader]);
#endif
#else
#if DISABLE_DEVART
            Assert.AreEqual(1151, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteDbDataReader]);
#else
            Assert.AreEqual(1291, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteDbDataReader]);
#endif
#endif
#endif
        }

        [Test]
        public void ExecuteNonQueryCount()
        {
#if NETFRAMEWORK
#if !NET40
            Assert.AreEqual(395, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteNonQuery]);
#else
            Assert.AreEqual(201, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteNonQuery]);
#endif
#elif NETCOREAPP
#if !(NETCOREAPP1_0 || NETCOREAPP1_1)
#if DISABLE_DEVART
            Assert.AreEqual(256, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteNonQuery]);
#else
            Assert.AreEqual(312, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteNonQuery]);
#endif
#else
#if DISABLE_DEVART
            Assert.AreEqual(176, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteNonQuery]);
#else
            Assert.AreEqual(232, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteNonQuery]);
#endif
#endif
#endif
        }

        [Test]
        public void ExecuteScalarCount()
        {
#if NETFRAMEWORK
#if !NET40
            Assert.AreEqual(436, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteScalar]);
#else
            Assert.AreEqual(220, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteScalar]);
#endif
#elif NETCOREAPP
#if NETCOREAPP2_0 || NETCOREAPP3_0 || NETCOREAPP3_1
#if DISABLE_DEVART
            Assert.AreEqual(294, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteScalar]);
#else
            Assert.AreEqual(396, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteScalar]);
#endif
#else
#if DISABLE_DEVART
            Assert.AreEqual(252, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteScalar]);
#else
            Assert.AreEqual(354, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteScalar]);
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
            MightyOrm.GlobalDataProfiler = new MightyTestsProfiler();
        }
    }
}
