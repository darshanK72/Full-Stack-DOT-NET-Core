# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/02. Object Oriented Programming/04. Static Members & Static Classes`

---

#### Q1. (R) An ASP.NET Core API caches the "current user's cart" in a static field so every controller can read it without DI. Under load, users report seeing each other's items. Review the code — what is wrong and how do you fix it?

**Answer:** A static `_items` list is one shared object for the entire application domain — every request overwrites and reads the same cart, so concurrent users bleed data across threads and requests; this is the classic mutable-static-state failure mode in web apps.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Mutable static `_items` holds per-user data | User A sees User B's cart under concurrency |
| Architecture | Static holder bypasses request scope and DI | Hidden global state; untestable without static resets |
| Scale-out | In-memory static state is per process | Sticky sessions won't help — race is on one instance |
| Thread safety | `List<T>` mutated without synchronization | Corrupted list / exceptions under parallel requests |

**Fix (priority order):**

1. Remove `CartContext` static mutable storage — register a **scoped** `ICartService` (or store cart keyed by user id in Redis/SQL).
2. Pass `HttpContext.User` identity into the service; never store "current user" in static fields.
3. If caching shared **read-only** reference data, use `IMemoryCache` or `IOptions<T>` with immutable snapshots — not a static `List` rewritten per request.
4. Add integration tests with parallel HTTP clients to catch cross-user leakage.

**Production takeaway:** Static members are fine for type-level constants and pure helpers (`BankAccount.IsValidRoutingNumber`) — not for request-scoped or user-scoped state. See **Program.cs** Section 1 — "mutable static fields are shared global state."

---

#### Q2. (M) A teammate adds runtime config loading to `AppSettings` and reports intermittent `TypeInitializationException` on first request. Review the static initialization — what ordering traps exist, and how would you make startup deterministic?

**Answer:** Static field initializers run in declaration order before the static constructor body, but circular reads between static fields or throwing initializers can fail type initialization once and poison the type for the AppDomain — the fix is to defer I/O to explicit startup (`IConfiguration`) instead of fragile static ctor chains.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Init order | `MaxLoginAttempts = LoadMaxAttempts()` runs before `static AppSettings()` body | `LoadMaxAttempts` uses `ConfigPath` — OK here, but reordering fields breaks silently |
| Runtime | `LoadMaxAttempts` throws on missing/malformed file | `TypeInitializationException` — type unusable until app restart |
| Design | Static ctor performs I/O and logging | Failures happen on first touch, not at controlled startup |
| Web hosting | First request triggers type load | Lazy failure in prod instead of fail-fast at `WebApplication` boot |

**Fix (priority order):**

1. Move config loading to ASP.NET Core **options pattern** — `builder.Services.Configure<LoginOptions>(configuration.GetSection("Login"))` — validate at startup with `ValidateOnStart`.
2. If static readonly is required, keep static fields **simple** (env var only); load file-backed values in `Program.cs` after `builder.Configuration` exists.
3. Avoid static initializers that depend on each other's side effects; document declaration order or use explicit static ctor assignment only.
4. Never swallow exceptions in static constructors — they wrap inner failures in `TypeInitializationException` and hide root cause in logs.

```csharp
// Prefer at startup, not in static type init:
builder.Services.AddOptions<LoginOptions>()
    .Bind(configuration.GetSection("Login"))
    .Validate(o => o.MaxAttempts > 0, "MaxAttempts required")
    .ValidateOnStart();
```

**Production takeaway:** Static constructors run once per type load (**Program.cs** Section 6 preview) — treat them like hidden startup code. Production apps load config through `IConfiguration`, not static field chains that throw on first access.

---

#### Q3. (R) Production logging uses the tutorial's `AuditLogger` singleton instead of `ILogger`. Tests pass locally but CI flakes and log counts are wrong under concurrent requests. Review the pattern — what's broken and what replaces it?

**Answer:** Hand-rolled singletons expose untestable global mutable state (`_entryCount++` is not thread-safe) and fight ASP.NET Core's built-in logging pipeline — register `ILogger<T>` and scoped/transient services instead of `AuditLogger.Instance`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Concurrency | Non-interlocked `_entryCount++` | Lost updates / wrong counts under parallel requests |
| Testability | Static `Instance` and private ctor | Tests share global counter; order-dependent flakes |
| DI misuse | `AddSingleton(AuditLogger.Instance)` registers pre-built object | Bypasses container ownership; can't substitute fakes easily |
| Observability | `Console.WriteLine` instead of `ILogger` | No levels, filters, structured fields, or centralized sinks |

**Fix (priority order):**

1. Delete the singleton — inject `ILogger<CheckoutController>` (or an application service) via constructor DI.
2. If audit is a domain concern, define `IAuditService` registered **scoped** or **singleton** only when the implementation is **stateless**; persist counts to storage if needed.
3. Use `Interlocked.Increment` only for cheap diagnostics — not as a substitute for proper logging/metrics (`IMeterFactory`, Application Insights).
4. In tests, use `WebApplicationFactory` with logging providers or mock `ILogger<T>` — no static reset hacks.

```csharp
public class CheckoutController : ControllerBase
{
    private readonly ILogger<CheckoutController> _logger;

    public CheckoutController(ILogger<CheckoutController> logger) => _logger = logger;

    public IActionResult Checkout()
    {
        _logger.LogInformation("Checkout completed for {UserId}", UserId);
        return Ok();
    }
}
```

**Production takeaway:** **Program.cs** Section 10 previews singleton for learning — production prefers DI singletons (container-managed, interface-based) over static `Instance` accessors. See foundation **Constructors** chapter for thread-safe lazy init when a true single instance is required.

---

#### Q4. (R) A developer refactors `TaxHelper` to support per-region tax profiles and adds instance state. Build fails. Review the changes — what rules did they violate, and what structure should replace a static class here?

**Answer:** Static classes cannot have instance members or instance constructors — the compiler rejects instance fields and `TaxHelper(decimal)` on a `static class`; once you need per-object state, convert to an ordinary instance class (often injected via DI) and keep only pure functions static if needed.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | Instance field + ctor on `static class` | CS0708 / CS0710 — build blocked |
| Design | Mixed static utility + instance profile in one type | Violates static class purpose (stateless helper group) |
| API | Callers would need `new TaxHelper(...)` | CS0712 — cannot instantiate static class even without other errors |

**Fix (priority order):**

1. Replace `static class TaxHelper` with a normal sealed class, e.g. `ITaxCalculator` / `TaxCalculator`, taking `regionRate` via constructor or options.
2. Register `ITaxCalculator` as scoped or singleton in DI depending on whether rate is per-request or app-wide config.
3. Keep stateless math as `public static decimal CalculateSalesTax(...)` on a separate `TaxMath` static class **or** private static method on the instance class — match **Program.cs** Section 7 (static class = no instance state).
4. Do not inherit from `TaxHelper` — static classes are implicitly sealed; use composition and interfaces instead.

```csharp
public interface ITaxCalculator
{
    decimal CalculateForRegion(decimal amount);
}

public sealed class TaxCalculator : ITaxCalculator
{
    private readonly decimal _regionRate;
    public TaxCalculator(IOptions<TaxOptions> options) => _regionRate = options.Value.Rate;
    public decimal CalculateForRegion(decimal amount) =>
        Math.Round(amount * _regionRate, 2, MidpointRounding.AwayFromZero);
}
```

**Production takeaway:** Static classes (`TaxHelper`, `AppSettings` helpers) are for stateless utilities — the moment you need `this`, use an instance type. Karat tests whether you know CS0712/CS0709 rules from **Program.cs** Section 7, not just memorize `static`.

---

#### Q5. (R) `BankAccount` account numbers duplicate in production after traffic increases. The team uses the tutorial counter as-is. Review the static field usage — what race exists and how do you fix it without abandoning a shared sequence?

**Answer:** `_nextAccountNumber++` is not atomic — two threads can read the same value before either writes back, producing duplicate `AccountNumber` values; use `Interlocked.Increment` for in-process sequences or a database/ID service for authoritative numbering.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Concurrency | Read-modify-write on `_nextAccountNumber++` | Duplicate account numbers under parallel ctor calls |
| Correctness | Assumes single-threaded console demo semantics | Web API creates many `BankAccount` objects concurrently |
| Scale-out | Static counter is per process | Two pods can still collide — DB sequence or distributed ID required |

**Fix (priority order):**

1. **In-process fix:** assign with `Interlocked.Increment(ref _nextAccountNumber)` (or `Interlocked.Add`) inside the constructor.
2. **Production fix:** generate account numbers from SQL `IDENTITY`/sequence, UUID, or Snowflake-style ID service — static fields don't survive multi-instance deployments.
3. Mark `_nextAccountNumber` `private static` and never expose mutability via public static setters.
4. Add stress test spawning parallel account creation tasks asserting unique numbers.

```csharp
public BankAccount(string ownerName, decimal openingDeposit)
{
    AccountNumber = Interlocked.Increment(ref _nextAccountNumber);
    OwnerName = ownerName;
    Balance = openingDeposit;
}
```

**Production takeaway:** **Program.cs** Section 1 warns that mutable static fields race unless synchronized — the tutorial's counter is correct for demos, not for concurrent web registration endpoints.

---

#### Q6. (D) Your API team debates three approaches for shared, read-mostly configuration: `public const` literals, `static readonly` loaded at type init, and mutable `public static` properties set from middleware. Which would you allow in a multi-instance ASP.NET Core deployment, and which would you ban? Why?

**Answer:** Allow `const` for true compile-time literals and immutable `static readonly` only when the value is identical on every instance and never changes after type init; ban mutable `public static` properties for app configuration — use `IOptions<T>` / `IConfiguration` so each pod reads consistent, reloadable, testable settings without global writes from middleware.

**Allow — `const` (e.g., `MaxLoginAttempts = 3`):**

- Fixed at compile time; zero runtime cost; safe to share everywhere.
- Trade-off: changing value requires recompile of all assemblies that inline it (**Program.cs** Section 8 — const metadata inlining).

**Allow with caution — `static readonly` set once at type init (e.g., `EnvironmentName` from env var):**

- OK for process-wide, immutable facts loaded before requests (deployment stamp, machine name).
- Must not read per-request data; env var is fixed for process lifetime.
- Prefer `IOptions<T>` for anything that might reload or differ by environment file.

**Ban — mutable `public static` properties (e.g., `BankAccount.BankName { get; set; }` set from middleware):**

- Creates hidden global state writable from anywhere; race-prone under concurrent requests.
- Multi-instance: each pod has its own static copy — "global" settings drift if one instance mutates.
- Breaks unit tests (order-dependent mutations) and violates DI/test seams.

**Production pattern:** `builder.Services.Configure<BankOptions>(configuration.GetSection("Bank"))` inject `IOptionsSnapshot<BankOptions>` where refresh matters. Keep static classes for pure functions only (`IsValidRoutingNumber`).

**Production takeaway:** **Program.cs** contrasts `const`, `static readonly`, and mutable static properties — in web apps, configuration flows through the options/configuration stack, not static setters touched during the HTTP pipeline.

---
