using System;

namespace Mighty.Dynamic.Tests.SqlServer.TableClasses
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
                    string.Format(TestConstants.WriteTestConnection, providerName),
                includeSchema ? "dbo.Categories" : "Categories",
                "CategoryID")
        {
        }
    }
}
