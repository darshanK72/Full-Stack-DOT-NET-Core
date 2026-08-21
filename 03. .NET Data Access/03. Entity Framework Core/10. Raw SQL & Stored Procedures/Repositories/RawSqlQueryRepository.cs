using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RawSqlAndStoredProcedures.Data;
using RawSqlAndStoredProcedures.Models;

namespace RawSqlAndStoredProcedures.Repositories;

/*
 * FILE ROLE: DbSet.FromSqlRaw and FromSqlInterpolated — raw SELECT SQL mapped to Product entities.
 *
 * SECTIONS IN THIS FILE:
 *   6. FromSqlRaw and FromSqlInterpolated
 */

/*
 * SECTION 6: FromSqlRaw AND FromSqlInterpolated
 *
 * When LINQ cannot express the query (window functions, vendor hints, legacy SQL), EF Core
 * lets you supply SQL that materializes mapped entities via DbSet<T>.FromSqlRaw.
 *
 * ADO.NET equivalent (ch.04 SqlDataReader):
 *   cmd.CommandText = "SELECT ... WHERE UnitPrice > @min";
 *   reader while-loop -> new Product { ... }
 *
 * Dapper equivalent (ch.01):
 *   connection.Query<Product>("SELECT ... WHERE UnitPrice > @min", new { min = 10m });
 *
 * EF Core adds change tracking on returned entities (default). Use AsNoTracking() when you
 * only read — COVERED IN DETAIL LATER → 11. Change Tracking, Async & Transactions.
 *
 * --- Parameter styles ---
 *
 *   | Method                 | Parameters              | SQL injection safe?
 *   |------------------------|-------------------------|-------------------
 *   | FromSqlRaw             | {0}, {1} placeholders   | Yes — values sent as params
 *   | FromSqlRaw             | string concat           | NO — never concatenate user input
 *   | FromSqlInterpolated    | FormattableString       | Yes — interpolated values become params
 *
 * Rules for entity queries:
 *   • SQL must be a query (SELECT) that returns columns matching the entity.
 *   • You can append LINQ after FromSqlRaw (Where, OrderBy) — EF composes SQL.
 *   • First column of SELECT should be unique key columns when tracking is on.
 *
 * Pitfall: FromSqlRaw("SELECT * FROM Products") works only if * matches mapped columns.
 * Pitfall: Stored procedures use EXEC form — see StoredProcedureRepository (SECTION 8).
 */
public sealed class RawSqlQueryRepository
{
    private readonly EfCoreTutorialDbContext _context;

    public RawSqlQueryRepository(EfCoreTutorialDbContext context)
    {
        _context = context;
    }

    public List<Product> GetActiveProductsAbovePrice(decimal minimumPrice)
    {
        // FromSqlRaw — positional {0} becomes a parameterized @p0 (not string concatenation)
        return _context.Products
            .FromSqlRaw(
                """
                SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
                FROM dbo.Products
                WHERE DiscontinuedDate IS NULL
                  AND UnitPrice > {0}
                """,
                minimumPrice)
            .OrderBy(p => p.ProductName) // composable — EF appends ORDER BY to subquery
            .AsNoTracking()              // read-only demo — skip change tracker overhead
            .ToList();
    }

    public List<Product> GetProductsByNamePrefix(string prefix)
    {
        // FromSqlInterpolated — C# interpolation is syntactic sugar for parameterized SQL
        return _context.Products
            .FromSqlInterpolated($"""
                SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
                FROM dbo.Products
                WHERE ProductName LIKE {prefix + "%"}
                ORDER BY ProductId
                """)
            .AsNoTracking()
            .ToList();
    }

    public int CountActiveProducts()
    {
        // Contrast: LINQ when raw SQL is not required (ch.08)
        return _context.Products.Count(p => p.DiscontinuedDate == null);
    }
}
