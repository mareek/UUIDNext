using System;
using UUIDNext.Generator;

namespace UUIDNext
{
    public static class Uuid
    {
        private static readonly UuidV7Generator _v7Generator = new();
        private static readonly UuidV5Generator _v5Generator = new();
        private static readonly UuidV4Generator _v4Generator = new();
        private static readonly UuidV8SqlServerGenerator _v8SqlServerGenerator = new();

        /// <summary>
        /// A read-only instance of the System.Guid structure whose value is all ones (FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF).
        /// </summary>
        public static readonly Guid Max = new("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF");

        /// <summary>
        /// Create a new UUID Version 7
        /// </summary>
        [Obsolete("You should use the overload that specifies the database used. "
                + "Every database has its way of storing UUID and UUID V7 might not be suited for your database.")]
        public static Guid NewDatabaseFriendly() => _v7Generator.New();

        /// <summary>
        /// Create a new UUID best suited for the selected database
        /// </summary>
        /// <param name="database">The database where the UUID will be stored</param>
        /// <returns></returns>
        public static Guid NewDatabaseFriendly(Database database)
            => database switch
            {
                Database.SqlServer => _v8SqlServerGenerator.New(),
                Database.SQLite=> _v7Generator.New(),
                _ => _v7Generator.New(),
            };

        /// <summary>
        /// Create a new UUID Version 5
        /// </summary>
        /// <param name="namespaceId">the name space ID</param>
        /// <param name="name">the name from which to generate the UUID</param>
        public static Guid NewNameBased(Guid namespaceId, string name) => _v5Generator.New(namespaceId, name);

        /// <summary>
        /// Create a new UUID Version 4
        /// </summary>
        public static Guid NewRandom() => _v4Generator.New();

        /// <summary>
        /// Create a new UUID Version 7
        /// </summary>
        public static Guid NewSequential() => _v7Generator.New();
    }
}
