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

        public CheckMethods()
        {
            interfaceDefinedMethods = new MethodChecker<MightyOrmAbstractInterface<CheckMethods>, CheckMethods>(true, true);
            dynamicMightyDefinedMethods = new MethodChecker<MightyOrm, dynamic>(false, false);
            genericMightyDefinedMethods = new MethodChecker<MightyOrm<CheckMethods>, CheckMethods>(false, true);
        }

        [Test]
        public void StaticFactoryMethods_Present()
        {
            Assert.AreEqual(0, interfaceDefinedMethods.StaticMethods.Count);
            Assert.AreEqual(1, dynamicMightyDefinedMethods.StaticMethods.Count);
            Assert.AreEqual(1, genericMightyDefinedMethods.StaticMethods.Count);
        }

        [Test]
        public void MightyOrm_IsJustMightyOrmDynamic()
        {
            Assert.AreEqual(0, dynamicMightyDefinedMethods.SyncOnlyMethods.Count);
            Assert.AreEqual(0, dynamicMightyDefinedMethods.SyncMethods.Count);
#if !NET40
            Assert.AreEqual(0, dynamicMightyDefinedMethods.AsyncMethods.Count);
#endif
        }

        [Test]
        public void Interface_MethodCounts()
        {
            Assert.AreEqual(10, interfaceDefinedMethods.SyncOnlyMethods.Count);
#if KEY_VALUES
            Assert.AreEqual(70, interfaceDefinedMethods.SyncMethods.Count);
#else
            Assert.AreEqual(69, interfaceDefinedMethods.SyncMethods.Count);
#endif
#if !NET40
#if KEY_VALUES
            Assert.AreEqual(136, interfaceDefinedMethods.AsyncMethods.Count);
#else
            Assert.AreEqual(134, interfaceDefinedMethods.AsyncMethods.Count);
#endif
#endif
        }

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

        [Test]
        public void SyncOnlyMethods_DoNotContainCancellationToken()
        {
            interfaceDefinedMethods
                .SyncOnlyMethods
                .DoNotContainParamType(cancellationTokenType);
        }

        [Test]
        public void SyncMethods_HaveDbConnectionAndNonDbConnectionVariants()
        {
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
        }
    }
}
