using System;

namespace Mighty.Generic.Tests.Oracle.TableClasses
{
    public class DEPT
    {
        public int DEPTNO { get; set; }
        public string DNAME { get; set; }
        public string LOC { get; set; }
    }

    public class Departments : MightyOrm<DEPT>
    {
        public Departments(string providerName, bool explicitConnection = false) : this(providerName, true, explicitConnection)
        {
        }


        public Departments(string providerName, bool includeSchema, bool explicitConnection = false)
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
