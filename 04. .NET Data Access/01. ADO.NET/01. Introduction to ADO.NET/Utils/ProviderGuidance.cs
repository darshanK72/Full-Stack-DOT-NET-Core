using System.Collections.Generic;

namespace IntroductionToAdoNet.Utils;

/*
 * FILE ROLE: SQL Server provider package guidance (Section 8).
 * SECTIONS IN THIS FILE:
 *   8. Provider model — Microsoft.Data.SqlClient vs System.Data.SqlClient
 */

/*
 * =========================================================================
 * SECTION 8: PROVIDER MODEL — Microsoft.Data.SqlClient VS System.Data.SqlClient
 * =========================================================================
 *
 * SQL Server has two similarly named packages. New .NET projects should use
 * Microsoft.Data.SqlClient exclusively.
 *
 *   Package / assembly              | Status in modern .NET
 *   --------------------------------|--------------------------------------------------
 *   Microsoft.Data.SqlClient        | Active; cross-platform; Azure AD, Always Encrypted,
 *                                   | TDS improvements, regular servicing
 *   System.Data.SqlClient           | Compatibility shim; same public type names but frozen;
 *                                   | do not start new work on this package
 *
 * Other databases ship their own ADO.NET providers inheriting the same
 * DbConnection/DbCommand base types:
 *
 *   Database        | Provider package (examples)
 *   ----------------|---------------------------------
 *   PostgreSQL      | Npgsql
 *   MySQL           | MySqlConnector
 *   SQLite          | Microsoft.Data.Sqlite
 *   Oracle          | Oracle.ManagedDataAccess.Core
 *
 * Factory pattern (optional): DbProviderFactories.GetFactory("Microsoft.Data.SqlClient")
 * resolves the provider at runtime — useful for plugin architectures.
 *
 * ch.02 adds Microsoft.Data.SqlClient to this project's .csproj.
 * -------------------------------------------------------------------------
 */
public static class ProviderGuidance
{
    public static IReadOnlyList<string> SqlClientComparison()
    {
        return new[]
        {
            "USE  → Microsoft.Data.SqlClient  (NuGet; active development)",
            "AVOID → System.Data.SqlClient    (legacy; compatibility only)",
        };
    }

    public static IReadOnlyList<string> OtherProviders()
    {
        return new[]
        {
            "PostgreSQL → Npgsql.NpgsqlConnection",
            "SQLite     → Microsoft.Data.Sqlite.SqliteConnection",
            "MySQL      → MySqlConnector.MySqlConnection",
        };
    }
}
