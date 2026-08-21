# Karat — Interview Questions

> **Folder:** `04. .NET Data Access/03. Entity Framework Core/06. Relationships & Navigation Properties`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) An admin "deactivate customer" endpoint loads a `Customer` with `Include(c => c.Orders)` and calls `context.Customers.Remove(customer)` + `SaveChanges`, expecting only the customer row to flip an `IsActive` flag. Support reports missing order history. Review the relationship setup and delete path — what actually happens in SQL, and how do you prevent accidental data loss?

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

---

#### Q2. (R) A teammate makes `Order.CustomerId` nullable so "orphan orders" can exist after a customer GDPR erasure, and configures `OnDelete(DeleteBehavior.SetNull)`. Review the model and fluent config — what breaks at migration/runtime, and what is the correct shape for an optional relationship?

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

---

#### Q3. (M) A legacy `Comment` entity has a navigation to `BlogPost` but **no** `BlogPostId` scalar. A developer writes this query and gets a compile error. Explain how EF models the FK, how to query/filter without a CLR property, and one production pitfall when inserting graphs.

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

---

#### Q4. (R) A catalog import links products to tags using the chapter's `ProductTag` join entity. CI passes on 10 rows but production throws on duplicate key. Review the import loop — what relationship rule is violated, and how do you fix idempotent linking when `AddedOn` payload matters?

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

---

#### Q5. (R) A checkout service builds an order graph in one `DbContext` but relationships look wrong after `SaveChanges` — wrong customer on the order, or `InvalidOperationException` about duplicate tracked instances. Review this method — diagnose the fix-up / tracking issues and show the corrected graph insert.

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

---

#### Q6. (D) Product managers want to delete unused `Tag` rows from an admin screen. The model matches **Program.cs Section 6b** — `Product`→`ProductTag` is Cascade, `Tag`→`ProductTag` is Restrict. A delete request returns a SQL FK exception. Explain the delete behavior from the database's perspective, and compare fixing this with Restrict+cleanup vs switching Tag side to Cascade vs soft-delete on tags.

---
