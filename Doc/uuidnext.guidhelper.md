# GuidHelper

Namespace: UUIDNext

Provite a set of static and extensions methods that brings .NET8+ features to .NET Standard 2.1 and .NET framework

```csharp
public static class GuidHelper
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [GuidHelper](./uuidnext.guidhelper.md)<br>
Attributes [ExtensionAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.extensionattribute)

## Methods

### **FromBytes(Span&lt;Byte&gt;, Boolean)**

Creates a new guid from a span of bytes.

```csharp
public static Guid FromBytes(Span<byte> bytes, bool bigEndian)
```

#### Parameters

`bytes` [Span&lt;Byte&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.span-1)<br>

`bigEndian` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

#### Returns

[Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid)<br>

### **ToByteArray(Guid, Boolean)**

Returns an unsigned byte array containing the GUID.

```csharp
public static Byte[] ToByteArray(Guid guid, bool bigEndian)
```

#### Parameters

`guid` [Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid)<br>

`bigEndian` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

#### Returns

[Byte[]](https://docs.microsoft.com/en-us/dotnet/api/system.byte)<br>

### **TryWriteBytes(Guid, Span&lt;Byte&gt;, Boolean, Int32)**

Returns whether bytes are successfully written to given span.

```csharp
public static bool TryWriteBytes(Guid guid, Span<byte> bytes, bool bigEndian, out int bytesWritten)
```

#### Parameters

`guid` [Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid)<br>

`bytes` [Span&lt;Byte&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.span-1)<br>

`bigEndian` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

`bytesWritten` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
