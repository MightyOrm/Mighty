---
title: Creating Instances of Mighty
layout: default
nav_order: 2
---

# Creating Instances of Mighty

## .NET Core

On both .NET Core and .NET Framework you can create an instance of Mighty by:

```c#
var db = new MightyOrm(connectionString);
```

The connecting string is any normal ADO.NET connection string, except that *it should additionally include the provider name in the connection string* using the non-standard (but convenient!) `ProviderName=` syntax. E.g. `ProviderName=Oracle.DataAccess.Client` or  `ProviderName=System.Data.SqlClient`.


## .NET Framework

On .NET Framework connection strings and their associated provider names are normally stored in the `<ConnectionStrings>` section of your `Web.Config` or `App.Config` file and on .NET Framework you can additionally create instances of Mighty with:

```c#
var db = new MightyOrm("Northwind");
```

Here what you have provided is the connection string name, to look up in the `<ConnectionStrings>` section of your `.Config` file.

On .NET Framework you can even use:

```c#
var db = new MightyOrm();
```

That creates a non-table specific instance using the first connection string in your `Web.Config` or `App.Config` file.

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
