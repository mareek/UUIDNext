using System.Data.Common;

/* SQLite configuration 
 */
const string dbmsName = "SQLite";         // Name of the DBMS being tested
const bool manageDatabase = false;        // Tells the program if it should create a new database or not
const string databaseName = "";           // Name of the temporary database created for the test (if manageDatabase is set to true)
const string tableName = "uuidTestTable"; // Name of the temporary table created for the tests
const string positionType = "INTEGER";    // Type of the column where the position in the array is stored
const string uuidType = "BLOB";           // Type of the column where the UUID wiil be stored

var connectionString = "Data Source=:memory:";
using DbConnection connection = new Microsoft.Data.Sqlite.SqliteConnection(connectionString);

/* SQL Server configuration
const string dbmsName = "MS SQL Server";    // Name of the DBMS being tested
const bool manageDatabase = true;           // Tells the program if it should create a new database or not
const string databaseName = "uuidTestDb";   // Name of the temporary database created for the test (if manageDatabase is set to true)
const string tableName = "#uuidTestTable";  // Name of the temporary table created for the tests
const string positionType = "int";          // Type of the column where the position in the array is stored
const string uuidType = "uniqueIdentifier"; // Type of the column where the UUID wiil be stored


var connectionString = "Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;";
using DbConnection connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
 */

try
{
    connection.Open();
}
catch 
{
    Console.Error.WriteLine($"Unable to connecto to {dbmsName} database \"{connectionString}\"");
    return;
}

try
{
    if (manageDatabase)
    {
        using var createDbCommand = connection.CreateCommand();
        createDbCommand.CommandText = $"CREATE DATABASE {databaseName};";
        createDbCommand.ExecuteNonQuery();
    }

    using var createTableCommand = connection.CreateCommand();
    createTableCommand.CommandText = $"CREATE TABLE {tableName} ( position {positionType}, uuid {uuidType} );";
    createTableCommand.ExecuteNonQuery();

    Guid[] orderedUuids =
    {
        Guid.Parse("00000000-0000-0000-0000-000000000001"),
        Guid.Parse("00000000-0000-0000-0000-000000000100"),
        Guid.Parse("00000000-0000-0000-0000-000000010000"),
        Guid.Parse("00000000-0000-0000-0000-000001000000"),
        Guid.Parse("00000000-0000-0000-0000-000100000000"),
        Guid.Parse("00000000-0000-0000-0000-010000000000"),
        Guid.Parse("00000000-0000-0000-0001-000000000000"),
        Guid.Parse("00000000-0000-0000-0100-000000000000"),
        Guid.Parse("00000000-0000-0001-0000-000000000000"),
        Guid.Parse("00000000-0000-0100-0000-000000000000"),
        Guid.Parse("00000000-0001-0000-0000-000000000000"),
        Guid.Parse("00000000-0100-0000-0000-000000000000"),
        Guid.Parse("00000001-0000-0000-0000-000000000000"),
        Guid.Parse("00000100-0000-0000-0000-000000000000"),
        Guid.Parse("00010000-0000-0000-0000-000000000000"),
        Guid.Parse("01000000-0000-0000-0000-000000000000")
    };


    for (int i = 0; i < orderedUuids.Length; i++)
    {
        const string paramName = "@uuid";

        using var insertCommand = connection.CreateCommand();
        insertCommand.CommandText = $"INSERT INTO {tableName} VALUES ({i}, {paramName})";

        var parameter = insertCommand.CreateParameter();
        parameter.ParameterName = paramName;
        parameter.Value = orderedUuids[i];
        insertCommand.Parameters.Add(parameter);

        insertCommand.ExecuteNonQuery();
    }

    using var selectCommand = connection.CreateCommand();
    selectCommand.CommandText = @$"SELECT position, uuid FROM {tableName} ORDER BY UUID;";

    Console.WriteLine($"UUID Order for {dbmsName} :");
    Console.WriteLine("___________________________________________");
    Console.WriteLine("|Pos.|                UUID                |");
    Console.WriteLine("|----|------------------------------------|");

    using var reader = selectCommand.ExecuteReader();
    while (reader.Read())
    {
        var position = reader.GetInt32(0);
        var uuid = reader.GetGuid(1);

        Console.WriteLine($"| {position:00} |{uuid}|");
    }
}
finally
{
    using var dropCommand = connection.CreateCommand();
    if (manageDatabase)
    {
        dropCommand.CommandText = $"DROP DATABASE {databaseName};";
    }
    else
    {
        dropCommand.CommandText = $"DROP TABLE {tableName};";
    }

    dropCommand.ExecuteNonQuery();
}
