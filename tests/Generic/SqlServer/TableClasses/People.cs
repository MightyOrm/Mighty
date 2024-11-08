using System;

namespace Mighty.Generic.Tests.SqlServer.TableClasses
{
    public class Person
    {
        public int PersonID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class People : MightyOrm<Person>
    {
        public People(string providerName) : this(providerName, true)
        {
        }


        public People(string providerName, bool includeSchema) :
            base(string.Format(TestConstants.ReadTestConnection, providerName), includeSchema ? "dbo.People" : "People", "PersonID")
        {
        }
    }
}
