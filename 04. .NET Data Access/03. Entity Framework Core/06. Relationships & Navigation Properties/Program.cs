using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RelationshipsAndNavigationProperties.Data;
using RelationshipsAndNavigationProperties.Models;
using RelationshipsAndNavigationProperties.Utils;

namespace RelationshipsAndNavigationProperties;

/*
 * TOPIC: Relationships & Navigation Properties  -  how EF Core models associations between
 *        entities using foreign keys, navigations, and delete behaviors.
 *
 * WHY IT MATTERS:
 *   Relational databases express associations with foreign keys. EF Core mirrors that in C#
 *   with scalar FK properties and navigation properties so you can query and persist object
 *   graphs instead of manual JOIN and FK bookkeeping. One-to-many and many-to-many appear
 *   in almost every domain model; misconfigured cascades cause accidental data loss.
 *
 * PREREQUISITE:
 *   EF Core ch02 DbContext & DbSet, ch03 Code-First Models & Migrations (entities, conventions).
 *
 * WHAT YOU WILL LEARN:
 *   1. Navigation properties  -  reference vs collection
 *   2. One-to-many  -  principal, dependent, inverse navigation
 *   3. Foreign key scalars  -  convention and graph inserts
 *   4. Fluent API  -  HasOne, WithMany, HasForeignKey, OnDelete
 *   5. Many-to-many  -  explicit join entity with payload (ProductTag)
 *   6. Cascade delete  -  Cascade vs Restrict in practice
 *   7. Demonstration  -  graph insert, Include preview, cascade delete
 *
 * CHAPTER MAP:
 *   1. Navigation properties (reference vs collection) -> Models/Customer.cs
 *   2. One-to-many principal                     -> Models/Customer.cs
 *   3. FK scalar + reference navigation          -> Models/Order.cs
 *   4. Fluent API one-to-many                    -> Data/RelationshipsDbContext.cs
 *   5. Many-to-many join entity                  -> Models/Product.cs
 *                                              -> Models/Tag.cs
 *                                              -> Models/ProductTag.cs
 *   6. Cascade delete rules                      -> Data/RelationshipsDbContext.cs
 *   8. Bootstrap + seed                          -> Utils/RelationshipBootstrap.cs
 *   Connection string                            -> Utils/ConnectionHelper.cs
 *   7. Demonstration                             -> Program.cs Main (below)
 *
 * Connection string:
 *   Server=(localdb)\MSSQLLocalDB;Database=EfCoreTutorial;Integrated Security=true;TrustServerCertificate=true
 */

public class Program
{
    /*
     * SECTION 7: DEMONSTRATION  -  Main orchestrates the chapter demo
     *
     * Bootstraps schema on EfCoreTutorial, then walks one-to-many graph insert,
     * many-to-many linking, Include preview, and cascade delete behavior.
     * -------------------------------------------------------------------------
     */
    public static void Main(string[] args)
    {
        Console.WriteLine("=== 06. Relationships & Navigation Properties (EF Core) ===");
        Console.WriteLine();

        DbContextOptions<RelationshipsDbContext> options = BuildOptions();

        if (!RelationshipBootstrap.TryEnsureSchema(options))
        {
            Console.WriteLine("Database demos skipped  -  start LocalDB and re-run.");
            return;
        }

        Console.WriteLine("EfCoreTutorial ready on LocalDB.");
        Console.WriteLine();

        DemonstrateOneToManyGraphInsert(options);
        DemonstrateManyToManyJoinEntity(options);
        DemonstrateIncludePreview(options);
        DemonstrateCascadeDelete(options);
    }

    private static DbContextOptions<RelationshipsDbContext> BuildOptions()
    {
        DbContextOptionsBuilder<RelationshipsDbContext> builder = new DbContextOptionsBuilder<RelationshipsDbContext>();
        builder.UseSqlServer(ConnectionHelper.LocalDbConnectionString);
        return builder.Options;
    }

    private static void DemonstrateOneToManyGraphInsert(DbContextOptions<RelationshipsDbContext> options)
    {
        Console.WriteLine("--- SECTIONS 2-3: One-to-many graph insert via navigation ---");

        using RelationshipsDbContext context = new RelationshipsDbContext(options);

        Customer newCustomer = new Customer
        {
            Name = "Grace Hopper",
            Email = "grace@example.com",
            Orders =
            {
                new Order { OrderDate = new DateOnly(2024, 6, 1), TotalAmount = 88.00m },
            },
        };

        context.Customers.Add(newCustomer); // EF tracks customer + nested orders; sets FK on SaveChanges
        context.SaveChanges();

        Console.WriteLine($"  Inserted {newCustomer.Name} with {newCustomer.Orders.Count} order(s).");
        Console.WriteLine($"  Order row FK: CustomerId = {newCustomer.Orders.First().CustomerId} (set by EF).");
        Console.WriteLine();
    }

    private static void DemonstrateManyToManyJoinEntity(DbContextOptions<RelationshipsDbContext> options)
    {
        Console.WriteLine("--- SECTION 5: Many-to-many through ProductTag join entity ---");

        using RelationshipsDbContext context = new RelationshipsDbContext(options);

        Product? widget = context.Products.FirstOrDefault(product => product.Name == "Widget");
        Tag? saleTag = context.Tags.FirstOrDefault(tag => tag.Label == "Sale");

        if (widget is null || saleTag is null)
        {
            Console.WriteLine("  Seed data missing  -  run bootstrap first.");
            Console.WriteLine();
            return;
        }

        bool linkExists = context.ProductTags.Any(link =>
            link.ProductId == widget.ProductId && link.TagId == saleTag.TagId);

        if (!linkExists)
        {
            widget.ProductTags.Add(new ProductTag
            {
                Tag = saleTag,
                AddedOn = new DateOnly(2024, 5, 20), // payload column on the join row
            });
            context.SaveChanges();
        }

        int linkCount = context.ProductTags.Count(link => link.ProductId == widget.ProductId);
        Console.WriteLine($"  Widget has {linkCount} tag link(s) in ProductTags.");
        foreach (ProductTag link in context.ProductTags
                     .Where(row => row.ProductId == widget.ProductId)
                     .Include(row => row.Tag)) // Include preview  -  full depth in ch09
        {
            Console.WriteLine($"    ProductTag ({link.ProductId}, {link.TagId}) AddedOn {link.AddedOn:d} -> {link.Tag.Label}");
        }

        Console.WriteLine();
    }

    private static void DemonstrateIncludePreview(DbContextOptions<RelationshipsDbContext> options)
    {
        Console.WriteLine("--- Include preview: customer with orders (eager load) ---");

        using RelationshipsDbContext context = new RelationshipsDbContext(options);

        Customer? ada = context.Customers
            .Include(customer => customer.Orders) // eager load collection navigation  -  ch09 covers Include/ThenInclude fully
            .FirstOrDefault(customer => customer.Name == "Ada Lovelace");

        if (ada is null)
        {
            Console.WriteLine("  Ada Lovelace not found in seed data.");
            Console.WriteLine();
            return;
        }

        Console.WriteLine($"  {ada.Name}: {ada.Orders.Count} order(s) loaded via Include.");
        foreach (Order order in ada.Orders.OrderBy(row => row.OrderDate))
        {
            Console.WriteLine($"    {order}");
        }

        Console.WriteLine("  Without Include, ada.Orders is empty until explicitly loaded -> ch09.");
        Console.WriteLine();
    }

    private static void DemonstrateCascadeDelete(DbContextOptions<RelationshipsDbContext> options)
    {
        Console.WriteLine("--- SECTION 6: Cascade delete  -  Customer removes Orders ---");

        using RelationshipsDbContext context = new RelationshipsDbContext(options);

        Customer? grace = context.Customers
            .Include(customer => customer.Orders)
            .FirstOrDefault(customer => customer.Name == "Grace Hopper");

        if (grace is null)
        {
            Console.WriteLine("  Grace Hopper not found  -  run graph insert demo first.");
            Console.WriteLine();
            return;
        }

        int orderCountBefore = grace.Orders.Count;
        int orderId = grace.Orders.First().OrderId;
        int customerId = grace.CustomerId;

        context.Customers.Remove(grace); // Cascade on Customer -> Order deletes dependent orders in SQL
        context.SaveChanges();

        bool orderStillExists = context.Orders.Any(order => order.OrderId == orderId);
        Console.WriteLine($"  Removed customer {customerId} who had {orderCountBefore} order(s).");
        Console.WriteLine($"  Order {orderId} still in database? {orderStillExists} (expected False  -  Cascade).");
        Console.WriteLine();

        Console.WriteLine("--- SECTION 6b: Product delete cascades ProductTag, not Tag ---");

        Product? gadget = context.Products
            .Include(product => product.ProductTags)
            .FirstOrDefault(product => product.Name == "Gadget");

        if (gadget is null)
        {
            Console.WriteLine("  Gadget product not found.");
            return;
        }

        int tagId = gadget.ProductTags.First().TagId;
        int linkCountBefore = gadget.ProductTags.Count;

        context.Products.Remove(gadget); // Cascade removes ProductTag rows; Tag rows remain (Restrict on Tag side)
        context.SaveChanges();

        bool tagStillExists = context.Tags.Any(tag => tag.TagId == tagId);
        int remainingLinks = context.ProductTags.Count(link => link.ProductId == gadget.ProductId);

        Console.WriteLine($"  Removed Gadget with {linkCountBefore} ProductTag row(s).");
        Console.WriteLine($"  Tag {tagId} still exists? {tagStillExists} (expected True  -  Tag not cascade-deleted).");
        Console.WriteLine($"  Remaining ProductTag rows for product? {remainingLinks} (expected 0).");
        Console.WriteLine();
    }
}

/*
 * QUICK REFERENCE  -  RELATIONSHIPS & NAVIGATION PROPERTIES
 *
 * --- Navigation kinds ---
 *
 *   Reference:  Order.Customer     (many-to-one from dependent)
 *   Collection: Customer.Orders    (one-to-many from principal)
 *
 * --- FK convention ---
 *
 *   Order.CustomerId + Order.Customer  ->  FK on dependent, named {Navigation}Id
 *
 * --- One-to-many Fluent API ---
 *
 *   modelBuilder.Entity<Order>()
 *       .HasOne(o => o.Customer)
 *       .WithMany(c => c.Orders)
 *       .HasForeignKey(o => o.CustomerId)
 *       .OnDelete(DeleteBehavior.Cascade);
 *
 * --- Graph insert (EF sets FK) ---
 *
 *   context.Customers.Add(new Customer {
 *       Orders = { new Order { TotalAmount = 10m, OrderDate = DateOnly.FromDateTime(DateTime.Today) } }
 *   });
 *   context.SaveChanges();
 *
 * --- Many-to-many join entity ---
 *
 *   Product --< ProductTag >-- Tag
 *   Composite key: new { ProductId, TagId }
 *   Payload columns (AddedOn) live on ProductTag
 *
 * --- Skip navigation (no payload)  -  EF Core 5+ preview ---
 *
 *   ICollection<Tag> Tags on Product + inverse on Tag  -  hidden join table
 *
 * --- DeleteBehavior ---
 *
 *   Cascade   -  delete dependents (Customer -> Orders)
 *   Restrict  -  block delete if dependents exist (Tag while ProductTags reference it)
 *   SetNull   -  optional FK only
 *
 * --- Loading related data ---
 *
 *   context.Customers.Include(c => c.Orders)  -> ch09 Loading Related Data
 */
