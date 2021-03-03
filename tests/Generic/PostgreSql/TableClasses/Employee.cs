using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mighty.Generic.Tests.PostgreSql.TableClasses
{
    public class Employee
    {
        public int employeeid { get; set; }
        public string companyname { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
    }

    public class Employees : MightyOrm<Employee>
    {
        public Employees(bool explicitConnection = false)
            : this(includeSchema: true, explicitConnection)
        {
        }


        public Employees(bool includeSchema, bool explicitConnection = false) :
            base(explicitConnection ?
                    $"ProviderName={TestConstants.ProviderName}" :
                    string.Format(TestConstants.ReadWriteTestConnection, TestConstants.ProviderName),
                includeSchema ? "public.employees" : "employees", "employeeid")
        {
        }
    }
}
