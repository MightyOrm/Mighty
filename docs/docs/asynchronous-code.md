---
title: Asynchronous Code
layout: default
nav_order: 6
---

# Asynchronous Code

Mighty fully supports async.

In .NET Core 3.0+ and C# 8.0+ you can use `await foreach` to consume asynchronous multi-row results:

```c#
MightyOrm people = new MightyOrm(connectionString, "People", "PersonID");
IAsyncEnumerable<dynamic> myPeople = await people.AllAsync(
    "DateOfBirth < @0 AND FamilyName = @1", new DateTime(2000, 1, 1), "Smith");
await foreach (var person in myPeople)
{
    Console.WriteLine($"{person.GivenName} {person.FamilyName} {person.DateOfBirth}");
}
```

In earlier versions of C# `await async` does not exist, but we use the excellent [Dasync/AsyncEnumerable](https://github.com/Dasync/AsyncEnumerable) package, so you can get the needed effect with:

```c#
MightyOrm people = new MightyOrm(connectionString, "People", "PersonID");
IAsyncEnumerable<dynamic> myPeople = await people.AllAsync(
    "DateOfBirth < @0 AND FamilyName = @1", new DateTime(2000, 1, 1), "Smith");
await myPeople.ForEachAsync(person =>
{
    Console.WriteLine($"{person.GivenName} {person.FamilyName} {person.DateOfBirth}");
});
```
