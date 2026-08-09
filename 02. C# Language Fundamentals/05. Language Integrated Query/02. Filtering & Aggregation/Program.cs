/*
 * =============================================================================
 * 02. FILTERING AND AGGREGATION — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: LINQ to Objects operators that narrow a sequence (Where, OfType) and
 *        operators that collapse a sequence into one value (Count, Sum,
 *        Average, Min, Max, Aggregate). Method syntax and query syntax for the
 *        same filters; how filtering composes with aggregation in everyday
 *        reports.
 *
 * WHY IT MATTERS:
 *   Business code rarely prints every row. You keep “shipped electronics over
 *   $500,” then ask “how many?”, “total revenue?”, “largest line?”. Filtering
 *   and aggregation are the everyday vocabulary of LINQ to Objects — they
 *   appear in reports, dashboards, validation gates, and API summary layers.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Where — keep elements that pass a predicate (method + query + index)
 *   2.  OfType — keep and cast elements of a given type from mixed sequences
 *   3.  Count / LongCount — element counts (whole sequence or filtered)
 *   4.  Sum — add numeric values (selector-free and with selector)
 *   5.  Average — mean of numeric values; empty-sequence pitfalls
 *   6.  Min and Max — smallest / largest value or projection
 *   7.  Aggregate — custom fold with seedless, seed, and result-selector forms
 *   8.  Method syntax vs query syntax for filter-then-aggregate pipelines
 *   9.  Select / SelectMany preview — projection deferred to ch.08
 *  10.  Any / All / Contains preview — quantifiers deferred to ch.09
 *
 * =============================================================================
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace FilteringAndAggregation;

/*
 * =========================================================================
 * SECTION 1: SAMPLE DOMAIN — ORDER LINES AND MIXED CATALOG ITEMS
 * =========================================================================
 *
 * Two small models power every demo below:
 *
 *  Type            | Role
 *  ----------------|----------------------------------------------------------
 *  OrderLine       | One SKU row on a customer order (filter + aggregate)
 *  CatalogItem     | Base type for OfType demos (Product / Service subclasses)
 *
 * Line total for one OrderLine = Quantity * UnitPrice (helper on Program).
 *
 * Prerequisite: IEnumerable<T> and lambdas from Generics & Collections and
 * Functional Style Programming. Deferred execution basics live in
 * 01. Introduction to LINQ.
 *
 * --- 1a. OrderLine — one SKU row ---
 * -------------------------------------------------------------------------
 */
public readonly record struct OrderLine(
    int OrderId,
    string Sku,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    string Category);

/*
 * --- 1b. CatalogItem hierarchy — OfType walks inheritance ---
 *
 * OfType<Product>() keeps Product (and derived) rows and casts them.
 * Service rows are skipped, not thrown.
 * -------------------------------------------------------------------------
 */
public abstract class CatalogItem
{
    protected CatalogItem(string code, string name)
    {
        Code = code; // SKU / service code
        Name = name; // display label
    }

    public string Code { get; }
    public string Name { get; }
}

public sealed class Product : CatalogItem
{
    public Product(string code, string name, decimal unitPrice)
        : base(code, name)
    {
        UnitPrice = unitPrice; // price per sellable unit
    }

    public decimal UnitPrice { get; }
}

public sealed class Service : CatalogItem
{
    public Service(string code, string name, decimal hourlyRate)
        : base(code, name)
    {
        HourlyRate = hourlyRate; // billed per hour
    }

    public decimal HourlyRate { get; }
}

public class Program
{
    public static void Main(string[] args)
    {
        /*
         * =========================================================================
         * SECTION 2: FILTERING VS AGGREGATION — OPERATOR MAP
         * =========================================================================
         *
         * Filtering returns a (usually deferred) sequence. Aggregation consumes
         * a sequence and returns one value — it is a terminal / immediate step.
         *
         *  Family        | Operators                         | Returns
         *  --------------|-----------------------------------|--------------------
         *  Filtering     | Where, OfType                     | IEnumerable<T>
         *  Aggregation   | Count, Sum, Average, Min, Max,    | scalar (int, decimal, …)
         *                | Aggregate                         |
         *
         * Typical report shape:
         *
         *   source.Where(…).Sum(…)     // filter first, then collapse
         *
         * Scenario: warehouse / office order lines plus a mixed catalog used
         * only for OfType.
         * -------------------------------------------------------------------------
         */

        OrderLine[] orderLines =
        [
            new OrderLine(1001, "WH-4412", "Industrial Shelving Unit", 4, 49.99m, "Warehouse"),
            new OrderLine(1001, "WH-3305", "Barcode Scanner Kit", 2, 245.00m, "Electronics"),
            new OrderLine(1002, "OF-2100", "Ergonomic Office Chair", 6, 189.50m, "Office"),
            new OrderLine(1002, "OF-1188", "Standing Desk Mat", 12, 34.25m, "Office"),
            new OrderLine(1003, "EL-9001", "Network Switch 24-Port", 1, 620.00m, "Electronics"),
            new OrderLine(1003, "EL-7720", "UPS Battery Backup", 3, 175.00m, "Electronics"),
            new OrderLine(1004, "WH-8890", "Heavy-Duty Pallet Jack", 1, 899.00m, "Warehouse"),
            new OrderLine(1004, "WH-2201", "Safety Vest (Bulk Pack)", 50, 12.50m, "Warehouse"),
        ];

        Console.WriteLine("=== Order line inventory (all rows) ===");
        PrintOrderLines(orderLines);


        /*
         * =========================================================================
         * SECTION 3: Where — FILTER WITH METHOD SYNTAX
         * =========================================================================
         *
         * Where keeps elements for which the predicate returns true.
         *
         *   IEnumerable<T> filtered = source.Where(item => condition);
         *
         * Signature (conceptually):
         *
         *   Where(this IEnumerable<T> source, Func<T, bool> predicate)
         *
         * Returns IEnumerable<T> — execution is deferred until you iterate or
         * call a terminal operator (Count, Sum, ToList, …).
         *
         * Chain multiple Where calls or combine conditions with && / || inside
         * one predicate — both are common; prefer clarity over micro-style wars.
         *
         * --- 3a. Single predicate ---
         * -------------------------------------------------------------------------
         */

        IEnumerable<OrderLine> electronicsMethod = orderLines.Where(
            line => line.Category == "Electronics"); // keep Electronics only

        Console.WriteLine();
        Console.WriteLine("--- Where (method): Category == \"Electronics\" ---");
        PrintOrderLines(electronicsMethod);


        /*
         * --- 3b. Compound predicate ---
         *
         * One lambda can combine several checks. Here: Electronics AND line
         * total strictly greater than $500.
         * -------------------------------------------------------------------------
         */

        IEnumerable<OrderLine> highValueElectronics = orderLines.Where(
            line => line.Category == "Electronics" && LineTotal(line) > 500m);

        Console.WriteLine();
        Console.WriteLine("--- Where (method): Electronics AND line total > $500 ---");
        PrintOrderLines(highValueElectronics);


        /*
         * --- 3c. Chained Where vs && ---
         *
         * Two Where calls compose as successive filters. Equivalent intent to
         * one compound predicate; each Where stays a deferred step.
         * -------------------------------------------------------------------------
         */

        IEnumerable<OrderLine> warehouseBulk = orderLines
            .Where(line => line.Category == "Warehouse")
            .Where(line => line.Quantity >= 10); // second filter on the filtered stream

        Console.WriteLine();
        Console.WriteLine("--- Where chained: Warehouse then qty >= 10 ---");
        PrintOrderLines(warehouseBulk);


        /*
         * =========================================================================
         * SECTION 4: Where — FILTER WITH QUERY SYNTAX
         * =========================================================================
         *
         * Query syntax rewrites to method calls. This filter matches Section 3a:
         *
         *   from line in orderLines
         *   where line.Category == "Electronics"
         *   select line          ← select is required in query form
         *                          (projection depth → ch.08; here we pass through)
         *
         * Multiple where clauses stack like chained Where calls.
         * -------------------------------------------------------------------------
         */

        IEnumerable<OrderLine> electronicsQuery =
            from line in orderLines
            where line.Category == "Electronics"
            select line; // pass-through select — required by query syntax

        IEnumerable<OrderLine> highValueLines = orderLines.Where(
            line => LineTotal(line) > 500m); // reused by Aggregate / Select preview

        Console.WriteLine();
        Console.WriteLine("--- Where (query): same Electronics filter ---");
        PrintOrderLines(electronicsQuery);


        /*
         * =========================================================================
         * SECTION 5: Where — INDEXED OVERLOAD
         * =========================================================================
         *
         * Where also has a two-parameter predicate:
         *
         *   Where((item, index) => condition)
         *
         * index is 0-based position in the source sequence (before this Where
         * filters anything out). Useful for “every other row,” “skip first,”
         * or demos that depend on position — not for business keys (use fields).
         *
         * Example: keep only odd-positioned lines in the original array
         * (indexes 1, 3, 5, …).
         * -------------------------------------------------------------------------
         */

        IEnumerable<OrderLine> oddIndexedLines = orderLines.Where(
            (line, index) => index % 2 == 1); // index from source, not filtered set

        Console.WriteLine();
        Console.WriteLine("--- Where (index): odd indexes in source array ---");
        PrintOrderLines(oddIndexedLines);


        /*
         * =========================================================================
         * SECTION 6: OfType — FILTER BY TYPE
         * =========================================================================
         *
         * OfType<TResult>() walks a sequence and yields only elements that are
         * assignable to TResult, casting each match:
         *
         *   mixed.OfType<Product>()
         *
         * Use OfType when the source is mixed (object[], non-generic IEnumerable,
         * or a base-type sequence that may hold subclasses).
         *
         *  Operator   | Keeps                          | Casts?
         *  -----------|--------------------------------|---------------------------
         *  Where      | elements matching a predicate  | No — same T throughout
         *  OfType<T>  | elements of type T (or derived)| Yes — yields T
         *
         * OfType skips incompatible items (no exception). Cast<T>() (conversion
         * chapter) throws on a bad element — different contract.
         *
         * --- 6a. Inheritance hierarchy on CatalogItem ---
         * -------------------------------------------------------------------------
         */

        CatalogItem[] catalog =
        [
            new Product("PRD-100", "Label Printer", 129.00m),
            new Service("SRV-10", "On-site Setup", 95.00m),
            new Product("PRD-200", "Thermal Ribbon Pack", 24.50m),
            new Service("SRV-20", "Annual Calibration", 140.00m),
            new Product("PRD-300", "Handheld Scanner", 310.00m),
        ];

        IEnumerable<Product> productsOnly = catalog.OfType<Product>(); // drops Service rows
        IEnumerable<Service> servicesOnly = catalog.OfType<Service>(); // drops Product rows

        Console.WriteLine();
        Console.WriteLine("--- OfType<Product> from CatalogItem[] ---");
        foreach (Product product in productsOnly)
        {
            Console.WriteLine($"  {product.Code}  {product.Name,-22}  {product.UnitPrice:C}");
        }

        Console.WriteLine("--- OfType<Service> from CatalogItem[] ---");
        foreach (Service service in servicesOnly)
        {
            Console.WriteLine($"  {service.Code}  {service.Name,-22}  {service.HourlyRate:C}/hr");
        }


        /*
         * --- 6b. Mixed object[] — classic OfType scenario ---
         *
         * Arrays typed as object[] (or List<object>) often appear at API
         * boundaries. OfType pulls out ints, strings, or domain types safely.
         * -------------------------------------------------------------------------
         */

        object[] mixedBag =
        [
            42,
            "warehouse-aisle-B",
            new Product("PRD-7", "Zip Ties (100)", 8.99m),
            3.14,
            new Service("SRV-7", "Rush Pick Fee", 25.00m),
            "dock-3",
        ];

        IEnumerable<string> labels = mixedBag.OfType<string>();          // only strings
        IEnumerable<int> boxedInts = mixedBag.OfType<int>();             // only boxed ints
        IEnumerable<Product> boxedProducts = mixedBag.OfType<Product>(); // only Product

        Console.WriteLine();
        Console.WriteLine("--- OfType on object[] ---");
        Console.WriteLine($"Strings:  {string.Join(", ", labels)}");
        Console.WriteLine($"Ints:     {string.Join(", ", boxedInts)}");
        Console.WriteLine($"Products: {string.Join(", ", boxedProducts.Select(p => p.Code))}");


        /*
         * =========================================================================
         * SECTION 7: Count AND LongCount
         * =========================================================================
         *
         * Count() — number of elements (iterates the full sequence unless the
         * source implements ICollection<T> / ICollection, which can answer in O(1)).
         *
         * Count(predicate) — number of matches without building a separate
         * filtered IEnumerable first (prefer over Where(...).Count() when you
         * only need the number).
         *
         * LongCount / LongCount(predicate) — same idea, returns long (rare for
         * in-memory lists; useful when counts may exceed Int32.MaxValue).
         *
         *  Overload                         | Meaning
         *  ---------------------------------|--------------------------------------
         *  Count()                          | All elements
         *  Count(Func<T, bool> pred)        | Elements where pred is true
         *  LongCount() / LongCount(pred)    | Same as Count, result is long
         *
         * Empty sequence: Count() returns 0 (does not throw).
         * -------------------------------------------------------------------------
         */

        int totalLineCount = orderLines.Count();
        int electronicsCount = orderLines.Count(line => line.Category == "Electronics");
        int bulkLineCount = orderLines.Count(line => line.Quantity >= 10);
        int highValueCount = highValueLines.Count(); // Count after Where — still fine
        int whereThenCount = orderLines
            .Where(line => line.Category == "Electronics")
            .Count(); // equivalent to Count(pred); prefer Count(pred) for scalars
        long totalAsLong = orderLines.LongCount(); // same scan, long result type
        int emptyCount = Array.Empty<OrderLine>().Count(); // 0 — never throws

        Console.WriteLine();
        Console.WriteLine("--- Count / LongCount ---");
        Console.WriteLine($"Total order lines:           {totalLineCount}");
        Console.WriteLine($"Electronics lines:           {electronicsCount}");
        Console.WriteLine($"Bulk lines (qty >= 10):      {bulkLineCount}");
        Console.WriteLine($"High-value lines (> $500):   {highValueCount}");
        Console.WriteLine($"Where.then.Count (same):     {whereThenCount}");
        Console.WriteLine($"LongCount (same total):      {totalAsLong}");
        Console.WriteLine($"Empty array Count():         {emptyCount}");


        /*
         * =========================================================================
         * SECTION 8: Sum
         * =========================================================================
         *
         * Sum() — adds numeric elements (int, long, float, double, decimal, and
         * nullable variants that ignore nulls).
         *
         * Sum(selector) — adds the value returned by the selector for each item:
         *
         *   decimal revenue = lines.Sum(line => LineTotal(line));
         *
         * Common mistake: calling Sum() on a sequence of objects without a
         * selector — CS0411 (cannot infer type). Provide Sum(l => numericField).
         *
         * Empty sequence: Sum returns 0 for numeric types (not an exception).
         *
         * Compose with Where when you need a filtered total:
         *
         *   lines.Where(…).Sum(…)
         *
         * --- 8a. Selector on OrderLine ---
         * -------------------------------------------------------------------------
         */

        int totalUnitsOrdered = orderLines.Sum(line => line.Quantity); // selector required
        decimal totalRevenue = orderLines.Sum(line => LineTotal(line));
        decimal electronicsRevenue = orderLines
            .Where(line => line.Category == "Electronics")
            .Sum(line => LineTotal(line)); // filter then aggregate

        Console.WriteLine();
        Console.WriteLine("--- Sum ---");
        Console.WriteLine($"Total units ordered:         {totalUnitsOrdered}");
        Console.WriteLine($"Total revenue (all lines):   {totalRevenue:C}");
        Console.WriteLine($"Electronics revenue:         {electronicsRevenue:C}");


        /*
         * --- 8b. Selector-free Sum and nullable numbers ---
         *
         * On IEnumerable<int> / decimal / …, Sum() needs no selector.
         * Nullable sequences skip nulls; an all-null or empty nullable Sum
         * returns 0 (or null for some nullable float/double overloads — check docs).
         * -------------------------------------------------------------------------
         */

        int[] unitBatches = [4, 2, 6, 12, 1, 3, 1, 50];
        int batchSum = unitBatches.Sum(); // no selector — sequence is already int

        decimal?[] maybeFees = [12.50m, null, 8.00m, null, 5.25m];
        decimal? nullableFeeSum = maybeFees.Sum(); // nulls ignored → 25.75m

        Console.WriteLine($"Unit batches Sum():          {batchSum}");
        Console.WriteLine($"Nullable fee Sum():          {nullableFeeSum:C}");
        Console.WriteLine($"Empty int[] Sum():           {Array.Empty<int>().Sum()}");


        /*
         * =========================================================================
         * SECTION 9: Average
         * =========================================================================
         *
         * Average() / Average(selector) — arithmetic mean.
         *
         * Uses the same numeric families as Sum. For decimal sequences the
         * result is decimal; for an int selector the result widens to double.
         *
         * Empty sequence: InvalidOperationException (“Sequence contains no
         * elements”). Guard with Any (preview Section 15) or Count() > 0 first.
         *
         * --- 9a. Means on order lines ---
         * -------------------------------------------------------------------------
         */

        double averageQuantity = orderLines.Average(line => line.Quantity); // int → double
        decimal averageUnitPrice = orderLines.Average(line => line.UnitPrice);
        decimal averageLineTotal = orderLines.Average(line => LineTotal(line));

        Console.WriteLine();
        Console.WriteLine("--- Average ---");
        Console.WriteLine($"Average quantity per line:   {averageQuantity:F2}");
        Console.WriteLine($"Average unit price:          {averageUnitPrice:C}");
        Console.WriteLine($"Average line total:          {averageLineTotal:C}");


        /*
         * --- 9b. Empty Average — throws unless you guard ---
         *
         * Prefer: if (seq.Any()) avg = seq.Average(...); else use a default.
         * Count() > 0 also works but may walk the whole sequence when Any short-circuits.
         * -------------------------------------------------------------------------
         */

        OrderLine[] noMatchingLines = orderLines
            .Where(line => line.Category == "DoesNotExist")
            .ToArray(); // materialize empty for the guard demo

        decimal safeOfficeAverage = noMatchingLines.Length == 0
            ? 0m
            : noMatchingLines.Average(line => LineTotal(line)); // would throw if unguarded

        Console.WriteLine($"Empty filter Average (safe): {safeOfficeAverage:C}  (guarded → 0)");


        /*
         * =========================================================================
         * SECTION 10: Min AND Max
         * =========================================================================
         *
         * Min() / Max() — smallest or largest element (type T must implement
         * IComparable<T>, or Comparer.Default must apply).
         *
         * Min(selector) / Max(selector) — compare projected values instead of
         * the whole object:
         *
         *   decimal cheapestUnit = orderLines.Min(l => l.UnitPrice);
         *   decimal largestLine  = orderLines.Max(l => LineTotal(l));
         *
         * To recover the *row* with the max line total, use MaxBy / MinBy in
         * 06. Element Operations — here we stay on scalar Min/Max.
         *
         * Empty sequence: InvalidOperationException (same as Average).
         * -------------------------------------------------------------------------
         */

        int minQuantity = orderLines.Min(line => line.Quantity);
        int maxQuantity = orderLines.Max(line => line.Quantity);
        decimal minUnitPrice = orderLines.Min(line => line.UnitPrice);
        decimal maxLineTotal = orderLines.Max(line => LineTotal(line));
        int maxBatch = unitBatches.Max(); // selector-free on IEnumerable<int>

        Console.WriteLine();
        Console.WriteLine("--- Min / Max ---");
        Console.WriteLine($"Smallest quantity on a line: {minQuantity}");
        Console.WriteLine($"Largest quantity on a line:  {maxQuantity}");
        Console.WriteLine($"Lowest unit price:           {minUnitPrice:C}");
        Console.WriteLine($"Highest line total:          {maxLineTotal:C}");
        Console.WriteLine($"Max unit batch (no selector):{maxBatch}");


        /*
         * =========================================================================
         * SECTION 11: Aggregate — CUSTOM FOLDS
         * =========================================================================
         *
         * Aggregate folds a sequence into one value using a combining function.
         * Prefer Sum / Count / Min / Max when they already express the math —
         * use Aggregate when the running state is custom (string build, pair of
         * values, domain accumulator).
         *
         * --- 11a. Seedless overload ---
         *
         *   source.Aggregate((accumulator, item) => combine(accumulator, item))
         *
         * Uses the first element as the initial accumulator, then combines with
         * each subsequent element. Empty sequence → InvalidOperationException.
         *
         * --- 11b. Seed overload ---
         *
         *   source.Aggregate(seed, (acc, item) => combine(acc, item))
         *
         * Starts from seed — preferred for “running total” patterns; empty
         * sequence returns the seed.
         *
         * --- 11c. Seed + result selector ---
         *
         *   source.Aggregate(seed, func, resultSelector)
         *
         * Folds with seed, then maps the final accumulator to another type
         * (e.g. decimal → formatted string) without a second pass.
         * -------------------------------------------------------------------------
         */

        // 11a — seedless: fold line totals; first line total is the starting acc
        decimal maxViaFold = orderLines
            .Select(line => LineTotal(line)) // Select preview — needed for a decimal stream
            .Aggregate((runningMax, total) => total > runningMax ? total : runningMax);

        // 11b — seed: build comma-separated SKU list for high-value lines
        string skuRollup = highValueLines.Aggregate(
            "",
            (accumulated, line) =>
                accumulated.Length == 0 ? line.Sku : accumulated + ", " + line.Sku);

        // 11b — seed: distinct category labels in encounter order
        string categoryRollup = orderLines.Aggregate(
            "",
            (accumulated, line) =>
            {
                if (accumulated.Contains(line.Category, StringComparison.Ordinal))
                {
                    return accumulated; // already recorded this category
                }

                return accumulated.Length == 0
                    ? line.Category
                    : accumulated + " | " + line.Category;
            });

        // 11b — seed revenue check (should match Sum)
        decimal revenueAccumulator = orderLines.Aggregate(
            0m,
            (runningTotal, line) => runningTotal + LineTotal(line));

        // 11c — seed + result selector: fold units, format once at the end
        string unitsSummary = orderLines.Aggregate(
            0,
            (units, line) => units + line.Quantity,
            units => $"{units} units across {orderLines.Length} lines");

        // seeded Aggregate on empty → seed (no throw)
        decimal emptySeeded = Array.Empty<OrderLine>().Aggregate(
            0m,
            (running, line) => running + LineTotal(line));

        Console.WriteLine();
        Console.WriteLine("--- Aggregate ---");
        Console.WriteLine($"Max line total (seedless):   {maxViaFold:C}");
        Console.WriteLine($"High-value SKUs (fold):      {skuRollup}");
        Console.WriteLine($"Distinct categories (fold):  {categoryRollup}");
        Console.WriteLine($"Revenue via Aggregate seed:  {revenueAccumulator:C}");
        Console.WriteLine($"Matches Sum() revenue:       {revenueAccumulator == totalRevenue}");
        Console.WriteLine($"Units (seed + selector):     {unitsSummary}");
        Console.WriteLine($"Empty seeded Aggregate:      {emptySeeded:C}  (returns seed)");


        /*
         * =========================================================================
         * SECTION 12: METHOD SYNTAX VS QUERY SYNTAX
         * =========================================================================
         *
         * Most LINQ operators are extension methods (method syntax). Query
         * syntax is sugar for a subset: from, where, select, orderby, group, join.
         *
         *  Capability              | Method syntax     | Query syntax
         *  ------------------------|-------------------|---------------------------
         *  Where                   | .Where(pred)      | where condition
         *  OfType                  | .OfType<T>()      | (no keyword — method only)
         *  Count / Sum / Average   | .Count() etc.     | wrap query in ( … ).Sum()
         *  Aggregate               | .Aggregate(…)     | method only
         *
         * Rule of thumb:
         *   • Simple filters + aggregates → either form; team consistency wins.
         *   • Operators with no query keyword (OfType, Aggregate, Max) → method.
         *   • Parentheses required when a query expression is followed by .Sum().
         * -------------------------------------------------------------------------
         */

        decimal officeRevenueMethod = orderLines
            .Where(line => line.Category == "Office")
            .Sum(line => LineTotal(line));

        decimal officeRevenueQuery = (
            from line in orderLines
            where line.Category == "Office"
            select line
        ).Sum(line => LineTotal(line)); // query produces IEnumerable, then Sum

        int warehouseLineCountMethod = orderLines.Count(
            line => line.Category == "Warehouse");

        int warehouseLineCountQuery = (
            from line in orderLines
            where line.Category == "Warehouse"
            select line
        ).Count();

        Console.WriteLine();
        Console.WriteLine("--- Method syntax vs query syntax ---");
        Console.WriteLine($"Office revenue (method):     {officeRevenueMethod:C}");
        Console.WriteLine($"Office revenue (query):      {officeRevenueQuery:C}");
        Console.WriteLine($"Warehouse lines (method):    {warehouseLineCountMethod}");
        Console.WriteLine($"Warehouse lines (query):     {warehouseLineCountQuery}");
        Console.WriteLine(
            "Both pairs equal:            " +
            $"{officeRevenueMethod == officeRevenueQuery && warehouseLineCountMethod == warehouseLineCountQuery}");


        /*
         * =========================================================================
         * SECTION 13: COMBINING FILTER + AGGREGATE — CATEGORY SUMMARY
         * =========================================================================
         *
         * Typical report fragment: for each dimension value, Where → Count / Sum /
         * Max. Uses method syntax (most production pipelines do). Guard Max when
         * the filtered set might be empty.
         * -------------------------------------------------------------------------
         */

        PrintCategorySummary(orderLines);


        /*
         * =========================================================================
         * SECTION 14: Select / SelectMany — PREVIEW (PROJECTION)
         * =========================================================================
         *
         * Select transforms each element to a new shape (same count, different
         * type or fields). SelectMany flattens nested sequences into one.
         *
         * Filtering removes rows; projection reshapes them. Query expressions
         * always end with select — even when you pass the original item through.
         *
         * COVERED IN DETAIL LATER → 08. Projection Operations
         *   (Select, SelectMany, anonymous types, named projections, index Select)
         *
         * Below: one minimal Select used only to print SKU labels — not a full
         * lesson on shaping data. SelectMany is shown as a one-liner shape hint.
         * -------------------------------------------------------------------------
         */

        IEnumerable<string> highValueSkus = highValueLines.Select(line => line.Sku);

        // Shape hint only: flatten nested int arrays — full depth in ch.08
        IEnumerable<int> flatDemo = new[] { new[] { 1, 2 }, new[] { 3 } }
            .SelectMany(group => group);

        Console.WriteLine();
        Console.WriteLine("--- Select / SelectMany preview (see ch.08) ---");
        Console.Write("High-value SKUs projected:   ");
        foreach (string sku in highValueSkus)
        {
            Console.Write(sku + " ");
        }

        Console.WriteLine();
        Console.WriteLine($"SelectMany flatten demo:     {string.Join(", ", flatDemo)}");


        /*
         * =========================================================================
         * SECTION 15: Any / All / Contains — PREVIEW (QUANTIFIERS)
         * =========================================================================
         *
         * Any(predicate) — true if at least one element matches (or Any() if the
         * sequence is non-empty). All(predicate) — true only if every element
         * matches; empty sequence returns true for All (vacuous truth).
         * Contains(value) — membership via default equality.
         *
         * Prefer Any(...) over Count(...) > 0 when you only need a yes/no —
         * Any can short-circuit on the first match.
         *
         * COVERED IN DETAIL LATER → 09. Quantifier Operations
         *   (Any, All, Contains, IEqualityComparer, short-circuit recipes)
         * -------------------------------------------------------------------------
         */

        bool anyHighValueLine = orderLines.Any(line => LineTotal(line) > 500m);
        bool allPositiveQuantity = orderLines.All(line => line.Quantity > 0);
        bool allElectronics = orderLines.All(line => line.Category == "Electronics");
        bool containsSku = orderLines.Select(line => line.Sku).Contains("EL-9001");

        Console.WriteLine();
        Console.WriteLine("--- Any / All / Contains preview (see ch.09) ---");
        Console.WriteLine($"Any line total > $500:       {anyHighValueLine}");
        Console.WriteLine($"All lines qty > 0:           {allPositiveQuantity}");
        Console.WriteLine($"All lines are Electronics:   {allElectronics}");
        Console.WriteLine($"Contains SKU EL-9001:        {containsSku}");
    }

    /*
     * =========================================================================
     * SECTION 16: HELPERS — LINE TOTAL AND PRINTERS
     * =========================================================================
     *
     * Small helpers keep Main focused on operator demos. LineTotal is the
     * shared projection used by Sum, Average, Min/Max, Aggregate, and Where.
     * -------------------------------------------------------------------------
     */

    private static decimal LineTotal(OrderLine line) =>
        line.Quantity * line.UnitPrice; // quantity × unit price

    private static void PrintOrderLines(IEnumerable<OrderLine> lines)
    {
        foreach (OrderLine line in lines)
        {
            Console.WriteLine(
                $"  #{line.OrderId}  {line.Sku,-10}  {line.ProductName,-28}  " +
                $"qty {line.Quantity,3}  @ {line.UnitPrice,8:C}  " +
                $"line {LineTotal(line),10:C}  [{line.Category}]");
        }
    }

    private static void PrintCategorySummary(IEnumerable<OrderLine> lines)
    {
        Console.WriteLine();
        Console.WriteLine("=== Category summary (filter + aggregate) ===");

        string[] categories = ["Electronics", "Office", "Warehouse"];

        foreach (string category in categories)
        {
            IEnumerable<OrderLine> categoryLines = lines.Where(
                line => line.Category == category);

            int lineCount = categoryLines.Count();
            decimal categoryRevenue = categoryLines.Sum(line => LineTotal(line));
            // guard Max — empty filter would throw InvalidOperationException
            decimal topLine = lineCount == 0
                ? 0m
                : categoryLines.Max(line => LineTotal(line));

            Console.WriteLine(
                $"{category,-12}  lines: {lineCount,2}  revenue: {categoryRevenue,10:C}  max line: {topLine,10:C}");
        }
    }
}

/*
 * =========================================================================
 * QUICK REFERENCE — FILTERING AND AGGREGATION
 * =========================================================================
 *
 * --- Where ---
 *
 *   items.Where(x => condition)
 *   items.Where((x, index) => index % 2 == 0)   // 0-based source index
 *   items.Where(a).Where(b)                     // chained filters
 *   from x in items where condition select x
 *
 * --- OfType ---
 *
 *   mixed.OfType<Product>()     // keep + cast; skips non-matches
 *   // Cast<T>() throws on bad elements — see Conversion chapter
 *
 * --- Count ---
 *
 *   items.Count()
 *   items.Count(x => condition) // prefer over Where(...).Count() for scalars
 *   items.LongCount()           // returns long
 *
 * --- Sum / Average ---
 *
 *   numbers.Sum()               // selector-free on numeric sequences
 *   items.Sum(x => x.Amount)
 *   nullables.Sum()             // nulls ignored
 *   items.Average(x => x.Score) // int selector → double result
 *
 * --- Min / Max ---
 *
 *   items.Min(x => x.Price)
 *   items.Max(x => x.Price)
 *   numbers.Max()               // selector-free
 *   // row with extreme value → MinBy / MaxBy (Element Operations)
 *
 * --- Aggregate ---
 *
 *   items.Aggregate((acc, x) => acc + x)              // first = seed; empty throws
 *   items.Aggregate(seed, (acc, x) => acc + x)        // empty → seed
 *   items.Aggregate(seed, func, resultSelector)
 *
 * --- Method vs query ---
 *
 *   Method:  source.Where(...).Sum(...)
 *   Query:   (from x in source where ... select x).Sum(...)
 *   OfType / Aggregate: method syntax only
 *
 * --- Empty sequence behavior ---
 *
 *  Operator     | Empty result
 *  -------------|------------------------------------------------
 *  Count()      | 0
 *  Sum()        | 0
 *  Average()    | InvalidOperationException
 *  Min / Max()  | InvalidOperationException
 *  Aggregate    | throws (seedless); returns seed (seeded)
 *  OfType       | empty sequence (no throw)
 *
 * --- Previews (full chapters later) ---
 *
 *   Select / SelectMany → 08. Projection Operations
 *   Any / All / Contains → 09. Quantifier Operations
 *
 * --- Related chapters ---
 *
 *   01. Introduction to LINQ  — deferred execution, IEnumerable<T>
 *   03. Ordering              — OrderBy after Where
 *   06. Element Operations    — MinBy / MaxBy, First, Single
 *   08. Projection Operations — Select, SelectMany
 *   09. Quantifier Operations — Any, All, Contains
 *
 * =========================================================================
 */
