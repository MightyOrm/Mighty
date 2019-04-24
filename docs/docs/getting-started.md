---
title: Getting Started
layout: default
nav_order: 1
---

# Getting Started
{: .no_toc }

- TOC
{:toc}

---

## Installing Mighty

Just add the current version of Mighty to your project using [NuGet](https://www.nuget.org/packages/Mighty) and make sure that you have installed the [ADO.NET data drivers]() for the database(s) which you need to access.

## Reading Data

It's very easy to get going with Mighty and you don't need to create any .NET database objects at all:

```c#
MightyOrm people = new MightyOrm(connectionString, "People", "PersonID");
dynamic person = people.Single(42);
Console.WriteLine($"{person.GivenName} {person.FamilyName}");
```

That just works, with no other set up required! The fields in the dynamic `person` object have their values and types driven by whatever the database returns.

Mighty now also supports [strongly typed](strongly-typed-mighty) data access:

```c#
public class Person
{
    public string GivenName { get; set; }
    public string FamilyName { get; set; }
    public DateTime dateOfBirth { get; set; }
}

MightyOrm people = new MightyOrm<Person>(connectionString, "People", "PersonID");
Person person = people.Single(42);
Console.WriteLine($"{person.GivenName} {person.FamilyName}");
```

> Dynamically typed code is great for quick prototyping, but generically typed code is more maintainable. In Mighty it's very easy to get something working with dynamic types and then tighten up your code by switching to generics.

All instances of `MightyOrm` support non-table-specific access, meaning you do not have to specify a table name to use Mighty:

```c#
MightyOrm db = new MightyOrm(connectionString);
dynamic person = db.Single("SELECT * FROM People WHERE PersonID = 42");
Console.WriteLine($"{person.GivenName}");
```

On .NET Framework you do not even have to specify a connection string - Mighty will use the first one in your config file by default:

```c#
MightyOrm db = new MightyOrm();
```

## Reading Multiple Rows

All the above examples return single items. Here is the syntax for reading multiple rows with Mighty. In the first example we'll select which people to display using the convenient `whereParams` syntax:

```c#
MightyOrm people = new MightyOrm(connectionString, "People", "PersonID");
IEnumerable<dynamic> smiths = people.All(new { FamilyName: "Smith" });
foreach (var person in smiths)
{
    Console.WriteLine($"{person.GivenName});
}
```

In this second example, we're fetching the people to display using a more general purpose WHERE clause:


```c#
MightyOrm people = new MightyOrm(connectionString, "People", "PersonID");
IEnumerable<dynamic> myPeople = people.All(
    "DateOfBirth < @0 AND FamilyName = @1", new DateTime(2000, 1, 1), "Smith");
foreach (var person in myPeople)
{
    Console.WriteLine($"{person.GivenName} {person.FamilyName} {person.DateOfBirth}");
}
```

> The parameter prefix to use (`@` in `@0` and `@1` above) [depends on which database you are using](supported-databases).

## Mighty Method Names

The methods in Mighty use a naming convention that will hopefully let you see what they do without having to read an instruction manual, but here is a list:

 | Method name | Purpose |
 |-|-|
 | Query | Reads rows from a general-purpose query |
 |-|-|
 | QueryMultiple | Read multiple resulsets |
 |-|-|
 | All | Reads the specified rows from the current table |
 |-|-|
 | Single | Reads a single item from the current table |
 |-|-|
 | Execute | Execute a query where no rows of results are expected |
 |-|-|
 | ExecuteProcedure | Execute a stored procedure or function |
 |-|-|
 | Paged | Return paged-results from a table, view or join |

All of these have many slight variations taking different argument patterns, as well as related commands for when you need combinations of what these do, such as `QueryFromProcedure`, `SingleFromProcedure`, `SingleFromQuery`, etc.

Mighty also has [CRUD methods](crud-actions) (`Insert`, `Update`, `Save`, `Delete`), aggregate methods (`Count`, `Sum`, `Max`, `Min`, `Avg`), and various other utility methods.

There are also a couple of standard method name suffixes:

 | Suffix | Example | Purpose |
 |-|-|
 | ...WithParams | QueryWithParams | Provides versions of the above methods with full named input, output, input-output and return parameter support (for those which do not have it by default) |
 |-|-|
 | ...Async | AllAsync | The asynchronous versions of the methods (this is standard naming in .NET) |
  