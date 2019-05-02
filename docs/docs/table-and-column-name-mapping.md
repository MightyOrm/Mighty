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

## Convention based mapping

There are two ways of managing table and column name mapping in Mighty. One is to pass in an instance of `SqlNamingMapper` to the constructor of `MightyOrm`.

Here is a null mapper (it applies Mighty defaults):

```c#
public sealed class MyMapper : SqlNamingMapper
{
    override public bool UseCaseSensitiveMapping() { return false; }
    override public string GetTableName(Type type) { return type.Name; }
    override public string GetColumnName(Type type, string name) { return name; }
    override public string GetPrimaryKeyFieldNames(Type type) { return null; }
    override public string GetSequenceName(Type type) { return null; }
    override public string QuoteDatabaseIdentifier(string id) { return id; }
}
```

You can then start to override the defaults to apply the conventions of the database you are using, for example:

```c#
    override public string GetTableName(Type type) { return $"tbl_{type.Name}"; }
```

To use this:

```c#
public class Category
{
	int CategoryID;
	string CategoryName;
}

var mapper new MyMapper();
var db = new MightyOrm<Category>(mapper: mapper);
// now all queries will be mapped using the mapper
```

> Mapping can also be used on dynamic (non-strongly typed) instances of `MightyOrm`. If you sub-class `MightyOrm`, as in the first examples [here](creating-instance-of-mighty), then the table name, primary key and sequence (for sequence based databases) can all optionally be generated from the subclass's name using a mapper.

IMPORTANT: If you do use convention based mapping, try to create one instance of your mapper and re-use it as often as possible. Mighty can only cache the mapping from database to class if it is certain you are using the same mapper.

## Attribute based mapping

In addition to convention based mapping using a mapper class you can also - or as well - use attribute based mapping.
Here's a short example of how you might set it up:

```c#
[MightyTable("tbl_Category")]
public class Category
{
	int CategoryID;
	string CategoryName;
	[MightyIgnore]
	IEnumerable<Subcategory> Subcategories;
}

[MightyTable("tbl_Subcategory")]
public class Subcategory
{
	[MightyColumn]
	internal int CategoryID; // only visible to Mighty because of the MightyColumn attribute
	int SubcategoryID;
	string CategoryName;
}

var mapper new MyMapper();
var categories = new MightyOrm<Category>(mapper: mapper);
var subcategories = new MightyOrm<Subcategory>(mapper: mapper);
// now all queries will be mapped using the mapper
```

There are some things you can only do with attribute based mapping, in particular you can only access non-public fields or properties by adding the `MightyColumn` attribute, so you can't ask Mighty to access non-public fields or properties in classes which you don't own.

> For more advanced usage, in addition to allowing a column name specification, the `MightyColumn` attribute can also specify the data direction (`Read`, `Write` or `Both`).