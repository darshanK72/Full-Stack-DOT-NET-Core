using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using RelationshipsAndNavigationProperties.Data;
using RelationshipsAndNavigationProperties.Models;

namespace RelationshipsAndNavigationProperties.Utils;

/*
 * FILE ROLE:
 *   Ensures EfCoreTutorial has relationship demo tables and seed rows for Program.cs demos.
 *
 * SECTIONS IN THIS FILE:
 *   8. EnsureCreated / CreateTables bootstrap for shared LocalDB database
 *   8b. Idempotent seed data for cascade delete demos
 */

public static class RelationshipBootstrap
{
    /*
     * SECTION 8: EnsureCreated / CreateTables BOOTSTRAP
     *
     * EfCoreTutorial may already exist from ch02 DbContext & DbSet or ch03 Code-First.
     * EnsureCreated() only runs when the database file is missing  -  it does not add tables
     * to an existing database. CreateTables() adds tables defined in the current model
     * without dropping existing ones (SQL Server CREATE TABLE for missing tables).
     *
     * Pitfall: EnsureDeleted + EnsureCreated is fine for throwaway tests but wipes all
     * EfCoreTutorial data from other chapters  -  avoid in shared tutorial databases unless
     * you intend to reset everything.
     */
    public static bool TryEnsureSchema(DbContextOptions<RelationshipsDbContext> options)
    {
        try
        {
            using RelationshipsDbContext context = new RelationshipsDbContext(options);

            RelationalDatabaseCreator creator =
                (RelationalDatabaseCreator)context.Database.GetService<IDatabaseCreator>();

            if (!creator.Exists())
            {
                creator.Create(); // create empty EfCoreTutorial database file
            }

            creator.CreateTables(); // create Customers, Orders, CatalogProducts, Tags, ProductTags if missing

            SeedIfEmpty(context);
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
     * SECTION 8b: IDEMPOTENT SEED DATA
     *
     * Seeds one customer with two orders, two products, two tags, and one product-tag link.
     * Cascade delete demo removes the seeded customer and relies on re-seed on next run,
     * or Program.cs inserts fresh rows before delete demos.
     */
    private static void SeedIfEmpty(RelationshipsDbContext context)
    {
        if (context.Customers.Any())
        {
            return;
        }

        Customer customer = new Customer
        {
            Name = "Ada Lovelace",
            Email = "ada@example.com",
            Orders =
            {
                new Order { OrderDate = new DateOnly(2024, 3, 1), TotalAmount = 120.00m },
                new Order { OrderDate = new DateOnly(2024, 4, 15), TotalAmount = 45.50m },
            },
        };

        Tag saleTag = new Tag { Label = "Sale" };
        Tag featuredTag = new Tag { Label = "Featured" };

        Product widget = new Product { Name = "Widget", UnitPrice = 9.99m };
        Product gadget = new Product
        {
            Name = "Gadget",
            UnitPrice = 24.50m,
            ProductTags =
            {
                new ProductTag { Tag = saleTag, AddedOn = new DateOnly(2024, 1, 10) },
                new ProductTag { Tag = featuredTag, AddedOn = new DateOnly(2024, 2, 1) },
            },
        };

        context.Customers.Add(customer);
        context.Products.AddRange(widget, gadget);
        context.Tags.Add(saleTag); // featuredTag already tracked via ProductTags graph
        context.SaveChanges();
    }
}
