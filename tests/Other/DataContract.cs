using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using Mighty.DataContracts;


namespace Mighty.Dynamic.Tests.X
{
    public class DataContract
    {
        [Test]
        public void CachingHits()
        {
            Assert.Greater(DataContractStore.Instance.CacheHits, 500);
        }

        [Test]
        public void CachingMisses()
        {
            Assert.Less(DataContractStore.Instance.CacheMisses, 20);
        }
    }

    public class TableMetaData
    {
        [Test]
        public void CachingHits()
        {
            Assert.Greater(MetaDataStore.Instance.CacheHits, 10);
        }

        [Test]
        public void CachingMisses()
        {
            Assert.Less(MetaDataStore.Instance.CacheMisses,10);
        }
    }
}

namespace Mighty.Generic.Tests.X
{
    public class DataContract
    {
        [Test]
        public void CachingHits()
        {
            Assert.Greater(DataContractStore.Instance.CacheHits, 800);
        }

        [Test]
        public void CachingMisses()
        {
            Assert.Less(DataContractStore.Instance.CacheMisses, 40);
        }
    }

    public class TableMetaData
    {
        [Test]
        public void CachingHits()
        {
            Assert.Greater(MetaDataStore.Instance.CacheHits, 60);
        }

        [Test]
        public void CachingMisses()
        {
            Assert.Less(MetaDataStore.Instance.CacheMisses, 30);
        }
    }
}
