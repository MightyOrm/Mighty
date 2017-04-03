using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Mighty.DatabasePlugins
{
	internal class Sqlite : DatabasePlugin
	{
		internal override string GetProviderFactoryClassName(string loweredProviderName)
		{
			switch (loweredProviderName)
			{
				case "system.data.sqlite":
					return "System.Data.SQLite.SQLiteFactory";

				case "microsoft.data.sqlite":
					return "Microsoft.Data.Sqlite.SqliteFactory";

				default:
					return null;
			}
		}
	}
}