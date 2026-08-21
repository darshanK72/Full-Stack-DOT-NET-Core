# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/03. Generics & Collections/01. Generics`

---

#### Q1. (R) A teammate adds a generic repository helper for warehouse stock rows. `dotnet build` fails. Review the constraint stack — what is wrong, and how do you fix it?

**Answer:** `where T : struct, StockEntry, new()` is illegal — a type parameter cannot be both a non-nullable value type (`struct`) and a reference-type base class (`StockEntry`). The compiler rejects the constraint combination before any call site is evaluated.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `struct` + base class `StockEntry` on same `T` | CS0454 — mutually exclusive constraints; build blocked |
| Design | `_cache` stores `StockEntry` but method returns `T` with value-type constraint | Even if it compiled, boxing/unified cache semantics would be wrong |
| API misuse | `new T { Sku = sku }` assumes `T` is a reference type with mutable `Sku` | Value-type `T` could not inherit `StockEntry` anyway |

**Fix (priority order):**

1. Drop `struct` — use `where T : StockEntry, new()` if you truly need default-constructible inventory rows (`InventoryItem`, etc.).
2. If value-type rows are required, do **not** inherit `StockEntry`; use a separate generic struct path (e.g., `Quantity<TUnit> where TUnit : struct`) or a shared interface instead of a class base.
3. Type the cache as `Dictionary<string, T>` inside a generic class `StockRepository<T> where T : StockEntry, new()`, not a mixed `Dictionary<string, StockEntry>` with an inconsistent method signature.
4. Align with this chapter's `DescribeStockEntry<T> where T : StockEntry` — base-class constraints apply to reference types in the inheritance hierarchy.

**Production takeaway:** Constraint misuse is a compile-time gate — Karat tests whether you recognize that `class`/base-type and `struct` constraints exclude each other. See **Program.cs** Sections 7–9 — constraint combinations.

---

#### Q2. (R) A developer "fixes" a method that accepts any payload list by widening to `List<object>`. Review the assignment and call site:

**Answer:** `List<T>` is **invariant** — `List<string>` is not assignable to `List<object>` because that would allow adding non-strings through the wider reference. Covariance applies only on interfaces like `IEnumerable<out T>` for **read-only** projection, not on mutable lists.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `AuditSkus(warehouseSkus)` with `List<object>` parameter | CS1503 — cannot convert `List<string>` to `List<object>` |
| Runtime | Cast `IEnumerable<object>` to `List<object>` and `Add(42)` | `InvalidCastException` — sequence is backed by `List<string>`, not `List<object>` |
| Design | Treating covariance as "free widening" for mutable collections | Silent data corruption if the language allowed it — type safety violated |

**Fix (priority order):**

1. For read-only aggregation, accept `IEnumerable<string>` or `IReadOnlyList<string>` — precise, no widening needed.
2. For heterogeneous payloads, use `List<object>` at the **source** (accept the boxing cost consciously) or a discriminated model (`List<StockPayload>` / union type).
3. Use `IEnumerable<object> widened = warehouseSkus` only when consuming items — never cast back to a mutable `List<object>` to add elements.
4. Remember: `IEnumerable<out T>` covariance lets you pass `IEnumerable<string>` where `IEnumerable<object>` is expected, but you still cannot mutate element types.

**Production takeaway:** Confusing `List<T>` invariance with `IEnumerable<out T>` covariance is a common review failure — matches **Program.cs** Section 13 and Quick Reference variance rows.

---

#### Q3. (R) An API endpoint helper should return the larger of two comparable stock metrics without boxing value types. Review the call chain:

**Answer:** Generic method inference requires a **single** type argument `T` that fits both parameters — `decimal` and `int` disagree, so the compiler cannot infer `T` (CS0411). Forcing `MaxOf<decimal>` then fails because `int` is not implicitly convertible to `decimal` at the call site (CS1503).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `MaxOf(priceA, unitsB)` — mismatched argument types | CS0411 — type arguments cannot be inferred from the arguments |
| Compile | `MaxOf<decimal>(priceA, unitsB)` | CS1503 — `int` cannot be passed where `decimal` expected |
| Design | One generic `MaxOf<T>` used across unrelated metrics | API encourages comparing apples to units — domain error masked as generic error |

**Fix (priority order):**

1. Call with **homogeneous** types: `MaxOf(priceA, otherPrice)` or `MaxOf(unitsA, unitsB)`.
2. If conversion is intentional, convert explicitly **before** the call: `MaxOf(priceA, (decimal)unitsB)` — documents that the comparison is cross-domain and may be wrong business-wise.
3. Prefer domain methods (`MaxPrice`, `MaxUnits`) or `INumber<T>` (.NET 7+) helpers where numeric widening is well-defined.
4. Do not rely on inference when types differ — specify intent at the call site or split overloads.

**Production takeaway:** Inference failures often signal a design smell — Karat checks that you read CS0411/CS1503 as "one T for all parameters," not as a compiler bug. See **Program.cs** Section 8 — `Swap<T>` inference requires matching types.

---

#### Q4. (M) A hot inventory path stores millions of pallet counts per hour. One service uses `List<object>` "for flexibility"; another uses `List<int>`. Review the read loop:

**Answer:** For each closed constructed type, the JIT specializes `List<T>.Add` and indexer access — `List<int>` stores unboxed ints in a `T[]` with no per-element heap boxing, while `List<object>` boxes every `int` on `Add` and unboxes on read, doubling heap traffic and cache pressure.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime / GC | Boxing 1M ints into `List<object>` | 1M heap allocations + GC pressure; slower hot loop |
| Runtime | Unbox + cast in `legacySum` loop | Extra CPU per iteration vs direct `int` access |
| JIT | Shared vs specialized code paths | `List<int>` gets efficient `int[]` storage; `List<object>` always handles references |
| Design | "Flexibility" on a numeric hot path | Latency spikes under load; harder to reason about in profiling |

**Fix (priority order):**

1. Use `List<int>` (or `Span<int>`, `int[]`, `ImmutableArray<int>`) for homogeneous numeric streams — matches **LegacyCollectionProbe** vs `List<int>` in **Program.cs** Section 1.
2. If mixed types are required, isolate boxing to boundaries (parse → strongly typed model) rather than the inner loop.
3. Accept `List<object>` only at integration seams (legacy APIs, `ArrayList` interop) with explicit conversion at the edge.
4. Profile with dotMemory / PerfView — boxed collections show as `System.Int32` allocations in GC heaps.

**Production takeaway:** Generics exist partly to eliminate boxing on value-type collections — Layer 2 tests whether you connect language feature to production GC behavior, not just "compile-time safety."

---

#### Q5. (R) A factory method should default-construct inventory DTOs for an import pipeline. Review:

**Answer:** `CreateRow<ImportedLine>()` succeeds — records with a primary constructor still get a synthesized parameterless constructor for `new()` when not explicitly removed. `CreateRow<PalletTag>()` fails — `PalletTag` only declares `PalletTag(int zoneId)`, so it does not satisfy `where T : new()` (CS0310).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `CreateRow<PalletTag>()` | CS0310 — `PalletTag` must have public parameterless constructor |
| Design | `new()` constraint on a factory used for both records and custom structs | Call sites look uniform but only some types qualify |
| Runtime / API | Mutating `row.Sku` on a record instance | Works here, but immutable record designs may prefer `with` instead of post-`new()` mutation |

**Fix (priority order):**

1. For `PalletTag`, add an explicit parameterless ctor **only if** default construction is valid: `public PalletTag() : this(0) { }` — or stop using `new()` for that type.
2. Split factories: `CreateRecord<T>() where T : new()` for DTOs; dedicated `PalletTag CreateTag(int zoneId)` for parameterized structs.
3. Prefer `Activator.CreateInstance<T>()` or DI-backed factories when construction needs parameters or injection — `new()` is for simple default graphs only.
4. Validate at compile time with tests that call `CreateRow<T>()` for every supported import row type.

**Production takeaway:** The `new()` constraint means "public parameterless constructor exists" — not "any struct" or "any record." See **Program.cs** `CreateDefault<T>() where T : new()` and Section 9a.

---

#### Q6. (R) A library author exposes typed domain exceptions via generics "so callers can catch exactly what they need." Review:

**Answer:** `throw new TException()` where `TException : Exception, new()` produces **parameterless** exceptions with no message, no inner exception, and no structured context — callers catch the right type but lose SKU, quantity, and stack context. Reusing the helper for `InvalidOperationException` by passing `0` to `EnsurePositive` is a semantic hack that obscures intent.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Design | Generic exception factory anti-pattern | Empty exceptions — useless logs and support tickets |
| Observability | `new TException()` only — no message/data | `_logger.LogWarning(ex, …)` has nothing actionable; APM groups by type only |
| API contract | `EnsurePositive<InvalidOperationException>(0)` for unknown SKU | Wrong exception type and wrong guard — conflates validation with business rules |
| Maintainability | Callers depend on **type** not **error shape** | Adding fields/codes requires new exception types instead of stable error codes |

**Fix (priority order):**

1. Throw **specific, constructed** exceptions: `throw new ArgumentOutOfRangeException(nameof(units), units, "Units must be positive.");`
2. Replace generic throw helpers with domain exceptions (`UnknownSkuException`) or `Result`/validation types for expected failures.
3. Use `ExceptionDispatchInfo` or `throw;` to preserve stack when rethrowing — never `throw new TException()` as a stand-in for wrapping.
4. For libraries, document thrown types in XML docs; avoid letting consumers catch generic `TException` via your helper.

```csharp
if (units <= 0)
{
    throw new ArgumentOutOfRangeException(nameof(units), units, "Reserve quantity must be positive.");
}

if (!IsKnownSku(sku))
{
    throw new InvalidOperationException($"SKU '{sku}' is not in the catalog.");
}
```

**Production takeaway:** Generics + `new()` on exceptions looks clever but fights .NET exception design — production code favors explicit throws with messages and structured error models. See foundation **Exception Handling** — `throw` vs `throw ex` for stack preservation when rethrowing.

---
