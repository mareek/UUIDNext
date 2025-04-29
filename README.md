# UUIDNext

A fast and modern .NET library to generate UUID/GUID that are either sequential and database friendly (versions 7 & 8), name based (versions  5) or random (version 4).

## How to Install

UUIDNext is [available on nuget.org](https://www.nuget.org/packages/UUIDNext/)

## How to Use

```C#
using System;
using UUIDNext;

// Creating a database friendly UUID for PostgreSQL (version 7) or MS SQL Server (Version 8)
Guid postgreSqlUuid = Uuid.NewDatabaseFriendly(Database.PostgreSql);
Console.WriteLine($"This is a PostgreSQL friendly UUID : {postgreSqlUuid}");

Guid sqlServerUuid = Uuid.NewDatabaseFriendly(Database.SqlServer);
Console.WriteLine($"This is a SQL Server friendly UUID : {sqlServerUuid}");



// Creating a name based UUID (Version 5)
Guid urlNamespaceId = Guid.Parse("6ba7b811-9dad-11d1-80b4-00c04fd430c8");
Guid nameBasedUuid = Uuid.NewNameBased(urlNamespaceId, "https://github.com/uuid6/uuid6-ietf-draft");
Console.WriteLine($"This is a name based UUID : {nameBasedUuid}");
```

## What are all these versions ? I didn't know there were so many types of GUID

The traditional GUID (a.k.a UUID Version 4) is fine and works really well for it's intended use. But its random nature is problematic in some scenarios that's why other UUID versions have been created.

UUID Version 3 and 5 are name-based UUIDs. They take a namespace and a name as input and produce a hash-like UUID. Usage of Version 3 is discouraged as it is based on the obsolete MD5 hash function.

UUID Version 7 and 8 are intended to be used as a primary key in a database. The randomness of UUID V4 has a negative impact on performance when used as a key in a database and UUID V1 exposed the MAC address of the machine where it was created. UUID V7 & 8 aims to take the best of both worlds without their drawbacks.

## Why creating a new Library ? is there a problem with Guid.NewGuid() ?

As I said, UUIDs V4 produced by Guid.NewGuid() are fine when they are not used in the scenarios described above and there's no reason to stop using them. But if you find yourself in a position where UUID V4 is suboptimal, this library is for you.

## Now that .NET 9 can generate UUID v7, do I still need this library ?

The fact that the .NET team added UUID v7 support is a good news but their implementation is pretty simple : it's just a timestamp in ms completed by random data. This may be fine in some use cases but can cause issue in others.
To sum up, you should use UUIDNext if you're in one of these situations:

* You target a .NET version older than .NET 9 (obviously).
* You use MS SQL Server. UUIDNext is the only library that generate UUIDs tailored for SQL Server.
* You do a lot of batch inserts. Contrary to .NET 9, UUIDNext ensure that each generated UUID is greater than the previous one even if they're generated in the same ms.

## But wait, there's more !

If you have some special needs, the [UuidToolkit class](https://github.com/mareek/UUIDNext/blob/main/Doc/uuidnext.tools.uuidtoolkit.md) offers a variety of helper methods To create custom UUIDs.

If you want to retrieve some information from a UUID like its version or the date when it as created, check the [UuidDecoder class](https://github.com/mareek/UUIDNext/blob/master/Doc/uuidnext.tools.uuiddecoder.md).

## Resources

[Documentation](https://github.com/mareek/UUIDNext/tree/main/Doc/index.md).

[RFC 9562](https://www.rfc-editor.org/rfc/rfc9562) : The new standard for UUID Version 1 to 8.
