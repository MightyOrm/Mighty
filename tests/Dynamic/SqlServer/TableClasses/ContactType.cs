using System;

namespace Mighty.Dynamic.Tests.SqlServer.TableClasses
{
    public class ContactType : MightyOrm
    {
        public ContactType() : this(true)
        {
        }


        public ContactType(bool includeSchema) :
            base(string.Format(TestConstants.ReadTestConnection, TestConstants.ProviderName), includeSchema ? "Person.ContactType" : "ContactType", "ContactTypeID", "Name")
        {
        }
    }
}
