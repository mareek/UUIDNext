# Uuid

Namespace: UUIDNext

Provite a set of static methods for generating UUIDs

```csharp
public static class Uuid
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Uuid](./uuidnext.uuid.md)

## Fields

### **Max**

The Max UUID is special form of UUID that is specified to have all 128 bits set to 1.

```csharp
public static Guid Max;
```

### **Nil**

The Nil UUID is special form of UUID that is specified to have all 128 bits set to zero.

```csharp
public static Guid Nil;
```

## Methods

### **NewDatabaseFriendly(Database)**

Create a new UUID best suited for the selected database

```csharp
public static Guid NewDatabaseFriendly(Database database)
```

#### Parameters

`database` [Database](./uuidnext.database.md)<br>
The database where the UUID will be stored

#### Returns

[Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid)<br>

### **NewNameBased(Guid, String)**

Create a new UUID Version 5

```csharp
public static Guid NewNameBased(Guid namespaceId, string name)
```

#### Parameters

`namespaceId` [Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid)<br>
the name space ID

`name` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
the name from which to generate the UUID

#### Returns

[Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid)<br>

### **NewSequential()**

Create a new UUID Version 7

```csharp
public static Guid NewSequential()
```

#### Returns

[Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid)<br>

### **NewRandom()**

Create a new UUID Version 4

```csharp
public static Guid NewRandom()
```

#### Returns

[Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid)<br>
