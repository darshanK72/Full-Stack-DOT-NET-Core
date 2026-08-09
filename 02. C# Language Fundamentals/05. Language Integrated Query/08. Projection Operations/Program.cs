/*
 * =============================================================================
 * 08. PROJECTION OPERATIONS — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: LINQ projection operators — Select (1→1 reshape) and SelectMany
 *        (1→many flatten), including result selectors, indexed overloads,
 *        anonymous types / value tuples / named DTOs as projection targets,
 *        and query-syntax select / nested from / let. This chapter owns
 *        projection depth for the module (ch.01–02 only preview Select).
 *
 * WHY IT MATTERS:
 *   APIs, reports, and pick lists rarely want the raw domain object graph.
 *   You need SKU labels, flat pick rows with parent OrderId, or ranked slip
 *   lines. Projection expresses those shapes as composable pipelines instead
 *   of nested foreach + temporary List<T> scaffolding. After Where, Select
 *   and SelectMany are the operators you reach for most often.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Projection mental model — Select vs SelectMany vs filtering
 *   2.  Domain sample — nested Order → OrderLine → Tags
 *   3.  Select — property, computed, and type-changing maps
 *   4.  The nested-sequence trap — why Select alone does not flatten
 *   5.  Anonymous types in projections (local shapes)
 *   6.  Value tuples in projections (method-friendly shapes)
 *   7.  Named DTOs / records as projection targets
 *   8.  SelectMany — flatten; resultSelector keeping parent context
 *   9.  Multi-level SelectMany and empty inner sequences
 *  10.  Indexed Select / SelectMany (0-based pipeline position)
 *  11.  Query syntax — select, nested from, and let
 *  12.  Zip — pairwise projection of two sequences (related pattern)
 *  13.  Composing projections with Where / OrderBy / Take (deferred)
 *
 * =============================================================================
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectionOperations;

/*
 * =========================================================================
 * SECTION 1: DOMAIN SAMPLE — ORDER, LINE, AND TAGS
 * =========================================================================
 *
 * Projection demos need a nested shape:
 *
 *   Order  →  many OrderLine  →  each line has Tags (string[])
 *
 *  Operator intent                         | Typical map
 *  ----------------------------------------|----------------------------------
 *  Select                                  | Order → header / label / total
 *  SelectMany (one level)                  | Order → flat OrderLine stream
 *  SelectMany + resultSelector             | (Order, OrderLine) → pick row
 *  SelectMany twice (or nested from)       | Order → Tags across all lines
 *
 * OrderLine is a readonly record struct (value-like line item).
 * Order is a sealed class with a computed OrderTotal from its lines.
 * -------------------------------------------------------------------------
 */

public readonly record struct OrderLine(
    string Sku,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    string[] Tags)
{
    public decimal LineTotal => UnitPrice * Quantity; // computed per line
}

public sealed class Order
{
    public Order(string orderId, string customer, DateOnly placedOn, IReadOnlyList<OrderLine> lines)
    {
        OrderId = orderId;
        Customer = customer;
        PlacedOn = placedOn;
        Lines = lines;
    }

    public string OrderId { get; }
    public string Customer { get; }
    public DateOnly PlacedOn { get; }
    public IReadOnlyList<OrderLine> Lines { get; }

    public decimal OrderTotal => Lines.Sum(line => line.LineTotal); // Sum needs System.Linq
}

/*
 * =========================================================================
 * SECTION 2: NAMED PROJECTION TYPE — OrderLineSummary (DTO)
 * =========================================================================
 *
 * When a projected shape crosses method boundaries, APIs, or tests, declare a
 * named type. Contrast with anonymous types (Section 6) which stay local.
 *
 *  Shape                | Best for                         | Return type OK?
 *  ---------------------|----------------------------------|----------------
 *  Anonymous `new {…}`  | One-off console / local reports  | No (var only)
 *  Value tuple          | Small private helpers            | Yes
 *  Named record/class   | Public APIs, serialization       | Yes
 *
 * Select / SelectMany can construct this directly:
 *   .SelectMany(o => o.Lines, (o, line) => new OrderLineSummary(...))
 * -------------------------------------------------------------------------
 */

public readonly record struct OrderLineSummary(
    string OrderId,
    string Customer,
    string Sku,
    string ProductName,
    int Quantity,
    decimal LineTotal);

public class Program
{
    public static void Main(string[] args)
    {
        /*
         * =========================================================================
         * SECTION 3: PROJECTION OVERVIEW — WHAT Select AND SelectMany DO
         * =========================================================================
         *
         * Filtering (Where) REMOVES elements. Projection KEEPS cardinality at the
         * source level (Select: N → N) or EXPANDS nested data (SelectMany: N → M).
         *
         *  Operator    | Cardinality              | Output shape
         *  ------------|--------------------------|----------------------------------
         *  Where       | N → ≤ N (subset)         | same element type
         *  Select      | N → N (1→1 map)          | IEnumerable<TResult>
         *  SelectMany  | N → M (1→many flatten)   | IEnumerable<TResult>
         *  Zip         | Min(N,K) pairs           | IEnumerable<TResult>
         *
         * Both Select and SelectMany return deferred IEnumerable<T> — the
         * projection lambda runs when you foreach or call a terminal operator
         * (see 01. Introduction to LINQ).
         *
         * Signature sketch (System.Linq.Enumerable):
         *
         *   Select(source, Func<TSource, TResult> selector)
         *   Select(source, Func<TSource, int, TResult> selector)          // index
         *   SelectMany(source, Func<TSource, IEnumerable<TCollection>> …)
         *   SelectMany(source, collectionSelector, resultSelector)
         *
         * Scenario: e-commerce fulfillment queue — orders with nested line items
         * and per-line tags. Project headers, flatten pick rows, number slip
         * lines, flatten tags, pair dock stations with Zip, and use query let.
         * -------------------------------------------------------------------------
         */

        Order[] orders = BuildSampleOrders();

        Console.WriteLine("=== Source orders ===");
        foreach (Order order in orders)
        {
            Console.WriteLine(
                $"  {order.OrderId}  {order.Customer,-16}  {order.Lines.Count} lines  total {order.OrderTotal:C}");
        }

        /*
         * =========================================================================
         * SECTION 4: Select — TRANSFORM EACH ELEMENT (1→1)
         * =========================================================================
         *
         * Select applies Func<TSource, TResult> to every element and yields one
         * result per source element — same count, possibly different type:
         *
         *   orders.Select(o => o.OrderId)           // Order → string
         *   orders.Select(o => o.OrderTotal)        // Order → decimal
         *
         * Common patterns:
         *   • Pick one property     →  .Select(o => o.Customer)
         *   • Build a string label  →  .Select(o => $"{o.OrderId}: {o.Customer}")
         *   • Compute a value       →  .Select(o => o.OrderTotal)
         *   • Method group          →  .Select(FormatOrderLabel) when signature matches
         *
         * Select does NOT flatten. Returning a collection from Select produces
         * IEnumerable<IEnumerable<…>> — see Section 5.
         *
         * --- 4a. Property, computed, and method-group projections ---
         * -------------------------------------------------------------------------
         */

        IEnumerable<string> orderIds = orders.Select(o => o.OrderId); // Order → string
        IEnumerable<string> orderLabels = orders.Select(FormatOrderLabel); // method group
        IEnumerable<decimal> orderTotals = orders.Select(o => o.OrderTotal);

        Console.WriteLine();
        Console.WriteLine("--- Select: ids, labels, totals ---");
        Console.WriteLine($"OrderIds: {string.Join(", ", orderIds)}");
        foreach (string label in orderLabels)
        {
            Console.WriteLine($"  {label}");
        }

        decimal combinedTotal = orderTotals.Sum();
        Console.WriteLine($"Sum of projected totals: {combinedTotal:C}");

        /*
         * --- 4b. Select changes the element TYPE, not just the fields ---
         *
         * The pipeline after Select is IEnumerable<TResult>. Downstream operators
         * see only TResult — original Order members are gone unless you projected
         * them into TResult.
         * -------------------------------------------------------------------------
         */

        IEnumerable<DateOnly> placedDates = orders.Select(o => o.PlacedOn);
        Console.WriteLine($"Placed dates: {string.Join(", ", placedDates.Select(d => d.ToString("yyyy-MM-dd")))}");

        /*
         * =========================================================================
         * SECTION 5: THE NESTED-SEQUENCE TRAP — Select DOES NOT FLATTEN
         * =========================================================================
         *
         * Mapping each order to its Lines collection yields a sequence OF sequences:
         *
         *   orders.Select(o => o.Lines)
         *     → IEnumerable<IReadOnlyList<OrderLine>>   // still nested
         *
         * You need a nested foreach (or a second SelectMany) to reach SKUs.
         * Section 9 replaces this with a single SelectMany.
         * -------------------------------------------------------------------------
         */

        IEnumerable<IReadOnlyList<OrderLine>> nestedLines = orders.Select(o => o.Lines);
        int nestedSequenceCount = nestedLines.Count(); // Count of INNER LISTS, not SKUs
        int skuCountViaNestedLoops = 0;
        foreach (IReadOnlyList<OrderLine> lines in nestedLines)
        {
            skuCountViaNestedLoops += lines.Count; // manual flatten proof
        }

        Console.WriteLine();
        Console.WriteLine("--- Select to nested collection (not flat) ---");
        Console.WriteLine(
            $"Select(o => o.Lines) → {nestedSequenceCount} inner list(s); " +
            $"SKU rows inside = {skuCountViaNestedLoops}.");

        /*
         * =========================================================================
         * SECTION 6: ANONYMOUS TYPES IN PROJECTIONS
         * =========================================================================
         *
         * When the result shape is used locally (console report, one-off query),
         * the compiler synthesizes a type from:
         *
         *   new { OrderId = o.OrderId, Customer = o.Customer, Total = o.OrderTotal }
         *
         * Rules:
         *   • Property names come from identifiers (OrderId) or explicit (Total = …)
         *   • Always use var for locals — you cannot write the type name in source
         *   • Cannot declare a public method return type as anonymous (use DTO/tuple)
         *   • Equals/GetHashCode compare property values (structural equality)
         *   • Two anonymous types are the same only if property names, types, and
         *     order match in the same assembly
         *
         * --- 6a. Order header summary (anonymous) ---
         * -------------------------------------------------------------------------
         */

        var orderHeaders = orders
            .Select(o => new
            {
                o.OrderId,
                o.Customer,
                LineCount = o.Lines.Count,
                o.OrderTotal,
            })
            .OrderByDescending(h => h.OrderTotal); // OrderBy sees projected shape

        Console.WriteLine();
        Console.WriteLine("--- Anonymous projection: order headers ---");
        foreach (var header in orderHeaders)
        {
            Console.WriteLine(
                $"  {header.OrderId,-8} {header.Customer,-16} lines={header.LineCount} total={header.OrderTotal,8:C}");
        }

        /*
         * --- 6b. Structural equality ---
         *
         * Two anonymous instances with the same property names and values compare
         * equal even though the compiler generated a unique type name.
         * -------------------------------------------------------------------------
         */

        var sampleA = new { Code = "WH-01", Qty = 3 };
        var sampleB = new { Code = "WH-01", Qty = 3 };
        bool anonymousEqual = sampleA.Equals(sampleB);

        Console.WriteLine();
        Console.WriteLine("--- Anonymous type equality ---");
        Console.WriteLine($"sampleA.Equals(sampleB) = {anonymousEqual}");

        /*
         * =========================================================================
         * SECTION 7: VALUE TUPLES IN PROJECTIONS
         * =========================================================================
         *
         * Value tuples ((string Id, decimal Total)) are a lightweight named shape
         * that CAN appear in method signatures — unlike anonymous types.
         *
         *   .Select(o => (o.OrderId, o.OrderTotal))
         *   .Select(o => (Id: o.OrderId, Total: o.OrderTotal))
         *
         * Prefer tuples for small private helpers; prefer named records for public
         * APIs and serializers (Section 2 / Section 8).
         * -------------------------------------------------------------------------
         */

        IEnumerable<(string OrderId, decimal Total)> orderTuples =
            orders.Select(o => (o.OrderId, Total: o.OrderTotal)); // named Total element

        Console.WriteLine();
        Console.WriteLine("--- Tuple projection: (OrderId, Total) ---");
        foreach ((string orderId, decimal total) in orderTuples)
        {
            Console.WriteLine($"  {orderId}  {total:C}");
        }

        IEnumerable<(string OrderId, decimal Total)> highValueTuples =
            ProjectHighValueOrders(orders, 100m); // helper return type = tuple

        Console.WriteLine("High-value tuples from helper (tuple return type):");
        foreach ((string orderId, decimal total) in highValueTuples)
        {
            Console.WriteLine($"  {orderId}  {total:C}");
        }

        /*
         * =========================================================================
         * SECTION 8: PROJECTING INTO A NAMED TYPE
         * =========================================================================
         *
         * BuildOrderLineSummaries returns IEnumerable<OrderLineSummary> — a stable
         * contract. That return type is impossible with anonymous types.
         * -------------------------------------------------------------------------
         */

        IEnumerable<OrderLineSummary> namedSummaries = BuildOrderLineSummaries(orders);

        Console.WriteLine();
        Console.WriteLine("--- Named projection: OrderLineSummary rows ---");
        foreach (OrderLineSummary row in namedSummaries)
        {
            Console.WriteLine(
                $"  {row.OrderId,-8} {row.Sku,-8} {row.ProductName,-22} qty={row.Quantity,2}  {row.LineTotal,8:C}");
        }

        List<OrderLineSummary> materialized = namedSummaries.ToList(); // materialize for export
        Console.WriteLine($"Materialized {materialized.Count} OrderLineSummary instance(s) for export.");

        /*
         * =========================================================================
         * SECTION 9: SelectMany — FLATTEN NESTED SEQUENCES
         * =========================================================================
         *
         * SelectMany maps each source element to an INNER sequence, then
         * concatenates those sequences into one flat IEnumerable<TResult>.
         *
         * Overloads you will use constantly:
         *
         *   SelectMany(source, collectionSelector)
         *     collectionSelector: TSource → IEnumerable<TCollection>
         *     result: flattened TCollection elements
         *
         *   SelectMany(source, collectionSelector, resultSelector)
         *     resultSelector: (TSource, TCollection) → TResult
         *     use when each child needs its parent context (OrderId, Customer, …)
         *
         * Warehouse use case: one pick-list row per physical line item, carrying
         * OrderId and Customer for the shipping label printer.
         *
         * --- 9a. Flatten lines only (no parent in result) ---
         * -------------------------------------------------------------------------
         */

        IEnumerable<OrderLine> allLines = orders.SelectMany(o => o.Lines);

        Console.WriteLine();
        Console.WriteLine("--- SelectMany: all line items (SKU only) ---");
        foreach (OrderLine line in allLines)
        {
            Console.WriteLine($"  {line.Sku,-8} {line.ProductName,-24} qty={line.Quantity,2}");
        }

        /*
         * --- 9b. SelectMany with resultSelector — parent context on each child ---
         *
         * Without resultSelector you lose OrderId when you flatten to OrderLine.
         * The three-parameter form is the idiomatic fix:
         *
         *   SelectMany(order => order.Lines, (order, line) => new …)
         * -------------------------------------------------------------------------
         */

        IEnumerable<OrderLineSummary> flatPickRows = orders.SelectMany(
            order => order.Lines,
            (order, line) => new OrderLineSummary(
                order.OrderId,
                order.Customer,
                line.Sku,
                line.ProductName,
                line.Quantity,
                line.LineTotal));

        Console.WriteLine();
        Console.WriteLine("--- SelectMany + named projection: warehouse pick list ---");
        foreach (OrderLineSummary pick in flatPickRows)
        {
            Console.WriteLine(
                $"  PICK  {pick.OrderId}  {pick.Customer,-14}  {pick.Sku,-8}  x{pick.Quantity}  {pick.ProductName}");
        }

        /*
         * --- 9c. Equivalence: SelectMany vs Select + SelectMany ---
         *
         *   orders.SelectMany(o => o.Lines)
         *     ↔  orders.Select(o => o.Lines).SelectMany(lines => lines)
         *
         * Prefer the single SelectMany when flattening one nested level.
         * -------------------------------------------------------------------------
         */

        int flatCount = orders.SelectMany(o => o.Lines).Count();
        int nestedThenFlatCount = orders.Select(o => o.Lines).SelectMany(lines => lines).Count();

        Console.WriteLine();
        Console.WriteLine("--- SelectMany equivalence ---");
        Console.WriteLine(
            $"Direct SelectMany: {flatCount} lines; Select then SelectMany: {nestedThenFlatCount} lines.");

        /*
         * =========================================================================
         * SECTION 10: MULTI-LEVEL SelectMany AND EMPTY INNERS
         * =========================================================================
         *
         * --- 10a. Two-level flatten — orders → lines → tags ---
         *
         * Chain SelectMany when nesting is deeper than one level:
         *
         *   orders.SelectMany(o => o.Lines).SelectMany(line => line.Tags)
         *
         * Or nest in one expression with resultSelector if you need parent fields.
         * -------------------------------------------------------------------------
         */

        IEnumerable<string> allTags = orders
            .SelectMany(o => o.Lines)
            .SelectMany(line => line.Tags); // second flatten: line → tags

        IEnumerable<string> taggedSkuLabels = orders.SelectMany(
            order => order.Lines,
            (order, line) => (order.OrderId, line.Sku, line.Tags))
            .SelectMany(
                row => row.Tags,
                (row, tag) => $"{row.OrderId}/{row.Sku}:{tag}"); // keep OrderId+Sku with each tag

        Console.WriteLine();
        Console.WriteLine("--- Multi-level SelectMany: tags ---");
        Console.WriteLine($"All tags (flat): {string.Join(", ", allTags)}");
        foreach (string label in taggedSkuLabels)
        {
            Console.WriteLine($"  {label}");
        }

        /*
         * --- 10b. Empty inner sequences contribute nothing ---
         *
         * If collectionSelector returns an empty sequence for an element, that
         * element simply adds zero rows — no null, no exception.
         * -------------------------------------------------------------------------
         */

        Order[] withEmpty = orders
            .Append(new Order(
                "ORD-EMPTY",
                "Empty Cart Co",
                new DateOnly(2026, 8, 7),
                Array.Empty<OrderLine>())) // Lines.Count == 0
            .ToArray();

        int linesIncludingEmptyOrder = withEmpty.SelectMany(o => o.Lines).Count();
        Console.WriteLine();
        Console.WriteLine("--- Empty inner sequences ---");
        Console.WriteLine(
            $"Orders={withEmpty.Length}; flat lines after SelectMany={linesIncludingEmptyOrder} " +
            "(empty order added 0 rows).");

        /*
         * =========================================================================
         * SECTION 11: INDEX PARAMETER IN Select AND SelectMany
         * =========================================================================
         *
         * Overloads expose the 0-based index of each element AFTER prior operators
         * in the pipeline:
         *
         *   .Select((item, index) => …)
         *   .SelectMany((item, index) => …)
         *
         * Index resets at 0 for the input to THAT call — not a global row number
         * across the whole history unless you project from a single flat sequence.
         *
         * --- 11a. Numbered packing slip lines per order ---
         * -------------------------------------------------------------------------
         */

        Console.WriteLine();
        Console.WriteLine("--- Select with index: numbered slip lines ---");

        foreach (Order order in orders.Where(o => o.OrderId == "ORD-1002"))
        {
            Console.WriteLine($"Packing slip for {order.OrderId} — {order.Customer}");

            IEnumerable<string> numberedLines = order.Lines.Select(
                (line, index) =>
                    $"{index + 1,2}. {line.Sku,-8} {line.ProductName,-20} qty {line.Quantity,2}");

            foreach (string slipLine in numberedLines)
            {
                Console.WriteLine($"  {slipLine}");
            }
        }

        /*
         * --- 11b. Rank flattened pick rows across the whole queue ---
         *
         * SelectMany first (flat stream), then Select with index for global rank.
         * -------------------------------------------------------------------------
         */

        var rankedPickList = orders
            .SelectMany(
                order => order.Lines,
                (order, line) => new { order.OrderId, order.Customer, Line = line })
            .Select((entry, index) => new
            {
                PickSequence = index + 1, // 1-based display rank
                entry.OrderId,
                entry.Customer,
                entry.Line.Sku,
                entry.Line.Quantity,
            });

        Console.WriteLine();
        Console.WriteLine("--- Index after SelectMany: global pick sequence ---");
        foreach (var pick in rankedPickList)
        {
            Console.WriteLine(
                $"  #{pick.PickSequence,2}  {pick.OrderId}  {pick.Sku,-8}  qty {pick.Quantity}");
        }

        /*
         * --- 11c. Indexed SelectMany — source index in the collection selector ---
         *
         * SelectMany((order, orderIndex) => …) tags each flattened row with the
         * parent order's position in the source array.
         * -------------------------------------------------------------------------
         */

        var indexedByOrder = orders.SelectMany(
            (order, orderIndex) => order.Lines.Select(line => new
            {
                OrderIndex = orderIndex, // index of Order in orders[]
                order.OrderId,
                line.Sku,
            }));

        Console.WriteLine();
        Console.WriteLine("--- SelectMany with source index ---");
        foreach (var row in indexedByOrder)
        {
            Console.WriteLine($"  order#{row.OrderIndex}  {row.OrderId}  {row.Sku}");
        }

        /*
         * =========================================================================
         * SECTION 12: QUERY SYNTAX — select, NESTED from, AND let
         * =========================================================================
         *
         * Query expression select maps to Select / SelectMany under the hood:
         *
         *   from o in orders select o.OrderId
         *     ↔  orders.Select(o => o.OrderId)
         *
         *   from o in orders
         *   from line in o.Lines
         *   select line
         *     ↔  orders.SelectMany(o => o.Lines)
         *
         *   from o in orders
         *   from line in o.Lines
         *   select new { o.OrderId, line.Sku }
         *     ↔  SelectMany with resultSelector
         *
         * let introduces a range variable for a computed intermediate — useful
         * when the same expression appears in where / orderby / select:
         *
         *   from o in orders
         *   let total = o.OrderTotal
         *   where total >= 100m
         *   select new { o.OrderId, total }
         *
         * Query expressions must end with select or group (see 04. Grouping).
         *
         * --- 12a. Query select with anonymous type ---
         * -------------------------------------------------------------------------
         */

        var highValueHeaders =
            from o in orders
            where o.OrderTotal >= 100m
            orderby o.PlacedOn
            select new
            {
                o.OrderId,
                o.PlacedOn,
                o.OrderTotal,
            };

        Console.WriteLine();
        Console.WriteLine("--- Query syntax → anonymous (orders >= $100) ---");
        foreach (var row in highValueHeaders)
        {
            Console.WriteLine($"  {row.OrderId}  {row.PlacedOn:yyyy-MM-dd}  {row.OrderTotal:C}");
        }

        /*
         * --- 12b. let clause — compute once, reuse in where / select ---
         * -------------------------------------------------------------------------
         */

        var letProjection =
            from o in orders
            let lineCount = o.Lines.Count
            let total = o.OrderTotal
            where lineCount >= 2
            orderby total descending
            select new { o.OrderId, lineCount, total };

        Console.WriteLine();
        Console.WriteLine("--- Query let: multi-line orders ---");
        foreach (var row in letProjection)
        {
            Console.WriteLine($"  {row.OrderId}  lines={row.lineCount}  total={row.total:C}");
        }

        /*
         * --- 12c. Nested from → SelectMany (with parent fields in select) ---
         * -------------------------------------------------------------------------
         */

        var queryFlatSkus =
            from o in orders
            from line in o.Lines
            select $"{o.OrderId}:{line.Sku}"; // compiles to SelectMany + resultSelector

        var queryTags =
            from o in orders
            from line in o.Lines
            from tag in line.Tags
            select $"{o.OrderId}/{line.Sku}:{tag}";

        Console.WriteLine();
        Console.WriteLine("--- Query nested from (SelectMany) ---");
        Console.WriteLine("  SKUs: " + string.Join(", ", queryFlatSkus));
        Console.WriteLine("  Tags: " + string.Join(", ", queryTags));

        /*
         * =========================================================================
         * SECTION 13: Zip — PAIRWISE PROJECTION OF TWO SEQUENCES
         * =========================================================================
         *
         * Zip walks two sequences in lockstep and projects each pair:
         *
         *   first.Zip(second, (a, b) => result)
         *   first.Zip(second)   // .NET 5+: yields (TFirst, TSecond)
         *
         * Stops when either sequence ends (length = Min(count1, count2)).
         * Use when you already have two parallel sequences (labels + values,
         * stations + orders) — not for parent/child nesting (use SelectMany).
         * -------------------------------------------------------------------------
         */

        string[] packStations = { "Dock-A", "Dock-B", "Dock-C" };

        IEnumerable<string> stationAssignments = packStations.Zip(
            orders,
            (station, order) => $"{station} → {order.OrderId} ({order.Customer})");

        Console.WriteLine();
        Console.WriteLine("--- Zip: pair pack stations with orders ---");
        foreach (string assignment in stationAssignments)
        {
            Console.WriteLine($"  {assignment}");
        }

        IEnumerable<(string Station, string OrderId)> stationTuples =
            packStations.Zip(orders.Select(o => o.OrderId)); // Zip → value tuples

        Console.WriteLine("Zip without resultSelector → value tuples:");
        foreach ((string station, string orderId) in stationTuples)
        {
            Console.WriteLine($"  {station} / {orderId}");
        }

        /*
         * =========================================================================
         * SECTION 14: DEFERRED PROJECTION AND COMPOSITION
         * =========================================================================
         *
         * Projections compose with Where, OrderBy, and Take like any LINQ operator.
         * The selector runs only when the pipeline is consumed — mutating data
         * between building and enumerating can change results (same lesson as ch.01).
         *
         * COVERED IN DETAIL LATER → 10. Conversion Operations (ToList / ToArray)
         * COVERED IN DETAIL LATER → 11. Partitioning Operations (Take / Skip depth)
         * -------------------------------------------------------------------------
         */

        IEnumerable<string> topPickLabels = orders
            .Where(o => o.PlacedOn >= new DateOnly(2026, 8, 5))
            .SelectMany(o => o.Lines, (o, line) => new { o.OrderId, line.Sku, line.LineTotal })
            .OrderByDescending(x => x.LineTotal)
            .Take(3)
            .Select((x, index) => $"Top {index + 1}: {x.OrderId} {x.Sku} {x.LineTotal:C}");

        Console.WriteLine();
        Console.WriteLine("--- Composed pipeline: recent orders, top 3 line totals ---");
        foreach (string label in topPickLabels)
        {
            Console.WriteLine($"  {label}");
        }
    }

    /*
     * =========================================================================
     * SECTION 15: HELPERS — LABELS, TUPLE RETURNS, SAMPLE DATA
     * =========================================================================
     *
     * FormatOrderLabel shows Select with a method group (Section 4).
     * ProjectHighValueOrders returns tuples — valid as a declared return type
     * (anonymous types cannot be used that way).
     * BuildOrderLineSummaries / BuildSampleOrders feed every demo above.
     * -------------------------------------------------------------------------
     */

    private static string FormatOrderLabel(Order order) =>
        $"{order.OrderId} ({order.Customer})"; // Select method-group target

    private static IEnumerable<(string OrderId, decimal Total)> ProjectHighValueOrders(
        IEnumerable<Order> orders,
        decimal minimumTotal)
    {
        return orders
            .Where(o => o.OrderTotal >= minimumTotal)
            .Select(o => (o.OrderId, Total: o.OrderTotal));
    }

    private static IEnumerable<OrderLineSummary> BuildOrderLineSummaries(IEnumerable<Order> orders)
    {
        return orders.SelectMany(
            order => order.Lines,
            (order, line) => new OrderLineSummary(
                order.OrderId,
                order.Customer,
                line.Sku,
                line.ProductName,
                line.Quantity,
                line.LineTotal));
    }

    private static Order[] BuildSampleOrders()
    {
        return
        [
            new Order(
                "ORD-1001",
                "Contoso Ltd",
                new DateOnly(2026, 8, 4),
                [
                    new OrderLine("SKU-A1", "Wireless Mouse", 2, 24.99m, ["peripheral", "usb"]),
                    new OrderLine("SKU-B2", "USB-C Hub", 1, 45.00m, ["dock", "usb"]),
                ]),
            new Order(
                "ORD-1002",
                "Fabrikam Inc",
                new DateOnly(2026, 8, 5),
                [
                    new OrderLine("SKU-C3", "Mechanical Keyboard", 1, 129.99m, ["peripheral"]),
                    new OrderLine("SKU-D4", "Desk Mat", 2, 18.50m, ["accessory"]),
                    new OrderLine("SKU-E5", "Monitor Arm", 1, 89.00m, ["ergonomic", "desk"]),
                ]),
            new Order(
                "ORD-1003",
                "Northwind Traders",
                new DateOnly(2026, 8, 6),
                [
                    new OrderLine("SKU-F6", "Webcam HD", 3, 59.99m, ["video", "usb"]),
                ]),
        ];
    }
}

/*
 * =========================================================================
 * QUICK REFERENCE — PROJECTION OPERATIONS
 * =========================================================================
 *
 * --- Select (1 source element → 1 result) ---
 *
 *   source.Select(x => x.Property)
 *   source.Select(x => new { x.Id, x.Name })
 *   source.Select(x => (x.Id, x.Name))
 *   source.Select(x => new MyDto(x.Id, x.Name))
 *   source.Select(FormatLabel)                 // method group when signatures match
 *   source.Select((x, index) => new { Rank = index + 1, x.Id })
 *
 * --- SelectMany (1 source element → many results, flattened) ---
 *
 *   source.SelectMany(x => x.Children)
 *   source.SelectMany(x => x.Children, (parent, child) => new { parent.Id, child })
 *   source.SelectMany((x, index) => x.Children.Select(c => (index, c)))
 *   source.SelectMany(x => x.Children).SelectMany(c => c.Tags)   // multi-level
 *
 * --- Zip (pairwise projection) ---
 *
 *   first.Zip(second, (a, b) => …)
 *   first.Zip(second)   // → IEnumerable<(TFirst, TSecond)>
 *
 * --- Anonymous vs tuple vs named ---
 *
 *   new { a, b }           — local only; var; structural equality
 *   (a, b) / (Id: a, …)    — small shapes; OK as return types
 *   new MyRecord(a, b)     — APIs, tests, serialization
 *
 * --- Method vs query syntax ---
 *
 *   orders.Select(o => o.OrderId)
 *   from o in orders select o.OrderId
 *
 *   orders.SelectMany(o => o.Lines)
 *   from o in orders from line in o.Lines select line
 *
 *   orders.SelectMany(o => o.Lines, (o, line) => new { o.OrderId, line.Sku })
 *   from o in orders from line in o.Lines select new { o.OrderId, line.Sku }
 *
 *   from o in orders
 *   let total = o.OrderTotal
 *   where total >= 100m
 *   select new { o.OrderId, total }
 *
 * --- Common mistakes ---
 *
 *  Mistake                              | Result
 *  -------------------------------------|----------------------------------
 *  Select when you need flatten           | Nested IEnumerable; use SelectMany
 *  Flatten without resultSelector         | Lose parent fields (OrderId, …)
 *  Anonymous type as public API return    | Not allowed — use DTO or tuple
 *  Expecting index from global source     | Index is per Select/SelectMany input
 *  Zip for parent/child nesting           | Wrong tool — use SelectMany
 *  Forgetting deferred execution          | Projection runs at foreach, not at Select
 *  Empty inner sequence                   | Contributes 0 rows (not an error)
 *
 * --- Related chapters ---
 *
 *   01. Introduction to LINQ      — deferred execution, IEnumerable<T>
 *   02. Filtering & Aggregation   — Where before Select; Select preview
 *   04. Grouping                  — Select after GroupBy; group … by
 *   05. Joins                     — Join / GroupJoin (related combining shapes)
 *   10. Conversion Operations     — ToList, ToArray after projection
 *   11. Partitioning Operations   — Take / Skip after projection
 *
 * =========================================================================
 */
