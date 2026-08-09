using FluentApiAndDataAnnotations.Models;
using FluentApiAndDataAnnotations.Utils;
using Microsoft.EntityFrameworkCore;

namespace FluentApiAndDataAnnotations.Data;

/*
 * FILE ROLE: DbContext with OnModelCreating — Fluent API for indexes, properties,
 *            and owned types where annotations are absent or insufficient.
 *
 * SECTIONS IN THIS FILE:
 *   4. OnModelCreating and entity configuration
 *   5. Indexes (HasIndex, IsUnique)
 *   6b. OwnsOne — owned type mapping (preview)
 */

/*
 * =========================================================================
 * SECTION 4: OnModelCreating AND ENTITY CONFIGURATION
 * =========================================================================
 *
 * EF Core builds a model from:
 *   1. Conventions (keys, table names — see ch03)
 *   2. Data annotations on entity properties (CatalogProduct)
 *   3. Fluent API in OnModelCreating (Supplier and cross-cutting rules)
 *
 * Override OnModelCreating to call modelBuilder.Entity<T>(...) for each type
 * you need to configure explicitly:
 *
 *   modelBuilder.Entity<Supplier>(entity => { ... });
 *
 * Common IEntityTypeConfiguration-style calls (inline here for reading flow):
 *
 *   entity.ToTable("Suppliers")           — table name override
 *   entity.HasKey(e => e.SupplierId)      — usually convention; shown explicitly
 *   entity.Property(e => e.X).IsRequired().HasMaxLength(n)
 *   entity.Ignore(e => e.NotMappedProp)   — exclude from model
 *
 * IEntityTypeConfiguration<T> classes (separate files per entity) keep
 * OnModelCreating thin in large apps — same Fluent API, different organization.
 *
 * Precedence when both annotation and Fluent API apply:
 *   Fluent API generally wins for conflicting mapping facets; validation may
 *   combine both. Prefer one style per property to avoid surprises.
 * -------------------------------------------------------------------------
 */
public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : base(options)
    {
    }

    public DbSet<CatalogProduct> CatalogProducts => Set<CatalogProduct>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(ConnectionHelper.LocalDbConnectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureSupplier(modelBuilder);
        // CatalogProduct relies on data annotations — no Fluent block required for this demo
    }

    private static void ConfigureSupplier(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.ToTable("Suppliers");
            entity.HasKey(e => e.SupplierId);

            entity.Property(e => e.CompanyName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.SupplierCode)
                .IsRequired()
                .HasMaxLength(20);

            /*
             * =========================================================================
             * SECTION 5: INDEXES — HasIndex, IsUnique
             * =========================================================================
             *
             * Indexes speed lookups and enforce uniqueness at the database level.
             *
             *   entity.HasIndex(e => e.SupplierCode)           — non-unique index
             *   entity.HasIndex(e => e.SupplierCode).IsUnique() — UNIQUE constraint
             *
             * Composite index example (not used in demo):
             *   entity.HasIndex(e => new { e.CompanyName, e.SupplierCode });
             *
             * Named index:
             *   entity.HasIndex(e => e.SupplierCode).HasDatabaseName("IX_Suppliers_Code");
             *
             * Filtered index (SQL Server):
             *   entity.HasIndex(e => e.SupplierCode).HasFilter("[SupplierCode] IS NOT NULL");
             *
             * EF creates the index in migrations / EnsureCreated DDL. Duplicate
             * SupplierCode on SaveChanges throws DbUpdateException (unique violation).
             *
             * Annotation alternative: [Index(nameof(SupplierCode), IsUnique = true)] on
             * the entity class — Fluent API is clearer for composite and filtered indexes.
             * -------------------------------------------------------------------------
             */
            entity.HasIndex(e => e.SupplierCode).IsUnique();

            /*
             * =========================================================================
             * SECTION 6b: OwnsOne — OWNED TYPE MAPPING (PREVIEW)
             * =========================================================================
             *
             * OwnsOne tells EF Core that Address has no independent key and belongs
             * to exactly one Supplier row:
             *
             *   entity.OwnsOne(e => e.Address, navigationBuilder => { ... });
             *
             * Column names default to Navigation_Property (Address_Street). Override with
             *   navigationBuilder.Property(a => a.Street).HasColumnName("Street");
             * when aligning to a legacy schema (ch04 database-first).
             *
             * COVERED IN DETAIL LATER — table-splitting owned types and OwnsMany
             * collections when modeling complex aggregates.
             * -------------------------------------------------------------------------
             */
            entity.OwnsOne(e => e.Address, address =>
            {
                address.Property(a => a.Street).HasMaxLength(200);
                address.Property(a => a.City).HasMaxLength(100);
                address.Property(a => a.PostalCode).HasMaxLength(20);
            });
        });
    }
}
