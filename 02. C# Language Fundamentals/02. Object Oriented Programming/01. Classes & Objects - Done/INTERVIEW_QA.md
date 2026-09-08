# C# Classes & Objects — Interview Q&A

---


## Table of Contents

1. [Q1. What is the difference between a class and an object in C#?](#q1-what-is-the-difference-between-a-class-and-an-object-in-c)
2. [Q2. What does the `new` keyword do when creating a class instance?](#q2-what-does-the-new-keyword-do-when-creating-a-class-instance)
3. [Q3. What is the `this` keyword, and what are its three distinct uses in a class?](#q3-what-is-the-this-keyword-and-what-are-its-three-distinct-uses-in-a-class)
4. [Q4. When does the compiler generate a default (parameterless) constructor, and when does it stop?](#q4-when-does-the-compiler-generate-a-default-parameterless-constructor-and-when-does-it-stop)
5. [Q5. What are reference semantics, and how do they differ from value semantics?](#q5-what-are-reference-semantics-and-how-do-they-differ-from-value-semantics)
6. [Q6. What methods does every C# class inherit from `System.Object`, and which are commonly overridden?](#q6-what-methods-does-every-c-class-inherit-from-systemobject-and-which-are-commonly-overridden)
7. [Q7. What is the difference between `typeof(T)` and `obj.GetType()`?](#q7-what-is-the-difference-between-typeoft-and-objgettype)
8. [Q8. What are object initializers, and what constraints must the target type satisfy?](#q8-what-are-object-initializers-and-what-constraints-must-the-target-type-satisfy)
9. [Q9. What are partial classes, and what problem do they solve?](#q9-what-are-partial-classes-and-what-problem-do-they-solve)
10. [Q10. What are record types (C# 9+), and how do they differ from a regular class?](#q10-what-are-record-types-c-9-and-how-do-they-differ-from-a-regular-class)
11. [Q11. What is an anonymous type, and where can it be used?](#q11-what-is-an-anonymous-type-and-where-can-it-be-used)
12. [Q12. What is a nested class, and when would you use one over a top-level private class?](#q12-what-is-a-nested-class-and-when-would-you-use-one-over-a-top-level-private-class)
13. [Q13. How does enabling nullable reference types (`<Nullable>enable</Nullable>`) change class design?](#q13-how-does-enabling-nullable-reference-types-nullableenablenullable-change-class-design)
14. [Q14. How is a class instance laid out in managed heap memory?](#q14-how-is-a-class-instance-laid-out-in-managed-heap-memory)
15. [Q15. What makes an object eligible for garbage collection, and what is the `IDisposable` pattern?](#q15-what-makes-an-object-eligible-for-garbage-collection-and-what-is-the-idisposable-pattern)
16. [Q16. What are the differences between `==`, `Equals()`, and `ReferenceEquals()` for class instances?](#q16-what-are-the-differences-between-equals-and-referenceequals-for-class-instances)
17. [Q17. What is the `dynamic` type, and when should you use it (or avoid it) in class design?](#q17-what-is-the-dynamic-type-and-when-should-you-use-it-or-avoid-it-in-class-design)
18. [Q18. If you declare a class with only a parameterized constructor and then use an object initializer in another file, will it compile? Why or why not?](#q18-if-you-declare-a-class-with-only-a-parameterized-constructor-and-then-use-an-object-initializer-in-another-file-will-it-compile-why-or-why-not)
19. [Q19. Does `==` always compare by reference for class types in C#?](#q19-does-always-compare-by-reference-for-class-types-in-c)
20. [Q20. What happens to an object's fields before the constructor body runs?](#q20-what-happens-to-an-objects-fields-before-the-constructor-body-runs)
21. [Q21. Can a `struct` be `null` when used as a field inside a class?](#q21-can-a-struct-be-null-when-used-as-a-field-inside-a-class)
22. [Q22. Is `var x = new Student()` statically typed or dynamically typed?](#q22-is-var-x-new-student-statically-typed-or-dynamically-typed)
23. [Q23. (Code Review) Your team's customer service module has a shared cache that returns the same `Customer` reference to all concurrent requests. Review the following code and identify the defects:](#q23-code-review-your-teams-customer-service-module-has-a-shared-cache-that-returns-the-same-customer-reference-to-all-concurrent-requests-review-the-following-code-and-identify-the-defects)
24. [Q24. (Scenario) A student enrollment system needs an `IDisposable` resource-holder class that wraps a file handle. Design the class correctly, explaining each decision.](#q24-scenario-a-student-enrollment-system-needs-an-idisposable-resource-holder-class-that-wraps-a-file-handle-design-the-class-correctly-explaining-each-decision)
25. [Q25. (Scenario) You are designing a domain model for a lending application. The team debates whether `LoanApplication` should be a `class` or a `record`. Walk through your decision process.](#q25-scenario-you-are-designing-a-domain-model-for-a-lending-application-the-team-debates-whether-loanapplication-should-be-a-class-or-a-record-walk-through-your-decision-process)
26. [Q26. (Code Review) A report generator passes student objects by reference alias without making copies. Downstream reports show wrong historical data. Review:](#q26-code-review-a-report-generator-passes-student-objects-by-reference-alias-without-making-copies-downstream-reports-show-wrong-historical-data-review)
27. [Q27. (Scenario) A code generator produces a `CustomerEntity` partial class file with persistence logic. Your team needs to add validation and domain methods without touching the generated file. Design the solution.](#q27-scenario-a-code-generator-produces-a-customerentity-partial-class-file-with-persistence-logic-your-team-needs-to-add-validation-and-domain-methods-without-touching-the-generated-file-design-the-solution)

---
## Foundation Questions

---

## Q1. What is the difference between a class and an object in C#?

**Concepts**
- Class as a type blueprint
- Object as a runtime instance
- Heap allocation on `new`
- Per-instance field storage
- Reference variable as a pointer to the heap

**Answer**

A class is a compile-time type definition — a blueprint that describes the shape of data (fields) and behavior (methods) that every instance of that type will carry. An object is a live, runtime instance of a class: memory is allocated on the managed heap when you write `new Student("Darshan", 101)`, and a reference (a pointer-sized value) is returned and stored in the variable. Every object gets its own copy of instance fields, so `student1.RollNumber` and `student2.RollNumber` are independent memory locations even though both come from the same `Student` class. The class itself is written once in source code and loaded once per AppDomain; the objects are created (and eventually garbage-collected) as the program runs. A useful mental model: the class is the cookie cutter, each object is a freshly stamped cookie — they share the same shape but are physically distinct.

---

## Q2. What does the `new` keyword do when creating a class instance?

**Concepts**
- Managed heap allocation
- Zero-initialization of fields before constructor runs
- Constructor invocation
- Reference returned to the caller
- Object header (sync block + method table pointer)

**Answer**

The `new` keyword performs two operations in sequence. First, the CLR allocates a contiguous block of memory on the managed heap large enough to hold all instance fields, plus an internal object header (a sync-block index and a pointer to the type's method table). All allocated bytes are zero-initialized before any user code runs, so value-type fields default to zero/`false` and reference-type fields default to `null`. Second, the designated constructor executes to give fields meaningful values — in `new Student("Darshan", 101)`, the constructor assigns `StudentName` and `RollNumber` while `Address` is explicitly set to `string.Empty`. When the constructor returns, the expression evaluates to a reference (a pointer) to the newly allocated object; that reference is then stored in a variable or passed to a method. Importantly, `new` does not pin the object — the GC can move it during compaction, updating all managed references transparently. After construction there is no user-visible second step: the object is fully live immediately.

---

## Q3. What is the `this` keyword, and what are its three distinct uses in a class?

**Concepts**
- `this` as implicit instance reference
- Field vs parameter disambiguation
- Passing the current instance to another method
- Constructor chaining with `:this(...)`
- Compile-time availability (instance context only)

**Answer**

Inside any instance method or constructor, `this` is a read-only reference to the object on whose behalf the code is executing. Its three practical uses are: first, disambiguating a field name from a parameter that shares the same identifier — `this.StudentName = studentName` makes clear the left side is the field and the right side is the parameter; without `this`, both names would resolve to the closer-scope parameter, silently leaving the field unchanged. Second, passing the current object as an argument — `SomeService.Register(this)` hands the current instance to another method without a local variable. Third, constructor chaining — `:this(defaultName, defaultRoll)` calls another overload of the same class's constructor before the calling constructor body runs, allowing shared initialization logic without duplication. `this` cannot appear in a static method because static methods have no associated instance; the compiler enforces this with a compile-time error.

---

## Q4. When does the compiler generate a default (parameterless) constructor, and when does it stop?

**Concepts**
- Compiler-synthesized parameterless constructor
- Rule: any explicit constructor suppresses synthesis
- Object initializer dependency on parameterless constructor
- Restoring the default with an explicit parameterless ctor
- Primary constructor (C# 12) as an alternative

**Answer**

The C# compiler synthesizes a `public` parameterless constructor whenever you declare a class with no constructors at all. The moment you write even one constructor — parameterized or parameterless — the compiler stops generating one. This rule is consequential: if you add `Student(string name, int roll)` to `Student`, callers who previously wrote `new Student()` or used object initializer syntax (`new Student { RollNumber = 1 }`) will see a compile-time error (CS7036 — no accessible parameterless constructor). The fix is either to explicitly add a parameterless constructor or to rewrite the calling code to use the parameterized path. This behavior enforces intent: if a constructor exists to require certain arguments at creation time, the compiler does not silently provide a bypass. In C# 12 the primary constructor syntax (`class Student(string name, int roll)`) integrates parameters directly into the class declaration and similarly removes the implicit default, so the same principle applies.

---

## Q5. What are reference semantics, and how do they differ from value semantics?

**Concepts**
- Reference type vs value type storage model
- Assignment copies reference (pointer), not object data
- `ReferenceEquals` for identity check
- Shared mutation through aliased references
- Value type assignment produces an independent copy

**Answer**

Class instances are reference types: a variable holds a pointer (reference) to an object on the heap, not the object's data itself. When you write `Student enrolled = student1`, no new object is created — `enrolled` and `student1` point to the same heap allocation, so `ReferenceEquals(enrolled, student1)` returns `true`. Any mutation through either variable affects the single shared object: if `enrolled.RollNumber = 999` executes, then `student1.RollNumber` is also `999` because they are the same instance. This is the root cause of many bugs where developers expect a variable to be an independent copy. Value types (structs like `DateTime`, `int`) behave differently: assignment copies the entire value into a new storage location. `DateTime birthDateCopy = student1.DateOfBirth` creates an independent copy; `birthDateCopy = birthDateCopy.AddYears(1)` does not affect `student1.DateOfBirth`. When designing APIs, return new instances from class factory methods when callers need snapshots, and use `MemberwiseClone` or explicit mapping when you must duplicate an object.

---

## Q6. What methods does every C# class inherit from `System.Object`, and which are commonly overridden?

**Concepts**
- `System.Object` as the universal base class
- `ToString()`, `Equals(object)`, `GetHashCode()`, `GetType()`
- `MemberwiseClone()` (protected shallow copy)
- `ReferenceEquals` as a static identity utility
- Contract between `Equals` and `GetHashCode`

**Answer**

Every class in C# implicitly inherits from `System.Object`, which contributes four instance methods and two static utilities. `ToString()` returns a type-qualified string by default (`"ClassesAndObjects.Student"`) — override it to produce domain-meaningful output, as `Student.ToString()` does to show name, roll, and percentage. `Equals(object obj)` compares by reference identity by default — two different `Student` instances with identical data are not equal unless you override it. `GetHashCode()` returns an integer representing the object's hash bucket; the contract requires that two objects where `Equals` returns `true` must have the same hash code, so you must override both together when overriding either. `GetType()` returns the runtime `Type` descriptor and cannot be overridden (it is non-virtual). The protected `MemberwiseClone()` produces a shallow copy — all fields are copied bit for bit, meaning reference-type fields share the original objects. Commonly overridden: `ToString()` for logging, `Equals` and `GetHashCode` for value-equality semantics in collections like `Dictionary` and `HashSet`.

---

## Q7. What is the difference between `typeof(T)` and `obj.GetType()`?

**Concepts**
- `typeof` resolves at compile time from a type name
- `GetType()` resolves at runtime from the actual instance
- Polymorphism gap: declared type vs runtime type
- Use in pattern matching and reflection
- Cannot be used interchangeably in inheritance hierarchies

**Answer**

`typeof(Student)` is a compile-time operator that resolves the `Type` descriptor from a type name written in source code — it requires no instance and works even when no object exists. `obj.GetType()` is a runtime call on an existing instance that returns the descriptor of the object's actual (concrete) type, regardless of the variable's declared type. The difference matters with inheritance: if `object boxed = new Student(...)`, then `typeof(object)` is `System.Object` but `boxed.GetType()` returns `ClassesAndObjects.Student`. This runtime resolution is how polymorphic dispatch and reflection work — `GetType().Name` returns `"Student"` even when the variable is typed as `object`. In type testing, `obj is Student` is the idiomatic modern form (pattern matching, no overhead over `GetType() == typeof(Student)`); `as Student` is preferred over a cast when you want `null` on failure rather than an `InvalidCastException`. Never compare `GetType()` to a hardcoded string; compare to `typeof(T)` or use `is T`.

---

## Q8. What are object initializers, and what constraints must the target type satisfy?

**Concepts**
- Object initializer as syntactic sugar over a constructor + property/field assignments
- Requires an accessible parameterless constructor (or primary constructor)
- Executes constructor first, then each assignment in order
- Works with properties and accessible fields
- Combined with `new` or collection initializers

**Answer**

An object initializer is a concise syntax that allows you to set public properties or accessible fields immediately after construction in a single expression: `new Student { RollNumber = 1, StudentName = "Ada" }`. The compiler desugars this into a temporary variable, a constructor call, and sequential assignments — it is not a bypass of the constructor. This has two important implications: the constructor runs first with whatever arguments (none if parameterless), and then each named assignment executes in source order. Because a parameterless constructor is required, adding a parameterized-only constructor to `Student` breaks any object initializer call site at compile time. Object initializers are especially useful with anonymous types (where you cannot write a constructor at all), with record types, and in LINQ projections. They do not provide atomicity — if an exception is thrown midway through the assignments, the partially initialized object is discarded and no reference escapes to the caller (the temp variable is local), which is safe. For required fields enforced at creation time, prefer a parameterized constructor over an initializer because invariants are then checked in one place.

---

## Q9. What are partial classes, and what problem do they solve?

**Concepts**
- Single logical class split across multiple source files
- `partial` keyword on each fragment
- Compiler merges all fragments at compile time
- Common use: code generation + hand-written logic separation
- Limitations: all fragments must be in the same assembly and namespace

**Answer**

A partial class lets a single type definition span multiple `.cs` files by adding the `partial` modifier to every fragment. The C# compiler merges all matching fragments into one type definition before compilation, so the resulting class is identical to one written in a single file. The canonical use case is code generation: a designer or generator tool produces one file (e.g., Entity Framework migrations, WinForms `InitializeComponent`, ASP.NET source generators), and developers write business logic in a separate file. This prevents re-running the generator from overwriting hand-written code. All fragments must reside in the same assembly, the same namespace, and carry compatible access modifiers — a `public partial class Student` and a `private partial class Student` in the same namespace is a compile error. Partial classes can have different bases declared per fragment only if they resolve to a consistent hierarchy (all fragments agree on the base or one specifies it). Partial methods (a related feature) allow a signature declared in one fragment to have its implementation supplied — or omitted — in another, with the compiler silently removing calls to unimplemented partial methods, which is useful for lightweight hooks in generated code.

---

## Q10. What are record types (C# 9+), and how do they differ from a regular class?

**Concepts**
- Records as reference types with value-based equality
- Compiler-generated `Equals`, `GetHashCode`, `ToString`, `Deconstruct`
- `with` expression for non-destructive mutation
- Positional records and primary constructors
- `record struct` (C# 10) vs `record class`

**Answer**

Records were introduced in C# 9 as a concise way to define types whose equality is based on their data rather than their identity. A `record` declaration generates value-based `Equals` and `GetHashCode` that compare all public properties, a `ToString` that prints property names and values, and a `Deconstruct` method for pattern matching — none of which you get automatically from a class. The `with` expression allows copying a record while changing selected properties: `var updated = original with { Name = "Priya" }` creates a new instance with all other properties cloned. Positional records (`record Student(string Name, int Roll)`) are even more concise, combining primary constructor, auto-properties, and deconstruct in one line. Under the hood, a `record` is still a reference type (heap-allocated, reference semantics for assignment) — `ReferenceEquals` behaves the same as for a class — but `==` and `Equals` compare content. In .NET 10, records are the preferred way to model immutable data transfer objects (DTOs), domain value objects, and snapshots where you care about what the data is, not which heap instance you hold. Use `record struct` when you want value-type stack allocation combined with value equality.

---

## Q11. What is an anonymous type, and where can it be used?

**Concepts**
- Compiler-generated unnamed class
- Inferred from property initializer syntax
- Properties are read-only after construction
- Scope limited to the declaring method (or `var` inference)
- Common in LINQ `select` projections

**Answer**

An anonymous type is a compiler-generated class with no user-visible name; you create one with `new { Name = "Darshan", Roll = 101 }` and must use `var` to capture it because the type name is inaccessible in source. The compiler generates a sealed class with read-only auto-properties, and implements `Equals`, `GetHashCode`, and `ToString` based on all properties — the same names and types in the same order yield structurally identical types within the same assembly, and the compiler reuses that single generated class. Anonymous types are most common in LINQ `select` projections where you need a shaped result for local consumption but do not want to declare a full named type: `var summary = students.Select(s => new { s.StudentName, s.Percentage }).ToList()`. Because properties are read-only, anonymous type instances are effectively immutable after creation. The main limitation is scope: anonymous types cannot cross method boundaries (without boxing to `object` or `dynamic`), so for data that needs to leave a method, declare a named class, record, or tuple. In modern C# (.NET 10) value tuples (`(string Name, int Roll)`) often replace anonymous types for lightweight returns because they can be named, returned, and deconstructed.

---

## Q12. What is a nested class, and when would you use one over a top-level private class?

**Concepts**
- Nested class declared inside another class body
- Access to outer class private members
- Encapsulation of implementation detail
- `private` nested class invisible outside the enclosing type
- Common patterns: iterators, builders, state objects

**Answer**

A nested class is a class declared inside another class's body. It can be `public`, `private`, `protected`, `internal`, or `protected internal`, and it can access private members of the enclosing class — something a top-level `internal` class cannot. A `private` nested class is completely invisible outside the enclosing type, making it ideal for implementation-detail types that should never escape the API surface. Common uses: a `Node` class inside a `LinkedList<T>`, a `Builder` class inside a domain entity, or an enumerator class produced when implementing `IEnumerable`. The enclosing type does not get implicit access to the nested class's private members — the access is one-directional. A nested class is a full type: it can have its own constructors, static members, and inheritance hierarchy. Prefer a nested class over a top-level `internal` class when the nested type is only meaningful in the context of the outer type, when you want to access private fields or methods of the outer class, or when you want to prevent the implementation detail from even appearing in IDE auto-complete outside the enclosing type.

---

## Q13. How does enabling nullable reference types (`<Nullable>enable</Nullable>`) change class design?

**Concepts**
- Nullable annotation context: `string` vs `string?`
- Non-null assertion operator (`!`)
- Flow analysis for null guards
- Default value initialization for reference fields
- `MaybeNull`, `NotNull`, and other nullable attributes

**Answer**

When nullable reference types are enabled (the default for new .NET 10 projects), the compiler distinguishes between `string` (the developer promises this will never be null) and `string?` (null is a valid state). This shifts null-safety from a runtime concern to a compile-time concern: assigning `null` to a `string` field produces a warning, and dereferencing a `string?` without a prior null check also produces a warning. Class design changes in three ways. First, fields that should never be null must be initialized — either via field initializers (`public string Name = string.Empty`) or in every constructor path; the compiler traces flow and warns if any constructor exit leaves a non-nullable field unset. Second, method return types and parameters should be annotated to express the API contract — `Student? FindByRoll(int roll)` communicates that the result may be absent, and callers must guard before dereferencing. Third, `default!` (null-forgiving assertion) is the escape hatch when you know the compiler's flow analysis is too conservative, but it should be rare. Together, these changes push classes toward invariant-safe designs where objects cannot be constructed in a partially null state, which eliminates whole categories of `NullReferenceException` bugs that previously only surfaced in production.

---

## Q14. How is a class instance laid out in managed heap memory?

**Concepts**
- Object header: sync-block index + method table pointer
- Instance fields in declaration order (with CLR-permitted reordering)
- Reference fields store 4- or 8-byte pointers
- Value-type fields stored inline
- Minimum object size (16 bytes on 64-bit for header + fields)

**Answer**

Every class instance on the managed heap begins with an object header that the CLR maintains. On a 64-bit runtime, this header consists of two pointer-sized words: the sync-block index (8 bytes, used for `lock`, `Monitor`, and `GetHashCode` fallback) and the method table pointer (8 bytes, pointing to type metadata and the virtual dispatch table). After the header, the CLR lays out all instance fields. The CLR may reorder fields relative to their declaration order to optimize alignment and minimize padding, unless `[StructLayout(LayoutKind.Sequential)]` is applied. Reference-type fields are stored as 4- or 8-byte pointers (depending on process bitness); value-type fields are stored inline, so a `DateTime` field contributes 8 bytes directly to the object's footprint rather than adding a pointer indirection. The practical minimum object size on 64-bit is 24 bytes (two header words + at least one field word, padded to 8-byte alignment). Understanding this layout matters for performance-sensitive code: many small short-lived objects cause GC pressure; packing related data into one class or using a `struct` for hot-path data can improve cache locality and reduce collection frequency.

---

## Q15. What makes an object eligible for garbage collection, and what is the `IDisposable` pattern?

**Concepts**
- Reachability as the GC eligibility criterion
- GC roots: stack variables, static fields, GC handles
- Finalization queue and `~ClassName()` finalizer
- `IDisposable.Dispose()` for deterministic cleanup
- `using` statement and `using` declaration

**Answer**

The garbage collector determines eligibility by tracing object references from GC roots — stack-local variables, static fields, and registered GC handles. When no root can reach an object through any chain of references, the object is unreachable and the GC may reclaim its memory at the next appropriate collection. Setting a variable to `null` removes one reference but does not immediately free the object — the GC runs on its own schedule and will collect the object when the generation it belongs to is collected. For objects holding unmanaged resources (file handles, database connections, native memory), the GC alone is insufficient because it knows nothing about unmanaged memory. The `IDisposable` pattern provides deterministic cleanup: implement `Dispose()` to release unmanaged resources immediately when the caller is done. The `using` statement (`using var conn = new DbConnection(...)`) calls `Dispose()` automatically at the end of the block, even if an exception is thrown. If a class also has a finalizer (`~ClassName()`), it provides a safety net — the GC calls it when the object is collected without `Dispose()` having been called — but finalizers are non-deterministic and slow collection for the object's generation. The standard pattern suppresses the finalizer inside `Dispose()` via `GC.SuppressFinalize(this)` so correctly-disposed objects do not pay the finalizer overhead.

---

## Q16. What are the differences between `==`, `Equals()`, and `ReferenceEquals()` for class instances?

**Concepts**
- Default `==` for classes compares references
- `Equals` virtual method, overridable for value semantics
- `ReferenceEquals` is static, non-overridable, always identity
- `record` overrides `==` and `Equals` to compare by value
- Operator overloading for `==` (class-specific)

**Answer**

For classes, `==` compares references by default — two variables are `==` only if they point to the same heap object. `Equals(object)` inherited from `System.Object` behaves identically by default, because the default implementation is `return ReferenceEquals(this, obj)`. Both can be overridden: many BCL types (`string`, `DateTime`, `Guid`) override them to provide value equality. `ReferenceEquals(a, b)` is a static method on `System.Object` that always tests identity and cannot be overridden, making it the reliable identity check regardless of what `==` or `Equals` do on a type. For `record` types, the compiler generates `Equals` and `==` that compare all public properties, so two records with identical data are equal even though they occupy different heap addresses. When you need to store objects in a `Dictionary` or `HashSet` with value-based equality, you must override both `Equals` and `GetHashCode` consistently; overriding one without the other violates the contract and causes subtle collection bugs. In production code, prefer `is null` over `== null` for null checks on types where `==` may be overloaded, since `is null` always calls `ReferenceEquals(x, null)` and is not interceptable.

---

## Q17. What is the `dynamic` type, and when should you use it (or avoid it) in class design?

**Concepts**
- `dynamic` defers member resolution to runtime via DLR
- Bypasses compile-time type checking
- Runtime `RuntimeBinderException` on invalid member access
- Legitimate uses: COM interop, reflection replacement, duck typing
- Performance cost: DLR call site caching

**Answer**

Declaring a variable as `dynamic` tells the compiler to defer all member access resolution to runtime via the Dynamic Language Runtime (DLR). `dynamic obj = someInstance; obj.Foo()` will compile regardless of whether `Foo` exists — the check happens at runtime, and a `RuntimeBinderException` is thrown if the member is absent. This is fundamentally different from `object`, which is statically typed and requires a cast before you can access members. Legitimate uses are narrow: interoperating with COM objects whose type library is unavailable, calling methods on anonymous types passed across assembly boundaries, implementing dynamic dispatch over JSON or other schema-less data, or simplifying certain reflection-heavy scenarios where the alternative is long `MethodInfo.Invoke` chains. In ordinary class design, `dynamic` should be avoided: it removes compile-time safety, suppresses IntelliSense and refactoring support, and introduces a performance overhead from DLR call site binding on the first invocation (though subsequent calls are cached). For type-safe polymorphism, prefer interfaces, base classes, or generics. A common interview pitfall is conflating `dynamic` with `var` — `var` is statically typed (the compiler infers the type at compile time), while `dynamic` genuinely defers typing to runtime.

---

## Gotchas — Classes & Objects (Interview Traps)

---

#### Gotcha 1. Object initializer syntax requires a parameterless constructor that may not be synthesized

**Concepts**
- `{ }` initializer desugars to ctor call + assignments
- Any explicit ctor suppresses default
- CS7036 if no parameterless ctor
- `required` properties (C# 11) as enforced alternative

**Answer**

When you write `new Student { Name = "Ada" }`, the compiler desugars this to first call a parameterless constructor, then assign `Name`. If the `Student` class defines only a parameterized constructor, no parameterless one is synthesized and the object initializer fails with CS7036. The fix is to explicitly add a parameterless constructor or switch to direct constructor argument syntax.

---

#### Gotcha 2. == compares references for user-defined classes unless operator is overloaded

**Concepts**
- Default `==` is reference equality
- `string` overloads `==` to content
- `record` auto-generates value-based `==`
- `ReferenceEquals` is always reference equality regardless of overloads

**Answer**

Two distinct `Student` instances with identical property values are not `==` by default because `==` for classes compares references (addresses on the heap). The `string` class and `record` types override `==` to perform value comparison, which is why string equality works intuitively. For custom classes needing value semantics, explicitly overload `==` (and `!=`) and override `Equals` and `GetHashCode` consistently.

---

#### Gotcha 3. Field initializers run before the constructor body — base constructor runs before derived constructor body

**Concepts**
- CLR zero-initializes first
- Derived field initializers run
- `base()` chain runs
- Derived ctor body last
- Field init order is top-to-bottom within the class

**Answer**

The execution order for object creation is: CLR zeroes all memory, derived class field initializers execute in source order, the base class constructor (and its chain) runs to completion, and then the derived class constructor body executes. Calling virtual methods in a constructor is dangerous precisely because the derived class body has not run yet, leaving derived fields at their zero values.

---

#### Gotcha 4. Calling virtual methods from a constructor invokes the derived override before derived fields are initialized

**Concepts**
- Virtual dispatch goes to most-derived override
- Derived ctor body has not run at base ctor time
- Derived fields still zero
- `NullReferenceException` risk
- `sealed` or non-virtual methods in ctors are safe

**Answer**

If a base class constructor calls a virtual method, and a derived class overrides that method and accesses its own fields, those fields are still `null` or zero because the derived constructor body has not yet executed at the time the base constructor runs. This pattern reliably produces `NullReferenceException` or incorrect behavior in the derived class, and the only safe fix is to avoid calling virtual methods from constructors.

---

#### Gotcha 5. A struct cannot be null in its unboxed form; Nullable<T> wraps it with a HasValue flag

**Concepts**
- Value types have no null state
- `Nullable<T>` adds `HasValue` + `Value`
- `typeof(int?) != typeof(int)`
- Boxing a null `int?` produces null object reference

**Answer**

An `int` field in a class is always zero-initialized by the CLR, never null. To represent 'no value', you need `int?` (`Nullable<int>`), which is a different struct type with a `HasValue` bool and a `Value` field. The null state in `Nullable<T>` is a `HasValue=false` struct value — there is no heap object, no reference, just a stack value with an extra boolean.

---

#### Gotcha 6. var is compile-time type inference — the type is fixed at the declaration and cannot change

**Concepts**
- Compiler infers concrete type at declaration
- `var x = 42` makes `x` an `int` forever
- Different from `dynamic`
- IDE shows inferred type
- Useful for complex generic types

**Answer**

`var` instructs the compiler to infer the variable's type from the right-hand side initializer expression. The result is a statically-typed variable identical to one declared with an explicit type name. Writing `var x = 42` and then attempting `x = "text"` fails with CS0029 (int does not implicitly convert to string) because `x` was inferred as `int` at compile time.

---

#### Gotcha 7. MemberwiseClone produces a shallow copy — reference-type fields are aliased between source and copy

**Concepts**
- `MemberwiseClone` copies field values
- Reference fields copy the address
- Source and copy alias same objects
- Mutation through copy visible in original
- Deep copy requires explicit cloning

**Answer**

`Object.MemberwiseClone()` creates a new object with the same field values, but for reference-type fields it copies the reference (the address), not the object it points to. After `MemberwiseClone`, both the original and the copy point to the same list or sub-object. Modifying that list through the copy modifies it in the original, which is why 'copy' is misleading — only a deep copy provides true independence.

---

#### Gotcha 8. Partial classes must be in the same assembly and must have consistent access modifiers

**Concepts**
- Partial merges at compile time within same assembly
- Access modifier must match or be absent
- Partial method body is optional (calls removed if absent)
- Useful for code-gen + developer split

**Answer**

Partial classes are a compile-time mechanism where the compiler merges all partial declarations into a single type. All parts must reside in the same assembly — placing a partial class part in a separate DLL is a compile error. If one part declares the class as `public`, the other must also declare `public` or omit the access modifier; mixing `public` and `internal` on different parts causes CS0262.

---

#### Gotcha 9. sealed class prevents further inheritance but not interface implementation

**Concepts**
- `sealed` prevents being a base class
- Sealed class can still implement interfaces
- All methods are implicitly final
- Useful for preventing fragile base class problems

**Answer**

Marking a class `sealed` means no other class can inherit from it, which eliminates the fragile base class problem for that type and allows the JIT to devirtualize calls. A sealed class can still implement any number of interfaces — sealing restricts inheritance only, not interface contracts. In fact, sealing a small value-object or utility class with no intended extension points is a good practice that also enables minor JIT optimizations.

---

#### Gotcha 10. Nested private class is accessible to the outer class but not to external code — even in the same assembly

**Concepts**
- Private nested type is invisible outside the outer class
- Inner class can access outer class private members
- Useful for implementation detail encapsulation
- Differs from `internal` visibility

**Answer**

A private nested class is entirely hidden from code outside the declaring class, including other classes in the same assembly. The outer class, however, can use the nested class freely including accessing its private members. This is stronger encapsulation than `internal` visibility, making private nested types ideal for implementation-detail data structures that no external code should ever construct or reference.

---

## Real-World Scenarios

---

## Q23. (Code Review) Your team's customer service module has a shared cache that returns the same `Customer` reference to all concurrent requests. Review the following code and identify the defects:

```csharp
public class CustomerCache
{
    private readonly Dictionary<int, Customer> _cache = new();

    public Customer GetOrCreate(int id)
    {
        if (!_cache.ContainsKey(id))
            _cache[id] = new Customer();
        return _cache[id];          // returns the live reference
    }

    public void UpdateLoan(int id, Customer updated)
    {
        var existing = GetOrCreate(id);
        existing.Name = updated.Name;
        existing.LoanAmount = updated.LoanAmount;
        existing.RateOfInterest = updated.RateOfInterest;
        existing.DurationOfLoan = updated.DurationOfLoan;
    }
}
```

**Concepts**
- Reference type sharing: all callers mutate one instance
- Cache entry as live mutable object vs immutable snapshot
- Thread safety: non-concurrent `Dictionary`
- Copy-on-read pattern
- `TryGetValue` efficiency

**Answer**

The core defect is reference semantics: `GetOrCreate` returns the cached reference directly, so any caller who holds the reference and mutates a field is simultaneously mutating the data every other caller sees. This causes cross-request data bleed — Request B calling `GetOrCreate(7)` gets the same object that Request A was modifying, and sees Request A's in-progress edits. `UpdateLoan` compounds this by mutating the cached instance in place rather than replacing it atomically.

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Reference sharing | `_cache[id]` stores one instance; all callers share a pointer to it | Cross-request data contamination; user B sees user A's data |
| Mutability | Cache returns a writable object with public fields | Any caller can silently alter shared state without `UpdateLoan` |
| Thread safety | `Dictionary<int, Customer>` is not thread-safe; concurrent reads/writes race | `KeyNotFoundException` or torn state under concurrent requests |
| Identity mismatch | `new Customer()` assigns Id from static `CustomerCount` counter; dictionary key is `id` parameter | Cache key and `Customer.Id` can diverge when counter and parameter differ |
| Efficiency | `ContainsKey` + indexer performs two lookups | Should use `TryGetValue` for one lookup |

**Fix priority**

1. Do not cache mutable domain entities as shared writeable objects — cache immutable DTOs or ids and reload per-request from a database.
2. If in-process caching is required, return defensive copies on read (`MemberwiseClone` as a stopgap; explicit DTO mapping preferred) so callers hold independent instances.
3. Replace `Dictionary` with `ConcurrentDictionary` and use `GetOrAdd` to eliminate the check-then-act race.
4. Replace public fields with properties and encapsulate all mutations behind validated methods.
5. Replace `ContainsKey` + indexer with `TryGetValue` for a single-lookup read path.

---

## Q24. (Scenario) A student enrollment system needs an `IDisposable` resource-holder class that wraps a file handle. Design the class correctly, explaining each decision.

**Concepts**
- `IDisposable` for deterministic resource release
- Dispose pattern: public `Dispose()` + protected virtual `Dispose(bool)`
- Finalizer as safety net, suppressed after `Dispose`
- `using` statement and `using` declaration syntax
- Avoiding double-dispose with a disposed flag

**Answer**

The standard `IDisposable` pattern separates two release paths: a deterministic path (caller explicitly calls `Dispose` or uses a `using` block) and a safety-net path (GC finalizer for objects that were never disposed). The pattern below shows both paths correctly.

```csharp
public sealed class EnrollmentFileWriter : IDisposable
{
    private FileStream? _stream;
    private bool _disposed;

    public EnrollmentFileWriter(string path)
    {
        _stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
    }

    public void WriteRecord(string record)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        byte[] data = System.Text.Encoding.UTF8.GetBytes(record + "\n");
        _stream!.Write(data, 0, data.Length);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _stream?.Dispose();
        _stream = null;
        _disposed = true;
        GC.SuppressFinalize(this);   // no need for the finalizer — already cleaned up
    }
}
```

The class is `sealed` to avoid complications with the virtual dispose pattern in base classes — if inheritance is needed, promote to the protected-virtual form. `_disposed` prevents double-dispose: calling `Dispose()` twice silently returns, which is the required contract (`Dispose` must be idempotent). `ObjectDisposedException.ThrowIf` (introduced in .NET 7) replaces the manual `if (_disposed) throw` pattern. `GC.SuppressFinalize(this)` tells the GC that no finalizer needs to run for this object when it is eventually collected, removing it from the finalization queue and allowing it to be collected in a single GC pass rather than two. Callers use `using var writer = new EnrollmentFileWriter(path)` to guarantee `Dispose` runs even when exceptions occur. For a class with unmanaged resources (e.g., a raw `IntPtr` from P/Invoke), add a finalizer (`~EnrollmentFileWriter`) that calls the internal cleanup as a safety net, and apply the protected virtual `Dispose(bool disposing)` overload pattern documented in the Microsoft guidelines.

---

## Q25. (Scenario) You are designing a domain model for a lending application. The team debates whether `LoanApplication` should be a `class` or a `record`. Walk through your decision process.

**Concepts**
- Record value equality vs class reference equality
- Immutability as a default in records
- `with` expression for non-destructive updates
- Domain entity identity vs value object identity
- Mutable state lifecycle in a DDD lending context

**Answer**

The choice hinges on two dimensions: identity semantics and mutability requirements. A `LoanApplication` in a lending domain has a lifecycle — it starts as Draft, moves to Submitted, gets Approved or Rejected, and may be Amended. It is identified by a unique `ApplicationId` (a GUID or database key), meaning two applications with the same data but different ids are different things in the business domain. This points toward a `class`: domain entities with persistent identity and a mutable state machine belong to the class model.

A `record` is the right choice when the type represents a value object — something defined entirely by its data, with no identity beyond its content. Loan terms (interest rate, duration, amount ceiling) are a good candidate: `record LoanTerms(decimal Rate, int MonthsDuration, decimal MaxAmount)`. Two `LoanTerms` with identical numbers are interchangeable. The compiler-generated value equality, `with` expressions for amended terms, and deconstruct support are all free.

For `LoanApplication` itself, use a `class` with a private setter or `init`-only properties for the identity field, and expose state transitions through methods rather than property setters. A `record class` (`record LoanApplication(...)`) is sometimes used for immutable snapshots (e.g., event sourcing projections), but mutable lifecycle entities are cleaner as ordinary classes with encapsulated state. In .NET 10, prefer `record` for DTOs, API request/response shapes, audit log entries, and configuration objects; use `class` for entities that have identity, lifecycle, and behavior.

---

## Q26. (Code Review) A report generator passes student objects by reference alias without making copies. Downstream reports show wrong historical data. Review:

```csharp
public class ReportGenerator
{
    private readonly List<Student> _snapshots = new();

    public void CaptureSnapshot(Student current)
    {
        _snapshots.Add(current);         // "snapshot" is actually an alias
    }

    public void PrintHistory()
    {
        foreach (var s in _snapshots)
            Console.WriteLine($"[Snapshot] {s}");
    }
}

// Caller:
var student = new Student("Priya Nair", 102);
generator.CaptureSnapshot(student);
student.RollNumber = 1102;              // corrects a data entry error
generator.PrintHistory();              // prints roll 1102, not 102
```

**Concepts**
- Reference assignment copies pointer, not object
- `ReferenceEquals` to detect aliasing
- Shallow clone vs deep clone
- Immutable snapshot via DTO or record
- Append-only audit log design

**Answer**

`_snapshots.Add(current)` stores the reference — the same pointer that `student` holds. When `student.RollNumber = 1102` mutates the live object, the "snapshot" in the list reflects the change because both variables point to the same heap instance. `ReferenceEquals(_snapshots[0], student)` is `true`. Historical reports show current state, not past state — the audit trail is invalid.

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Reference aliasing | `List<Student>` stores pointers to live objects | Snapshots silently update when live record changes |
| Naming mismatch | Method is called `CaptureSnapshot` but stores a live reference | Misleads maintainers; subtle under code review |
| Encapsulation | Public fields on `Student` make any holder a potential mutator | No control over who changes the "snapshot" |
| Correctness | No `ReferenceEquals` test or defensive copy | Only discovered at report time, not at capture time |

**Fix priority**

1. Store an immutable value copy at capture time — introduce a `StudentSnapshot` record: `record StudentSnapshot(string Name, int Roll, double Percentage, DateTime CapturedAt)` and map to it inside `CaptureSnapshot`.
2. If `Student` instances must be stored, call `(Student)student.MemberwiseClone()` (requires exposing a `Clone()` method) for a shallow copy, then verify no reference-type fields need deep copying.
3. Add a test that asserts `ReferenceEquals(_snapshots[0], student)` is `false` after capture.
4. Long-term, model audit history as an event stream (immutable appended events) rather than a list of mutable object references.

---

## Q27. (Scenario) A code generator produces a `CustomerEntity` partial class file with persistence logic. Your team needs to add validation and domain methods without touching the generated file. Design the solution.

**Concepts**
- `partial` class split across source files
- Generated file vs hand-authored file in same namespace
- Partial methods for generated-to-hand-authored hooks
- File-scoped namespace and `partial` keyword placement
- Build pipeline re-generation safety

**Answer**

Partial classes are the standard solution for code-generation/hand-authored splits. The generator produces `CustomerEntity.g.cs` with persistence mapping, and the team's file `CustomerEntity.cs` contains business logic — both files share `partial class CustomerEntity` in the same namespace. The compiler merges them at compile time into one type, so business methods can call persistence properties and vice versa.

```csharp
// CustomerEntity.g.cs — generated, do not edit
namespace LendingApp.Domain;

public partial class CustomerEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal LoanAmount { get; set; }

    partial void OnAfterLoad();   // generator hooks hand-authored code
}
```

```csharp
// CustomerEntity.cs — hand-authored
namespace LendingApp.Domain;

public partial class CustomerEntity
{
    public bool IsHighRisk => LoanAmount > 500_000m;

    public void ValidateLoan()
    {
        if (LoanAmount <= 0)
            throw new ArgumentOutOfRangeException(nameof(LoanAmount), "Loan amount must be positive.");
        if (string.IsNullOrWhiteSpace(Name))
            throw new InvalidOperationException("Customer name is required.");
    }

    partial void OnAfterLoad()
    {
        // runs after generator calls it — optional: compiler removes call if not implemented
    }
}
```

Key decisions: the generated file uses `partial void OnAfterLoad()` as a hook; if the hand-authored file does not implement it, the compiler silently removes the call site in the generated code (zero overhead). This pattern is used extensively in Entity Framework source generators and ASP.NET Minimal API source generators. Ensure both files share the exact same namespace and assembly — `partial` cannot span assemblies. Add the generated file to `.gitignore` if it is re-emitted by the build pipeline; add a build target ordering guarantee so the generated file exists before the compiler resolves the merged type.
