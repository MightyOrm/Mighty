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
            Assert.AreEqual(1394, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteDbDataReader]);
#else
            Assert.AreEqual(699, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteDbDataReader]);
#endif
#elif NETCOREAPP
#if !(NETCOREAPP1_0 || NETCOREAPP1_1)
            Assert.AreEqual(1315, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteDbDataReader]);
#else
            Assert.AreEqual(1249, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteDbDataReader]);
#endif
#endif
        }

        [Test]
        public void ExecuteNonQueryCount()
        {
#if NETFRAMEWORK
#if !NET40
            Assert.AreEqual(341, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteNonQuery]);
#else
            Assert.AreEqual(176, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteNonQuery]);
#endif
#elif NETCOREAPP
#if !(NETCOREAPP1_0 || NETCOREAPP1_1)
            Assert.AreEqual(268, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteNonQuery]);
#else
            Assert.AreEqual(198, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteNonQuery]);
#endif
#endif
        }

        [Test]
        public void ExecuteScalarCount()
        {
#if NETFRAMEWORK
#if !NET40
            Assert.AreEqual(395, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteScalar]);
#else
            Assert.AreEqual(198, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteScalar]);
#endif
#elif NETCOREAPP
#if !(NETCOREAPP1_0 || NETCOREAPP1_1)
            Assert.AreEqual(359, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteScalar]);
#else
            Assert.AreEqual(323, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteScalar]);
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
