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
        /// This initialisation already does a lot of sanity checks as to whether the found methods on
        /// each interface are as expected.
        /// </summary>
        public CheckMethods()
        {
            interfaceDefinedMethods = new MethodChecker<MightyOrmAbstractInterface<CheckMethods>, CheckMethods>(true, true);
            dynamicMightyDefinedMethods = new MethodChecker<MightyOrm, dynamic>(false, false);
            genericMightyDefinedMethods = new MethodChecker<MightyOrm<CheckMethods>, CheckMethods>(false, true);
        }

        /// <summary>
        /// Expecting the factory methods inherited from Massive, and nothing else.
        /// </summary>
        [Test]
        public void StaticFactoryMethods_Present()
        {
            Assert.AreEqual(0, interfaceDefinedMethods.StaticMethods.Count);
            Assert.AreEqual(1, dynamicMightyDefinedMethods.StaticMethods.Count);
            Assert.AreEqual(1, genericMightyDefinedMethods.StaticMethods.Count);
        }

        /// <summary>
        /// Not expecting any additional methods to be defined on <see cref="MightyOrm"/> itself.
        /// </summary>
        [Test]
        public void MightyOrm_IsJustMightyOrmDynamic()
        {
            Assert.AreEqual(0, dynamicMightyDefinedMethods.SyncOnlyMethods.Count);
            Assert.AreEqual(0, dynamicMightyDefinedMethods.SyncMethods.Count);
#if !NET40
            Assert.AreEqual(0, dynamicMightyDefinedMethods.AsyncMethods.Count);
#endif
        }

        /// <summary>
        /// As in the case of the cache tests, it's a bit of extra effort to keep these up to
        /// date, but it's probably worth it, as a sanity check that any changes you have to make
        /// here correspond only to changes you intended to make.
        /// </summary>
        [Test]
        public void Interface_MethodCounts()
        {
            Assert.AreEqual(10, interfaceDefinedMethods.SyncOnlyMethods.Count);
#if KEY_VALUES
            Assert.AreEqual(72, interfaceDefinedMethods.SyncMethods.Count);
#else
            Assert.AreEqual(71, interfaceDefinedMethods.SyncMethods.Count);
#endif
#if !NET40
#if KEY_VALUES
            Assert.AreEqual(140, interfaceDefinedMethods.AsyncMethods.Count);
#else
            Assert.AreEqual(138, interfaceDefinedMethods.AsyncMethods.Count);
#endif
#endif
        }

        /// <summary>
        /// We don't want the generic class to have any public methods that are not on the abstract interface.
        /// (This is only checking the total counts at this point, but assuming that the class implements the
        /// abstract interface, that is all we need to check.)
        /// </summary>
        [Test]
        public void GenericClass_NoExtraMethods()
        {
            Assert.AreEqual(interfaceDefinedMethods.SyncOnlyMethods.Count, genericMightyDefinedMethods.SyncOnlyMethods.Count);
            Assert.AreEqual(interfaceDefinedMethods.SyncMethods.Count, genericMightyDefinedMethods.SyncMethods.Count);
#if !NET40
            Assert.AreEqual(interfaceDefinedMethods.AsyncMethods.Count, genericMightyDefinedMethods.AsyncMethods.Count);
#endif
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
                interfaceDefinedMethods
                    .SyncOnlyMethods
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
            interfaceDefinedMethods
                .SyncOnlyMethods
                .Where(m => !m.Name.StartsWith(CreateCommand))
                .DoNotContainParamType(dbConnectionType);
        }

        [Test]
        public void SyncMethods_OpenConnectionCount()
        {
            Assert.AreEqual(1,
                interfaceDefinedMethods
                    .SyncMethods
                    .Where(m => m.Name.StartsWith(OpenConnection)).Count());
        }

        /// <summary>
        /// Sync-only methods must not contain a <see cref="CancellationToken"/>
        /// </summary>
        [Test]
        public void SyncOnlyMethods_DoNotContainCancellationToken()
        {
            interfaceDefinedMethods
                .SyncOnlyMethods
                .DoNotContainParamType(cancellationTokenType);
        }

        /// <summary>
        /// Sync methods must not contain a <see cref="CancellationToken"/>
        /// </summary>
        [Test]
        public void SyncMethods_DoNotContainCancellationToken()
        {
            interfaceDefinedMethods
                .SyncMethods
                .DoNotContainParamType(cancellationTokenType);
        }

        /// <summary>
        /// All sync methods must have two async variants (one with and one without a <see cref="CancellationToken"/>),
        /// and vice versa.
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
        /// All async methods must have a <see cref="CancellationToken"/> and non-<see cref="CancellationToken"/> variant,
        /// and the <see cref="CancellationToken"/> must occur in the right place in the args list.
        /// </summary>
        [Test]
        [Ignore("Not implemented")]
        public void AsyncMethods_HaveCancellationTokenAndNonCancellationTokenVariants()
        {
        }

        /// <summary>
        /// All sync methods must have a <see cref="DbCommand"/> and non-<see cref="DbCommand"/> variant,
        /// and the <see cref="DbCommand"/> must occur in the right place in the args list.
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
    }
}
