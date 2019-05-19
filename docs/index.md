---
title: Mighty
layout: default
nav_exclude: true 
---

# Mighty

[Get Started](docs/getting-started){: .btn .btn-primary }

A new small, happy, dynamic micro-ORM and general purpose .NET data access wrapper.

Based on and highly compatible with Massive, but now with added:

* .NET Core support (1.0, 1.1, 2.0, 3.0-preview)
* Strongly typed as well as dynamic results
* Parameter names and directions (where you need it; automatic parameter naming as in Massive still works as before)
* Multiple result sets
* Transactions
* Cursors (on Oracle and PostgreSQL; cursors are not designed to be passed out to client code on other databases)
* Simultaneous access to more then one database provider

In addition to .NET Core, Mighty still runs on .NET Framework 4.5+; and on .NET Framework 4.0+ without async support.
