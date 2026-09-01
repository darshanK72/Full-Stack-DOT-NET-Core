# 04. Functional Style Programming â€" Interview Q&A
> Back to [README](../README.md)

## Table of Contents

- [01. Delegates](#01-delegates)
  - [Q1. What is Functional Programming, and how does C# support it without being a purely functional language?](#q1-what-is-functional-programming-and-how-does-c-support-it-without-being-a-purely-functional-language)
  - [Q2. What are the key principles of Functional Programming (immutability, pure functions, first-class functions, higher-order functions, referential transparency)?](#q2-what-are-the-key-principles-of-functional-programming-immutability-pure-functions-first-class-functions-higher-order-functions-referential-transparency)
  - [Q3. What is the difference between imperative and declarative programming styles? Give a C# example of each.](#q3-what-is-the-difference-between-imperative-and-declarative-programming-styles-give-a-c-example-of-each)
  - [Q4. What does it mean for functions to be first-class citizens in C#?](#q4-what-does-it-mean-for-functions-to-be-first-class-citizens-in-c)
  - [Q5. What is a delegate in C#? How does it differ from a method group and from an interface with a single method?](#q5-what-is-a-delegate-in-c-how-does-it-differ-from-a-method-group-and-from-an-interface-with-a-single-method)
  - [Q6. How do you declare, instantiate, and invoke a custom delegate type?](#q6-how-do-you-declare-instantiate-and-invoke-a-custom-delegate-type)
  - [Q7. What is a multicast delegate? How does `+=` and `-=` work on delegate instances?](#q7-what-is-a-multicast-delegate-how-does-and---work-on-delegate-instances)
  - [Q8. What is the difference between single-cast and multicast delegates at invocation time?](#q8-what-is-the-difference-between-single-cast-and-multicast-delegates-at-invocation-time)
  - [Q9. What happens when you invoke a multicast delegate and one subscriber throws an exception?](#q9-what-happens-when-you-invoke-a-multicast-delegate-and-one-subscriber-throws-an-exception)
  - [Q10. What is delegate covariance and contravariance in C#?](#q10-what-is-delegate-covariance-and-contravariance-in-c)
  - [Q11. When would you prefer a named delegate type over `Func`/`Action` in a public API?](#q11-when-would-you-prefer-a-named-delegate-type-over-funcaction-in-a-public-api)
  - [Q12. What are the advantages and limitations of adopting a functional style in typical enterprise C# codebases?](#q12-what-are-the-advantages-and-limitations-of-adopting-a-functional-style-in-typical-enterprise-c-codebases)

- [02. Lambda Expressions](#02-lambda-expressions)
  - [Q1. What is a lambda expression in C#? What problem does it solve compared to named methods?](#q1-what-is-a-lambda-expression-in-c-what-problem-does-it-solve-compared-to-named-methods)
  - [Q2. What is the difference between an expression lambda and a statement lambda?](#q2-what-is-the-difference-between-an-expression-lambda-and-a-statement-lambda)
  - [Q3. When can parameter types be omitted in a lambda, and when must they be explicit?](#q3-when-can-parameter-types-be-omitted-in-a-lambda-and-when-must-they-be-explicit)
  - [Q4. What are target-typed lambdas (C# 10+)? In what contexts does the compiler infer the delegate type?](#q4-what-are-target-typed-lambdas-c-10-in-what-contexts-does-the-compiler-infer-the-delegate-type)
  - [Q5. What is the natural type of a lambda â€" when does the compiler infer `Func`/`Action` vs require an explicit target type?](#q5-what-is-the-natural-type-of-a-lambda-when-does-the-compiler-infer-funcaction-vs-require-an-explicit-target-type)
  - [Q6. How do lambda expressions differ from anonymous methods in syntax, capabilities, and compiler output?](#q6-how-do-lambda-expressions-differ-from-anonymous-methods-in-syntax-capabilities-and-compiler-output)
  - [Q7. Can a lambda expression access `ref`, `out`, or `in` parameters from the enclosing method?](#q7-can-a-lambda-expression-access-ref-out-or-in-parameters-from-the-enclosing-method)
  - [Q8. Can a lambda be converted to an expression tree? What syntax or API constraints apply?](#q8-can-a-lambda-be-converted-to-an-expression-tree-what-syntax-or-api-constraints-apply)
  - [Q9. What is the difference between a lambda that captures no locals vs one that captures outer variables?](#q9-what-is-the-difference-between-a-lambda-that-captures-no-locals-vs-one-that-captures-outer-variables)
  - [Q10. How do async lambdas work (`async x => ...`)? What delegate types can they target?](#q10-how-do-async-lambdas-work-async-x-what-delegate-types-can-they-target)
  - [Q11. What happens if you use a lambda where a `Expression<TDelegate>` is expected vs where a `TDelegate` is expected?](#q11-what-happens-if-you-use-a-lambda-where-a-expressiontdelegate-is-expected-vs-where-a-tdelegate-is-expected)

- [03. Anonymous Methods](#03-anonymous-methods)
  - [Q1. What are anonymous methods in C#? Why were they introduced, and what largely replaced them?](#q1-what-are-anonymous-methods-in-c-why-were-they-introduced-and-what-largely-replaced-them)
  - [Q2. What is the syntax for an anonymous method, and how does it compare to lambda syntax?](#q2-what-is-the-syntax-for-an-anonymous-method-and-how-does-it-compare-to-lambda-syntax)
  - [Q3. Can anonymous methods omit parameter lists? When is that useful?](#q3-can-anonymous-methods-omit-parameter-lists-when-is-that-useful)
  - [Q4. What outer scope variables can anonymous methods access, and how does capture work?](#q4-what-outer-scope-variables-can-anonymous-methods-access-and-how-does-capture-work)
  - [Q5. In modern C# code, when (if ever) would you still choose an anonymous method over a lambda?](#q5-in-modern-c-code-when-if-ever-would-you-still-choose-an-anonymous-method-over-a-lambda)
  - [Q6. A teammate argues that anonymous methods in `RunAllRules` should stay inline because "they are only five lines," but QA cannot unit-test individual rules without running the whole pipeline.](#q6-a-teammate-argues-that-anonymous-methods-in-runallrules-should-stay-inline-because-they-are-only-five-lines-but-qa-cannot-unit-test-individual-rules-without-running-the-whole-pipeline)

- [04. Extension Methods](#04-extension-methods)
  - [Q1. What are extension methods in C#? How do they appear to the caller vs how they are implemented?](#q1-what-are-extension-methods-in-c-how-do-they-appear-to-the-caller-vs-how-they-are-implemented)
  - [Q2. What are the language rules for declaring an extension method (static class, `this` parameter, accessibility)?](#q2-what-are-the-language-rules-for-declaring-an-extension-method-static-class-this-parameter-accessibility)
  - [Q3. How does the compiler resolve an extension method call at compile time?](#q3-how-does-the-compiler-resolve-an-extension-method-call-at-compile-time)
  - [Q4. What is the difference in resolution order between an instance method and an extension method with the same signature?](#q4-what-is-the-difference-in-resolution-order-between-an-instance-method-and-an-extension-method-with-the-same-signature)
  - [Q5. Can extension methods access `private` members of the extended type? Why or why not?](#q5-can-extension-methods-access-private-members-of-the-extended-type-why-or-why-not)
  - [Q6. What are the limitations of extension methods?](#q6-what-are-the-limitations-of-extension-methods)
  - [Q7. How do extension methods work on interfaces? What are design implications (e.g., LINQ)?](#q7-how-do-extension-methods-work-on-interfaces-what-are-design-implications-eg-linq)
  - [Q8. What happens when two namespaces define extensions with the same name and signature for the same type?](#q8-what-happens-when-two-namespaces-define-extensions-with-the-same-name-and-signature-for-the-same-type)
  - [Q9. Can you define generic extension methods? How does type inference work at the call site?](#q9-can-you-define-generic-extension-methods-how-does-type-inference-work-at-the-call-site)
  - [Q10. What are anti-patterns with extension methods (god extensions, violating encapsulation)?](#q10-what-are-anti-patterns-with-extension-methods-god-extensions-violating-encapsulation)

- [05. Func, Action & Predicate](#05-func-action-predicate)
  - [Q1. What are `Func<T>`, `Func<T, TResult>`, and the general `Func<...>` family?](#q1-what-are-funct-funct-tresult-and-the-general-func-family)
  - [Q2. What is `Action` vs `Action<T>` vs `Action<T1, T2, ...>`?](#q2-what-is-action-vs-actiont-vs-actiont1-t2)
  - [Q3. What is `Predicate<T>`, and how does it relate to `Func<T, bool>`?](#q3-what-is-predicatet-and-how-does-it-relate-to-funct-bool)
  - [Q4. When should you use `Func` vs `Action` vs `Predicate` vs a custom delegate?](#q4-when-should-you-use-func-vs-action-vs-predicate-vs-a-custom-delegate)
  - [Q5. What are higher-order functions? Give C# examples using `Func` and `Action`.](#q5-what-are-higher-order-functions-give-c-examples-using-func-and-action)
  - [Q6. What is function composition, and how can it be achieved in C#?](#q6-what-is-function-composition-and-how-can-it-be-achieved-in-c)
  - [Q7. How many generic parameters do `Func` and `Action` support, and which parameter is always the return type for `Func`?](#q7-how-many-generic-parameters-do-func-and-action-support-and-which-parameter-is-always-the-return-type-for-func)
  - [Q8. How are `Func` and `Action` used in LINQ method parameters (`Select`, `Where`, etc.)?](#q8-how-are-func-and-action-used-in-linq-method-parameters-select-where-etc)
  - [Q9. When does using `Func<T, bool>` instead of `Predicate<T>` improve or hurt API clarity?](#q9-when-does-using-funct-bool-instead-of-predicatet-improve-or-hurt-api-clarity)

- [06. Closures](#06-closures)
  - [Q1. What is a closure in C#?](#q1-what-is-a-closure-in-c)
  - [Q2. How does the compiler implement variable capture for lambdas and anonymous methods?](#q2-how-does-the-compiler-implement-variable-capture-for-lambdas-and-anonymous-methods)
  - [Q3. What is the difference between capturing a variable vs capturing a value at closure creation time?](#q3-what-is-the-difference-between-capturing-a-variable-vs-capturing-a-value-at-closure-creation-time)
  - [Q4. What is the classic `for` loop closure bug, and how did C# 5 change loop variable capture semantics?](#q4-what-is-the-classic-for-loop-closure-bug-and-how-did-c-5-change-loop-variable-capture-semantics)
  - [Q5. How does the same capture bug appear in `foreach`, LINQ, and `Task.Run` callbacks?](#q5-how-does-the-same-capture-bug-appear-in-foreach-linq-and-taskrun-callbacks)
  - [Q6. What problems arise when multiple closures share the same captured variable?](#q6-what-problems-arise-when-multiple-closures-share-the-same-captured-variable)
  - [Q7. What is a display class (compiler-generated closure type), and what performance cost does capture introduce?](#q7-what-is-a-display-class-compiler-generated-closure-type-and-what-performance-cost-does-capture-introduce)
  - [Q8. What is a pure function? Give an example in C# and explain what makes it pure.](#q8-what-is-a-pure-function-give-an-example-in-c-and-explain-what-makes-it-pure)
  - [Q9. What is immutability, and why is it important in functional and concurrent programming?](#q9-what-is-immutability-and-why-is-it-important-in-functional-and-concurrent-programming)
  - [Q10. How can immutability be achieved in C# (`readonly`, `record`, avoiding mutable captures)?](#q10-how-can-immutability-be-achieved-in-c-readonly-record-avoiding-mutable-captures)
  - [Q11. How do you avoid side effects when passing lambdas to APIs that store or invoke them later?](#q11-how-do-you-avoid-side-effects-when-passing-lambdas-to-apis-that-store-or-invoke-them-later)
  - [Q12. When should you copy loop values to a local inside the loop before capturing (`var copy = item`)?](#q12-when-should-you-copy-loop-values-to-a-local-inside-the-loop-before-capturing-var-copy-item)
  - [Q13. How do local functions compare to lambdas regarding capture and allocation behavior?](#q13-how-do-local-functions-compare-to-lambdas-regarding-capture-and-allocation-behavior)
  - [Q14. Closure captures the variable, not the value — Loop lambda prints `3, 3, 3`, not `0, 1, 2`.](#q14-closure-captures-the-variable-not-the-value--loop-lambda-prints-3-3-3-not-0-1-2)
  - [Q15. Same trap in LINQ and tasks — Capturing loop variables inside `.Where()` / `Task.Run` produces identical bugs.](#q15-same-trap-in-linq-and-tasks--capturing-loop-variables-inside-where--taskrun-produces-identical-bugs)
  - [Q16. Multicast delegate short-circuit on exception — Later subscribers may not run if an early one throws.](#q16-multicast-delegate-short-circuit-on-exception--later-subscribers-may-not-run-if-an-early-one-throws)
  - [Q17. Extension method not in scope — Missing `using` for the static class namespace.](#q17-extension-method-not-in-scope--missing-using-for-the-static-class-namespace)
  - [Q18. Instance method wins over extension — An instance method hides the extension; you cannot "override" with an extension.](#q18-instance-method-wins-over-extension--an-instance-method-hides-the-extension-you-cannot-override-with-an-extension)
  - [Q19. Shared captured storage — Multiple lambdas share one slot for the same outer variable.](#q19-shared-captured-storage--multiple-lambdas-share-one-slot-for-the-same-outer-variable)
  - [Q20. Target-typed lambda ambiguity — Without a clear target type, lambda expressions may fail to compile.](#q20-target-typed-lambda-ambiguity--without-a-clear-target-type-lambda-expressions-may-fail-to-compile)
  - [Q21. Expression tree vs delegate — Expression-tree lambdas cannot contain many C# constructs that delegate lambdas allow.](#q21-expression-tree-vs-delegate--expression-tree-lambdas-cannot-contain-many-c-constructs-that-delegate-lambdas-allow)
  - [Q22. Capturing `this` implicitly — Instance lambdas capture `this`, extending object lifetime.](#q22-capturing-this-implicitly--instance-lambdas-capture-this-extending-object-lifetime)
  - [Q23. Extension on null reference — Extension methods can be called on null receivers; may throw inside the method.](#q23-extension-on-null-reference--extension-methods-can-be-called-on-null-receivers-may-throw-inside-the-method)

---

### 01. Delegates

---

## Q1. What is Functional Programming, and how does C# support it without being a purely functional language?

**Concepts**
- Functional programming support in C# via delegates and lambdas
- Multicast delegate return-value semantics (last value only)
- `GetInvocationList()` for explicit pipeline enumeration
- Function pipeline vs void notification chain design
- Explicit value fold via loop or `Aggregate`
- First-class function assignment and composition

**Answer**

C# supports functional programming through delegates, lambdas, higher-order functions, and LINQ while remaining an imperative language at its core. A sharp distinction appears with multicast delegates that have non-void return types: the runtime runs every subscriber but silently discards all but the last return value, so `pipeline += step1; pipeline += step2;` looks like sequential composition but only `step2`'s result reaches the caller. Since the intermediate prices are never threaded forward, a chained `PriceAdjuster` cannot produce an audited step-by-step result without a different design. I fix this by iterating `GetInvocationList()` and threading the value through each cast handler explicitly, which is the correct functional fold pattern:

```csharp
decimal price = listPrice;
foreach (PriceAdjuster step in pipeline.GetInvocationList().Cast<PriceAdjuster>())
{
    price = step(price);
    _logger.LogInformation("After {Step}: {Price}", step.Method.Name, price);
}
```

Alternatively I keep an explicit `List<Func<decimal, decimal>>` folded with `Aggregate`. Multicast delegates belong in void notification chains; functional value pipelines require explicit chaining.

---

## Q2. What are the key principles of Functional Programming (immutability, pure functions, first-class functions, higher-order functions, referential transparency)?

**Concepts**
- Null-conditional delegate invocation (`?.Invoke`)
- Multicast fault isolation via per-handler try/catch
- `GetInvocationList()` for resilient subscriber enumeration
- `event` vs public delegate field for access control
- Exception propagation from audit handlers blocking business logic

**Answer**

The five core FP principles all point toward predictable, isolated behavior. In C# event wiring, two common violations compound each other: invoking a nullable delegate without `?.Invoke` causes `NullReferenceException` when no subscribers are registered, and letting one throwing subscriber propagate unhandled aborts every subsequent subscriber in the chain. Because the disk-full `IOException` in an audit handler can prevent the metrics increment that must always run after saving, the ordering and fault isolation of the callback chain matter. I address both by using `?.Invoke` for the null check and by iterating `GetInvocationList()` with individual try/catch blocks so a failing handler is logged without aborting the rest:

```csharp
if (OnOrderProcessed is not null)
{
    foreach (OrderAuditHandler handler in OnOrderProcessed.GetInvocationList())
    {
        try { handler.Invoke($"Completed {orderId}: {total:C}"); }
        catch (Exception ex) { _logger.LogWarning(ex, "Audit handler failed"); }
    }
}
_metrics.Increment("orders.completed");
```

Replacing the public setter with `event` prevents external code from assigning `= null` and wiping all subscribers.

---

## Q3. What is the difference between imperative and declarative programming styles? Give a C# example of each.

**Concepts**
- Imperative style (explicit step-by-step mutation)
- Declarative style (what, not how)
- Public delegate field vs `event` access control
- External invocation risk from exposed delegate
- Test teardown corrupting shared subscriber chains

**Answer**

Imperative code spells out every step of execution explicitly; declarative code states the desired outcome and lets the runtime determine how to achieve it. Exposing a notification hook as a public delegate field is an imperative anti-pattern because it gives every consumer the ability to replace the entire chain with `= null`, invoke it externally to simulate publisher behavior, or drop all subscribers without the publisher knowing. The `event` keyword restricts outside code to `+=` and `-=` only, which is the declarative contract: callers declare their intent to subscribe or unsubscribe, and the publisher controls invocation. Concretely, `sync.SyncCompleted = null` in a test teardown silently destroys all other modules' subscriptions, while `public event OrderAuditHandler? SyncCompleted` makes that assignment a compile error outside the owning class.

---

## Q4. What does it mean for functions to be first-class citizens in C#?

**Concepts**
- First-class functions: assigned, passed, and returned
- Delegate field as shared mutable reference
- Thread-safe raise via local delegate copy
- Non-atomicity of null check followed by `Invoke`
- Immutable invocation list snapshot in local variable

**Answer**

Functions are first-class citizens when they can be assigned to variables, passed as arguments, and returned from other functions—in C# this means delegates and lambdas are values just like `int` or `string`. The threading consequence of storing a delegate in a field is that reading the field and invoking it are not atomic: a concurrent unsubscribe between the null check and the `Invoke` call can replace the field with `null`, causing `NullReferenceException` even with the `?.` operator if the read and the call are not protected. I copy the delegate reference to a local before the check—the local holds the invocation list as it existed at copy time, so any unsubscribes during the raise do not affect the current notification round:

```csharp
var chain = _auditChain;
if (chain is null) return;
foreach (OrderAuditHandler handler in chain.GetInvocationList())
{
    try { handler(message); }
    catch (Exception ex) { _logger.LogError(ex, "Audit handler failed"); }
}
```

---

## Q5. What is a delegate in C#? How does it differ from a method group and from an interface with a single method?

**Concepts**
- Delegate as type-safe function pointer
- Method group as pre-conversion method reference expression
- Single-method interface requiring full implementation class
- Captive dependency via delegate closure in singleton
- `ValidateOnBuild`/`ValidateScopes` for DI lifetime detection

**Answer**

A delegate is a type-safe object that references a method matching its signature; a method group is an expression naming one or more overloads before the compiler selects the matching delegate type; a single-method interface is a reference contract requiring a full implementation class. The critical difference from a DI perspective is that a delegate closes over its captured state at creation time—so a `Func<decimal, decimal>` built from a scoped `TaxRateProvider` during singleton construction captures that one scoped instance for the application lifetime. Later requests use a stale or disposed provider because the closure hides the captured lifetime from DI's lifetime validation. The fix is either to make `ShippingCalculator` scoped so it matches `TaxRateProvider`, or to inject `IServiceScopeFactory` and resolve a fresh provider per call rather than embedding one in a cached closure. Enabling `ValidateOnBuild` and `ValidateScopes` in development catches this class of captive dependency at startup.

---

## Q6. How do you declare, instantiate, and invoke a custom delegate type?

**Concepts**
- Custom delegate declaration syntax
- Method group vs lambda instantiation
- `?.Invoke` for nullable delegate invocation
- Delegate vs interface for single-method callback slots
- Multicast chain lifetime and mandatory unsubscribe on dispose

**Answer**

I declare a custom delegate with `public delegate TReturn TypeName(params);`, instantiate it via method group assignment (`ShippingRule rule = StandardShipping;`) or lambda, and invoke it with `instance(args)` or `instance?.Invoke(args)` for nullable delegates. For a pluggable strategy in ASP.NET Core, delegates suit single-method algorithm slots passed directly into one method call—a `ShippingRule` or `PriceAdjuster` parameter to a single method. Interfaces suit multi-method contracts that need DI registration, test doubles, or domain meaning beyond one callback. When multicast delegate chains live in-process, every subscriber must remove itself on dispose because the publisher holds a GC root; for web apps I avoid cross-request multicast chains entirely and prefer `IHostedService`, channels, or keyed DI services rather than in-process `+=`/`-=` wiring.

---

## Q7. What is a multicast delegate? How does `+=` and `-=` work on delegate instances?

**Concepts**
- Multicast delegate invocation list immutability
- `+=` and `-=` creating new delegate instances
- Delegate return-type covariance
- Static vs runtime type of a covariant delegate return
- Pattern matching for safe downcast after covariant assignment

**Answer**

A multicast delegate holds an ordered invocation list; `+=` creates a new delegate combining the existing list with the new target, and `-=` creates a new delegate with the matching target removed—both operations produce a new immutable delegate instance rather than mutating the original field. Covariance lets a method returning `DetailedReport` satisfy a `SummaryFactory` delegate typed to return `ReportSummary`, but the compiler types `factory()` as `ReportSummary` because that is the declared return type. If a later assignment swaps in a method returning a plain `ReportSummary`, a direct cast to `DetailedReport` throws `InvalidCastException` at runtime. I use pattern matching to handle both cases safely:

```csharp
ReportSummary summary = factory();
if (summary is DetailedReport detailed)
    _pdfPaginator.Configure(detailed.PageCount);
else
    _pdfPaginator.Configure(defaultPageCount: 1);
```

When all callers need derived members, I narrow the delegate type to `Func<DetailedReport>` to eliminate the downcast entirely.

---

## Q8. What is the difference between single-cast and multicast delegates at invocation time?

**Concepts**
- Single-cast delegate holds one target
- Multicast invocation list sequential execution
- Last non-void return value propagated to caller
- Exception short-circuit stops remaining subscribers
- `GetInvocationList()` for per-target control

**Answer**

A single-cast delegate holds exactly one target; invoking it runs that target and returns its result directly. A multicast delegate holds two or more targets in an ordered list; invoking it runs each target sequentially, discards all but the last non-void return value, and stops immediately if any target throws an unhandled exception—later targets in the list never run. Because of this sequential short-circuit behavior and the discard of intermediate returns, multicast delegates are appropriate for void notification chains (events, logging hooks) but not for pipelines where every step must contribute or return values must accumulate.

---

## Q9. What happens when you invoke a multicast delegate and one subscriber throws an exception?

**Concepts**
- Exception from one subscriber propagates to call site
- Subsequent subscribers skipped after exception
- `GetInvocationList()` for resilient per-handler invocation
- Per-handler try/catch pattern for independent subscribers
- Void notification chains as the primary multicast use case

**Answer**

When one subscriber in a multicast chain throws, the exception propagates immediately to the invoker and all subsequent subscribers are skipped—their handlers never run. Since independently-owned audit hooks, UI updaters, or logging callbacks should not fail together, I iterate `GetInvocationList()`, cast each element to the delegate type, and invoke each in its own try/catch block so a failing subscriber is logged without aborting the others:

```csharp
foreach (OrderAuditHandler h in OnOrderProcessed.GetInvocationList().Cast<OrderAuditHandler>())
{
    try { h(message); }
    catch (Exception ex) { _logger.LogWarning(ex, "Audit handler failed"); }
}
```

This pattern is essential for any event chain where subscribers are written by different teams and failure in one must not cascade.

---

## Q10. What is delegate covariance and contravariance in C#?

**Concepts**
- Return-type covariance on delegates
- Parameter-type contravariance on delegates
- `out`/`in` variance modifiers on generic `Func`/`Action`
- Compile-time widening vs runtime type after covariant assignment
- Pattern matching over direct cast for safe downcast

**Answer**

Delegate return-type covariance means a method returning a derived type satisfies a delegate expecting a base return type—`BuildDetailedReport` returning `DetailedReport` can be assigned to a `SummaryFactory` that returns `ReportSummary`. Contravariance is the inverse for parameters: a method accepting a base-type parameter can satisfy a delegate declaring a derived-type parameter. Both forms are supported on generic `Func`/`Action` through the `out` and `in` modifiers on type parameters. The key limitation is that covariance only widens the static return type; if a different implementation later returns a plain `ReportSummary`, a hard cast to `DetailedReport` throws at runtime. I always use `is`-pattern matching rather than a direct cast after covariant assignment, since the actual runtime type depends on which concrete method was last assigned to the delegate.

---

## Q11. When would you prefer a named delegate type over `Func`/`Action` in a public API?

**Concepts**
- Named delegate for domain-meaningful parameter intent
- `Func`/`Action` for generic utilities and LINQ pipelines
- XML doc discoverability on named delegates
- Stable API contract via named type
- Attribute support on named delegate parameters

**Answer**

I prefer a named delegate type in a public API when the callback carries domain meaning that `Func` obscures—`OrderAuditHandler`, `ShippingRule`, and `PriceAdjuster` tell a reader what the delegate represents and allow XML documentation that describes expected behavior, preconditions, and threading contract clearly. A public extension point named `Func<string, string, decimal>` forces callers to inspect every overload to understand what the three parameters mean. Named delegates also provide stable API contracts: I can attach attributes, refine documentation, or add overloads to the delegate type without breaking every call site already supplying a lambda. For private helpers and LINQ chains where context is obvious, generic `Func`/`Action` reduce boilerplate without losing clarity.

---

## Q12. What are the advantages and limitations of adopting a functional style in typical enterprise C# codebases?

**Concepts**
- Functional style benefits (composability, immutability, reduced mutable state)
- Closure allocation overhead on hot paths
- Captured-lifetime surprises in ASP.NET Core scoped services
- Cultural mismatch with OOP-oriented teams
- Incremental adoption strategy (LINQ, records, pure helpers first)

**Answer**

Functional style in C# yields clearer data transformations—LINQ pipelines, pure projection functions, and immutable `record` types compose well and reduce the mutable-state bugs common in threaded enterprise code. The main operational limitations are hidden allocation and lifetime costs: closures introduce heap-allocated display classes and captured-lifetime surprises that surface as memory pressure or scoped-service captive dependencies in ASP.NET Core—problems that a plain service class with explicit constructor injection would have made visible. There is also a cultural cost: teams accustomed to stateful OOP patterns find heavy closure use and functional composition harder to reason about in production crash dumps and profiler traces. I adopt functional style incrementally, starting with LINQ projections, pure helpers, and `record` value objects, and introduce more advanced patterns only where they measurably reduce defect surface.

---

### 02. Lambda Expressions

---

## Q1. What is a lambda expression in C#? What problem does it solve compared to named methods?

**Concepts**
- Lambda as inline anonymous delegate
- `foreach` closure capture by variable reference
- Per-iteration variable snapshot via inner local copy
- Display class generation for captured variables
- Deferred invocation vs creation-time value

**Answer**

A lambda expression is an anonymous function defined inline using `=>` syntax, solving the ceremony of declaring a separate named method for short, single-use callbacks. The tradeoff is that lambdas close over outer variables by reference rather than by value, which means each lambda stored in a collection from a `foreach` loop shares the same loop variable—when the delegates execute later, they all read the variable's current (final) value rather than the value at the iteration when they were created. I fix this by copying the iteration value to a local inside the loop so each delegate closes over an independent snapshot:

```csharp
foreach (var item in catalog)
{
    decimal rate = item.Rate;
    _rules.Add(price => price * (1m - rate));
}
```

---

## Q2. What is the difference between an expression lambda and a statement lambda?

**Concepts**
- Expression lambda (no braces, implicit return)
- Statement lambda (block body, explicit `return`)
- CS0834 compiler error for invalid block in expression position
- CS0161 not-all-code-paths-return error
- When to keep block body over expression form

**Answer**

An expression lambda contains a single expression and implicitly returns its value—`price => price * 0.9m`. A statement lambda uses a block body with braces and requires an explicit `return` for non-void delegates—`price => { if (price <= 0m) return false; return true; }`. Attempting `price => { price > 0m && price <= 999_999m }` fails because the braces declare a statement block containing only an expression statement with no `return`, causing CS0834 or CS0161. Multi-step validation with branching belongs in a statement lambda; I only use the expression form when the entire logic fits on one line without control flow. If the statement body is complex enough to warrant a name, I prefer a named method or local function assigned via method group.

---

## Q3. When can parameter types be omitted in a lambda, and when must they be explicit?

**Concepts**
- Type inference from target delegate context
- Explicit types required for ambiguous overloads
- `??=` caching a closure over a per-call parameter
- Stale delegate when parameter changes after first call
- When not to cache lambdas that close over call-site arguments

**Answer**

Parameter types can be omitted when the compiler infers them from the target delegate type—assigning `price => price * 1.08m` to a `Func<decimal, decimal>` variable lets the compiler infer `price` as `decimal`. Types must be explicit when inference is ambiguous, when using parameter attributes (C# 10+), or when two applicable delegate overloads exist at the call site. A related pitfall arises when a lambda closes over a method parameter and is cached with `??=`: the first call populates the cached delegate with the current argument's value, and subsequent calls with different arguments still invoke the original closure because the delegate is never rebuilt—caching freezes the captured value, not the calculation pattern. I fix this by computing inline or by keying the cache on the argument value rather than storing a closure over a per-call parameter.

---

## Q4. What are target-typed lambdas (C# 10+)? In what contexts does the compiler infer the delegate type?

**Concepts**
- Target-typed lambda delegate inference from assignment context
- `Expression<Func<T, bool>>` for IQueryable SQL translation
- `Func<T, bool>` forces client-side evaluation (table scan)
- EF Core provider inspects expression tree structure
- `AsEnumerable()` to intentionally switch to in-memory evaluation

**Answer**

Target-typed lambdas (C# 10+) allow the compiler to infer the delegate type from the assignment target, return type, or parameter type. The distinction between `Expression<Func<T, bool>>` and `Func<T, bool>` is critical for EF Core: `IQueryable.Where` takes an expression tree, so the provider inspects the lambda's structure and generates SQL; passing a compiled `Func<T, bool>` forces `IQueryable` to fall back to client evaluation, materializing the entire table before filtering in memory. The same `=>` syntax compiles to fundamentally different runtime behavior depending on the declared parameter type. I always use `Expression<Func<T, bool>>` in repository filter parameters and compile to `Func` only after an explicit `.AsEnumerable()` call when switching intentionally to in-memory processing.

---

## Q5. What is the natural type of a lambda — when does the compiler infer `Func`/`Action` vs require an explicit target type?

**Concepts**
- Async lambda natural type as `Func<Task>` or `Func<Task<T>>`
- Discarding the returned `Task` hides exceptions
- Unobserved task exceptions never reach logging infrastructure
- `foreach` closure capture in async fan-out
- `Task.WhenAll` for observable aggregated completion

**Answer**

A lambda's natural type in C# 10+ is the `Func` or `Action` overload whose parameter and return types match—an explicit target type is required when inference is ambiguous (e.g., standalone `x => x` with no surrounding context). An `async () => { ... }` lambda has natural type `Func<Task>`, so `Task.Run` wraps it and returns a `Task`—but if that task is discarded without awaiting, any exception stored on the task is unobserved and never reaches Application Insights. Combined with a `foreach` closure where every async lambda captures the same `sku` variable, the result is tasks that all process the last SKU and swallow their errors. I fix both by capturing a local copy of `sku` and awaiting all tasks:

```csharp
public async Task ScheduleCatalogRefreshAsync(IEnumerable<string> skus, CancellationToken ct)
{
    var tasks = skus.Select(async sku =>
    {
        try { var price = await _gateway.FetchPriceAsync(sku, ct); _cache.Set(sku, price); }
        catch (Exception ex) { _logger.LogError(ex, "Price sync failed for {Sku}", sku); throw; }
    });
    await Task.WhenAll(tasks);
}
```

---

## Q6. How do lambda expressions differ from anonymous methods in syntax, capabilities, and compiler output?

**Concepts**
- Lambda vs anonymous method syntactic differences
- Expression tree conversion available only on lambdas
- Deferred LINQ pipeline re-evaluation on each enumeration
- Closure capture causing count vs foreach mismatch
- `.ToList()` snapshot before mutating captured outer variable

**Answer**

Lambda expressions (`=>`) are syntactically lighter than anonymous methods (`delegate (T x) { ... }`) and support expression-tree conversion—a lambda assigned to `Expression<Func<T,bool>>` becomes an inspectable tree rather than compiled IL. Both forms capture outer variables by reference and share identical closure semantics; the syntax difference is cosmetic for most purposes. In deferred LINQ pipelines both exhibit the same execution-time surprise: `Count()` and a subsequent `foreach` on the same query each re-run the pipeline, reading the captured outer variable at their respective execution times. If `minPromoPrice` is mutated between the two, the count and the foreach results use different thresholds—an inconsistent financial report. I materialize the query once with `.ToList()` before mutating any captured outer variable.

---

## Q7. Can a lambda expression access `ref`, `out`, or `in` parameters from the enclosing method?

**Concepts**
- `ref`/`out`/`in` parameters not capturable in lambdas
- Stack lifetime incompatibility with heap-allocated display class
- `static` lambda keyword (C# 9+) enforces no capture at compile time
- Method group allocation characteristics for static methods
- Hoisting delegate to `static readonly` field for hot-path reuse

**Answer**

A lambda cannot capture `ref`, `out`, or `in` parameters from the enclosing method because those parameters have stack-bound lifetimes that the compiler cannot lift to the heap-allocated display class—the compiler reports an error. If by-reference data is needed, I copy it to a local first. For hot-path code where allocation matters, three styles offer different trade-offs: an inline capturing lambda allocates a display class per creation site; a method group to a static method allows the compiler to cache the same delegate instance; a `static` lambda (C# 9+) enforces at compile time that no outer locals or `this` are captured, making it safe to hoist to a `static readonly` field. I use `static` lambdas or method groups for fixed transforms and inline capturing lambdas only when the closure lifetime is scoped and the per-request allocation cost is acceptable.

---

## Q8. Can a lambda be converted to an expression tree? What syntax or API constraints apply?

**Concepts**
- `Expression<TDelegate>` target type triggers tree construction
- Expression tree body constraints (single expression, no async/statements/ref)
- `Compile()` to obtain executable delegate from tree
- LINQ provider traversal of expression node types
- Caching compiled delegates to amortize `Compile()` cost

**Answer**

A lambda converts to an expression tree when assigned to `Expression<TDelegate>`—the compiler emits abstract syntax tree construction code rather than IL for the lambda body. The constraints are significant: the body must be a single expression (no block, no `if`, no loops, no `await`, no `ref`/`out` parameters), and pointer types and unbound generic methods are not allowed. The expression tree is traversable at runtime through `Body`, `Parameters`, and node types in `System.Linq.Expressions`, which is how EF Core translates predicates to SQL. Calling `.Compile()` on the expression returns a regular invocable delegate, though this compilation is expensive and the result should be cached. When complex logic is needed in a filter that must translate to SQL, I split it: an expression tree for the translatable parts and a delegate for any remaining in-memory refinement after `AsEnumerable()`.

---

## Q9. What is the difference between a lambda that captures no locals vs one that captures outer variables?

**Concepts**
- Capture-free lambda compiled as reusable singleton delegate
- Capturing lambda allocates display class and new delegate per creation
- `static` lambda keyword prevents accidental capture
- Allocation impact on high-throughput endpoints
- Hoisting `static readonly` delegate for non-capturing transforms

**Answer**

A lambda that captures no outer variables or `this` is effectively a static method—the compiler can cache it as a singleton delegate instance, avoiding allocation on repeated calls. A capturing lambda triggers the compiler to generate a display class holding the captured variables and to allocate a new delegate instance pointing to it each time the enclosing code runs. On hot endpoints processing thousands of requests per second, creating a new closure object on each request inflates Gen0 allocation and increases GC pressure. I use `static` lambdas (`static p => p * 1.08m`) when the transform is constant to enforce no capture at compile time, hoist delegates to `static readonly` fields when possible, and reserve capturing lambdas for code paths where per-call closure allocation is justified by the need to close over request-specific state.

---

## Q10. How do async lambdas work (`async x => ...`)? What delegate types can they target?

**Concepts**
- Async lambda compiles to state machine
- Target types: `Func<Task>`, `Func<Task<T>>`, `Func<CancellationToken, Task>`
- `async void` lambda swallows exceptions — avoid
- Fire-and-forget task discard hides failures
- `Task.WhenAll` for observable concurrent async batches

**Answer**

An `async` lambda compiles to a state machine the same way a named `async` method does, and it can target `Func<Task>` (no result) or `Func<Task<T>>` (with result). It should not target `Action` because `async void` swallows exceptions—there is no `Task` to observe the fault. When passing an `async` lambda to `Task.Run`, the returned `Task` represents the async work and must be awaited or aggregated with `Task.WhenAll` to surface exceptions. Combining an `async` lambda with a fire-and-forget discard (`_ = Task.Run(async () => ...)` or simply not awaiting) is the most common source of silent background failures in .NET services, since the exception is stored on the unreferenced task and never observed.

---

## Q11. What happens if you use a lambda where a `Expression<TDelegate>` is expected vs where a `TDelegate` is expected?

**Concepts**
- Expression tree compilation (no IL, tree objects emitted)
- Delegate compilation (IL emitted, immediately executable)
- IQueryable provider uses expression tree for SQL translation
- Delegate parameter forces client-side evaluation
- Overload resolution between expression and delegate overloads

**Answer**

When a lambda is assigned to `Expression<Func<T, bool>>`, the compiler emits expression-tree construction code—no IL for the lambda body runs until `Compile()` is called. When assigned to `Func<T, bool>`, the compiler emits IL for the lambda body directly and the result is immediately invocable. For `IQueryable.Where`, the expression-tree overload lets the provider translate the predicate to SQL; the delegate overload forces client-side evaluation because the provider cannot inspect compiled IL, which means the entire table is materialized first. If both overloads are available and the call is ambiguous, the compiler picks the more specific one or raises an ambiguity error. I declare repository filter methods with `Expression<Func<T, bool>>` parameters and compile to delegates only when explicitly switching to in-memory processing with `.AsEnumerable()`.

---

### 03. Anonymous Methods

---

## Q1. What are anonymous methods in C#? Why were they introduced, and what largely replaced them?

**Concepts**
- Anonymous method `delegate (T x) { ... }` syntax
- Event subscription without paired unsubscription
- Implicit `this` capture via instance method call
- Long-lived publisher as GC root into subscriber
- Stored handler in field for `-=` on dispose

**Answer**

Anonymous methods were introduced in C# 2.0 to write short delegate implementations inline without declaring a named method. Lambda expressions (C# 3.0) replaced them for most uses because lambdas are terser, support expression trees, and allow parameter type inference. Both forms share identical closure semantics. The critical lifetime issue that remains regardless of syntax is that subscribing to a long-lived event without unsubscribing creates a GC root from the publisher to the subscriber—when an anonymous method captures `this` via an instance method call like `RefreshGrid`, the entire form and all its fields remain reachable. I store the delegate in a field and call `-=` in `OnFormClosed`:

```csharp
private EventHandler<OrderUpdatedEventArgs>? _handler;

public OrderDetailDialog(OrderService service, int orderId)
{
    _service = service;
    _handler = delegate (object sender, OrderUpdatedEventArgs e)
    {
        if (e.OrderId == orderId) RefreshGrid(e.Order);
    };
    _service.OrderUpdated += _handler;
}

protected override void OnFormClosed(FormClosedEventArgs e)
{
    _service.OrderUpdated -= _handler;
    base.OnFormClosed(e);
}
```

---

## Q2. What is the syntax for an anonymous method, and how does it compare to lambda syntax?

**Concepts**
- Anonymous method vs lambda syntactic equivalence
- Identical closure capture semantics in both forms
- Config snapshot captured at delegate creation time
- Cached rule array unaffected by runtime config changes
- `IOptionsMonitor<T>` for live configuration reads

**Answer**

An anonymous method is written as `delegate (T x) { ... }`; the equivalent lambda is `x => { ... }` or `x => expr`. Beyond syntax they generate the same compiler display class and capture outer variables by reference—migrating from one to the other is purely cosmetic unless the capture intent changes. The shared closure pitfall is that when a rule delegate is built from a config value at construction time, later config changes do not affect cached delegates because the closure captured the value that existed at creation. Both the anonymous-method version and the lambda version of the same factory have this bug; blaming the syntax wastes time. I fix it by subscribing to `IOptionsMonitor<T>` reload events and rebuilding the rule set, or by capturing the options accessor rather than the resolved value so each invocation reads current config.

---

## Q3. Can anonymous methods omit parameter lists? When is that useful?

**Concepts**
- Parameter-list omission with `delegate { ... }` (any signature match)
- Lambda requires explicit parameter declaration
- CS0126: `return value` in void delegate body
- Captured `StringBuilder` lives on heap after factory returns
- Thread-safety of shared mutable captures under concurrency

**Answer**

An anonymous method can omit its parameter list with `delegate { ... }`, which matches any delegate signature regardless of parameters—useful for event handlers where the sender and args are irrelevant and naming them adds noise. A lambda cannot do this and must declare all parameters. The compile error CS0126 occurs when a void-return delegate body attempts `return value`—a `void` `OrderNotifier` body that tries `return message.Length > 0` does not compile because the delegate declares no return type. A bare `return;` for early exit is legal. Independently, capturing a `StringBuilder` by reference means the same buffer instance is mutated by every invocation, which is fine for single-threaded use but corrupts state under concurrent access since `StringBuilder` is not thread-safe.

---

## Q4. What outer scope variables can anonymous methods access, and how does capture work?

**Concepts**
- Captured scope: locals, parameters, and implicit `this`
- Display class heap allocation promotes captured locals off stack
- Incremental migration preserving capture intent
- Omitted parameter list uniqueness to anonymous methods
- Event handler field-plus-unsubscribe verification before merge

**Answer**

Both anonymous methods and lambdas can capture any variable in scope at the point of definition—local variables, method parameters, and implicitly `this` for instance member access. The compiler lifts captured variables into a heap-allocated display class, so they outlive the enclosing method's stack frame for as long as any delegate referencing them remains alive. For incremental migration from `delegate { ... }` to lambdas in a legacy codebase, I touch only files already being changed, verify that captured variables are identical before and after, confirm that handlers using the omitted parameter list (`delegate { ... }`) become `(_, _) => { ... }` or similar rather than accidentally naming parameters that then require the right types, and run existing tests plus a memory profile on dialogs with event subscriptions to confirm no new leaks appear.

---

## Q5. In modern C# code, when (if ever) would you still choose an anonymous method over a lambda?

**Concepts**
- Anonymous method parameter-list omission as unique feature
- Value types on heap when captured (display class field)
- Closure mutation through display class field
- Lambda vs anonymous method allocation equivalence
- `int` captured local not stack-only after closure creation

**Answer**

In modern C# I choose an anonymous method over a lambda only when I need to omit the parameter list entirely—`delegate { ... }` matches any delegate type without naming parameters, which is convenient for subscribing to events where the args are irrelevant and verbose naming adds noise. For all other uses, lambdas are cleaner. The separate misconception that `int matchCount = 0` stays on the stack is wrong the moment the anonymous method captures it: the compiler generates a display class with a `matchCount` field, and the local variable becomes an alias to that field. The `int` lives on the heap inside the display class for as long as any delegate referencing it remains alive, regardless of it being a value type. Mutations inside the anonymous method update the heap field, which is why the count increments correctly across invocations.

---

## Q6. A teammate argues that anonymous methods in `RunAllRules` should stay inline because "they are only five lines," but QA cannot unit-test individual rules without running the whole pipeline.

**Concepts**
- Testability via named delegates or local functions
- Single-responsibility applied at anonymous-method level
- Pipeline dependency reduction through rule extraction
- `static` local function to prevent accidental capture
- Five-line inline vs independently verifiable unit

**Answer**

The "five lines" argument favors brevity but ignores that inline anonymous methods cannot be independently tested, named in call stacks, or reused outside the pipeline. I extract each rule to either a named private method (best for complex logic), a `static` local function (scoped to the enclosing method, compile-time enforcement of no accidental capture), or a stored `Func<Order, bool>` field so QA can inject individual rules in isolation. The pipeline orchestrator then holds a list of extracted delegates, making each rule a separate, testable unit. This is the same argument against god classes applied at the anonymous-method level: brevity at the definition site costs testability and discoverability, and the trade-off is rarely worth it for rules that carry business meaning.

---

### 04. Extension Methods

---

## Q1. What are extension methods in C#? How do they appear to the caller vs how they are implemented?

**Concepts**
- Extension method as static method with `this`-prefixed first parameter
- Caller sees instance-style syntax; compiler emits static call
- Non-nested top-level static class requirement
- CS1110/CS1106 for extension in nested class
- Dedicated `*.Extensions` namespace and class placement

**Answer**

An extension method is a static method in a non-nested static class whose first parameter is prefixed with `this`—this signals the compiler to allow instance-style invocation on that type. The caller sees `line.ToReceiptLine()` but the compiler emits the static call `OrderLineExtensions.ToReceiptLine(line)`. The extension must live in a non-nested, top-level static class at namespace scope; placing it inside another class (even a nested `static class`) produces CS1110 or CS1106 because the compiler does not search nested types for extension candidates. I relocate it to a dedicated `public static class OrderLineExtensions` at the appropriate namespace root:

```csharp
namespace Acme.Ordering.Extensions;

public static class OrderLineExtensions
{
    public static string ToReceiptLine(this OrderLine line) =>
        $"{line.Sku} x{line.Quantity} = {line.LineTotal:C}";
}
```

---

## Q2. What are the language rules for declaring an extension method (static class, `this` parameter, accessibility)?

**Concepts**
- Extension discovered by namespace `using`, not assembly reference alone
- `using static` imports static members but not extension binding
- `this` as first parameter position requirement
- Public static class accessibility requirement
- `GlobalUsings.cs` for project-wide extension namespace imports

**Answer**

An extension method requires a public static class at namespace scope, with the extended type as the first parameter marked `this`, and accessible visibility matching the caller's context. Discovery at the call site depends on a plain `using` directive for the extension class's namespace—without it, even a referenced assembly's extensions are invisible for instance-style calls. `using static Acme.Common.Extensions.StringExtensions` imports static members for direct-call syntax (`StringExtensions.ToDisplayLabel(id)`) but does not enable instance-style extension binding (`id.ToDisplayLabel()`), which is a common source of confusion. I add `using Acme.Common.Extensions;` to every file that needs instance-style syntax, or I add it globally in `GlobalUsings.cs` for widely used helpers.

---

## Q3. How does the compiler resolve an extension method call at compile time?

**Concepts**
- Extension call compiled to static call — null receiver allowed
- No NullReferenceException at call site for null receiver
- Exception originates inside method body on null dereference
- `this string?` for null-safe extension annotation
- Nullable reference type flow analysis for extension parameters

**Answer**

The compiler resolves an extension method call by searching in-scope `using` namespaces for static methods whose `this`-prefixed parameter type is compatible with the receiver type. Because the call compiles to a static invocation, a null receiver is passed as the first argument without a null check at the call site—no `NullReferenceException` at the call site, but the method body throws when it dereferences the argument. This differs from instance method dispatch, which throws immediately before entering the method body. I annotate the `this` parameter as `string?` in extensions designed to handle null gracefully and document non-null preconditions with `string` (non-nullable) when the method legitimately throws on null input, enabling nullable flow analysis to warn callers who pass a `string?` without a guard:

```csharp
public static string? NormalizePromoOrNull(this string? code) =>
    string.IsNullOrWhiteSpace(code) ? null : code.Trim().ToUpperInvariant();
```

---

## Q4. What is the difference in resolution order between an instance method and an extension method with the same signature?

**Concepts**
- Instance method always wins over extension method
- Extension tie-break by `this` type specificity then namespace order
- Silent rebind when duplicate extension is added by a new package
- Explicit static call to disambiguate
- Security-critical call sites require explicit disambiguation

**Answer**

Instance methods always win over extension methods with the same signature—the compiler never considers an extension when an applicable instance method exists. Among competing extension methods, the compiler picks the most specific match by `this` type; if still tied, namespace ordering and internal tie-break rules apply, and one extension wins at compile time without a runtime error. A silent rebind happens when tie-breaking changes after adding a second NuGet package with an identically-named extension method—`userInput.Sanitize()` now calls a different implementation than before without any compile-time warning. I disambiguate by calling the static form explicitly: `StringJsonExtensions.Sanitize(userInput)`. At integration boundaries involving security or serialization, I always use explicit static calls rather than relying on implicit extension resolution.

---

## Q5. Can extension methods access `private` members of the extended type? Why or why not?

**Concepts**
- Extension method sees only public/internal members
- Encapsulation preserved because extension is an external static call
- `yield return` deferred enumeration in extension methods
- Multiple enumeration of deferred pipeline (double DB round-trip)
- `.ToList()` snapshot for stable multi-pass aggregation

**Answer**

Extension methods cannot access `private` or `protected` members because they are static methods in a separate class and the compiler emits ordinary static calls—they see only the same members accessible to any external caller. This is intentional: extensions extend behavior without breaking encapsulation. A separate concern with extension methods on `IEnumerable<T>` using `yield return` is that they are deferred—each enumeration of the result re-executes the full pipeline from the source. When a caller uses `.Sum()` and then `.Count()` on a non-materialized pipeline backed by a database query with per-element logging, both the database round-trip and the logging fire twice. I materialize with `.ToList()` before multiple passes when the source is expensive or has observable side effects.

---

## Q6. What are the limitations of extension methods?

**Concepts**
- No access to private or protected members
- Cannot override virtual instance methods
- No polymorphic dispatch (resolved by static type)
- Domain logic in instance methods for owned types
- Extension as syntactic adapter for unowned or BCL types

**Answer**

Extension methods cannot access private or protected members, cannot override virtual instance methods, and do not participate in polymorphic dispatch—the method is resolved by the static type of the variable, not the runtime type. They do not belong to the type's contract, so discoverability relies on the caller importing the right namespace. For domain types I own with core business rules like `ApplyBulkDiscount` and `ApplyRegionalTax`, I prefer instance methods because they appear with the type in documentation, are visible in derived types, and do not pollute IntelliSense for callers who never import the extension namespace. I reserve extensions for cross-cutting syntactic helpers, formatting adapters (`ToReceiptLine`), and operations on types I cannot modify—BCL types, third-party sealed types, and interfaces.

---

## Q7. How do extension methods work on interfaces? What are design implications (e.g., LINQ)?

**Concepts**
- Extension on interface attaches behavior to all implementors
- LINQ `Select`/`Where` as interface extension methods
- `IApplicationBuilder` fluent middleware registration pattern
- `HttpContext.Items` for per-request correlation state
- `ILogger` scope for structured log correlation

**Answer**

Extension methods on interfaces attach behavior to any type implementing the interface without modifying the interface's contract—this is how LINQ's `Select`, `Where`, and `OrderBy` work on `IEnumerable<T>`, and how `UseRouting` and `UseAuthentication` work on `IApplicationBuilder`. The extension is discovered for all implementors when the right `using` is imported. For middleware, `app.UseCorrelationId()` reads naturally as pipeline registration, returns `IApplicationBuilder` for chaining, and keeps registration API in a separate static class from the middleware logic. A common mistake is echoing the correlation ID to the response header but forgetting to store it in `HttpContext.Items` and an `ILogger` scope—downstream middleware and controllers never see the ID because the header goes to the client, not to in-process logging infrastructure.

---

## Q8. What happens when two namespaces define extensions with the same name and signature for the same type?

**Concepts**
- Extension static dispatch not mockable with standard test frameworks
- Missing `using` causes CI discovery failure
- Duplicate extension silent rebind on package addition
- Interface injection for swappable per-environment behavior
- Pure extension vs policy extension testability distinction

**Answer**

When two namespaces define extensions with the same name and signature, the compiler may silently choose one based on namespace specificity or raise an ambiguity error requiring explicit disambiguation. Adding a package can silently rebind extension calls—a maintenance hazard with no runtime signal. A related discoverability issue is that extension methods require the correct `using` directive in every file that uses them; CI builds may fail when a file references an extension whose namespace is not imported, while local builds pass because a `GlobalUsings.cs` or file-level `using` already exists there. For validation or transformation logic that must be swappable by environment, I extract behavior behind an interface and inject it—extensions are static dispatch and cannot be mocked with Moq or NSubstitute, while interfaces enable per-environment substitution without `#if DEBUG` forks.

---

## Q9. Can you define generic extension methods? How does type inference work at the call site?

**Concepts**
- Generic `this` parameter for type-parameterized extensions
- Type inference from receiver type at call site
- Explicit type argument when inference is ambiguous
- Generic constraints narrowing extension applicability
- LINQ built on generic extension methods with inferred type parameters

**Answer**

Generic extension methods use a type parameter on the `this`-prefixed parameter—`public static T Tap<T>(this T source, Action<T> action)`. At the call site, the compiler infers `T` from the receiver type without requiring explicit type arguments. When the receiver type uniquely constrains inference (e.g., `source` is `List<int>`), inference succeeds; when ambiguous, I supply the type argument explicitly. Constraints (`where T : class`, `where T : IEnumerable<TItem>`) narrow applicability and improve IntelliSense by filtering out irrelevant suggestions. LINQ is built entirely on generic extension methods—`Select<TSource, TResult>`, `Where<TSource>`—where inference from the source sequence type drives the entire pipeline without visible type arguments at the call site.

---

## Q10. What are anti-patterns with extension methods (god extensions, violating encapsulation)?

**Concepts**
- God extension class (many unrelated methods on one type)
- Domain logic in extensions bypasses DI and constructor seams
- IntelliSense pollution from oversized extension classes
- Extensions shadowing or duplicating instance members
- Owned type behavior belongs on the type or a domain service

**Answer**

The primary extension method anti-patterns are god extension classes that accumulate dozens of unrelated helpers on a single type—breaking single responsibility and making IntelliSense lists unmanageable—and placing domain-critical behavior like pricing algorithms in extensions on owned types when an instance method would be clearer. Extensions that patch behavior on third-party or BCL types are the legitimate use case; extensions adding 40-line pricing algorithms to domain types obscure the domain model, bypass constructor and DI seams, and make the logic uninjectableand harder to test in isolation. I also avoid extensions that duplicate or slightly shadow public instance members with different behavior, since the instance always wins in resolution and the extension becomes invisible once the instance method exists.

---

### 05. Func, Action & Predicate

---

## Q1. What are `Func<T>`, `Func<T, TResult>`, and the general `Func<...>` family?

**Concepts**
- `Func` generic delegate family for value-returning callbacks
- Zero-to-sixteen input type parameters
- Last type parameter always the return type (`TResult`)
- Method group assignment compatibility
- Replaces boilerplate custom delegate declarations for internal helpers

**Answer**

`Func` delegates are generic built-in types in `System` representing functions that return a value. `Func<TResult>` takes no parameters and returns `TResult`; `Func<T, TResult>` takes one argument; the family extends to sixteen input type parameters, with the last parameter always being the return type. I use `Func<string, int>` to store `s => s.Length` or assign a method group like `Math.Abs` to `Func<int, int>`. For private helpers and LINQ pipelines, `Func` removes the boilerplate of declaring a named delegate type while preserving type safety and allowing method group assignment when a compatible static or instance method exists.

---

## Q2. What is `Action` vs `Action<T>` vs `Action<T1, T2, ...>`?

**Concepts**
- `Action` as void-returning delegate family
- Zero-to-sixteen parameter overloads
- Side-effect callbacks: logging, notifications, UI updates
- `Action` fills the void-return role that `Func<void>` cannot

**Answer**

`Action` delegates represent void-returning callbacks. Plain `Action` takes no parameters; `Action<T>` takes one; multi-parameter overloads extend to sixteen inputs, all returning void. I use `Action` when the callback is purely for side effects—logging, UI updates, notifications—where no value needs to propagate back to the caller. `Func` with a void return type does not exist in the BCL, so `Action` fills that role, and confusing the two causes compile errors when passing a callback to an API that expects one and not the other.

---

## Q3. What is `Predicate<T>`, and how does it relate to `Func<T, bool>`?

**Concepts**
- `Predicate<T>` as legacy single-argument bool-returning delegate
- Semantic equivalence with `Func<T, bool>`
- Legacy BCL APIs: `List<T>.FindAll`, `Array.TrueForAll`
- Prefer `Func<T, bool>` in new APIs for LINQ alignment
- Trivial lambda/method-group conversion between both types

**Answer**

`Predicate<T>` is a legacy built-in delegate type taking one argument and returning `bool`—semantically identical to `Func<T, bool>`. It predates generic `Func` delegates and is still required by older BCL methods like `List<T>.FindAll` and `Array.TrueForAll`. In new code I use `Func<T, bool>` because it aligns with LINQ's `Where` signature and avoids a distinct type that callers must convert from when composing with LINQ. Lambdas and method groups bind to either type when context demands it, so conversion at call sites is trivial when wrapping legacy BCL methods.

---

## Q4. When should you use `Func` vs `Action` vs `Predicate` vs a custom delegate?

**Concepts**
- `Func` for value-returning callbacks
- `Action` for void side-effect callbacks
- `Func<T, bool>` as modern predicate standard
- Named delegate for public domain-meaningful API contracts
- `Predicate<T>` only for legacy BCL interop

**Answer**

I use `Func` when the callback returns a value, `Action` for void side effects, and `Func<T, bool>` for filter predicates in new code. A custom named delegate earns its place in a public API when the signature carries domain meaning that a generic `Func` obscures—`ShippingRule`, `OrderAuditHandler`, and `PriceAdjuster` document intent and can carry XML documentation that `Func<decimal, decimal>` cannot. I avoid `Predicate<T>` except when calling legacy BCL methods that require it, and I do not use custom delegates for private helpers where `Func`/`Action` reduce boilerplate without losing clarity.

---

## Q5. What are higher-order functions? Give C# examples using `Func` and `Action`.

**Concepts**
- Higher-order function definition (takes or returns functions)
- `Func` as parameter type for strategy injection
- Factory methods returning `Func` as higher-order producers
- LINQ `Select`/`Where` as built-in higher-order functions
- Strategy injection via delegate parameter vs subclass explosion

**Answer**

A higher-order function either takes a function as a parameter or returns a function as a result. In C#, `ApplyToAll(decimal[] prices, Func<decimal, decimal> transform)` is higher-order because it accepts a function for the transform. Factory methods returning `Func<int, int>` are higher-order because they produce functions configured at creation time. LINQ `Select` and `Where` are higher-order—they accept `Func` delegates and apply them across sequences lazily. This style enables strategy injection without subclass explosion: instead of subclassing `PricingService` for each pricing rule, I pass a different `Func<decimal, decimal>` at the call site, keeping the algorithm and the orchestration cleanly separated.

---

## Q6. What is function composition, and how can it be achieved in C#?

**Concepts**
- Function composition: output of one is input of next
- Nested lambda composition (`x => f(g(x))`)
- `Aggregate` for folding a list of functions
- LINQ `Select` chaining as compositional pipeline
- Custom `Compose`/`Pipe` extension method pattern

**Answer**

Function composition chains two functions so the output of one becomes the input of the next—mathematically `(f ∘ g)(x) = f(g(x))`. C# has no built-in composition operator, so I compose by nesting: `Func<int, int> h = x => f(g(x));`. For longer chains I fold a list of `Func<decimal, decimal>` using `Aggregate`: `decimal result = steps.Aggregate(input, (acc, fn) => fn(acc))`. LINQ pipelines compose declaratively when chaining `Select` calls—`.Select(g).Select(f)` maps `f(g(x))` over the sequence. Deferred execution means neither function runs until the pipeline is enumerated, which is the correct behavior for composing transformations that are defined separately from where data flows through them.

---

## Q7. How many generic parameters do `Func` and `Action` support, and which parameter is always the return type for `Func`?

**Concepts**
- `Func` arity: up to sixteen input parameters plus one return type
- `Action` arity: up to sixteen input parameters, no return type parameter
- Last type parameter in `Func` is always `TResult`
- Practical arity rarely exceeds three in production code
- High arity signals refactoring opportunity

**Answer**

Both `Func` and `Action` have overloads supporting up to sixteen input type parameters. For `Func`, the last type parameter is always `TResult`—the return type—so `Func<T1, T2, T3, TResult>` has three inputs and one return. `Action` overloads have no return type parameter since all return void. In practice, most code uses zero to three parameters; very high arities suggest the function takes too many arguments and the signature should be refactored, perhaps by grouping related parameters into a dedicated type.

---

## Q8. How are `Func` and `Action` used in LINQ method parameters (`Select`, `Where`, etc.)?

**Concepts**
- `Where` takes `Func<T, bool>` predicate
- `Select` takes `Func<T, TResult>` projector
- Deferred execution: delegates run during enumeration not registration
- `IQueryable` overloads take `Expression<Func<...>>` for provider translation
- Same lambda syntax compiles to different behavior based on declared parameter type

**Answer**

LINQ extension methods on `IEnumerable<T>` accept `Func` delegates: `Where` takes `Func<T, bool>`, `Select` takes `Func<T, TResult>`, and `Aggregate` takes `Func<TAccumulate, T, TAccumulate>`. The caller supplies lambdas or method groups matching those shapes, and the delegates are invoked lazily during enumeration rather than when `Where` or `Select` is called. For `IQueryable`, the overloads accept `Expression<Func<...>>` so the provider can translate to SQL; the syntactic appearance at the call site is identical, which is why accidentally passing a compiled `Func` to an `IQueryable` pipeline is such a common source of full-table scans in production.

---

## Q9. When does using `Func<T, bool>` instead of `Predicate<T>` improve or hurt API clarity?

**Concepts**
- `Func<T, bool>` aligns with LINQ `Where` for composability
- `Predicate<T>` required by legacy BCL methods
- Mixed usage increases mental overhead for callers
- Standardization on `Func<T, bool>` in new APIs
- Isolation of `Predicate<T>` conversions at legacy BCL call sites

**Answer**

`Func<T, bool>` improves API clarity and consistency in new code because it aligns with `IEnumerable.Where`, making filters composable with LINQ pipelines without conversion. Using `Predicate<T>` alongside `Func<T, bool>` in the same API hurts clarity because callers must track which overload requires which type and occasionally adapt lambdas. The only time `Predicate<T>` improves clarity is when wrapping legacy BCL methods like `List<T>.FindAll`—matching the BCL's own naming signals the adapter's purpose. In all other new code I standardize on `Func<T, bool>` and isolate `Predicate<T>` conversions to the thin wrappers that call legacy BCL APIs directly.

---

### 06. Closures

---

## Q1. What is a closure in C#?

**Concepts**
- Closure as function plus captured variable environment
- `for` loop single shared variable slot
- Deferred task execution reads final variable value
- Inner local copy to snapshot per-iteration value
- `foreach` C# 5+ per-iteration variable semantics

**Answer**

A closure is a function (lambda or anonymous method) together with the environment of outer variables it references—those variables are captured by reference into a compiler-generated display class, not copied by value. In a `for` loop, all iterations share one `i` variable, so every lambda stored during the loop captures a reference to the same field. When `Task.Run` callbacks execute after the loop finishes, `i` has reached its final value and every callback processes the same (wrong) order ID. A fast local smoke test can mask this bug if tasks start before the loop increments, which is why it reaches production. I fix it by copying `i` to an inner local at each iteration:

```csharp
for (int i = 0; i < 3; i++)
{
    int orderId = i;
    Task.Run(() => orders.ProcessOrder(orderId));
}
```

---

## Q2. How does the compiler implement variable capture for lambdas and anonymous methods?

**Concepts**
- Display class generated for each captured scope
- Single display class field shared by all lambdas in same scope
- Deferred delegate invocation reads field at invocation time
- Inner local copy creates per-iteration display class field
- Factory helper method as alternative to force per-call scope

**Answer**

When a lambda captures an outer variable, the compiler generates a display class—a heap-allocated object with a field for each captured variable. The lambda's body becomes a method on this class, and all lambdas created within the same scope sharing the same variable use one display class instance and therefore one field. In a `for` loop, all iterations share a single `tier` field; every `filters.Add(price => price >= thresholds[tier])` stores a delegate that reads the same field at execution time. When the list executes later, `tier` holds its final post-loop value and all filters behave identically. I introduce a per-iteration local—`int capturedTier = tier;`—so each iteration's lambda gets its own display class field, or I extract a helper method whose parameter creates a fresh scope per call.

---

## Q3. What is the difference between capturing a variable vs capturing a value at closure creation time?

**Concepts**
- Variable capture: reference to display class field (live updates visible)
- Value capture: copy to local before lambda (independent snapshot)
- Implicit `this` capture via instance method call in lambda
- Event subscription GC root from publisher to subscriber
- Unsubscribe in `Dispose` to release subscriber from root chain

**Answer**

Capturing a variable means the lambda holds a reference to the display class field—any mutation of that field is immediately visible to the lambda. Capturing a value requires copying to a local before the lambda, so the closure holds a snapshot independent of subsequent changes. In the `TradeDetailPanel` example, the event handler implicitly captures `this` by calling the instance method `UpdateChart`, and the publisher's multicast delegate then holds a GC root into the panel and its 512 KB buffer. Since `Dispose` does not call `-=`, every closed panel accumulates in the event chain. I store the handler in a field and unsubscribe in `Dispose`:

```csharp
private readonly EventHandler<TickEventArgs> _tickHandler;

public TradeDetailPanel(MarketDataFeed feed)
{
    _feed = feed;
    _tickHandler = (_, tick) => UpdateChart(tick, _quoteBuffer);
    _feed.TickReceived += _tickHandler;
}

public void Dispose() => _feed.TickReceived -= _tickHandler;
```

---

## Q4. What is the classic `for` loop closure bug, and how did C# 5 change loop variable capture semantics?

**Concepts**
- `for` loop single variable slot unchanged in C# 5
- C# 5 `foreach` per-iteration variable (fixes the equivalent foreach bug)
- Timer as long-lived closure host with minimal-capture pattern
- Capturing identifiers only, not full object graphs
- `Timer.Dispose()` after callback completes to release GC root

**Answer**

The classic `for` loop closure bug occurs because a `for` statement declares one loop variable shared across all iterations—lambdas captured inside the loop all reference the same field, so deferred invocations read the final post-loop value. In C# 5, `foreach` was changed so each iteration gets its own copy of the iteration variable, fixing the parallel bug for `foreach` without requiring an inner-local workaround. The `for` loop was not changed—it still needs the explicit copy. Timers are a particularly dangerous closure host because they stay alive in a list and fire on thread-pool threads; I capture only primitive identifiers, load fresh state from a service on each callback invocation rather than closing over a full request object, and call `timer.Dispose()` after success or final failure to break the GC root.

---

## Q5. How does the same capture bug appear in `foreach`, LINQ, and `Task.Run` callbacks?

**Concepts**
- Same capture mechanics in `foreach`/LINQ/`Task.Run`
- Parallel closure mutation of shared locals as data race
- `Interlocked.Increment` for atomic counter under parallelism
- `ConcurrentBag<T>` for thread-safe collection under parallelism
- `Parallel.ForEach` local-init/finally for lock-free aggregation

**Answer**

The same capture mechanic that causes the `for` loop bug appears in `foreach` loops (before C# 5), LINQ deferred queries that re-evaluate after a variable changes, and `Task.Run` callbacks that run after the enclosing method returns. In all cases the lambda captures a reference rather than a snapshot. The parallel variant is worse: `Parallel.ForEach` runs iterations concurrently, so two threads can both read `invalidCount`, compute `count + 1`, and write back the same value—a lost update. `List<T>.Add` under concurrent access corrupts internal state. I replace `List<T>` with `ConcurrentBag<T>` and `invalidCount++` with `Interlocked.Increment(ref invalidCount)`, or I use the per-partition local-state overload of `Parallel.ForEach` to accumulate without shared mutation.

---

## Q6. What problems arise when multiple closures share the same captured variable?

**Concepts**
- Shared display class field: mutations visible across all sharing lambdas
- Shared mutable state as data race under parallelism
- Per-request closure allocation on hot API endpoints
- 50 display classes plus 50 delegates per request = GC pressure
- Inline range check or local function to eliminate per-iteration allocation

**Answer**

When multiple lambdas share a captured variable, they all read and write the same display class field—mutations from one lambda are visible to others, which is intentional for accumulators but a race condition under parallelism. The secondary problem is allocation: each capturing lambda creates a display class instance plus a delegate instance, so a loop creating 50 lambdas per HTTP request produces roughly 50 display classes and 50 delegates on every request, inflating Gen0 pressure. I eliminate this by replacing the 50 closures with a single inline range check capturing only `minPrice`, or by extracting the predicate to a local function or static method so no per-iteration heap allocation occurs:

```csharp
app.MapGet("/products", (decimal minPrice, ProductRepository repo) =>
{
    decimal maxFloor = minPrice + 49;
    return repo.GetAll()
        .Where(p => p.UnitPrice >= minPrice && p.UnitPrice <= maxFloor)
        .ToList();
});
```

---

## Q7. What is a display class (compiler-generated closure type), and what performance cost does capture introduce?

**Concepts**
- Display class as compiler-generated heap-allocated closure type
- One display class instance per factory call (per-call isolation)
- Mutable captured state hidden from API surface
- Named `RuleState` class for testable explicit state
- `static` local function with explicit parameters for zero hidden capture

**Answer**

A display class is the heap-allocated type the compiler generates when a lambda captures outer variables; it holds one field per captured variable and exposes the lambda body as a method. Each `CreateRule` call produces its own display class instance, so the mutable `hits` counter is per-factory-call rather than globally shared—the captures are isolated per returned delegate. The performance cost is one heap allocation per closure creation plus ongoing GC pressure proportional to delegate lifetime. For the mutable `hits` counter, I can keep the closure if the semantics are well understood, but testability improves by extracting a named `RuleState` class whose properties tests inspect directly, or by using a `static` local function with explicit state parameters so no hidden capture exists:

```csharp
public Func<int, bool> CreateRule(int threshold)
{
    var state = new RuleState();
    return value => Evaluate(value, threshold, state);

    static bool Evaluate(int value, int threshold, RuleState s)
    {
        bool pass = value >= threshold;
        if (pass) s.Hits++;
        return pass && s.Hits <= 3;
    }
}
```

---

## Q8. What is a pure function? Give an example in C# and explain what makes it pure.

**Concepts**
- Deterministic output for identical inputs
- No observable side effects (no shared state mutation, no I/O)
- Referential transparency: call replaceable with its return value
- Thread safety without synchronization
- Composability in LINQ pipelines

**Answer**

A pure function always produces the same output for the same inputs and produces no observable side effects—it does not modify shared state, write to I/O, or depend on mutable external data. `static decimal ApplyDiscount(decimal price, decimal rate) => price * (1m - rate);` is pure because it reads only its parameters, returns a computed value, and leaves no trace outside the call. I can call it from multiple threads simultaneously without synchronization, substitute its call with its return value in reasoning about the program (referential transparency), and compose it freely in LINQ pipelines without worrying about execution order. Pure functions belong at the leaf nodes of a pipeline; side effects—database writes, logging, time reads—are isolated at the boundary where they are unavoidable.

---

## Q9. What is immutability, and why is it important in functional and concurrent programming?

**Concepts**
- Immutability: state cannot change after construction
- Thread safety without locks for immutable objects
- Predictable function composition with unchanging inputs
- `record` and `with` expressions for non-destructive mutation
- Eliminating shared-state race conditions

**Answer**

Immutability means an object's state cannot change after construction—every "mutation" produces a new object rather than modifying the original. Immutable objects are inherently thread-safe because no thread can corrupt another's view of the data, which eliminates entire classes of race conditions without locks. In functional composition, passing immutable values through a pipeline guarantees that each step receives undisturbed input, making the behavior of each function predictable regardless of execution order or scheduler interleaving. In C# I express immutability with `readonly` fields, `init`-only properties, and `record` types whose `with` expressions create modified copies—`var updated = original with { Price = newPrice }` returns a new record without mutating `original`.

---

## Q10. How can immutability be achieved in C# (`readonly`, `record`, avoiding mutable captures)?

**Concepts**
- `readonly` fields settable only in constructor or initializer
- `init`-only properties for object initialization expressions
- `record` and `record struct` with structural equality
- `with` expressions for non-destructive copy
- `static` lambda to enforce no mutable capture at compile time

**Answer**

I achieve immutability through multiple mechanisms. `readonly` fields can be set only in constructors or field initializers, preventing reassignment afterward. `init`-only properties (C# 9+) allow setting during object initialization but block later assignment. `record` types provide structural equality and generate `with` expressions for non-destructive copies—so callers get a new instance with one property changed rather than mutating the original. For lambdas, I avoid capturing mutable outer variables when the lambda outlives the enclosing scope: I copy required values to locals and capture those, or I use `static` lambdas to enforce at compile time that no mutable state is captured. These mechanisms compose: a `record` type with `readonly` fields and a `static` lambda that processes it gives me end-to-end immutability from data through transformation.

---

## Q11. How do you avoid side effects when passing lambdas to APIs that store or invoke them later?

**Concepts**
- Mutable captured variable as shared communication channel
- Immutable snapshot capture via local copy before lambda
- Capturing factory or `IServiceScopeFactory` for live state
- `static` lambda keyword enforces no capture at compile time
- Documenting snapshot vs live-read semantics on stored delegates

**Answer**

When a lambda is stored and invoked later, any mutable outer variable it captures becomes a shared channel between the lambda and the code running between creation and invocation—this is the root of most closure bugs. I avoid side effects by capturing only immutable values: I copy primitives and `record` values to locals before the lambda so the closure holds a snapshot that is independent of subsequent changes. When the lambda must read live state, I capture a factory or `IServiceScopeFactory` and resolve fresh dependencies per invocation rather than capturing a stale resolved instance. I mark lambdas as `static` when they should not capture anything, and I document clearly whether a stored delegate takes a snapshot or reads live state so callers reason correctly about deferred execution.

---

## Q12. When should you copy loop values to a local inside the loop before capturing (`var copy = item`)?

**Concepts**
- Per-iteration local creates distinct display class field per iteration
- `for` loop single-slot variable always needs copy for deferred lambdas
- `foreach` C# 5+ per-iteration variable (copy usually redundant)
- Custom enumerator reusing struct `Current` as exception requiring copy
- Copy pattern applies identically to LINQ, `Task.Run`, and timer callbacks

**Answer**

I copy a loop value to a local—`var copy = item;`—whenever I create a lambda or anonymous method inside a loop and that lambda is invoked after the loop's current iteration ends. This applies to `for` loops unconditionally, since the loop variable is a single shared slot. For `foreach` loops in C# 5+, the compiler creates a per-iteration variable, so copying is usually redundant—but I still copy when iterating over a custom enumerator that reuses a single struct `Current`, or when the loop variable is a reference type whose referenced object mutates between loop and invocation. The copy works because each iteration's local results in a distinct display class field, giving each lambda its own independent snapshot.

---

## Q13. How do local functions compare to lambdas regarding capture and allocation behavior?

**Concepts**
- `static` local function: stack allocation when no capture, enforced no-capture
- Non-static capturing local function: same display class as lambda
- Lambda always allocates delegate instance; local function may not
- Local function supports recursion and named stack frames
- `static` modifier on local function makes zero-allocation verifiable

**Answer**

Local functions declared without capturing any outer variables are compiled as direct method calls—no display class, no delegate allocation. When they do capture, the compiler lifts them to a display class the same way it does for lambdas, incurring heap allocation. The `static` modifier on a local function prevents any capture (compile-time error if capture is attempted), making the zero-allocation guarantee explicit and auditable at code review. Lambdas always produce at least a delegate instance and, when capturing, a display class; there is no equivalent `static` enforcement on a lambda that produces zero allocation. Local functions also support recursion naturally, appear in stack traces with readable names, and can be declared with `async`—making them preferable to lambdas for deferred work that benefits from clear stack frames and explicit capture control.

---

## Q14. Closure captures the variable, not the value — Loop lambda prints `3, 3, 3`, not `0, 1, 2`.

**Concepts**
- Variable capture as reference to storage location
- Value at creation vs value at invocation distinction
- `for` loop single field, all lambdas share it
- Inner local copy as the canonical fix
- Deferred invocation after loop completion

**Answer**

A closure captures the variable itself—a reference to the storage location—not the value stored in that location at creation time. When I write `for (int i = 0; i < 3; i++) { actions.Add(() => Console.WriteLine(i)); }` and invoke all three actions after the loop, every action reads `i` from the single shared field, which holds `3` after the loop exits. Printing `3, 3, 3` rather than `0, 1, 2` is the expected behavior given capture-by-reference semantics—it is not a bug in the runtime, it is a misunderstanding of what was captured. The fix is `int copy = i;` inside the loop body before the lambda, so each action closes over a distinct field initialized to the loop's value at that iteration.

---

## Q15. Same trap in LINQ and tasks — Capturing loop variables inside `.Where()` / `Task.Run` produces identical bugs.

**Concepts**
- Deferred LINQ query re-evaluated with captured loop variable
- `Task.Run` callback executes after loop completes
- `foreach` C# 5+ creates per-iteration variable (avoids the bug)
- Inner copy pattern identical across LINQ, `Task.Run`, and timers
- Deferred execution as the common thread across all three contexts

**Answer**

The same capture mechanics apply in `Task.Run` callbacks and LINQ deferred queries built inside loops. `tasks.Add(Task.Run(() => Process(i)))` stores a delegate that reads `i` when the task runs, not when `Add` is called—if the loop finishes before any task starts, all tasks read the final `i`. Similarly, building a list of deferred predicates inside a loop with `queries.Add(db.Orders.Where(o => o.Region == region))` produces queries that all filter on the final `region` value. The fix is identical in both contexts: copy the loop variable to a local before the lambda, or use `foreach` in C# 5+ which creates per-iteration variables automatically. The pattern extends uniformly to `Timer` callbacks, `ThreadPool.QueueUserWorkItem`, and any deferred execution mechanism.

---

## Q16. Multicast delegate short-circuit on exception — Later subscribers may not run if an early one throws.

**Concepts**
- Sequential multicast invocation
- First-throwing subscriber aborts remaining targets
- `GetInvocationList()` for isolated per-handler invocation
- Per-handler try/catch pattern
- Independently-owned subscribers must not fail together

**Answer**

The CLR invokes multicast delegate targets sequentially, and if one throws an unhandled exception, the exception propagates to the call site immediately—all subsequent targets in the invocation list are skipped. Since independently-owned audit handlers, logging subscribers, or UI updaters should not fail together, I iterate `GetInvocationList()`, cast each element to the delegate type, and invoke each inside a try/catch block, logging failures without aborting the remaining subscribers. This pattern should be standard for any notification delegate where subscribers are independently owned and not expected to coordinate failure handling.

---

## Q17. Extension method not in scope — Missing `using` for the static class namespace.

**Concepts**
- Extension discovery requires namespace `using`, not just assembly reference
- `using static` enables direct static calls but not instance-style extension binding
- `GlobalUsings.cs` for project-wide extension namespace imports
- Explicit static call as always-available fallback
- CS1061 as the typical compile error for undiscovered extensions

**Answer**

Referencing an assembly is not enough for extension method discovery—the compiler also requires a `using` directive for the namespace of the static class that declares the extension. Without it, `value.ToDisplayLabel()` fails with CS1061 even if the extension class is in a referenced assembly. `using static Acme.Common.Extensions.StringExtensions` imports static members for direct calls but does not enable instance-style extension binding. I add `using Acme.Common.Extensions;` per file or globally in `GlobalUsings.cs`. As a fallback I can always call the extension as a static method—`StringExtensions.ToDisplayLabel(value)`—without importing the namespace.

---

## Q18. Instance method wins over extension — An instance method hides the extension; you cannot "override" with an extension.

**Concepts**
- Instance method resolution priority over extensions
- Extension never overrides instance method
- Extension invisible when instance method signature matches
- Library update adding instance method can silently hide extension
- Explicit static call to force extension invocation

**Answer**

When both an instance method and an extension method have the same name and a compatible signature, the compiler always chooses the instance method—extensions are only considered when no applicable instance method exists. This means I cannot use an extension to "override" or "patch" instance behavior on a type I own; the extension is invisible when the instance method matches. For types I do not own, extensions add behavior without risk of conflict with existing instance methods as long as names are chosen carefully. If an extension is silently ignored after a library update, I check whether the type has acquired an instance method with the same signature, which can happen when a library adds behavior that matches my extension.

---

## Q19. Shared captured storage — Multiple lambdas share one slot for the same outer variable.

**Concepts**
- Single display class per scope shared by all lambdas in that scope
- Mutual visibility of mutations across lambdas sharing one field
- Explicit per-closure state object to decouple
- `static` lambda to prevent shared capture entirely
- Per-iteration copy to create independent fields

**Answer**

All lambdas created within the same scope that capture the same outer variable share one display class instance and one field slot. If two lambdas both capture `int count` from the same enclosing method, mutations by one are immediately visible to the other because they both read and write the same field. This is intentional for accumulators but dangerous for independent concurrent workers that must not share state. I decouple closures that should not share state by creating per-closure state objects (`var state = new Counter()` before each lambda), or by using `static` lambdas with explicit parameters when closures must not communicate through shared storage at all.

---

## Q20. Target-typed lambda ambiguity — Without a clear target type, lambda expressions may fail to compile.

**Concepts**
- Target-typed lambda natural type inference from context (C# 10+)
- Ambiguous overload resolution when multiple delegate types match
- Explicit `Func`/`Action` annotation to resolve ambiguity
- Cast at call site to guide overload resolution
- `var` with annotated parameter types for unambiguous inference

**Answer**

In C# 10+, lambdas have a natural type inferred from their parameters and body—`var f = (int x) => x * 2;` infers `Func<int, int>`. However, when a lambda appears as an argument to an overloaded method where multiple overloads accept different delegate types, the compiler may report an ambiguity error because it cannot choose between them without more context. I resolve this by explicitly annotating the variable—`Func<int, int> f = x => x * 2;`—or by casting the lambda at the call site—`((Func<int, int>)(x => x * 2))`—to guide overload resolution unambiguously.

---

## Q21. Expression tree vs delegate — Expression-tree lambdas cannot contain many C# constructs that delegate lambdas allow.

**Concepts**
- Expression tree compilation (no IL, tree nodes emitted)
- Delegate lambda compiles to IL directly
- Expression tree restrictions (no block body, async, ref/out, loops)
- LINQ provider traversal of expression nodes for SQL translation
- `Compile()` to obtain executable delegate from expression tree

**Answer**

When a lambda is assigned to `Expression<TDelegate>`, the compiler emits expression-tree construction code rather than IL for the lambda body—this makes the tree inspectable at runtime so LINQ providers can translate predicates to SQL. The restriction is that expression-tree lambdas must be single expressions without block bodies, `if` statements, loops, `async`/`await`, `ref`/`out` parameters, or pointer operations. A delegate lambda compiled to IL has none of these restrictions. When I need both inspectability and complex logic, I split the predicate: an expression tree for the filterable parts (which translate to a SQL `WHERE` clause) and a delegate for any remaining in-memory logic after `.AsEnumerable()`, so the expensive filtering stays in the database.

---

## Q22. Capturing `this` implicitly — Instance lambdas capture `this`, extending object lifetime.

**Concepts**
- Implicit `this` capture when lambda accesses instance member
- Long-lived publisher holds GC root to subscribing instance
- Stored handler in field required for `-=` unsubscription
- Dispose pattern for event unsubscription
- Minimal capture: close over data, not `this`

**Answer**

Any lambda that references an instance method or field implicitly captures `this`—the compiler inserts `this` as a captured variable in the display class. When such a lambda is subscribed to a long-lived event or stored in a static collection, the publisher holds a GC root into the entire object, preventing garbage collection of the subscriber even after it logically goes away. The fix is to unsubscribe the handler in `Dispose`, which requires storing the lambda in a field so the same delegate instance can be removed with `-=`. Alternatively, I refactor the handler to avoid capturing `this` by passing only the data it needs as closed-over locals—closing over a string ID and a channel reference rather than the entire service instance reduces the retained object graph significantly.

---

## Q23. Extension on null reference — Extension methods can be called on null receivers; may throw inside the method.

**Concepts**
- Null receiver passed as first static argument (no call-site NRE)
- Exception originates inside method body on null dereference
- Difference from instance method dispatch (NRE before entering method)
- `this string?` annotation for null-safe extension
- Nullable reference type flow analysis guiding callers

**Answer**

Because the compiler rewrites `value.ToDisplayLabel()` to `StringExtensions.ToDisplayLabel(value)`, calling an extension method on a null reference does not throw `NullReferenceException` at the call site—null is simply passed as the first argument. The exception occurs inside the method body when the argument is dereferenced without a null check. This differs from instance method dispatch, where the runtime throws immediately at the call site if the receiver is null. I annotate the `this` parameter as `string?` in extensions designed to handle null gracefully, and I use `string` (non-nullable) when the method legitimately requires a non-null input—enabling nullable flow analysis to warn callers who pass a `string?` without a prior null check.
