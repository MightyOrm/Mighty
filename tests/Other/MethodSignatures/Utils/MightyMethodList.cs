using Mighty.MethodSignatures;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Mighty.MethodSignatures
{
    public class MightyMethodList : IEnumerable<MethodInfo>
    {
        public readonly List<MightyMethodInfo> mightyMethods;

        public int MethodCount
        {
            get
            {
                return mightyMethods.Count;
            }
        }

        public MightyMethodList(IEnumerable<MightyMethodInfo> _methods)
        {
            mightyMethods = _methods.ToList();
        }

        public MightyMethodList this[MightyMethodType type]
        {
            get
            {
                return new MightyMethodList(mightyMethods.Where(m => m.methodType == type));
            }
        }

        public MightyMethodList this[MightySyncType type]
        {
            get
            {
                return new MightyMethodList(mightyMethods.Where(m => m.syncType == type));
            }
        }

        public MightyMethodList this[MightyParamsType type]
        {
            get
            {
                return new MightyMethodList(mightyMethods.Where(m => m.paramsType == type));
            }
        }

        public MightyMethodList this[MightyVariantType type]
        {
            get
            {
                return new MightyMethodList(mightyMethods.Where(m => m.variantType == type));
            }
        }

        public MightyMethodList this[Func<MightyMethodInfo, bool> filter]
        {
            get
            {
                return new MightyMethodList(mightyMethods.Where(m => filter(m)));
            }
        }

        private IEnumerable<MethodInfo> GetMethods()
        {
            foreach (var mmi in mightyMethods)
            {
                yield return mmi.method;
            }
        }

        public IEnumerator<MethodInfo> GetEnumerator()
        {
            return GetMethods().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetMethods().GetEnumerator();
        }
    }
}
