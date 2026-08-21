namespace DapperMappingAndAdvancedPatterns.Utils;

/*
 * FILE ROLE:
 *   Shared LocalDB connection string for AdoNetTutorial (same database as ADO.NET chapters).
 *
 * SECTIONS IN THIS FILE:
 *   1. Connection string constant
 */

/*
 * SECTION 1: CONNECTION STRING CONSTANT
 *
 * Database: AdoNetTutorial on (localdb)\MSSQLLocalDB.
 * Bootstrap in Data/DatabaseBootstrap.cs creates Customers, Orders, and OrderLines
 * alongside the Products table from earlier ADO.NET chapters.
 * -------------------------------------------------------------------------
 */
public static class ConnectionHelper
{
    public const string AdoNetTutorial =
        "Server=(localdb)\\MSSQLLocalDB;Database=AdoNetTutorial;Integrated Security=true;TrustServerCertificate=true";
}
