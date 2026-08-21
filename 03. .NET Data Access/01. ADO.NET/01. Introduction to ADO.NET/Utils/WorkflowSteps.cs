using System.Collections.Generic;

namespace IntroductionToAdoNet.Utils;

/*
 * FILE ROLE: Typical connected and disconnected data-access workflows (Section 9).
 * SECTIONS IN THIS FILE:
 *   9. Typical data-access workflow
 */

/*
 * =========================================================================
 * SECTION 9: TYPICAL DATA-ACCESS WORKFLOW
 * =========================================================================
 *
 * Most read paths follow the same connected pipeline. Write paths add
 * transactions when multiple tables must stay consistent.
 *
 *   ┌─────────────┐     ┌─────────────┐     ┌──────────────────┐
 *   │ DbConnection│────▶│  DbCommand  │────▶│   DbDataReader   │
 *   │  .Open()    │     │ Parameters  │     │  while Read()    │
 *   └─────────────┘     │ ExecuteReader│     │  map columns     │
 *         │             └─────────────┘     └──────────────────┘
 *         │                    │                      │
 *   using/dispose         using/dispose          using/dispose
 *   returns conn          clears command         close before next
 *   to pool (ch.06)                              command on conn
 *
 * WRITE (multi-step, atomic):
 *
 *   Open → BeginTransaction → Command+Transaction → ExecuteNonQuery (×N)
 *        → Commit or Rollback → Dispose
 *
 * DISCONNECTED SNAPSHOT:
 *
 *   Open → SqlDataAdapter.Fill(DataTable) → Close → edit rows → adapter.Update()
 *
 * ch.08 adds async variants: OpenAsync, ExecuteReaderAsync, cancellation tokens.
 * -------------------------------------------------------------------------
 */
public static class WorkflowSteps
{
    public static IReadOnlyList<string> ConnectedReadSteps()
    {
        return new[]
        {
            "1. Build connection string (server, database, auth — ch.02)",
            "2. using var connection = new SqlConnection(connectionString)",
            "3. connection.Open()",
            "4. using var command = new SqlCommand(sql, connection)",
            "5. command.Parameters.Add(...)",
            "6. using DbDataReader reader = command.ExecuteReader()",
            "7. while (reader.Read()) { map columns }",
            "8. Dispose reader, command, connection (using handles 6—8)",
        };
    }

    public static IReadOnlyList<string> DisconnectedSnapshotSteps()
    {
        return new[]
        {
            "1. Open connection",
            "2. SqlDataAdapter.Fill(dataTable)",
            "3. Close connection — data remains in DataTable",
            "4. UI or business logic edits rows in memory",
            "5. SqlCommandBuilder + adapter.Update(dataTable) pushes changes",
        };
    }
}
