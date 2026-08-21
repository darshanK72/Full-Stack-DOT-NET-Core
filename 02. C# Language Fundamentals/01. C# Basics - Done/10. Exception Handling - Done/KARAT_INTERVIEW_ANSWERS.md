# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/01. C# Basics - Done/10. Exception Handling - Done`

---

#### Q1. (R) A teammate adds logging around order validation before rethrowing. Review this method — what would you change and why?

**Answer:** Logging only `ex.Message` strips stack trace and inner exceptions from structured logs, and `throw ex` resets the stack trace to this catch block — so on-call engineers see the handler as the fault site instead of `Order.Validate()` or `ProcessPayment`. Log the full exception and rethrow with `throw;`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Observability | `LogError(ex.Message)` — no exception parameter | Serilog / Application Insights cannot index stack trace, type, or `InnerException` chain |
| Diagnostics | `throw ex` resets stack trace | Original throw site in **Program.cs** Section 11 (`LogAndRethrow`) is lost — same anti-pattern as `throw ex` in the inner try |
| Design | Broad `catch (Exception)` without recovery | Catches business declines (`InsufficientFundsException`) the same as bugs — may over-log expected paths |

**Fix (priority order):**

1. Replace `logger.LogError(ex.Message)` with `logger.LogError(ex, "ValidateAndCharge failed for order {OrderId}", order.OrderId)` so the logging provider captures the full exception object.
2. Replace `throw ex` with `throw;` to preserve the original stack trace (See foundation **Exception Handling** — `throw;` vs `throw ex`).
3. Catch narrower types where you can recover (`InsufficientFundsException` → user message) and let unexpected failures propagate after logging, or wrap with an inner exception: `throw new InvalidOrderException(order.OrderId, "Charge failed.", ex)`.

```csharp
catch (Exception ex)
{
    logger.LogError(ex, "ValidateAndCharge failed for order {OrderId}", order.OrderId);
    throw;
}
```

**Production takeaway:** This debrief snippet passes visual review but breaks the first production incident — Karat tests whether you diagnose observability and stack-trace preservation together, not just "log and rethrow."

---

#### Q2. (R) A nightly reconciliation job wraps payment-gateway calls like this. Support reports "job succeeded" but ledger rows are missing after gateway timeouts. What is wrong?

**Answer:** The empty `catch (Exception)` swallows every failure — including `TimeoutException` from the gateway — while the method still returns a success count, so callers and monitors believe orders were charged when they were not.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Empty catch swallows all exceptions | Silent data loss — matches **Program.cs** Section 16a anti-pattern |
| Operability | `processed` increments only on success but job exits normally | Dashboards show green runs; finance finds missing ledger entries |
| Diagnostics | No log of SKU/order id, exception type, or inner cause | Cannot distinguish transient timeout from permanent misconfiguration |

**Fix (priority order):**

1. Never use empty catch — at minimum log with full exception and order context, then continue or fail the batch explicitly.
2. Decide policy: **fail fast** (stop batch, return non-zero exit) vs **partial success** (track failed order ids, emit summary metric) — document which.
3. Catch specific recoverable types (`TimeoutException`) separately from programming errors; rethrow or aggregate unexpected exceptions.
4. For expected gateway timeouts, use retry with backoff — not silent skip.

```csharp
catch (Exception ex)
{
    logger.LogError(ex, "Reconcile failed for order {OrderId}", order.OrderId);
    failedOrderIds.Add(order.OrderId);
}
// return processed + failed lists; exit non-zero if any failed
```

**Production takeaway:** Swallowing exceptions makes batch jobs the worst kind of green — the process completed, but business state is wrong. See **Program.cs** Section 16 — swallow demo vs explicit result strings.

---

#### Q3. (R) Audit entries must always be closed, even when `WriteEntry` throws. A junior developer refactors Section 15 without `using`. Review:

```csharp
public static string WriteAuditTrail(string orderId)
{
    AuditLogWriter audit = new AuditLogWriter();
    audit.WriteEntry($"Payment attempt for {orderId}");
    string trail = audit.LastEntry;
    audit.Dispose();
    return trail;
}
```

What can go wrong in production, and how would you fix it?

**Answer:** If `WriteEntry` throws, `Dispose()` never runs — the audit writer stays open, `[closed]` is never appended, and native handles (file streams in real code) leak until GC finalization. Use `using`, a `try/finally`, or `try/finally` with explicit `Dispose()` so cleanup runs on every path.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Resource leak | `Dispose()` only on happy path | Unclosed streams, held locks, incomplete audit flush |
| Correctness | `_disposed` guard never set on failure path | Subsequent use may write to a half-closed resource |
| Maintainability | Manual dispose easy to break on new return/throw | **Program.cs** Section 4 + 15 teach compiler-generated `finally` via `using` for this reason |

**Fix (priority order):**

1. Restore `using (AuditLogWriter audit = new AuditLogWriter()) { ... }` — compiler emits `try/finally` calling `Dispose()` even when `WriteEntry` throws.
2. Alternatively explicit `try/finally` with null check if construction itself can fail.
3. Prefer C# 8 `using AuditLogWriter audit = new();` when style allows — same guarantee.

```csharp
using (AuditLogWriter audit = new AuditLogWriter())
{
    audit.WriteEntry($"Payment attempt for {orderId}");
    return audit.LastEntry;
} // Dispose always runs
```

**Production takeaway:** `finally` and `IDisposable` exist because success-only cleanup fails under real faults — payment and audit paths are exactly where this bites.

---

#### Q4. (D) A team introduces `InvalidOrderException`, `InsufficientFundsException`, and `CustomerNotFoundException` for every validation failure — including null method parameters and missing optional query filters. When is a custom domain exception the right choice vs `ArgumentException`, a result type, or no throw at all?

**Answer:** Custom exceptions like `InvalidOrderException` and `InsufficientFundsException` fit **business-rule violations on domain entities** where callers catch by type and need structured properties (`OrderId`, `RequestedAmount`). They are the wrong tool for bad parameters, expected "not found" lookups, or control flow.

- **Use domain exceptions** when the rule is part of the entity's integrity (`Order.Validate()` throwing `InvalidOrderException` for quantity ≤ 0 — **Program.cs** Section 3) and upper layers map them to HTTP 400/402 or user-facing decline messages.
- **Use `ArgumentNullException` / `ArgumentException`** for invalid method inputs (null `order`, negative page size) — preconditions, not business outcomes.
- **Use return / `Try*` / result types** for expected outcomes callers handle often (optional filter absent, SKU not in catalog) — **Program.cs** Section 16c `ValidateWithResult` vs throw.
- **Do not inherit `ApplicationException`** — derive from `Exception` directly (**Program.cs** Section 1).
- **Avoid exception explosion** — three similar types with only message differences force catch clutter; one type with an error code enum may suffice.

**Production takeaway:** Custom exceptions are for exceptional **domain** state, not a replacement for validation attributes, result objects, or HTTP semantics — misuse slows hot paths and obscures which failures are operational vs programmer errors.

---

#### Q5. (R) Two implementations look up a product by SKU. One is in code review. Which approach would you approve for a catalog service called millions of times per day, and why?

**Answer:** Approve **B** (`TryGetProduct`) for the high-volume lookup path — missing SKU is an expected outcome, not an exceptional one. Reserve **A** only when absence truly indicates a programmer or configuration error that should fail fast.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Control flow | **A** uses exceptions for "not found" | Exception throw/catch is orders of magnitude slower on hot paths; clutters logs with stack traces for normal browsing |
| API clarity | **A** forces every caller into try/catch | **B** matches **Program.cs** Section 16c — return/Try* for expected flow |
| Semantics | `KeyNotFoundException` is a BCL type | Callers cannot distinguish catalog miss from dictionary bug without message parsing |

**Fix (priority order):**

1. Expose `TryGetProduct` (or `Product? GetProductOrDefault`) as the primary API for user-driven lookups.
2. If **A** remains for internal invariants ("SKU must exist after order commit"), document that contract and keep it off the request hot path.
3. At the API boundary, map `false` from TryGet to 404 — not an unhandled exception.

**Production takeaway:** Exceptions are for **exceptional** conditions — the chapter's payment decline (`InsufficientFundsException`) is appropriate; "customer typed wrong SKU" is not.

---

#### Q6. (P) This console chapter lets `InvalidOrderException` bubble out of `Main` when validation fails. In an ASP.NET Core API, the same unhandled domain exception currently returns a raw 500 HTML page. What centralized pattern replaces scattered try/catch in every controller, and what must differ between Development and Production responses?

**Answer:** Register a **global exception handler** (`IExceptionHandler` + `AddExceptionHandler<T>()` and `UseExceptionHandler()` in .NET 8+, or exception-handling middleware) once in `Program.cs` so unhandled exceptions from any endpoint become a uniform JSON response — controllers stay thin and throw domain types like this chapter teaches.

- **Map domain types deliberately:** `InvalidOrderException` → 400 + `ProblemDetails` with safe message; `InsufficientFundsException` → 402 or 400 with decline detail; unexpected → 500.
- **Development:** richer body — exception detail, stack trace, or `DeveloperExceptionPage` for local debugging (never expose raw stacks to external clients).
- **Production:** log full exception with `LogError(ex, "...")` including `TraceIdentifier`; return RFC 7807 `ProblemDetails` without internal stack or connection strings — same rule as **Program.cs** Section 13 (log `StackTrace` internally, not in UI).
- **Preserve stacks upstream:** middleware sees the original trace only if lower layers used `throw;` or wrapped with `inner` — not `throw ex` (Q1).
- **Avoid duplicating catch blocks** in every action; handle expected failures locally only when the response shape differs (e.g., 201 vs 404).

```csharp
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
// ...
app.UseExceptionHandler();
```

**Production takeaway:** Console `Main` terminating on unhandled exceptions is the same failure mode as an unhandled API exception — centralized handling is the web equivalent of "one place decides log + user-safe message + status code." Full implementation lives in **05. ASP.NET Core/10. Exception Handling**.

---
