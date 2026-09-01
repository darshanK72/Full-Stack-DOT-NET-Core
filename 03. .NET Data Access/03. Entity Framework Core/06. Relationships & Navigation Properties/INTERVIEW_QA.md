# 06. Relationships & Navigation Properties — Interview Q&A
> Back to [Entity Framework Core Interview Q&A](../INTERVIEW_QA.md)

## Table of Contents

- [Q38. What is a navigation property in EF Core?](#q38-what-is-a-navigation-property-in-ef-core)
- [Q39. What is the difference between one-to-many and many-to-many relationships?](#q39-what-is-the-difference-between-one-to-many-and-many-to-many-relationships)
- [Q40. How do you configure a one-to-many relationship in code-first?](#q40-how-do-you-configure-a-one-to-many-relationship-in-code-first)
- [Q41. What is a foreign key property in EF Core?](#q41-what-is-a-foreign-key-property-in-ef-core)
- [Q42. What is cascade delete in EF Core?](#q42-what-is-cascade-delete-in-ef-core)
- [Q43. When would you disable cascade delete?](#q43-when-would-you-disable-cascade-delete)
- [Q44. What is a many-to-many relationship in EF Core 5+?](#q44-what-is-a-many-to-many-relationship-in-ef-core-5)
- [Gotchas](#gotchas)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Chapter 06. Relationships & Navigation Properties

---

## Q38. What is a navigation property in EF Core?

**Concepts**
- reference navigation to one entity
- collection navigation to many entities
- relationship traversal without manual joins
- convention-based and Fluent API inference

**Answer**

A navigation property is a CLR property on an entity that represents a relationship to one or more related entities — a reference to a single related row or a collection of related rows. EF Core uses navigation properties to traverse associations in LINQ and to load related data through includes, lazy loading, or explicit loading. A reference navigation points to one related entity (for example `Order.Customer`); A collection navigation points to many related entities (for example `Customer.Orders`). Navigation properties are optional in the model but enable relationship traversal without manually joining foreign keys in every query. EF Core infers relationships from navigation properties paired with foreign key properties or configures them explicitly via Fluent API or data annotations.

---

## Q39. What is the difference between one-to-many and many-to-many relationships?

**Concepts**
- FK on child side for one-to-many
- join table for many-to-many
- collection navigation cardinality
- EF Core 5+ implicit join table support

**Answer**

A one-to-many relationship means one parent entity relates to many child entities through a foreign key on the child side. A many-to-many relationship means entities on both sides can relate to multiple rows on the other side, typically modeled with a join entity or implicit join table in EF Core 5+. One-to-many: the child table holds the foreign key (`Order.CustomerId` → `Customer.Id`); Many-to-many: neither side stores the other's key directly — EF Core 5+ can use a join entity or an implicit join table with two foreign keys. One-to-many navigation is usually a single reference on the many side and a collection on the one side. Many-to-many navigation is a collection on both sides (`Student.Courses` and `Course.Students`).

---

## Q40. How do you configure a one-to-many relationship in code-first?

**Concepts**
- convention-based FK inference
- HasOne/WithMany Fluent API configuration
- ForeignKey and InverseProperty annotations
- required vs optional relationship control

**Answer**

EF Core can infer one-to-many from navigation and foreign key properties by convention, or you configure it explicitly with Fluent API or data annotations when conventions are insufficient. The child entity must expose a foreign key property or shadow foreign key pointing to the parent primary key. By convention: add `public int CustomerId { get; set; }` and `public Customer Customer { get; set; }` on `Order` — EF Core wires the relationship automatically; Fluent API: `modelBuilder.Entity<Order>().HasOne(o => o.Customer).WithMany(c => c.Orders).HasForeignKey(o => o.CustomerId);`. Data annotations: `[ForeignKey(nameof(Customer))]` on the FK property or `[InverseProperty]` when multiple relationships exist between the same types. Required versus optional relationships are controlled with `.IsRequired()` or nullable foreign key types.

---

## Q41. What is a foreign key property in EF Core?

**Concepts**
- scalar FK column on dependent entity
- shadow property as alternative
- relationship fix-up on FK value change
- composite FK explicit configuration

**Answer**

A foreign key property is the scalar column on the dependent entity that stores the primary key value of the related principal entity. EF Core maps it to a database FK constraint and uses it for relationship fix-up, cascade rules, and LINQ joins. It can be a CLR property (`public int CustomerId`) or a shadow property configured only in the model; The dependent entity is the side that holds the foreign key — in one-to-many, the "many" side typically owns the FK column. Changing the FK value reassigns the relationship without necessarily loading the related navigation object. Composite foreign keys require multiple properties and explicit configuration in Fluent API.

---

## Q42. What is cascade delete in EF Core?

**Concepts**
- ON DELETE CASCADE database semantics
- required relationship default behavior
- DeleteBehavior enum options
- ClientCascade for in-memory cascade

**Answer**

Cascade delete means when the principal (parent) entity is deleted, EF Core automatically deletes dependent (child) entities that reference it, matching `ON DELETE CASCADE` in the database. It is the default for required relationships in many configurations. Configured with `OnDelete(DeleteBehavior.Cascade)` in Fluent API or by convention for required FKs; When you call `Remove(parent)`, EF Core marks related children as `Deleted` if cascade is enabled. The database schema must also define the matching FK cascade rule for deletes executed outside EF Core. `DeleteBehavior.ClientCascade` deletes dependents in memory without relying on database cascade — used in specific scenarios.

---

## Q43. When would you disable cascade delete?

**Concepts**
- Restrict behavior for independent child records
- soft-delete pattern compatibility
- NO ACTION legacy database constraint
- explicit orphan management clarity

**Answer**

Disable cascade delete when deleting a parent must not automatically remove children — for example when child records have independent business meaning, audit requirements, or soft-delete policies. Use `DeleteBehavior.Restrict` or `NoAction` to force explicit handling of dependents before deleting the principal. Lookup or reference tables where child rows should block parent deletion (`Restrict` throws on delete if dependents exist); Soft-delete patterns where children remain linked to an archived parent instead of being physically removed. Legacy databases where FK constraints use `NO ACTION` and database-enforced rules differ from EF defaults. Explicit orphan management gives clearer error messages and audit trails than silent cascade removal.

---

## Q44. What is a many-to-many relationship in EF Core 5+?

**Concepts**
- skip navigation on both sides
- implicit join table generated by EF Core
- explicit join entity with extra payload columns
- migration-created join table schema

**Answer**

In EF Core 5 and later, many-to-many can be modeled with skip navigation properties on both entities and an implicit join table managed by EF Core, without requiring a dedicated join entity class. EF Core creates and maps the join table automatically based on conventions. Both entities expose collection navigations (`Post.Tags` and `Tag.Posts`); EF Core generates a hidden join table (for example `PostTag`) with composite foreign keys to both sides. You can expose the join entity explicitly when you need extra columns on the link (for example `AssignedDate` on a `CourseEnrollment` entity). Migrations create the join table schema; querying either collection loads related entities through the join.

---

## Gotchas

---

## Gotcha 11. N+1 from lazy load or missing Include

**Concepts**
- N+1 from missing Include in loop
- one query per parent row multiplication
- eager loading and projection as fixes

**Answer**

Listing parent entities then accessing navigation properties in a loop without eager loading or projection fires one SQL query per parent row — classic N+1 performance collapse in EF Core APIs. One query for N orders plus N queries for each order's lines equals N+1 round-trips per request; Fix with `Include`/`ThenInclude`, split queries, or `Select` projections that join needed data in one statement. EF Core command logging revealing identical query templates with different IDs signals N+1 immediately.

---

## Gotcha 12. Cartesian explosion with multiple Includes

**Concepts**
- cartesian explosion from multiple collection Includes
- row multiplication by collection cardinality product
- AsSplitQuery and DTO projection as mitigations

**Answer**

Eager-loading two or more collection navigations in one SQL query multiplies result rows by the product of collection sizes, spiking memory and network use even though parent entity count is modest. EF Core deduplicates parents during fix-up, but SQL Server already sent the inflated rowset across the wire; Use `AsSplitQuery()` to fetch collections with separate SELECT statements instead of one giant join. Projection to DTOs avoids loading full collection graphs when only counts or summaries are needed.

---

## Gotcha 10. Lazy loading after the context is disposed

**Concepts**
- lazy loading after context disposal
- serializer-triggered navigation access
- explicit Include or projection before scope ends

**Answer**

Lazy loading triggers SQL when navigation properties are accessed — if that happens after the request-scoped `DbContext` is disposed, EF Core throws or the serializer triggers hidden queries that fail mid-response. ASP.NET Core disposes scoped contexts at the end of the request pipeline — serialization often runs near that boundary; Prefer explicit includes or projections inside the request scope instead of returning entity graphs with unresolved lazy navigations. Proxy types plus disposed contexts produce intermittent failures depending on property access order.

---

## Scenario-Based Questions (Karat Format)

---

## Q126. (R) An admin "deactivate customer" endpoint loads a `Customer` with `Include(c => c.Orders)` and calls `context.Customers.Remove(customer)` + `SaveChanges`, expecting only the customer row to flip an `IsActive` flag. Support reports missing order history. Review the relationship setup and delete path — what actually happens in SQL, and how do you prevent accidental data loss?

```csharp
// RelationshipsDbContext — unchanged from tutorial
modelBuilder.Entity<Order>(entity =>
{
    entity.HasOne(order => order.Customer)
        .WithMany(customer => customer.Orders)
        .HasForeignKey(order => order.CustomerId)
        .OnDelete(DeleteBehavior.Cascade);
});

// AdminService.cs
public void DeactivateCustomer(RelationshipsDbContext context, int customerId)
{
    var customer = context.Customers
        .Include(c => c.Orders)
        .First(c => c.CustomerId == customerId);
    // TODO: set IsActive = false — never implemented
    context.Customers.Remove(customer);
    context.SaveChanges();
}
```

**Concepts**
- cascade delete removing orders on customer delete
- soft delete vs physical delete mismatch
- DeleteBehavior.Restrict to block accidental removal
- IsActive flag pattern for logical deactivation

**Answer**

`Remove(customer)` is a physical delete, and `DeleteBehavior.Cascade` on `Order`→`Customer` tells SQL Server to DELETE all dependent order rows when the customer row is deleted — not deactivate anything. The missing `IsActive` assignment means the code never implemented soft delete; it wiped the principal and cascaded to dependents exactly as DemonstrateCascadeDelete in Program.cs shows.

1. Implement **soft delete**: add `IsActive` / `DeletedAt`, query with global filter, **never** `Remove` for deactivation — update scalar flags only.
2. If physical delete is required, change to `DeleteBehavior.Restrict` (or `NoAction`) on Customer→Orders so delete fails until orders are archived/moved explicitly.
3. Document cascade paths in `OnModelCreating` — long cascade chains (Customer→Orders→LineItems) multiply surprise deletions.
4. Return order count in admin UI before confirm; use integration tests that assert orders survive deactivation.

---

## Q127. (R) A teammate makes `Order.CustomerId` nullable so "orphan orders" can exist after a customer GDPR erasure, and configures `OnDelete(DeleteBehavior.SetNull)`. Review the model and fluent config — what breaks at migration/runtime, and what is the correct shape for an optional relationship?

```csharp
public sealed class Order
{
    public int OrderId { get; set; }
    public int? CustomerId { get; set; }           // changed from int
    public Customer? Customer { get; set; }        // changed from Customer = null!
    // ...
}

modelBuilder.Entity<Order>(entity =>
{
    entity.HasOne(order => order.Customer)
        .WithMany(customer => customer.Orders)
        .HasForeignKey(order => order.CustomerId)
        .OnDelete(DeleteBehavior.SetNull)
        .IsRequired(false);
});
```

**Concepts**
- nullable CustomerId with SetNull delete behavior
- optional relationship Fluent API shape
- shadow FK vs CLR property for optional relationship
- migration for nullable FK column

**Answer**

The fluent snippet is internally consistent for an optional relationship — nullable FK + `IsRequired(false)` + `SetNull` is the valid combination. The surprise is twofold: (1) if any code still treats `Customer` as required (`null!`, `.Customer.Name` without null check), you get null-reference bugs after GDPR erasure; (2) if someone leaves `CustomerId` as non-nullable `int` while calling `SetNull`, migration or runtime FK constraint creation fails because SQL cannot SET NULL on a NOT NULL column.

1. Align all three: nullable `int? CustomerId`, nullable `Customer?` navigation, `IsRequired(false)`, `OnDelete(SetNull)` — only for truly optional associations.
2. Update LINQ and DTOs for null customer — `order.Customer?.Name ?? "Redacted"`.
3. If orders **must** always have a customer (this chapter's **Order.cs** uses required `int CustomerId`), keep required FK and use **Restrict** or archive workflow instead of SetNull.
4. Add filtered indexes and reporting views for orphan orders if GDPR requires principal deletion while retaining financial records.

---

## Q128. (M) A legacy `Comment` entity has a navigation to `BlogPost` but **no** `BlogPostId` scalar. A developer writes this query and gets a compile error. Explain how EF models the FK, how to query/filter without a CLR property, and one production pitfall when inserting graphs.

```csharp
public sealed class Comment
{
    public int CommentId { get; set; }
    public string Text { get; set; } = string.Empty;
    public BlogPost BlogPost { get; set; } = null!; // no BlogPostId property
}

// Fails to compile:
var comments = context.Comments
    .Where(c => c.BlogPostId == postId)
    .ToList();
```

**Concepts**
- shadow foreign key without CLR scalar property
- filtering by shadow property in queries
- EF.Property<T> for shadow property access
- graph insert without explicit FK assignment

**Answer**

EF Core creates a shadow property — a FK column (`BlogPostId`) in the model and database without a CLR property on `Comment`. You cannot reference `c.BlogPostId` in LINQ because it does not exist on the class; filter via navigation or `EF.Property<int>(c, "BlogPostId")`.

1. **Preferred:** Expose FK explicitly — add `public int BlogPostId { get; set; }` paired with `BlogPost` navigation (same pattern as **Order.CustomerId** in **Models/Order.cs**).
2. **Query without CLR FK:** `context.Comments.Where(c => c.BlogPost.PostId == postId)` or `EF.Property<int>(c, "BlogPostId") == postId`.
3. **Inspect model:** `context.Model.FindEntityType(typeof(Comment))!.FindProperty("BlogPostId")` or migration snapshot to confirm shadow name.
4. **Graph insert pitfall:** Attaching a `Comment` with `BlogPost` navigation pointing to an untracked `BlogPost` stub with wrong key can fix-up incorrectly — assign FK scalar or attach principal first.

---

## Q129. (R) A catalog import links products to tags using the chapter's `ProductTag` join entity. CI passes on 10 rows but production throws on duplicate key. Review the import loop — what relationship rule is violated, and how do you fix idempotent linking when `AddedOn` payload matters?

```csharp
foreach (var row in importRows)
{
    var product = context.Products.First(p => p.Sku == row.Sku);
    var tag = context.Tags.First(t => t.Label == row.TagLabel);

    product.ProductTags.Add(new ProductTag
    {
        Tag = tag,
        AddedOn = row.AddedOn
    });
    context.SaveChanges(); // throws on re-run for same (ProductId, TagId)
}
```

**Concepts**
- duplicate key on ProductTag from non-idempotent import
- join entity insert checking existing link
- AddedOn payload requirement on explicit join entity
- Any check before Add for idempotent linking

**Answer**

`ProductTag` has a composite primary key `(ProductId, TagId)` configured in RelationshipsDbContext — inserting the same pair twice violates the PK on re-run. Per-row `SaveChanges` does not cause the duplicate; re-importing the same SKU/tag pair does. This is why the chapter uses an explicit join entity instead of blind skip navigation when you need payload columns like `AddedOn`.

1. Upsert pattern — query first (as **Program.cs** `DemonstrateManyToManyJoinEntity` lines 127–128): `if (!context.ProductTags.Any(...))` then add.
2. Or load existing link and update `AddedOn` only when the row exists; add when missing.
3. Batch `SaveChanges` once per chunk after deduplicating in memory — not once per row.
4. Keep explicit `ProductTag` entity when payload exists; skip navigation is for link tables **without** extra columns (**RelationshipsDbContext** Section 5b comment).

---

## Q130. (R) A checkout service builds an order graph in one `DbContext` but relationships look wrong after `SaveChanges` — wrong customer on the order, or `InvalidOperationException` about duplicate tracked instances. Review this method — diagnose the fix-up / tracking issues and show the corrected graph insert.

```csharp
public void PlaceOrder(RelationshipsDbContext context, int customerId, decimal total)
{
    var customerA = context.Customers.Find(customerId);
    var customerB = new Customer { CustomerId = customerId, Name = "cached" };

    var order = new Order
    {
        OrderDate = DateOnly.FromDateTime(DateTime.UtcNow),
        TotalAmount = total,
        CustomerId = customerId,
        Customer = customerB   // different instance than customerA
    };

    context.Orders.Add(order);
    context.SaveChanges();
}
```

**Concepts**
- navigation fix-up after SaveChanges with shared context
- duplicate tracked instance causing InvalidOperationException
- graph construction with new vs attached entities
- correct principal-first insert order

**Answer**

The method creates two different `Customer` instances with the same key — `customerA` from `Find` (tracked) and `customerB` (new stub). Setting `order.Customer = customerB` while also setting `CustomerId` triggers EF relationship fix-up and change-tracker conflicts: either duplicate-key tracking exception or fix-up replacing navigations with inconsistent state.

1. **Use one instance** — set `order.Customer = customerA` (from `Find`) and remove redundant `customerB`; or omit `CustomerId` and let EF set FK from navigation.
2. **Graph insert from principal** (tutorial pattern):

```csharp
var customer = context.Customers.Find(customerId)!;
customer.Orders.Add(new Order
{
    OrderDate = DateOnly.FromDateTime(DateTime.UtcNow),
    TotalAmount = total
});
context.SaveChanges();
```

3. Never new-up a principal with an existing key in the same context unless you `Attach` with correct state intentionally.
4. If only FK is known: set `CustomerId` only, leave `Customer` null until loaded — do not assign a second stub instance.

---

## Q131. (D) Product managers want to delete unused `Tag` rows from an admin screen. The model matches **Program.cs Section 6b** — `Product`→`ProductTag` is Cascade, `Tag`→`ProductTag` is Restrict. A delete request returns a SQL FK exception. Explain the delete behavior from the database's perspective, and compare fixing this with Restrict+cleanup vs switching Tag side to Cascade vs soft-delete on tags.

**Concepts**
- Tag delete blocked by Restrict FK from ProductTag
- Cascade vs Restrict trade-off on join entity FK
- soft-delete on Tag vs cascade cleanup comparison
- FK behavior mismatch between EF and database

**Answer**

`DeleteBehavior.Restrict` on `Tag`→`ProductTag` maps to a FK that blocks deleting a Tag while any ProductTag row references it — SQL Server raises a reference constraint error. Deleting a Product still cascades to its ProductTag rows only; shared tags used by other products remain referenced, so Tag delete correctly fails until links are removed.

1. **Restrict + explicit cleanup (recommended for shared tags):** Delete or reassign `ProductTag` rows first, then delete Tag — matches tutorial intent (**Program.cs** lines 223–230: Product delete removes links, Tag survives).
2. **Cascade on Tag side:** Deleting Tag would remove **all** ProductTag links catalog-wide — rarely desired for shared labels like "Sale"; risks accidental mass unlinking.
3. **Soft-delete tags:** `IsActive` flag + filtered queries; keep FK integrity and history; best when tags appear in historical reports.
4. Handle `DbUpdateException` in API → 409 Conflict with "Tag in use by N products."
