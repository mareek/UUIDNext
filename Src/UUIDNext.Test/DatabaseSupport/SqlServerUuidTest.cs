using System;
using System.Linq;
using Microsoft.Data.SqlClient;
using NFluent;
using UUIDNext.Generator;
using Xunit;

namespace UUIDNext.Test.DatabaseSupport
{
    public class SqlServerUuidTest
    {
        // Tests for different target frameworks are executed in parallel so each target framework must
        // have a different dayabase name to prevent failing test due to concurrency errors
#if NET472
        const string databaseName = "uuidTestDb_472";
#elif NET6_0
        const string databaseName = "uuidTestDb_6";
#else
        const string databaseName = "uuidTestDb_8";
#endif

        const string tableName = "#uuidTestTable";

        [Fact]
        public void TestSQLServerOnLocalDb()
        {
            using (var connection = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;"))
            {
                try
                {
                    connection.Open();
                }
                catch
                {
                    // localdb is not available on this computer
                    return;
                }

                try
                {
                    ExecuteNonQuery(connection, $"CREATE DATABASE {databaseName};");

                    ExecuteNonQuery(connection, DatabaseTestHelper.GenerateTableCreationQuery(tableName, "int", "uniqueIdentifier"));

                    var generator = new UuidV8SqlServerGenerator();
                    var insertCommand = connection.CreateCommand();
                    InitInsertCommand(insertCommand, UuidTestHelper.GetDatabaseTestSet(generator, 10).ToArray());
                    insertCommand.ExecuteNonQuery();

                    var selectCommand = connection.CreateCommand();
                    selectCommand.CommandText = @$"SELECT * FROM {tableName} ORDER BY UUID;";

                    using var reader = selectCommand.ExecuteReader();
                    int previousOrder = int.MinValue;
                    while (reader.Read())
                    {
                        var expectedOrder = reader.GetInt32(0);
                        Check.That(expectedOrder).IsStrictlyGreaterThan(previousOrder);
                    }
                }
                finally
                {
                    ExecuteNonQuery(connection, $"DROP DATABASE {databaseName};");
                }
            }
        }

        private static void InitInsertCommand(SqlCommand insertCommand, (int expectedPosition, Guid uuid)[] dataToInsert)
        {
            insertCommand.CommandText = DatabaseTestHelper.GenerateSqlInsertionQuery(tableName, dataToInsert.Select(d => $"{d.expectedPosition}, @uuid{d.expectedPosition}"));
            foreach (var (expectedPosition, uuid) in dataToInsert)
            {
                insertCommand.Parameters.AddWithValue($"@uuid{expectedPosition}", uuid);
            }
        }

        private static void ExecuteNonQuery(SqlConnection connection, string query)
        {
            var command = connection.CreateCommand();
            command.CommandText = query;
            command.ExecuteNonQuery();
        }

    }
}
