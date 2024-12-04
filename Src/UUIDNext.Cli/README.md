# UUIDNext.Cli

A command line tool to generate and decode UUID/GUID that are either sequential and database friendly (versions 7 & 8) or random (version 4).

## How to Install

UUIDNext.Cli is [available as a tool on nuget.org](https://www.nuget.org/packages/UUIDNext.Cli/)

```text
dotnet tool install --global UUIDNext.Cli
```

## How to Use

```bash
# Creating a UUID Version 4
uuidnext random

# Creating a UUID Version 7
uuidnext sequential

# Creating a database friendly UUID for MS SQL Server (Version 8)
uuidnext database sqlServer

# Decoding version, timestamp and sequence from a UUID version 7
uuidnext decode 017F22E2-79B0-7CC3-98C4-DC0C0C07398F
```

## Quick documentation

```text
Description : 
    Generate a new UUID

Usage : 
    uuidnext command [options] [--clipboard]

Commands : 
    Random            Create a new UUID v4
    Sequential        Create a new UUID v7
    Database [dbName] Create a UUID to be used as a database primary key (v7 or v8 depending on the database)
                      dbName can be "PostgreSQL", "SqlServer", "SQLite" or "Other"
    Decode   [UUID]   Decode the versioo of the UUID and optionally the timestamp an sequence number of UUID v1, 6, 7 and 8
    Version           Show the version

--clipboard : copy output to clipboard
```
