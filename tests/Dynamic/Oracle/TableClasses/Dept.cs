using System;

namespace Mighty.Dynamic.Tests.Oracle.TableClasses
{
    public class Department : MightyOrm
    {
        public Department(string providerName, bool explicitConnection = false) : this(providerName, true, explicitConnection)
        {
        }


        public Department(string providerName, bool includeSchema, bool explicitConnection = false) 
            : base(
                explicitConnection ? $"ProviderName={providerName}" : string.Format(TestConstants.ReadWriteTestConnection, providerName),
                includeSchema ? "SCOTT.DEPT" : "DEPT", "DEPTNO",
#if KEY_VALUES
                string.Empty,
#endif
                includeSchema ? "SCOTT.DEPT_SEQ" : "DEPT_SEQ")
        {
            
        }
    }
}
