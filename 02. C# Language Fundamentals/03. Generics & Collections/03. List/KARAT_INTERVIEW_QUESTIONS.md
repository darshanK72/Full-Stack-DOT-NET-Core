# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/03. Generics & Collections/03. List`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A nightly import job loads 500,000 shipment SKUs into a `List<string>` by calling `Add` one at a time in a loop. Memory profiling shows repeated large allocations and GC pressure. Review the pattern below. What is happening internally, and how would you fix it?

```csharp
public static List<string> LoadSkusFromFeed(IEnumerable<string> feedLines)
{
    var skus = new List<string>();
    foreach (var line in feedLines)
    {
        skus.Add(line.Trim());
    }
    return skus;
}
```

---

#### Q2. (R) A warehouse service removes cancelled dock labels during iteration. In staging it throws intermittently. Review this method — what breaks, and what is the correct fix?

```csharp
public void PurgeCancelledLabels(List<string> dockLabels, HashSet<string> cancelled)
{
    foreach (string label in dockLabels)
    {
        if (cancelled.Contains(label))
        {
            dockLabels.Remove(label);
        }
    }
}
```

---

#### Q3. (M) A shipment validator checks whether each incoming pallet's SKU already exists in a queue of 50,000 items by calling `IndexOf` inside a loop. What is the performance problem, and what structure would you use instead?

```csharp
public bool AllSkusAlreadyQueued(List<ShipmentItem> queue, IEnumerable<ShipmentItem> incoming)
{
    foreach (var item in incoming)
    {
        if (queue.IndexOf(item) < 0)
        {
            return false;
        }
    }
    return true;
}
```

*(Assume `ShipmentItem` does not override `Equals` / `GetHashCode`.)*

---

#### Q4. (D) Two developers search a pallet-count list for the first value over 20. One uses `List.Find`; the other uses LINQ `FirstOrDefault`. When would you prefer each, and what subtle difference matters for value types?

```csharp
List<int> palletCounts = GetPalletCounts();

int a = palletCounts.Find(n => n > 20);
int b = palletCounts.FirstOrDefault(n => n > 20);
```

---

#### Q5. (R) A `ShipmentQueueService` exposes its internal lane list directly to API callers. Review the property and usage — what can go wrong in production, and how would you expose the data safely?

```csharp
public class ShipmentQueueService
{
    private readonly List<string> _lanes = new() { "Lane-1", "Lane-2" };

    public List<string> Lanes => _lanes;

    public void Reassign(string sku, string lane)
    {
        _lanes.Add(lane);
    }
}

// Controller
var lanes = _queueService.Lanes;
lanes.Clear();
lanes.Add("Hijacked-Lane");
```

---

#### Q6. (P) A singleton background worker and several API threads share one static `List<ShipmentItem>` for the live shipment queue. Under load, counts become wrong and the process occasionally throws. Explain why `List<T>` is unsafe here and what pattern you would use instead.

```csharp
public static class ShipmentHub
{
    public static readonly List<ShipmentItem> LiveQueue = new();

    public static void Enqueue(ShipmentItem item) => LiveQueue.Add(item);

    public static void ProcessNext()
    {
        if (LiveQueue.Count > 0)
        {
            var next = LiveQueue[0];
            LiveQueue.RemoveAt(0);
            Ship(next);
        }
    }
}
```
