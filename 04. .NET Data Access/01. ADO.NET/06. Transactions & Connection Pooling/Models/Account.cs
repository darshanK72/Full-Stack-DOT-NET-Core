namespace TransactionsAndConnectionPooling.Models;

/*
 * FILE ROLE: Domain row type for the account transfer transaction demo.
 *
 * SECTIONS IN THIS FILE:
 *   2. Account — domain row for the transfer demo
 */

/*
 * SECTION 2: ACCOUNT - domain row for the transfer demo
 *
 * Two accounts seed the AdoNetTutorial.dbo.Accounts table:
 *   1 Checking  $1,000.00
 *   2 Savings     $500.00
 */
internal sealed class Account
{
    public int AccountId { get; init; }
    public string AccountName { get; init; } = string.Empty;
    public decimal Balance { get; set; }
}
