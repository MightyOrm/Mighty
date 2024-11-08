using System;

namespace Mighty.Generic.Tests.SqlServer.TableClasses
{
    public class ContactType
    {
        public int ContactTypeID { get; set; }
        public string Name { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
    public class ContactTypes : MightyOrm<ContactType>
    {
        public ContactTypes(string providerName) : this(providerName, true)
        {
        }


        public ContactTypes(string providerName, bool includeSchema) :
            base(string.Format(TestConstants.ReadTestConnection, providerName), includeSchema ? "Person.ContactType" : "ContactType", "ContactTypeID", "Name")
        {
        }
    }
}
