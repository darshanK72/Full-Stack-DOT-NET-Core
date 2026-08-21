using ChangeTrackingAsyncAndTransactions.Models;
using Microsoft.EntityFrameworkCore;

namespace ChangeTrackingAsyncAndTransactions.Data;

/*
 * FILE ROLE:
 *   DbContext for the inventory demo database and optional interceptor registration preview.
 *
 * SECTIONS IN THIS FILE:
 *   3. InventoryDbContext and connection configuration
 *   9. Interceptors preview (registration hook only)
 */

/*
 * SECTION 3: INVENTORY DBCONTEXT AND CONNECTION CONFIGURATION
 *
 * LocalDB connection string (same server as ADO.NET / Dapper chapters):
 *   Server=(localdb)\MSSQLLocalDB;Database=EfCoreInventoryTutorial;Integrated Security=true;TrustServerCertificate=true
 *
 * EnsureCreated (called from DatabaseBootstrap) creates dbo.Products for this chapter.
 * Production apps use migrations (ch.03) instead of EnsureCreated.
 * -------------------------------------------------------------------------
 */
public sealed class InventoryDbContext : DbContext
{
    public const string ConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=EfCoreInventoryTutorial;Integrated Security=true;TrustServerCertificate=true";

    public DbSet<Product> Products => Set<Product>();

    public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
        : base(options)
    {
    }

    /*
     * Convenience ctor for demos that build options inline in Program.cs.
     * Prefer DI + DbContextOptions in ASP.NET Core (deferred to Web API modules).
     */
    public InventoryDbContext()
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(ConnectionString);
        }

        // SECTION 9 preview — register interceptors here when you implement them fully.
        // optionsBuilder.AddInterceptors(new AuditSaveChangesInterceptor());
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(p => p.ProductId);
            entity.Property(p => p.ProductId).ValueGeneratedNever(); // seed with explicit Ids 1, 2, 3
            entity.Property(p => p.ProductName).HasMaxLength(100).IsRequired();
            entity.Property(p => p.UnitPrice).HasColumnType("decimal(10,2)");
        });
    }
}
