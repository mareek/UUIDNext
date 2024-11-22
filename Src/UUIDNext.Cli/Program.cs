using System.Reflection;
using System.Text;
using UUIDNext;
using UUIDNext.Tools;

const string Doc = """
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
        """;

bool outputToClipboard = args.LastOrDefault() == "--clipboard";
args = outputToClipboard ? args[..^1] : args;
if (args.Length == 0)
    Console.WriteLine(Doc);
else
{
    var command = args[0].ToLowerInvariant();
    var option = args.ElementAtOrDefault(1);
    string output;
    switch (command)
    {
        case "random":
            output = $"{Uuid.NewRandom()}";
            break;
        case "sequential":
            output = $"{Uuid.NewSequential()}";
            break;
        case "database":
            output = OutputDatabaseUuid(option);
            break;
        case "decode":
            output = OutputDecode(option);
            break;
        case "version":
            var assembly = Assembly.GetExecutingAssembly()!;
            var versionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!;
            // This InformationalVersion contains the content of the <version> element in the csproj + the commit id
            // See https://learn.microsoft.com/en-us/dotnet/api/system.reflection.assemblyinformationalversionattribute#remarks
            var versionLabel = versionAttribute.InformationalVersion.Split("+")[0];
            output = versionLabel;
            break;
        default:
            Console.WriteLine($"Unkown command [{command}]");
            Console.WriteLine();
            Console.WriteLine(Doc);
            return;
    }

    if (outputToClipboard)
        TextCopy.ClipboardService.SetText(output);
    else
        Console.WriteLine(output);
}

static string OutputDatabaseUuid(string? dbName)
{
    if (!Enum.TryParse(dbName, ignoreCase: true, result: out Database db))
    {
        var expectedValues = Enum.GetValues<Database>().ToList();
        // Ensure that Database.Other is the last of the list
        expectedValues.Remove(Database.Other);
        expectedValues.Add(Database.Other);
        return $"Unkown dbName [{dbName}]. Expected dbName values are [{string.Join(", ", expectedValues)}]";
    }

    return $"{Uuid.NewDatabaseFriendly(db)}";
}

static string OutputDecode(string? strUuid)
{
    strUuid ??= Console.ReadLine();
    if (string.IsNullOrWhiteSpace(strUuid) || !Guid.TryParse(strUuid, out var uuid))
    {
        return $"The string [{strUuid}] is not a valid UUID";
    }

    StringBuilder resultBuilder = new();
    resultBuilder.Append("{ ");

    resultBuilder.Append($"Version: {UuidDecoder.GetVersion(uuid)}");

    if (UuidDecoder.TryDecodeTimestamp(uuid, out var date))
        resultBuilder.Append($", Timestamp: \"{date:O}\"");

    if (UuidDecoder.TryDecodeSequence(uuid, out var sequence))
        resultBuilder.Append($", Sequence: {sequence}");

    resultBuilder.Append(" }");

    return resultBuilder.ToString();
}
