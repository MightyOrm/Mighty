# ![logo](https://mightyorm.github.io/Mighty/assets/realfavicon/favicon-32x32.png) Mighty

[![NuGet](https://img.shields.io/nuget/v/Mighty.svg)](https://nuget.org/packages/Mighty)

Available for:

 - .NET Core 1.0+ (including 3.0 preview)
 - .NET Framework 4.5+
 - .NET Framework 4.0+ (without async support)

---

Mighty features classic Massive-style sweetness.

It really is simple and sweet, with *no* other setup these lines just do what they say:

```c#
var db = new MightyOrm(connectionString, "Film", "FilmID");

var film = db.Single(47);
film.Description = "This is a better description";
film.Save();

var films = db.All(new { Director = "Spielberg" });
foreach (var film in films) Console.WriteLine(film.Title);
```

But now extended - hopefully just as sweetly! - in several new directions.

# Feature Overview

More complete documentation on all these features is [available here](https://mightyorm.github.io/Mighty/).

## Named and directional parameters

```c#
var result = db.ExecuteProcedure("my_add_proc",
    inParams: new { a = 1, b = 2}, outParams: new { c = (int?)null });
Console.WriteLine(result.c); // 3
```

Also with support for named input-output and return parameter types; and these can even be mixed with classic Massive-style auto-numbered parameters (`@0`, `@1`, etc. where you provide only the values).

## Optional strong typing

Mighty now supports generic types:

```c#
var db = new MightyOrm<Film>(connectionString, primaryKeys: "FilmID");
var films = db.All();
foreach (Film film in films)
{
    Console.WriteLine($"{film.Title}: {film.Description}");
}
```

## Table and column name mapping

Recently added, support for function based and attribute based field and class to column and table name mapping. Here's a quick attribute based example:

```c#
[DatabaseTable("films")]
public class Film
{
    [DatabaseColumn("film_id")]
    public int FilmID;

    [DatabaseColumn("film_title")]
    public int Title;
}
```


## Multiple result sets

Hot off the press!

As well as dynamic multiple results sets, Mighty now properly supports strongly typed multiple result sets:

```c#
var db = new MightyOrm(connectionString);
var now = DateTime.Now;
using (var multiple = db.ExecuteMultipleFromProcedure("procPurchaseReport",
    inParams: new { StartDate = now.AddMonths(6), EndDate = now })
{
    multiple.NextResultSet();
    foreach (PurchaseReportSummary summary in
        multiple.CurrentResultSet.ResultsAs<ReportSummary>())
    {
        Console.WriteLine($"Total Sales for Report Period: ${summary.Total}");
        break; // assume only one summary item
    }
    multiple.NextResultSet();
    foreach (PurchaseReportMonthly monthly in
        multiple.CurrentResultSet.ResultsAs<PurchaseReportMonthly>())
    {
        Console.WriteLine($"Total Sales for Month ${monthly.Month}: ${monthly.Total}");
    }
}
```

## Cursor parameter support

Unique to Mighty!

Cursor support (on Oracle and PostgreSQL - the only two supported databases which will pass cursors out to client code):


```c#
var results = db.ExecuteWithParams("begin open :p_rc for select * from emp where deptno = 10; end;",
    outParams: new { p_rc = new Cursor() },
    // shared connection (Oracle) or transaction (PostgreSQL) required to share cursors
    connection: conn);
db.ExecuteAsProcedure("cursor_in_out.process_cursor",
    inParams: new { p_cursor = results.p_rc },
    connection: conn);
```

### Npgsql

We also support automatic cursor dereferencing on Npgsql, something Npgsql no longer directly supports - but potentially very useful since it makes it easy to emulate multiple resultsets from stored procedures in PostgreSQL. This isn't the most efficient way to pass result sets on PostrgeSQL. But it is the *only* way to pass multiple result sets, and it is simple to set up and easy to code against!

## Transactions

All database access methods on Mighty accept a `DbConnection`.  Unlike Dapper you don't *have* to do this, and in fact, you can spend most of your life in Mighty avoiding directly using `System.Data.Common` at all. But when you do need transactions, now you can:

```c#
var db = new MightyOrm(connectionString);
using (var connection = db.OpenConnection())
{
    using (var trans = conn.BeginTransaction())
    {
        var customer = db.Insert(CustomerInfo, connection);
        OrderInfo.CustomerID = customer.CustomerID;
        db.Insert(OrderInfo, connection);
        trans.Commit();
    }
}
```

## Paging support

Writing paging SQL ought to be simple, right? On `LIMIT`-`OFFSET` databases it is. On `ROW_NUMBER()` databases (SQL Server and Oracle), it's really not. Fortunately, Mighty writes it for you!

```c#
var films = new MightyOrm(connectionString, "Film");
var page = films.Paged(orderBy: "Title", currentPage: 2, pageSize: 30);
```

## Supported databases

 - SQL Server
 - MySQL
 - PostgreSQL
 - Oracle
 - SQLite

## Relation to Massive

Everything that happened at the origin of this project was [originally](https://github.com/FransBouma/Massive/issues/265) [offered](https://github.com/FransBouma/Massive/pull/281) [to](https://github.com/FransBouma/Massive/issues/284) [Massive](https://github.com/FransBouma/Massive/issues/293). In fact, I created the [MySQL support in Massive](https://github.com/FransBouma/Massive/graphs/contributors) while I was working on this project. I was new to Open Source then (though certainly not to programming!), and I was too pushy. On the other hand it also became clear that the Massive project was - and is - happy where it was... [and is](https://github.com/FransBouma/Massive/graphs/code-frequency).

Because of this, I felt something else was wanted, and needed. I wanted this because it seemed clear to me that Massive was the sweetest micro-ORM. But I wanted to add my own named and directional parameter support. And then I Needed .NET Core support. And I wanted to prove that my approach to parameter support could deal even with the difficult edge cases which Frans Bouma mentioned - which is the origin of the very thorough support, including cursor support, in Mighty! ;)

That's it. I genuinely believe this is a top-tier micro-ORM. The design, the interface, owes everything, originally, to Rob Conery. And the extensions to Massive which are now Mighty, genuinely owe a lot to the feedback which Frans Bouma gave me as to what I should and shouldn't be doing if I wanted to work on something like Massive.

It's a re-write, not a fork. Nevertheless, the architectural decisions of the core engine are fundamentally based on those of Massive. But a lot has been added on top. With a great effort to maintain compatibility with Massive, and to retain its sweet combination of power and simplicity.

I haven't been able to resist giving this a 3.x version number. But of course it's not a version 3 of Massive - even though it now has the features I'd like to have seen in one. Massive didn't want to go in these directions; and why should it? It's work in the micro-ORM sphere was done, long ago! But I really wanted *Massive*'s interface, with these extra features, because I really like Massive. So I finished this! It's now 100% a separate project. Though of course with the intention of maintaining and supporting the Massive-style interface which I still so like!

I intend to support it, and I hope some other people find it as useful - and pleasant to use - for .NET database programming as I do.
