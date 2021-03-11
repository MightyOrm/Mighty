using System;

namespace Mighty.Dynamic.Tests.SqlServer.TableClasses
{
    public class Category : MightyOrm
    {
        public Category(bool explicitConnection = false) : this(true, explicitConnection)
        {
        }


        public Category(bool includeSchema, bool explicitConnection = false) :
            base(
                explicitConnection ?
                    $"ProviderName={TestConstants.ProviderName}" :
                    string.Format(TestConstants.WriteTestConnection, TestConstants.ProviderName),
                includeSchema ? "dbo.Categories" : "Categories",
                "CategoryID")
        {
        }
    }
}
