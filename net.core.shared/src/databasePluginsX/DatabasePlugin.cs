using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

using Mighty;

namespace Mighty.DatabasePlugins
{
	abstract internal class DatabasePlugin
	{
		internal MightyORM _mightyInstance;

		abstract internal string GetProviderFactoryClassName(string providerName);
	}
}