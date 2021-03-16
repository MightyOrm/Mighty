# Mighty Change Log

## 3.2.0

 - Improve async support and optional explicit `DbConnection` support in CRUD methods: New, Insert, Update, Save, Delete

## 3.1.3

 - Prevent .NET Core ProviderName support getting confused by extra spacing in the connection string

## 3.1.2

- Convert DataContractStore and TableMetaDataStore from Dictionary to ConcurrentDictionary
- Fix bug in DataContractKey.Equals

## 3.1.1

- Minor updates to NuGet package description only

## 3.1.0

- Allow reading from database into enum members (despite C# not being keen to allow this!)
- Related minor fixes

## 3.0.6

- Update to .NET Core 3.0 final
- Update to use AsyncEnumerator 3.1.0 (non-beta version)

## 3.0.5

 - Fix Intellisense code comments not working due to [stackoverflow.com/a/57731750/795690](https://stackoverflow.com/a/57731750/795690)

## 3.0.4

- Finish [database mapping](https://mightyorm.github.io/Mighty/docs/database-mapping.html) support
- Finish [documentation](https://mightyorm.github.io/Mighty/)
- Minor improvements to API

## 3.0.3-beta

First release of Mighty.

It's what I would have done if I could have released a version 3 of Massive ;).

.NET Core support and optional named and directional parameter support are where it all came from, everything else springs from those.
