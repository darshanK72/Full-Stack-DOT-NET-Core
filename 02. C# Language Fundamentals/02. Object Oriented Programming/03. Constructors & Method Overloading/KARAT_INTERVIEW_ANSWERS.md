# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/02. Object Oriented Programming/03. Constructors & Method Overloading`

---

#### Q1. (R) A teammate refactors `OrderLine` to chain constructors like the chapter's `Product` type. QA reports invalid lines in production — empty SKU and zero quantity slip through. Review the ctors. What went wrong, and how do you fix it?

**Answer:** The single-parameter constructor does not chain to the validated `(string, int)` ctor — it duplicates initialization logic without guards, so callers using `new OrderLine("")` or `new OrderLine(null)` get empty SKUs that never hit the validation in the three-parameter constructor.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Design | `OrderLine(string sku)` bypasses `: this(sku, 1)` | Invalid SKU values reach production data |
| Correctness | `sku?.Trim() ?? string.Empty` masks null instead of rejecting | Silent bad state instead of fail-fast at creation |
| Maintainability | Validation duplicated in intent but only implemented once | Future ctors can repeat the same bypass mistake |

**Fix (priority order):**

1. Chain the one-parameter ctor: `public OrderLine(string sku) : this(sku, 1) { }` — single validation path.
2. Keep all invariant checks in the **most complete** ctor (here, `(string sku, int quantity)`), matching the chapter's `Product` pattern in **Program.cs** Sections 1c and 1b.
3. Remove defensive null-coalescing to empty string in convenience ctors — let the validated ctor throw `ArgumentException`.
4. Add unit tests per ctor overload to assert invalid SKU/quantity throws before any repository write.

```csharp
public OrderLine(string sku)
    : this(sku, 1)
{
}
```

**Production takeaway:** Constructor chaining only enforces invariants when **every** ctor path reaches the guarded ctor — a common Karat trap after "helpful" shortcut ctors are added without `: this(...)`.

---

#### Q2. (R) A .NET 8 service adopts a **primary constructor** for a warehouse DTO. Unit tests expecting `ArgumentException` on bad input fail with `NullReferenceException` instead. Review the type. What is the initialization order problem, and how would you enforce invariants?

**Answer:** Field initializers on the primary-constructor type run **before** the instance constructor body block, so `sku.Trim()` executes while `sku` is still null — throwing `NullReferenceException` instead of the intended `ArgumentException` from the guard block below.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | `Sku = sku.Trim()` before validation block | Wrong exception type; callers/tests cannot rely on contract |
| Correctness | Invariants assumed to run "first" in `{ }` body | Primary ctor initialization order differs from mental model |
| API contract | Mixed primary params + property initializers | Hard to see which line can throw what |

**Fix (priority order):**

1. Validate **before** any use of parameters — either in the constructor body as the first statements with manual assignment to properties, or via a static factory `StockReceipt.Create(...)` that validates then calls a private ctor.
2. Do not call instance methods (`Trim`) on parameters in field/property initializers when null is invalid.
3. Prefer explicit parameterized ctor + chaining for domain types with strict invariants; use primary constructors for simple immutable carriers where validation is minimal or delegated to a factory.
4. Align tests to assert the final exception type after fix (`ArgumentException` for null/whitespace SKU).

```csharp
public sealed class StockReceipt
{
    public string Sku { get; }
    public decimal UnitCost { get; }
    public int Quantity { get; }

    public StockReceipt(string sku, decimal unitCost, int quantity)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU is required.", nameof(sku));
        if (unitCost < 0m)
            throw new ArgumentOutOfRangeException(nameof(unitCost));
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        Sku = sku.Trim();
        UnitCost = unitCost;
        Quantity = quantity;
    }
}
```

**Production takeaway:** Primary constructors do not replace the chapter rule — **enforce invariants at creation** — but the execution order is initializer expressions first, then body; Karat tests whether you know where validation must live.

---

#### Q3. (R) After adding a convenience overload to `LineItemCalculator`-style pricing helpers, `dotnet build` fails with **CS0121** ("The call is ambiguous"). Which overloads conflict, and how do you resolve the call site or signatures?

**Answer:** The call `LineTotal(3, 2.49m, 0.10m)` matches both the three-parameter overload (with optional `discountRate`) and the four-parameter overload equally well — the third argument `0.10m` can bind to either `discountRate` or the third positional parameter before `taxRate`, so the compiler cannot pick a unique best match.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | Optional parameter on overload A overlaps arity with overload B | CS0121 — build blocked |
| Design | Two overloads differ only by trailing optional vs required extension | Call sites with three decimals are ambiguous |
| Maintainability | Mixing optional params and extra overloads (chapter Section 9 warning) | Every new decimal argument risks new ambiguity |

**Fix (priority order):**

1. **Preferred:** Remove the optional from the three-parameter signature — use two explicit overloads (`qty, price` and `qty, price, discount`) plus a separate `WithTax(...)` method, mirroring **Program.cs** `Price(int, decimal)` vs `Price(int, decimal, decimal)`.
2. At the call site, disambiguate with a **named argument**: `LineTotal(3, 2.49m, discountRate: 0.10m)` if you must keep the optional temporarily.
3. Avoid `params` + optional + overlapping arity in the same method group — chapter Section 15 / CS0121.
4. Add a compiler-focused unit test project or analyzer rule comment so overlapping optionals are caught in review.

**Production takeaway:** Overload resolution is compile-time — ambiguous APIs never ship — but Karat uses this to test whether you can diagnose **optional parameters colliding with additional overloads**, not just recall the CS0121 code.

---

#### Q4. (M) A junior dev models discounted inventory items by inheriting from `Product` (chapter pattern). The project does not compile. Diagnose **`: this(...)` vs `: base(...)`** mistakes and state the correct ctor initialization order.

**Answer:** Attempt A lists **both** `: base(...)` and `: this(...)` on one constructor header — only one initializer is allowed (CS2506). Attempt B omits `: base(...)` while `Product` has no parameterless ctor, so the compiler cannot construct the base part (CS7036). Derived ctors must eventually reach the parent through `: base(...)`; `: this(...)` only delegates to another ctor in the **same** derived type. Initialization order: static base → static derived → instance base → instance derived → base ctor body → derived ctor body.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `: base(...)` and `: this(...)` on Attempt A | CS2506 — only one constructor initializer permitted |
| Compile | Attempt B missing `: base(name, unitPrice)` | CS7036 — no accessible parameterless base ctor |
| Design | Treating `: this(...)` as a way to call the parent | Parent state never constructed; validation in `Product` skipped |

**Fix (priority order):**

1. Split Attempt A into **either** sibling delegation **or** base forwarding — not both on one header:
   `public DiscountedProduct(string name, decimal unitPrice, decimal discountRate) : this(name, unitPrice, discountRate, applyMinimum: true) { }`
2. Fix Attempt B: `public DiscountedProduct(..., bool applyMinimum) : base(name, unitPrice) { DiscountRate = discountRate; }`.
3. Ensure every `: this(...)` chain in the derived class terminates at a ctor that calls `: base(...)` so `Product` validation runs — matching chapter **Program.cs** Section 3 preview.
4. Document ctor order in review: base static → derived static → base instance → derived instance → base ctor → derived ctor.

**Production takeaway:** `: this(...)` chains within a type; `: base(...)` crosses inheritance — Karat stacks this with CS7036 when the base parameterless ctor disappears after adding a parameterized base ctor.

---

#### Q5. (P) An ASP.NET Core API maps inbound JSON to a **`required`** init-only request type before calling domain ctors. A client omits `Name` but the payload still deserializes and reaches `new Product(...)`. What happened at compile time vs runtime, and how do you align API contracts with constructor validation?

**Answer:** `required` is enforced at **object creation** for object initializers and `new()` expressions at compile time, but **System.Text.Json** (and Newtonsoft) can still materialize instances without required members unless you enable required-member deserialization validation — so `Name` may default to `null` at runtime, and `Product`'s ctor then throws or mis-validates depending on null checks.

- **Compile time:** `new CreateProductRequest { UnitPrice = 8.99m }` without `Name` fails to compile — `required` works for in-code construction.
- **Runtime (JSON):** Deserializer may not enforce `required` unless configured (`JsonSerializerOptions` / `[JsonRequired]` / validation attributes / manual guard in minimal APIs).
- **Domain layer:** `Product(string name, decimal unitPrice)` should still validate — last line of defense — but the API should return **400 ProblemDetails**, not a 500 from an unhandled `ArgumentException`.
- **Alignment:** Use `[Required]` + FluentValidation or ASP.NET model validation, enable required property support for STJ in .NET 7+, map to domain via factory that throws typed validation exceptions converted to 400.

```csharp
// Minimal API guard example:
if (string.IsNullOrWhiteSpace(body.Name))
    return Results.ValidationProblem(new Dictionary<string, string[]>
    {
        [nameof(body.Name)] = ["Name is required."]
    });
```

**Production takeaway:** Required members and constructor validation solve different layers — DTO `required` for developer mistakes, ctor invariants for domain truth, API validation for external clients — Karat tests stacking all three.

---

#### Q6. (D) A warehouse microservice registers services in DI but still constructs dependencies manually inside ctors. Review startup and `InventorySyncService`. What breaks in tests, lifetimes, and startup, and what pattern replaces it?

**Answer:** The service graph mixes DI registration with static singleton access and throws inside `ProductCatalog`'s ctor during container build — startup fails (or the host never becomes healthy), tests cannot substitute a fake registry, and two lifetimes (DI singleton vs static `Instance`) fight for the same responsibility.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Startup | `new Product("Seed SKU", -1.00m)` in `ProductCatalog` ctor | `ArgumentOutOfRangeException` during `BuildServiceProvider` — app won't start |
| DI | `InventorySyncService` uses `InventoryRegistry.Instance` | Bypasses container; cannot mock `IInventoryRegistry` in tests |
| Lifetime | Static singleton + `AddSingleton<>` duplicate ownership | Hidden global state; unclear thread-safety and test isolation |
| Design | Chapter singleton (`InventoryRegistry`) copied into production service | Violates "prefer DI" note in **Program.cs** Section 2 |

**Fix (priority order):**

1. **Constructor injection:** `public InventorySyncService(IInventoryRegistry registry)` — no parameterless ctor grabbing statics.
2. Register an abstraction: `builder.Services.AddSingleton<IInventoryRegistry, InventoryRegistry>()` with a **public or internal** ctor (or factory delegate) — retire `Instance` for app code; keep private ctor only if factory registration is used.
3. Move seed data out of the ctor — use `IHostedService`, explicit `SeedAsync`, or configuration-driven load so invalid catalog data surfaces as a controlled startup error with logging, not ctor throw during DI resolution.
4. Let `Product`'s validated ctor throw for bad **runtime** input; seed paths must pass valid arguments or use a dedicated test factory.
5. Integration tests build `WebApplicationFactory` with replaced `IInventoryRegistry` fake — only possible when ctors demand interfaces.

```csharp
builder.Services.AddSingleton<IInventoryRegistry, InventoryRegistry>();
builder.Services.AddSingleton<IInventorySyncService, InventorySyncService>();

public sealed class InventorySyncService : IInventorySyncService
{
    private readonly IInventoryRegistry _registry;

    public InventorySyncService(IInventoryRegistry registry)
    {
        _registry = registry;
    }

    public void Sync(Product product) => _registry.Register(product);
}
```

**Production takeaway:** Object creation belongs in the composition root — ctors enforce invariants for **their** parameters, not for bootstrapping entire graphs via `new` and static `Instance`; Karat links chapter singleton intro to real ASP.NET Core registration mistakes.

---
