using System;
using System.Linq;
using DatabaseFirstAndReverseEngineering.Data;
using DatabaseFirstAndReverseEngineering.Models;
using DatabaseFirstAndReverseEngineering.Utils;
using Microsoft.EntityFrameworkCore;

namespace DatabaseFirstAndReverseEngineering;

/*
 * TOPIC: Database-First and Reverse Engineering - generate EF Core models from an existing database.
 *
 * WHY IT MATTERS:
 *   Enterprise apps often integrate with SQL Server schemas owned by DBAs or legacy systems.
 *   Reverse engineering (scaffolding) produces DbContext and entity classes from live metadata
 *   so you can query and update legacy data with LINQ instead of hand-written ADO.NET mapping.
 *
 * PREREQUISITE:
 *   EF Core ch.01-02 (ORM overview, DbContext/DbSet) and MS SQL Server basics.
 *   Code-First migrations are ch.03 - this chapter is the opposite direction.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Database-First mindset - existing DB as source of truth
 *   2.  Sample schema DDL on LocalDB
 *   3.  dotnet ef dbcontext scaffold command and key flags
 *   4.  Generated DbContext and fluent mapping
 *   5.  Generated entity classes (Category, Product)
 *   6.  Partial classes - custom code that survives re-scaffold
 *   7.  Re-scaffold workflow when the database changes
 *   8.  PREVIEW: Scaffold-DbContext in Package Manager Console
 *   9.  PREVIEW: --table / --schema filtering
 *  10.  Demonstration - query scaffolded model + partial helpers
 *
 * CHAPTER MAP:
 *   1.  Database-First mindset           -> Data/InventoryBootstrap.cs
 *   2.  Sample schema DDL              -> Data/InventoryBootstrap.cs
 *   3.  dotnet ef dbcontext scaffold   -> Program.cs (section comment below)
 *   4.  Generated Category entity      -> Models/Category.cs
 *   5.  Generated DbContext            -> Data/InventoryDbContext.cs
 *   6.  Generated Product entity       -> Models/Product.cs
 *   7.  Partial classes                -> Models/Category.Partial.cs, Models/Product.Partial.cs
 *   8.  Re-scaffold workflow           -> Utils/ScaffoldWorkflow.cs
 *   9.  PMC Scaffold-DbContext preview -> Utils/ScaffoldWorkflow.cs
 *  10.  --table / --schema preview     -> Utils/ScaffoldWorkflow.cs
 *       Connection string              -> Utils/ConnectionHelper.cs
 *  11.  Demonstration                 -> Program.cs Main (below)
 *
 * Connection string:
 *   Server=(localdb)\MSSQLLocalDB;Database=EfScaffoldTutorial;Integrated Security=true;TrustServerCertificate=true
 */

/*
 * SECTION 3: DOTNET EF DBCONTEXT SCAFFOLD COMMAND
 *
 * Prerequisites (.csproj):
 *   Microsoft.EntityFrameworkCore.Design (PrivateAssets) - design-time package
 *   Microsoft.EntityFrameworkCore.SqlServer - provider passed to scaffold
 *
 * Install global tool once:
 *   dotnet tool install --global dotnet-ef
 *
 * From the project directory (where .csproj lives):
 *
 *   dotnet ef dbcontext scaffold "Server=(localdb)\MSSQLLocalDB;Database=EfScaffoldTutorial;Integrated Security=true;TrustServerCertificate=true" Microsoft.EntityFrameworkCore.SqlServer ^
 *       --output-dir Models ^
 *       --context-dir Data ^
 *       --context InventoryDbContext ^
 *       --namespace DatabaseFirstAndReverseEngineering.Models ^
 *       --context-namespace DatabaseFirstAndReverseEngineering.Data ^
 *       --force
 *
 * Common flags:
 *
 * | Flag                  | Purpose                                           |
 * |-----------------------|---------------------------------------------------|
 * | --output-dir Models   | Entity .cs files folder                           |
 * | --context-dir Data    | DbContext .cs folder                              |
 * | --context Name        | DbContext class name                              |
 * | --namespace           | Namespace for entities                            |
 * | --context-namespace   | Namespace for DbContext                           |
 * | --force               | Overwrite existing generated files                |
 * | --no-onconfiguring    | Omit OnConfiguring (use DI / options)             |
 * | --no-pluralize        | DbSet property names match table names            |
 * | --table dbo.X         | Include only listed tables (repeat flag)          |
 * | --schema dbo          | Include only objects in listed schemas            |
 *
 * This repository hand-wrote Models/ and Data/ to match scaffold output so the chapter
 * builds without running the tool in CI. Run the command locally against EfScaffoldTutorial
 * to compare generated files with the tutorial copies.
 * -------------------------------------------------------------------------
 */

public class Program
{
    /*
     * SECTION 11: DEMONSTRATION - Main orchestrates the chapter demo
     *
     * Bootstraps LocalDB schema, prints scaffold command reference, then queries via
     * scaffolded InventoryDbContext and partial-class helpers on Category/Product.
     * -------------------------------------------------------------------------
     */
    public static void Main(string[] args)
    {
        Console.WriteLine("=== 04. Database-First and Reverse Engineering (EF Core) ===");
        Console.WriteLine();

        PrintScaffoldCommandReference();

        Console.WriteLine("--- SECTION 8: Re-scaffold workflow ---");
        ScaffoldWorkflow.PrintReScaffoldChecklist();
        Console.WriteLine($"  Command: {ScaffoldWorkflow.SampleReScaffoldCommand}");
        Console.WriteLine();

        Console.WriteLine("--- SECTIONS 9-10: PMC and filter previews ---");
        ScaffoldFilterPreview.PrintFilterHints();
        Console.WriteLine("  PMC: Scaffold-DbContext ... -Force (see Utils/ScaffoldWorkflow.cs)");
        Console.WriteLine();

        bool databaseReady = InventoryBootstrap.TryEnsureSchema();
        if (!databaseReady)
        {
            Console.WriteLine("Database demos skipped - start LocalDB and re-run.");
            Console.WriteLine("  DDL documented in Data/InventoryBootstrap.cs");
            return;
        }

        Console.WriteLine("EfScaffoldTutorial ready on LocalDB.");
        Console.WriteLine();

        DemonstrateScaffoldedQueries();
        DemonstratePartialClassExtensions();
    }

    private static void PrintScaffoldCommandReference()
    {
        Console.WriteLine("--- SECTION 3: dotnet ef dbcontext scaffold ---");
        Console.WriteLine("  dotnet ef dbcontext scaffold \"<connection-string>\" Microsoft.EntityFrameworkCore.SqlServer \\");
        Console.WriteLine("      --output-dir Models --context-dir Data --context InventoryDbContext \\");
        Console.WriteLine("      --namespace DatabaseFirstAndReverseEngineering.Models \\");
        Console.WriteLine("      --context-namespace DatabaseFirstAndReverseEngineering.Data --force");
        Console.WriteLine("  Requires: dotnet-ef global tool + Microsoft.EntityFrameworkCore.Design in .csproj");
        Console.WriteLine();
    }

    private static void DemonstrateScaffoldedQueries()
    {
        Console.WriteLine("--- SECTIONS 4-6: Query via scaffolded DbContext ---");

        using InventoryDbContext context = new InventoryDbContext();

        var categoriesWithProducts = context.Categories
            .Include(c => c.Products)
            .OrderBy(c => c.Name)
            .ToList();

        foreach (Category category in categoriesWithProducts)
        {
            Console.WriteLine($"  {category.Name} ({category.Products.Count} product(s))");
            foreach (Product product in category.Products.OrderBy(p => p.ProductName))
            {
                Console.WriteLine($"    {product.ProductId}: {product.ProductName} @ ${product.UnitPrice:F2} x {product.StockQuantity}");
            }
        }

        Console.WriteLine();
    }

    private static void DemonstratePartialClassExtensions()
    {
        Console.WriteLine("--- SECTION 7: Partial class helpers (survive re-scaffold) ---");

        using InventoryDbContext context = new InventoryDbContext();

        Product? sample = context.Products
            .Include(p => p.Category)
            .OrderBy(p => p.ProductId)
            .FirstOrDefault();

        if (sample is null)
        {
            Console.WriteLine("  No products found.");
            return;
        }

        Category category = sample.Category;
        Console.WriteLine($"  Product: {sample.ProductName} - {sample.StockStatus}, inventory value ${sample.InventoryValue:F2}");
        Console.WriteLine($"  Category partial: {category.DisplayLabel}, ProductCount={category.ProductCount}");
        Console.WriteLine();
    }
}

/*
 * QUICK REFERENCE - DATABASE-FIRST AND REVERSE ENGINEERING
 *
 * --- Mindset ---
 *
 *   SQL Server schema = source of truth; C# model is generated, not migrated from code.
 *
 * --- Scaffold (CLI) ---
 *
 *   dotnet ef dbcontext scaffold "<conn>" Microsoft.EntityFrameworkCore.SqlServer \
 *       --output-dir Models --context-dir Data --context MyDbContext --force
 *
 * --- PMC (preview) ---
 *
 *   Scaffold-DbContext "<conn>" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Force
 *
 * --- Filtering (preview) ---
 *
 *   --table dbo.Products --table dbo.Categories
 *   --schema dbo
 *
 * --- Partial classes ---
 *
 *   Generated:  Models/Product.cs        (overwrite on scaffold)
 *   Yours:      Models/Product.Partial.cs (never touched by scaffold)
 *
 * --- Re-scaffold ---
 *
 *   1. Apply SQL changes  2. dotnet ef dbcontext scaffold ... --force  3. dotnet build
 *
 * --- Next ---
 *
 *   CRUD with scaffolded context -> ch.05 CRUD Operations and SaveChanges
 *   Relationship tuning -> ch.06, fluent overrides -> ch.07
 */
