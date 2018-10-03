# Welcome to Mighty, a new, small, dynamic microORM

Mighty is 100% inspired by the highly popular Massive microORM, but it's a rewrite from scratch.

The reason for re-writing it is that, whilst it will indeed do for you what Massive did for you (and in a highly compatible, near drop-in, way - see below), it additionally has all of these new features added, should you need any of them ;) :

* .NET Core :-)
* Transactions
* Stored procedures
* Parameter names and directions
* Multiple result sets
* Cursors
* Compound primary keys
* Simultaneous support for more then one database provider

Why might you use Mighty over Massive? Because you need any of the above.

Why might you use it over Dapper? Because Mighty inherits the lovely approach to wrapping .NET data access  developed by [@RobConery](https://github.com/RobConery) in the original Massive.

It's lightweight, easy to use, intuitive and quick to develop with.

## Getting Started

### General

Mighty is no longer delivered as two or three (large) drop in code files. Unless you specifically want to modify the Mighty codebase, you no longer need to download any files out of this repository, just import the [MightyORM](https://www.nuget.org/packages?q=MightyORM) NuGet library into your project. It is currently a prerelease package, so remember to tick 'Include prerelease' in order to be able to find it in searches of NuGet.

Then, if you were previously using Massive, to get started you must change `using Massive` to `using Mighty` and `DynamicModel` to `MightyORM`. After that, the vast majority of code which used to run against Massive will just start working against Mighty instead. Also, for now, if you are looking at the Massive code samples whilst I get on with finishing that part of this document(!), then the above will also be the only changes you need to make, to get the vast majority of that code working.

### .NET Core

If you are switching to .NET Core, then you must now pass in a connection string where you would have previously passed in a connection string *name* to Massive. This is because .NET Core doesn't have any default config file support and so there no standard meaning for a connection string *name* in .NET Core, unlike in .NET.

When *not* on .NET Core, both these syntaxes are supported, Mighty works out for itself which one was passed in: it can make a quick test to see if what you have passed in is indeed a connection string name, according to .NET's config file support, and if it's not then Mighty will try to parse what you've sent in as a connection string. Both syntaxes are perfectly useful (when not on .NET Core) and having both also means that old Massive code will just keep working.

NB When you pass in a connection string (as opposed to a connection string name) (as you MUST, on .NET Core) this needs some new way of specifying which database provider you want to use (the provider is specified next to the connection string, for each named connection string in .NET config files). To do so, simply add the non-standard `ProviderName=...` to your original connection string:

```csharp
string someConnectionString = "param1=abc;param2=dec;param3=fgh";
string mightyConnectionString = someConnectionString + ";ProviderName=System.Data.SqlClient";
var db = new MightyORM(mightyConnectionString);
... etc. ...
```

Mighty looks in connection strings for this additional parameter, strips it out, and then passes what's left as a normal connection string to the specified provider.

## The History of Mighty

Originally I hoped I could call the predecessor of this project Massive 3, or at least Massive X or something, because I spent a lot of time producing all of this new stuff in a way which could simply drop in to an existing Massive project and just work. And it did work. You could compile that code as a DLL and it dropped in and passed the Massive v2.0 test suite with no changes at all required - as well, of course, as passing all its own new tests.

What I didn't do - a naive but genuine mistake - was to make sure that I had the maintainer of the repository properly on board. I mean, I did say I was working on stuff (SP support, .NET support; and that *was* duly acknowledged), but I got carried away and developed a ton of new stuff and then stupidly dropped it like a ton of bricks. I could have tried to back-track, and offer my changes slowly. In fact, I *did* offer MySQL support for Massive: it was accepted and included. But it was also becoming increasingly clear that the changes I was making were going beyond where the maintainer wanted Massive ever to go. That's entirely his choice, not a problem.

So at that point, perhaps the obvious choice to make would be to let this project live as a fork of Massive - with some extra features (including .NET Core support) for people who need them - and perhaps with me running to catch up with any changes made on the main project. That isn't the choice I made. This *isn't* a fork of Massive. It's a re-write from scratch of the core engine (ending up with a similar, and still highly compatible - but not identical in a few edge cases (see below) - API), then with all of the new features that I originally worked on added back on top.

This project wouldn't possibly exist in its present form without [@RobConery](https://github.com/RobConery)'s genius approach to beautifully wrapping .NET database access in the original version of Massive, in the first place, nor indeed without [@FransBouma](https://github.com/FransBouma)'s refactoring and updating of the Massive codebase after that. But this codebase genuinely is NOT Massive. It IS what I wanted Massive to be (and very much *did* offer to help make it be). It's also not what Massive is ever going to be. Not because I didn't want to contribute these changes, but because it turns out that they take Massive in directions that @FransBouma didn't ever want to go in - arguably for some good reasons (see a litt more in **The New Stuff** section below). So this project is what it is. It's the version of Massive I wanted to use myself. Only now it's not Massive, it's Mighty.

## A Little Technical Background

### Why `dynamic`?
Like Massive, Mighty isn't just a dynamic microORM, it's specifically a `dynamic` microORM.

That is, it's based around C#4's `dynamic` types. If you don't know them, you can think of them as a bit like JavaScript objects (if you know those). They have named and typed members and methods, accessible with normal C# dot notation; but exactly what those members and methods are is only determined at runtime, and can change during an object's life. This is great for returning typed data which matches whatever is in your SQL tables (or your tabled-valued functions, stored produces, etc.), but with basically no mapping or set up on your part at all!

### The power of ADO.NET
The other secret which Mighty uses, a lot, and which I only found out about from working on Massive, is that ADO.NET is actually very good at automatically setting the `DbType` of parameters from the data you put into them.

Without some additional code this unfortunately cannot work in a few key cases - which means that most of the ADO.NET database code you've likely ever seen, or worked on, always sets *all* the parameter types explicitly. But hey, now you don't need to! Like Massive, Mighty does it for you; or rather, it mostly lets ADO.NET do it for you, but it deals with the special cases.

### So, what is a microORM?

Mighty is a microORM, like Massive. This means that when you create an instance of `MightyORM` with a table name (and optionally a primary key name), then Mighty already knows all about:

- Querying
- Inserting
- Updating
- Deleting

and even

- Paged Result Sets (hurrah!)

Not only that, you can do all of these from *very* sweet and simple API calls.

## The New Stuff

Some databases require more than just CRUD table access.

For instance, you might want to read data from a view (Massive and Mighty can both do that), but then write it back via a Stored Procedure and see output or return parameter results from that call. Or you might need more control over your transactions, or you might need multiple result sets, or you might need named and directional parameters (to stored procedures or arbitrary SQL blocks). Or you might need to mix and match all of these. You might even need support for compound primary keys, or access to cursors (on the database engines where it makes sense to pass these out of SQL and back to C# - which is not all of them).

This is where Mighty comes into its own, because Massive simply cannot do all of that, right now. In fact, it's almost certainly never going to do all of that, because its design goal is to be a relatively simple microORM which inspires other ORMs. It has worked. (And far more than once: [**ref**]() [**ref**]() and many more.) Note, though, that Mighty is still a relatively small and simple codebase. The only way it can possibly be that *and* support all of the above, is to be (re)built, at its core, using the genius principles which Massive introduced.

Basically, this is why I wrote Mighty:

- Because I wanted all that cool, lightweight, `dynamic` microORM stuff which Massive invented, and already perfected
- But I also want access to all that other ADO.NET stuff - preferably also via a cool, lightweight (and `dynamic`, where applicable) interface

Mighty is all of that, and does it now. Plus .NET Core.

## Differences from Massive

### Code changes required, *if* you are using any of these more obscure parts of Massive

Here is a list of those items which won't compile directly against former Massive code. In all cases, the same (sort of) thing *is* still supported, with some minor code changes on your part; and whilst this might look like quite a large list, these are all actually 'edge cases' - they're not the core features, at all, and you very probably aren't using them (unless you know that you are, of course!):

#### DataTable support

- DataTable is no longer directly supported (not even on .NET Framework) (however if you need it, the (short) Massive source code for .ToDataTable() is [here](https://github.com/FransBouma/Massive/blob/583c0932cb5da17f06216777be74e16a421f2df4/src/Massive.Shared.cs#L140-L187)) (it is open source, under the same license as this project)

#### Table meta-data

- .Schema is now called .TableMetaData ('schema' was potentially misleading on those DB systems where it also means something like 'namespace')
- .DefaultValue(columnName) is now called .GetColumnDefault(columnName)
- .Prototype is replaced by .New()
- .CreateFrom(coll) is replaced by .NewFrom(obj) (where obj can be NameValueCollection as before, but now can also be any other sensible way of specifying a name-value set)

#### Validation

- .IsValid(item) now returns a List&lt;object&gt; of errors (this is intentionally a List, not an IList!). Each object is one error; the error objects are typically strings, but you get to decide this in your validation class. So `db.IsValid(item).Errors.Count == 0` checks for no errors (but also gets back the reported errors without storing them in a shared variable). But (as in Massive) you typically don't need to call .IsValid(item) directly since validation is called automatically during CRUD operations, as follows.
- All validation is now done via hooks in the `Mighty.Validation.Validator` class, not in the MightyORM class itself. If you need validation you should create your own subclass of this, override the hook methods which you need, and pass a new instance of it to the constructor of MightyORM.

#### CRUD

- .SaveAsNew(items) is no longer required, use .Insert(items) instead; other uses of .Insert(item) still work as before
- .Update(item, key) is replaced by .Update(item) where item contains the key; your instance of MightyORM already knows which field the key is in
- .Delete(null, where, args) is replaced by .Delete(where, args); note that .Delete(pk) still works as before and .Delete(item) is newly available

#### Paging

- The version of .Paged(string sql, ...) for use with arbitrary SQL has been replaced by .PagedFromSelect(columns, tablesAndJoins, where, orderBy, ...), which is similar but allows correct handling of paged queries with arbitrary joins and qualified column names if you need them; note that the version of .Paged(...) for use with the default table still works as before
- Paged result sets on SQL Server now include an additional `RowNumber` column even if explicit columns are specified (this extra column already appeared if all columns were selected, though it was called `Row` before)
- Paged result sets on Oracle now have `RowNumber` instead of `r___` as the additional row number column name (and the additional column always appears, as it did before)


The only one of the above which will compile against a strongly-typed instance of MightyORM even if you don't make the mentioned changes is .Update(item, key), but running it will give you a meaningful runtime exception. (You will also get runtime not compile-time errors if you call any of the old versions against MightyORM stored in a dynamic variable.)

### Not yet supported

Here's the **only** thing from Massive (to the best of my knowledge) that isn't *yet* supported or ported, in any way:

#### Async

- I have not yet ported the async support which [@FransBouma](https://github.com/FransBouma) very usefully added to Massive when he took it over (even though I have made a great effort to preserve and support *everything* else which Massive does). This is simply because I didn't need it and didn't think I could justify the time, at the time. (It's also because I don't think async support will be fundamentally complex to add back. Making sure that Mighty supported everything else which Massive supports *was* complex - in the sense that it's a whole inter-linked set of features, which all need to be provably (by the extended test suite) right at the get-go, to make this project a go-er, IMHO!) Async support is on the road-map.


## Code ... please ;)

TBD - For now, please do refer to the Massive documentation - all of that code still works against Mighty. And for examples of code using the new features, as in Massive (sorry, purists, I know it's wrong!), for now please have a look in the tests.

### A note on transactions

> Note: Mighty now supports transactions. But go easy, they're often not as necessary as you think. For instance, you (almost certainly) don't normally update your C# objects within C# 'transactions', do you? Yet things work fine. Database transactions tend to lock everything up, often unecessarily. It is usually far better to use good database design to prevent inconsistent data from even being possible, and then to carry on lightly, treating updates the way you already treat C# updates. (Obviously there *are* cases where correct transactional handling is very important, the canonical example being financial software; but database transactions are an expensive solution to an expensive problem - don't think that they come for free, just because they are supported for when you really need them.)

## What's next?

Coming soon:

- Generically type return value support (e.g. ), like Dapper, whilst still supporting exactly Massive's dynamic return value syntax as well
- Async support
- Firebird database support (it really should't be hard, if anyone says they need it...)

## Using Mighty with .NET Web API 2

This just works out of the box, and it's very easy to do:

`... TO DO: insert code sample ...`

However, out of the box, this only works when each call to your API requests that the API return JSON. It does not work when the caller requests XML. This is because XML is strongly typed, and Massive's `dynamic` results aren't. There is no way for .NET to *automatically* fill out an XML response from a dynamic object, but it can for a JSON response.

An annoying side effect of this is that if you use your browser to look at the return results from your RESTFUL API, e.g. by manually typing GET URLs (which can be a perfectly reasonable thing to do, I'd say), then this will fail (with an exception) because using your browser this way, it doesn't specify the response type, and .NET Web API 2 defaults to XML. The simplest solution to disable XML support in your Web 2 API project, assuming you don't need it, like this:

`... TO DO: insert code sample ...`

Another, more complex solution (I don't claim the below code is complete or fully correct, it's a quick hack which does show the general idea) is to add `ExpandoObject` support to the WebAPI 2 XML output processing pipe, as follows:

`... TO DO: insert code sample ...`

It's worth being aware that this second approach is really an abuse of XML, because now `<DynamicObject ... />` will be the object type of *all* your API responses, but will mean something different, with differently shaped contents, in each. For that reason, I'd recommend turning off XML and just using JSON if you can. 

## Configuration

For more advanced usage, the following properties can be set in the constructor:

 - Validator - custom validation for per-item insert/update/delete
 - SqlMapper - mapper from C# class and property names to SQL table and column names
 - ConnectionProvider - support custom ways of mapping from connection string to DbProviderFactory (you don't normally need this, default versions for .NET Framework and .NET Core are provided, and automatically instantiated if you don't specify your own)