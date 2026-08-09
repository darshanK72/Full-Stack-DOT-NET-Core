/*
 * =============================================================================
 * 10. CONVERSION OPERATIONS — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: LINQ conversion operators — materialize a deferred IEnumerable<T>
 *        pipeline into a concrete collection (ToList, ToArray, ToHashSet,
 *        ToDictionary, ToLookup), or reshape typing / query-provider binding
 *        (Cast, OfType preview, AsEnumerable, AsQueryable).
 *
 * WHY IT MATTERS:
 *   Most LINQ operators return a recipe, not data. Conversion operators are
 *   how you force execution, freeze a snapshot, build keyed indexes, pass
 *   arrays to legacy APIs, or extract typed rows from mixed/untyped feeds.
 *   Choosing the wrong conversion (ToDictionary vs ToLookup, Cast vs OfType)
 *   is a common source of ArgumentException and InvalidCastException bugs.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Deferred vs materialization — when conversion forces execution
 *   2.  ToList / ToArray — List vs fixed array snapshots
 *   3.  ToHashSet — unique set materialization (optional comparer)
 *   4.  ToDictionary — unique-key map; duplicate-key failure
 *   5.  ToLookup — multi-value keyed index vs GroupBy / ToDictionary
 *   6.  Cast — strict cast on every element (InvalidCastException)
 *   7.  OfType — brief preview (FULL in 02. Filtering & Aggregation)
 *   8.  AsEnumerable — force Enumerable extension-method binding
 *   9.  AsQueryable — preview toward IQueryable providers
 *
 * =============================================================================
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ConversionOperations;

/*
 * =========================================================================
 * SECTION A: DOMAIN TYPES — InventoryItem / ItemCategory
 * =========================================================================
 *
 * Warehouse catalog row used across materialization and typing demos.
 *
 *  Field / type     | Role in this chapter
 *  -----------------|---------------------------
 *  Sku              | Natural unique key for ToDictionary
 *  Category         | Natural multi-value key for ToLookup
 *  UnitPrice        | Projection target for element selectors
 *  StockQty         | Filter predicate for deferred pipelines
 *
 * UnitPrice and StockQty are settable so snapshot demos can show that
 * ToList freezes membership/count, not deep copies of reference objects.
 * -------------------------------------------------------------------------
 */
public enum ItemCategory
{
    Hardware,
    Equipment,
    Apparel,
}

public sealed class InventoryItem
{
    public InventoryItem(string sku, string name, decimal unitPrice, int stockQty, ItemCategory category)
    {
        Sku = sku;
        Name = name;
        UnitPrice = unitPrice;
        StockQty = stockQty;
        Category = category;
    }

    public string Sku { get; }                          // business key — ToDictionary
    public string Name { get; }
    public decimal UnitPrice { get; set; }              // mutable — shared instance after ToList
    public int StockQty { get; set; }                   // mutable stock level
    public ItemCategory Category { get; }               // multi-map key — ToLookup
}

/*
 * =========================================================================
 * SECTION B: CUSTOM COLLECTION — WHY AsEnumerable EXISTS
 * =========================================================================
 *
 * Implements IEnumerable<InventoryItem> and ALSO exposes an instance Where
 * that runs eagerly. Without AsEnumerable(), concreteCatalog.Where(...) binds
 * to this method instead of Enumerable.Where (deferred LINQ).
 * -------------------------------------------------------------------------
 */
public sealed class InventoryCatalogCollection : IEnumerable<InventoryItem>
{
    private readonly List<InventoryItem> _items;

    public InventoryCatalogCollection(IEnumerable<InventoryItem> seed)
    {
        _items = seed.ToList(); // copy seed into inner storage
    }

    public List<InventoryItem> Where(Func<InventoryItem, bool> predicate)
    {
        Console.WriteLine("  [InventoryCatalogCollection.Where — eager custom implementation]");
        List<InventoryItem> matches = new List<InventoryItem>();
        foreach (InventoryItem item in _items)
        {
            if (predicate(item))
            {
                matches.Add(item);
            }
        }

        return matches; // already materialized — not a deferred LINQ query
    }

    public IEnumerator<InventoryItem> GetEnumerator() => _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class Program
{
    /*
     * =========================================================================
     * SECTION 1–10: DEMONSTRATION — Main orchestrates conversion demos
     * =========================================================================
     *
     * Scenario: a warehouse catalog tracks sellable inventory. A legacy import
     * feed arrives as untyped objects. We filter, snapshot, build keyed
     * indexes, and extract typed rows from the mixed feed.
     * -------------------------------------------------------------------------
     */
    public static void Main(string[] args)
    {
        /*
         * =========================================================================
         * SECTION 1: DEFERRED vs MATERIALIZATION
         * =========================================================================
         *
         * ch.01 Introduction to LINQ established two execution modes:
         *
         *  Mode        | When it runs                    | Typical return
         *  ------------|---------------------------------|---------------------------
         *  Deferred    | On foreach / re-foreach / Count | IEnumerable<T> (Where…)
         *  Immediate   | When the operator is called     | List, array, Dictionary…
         *
         * Conversion operators fall into two buckets:
         *
         *  Family              | Operators                         | Execution
         *  --------------------|-----------------------------------|-----------
         *  Materializing       | ToList, ToArray, ToHashSet,       | IMMEDIATE
         *                      | ToDictionary, ToLookup            | (terminal)
         *  Typing / provider   | Cast, OfType, AsEnumerable,       | DEFERRED
         *                      | AsQueryable                       | (no copy)
         *
         * Calling a materializing operator walks the entire upstream pipeline
         * NOW, allocates a new structure, and returns it. Later changes to the
         * source collection do not change that structure's membership (though
         * shared reference-type instances can still mutate).
         * -------------------------------------------------------------------------
         */

        InventoryItem[] catalog =
        [
            new InventoryItem("WH-4412", "Industrial Shelving Unit", 49.99m, 120, ItemCategory.Hardware),
            new InventoryItem("WH-8890", "Heavy-Duty Pallet Jack", 899.00m, 8, ItemCategory.Equipment),
            new InventoryItem("WH-2201", "Safety Vest (Bulk)", 12.50m, 0, ItemCategory.Apparel),
            new InventoryItem("WH-3305", "Barcode Scanner Kit", 245.00m, 34, ItemCategory.Equipment),
            new InventoryItem("WH-1100", "Forklift Battery Charger", 1250.00m, 3, ItemCategory.Equipment),
            new InventoryItem("WH-4412", "Industrial Shelving Unit", 49.99m, 120, ItemCategory.Hardware), // dup SKU row
        ];

        Console.WriteLine("=== Warehouse catalog (source) ===");
        PrintCatalog(catalog);

        /*
         * --- 1a. Deferred pipeline — no materialization yet ---
         *
         * Building this query does NOT scan catalog until you consume it.
         * -------------------------------------------------------------------------
         */

        int filterRuns = 0;

        IEnumerable<InventoryItem> lowStockDeferred =
            catalog
                .Where(item =>
                {
                    filterRuns++; // counts how often the predicate actually runs
                    return item.StockQty > 0 && item.StockQty <= 10;
                })
                .OrderBy(item => item.StockQty);

        Console.WriteLine();
        Console.WriteLine("--- Deferred vs immediate ---");
        Console.WriteLine($"Pipeline built; filterRuns = {filterRuns} (still 0 — not executed)");

        foreach (InventoryItem item in lowStockDeferred)
        {
            Console.WriteLine($"  LOW  {item.Sku}  qty={item.StockQty}");
        }

        Console.WriteLine($"After foreach, filterRuns = {filterRuns}");


        /*
         * =========================================================================
         * SECTION 2: ToList() — MATERIALIZE INTO List<T>
         * =========================================================================
         *
         * ToList() executes the upstream pipeline immediately and copies matching
         * elements into a new List<T>.
         *
         *   List<TSource> ToList<TSource>(this IEnumerable<TSource> source)
         *
         * Why materialize?
         *   • Structural snapshot — count/membership frozen at ToList() time
         *   • Multiple passes — iterate without re-running Where/Select/OrderBy
         *   • APIs expecting IList<T> or List<T> (Add, indexing, Count property)
         *
         * Snapshot semantics for reference types:
         *   • Adding/removing source items later does NOT change the list's count
         *   • Both lists still point at the SAME object instances — mutating a
         *     property is visible through both. Deep clone is a separate concern.
         *
         * --- 2a. Structural snapshot ---
         * -------------------------------------------------------------------------
         */

        List<InventoryItem> liveCatalog = catalog.DistinctBy(item => item.Sku).ToList();

        List<InventoryItem> equipmentSnapshot =
            liveCatalog
                .Where(item => item.Category == ItemCategory.Equipment)
                .OrderBy(item => item.UnitPrice)
                .ToList(); // forces Where + OrderBy to run now

        Console.WriteLine();
        Console.WriteLine("--- ToList() snapshot ---");
        Console.WriteLine($"Equipment snapshot count before source add: {equipmentSnapshot.Count}");

        liveCatalog.Add(new InventoryItem("WH-NEW1", "Temporary Demo SKU", 1.00m, 1, ItemCategory.Equipment));

        Console.WriteLine($"Live catalog count after add: {liveCatalog.Count}");
        Console.WriteLine($"Equipment snapshot count unchanged: {equipmentSnapshot.Count}");

        /*
         * --- 2b. Re-running deferred vs cached list ---
         *
         * Each foreach over a deferred query re-executes filters. A ToList cache
         * stores results once; later iterations are cheap.
         * -------------------------------------------------------------------------
         */

        filterRuns = 0;

        IEnumerable<InventoryItem> deferredLowStock = liveCatalog.Where(item =>
        {
            filterRuns++;
            return item.StockQty <= 10;
        });

        List<InventoryItem> cachedLowStock = deferredLowStock.ToList(); // first (and only) filter pass

        Console.WriteLine();
        Console.WriteLine("--- Deferred re-run vs ToList cache ---");
        Console.WriteLine($"After ToList, filterRuns = {filterRuns}");

        foreach (InventoryItem _ in deferredLowStock)
        {
            // second enumeration of deferred query — Where runs again
        }

        Console.WriteLine($"Second foreach deferred: filterRuns = {filterRuns} (pipeline re-run)");

        foreach (InventoryItem _ in cachedLowStock)
        {
            // cached list — no Where re-execution
        }

        Console.WriteLine($"After iterating cached list, filterRuns still = {filterRuns}");


        /*
         * =========================================================================
         * SECTION 3: ToArray() — MATERIALIZE INTO T[]
         * =========================================================================
         *
         * ToArray() also executes immediately and copies elements into a new array.
         *
         *  List<T> vs T[]:
         *    List<T>  — resizable, IList<T>, common for business collections
         *    T[]      — fixed length, required by many legacy APIs (params T[], interop)
         *
         * Choose ToArray when the consumer needs an array; otherwise ToList is often
         * more convenient. Both are immediate (terminal) operators.
         * -------------------------------------------------------------------------
         */

        decimal priceFloor = 100.00m;
        decimal priceCeiling = 1000.00m;

        InventoryItem[] midTierSkus =
            liveCatalog
                .Where(item => item.UnitPrice >= priceFloor && item.UnitPrice <= priceCeiling)
                .OrderBy(item => item.UnitPrice)
                .ToArray(); // forces pipeline; result length is fixed

        Console.WriteLine();
        Console.WriteLine($"--- ToArray() mid-tier SKUs ({priceFloor:C}–{priceCeiling:C}) ---");
        Console.WriteLine($"Array.Length = {midTierSkus.Length} (fixed after materialization)");
        foreach (InventoryItem item in midTierSkus)
        {
            Console.WriteLine($"  {item.Sku}  {item.UnitPrice:C}");
        }

        ProcessSkuBatch(midTierSkus); // API that requires T[]


        /*
         * =========================================================================
         * SECTION 4: ToHashSet() — MATERIALIZE INTO HashSet<T>
         * =========================================================================
         *
         * ToHashSet() executes immediately and builds a HashSet<T> of unique
         * elements (default equality, or an optional IEqualityComparer<T>).
         *
         *   HashSet<TSource> ToHashSet()
         *   HashSet<TSource> ToHashSet(IEqualityComparer<TSource>? comparer)
         *
         * Use when you need:
         *   • O(1) membership tests (Contains) after materialization
         *   • A set of unique keys/values without Dictionary overhead
         *   • Deduplicated results you will mutate as a set (UnionWith, etc.)
         *
         * Distinct() is deferred and yields uniqueness on enumeration.
         * ToHashSet() is immediate and returns a mutable HashSet<T>.
         *
         * COVERED IN DETAIL LATER → 07. Set Operations (Distinct / Union / …)
         *
         * --- 4a. Unique SKUs as a HashSet ---
         * -------------------------------------------------------------------------
         */

        HashSet<string> skuSet = catalog.Select(item => item.Sku).ToHashSet(); // immediate + unique

        Console.WriteLine();
        Console.WriteLine("--- ToHashSet() unique SKUs ---");
        Console.WriteLine($"Source rows: {catalog.Length}; unique SKUs: {skuSet.Count}");
        Console.WriteLine($"Contains WH-3305: {skuSet.Contains("WH-3305")}");
        Console.WriteLine($"Contains WH-9999: {skuSet.Contains("WH-9999")}");

        /*
         * --- 4b. Custom comparer — case-insensitive SKU set ---
         * -------------------------------------------------------------------------
         */

        string[] noisySkus = ["wh-3305", "WH-3305", "Wh-4412", "WH-4412", "WH-NEW1"];
        HashSet<string> caseInsensitiveSkus =
            noisySkus.ToHashSet(StringComparer.OrdinalIgnoreCase);

        Console.WriteLine();
        Console.WriteLine("--- ToHashSet with StringComparer.OrdinalIgnoreCase ---");
        Console.WriteLine($"Noisy input count {noisySkus.Length} → set count {caseInsensitiveSkus.Count}");
        Console.WriteLine($"Contains 'WH-3305': {caseInsensitiveSkus.Contains("WH-3305")}");


        /*
         * =========================================================================
         * SECTION 5: ToDictionary() — UNIQUE-KEY LOOKUP
         * =========================================================================
         *
         * ToDictionary builds Dictionary<TKey, TElement> in one pass (immediate).
         *
         *  Overloads (common):
         *    ToDictionary(keySelector)
         *    ToDictionary(keySelector, elementSelector)
         *    ToDictionary(keySelector, elementSelector, comparer)
         *
         * Each key must be UNIQUE. Duplicate keys throw ArgumentException —
         * unlike ToLookup / GroupBy, ToDictionary does not merge duplicates.
         *
         * Typical pattern: SKU → item for O(1) lookup during order validation.
         *
         * --- 5a. SKU lookup dictionary ---
         * -------------------------------------------------------------------------
         */

        Dictionary<string, InventoryItem> skuLookup =
            liveCatalog.ToDictionary(item => item.Sku); // key = Sku; value = full item

        Console.WriteLine();
        Console.WriteLine("--- ToDictionary() SKU lookup ---");
        Console.WriteLine($"Lookup WH-3305: {skuLookup["WH-3305"].Name}");
        Console.WriteLine($"Contains WH-9999: {skuLookup.ContainsKey("WH-9999")}");

        /*
         * --- 5b. Key + element selector — lightweight values ---
         * -------------------------------------------------------------------------
         */

        Dictionary<string, decimal> skuPriceMap =
            liveCatalog.ToDictionary(
                item => item.Sku,
                item => item.UnitPrice); // store price only, not full entity

        Console.WriteLine();
        Console.WriteLine("--- ToDictionary with element selector ---");
        Console.WriteLine($"WH-4412 unit price from map: {skuPriceMap["WH-4412"]:C}");

        /*
         * --- 5c. Duplicate keys fail fast ---
         *
         * If two elements produce the same key, ToDictionary throws:
         *   ArgumentException: An item with the same key has already been added.
         *
         * When duplicates are expected, use GroupBy / ToLookup, or DistinctBy
         * upstream — do not call ToDictionary on raw duplicates.
         * -------------------------------------------------------------------------
         */

        bool duplicateKeyThrown = false;
        try
        {
            catalog.ToDictionary(item => item.Sku); // catalog has duplicate WH-4412
        }
        catch (ArgumentException)
        {
            duplicateKeyThrown = true;
        }

        Console.WriteLine();
        Console.WriteLine("--- Duplicate key guard ---");
        Console.WriteLine($"ToDictionary on duplicate SKU threw ArgumentException: {duplicateKeyThrown}");


        /*
         * =========================================================================
         * SECTION 6: ToLookup() — MULTI-VALUE KEYED INDEX
         * =========================================================================
         *
         * ToLookup builds ILookup<TKey, TElement> immediately — a dictionary-like
         * structure where each key maps to a SEQUENCE of elements (one-to-many).
         *
         *   ILookup<TKey, TElement> ToLookup(keySelector)
         *   ILookup<TKey, TElement> ToLookup(keySelector, elementSelector)
         *   ILookup<TKey, TElement> ToLookup(keySelector, comparer)
         *
         * Contrast with related operators:
         *
         *  Operator       | Keys        | Execution | Access by key
         *  ---------------|-------------|-----------|---------------------------
         *  GroupBy        | one→many    | Deferred  | enumerate groupings
         *  ToLookup       | one→many    | Immediate | lookup[key] → IEnumerable
         *  ToDictionary   | one→one     | Immediate | dict[key] → single value
         *
         * ILookup quirks:
         *   • Missing keys return an EMPTY sequence — no KeyNotFoundException
         *   • Duplicate keys are EXPECTED (that is the point)
         *   • ILookup is read-only after construction
         *
         * GroupBy depth (IGrouping, query syntax, nested groups) lives in
         * COVERED IN DETAIL LATER → 04. Grouping
         * (that chapter also previews ToLookup; this chapter owns the conversion
         * angle — materialization family and ToDictionary contrast).
         *
         * --- 6a. Index catalog by category ---
         * -------------------------------------------------------------------------
         */

        ILookup<ItemCategory, InventoryItem> byCategory =
            liveCatalog.ToLookup(item => item.Category); // immediate multi-map

        Console.WriteLine();
        Console.WriteLine("--- ToLookup() by category ---");
        Console.WriteLine($"Distinct category keys: {byCategory.Count}");
        Console.WriteLine($"Equipment count: {byCategory[ItemCategory.Equipment].Count()}");
        foreach (InventoryItem item in byCategory[ItemCategory.Equipment].OrderBy(i => i.Sku))
        {
            Console.WriteLine($"  EQUIP  {item.Sku}  {item.Name}");
        }

        int missingCategoryCount = byCategory[(ItemCategory)999].Count(); // missing → empty, no throw
        Console.WriteLine($"Missing key → empty sequence, Count = {missingCategoryCount}");

        /*
         * --- 6b. Element selector — project values stored under each key ---
         * -------------------------------------------------------------------------
         */

        ILookup<ItemCategory, string> skusByCategory =
            liveCatalog.ToLookup(
                item => item.Category,
                item => item.Sku); // store SKU strings under each category key

        Console.WriteLine();
        Console.WriteLine("--- ToLookup with element selector ---");
        Console.WriteLine(
            "Hardware SKUs: " + string.Join(", ", skusByCategory[ItemCategory.Hardware]));


        /*
         * =========================================================================
         * SECTION 7: Cast<T>() — CONVERT EVERY ELEMENT TO T
         * =========================================================================
         *
         * Cast<T>() applies an explicit cast to each element as you enumerate.
         * Works on non-generic IEnumerable and when you need IEnumerable<Derived>
         * from a base-typed sequence.
         *
         * Cast is DEFERRED — the cast happens per element during enumeration, not
         * when you call Cast. If ANY element is not assignable to T, enumeration
         * throws InvalidCastException (no skip behavior).
         *
         * Legacy import feed: ArrayList / object[] with mixed types simulates
         * pre-generics APIs and COM interop payloads.
         *
         * --- 7a. Successful Cast on compatible elements ---
         * -------------------------------------------------------------------------
         */

        IEnumerable legacyFeed = BuildLegacyImportFeed();

        IEnumerable<InventoryItem> typedItems = legacyFeed.Cast<InventoryItem>(); // deferred casts

        Console.WriteLine();
        Console.WriteLine("--- Cast<InventoryItem>() on clean legacy rows ---");
        foreach (InventoryItem item in typedItems)
        {
            Console.WriteLine($"  CAST  {item.Sku}  {item.Name}");
        }

        /*
         * --- 7b. Cast fails when an element cannot convert ---
         *
         * InvalidCastException — same family as a bad unbox cast in C# fundamentals.
         * -------------------------------------------------------------------------
         */

        IEnumerable feedWithBadRow = BuildLegacyImportFeed(includeNonInventoryRow: true);

        bool castFailed = false;
        try
        {
            foreach (InventoryItem _ in feedWithBadRow.Cast<InventoryItem>())
            {
                // throws when the non-InventoryItem row is encountered
            }
        }
        catch (InvalidCastException)
        {
            castFailed = true;
        }

        Console.WriteLine();
        Console.WriteLine("--- Cast failure on mixed feed ---");
        Console.WriteLine($"Cast<InventoryItem> threw InvalidCastException: {castFailed}");


        /*
         * =========================================================================
         * SECTION 8: OfType<T>() — PREVIEW (FILTERING CHAPTER OWNS DEPTH)
         * =========================================================================
         *
         * OfType<T>() yields only elements assignable to T. Incompatible elements
         * are skipped — no exception. Same deferred execution model as Cast.
         *
         *  Operator   | On mismatch              | Use when
         *  -----------|--------------------------|----------------------------------
         *  Cast<T>    | InvalidCastException     | You expect ALL elements to be T
         *  OfType<T>  | Skip element, continue   | Mixed bag; extract one type safely
         *
         * COVERED IN DETAIL LATER → 02. Filtering & Aggregation
         *   (OfType as a filter operator, inheritance hierarchies, object[])
         *
         * Here: one contrast demo — same mixed feed where Cast failed.
         * -------------------------------------------------------------------------
         */

        IEnumerable mixedFeed = BuildLegacyImportFeed(includeNonInventoryRow: true);
        List<InventoryItem> safeExtract = mixedFeed.OfType<InventoryItem>().ToList();

        Console.WriteLine();
        Console.WriteLine("--- OfType preview (same mixed feed) ---");
        Console.WriteLine($"Extracted {safeExtract.Count} inventory row(s) (non-inventory skipped):");
        foreach (InventoryItem item in safeExtract)
        {
            Console.WriteLine($"  OFTYPE  {item.Sku}");
        }


        /*
         * =========================================================================
         * SECTION 9: AsEnumerable() — FORCE Enumerable EXTENSION CHAINING
         * =========================================================================
         *
         * AsEnumerable() returns IEnumerable<T> without copying elements.
         * It is NOT materialization — execution stays deferred.
         *
         * Primary use: when the static type is a concrete collection that exposes
         * its own Where/Select-like methods, AsEnumerable() hides those members so
         * subsequent calls bind to System.Linq.Enumerable extensions instead.
         *
         * InventoryCatalogCollection (Section B) defines a Where that logs and runs
         * eagerly. Wrapping with AsEnumerable() restores standard deferred LINQ.
         *
         * --- 9a. Without AsEnumerable — custom collection Where runs ---
         * -------------------------------------------------------------------------
         */

        InventoryCatalogCollection concreteCatalog = new InventoryCatalogCollection(liveCatalog);

        Console.WriteLine();
        Console.WriteLine("--- AsEnumerable() extension resolution ---");
        Console.WriteLine("Concrete collection Where (eager, custom):");
        List<InventoryItem> eagerResult = concreteCatalog
            .Where(item => item.StockQty > 0)
            .ToList();
        Console.WriteLine($"  Eager Where returned {eagerResult.Count} item(s)");

        /*
         * --- 9b. With AsEnumerable — Enumerable.Where (deferred) ---
         * -------------------------------------------------------------------------
         */

        int deferredWhereRuns = 0;

        IEnumerable<InventoryItem> deferredChain =
            concreteCatalog
                .AsEnumerable() // bind to Enumerable.* instead of custom Where
                .Where(item =>
                {
                    deferredWhereRuns++;
                    return item.UnitPrice >= 500.00m;
                });

        Console.WriteLine();
        Console.WriteLine("After AsEnumerable().Where(...) built, deferredWhereRuns = "
            + deferredWhereRuns);

        List<InventoryItem> deferredResult = deferredChain.ToList();
        Console.WriteLine($"Deferred Where + ToList returned {deferredResult.Count} item(s); "
            + $"deferredWhereRuns = {deferredWhereRuns}");


        /*
         * =========================================================================
         * SECTION 10: AsQueryable() — PREVIEW
         * =========================================================================
         *
         * AsQueryable() wraps an IEnumerable<T> as IQueryable<T> so subsequent
         * operators bind to Queryable.* extension methods (expression trees)
         * instead of Enumerable.* (delegates).
         *
         * On a plain in-memory list, AsQueryable still runs locally — you gain
         * expression-tree shape, not a remote database round-trip. Real provider
         * translation (SQL, etc.) belongs to EF Core / LINQ providers.
         *
         * COVERED IN DETAIL LATER → EF Core modules (IQueryable translation)
         *
         * --- 10a. In-memory AsQueryable still enumerates locally ---
         * -------------------------------------------------------------------------
         */

        IQueryable<InventoryItem> queryableCatalog = liveCatalog.AsQueryable();

        List<string> expensiveSkus =
            queryableCatalog
                .Where(item => item.UnitPrice >= 500.00m)
                .Select(item => item.Sku)
                .ToList(); // still local execution for List.AsQueryable()

        Console.WriteLine();
        Console.WriteLine("--- AsQueryable() preview (in-memory) ---");
        Console.WriteLine($"Provider type: {queryableCatalog.Provider.GetType().Name}");
        Console.WriteLine("Expensive SKUs: " + string.Join(", ", expensiveSkus));


        /*
         * =========================================================================
         * SECTION 11: PUTTING IT TOGETHER — ORDER VALIDATION
         * =========================================================================
         *
         * Combine ToDictionary lookup + deferred Select + ToList materialization:
         * validate order lines against the SKU map, return confirmed lines as a list.
         * -------------------------------------------------------------------------
         */

        string[] orderSkus = ["WH-3305", "WH-2201", "WH-9999"];

        List<(string Sku, string Status, decimal? Price)> validationResults =
            orderSkus
                .Select(sku =>
                {
                    if (skuLookup.TryGetValue(sku, out InventoryItem? found))
                    {
                        string status = found.StockQty > 0 ? "OK" : "OUT_OF_STOCK";
                        return (Sku: sku, Status: status, Price: (decimal?)found.UnitPrice);
                    }

                    return (Sku: sku, Status: "NOT_FOUND", Price: (decimal?)null);
                })
                .ToList(); // materialize validation rows for the caller

        Console.WriteLine();
        Console.WriteLine("=== Order validation (ToDictionary + Select + ToList) ===");
        foreach ((string sku, string status, decimal? price) in validationResults)
        {
            string priceText = price.HasValue ? price.Value.ToString("C") : "n/a";
            Console.WriteLine($"  {sku,-10}  {status,-12}  {priceText}");
        }
    }

    private static void PrintCatalog(IEnumerable<InventoryItem> items)
    {
        foreach (InventoryItem item in items)
        {
            Console.WriteLine($"  {item.Sku,-10}  {item.Name,-32}  {item.UnitPrice,8:C}  qty={item.StockQty,4}");
        }
    }

    private static void ProcessSkuBatch(InventoryItem[] batch)
    {
        Console.WriteLine($"  ProcessSkuBatch received array[{batch.Length}] for downstream API");
    }

    private static IEnumerable BuildLegacyImportFeed(bool includeNonInventoryRow = false)
    {
        ArrayList feed = new ArrayList
        {
            new InventoryItem("LEG-100", "Legacy Pallet Wrap", 8.50m, 200, ItemCategory.Hardware),
            new InventoryItem("LEG-200", "Legacy Stretch Film", 6.25m, 150, ItemCategory.Hardware),
        };

        if (includeNonInventoryRow)
        {
            feed.Add("CORRUPT-ROW-NOT-AN-ITEM"); // wrong runtime type for Cast<InventoryItem>
        }

        return feed;
    }
}

/*
 * =========================================================================
 * QUICK REFERENCE — CONVERSION OPERATIONS
 * =========================================================================
 *
 * --- Execution mode ---
 *
 *   Deferred   Where, Select, OrderBy, Cast, OfType, AsEnumerable, AsQueryable
 *   Immediate  ToList, ToArray, ToHashSet, ToDictionary, ToLookup, Count, First…
 *
 * --- Materialization (forces execution) ---
 *
 *   ToList()                 → List<T>        snapshot, resizable
 *   ToArray()                → T[]            fixed size / array APIs
 *   ToHashSet()              → HashSet<T>     unique set; optional comparer
 *   ToDictionary(k)          → Dictionary     unique keys required
 *   ToDictionary(k, e)       → Dictionary     custom value projection
 *   ToLookup(k)              → ILookup        one key → many elements
 *   ToLookup(k, e)           → ILookup        projected elements per key
 *
 * --- Typing / provider binding (deferred; no copy) ---
 *
 *   AsEnumerable()           bind to Enumerable.* on concrete types
 *   AsQueryable()            bind to Queryable.* (expression trees; EF later)
 *   Cast<T>()                all elements must cast; else InvalidCastException
 *   OfType<T>()              skip non-T elements — FULL in 02. Filtering
 *
 * --- ToDictionary vs ToLookup vs GroupBy ---
 *
 *  Need                         | Operator
 *  -----------------------------|---------------------------
 *  Unique key → one value       | ToDictionary
 *  Key → many values, indexed   | ToLookup (immediate)
 *  Key → many values, stream    | GroupBy (deferred) — ch.04
 *
 * --- Cast vs OfType ---
 *
 *  Expect every element is T    | Cast<T>
 *  Mixed feed; keep only T      | OfType<T> (preview here; FULL in ch.02)
 *
 * --- Common mistakes ---
 *
 *  Mistake                              | Result
 *  -------------------------------------|----------------------------------
 *  ToDictionary with duplicate keys     | ArgumentException
 *  Assuming ToList tracks source adds   | Structural snapshot only
 *  Cast on mixed types                  | InvalidCastException mid-enum
 *  Using Cast when feed may be mixed    | Prefer OfType
 *  Treating AsEnumerable as a copy      | It only changes binding / type
 *
 * --- Related chapters ---
 *
 *   01. Introduction to LINQ       — deferred vs immediate
 *   02. Filtering & Aggregation    — OfType in depth
 *   04. Grouping                   — GroupBy; ToLookup preview
 *   07. Set Operations             — Distinct vs ToHashSet uniqueness
 *   EF Core modules                — real IQueryable providers
 *
 * =========================================================================
 */
