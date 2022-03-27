using System;
using UUIDNext.Generator;

namespace UUIDNext
{
    public static class Uuid
    {
        private static readonly UuidV7Generator _v7Generator = new();
        private static readonly UuidV5Generator _v5Generator = new();
        private static readonly UuidV4Generator _v4Generator = new();

        /// <summary>
        /// A read-only instance of the System.Guid structure whose value is all ones (FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF).
        /// </summary>
        public static readonly Guid Max = new("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF");

        /// <summary>
        /// Create a new UUID Version 7
        /// </summary>
        public static Guid NewDatabaseFriendly() => _v7Generator.New();

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
