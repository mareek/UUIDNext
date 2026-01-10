using UUIDNext.Generator;

namespace UUIDNext;

/// <summary>
/// Provite a set of static methods for generating UUIDs
/// </summary>
public static class Uuid
{
    private static readonly UuidV7Generator _v7Generator = new();
    private static readonly UuidV5Generator _v5Generator = new();
    private static readonly UuidV4Generator _v4Generator = new();
    private static readonly UuidV8SqlServerGenerator _v8SqlServerGenerator = new();

    /// <summary>
    /// The Max UUID is special form of UUID that is specified to have all 128 bits set to 1.
    /// </summary>
    public static readonly Guid Max = new("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF");

    /// <summary>
    /// The Nil UUID is special form of UUID that is specified to have all 128 bits set to zero.
    /// </summary>
    public static readonly Guid Nil = new("00000000-0000-0000-0000-000000000000");

    /// <summary>
    /// Create a new UUID best suited for the selected database
    /// </summary>
    /// <param name="database">The database where the UUID will be stored</param>
    /// <returns></returns>
    public static Guid NewDatabaseFriendly(Database database)
        => database switch
        {
            Database.SqlServer => _v8SqlServerGenerator.New(),
            Database.SQLite => _v7Generator.New(),
            Database.PostgreSql => _v7Generator.New(),
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

    /// <summary>
    /// The IANA approved UUID namespaces
    /// see https://www.iana.org/assignments/uuid/uuid.xhtml#table-uuid-namespace-ids
    /// </summary>
    public static class Namespace
    {
        /// <summary>
        /// Domain Name System
        /// </summary>
        public static readonly Guid DNS = new("6ba7b810-9dad-11d1-80b4-00c04fd430c8");

        /// <summary>
        /// Uniform Resource Locator
        /// </summary>
        public static readonly Guid URL = new("6ba7b811-9dad-11d1-80b4-00c04fd430c8");
        
        /// <summary>
        /// ASN.1 Object Identifier
        /// </summary>
        public static readonly Guid OID = new("6ba7b812-9dad-11d1-80b4-00c04fd430c8");
        
        /// <summary>
        /// X.500 Distinguished Name
        /// </summary>
        public static readonly Guid X500 = new("6ba7b814-9dad-11d1-80b4-00c04fd430c8");
        
        /// <summary>
        /// Concise Binary Object Representation - Private Enterprise Number
        /// </summary>
        public static readonly Guid CBOR_PEN = new ("47fbdabb-f2e4-55f0-bb39-3620c2f6df4e");
    }
}
