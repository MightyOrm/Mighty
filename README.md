# Welcome to Mighty, a new, small, dynamic microORM

Mighty is 100% inspired by Massive, but it's a rewrite from scratch.

I believe (and hope!) it will do for you what Massive did for you - only with all of these new features added, should you need any of them ;) :

* .NET Core :-)
* Transactions
* Stored procedures
* Parameter names and directions
* Cursors
* Multiple result sets
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

Without some additional code, this unfortunately cannot work in a few key cases - which means that most of the ADO.NET database code you've likely ever seen, or worked on, always sets *all* the parameter types explicitly. But hey, now you don't need to. Like Massive, Mighty does it for you; or rather, it mostly lets ADO.NET do it for you, but it deals with the special cases.

### So, what is a microORM?

Finally Mighty is a microORM, like Massive. This means that when you create an instance of `MightyModel` with a table name (and optionally a primary key name), then Mighty already knows all about:

- Querying
- Inserting
- Updating
- Deleting

and even

- Paged Result Sets (hurrah!)

Not only that, you can do all of these from *very* sweet and simple API calls.

#### Plus... The New Stuff!

Some databases require more than just CRUD table access. For instance, you might want to read data from a view (Massive and Mighty can both do that), but then write it back via a Stored Procedure, and you might want to be able to see the results of that write-back. Or you might need more control over your transactions (although: go easy on transactions, they're often not as necessary as you think - you don't normally update all your C# objects within C# 'transactions', do you? - and they tend to lock everything up; use good database design to prevent inconsistent data from even being possible, and then carry on happily!), or you might need multiple result sets, or you might need named and directional parameters (to your stored procedures, or even to arbitrary SQL blocks), or you might need to mix and match all of these. And you might even need support for cursors (on the database engines where it makes sense to pass these out of SQL and back to C# - which is not all of them).

This is where Mighty comes into its own, because Massive simply cannot do all of that; and because of design decisions which are pretty much locked in now, I don't realisitically think it's ever going to.

This is why I wrote Mighty:

- Because I want all the cool, lightweight, `dynamic` microORM stuff which Massive invented, and already perfected
- But I also want access to all that other ADO.NET stuff, and preferably also with cool, lightweight (and `dynamic`, where applicable!) ways to do it

That's what Mighty is, and does, now. Plus .NET Core.

You're welcome!

## Code ... please ;)