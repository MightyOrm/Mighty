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
        }
    }
}
