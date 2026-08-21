# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/06. Multithreading & Async Programming/06. Synchronization and Locks/`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A payment microservice registers `LedgerService` as a **Singleton**. Under concurrent deposits and withdrawals, balances drift and QA sees different totals on every run. Review:

```csharp
public sealed class LedgerService
{
    private decimal _balance;

    public void Credit(decimal amount)
    {
        lock (this)
        {
            _balance += amount;
        }
    }

    public void Debit(decimal amount)
    {
        lock (typeof(LedgerService))
        {
            _balance -= amount;
        }
    }

    public decimal Balance => _balance; // read without synchronization
}
```

What fails in production, and what is the prioritized fix?

---

#### Q2. (R) A batch job transfers funds between two `BankAccount` instances on background threads. The job hangs intermittently under load — no exception, threads stuck in `Monitor.Wait`. Review:

```csharp
public static void Transfer(BankAccount from, BankAccount to, decimal amount)
{
    lock (from.SyncRoot)
    {
        Thread.Sleep(15); // simulate ledger validation
        lock (to.SyncRoot)
        {
            if (from.Balance >= amount)
            {
                from.WithdrawInternal(amount);
                to.DepositInternal(amount);
            }
        }
    }
}

// Worker pool runs Transfer(alpha, beta, 40) and Transfer(beta, alpha, 25) concurrently.
// BankAccount.Deposit/Withdraw each lock the same private SyncRoot internally.
```

What causes the hang, and how do you fix it without removing multi-account transfers?

---

#### Q3. (R) A developer "async-ified" a cache warmer registered as a **Singleton** in ASP.NET Core. The app compiles in some branches but stalls request threads under traffic. Review:

```csharp
public sealed class RateCacheWarmer
{
    private readonly object _sync = new();
    private readonly Dictionary<string, decimal> _rates = new();
    private readonly HttpClient _http = new();

    public async Task RefreshAsync(string productCode, CancellationToken ct)
    {
        lock (_sync)
        {
            var json = _http.GetStringAsync($"/rates/{productCode}", ct).Result;
            _rates[productCode] = ParseRate(json);
        }
    }
}
```

What are the problems (compile-time where applicable, runtime, and scalability), and how do you fix them in priority order?

---

#### Q4. (R) A read-heavy interest-rate API uses `ReaderWriterLockSlim` like the chapter tutorial. The first request for a missing product code freezes the entire rate service. Review:

```csharp
public decimal GetOrAddRate(string productCode)
{
    _rwLock.EnterReadLock();
    try
    {
        if (!_rates.ContainsKey(productCode))
        {
            _rwLock.EnterWriteLock();
            try
            {
                _rates[productCode] = LoadDefaultFromConfig(productCode);
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }
        return _rates[productCode];
    }
    finally
    {
        _rwLock.ExitReadLock();
    }
}
```

What breaks, and what is the correct locking pattern for lazy insert under concurrent readers?

---

#### Q5. (P) An outbound API integration must allow at most **50 concurrent HTTP calls** cluster-wide per process, record a global request counter for metrics, and support cooperative shutdown of a background poller. Which synchronization primitives do you use for each concern, and what breaks if you use `lock` for all three?

---

#### Q6. (M) A nightly vault-scan worker runs on a dedicated thread. Operators click "Stop" in a WinForms-style host; locally it often exits, but on release builds in production the thread keeps running until the process is killed. Review:

```csharp
public sealed class VaultScanWorker
{
    private bool _stopRequested;

    public void Run(CancellationToken externalToken)
    {
        while (!externalToken.IsCancellationRequested && !_stopRequested)
        {
            ScanNextBatch();
            Thread.Sleep(40);
        }
    }

    public void RequestStop()
    {
        _stopRequested = true;
    }
}
```

Why does `_stopRequested` fail to stop the loop reliably across CPU cores, and what is the production-safe fix?

---

#### Q7. (D) Two designs protect a singleton in-memory fee schedule updated once per hour and read on every pricing request:

**A.** `Dictionary<string, decimal>` + `ReaderWriterLockSlim` (manual read/write locks)  
**B.** `ConcurrentDictionary<string, decimal>` with snapshot replace on refresh

Which do you ship for a read-heavy ASP.NET Core pricing API, and what trade-offs drive the choice?
