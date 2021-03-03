using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mighty.Dynamic.Tests.PostgreSql.TableClasses
{
    public class SPTestsDatabase : MightyOrm
    {
        public SPTestsDatabase(bool providerNameOnly = false)
            : base(providerNameOnly ?
                  $"ProviderName={TestConstants.ProviderName}" :
                  string.Format(TestConstants.ReadWriteTestConnection, TestConstants.ProviderName))
        {
        }
    }
}
