using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Mighty.DatabasePlugins
{
	internal class Oracle : DatabasePlugin
	{
		internal override string GetProviderFactoryClassName(string loweredProviderName)
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
	}
}