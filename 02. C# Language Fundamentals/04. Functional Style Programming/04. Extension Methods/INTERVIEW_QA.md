# C# Extension Methods — Interview Q&A


## Table of Contents

1. [Q1. What are extension methods and how do you declare one?](#q1-what-are-extension-methods-and-how-do-you-declare-one)
2. [Q2. What are the declaration requirements for an extension method class?](#q2-what-are-the-declaration-requirements-for-an-extension-method-class)
3. [Q3. How does the compiler resolve a call when both an instance method and an extension method have the same name and compatible signatures?](#q3-how-does-the-compiler-resolve-a-call-when-both-an-instance-method-and-an-extension-method-have-the-same-name-and-compatible-signatures)
4. [Q4. Can extension methods be defined for interfaces, and why is that useful?](#q4-can-extension-methods-be-defined-for-interfaces-and-why-is-that-useful)
5. [Q5. What happens when you call an extension method on a null reference?](#q5-what-happens-when-you-call-an-extension-method-on-a-null-reference)
6. [Q6. How do extension methods support fluent API and method chaining?](#q6-how-do-extension-methods-support-fluent-api-and-method-chaining)
7. [Q7. Can extension methods access private members of the extended type?](#q7-can-extension-methods-access-private-members-of-the-extended-type)
8. [Q8. What is a generic extension method and when would you write one?](#q8-what-is-a-generic-extension-method-and-when-would-you-write-one)
9. [Q9. How does the `using` directive scope affect extension method discovery?](#q9-how-does-the-using-directive-scope-affect-extension-method-discovery)
10. [Q10. Why can extension methods be defined on sealed classes and value types?](#q10-why-can-extension-methods-be-defined-on-sealed-classes-and-value-types)
11. [Q11. How does LINQ use extension methods, and what makes `IEnumerable<T>` the right extension point?](#q11-how-does-linq-use-extension-methods-and-what-makes-ienumerablet-the-right-extension-point)
12. [Q12. What are the risks of creating extension methods on very broad types like `object`?](#q12-what-are-the-risks-of-creating-extension-methods-on-very-broad-types-like-object)
13. [Q13. An instance method added to a library you depend on silently breaks your extension method. Why does this happen and how do you detect it?](#q13-an-instance-method-added-to-a-library-you-depend-on-silently-breaks-your-extension-method-why-does-this-happen-and-how-do-you-detect-it)
14. [Q14. Why does calling an extension method via an interface reference not dispatch to the concrete type's implementation?](#q14-why-does-calling-an-extension-method-via-an-interface-reference-not-dispatch-to-the-concrete-types-implementation)
15. [Q15. Can you create an extension method for a delegate type, and what would that be useful for?](#q15-can-you-create-an-extension-method-for-a-delegate-type-and-what-would-that-be-useful-for)
16. [Q16. Why can an extension method silently be called on a null reference without throwing, and is this always desirable?](#q16-why-can-an-extension-method-silently-be-called-on-a-null-reference-without-throwing-and-is-this-always-desirable)
17. [Q17. You need to add domain-specific methods to a third-party `Result<T>` type you cannot modify. How do you design the extension method surface?](#q17-you-need-to-add-domain-specific-methods-to-a-third-party-resultt-type-you-cannot-modify-how-do-you-design-the-extension-method-surface)
18. [Q18. You are implementing a fluent configuration API for a library. Walk through the design decisions for using extension methods vs a builder class.](#q18-you-are-implementing-a-fluent-configuration-api-for-a-library-walk-through-the-design-decisions-for-using-extension-methods-vs-a-builder-class)
19. [Q19. You want to implement a custom LINQ-style operator `Batch<T>` that groups a sequence into chunks of a given size. Implement and explain the design.](#q19-you-want-to-implement-a-custom-linq-style-operator-batcht-that-groups-a-sequence-into-chunks-of-a-given-size-implement-and-explain-the-design)

---
## Foundation Questions

---

## Q1. What are extension methods and how do you declare one?

**Concepts**
- static class requirement
- static method with `this` parameter
- first parameter as extended type
- namespace import for discovery
- compiler syntactic sugar

**Answer**

Extension methods let you add new methods to an existing type without modifying its source code or using inheritance. You declare one as a static method inside a static class, where the first parameter is prefixed with the `this` keyword and specifies the type being extended. For example, `public static bool IsNullOrEmpty(this string s) => string.IsNullOrEmpty(s);` allows you to call `myString.IsNullOrEmpty()` on any string. The compiler translates the call-site dot notation into a regular static method call — `Extensions.IsNullOrEmpty(myString)` — so there is no runtime overhead or new vtable entry. The static class containing the extensions must be imported via a `using` directive; without the right namespace in scope, the method is invisible to IntelliSense and the compiler.

---

## Q2. What are the declaration requirements for an extension method class?

**Concepts**
- top-level static class
- no generic class constraint
- no nested class restriction
- public or internal visibility
- single assembly or cross-assembly export

**Answer**

The class hosting extension methods must be a non-generic static class at the top level of its namespace — you cannot nest it inside another class. It must be `static` (no constructors, no instance members). The class itself can be `public` or `internal`; an `internal` class limits extension method visibility to the declaring assembly. The method must also be `static`, and its first parameter must carry the `this` modifier. Generic type parameters are allowed on the method itself, enabling generic extension methods like `public static T[] ToArray<T>(this IEnumerable<T> source)`. In C# 13, the language team began exploring extension members (properties, operators, static methods on the extended type) as a broader feature, but as of .NET 10 the core declaration model remains the static-class, `this`-parameter pattern.

---

## Q3. How does the compiler resolve a call when both an instance method and an extension method have the same name and compatible signatures?

**Concepts**
- instance method priority rule
- extension method as fallback
- method resolution order
- overload resolution steps
- shadowing behavior

**Answer**

The C# compiler always prefers instance methods over extension methods during overload resolution. If the receiver type has an accessible instance method matching the call, the extension method is silently bypassed — it is never even considered unless no applicable instance method exists. This means an extension method can be "shadowed" by a future instance method added to the type: code that currently calls your extension method may silently start calling the new instance method after a library update, potentially changing behavior without a compile error. This is a subtle maintenance risk when extending types you do not control, such as BCL types or third-party libraries. You can always call an extension method explicitly using static syntax — `MyExtensions.IsNullOrEmpty(s)` — to bypass the resolution rule and guarantee you are calling the extension.

---

## Q4. Can extension methods be defined for interfaces, and why is that useful?

**Concepts**
- interface extension method
- default behavior for all implementors
- open/closed principle application
- mixin-like composition
- LINQ on `IEnumerable<T>`

**Answer**

Yes — the `this` parameter can be any type, including an interface. This is one of the most powerful uses of extension methods: you add behavior that applies to every implementor of the interface without modifying the interface itself or requiring implementors to change. LINQ is the canonical example — all LINQ operators (`Where`, `Select`, `OrderBy`, etc.) are extension methods on `IEnumerable<T>`, which means any class that implements `IEnumerable<T>` automatically gains the entire LINQ vocabulary. You can apply the same pattern to your own interfaces: define a minimal interface capturing the core contract, then add richer convenience methods as extensions. This keeps the interface small and easy to implement while providing a rich surface area for callers. It is a form of mixin composition in a language that supports only single inheritance.

---

## Q5. What happens when you call an extension method on a null reference?

**Concepts**
- null receiver allowed
- no NullReferenceException at call site
- guard check inside extension body
- difference from instance method
- null safety design pattern

**Answer**

Unlike instance method calls, calling an extension method on a null reference does not automatically throw a `NullReferenceException` at the call site. Because the compiler transforms the dot-notation call into a static method invocation, the null value is simply passed as the first argument. Whether an exception is thrown depends entirely on what the method body does with that argument. This means you can write null-safe extension methods that handle null receivers gracefully: `public static string OrEmpty(this string? s) => s ?? string.Empty;`. Callers can write `nullableString.OrEmpty()` without a preceding null check. Conversely, if your extension method dereferences the `this` parameter without a null check, a NullReferenceException will occur inside the method body rather than at the call site, which can confuse stack traces. The guideline is to validate the `this` parameter and throw `ArgumentNullException` explicitly when null is not a valid receiver.

---

## Q6. How do extension methods support fluent API and method chaining?

**Concepts**
- fluent interface pattern
- return type as `this` parameter type
- builder pattern composition
- readability through chaining
- immutable vs mutable receiver

**Answer**

Fluent APIs depend on each method returning an object the caller can immediately call another method on. Extension methods enable this pattern on types you do not own by returning the extended type (or a wrapper) from each extension. For example, `public static IServiceCollection AddLogging(this IServiceCollection services) { ... return services; }` allows `services.AddLogging().AddOptions().AddHttpClient()`. ASP.NET Core's `WebApplicationBuilder` API is built almost entirely on this pattern: the framework defines a minimal core and then extension packages add their own surface area. The key design decision is whether the receiver is mutated in place and returned (typical for mutable builders) or whether a new instance is created and returned (correct for immutable types like `string`). For immutable types, the extension method is purely a transformation, not a mutation.

---

## Q7. Can extension methods access private members of the extended type?

**Concepts**
- visibility rules unchanged
- no special access grant
- public/protected API surface only
- contrast with partial classes
- workaround via internal visibility

**Answer**

No — extension methods obey standard C# access rules and can only access public and protected members (in the rare case of an extension on a base class reference). The `this` parameter gives you a reference to an instance, not elevated access. This is a fundamental limitation: extension methods are syntactic sugar over static method calls, and there is no mechanism in the CLR for granting a static method access to private members of an unrelated type. If you need to add behavior that requires private member access, the options are to use partial classes (which requires owning the source), expose the necessary state through a new internal or public member, or use reflection as a last resort (brittle and performance-sensitive). In practice, well-designed extension methods work with the public contract of the type, which aligns with the open/closed principle.

---

## Q8. What is a generic extension method and when would you write one?

**Concepts**
- type parameter on extension method
- type inference from `this` parameter
- constraint clauses
- reusable utilities across types
- avoiding code duplication

**Answer**

A generic extension method includes one or more type parameters after the method name, allowing the same logic to apply to many different types while remaining type-safe. The compiler infers the type arguments from the call site — you rarely need to specify them explicitly. A practical example: `public static T ThrowIfNull<T>(this T? value, string name) where T : class { if (value is null) throw new ArgumentNullException(name); return value; }`. This single method works for any reference type. Constraint clauses (`where T : ...`) let you restrict what types are valid, enabling access to members defined on the constraint interface inside the method body. Generic extension methods are the building blocks of libraries like MoreLINQ and Polly, which extend `IEnumerable<T>`, `Task<T>`, and other generic types with additional operators.

---

## Q9. How does the `using` directive scope affect extension method discovery?

**Concepts**
- namespace scope for extension resolution
- missing `using` causes compiler error
- ambiguity with same-named extensions in different namespaces
- `global using` in .NET 6+
- deliberate namespace isolation

**Answer**

Extension methods are only visible to the compiler when the namespace containing their static class is imported with a `using` directive. If the directive is absent, the method simply does not exist from the compiler's perspective — you get a "does not contain a definition" error even though the type is technically in scope. This is by design: it prevents extension methods from polluting all code globally. Multiple extension methods with the same signature in different namespaces are independent; a file that imports both namespaces will get an ambiguity error, resolvable only by reverting to static call syntax with the fully qualified class name. In .NET 6+, `global using` statements in a central file (often `GlobalUsings.cs`) can make high-value extension namespaces available project-wide without per-file imports, which is the default behavior for namespaces like `System.Linq` in the default project templates.

---

## Q10. Why can extension methods be defined on sealed classes and value types?

**Concepts**
- sealed class extensibility
- value type (struct) extension
- no inheritance required
- boxing consideration for struct extensions
- extension method as alternative to inheritance

**Answer**

Extension methods work independently of the type hierarchy — they require no inheritance relationship between the extending code and the extended type. This makes them ideal for sealed classes, which cannot be subclassed, and for value types (structs), which have limited inheritance. For example, you can add domain-specific helpers to `string` (sealed), `int`, `DateTime` (struct), or any third-party sealed class. When extending a struct, be aware that the `this` parameter receives a copy of the struct value (by value), not the original. Mutating fields in an extension method on a struct will not affect the original value at the call site unless you declare the parameter as `ref this` (available as an experimental feature in C# for `ref` extension methods). This is consistent with how structs behave generally and should guide you toward designing extension methods on value types as pure transformations rather than mutations.

---

## Q11. How does LINQ use extension methods, and what makes `IEnumerable<T>` the right extension point?

**Concepts**
- `System.Linq` namespace
- extension methods on `IEnumerable<T>`
- deferred execution via iterator
- composability through chaining
- `IQueryable<T>` variant for remote execution

**Answer**

LINQ is implemented entirely as extension methods in the `System.Linq` namespace on `IEnumerable<T>` and `IQueryable<T>`. Each operator (`Where`, `Select`, `GroupBy`, etc.) is a static method that accepts and returns `IEnumerable<T>`, enabling fluent chaining. Because `IEnumerable<T>` is the most general collection abstraction in .NET, every array, `List<T>`, `Queue<T>`, and custom collection automatically supports LINQ. The methods use deferred execution — `Where` and `Select` return `IEnumerable<T>` wrappers that yield elements lazily when iterated — so building a LINQ pipeline does no work until the result is consumed. The parallel `IQueryable<T>` surface accepts `Expression<Func<T, bool>>` lambdas instead of compiled delegates, allowing providers like Entity Framework to translate the expression tree to SQL. This two-surface design (objects vs database) is only possible because extension methods allow the two contracts to share the same dot-notation call site.

---

## Q12. What are the risks of creating extension methods on very broad types like `object`?

**Concepts**
- universal applicability
- IntelliSense pollution
- namespace contamination
- unintended method availability
- discoverability vs clutter tradeoff

**Answer**

Extending `object` means the method appears on every single type in the system — strings, ints, custom classes, enums, delegates, everything. While this can be convenient for true cross-cutting concerns (like a null-safe `.ToJson()` or `.Dump()` for debugging), it pollutes IntelliSense for every type in every file that imports the namespace, making the API harder to navigate. It also increases the risk that the extension method name collides with an instance method added to some type in a future version of the library, silently changing behavior. The pragmatic rule is to extend `object` only for genuinely universal utilities — debugging helpers, logging adapters, or infrastructure-level serialization — and to place them in a namespace that developers opt into deliberately rather than importing by default. For anything more targeted, extend the most specific type or interface that makes sense.

---

## Gotchas — Extension Methods (Interview Traps)

---

#### Gotcha 1. Extension Method on a Null Receiver Does Not Throw at the Call Site

**Concepts**
- compiler translates dot-notation to a static method call
- null receiver is passed as the first argument with no null check
- `NullReferenceException` occurs inside the body, not at the call site
- explicitly throw `ArgumentNullException` for non-null-safe extensions

**Answer**

Because the compiler rewrites `obj.Ext()` as `Extensions.Ext(obj)`, a null `obj` is simply passed as the first argument — no `NullReferenceException` fires at the call site as it would for an instance method call. Whether the method blows up depends entirely on whether the body dereferences the `this` parameter. For extensions explicitly designed to handle null (such as a null-safe `OrEmpty` on string), this behavior is a feature. For all other extensions, explicitly check and throw `ArgumentNullException` at the top of the method to produce a clear failure message pointing to the right location.

---

#### Gotcha 2. Instance Method Always Wins Over an Extension Method with the Same Name

**Concepts**
- C# overload resolution prefers instance methods unconditionally
- extension method is a fallback when no instance method matches
- a library update adding an instance method silently shadows your extension
- static call syntax `MyExtensions.Method(obj)` bypasses the priority rule

**Answer**

C# resolves instance methods before extension methods during overload resolution. If the receiver type or any of its base types has an accessible instance method whose signature is compatible with the call, the extension method is never even considered. This means a third-party library that ships a new instance method with the same name as your extension will silently start winning after an upgrade, potentially changing behavior without a compile error or warning. The only reliable defense is to call the extension via its static form: `MyExtensions.DoSomething(obj)`.

---

#### Gotcha 3. Extension Methods Are Invisible Without the Correct `using` Namespace

**Concepts**
- extension method discovery requires importing the declaring namespace
- missing `using` causes "does not contain a definition" compile error
- `global using` in .NET 6+ can make critical extension namespaces project-wide
- namespace choice affects how broadly extensions are available

**Answer**

The compiler only sees extension methods whose containing static class is in a namespace that the current compilation unit has imported with a `using` directive. Without the right namespace imported, the method simply does not exist from the compiler's perspective and you get a "does not contain a definition" error even though the method is present in the referenced assembly. This is intentional — it prevents extension methods from polluting every file automatically — but it is a common source of confusion, especially when moving code between projects or when a new developer is unfamiliar with which namespace houses the extensions.

---

#### Gotcha 4. Extension Methods Cannot Override Interface Default Implementations

**Concepts**
- default interface methods (C# 8+) provide polymorphic runtime behavior
- extension methods use static dispatch resolved at compile time
- extension method on an interface does not override the default implementation
- choose default interface method when behavior must vary by concrete implementation

**Answer**

Default interface methods introduced in C# 8 allow an interface to provide a method body that concrete implementors inherit polymorphically. Extension methods on an interface look similar but are fundamentally different: they are resolved statically at compile time based on the declared type of the variable, not the runtime type. An extension method defined on `IAnimal` does not override or shadow a default implementation of the same method on `IAnimal` — instead, the compiler applies normal instance-method priority and calls the default implementation if it exists. Use default interface methods when you need the behavior to vary by concrete implementation; use extension methods for utility operations that work identically for all implementors.

---

#### Gotcha 5. Extension Methods on `object` Apply to Everything — Use Sparingly

**Concepts**
- `object` extension pollutes IntelliSense for every type in scope
- name collision risk with instance methods on every future type
- appropriate only for universal cross-cutting concerns
- place in an opt-in namespace, never imported by default

**Answer**

Extending `object` means the method appears on every single type in the system as soon as the namespace is imported. This pollutes IntelliSense with methods that are irrelevant to most types, increases the risk that a future instance method addition to any type in the codebase silently shadows your extension, and makes the API surface confusing to new developers. Restrict `object` extensions to genuinely universal utilities — null-safe serialization helpers, deep-clone methods, debugging inspectors — and always place them in a dedicated namespace that developers opt into explicitly rather than one that is imported globally.

---

#### Gotcha 6. Extension Method Namespace Proximity Affects Resolution Priority

**Concepts**
- closer namespace takes priority when multiple extension methods match
- extension in the same namespace as the call site wins over one in a different namespace
- ambiguity error when two equally close extensions match
- explicit static call syntax resolves ambiguity definitively

**Answer**

When multiple extension methods with the same name and compatible signature are in scope, the compiler uses namespace proximity to break the tie: an extension declared in the same namespace as the call site takes priority over one in an outer or unrelated namespace. If two equally close extensions match, the compiler reports an ambiguity error rather than guessing. This proximity rule is rarely documented and surprises developers who assume all imported extensions are equal. When naming extension methods, use distinctive names that are unlikely to conflict with extensions from other libraries imported at the same time.

---

#### Gotcha 7. Removing `using System.Linq` Breaks All LINQ

**Concepts**
- all standard LINQ operators are extension methods in `System.Linq`
- removing or forgetting the `using` directive removes the entire LINQ vocabulary
- "does not contain a definition" errors appear on `Where`, `Select`, `OrderBy`, etc.
- `global using System.Linq` is added by default in modern SDK project templates

**Answer**

Every standard LINQ operator — `Where`, `Select`, `OrderBy`, `GroupBy`, and all others — is defined as an extension method on `IEnumerable<T>` and `IQueryable<T>` in the `System.Linq` namespace. Removing the `using System.Linq` directive from a file causes every one of those calls to produce "does not contain a definition" compile errors, which can look alarming until the cause is identified. Modern SDK-style projects add `global using System.Linq` automatically, but older projects or files migrated from legacy code sometimes lose this directive and appear to break LINQ entirely.

---

#### Gotcha 8. Extension Methods Cannot Access Private Members of the Extended Type

**Concepts**
- extension methods obey standard C# visibility rules
- only public and protected members of the extended type are accessible
- no special access grant — the `this` parameter is just a reference
- partial classes required for true private member access on types you own

**Answer**

Extension methods are, at the CLR level, ordinary static methods in a different class. They receive a reference to the extended object as their first parameter but are granted no special access beyond the object's public (and for base-type references, protected) surface area. There is no mechanism to elevate an extension method's access to private members, unlike partial class methods which live inside the same type declaration. If you need to add behavior that requires private member access to a type you own, use a partial class; if you cannot modify the type, expose the necessary state through a new public or internal member.

---

#### Gotcha 9. Extension Methods Do Not Change the Type Hierarchy

**Concepts**
- extension methods add callable syntax, not an inheritance relationship
- the extended type's runtime type is unchanged
- `is`/`as` operators and `typeof` checks are unaffected
- useful for sealed types because no subclassing is required

**Answer**

Extension methods add methods that are callable on a type without modifying the type, its base classes, or its interfaces. An extension method on a sealed class does not create a subtype, does not appear in reflection's method tables on the extended type, and does not affect any `is`/`as` type checks. This is intentional — extension methods are a syntactic convenience, not a type-system feature. Code that uses reflection to enumerate methods on a type will not see extension methods, and extension methods cannot be used to satisfy an interface contract even if their signature matches.

---

#### Gotcha 10. Extension Method on `IEnumerable<T>` Uses Static Dispatch — No Polymorphism

**Concepts**
- extension method called on the declared (compile-time) type, not the runtime type
- no virtual dispatch through the extension mechanism
- `List<T>` variable vs `IEnumerable<T>` variable: declared type determines which extension fires
- interface default methods provide polymorphism; extension methods do not

**Answer**

When you call an extension method, the compiler resolves it based on the declared type of the variable, not its runtime type. If a variable is declared as `IEnumerable<T>`, the extension method defined on `IEnumerable<T>` is called even if the runtime object is a `List<T>` or a custom collection with its own overriding behavior. There is no dynamic dispatch through extension methods the way there is through virtual instance methods. If you need behavior that varies by concrete collection type, define an interface method (or a default interface method in C# 8+) rather than relying on extension method overloading.

---

## Real-World Scenarios

---

## Q17. You need to add domain-specific methods to a third-party `Result<T>` type you cannot modify. How do you design the extension method surface?

**Concepts**
- sealed third-party type extensibility
- cohesive extension class per domain
- namespace strategy
- fluent chaining return type
- avoiding extension sprawl

**Answer**

Start by identifying the cohesive group of operations your domain needs: mapping successes, folding errors, converting to DTOs, etc. Create one or two static classes named descriptively — `ResultExtensions` or `OrderResultExtensions` — placed in a namespace that mirrors your domain layer. Each extension method should return the same or a compatible type to enable chaining: `public static Result<TOut> Map<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> mapper)`. Group extensions by concern rather than cramming everything into one class; this keeps file size manageable and makes it obvious which `using` to add. Avoid defining extensions in the `System` namespace or the third-party library's own namespace — put them in your domain namespace so they are opt-in. Write unit tests for each extension as you would any other method; the static nature does not reduce testability since you can call them directly through both the dot notation and the static syntax.

---

## Q18. You are implementing a fluent configuration API for a library. Walk through the design decisions for using extension methods vs a builder class.

**Concepts**
- extension method fluent API
- builder pattern comparison
- state management difference
- discoverability through IntelliSense
- composition vs inheritance

**Answer**

A builder class centralizes state and exposes chainable methods directly on the class, making it easy to carry configuration across calls: `new Builder().WithTimeout(5).WithRetries(3).Build()`. Extension methods on a configuration object push the state into the object itself and add the fluent surface externally, which is better when you want different assemblies to contribute configuration methods without modifying the central class — exactly the model ASP.NET Core uses with `IServiceCollection`. Choose extension methods when the core object is a minimal interface or a simple data bag and contributors should be able to add their own configuration surface. Choose a concrete builder class when the state transitions are complex, need validation, or form a clear finite state machine. In practice, many libraries use both: a concrete builder for core settings and extension methods for optional, pluggable concerns added by separate packages.

---

## Q19. You want to implement a custom LINQ-style operator `Batch<T>` that groups a sequence into chunks of a given size. Implement and explain the design.

**Concepts**
- `IEnumerable<T>` extension method
- deferred execution with `yield return`
- generic method with constraint
- argument validation strategy
- composability with existing LINQ operators

**Answer**

The implementation lives in a static class and extends `IEnumerable<T>`:

```csharp
public static class EnumerableExtensions
{
    public static IEnumerable<IReadOnlyList<T>> Batch<T>(
        this IEnumerable<T> source, int size)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfLessThan(size, 1);

        var batch = new List<T>(size);
        foreach (var item in source)
        {
            batch.Add(item);
            if (batch.Count == size)
            {
                yield return batch.AsReadOnly();
                batch = new List<T>(size);
            }
        }
        if (batch.Count > 0)
            yield return batch.AsReadOnly();
    }
}
```

Argument validation is performed eagerly before the first `yield` so callers get exceptions immediately rather than on first iteration. The method returns `IReadOnlyList<T>` rather than `IEnumerable<T>` so the caller can index into each batch without a second enumeration. Because it uses `yield return`, execution is deferred and memory usage is bounded to one batch at a time regardless of source size. The method composes naturally with other LINQ operators: `source.Batch(100).Select(batch => ProcessBatch(batch))` reads cleanly and works with any `IEnumerable<T>`.
