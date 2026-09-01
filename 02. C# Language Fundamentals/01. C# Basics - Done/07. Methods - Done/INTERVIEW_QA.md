# C# Methods — Interview Q&A

## Foundation Questions

---

## Q1. What is a C# method and what does a method declaration consist of?

**Concepts**
- Named, reusable block of statements executed by a call
- Declaration parts: access modifier, optional static, return type, name, parameter list, body
- void vs non-void return type
- CS0161: not all code paths return a value
- PascalCase naming convention for methods

**Answer**

A method is a named block of statements that executes when called by name. The declaration has five structural parts: an access modifier (public, private, internal, etc.) that controls who can invoke it; an optional static keyword that makes the method belong to the type rather than an object instance; a return type, which is any valid type when the method produces a value or void when it does not; a name; and a parameter list in parentheses followed by a body in braces. Non-void methods must return a value of the declared type on every possible code path — any path that reaches the closing brace without a return statement causes CS0161 at compile time. Void methods end naturally at the closing brace or can exit early with a bare `return;`, which is useful for guard clauses at the top of a method. In the warehouse project, `public static decimal CalculateLineTotal(int qty, decimal price)` is public, static, returns decimal, and has two parameters. `Main(string[] args)` is the special entry-point method the runtime calls to start execution. C# convention is PascalCase for all method names.

---

## Q2. What is the difference between parameters and arguments?

**Concepts**
- Parameters: variables in the method signature (declaration)
- Arguments: actual values supplied at the call site
- Positional mapping: first argument to first parameter by default
- Named arguments allow explicit mapping by parameter name
- Compiler checks argument types against parameter types

**Answer**

Parameters are the variables declared inside the method signature — they are the method's formal description of what inputs it needs. Arguments are the actual values the caller supplies when invoking the method. In the declaration `CalculateLineTotal(int qty, decimal price)`, qty and price are parameters. At the call site `CalculateLineTotal(36, 49.99m)`, the integer 36 and the decimal 49.99m are the arguments. By default, arguments map to parameters positionally: the first argument binds to the first parameter, the second to the second, and so on. Named arguments (covered in Q13) allow a caller to bind by parameter name, enabling reordering or skipping optional parameters. The compiler verifies that each argument's type either matches or implicitly converts to the corresponding parameter's type, and that all required parameters are satisfied. Getting the terminology right matters in interviews because questions about overload resolution, ref/out/in, and optional parameters all hinge on precisely distinguishing what the declaration says from what the call site provides.

---

## Q3. How do expression-bodied methods work and when should you prefer them?

**Concepts**
- Arrow syntax `=>` replaces `{ return expr; }` for a single expression
- Available for both non-void (expression is the return value) and void methods (expression is the single statement)
- Generates identical IL to the block-body form
- Readability guideline: use for short, obvious calculations only
- Avoid for multi-step or branching logic

**Answer**

An expression-bodied method uses the `=>` arrow syntax in place of a brace block. Instead of `{ return amount * rate; }` you write `=> amount * rate`. The compiler generates identical IL — the difference is purely stylistic. For non-void methods the expression becomes the return value; for void methods the expression becomes the single executed statement. In the warehouse code, `ApplyPercent(decimal amount, decimal rate) => amount * rate` is a clean example of a pure computation that has no side effects and fits on one line. The guideline is to use expression-bodied form when the entire logic is a single expression and the intent is immediately clear to any reader. For methods that involve branching, multiple statements, loops, or any logic that benefits from vertical whitespace, a regular block body is more readable. Overusing expression-bodied form for complex logic trades away clarity for terseness without any runtime benefit. The same `=>` syntax also applies to properties, constructors, and finalizers in C#.

---

## Q4. What does the void return type mean, and how does early return work in void and non-void methods?

**Concepts**
- void: method produces no value; caller cannot assign the result
- void method exits at closing brace or with bare `return;`
- Early return as a guard clause: validate input before main logic
- Non-void: all paths must return a value or throw — CS0161
- Throwing an exception satisfies the compiler on a non-void path

**Answer**

A void return type declares that the method produces no output. The call cannot appear on the right side of an assignment. Void methods run for their side effects — printing, updating state, triggering events — and the runtime returns control to the caller when execution reaches the closing brace. A void method can also exit early with a bare `return;`, which is the basis of the guard-clause pattern: validate at the top and return immediately if the input is invalid or the precondition is unmet, then proceed with the main logic unindented below. In `PrintSkuBanner`, the method returns early when the SKU string is null or whitespace, avoiding the Console.WriteLine call entirely. For non-void methods, every possible code path must either reach a `return value;` statement or throw an exception — the compiler enforces this with CS0161 when any path reaches the closing brace without returning. Throwing counts as satisfying the return requirement because the method can never silently produce no value; it either returns or propagates an exception up the call stack.

---

## Q5. What is method overloading and what constitutes a distinct method signature?

**Concepts**
- Overloading: same name, different parameter list within the same type
- Signature: method name + ordered parameter types + modifiers (ref/out/in/params)
- Return type alone does NOT distinguish overloads — CS0111
- Parameter names alone do NOT distinguish overloads
- Overload resolution happens at compile time

**Answer**

Method overloading means declaring multiple methods with the same name in the same class, each with a different parameter list. The compiler uses the types and count of the arguments at each call site to choose among them at compile time. A method signature consists of the method name combined with the ordered list of parameter types plus any modifiers such as ref, out, in, or params. Parameter names and return types are not part of the signature. Declaring two methods with identical names and identical parameter type sequences but different return types causes CS0111 — the compiler sees them as the same member and rejects the duplicate. The reason return type is excluded is that any call site can discard the return value: if you write `Format(42);`, the compiler has no way to determine which return-type variant you intended. In the warehouse project, `EstimateShipping(decimal)` and `EstimateShipping(decimal, double)` are valid overloads because they differ in parameter count. Adding a third `EstimateShipping(int, double)` returning string would also be valid because the first parameter type differs, giving the compiler a basis for selection.

---

## Q6. How does overload resolution work — how does the compiler pick the best match among applicable overloads?

**Concepts**
- Phase 1: collect applicable candidates (argument types implicitly convertible to parameter types)
- Phase 2: pick better function member (fewer conversions, exact type beats conversion, fixed arity beats params, required beats optional)
- CS0121: equally-good candidates produce an ambiguous call error
- Named arguments applied after resolution — do not influence which overload is chosen
- Non-params overload preferred over params expansion when arity matches exactly

**Answer**

When the compiler sees a method call, it first collects every accessible overload that could accept the arguments through implicit conversion — the applicable candidates. It then selects a single best candidate using tie-breaking rules: an exact type match beats an implicit conversion; a signature requiring fewer total conversions beats one requiring more; a fixed-arity overload beats a params expansion when the argument count matches exactly; and a required-parameter overload beats an optional-defaulted one that fills in defaults to match. In the warehouse demo, `CountTokens("A", "B")` resolves to the fixed two-string overload rather than the params version because fixed arity is preferred. If two overloads are equally good — neither is strictly better by any rule — the compiler raises CS0121 and refuses to build. The fix is to cast an argument to the intended type, making one overload strictly better. A common mistake is assuming named arguments can steer overload selection; they cannot. Overload resolution ignores argument names entirely, choosing the overload based on types only, and then applies named arguments to fill parameters within the already-selected overload.

---

## Q7. What is the difference between passing a value type and a reference type to a method by default (no modifier)?

**Concepts**
- Value types: method receives a copy of the bits — changes to the parameter do not affect the caller
- Reference types: method receives a copy of the reference — both see the same heap object
- Mutating the object's state through the reference IS visible to the caller
- Reassigning the parameter to a new object is NOT visible to the caller
- ref modifier required to redirect the caller's variable to a different object

**Answer**

When a value-type argument — int, decimal, bool, a struct — is passed without a modifier, the method receives an independent copy of the value's bits. Any assignment to the parameter stays local; the caller's variable is unaffected. In the warehouse demo, `TryBumpByValue(packedUnits, 5)` increments its local copy of units but leaves `packedUnits` unchanged in Main. When a reference-type argument — a class instance, array, or string — is passed without a modifier, the method receives a copy of the reference, which is essentially a second pointer to the same heap object. Both the caller and the method see the same object in memory. If the method modifies the object's fields or calls mutating methods on it, the caller observes those changes. However, if the method reassigns the parameter variable to point at a completely new object (`param = new Foo()`), the caller's variable still points to the original object — only the local copy of the address changed. To let a method redirect the caller's pointer, you need the ref modifier. This distinction is a frequent source of confusion: you can alter the contents of a shared object by default, but you cannot substitute a different object without ref.

---

## Q8. What does the ref modifier do, and what requirements must both the declaration and the call site satisfy?

**Concepts**
- ref passes an alias to the caller's variable — reads and writes go to the caller's storage location
- Caller must initialize the variable before the call — CS0165 for uninitialized
- Both declaration and call site must carry the ref keyword
- Differs from default: assignment inside the method changes the caller's variable
- Guideline: prefer returning a new value; use ref when updating an existing meaningful variable

**Answer**

The ref modifier makes the parameter an alias for the caller's actual variable. Reading or writing the parameter inside the method is identical to reading or writing the caller's variable — they share the same storage location. Two requirements must be satisfied: the caller must fully initialize the variable before passing it (the compiler raises CS0165 for an uninitialized ref argument), and both the method declaration and the call site must carry the ref keyword — the symmetry is intentional, making the aliasing visible to anyone reading either side. In the warehouse demo, `AdjustQuantity(ref packedUnits, 12)` adds 12 directly to the caller's `packedUnits` variable, whereas the by-value version left it unchanged. The practical guideline is to prefer returning a new value when the method produces a single result — the function signature is cleaner and the intent is clearer. Reserve ref for cases where the method genuinely needs to update a pre-existing, already-meaningful variable in place, such as accumulating into a running total that already holds intermediate state. Be aware that for large value types, ref and in both avoid copies, but only ref allows the method to write.

---

## Q9. What does the out modifier do, and how does it differ from ref in contract and usage?

**Concepts**
- out: method must assign the parameter on every code path before returning — CS0177
- Caller does not need to initialize beforehand
- C# 7 inline declaration: declare the variable directly at the call site
- Try-pattern idiom: bool return + one or more out parameters for extra outputs
- Comparison table: ref (read/write alias, caller must init), out (write-only alias, callee must assign)

**Answer**

The out modifier is a close sibling of ref — it also passes a reference to the caller's variable so the method can write back a result. The differences are contractual: the caller is not required to initialize the variable before the call (out is purely an output direction), and the method is required to assign every out parameter on every reachable code path before returning; any path that returns without assigning an out parameter causes CS0177. This makes the contract unambiguous: the caller gets a fresh value out of the method, not a value it had to pre-set. The canonical idiom is the Try-parse pattern where a bool return signals success and an out parameter carries the parsed or retrieved value. In the warehouse demo, `TrySplitCases(packedUnits, 12, out int fullCases, out int looseUnits)` uses the C# 7 inline declaration syntax, eliminating the need to declare fullCases and looseUnits on separate lines before the call. The rule to remember: ref requires the caller to initialize and allows the callee to read or write; out requires the callee to write on every path and frees the caller from initializing first.

---

## Q10. What is the in modifier and when does it give a performance benefit?

**Concepts**
- in: read-only reference — compiler error CS8331 if callee tries to assign
- Avoids copying large struct fields onto the stack
- Most effective with readonly struct — no defensive copy overhead
- Mutable struct + in: compiler inserts defensive copy before each instance method call
- Call site: in keyword optional for variables, typically required only when forwarding to another in parameter

**Answer**

The in modifier passes a read-only reference: no copy is made (as with ref), but the compiler prevents any assignment to the parameter inside the method body, raising CS8331 if you try. The primary motivation is performance with large value types. A struct with many fields would normally be copied onto the stack for every call, paying a cost proportional to the struct's size. With in, only a pointer is passed regardless of struct size. The benefit is most meaningful for structs of roughly 32 bytes or more. In the warehouse code, `ReadOnlyLineTotal(in PackingSlip slip)` avoids copying the three-field PackingSlip every call. There is an important trap with mutable structs: when the struct is not declared readonly, the compiler must insert a defensive copy before any instance method call on the in parameter, because it cannot verify that the method won't mutate the struct. This defensive copy restores the per-call overhead that in was meant to eliminate. Declaring the struct as readonly removes all mutable instance methods (the compiler enforces this), so no defensive copy is needed and the in modifier delivers its full benefit. The combination of readonly struct and in is the idiomatic, safe, high-performance pattern.

---

## Q11. How does the params keyword work and what are its constraints on the parameter list?

**Concepts**
- params: collects a variable number of arguments into an array
- Must be the last parameter — CS0231 if not
- Only one params parameter per method
- Caller may pass zero or more individual values, or pass an array directly
- Fixed-arity overload preferred over params expansion when arity matches exactly

**Answer**

The params keyword declares that a parameter will collect a variable number of caller-supplied arguments of a given element type, packaging them into an array transparently. At the call site, the caller passes zero or more comma-separated values of the element type; the compiler wraps them into an array. The caller may alternatively pass a pre-existing array of the element type directly. Two hard constraints apply: the params parameter must be the last parameter in the list (CS0231 if violated), and only one params parameter is allowed per method (a second params anywhere also triggers CS0231). Calling `SumLineQuantities(10, 20, 6)` delivers `int[] {10, 20, 6}` to the method; calling `SumLineQuantities()` with no arguments delivers an empty array, so the foreach loop runs zero iterations safely. Overload resolution favors a fixed-arity overload over params expansion when the argument count matches exactly — this is why `CountTokens("A", "B")` binds to the fixed two-string overload rather than the params version even though both accept two strings.

---

## Q12. What are optional parameters and what is the key caveat about changing their default values in a library?

**Concepts**
- Optional parameter: has a default value; caller may omit the argument
- Default must be a compile-time constant (literal, null, default(T), enum member)
- Required parameters must precede all optional ones — CS1737
- Default value is baked into the call site IL, not stored in the method
- Library versioning trap: changing the default does not update pre-compiled callers

**Answer**

An optional parameter has a default value specified with `=` in the parameter list. A caller who omits the corresponding argument receives the default. In `FormatMoney(decimal amount, string currency = "USD")`, calling `FormatMoney(99.99m)` produces "USD 99.99". The default value must be a compile-time constant — a literal, null, `default(T)`, or an enum member; expressions that require runtime evaluation are not allowed. All required parameters must appear before optional ones; violating this ordering causes CS1737. The critical caveat is that the default value is not stored in the method's intermediate language — it is inlined into the calling assembly's IL at compile time. If a library ships with `currency = "USD"` and later changes the default to "EUR", applications compiled against the old library continue to use "USD" until they are recompiled against the new binary. There is no runtime error or warning — the behavioral change is invisible. For stable public APIs exposed in shared libraries, overloads are safer: the default logic lives in the method body inside the library, so updating it takes effect immediately for all callers on the next load without recompilation.

---

## Q13. How do named arguments work, and what is the rule about mixing them with positional arguments?

**Concepts**
- Named argument: `paramName: value` maps explicitly to a parameter by name
- Allows reordering arguments or skipping optional parameters by name
- Positional arguments must precede all named arguments — CS1738
- Named arguments do not participate in overload selection
- Particularly useful with bool flag parameters for clarity

**Answer**

Named arguments allow a caller to supply an argument using the syntax `paramName: value`, explicitly binding the argument to a named parameter instead of relying on position. This makes call sites self-documenting — particularly valuable for methods with multiple bool or int parameters where position alone gives no indication of intent. Named arguments also allow a caller to skip optional parameters in the middle of the list by naming only the optional they want to override. In the warehouse demo, `BuildInvoiceLine(qty: 36, price: 49.99m, sku: "WH-4412")` supplies all three required arguments out of their declared order using names. The mixing rule is strict: positional arguments must come before all named arguments in the argument list; placing a positional argument after a named one causes CS1738. A common misconception is that naming an argument can influence overload selection — it cannot. The compiler resolves the overload based purely on argument count and types; named arguments are applied afterward to bind arguments to parameters within the already-chosen overload. You cannot use a parameter name to force a different overload.

---

## Q14. What is a local function and how does it differ from a private static helper method on the class?

**Concepts**
- Local function: method declared inside another method's body
- Visible only within the enclosing method — no access from outside
- Can capture locals and parameters of the enclosing method (closure)
- Can be marked static to prevent accidental capture
- Useful for recursive helpers or one-use sub-computations scoped to one method

**Answer**

A local function is a method declared inside the body of another method. It is a first-class method — it has a parameter list, a return type, and a body — but its scope is restricted to the enclosing method. No code outside that method can call or reference it. In `QuoteWithLocalHelper`, the local function `decimal Add(decimal a, decimal b) => a + b` is declared and consumed entirely within one method's scope, keeping it out of the class's public or private method list. Unlike a private static method on the class, a local function can close over the enclosing method's local variables and parameters, accessing them without needing to pass them as arguments. This closure capability is powerful but can introduce subtle bugs if a captured variable mutates after the function definition. Marking a local function with the static keyword prevents it from capturing any outer variables, making the intent explicit and eliminating accidental captures at compile time. Local functions are well suited for recursive helpers where the recursion is an implementation detail of one operation, for iterator or async helper sub-routines, and for any sub-computation that is meaningful only within a single method's context.

---

## Q15. How does recursion work in C# and what risk does deep recursion carry?

**Concepts**
- Recursive method calls itself with a modified input
- Base case: terminates the chain — no further self-call
- Recursive case: must move toward the base case on every branch
- Stack frame per call — thread stack ~1 MB default
- StackOverflowException: uncatchable, terminates the process
- Prefer iteration for deep or data-bounded depth

**Answer**

A recursive method calls itself as part of its own implementation, reducing the problem toward a base case that terminates the chain. Every correct recursive design needs two things: a base case that returns without a further self-call, and a recursive case whose arguments move strictly closer to the base case on every invocation. In `Factorial(n)`, the base case is `n <= 1` returning 1, and the recursive case is `n * Factorial(n - 1)`, which decrements n by 1 each call until it reaches the base. Each call pushes a new stack frame onto the thread's call stack, consuming memory proportional to the frame size and the recursion depth. The default thread stack in .NET is approximately one megabyte. A recursion that never reaches its base case — for example, because negative input bypasses the n == 0 guard — will exhaust the stack and raise StackOverflowException, which cannot be caught in a try/catch block and terminates the process immediately. For algorithms whose depth is bounded by structure (binary tree height, merge sort split depth), recursion is elegant and safe. For algorithms whose depth is bounded by data size — iterating over a list of orders that could hold 100,000 items — an iterative loop is the right tool; it processes any number of items with no stack growth.

---

## Gotchas

---

## Q16. What compiler error results from declaring two methods that differ only in return type, and why does C# prohibit it?

**Concepts**
- CS0111: type already defines a member with the same parameter types
- Return type is not part of the method signature
- Ambiguity: caller can discard the return value — compiler has no basis to select
- Fix: rename one method or use a single method with a suitable return type
- Contrast with operator overloading where return type is structurally fixed

**Answer**

Declaring two methods with the same name and identical parameter types but different return types causes CS0111. The compiler reports that the type already defines a member with the same parameter types, and rejects the duplicate. Return type is deliberately excluded from the method signature because a caller can always discard the return value. If you write `Format(42);` as a statement and two versions exist — one returning int, one returning string — the compiler has no signal about which you want. Overload resolution would be fundamentally ambiguous. This rule occasionally surprises developers coming from languages that support return-type covariance or more flexible overloading. The fix is to give the two methods distinct names that reflect what they return or what they do, or to use a generic method where the return type is a type parameter. The single exception in C# is method overriding in inheritance: an overriding method may return a more derived type than the base (covariant returns, supported since C# 9), but that is not a new overload in the same type — it replaces an inherited member.

---

## Q17. Optional parameter defaults are baked into the call site at compile time — why is this a library versioning trap?

**Concepts**
- Compile-time baking: default value inlined into calling assembly's IL, not the library's IL
- Changing a default in the library does not update pre-compiled callers
- No error or warning — silent behavioral inconsistency
- Fix: use overloads so default logic lives in library's method body
- Applies to any cross-assembly call; intra-assembly is recompiled together

**Answer**

When the compiler encounters `FormatMoney(amount)`, it does not emit an instruction saying "fetch the default at runtime." Instead, it writes `FormatMoney(amount, "USD")` directly into the calling assembly's IL, with the literal "USD" stamped in the caller's binary. The default value lives in the caller, not in the library. This is binary-invisible: if you ship version 1.0 with `currency = "USD"` and later publish version 2.0 with `currency = "GBP"`, every application that was compiled against version 1.0 continues to pass "USD" forever, even after upgrading to the new library DLL. There is no runtime error, no warning, and no obvious diagnostic — the application just silently uses the old default until someone recompiles it. For libraries distributed via NuGet or shared across teams, this makes optional parameter defaults a maintenance hazard. The mitigation is to use overloads: the shorter overload calls the longer one with the current default written in the library's method body. When you update that internal call, all callers pick up the change on the next load without needing recompilation. Optional parameters are fine for private or internal helpers where the caller and callee are always compiled together.

---

## Q18. Named arguments do not participate in overload resolution — what does this mean and what mistake does it prevent?

**Concepts**
- Overload resolution based on argument count and types only — names ignored
- Named arguments applied after the overload is chosen, to bind arguments to parameters
- Cannot use parameter name to select a differently-typed overload
- Two overloads differing only in parameter names are still ambiguous — CS0121
- Common misconception: naming steers to the "correct" overload

**Answer**

A common misconception is that supplying a named argument can influence which overload the compiler selects. It cannot. Overload resolution examines the number and types of arguments, ignoring their names entirely, to identify and rank applicable candidates. Only after the overload is definitively chosen does the compiler apply named arguments, mapping each named argument to the parameter that matches its name within that already-selected overload. The practical consequence is that if two overloads share the same parameter types in the same positions but have different parameter names, they are still ambiguous: the compiler sees identical signatures and raises CS0111 (or CS0121 if both are accessible). You cannot use `Apply(amount: 100m, percentOff: 5)` to force the int-percent overload when the compiler has already resolved the overload based on types. Named arguments are purely a readability and reordering tool: they clarify intent at the call site, allow skipping optional parameters, and permit argument reordering — but they make no contribution to the type-based selection that determines which overload runs.

---

## Q19. When you pass a reference-type argument and the method reassigns the parameter, why does the caller's variable remain unchanged?

**Concepts**
- Default pass copies the reference value (address), not the heap object
- Reassigning the parameter changes the local address copy only
- Caller's variable still holds the original address — unaffected
- Mutation through the shared reference IS visible (same heap object)
- ref modifier needed to redirect the caller's pointer

**Answer**

Passing a reference-type argument without a modifier copies the reference — the address pointing to the heap object — not the object itself. After the call, both the caller and the method hold separate address variables that both point to the same heap object. If the method calls mutating methods on the object or writes to its fields through the parameter, the caller observes those changes because both addresses lead to the same memory. However, if the method writes `param = new Foo()`, it is only overwriting its local copy of the address. The caller's address variable is entirely separate and still holds the original address — nothing about the caller's heap object changed. To allow a method to redirect the caller's pointer to a different object, you must pass the address variable itself by reference using ref, making the parameter an alias for the caller's address variable. A concrete example in the warehouse context: passing an `Order` instance allows the method to update `order.Quantity` visibly, but assigning `order = new Order()` inside the method leaves the caller's Order reference pointing at the original object. Understanding this prevents a class of subtle bugs where developers expect a method to "replace" an object but the caller continues using the original.

---

## Q20. How can two numeric overloads produce a CS0121 ambiguous call, and what is the correct fix?

**Concepts**
- CS0121: method call is ambiguous between two methods
- Ambiguity: argument type converts implicitly to both overloads' parameter types at equal cost
- Example: a literal whose type fits two overloads without one being strictly better
- Fix: explicit cast at the call site to select a single applicable overload
- Alternative fix: rename overloads to remove reliance on type-driven disambiguation

**Answer**

Overload ambiguity arises when the compiler finds two applicable candidates and neither is strictly better than the other by any tie-breaking rule. A common numeric scenario: suppose a class declares `Apply(decimal price, decimal rate)` and `Apply(decimal price, float rate)`. A caller passing a float literal matches the float overload exactly. But if both overloads require only one implicit conversion from the argument's type, and neither conversion is strictly cheaper, neither overload wins and the compiler emits CS0121. C# integer literals (such as 10) have type int by default, so `Apply(price, 10)` can resolve unambiguously if one overload accepts int and another does not. Floating-point literals depend on their suffix: `10.0` is double by default, `10.0f` is float, `10.0m` is decimal — choosing a literal suffix often resolves the ambiguity without an explicit cast. When a cast is needed, writing `Apply(price, (decimal)rate)` makes the intended overload explicit and eliminates the CS0121. The alternative is to rename overloads to reflect their intent in the name rather than the type, eliminating the need for callers to reason about numeric promotion rules to pick the right variant.

---

## Real-World Scenarios

---

## Q21. You are reviewing the following warehouse-discount helper. Identify the issues and state your fix priority.

```csharp
public static bool TryGetCouponRate(string code, ref decimal rate)
{
    if (code == "SAVE10") { rate = 0.10m; return true; }
    if (code == "HALF50") { rate = 0.50m; return true; }
    return false;
}

// OrderProcessor.cs
decimal discountRate;
bool found = TryGetCouponRate(input, ref discountRate);
if (found)
    finalPrice = basePrice * (1m - discountRate);
```

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Compilation | `ref` requires `discountRate` to be initialized before the call; it is declared but uninitialized | CS0165 compile error — code does not build |
| API Contract | On the false-return path, `rate` is never assigned | Caller may read a stale or garbage value on the false branch if pre-initialized to a non-zero sentinel |
| Idiom Violation | Try-pattern methods use `out` for output parameters by convention | Confuses readers familiar with `int.TryParse` and `Dictionary.TryGetValue`; unexpected ref semantics |

**Fix Priority**

1. Change `ref decimal rate` to `out decimal rate` in the method declaration and remove the `ref` keyword from the call site.
2. Add `rate = 0m;` assignment on the false-return path — `out` requires every path to assign the parameter before returning.
3. Remove the explicit initialization of `discountRate` at the call site; `out` does not require it (though leaving it is harmless).

**Answer**

The root defect is using ref where out is the correct modifier. Because ref declares a read/write alias, the compiler requires the caller to initialize the variable before the call — `decimal discountRate;` declares but does not initialize, so the compiler raises CS0165 at the call site. Even if the developer pre-initializes to `0m` to suppress the error, the method silently leaves the parameter unchanged on the false path, meaning after a false return the caller cannot distinguish "coupon not found" from "rate happened to be 0m from a previous call." The out modifier corrects all three issues simultaneously: it removes the caller's initialization burden (out is output-only, so uninitialized is fine), it requires the compiler to verify that every code path assigns the parameter before returning (CS0177 enforces this), and it aligns the API with the established Try-pattern used throughout the .NET runtime. After the fix, any developer reading the signature `TryGetCouponRate(string code, out decimal rate)` immediately understands: this method will assign rate before returning; I do not need to initialize it first; on false return, rate will be 0m as the explicit default. The change is a single-word substitution in the declaration with no behavioral cost.

---

## Q22. An intern submitted the following shipping-label builder. Identify the issues and state your fix priority.

```csharp
public static string CreateShipLabel(
    string carrier = "UPS",
    string trackingNumber,
    int weightGrams,
    bool isPriority = false)
{
    string tag = isPriority ? "[PRIORITY] " : "";
    return $"{tag}{carrier}: {trackingNumber} — {weightGrams}g";
}
```

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Compilation | Required parameters `trackingNumber` and `weightGrams` appear after optional parameter `carrier` | CS1737 compile error — code does not build |
| API Design | `carrier = "UPS"` is a business assumption baked as a default | Future carrier additions silently produce mislabeled shipments when callers omit the argument |
| Readability | Optional and required parameters mixed without logical grouping | Caller confusion about which arguments must be supplied; IntelliSense tooltip is misleading |

**Fix Priority**

1. Move all required parameters (`trackingNumber`, `weightGrams`) before all optional parameters (`carrier`, `isPriority`) to fix CS1737.
2. Evaluate whether `carrier` should remain optional at all; if all shipments must declare a carrier explicitly, remove the default to make the requirement visible.
3. Group parameters logically: identity fields first (`trackingNumber`, `carrier`), then measurements (`weightGrams`), then flags (`isPriority`).

**Answer**

C# requires that all optional parameters follow all required parameters in the parameter list. Placing an optional parameter (`carrier = "UPS"`) before required ones (`trackingNumber`, `weightGrams`) causes CS1737 and prevents the project from building. The immediate fix is reordering: required parameters first, optional last. Beyond the compile error, the `carrier = "UPS"` default deserves scrutiny in a real code review. A shipping carrier is a meaningful business value — every label must name a carrier — and defaulting to "UPS" means any call that forgets the carrier argument silently ships with the wrong label. Unlike `isPriority = false`, which is a safe zero-value default that callers will almost always want, defaulting the carrier is a latent correctness risk. Making `carrier` a required parameter forces the caller to state it explicitly, turning a potential runtime mislabeling into a compile-time omission. The parameter ordering fix takes one line; the decision about required vs optional is a team design conversation, but the code review should surface it.

---

## Q23. A developer wrote the following recursive order-quantity accumulator. Identify the issues and state your fix priority.

```csharp
public static long SumToN(int n)
{
    if (n == 0)
        return 0;

    return n + SumToN(n - 1);
}

// In OrderProcessor.cs:
int adjustedQty = receivedQty - reservedQty;
long runningTotal = SumToN(adjustedQty);
```

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Correctness | No guard for negative input; `n` decrements indefinitely below zero and never reaches the base case | StackOverflowException at runtime when `adjustedQty` is negative |
| Robustness | `adjustedQty` is a subtraction that can go negative on oversold inventory | Defect is latent; triggered only on edge-case data, not in happy-path testing |
| Contract | No `ArgumentOutOfRangeException` thrown for `n < 0` | Process crashes with an uncatchable exception instead of a diagnosable domain error |

**Fix Priority**

1. Add a guard at the top of `SumToN`: `if (n < 0) throw new ArgumentOutOfRangeException(nameof(n), "n must be >= 0");`
2. Validate `adjustedQty` in OrderProcessor before calling `SumToN`, or clamp it: `Math.Max(0, adjustedQty)`.
3. Replace the recursion with the closed-form formula `(long)n * (n + 1) / 2` — it runs in O(1), eliminates all stack growth, and is immune to both the overflow risk and deep recursion.

**Answer**

The only termination guard is `n == 0`. When n is negative, the recursive case subtracts 1, driving n further from zero, creating an infinite chain of calls. Each call pushes a frame onto the thread stack until the stack is exhausted. In .NET, StackOverflowException is an asynchronous exception that the runtime does not allow user code to catch — it terminates the process, logs nothing to your error handler, and leaves no managed stack trace in most configurations. The silent root cause is that `adjustedQty` is computed from a subtraction without a lower-bound check, so any inventory scenario where reserved quantity exceeds received quantity passes a negative value silently. The guard at the top of SumToN is the first defensive layer: it converts an unbounded crash into a diagnosable ArgumentOutOfRangeException that callers can catch and log. The validation in OrderProcessor is the second layer: it handles the domain logic (zero or negative adjusted quantity means no units to sum) before the utility function ever sees it. Replacing the recursion with the O(1) formula eliminates the risk category entirely and is the right long-term fix, since SumToN's depth grows linearly with n and warehouse quantities are data-driven.

---

## Q24. When would you choose `in` over `ref` or a plain value pass for a struct parameter, and what is the risk if the struct is not declared `readonly`?

**Concepts**
- Plain value pass: full copy of struct fields onto the stack per call
- ref: read/write alias — appropriate only when callee must write back
- in: read-only alias — avoids copy, enforces no mutation
- readonly struct + in: no defensive copy — full performance benefit
- Mutable struct + in: compiler inserts defensive copy before instance method calls

**Answer**

The decision between plain pass, ref, and in comes down to two questions: does the method need to write back, and is the struct large enough to make copying expensive? A plain pass is correct for small structs (up to the pointer size, typically 8–16 bytes on 64-bit) where the copy cost is negligible and the code is clearest. ref is appropriate when the method must update an existing meaningful variable in the caller — it provides a full read/write alias. The in modifier is the right choice when the method only reads the struct and the struct is large enough for copy cost to matter; it passes a read-only reference, paying only pointer-size overhead regardless of the struct's field count. The critical risk appears with mutable structs. When a struct has instance methods that can write to its fields, and you pass it as in, the compiler cannot guarantee that calling those methods won't mutate the struct through the read-only reference. To enforce the contract, the compiler inserts a defensive copy — a full copy of the struct — before every such method call. The defensive copy silently restores the per-call overhead that in was intended to eliminate. Declaring the struct as readonly removes all mutable instance members (the compiler enforces that readonly struct fields cannot be assigned), eliminating both the defensive-copy overhead and the risk of accidental mutation. The `PackingSlip` in the warehouse code is correctly declared as readonly struct, making `in PackingSlip slip` both safe and efficient.

---

## Q25. When should you prefer method overloads over optional parameters for a public API, and what are the trade-offs of each approach?

**Concepts**
- Optional parameters: concise, fewer methods, defaults baked at call site
- Overloads: default logic in library body, immediately updated for all callers
- Binary compatibility: overloads are safer across library version boundaries
- Optional parameters: first-class in COM interop and scripting languages
- Guideline: optional parameters for private/internal; overloads for public stable APIs

**Answer**

Optional parameters produce concise code and keep the IntelliSense method list short — one method rather than two or three. They are appropriate for private helpers and internal methods where the calling code is always compiled alongside the library, so the call-site baking risk is irrelevant. For public APIs in shared libraries, overloads are safer for exactly the reason described in Q12: the default value is embedded in the caller's binary at compile time, not retrieved from the library at runtime. Updating the default in a later version of the library has no effect on pre-compiled consumers. Overloads solve this by placing the default logic inside the method body — the one-argument overload calls the two-argument overload with the current default written in the library code. Any update to that internal call takes effect immediately for all callers without recompilation. Overloads also allow genuinely different implementations per arity rather than just defaulting a value; sometimes the single-argument case uses a completely different algorithm that would be awkward to express as a default. The trade-off is surface area: more overloads mean more documentation entries, more signatures to maintain, and more combinations to test. The pragmatic guidance used by the .NET BCL itself is to use optional parameters freely in internal code and to lean on overloads in stable public APIs where consumers compile independently and defaults might evolve over time.

---

## Q26. A colleague argues that any iterative algorithm can be rewritten as a recursive method with no downside. How would you respond, and what does the warehouse codebase illustrate?

**Concepts**
- Each recursive call pushes a stack frame — thread stack ~1 MB default
- Iterative loops have O(1) stack overhead regardless of iteration count
- .NET JIT does not guarantee tail-call optimization for general recursive methods
- StackOverflowException is uncatchable — no recovery path
- Recursion shines when depth is structurally bounded (O(log n)); iteration wins when depth is data-bounded (O(n))

**Answer**

The claim is true in a theoretical sense — any loop can be expressed as a recursion — but it ignores the practical constraint that .NET enforces a finite thread stack. Each recursive call pushes a new frame onto the stack, and the default stack size is approximately one megabyte. For algorithms whose recursion depth is bounded by structure rather than data size — a binary tree search whose depth is O(log n), or merge sort whose split depth is O(log n) — the maximum frame count is small and well-bounded regardless of input size. Recursion is elegant and safe there. But for algorithms whose depth is bounded by data size — iterating over every unit in an order, summing quantities across line items — the depth grows linearly with the input. An order with 100,000 items would push 100,000 frames, far exceeding the default stack. In .NET the compiler and JIT do not guarantee tail-call optimization for general recursive methods the way some functional-language runtimes do, so even a tail-recursive form provides no protection. StackOverflowException cannot be caught; the process terminates with no managed recovery path. The warehouse's `SumLineQuantities` is correctly implemented as a foreach loop for exactly this reason: order sizes are data-driven and arbitrarily large in production, while the loop uses O(1) stack regardless of how many quantities are accumulated. The `Factorial` method is an acceptable use of recursion for demo purposes because n is passed explicitly and callers are expected to keep it small and non-negative.
