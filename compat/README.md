## Mighty Binary Compatibility Tests

This is a harness for testing binary compatibility between different versions of Mighty.

We can use this to test whether the test suite from an older version of the Mighty can run against a proposed new NuGet
package of another, newer version.

If it works, this shows that we haven't broken binary compatibility, at least for everything which the test suite tests.

> Binary compatibility is a tricky thing: it is not a strict subset nor a strict superset of recompilation compatibility; also, especially because of reflection, every
code change is binary incompatible against *some* code which would previously have worked (done what the programmer wanted, which may or
may not be what the library was 'intended' to support).
So assuming you have a reasonably comprehensive test suite, then testing whether the test suite works between different versions is a pretty
reasonable binary compatibility test.

### Changes to MightyTests.csproj

The MightyTests project now produces a tests package in its Release version; it still runs the tests directly in Visual Studio
in the Debug version.

### Why two projects?

This solution is split into two projects, for .NET Core and Framework, because I was getting a 'Detected package downgrade' error
when trying to build executable files of both types in one project, and this goes away when building just one or the other.
It seems as if this must be a (VS?) bug, since you can certainly build DLLs of both types in one project.

### Showing progress

When running these tests, add the command line argument `--labels=All` to make the NUnit test runner show more of what is going on.
Command line args are just passed on by our code, it is the NUnit code which is responding to this argument.

### Other notes

 - When switching to stand-alone tests, it seems that you NEED to remove the Test.SDK package (&/or possibly
   the TestAdapater package) before the tests will pack

 - If it just hangs, it's possible that the PostgreSQL DB is just not running
   - Potentially `pg_ctl start` will start it, if the PG database path env var is configured (PGDATA=C:\Program Files\PostgreSQL\data\pg96)
   - More fundamentally, it's possible that what has gone wrong is permissions for the PostgreSQL Server service, after a Windows update,
     in which case
	 ```
		pg_ctl unregister -N "PostgreSQL 9.6 Server"
		pg_ctl register -N "PostgreSQL 9.6 Server" -D "C:\Program Files\PostgreSQL\data\pg96"
	 ```
	 should fix it (-D not required if PGDATA env var is already set)
	 
### SQLite

 - Loading the wrong version of System.Data.SQLite:
	- It's simply when we have an App.config from an older (or newer!) version of the tests; after which, clear the bin and obj before retrying
 - Even after doing that, cannot load SQLite with the right version number (says something about wrong format):
	- The processor architecture set for the test runner project has to match the processor architecture set for the pre-built tests
 - We need a reference to System.Data.SQLite in the .NET Framework project file for MTest, or else interop (processor specific) packages which
   SQLite depends on are not copied into the build directory
 
 - 'Unexpected data section' when loading in SQLLite happens when using the SQLLite data section from .NET Framework in the config
   file for .NET Core

### Other Notes

Since we're building the tests as a library in the Release version, perhaps we should *really* be building the same targets as Mighty builds
(i.e. .NET Standard, not .NET Core) in that case, and only be targeting .NET Core in this final test runner project; one reason not to is just
that MightyTests.csproj would need updating to support .netstandard targets as well as .netcoreapp targets.
But since the original target of the tests was .exe's, and still is in Debug mode, perhaps it doesn't really matter (i.e. perhaps the net
result is guaranteed to be the same - though I'm not quite sure!).
