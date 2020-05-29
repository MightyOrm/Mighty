using NUnit.Framework;

using Mighty.Interfaces;
using System.Data.Common;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework.Constraints;

namespace Mighty.MethodSignatures
{
    [TestFixture]
    public class CheckMethods
    {
        private readonly MethodChecker<MightyOrmAbstractInterface<ExampleGenericClass>, ExampleGenericClass> interfaceDefinedMethods;
        private readonly MethodChecker<MightyOrm, dynamic> dynamicDefinedMethods;
        private readonly MethodChecker<MightyOrm<ExampleGenericClass>, ExampleGenericClass> genericDefinedMethods;

        public class ExampleGenericClass { }

        private static readonly Type objArray = typeof(object[]);

        private bool IsParamsArg(ParameterInfo param, MethodInfo method)
        {

            bool retval;
#if NET40
            retval = param.ParameterType == objArray; // this isn't definitive, we could add check for .Name == "args" (but it still wouldn't be definitive)
#else
            retval = param.GetCustomAttribute(typeof(ParamArrayAttribute)) != null; // this is definitive
            Assert.That(retval, Is.EqualTo(param.ParameterType == objArray), "If this fails the current NET40 'params' argument identification code probably will not work");
            if (retval && param.Name != "args" && param.Name != "items")
            {
                throw new Exception($"Mighty 'params' arguments should all be named 'args' or 'items' in {method}");
            }
#endif
            return retval;
        }

        /// <summary>
        /// This initialisation stage already does quite a lot of sanity checking as to whether the methods on
        /// each class/interface are as expected.
        /// </summary>
        public CheckMethods()
        {
            // we are also using CheckMethods here as just a placeholder type
            interfaceDefinedMethods = new MethodChecker<MightyOrmAbstractInterface<ExampleGenericClass>, ExampleGenericClass>(true, true);
            dynamicDefinedMethods = new MethodChecker<MightyOrm, dynamic>(false, false);
            genericDefinedMethods = new MethodChecker<MightyOrm<ExampleGenericClass>, ExampleGenericClass>(false, true);
        }

        /// <summary>
        /// In terms of static methods, we are expecting the factory method inherited from Massive and nothing else.
        /// </summary>
        [Test]
        public void StaticFactoryMethods_Present()
        {
            Assert.AreEqual(0, interfaceDefinedMethods[MightySyncType.Static].MethodCount);
            Assert.AreEqual(1, dynamicDefinedMethods[MightySyncType.Static].MethodCount);
            Assert.AreEqual(1, genericDefinedMethods[MightySyncType.Static].MethodCount);
        }

        /// <summary>
        /// We are not expecting any additional methods to be defined on <see cref="MightyOrm"/> (for dynamic type) itself,
        /// they should all be defined in what it derives from, i.e. <see cref="MightyOrm{T}"/> with a T of dynamic.
        /// </summary>
        [Test]
        public void MightyOrm_IsJustMightyOrmDynamic()
        {
            Assert.AreEqual(0, dynamicDefinedMethods[MightySyncType.SyncOnly].MethodCount);
            Assert.AreEqual(0, dynamicDefinedMethods[MightySyncType.Sync].MethodCount);
#if !NET40
            Assert.AreEqual(0, dynamicDefinedMethods[MightySyncType.Async].MethodCount);
#endif
        }

        /// <summary>
        /// We don't expect the generic class to have any public methods that are not on the abstract interface.
        /// This test only checks the total counts, but assuming that the class actually implements the abstract interface,
        /// that is all we need to check (we must have as many methods, we just need to check that we don't have more).
        /// </summary>
        [Test]
        public void GenericClass_NoExtraMethods()
        {
            Assert.AreEqual(
                interfaceDefinedMethods[MightySyncType.SyncOnly].MethodCount,
                genericDefinedMethods[MightySyncType.SyncOnly].MethodCount);
            Assert.AreEqual(
                interfaceDefinedMethods[MightySyncType.Sync].MethodCount,
                genericDefinedMethods[MightySyncType.Sync].MethodCount);
#if !NET40
            Assert.AreEqual(
                interfaceDefinedMethods[MightySyncType.Async].MethodCount,
                genericDefinedMethods[MightySyncType.Async].MethodCount);
#endif
        }


        /// <summary>
        /// Confirm that all the hand coded method types were actually found
        /// </summary>
        [Test]
        public void AllMethodTypes_ArePresent()
        {
            foreach (MightyMethodType type in Enum.GetValues(typeof(MightyMethodType)))
            {
                // illegal methods obviously not supposed to be present; factory method not present on interface (and checked for in tests above)
                Assert.That(
                    interfaceDefinedMethods[type].MethodCount,
                    (type == MightyMethodType._Illegal || type == MightyMethodType.Factory) ?
                        (Constraint)Is.EqualTo(0) :
                        (Constraint)Is.GreaterThan(0)
                );
            }
        }

        /// <summary>
        /// As in the case of the caching tests, it's a bit of extra effort to keep the numbers in this test
        /// up to date but it's probably worth it as a sanity check that any changes required here correspond
        /// only to intended changes elsewhere.
        /// </summary>
        /// <remarks>
        /// Given the comments on the tests above, in all other methods below it makes sense to treat
        /// the abstract interface methods as the canonical set of methods for all remaining tests.
        /// </remarks>
        [Test]
        public void Interface_MethodCounts()
        {
            Assert.AreEqual(10, interfaceDefinedMethods[MightySyncType.SyncOnly].MethodCount);
            Assert.AreEqual(
#if KEY_VALUES
                72,
#else
                71,
#endif
                interfaceDefinedMethods[MightySyncType.Sync].MethodCount);
#if !NET40
            Assert.AreEqual(
#if KEY_VALUES
                144,
#else
                142,
#endif
                interfaceDefinedMethods[MightySyncType.Async].MethodCount);
#endif
        }

        private const string CreateCommand = "CreateCommand";
        private const string OpenConnection = "OpenConnection";

        private static readonly Type dbConnectionType = typeof(DbConnection);
        private static readonly Type cancellationTokenType = typeof(CancellationToken);

        /// <summary>
        /// We have three CreateCommand variants.
        /// </summary>
        [Test]
        public void SyncOnlyMethods_CreateCommandCount()
        {
            Assert.AreEqual(3,
                interfaceDefinedMethods[MightySyncType.SyncOnly]
                    .Where(m => m.Name.StartsWith(CreateCommand)).Select(m => m).Count());
        }

        /// <remarks>
        /// Sync only methods do not have a <see cref="DbConnection"/> param, except for two of
        /// the three variants of <see cref="MightyOrm{T}.CreateCommand(string, DbConnection)"/>
        /// (as just checked above), which do.
        /// Because all other sync methods which have a <see cref="DbConnection"/> also have an async variant
        /// (and so are not sync ONLY, as it is meant here).
        /// </remarks>
        [Test]
        public void SyncOnlyMethods_DoNotContainDbConnection()
        {
            interfaceDefinedMethods[MightySyncType.SyncOnly]
                .Where(m => !m.Name.StartsWith(CreateCommand))
                .DoNotContainParamType(dbConnectionType);
        }

        /// <summary>
        /// We expect just one OpenConnection method.
        /// TO DO: Do we want `StartsWith` here, and in the test above?
        /// </summary>
        [Test]
        public void SyncMethods_OpenConnectionCount()
        {
            Assert.AreEqual(1,
                interfaceDefinedMethods[MightySyncType.Sync]
                    .Where(m => m.Name.StartsWith(OpenConnection)).Count());
        }

        /// <summary>
        /// Sync-only methods must not contain a <see cref="CancellationToken"/>
        /// </summary>
        [Test]
        public void SyncOnlyMethods_DoNotContainCancellationToken()
        {
            interfaceDefinedMethods[MightySyncType.SyncOnly]
                .DoNotContainParamType(cancellationTokenType);
        }

        /// <summary>
        /// Sync methods must not contain a <see cref="CancellationToken"/>
        /// </summary>
        [Test]
        public void SyncMethods_DoNotContainCancellationToken()
        {
            interfaceDefinedMethods[MightySyncType.Sync]
                .DoNotContainParamType(cancellationTokenType);
        }

        /// <summary>
        /// For all methods which have one or other of <see cref="DbConnection"/> and <see cref="CancellationToken"/>,
        /// these should always (with one intentional exception!) occur in a consistent order within the parameters.
        /// </summary>
        /// <remarks>
        /// As now implemented (Mighty v4+):
        ///  - <see cref="CancellationToken"/> if present is always the first parameter
        ///  - <see cref="DbConnection"/> if present is always the last parameter before any `params` arguments
        ///         (with one exception for one variant of Single/SingleAsync, where putting it elsewhere
        ///         makes it possible to disambiguate another argment)
        /// </remarks>
        [Test]
        public void DbConnectionAndCancellationToken_OccurInTheRightPlace()
        {
            var variantMethods = interfaceDefinedMethods[mi => mi.variantType != 0];
            foreach (var method in variantMethods)
            {
                int posDbConnection = -1;
                int posCancellationToken = -1;
                bool hasParamsArguments = false;
                var theParams = method.GetParameters();
                int lastParam = theParams.Length - 1;
                for (int i = 0; i < theParams.Length; i++)
                {
                    var param = theParams[i];
                    if (i == lastParam && IsParamsArg(param, method))
                    {
                        hasParamsArguments = true;
                    }
                    else if (param.ParameterType == dbConnectionType)
                    {
                        posDbConnection = i;
                    }
                    else if (param.ParameterType == cancellationTokenType)
                    {
                        posCancellationToken = i;
                    }
                }

                if (posCancellationToken != -1)
                {
                    Assert.That(posCancellationToken, Is.EqualTo(0), $"position of {nameof(CancellationToken)} parameter in {method}");
                }

                if (posDbConnection != -1)
                {
                    var expectedPos = hasParamsArguments ? lastParam - 1 : lastParam;

                    // The one exception in the position of DbConnection, as documented in the comments on the methods themselves
                    if ((method.Name == "Single" || method.Name == "SingleAsync") &&
                        theParams[posCancellationToken + 1].Name == "where" &&
                        theParams[posCancellationToken + 3].Name == "orderBy")
                    {
                        expectedPos = posCancellationToken + 2;
                    }

                    Assert.That(posDbConnection, Is.EqualTo(expectedPos), $"position of {nameof(DbConnection)} parameter in {method}");
                }
            }
        }

        /// <summary>
        /// Confirm that all methods without param of type <paramref name="additionalParamType"/> have an exact variant
        /// with a param of that type in the right place.
        /// If <paramref name="additionalParamType"/> is <c>null</c>, then just directly compare the method signatures.
        /// At the same time, as it makes the next part of the tests easier, remove any matched methods from the original
        /// 'with' list.
        /// </summary>
        /// <param name="wihtout">List without the param</param>
        /// <param name="with">List where items hopefully with the param are to be found</param>
        /// <param name="additionalParamType">The param type</param>
        /// <param name="filter">Optional filter, if present only check methods for which filter is <c>true</c></param>
        /// <param name="compareMethodNames">Optional way to compare method names, if not present they must just be the same</param>
        private void CompareMethodVariants(
            List<MightyMethodInfo> without,
            List<MightyMethodInfo> with,
            Type additionalParamType = null,
            Func<MightyMethodInfo, bool> filter = null,
            Func<string, string, bool> compareMethodNames = null)
        {
            for (int k = 0; k < without.Count; k++)
            {
                var mmi = without[k];

                if (filter != null && !filter(mmi)) continue;

                var method = mmi.method;
                var theParams = method.GetParameters();

                int insertAt;
                if (additionalParamType == dbConnectionType)
                {
                    if (theParams.Length == 0)
                    {
                        // and will currently break the code below
                        throw new Exception("Not meant to be getting any zero parameter methods through to this line");
                    }

                    insertAt = theParams.Length;
                    if (IsParamsArg(theParams[insertAt - 1], method)) insertAt--;
                }
                else if (additionalParamType == cancellationTokenType)
                {
                    insertAt = 0;
                }
                else if (additionalParamType == null)
                {
                    // will never be triggered
                    insertAt = theParams.Length;
                }
                else
                {
                    throw new Exception($"Unsupported {nameof(additionalParamType)} = {additionalParamType}");
                }

                bool match = false;
                for (int i = 0; i < with.Count; i++)
                {
                    var otherMethod = with[i].method;
                    if (compareMethodNames != null ? compareMethodNames(method.Name, otherMethod.Name) : method.Name == otherMethod.Name)
                    {
                        var otherParams = otherMethod.GetParameters();
                        if (theParams.Length + (additionalParamType != null ? 1 : 0) == otherParams.Length)
                        {
                            match = true;
                            for (int j = 0; j < theParams.Length; j++)
                            {
                                var match1 = theParams[j];
                                var match2 = otherParams[j + (j >= insertAt ? 1 : 0)];
                                if (match1.Name != match2.Name || match1.ParameterType != match2.ParameterType)
                                {
                                    match = false;
                                    break;
                                }
                            }
                            if (match && additionalParamType != null)
                            {
                                if (otherParams[insertAt].ParameterType != additionalParamType)
                                {
                                    match = false;
                                }
                                else
                                {
                                    Assert.That(otherParams[insertAt].IsOptional, Is.EqualTo(false), $"Expection non-optional {additionalParamType.Name} param when matching to method no {additionalParamType.Name} param");
                                }
                            }
                        }
                    }
                    if (match)
                    {
                        // remove it, so that the ones to check next are only the ones that didn't match here
                        with.RemoveAt(i);
                        break;
                    }
                }
                if (!match)
                {
                    Assert.Fail($"No {(additionalParamType != null ? additionalParamType.Name : "async")} variant for {method}");
                }
            }
        }

        /// <summary>
        /// All sync methods with no <see cref="DbConnection"/> param must have an exact variant but with <see cref="DbConnection"/>.
        /// It is not the case that all the methods with <see cref="DbConnection"/> must have a variant without - but in that case
        /// at least the <see cref="DbConnection"/> param should always be optional.
        /// </summary>
        [Test]
        public void SyncMethods_HaveDbConnectionAndNonDbConnectionVariants()
        {
            var syncMethodsWithoutDbConnection = interfaceDefinedMethods[MightySyncType.Sync][MightyVariantType.None];
            var syncMethodsWithDbConnection = interfaceDefinedMethods[MightySyncType.Sync][MightyVariantType.DbConnection];

            // first confirm that all methods without DbConnection have an exact variant but with DbConnection
            CompareMethodVariants(
                syncMethodsWithoutDbConnection.mightyMethods,
                syncMethodsWithDbConnection.mightyMethods,
                dbConnectionType,
                // no DbConnection variant expected or required for these types
                mmi => !(
                    mmi.methodType == MightyMethodType.OpenConnection
#if KEY_VALUES
                    || mmi.methodType == MightyMethodType.KeyValues
#endif
                )
            );

            // now confirm that all the remaining methods at least have the DbConnection param as optional
            foreach (var method in syncMethodsWithDbConnection)
            {
                Assert.That(method.GetParameters().Where(p => p.ParameterType == dbConnectionType).FirstOrDefault().IsOptional, $"{nameof(DbConnection)} param in unmatched methods must be optional");
            }
        }

#if !NET40
        /// <summary>
        /// All sync methods must have an async variant without a <see cref="CancellationToken"/>
        /// </summary>
        /// <remarks>
        /// We've already checked the method return types when gathering the lists, so here we only need to check
        /// that the parameters correspond.
        /// </remarks>
        [Test]
        public void SyncAndAsyncMethods_Correspond()
        {
            var syncMethods = interfaceDefinedMethods[MightySyncType.Sync];
            var asyncMethodsWithoutToken = interfaceDefinedMethods[MightySyncType.Async][mi => (mi.variantType & MightyVariantType.CancellationToken) == 0];

            // confirm that all sync methods have an exact async variant without a CancellationToken
            CompareMethodVariants(
                syncMethods.mightyMethods,
                asyncMethodsWithoutToken.mightyMethods,
                compareMethodNames: (s, a) => a == s + "Async");

            // and confirm that no unmatched async methods are left
            Assert.That(asyncMethodsWithoutToken.mightyMethods.Count, Is.EqualTo(0), $"Expected no unmatched async methods");
        }

        /// <summary>
        /// All async methods must have a <see cref="CancellationToken"/> and non-<see cref="CancellationToken"/> variant.
        /// </summary>
        [Test]
        public void AsyncMethods_HaveCancellationTokenAndNonCancellationTokenVariants()
        {
            var asyncMethodsWithoutToken = interfaceDefinedMethods[MightySyncType.Async][mi => (mi.variantType & MightyVariantType.CancellationToken) == 0];
            var asyncMethodsWithToken = interfaceDefinedMethods[MightySyncType.Async][mi => (mi.variantType & MightyVariantType.CancellationToken) == MightyVariantType.CancellationToken];

            // confirm that all methods without a CancellationToken have an exact variant but with a CancellationToken
            CompareMethodVariants(
                asyncMethodsWithoutToken.mightyMethods,
                asyncMethodsWithToken.mightyMethods,
                cancellationTokenType);

            // and confirm that nothing with a CancellationToken was left unmatched
            Assert.That(asyncMethodsWithToken.mightyMethods.Count, Is.EqualTo(0), $"Expected no unmatched methods with a {cancellationTokenType.Name}");
        }
#endif
    }
}
