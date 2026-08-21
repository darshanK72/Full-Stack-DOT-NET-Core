# Karat — Interview Questions

> **Folder:** `07. ASP.Net Core Web API/13. Integration with EF Core`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)  
> **Cross-ref:** [04. .NET Data Access/03. Entity Framework Core/05. CRUD Operations & SaveChanges](../../04.%20.NET%20Data%20Access/03.%20Entity%20Framework%20Core/05.%20CRUD%20Operations%20&%20SaveChanges/KARAT_INTERVIEW_QUESTIONS.md) (change tracking, SaveChanges, attach vs update)

---

#### Q1. (R) Review this `OrdersController`. QA reports intermittent `DbUpdateConcurrencyException` and slow list endpoints under load.

```csharp
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _db;

    public OrdersController(AppDbContext db) => _db = db;

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> Get(int id)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order is null) return NotFound();
        return Ok(new OrderDto(order.Id, order.Total, order.CustomerId));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderRequest req)
    {
        _db.Orders.Add(new Order { CustomerId = req.CustomerId, Total = req.Total });
        await _db.SaveChangesAsync();
        return Ok();
    }
}
```

The team says "controllers are thin — we just use EF directly." What breaks at scale, and what would you change first?

---

#### Q2. (R) Review this API projection. APM shows 1 query for the order list and 200 extra queries when the endpoint is hit with 100 rows.

```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<OrderSummaryDto>>> List()
{
    var orders = await _db.Orders
        .Where(o => o.Status == OrderStatus.Open)
        .Select(o => new OrderSummaryDto
        {
            Id = o.Id,
            CustomerName = o.Customer.Name,
            LineCount = o.Lines.Count
        })
        .ToListAsync();

    return Ok(orders);
}
```

`Customer` and `Lines` are navigation properties; lazy loading is enabled globally.

---

#### Q3. (R) Review this read-only catalog endpoint. Memory spikes on the API pods during flash sales; GC pauses correlate with `GET /api/products`.

```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts(
    [FromQuery] string? category)
{
    IQueryable<Product> query = _db.Products;

    if (!string.IsNullOrEmpty(category))
        query = query.Where(p => p.Category == category);

    var products = await query
        .Include(p => p.Supplier)
        .Include(p => p.Reviews)
        .ToListAsync();

    return Ok(products.Select(ProductDto.FromEntity));
}
```

No writes occur on this action. `_db` is scoped per request via DI.

---

#### Q4. (R) Review this checkout flow. Money is deducted from inventory but the order row is missing when support queries SQL after a transient failure mid-request.

```csharp
[HttpPost("checkout")]
public async Task<IActionResult> Checkout(CheckoutRequest req)
{
    var product = await _db.Products.FindAsync(req.ProductId);
    if (product!.Stock < req.Quantity) return BadRequest("Out of stock");

    product.Stock -= req.Quantity;
    await _db.SaveChangesAsync();

    _db.Orders.Add(new Order { ProductId = req.ProductId, Qty = req.Quantity });
    await _db.SaveChangesAsync();

    return Ok();
}
```

---

#### Q5. (R) Review this repository and controller pair. Filtering by date works in unit tests (in-memory list) but returns wrong counts in production and sometimes throws after deploy.

```csharp
public class OrderRepository
{
    private readonly AppDbContext _db;
    public OrderRepository(AppDbContext db) => _db = db;

    public IQueryable<Order> GetAll() => _db.Orders.Include(o => o.Lines);
}

[HttpGet]
public ActionResult<IEnumerable<OrderDto>> Search([FromQuery] DateTime since)
{
    var orders = _repo.GetAll()
        .Where(o => o.CreatedUtc >= since)
        .ToList();

    return Ok(orders.Select(OrderDto.FromEntity));
}
```

The controller is synchronous; `_repo.GetAll()` returns `IQueryable<Order>`.

---

#### Q6. (R) Review this paginated list endpoint. Page 2 sometimes repeats rows from page 1; under concurrent inserts, clients see duplicates and gaps.

```csharp
[HttpGet]
public async Task<ActionResult<PagedResult<ProductDto>>> GetPage(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20)
{
    var skip = (page - 1) * pageSize;

    var items = await _db.Products
        .Skip(skip)
        .Take(pageSize)
        .Select(p => new ProductDto(p.Id, p.Name, p.Price))
        .ToListAsync();

    var total = await _db.Products.CountAsync();

    return Ok(new PagedResult<ProductDto>(items, total, page, pageSize));
}
```

---

#### Q7. (P) An API team registers `AppDbContext` as scoped and injects it into controllers, services, and a **Singleton** `PricingCacheWarmupService` that preloads prices at startup. What failure mode appears in production, and what patterns fix EF usage in background work?

---

#### Q8. (D) You inherit an API where every list endpoint returns full EF entities (including navigation graphs) serialized directly to JSON. Product list responses are 2 MB and Swagger shows circular reference warnings. Compare three remediation options and when you would pick each.

---

#### Q9. (M) A teammate proposes `AddDbContextFactory<AppDbContext>()` alongside scoped `AppDbContext` for the same API. Under what request patterns does `IDbContextFactory` help, and when should handlers keep using scoped `DbContext` from DI?

---
