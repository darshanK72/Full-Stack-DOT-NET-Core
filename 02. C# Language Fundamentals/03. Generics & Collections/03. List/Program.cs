/*
 * =============================================================================
 * 03. List<T> — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: List<T> — the generic, resizable collection for ordered sequences of
 *        items. Full CRUD, search and sort helpers, sorting custom types,
 *        read-only views, conversions, and when to choose List over array.
 *
 * WHY IT MATTERS:
 *   Arrays have a fixed length once created. Real programs grow and shrink
 *   collections constantly — cart lines, shipment items, log entries. List<T>
 *   gives type-safe, dynamic storage without the boxing and casting pain of
 *   legacy ArrayList (see 02. ArrayList).
 *
 * WHAT YOU WILL LEARN:
 *   1.  Creating List<T> and collection initializers
 *   2.  CRUD: Add, AddRange, Insert, Remove, RemoveAt, RemoveAll, Clear, indexer
 *   3.  Count, Capacity growth, Contains, IndexOf
 *   4.  Sort, Reverse, Find, FindAll, Exists, ForEach
 *   5.  Sorting complex types: IComparable<T>, IComparer<T>, Comparison<T>
 *   6.  AsReadOnly, ConvertAll, ToArray, CopyTo, array ↔ List
 *   7.  List<T> vs array — when to use each
 *   8.  LinkedList<T> (preview)
 *   9.  Dictionary<TKey,TValue> (preview)
 *
 * Scenario: warehouse shipment queue — SKUs, priorities, and dock labels.
 *
 * =============================================================================
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ListDemo;

/*
 * =========================================================================
 * SECTION 1: WHAT IS List<T>?
 * =========================================================================
 *
 * List<T> lives in System.Collections.Generic. T is the element type — the
 * compiler knows every slot holds a T (no boxing for value types, no cast
 * on read).
 *
 *   List<string> dockLabels = new List<string>();
 *   List<int> palletCounts = new List<int> { 12, 8, 15 };  // collection initializer
 *
 * Internally List<T> wraps a resizable array. When Count exceeds Capacity,
 * it allocates a larger backing array and copies elements (typically doubling
 * capacity — amortized O(1) for Add at the end).
 *
 * Generics fundamentals (constraints, why T) →
 * COVERED IN DETAIL LATER → 01. Generics
 * -------------------------------------------------------------------------
 */
public partial class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("=== 03. List<T> ===");

        DemoBasics();
        DemoCrud();
        DemoCountCapacityAndSearch();
        DemoSortReverseAndPredicates();
        DemoNaturalSortWithIComparable();
        DemoSortWithIComparer();
        DemoSortWithComparisonDelegate();
        DemoAsReadOnly();
        DemoConversions();
        DemoListVsArray();
        DemoLinkedListPreview();
        DemoDictionaryPreview();
    }

    /*
     * --- 1a. Create and initialize ---
     */
    public static void DemoBasics()
    {
        List<string> dockLabels = new List<string> { "Dock-A", "Dock-B", "Dock-C" };

        Console.WriteLine();
        Console.WriteLine("--- Section 1: basics ---");
        Console.WriteLine($"Initial dock labels ({dockLabels.Count}): {string.Join(", ", dockLabels)}");
    }

    /*
     * =========================================================================
     * SECTION 2: CRUD OPERATIONS
     * =========================================================================
     *
     * CRUD = Create (add items), Read (indexer / iteration), Update (indexer
     * assignment), Delete (Remove / RemoveAt / RemoveAll / Clear).
     *
     *  Operation          | API                           | Notes
     *  -------------------|-------------------------------|---------------------------
     *  Add at end         | list.Add(item)                | Most common insert
     *  Add many at end    | list.AddRange(collection)     | One resize for batch
     *  Insert at index    | list.Insert(index, item)      | Shifts elements right
     *  Read by index      | list[i]                       | Zero-based; throws if bad index
     *  Update by index    | list[i] = newValue            | Replaces slot, does not grow
     *  Remove by value    | list.Remove(item)             | First match only; returns bool
     *  Remove by index    | list.RemoveAt(index)          | Shifts elements left
     *  Remove by rule     | list.RemoveAll(predicate)     | Removes all matches; returns count
     *  Clear all          | list.Clear()                  | Count → 0; Capacity unchanged
     *
     * Remove(object) uses EqualityComparer<T>.Default — reference types compare
     * by reference unless you override Equals. Value types compare by value.
     *
     * Bad index on list[i] or RemoveAt → ArgumentOutOfRangeException.
     * -------------------------------------------------------------------------
     */
    public static void DemoCrud()
    {
        List<string> dockLabels = new List<string> { "Dock-A", "Dock-B", "Dock-C" };

        dockLabels.Add("Dock-D");
        dockLabels.AddRange(new[] { "Dock-E", "Dock-F" }); // batch append from array
        dockLabels.Insert(1, "Dock-Alpha");

        string firstLabel = dockLabels[0];           // read by indexer
        dockLabels[0] = "Dock-A (renamed)";            // update by indexer

        bool removedDockB = dockLabels.Remove("Dock-B");
        dockLabels.RemoveAt(dockLabels.Count - 1);     // remove last element

        List<int> palletCounts = new List<int> { 42, 8, 15, 8, 30, 8 };
        int removedEights = palletCounts.RemoveAll(n => n == 8); // remove every 8

        dockLabels.Clear(); // Count → 0; Capacity stays allocated

        Console.WriteLine();
        Console.WriteLine("--- Section 2: CRUD ---");
        Console.WriteLine($"First label after rename: {firstLabel} → now Dock-A (renamed)");
        Console.WriteLine($"Removed 'Dock-B': {removedDockB}");
        Console.WriteLine($"RemoveAll removed {removedEights} eights; remaining counts: {string.Join(", ", palletCounts)}");
        Console.WriteLine($"After Clear: dock label Count={dockLabels.Count}, Capacity={dockLabels.Capacity}");
    }

    /*
     * =========================================================================
     * SECTION 3: COUNT, CAPACITY, CONTAINS, INDEXOF
     * =========================================================================
     *
     *  Property / method | Meaning
     *  -------------------|----------------------------------------------------
     *  Count               | Elements currently stored (logical size)
     *  Capacity            | Backing array length (≥ Count; grows automatically)
     *  Contains(item)      | true if any element equals item (same rules as Remove)
     *  IndexOf(item)       | First index, or -1 if not found
     *
     * Capacity grows when Add/AddRange needs more room — usually doubles (e.g.
     * 0 → 4 → 8 → 16). Capacity does not shrink on Remove/Clear; call
     * TrimExcess() after bulk deletes to release unused backing array memory.
     * -------------------------------------------------------------------------
     */
    public static void DemoCountCapacityAndSearch()
    {
        List<int> growth = new List<int>();
        List<int> capacitySteps = new List<int>();

        for (int i = 0; i < 20; i++)
        {
            int previousCapacity = growth.Capacity;
            growth.Add(i);
            if (growth.Capacity != previousCapacity)
            {
                capacitySteps.Add(growth.Capacity); // record each resize
            }
        }

        List<string> dockLabels = new List<string> { "Dock-Alpha", "Dock-B", "Dock-C" };
        bool hasAlpha = dockLabels.Contains("Dock-Alpha");
        int indexOfBeta = dockLabels.IndexOf("Dock-B");

        Console.WriteLine();
        Console.WriteLine("--- Section 3: Count / Capacity / Contains / IndexOf ---");
        Console.WriteLine($"After 20 Adds: Count={growth.Count}, final Capacity={growth.Capacity}");
        Console.WriteLine($"Capacity grew through: {string.Join(" → ", capacitySteps)}");
        Console.WriteLine($"Contains Dock-Alpha={hasAlpha}; IndexOf Dock-B={indexOfBeta}");
    }

    /*
     * =========================================================================
     * SECTION 4: SORT, REVERSE, FIND, FINDALL, EXISTS, FOREACH
     * =========================================================================
     *
     * Sort() and Reverse() mutate the list IN PLACE and return void.
     *
     *   list.Sort();     // ascending; requires IComparable<T> or use overload
     *   list.Reverse();  // reverses current order (not necessarily sorted)
     *
     * For int, string, DateTime, etc., Sort() uses the type's default comparer.
     * Strings sort ordinally by default (Unicode code-point order).
     *
     * Search without removing:
     *
     *  Method              | Returns
     *  --------------------|---------------------------------------------------
     *  Find(predicate)     | First match or default(T) if none
     *  FindAll(predicate)  | New List<T> of all matches
     *  Exists(predicate)   | bool — any match?
     *  ForEach(action)     | Invokes Action<T> on each element (void return)
     *
     * Predicate = Func<T, bool>. ForEach takes Action<T> — side effects only.
     * -------------------------------------------------------------------------
     */
    public static void DemoSortReverseAndPredicates()
    {
        List<int> palletCounts = new List<int> { 42, 8, 15, 8, 30 };

        palletCounts.Sort();
        palletCounts.Reverse();

        int firstEight = palletCounts.Find(n => n == 8);
        List<int> allEights = palletCounts.FindAll(n => n == 8);
        bool anyOverTwenty = palletCounts.Exists(n => n > 20);
        int indexOfThirty = palletCounts.IndexOf(30);

        List<string> scanLog = new List<string>();
        palletCounts.ForEach(n => scanLog.Add($"scanned:{n}")); // Action<T> per item

        Console.WriteLine();
        Console.WriteLine("--- Section 4: Sort / Reverse / Find / ForEach ---");
        Console.WriteLine($"Sorted then reversed counts: {string.Join(", ", palletCounts)}");
        Console.WriteLine($"Find first 8 → {firstEight}; FindAll 8 → [{string.Join(", ", allEights)}]");
        Console.WriteLine($"Exists > 20: {anyOverTwenty}; IndexOf 30: {indexOfThirty}");
        Console.WriteLine($"ForEach log: {string.Join(", ", scanLog)}");
    }
}

/*
 * =========================================================================
 * SECTION 5: COMPLEX TYPE — ShipmentItem WITH IComparable<T>
 * =========================================================================
 *
 * When T is your own class, parameterless Sort() needs a NATURAL ORDER:
 * implement IComparable<T> and CompareTo(T other).
 *
 * CompareTo contract (same idea as int.CompareTo):
 *   negative  → this sorts before other
 *   zero      → equal for sort purposes
 *   positive  → this sorts after other
 *
 * If T does not implement IComparable<T>, parameterless Sort() throws
 * InvalidOperationException at runtime — use Sort(Comparison<T>) or
 * Sort(IComparer<T>) instead (Sections 6–7).
 * -------------------------------------------------------------------------
 */
public class ShipmentItem : IComparable<ShipmentItem>
{
    public string Sku { get; }
    public int Quantity { get; set; }
    public int Priority { get; set; }

    public ShipmentItem(string sku, int quantity, int priority)
    {
        Sku = sku;
        Quantity = quantity;
        Priority = priority;
    }

    public int CompareTo(ShipmentItem? other)
    {
        if (other is null)
        {
            return 1; // non-null sorts after null in this demo
        }

        return Priority.CompareTo(other.Priority); // lower priority value ships first
    }

    public override string ToString() => $"{Sku} ×{Quantity} (priority {Priority})";
}

public partial class Program
{
    public static void DemoNaturalSortWithIComparable()
    {
        List<ShipmentItem> shipmentQueue = new List<ShipmentItem>
        {
            new ShipmentItem("WH-4412", 36, priority: 3),
            new ShipmentItem("WH-9901", 12, priority: 1),
            new ShipmentItem("WH-2200", 8, priority: 2)
        };

        shipmentQueue.Sort(); // uses ShipmentItem.CompareTo (by Priority)

        Console.WriteLine();
        Console.WriteLine("--- Section 5: IComparable<T> natural sort (by priority) ---");
        PrintShipmentItems(shipmentQueue);
    }
}

/*
 * =========================================================================
 * SECTION 6: SORT WITH IComparer<T>
 * =========================================================================
 *
 * IComparer<T> separates comparison logic from the domain type:
 *
 *   interface IComparer<in T>
 *   {
 *       int Compare(T? x, T? y);
 *   }
 *
 * Use when:
 *   • The class should not implement IComparable<T> (multiple sort orders).
 *   • Comparison belongs in a reusable helper class.
 *   • You want Sort(IComparer<T>) without mutating natural order elsewhere.
 *
 *   list.Sort(new ShipmentItemSkuComparer());
 *
 * Return value same as CompareTo: negative / zero / positive.
 * -------------------------------------------------------------------------
 */
public class ShipmentItemSkuComparer : IComparer<ShipmentItem>
{
    public int Compare(ShipmentItem? x, ShipmentItem? y)
    {
        if (x is null)
        {
            return y is null ? 0 : -1;
        }

        if (y is null)
        {
            return 1;
        }

        return string.Compare(x.Sku, y.Sku, StringComparison.Ordinal);
    }
}

public partial class Program
{
    public static void DemoSortWithIComparer()
    {
        List<ShipmentItem> shipmentQueue = CreateSampleShipmentQueue();
        shipmentQueue.Sort(new ShipmentItemSkuComparer()); // external comparer object

        Console.WriteLine();
        Console.WriteLine("--- Section 6: IComparer<T> (by SKU ascending) ---");
        PrintShipmentItems(shipmentQueue);
    }
}

/*
 * =========================================================================
 * SECTION 7: Comparison<T> DELEGATE
 * =========================================================================
 *
 * Comparison<T> is a delegate type:
 *
 *   delegate int Comparison<in T>(T x, T y);
 *
 * Sort(Comparison<T> comparison) sorts using YOUR rule without a separate
 * IComparer class — handy for one-off sorts or when you already have a method.
 *
 * --- 7a. Lambda ---
 *
 *   list.Sort((a, b) => b.Quantity.CompareTo(a.Quantity));  // descending qty
 *
 * --- 7b. Method group ---
 *
 *   list.Sort(CompareShipmentBySku);
 *   static int CompareShipmentBySku(ShipmentItem a, ShipmentItem b) => …
 *
 * Return value same as CompareTo: negative / zero / positive.
 * -------------------------------------------------------------------------
 */
public partial class Program
{
    public static void DemoSortWithComparisonDelegate()
    {
        List<ShipmentItem> byQuantity = CreateSampleShipmentQueue();
        byQuantity.Sort((a, b) => b.Quantity.CompareTo(a.Quantity)); // lambda — largest first

        List<ShipmentItem> bySku = CreateSampleShipmentQueue();
        bySku.Sort(CompareShipmentBySku); // method group — same rule as Section 6

        Console.WriteLine();
        Console.WriteLine("--- Section 7: Comparison<T> ---");
        Console.WriteLine("By quantity (descending, lambda):");
        PrintShipmentItems(byQuantity);
        Console.WriteLine("By SKU (ascending, method group):");
        PrintShipmentItems(bySku);
    }

    public static int CompareShipmentBySku(ShipmentItem a, ShipmentItem b)
    {
        return string.Compare(a.Sku, b.Sku, StringComparison.Ordinal);
    }
}

/*
 * =========================================================================
 * SECTION 8: AsReadOnly — READ-ONLY VIEW
 * =========================================================================
 *
 * AsReadOnly() wraps the list in a ReadOnlyCollection<T> — callers can read
 * and iterate but cannot Add, Remove, or assign through the wrapper's indexer.
 *
 * The underlying List<T> is still mutable; changes to the source list are
 * visible through the read-only view. Use when exposing data to consumers
 * who should not modify your internal collection.
 *
 * Mutating through the wrapper → NotSupportedException.
 * -------------------------------------------------------------------------
 */
public partial class Program
{
    public static void DemoAsReadOnly()
    {
        List<string> internalLanes = new List<string> { "Lane-1", "Lane-2", "Lane-3" };
        ReadOnlyCollection<string> publishedLanes = internalLanes.AsReadOnly();

        internalLanes.Add("Lane-4"); // source list still mutable

        Console.WriteLine();
        Console.WriteLine("--- Section 8: AsReadOnly ---");
        Console.WriteLine($"Read-only view Count={publishedLanes.Count}: {string.Join(", ", publishedLanes)}");
        Console.WriteLine("(Adding via publishedLanes.Add would throw NotSupportedException)");
    }
}

/*
 * =========================================================================
 * SECTION 9: CONVERSIONS — ConvertAll, ToArray, CopyTo, CONSTRUCTOR
 * =========================================================================
 *
 * --- List → new List (transform) ---
 *
 *   List<U> mapped = list.ConvertAll(item => …);  // new List<U>, same Count
 *
 * --- List → array ---
 *
 *   T[] copy = list.ToArray();                    // new array, length = Count
 *   list.CopyTo(existing, startIndex);            // into pre-sized array
 *
 * --- array → List ---
 *
 *   var list = new List<T>(array);                // copies all elements
 *
 * ToArray() and ConvertAll() allocate new storage. CopyTo throws if destination
 * has insufficient space (ArgumentException). After conversion, mutating one side
 * does not affect the other — reference types copy references, not new objects.
 * -------------------------------------------------------------------------
 */
public partial class Program
{
    public static void DemoConversions()
    {
        List<ShipmentItem> shipmentQueue = CreateSampleShipmentQueue();

        List<string> skuLabels = shipmentQueue.ConvertAll(item => item.Sku); // List<T> → List<U>

        List<string> exportLabels = new List<string> { "Lane-1", "Lane-2", "Lane-3" };
        string[] labelSnapshot = exportLabels.ToArray();

        string[] buffer = new string[5];
        exportLabels.CopyTo(buffer, 1); // writes at index 1, leaves buffer[0] default

        string[] incoming = { "X", "Y", "Z" };
        List<string> fromArray = new List<string>(incoming);

        Console.WriteLine();
        Console.WriteLine("--- Section 9: ConvertAll / ToArray / CopyTo ---");
        Console.WriteLine($"ConvertAll SKUs: [{string.Join(", ", skuLabels)}]");
        Console.WriteLine($"ToArray: [{string.Join(", ", labelSnapshot)}]");
        Console.WriteLine($"CopyTo buffer[1..]: [{string.Join(", ", buffer)}]");
        Console.WriteLine($"From array: [{string.Join(", ", fromArray)}]");
    }
}

/*
 * =========================================================================
 * SECTION 10: List<T> VS ARRAY
 * =========================================================================
 *
 *  Feature              | T[] array              | List<T>
 *  ---------------------|------------------------|---------------------------
 *  Size after creation  | Fixed (Length)         | Grows/shrinks (Count)
 *  Add at end           | Manual copy to new[]   | list.Add — built in
 *  Insert middle        | Manual shift           | list.Insert
 *  Memory               | Minimal overhead       | Small object + backing array
 *  Type safety          | Yes (T[])              | Yes (generic)
 *  When to prefer       | Known fixed size,      | Unknown count, frequent
 *                       | performance-critical   | add/remove, APIs returning
 *                       | buffers                | IList<T>
 *
 * Arrays excel when size is fixed (days of week, RGB triple). List<T> excels
 * when the collection changes during the program (user input, file lines).
 *
 * Both implement IList<T> and IEnumerable<T> — but only List<T> has Add/Remove.
 * -------------------------------------------------------------------------
 */
public partial class Program
{
    public static void DemoListVsArray()
    {
        string[] fixedDocks = { "North", "South", "East" };
        List<string> flexibleDocks = new List<string>(fixedDocks); // array → List copy
        flexibleDocks.Add("West");

        Console.WriteLine();
        Console.WriteLine("--- Section 10: List vs array ---");
        Console.WriteLine($"Array Length={fixedDocks.Length} (fixed): {string.Join(", ", fixedDocks)}");
        Console.WriteLine($"List Count={flexibleDocks.Count} (grew): {string.Join(", ", flexibleDocks)}");
    }
}

/*
 * =========================================================================
 * SECTION 11: LinkedList<T> (PREVIEW)
 * =========================================================================
 *
 * LinkedList<T> is a doubly-linked list — each node has Next/Previous pointers.
 * Fast insert/remove at known nodes; no index-based list[i] access.
 *
 *   LinkedList<string> chain = new LinkedList<string>();
 *   chain.AddLast("tail");
 *   chain.AddFirst("head");
 *
 * List<T> uses a contiguous array (better cache locality, indexer O(1)).
 * LinkedList<T> wins when you insert/remove often in the middle AND already
 * hold a LinkedListNode<T> reference.
 *
 * This chapter covers LinkedList<T> at preview depth only (module README).
 * There is no separate LinkedList folder — use List<T> unless node refs matter.
 * -------------------------------------------------------------------------
 */
public partial class Program
{
    public static void DemoLinkedListPreview()
    {
        LinkedList<string> handoffChain = new LinkedList<string>();
        handoffChain.AddLast("Pallet-100");
        handoffChain.AddFirst("Pallet-099");
        handoffChain.AddAfter(handoffChain.First!, "Pallet-099A");

        Console.WriteLine();
        Console.WriteLine("--- Section 11: LinkedList<T> preview ---");
        Console.WriteLine($"Linked handoff: {string.Join(" ↔ ", handoffChain)}");
    }
}

/*
 * =========================================================================
 * SECTION 12: Dictionary<TKey,TValue> (PREVIEW)
 * =========================================================================
 *
 * List<T> is ordered by insertion index — find-by-key requires scanning
 * (IndexOf, Find). Dictionary<TKey,TValue> maps keys to values with fast
 * lookup by key (hash table under the hood).
 *
 *   Dictionary<string, int> stock = new Dictionary<string, int>();
 *   stock["WH-4412"] = 120;
 *   bool found = stock.TryGetValue("WH-4412", out int qty);
 *
 * Use List when order and duplicates matter. Use Dictionary when you need
 * unique keys and O(1) average lookup.
 *
 * COVERED IN DETAIL LATER → 04. Dictionary
 * -------------------------------------------------------------------------
 */
public partial class Program
{
    public static void DemoDictionaryPreview()
    {
        Dictionary<string, int> stockBySku = new Dictionary<string, int>
        {
            ["WH-4412"] = 120,
            ["WH-9901"] = 45
        };

        bool found = stockBySku.TryGetValue("WH-4412", out int onHand);

        Console.WriteLine();
        Console.WriteLine("--- Section 12: Dictionary preview ---");
        Console.WriteLine($"TryGetValue WH-4412: found={found}, qty={onHand}");
        Console.WriteLine("(Full Dictionary API → 04. Dictionary)");
    }

    public static List<ShipmentItem> CreateSampleShipmentQueue()
    {
        return new List<ShipmentItem>
        {
            new ShipmentItem("WH-4412", 36, priority: 3),
            new ShipmentItem("WH-9901", 12, priority: 1),
            new ShipmentItem("WH-2200", 8, priority: 2)
        };
    }

    public static void PrintShipmentItems(List<ShipmentItem> items)
    {
        foreach (ShipmentItem item in items)
        {
            Console.WriteLine($"  {item}");
        }
    }
}

/*
 * =========================================================================
 * QUICK REFERENCE — List<T>
 * =========================================================================
 *
 * --- Create ---
 *
 *   List<T> list = new List<T>();
 *   List<T> list = new List<T> { a, b, c };
 *   List<T> list = new List<T>(existingArray);
 *
 * --- CRUD ---
 *
 *   list.Add(item);              list.AddRange(collection);
 *   list.Insert(i, item);        T x = list[i];           list[i] = value;
 *   list.Remove(item);           list.RemoveAt(i);
 *   list.RemoveAll(predicate);   list.Clear();
 *
 * --- Metadata ---
 *
 *   list.Count;   list.Capacity;   list.Contains(item);   list.IndexOf(item);
 *
 * --- Search / mutate order ---
 *
 *   list.Sort();                 list.Reverse();
 *   list.Find(pred);             list.FindAll(pred);
 *   list.Exists(pred);           list.ForEach(action);
 *
 * --- Sort custom T ---
 *
 *   class T : IComparable<T>     →  list.Sort();
 *   list.Sort(IComparer<T>);     list.Sort((a,b) => …);
 *
 * --- Views / conversions ---
 *
 *   list.AsReadOnly();           list.ConvertAll(mapper);
 *   T[] a = list.ToArray();      list.CopyTo(a, startIndex);
 *   new List<T>(array);
 *
 * --- vs array ---
 *
 *   array: fixed Length, minimal overhead, best for known-size buffers
 *   List<T>: dynamic Count, Add/Insert/Remove, preferred for growing collections
 *
 * --- Related chapters ---
 *
 *   Generics / type safety           → 01. Generics
 *   Legacy ArrayList (non-generic)   → 02. ArrayList
 *   Key/value lookup                 → 04. Dictionary
 *   LinkedList<T> (preview)          → Section 11 above (no dedicated chapter)
 *   IEnumerable / foreach            → 08. IEnumerable & IEnumerator
 *
 * --- Common mistakes ---
 *
 *  Mistake                           | Result
 *  ----------------------------------|----------------------------------
 *  list[i] when i >= Count           | ArgumentOutOfRangeException
 *  Sort() on T without IComparable   | InvalidOperationException
 *  Assuming Remove removes all       | Only first match — use RemoveAll
 *  CopyTo buffer too small           | ArgumentException
 *  Confusing Capacity with Count     | Capacity ≥ Count; Clear keeps Capacity
 *  Mutating via AsReadOnly() view    | NotSupportedException
 *
 * =========================================================================
 */
