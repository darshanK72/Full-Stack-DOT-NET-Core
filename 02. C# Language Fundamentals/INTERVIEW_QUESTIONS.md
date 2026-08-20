# C# Language Fundamentals — Interview Questions (Extended Reference Bank)

Organized by the curriculum folder structure under `02. C# Language Fundamentals`.  
Overlapping questions are deduplicated; each topic appears once in its best-fit chapter.  
**Gotchas** at the end of each module are real interview traps — patterns candidates commonly miss, not forced trivia per API.

> **Scope:** Modules 01–09 only. ASP.NET Core and EF Core live in separate repo folders (`05. ASP.NET Core`, `04. .NET Data Access/03. Entity Framework Core`).  
> **Purpose:** Long-term reference bank — questions only (no answers). Coverage-driven; chapter count varies by topic depth.

---

## Module 01. C# Basics

### 01. Hello World
1. What is C# and what are its key features?
2. Explain namespaces in C#.
3. How do nested namespaces work in C#?
4. What is the purpose of the `using` directive (importing namespaces)?
5. Explain preprocessor directives in C# (`#if`, `#define`, `#region`, `#pragma`, etc.).
6. What is the role of the `Main` method, and how has entry-point syntax evolved (classic `Main`, top-level statements)?
7. What is the difference between a project, a solution, and an assembly in a .NET workspace?
8. What does the `global using` directive do (C# 10+), and when is it useful?
9. Explain file-scoped namespaces (`namespace X;`) vs block-scoped namespace syntax.
10. What is the purpose of `Program.cs` in a console application, and what other files typically accompany it (`.csproj`, `global usings`)?
11. What is the Common Language Runtime (CLR), and how does C# code become executable?
12. What is the difference between compiling to IL and JIT compilation at runtime?
13. What are SDK-style projects, and what does `<TargetFramework>` in the `.csproj` control?
14. When would you use `#nullable enable` at the project or file level?
15. What is the difference between `Console.Out`, `Console.Error`, and writing directly with `Console.WriteLine`?

### 02. Data Types & Variables
1. What are the different data types in C#?
2. What are value types and reference types in C#?
3. What is the difference between value types and reference types?
4. What is boxing and unboxing in C#?
5. Explain the `var` keyword in C#.
6. What are nullable types in C#? (including nullable reference types in C# 8+)
7. Explain the `default` keyword and default values in C#.
8. What are constants, literals, and readonly fields in C#?
9. What is the difference between `const` and `readonly`?
10. What is an enum in C#?
11. What is a `struct` in C#? (basics — comparison with `class` is in OOP)
12. What is a tuple in C#? (ValueTuple vs `Tuple<T>`)
13. Where do value types typically live (stack vs heap), and where do reference types live?
14. When a value type is boxed, where does the data end up, and why does that matter for performance?
15. What is the difference between `int`, `long`, `decimal`, `float`, and `double` — when would you choose each?
16. What is the difference between signed and unsigned integer types (`int` vs `uint`, etc.)?
17. What is `char` in C# — is it a numeric type or a text type, and how does it relate to Unicode?
18. What is the difference between `bool` and nullable `bool?` in terms of default values and usage?
19. Explain the `??` (null-coalescing) and `??=` (null-coalescing assignment) operators with nullable types.
20. What is the difference between `var` and an explicit type declaration — when must you use explicit types?
21. What are digit separators in numeric literals (e.g., `1_000_000`), and what problem do they solve?
22. What is the difference between `default(int)` and `default` for a reference type?
23. What is a nullable reference type annotation (`string?` vs `string`), and is enforcement compile-time or runtime?
24. What happens when you assign `null` to a non-nullable reference type variable under `#nullable enable`?
25. What is the difference between `object` as a universal base type and using `dynamic`?
26. What are `nint` and `nuint`, and when might you encounter them?
27. What is the difference between declaring a variable with and without an initializer?
28. Can you use `const` with user-defined types like `DateTime` or `decimal` computed at runtime? Why or why not?

### 03. Input & Output
1. What is the difference between `Console.WriteLine`, `Console.Write`, and string interpolation for output?
2. What is the difference between `Console.ReadLine()` and `Console.ReadKey()`?
3. How do you safely parse user input (`int.TryParse`, `Parse`, `Convert`) and handle invalid input?
4. How does formatted console output work (`Console.WriteLine("{0}", value)` vs interpolation)?
5. What is the difference between `CultureInfo.CurrentCulture`, `CurrentUICulture`, and `InvariantCulture`?
6. When should you use `InvariantCulture` for formatting numbers and dates instead of `CurrentCulture`?
7. How do culture settings affect decimal separators, currency symbols, and date formats in console output?
8. What is composite formatting (`string.Format`, `{0:N2}`, alignment `{0,10}`, `{0,-10}`)?
9. What is the difference between `Console.InputEncoding` and `Console.OutputEncoding`, and why can mismatched encodings garble console text?
10. How do you capture console output programmatically (e.g., `StringWriter` redirected to `Console.SetOut`)?
11. What is the difference between `Console.Read` and `Console.ReadLine`?
12. How do you format output with alignment and padding using interpolation (`$"{value,10}"`, `$"{value:N2}"`)?
13. What is `IFormattable`, and how does it relate to custom formatting in `ToString(format, provider)`?
14. What happens if you call `int.Parse` on invalid input vs `int.TryParse` — which pattern is preferred in production console apps?
15. How does changing `CultureInfo.CurrentCulture` on the current thread affect subsequent formatting calls that omit an explicit provider?
16. What is `NumberFormatInfo`, and how does it differ from `CultureInfo`?
17. When reading numeric input from users in different locales, what pitfalls arise with comma vs period decimal separators?
18. What is the purpose of `Console.ForegroundColor`, `BackgroundColor`, and resetting colors after use?

### 04. Operators & Expressions
1. What are the different types of operators in C#? (Arithmetic, Relational, Logical, Bitwise, Assignment, Ternary, Null-coalescing, etc.)
2. Explain the `checked` and `unchecked` keywords in C#.
3. What is the difference between `==` and `.Equals()` for value types vs reference types?
4. What is integer division in C#, and how do you get a fractional result?
5. Explain operator precedence and associativity — why does `a + b * c` evaluate differently than `(a + b) * c`?
6. What is the difference between prefix and postfix increment (`++i` vs `i++`)?
7. What are short-circuit logical operators (`&&`, `||`), and why do they matter beyond boolean logic?
8. Explain the null-conditional operator (`?.`) and null-coalescing operators (`??`, `??=`).
9. What are bitwise operators (`&`, `|`, `^`, `~`, `<<`, `>>`), and when are they used in application code?
10. What is the difference between logical AND (`&&`) and bitwise AND (`&`) when applied to `bool` operands?
11. What is the ternary conditional operator (`?:`), and how does it differ from an `if/else` statement?
12. What is the difference between `is` pattern matching and a simple boolean expression in a condition?
13. When does overflow occur for integer arithmetic, and how do `checked` blocks change behavior?
14. What is the difference between `==` and `ReferenceEquals` for reference types?
15. Can you overload operators in C# — which operators can and cannot be overloaded?
16. What is the difference between compound assignment (`+=`, `-=`) and the expanded form (`x = x + y`) for value vs reference types?
17. What is the `nameof` operator, and how is it used in validation messages and refactoring-safe code?

### 05. Type Conversion & Casting
1. What is the difference between the `is` and `as` operators?
2. What is the difference between implicit and explicit type conversion (casting)?
3. What is the difference between `Convert.ToInt32`, `(int)`, and `int.Parse`?
4. When does a cast succeed at compile time but fail at runtime?
5. What is widening vs narrowing conversion — which direction is implicit?
6. What is the difference between `Parse`, `TryParse`, and `Convert.ChangeType`?
7. When would you use the `is` pattern with a declaration (`if (obj is int n)`) vs a traditional cast?
8. What exception types are commonly thrown by failed casts and parses (`FormatException`, `OverflowException`, `InvalidCastException`)?
9. What is `TryFormat`, and how does writing into a `Span<char>` differ from calling `ToString()`?
10. How does culture affect parsing and formatting during type conversion (e.g., `"1,234.56"` vs `"1.234,56"`)?
11. What is the difference between boxing during conversion to `object` and a direct numeric cast?
12. When is the `as` operator preferred over a cast, and what does it return on failure?
13. What is user-defined explicit/implicit conversion operator syntax (preview level)?
14. What happens when you cast a `double` to `int` — is rounding or truncation applied?
15. What is the difference between `default(T)` casting patterns and `Convert` methods for nullable value types?
16. When converting between `string` and numeric types in APIs and logs, why is `InvariantCulture` often specified explicitly?

### 06. Control Flow & Loops
1. What is the difference between `if/else` and the ternary operator?
2. What is the difference between traditional `switch` and switch expressions (C# 8+)?
3. When should you use `for`, `foreach`, `while`, and `do-while`?
4. What is the difference between `break`, `continue`, and `return` inside a loop?
5. What are common pitfalls with nested loops and loop variable scope?
6. What is a switch expression, and how do relational and property patterns work in `switch`?
7. What is the difference between `break` in a `switch` vs `break` in a loop?
8. When is `goto` still used in C# (e.g., `goto case`, `goto default`), and why is it generally discouraged?
9. What is the scope of a variable declared in the initializer of a `for` loop (C# rules)?
10. Why did C# 5 change loop variable capture semantics in lambdas, and how does that affect `foreach` vs `for`?
11. Can you modify the collection you are iterating in a `foreach` loop — what exception results?
12. What is the difference between `while` and `do-while` when the condition is false on the first check?
13. When would you prefer a `switch` over a chain of `if/else if` statements?
14. What is pattern matching with `is` in an `if` statement vs a `switch` on type?
15. What happens if you use `return` inside a `try` block that has a `finally` — which executes first?

### 07. Methods
1. What are the `out`, `ref`, and `in` parameter modifiers? Explain their usage.
2. What is the `params` keyword in method definitions?
3. What are expression-bodied members in C#?
4. Explain named arguments and optional parameters in C#.
5. What are local functions in C#?
6. What is the `yield` keyword and iterators in C#? *(Cross-ref: Module 03 — IEnumerable)*
7. Explain method overloading — what makes two methods overloads vs duplicate definitions?
8. How does overload resolution work when multiple overloads could apply — what is the "better function member" rule?
9. Why can't you overload methods by return type alone?
10. What is the difference between call-by-value for value types vs reference types at the parameter boundary?
11. When should you use `ref` vs `out` vs `in` for parameters?
12. What problem does the `in` modifier solve for large readonly structs?
13. What is the Try-pattern (`bool TryX(..., out T result)`), and why is it preferred over exceptions for expected failures?
14. Can optional parameters precede required parameters — what are the ordering rules?
15. What is the difference between `params int[]` and passing an explicit `int[]` at the call site?
16. When does overload resolution fail with ambiguity (CS0121), and how do casts or named arguments resolve it?
17. What is the difference between a local function and a private instance method in the same class?
18. What is recursion, what is a base case, and what risk does unbounded recursion pose?
19. Can `out` variables be declared inline at the call site (`TryParse(text, out int n)`)?
20. What is the difference between mutating an object through a reference parameter vs reassigning the parameter variable itself?

### 08. Strings
1. Explain string handling in C# (`string` vs `StringBuilder`).
2. What are the different ways to format strings in C#? (`String.Format`, interpolation, composite formatting)
3. Are strings mutable or immutable in C#? What are the implications?
4. What is string interning?
5. What is the difference between `==`, `Equals`, `Compare`, and `CompareTo` for strings?
6. When should you use `StringComparison.Ordinal` vs `OrdinalIgnoreCase` vs culture-sensitive comparisons?
7. What are verbatim string literals (`@"..."`), and when are they useful?
8. What are raw string literals (`"""..."""`, C# 11+), and how do they handle quotes and newlines?
9. What is the difference between `StringBuilder` and repeated string concatenation in a loop?
10. What is the difference between `string.Concat`, the `+` operator, and interpolation for combining text?
11. What is the difference between `IsNullOrEmpty`, `IsNullOrWhiteSpace`, and checking `Length == 0`?
12. What is the difference between culture-sensitive (`ToUpper()`) and invariant (`ToUpperInvariant()`) case conversion?
13. What methods would you use to split, trim, replace, pad, and search within strings?
14. What is UTF-16 storage in .NET strings, and how does that relate to surrogate pairs and `char`?
15. What is the string intern pool, and what does `string.Intern` do?
16. Why can two strings with identical content fail `ReferenceEquals` while still passing `==`?
17. What is the difference between `Substring` and range/index syntax (`s[start..end]`) for slicing strings?
18. When is `StringBuilder` not the best choice despite many append operations?
19. How does string interpolation handle format specifiers and alignment (`$"{price:C2}"`, `$"{name,-20}"`)?
20. What is the performance implication of calling `Replace` or `Trim` on large strings repeatedly?

### 09. Arrays
1. What are arrays in C#? How is memory managed for single-dimensional, multi-dimensional, and jagged arrays?
2. What is a jagged array?
3. What is the difference between `Array.Copy()`, `Clone()`, and assigning one array variable to another?
4. What is the difference between a single-dimensional array, a rectangular multi-dimensional array (`[,]`), and a jagged array (`[][]`)?
5. Are arrays value types or reference types in C#?
6. What is array covariance for reference types, and why is `object[] arr = new string[3]; arr[0] = 42;` dangerous?
7. What do `Array.Resize`, `Array.Fill`, and `Array.Clear` do — which allocate new memory?
8. What is the difference between `Length` on a single-dimensional array vs `GetLength(dimension)` on multi-dimensional arrays?
9. How do you initialize arrays with collection initializer syntax and `new int[] { 1, 2, 3 }`?
10. What is the relationship between arrays and `params` parameters in methods?
11. What is the difference between shallow copy of an array reference and copying array elements?
12. When would you use `Array.Sort` vs LINQ `OrderBy` on an array?
13. What bounds-checking behavior does C# provide for array indexing?
14. What is `Span<T>`/`ReadOnlySpan<T>` in relation to arrays (preview — stack-friendly views)?
15. How do jagged arrays differ in memory layout from rectangular 2D arrays?
16. What happens when you pass an array to a method — can the callee change the caller's array contents?

### 10. Exception Handling
1. Explain exception handling in C# (`try`, `catch`, `finally`, `throw`, and custom exceptions).
2. What is the difference between `throw` and `throw ex`?
3. Explain the `using` statement in the context of exception handling and resource management.
4. What are exception filters in C#?
5. What is the difference between catching a specific exception type vs `catch (Exception)`?
6. What happens if an exception is thrown inside a `finally` block?
7. What is the base class hierarchy for exceptions in .NET (`Exception`, `SystemException`, application-specific types)?
8. When should you create a custom exception type vs using an existing BCL exception?
9. What is the difference between `using` statement and `using` declaration (`using var`) for disposal?
10. Can you have multiple `catch` blocks — what is the order rule for catching derived vs base exceptions?
11. What is `finally` guaranteed to do, and can it prevent an exception from propagating?
12. What is the difference between handled exceptions and unhandled exceptions in a console vs ASP.NET host?
13. When is it appropriate to catch and swallow an exception vs rethrow?
14. What is `ExceptionDispatchInfo`, and when is `throw;` insufficient?
15. What happens if both `try` and `finally` contain `return` statements?
16. What is the difference between `IDisposable.Dispose` and finalizers in exception-safe cleanup?

### Gotchas — Module 01
1. **String interning** — `string a = "hello"; string b = "hello"; a == b` is `true`, but two separately constructed strings may not be reference-equal even when content matches.
2. **Integer division** — `10 / 3` is `3`, not `3.33`. At least one operand must be floating-point for fractional results.
3. **`const` vs runtime values** — You cannot use `const` with a value that requires computation (e.g., `DateTime.Now`); use `readonly` or a property instead.
4. **Boxing silently hurts performance** — Assigning value types to `object` or non-generic collections causes heap allocations; repeated boxing in hot paths is a common production issue.
5. **Modifying a struct inside `foreach`** — Compile error: the iteration variable is a copy. Use a `for` loop with index or `ref`/`Span` patterns.
6. **`throw;` vs `throw ex;`** — `throw ex;` resets the stack trace; `throw;` preserves the original.
7. **`return` in `try` vs `finally`** — `finally` always runs before the method actually returns; a `return` in `finally` can override the `try` return value.
8. **Array covariance trap** — `object[] arr = new string[3]; arr[0] = 42;` compiles but throws `ArrayTypeMismatchException` at runtime.
9. **Culture-sensitive parse/format** — `"3,14"` parses as 314 in `en-US` but as 3.14 in `de-DE`; logs and APIs should use `InvariantCulture` when format must be fixed.
10. **`Parse` vs `TryParse` in user input paths** — `int.Parse` on bad console input crashes the app; Try-pattern avoids exceptions for expected failure.
11. **`ref` reassignment vs mutation** — Reassigning a reference parameter does not change the caller's variable; mutating the object it points to does.
12. **`params` must be last** — Only one `params` array parameter is allowed, and it must be the final parameter in the signature.
13. **Optional parameter defaults are compile-time** — Changing a default value in a method signature does not update callers compiled against the old default unless recompiled.
14. **`checked` default is context-dependent** — Integer overflow wraps silently in unchecked default contexts; financial code may need explicit `checked` blocks.
15. **Console encoding mismatch** — Writing Unicode to a console whose output encoding is not UTF-8 can display replacement characters or mojibake on Windows.

---

## Module 02. Object Oriented Programming

### 01. Classes & Objects
1. What is a class and what is an object in C#?
2. What is the difference between `struct` and `class` in C#?
3. What are the different principles of OOP supported in C#?
4. What is a partial class in C#?
5. Explain object initializers and collection initializers in C#.
6. What is the difference between shallow copy and deep copy in C#?
7. What is the difference between object identity and object equality?
8. What is the difference between `IDisposable` and a finalizer (`~ClassName()`)?
9. What happens at runtime when you execute `new MyClass()` — allocation, constructor, and reference assignment?
10. Where are class instances stored vs where are struct instances typically stored when local variables?
11. What is the difference between a field, a property, and a method on a class?
12. What is a static class vs an instance class — can you instantiate a static class?
13. What is the `null` reference for reference types, and what is `default` for a struct vs a class?
14. What is object initializer syntax, and how does it interact with constructors?
15. What is the difference between `ReferenceEquals`, `==`, and `Equals` for classes that do not override equality?
16. When is a struct copied vs when is a reference copied when passed to a method?
17. What is the fragile base class problem at a high level?
18. What is the difference between stack allocation (`stackalloc`, local structs) and heap allocation for objects?
19. What does `GC.GetTotalMemory` measure, and why is it only a rough indicator?
20. What is the difference between an anemic class (data-only) and a rich domain object?

### 02. Properties & Indexers
1. Explain properties and fields in C#.
2. What are auto-implemented properties?
3. What are indexers in C#?
4. What is the difference between a `public` field and a `public` auto-property — if they behave similarly, why prefer properties?
5. What are init-only properties (`get; init;`), and how do they differ from get-only and `{ get; set; }`?
6. What is the difference between `{ get; private set; }` and a property with only a public getter backed by a private setter method?
7. What are expression-bodied properties (`public string Label => $"{Title}";`)?
8. Can indexers be overloaded — what distinguishes overloads?
9. What is the syntax for an indexer (`this[int index]`, `this[string key]`)?
10. When should you use a full property with validation vs an auto-property?
11. What is a computed/read-only property that derives its value from other members?
12. What is the difference between `init` properties and constructor parameters for immutable objects?
13. How do properties participate in object initializer syntax?
14. What is a preview-level understanding of `record` types and synthesized properties?
15. Why might exposing a public `{ get; set; }` on a collection-typed property break encapsulation?
16. What is the difference between an indexer and a method named `GetByIndex`?
17. Can interface types declare indexers, and how are they implemented?
18. What is the relationship between properties and data binding / serialization frameworks?

### 03. Constructors & Method Overloading
1. Explain constructors and their types in C# (default, parameterized, static, private).
2. What is a destructor/finalizer in C#?
3. Explain constructor chaining in C# (`: this(...)` vs `: base(...)`).
4. How can you call the base class constructor from a derived class?
5. In what order do constructors and field initializers run in an inheritance chain?
6. Explain method overloading and method overriding in C#.
7. What is a static constructor, and when does it run?
8. Can a struct have a parameterless constructor (C# 10+ rules vs earlier)?
9. What is the difference between a primary constructor (C# 12 on classes/records) and traditional constructors?
10. What happens if you do not define any constructor — what default constructor is provided?
11. Why might you mark a constructor `private` (singleton, factory patterns)?
12. What is constructor overloading, and how does `: this(...)` reduce duplication?
13. What is the exact order: static constructor, instance field initializers, instance constructor body, base constructor?
14. What is the difference between method overloading (compile-time) and method overriding (runtime polymorphism)?
15. When does the compiler fail to pick an overload due to ambiguity involving optional parameters and `params`?
16. Can constructors be inherited — how does a derived class get a base constructor?
17. What validation belongs in a constructor vs a factory method?
18. What is the difference between calling an overloaded instance method vs a static overloaded method?

### 04. Static Members & Static Classes
1. Explain the `static` keyword in detail.
2. What is a static class in C#?
3. Why can you not override a `static` method?
4. What is the difference between a static class and the singleton pattern?
5. What is a static field, and how is lifetime different from an instance field?
6. What is a static property and static method — what is the `this` reference inside them?
7. Why can static methods not access instance members directly?
8. When are static constructors executed, and how many times per AppDomain/process?
9. What is the difference between `const` (implicitly static) and `static readonly`?
10. Can a static class implement interfaces?
11. What thread-safety concerns apply to mutable static fields?
12. Why is overusing static state a testing and maintainability problem?
13. What is the difference between static nested classes and non-static nested classes?
14. How do static members participate in inheritance — are they polymorphic?

### 05. Inheritance & Polymorphism
1. Explain inheritance in detail in C#.
2. Explain polymorphism in C# and how it can be achieved.
3. What is the difference between compile-time (static) and runtime (dynamic) polymorphism?
4. What is a sealed class in C#?
5. What is a virtual method in C#?
6. What is the difference between `this` and `base` keywords?
7. What is operator overloading in C#?
8. Explain the difference between `virtual`, `abstract`, and `override` keywords.
9. Explain the `new` keyword in the context of method hiding.
10. Explain how C# handles multiple inheritance (using interfaces).
11. Why does C# not support multiple inheritance of classes?
12. What is the fragile base class problem?
13. Why is "favor composition over inheritance" a common guideline?
14. What is runtime dispatch — how does the CLR resolve `override` calls through a base reference?
15. What is the difference between hiding with `new` and overriding with `override` when calling through a base-typed variable?
16. Can you inherit from a sealed class?
17. What is the difference between `is` type testing and casting in polymorphic code paths?
18. What is the Liskov Substitution Principle in one sentence, and how does it relate to inheritance?
19. When does `base.Method()` call the parent's implementation vs the current type's override?
20. What is the difference between extending behavior with inheritance vs wrapping with composition?

### 06. Abstract Classes & Interfaces
1. Explain abstraction in detail in C#.
2. What is the difference between abstraction and encapsulation?
3. What is the difference between abstraction and polymorphism?
4. What is the difference between an abstract class and an interface?
5. What is the difference between an abstract class and an interface before C# 8 vs after (default interface methods)?
6. Why do we need interfaces in C#?
7. What is explicit interface implementation and when is it used?
8. What are static abstract members in interfaces (C# 11)?
9. Can an abstract class have concrete (non-abstract) methods?
10. Can a class implement multiple interfaces — what about an interface inheriting another interface?
11. When would you choose an abstract base class over an interface for shared implementation?
12. What is the diamond problem, and how does C# avoid it for classes but address it for interfaces with default methods?
13. What is explicit interface implementation — why might `((IMyInterface)obj).Method()` work when `obj.Method()` does not?
14. Can interfaces declare fields, constructors, or static concrete state (pre- and post-C# 8)?
15. What is the difference between `IReadOnlyList<T>` as a parameter type and `List<T>` for abstraction?
16. When should API surface depend on interfaces vs abstract classes?

### 07. Encapsulation & Access Modifiers
1. Explain encapsulation in C# with examples.
2. What are the different access modifiers in C#? (`private`, `protected`, `internal`, `protected internal`, `private protected`)
3. What is the difference between "information hiding" and "data hiding"?
4. Why is exposing a mutable collection through a public getter an encapsulation break?
5. What is the difference between `protected internal` and `private protected`?
6. What does `internal` mean in the context of assemblies and `InternalsVisibleTo`?
7. What is the default access level for class members if you omit an modifier?
8. How do access modifiers apply to nested types vs top-level types?
9. What is defensive copying when returning collections from properties?
10. What is the difference between encapsulation and immutability?
11. Why are public fields discouraged in public APIs even for simple DTOs in some codebases?
12. How does `private protected` restrict visibility compared to `protected` alone?
13. What is a friend assembly pattern, and what are its trade-offs?
14. How do property accessors use asymmetric access (`public get; private set;`)?

### 08. Events
1. Explain events in C# (including event handling and publisher-subscriber pattern).
2. What is the difference between an `event` and a plain public delegate field?
3. Why should you unsubscribe from events, and what problem does this prevent?
4. What happens during multicast delegate invocation if one subscriber throws?
5. What is the standard `EventHandler` / `EventHandler<TEventArgs>` pattern?
6. How do you raise an event safely (null-check, `?.Invoke`, local copy pattern)?
7. What is the difference between custom delegate types and `EventHandler` for events?
8. Can interfaces declare events, and how are they implemented?
9. What memory-leak scenario arises when a long-lived publisher holds references to short-lived subscribers?
10. What is the difference between events and the Observer pattern / IObservable?
11. Can you assign to an event from outside the declaring class (`event += handler` vs `event = handler`)?
12. What is thread-safe event raising, and when is locking required?

### 09. OOP Real-World Examples
1. Explain the SOLID principles with concrete C# examples.
2. What is the Liskov Substitution Principle? Give a classic violation (e.g., `Square`/`Rectangle`).
3. What is Dependency Inversion, and how does constructor injection implement it?
4. What is the difference between Dependency Injection and the Service Locator pattern?
5. What is the difference between "has-a" and "is-a" relationships? When is inheritance the wrong choice?
6. What is the anemic domain model anti-pattern?
7. What is the Open/Closed Principle, and how do interfaces support extension without modification?
8. What is the Single Responsibility Principle — how do you recognize a class that violates it?
9. What is the Interface Segregation Principle — why are fat interfaces problematic?
10. What is a factory method vs a simple constructor — when do you introduce a factory?
11. What is the Strategy pattern, and how does it map to interfaces/delegates in C#?
12. What is the Repository pattern at a high level, and why depend on abstractions?
13. How does polymorphism simplify replacing implementations in tests (mock/stub scenarios)?
14. What is the difference between domain modeling with rich behavior vs CRUD-style service objects?

### Gotchas — Module 02
1. **Virtual method from base constructor** — Calling an overridden virtual method from a base constructor runs before derived field initializers complete; overridden code sees default values.
2. **Method hiding vs overriding** — `new` hides by compile-time type; `override` dispatches by runtime type. Mixing them breaks expected polymorphism.
3. **`Equals()` without `GetHashCode()`** — Breaks the hash contract; objects can exist in a `Dictionary`/`HashSet` but not be found again after mutation.
4. **Mutable object as dictionary key** — Changing a key after insertion causes "lost" entries at runtime.
5. **Struct boxing via interface** — Assigning a struct to an interface type boxes; subsequent struct mutations don't affect the boxed copy.
6. **`protected internal` vs `private protected`** — `protected internal` = protected OR internal; `private protected` = protected AND internal (same assembly only).
7. **Type-checking anti-pattern** — Long `if (animal is Dog)` chains defeat polymorphism; prefer virtual methods or pattern matching on a common abstraction.
8. **Memory leaks despite GC** — Event handlers and static caches holding references to short-lived objects are the classic managed leak.
9. **Exposing `List<T>` directly** — Callers can mutate internal state without invariant checks; return `IReadOnlyList<T>` or defensive copies.
10. **`init` after construction** — Init-only properties can be set in object initializers and constructors but not arbitrary code afterward; confusing with `{ get; private set; }`.
11. **Static "singleton" vs DI singleton** — A static class is hard to test and replace; instance singletons registered in DI are still mockable if designed carefully.
12. **Explicit interface hiding** — Public class method and explicit interface method can coexist with different behavior; callers must know which API they use.
13. **Finalizer timing** — `~ClassName()` runs non-deterministically; do not rely on it for timely resource release — use `Dispose`.
14. **Overriding `==` without consistent `Equals`/`GetHashCode`** — Custom equality operators that disagree with `Equals` break collections and LINQ.
15. **Default interface methods on structs** — Calling a default interface method on a struct may box the struct depending on how it is invoked.

---

## Module 03. Generics & Collections

### 01. Generics
1. What are generics in C#? Why were they introduced?
2. What is the difference between generic and non-generic collections?
3. Explain generic constraints in C# (`where` clause) with examples.
4. Explain covariance and contravariance in generics (`in` and `out` keywords).
5. Can you use `where T : Enum` or `where T : unmanaged`? What problems do these solve?
6. What happens when you use `default(T)` on an unconstrained type parameter?
7. Why can't you write `T value = null;` unless `T` is constrained to `class`?
8. What is the difference between a generic class and a generic method?
9. What's the difference between reflection over an open generic type (`List<>`) and a closed generic type (`List<int>`)?
10. Why does `typeof(List<int>) == typeof(List<string>)` return `false`, and how do you get the shared generic type definition?
11. What is type erasure vs reification — does C# retain generic type information at runtime?
12. What constraints allow calling `new T()` — what does `where T : new()` enable?
13. What is the difference between `where T : class` and `where T : struct` constraints?
14. What does `where T : notnull` mean for nullable reference type analysis?
15. Why are generic value types separate closed types at runtime for static fields?
16. What is covariance on `IEnumerable<out T>` — why can you assign `IEnumerable<string>` to `IEnumerable<object>`?
17. What is contravariance on `Action<in T>` / `IComparer<in T>`?
18. Why is `List<T>` neither covariant nor contravariant on `T`?
19. What is the difference between generic specialization performance for value types vs reference types?
20. Can you cast from `List<string>` to `List<object>` — what error or exception occurs?

### 02. ArrayList
1. What is the difference between `Array` and `ArrayList`?
2. What is the difference between `List<T>` and `ArrayList`?
3. Why is `ArrayList` considered a legacy collection in modern C#?
4. What boxing occurs when storing `int` values in an `ArrayList`?
5. What is the performance cost of repeated boxing/unboxing in hot loops using `ArrayList`?
6. Can you store mixed types in an `ArrayList`, and what typing risks does that create?
7. What is the difference between `ArrayList.Capacity` and `Count`?
8. When might you still encounter `ArrayList` in maintained legacy codebases?
9. What is the difference between `ArrayList` and `object[]`?
10. What legacy non-generic collections (`Hashtable`, `Queue`, `Stack`) should you know for maintenance scenarios?

### 03. List
1. Explain the internal working and performance of `List<T>` vs `LinkedList<T>`.
2. What is `LinkedList<T>` and when should it be used?
3. What is the difference between `List<T>.Sort()` stability and `OrderBy()` stability?
4. How does `List<T>` grow its internal buffer when capacity is exceeded?
5. What is the amortized cost of `Add` on `List<T>` vs `Insert` at the beginning or middle?
6. What does `List<T>.AsReadOnly()` return, and can callers still mutate the underlying list?
7. What is the difference between `ConvertAll`, `ForEach`, and LINQ `Select` on a list?
8. What do `ToArray`, `CopyTo`, and `GetRange` do — which allocate new arrays?
9. When would you expose `List<T>` as a return type vs `IReadOnlyList<T>` or `IEnumerable<T>`?
10. What is the difference between `List<T>.Capacity` and `Count`?
11. What happens if you mutate a list while iterating with `foreach`?
12. What is `TrimExcess`, and when is it useful?
13. What is binary search on a list (`BinarySearch`) — what precondition must the list satisfy?
14. How does `List<T>` indexer access compare to `LinkedList<T>` (no indexer)?
15. What is `Comparison<T>` delegate, and how does it relate to `List<T>.Sort`?

### 04. Dictionary
1. What is the difference between `Dictionary<TKey, TValue>` and `Hashtable`?
2. Explain `IDictionary<TKey, TValue>` and `IReadOnlyDictionary<TKey, TValue>`.
3. How does `Dictionary<TKey, TValue>` handle hashing and collisions?
4. What is the difference between `Dictionary.Add` and the indexer when the key already exists?
5. What is the difference between `ContainsKey`, `TryGetValue`, and the indexer for lookup?
6. Why must keys be immutable (or stable) after insertion for correct hash table behavior?
7. What exception is thrown when accessing a missing key via the indexer?
8. Can `null` be used as a key when `TKey` is a reference type?
9. What is the average vs worst-case time complexity for lookup, insert, and remove?
10. What is the hash code contract between `GetHashCode` and `Equals` for custom key types?
11. What is `IEqualityComparer<TKey>`, and when do you pass a custom comparer to the constructor?
12. When would you choose `Dictionary` over `List` for lookups by id or SKU?
13. What happens internally when two keys hash to the same bucket?

### 05. HashSet
1. Explain `HashSet<T>` and its use cases. How is it different from `List<T>`?
2. What is the difference between `SortedSet<T>` and `HashSet<T>`?
3. Why does `HashSet<T>` require correct `GetHashCode()`/`Equals()` for custom types?
4. What set operations does `HashSet<T>` provide (`UnionWith`, `IntersectWith`, `ExceptWith`, `SymmetricExceptWith`)?
5. What is the difference between `Add` returning `false` on duplicate vs `List.Add` behavior?
6. How do you construct a `HashSet<T>` with custom equality (`IEqualityComparer<T>`)?
7. When would you use `HashSet<T>` for deduplication vs `Distinct()` in LINQ?
8. What is the difference between set membership test in `HashSet` vs scanning a `List`?
9. What is `IsSubsetOf`, `IsSupersetOf`, and `Overlaps` used for?
10. Can you modify an element in a `HashSet` in place if it affects equality — what goes wrong?

### 06. Queue and Stack
1. Explain `Queue<T>` and `Stack<T>` vs their non-generic counterparts.
2. What is FIFO vs LIFO, and which collection maps to each?
3. What operations does `Queue<T>` expose (`Enqueue`, `Dequeue`, `Peek`, `TryDequeue`, `TryPeek`)?
4. What operations does `Stack<T>` expose (`Push`, `Pop`, `Peek`, `TryPop`)?
5. Why do `Queue` and `Stack` not support random access by index?
6. How is `Queue<T>` used in breadth-first search (BFS) on a graph or grid?
7. Why does BFS find shortest paths in unweighted graphs?
8. What real-world workflows map naturally to a stack (undo/redo, call stack, DFS)?
9. What is the difference between non-generic `Queue`/`Stack` and generic versions regarding boxing?
10. When would you use `Queue<T>` over `List<T>` with remove-from-front patterns?

### 07. SortedList & SortedDictionary
1. What is `SortedList<TKey, TValue>` and `SortedDictionary<TKey, TValue>`? When would you use each?
2. What interface defines ordering for sorted collections (`IComparer<TKey>` vs `IEqualityComparer<TKey>`)?
3. What is the difference between `SortedList` (array-backed) and `SortedDictionary` (tree-backed) performance?
4. When is `SortedList` preferred over `SortedDictionary` for memory or indexed access?
5. What is the cost of inserting out-of-order keys into a sorted collection?
6. Can you look up by index in `SortedList` — what does `Keys[index]` provide?
7. What is the difference between `SortedSet<T>` and `SortedDictionary<TKey, TValue>`?
8. When would you choose `SortedDictionary` over sorting keys from a `Dictionary` at read time?

### 08. IEnumerable & IEnumerator
1. What is the difference between `IEnumerable<T>` and `ICollection<T>`?
2. What is the difference between `ICollection<T>` and `IList<T>`?
3. What are `IReadOnlyList<T>` and `IReadOnlyCollection<T>`?
4. What is the difference between `IEnumerator` and `IEnumerator<T>`?
5. What is the `yield` keyword, and how do iterator methods relate to `IEnumerable<T>`?
6. What is the difference between deferred execution and immediate execution for IEnumerable sequences?
7. What is the iterator pattern — what do `MoveNext`, `Current`, and `Reset` do?
8. What is `yield break` vs `return` in an iterator method?
9. Why can multiple enumeration of the same `IEnumerable` from a LINQ query re-run the pipeline?
10. What is the difference between returning `IEnumerable<T>` from a method vs `List<T>`?
11. What happens if you modify a collection during `foreach` — how does the enumerator detect it?
12. What is covariance on `IEnumerable<out T>` — practical assignment examples?
13. What is the difference between `foreach` and manual `while (enumerator.MoveNext())`?
14. What is `ToList()` materialization, and when must you materialize before multiple passes?
15. What is the relationship between `IAsyncEnumerable<T>` and iterators (preview)?

### Gotchas — Module 03
1. **Modify while iterating** — Changing a collection during `foreach` throws `InvalidOperationException`.
2. **Mutable keys** — Changing equality-relevant state on a key after insertion causes silent lookup failures.
3. **`IEnumerable<T>` covariant, `List<T>` not** — Covariance on mutable lists would break type safety.
4. **Wrong collection for the job** — Frequent middle inserts on `List<T>` are O(n).
5. **Static fields on generic types** — Separate static slots per closed generic type.
6. **Boxing in non-generic collections** — `ArrayList` boxes value types; `List<T>` avoids this.
7. **Passing `List<T>` by value** — Reference is copied; contents still shared.
8. **`Dictionary.Add` vs indexer on duplicate key** — `Add` throws; indexer overwrites silently.
9. **`AsReadOnly()` is a view** — Original list mutations remain visible through the wrapper.
10. **Assuming dictionary enumeration order** — Undefined; sort keys explicitly if order matters.
11. **Multiple enumeration cost** — `yield`/LINQ re-executes work each pass; materialize when needed.
12. **Wrong comparer on sorted types** — `SortedDictionary` uses `IComparer<TKey>`, not `IEqualityComparer<TKey>`.
13. **Poor `GetHashCode` distribution** — Constant hash codes degrade to O(n) buckets.
14. **Queue `Contains` is O(n)** — Use a `HashSet` alongside if you need fast membership checks.

---

## Module 04. Functional Style Programming

### 01. Delegates
1. What is Functional Programming, and how does C# support it without being a purely functional language?
2. What are the key principles of Functional Programming (immutability, pure functions, first-class functions, higher-order functions, referential transparency)?
3. What is the difference between imperative and declarative programming styles? Give a C# example of each.
4. What does it mean for functions to be first-class citizens in C#?
5. What is a delegate in C#? How does it differ from a method group and from an interface with a single method?
6. How do you declare, instantiate, and invoke a custom delegate type?
7. What is a multicast delegate? How does `+=` and `-=` work on delegate instances?
8. What is the difference between single-cast and multicast delegates at invocation time?
9. What happens when you invoke a multicast delegate and one subscriber throws an exception?
10. What is delegate covariance and contravariance in C#?
11. When would you prefer a named delegate type over `Func`/`Action` in a public API?
12. What are the advantages and limitations of adopting a functional style in typical enterprise C# codebases?

### 02. Lambda Expressions
1. What is a lambda expression in C#? What problem does it solve compared to named methods?
2. What is the difference between an expression lambda and a statement lambda?
3. When can parameter types be omitted in a lambda, and when must they be explicit?
4. What are target-typed lambdas (C# 10+)? In what contexts does the compiler infer the delegate type?
5. What is the natural type of a lambda — when does the compiler infer `Func`/`Action` vs require an explicit target type?
6. How do lambda expressions differ from anonymous methods in syntax, capabilities, and compiler output?
7. Can a lambda expression access `ref`, `out`, or `in` parameters from the enclosing method?
8. Can a lambda be converted to an expression tree? What syntax or API constraints apply?
9. What is the difference between a lambda that captures no locals vs one that captures outer variables?
10. How do async lambdas work (`async x => ...`)? What delegate types can they target?
11. What happens if you use a lambda where a `Expression<TDelegate>` is expected vs where a `TDelegate` is expected?

### 03. Anonymous Methods
1. What are anonymous methods in C#? Why were they introduced, and what largely replaced them?
2. What is the syntax for an anonymous method, and how does it compare to lambda syntax?
3. Can anonymous methods omit parameter lists? When is that useful?
4. What outer scope variables can anonymous methods access, and how does capture work?
5. In modern C# code, when (if ever) would you still choose an anonymous method over a lambda?

### 04. Extension Methods
1. What are extension methods in C#? How do they appear to the caller vs how they are implemented?
2. What are the language rules for declaring an extension method (static class, `this` parameter, accessibility)?
3. How does the compiler resolve an extension method call at compile time?
4. What is the difference in resolution order between an instance method and an extension method with the same signature?
5. Can extension methods access `private` members of the extended type? Why or why not?
6. What are the limitations of extension methods?
7. How do extension methods work on interfaces? What are design implications (e.g., LINQ)?
8. What happens when two namespaces define extensions with the same name and signature for the same type?
9. Can you define generic extension methods? How does type inference work at the call site?
10. What are anti-patterns with extension methods (god extensions, violating encapsulation)?

### 05. Func, Action & Predicate
1. What are `Func<T>`, `Func<T, TResult>`, and the general `Func<...>` family?
2. What is `Action` vs `Action<T>` vs `Action<T1, T2, ...>`?
3. What is `Predicate<T>`, and how does it relate to `Func<T, bool>`?
4. When should you use `Func` vs `Action` vs `Predicate` vs a custom delegate?
5. What are higher-order functions? Give C# examples using `Func` and `Action`.
6. What is function composition, and how can it be achieved in C#?
7. How many generic parameters do `Func` and `Action` support, and which parameter is always the return type for `Func`?
8. How are `Func` and `Action` used in LINQ method parameters (`Select`, `Where`, etc.)?
9. When does using `Func<T, bool>` instead of `Predicate<T>` improve or hurt API clarity?

### 06. Closures
1. What is a closure in C#?
2. How does the compiler implement variable capture for lambdas and anonymous methods?
3. What is the difference between capturing a variable vs capturing a value at closure creation time?
4. What is the classic `for` loop closure bug, and how did C# 5 change loop variable capture semantics?
5. How does the same capture bug appear in `foreach`, LINQ, and `Task.Run` callbacks?
6. What problems arise when multiple closures share the same captured variable?
7. What is a display class (compiler-generated closure type), and what performance cost does capture introduce?
8. What is a pure function? Give an example in C# and explain what makes it pure.
9. What is immutability, and why is it important in functional and concurrent programming?
10. How can immutability be achieved in C# (`readonly`, `record`, avoiding mutable captures)?
11. How do you avoid side effects when passing lambdas to APIs that store or invoke them later?
12. When should you copy loop values to a local inside the loop before capturing (`var copy = item`)?
13. How do local functions compare to lambdas regarding capture and allocation behavior?

### Gotchas — Module 04
1. **Closure captures the variable, not the value** — Loop lambda prints `3, 3, 3`, not `0, 1, 2`.
2. **Same trap in LINQ and tasks** — Capturing loop variables inside `.Where()` / `Task.Run` produces identical bugs.
3. **Multicast delegate short-circuit on exception** — Later subscribers may not run if an early one throws.
4. **Extension method not in scope** — Missing `using` for the static class namespace.
5. **Instance method wins over extension** — An instance method hides the extension; you cannot "override" with an extension.
6. **Shared captured storage** — Multiple lambdas share one slot for the same outer variable.
7. **Target-typed lambda ambiguity** — Without a clear target type, lambda expressions may fail to compile.
8. **Expression tree vs delegate** — Expression-tree lambdas cannot contain many C# constructs that delegate lambdas allow.
9. **Capturing `this` implicitly** — Instance lambdas capture `this`, extending object lifetime.
10. **Extension on null reference** — Extension methods can be called on null receivers; may throw inside the method.

---

## Module 05. Language Integrated Query

### 01. Introduction to LINQ
1. What is LINQ, and what problem does it unify across in-memory collections, databases, XML, and more?
2. What is the difference between query syntax and method syntax? Are they equivalent?
3. What is deferred execution in LINQ, and which operators break it?
4. What is the difference between deferred execution and lazy evaluation?
5. What is the difference between `IEnumerable<T>` and `IQueryable<T>`?
6. What is an expression tree, and why does `IQueryable` depend on it?
7. What is the difference between LINQ to Objects and LINQ to Entities (EF Core)?
8. What is the role of the `Enumerable` vs `Queryable` static classes?
9. What does it mean for a LINQ provider to translate a query? What happens when translation fails?
10. How do you inspect or debug the SQL generated by an `IQueryable` provider?
11. What is the difference between chaining LINQ operators vs building queries incrementally with `if` conditions?
12. What are common signs that LINQ is hurting readability, and how do you refactor without losing composability?
13. How does LINQ interact with nullable reference types and null propagation in projections?
14. What is query rewriting (e.g., `let`, `join`, `group` in query syntax) at a high level?

### 02. Filtering & Aggregation
1. What is the difference between `.Where()` and `.Select()` in intent and output shape?
2. When should you filter before projecting vs project before filtering?
3. What is the difference between `.Count()`, `.LongCount()`, `.Sum()`, `.Average()`, `.Min()`, and `.Max()`?
4. What happens when `.Average()` or `.Sum()` is called on an empty sequence?
5. What is `.Aggregate()`, and how does it generalize other aggregations?
6. How do seed and accumulator overloads of `.Aggregate()` work? Give use cases (running totals, building strings, merging objects).
7. What is the difference between `.Aggregate()` with and without a result selector?
8. What is `.Count(predicate)` vs `.Where().Count()` in terms of readability and performance?
9. What is the difference between `.Any()` and `.Count() > 0` for `IEnumerable<T>` vs `ICollection<T>`?
10. How do nullable numeric aggregations behave in LINQ to Objects?
11. What is the difference between `.Sum()` on `int` vs `long` vs `decimal` regarding overflow?
12. How do aggregations translate (or fail to translate) in LINQ to Entities?
13. When would you use `.Aggregate()` instead of a simple loop for custom accumulation logic?
14. What are pitfalls of aggregating floating-point values from large sequences?

### 03. Ordering
1. What is the difference between `OrderBy().ThenBy()` and calling `OrderBy()` twice?
2. What is stable sort in LINQ to Objects, and why does it matter?
3. Does `OrderBy` guarantee stability across all .NET versions and providers?
4. What is the difference between `OrderBy` and `OrderByDescending` when keys compare equal?
5. How do `ThenBy` and `ThenByDescending` chain comparers?
6. Can you sort by multiple keys using query syntax? How?
7. What is the difference between sorting before `GroupBy` vs sorting groups with `OrderBy` on the outer sequence?
8. How does `IOrderedEnumerable<T>` differ from `IEnumerable<T>`?
9. When should you pass a custom `IComparer<T>` to `OrderBy`?
10. How does ordering translate to SQL in EF Core (`ORDER BY`, composite keys)?
11. What is the performance characteristic of `OrderBy` in LINQ to Objects?
12. What happens if the key selector throws for some elements during ordering?
13. How do `OrderBy` and `Reverse()` interact — does `Reverse` undo sort stability semantics?
14. When is in-memory sorting the wrong choice for large EF Core queries?

### 04. Grouping
1. Explain `GroupBy` in LINQ to Objects — what is the shape of the result?
2. What is the difference between `GroupBy(keySelector)` and `GroupBy(keySelector, elementSelector)`?
3. What is the difference between `GroupBy` with a result selector vs post-processing grouped sequences?
4. How does `GroupBy` differ when translated to SQL (LINQ to Entities) vs in-memory?
5. What is the difference between `GroupBy` and `ToLookup()`?
6. When should you use `ToLookup()` instead of `GroupBy().ToDictionary()`?
7. What is `ILookup<TKey, TElement>` and how is it immutable compared to `Dictionary` of lists?
8. How do you group by composite keys (anonymous types, value tuples, custom key types)?
9. How does key equality affect grouping (`Equals`/`GetHashCode`, reference vs value semantics)?
10. What is the difference between `group by` in query syntax and method syntax?
11. How do you flatten or regroup nested groups efficiently?
12. What are common mistakes grouping large EF Core queries (client evaluation, pulling too much data)?
13. How does `GroupBy` interact with ordering of elements within each group?
14. When would you prefer `Dictionary` manual grouping over `GroupBy` for performance?

### 05. Joins
1. What is the difference between inner join, left join, and cross join in LINQ?
2. How do you express a left outer join in method syntax vs query syntax?
3. What is the difference between `join` and `GroupJoin`?
4. When should you use `GroupJoin` followed by `SelectMany` vs a direct `join`?
5. How do you join on composite keys using anonymous types or tuples?
6. What are equality requirements for join keys?
7. What is the difference between equijoin and non-equijoin — can LINQ express non-equijoins cleanly?
8. How do joins translate to SQL in EF Core (`INNER JOIN`, `LEFT JOIN`)?
9. What is a many-to-many join pattern in LINQ?
10. What performance pitfalls appear when joining large in-memory sequences vs database-side joins?
11. How does `DefaultIfEmpty()` enable left outer join semantics?
12. What is the difference between a join and a correlated subquery in LINQ query syntax?
13. When is `Join` preferable to building a `Dictionary` lookup manually?
14. What happens when duplicate keys exist on the inner or outer sequence?

### 06. Element Operations
1. What is the difference between `.First()`, `.FirstOrDefault()`, `.Single()`, and `.SingleOrDefault()` — when does each throw?
2. What is the difference between `.Last()` and `.LastOrDefault()` on deferred vs indexed sequences?
3. What is the difference between `.ElementAt(index)` and indexing (`list[index]`)?
4. What is `.ElementAtOrDefault()` behavior for out-of-range indexes?
5. How do element operations behave on empty sequences for each overload?
6. What is the difference between `.First(predicate)` vs `.Where(predicate).First()`?
7. Why can `.Single()` be dangerous on filtered EF Core queries?
8. What is the time complexity of `.ElementAt()` on a linked list vs `IList<T>`?
9. When should you use `.FirstOrDefault()` vs `.SingleOrDefault()` defensively in APIs?
10. How do element operators short-circuit enumeration?
11. What exceptions are thrown vs null/default returned for reference and value types?
12. How do `.MinBy()` / `.MaxBy()` (modern LINQ) relate to element operations conceptually?

### 07. Set Operations
1. What is the difference between `.Distinct()`, `.Union()`, `.Intersect()`, and `.Except()`?
2. How does equality comparer selection work for set operations?
3. What is `.DistinctBy()` (modern LINQ), and how does it differ from `.GroupBy().Select(g => g.First())`?
4. Are `Union`/`Intersect`/`Except` multisets or sets — how are duplicate inputs handled?
5. What is the difference between `.Union()` and `.Concat().Distinct()`?
6. How do set operations translate in EF Core?
7. What is the performance of set operations on sorted vs unsorted inputs?
8. When would you use `HashSet<T>` manually instead of LINQ set operators?
9. How do reference equality and value equality change set operation results?
10. What is the difference between set operations on in-memory sequences vs `IQueryable`?

### 08. Projection Operations
1. What is the difference between `.Select()` and `.SelectMany()`?
2. When should you use `.SelectMany()` for one-to-many relationships?
3. What is projection to anonymous types vs named DTOs — trade-offs for maintenance and testing?
4. How do you project into nested shapes or hierarchical DTOs?
5. What is the difference between `.Select()` before vs after `.Where()` for EF Core translation?
6. How does `.Select()` interact with nullable reference types?
7. What is a selector that returns `IEnumerable<T>` vs flattened `SelectMany`?
8. How do you project with index using `.Select((item, index) => ...)`?
9. What are common causes of N+1 queries related to projection in EF Core?
10. How does `let` in query syntax relate to projection and intermediate variables?
11. When does projection cause full entity materialization vs column slicing in SQL?
12. What is the difference between projecting computed values vs mapping existing properties only?
13. What is `.Zip()`, and how do you combine two sequences element-by-element (including unequal lengths)?

### 09. Quantifier Operations
1. What do `.All()`, `.Any()`, and `.Contains()` do, and when would you use each?
2. What is the difference between `.Any(predicate)` and `.Where(predicate).Any()`?
3. How does `.All()` behave on an empty sequence (vacuous truth)?
4. What is the difference between `.Contains(item)` and `.Any(x => x.Equals(item))` with custom equality?
5. How do quantifiers short-circuit enumeration?
6. How do quantifiers translate to SQL (`EXISTS`, `IN`, `ALL`) in EF Core?
7. When is `.Contains` with a large in-memory list a performance problem for EF Core?
8. What is the difference between `.Any()` on `IQueryable` vs materialized collections?
9. How do quantifiers interact with null keys or null elements in sequences?
10. When should you prefer `.All()` vs validating with `.Count()` or exceptions?
11. What is `.SequenceEqual()`, and how does it compare sequences with optional `IEqualityComparer<T>`?

### 10. Conversion Operations
1. When should you use `.ToList()`, `.ToArray()`, `.ToDictionary()`, `.ToHashSet()`, and `.AsEnumerable()`?
2. What is `.ToLookup()` and when is it preferable to `.GroupBy().ToDictionary()`?
3. What are duplicate-key behaviors for `.ToDictionary()` vs `.ToLookup()`?
4. Why can calling `.ToList()` too early in an EF Core query hurt performance?
5. What is the difference between `.AsEnumerable()` and `.ToList()` for switching from `IQueryable` to LINQ to Objects?
6. What is `.Cast<T>()` vs `.OfType<T>()` — when does each throw vs filter?
7. What is `.AsQueryable()` on an in-memory sequence — what provider backs it?
8. How do `.ToArray()` and `.ToList()` differ for subsequent mutations and memory?
9. When should you use `.ToImmutableArray()` / `.ToImmutableList()` from System.Collections.Immutable?
10. What is the cost of multiple conversions in a hot path?
11. How does `.ToDictionary()` handle null keys?
12. When is explicit materialization required before passing sequences across async boundaries?

### 11. Partitioning Operations
1. What is the difference between `.Take()`, `.Skip()`, `.TakeWhile()`, and `.SkipWhile()`?
2. How do `.Take`/`Skip` translate to SQL paging in EF Core?
3. What is `.Chunk()` (modern LINQ), and how does it differ from manual batching loops?
4. What is the difference between `.TakeWhile`/`SkipWhile` vs `.Where` for conditional paging?
5. What are pitfalls of using `.Skip(n).Take(m)` without stable ordering in databases?
6. How does keyset (seek) pagination compare to offset pagination in LINQ/EF?
7. What happens when `Skip`/`Take` arguments are negative or larger than the sequence?
8. When does partitioning force full enumeration vs true streaming?
9. How do partitioning operators interact with deferred execution?
10. How would you batch-process a large `IEnumerable<T>` using `.Chunk()` for database updates?
11. What is `.TakeLast()` / `.SkipLast()`, and how do they differ from reversing then taking?

### 12. Generation Operations
1. What are `Enumerable.Range`, `Repeat`, and `Empty` used for?
2. What is the difference between `Enumerable.Repeat` and repeating elements in a collection?
3. How do generation methods behave with deferred execution?
4. When is `Enumerable.Empty<T>()` preferable to `Array.Empty<T>()` or `new List<T>()`?
5. How do you generate sequences lazily without preallocating large arrays?
6. What are pitfalls of `Range` with large counts (memory, overflow)?
7. How do generation operators combine with `.Select` to produce synthetic keys or indexes?
8. When would you use `yield return` in custom iterators vs `Enumerable.Range`?
9. How do infinite or unbounded sequences interact with operators like `.Count()` or `.Take()`?
10. What is the difference between generating sequences in LINQ vs using `Random` or GUID factories in projections?

### 13. LINQ to XML
1. What is the difference between LINQ to XML (`XDocument`, `XElement`) and XML serialization (`XmlSerializer`, `DataContractSerializer`)?
2. How do you load, create, and mutate XML with `XDocument` and `XElement`?
3. How do you query XML with LINQ (`Descendants`, `Elements`, `Attributes`, `XPath` extensions)?
4. What is the difference between `Elements()` and `Descendants()`?
5. How do you project XML into CLR objects manually vs using deserialization?
6. What is the difference between `XElement` and `XAttribute` in queries?
7. How do namespaces affect LINQ to XML queries (`XNamespace`, `XName.Get`)?
8. When should you use `XmlReader` streaming vs LINQ to XML DOM-style loading?
9. How do you handle malformed XML and exceptions in LINQ to XML pipelines?
10. What are performance and memory considerations for large XML documents with LINQ to XML?
11. How do you transform XML shape with functional-style projections?
12. What is the difference between `XDocument.Save` formatting options and writer-based output?

### Gotchas — Module 05
1. **Multiple enumeration** — Deferred queries re-run on each `foreach`; dangerous with DB connections, file streams, or random sources.
2. **`.ToList()` too early with EF Core** — Materializing before filtering/projects entire tables into memory.
3. **Unstable paging** — `.Skip`/`Take` without `OrderBy` yields nondeterministic pages in SQL.
4. **Double `OrderBy`** — Second `OrderBy` replaces the first sort key; use `ThenBy` for secondary keys.
5. **`.Single()` vs `.First()`** — `.Single()` throws on zero *or* more than one match; easy to misuse on filtered data.
6. **Closure over loop variable in LINQ** — `.Where(x => x.Id == ids[i])` inside a loop captures the wrong index/value.
7. **`.Count()` cost** — O(1) on `ICollection<T>`; O(n) when the sequence must be fully walked.
8. **Set operators and comparers** — Without explicit `IEqualityComparer`, reference types may not dedupe as expected.
9. **`GroupBy` vs `ToLookup` timing** — `GroupBy` is deferred; `ToLookup` executes immediately and is immutable.
10. **Client evaluation surprises** — Custom CLR methods in `Where`/`Select` may pull data client-side silently or fail translation in strict EF Core mode.

---

## Module 06. Multithreading & Async Programming

### 01. Threads & Thread Lifecycle
1. Explain multithreading in C# and when it is appropriate vs async I/O or tasks.
2. What is a `Thread`, and how do you create and start one?
3. What are foreground vs background threads, and how do they affect process shutdown?
4. What are the main thread states in the lifecycle (unstarted, running, wait/sleep/join, stopped)?
5. What is `Thread.Join()`, and what happens if you never join a foreground thread?
6. What is `Thread.Sleep()` vs spinning vs waiting — when is each appropriate?
7. What is thread affinity, and why does it matter for UI applications?
8. What is the difference between creating a raw `Thread` and using thread pool threads?
9. What are `Thread.Name`, `IsBackground`, `Priority` — which actually affect scheduling?
10. What is a race condition at the thread level, and how can two threads interleave unpredictably?
11. What is the difference between kernel threads and managed threads (conceptual model)?
12. Why is manually creating many threads often a scalability anti-pattern?
13. What is `ThreadStatic`, and how does it differ from `ThreadLocal<T>`?
14. What exceptions can occur when aborting or interrupting threads (historical vs modern guidance)?
15. How does the main thread exiting affect background work still running?

### 02. ThreadPool
1. What is the thread pool in .NET, and why is it preferred over creating raw threads?
2. How does the thread pool manage worker threads and I/O completion threads?
3. What is hill-climbing in the .NET thread pool (high level)?
4. What is `ThreadPool.QueueUserWorkItem`, and how does it relate to `Task.Run`?
5. What is starvation in the thread pool, and what causes it?
6. How do synchronous blocking calls inside pool threads affect throughput?
7. What is the difference between dedicated threads and pool threads for long-running work?
8. What is `ThreadPool.SetMinThreads` / `SetMaxThreads`, and when might you tune them?
9. How does the thread pool interact with `async`/`await` continuations?
10. What is the danger of blocking the UI thread vs blocking a pool thread?
11. How do thread pool threads relate to `Parallel.For` and PLINQ?
12. What diagnostics exist for thread pool queue length and thread counts (`ThreadPool.ThreadCount`, ETW)?

### 03. Tasks & Task Parallel Library
1. What is the Task Parallel Library (TPL)?
2. Explain the difference between `Thread` and `Task` in purpose and scheduling.
3. Explain `Task`, `Task<T>`, and `ValueTask<T>` — when to use each.
4. What is `Task.Run`, and when should it be used vs when it should be avoided?
5. What is `Task.Factory.StartNew`, and why is `Task.Run` usually preferred?
6. Explain task continuations with `ContinueWith` — options, scheduling, and exception handling.
7. What is `Task.WhenAll`, `Task.WhenAny`, and how do they differ from manual continuation chaining?
8. What is `TaskCompletionSource<T>`, and what scenarios does it enable (bridging callbacks, manual completion)?
9. What is the difference between completing a `TaskCompletionSource` with result, exception, or cancellation?
10. What is `Task.FromResult`, `Task.CompletedTask`, and when are they preferable to `Task.Run`?
11. What is the difference between `AggregateException` and a regular exception when tasks fail?
12. How do child tasks relate to parent tasks (`TaskCreationOptions`, attached vs detached)?
13. What is task cancellation via `CancellationToken` registration vs `TrySetCanceled`?
14. What are unobserved task exceptions, and how does .NET handle them?
15. What is `ValueTask` pooling/caching, and why must consumers avoid double-awaiting unless documented safe?
16. How do you implement a timeout around a `Task` using `CancellationTokenSource` or `WhenAny`?

### 04. Async and Await
1. Explain asynchronous programming in C# — what problem does it solve?
2. Explain the `async` and `await` keywords in detail.
3. What is the difference between CPU-bound and I/O-bound async work?
4. What is `ConfigureAwait(false)`, and when should library vs application code use it?
5. How do you handle exceptions in async/await methods?
6. What is the difference between `async void`, `async Task`, and `async Task<T>`?
7. What is an async stream (`IAsyncEnumerable<T>`) in C# 8+, and how does `await foreach` work?
8. How does the async state machine work under the hood (high level: `MoveNext`, `IAsyncStateMachine`)?
9. What is synchronization context, and how does it affect continuation marshaling?
10. Why can `.Result`, `.Wait()`, and `.GetAwaiter().GetResult()` cause deadlocks?
11. What is the difference between `await task` and `return task` from an async method (async method builder behavior)?
12. How do you implement retry with exponential backoff in async code?
13. What is jitter in backoff strategies, and why is it used?
14. How do Polly-style resilience policies relate to manual retry loops?
15. What is `CancellationTokenSource.CreateLinkedTokenSource`, and when is linking tokens needed?
16. How do you propagate cancellation through layered async APIs?
17. What is `Task.Delay` vs `Thread.Sleep` in async methods?
18. What is "async all the way" — why is mixing blocking and async problematic?
19. How do you unit test async methods and time-dependent retry logic?
20. What is `IAsyncDisposable`, and how does `await using` work?

### 05. Parallel Programming
1. What is `Parallel.For` and `Parallel.ForEach`?
2. What is `ParallelOptions` (`MaxDegreeOfParallelism`, `CancellationToken`) used for?
3. What is a `Partitioner<TSource>`, and when would you supply a custom partitioner?
4. What is the difference between range partitioning and chunk partitioning?
5. Explain PLINQ (`AsParallel`, `WithDegreeOfParallelism`, `WithMergeOptions`).
6. When is parallelization slower than sequential execution?
7. What types of workloads benefit from PLINQ vs `Parallel.ForEach`?
8. What are thread-safe requirements when using parallel loops (shared state, locals, aggregation)?
9. How do you perform parallel aggregation with `lock`, `Interlocked`, or thread-local accumulators?
10. What is `ParallelLoopResult`, and how do you detect partial failures?
11. What are ordering guarantees in PLINQ (`AsOrdered`) and their cost?
12. How does parallel LINQ decide default partition sizes?
13. What exceptions are thrown from parallel loops (`AggregateException`, inner exceptions)?
14. How do you combine async I/O with parallel CPU work without blocking the pool?
15. What are best practices for parallel and async code in server applications?

### 06. Synchronization and Locks
1. Explain synchronization primitives: `lock`, `Monitor`, `Mutex`, and `Semaphore`/`SemaphoreSlim`.
2. What is `ReaderWriterLockSlim`, and when is it preferable to a plain `lock`?
3. Explain `AutoResetEvent`, `ManualResetEvent`, and `ManualResetEventSlim`.
4. What is `CancellationToken`, and how do you implement cooperative cancellation?
5. Explain deadlocks in multithreading — necessary conditions and prevention strategies.
6. What are race conditions, and how can they be prevented?
7. What is the `volatile` keyword, and when does it provide visibility guarantees?
8. What is the difference between `volatile` and `lock` for thread safety?
9. What is `Interlocked` (`Increment`, `CompareExchange`, `Add`), and when is it enough without `lock`?
10. What is `SpinLock`, and when might low-latency spinning beat `lock`?
11. What is lock ordering, and how does it prevent deadlock?
12. What is the `Monitor.TryEnter` pattern, and how do timeouts help avoid indefinite blocking?
13. What is async-compatible locking (`SemaphoreSlim.WaitAsync`) vs blocking `lock` in async code?
14. What is a priority inversion problem (conceptual), and which primitives exacerbate it?
15. How do you diagnose deadlocks and lock contention in production (dump analysis, `dotnet-sync`, counters)?
16. What is thread-safe lazy initialization (`Lazy<T>`, double-checked locking pitfalls)?

### 07. Concurrent Collections
1. What concurrent collections exist in .NET (`ConcurrentDictionary`, `ConcurrentQueue`, `ConcurrentBag`, `BlockingCollection`, etc.)?
2. When should you use thread-safe collections instead of standard collections plus locks?
3. What is the difference between `Dictionary<TKey, TValue>` and `ConcurrentDictionary<TKey, TValue>`?
4. What are `AddOrUpdate`, `GetOrAdd`, and `TryUpdate` on `ConcurrentDictionary`?
5. What is `BlockingCollection<T>`, and how does it implement producer-consumer patterns?
6. What is the difference between bounded and unbounded `BlockingCollection` behavior?
7. How do you use `BlockingCollection` with multiple producers and consumers?
8. What is `ConcurrentQueue` vs `ConcurrentStack` vs `ConcurrentBag` — ordering and stealing semantics?
9. When is `ConcurrentBag` the wrong choice despite being thread-safe?
10. What is `IProducerConsumerCollection<T>` and custom underlying stores for `BlockingCollection`?
11. How do concurrent collections compare to locking a `List<T>` for high-contention scenarios?
12. What enumeration semantics do concurrent collections provide (weakly consistent iterators)?
13. How do you gracefully complete adding to a `BlockingCollection` (`CompleteAdding`)?
14. What pitfalls arise when mixing concurrent collections with LINQ?
15. When should you use channels (`System.Threading.Channels`) instead of `BlockingCollection` in modern code?

### Gotchas — Module 06
1. **`.Result` / `.Wait()` deadlock** — Blocking async on a captured synchronization context (UI, legacy ASP.NET) deadlocks when the continuation needs that same context.
2. **`async void` swallows observability** — Exceptions cannot be awaited by callers; use only for event handlers.
3. **`Task.Run` for I/O** — Offloading blocking I/O to the pool wastes threads; prefer truly async APIs.
4. **Async does not mean threaded** — I/O `await` often completes without extra threads; continuations may run on any pool thread.
5. **Unobserved task exceptions** — Faulted tasks that are never awaited may surface later as unobserved exception events.
6. **Race on `List<T>`/`Dictionary<,>`** — Even `Add` is not thread-safe; use locks or concurrent collections.
7. **`ConfigureAwait(false)` in libraries** — Library code should not marshal back to UI context; app code often needs the default for UI updates.
8. **`ValueTask` double-await** — Re-awaiting or concurrent awaits on a pooled `ValueTask` can corrupt state unless documented safe.
9. **`TaskCompletionSource` set twice** — Second `TrySet*` calls fail; race to complete can drop results if not coordinated.
10. **`BlockingCollection` after `CompleteAdding`** — Adding throws; consumers must drain remaining items correctly.
11. **`Interlocked` is not composable** — Check-then-act on complex invariants still needs `lock` or careful CAS loops.
12. **`volatile` does not make operations atomic** — `i++` still races even if `i` is volatile.
13. **Parallel loop over small work** — Partitioning overhead can make `Parallel.ForEach` slower than sequential code.
14. **Shared `Random` is not thread-safe** — Use `Random.Shared` or thread-local RNG in parallel code.
15. **Retry without cancellation** — Exponential backoff loops must honor `CancellationToken` and max attempts to avoid runaway delays.

---

## Module 07. File Input & Output and Streams

### 01. File & Directory Operations
1. Explain file handling in C# and the role of the `System.IO` namespace.
2. What is the difference between the static `File`/`Directory` classes and the instance `FileInfo`/`DirectoryInfo` classes?
3. When would you prefer `FileInfo` over repeated `File.*` static calls on the same path?
4. How do `Directory.GetFiles`, `Directory.GetDirectories`, and their `Enumerate*` counterparts differ in memory behavior?
5. What does `Directory.CreateDirectory` do when intermediate folders already exist?
6. What is the difference between `File.Copy` with `overwrite: false` vs `overwrite: true`, and what exception indicates a conflict?
7. How does `File.Move` differ from copy-then-delete, and what happens to metadata and hard links?
8. What is `File.Replace`, and when is it preferable to manual backup-and-overwrite?
9. How do you safely delete a directory tree using `Directory.Delete(path, recursive: true)`?
10. What file metadata can you read via `File` static methods vs `FileInfo` instance properties?
11. Explain `File.GetCreationTime`, `GetLastWriteTime`, and `GetLastAccessTime` — and their `Utc` variants.
12. How do you set creation, last-write, and last-access timestamps programmatically?
13. What are `FileAttributes` (ReadOnly, Hidden, System, Archive)? How do you read and modify them?
14. What is the difference between `File.Exists` and attempting to open a file that may be deleted concurrently?
15. What exceptions should you expect during file operations (`FileNotFoundException`, `DirectoryNotFoundException`, `IOException`, `UnauthorizedAccessException`)?
16. How does `File.AppendAllText` differ from opening with `FileMode.Append`?
17. When is `File.ReadAllBytes` / `File.WriteAllBytes` appropriate vs stream-based APIs?
18. Explain `File.Create`, `File.Open`, `File.OpenRead`, and `File.OpenWrite` — what modes and access do they imply?
19. How do you handle TOCTOU (time-of-check-time-of-use) races when checking existence before read/write?
20. What is the difference between deleting a file and clearing its contents while keeping the path?

### 02. StreamReader & StreamWriter
1. Explain the `Stream` base class hierarchy and where `StreamReader`/`StreamWriter` fit.
2. What is the difference between `File.ReadAllText`, `File.ReadAllLines`, and `File.ReadLines`?
3. Why can `File.ReadLines` hold a file lock until enumeration completes?
4. How do `StreamReader.ReadLine`, `ReadToEnd`, and `ReadBlock` differ for large files?
5. What is the default encoding for `StreamReader` and `StreamWriter`, and why can that cause mojibake?
6. How do you specify `Encoding.UTF8`, UTF-8 with BOM, and legacy encodings (`Encoding.GetEncoding`)?
7. What does `StreamReader.DetectEncodingFromByteOrderMarks` control?
8. Explain async read/write methods on `StreamReader`/`StreamWriter` (`ReadLineAsync`, `WriteLineAsync`, `ReadToEndAsync`).
9. What is `StreamWriter.AutoFlush`, and when should you call `Flush()` explicitly?
10. How do you append text to an existing file with `StreamWriter` (constructor overload with `append: true`)?
11. What happens if you forget to dispose a `StreamWriter` — especially on Windows file locking?
12. Can you use `StreamReader`/`StreamWriter` with non-file streams (memory, network)? Give examples.
13. What is the difference between `using` blocks and C# 8 `using` declarations for stream cleanup?
14. How do you read a file line-by-line without loading it entirely into memory?
15. What is `TextReader`/`TextWriter`, and why do APIs often accept these abstractions?

### 03. FileStream & Binary Files
1. What is the difference between `File`, `Stream`, and `FileStream`?
2. Explain `FileMode` (`CreateNew`, `Create`, `Open`, `OpenOrCreate`, `Truncate`, `Append`) — when use each?
3. Explain `FileAccess` (`Read`, `Write`, `ReadWrite`) and `FileShare` (`None`, `Read`, `Write`, `ReadWrite`, `Delete`).
4. Why does default `FileShare.None` cause sharing violations when another process needs read access?
5. What are `FileStream.Position`, `Seek`, and `Length` — and when is seeking valid?
6. Explain `SeekOrigin` (`Begin`, `Current`, `End`) with a concrete read-modify-write scenario.
7. What happens if you seek on a non-seekable stream (e.g., some network streams)?
8. What is the difference between `FileStream.Read`/`Write` and `ReadAsync`/`WriteAsync`?
9. What do `BinaryReader` and `BinaryWriter` add over raw `FileStream` byte operations?
10. How does `BinaryReader` handle endianness and primitive types (`ReadInt32`, `ReadDouble`, `ReadString`)?
11. What is the on-disk format of `BinaryWriter.Write(string)` — and why does it matter for cross-platform files?
12. What is the difference between text and binary file handling in C#?
13. When should you use `MemoryStream` instead of `FileStream`?
14. What is buffered I/O, and how do `FileStream` buffer size options affect performance?
15. How do you read a fixed header followed by variable-length records from a binary file?
16. What is a file signature (magic bytes), and how do you validate one without trusting the extension?

### 04. Path & Environment Classes
1. What is the `Path` class, and why should you never hard-code `\` or `/` separators?
2. How does `Path.Combine` behave with trailing slashes, rooted segments, and empty segments?
3. What is the difference between `Path.GetFullPath` and passing a relative path directly to `File.Open`?
4. Explain `Path.GetDirectoryName`, `GetFileName`, `GetFileNameWithoutExtension`, and `GetExtension`.
5. What do `Path.GetTempPath`, `Path.GetTempFileName`, and `Path.GetRandomFileName` return — and what are the security implications of `GetTempFileName`?
6. How do `Path.IsPathRooted`, `HasExtension`, and `ChangeExtension` work?
7. What invalid path characters does `Path.GetInvalidPathChars` / `GetInvalidFileNameChars` expose?
8. What is `Environment.SpecialFolder`, and how do you resolve `MyDocuments`, `ApplicationData`, and `LocalApplicationData`?
9. How does `Environment.GetFolderPath` differ from hard-coding `C:\Users\...`?
10. What is `Environment.CurrentDirectory`, and how can it differ from the executable's location?
11. How do you get the application base directory in modern .NET (`AppContext.BaseDirectory`, `AppDomain.CurrentDomain.BaseDirectory`)?
12. What is the difference between absolute and relative paths in console apps vs ASP.NET Core?
13. How do UNC paths (`\\server\share`) interact with `Path.Combine` and `Path.GetFullPath`?
14. What cross-platform path differences matter when deploying the same code on Windows and Linux?

### 05. Working with CSV and Text Files
1. Why is there no built-in CSV parser in the BCL, and what libraries are commonly used?
2. What are RFC 4180 rules for CSV fields, delimiters, and record terminators?
3. When must a CSV field be wrapped in double quotes?
4. How do you escape a literal double quote inside a quoted CSV field?
5. What goes wrong if you split CSV lines on `Split(',')` without a proper parser?
6. How do you handle embedded newlines inside quoted CSV fields?
7. What issues arise with culture-specific decimal separators in CSV numeric columns?
8. How do you write CSV headers and ensure stable column ordering for downstream consumers?
9. What is the difference between `\n`, `\r\n`, and `Environment.NewLine` for text file line endings?
10. How do you normalize line endings when reading files produced on Windows vs Linux?
11. What are best practices for large CSV ingestion (streaming vs loading all rows)?
12. How do you validate CSV row shape (column count) before deserializing to objects?
13. When should you use fixed-width text formats instead of CSV?
14. How do you properly dispose file resources across layered readers (`FileStream` → `StreamReader`)?
15. What logging and rotation patterns apply when appending to text log files over time?

### Gotchas — Module 07
1. **`ReadAllLines` vs `ReadLines`** — `ReadAllLines` loads the entire file into a `string[]`; `ReadLines` is lazy but keeps the file open until enumeration finishes or is disposed.
2. **Undisposed streams lock files on Windows** — A finalized-but-not-disposed `FileStream`/`StreamWriter` can block deletes, renames, and antivirus scans until GC runs.
3. **`FileShare` defaults to exclusive access** — Opening without `FileShare.Read` prevents other processes from reading concurrently.
4. **Hard-coded path separators break cross-platform** — `"folder\\file.txt"` fails on Linux; always use `Path.Combine`.
5. **Relative paths depend on `CurrentDirectory`** — A path valid in Visual Studio may fail as a Windows Service or cron job where CWD differs.
6. **`Path.Combine` with an absolute second segment discards earlier parts** — `Path.Combine("C:\\a", "D:\\b")` yields `D:\b`, which surprises many candidates.
7. **Encoding mismatch silently corrupts text** — Default UTF-8 assumptions break on Windows-1252 or UTF-16 LE files; specify `Encoding` explicitly.
8. **Seeking past EOF then writing extends the file with undefined gap bytes** — Understand sparse/hole behavior when patching binary files in place.
9. **CSV `Split(',')` breaks on quoted commas** — `"Smith, Jr.",42` becomes three fields; use a real parser or state machine.
10. **Double-quote escaping in CSV is `""` not `\"`** — Getting escape rules wrong produces columns that shift on import.

---

## Module 08. Advanced C# Features

### 01. Serialization & Deserialization
1. What is serialization and deserialization, and what is an object graph?
2. Does deserialization resurrect original object identity or create new instances?
3. Compare JSON, XML, and binary as wire formats — trade-offs for APIs, config, and storage.
4. Explain `System.Text.Json.JsonSerializer.Serialize` and `Deserialize` for files and streams.
5. What is `JsonSerializerOptions`, and which settings affect naming, indentation, and enum handling?
6. Why is creating a new `JsonSerializerOptions` on every call a performance problem?
7. When should you use `JsonSerializerContext` source generators vs reflection-based serialization?
8. Explain `[JsonPropertyName]`, `[JsonIgnore]`, `[JsonInclude]`, and `[JsonPropertyOrder]`.
9. What happens when JSON contains properties not present on the C# type (extra members)?
10. What happens when JSON is missing a property mapped to a non-nullable reference type vs a value type?
11. How do enums serialize by default in `System.Text.Json`, and what production risk does numeric enum wire format create?
12. How do you serialize enums as strings using `JsonStringEnumConverter`?
13. How do you handle circular references in an object graph (`ReferenceHandler.Preserve` / `IgnoreCycles`)?
14. Why does serializing `Animal pet = new Dog()` sometimes drop `Dog`-only properties?
15. How do you enable polymorphic serialization in modern `System.Text.Json`?
16. What is `JsonNode`, `JsonObject`, and `JsonArray`, and when prefer them over strongly typed models?
17. How do you navigate and mutate JSON with `JsonNode` without deserializing to a fixed class?
18. What is a custom `JsonConverter<T>`, and when would you implement `Read`/`Write` manually?
19. How do `Utf8JsonReader` and `Utf8JsonWriter` differ from `JsonSerializer` helpers?
20. Explain `XmlSerializer` requirements (parameterless constructor, public read/write properties).
21. What do `[XmlRoot]`, `[XmlElement]`, `[XmlAttribute]`, and `[XmlIgnore]` control?
22. Why can `XmlSerializer` fail at runtime even when the project compiles?
23. What is `[Serializable]` actually used for in modern .NET?
24. Explain `BinaryFormatter` — why is it obsolete, and what security risks led to its removal?
25. What are recommended modern alternatives to `BinaryFormatter` for trusted internal persistence?
26. How does `DateTime` with unspecified `Kind` behave across time zones during serialization?
27. Why is `DateTimeOffset` often safer on the wire than `DateTime`?
28. Can a type with only get-only properties serialize but fail to deserialize? Why?
29. How do `[JsonConstructor]` and parameterized constructors interact with deserialization?
30. What is the difference between `System.Text.Json` and `Newtonsoft.Json` feature sets (contract customization, references)?

### 02. Reflection & Attributes
1. What is reflection in C#, and what problems does it solve?
2. What is the difference between early binding and late binding?
3. Explain `typeof(T)` vs `obj.GetType()` — compile-time token vs runtime type.
4. Why does `typeof(List<int>) == typeof(List<string>)` return false?
5. How do you obtain the generic type definition from a closed generic type?
6. What is the `Type` class, and what members expose metadata (methods, properties, fields, attributes)?
7. What is `BindingFlags`, and how do `Instance`, `Static`, `Public`, `NonPublic`, and `DeclaredOnly` combine?
8. How do you enumerate properties, methods, fields, and constructors with reflection?
9. How do you invoke a method dynamically via `MethodInfo.Invoke`?
10. What is `Activator.CreateInstance`, and how do you pass constructor arguments?
11. How do you create generic types at runtime (`MakeGenericType`) and invoke generic methods (`MakeGenericMethod`)?
12. How do you get and set property and field values through `PropertyInfo` / `FieldInfo`?
13. How can reflection access private members, and why is that a maintenance and security concern?
14. Explain `Assembly`, `Module`, `MemberInfo`, `MethodInfo`, `PropertyInfo`, and `FieldInfo` relationships.
15. What is the difference between `Assembly.Load`, `Assembly.LoadFrom`, and `AssemblyLoadContext`?
16. Can you unload an assembly in .NET Framework vs .NET Core / .NET 5+?
17. How do you discover and read custom attributes at runtime (`GetCustomAttribute`, `IsDefined`)?
18. How do you define and apply your own attribute classes (`AttributeUsage`)?
19. What are performance costs of reflection vs compiled code, and how do trimming/AOT affect it?
20. What is `Reflection.Emit`, and when is dynamic IL generation justified?
21. How does reflection interact with nullable reference type annotations?
22. What security permissions historically gated reflection, and what changed in modern .NET?

### 03. Regular Expressions
1. What is the purpose of the `Regex` class in C#?
2. Explain `Regex.IsMatch`, `Match`, `Matches`, `Replace`, and `Split`.
3. What is the difference between verbatim regex strings (`@"\d+"`) and escaped regular strings?
4. What are common metacharacters candidates should know (`.`, `*`, `+`, `?`, `^`, `$`, `\d`, `\w`, groups)?
5. What is catastrophic backtracking, and what pattern shapes trigger it?
6. How do you mitigate ReDoS using `Regex.MatchTimeout` or the `matchTimeout` parameter in .NET?
7. What happens when a regex times out — which exception is thrown?
8. What is atomic grouping's role in preventing backtracking explosions?
9. How does culture affect case-insensitive matching, and what does `RegexOptions.CultureInvariant` do?
10. When should you compile regexes with `RegexOptions.Compiled` (or source generators in .NET 7+)?
11. What is `RegexOptions.NonBacktracking` (.NET 7+), and what trade-offs does it have?
12. How do named capture groups work, and how do you read them from `Match.Groups`?
13. What is the difference between greedy and lazy quantifiers (`+` vs `+?`)?
14. When should you prefer `Regex` over simple `string.Contains` / `Split` for maintainability?
15. How do you validate input with regex without using it as a full parser (e.g., email, phone)?

### 04. Var, Dynamic & Special Keywords
1. Explain `var` — what is known at compile time vs runtime?
2. When is `var` required (anonymous types) vs merely convenient?
3. Explain the `dynamic` keyword and the DLR's role.
4. What is the difference between `var`, `dynamic`, and `object`?
5. When does `dynamic` defer member binding to runtime, and what errors appear only then?
6. What is `DynamicObject`, and when would you subclass it?
7. What is `ExpandoObject`, and how does it differ from `Dictionary<string, object>`?
8. Explain `nameof` — how does it help refactoring and logging?
9. What is the `global::` qualifier, and when is it needed to disambiguate namespaces?
10. What is `default` literal (C# 7.1+) vs `default(T)`?
11. What is `@` verbatim identifier syntax (`@class`, `@event`) used for?
12. What is unsafe code, and when are pointers justified in C#?
13. What is `stackalloc`, and how does it relate to performance-sensitive code?
14. What is `ref readonly` return, and how does it differ from returning by value?
15. How does `dynamic` interact with extension methods (why don't they bind dynamically)?

### 05. C# 7 Features
1. What are tuple deconstruction and named tuple elements?
2. How do `out` variables declared inline in method calls work?
3. What are discards (`_`), and where are they used (deconstruction, unused returns)?
4. Explain pattern matching enhancements in C# 7 — `is` type patterns and `switch` patterns.
5. What are `ref` returns and `ref` locals, and what safety rules apply?
6. What is `ref`/`in`/`out` in the context of `ReadOnlySpan`-era performance APIs (conceptual link)?
7. What are local functions, and how do they differ from lambdas for recursion and capture?
8. What are expression-bodied members beyond properties (methods, constructors, finalizers)?
9. What binary literals and digit separators (`0b1010`, `1_000_000`) improve in readability?
10. What is `throw` as an expression inside ternary/null-coalescing forms?
11. How do generalized async return types work (`ValueTask` as async return)?
12. What are `default` in generic constraints improvements in C# 7?

### 06. C# 8 Features
1. Explain nullable reference types — how do they differ from `Nullable<T>` value types?
2. What do `?`, `!`, and `#nullable` directives mean at compile time?
3. Are nullable reference annotations enforced at runtime?
4. What are default interface methods, and how do they relate to the diamond problem?
5. What are asynchronous streams (`IAsyncEnumerable<T>` and `await foreach`)?
6. Explain null-coalescing assignment (`??=`) with examples.
7. Explain range (`..`) and index (`^`) operators — how does `^1` differ from `Length - 1`?
8. What are `using` declarations vs `using` statements for IDisposable?
9. What are nullable-aware APIs in the BCL reacting to NRT (`NotNullWhen`, `MaybeNull`)?
10. What is `IAsyncDisposable`, and how does `await using` work?
11. What are static local functions, and why were they added?
12. What is a `readonly struct`, and what mutability restrictions apply to its members?
13. What is the `readonly` modifier on struct instance members (C# 8)?
14. What are stackalloc in safe contexts and `Span<T>` integrations introduced alongside C# 8?
15. What is target-typed `new()` vs explicit type names?

### Cross-chapter — Records & Pattern Matching *(C# 9–11; grouped here)*
1. What are records (C# 9), and what boilerplate do they synthesize?
2. What is the difference between `record class` and `record struct`?
3. How does value-based equality in records differ from default class equality?
4. What is the difference between positional records and records with manual properties?
5. Explain `with` expressions — how do they relate to non-destructive mutation?
6. What are init-only setters (`init`), and how do they differ from `{ get; set; }` and `{ get; }`?
7. Can init-only properties be set inside the type's constructors after object creation semantics?
8. What is primary constructor syntax for records/classes (C# 12 preview cross-ref) vs positional records?
9. What is pattern matching in modern C# beyond C# 7 — switch expressions, relational, logical, and property patterns?
10. Explain property patterns (`person is { Age: > 18, Name: var n }`).
11. What are relational patterns (`>`, `<=`) and combinator patterns (`and`, `or`, `not`)?
12. What is list patterns (C# 11) — `[_, .., var last]`?
13. What is `switch` expression vs traditional `switch` statement for exhaustiveness?
14. What happens when a `switch` expression is not exhaustive over an enum?
15. What is the difference between `is null` and `== null` when a type overloads `==`?
16. What are expression trees (`Expression<T>`), and how do they differ from delegates?
17. How are expression trees used by LINQ providers (EF Core, `IQueryable`)?
18. Why can't all C# lambdas be converted to expression trees?
19. What is the difference between compile-time constant patterns and runtime type patterns?
20. When should you prefer records over classes for DTOs and domain models?

### Gotchas — Module 08
1. **`typeof` vs `GetType()`** — `typeof(Base)` is known at compile time; `instance.GetType()` returns the actual runtime derived type.
2. **Serialization type loss** — Assigning `Animal ref = new Dog()` and serializing as `Animal` drops derived-only properties unless polymorphism is configured.
3. **`[Serializable]` ignored by System.Text.Json** — Candidates conflate legacy binary markers with modern JSON/XML serializers.
4. **`BinaryFormatter` is a security footgun** — Deserializing untrusted payloads enables remote code execution; obsolete/removed on modern .NET.
5. **Missing JSON property on non-nullable value type** — Deserialization may default the value silently; missing `required`/`[JsonRequired]` validation causes subtle bugs.
6. **Enum numeric wire values** — Renumbering enum members breaks persisted JSON; prefer string enums for long-lived contracts.
7. **`JsonSerializerOptions` not thread-safe for mutation** — Cache a configured instance; do not tweak shared options concurrently.
8. **`dynamic` hides errors until runtime** — Misspelled members compile; also blocks many refactorings and overload resolution surprises.
9. **Extension methods do not dispatch on `dynamic`** — Must cast to static type or call like static methods.
10. **Reflection string names don't refactor** — Renaming a property breaks reflection unless tests catch it.
11. **Regex without timeout on user input** — Crafted input can hang the process via catastrophic backtracking.
12. **Nullable reference types are annotations only** — `#nullable enable` does not stop null at runtime without guards.
13. **Records are still reference types (`record class`)** — Identity semantics differ from `record struct`; boxing/equality surprises follow.
14. **Expression trees cannot contain statements arbitrarily** — Many C# constructs are not translatable for EF/LINQ providers.

---

## Module 09. Unit Testing

### 01. Unit Testing Basics
1. What is unit testing, and how does it differ from integration, component, and end-to-end testing?
2. Explain the AAA pattern (Arrange, Act, Assert) and why order matters psychologically for readers.
3. What makes a good unit test (FIRST / TRICE — fast, isolated, repeatable, self-validating, timely)?
4. What is the System Under Test (SUT), and how do you identify its boundaries?
5. What is test coverage, and why can 100% line coverage still miss important bugs?
6. What is mutation testing, and how does it critique coverage metrics?
7. What is the difference between state-based and interaction-based testing?
8. What is a test fixture, and how is it different from a test case?
9. What are flaky tests, and what common causes (time, threading, shared state, external I/O)?
10. What is the test pyramid, and where do unit tests sit relative to integration tests?
11. What is TDD (Red-Green-Refactor), and what benefits/challenges does it bring?
12. When should a bug fix include a regression test?
13. What is the difference between testing public behavior vs internal implementation?
14. How do deterministic tests handle `DateTime.Now`, `Guid.NewGuid()`, and randomness?
15. What is arrange duplication, and when is shared setup justified vs harmful?

### 02. xUnit
1. What is xUnit.net, and how does its philosophy differ from MSTest and NUnit?
2. Explain `[Fact]` vs `[Theory]` — when is parameterized testing appropriate?
3. How do `[InlineData]`, `[MemberData]`, and `[ClassData]` supply theory inputs?
4. How does xUnit create test class instances — per test or per class?
5. What are `IClassFixture<T>` and `ICollectionFixture<T>`, and when use each?
6. How do collection definitions (`[Collection("Name")]`) serialize tests that share expensive resources?
7. What is `IAsyncLifetime`, and how does it replace async setup/teardown patterns?
8. How does xUnit handle parallel test execution by default, and how do you disable it?
9. What is `ITestOutputHelper`, and how is it injected into tests?
10. How do `[Trait("Category", "Slow")]` attributes help filter tests in CI?
11. How does constructor injection of dependencies work in xUnit test classes?
12. What happens if a test constructor throws — how does xUnit report it?
13. How do you assert exceptions with `Assert.Throws<T>` vs `Assert.ThrowsAsync<T>`?
14. What is the difference between returning `Task` from a test vs `async void`?
15. How do you run xUnit tests from CLI (`dotnet test`) and filter by fully qualified name?

### 03. MSTest
1. What NuGet packages compose an MSTest project (`MSTest.TestFramework`, `MSTest.TestAdapter`, `Microsoft.NET.Test.Sdk`)?
2. Explain `[TestClass]`, `[TestMethod]`, and how discovery finds tests.
3. What are `[DataTestMethod]` and `[DataRow]` equivalents to xUnit theories?
4. What is `[TestInitialize]` / `[TestCleanup]` vs `[ClassInitialize]` / `[ClassCleanup]` vs `[AssemblyInitialize]` / `[AssemblyCleanup]`?
5. Why must `[ClassInitialize]` and `[AssemblyInitialize]` be `static`?
6. What is the `[TestContext]` property, and what runtime services does it expose?
7. How does MSTest instance lifecycle differ from xUnit's new-instance-per-test model?
8. What are `[ExpectedException]` / `[ExpectedExceptionAttribute]` (legacy), and why is `Assert.ThrowsException` preferred?
9. What Assert helpers exist in MSTest (`Assert.AreEqual`, `Assert.IsTrue`, `Assert.ThrowsException`, `StringAssert`)?
10. How do you deploy test content files (`[DeploymentItem]`) — and what are modern alternatives?
11. What is `[Ignore]` / `[TestCategory]`, and how do you filter categories in `dotnet test`?
12. How does MSTest parallelization work (`Parallelize` attribute at assembly/class level)?
13. What is the difference between MSTest V1 and V2/V3 adapters in SDK-style projects?
14. When would teams choose MSTest over xUnit in greenfield .NET projects?
15. How do MSTest data sources (`[DynamicData]`) compare to xUnit `[MemberData]`?

### 04. Mocking & Test Doubles
1. Define the test double taxonomy — dummy, fake, stub, spy, mock; what distinguishes each?
2. What is a mock in the strict sense (interaction verification) vs informal "mock" meaning any fake?
3. What is a stub, and when do you configure return values without verifying calls?
4. What is a spy, and how does it record interactions for later assertion?
5. What is a fake (e.g., in-memory repository), and when is it preferable to mocks?
6. What is dependency injection's role in making code testable?
7. When should you use a mocking framework (Moq, NSubstitute, FakeItEasy) vs hand-written fakes?
8. What is the difference between mocking an interface vs a concrete class?
9. Why can't Moq intercept non-virtual methods on concrete classes?
10. What is `Mock<T>.Setup`, `Returns`, `Callback`, and `Verify` in Moq terms?
11. What is over-specification / brittle mocking, and how does it couple tests to implementation?
12. What is the difference between verifying behavior (`Verify`) vs asserting output state?
13. How do you mock `async` methods returning `Task` / `Task<T>`?
14. How do you substitute `HttpClient`, `ILogger<T>`, and `DateTime` abstractions in tests?
15. What is a seam, and how do partial wrappers or interfaces introduce testability?
16. When is integration testing with real dependencies better than mocking everything?
17. What are anti-patterns: mocking concrete DB providers, verifying private collaborators, testing framework code?
18. How do manual test doubles (hand-rolled stubs) compare to framework mocks for readability?

### Gotchas — Module 09
1. **Testing private methods directly** — Usually signals a design problem; test through public seams or extract collaborators.
2. **Shared mutable static state** — Tests pass alone but fail in parallel or random order; isolate with instance state or `[Collection]` serialization.
3. **Mocking concrete classes with non-virtual members** — Framework proxies cannot override sealed/non-virtual methods; depend on interfaces.
4. **`async void` test methods** — Runners may not observe exceptions; always return `Task` from async tests.
5. **Over-verifying mock calls** — `Verify` on every internal call breaks on refactor; assert outcomes and critical interactions only.
6. **Integration tests disguised as unit tests** — Real SQL/file/network makes tests slow and flaky; name and folder them honestly.
7. **MSTest `[ClassInitialize]` sharing mutable state** — Static setup mutated by one test leaks into others unless reset in `[TestInitialize]`.
8. **xUnit class fixtures shared across unrelated tests** — Fixture lifetime is per-class; accidental shared state causes order-dependent failures.
9. **Theory data referencing mutable objects** — Shared array/list mutated in one run corrupts later theory cases.
10. **Not awaiting async assertions** — `Assert.ThrowsAsync` must be awaited; fire-and-forget hides failures.
11. **Assuming test execution order** — xUnit and parallel MSTest do not guarantee order; tests must be independent.
12. **Confusing stub with mock** — Stubs set up responses; mocks (strict sense) verify interactions — mixing terms leads to wrong test design.

---

## Appendix — Topics Relocated

| Original / overlapping topic | Now lives in |
|---|---|
| `using` statement for IDisposable (basics) | Module 01, ch. 10 — Exception Handling |
| `async`/`await` file I/O (general async model) | Module 06, ch. 04 — Async and Await |
| `ConfigureAwait` in library code | Module 06, ch. 04 + Module 07 gotchas (I/O context) |
| `Span<T>` / `Memory<T>` low-level I/O | Advanced / performance topics (outside Fundamentals 07 scope) |
| `volatile` keyword | Module 06, ch. 06 — Synchronization and Locks |
| `IAsyncEnumerable<T>` async streams (language feature) | Module 08, ch. 06 — C# 8 Features |
| Nullable reference types (NRT) | Module 08, ch. 06 — C# 8 Features |
| Expression trees (LINQ provider angle) | Module 08, Cross-chapter — Records & Pattern Matching |
| Delegates, lambdas, closures | Module 04 — Functional Style Programming |
| DI for testability (ASP.NET Core container) | `05. ASP.NET Core` (separate repo module) |
| Integration testing with WebApplicationFactory | ASP.NET Core testing (separate repo module) |
| LINQ to Entities / EF Core query translation | `04. .NET Data Access/03. Entity Framework Core` |
| Source generators (`JsonSerializerContext`) | Module 08, ch. 01 — Serialization (extended); deep dive in Advanced .NET |

---

## Appendix — Deduplication Log

| Duplicate / overlap | Kept in | Notes |
|---|---|---|
| File dispose / `using` basics | Module 01, ch. 10 + Module 07, ch. 02/05 | Module 07 focuses on stream handles and file locks |
| Async file read/write | Module 06 + Module 07, ch. 02 | Module 07 asks I/O-specific API shapes; Module 06 owns async mechanics |
| `Path.Combine` cross-platform | Module 07, ch. 04 | Removed repeat from ch. 01 overview questions |
| Encoding / UTF-8 assumptions | Module 07 gotchas | Single consolidated trap (was in ch. 02 + gotchas) |
| Equality contract (`Equals`/`GetHashCode`) | Module 02, ch. 01 + Module 08, ch. 01 | Module 08 angle: serialization round-trip and record equality |
| `typeof` vs `GetType()` | Module 08 gotchas + ch. 02 | One gotcha; chapter questions go deeper on generics |
| Pattern matching (C# 7 vs 9+) | Module 01, ch. 06 (switch expressions) + Module 08 cross-chapter | Module 01: basics; Module 08: relational/list/property patterns |
| `IAsyncEnumerable` | Module 06, ch. 04 + Module 08, ch. 06 | Module 06: threading; Module 08: language feature |
| `[ExpectedException]` legacy MSTest | Module 09, ch. 03 only | Not repeated in xUnit chapter |
| Test double definitions | Module 09, ch. 04 only | xUnit/MSTest chapters reference ch. 04 taxonomy |
| `JsonSerializerOptions` performance | Module 08, ch. 01 + gotchas | Merged duplicate "new options every call" into ch. 01 Q6 + gotcha 7 |
| Records vs classes equality | Module 08 cross-chapter + gotcha 13 | Class vs struct record split clarified once |
| Reflection + custom attributes | Module 08, ch. 02 | Serialization ch. references attributes; full reflection depth in ch. 02 |
| BinaryFormatter security | Module 08, ch. 01 Q23–24 + gotcha 4 | Single extended treatment |
| CSV quoting / splitting | Module 07, ch. 05 + gotchas 9–10 | Parser pitfalls not duplicated in ch. 02 text I/O |
| LINQ deferred execution / `yield` | Module 03, ch. 08 + Module 05, ch. 01 | Module 03: iterator mechanics; Module 05: query pipeline behavior |
| Closure loop variable trap | Module 04 gotchas + Module 05 gotcha 6 | Module 04: general capture; Module 05: LINQ-specific call site |
| `ValueTask` usage | Module 06, ch. 03 + Module 08, ch. 05 | Module 06: task model; Module 08: C# 7 async return syntax |
