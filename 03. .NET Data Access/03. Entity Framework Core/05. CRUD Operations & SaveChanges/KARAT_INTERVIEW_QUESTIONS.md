# Karat — Interview Questions

> **Folder:** `04. .NET Data Access/03. Entity Framework Core/05. CRUD Operations & SaveChanges`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) An API endpoint maps a JSON body to `Product`, assigns `ProductId` from the route, changes `UnitPrice`, and calls `SaveChanges`. The response is 200 but the price in SQL is unchanged. Review this service method — what is wrong and how do you fix it?

```csharp
public void PatchPrice(AppDbContext context, int productId, decimal newPrice)
{
    var stub = new Product
    {
        ProductId = productId,
        UnitPrice = newPrice
    };
    // stub built from DTO — not loaded from context
    context.SaveChanges();
}
```

---

#### Q2. (R) A teammate "fixes" detached updates by always calling `DbSet.Update()`. Partial PATCH requests now wipe columns the client did not send. Review this controller helper — what breaks, and what pattern fixes it without overwriting untouched fields?

```csharp
public async Task UpdateFromDto(AppDbContext context, ProductDto dto)
{
    var entity = new Product
    {
        ProductId = dto.Id,
        ProductName = dto.Name,
        UnitPrice = dto.Price
        // StockQuantity and IsDiscontinued not in DTO — default to 0 / false
    };
    context.Products.Update(entity);
    await context.SaveChangesAsync();
}
```

---

#### Q3. (M) This code mirrors **Program.cs Section 5b** but skips `ChangeTracker.Clear()`. What exception or silent failure do you expect at runtime, and what are two safe ways to update a detached stub when another instance might already be tracked?

```csharp
public void RenameProduct(AppDbContext context, int id, string newName)
{
    Product? tracked = context.Products.Find(id);
    tracked!.ProductName = "stale name from earlier step"; // still Modified in tracker

    var stub = new Product { ProductId = id, ProductName = newName, UnitPrice = 9.99m };
    context.Products.Update(stub);
    context.SaveChanges();
}
```

---

#### Q4. (P) A catalog import reads 10,000 CSV rows and persists each with `Add` + `SaveChanges` inside the loop. It works on 50 rows in QA but times out in production. What is wrong with the persistence pattern, and what would you change?

```csharp
foreach (var row in csvRows)
{
    context.Products.Add(MapRow(row));
    context.SaveChanges(); // one round trip per row
}
```

---

#### Q5. (P) Two admins edit the same product. Admin A loads price 10.00, Admin B loads price 10.00; B saves 12.00, then A saves 11.00. No error is thrown. The `Product` entity has no concurrency column. What data-loss scenario is this, and how would you add optimistic concurrency in EF Core?

---

#### Q6. (R) A bulk pricing endpoint accepts client-supplied `UnitPrice` and `StockQuantity`, calls `AddProduct` + `SaveChanges`, and returns the new `ProductId`. Negative prices and zero stock slip through to SQL. Review this flow — what is missing before `SaveChanges`, and where should validation live in a real API?

```csharp
public Product AddProduct(string name, decimal unitPrice, int stockQuantity)
{
    var product = new Product
    {
        ProductName = name,
        UnitPrice = unitPrice,
        StockQuantity = stockQuantity
    };
    _context.Products.Add(product);
    return product;
}

// caller
var created = service.AddProduct(dto.Name, dto.UnitPrice, dto.StockQuantity);
service.SaveChanges();
return Ok(created.ProductId);
```

---
