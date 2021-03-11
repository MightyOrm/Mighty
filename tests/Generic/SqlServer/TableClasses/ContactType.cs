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
        public ContactTypes() : this(true)
        {
        }


        public ContactTypes(bool includeSchema) :
            base(string.Format(TestConstants.ReadTestConnection, TestConstants.ProviderName), includeSchema ? "Person.ContactType" : "ContactType", "ContactTypeID", "Name")
        {
        }
    }
}
