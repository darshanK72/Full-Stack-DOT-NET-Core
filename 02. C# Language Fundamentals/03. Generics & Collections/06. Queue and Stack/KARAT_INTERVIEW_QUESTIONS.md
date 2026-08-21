# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/03. Generics & Collections/06. Queue and Stack`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A help-desk service was refactored from `Queue<SupportTicket>` to `Stack<SupportTicket>` "because stacks are faster." Review the handler loop. What ordering bug appears in production, and how do you fix it?

```csharp
public sealed class TicketProcessor
{
    private readonly Stack<SupportTicket> _pending = new Stack<SupportTicket>();

    public void EnqueueTicket(SupportTicket ticket) => _pending.Push(ticket);

    public void ProcessAll()
    {
        while (_pending.Count > 0)
        {
            SupportTicket next = _pending.Pop();
            Resolve(next);
        }
    }

    private void Resolve(SupportTicket ticket) { /* SLA tracking */ }
}

// Arrival order: #1001 (9:00), #1002 (9:05), #1003 (9:10)
// ProcessAll resolves: #1003, #1002, #1001
```

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

---

#### Q3. (P) Three ASP.NET Core request threads enqueue audit events; one background `IHostedService` dequeues them for batch upload. The team shares one `Queue<AuditEvent>` instance registered as a **Singleton**. Occasionally events disappear or `InvalidOperationException` appears under concurrent `Enqueue`/`Dequeue`. Explain why `Queue<T>` is unsafe here and what you would register instead.

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

---

#### Q5. (D) Your team must pick a frontier collection for two graph tasks on an unweighted social network: (A) find **shortest path** in friend hops from user A to user B, and (B) detect whether a **cycle** exists in a follow graph (direction matters). One engineer says "both are graph search — use `Stack<T>` for both." What would you choose for each task and why?

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
