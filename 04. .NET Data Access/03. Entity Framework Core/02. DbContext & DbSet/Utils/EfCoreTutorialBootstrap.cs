using System;
using System.Linq;
using DbContextAndDbSet.Data;
using DbContextAndDbSet.Models;
using Microsoft.EntityFrameworkCore;

namespace DbContextAndDbSet.Utils;

/*
 * FILE ROLE: Creates the EfCoreTutorial database and seeds sample rows using EnsureCreated.
 *
 * SECTIONS IN THIS FILE:
 *   7. EnsureCreated vs migrations (preview)
 *   7b. Seed demo products idempotently
 */

public static class EfCoreTutorialBootstrap
{
    /*
     * SECTION 7: EnsureCreated VS MIGRATIONS (PREVIEW)
     *
     * EnsureCreated() creates the database and schema from the current model if they do not
     * exist. It does NOT use migrations history and will NOT update an existing schema when
     * the model changes.
     *
     *   Approach          | When to use                         | Covered in
     *   ------------------|-------------------------------------|----------------------------
     *   EnsureCreated     | Throwaway demos, integration tests    | this bootstrap helper
     *   Migrations        | Real apps, evolving schema            | ch.03 Code-First & Migrations
     *
     * Pitfall: Calling EnsureCreated after migrations were applied on the same database can
     * leave you with conflicting schema expectations. Pick one strategy per database.
     */
    public static bool TryEnsureDatabaseReady(DbContextOptions<AppDbContext> options)
    {
        try
        {
            using AppDbContext context = new AppDbContext(options);
            context.Database.EnsureCreated(); // creates DB + tables when missing
            SeedProductsIfEmpty(context);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Could not create or connect to EfCoreTutorial on LocalDB.");
            Console.WriteLine($"  {ex.GetType().Name}: {ex.Message}");
            Console.WriteLine("  Ensure LocalDB is installed: Server=(localdb)\\MSSQLLocalDB");
            return false;
        }
    }

    /*
     * SECTION 7b: SEED DEMO PRODUCTS IDEMPOTENTLY
     *
     * Uses Any() to skip seeding when rows already exist so repeated runs stay stable.
     * Id values are omitted so SQL Server IDENTITY generates keys (setting Id manually on insert
     * fails unless IDENTITY_INSERT is enabled - see ch.05 for explicit key scenarios).
     * SaveChanges persists inserts -> full CRUD depth in ch.05 CRUD Operations & SaveChanges.
     */
    private static void SeedProductsIfEmpty(AppDbContext context)
    {
        if (context.Products.Any())
        {
            return;
        }

        context.Products.AddRange(
            new Product { Name = "Widget A", UnitPrice = 9.99m, StockQuantity = 100 },
            new Product { Name = "Widget B", UnitPrice = 14.50m, StockQuantity = 40 },
            new Product { Name = "Gadget C", UnitPrice = 29.00m, StockQuantity = 15 });

        context.SaveChanges(); // sends INSERT statements for staged entities
    }
}
