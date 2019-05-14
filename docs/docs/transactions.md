---
title: Transactions
layout: default
nav_order: 13
---

# Transactions
{: .no_toc }

- TOC
{:toc}

Mighty fully supports transactions.

You can always (i.e. on .NET Framework and .NET Core, and whether your code is sync or async) use `DbConnection.BeginTransaction`.

If your code is not async and is running on .NET Framework you can also use `TransactionScope`.

## BeginTransaction

```c#
var db = new MightyOrm(connectionString);
using (var conn = db.OpenConnection())
{
    using (var trans = conn.BeginTransaction())
    {
        var results = db.ExecuteProcedure("createClient",
            inParams: clientInfo, retParams: new { ClientID = 0 },
            connection: conn);
        foreach (var order in orderInfo)
        {
            var orderPlusId = order.ToExpando();
            orderPlusId.ClientID = results.ClientID;
            db.ExecuteProcedure("createOrder", inParams: orderPlusId, connection: conn);
        }
        trans.Commit();
    }
}
```

## BeginTransaction (async example)

```c#
var db = new MightyOrm(connectionString);
using (var conn = await db.OpenConnectionAsync())
{
    using (var trans = conn.BeginTransaction())
    {
        var results = db.ExecuteProcedureAsync("createClient",
            inParams: clientInfo, retParams: new { ClientID = 0 },
            connection: conn);
        foreach (var order in orderList)
        {
            var orderPlusId = order.ToExpando();
            orderPlusId.ClientID = results.ClientID;
            db.ExecuteProcedureAsync("createOrder", inParams: orderPlusId, connection: conn);
        }
        trans.Commit();
    }
}
```

## TransactionScope (.NET Framework only, sync only)

```c#
var db = new MightyOrm(connectionString);
using (var scope = new TransactionScope())
{
    // with TransactionScope no connection needs to be created or passed in
    var results = db.ExecuteProcedure("createClient",
        inParams: clientInfo, retParams: new { ClientID = 0 });
    foreach (var order in orderInfo)
    {
        var orderPlusId = order.ToExpando();
        orderPlusId.ClientID = results.ClientID;
        db.ExecuteProcedure("createOrder", inParams: orderPlusId);
    }
    scope.Complete();
}
```
