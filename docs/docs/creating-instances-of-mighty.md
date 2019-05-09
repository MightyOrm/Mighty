---
title: Creating Instances of Mighty
layout: default
nav_order: 2
---

# Creating Instances of Mighty
{: .no_toc }

- TOC
{:toc}

---

## Subclassing MightyOrm

The most ORM-flavoured way of creating instances of Mighty is to create a subclass of `MightyOrm` for each table:

```c#
public class Products : MightyOrm
{
    public Products() : base(Constants.ConnectionString, primaryKeys: "ProductID") {}
}
```

or, using [strong types](strongly-typed-mighty):

```c#
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

The table name is taken from the subclass name. You can override this by passing in a value for the `tableName` constructor parameter.

Then you can start to read and write data:

```c#
var products = new Products();

var allProducts = products.All();
foreach (var product in allProducts)
{
    Console.WriteLine($"{product.Name}: {product.Description}");
}

var product1 = products.Single(1245);
product1.Description("Even better than the real thing");
products.Update(product1);

// etc.
```

In the strongly typed version, each returned product is of type `Product`. In the dynamically typed version each product is a `dynamic` object (actually of type `ExpandObject`) and the fields returned in it are whatever the database sends back. You can control the fields returned by using the `columns` parameter in the constructor:

```c#
public class Products : MightyOrm
{
    public Products() : base(Constants.ConnectionString, primaryKeys: "ProductID", columns: "ProductID, Name, Description") {}
}
```

## Creating Instances Directly

You can also create instances of Mighty directly, without making any subclasses.

> All of the examples in these subsections can be modified slightly to generate [strongly typed](strongly-typed-mighty) instances of `MightyOrm` as well.

### .NET Core

On both .NET Core and .NET Framework you can create an instance by:

```c#
// Non-table specific version
var db = new MightyOrm(connectionString);
```

```c#
// Table specific version
var db = new MightyOrm(connectionString, "Products", "ProductID");
```

The connecting string you pass is any normal ADO.NET connection string, except that *it should additionally include the provider name in the connection string* using the non-standard (but convenient!) `ProviderName=` syntax. E.g. `ProviderName=Oracle.ManagedDataAccess.Client` or  `ProviderName=System.Data.SqlClient`.


### .NET Framework

On .NET Framework connection strings and their associated provider names are normally stored in the `<ConnectionStrings>` section of your `Web.Config` or `App.Config` file and on .NET Framework you can additionally create instances of Mighty with:

```c#
// Non-table specific version
var db = new MightyOrm("Northwind");
```

```c#
// Table specific version
var db = new MightyOrm("Northwind", "Products", "ProductID");
```

Here what you have provided is the connection string name, to look up in the `<ConnectionStrings>` section of your `.Config` file.

> If Mighty can't find a connection string with this name then it will try to use what you passed in directly as a connection string before failing. This means that on .NET Framework you can pass in either a connection string or a connection string name and both will work.

On .NET Framework you can even use:

```c#
var db = new MightyOrm();
```

This creates a non-table specific instance of Mighty using the first connection string in your `Web.Config` or `App.Config` file.

### Factory Method

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
