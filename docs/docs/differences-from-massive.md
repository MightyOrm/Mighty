---
title: Differences from Massive
layout: default
nav_order: 16
---

# Differences from Massive
{: .no_toc }

- TOC
{:toc}

## Porting Code from Massive to Mighty

> A lot of code written against Massive will just work with Mighty, despite all the new features. Just change `using Massive` to `using Mighty` and `DynamicModel` to `MightyOrm`.

Here is a list of those items which won't compile directly against former Massive code. In all cases, the same (sort of) thing *is* still supported, with some minor code changes on your part; and whilst this might look like quite a large list, many of these are actually 'edge cases' - they're not the core features, and you quite possibly aren't even using them.

## DataTable support

- DataTable is no longer directly supported (not even on .NET Framework). However if you need it, the (short) Massive source code for .ToDataTable() is [here](https://github.com/FransBouma/Massive/blob/583c0932cb5da17f06216777be74e16a421f2df4/src/Massive.Shared.cs#L140-L187); it is open source, under the same license as this project.

## Table meta-data

- `Schema` is now called `TableMetaData` (the word 'schema' was potentially misleading on those DB systems where it also means something like 'namespace')
- `DefaultValue(columnName)` is now called `GetColumnDefault(columnName)`
- `Prototype` is replaced by `New()`
- `CreateFrom(collection)` is replaced by `NewFrom(obj)` (where `obj` can be a `NameValueCollection` as before, but now can also be any other sensible way of specifying a name-value set)

## Validation

- `IsValid(item)` now returns a `List&lt;object&gt;` of errors (this is intentionally a `List`, not an `IList`). Each object is one error; the error objects are typically strings, but you get to decide this in your validation class. So `db.IsValid(item).Errors.Count == 0` checks for no errors (but `IsValid` can also returns the reported errors without storing them in a shared variable which might potentially be overwritten but other calls to the same instance of Mighty). As in Massive, you may not even need to to call `IsValid(item)` directly, since validation is called automatically during CRUD operations.
- All validation is now done via hooks in the `Mighty.Validation.Validator` class, not in the `MightyOrm` class itself. If you need validation you should create your own subclass of this, override the hook methods which you need, and pass a new instance of it to the constructor of `MightyOrm`.

## CRUD

- `SaveAsNew(items)` is no longer required, use `Insert(items)` instead; other uses of `Insert(item)` still work as before
- `Update(item, key)` is replaced by `Update(item)` where item contains the key; your instance of `MightyOrm` already knows which field the key is in
- `Update(item, where, args)` is replaced by `UpdateUsing(partialItem, where, args)`
  - Amongst other things the above two changes allow the new `Update(item1, item2, ...)` to update multiple items, each containing its own key
- `Delete(null, where, args)` is replaced by `Delete(where, args)`; note that `Delete(pk)` still works as before and `Delete(item)` (where item must contain the PK value) is newly available

## Paging

- The version of `Paged(string sql, ...)` for use with arbitrary SQL has been replaced by `PagedFromSelect(columns, tablesAndJoins, where, orderBy, ...)` which is similar but allows correct handling of paged queries with arbitrary joins and qualified column names if you need them; note that the version of `Paged(...)` for use with the default table still works as before
- Paged result sets on SQL Server now include an additional `RowNumber` column even if explicit columns are specified (this extra column already appeared if all columns were selected with `"*"`, though it was called `Row` before)
- Paged result sets on Oracle now have `RowNumber` instead of `r___` as the additional row number column name (the additional column always appears, as it did before)

## Dynamic Methods

Massive had various useful, but effectively rather well-hidden methods, which were supported only dynamically, i.e. the named methods and arguments didn't 'really' exist, but were all processed by a dynamic method provider.

These methods were useful, but having dynamic methods adds a slight overhead to *every* call to Massive (even if it's not stored in a dynamic variable), and more importantly there is no Intellisense at all (so the fact that the methods exist is invisible, and there is no information about what the methods and their arguments do). So, although early versions of Mighty *did* support these methods, they have now all been turned into normal instance methods.

Perhaps the most useful were the `Find`/`Get`/`Single` synonyms. In Massive you could do:

```c#
var film = films.Find(film_id: 42);
var film = films.Single(film_id: 42);
var employee = employees.Find(surname: "Smith", departmentid: 1234);
```

In Mighty, to get an item by primary key, you can just do:

```c#
var film = films.Single(42);
```

And to get an item by a named column, or columns, you can do:

```c#
var film = films.Single(new { film_id = 42});
var employee = employees.Single(new { surname = "Smith", departmentid = 1234 });
```

As you can see, the syntax is pretty close and the same features are available. And yes, the same method `Single` works for both of the above; if passed an object with values only (including a primitive type, or list of primitive types), it uses these as the primary key value (or values). If passed an object which has names and values (including, but not limited to, anonymous objects as used above), then it uses these as a `WHERE` specification.

All of the aggregate functions (`Max`, `Min`, `Sum`, etc.) which were previously available as dynamic methods in Massive are also now full, intellisense-able methods in Mighty.

## Runtime Exceptions

The only one of the above changes which will compile against `MightyOrm` when stored in a non-dynamic variable, even if you don't make the mentioned changes, is `Update(item, key)`. This will be misinterpreted initially as an attempt to update two different items, but running it will give you a helpful runtime exception: `System.InvalidOperationException : Value-only collections not supported for action Update; use Update(item), not Update(item, pk)`.

 You will also get a runtime exception (`RuntimeBinderException`) rather than compile-time errors if you call any no-longer-supported Massive methods against `MightyOrm` stored in a dynamic variable (`dynamic db = new MightyOrm(...)`). To avoid this, and since there's no other reason to store `MightyOrm` itself (as opposed to its results!) [in a dynamic variable any more](#dynamic-methods), we recommend changing your dynamically stored instances of `DynamicModel` (e.g. `dynamic db = new DynamicModel(...)`) if any, to non-dynamically stored instances of `MightyOrm` (e.g. either `var db = new MightyOrm(...)` or `MightyOrm db = new MightyOrm(...)`).