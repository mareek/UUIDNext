namespace UUIDNext;

/// <summary>
/// The Database where the UUID will be stored
/// </summary>
public enum Database
{
    /// <summary>
    /// Any other Database (MySQL, Oracle, MongoDB, etc.)
    /// </summary>
    Other = 0,
    /// <summary>
    /// Microsoft SQL Server (uniqueidentifier Type)
    /// </summary>
    SqlServer,
    /// <summary>
    /// SQLite (BLOB or TEXT Type)
    /// </summary>
    SQLite,
    /// <summary>
    /// PostgreSQL (UUID Type)
    /// </summary>
    PostgreSql
}
