using System;
using System.Reflection;

namespace Mighty.MethodSignatures
{
    public class MightyMethodInfo
    {
        public MightySyncType syncType;
        public MightyMethodType methodType;
        public MightyParamsType paramsType;
        public MightyVariantType variantType;
        public MethodInfo method;
    }
}
