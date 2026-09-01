# 07. Fluent API & Data Annotations — Interview Q&A
> Back to [Entity Framework Core Interview Q&A](../INTERVIEW_QA.md)

## Table of Contents

- [Q45. What is the Fluent API in EF Core?](#q45-what-is-the-fluent-api-in-ef-core)
- [Q46. When should you prefer Fluent API over data annotations?](#q46-when-should-you-prefer-fluent-api-over-data-annotations)
- [Q47. What is `IEntityTypeConfiguration<T>`?](#q47-what-is-ientitytypeconfigurationt)
- [Q48. How do Fluent API and annotations interact when both configure the same property?](#q48-how-do-fluent-api-and-annotations-interact-when-both-configure-the-same-property)
- [Q49. How do you configure indexes with Fluent API?](#q49-how-do-you-configure-indexes-with-fluent-api)
- [Gotchas](#gotchas)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Chapter 07. Fluent API & Data Annotations

---

## Q45. What is the Fluent API in EF Core?

**Concepts**
- ModelBuilder method chain in OnModelCreating
- configuration beyond convention limits
- IEntityTypeConfiguration<T> for modular mapping
- ApplyConfigurationsFromAssembly discovery

**Answer**

The Fluent API is a code-based configuration surface in `OnModelCreating` (or `IEntityTypeConfiguration<T>` classes) that describes the EF Core model using method chains instead of attributes on entity classes. It controls tables, keys, properties, relationships, indexes, and constraints with full expressiveness. Called on `ModelBuilder` — for example `modelBuilder.Entity<Product>().ToTable("Products").HasKey(p => p.Id);`; Supports configurations that have no data annotation equivalent (composite keys, complex relationship rules, value conversions). Keeps persistence concerns out of domain entity classes when you prefer POCOs without attributes. `ApplyConfigurationsFromAssembly` discovers and applies all `IEntityTypeConfiguration<T>` implementations in an assembly.

---

## Q46. When should you prefer Fluent API over data annotations?

**Concepts**
- complex configuration not expressible as annotations
- POCO domain class separation from persistence
- cross-cutting model rules in configuration classes
- annotation simplicity for basic constraints

**Answer**

Prefer Fluent API when configuration is complex, affects multiple types, has no annotation equivalent, or when you want to keep entity classes free of persistence attributes. Data annotations suit simple, visible constraints on individual properties. Composite keys, alternate keys, owned types, and detailed cascade/index tuning require Fluent API; Large models benefit from separate configuration classes rather than cluttering entities with `[Column]`, `[ForeignKey]`, and `[Index]` on every property. Cross-cutting rules (global naming, soft-delete filters, shared base configurations) belong in Fluent API or configuration classes. Data annotations remain fine for basic validation attributes (`[Required]`, `[MaxLength]`) that double as API validation in ASP.NET Core.

---

## Q47. What is `IEntityTypeConfiguration<T>`?

**Concepts**
- per-entity configuration encapsulation
- Configure method with EntityTypeBuilder<T>
- assembly-wide discovery via ApplyConfigurationsFromAssembly
- reuse across multiple DbContext types

**Answer**

`IEntityTypeConfiguration<T>` is an interface for encapsulating Fluent API configuration for a single entity type in its own class, implementing `Configure(EntityTypeBuilder<T> builder)`. It keeps `OnModelCreating` clean and groups all mapping rules for one entity in one place. Example: `public class OrderConfiguration : IEntityTypeConfiguration<Order> { public void Configure(EntityTypeBuilder<Order> builder) { ... } }`; Register with `modelBuilder.ApplyConfiguration(new OrderConfiguration())` or `ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly)`. Each configuration class owns table name, keys, properties, relationships, and indexes for that entity. Supports testing and reuse — the same configuration applies across multiple `DbContext` types if needed.

---

## Q48. How do Fluent API and annotations interact when both configure the same property?

**Concepts**
- Fluent API precedence over data annotations
- same-facet conflict resolution
- pick-one source of truth principle
- migration verification after configuration change

**Answer**

When Fluent API and data annotations configure the same aspect of the model, Fluent API takes precedence and overrides the annotation. EF Core merges configurations but resolves conflicts in favor of explicit Fluent API calls. If `[MaxLength(100)]` is on a property and Fluent API sets `.HasMaxLength(200)`, the effective limit is 200; Avoid duplicating the same rule in both places — pick one source of truth per property to prevent confusion during maintenance. Some annotations (for example `[Key]`) and Fluent API can coexist when they agree; conflicting values silently favor Fluent API. Review generated migrations when changing configuration sources to ensure schema matches intent.

---

## Q49. How do you configure indexes with Fluent API?

**Concepts**
- HasIndex single-column index
- composite index with anonymous type
- IsUnique constraint
- HasDatabaseName explicit index name

**Answer**

Define indexes in Fluent API with `HasIndex` on the entity builder, optionally marking them unique or naming them explicitly. Indexes speed up queries on filtered, sorted, or joined columns and enforce uniqueness at the database level. Single column: `builder.HasIndex(p => p.Sku);`; Composite: `builder.HasIndex(p => new { p.LastName, p.FirstName });`. Unique: `builder.HasIndex(p => p.Email).IsUnique();`. Named index: `.HasDatabaseName("IX_Product_Sku")` — migrations emit `CREATE INDEX` statements accordingly.

---

## Gotchas

---

## Gotcha 9. Scoped `DbContext` captured in a singleton

**Concepts**
- scoped DbContext captured in singleton field
- captive dependency anti-pattern
- IDbContextFactory for singleton database access

**Answer**

Registering a singleton service that holds a scoped `DbContext` creates a captive dependency — the context may be disposed while the singleton lives, or state leaks across HTTP requests. `DbContext` is scoped per request in ASP.NET Core — singletons must not store it in fields; Inject `IDbContextFactory<TContext>` into singletons when long-lived services need occasional database access. Symptoms include "Cannot access a disposed context" or cross-user data contamination in tracked entities.

---

## Scenario-Based Questions (Karat Format)

---

## Q132. (R) After a schema review, QA reports that `CatalogProducts.Name` truncates at 50 characters even though the entity still shows `[MaxLength(100)]`. Review the configuration split across files. What conflicted, what actually landed in SQL Server, and how do you fix it so the team is not surprised again?

```csharp
// Models/CatalogProduct.cs
[Required(AllowEmptyStrings = false)]
[MaxLength(100)]
public string Name { get; set; } = string.Empty;

// Data/CatalogDbContext.cs — added during a hotfix
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<CatalogProduct>(entity =>
    {
        entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
    });
}
```

**Concepts**
- MaxLength annotation vs Fluent API conflict
- Fluent API wins on same-facet conflict
- single source of truth for property configuration
- migration schema verification after config change

**Answer**

EF Core applied both configuration sources, but when annotation and Fluent API disagree on the same mapping facet, Fluent API wins — the column is `NVARCHAR(50)`, not 100. Developers reading only `CatalogProduct.cs` will misdiagnose truncation as an application bug.

1. Pick **one** authoritative rule per property — remove the loser (`[MaxLength(100)]` **or** the Fluent `HasMaxLength(50)`), matching the intended business limit.
2. Regenerate or add a migration so the database, model snapshot, and entity agree; verify with `dotnet ef migrations script` or SSMS column definition.
3. Adopt team convention: simple per-property rules on the entity **or** centralized Fluent/`IEntityTypeConfiguration<T>` — not both for the same facet (matches **CatalogDbContext.cs** precedence note).
4. Add a PR checklist item: "no duplicate mapping facets across annotations and Fluent."

---

## Q133. (R) Finance rejects migrated totals: penny amounts drift after bulk price imports. The team points at `CatalogProduct.UnitPrice`. Review the entity and migration snippet. What is wrong with the money mapping, and what do you change before the next deploy?

```csharp
// Models/CatalogProduct.cs
public decimal UnitPrice { get; set; }

// Generated migration Up()
migrationBuilder.CreateTable(
    name: "CatalogProducts",
    columns: table => new
    {
        UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
    });
```

```csharp
entity.Property(e => e.UnitPrice).HasPrecision(19, 4);
```

**Concepts**
- decimal precision default causing penny rounding
- decimal(18,2) vs money type mapping
- HasColumnType for SQL Server money precision
- bulk import precision loss

**Answer**

Money columns need an explicit precision and scale (`HasPrecision` / `[Precision]`); relying on SQL Server's default `decimal(18,2)` silently rounds values like `12.3456789m` to two decimal places at persistence. When Fluent `HasPrecision(19,4)` conflicts with `[Column(TypeName = "decimal(18,2)")]`, Fluent API wins — but stale attributes and old migrations still confuse reviewers and can block clean diffs.

1. Decide business scale (e.g., `(19,4)` for unit prices) and configure **once**: `entity.Property(e => e.UnitPrice).HasPrecision(19, 4);` or `[Precision(19, 4)]` — not both conflicting.
2. Remove obsolete `[Column(TypeName = ...)]` if Fluent owns the facet; regenerate migration to `ALTER COLUMN` with correct precision/scale.
3. Validate rounding rules in the domain layer if display/storage scales differ (store 4 dp, present 2 dp in UI).
4. Add integration tests that insert boundary decimals and assert round-trip equality from SQL.

```csharp
entity.Property(e => e.UnitPrice).HasPrecision(19, 4);
```

---

## Q134. (R) Production paging on order history is slow after go-live. The `OrderLines` table has ~2M rows. Review the relationship configuration — FK exists, but DBAs see a table scan on every lookup by product. What is missing, and how do you fix it in EF Core?

```csharp
public class OrderLine
{
    public int OrderLineId { get; set; }
    public int CatalogProductId { get; set; }
    public int Quantity { get; set; }
    public CatalogProduct Product { get; set; } = null!;
}

// CatalogDbContext.OnModelCreating
modelBuilder.Entity<OrderLine>(entity =>
{
    entity.HasOne(e => e.Product)
          .WithMany()
          .HasForeignKey(e => e.CatalogProductId)
          .OnDelete(DeleteBehavior.Restrict);
    // no index configuration
});
```

**Concepts**
- missing FK index causing table scan on OrderLines
- HasIndex Fluent API for FK column
- CREATE INDEX migration operation
- query plan improvement after index

**Answer**

EF Core creates the foreign-key constraint but does not automatically create a nonclustered index on the FK column. Without `HasIndex(e => e.CatalogProductId)`, queries filtering or joining on `CatalogProductId` scan the heap as row count grows.

1. Add index in Fluent API: `entity.HasIndex(e => e.CatalogProductId);` (name it in migrations for clarity).
2. Generate and deploy migration; verify plan uses seek/lookup in SSMS/`SET STATISTICS IO ON`.
3. For composite queries (e.g., product + date range), consider composite index `{ CatalogProductId, CreatedUtc }` based on actual query shapes.
4. Document team rule: **every FK used in filters/joins gets an index** unless a covering clustered key already serves it.

```csharp
modelBuilder.Entity<OrderLine>(entity =>
{
    entity.HasOne(e => e.Product)
          .WithMany()
          .HasForeignKey(e => e.CatalogProductId)
          .OnDelete(DeleteBehavior.Restrict);

    entity.HasIndex(e => e.CatalogProductId);
});
```

---

## Q135. (R) With `#nullable enable`, a developer "fixes" compiler warnings on `CatalogProduct` but SaveChanges behavior changes in staging. Review the property declarations. What do nullable reference types vs `[Required]` each control, and what would you standardize?

```csharp
#nullable enable

public class CatalogProduct
{
    public int CatalogProductId { get; set; }

    // Developer removed [Required] — "string is already non-nullable"
    public string Name { get; set; } = string.Empty;

    [Required]
    public string? Sku { get; set; }

    public string? Description { get; set; }
}
```

**Concepts**
- nullable reference types vs Required annotation interaction
- NRT controls compiler nullability not SQL schema
- Required attribute for NOT NULL column
- standardized nullability convention

**Answer**

Nullable reference types are a compile-time C# feature; `[Required]` is an EF Core / DataAnnotations mapping and validation facet. A non-nullable `string Name` without `[Required]` is still mapped required by EF convention, but removing `[Required(AllowEmptyStrings = false)]` allows empty string at validation. `[Required]` on `string? Sku` is contradictory — the column is required in the model while the type promises nullability to the compiler.

1. Align each property: required value → `string` + `[Required(AllowEmptyStrings = false)]` when empty string must fail; optional → `string?` without `[Required]`.
2. Remove `[Required]` from nullable reference properties unless you intentionally require non-null at persistence (then use non-nullable `string` with initializer).
3. Standardize: **NRT for C# contracts**, **`[Required]` / Fluent `IsRequired()` for persistence and SaveChanges validation** — document on the team wiki.
4. Mirror rules in ASP.NET model binding if the same entities surface in APIs.

```csharp
[Required(AllowEmptyStrings = false)]
[MaxLength(100)]
public string Name { get; set; } = string.Empty;

[StringLength(20, MinimumLength = 3)]
public string Sku { get; set; } = string.Empty;

public string? Description { get; set; }
```

---

## Q136. (P) Your team inherits scaffolded `Supplier` rows from a legacy database and adds `[StringLength(20)]` on `SupplierCode` while `CatalogDbContext` already calls `HasIndex(e => e.SupplierCode).IsUnique()` and `HasMaxLength(20)` in Fluent API. Builds succeed; tests pass locally. What production risks remain if you leave both styles on the same property, and what convention would you enforce?

**Concepts**
- StringLength annotation plus Fluent API HasMaxLength duplication
- Fluent API precedence rule
- pick-one configuration source convention
- CI annotation lint for duplicate config

**Answer**

When values agree, EF merges facets and tests pass, but duplicate configuration still creates drift risk on the next edit — someone updates the annotation during a scaffold regen and misses Fluent, or vice versa. Indexes and uniqueness belong in Fluent anyway; sprinkling annotations on a POCO designed for OnModelCreating (this chapter's Supplier) fights the intended architecture. - Drift risk: Scaffold regen overwrites entity attributes; Fluent block unchanged → next migration surprises (length, required, index name). - Review blind spot: Reviewers see `[StringLength(20)]` on the class and skip CatalogDbContext — unique index or filtered index rules stay invisible. - Validation duplication: DataAnnotations validators and EF Fluent may both apply; conflicting error messages in API vs console paths. - Convention: Keep Supplier annotation-free; all mapping in `ConfigureSupplier` or `IEntityTypeConfiguration<Supplier>`. Use annotations on CatalogProduct-style simple entities only when the team owns the file and won't rescaffold. - Production enforcement: Analyzer or PR rule — "no `[MaxLength]` / `[Required]` on entities configured in Fluent for the same property"; single `IEntityTypeConfiguration` per aggregate in large apps. Production takeaway: Builds succeeding does not mean configuration is maintainable — Karat tests judgment about one boss per facet, not whether EF can merge identical duplicates today.

---

## Q137. (R) A pricing microservice bulk-inserts catalog rows. One bad batch passes C# compilation and EF model validation but corrupts amounts in SQL Server. Review the insert path and configuration. List the issues and prioritized fixes.

```csharp
var batch = new[]
{
    new CatalogProduct { Name = "Widget", Sku = "WDG-001", UnitPrice = 12.3456789m },
    new CatalogProduct { Name = "Gadget", Sku = "GDG", UnitPrice = 0m }
};

context.CatalogProducts.AddRange(batch);
await context.SaveChangesAsync();

// Entity — no precision facet
public decimal UnitPrice { get; set; }

// DbContext — only Supplier uses Fluent; CatalogProduct is annotation-only
```

**Concepts**
- bulk insert bypassing EF decimal and string validation
- database CHECK constraint vs EF validation
- ExecuteSqlRaw parameter type safety
- layered validation for bulk write paths

**Answer**

The batch relies on implicit `decimal(18,2)` mapping, so high-precision literals are rounded at the database without throwing; short SKUs may pass or fail depending on whether `[StringLength(20, MinimumLength = 3)]` is present on the entity — and annotation-only configuration on CatalogProduct means no Fluent safety net for money or indexes.

1. Add explicit money mapping: `HasPrecision(19, 4)` (or team standard) via Fluent or `[Precision]` on `UnitPrice`; add migration.
2. Validate in domain/service before `AddRange`: scale, min SKU length, non-empty name — do not rely on DB rounding as validation.
3. Consider `[StringLength(20, MinimumLength = 3)]` + `[Required(AllowEmptyStrings = false)]` consistently (as in chapter **CatalogProduct**); fail fast before `SaveChanges`.
4. For bulk paths, use transactions and row-level error reporting; optionally `ExecuteUpdate`/bulk extensions with explicit column types.
5. Add tests that assert inserted `UnitPrice` equals source after read-back from SQL.
