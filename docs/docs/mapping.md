---
title: Table and Column Name Mapping
layout: default
nav_order: 9
---

# Table and Column Name Mapping
{: .no_toc }

- TOC
{:toc}

---

## Quick'n'dirty mapping

Like Massive, for read-only purposes you can just map your column names to field names using the constructor `columns` parameter:

```c#
var films = new MightyOrm(
    connectionString,
    tableName: "film",
    columns: "film_id AS FilmID, description AS Description");
var films = films.All();
foreach (var film in films)
{
    Console.WriteLine($"{film.FilmID}: {film.Description}");
}
```

This type of mapping will work with strongly-typed instances of `MightyOrm<T>` too.

## Convention based mapping

For more control and the ability to support writes as well as reads, you can use convention based mapping or [attribute based mapping](#attribute-based-mapping) or both.

Here's a quick convention based map for a strongly-typed instance of Mighty:

```c#

public class Film
{
    public int FilmID;
    public string Description;
}

...

var films = new MightyOrm<Film>(
    connectionString,
    mapper: new SqlNamingMapper(columnNameMapping: (t, n) => n
        .Map(nameof(Film.FilmID), "film_id")
        .Map(nameof(Film.Description), "description"));
```

You can control table names, primary keys and a lot more by providing different functions to Mighty's `SqlNamingMapper`.

As long as you provide a `columns` parameter, you can even do convention based mapping on a dynamic instance of Mighty 😊 :

```c#
var films = new MightyOrm(
    connectionString,
    tableName: "film",
    columns: "FilmID, Description",
    // `.Map` string extension for creating quick maps is defined in `Mighty.Mapping`, but is entirely optional
    mapper: new SqlNamingMapper(columnNameMapping: (t, n) => n
        .Map("FilmID", "film_id")
        .Map("Description", "description"));
var films = films.All();
foreach (var film in films)
{
    Console.WriteLine($"{film.FilmID}: {film.Description}");
}
```

## Attribute based mapping

Mighty also supports attribute based mapping:

```c#

[DatabaseTable("film")]
public class MyFilmClass
{
    [DatabaseColumn("film_id")]
    public int FilmID;

    // note that class-column mapping in Mighty is not case sensitive by default (i.e. you probably don't need this one)
    [DatabaseColumn("description")]
    public string Description;
}

...

var films = new MightyOrm<MyFilmClass>(connectionString);
var films = films.All();
foreach (var film in films)
{
    Console.WriteLine($"{film.FilmID}: {film.Description}");
}
```

In addition to mapping field or property names to column names and class names to table names, you can also tell Mighty to ignore columns with `[DatabaseIgnore]`
and you can specify primary key fields directly in the class definition using `[DatabasePrimaryKey]`.

> You can get Mighty to read and write non-public data members by applying `[DatabaseColumn]` (with or without any constructor parameters) to fields or properties which you control.

Even though most features of `SqlNamingMapper` can be done instead using attributes, and vice versa, there is no way to get Mighty to access non-public fields or properties purely using `SqlNamingMapper`. This is on purpose, to make it hard to intentionally or unintentionally make Mighty get or write object data which it shouldn't have access to.

## Auto-mapping

Once you apply any column name mapping, Mighty switches on field name mapping by default. The rules are as follows:

|----|----|----|
|Name of argument to `MightyOrm` contructor or method|With mapping|With no mapping, or mapping disabled (also default behaviour, when no column renaming has been done)|
|----|----|----|
|`primaryKeys`|Field and property names only, e.g. `"FilmID"`|Simple column names only, e.g. `"film_id"`|
|`columns`|Field and property names only, e.g. `"FilmID, Description"`|Column names or any other valid SQL column specification, e.g. `"film_id, LTRIM(RTRIM(description)) AS description"`|
|`orderBy`|Field and property names only, but with ASC and DESC support, e.g. `"Description DESC"`|Column names or any other valid SQL ORDER BY specification, e.g. `"LEN(description)"`|

As long as  you haven't done any field to column name re-mapping (even if you have used other features of `SqlNamingMapper` or attributes) then auto-mapping is *always* off and *all* of the above input formats (and SQL tricks!) are always available if you want to use them.

> As you can see, just like Massive, Mighty often assembles SQL fragments which you pass in (e.g. `where`, `columns`, `orderBy`); but, also just like Massive, within Mighty [database parameters](parameters) *are never directly interpolated into SQL* and instead are always passed to the underlying database as true `DbParameter` values. This is essential to help avoid SQL injection attacks.

You can provide a set of flags which will turn auto-mapping off (for some, none or all of the above items) by passing an `autoMap` function to `SqlNamingMapper`, or by setting the `autoMap` parameter of the `DatabaseTable` attribute.

### Examples:
{: .no_toc }

```c#
\\ With auto-mapping

public class Film
{
    [DatabaseColumn("film_id")]
    public int FilmID;

    [DatabaseColumn("description")]
    public string Description;
}

...

var films = new MightyOrm<Film>(connectionString);
var films = films.All(orderBy: "FilmID");

// Or, more maintainably:
var films = films.All(orderBy: nameof(Film.FilmID));
```

```c#
\\ Without auto-mapping

[DatabaseTable(autoMap: AutoMap.Off)]
public class Film
{
    [DatabaseColumn("film_id")]
    public int FilmID;

    [DatabaseColumn("description")]
    public string Description;
}

...

var films = new MightyOrm<Film>(connectionString);
var films = films.All(orderBy: "film_id");

// Or, more maintainably:
var films = films.All(orderBy: films.DataContract.Map(nameof(Film.FilmID));
```