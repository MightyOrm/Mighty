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
        public People() : this(true)
        {
        }


        public People(bool includeSchema) :
            base(string.Format(TestConstants.ReadTestConnection, TestConstants.ProviderName), includeSchema ? "dbo.People" : "People", "PersonID")
        {
        }
    }
}
