namespace Mighty.DatabasePlugins
{
	internal class SQLServer //: DatabasePlugin
	{
		/*override*/ static internal string GetProviderFactoryClassName(string loweredProviderName)
		{
			switch (loweredProviderName)
			{
				case "system.data.sqlclient":
					return "System.Data.SqlClient.SqlClientFactory";

				default:
					return null;
			}
		}

		// SELECT t.*
		// FROM
		// (
		// 	   SELECT ROW_NUMBER() OVER (ORDER BY t.Color DESC, t.ProductNumber) AS [ROW_NUMBER], t.*
		// 	   FROM Production.Product AS t
		// 	   WHERE t.ProductNumber LIKE '%R%'
		// ) t
		// WHERE [ROW_NUMBER] BETWEEN 10 AND 19
		// ORDER BY [ROW_NUMBER]
	}
}