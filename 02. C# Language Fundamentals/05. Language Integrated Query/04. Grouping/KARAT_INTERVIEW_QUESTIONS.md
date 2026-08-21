# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/05. Language Integrated Query/04. Grouping`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A support dashboard builds assignee buckets once at startup, then mutates shared `Department` objects when tickets are reassigned. Review this grouping code. What breaks after reassignment, and how do you fix it?

```csharp
public sealed class Department
{
    public string Code { get; set; } = "";
    public override bool Equals(object? obj) =>
        obj is Department d && Code == d.Code;
    public override int GetHashCode() => Code.GetHashCode();
}

public readonly record struct Ticket(
    int TicketId, string Title, Department Dept, string Assignee);

// Startup — board built from DB; several tickets share the same Dept instance
ILookup<Department, Ticket> byDept = board.ToLookup(t => t.Dept);

// Later, reassignment mutates the shared key object in place:
board[0].Dept.Code = "NET";  // was "HW"

// Dashboard still queries old buckets:
int hwCount = byDept[sharedHwDept].Count();  // stale / empty
int netCount = byDept[sharedHwDept].Count(); // same mutated instance, wrong bucket
```

---

#### Q2. (R) A nightly report caches `GroupBy` results in a field so the web tier can reuse them all day. Review this service. What is wrong with treating `IEnumerable<IGrouping<…>>` as a snapshot, and how do you materialize correctly?

```csharp
public sealed class TicketReportCache
{
    private IEnumerable<IGrouping<string, Ticket>>? _byPriority;

    public void Refresh(List<Ticket> board)
    {
        _byPriority = board.GroupBy(t => t.Priority); // stored as "cache"
    }

    public int GetCount(string priority, List<Ticket> liveBoard)
    {
        liveBoard.Add(new Ticket(99, "Hotfix", priority, "Net", "Ada", 1)); // live mutations
        return _byPriority!.First(g => g.Key == priority).Count();
    }
}
```

---

#### Q3. (P) An API endpoint receives 50k tickets and must answer "how many tickets per assignee?" for **each** of 200 assignee names in a loop (authorization filter). A developer uses deferred `GroupBy` inside the loop. Review the pattern and choose the correct LINQ operator for production.

```csharp
public Dictionary<string, int> CountByAssignee(IEnumerable<Ticket> board, IReadOnlyList<string> assignees)
{
    var counts = new Dictionary<string, int>();
    foreach (string assignee in assignees)
    {
        IEnumerable<IGrouping<string, Ticket>> groups = board.GroupBy(t => t.Assignee);
        counts[assignee] = groups.First(g => g.Key == assignee).Count();
    }
    return counts;
}
```

What is the performance problem, and what should replace it?

---

#### Q4. (R) A tree-view UI renders Category → Priority → tickets using nested `GroupBy`. Product later complains the page times out on a 120k-row export. Review the nesting approach vs a flat composite key. What is inefficient here, and how would you refactor?

```csharp
public IEnumerable<CategoryNode> BuildTree(IEnumerable<Ticket> board)
{
    foreach (IGrouping<string, Ticket> categoryGroup in board.GroupBy(t => t.Category).OrderBy(g => g.Key))
    {
        var priorityNodes = new List<PriorityNode>();
        foreach (IGrouping<string, Ticket> priorityGroup in
                 categoryGroup.GroupBy(t => t.Priority).OrderBy(g => g.Key))
        {
            priorityNodes.Add(new PriorityNode(
                priorityGroup.Key,
                priorityGroup.Select(t => t.Title).ToList()));
        }
        yield return new CategoryNode(categoryGroup.Key, priorityNodes);
    }
}

// Alternative mentioned in code review:
// board.GroupBy(t => (t.Category, t.Priority))
```

---

#### Q5. (M) Imported tickets allow `Category` to be null when the CSV field is blank. A developer groups and then tries to fetch the "Hardware" bucket with `First`. Review behavior for null keys and the lookup below.

```csharp
List<Ticket> board = LoadFromCsv(); // some rows have Category = null

IEnumerable<IGrouping<string?, Ticket>> byCategory =
    board.GroupBy(t => t.Category);

IGrouping<string?, Ticket> hardware =
    byCategory.First(g => g.Key == "Hardware");

ILookup<string?, Ticket> lookup = board.ToLookup(t => t.Category);
int uncategorized = lookup[null].Count();
bool hasNullBucket = lookup.Contains(null);
```

What happens with null keys, missing "Hardware", and `lookup[null]`?

---

#### Q6. (D) You are designing a ticket-routing service. Two paths are proposed:

- **Path A:** `board.GroupBy(t => t.Assignee)` — deferred, walk groups when building each route batch.
- **Path B:** `board.ToLookup(t => t.Assignee)` — built once after each poll from the queue.

When would you choose each in production (single-pass report vs repeated random access by assignee), and what are the trade-offs for memory, staleness, and missing keys?
