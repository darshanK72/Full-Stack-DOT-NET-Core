# C# Anonymous Methods — Interview Q&A

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

## Gotchas

---

## Q13. Why can you not use `yield return` inside an anonymous method?

**Concepts**
- iterator method restriction
- state machine generation requirement
- anonymous method compilation model
- workaround using local function

**Answer**

`yield return` transforms a method into a state machine, and the C# compiler requires the entire method to be designated as an iterator — a constraint that anonymous methods cannot satisfy because they have no return type declaration that the compiler can change to `IEnumerable<T>`. Attempting to use `yield return` inside a `delegate { }` block causes a compile error. The idiomatic workaround before C# 7 was to extract a named iterator method. Since C# 7, local functions provide a cleaner in-place solution: `IEnumerable<int> GetItems() { yield return 1; yield return 2; }` defined inside the outer method captures the same enclosing variables without needing a class-level method. If you truly need an anonymous-method-like syntax, LINQ's `Select` and `SelectMany` often replace what a yield-based closure would have done.

---

## Q14. What happens when two anonymous methods capture the same outer variable?

**Concepts**
- shared display class instance
- aliased field access
- unintended mutation coupling
- multi-delegate interaction

**Answer**

When multiple anonymous methods in the same scope capture the same outer variable, the compiler places all of them into the same display class instance. This means they all share the same field representing that variable, and any write performed through one anonymous method is immediately visible to all others. This coupling is often unintentional — a developer writes two seemingly independent closures, tests them in isolation, and is surprised when invoking one changes the observable state seen by the other. The fix is to ensure that each closure that needs an independent copy of a value captures a separate local: `var a1 = val; var a2 = val; Action f1 = delegate { Use(a1); }; Action f2 = delegate { Use(a2); };`. The two locals get separate display class fields and mutations are isolated.

---

## Q15. Why does an event handler using an anonymous method create a memory leak risk?

**Concepts**
- event subscription lifetime
- publisher–subscriber lifetime coupling
- GC root via delegate reference
- inability to unsubscribe anonymous delegate
- weak event pattern

**Answer**

When you subscribe to an event with an anonymous method and never unsubscribe, the event's invocation list holds a delegate that in turn holds a reference to the display class (or the subscriber object captured inside it). This keeps the subscriber alive in memory as long as the event source is alive, even if the subscriber has logically finished its work. In long-lived objects such as singletons, static event sources, or application-lifetime services, this becomes a genuine memory leak pattern. Because you have no stored reference to the anonymous delegate, you cannot call `-=` on it. The fix is to store the delegate in a variable so you can unsubscribe, use a weak event pattern (via `WeakReference` or the WeakEventManager in WPF), or design the subscription lifetime to match the subscriber's lifetime explicitly.

---

## Q16. Can an anonymous method be used where an `Expression<Func<T>>` is expected?

**Concepts**
- expression tree vs delegate
- compile-time representation
- lambda-only conversion
- LINQ IQueryable requirement

**Answer**

No. Expression trees require the compiler to capture the code as a data structure (an abstract syntax tree) rather than compiling it to IL. This analysis happens only when a lambda expression is assigned to an `Expression<TDelegate>` type. Anonymous methods use the `delegate` keyword and are always compiled directly to IL; the compiler has no mechanism to treat them as expression trees. Practically, this means anonymous methods cannot be passed to LINQ providers like Entity Framework's `IQueryable<T>` extension methods, which use `Expression<Func<T, bool>>` parameters to translate predicates to SQL. You must use lambda syntax there. This is a concrete reason why modern C# code overwhelmingly prefers lambdas: they are a strict superset of anonymous methods for functional-style scenarios.

---

## Q17. Is it safe to use an anonymous method inside a multi-threaded loop?

**Concepts**
- shared mutable captured variable
- race condition
- display class field access
- `Interlocked` or lock requirement
- thread-local copy pattern

**Answer**

Not without explicit synchronization. If an anonymous method captures a variable and multiple threads invoke delegates that close over it concurrently, all threads access the same display class field without any memory ordering guarantees. This is a classic data race. A common mistake is capturing a loop counter in a multi-threaded scenario: `for (int i = 0; i < 10; i++) { ThreadPool.QueueUserWorkItem(delegate { Console.WriteLine(i); }); }`. Here all delegates share the same `i` field, and by the time they execute, `i` may have already reached 10. The fix is to copy the loop variable into a local before the delegate: `int copy = i; ThreadPool.QueueUserWorkItem(delegate { Console.WriteLine(copy); });`. For shared mutable state across threads, use `Interlocked` operations or `lock` on a dedicated object.

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
