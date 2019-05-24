using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using Mighty;
using Mighty.Interfaces;

namespace Mighty.MethodSignatures
{
    [TestFixture]
    public class CheckMethods
    {
        [Test]
        public void CheckAll()
        {
            var interfaceMethods = new MethodChecker<MightyOrmAbstractInterface<CheckMethods>, CheckMethods>(true, true);
            var dynamicMightyMethods = new MethodChecker<MightyOrm, dynamic>(false, false);
            var genericMightyMethods = new MethodChecker<MightyOrm<CheckMethods>, CheckMethods>(false, true);

            // just the factory methods
            Assert.AreEqual(0, interfaceMethods.StaticMethods.Count);
            Assert.AreEqual(1, dynamicMightyMethods.StaticMethods.Count);
            Assert.AreEqual(1, genericMightyMethods.StaticMethods.Count);

            // apart from the static factory method MightyOrm just is MightyOrm<dynamic>
            Assert.AreEqual(0, dynamicMightyMethods.SyncOnlyMethods.Count);
            Assert.AreEqual(0, dynamicMightyMethods.SyncMethods.Count);
            Assert.AreEqual(0, dynamicMightyMethods.AsyncMethods.Count);

            // we know that all the interface methods must be present, but check that nothing extra is present
            Assert.AreEqual(10, interfaceMethods.SyncOnlyMethods.Count);
            Assert.AreEqual(interfaceMethods.SyncOnlyMethods.Count, genericMightyMethods.SyncOnlyMethods.Count);

            Assert.AreEqual(70, interfaceMethods.SyncMethods.Count);
            Assert.AreEqual(interfaceMethods.SyncMethods.Count, genericMightyMethods.SyncMethods.Count);

            Assert.AreEqual(136, interfaceMethods.AsyncMethods.Count);
            Assert.AreEqual(interfaceMethods.AsyncMethods.Count, genericMightyMethods.AsyncMethods.Count);

            // okay, so interface and generic mighty must be identical at this point
            // now we can start checking either one of them for the sync/async pattern, etc.

            // check that all the sync methods have a version with and without dbconnection

            // check that all the async methods have a variant with and one without cancellationtoken
        }
    }
}
