using System;
using System.Collections.Generic;
using System.Linq;
using LinqToEntitiesAndQueryPatterns.Data;
using LinqToEntitiesAndQueryPatterns.Models;
using Microsoft.EntityFrameworkCore;

namespace LinqToEntitiesAndQueryPatterns.Services;

/*
 * FILE ROLE:
 *   Demonstrates IQueryable from DbSet and deferred execution against SQL Server.
 *
 * SECTIONS IN THIS FILE:
 *   1. IQueryable from DbSet  -  provider translation preview
 *   2. Deferred execution  -  when SQL actually runs
 */

public sealed class IQueryableDemonstrations
{
    private readonly StoreDbContext _context;

    public IQueryableDemonstrations(StoreDbContext context)
    {
        _context = context;
    }

    /*
     * SECTION 1: IQueryable FROM DbSet  -  PROVIDER TRANSLATION PREVIEW
     *
     * DbSet<Product> is IQueryable<Product>. Each LINQ operator (Where, OrderBy, Select)
     * builds an expression tree; EF Core translates it to SQL at execution time.
     *
     * Compare LINQ to Objects (IEnumerable in memory from ch.01) vs LINQ to Entities here:
     *   - Same method names (Where, Select, ...)  -  different provider underneath
     *   - Not every C# lambda translates  -  client evaluation pitfalls (see quick reference)
     *
     * ToQueryString() prints the SQL EF Core would run  -  use during learning, not in production hot paths.
     * -------------------------------------------------------------------------
     */
    public string ShowTranslatedSql(decimal minimumPrice)
    {
        IQueryable<Product> query = _context.Products
            .Where(p => p.UnitPrice >= minimumPrice)
            .OrderBy(p => p.Name);

        return query.ToQueryString(); // no DB round-trip yet  -  only builds SQL text
    }

    /*
     * SECTION 2: DEFERRED EXECUTION  -  WHEN SQL ACTUALLY RUNS
     *
     * Assigning/chaining on IQueryable does NOT hit the database.
     * Execution triggers on terminal operators: ToList, ToArray, Count, First, foreach, etc.
     *
     * Same deferred model as LINQ ch.01  -  difference is execution happens on SQL Server.
     *
     * Pitfall: capturing IQueryable and enumerating after DbContext is disposed throws.
     * Materialize (ToList) while the context is alive, or scope the context per operation.
     * -------------------------------------------------------------------------
     */
    public DeferredExecutionResult DemonstrateDeferredExecution(decimal minimumPrice)
    {
        IQueryable<Product> query = _context.Products
            .Where(p => p.UnitPrice >= minimumPrice)
            .OrderBy(p => p.Name);

        string sqlBeforeExecution = query.ToQueryString();

        List<Product> materialized = query.ToList(); // SQL executes here

        return new DeferredExecutionResult(sqlBeforeExecution, materialized.Count, materialized);
    }
}

public sealed class DeferredExecutionResult
{
    public DeferredExecutionResult(string sqlPreview, int resultCount, IReadOnlyList<Product> products)
    {
        SqlPreview = sqlPreview;
        ResultCount = resultCount;
        Products = products;
    }

    public string SqlPreview { get; }
    public int ResultCount { get; }
    public IReadOnlyList<Product> Products { get; }
}
