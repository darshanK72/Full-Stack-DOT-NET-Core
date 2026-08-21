# Karat — Interview Questions

> **Folder:** `05. ASP.NET Core/09. Filters`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)  
> **Raw debrief:** [KARAT_DEBRIEF_SOURCE.txt](../../00. Notes & Practice/04. Citi Karat Interview Practice/KARAT_DEBRIEF_SOURCE.txt)

---

#### Q1. (M) A teammate says "we'll put auth in an action filter instead of middleware." Walk through the MVC filter execution order — authorization filter, resource filter, action filter, exception filter, result filter — and explain which filter type owns authentication vs authorization vs action-specific validation.

---

#### Q2. (R) Review this custom authorization filter. What breaks at runtime or under load, and what would you change?

```csharp
public class ApiKeyAuthFilter : IAuthorizationFilter
{
    private readonly IUserRepository _users;

    public ApiKeyAuthFilter(IUserRepository users) => _users = users;

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var key = context.HttpContext.Request.Headers["X-Api-Key"];
        var user = _users.FindByApiKey(key).Result;
        if (user is null)
            context.Result = new UnauthorizedResult();
    }
}
```

*(Assume the filter is registered globally via `AddControllers(options => options.Filters.Add<ApiKeyAuthFilter>())`.)*

---

#### Q3. (P) Implement cross-cutting request timing and audit logging around controller actions using `IAsyncActionFilter`. What runs in `OnActionExecutionAsync` before vs after `await next()`, and what can you still change at each stage?

---

#### Q4. (D) Your team needs to reject requests without a valid API key before routing reaches expensive database middleware. Another developer wants an `IActionFilter` on every controller. Compare filter short-circuit (`context.Result = …`) vs middleware short-circuit (no `_next`). When is each the right seam?

---

#### Q5. (M) `[Authorize(Roles = "Admin")]` is implemented as an authorization filter. How does that differ from calling `app.UseAuthentication()` / `UseAuthorization()` middleware, and what happens if authorization middleware runs but no authorization filter is reached (e.g., minimal API endpoint)?

---

#### Q6. (R) Review this resource filter intended to cache expensive lookup data for the duration of one request. What is wrong?

```csharp
public class CatalogCacheResourceFilter : IResourceFilter
{
    private static readonly Dictionary<string, object> _cache = new();

    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        var key = context.RouteData.Values["categoryId"]?.ToString();
        if (key != null && _cache.TryGetValue(key, out var cached))
            context.HttpContext.Items["Catalog"] = cached;
    }

    public void OnResourceExecuted(ResourceExecutedContext context)
    {
        var key = context.RouteData.Values["categoryId"]?.ToString();
        if (key != null && !context.HttpContext.Items.ContainsKey("Catalog"))
            _cache[key] = context.HttpContext.Items["Catalog"]!;
    }
}
```

---

#### Q7. (P) A global action filter is registered in `Program.cs` and constructor-injects `AppDbContext`. The app starts in Development but throws `Cannot consume scoped service from singleton` in Production with scope validation enabled. Explain filter DI lifetime, how global filters are resolved, and the correct fix.

```csharp
builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditActionFilter>();
});
builder.Services.AddDbContext<AppDbContext>();
```

---

#### Q8. (D) Unhandled exceptions in a controller can be caught by an exception filter, `IExceptionHandler` middleware, or `UseExceptionHandler`. Compare scope (MVC-only vs entire pipeline), ordering, and when you would keep an exception filter vs centralizing everything in middleware.
