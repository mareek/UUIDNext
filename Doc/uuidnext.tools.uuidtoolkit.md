# UuidToolkit

Namespace: UUIDNext.Tools

Provite a set of static methods for generating custom UUIDs

```csharp
public static class UuidToolkit
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [UuidToolkit](./uuidnext.tools.uuidtoolkit.md)

## Methods

### **CreateUuidV7FromSpecificDate(DateTimeOffset)**

Create a new UUID version 7 with the given date as timestamp

```csharp
public static Guid CreateUuidV7FromSpecificDate(DateTimeOffset date)
```

#### Parameters

`date` [DateTimeOffset](https://docs.microsoft.com/en-us/dotnet/api/system.datetimeoffset)<br>

#### Returns

[Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid)<br>

### **CreateUuidV7(Int64, Span&lt;Byte&gt;)**

Create a new UUID version 7 with the given timestamp and bytes of otherBytes, filling the rest with random data

```csharp
public static Guid CreateUuidV7(long timestamp, Span<byte> followingBytes)
```

#### Parameters

`timestamp` [Int64](https://docs.microsoft.com/en-us/dotnet/api/system.int64)<br>
A unix epoch timestamp in ms

`followingBytes` [Span&lt;Byte&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.span-1)<br>
A series of 0 to 10 bytes used to fill the rest of the UUID. 
 Be careful: some bits will be overxritten by the version and variant of the UUID (see remarks)

#### Returns

[Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid)<br>
A UUID Version 7

#### Exceptions

[ArgumentException](https://docs.microsoft.com/en-us/dotnet/api/system.argumentexception)<br>

**Remarks:**

Here is the bit layout of the UUID Version 7 created
```
 0                   1                   2                   3
 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|                           timestamp                           |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|           timestamp           |  ver  |      otherBytes       |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|var|                      otherBytes                           |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|                          otherBytes                           |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
```

### **CreateGuidFromBigEndianBytes(Span&lt;Byte&gt;)**

Create new UUID version 8 with the provided bytes with the variant and version bits set

```csharp
public static Guid CreateGuidFromBigEndianBytes(Span<byte> bigEndianBytes)
```

#### Parameters

`bigEndianBytes` [Span&lt;Byte&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.span-1)<br>
the bytes that will populate the UUID in big endian order

#### Returns

[Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid)<br>
a UUID version 8

**Remarks:**

Here is the bit layout of the UUID Version 8 created :
```
 0                   1                   2                   3
 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|                        bigEndianBytes                         |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|       bigEndianBytes          |  ver  |    bigEndianBytes     |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|var|                    bigEndianBytes                         |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|                        bigEndianBytes                         |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ 
```

### **CreateUuidFromName(Guid, String, HashAlgorithm)**

Create a new name based UUID version 8 according to section 6.5 of the RFC

```csharp
public static Guid CreateUuidFromName(Guid namespaceId, string name, HashAlgorithm hashAlgorithm)
```

#### Parameters

`namespaceId` [Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid)<br>
the namespace where the name belongs

`name` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
the name to be hashed

`hashAlgorithm` HashAlgorithm<br>
the hash algorithm used to compute the UUID (MD5, SHA-256, etc.)

#### Returns

[Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid)<br>
A UUID version 8
