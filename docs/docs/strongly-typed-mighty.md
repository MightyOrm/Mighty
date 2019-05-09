---
title: Strongly-Typed Mighty
layout: default
nav_order: 3
---

# Strongly-Typed Mighty

Mighty supports strong typing for all operations.

Because Mighty (following Massive) was dynamic from the start, strong typing only affects the output of Mighty operations. Inputs can be objects of the required type, but can also be any reasonable name-value collection, including anonymous objects, etc.

```c#
public class Person
{
    public int PersonID { get; set; }
    public string GivenName { get; set; }
    public string FamilyName { get; set; }
    public bool LoyalCustomer { get; set; }
}

var db = new Mighty<Person>(connectionString, primaryKeys: "PersonID");

// The input here is an anonymous object (though it would have worked fine with
// a Person object, or an ExpandoObject, or most other reasonable choices),
// the output is strongly typed
Person p = db.Insert(new { FamilyName = "Beaton", GivenName = "Mike" });

// The primary key which the database generated in the insert
Console.WriteLine(p.PersonID);
```
