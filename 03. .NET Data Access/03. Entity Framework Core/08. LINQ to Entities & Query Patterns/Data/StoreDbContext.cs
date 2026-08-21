using System;
using LinqToEntitiesAndQueryPatterns.Models;
using Microsoft.EntityFrameworkCore;

namespace LinqToEntitiesAndQueryPatterns.Data;

/*
 * FILE ROLE:
 *   DbContext with Fluent configuration, HasData seed, and global query filter preview.
 *
 * SECTIONS IN THIS FILE:
 *   1. DbSet properties and constructor
 *   2. OnModelCreating  -  relationships, seed data, global query filter (preview)
 */

/*
 * SECTION 1: DbSet PROPERTIES AND CONSTRUCTOR
 *
 * Each DbSet<T> implements IQueryable<T>  -  LINQ operators chain onto the provider
 * (EF Core SQL Server) instead of LINQ to Objects (IEnumerable in memory).
 *
 * PREREQUISITE: LINQ ch.01 Introduction to LINQ  -  IQueryable<T> vs IEnumerable<T>;
 *               LINQ ch.10 Conversion Operations  -  AsQueryable() preview toward providers.
 *               EF Core ch.02 DbContext & DbSet  -  DbSet basics.
 * -------------------------------------------------------------------------
 */
public sealed class StoreDbContext : DbContext
{
    public StoreDbContext(DbContextOptions<StoreDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>(); // IQueryable<Category> entry point
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();

    /*
     * SECTION 2: OnModelCreating  -  RELATIONSHIPS, SEED DATA, GLOBAL QUERY FILTER (PREVIEW)
     *
     * HasData seeds fixed keys for tutorial repeatability (EnsureCreated on first run).
     *
     * --- Global query filter (PREVIEW) ---
     * HasQueryFilter adds WHERE IsDiscontinued = 0 to every query against Product
     * unless you call IgnoreQueryFilters(). Useful for soft-delete / tenant / archive rows.
     *
     * COVERED IN DETAIL LATER  -  multi-tenant filters, composing filters, migrations with filters
     * are production topics; this chapter shows the API and one demo call site.
     *
     * Pitfall: filtered required relationships can hide parent rows  -  test with IgnoreQueryFilters().
     * -------------------------------------------------------------------------
     */
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.CategoryId);
            entity.Property(c => c.Name).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.ProductId);
            entity.Property(p => p.Name).HasMaxLength(100).IsRequired();
            entity.Property(p => p.UnitPrice).HasPrecision(10, 2);

            entity.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);

            entity.HasQueryFilter(p => !p.IsDiscontinued); // global filter  -  SECTION 6 preview
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.OrderId);
            entity.Property(o => o.CustomerName).HasMaxLength(100).IsRequired();
            entity.Property(o => o.TotalAmount).HasPrecision(10, 2);
        });

        modelBuilder.Entity<OrderLine>(entity =>
        {
            entity.HasKey(l => l.OrderLineId);
            entity.Property(l => l.LineTotal).HasPrecision(10, 2);

            entity.HasOne(l => l.Order)
                .WithMany(o => o.Lines)
                .HasForeignKey(l => l.OrderId);

            entity.HasOne(l => l.Product)
                .WithMany()
                .HasForeignKey(l => l.ProductId);
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasData(
            new Category { CategoryId = 1, Name = "Electronics" },
            new Category { CategoryId = 2, Name = "Office Supplies" });

        modelBuilder.Entity<Product>().HasData(
            new Product { ProductId = 1, Name = "Wireless Mouse", UnitPrice = 24.99m, StockQuantity = 120, CategoryId = 1, IsDiscontinued = false },
            new Product { ProductId = 2, Name = "USB-C Hub", UnitPrice = 49.99m, StockQuantity = 45, CategoryId = 1, IsDiscontinued = false },
            new Product { ProductId = 3, Name = "Mechanical Keyboard", UnitPrice = 89.99m, StockQuantity = 30, CategoryId = 1, IsDiscontinued = false },
            new Product { ProductId = 4, Name = "Legacy Dock", UnitPrice = 59.99m, StockQuantity = 0, CategoryId = 1, IsDiscontinued = true },
            new Product { ProductId = 5, Name = "Notebook Pack", UnitPrice = 12.50m, StockQuantity = 200, CategoryId = 2, IsDiscontinued = false },
            new Product { ProductId = 6, Name = "Ballpoint Pens (12)", UnitPrice = 8.99m, StockQuantity = 350, CategoryId = 2, IsDiscontinued = false },
            new Product { ProductId = 7, Name = "Discontinued Stapler", UnitPrice = 15.00m, StockQuantity = 5, CategoryId = 2, IsDiscontinued = true });

        modelBuilder.Entity<Order>().HasData(
            new Order { OrderId = 1, CustomerName = "Alice Chen", OrderDate = new DateOnly(2024, 6, 1), TotalAmount = 74.98m },
            new Order { OrderId = 2, CustomerName = "Bob Rivera", OrderDate = new DateOnly(2024, 7, 10), TotalAmount = 21.49m },
            new Order { OrderId = 3, CustomerName = "Alice Chen", OrderDate = new DateOnly(2024, 8, 2), TotalAmount = 89.99m });

        modelBuilder.Entity<OrderLine>().HasData(
            new OrderLine { OrderLineId = 1, OrderId = 1, ProductId = 1, Quantity = 1, LineTotal = 24.99m },
            new OrderLine { OrderLineId = 2, OrderId = 1, ProductId = 2, Quantity = 1, LineTotal = 49.99m },
            new OrderLine { OrderLineId = 3, OrderId = 2, ProductId = 5, Quantity = 1, LineTotal = 12.50m },
            new OrderLine { OrderLineId = 4, OrderId = 2, ProductId = 6, Quantity = 1, LineTotal = 8.99m },
            new OrderLine { OrderLineId = 5, OrderId = 3, ProductId = 3, Quantity = 1, LineTotal = 89.99m });
    }
}
