using System;

using NUnit.Framework;

namespace Mighty.Other
{
    [TestFixture]
    public class DataContractKey
    {
        [Test]
        public void Equals_IsNotMakingStupidMistakes()
        {
            var one = new DataContracts.DataContractKey(false, typeof(Dynamic.Tests.MySql.TableClasses.SPTestsDatabase), null, new Mapping.SqlNamingMapper());
            var theOther = new DataContracts.DataContractKey(true, typeof(Generic.Tests.PostgreSql.TableClasses.Customer), null, new Mapping.SqlNamingMapper());
            Assert.That(one.Equals(theOther), Is.False, "no explosions");
        }
    }
}
