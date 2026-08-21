using Microsoft.EntityFrameworkCore;
using RelationshipsAndNavigationProperties.Models;
using RelationshipsAndNavigationProperties.Utils;

namespace RelationshipsAndNavigationProperties.Data;

/*
 * FILE ROLE:
 *   DbContext with relationship configuration in OnModelCreating (SECTIONS 4-6).
 *
 * SECTIONS IN THIS FILE:
 *   4. Fluent API  -  one-to-many (HasOne / WithMany / HasForeignKey)
 *   5. Fluent API  -  many-to-many join entity (composite key)
 *   6. Cascade delete  -  DeleteBehavior on FK relationships
 *   5b. Skip navigation many-to-many (preview pointer)
 */

/*
 * SECTION 4: FLUENT API  -  ONE-TO-MANY
 *
 * Convention would discover Customer-Order without OnModelCreating, but Fluent API makes
 * cardinality, FK, and delete behavior explicit  -  the pattern used in real apps.
 *
 *   modelBuilder.Entity<Order>()
 *       .HasOne(o => o.Customer)       // dependent -> principal (reference side)
 *       .WithMany(c => c.Orders)      // inverse collection on principal
 *       .HasForeignKey(o => o.CustomerId)
 *       .OnDelete(DeleteBehavior.Cascade);
 *
 * Method chain reads as English: "Order has one Customer with many Orders via CustomerId."
 * -------------------------------------------------------------------------
 */
public sealed class RelationshipsDbContext : DbContext
{
    public RelationshipsDbContext(DbContextOptions<RelationshipsDbContext> options)
        : base(options)
    {
    }

    public RelationshipsDbContext()
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<ProductTag> ProductTags => Set<ProductTag>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(ConnectionHelper.LocalDbConnectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        /*
         * SECTION 4 (continued): CUSTOMER -> ORDERS ONE-TO-MANY
         *
         * DeleteBehavior.Cascade: deleting a Customer deletes dependent Orders in SQL Server.
         * Default for required relationships is often Cascade already  -  explicit for teaching.
         */
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasOne(order => order.Customer)
                .WithMany(customer => customer.Orders)
                .HasForeignKey(order => order.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        /*
         * SECTION 5: MANY-TO-MANY JOIN ENTITY  -  PRODUCTTAG
         *
         * Composite PK on (ProductId, TagId). Each FK gets its own relationship to
         * Product or Tag with separate delete rules (SECTION 6).
         */
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("CatalogProducts"); // avoid dbo.Products name clash with ch02/ch03
            entity.HasKey(product => product.ProductId);
        });

        modelBuilder.Entity<ProductTag>(entity =>
        {
            entity.HasKey(link => new { link.ProductId, link.TagId });

            entity.HasOne(link => link.Product)
                .WithMany(product => product.ProductTags)
                .HasForeignKey(link => link.ProductId)
                .OnDelete(DeleteBehavior.Cascade); // delete product -> delete its ProductTag rows

            entity.HasOne(link => link.Tag)
                .WithMany(tag => tag.ProductTags)
                .HasForeignKey(link => link.TagId)
                .OnDelete(DeleteBehavior.Restrict); // cannot delete Tag while ProductTag rows reference it
        });

        /*
         * SECTION 6: CASCADE DELETE  -  DeleteBehavior SUMMARY
         *
         *   Behavior   | Effect when principal is deleted
         *   -----------|----------------------------------------------------------
         *   Cascade    | Delete dependent rows (Orders when Customer deleted)
         *   Restrict   | Throw  -  FK would be orphaned (Tag delete while links exist)
         *   SetNull    | Set FK column to NULL (optional relationships only)
         *   NoAction   | Database-dependent; often same as Restrict on SQL Server
         *
         * This model:
         *   Customer --Cascade--> Order
         *   Product  --Cascade--> ProductTag
         *   Tag      --Restrict--> ProductTag (must remove links first)
         *
         * Pitfall: Cascade chains can surprise you (delete Customer wipes all Orders).
         * Use Restrict or soft-delete when business rules forbid physical cascade.
         *
         * Data annotations alternative ([DeleteBehavior]) -> ch07 Fluent API & Data Annotations.
         */

        /*
         * SECTION 5b: SKIP NAVIGATION MANY-TO-MANY (PREVIEW)
         *
         * When the link table has NO extra columns, EF Core 5+ can model:
         *
         *   public ICollection<Tag> Tags { get; set; }   // on Product
         *   public ICollection<Product> Products { get; set; }  // on Tag
         *
         * EF generates a hidden join table. Use explicit ProductTag when you need AddedOn
         * or other payload on the association row (this chapter's approach).
         */
    }
}
