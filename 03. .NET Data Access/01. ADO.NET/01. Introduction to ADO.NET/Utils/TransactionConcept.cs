using IntroductionToAdoNet.Models;

namespace IntroductionToAdoNet.Utils;

/*
 * FILE ROLE: DbTransaction commit/rollback preview before ch.06 (Section 7).
 * SECTIONS IN THIS FILE:
 *   7. DbTransaction — atomic batches (preview)
 */

/*
 * =========================================================================
 * SECTION 7: DbTransaction — ATOMIC BATCHES (PREVIEW)
 * =========================================================================
 *
 * A transaction groups multiple commands into one atomic unit: all commit or
 * all roll back. You begin on an open connection, assign the transaction to
 * each command, then Commit() or Rollback().
 *
 * Sketch (FULL detail → ch.06 Transactions & Connection Pooling):
 *
 *   using var conn = new SqlConnection(cs);
 *   conn.Open();
 *   using var tx = conn.BeginTransaction();
 *   try
 *   {
 *       cmd1.Transaction = tx; cmd1.ExecuteNonQuery();
 *       cmd2.Transaction = tx; cmd2.ExecuteNonQuery();
 *       tx.Commit();
 *   }
 *   catch
 *   {
 *       tx.Rollback();
 *       throw;
 *   }
 *
 * ch.06 also covers connection pooling (why Dispose returns connections to the
 * pool) and SqlBulkCopy for high-volume inserts.
 * -------------------------------------------------------------------------
 */
public static class TransactionConcept
{
    public static string DescribeOutcome(TransactionOutcome outcome)
    {
        return outcome switch
        {
            TransactionOutcome.Commit => "Commit() — all commands persisted together",
            TransactionOutcome.Rollback => "Rollback() — no partial writes visible to other sessions",
            _ => string.Empty,
        };
    }
}
