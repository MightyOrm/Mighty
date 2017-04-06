using System;
using System.Data;
using System.Data.Common;

namespace Mighty.DatabasePlugins
{
	abstract public class DatabasePlugin
	{
		public MightyORM mightyInstance { get; internal set; }

		abstract public string GetProviderFactoryClassName(string providerName);

		abstract public string PrefixParameterName(string rawName, DbCommand cmd = null);
		abstract public string DeprefixParameterName(string dbParamName, DbCommand cmd);

#region DbCommand
		abstract public DbDataReader ExecuteDereferencingReader(DbCommand cmd, DbConnection conn);
		abstract public bool RequiresWrappingTransaction(DbCommand cmd);
#endregion

#region DbParameter
		abstract public void SetDirection(DbParameter p, ParameterDirection direction);
		abstract public void SetValue(DbParameter p, object value);
		abstract public object GetValue(DbParameter p);
		abstract public bool SetCursor(DbParameter p, object value);
		abstract public bool IsCursor(DbParameter p);
		abstract public bool SetAnonymousParameter(DbParameter p);
		abstract public bool IgnoresOutputTypes(DbParameter p);
#endregion
	}
}