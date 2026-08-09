using System;
using Microsoft.Data.SqlClient;

namespace SqlConnectionAndConnectionStrings.Utils;

/*
 * FILE ROLE:
 *   Build and parse connection strings with SqlConnectionStringBuilder and
 *   document the common key reference table.
 *
 * SECTIONS IN THIS FILE:
 *   2. SqlConnectionStringBuilder — build strings in code
 *   3. Connection string key reference table
 */

/*
 * =========================================================================
 * SECTION 2: SqlConnectionStringBuilder — BUILD STRINGS IN CODE
 * =========================================================================
 *
 * SqlConnectionStringBuilder wraps the same key=value format with typed properties.
 * Benefits:
 *   • IntelliSense for property names (fewer silent typos than raw strings)
 *   • ConnectionString property regenerates a valid string after you set fields
 *   • Indexer syntax builder["Key"] for less common keys
 *
 * Always assign builder.ConnectionString (or ToString()) to SqlConnection —
 * do not pass the builder object itself to the constructor.
 * -------------------------------------------------------------------------
 */
public static class DemoConnectionBuilder
{
    public static string BuildLocalDbString(
        string server,
        string database,
        int connectionTimeoutSeconds = 15,
        bool multipleActiveResultSets = false)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = server,                              // same as Server key
            InitialCatalog = database,                        // same as Database key
            IntegratedSecurity = true,                        // Windows authentication
            TrustServerCertificate = true,                  // dev LocalDB — not for production
            Encrypt = true,                                   // SqlClient 5.x default; explicit here
            ConnectTimeout = connectionTimeoutSeconds,        // open timeout in seconds
            MultipleActiveResultSets = multipleActiveResultSets
        };

        return builder.ConnectionString;
    }

    public static void PrintBuilderBreakdown(string connectionString)
    {
        var parsed = new SqlConnectionStringBuilder(connectionString);
        Console.WriteLine("  Parsed SqlConnectionStringBuilder:");
        Console.WriteLine($"    DataSource (Server)     = {parsed.DataSource}");
        Console.WriteLine($"    InitialCatalog (Database) = {parsed.InitialCatalog}");
        Console.WriteLine($"    IntegratedSecurity    = {parsed.IntegratedSecurity}");
        Console.WriteLine($"    Encrypt               = {parsed.Encrypt}");
        Console.WriteLine($"    TrustServerCertificate= {parsed.TrustServerCertificate}");
        Console.WriteLine($"    ConnectTimeout        = {parsed.ConnectTimeout}s");
        Console.WriteLine($"    MultipleActiveResultSets = {parsed.MultipleActiveResultSets}");
    }
}

/*
 * =========================================================================
 * SECTION 3: CONNECTION STRING KEY REFERENCE
 * =========================================================================
 *
 *  Key (alias)              | Purpose
 *  -------------------------|--------------------------------------------------
 *  Server (Data Source)     | Hostname, IP, or (localdb)\InstanceName
 *  Database (Initial Catalog)| Database name used after authentication
 *  Integrated Security      | true / SSPI — Windows auth; omit User ID/Password
 *  User ID (UID)            | SQL login name when not using Integrated Security
 *  Password (PWD)           | SQL login password — never commit to source control
 *  Encrypt                  | true (default) — TLS to server; required for Azure
 *  TrustServerCertificate   | true skips server cert chain validation (dev only)
 *  Connection Timeout       | Seconds waiting for Open(); default 15
 *  MultipleActiveResultSets | true enables MARS (multiple active readers)
 *
 * SqlConnectionStringBuilder maps each row to a property (see Section 2 demo).
 * -------------------------------------------------------------------------
 */
