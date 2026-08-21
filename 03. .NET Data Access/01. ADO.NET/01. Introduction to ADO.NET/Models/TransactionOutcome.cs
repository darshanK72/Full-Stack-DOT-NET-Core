namespace IntroductionToAdoNet.Models;

/*
 * FILE ROLE: Enum for transaction commit vs rollback outcomes (Section 7 preview).
 * SECTIONS IN THIS FILE:
 *   7. DbTransaction — atomic batches (preview) — TransactionOutcome enum
 */

public enum TransactionOutcome
{
    Commit,
    Rollback,
}
