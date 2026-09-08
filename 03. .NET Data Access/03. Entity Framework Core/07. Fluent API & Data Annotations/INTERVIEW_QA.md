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

## Gotchas — Fluent API & Data Annotations (Interview Traps)

---

#### Gotcha 1. Fluent API overrides Data Annotations on the same facet — silent truncation or wrong constraint

**Concepts**
- both annotations and Fluent API configure the same EF facet
- Fluent API wins when they conflict on the same property
- `[MaxLength(100)]` on entity + `HasMaxLength(50)` in Fluent API → column is 50
- developer reading entity sees 100, actual column is 50 — silent truncation
- single source of truth: choose one convention and enforce it

**Answer**

When `[MaxLength(100)]` on a property and `HasMaxLength(50)` in Fluent API both configure the same facet, Fluent API wins. The database column is `nvarchar(50)`, but a developer reading only the entity class sees `[MaxLength(100)]` and trusts it, leading to unexpected truncation errors with values between 51 and 100 characters. Establish a team convention — either use Data Annotations exclusively for simple per-property rules, or use Fluent API centralized in `IEntityTypeConfiguration<T>` classes — and enforce it so the same facet is never configured in both places.

---

#### Gotcha 2. `[Required]` on a reference type does not add NOT NULL without NRT enabled

**Concepts**
- without nullable reference types, all reference properties are nullable in C#
- `[Required]` adds NOT NULL constraint and model validation
- without `[Required]`, `string` property mapped as nullable `nvarchar`
- NRT enabled: non-nullable `string` automatically treated as required by EF Core
- `HasRequired` not available — use `IsRequired()` in Fluent API

**Answer**

Without nullable reference types (NRT) enabled on the project, a `string` property without `[Required]` is mapped as nullable `nvarchar` in the database, even if the business rule says it must have a value. `[Required]` adds the NOT NULL constraint and also triggers model validation in ASP.NET Core. With NRT enabled, EF Core infers `IsRequired()` from the `string` (non-nullable) vs `string?` (nullable) declaration, eliminating the need for `[Required]` on non-nullable string properties. Ensure the project's NRT mode matches the team's approach.

---

#### Gotcha 3. `[MaxLength]` vs `[StringLength]` — different purposes

**Concepts**
- `[MaxLength(200)]` configures EF Core column length
- `[StringLength(200)]` configures ASP.NET Core model validation only
- `[StringLength]` does NOT generate a column length constraint in EF Core
- `[MaxLength]` used for both EF column length and validation
- using `[StringLength]` alone leaves column as `nvarchar(max)`

**Answer**

`[StringLength(200)]` configures ASP.NET Core model validation (enforced during `ModelState.IsValid`) but does not configure the database column length in EF Core — the column remains `nvarchar(max)`. `[MaxLength(200)]` configures both the EF Core column length (generating `nvarchar(200)` in migrations) and works as validation input. For database column size constraints, always use `[MaxLength]` or `HasMaxLength()` in Fluent API. `[StringLength]` is appropriate only for controller model validation without any database schema intent.

---

#### Gotcha 4. `decimal` without `HasPrecision` defaults to `decimal(18,2)` — rounding financial data

**Concepts**
- EF Core default for `decimal` is `decimal(18,2)` if not configured
- four-decimal precision money values silently rounded to two places
- `HasPrecision(precision, scale)` in Fluent API to control precision
- migration must be regenerated after adding `HasPrecision`
- financial calculations with rounded storage produce penny-off errors

**Answer**

EF Core maps `decimal` properties to `decimal(18,2)` by default. If your financial model requires four decimal places (e.g., exchange rates, unit prices to millicents), values are silently rounded to two decimal places at the database level. Use `entity.Property(p => p.ExchangeRate).HasPrecision(18, 6)` in Fluent API to set the required precision. After adding `HasPrecision`, generate a new migration to alter the column type — the existing migration may have used the wrong precision that is already in production.

---

#### Gotcha 5. Index not configured — EF Core generates only PK and FK indexes by default

**Concepts**
- EF Core auto-generates PK index and FK indexes
- non-FK columns with frequent WHERE, JOIN, or ORDER BY — no automatic index
- `HasIndex(p => p.Email).IsUnique()` in Fluent API for explicit indexes
- missing index on high-cardinality filter columns causes table scans
- index review required when EF model goes to production

**Answer**

EF Core automatically creates indexes for primary keys and foreign keys, but no other indexes are generated automatically. Columns frequently used in `WHERE`, `ORDER BY`, or `JOIN` conditions without indexes force SQL Server to perform table or index scans. Production performance issues often stem from missing explicit indexes on columns like `Email`, `Sku`, `OrderDate`, or `Status`. Add `HasIndex(e => e.Email)` (and `.IsUnique()` for unique constraints) in `OnModelCreating` or `IEntityTypeConfiguration<T>` to generate the required indexes in migrations.

---

#### Gotcha 6. `[NotMapped]` property still serialized by JSON serializer

**Concepts**
- `[NotMapped]` is an EF Core attribute — tells EF to ignore the property
- JSON serializers read all public properties regardless of EF attributes
- `[NotMapped]` computed property still appears in API response JSON
- `[JsonIgnore]` required to exclude from JSON serialization
- separation of persistence and serialization concerns

**Answer**

`[NotMapped]` instructs EF Core to ignore a property during schema mapping and query generation, but JSON serializers (`System.Text.Json`, Newtonsoft.Json) read all public properties regardless of EF attributes. A `[NotMapped]` computed property (e.g., `FullName { get; }`) appears in the serialized JSON response even though it is not persisted. To exclude it from JSON output, add `[JsonIgnore]` (System.Text.Json) or `[JsonIgnore]`/`[JsonProperty(Ignored = true)]` (Newtonsoft.Json) in addition to `[NotMapped]`.

---

#### Gotcha 7. `HasComputedColumnSql` without `stored: true` — value not returned after INSERT/UPDATE

**Concepts**
- virtual computed column recalculated on every SELECT
- `stored: true` persisted computed column written to disk
- EF Core does not re-read virtual computed column after INSERT/UPDATE
- tracked entity has stale property value after write without explicit reload
- `ValueGeneratedOnAddOrUpdate()` needed for EF to know value is DB-generated

**Answer**

`HasComputedColumnSql("expression")` without `stored: true` creates a virtual computed column that SQL Server recalculates on every SELECT but does not physically store. After EF Core inserts or updates a row, the tracked entity's corresponding property still holds the pre-save value — EF Core does not automatically re-read virtual computed columns after writes. Add `stored: true` to persist the value to disk (allowing EF Core to read it back in the INSERT result), or call `context.Entry(entity).ReloadAsync()` after `SaveChanges` to refresh the entity from the database.

---

#### Gotcha 8. `[Column(TypeName = "...")]` with wrong type string causes migration failure

**Concepts**
- `[Column(TypeName = "datetime")]` vs `datetime2` precision difference
- wrong TypeName accepted by EF Core but rejected by SQL Server
- `datetime` max precision vs `datetime2` range difference
- `money` vs `decimal` precision semantics
- migration generates correct DDL but wrong type causes runtime data loss

**Answer**

`[Column(TypeName = "datetime")]` maps the property to the SQL Server `datetime` type (millisecond precision, 1753–9999 range) rather than `datetime2` (100-nanosecond precision, 0001–9999 range). For modern .NET `DateTime` values before 1753, `datetime` storage fails. Using `[Column(TypeName = "money")]` maps to SQL Server's monetary type with fixed 4-decimal precision instead of a configurable `decimal`. Always verify the TypeName matches SQL Server's actual type behavior, and prefer `datetime2` over `datetime` for all new date/time columns.

---

#### Gotcha 9. `IEntityTypeConfiguration<T>` not registered — Fluent API silently not applied

**Concepts**
- `IEntityTypeConfiguration<T>` must be registered via `ApplyConfiguration` or `ApplyConfigurationsFromAssembly`
- class not registered → no Fluent API applied → defaults used silently
- `modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly())`
- configuration missing from `OnModelCreating` — no error, wrong schema
- migration generates different schema than intended

**Answer**

An `IEntityTypeConfiguration<Product>` class that is not registered in `OnModelCreating` has no effect — EF Core uses convention defaults for that entity type without any error. Forgetting `modelBuilder.ApplyConfiguration(new ProductConfiguration())` means all the Fluent API in that class (precision, indexes, relationship behavior) is silently ignored and the migration generates the wrong schema. The safest registration is `modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly())` which discovers and applies all `IEntityTypeConfiguration<T>` implementations automatically.

---

#### Gotcha 10. `[Key]` on non-integer property requires `DatabaseGeneratedOption.None` to avoid auto-increment attempt

**Concepts**
- EF Core convention assumes `int`/`long` PK is auto-incremented by database
- `[Key]` on `string` or `Guid` PK defaults to `DatabaseGeneratedOption.Identity`
- EF Core may try to read back a server-generated key on a GUID PK
- `[DatabaseGenerated(DatabaseGeneratedOption.None)]` for client-assigned keys
- `HasDefaultValueSql("NEWSEQUENTIALID()")` for SQL Server–generated GUID PKs

**Answer**

For primary keys of type `string` or `Guid`, EF Core defaults to `DatabaseGeneratedOption.Identity`, expecting the database to generate the key value. On a `string` PK (natural key like an ISO code), EF Core will attempt to read back a server-generated value that does not exist. Add `[DatabaseGenerated(DatabaseGeneratedOption.None)]` or `ValueGeneratedNever()` in Fluent API to tell EF Core that the application supplies the key value. For `Guid` PKs where you want the database to generate the value, use `HasDefaultValueSql("NEWSEQUENTIALID()")` in Fluent API.

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
