# Welcome to Mighty, a new, small, dynamic microORM

Mighty is 100% inspired by Massive, but it's a rewrite from scratch.

I believe (and hope!) it will do for you what Massive did for you - only with all of these new features added, should you need any of them ;) :

* .NET Core :-)
* Transactions
* Stored procedures
* Parameter names and directions
* Multiple result sets
* Cursors
* Compound primary keys
* Simultaneous support for more then one database provider

### The History of Mighty
I really, really wanted to call the predecessor of this project Massive3 or MassiveX or something, because initially I spent a lot of time producing all of this new stuff in a way which could simply drop in to an existing Massive project and just work. (And it worked! You could compile it as a DLL and it dropped in and passed the Massive v2.0 test suite with no changes at all required - as well, of course, as passing all its own new tests.)

But I was forcefully told, by several members of the community, include @RobConery, that this was simply the wrong way to go about it, and that if I want to make a fork of Massive, I had to call it something else. They were right, I was wrong. So, in the end, this *isn't* a fork of Massive. It's a re-write from scratch of the same ideas which Massive uses in its core engine (with a similar, but not identical, API), then with all my own new ADO.NET stuff added back on top.

This project couldn't possibly exist without @RobConery's genius Massive in the first place, and without @FransBouma's expert refactoring and updating of the Massive codebase after that. But this codebase is NOT the Massive codebase. It is what I wanted Massive to be (and very much *did* offer to help make it be - maybe in an overbearing and annoying way). But it's not what Massive is ever going to be. So it is what it is. It's "the version of Massive I wanted", only now it's not Massive, it's Mighty.

### A Little Technical Background

#### Why `dynamic`?
Like Massive, Mighty isn't just a dynamic microORM, it's specifically a `dynamic` microORM.

That is, it's based around C#4's `dynamic` types. If you don't know them, you can think of them as a bit like JavaScript objects (if you know those). They have named and typed members and methods, accessible with normal C# dot notation; but exactly what those members and methods are is only determined at runtime, and can change during an object's life. This is great for returning typed data which matches whatever is in your SQL tables (or your tabled-valued functions, stored produces, etc.), but with basically no mapping or set up on your part at all!

#### The power of ADO.NET
The other secret which Mighty uses, a lot, and which I only found out about from working on Massive, is that ADO.NET is actually very good at automatically setting the `DbType` of parameters from the data you put into them.

Without some additional code this unfortunately cannot work in a few key cases - which means that most of the ADO.NET database code you've likely ever seen, or worked on, always sets *all* the parameter types explicitly. But hey, now you don't need to! Like Massive, Mighty does it for you; or rather, it mostly lets ADO.NET do it for you, but it deals with the special cases.

### So, what is a microORM?

Mighty is a microORM, like Massive. This means that when you create an instance of `MightyModel` with a table name (and optionally a primary key name), then Mighty already knows all about:

- Querying
- Inserting
- Updating
- Deleting

and even

- Paged Result Sets (hurrah!)

Not only that, you can do all of these from *very* sweet and simple API calls.

#### Plus... The New Stuff!

Some databases require more than just CRUD table access.

For instance, you might want to read data from a view (Massive and Mighty can both do that), but then write it back via a Stored Procedure and see output or return parameter results from that call. Or you might need more control over your transactions, or you might need multiple result sets, or you might need named and directional parameters (to stored procedures or arbitrary SQL blocks). Or you might need to mix and match all of these. You might even need support for compound primary keys, or access to cursors (on the database engines where it makes sense to pass these out of SQL and back to C# - which is not all of them).

This is where Mighty comes into its own, because Massive simply cannot do all of that, right now. In fact, it's almost certainly never going to do all of that, because its design goal is to be a relatively simple microORM which inspires other ORMs. It has worked. (And far more than once: [**ref**]() [**ref**]() and many more.) Note, though, that Mighty is still a relatively small and simple codebase. The only way it can possibly be that *and* support all of the above, is to be (re)built, at its core, using the genius principles which Massive introduced.

Basically, this is why I wrote Mighty:

- Because I want all that cool, lightweight, `dynamic` microORM stuff which Massive invented, and already perfected
- But I also want access to all that other ADO.NET stuff - preferably also via a cool, lightweight (and `dynamic`, where applicable) interface

Mighty is all of that, and does it now. Plus .NET Core.

You're welcome!

## Code ... please ;)

### Transactions

> Note: Go easy on transactions, they're often not as necessary as you think. For instance, you (almost certainly) don't normally update your C# objects within C# 'transactions', do you? Yet things work fine. Database transactions tend to lock everything up, often unecessarily. It is usually far better to use good database design to prevent inconsistent data from even being possible, and then to carry on lightly, treating updates the way you already treat C# updates. (Obviously there *are* cases where correct transactional handling is very important, the canonical example being financial software; but database transactions are an expensive solution to an expensive problem - don't think that they come for free, just because they are supported for when you really need them.)

## What's next?

Coming soon:

- Firebird database support
- Async support
- Possibly, *optional* generically typed return value support; for now, if you are sure you really need that, try PetaPoco
	- My guess is you'll find you really don't *need* that; try Mighty for a bit, you'll get used to working with dynamic objects pretty quickly, and you'll start to miss them in other contexts, where you can't use them!

## Using Mighty with .NET Web API 2

This just works out of the box, and it's very easy to do:


However, out of the box this works with JSON, but not with XML - because XML wants to specify the type of each object it is returning, and `dynamic` objects don't have any specific type. Unfortunately, if you use a browser to look at the results from your RESTFUL API (which is a pretty reasonable thing to do) then your browser won't specify either JSON or XML and Web API 2 will chose to return XML by default. The simplest solution to disable XML support, so that JSON is the only (and therefore the default) input and output type:

Another more complex solution (I don't claim that the below is complete or fully correct, it's a quick hack to show the general idea) is to add 'fake' ExpandoObject support to the WebAPI 2 XML output:

This is arguably an abuse of XML since now '&lt;DynamicObject&gt;' will occur in all your API responses but will contain appropriate, but different, data in each different API you provide.

## Configuration

For more advanced usage, the following properties can be set in the constructor:

 - Validator - custom validation for per-item insert/update/delete
 - Mapper - mapper from class and property names to table and column names
 - ConnectionProvider - support custom ways of mapping from connection string to 