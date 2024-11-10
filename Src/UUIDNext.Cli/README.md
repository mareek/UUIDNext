# UUIDNext.Cli

A command line tool to generate and decode UUID/GUID that are either sequential and database friendly (versions 7 & 8), name based (versions  5) or random (version 4).

## How to Install

UUIDNext.Cli is [available as a tool on nuget.org](https://www.nuget.org/packages/UUIDNext.Cli/)
```
dotnet tool install --global UUIDNext.Cli
```

## Command line documentation

```
Description : 
    Generate a new UUID

Usage : 
    uuidnext command [options]

Commands : 
    Random            Create a new UUID v4
    Sequential        Create a new UUID v7
    Database [dbName] Create a UUID to be used as a database primary key (v7 or v8 depending on the database)
                      dbName can be "PostgreSQL", "SqlServer", "SQLite" or "Other"
    Decode   [UUID]   Decode the versioo of the UUID and optionally the timestamp an sequence number of UUID v1, 6, 7 and 8
```
