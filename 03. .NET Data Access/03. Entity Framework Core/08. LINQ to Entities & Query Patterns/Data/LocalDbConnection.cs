namespace LinqToEntitiesAndQueryPatterns.Data;

/*
 * FILE ROLE:
 *   Shared LocalDB connection string for the EfCoreLinqDemo database used in this chapter.
 *
 * SECTIONS IN THIS FILE:
 *   1. Connection string constant
 */

/*
 * SECTION 1: CONNECTION STRING CONSTANT
 *
 * Server: (localdb)\MSSQLLocalDB
 * Database: EfCoreLinqDemo (created on first run via EnsureCreated in Program.cs Main)
 *
 * Same LocalDB instance as ADO.NET/Dapper AdoNetTutorial chapters; separate database
 * so EF Core can own schema and HasData seed without conflicting with raw SQL DDL.
 * -------------------------------------------------------------------------
 */
public static class LocalDbConnection
{
    public const string EfCoreLinqDemo =
        "Server=(localdb)\\MSSQLLocalDB;Database=EfCoreLinqDemo;Integrated Security=true;TrustServerCertificate=true";
}
