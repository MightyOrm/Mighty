---
title: Multiple Resultsets
layout: default
nav_order: 7
---

# Multiple Resultsets

The pattern for reading multiple resultsets in Mighty is as follows:

```c#
MightyOrm db = new MightyOrm(connectionString);
var twoSets = db.QueryMultiple("select 1 as a, 2 as b; select 3 as a, 4 as c;");
int sets = 0;
foreach (var set in twoSets)
{
    foreach (var item in set)
    {
        Console.Log(item.a);
        if (sets == 0) Console.Log(item.b);
        else Console.Log(item.c);
    }
    sets++;
}
```

Some comments:

 - The main use-case for multiple resultsets would probably for accessing multiple results from a stored procedure using `QueryMultipleFromProcedure`, but the above example makes clear what the results will be
 - On a strongly typed instance of Mighty the items from all resultsets have to be of the same type, so unless your data is like that it probably makes more sense to use a `dynamic` instance of Mighty in this case; you could instead make a contrived class to hold all possible return values
 - You don't have to use `foreach` to consume the result sets, or even the items within the set, you can always manually use `IEnumerable.GetEnumerator`, `IEnumerator.MoveNext` and `IEnumerator.Current`