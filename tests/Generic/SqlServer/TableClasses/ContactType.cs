using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            base(TestConstants.ReadTestConnection, includeSchema ? "Person.ContactType" : "ContactType", "ContactTypeID", "Name")
        {
        }
    }
}
