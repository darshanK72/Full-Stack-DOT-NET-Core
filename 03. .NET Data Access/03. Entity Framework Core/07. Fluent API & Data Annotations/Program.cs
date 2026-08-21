using System;
using System.Linq;
using FluentApiAndDataAnnotations.Data;
using FluentApiAndDataAnnotations.Models;
using FluentApiAndDataAnnotations.Utils;
using Microsoft.EntityFrameworkCore;

namespace FluentApiAndDataAnnotations;

/*
 * TOPIC: Fluent API & Data Annotations - two complementary ways to configure how
 *        C# entity classes map to tables, columns, indexes, and constraints.
 *
 * WHY IT MATTERS:
 *   Conventions (ch03) get you started, but real schemas need explicit rules:
 *   required columns, string lengths, unique business keys, and value objects
 *   without their own tables. Annotations keep simple rules on the entity; Fluent
 *   API in OnModelCreating handles indexes, owned types, and keeps POCOs clean.
 *
 * PREREQUISITE:
 *   EF Core ch03 (Code-First entities, DbContext) and ch02 (DbContext options).
 *
 * WHAT YOU WILL LEARN:
 *   1.  [Required] - NOT NULL columns and SaveChanges validation
 *   2.  [MaxLength] and [StringLength] - NVARCHAR(n) and length validation
 *   3.  When to use annotations vs Fluent API on the same model
 *   4.  OnModelCreating and entity.Property(...).IsRequired().HasMaxLength(...)
 *   5.  HasIndex and IsUnique - database indexes and uniqueness
 *   6.  Owned types preview - OwnsOne for Address on Supplier
 *   7.  LocalDB connection for EfCoreTutorial
 *   8.  Runnable demo - validation, seed data, unique index violation
 *
 * CHAPTER MAP:
 *   1.  [Required]                         -> Models/CatalogProduct.cs
 *   2.  [MaxLength] / [StringLength]       -> Models/CatalogProduct.cs
 *   3.  POCO vs annotations                -> Models/Supplier.cs
 *   4.  OnModelCreating / Fluent API       -> Data/CatalogDbContext.cs
 *   5.  Indexes (HasIndex, IsUnique)       -> Data/CatalogDbContext.cs
 *   6.  Owned types preview                -> Models/Address.cs
 *                                       -> Data/CatalogDbContext.cs (OwnsOne)
 *   7.  LocalDB connection string          -> Utils/ConnectionHelper.cs
 *   8b. Bootstrap (schema + seed)          -> Utils/CatalogBootstrap.cs
 *   8.  Demonstration                      -> Program.cs Main (below)
 *
 * Connection string:
 *   Server=(localdb)\MSSQLLocalDB;Database=EfCoreTutorial;Integrated Security=true;TrustServerCertificate=true
 */

public class Program
{
    /*
     * SECTION 8: DEMONSTRATION - Main orchestrates the chapter demo
     *
     * Builds DbContextOptions, ensures schema on LocalDB, seeds rows, then
     * exercises annotation validation, owned-type materialization, and a
     * unique-index violation. Offline sections still print when LocalDB is down.
     * -------------------------------------------------------------------------
     */
    public static void Main(string[] args)
    {
        Console.WriteLine("=== 07. Fluent API & Data Annotations (EF Core) ===");
        Console.WriteLine();

        DbContextOptions<CatalogDbContext> options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseSqlServer(ConnectionHelper.LocalDbConnectionString)
            .Options;

        if (!CatalogBootstrap.TryEnsureReady(options, out string? failureReason))
        {
            Console.WriteLine("Database demos skipped - LocalDB unavailable.");
            Console.WriteLine($"  Reason: {failureReason ?? "Unknown"}");
            Console.WriteLine("  Install/start (localdb)\\MSSQLLocalDB, then re-run.");
            Console.WriteLine();
            PrintOfflineSummary();
            return;
        }

        using CatalogDbContext context = new CatalogDbContext(options);
        Console.WriteLine("EfCoreTutorial ready on LocalDB (EnsureCreated applied).");
        Console.WriteLine();

        PrintSeedSummary(context);
        DemonstrateAnnotationValidation(context);
        DemonstrateOwnedTypeQuery(context);
        DemonstrateUniqueIndexViolation(context);
    }

    private static void PrintSeedSummary(CatalogDbContext context)
    {
        Console.WriteLine("--- Seed data (CatalogBootstrap) ---");
        PrintCatalogProducts(context);
        PrintSuppliers(context);
        Console.WriteLine();
    }

    private static void PrintCatalogProducts(CatalogDbContext context)
    {
        Console.WriteLine("  CatalogProducts:");
        foreach (CatalogProduct product in context.CatalogProducts.AsNoTracking().OrderBy(p => p.CatalogProductId))
        {
            Console.WriteLine($"    {product}");
        }
    }

    private static void DemonstrateAnnotationValidation(CatalogDbContext context)
    {
        Console.WriteLine("--- SECTION 1-2: [Required] / [StringLength] at SaveChanges ---");
        Console.WriteLine("  Sku exceeds [StringLength(20)] - SQL Server rejects INSERT (column max length).");
        Console.WriteLine("  For Name, use [Required(AllowEmptyStrings = false)] - default [Required] allows \"\".");

        CatalogProduct invalid = new CatalogProduct
        {
            Name = "",                              // [Required(AllowEmptyStrings = false)] fails
            Sku = new string('X', 25),              // [StringLength(20)] fails - exceeds max
            UnitPrice = 9.99m
        };

        context.CatalogProducts.Add(invalid);

        try
        {
            context.SaveChanges();
            Console.WriteLine("  Unexpected: SaveChanges succeeded for invalid product.");
        }
        catch (DbUpdateException ex) when (ex.InnerException is not null)
        {
            Console.WriteLine("  SaveChanges rejected invalid entity (expected):");
            Console.WriteLine($"    {ex.InnerException.Message}");
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine("  SaveChanges rejected invalid entity (expected):");
            Console.WriteLine($"    {ex.Message}");
        }

        context.Entry(invalid).State = EntityState.Detached; // drop invalid tracked instance
        Console.WriteLine("  Fix: set Name and Sku within length rules before SaveChanges.");
        Console.WriteLine();
    }

    private static void DemonstrateOwnedTypeQuery(CatalogDbContext context)
    {
        Console.WriteLine("--- SECTION 6: Owned type - Address columns on Suppliers row ---");

        Supplier? supplier = context.Suppliers
            .AsNoTracking()
            .FirstOrDefault(s => s.SupplierCode == "ACME");

        if (supplier is null)
        {
            Console.WriteLine("  Supplier ACME not found.");
            Console.WriteLine();
            return;
        }

        Console.WriteLine($"  Loaded: {supplier}");
        Console.WriteLine("  Address is not a separate DbSet - materialized with Supplier via OwnsOne.");
        Console.WriteLine();
    }

    private static void DemonstrateUniqueIndexViolation(CatalogDbContext context)
    {
        Console.WriteLine("--- SECTION 5: Unique index on SupplierCode ---");

        Supplier duplicate = new Supplier
        {
            CompanyName = "Acme Duplicate",
            SupplierCode = "ACME", // same code as seeded row - IX unique violation
            Address = new Address { Street = "200 Copy Lane", City = "Portland", PostalCode = "97202" }
        };

        context.Suppliers.Add(duplicate);

        try
        {
            context.SaveChanges();
            Console.WriteLine("  Unexpected: duplicate SupplierCode was allowed.");
        }
        catch (DbUpdateException ex) when (ex.InnerException is not null)
        {
            Console.WriteLine("  SaveChanges rejected duplicate SupplierCode (expected):");
            Console.WriteLine($"    {ex.InnerException.Message}");
        }

        context.Entry(duplicate).State = EntityState.Detached;
        Console.WriteLine("  HasIndex(e => e.SupplierCode).IsUnique() enforces this at the database.");
        Console.WriteLine();
    }

    private static void PrintSuppliers(CatalogDbContext context)
    {
        Console.WriteLine("  Suppliers:");
        foreach (Supplier supplier in context.Suppliers.AsNoTracking().OrderBy(s => s.SupplierId))
        {
            Console.WriteLine($"    {supplier}");
        }
    }

    private static void PrintOfflineSummary()
    {
        Console.WriteLine("Offline reading path - open files in CHAPTER MAP order:");
        Console.WriteLine("  CatalogProduct.cs  -> [Required], [MaxLength], [StringLength]");
        Console.WriteLine("  Supplier.cs        -> POCO configured via Fluent API");
        Console.WriteLine("  CatalogDbContext.cs -> OnModelCreating, HasIndex, OwnsOne");
        Console.WriteLine("  Address.cs         -> owned type preview");
    }
}

/*
 * QUICK REFERENCE - FLUENT API & DATA ANNOTATIONS
 *
 * --- Data annotations (System.ComponentModel.DataAnnotations) ---
 *
 *   [Required]                    NOT NULL + validation
 *   [MaxLength(100)]              column max length (strings, byte[])
 *   [StringLength(500, MinimumLength = 1)]  string validation + max length
 *   [Column("ProductName")]       column rename (Fluent: HasColumnName)
 *   [Table("Products")]           table rename (Fluent: ToTable)
 *   [Key] / [NotMapped]           key override / exclude property
 *   [Index(nameof(Code), IsUnique = true)]  simple index (Fluent preferred for composite)
 *
 * --- Fluent API (OnModelCreating) ---
 *
 *   modelBuilder.Entity<Product>(entity =>
 *   {
 *       entity.ToTable("Products");
 *       entity.HasKey(e => e.ProductId);
 *       entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
 *       entity.HasIndex(e => e.Sku).IsUnique();
 *       entity.OwnsOne(e => e.Address, a => { a.Property(x => x.City).HasMaxLength(100); });
 *   });
 *
 * --- OnModelCreating ---
 *
 *   protected override void OnModelCreating(ModelBuilder modelBuilder)
 *   {
 *       modelBuilder.Entity<Supplier>(ConfigureSupplier);
 *   }
 *
 * --- Indexes ---
 *
 *   entity.HasIndex(e => e.Email);
 *   entity.HasIndex(e => new { e.TenantId, e.Email }).IsUnique();
 *
 * --- Owned types (preview) ---
 *
 *   entity.OwnsOne(e => e.Address);
 *   // columns on parent table; no DbSet<Address>
 *
 * --- Annotations vs Fluent ---
 *
 *   Simple per-property rules     -> annotations OK on entity
 *   Indexes, owned types, complex -> Fluent API in DbContext
 *   Scaffolded entities (ch04)      -> prefer Fluent or partial classes
 */
