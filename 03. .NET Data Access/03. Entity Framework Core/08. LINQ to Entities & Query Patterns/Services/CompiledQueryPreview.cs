using System;
using System.Collections.Generic;
using System.Linq;
using LinqToEntitiesAndQueryPatterns.Data;
using LinqToEntitiesAndQueryPatterns.Models;
using Microsoft.EntityFrameworkCore;

namespace LinqToEntitiesAndQueryPatterns.Services;

/*
 * FILE ROLE:
 *   Compiled queries preview  -  cache translated expression for repeated execution.
 *
 * SECTIONS IN THIS FILE:
 *   1. EF.CompileQuery  -  sync compiled delegate
 *   2. EF.CompileAsyncQuery  -  async preview (comment-only signature)
 */

public sealed class CompiledQueryPreview
{
    /*
     * SECTION 1: EF.CompileQuery  -  SYNC COMPILED DELEGATE
     *
     * First call compiles the expression tree into a cached delegate; later calls reuse it.
     * Helpful for hot paths with the same query shape and different parameter values.
     *
     * PREVIEW  -  production use also needs DbContext pooling, parameter validation, and profiling.
     * COVERED IN DETAIL LATER  -  performance tuning, compiled models (EF Core 8+), interceptors in ch.11.
     *
     * Note: compiled query must be stored static readonly; DbContext instance passed per invocation.
     * -------------------------------------------------------------------------
     */
    private static readonly Func<StoreDbContext, int, IEnumerable<Product>> ProductsByCategoryQuery =
        EF.CompileQuery((StoreDbContext db, int categoryId) =>
            db.Products.Where(p => p.CategoryId == categoryId));

    public IReadOnlyList<Product> GetProductsByCategory(StoreDbContext context, int categoryId)
    {
        return ProductsByCategoryQuery(context, categoryId)
            .OrderBy(p => p.Name) // applied when enumerated  -  keep compiled shape to Where-only for EF
            .ToList();
    }

    /*
     * SECTION 2: EF.CompileAsyncQuery  -  ASYNC PREVIEW
     *
     * Same caching model with IAsyncEnumerable<T> return for async streaming:
     *
     *   private static readonly Func<StoreDbContext, decimal, IAsyncEnumerable<Product>> HighPriceQuery =
     *       EF.CompileAsyncQuery((StoreDbContext db, decimal min) =>
     *           db.Products.Where(p => p.UnitPrice >= min).OrderBy(p => p.Name));
     *
     *   await foreach (Product p in HighPriceQuery(context, 50m)) { ... }
     *
     * Async SaveChanges and transactions  -  EF Core ch.11.
     * -------------------------------------------------------------------------
     */
    public string AsyncCompiledQueryNote =>
        "Use EF.CompileAsyncQuery for repeated async enumeration; see SECTION 2 comment above.";
}
