using System;

namespace IntroductionToDapper.Utils;

/*
 * FILE ROLE: Centralizes the AdoNetTutorial LocalDB connection string shared by
 *            every demo in this chapter (same database as ADO.NET modules).
 *
 * SECTIONS IN THIS FILE:
 *   (connection string constant — referenced by repository and connection demos)
 */

public static class ConnectionHelper
{
    public const string LocalDbConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=AdoNetTutorial;Integrated Security=true;TrustServerCertificate=true";
}
