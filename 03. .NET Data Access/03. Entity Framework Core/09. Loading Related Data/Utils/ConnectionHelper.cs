namespace LoadingRelatedData.Utils;

/*
 * FILE ROLE:
 *   Shared LocalDB connection string for AdoNetTutorial (same database as ADO.NET / Dapper chapters).
 *
 * SECTIONS IN THIS FILE:
 *   1. Connection string constant
 */

/*
 * SECTION 1: CONNECTION STRING CONSTANT
 *
 * Database: AdoNetTutorial on (localdb)\MSSQLLocalDB.
 * Data/DatabaseBootstrap.cs ensures Customers, Orders, OrderLines, and Products exist
 * before EF Core demos run.
 * -------------------------------------------------------------------------
 */
public static class ConnectionHelper
{
    public const string AdoNetTutorial =
        "Server=(localdb)\\MSSQLLocalDB;Database=AdoNetTutorial;Integrated Security=true;TrustServerCertificate=true";
}
