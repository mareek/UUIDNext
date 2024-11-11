# GuidComparer

Namespace: UUIDNext.Tools

Compares two Guids

```csharp
public class GuidComparer : System.Collections.Generic.IComparer<System.Guid>
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [GuidComparer](./uuidnext.tools.guidcomparer.md)<br>
Implements [IComparer&lt;Guid&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.icomparer-1)

## Constructors

### **GuidComparer()**

```csharp
public GuidComparer()
```

## Methods

### **Compare(Guid, Guid)**

Compares two Guids and returns an indication of their relative sort order.

```csharp
public int Compare(Guid x, Guid y)
```

#### Parameters

`x` [Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid)<br>

`y` [Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid)<br>

#### Returns

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>
