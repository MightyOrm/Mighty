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
    /// While it's arguably boring to update the cache hits and misses in here as the tests change, it's:
    /// a) Important the understand why the numbers change when they do, and to check that (and understand
    ///    why) each change actually makes sense, and it's
    /// b) Essential to be able to spot if the caching gets crashed completely by any code change.
    /// 
    /// Also, yes these cache hit/miss tests rely on NUnit running the tests one at a time in name
    /// order, which doesn't apply in XUnit, but they do what is needed (as above)!
    /// </remarks>
    public class DataContract
    {
        [Test]
        public void CacheHits()
        {
            Assert.AreEqual(534, DataContractStore.Instance.CacheHits);
        }

        [Test]
        public void CacheMisses()
        {
            // all the dynamic tests should only ever need one contract now,
            // even though some of the dynamic tests specify columns
            Assert.AreEqual(3, DataContractStore.Instance.CacheMisses);
        }

        [Test]
        public void WithDefaultMapper_CreatesOk()
        {
            new MightyOrm(
                mapper: new SqlNamingMapper(columnNameMapping: SqlNamingMapper.IdentityColumnMapping));
        }

        [Test]
        public void WithNonDefaultMapperAndColumns_CreatesOk()
        {
            new MightyOrm(
                columns: "col1, col_33",
                mapper: new SqlNamingMapper(columnNameMapping: (t, n) => n));
        }

        [Test]
        public void WithNonDefaultMapperNoColumns_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                new MightyOrm(mapper: new SqlNamingMapper(columnNameMapping: (t, n) => n));
            });
        }
    }

    public class TableMetaData
    {
        [Test]
        public void CacheHits()
        {
            Assert.AreEqual(12, TableMetaDataStore.Instance.CacheHits);
        }

        [Test]
        public void CacheMisses()
        {
            Assert.AreEqual(6, TableMetaDataStore.Instance.CacheMisses);
        }
    }
}

namespace Mighty.Generic.Tests.X
{
    public class DataContract
    {
        [Test]
        public void CacheHits()
        {
            Assert.AreEqual(889, DataContractStore.Instance.CacheHits);
        }

        [Test]
        public void CacheMisses()
        {
            Assert.AreEqual(19, DataContractStore.Instance.CacheMisses);
        }
    }

    public class TableMetaData
    {
        [Test]
        public void CacheHits()
        {
            Assert.AreEqual(69, TableMetaDataStore.Instance.CacheHits);
        }

        [Test]
        public void CacheMisses()
        {
            Assert.AreEqual(22, TableMetaDataStore.Instance.CacheMisses);
        }
    }
}
