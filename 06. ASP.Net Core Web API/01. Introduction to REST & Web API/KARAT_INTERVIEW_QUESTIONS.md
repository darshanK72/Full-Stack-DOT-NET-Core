# Karat — Interview Questions

> **Folder:** `07. ASP.Net Core Web API/01. Introduction to REST & Web API`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)  
> **Raw debrief:** [KARAT_DEBRIEF_SOURCE.txt](../../00. Notes & Practice/04. Citi Karat Interview Practice/KARAT_DEBRIEF_SOURCE.txt)

---

#### Q1. (R) Review this "RESTful" order API. QA reports duplicate charges when users refresh the browser after checkout. Which REST constraints are violated and what status codes should change?

```csharp
[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    [HttpGet("checkout")]
    public IActionResult Checkout([FromQuery] int customerId, [FromQuery] decimal amount)
    {
        var order = _orders.Charge(customerId, amount);
        return Ok(order);
    }
}
```

---

#### Q2. (R) A legacy integration team exposes this controller and claims it is REST. Identify RPC-in-REST smells and how you would reshape routes and verbs without breaking existing clients immediately.

```csharp
[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    [HttpPost("CreateOrder")]
    public IActionResult CreateOrder([FromBody] CreateOrderDto dto) => Ok(_svc.Create(dto));

    [HttpPost("CancelOrder")]
    public IActionResult CancelOrder([FromBody] CancelOrderDto dto) => Ok(_svc.Cancel(dto.Id));

    [HttpPost("GetOrderById")]
    public IActionResult GetOrderById([FromBody] int id) => Ok(_svc.Get(id));
}
```

---

#### Q3. (R) Review error handling in this inventory API. Clients cannot distinguish "bad request" from "not found," and failed updates sometimes return HTTP 200 with `{ "success": false }`.

```csharp
[HttpPut("{id:int}")]
public IActionResult Update(int id, [FromBody] UpdateItemDto dto)
{
    if (dto.Quantity < 0)
        return Ok(new { success = false, message = "Invalid quantity" });

    var item = _repo.Find(id);
    if (item is null)
        return Ok(new { success = false, message = "Not found" });

    _repo.Update(id, dto);
    return Ok(new { success = true });
}
```

---

#### Q4. (M) A mobile client retries `PUT /api/customers/42` after a timeout. The first request actually succeeded but the client never received the response. Explain why PUT is the right verb here and what idempotency guarantees the server should document.

---

#### Q5. (R) Review this search endpoint from a security audit. What REST and HTTP semantics are wrong, and what breaks under caching proxies?

```csharp
[HttpGet("delete")]
public IActionResult Delete([FromQuery] int id)
{
    _repo.Delete(id);
    return NoContent();
}

[HttpGet("search")]
public IActionResult Search([FromQuery] string q, [FromQuery] string apiKey)
{
    return Ok(_repo.Search(q, apiKey));
}
```

---

#### Q6. (D) Product asks for full HATEOAS on every list response (`_links.self`, `_links.next`, etc.). When is hypermedia worth the contract cost in a B2B JSON API, and when is a stable OpenAPI document plus explicit pagination query params enough?

---

#### Q7. (P) You are introducing `/api/v2/customers` while v1 stays live for six months. Compare URL path versioning, `X-Api-Version` header, and `Accept: application/vnd.company.customers.v2+json` — which fits gateway routing, mobile apps, and breaking DTO changes?

---

#### Q8. (R) Review this mixed-style API surface from a fintech partner integration. Prioritize the highest-risk contract issues for production.

```csharp
[HttpPost]
[Route("api/transfers/sendMoney")]          // verb in path
public IActionResult SendMoney([FromBody] TransferDto dto) => Ok(_transfers.Send(dto));

[HttpGet("api/transfers/{id}/status")]      // duplicate api prefix on action
public IActionResult Status(int id) => Ok(_transfers.Status(id));

[HttpDelete("transfers")]                   // missing leading api segment; body on DELETE
public IActionResult Cancel([FromBody] CancelDto dto) => Ok();
```

`TransfersController` has `[Route("api/[controller]")]` at class level.
