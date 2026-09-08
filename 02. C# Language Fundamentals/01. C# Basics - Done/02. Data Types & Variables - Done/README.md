# Data Types & Variables in C# — Interview Q&A

---


## Table of Contents

1. [Q1. What is the difference between a value type and a reference type in C#?](#q1-what-is-the-difference-between-a-value-type-and-a-reference-type-in-c)
2. [Q2. Where are value types and reference types stored in memory?](#q2-where-are-value-types-and-reference-types-stored-in-memory)
3. [Q3. What primitive numeric types does C# provide, and how do you choose between them?](#q3-what-primitive-numeric-types-does-c-provide-and-how-do-you-choose-between-them)
4. [Q4. What is `var` in C#, and is it the same as `dynamic`?](#q4-what-is-var-in-c-and-is-it-the-same-as-dynamic)
5. [Q5. How do nullable value types work, and what is `Nullable<T>`?](#q5-how-do-nullable-value-types-work-and-what-is-nullablet)
6. [Q6. What is the difference between `const` and `readonly`?](#q6-what-is-the-difference-between-const-and-readonly)
7. [Q7. What are the default values for common C# types?](#q7-what-are-the-default-values-for-common-c-types)
8. [Q8. What is boxing and unboxing, and why does it matter for performance?](#q8-what-is-boxing-and-unboxing-and-why-does-it-matter-for-performance)
9. [Q9. How does `==` compare value types versus reference types?](#q9-how-does-compare-value-types-versus-reference-types)
10. [Q10. What is type inference with `var`, and what are its limitations?](#q10-what-is-type-inference-with-var-and-what-are-its-limitations)
11. [Q11. What is integer overflow in C#, and how do `checked` and `unchecked` contexts affect it?](#q11-what-is-integer-overflow-in-c-and-how-do-checked-and-unchecked-contexts-affect-it)
12. [Q12. How does `string` differ from other reference types in C#?](#q12-how-does-string-differ-from-other-reference-types-in-c)
13. [Q13. What are nullable reference types, and how do they differ from nullable value types?](#q13-what-are-nullable-reference-types-and-how-do-they-differ-from-nullable-value-types)
14. [Q14. What does `default` return for different types in C#?](#q14-what-does-default-return-for-different-types-in-c)
15. [Q15. What is the purpose of literal suffixes in C#, and what happens when they are omitted?](#q15-what-is-the-purpose-of-literal-suffixes-in-c-and-what-happens-when-they-are-omitted)
16. [Q16. How do `==` and `Equals()` differ for strings that were interned versus newly allocated?](#q16-how-do-and-equals-differ-for-strings-that-were-interned-versus-newly-allocated)
17. [Q17. Why does `(double)0.1 + (double)0.2` not equal `0.3`, and how does this affect financial calculations?](#q17-why-does-double01-double02-not-equal-03-and-how-does-this-affect-financial-calculations)
18. [Q18. What happens when you call `.Value` on a null `int?`, and how should nullable values be accessed safely?](#q18-what-happens-when-you-call-value-on-a-null-int-and-how-should-nullable-values-be-accessed-safely)
19. [Q19. Why does assigning one reference-type variable to another not create a copy of the object?](#q19-why-does-assigning-one-reference-type-variable-to-another-not-create-a-copy-of-the-object)
20. [Q20. Why can a `const` field not be assigned from a method call, and what should you use instead?](#q20-why-can-a-const-field-not-be-assigned-from-a-method-call-and-what-should-you-use-instead)
21. [Q21. What is the difference between `int?` and `string?`, and why is one a runtime change and the other is not?](#q21-what-is-the-difference-between-int-and-string-and-why-is-one-a-runtime-change-and-the-other-is-not)
22. [Q22. What happens to integer overflow in unchecked arithmetic, and why is this dangerous in ID generation?](#q22-what-happens-to-integer-overflow-in-unchecked-arithmetic-and-why-is-this-dangerous-in-id-generation)
23. [Q23. Finance QA reports order totals off by one cent on some invoices. A developer submits this pricing helper from a prototype:](#q23-finance-qa-reports-order-totals-off-by-one-cent-on-some-invoices-a-developer-submits-this-pricing-helper-from-a-prototype)
24. [Q24. A loyalty API returns `int?` for optional points. After deploy, logs show both `InvalidOperationException` and `NullReferenceException`. Review this method:](#q24-a-loyalty-api-returns-int-for-optional-points-after-deploy-logs-show-both-invalidoperationexception-and-nullreferenceexception-review-this-method)
25. [Q25. A metrics exporter builds a snapshot for a dashboard. Under load, Gen2 GC collections spike. Review:](#q25-a-metrics-exporter-builds-a-snapshot-for-a-dashboard-under-load-gen2-gc-collections-spike-review)
26. [Q26. A warehouse service increments a 32-bit `int transactionId` inside a tight loop for bulk imports. In staging, IDs look fine; in production, duplicate IDs and negative values appear after long runs.](#q26-a-warehouse-service-increments-a-32-bit-int-transactionid-inside-a-tight-loop-for-bulk-imports-in-staging-ids-look-fine-in-production-duplicate-ids-and-negative-values-appear-after-long-runs)
27. [Q27. A developer models per-store configuration using `const` for a tax rate loaded from environment variables. The build fails. They propose `static readonly`. Is that sufficient?](#q27-a-developer-models-per-store-configuration-using-const-for-a-tax-rate-loaded-from-environment-variables-the-build-fails-they-propose-static-readonly-is-that-sufficient)
28. [Q28. Your team debates whether to use `var` or explicit types in service-layer financial code. A junior developer proposes using `var` everywhere for consistency.](#q28-your-team-debates-whether-to-use-var-or-explicit-types-in-service-layer-financial-code-a-junior-developer-proposes-using-var-everywhere-for-consistency)

---
## Foundation Questions

---

## Q1. What is the difference between a value type and a reference type in C#?

**Concepts**
- Value type stores data directly in the variable
- Reference type stores a pointer to heap-allocated data
- Assignment semantics: copy vs shared reference
- Default values: zero/false vs null
- Common examples: struct/enum vs class/string/array

**Answer**

Value types and reference types differ in where data lives and what assignment means. A value type — such as `int`, `bool`, `decimal`, `char`, or any `struct` — stores its data directly inside the variable. When you assign one value-type variable to another, the runtime copies all the bits, so each variable is fully independent; changing one has no effect on the other. Reference types — such as `string`, any class, or an array — store a reference (a memory address) in the variable, while the actual data lives on the managed heap. Assigning one reference-type variable to another copies only the reference, meaning both variables point at the same object. Mutating the object through one variable is visible through the other. The default value for any value type is its zero equivalent (`0`, `false`, `'\0'`) while any reference type defaults to `null`, because an uninitialised reference points at nothing. `string` is a notable reference type that behaves value-like because it is immutable: "changing" a string actually allocates a new string object and leaves the original untouched. This distinction drives decisions around equality testing, parameter passing, and memory allocation throughout C# code.

---

## Q2. Where are value types and reference types stored in memory?

**Concepts**
- Stack: method-local storage, LIFO, fast allocation
- Heap: long-lived managed memory, garbage collected
- Value type inside a class field lives on the heap with the object
- Stack frame lifetime tied to method scope
- Boxing moves a value type copy onto the heap

**Answer**

The common shorthand "value types live on the stack, reference types on the heap" is a useful starting point but needs nuance. Local variables that are value types are allocated in the current method's stack frame, which is reclaimed automatically when the method returns. The stack is extremely fast because allocation is just a pointer decrement and the lifetime is deterministic. Reference types allocate their data on the managed heap, and the garbage collector (GC) reclaims that memory when no live references remain. The critical nuance is that a value type declared as a field inside a class does not live on the stack — it is embedded directly in the class's heap-allocated memory block. Similarly, a value type captured by a lambda or iterator is promoted to a heap-allocated closure object. Boxing is the mechanism that explicitly copies a value type onto the heap: assigning an `int` to an `object` variable wraps the value in a heap-allocated wrapper. Understanding this layout matters for performance-sensitive code because excessive heap allocation drives GC pressure, while stack allocation is essentially free and produces no GC overhead.

---

## Q3. What primitive numeric types does C# provide, and how do you choose between them?

**Concepts**
- Integer family: sbyte, byte, short, ushort, int, uint, long, ulong
- Floating-point: float (32-bit), double (64-bit), decimal (128-bit)
- Binary vs base-10 arithmetic for float/double vs decimal
- Literal suffixes: f/F, L, u/U, m/M
- Default int for counting; decimal for money; double for general math

**Answer**

C# provides eight integer types covering 8-bit to 64-bit widths in both signed and unsigned variants. The everyday default is `int` (signed 32-bit, range roughly ±2.1 billion), which handles most counting, indexing, and loop variables. When values exceed `int.MaxValue` — such as database row IDs or Unix timestamps — `long` (signed 64-bit) is appropriate. `byte` (unsigned 8-bit, 0–255) suits raw binary data or small enumerations. For floating-point values C# offers `float` (32-bit, ~6–9 significant digits), `double` (64-bit, ~15–16 digits), and `decimal` (128-bit, ~28–29 digits). The critical difference is that `float` and `double` use binary (base-2) arithmetic, which cannot represent most decimal fractions exactly — `(double)0.1 + (double)0.2` evaluates to `0.30000000000000004`. `decimal` uses base-10 arithmetic, so `0.1m + 0.2m` is exactly `0.3m`. This makes `decimal` mandatory for financial calculations: prices, tax amounts, and order totals. Numeric literals require suffixes to select the right type: `3.14f` is `float`, `3.14` is `double`, `99.99m` is `decimal`, and `100L` is `long`. Mixing types without these suffixes causes silent implicit widening that can introduce the very rounding errors `decimal` was chosen to avoid.

---

## Q4. What is `var` in C#, and is it the same as `dynamic`?

**Concepts**
- var: compile-time type inference from the initializer
- Still strongly typed — type is fixed at declaration
- dynamic: bypasses compile-time type checking entirely
- var requires an initializer; dynamic does not
- Appropriate use: obvious types vs explicit types for clarity

**Answer**

`var` is a compile-time convenience keyword that instructs the compiler to infer the variable's type from its initializer expression. Once inferred, the type is fixed and fully static — the variable is no different at runtime from one declared with an explicit type. `var city = "Pune"` declares a `string`; the compiler rejects any later assignment of an incompatible type. `dynamic`, by contrast, defers all type checking to runtime: a `dynamic` variable can hold any value and any operation on it is resolved at runtime via the DLR, making type errors surface as exceptions rather than compile errors. The practical guidance is to use `var` when the type is obvious from the right-hand side — `var config = new StoreConfig(...)` — and to write the explicit type when it carries important information that `var` would hide: `decimal unitPrice = ...` communicates that precision matters, whereas `var unitPrice = ...` leaves the reader uncertain whether the value is `double` or `decimal`. `var` also cannot be used without an initializer, so `var x;` is a compile error, whereas `dynamic x;` is valid. Avoiding `dynamic` in production code is generally advisable because you lose IntelliSense, refactoring safety, and all compile-time type guarantees.

---

## Q5. How do nullable value types work, and what is `Nullable<T>`?

**Concepts**
- Value types cannot be null by default
- int? is syntactic sugar for Nullable<T>
- HasValue and Value members
- Null-coalescing operator ??
- Null-coalescing assignment ??=

**Answer**

Ordinary value types like `int` and `decimal` cannot hold `null` because they have no concept of "absent" — they always contain a numeric value. Nullable value types solve this by wrapping the value in a `Nullable<T>` struct that adds a boolean `HasValue` flag alongside the underlying `T`. The shorthand syntax `int?` is identical to `Nullable<int>` and is preferred in practice. When `HasValue` is `false`, accessing `.Value` throws `InvalidOperationException`, so code must guard against null before dereferencing. The null-coalescing operator `??` provides a concise fallback: `int points = loyaltyPoints ?? 0` returns the wrapped integer if `HasValue` is `true`, otherwise returns zero without ever touching `.Value`. The null-coalescing assignment `??=` is a write equivalent: `loyaltyPoints ??= 0` assigns zero only when `loyaltyPoints` is currently `null`. Nullable value types are common in data-access scenarios where database columns may be `NULL` — a `decimal?` maps cleanly to a nullable SQL column. They are also useful for optional API response fields. When comparing two `int?` values, the `==` operator is aware of nullability: `null == null` is `true` and `null == 5` is `false`, matching intuitive expectations without requiring explicit `HasValue` checks in every equality comparison.

---

## Q6. What is the difference between `const` and `readonly`?

**Concepts**
- const: compile-time constant, value inlined by compiler
- readonly: runtime constant, set at declaration or in constructor
- const is implicitly static on class fields
- readonly allows per-instance values
- const only allows primitive types and string; readonly allows any type

**Answer**

Both `const` and `readonly` prevent a field or variable from being reassigned after initialisation, but they operate at different points in time and carry different constraints. A `const` value must be known at compile time — the compiler evaluates it and inlines it directly into every call site, which means the assembly itself contains the literal value rather than a reference to a memory location. `const` is therefore limited to primitive numeric types, `bool`, `char`, and `string`. It is implicitly `static` on class members, so there is only one copy regardless of how many instances exist. A `readonly` field is evaluated at runtime: it can be assigned either at the declaration site or once inside any constructor, after which it cannot be changed. This makes `readonly` suitable for values that are fixed per-instance but unknown until construction — such as a store code read from configuration. Because `readonly` holds a reference at runtime, it can also store any type including class instances, making it far more flexible than `const`. The practical rule of thumb: use `const` for mathematical constants, maximum limits, and fixed string keys that will never change across deployments; use `readonly` for configuration values injected at startup or per-instance data known only when the object is constructed.

---

## Q7. What are the default values for common C# types?

**Concepts**
- Value types default to their zero equivalent
- Reference types default to null
- default keyword and default(T) expression
- Unassigned local variables are a compile error
- Fields vs local variables: fields get automatic defaults

**Answer**

C# guarantees that every type has a well-defined default value. For numeric value types the default is zero: `int` defaults to `0`, `double` to `0.0`, `decimal` to `0.0m`. `bool` defaults to `false`, `char` to `'\0'` (the null character), and any `struct` has all its fields set to their own defaults. Reference types — including `string`, classes, and arrays — default to `null`. The `default` keyword (or the `default(T)` expression in older syntax) produces this default for any type, which is useful in generic code where `T` is unknown. An important distinction exists between class fields and local variables: class fields and static fields are automatically initialised to their default value before any user code runs, so an unassigned `int` field on a class is `0` without any explicit assignment. Local variables inside a method do not receive this treatment — reading an unassigned local is a compile error (CS0165), not a runtime null-reference surprise. This compile-time enforcement means C# catches the vast majority of uninitialized-variable bugs before the code ever runs, which is one of its safety advantages over C and C++.

---

## Q8. What is boxing and unboxing, and why does it matter for performance?

**Concepts**
- Boxing: wrapping a value type in a heap-allocated object
- Unboxing: casting the object back to the value type
- Triggered by assigning value type to object or interface variable
- Heap allocation and GC pressure implications
- InvalidCastException on incorrect unbox type

**Answer**

Boxing is the automatic process of copying a value type's data into a newly allocated heap object so it can be stored where an `object` or interface reference is expected. When you write `object boxed = 42`, the runtime allocates a small wrapper object on the managed heap, copies the integer `42` into it, and stores a reference in `boxed`. Unboxing is the reverse: the explicit cast `(int)boxed` copies the value back out of the heap wrapper and into a local stack variable. Both operations have costs that matter in hot paths. Boxing performs a heap allocation and triggers write-barrier instrumentation, contributing to GC pressure; unboxing performs a type check at runtime and throws `InvalidCastException` if the boxed type does not exactly match the cast type — unboxing a boxed `int` as `long` fails even though an implicit widening conversion exists between them. Pre-generics C# collections like `ArrayList` boxed every value type stored in them, causing measurable overhead in tight loops. Modern code largely avoids boxing by using generic collections (`List<int>` instead of `ArrayList`) and generic constraints. Remaining hot spots include passing value types to `string.Format` or logging frameworks that accept `params object[]` — structured logging APIs and `FormattableString` exist precisely to avoid this pattern.

---

## Q9. How does `==` compare value types versus reference types?

**Concepts**
- Value types: == compares the actual values (structural equality)
- Reference types: == compares references (identity) by default
- string overloads == for value equality
- Equals() method vs == operator
- ReferenceEquals() for explicit identity check

**Answer**

The behaviour of `==` depends entirely on the types involved and any operator overloading applied to them. For value types such as `int`, `decimal`, and `struct`, the compiler generates code that compares the actual stored values, so `5 == 5` is always `true`. For reference types, the default `==` operator inherited from `object` compares the memory addresses of the two references — two separate `List<int>` objects with identical contents are not `==` because they are different heap objects. The important exception is `string`: the C# `string` class overloads `==` to compare the characters in the two strings rather than their references, so two `string` variables containing `"hello"` are `==` even if they are different heap objects. The `Equals()` method mirrors this pattern: `object.Equals` defaults to reference equality, but it can be overridden to provide value semantics, as `string` does. `ReferenceEquals(a, b)` always tests object identity regardless of any overloads — it returns `true` only when both references point at the exact same heap object. For user-defined `struct` types, `==` is not automatically defined; you must overload it or rely on `Equals()`. In practice, understanding this layering prevents subtle bugs where two strings that look equal fail an `object`-typed equality check that was not routed through `string`'s overloaded operator.

---

## Q10. What is type inference with `var`, and what are its limitations?

**Concepts**
- Compiler infers the static type from the initializer expression
- var requires an initializer at declaration
- Cannot declare var without assignment
- Cannot use var for method parameters or return types
- Type is sealed at declaration — no later coercion

**Answer**

`var` delegates type selection to the compiler, which examines the initializer expression and assigns the most specific type it can determine. `var count = 10` becomes `int`, `var rate = 0.18m` becomes `decimal`, and `var config = new StoreConfig(...)` becomes `StoreConfig`. The resulting variable is completely statically typed — there is no runtime overhead and no reduction in type safety. The limitations follow logically from this design. First, `var` requires an initializer: `var x;` is a compile error because there is nothing to infer from. Second, `var` is not valid for method parameters, return types, or field declarations (outside of top-level statements and local functions) — those positions require an explicit type because the compiler cannot infer across usage sites. Third, the inferred type is fixed at declaration and cannot change; assigning an incompatible type later is a compile error just as it would be with an explicit type. A practical subtlety is that `var` always infers the right-hand side's declared type, not the runtime type: `var store = GetStore()` is the return type of `GetStore()`, which might be an interface even if the actual object is a subclass. Overuse of `var` in financial or numeric code can mask type choice — `var total = a + b` when `a` is `double` and `b` is `int` silently produces a `double`, whereas `decimal total = a + b` would be a compile error that forces an intentional decision.

---

## Q11. What is integer overflow in C#, and how do `checked` and `unchecked` contexts affect it?

**Concepts**
- Default arithmetic is unchecked — overflow wraps silently (two's complement)
- checked block or keyword throws OverflowException on overflow
- unchecked block suppresses the check even when project-level checked is on
- int.MaxValue + 1 wraps to int.MinValue in unchecked context
- Affects integral types only; float/double use IEEE 754 infinity instead

**Answer**

Integer overflow occurs when an arithmetic result exceeds the representable range of its type. For a 32-bit signed `int`, adding `1` to `int.MaxValue` (2,147,483,647) produces a result that does not fit in 32 bits. By default, C# operates in an unchecked context for integral arithmetic: the bits simply wrap around using two's complement, so `int.MaxValue + 1` silently becomes `int.MinValue` (-2,147,483,648) with no exception and no warning. This is the same behaviour as C and C++, chosen for performance because overflow checks add a branch to every arithmetic operation. The `checked` keyword or block changes this contract: inside a `checked` block, any overflow throws `OverflowException` at runtime, converting a silent corruption into a detectable crash. The `unchecked` keyword can appear inside a globally-checked project to suppress checks in a specific expression when wrap-around is intentional, such as when computing hash codes. Floating-point types are unaffected by `checked`/`unchecked` because they use IEEE 754 infinity and NaN to represent out-of-range results rather than integer overflow. The practical guidance is to use `checked` in financial counters, ID generators, and any accumulation that could realistically exhaust a 32-bit range over time, and to prefer `long` proactively when there is any doubt about whether values will stay within `int`'s range in production workloads.

---

## Q12. How does `string` differ from other reference types in C#?

**Concepts**
- string is a reference type but immutable
- Assignment creates a new string object; original unchanged
- String interning: identical literals may share references
- == overloaded to compare content, not reference
- StringBuilder for mutable string accumulation

**Answer**

`string` is declared as a class in .NET, making it a reference type — a `string` variable holds a reference to a heap-allocated sequence of UTF-16 characters. However, `string` is also immutable: once created, the character sequence inside a string object never changes. Every operation that appears to "modify" a string — concatenation, replacement, trimming — actually allocates a new string object and returns a reference to it. The original object is left intact, which is why assigning `secondaryLabel = "Deliver"` does not affect `primaryLabel` that was previously set to `"Ship"`, even though both variables once held the same reference. This immutability makes string safe to share across threads and to use as dictionary keys, but it means naive repeated concatenation in a loop allocates many intermediate strings. The `StringBuilder` class addresses this with a mutable internal buffer that minimises allocation during heavy string construction. The `==` operator on `string` is overloaded to compare character content rather than reference identity, so two separately allocated strings with the same content are `==`. The CLR also applies string interning, whereby identical string literals in the same assembly may be stored only once — meaning two `string` variables holding the same literal could actually point to the same heap object, though this is an implementation detail rather than a language guarantee and should not be relied upon for correctness.

---

## Q13. What are nullable reference types, and how do they differ from nullable value types?

**Concepts**
- Nullable reference types: compile-time annotation, no runtime change
- Enabled via `<Nullable>enable</Nullable>` in the project file
- string? vs string — communicates intent to the compiler
- Nullable value types (int?): a true Nullable<T> struct wrapping the value
- Null-forgiving operator ! suppresses compiler warnings

**Answer**

Nullable value types (`int?`, `decimal?`) and nullable reference types (`string?`, `MyClass?`) serve similar communicative purposes but work at entirely different levels. A nullable value type is a true runtime type change: `int?` is `Nullable<int>`, a struct with a `HasValue` flag, and its size is larger than plain `int`. Without `?`, an `int` cannot hold `null` at runtime — the compiler and CLR enforce this. Nullable reference types, introduced in C# 8 and enabled with `<Nullable>enable</Nullable>` in the project file, are purely a compile-time annotation. At runtime, `string?` and `string` are identical — they are both `System.String` — and there is no additional struct wrapper. The `?` annotation is a contract expressed to the compiler: `string?` tells the compiler "this reference may legitimately be null; warn me if I dereference it without a null check", while plain `string` means "I intend this to always be non-null; warn me if I assign null to it". The null-forgiving operator `!` (`definiteMessage = resolvedMessage!`) suppresses the warning when the developer has out-of-band knowledge that a nullable reference is actually non-null at that point. The practical value of enabling nullable reference types project-wide is that null-reference exceptions, historically one of the most common runtime crashes in .NET, become compile-time warnings or errors that are caught before deployment.

---

## Q14. What does `default` return for different types in C#?

**Concepts**
- default(T) returns the zero-equivalent for value types
- default(T) returns null for reference types
- Unparameterised default infers T from the declaration context
- Useful in generic methods where T is unknown
- Fields use implicit defaults; locals require explicit assignment

**Answer**

The `default` keyword produces the zero-like initial value for any type, determined at compile time. For numeric types it is `0` (or `0.0`, `0.0m`), for `bool` it is `false`, for `char` it is `'\0'`, and for any reference type it is `null`. In generic code the pattern `T result = default!;` initializes a local to the default of whatever `T` is at call time — whether `T` is `int`, `string`, or a custom class — without knowing which branch applies. Since C# 7.1 the unparameterised form `default` can appear wherever the type is unambiguous from context: `int x = default;` is equivalent to `int x = default(int);` and is slightly cleaner. An important design choice in C# is that class fields and static fields receive their type's default automatically before any user code runs, so an `int` field on a class is `0` without any explicit `= 0`. Local variables inside methods do not receive this treatment; the compiler statically checks that every code path assigns a value before the variable is read, and CS0165 is emitted if any path is unassigned. This asymmetry exists because automatic zero-initialisation of locals would hide bugs where a developer forgets to assign a meaningful value — fields have a wider scope and are harder to trace statically, so the runtime zeros them as a safety baseline.

---

## Q15. What is the purpose of literal suffixes in C#, and what happens when they are omitted?

**Concepts**
- f or F suffix: float literal
- m or M suffix: decimal literal
- L suffix: long literal
- u or U suffix: uint literal
- Default unsuffixed integer literal is int; default floating-point is double

**Answer**

Numeric literal suffixes tell the compiler the exact type a literal represents, which matters because different types have different precision and arithmetic behaviour. An unsuffixed decimal-point literal like `3.14` is a `double` — assigning it to a `float` variable requires an explicit suffix (`3.14f`) or a cast. An unsuffixed integer literal like `42` is an `int` unless it overflows `int.MaxValue`, in which case the compiler widens it to `long` automatically. The `m` or `M` suffix is critical for financial code: `0.18` is a `double` literal and carries binary rounding error, while `0.18m` is a `decimal` literal with exact base-10 representation. Forgetting the `m` suffix is a common source of penny-off bugs — `double subtotal * 0.18` performs all arithmetic in binary floating-point, and casting the final result to `decimal` does not recover the lost precision. The `L` suffix marks a `long` literal; without it, a literal that fits in `int` is `int`, so `long id = 9_007_199_254_740_993` requires the `L` suffix to avoid an implicit widening conversion from `int` at the call site. Underscore separators (`1_400_000_000`) are also valid inside numeric literals from C# 7 onward and improve readability for large numbers without affecting the value or type.

---

## Q16. How do `==` and `Equals()` differ for strings that were interned versus newly allocated?

**Concepts**
- String interning: identical literals share one heap object
- == on string: overloaded for content equality
- ReferenceEquals: always identity, bypasses overloads
- Equals(StringComparison): culture and case control
- Intern table: String.Intern / String.IsInterned

**Answer**

The CLR maintains an intern table where string literals that appear in compiled assemblies are stored once. When two variables hold identical string literals, they may share the same reference — `ReferenceEquals("hello", "hello")` returns `true` in most cases because the literals are interned. Programmatically constructed strings, such as those from `new string(chars)`, `StringBuilder.ToString()`, or string concatenation at runtime, are not automatically interned and each occupies its own heap object. This distinction becomes observable when `ReferenceEquals` is used: two separately constructed strings with identical content return `false` from `ReferenceEquals` even though `==` and `Equals()` return `true`, because `string` overloads both for content comparison. The practical implication is that code should never rely on `ReferenceEquals` to compare strings by value, nor should it rely on interning as a performance optimization for lookup-by-identity. `String.Intern(s)` can explicitly intern a runtime string, returning the canonical reference from the intern table, but interned strings are never collected by the GC during the process lifetime, making this a memory trade-off. For case-insensitive or culture-aware comparison, `string.Equals(a, b, StringComparison.OrdinalIgnoreCase)` is the correct form — `==` always performs ordinal case-sensitive comparison and has no overload that accepts a `StringComparison`, so cultural correctness requires the explicit `Equals` call.

---

## Foundation Questions — End

---

## Gotchas — Data Types & Variables (Interview Traps)

---

#### Gotcha 1. float arithmetic silently loses precision — 0.1f + 0.2f != 0.3f

**Concepts**
- Binary floating-point cannot represent most decimal fractions
- Accumulated rounding error
- decimal type uses base-10 arithmetic
- Comparing double with == is unreliable

**Answer**

The IEEE 754 binary representation of 0.1 is a repeating fraction, so 0.1f + 0.2f produces approximately 0.30000001 rather than 0.3. Financial calculations must use decimal, which performs base-10 arithmetic and represents 0.1m, 0.2m, and their sum exactly.

---

#### Gotcha 2. Nullable<T>.Value throws InvalidOperationException when HasValue is false — not NullReferenceException

**Concepts**
- int? is Nullable<int> struct
- HasValue property guards Value
- Null-coalescing ?? for safe fallback
- GetValueOrDefault() returns T default without throw

**Answer**

Accessing .Value on a null Nullable<T> throws InvalidOperationException, not NullReferenceException — a distinction that confuses developers expecting reference-type behavior. The safe access patterns are the null-coalescing operator (points ?? 0), the GetValueOrDefault() method, or a null pattern match (if (points is int actual)).

---

#### Gotcha 3. Assigning one reference-type variable to another copies the reference, not the object

**Concepts**
- Reference copy vs heap object copy
- Both variables point to same heap object
- Mutation through one is visible via the other
- MemberwiseClone for shallow copy

**Answer**

After StoreConfig b = a, both variables hold the same memory address pointing to one object on the heap. Changing a property through b is immediately visible when reading through a, because there is only one underlying object. Creating a truly independent copy requires a deep-copy strategy or using record with expression to produce a new instance.

---

#### Gotcha 4. const fields are inlined at compile time — consumers must recompile to pick up a changed value

**Concepts**
- const is compile-time constant expression
- Method calls are runtime; CS0133 error for non-constant
- static readonly evaluated once at class load time
- Versioning trap for shared libraries

**Answer**

The C# compiler replaces every reference to a const field with its literal value at compile time. This means changing a const in a shared library and redeploying only the library leaves all consuming assemblies with the old value baked in until they are recompiled, a versioning trap that static readonly avoids.

---

#### Gotcha 5. int? and string? use completely different mechanisms despite identical ? syntax

**Concepts**
- int? is Nullable<int> — different runtime type with HasValue
- string? is compiler annotation only — same runtime type as string
- Nullable context enables reference type annotations
- Null-forgiving ! operator

**Answer**

For value types, the ? suffix changes the underlying type from System.Int32 to System.Nullable<System.Int32> — a genuine runtime difference with a new struct wrapping. For reference types, the ? is a compile-time nullability annotation only; System.String and System.String? are the same runtime type, and the annotation simply arms the compiler's flow analysis to warn about unsafe dereferences.

---

#### Gotcha 6. Integer overflow wraps silently to the minimum value in the default unchecked context

**Concepts**
- Default arithmetic is unchecked (no overflow detection)
- int.MaxValue + 1 == int.MinValue
- checked keyword or block detects and throws OverflowException
- Use long or checked for counters near type boundaries

**Answer**

In C#'s default unchecked arithmetic context, int.MaxValue + 1 silently produces -2,147,483,648 (int.MinValue) via two's-complement wraparound, with no exception and no warning. ID generators or transaction counters that approach int.MaxValue will silently wrap and produce duplicate IDs; use long proactively or wrap the arithmetic in a checked block to get a clear OverflowException.

---

#### Gotcha 7. var is statically typed — the inferred type is fixed at compile time and cannot change

**Concepts**
- Compiler infers the type at declaration
- var x = 42 is int, not object
- IntelliSense shows the inferred type
- Different from dynamic which defers to runtime

**Answer**

var instructs the compiler to infer the type from the right-hand side initializer, producing a strictly typed variable identical to one declared with an explicit type name. Writing var x = 42; then x = "hello"; is a compile error CS0029 because x has been inferred as int, not a shape-shifting container.

---

#### Gotcha 8. Local variables are NOT zero-initialized — reading before assignment is a compile error CS0165

**Concepts**
- CLR zero-initializes class fields
- Local variable reads before assignment = CS0165
- out parameters must be assigned before method returns
- Nullable analysis tracks definite assignment

**Answer**

Unlike class fields which the CLR automatically zero-initializes, local variables must be explicitly assigned before they are read. The compiler's definite assignment analysis enforces this at compile time (CS0165), so the error surfaces before any test can catch it. Fields, by contrast, are always zero-initialized (false for bool, 0 for numerics, null for references).

---

#### Gotcha 9. Widening numeric conversions can still lose data at extreme boundary values

**Concepts**
- long to int truncates high bits
- double to float loses precision
- ulong to long wraps for values > long.MaxValue
- checked block catches overflow in explicit casts

**Answer**

Implicit conversions like int to long are always safe, but explicit narrowing conversions like (int)longValue silently truncate the high bits for values outside int's range. Even 'widening' in the sense of range — like double to float — loses decimal precision because float has fewer mantissa bits, producing unexpected rounding in financial or scientific code.

---

#### Gotcha 10. string is a reference type but behaves like a value type because it is immutable

**Concepts**
- string inherits from object (reference type)
- String interning
- == compares content not reference
- Immutability means mutations return new string

**Answer**

Despite being a reference type that lives on the heap, string overloads == to compare character content rather than reference identity, so "hello" == "hello" is true even if they are different heap objects. This value-like behavior stems from immutability — every string operation that appears to modify the string actually returns a new string, leaving the original unchanged.

---

## Real-World Scenarios

---

## Q23. Finance QA reports order totals off by one cent on some invoices. A developer submits this pricing helper from a prototype:

```csharp
// Target: .NET 10 (net10.0)
public decimal CalculateOrderTotal(double unitPrice, int quantity)
{
    double subtotal = unitPrice * quantity;
    double tax = subtotal * 0.18;
    return (decimal)(subtotal + tax);
}
```

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Type choice | `unitPrice` declared as `double`; all arithmetic is binary floating-point | Rounding error introduced before any decimal conversion |
| Constant precision | `0.18` is a `double` literal; tax multiplier carries binary approximation | Compounds floating-point error in every invoice |
| Late conversion | `(decimal)` cast applied to final `double` result | Preserves, does not fix, the accumulated rounding error |
| Parameter type | `unitPrice` should be `decimal` at the API boundary | Any caller passing a `double` silently introduces error |

**Fix priority**
1. Change `unitPrice` parameter type to `decimal`
2. Change all intermediate locals to `decimal`
3. Change the tax literal to `0.18m`
4. Remove the cast — the return type is already `decimal`

**Answer**

The core defect is that all arithmetic runs in `double`, a binary floating-point type that cannot represent `0.18` exactly. The binary approximation of `0.18` is `0.17999999999999999289...`, and multiplying this by an 8-digit price accumulates an error in the fourth or fifth decimal place. Casting the final `double` to `decimal` at the end does not recover the lost precision — it preserves the rounded binary value and merely re-expresses it in a decimal type. The fix is to perform all arithmetic in `decimal` from the start: declare `unitPrice` as `decimal`, use the `0.18m` literal for the tax rate, and let `decimal` multiplication produce an exact base-10 result. Because `decimal` uses base-10 arithmetic internally, `0.1m + 0.2m == 0.3m` is `true`, and `unitPrice * 0.18m` for any `decimal` price produces a result that rounds consistently to two decimal places. This fix must extend to the API boundary — the parameter type should be `decimal` so callers cannot accidentally pass a pre-rounded `double`. In practice, monetary values should be represented as `decimal` from the point of input (parsing user input with `decimal.Parse` or reading from a database column typed as `DECIMAL`) through all calculation layers to the output, with rounding applied once at the final display step using `Math.Round(total, 2, MidpointRounding.AwayFromZero)` for currency rounding rules.

---

## Q24. A loyalty API returns `int?` for optional points. After deploy, logs show both `InvalidOperationException` and `NullReferenceException`. Review this method:

```csharp
// Target: .NET 10 (net10.0)
public int ComputeBonus(int? loyaltyPoints, string? tierCode)
{
    var bonus = loyaltyPoints.Value * 2;
    if (tierCode.Equals("Gold", StringComparison.OrdinalIgnoreCase))
        bonus += 50;
    return bonus;
}
```

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Nullable value type | `.Value` accessed without `HasValue` check | `InvalidOperationException` when `loyaltyPoints` is null |
| Nullable reference type | `tierCode.Equals(...)` called without null guard | `NullReferenceException` when `tierCode` is null |
| var inference | `bonus` inferred as `int` from `.Value` — fine if prior bugs are fixed | No issue after the null guard is added |
| API contract | No documentation of what null inputs produce | Callers cannot reason about return value |

**Fix priority**
1. Replace `.Value` with null-coalescing: `loyaltyPoints ?? 0`
2. Replace `tierCode.Equals(...)` with `string.Equals(tierCode, "Gold", StringComparison.OrdinalIgnoreCase)` (null-safe static overload)
3. Add XML doc comment specifying null input behavior

**Answer**

Both exceptions share the same root cause: the method assumes non-null inputs but the parameter types declare both inputs as potentially null. `loyaltyPoints.Value` is the direct trigger for `InvalidOperationException` — when `HasValue` is `false`, this property throws rather than returning zero. The null-coalescing operator resolves this cleanly: `var bonus = (loyaltyPoints ?? 0) * 2` evaluates to zero when the API returns no points and avoids `.Value` entirely. The `NullReferenceException` from `tierCode.Equals(...)` is a classic pattern error: instance method calls on possibly-null references throw rather than silently return false. The null-safe fix is `string.Equals(tierCode, "Gold", StringComparison.OrdinalIgnoreCase)` — the static overload of `string.Equals` returns `false` when either argument is null rather than throwing. After both fixes the method's logic is sound for all input combinations, but it should also be clear to callers whether null `loyaltyPoints` means "no points" (return zero bonus) or "unresolvable member" (throw `ArgumentNullException`). If the business rule is that null points means the customer has no loyalty account and earns no bonus, then the `?? 0` approach is correct. If null means an error state that should be escalated, the method should validate early and throw `ArgumentNullException`. Whichever contract is chosen should be documented in XML doc comments so that callers can handle the method correctly without reading the implementation.

---

## Q25. A metrics exporter builds a snapshot for a dashboard. Under load, Gen2 GC collections spike. Review:

```csharp
// Target: .NET 10 (net10.0)
public IReadOnlyList<object> BuildDailyCounts(IEnumerable<int> orderCounts)
{
    var snapshot = new List<object>();
    foreach (var count in orderCounts)
        snapshot.Add(count);
    return snapshot;
}
```

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Boxing | Each `int` is boxed to `object` on `snapshot.Add(count)` | One heap allocation per element; large inputs produce thousands of objects |
| Return type | `IReadOnlyList<object>` forces callers to unbox or cast every element they use | Unboxing cost at every consumer; loses type information |
| GC pressure | Thousands of small short-lived object allocations fill Gen0, promote to Gen2 | Increased GC pause frequency and duration under load |
| Type safety | Callers receive `object` elements with no compile-time count guarantee | `InvalidCastException` risk on any downstream cast |

**Fix priority**
1. Change return type to `IReadOnlyList<int>`
2. Change local list to `List<int>` — no boxing occurs with generic collections
3. If pre-allocation is possible, pass capacity: `new List<int>(orderCounts.TryGetNonEnumeratedCount(out int c) ? c : 0)` to avoid list resizing

**Answer**

The performance problem is boxing. Every time `snapshot.Add(count)` is called, the `int` value type is wrapped in a heap-allocated `object` — a new managed object is created for each element. At dashboard scale with thousands of data points, this produces thousands of small short-lived allocations that saturate the Gen0 heap, are promoted to Gen1 after surviving a GC pass, and eventually to Gen2 if the list outlives the Gen1 threshold — exactly the Gen2 spike the monitoring shows. The root cause is the `List<object>` collection type and `IReadOnlyList<object>` return type: a generic collection parameterized on `object` is the same as a pre-generics `ArrayList` from the developer's perspective. The fix is to use the typed generic: `List<int>` stores integers directly in its backing array without any heap allocation per element, because value types in generic collections are not boxed. The caller receiving `IReadOnlyList<int>` also saves the unboxing cost at every access site. An additional micro-optimization is to pre-size the list when the input count is known: `List<int>` doubles its backing array each time it resizes, causing extra allocations and copies; `new List<int>(expectedCount)` allocates once. In .NET 10, `IEnumerable<T>.TryGetNonEnumeratedCount()` can retrieve the count without enumerating when the source is a concrete collection, enabling pre-sized allocation without requiring the caller to pass a count separately.

---

## Q26. A warehouse service increments a 32-bit `int transactionId` inside a tight loop for bulk imports. In staging, IDs look fine; in production, duplicate IDs and negative values appear after long runs.

**Answer**

The bug is silent integer overflow in an unchecked arithmetic context. C#'s default unchecked arithmetic wraps a signed 32-bit `int` from `int.MaxValue` (2,147,483,647) to `int.MinValue` (-2,147,483,648) when incremented, with no exception and no warning. In a staging environment with a small import file this never occurs — the counter never approaches the boundary. In production, after months of bulk imports accumulating over two billion transactions, the counter wraps, produces negative IDs, then continues counting toward zero where it collides with IDs from the system's earliest records. The team's claim that "C# integers don't overflow in normal use" reflects a common misconception: overflow is silently accepted by default, and the boundary is reachable in high-volume systems on a multi-year timeline. The correct fix has two parts. First, change the type from `int` to `long` (signed 64-bit, maximum 9.2 × 10^18), which is effectively inexhaustible for any practical transaction volume. Second, add a `checked` block around the increment anyway — even for `long` — so that if the counter ever approaches `long.MaxValue` an `OverflowException` alerts the operations team rather than silently corrupting ID uniqueness. A complementary operational measure is to monitor the current maximum ID value in the database and alert when it exceeds a threshold (e.g., 90% of the type's maximum), giving the team time to respond before the boundary is reached in production.

---

## Q27. A developer models per-store configuration using `const` for a tax rate loaded from environment variables. The build fails. They propose `static readonly`. Is that sufficient?

**Answer**

The build fails with CS0133 because `const` requires a compile-time constant expression, and `decimal.Parse(Environment.GetEnvironmentVariable("TAX_RATE") ?? "0.18")` is a runtime method call. The compiler cannot evaluate it before the program starts. `static readonly` solves the CS0133 error: a static field initializer runs once when the class is first loaded, so `LoadTaxRateFromConfiguration()` executes at application startup and the result is stored in the field for the process lifetime. However, `static readonly` with direct `Environment.GetEnvironmentVariable` access has problems for production ASP.NET Core or Worker Service code: it is read once at class initialization time, not on each request, so changes to the environment variable require a full process restart. More critically, a `static readonly` field is global state that cannot be overridden in unit tests — any test that exercises pricing logic must have the environment variable set correctly or the static initializer produces a wrong or default value. The recommended production pattern is dependency injection with `IOptions<PricingOptions>`: declare a `PricingOptions` POCO class with a `StandardTaxRate` property, bind it to the configuration system in `Program.cs`, and inject `IOptions<PricingOptions>` into the services that need the rate. This approach makes the tax rate testable (inject different options in tests), configurable without code changes, and reloadable at runtime if `IOptionsMonitor<T>` is used. The `const` and `static readonly` approaches are appropriate for truly invariant values that never differ between environments or deployments.

---

## Q28. Your team debates whether to use `var` or explicit types in service-layer financial code. A junior developer proposes using `var` everywhere for consistency.

**Answer**

The "use `var` everywhere" policy is reasonable in CRUD and utility code but becomes risky in financial service-layer code for one concrete reason: type inference silently selects the type the compiler sees, which may not be the type the developer intended. In financial code the difference between `double` and `decimal` is the difference between correct and incorrect arithmetic, and `var total = price * quantity` gives no indication whether `total` is `decimal` (safe) or `double` (risky). If `price` is later refactored from `decimal` to `double` — perhaps by another developer changing a shared library — the `var total` assignment continues to compile without warning, and the service now silently performs binary floating-point arithmetic for all calculations downstream. Writing `decimal total = price * quantity` makes the intent explicit and causes a compile error if `price` ever becomes a type that cannot implicitly convert to `decimal`, surfacing the problem immediately. The team rule that serves most codebases is: use `var` freely when the type is unambiguously clear from the right-hand side (`var config = new StoreConfig(...)`, `var items = new List<OrderLine>()`) and when precision does not carry domain risk. Write explicit types when the type choice carries domain meaning (`decimal` for money, `long` for IDs), when the method return type is abstract or ambiguous, or when a future refactor silently changing the inferred type would introduce correctness bugs. The goal of explicit types in these cases is not verbosity — it is making type contracts visible to reviewers and refactoring tools.

---

## Real-World Scenarios — End
