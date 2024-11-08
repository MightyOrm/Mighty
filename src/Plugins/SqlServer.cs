using System;
using System.Data.Common;
using System.Reflection;

namespace Mighty.Plugins
{
    internal class SqlServer : PluginBase
    {
        #region Provider support
        // we must use new because there are no overrides on static methods, see e.g. http://stackoverflow.com/q/7839691
        new static public string GetProviderFactoryClassName(string loweredProviderName)
        {
            switch (loweredProviderName)
            {
                case "system.data.sqlclient":
                    return "System.Data.SqlClient.SqlClientFactory";

                case "microsoft.data.sqlclient":
                    return "Microsoft.Data.SqlClient.SqlClientFactory";

                default:
                    return null;
            }
        }
        #endregion

        #region SQL
        override public string BuildSelect(string columns, string tableName, string where, string orderBy = null, int limit = 0)
        {
            return BuildTopSelect(columns, tableName, where, orderBy, limit);
        }

        override public PagingQueryPair BuildPagingQueryPair(string columns, string tableNameOrJoinSpec, string orderBy, string where,
            int limit, int offset)
        {
            return BuildRowNumberPagingQueryPair(columns, tableNameOrJoinSpec, orderBy, where, limit, offset);
        }
        #endregion

        #region Keys and sequences
        override public string IdentityRetrievalFunction { get; protected set; } = "SCOPE_IDENTITY()";
        #endregion

        #region DbCommand
        private static PropertyInfo InnerConnectionProperty;
        private static PropertyInfo CurrentTransactionProperty;
        private static PropertyInfo ParentTransactionProperty;

        /// <summary>
        /// Enlist the command to the transaction on the current connection, if any
        /// </summary>
        /// <param name="mighty"></param>
        /// <param name="command"></param>
        /// <param name="connection"></param>
        override public void SetProviderSpecificCommandProperties<T>(MightyOrm<T> mighty, DbCommand command, DbConnection connection = null)
        {
            if (connection == null ||
                command.Transaction != null ||
                !mighty.SqlServerAutoEnlistCommandsToTransactions) return;
            if (InnerConnectionProperty == null)
                InnerConnectionProperty = connection.GetType().GetProperty("InnerConnection", BindingFlags.NonPublic | BindingFlags.Instance);
            var innerConnection = InnerConnectionProperty.GetValue(connection, null);
            if (CurrentTransactionProperty == null)
                CurrentTransactionProperty = innerConnection.GetType().GetProperty("CurrentTransaction", BindingFlags.NonPublic | BindingFlags.Instance);
            var currentTransaction = CurrentTransactionProperty.GetValue(innerConnection, null);
            if (currentTransaction == null) return;
            if (ParentTransactionProperty == null)
                ParentTransactionProperty = currentTransaction.GetType().GetProperty("Parent", BindingFlags.NonPublic | BindingFlags.Instance);
            DbTransaction transaction = (DbTransaction)ParentTransactionProperty.GetValue(currentTransaction, null);
            if (transaction != null)
                command.Transaction = transaction;
        }
        #endregion

        #region Prefix/deprefix parameters
        override public string PrefixParameterName(string rawName, DbCommand cmd = null)
        {
            return (cmd != null) ? rawName : ("@" + rawName);
        }
        #endregion
    }
}