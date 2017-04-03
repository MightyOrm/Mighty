using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Mighty
{
	internal abstract class DatabasePlugin
	{
		internal abstract string GetProviderFactoryClassName(string providerName);
	}
}