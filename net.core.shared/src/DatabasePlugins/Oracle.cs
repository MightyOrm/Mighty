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

		// SELECT t.*
		// FROM
		// (
		// 	   SELECT ROW_NUMBER() OVER (ORDER BY e.Salary DESC, e.Employee_ID) "ROW_NUMBER", e.*
		// 	   FROM employees e
		// 	   WHERE e.last_name LIKE '%i%'
		// ) t
		// WHERE "ROW_NUMBER" BETWEEN 10 AND 19
		// ORDER BY "ROW_NUMBER";
	}
}