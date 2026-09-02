# Interview Q&A — C# Exception Handling


## Table of Contents

1. [Q1. What is the execution order of try / catch / finally, and when does each block run?](#q1-what-is-the-execution-order-of-try-catch-finally-and-when-does-each-block-run)
2. [Q2. How is the .NET exception hierarchy structured, and where do custom domain exceptions fit?](#q2-how-is-the-net-exception-hierarchy-structured-and-where-do-custom-domain-exceptions-fit)
3. [Q3. Why must catch clauses be ordered from most specific to most general, and what happens if they are not?](#q3-why-must-catch-clauses-be-ordered-from-most-specific-to-most-general-and-what-happens-if-they-are-not)
4. [Q4. What is the difference between `throw;` and `throw ex;`, and why does it matter?](#q4-what-is-the-difference-between-throw-and-throw-ex-and-why-does-it-matter)
5. [Q5. When does `finally` NOT run?](#q5-when-does-finally-not-run)
6. [Q6. How do you design a custom exception class in C#, and what constructors should it expose?](#q6-how-do-you-design-a-custom-exception-class-in-c-and-what-constructors-should-it-expose)
7. [Q7. What is an inner exception, how do you create one, and how do you walk the chain?](#q7-what-is-an-inner-exception-how-do-you-create-one-and-how-do-you-walk-the-chain)
8. [Q8. What exception properties are most valuable in production logging, and which should never be exposed to end users?](#q8-what-exception-properties-are-most-valuable-in-production-logging-and-which-should-never-be-exposed-to-end-users)
9. [Q9. How do exception filters (`catch when`) work, and what are the rules for when they run?](#q9-how-do-exception-filters-catch-when-work-and-what-are-the-rules-for-when-they-run)
10. [Q10. What is `AggregateException`, when does the runtime produce one, and how do you handle it correctly?](#q10-what-is-aggregateexception-when-does-the-runtime-produce-one-and-how-do-you-handle-it-correctly)
11. [Q11. What is `ExceptionDispatchInfo` and what problem does it solve compared to plain rethrow?](#q11-what-is-exceptiondispatchinfo-and-what-problem-does-it-solve-compared-to-plain-rethrow)
12. [Q12. When should you use `ArgumentException` and its subtypes instead of a custom domain exception?](#q12-when-should-you-use-argumentexception-and-its-subtypes-instead-of-a-custom-domain-exception)
13. [Q13. How does the `using` statement provide exception-safe resource cleanup, and what does the compiler emit?](#q13-how-does-the-using-statement-provide-exception-safe-resource-cleanup-and-what-does-the-compiler-emit)
14. [Q14. What are the key anti-patterns in exception handling, and why is each one harmful?](#q14-what-are-the-key-anti-patterns-in-exception-handling-and-why-is-each-one-harmful)
15. [Q15. What is the difference between `Exception.Message` and logging the exception object itself?](#q15-what-is-the-difference-between-exceptionmessage-and-logging-the-exception-object-itself)
16. [Q16. What does `ObjectDisposedException` indicate, and how should `IDisposable` types guard against use-after-dispose?](#q16-what-does-objectdisposedexception-indicate-and-how-should-idisposable-types-guard-against-use-after-dispose)
17. [Q17. A `catch when` filter is evaluated but the type matches — can the exception still escape?](#q17-a-catch-when-filter-is-evaluated-but-the-type-matches-can-the-exception-still-escape)
18. [Q18. If an exception is thrown inside a `finally` block, what happens to the original exception?](#q18-if-an-exception-is-thrown-inside-a-finally-block-what-happens-to-the-original-exception)
19. [Q19. When you `await Task.WhenAll(...)`, which exceptions do you see in the catch block?](#q19-when-you-await-taskwhenall-which-exceptions-do-you-see-in-the-catch-block)
20. [Q20. What happens if you rethrow an exception using `throw;` when you captured it with `ExceptionDispatchInfo.Throw()`?](#q20-what-happens-if-you-rethrow-an-exception-using-throw-when-you-captured-it-with-exceptiondispatchinfothrow)
21. [Q21. Can a `catch (Exception)` block catch a `StackOverflowException` or `OutOfMemoryException`?](#q21-can-a-catch-exception-block-catch-a-stackoverflowexception-or-outofmemoryexception)
22. [Q22. (R) A teammate's order service method logs exceptions before rethrowing. Review this code and identify all defects.](#q22-r-a-teammates-order-service-method-logs-exceptions-before-rethrowing-review-this-code-and-identify-all-defects)
23. [Q23. (R) A nightly reconciliation job wraps each order in try/catch but support reports "job succeeded" while ledger rows are missing after gateway timeouts. Find all defects.](#q23-r-a-nightly-reconciliation-job-wraps-each-order-in-trycatch-but-support-reports-job-succeeded-while-ledger-rows-are-missing-after-gateway-timeouts-find-all-defects)
24. [Q24. (R) A junior developer refactors the audit writer to avoid `using`, but audit entries are incomplete in production. Identify all defects.](#q24-r-a-junior-developer-refactors-the-audit-writer-to-avoid-using-but-audit-entries-are-incomplete-in-production-identify-all-defects)
25. [Q25. (D) A team-wide discussion: when should a service use `AggregateException` handling for parallel order processing vs awaiting tasks individually?](#q25-d-a-team-wide-discussion-when-should-a-service-use-aggregateexception-handling-for-parallel-order-processing-vs-awaiting-tasks-individually)
26. [Q26. (D) An ASP.NET Core .NET 10 API currently returns a raw 500 HTML page for unhandled domain exceptions. Design a centralized exception-handling strategy.](#q26-d-an-aspnet-core-net-10-api-currently-returns-a-raw-500-html-page-for-unhandled-domain-exceptions-design-a-centralized-exception-handling-strategy)
27. [Q27. (P) A high-volume pricing service validates millions of SKU lookups per second. A code review reveals it uses `try/catch` to detect missing SKUs. Redesign it.](#q27-p-a-high-volume-pricing-service-validates-millions-of-sku-lookups-per-second-a-code-review-reveals-it-uses-trycatch-to-detect-missing-skus-redesign-it)

---
> Folder: `02. C# Language Fundamentals/01. C# Basics - Done/10. Exception Handling - Done`
> Source: `Program.cs` — Order/payment domain; custom exceptions, filters, rethrow, using, anti-patterns.

---

## Foundation Questions

---

## Q1. What is the execution order of try / catch / finally, and when does each block run?

**Concepts**
- try block encloses statements that may throw
- catch block runs only when a matching exception type is thrown inside try
- finally block runs after try completes or after catch runs — always
- Execution path on success: try → finally (catch is skipped)
- Execution path on handled exception: try → catch → finally
- Execution path on unhandled exception: try → finally → exception propagates
- return inside try or catch does not prevent finally from running

**Answer**

The runtime enters try and executes its statements. If no exception is thrown, the catch clauses are skipped entirely and finally runs. If an exception is thrown, the runtime walks the catch clauses in source order looking for a type match; the first matching clause executes, then finally runs. If no catch matches, finally still runs before the exception propagates to the caller. A `return` statement inside try or catch does not prevent finally — the return value is captured, finally executes, and then the value is returned. The one case where finally can be skipped is a hard process abort: `Environment.FailFast`, a fatal CLR error, or `StackOverflowException` in older runtimes where the process terminates without orderly unwinding.

```csharp
// Program.cs Section 6 — RunTryCatchFinallySuccess
try
{
    order.Validate();       // no exception thrown → catch skipped
    outcome = "valid";
}
catch (InvalidOrderException ex)
{
    outcome = "invalid: " + ex.Message;
}
finally
{
    finallyRan = true;      // always true regardless of outcome
}
```

---

## Q2. How is the .NET exception hierarchy structured, and where do custom domain exceptions fit?

**Concepts**
- `System.Exception` is the root of all exception types
- `SystemException` marks CLR and BCL runtime failures (NullReferenceException, FormatException)
- `ApplicationException` exists but is discouraged for new code
- Custom domain exceptions derive from `Exception` directly
- `is` / `as` operators respect the full inheritance chain
- BCL types like `ArgumentException`, `IOException`, `TimeoutException` sit under `SystemException`
- Domain types and `SystemException` are siblings under `Exception`

**Answer**

All exceptions derive from `System.Exception`. Beneath it the tree splits into `SystemException`, which holds CLR and BCL failures like `NullReferenceException`, `ArgumentException`, and `FormatException`, and the application-defined subtree where your own types live. `ApplicationException` was once intended as the base for application exceptions, but the .NET team deprecated that guidance — new code should derive custom exceptions directly from `Exception`. This matters at catch sites: `catch (SystemException)` matches `FormatException` but not `InvalidOrderException`; `catch (Exception)` matches everything.

```csharp
// Program.cs Section 7 — InspectExceptionHierarchy
Exception custom = new InvalidOrderException("X", "sample");
Exception format = new FormatException("bad format");

bool customIsSystem = custom is SystemException;   // false — domain type
bool formatIsSystem = format is SystemException;   // true  — BCL runtime type
bool customIsRoot   = custom is Exception;          // true  — everything is Exception
```

---

## Q3. Why must catch clauses be ordered from most specific to most general, and what happens if they are not?

**Concepts**
- Runtime evaluates catch clauses in source order — first match wins
- A base-type catch before a derived-type catch makes the derived clause unreachable
- Compiler issues CS0160 for provably unreachable catch clauses
- `when` filters on the same type can appear in any order relative to each other
- General `catch (Exception)` must always appear last if present
- Specific ordering applies within the type hierarchy, not across unrelated branches

**Answer**

The runtime tests catch clauses top-to-bottom and executes the first one whose declared type is assignable from the thrown exception. If `catch (Exception)` appears before `catch (InvalidOrderException)`, every exception including `InvalidOrderException` matches the first clause, making the second clause permanently unreachable. The C# compiler detects this statically and reports CS0160 at build time, so the error surfaces before production. The correct pattern lists the most-derived type first and places the general `catch (Exception)` safety net last.

```csharp
// Program.cs Section 8 — RunMultipleCatchInvalidOrder
catch (InvalidOrderException ex) { /* most specific — business rule */ }
catch (FormatException ex)       { /* less specific — parse failure  */ }
catch (Exception ex)             { /* general last — safety net      */ }
```

---

## Q4. What is the difference between `throw;` and `throw ex;`, and why does it matter?

**Concepts**
- `throw;` rethrows the current exception with its original stack trace intact
- `throw ex;` resets the stack trace to the current catch block
- Original throw site is lost with `throw ex;`, making root-cause diagnosis harder
- `throw;` is only legal inside a catch block
- Wrapping with inner exception preserves both old and new context
- `ExceptionDispatchInfo` enables rethrow on a different thread with preserved trace

**Answer**

Inside a catch block, `throw;` (no operand) rethrows the exact exception object that was caught, leaving its `StackTrace` pointing at the original throw site — the method where the fault actually occurred. `throw ex;` (with the exception variable) also rethrows that object but first resets its stack trace so it now points at the catch block line, hiding where the fault started. When an on-call engineer reads a log, `throw ex;` makes the catch site look like the fault origin, which sends them to the wrong place. Use `throw;` when you need to log and rethrow. Use `throw new WrapperException("context", ex)` when you want to add caller-level context while preserving the original as `InnerException`.

```csharp
// Program.cs Section 11 — LogAndRethrow
catch (InvalidOrderException)
{
    _ = ex.Message;   // simulate logging
    throw;            // preserves original stack trace — NOT throw ex;
}
```

---

## Q5. When does `finally` NOT run?

**Concepts**
- `Environment.FailFast` aborts the process without running finally blocks
- `StackOverflowException` can terminate the process before finally on .NET Framework
- A fatal CLR error (ExecutionEngineException) bypasses unwinding
- Power loss or OS kill signal skips finally
- `Thread.Abort` (removed in .NET Core) historically could prevent finally in rare cases
- Ordinary unhandled exceptions still run finally before propagation

**Answer**

In the vast majority of situations finally runs — even when exceptions are unhandled, even when a return is inside the try block. The documented cases where it does not run are: `Environment.FailFast` exits the process immediately without orderly unwinding; a `StackOverflowException` on .NET Framework can terminate the CLR before finally executes (on .NET Core the runtime usually triggers a fast-fail instead); a fatal `ExecutionEngineException` corrupts runtime state and unwinding cannot proceed; and any external kill — power off, `kill -9`, or the operating system terminating the process — obviously bypasses all managed code. For application code these are edge cases; the practical guarantee is that finally runs whenever the managed stack can be unwound. This is why the `using` statement, which compiles to `try/finally`, provides reliable cleanup for ordinary faults and all exceptions your application can handle.

---

## Q6. How do you design a custom exception class in C#, and what constructors should it expose?

**Concepts**
- Derive from `Exception` directly, not `ApplicationException`
- Provide a message-only constructor for simple validation failures
- Provide a message + inner exception constructor for wrapping lower-level errors
- Add domain properties only when callers need structured data beyond `Message`
- Parameterless constructor required if marking the type `[Serializable]`
- Exception class names should end with `Exception` by convention
- `[Serializable]` is optional for new in-process-only types

**Answer**

A custom exception inherits from `Exception` and calls the appropriate base constructor. The minimum viable implementation provides two constructors: one that takes a message string for simple validation failures and one that additionally takes an `innerException` for wrapping lower-level faults. Domain properties beyond `Message` — like `OrderId` or `RequestedAmount` — are added only when catch blocks need structured data, because they keep handler code readable: `catch (InvalidOrderException ex) { Log(ex.OrderId); }`. Do not inherit from `ApplicationException`; the .NET team discourages it and it adds no value. Mark the type `[Serializable]` and add a protected serialization constructor only if the exception will cross AppDomain boundaries or you need legacy remoting, which is uncommon in modern .NET 10 code.

```csharp
// Program.cs Section 1 — InvalidOrderException
public class InvalidOrderException : Exception
{
    public string OrderId { get; }

    public InvalidOrderException(string orderId, string message)
        : base(message) { OrderId = orderId; }

    public InvalidOrderException(string orderId, string message, Exception innerException)
        : base(message, innerException) { OrderId = orderId; }
}
```

---

## Q7. What is an inner exception, how do you create one, and how do you walk the chain?

**Concepts**
- `InnerException` property links the root-cause exception to a higher-level wrapper
- Pass the original exception as the second argument to the new exception's constructor
- `ex.InnerException` gives direct access to the first wrapped exception
- Walking the chain: loop while `ex.InnerException != null`
- `AggregateException.Flatten()` removes nested aggregate layers before walking
- `GetBaseException()` returns the innermost non-aggregate exception in the chain
- Never expose the full chain to end users — log it internally

**Answer**

When a high-level operation fails because of a low-level error, wrap the original exception by passing it as the inner argument to the new exception's constructor. This creates an `InnerException` chain that preserves the full causal story. The outer exception carries context the high-level layer understands — "Could not charge ORD-GW" — while `InnerException.Message` holds the technical cause — "Payment gateway timed out." To walk an arbitrary chain you loop while `InnerException` is not null; `GetBaseException()` jumps directly to the innermost exception. For `AggregateException`, call `Flatten()` first to collapse nested aggregates into a single level before iterating `InnerExceptions`.

```csharp
// Program.cs Section 12 — ChargeViaSimulatedGateway
catch (TimeoutException inner)
{
    throw new InvalidOrderException("ORD-GW", $"Could not charge {total:C}.", inner);
}

// reading the chain
catch (InvalidOrderException ex)
{
    Console.WriteLine(ex.Message);                   // outer context
    Console.WriteLine(ex.InnerException?.Message);   // root cause
}
```

---

## Q8. What exception properties are most valuable in production logging, and which should never be exposed to end users?

**Concepts**
- `Message` — human-readable summary, safe for logs and structured error responses
- `StackTrace` — call stack at throw site, essential for debugging, never for end users
- `InnerException` — root-cause chain, log fully, never surface to clients
- `Source` — assembly name set by the thrower or runtime
- `HResult` — HRESULT code, useful for COM interop and some I/O exceptions
- Structured logging passes the full exception object, not just `ex.Message`
- ProblemDetails (RFC 7807) carries safe fields for API clients

**Answer**

In structured logging (Serilog, Application Insights, OpenTelemetry), always pass the exception object — `logger.LogError(ex, "Failed for order {OrderId}", orderId)` — not just `ex.Message`. Passing only the message loses the type, stack trace, and inner exception chain that are captured by the logging provider as indexed fields. `StackTrace` is invaluable for diagnosing the throw site but must stay inside internal telemetry; exposing it in an API response leaks implementation details and file paths. `InnerException` similarly belongs in logs. For client-facing API responses, return an RFC 7807 `ProblemDetails` body with a safe `title`, a `detail` field from `ex.Message` only if the message is business-friendly, and a `traceId` the support team uses to correlate against internal logs.

```csharp
// Program.cs Section 13 — ReadExceptionProperties
string message     = ex.Message;               // safe for user/log summary
string stackLine   = GetStackTraceFirstLine(ex);  // debugging — log internally only
string inner       = ex.InnerException?.Message ?? "(none)";
string source      = ex.Source ?? "(not set)";
```

---

## Q9. How do exception filters (`catch when`) work, and what are the rules for when they run?

**Concepts**
- Syntax: `catch (ExceptionType ex) when (bool expression)`
- Type check happens first; filter expression runs only if type matches
- When filter evaluates to false, the clause is skipped and the next clause is tested
- Multiple clauses for the same type differ only by their when predicate
- Filter expression runs in the caller's stack frame — before stack unwinding
- Throwing from a when filter replaces the original exception (avoid)
- Useful for HTTP status codes, transient vs permanent errors, and error-code routing

**Answer**

A `when` filter adds a boolean condition to a catch clause. The runtime first checks whether the thrown exception is assignable to the catch type; only if that passes does it evaluate the `when` expression. If the expression is false, the clause is skipped and the runtime continues checking subsequent clauses — even other clauses for the same type. This allows multiple handlers for one exception type differentiated by properties rather than by separate subclasses. A critical subtlety is that filters run before the stack is unwound, while the faulting call stack is still live; this means a debugger can see local variables at the throw site even when the filter eventually matches. Never throw from inside a `when` expression — doing so discards the original exception and replaces it with the filter's exception, which is confusing and difficult to diagnose.

```csharp
// Program.cs Section 14 — RunCatchWhenOnException
catch (Exception ex) when (ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase))
{
    label = "timeout filter matched";
}
catch (InsufficientFundsException ex) when (ex.RequestedAmount > 1000m)
{
    label = "high-amount decline: " + ex.RequestedAmount;
}
catch (InsufficientFundsException ex)   // when above evaluated false — falls here
{
    label = "standard decline: " + ex.AvailableBalance;
}
```

---

## Q10. What is `AggregateException`, when does the runtime produce one, and how do you handle it correctly?

**Concepts**
- `AggregateException` wraps one or more exceptions from parallel or async operations
- Produced by `Task.Wait()`, `Task.Result`, `Task.WhenAll`, and `Parallel.ForEach`
- `InnerExceptions` property (plural) holds the full collection
- `Flatten()` collapses nested `AggregateException` layers into one
- `Handle(Func<Exception, bool>)` processes each inner exception; unhandled ones are re-thrown
- `await` unwraps the first inner exception by default — others may be lost
- Direct `await` on individual tasks preserves specific exception types

**Answer**

`AggregateException` is the container the Task Parallel Library and `Task.WhenAll` use to report multiple concurrent failures. When `Task.Wait()` or `Task.Result` is called on a faulted task, the runtime wraps the task's exception(s) in an `AggregateException`. When multiple tasks fault simultaneously — as in `await Task.WhenAll(t1, t2, t3)` — the aggregate may contain many inner exceptions. If you `await` the `WhenAll` call directly, C# unwraps only the first inner exception, so the others are silently discarded. To inspect all failures, catch `AggregateException` explicitly or capture the task and check `.Exception` after it faults. Call `Flatten()` first to collapse any nested aggregates, then iterate `InnerExceptions` or use `Handle()` to process each one.

```csharp
// .NET 10 parallel processing example
var tasks = orders.Select(o => ProcessOrderAsync(o)).ToArray();
try
{
    await Task.WhenAll(tasks);
}
catch (AggregateException aex)
{
    foreach (var inner in aex.Flatten().InnerExceptions)
        logger.LogError(inner, "Order processing failed");
}
// Or: inspect each task directly
foreach (var t in tasks.Where(t => t.IsFaulted))
    logger.LogError(t.Exception!.InnerException, "Task faulted");
```

---

## Q11. What is `ExceptionDispatchInfo` and what problem does it solve compared to plain rethrow?

**Concepts**
- `ExceptionDispatchInfo.Capture(ex)` captures an exception with its current stack trace
- `.Throw()` rethrows the captured exception from a different location with original trace preserved
- Appends a "--- End of stack trace from previous location ---" marker at the rethrow site
- Solves the problem of rethrowing on a different thread where `throw;` is not valid
- Used internally by `await` to restore exception context from a thread pool thread
- Useful in interceptor pipelines that capture exceptions and replay them later
- Available since .NET 4.5; no additional packages needed

**Answer**

`throw;` can only appear directly inside a catch block. If you capture an exception from a background thread, store it in a field, and want to rethrow it on the calling thread later, `throw;` is not available and `throw ex;` would reset the stack trace. `ExceptionDispatchInfo` solves this: `ExceptionDispatchInfo.Capture(ex)` saves the exception and its current stack trace at the moment of capture, and `.Throw()` rethrows it wherever called, appending a separator marker so the stack trace shows both the original throw site and the rethrow site. This is the mechanism the C# compiler and runtime use under the covers when you `await` a task — the exception thrown on a thread pool thread is captured and rethrown on the awaiter's continuation with its full original context intact.

```csharp
// .NET 10 — capturing on a background thread, rethrowing on the caller
ExceptionDispatchInfo? captured = null;

await Task.Run(() =>
{
    try { RiskyOperation(); }
    catch (Exception ex) { captured = ExceptionDispatchInfo.Capture(ex); }
});

captured?.Throw();  // rethrows with original stack trace preserved
// stack trace shows both the background thread site and this rethrow point
```

---

## Q12. When should you use `ArgumentException` and its subtypes instead of a custom domain exception?

**Concepts**
- `ArgumentException` signals invalid method parameters — precondition violations
- `ArgumentNullException` for null parameters where null is not accepted
- `ArgumentOutOfRangeException` for values outside an acceptable range
- Custom domain exceptions signal business-rule violations on entities
- Precondition exceptions are for programmer errors; domain exceptions are for business outcomes
- `throw new ArgumentNullException(nameof(param))` is the idiomatic form
- Parameter guard clauses should appear before any business logic

**Answer**

Use `ArgumentNullException`, `ArgumentException`, and `ArgumentOutOfRangeException` when a method receives invalid input that indicates a programming mistake by the caller — a null reference where the API contract forbids null, a page-size of -1, or an empty collection where at least one item is required. These are precondition failures: the caller broke a contract, not a business rule. Use custom domain exceptions — like `InvalidOrderException` from `Program.cs Section 1` — when a business-rule invariant is violated on a domain entity, a user-visible action fails, or structured error properties (`OrderId`, `RequestedAmount`) are needed for logging and UI mapping. The practical test is audience: precondition exceptions say "you, the developer, called this wrong"; domain exceptions say "this business state is invalid, here is what a human or policy needs to know."

```csharp
// .NET 10 guard clause pattern
public void ProcessOrder(Order order, ILogger logger)
{
    ArgumentNullException.ThrowIfNull(order);           // precondition — programmer error
    ArgumentNullException.ThrowIfNull(logger);

    order.Validate();  // may throw InvalidOrderException — business rule
}
```

---

## Q13. How does the `using` statement provide exception-safe resource cleanup, and what does the compiler emit?

**Concepts**
- `using` compiles to a `try/finally` block that calls `Dispose()` on exit
- Dispose runs even when an exception escapes the using block
- C# 8 `using` declaration (`using var x = ...`) scopes to the enclosing block
- IDisposable types include streams, database connections, HttpClient, and timers
- `await using` handles `IAsyncDisposable` for async cleanup
- Manual `Dispose()` after a throw-capable line does not run without finally
- ObjectDisposedException is thrown when a disposed object is used after disposal

**Answer**

The `using` statement is syntactic sugar for a `try/finally` that calls `Dispose()` unconditionally on the declared variable. If any statement inside the using block throws, the finally clause still invokes Dispose, releasing file handles, connection pool slots, and any other unmanaged resources. Manual cleanup — calling `Dispose()` at the end of a happy-path method — fails silently the moment an exception is thrown before that line. The C# 8 declaration form `using AuditLogWriter audit = new();` disposes at the end of the enclosing scope rather than a nested block, which is cleaner for methods with multiple disposables. For async resources implementing `IAsyncDisposable`, use `await using` so the async cleanup path runs without blocking a thread pool thread.

```csharp
// Program.cs Section 15 — RunUsingDisposalPreview
// This:
using (AuditLogWriter audit = new AuditLogWriter())
{
    audit.WriteEntry($"Payment attempt for {orderId}");
    return audit.LastEntry;
} // Dispose always runs — even if WriteEntry throws

// Compiles to approximately:
AuditLogWriter audit = new AuditLogWriter();
try { audit.WriteEntry(...); return audit.LastEntry; }
finally { audit.Dispose(); }
```

---

## Q14. What are the key anti-patterns in exception handling, and why is each one harmful?

**Concepts**
- Empty catch block — swallows exceptions silently, hiding bugs
- `throw ex;` — resets stack trace, losing original fault location
- Catch-all before specific types — CS0160 unreachable catch at compile time
- Using exceptions as control flow — slow, clutters logs, unclear API
- Catching exceptions without logging or recovery — no operational visibility
- Exposing `StackTrace` in user-facing responses — security and UX risk
- Re-throwing without inner exception — discards diagnostic context

**Answer**

Empty catch blocks are the most damaging pattern: `catch (Exception) { }` swallows every failure silently, so the caller believes the operation succeeded while the underlying state is corrupt or incomplete. The next most common mistake is `throw ex;` in a rethrow path, which resets the stack trace and sends engineers to the wrong file and line during an incident. Using exceptions for expected, frequent outcomes — throwing `NotFoundException` for a normal "product not in catalog" lookup — imposes the cost of exception object allocation and stack-walk on the hot path and fills structured logs with stack traces that represent normal behaviour, burying the real errors. Exposing `StackTrace` or `InnerException` chains in API responses leaks class names, file paths, and connection strings to callers. The safe default is: catch only what you can handle, always log the full exception object, use result types or Try* patterns for expected outcomes, and surface only safe `ProblemDetails` to clients.

```csharp
// Program.cs Section 16 — anti-pattern demos
// BAD — swallowing
catch (InvalidOperationException) { /* silent */ }

// GOOD — return for expected validation result, not throw
private static string ValidateWithResult(Order order)
{
    if (order.Quantity <= 0)
        return $"Order {order.OrderId}: quantity must be positive (no exception thrown).";
    return $"Order {order.OrderId}: OK";
}
```

---

## Q15. What is the difference between `Exception.Message` and logging the exception object itself?

**Concepts**
- `ex.Message` is a string — it carries only the top-level message text
- Passing the exception object to a logger captures type, stack trace, inner chain, and properties
- Structured logging providers (Serilog, Application Insights) index exception fields separately
- `ex.Message` alone makes root-cause queries impossible in a log aggregator
- Exception properties like `HResult`, `Source`, and custom domain fields are lost with message-only logging
- OpenTelemetry records exception type and stack trace as span attributes when passed as exception object
- The pattern `logger.LogError(ex, "message template {Param}", value)` is the correct form

**Answer**

`ex.Message` is a plain string. When you write `logger.LogError(ex.Message)` you lose the exception type name, the stack trace, every `InnerException` in the chain, and any custom properties like `OrderId` or `RequestedAmount`. Log aggregators like Kibana, Datadog, and Application Insights receive only an unstructured string; they cannot filter by exception type or correlate stack frames across services. Passing the exception object — `logger.LogError(ex, "ValidateAndCharge failed for order {OrderId}", order.OrderId)` — lets the provider serialize the full exception graph into structured fields that are indexed and searchable. The message template provides business context while the exception object provides all the diagnostic data. This distinction is exactly what the code review in `KARAT_INTERVIEW_ANSWERS.md Q1` tests.

---

## Q16. What does `ObjectDisposedException` indicate, and how should `IDisposable` types guard against use-after-dispose?

**Concepts**
- `ObjectDisposedException` signals that a method was called on an already-disposed object
- Guard with a `bool _disposed` flag checked at the start of every public method
- `Dispose()` sets `_disposed = true` and performs cleanup only once (idempotent)
- `GC.SuppressFinalize(this)` prevents the finalizer from running after explicit disposal
- `IAsyncDisposable.DisposeAsync()` is the async counterpart for async resources
- Dispose pattern: public `Dispose()` calls protected virtual `Dispose(bool disposing)`
- ObjectDisposedException constructor accepts the object name via `nameof`

**Answer**

When a caller holds a reference to a disposable object and calls methods on it after `Dispose()` has run, the object should throw `ObjectDisposedException` rather than silently operating on a half-closed resource or crashing with a null reference. The standard guard is a private `bool _disposed` field checked at the beginning of every public method. The `Dispose()` method must be idempotent — calling it twice should not throw. The full dispose pattern adds a protected virtual `Dispose(bool disposing)` so subclasses can release both managed and unmanaged resources in the right order. On .NET 10, if your type holds async resources like async streams, implement `IAsyncDisposable` as well and provide `DisposeAsync()`.

```csharp
// Program.cs Section 4 — AuditLogWriter
public void WriteEntry(string entry)
{
    if (_disposed)
        throw new ObjectDisposedException(nameof(AuditLogWriter));
    LastEntry = entry;
}

public void Dispose()
{
    if (!_disposed)
    {
        LastEntry += " [closed]";
        _disposed = true;
        GC.SuppressFinalize(this);  // suppress finalizer — resource already released
    }
}
```

---

## Gotchas

---

## Q17. A `catch when` filter is evaluated but the type matches — can the exception still escape?

**Concepts**
- `when` is evaluated after type matching, before entering the handler body
- If `when` expression is false, the clause is skipped — exception continues unwinding
- No catch block further up the call stack may see the exception until unwinding completes
- Filter evaluation happens before stack unwinding — faulting stack frames are still live
- Subsequent catch clauses for the same or base types are still tested after a false filter
- An unhandled exception after all filters are false propagates to the caller as if no catch existed

**Answer**

Yes. A `when` filter whose expression evaluates to false causes the entire catch clause to be skipped, even though the exception type matched. The runtime proceeds to the next clause in source order. If no remaining clause matches — or all remaining filters also evaluate to false — the exception propagates to the caller exactly as if no catch existed at all. The observable gotcha is that you can write a catch clause for a specific type, see the type match in a debugger, and still have the exception escape because the `when` condition was false. This is intentional: filters are designed to let exceptions pass through to outer handlers when the current catch cannot actually handle them. A related subtlety is that because filter evaluation precedes unwinding, a debugger set to break on first-chance exceptions sees the full faulting stack, which is one of the key benefits of `catch when` over catching and re-throwing.

---

## Q18. If an exception is thrown inside a `finally` block, what happens to the original exception?

**Concepts**
- A new exception thrown from `finally` replaces the original exception in the propagation chain
- The original exception is lost — not chained as `InnerException`
- The new exception propagates to the caller instead
- Catch blocks in the enclosing scope see only the new exception
- Using `throw;` inside finally is valid but rare and still replaces the pending exception
- Best practice: do not throw from finally; catch and log inside finally instead
- `ExceptionDispatchInfo` cannot save you here — the original is simply discarded

**Answer**

If `finally` throws a new exception, the CLR discards the original exception that was being propagated and replaces it with the new one. There is no automatic chaining — the original is gone. This is one of the most silent data-loss bugs in C#. The caller catches or sees only the new finally-exception, with no trace of what triggered the finally block in the first place. The fix is to wrap the finally body's risky work in its own try/catch and log or swallow inside it rather than letting a secondary exception escape. A `Dispose()` call in a finally block is the most common trigger: if the underlying stream's `Close()` throws, the original business exception that caused the early return is lost.

```csharp
// DANGEROUS — finally throws, original exception disappears
try
{
    ProcessPayment(balance, amount);
}
finally
{
    auditWriter.Flush();  // if Flush() throws, ProcessPayment exception is gone
}

// SAFE — secondary exceptions handled inside finally
finally
{
    try { auditWriter.Flush(); }
    catch (Exception flushEx) { logger.LogWarning(flushEx, "Flush failed on cleanup path"); }
}
```

---

## Q19. When you `await Task.WhenAll(...)`, which exceptions do you see in the catch block?

**Concepts**
- `await Task.WhenAll` throws only the first inner exception by default
- The remaining exceptions in the `AggregateException` are silently discarded
- To see all failures, capture the `Task` and check `.Exception.Flatten()` after it faults
- `Task.Wait()` and `Task.Result` throw an `AggregateException` containing all failures
- `await` unwraps `AggregateException` to its first inner exception for ergonomics
- You can re-enable aggregate visibility by catching `Exception` and inspecting the task directly
- `Task.WhenAll` marks itself faulted after all tasks complete, even if some succeed

**Answer**

`await Task.WhenAll(t1, t2, t3)` waits for all tasks to complete and then, if any faulted, throws an exception. However, `await` unwraps the `AggregateException` that `WhenAll` stores internally and throws only the first inner exception, discarding the rest. If all three tasks threw different exceptions, the catch block sees only the first one; the other two are invisible. To observe every failure, capture the `Task` reference before awaiting, catch `Exception` after `await`, and then inspect `whenAllTask.Exception?.Flatten().InnerExceptions`. Alternatively, avoid `await Task.WhenAll` entirely when you need all errors: await tasks individually in a loop or use `Task.WhenEach` (added in .NET 9) which yields tasks as they complete.

---

## Q20. What happens if you rethrow an exception using `throw;` when you captured it with `ExceptionDispatchInfo.Throw()`?

**Concepts**
- `ExceptionDispatchInfo.Capture(ex)` takes a snapshot of the exception and its current stack trace
- `.Throw()` rethrows the exception and appends the rethrow site with a separator marker
- The stack trace shows the original throw site, the separator, and the `.Throw()` call site
- If `.Throw()` is called inside a catch block, a subsequent `throw;` would rethrow the same object again
- `throw;` after `ExceptionDispatchInfo.Throw()` adds yet another rethrow marker to the trace
- The exception object's stack trace field is mutated by each rethrow call
- In practice, call `.Throw()` only once per captured instance

**Answer**

`ExceptionDispatchInfo.Throw()` rethrows the captured exception and appends the current call site to the stack trace with a "--- End of stack trace from previous location ---" separator. If you then catch that exception and use `throw;`, the runtime appends another rethrow marker, so the stack trace grows by another entry. Each `.Throw()` or `throw;` call mutates the exception object's stack trace text in place. In practice this is rarely a problem because you call `.Throw()` once to replay the exception on the calling thread and let it propagate naturally from there. The gotcha is catching the result of `.Throw()` and rethrowing multiple times in a retry loop — you end up with a progressively longer stack trace that can obscure the original throw site among layers of retry markers.

---

## Q21. Can a `catch (Exception)` block catch a `StackOverflowException` or `OutOfMemoryException`?

**Concepts**
- `StackOverflowException` cannot be caught in .NET Core — the CLR terminates the process
- `OutOfMemoryException` can be caught but the runtime state is unreliable after it
- Both are `SystemException` descendants in the type hierarchy
- Catching and swallowing `OutOfMemoryException` can mask resource leaks
- `ThreadAbortException` (removed in .NET Core) was also special in .NET Framework
- Best practice: do not catch `OutOfMemoryException` unless you have a deliberate degradation path
- A `StackOverflowException` in .NET Core triggers the runtime's fast-fail — finally blocks may not run

**Answer**

In .NET Core (and therefore .NET 10) a `StackOverflowException` causes the CLR to perform a hard fast-fail — the process terminates, finally blocks do not run, and no managed catch block can intercept it. This is a deliberate design to prevent a corrupted stack from continuing execution. `OutOfMemoryException` is technically catchable via `catch (Exception)` or `catch (OutOfMemoryException)`, and in specific scenarios — such as a retry that can release a large buffer and try a smaller allocation — catching it is defensible. However, the managed heap may be in an inconsistent state after OOM, so most catch actions risk further failures. The general guidance for production .NET 10 code is to let both exceptions terminate the process or app domain and rely on process-level restart policies (Kubernetes, Windows Service recovery) rather than attempting in-process recovery.

---

## Real-World Scenarios

---

## Q22. (R) A teammate's order service method logs exceptions before rethrowing. Review this code and identify all defects.

```csharp
public async Task<string> ValidateAndChargeAsync(Order order)
{
    try
    {
        order.Validate();
        await _gateway.ChargeAsync(order.Total);
        return "charged";
    }
    catch (Exception ex)
    {
        _logger.LogError(ex.Message);
        throw ex;
    }
}
```

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Observability | `LogError(ex.Message)` — string argument only, no exception parameter | Logger receives a plain string; stack trace, exception type, `InnerException`, and custom properties (`OrderId`) are never indexed in the telemetry store |
| Diagnostics | `throw ex;` resets the stack trace | On-call engineer sees `ValidateAndChargeAsync` as the fault origin instead of `Order.Validate()` or the gateway client; root cause takes far longer to identify |
| Design | `catch (Exception)` without type discrimination | `InvalidOrderException` (business decline) and `HttpRequestException` (infrastructure fault) are logged and rethrown identically; alert routing, retry logic, and SLO dashboards cannot distinguish them |
| Async contract | `throw ex;` on a faulted `Task` path | `await` at the call site unwraps `AggregateException` — using `throw;` preserves the original exception type and trace for the awaiter to see correctly |

**Fix priority**

1. Replace `_logger.LogError(ex.Message)` with `_logger.LogError(ex, "ValidateAndCharge failed for order {OrderId}", order.OrderId)` — pass the exception object as first argument so the provider serializes the full exception graph.
2. Replace `throw ex;` with `throw;` to preserve the original stack trace pointing at the actual fault site.
3. Separate catch clauses for `InvalidOrderException` (log at Warning, return a 400 result) and infrastructure exceptions (log at Error, rethrow for the global handler) — do not treat business declines as server errors.

```csharp
// .NET 10 — corrected form
catch (InvalidOrderException ex)
{
    _logger.LogWarning(ex, "Order validation failed {OrderId}", order.OrderId);
    throw;  // let global handler map to 400
}
catch (Exception ex)
{
    _logger.LogError(ex, "Gateway charge failed for order {OrderId}", order.OrderId);
    throw;
}
```

---

## Q23. (R) A nightly reconciliation job wraps each order in try/catch but support reports "job succeeded" while ledger rows are missing after gateway timeouts. Find all defects.

```csharp
public int ReconcileOrders(IEnumerable<Order> orders)
{
    int processed = 0;
    foreach (var order in orders)
    {
        try
        {
            _gateway.Charge(order.Total);
            _ledger.Record(order.OrderId, order.Total);
            processed++;
        }
        catch (Exception)
        {
        }
    }
    return processed;
}
```

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Empty `catch (Exception)` swallows every failure | `TimeoutException` from the gateway is silently discarded; `processed` does not increment but the method returns a non-zero value and exits normally — callers and monitors believe work was done |
| Operability | No logging of order id, exception type, or inner cause | Support cannot identify which orders failed, when, or why — no actionable signal for retries or manual remediation |
| Atomicity | `_ledger.Record` is inside the same try/catch as `_gateway.Charge` | A charge succeeds but record fails — the same empty catch hides a ledger gap with a charged order |
| Return value semantics | `processed` counts only fully successful pairs | Callers cannot distinguish "10 orders processed" from "0 processed, 10 swallowed" |

**Fix priority**

1. Never use an empty catch — at minimum log `_logger.LogError(ex, "Reconcile failed for {OrderId}", order.OrderId)` and add the order id to a failure list.
2. Decide batch policy explicitly: fail-fast (throw after first failure), partial-success (return both processed and failed counts), or dead-letter (write failed ids to a queue for retry).
3. Separate `_ledger.Record` into its own try/catch with distinct error handling from the gateway call — charge failure vs ledger failure require different recovery actions.
4. For transient gateway timeouts, apply a retry with exponential back-off (Polly) rather than a silent skip.

```csharp
// .NET 10 — partial-success pattern
var failed = new List<string>();
catch (Exception ex)
{
    _logger.LogError(ex, "Reconcile failed for order {OrderId}", order.OrderId);
    failed.Add(order.OrderId);
}
return (processed, failed);
```

---

## Q24. (R) A junior developer refactors the audit writer to avoid `using`, but audit entries are incomplete in production. Identify all defects.

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

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Resource leak | `Dispose()` is only on the happy path; any exception from `WriteEntry` skips it | File streams, socket handles, and native resources held by the writer leak until the finalizer runs (non-deterministic) |
| Correctness | `[closed]` marker is never appended on the exception path | Downstream audit consumers that check for the marker treat in-flight entries as valid, producing inconsistent audit records |
| Maintainability | Manual dispose is fragile under refactoring | Adding a second `return` or a new code path before `Dispose()` silently re-introduces the leak — `using` makes this structurally impossible |

**Fix priority**

1. Restore `using (AuditLogWriter audit = new AuditLogWriter()) { ... }` — the compiler emits a `try/finally` that calls `Dispose()` even when `WriteEntry` throws.
2. For C# 8+ style, use the declaration form `using AuditLogWriter audit = new();` which disposes at the end of the enclosing method scope.
3. If construction itself can fail, wrap the instantiation in a null-checked `try/finally` as a fallback.

```csharp
// .NET 10 — declaration form
public static string WriteAuditTrail(string orderId)
{
    using AuditLogWriter audit = new();
    audit.WriteEntry($"Payment attempt for {orderId}");
    return audit.LastEntry;
}  // Dispose always runs here — even if WriteEntry threw
```

---

## Q25. (D) A team-wide discussion: when should a service use `AggregateException` handling for parallel order processing vs awaiting tasks individually?

**Concepts**
- `Task.WhenAll` stores all failures in `AggregateException.InnerExceptions`
- `await Task.WhenAll` unwraps only the first inner exception
- Awaiting tasks individually in a loop surfaces each failure separately
- Batch jobs tolerate partial success; transactional APIs do not
- `Task.WhenEach` (.NET 9+) yields tasks as they complete for streaming processing
- Retry and circuit-breaker policies apply per-task in individual-await patterns
- Logging each failure requires iterating `InnerExceptions` after `WhenAll`

**Answer**

Use `await Task.WhenAll` when you need all tasks to complete before proceeding and can accept that only the first exception is surfaced by the awaiter directly. For batch reconciliation jobs where partial success is acceptable and you need to log every failure, capture the `Task` reference and inspect `.Exception.Flatten().InnerExceptions` after the await, or switch to `Task.WhenEach` (introduced in .NET 9) to process each result as it arrives. Use individual task awaiting inside a loop when per-item retry policies, per-item error handling, or per-item circuit-breaker state matter — Polly integrates cleanly with individual awaits. Avoid `Task.WaitAll` (blocking) in async code because it blocks a thread pool thread and can cause deadlocks in ASP.NET Core. The guiding question is whether failures are independent (batch — handle each, accumulate results) or correlated (transactional — one failure means the whole operation is invalid, so fail fast on the first exception).

```csharp
// .NET 10 — observe all WhenAll failures
var tasks = orders.Select(o => ChargeAsync(o)).ToArray();
Task whenAllTask = Task.WhenAll(tasks);
try { await whenAllTask; }
catch
{
    foreach (var ex in whenAllTask.Exception!.Flatten().InnerExceptions)
        logger.LogError(ex, "Order charge failed");
    throw;
}
```

---

## Q26. (D) An ASP.NET Core .NET 10 API currently returns a raw 500 HTML page for unhandled domain exceptions. Design a centralized exception-handling strategy.

**Concepts**
- `IExceptionHandler` interface — .NET 8+ recommended approach
- `AddExceptionHandler<T>()` and `UseExceptionHandler()` in `Program.cs`
- RFC 7807 `ProblemDetails` for structured error responses
- Domain exception → HTTP status code mapping (400, 402, 404, 500)
- Development vs Production response content rules
- `ILogger.LogError(ex, ...)` with `TraceIdentifier` correlation
- `IProblemDetailsService` for consistent JSON serialization

**Answer**

Register a global exception handler once in `Program.cs` so every controller and minimal-API endpoint can throw domain exceptions without wrapping them individually. In .NET 8 and .NET 10, implement `IExceptionHandler` in a class that maps known types to HTTP status codes and `ProblemDetails` bodies, then register it with `builder.Services.AddExceptionHandler<GlobalExceptionHandler>()` and `app.UseExceptionHandler()`. The handler maps `InvalidOrderException` to 400, `InsufficientFundsException` to 402, and all other exceptions to 500. In Development, populate the `ProblemDetails.Detail` with the exception message and optionally the stack trace for local debugging. In Production, use only a safe, user-friendly message in `Detail` and include the `TraceIdentifier` so support can correlate the response with the internal log entry where the full exception was logged. Never include stack trace, connection strings, or inner exception messages in production responses.

```csharp
// .NET 10 — IExceptionHandler registration
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
app.UseExceptionHandler();

// GlobalExceptionHandler.TryHandleAsync
public async ValueTask<bool> TryHandleAsync(
    HttpContext context, Exception ex, CancellationToken ct)
{
    var (status, title) = ex switch
    {
        InvalidOrderException     => (400, "Invalid order"),
        InsufficientFundsException => (402, "Payment declined"),
        _                         => (500, "An error occurred")
    };
    logger.LogError(ex, "Unhandled exception {TraceId}", context.TraceIdentifier);
    await context.Response.WriteAsJsonAsync(new ProblemDetails
    {
        Status = status, Title = title,
        Extensions = { ["traceId"] = context.TraceIdentifier }
    }, ct);
    return true;
}
```

---

## Q27. (P) A high-volume pricing service validates millions of SKU lookups per second. A code review reveals it uses `try/catch` to detect missing SKUs. Redesign it.

**Concepts**
- Exception throw and catch has significant per-invocation cost on the hot path
- `Dictionary.TryGetValue` returns bool, avoids KeyNotFoundException entirely
- `TryParse` / `TryGet` patterns avoid exception overhead for expected-negative cases
- `Result<T>` or `Option<T>` types communicate absence without exceptions
- Exceptions are appropriate for programming errors and unexpected runtime faults
- Benchmarking: exception-path code can be 100–1000× slower than a boolean check
- API surface should communicate expected vs exceptional outcomes through return types

**Answer**

In a service processing millions of lookups per second, using exceptions to signal "SKU not found" causes measurable latency and CPU overhead. The CLR must allocate the exception object, capture the stack trace, and unwind through handler registration — all of which are multiple orders of magnitude slower than a boolean branch. Replace the exception-throwing lookup with `Dictionary.TryGetValue` or a `TryGetPricing(string sku, out PricingRecord? result)` method that returns `false` for a cache miss. At the API boundary, map `false` to a 404 response — not an unhandled exception. Reserve `InvalidOperationException` or a domain exception for genuinely unexpected states: a pricing record that exists but has a negative price, or a null configuration that should never be null. The observable rule is: if the negative outcome happens in normal production operation at meaningful frequency, use a return type; if it should never happen in correct operation, throw.

```csharp
// .NET 10 — TryGet pattern for hot path
public bool TryGetPricing(string sku, out PricingRecord? record)
{
    return _pricingCache.TryGetValue(sku, out record);
}

// at the endpoint
if (!_service.TryGetPricing(sku, out var record))
    return Results.NotFound(new ProblemDetails { Title = "SKU not found", Status = 404 });
return Results.Ok(record);
```
