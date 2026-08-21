using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using IntroductionToDapper.Models;
using Microsoft.Data.SqlClient;

namespace IntroductionToDapper.Utils;

/*
 * FILE ROLE: Demonstrates IDbConnection requirements and Dapper's open-or-auto-open
 *            connection behavior.
 *
 * SECTIONS IN THIS FILE:
 *   5. IDbConnection requirement — SqlConnection, open vs closed
 */

public static class ConnectionBehaviorDemo
{
    private const string ActiveProductsSql = """
        SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
        FROM dbo.Products
        WHERE DiscontinuedDate IS NULL
        ORDER BY ProductId;
        """;

    /*
     * =========================================================================
     * SECTION 5: IDbConnection REQUIREMENT — SqlConnection, OPEN VS CLOSED
     * =========================================================================
     *
     * Dapper extends IDbConnection — not SqlConnection specifically. On SQL Server
     * you use Microsoft.Data.SqlClient.SqlConnection, which implements IDbConnection.
     *
     * Connection state before Query:
     *   • Closed — Dapper opens the connection, runs the query, and leaves it
     *     open when the call completes (same as ADO.NET command behavior).
     *   • Open — Dapper uses the existing open connection (typical in repositories
     *     that receive an open connection from a unit-of-work or transaction scope).
     *
     * Always dispose the connection (using var) so it returns to the pool.
     * ch02 covers async open patterns; ch06 ADO.NET covers pooling in depth.
     * -------------------------------------------------------------------------
     */
    public static void DemonstrateClosedConnectionQuery(string connectionString)
    {
        Console.WriteLine("--- SECTION 5a: Query with closed connection (Dapper opens it) ---");

        using IDbConnection connection = new SqlConnection(connectionString); // IDbConnection reference
        Console.WriteLine($"  State before Query: {connection.State}");       // Closed

        List<Product> products = connection.Query<Product>(ActiveProductsSql).ToList(); // Dapper opens if needed

        Console.WriteLine($"  State after Query:  {connection.State}");       // Open
        Console.WriteLine($"  Rows materialized:  {products.Count}");
    }

    public static void DemonstrateOpenConnectionQuery(string connectionString)
    {
        Console.WriteLine("--- SECTION 5b: Query with caller-opened connection ---");

        using IDbConnection connection = new SqlConnection(connectionString);
        connection.Open(); // explicit open — same pattern as ADO.NET ch02/ch03 demos

        Console.WriteLine($"  State before Query: {connection.State}");       // Open

        List<Product> products = connection.Query<Product>(ActiveProductsSql).ToList();

        Console.WriteLine($"  State after Query:  {connection.State}");       // still Open
        Console.WriteLine($"  Rows materialized:  {products.Count}");
    }
}
