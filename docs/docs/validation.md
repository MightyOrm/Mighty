---
title: Validation
layout: default
nav_order: 8
---

# Validation
{: .no_toc }

- TOC
{:toc}

## Simple Validation

To provide validation in Mighty, you have to pass in a subclass of the `Validator` class.

A simple validator might just be:

```c#
public class FilmValidator : Validator
{
    override public void Validate<Film>(dynamic item, Action<object> reportError)
    {
        // bogus validation: isn't valid if rental_duration > 5
        if (item.rental_duration > 5)
        {
            reportError("rental_duration > 5");
        }
    }
}

...

var films = new MightyOrm<Film>(connectionString, "Film", validator: new FilmValidator());
```

Note how we use `dynamic` for the item to validate. This means it can hold whatever you can pass in as an input object to Mighty (which is pretty much anything with names and values in it), and importantly is NOT restricted to items of the generic type even for generically typed <see cref="MightyOrm{T}"/>. If you know you're only going to pass in items of the generic type, you can simply put a cast in your validator.

## More Complex Validation

The `Validate` method as above does not distinguish between different [actions](crud-actions) which you might be perfoming (i.e. `Save`, `Insert`, `Update`, `Delete`). To achieve this, override `ValidateForAction(OrmAction action, dynamic item, Action<object> reportError)` instead. The default implementation of this directly calls `Validate` (throwing away its `action` parameter), but you can change that.

`MightyOrm` also provides an `IsValid(item)` method. This always calls `ValidateForAction` with the action type set to `Save` (which in turn just calls `Validate` by default, but you can change that).

## Prevalidation

Prevalidation is off by default, but can be enabled by setting the `PrevalidationType` field of your `Validator` class. Available values are `Off` (default), `Lazy` and `Full`.

When prevalidation is on, before performing an action on an item or items `ValidateForAction(OrmAction action, dynamic item, Action<object> reportError)` is called one at a time on each item. With lazy validation this is stopped after the first item to produce an error. With full validation, all errors for all items are accumulated. In either case, if any errors are found then a `ValidationException` (containing all the errors) is thrown, and no database actions are taken.

## Pre- and Post-Action Hooks

More fine-grained control is additionally provided with the `bool ShouldPerformAction(dynamic item, OrmAction action)`
and `HasPerformedAction(dynamic item, OrmAction action)` methods. `ShouldPerformAction` lets you determine whether an action on an item should take place (without an exception unless you choose to throw one). `HasPerformedAction` lets you check the item, or do anything else you need to do, once an action has taken place.
