using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Data.SqlClient;

namespace UUIDNext.Demo;

internal static class SqlServerHelper
{
    private const string databaseName = "uuidNextDemoDb";
    private const string tableName = "#uuidTestTable";
    private const int dataSize = 255;
    private const string serverConnectionString = "Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;";
    private const string databaseConnectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Integrated Security=true;";

    public static async Task CreateDatabase()
    {
        using SqlConnection connection = await OpenServerConnection();
        using var createDbCommand = connection.CreateCommand();
        createDbCommand.CommandText = $"CREATE DATABASE {databaseName};";
        await createDbCommand.ExecuteNonQueryAsync();
        Console.WriteLine("Database created");
    }

    private static async Task<SqlConnection> OpenServerConnection() => await OpenConnection(serverConnectionString);

    public static async Task<SqlConnection> OpenDBConnection() => await OpenConnection(databaseConnectionString);

    private static async Task<SqlConnection> OpenConnection(string connectionString)
    {
        SqlConnection connection = new(connectionString);
        await connection.OpenAsync();
        return connection;
    }

    public static async Task CreateTable()
    {
        using var connection = await OpenDBConnection();
        using var createTableCommand = connection.CreateCommand();
        createTableCommand.CommandText = $"""
            CREATE TABLE {tableName} (uuid uniqueIdentifier not null primary key, 
                                      date datetime2 not null, 
                                      data varchar({dataSize}));
            """;
        await createTableCommand.ExecuteNonQueryAsync();
        Console.WriteLine("Table created");
    }

    public static async Task<TimeSpan> InsertLine(SqlConnection connection, Guid id, string data)
    {
        const string idParamName = "@uuid";
        const string dateParamName = "@date";
        const string dataParamName = "@data";

        using var insertCommand = connection.CreateCommand();
        insertCommand.CommandText = $"INSERT INTO {tableName} VALUES ({idParamName}, {dateParamName}, {dataParamName})";

        var idParam = insertCommand.CreateParameter();
        idParam.ParameterName = idParamName;
        idParam.Value = id;
        insertCommand.Parameters.Add(idParam);

        var dateParam = insertCommand.CreateParameter();
        dateParam.ParameterName = dateParamName;
        dateParam.Value = DateTime.UtcNow;
        insertCommand.Parameters.Add(dateParam);

        var dataParam = insertCommand.CreateParameter();
        dataParam.ParameterName = dataParamName;
        dataParam.Value = data;
        insertCommand.Parameters.Add(dataParam);

        var chrono = Stopwatch.StartNew();
        await insertCommand.ExecuteNonQueryAsync();
        chrono.Stop();

        return chrono.Elapsed;
    }

    public static async Task DropDatabase()
    {
        using SqlConnection connection = await OpenServerConnection();
        using var dropCommand = connection.CreateCommand();
        dropCommand.CommandText = $"DROP DATABASE {databaseName};";
        await dropCommand.ExecuteNonQueryAsync();
        Console.WriteLine("Database dropped");
    }

    public static string GenerateData()
    {
        const string LoremIpsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
        var loremStart = Random.Shared.Next(LoremIpsum.Length);
        var loremLength = int.Min(dataSize, LoremIpsum.Length - loremStart);
        return LoremIpsum.Substring(loremStart, loremLength);
    }
}
