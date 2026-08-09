using System;
using System.Collections.Generic;
using TransactionsAndConnectionPooling.Models;

namespace TransactionsAndConnectionPooling.Services;

/*
 * FILE ROLE: In-memory fallback ledger that mimics commit/rollback when LocalDB is unavailable.
 *
 * SECTIONS IN THIS FILE:
 *   3. In-memory ledger — fallback when LocalDB is unavailable
 */

/*
 * SECTION 3: IN-MEMORY LEDGER - fallback when LocalDB is unavailable
 *
 * If SqlConnection.Open fails (LocalDB not installed, service stopped), the demo
 * continues with an in-memory dictionary that mimics commit/rollback semantics:
 *   - snapshot balances at transaction start
 *   - on success - keep updated balances (Commit)
 *   - on failure - restore snapshot (Rollback)
 *
 * Same business rules as the SQL path: insufficient funds aborts the transfer.
 */
internal sealed class InMemoryLedger
{
    private readonly Dictionary<int, Account> _accounts = new Dictionary<int, Account>
    {
        [1] = new Account { AccountId = 1, AccountName = "Checking", Balance = 1000.00m },
        [2] = new Account { AccountId = 2, AccountName = "Savings", Balance = 500.00m },
    };

    public IReadOnlyList<Account> GetAccounts()
    {
        var copy = new List<Account>();
        foreach (Account account in _accounts.Values)
        {
            copy.Add(new Account
            {
                AccountId = account.AccountId,
                AccountName = account.AccountName,
                Balance = account.Balance,
            });
        }

        return copy;
    }

    public void ResetToSeed()
    {
        _accounts[1].Balance = 1000.00m;
        _accounts[2].Balance = 500.00m;
    }

    public (bool Committed, string Message) Transfer(int fromAccountId, int toAccountId, decimal amount)
    {
        Dictionary<int, decimal> snapshot = new Dictionary<int, decimal>();
        foreach (KeyValuePair<int, Account> pair in _accounts)
        {
            snapshot[pair.Key] = pair.Value.Balance; // snapshot - rollback restores these values
        }

        try
        {
            if (amount <= 0m)
            {
                throw new InvalidOperationException("Transfer amount must be positive.");
            }

            if (_accounts[fromAccountId].Balance < amount)
            {
                throw new InvalidOperationException(
                    $"Insufficient funds: {_accounts[fromAccountId].AccountName} has {_accounts[fromAccountId].Balance:C}, need {amount:C}.");
            }

            _accounts[fromAccountId].Balance -= amount; // debit
            _accounts[toAccountId].Balance += amount;   // credit
            return (true, "In-memory COMMIT - balances kept.");
        }
        catch (Exception ex)
        {
            foreach (KeyValuePair<int, decimal> pair in snapshot)
            {
                _accounts[pair.Key].Balance = pair.Value; // ROLLBACK - restore pre-transfer snapshot
            }

            return (false, "In-memory ROLLBACK - " + ex.Message);
        }
    }
}
