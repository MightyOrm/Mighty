---
title: Paging
layout: default
nav_order: 11
---

# Paging
{: .no_toc }

- TOC
{:toc}

## Simple Paging

Paging queries [are complicated](https://stackoverflow.com/q/241622)... particularly on `ROW_NUMBER()` based databases (SQL Server and Oracle); less so, on `LIMIT`-`OFFSET` based databases (SQLite, MySQL, PostgreSQL).

Fortunately, Mighty writes paging queries for you! 😊

```c#
// dynamic version
var films = new MightyOrm(connectionString, "Film");
var page = films.Paged(orderBy: "Title", currentPage: 2, pageSize: 30);
```

```c#
// generic version
var films = new MightyOrm<Film>(connectionString);
var page = films.Paged(orderBy: "Title", currentPage: 3, pageSize: 10);
```

That's it!

You don't even necessarily need the `orderBy`, paging will sort by primary key by default. The default page to fetch is page 1 (note that `currentPage` is 1-based, not 0-based), and the default for `pageSize` is 20.

## Paging from an Arbitrary Select

If you need to, still without having to write the full paging query, you can specify what you want to select, and where from, and get Mighty to stick it together and page over it for you. This works even over an arbitrary select based on joins, if you need it:

```c#
var db = new MightyOrm(connectionString);
var page = db.PagedFromSelect(
	"Employee e INNER JOIN Department d ON e.DepartmentID = d.DepartmentID", // could have just been a table name!
	"e.EmployeeID", // order by
	"e.EmployeeID, e.GivenName, e.FamilyName, d.Name AS DepartmentName", // columns
	"e.FamilyName = @0", // OPTIONAL WHERE spec
	currentPage: 3, pageSize: 20, // page specs (defaults are 1 and 20)
	args: "Smith"); // OPTIONAL args for WHERE spec
```