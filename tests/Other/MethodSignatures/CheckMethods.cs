using NUnit.Framework;

using Mighty.Interfaces;
using System.Data.Common;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Threading;

namespace Mighty.MethodSignatures
{
    [TestFixture]
    public class CheckMethods
    {
        private readonly MethodChecker<MightyOrmAbstractInterface<CheckMethods>, CheckMethods> interfaceDefinedMethods;
        private readonly MethodChecker<MightyOrm, dynamic> dynamicMightyDefinedMethods;
        private readonly MethodChecker<MightyOrm<CheckMethods>, CheckMethods> genericMightyDefinedMethods;

        /// <summary>
        /// This initialisation stage already does quite a lot of sanity checking as to whether the methods on
        /// each class/interface are as expected.
        /// </summary>
        public CheckMethods()
        {
            // we are also using CheckMethods here as just a placeholder type
            interfaceDefinedMethods = new MethodChecker<MightyOrmAbstractInterface<CheckMethods>, CheckMethods>(true, true);
            dynamicMightyDefinedMethods = new MethodChecker<MightyOrm, dynamic>(false, false);
            genericMightyDefinedMethods = new MethodChecker<MightyOrm<CheckMethods>, CheckMethods>(false, true);
        }

        /// <summary>
        /// In terms of static methods, we are expecting the factory method inherited from Massive and nothing else.
        /// </summary>
        [Test]
        public void StaticFactoryMethods_Present()
        {
            Assert.AreEqual(0, interfaceDefinedMethods[MightySyncType.Static].MethodCount);
            Assert.AreEqual(1, dynamicMightyDefinedMethods[MightySyncType.Static].MethodCount);
            Assert.AreEqual(1, genericMightyDefinedMethods[MightySyncType.Static].MethodCount);
        }

        /// <summary>
        /// We are not expecting any additional methods to be defined on <see cref="MightyOrm"/> (for dynamic type) itself,
        /// they should all be defined in what it derives from, i.e. <see cref="MightyOrm{T}"/> with a T of dynamic.
        /// </summary>
        [Test]
        public void MightyOrm_IsJustMightyOrmDynamic()
        {
            Assert.AreEqual(0, dynamicMightyDefinedMethods[MightySyncType.SyncOnly].MethodCount);
            Assert.AreEqual(0, dynamicMightyDefinedMethods[MightySyncType.Sync].MethodCount);
            Assert.AreEqual(0, dynamicMightyDefinedMethods[MightySyncType.Async].MethodCount);
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
                genericMightyDefinedMethods[MightySyncType.SyncOnly].MethodCount);
            Assert.AreEqual(
                interfaceDefinedMethods[MightySyncType.Sync].MethodCount,
                genericMightyDefinedMethods[MightySyncType.Sync].MethodCount);
            Assert.AreEqual(
                interfaceDefinedMethods[MightySyncType.Async].MethodCount,
                genericMightyDefinedMethods[MightySyncType.Async].MethodCount);
        }

        /// <summary>
        /// As in the case of the caching tests, it's a bit of extra effort to keep these numbers in this test
        /// up to date but it's probably worth it as a sanity check that any changes required here correspond
        /// only to intended changes elsewhere.
        /// </summary>
        /// <remarks>
        /// Given the comments on the two tests just above, in all other methods below here it makes sense to
        /// treat the abstract interface methods as the canonical set of methods for all remaining checks.
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
            Assert.AreEqual(
#if NET40
                0,
#else
#if KEY_VALUES
                140,
#else
                138,
#endif
#endif
                interfaceDefinedMethods[MightySyncType.Async].MethodCount);
        }

        private const string CreateCommand = "CreateCommand";
        private const string OpenConnection = "OpenConnection";
        private const string KeyValues = "KeyValues";
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
        /// All sync methods must have two async variants (one with and one without a <see cref="CancellationToken"/>),
        /// and vice versa, and the <see cref="CancellationToken"/> must occur in the right place in the args list.
        /// </summary>
        /// <remarks>
        /// We've already checked the method return types when gathering the lists, so here we only need to check
        /// that the parameters correspond.
        /// </remarks>
        [Test]
        [Ignore("Not implemented")]
        public void SyncAndAsyncMethods_Correspond()
        {
        }

        /// <summary>
        /// *** May not need this given the above? ***
        /// All async methods must have a <see cref="CancellationToken"/> and non-<see cref="CancellationToken"/> variant,
        /// and the <see cref="CancellationToken"/> must occur in the right place in the args list.
        /// </summary>
        [Test]
        [Ignore("Not implemented")]
        public void AsyncMethods_HaveCancellationTokenAndNonCancellationTokenVariants()
        {
        }

        /// <summary>
        /// All sync methods must have a <see cref="DbConnection"/> and non-<see cref="DbConnection"/> variant,
        /// and the <see cref="DbConnection"/> must occur in the right place in the args list.
        /// </summary>
        [Test]
        [Ignore("Not implemented")]
        public void SyncMethods_HaveDbConnectionAndNonDbConnectionVariants()
        {
#if false
            var hasDbConnection = interfaceDefinedMethods
                                    .SyncMethods
                                    .Where(m =>
                                        m.Name != OpenConnection &&
                                        m.Name != KeyValues)
                                    .GroupBy(m => m.ContainsParamType(dbConnectionType));

            Assert.AreEqual(hasDbConnection[false].Count(), )

            List<MethodInfo> dbConnectionMethods = new List<MethodInfo>();
            List<MethodInfo> nonDbConnectionMethods = new List<MethodInfo>();
            interfaceDefinedMethods
                .SyncMethods
                .Where(m =>
                    m.Name != OpenConnection &&
                    m.Name != KeyValues)
                .ForEach(m => {
                    if (m.ContainsParamType(dbConnectionType)) dbConnectionMethods.Add(m);
                    else nonDbConnectionMethods.Add(m);
                });
            nonDbConnectionMethods.ForEach(m => {
                var o = m;
            });
#endif
        }

        /// <summary>
        /// Confirm that all the hand coded method types were actually found
        /// </summary>
        /// <remarks>
        /// TO DO: I think <see cref="MightyMethodType.Insert"/> isn't currently being found, for a start. What's going on?
        /// </remarks>
        [Test]
        [Ignore("Not implemented")]
        public void AllMethodTypes_ArePresent()
        {

        }
    }
}
