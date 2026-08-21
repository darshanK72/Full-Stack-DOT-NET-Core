using System.Collections.Generic;
using System.Linq;
using Dapper;
using DapperMappingAndAdvancedPatterns.Models;
using Microsoft.Data.SqlClient;

namespace DapperMappingAndAdvancedPatterns.Repositories;

/*
 * FILE ROLE:
 *   Multi-mapping, splitOn, and one-to-many lookup dictionary patterns.
 *
 * SECTIONS IN THIS FILE:
 *   3. Multi-mapping — Query<TFirst, TSecond, TReturn> with map func
 *   4. splitOn — where Dapper splits columns between types
 *   5. One-to-many — multi-map + Dictionary lookup idiom
 */

public sealed class MultiMapRepository
{
    private readonly string _connectionString;

    public MultiMapRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    /*
     * SECTION 3: MULTI-MAPPING — Query<TFirst, TSecond, TReturn>
     *
     * Signature:
     *   Query<TFirst, TSecond, TReturn>(sql, map, param, splitOn)
     *
     * | Type param | Role                                              |
     * |------------|---------------------------------------------------|
     * | TFirst     | Left side of JOIN (Order)                         |
     * | TSecond    | Right side of JOIN (Customer)                     |
     * | TReturn    | What map returns — often TFirst with nav filled   |
     *
     * map delegate: (first, second) => { first.Customer = second; return first; }
     *
     * Dapper calls map once per row in the result set.
     * -------------------------------------------------------------------------
     */
    public IReadOnlyList<Order> GetOrdersWithCustomers()
    {
        const string sql = """
            SELECT
                o.OrderId,
                o.OrderDate,
                o.TotalAmount,
                o.CustomerId,
                c.CustomerId,
                c.Name,
                c.Email
            FROM dbo.Orders o
            INNER JOIN dbo.Customers c ON o.CustomerId = c.CustomerId
            ORDER BY o.OrderId;
            """;

        using SqlConnection connection = new SqlConnection(_connectionString);

        IEnumerable<Order> orders = connection.Query<Order, Customer, Order>(
            sql,
            (order, customer) =>
            {
                order.Customer = customer; // attach related entity in map func
                return order;
            },
            splitOn: "Name"); // SECTION 4 — first Customer-only column (Order has no Name)

        return orders.ToList();
    }

    /*
     * SECTION 4: splitOn — WHERE DAPPER SPLITS COLUMNS BETWEEN TYPES
     *
     * splitOn names the first column that belongs to TSecond (comma-separated for 3+ types).
     *
     * Default if omitted: "Id" — often wrong for JOINs; always set explicitly.
     *
     * Pitfall: duplicate column names (CustomerId on both sides) — splitOn hits the FIRST
     * match. Fix: alias the second type's key (c.CustomerId AS CustId) or pick a column
     * unique to TSecond (Name here — Orders have no Name column).
     *
     * Column order in SELECT must match: all TFirst columns, then all TSecond columns.
     * -------------------------------------------------------------------------
     */

    /*
     * SECTION 5: ONE-TO-MANY — MULTI-MAP + LOOKUP DICTIONARY
     *
     * JOIN returns one row per child (OrderLine); Order columns repeat.
     * map runs per row — use Dictionary<int, Order> to collapse duplicates:
     *
     *   if (!lookup.TryGetValue(order.OrderId, out var entry)) { lookup.Add(...); }
     *   entry.Lines.Add(line);
     *
     * Return lookup.Values — not the IEnumerable from Query (duplicate parent refs).
     * -------------------------------------------------------------------------
     */
    public Order? GetOrderWithLines(int orderId)
    {
        const string sql = """
            SELECT
                o.OrderId,
                o.CustomerId,
                o.OrderDate,
                o.TotalAmount,
                ol.OrderLineId,
                ol.OrderId,
                ol.ProductId,
                ol.Quantity,
                ol.LineTotal
            FROM dbo.Orders o
            INNER JOIN dbo.OrderLines ol ON o.OrderId = ol.OrderId
            WHERE o.OrderId = @orderId
            ORDER BY ol.OrderLineId;
            """;

        using SqlConnection connection = new SqlConnection(_connectionString);

        Dictionary<int, Order> lookup = new Dictionary<int, Order>();

        connection.Query<Order, OrderLine, Order>(
            sql,
            (order, line) =>
            {
                if (!lookup.TryGetValue(order.OrderId, out Order? tracked))
                {
                    tracked = order;
                    tracked.Lines = new List<OrderLine>();
                    lookup.Add(order.OrderId, tracked);
                }

                tracked.Lines.Add(line); // accumulate child rows under one parent
                return tracked;
            },
            new { orderId },
            splitOn: "OrderLineId"); // first OrderLine column — start of TSecond

        lookup.TryGetValue(orderId, out Order? result);
        return result;
    }
}
