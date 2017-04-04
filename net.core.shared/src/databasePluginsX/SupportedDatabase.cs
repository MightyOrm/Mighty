using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Mighty.DatabasePlugins
{
	// these need to match the names of the plugin classes
	// for future link compatibility, don't re-order this list
	public enum SupportedDatabase
	{
		MySQL,
		Oracle,
		PostgreSQL,
		SQLite,
		SQLServer
	}
}