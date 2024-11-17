# Version History

## 4.0.0 (TBD)

* Added UuidToolkit for advanced UUID generation
* UuidDecoder can now read timestamp and sequence from every version that has timestamp and sequence
* Added some documentation
* Created UUIDNext.Cli, a command line utility to generate and decode UUID. Available on nuget as a .NET tool
* Removed obsolete methods (this might cause breaking changes)

## 3.0.0 (2024-07-02)

* Added support for .NET Framework 4.7+
* Reduced the public surface of the library (this might cause breaking changes)
* Added a logo
* Enabled SourceLink

## 2.0.2 (2023-06-08)

* Added PostgreSQL to the list of supported databases
* Added Nil UUID

## 2.0.0 (2022-01-09)

* Added the ability to genereate UUID V8 taylored for Microsoft SQL Server
* NewDatabaseFriendly requires a database parameter to genereate the best UUID for each database

## 1.1.1 (2022-04-25)

* Greatly improved UUID V7 performance when generating a lot of UUID in batch

## 1.1.0 (2022-04-03)

* Updated UUID V7 format according to latest draft RFC
* Added Max UUID

## 1.0.2 (2021-12-22)

* Fix handling of sequence overflow
* Fix various bugs

## 1.0.1 (2021-08-27)

* Added Uuid.NewDatabaseFriendly static method

## 1.0.0 (2021-08-23)

* First version
