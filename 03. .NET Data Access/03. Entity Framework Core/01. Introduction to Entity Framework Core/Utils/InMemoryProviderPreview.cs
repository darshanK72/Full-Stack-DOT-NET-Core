using System;
using System.Collections.Generic;
using System.Linq;
using IntroductionToEntityFrameworkCore.Data;
using IntroductionToEntityFrameworkCore.Models;
using Microsoft.EntityFrameworkCore;

namespace IntroductionToEntityFrameworkCore.Utils;

/*
 * FILE ROLE: Runnable preview of the In-Memory EF Core provider - seed and
 *            query products without SQL Server or LocalDB.
 *
 * SECTIONS IN THIS FILE:
 *   9. In-Memory provider preview - testing and intro demos
 */

public static class InMemoryProviderPreview
{
    /*
     * =========================================================================
     * SECTION 9: IN-MEMORY PROVIDER PREVIEW - TESTING AND INTRO DEMOS
     * =========================================================================
     *
     * Microsoft.EntityFrameworkCore.InMemory stores entities in a process-local
     * dictionary keyed by a database name. It is useful for:
     *
     *   - Unit tests that need DbContext without spinning up SQL Server
     *   - Intro chapters (like this one) that must compile and run anywhere
     *
     * Limitations (why it is PREVIEW only here):
     *
     *   - No real SQL translation - behavior can diverge from SQL Server
     *   - No constraints, indexes, or provider-specific types
     *   - Not a substitute for integration tests against LocalDB or Docker SQL
     *
     * Setup pattern:
     *
     *   var options = new DbContextOptionsBuilder<AppDbContext>()
     *       .UseInMemoryDatabase("UniqueNamePerTest")
     *       .Options;
     *   using var db = new AppDbContext(options);
     *
     * EnsureCreated() creates the in-memory schema; SQL Server chapters use
     * migrations instead -> ch03 Code-First Models & Migrations.
     *
     * COVERED IN DETAIL LATER -> use real SqlServer provider in ch02+ demos;
     * In-Memory remains a testing tool, not production storage.
     * -------------------------------------------------------------------------
     */
    public static IReadOnlyList<Product> RunDemo()
    {
        Console.WriteLine("--- SECTION 9: In-Memory provider preview ---");

        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("EfCoreIntroCh01") // unique name isolates this demo's store
            .Options;

        using AppDbContext db = new AppDbContext(options);

        db.Database.EnsureCreated(); // creates in-memory schema for DbSets

        if (!db.Products.Any()) // seed sample rows once per fresh in-memory database
        {
            db.Products.AddRange(
                new Product { ProductId = 1, ProductName = "Widget A", UnitPrice = 9.99m, StockQuantity = 100 },
                new Product { ProductId = 2, ProductName = "Widget B", UnitPrice = 14.50m, StockQuantity = 40, DiscontinuedDate = new DateTime(2024, 6, 1) },
                new Product { ProductId = 3, ProductName = "Gadget C", UnitPrice = 24.00m, StockQuantity = 15 });
            db.SaveChanges(); // persists tracked inserts to the in-memory store
        }

        List<Product> active = db.Products
            .Where(p => p.DiscontinuedDate == null) // LINQ translated by In-Memory provider (not T-SQL)
            .OrderBy(p => p.ProductId)
            .ToList();

        Console.WriteLine("  Provider: UseInMemoryDatabase (no SQL Server required for this demo).");
        Console.WriteLine("  Query: db.Products.Where(...).OrderBy(...).ToList()");
        Console.WriteLine();

        foreach (Product product in active)
        {
            Console.WriteLine($"  {product}");
        }

        Console.WriteLine($"  Active products: {active.Count} (total in store: {db.Products.Count()})");
        return active;
    }
}
