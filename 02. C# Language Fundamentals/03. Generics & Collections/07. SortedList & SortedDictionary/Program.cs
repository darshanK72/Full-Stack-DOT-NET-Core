/*
 * =============================================================================
 * 07. SORTEDLIST AND SORTEDDICTIONARY — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: Generic sorted associative collections — SortedList<TKey,TValue> and
 *        SortedDictionary<TKey,TValue> — key/value maps whose keys stay in
 *        sorted order automatically.
 *
 * WHY IT MATTERS:
 *   Dictionary<TKey,TValue> (chapter 04) and HashSet<T> (chapter 05) give O(1)
 *   average lookup but no predictable key order. When you need alphabetical
 *   reports, ranked leaderboards, or range-style scans ("all keys from A to M"),
 *   sorted collections trade a little speed for built-in order without sorting
 *   on every read.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Sorted maps vs hash-based Dictionary — when order matters
 *   2.  SortedList<TKey,TValue> — array-backed sorted map, Add, indexer
 *   3.  SortedList Keys/Values — O(1) access by sorted rank
 *   4.  SortedList lookup, update, Remove, RemoveAt, IndexOfKey
 *   5.  SortedDictionary<TKey,TValue> — tree-backed sorted map
 *   6.  SortedDictionary Keys/Values and common operations
 *   7.  Custom sort order with IComparer<TKey>
 *   8.  SortedList vs SortedDictionary — when to pick which
 *   9.  Performance vs Dictionary and HashSet
 *   10. Non-generic SortedList (legacy preview)
 *
 * =============================================================================
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace SortedListAndSortedDictionary;

/*
 * =========================================================================
 * SECTION 7: CUSTOM SORT ORDER — IComparer<TKey>
 * =========================================================================
 *
 * Both sorted types accept an optional IComparer<TKey> in the constructor.
 * When omitted, keys compare via Comparer<TKey>.Default (requires
 * IComparable<TKey> on TKey, or IComparable for legacy types).
 *
 *  Approach                         | Example
 *  ---------------------------------|----------------------------------
 *  Built-in string comparer         | StringComparer.OrdinalIgnoreCase
 *  Reverse numeric order            | Comparer<int>.Create((a,b) => b.CompareTo(a))
 *  Domain rule in a class           | IComparer<LeaderboardEntry> (below)
 *
 * IMPORTANT: Sorted collections use COMPARISON order, not GetHashCode.
 * Dictionary/HashSet use IEqualityComparer<T> (hash + equals). Different
 * interfaces for different jobs — do not confuse them.
 *
 * Duplicate keys still throw ArgumentException regardless of comparer.
 * -------------------------------------------------------------------------
 */
public class LeaderboardEntry : IComparable<LeaderboardEntry>
{
    public string Player { get; set; } = string.Empty;
    public int Score { get; set; }

    public int CompareTo(LeaderboardEntry? other)
    {
        if (other is null)
        {
            return 1;
        }

        int byScore = other.Score.CompareTo(Score); // higher score first when used with ScoreDescendingComparer
        return byScore != 0 ? byScore : string.Compare(Player, other.Player, StringComparison.Ordinal);
    }
}

public class ScoreDescendingComparer : IComparer<LeaderboardEntry>
{
    public int Compare(LeaderboardEntry? x, LeaderboardEntry? y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0;
        }

        if (x is null)
        {
            return -1;
        }

        if (y is null)
        {
            return 1;
        }

        int byScore = y.Score.CompareTo(x.Score); // descending by score
        return byScore != 0 ? byScore : string.Compare(x.Player, y.Player, StringComparison.Ordinal);
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("=== 07. SortedList & SortedDictionary ===\n");

        DemoSortedMapsVsDictionary();
        DemoSortedListBasics();
        DemoSortedListIndexedAccess();
        DemoSortedListLookupAndRemove();
        DemoSortedDictionaryBasics();
        DemoSortedDictionaryOperations();
        DemoCustomComparer();
        DemoSortedListVsSortedDictionary();
        DemoPerformanceComparison();
        DemoNonGenericSortedListPreview();
        PrintReorderReport(BuildReorderList());
    }

    /*
     * =========================================================================
     * SECTION 1: SORTED MAPS VS HASH-BASED DICTIONARY
     * =========================================================================
     *
     * Dictionary<TKey,TValue> (chapter 04) uses a hash table. Keys have no
     * inherent order — foreach may visit them in any sequence.
     *
     * SortedList and SortedDictionary keep keys sorted by:
     *   • Natural order when TKey implements IComparable<TKey>, or
     *   • A custom IComparer<TKey> passed to the constructor (section 7).
     *
     *  Collection              | Key order        | Typical lookup
     *  ------------------------|------------------|------------------
     *  Dictionary<K,V>         | None (hash)      | O(1) average
     *  SortedList<K,V>         | Sorted           | O(log n)
     *  SortedDictionary<K,V>   | Sorted           | O(log n)
     *
     * HashSet<T> (chapter 05) stores unique values only — no TValue slot.
     * Use HashSet for membership; use sorted maps when you need key→value
     * pairs in order.
     * -------------------------------------------------------------------------
     */
    private static void DemoSortedMapsVsDictionary()
    {
        Dictionary<string, int> hashOrder = new Dictionary<string, int>
        {
            ["Zulu"] = 3,
            ["Alpha"] = 1,
            ["Mike"] = 2
        };

        SortedDictionary<string, int> sortedOrder = new SortedDictionary<string, int>
        {
            ["Zulu"] = 3,
            ["Alpha"] = 1,
            ["Mike"] = 2
        };

        Console.WriteLine("--- Dictionary foreach (order undefined) ---");
        foreach (KeyValuePair<string, int> pair in hashOrder)
        {
            Console.WriteLine($"  {pair.Key} → {pair.Value}");
        }

        Console.WriteLine("\n--- SortedDictionary foreach (ascending keys) ---");
        foreach (KeyValuePair<string, int> pair in sortedOrder)
        {
            Console.WriteLine($"  {pair.Key} → {pair.Value}");
        }

        Console.WriteLine();
    }

    /*
     * =========================================================================
     * SECTION 2: SortedList<TKey,TValue> — BASICS
     * =========================================================================
     *
     * SortedList stores two parallel arrays: one for keys, one for values.
     * Inserts shift elements to keep sort order. Capacity doubles when full
     * (like List<T> resizing).
     *
     * Keys MUST be unique (like Dictionary). Duplicate Add throws
     * ArgumentException — same as Dictionary.Add, unlike indexer assignment
     * which updates an existing key.
     *
     * --- 2a. Creation and Add ---
     *
     * Keys sort immediately on Add. You never call Sort yourself.
     * -------------------------------------------------------------------------
     */
    private static void DemoSortedListBasics()
    {
        SortedList<string, decimal> skuPrices = new SortedList<string, decimal>();

        skuPrices.Add("WH-9920", 14.50m);
        skuPrices.Add("WH-1100", 8.25m);
        skuPrices.Add("WH-5500", 22.00m);
        skuPrices["WH-3300"] = 11.75m; // indexer adds when key missing, updates when present

        Console.WriteLine("--- SortedList: keys auto-sorted on Add ---");
        foreach (KeyValuePair<string, decimal> pair in skuPrices)
        {
            Console.WriteLine($"  {pair.Key} → {pair.Value:C}");
        }

        Console.WriteLine($"  Count: {skuPrices.Count}");
        Console.WriteLine();
    }

    /*
     * =========================================================================
     * SECTION 3: SortedList — Keys AND Values INDEXED ACCESS
     * =========================================================================
     *
     * SortedList exposes IList<TKey> Keys and IList<TValue> Values.
     * Index i always refers to the i-th key in SORTED order — O(1) random
     * access by rank. SortedDictionary Keys/Values are read-only ICollection
     * with NO indexer — you iterate or use TryGetValue only.
     *
     *   skuPrices.Keys[0]    → lowest key (lexicographically here)
     *   skuPrices.Values[2]  → value paired with Keys[2]
     *
     * IndexOfKey / IndexOfValue use binary search — O(log n).
     * -------------------------------------------------------------------------
     */
    private static void DemoSortedListIndexedAccess()
    {
        SortedList<string, decimal> skuPrices = new SortedList<string, decimal>
        {
            ["WH-9920"] = 14.50m,
            ["WH-1100"] = 8.25m,
            ["WH-5500"] = 22.00m,
            ["WH-3300"] = 11.75m
        };

        Console.WriteLine("--- SortedList: index access via Keys / Values ---");
        Console.WriteLine($"  First SKU (index 0): {skuPrices.Keys[0]} @ {skuPrices.Values[0]:C}");
        Console.WriteLine($"  Capacity: {skuPrices.Capacity}");

        int middleIndex = skuPrices.Count / 2;
        Console.WriteLine($"  Middle entry [{middleIndex}]: {skuPrices.Keys[middleIndex]} → {skuPrices.Values[middleIndex]:C}");

        Console.WriteLine("\n--- Keys collection (already sorted) ---");
        foreach (string sku in skuPrices.Keys)
        {
            Console.WriteLine($"  {sku}");
        }

        Console.WriteLine();
    }

    /*
     * =========================================================================
     * SECTION 4: SortedList — LOOKUP, UPDATE, REMOVE
     * =========================================================================
     *
     * --- 4a. TryGetValue and ContainsKey / ContainsValue ---
     *
     * Same safe lookup pattern as Dictionary (chapter 04). TryGetValue avoids
     * double lookup. ContainsValue scans values — O(n) on SortedList.
     *
     * --- 4b. IndexOfKey / IndexOfValue ---
     *
     * Binary search on the sorted arrays — O(log n).
     *
     * --- 4c. Remove and RemoveAt ---
     *
     * Remove(key) or RemoveAt(index) shifts remaining elements — O(n).
     *
     * Indexer GET with missing key throws KeyNotFoundException (like Dictionary).
     * -------------------------------------------------------------------------
     */
    private static void DemoSortedListLookupAndRemove()
    {
        SortedList<string, decimal> skuPrices = new SortedList<string, decimal>
        {
            ["WH-9920"] = 14.50m,
            ["WH-1100"] = 8.25m,
            ["WH-5500"] = 22.00m,
            ["WH-3300"] = 11.75m
        };

        if (skuPrices.TryGetValue("WH-5500", out decimal bulkPrice))
        {
            Console.WriteLine($"--- TryGetValue: WH-5500 costs {bulkPrice:C} ---");
        }

        Console.WriteLine($"  ContainsKey(\"WH-9999\"): {skuPrices.ContainsKey("WH-9999")}");
        Console.WriteLine($"  ContainsValue(8.25m): {skuPrices.ContainsValue(8.25m)}");

        int keyIndex = skuPrices.IndexOfKey("WH-3300");
        Console.WriteLine($"  IndexOfKey(\"WH-3300\"): {keyIndex}");

        skuPrices.Remove("WH-1100");
        Console.WriteLine($"  After Remove(\"WH-1100\"), Count: {skuPrices.Count}");

        skuPrices.RemoveAt(0); // removes lowest remaining key
        Console.WriteLine($"  After RemoveAt(0), first key: {skuPrices.Keys[0]}");
        Console.WriteLine();
    }

    /*
     * =========================================================================
     * SECTION 5: SortedDictionary<TKey,TValue> — BASICS
     * =========================================================================
     *
     * SortedDictionary uses a red-black tree internally. Keys stay sorted;
     * insert and remove rebalance the tree — no array shifting.
     *
     * There is NO Keys[index] — iterate Keys/Values or use TryGetValue.
     * Duplicate keys on Add still throw ArgumentException.
     *
     * Optional constructor args: IComparer<TKey> for custom sort rules.
     * -------------------------------------------------------------------------
     */
    private static void DemoSortedDictionaryBasics()
    {
        SortedDictionary<string, int> regionSales = new SortedDictionary<string, int>();

        regionSales.Add("West", 4200);
        regionSales.Add("East", 5100);
        regionSales.Add("Central", 3800);
        regionSales["North"] = 2900;

        Console.WriteLine("--- SortedDictionary: foreach in key order ---");
        foreach (KeyValuePair<string, int> entry in regionSales)
        {
            Console.WriteLine($"  {entry.Key,-8} units: {entry.Value:N0}");
        }

        Console.WriteLine($"  Count: {regionSales.Count}");
        Console.WriteLine();
    }

    /*
     * =========================================================================
     * SECTION 6: SortedDictionary — OPERATIONS AND Keys/Values
     * =========================================================================
     *
     * API mirrors Dictionary for day-to-day use:
     *   Add, Remove, ContainsKey, TryGetValue, Count, Clear, indexer.
     *
     * Keys and Values are read-only ICollection<T> — you can foreach them but
     * cannot Add/Remove through those views (same idea as Dictionary).
     *
     * Foreach on Keys visits ascending order. First key = minimum, last = max
     * without LINQ — useful for range endpoints.
     * -------------------------------------------------------------------------
     */
    private static void DemoSortedDictionaryOperations()
    {
        SortedDictionary<string, int> regionSales = new SortedDictionary<string, int>
        {
            ["West"] = 4200,
            ["East"] = 5100,
            ["Central"] = 3800,
            ["North"] = 2900
        };

        string? lowestRegion = null;
        string? highestRegion = null;
        foreach (string region in regionSales.Keys)
        {
            if (lowestRegion == null)
            {
                lowestRegion = region;
            }

            highestRegion = region;
        }

        Console.WriteLine("--- SortedDictionary: first/last key via foreach on Keys ---");
        Console.WriteLine($"  Lowest region key:  {lowestRegion}");
        Console.WriteLine($"  Highest region key: {highestRegion}");

        if (regionSales.TryGetValue("East", out int eastUnits))
        {
            regionSales["East"] = eastUnits + 150; // indexer update
            Console.WriteLine($"  Updated East units: {regionSales["East"]:N0}");
        }

        regionSales.Remove("North");
        Console.WriteLine($"  After Remove(\"North\"), Count: {regionSales.Count}");

        Console.WriteLine("\n--- Values collection (pairs with Keys in same order) ---");
        foreach (int units in regionSales.Values)
        {
            Console.WriteLine($"  {units:N0} units");
        }

        Console.WriteLine();
    }

    /*
     * Wires SECTION 7 types — case-insensitive string keys and score-ranked entries.
     */
    private static void DemoCustomComparer()
    {
        SortedDictionary<string, int> tagsByCount = new SortedDictionary<string, int>(
            StringComparer.OrdinalIgnoreCase)
        {
            ["dotnet"] = 12,
            ["CSharp"] = 8,
            ["LINQ"] = 5,
            ["csharp"] = 99 // indexer updates existing key — comparer treats "CSharp"/"csharp" as same key
        };

        Console.WriteLine("--- Custom comparer: StringComparer.OrdinalIgnoreCase ---");
        foreach (KeyValuePair<string, int> tag in tagsByCount)
        {
            Console.WriteLine($"  {tag.Key} → {tag.Value}");
        }

        ScoreDescendingComparer scoreComparer = new ScoreDescendingComparer();
        SortedList<LeaderboardEntry, string> board = new SortedList<LeaderboardEntry, string>(scoreComparer)
        {
            [new LeaderboardEntry { Player = "Alex", Score = 8800 }] = "Gold",
            [new LeaderboardEntry { Player = "Jordan", Score = 9200 }] = "Gold",
            [new LeaderboardEntry { Player = "Sam", Score = 7500 }] = "Silver"
        };

        Console.WriteLine("\n--- Custom IComparer<LeaderboardEntry>: highest score first ---");
        foreach (KeyValuePair<LeaderboardEntry, string> rank in board)
        {
            Console.WriteLine($"  {rank.Key.Player,-8} {rank.Key.Score,5} → {rank.Value}");
        }

        Console.WriteLine();
    }

    /*
     * =========================================================================
     * SECTION 8: SortedList vs SortedDictionary
     * =========================================================================
     *
     *  Need                              | Prefer
     *  ----------------------------------|----------------------------------
     *  Random access by sorted rank        | SortedList (Keys[i], Values[i])
     *  Frequent insert/remove mid-life     | SortedDictionary (tree, no shift)
     *  Small, mostly read-only maps        | SortedList (cache-friendly arrays)
     *  Large maps with churn               | SortedDictionary
     *  Memory tight, few entries           | SortedList (less tree overhead)
     *
     * Both expose sorted Keys and Values collections. Neither allows duplicate
     * keys. Neither is thread-safe without external locking.
     *
     * SortedList implements IDictionary AND IList on Keys/Values.
     * SortedDictionary is IDictionary only — no index by position.
     * -------------------------------------------------------------------------
     */
    private static void DemoSortedListVsSortedDictionary()
    {
        Console.WriteLine("--- SortedList vs SortedDictionary (see section comment above) ---");
        Console.WriteLine("  SortedList:       Keys[i]/Values[i], shifting inserts, great for small static maps");
        Console.WriteLine("  SortedDictionary: tree rebalance, better when entries churn frequently");
        Console.WriteLine();
    }

    /*
     * =========================================================================
     * SECTION 9: PERFORMANCE VS Dictionary AND HashSet
     * =========================================================================
     *
     *  Operation              | Dictionary | SortedList | SortedDictionary
     *  -----------------------|------------|------------|------------------
     *  Lookup by key          | O(1)*      | O(log n)   | O(log n)
     *  Insert                 | O(1)*      | O(n)†      | O(log n)
     *  Remove by key          | O(1)*      | O(n)†      | O(log n)
     *  Iterate sorted keys    | Sort first | Built-in   | Built-in
     *  Access by sorted index | No         | O(1)       | No
     *
     *  * Average case; worst-case hash collisions degrade Dictionary.
     *  † SortedList insert/remove shifts array elements — costly at scale.
     *
     * HashSet<T> is for unique elements only — no values. Use HashSet when you
     * need membership tests, not key→value mapping.
     *
     * Rule of thumb:
     *   • Need fastest lookup, order irrelevant → Dictionary / HashSet
     *   • Need sorted keys + index by rank → SortedList
     *   • Need sorted keys + frequent updates → SortedDictionary
     * -------------------------------------------------------------------------
     */
    private static void DemoPerformanceComparison()
    {
        const int itemCount = 5000;
        Random random = new Random(42);

        Dictionary<int, string> hashMap = new Dictionary<int, string>(itemCount);
        SortedList<int, string> sortedListBench = new SortedList<int, string>(itemCount);
        SortedDictionary<int, string> sortedDictBench = new SortedDictionary<int, string>();
        HashSet<int> membershipOnly = new HashSet<int>(itemCount);

        for (int i = 0; i < itemCount; i++)
        {
            int key = random.Next(itemCount * 2);
            string label = $"item-{key}";

            hashMap.TryAdd(key, label);
            membershipOnly.Add(key);

            if (!sortedListBench.ContainsKey(key))
            {
                sortedListBench.Add(key, label);
            }

            if (!sortedDictBench.ContainsKey(key))
            {
                sortedDictBench.Add(key, label);
            }
        }

        int probeKey = itemCount / 2;
        Stopwatch sw = Stopwatch.StartNew();
        for (int i = 0; i < 100_000; i++)
        {
            hashMap.TryGetValue(probeKey, out _);
        }

        long dictionaryTicks = sw.ElapsedTicks;
        sw.Restart();
        for (int i = 0; i < 100_000; i++)
        {
            sortedListBench.TryGetValue(probeKey, out _);
        }

        long sortedListTicks = sw.ElapsedTicks;
        sw.Restart();
        for (int i = 0; i < 100_000; i++)
        {
            sortedDictBench.TryGetValue(probeKey, out _);
        }

        long sortedDictionaryTicks = sw.ElapsedTicks;
        sw.Restart();
        for (int i = 0; i < 100_000; i++)
        {
            membershipOnly.Contains(probeKey);
        }

        long hashSetTicks = sw.ElapsedTicks;

        Console.WriteLine("--- Performance snapshot (100k lookups, ~5k keys) ---");
        Console.WriteLine($"  Dictionary TryGetValue ticks:       {dictionaryTicks}");
        Console.WriteLine($"  SortedList TryGetValue ticks:       {sortedListTicks}");
        Console.WriteLine($"  SortedDictionary TryGetValue ticks: {sortedDictionaryTicks}");
        Console.WriteLine($"  HashSet Contains ticks:             {hashSetTicks}");
        Console.WriteLine("  (Lower is faster; ratio varies by machine and n.)");
        Console.WriteLine();
    }

    /*
     * =========================================================================
     * SECTION 10: NON-GENERIC SortedList (PREVIEW)
     * =========================================================================
     *
     * System.Collections.SortedList (non-generic) predates generics. It stores
     * object keys and object values — value types BOX on insert and UNBOX on
     * read. Prefer SortedList<TKey,TValue> in modern code.
     *
     * COVERED IN DETAIL LATER → 02. ArrayList (legacy collections)
     *
     * Shown here only to recognize legacy APIs in older codebases.
     * -------------------------------------------------------------------------
     */
    private static void DemoNonGenericSortedListPreview()
    {
        SortedList legacyRanks = new SortedList();
        legacyRanks.Add("Charlie", 3);
        legacyRanks.Add("Alice", 1);
        legacyRanks.Add("Bob", 2);

        Console.WriteLine("--- Non-generic SortedList (preview) ---");
        for (int i = 0; i < legacyRanks.Count; i++)
        {
            Console.WriteLine($"  [{i}] {legacyRanks.GetKey(i)} → {legacyRanks.GetByIndex(i)}");
        }

        Console.WriteLine();
    }

    /*
     * =========================================================================
     * SECTION 11: RUNNABLE DEMO — REORDER REPORT BY SKU
     * =========================================================================
     *
     * Warehouse scenario: SKUs and reorder quantities print in SKU order
     * without an explicit Sort call — the collection enforces order.
     * -------------------------------------------------------------------------
     */
    private static SortedList<string, int> BuildReorderList()
    {
        SortedList<string, int> reorderQty = new SortedList<string, int>();
        reorderQty.Add("ZEBRA-CLIP", 40);
        reorderQty.Add("ALPHA-PAD", 120);
        reorderQty.Add("MICRO-USB", 85);
        reorderQty.Add("BETA-INK", 200);
        return reorderQty;
    }

    private static void PrintReorderReport(SortedList<string, int> reorderQty)
    {
        Console.WriteLine("=== Reorder report (sorted by SKU) ===");
        for (int i = 0; i < reorderQty.Count; i++)
        {
            Console.WriteLine($"  {reorderQty.Keys[i],-12} qty: {reorderQty.Values[i],4}");
        }
    }
}

/*
 * =========================================================================
 * QUICK REFERENCE — SortedList & SortedDictionary
 * =========================================================================
 *
 * --- Creation ---
 *
 *   new SortedList<TKey,TValue>()
 *   new SortedList<TKey,TValue>(capacity)
 *   new SortedList<TKey,TValue>(IComparer<TKey>)
 *   new SortedList<TKey,TValue>(IDictionary<TKey,TValue>)
 *
 *   new SortedDictionary<TKey,TValue>()
 *   new SortedDictionary<TKey,TValue>(IComparer<TKey>)
 *   new SortedDictionary<TKey,TValue>(IDictionary<TKey,TValue>)
 *
 * --- Common members (both) ---
 *
 *   Add(key, value)           throws ArgumentException if key exists
 *   this[key]                 get/set (set adds or updates)
 *   TryGetValue(key, out val) safe lookup — no exception when missing
 *   ContainsKey(key)
 *   Remove(key)
 *   Count, Clear
 *   foreach / Keys / Values   ascending key order
 *
 * --- SortedList only ---
 *
 *   Keys[i], Values[i]        O(1) access by sorted rank
 *   IndexOfKey(key)           O(log n)
 *   IndexOfValue(value)       O(log n)
 *   RemoveAt(index)           O(n) — shifts arrays
 *   ContainsValue(value)      O(n) linear scan
 *   Capacity, TrimExcess()
 *
 * --- Custom sort ---
 *
 *   IComparer<TKey> ctor      StringComparer.OrdinalIgnoreCase, Comparer<T>.Create, custom class
 *   Not IEqualityComparer     that is for Dictionary / HashSet
 *
 * --- Pick the type ---
 *
 *   Dictionary        fastest lookup, no order
 *   HashSet           unique values only (no pairs)
 *   SortedList        sorted + index by rank, small/mostly static
 *   SortedDictionary  sorted + frequent inserts/removes
 *
 * --- Legacy preview ---
 *
 *   System.Collections.SortedList   object/object, boxing — avoid in new code
 *
 * --- Common mistakes ---
 *
 *  Mistake                         | Result
 *  --------------------------------|----------------------------------
 *  Add duplicate key               | ArgumentException
 *  Keys[i] on SortedDictionary     | Not available — use foreach
 *  Assume Dictionary foreach order | Order undefined — use sorted type
 *  IEqualityComparer on sorted map | Wrong interface — use IComparer<TKey>
 *  Non-generic SortedList for ints | Boxing overhead, cast on read
 *
 * =========================================================================
 */
