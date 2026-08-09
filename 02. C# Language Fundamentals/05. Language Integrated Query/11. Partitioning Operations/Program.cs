/*
 * =============================================================================
 * 11. PARTITIONING OPERATIONS — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: LINQ partitioning operators — Take, Skip, TakeWhile, SkipWhile,
 *        TakeLast, SkipLast, and Chunk (.NET 6+) — plus Skip + Take paging
 *        patterns for fixed-size windows over a sequence.
 *
 * WHY IT MATTERS:
 *   APIs and UIs rarely show an entire dataset at once. You need the first
 *   page of search results, the last N audit rows, batches for export, or a
 *   prefix of items that still satisfy a rule. Partitioning operators express
 *   those slices declaratively instead of manual index arithmetic in for-loops.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Partitioning overview — window vs filter, streaming vs buffering
 *   2.  Take / Skip — fixed-count windows from the start
 *   3.  TakeWhile / SkipWhile — condition-based prefixes (order-sensitive)
 *   4.  TakeLast / SkipLast — windows from the end (buffering cost)
 *   5.  Chunk — split a sequence into fixed-size arrays (.NET 6+)
 *   6.  Pagination with Skip + Take and a reusable GetPage helper
 *   7.  Empty sources, oversized counts, and deferred execution
 *
 * =============================================================================
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace PartitioningOperations;

/*
 * =========================================================================
 * SECTION 1: SAMPLE TYPE — Product (catalog row)
 * =========================================================================
 *
 * Immutable catalog row used throughout the demos. IsActiveInStock combines
 * the active flag and quantity so TakeWhile / SkipWhile examples can talk
 * about "sellable stock" without repeating two checks in every lambda.
 *
 * The seeded catalog below intentionally starts with out-of-stock rows so
 * SkipWhile has a real leading prefix to drop (order matters for While*).
 * -------------------------------------------------------------------------
 */
public readonly record struct Product(
    string Sku,
    string Name,
    decimal UnitPrice,
    int StockQty,
    bool IsActive)
{
    public bool IsActiveInStock => IsActive && StockQty > 0; // both flag and qty must allow sale
}

public class Program
{
    public static void Main(string[] args)
    {
        /*
         * =========================================================================
         * SECTION 2: PARTITIONING OVERVIEW
         * =========================================================================
         *
         * Partitioning operators window or split an IEnumerable<T> without
         * mutating the source. They live in System.Linq (method syntax only —
         * query expressions have no take/skip/chunk keywords).
         *
         *  Operator     | What it returns                         | Streaming?
         *  -------------|-----------------------------------------|------------------
         *  Take(n)      | Up to first n elements                  | Yes
         *  Skip(n)      | Everything after first n                | Yes
         *  TakeWhile    | Prefix while predicate true             | Yes (stops early)
         *  SkipWhile    | Rest after leading predicate-true run   | Yes
         *  TakeLast(n)  | Last n elements                         | No — buffers
         *  SkipLast(n)  | All but the last n                      | No — buffers
         *  Chunk(size)  | Consecutive T[] batches of size         | Yes (per chunk)
         *
         * Partitioning vs filtering (ch.02):
         *   Where keeps every match anywhere in the sequence.
         *   TakeWhile / SkipWhile only consider a contiguous prefix from the start.
         *
         * COVERED IN DETAIL LATER → 06. Element Operations
         *   ElementAt picks one index; Take/Skip return a slice.
         *
         * Scenario: warehouse safety-gear catalog for an inventory portal.
         * -------------------------------------------------------------------------
         */

        Product[] catalog =
        [
            new Product("SKU-001", "Reflective Vest", 18.75m, 0, false),       // out of stock — leads SkipWhile
            new Product("SKU-002", "Safety Goggles", 11.50m, 0, false),        // out of stock
            new Product("SKU-003", "Safety Gloves (Box)", 14.99m, 240, true),
            new Product("SKU-004", "Hard Hat — Yellow", 22.50m, 85, true),
            new Product("SKU-005", "Steel Toe Boots", 89.99m, 42, true),       // breaks TakeWhile(price < 30)
            new Product("SKU-006", "Ear Protection Plugs", 6.25m, 500, true),
            new Product("SKU-007", "First Aid Kit — Large", 54.00m, 18, true),
            new Product("SKU-008", "Fire Extinguisher 5lb", 79.00m, 12, true),
            new Product("SKU-009", "Work Gloves — Leather", 19.99m, 130, true),
            new Product("SKU-010", "Hi-Vis Rain Jacket", 45.00m, 27, true),
        ];

        // Stable sort before paging — OrderBy depth is in ch.03 Ordering.
        IOrderedEnumerable<Product> sortedCatalog = catalog.OrderBy(p => p.Sku);

        Console.WriteLine("=== Catalog (sorted by SKU) ===");
        PrintProductLines(sortedCatalog);


        /*
         * =========================================================================
         * SECTION 3: Take — FIRST N ELEMENTS
         * =========================================================================
         *
         * Take(int count) yields at most count elements from the start.
         *
         *   IEnumerable<T> firstThree = source.Take(3);
         *
         * Rules:
         *   • count > length → entire sequence (no throw)
         *   • count == 0     → empty
         *   • count < 0      → ArgumentOutOfRangeException
         *
         * Common uses: "top 5" lists, preview rows, limiting API payload size
         * when combined with OrderBy / OrderByDescending.
         * -------------------------------------------------------------------------
         */

        IEnumerable<Product> topThree = sortedCatalog.Take(3); // at most first 3

        Console.WriteLine();
        Console.WriteLine("--- Take(3) — first three SKUs ---");
        PrintProductLines(topThree);


        /*
         * =========================================================================
         * SECTION 4: Skip — BYPASS FIRST N ELEMENTS
         * =========================================================================
         *
         * Skip(int count) bypasses the first count elements and yields the rest.
         *
         *   IEnumerable<T> afterFour = source.Skip(4);
         *
         * Rules:
         *   • count >= length → empty (not an error)
         *   • count == 0      → entire sequence
         *   • count < 0       → ArgumentOutOfRangeException
         *
         * Pair Skip with Take for page offsets (Section 9).
         * -------------------------------------------------------------------------
         */

        IEnumerable<Product> afterFirstFour = sortedCatalog.Skip(4); // drop SKU-001..SKU-004

        Console.WriteLine();
        Console.WriteLine("--- Skip(4) — catalog from fifth item onward ---");
        PrintProductLines(afterFirstFour);


        /*
         * =========================================================================
         * SECTION 5: TakeWhile — PREFIX WHILE PREDICATE IS TRUE
         * =========================================================================
         *
         * TakeWhile(Func<T, bool> predicate) scans from the start and yields
         * elements while the predicate returns true. It STOPS at the first
         * false — it does not resume when a later element would match again.
         *
         *   TakeWhile(p => p.UnitPrice < 30m)
         *
         * Overload TakeWhile(Func<T, int, bool>) also receives the zero-based
         * index — useful for "first rows while index < N AND condition".
         *
         * Because evaluation stops early, order matters. For "all items under
         * $30" regardless of position, use Where (ch.02 Filtering) instead.
         *
         * TakeWhile answers: "Give me the initial run that still satisfies X."
         * -------------------------------------------------------------------------
         */

        // Sorted: Vest 18.75, Goggles 11.50, Gloves 14.99, Hard Hat 22.50, Boots 89.99…
        IEnumerable<Product> affordablePrefix =
            sortedCatalog.TakeWhile(p => p.UnitPrice < 30m); // stops at first >= 30 (Boots)

        // Index overload: under $30 AND still in the first 5 slots (0..4).
        IEnumerable<Product> affordableInFirstFive =
            sortedCatalog.TakeWhile((p, index) => p.UnitPrice < 30m && index < 5);

        Console.WriteLine();
        Console.WriteLine("--- TakeWhile(unit price < 30) — stops at first >= 30 ---");
        PrintProductLines(affordablePrefix);

        Console.WriteLine();
        Console.WriteLine("--- TakeWhile(price < 30 && index < 5) — index overload ---");
        PrintProductLines(affordableInFirstFive);


        /*
         * =========================================================================
         * SECTION 6: SkipWhile — DROP PREFIX WHILE PREDICATE IS TRUE
         * =========================================================================
         *
         * SkipWhile(Func<T, bool> predicate) skips elements from the start while
         * the predicate is true. It starts yielding at the first element where
         * the predicate is false (that element is included).
         *
         *   SkipWhile(p => !p.IsActiveInStock)
         *
         * Only the leading contiguous run is considered. An out-of-stock item
         * later in the list is NOT skipped by SkipWhile — use Where for that.
         *
         * SkipWhile(Func<T, int, bool>) adds the zero-based index to the test.
         *
         * Catalog starts with two out-of-stock SKUs, then in-stock gloves —
         * SkipWhile drops the leading dead stock and yields from SKU-003 on.
         * -------------------------------------------------------------------------
         */

        IEnumerable<Product> onceInStockStarts =
            sortedCatalog.SkipWhile(p => !p.IsActiveInStock); // yield from first sellable row

        // Index overload: skip while out of stock AND still in the first 3 slots.
        IEnumerable<Product> skipLeadingDeadStock =
            sortedCatalog.SkipWhile((p, index) => !p.IsActiveInStock && index < 3);

        Console.WriteLine();
        Console.WriteLine("--- SkipWhile(not in stock) — yield from first in-stock row ---");
        PrintProductLines(onceInStockStarts);

        Console.WriteLine();
        Console.WriteLine("--- SkipWhile(not in stock && index < 3) — index overload ---");
        PrintProductLines(skipLeadingDeadStock);


        /*
         * =========================================================================
         * SECTION 7: TakeLast — LAST N ELEMENTS
         * =========================================================================
         *
         * TakeLast(int count) returns the final count elements of the sequence.
         *
         *   IEnumerable<T> lastTwo = source.TakeLast(2);
         *
         * Unlike Take, TakeLast cannot stream: it must buffer (or know Count)
         * to discover the end. On a plain IEnumerable<T> that means reading
         * the whole source before the first result is yielded.
         *
         * Rules:
         *   • count >= length → entire sequence
         *   • count == 0      → empty
         *   • count < 0       → ArgumentOutOfRangeException
         *
         * Typical uses: "latest 5 log lines" after OrderBy timestamp, or the
         * trailing window of an already-ordered list.
         * -------------------------------------------------------------------------
         */

        IEnumerable<Product> lastThree = sortedCatalog.TakeLast(3); // SKU-008..SKU-010

        Console.WriteLine();
        Console.WriteLine("--- TakeLast(3) — final three SKUs ---");
        PrintProductLines(lastThree);


        /*
         * =========================================================================
         * SECTION 8: SkipLast — ALL BUT THE LAST N ELEMENTS
         * =========================================================================
         *
         * SkipLast(int count) yields every element except the trailing count.
         *
         *   IEnumerable<T> withoutTail = source.SkipLast(2);
         *
         * Like TakeLast, SkipLast buffers: it keeps a sliding window of size
         * count so it can withhold the eventual tail. Prefer Take/Skip when
         * you already know a start offset and do not need "from the end".
         *
         * Rules:
         *   • count >= length → empty
         *   • count == 0      → entire sequence
         *   • count < 0       → ArgumentOutOfRangeException
         * -------------------------------------------------------------------------
         */

        IEnumerable<Product> withoutLastTwo = sortedCatalog.SkipLast(2); // drop SKU-009, SKU-010

        Console.WriteLine();
        Console.WriteLine("--- SkipLast(2) — everything except the final two ---");
        PrintProductLines(withoutLastTwo);


        /*
         * =========================================================================
         * SECTION 9: Chunk — FIXED-SIZE BATCHES (.NET 6+)
         * =========================================================================
         *
         * Chunk(int size) splits the source into consecutive arrays of length
         * size. The final chunk may be shorter when the count is not divisible.
         *
         *   IEnumerable<T[]> batches = source.Chunk(3);
         *
         * Each yielded element is a T[] (a materialized slice for that batch).
         * size must be >= 1; otherwise ArgumentOutOfRangeException.
         *
         * Use Chunk when you process or display "batches of N" without caring
         * about a 1-based page number. For classic API paging (page 2 of 4),
         * Skip + Take (Section 10) is usually clearer.
         *
         * Chunk vs Skip/Take paging:
         *
         *  Need                         | Prefer
         *  -----------------------------|---------------------------
         *  Process every batch once     | Chunk(size)
         *  Fetch page k on demand       | Skip(offset).Take(size)
         *  Last partial batch OK        | Both (Chunk yields shorter tail)
         * -------------------------------------------------------------------------
         */

        Console.WriteLine();
        Console.WriteLine("--- Chunk(3) — batches of three SKUs ---");

        int batchNumber = 0;
        foreach (Product[] batch in sortedCatalog.Chunk(3)) // last batch may be shorter
        {
            batchNumber++;
            Console.WriteLine($"Batch {batchNumber} ({batch.Length} items):");
            PrintProductLines(batch, indent: "  ");
        }


        /*
         * =========================================================================
         * SECTION 10: PAGINATION WITH Skip + Take
         * =========================================================================
         *
         * Fixed-size pages over an ordered sequence:
         *
         *   pageNumber = 1-based index (first page = 1)
         *   pageSize   = items per page
         *   offset     = (pageNumber - 1) * pageSize
         *
         *   IEnumerable<T> page = source
         *       .Skip(offset)
         *       .Take(pageSize);
         *
         * Rules:
         *   • Order BEFORE Skip/Take (OrderBy in ch.03 Ordering).
         *   • Last page may have fewer than pageSize items.
         *   • Page number beyond the last page returns empty (not an error).
         *
         * Chunk walks every batch sequentially; Skip/Take jumps to one page.
         * For a single index without a window, see ElementAt in ch.06.
         *
         * GetPage (Section 12) encapsulates the offset math and validates inputs.
         * -------------------------------------------------------------------------
         */

        const int pageSize = 3;
        int totalItems = sortedCatalog.Count();
        int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        Console.WriteLine();
        Console.WriteLine($"--- Paging (page size {pageSize}, {totalPages} pages) ---");

        for (int pageNumber = 1; pageNumber <= totalPages; pageNumber++)
        {
            IEnumerable<Product> page = GetPage(sortedCatalog, pageNumber, pageSize);
            Console.WriteLine($"Page {pageNumber}:");
            PrintProductLines(page, indent: "  ");
        }

        // Beyond last page — empty, not an exception
        IEnumerable<Product> emptyPage = GetPage(sortedCatalog, pageNumber: 99, pageSize);
        Console.WriteLine();
        Console.WriteLine($"Page 99 (beyond range): {emptyPage.Count()} items");

        Console.WriteLine();
        Console.WriteLine("--- GetPage(2, 4) — second page, four per page ---");
        PrintProductLines(GetPage(sortedCatalog, pageNumber: 2, pageSize: 4), indent: "  ");


        /*
         * =========================================================================
         * SECTION 11: EDGE CASES — EMPTY, OVERSIZED, ZERO, DEFERRED
         * =========================================================================
         *
         * Empty source:
         *   Take / Skip / TakeWhile / SkipWhile / TakeLast / SkipLast / Chunk
         *   all yield empty (Chunk yields no arrays).
         *
         * Oversized counts:
         *   Take(100) on 3 items     → 3 items
         *   Skip(100) on 3 items     → empty
         *   TakeLast(100) on 3 items → 3 items
         *   SkipLast(100) on 3 items → empty
         *
         * Zero counts:
         *   Take(0) / TakeLast(0) → empty
         *   Skip(0) / SkipLast(0) → entire sequence
         *
         * Predicate edge cases on While*:
         *   TakeWhile — first element fails → empty
         *   SkipWhile — first element fails → entire sequence (nothing skipped)
         *   TakeWhile — all true → entire sequence
         *   SkipWhile — all true → empty
         *
         * Negative count on Take / Skip / TakeLast / SkipLast →
         * ArgumentOutOfRangeException. Chunk size < 1 → same.
         *
         * Take / Skip / TakeWhile / SkipWhile / Chunk return deferred
         * IEnumerable<T> — enumeration walks the source at foreach time.
         * TakeLast / SkipLast also expose IEnumerable<T> but must buffer
         * before yielding (end-relative windows).
         *
         * IQueryable<T> (EF Core) reuses Take/Skip names; providers often
         * translate them to SQL OFFSET/FETCH — preview only here.
         * -------------------------------------------------------------------------
         */

        int[] smallBatch = [10, 20, 30];
        int[] emptySource = [];

        Console.WriteLine();
        Console.WriteLine("--- Edge cases on int[] { 10, 20, 30 } ---");
        Console.WriteLine($"Take(10):      {FormatInts(smallBatch.Take(10))}");
        Console.WriteLine($"Skip(10):      {FormatInts(smallBatch.Skip(10))} (empty)");
        Console.WriteLine($"Take(0):       {FormatInts(smallBatch.Take(0))} (empty)");
        Console.WriteLine($"Skip(0):       {FormatInts(smallBatch.Skip(0))}");
        Console.WriteLine($"TakeLast(2):   {FormatInts(smallBatch.TakeLast(2))}");
        Console.WriteLine($"SkipLast(1):   {FormatInts(smallBatch.SkipLast(1))}");
        Console.WriteLine($"TakeLast(0):   {FormatInts(smallBatch.TakeLast(0))} (empty)");
        Console.WriteLine($"SkipLast(0):   {FormatInts(smallBatch.SkipLast(0))}");
        Console.WriteLine($"Chunk(2) lens: {string.Join(", ", smallBatch.Chunk(2).Select(c => c.Length))}");

        Console.WriteLine();
        Console.WriteLine("--- Empty source ---");
        Console.WriteLine($"Take(3):       {FormatInts(emptySource.Take(3))} (empty)");
        Console.WriteLine($"Skip(1):       {FormatInts(emptySource.Skip(1))} (empty)");
        Console.WriteLine($"TakeLast(2):   {FormatInts(emptySource.TakeLast(2))} (empty)");
        Console.WriteLine($"SkipLast(1):   {FormatInts(emptySource.SkipLast(1))} (empty)");
        Console.WriteLine($"Chunk(2) count:{emptySource.Chunk(2).Count()}");

        Console.WriteLine();
        Console.WriteLine("--- While* predicate edges on { 10, 20, 30 } ---");
        Console.WriteLine($"TakeWhile(x > 100): {FormatInts(smallBatch.TakeWhile(x => x > 100))} (empty — first fails)");
        Console.WriteLine($"SkipWhile(x > 100): {FormatInts(smallBatch.SkipWhile(x => x > 100))} (nothing skipped)");
        Console.WriteLine($"TakeWhile(x > 0):   {FormatInts(smallBatch.TakeWhile(x => x > 0))} (all)");
        Console.WriteLine($"SkipWhile(x > 0):   {FormatInts(smallBatch.SkipWhile(x => x > 0))} (empty — all skipped)");

        // Deferred: query defined here; executes when joined below.
        IEnumerable<int> deferredWindow = smallBatch.Skip(1).Take(2);
        Console.WriteLine();
        Console.WriteLine($"Skip(1).Take(2) materialized: {FormatInts(deferredWindow)}");
    }

    /*
     * =========================================================================
     * SECTION 12: GetPage — REUSABLE 1-BASED PAGING HELPER
     * =========================================================================
     *
     * offset = (pageNumber - 1) * pageSize
     * Then Skip(offset).Take(pageSize).
     *
     * Throws ArgumentOutOfRangeException when pageNumber or pageSize < 1 so
     * callers cannot silently produce a negative Skip offset. Encapsulating
     * the math avoids copy-paste bugs (0-based vs 1-based page numbers).
     * -------------------------------------------------------------------------
     */
    public static IEnumerable<T> GetPage<T>(IEnumerable<T> source, int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be >= 1.");
        }

        if (pageSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be >= 1.");
        }

        int offset = (pageNumber - 1) * pageSize; // convert 1-based page to Skip count
        return source.Skip(offset).Take(pageSize);
    }

    /*
     * =========================================================================
     * SECTION 13: OUTPUT HELPERS
     * =========================================================================
     *
     * Small formatters so Main stays focused on the operators. PrintProductLines
     * labels stock; FormatInts joins ints for edge-case console lines.
     * -------------------------------------------------------------------------
     */
    public static void PrintProductLines(IEnumerable<Product> products, string indent = "")
    {
        foreach (Product product in products)
        {
            string stockLabel = product.IsActiveInStock
                ? $"in stock ({product.StockQty})"
                : "out of stock";
            Console.WriteLine(
                $"{indent}{product.Sku}  {product.Name,-28}  {product.UnitPrice,7:C}  {stockLabel}");
        }
    }

    public static string FormatInts(IEnumerable<int> values)
    {
        string joined = string.Join(", ", values);
        return string.IsNullOrEmpty(joined) ? "[]" : joined; // show empty windows clearly
    }
}

/*
 * =========================================================================
 * QUICK REFERENCE — PARTITIONING OPERATIONS
 * =========================================================================
 *
 * --- Fixed-count from the start ---
 *
 *   source.Take(n)              // first n (or all if shorter); Take(0) → empty
 *   source.Skip(n)              // after first n (empty if n >= length); Skip(0) → all
 *   source.Skip(offset).Take(pageSize)   // one page; offset = (page-1)*size
 *
 * --- Fixed-count from the end (buffers) ---
 *
 *   source.TakeLast(n)          // last n elements
 *   source.SkipLast(n)          // all but last n
 *
 * --- Condition-based prefix (order-sensitive) ---
 *
 *   source.TakeWhile(x => test)           // prefix while true; stop at first false
 *   source.TakeWhile((x, i) => test)      // same + zero-based index
 *   source.SkipWhile(x => test)           // skip prefix while true; start at first false
 *   source.SkipWhile((x, i) => test)      // same + zero-based index
 *
 * --- Batches (.NET 6+) ---
 *
 *   source.Chunk(size)          // IEnumerable<T[]>; last chunk may be shorter
 *
 * --- Paging helper pattern ---
 *
 *   static IEnumerable<T> GetPage<T>(IEnumerable<T> src, int page, int size)
 *       => src.Skip((page - 1) * size).Take(size);   // validate page/size >= 1
 *
 * --- vs related operators ---
 *
 *  Goal                         | Use
 *  -----------------------------|------------------------------------------
 *  First N after sorting        | OrderBy… then Take(N)
 *  Last N after sorting         | OrderBy… then TakeLast(N)
 *  All items matching filter    | Where (ch.02) — not TakeWhile
 *  Process every batch once     | Chunk(size)
 *  Fetch page k on demand       | Skip + Take / GetPage
 *  Single item at index i       | ElementAt(i) (ch.06)
 *  Reverse entire sequence      | Reverse (ch.03 Ordering)
 *
 * --- Common mistakes ---
 *
 *  Mistake                              | Result
 *  -------------------------------------|----------------------------------
 *  Paging without OrderBy               | Unstable page contents
 *  TakeWhile for "all matching"         | Misses matches after first gap
 *  TakeLast on huge lazy stream         | Buffers entire sequence
 *  Negative Take/Skip/Chunk size        | ArgumentOutOfRangeException
 *  Assuming Take fills last page        | May return fewer than pageSize
 *  SkipWhile for every matching row     | Only drops a leading prefix
 *
 * --- Related chapters ---
 *
 *   03. Ordering                  — OrderBy before paging
 *   02. Filtering and Aggregation — Where vs TakeWhile
 *   06. Element Operations        — ElementAt, First, Last
 *   10. Conversion Operations     — ToList to snapshot a page
 *   12. Generation Operations     — Range / Repeat for synthetic sources
 *
 * =========================================================================
 */
