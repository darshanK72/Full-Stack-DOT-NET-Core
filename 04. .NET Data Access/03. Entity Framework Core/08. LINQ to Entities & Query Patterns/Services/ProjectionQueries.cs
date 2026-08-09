using System.Collections.Generic;
using System.Linq;
using LinqToEntitiesAndQueryPatterns.Data;
using LinqToEntitiesAndQueryPatterns.Models;
using Microsoft.EntityFrameworkCore;

namespace LinqToEntitiesAndQueryPatterns.Services;

/*
 * FILE ROLE:
 *   Projection queries  -  Select to DTOs, computed fields, and SelectMany flattening.
 *
 * SECTIONS IN THIS FILE:
 *   1. Select to named DTO (server-side column subset)
 *   2. Anonymous type projection for ad-hoc reports
 *   3. SelectMany  -  flatten order lines
 */

public sealed class ProjectionQueries
{
    private readonly StoreDbContext _context;

    public ProjectionQueries(StoreDbContext context)
    {
        _context = context;
    }

    /*
     * SECTION 1: SELECT TO NAMED DTO (SERVER-SIDE COLUMN SUBSET)
     *
     * Select runs in SQL as column list  -  only requested columns cross the wire.
     * Navigation Category.Name becomes JOIN + projected alias (translator handles shape).
     *
     * PREREQUISITE: LINQ ch.08 Projection Operations  -  Select, shaping results, deferred execution.
     * -------------------------------------------------------------------------
     */
    public IReadOnlyList<ProductListItem> GetProductCatalogItems()
    {
        return _context.Products
            .OrderBy(p => p.Category!.Name)
            .ThenBy(p => p.Name)
            .Select(p => new ProductListItem
            {
                ProductId = p.ProductId,
                Name = p.Name,
                UnitPrice = p.UnitPrice,
                CategoryName = p.Category!.Name,
                InStock = p.StockQuantity > 0
            })
            .ToList(); // SELECT p.Id, p.Name, ... c.Name AS CategoryName, CASE WHEN ...
    }

    /*
     * SECTION 2: ANONYMOUS TYPE PROJECTION FOR AD-HOC REPORTS
     *
     * Fine inside a single method; prefer ProductListItem (or record) when returning from services/API.
     * EF Core translates property names in anonymous projections to SQL column aliases.
     * -------------------------------------------------------------------------
     */
    public IReadOnlyList<string> GetCategoryStockSummary()
    {
        var rows = _context.Products
            .GroupBy(p => p.Category!.Name)
            .Select(g => new
            {
                Category = g.Key,
                SkuCount = g.Count(),
                TotalUnits = g.Sum(p => p.StockQuantity)
            })
            .OrderBy(r => r.Category)
            .ToList();

        return rows
            .Select(r => $"{r.Category}: {r.SkuCount} SKU(s), {r.TotalUnits} unit(s)")
            .ToList();
    }

    /*
     * SECTION 3: SelectMany  -  FLATTEN ORDER LINES
     *
     * SelectMany expands nested collection into one row per line  -  SQL JOIN or APPLY pattern.
     * Useful for export/report queries; watch row duplication if you also project parent fields.
     * -------------------------------------------------------------------------
     */
    public IReadOnlyList<string> GetFlatOrderLineDescriptions()
    {
        return _context.Orders
            .OrderBy(o => o.OrderId)
            .SelectMany(o => o.Lines, (order, line) => new
            {
                order.CustomerName,
                order.OrderDate,
                line.ProductId,
                line.Quantity,
                line.LineTotal
            })
            .Select(row => $"{row.CustomerName} ({row.OrderDate:d}): Product {row.ProductId} x{row.Quantity} = ${row.LineTotal:F2}")
            .ToList();
    }
}
