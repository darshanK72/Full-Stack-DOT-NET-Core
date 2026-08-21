namespace DataSetDataTableAndSqlDataAdapter;

/*
 * FILE ROLE: Shared BikeStores LocalDB connection string for optional SqlDataAdapter demos.
 *
 * SECTIONS IN THIS FILE:
 *   (supporting constant — used by sections 8 and 9 in Services/)
 */

public static class LocalDbConnection
{
    public const string BikeStoresConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=BikeStores;Trusted_Connection=True;TrustServerCertificate=True;";
}
