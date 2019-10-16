---
title: Creating Instances of Mighty
layout: default
nav_order: 2
---

# Creating Instances of Mighty
{: .no_toc }

- TOC
{:toc}

## Creating Instances

You can create instances of Mighty very simply, without making any subclasses:

```c#
// Non-table specific dynamic version
var db = new MightyOrm(connectionString);
```

```c#
// Table specific dynamic version
var db = new MightyOrm(connectionString, "Products", "ProductID");
```

You can also create strongly typed instances:

```c#
// Table specific strongly typed version
public class Product
{
    int ProductID { get; set; }
    string Name { get; set; }
    string Description { get; set; }
}

public class Products : MightyOrm<Product>
{
    public Products() : base(Constants.ConnectionString, primaryKeys: "ProductID") { }
}
```

And, especially useful when working with dynamic types, you can also create instances of Mighty by creating a subclass of `MightyOrm` for each table:

```c#
public class Products : MightyOrm
{
    public Products() : base(Constants.ConnectionString, primaryKeys: "ProductID") {}
}
```

In that last example the table name is taken from the subclass name, though you could still override this by passing in a value for the `tableName` constructor parameter.

In the strongly typed table-specific version above, each returned product is of type `Product`. In the dynamically typed versions each product is a `dynamic` object (actually of type `ExpandObject`) and the fields returned in it are whatever the database sends back. For either type, you can simply control the fields returned by using the `columns` parameter in the constructor. (Though more advanced [mapping](database-mapping) is also available.)

## Using Connection Strings

### .NET Core
{: .no_toc }

On .NET Core the connecting string you pass is any normal ADO.NET connection string, except that *it should additionally include the provider name in the connection string* using the non-standard (but convenient!) `ProviderName=` syntax. E.g. `ProviderName=Oracle.ManagedDataAccess.Client` or  `ProviderName=System.Data.SqlClient`.

### .NET Framework
{: .no_toc }

On .NET Framework connection strings and their associated provider names are normally stored in the `<ConnectionStrings>` section of your `Web.Config` or `App.Config` file and on .NET Framework you can additionally pass the connection string name to create instances of Mighty:

```c#
var db = new MightyOrm("Northwind");
```

> If Mighty can't find a connection string with this name then it will try to use what you passed in directly as a connection string before failing. This means that on .NET Framework you can pass in either a connection string or a connection string name and both will work.

On .NET Framework you can even use:

```c#
var db = new MightyOrm();
```

This creates a non-table specific instance of Mighty using the first connection string in your `Web.Config` or `App.Config` file.

## Factory Method

For convenience and compatibility with Massive, Mighty also provides the `Open()` factory method:

```c#
// .NET Core and .NET Framework (on .NET Framework you may pass a connection string name)
var db = Mighty.Open(connectionString);
```

```c#
// .NET Framework only (uses first connection string in .config file)
var db = Mighty.Open();
```

> Using `Open()` you can only specify a connection string, which is all you need for quick, non-table specific queries. When you need to use non-default values for any of the other `MightyOrm` constructor parameters, just use the constructor directly.
