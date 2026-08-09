namespace RelationshipsAndNavigationProperties.Utils;

/*
 * FILE ROLE:
 *   Centralizes the EfCoreTutorial LocalDB connection string for this chapter.
 *
 * SECTIONS IN THIS FILE:
 *   1. Connection string constant
 */

/*
 * SECTION 1: CONNECTION STRING CONSTANT
 *
 * Same EfCoreTutorial database used across EF Core chapters (see ch02 ConnectionOptions).
 * Relationship demo tables (Customers, Orders, CatalogProducts, Tags, ProductTags) coexist
 * with tables created by earlier chapters; CatalogProducts avoids colliding with dbo.Products
 * from ch02 DbContext & DbSet when both tutorials share one LocalDB instance.
 *
 *   Server=(localdb)\MSSQLLocalDB
 *   Database=EfCoreTutorial
 *   Integrated Security=true
 *   TrustServerCertificate=true
 * -------------------------------------------------------------------------
 */
public static class ConnectionHelper
{
    public const string LocalDbConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=EfCoreTutorial;Integrated Security=true;TrustServerCertificate=true";
}
