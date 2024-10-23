# UuidDecoder

Namespace: UUIDNext.Tools

Provite a set of static methods for decoding UUIDs

```csharp
public static class UuidDecoder
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [UuidDecoder](./uuidnext.tools.uuiddecoder.md)

## Methods

### **GetVersion(Guid)**

Returns the version of the UUID

```csharp
public static int GetVersion(Guid guid)
```

#### Parameters

`guid` [Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid)<br>

#### Returns

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **TryDecodeTimestamp(Guid, DateTime&)**

Try to retrieve the date part a UUID v1, v6, v7 or V8 (if the UUIDv8 is a sequential UUID for SQL Server)

```csharp
public static bool TryDecodeTimestamp(Guid guid, DateTime& date)
```

#### Parameters

`guid` [Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid)<br>

`date` [DateTime&](https://docs.microsoft.com/en-us/dotnet/api/system.datetime&)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
