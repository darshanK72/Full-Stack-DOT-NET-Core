/*
 * =============================================================================
 * 05. JOINS — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: LINQ join operators — correlate two (or more) sequences by shared
 *        keys. Join produces a flat inner join; GroupJoin nests matching
 *        inners per outer element; left outer join is the GroupJoin +
 *        SelectMany + DefaultIfEmpty pattern. Query syntax uses join / on /
 *        equals / into; method syntax uses Join and GroupJoin with key and
 *        result selectors.
 *
 * WHY IT MATTERS:
 *   Real data rarely lives in one list. Orders reference customers;
 *   shipments reference orders. Joins correlate related rows without nested
 *   loops and manual Dictionary lookups — the same shapes appear in
 *   LINQ to Objects, EF Core, and SQL.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Inner join — query syntax (join / on / equals) and method syntax
 *   2.  Result selectors — anonymous types, value tuples, named records
 *   3.  One-to-many matches — how Join multiplies rows
 *   4.  Composite keys and IEqualityComparer<TKey>
 *   5.  Chained joins — customers + orders + shipments
 *   6.  GroupJoin — into groups; one outer row with a nested inner sequence
 *   7.  Join vs GroupJoin — flat matches vs nested groups; row-count rules
 *   8.  Left outer join — GroupJoin + SelectMany + DefaultIfEmpty
 *   9.  Keeping unmatched rows by swapping outer/inner (right-outer shape)
 *  10.  Cross-join preview via SelectMany vs true key-based Join
 *
 * =============================================================================
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Joins;

/*
 * =========================================================================
 * SECTION 1: SAMPLE MODELS — CUSTOMERS, ORDERS, SHIPMENTS
 * =========================================================================
 *
 * Three related types model a small fulfillment system. Keys deliberately
 * cross collections so Join / GroupJoin have something to match on:
 *
 *  Type       | Key            | Foreign key / relates to
 *  -----------|----------------|------------------------------------------
 *  Customer   | CustomerId     | outer side for customer-centric joins
 *  Order      | OrderId        | CustomerId → Customer
 *  Shipment   | ShipmentId     | OrderId → Order
 *
 * Order is a reference-type record so left-outer-join DefaultIfEmpty can
 * yield null when a customer has no orders. Customer and Shipment are
 * readonly record structs (value types) — fine for inner joins.
 *
 * Result shapes used later:
 *   CustomerOrderGroup — GroupJoin output (name + nested orders)
 *   FulfillmentLine    — chained inner-join report row
 * -------------------------------------------------------------------------
 */

public readonly record struct Customer(int CustomerId, string Name, string Region);

public sealed record Order(int OrderId, int CustomerId, DateOnly OrderDate, decimal Total);

public readonly record struct Shipment(int ShipmentId, int OrderId, string Carrier, DateOnly ShipDate);

public readonly record struct CustomerOrderGroup(
    string Name,
    string Region,
    IEnumerable<Order> Orders);

public readonly record struct FulfillmentLine(
    int OrderId,
    string CustomerName,
    string Region,
    string Carrier,
    DateOnly ShipDate,
    decimal Total);

/*
 * =========================================================================
 * SECTION 2: COMPOSITE-KEY DEMO TYPES — WAREHOUSE SKU ROWS
 * =========================================================================
 *
 * Join keys are often more than one field. These two sequences share a
 * (Sku, WarehouseCode) pair. Method syntax joins on a value tuple key;
 * query syntax uses an anonymous type on each side of equals.
 * -------------------------------------------------------------------------
 */

public readonly record struct StockRow(string Sku, string WarehouseCode, int OnHand);

public readonly record struct ReorderRow(string Sku, string WarehouseCode, int ReorderQty);

public class Program
{
    /*
     * =========================================================================
     * SECTION 3: DEMONSTRATION — Main orchestrates the join chapter
     * =========================================================================
     *
     * Seed data has intentional gaps so join semantics are visible:
     *
     *   • Harbor Supplies (C004) has NO orders — left join keeps them; inner drops them
     *   • Order O105 has NO shipment — chained inner join to shipments excludes it
     *   • Order O101 has TWO shipments — Join multiplies that order into two rows
     *   • Cascade Foods (C003) has TWO orders — GroupJoin nests both under one customer
     *
     * Operator map for this chapter:
     *
     *  Operator / pattern   | Shape                         | Unmatched outer?
     *  ---------------------|--------------------------------|------------------
     *  Join                 | Flat row per key match         | Dropped
     *  GroupJoin            | One outer + IEnumerable inners | Kept (empty group)
     *  Left outer (pattern) | Flat row; null/default inner   | Kept
     *  Cross (SelectMany)   | Every A × every B              | N/A (no key)
     *
     * COVERED IN DETAIL LATER → 04. Grouping
     *   GroupBy partitions ONE sequence by a key. GroupJoin correlates TWO
     *   sequences and nests the inner matches — different operators.
     *
     * COVERED IN DETAIL LATER → 08. Projection Operations
     *   Select / SelectMany depth; result-selector overloads used lightly here.
     *
     * COVERED IN DETAIL LATER → 12. Generation Operations
     *   DefaultIfEmpty as a generation-adjacent operator; here it is only the
     *   left-outer-join flattening tool.
     * -------------------------------------------------------------------------
     */
    public static void Main(string[] args)
    {
        Customer[] customers =
        [
            new Customer(1, "Acme Retail", "West"),
            new Customer(2, "Bright Labs", "East"),
            new Customer(3, "Cascade Foods", "West"),
            new Customer(4, "Harbor Supplies", "East"), // no orders — left-join demo
        ];

        Order[] orders =
        [
            new Order(101, 1, new DateOnly(2026, 3, 1), 420.00m),
            new Order(102, 2, new DateOnly(2026, 3, 3), 189.50m),
            new Order(103, 3, new DateOnly(2026, 3, 5), 975.25m),
            new Order(104, 3, new DateOnly(2026, 3, 7), 310.00m), // second order for C003
            new Order(105, 1, new DateOnly(2026, 3, 9), 55.00m),  // no shipment yet
        ];

        Shipment[] shipments =
        [
            new Shipment(9001, 101, "FedEx", new DateOnly(2026, 3, 2)),
            new Shipment(9005, 101, "UPS", new DateOnly(2026, 3, 2)), // second ship for O101
            new Shipment(9002, 102, "UPS", new DateOnly(2026, 3, 4)),
            new Shipment(9003, 103, "FedEx", new DateOnly(2026, 3, 6)),
            new Shipment(9004, 104, "DHL", new DateOnly(2026, 3, 8)),
        ];

        Console.WriteLine("=== Join tutorial — customers, orders, shipments ===");
        PrintSeedCounts(customers, orders, shipments);


        /*
         * =========================================================================
         * SECTION 4: INNER JOIN — QUERY SYNTAX (join / on / equals)
         * =========================================================================
         *
         * Join matches rows from two sequences where keys are EQUAL.
         * Unmatched rows on EITHER side are dropped — that is an INNER join.
         *
         * Query syntax:
         *
         *   from outer in outerSequence
         *   join inner in innerSequence
         *     on outer.Key equals inner.Key
         *   select …
         *
         * Rules:
         *
         *   • Use the keyword equals (not ==) — the compiler builds key selectors
         *   • Outer key expression must be on the left of equals; inner on the right
         *   • Key types must match (or be compatible) — mismatch → CS1943
         *   • Result is flat — one output element per successful match
         *
         * Harbor Supplies never appears here (no matching orders).
         * -------------------------------------------------------------------------
         */

        var ordersWithCustomersQuery =
            from order in orders
            join customer in customers
                on order.CustomerId equals customer.CustomerId // equals required in query join
            select new
            {
                order.OrderId,
                CustomerName = customer.Name,
                order.Total,
            };

        Console.WriteLine();
        Console.WriteLine("--- SECTION 4: Inner join (query syntax) — orders + customers ---");
        foreach (var row in ordersWithCustomersQuery)
        {
            Console.WriteLine($"  Order {row.OrderId}  {row.CustomerName,-16}  {row.Total,8:C}");
        }


        /*
         * =========================================================================
         * SECTION 5: INNER JOIN — METHOD SYNTAX (Enumerable.Join)
         * =========================================================================
         *
         *   outer.Join(
         *       inner,
         *       outerKeySelector,   // Func<TOuter, TKey>
         *       innerKeySelector,   // Func<TInner, TKey>
         *       resultSelector)     // Func<TOuter, TInner, TResult>
         *
         * Mapping from query syntax:
         *
         *  Query part              | Method parameter
         *  ------------------------|------------------------------------------
         *  from order in orders    | outer sequence (receiver)
         *  join customer in …      | inner sequence
         *  on order.CustomerId …   | outerKeySelector / innerKeySelector
         *  select new { … }        | resultSelector (order, customer) => …
         *
         * Both forms are deferred — nothing runs until foreach / Count / ToList.
         * An overload accepts IEqualityComparer<TKey> when default equality is wrong
         * (shown in Section 7).
         * -------------------------------------------------------------------------
         */

        IEnumerable<(int OrderId, string CustomerName, decimal Total)> ordersWithCustomersMethod =
            orders.Join(
                customers,
                order => order.CustomerId,       // outer key
                customer => customer.CustomerId, // inner key
                (order, customer) => (order.OrderId, customer.Name, order.Total)); // result row

        Console.WriteLine();
        Console.WriteLine("--- SECTION 5: Inner join (method syntax) — same rows ---");
        foreach ((int orderId, string customerName, decimal total) in ordersWithCustomersMethod)
        {
            Console.WriteLine($"  Order {orderId}  {customerName,-16}  {total,8:C}");
        }


        /*
         * =========================================================================
         * SECTION 6: ONE-TO-MANY — JOIN MULTIPLIES ROWS
         * =========================================================================
         *
         * Join is not "one row per outer." It emits one result per KEY MATCH.
         * If one order has two shipments, that order appears twice in the join.
         *
         *   Order 101 → Shipment 9001 (FedEx)
         *   Order 101 → Shipment 9005 (UPS)
         *
         * Count(orders) can be less than Count(join results). GroupJoin keeps
         * one outer element and nests both shipments instead (Section 9).
         * -------------------------------------------------------------------------
         */

        IEnumerable<(int OrderId, string Carrier, DateOnly ShipDate)> orderShipments =
            orders.Join(
                shipments,
                order => order.OrderId,
                shipment => shipment.OrderId,
                (order, shipment) => (order.OrderId, shipment.Carrier, shipment.ShipDate));

        int order101ShipmentRows = orderShipments.Count(row => row.OrderId == 101);

        Console.WriteLine();
        Console.WriteLine("--- SECTION 6: One-to-many — order × shipments ---");
        foreach ((int orderId, string carrier, DateOnly shipDate) in orderShipments.OrderBy(r => r.OrderId))
        {
            Console.WriteLine($"  Order {orderId}  {carrier,-6}  {shipDate:yyyy-MM-dd}");
        }

        Console.WriteLine($"  Order 101 shipment rows from Join: {order101ShipmentRows} (expect 2)");
        Console.WriteLine($"  Distinct orders in join result: {orderShipments.Select(r => r.OrderId).Distinct().Count()}");


        /*
         * =========================================================================
         * SECTION 7: COMPOSITE KEYS AND IEqualityComparer
         * =========================================================================
         *
         * --- 7a. Composite keys ---
         *
         * When equality needs two (or more) fields, project a single TKey:
         *
         *   Method:  value tuple  (Sku, WarehouseCode)
         *   Query:   anonymous type  new { Sku, WarehouseCode } on BOTH sides
         *
         * Anonymous-type property names and order must match for equals.
         *
         * --- 7b. IEqualityComparer<TKey> ---
         *
         * Join / GroupJoin overloads accept a comparer as the last argument.
         * Useful for case-insensitive string keys or custom equality.
         *
         *   outer.Join(inner, ok, ik, result, StringComparer.OrdinalIgnoreCase)
         * -------------------------------------------------------------------------
         */

        StockRow[] stock =
        [
            new StockRow("SKU-100", "WH-A", 40),
            new StockRow("SKU-100", "WH-B", 12),
            new StockRow("SKU-200", "WH-A", 8),
            new StockRow("SKU-300", "WH-C", 25), // no matching reorder
        ];

        ReorderRow[] reorders =
        [
            new ReorderRow("SKU-100", "WH-A", 20),
            new ReorderRow("SKU-100", "WH-B", 15),
            new ReorderRow("SKU-200", "WH-A", 30),
            new ReorderRow("sku-200", "wh-a", 5), // same logical key; needs comparer
        ];

        // Query syntax — anonymous composite key (property names must align)
        var compositeQuery =
            from s in stock
            join r in reorders
                on new { s.Sku, s.WarehouseCode } equals new { r.Sku, r.WarehouseCode }
            select new { s.Sku, s.WarehouseCode, s.OnHand, r.ReorderQty };

        // Method syntax — value-tuple key
        var compositeMethod = stock.Join(
            reorders,
            s => (s.Sku, s.WarehouseCode),
            r => (r.Sku, r.WarehouseCode),
            (s, r) => new { s.Sku, s.WarehouseCode, s.OnHand, r.ReorderQty });

        // Case-insensitive join on a single string key (warehouse codes alone)
        string[] activeWarehouses = ["wh-a", "WH-B", "WH-C"];
        IEnumerable<(string Warehouse, int StockRows)> warehouseJoinIgnoreCase =
            activeWarehouses.Join(
                stock,
                code => code,
                row => row.WarehouseCode,
                (code, row) => (Warehouse: code, StockRows: 1),
                StringComparer.OrdinalIgnoreCase) // comparer overload
            .GroupBy(pair => pair.Warehouse, StringComparer.OrdinalIgnoreCase)
            .Select(g => (Warehouse: g.Key, StockRows: g.Count()));

        Console.WriteLine();
        Console.WriteLine("--- SECTION 7a: Composite key join (Sku + Warehouse) ---");
        Console.WriteLine($"  Query matches:  {compositeQuery.Count()}");
        Console.WriteLine($"  Method matches: {compositeMethod.Count()} (same default equality)");
        foreach (var row in compositeMethod.OrderBy(r => r.Sku).ThenBy(r => r.WarehouseCode))
        {
            Console.WriteLine($"  {row.Sku} @ {row.WarehouseCode}: on-hand {row.OnHand}, reorder {row.ReorderQty}");
        }

        Console.WriteLine("--- SECTION 7b: Join with StringComparer.OrdinalIgnoreCase ---");
        foreach ((string warehouse, int stockRows) in warehouseJoinIgnoreCase.OrderBy(w => w.Warehouse))
        {
            Console.WriteLine($"  Active code '{warehouse}' matched {stockRows} stock row(s)");
        }


        /*
         * =========================================================================
         * SECTION 8: CHAINED INNER JOINS — FULFILLMENT REPORT
         * =========================================================================
         *
         * Stack joins: each join's result feeds the next correlation.
         *
         *   orders ──CustomerId──▶ customers
         *   orders ──OrderId─────▶ shipments
         *
         * Query syntax allows multiple join clauses in one expression. Method
         * syntax nests Join calls (join the previous projection).
         *
         * Order O105 has no shipment — INNER join drops it. Section 11 shows
         * how to keep outer rows when the inner side is missing.
         *
         * Because O101 has two shipments, the chained report includes TWO lines
         * for Acme / O101 (one per carrier) — same one-to-many rule as Section 6.
         * -------------------------------------------------------------------------
         */

        IEnumerable<FulfillmentLine> fulfillmentQuery =
            from order in orders
            join customer in customers on order.CustomerId equals customer.CustomerId
            join shipment in shipments on order.OrderId equals shipment.OrderId // second join
            select new FulfillmentLine(
                order.OrderId,
                customer.Name,
                customer.Region,
                shipment.Carrier,
                shipment.ShipDate,
                order.Total);

        // Method syntax — nest Join: (order, customer) then join shipments
        IEnumerable<FulfillmentLine> fulfillmentMethod =
            orders
                .Join(
                    customers,
                    order => order.CustomerId,
                    customer => customer.CustomerId,
                    (order, customer) => new { order, customer })
                .Join(
                    shipments,
                    row => row.order.OrderId,
                    shipment => shipment.OrderId,
                    (row, shipment) => new FulfillmentLine(
                        row.order.OrderId,
                        row.customer.Name,
                        row.customer.Region,
                        shipment.Carrier,
                        shipment.ShipDate,
                        row.order.Total));

        Console.WriteLine();
        Console.WriteLine("--- SECTION 8: Chained inner joins — shipped orders only ---");
        foreach (FulfillmentLine line in fulfillmentQuery.OrderBy(l => l.OrderId).ThenBy(l => l.Carrier))
        {
            Console.WriteLine(
                $"  #{line.OrderId}  {line.CustomerName,-16}  {line.Region,-5}  " +
                $"{line.Carrier,-6}  {line.ShipDate:yyyy-MM-dd}  {line.Total,8:C}");
        }

        Console.WriteLine(
            $"  Query rows: {fulfillmentQuery.Count()}; method rows: {fulfillmentMethod.Count()} (expect equal)");


        /*
         * =========================================================================
         * SECTION 9: GroupJoin — QUERY into AND METHOD SYNTAX
         * =========================================================================
         *
         * GroupJoin keeps EVERY outer element and attaches a GROUP of matching
         * inner elements (possibly empty). Think "left join that stays nested"
         * rather than flattened SQL LEFT JOIN rows.
         *
         * Query syntax names the group with into:
         *
         *   join order in orders on customer.CustomerId equals order.CustomerId
         *     into orderGroup
         *
         * After into, the range variable "order" is gone — you work with orderGroup
         * (IEnumerable<Order>). A later from clause can flatten (Section 11).
         *
         * Method syntax:
         *
         *   customers.GroupJoin(
         *       orders,
         *       customer => customer.CustomerId,
         *       order => order.CustomerId,
         *       (customer, orderGroup) => …)
         *
         * Cascade Foods gets two orders in one group; Harbor Supplies gets empty.
         * Do not confuse query `into` after group … by (ch.04) with join … into
         * (GroupJoin) — same keyword, different translation.
         * -------------------------------------------------------------------------
         */

        IEnumerable<CustomerOrderGroup> customersWithOrderGroupsQuery =
            from customer in customers
            join order in orders
                on customer.CustomerId equals order.CustomerId
                into orderGroup // GroupJoin — nest matches under each customer
            select new CustomerOrderGroup(customer.Name, customer.Region, orderGroup);

        IEnumerable<CustomerOrderGroup> customersWithOrderGroupsMethod = customers.GroupJoin(
            orders,
            customer => customer.CustomerId,
            order => order.CustomerId,
            (customer, orderGroup) => new CustomerOrderGroup(customer.Name, customer.Region, orderGroup));

        // GroupJoin orders → shipments: O101 nests TWO carriers; O105 nests none
        var ordersWithShipmentGroups = orders.GroupJoin(
            shipments,
            order => order.OrderId,
            shipment => shipment.OrderId,
            (order, shipGroup) => new
            {
                order.OrderId,
                Carriers = shipGroup.Select(s => s.Carrier).ToArray(),
            });

        Console.WriteLine();
        Console.WriteLine("--- SECTION 9: GroupJoin — customers with nested order groups ---");
        PrintCustomerOrderGroups(customersWithOrderGroupsQuery, "  [query]  ");
        PrintCustomerOrderGroups(customersWithOrderGroupsMethod, "  [method] ");

        Console.WriteLine("--- SECTION 9b: GroupJoin — orders with nested shipment carriers ---");
        foreach (var row in ordersWithShipmentGroups.OrderBy(r => r.OrderId))
        {
            string carrierList = row.Carriers.Length == 0
                ? "(none)"
                : string.Join(", ", row.Carriers);
            Console.WriteLine($"  Order {row.OrderId}: {carrierList}");
        }


        /*
         * =========================================================================
         * SECTION 10: Join vs GroupJoin — DIFFERENCES THAT MATTER
         * =========================================================================
         *
         * Same keys, different result shapes and row counts:
         *
         *  Aspect              | Join (inner)              | GroupJoin
         *  --------------------|---------------------------|---------------------------
         *  SQL analogy         | INNER JOIN                | LEFT JOIN (nested)
         *  Output shape        | Flat (outer, inner) pair  | (outer, IEnumerable<inner>)
         *  Unmatched outer     | Dropped                   | Kept with empty sequence
         *  Unmatched inner     | Dropped                   | Never appear alone
         *  Typical Count()     | # of key matches          | |outer| always
         *  Query keyword       | join … on … equals …      | join … into groupName
         *  Method              | .Join(...)                | .GroupJoin(...)
         *
         * Demo: customers→orders. Inner Join yields 5 rows (one per order).
         * GroupJoin yields 4 (one per customer), including Harbor with zero orders.
         * orders→shipments Join yields 5 rows (O101 counted twice); GroupJoin
         * yields 5 groups (one per order), with O101's group size 2.
         *
         * Do not confuse with GroupBy (ch.04): GroupBy needs one sequence;
         * GroupJoin needs two sequences and key selectors on both.
         * -------------------------------------------------------------------------
         */

        int innerJoinRowCount = customers.Join(
            orders,
            customer => customer.CustomerId,
            order => order.CustomerId,
            (customer, order) => order.OrderId).Count(); // one result per match

        int groupJoinRowCount = customers.GroupJoin(
            orders,
            customer => customer.CustomerId,
            order => order.CustomerId,
            (customer, orderGroup) => orderGroup.Count()).Count(); // one result per outer

        int emptyGroups = customers.GroupJoin(
            orders,
            customer => customer.CustomerId,
            order => order.CustomerId,
            (customer, orderGroup) => new { customer.Name, Count = orderGroup.Count() })
            .Count(g => g.Count == 0); // Harbor Supplies

        int flatShipJoinCount = orders.Join(
            shipments,
            o => o.OrderId,
            s => s.OrderId,
            (o, s) => s.ShipmentId).Count();

        int nestedShipGroupCount = orders.GroupJoin(
            shipments,
            o => o.OrderId,
            s => s.OrderId,
            (o, g) => g).Count(); // always |orders|

        Console.WriteLine();
        Console.WriteLine("--- SECTION 10: Join vs GroupJoin — row counts ---");
        Console.WriteLine($"  Join (customers×orders, flat) : {innerJoinRowCount} rows (expect 5)");
        Console.WriteLine($"  GroupJoin (by customer)       : {groupJoinRowCount} groups (expect 4)");
        Console.WriteLine($"  GroupJoin empty groups        : {emptyGroups} (expect 1 — Harbor Supplies)");
        Console.WriteLine($"  Join (orders×shipments)       : {flatShipJoinCount} rows (expect 5 — O101×2)");
        Console.WriteLine($"  GroupJoin (orders×shipments)  : {nestedShipGroupCount} groups (expect 5 — |orders|)");


        /*
         * =========================================================================
         * SECTION 11: LEFT OUTER JOIN — GroupJoin + DefaultIfEmpty
         * =========================================================================
         *
         * LINQ to Objects has no LeftJoin operator. The idiomatic pattern:
         *
         *   1. GroupJoin outer with inner (Section 9)
         *   2. Flatten with a second from / SelectMany
         *   3. Call DefaultIfEmpty() on the inner group
         *
         * Query syntax:
         *
         *   from customer in customers
         *   join order in orders on customer.CustomerId equals order.CustomerId
         *     into orderGroup
         *   from order in orderGroup.DefaultIfEmpty()
         *   select …
         *
         * When orderGroup is empty, DefaultIfEmpty yields one default element:
         *   • reference types → null
         *   • value types     → default(T) (often useless for joins — prefer class/record)
         *
         * Use order?.OrderId (or null checks) — never dereference a missing match.
         *
         * Method syntax:
         *
         *   customers.GroupJoin(…)
         *     .SelectMany(
         *         x => x.orderGroup.DefaultIfEmpty(),
         *         (x, order) => …)
         *
         * Harbor Supplies appears once with a null order — inner Join omitted them.
         *
         * Optional: DefaultIfEmpty(fallbackInstance) injects a sentinel instead of null
         * (see 12. Generation Operations for the operator itself).
         * -------------------------------------------------------------------------
         */

        var leftJoinQuery =
            from customer in customers
            join order in orders
                on customer.CustomerId equals order.CustomerId
                into orderGroup
            from order in orderGroup.DefaultIfEmpty() // null Order when group empty
            orderby customer.CustomerId, order != null ? order.OrderId : 0
            select new
            {
                customer.CustomerId,
                customer.Name,
                OrderId = order != null ? order.OrderId : (int?)null,
                OrderTotal = order != null ? order.Total : (decimal?)null,
            };

        IEnumerable<(int CustomerId, string Name, int? OrderId, decimal? OrderTotal)> leftJoinMethod =
            customers.GroupJoin(
                orders,
                customer => customer.CustomerId,
                order => order.CustomerId,
                (customer, orderGroup) => new { customer, orderGroup })
            .SelectMany(
                x => x.orderGroup.DefaultIfEmpty(), // preserve unmatched customers
                (x, order) => (
                    x.customer.CustomerId,
                    x.customer.Name,
                    OrderId: order != null ? order.OrderId : (int?)null,
                    OrderTotal: order != null ? order.Total : (decimal?)null));

        Console.WriteLine();
        Console.WriteLine("--- SECTION 11: Left outer join — all customers, optional orders ---");
        foreach (var row in leftJoinQuery)
        {
            string orderPart = row.OrderId.HasValue
                ? $"Order {row.OrderId}  {row.OrderTotal:C}"
                : "(no orders)";
            Console.WriteLine($"  C{row.CustomerId:D3}  {row.Name,-16}  {orderPart}");
        }

        int harborRows = leftJoinQuery.Count(r => r.Name == "Harbor Supplies");
        Console.WriteLine($"  Harbor Supplies rows in left join: {harborRows} (expect 1 with null order)");
        Console.WriteLine($"  Method-syntax left join rows: {leftJoinMethod.Count()} (same as query)");


        /*
         * =========================================================================
         * SECTION 12: "RIGHT OUTER" SHAPE — SWAP OUTER AND INNER
         * =========================================================================
         *
         * There is no RightJoin operator either. To keep every INNER-side row
         * and optionally attach an outer match, make the former inner sequence
         * the GroupJoin outer (left) side:
         *
         *   orders GroupJoin customers  → every order kept (usual left join)
         *   To keep every shipment even without an order — start from shipments
         *
         * Demo: start from shipments, left-join orders — every shipment appears;
         * if an orphan shipment existed, it would still show with a null order.
         * -------------------------------------------------------------------------
         */

        var shipmentsWithOrders =
            from shipment in shipments
            join order in orders on shipment.OrderId equals order.OrderId into orderGroup
            from order in orderGroup.DefaultIfEmpty()
            select new
            {
                shipment.ShipmentId,
                shipment.Carrier,
                OrderId = order != null ? order.OrderId : (int?)null,
                OrderTotal = order != null ? order.Total : (decimal?)null,
            };

        Console.WriteLine();
        Console.WriteLine("--- SECTION 12: Swap sides — every shipment, optional order ---");
        foreach (var row in shipmentsWithOrders.OrderBy(r => r.ShipmentId))
        {
            Console.WriteLine(
                $"  Ship {row.ShipmentId}  {row.Carrier,-6}  Order {row.OrderId}  {row.OrderTotal:C}");
        }


        /*
         * =========================================================================
         * SECTION 13: CROSS JOIN PREVIEW — SelectMany (no key) vs TRUE JOIN
         * =========================================================================
         *
         * A CROSS join pairs every element of A with every element of B.
         * SQL: FROM A CROSS JOIN B (no ON clause).
         *
         * In LINQ to Objects the usual pattern is SelectMany with a result selector:
         *
         *   customers.SelectMany(
         *       c => carriers,
         *       (customer, carrier) => new { customer.Name, carrier })
         *
         * Cartesian product — |A| × |B| rows (here 4 × 3 = 12).
         *
         * Contrast with true Join:
         *
         *  Pattern     | Needs key? | Row count driver
         *  ------------|------------|---------------------------------
         *  Join        | Yes        | Number of equal-key matches
         *  Cross       | No         | |outer| × |inner| always
         *
         * Query-syntax cross join is a nested from (no join clause):
         *
         *   from c in customers
         *   from carrier in carriers
         *   select …
         *
         * COVERED IN DETAIL LATER → 08. Projection Operations
         * -------------------------------------------------------------------------
         */

        string[] carriers = ["FedEx", "UPS", "DHL"];

        var shippingMatrix = customers.SelectMany(
            customer => carriers, // collection selector — each customer gets all carriers
            (customer, carrier) => new { customer.Name, customer.Region, carrier });

        var shippingMatrixQuery =
            from customer in customers
            from carrier in carriers // nested from → SelectMany (cross)
            select new { customer.Name, carrier };

        int trueJoinPairs = customers.Join(
            orders,
            c => c.CustomerId,
            o => o.CustomerId,
            (c, o) => 1).Count();

        Console.WriteLine();
        Console.WriteLine("--- SECTION 13: Cross join preview (SelectMany) vs true Join ---");
        Console.WriteLine(
            $"  Cross pair count: {shippingMatrix.Count()} " +
            $"(customers {customers.Length} × carriers {carriers.Length})");
        Console.WriteLine($"  Query nested-from pairs: {shippingMatrixQuery.Count()} (same cartesian)");
        Console.WriteLine($"  True Join customer×order matches: {trueJoinPairs} (key-based, not 4×5)");
        foreach (var pair in shippingMatrix)
        {
            Console.WriteLine($"  {pair.Name,-16}  {pair.Region,-5}  →  {pair.carrier}");
        }


        /*
         * =========================================================================
         * SECTION 14: PATTERN SUMMARY — COUNTS SIDE BY SIDE
         * =========================================================================
         *
         *  Pattern              | Row count driver
         *  ---------------------|--------------------------------------------------
         *  Inner Join           | Only key matches (orders with customers: 5)
         *  Inner + shipment     | Key matches incl. multiples (shipped lines: 5)
         *  GroupJoin            | One per outer (customers: 4)
         *  Left join flatten    | One per match; 1 if inner empty (6 here)
         *  Cross SelectMany     | |outer| × |inner| (12)
         * -------------------------------------------------------------------------
         */

        Console.WriteLine();
        Console.WriteLine("--- SECTION 14: Join pattern summary ---");
        Console.WriteLine($"  Inner orders+customers     : {ordersWithCustomersQuery.Count()} rows");
        Console.WriteLine($"  Inner + shipment (lines)  : {fulfillmentQuery.Count()} rows");
        Console.WriteLine($"  GroupJoin (by customer)    : {customersWithOrderGroupsMethod.Count()} groups");
        Console.WriteLine($"  Left join flattened        : {leftJoinMethod.Count()} rows");
        Console.WriteLine($"  Cross join preview         : {shippingMatrix.Count()} pairs");
    }

    /*
     * --- Helpers: seed counts and GroupJoin printing ---
     */

    public static void PrintSeedCounts(Customer[] customers, Order[] orders, Shipment[] shipments)
    {
        Console.WriteLine($"  Customers : {customers.Length}");
        Console.WriteLine($"  Orders    : {orders.Length}");
        Console.WriteLine($"  Shipments : {shipments.Length}");
    }

    public static void PrintCustomerOrderGroups(
        IEnumerable<CustomerOrderGroup> groups,
        string prefix)
    {
        foreach (CustomerOrderGroup group in groups)
        {
            int count = group.Orders.Count();
            Console.Write($"{prefix}{group.Name,-16}  {group.Region,-5}  orders: {count}");
            if (count > 0)
            {
                Console.Write("  [");
                Console.Write(string.Join(", ", group.Orders.Select(o => o.OrderId)));
                Console.Write("]");
            }

            Console.WriteLine();
        }
    }
}

/*
 * =========================================================================
 * QUICK REFERENCE — LINQ JOINS
 * =========================================================================
 *
 * --- Inner join (flat, matched keys only) ---
 *
 *   // Query
 *   from a in outer
 *   join b in inner on a.Key equals b.Key
 *   select …
 *
 *   // Method
 *   outer.Join(inner, a => a.Key, b => b.Key, (a, b) => …)
 *   // optional: IEqualityComparer<TKey> as last overload argument
 *
 * --- Composite keys ---
 *
 *   // Query — anonymous type on both sides of equals
 *   on new { a.Sku, a.Wh } equals new { b.Sku, b.Wh }
 *
 *   // Method — value tuple (or anonymous type) as TKey
 *   outer.Join(inner, a => (a.Sku, a.Wh), b => (b.Sku, b.Wh), (a, b) => …)
 *
 * --- GroupJoin (outer + IEnumerable of inner matches) ---
 *
 *   from a in outer
 *   join b in inner on a.Key equals b.Key into grp
 *   select new { a, Matches = grp };
 *
 *   outer.GroupJoin(inner, a => a.Key, b => b.Key, (a, grp) => …)
 *
 * --- Left outer join (keep unmatched outer rows) ---
 *
 *   from a in outer
 *   join b in inner on a.Key equals b.Key into grp
 *   from b in grp.DefaultIfEmpty()
 *   select new { a, Id = b?.Id };
 *
 *   outer.GroupJoin(…)
 *     .SelectMany(x => x.grp.DefaultIfEmpty(), (x, b) => …)
 *
 * --- Right-outer shape ---
 *
 *   Swap which sequence is the GroupJoin outer (left) side.
 *
 * --- Cross join preview (no key — ch.08 SelectMany) ---
 *
 *   outer.SelectMany(o => inner, (o, i) => new { o, i })
 *   from o in outer from i in inner select …   // nested from
 *
 * --- Join vs GroupJoin vs left vs cross ---
 *
 *  Pattern    | Unmatched outer | Result shape              | Typical Count
 *  -----------|-----------------|---------------------------|----------------
 *  Join       | Dropped         | Flat pair                 | # matches
 *  GroupJoin  | Kept (empty)    | Nested IEnumerable        | |outer|
 *  Left join  | Kept (null inn) | Flat; nullable inner      | ≥ |outer|
 *  Cross      | N/A             | Cartesian pairs           | |A| × |B|
 *
 * --- Common mistakes ---
 *
 *  Mistake                          | Result
 *  ---------------------------------|----------------------------------------
 *  Using == instead of equals       | CS syntax error in query expression
 *  Key type mismatch                | CS1943 — keys must be same type
 *  Outer key on wrong side of equals| CS1941 / ill-formed join
 *  Expecting Join to keep all outer | Use GroupJoin + DefaultIfEmpty
 *  Expecting one row per outer      | Join multiplies on one-to-many matches
 *  Null inner after left join       | Use b?.Property; do not dereference
 *  Value-type DefaultIfEmpty        | default(T) not null — prefer class/record
 *  Confusing GroupJoin with GroupBy | GroupBy = one sequence; GroupJoin = two
 *  join into vs group by into       | Same keyword; different translations
 *
 * --- Related chapters ---
 *
 *   04. Grouping              — GroupBy vs GroupJoin
 *   08. Projection Operations — SelectMany depth, result selectors
 *   12. Generation Operations — DefaultIfEmpty operator details
 *   01. Introduction to LINQ  — deferred execution, query vs method syntax
 *
 * =========================================================================
 */
