using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mighty.Dynamic.Tests.MySql.TableClasses
{
    public class SPTestsDatabase : MightyOrm
    {
        public SPTestsDatabase(string providerName) : base(WhenDevart.AddLicenseKey(TestConstants.ReadTestConnection, providerName), tableName: "")
        {
        }
    }
}
