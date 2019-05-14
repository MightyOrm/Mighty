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
}
