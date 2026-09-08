# C# Constructors & Method Overloading — Interview Q&A


## Table of Contents

1. [Q1. What is a constructor in C# and what is its primary purpose?](#q1-what-is-a-constructor-in-c-and-what-is-its-primary-purpose)
2. [Q2. What is the difference between a default constructor and a parameterized constructor?](#q2-what-is-the-difference-between-a-default-constructor-and-a-parameterized-constructor)
3. [Q3. What is constructor chaining with `this(...)`, and why is it used?](#q3-what-is-constructor-chaining-with-this-and-why-is-it-used)
4. [Q4. What is the difference between `: this(...)` and `: base(...)`?](#q4-what-is-the-difference-between-this-and-base)
5. [Q5. What is a static constructor, when does it run, and what are its restrictions?](#q5-what-is-a-static-constructor-when-does-it-run-and-what-are-its-restrictions)
6. [Q6. What is a copy constructor pattern in C#, and when would you implement one?](#q6-what-is-a-copy-constructor-pattern-in-c-and-when-would-you-implement-one)
7. [Q7. What are primary constructors in C# 12, and how do they differ from traditional constructors?](#q7-what-are-primary-constructors-in-c-12-and-how-do-they-differ-from-traditional-constructors)
8. [Q8. What is method overloading, and what rules determine which overload is selected?](#q8-what-is-method-overloading-and-what-rules-determine-which-overload-is-selected)
9. [Q9. How do optional parameters interact with method overloading, and what problems can arise?](#q9-how-do-optional-parameters-interact-with-method-overloading-and-what-problems-can-arise)
10. [Q10. What is the `params` keyword and how does it affect overload resolution?](#q10-what-is-the-params-keyword-and-how-does-it-affect-overload-resolution)
11. [Q11. What is the difference between method overloading and method overriding?](#q11-what-is-the-difference-between-method-overloading-and-method-overriding)
12. [Q12. How does constructor execution order work in an inheritance chain?](#q12-how-does-constructor-execution-order-work-in-an-inheritance-chain)
13. [Q13. What are named arguments, and how do they interact with overloads?](#q13-what-are-named-arguments-and-how-do-they-interact-with-overloads)
14. [Q14. Why should you avoid calling virtual members from constructors?](#q14-why-should-you-avoid-calling-virtual-members-from-constructors)
15. [Q15. What is the role of the `new` keyword in method hiding vs constructor invocation?](#q15-what-is-the-role-of-the-new-keyword-in-method-hiding-vs-constructor-invocation)
16. [Q16. A convenience constructor chains via `: this(sku, 1)` to a parameterized constructor that validates SKU. Why might validation still be bypassed?](#q16-a-convenience-constructor-chains-via-thissku-1-to-a-parameterized-constructor-that-validates-sku-why-might-validation-still-be-bypassed)
17. [Q17. Why does adding a default value to an existing method parameter break binary compatibility in a published NuGet package?](#q17-why-does-adding-a-default-value-to-an-existing-method-parameter-break-binary-compatibility-in-a-published-nuget-package)
18. [Q18. What happens when two overloads are equally applicable — for example, `Foo(int, double)` and `Foo(double, int)` — and you call `Foo(1, 2)`?](#q18-what-happens-when-two-overloads-are-equally-applicable-for-example-fooint-double-and-foodouble-int-and-you-call-foo1-2)
19. [Q19. In C# 12 primary constructors, does the compiler generate backing fields automatically for the parameters?](#q19-in-c-12-primary-constructors-does-the-compiler-generate-backing-fields-automatically-for-the-parameters)
20. [Q20. Why does a static constructor have no access modifier, and what if it throws?](#q20-why-does-a-static-constructor-have-no-access-modifier-and-what-if-it-throws)
21. [Q21. Can you overload operators in C#, and what rules apply?](#q21-can-you-overload-operators-in-c-and-what-rules-apply)
22. [Q22. A teammate chains constructors in `OrderLine` but QA reports that empty SKUs reach production. Review this code and identify the problem.](#q22-a-teammate-chains-constructors-in-orderline-but-qa-reports-that-empty-skus-reach-production-review-this-code-and-identify-the-problem)
23. [Q23. A .NET 10 service uses a C# 12 primary constructor for `StockReceipt`. Unit tests expecting `ArgumentException` on null SKU throw `NullReferenceException` instead. What is the initialization order problem?](#q23-a-net-10-service-uses-a-c-12-primary-constructor-for-stockreceipt-unit-tests-expecting-argumentexception-on-null-sku-throw-nullreferenceexception-instead-what-is-the-initialization-order-problem)
24. [Q24. Overload ambiguity CS0121 is reported on `PricingHelper.LineTotal(3, 2.49m, 0.10m)`. How do you diagnose and fix it?](#q24-overload-ambiguity-cs0121-is-reported-on-pricinghelperlinetotal3-249m-010m-how-do-you-diagnose-and-fix-it)
25. [Q25. A DI-based ASP.NET Core service still constructs dependencies manually inside its constructor. What breaks, and what is the correct pattern?](#q25-a-di-based-aspnet-core-service-still-constructs-dependencies-manually-inside-its-constructor-what-breaks-and-what-is-the-correct-pattern)
26. [Q26. How would you design a `ProductFactory` that enforces invariants through constructors while keeping a clean separation from ASP.NET Core request deserialization?](#q26-how-would-you-design-a-productfactory-that-enforces-invariants-through-constructors-while-keeping-a-clean-separation-from-aspnet-core-request-deserialization)

---
## Foundation Questions

---

## Q1. What is a constructor in C# and what is its primary purpose?

**Concepts**
- Special method invoked at object creation
- Same name as the class, no return type
- Initializes object state
- Called by the `new` keyword
- Can be overloaded

**Answer**

A constructor is a special method that runs automatically when an object is created with `new`. Its primary purpose is to put the object into a valid, consistent initial state before any other code can use it. Unlike regular methods, a constructor has the same name as the class, has no return type (not even `void`), and cannot be called explicitly after construction. If you define no constructor at all, the C# compiler silently generates a public parameterless default constructor that zero-initializes all fields. The moment you declare any constructor yourself, the compiler stops generating that default, which often surprises developers who then receive CS7036 when calling `new MyClass()` without arguments.

---

## Q2. What is the difference between a default constructor and a parameterized constructor?

**Concepts**
- Default constructor (parameterless)
- Parameterized constructor
- Compiler-generated vs explicit
- Field initialization defaults
- Object validity guarantee

**Answer**

A default constructor takes no parameters and is either compiler-generated (when no constructor is declared) or explicitly written. It usually initializes fields to sensible defaults or zero-equivalent values. A parameterized constructor accepts arguments that callers supply at construction time, enabling the object to be fully initialized in one step. The key practical difference is that a parameterized constructor lets you enforce invariants at creation: you can validate arguments and throw before the object escapes. A compiler-generated default constructor cannot do that. In domains where every object must have at least an identity — an `OrderLine` with a SKU, a `BankAccount` with an account number — always write an explicit parameterized constructor so invalid objects cannot be created at all.

---

## Q3. What is constructor chaining with `this(...)`, and why is it used?

**Concepts**
- `this(...)` initializer
- Constructor delegation
- DRY principle in initialization
- Execution order (delegated-to runs first)
- Avoiding duplicated validation logic

**Answer**

Constructor chaining allows one constructor to call another constructor in the same class using `: this(args)` before the body executes. The delegated-to constructor runs first, then control returns to the body of the calling constructor. The primary benefit is keeping validation and initialization logic in one place — usually the most specific (most-parameterized) constructor — while convenience overloads simply forward with sensible defaults. For example, a no-argument `OrderLine()` can chain to `OrderLine("MISC", 1)`, guaranteeing the same validation runs regardless of which overload the caller picks. Without chaining, each constructor body would duplicate the guard clauses, and future changes to validation rules must be applied everywhere — a maintenance liability.

---

## Q4. What is the difference between `: this(...)` and `: base(...)`?

**Concepts**
- `this(...)` — delegates to another constructor in the same class
- `base(...)` — delegates to a constructor in the parent class
- Mutually exclusive in a single constructor declaration
- Execution order in inheritance
- CS2506 compiler error for chaining both

**Answer**

`: this(...)` delegates to a different constructor within the same class, while `: base(...)` delegates to a constructor in the direct base class. A single constructor can use one or the other but never both in the same declaration — the C# compiler issues CS2506 if you try. When using `: base(...)`, the base class constructor runs first, completing full initialization of the inherited state, and then the derived constructor body runs. This ordering matters because the derived constructor body can safely read base properties. A common mistake is omitting `: base(...)` in a derived class when the base has no parameterless constructor; the compiler then reports CS7036. In deep hierarchies, every constructor in the chain executes in base-to-derived order before any derived body code runs.

---

## Q5. What is a static constructor, when does it run, and what are its restrictions?

**Concepts**
- `static MyClass()` syntax
- Type initializer (cctor)
- Runs once, before first use
- No access modifiers or parameters allowed
- `TypeInitializationException` wrapping

**Answer**

A static constructor (type initializer) is declared with the `static` keyword, takes no parameters, and has no access modifier. The CLR guarantees it runs exactly once — before the first instance is created or any static member is accessed — and it is thread-safe by design. You cannot call it explicitly. Its main use is initializing complex static state that cannot be expressed in a field initializer, such as reading a config file or constructing a static lookup table. A critical pitfall is that any unhandled exception inside a static constructor is wrapped in a `TypeInitializationException` and the type becomes permanently unusable for the lifetime of the `AppDomain`. This makes static constructors risky for I/O operations; prefer lazy initialization patterns or explicit initialization methods when failure must be recoverable.

---

## Q6. What is a copy constructor pattern in C#, and when would you implement one?

**Concepts**
- Defensive copy idiom
- No language-enforced copy constructor
- Shallow vs deep copy
- Value semantics on reference types
- Record types as an alternative

**Answer**

C# has no built-in copy constructor mechanism the way C++ does, but the pattern is implemented by convention as a constructor that accepts an instance of the same type: `public Product(Product other)`. It is used when you need a new object with the same field values as an existing object, particularly when the type holds mutable reference-type fields that should not be shared between copies. A shallow copy (simply assigning reference fields) leaves both instances sharing the same nested object, which can lead to unexpected mutations. A deep copy recursively creates new instances of any mutable reference fields. In .NET 10, records provide built-in shallow-copy semantics via `with` expressions, making explicit copy constructors less necessary for simple DTOs. For complex domain objects with mutable collections, an explicit copy constructor remains the cleanest approach.

---

## Q7. What are primary constructors in C# 12, and how do they differ from traditional constructors?

**Concepts**
- Primary constructor syntax (`class Foo(int x)`)
- Parameters as class-scoped identifiers
- Field generation not automatic
- `record` primary constructors vs class primary constructors
- Initialization and invariant placement

**Answer**

Introduced in C# 12, primary constructors place constructor parameters directly in the class declaration: `public class Service(ILogger logger)`. The parameters are in scope throughout the class body, but unlike records, the compiler does not automatically generate backing properties or fields. If you need to store a primary constructor parameter beyond initialization, you must capture it into a field or property yourself, e.g. `private readonly ILogger _logger = logger;`. This is a meaningful difference from record primary constructors, which do generate public init-only properties automatically. Primary constructors reduce ceremony for dependency injection scenarios where parameters flow directly into readonly fields. Guard clauses can be placed inline in field initializers (`private readonly ILogger _logger = logger ?? throw new ArgumentNullException(...)`), though complex validation may still call for a traditional constructor body using the explicit approach.

---

## Q8. What is method overloading, and what rules determine which overload is selected?

**Concepts**
- Same method name, different parameter signatures
- Compile-time (static) dispatch
- Parameter count, types, and order as differentiators
- Overload resolution algorithm
- Return type does NOT differentiate overloads

**Answer**

Method overloading allows multiple methods with the same name to coexist in a class, provided their parameter lists differ in count, types, or order. The compiler selects the best matching overload at compile time through overload resolution — a process that looks for an exact match first, then considers implicit conversions. Return type alone cannot distinguish overloads; two methods that differ only in return type cause a compile error. The resolution algorithm scores candidates by how precisely each parameter matches the argument type: an exact match beats an implicit widening conversion. When two overloads are equally applicable (e.g., `LineTotal(int, decimal, decimal)` matching both a three-parameter and a four-parameter overload with a default), the compiler emits CS0121. Optional parameters can interact badly with overloading, creating ambiguous call sites that are valid to write but confusing to resolve.

---

## Q9. How do optional parameters interact with method overloading, and what problems can arise?

**Concepts**
- Default parameter values at call site
- Compile-time argument substitution
- Overload resolution priority
- CS0121 ambiguity error
- Versioning brittleness

**Answer**

Optional parameters (defined with default values like `decimal rate = 0m`) let callers omit trailing arguments. When combined with overloads, they can create ambiguity. If `LineTotal(int, decimal, decimal)` with a default third argument and `LineTotal(int, decimal, decimal, decimal)` both exist, a call with three arguments is ambiguous and the compiler reports CS0121. Overload resolution prefers an overload where all provided arguments exactly satisfy the parameter list without using defaults, but when two candidates both satisfy the call equally, it fails. A subtler issue is binary compatibility: changing a default value in a published library does not update callers compiled against the old version because default values are baked into the caller's IL at compile time. For APIs that must evolve, explicit overloads are safer than optional parameters.

---

## Q10. What is the `params` keyword and how does it affect overload resolution?

**Concepts**
- Variable-length argument array
- `params` must be last parameter
- Single `params` per method
- Expanded form vs normal form in resolution
- Heap allocation implications

**Answer**

The `params` keyword allows a method to accept a variable number of arguments of the same type, which the compiler packages into an array: `public void Log(params string[] messages)`. Callers can pass comma-separated values or an existing array. In overload resolution, the compiler first tries the "normal form" (treating `params` as an array parameter requiring an array argument) and then the "expanded form" (where individual arguments are collected). The expanded form is only chosen when no better candidate exists in normal form. A practical consequence is that calling `Log("a", "b")` creates a `string[]` array on the heap each time, which can matter in tight loops. In .NET 10, `params Span<T>` is supported for span-based overloads that avoid heap allocation when the compiler can stack-allocate the collection.

---

## Q11. What is the difference between method overloading and method overriding?

**Concepts**
- Overloading — compile-time, same class or subclass, different signatures
- Overriding — runtime polymorphism, `virtual`/`override` pair
- `new` keyword for method hiding
- Static dispatch vs dynamic dispatch
- LSP implications of overriding

**Answer**

Overloading is a compile-time feature where multiple methods share a name but differ in signature within the same class (or across a class hierarchy). The correct method is chosen by the compiler based on argument types at the call site. Overriding is a runtime feature that changes the behavior of a `virtual` base class method in a derived class using the `override` keyword. The CLR dispatches the call at runtime through a vtable lookup, selecting the most derived implementation even when the reference type is the base. This is the mechanism behind polymorphism. Confusing the two leads to method hiding — using `new` instead of `override` — where calling through a base reference invokes the base implementation instead of the derived one, silently breaking polymorphic behavior.

---

## Q12. How does constructor execution order work in an inheritance chain?

**Concepts**
- Base constructors run before derived constructors
- Field initializers run before constructor body
- Static constructors run before instance constructors
- `base(...)` call initiating the chain
- Virtual member calls in constructors as a hazard

**Answer**

When you create an instance of a derived class, execution flows in a strict order. First, field initializers in the derived class run top-to-bottom. Then the `: base(...)` call (explicit or implicit) triggers the base class's field initializers and constructor body. After the base constructor completes, the derived constructor body executes. For a multi-level hierarchy (A → B → C), this means A's constructor finishes before B's, which finishes before C's. Static constructors for each type in the chain run before the first instance of that type is created, following the same base-to-derived order. A dangerous pattern is calling virtual methods from within a constructor: the most derived override will be invoked even though the derived object's constructor has not yet run, potentially reading uninitialized fields.

---

## Q13. What are named arguments, and how do they interact with overloads?

**Concepts**
- `name: value` argument syntax
- Order-independent passing
- Clarifying intent at call site
- Interaction with optional parameters
- Overload selection is still by parameter types

**Answer**

Named arguments let you specify which parameter receives each argument by name rather than by position: `Product.Create(name: "Bolt", unitPrice: 1.99m)`. This improves readability for methods with many parameters of the same type where positional order is easy to misread. Named arguments can be mixed with positional ones, provided the positional arguments come first. They also let you skip optional parameters in the middle of a list without providing placeholders. Overload selection still happens based on the parameter types implied by the named arguments; named arguments do not create new overloads. One subtlety is that renaming a parameter in a published API becomes a breaking change if consumers use named-argument syntax, so parameter names are part of the public contract for such APIs.

---

## Q14. Why should you avoid calling virtual members from constructors?

**Concepts**
- Constructor execution order
- Virtual dispatch resolves to most-derived type
- Derived class fields not yet initialized
- Uninitialized state read by overridden method
- Defensive pattern: call only `private`/`sealed` members

**Answer**

Calling a virtual method from a constructor is dangerous because the vtable dispatch resolves to the most derived override even though the derived class's constructor body has not yet run. If that override reads a field defined in the derived class, it reads a zero or null value — the field has been allocated but not yet initialized by the derived constructor. This can produce subtle bugs: the call appears to succeed but produces incorrect output or throws a NullReferenceException depending on what the derived field holds. The pattern is hard to detect in code review because the base class cannot know what derived classes will do with the virtual call. The safe rule is: from within a constructor, only call `private` methods or `sealed` methods — neither of which can be overridden. Framework analyzers such as CA2214 flag virtual calls in constructors.

---

## Q15. What is the role of the `new` keyword in method hiding vs constructor invocation?

**Concepts**
- `new` for object creation
- `new` modifier for method hiding
- Hiding vs overriding distinction
- Compile-time reference type determines dispatch
- Compiler warning CS0108

**Answer**

The `new` keyword serves two unrelated purposes in C#. In `new Product(...)` it invokes a constructor to allocate and initialize an object. As a method modifier (`public new string GetBadgeCode()`), it intentionally hides an inherited member with the same name, suppressing the CS0108 compiler warning. Method hiding is distinct from overriding: when a method is hidden with `new`, calling through a base-type reference invokes the base implementation, not the derived one. This is static dispatch, not dynamic. Hiding is occasionally legitimate — for example, when a derived type needs a method with the same name but an incompatible contract — but more often it is a mistake made by developers who forgot to add `virtual` to the base method. Overriding with `override` achieves polymorphism; hiding with `new` does not.

---

## Gotchas — Constructors & Method Overloading (Interview Traps)

---

#### Gotcha 1. Constructor chaining with this() runs the chained constructor to completion before the calling body executes

**Concepts**
- `this()` chain executes first
- Then the calling constructor body
- `base()` similarly runs base first
- Field initializers run before the first `this()` chain constructor

**Answer**

When a constructor uses `this(arg)` to call another overload, that entire overload executes — including its own body — before control returns to the calling constructor's body. Field initializers run before any constructor in the chain begins. This order matters when one constructor calls `this()` and expects the chained constructor to set up state that the calling body needs.

---

#### Gotcha 2. base() is called implicitly only if the base class has a public parameterless constructor

**Concepts**
- Implicit `base()` only for parameterless
- CS7036 if base lacks parameterless ctor
- Must call `base(args)` explicitly
- Base chain runs top-down

**Answer**

If you do not explicitly call `base(args)` in a derived class constructor, the compiler inserts an implicit call to `base()` — the parameterless base constructor. If the base class defines only parameterized constructors, this implicit call fails with CS7036 because no parameterless constructor exists. You must explicitly call `base(requiredArg)` in every derived class constructor.

---

#### Gotcha 3. Static constructor runs at most once per type and cannot be called or retried

**Concepts**
- Triggered by first instance or static member access
- Executes once in the AppDomain
- Exception makes type permanently unusable (`TypeInitializationException`)
- Cannot call manually

**Answer**

The static constructor (or type initializer) runs exactly once for the lifetime of the type in the application. If it throws, the CLR marks the type as failed, and every subsequent attempt to use it throws `TypeInitializationException` with the original exception as the inner exception — there is no retry. Static constructors with unreliable initialization (file reads, network calls) are a reliability hazard.

---

#### Gotcha 4. Overload resolution selects exact match first, then widening, then params — optional parameters are lower priority than explicit

**Concepts**
- Exact type match wins
- Widening in order of closest
- `params` array is lowest priority
- Optional parameter overloads can produce ambiguity warnings

**Answer**

When multiple overloads could accept a given argument, the compiler first tries exact type match, then applies numeric widening in order of closeness (`int` to `long` before `int` to `double`), and uses `params` arrays only as a last resort. Optional parameters create additional candidate overloads that can produce ambiguity warnings; if both `M(int a)` and `M(int a, int b = 0)` exist, `M(42)` is ambiguous in some cases.

---

#### Gotcha 5. Optional parameter default values are embedded in the calling assembly — changing them requires recompiling callers

**Concepts**
- Defaults embedded at call site at compile time
- No runtime indirection
- Versioning problem for library authors
- Overloads without defaults avoid this

**Answer**

When a caller compiles against a method with an optional parameter default, the compiler bakes the default value into the caller's IL directly. If the library later ships with a changed default and only the library is redeployed, all existing callers continue to pass the old default until they are recompiled against the new library — a silent behavioral change.

---

#### Gotcha 6. Named arguments allow passing arguments to non-adjacent optional parameters, not out of order for positional

**Concepts**
- Named args set parameters by name
- Positional after named is CS1738
- Reorder only optional parameters
- Useful for long optional parameter lists

**Answer**

Named arguments allow you to specify which optional parameter you are providing without supplying all the preceding ones — `M(required: "x", optional2: true)` — by naming the target parameter. You cannot mix positional arguments after named arguments (CS1738). Named arguments do not change the order in which parameters are evaluated; they only select which optional slot to fill.

---

#### Gotcha 7. MemberwiseClone is protected — the class must expose its own Clone method to external callers

**Concepts**
- `MemberwiseClone` returns `object`
- Requires explicit override or new method
- Shallow copy semantics
- `ICloneable` is legacy — prefer typed `Clone<T>()` pattern

**Answer**

`Object.MemberwiseClone()` is a protected method, meaning it can only be called from within the class hierarchy. External code that needs to clone instances must call a public method you define (often `Clone()` or `Copy()`) that internally calls `MemberwiseClone()` and casts the result. This indirection gives the class control over what 'cloning' means, which is important for classes that need deep copying instead.

---

#### Gotcha 8. A constructor that throws leaves an object in a partially initialized state — the finalizer may still run

**Concepts**
- If finalizer registered before throw, GC will call it
- `GC.SuppressFinalize` not called
- Finalizer may reference null fields
- Guard finalizer with null checks

**Answer**

If a class has a finalizer and its constructor throws after the object is allocated but before `GC.SuppressFinalize` is called, the GC will still run the finalizer on the partially-constructed object. The finalizer may then try to access fields that were never initialized, causing `NullReferenceException` inside the finalizer thread. Guard finalizer code with null checks or only register the finalizer after successful construction.

---

#### Gotcha 9. Primary constructor parameters in C# 12 records and classes are scoped differently

**Concepts**
- Record primary ctor params become public init properties
- Class primary ctor params are captured fields only if used
- Not automatically public
- Accessed by name within the class

**Answer**

In a record (`record Point(int X, int Y)`), the primary constructor parameters automatically become public init-only properties `X` and `Y`. In a class (`class MyService(ILogger logger)`), the primary constructor parameter `logger` is only a private captured field used by other members; it is not a public property. This difference causes confusion when developers expect class primary constructor parameters to expose public members the way record parameters do.

---

#### Gotcha 10. Overloading on ref and out is not allowed — they have the same parameter-passing mechanism signature

**Concepts**
- `ref` and `out` are both by-reference
- CS0663 for `ref`/`out` overloading
- Different method names needed
- `in` is allowed as a separate overload from by-value

**Answer**

`void M(ref int x)` and `void M(out int x)` in the same class cause CS0663 because the compiler cannot distinguish between them at the call site — both are called with the `ref` or `out` keyword and a variable argument. To distinguish, use different method names. However, `void M(int x)` and `void M(in int x)` can coexist as overloads because one passes by value and the other by read-only reference.

---

## Real-World Scenarios

---

## Q22. A teammate chains constructors in `OrderLine` but QA reports that empty SKUs reach production. Review this code and identify the problem.

**Concepts**
- Constructor chain execution order
- Validation placement in the terminal constructor
- Intermediate constructor bypassing guards
- `string.IsNullOrWhiteSpace` guard
- Fix: direct chain to validating constructor

```csharp
public sealed class OrderLine
{
    public string Sku { get; }
    public int Quantity { get; }

    public OrderLine() : this("MISC", 1) { }

    public OrderLine(string sku)
    {
        Sku = sku?.Trim() ?? string.Empty;
        Quantity = 1;
    }

    public OrderLine(string sku, int quantity)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU is required.", nameof(sku));
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));
        Sku = sku.Trim();
        Quantity = quantity;
    }
}
```

| Category | Problem | Impact |
|---|---|---|
| Chain routing | `OrderLine(string sku)` does NOT chain to the two-arg constructor | Calls via the single-arg overload bypass `IsNullOrWhiteSpace` |
| Incomplete validation | Single-arg ctor assigns `sku?.Trim() ?? string.Empty` — allows empty string | Empty SKUs stored in DB |
| Unreachable guard | Two-arg constructor validates correctly but is bypassed | Guard exists but does not protect all paths |

**Fix priority list**
1. Change `OrderLine(string sku)` to `: this(sku, 1)` so all paths terminate at the validated two-arg constructor.
2. Remove the duplicate field assignment logic from the single-arg body.
3. Add an integration test calling `new OrderLine("")` and `new OrderLine(" ")` to prevent regression.

**Answer**

The root problem is that the single-argument constructor `OrderLine(string sku)` does not chain to the two-argument constructor that contains the validation. It has its own body that assigns `sku?.Trim() ?? string.Empty` — silently converting a blank SKU to empty string instead of throwing. Any caller using `new OrderLine("")` reaches the single-arg constructor, bypasses the guard in the two-arg constructor, and gets an `OrderLine` with `Sku == ""`. The fix is to change the single-arg constructor to `: this(sku, 1)`, making it delegate entirely to the validating terminal constructor. This ensures that every overload path, including the no-arg convenience constructor (which already chains correctly via `"MISC"`), runs through the same guard clause. The principle is that only one constructor should contain validation logic, and all others should chain to it.

---

## Q23. A .NET 10 service uses a C# 12 primary constructor for `StockReceipt`. Unit tests expecting `ArgumentException` on null SKU throw `NullReferenceException` instead. What is the initialization order problem?

**Concepts**
- Primary constructor parameter capture timing
- Field initializers run before constructor body block
- `sku.Trim()` executes before null-check in instance constructor block
- Guard clause must precede field assignment
- Primary constructor limitation vs traditional constructor

```csharp
public sealed class StockReceipt(string sku, decimal unitCost, int quantity)
{
    public string Sku { get; } = sku.Trim();   // runs first — throws NRE if sku is null
    public decimal UnitCost { get; } = unitCost;
    public int Quantity { get; } = quantity;

    // This block runs AFTER property initializers
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU is required.", nameof(sku));
        if (unitCost < 0m)
            throw new ArgumentOutOfRangeException(nameof(unitCost));
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));
    }
}
```

**Answer**

In C#, property and field initializers execute before any constructor body block. In this `StockReceipt` primary constructor, `public string Sku { get; } = sku.Trim()` runs as a field initializer. When `sku` is `null`, `sku.Trim()` throws `NullReferenceException` at that line, before the guard clause in the instance constructor block ever runs. The fix is to move validation logic into the initializers themselves or use a static factory method. The cleanest approach for primary constructors is to put guard expressions directly in the initializer: `public string Sku { get; } = (string.IsNullOrWhiteSpace(sku) ? throw new ArgumentException("SKU is required.", nameof(sku)) : sku.Trim())`. Alternatively, revert to a traditional constructor where the guard clauses precede all assignments. This initialization order — initializers run before constructor body — is a foundational rule that must be kept in mind when placing validation logic in types using primary constructors.

---

## Q24. Overload ambiguity CS0121 is reported on `PricingHelper.LineTotal(3, 2.49m, 0.10m)`. How do you diagnose and fix it?

**Concepts**
- CS0121 — ambiguous overload
- Optional parameters broadening resolution candidates
- Explicit cast to disambiguate
- Signature refactoring to eliminate overlap
- Overload design principle

```csharp
public static decimal LineTotal(int qty, decimal unitPrice, decimal discountRate = 0m) { ... }
public static decimal LineTotal(int qty, decimal unitPrice, decimal discountRate, decimal taxRate) { ... }
```

**Answer**

The call `LineTotal(3, 2.49m, 0.10m)` is ambiguous because both overloads can accept three decimal values: the first accepts three parameters (with `discountRate` required in this call), and the second accepts four with `taxRate` left to its implicit default — but `decimal` parameters have no default here, so only the first truly applies. Actually, the compiler flags this because the first overload with a default `discountRate = 0m` becomes applicable even when `discountRate` is explicitly passed, causing confusion between the two candidates when type resolution produces equally-specific matches. The correct diagnosis is: the compiler cannot determine whether the three-argument form is the "full" call or the first three arguments of the four-parameter form. The fix options are: (1) rename one overload to `LineTotalWithTax`; (2) remove the default on `discountRate` in the three-parameter overload; (3) add an explicit cast at the call site to force selection. Option 1 (distinct names) is the most readable API design because it eliminates the conceptual ambiguity in addition to the compiler error.

---

## Q25. A DI-based ASP.NET Core service still constructs dependencies manually inside its constructor. What breaks, and what is the correct pattern?

**Concepts**
- Constructor injection via DI
- Manual instantiation bypassing DI container
- Singleton anti-pattern with static accessor
- Testability — cannot mock manually-newed dependencies
- Lifetime and disposal management

```csharp
public sealed class InventorySyncService : IInventorySyncService
{
    private readonly InventoryRegistry _registry;

    public InventorySyncService()
    {
        _registry = InventoryRegistry.Instance;  // static singleton
    }

    public void Sync(Product product) => _registry.Register(product);
}
```

**Answer**

The `InventorySyncService` constructor reaches out to a static singleton `InventoryRegistry.Instance` rather than accepting the dependency through its constructor parameter list. This creates three interconnected problems. First, testability is broken — unit tests cannot replace `InventoryRegistry` with a mock because the dependency is hard-coded to the static instance. Second, lifetime management is bypassed — if `IInventorySyncService` is registered as Scoped or Transient but `InventoryRegistry` is a process-wide static, lifetime mismatches produce subtle state-sharing bugs. Third, the `InventorySyncService` constructor is not honest about its requirements: callers and the DI container do not know it depends on `InventoryRegistry`. The fix is to declare the dependency as a constructor parameter (`public InventorySyncService(IInventoryRegistry registry)`) and register `IInventoryRegistry` in the DI container. The container then owns creation and lifetime, and tests can inject a mock or stub. Static singletons should be replaced with DI-registered singletons (`AddSingleton`) so the container controls the single-instance guarantee.

---

## Q26. How would you design a `ProductFactory` that enforces invariants through constructors while keeping a clean separation from ASP.NET Core request deserialization?

**Concepts**
- Factory method pattern
- `required` init-only properties vs constructor validation
- `System.Text.Json` deserialization and null semantics
- Compile-time vs runtime contract enforcement
- Domain constructor vs DTO constructor separation

**Answer**

The challenge is that JSON deserialization frameworks like `System.Text.Json` typically require a parameterless constructor or supported constructor attributes, but domain constructors should validate at construction time. The clean solution separates two types: a `CreateProductRequest` record or class with `required` init-only properties that the deserializer populates, and a domain `Product` class with a validating parameterized constructor. The `required` keyword enforces that C# callers supply the property in an object initializer, but JSON deserialization can bypass that if `[JsonRequired]` is not also applied at the API layer. In .NET 10, `[JsonRequired]` on the DTO properties ensures the deserializer throws for missing values. A `ProductFactory.FromRequest(CreateProductRequest req)` method then calls `new Product(req.Name, req.UnitPrice)`, where the domain constructor performs deeper business validation (non-empty name, non-negative price). This two-layer approach keeps deserialization concerns out of the domain object while ensuring both compile-time and runtime safety.
