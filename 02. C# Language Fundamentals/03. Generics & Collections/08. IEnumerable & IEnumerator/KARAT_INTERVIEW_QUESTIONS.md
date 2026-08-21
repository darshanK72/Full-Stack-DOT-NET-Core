# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/03. Generics & Collections/08. IEnumerable & IEnumerator`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A warehouse API returns `IEnumerable<PickLine>` from a `yield return` filter. A report job calls `Count()` then `Sum()` on the same reference without materializing. Totals disagree with the pick ticket and logs show the database query ran twice. Review the service method and caller. What went wrong, and how do you fix it?

```csharp
public IEnumerable<PickLine> GetHeavyLines(string ticketId, decimal minKg)
{
    foreach (PickLine line in _repository.LoadLines(ticketId)) // hits DB per enumeration
    {
        if (line.TotalWeightKg >= minKg)
            yield return line;
    }
}

// ReportJob:
var heavy = _service.GetHeavyLines("PB-2201", 1.0m);
int lineCount = heavy.Count();
decimal totalKg = heavy.Sum(l => l.TotalWeightKg);
_logger.LogInformation("Heavy lines: {Count}, total kg: {Total}", lineCount, totalKg);
```

---

#### Q2. (R) A custom `IEnumerator<PickLine>` wraps a file reader. A developer copies the manual loop from a tutorial but drops the `using` block. Under load, temp files pile up on disk. Review the loop. What is missing, and what does `foreach` do differently?

```csharp
public sealed class PickLineFileEnumerator : IEnumerator<PickLine>
{
    private readonly StreamReader _reader;
    private PickLine? _current;

    public PickLineFileEnumerator(string path)
    {
        _reader = new StreamReader(path);
    }

    public PickLine Current => _current!;
    public bool MoveNext() { /* read next line into _current */ return _current != null; }
    public void Dispose() => _reader.Dispose();
}

// Caller:
IEnumerator<PickLine> walk = batch.GetEnumerator();
while (walk.MoveNext())
{
    Process(walk.Current);
    if (walk.Current.Sku.StartsWith("STOP"))
        break; // exit early on first bad aisle
}
// walk goes out of scope here — no Dispose call
```

---

#### Q3. (R) A batch-picking screen tries to skip short lines by removing them while iterating. It crashes on the second line every time. Review the loop (same pattern as **Program.cs** Section 4g). What throws, why is it allowed, and what is the safe fix?

```csharp
List<PickLine> lines = _ticket.Lines.ToList();

foreach (PickLine line in lines)
{
    if (line.Quantity < 5)
        lines.Remove(line); // shrink list during foreach
    else
        _picker.Assign(line);
}
```

---

#### Q4. (M) A developer builds a lazy LINQ pipeline over live pick lines, logs the count, then mutates the underlying list before a second `foreach`. Results differ between the two passes. Walk through what runs when and why the second pass can change.

```csharp
List<PickLine> pickList = LoadOpenTicket("PB-2201");

IEnumerable<PickLine> heavy = pickList
    .Where(l => l.TotalWeightKg >= 1.0m); // deferred — no filter yet

int previewCount = heavy.Count(); // first full enumeration

pickList.Add(new PickLine("RUSH-ADD", 1, 2.5m)); // mutates source between passes

foreach (PickLine line in heavy) // second enumeration — different sequence
{
    Console.WriteLine(line.Sku);
}
```

What surprises a developer who assumes `heavy` is a snapshot?

---

#### Q5. (M) An iterator method logs each SKU as it yields. A caller breaks out of `foreach` after the first match. Later code assumes every line was scanned. Review the iterator and caller. What does `yield return` guarantee about execution state, and when does work *not* run?

```csharp
private static IEnumerable<PickLine> FirstMatchPerAisle(IEnumerable<PickLine> source)
{
    var seenAisles = new HashSet<string>();

    foreach (PickLine line in source)
    {
        string aisle = line.Sku[..1];
        if (seenAisles.Add(aisle))
        {
            _metrics.RecordScan(line.Sku); // side effect on each yield
            yield return line;
        }
    }
}

// Caller:
foreach (PickLine line in FirstMatchPerAisle(pickList))
{
    Ship(line);
    break; // stop after first aisle representative
}
// Ops dashboard shows 1 scan; warehouse expected full ticket walk
```

---

#### Q6. (P) A code review flags `var lines = GetHeavyLines(...).ToList()` as "unnecessary allocation." The author argues it prevents double DB hits and stabilizes results if the ticket changes mid-request. When is `ToList()` (or `ToArray()`) the right production fix for `IEnumerable<T>`, and when is it waste?

Consider: single `foreach`, multiple LINQ passes, `IEnumerable<T>` returned from repositories, and ASP.NET request-scoped mutation of shared lists.

---

#### Q7. (R) Two developers iterate the same `PickBatch` concurrently — one with `foreach`, one with a stored `IEnumerator<PickLine>` from an earlier `GetEnumerator()` call. Intermittent duplicates and skipped SKUs appear. Review `PickBatch` (fresh enumerator per `GetEnumerator()`). What contract did the second developer violate, and how should multiple consumers walk the same batch?

```csharp
PickBatch batch = new PickBatch(/* lines */);

IEnumerator<PickLine> manual = batch.GetEnumerator();
manual.MoveNext(); // advanced once manually

foreach (PickLine line in batch) // second cursor — OK alone
{
    Process(line);
}

while (manual.MoveNext()) // first cursor still mid-stream
{
    Process(manual.Current); // overlaps with foreach timing in other threads
}
```
