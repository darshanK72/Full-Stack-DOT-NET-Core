# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/02. Object Oriented Programming/03. Constructors & Method Overloading`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A teammate refactors `OrderLine` to chain constructors like the chapter's `Product` type. QA reports invalid lines in production — empty SKU and zero quantity slip through. Review the ctors. What went wrong, and how do you fix it?

```csharp
public sealed class OrderLine
{
    public string Sku { get; }
    public int Quantity { get; }

    public OrderLine()
        : this("MISC", 1)
    {
    }

    public OrderLine(string sku)
    {
        Sku = sku?.Trim() ?? string.Empty;
        Quantity = 1;
    }

    public OrderLine(string sku, int quantity)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU is required.", nameof(sku));
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        Sku = sku.Trim();
        Quantity = quantity;
    }
}
```

---

#### Q2. (R) A .NET 8 service adopts a **primary constructor** for a warehouse DTO. Unit tests expecting `ArgumentException` on bad input fail with `NullReferenceException` instead. Review the type. What is the initialization order problem, and how would you enforce invariants?

```csharp
public sealed class StockReceipt(string sku, decimal unitCost, int quantity)
{
    public string Sku { get; } = sku.Trim();
    public decimal UnitCost { get; } = unitCost;
    public int Quantity { get; } = quantity;

    // Intended guard — runs after field initializers above
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU is required.", nameof(sku));
        if (unitCost < 0m)
            throw new ArgumentOutOfRangeException(nameof(unitCost));
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));
    }
}
```

---

#### Q3. (R) After adding a convenience overload to `LineItemCalculator`-style pricing helpers, `dotnet build` fails with **CS0121** ("The call is ambiguous"). Which overloads conflict, and how do you resolve the call site or signatures?

```csharp
public static class PricingHelper
{
    public static decimal LineTotal(int qty, decimal unitPrice, decimal discountRate = 0m)
    {
        decimal gross = qty * unitPrice;
        return gross - (gross * discountRate);
    }

    public static decimal LineTotal(int qty, decimal unitPrice, decimal discountRate, decimal taxRate)
    {
        decimal net = LineTotal(qty, unitPrice, discountRate);
        return net + (net * taxRate);
    }
}

// Call site in OrderService:
decimal total = PricingHelper.LineTotal(3, 2.49m, 0.10m);
```

---

#### Q4. (M) A junior dev models discounted inventory items by inheriting from `Product` (chapter pattern). `dotnet build` reports **CS2506** and **CS7036**. Diagnose **`: this(...)` vs `: base(...)`** mistakes and state the correct ctor initialization order.

```csharp
public class Product
{
    public Product(string name, decimal unitPrice) { /* validates */ }
    // No parameterless constructor — adding one removed compiler default.
}

public class DiscountedProduct : Product
{
    public decimal DiscountRate { get; }

    // Attempt A — build error CS2506
    public DiscountedProduct(string name, decimal unitPrice, decimal discountRate)
        : base(name, unitPrice)
        : this(name, unitPrice, discountRate, applyMinimum: true)
    {
        DiscountRate = discountRate;
    }

    // Attempt B — would be CS7036 without : base(...)
    public DiscountedProduct(string name, decimal unitPrice, decimal discountRate, bool applyMinimum)
    {
        DiscountRate = discountRate;
    }

    public DiscountedProduct(string name)
        : base(name, 0m)
    {
        DiscountRate = 0.10m;
    }
}
```

---

#### Q5. (P) An ASP.NET Core API maps inbound JSON to a **`required`** init-only request type before calling domain ctors. A client omits `Name` but the payload still deserializes and reaches `new Product(...)`. What happened at compile time vs runtime, and how do you align API contracts with constructor validation?

```csharp
public sealed class CreateProductRequest
{
    public required string Name { get; init; }
    public required decimal UnitPrice { get; init; }
}

public static class ProductFactory
{
    public static Product FromRequest(CreateProductRequest request)
    {
        return new Product(request.Name, request.UnitPrice);
    }
}

// Controller (simplified):
[HttpPost]
public IActionResult Create([FromBody] CreateProductRequest body)
{
    var product = ProductFactory.FromRequest(body);
    return Ok(product.Describe());
}
```

Client POST body:

```json
{ "unitPrice": 8.99 }
```

---

#### Q6. (D) A warehouse microservice registers services in DI but still constructs dependencies manually inside ctors. Review startup and `InventorySyncService`. What breaks in tests, lifetimes, and startup, and what pattern replaces it?

```csharp
// Program.cs
builder.Services.AddSingleton<IInventorySyncService, InventorySyncService>();
builder.Services.AddSingleton<IProductCatalog, ProductCatalog>();

public sealed class InventoryRegistry  // legacy singleton from tutorial
{
    private static readonly InventoryRegistry Shared = new();
    private InventoryRegistry() { }
    public static InventoryRegistry Instance => Shared;
    public void Register(Product p) { /* ... */ }
}

public sealed class ProductCatalog : IProductCatalog
{
    private readonly List<Product> _products = new();

    public ProductCatalog()
    {
        _products.Add(new Product("Seed SKU", -1.00m)); // negative price
    }
}

public sealed class InventorySyncService : IInventorySyncService
{
    private readonly InventoryRegistry _registry;

    public InventorySyncService()
    {
        _registry = InventoryRegistry.Instance;
    }

    public void Sync(Product product) => _registry.Register(product);
}
```

What would you change in registration, ctor signatures, and object creation?

---
