# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/04. Functional Style Programming/04. Extension Methods`

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

**Answer:** Extension methods must live in a **non-nested** static class at namespace scope — a nested `static class` inside `OrderPricingService` cannot host extensions (CS1110 / CS1106), so the compiler never registers `ToReceiptLine` as an extension even if the nested class compiles.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile rules | Nested static class hosts extension | CS1110 — extension not in scope for instance-style calls |
| Discovery | Extension tied to service type, not `OrderLine` | Callers cannot find method via normal `using` on extension namespace |
| Design | Mixes domain service with syntactic sugar API | Violates separation — extensions belong in dedicated `*.Extensions` types |

**Fix (priority order):**

1. Move `ToReceiptLine` to a top-level `public static class OrderLineExtensions` in its own file (or at namespace root), matching **Program.cs** Section 2 and Section 5.
2. Place it in a namespace imported by callers — e.g. `Acme.Ordering.Extensions` — and add `using Acme.Ordering.Extensions;`.
3. Keep `OrderPricingService` as a normal instance/service class with no nested extension containers.

```csharp
namespace Acme.Ordering.Extensions;

public static class OrderLineExtensions
{
    public static string ToReceiptLine(this OrderLine line) =>
        $"{line.Sku} x{line.Quantity} = {line.LineTotal:C}";
}
```

**Production takeaway:** Karat uses nested-class extensions to test whether you know the static-class rule set — not just `this` syntax. Relocate to a top-level static class every time.

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

**Answer:** Extension methods are discovered by the namespace of the **static extension class**, not the extended type — the controller needs `using Acme.Common.Extensions;`. `using static` imports static members for direct calls (`ToDisplayLabel(id)`) but does **not** import extension methods for instance-style syntax.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | Missing `using Acme.Common.Extensions;` | CS1061 — method not found on `string` |
| Misconception | `using static StringExtensions` expected to enable `id.ToDisplayLabel()` | Instance-style extension syntax still fails |
| API surface | Extensions hidden from IntelliSense in WebApi layer | Team thinks library reference alone is enough |

**Fix (priority order):**

1. Add `using Acme.Common.Extensions;` to the controller (or a global `GlobalUsings.cs` in the WebApi project).
2. Alternatively call explicitly: `StringExtensions.ToDisplayLabel(id)` — no extension `using` required.
3. Do **not** rely on `using static` for extension discovery — it only lifts static members, not extension method binding.

**Production takeaway:** Shared helper libraries fail at the call site, not the definition — Karat tests namespace import rules from **Program.cs** Section 8c. Convention: `*.Extensions` namespaces + document required `using` in README or analyzer.

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

**Answer:** Unlike a true instance method, an extension **can** be invoked when the receiver is `null` — the compiler emits a static call and passes `null` as the first argument. `RequirePromoCode` then throws inside the method body (from `IsNullOrWhiteSpace`), but the developer lost nullable flow analysis because `this string` (non-nullable) does not warn on a `string?` receiver.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Null semantics | Extension call allowed on null receiver | Differs from instance-method NRE at call site |
| Nullable | `this string` on nullable receiver | No CS8602/CS8604 — silent null reaches helper |
| API design | `Require*` throws on missing optional field | 500s for valid "no promo" checkout paths |

**Fix (priority order):**

1. For optional promos, use a null-tolerant extension with `this string?` — mirror **Program.cs** `IsNullOrBlank` (Section 8d).
2. Split APIs: `NormalizePromoCode(this string code)` (non-null precondition) vs `TryNormalizePromo(this string? code, out string normalized)`.
3. At the call site, guard before require: `if (!promo.IsNullOrBlank()) { … }` or pattern-match nullable promo in the service layer.
4. Enable nullable reference types project-wide so `this string` vs `this string?` documents intent.

```csharp
public static string? NormalizePromoOrNull(this string? code) =>
    string.IsNullOrWhiteSpace(code) ? null : code.Trim().ToUpperInvariant();
```

**Production takeaway:** Extensions on reference types are a common null trap — Karat checks whether you know null is passed **into** the static method, not blocked at the call site like instance dispatch.

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

**Answer:** When multiple extension methods match, the compiler picks the **most specific** `this` type match; if still tied, **namespace/usings order** and internal tie-break rules apply — one extension wins at compile time with no runtime error. Adding a second package with the same signature can silently rebind the call.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Binding | Duplicate extension signatures in scope | Behavior change without compile failure |
| Maintainability | `Sanitize` name collision across packages | Staging/prod diverge when usings reorder |
| Security | Wrong sanitizer (JSON vs security) | Data corruption or missed normalization |

**Fix (priority order):**

1. Call explicitly via static syntax: `StringJsonExtensions.Sanitize(userInput)` — unambiguous, refactor-safe.
2. Remove one `using` and fully-qualify the chosen extension class.
3. Rename internal extensions to domain-specific names (`SanitizeForJson`, `SanitizeForLog`) — avoid BCL-style generic names on `string`.
4. Remember: **instance methods always beat extensions** with the same signature — extensions never override existing instance API (**Program.cs** Section 8e).

**Production takeaway:** Extension conflicts do not throw — they compile and swap implementations. Prefer explicit static calls at integration boundaries (security, serialization).

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

**Answer:** `Tap` uses `yield return`, so it is **deferred** — the database query and logging run only when the pipeline is enumerated. Removing `.ToList()` while still calling `Sum` and `Count` (or any two consumers) re-executes the entire chain twice: double DB round-trips, double side effects in `Tap`, and inconsistent snapshots if data changes between enumerations.

- With `.ToList()`, enumeration happens once; `Sum`/`Count` operate on an in-memory list — correct for reporting totals.
- Without materialization, each terminal operator (`Sum`, `Count`, `foreach`) re-walks the deferred chain from `_cache.GetLines`.
- `Tap` side effects (logging) fire once per enumeration — log volume and DB load multiply with chained consumers.

**When to materialize vs defer:**

- **Materialize** (`ToList`, `ToArray`) when you need a stable snapshot, multiple passes, or bounded side effects — typical for report aggregation after filtering.
- **Stay deferred** when a single downstream consumer streams once (export pipeline, single `foreach`) and the source is cheap/idempotent.

**Production takeaway:** IEnumerable extensions compose like LINQ — deferred by default (**Program.cs** Sections 7 and 8h). Karat pairs extensions with enumeration cost, not just syntax.

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

**Answer:** For **owned domain types** with core business rules (`ApplyBulkDiscount`, tax), prefer **instance methods on the type** (Option B) — discoverability, single place for behavior, and clearer unit tests. Reserve extensions for cross-cutting syntactic helpers that should not bloat the domain model, or when you **cannot** modify the type.

- **Use extensions on owned types sparingly:** formatting (`ToReceiptLine`), small adapters, or keeping `OrderLine` a pure data record while rules live in a policy service injected via DI.
- **Use extensions on BCL/third-party types:** `string`, `DateTime`, `HttpRequest`, `IEnumerable<T>` — you cannot add instance methods to sealed framework types (**Program.cs** Sections 3–4).
- **Avoid** putting 40-line pricing rules in extensions — they hide domain logic, bypass constructor/DI seams, and appear everywhere IntelliSense lists `OrderLine` methods.
- **Middle ground:** `OrderLine` stays immutable data; `IPricingPolicy` or domain service applies discounts — testable and mockable without static extension soup.

**Production takeaway:** Extensions extend surface area without extending responsibility — Karat tests judgment: `ToReceiptLine` on `OrderLine` fits; `ApplyRegionalTax` belongs on the type or a service, not a static helper class.

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

**Answer:** The middleware echoes a correlation ID on the **response** but never stores it in `HttpContext.Items`, `Activity`/OpenTelemetry baggage, or an `ILogger` scope — downstream middleware, controllers, and `ILogger` output never see the ID. The extension-method shape is standard because it attaches fluent, discoverable pipeline entry points to `IApplicationBuilder` without modifying the framework type — same mechanism as `StringExtensions.ToDisplayLabel(this string)`.

- **Fix the body:** after resolving `id`, set `context.Items["CorrelationId"] = id` and wrap `await next()` in `using (_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = id }))` or `Activity.Current?.SetTag(...)`.
- **Order:** correlation middleware should run **early** (before auth/logging-heavy middleware) so all subsequent components share the same ID — often immediately after `UseForwardedHeaders` / before `UseAuthentication`.
- **Why extension on `IApplicationBuilder`:** reads as `app.UseCorrelationId()` in `Program.cs`; groups middleware registration API in one static class; returns `IApplicationBuilder` for chaining — identical compiler rewrite to `CorrelationIdExtensions.UseCorrelationId(app)`.

**Production takeaway:** ASP.NET `Use*` methods are extension methods — Karat connects the C# feature to production pipeline ergonomics. Fixing the sugar without fixing `HttpContext`/logging scope leaves observability broken.

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

**Answer:** With the production assembly referenced but no `using Acme.Common.Extensions;`, the test fails at **compile time** with CS1061 — extension methods are not instance members of `string`. If a duplicate local extension exists in the test project, CI may bind differently. Extensions are **static dispatch** — you cannot mock `promo.IsNullOrBlank()` with Moq/NSubstitute the way you mock `ICheckoutValidator.IsBlank(promo)`.

- **Why hard to mock:** extensions compile to static calls on a fixed class; no interface, no virtual slot, no DI seam.
- **Fix immediate CI failure:** add `using Acme.Common.Extensions;` or call `StringExtensions.IsNullOrBlank(promo)` explicitly.
- **Swappable rules per environment:** extract behavior behind an interface — `IStringNormalizer` / `ICheckoutValidator` injected into the service; keep thin extensions as one-liner wrappers over injected services only at the edges (API binding), not core validation.
- **Testing extensions directly:** unit-test the static extension class with plain xUnit/NUnit tests — no mocking needed for pure functions like `IsNullOrBlank`.

**Production takeaway:** Extensions are ideal for pure, stateless helpers on types you do not own; inject interfaces when behavior must vary, be mocked, or carry policy — Karat stacks syntax discovery (Q2) with testability judgment here.
