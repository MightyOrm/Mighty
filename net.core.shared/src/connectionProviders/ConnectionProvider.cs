using System;

namespace Mighty
{
	public abstract class ConnectionProvider
	{
		public DbProviderFactory ProviderFactory { get; protected set; }
		public SupportedDatabase SupportedDatabase { get; protected set; }
		public string connectionString { get; protected set; }
		// fluent API, must return itself at the end
		abstract public ConnectionProvider Init(string connectionString);
	}
}