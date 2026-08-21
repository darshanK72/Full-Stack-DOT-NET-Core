using System;

namespace TransactionsAndConnectionPooling.Services;

/*
 * FILE ROLE: Reference table and descriptions for SQL Server transaction isolation levels.
 *
 * SECTIONS IN THIS FILE:
 *   7. Transaction isolation levels
 */

/*
 * SECTION 7: TRANSACTION ISOLATION LEVELS
 *
 * Isolation controls what one transaction sees while another is in flight.
 * SQL Server default (and BeginTransaction() default) is ReadCommitted.
 *
 * | Level             | Dirty read | Non-repeatable read | Phantom read | Typical use              |
 * |-------------------|------------|---------------------|--------------|--------------------------|
 * | ReadUncommitted   | Yes        | Yes                 | Yes          | Reporting approximations |
 * | ReadCommitted     | No         | Yes                 | Yes          | Default - most OLTP      |
 * | RepeatableRead    | No         | No                  | Yes          | Stable reads in txn      |
 * | Serializable      | No         | No                  | No           | Strictest locking        |
 * | Snapshot          | No         | No                  | No           | Row versioning (DB opt)  |
 *
 * Pass the level to BeginTransaction(IsolationLevel.ReadCommitted) when you need
 * to be explicit or elevate/reduce isolation for a specific operation.
 *
 * Pitfall: long-running transactions at Serializable block other writers - keep
 * transaction scope as short as possible (open -> work -> commit/rollback -> close).
 */
internal static class IsolationLevelReference
{
    public static string DescribeLevels()
    {
        return string.Join(Environment.NewLine, new[]
        {
            "Isolation overview:",
            "  ReadUncommitted  - sees uncommitted data from others (dirty reads)",
            "  ReadCommitted    - default; only committed rows visible",
            "  RepeatableRead   - same row read twice returns same value in txn",
            "  Serializable     - strictest; phantoms prevented",
            "  Snapshot         - statement-level consistency via row versioning",
        });
    }
}
