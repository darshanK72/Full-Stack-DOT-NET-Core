/*
 * =============================================================================
 * 02. ARRAYLIST & LEGACY NON-GENERIC COLLECTIONS
 * =============================================================================
 *
 * TOPIC: System.Collections.ArrayList and related pre-generics types (Hashtable,
 *        Stack, Queue, SortedList) — dynamic storage as object, boxing cost,
 *        ICollection/IList interfaces, and why List<T> replaced them.
 *
 * WHY IT MATTERS:
 *   ArrayList and Hashtable still appear in older .NET Framework code, migration
 *   projects, and interviews. Non-generic collections explain boxing overhead,
 *   runtime InvalidCastException, and the motivation for generics (01. Generics).
 *
 * WHAT YOU WILL LEARN:
 *   1.  What ArrayList is and where it lives (System.Collections)
 *   2.  Count, Capacity, TrimToSize, and backing-array growth
 *   3.  Add, Insert, Remove, RemoveAt, RemoveRange, Clear, Contains, IndexOf
 *   4.  Indexer read/write — returns object, requires casts
 *   5.  foreach on non-generic collections (IEnumerable / IEnumerator)
 *   6.  ICollection and IList non-generic interfaces
 *   7.  Boxing and unboxing pitfalls with value types
 *   8.  Product catalog scenario — typed objects with manual casts
 *   9.  Hashtable key/value basics
 *   10. Non-generic Stack, Queue, SortedList (brief intro)
 *   11. When to avoid ArrayList — preview of List<T>
 *
 * Scenario: warehouse inventory — mixed lines, SKU lookup, pick stack, ship queue.
 *
 * =============================================================================
 */

using System;
using System.Collections;
using System.Collections.Generic;

namespace ArrayList;

/*
 * SECTION 2: PRODUCT — REFERENCE OBJECTS IN A NON-GENERIC LIST
 *
 * ArrayList stores object references. Reference types (classes like Product)
 * are added without boxing — the list holds a pointer to the heap object.
 *
 * Reading back still returns object, so every access needs an explicit cast:
 *   Product p = (Product)list[i];
 *
 * Wrong cast → InvalidCastException at runtime (not a compile error).
 *
 * List<Product> (03. List) removes those casts — preview in Section 11.
 */
public class Product
{
    public int ProductNo { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal ProductPrice { get; set; }

    public override string ToString() =>
        $"#{ProductNo} {ProductName} — {ProductPrice:C}";
}

/*
 * SECTION 3: LEGACY OBJECT SLOTS — RUNTIME TYPE FRAGILITY
 *
 * Pre-generics data bags often used object? for every field. Nothing stops you
 * from assigning incompatible types at compile time — errors surface only when
 * you cast or call a member on the wrong runtime type.
 *
 * This pattern mirrors Hashtable values and ArrayList elements in legacy code.
 */
file class LegacyLineItem
{
    public object? LineId { get; set; }
    public object? Description { get; set; }
    public object? Quantity { get; set; }
    public object? UnitPrice { get; set; }

    public override string ToString() =>
        $"Line {LineId}: {Description} x{Quantity} @ {UnitPrice}";
}

/*
 * SECTION 4: COUNT, CAPACITY, AND BACKING-ARRAY GROWTH
 *
 * ArrayList wraps a resizable object[] internally (same idea as List<T>).
 *
 *  Member / property | Meaning
 *  ------------------|--------------------------------------------------------
 *  Count             | Elements currently stored (logical size)
 *  Capacity          | Length of the internal backing array (≥ Count)
 *  Capacity = n      | Pre-allocate space before many Add calls
 *  TrimToSize()      | Shrink Capacity down to Count (release unused slots)
 *
 * When Count exceeds Capacity, ArrayList allocates a larger array and copies
 * elements — amortized O(1) for Add at the end, like List<T>.
 */
static class CapacityDemo
{
    public static void Run()
    {
        System.Collections.ArrayList lines = new System.Collections.ArrayList(4); // initial capacity hint
        Console.WriteLine($"Empty — Count: {lines.Count}, Capacity: {lines.Capacity}");

        lines.Add("SKU-A");
        lines.Add("SKU-B");
        lines.Add("SKU-C");
        Console.WriteLine($"After 3 Add — Count: {lines.Count}, Capacity: {lines.Capacity}");

        lines.Capacity = 20; // grow backing array without changing Count
        Console.WriteLine($"After Capacity = 20 — Count: {lines.Count}, Capacity: {lines.Capacity}");

        lines.TrimToSize(); // shrink Capacity to match Count
        Console.WriteLine($"After TrimToSize — Count: {lines.Count}, Capacity: {lines.Capacity}");
    }
}

/*
 * SECTION 5: ADD, INSERT, REMOVE, CLEAR — CORE CRUD API
 *
 *  Method / member      | Purpose
 *  ---------------------|---------------------------------------------------
 *  Add(object)          | Append at end; returns index of new item
 *  Insert(index, obj)   | Shift elements right and place at index
 *  Remove(object)       | Remove first match (uses Equals)
 *  RemoveAt(index)      | Remove by zero-based index
 *  RemoveRange(i, n)    | Remove n items starting at index i
 *  Clear()              | Remove all elements; Count → 0
 *  Contains(object)     | true if an equal item exists
 *  IndexOf(object)      | First index, or -1 if not found
 *
 * Bad index on Insert/RemoveAt → ArgumentOutOfRangeException.
 */
static class CrudDemo
{
    public static void Run()
    {
        System.Collections.ArrayList warehouseLines = new System.Collections.ArrayList();

        warehouseLines.Add("SKU-WH-100");
        warehouseLines.Add(250);
        warehouseLines.Add(new Product { ProductNo = 100, ProductName = "Cable Kit", ProductPrice = 19.99m });
        Console.WriteLine("After Add — Count: " + warehouseLines.Count);

        warehouseLines.Insert(1, "RUSH-PICK");
        Console.WriteLine("After Insert at 1:");
        ArrayListPrinter.Print(warehouseLines);

        warehouseLines.Remove("RUSH-PICK");
        warehouseLines.RemoveAt(0);
        Console.WriteLine("After Remove / RemoveAt:");
        ArrayListPrinter.Print(warehouseLines);

        warehouseLines.Add("TEMP-1");
        warehouseLines.Add("TEMP-2");
        warehouseLines.RemoveRange(0, 2);
        Console.WriteLine("After RemoveRange(0,2) — Count: " + warehouseLines.Count);

        bool hasTemp = warehouseLines.Contains("TEMP-1");
        Console.WriteLine("Contains 'TEMP-1': " + hasTemp);

        warehouseLines.Clear();
        Console.WriteLine("After Clear — Count: " + warehouseLines.Count);
    }
}

/*
 * SECTION 6: INDEXER — GET AND SET BY POSITION
 *
 * ArrayList implements IList.this[int index]:
 *   object value = list[i];   // get — returns object
 *   list[i] = newValue;       // set — replaces slot; does not grow Count
 *
 * Assigning without cast fails at compile time — CS0266 cannot implicitly
 * convert type 'object' to 'int'. You must write (int)list[0].
 *
 * Setting list[Count] or list[-1] → ArgumentOutOfRangeException.
 */
static class IndexingDemo
{
    public static void Run()
    {
        System.Collections.ArrayList slots = new System.Collections.ArrayList { "Dock-A", "Dock-B", "Dock-C" };

        string dockA = (string)slots[0]!;           // get returns object — cast to string
        slots[1] = "Dock-B (expanded)";             // set replaces element at index 1

        Console.WriteLine($"Indexer get [0]: {dockA}");
        Console.WriteLine($"Indexer set [1]: {(string)slots[1]!}");
        Console.WriteLine($"IndexOf 'Dock-C': {slots.IndexOf("Dock-C")}");
    }
}

/*
 * SECTION 7: FOREACH ON NON-GENERIC COLLECTIONS
 *
 * ArrayList implements IEnumerable (non-generic). foreach compiles to:
 *
 *   IEnumerator enumerator = list.GetEnumerator();
 *   while (enumerator.MoveNext()) { object item = enumerator.Current; … }
 *
 * Current returns object — cast each element to the type you expect.
 * Mixing types in one list means every iteration is a runtime guess.
 *
 * Prefer typed iteration with List<T> (03. List) when all elements share one type.
 */
static class ForeachDemo
{
    public static void Run()
    {
        System.Collections.ArrayList mixed = new System.Collections.ArrayList();
        mixed.Add("WH-100");
        mixed.Add(42);
        mixed.Add(new Product { ProductNo = 5, ProductName = "Tape", ProductPrice = 3.99m });

        Console.WriteLine("foreach (object item in mixed):");
        foreach (object item in mixed)
        {
            Console.WriteLine("  " + item);
        }

        Console.WriteLine("Manual IEnumerator walk:");
        IEnumerator walker = mixed.GetEnumerator();
        while (walker.MoveNext())
        {
            object current = walker.Current ?? "(null)";
            Console.WriteLine("  Current: " + current);
        }
    }
}

/*
 * SECTION 8: ICollection AND IList (NON-GENERIC INTERFACES)
 *
 * ArrayList implements several legacy interfaces:
 *
 *  Interface    | Key members used in practice
 *  -------------|-------------------------------------------------------------
 *  IList        | Add, Insert, Remove, RemoveAt, Contains, IndexOf, Item[i]
 *  ICollection  | Count, CopyTo(array, index), IsSynchronized, SyncRoot
 *  IEnumerable  | GetEnumerator() — powers foreach
 *
 * Programming against IList or ICollection accepts ArrayList or any compatible
 * collection — useful when reading APIs that predate generics.
 *
 * CopyTo copies elements into a object[] starting at a given index.
 */
static class InterfaceDemo
{
    public static void Run()
    {
        System.Collections.ArrayList internalList = new System.Collections.ArrayList();
        internalList.Add("Alpha");
        internalList.Add("Beta");
        internalList.Add("Gamma");

        IList listAsIList = internalList;           // ArrayList → IList reference
        listAsIList.Add("Delta");                   // polymorphic Add through interface
        Console.WriteLine("IList.Add — Count: " + listAsIList.Count);

        ICollection collectionView = internalList;  // ArrayList → ICollection reference
        object[] buffer = new object[collectionView.Count];
        collectionView.CopyTo(buffer, 0);           // copy all elements into object[]
        Console.WriteLine("ICollection.CopyTo → [" + string.Join(", ", buffer) + "]");
        Console.WriteLine("IsSynchronized: " + collectionView.IsSynchronized);
    }
}

/*
 * SECTION 9: BOXING AND UNBOXING
 *
 * Value types (int, double, decimal, bool, struct, …) do not inherit object,
 * but every value can be converted to object — that BOXES the value on the heap.
 *
 * Reference types (string, class instances) are stored as references — no extra
 * boxing wrapper for the reference itself.
 *
 * Reading a boxed value requires an explicit UNBOX cast to the original type.
 * Wrong type → InvalidCastException (e.g. (long)boxedInt fails).
 *
 * Compile error: int n = list[0];  // CS0266 — must cast: (int)list[0]
 */
static class BoxingDemo
{
    public static void Run()
    {
        System.Collections.ArrayList readings = new System.Collections.ArrayList();
        readings.Add(42);           // int boxed into object
        readings.Add("sensor-A");   // string reference — no boxing
        readings.Add(98.6);         // double boxed
        readings.Add(true);         // bool boxed

        int readingCount = (int)readings[0]!;
        string sensorId = (string)readings[1]!;
        double temperature = (double)readings[2]!;
        bool sensorOk = (bool)readings[3]!;

        Console.WriteLine($"Unboxed — count={readingCount}, id={sensorId}, temp={temperature}, ok={sensorOk}");

        LegacyLineItem looseItem = new LegacyLineItem
        {
            LineId = 1,
            Description = "Loose pack tape",
            Quantity = 24,
            UnitPrice = 4.50
        };
        readings.Add(looseItem);

        LegacyLineItem retrieved = (LegacyLineItem)readings[4]!;
        Console.WriteLine("Legacy object slot: " + retrieved);
    }
}

/*
 * SECTION 10: PRODUCT CATALOG — SORTED INSERT AND TYPED FOREACH
 *
 * Common legacy pattern: store homogeneous Product instances in an ArrayList,
 * cast each element inside foreach, and maintain order with manual Insert logic.
 *
 * Insert-in-order by ProductNo: walk the list; Insert when the new id is smaller;
 * otherwise Add at end. Every loop still needs (Product) — List<Product> removes that.
 */
static class CatalogDemo
{
    public static void Run()
    {
        System.Collections.ArrayList catalog = new System.Collections.ArrayList();
        InsertProductSorted(catalog, new Product { ProductNo = 30, ProductName = "Label Printer", ProductPrice = 129.00m });
        InsertProductSorted(catalog, new Product { ProductNo = 10, ProductName = "Barcode Scanner", ProductPrice = 89.50m });
        InsertProductSorted(catalog, new Product { ProductNo = 20, ProductName = "Docking Station", ProductPrice = 45.00m });

        Console.WriteLine("\nProduct catalog (sorted by ProductNo):");
        foreach (object entry in catalog)
        {
            Product product = (Product)entry;
            Console.WriteLine("  " + product);
        }

        Product? target = FindProductByNo(catalog, 20);
        int foundIndex = target is not null ? catalog.IndexOf(target) : -1;

        if (foundIndex >= 0)
        {
            Product removed = (Product)catalog[foundIndex]!;
            catalog.RemoveAt(foundIndex);
            Console.WriteLine("Removed: " + removed);
        }

        Console.WriteLine("Catalog after delete #20 — Count: " + catalog.Count);
    }

    static void InsertProductSorted(System.Collections.ArrayList catalog, Product product)
    {
        for (int i = 0; i < catalog.Count; i++)
        {
            Product existing = (Product)catalog[i]!;
            if (product.ProductNo < existing.ProductNo)
            {
                catalog.Insert(i, product);
                return;
            }
        }

        catalog.Add(product);
    }

    static Product? FindProductByNo(System.Collections.ArrayList catalog, int productNo)
    {
        foreach (object entry in catalog)
        {
            Product product = (Product)entry;
            if (product.ProductNo == productNo)
            {
                return product;
            }
        }

        return null;
    }
}

/*
 * SECTION 11: WHEN TO AVOID ARRAYLIST — PREVIEW List<T>
 *
 *  ArrayList                         | List<T>
 *  ----------------------------------|------------------------------------
 *  Stores object                     | Stores T — compile-time type check
 *  Value types boxed                 | No boxing for unconstrained T
 *  Cast on every read                | Direct typed access
 *  No generic LINQ without Cast<T>() | Full LINQ on T
 *
 * COVERED IN DETAIL LATER → 03. List
 *   (List<T> API, capacity, sorting, searching, List vs Array)
 */
static class ListPreviewDemo
{
    public static void Run()
    {
        List<Product> typedCatalog = new List<Product>
        {
            new Product { ProductNo = 10, ProductName = "Barcode Scanner", ProductPrice = 89.50m },
            new Product { ProductNo = 30, ProductName = "Label Printer", ProductPrice = 129.00m }
        };

        decimal catalogTotal = 0m;
        foreach (Product product in typedCatalog)
        {
            catalogTotal += product.ProductPrice;
        }

        Console.WriteLine("\nList<Product> catalog total: " + catalogTotal.ToString("C"));
    }
}

/*
 * SECTION 12: HASHTABLE — KEY/VALUE LOOKUP (LEGACY)
 *
 * Hashtable maps object keys to object values. Fast average O(1) lookup by key;
 * keys need consistent GetHashCode and Equals.
 *
 *  Member           | Role
 *  -----------------|-------------------------------------------------------
 *  Add(key, value)  | Insert pair (throws if key already exists)
 *  this[key]        | Get or set value
 *  ContainsKey(key) | Key exists?
 *  Remove(key)      | Drop entry
 *  Count            | Number of pairs
 *
 * Modern replacement: Dictionary<TKey,TValue> (04. Dictionary).
 */
static class HashtableDemo
{
    public static void Run()
    {
        Hashtable skuToBin = new Hashtable();
        skuToBin.Add("WH-100", "Aisle 3-B");
        skuToBin.Add("WH-200", "Aisle 1-A");
        skuToBin["WH-300"] = "Aisle 5-C";

        string? bin = skuToBin["WH-200"] as string;
        Console.WriteLine("\nHashtable lookup WH-200 → " + bin);
        Console.WriteLine("ContainsKey WH-999: " + skuToBin.ContainsKey("WH-999"));
    }
}

/*
 * SECTION 13: STACK, QUEUE, SORTEDLIST — BRIEF INTRO (LEGACY)
 *
 * Other System.Collections types in older code. Each has a generic counterpart.
 *
 * COVERED IN DETAIL LATER → 06. Queue and Stack
 *   (Stack<T>, Queue<T>, LIFO vs FIFO)
 *
 * COVERED IN DETAIL LATER → 07. SortedList and SortedDictionary
 *   (SortedList<TKey,TValue>, sorted keys)
 *
 * --- 13a. Stack (LIFO — last in, first out) ---
 * --- 13b. Queue (FIFO — first in, first out) ---
 * --- 13c. SortedList (sorted keys, key/value pairs) ---
 */
static class LegacyCollectionsPreview
{
    public static void Run()
    {
        Stack pickStack = new Stack();
        pickStack.Push("Order-101");
        pickStack.Push("Order-102");
        pickStack.Push("Order-103");

        string nextPick = (string)pickStack.Pop()!;
        Console.WriteLine("\nStack Pop (most recent pick): " + nextPick);
        Console.WriteLine("Stack Peek: " + (string)pickStack.Peek()!);

        Queue shipQueue = new Queue();
        shipQueue.Enqueue("Box-A");
        shipQueue.Enqueue("Box-B");

        string nextShip = (string)shipQueue.Dequeue()!;
        Console.WriteLine("Queue Dequeue (oldest): " + nextShip);

        SortedList zoneCapacity = new SortedList();
        zoneCapacity.Add("Zone-A", 500);
        zoneCapacity.Add("Zone-C", 1200);
        zoneCapacity.Add("Zone-B", 800);

        Console.WriteLine("\nSortedList keys (automatic order):");
        foreach (object key in zoneCapacity.Keys)
        {
            Console.WriteLine("  " + key + " → " + zoneCapacity[key!] + " units");
        }
    }
}

/*
 * Helper — print ArrayList contents by index (uses Count and indexer).
 */
static class ArrayListPrinter
{
    public static void Print(System.Collections.ArrayList list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Console.WriteLine("  [" + i + "] " + list[i]);
        }
    }
}

public class Program
{
    /*
     * SECTION 1: WHAT IS AN ARRAYLIST?
     *
     * ArrayList lives in System.Collections — the namespace used before C# 2
     * generics. It is a resizable list that stores every element as object.
     *
     *   System.Collections.ArrayList list = new System.Collections.ArrayList();
     *   (Fully qualified here — chapter namespace is also ArrayList.)
     *
     *  Feature              | Detail
     *  ---------------------|-------------------------------------------------
     *  Underlying storage   | object[] that grows automatically
     *  Element type         | object — anything can be added
     *  Type safety          | None at compile time; casts required on read
     *  Modern replacement   | List<T> (see 03. List)
     *
     * Main below runs each section in reading order — warehouse inventory scenario.
     */
    public static void Main(string[] args)
    {
        Console.WriteLine("=== 02. ArrayList & Legacy Collections ===\n");

        Console.WriteLine("--- Section 4: Count & Capacity ---");
        CapacityDemo.Run();

        Console.WriteLine("\n--- Section 5: CRUD ---");
        CrudDemo.Run();

        Console.WriteLine("\n--- Section 6: Indexer ---");
        IndexingDemo.Run();

        Console.WriteLine("\n--- Section 7: foreach ---");
        ForeachDemo.Run();

        Console.WriteLine("\n--- Section 8: ICollection / IList ---");
        InterfaceDemo.Run();

        Console.WriteLine("\n--- Section 9: Boxing / Unboxing ---");
        BoxingDemo.Run();

        Console.WriteLine("\n--- Section 10: Product catalog ---");
        CatalogDemo.Run();

        Console.WriteLine("\n--- Section 11: List<T> preview ---");
        ListPreviewDemo.Run();

        Console.WriteLine("\n--- Section 12: Hashtable ---");
        HashtableDemo.Run();

        Console.WriteLine("\n--- Section 13: Stack / Queue / SortedList ---");
        LegacyCollectionsPreview.Run();
    }
}

/*
 * QUICK REFERENCE — ARRAYLIST & LEGACY COLLECTIONS
 *
 *  Type          | Namespace            | Modern generic replacement
 *  --------------|----------------------|-----------------------------
 *  ArrayList     | System.Collections   | List<T>
 *  Hashtable     | System.Collections   | Dictionary<TKey,TValue>
 *  Stack         | System.Collections   | Stack<T>
 *  Queue         | System.Collections   | Queue<T>
 *  SortedList    | System.Collections   | SortedList<TKey,TValue>
 *
 *  ArrayList essentials:
 *    Add, Insert, Remove, RemoveAt, RemoveRange, Clear
 *    Count, Capacity, TrimToSize
 *    Contains, IndexOf, [index] get/set
 *    Implements IList, ICollection, IEnumerable (non-generic)
 *
 *  foreach: IEnumerable.GetEnumerator() → Current returns object
 *
 *  Pitfalls:
 *    • Value types boxed → heap allocation + cast on read
 *    • InvalidCastException on wrong unbox type
 *    • No compile-time type check on Add
 *    • CS0266 if you assign object to a value type without cast
 */
