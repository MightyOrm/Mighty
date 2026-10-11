# Connection handling regression tests

Database-free NUnit tests, included automatically in `MightyTests.csproj`. Run just this fixture with:

```powershell
dotnet test tests/MightyTests.csproj -f netcoreapp3.1 --filter "TestCategory=ConnectionHandling"
```

This test-first branch starts at v4 tip `c5043924c3ec24b37ed6f2fec4b5192af497eba1`. Tests specify the intended corrected alpha-4 behaviour: honour attached connections, detach disposed local connections from reusable commands, propagate connections through queries, and reject conflicting connections. Some tests intentionally fail until that runtime work is integrated; none are ignored or marked as expected failures.

The fake provider checks ownership, transaction preservation, command reuse, query propagation, partial enumeration, failures, pending async execution and token cancellation. Its state is per case, and profiling is explicit. The `Mighty.ZConnectionHandling` namespace places the fixture after the existing cache-total checks, following the suite's current name-order convention. Async cases are excluded on `net40`.

These tests exercise Mighty through its public methods. They do not establish real database commit/rollback, SQL Server transaction enlistment, PostgreSQL cursor correctness or the cause of the internal API regression.

Validation: all seven existing test targets compile. On both .NET Core 3.1 and .NET Framework 4.5, 123 of 200 cases pass and 77 fail against the unchanged v4 tip. All 200 pass against an isolated copy of that tip with the corrected alpha-4 connection handling applied. The full database suite has not been run.
