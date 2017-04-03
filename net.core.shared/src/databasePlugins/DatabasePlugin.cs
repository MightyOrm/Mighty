using System;

namespace Mighty
{
	internal abstract class DatabasePlugin
	{
		internal abstract string GetProviderFactoryClassName(string providerName);
	}
}