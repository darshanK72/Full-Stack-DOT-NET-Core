# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/03. Generics & Collections/06. Queue and Stack`

---

#### Q1. (R) A help-desk service was refactored from `Queue<SupportTicket>` to `Stack<SupportTicket>` "because stacks are faster." Review the handler loop. What ordering bug appears in production, and how do you fix it?

**Answer:** `Stack<T>` is LIFO — the last ticket pushed is the first popped — so SLA fairness is inverted: newest tickets are resolved before older ones waiting longer. Ticket queues require FIFO semantics, which `Queue<T>` enforces at the type level.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `Stack` + `Push`/`Pop` for arrival-order work | Newest-first processing — SLA breaches on oldest tickets |
| Naming/API | Method still named `EnqueueTicket` but calls `Push` | Misleading API; code review misses semantic mismatch |
| Design | Chose collection for perceived speed, not ordering rule | Wrong abstraction — `List<T>` with `Insert(0,…)` would be equally wrong |

**Fix (priority order):**

1. Restore `Queue<SupportTicket>` with `Enqueue` / `TryDequeue` (or `Dequeue` when empty is impossible by contract).
2. Rename methods to match semantics: `EnqueueTicket` + `TryResolveNextTicket` as in **Program.cs** Section 4.
3. If priority tiers are needed later, use `PriorityQueue<TElement, TPriority>` — not `Stack<T>`.
4. Document ordering invariant in tests: enqueue A, B, C → resolve A, B, C.

```csharp
private readonly Queue<SupportTicket> _pending = new();

public void EnqueueTicket(SupportTicket ticket) => _pending.Enqueue(ticket);

public bool TryResolveNext(out SupportTicket ticket) => _pending.TryDequeue(out ticket);
```

**Production takeaway:** Karat tests whether you match collection type to business ordering — FIFO for fair queues, LIFO for undo/call-stack models. See **Program.cs** Section 1 — FIFO vs LIFO table.

---

#### Q2. (R) A background worker drains a print queue when the upstream publisher is idle. Under load, the service logs unhandled `InvalidOperationException` and the host restarts. Review the consumer:

```csharp
public sealed class PrintWorker
{
    private readonly Queue<PrintJob> _jobs = new Queue<PrintJob>();

    public void Submit(PrintJob job) => _jobs.Enqueue(job);

    public void Run(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            PrintJob job = _jobs.Dequeue();  // throws when queue empty
            Print(job);
        }
    }
}
```

What breaks, and how would you harden this for production idle periods?

**Answer:** `Dequeue()` throws `InvalidOperationException` when the queue is empty — the tight loop calls it continuously during idle periods, crashing the worker instead of waiting for the next job.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | `Dequeue()` on empty queue | Unhandled exception → host restart / lost in-flight work |
| Control flow | Busy loop with no back-off when empty | 100% CPU spin if switched to `Count` check without delay |
| Concurrency | Plain `Queue<T>` if multiple producers (minor here) | Not thread-safe — separate from empty-queue bug but common in same services |

**Fix (priority order):**

1. Replace `Dequeue()` with `TryDequeue(out PrintJob? job)` — process only when `true`.
2. When empty, await a signal (`Channel<PrintJob>`, `BlockingCollection<T>`, or `ManualResetEventSlim` + lock) instead of spinning.
3. Optionally combine with `await Task.Delay(pollInterval, ct)` only if a simple poll model is acceptable — prefer event-driven dequeue.
4. For multi-producer scenarios, use `ConcurrentQueue<T>` or a `Channel<T>` writer/reader pair.

```csharp
public async Task RunAsync(CancellationToken ct)
{
    while (!ct.IsCancellationRequested)
    {
        if (_jobs.TryDequeue(out PrintJob? job))
        {
            Print(job);
            continue;
        }

        await Task.Delay(100, ct); // or await _signal.WaitAsync(ct);
    }
}
```

**Production takeaway:** Empty is an expected state for workers — `TryDequeue`/`TryPop` exist precisely to avoid exception-driven control flow. See **Program.cs** Section 2a — empty queue behavior.

---

#### Q3. (P) Three ASP.NET Core request threads enqueue audit events; one background `IHostedService` dequeues them for batch upload. The team shares one `Queue<AuditEvent>` instance registered as a **Singleton**. Occasionally events disappear or `InvalidOperationException` appears under concurrent `Enqueue`/`Dequeue`. Explain why `Queue<T>` is unsafe here and what you would register instead.

**Answer:** `Queue<T>` is not thread-safe — concurrent `Enqueue` and `Dequeue` from multiple threads corrupt internal state without external locking, causing lost items or exceptions. A singleton shared across request threads requires a concurrent collection or a `Channel<T>`.

- **`ConcurrentQueue<T>`:** Lock-free FIFO safe for multiple producers and consumers; `TryDequeue` for the background drainer. Good when you only need in-memory fan-in.
- **`Channel<T>` (System.Threading.Channels):** Preferred in modern ASP.NET Core — bounded capacity for back-pressure, async `Reader.ReadAllAsync`, clean producer/consumer split in DI.
- **`BlockingCollection<T>`:** Legacy pattern wrapping a concurrent queue with blocking take — workable but heavier than channels for new code.
- **Do not** wrap `Queue<T>` in a singleton and synchronize ad hoc on every call without reviewing lock ordering — easy to deadlock with `Dequeue` inside `lock` while producers hold the same lock incorrectly.

```csharp
// Registration sketch
builder.Services.AddSingleton(Channel.CreateBounded<AuditEvent>(
    new BoundedChannelOptions(10_000) { FullMode = BoundedChannelFullMode.Wait }));
builder.Services.AddHostedService<AuditBatchUploader>();
```

**Production takeaway:** FIFO ordering does not imply thread safety — choose `ConcurrentQueue<T>` or `Channel<T>` when a queue crosses thread boundaries. See **Program.cs** Section 2 — `Queue<T>` API assumes single-threaded mutation unless externally synchronized.

---

#### Q4. (M) A developer rewrites maze pathfinding from the chapter's BFS to recursive DFS. On large grids the process terminates with `StackOverflowException`. They propose "just use `Stack<T>` instead of recursion." Review both approaches:

```csharp
// Original (chapter-style BFS) — works on large maze
Queue<(int Row, int Col)> frontier = new();
frontier.Enqueue(start);
while (frontier.TryDequeue(out var current)) { /* expand neighbors */ }

// Rewrite — deep recursion on 2000×2000 grid
void Dfs(int row, int col)
{
    if (visited[row, col]) return;
    visited[row, col] = true;
    foreach (var neighbor in GetNeighbors(row, col))
        Dfs(neighbor.Row, neighbor.Col);  // one frame per depth level
}
```

What actually causes the overflow, and when does an explicit `Stack<T>` fix it vs when recursion is acceptable?

**Answer:** `StackOverflowException` comes from the **CLR call stack** — each recursive `Dfs` call consumes a stack frame (~1 MB default thread stack limit), not from `Stack<T>` heap storage. Replacing recursion with an explicit `Stack<(int,int)>` loop uses the heap for frontier cells, avoiding deep call stacks on large grids.

- **Cause:** Depth-first recursion on a path thousands of cells long nests that many frames; the OS/thread stack overflows before the algorithm finishes.
- **Explicit `Stack<T>` fix:** Push start cell; `while (stack.TryPop(out current))` expand neighbors and push unvisited — same LIFO DFS order, bounded by heap memory instead of call-stack depth.
- **When recursion is fine:** Shallow trees (expression AST depth &lt; ~100), divide-and-conquer with logarithmic depth, or problems with guaranteed small branching depth.
- **BFS vs DFS choice (related):** Chapter BFS with `Queue<T>` finds shortest paths in unweighted grids; DFS (recursive or `Stack<T>`) does not guarantee shortest path but uses less memory for some sparse graphs.
- **Not a fix:** Switching BFS to `Stack<T>` without changing algorithm — that yields DFS traversal order and breaks shortest-path guarantees from **Program.cs** Section 5.

**Production takeaway:** Karat distinguishes the **call stack** (recursion limit) from **`Stack<T>`** (heap collection) — iterative DFS with `Stack<T>` is the standard production pattern for deep graph search.

---

#### Q5. (D) Your team must pick a frontier collection for two graph tasks on an unweighted social network: (A) find **shortest path** in friend hops from user A to user B, and (B) detect whether a **cycle** exists in a follow graph (direction matters). One engineer says "both are graph search — use `Stack<T>` for both." What would you choose for each task and why?

**Answer:** Shortest hop count in an unweighted graph requires BFS with `Queue<T>` so nodes are discovered in non-decreasing distance from the start; cycle detection in a directed graph is typically DFS with `Stack<T>` (or recursion) and a recursion/recursion-stack coloring strategy — not the same frontier choice.

**Task A — shortest friend hops (unweighted):**

- Use **`Queue<UserId>`** BFS — first time you dequeue B, you have minimum hop count.
- `Stack<T>` DFS may find *a* path quickly but not the shortest — wrong for "degrees of separation" product features.

**Task B — cycle in directed follow graph:**

- Use **DFS** with **`Stack<UserId>`** (explicit or recursion) plus `visited` / `onStack` (three-color) state to detect back edges.
- BFS with `Queue<T>` finds cycles in undirected graphs with parent tracking but directed cycle detection is awkward with BFS alone.

| Task | Collection | Why |
|---|---|---|
| Shortest hops (unweighted) | `Queue<T>` — BFS | Layer-by-layer discovery = minimum edges |
| Directed cycle detection | `Stack<T>` — DFS | Back edge to active stack frame signals cycle |

- **Production note:** At web scale, graph logic moves to a graph DB or precomputed index — but the collection choice still signals correct algorithmic reasoning in code reviews and Karat screens.

**Production takeaway:** Match FIFO vs LIFO to the **invariant** you need (shortest layer vs deep path/back-edge detection), not to "both are graphs." See **Program.cs** Quick Reference — BFS → `Queue<T>`, DFS → `Stack<T>` or recursion.

---

#### Q6. (R) A response editor copied from the chapter's `HelpDeskSession` mixes undo (`Stack<string>`) with ticket draining. Review this merge:

```csharp
public sealed class AgentSession
{
    private readonly Queue<SupportTicket> _tickets = new();
    private readonly Stack<string> _undo = new();
    private readonly StringBuilder _draft = new();

    public void BeginResponse(SupportTicket ticket)
    {
        _draft.Clear();
        _undo.Clear();                    // clears undo history
        _tickets.Enqueue(ticket);         // re-queues active ticket to tail
    }

    public void ApplyEdit(Action<StringBuilder> edit)
    {
        _undo.Push(_draft.ToString());
        edit(_draft);
    }

    public SupportTicket? TakeNextTicket()
    {
        return _tickets.Count > 0 ? _tickets.Dequeue() : null;
    }
}
```

The agent reports tickets jumping to the back of the line and undo lost mid-edit. What went wrong with collection choice and API usage?

**Answer:** `BeginResponse` misuses both collections — it re-`Enqueue`s the ticket already being worked (sending it to the tail instead of keeping it as the active item) and clears the undo stack even when only the draft should reset. Tickets and undo stacks serve different lifecycles and must not be conflated in one "begin" method.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Queue misuse | `Enqueue(ticket)` on ticket already removed for editing | Ticket moves to back — others processed first |
| Stack misuse | `_undo.Clear()` on every begin | Undo history wiped — agent cannot revert prior edits |
| API design | `BeginResponse` accepts ticket param implying re-queue | Confuses "start draft text" with "return ticket to queue" |
| Empty handling | `TakeNextTicket` uses ternary + `Dequeue` | Acceptable here, but inconsistent with chapter's `TryDequeue` pattern |

**Fix (priority order):**

1. Split responsibilities: `TryResolveNextTicket` dequeues once; `BeginResponse(string openingLine)` only clears draft + undo — **no** queue mutation (mirror **Program.cs** Section 4 `HelpDeskSession`).
2. Do not pass the active ticket back into the queue until the response is sent or explicitly re-queued.
3. Clear `_undo` only when starting a **new** response for a **new** ticket, not on every keystroke batch.
4. Prefer `TryDequeue` over `Count` + `Dequeue` to avoid races if the session becomes multi-threaded.

```csharp
public void BeginResponse(string openingLine)
{
    _undo.Clear();
    _draft.Clear();
    _draft.Append(openingLine);
}

public bool TryResolveNextTicket(out SupportTicket ticket)
    => _tickets.TryDequeue(out ticket);
```

**Production takeaway:** Queue and Stack often appear together in one workflow (tickets FIFO + undo LIFO) — Karat tests that you keep each collection's contract isolated. See **Program.cs** Section 4 — help-desk scenario wiring.
