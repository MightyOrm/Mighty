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
            Assert.AreEqual(520, ColumnsContractStore.Instance.CacheHits);
        }

        [Test]
        public void CacheMisses()
        {
            // all the dynamic tests should only ever need one contract now,
            // even though some of the dynamic tests specify columns
            Assert.AreEqual(3, ColumnsContractStore.Instance.CacheMisses);
        }

        [Test]
        public void WithDefautMapper_CreatesOk()
        {
            new MightyOrm(
                mapper: new SqlNamingMapper(columnNameMapping: SqlNamingMapper.IdentityColumnMapping));
        }

        [Test]
        public void WithNonDefautMapperAndColumns_CreatesOk()
        {
            new MightyOrm(
                columns: "col1, col_33",
                mapper: new SqlNamingMapper(columnNameMapping: (t, n) => n));
        }

        [Test]
        public void WithNonDefautMapperNoColumns_ThrowsException()
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
            Assert.AreEqual(861, ColumnsContractStore.Instance.CacheHits);
        }

        [Test]
        public void CacheMisses()
        {
            Assert.AreEqual(19, ColumnsContractStore.Instance.CacheMisses);
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
