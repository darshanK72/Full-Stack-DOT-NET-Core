# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/04. Functional Style Programming/04. Extension Methods`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A teammate nests extension helpers inside an existing service class to "keep related code together." Review this addition:

```csharp
public sealed class OrderPricingService
{
    public decimal CalculateTotal(IEnumerable<OrderLine> lines) =>
        lines.Sum(l => l.LineTotal);

    public static class LineExtensions
    {
        public string ToReceiptLine(this OrderLine line) =>
            $"{line.Sku} x{line.Quantity} = {line.LineTotal:C}";
    }
}

// Caller in another file (using OrderServices;):
var text = line.ToReceiptLine(); // CS1061 — 'OrderLine' does not contain a definition for 'ToReceiptLine'
```

What compile-time rules block this pattern, and how should the extension be relocated?

---

#### Q2. (R) After splitting helpers into a shared library, API controllers fail to build. Review the controller and library layout:

```csharp
// File: Acme.WebApi/Controllers/OrdersController.cs
using Acme.Domain;

public class OrdersController : ControllerBase
{
    [HttpGet("{id}")]
    public IActionResult Get(string id)
    {
        string label = id.ToDisplayLabel(); // CS1061
        return Ok(label);
    }
}

// File: Acme.Common/StringExtensions.cs
namespace Acme.Common.Extensions;

public static class StringExtensions
{
    public static string ToDisplayLabel(this string value) => $"[{value}]";
}
```

The domain models compile fine; only the controller breaks. What is missing, and why does `using static Acme.Common.Extensions.StringExtensions;` not fix it?

---

#### Q3. (R) A null-safe helper was added for optional promo codes on checkout. Review the extension and its first production call:

```csharp
public static class StringExtensions
{
    public static string RequirePromoCode(this string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidOperationException("Promo code required.");
        return code.Trim().ToUpperInvariant();
    }
}

// CheckoutService:
string? promo = request.PromoCode; // may be null when omitted
string normalized = promo.RequirePromoCode(); // no compiler warning
```

The developer assumed "extension methods behave like instance methods on null." What actually happens at runtime, and how should the API be shaped for optional promo codes?

---

#### Q4. (R) Two NuGet packages ship extensions on `string` with the same signature. After adding both, CI builds but behavior flipped in staging:

```csharp
// Package A — Acme.Text.JsonHelpers
namespace Acme.Text.JsonHelpers;
public static class StringJsonExtensions
{
    public static string Sanitize(this string input) =>
        input.Trim().Replace("\"", "'");
}

// Package B — Contoso.Security
namespace Contoso.Security;
public static class StringSecurityExtensions
{
    public static string Sanitize(this string input) =>
        input.Trim().ToLowerInvariant();
}

// Startup (both usings present):
using Acme.Text.JsonHelpers;
using Contoso.Security;

var safe = userInput.Sanitize(); // now calls Contoso's version
```

What binding rule caused the silent behavior change, and what are your options to make the call explicit and stable?

---

#### Q5. (P) A logging extension on `IEnumerable<T>` looks convenient but skews metrics under load. Review:

```csharp
public static class EnumerableDiagnosticsExtensions
{
    public static IEnumerable<T> Tap<T>(
        this IEnumerable<T> source,
        Action<T> onEach)
    {
        foreach (var item in source)
        {
            onEach(item);           // logs every element
            yield return item;
        }
    }
}

// OrderReportService:
var premiumLines = _cache.GetLines(orderId)
    .Where(l => l.UnitPrice >= 50m)
    .Tap(l => _logger.LogDebug("Premium line {Sku}", l.Sku))
    .ToList();

decimal total = premiumLines.Sum(l => l.LineTotal);
int count = premiumLines.Count(); // second pass — but source was already materialized
```

Assume `_cache.GetLines` returns a deferred `IEnumerable` backed by a live database query. A developer later removes `.ToList()` to "avoid an extra allocation." What breaks in production, and when should this extension materialize vs stay deferred?

---

#### Q6. (D) Your team debates where pricing rules belong for `OrderLine`. Option A adds extensions; Option B keeps methods on the type:

```csharp
// Option A — OrderLineExtensions.cs
public static decimal ApplyBulkDiscount(this OrderLine line, int tier) { /* 40 lines */ }
public static decimal ApplyRegionalTax(this OrderLine line, string region) { /* … */ }

// Option B — OrderLine.cs (sealed domain type)
public decimal ApplyBulkDiscount(int tier) { /* same logic */ }
```

The type is **sealed**, owned by your team, and referenced from API, tests, and a reporting job. When do extensions earn their place vs polluting discoverability, and what is your rule of thumb for third-party `HttpRequest`/`string` helpers vs domain types?

---

#### Q7. (P) An ASP.NET Core teammate models custom middleware as extension methods on `IApplicationBuilder`, mirroring `UseRouting` / `UseAuthentication`. Review this registration block:

```csharp
public static class CorrelationIdExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            var id = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                     ?? Guid.NewGuid().ToString("N");
            context.Response.Headers["X-Correlation-Id"] = id;
            await next(); // forgot to push id into HttpContext.Items / ILogger scope
        });
        return app;
    }
}

// Program.cs:
app.UseHttpsRedirection();
app.UseCorrelationId();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

Logs still cannot be correlated across services. What is wrong with the middleware body, and why is the extension-method shape (`this IApplicationBuilder`) the standard pattern here even though it is "just syntactic sugar"?

---

#### Q8. (M) Unit tests for a service that uses string extensions pass locally but fail in CI with `NullReferenceException`. Review the test setup:

```csharp
// Production code — Acme.Common.Extensions
public static class StringExtensions
{
    public static bool IsNullOrBlank(this string? value) =>
        string.IsNullOrWhiteSpace(value);
}

// Test project — no reference usings to Acme.Common.Extensions
public class CheckoutValidatorTests
{
    [Fact]
    public void Missing_promo_is_treated_as_blank()
    {
        string? promo = null;
        Assert.True(promo.IsNullOrBlank()); // fails in CI — CS1061 or runtime?
    }
}
```

The test project references the production assembly. Explain why extension methods are harder to mock than injected services, and what you would change if the team needs to swap validation rules per environment without `#if DEBUG` forks.
