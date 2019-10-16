#if false //NETSTANDARD2_0 // || NET40
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using SqlProfiler;

namespace MightyTests
{
    // This is a DynamicObject with TryInvokeMember and TryGetMember instantiated
    // (We're using MSDynamicObject, which is DynamicObject copied from the MS reference code, so that we can single step and look at the code)
    public class TestDynamic : MSDynamicObject
    {
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = "TryInvokeMember result for " + binder.Name;
            return true;
        }
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;
            // What we'd do if we don't want to return a member for everything...
            //if (binder.Name.StartsWith("Z")) return false;
            result = "TryGetMember result for " + binder.Name;
            return true;
        }
        public string A() { return "this is A from TestDynamic"; }
        public string B() { return "this is B from TestDynamic"; }

        public string W = "this is W from TestDynamic";
        public string X = "this is X from TestDynamic";
    }

    public class TestPOCO
    {
        public string A() { return "this is A from TestPOCO"; }
        public string B() { return "this is B from TestPOCO"; }

        public string W = "this is W from TestPOCO";
        public string X = "this is X from TestPOCO";
    }

    public class TestClass : IDynamicMetaObjectProvider
    {
        object Wrapped { get; }

        public TestClass(object wrapped)
        {
            Wrapped = wrapped;
        }

        public DynamicMetaObject GetMetaObject(Expression parameter)
        {
            return new SqlProfiler.DelegatingMetaObject(parameter, this, "Wrapped", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public string A() { return "this is A from TestClass"; }
        public string C() { return "this is C from TestClass"; }
        public string W = "this is W from TestClass";
        public string Y = "this is Y from TestClass";
    }
}
#endif