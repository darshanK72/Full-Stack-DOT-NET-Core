# Karat — Interview Questions

> **Folder:** `07. ASP.Net Core Web API/07. API Versioning`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)  
> **Raw debrief:** [KARAT_DEBRIEF_SOURCE.txt](../../00. Notes & Practice/04. Citi Karat Interview Practice/KARAT_DEBRIEF_SOURCE.txt)

---

#### Q1. (D) A public payments API must support v1 clients for 18 months while shipping v2 with breaking JSON shape changes. Compare URL path versioning (`/api/v2/...`), `X-Api-Version` header, and `?api-version=2.0` query — cacheability, gateway routing, discoverability, and client SDK ergonomics.

---

#### Q2. (P) Review URL-based versioning registration. Clients calling `/api/products` without a version segment receive 404; product owner expected unversioned calls to hit v1.

```csharp
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = false;
    options.ReportApiVersions = true;
});

builder.Services.AddControllers();

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    public IActionResult List() => Ok(_products.ListV1());
}
```

---

#### Q3. (R) Review header-based versioning. Mobile app sends `X-Api-Version: 2.0` but always receives v1 payload shape; server logs show route matched v1 controller.

```csharp
builder.Services.AddApiVersioning(options =>
{
    options.ApiVersionReader = new HeaderApiVersionReader("X-Api-Version");
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
});

[ApiVersion("1.0")]
[Route("api/[controller]")]
public class OrdersV1Controller : ControllerBase { /* ... */ }

[ApiVersion("2.0")]
[Route("api/[controller]")]
public class OrdersV2Controller : ControllerBase { /* ... */ }
```

Both controllers registered; no `[MapToApiVersion]` or versioned route constraints on actions.

---

#### Q4. (P) v1 of `GET /api/customers/{id}` returns `name`; v2 returns `fullName` and drops `name`. What constitutes a breaking change vs a safe additive change, and how do you document the migration in OpenAPI and release notes without breaking existing integrators?

---

#### Q5. (P) v1 endpoints must announce deprecation before removal in six months. Review the proposed middleware and response headers for Sunset / Link / `Deprecation` signaling — what should clients and API gateways observe?

```csharp
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api/v1"))
        context.Response.Headers.Append("X-Deprecated", "true");
    await next();
});
```

No `Sunset` date, no `Link` relation to v2 documentation, OpenAPI still marks v1 as current.

---

#### Q6. (M) With `Asp.Versioning.Mvc` and Swashbuckle, explain how each API version appears as a separate OpenAPI document, how `DocInclusionPredicate` maps controller versions to `SwaggerDoc` names, and what breaks if v2 actions share DTO type names with v1.

---

#### Q7. (R) Review query-string versioning behind a CDN. Cached responses serve v1 JSON to v2 clients intermittently.

```csharp
builder.Services.AddApiVersioning(options =>
{
    options.ApiVersionReader = new QueryStringApiVersionReader("api-version");
});

[ApiVersion("1.0")]
[HttpGet("api/items")]
public IActionResult ListV1() => Ok(_items.ListV1());

[ApiVersion("2.0")]
[HttpGet("api/items")]
public IActionResult ListV2() => Ok(_items.ListV2());
```

CDN caches `GET /api/items` without varying on query string; `Cache-Control: public, max-age=300` set on responses.

---

#### Q8. (D) Default version behavior: `AssumeDefaultVersionWhenUnspecified = true` with `DefaultApiVersion = 2.0` while v1 remains deployed for legacy partners. What operational and contract risks does this create, and when is implicit default versioning acceptable vs requiring explicit version on every call?
