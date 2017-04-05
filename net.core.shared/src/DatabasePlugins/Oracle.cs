namespace Mighty.DatabasePlugins
{
	internal class Oracle //: DatabasePlugin
	{
		/*override*/ static internal string GetProviderFactoryClassName(string loweredProviderName)
		{
			switch (loweredProviderName)
			{
				case "oracle.manageddataaccess.client":
					return "Oracle.ManagedDataAccess.Client.OracleClientFactory";

				case "oracle.dataaccess.client":
					return "Oracle.DataAccess.Client.OracleClientFactory";

				default:
					return null;
			}
		}

		// t outer table name does not conflict with any use of t table name in inner SELECT
		// SELECT t.*
		// FROM
		// (
		// 	   SELECT ROW_NUMBER() OVER (ORDER BY t.Salary DESC, t.Employee_ID) "ROW_NUMBER", t.*
		// 	   FROM employees t
		// 	   WHERE t.last_name LIKE '%i%'
		// ) t
		// WHERE "ROW_NUMBER" BETWEEN 10 AND 19
		// ORDER BY "ROW_NUMBER";
	}
}