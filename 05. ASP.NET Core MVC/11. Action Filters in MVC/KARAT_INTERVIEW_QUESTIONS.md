# Karat — Interview Questions

> **Folder:** `06. ASP.NET Core MVC/11. Action Filters in MVC`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)  
> **Cross-ref:** [Middleware Pipeline — filter vs middleware judgment](../../05.%20ASP.NET%20Core/03.%20Middleware%20Pipeline/KARAT_INTERVIEW_QUESTIONS.md#q8-m-product-needs-request-timing-in-logs-for-every-endpoint-including-minimal-apis-plus-early-rejection-of-oversized-uploads-before-mvc-model-binding-would-you-use-middleware-an-endpoint-filter-or-an-action-filter-for-each-concern--and-why)

---

#### Q1. (R) Review this custom action filter registered globally on MVC controllers. Under load, thread-pool starvation appears and Kestrel queue depth grows. What is wrong, and when should this be `IAsyncActionFilter` instead of `IActionFilter`?

```csharp
public class TenantLookupFilter : IActionFilter
{
    private readonly ITenantRepository _tenants;

    public TenantLookupFilter(ITenantRepository tenants) => _tenants = tenants;

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var slug = context.RouteData.Values["tenant"]?.ToString();
        var tenant = _tenants.GetBySlugAsync(slug!).GetAwaiter().GetResult();
        context.HttpContext.Items["Tenant"] = tenant;
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}

// Program.cs
builder.Services.AddScoped<TenantLookupFilter>();
builder.Services.AddControllersWithViews(o => o.Filters.AddService<TenantLookupFilter>());
```

---

#### Q2. (M) A controller has three filters on one action: a global `AuditFilter` (`Order = 100`), controller-level `[ValidateAntiForgeryToken]`, and action-level `[RequireRole("Admin")]`. Describe MVC filter **scope** (global → controller → action) and **order** within the same filter stage — which runs first within authorization filters, and can a lower-ordered authorization filter still short-circuit after a higher-ordered one passes?

---

#### Q3. (R) Review global filter registration and DI. Production enables scope validation; the first POST to `/Orders/Create` throws `InvalidOperationException: Cannot consume scoped service 'AppDbContext' from singleton`. Diagnose.

```csharp
builder.Services.AddDbContext<AppDbContext>();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<AuditActionFilter>(); // not AddService
});

public class AuditActionFilter : IAsyncActionFilter
{
    private readonly AppDbContext _db;
    public AuditActionFilter(AppDbContext db) => _db = db;

    public async Task OnActionExecutionAsync(ActionExecutingContext ctx, ActionExecutionDelegate next)
    {
        _db.AuditLogs.Add(new AuditLog { Action = ctx.ActionDescriptor.DisplayName });
        await _db.SaveChangesAsync();
        await next();
    }
}
```

---

#### Q4. (D) Product wants to block unauthenticated traffic before a custom middleware that opens an expensive read replica connection on every request. A developer proposes `[Authorize]` on every MVC controller instead of moving auth earlier in the pipeline. Compare authorization-filter short-circuit (`context.Result = …`) vs middleware short-circuit (no `_next`). Which seam fixes replica churn for MVC-only apps vs mixed MVC + minimal API hosts?

---

#### Q5. (R) Review this exception filter deployed to Production alongside `app.UseExceptionHandler()`. Clients sometimes receive HTML error pages and sometimes JSON `ProblemDetails` for the same exception type; logs show duplicate stack traces. What is wrong?

```csharp
public class MvcExceptionFilter : IExceptionFilter
{
    private readonly ILogger<MvcExceptionFilter> _logger;

    public MvcExceptionFilter(ILogger<MvcExceptionFilter> logger) => _logger = logger;

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "Action failed");
        context.ExceptionHandled = true;
        context.Result = new ObjectResult(new ProblemDetails
        {
            Status = 500,
            Title = "Server error",
            Detail = context.Exception.Message
        }) { StatusCode = 500 };
        throw context.Exception; // ensure middleware also sees it
    }
}
```

---

#### Q6. (R) Review this result filter meant to skip view rendering when a cached HTML fragment exists. QA reports the view engine still runs and `_Layout.cshtml` executes twice on cache hits. Diagnose.

```csharp
public class FragmentCacheResultFilter : IResultFilter
{
    private readonly IMemoryCache _cache;

    public FragmentCacheResultFilter(IMemoryCache cache) => _cache = cache;

    public void OnResultExecuting(ResultExecutingContext context)
    {
        var key = context.HttpContext.Request.Path.Value;
        if (_cache.TryGetValue(key, out string? html))
            context.HttpContext.Response.WriteAsync(html); // write cached body
    }

    public void OnResultExecuted(ResultExecutedContext context) { }
}
```

---

#### Q7. (P) You need an audit action filter on every MVC controller but not on `/api/*` minimal routes in the same app. Show the correct global registration pattern with `AddControllersWithViews`, filter `Order`, and how `[AllowAnonymous]` / `[IgnoreAntiforgeryToken]` interact with globally registered filters.

---

#### Q8. (R) A teammate registers a filter with `[TypeFilter(typeof(RateLimitFilter), Arguments = new object[] { 60 })]` but the app fails at first request with `InvalidOperationException` about `IRateLimitStore`. Another controller uses `[ServiceFilter(typeof(RateLimitFilter))]` successfully. Explain filter factory resolution (`TypeFilterAttribute` vs `ServiceFilterAttribute`) and fix the broken registration.

```csharp
public class RateLimitFilter : IActionFilter
{
    private readonly IRateLimitStore _store;
    private readonly int _perMinute;

    public RateLimitFilter(IRateLimitStore store, int perMinute)
    {
        _store = store;
        _perMinute = perMinute;
    }
    // OnActionExecuting checks _store ...
}

// Broken controller
[TypeFilter(typeof(RateLimitFilter), Arguments = new object[] { 60 })]
public class ReportsController : Controller { }

// Working controller — Program.cs has: builder.Services.AddScoped<RateLimitFilter>();
[ServiceFilter(typeof(RateLimitFilter))]
public class InvoicesController : Controller { }
```

---

#### Q9. (R) An AJAX partial-update endpoint returns `400 Bad Request` with empty body after deploy. The action has `[ValidateAntiForgeryToken]`; the jQuery client POSTs JSON to `/Cart/AddItem` without a token header. Review the controller setup and client contract — what changed with `[AutoValidateAntiforgeryToken]`, and how do you fix AJAX without disabling CSRF?

```csharp
[AutoValidateAntiforgeryToken]
public class CartController : Controller
{
    [HttpPost]
    public IActionResult AddItem([FromBody] AddItemRequest request) { /* ... */ }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public IActionResult AddItemLegacy(AddItemRequest request) { /* ... */ }
}

// Client
$.ajax({ url: '/Cart/AddItem', method: 'POST', contentType: 'application/json',
         data: JSON.stringify({ sku: 'ABC', qty: 1 }) });
```

---

#### Q10. (R) Review this resource filter intended to memoize an expensive catalog lookup **once per HTTP request** across two action filters and the action. Instead, users intermittently see another tenant's catalog after scale-out to three pods. Diagnose lifetime and caching scope.

```csharp
public class CatalogResourceFilter : IResourceFilter
{
    private static readonly Dictionary<string, CatalogDto> _requestCache = new();

    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        var tenantId = context.HttpContext.User.FindFirst("tenant_id")?.Value;
        var key = $"{tenantId}:{context.RouteData.Values["categoryId"]}";
        if (_requestCache.TryGetValue(key, out var hit))
            context.HttpContext.Items["Catalog"] = hit;
    }

    public void OnResourceExecuted(ResourceExecutedContext context)
    {
        if (context.HttpContext.Items["Catalog"] is CatalogDto dto)
        {
            var tenantId = context.HttpContext.User.FindFirst("tenant_id")?.Value;
            var key = $"{tenantId}:{context.RouteData.Values["categoryId"]}";
            _requestCache[key] = dto;
        }
    }
}
```

---

#### Q11. (D) Your MVC app needs (a) per-action audit with route values and bound model metadata, (b) uniform 413 rejection for oversized uploads before model binding, and (c) a correlation ID on every response including static files. For each concern, choose middleware, authorization filter, action filter, resource filter, or result filter — and cite one reason the wrong layer fails.

---

#### Q12. (M) Two action filters both implement `IOrderedFilter`: `IdempotencyFilter` (`Order = -1000`, short-circuits duplicate POSTs) and `ModelStateValidationFilter` (`Order = 0`, returns `BadRequestObjectResult` when invalid). Walk through `OnActionExecutionAsync` / `OnActionExecuting` invocation order for `[ServiceFilter(typeof(IdempotencyFilter)), ServiceFilter(typeof(ModelStateValidationFilter))]` on a POST action — which runs first, and why does order matter for idempotency?
