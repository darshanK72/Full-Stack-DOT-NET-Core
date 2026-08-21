# Karat — Interview Questions

> **Folder:** `05. ASP.NET Core/01. Introduction to ASP.NET Core`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)  
> **Raw debrief:** [KARAT_DEBRIEF_SOURCE.txt](../../00. Notes & Practice/04. Citi Karat Interview Practice/KARAT_DEBRIEF_SOURCE.txt)

---

#### Q1. (R) A developer ports a .NET Framework Web API to ASP.NET Core and keeps this controller pattern. What breaks at compile time and at runtime, and what architectural shift does ASP.NET Core require instead?

```csharp
public class OrdersController : ApiController
{
    public IActionResult Get(int id)
    {
        var user = HttpContext.Current.User.Identity.Name;
        var order = OrderRepository.Find(id); // static data access
        return Json(order);
    }
}
```

---

#### Q2. (P) Your team deploys the same ASP.NET Core API to Linux containers behind nginx and to Windows with IIS. Explain who runs application code, who terminates TLS, and what stays the same in `Program.cs` across both targets.

---

#### Q3. (R) Review this `Program.cs` from a tutorial copied into a staging environment. The app starts but health checks fail and Swagger is exposed publicly. What is wrong?

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run("http://0.0.0.0:5000");
```

*(Assume `ASPNETCORE_ENVIRONMENT=Staging` and the team expects HTTPS-only, no public API docs.)*

---

#### Q4. (M) A request hits `GET /api/orders/42` on a deployed ASP.NET Core app. Walk through the major stages from Kestrel accepting the socket through to the JSON response leaving the process — name the layers, not every middleware.

---

#### Q5. (D) Product wants a small internal tool: one POST endpoint, one GET endpoint, no MVC views, team knows C# well. When would you choose **Minimal APIs** vs **controllers**, and what would make you regret Minimal APIs six months later?

---

#### Q6. (R) A consultant claims "ASP.NET Core is just Kestrel — you don't need IIS or nginx." Review their deployment diagram assumptions. What production gaps appear when Kestrel is the only layer in front of your app?

---

#### Q7. (R) The unified hosting model (`WebApplication.CreateBuilder`) replaced `IWebHost` / `Startup.cs` for most new projects. Review this partial migration — what breaks middleware order, integration tests, and endpoint discovery?

```csharp
// Migrated from Startup.Configure — "should be equivalent"
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseRouting();
app.Run();

// Integration tests still use:
// new HostBuilder().ConfigureWebHost(b => b.UseStartup<Startup>()).Build();
```

---

#### Q8. (R) Review this cross-platform CI script comment and the accompanying `Program.cs` change. Builds pass on Windows agents but the container crashes on Linux with `Address already in use`. Diagnose and fix.

```csharp
// Dockerfile EXPOSE 8080 — CI sets ASPNETCORE_URLS=http://+:8080
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5000"); // force dev URL
var app = builder.Build();
app.MapGet("/health", () => Results.Ok("healthy"));
app.Run();
```
