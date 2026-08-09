using CodeFirstModelsAndMigrations.Models;
using CodeFirstModelsAndMigrations.Utils;
using Microsoft.EntityFrameworkCore;

namespace CodeFirstModelsAndMigrations.Data;

/*
 * FILE ROLE: DbContext that exposes entity sets and connects the model to SQL Server.
 *
 * SECTIONS IN THIS FILE:
 *   3. DbContext and DbSet<T> for Code-First
 */

/*
 * =========================================================================
 * SECTION 3: DbContext AND DbSet<T> FOR CODE-FIRST
 * =========================================================================
 *
 * DbContext is the session with the database:
 *   - tracks entity instances (change tracking preview -> ch11)
 *   - translates LINQ to SQL (preview -> ch08)
 *   - applies migrations through Database.Migrate() / EnsureCreated()
 *
 * DbSet<T> represents a table (collection) for entity type T:
 *
 *   DbSet<Category> Categories  ->  dbo.Categories
 *   DbSet<Product> Products     ->  dbo.Products
 *
 * Constructor pattern:
 *   - DbContextOptions<StoreDbContext> from DI or built in Main (this demo)
 *   - OnConfiguring fallback for dotnet ef design-time when options not pre-built
 *
 * DbContext lifetime: create per unit of work, dispose promptly (using statement).
 * ASP.NET Core registers it scoped - deferred to Web API modules.
 *
 * PREVIEW from ch02 DbContext & DbSet: options builder, pooling, and DI registration
 * are expanded there. This chapter focuses on the Code-First model + migrations path.
 * -------------------------------------------------------------------------
 */
public class StoreDbContext : DbContext
{
    public StoreDbContext(DbContextOptions<StoreDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Design-time fallback: dotnet ef migrations add uses this when no factory is registered.
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(ConnectionHelper.LocalDbConnectionString);
        }
    }
}
