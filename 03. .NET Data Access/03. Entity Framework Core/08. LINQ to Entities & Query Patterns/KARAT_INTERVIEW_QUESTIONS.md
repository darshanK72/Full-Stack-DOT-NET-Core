# Karat — Interview Questions

> **Folder:** `04. .NET Data Access/03. Entity Framework Core/08. LINQ to Entities & Query Patterns`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A catalog API endpoint is slow under load. Review this repository method from the EF Core LINQ chapter's `StoreDbContext` model. What executes on SQL Server vs in the app, and what would you change?

```csharp
public IReadOnlyList<Product> GetExpensiveElectronics(StoreDbContext context)
{
    return context.Products
        .AsEnumerable()
        .Where(p => p.Category!.Name == "Electronics" && p.UnitPrice >= 50m)
        .OrderBy(p => p.Name)
        .ToList();
}
```

---

#### Q2. (R) This search compiles but throws at runtime when hit through the API. Review the query and helper. What failed to translate, and how do you fix it without loading the whole table?

```csharp
public IReadOnlyList<Product> GetPremiumInStock(StoreDbContext context)
{
    return context.Products
        .Where(p => IsPremiumSku(p.Name) && p.StockQuantity > 0)
        .OrderBy(p => p.Name)
        .ToList();
}

private static bool IsPremiumSku(string name) =>
    name.StartsWith("Pro", StringComparison.OrdinalIgnoreCase);
```

---

#### Q3. (R) An order-history endpoint works in dev with 3 seed orders but degrades badly in production. Review this service method. What query pattern causes the regression, and how do you fix it?

```csharp
public List<string> BuildOrderLineSummaries(StoreDbContext context)
{
    List<Order> orders = context.Orders
        .OrderBy(o => o.OrderDate)
        .ToList();

    return orders.Select(o =>
        $"{o.CustomerName} ({o.OrderDate:d}): " +
        string.Join(", ", o.Lines.Select(l => l.Product!.Name)))
        .ToList();
}
```

---

#### Q4. (R) A junior dev refactors the product search to "keep filtering composable" by returning `IQueryable<Product>` from the repository. Under ASP.NET Core scoped `DbContext`, intermittent `ObjectDisposedException` appears in production. Review both layers. What leaked, and what boundary would you enforce?

```csharp
public sealed class ProductRepository
{
    private readonly StoreDbContext _context;

    public ProductRepository(StoreDbContext context) => _context = context;

    public IQueryable<Product> BuildSearchQuery(int? categoryId, decimal? minPrice)
    {
        IQueryable<Product> query = _context.Products;

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (minPrice.HasValue)
            query = query.Where(p => p.UnitPrice >= minPrice.Value);

        return query;
    }
}

public sealed class CatalogService
{
    private readonly ProductRepository _repo;

    public CatalogService(ProductRepository repo) => _repo = repo;

    public IReadOnlyList<Product> GetCatalog(int? categoryId, decimal? minPrice, string sort)
    {
        IQueryable<Product> query = _repo.BuildSearchQuery(categoryId, minPrice);

        query = sort == "price"
            ? query.OrderBy(p => p.UnitPrice)
            : query.OrderBy(p => p.Name);

        return query.ToList(); // sometimes throws after the HTTP request ends
    }
}
```

---

#### Q5. (P) The chapter's `ProjectionQueries.GetProductCatalogItems()` projects to `ProductListItem` in the database. A teammate returns full `Product` entities from the repository and maps to DTOs in the controller with AutoMapper. Both compile. When would you insist on server-side `Select` projection, and what breaks if you skip it?

---

#### Q6. (M) A background job captures an `IQueryable<Product>` during request handling and enumerates it minutes later on a thread-pool thread. The chapter's `IQueryableDemonstrations` stresses deferred execution. Walk through what happens from query construction to `foreach`, and what materialization rule you would enforce in code review.

```csharp
public IQueryable<Product> GetPendingPriceReview(StoreDbContext context, decimal threshold)
{
    return context.Products
        .Where(p => p.UnitPrice >= threshold)
        .OrderBy(p => p.Name);
}

// elsewhere, minutes later:
foreach (Product product in GetPendingPriceReview(context, 75m))
{
    await PublishReviewEventAsync(product);
}
```

---
