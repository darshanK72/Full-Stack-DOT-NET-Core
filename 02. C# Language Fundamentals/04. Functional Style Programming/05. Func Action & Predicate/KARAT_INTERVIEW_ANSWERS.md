# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/04. Functional Style Programming/05. Func Action & Predicate`

---

#### Q1. (R) A warehouse API reuses a shared filter delegate across `List<T>` and LINQ. The build fails after a refactor. What is wrong, and how do you fix it without duplicating filter logic?

**Answer:** `List<T>.FindAll` requires `Predicate<T>`, not `Func<T, bool>` — they have identical invoke shapes but are different delegate types with no implicit conversion. Store one shape and wrap at the boundary, or use lambdas at call sites so the compiler infers the expected type.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `FindAll(_inStockFilter)` — `Func<Product, bool>` → `Predicate<Product>` | CS1503; build fails |
| Design | Assumed semantic equivalence implies type equivalence | Refactors break when crossing BCL APIs |
| Maintainability | Duplicating filter logic in two delegate variables | Drift between List and LINQ paths |

**Fix (priority order):**

1. Pick a canonical stored type — usually `Func<Product, bool>` for LINQ-heavy code — and wrap for List APIs: `items.FindAll(p => _inStockFilter(p))` or `new Predicate<Product>(_inStockFilter.Invoke)`.
2. Alternatively store `Predicate<Product>` and wrap for LINQ: `items.Where(p => _predicate(p))`.
3. Extract the condition once: `private static bool IsInStock(Product p) => p.IsActive && p.StockQty > 0;` then method-group into either delegate type at each call site.
4. Prefer a single domain method or small `IProductFilter` when the rule is shared across many APIs — avoids delegate-type friction entirely.

**Production takeaway:** Karat embeds the **Program.cs** lesson — lambdas infer the parameter type at the call site, but **stored** delegates do not convert between `Predicate<T>` and `Func<T, bool>`. See Section 7 and QUICK REFERENCE.

---

#### Q2. (R) A teammate wires logging callbacks into a pick-list pipeline. Review the registration and invocation:

```csharp
public static void ProcessPickList(
    List<Product> items,
    Func<Product, decimal> lineTotal,
    Func<string> logHeader)   // intended: print banner once, return nothing
{
    logHeader(); // CS0029 — cannot convert void to decimal
    items.ForEach(p => Console.WriteLine($"{p.Sku}: {lineTotal(p):C}"));
}
```

What are the compile-time mistakes, and which built-in delegate types belong here?

**Answer:** `logHeader` is declared as `Func<string>` (returns `string`) but the lambda returns `void`, and the call site treats it like a side-effect callback. Void-returning work belongs on `Action` or `Action<string>`, not `Func`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `Func<string>` assigned `() => Console.WriteLine(...)` | CS0123 / CS0029 — void is not a valid `Func` return |
| API design | `Func<string>` implies a computed header string | Misleading contract; callers expect a return value |
| Invoke | `logHeader()` used for effect only | Wrong type family — `Action` expresses intent |

**Fix (priority order):**

1. Change the parameter to `Action printHeader` (zero parameters, void return): `printHeader();`
2. If the banner string is needed elsewhere, use `Func<string> headerFactory = () => "=== Pick list ===";` and pass that separately from `Action<string> emit`.
3. Keep `Func<Product, decimal> lineTotal` — it returns a value; that is the correct choice.
4. Match **Program.cs** Section 4: side effects → `Action`; computations → `Func`.

**Production takeaway:** Using `Func` for void methods is one of the most common compile errors in callback-heavy code — Karat tests whether you reach for `Action` immediately. See QUICK REFERENCE — "Using Func for void method → CS0123."

---

#### Q3. (R) After making a filter optional, production throws intermittently when a branch has no active rule. Review:

```csharp
public IEnumerable<Product> FilterCatalog(
    IEnumerable<Product> items,
    Func<Product, bool>? rule)
{
    return items.Where(rule);
}
```

What breaks at runtime, why does it pass some code paths, and what is the production-safe fix?

**Answer:** LINQ's `Where` invokes the predicate for every element — passing `null` throws `NullReferenceException` on the first item, not at the call to `Where`. Branches that always supply a rule appear fine until configuration omits one.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | `Where(null)` — predicate invoked per element | NRE on first enumeration; looks like "random" prod failure |
| Nullability | Nullable parameter without default | Optional filter contract is unsafe |
| API | Deferred execution hides failure until `foreach`/materialization | Fails in reporting job, not at startup |

**Fix (priority order):**

1. Apply a default before LINQ: `rule ??= _ => true;` or `return items.Where(rule ?? (_ => true));`
2. Short-circuit when absent: `if (rule is null) return items;`
3. For optional callbacks in custom APIs, use `?.Invoke` pattern from **Program.cs** Section 6 — but LINQ operators require a non-null delegate; guard at the API boundary.
4. Add a unit test with `rule: null` that forces enumeration (`ToList()`) — catches deferred-execution traps.

**Production takeaway:** Null-safe invoke (`?.Invoke`) works for your own optional `Action`/`Func` fields; BCL LINQ methods never accept null predicates. See **Program.cs** Section 6 and Section 11 preview.

---

#### Q4. (P) An ASP.NET Core app registers a `Func<IServiceProvider, decimal>` factory in DI to read tax rate per request. Review startup:

```csharp
builder.Services.AddSingleton<Func<IServiceProvider, decimal>>(sp => { /* … */ });
builder.Services.AddScoped<ProductPricingService>();
// ProductPricingService ctor: Func<decimal> taxRate
```

What lifetime and resolution problems appear under load or with `IOptionsMonitor`, and how should factories be registered instead?

**Answer:** Registering the outer factory as singleton while resolving scoped or monitor-backed options inside it captures stale configuration and blurs request scope. Inject `IOptionsMonitor<TaxOptions>` or `Func<decimal>` via a scoped factory registration so each request gets current values.

- **Singleton factory + scoped dependencies:** `GetRequiredService` inside a singleton delegate can resolve scoped services from the root provider — invalid outside a scope (may throw or return wrong instance depending on version/options).
- **Closed-over `IOptions` vs `IOptionsMonitor`:** snapshot `IOptions<T>` inside a singleton `Func<decimal>` never sees updated `appsettings` or key-vault reloads.
- **`Func<decimal>` in scoped service:** acceptable when the func is registered scoped or when it reads from `IOptionsMonitor` per invocation, not once at singleton creation.

**Fix (priority order):**

1. Register per-request factory: `builder.Services.AddScoped<Func<decimal>>(sp => () => sp.GetRequiredService<IOptionsMonitor<TaxOptions>>().CurrentValue.Rate);`
2. Better: inject `IOptionsMonitor<TaxOptions>` directly into `ProductPricingService` — clearer than func indirection for a single value.
3. Reserve `Func<IServiceProvider, T>` singleton factories for truly stateless object creation (e.g., `Func<IServiceProvider, ILogger>` patterns) — not for request-scoped configuration reads.
4. Enable scope validation in development: `builder.Host.UseDefaultServiceProvider(o => o.ValidateScopes = true);` — surfaces captive dependencies early.

**Production takeaway:** Func factories in DI are convenient but do not bypass lifetime rules — Karat stacks delegate typing with DI scope traps. Aligns with **Program.cs** Section 2 — `Func<decimal>` as testable configuration reader, but lifetime must match how often the value may change.

---

#### Q5. (R) A pricing service accepts `Func<Product, decimal>` so callers can plug in "async catalog lookups." Review usage from a minimal API endpoint:

```csharp
Func<Product, decimal> lookup = product =>
    catalog.GetUnitPriceAsync(product.Sku).Result;
```

What are the async, scalability, and delegate-signature problems, and what signature should replace `Func<Product, decimal>`?

**Answer:** `Func<Product, decimal>` cannot represent asynchronous work — forcing `.Result` blocks a thread and causes sync-over-async under load. The API should accept `Func<Product, CancellationToken, Task<decimal>>` (or a dedicated service interface) and `await` end-to-end.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Async | `.Result` on `GetUnitPriceAsync` | Thread-pool starvation; potential deadlocks |
| Signature | Sync `Func` for I/O-bound lookup | Misleading contract hides async requirement |
| Scalability | Blocking minimal API delegate | Reduced throughput on catalog-bound endpoints |
| Design | Func used to smuggle async into sync shape | Callers repeat `.Result` at every site |

**Fix (priority order):**

1. Change to async API: `public async Task<decimal> GetExtendedPriceAsync(Product p, Func<Product, CancellationToken, Task<decimal>> unitPriceLookup, CancellationToken ct)` and `await unitPriceLookup(p, ct)`.
2. Prefer injecting `ICatalogClient` into the service instead of passing func per call — func parameter is for pluggable algorithms, not HTTP clients.
3. Endpoint: `return Results.Ok(await pricing.GetExtendedPriceAsync(product, catalog.GetUnitPriceAsync, ct));` — method group to compatible func or direct service call.
4. Never register `Func<Product, decimal>` in DI when the implementation performs I/O — use typed client + async methods.

**Production takeaway:** Func/Action/Predicate are synchronous delegate shapes — async work needs `Func<..., Task<T>>` and `await`, or an interface. See C# Module 06 — sync-over-async; this question stacks it with delegate choice.

---

#### Q6. (D) A team replaces every inventory rule interface with `Func<Product, bool>` parameters "to reduce boilerplate." Tests now require copying lambdas from production code. Compare:

```csharp
public interface IProductFilter { bool Include(Product p); }
// After: Func<Product, bool> include everywhere
```

What testability and design seams do you lose, and when is `Func<Product, bool>` still the right API surface?

**Answer:** A named `IProductFilter` (or small record rule type) is a stable, mockable contract with discoverable implementations; bare `Func<Product, bool>` hides intent, prevents polymorphic composition, and pushes test doubles toward duplicating lambda logic instead of substituting a fake rule.

- **Lost seams:** no `Mock<IProductFilter>` / `FakeActiveOnlyFilter`; tests pass inline lambdas that mirror production conditions — refactors break tests silently.
- **Lost composition:** interfaces support chaining decorators (`AndFilter`, `OrFilter`); func parameters encourage copy-paste boolean expressions.
- **Lost discoverability:** "implements `IProductFilter`" is grep-friendly; anonymous funcs are not.
- **When `Func<Product, bool>` is right:** local helpers (`ProcessInventory` in **Program.cs** Section 9), LINQ-shaped APIs (`Where`), one-off pipeline parameters where the caller owns the logic and tests assert on outputs not filter identity.

**Fix (priority order):**

1. Keep domain rules as `IProductFilter` or static named methods (`IsActiveInStock`) method-grouped into funcs at boundaries.
2. Use `Func<Product, bool>` at the **pipeline edge** only — convert `filter.Include` to func internally if needed.
3. For Karat-style judgment: prefer named types on public service contracts; func for internal utility parameters.

**Production takeaway:** Func reduces ceremony but is not a free replacement for interfaces on bounded contexts — Karat tests whether you preserve test seams. **Program.cs** Section 8 — named `LineTotalCalculator` vs `Func` when the name is part of the contract.

---

#### Q7. (P) An API adds a custom endpoint filter using a predicate delegate. Review registration and behavior:

```csharp
builder.Services.AddSingleton<Func<HttpContext, bool>>(_ =>
    ctx => ctx.Request.Headers.ContainsKey("X-Warehouse-Id"));
```

What breaks for multi-tenant routing, testing, and filter ordering compared to `IEndpointFilter` or a typed authorization requirement?

**Answer:** A singleton `Func<HttpContext, bool>` encodes authorization as an untyped header check with no access to route data, user claims, or scoped tenant services — it is hard to test in isolation, runs late if registered inside an ad hoc filter, and cannot participate in policy-based auth.

- **Multi-tenant:** header presence ≠ valid warehouse; no correlation to route `{warehouseId}`, claim, or scoped `ITenantContext` — wrong tenant data can still be served if the header is spoofed without validation.
- **Testing:** must spin `HttpContext` and service provider to test a func pulled from DI; `IAuthorizationService` / policy tests are standard.
- **Ordering:** custom inline filter runs after routing but competes with auth middleware — warehouse checks belong in authorization policy or early middleware, not a one-off func gate duplicated per endpoint.
- **Singleton:** cannot inject scoped tenant/store services into the predicate without captive dependency.

**Fix (priority order):**

1. Replace with policy: `[Authorize(Policy = "WarehouseAccess")]` and `AddAuthorization` handler reading claim + route.
2. If endpoint-specific, implement `IEndpointFilter` as a typed class injecting `ITenantValidator` (scoped) — unit-test the filter class directly.
3. Use built-in `RequireAuthorization()` / `AddEndpointFilter<WarehouseFilter>()` — consistent ordering via `MapGroup` filters.
4. Drop singleton `Func<HttpContext, bool>` from DI — it hides security rules and blocks scoped dependencies.

**Production takeaway:** Func fits local predicates (`Predicate<Product>` on in-memory lists); HTTP gates need typed filters, policies, and scoped services — not a global bool func. Connects to **Program.cs** pipeline pattern (Section 9) at the wrong abstraction layer for ASP.NET.

---

#### Q8. (M) A generic helper tries to widen a discontinued-SKU predicate for use on the full catalog. Review:

```csharp
Predicate<DiscontinuedProduct> discontinuedOnly =
    p => p.EndOfLifeDate.HasValue;

Predicate<Product> catalogFilter = discontinuedOnly; // CS0029
```

Why does assignment fail despite `DiscontinuedProduct : Product`, and how does delegate variance differ from `IEnumerable<T>` assignment?

**Answer:** `Predicate<T>` is declared contravariant (`in T`) — you may assign a **wider** input predicate (`Predicate<Product>`) to a **narrower** slot (`Predicate<DiscontinuedProduct>`), but not the reverse. Widening `Predicate<DiscontinuedProduct>` to `Predicate<Product>` would let `RemoveAll` invoke the rule with plain `Product` instances that are not `DiscontinuedProduct`, so accessing `EndOfLifeDate` would be unsound.

- **`Predicate<in T>` (contravariant):** valid direction — `Predicate<Product> wide = p => p.IsActive; Predicate<DiscontinuedProduct> narrow = wide;` (callers pass `DiscontinuedProduct`, handler accepts any `Product`).
- **Invalid direction (this snippet):** `Predicate<DiscontinuedProduct>` → `Predicate<Product>` — catalog list can contain non-discontinued SKUs; the delegate body assumes derived-only members.
- **vs `IEnumerable<out T>` (covariant):** `IEnumerable<DiscontinuedProduct>` → `IEnumerable<Product>` works because you only **read** items out; delegate parameters are **inputs**, so variance flips.

**Fix (priority order):**

1. Keep the narrow predicate on `List<DiscontinuedProduct>` only; do not widen the delegate type.
2. For mixed catalogs, filter with a lambda that pattern-matches: `items.RemoveAll(p => p is DiscontinuedProduct d && d.EndOfLifeDate.HasValue);`
3. Or extract a safe `Product`-level rule that uses only `Product` members: `Predicate<Product> catalogFilter = p => !p.IsActive;`
4. Reuse `Predicate<Product>` on derived lists via contravariance: `List<DiscontinuedProduct> disc; disc.RemoveAll(wideProductPredicate);` — valid when the stored delegate is `Predicate<Product>`.

**Production takeaway:** Inheritance intuition from collections does not transfer to input delegates — Karat tests contravariance direction, not just "same shape." See **Program.cs** Section 5 — `Predicate<in T>`; variance governs which stored predicate can be reused across base/derived lists.
