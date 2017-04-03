using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Mighty
{
	public abstract class ConnectionProvider
	{
		public DbProviderFactory ProviderFactory { get; protected set; }
		public SupportedDatabase SupportedDatabase { get; protected set; }
		public string ConnectionString { get; protected set; }
		// fluent API, must return itself at the end
		abstract public ConnectionProvider Init(string connectionString);
	}
}