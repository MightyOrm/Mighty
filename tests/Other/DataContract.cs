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
    public class DataContract
    {
        [Test]
        public void CacheHits()
        {
            Assert.AreEqual(518, ColumnsContractStore.Instance.CacheHits);
        }

        [Test]
        public void CacheMisses()
        {
            // all the dynamic tests should only ever need one contract now,
            // even though some of the dynamic tests specify columns
            Assert.AreEqual(1, ColumnsContractStore.Instance.CacheMisses);
        }

        [Test]
        public void IncorrectDynamicColumnMappingCausesException()
        {
            // okay to use the default mapper
            var good = new MightyOrm(
                mapper: new SqlNamingMapper(columnName: SqlNamingMapper.IdentityColumnMapping));
            // now okay to use the non-default mapper with a columns spec
            var good2 = new MightyOrm(
                columns: "col1, col_33",
                mapper: new SqlNamingMapper(columnName: (t, n) => n));
            // not okay to override, even with 'the same' function
            Assert.Throws<InvalidOperationException>(() =>
            {
                var bad = new MightyOrm(mapper: new SqlNamingMapper(columnName: (t, n) => n));
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
            // This is upped by 300 - 3 by some forced cache hits in Insert_FromNew
            Assert.AreEqual(1154, ColumnsContractStore.Instance.CacheHits);
        }

        [Test]
        public void CacheMisses()
        {
            Assert.AreEqual(17, ColumnsContractStore.Instance.CacheMisses);
        }
    }

    public class TableMetaData
    {
        [Test]
        public void CacheHits()
        {
            // This is upped by 300 - 3 by some forced cache hits in Insert_FromNew
            Assert.AreEqual(364, TableMetaDataStore.Instance.CacheHits);
        }

        [Test]
        public void CacheMisses()
        {
            Assert.AreEqual(22, TableMetaDataStore.Instance.CacheMisses);
        }
    }
}
