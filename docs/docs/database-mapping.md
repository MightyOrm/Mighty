---
title: Database Mapping
layout: default
nav_order: 9
---

# Database Mapping
{: .no_toc }

- TOC
{:toc}

Like Massive, Mighty sypports [manual mapping](#manual-mapping). This is useful for knocking up quick, read-only column name mapping, and other data transforms if you need them (e.g. `LTRIM(RTRIM(Name))`, to clean up legacy data).

But Mighty now supports full [convention based mapping](#convention-based-mapping) (i.e. class to database name mapping functions) and C# [attribute based mapping](#attribute-based-mapping) as well.

---

## Manual mapping

For read-only purposes (and also for knocking up quick SQL dta transforms) you can just map your column names to field names using the `columns` parameter (in the constructor, or in the data access method):

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

    // class-column mapping in Mighty is not case sensitive by default
    // (unless you set it to case-sensitive, you don't actually need this mappping)
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

## Auto-mapping in Mighty

Once you apply any column name mapping, Mighty switches on field name mapping by default. The rules are as follows:

|----|----|----|----|
|`MightyOrm` contructor or method parameter|Default (no columns renamed)|Some columns renamed, but auto-mapping manually disabled|Default auto-mapping, once some columns renamed|
|----|----|----|----|
|`primaryKeys`|List of primary key names|Column name(s) only, e.g. `"film_id"`|C# field/property name(s) only (e.g. `"FilmID"`)|
|`columns`|Any valid SQL column specification|Any valid SQL column specification (e.g. `"film_id AS FilmID, LTRIM(RTRIM(description)) AS Description"`)|C# field/property names only (e.g. `"FilmID, Description"`)|
|`orderBy`|Any valid SQL `ORDER BY` specification|Any valid SQL `ORDER BY` specification (e.g. `"LEN(description)"`)|C# field/property names only, but with ASC and DESC supported (e.g. `"Description DESC"`)|

You can provide a set of flags which will turn auto-mapping off (for some, none or all of the above items) by passing an `autoMap` function to `SqlNamingMapper`, or by setting the `autoMap` parameter on the `DatabaseTable` attribute.

> Just like Massive, Mighty assembles SQL fragments which you pass in (e.g. `where`, `columns`, `orderBy`). Also just like Massive, [database parameters](parameters) *are never directly interpolated into SQL* and instead are always passed to the underlying database as true `DbParameter` values. This is essential to help avoid SQL injection attacks.


### Maintainable auto-mapping:
{: .no_toc }

With auto-mapping (or as the default, with no column renaming), instead of using raw strings as your field/column names which are passed in to Mighty you can (completely optionally) use the C# `nameof` function instead. This is more future-proof against field renames, and in general just allows your IDE to track references to the field name for you:

```c#
var films = films.All(orderBy: nameof(Film.FilmID));
```

To get the same effect if you have column renames but have manually disabled auto-mapping, you additionally have to map the column name. You can do this as follows:

```c#
var films = films.All(orderBy: films.DataContract.Map(nameof(Film.FilmID));
```