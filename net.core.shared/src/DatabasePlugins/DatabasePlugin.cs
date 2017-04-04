using System.Data;
using System.Data.Common;

namespace Mighty.DatabasePlugins
{
	abstract internal class DatabasePlugin
	{
		internal MightyORM _mightyInstance;

		abstract internal string GetProviderFactoryClassName(string providerName);

		abstract internal string PrefixParameterName(string rawName, DbCommand cmd = null);
		abstract internal string DeprefixParameterName(string dbParamName, DbCommand cmd);

#region DbCommand
		abstract internal DbDataReader ExecuteDereferencingReader(DbCommand cmd, DbConnection conn);
		abstract internal bool RequiresWrappingTransaction(DbCommand cmd);
#endregion

#region DbParameter
		abstract internal void SetDirection(DbParameter p, ParameterDirection direction);
		abstract internal void SetValue(DbParameter p, object value);
		abstract internal object GetValue(DbParameter p);
		abstract internal bool SetCursor(DbParameter p, object value);
		abstract internal bool IsCursor(DbParameter p);
		abstract internal bool SetAnonymousParameter(DbParameter p);
		abstract internal bool IgnoresOutputTypes(DbParameter p);
#endregion
	}
}