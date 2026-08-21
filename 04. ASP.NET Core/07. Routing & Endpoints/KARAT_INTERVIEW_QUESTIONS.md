# Karat — Interview Questions

> **Folder:** `05. ASP.NET Core/07. Routing & Endpoints`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)  
> **Raw debrief:** [KARAT_DEBRIEF_SOURCE.txt](../../00. Notes & Practice/04. Citi Karat Interview Practice/KARAT_DEBRIEF_SOURCE.txt)

---

#### Q1. (P) When an API creates a new resource and returns HTTP 201 Created, how should it include the newly created resource and its `Location` header? Compare MVC `CreatedAtAction` with minimal API `Results.Created`.

---

#### Q2. (R) Review this POST endpoint. Integration tests report `Location: http://localhost/api/orders/42` but production clients receive a broken link.

```csharp
[HttpPost]
public async Task<ActionResult<OrderDto>> Create(CreateOrderRequest request, CancellationToken ct)
{
    var order = await _service.CreateAsync(request, ct);
    return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
}

[HttpGet("{id:int}")]
public async Task<ActionResult<OrderDto>> GetById(int id, CancellationToken ct)
    => Ok(await _service.GetAsync(id, ct));
```

Production runs behind nginx TLS termination; `UseForwardedHeaders` is registered **after** `MapControllers`. Swagger "Try it" shows correct HTTPS locally.

---

#### Q3. (M) A codebase mixes `[Route("api/[controller]")]` on controllers with `app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}")`. Explain how endpoint routing merges attribute and conventional routes, and when you would standardize on one approach for a greenfield API.

---

#### Q4. (R) Review these two controller actions. `GET /api/products/sale` intermittently returns the wrong handler's response after deploy.

```csharp
[HttpGet("{category}")]
public IActionResult GetByCategory(string category) => Ok(_repo.ByCategory(category));

[HttpGet("sale")]
public IActionResult GetSaleItems() => Ok(_repo.SaleItems());
```

Route order looks fine in source control; both actions live on `ProductsController` with `[Route("api/[controller]")]`.

---

#### Q5. (R) Review route constraints and binding. Clients call `GET /api/users/not-a-guid` and receive 500 instead of 404.

```csharp
[HttpGet("{id:guid}")]
public async Task<ActionResult<UserDto>> GetUser(Guid id, CancellationToken ct)
{
    var user = await _users.FindAsync(id, ct);
    if (user is null) return NotFound();
    return Ok(user);
}
```

Global exception middleware logs `FormatException` from model binding.

---

#### Q6. (P) An SPA behind Cloudflare calls your API's OpenAPI-generated client; pagination links in responses use `http://` while the browser page is `https://`. Which routing/link-generation inputs must be correct, and where does forwarded header middleware belong?

---

#### Q7. (R) Review this minimal API layout. `GET /api/v2/customers/5/invoices` returns 404 but `GET /api/customers/5/invoices` works.

```csharp
var api = app.MapGroup("/api");

var customers = api.MapGroup("/customers");
customers.MapGet("/{id:int}", (int id) => Results.Ok(GetCustomer(id)));

var v2 = api.MapGroup("/v2");
var v2Customers = v2.MapGroup("/customers");
v2Customers.MapGet("/{id:int}/invoices", (int id) => Results.Ok(GetInvoices(id)));
```

---

#### Q8. (M) Two endpoints match the same HTTP method and path template after a refactor — startup throws `AmbiguousMatchException`. What tools or configuration surface the conflict, and how do route precedence and specificity resolve `{id}` vs `{id:int}` vs literal segments?

---

#### Q9. (D) A team debates `MapGet` minimal endpoints vs attribute-routed controllers for a new internal admin API. Trade-offs for link generation, filters, versioning, OpenAPI, and testability — what would you choose and why?
