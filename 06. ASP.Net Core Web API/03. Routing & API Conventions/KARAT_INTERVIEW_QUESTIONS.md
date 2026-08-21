# Karat — Interview Questions

> **Folder:** `07. ASP.Net Core Web API/03. Routing & API Conventions`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)  
> **Cross-ref:** [05. ASP.NET Core/07. Routing & Endpoints](../../05.%20ASP.NET%20Core/07.%20Routing%20&%20Endpoints/KARAT_INTERVIEW_QUESTIONS.md)

---

#### Q1. (R) Review this controller after a rename from `CustomerController` to `ClientsController`. Partner calls to `POST /api/customers` return 404; Swagger still lists `/api/Clients`.

```csharp
[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    [HttpPost]
    public ActionResult<ClientDto> Create([FromBody] CreateClientDto dto) { /* ... */ }
}
```

Partners were onboarded with lowercase `/api/customers` from the old controller name. No `[Route("api/customers")]` override exists.

---

#### Q2. (R) Review ambiguous attribute routes. `GET /api/products/10` intermittently hits the wrong action depending on build order.

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id) => Ok(_repo.Get(id));

    [HttpGet("{category}")]
    public IActionResult GetByCategory(string category) => Ok(_repo.ByCategory(category));

    [HttpGet("featured")]
    public IActionResult Featured() => Ok(_repo.Featured());
}
```

Request `GET /api/products/featured` works; `GET /api/products/10` sometimes binds `category = "10"`.

---

#### Q3. (R) Review RESTful route design for a code review. Which templates would you reject before merge?

```csharp
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    [HttpPost("GenerateInvoice")]
    public IActionResult Generate([FromBody] GenerateDto dto) => Ok();

    [HttpGet("GetInvoice/{invoiceId}")]
    public IActionResult GetInvoice(int invoiceId) => Ok();

    [HttpPost("{invoiceId}/SendEmail")]
    public IActionResult SendEmail(int invoiceId) => Ok();
}
```

---

#### Q4. (M) After enabling lowercase URLs (`RouteOptions.LowercaseUrls = true`), `CreatedAtAction` generates `Location: /api/orders/5` but the gateway publicly exposes `/api/v1/store/orders/5`. What routing and link-generation pieces are missing?

```csharp
// Program.cs excerpt
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddControllers();
var app = builder.Build();
app.UsePathBase("/api/v1/store");
app.MapControllers();
```

---

#### Q5. (D) A junior developer proposes conventional routing for a new public JSON API because "Startup.cs tutorials use `MapControllerRoute`." Argue for or against attribute routing for Web APIs in ASP.NET Core 8.

---

#### Q6. (R) Review `[ApiController]` route prefix behavior. Integration tests call `/api/v1/orders` but receive 404 — unit tests on the controller pass.

```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class OrdersController : ControllerBase
{
    [HttpGet("{id:int}")]
    public IActionResult Get(int id) => Ok(_orders.Get(id));
}

// Test project helper:
var client = _factory.CreateClient();
var response = await client.GetAsync("/api/orders/1");
```

No global route prefix configured in `Program.cs`.

---

#### Q7. (R) Review duplicate HTTP method registration. Swagger shows two operations for `DELETE /api/items/{id}`; one returns 405 in production.

```csharp
[Route("api/[controller]")]
[ApiController]
public class ItemsController : ControllerBase
{
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id) { _repo.Delete(id); return NoContent(); }

    [HttpDelete("{id}")]
    public IActionResult Remove(string id) { _repo.Delete(int.Parse(id)); return NoContent(); }
}
```

---

#### Q8. (R) Review nested resource routes and link generation for a parent/child REST surface.

```csharp
[Route("api/customers/{customerId:int}/orders")]
[ApiController]
public class CustomerOrdersController : ControllerBase
{
    [HttpPost]
    public ActionResult<OrderDto> Create(int customerId, [FromBody] CreateOrderDto dto)
    {
        var order = _svc.Create(customerId, dto);
        return CreatedAtAction(nameof(Get), new { orderId = order.Id }, order);
    }

    [HttpGet("{orderId:int}")]
    public ActionResult<OrderDto> Get(int customerId, int orderId) => Ok(_svc.Get(customerId, orderId));
}
```

`CreatedAtAction` Location returns `/api/customers/3/orders?orderId=7` without the order segment.
