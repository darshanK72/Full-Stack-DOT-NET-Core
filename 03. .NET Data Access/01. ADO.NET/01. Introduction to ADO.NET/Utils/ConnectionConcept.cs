using System.Data;

namespace IntroductionToAdoNet.Utils;

/*
 * FILE ROLE: DbConnection concepts preview before ch.02 (Section 3).
 * SECTIONS IN THIS FILE:
 *   3. DbConnection — open a channel to the database (preview)
 */

/*
 * =========================================================================
 * SECTION 3: DbConnection — OPEN A CHANNEL TO THE DATABASE (PREVIEW)
 * =========================================================================
 *
 * DbConnection represents a session with a data source. Concrete providers
 * implement Open(), Close()/Dispose(), ConnectionString, and State.
 *
 * Typical pattern (FULL detail → ch.02 SqlConnection & Connection Strings):
 *
 *   using var connection = new SqlConnection(connectionString);
 *   connection.Open();
 *   // run commands while State == ConnectionState.Open
 *
 * Pitfalls preview:
 *   • Always dispose (using) — leaked connections exhaust the pool (ch.06)
 *   • Open only when needed; holding open connections blocks scalability
 *   • ConnectionString holds server, database, auth — never hard-code secrets
 *
 * DbConnection is abstract — you cannot `new DbConnection()`. The catalog
 * above lists SqlConnection as the SQL Server implementation used from ch.02.
 * -------------------------------------------------------------------------
 */
public static class ConnectionConcept
{
    public static string TypicalOpenPattern =>
        "using var conn = new SqlConnection(cs); conn.Open(); // ch.02";

    public static ConnectionState ClosedDefault => ConnectionState.Closed; // before Open()
}
