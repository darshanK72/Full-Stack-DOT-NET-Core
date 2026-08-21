# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/01. C# Basics - Done/05. Type Conversion & Casting - Done`

---

#### Q1. (R) A warehouse pricing service receives quantities from an upstream JSON deserializer boxed as `object`. Review this method — what fails at runtime, and how would you fix it?

**Answer:** Unboxing requires the cast target to match the exact boxed type — `(long)quantityBoxed` throws `InvalidCastException` because the heap object holds an `int`, not a `long`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | `(long)` unbox on object boxed as `int` | `InvalidCastException` — order total calculation fails mid-request |
| Type model | Treating `object` as a generic numeric without inspection | Fragile when upstream changes serializer or numeric width |
| Design | No validation before arithmetic | Bad data propagates into pricing instead of failing fast at boundary |

**Fix (priority order):**

1. Unbox to the exact stored type: `int units = (int)quantityBoxed`, or use pattern matching: `if (quantityBoxed is int units)`.
2. Prefer strongly typed parameters (`int unitsOrdered`) at service boundaries — avoid `object` for scalars unless truly dynamic.
3. If the source type varies at runtime, branch with `is int`, `is long`, etc., or normalize to `decimal`/`long` at the API edge once.
4. Add a unit test that boxes `int` and asserts no exception — mirrors **Program.cs** Section 9 boxing demo.

**Production takeaway:** Boxing/unboxing looks harmless in tutorials; Karat uses it to test whether you know unbox casts are exact-type, not widening. See foundation **Type Conversion** — `(long)boxed` when boxed as `int` throws.

---

#### Q2. (R) An ASP.NET Core order API accepts a quantity path segment. Review the action — what breaks for bad input, and what would you change?

**Answer:** `int.Parse` throws `FormatException` or `OverflowException` on non-numeric or out-of-range path values — ASP.NET turns that into a 500 instead of a client-facing 400 with a clear validation message.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | `Parse` on untrusted route segment `"abc"` or `"99999999999999999999"` | Unhandled exception → 500 Internal Server Error |
| API contract | Validation runs only after a throwing parse | Positive-quantity check never reached on bad text |
| Operability | Exception-based control flow on hot path | Noisy logs; harder to distinguish client mistakes from server bugs |

**Fix (priority order):**

1. Replace with `int.TryParse(quantity, out int units)` — return `BadRequest` when `false`.
2. Optionally bind with `[FromRoute] int quantity` and let model binding produce 400 for non-int routes (framework handles format).
3. Keep range checks (`units <= 0`, max order size) after successful parse.
4. Reserve `Parse` for trusted constants (config, test fixtures) — matches **Program.cs** Section 5 guidance.

```csharp
if (!int.TryParse(quantity, out int units) || units <= 0)
    return BadRequest("Quantity must be a positive integer.");
```

**Production takeaway:** Parse vs TryParse is not stylistic — on HTTP boundaries, throwing parse crashes the request pipeline. See **ParseDemo** — `TryParse` for external input, `Parse` for known-good text.

---

#### Q3. (R) A bonus-units endpoint maps optional query text to an integer. Review both methods — which hidden behavior causes incorrect totals in production?

**Answer:** `Convert.ToInt32(null)` silently returns `0`, so a missing optional bonus query applies zero bonus without error — while `int.Parse(null!)` throws `ArgumentNullException` and fails the request loudly.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `Convert.ToInt32` maps null → default `0` | Missing `?bonus=` treated as "zero bonus" — under-credits or wrong pricing |
| Runtime | `int.Parse` on null (even with `!`) | `ArgumentNullException` — 500 for absent optional parameter |
| API design | No tri-state (missing vs zero vs invalid) | Cannot distinguish "no bonus specified" from "bonus is literally 0" |

**Fix (priority order):**

1. Model optional input explicitly: `int? bonus = string.IsNullOrWhiteSpace(bonusText) ? null : int.TryParse(...) ? n : throw/400`.
2. Do not use `Convert.ToInt32` for optional HTTP query parameters unless zero-is-correct is a documented business rule.
3. Remove null-forgiving on `Parse` — it hides nullability warnings without making null valid.
4. Return `400` for malformed text; omit bonus logic when parameter is absent.

**Production takeaway:** The Convert class is convenient for legacy `object`/`DBNull` pipelines (**Program.cs** Section 7), but its null→default behavior is a silent data bug on optional API fields. Prefer `TryParse` + nullable types at boundaries.

---

#### Q4. (R) Inventory assigns shelf slot IDs stored in a `byte` column. Review this service — what corrupts data silently, and how do you prevent it?

**Answer:** Narrowing `(byte)inventoryLocationId` in default unchecked context wraps values above 255 — location `300` persists as slot `44` (`300 % 256`) with no exception, corrupting bin assignments.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Unchecked narrowing cast int → byte | Silent wrap — wrong shelf slot in database |
| Data integrity | No validation before SQL write | Corruption spreads to picking, shipping, audits |
| Observability | No exception or log on overflow | Bug discovered only when physical inventory mismatches |

**Fix (priority order):**

1. Validate range before cast: `if (locationId is < 0 or > 255) throw/return error`.
2. Use `checked((byte)locationId)` to throw `OverflowException` if you prefer fail-fast over manual bounds check.
3. Consider widening the column to `smallint`/`int` if business IDs legitimately exceed 255 — schema fix beats repeated casts.
4. Add tests for boundary values 255, 256, 300 — mirrors **OverflowCastDemo** in **Program.cs** Section 4.

**Production takeaway:** Explicit casts truncate or wrap; they never round. Production inventory code needs validation or `checked` context — Karat tests whether you treat narrowing as a data-loss operation, not a free conversion.

---

#### Q5. (R) A shipping label builder walks a heterogeneous `List<object>` of line items. Review this code — what throws or returns wrong data?

**Answer:** After `as DigitalLineItem` fails for unknown or null items, accessing `digital.DownloadCode` throws `NullReferenceException` — and the redundant cast after `is PhysicalLineItem` shows incomplete pattern-matching adoption.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | `digital.DownloadCode` when `as` returned null | `NullReferenceException` for unsupported line types |
| Style / maintainability | `is` check then manual `(PhysicalLineItem)` cast | Duplicated type test; use pattern matching instead |
| Compile-time | `lineItem as int` on `object` | Invalid — `as` only works with reference and nullable value types, not unboxing boxed ints (**Program.cs** Section 10) |

**Fix (priority order):**

1. Replace physical branch with pattern: `if (lineItem is PhysicalLineItem physical) return physical.Sku;`.
2. Guard digital branch: `if (lineItem is DigitalLineItem digital) return digital.DownloadCode;`.
3. Return fallback or throw a domain exception for unrecognized types — never dereference unchecked `as` results.
4. For boxed value types, use `is int n` or `(int)obj`, not `as int`.

```csharp
return lineItem switch
{
    PhysicalLineItem physical => physical.Sku,
    DigitalLineItem digital => digital.DownloadCode,
    _ => throw new InvalidOperationException($"Unknown line item type {lineItem?.GetType().Name}")
};
```

**Production takeaway:** `as` returns null on failure — production code must null-check or prefer `is` patterns that assign in one step. See **IsAsDemo** — interface and derived-type inspection on `object` references.

---

#### Q6. (P) A partner integration POSTs prices as formatted strings in JSON (`"amountText": "1.234,56"`). The API runs on en-US servers. Review the handler — what fails across environments, and what contract would you enforce?

**Answer:** `decimal.Parse` without `IFormatProvider` uses the current thread culture — en-US servers reject `"1.234,56"` (German grouping/decimal) or misread `"1,234.56"`, causing intermittent `FormatException` or wrong stored prices depending on deployment region.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Culture-dependent parse on API string field | Same JSON payload succeeds in one region, fails or mis-parses in another |
| API contract | Locale-formatted strings in machine-to-machine JSON | Partners must guess server culture; brittle integration |
| Runtime | `FormatException` on valid partner data | 500 or skipped catalog updates in production |

**Fix (priority order):**

1. **Best contract:** Accept JSON numbers (`"amount": 1234.56`) — deserialization handles invariant binary representation; no parse step.
2. If strings are required, parse with `CultureInfo.InvariantCulture` (or documented partner culture): `decimal.Parse(dto.AmountText, NumberStyles.Number, CultureInfo.InvariantCulture)`.
3. Prefer `decimal.TryParse` and return `400 ProblemDetails` with field-level validation errors.
4. Document and test both `"1234.56"` invariant and reject ambiguous formats — align with **ParseDemo** European `de-DE` example in **Program.cs** Section 5.

```csharp
if (!decimal.TryParse(dto.AmountText, NumberStyles.Number,
        CultureInfo.InvariantCulture, out var price))
    return ValidationProblem(/* ... */);
```

**Production takeaway:** Culture-aware parsing belongs where locale is intentional (UI, printed invoices). HTTP API bodies should use invariant culture or native JSON numeric types — Karat stacks globalization with API design in one question.

---
