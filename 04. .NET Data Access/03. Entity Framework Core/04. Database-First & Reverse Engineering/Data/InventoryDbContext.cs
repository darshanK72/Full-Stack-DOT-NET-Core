using DatabaseFirstAndReverseEngineering.Models;
using DatabaseFirstAndReverseEngineering.Utils;
using Microsoft.EntityFrameworkCore;

namespace DatabaseFirstAndReverseEngineering.Data;

/*
 * FILE ROLE:
 *   Scaffolded-style DbContext for EfScaffoldTutorial (hand-written to match reverse-engineering output).
 *
 * SECTIONS IN THIS FILE:
 *   5. Generated DbContext and fluent mapping
 */

/*
 * SECTION 5: GENERATED DBCONTEXT AND FLUENT MAPPING
 *
 * Scaffold emits:
 *   - partial class InventoryDbContext : DbContext
 *   - DbSet<T> for each table
 *   - OnModelCreating with entity configuration (table names, keys, relationships, column types)
 *   - partial void OnModelCreatingPartial(ModelBuilder) hook for your extensions
 *
 * Modern scaffold flags:
 *   --no-onconfiguring   DbContext has no connection string; use DI / DbContextOptions
 *   --no-pluralize       DbSet names match table names exactly
 *
 * This tutorial keeps OnConfiguring so the console demo runs without DI (see ch.02 for DI registration).
 * Re-scaffold overwrites this file unless you split custom config into InventoryDbContext.Partial.cs.
 * -------------------------------------------------------------------------
 */
public partial class InventoryDbContext : DbContext
{
    public InventoryDbContext()
    {
    }

    public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; } = null!;

    public virtual DbSet<Product> Products { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(ConnectionHelper.EfScaffoldTutorial);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId);

            entity.ToTable("Categories");

            entity.Property(e => e.Name)
                .HasMaxLength(100);

            entity.Property(e => e.Description)
                .HasMaxLength(500);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId);

            entity.ToTable("Products");

            entity.Property(e => e.ProductName)
                .HasMaxLength(100);

            entity.Property(e => e.UnitPrice)
                .HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Category)
                .WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Products_Categories");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
