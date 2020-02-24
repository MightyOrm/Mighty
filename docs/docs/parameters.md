---
title: Parameters
layout: default
nav_order: 10
---

# Parameters
{: .no_toc }

- TOC
{:toc}

## Auto-Numbered Parameters

Like Massive, Mighty supports passing simple auto-numbered parameters using the convenient C# `params` syntax:

```c#
var db = new MightyOrm(connectionString);
var salesSmiths = db.Query(
    "SELECT * FROM Employees e WHERE e.FamilyName = @0 AND e.DepartmentID = @1",
    "Smith",
    SalesID);
```

You could have added as many arguments as you need at the end of this, simply separating them by commas.

If you have used C# named parameters before you get to the optional `args` then you can't use the params syntax any more to pass multiple arguments as if they were true arguments to the method, but you can still use:

```c#
var employees = new MightyOrm(connectionString, "Employees");
var salesSmiths = employees.All(
    where: "e.FamilyName = @0 AND e.DepartmentID = @1",
    args: new object[] { "Smith", SalesID });
```

Though it is a feature of C# not of Mighty, it is worth noting that when passing a single argument after named params this can be simplified to:

```c#
var employees = new MightyOrm(connectionString, "Employees");
var smiths = employees.All(
    where: "e.FamilyName = @0",
    args: "Smith");
```

> Whether to use `@0`, `@1`, etc. or `:0`, `:1`, etc. for auto-numbered parameters [depends on the database](supported-databases): use `:0`, etc. for Oracle and PostgreSQL, and `@0`, etc. for everything else.

## Named and Directional Parameters

Mighty adds full support for named and directional parameters as well. This includes input, output, input-output and return parameters.

For example:

```c#
var db = new MightyOrm(connectionString);
var result = db.ExecuteProcedure(
    "rewards_report_for_date",
    inParams: new {
        min_monthly_purchases = 3,
        min_dollar_amount_purchased = 20,
        report_date = new DateTime(2015, 5, 1)
    },
    outParams: new {
        count_rewardees = 0
    });
Console.WriteLine(result.count_rewardees);
```

This is especially useful for stored procedures, but can also be used with arbitrary SQL:

```c#
var db = new MightyOrm(connectionString);
dynamic result = db.ExecuteWithParams(
    "set @a = @b + 1",
    inParams: new { b = 1233 },
    outParams: new { a = 0 });
Assert.AreEqual(1234, result.a);
```

> You need to specify a dummy value for output and return parameters to set their database type.

## Using Parameters with Queries

Input, output and return parameter objects can't be updated, because it is
[not officially supported](https://stackoverflow.com/questions/17441420/how-to-set-value-for-property-of-an-anonymous-object)
to update members of an anonymous object in C#, even though anonymous objects are the nicest syntax for providing parameter objects.

So, as you can see in the examples above, for any parameters which may have an ouput value (output, io and return parameters), these are copied with their values into a new dynamic object with is returned
when executing a command with parameters.

If you use a `Query...` variant in Mighty, you get the query results but not the param results. If you use an `Execute...` varaint you get the param results, but not the query results.

It's a bit fiddly (it has to be, both for the reason above and for another reason described just below), but you can also get both. Here is an example of how to do it:

``` c#
List<ImportantItem> GetItemsBelongingToParent(int parentId)
{
    List<ImportantItem> items;
    dynamic results;

    var db = new MightyOrm<ImportantItem>(_connectionString);

    // this is the way to get query results AND output param results in Mighty
    using (var cmd = db.CreateCommandWithParams(
        "GetImportantItems",
        inParams: new { ParentID = parentId },
        returnParams: new { RETVAL = (int?)null },
        isProcedure: true
    ))
    {
        // because Mighty/Massive always uses delayed execution (query never executed until
        // we enumerate the enumerable), we need to use .ToList() to force execution...
        items = db.Query(cmd).ToList();

        // ... _before_ reading back the param results
        // (this does not re-execute the command, it just gathers up the results)
        results = db.ResultsAsExpando(cmd);
    }

    if (results.RETVAL != 0)
    {
        throw new InvalidOperationException($"ParentID={parentID} not found");
    }

    return items;
}
```

As you can see in the example above, there's another - even more fundamental - issue here. We can't combine the output, io and return parameters into the return value from a Mighty `Query...` method even if we wanted to;
because *the query hasn't happened by the time we return*! This is a [fundamental decision in Mighty](ienumerables-in-mighty) (adopted, happily, from Massive), so for this reason getting the param results from a query has
to be a separate call which you, the programmer, do once you know that you have consumed (or started to consume) the query results.

## SQL Injection

> Database parameters *are never directly interpolated into SQL* in Mighty and instead are always passed to the underlying database as true `DbParameter` values. This is essential to help avoid SQL injection attacks. Always pass user-supplied values via parameters!
