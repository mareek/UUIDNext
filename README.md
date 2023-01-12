# UUIDNext

A fast and modern .NET library to generate UUID/GUID that are either sequential and database friendly (versions 7 & 8), name based (versions  5) or random (version 4).

## How to Install

UUIDNext is [available on nuget.org](https://www.nuget.org/packages/UUIDNext/)

## How to Use

```C#
using System;
using UUIDNext;

// Creating a database friendly UUID (version 7)
Guid sequentialUuid = Uuid.NewDatabaseFriendly(Database.SQLite);
Console.WriteLine($"This is a database friendly UUID : {sequentialUuid}");



// Creating a name based UUID (Version 5)
Guid urlNamespaceId = Guid.Parse("6ba7b811-9dad-11d1-80b4-00c04fd430c8");
Guid nameBasedUuid = Uuid.NewNameBased(urlNamespaceId, "https://github.com/uuid6/uuid6-ietf-draft");
Console.WriteLine($"This is a name based UUID : {nameBasedUuid}");
```

## What are all these versions ? I didn't know there were so many types of GUID

The traditional GUID (a.k.a UUID Version 4) is fine and works really well for it's intended use. But its random nature is problematic in some scenarios that's why other UUID versions have been created.

UUID Version 3 and 5 are name-based UUIDs. They take a namespace and a name as input and produce a hash-like UUID. Usage of Version 3 is discouraged as it is based on the obsolete MD5 hash function.

UUID Version 7 and 8 are intended to be used as a primary key in a database. The randomness of UUID V4 has a negative impact on performance when used as a key in a database and UUID V1 exposed the MAC address of the machine where it was created. UUID V7 & 8 aims to take the best of both worlds without their drawbacks. They are currently at the draft stage so their structure and implementation may change.

## Why creating a new Library ? is there a problem with Guid.NewGuid() ?

As I said, UUIDs V4 produced by Guid.NewGuid() are fine when they are not used in the scenarios described above and there's no reason to stop using them. But if you find yourself in a position where UUID V4 is suboptimal, this library is for you.

## Resources

[RFC 4122](https://datatracker.ietf.org/doc/html/rfc4122) : The standard for UUID Version 1 to 5.

[Draft UUID V6 RFC](https://github.com/uuid6/uuid6-ietf-draft) : The proposed standard for UUID Version 6 to 8
