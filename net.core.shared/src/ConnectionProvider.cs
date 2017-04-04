using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

using Mighty.DatabasePlugins;

namespace Mighty
{
	abstract public class ConnectionProvider
	{
		public DbProviderFactory ProviderFactory { get; protected set; }
		public SupportedDatabase SupportedDatabase { get; protected set; }
		public string ConnectionString { get; protected set; }

		// fluent API, must return itself at the end; should set all three public properties (may ignore connectionStringOrName input here if you wish,
		// in which case you would pass null as the connectionStringOrName value to the MightyORM constructor)
		abstract public ConnectionProvider Init(string connectionStringOrName);
	}
}