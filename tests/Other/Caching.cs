using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using Mighty.DataContracts;
using Mighty.Mapping;

namespace Mighty.Dynamic.Tests.X
{
    /// <remarks>
    /// While it's arguably tedious to update the cache hits and misses here as the tests change, it's:
    /// a) Important the understand exactly why the numbers change when they do, and to check that each
    ///    change actually makes sense, and it's also
    /// b) Essential to be able to spot quickly whenever the caching gets crashed completely by any
    ///    code change!
    /// 
    /// These cache hits/misses tests rely on NUnit running all the tests in the project one at a time,
    /// in name order (which doesn't apply in XUnit, for instance), but they do what is needed and it's
    /// certainly useful to be able to leverage the whole test suite as a caching test.
    /// 
    /// There doesn't seem any way to indicate to NUnit that it should run something before and after all
    /// tests (https://stackoverflow.com/q/18485622) - and even if there was, I suppose that something
    /// wouldn't be a test itself, anyway (as existing [<see cref="OneTimeTearDownAttribute"/>] code isn't,
    /// for example).
    /// </remarks>
    public partial class DataContract
    {
        [Test]
        public void CacheSize()
        {
            Assert.AreEqual(DataContractStore.Instance.CacheSize, DataContractStore.Instance.CacheMisses);
        }

        [Test]
        public void CacheHits()
        {
#if NETFRAMEWORK
#if !NET40
            Assert.AreEqual(561, DataContractStore.Instance.CacheHits);
#else
            Assert.AreEqual(270, DataContractStore.Instance.CacheHits);
#endif
#elif NETCOREAPP
#if !(NETCOREAPP1_0 || NETCOREAPP1_1)
#if DISABLE_DEVART
            Assert.AreEqual(378, DataContractStore.Instance.CacheHits);
#else
            Assert.AreEqual(486, DataContractStore.Instance.CacheHits);
#endif
#else
#if DISABLE_DEVART
            Assert.AreEqual(309, DataContractStore.Instance.CacheHits);
#else
            Assert.AreEqual(417, DataContractStore.Instance.CacheHits);
#endif
#endif
#endif
        }

        [Test]
        public void CacheMisses()
        {
            // all the dynamic tests should only ever need one contract now,
            // even though some of the dynamic tests specify columns
#if NETFRAMEWORK
#if !NET40
            Assert.AreEqual(3, DataContractStore.Instance.CacheMisses);
#else
            Assert.AreEqual(2, DataContractStore.Instance.CacheMisses);
#endif
#elif NETCOREAPP
#if !(NETCOREAPP1_0 || NETCOREAPP1_1)
            Assert.AreEqual(3, DataContractStore.Instance.CacheMisses);
#else
            Assert.AreEqual(3, DataContractStore.Instance.CacheMisses);
#endif
#endif
        }
    }

    public class TableMetaData
    {
        [Test]
        public void CacheSize()
        {
            Assert.AreEqual(TableMetaDataStore.Instance.CacheSize, TableMetaDataStore.Instance.CacheMisses);
        }

        [Test]
        public void CacheHits()
        {
#if NETFRAMEWORK
#if !NET40
            Assert.AreEqual(12, TableMetaDataStore.Instance.CacheHits);
#else
            Assert.AreEqual(3, TableMetaDataStore.Instance.CacheHits);
#endif
#elif NETCOREAPP
#if DISABLE_DEVART
            Assert.AreEqual(8, TableMetaDataStore.Instance.CacheHits);
#else
            Assert.AreEqual(12, TableMetaDataStore.Instance.CacheHits);
#endif
#endif
        }

        [Test]
        public void SyncCacheHits()
        {
#if NETFRAMEWORK
#if !NET40
            Assert.AreEqual(9, TableMetaDataStore.Instance.SyncCacheHits);
#else
            Assert.AreEqual(3, TableMetaDataStore.Instance.SyncCacheHits);
#endif
#elif NETCOREAPP
#if DISABLE_DEVART
            Assert.AreEqual(6, TableMetaDataStore.Instance.SyncCacheHits);
#else
            Assert.AreEqual(9, TableMetaDataStore.Instance.SyncCacheHits);
#endif
#endif
        }

#if !NET40
        [Test]
        public void AsyncCacheHits()
        {
#if NETFRAMEWORK
            Assert.AreEqual(3, TableMetaDataStore.Instance.AsyncCacheHits);
#elif NETCOREAPP
#if DISABLE_DEVART
            Assert.AreEqual(2, TableMetaDataStore.Instance.AsyncCacheHits);
#else
            Assert.AreEqual(3, TableMetaDataStore.Instance.AsyncCacheHits);
#endif
#endif
        }
#endif

        [Test]
        public void TotalCacheHits()
        {
#if !NET40
            Assert.AreEqual(
                TableMetaDataStore.Instance.CacheHits,
                TableMetaDataStore.Instance.SyncCacheHits +
                TableMetaDataStore.Instance.AsyncCacheHits);
#else
            Assert.AreEqual(
                TableMetaDataStore.Instance.CacheHits,
                TableMetaDataStore.Instance.SyncCacheHits);
#endif
        }

        [Test]
        public void CacheMisses()
        {
#if DISABLE_DEVART
            Assert.AreEqual(4, TableMetaDataStore.Instance.CacheMisses);
#else
            Assert.AreEqual(6, TableMetaDataStore.Instance.CacheMisses);
#endif
        }

        [Test]
        public void SyncCacheMisses()
        {
#if DISABLE_DEVART
            Assert.AreEqual(2, TableMetaDataStore.Instance.SyncCacheMisses);
#else
#if NET40
            Assert.AreEqual(6, TableMetaDataStore.Instance.SyncCacheMisses);
#else
            Assert.AreEqual(3, TableMetaDataStore.Instance.SyncCacheMisses);
#endif
#endif
        }

#if !NET40
        [Test]
        public void AsyncCacheMisses()
        {
#if DISABLE_DEVART
            Assert.AreEqual(2, TableMetaDataStore.Instance.AsyncCacheMisses);
#else
            Assert.AreEqual(3, TableMetaDataStore.Instance.AsyncCacheMisses);
#endif
        }
#endif

        [Test]
        public void TotalCacheMisses()
        {
#if !NET40
            Assert.AreEqual(
                TableMetaDataStore.Instance.CacheMisses,
                TableMetaDataStore.Instance.SyncCacheMisses +
                TableMetaDataStore.Instance.AsyncCacheMisses);
#else
            Assert.AreEqual(
                TableMetaDataStore.Instance.CacheMisses,
                TableMetaDataStore.Instance.SyncCacheMisses);
#endif
        }
    }
}

namespace Mighty.Generic.Tests.X
{
    public class DataContract
    {
        [Test]
        public void CacheSize()
        {
            Assert.AreEqual(DataContractStore.Instance.CacheSize, DataContractStore.Instance.CacheMisses);
        }

        [Test]
        public void CacheHits()
        {
#if NETFRAMEWORK
#if !NET40
            Assert.AreEqual(938, DataContractStore.Instance.CacheHits);
#else
            Assert.AreEqual(464, DataContractStore.Instance.CacheHits);
#endif
#elif NETCOREAPP
#if !(NETCOREAPP1_0 || NETCOREAPP1_1)
#if DISABLE_DEVART
            Assert.AreEqual(627, DataContractStore.Instance.CacheHits);
#else
            Assert.AreEqual(816, DataContractStore.Instance.CacheHits);
#endif
#else
#if DISABLE_DEVART
            Assert.AreEqual(515, DataContractStore.Instance.CacheHits);
#else
            Assert.AreEqual(704, DataContractStore.Instance.CacheHits);
#endif
#endif
#endif
        }

        [Test]
        public void CacheMisses()
        {
#if NETFRAMEWORK
#if !NET40
            Assert.AreEqual(21, DataContractStore.Instance.CacheMisses);
#else
            Assert.AreEqual(20, DataContractStore.Instance.CacheMisses);
#endif
#elif NETCOREAPP
#if !(NETCOREAPP1_0 || NETCOREAPP1_1)
            Assert.AreEqual(20, DataContractStore.Instance.CacheMisses);
#else
            Assert.AreEqual(17, DataContractStore.Instance.CacheMisses);
#endif
#endif
        }
    }

    public class TableMetaData
    {
        [Test]
        public void CacheSize()
        {
            Assert.AreEqual(TableMetaDataStore.Instance.CacheSize, TableMetaDataStore.Instance.CacheMisses);
        }

        [Test]
        public void CacheHits()
        {
#if NETFRAMEWORK
#if !NET40
            Assert.AreEqual(105, TableMetaDataStore.Instance.CacheHits);
#else
            Assert.AreEqual(29, TableMetaDataStore.Instance.CacheHits);
#endif
#elif NETCOREAPP
#if NETCOREAPP2_0 || NETCOREAPP3_0 || NETCOREAPP3_1
#if DISABLE_DEVART
            Assert.AreEqual(73, TableMetaDataStore.Instance.CacheHits);
#else
            Assert.AreEqual(103, TableMetaDataStore.Instance.CacheHits);
#endif
#else
#if DISABLE_DEVART
            Assert.AreEqual(70, TableMetaDataStore.Instance.CacheHits);
#else
            Assert.AreEqual(100, TableMetaDataStore.Instance.CacheHits);
#endif
#endif
#endif
        }

        [Test]
        public void SyncCacheHits()
        {
#if NETFRAMEWORK
#if !NET40
            Assert.AreEqual(57, TableMetaDataStore.Instance.SyncCacheHits);
#else
            Assert.AreEqual(29, TableMetaDataStore.Instance.SyncCacheHits);
#endif
#elif NETCOREAPP
#if NETCOREAPP2_0 || NETCOREAPP3_0 || NETCOREAPP3_1
#if DISABLE_DEVART
            Assert.AreEqual(40, TableMetaDataStore.Instance.SyncCacheHits);
#else
            Assert.AreEqual(55, TableMetaDataStore.Instance.SyncCacheHits);
#endif
#else
#if DISABLE_DEVART
            Assert.AreEqual(37, TableMetaDataStore.Instance.SyncCacheHits);
#else
            Assert.AreEqual(52, TableMetaDataStore.Instance.SyncCacheHits);
#endif
#endif
#endif
        }

#if !NET40
        [Test]
        public void AsyncCacheHits()
        {
#if NETFRAMEWORK
            Assert.AreEqual(48, TableMetaDataStore.Instance.AsyncCacheHits);
#elif NETCOREAPP
#if NETCOREAPP2_0 || NETCOREAPP3_0 || NETCOREAPP3_1
#if DISABLE_DEVART
            Assert.AreEqual(33, TableMetaDataStore.Instance.AsyncCacheHits);
#else
            Assert.AreEqual(48, TableMetaDataStore.Instance.AsyncCacheHits);
#endif
#else
#if DISABLE_DEVART
            Assert.AreEqual(33, TableMetaDataStore.Instance.AsyncCacheHits);
#else
            Assert.AreEqual(48, TableMetaDataStore.Instance.AsyncCacheHits);
#endif
#endif
#endif
        }
#endif

        [Test]
        public void TotalCacheHits()
        {
#if !NET40
            Assert.AreEqual(
                TableMetaDataStore.Instance.CacheHits,
                TableMetaDataStore.Instance.SyncCacheHits +
                TableMetaDataStore.Instance.AsyncCacheHits);
#else
            Assert.AreEqual(
                TableMetaDataStore.Instance.CacheHits,
                TableMetaDataStore.Instance.SyncCacheHits);
#endif
        }

        [Test]
        public void CacheMisses()
        {
#if NETFRAMEWORK
            Assert.AreEqual(29, TableMetaDataStore.Instance.CacheMisses);
#elif NETCOREAPP2_0 || NETCOREAPP3_0 || NETCOREAPP3_1
#if DISABLE_DEVART
            Assert.AreEqual(20, TableMetaDataStore.Instance.CacheMisses);
#else
            Assert.AreEqual(27, TableMetaDataStore.Instance.CacheMisses);
#endif
#else
#if DISABLE_DEVART
            Assert.AreEqual(17, TableMetaDataStore.Instance.CacheMisses);
#else
            Assert.AreEqual(24, TableMetaDataStore.Instance.CacheMisses);
#endif
#endif
        }

        [Test]
        public void SyncCacheMisses()
        {
#if NETFRAMEWORK
#if NET40
            Assert.AreEqual(29, TableMetaDataStore.Instance.SyncCacheMisses);
#else
            Assert.AreEqual(7, TableMetaDataStore.Instance.SyncCacheMisses);
#endif
#elif NETCOREAPP2_0 || NETCOREAPP3_0 || NETCOREAPP3_1
#if DISABLE_DEVART
            Assert.AreEqual(5, TableMetaDataStore.Instance.SyncCacheMisses);
#else
            Assert.AreEqual(7, TableMetaDataStore.Instance.SyncCacheMisses);
#endif
#else
#if DISABLE_DEVART
            Assert.AreEqual(4, TableMetaDataStore.Instance.SyncCacheMisses);
#else
            Assert.AreEqual(6, TableMetaDataStore.Instance.SyncCacheMisses);
#endif
#endif
        }

#if !NET40
        [Test]
        public void AsyncCacheMisses()
        {
#if NETFRAMEWORK
            Assert.AreEqual(22, TableMetaDataStore.Instance.AsyncCacheMisses);
#elif NETCOREAPP2_0 || NETCOREAPP3_0 || NETCOREAPP3_1
#if DISABLE_DEVART
            Assert.AreEqual(15, TableMetaDataStore.Instance.AsyncCacheMisses);
#else
            Assert.AreEqual(20, TableMetaDataStore.Instance.AsyncCacheMisses);
#endif
#else
#if DISABLE_DEVART
            Assert.AreEqual(13, TableMetaDataStore.Instance.AsyncCacheMisses);
#else
            Assert.AreEqual(18, TableMetaDataStore.Instance.AsyncCacheMisses);
#endif
#endif
        }
#endif

        [Test]
        public void TotalCacheMisses()
        {
#if !NET40
            Assert.AreEqual(
                TableMetaDataStore.Instance.CacheMisses,
                TableMetaDataStore.Instance.SyncCacheMisses +
                TableMetaDataStore.Instance.AsyncCacheMisses);
#else
            Assert.AreEqual(
                TableMetaDataStore.Instance.CacheMisses,
                TableMetaDataStore.Instance.SyncCacheMisses);
#endif
        }
    }
}
