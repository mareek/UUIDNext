using System;
using System.Collections.Generic;
using System.Linq;

namespace UUIDNext.Test.DatabaseSupport
{
    internal static class DatabaseTestHelper
    {
        public static string GenerateTestSetSqlInsertionScript(string tableName, IEnumerable<(int expectedPosition, Guid uuid)> dataToInsert)
            => string.Join("\n\n",
                           GenerateTableCreationQuery(tableName, "int", "uniqueidentifier"),
                           GenerateSqlInsertionQuery(tableName, dataToInsert),
                           GenerateSelectQuery(tableName),
                           GenerateTableDropQuery(tableName));

        public static string GenerateTableCreationQuery(string tableName, string positionType, string uuidType)
            => @$"CREATE TABLE {tableName} (
    ExpectedPosition {positionType},
    UUID {uuidType}
);";

        public static string GenerateSqlInsertionQuery(string tableName, IEnumerable<(int expectedPosition, Guid uuid)> dataToInsert)
            => GenerateSqlInsertionQuery(tableName, dataToInsert.Select(d => $"{d.expectedPosition}, '{d.uuid}'"));

        public static string GenerateSqlInsertionQuery(string tableName, IEnumerable<string> valuesToInsert)
            => @$"INSERT INTO {tableName}
VALUES
{string.Join(",\n", valuesToInsert.Select(v => $"({v})"))};";

        public static string GenerateSelectQuery(string tableName)
            => @$"SELECT * FROM {tableName} ORDER BY UUID;";

        public static string GenerateTableDropQuery(string tableName)
            => @$"DROP TABLE {tableName};";


    }
}
