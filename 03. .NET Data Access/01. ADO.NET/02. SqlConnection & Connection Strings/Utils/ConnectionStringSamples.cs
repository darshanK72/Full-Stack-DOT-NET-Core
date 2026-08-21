using System;

namespace SqlConnectionAndConnectionStrings.Utils;

/*
 * FILE ROLE:
 *   Connection string literals and key=value pair basics for LocalDB, Express,
 *   and SQL authentication variants (comments only for non-default servers).
 *
 * SECTIONS IN THIS FILE:
 *   1. Connection string basics — key=value pairs and sample constants
 */

/*
 * =========================================================================
 * SECTION 1: CONNECTION STRING BASICS — KEY=VALUE PAIRS
 * =========================================================================
 *
 * A connection string is one semicolon-delimited list of settings the provider
 * uses to locate SQL Server and authenticate:
 *
 *   Server=(localdb)\MSSQLLocalDB;Database=AdoNetTutorial;Integrated Security=true;
 *
 * Keys are case-insensitive. Values with spaces or special characters may need
 * quoting. Typos in key names are silently ignored — always verify with Open().
 *
 * Common keys taught in this chapter (full table in Section 3):
 *
 *   Server / Data Source     host\instance or (localdb)\MSSQLLocalDB
 *   Database / Initial Catalog   catalog name after login
 *   Integrated Security      Windows auth (true / SSPI)
 *   User ID / Password       SQL authentication (avoid in source code)
 *   Encrypt                  encrypt traffic (default true in SqlClient 5.x)
 *   TrustServerCertificate   skip cert validation — dev only
 *   Connection Timeout       seconds to wait while opening (default 15)
 *   MultipleActiveResultSets MARS — multiple readers on one connection
 *
 * ch.01 Introduction to ADO.NET covers the ADO.NET stack and providers.
 * -------------------------------------------------------------------------
 */
public static class ConnectionStringSamples
{
    /*
     * Raw literal — fine for tutorials; production apps read from configuration
     * (Section 8 in Utils/ConnectionSecurityPreview.cs). Integrated Security uses
     * the Windows account running the app.
     */
    public const string LocalDbIntegratedSecurity =
        "Server=(localdb)\\MSSQLLocalDB;Database=AdoNetTutorial;Integrated Security=true;TrustServerCertificate=true";

    /*
     * --- LocalDB variants (comments only — pick one Server value) ---
     *
     *   Server=(localdb)\MSSQLLocalDB
     *   Server=(localdb)\ProjectsV13          -- VS 2015+ default instance name
     *   Server=(localdb)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|AdoNetTutorial.mdf
     *
     * --- SQL Express (installed as a Windows service) ---
     *
     *   Server=.\SQLEXPRESS
     *   Server=localhost\SQLEXPRESS;Database=AdoNetTutorial;Integrated Security=true
     *
     * --- SQL authentication (lab / cloud — store credentials in secrets, not code) ---
     *
     *   Server=localhost;Database=AdoNetTutorial;User ID=app_user;Password=***;Encrypt=true
     */
}
