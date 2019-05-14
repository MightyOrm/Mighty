---
title: SQL Profiling
layout: default
nav_order: 14
---

# SQL Profiling

When you are using a data access layer one thing you sometimes want to do is actually see the SQL that is generated for you.

Even though Mighty (just like all micro-ORMs) is much more close to the metal than a library such as Entity Framework, it's still helpful (and sometimes really useful for debugging, especially if you are using [mapping](database-mapping)) to be able to do this.

To profile Mighty, use the companion [SqlProfiler](https://github.com/MightyOrm/SqlProfiler) NuGet library. It's really pretty simple!

```c#
public class MyDataProfiler : Mighty.Profiling.DataProfiler
{
    public MyDataProfiler()
    {
        // provide a DbCommand wrapper function for Mighty's DataProfiler
        CommandWrapping = wrapped => new SqlProfiler.Simple.SimpleCommandProfiler(
            wrapped,
            // provide a callback action for SqlProfiler's SimpleCommandProfiler
            (method, command, behavior) =>
            {
                // or whatever you want...
                Debug.WriteLine(command.CommandText);
            });
    }
}
```

Then you can set the profiler for one instance of Mighty with :

```c#
var db = new MightyOrm(connectionString, profiler: new MyDataProfiler());
```

Or globally for all instances of Mighty (both dynamic and strongly typed), with:

```c#
MightyOrm.GlobalSqlProfiler = new MyDataProfiler();
```

Or even globally, but only for all instances of a certain generic type, with:

```c#
MightyOrm<Film>.GlobalSqlProfiler = new MyDataProfiler();
```
