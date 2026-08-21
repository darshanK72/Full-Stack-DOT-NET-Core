namespace CrudOperationsAndSaveChanges.Utils;

/*
 * FILE ROLE: Centralizes the EfCoreTutorial LocalDB connection string used by
 *            DbContext configuration and the runtime bootstrap in this chapter.
 *
 * SECTIONS IN THIS FILE:
 *   (connection string constant — referenced by Data/AppDbContext and bootstrap)
 */

public static class ConnectionHelper
{
    public const string LocalDbConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=EfCoreTutorial;Integrated Security=true;TrustServerCertificate=true";

    public const string MasterConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=master;Integrated Security=true;TrustServerCertificate=true";
}
