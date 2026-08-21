# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/06. Multithreading & Async Programming/06. Synchronization and Locks/`

---

#### Q1. (R) A payment microservice registers `LedgerService` as a **Singleton**. Under concurrent deposits and withdrawals, balances drift and QA sees different totals on every run. Review:

**Answer:** `Credit` and `Debit` lock on different objects (`this` vs `typeof(LedgerService)`), so they do not serialize against each other, and `Balance` reads `_balance` without any lock. The singleton shares one field across all requests — you get lost updates and torn reads.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Different sync roots per mutation path | Credit and Debit can interleave on `_balance` |
| Correctness | Unsynchronized read on `Balance` | Callers see stale or torn decimal values |
| Design | `lock (this)` / `lock (typeof(T))` | External code can deadlock on the same object; violates ch.06 guidance |
| DI / lifetime | Singleton + mutable balance field | All HTTP requests share one ledger — drift scales with traffic |

**Fix (priority order):**

1. Use one private readonly sync root for all balance access — same object for Credit, Debit, and Balance getter (see `BankAccount._syncRoot` in **Program.cs** Section 3).
2. Lock (or use `Interlocked` only if you refactor to a single `long` cents field) on every read/write of `_balance`.
3. Re-evaluate singleton lifetime — per-tenant or scoped ledger may be required; at minimum document that this service is a process-wide counter, not per-account isolation.
4. Never lock on `this` or `typeof(LedgerService)`.

```csharp
private readonly object _sync = new();
private decimal _balance;

public void Credit(decimal amount) { lock (_sync) { _balance += amount; } }
public void Debit(decimal amount)  { lock (_sync) { _balance -= amount; } }
public decimal Balance { get { lock (_sync) { return _balance; } } }
```

**Production takeaway:** One resource, one sync root — mixed lock targets are a classic "looks synchronized but isn't" defect on singleton services.

---

#### Q2. (R) A batch job transfers funds between two `BankAccount` instances on background threads. The job hangs intermittently under load — no exception, threads stuck in `Monitor.Wait`. Review:

**Answer:** `Transfer` acquires `from` then `to`, while concurrent `Transfer(beta, alpha, …)` acquires in the opposite order — classic circular wait deadlock. Nested locks on account roots that are also locked inside `Deposit`/`Withdraw` compound contention but the hang is the ordering inversion.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | Opposite lock order on two accounts | Intermittent deadlock — threads block forever |
| Design | Nested lock on `to` while holding `from` | Circular wait when transfers run in both directions |
| Maintainability | `Sleep` inside lock | Extends hold time, increases deadlock window and throughput collapse |

**Fix (priority order):**

1. Acquire locks in a consistent global order — e.g. by `AccountId` string comparison (`TransferSafely` in **Program.cs** Section 10).
2. Call internal mutators (`WithdrawInternal` / `DepositInternal`) only while both locks are held; do not re-enter public methods that lock again on the same root (reentrancy saves you here, but the pattern is fragile).
3. Remove simulated I/O from inside the lock; validate outside or use a short critical section.
4. Optionally use `Monitor.TryEnter` with timeout and retry/backoff when ordering cannot be guaranteed.

```csharp
var first  = string.CompareOrdinal(from.AccountId, to.AccountId) <= 0 ? from : to;
var second = ReferenceEquals(first, from) ? to : from;
lock (first.SyncRoot) {
    lock (second.SyncRoot) {
        if (from.Balance >= amount) { from.WithdrawInternal(amount); to.DepositInternal(amount); }
    }
}
```

**Production takeaway:** Any time two resources can be locked together, define a total order — Karat expects you to name deadlock before suggesting `lock` everywhere.

---

#### Q3. (R) A developer "async-ified" a cache warmer registered as a **Singleton** in ASP.NET Core. The app compiles in some branches but stalls request threads under traffic. Review:

**Answer:** `RefreshAsync` is async in name only — it blocks a thread inside `lock` via `.Result` on `GetStringAsync`, which can deadlock on ASP.NET's sync context and always starves the thread pool. Holding `lock` during network I/O serializes all refreshes and blocks other readers.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Async | `.Result` on `GetStringAsync` inside lock | Thread-pool starvation; potential ASP.NET deadlock |
| Scalability | Network I/O under `lock` | One refresh blocks all other cache access |
| DI | Singleton + `new HttpClient()` | Socket exhaustion; no DNS refresh — use `IHttpClientFactory` |
| API design | `async Task` method with no `await` | Misleading signature; analyzers may flag CS1998 |

**Fix (priority order):**

1. Remove `.Result` — `await _http.GetStringAsync(...)` **outside** any lock; only lock for the dictionary write (or use `ConcurrentDictionary` from ch.07).
2. Inject `IHttpClientFactory` and create clients via `CreateClient("rates")`.
3. Use `SemaphoreSlim` (not `lock`) if you need to limit concurrent refreshes — `await gate.WaitAsync(ct)` is async-safe.
4. Consider double-checked locking with versioned snapshot replace instead of locking around HTTP.

```csharp
var json = await _http.GetStringAsync($"/rates/{productCode}", ct).ConfigureAwait(false);
var rate = ParseRate(json);
lock (_sync) { _rates[productCode] = rate; }
```

**Production takeaway:** Never combine `lock` + sync-over-async — ch.04 async rules and ch.06 lock rules collide here; pick async coordination primitives.

---

#### Q4. (R) A read-heavy interest-rate API uses `ReaderWriterLockSlim` like the chapter tutorial. The first request for a missing product code freezes the entire rate service. Review:

**Answer:** The code calls `EnterWriteLock` while already holding `EnterReadLock` on the same `ReaderWriterLockSlim`. That lock type is not upgradeable — the thread blocks forever waiting for itself to release the read lock.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | Read lock → write lock upgrade on same thread | Self-deadlock on first cache miss — service hangs |
| Correctness | Lazy insert under read lock | Write never acquired; all readers eventually block |
| Design | Check-then-act outside write lock | Duplicate loads possible even after fix — needs double-check |

**Fix (priority order):**

1. Release read lock before taking write lock — exit read, enter write, re-check, insert, exit write, re-enter read for return (classic double-checked locking).
2. Or enter write lock directly when miss is likely; keep read lock only for the happy path (`GetRate` in **Program.cs** Section 7 shows the read-only path).
3. For ASP.NET Core read-heavy caches, consider immutable snapshot replace or `ConcurrentDictionary.GetOrAdd` (ch.07) to avoid manual RW lock upgrade entirely.

```csharp
_rwLock.EnterReadLock();
try {
    if (_rates.TryGetValue(productCode, out var rate)) return rate;
} finally { _rwLock.ExitReadLock(); }

_rwLock.EnterWriteLock();
try {
    if (!_rates.ContainsKey(productCode))
        _rates[productCode] = LoadDefaultFromConfig(productCode);
    return _rates[productCode];
} finally { _rwLock.ExitWriteLock(); }
```

**Production takeaway:** `ReaderWriterLockSlim` does not support lock upgrade — lazy insert requires release-then-acquire or a concurrent collection.

---

#### Q5. (P) An outbound API integration must allow at most **50 concurrent HTTP calls** cluster-wide per process, record a global request counter for metrics, and support cooperative shutdown of a background poller. Which synchronization primitives do you use for each concern, and what breaks if you use `lock` for all three?

**Answer:** Use `SemaphoreSlim(50, 50)` with `WaitAsync`/`Release` for outbound throttling, `Interlocked.Increment` (or `Interlocked.Read` patterns) for the metrics counter, and `volatile bool` or `CancellationToken` for cooperative shutdown. Using `lock` for all three serializes HTTP concurrency to one call at a time and blocks async waits inside the lock.

- **Concurrency cap (50 calls):** `SemaphoreSlim` — counting semaphore matches "N at a time" (Section 6). `WaitAsync` avoids blocking thread-pool threads during I/O.
- **Global request counter:** `Interlocked.Increment(ref _totalRequests)` — single-field atomic math without lock overhead (Section 8).
- **Cooperative shutdown:** `CancellationTokenSource.Cancel()` linked to host shutdown, or `volatile bool _stopRequested` checked in the poller loop (Section 9).

**What breaks with `lock` everywhere:**

- Throttling under `lock` during `await` — cannot await inside `lock`; you'd block one thread per wait, defeating parallelism and risking deadlocks.
- Counter under `lock` works but adds contention on every metric tick; unnecessary when `Interlocked` suffices.
- Stop flag under `lock` on every loop iteration adds latency; visibility is solved by `volatile` or `CancellationToken` without serializing the loop.

**Production takeaway:** Match primitive to concern — `SemaphoreSlim` for N-way gates, `Interlocked` for counters, `CancellationToken`/`volatile` for flags; `lock` is the default exclusive choice, not the only hammer.

---

#### Q6. (M) A nightly vault-scan worker runs on a dedicated thread. Operators click "Stop" in a WinForms-style host; locally it often exits, but on release builds in production the thread keeps running until the process is killed. Review:

**Answer:** `_stopRequested` is a plain `bool` without `volatile` or synchronization. The JIT/CPU may cache the field in a register on the worker core, so writes from the UI thread are not guaranteed visible — the loop never observes `true`. This is the visibility problem **Program.cs** Section 9 demonstrates with `volatile bool _stopRequested`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Memory model | Non-volatile bool flag | Worker may never see stop write across cores |
| Correctness | Cooperative cancel relies on visibility | Production-only hangs after Stop click |
| Design | Ignores `CancellationToken` already passed in | Host shutdown cannot propagate cleanly |

**Fix (priority order):**

1. Mark `_stopRequested` as `volatile bool` **or** prefer checking `externalToken.IsCancellationRequested` only and call `RequestStop` via `CancellationTokenSource.Cancel()`.
2. Wire host shutdown to cancel the same token passed to `Run`.
3. Do not use `Thread.Abort` — cooperative exit only.
4. For complex state, use `lock` around flag read/write or `Interlocked.Exchange` — overkill for a simple stop bit but valid.

```csharp
private volatile bool _stopRequested;

public void RequestStop() => _stopRequested = true;
// Better: inject CancellationToken and drop the bool entirely.
```

**Production takeaway:** Visibility ≠ atomicity — a stop flag needs `volatile`, `CancellationToken`, or a lock; `bool` alone is a release-build Heisenbug.

---

#### Q7. (D) Two designs protect a singleton in-memory fee schedule updated once per hour and read on every pricing request:

**Answer:** Ship **Option B** (`ConcurrentDictionary` with snapshot replace on refresh) for a read-heavy ASP.NET Core pricing API. Readers never block writers except briefly during reference swap; no manual RW lock upgrade risk; scales with concurrent pricing requests.

| | A — RW lock + Dictionary | B — ConcurrentDictionary + snapshot replace |
|---|---|---|
| Read path | `EnterReadLock` per request — readers parallel but lock object overhead | Unsynchronized reads on stable dictionary reference |
| Refresh | Must take write lock — blocks all pricing during update | Build new dictionary off-thread; `Interlocked.Exchange` or volatile swap of reference |
| Complexity | Upgrade/lazy-insert traps; must dispose `ReaderWriterLockSlim` | Refresh logic must publish immutable snapshot atomically |
| ASP.NET fit | OK for moderate read load | Better for high RPS read-heavy APIs |

**Trade-offs:**

- Choose **A** when refresh mutates entries in place and you need fine-grained per-key updates with strong in-process RW semantics and moderate traffic.
- Choose **B** when updates are batch/hourly and reads dominate — copy-on-write avoids long write locks and matches options-pattern snapshot refresh.
- Either way: do not expose mutable `Dictionary` without synchronization; document that pricing reads see eventually consistent fees for one refresh window.

**Production takeaway:** Read-heavy web APIs favor immutable snapshot publish over long-lived RW locks — aligns with ch.07 concurrent collections and ch.06 "short critical sections."
