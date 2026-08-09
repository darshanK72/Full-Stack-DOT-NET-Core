/*
 * =============================================================================
 * 03. ORDERING — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: LINQ ordering operators — OrderBy, OrderByDescending, ThenBy,
 *        ThenByDescending, Reverse — plus IOrderedEnumerable<T>, sort
 *        stability, and custom IComparer<TKey> key comparison. Method syntax
 *        and query syntax (orderby / descending) for the same operations.
 *
 * WHY IT MATTERS:
 *   Reports, pick lists, leaderboards, and paginated grids almost always need
 *   sorted data. LINQ ordering builds on IEnumerable<T> and composes with
 *   Where / Select from sibling chapters. Multi-key sorts (priority then date)
 *   are one ThenBy away once you know the pattern — and knowing that a second
 *   OrderBy replaces the sort (instead of ThenBy) prevents a common bug.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Ordering operators overview — returns, deferred execution, stability
 *   2.  OrderBy — ascending primary sort (method + query syntax)
 *   3.  OrderByDescending — descending primary sort
 *   4.  ThenBy / ThenByDescending — secondary keys on IOrderedEnumerable
 *   5.  IOrderedEnumerable<T> — why the return type matters (ThenBy vs OrderBy)
 *   6.  Stable sort — equal keys keep relative source order
 *   7.  Custom IComparer<TKey> — StringComparer, Comparer.Create, domain rules
 *   8.  Reverse — flip current order (not a key-based sort)
 *   9.  Parallel LINQ (PLINQ) ordering — preview only
 *
 * =============================================================================
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Ordering;

/*
 * =========================================================================
 * SECTION 1: SAMPLE MODEL — FULFILLMENT LINES
 * =========================================================================
 *
 * Demo data for a warehouse fulfillment center. Each line is one open order
 * waiting to be picked. Fields used as sort keys later:
 *
 *  Field        | Used for
 *  -------------|----------------------------------------------------------
 *  Priority     | Higher number ships first (3 > 2 > 1)
 *  PlacedAt     | Oldest first within the same priority
 *  LineTotal    | Highest revenue first on reports
 *  Customer     | Alphabetical grouping; case variants for comparer demos
 *  Zone         | Warehouse aisle letter (custom string rules)
 *
 * A record keeps construction short; ordering operators work the same on
 * classes, structs, and anonymous types.
 * -------------------------------------------------------------------------
 */
public readonly record struct FulfillmentLine(
    string OrderId,
    string Customer,
    int Priority,
    decimal LineTotal,
    DateTime PlacedAt,
    string Zone);

/*
 * =========================================================================
 * SECTION 2: CUSTOM IComparer — ZONE SORT RULES
 * =========================================================================
 *
 * OrderBy / ThenBy accept an optional IComparer<TKey>:
 *
 *   source.OrderBy(keySelector, comparer)
 *
 * When Comparer<TKey>.Default is wrong for your domain (case, culture, or
 * a special string format), supply your own IComparer<TKey>.
 *
 * Contract (same idea as sorting in 03. Generics & Collections):
 *
 *   Compare(x, y) < 0  →  x before y
 *   Compare(x, y) = 0  →  equal for sorting (ties → ThenBy or stable order)
 *   Compare(x, y) > 0  →  x after y
 *
 * This comparer sorts warehouse zones by aisle letter, then by numeric bin
 * when the zone looks like "A-12". Unknown formats fall back to ordinal text.
 * -------------------------------------------------------------------------
 */
public sealed class ZoneComparer : IComparer<string>
{
    public int Compare(string? x, string? y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0; // same reference (or both null) → equal
        }

        if (x is null)
        {
            return -1; // null zones sort first
        }

        if (y is null)
        {
            return 1;
        }

        if (TryParseZone(x, out char aisleX, out int binX) &&
            TryParseZone(y, out char aisleY, out int binY))
        {
            int aisleCmp = aisleX.CompareTo(aisleY); // letter first
            if (aisleCmp != 0)
            {
                return aisleCmp;
            }

            return binX.CompareTo(binY); // then numeric bin (so A-3 before A-12)
        }

        return string.Compare(x, y, StringComparison.Ordinal); // fallback
    }

    private static bool TryParseZone(string zone, out char aisle, out int bin)
    {
        aisle = default;
        bin = 0;

        string[] parts = zone.Split('-'); // expect "A-12"
        if (parts.Length != 2 || parts[0].Length != 1)
        {
            return false;
        }

        aisle = char.ToUpperInvariant(parts[0][0]);
        return int.TryParse(parts[1], out bin);
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        /*
         * =========================================================================
         * SECTION 3: ORDERING OPERATORS — OVERVIEW
         * =========================================================================
         *
         * LINQ to Objects ordering extensions live in System.Linq:
         *
         *  Operator              | Sorts? | Returns                    | Notes
         *  ----------------------|--------|----------------------------|------------------
         *  OrderBy               | Yes    | IOrderedEnumerable<T>      | Ascending key
         *  OrderByDescending     | Yes    | IOrderedEnumerable<T>      | Descending key
         *  ThenBy                | Yes    | IOrderedEnumerable<T>      | After OrderBy*
         *  ThenByDescending      | Yes    | IOrderedEnumerable<T>      | After OrderBy*
         *  Reverse               | No     | IEnumerable<T>             | Flips enumeration
         *
         * *ThenBy / ThenByDescending require a prior OrderBy or OrderByDescending.
         *  They sort within groups that share the same primary key (tie-break).
         *
         * LINQ to Objects OrderBy is a STABLE sort — equal keys keep relative
         * source order (Section 8). Execution is DEFERRED until you foreach /
         * ToList / Count / etc. (see 01. Introduction to LINQ).
         *
         * Scenario: build pick lists and revenue views from open order lines.
         * -------------------------------------------------------------------------
         */

        FulfillmentLine[] openLines =
        [
            new("ORD-1042", "Acme Corp", 3, 412.50m, new DateTime(2026, 8, 5, 9, 15, 0), "B-10"),
            new("ORD-1045", "beta llc", 1, 189.00m, new DateTime(2026, 8, 7, 14, 30, 0), "A-12"),
            new("ORD-1043", "Acme Corp", 2, 67.25m, new DateTime(2026, 8, 5, 11, 0, 0), "A-3"),
            new("ORD-1046", "Gamma Inc", 3, 1204.99m, new DateTime(2026, 8, 6, 8, 45, 0), "C-1"),
            new("ORD-1044", "Beta LLC", 2, 310.00m, new DateTime(2026, 8, 6, 16, 20, 0), "A-12"),
            new("ORD-1047", "acme corp", 1, 95.50m, new DateTime(2026, 8, 7, 10, 5, 0), "B-2"),
            new("ORD-1048", "Delta Co", 1, 55.00m, new DateTime(2026, 8, 7, 7, 50, 0), "A-3"),
        ];

        PrintLines("=== Open fulfillment lines (unsorted) ===", openLines);

        /*
         * =========================================================================
         * SECTION 4: OrderBy — ASCENDING PRIMARY SORT
         * =========================================================================
         *
         * OrderBy sorts ascending by a single key extracted from each element:
         *
         *   IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(
         *       this IEnumerable<TSource> source,
         *       Func<TSource, TKey> keySelector);
         *
         * keySelector is often a lambda: line => line.PlacedAt
         *
         * Sorting uses Comparer<TKey>.Default unless you pass an IComparer.
         * Strings use culture-aware default comparison; decimals and DateTime
         * sort naturally.
         *
         * --- 4a. Method syntax ---
         * -------------------------------------------------------------------------
         */

        IOrderedEnumerable<FulfillmentLine> byPlacedAtMethod =
            openLines.OrderBy(line => line.PlacedAt); // oldest PlacedAt first

        PrintLines("--- OrderBy PlacedAt (method) — oldest first ---", byPlacedAtMethod);

        /*
         * --- 4b. Query syntax ---
         *
         * Query syntax maps orderby to OrderBy and orderby … descending to
         * OrderByDescending. Multiple comma-separated keys map to a ThenBy chain.
         *
         *   from line in openLines
         *   orderby line.PlacedAt
         *   select line
         *
         * Equivalent to: openLines.OrderBy(line => line.PlacedAt)
         * -------------------------------------------------------------------------
         */

        IEnumerable<FulfillmentLine> byPlacedAtQuery =
            from line in openLines
            orderby line.PlacedAt
            select line;

        PrintLines("--- OrderBy PlacedAt (query) — same result ---", byPlacedAtQuery);

        /*
         * =========================================================================
         * SECTION 5: OrderByDescending — DESCENDING PRIMARY SORT
         * =========================================================================
         *
         * OrderByDescending is the mirror of OrderBy — highest key value first.
         *
         *   openLines.OrderByDescending(line => line.LineTotal)
         *
         * Query syntax adds the descending keyword:
         *
         *   orderby line.LineTotal descending
         *
         * Use when the business rule is "largest / newest / highest priority
         * number first" on the PRIMARY key.
         * -------------------------------------------------------------------------
         */

        IOrderedEnumerable<FulfillmentLine> byTotalDescMethod =
            openLines.OrderByDescending(line => line.LineTotal); // highest revenue first

        IEnumerable<FulfillmentLine> byTotalDescQuery =
            from line in openLines
            orderby line.LineTotal descending
            select line;

        PrintLines("--- OrderByDescending LineTotal (method) ---", byTotalDescMethod);
        PrintLines("--- OrderByDescending LineTotal (query) ---", byTotalDescQuery);

        /*
         * =========================================================================
         * SECTION 6: ThenBy AND ThenByDescending — MULTI-KEY SORT
         * =========================================================================
         *
         * Real pick lists rarely use one key. Warehouse rule here:
         *   1. Higher Priority number ships first (3 before 2 before 1)
         *   2. Within the same priority, oldest PlacedAt first
         *
         * Chain on the IOrderedEnumerable returned by OrderBy / OrderByDescending:
         *
         *   openLines
         *       .OrderByDescending(line => line.Priority)   // primary
         *       .ThenBy(line => line.PlacedAt);             // secondary
         *
         * ThenByDescending flips the secondary key only:
         *
         *   .OrderBy(line => line.Customer)
         *   .ThenByDescending(line => line.LineTotal)
         *
         * Calling OrderBy a second time REPLACES the entire sort — it does not
         * add a secondary key. Always use ThenBy after the first OrderBy*.
         *
         * --- 6a. Method syntax — priority then date ---
         * -------------------------------------------------------------------------
         */

        IOrderedEnumerable<FulfillmentLine> priorityThenDateMethod =
            openLines
                .OrderByDescending(line => line.Priority) // primary: rush first
                .ThenBy(line => line.PlacedAt);           // secondary: oldest within priority

        PrintLines(
            "--- OrderByDescending Priority, ThenBy PlacedAt (method) ---",
            priorityThenDateMethod);

        /*
         * --- 6b. Query syntax — comma-separated keys ---
         *
         * Keys left-to-right match OrderBy → ThenBy → ThenBy chain:
         *
         *   orderby line.Priority descending, line.PlacedAt
         *
         * First key descending, second key ascending (default).
         * -------------------------------------------------------------------------
         */

        IEnumerable<FulfillmentLine> priorityThenDateQuery =
            from line in openLines
            orderby line.Priority descending, line.PlacedAt
            select line;

        PrintLines(
            "--- orderby Priority descending, PlacedAt (query) ---",
            priorityThenDateQuery);

        /*
         * --- 6c. ThenByDescending — customer then highest line total ---
         * -------------------------------------------------------------------------
         */

        IOrderedEnumerable<FulfillmentLine> customerThenTotalDesc =
            openLines
                .OrderBy(line => line.Customer)            // primary: customer A→Z
                .ThenByDescending(line => line.LineTotal); // secondary: largest total first

        PrintLines(
            "--- OrderBy Customer, ThenByDescending LineTotal ---",
            customerThenTotalDesc);

        /*
         * =========================================================================
         * SECTION 7: IOrderedEnumerable<T> — WHY THE RETURN TYPE MATTERS
         * =========================================================================
         *
         * OrderBy / OrderByDescending / ThenBy* return IOrderedEnumerable<T>,
         * which extends IEnumerable<T> and adds ThenBy / ThenByDescending.
         *
         *  Type                        | ThenBy available? | Typical use
         *  ----------------------------|-------------------|---------------------------
         *  IEnumerable<T>              | No                | After Where / Select / Reverse
         *  IOrderedEnumerable<T>       | Yes               | After OrderBy* / ThenBy*
         *
         * Pitfall — second OrderBy replaces the sort:
         *
         *   // WRONG for multi-key: only PlacedAt remains as the ordering
         *   openLines.OrderByDescending(l => l.Priority).OrderBy(l => l.PlacedAt);
         *
         *   // RIGHT: keep Priority, break ties with PlacedAt
         *   openLines.OrderByDescending(l => l.Priority).ThenBy(l => l.PlacedAt);
         *
         * Typing the result as IOrderedEnumerable documents intent and keeps
         * ThenBy available without casting. Assigning to IEnumerable still works
         * for foreach / ToList, but drops ThenBy from IntelliSense.
         * -------------------------------------------------------------------------
         */

        IOrderedEnumerable<FulfillmentLine> ordered =
            openLines.OrderByDescending(line => line.Priority); // still IOrderedEnumerable

        IOrderedEnumerable<FulfillmentLine> withTieBreak =
            ordered.ThenBy(line => line.PlacedAt); // ThenBy only exists on IOrderedEnumerable

        // Demonstrates the pitfall: a second OrderBy replaces Priority sort
        IOrderedEnumerable<FulfillmentLine> replacedSort =
            openLines
                .OrderByDescending(line => line.Priority)
                .OrderBy(line => line.PlacedAt); // NEW primary key — Priority is lost

        PrintLines("--- IOrderedEnumerable: ThenBy keeps Priority ---", withTieBreak);
        PrintLines("--- Pitfall: second OrderBy replaces (PlacedAt only) ---", replacedSort);

        /*
         * =========================================================================
         * SECTION 8: STABLE SORT — EQUAL KEYS KEEP SOURCE ORDER
         * =========================================================================
         *
         * LINQ to Objects OrderBy / ThenBy are STABLE: when two elements compare
         * equal on the sort key(s), their relative order from the source is kept.
         *
         * Example: three priority-1 lines in declaration order
         *   ORD-1045, ORD-1047, ORD-1048
         *
         * After OrderBy(Priority) alone (no ThenBy), those three stay in that
         * same relative order among themselves — the sort does not reshuffle ties.
         *
         * Why it matters:
         *   • You can OrderBy a weak key and still preserve an earlier “natural”
         *     sequence for ties (e.g. import order, UI selection order).
         *   • ThenBy is still preferred when a business rule names a second key —
         *     stability is a fallback, not a substitute for an explicit tie-break.
         *
         * Note: other LINQ providers (EF Core → SQL) may NOT guarantee the same
         * stability; do not rely on it when ordering is translated to a database.
         * -------------------------------------------------------------------------
         */

        string[] priorityOneSourceOrder = openLines
            .Where(line => line.Priority == 1) // preview: Where depth → ch.02
            .Select(line => line.OrderId)
            .ToArray();

        string[] priorityOneAfterOrderByOnly = openLines
            .OrderBy(line => line.Priority) // all priorities; ties among pri=1 keep source order
            .Where(line => line.Priority == 1)
            .Select(line => line.OrderId)
            .ToArray();

        Console.WriteLine();
        Console.WriteLine("--- Stable sort: Priority == 1 ties ---");
        Console.WriteLine($"  Source order among pri=1:     {string.Join(", ", priorityOneSourceOrder)}");
        Console.WriteLine($"  After OrderBy(Priority) only: {string.Join(", ", priorityOneAfterOrderByOnly)}");
        Console.WriteLine(
            $"  Relative order preserved:     {priorityOneSourceOrder.SequenceEqual(priorityOneAfterOrderByOnly)}");

        /*
         * =========================================================================
         * SECTION 9: CUSTOM IComparer — OrderBy / ThenBy OVERLOADS
         * =========================================================================
         *
         * Overloads that accept a comparer:
         *
         *   OrderBy(keySelector, comparer)
         *   OrderByDescending(keySelector, comparer)
         *   ThenBy(keySelector, comparer)
         *   ThenByDescending(keySelector, comparer)
         *
         * Built-in helpers cover many cases without a custom class:
         *
         *   StringComparer.OrdinalIgnoreCase
         *   StringComparer.CurrentCultureIgnoreCase
         *   Comparer<int>.Default
         *   Comparer<T>.Create((x, y) => …)   // inline IComparer without a type
         *
         * Use a custom IComparer when the key has domain rules (zone "A-3"
         * before "A-12", special null handling, multi-part keys).
         *
         * Query syntax has no comparer clause — use method syntax when you
         * need a custom IComparer.
         *
         * --- 9a. Built-in StringComparer (case-insensitive customer) ---
         * -------------------------------------------------------------------------
         */

        IOrderedEnumerable<FulfillmentLine> byCustomerIgnoreCase =
            openLines.OrderBy(
                line => line.Customer,
                StringComparer.OrdinalIgnoreCase); // "Acme Corp" / "acme corp" group together

        PrintLines("--- OrderBy Customer (OrdinalIgnoreCase) ---", byCustomerIgnoreCase);

        /*
         * --- 9b. Comparer.Create — inline comparer without a named type ---
         *
         * Handy for one-off rules. For reusable domain rules, prefer a named
         * class (ZoneComparer) so tests and call sites stay clear.
         * -------------------------------------------------------------------------
         */

        IComparer<string> customerLengthThenAlpha = Comparer<string>.Create(
            (a, b) =>
            {
                int lengthCmp = a.Length.CompareTo(b.Length); // shorter names first
                return lengthCmp != 0
                    ? lengthCmp
                    : string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
            });

        IOrderedEnumerable<FulfillmentLine> byCustomerLength =
            openLines.OrderBy(line => line.Customer, customerLengthThenAlpha);

        PrintLines("--- OrderBy Customer (Comparer.Create: length then alpha) ---", byCustomerLength);

        /*
         * --- 9c. Custom ZoneComparer — aisle letter then numeric bin ---
         *
         * Default string sort would put "A-12" before "A-3" (character '1' < '3').
         * ZoneComparer parses the bin as an int so A-3 comes before A-12.
         * -------------------------------------------------------------------------
         */

        ZoneComparer zoneComparer = new ZoneComparer();

        IOrderedEnumerable<FulfillmentLine> byZoneDefault =
            openLines.OrderBy(line => line.Zone); // default string compare

        IOrderedEnumerable<FulfillmentLine> byZoneCustom =
            openLines.OrderBy(line => line.Zone, zoneComparer); // aisle then numeric bin

        PrintLines("--- OrderBy Zone (default string) ---", byZoneDefault);
        PrintLines("--- OrderBy Zone (ZoneComparer) ---", byZoneCustom);

        /*
         * --- 9d. ThenBy with a comparer after a primary OrderBy ---
         * -------------------------------------------------------------------------
         */

        IOrderedEnumerable<FulfillmentLine> priorityThenZone =
            openLines
                .OrderByDescending(line => line.Priority)
                .ThenBy(line => line.Zone, zoneComparer); // custom secondary key

        PrintLines("--- Priority desc, ThenBy Zone (custom comparer) ---", priorityThenZone);

        /*
         * =========================================================================
         * SECTION 10: Reverse — FLIP CURRENT ORDER (NOT A SORT)
         * =========================================================================
         *
         * Reverse() is easy to confuse with OrderByDescending — it is different:
         *
         *  OrderByDescending(key)     Sorts by key, high → low
         *  Reverse()                  Keeps sequence as-is but enumerates backward
         *
         * Reverse operates on IEnumerable<T>. It does NOT inspect a sort key.
         * If the source was never sorted, Reverse merely flips whatever order the
         * underlying sequence already had (e.g. array declaration order).
         *
         * Typical pattern: sort first, then reverse the sorted view:
         *
         *   var oldestFirst = lines.OrderBy(l => l.PlacedAt);
         *   var newestFirst = oldestFirst.Reverse();
         *
         * Reverse returns IEnumerable<T> (not IOrderedEnumerable) — you cannot
         * ThenBy after Reverse without ordering again.
         *
         * Compare to List<T>.Reverse() in 03. Generics & Collections — that mutates
         * the list in place. LINQ Reverse returns a new deferred IEnumerable view.
         * -------------------------------------------------------------------------
         */

        IOrderedEnumerable<FulfillmentLine> oldestFirst =
            openLines.OrderBy(line => line.PlacedAt);

        IEnumerable<FulfillmentLine> newestFirst = oldestFirst.Reverse(); // flip sorted view

        PrintLines("--- OrderBy PlacedAt (oldest first) ---", oldestFirst);
        PrintLines("--- .Reverse() on that sort (newest first) ---", newestFirst);

        IEnumerable<FulfillmentLine> declarationOrderReversed = openLines.Reverse();
        PrintLines("--- Reverse on unsorted array (declaration order flipped) ---", declarationOrderReversed);

        /*
         * =========================================================================
         * SECTION 11: PARALLEL LINQ (PLINQ) — PREVIEW
         * =========================================================================
         *
         * Large in-memory sequences can opt into parallel execution:
         *
         *   openLines.AsParallel().OrderBy(line => line.LineTotal)
         *
         * AsParallel() returns ParallelQuery<T> with parallel-aware OrderBy.
         * Partitioning, merge order, and AsOrdered() are non-trivial — full
         * treatment belongs in the parallel chapter.
         *
         * COVERED IN DETAIL LATER → 06. Multithreading & Async Programming /
         *   05. Parallel Programming
         *   (AsParallel, AsOrdered, degree of parallelism, when parallel sort
         *    helps vs overhead on small data)
         * -------------------------------------------------------------------------
         */

        decimal plinqHighestTotal = openLines
            .AsParallel()
            .OrderByDescending(line => line.LineTotal)
            .First()
            .LineTotal;

        Console.WriteLine();
        Console.WriteLine("--- PLINQ preview ---");
        Console.WriteLine(
            $"AsParallel().OrderByDescending(...).First() → highest LineTotal = {plinqHighestTotal:C}");
        Console.WriteLine(
            "Full PLINQ ordering, merge behavior, and AsOrdered() → 06. Multithreading / 05. Parallel Programming.");

        /*
         * =========================================================================
         * SECTION 12: FULFILLMENT PICK LIST — COMPOSED PIPELINE
         * =========================================================================
         *
         * Combine ordering with a light filter + projection into one pipeline.
         * Where depth → 02. Filtering and Aggregation; Select depth → 08.
         * Projection Operations. Here the focus is the OrderBy / ThenBy chain.
         * -------------------------------------------------------------------------
         */

        List<string> pickList = openLines
            .Where(line => line.Priority >= 2)                        // filter first (ch.02)
            .OrderByDescending(line => line.Priority)                 // primary sort
            .ThenBy(line => line.Zone, zoneComparer)                  // zone walk order
            .ThenBy(line => line.PlacedAt)                            // oldest within zone
            .Select(line => $"{line.OrderId}  {line.Zone,-4}  {line.Customer,-12}  {line.LineTotal,8:C}")
            .ToList();                                                // materialize

        Console.WriteLine();
        Console.WriteLine("=== Pick list (Priority >= 2, sorted) ===");
        foreach (string row in pickList)
        {
            Console.WriteLine($"  {row}");
        }
    }

    /*
     * =========================================================================
     * SECTION 13: HELPER — PRINT FULFILLMENT LINES
     * =========================================================================
     *
     * Shared console formatter so each section can focus on the ordering call.
     * Enumeration here is what triggers deferred OrderBy / ThenBy / Reverse.
     * -------------------------------------------------------------------------
     */
    private static void PrintLines(string heading, IEnumerable<FulfillmentLine> lines)
    {
        Console.WriteLine();
        Console.WriteLine(heading);
        foreach (FulfillmentLine line in lines)
        {
            Console.WriteLine(
                $"  {line.OrderId,-8} {line.Customer,-12} {line.Zone,-4} {line.LineTotal,9:C}  " +
                $"pri={line.Priority}  {line.PlacedAt:yyyy-MM-dd HH:mm}");
        }
    }
}

/*
 * =========================================================================
 * QUICK REFERENCE — LINQ ORDERING
 * =========================================================================
 *
 * --- Method syntax ---
 *
 *   seq.OrderBy(x => x.Key)
 *   seq.OrderBy(x => x.Key, comparer)
 *   seq.OrderByDescending(x => x.Key)
 *   seq.OrderByDescending(x => x.Key, comparer)
 *   seq.OrderBy(x => x.A).ThenBy(x => x.B)
 *   seq.OrderBy(x => x.A).ThenBy(x => x.B, comparer)
 *   seq.OrderBy(x => x.A).ThenByDescending(x => x.B)
 *   sortedSeq.Reverse()                         // flip enumeration order
 *
 * --- Query syntax ---
 *
 *   from x in seq orderby x.Key select x
 *   from x in seq orderby x.Key descending select x
 *   from x in seq orderby x.A descending, x.B select x
 *
 *   (no comparer clause in query syntax — use method syntax for IComparer)
 *
 * --- Return types ---
 *
 *   OrderBy / OrderByDescending / ThenBy*  →  IOrderedEnumerable<T>
 *   Reverse                                  →  IEnumerable<T>
 *
 * --- Stability ---
 *
 *   LINQ to Objects OrderBy / ThenBy are stable (equal keys keep source order)
 *   Do not assume stability for SQL / EF Core translations
 *
 * --- IComparer tips ---
 *
 *   StringComparer.OrdinalIgnoreCase     case-insensitive text keys
 *   Comparer<T>.Create((x, y) => …)      inline comparer without a type
 *   Custom IComparer<TKey>               domain rules (zones, codes, …)
 *   Compare returns <0 / 0 / >0          before / equal / after
 *
 * --- Common mistakes ---
 *
 *  Mistake                                    | Result / fix
 *  -------------------------------------------|----------------------------------
 *  ThenBy before OrderBy                      | CS1061 — call OrderBy first
 *  Second OrderBy instead of ThenBy           | Replaces sort; use ThenBy
 *  Reverse expecting numeric sort             | Use OrderByDescending instead
 *  Confusing LINQ Reverse with List.Reverse   | LINQ = new view; List = in-place
 *  Comparer in query syntax                   | Not supported — use method syntax
 *  Forgetting deferred execution              | Sort runs at enumeration / ToList
 *  Relying on stability against a database    | Explicit ThenBy / ORDER BY columns
 *
 * --- Operator vs imperative ---
 *
 *   LINQ OrderBy*     returns new ordered view; original sequence unchanged
 *   List<T>.Sort()    mutates list in place (03. Generics & Collections)
 *
 * --- Related chapters ---
 *
 *   01. Introduction to LINQ             deferred vs immediate execution
 *   02. Filtering and Aggregation        Where before OrderBy in pipelines
 *   08. Projection Operations            Select after ordering
 *   06. Multithreading / 05. Parallel    PLINQ AsParallel, AsOrdered
 *
 * =========================================================================
 */
