using System;

namespace Mighty.Dynamic.Tests.MySql.TableClasses
{
    public class Category : MightyOrm
    {
        public Category(string providerName, bool explicitConnection = false) : this(providerName, true, explicitConnection)
        {
        }


        public Category(string providerName, bool includeSchema, bool explicitConnection = false) :
            base(
                explicitConnection ?
                    $"ProviderName={providerName}" :
                    WhenDevart.AddLicenseKey(providerName, string.Format(TestConstants.WriteTestConnection, providerName)),
                includeSchema ? "MassiveWriteTests.Categories" : "Categories",
                "CategoryID")
        {
        }
    }
}
