using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mighty.Dynamic.Tests.SqlServer.TableClasses
{
    public class SPTestsDatabase : MightyOrm
    {
        public SPTestsDatabase(string providerName) : base(string.Format(TestConstants.ReadTestConnection, providerName))
        {
        }
    }
}
