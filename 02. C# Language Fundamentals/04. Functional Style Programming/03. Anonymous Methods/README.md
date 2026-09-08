# C# Anonymous Methods — Interview Q&A


## Table of Contents

1. [Q1. What is an anonymous method in C# and how do you declare one?](#q1-what-is-an-anonymous-method-in-c-and-how-do-you-declare-one)
2. [Q2. How does an anonymous method differ from a lambda expression?](#q2-how-does-an-anonymous-method-differ-from-a-lambda-expression)
3. [Q3. Can an anonymous method access variables from the enclosing scope?](#q3-can-an-anonymous-method-access-variables-from-the-enclosing-scope)
4. [Q4. When can you omit the parameter list in an anonymous method?](#q4-when-can-you-omit-the-parameter-list-in-an-anonymous-method)
5. [Q5. How does the compiler implement an anonymous method internally?](#q5-how-does-the-compiler-implement-an-anonymous-method-internally)
6. [Q6. What restrictions apply to anonymous methods that do not apply to regular methods?](#q6-what-restrictions-apply-to-anonymous-methods-that-do-not-apply-to-regular-methods)
7. [Q7. How do anonymous methods interact with the event pattern in C#?](#q7-how-do-anonymous-methods-interact-with-the-event-pattern-in-c)
8. [Q8. What is the relationship between anonymous methods and multicast delegates?](#q8-what-is-the-relationship-between-anonymous-methods-and-multicast-delegates)
9. [Q9. Can an anonymous method be used as a method argument?](#q9-can-an-anonymous-method-be-used-as-a-method-argument)
10. [Q10. How do anonymous methods compare to named methods for testability and maintainability?](#q10-how-do-anonymous-methods-compare-to-named-methods-for-testability-and-maintainability)
11. [Q11. What happens if you capture a variable in an anonymous method that is later reassigned?](#q11-what-happens-if-you-capture-a-variable-in-an-anonymous-method-that-is-later-reassigned)
12. [Q12. How do you convert an anonymous method to a lambda expression?](#q12-how-do-you-convert-an-anonymous-method-to-a-lambda-expression)
13. [Q13. Why can you not use `yield return` inside an anonymous method?](#q13-why-can-you-not-use-yield-return-inside-an-anonymous-method)
14. [Q14. What happens when two anonymous methods capture the same outer variable?](#q14-what-happens-when-two-anonymous-methods-capture-the-same-outer-variable)
15. [Q15. Why does an event handler using an anonymous method create a memory leak risk?](#q15-why-does-an-event-handler-using-an-anonymous-method-create-a-memory-leak-risk)
16. [Q16. Can an anonymous method be used where an `Expression<Func<T>>` is expected?](#q16-can-an-anonymous-method-be-used-where-an-expressionfunct-is-expected)
17. [Q17. Is it safe to use an anonymous method inside a multi-threaded loop?](#q17-is-it-safe-to-use-an-anonymous-method-inside-a-multi-threaded-loop)
18. [Q18. You are reviewing legacy C# 2.0 code that uses anonymous methods throughout. When should you migrate them to lambdas and when should you leave them alone?](#q18-you-are-reviewing-legacy-c-20-code-that-uses-anonymous-methods-throughout-when-should-you-migrate-them-to-lambdas-and-when-should-you-leave-them-alone)
19. [Q19. A colleague has written an event handler using an anonymous method and the application is leaking memory in a long-running service. Walk through the diagnosis and fix.](#q19-a-colleague-has-written-an-event-handler-using-an-anonymous-method-and-the-application-is-leaking-memory-in-a-long-running-service-walk-through-the-diagnosis-and-fix)
20. [Q20. You are onboarding a junior developer who is confused about why their anonymous method sees a different value than they expected. Explain the capture semantics with a minimal example.](#q20-you-are-onboarding-a-junior-developer-who-is-confused-about-why-their-anonymous-method-sees-a-different-value-than-they-expected-explain-the-capture-semantics-with-a-minimal-example)
21. [Q21. Review the following code and identify problems:](#q21-review-the-following-code-and-identify-problems)

---
## Foundation Questions

---

## Q1. What is an anonymous method in C# and how do you declare one?

**Concepts**
- `delegate` keyword syntax
- inline method body
- no explicit method name
- delegate type compatibility
- C# 2.0 introduction

**Answer**

An anonymous method is an inline block of code that can be assigned to a delegate variable without needing a separately named method. You declare one using the `delegate` keyword followed by an optional parameter list and a code block. Because there is no method name, the code is bound directly to the delegate instance at the point of assignment. For example, `EventHandler handler = delegate(object sender, EventArgs e) { Console.WriteLine("clicked"); };` creates a complete delegate without defining a named method anywhere. The compiler generates a hidden class member to hold the code, making anonymous methods a stepping stone between named methods and the more concise lambda syntax introduced in C# 3.0. Anonymous methods remain valid in modern C#, though lambdas have largely superseded them for new code.

---

## Q2. How does an anonymous method differ from a lambda expression?

**Concepts**
- `delegate` keyword vs `=>` operator
- parameter list requirements
- expression vs statement body
- conversion to expression trees
- readability and brevity

**Answer**

The key syntactic difference is that anonymous methods use the `delegate` keyword while lambdas use the `=>` arrow operator. Anonymous methods require you to spell out the parameter list (or omit it entirely if no parameters are used), whereas lambdas infer parameter types from context and offer a more concise single-expression form. A more significant semantic difference is that anonymous methods cannot be converted to `Expression<TDelegate>` — only lambda expressions support that conversion, which is why LINQ providers like Entity Framework rely on lambdas rather than anonymous methods for query translation. Anonymous methods do offer one feature lambdas lack: you can omit the parameter list entirely when the parameters are not used, writing just `delegate { DoSomething(); }`, which is handy for event handler discards where sender and args are irrelevant.

---

## Q3. Can an anonymous method access variables from the enclosing scope?

**Concepts**
- outer variable capture
- closure semantics
- display class generation
- variable lifetime extension
- captured-by-reference semantics

**Answer**

Yes — an anonymous method can access any variable in scope at the point where it is declared, including local variables, method parameters, and instance fields. The compiler achieves this by generating a display class (a compiler-synthesized nested class) that holds captured variables as fields, and both the enclosing method and the anonymous method body share the same field. Because the capture is by reference to the shared field rather than by value at the moment of capture, mutations to a captured variable after the anonymous method is created are visible inside it, and vice versa. This also extends the lifetime of the captured variable: the local variable on the stack effectively moves to the heap inside the display class, keeping it alive as long as the delegate is reachable. This behavior is identical to how lambda closures work, since both compile to the same underlying mechanism.

---

## Q4. When can you omit the parameter list in an anonymous method?

**Concepts**
- parameter omission syntax
- delegate type inference
- event handler simplification
- parameter compatibility rule

**Answer**

You can omit the parameter list from an anonymous method when the anonymous method's body does not use any of the delegate's parameters. In that case, you write just `delegate { body }` with no parentheses at all. This is most commonly seen with event handlers where you want to perform a side effect but have no interest in the sender or event arguments. For example, `button.Click += delegate { Close(); };` is completely valid. This feature is intentional and is one area where anonymous methods offer a mild advantage over lambdas — lambdas must always include a parameter list, even if it is just empty parentheses `() =>` or a discard `_ =>`. The delegate-with-no-params shorthand reads naturally when the handler is entirely action-oriented.

---

## Q5. How does the compiler implement an anonymous method internally?

**Concepts**
- compiler-generated method
- display class (synthesized nested type)
- closure field hoisting
- IL emission
- delegate instance creation

**Answer**

When the compiler encounters an anonymous method, it synthesizes a private method on the enclosing class (or a display class if variables are captured) and replaces the anonymous method expression with a delegate instantiation pointing to that generated method. If the anonymous method captures no outer variables, the generated method is a simple static method and the delegate can be cached. If outer variables are captured, the compiler creates a display class — a private nested class — promotes all captured locals to fields on that class, creates an instance of it in the enclosing method, and emits the anonymous method as an instance method on the display class so it can read and write those fields. This is why captured locals appear to "escape" their declaring scope: they have actually been promoted from the stack to a heap-allocated object. Inspecting the compiler output with a decompiler clearly shows these generated types and methods.

---

## Q6. What restrictions apply to anonymous methods that do not apply to regular methods?

**Concepts**
- `goto` and `break` jump-out restrictions
- `unsafe` block requirement for pointers
- `yield return` prohibition
- `ref`/`out` outer variable capture restriction
- `params` keyword unavailability

**Answer**

Anonymous methods carry several constraints. You cannot use `goto`, `break`, or `continue` inside an anonymous method to jump to a label or loop that is defined outside the anonymous method body — these control-flow statements are scoped strictly within the anonymous method. You cannot use `yield return` or `yield break` inside an anonymous method, so they cannot be turned into iterators. Pointer arithmetic requires the anonymous method body to be in an `unsafe` context, but the `unsafe` modifier must appear on the anonymous method itself rather than being inherited from the enclosing method. Finally, when you capture a variable that is declared with `ref` or `out` in the enclosing signature, the anonymous method can read and write it, but there are strict lifetime rules to prevent dangling references since a captured `ref`/`out` parameter must not outlive the method call — the compiler enforces this.

---

## Q7. How do anonymous methods interact with the event pattern in C#?

**Concepts**
- event subscription with anonymous method
- inability to unsubscribe
- delegate reference capture workaround
- event handler compatibility
- `+=` operator assignment

**Answer**

Anonymous methods are frequently used to subscribe to events when the handling logic is brief and one-off. You write `someObject.SomeEvent += delegate(object s, EventArgs e) { HandleIt(); };`. The anonymous method is converted to a delegate instance compatible with the event's delegate type and added to the invocation list via `+=`. The critical limitation is that you cannot easily unsubscribe an anonymous method. Because the `delegate { ... }` expression creates a new delegate object each time it is evaluated, you have no reference to compare against when calling `-=`. The standard workaround is to capture the delegate in a variable before subscribing: `EventHandler h = delegate { HandleIt(); }; obj.Event += h;` — you can then call `obj.Event -= h;` later. If you never need to unsubscribe, the anonymous method approach is perfectly fine.

---

## Q8. What is the relationship between anonymous methods and multicast delegates?

**Concepts**
- delegate invocation list
- `+=` chaining
- multicast delegate behavior
- return value from last invocation
- exception propagation

**Answer**

An anonymous method assigned via `+=` is simply another delegate in the multicast invocation list — there is nothing special about its anonymous nature from the delegate runtime's perspective. When the delegate is invoked, each entry in the list runs in order. For delegates with a non-void return type, only the return value of the last invocation is surfaced to the caller; all others are discarded. If any delegate in the list throws an exception, execution stops at that point and subsequent delegates in the chain are not called unless you manually iterate `GetInvocationList()` and invoke each with its own try-catch. These multicast semantics apply equally whether the delegate entries are named methods, anonymous methods, or lambda expressions.

---

## Q9. Can an anonymous method be used as a method argument?

**Concepts**
- inline delegate expression
- parameter type matching
- compiler type inference
- LINQ predecessor pattern

**Answer**

Yes — you can pass an anonymous method wherever a delegate type is expected, including as a method argument. Before lambdas, this was the idiomatic way to pass callbacks inline: `list.Sort(delegate(int x, int y) { return x.CompareTo(y); });`. The compiler checks that the parameter list and return type of the anonymous method body match the delegate type expected by the method and emits the appropriate delegate instantiation. Because anonymous methods predate expression trees, they cannot be used where an `Expression<TDelegate>` is required — that slot must be filled by a lambda. When you call a method that accepts a `Func<T, TResult>`, you can pass either a lambda or an anonymous method and the compiler handles the conversion transparently.

---

## Q10. How do anonymous methods compare to named methods for testability and maintainability?

**Concepts**
- code discoverability
- unit test access
- responsibility separation
- anonymous method size guidelines
- refactoring triggers

**Answer**

Anonymous methods sacrifice discoverability and direct testability in exchange for locality. Because there is no named method, you cannot reference it in a test, apply an attribute to it, or find it through reflection by name. This is acceptable for trivial, single-purpose inline code, but becomes a maintenance liability when the body grows. The general guideline is to limit anonymous methods (and lambdas) to a few lines — anything beyond roughly five lines or requiring independent unit testing should be extracted into a named private method. Code review tools and static analyzers such as Roslyn analyzers can flag overly complex anonymous methods. Modern IDEs like Visual Studio and Rider offer "Extract Method" refactoring that works on anonymous method bodies, making the transition to a named method straightforward.

---

## Q11. What happens if you capture a variable in an anonymous method that is later reassigned?

**Concepts**
- shared display class field
- capture by reference semantics
- mutation visibility
- variable aliasing
- temporal coupling

**Answer**

When an anonymous method captures an outer variable, both the enclosing code and the anonymous method body share the same storage location — the field on the compiler-generated display class. If the enclosing code reassigns the variable after the anonymous method is created but before it is invoked, the anonymous method will see the updated value when it runs, not the value at the time of capture. This is a common source of bugs: a developer expects the closure to "snapshot" the value at creation time, but C# closures capture the variable (the storage location) rather than the value. The solution is to copy the variable to a new local before capturing: `var copy = outerVar; Action a = delegate { Use(copy); };`. This forces each iteration or call site to have its own display class field holding the snapshot.

---

## Q12. How do you convert an anonymous method to a lambda expression?

**Concepts**
- `=>` operator substitution
- parameter list simplification
- expression body extraction
- static lambda option
- Roslyn quick-fix

**Answer**

Converting an anonymous method to a lambda is mechanical. Replace `delegate(T1 p1, T2 p2) { return expr; }` with `(T1 p1, T2 p2) => expr` for a single-expression body, or `(p1, p2) => { statements; }` for a statement body, relying on type inference to drop the explicit types. If the anonymous method omits its parameter list (`delegate { body }`), the lambda equivalent uses a discard pattern or empty parens: `_ => { body }` or `() => { body }`, depending on whether the delegate type requires parameters. Visual Studio's Roslyn analyzer IDE0039 flags anonymous methods with a quick-fix action that performs this conversion automatically, so in practice the transformation is a one-click operation. The only case where the conversion is not purely mechanical is when the body uses `goto` or `break` to jump outside the body — those must be restructured before conversion.

---

## Gotchas — Anonymous Methods (Interview Traps)

---

#### Gotcha 1. Anonymous Method vs Lambda — `delegate(int x) { }` Is Legacy Syntax

**Concepts**
- anonymous method syntax uses the `delegate` keyword
- lambda syntax using `=>` is the modern replacement
- lambdas support expression tree conversion; anonymous methods do not
- `delegate { }` without a parameter list is one anonymous method advantage over lambdas

**Answer**

Anonymous methods using the `delegate` keyword were introduced in C# 2.0 and are now considered legacy syntax. Lambdas introduced in C# 3.0 are shorter, support expression tree conversion, and are preferred in all new code. The one case where anonymous methods retain an advantage is the parameterless form `delegate { }`, which can be assigned to any delegate type regardless of parameter count — useful as a no-op event handler that intentionally discards all parameters without listing them.

---

#### Gotcha 2. Anonymous Methods Cannot Use `yield return`

**Concepts**
- `yield return` requires the compiler to generate an iterator state machine
- state machine generation requires a full method declaration with a return type
- anonymous method lacks a named return type the compiler can transform
- local function is the correct in-scope iterator replacement

**Answer**

The `yield return` statement transforms the enclosing method into a state machine whose return type is `IEnumerable<T>` or `IEnumerator<T>`. Anonymous methods compile to hidden static or instance methods without a declared return type the compiler can change to an iterator type, so using `yield return` inside a `delegate { }` block is a compile-time error. The recommended replacement is a local function, which has a named return type declaration and full support for `yield return`, while still being declared inline within the enclosing method.

---

#### Gotcha 3. Parameter List Can Be Omitted When No Parameters Are Used

**Concepts**
- `delegate { body }` compiles against any delegate type
- useful for no-op event handlers that discard sender and args
- lambda requires an explicit parameter list even when parameters are unused
- potential confusion with a zero-parameter delegate type

**Answer**

An anonymous method written as `delegate { DoWork(); }` with no parentheses at all can be assigned to any delegate type regardless of how many parameters the delegate declares, because the compiler does not expose the parameters inside the body. This is the one syntactic feature anonymous methods have over lambdas, which always require an explicit parameter list (even an empty one or a discard). It is most useful for subscribing to events where you want to perform a side effect but have no interest in the sender or event arguments.

---

#### Gotcha 4. Variable Capture Semantics Identical to Lambdas — Same Loop-Closure Trap

**Concepts**
- anonymous method captures the variable reference, not a value snapshot
- shared display class field for all closures in the same scope
- `for` loop counter is shared across all iterations
- local copy inside the loop body is required for independent captures

**Answer**

Anonymous methods and lambdas use the same compiler mechanism for closure capture: both promote captured variables to fields on a display class and share a single field for all closures in the same scope. This means the classic for-loop variable capture bug applies equally to anonymous methods: `for (int i = 0; i < 5; i++) list.Add(delegate { Console.WriteLine(i); });` prints `5` five times. The fix is identical — declare `int copy = i;` inside the loop body and capture `copy` instead.

---

#### Gotcha 5. Anonymous Methods Cannot Be Converted to Expression Trees

**Concepts**
- expression tree conversion requires lambda syntax
- anonymous method always compiles to IL
- `IQueryable<T>` providers require `Expression<Func<T,bool>>` for SQL translation
- replacing anonymous methods with lambdas is required for ORM usage

**Answer**

Expression tree conversion is a compile-time feature available only for lambda expressions. When you assign a lambda to `Expression<Func<T, bool>>`, the compiler emits AST construction code instead of IL for the body. Anonymous methods using the `delegate` keyword always compile to IL and cannot be assigned to `Expression` types. This means anonymous methods are incompatible with Entity Framework Core and other `IQueryable` providers that depend on expression trees for SQL translation — the method must be rewritten as a lambda.

---

#### Gotcha 6. `goto` Inside an Anonymous Method Cannot Jump to an Outside Label

**Concepts**
- `goto` label scope is restricted to the anonymous method body
- control flow cannot escape the anonymous method via `goto`
- `break` and `continue` are similarly scoped to the anonymous method
- extract code outside the anonymous method if cross-body flow is needed

**Answer**

The `goto`, `break`, and `continue` statements inside an anonymous method can only target labels and loops within the anonymous method's own body. Attempting to jump to a label declared outside the anonymous method is a compile-time error. This scoping rule exists because the anonymous method compiles to a separate hidden method; a `goto` across method boundaries has no IL equivalent. If the desired control flow genuinely needs to exit the anonymous method's scope, restructure the logic so the condition is checked after the delegate returns, using a return value or a captured boolean flag.

---

#### Gotcha 7. Anonymous Method Type Inference — Cannot Use `var` for Assignment

**Concepts**
- anonymous method has no standalone type
- requires an explicit delegate type on the left side
- `var` inference fails with no target type
- compile error when no surrounding context provides the delegate type

**Answer**

Like lambdas before C# 10, anonymous methods have no natural type of their own. Assigning `var handler = delegate(int x) { return x * 2; };` is a compile-time error because the compiler cannot infer a delegate type from the anonymous method alone. The variable must be given an explicit type: `Func<int, int> handler = delegate(int x) { return x * 2; };` or a custom delegate type. Even in C# 10+, the natural type feature that allows `var` with lambdas does not extend to anonymous methods using the `delegate` keyword.

---

#### Gotcha 8. Removing an Anonymous Method Event Handler Requires a Stored Reference

**Concepts**
- each `delegate { }` expression creates a new delegate instance
- `-=` with a second anonymous method expression never matches the first
- must store the anonymous method in a field before subscribing
- named method or stored delegate is the only way to unsubscribe

**Answer**

When you subscribe to an event with an inline anonymous method, the delegate instance created by that expression is never stored anywhere accessible for later removal. A subsequent `-=` with a different anonymous method expression — even with the same body — creates a fresh delegate instance that does not match the subscribed one, so `Delegate.Remove` finds no match and the subscription persists silently. The only way to unsubscribe is to save the delegate in a field before calling `+=`: `EventHandler h = delegate { Handle(); }; obj.Event += h;`, then call `obj.Event -= h;` when cleanup is needed.

---

#### Gotcha 9. `params` in Anonymous Methods — Allowed but Rarely Used Correctly

**Concepts**
- anonymous method parameter list can include the `params` keyword
- must match the delegate type's `params` signature exactly
- `Func` and `Action` do not support `params` parameters
- custom delegate type required to use this feature

**Answer**

Anonymous methods can declare a `params` parameter as long as the delegate type they are assigned to also declares a `params` parameter with a matching element type. However, the generic `Func` and `Action` families do not include any overloads with `params` parameters, so this feature is only accessible when using a custom-declared delegate type. In practice, `params` in anonymous methods is rarely encountered because the use cases are narrow, and modern code tends to use lambda expressions and collection expressions instead.

---

#### Gotcha 10. Anonymous Method in Generic Context — Enclosing Type Parameters Are Available

**Concepts**
- anonymous method can use type parameters of the enclosing generic method
- the compiler-generated display class is also generic with those type parameters
- type parameter is not re-declared inside the anonymous method
- capture semantics are identical to non-generic contexts

**Answer**

An anonymous method declared inside a generic method has access to that method's type parameters and uses them correctly. The compiler makes the generated display class generic with the same type parameters, so the captured type context is preserved. For example, inside `void Process<T>(T value)`, writing `Action printer = delegate { Console.WriteLine(value.ToString()); };` correctly closes over `value` of type `T` without requiring any special syntax. This is sometimes surprising to developers who expect type parameters to be lost in anonymous method compilation, but the generated display class handles the generics transparently.

---

## Real-World Scenarios

---

## Q18. You are reviewing legacy C# 2.0 code that uses anonymous methods throughout. When should you migrate them to lambdas and when should you leave them alone?

**Concepts**
- migration cost vs benefit
- parameter-omission syntax advantage
- expression tree requirement as trigger
- automated Roslyn refactoring
- risk in behavior-identical transforms

**Answer**

The highest-value targets for migration are anonymous methods that are already passed to APIs requiring `Expression<Func<...>>` — these currently fail at compile time (or were bypassed with workarounds) and must be converted to lambdas to work correctly. The next priority is anonymous methods that participate in LINQ method chains, because lambdas read more naturally in that context and enable expression tree translation if the source is later changed to `IQueryable`. For event handlers that intentionally omit the parameter list (`delegate { Close(); }`), the benefit of migrating to `_ => Close()` or `(s, e) => Close()` is cosmetic — the behavior is identical, and the migration adds a small risk of introducing a typo or changing captured variable scope accidentally. The safest approach is to run the Roslyn IDE0039 quick-fix in bulk on a feature branch, review the diff carefully for any changes to captured variable sets, and run the test suite before merging. Leave well-tested, legacy-only code alone unless the file is being touched for another reason.

---

## Q19. A colleague has written an event handler using an anonymous method and the application is leaking memory in a long-running service. Walk through the diagnosis and fix.

**Concepts**
- heap snapshot analysis
- GC root via event invocation list
- delegate reference chain
- variable-capture workaround
- subscription lifetime alignment

**Answer**

The first diagnostic step is to take two heap snapshots with a profiler (dotMemory, PerfView, or Visual Studio Diagnostics) with a GC between them, then diff the retained object graph. Look for objects that are alive longer than expected and trace their GC roots — a common finding is that the root chain passes through a delegate instance held by an event's backing invocation list. Once you confirm the event subscription is the root, examine the subscriber code: if the anonymous method closes over `this` or any instance field of the subscriber, the delegate holds a reference to the subscriber, preventing collection. The fix is to store the anonymous delegate in a field so it can be unsubscribed in a `Dispose` or `IAsyncDisposable.DisposeAsync` implementation. If the subscriber's lifetime is shorter than the publisher's, registering in the constructor and unregistering in `Dispose` is the clean solution. For cases where explicit unsubscription is impractical (e.g., deeply composed event chains), adopt the weak event pattern using `WeakReference<T>` wrappers or the `WeakEventManager` available in WPF, or restructure ownership so the publisher does not outlive its subscribers.

---

## Q20. You are onboarding a junior developer who is confused about why their anonymous method sees a different value than they expected. Explain the capture semantics with a minimal example.

**Concepts**
- value capture misconception
- reference capture reality
- display class field sharing
- minimal repro pattern
- debugging display class in decompiler

**Answer**

The confusion almost always stems from expecting the anonymous method to snapshot the value at creation time, like taking a photograph. In reality, C# captures the storage location — the variable slot — not its current value. A minimal demonstration: `int x = 1; Action a = delegate { Console.WriteLine(x); }; x = 42; a();` prints `42`, not `1`, because the display class field for `x` was updated to `42` before the delegate ran. To show the junior developer what is actually happening, open the compiled output in a decompiler (SharpLab.io is convenient): they will see a `<>c__DisplayClass` type with a field `int x`, the assignment `displayClass.x = 1` followed by `displayClass.x = 42`, and the delegate body reading `displayClass.x`. Once the structure is visible, the behavior becomes obvious. The fix for the snapshot pattern is to copy: `int snapshot = x; Action a = delegate { Console.WriteLine(snapshot); };`. Now `snapshot` and `x` are separate display class fields, and reassigning `x` does not affect `snapshot`.

---

## Q21. Review the following code and identify problems:

```csharp
public class ButtonManager
{
    private List<Button> _buttons = new();

    public void RegisterAll(IEnumerable<Button> buttons)
    {
        int index = 0;
        foreach (var btn in buttons)
        {
            int i = index; // local copy
            btn.Click += delegate(object s, EventArgs e)
            {
                Console.WriteLine($"Button {i} clicked");
                _buttons.Add((Button)s);
            };
            index++;
        }
    }
}
```

**Issues**

| Category | Problem | Impact |
|----------|---------|--------|
| Memory leak | Each anonymous method captures `this` via `_buttons` field access, keeping `ButtonManager` alive as long as any button is alive | Subscriber outlived by publishers causes GC retention |
| Thread safety | `_buttons.Add(...)` is not thread-safe if Click fires on multiple threads | `List<T>` corruption under concurrent access |
| Unsubscription | No way to call `-=` since delegate reference is not stored | Cannot clean up when buttons are removed |
| Casting | `(Button)s` will throw `InvalidCastException` if sender is not a Button | Runtime crash on unexpected sender |

**Fix priority**
1. Store each delegate in a field or dictionary keyed by button to enable unsubscription in a cleanup method.
2. Replace `List<T>` with `ConcurrentBag<T>` or add `lock` around `_buttons.Add`.
3. Use `btn` directly instead of `(Button)s` since `btn` is already in scope and is the correct reference.
4. Implement `IDisposable` to unsubscribe all registered handlers when `ButtonManager` is disposed.
