using Microsoft.EntityFrameworkCore;
using RawSqlAndStoredProcedures.Models;
using RawSqlAndStoredProcedures.Utils;

namespace RawSqlAndStoredProcedures.Data;

/*
 * FILE ROLE: DbContext for EfCoreTutorial — registers Product as a mapped entity and supplies
 *            the connection used by FromSqlRaw, ExecuteSqlRaw, and SqlQuery demos.
 *
 * SECTIONS IN THIS FILE:
 *   4. DbContext and fluent mapping
 */

/*
 * SECTION 4: DbContext AND FLUENT MAPPING
 *
 * DbContext is the EF Core entry point (ch.02). Raw SQL methods hang off:
 *   • context.Products          — DbSet<Product> for FromSqlRaw / FromSqlInterpolated
 *   • context.Database          — ExecuteSqlRaw and SqlQuery<T>
 *
 * OnConfiguring sets the SQL Server provider for tutorial simplicity. Production apps pass
 * DbContextOptions via DI instead (DEFER → ASP.NET Web API modules).
 *
 * OnModelCreating maps Product to dbo.Products. FromSqlRaw still requires the SELECT list to
 * match these columns when materializing Product instances.
 *
 * Keyless types (InventorySummary, LowStockRow) are NOT registered here — EF Core 8
 * SqlQuery<T> materializes them without DbSet registration (SECTION 9).
 */
public sealed class EfCoreTutorialDbContext : DbContext
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(ConnectionHelper.ConnectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products", "dbo");
            entity.HasKey(p => p.ProductId);

            entity.Property(p => p.ProductId).HasColumnName("ProductId");
            entity.Property(p => p.ProductName).HasColumnName("ProductName").HasMaxLength(100).IsRequired();
            entity.Property(p => p.UnitPrice).HasColumnName("UnitPrice").HasColumnType("decimal(10,2)");
            entity.Property(p => p.StockQuantity).HasColumnName("StockQuantity");
            entity.Property(p => p.DiscontinuedDate).HasColumnName("DiscontinuedDate").HasColumnType("date");

            entity.Ignore(p => p.IsDiscontinued); // not persisted — computed in C#
        });
    }
}
