# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/04. Functional Style Programming/05. Func Action & Predicate`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A warehouse API reuses a shared filter delegate across `List<T>` and LINQ. The build fails after a refactor. What is wrong, and how do you fix it without duplicating filter logic?

```csharp
public class InventoryService
{
    private readonly Func<Product, bool> _inStockFilter = p => p.IsActive && p.StockQty > 0;

    public List<Product> GetPickList(List<Product> items) =>
        items.FindAll(_inStockFilter); // CS1503

    public IEnumerable<Product> GetPickListLinq(IEnumerable<Product> items) =>
        items.Where(_inStockFilter);
}
```

---

#### Q2. (R) A teammate wires logging callbacks into a pick-list pipeline. Review the registration and invocation:

```csharp
public static void ProcessPickList(
    List<Product> items,
    Func<Product, decimal> lineTotal,
    Func<string> logHeader)   // intended: print banner once, return nothing
{
    logHeader(); // CS0029 — cannot convert void to decimal
    items.ForEach(p => Console.WriteLine($"{p.Sku}: {lineTotal(p):C}"));
}

// Startup:
ProcessPickList(
    catalog,
    CalculateLineTotal,
    () => Console.WriteLine("=== Pick list ==="));
```

What are the compile-time mistakes, and which built-in delegate types belong here?

---

#### Q3. (R) After making a filter optional, production throws intermittently when a branch has no active rule. Review:

```csharp
public IEnumerable<Product> FilterCatalog(
    IEnumerable<Product> items,
    Func<Product, bool>? rule)
{
    return items.Where(rule); // sometimes NullReferenceException at runtime
}

// Caller when no custom rule configured:
var visible = FilterCatalog(catalog, null);
```

What breaks at runtime, why does it pass some code paths, and what is the production-safe fix?

---

#### Q4. (P) An ASP.NET Core app registers a `Func<IServiceProvider, decimal>` factory in DI to read tax rate per request. Review startup:

```csharp
builder.Services.AddSingleton<Func<IServiceProvider, decimal>>(sp =>
{
    var options = sp.GetRequiredService<IOptions<TaxOptions>>();
    return () => options.Value.Rate; // Func<decimal> closed over IOptions snapshot
});

builder.Services.AddScoped<ProductPricingService>();

public class ProductPricingService
{
    private readonly Func<decimal> _taxRate;

    public ProductPricingService(Func<decimal> taxRate) => _taxRate = taxRate;

    public decimal PriceWithTax(Product p) => p.UnitPrice * (1m + _taxRate());
}
```

What lifetime and resolution problems appear under load or with `IOptionsMonitor`, and how should factories be registered instead?

---

#### Q5. (R) A pricing service accepts `Func<Product, decimal>` so callers can plug in "async catalog lookups." Review usage from a minimal API endpoint:

```csharp
public decimal GetExtendedPrice(Product p, Func<Product, decimal> unitPriceLookup)
{
    decimal unit = unitPriceLookup(p); // blocks
    return unit * 1.0825m;
}

app.MapGet("/price/{sku}", async (string sku, CatalogClient catalog) =>
{
    Func<Product, decimal> lookup = product =>
        catalog.GetUnitPriceAsync(product.Sku).Result;

    var product = new Product(sku, "Item", 0m, 1, true);
    return Results.Ok(pricing.GetExtendedPrice(product, lookup));
});
```

What are the async, scalability, and delegate-signature problems, and what signature should replace `Func<Product, decimal>`?

---

#### Q6. (D) A team replaces every inventory rule interface with `Func<Product, bool>` parameters "to reduce boilerplate." Tests now require copying lambdas from production code. Compare:

```csharp
// Before
public interface IProductFilter { bool Include(Product p); }

// After
public class InventoryReport
{
    public decimal TotalValue(IEnumerable<Product> items, Func<Product, bool> include) { /* … */ }
}
```

What testability and design seams do you lose, and when is `Func<Product, bool>` still the right API surface?

---

#### Q7. (P) An API adds a custom endpoint filter using a predicate delegate. Review registration and behavior:

```csharp
builder.Services.AddSingleton<Func<HttpContext, bool>>(_ =>
    ctx => ctx.Request.Headers.ContainsKey("X-Warehouse-Id"));

app.MapGet("/pick-list", (IInventoryService svc) => svc.GetPickList())
   .AddEndpointFilter(async (ctx, next) =>
   {
       var gate = ctx.HttpContext.RequestServices
           .GetRequiredService<Func<HttpContext, bool>>();

       if (!gate(ctx.HttpContext))
           return Results.Unauthorized();

       return await next(ctx);
   });
```

What breaks for multi-tenant routing, testing, and filter ordering compared to `IEndpointFilter` or a typed authorization requirement?

---

#### Q8. (M) A generic helper tries to widen a discontinued-SKU predicate for use on the full catalog. Review:

```csharp
public sealed class DiscontinuedProduct : Product
{
    public DateTime? EndOfLifeDate { get; init; }
}

Predicate<DiscontinuedProduct> discontinuedOnly =
    p => p.EndOfLifeDate.HasValue;

Predicate<Product> catalogFilter = discontinuedOnly; // CS0029

List<Product> items = GetFullCatalog();
items.RemoveAll(catalogFilter);
```

Why does assignment fail despite `DiscontinuedProduct : Product`, and how does delegate variance differ from `IEnumerable<T>` assignment?
