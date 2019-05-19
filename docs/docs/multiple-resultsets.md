---
title: Multiple Resultsets
layout: default
nav_order: 7
---

# Multiple Resultsets

Hot off the press!

We now properly support strongly typed multiple result sets:

```c#
var db = new MightyOrm(connectionString);
var now = DateTime.Now;
using (var multiple = db.ExecuteMultipleFromProcedure("procPurchaseReport",
    inParams: new { StartDate = now.AddMonths(6), EndDate = now })
{
    multiple.NextResultSet();
    foreach (var summary in multiple.CurrentResultSet.ResultsAs<ReportSummary>())
    {
        Console.WriteLine($"Total Sales for Report Period: ${summary.Total}");
    }

    multiple.NextResultSet();
    foreach (var monthly in multiple.CurrentResultSet.ResultsAs<PurchaseReportMonthly>())
    {
        Console.WriteLine($"Total Sales for Month ${monthly.Month}: ${monthly.Total}");
    }
}
```

The above pattern would work perfectly well (without having to predefine *any* classes at all to hold the return data) by enumerating over `CurrentResultSet.Results()` instead of `ResultsAs<T>()` - or equivalently just over `CurrentResultSet` directly - to return dynamically typed items for each result set, with their fields driven by the returned data.

Below is the previous pattern for reading multiple resultsets in Mighty. This 'enumerable of enumerables' pattern still works in all the same places where the above pattern works, and might even be of some use for cheap and cheerful coding if you're using a dynamic instance of Mighty:

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
