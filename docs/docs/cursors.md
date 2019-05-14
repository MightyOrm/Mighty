---
title: Cursors
layout: default
nav_order: 12
---

# Cursors
{: .no_toc }

- TOC
{:toc}

## Using Cursors in Mighty

Of the five databases which Mighty [currently supports](supported-databases) four of these (all except SQLite) have cursors, but only two of those (Oracle and PostgreSQL) support passing cursors out to client code. On those two databases, Mighty fully supports working with cursors.

Cursors are created and referenced using the Mighty `Cursor` class, as in the examples below.

> When looking at these examples, bear in mind that you no NOT normally need to manually open and pass connections when using Mighty. It is supported for advanced use-cases where you have to share a connection (such as when using cursors on Oracle) or a transaction (such as when using cursors on PostgreSQL).

### Oracle

Passing a cursor from one function to another:

```c#
// To share cursors between commands in Oracle the commands must use the same connection
var db = new MightyOrm(connectionString);
using (var conn = db.OpenConnection())
{
    var result1 = db.ExecuteProcedure("get_cursor",
        outParams: new { o_cursor = new Cursor() },
        connection: conn);
    var result2 = db.ExecuteProcedure("process_cursor",
        inParams: new { p_cursor = result.o_cursor }, outParams: new { p_count = 0 },
        connection: conn);
    Assert.AreEqual(100, result2.p_count);
}
```

### PostgreSQL

Passing a cursor from one function to another:

```c#
// To share cursors between commands in PostgreSQL the commands must use the same transaction
var db = new MightyOrm(connectionString);
using (var conn = db.OpenConnection())
{
    using (var trans = conn.BeginTransaction())
    {
        var cursors = db.ExecuteProcedure("get_cursor",
            outParams: new { o_cursor = new Cursor() },
            connection: conn);
        
        var results = db.QueryFromProcedure("fetch_next_row_from_cursor",
            new { cursor_param = cursors.o_cursor },
            connection: conn);
            
        int count = 0;
        foreach (var item in results)
        {
            Console.WriteLine($"{item.id} {item.name}")
            count++;
        }
        Assert.AreEqual(1, count1);

        trans.Commit();
    }
}
```

### PostgreSQL - Manual Cursor Dereferencing

Manually dereferencing a cursor. (This example shows you *how* to do this but YOU DO NOT NEED TO, Mighty will do this for you - see [next section](#automatic-cursor-dereferencing))! 😊)


```c#
var db = new MightyOrm(connectionString);
using (var conn = db.OpenConnection())
{
    using (var trans = conn.BeginTransaction())
    {
        int fetchSize = 1000;
        var result = db.ExecuteProcedure("get_cursor", returnParams: new { o_cursor = new Cursor() }, connection: conn);
        while (true)
        {
            var fetchResults = db.Query($"FETCH {FetchSize} FROM \"{result.cursor.CursorRef}\", connection: conn);
            int subcount = 0;
            foreach (var item in fetchResults)
            {
                Console.WriteLine($"{item.id} {item.name}");
                subcount++;
            }
            if (subcount == 0)
            {
                break;
            }
        }
        db.Execute($"CLOSE \{result.cursor.CursorRef}\", connection: conn);
        trans.Commit();
    }
}
```

## Automatic Cursor Dereferencing

### Oracle

The default behaviour of the Oracle ADO.NET driver(s) is to automatically dereference cursors. That is, if you run a query which actually, at the database level, returns one or more rows with one or more cursors in them, then you don't get back the cursors as the result of your query, you get back multiple result sets, each one containing the result of dereferencing (returning all the rows from) a cursor from the original results.

Here's an Oracle example - the procedure actually returns a cursor (and you need to specify that it does), but the Oracle ADO.NET driver automatically dereferences the result:

```c#
var db = new MightyOrm(connectionString);
var employees = db.QueryFromProcedure("get_dept_emps",
    inParams: new { p_DeptNo = 10 }, returnParams: new { v_rc = new Cursor() });
foreach (var employee in employees)
{
    Console.WriteLine($"{employee.EMPNO} {employee.ENAME}");
}
```

- Cursor valued functions and cursor output parameters are both dereferenced by default
- If you call `ExecuteProcedure` instead of `QueryFromProcedure` then you would got back the actual cursor results, with no dereferencing
- This is all the default behaviour of the Oracle ADO.NET drivers

### PostgreSQL

The default behaviour of the v2 Npgsql driver for PostrgeSQL was to deference cursors, but this was removed in v3 (partly because the implementation was incomplete; partly to avoid encouraging people to write code which dereferences cursors on PostgreSQL, since returning tables from functions and querying these is actually more efficient - considerably so for large result sets).

Whilst it is not advised for genuinely large datasets, having functions which return cursors and a driver layer which automatically dereferences them is often convenient, and it is also the only way to replicate on PostgreSQL the feature of returning multiple resultsets from a stored procudure which is available on other databases such as SQL Server.

Mighty has included (and completed) the dereferencing code from Npgsql v2 - so if you use Mighty as your data access layer then cursors will once again be dereferenced for you, even on Npgsql. 😊

Basically the same code as in Oracle just works:

```c
var db = new MightyOrm(connectionString);
var employees = db.QueryFromProcedure("cursor_employees", outParams: new { refcursor = new Cursor() });
foreach (var employee in employees)
{
    Console.WriteLine($"{employee.firstname} {employee.lastname}");
}
```

- If you need to, you can turn off Npgsql automatic cursor dereferencing by setting `NpgsqlAutoDereferenceCursors` to false
- But note that (as in Oracle) using `Execute` instead of `Query` will let you get at the raw (non-dereferenced) cursors even when dereferencing is on

> You do not need to manually create a transaction for Npgsql automatic cursor dereferencing, Mighty does this for you, if necessary, and will use the existing transaction if one is already present (on the current `DbConnection` if you passed one in, or on the current `TransactionScope` in .NET Framework).