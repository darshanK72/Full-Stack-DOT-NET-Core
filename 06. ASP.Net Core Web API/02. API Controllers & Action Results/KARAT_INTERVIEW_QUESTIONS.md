# Karat — Interview Questions

> **Folder:** `07. ASP.Net Core Web API/02. API Controllers & Action Results`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)  
> **Raw debrief:** [KARAT_DEBRIEF_SOURCE.txt](../../00. Notes & Practice/04. Citi Karat Interview Practice/KARAT_DEBRIEF_SOURCE.txt)

---

#### Q1. (R) Review this controller action under load. Integration tests pass locally; production threads spike and requests time out under concurrent traffic.

```csharp
[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var report = _reportService.GenerateAsync(id).Result;
        return Ok(report);
    }
}
```

---

#### Q2. (R) Review resource creation. Mobile clients create accounts successfully but cannot find the new user URI for subsequent PATCH calls — OpenAPI documents `201` with a Location header.

```csharp
[HttpPost]
public IActionResult Create([FromBody] CreateUserDto dto)
{
    var user = _users.Create(dto);
    return Ok(user);
}
```

`UsersController` uses `[Route("api/[controller]")]`; action name is `Create`.

---

#### Q3. (R) Review `CreatedAtAction` usage. After deploy, the Location header points to `GET /api/users/GetUser/5` which returns 404.

```csharp
[HttpPost]
public ActionResult<UserDto> Create([FromBody] CreateUserDto dto)
{
    var created = _users.Create(dto);
    return CreatedAtAction(
        nameof(GetUser),
        new { id = created.Id },
        created);
}

[HttpGet("{id:int}")]
public ActionResult<UserDto> GetById(int id) => Ok(_users.Get(id));
```

---

#### Q4. (P) When should a production Web API action return `ActionResult<T>` vs `IActionResult`, and how does that choice affect OpenAPI schema generation and unit testing?

---

#### Q5. (R) Review DELETE and update responses. Cache invalidation middleware keys on status code; QA reports stale list pages after delete.

```csharp
[HttpDelete("{id:int}")]
public IActionResult Delete(int id)
{
    _repo.Delete(id);
    return Ok(new { deleted = true, id });
}

[HttpPut("{id:int}")]
public IActionResult Replace(int id, [FromBody] UpdateDto dto)
{
    _repo.Replace(id, dto);
    return Ok();
}
```

---

#### Q6. (R) Review validation behavior. Frontend sends invalid JSON bodies but receives `200`-series responses with null fields processed as defaults.

```csharp
public class ProductsController : ControllerBase
{
    [HttpPost]
    public IActionResult Create([FromBody] CreateProductRequest request)
    {
        var product = _svc.Create(request);
        return Ok(product);
    }
}

public class CreateProductRequest
{
    [Required] public string Sku { get; set; } = "";
    [Range(0.01, 100000)] public decimal Price { get; set; }
}
```

No `[ApiController]` on the class; `[Required]` attributes present on the model.

---

#### Q7. (D) Review this "thin controller" refactor proposal. The interviewer asks whether moving logic to the service layer actually fixed the design problems.

```csharp
[HttpPost("checkout")]
public async Task<ActionResult<OrderDto>> Checkout([FromBody] CheckoutRequest req)
{
    if (req.Items.Count == 0) return BadRequest("No items");
    if (!_inventory.HasStock(req.Items)) return Conflict("Out of stock");
    if (!_payments.Authorize(req.PaymentToken, req.Total)) return PaymentRequired();
    var order = await _orders.PlaceAsync(req);
    await _email.SendConfirmationAsync(order);
    await _analytics.TrackPurchaseAsync(order);
    return CreatedAtAction(nameof(Get), new { id = order.Id }, order.ToDto());
}
```

All dependencies are injected; no `[Authorize]` yet.

---

#### Q8. (R) Review return types and leaked domain models. Security scan flags internal fields in JSON responses.

```csharp
[HttpGet("{id:int}")]
public IActionResult Get(int id)
{
    var entity = _db.Orders
        .Include(o => o.InternalNotes)
        .Include(o => o.PaymentAudit)
        .FirstOrDefault(o => o.Id == id);

    if (entity is null) return NotFound();
    return Ok(entity);
}
```

Entity types map 1:1 to EF Core tables; no `[JsonIgnore]` or DTO projection.

---

#### Q9. (M) Compare `CreatedAtAction`, `CreatedAtRoute`, and `Created(uri, value)` for a multi-tenant API where the public URL is `https://api.example.com/tenant/{tenantId}/orders/{id}`. Which helper survives renamed actions and attribute route refactors?

```csharp
[Route("api/{tenantId}/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    [HttpPost]
    public ActionResult<OrderDto> Create(string tenantId, [FromBody] CreateOrderDto dto) { /* ... */ }
}
```
