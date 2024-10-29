using System.Text;
using UUIDNext;
using UUIDNext.Tools;

const string Doc = """
        Description : 
            Generate a new UUID
        
        Usage : 
            uuidnext command [options]
        
        Commands : 
            Random            Create a new UUID v4.
            Sequential        Create a new UUID v7
            Database [dbName] Create a UUID to be used as a database primary key (v7 or v8 depending on the database)
                              dbName can be "PostgreSQL", "SqlServer", "SQLite", "Other" or empty
            Decode   [UUID]   Decode the versioo of the UUID and optionally the timestamp an sequence number of UUID v1, 6, 7 and 8
        """;


if (args.Length == 0)
    Console.WriteLine(Doc);
else
{
    var command = args[0].ToLowerInvariant();
    switch (command)
    {
        case "random":
            Console.WriteLine($"{Uuid.NewRandom()}");
            break;
        case "sequential":
            Console.WriteLine($"{Uuid.NewSequential()}");
            break;
        case "database":
            OutputDatabaseUuid(args.ElementAtOrDefault(1));
            break;
        case "decode":
            OutputDecode(args.ElementAtOrDefault(1));
            break;
        default:
            Console.WriteLine($"Unkown command [{command}]");
            Console.WriteLine();
            Console.WriteLine(Doc);
            break;
    }
}

static void OutputDatabaseUuid(string? dbName)
{
    Database db;
    if (string.IsNullOrWhiteSpace(dbName))
        db = Database.Other;
    else if (!Enum.TryParse<Database>(dbName, ignoreCase: true, result: out db))
    {
        Console.WriteLine($"Unkown database [{dbName}]");
        return;
    }

    Console.WriteLine($"{Uuid.NewDatabaseFriendly(db)}");
}

static void OutputDecode(string? strUuid)
{
    if (string.IsNullOrWhiteSpace(strUuid) || !Guid.TryParse(strUuid, out var parsedUuid))
    {
        Console.WriteLine($"The string [{strUuid}] is not a valid UUID");
        return;
    }

    StringBuilder resultBuilder = new();
    resultBuilder.Append("{ ");
    
    resultBuilder.Append($"Version: {UuidDecoder.GetVersion(parsedUuid)}");

    if (UuidDecoder.TryDecodeTimestamp(parsedUuid, out var date))
        resultBuilder.Append($", Timestamp: \"{date:O}\"");

    if (UuidDecoder.TryDecodeSequence(parsedUuid, out var sequence))
        resultBuilder.Append($", Sequence: {sequence}");

    resultBuilder.Append(" }");

    Console.WriteLine(resultBuilder.ToString());
}
