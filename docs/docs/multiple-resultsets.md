---
title: Multiple Resultsets
layout: default
nav_order: 7
---

# Multiple Resultsets

The pattern for reading multiple resultsets in Mighty is as follows:

```c#
MightyOrm db = new MightyOrm(connectionString);
var resultsets = db.QueryMultiple("SELECT * FROM Employees; SELECT * FROM Contacts;");
foreach (var results in resultsets)
{
	foreach (var item in results)
	{
		Console.WriteLine($"{item.Name}");
	}
}
```
