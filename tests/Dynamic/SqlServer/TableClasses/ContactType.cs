using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mighty.Dynamic.Tests.SqlServer.TableClasses
{
    public class ContactType : MightyOrm
    {
        public ContactType() : this(true)
        {
        }


        public ContactType(bool includeSchema) :
            base(TestConstants.ReadTestConnection, includeSchema ? "Person.ContactType" : "ContactType", "ContactTypeID", "Name")
        {
        }
    }
}
