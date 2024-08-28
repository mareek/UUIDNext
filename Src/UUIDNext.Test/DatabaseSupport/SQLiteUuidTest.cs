using System;
using System.Linq;
using Microsoft.Data.Sqlite;
using NFluent;
using UUIDNext.Generator;
using Xunit;

namespace UUIDNext.Test.DatabaseSupport
{
    public class SQLiteUuidTest
    {
        const string tableName = "uuidTestTable";

        private readonly bool _sqliteIsAvailable;

        public SQLiteUuidTest()
        {
            try
            {
                SQLitePCL.Batteries.Init();
                _sqliteIsAvailable = true;
            }
            catch (DllNotFoundException)
            {
                // On my machine, these tests fails when run for .NET framework 4.x with the following exception : "Unable to load DLL 'sqlite3': The specified module could not be found."
                // As these test work well on .net 6+, I will simply ignore these tests when there is a dll loading error
                _sqliteIsAvailable = false;
            }
        }

        [Fact]
        public void TestSQLiteWithString()
        {
            if (!_sqliteIsAvailable)
                return; 

            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                ExecuteNonQuery(connection, DatabaseTestHelper.GenerateTableCreationQuery(tableName, "INTEGER", "TEXT"));

                try
                {
                    var generator = new UuidV7Generator();
                    ExecuteNonQuery(connection, DatabaseTestHelper.GenerateSqlInsertionQuery(tableName, generator.GetDatabaseTestSet(100)));

                    var command = connection.CreateCommand();
                    command.CommandText = @$"SELECT * FROM {tableName} ORDER BY UUID;";

                    using var reader = command.ExecuteReader();
                    int previousOrder = int.MinValue;
                    while (reader.Read())
                    {
                        var expectedOrder = reader.GetInt32(0);
                        Check.That(expectedOrder).IsStrictlyGreaterThan(previousOrder);
                    }
                }
                finally
                {
                    ExecuteNonQuery(connection, DatabaseTestHelper.GenerateTableDropQuery(tableName));
                }
            }
        }

        [Fact]
        public void TestSQLiteWithBlob()
        {
            if (!_sqliteIsAvailable)
                return;

            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                ExecuteNonQuery(connection, DatabaseTestHelper.GenerateTableCreationQuery(tableName, "INTEGER", "BLOB"));

                try
                {
                    var generator = new UuidV7Generator();

                    var insertCommand = connection.CreateCommand();
                    InitInsertCommand(insertCommand, generator.GetDatabaseTestSet(10).ToArray());
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
                    ExecuteNonQuery(connection, DatabaseTestHelper.GenerateTableDropQuery(tableName));
                }
            }
        }

        private static void InitInsertCommand(SqliteCommand insertCommand, (int expectedPosition, Guid uuid)[] dataToInsert)
        {
            insertCommand.CommandText = DatabaseTestHelper.GenerateSqlInsertionQuery(tableName, dataToInsert.Select(d => $"{d.expectedPosition}, $uuid{d.expectedPosition}"));
            foreach (var (expectedPosition, uuid) in dataToInsert)
            {
                insertCommand.Parameters.AddWithValue($"$uuid{expectedPosition}", uuid.ToByteArray());
            }
        }

        private static void ExecuteNonQuery(SqliteConnection connection, string query)
        {
            var command = connection.CreateCommand();
            command.CommandText = query;
            command.ExecuteNonQuery();
        }
    }
}
