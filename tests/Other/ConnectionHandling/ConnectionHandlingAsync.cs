#if !NET40
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Mighty.ZConnectionHandling
{
    public partial class ConnectionHandling
    {
        public static IEnumerable<TestCaseData> AsyncExecutionCases
        {
            get
            {
                foreach (var scalar in new[] { false, true })
                foreach (var mode in BorrowingModes)
                foreach (var fail in new[] { false, true })
                foreach (var explicitToken in new[] { false, true })
                    yield return new TestCaseData(scalar, mode, fail, explicitToken);
            }
        }

        public static IEnumerable<TestCaseData> AsyncQueryCases
        {
            get
            {
                foreach (var mode in BorrowingModes)
                foreach (var multiple in new[] { false, true })
                foreach (var consumption in new[] { Consumption.All, Consumption.FirstRow, Consumption.FirstResult })
                foreach (var explicitToken in new[] { false, true })
                    if (multiple || consumption != Consumption.FirstResult)
                        yield return new TestCaseData(mode, multiple, consumption, explicitToken);
            }
        }

        [TestCaseSource(nameof(AsyncExecutionCases))]
        public async Task ExecuteOrScalarAsync_PreservesOwnershipAndAllowsReuse(bool scalar, ConnectionMode mode, bool fail, bool explicitToken)
        {
            using (var c = new ConnectionCase(mode))
            using (var cancellation = new CancellationTokenSource())
            {
                c.Command.FailExecution = fail;
                var token = explicitToken ? cancellation.Token : CancellationToken.None;
                for (int attempt = 1; attempt <= 2; attempt++)
                {
                    await AssertOutcome(ExecuteAsync(c, scalar, explicitToken, token), fail);
                    Assert.That(c.Command.LastToken, Is.EqualTo(token));
                    AssertExecutionOwnership(c, attempt);
                }
            }
        }

        [TestCase(false, false), TestCase(false, true), TestCase(true, false), TestCase(true, true)]
        public async Task ExecuteOrScalarAsync_RejectsDistinctConnections(bool scalar, bool explicitToken)
        {
            using (var c = new ConnectionCase(ConnectionMode.Conflict))
            {
                var exception = await CaptureArgumentException(ExecuteAsync(c, scalar, explicitToken, CancellationToken.None));
                Assert.That(exception.ParamName, Is.EqualTo("connection"));
                AssertConflict(c);
            }
        }

        [TestCaseSource(nameof(ExecutionCases)), Timeout(10000)]
        public async Task ExecuteOrScalarAsync_KeepsConnectionAliveUntilCompletion(bool scalar, ConnectionMode mode, bool fail)
        {
            using (var c = new ConnectionCase(mode))
            using (var cancellation = new CancellationTokenSource())
            {
                c.Command.Gate = new TaskCompletionSource<bool>();
                c.Command.FailExecution = fail;
                var operation = ExecuteAsync(c, scalar, true, cancellation.Token);
                try
                {
                    await c.Command.Started.Task;
                    Assert.That(operation.IsCompleted, Is.False);
                    c.Command.CheckConnection();
                    Assert.That(c.Command.LastToken, Is.EqualTo(cancellation.Token));
                }
                finally { c.Command.Gate.TrySetResult(true); }
                await AssertOutcome(operation, fail);
                AssertExecutionOwnership(c, 1);
            }
        }

        [TestCaseSource(nameof(ExecutionModes)), Timeout(10000)]
        public async Task ExecuteOrScalarAsync_CancellationDisposesOnlyOwnedConnections(bool scalar, ConnectionMode mode)
        {
            using (var c = new ConnectionCase(mode))
            using (var cancellation = new CancellationTokenSource())
            {
                c.Command.Gate = new TaskCompletionSource<bool>();
                var operation = ExecuteAsync(c, scalar, true, cancellation.Token);
                try { await c.Command.Started.Task; }
                finally { cancellation.Cancel(); }
                await AssertCancelled(operation);
                Assert.That(c.Command.LastToken, Is.EqualTo(cancellation.Token));
                AssertExecutionOwnership(c, 1);
            }
        }

        [TestCaseSource(nameof(AsyncQueryCases))]
        public async Task QueryAsync_PropagatesConnectionAndDisposesResources(ConnectionMode mode, bool multiple, Consumption consumption, bool explicitToken)
        {
            using (var c = new ConnectionCase(mode))
            using (var cancellation = new CancellationTokenSource())
            {
                var token = explicitToken ? cancellation.Token : CancellationToken.None;
                var values = await ReadQueryAsync(c, multiple, consumption, explicitToken, token);
                Assert.That(values, Is.EqualTo(ExpectedValues(multiple, consumption)));
                Assert.That(c.Command.LastToken, Is.EqualTo(token));
                Assert.That(c.Command.Reader.ReadToken, Is.EqualTo(token));
                if (multiple && consumption == Consumption.All) Assert.That(c.Command.Reader.NextResultToken, Is.EqualTo(token));
                if (consumption != Consumption.All) Assert.That(c.Command.Reader.NextResults, Is.Zero);
                AssertQueryOwnership(c, consumption == Consumption.All);
            }
        }

        [TestCaseSource(nameof(QueryFailureCases))]
        public async Task QueryAsync_FailureDisposesOwnedResources(ConnectionMode mode, bool multiple, bool failReading)
        {
            using (var c = new ConnectionCase(mode))
            {
                c.Command.FailExecution = !failReading;
                c.Command.FailReading = failReading;
                await AssertOutcome(ReadQueryAsync(c, multiple, Consumption.All, true, CancellationToken.None), true);
                AssertQueryOwnership(c, false);
            }
        }

        [TestCase(false, false), TestCase(false, true), TestCase(true, false), TestCase(true, true)]
        public async Task QueryAsync_RejectsDistinctConnectionsBeforeExecution(bool multiple, bool explicitToken)
        {
            using (var c = new ConnectionCase(ConnectionMode.Conflict))
            {
                if (multiple)
                {
                    var results = explicitToken ? await c.Db.QueryMultipleAsync(CancellationToken.None, c.Command, c.Supplied) : await c.Db.QueryMultipleAsync(c.Command, c.Supplied);
                    Assert.That(c.Command.Executions, Is.Zero);
                    var e = results.GetAsyncEnumerator();
                    try { Assert.That((await CaptureArgumentException(e.MoveNextAsync().AsTask())).ParamName, Is.EqualTo("connection")); }
                    finally { await e.DisposeAsync(); }
                }
                else
                {
                    var results = explicitToken ? await c.Db.QueryAsync(CancellationToken.None, c.Command, c.Supplied) : await c.Db.QueryAsync(c.Command, c.Supplied);
                    Assert.That(c.Command.Executions, Is.Zero);
                    var e = results.GetAsyncEnumerator();
                    try { Assert.That((await CaptureArgumentException(e.MoveNextAsync().AsTask())).ParamName, Is.EqualTo("connection")); }
                    finally { await e.DisposeAsync(); }
                }
                AssertConflict(c);
            }
        }

        [TestCaseSource(nameof(QueryFailureCases)), Timeout(10000)]
        public async Task QueryAsync_PendingExecutionAndCancellationPreserveOwnership(ConnectionMode mode, bool multiple, bool cancel)
        {
            using (var c = new ConnectionCase(mode))
            using (var cancellation = new CancellationTokenSource())
            {
                c.Command.Gate = new TaskCompletionSource<bool>();
                var operation = ReadQueryAsync(c, multiple, Consumption.All, true, cancellation.Token);
                try
                {
                    await c.Command.Started.Task;
                    Assert.That(operation.IsCompleted, Is.False);
                    c.Command.CheckConnection();
                    Assert.That(c.Command.LastToken, Is.EqualTo(cancellation.Token));
                }
                finally
                {
                    if (cancel) cancellation.Cancel();
                    else c.Command.Gate.TrySetResult(true);
                }
                if (cancel) await AssertCancelled(operation);
                else Assert.That(await operation, Is.EqualTo(ExpectedValues(multiple, Consumption.All)));
                AssertQueryOwnership(c, !cancel);
            }
        }

        [TestCaseSource(nameof(QueryModes))]
        public async Task QueryAsync_CancellationDuringReadingDisposesReader(ConnectionMode mode, bool multiple)
        {
            using (var c = new ConnectionCase(mode))
            using (var cancellation = new CancellationTokenSource())
            {
                var operation = ReadQueryAsync(c, multiple, Consumption.All, true, cancellation.Token, cancellation);
                await AssertCancelled(operation);
                Assert.That(c.Command.Reader.RowsRead, Is.EqualTo(1));
                Assert.That(c.Command.Reader.ReadToken, Is.EqualTo(cancellation.Token));
                AssertQueryOwnership(c, false);
            }
        }

        private static async Task ExecuteAsync(ConnectionCase c, bool scalar, bool explicitToken, CancellationToken token)
        {
            if (scalar)
            {
                var value = explicitToken ? await c.Db.ScalarAsync(token, c.Command, c.Supplied) : await c.Db.ScalarAsync(c.Command, c.Supplied);
                Assert.That(value, Is.EqualTo(1));
            }
            else
            {
                var value = explicitToken ? await c.Db.ExecuteAsync(token, c.Command, c.Supplied) : await c.Db.ExecuteAsync(c.Command, c.Supplied);
                Assert.That(value, Is.EqualTo(1));
            }
        }

        private static async Task<List<int>> ReadQueryAsync(ConnectionCase c, bool multiple, Consumption consumption, bool explicitToken, CancellationToken token, CancellationTokenSource cancelAfterRow = null)
        {
            var values = new List<int>();
            if (multiple)
            {
                var results = explicitToken ? await c.Db.QueryMultipleAsync(token, c.Command, c.Supplied) : await c.Db.QueryMultipleAsync(c.Command, c.Supplied);
                Assert.That(c.Command.Executions, Is.Zero, "Creating a query must remain lazy.");
                var outer = results.GetAsyncEnumerator();
                try
                {
                    while (await outer.MoveNextAsync())
                    {
                        var inner = outer.Current.GetAsyncEnumerator();
                        try
                        {
                            while (await inner.MoveNextAsync())
                            {
                                values.Add((int)inner.Current.Value);
                                if (cancelAfterRow != null) cancelAfterRow.Cancel();
                                if (consumption == Consumption.FirstRow) break;
                            }
                        }
                        finally { await inner.DisposeAsync(); }
                        if (consumption != Consumption.All) break;
                    }
                }
                finally { await outer.DisposeAsync(); }
            }
            else
            {
                var results = explicitToken ? await c.Db.QueryAsync(token, c.Command, c.Supplied) : await c.Db.QueryAsync(c.Command, c.Supplied);
                Assert.That(c.Command.Executions, Is.Zero, "Creating a query must remain lazy.");
                var e = results.GetAsyncEnumerator();
                try
                {
                    while (await e.MoveNextAsync())
                    {
                        values.Add((int)e.Current.Value);
                        if (cancelAfterRow != null) cancelAfterRow.Cancel();
                        if (consumption == Consumption.FirstRow) break;
                    }
                }
                finally { await e.DisposeAsync(); }
            }
            return values;
        }

        private static async Task AssertOutcome(Task operation, bool fail)
        {
            try { await operation; }
            catch (InjectedFailureException) { Assert.That(fail, Is.True); return; }
            Assert.That(fail, Is.False, "Expected the injected failure.");
        }

        private static async Task AssertCancelled(Task operation)
        {
            try { await operation; }
            catch (OperationCanceledException) { return; }
            Assert.Fail("Expected cancellation.");
        }

        private static async Task<ArgumentException> CaptureArgumentException(Task operation)
        {
            try { await operation; }
            catch (ArgumentException exception) { return exception; }
            Assert.Fail("Expected rejection of distinct connections.");
            return null;
        }
    }
}
#endif
