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

### **DecodeUuidV7(Guid)**

Returns the timestamp and the sequence number of a UUID version 7

```csharp
public static ValueTuple<long, short> DecodeUuidV7(Guid guid)
```

#### Parameters

`guid` [Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid)<br>

#### Returns

[ValueTuple&lt;Int64, Int16&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.valuetuple-2)<br>

### **DecodeUuidV8ForSqlServer(Guid)**

Returns the timestamp and the sequence number of a UUID version 8 for SQL Server

```csharp
public static ValueTuple<long, short> DecodeUuidV8ForSqlServer(Guid guid)
```

#### Parameters

`guid` [Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid)<br>

#### Returns

[ValueTuple&lt;Int64, Int16&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.valuetuple-2)<br>
