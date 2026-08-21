using LoadingRelatedData.Models;
using Microsoft.EntityFrameworkCore;

namespace LoadingRelatedData.Data;

/*
 * FILE ROLE:
 *   DbContext mapping Order, Customer, OrderLine, and Product to AdoNetTutorial tables.
 *
 * SECTIONS IN THIS FILE:
 *   2. DbContext - DbSet registration and relationship configuration
 */

/*
 * SECTION 2: DbContext - DbSet REGISTRATION AND RELATIONSHIPS
 *
 * DbContext basics (options, lifetime): COVERED IN DETAIL -> 02. DbContext & DbSet
 * Fluent relationship API depth:      COVERED IN DETAIL -> 06. Relationships & Navigation Properties
 *
 * OnModelCreating wires FK columns to navigation properties so Include/Load know
 * which JOINs to generate. Table names match the ADO.NET / Dapper bootstrap schema.
 * -------------------------------------------------------------------------
 */
public sealed class ShopDbContext : DbContext
{
    public ShopDbContext(DbContextOptions<ShopDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("Customers");
            entity.HasKey(c => c.CustomerId);
            entity.Property(c => c.Name).HasMaxLength(100);
            entity.Property(c => c.Email).HasMaxLength(200);
            entity.HasMany(c => c.Orders)
                .WithOne(o => o.Customer)
                .HasForeignKey(o => o.CustomerId);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(o => o.OrderId);
            entity.Property(o => o.TotalAmount).HasColumnType("decimal(10,2)");
            entity.HasMany(o => o.Lines)
                .WithOne(l => l.Order)
                .HasForeignKey(l => l.OrderId);
        });

        modelBuilder.Entity<OrderLine>(entity =>
        {
            entity.ToTable("OrderLines");
            entity.HasKey(l => l.OrderLineId);
            entity.Property(l => l.LineTotal).HasColumnType("decimal(10,2)");
            entity.HasOne(l => l.Product)
                .WithMany()
                .HasForeignKey(l => l.ProductId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(p => p.ProductId);
            entity.Property(p => p.ProductName).HasMaxLength(100);
            entity.Property(p => p.UnitPrice).HasColumnType("decimal(10,2)");
        });
    }
}
