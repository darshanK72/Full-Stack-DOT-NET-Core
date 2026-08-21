# Karat — Interview Questions

> **Folder:** `05. ASP.NET Core/13. Minimal APIs`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)  
> **Raw debrief:** [KARAT_DEBRIEF_SOURCE.txt](../../00. Notes & Practice/04. Citi Karat Interview Practice/KARAT_DEBRIEF_SOURCE.txt)

---

#### Q1. (R) Review this minimal API registration. The app starts and passes smoke tests, but order totals drift under concurrent load. What is wrong?

```csharp
var orderTotals = new Dictionary<int, decimal>();

app.MapPost("/orders/{id:int}/lines", async (int id, OrderLine line, AppDbContext db) =>
{
    db.OrderLines.Add(new OrderLineEntity { OrderId = id, Sku = line.Sku, Qty = line.Qty });
    await db.SaveChangesAsync();

    if (!orderTotals.ContainsKey(id))
        orderTotals[id] = 0;
    orderTotals[id] += line.UnitPrice * line.Qty;

    return Results.Ok(new { orderId = id, runningTotal = orderTotals[id] });
});
```

*(Assume `AppDbContext` is scoped and registered correctly.)*

---

#### Q2. (M) A teammate returns `IActionResult` from a minimal API route handler while another uses `Results.Ok()` and `TypedResults.Created()`. When does each approach fit, and what does ASP.NET Core lose when you pick the wrong one?

---

#### Q3. (P) You need request-body validation on a minimal API POST without MVC controllers. How do you validate a DTO and return RFC 7807 `ProblemDetails` on failure using endpoint filters?

---

#### Q4. (R) OpenAPI/Swagger shows every minimal endpoint as `200 OK` with an empty schema, and one POST is missing from the document entirely. Review the setup — what is wrong and how do you fix it for client generation?

```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/reports", async (ReportRequest req, IReportService svc) =>
{
    await svc.EnqueueAsync(req);
    return Results.Accepted();
});

app.MapGet("/reports/{id:guid}", (Guid id, IReportService svc) =>
    svc.GetStatus(id)); // returns anonymous object or DTO

// Dev-only internal endpoint — should not appear in public OpenAPI
app.MapDelete("/admin/purge-cache", () => Results.NoContent())
   .ExcludeFromDescription();
```

*(Focus on metadata, response typing, and document completeness — not generic "install Swashbuckle" advice.)*

---

#### Q5. (P) How do you protect a subset of minimal API endpoints with JWT bearer auth and a named authorization policy while leaving health checks anonymous? Where do you register requirements vs apply them on routes?

---

#### Q6. (D) The team wants URL-based API versioning (`/api/v1/...`, `/api/v2/...`) using `MapGroup` without duplicating middleware and OpenAPI tags. What structure would you use, and what breaks if v1 and v2 share the same route parameter names but different DTO shapes?

---

#### Q7. (R) Review this `Program.cs` excerpt from a production service. What maintainability, testability, and DI problems will appear as the API grows?

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var conn = builder.Configuration.GetConnectionString("Default");
var cache = new MemoryCache(new MemoryCacheOptions());
var http = new HttpClient();

app.MapGet("/customers/{id}", async (string id) =>
{
    if (cache.TryGetValue(id, out Customer? c)) return Results.Ok(c);
    await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlServer(conn).Options);
    c = await db.Customers.FindAsync(id);
    cache.Set(id, c, TimeSpan.FromMinutes(10));
    return c is null ? Results.NotFound() : Results.Ok(c);
});

app.MapPost("/customers", async (CustomerDto dto) =>
{
    var resp = await http.PostAsJsonAsync("https://legacy.example/validate", dto);
    resp.EnsureSuccessStatusCode();
    await using var db = new AppDbContext(/* same inline options */);
    db.Customers.Add(new Customer { Name = dto.Name });
    await db.SaveChangesAsync();
    return Results.Created($"/customers/{dto.Name}", dto);
});

app.Run();
```

---

#### Q8. (M) Minimal APIs resolve route-handler parameters from route values, body, services, and `HttpContext`. A handler injects `IOptions<FeatureFlags>` and `[FromServices] IAuditLogger` alongside `[FromBody] CreateOrderRequest`. What determines injection order and failure modes when a parameter cannot be bound?
