# Coding Style

I'm not claiming any perfect coding style in Mighty - though it does aim to be consistent and readable. It's also been adjusted to at least not produce any code-style warnings in the current versions of VS and VS Code.

I've coded in many different styles (including, but certainly not limited to, SQL programming) for years. I think I know how to write and maintain efficient, readable code and efficient, readable SQL. But I did pick up a few great tips from the way @RobConery's Massive was written, which I've tried to stick to in this project.

## KISS

The main principle (as I see it) is KISS - Keep It Simple, Stupid!

- Use the underlying libraries - if they already do something, don't do it again
- If they don't do quite what you think you need then, first and foremost, evaluate whether you are right about what you think you need!
- If they *really* don't do what you need, write little bits of code to patch things up - but that's quite possibly a sign that you're doing something wrong

I've really tried to keep everything else in Mighty as simple as possible (but no more so ;) **[[ref]]()**), too. I certainly don't claim to have done that perfectly. It's really not easy, and sometimes it's more art than science.

To the extent that I've succeeded, this might make it look as if certain bits of code in Mighty (and, in my experience, in Massive too) are really pretty small, trivial and simple. Often, they're not! Getting the codebase to do 'that', whatever that is, in a small, apparently simple piece of code is what @RobConery was (and is, I'm quite sure!) really excellent at, and it's what I've tried to keep to in the coding style here, too.

## Exceptions

If the underlying libraries will throw a meaningful exception then *don't* wrap it up in an another exception.

This means that a lot of code in Mighty (and actually, in Massive) looks like it isn't really bothering with exception handling. This is not necessarily true. If the exception thrown by the underlying library would already be a comprehensible, meaningful report of the problem to someone coding against this microORM (and if it's a problem for which you would want an exception thrown, anyway) then the coding style here is - do nothing!

That means: think about it first, and make a conscious decision! But a reasonable decision (and the recommended decision, where possible, in this codebase) is to do nothing. This keeps the codebase small, readable and maintainable.

## Unit Testing

As anybody who knows unit testing and reads the above will probably realise, Mighty (like Massive before it) doesn't have *unit* tests. It does have extensive integration tests: if you set up the full test environment and then change things, you *will* know whether or not Mighty is still accessing all the target databases correctly, in all the many different ways which it can. What you won't know is whether or not all those sneaky bits which I've just mentioned are still working exactly as they should. That is exactly what unit tests are for. Adding unit tests to Mighty would involve considerable refactoring. It would be worth doing. It isn't yet done.