using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mighty.Dynamic.Tests.MySql.TableClasses
{
    public class SPTestsDatabase : MightyOrm
    {
        public SPTestsDatabase(string providerName, bool providerNameOnly = false)
            : base(
                providerNameOnly ?
                    $"ProviderName={providerName}" :
                    WhenDevart.AddLicenseKey(providerName, string.Format(TestConstants.ReadTestConnection, providerName)),
                tableName: ""
            )
        {
        }
    }
}
