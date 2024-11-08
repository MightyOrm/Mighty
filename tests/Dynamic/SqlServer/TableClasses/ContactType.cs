using System;

namespace Mighty.Dynamic.Tests.SqlServer.TableClasses
{
    public class ContactType : MightyOrm
    {
        public ContactType(string providerName) : this(providerName, true)
        {
        }


        public ContactType(string providerName, bool includeSchema) :
            base(string.Format(TestConstants.ReadTestConnection, providerName), includeSchema ? "Person.ContactType" : "ContactType", "ContactTypeID", "Name")
        {
        }
    }
}
