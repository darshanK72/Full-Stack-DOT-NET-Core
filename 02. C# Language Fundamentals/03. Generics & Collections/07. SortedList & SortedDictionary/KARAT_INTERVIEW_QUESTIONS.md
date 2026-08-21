# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/03. Generics & Collections/07. SortedList & SortedDictionary`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A warehouse dashboard prints the lowest and highest SKU from a live price map. Review:

```csharp
SortedDictionary<string, decimal> skuPrices = LoadAllSkuPrices(); // ~12_000 entries

string lowestSku = skuPrices.Keys[0];
string highestSku = skuPrices.Keys[skuPrices.Count - 1];

Console.WriteLine($"Range: {lowestSku} … {highestSku}");
```

Build fails. A teammate suggests switching to `Dictionary<string, decimal>` and sorting keys with LINQ on every page load. What is wrong with the original code, and what is the better fix that keeps sorted key order?

---

#### Q2. (R) An inventory sync service upserts pallet counts every few seconds. Review the hot path:

```csharp
public sealed class PalletSyncService
{
    private readonly SortedList<int, int> _countsByZone = new(capacity: 500);

    public void UpsertZoneCount(int zoneId, int palletCount)
    {
        if (_countsByZone.ContainsKey(zoneId))
            _countsByZone[zoneId] = palletCount;
        else
            _countsByZone.Add(zoneId, palletCount);
    }
}

// Startup loads 500 zones; sync runs 40 upserts/sec with mixed new and existing zone IDs.
```

Latency spikes after deploy though lookups are fast. What collection cost dominates here, and what type swap fixes churn without losing sorted iteration?

---

#### Q3. (D) You expose a `/regions/sales` JSON endpoint. Product wants keys returned alphabetically by region code. Two proposals:

**A.** `Dictionary<string, int>` — load from SQL, then `OrderBy(k => k.Key).ToDictionary()` before serialize.

**B.** `SortedDictionary<string, int>` — insert as rows stream in; foreach already ascending.

The map holds ~30 regions, refreshes once per hour, and serves ~2k requests/min with read-heavy traffic. Which backing store do you pick, and what breaks if you choose wrong?

---

#### Q4. (R) A catalog search feature stores product tags in a case-insensitive sorted map. QA reports duplicate logical tags after a Turkish-locale server deploy. Review:

```csharp
var tagsByCount = new SortedDictionary<string, int>(
    StringComparer.CurrentCulture)
{
    ["dotnet"] = 12,
    ["CSharp"] = 8,
    ["LINQ"] = 5
};

tagsByCount["csharp"] = 99; // developer expects this to update "CSharp"

foreach (var tag in tagsByCount)
    Console.WriteLine($"{tag.Key} → {tag.Value}");
```

What comparer behavior caused the surprise, and what comparer + API pattern matches the chapter's `StringComparer.OrdinalIgnoreCase` demo?

---

#### Q5. (M) A pricing microservice benchmarks three shapes for a nightly job that inserts 50_000 random SKUs once, then performs 500_000 lookups:

```csharp
var hash = new Dictionary<int, decimal>(50_000);
var sortedList = new SortedList<int, decimal>(50_000);
var sortedDict = new SortedDictionary<int, decimal>();

for (int i = 0; i < 50_000; i++)
{
    int key = Random.Shared.Next(100_000);
    hash.TryAdd(key, 1.99m);
    if (!sortedList.ContainsKey(key)) sortedList.Add(key, 1.99m);
    if (!sortedDict.ContainsKey(key)) sortedDict.Add(key, 1.99m);
}

// Then 500_000 TryGetValue / Contains loops on each collection...
```

`SortedList` dominates wall-clock time during the load phase. Explain **why** insert is asymptotically worse than the other two, and state the rule of thumb from this chapter for pick-list vs tree-backed sorted maps.

---

#### Q6. (R) A developer ports a `Dictionary` helper to sorted collections but copies the wrong comparer interface. Review:

```csharp
public sealed class SkuIgnoreCaseEquality : IEqualityComparer<string>
{
    public bool Equals(string? x, string? y) =>
        string.Equals(x, y, StringComparison.OrdinalIgnoreCase);

    public int GetHashCode(string obj) =>
        StringComparer.OrdinalIgnoreCase.GetHashCode(obj);
}

var reorderQty = new SortedDictionary<string, int>(new SkuIgnoreCaseEquality())
{
    ["ZEBRA-CLIP"] = 40,
    ["ALPHA-PAD"] = 120
};

reorderQty["alpha-pad"] = 200;
```

What fails at compile time, what would fail at runtime if they forced it to compile, and how do you wire case-insensitive **sort order** correctly for `SortedList` / `SortedDictionary`?
