# Karat — Interview Questions

> **Folder:** `07. ASP.Net Core Web API/10. Authentication & Authorization in APIs`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)  
> **Cross-ref:** [05. ASP.NET Core/08. Authentication & Authorization](../../05.%20ASP.NET%20Core/08.%20Authentication%20&%20Authorization/KARAT_INTERVIEW_QUESTIONS.md) (cookie vs bearer fundamentals)

---

#### Q1. (R) Review `Program.cs` and a protected controller. Tokens validate in jwt.io but every API call returns 401 in staging.

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://login.company.com";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidIssuer = "https://wrong-issuer.example.com"
        };
    });

builder.Services.AddAuthorization();
// ...

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

[Authorize]
[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    [HttpGet]
    public IActionResult List() => Ok(_repo.All());
}
```

Identity provider issues JWTs with `iss: https://login.company.com` and `aud: order-api`.

---

#### Q2. (R) Same API as Q1 — after fixing JWT validation, `[Authorize]` endpoints still return 401 while anonymous health checks work. Review middleware order and what's missing.

```csharp
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
```

---

#### Q3. (P) When should you use **`[Authorize(Roles = "Admin")]`** vs **`[Authorize(Policy = "CanManageOrders")]`** on Web API controllers? How do you register policies, and what breaks when you only use roles for fine-grained API access?

---

#### Q4. (M) An API accepts Azure AD JWTs. One endpoint must allow users with **`scope: orders.read`**; another requires **`scope: orders.write`**. A token has role `OrderClerk` but no scopes. Explain how **scope claims** differ from **role claims**, and how you enforce scopes in ASP.NET Core authorization.

---

#### Q5. (R) Review this API key authentication handler. Security audit flags credential leakage in access logs and browser history.

```csharp
public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Query.TryGetValue("apiKey", out var key))
            return Task.FromResult(AuthenticateResult.NoResult());

        if (key == _options.ApiKey)
        {
            var claims = new[] { new Claim(ClaimTypes.Name, "api-client") };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(
                new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
        }
        return Task.FromResult(AuthenticateResult.Fail("Invalid key"));
    }
}
```

---

#### Q6. (R) Penetration test found sensitive data on "public" endpoints. Review this controller — which actions are anonymously reachable and why?

```csharp
[Authorize]
[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    [HttpGet("{id}")]
    public IActionResult Get(int id) => Ok(_service.Get(id));

    [HttpGet("export")]
    public IActionResult ExportAll() => Ok(_service.ExportAll());

    [HttpPost]
    [AllowAnonymous]
    public IActionResult Register([FromBody] RegisterDto dto) => Ok(_service.Register(dto));
}

[ApiController]
[Route("api/internal/metrics")]
public class MetricsController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(_metrics.Snapshot());
}
```

Global policy: no fallback authorization policy configured.

---

#### Q7. (R) Review this diagnostic middleware deployed to Production. Compliance finds bearer tokens in log storage.

```csharp
public async Task InvokeAsync(HttpContext context, RequestDelegate next)
{
    var authHeader = context.Request.Headers.Authorization.ToString();
    _logger.LogInformation("Request {Method} {Path} Auth={Auth}",
        context.Request.Method, context.Request.Path, authHeader);

    if (context.Request.QueryString.HasValue)
        _logger.LogInformation("Query={Query}", context.Request.QueryString.Value);

    await next(context);
}
```

Clients send `Authorization: Bearer eyJhbG...`.

---

#### Q8. (P) Your API supports **JWT Bearer** for mobile apps and **API Key** header for batch jobs. Both schemes are registered. Explain **authentication scheme order**, default scheme selection, and how **`[Authorize(AuthenticationSchemes = "...")]`** prevents the wrong handler from running first.

---

#### Q9. (D) Design authorization for a multi-tenant orders API: tenants must only read their own data; admins can read all; partner integrations use API keys scoped to one tenant. Compare role-only, policy + requirements, and resource-based authorization — what would you implement and where?

---

#### Q10. (M) A controller has class-level `[Authorize]` and one webhook action marked `[AllowAnonymous]`. The webhook validates an HMAC signature in the action body. Review risks — when is `[AllowAnonymous]` appropriate on APIs, and what must still protect the endpoint?

```csharp
[Authorize]
[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> StripeWebhook()
    {
        var json = await new StreamReader(Request.Body).ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"];
        if (!_stripe.Verify(json, signature)) return BadRequest();
        await _handler.Process(json);
        return Ok();
    }
}
```
