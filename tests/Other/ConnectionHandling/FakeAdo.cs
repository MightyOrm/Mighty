using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Mighty.ConnectionProviders;
using Mighty.Plugins;
using Mighty.Profiling;
#if !NET40
using System.Threading;
using System.Threading.Tasks;
#endif

namespace Mighty.ZConnectionHandling
{
    public enum ConnectionMode { Local, Attached, Argument, Same, Conflict }
    public enum Consumption { All, FirstRow, FirstResult }

    internal sealed class ConnectionCase : IDisposable
    {
        public readonly FakeFactory Factory = new FakeFactory();
        public readonly FakeConnection Attached = new FakeConnection();
        public readonly FakeConnection Argument = new FakeConnection();
        public readonly FakeCommand Command = new FakeCommand();
        public readonly MightyOrm Db;
        public readonly FakePlugin Plugin;
        public readonly ConnectionMode Mode;
        public readonly DbConnection Supplied;
        public readonly FakeConnection Expected;
        public readonly FakeTransaction Transaction;

        public ConnectionCase(ConnectionMode mode)
        {
            Mode = mode;
            Attached.Open();
            Argument.Open();
            if (mode == ConnectionMode.Attached || mode == ConnectionMode.Same || mode == ConnectionMode.Conflict)
                Command.Connection = Attached;
            Supplied = mode == ConnectionMode.Same ? Attached :
                mode == ConnectionMode.Argument || mode == ConnectionMode.Conflict ? Argument : null;
            Expected = mode == ConnectionMode.Local ? null : mode == ConnectionMode.Argument ? Argument : Attached;
            if (Expected != null)
            {
                Transaction = new FakeTransaction(Expected);
                Command.Transaction = Transaction;
            }
            // Explicit profiling prevents these tests contributing to the suite-wide SQL totals.
            Db = new MightyOrm(profiler: new DataProfiler(factoryWrapping: f => f), connectionProvider: new FakeProvider(Factory));
            Plugin = (FakePlugin)Db.Plugin;
        }

        public void Dispose()
        {
            Command.Dispose();
            if (Transaction != null) Transaction.Dispose();
            Attached.Dispose();
            Argument.Dispose();
            foreach (var connection in Factory.Connections) connection.Dispose();
        }
    }

    public sealed class FakeProvider : ConnectionProvider
    {
        public FakeProvider(FakeFactory factory)
        {
            ProviderFactoryInstance = factory;
            DatabasePluginType = typeof(FakePlugin);
            ConnectionString = "fake";
        }
        public override ConnectionProvider Init(string connectionStringOrName, string providerName) { return this; }
    }

    public sealed class FakePlugin : PluginBase
    {
        public readonly List<DbConnection> ReaderConnections = new List<DbConnection>();
        public int TransactionChecks;
        public override string BuildSelect(string columns, string tableName, string where, string orderBy = null, int limit = 0) { throw new NotSupportedException(); }
        public override PagingQueryPair BuildPagingQueryPair(string columns, string tableNameOrJoinSpec, string orderBy, string where, int limit, int offset) { throw new NotSupportedException(); }
        public override string PrefixParameterName(string rawName, DbCommand cmd = null) { return rawName; }
        public override bool RequiresWrappingTransaction(DbCommand cmd) { TransactionChecks++; return true; }
        public override DbDataReader ExecuteDereferencingReader(DbCommand cmd, CommandBehavior behavior, DbConnection conn)
        {
            ReaderConnections.Add(conn);
            return cmd.ExecuteReader(behavior);
        }
#if !NET40
        public override Task<DbDataReader> ExecuteDereferencingReaderAsync(DbCommand cmd, CommandBehavior behavior, DbConnection conn, CancellationToken token)
        {
            ReaderConnections.Add(conn);
            return cmd.ExecuteReaderAsync(behavior, token);
        }
#endif
    }

    public sealed class FakeFactory : DbProviderFactory
    {
        public readonly List<FakeConnection> Connections = new List<FakeConnection>();
        public override DbConnection CreateConnection()
        {
            var connection = new FakeConnection();
            Connections.Add(connection);
            return connection;
        }
    }

    public sealed class FakeConnection : DbConnection
    {
        public bool WasDisposed;
        public readonly List<FakeTransaction> Transactions = new List<FakeTransaction>();
        private ConnectionState state;
        // Distinct instances deliberately compare equal: connection selection must use reference identity.
        public override bool Equals(object obj) { return obj is FakeConnection; }
        public override int GetHashCode() { return 1; }
        public override string ConnectionString { get; set; }
        public override string Database { get { return "fake"; } }
        public override string DataSource { get { return "fake"; } }
        public override string ServerVersion { get { return "1"; } }
        public override ConnectionState State { get { return state; } }
        public override void ChangeDatabase(string databaseName) { throw new NotSupportedException(); }
        public override void Open()
        {
            if (WasDisposed) throw new ObjectDisposedException("connection");
            state = ConnectionState.Open;
        }
        public override void Close() { state = ConnectionState.Closed; }
        protected override void Dispose(bool disposing) { WasDisposed = true; Close(); base.Dispose(disposing); }
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            var transaction = new FakeTransaction(this);
            Transactions.Add(transaction);
            return transaction;
        }
        protected override DbCommand CreateDbCommand() { return new FakeCommand { Connection = this }; }
    }

    public sealed class FakeTransaction : DbTransaction
    {
        private readonly DbConnection connection;
        public bool WasDisposed;
        public bool Committed;
        public FakeTransaction(DbConnection connection) { this.connection = connection; }
        public override IsolationLevel IsolationLevel { get { return IsolationLevel.ReadCommitted; } }
        protected override DbConnection DbConnection { get { return connection; } }
        public override void Commit() { Committed = true; }
        public override void Rollback() { }
        protected override void Dispose(bool disposing) { WasDisposed = true; base.Dispose(disposing); }
    }

    public sealed class InjectedFailureException : Exception { }

    public sealed class FakeCommand : DbCommand
    {
        public bool FailExecution;
        public bool FailReading;
        public bool WasDisposed;
        public int Executions;
        public FakeReader Reader;
        public override string CommandText { get; set; } = "fake";
        public override int CommandTimeout { get; set; }
        public override CommandType CommandType { get; set; }
        public override bool DesignTimeVisible { get; set; }
        public override UpdateRowSource UpdatedRowSource { get; set; }
        protected override DbConnection DbConnection { get; set; }
        protected override DbTransaction DbTransaction { get; set; }
        protected override DbParameterCollection DbParameterCollection { get; } = new EmptyParameters();
        public override void Cancel() { }
        public override void Prepare() { }
        protected override DbParameter CreateDbParameter() { throw new NotSupportedException(); }
        protected override void Dispose(bool disposing) { WasDisposed = true; base.Dispose(disposing); }
        internal void CheckConnection()
        {
            if (Connection == null || Connection.State != ConnectionState.Open)
                throw new InvalidOperationException("Executing with a missing, closed or disposed connection.");
        }
        public override int ExecuteNonQuery()
        {
            Executions++;
            CheckConnection();
            if (FailExecution) throw new InjectedFailureException();
            return 1;
        }
        public override object ExecuteScalar() { return ExecuteNonQuery(); }
        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            ExecuteNonQuery();
            return Reader = new FakeReader(this);
        }
#if !NET40
        public CancellationToken LastToken;
        public TaskCompletionSource<bool> Gate;
        public readonly TaskCompletionSource<bool> Started = new TaskCompletionSource<bool>();
        private async Task<int> ExecutePendingAsync(CancellationToken token)
        {
            Executions++;
            LastToken = token;
            CheckConnection();
            Started.TrySetResult(true);
            if (Gate != null)
            {
                using (token.Register(() => Gate.TrySetCanceled())) await Gate.Task.ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            CheckConnection();
            if (FailExecution) throw new InjectedFailureException();
            return 1;
        }
        public override Task<int> ExecuteNonQueryAsync(CancellationToken token) { return ExecutePendingAsync(token); }
        public override async Task<object> ExecuteScalarAsync(CancellationToken token) { return await ExecutePendingAsync(token).ConfigureAwait(false); }
        protected override async Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken token)
        {
            await ExecutePendingAsync(token).ConfigureAwait(false);
            return Reader = new FakeReader(this);
        }
#endif
    }

    // Two result sets, three rows each; early disposal can leave actual unread rows and results.
    public sealed class FakeReader : DbDataReader
    {
        private readonly FakeCommand command;
        private int row = -1;
        private int result;
        public bool WasDisposed;
        public int RowsRead;
        public int NextResults;
        public FakeReader(FakeCommand command) { this.command = command; }
        public override bool Read()
        {
            command.CheckConnection();
            if (command.FailReading && RowsRead == 1) throw new InjectedFailureException();
            if (++row >= 3) return false;
            RowsRead++;
            return true;
        }
        public override bool NextResult() { NextResults++; row = -1; return ++result < 2; }
#if !(NETCOREAPP1_0 || NETCOREAPP1_1)
        public override void Close() { WasDisposed = true; }
#endif
        protected override void Dispose(bool disposing) { WasDisposed = true; base.Dispose(disposing); }
        public override bool IsClosed { get { return WasDisposed; } }
        public override bool HasRows { get { return true; } }
        public override int FieldCount { get { return 1; } }
        public override int Depth { get { return 0; } }
        public override int RecordsAffected { get { return -1; } }
        public override object this[int ordinal] { get { return GetValue(ordinal); } }
        public override object this[string name] { get { return GetValue(GetOrdinal(name)); } }
        public override string GetName(int ordinal) { return "Value"; }
        public override int GetOrdinal(string name) { return 0; }
        public override string GetDataTypeName(int ordinal) { return "int"; }
        public override Type GetFieldType(int ordinal) { return typeof(int); }
        public override object GetValue(int ordinal) { return result * 10 + row; }
        public override int GetValues(object[] values) { values[0] = GetValue(0); return 1; }
        public override bool IsDBNull(int ordinal) { return false; }
        public override IEnumerator GetEnumerator() { throw new NotSupportedException(); }
#if NETFRAMEWORK
        public override DataTable GetSchemaTable() { throw new NotSupportedException(); }
#endif
        public override bool GetBoolean(int ordinal) { throw new NotSupportedException(); }
        public override byte GetByte(int ordinal) { throw new NotSupportedException(); }
        public override long GetBytes(int ordinal, long offset, byte[] buffer, int bufferOffset, int length) { throw new NotSupportedException(); }
        public override char GetChar(int ordinal) { throw new NotSupportedException(); }
        public override long GetChars(int ordinal, long offset, char[] buffer, int bufferOffset, int length) { throw new NotSupportedException(); }
        public override DateTime GetDateTime(int ordinal) { throw new NotSupportedException(); }
        public override decimal GetDecimal(int ordinal) { throw new NotSupportedException(); }
        public override double GetDouble(int ordinal) { throw new NotSupportedException(); }
        public override float GetFloat(int ordinal) { throw new NotSupportedException(); }
        public override Guid GetGuid(int ordinal) { throw new NotSupportedException(); }
        public override short GetInt16(int ordinal) { throw new NotSupportedException(); }
        public override int GetInt32(int ordinal) { return (int)GetValue(ordinal); }
        public override long GetInt64(int ordinal) { throw new NotSupportedException(); }
        public override string GetString(int ordinal) { throw new NotSupportedException(); }
#if !NET40
        public CancellationToken ReadToken;
        public CancellationToken NextResultToken;
        public override Task<bool> ReadAsync(CancellationToken token)
        {
            ReadToken = token;
            token.ThrowIfCancellationRequested();
            return Task.FromResult(Read());
        }
        public override Task<bool> NextResultAsync(CancellationToken token)
        {
            NextResultToken = token;
            token.ThrowIfCancellationRequested();
            return Task.FromResult(NextResult());
        }
#endif
    }

    internal sealed class EmptyParameters : DbParameterCollection
    {
        public override int Count { get { return 0; } }
        public override object SyncRoot { get { return this; } }
#if NETFRAMEWORK
        public override bool IsReadOnly { get { return true; } }
        public override bool IsFixedSize { get { return true; } }
        public override bool IsSynchronized { get { return false; } }
#endif
        public override int Add(object value) { throw new NotSupportedException(); }
        public override void AddRange(Array values) { throw new NotSupportedException(); }
        public override void Clear() { }
        public override bool Contains(object value) { return false; }
        public override bool Contains(string value) { return false; }
        public override void CopyTo(Array array, int index) { }
        public override IEnumerator GetEnumerator() { return new DbParameter[0].GetEnumerator(); }
        public override int IndexOf(object value) { return -1; }
        public override int IndexOf(string parameterName) { return -1; }
        public override void Insert(int index, object value) { throw new NotSupportedException(); }
        public override void Remove(object value) { throw new NotSupportedException(); }
        public override void RemoveAt(int index) { throw new NotSupportedException(); }
        public override void RemoveAt(string parameterName) { throw new NotSupportedException(); }
        protected override DbParameter GetParameter(int index) { throw new NotSupportedException(); }
        protected override DbParameter GetParameter(string parameterName) { throw new NotSupportedException(); }
        protected override void SetParameter(int index, DbParameter value) { throw new NotSupportedException(); }
        protected override void SetParameter(string parameterName, DbParameter value) { throw new NotSupportedException(); }
    }
}
