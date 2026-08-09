namespace RawSqlAndStoredProcedures.Utils;

/*
 * FILE ROLE: Shared LocalDB connection string for the EfCoreTutorial database used by every
 *            demo in this chapter.
 *
 * SECTIONS IN THIS FILE:
 *   1. Connection string — EfCoreTutorial on LocalDB
 */

/*
 * SECTION 1: CONNECTION STRING — EfCoreTutorial ON LOCALDB
 *
 * Same server instance as ADO.NET/Dapper chapters, but a separate database name so EF
 * bootstrap (tables + stored procedures) does not collide with AdoNetTutorial.
 *
 *   Server=(localdb)\MSSQLLocalDB;Database=EfCoreTutorial;Integrated Security=true;TrustServerCertificate=true
 *
 * COVERED IN ADO.NET ch.02 → SqlConnection & Connection Strings (keywords and LocalDB).
 */
public static class ConnectionHelper
{
    public const string Server = @"(localdb)\MSSQLLocalDB";
    public const string DatabaseName = "EfCoreTutorial";

    public static string ConnectionString { get; } =
        $"Server={Server};Database={DatabaseName};Integrated Security=true;TrustServerCertificate=true;";

    public static string MasterConnectionString { get; } =
        $"Server={Server};Database=master;Integrated Security=true;TrustServerCertificate=true;";
}
