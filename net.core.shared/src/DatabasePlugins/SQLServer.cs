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

		// works with QUOTED_IDENTIFIER OFF OR ON;
		// t outer table name does not conflict with any use of t table name in inner SELECT
		// we need to call the column ROW_NUMBER() and then remove it from any results, if we are going to be consistent across DBs;
		// note that first number is offset, and second number is offset + limit + 1
		// SELECT t.*
		// FROM
		// (
		// 	   SELECT ROW_NUMBER() OVER (ORDER BY t.Color DESC, t.ProductNumber) AS [ROW_NUMBER()], t.*
		// 	   FROM Production.Product AS t
		// 	   WHERE t.ProductNumber LIKE '%R%'
		// ) t
		// WHERE [ROW_NUMBER()] > 10 AND [ROW_NUMBER()] < 21
		// ORDER BY [ROW_NUMBER()]
	}
}