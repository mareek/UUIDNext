# Database

Namespace: UUIDNext

The Database where the UUID will be stored

```csharp
public enum Database
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [Enum](https://docs.microsoft.com/en-us/dotnet/api/system.enum) → [Database](./uuidnext.database.md)<br>
Implements [IComparable](https://docs.microsoft.com/en-us/dotnet/api/system.icomparable), [ISpanFormattable](https://docs.microsoft.com/en-us/dotnet/api/system.ispanformattable), [IFormattable](https://docs.microsoft.com/en-us/dotnet/api/system.iformattable), [IConvertible](https://docs.microsoft.com/en-us/dotnet/api/system.iconvertible)

## Fields

| Name | Value | Description |
| --- | --: | --- |
| Other | 0 | Any other Database (MySQL, Oracle, MongoDB, etc.) |
| SqlServer | 1 | Microsoft SQL Server (uniqueidentifier Type) |
| SQLite | 2 | SQLite (BLOB or TEXT Type) |
| PostgreSql | 3 | PostgreSQL (UUID Type) |
