using System;

namespace Mighty.Dynamic.Tests.SqlServer.TableClasses
{
    public class SPTestsDatabase : MightyOrm
    {
        public SPTestsDatabase() : base(string.Format(TestConstants.ReadTestConnection, TestConstants.ProviderName))
        {
        }
    }
}
