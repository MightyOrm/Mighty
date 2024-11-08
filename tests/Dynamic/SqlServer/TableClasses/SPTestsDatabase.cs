using System;

namespace Mighty.Dynamic.Tests.SqlServer.TableClasses
{
    public class SPTestsDatabase : MightyOrm
    {
        public SPTestsDatabase(string providerName) : base(string.Format(TestConstants.ReadTestConnection, providerName))
        {
        }
    }
}
