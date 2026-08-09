using System.Collections.Generic;
using System.Linq;
using LinqToEntitiesAndQueryPatterns.Data;
using LinqToEntitiesAndQueryPatterns.Models;
using Microsoft.EntityFrameworkCore;

namespace LinqToEntitiesAndQueryPatterns.Services;

/*
 * FILE ROLE:
 *   Filtering patterns with LINQ to Entities  -  Where, Any, Contains, and composable filters.
 *
 * SECTIONS IN THIS FILE:
 *   1. Predicate filtering on scalar columns
 *   2. Filtering with Any on related collections
 *   3. Composable IQueryable filters (dynamic search pattern)
 *   4. Global query filter vs IgnoreQueryFilters (preview)
 */

public sealed class FilteringQueries
{
    private readonly StoreDbContext _context;

    public FilteringQueries(StoreDbContext context)
    {
        _context = context;
    }

    /*
     * SECTION 1: PREDICATE FILTERING ON SCALAR COLUMNS
     *
     * Where translates to SQL WHERE. Parameters become SqlParameters (safe from injection).
     *
     * PREREQUISITE: LINQ ch.02 Filtering & Aggregation  -  Where, comparison operators.
     * -------------------------------------------------------------------------
     */
    public IReadOnlyList<Product> GetInStockProducts(int minimumStock)
    {
        return _context.Products
            .Where(p => p.StockQuantity >= minimumStock)
            .OrderBy(p => p.Name)
            .ToList(); // terminal  -  runs SELECT ... WHERE StockQuantity >= @min
    }

    /*
     * SECTION 2: FILTERING WITH Any ON RELATED COLLECTIONS
     *
     * Any on navigation translates to EXISTS subquery in SQL (no need to load all lines first).
     * Include/ThenInclude for eager loading is ch.09  -  here we only filter parent rows.
     * -------------------------------------------------------------------------
     */
    public IReadOnlyList<Order> GetOrdersContainingProductNamed(string productNameFragment)
    {
        return _context.Orders
            .Where(o => o.Lines.Any(l => l.Product!.Name.Contains(productNameFragment)))
            .OrderBy(o => o.OrderDate)
            .ToList();
    }

    /*
     * SECTION 3: COMPOSABLE IQueryable FILTERS (DYNAMIC SEARCH PATTERN)
     *
     * Build IQueryable step-by-step; only append Where when the caller supplied a criterion.
     * One translated SQL statement with all predicates  -  not multiple round-trips.
     *
     * Pattern used in repository/service layers before passing to pagination (Take/Skip).
     * -------------------------------------------------------------------------
     */
    public IReadOnlyList<Product> SearchProducts(int? categoryId, decimal? minPrice, decimal? maxPrice)
    {
        IQueryable<Product> query = _context.Products;

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(p => p.UnitPrice >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.UnitPrice <= maxPrice.Value);
        }

        return query.OrderBy(p => p.UnitPrice).ToList();
    }

    /*
     * SECTION 4: GLOBAL QUERY FILTER VS IgnoreQueryFilters (PREVIEW)
     *
     * StoreDbContext applies HasQueryFilter(p => !p.IsDiscontinued) on Product.
     * Default queries hide ProductId 4 and 7 (seed data marked discontinued).
     *
     * IgnoreQueryFilters() opts out for admin/report scenarios  -  use deliberately.
     * COVERED IN DETAIL LATER  -  filter interaction with required relationships, owned types.
     * -------------------------------------------------------------------------
     */
    public GlobalFilterComparison CompareGlobalFilter()
    {
        int filteredCount = _context.Products.Count(); // excludes discontinued (global filter)
        int allRowsIncludingDiscontinued = _context.Products.IgnoreQueryFilters().Count();

        return new GlobalFilterComparison(filteredCount, allRowsIncludingDiscontinued);
    }
}

public sealed class GlobalFilterComparison
{
    public GlobalFilterComparison(int activeProductCount, int totalIncludingDiscontinued)
    {
        ActiveProductCount = activeProductCount;
        TotalIncludingDiscontinued = totalIncludingDiscontinued;
    }

    public int ActiveProductCount { get; }
    public int TotalIncludingDiscontinued { get; }
}
