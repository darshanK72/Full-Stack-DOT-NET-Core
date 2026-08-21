# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `04. .NET Data Access/03. Entity Framework Core/06. Relationships & Navigation Properties`

---

#### Q1. (R) An admin "deactivate customer" endpoint loads a `Customer` with `Include(c => c.Orders)` and calls `context.Customers.Remove(customer)` + `SaveChanges`, expecting only the customer row to flip an `IsActive` flag. Support reports missing order history. Review the relationship setup and delete path — what actually happens in SQL, and how do you prevent accidental data loss?

**Answer:** `Remove(customer)` is a **physical delete**, and `DeleteBehavior.Cascade` on `Order`→`Customer` tells SQL Server to **DELETE all dependent order rows** when the customer row is deleted — not deactivate anything. The missing `IsActive` assignment means the code never implemented soft delete; it wiped the principal and cascaded to dependents exactly as **DemonstrateCascadeDelete** in **Program.cs** shows.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Delete behavior | `OnDelete(Cascade)` on Customer→Orders | All order history deleted with customer |
| Intent vs API | `Remove()` used for "deactivate" | Irreversible data loss; audit/compliance failure |
| Design | No soft-delete column or Restrict | Business rule "keep orders" incompatible with cascade |

**Fix (priority order):**

1. Implement **soft delete**: add `IsActive` / `DeletedAt`, query with global filter, **never** `Remove` for deactivation — update scalar flags only.
2. If physical delete is required, change to `DeleteBehavior.Restrict` (or `NoAction`) on Customer→Orders so delete fails until orders are archived/moved explicitly.
3. Document cascade paths in `OnModelCreating` — long cascade chains (Customer→Orders→LineItems) multiply surprise deletions.
4. Return order count in admin UI before confirm; use integration tests that assert orders survive deactivation.

**Production takeaway:** EF defaults and explicit **Cascade** are convenient for graph cleanup in demos (**Program.cs** lines 200–205) but dangerous when product language says "deactivate." Prefer Restrict + explicit cleanup or soft-delete for customer/principal entities with valuable dependents.

---

#### Q2. (R) A teammate makes `Order.CustomerId` nullable so "orphan orders" can exist after a customer GDPR erasure, and configures `OnDelete(DeleteBehavior.SetNull)`. Review the model and fluent config — what breaks at migration/runtime, and what is the correct shape for an optional relationship?

**Answer:** The fluent snippet is internally consistent for an **optional** relationship — nullable FK + `IsRequired(false)` + `SetNull` is the valid combination. The surprise is twofold: (1) if any code still treats `Customer` as required (`null!`, `.Customer.Name` without null check), you get null-reference bugs after GDPR erasure; (2) if someone leaves `CustomerId` as non-nullable `int` while calling `SetNull`, migration or runtime FK constraint creation fails because SQL cannot SET NULL on a NOT NULL column.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Required vs optional | Mixing `int` FK with `SetNull` | Migration/DB error: cannot null non-nullable column |
| Semantics | Optional FK without null-safe queries | `Include` + dereference throws after customer deleted |
| GDPR | SetNull orphans orders | Correct for retention policy but breaks reports keyed by customer |

**Fix (priority order):**

1. Align all three: nullable `int? CustomerId`, nullable `Customer?` navigation, `IsRequired(false)`, `OnDelete(SetNull)` — only for truly optional associations.
2. Update LINQ and DTOs for null customer — `order.Customer?.Name ?? "Redacted"`.
3. If orders **must** always have a customer (this chapter's **Order.cs** uses required `int CustomerId`), keep required FK and use **Restrict** or archive workflow instead of SetNull.
4. Add filtered indexes and reporting views for orphan orders if GDPR requires principal deletion while retaining financial records.

**Production takeaway:** **SetNull applies only to optional FKs** — see **Program.cs** quick reference (`SetNull — optional FK only`). Required relationships default to Cascade on SQL Server; making FK nullable is a domain decision, not just a delete-behavior tweak.

---

#### Q3. (M) A legacy `Comment` entity has a navigation to `BlogPost` but **no** `BlogPostId` scalar. A developer writes this query and gets a compile error. Explain how EF models the FK, how to query/filter without a CLR property, and one production pitfall when inserting graphs.

**Answer:** EF Core creates a **shadow property** — a FK column (`BlogPostId`) in the model and database without a CLR property on `Comment`. You cannot reference `c.BlogPostId` in LINQ because it does not exist on the class; filter via navigation or `EF.Property<int>(c, "BlogPostId")`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Model | No scalar FK on entity | Compile error on direct property access |
| Query | Shadow FK invisible in C# | Developers guess wrong column name |
| Graph insert | Set navigation only on one side | Usually works; setting mismatched shadow FK via entry API causes silent wrong links |

**Fix (priority order):**

1. **Preferred:** Expose FK explicitly — add `public int BlogPostId { get; set; }` paired with `BlogPost` navigation (same pattern as **Order.CustomerId** in **Models/Order.cs**).
2. **Query without CLR FK:** `context.Comments.Where(c => c.BlogPost.PostId == postId)` or `EF.Property<int>(c, "BlogPostId") == postId`.
3. **Inspect model:** `context.Model.FindEntityType(typeof(Comment))!.FindProperty("BlogPostId")` or migration snapshot to confirm shadow name.
4. **Graph insert pitfall:** Attaching a `Comment` with `BlogPost` navigation pointing to an untracked `BlogPost` stub with wrong key can fix-up incorrectly — assign FK scalar or attach principal first.

**Production takeaway:** Shadow FKs work for reverse-engineered schemas but hurt maintainability in team codebases. Explicit `{Navigation}Id` scalars match this chapter's convention and avoid "property does not exist" failures in LINQ projections and APIs.

---

#### Q4. (R) A catalog import links products to tags using the chapter's `ProductTag` join entity. CI passes on 10 rows but production throws on duplicate key. Review the import loop — what relationship rule is violated, and how do you fix idempotent linking when `AddedOn` payload matters?

**Answer:** `ProductTag` has a **composite primary key** `(ProductId, TagId)` configured in **RelationshipsDbContext** — inserting the same pair twice violates the PK on re-run. Per-row `SaveChanges` does not cause the duplicate; re-importing the same SKU/tag pair does. This is why the chapter uses an explicit join entity instead of blind skip navigation when you need payload columns like `AddedOn`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Cardinality | Duplicate `(ProductId, TagId)` | `DbUpdateException` on PK violation |
| Idempotency | No existence check before Add | Re-run imports fail in production |
| Join entity | Payload on link row | Cannot replace with naive skip M2M without losing `AddedOn` |

**Fix (priority order):**

1. Upsert pattern — query first (as **Program.cs** `DemonstrateManyToManyJoinEntity` lines 127–128): `if (!context.ProductTags.Any(...))` then add.
2. Or load existing link and update `AddedOn` only when the row exists; add when missing.
3. Batch `SaveChanges` once per chunk after deduplicating in memory — not once per row.
4. Keep explicit `ProductTag` entity when payload exists; skip navigation is for link tables **without** extra columns (**RelationshipsDbContext** Section 5b comment).

**Production takeaway:** Many-to-many join entities inherit **relational PK rules** — one row per pair. Production imports must be idempotent; the tutorial demo already guards with `linkExists` before insert.

---

#### Q5. (R) A checkout service builds an order graph in one `DbContext` but relationships look wrong after `SaveChanges` — wrong customer on the order, or `InvalidOperationException` about duplicate tracked instances. Review this method — diagnose the fix-up / tracking issues and show the corrected graph insert.

**Answer:** The method creates **two different `Customer` instances with the same key** — `customerA` from `Find` (tracked) and `customerB` (new stub). Setting `order.Customer = customerB` while also setting `CustomerId` triggers EF **relationship fix-up** and change-tracker conflicts: either duplicate-key tracking exception or fix-up replacing navigations with inconsistent state.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Change tracker | Two instances, same `CustomerId` | `InvalidOperationException` on track/fix-up |
| Graph consistency | FK scalar + navigation point at different instances | Undefined which customer wins after SaveChanges |
| Pattern | Manual stub instead of navigation graph | Breaks **Program.cs** Section 2–3 graph-insert pattern |

**Fix (priority order):**

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

**Production takeaway:** EF **fix-up** wires navigations and FKs bidirectionally for tracked graphs — mixed instances with the same key are a common Karat trap. Follow **DemonstrateOneToManyGraphInsert** in **Program.cs**: one tracked graph, EF sets `Order.CustomerId` on save.

---

#### Q6. (D) Product managers want to delete unused `Tag` rows from an admin screen. The model matches **Program.cs Section 6b** — `Product`→`ProductTag` is Cascade, `Tag`→`ProductTag` is Restrict. A delete request returns a SQL FK exception. Explain the delete behavior from the database's perspective, and compare fixing this with Restrict+cleanup vs switching Tag side to Cascade vs soft-delete on tags.

**Answer:** `DeleteBehavior.Restrict` on `Tag`→`ProductTag` maps to a FK that **blocks deleting a Tag while any ProductTag row references it** — SQL Server raises a reference constraint error. Deleting a **Product** still cascades to its ProductTag rows only; shared tags used by other products remain referenced, so Tag delete correctly fails until links are removed.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Delete behavior | Restrict on Tag side | Admin delete Tag fails while links exist |
| UX | UI offers "delete tag" without unlink | 500 / unhandled DbUpdateException |
| Shared tags | Tag used by many products | Physical delete affects catalog semantics |

**Fix (priority order):**

1. **Restrict + explicit cleanup (recommended for shared tags):** Delete or reassign `ProductTag` rows first, then delete Tag — matches tutorial intent (**Program.cs** lines 223–230: Product delete removes links, Tag survives).
2. **Cascade on Tag side:** Deleting Tag would remove **all** ProductTag links catalog-wide — rarely desired for shared labels like "Sale"; risks accidental mass unlinking.
3. **Soft-delete tags:** `IsActive` flag + filtered queries; keep FK integrity and history; best when tags appear in historical reports.
4. Handle `DbUpdateException` in API → 409 Conflict with "Tag in use by N products."

**Production takeaway:** Many-to-many with explicit join entity gives **independent delete rules per FK** — Product cascade vs Tag restrict is deliberate (**RelationshipsDbContext** lines 90–98). Karat tests whether you read cascade direction per relationship, not "cascade everything." Shared dimension rows (tags, categories) almost always need Restrict or soft-delete, not cascade from the lookup table.

---
