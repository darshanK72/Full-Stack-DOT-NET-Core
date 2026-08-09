namespace DatabaseFirstAndReverseEngineering.Utils;

/*
 * FILE ROLE:
 *   Shared LocalDB connection string for the EfScaffoldTutorial sample database.
 *
 * SECTIONS IN THIS FILE:
 *   1. Connection string constant
 */

/*
 * SECTION 1: CONNECTION STRING CONSTANT
 *
 * Database: EfScaffoldTutorial on (localdb)\MSSQLLocalDB.
 * InventoryBootstrap in Data/InventoryBootstrap.cs creates dbo.Categories and dbo.Products
 * plus an audit.ChangeLog table (excluded from scaffold demos via --schema filtering).
 * -------------------------------------------------------------------------
 */
public static class ConnectionHelper
{
    public const string EfScaffoldTutorial =
        "Server=(localdb)\\MSSQLLocalDB;Database=EfScaffoldTutorial;Integrated Security=true;TrustServerCertificate=true";
}
