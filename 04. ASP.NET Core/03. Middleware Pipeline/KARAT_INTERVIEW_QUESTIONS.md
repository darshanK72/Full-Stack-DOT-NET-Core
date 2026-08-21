# Karat — Interview Questions

> **Folder:** `05. ASP.NET Core/03. Middleware Pipeline`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)  
> **Raw debrief:** [KARAT_DEBRIEF_SOURCE.txt](../../00. Notes & Practice/04. Citi Karat Interview Practice/KARAT_DEBRIEF_SOURCE.txt)

---

#### Q1. (P) How do you limit incoming client requests within a time window using ASP.NET Core's built-in rate limiting middleware? What must you register in services, how do you attach policies to endpoints, and where should `UseRateLimiter()` sit relative to routing and authentication?

---

#### Q2. (R) Review this API key middleware. What is wrong with behavior, headers, and security — and what happens to downstream middleware when the key is missing?

```csharp
public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    public ApiKeyMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.ContainsKey("X-Api-Key"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }
        await _next(context);
    }
}
```

*(Registration order is correct; focus on response completion, validation, and client contract.)*

---

#### Q3. (M) An authentication middleware sets `Response.StatusCode = 401` and returns without calling `_next`. Trace one request through the pipeline — which components still run, which are skipped, and why is omitting `_next` intentional rather than a bug?

---

#### Q4. (R) After deploy behind nginx, HTTPS redirects loop and `[Authorize]` sees anonymous users. Review middleware order — what is wrong?

```csharp
var app = builder.Build();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseForwardedHeaders();
app.MapControllers();
app.Run();
```

---

#### Q5. (R) Review `Run` vs `Map` branching. `/admin` returns 404 and static files leak on `/admin/config.json`. Explain pipeline behavior and fix order.

```csharp
app.UseStaticFiles();
app.Map("/admin", adminApp =>
{
    adminApp.UseRouting();
    adminApp.MapControllers();
});
app.Run(async context =>
{
    await context.Response.WriteAsync("Fallback for unmatched routes");
});
app.MapControllers();
```

---

#### Q6. (P) Where should global exception-handling middleware sit relative to routing, authentication, and `UseDeveloperExceptionPage`? What breaks if exception middleware is registered too early or too late?

---

#### Q7. (R) This middleware tries to short-circuit banned clients but clients still receive full response bodies and logs show "Headers already sent." What went wrong?

```csharp
public async Task InvokeAsync(HttpContext context)
{
    await _next(context);

    if (IsBanned(context.Connection.RemoteIpAddress))
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsJsonAsync(new { error = "Forbidden" });
    }
}
```

---

#### Q8. (M) Product needs request timing in logs for every endpoint including minimal APIs, plus early rejection of oversized uploads before MVC model binding. Would you use middleware, an endpoint filter, or an action filter for each concern — and why?

---

#### Q9. (R) Review custom correlation-ID middleware and registration. Some responses have two `X-Correlation-Id` headers and downstream services receive empty IDs. Diagnose.

```csharp
public async Task InvokeAsync(HttpContext context)
{
    var id = context.Request.Headers["X-Correlation-Id"].FirstOrDefault() ?? Guid.NewGuid().ToString();
    context.Items["CorrelationId"] = id;
    context.Response.Headers.Append("X-Correlation-Id", id);
    await _next(context);
}

// Program.cs
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>(); // registered twice by mistake
app.MapControllers();
```

---

#### Q10. (D) You inherit a pipeline with 14 custom middleware components, some duplicated between `MapWhen` branches. How do you refactor without changing outward behavior — what belongs in middleware vs endpoint filters vs hosting reverse proxy?
