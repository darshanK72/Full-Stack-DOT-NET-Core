# C# Lambda Expressions — Interview Q&A

---


## Table of Contents

1. [Q1. What is a lambda expression in C# and what does the `=>` operator mean?](#q1-what-is-a-lambda-expression-in-c-and-what-does-the-operator-mean)
2. [Q2. What is the difference between an expression lambda and a statement lambda?](#q2-what-is-the-difference-between-an-expression-lambda-and-a-statement-lambda)
3. [Q3. What are the valid parameter list forms for a lambda, and when must you add parentheses?](#q3-what-are-the-valid-parameter-list-forms-for-a-lambda-and-when-must-you-add-parentheses)
4. [Q4. How does the compiler perform type inference for a lambda, and what is "target typing"?](#q4-how-does-the-compiler-perform-type-inference-for-a-lambda-and-what-is-target-typing)
5. [Q5. How do you assign a lambda to a custom delegate type, and how does that differ from using `Func` or `Action`?](#q5-how-do-you-assign-a-lambda-to-a-custom-delegate-type-and-how-does-that-differ-from-using-func-or-action)
6. [Q6. What is an expression tree, and how does a lambda become one?](#q6-what-is-an-expression-tree-and-how-does-a-lambda-become-one)
7. [Q7. What are closures and captured variables in the context of lambda expressions?](#q7-what-are-closures-and-captured-variables-in-the-context-of-lambda-expressions)
8. [Q8. How does a lambda relate to the `delegate` keyword and anonymous methods?](#q8-how-does-a-lambda-relate-to-the-delegate-keyword-and-anonymous-methods)
9. [Q9. How do lambdas work with LINQ operators such as `Where`, `Select`, and `OrderBy`?](#q9-how-do-lambdas-work-with-linq-operators-such-as-where-select-and-orderby)
10. [Q10. What are discards (`_`) in lambda parameter lists, and when should you use them?](#q10-what-are-discards-in-lambda-parameter-lists-and-when-should-you-use-them)
11. [Q11. What is the natural type of a lambda in C# 10, and why does it matter?](#q11-what-is-the-natural-type-of-a-lambda-in-c-10-and-why-does-it-matter)
12. [Q12. What is a static lambda, and what problem does it solve?](#q12-what-is-a-static-lambda-and-what-problem-does-it-solve)
13. [Q13. What happens when a lambda is passed to an overloaded method that has both a `Func<T,bool>` and an `Expression<Func<T,bool>>` overload?](#q13-what-happens-when-a-lambda-is-passed-to-an-overloaded-method-that-has-both-a-functbool-and-an-expressionfunctbool-overload)
14. [Q14. How do lambda expressions interact with `async` and `await`?](#q14-how-do-lambda-expressions-interact-with-async-and-await)
15. [Q15. What compile errors are unique to lambda expressions and what causes each one?](#q15-what-compile-errors-are-unique-to-lambda-expressions-and-what-causes-each-one)
16. [Q16. How does a lambda differ from a local function, and when should you choose each?](#q16-how-does-a-lambda-differ-from-a-local-function-and-when-should-you-choose-each)
17. [Q17. What does method group conversion mean, and how does it compare to a wrapping lambda?](#q17-what-does-method-group-conversion-mean-and-how-does-it-compare-to-a-wrapping-lambda)
18. [Q18. When should you prefer a named method over a lambda, despite lambda being more concise?](#q18-when-should-you-prefer-a-named-method-over-a-lambda-despite-lambda-being-more-concise)

---
## Foundation Questions

---

## Q1. What is a lambda expression in C# and what does the `=>` operator mean?

**Concepts**
- anonymous inline function
- `=>` "goes to" operator
- delegate assignment target
- compiler-generated private method
- expression body vs block body distinction

**Answer**

A lambda expression is an anonymous function written directly at the point of use rather than as a named method elsewhere in the class. The `=>` token, read aloud as "goes to," separates the parameter list on the left from the function body on the right. When the compiler sees `x => x * 2m`, it generates a private static or instance method behind the scenes and creates a delegate instance pointing to it. The lambda itself has no standalone identity — it must be assigned to a delegate type (a named delegate, `Func<T,TResult>`, `Action<T>`, or `Predicate<T>`) or passed directly to a parameter whose type is already known so the compiler can infer the target. This design lets you express small, single-purpose pieces of behavior inline — a filter, a transformation, a comparison — without polluting the class with one-off named methods that are never called from more than one place.

---

## Q2. What is the difference between an expression lambda and a statement lambda?

**Concepts**
- expression lambda — single expression, implicit return
- statement lambda — braces, explicit `return`
- block body for multi-statement logic
- readability trade-off
- compile error CS0834

**Answer**

An expression lambda has a single expression on the right side of `=>` and no curly braces. The compiler treats that expression as the return value, so you never write the `return` keyword: `x => x * 1.08m`. A statement lambda wraps one or more statements inside `{ }` and, for non-void delegates, must include at least one `return` statement: `x => { if (x < 0m) return 0m; return x * 1.08m; }`. The practical rule is to prefer expression form whenever the logic fits comfortably on one line, and switch to block form the moment you need a local variable, a conditional, an early return, or a loop. Trying to shoehorn a block construct into expression form — placing `{ }` inside an expression lambda — causes CS0834 at compile time. For void-returning delegates, a block body can use a bare `return;` to exit early, while expression form works only when the single expression itself is `void`-compatible, such as a method call with no return value.

---

## Q3. What are the valid parameter list forms for a lambda, and when must you add parentheses?

**Concepts**
- zero-parameter form `() =>`
- single-parameter parentheses optional
- multi-parameter parentheses required
- explicit type annotation
- discard parameter `_`

**Answer**

Parentheses around the parameter list are optional when the lambda takes exactly one parameter and the compiler can infer its type: `x => x * 2m` and `(x) => x * 2m` are identical. Parentheses become mandatory in three situations: zero parameters (`() => 0.0825m`), two or more parameters (`(qty, price) => qty * price`), and whenever you supply explicit type annotations because inference fails (`(int qty, decimal price) => qty * price`). Within a multi-parameter list you can use `_` as a discard to signal that a particular slot is intentionally unused — `(_, price) => price * 2m` — which communicates intent to readers without introducing a named variable that is never referenced. Starting with C# 9 you can also use `_` as the sole parameter name for a single-argument lambda: `_ => Console.WriteLine("received")`. Each discard is local to the lambda; they do not interact with outer discard conventions.

---

## Q4. How does the compiler perform type inference for a lambda, and what is "target typing"?

**Concepts**
- target-typed lambda (C# 10+)
- delegate type from assignment context
- parameter type inference from delegate signature
- natural type of a lambda (C# 10)
- CS8917 — no target type error

**Answer**

The compiler resolves a lambda's parameter types and return type by examining the delegate type expected at the point of use — a process called target typing. When you write `Func<decimal, decimal> t = x => x * 2m;`, the compiler sees that `Func<decimal, decimal>` requires one `decimal` input and a `decimal` output, so it infers that `x` is `decimal` and that the expression must also produce a `decimal`. The same inference applies when the lambda is passed directly to a method parameter: `list.Where(p => p > 20m)` infers `p` is the element type because `Where` expects `Func<T, bool>`. Before C# 10, a standalone lambda with no surrounding context produced CS8917 because there was no target type. C# 10 introduced a "natural type" for lambdas: the compiler synthesises an internal delegate type so you can write `var f = (decimal x) => x * 2m;` — the explicit parameter annotation gives the compiler enough information to construct the natural type and assign it to `var`. If parameter types are ambiguous, the compiler still cannot proceed without a target type or explicit annotations.

---

## Q5. How do you assign a lambda to a custom delegate type, and how does that differ from using `Func` or `Action`?

**Concepts**
- custom delegate declaration
- signature compatibility — parameter and return types must match
- `Func<T,TResult>` built-in generic delegate
- `Action<T>` void-returning delegate
- `Predicate<T>` equivalent to `Func<T, bool>`

**Answer**

A custom delegate is declared with a specific name, parameter list, and return type: `public delegate decimal PriceTransform(decimal unitPrice);`. Any lambda whose signature matches — one `decimal` in, one `decimal` out — can be assigned to that type: `PriceTransform markup = price => price * 1.10m;`. The custom name gives the delegate semantic meaning in documentation and error messages, which is valuable for domain-rich APIs. Built-in generic delegates like `Func<decimal, decimal>` are structurally identical but anonymous at the type level; they are preferred when you do not need a domain-specific name and want to avoid declaring boilerplate. Because delegates are not structurally equivalent in C# — two distinct delegate types with identical signatures are not assignable to each other — a lambda that compiles against `PriceTransform` cannot be directly assigned to a variable of type `Func<decimal, decimal>` and vice versa without an explicit cast or re-assignment. In practice, most modern APIs standardise on `Func`, `Action`, and `Predicate`, reserving custom delegates for cases where the name meaningfully narrows the intent.

---

## Q6. What is an expression tree, and how does a lambda become one?

**Concepts**
- `Expression<TDelegate>` wrapper type
- abstract syntax tree at runtime
- LINQ-to-SQL / EF Core query translation
- compile-time restriction — no statement lambdas
- `Compile()` to produce a live delegate

**Answer**

When you assign a lambda to `Expression<Func<T, TResult>>` instead of `Func<T, TResult>`, the compiler does not emit IL for the lambda body. Instead, it emits code that constructs an in-memory abstract syntax tree — an `Expression<TDelegate>` object — representing the lambda's structure as data. This tree can be inspected, transformed, or serialised at runtime. Entity Framework Core and other LINQ providers exploit this by walking the tree and translating it into SQL rather than executing it as C# code. The assignment `Expression<Func<Product, bool>> pred = p => p.Price > 100m;` compiles; but statement lambdas — those using `{ }` blocks — cannot become expression trees because a block's structure is too rich to represent in the AST model the framework expects. If you need to execute the expression locally, call `pred.Compile()` to produce a regular `Func<Product, bool>` delegate. The practical interview answer is: use `Func` when you are executing in memory, use `Expression<Func>` when a provider must translate the query.

---

## Q7. What are closures and captured variables in the context of lambda expressions?

**Concepts**
- closure — lambda that references outer scope
- captured variable — outer local hoisted by compiler
- compiler-generated display class
- lifetime extension of captured variable
- shared mutable state between invocations

**Answer**

A closure is a lambda that references a variable declared in its enclosing scope — a local variable, a method parameter, or even `this`. When the compiler encounters such a reference, it lifts the variable out of the stack frame into a compiler-generated "display class" (a heap-allocated object), so the lambda delegate can access it after the declaring method has returned. The key behaviour is that the closure captures the variable itself, not a copy of its value at the moment the lambda is written. If `discountRate` is 0.15m when the lambda is created but you later set it to 0.25m, an invocation after the assignment will use 0.25m. This shared-reference semantics is intentional and enables elegant patterns — a factory method that produces a lambda pre-loaded with a parameter — but it is also the source of the classic loop capture bug, where all loop iterations end up sharing a single captured loop variable rather than each having their own copy. Understanding closures means understanding that the lambda holds a reference to the variable's storage location, not a snapshot of its value.

---

## Q8. How does a lambda relate to the `delegate` keyword and anonymous methods?

**Concepts**
- anonymous method syntax (`delegate(params) { }`)
- lambda as modern replacement
- method group conversion
- readability improvement
- remaining differences

**Answer**

Anonymous methods, introduced in C# 2.0, let you write inline delegate code without a named method: `button.Click += delegate(object s, EventArgs e) { Handle(s); };`. Lambda expressions arrived in C# 3.0 and replaced this syntax for all new code. The lambda equivalent is `button.Click += (s, e) => Handle(s);` — shorter because parameter types are inferred from the event delegate's signature and the `delegate` keyword is replaced by the `=>` notation. Both forms support closures; both compile to the same underlying delegate mechanics. The one functional difference that occasionally matters is that an anonymous method written as `delegate { }` — with no parameter list at all — can be assigned to any delegate type regardless of its parameter count, making it a convenient null-object-pattern no-op for events. A lambda without parameters must have `()` on the left and can only be assigned to a zero-parameter delegate. In modern C# you will essentially never write a new anonymous method; lambdas are the idiomatic form, and method group conversions cover the case where a compatible named method already exists.

---

## Q9. How do lambdas work with LINQ operators such as `Where`, `Select`, and `OrderBy`?

**Concepts**
- LINQ extension methods on `IEnumerable<T>`
- predicate lambda `Func<T, bool>` in `Where`
- projection lambda `Func<T, TResult>` in `Select`
- key-selector lambda in `OrderBy`
- deferred execution and lazy evaluation

**Answer**

LINQ's standard query operators are extension methods on `IEnumerable<T>` (and `IQueryable<T>`) that accept lambdas for their core logic. `Where` takes a `Func<T, bool>` predicate and returns only elements for which it returns `true`; `Select` takes a `Func<T, TResult>` projection and returns a transformed sequence; `OrderBy` takes a `Func<T, TKey>` key selector and sorts ascending by that key. A typical pipeline looks like: `products.Where(p => p.Price > 20m).OrderBy(p => p.Name).Select(p => p.Price * 0.9m)`. These calls do not execute immediately — each returns an `IEnumerable<T>` wrapping the previous, deferring evaluation until something iterates the sequence, such as `ToList()`, `foreach`, or `Count()`. This deferred execution means that if a captured variable changes between pipeline definition and iteration, the lambda sees the new value — a property that can be surprising but is fundamental to how LINQ achieves composability. On `IQueryable<T>` the lambdas must be expressible as expression trees, so LINQ providers can translate them into the target query language rather than pulling all data into memory first.

---

## Q10. What are discards (`_`) in lambda parameter lists, and when should you use them?

**Concepts**
- discard identifier `_`
- communicating unused parameters
- multiple discards in one lambda
- difference from a real parameter named `_`
- C# 9+ single-discard without parentheses

**Answer**

A discard in a lambda parameter list is the `_` identifier used in place of a parameter name to signal that the value will not be read inside the lambda body. Writing `(_, e) => Handle(e)` for an event handler immediately tells the reviewer that the sender is irrelevant to this handler. In C# 9 and later you can use a single `_` without parentheses when the delegate takes exactly one parameter: `_ => DoWork()`. For delegates with two or more parameters, all discards still need the full parenthesised list: `(_, _) => Constant()`. Discards reduce noise — you avoid compiler warnings about unused variables and avoid inventing a dummy name like `sender` that is never referenced. One subtlety: in pre-C# 9 code, `_` was treated as a regular identifier, so if you declared an outer variable named `_` before the lambda, the discard inside the lambda would capture it rather than discard it. C# 9's "true discard" semantics resolved this ambiguity for lambda parameters.

---

## Q11. What is the natural type of a lambda in C# 10, and why does it matter?

**Concepts**
- natural type feature (C# 10)
- explicit parameter type annotation enables `var`
- inferred `Func` or `Action` delegate type
- method group natural type
- overload resolution with natural types

**Answer**

Before C# 10, every lambda required a surrounding context — a typed variable, a method parameter, or an explicit cast — to supply the delegate type, because lambdas had no type of their own. C# 10 gave lambdas a natural type: when you annotate the parameter types explicitly, the compiler synthesises a `Func` or `Action` delegate and allows you to use `var`: `var double = (decimal x) => x * 2m;` infers `Func<decimal, decimal>`. Return types can also be annotated explicitly: `var parse = decimal (string s) => decimal.Parse(s);`. This matters for two scenarios. First, it enables cleaner patterns with generic methods that infer type arguments from lambda parameters without requiring a typed intermediate variable. Second, it helps overload resolution when multiple overloads could match: the compiler can now rank candidates using the natural type of the lambda rather than failing with an ambiguity error. Method groups also gained a natural type in C# 10, following the same principle. The feature is largely additive — old code that provided explicit target types still works — but it removes a class of verbose intermediate variable declarations that previously existed only to guide the type inference algorithm.

---

## Q12. What is a static lambda, and what problem does it solve?

**Concepts**
- `static` modifier on a lambda (C# 9+)
- prevents accidental capture of `this` or outer locals
- allocation reduction on hot paths
- compile-time enforcement
- difference from static anonymous methods

**Answer**

A static lambda is written by placing the `static` keyword before the lambda: `static p => p * 1.08m`. The `static` modifier instructs the compiler to refuse any attempt to capture a non-static variable from the enclosing scope — neither outer locals nor `this`. This has two benefits. The first is correctness: marking a lambda static makes it impossible to accidentally close over a mutable variable, preventing the class of bugs where a shared lambda subtly reads state from the wrong execution context. The second is performance: a lambda that captures nothing can be cached as a single static delegate instance by the compiler, so each invocation does not allocate a new closure object. On high-throughput code paths — inner loops processing thousands of items per request — the difference between a delegate allocated per call and one cached permanently can be measurable. If you add a capture inside a `static` lambda the compiler emits an error immediately, making the restriction self-documenting in code review. The feature was introduced in C# 9 alongside the broader push to give developers explicit control over allocations in performance-sensitive paths.

---

## Q13. What happens when a lambda is passed to an overloaded method that has both a `Func<T,bool>` and an `Expression<Func<T,bool>>` overload?

**Concepts**
- overload resolution with delegate vs expression-tree overloads
- `IQueryable<T>.Where` vs `IEnumerable<T>.Where`
- compile-time ambiguity when both match
- implicit conversion to expression tree
- explicit cast or intermediate variable to disambiguate

**Answer**

The C# compiler converts a lambda to `Expression<Func<T, bool>>` only when the target type is explicitly an expression tree. When two overloads exist — one taking `Func<T, bool>` and one taking `Expression<Func<T, bool>>` — the compiler applies its overload resolution rules, which prefer the `Func` overload because a direct delegate conversion is considered a better conversion than a conversion to an expression tree wrapper. The most common encounter with this asymmetry is the `Where` operator: `IQueryable<T>.Where` takes `Expression<Func<T, bool>>` and causes the ORM to generate SQL, while `IEnumerable<T>.Where` takes `Func<T, bool>` and runs in memory. If you accidentally call `.AsEnumerable()` before `.Where(...)`, the ORM path is abandoned silently. When genuine ambiguity arises — your own API has both overloads — use an explicit intermediate variable typed to the target you intend, or cast the lambda: `(Expression<Func<Product, bool>>)(p => p.Price > 100m)`. This forces expression-tree conversion regardless of overload preference.

---

## Q14. How do lambda expressions interact with `async` and `await`?

**Concepts**
- `async` modifier on a lambda
- `Func<Task>` and `Func<Task<T>>` return types
- `async void` lambda — fire-and-forget danger
- `await` inside lambda body
- exception propagation differences

**Answer**

A lambda can be marked `async` the same way a method can: `async () => await FetchAsync()`. The return type becomes `Task` for a void-result async lambda assigned to `Func<Task>`, or `Task<T>` for one assigned to `Func<Task<T>>`. The critical danger is assigning an async lambda to `Action` or an `Action<T>` — the compiler accepts it as `async void`, which means exceptions thrown inside the lambda propagate through the thread's synchronisation context rather than being placed on a `Task`. The calling code cannot await the result, cannot catch exceptions with `try/catch`, and the process can crash with an unhandled exception in a background thread. The safe pattern is to declare the parameter type as `Func<Task>` or `Func<CancellationToken, Task>` whenever asynchronous behaviour is needed, ensuring the `Task` is both awaited by the caller and carries exceptions back through normal `await` propagation. Inside the lambda body, `await` behaves identically to inside a named `async` method — continuations, `ConfigureAwait`, cancellation tokens, and exception capture all work the same way.

---

## Q15. What compile errors are unique to lambda expressions and what causes each one?

**Concepts**
- CS0834 — statement in expression lambda
- CS0161 — not all code paths return
- CS1662 — lambda return type mismatch
- CS8917 — no target type
- CS8971 — static lambda captures outer variable

**Answer**

Five compiler errors appear specifically in lambda contexts. CS0834 fires when you place braces or statements inside what the compiler thinks is an expression lambda; the fix is to either remove the braces and collapse to a single expression, or keep the braces and ensure you have a full statement lambda. CS0161 fires on a block-body lambda assigned to a non-void delegate when the compiler can find a code path that falls through without reaching a `return` statement; add a final `return` covering the missed path. CS1662 fires when the expression or `return` statement inside the lambda produces a type that does not implicitly convert to the delegate's declared return type — the classic example is returning `2` (an `int` literal) when the delegate expects `decimal`; add the `m` suffix. CS8917 fires when a lambda appears in an expression where no surrounding context provides a target type — assign to a typed variable, pass to a typed parameter, or annotate the lambda's parameters explicitly. CS8971 fires when you use `static` on a lambda that then references an outer local or instance member; remove the capture or remove the `static` modifier.

---

## Q16. How does a lambda differ from a local function, and when should you choose each?

**Concepts**
- local function — named, inside method body
- lambda — anonymous, assigned to delegate variable
- recursion support
- iterator and unsafe modifiers on local functions
- allocation behaviour

**Answer**

A local function is a named method declared inside another method using standard method syntax. A lambda is an anonymous function assigned to a delegate variable. The distinction matters in three practical ways. First, local functions can be recursive by name — `int Fib(int n) => n <= 1 ? n : Fib(n - 1) + Fib(n + 2);` — while a lambda cannot call itself by name without a workaround such as assigning it to a variable first and referencing the variable (which requires the variable to be declared before the assignment, creating a forward-reference problem). Second, local functions can be iterators (`yield return`) and can carry `unsafe` or `extern` modifiers; lambdas cannot. Third, a non-capturing local function that the compiler can determine is never used as a delegate is emitted as a static method with zero allocation, while a non-capturing lambda that is cached by the compiler is still technically a delegate instance. For short, inline transformations passed to LINQ or a higher-order method, lambdas are idiomatic. For complex helper logic that needs a name, recursion, or iterator semantics, prefer a local function.

---

## Q17. What does method group conversion mean, and how does it compare to a wrapping lambda?

**Concepts**
- method group — reference to a named method
- delegate creation from method group
- performance: no extra call frame vs lambda wrapper
- overload resolution with method groups
- natural type of method groups (C# 10)

**Answer**

A method group conversion assigns a named method directly to a delegate variable without wrapping it in a lambda: `Func<decimal, decimal> round = Math.Round;`. The compiler resolves which overload of `Math.Round` matches the delegate's signature and produces a delegate pointing to that method. The equivalent lambda `x => Math.Round(x)` does the same thing but adds an extra call frame — the lambda's generated method body simply calls the target method — and allocates a separate delegate. Method group conversion skips that wrapper, so it is both more concise and slightly more efficient. The meaningful trade-off is that a method group is less flexible at the call site: you cannot partially apply arguments, rename parameters for readability, or add light transformation inline. Use a method group when the named method already does exactly what the delegate requires. Use a lambda when you need to adapt — reorder arguments, fold in a constant, or combine with a captured variable. Since C# 10, method groups also have a natural type, allowing `var f = Math.Round;` to compile when the overload set is unambiguous.

---

## Q18. When should you prefer a named method over a lambda, despite lambda being more concise?

**Concepts**
- reuse across call sites
- testability — named method is independently testable
- readability of complex logic
- stack trace clarity
- documentation and discoverability

**Answer**

A lambda's conciseness is a net gain when the logic is trivial, used in exactly one place, and its intent is obvious from context: `prices.Where(p => p > 0m)` needs no further explanation. The calculation tips toward a named method in four situations. First, reuse: if the same logic appears in more than one lambda across the codebase, extract it into a named method and convert those lambdas to method group conversions. Second, testability: a named method can be unit-tested directly, while testing logic buried inside a lambda requires invoking the larger method or class that contains it. Third, complexity: multi-branch logic, loops, or more than one level of nesting inside a lambda is almost always clearer as a named method with a descriptive verb phrase. Fourth, debugging: stack traces from named methods surface the method name; lambdas produce compiler-generated names like `<CalculateTotal>b__0_0`, which are harder to parse in production logs. The guiding principle is that a lambda earns its place when removing the name makes the code clearer, not just shorter.

---

## Gotchas — Lambda Expressions (Interview Traps)

---

#### Gotcha 1. Loop Variable Capture — All Lambdas See the Final Value of `i`

**Concepts**
- closure captures the variable reference, not the value at capture time
- `for` loop counter is a single shared variable across all iterations
- all lambdas created in the loop see the post-loop final value
- local copy inside the loop body creates an independent variable per iteration

**Answer**

The classic loop-capture trap is `for (int i = 0; i < n; i++) list.Add(() => i)` — when any stored lambda is later invoked, every one of them prints `n`, not the per-iteration value. This happens because all lambdas share the same captured variable `i`, which by execution time holds its final loop-exit value. The fix is to introduce a loop-local copy: `int copy = i; list.Add(() => copy);` — each iteration's `copy` is a distinct variable on its own display class, so each lambda is independent.

---

#### Gotcha 2. Closure Keeps the Captured Variable Alive Beyond Its Original Scope

**Concepts**
- display class promotes captured variable to the heap
- delegate holds a strong reference to the display class
- large objects or `IDisposable` resources captured unintentionally stay alive
- event subscription with a capturing lambda creates a GC root through the publisher

**Answer**

When a lambda captures a variable, the compiler lifts it into a heap-allocated display class. The display class lives as long as any delegate referencing it is reachable, which can be far longer than the method's stack frame. Capturing a large collection, an `HttpContext`, or a `DbContext` inside a lambda stored in a static field or a long-lived event subscription prevents the garbage collector from reclaiming those objects. Always ask: what does this lambda capture, and for how long will the delegate holding it live?

---

#### Gotcha 3. Static Lambdas (C# 9) Prevent Accidental Capture

**Concepts**
- `static` modifier forbids capture of `this`, locals, and instance members
- CS8971 compile error on any attempted capture
- allocation benefit: no closure object; delegate can be cached statically
- static fields and static methods remain accessible inside a static lambda

**Answer**

Adding the `static` keyword before a lambda — `static x => x * 2` — instructs the compiler to refuse any variable or instance-member capture. This is a compile-time guardrail, not a mere performance hint: if a later code change accidentally introduces a capture inside a static lambda, the compiler reports CS8971 immediately rather than silently introducing a closure allocation. For hot-path code where allocation matters, static lambdas give the runtime enough information to cache the delegate in a static slot, eliminating per-call allocation.

---

#### Gotcha 4. Lambda vs Method Group — Different Delegate Equality Semantics

**Concepts**
- method group conversion to the same method on the same instance produces equal delegates
- two lambda expressions are never equal even with identical bodies
- `-=` with a wrapping lambda fails silently; `-=` with a method group succeeds
- use a cached delegate or method group when reliable unsubscription is required

**Answer**

Two delegates created from the same method group — `service.Handler += obj.HandleEvent` followed by `service.Handler -= obj.HandleEvent` — compare as equal and unsubscription works correctly. Two delegates created from separately written lambda expressions, even with identical bodies, compare as unequal, and the `-=` is a silent no-op. When reliable unsubscription is required, prefer a cached delegate or a method group over an inline lambda.

---

#### Gotcha 5. Expression Trees vs Delegates — `Func<T>` Compiles to IL; `Expression<Func<T>>` Is an AST

**Concepts**
- `Func<T>` compiles the lambda body to IL executable code
- `Expression<Func<T>>` compiles the lambda body to an in-memory syntax tree; no IL for the body
- statement lambdas cannot be expression trees
- EF Core and other `IQueryable` providers require `Expression<Func<T,bool>>` for SQL translation

**Answer**

When you assign a lambda to `Func<T, bool>`, the compiler emits IL for the body and you get an executable delegate. When you assign the same lambda to `Expression<Func<T, bool>>`, the compiler emits code that constructs an abstract syntax tree object — the body is never compiled to IL directly. LINQ-to-database providers walk this tree and translate it to SQL; they cannot work with a compiled `Func` because there is no IL to inspect. Statement lambdas (block bodies with braces) cannot be converted to expression trees because the AST model has no representation for imperative statements.

---

#### Gotcha 6. Lambda in LINQ-to-SQL/EF Must Be Translatable to SQL

**Concepts**
- `IQueryable<T>` extension methods accept `Expression<Func<T,bool>>`
- lambdas calling C# methods the provider cannot translate throw at runtime
- calling `.AsEnumerable()` or `.ToList()` before `Where` switches to in-memory evaluation
- provider-specific functions (`EF.Functions.Like`) are safe; arbitrary C# methods are not

**Answer**

When LINQ executes against an `IQueryable` provider such as EF Core, every lambda inside `Where`, `Select`, and similar operators must be translatable to the target query language. Calling a custom C# method — `p => p.Name.ToTitleCase()` — compiles without error but throws an `InvalidOperationException` at runtime because EF has no SQL equivalent. The fix is either to pull the query result into memory first with `ToList()` and then apply the untranslatable filter in-memory, or to rewrite the predicate using only BCL methods and EF-specific functions that the provider knows how to translate.

---

#### Gotcha 7. `var` Cannot Infer a Lambda's Type Without an Explicit Target Type

**Concepts**
- lambdas have no standalone type before C# 10
- C# 10 natural type requires explicit parameter type annotations
- without a target type or annotations, compiler reports CS8917
- must assign to an explicit `Func<>`, `Action<>`, or custom delegate type

**Answer**

Before C# 10, a lambda expression had no type of its own and could not be assigned to `var`. The compiler requires a target delegate type — either from the variable declaration, a method parameter, or an explicit cast — to resolve parameter and return types. C# 10 introduced a natural type: if you annotate the parameters explicitly, the compiler synthesises a `Func` or `Action` delegate type and allows `var f = (int x) => x * 2;`. Without the annotation the compiler still reports CS8917, so the natural type feature is not a full substitute for an explicit delegate type on the left side.

---

#### Gotcha 8. Recursive Lambda — A Lambda Cannot Reference Itself by Name

**Concepts**
- lambda has no name to call recursively
- declare a variable first, then assign the lambda that closes over it
- forward-reference problem: variable must be declared before the lambda uses it
- local function is the idiomatic solution for recursion

**Answer**

A lambda expression has no name, so it cannot call itself directly the way a named method can. The workaround is to declare the variable first, then assign the lambda that references the variable: `Func<int,int> fib = null!; fib = n => n <= 1 ? n : fib(n-1) + fib(n-2);`. This works because the variable is in scope when the lambda body executes, even though it was null at the moment the lambda was written. The cleaner idiomatic solution is a local function, which supports direct recursion by name without the null-initialization ceremony.

---

#### Gotcha 9. Lambdas Cannot Capture `ref` or `out` Variables

**Concepts**
- `ref`/`out` variables are aliases to stack locations that cannot be heap-promoted
- captured variables must outlive the declaring method's stack frame
- combining heap promotion with a stack alias is unsafe
- CS1628 compile error when attempting to capture a `ref`/`out` variable

**Answer**

The compiler prohibits capturing a variable declared with `ref` or `out` inside a lambda because those variables are aliases to stack locations that the runtime cannot safely promote to the heap. If the stack frame is released while the delegate is still alive, the captured `ref` would point to garbage memory. The compiler catches this with CS1628 at compile time. The workaround is to copy the `ref`/`out` value into a regular local variable before the lambda, capturing the copy instead of the original ref-typed variable.

---

#### Gotcha 10. Lambda Allocation — Every Capturing Lambda Creates a Closure Object

**Concepts**
- non-capturing lambda: no allocation (can be cached as a static delegate)
- capturing lambda: allocates a display class object on each enclosing method call
- allocations on hot paths accumulate GC pressure
- static lambda or pre-allocated delegate field avoids repeated allocation

**Answer**

A lambda that captures no outer variables can be cached by the compiler as a single static delegate instance, so no heap allocation occurs at the call site. A lambda that captures even one variable creates a new display class object on every invocation of the enclosing method, because each call needs independent captured variable storage. In tight loops or request-handling hot paths, this allocation can add measurable GC pressure. The mitigation is to use static lambdas where possible, move capturing lambdas to instance fields so the closure object is allocated once per object lifetime, or use local functions which the compiler can sometimes implement without a display class.

---

## Real-World Scenarios

---

## S1. Build a LINQ pipeline that filters, sorts, and projects a product list using lambdas.

**Concepts**
- `Where` predicate lambda
- `OrderBy` key-selector lambda
- `Select` projection lambda
- `ToList` to materialise
- deferred execution and single-pass evaluation

**Answer**

A real-world catalog query chains `Where`, `OrderBy`, and `Select` with lambdas, each expressing one concern:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

var products = new List<(string Name, decimal Price, bool InStock)>
{
    ("Widget A", 49.99m, true),
    ("Widget B", 8.50m,  false),
    ("Gadget X", 129.00m, true),
    ("Gadget Y", 24.99m, true),
    ("Part Z",   5.00m,  true),
};

var report = products
    .Where(p => p.InStock && p.Price >= 10m)
    .OrderBy(p => p.Price)
    .Select(p => new { p.Name, Discounted = Math.Round(p.Price * 0.90m, 2) })
    .ToList();

foreach (var item in report)
    Console.WriteLine($"{item.Name}: {item.Discounted:C}");
```

None of the three lambdas executes until `ToList()` forces evaluation. Each lambda receives inferred types from the element type of the sequence it operates on — `p` is a named tuple in `Where` and `OrderBy`, and the anonymous type in `Select`. The pipeline is readable because each lambda expresses a single, named concern at the call site. In an interview, emphasise that `OrderBy` always precedes `Select` in a typical pipeline because you sort the original shape before projecting to a potentially smaller type, and that adding `.AsNoTracking()` or moving to `IQueryable<T>` would let EF Core translate these identical lambdas to SQL without any code change — provided the lambdas are kept as expression-compatible single expressions.

---

## S2. Wire up an event handler with a lambda, and explain when to use a lambda vs a named method.

**Concepts**
- event `+=` with lambda
- closure captures enclosing state
- unsubscription impossible with anonymous lambda
- named method enables `-=` unsubscription
- `IDisposable` pattern for event cleanup

**Answer**

Attaching a lambda to an event is idiomatic for short-lived, UI-style bindings:

```csharp
using System;

var button = new Button();
string prefix = "Sale";

button.Click += (sender, e) =>
{
    Console.WriteLine($"{prefix}: button clicked at {DateTime.UtcNow}");
};

button.SimulateClick();

public class Button
{
    public event EventHandler? Click;
    public void SimulateClick() => Click?.Invoke(this, EventArgs.Empty);
}
```

The lambda captures `prefix` from the enclosing scope, making it easy to include contextual state without threading it through parameters. The critical limitation is that you cannot unsubscribe a lambda event handler because the `+=` expression creates a new delegate instance each time, and you hold no reference to it for a later `-=`. If the handler must be removed — because the subscriber's lifetime is shorter than the publisher's — store the lambda in a field or use a named method: `button.Click += HandleClick;` then `button.Click -= HandleClick;`. In production code, long-lived publishers (services, singletons) that receive lambda subscriptions from short-lived objects (view models, request handlers) are a common source of memory leaks because the publisher holds the delegate, which holds the closure, which holds `this`. The pattern for safe cleanup is to implement `IDisposable` on the subscriber and unsubscribe in `Dispose`.

---

## S3. Implement a predicate factory method that returns a configured lambda.

**Concepts**
- higher-order function returning `Func<T, bool>`
- closure over factory parameter
- reusable predicate composition
- `Predicate<T>` vs `Func<T, bool>` in APIs
- factory isolation prevents shared-mutable-state bugs

**Answer**

A predicate factory encapsulates a rule as a returnable lambda, keeping configuration at the factory boundary:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

public static class PriceFilters
{
    public static Func<decimal, bool> InRange(decimal min, decimal max)
    {
        if (min > max) throw new ArgumentException("min must be <= max");
        return price => price >= min && price <= max;
    }

    public static Func<decimal, bool> AboveThreshold(decimal threshold) =>
        price => price > threshold;
}

var prices = new[] { 5m, 15m, 25m, 75m, 150m };

var midRange = PriceFilters.InRange(10m, 100m);
var results  = prices.Where(midRange).ToList();

Console.WriteLine(string.Join(", ", results.Select(p => p.ToString("C"))));
```

Each call to `InRange` creates a fresh closure capturing its own `min` and `max`, so two predicates built with different arguments are completely independent. This pattern is more testable than inline lambdas — `PriceFilters.InRange` is a pure function whose output can be exercised in a unit test against a known set of prices. It also composes: `prices.Where(PriceFilters.InRange(10m, 100m)).Where(PriceFilters.AboveThreshold(20m))` chains two independently-built predicates. When the factory is used in an EF Core context, you would change the return type to `Expression<Func<decimal, bool>>` so the predicate can be translated to SQL; the call-site code would look identical.

---

## S4. Apply a lambda-based strategy pattern to select a discount rule at runtime.

**Concepts**
- strategy pattern with `Func<decimal, decimal>`
- dictionary or enum dispatch to lambda
- open/closed principle via delegate storage
- runtime strategy selection
- avoids switch/if-else growth

**Answer**

Storing discount strategies as lambdas in a dictionary implements the strategy pattern without a class hierarchy:

```csharp
using System;
using System.Collections.Generic;

public enum TierCode { Standard, Premium, Wholesale }

public static class DiscountStrategies
{
    private static readonly Dictionary<TierCode, Func<decimal, decimal>> _rules = new()
    {
        [TierCode.Standard]  = price => price,
        [TierCode.Premium]   = price => price * 0.90m,
        [TierCode.Wholesale] = price => price > 100m ? price * 0.75m : price * 0.80m,
    };

    public static decimal Apply(TierCode tier, decimal price)
    {
        if (!_rules.TryGetValue(tier, out var rule))
            throw new InvalidOperationException($"No rule for tier {tier}");
        return rule(price);
    }
}

Console.WriteLine(DiscountStrategies.Apply(TierCode.Premium,   199.00m)); // 179.10
Console.WriteLine(DiscountStrategies.Apply(TierCode.Wholesale, 199.00m)); // 149.25
```

The lambdas are stored as `static`-equivalent values in a `static readonly` dictionary, so they are cached once and reused without allocation per call. Adding a new tier requires one new dictionary entry — no switch statement to update, no new class to register. Each rule is small enough to test by calling `DiscountStrategies.Apply` with a known input and asserting the output. The pattern is effective when rules are simple; once a rule requires injected dependencies (a database lookup, a configuration service), promote it to a class implementing an `IDiscountStrategy` interface and resolve it through the container, using the lambda form only for rules that are genuinely pure functions of price.

---

## S5. Use an async lambda inside `Task.Run`, handling the loop-capture bug and observability.

**Concepts**
- `Task.Run(Func<Task>)` overload
- captured loop variable copy
- `await` inside async lambda
- collecting `Task` references for `Task.WhenAll`
- exception propagation through awaited tasks

**Answer**

Launching async work per item in a loop requires capturing a loop-local copy and collecting all tasks for observation:

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public sealed class CatalogRefreshService
{
    private readonly IPriceGateway _gateway;
    private readonly IPriceCache   _cache;

    public CatalogRefreshService(IPriceGateway gateway, IPriceCache cache)
    {
        _gateway = gateway;
        _cache   = cache;
    }

    public async Task RefreshAllAsync(IEnumerable<string> skus)
    {
        var tasks = new List<Task>();
        foreach (var sku in skus)
        {
            var localSku = sku;                  // loop-local copy avoids capture bug
            tasks.Add(Task.Run(async () =>
            {
                var price = await _gateway.FetchPriceAsync(localSku);
                _cache.Set(localSku, price);
            }));
        }
        await Task.WhenAll(tasks);               // all exceptions surface here
    }
}

public interface IPriceGateway { Task<decimal> FetchPriceAsync(string sku); }
public interface IPriceCache   { void Set(string sku, decimal price); }
```

Three corrections relative to a naive implementation: `var localSku = sku` creates a per-iteration copy so every lambda closes over a distinct string rather than sharing the last value from the enumerator. `Task.Run(async () => ...)` uses the overload that accepts `Func<Task>`, returning an outer `Task` that wraps the inner one — without this, `Task.Run` would wrap an `async void` and the inner task would be unobserved. Collecting all tasks in a list and awaiting `Task.WhenAll` ensures that every exception from every lambda is captured and rethrown as an `AggregateException`, making failures visible to Application Insights or any other exception handler watching the `Task` returned from `RefreshAllAsync`.

---

## S6. Use a lambda with `Expression<Func<T, bool>>` in an EF Core repository to translate filters to SQL.

**Concepts**
- `Expression<Func<T, bool>>` parameter type
- EF Core `IQueryable<T>.Where` extension
- SQL translation vs in-memory evaluation
- composable expression building
- `AsQueryable` entry point

**Answer**

An EF Core repository that accepts an expression-tree predicate keeps filtering server-side:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public class Product
{
    public int     Id        { get; set; }
    public string  Name      { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public bool    InStock   { get; set; }
}

public sealed class ProductRepository
{
    private readonly DbContext _db;

    public ProductRepository(DbContext db) => _db = db;

    public async Task<List<Product>> FindAsync(
        Expression<Func<Product, bool>> predicate,
        CancellationToken ct = default)
    {
        return await _db.Set<Product>()
                        .Where(predicate)
                        .AsNoTracking()
                        .ToListAsync(ct);
    }
}

// Call site — the lambda is an expression tree, translated to SQL:
// var cheap = await repo.FindAsync(p => p.UnitPrice < 50m && p.InStock);
```

Because `FindAsync` declares `Expression<Func<Product, bool>>`, the compiler emits an expression tree at each call site. EF Core's `Where` overload on `IQueryable<T>` accepts this tree and uses its internal translator to emit `WHERE UnitPrice < 50 AND InStock = 1` — only the matching rows travel over the network. If the parameter were changed to `Func<Product, bool>`, calling `_db.Set<Product>().Where(funcPredicate)` would invoke the `IEnumerable<T>.Where` overload, loading the entire table into memory before filtering. The repository pattern shown here is also composable: multiple `Expression` predicates can be combined with `Expression.AndAlso` before being passed in, enabling dynamic query building without raw SQL strings.

---

## S7. Code review: identify the defects in this lambda-heavy pricing pipeline.

**Concepts**
- loop-capture bug
- `async void` event subscription
- static lambda with captured variable
- missing `await` on returned `Task`
- wrong LINQ overload (`IEnumerable` instead of `IQueryable`)

**Answer**

Review the following pricing pipeline for defects:

```csharp
public class PricingController
{
    private decimal _taxRate = 0.08m;
    private readonly List<Func<decimal, decimal>> _transforms = new();

    public void LoadCatalog(string[] skuList)
    {
        for (int i = 0; i < skuList.Length; i++)
        {
            _transforms.Add(static price => price * (1m + _taxRate)); // line A
        }
    }

    public void WireEvents(PriceUpdater updater)
    {
        updater.PriceChanged += async (s, e) =>        // line B
        {
            await Task.Delay(100);
            throw new InvalidOperationException("bad price");
        };
    }

    public async Task<List<decimal>> GetDiscountedAsync(
        IQueryable<decimal> prices, decimal cut)
    {
        return await Task.Run(() =>                    // line C
            prices.Where(p => p > cut).ToList()
        );
    }
}
```

| Category | Problem | Impact |
|---|---|---|
| Static lambda | Line A: `static` lambda references instance field `_taxRate` — CS8971 compile error | Will not compile |
| Loop redundancy | Even after removing `static`, identical lambdas are added once per SKU with no per-SKU variation — semantically wasteful | Logic error / performance |
| Async void event | Line B: event handler is `async void`; the thrown `InvalidOperationException` escapes to the synchronisation context | Unhandled exception, potential process crash |
| IQueryable + Task.Run | Line C: `prices.Where(...)` on `IQueryable<T>` enumerates the provider inside a thread-pool thread, bypassing async DB I/O; `ToListAsync` is the correct terminal | Blocks a thread-pool thread, no async DB benefit |
| Missing await | `Task.Run` result is returned from an `async` method but the caller must await the outer `Task`; without `await Task.Run(...)` the exception in the lambda would be lost if the `Task` were fire-and-forget | Potential lost exceptions |

**Fix priority:**

1. Remove `static` from the lambda on line A, or pass `_taxRate` as a parameter rather than capturing it.
2. Change the event-handler signature to accept a `Func<Task>` stored separately, or handle exceptions inside the lambda with `try/catch` so they do not propagate as unhandled.
3. Replace `Task.Run(() => prices.Where(...).ToList())` with `await prices.Where(...).ToListAsync(ct)` using EF Core's async terminal operator.
4. Remove the loop if all transforms are identical; add one transform once, or vary the transform per SKU using a loop-local copy of index-specific data.
