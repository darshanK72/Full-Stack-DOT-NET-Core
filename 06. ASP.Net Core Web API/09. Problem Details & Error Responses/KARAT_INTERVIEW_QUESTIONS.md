# Karat — Interview Questions

> **Folder:** `07. ASP.Net Core Web API/09. Problem Details & Error Responses`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)  
> **Cross-ref:** [05. ASP.NET Core/10. Exception Handling](../../05.%20ASP.NET%20Core/10.%20Exception%20Handling/KARAT_INTERVIEW_QUESTIONS.md) (global handlers, `throw;` vs `throw ex`)

---

#### Q1. (R) Review this payment service called from a Web API controller. QA sees 500 responses with no correlation id in logs, and Application Insights shows the stack trace starting at the rethrow site — not the gateway call.

```csharp
public async Task ChargeAsync(Guid orderId, decimal amount, CancellationToken ct)
{
    try
    {
        await _gateway.ChargeAsync(orderId, amount, ct);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex.Message);
        throw ex;
    }
}
```

The controller has no try/catch; it expects a global handler to shape errors.

---

#### Q2. (P) You inherit a Web API with per-controller try/catch blocks returning `{ error = ex.Message }`, plain strings for 404, and occasional 500 for validation failures. How do you move to **centralized** exception handling with RFC 7807 `ProblemDetails`, consistent status codes, and different Development vs Production response bodies — without `#if DEBUG` in every controller?

---

#### Q3. (M) A mobile client parses error JSON using RFC 7807. Review this `[ApiController]` action and the actual response body when model validation fails. What status code and shape does the client receive, and what fields should they rely on?

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    [HttpPost]
    public IActionResult Create([FromBody] CreateOrderDto dto)
    {
        // dto.OrderLines is required but missing in request body
        return Ok(_service.Create(dto));
    }
}
```

Assume `CreateOrderDto` has `[Required]` on `OrderLines` and no custom filter overrides validation.

---

#### Q4. (R) Review this controller catch blocks. Swagger documents 400/404/422, but production clients receive inconsistent shapes and wrong status codes.

```csharp
catch (ValidationException ex)
{
    return StatusCode(500, new { error = ex.Message, fields = ex.Errors });
}
catch (NotFoundException ex)
{
    return NotFound(ex.Message);
}
catch (DuplicateOrderException ex)
{
    return Conflict(new ProblemDetails { Title = "Duplicate", Detail = ex.Message });
}
```

---

#### Q5. (D) Compare **Development** vs **Production** error responses for the same unhandled `NullReferenceException` in a minimal API endpoint. What should each environment return (status, `ProblemDetails` fields, stack trace), and what must never appear in an external Production JSON body?

---

#### Q6. (P) Implement global handling with **`IExceptionHandler`** (.NET 8+) for a JSON Web API. Show registration in `Program.cs`, a handler that maps `NotFoundException` → 404 and unknown exceptions → 500, and explain what `TryHandleAsync` returning `true` vs `false` does when multiple handlers are registered.

---

#### Q7. (R) Two teams merged their APIs. Support tickets report "sometimes 404 is JSON ProblemDetails, sometimes plain text." Review these endpoints for **status code and body consistency**.

```csharp
// Team A
[HttpGet("{id}")]
public IActionResult Get(int id)
{
    var item = _repo.Find(id);
    if (item is null) return NotFound(); // default ProblemDetails when ApiController
    return Ok(item);
}

// Team B — same host, different controller base
public class LegacyReportsController : ControllerBase // no [ApiController]
{
    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var report = _repo.Find(id);
        if (report is null) return NotFound($"Report {id} not found");
        return Ok(report);
    }
}
```

---

#### Q8. (M) Your API uses `[ApiController]` automatic validation responses. A partner sends `POST /api/customers` with an invalid email. Walk through the pipeline: who produces the response, what HTTP status is used (400 vs 422), and how does **`ValidationProblemDetails`** differ from a hand-written `{ errors: [...] }` object?

---

#### Q9. (D) Design a **status-code mapping table** for domain exceptions in a REST API (`ValidationException` → 400, `NotFoundException` → 404, duplicate → 409, forbidden business rule → 403, unexpected → 500). Where should mapping live so controllers stay thin, logs retain full exceptions, and OpenAPI documents the correct response schemas per status?
