using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using NUnit.Framework;

// The existing suite checks cache totals in name order. Z places this fixture after those checks.
namespace Mighty.ZConnectionHandling
{
    [TestFixture, Category("ConnectionHandling"), NonParallelizable]
    public partial class ConnectionHandling
    {
        private static readonly ConnectionMode[] BorrowingModes =
            { ConnectionMode.Local, ConnectionMode.Attached, ConnectionMode.Argument, ConnectionMode.Same };

        public static IEnumerable<TestCaseData> ExecutionModes
        {
            get
            {
                foreach (var scalar in new[] { false, true })
                foreach (var mode in BorrowingModes)
                    yield return new TestCaseData(scalar, mode);
            }
        }

        public static IEnumerable<TestCaseData> QueryModes
        {
            get
            {
                foreach (var mode in BorrowingModes)
                foreach (var multiple in new[] { false, true })
                    yield return new TestCaseData(mode, multiple);
            }
        }

        public static IEnumerable<TestCaseData> ExecutionCases
        {
            get
            {
                foreach (var scalar in new[] { false, true })
                foreach (var mode in BorrowingModes)
                foreach (var fail in new[] { false, true })
                    yield return new TestCaseData(scalar, mode, fail);
            }
        }

        public static IEnumerable<TestCaseData> QueryCases
        {
            get
            {
                foreach (var mode in BorrowingModes)
                foreach (var multiple in new[] { false, true })
                foreach (var consumption in new[] { Consumption.All, Consumption.FirstRow, Consumption.FirstResult })
                    if (multiple || consumption != Consumption.FirstResult)
                        yield return new TestCaseData(mode, multiple, consumption);
            }
        }

        public static IEnumerable<TestCaseData> QueryFailureCases
        {
            get
            {
                foreach (var mode in BorrowingModes)
                foreach (var multiple in new[] { false, true })
                foreach (var failReading in new[] { false, true })
                    yield return new TestCaseData(mode, multiple, failReading);
            }
        }

        [TestCaseSource(nameof(ExecutionCases))]
        public void ExecuteOrScalar_PreservesOwnershipAndAllowsReuse(bool scalar, ConnectionMode mode, bool fail)
        {
            using (var c = new ConnectionCase(mode))
            {
                c.Command.FailExecution = fail;
                for (int attempt = 1; attempt <= 2; attempt++)
                {
                    TestDelegate execute = () =>
                    {
                        if (scalar) Assert.That(c.Db.Scalar(c.Command, c.Supplied), Is.EqualTo(1));
                        else Assert.That(c.Db.Execute(c.Command, c.Supplied), Is.EqualTo(1));
                    };
                    if (fail) Assert.Throws<InjectedFailureException>(execute);
                    else execute();
                    AssertExecutionOwnership(c, attempt);
                }
            }
        }

        [TestCase(false), TestCase(true)]
        public void ExecuteOrScalar_RejectsDistinctConnections(bool scalar)
        {
            using (var c = new ConnectionCase(ConnectionMode.Conflict))
            {
                var exception = Assert.Throws<ArgumentException>(() =>
                {
                    if (scalar) c.Db.Scalar(c.Command, c.Supplied);
                    else c.Db.Execute(c.Command, c.Supplied);
                });
                Assert.That(exception.ParamName, Is.EqualTo("connection"));
                AssertConflict(c);
            }
        }

        [TestCaseSource(nameof(QueryCases))]
        public void Query_PropagatesConnectionAndDisposesResources(ConnectionMode mode, bool multiple, Consumption consumption)
        {
            using (var c = new ConnectionCase(mode))
            {
                var values = ReadQuery(c, multiple, consumption);
                Assert.That(values, Is.EqualTo(ExpectedValues(multiple, consumption)));
                AssertQueryOwnership(c, consumption == Consumption.All);
                if (consumption != Consumption.All) Assert.That(c.Command.Reader.NextResults, Is.Zero);
            }
        }

        [TestCaseSource(nameof(QueryFailureCases))]
        public void Query_FailureDisposesOwnedResources(ConnectionMode mode, bool multiple, bool failReading)
        {
            using (var c = new ConnectionCase(mode))
            {
                c.Command.FailExecution = !failReading;
                c.Command.FailReading = failReading;
                Assert.Throws<InjectedFailureException>(() => ReadQuery(c, multiple, Consumption.All));
                AssertQueryOwnership(c, false);
            }
        }

        [TestCase(false), TestCase(true)]
        public void Query_RejectsDistinctConnectionsBeforeEnumerationExecutes(bool multiple)
        {
            using (var c = new ConnectionCase(ConnectionMode.Conflict))
            {
                // Constructing the enumerable remains lazy; rejection happens at its first MoveNext.
                if (multiple)
                {
                    var results = c.Db.QueryMultiple(c.Command, c.Supplied);
                    Assert.That(c.Command.Executions, Is.Zero);
                    using (var e = results.GetEnumerator()) Assert.That(Assert.Throws<ArgumentException>(() => e.MoveNext()).ParamName, Is.EqualTo("connection"));
                }
                else
                {
                    var results = c.Db.Query(c.Command, c.Supplied);
                    Assert.That(c.Command.Executions, Is.Zero);
                    using (var e = results.GetEnumerator()) Assert.That(Assert.Throws<ArgumentException>(() => e.MoveNext()).ParamName, Is.EqualTo("connection"));
                }
                AssertConflict(c);
            }
        }

        private static List<int> ReadQuery(ConnectionCase c, bool multiple, Consumption consumption)
        {
            var values = new List<int>();
            if (multiple)
            {
                foreach (var result in c.Db.QueryMultiple(c.Command, c.Supplied))
                {
                    foreach (var row in result)
                    {
                        values.Add((int)row.Value);
                        if (consumption == Consumption.FirstRow) break;
                    }
                    if (consumption != Consumption.All) break;
                }
            }
            else
            {
                foreach (var row in c.Db.Query(c.Command, c.Supplied))
                {
                    values.Add((int)row.Value);
                    if (consumption == Consumption.FirstRow) break;
                }
            }
            return values;
        }

        private static int[] ExpectedValues(bool multiple, Consumption consumption)
        {
            return consumption == Consumption.FirstRow ? new[] { 0 } :
                multiple && consumption == Consumption.All ? new[] { 0, 1, 2, 10, 11, 12 } : new[] { 0, 1, 2 };
        }

        private static void AssertBorrowedOwnership(ConnectionCase c)
        {
            Assert.That(c.Factory.Connections, Is.Empty, "A borrowed connection must not open another connection.");
            Assert.That(c.Command.Connection, Is.SameAs(c.Expected));
            Assert.That(c.Attached.WasDisposed || c.Argument.WasDisposed, Is.False);
            Assert.That(c.Command.Transaction, Is.SameAs(c.Transaction));
            Assert.That(c.Transaction.WasDisposed || c.Transaction.Committed, Is.False);
            Assert.That(c.Attached.Transactions.Concat(c.Argument.Transactions), Is.Empty);
        }

        private static void AssertExecutionOwnership(ConnectionCase c, int executions)
        {
            Assert.That(c.Command.Executions, Is.EqualTo(executions));
            Assert.That(c.Command.WasDisposed, Is.False, "Execute/Scalar do not own a supplied command.");
            if (c.Mode == ConnectionMode.Local)
            {
                Assert.That(c.Command.Connection, Is.Null, "A disposed local connection must not remain attached.");
                Assert.That(c.Factory.Connections.Count, Is.EqualTo(executions));
                Assert.That(c.Factory.Connections.All(conn => conn.WasDisposed), Is.True);
            }
            else AssertBorrowedOwnership(c);
        }

        private static void AssertQueryOwnership(ConnectionCase c, bool completed)
        {
            Assert.That(c.Command.WasDisposed, Is.True, "Queries own their command and reader.");
            if (c.Command.Reader != null) Assert.That(c.Command.Reader.WasDisposed, Is.True);
            Assert.That(c.Plugin.ReaderConnections.Count, Is.EqualTo(1));
            if (c.Mode == ConnectionMode.Local)
            {
                Assert.That(c.Factory.Connections.Count, Is.EqualTo(1), "Inner result sets must share the outer connection.");
                var connection = c.Factory.Connections.Single();
                Assert.That(connection.WasDisposed, Is.True);
                Assert.That(c.Plugin.ReaderConnections.Single(), Is.SameAs(connection));
                Assert.That(c.Plugin.TransactionChecks, Is.EqualTo(1));
                var transaction = connection.Transactions.Single();
                Assert.That(transaction.WasDisposed, Is.True);
                Assert.That(transaction.Committed, Is.EqualTo(completed));
            }
            else
            {
                AssertBorrowedOwnership(c);
                Assert.That(c.Plugin.ReaderConnections.Single(), Is.SameAs(c.Expected));
                Assert.That(c.Plugin.TransactionChecks, Is.Zero, "Borrowed transactions are caller-managed.");
            }
        }

        private static void AssertConflict(ConnectionCase c)
        {
            Assert.That(c.Command.Executions, Is.Zero);
            Assert.That(c.Plugin.ReaderConnections, Is.Empty);
            Assert.That(c.Plugin.TransactionChecks, Is.Zero);
            AssertBorrowedOwnership(c);
        }
    }
}
