# Karat — Interview Questions

> **Folder:** `05. ASP.NET Core/06. Logging & Diagnostics`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)  
> **Raw debrief:** [KARAT_DEBRIEF_SOURCE.txt](../../00. Notes & Practice/04. Citi Karat Interview Practice/KARAT_DEBRIEF_SOURCE.txt)

---

#### Q1. (R) Review this exception handling in a payment service. Support cannot find stack traces in Application Insights after incidents.

```csharp
public async Task ChargeAsync(Guid orderId, CancellationToken ct)
{
    try
    {
        await _gateway.ChargeAsync(orderId, ct);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex.Message);
        throw ex;
    }
}
```

---

#### Q2. (R) Review this controller action. Logs in Seq show duplicate lines with no way to tie gateway, repository, and controller entries to one customer checkout.

```csharp
[HttpPost("checkout")]
public async Task<IActionResult> Checkout(CheckoutRequest request, CancellationToken ct)
{
    _logger.LogInformation("Checkout started for {Email}", request.Email);
    var order = await _orderService.CreateAsync(request, ct);
    _logger.LogInformation("Checkout completed order {OrderId}", order.Id);
    return Ok(order);
}

// OrderService.cs — no scope, logs only order id
_logger.LogInformation("Persisting order {OrderId}", order.Id);
```

---

#### Q3. (P) Production `appsettings.Production.json` sets `"Default": "Debug"` for logging while Development uses `"Information"`. What risks does this create, and what levels would you configure per namespace for a public API?

---

#### Q4. (R) Review this audit logging added for a login endpoint. Security flags the change in PR review.

```csharp
[HttpPost("login")]
public async Task<IActionResult> Login(LoginRequest request)
{
    _logger.LogInformation(
        "Login attempt user={User} password={Password} ip={Ip}",
        request.Username,
        request.Password,
        HttpContext.Connection.RemoteIpAddress);

    var result = await _auth.SignInAsync(request.Username, request.Password);
    return result.Succeeded ? Ok() : Unauthorized();
}
```

---

#### Q5. (P) A team wants distributed traces from ASP.NET Core through an outbound `HttpClient` call to a downstream pricing service. Outline the OpenTelemetry setup in `Program.cs` and what must the outbound call participate in for trace continuity.

---

#### Q6. (R) Review this high-traffic endpoint logging. Log volume spikes cost and hides real errors in noise.

```csharp
[HttpGet("products")]
public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetProducts(CancellationToken ct)
{
    _logger.LogInformation($"GetProducts called at {DateTime.UtcNow}");
    var products = await _repo.GetAllAsync(ct);
    foreach (var p in products)
        _logger.LogDebug("Product: " + p.Name + " price=" + p.Price);
    _logger.LogInformation("Returning " + products.Count + " products");
    return Ok(products);
}
```

---

#### Q7. (M) Explain how `ILogger.BeginScope` (or `LoggerMessage` scopes) propagates correlation IDs across async calls in a request, and where you would set the initial `TraceIdentifier` or custom `CorrelationId` in the ASP.NET Core pipeline.

---

#### Q8. (D) An on-call alert fires on `LogCritical` from a background worker when a queue is full, but the same team ignores `LogError` in controllers. How do you define log level policy, sampling, and alert thresholds so production signal stays actionable?
