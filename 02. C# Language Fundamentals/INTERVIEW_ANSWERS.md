# C# Language Fundamentals — Interview Answers

Answers for [INTERVIEW_QUESTIONS.md](./INTERVIEW_QUESTIONS.md).  
---

## Module 01. C# Basics

### 01. Hello World

#### Q1. What is C# and what are its key features?

**Answer:** C# is a modern, statically typed, object-oriented programming language designed for the .NET platform, where source code is compiled to Intermediate Language (IL) and executed by the Common Language Runtime (CLR). It combines C-style syntax with garbage-collected memory management, strong typing, and a rich standard library so you can build console apps, services, desktop clients, and web APIs from the same language.

- C# is multi-paradigm: you write classes and interfaces for object-oriented design, but also use delegates, lambdas, and Language Integrated Query (LINQ) for functional-style data processing.
- The language evolves with the runtime through annual releases, adding features such as nullable reference types, pattern matching, records, and top-level statements while keeping backward compatibility within a target framework.
- As a .NET language, C# interoperates with other CLI languages (F#, VB.NET) because all of them target the same assembly model defined by ECMA-335.
- Key practical features include exception handling, generics, async/await for non-blocking I/O, and compile-time safety that catches many errors before the program runs.

---

#### Q2. Explain namespaces in C#.

**Answer:** A namespace is a hierarchical naming container that groups related types and prevents name collisions when two libraries define types with the same simple name. You declare a namespace in source code, and the fully qualified name of a type combines the namespace path with the type name (for example, `System.Console`).

- Namespaces do not dictate physical folder layout or assembly boundaries by themselves; they are a logical organization tool, though teams often mirror folder structure for readability.
- The root `System` namespace and its children (`System.Collections.Generic`, `System.IO`, and others) form the Base Class Library (BCL) surface you import with `using` directives.
- Two different assemblies can expose types in the same namespace, and the compiler resolves types by namespace plus assembly reference at build time.
- Without namespaces, every type name would have to be globally unique across all referenced libraries, which would be impractical in large ecosystems.

---

#### Q3. How do nested namespaces work in C#?

**Answer:** Nested namespaces express a parent-child hierarchy either by nesting `namespace` blocks inside one another or by using dotted names such as `Company.Product.Feature`, which the compiler treats as nested namespace declarations.

- Dot notation is syntactic sugar: `namespace A.B.C` is equivalent to nesting `namespace A { namespace B { namespace C { ... } } }`.
- Types inside a nested namespace are referenced with the full dotted path unless a `using` directive or alias shortens the name at the top of the file.
- Nested namespaces help large teams partition domains (for example, `MyApp.Services` vs `MyApp.Models`) without creating separate assemblies for every slice.
- The nesting is purely lexical; it does not automatically grant access to `internal` members of types in parent namespaces—access modifiers still follow class and assembly rules.

---

#### Q4. What is the purpose of the `using` directive (importing namespaces)?

**Answer:** The `using` directive tells the compiler to search specified namespaces when resolving unqualified type names, so you can write `Console.WriteLine` instead of the fully qualified `System.Console.WriteLine`. It affects compile-time name lookup only and does not copy code or change runtime behavior.

- A single file typically lists several `using` lines for namespaces whose types it references, keeping member access readable while the compiler still resolves to the same IL regardless of how many usings you add.
- You can also define `using` aliases (`using IO = System.IO;`) to disambiguate when two namespaces expose conflicting simple names.
- Removing a `using` does not remove the dependency on the underlying assembly; you must still reference the project or NuGet package that contains the type.
- Over-importing unused namespaces is harmless at runtime but may trigger analyzer warnings; add usings when the file actually needs types from that namespace.

---

#### Q5. Explain preprocessor directives in C# (`#if`, `#define`, `#region`, `#pragma`, etc.).

**Answer:** Preprocessor directives are compile-time instructions processed before semantic analysis; they conditionally include or exclude code, define symbols, organize editor regions, or suppress warnings without changing runtime semantics of the code that remains.

- `#define` and `#undef` create or remove conditional compilation symbols (often combined with `#if`, `#elif`, `#else`, `#endif`) to build debug-only logging, platform-specific branches, or feature flags.
- `#region` / `#endregion` fold code blocks in the IDE for navigation; they do not affect generated IL and should not replace meaningful structure.
- `#pragma warning disable` / `restore` and `#nullable enable` / `disable` tune analyzer and nullable reference type behavior for a file or a span of code.
- `#line` can remap reported line numbers when generating source, which matters for source generators and tooling rather than everyday application code.

---

#### Q6. What is the role of the `Main` method, and how has entry-point syntax evolved (classic `Main`, top-level statements)?

**Answer:** `Main` is the application entry point—the method the Common Language Runtime (CLR) invokes after loading the assembly to begin execution. Classic console apps declare `public static void Main(string[] args)` (or an equivalent return type/int signature), while C# 9+ allows top-level statements that the compiler synthesizes into a hidden `Main` method.

- The runtime requires exactly one entry point per executable project; duplicate `Main` methods produce compile error CS0017.
- `string[] args` receives command-line tokens split by the host; `args[0]` is the first argument after the program name (host-dependent).
- Top-level statements must appear in one file per project, before any type declarations, and are translated to a generated `Program` class with a `Main` method—ideal for scripts and small tools.
- `Main` can return `int` for process exit codes or be `async Task` / `async Task<int>` in modern templates so the entry point can await asynchronous work.

---

#### Q7. What is the difference between a project, a solution, and an assembly in a .NET workspace?

**Answer:** A solution (`.sln`) is a container that groups related projects for IDE and build orchestration; a project (`.csproj`) is the build unit that compiles source into one primary output assembly (and copies dependencies); an assembly is the compiled `.dll` or `.exe` containing IL and metadata that the CLR loads at runtime.

| Concept | Role |
|---|---|
| Solution | Organizes multiple projects, build order, and shared configuration |
| Project | Declares target framework, references, and compiles to an assembly |
| Assembly | Deployable unit of IL + metadata + manifest (name, version, references) |

- One solution commonly holds a web API project, a class library, and test projects, each producing its own assembly.
- Project references tell the compiler where to find dependent assemblies; at runtime the loader resolves them from the output folder or NuGet cache.
- The executable project's assembly contains the entry point; class library projects produce `.dll` files referenced by hosts.

---

#### Q8. What does the `global using` directive do (C# 10+), and when is it useful?

**Answer:** A `global using` directive imports a namespace for every source file in the project (or a defined subset), eliminating repeated `using System.Collections.Generic;` lines across dozens of files. The compiler treats it as if each file included that `using` at the top.

- SDK-style projects often generate `GlobalUsings.g.cs` when `<ImplicitUsings>enable</ImplicitUsings>` is set, adding common BCL namespaces automatically for console, web, and class library templates.
- You can add a `GlobalUsings.cs` file with `global using MyCompany.Shared;` so domain types resolve everywhere without per-file imports.
- `global using` aliases work too (`global using Json = System.Text.Json;`), giving a project-wide short name.
- Use global usings for truly common imports; keep file-specific or rare namespaces local to avoid hiding where a type originates.

---

#### Q9. Explain file-scoped namespaces (`namespace X;`) vs block-scoped namespace syntax.

**Answer:** File-scoped namespace syntax (`namespace MyApp;`) declares that all types in the file belong to `MyApp` without wrapping the entire file in an extra indentation level of braces, while block-scoped syntax (`namespace MyApp { ... }`) explicitly delimits the namespace with a code block.

- Both forms produce identical IL; the choice is readability and editor ergonomics, especially in files with a single namespace.
- File-scoped namespaces require C# 10 or later and must appear before other members; only one file-scoped namespace is allowed per file.
- Block-scoped namespaces still allow multiple namespaces in one file (unusual) and remain required when nesting multiple namespace levels with different members in the same file.
- Teams migrating legacy code often keep block syntax until they standardize on file-scoped style for new files.

---

#### Q10. What is the purpose of `Program.cs` in a console application, and what other files typically accompany it (`.csproj`, `global usings`)?

**Answer:** `Program.cs` holds the entry-point logic—either an explicit `Main` method or top-level statements—that starts the console application. It is the file developers open first to trace startup flow, though the runtime ultimately executes the compiled assembly described by the project file.

- The `.csproj` file defines the target framework (`<TargetFramework>net8.0</TargetFramework>`), output type (`Exe` vs `Library`), nullable settings, implicit usings, and package references.
- `GlobalUsings.cs` or generated global usings centralize namespace imports; some repos disable implicit usings while learning so every import is visible in source.
- Additional files include `appsettings.json` in larger apps, `AssemblyInfo` (often SDK-generated), and optional `Usings.cs` for project-wide aliases.
- In SDK-style projects, build artifacts land in `bin/` and `obj/` folders; source stays minimal with `Program.cs` plus types split into other `.cs` files as the app grows.

---

#### Q11. What is the Common Language Runtime (CLR), and how does C# code become executable?

**Answer:** The Common Language Runtime (CLR)—CoreCLR in modern .NET—is the managed execution engine that loads assemblies, verifies type safety where applicable, Just-In-Time (JIT) compiles IL to native machine code, and provides garbage collection, threading, and exception services. C# source is compiled by Roslyn into IL and metadata stored in a `.dll` or `.exe`, which the host (`dotnet` CLI or a native apphost) starts by loading the CLR.

1. **Compile** — The C# compiler translates `.cs` files into IL instructions and embeds type metadata in a portable executable assembly.
2. **Launch** — The host reads the target framework from the `.runtimeconfig.json` and loads CoreCLR into the process.
3. **Load** — The loader reads the assembly manifest, resolves references from the output directory, and constructs runtime types from metadata.
4. **JIT** — On first method invocation, the JIT compiler translates IL to CPU-specific native code and caches it for later calls.
5. **Execute** — The processor runs native instructions while the CLR manages memory, exceptions, and thread scheduling.

See Module questions on IL vs JIT (Q12) for the distinction between build-time IL emission and runtime native compilation.

---

#### Q12. What is the difference between compiling to IL and JIT compilation at runtime?

**Answer:** Compiling C# to IL happens at build time and produces portable, CPU-neutral bytecode plus metadata in an assembly, while JIT compilation happens at runtime when the CLR translates each method's IL into native machine instructions for the actual processor executing the process.

- IL compilation is done by Roslyn (or another C# compiler); the output is the same regardless of whether the app runs on x64, ARM64, or another supported architecture.
- JIT runs on first use of each method (with tiered compilation optimizing hot paths over time), so startup pays compilation cost lazily rather than ahead of time for every method.
- Alternatives such as ReadyToRun embed precompiled native images for faster startup, and Native Ahead-of-Time (AOT) compilation avoids JIT entirely for trimmed deployments.
- IL keeps assemblies compact and language-neutral; JIT enables processor-specific optimizations that would be impossible to bake in fully at build time on every target machine.

---

#### Q13. What are SDK-style projects, and what does `<TargetFramework>` in the `.csproj` control?

**Answer:** SDK-style projects use a concise `.csproj` that begins with `<Project Sdk="Microsoft.NET.Sdk">` and relies on MSBuild SDK defaults to glob `.cs` files automatically, replacing the verbose legacy Framework project format. The `<TargetFramework>` element (or `<TargetFrameworks>` for multi-targeting) sets the Target Framework Moniker (TFM), which selects the API surface and runtime the compiler assumes.

- `net8.0` targets modern .NET 8 with its full BCL; `net48` targets .NET Framework 4.8; `netstandard2.0` targets the portable API contract.
- The TFM controls which NuGet packages are compatible, which language features are available, and which runtime must be installed to execute the built output.
- SDK-style projects integrate NuGet restore, implicit usings, and `dotnet build` / `dotnet run` without hand-maintaining file lists.
- Changing TFM can enable or disable APIs—migrating from `net472` to `net8.0` unlocks modern libraries but may require code changes for removed APIs.

---

#### Q14. When would you use `#nullable enable` at the project or file level?

**Answer:** `#nullable enable` turns on nullable reference type (NRT) analysis so the compiler warns when you assign `null` to a non-nullable reference type or dereference a value that might be null. You apply it project-wide via `<Nullable>enable</Nullable>` in the `.csproj` or per file with `#nullable enable` at the top when migrating legacy code incrementally.

- NRT annotations (`string` vs `string?`) are compile-time contracts; the runtime still allows null references unless you enforce checks in code.
- Enable nullable when writing new code or refactoring modules where null-related bugs (`NullReferenceException`) are costly, such as public APIs and service layers.
- File-level enable lets you modernize one class at a time in a large codebase without fixing every warning in a single change.
- Pair NRT with defensive patterns (`??`, null-conditional `?.`, `ArgumentNullException.ThrowIfNull`) so warnings reflect actual runtime guarantees.

---

#### Q15. What is the difference between `Console.Out`, `Console.Error`, and writing directly with `Console.WriteLine`?

**Answer:** `Console.WriteLine` writes to standard output (`stdout`) through the `Console.Out` `TextWriter`, while error messages intended for diagnostic streams should go to `Console.Error`, which maps to `stderr`. Both are static properties you can redirect, but the convenience methods on `Console` target `Out` by default.

- Operating systems and hosts can pipe `stdout` and `stderr` separately; logging frameworks and CI systems often capture stderr for errors while treating stdout as normal program output.
- `Console.SetOut` and `Console.SetError` replace the writers—for example, redirecting output to a `StringWriter` in tests—without changing call sites that use `Console.WriteLine`.
- `Console.WriteLine` is equivalent to `Console.Out.WriteLine`; there is no separate `Console` buffer—it's a facade over the underlying writers.
- Use `Console.Error.WriteLine` for failure messages when stdout is consumed by another process (piping, automation) so errors remain visible on the error stream.

---

### 02. Data Types & Variables

#### Q1. What are the different data types in C#?

**Answer:** C# data types fall into value types (stored inline with their data, such as `int`, `bool`, `char`, and `struct`) and reference types (variables hold a reference to heap objects, such as `string`, arrays, and `class` instances). The type system also includes pointer-like `unsafe` types, generic type parameters, and special types like `dynamic` and `object`.

- Built-in numeric types span signed and unsigned integers (`sbyte` through `ulong`), floating-point (`float`, `double`), high-precision decimal (`decimal`), and platform-sized `nint`/`nuint`.
- Reference types include `string`, delegates, interfaces, arrays, and user-defined classes; all reference types inherit from `System.Object`.
- Nullable value types (`int?`, `bool?`) wrap value types so they can represent an absent value with `null`.
- Enumerations (`enum`) and tuples (`(int Id, string Name)` or `ValueTuple`) model named constants and lightweight multi-value groupings respectively.

---

#### Q2. What are value types and reference types in C#?

**Answer:** Value types store their data directly in the variable's storage location, so assigning one variable to another copies the bits of the value. Reference types store a reference (address) to an object on the managed heap, so assignment copies the reference while both variables may point to the same object.

- Structs and enum underlying types are value types; classes, interfaces (as references to implementing objects), strings, and arrays are reference types.
- Value types cannot be `null` unless wrapped in `Nullable<T>` (`int?`); reference types default to `null`.
- Value types inherit from `System.ValueType` (which inherits `object`); reference type variables always refer to heap objects with object header and method table.
- Understanding the distinction drives correct equality semantics, parameter passing behavior, and performance (heap allocation vs stack/local storage).

---

#### Q3. What is the difference between value types and reference types?

**Answer:** The practical difference is what gets copied on assignment and where mutable state lives: value types copy data, reference types copy pointers to shared heap objects. Method parameters and returns follow the same rules unless modified by `ref`, `out`, or `in`.

| Aspect | Value type | Reference type |
|---|---|---|
| Assignment | Copies entire value | Copies reference; both may alias same object |
| Default | Zero-bit pattern (0, false, etc.) | `null` |
| Storage | Often stack or inline in object | Object on heap; variable holds reference |
| Mutability | Mutating copy does not affect original | Mutating object affects all references |

See Q2 for definitions; see Q13 for stack vs heap nuance and Q4 for boxing when value types meet reference contexts.

---

#### Q4. What is boxing and unboxing in C#?

**Answer:** Boxing converts a value type instance into a reference-type `object` (or interface) by copying the value onto the heap and wrapping it in a boxed object, while unboxing extracts the value type back from that object with an explicit cast. Both operations have allocation and type-check costs.

- Boxing occurs when you assign an `int` to `object`, call a non-generic collection's `Add(1)`, or invoke an interface method on a struct through the interface reference.
- Unboxing requires an explicit cast to the exact value type (`(int)obj`); wrong types throw `InvalidCastException`.
- Repeated boxing in hot loops (for example, storing many integers in `ArrayList`) causes garbage collection pressure—prefer generic collections like `List<int>`.
- Nullable value types box as either `null` or a boxed underlying value, not a boxed `Nullable<T>` wrapper.

---

#### Q5. Explain the `var` keyword in C#.

**Answer:** `var` instructs the compiler to infer the variable's type from the initializer expression at compile time, producing the same strong typing as an explicit declaration. Once inferred, the variable remains that fixed type—you cannot later assign an incompatible type.

- `var x = 10;` is compiled as `int x = 10;`; `var` is not `dynamic` and does not defer type checking to runtime.
- You must initialize `var` variables in the declaration because the compiler has no expression from which to infer a type.
- `var` improves readability for long generic types (`Dictionary<string, List<Order>>`) but can obscure intent when the initializer is unclear.
- See Q20 for when explicit types are required (no initializer, `null` without target type, or public API signatures).

---

#### Q6. What are nullable types in C#? (including nullable reference types in C# 8+)

**Answer:** Nullable value types (`int?`, `bool?`, etc.) extend value types with a `HasValue` flag so they can represent missing data, while nullable reference types (`string?` vs `string`) add compile-time annotations indicating whether a reference may be null under `#nullable enable`.

- `int?` is shorthand for `Nullable<int>` and supports `null`, `.Value`, and `.GetValueOrDefault()`.
- Nullable reference types do not change runtime behavior—the CLR still allows null on any reference; the compiler emits warnings when flow analysis cannot prove safety.
- Use nullable value types for optional numeric or date fields in databases and APIs; use NRT for documenting optional strings and navigation properties.
- The null-forgiving operator (`!`) suppresses warnings when you have external guarantees the compiler cannot see.

---

#### Q7. Explain the `default` keyword and default values in C#.

**Answer:** The `default` keyword produces the type's zero-initialized value: numeric zero, `false` for `bool`, `\0` for `char`, and `null` for reference types and nullable types. `default(T)` in generics applies the same rule for any type parameter `T`.

- Local variables must be assigned before use; `default` is common when you need a placeholder before conditional assignment.
- Struct fields initialize to default before constructors run unless field initializers override them.
- `default` for `int?` is a null nullable with no value; for `string` it is `null`.
- See Q22 for the difference between `default(int)` and `default` on reference types—they both yield zero/`null` but generic `default(T)` unifies the pattern.

---

#### Q8. What are constants, literals, and readonly fields in C#?

**Answer:** Literals are source-code representations of fixed values (`42`, `"hi"`, `3.14m`), constants (`const`) are compile-time constants embedded in metadata, and `readonly` fields are assigned at declaration or in the constructor but can differ per instance or per run.

- Integer literals default to `int`; suffixes (`L`, `M`, `F`, `D`) select `long`, `decimal`, `float`, or `double`.
- `const` values must be computable at compile time and are implicitly static.
- `readonly` instance fields can be set in the constructor, enabling per-object configuration that constants cannot express.
- String literals live in the metadata and may be interned; see Module 01 Strings chapter for immutability implications.

---

#### Q9. What is the difference between `const` and `readonly`?

**Answer:** `const` fields must be compile-time constants and are implicitly static, while `readonly` fields can be set at runtime in instance or static constructors and may hold values computed when the program runs.

| | `const` | `readonly` |
|---|---|---|
| When set | Compile time only | Declaration or constructor |
| Static | Always implicit static | Instance or explicit `static readonly` |
| Types allowed | Numeric, `bool`, `char`, `string`, null ref | Any type, including `DateTime` from runtime |
| Per instance | No—one shared value | Instance fields differ per object |

Use `const` for true symbolic constants; use `readonly` for configuration loaded at startup (see Gotcha 3 on `DateTime.Now`).

---

#### Q10. What is an enum in C#?

**Answer:** An enumeration defines a named set of integral constants sharing one underlying type (default `int`), giving readable names to magic numbers such as days of the week or HTTP status categories. Enums are value types backed by their underlying integer.

- By default, the first member is 0 unless you assign explicit values; unspecified members increment by one.
- You can specify `: byte`, `: long`, or other integral underlying types to control size and interoperability.
- `[Flags]` enums combine bitwise values (`Read | Write`) and typically use powers of two for members.
- Enums convert implicitly to their underlying type and can be parsed from strings with `Enum.Parse` or `Enum.TryParse`.

---

#### Q11. What is a `struct` in C#? (basics — comparison with `class` is in OOP)

**Answer:** A `struct` is a value type that encapsulates fields and methods inline, suitable for small, immutable or frequently copied data bundles where heap allocation should be avoided. Structs inherit from `System.ValueType` and do not support inheritance beyond interfaces.

- Structs are copied on assignment and passed by value unless you use `ref` or `in` modifiers.
- Parameterless constructors are supported from C# 10 onward with explicit field initialization rules; earlier versions relied on implicit zero initialization.
- Large structs can hurt performance when copied repeatedly; classes may be better for mutable or large state.
- Module 02 covers struct vs class trade-offs in depth (storage, identity, polymorphism).

---

#### Q12. What is a tuple in C#? (ValueTuple vs `Tuple<T>`)

**Answer:** Tuples group multiple values into one lightweight value without defining a named class. C# 7+ value tuples (`(int Id, string Name)`) are mutable structs with named fields, while `Tuple<T1,T2,...>` is a reference-type class from earlier APIs.

- Value tuples use `(1, "Ada")` syntax, deconstruction, and `Item1`/`Item2` or custom element names for readability.
- `Tuple.Create` allocates on the heap and is legacy; prefer value tuples for local multi-return scenarios.
- Value tuples are value types—assignment copies all elements; equality compares element values when types match.
- Use a named record or class when the grouping becomes part of a public API with behavior and invariants.

---

#### Q13. Where do value types typically live (stack vs heap), and where do reference types live?

**Answer:** Local value type variables and parameters usually live in stack frames or CPU registers, while reference type objects always live on the managed heap with the variable holding a reference. Value types embedded in classes or boxed into `object` also reside on the heap as part of the containing object or box.

- The CLR may optimize away the stack entirely through registers and may allocate value types on the heap when they escape the method (closures, async state machines).
- Arrays of value types store elements contiguously on the heap inside the array object; the array variable itself is a reference.
- "Stack vs heap" is a teaching model; the accurate rule is value types copy by value, reference types identify heap objects by reference.
- See Q14 for boxing, which moves a value type copy onto the heap wrapped in an object header.

---

#### Q14. When a value type is boxed, where does the data end up, and why does that matter for performance?

**Answer:** Boxing copies the value type's bits into a new heap object with an object header and method table pointer, so the original stack or inline value is separate from the boxed copy. Each box is a heap allocation that the garbage collector must eventually reclaim.

- Interface calls on structs often box because the dispatch requires a reference to an object implementing the interface.
- Non-generic collections (`ArrayList`, `Hashtable`) box every value type element on insertion.
- Hot-path boxing causes allocation churn, increased GC pauses, and cache misses compared to generic `List<T>` or span-based APIs.
- Mutating the struct after boxing does not change the boxed copy—another reason to avoid unintended boxing.

---

#### Q15. What is the difference between `int`, `long`, `decimal`, `float`, and `double` — when would you choose each?

**Answer:** `int` and `long` are fixed-point integers for counting and indexing; `float` and `double` are binary floating-point types for scientific and graphics math; `decimal` is a 128-bit base-10 floating type designed for financial calculations where binary rounding errors are unacceptable.

| Type | Size | Typical use |
|---|---|---|
| `int` | 32-bit signed integer | Loop counters, IDs, general arithmetic |
| `long` | 64-bit signed integer | Large counts, timestamps in ticks |
| `float` | 32-bit binary float | GPU/math when memory bandwidth matters |
| `double` | 64-bit binary float | Default floating literal type, scientific code |
| `decimal` | 128-bit base-10 | Money, tax, accounting (suffix `m`) |

Choose `decimal` for currency; choose `double` for physics simulations; choose integers when fractional parts are impossible by domain rules.

---

#### Q16. What is the difference between signed and unsigned integer types (`int` vs `uint`, etc.)?

**Answer:** Signed integers (`sbyte`, `short`, `int`, `long`) represent negative and positive values using two's complement, while unsigned types (`byte`, `ushort`, `uint`, `ulong`) represent zero and positive values only, doubling the positive range at the same bit width.

- `int` ranges approximately ±2.1 billion; `uint` ranges 0 to ~4.3 billion at 32 bits.
- Mixing signed and unsigned in expressions follows C# promotion rules and can surprise you when comparing across signs.
- Use unsigned types for bit masks, hash codes, and protocols that define unsigned fields—not as a trick to get "more positive int" without domain justification.
- `char` is unsigned 16-bit for UTF-16 code units, not a signed numeric type in practice.

---

#### Q17. What is `char` in C# — is it a numeric type or a text type, and how does it relate to Unicode?

**Answer:** `char` is a 16-bit Unicode code unit (UTF-16) value type, technically numeric under the hood but used primarily to represent a single text element or half of a surrogate pair. It is not a full Unicode grapheme or string.

- Literal `'A'` is a `char`; strings `"A"` are reference types of length one or more UTF-16 code units.
- Characters outside the Basic Multilingual Plane encode as surrogate pairs—two `char` values in one `string`.
- Arithmetic on `char` is legal (promotes to `int`) but rarely appropriate except for range checks or custom encodings.
- For full Unicode text, use `string`; for code-point-level work, consider `Rune` (.NET Core 3+) or UTF-8 APIs.

---

#### Q18. What is the difference between `bool` and nullable `bool?` in terms of default values and usage?

**Answer:** `bool` defaults to `false` and always holds `true` or `false`, while `bool?` defaults to `null` meaning "unknown or not specified" in addition to `true` and `false`.

- Use `bool?` for optional form fields (tri-state checkbox) or database columns that allow NULL.
- `bool?` has `.HasValue` and `.Value`; accessing `.Value` when null throws `InvalidOperationException`.
- Nullable booleans participate in lifted operators (`bool? a = true; bool? b = null; var c = a & b;` yields `null`).
- Prefer `bool` when the domain requires a definite yes/no with no missing state.

---

#### Q19. Explain the `??` (null-coalescing) and `??=` (null-coalescing assignment) operators with nullable types.

**Answer:** The `??` operator returns the left operand when it is not null; otherwise it evaluates and returns the right operand. The `??=` operator assigns the right side to the left variable only when the left is currently null.

- `name ?? "Guest"` yields `"Guest"` when `name` is null—common for defaults on nullable reference and value types.
- `cache ??= new Dictionary<string, string>();` lazily initializes `cache` once without repeating null checks.
- The right side of `??` is not evaluated unless the left is null, which matters when the fallback is expensive or has side effects.
- Chaining (`a ?? b ?? c`) walks left to right until a non-null value appears.

---

#### Q20. What is the difference between `var` and an explicit type declaration — when must you use explicit types?

**Answer:** `var` and explicit declarations produce identical IL when the inferred type matches; the choice is readability. You must use an explicit type when there is no initializer, when the initializer is `null` without a target-typed context, or in public member signatures where `var` is not permitted.

- `var list = new List<int>();` is idiomatic; `List<int> list = new();` target-typed `new` also works from C# 9.
- `string? name = null;` must name the type because `var name = null;` does not compile.
- Interface or base-class typing often requires explicit types: `IEnumerable<int> ids = GetIds();` even if `var` could infer `List<int>`.
- See Q5 for how inference works at compile time.

---

#### Q21. What are digit separators in numeric literals (e.g., `1_000_000`), and what problem do they solve?

**Answer:** Digit separators are underscore characters placed inside numeric literals to improve human readability; the compiler ignores them, so `1_000_000` equals `1000000`. They work in integer, floating, binary (`0b1010_0001`), and hex literals.

- They reduce transcription errors when reading large constants such as file sizes, bit masks, or financial limits.
- You cannot place two underscores adjacent or lead/trail an underscore in ways the grammar forbids (for example, `_100` is invalid).
- Separators do not affect runtime type or value—only source readability.
- They pair well with explicit suffixes (`1_000_000L`, `3.14_15_92d`) in configuration and test data.

---

#### Q22. What is the difference between `default(int)` and `default` for a reference type?

**Answer:** Both forms yield the type's zero value: `default(int)` is `0`, and `default(string)` (or `default` for any reference type) is `null`. `default(T)` in generic code applies the same rule uniformly without knowing `T` at source-writing time.

- `default` without a type argument is inferred from context in declarations like `int x = default;`.
- For nullable value types, `default(int?)` is null with `HasValue == false`.
- There is no behavioral difference between `default(string)` and `null`—they are the same constant concept.
- Use `default` in generic libraries to initialize locals and return slots when `T` might be value or reference type.

---

#### Q23. What is a nullable reference type annotation (`string?` vs `string`), and is enforcement compile-time or runtime?

**Answer:** Under nullable reference type analysis, `string` means the author intends a non-null reference and `string?` allows null, with the compiler emitting warnings when assignments and dereferences violate those intentions. Enforcement is compile-time only—the runtime does not distinguish `string` from `string?`.

- Flow analysis tracks whether a variable might be null after conditionals (`if (s is not null)`).
- Attributes like `[NotNullWhen(true)]` on `Try` methods improve analysis for patterns the compiler cannot infer alone.
- Without `#nullable enable`, annotations are ignored for warning purposes though they document intent.
- Runtime null checks still require explicit code; NRT prevents many bugs early but is not a runtime guard.

---

#### Q24. What happens when you assign `null` to a non-nullable reference type variable under `#nullable enable`?

**Answer:** The compiler emits a warning (typically CS8625 or CS8600 depending on context) because you promised the variable should not hold null, yet you assigned null anyway. The code still compiles unless warnings are treated as errors, and the assignment succeeds at runtime like any reference assignment.

- Suppress only with documented justification (`!`, `#pragma`, or `[AllowNull]` attributes on APIs).
- Fixing the warning means changing the type to nullable, ensuring initialization in all paths, or using a non-null default (`string.Empty`).
- Treat warnings as design feedback: either the annotation or the assignment logic is wrong.
- See Q23 for the compile-time-only nature of NRT enforcement.

---

#### Q25. What is the difference between `object` as a universal base type and using `dynamic`?

**Answer:** `object` is the root reference type for all managed types; assigning a value boxes value types and requires casts or pattern matching to use specific members after storage as `object`. `dynamic` defers member resolution to runtime via Dynamic Language Runtime (DLR) binding, skipping compile-time member checking.

- `object o = 42; int n = (int)o;` needs explicit unboxing.
- `dynamic d = GetSomething(); d.AnyMethod();` compiles even if `AnyMethod` might not exist—failures become runtime `RuntimeBinderException`.
- Use `object` for heterogeneous collections and reflection scenarios with explicit type tests.
- Use `dynamic` sparingly for COM interop and truly dynamic payloads; prefer strong typing or `JsonSerializer` elsewhere.

---

#### Q26. What are `nint` and `nuint`, and when might you encounter them?

**Answer:** `nint` and `nuint` are native-sized signed and unsigned integer types whose width matches a pointer on the platform (32-bit on 32-bit processes, 64-bit on 64-bit processes). They alias `System.IntPtr` and `System.UIntPtr` for interop and low-level pointer arithmetic.

- Common in P/Invoke signatures mirroring C `intptr_t`/`size_t` and in `Span<T>` length scenarios tied to platform limits.
- Arithmetic on `nint` avoids casting pointers to `long` on 64-bit while remaining correct on 32-bit.
- They are value types but not considered "built-in" in the same sense as `int`; use when size must track pointer width.
- Unsafe code and interop layers are the typical application domains—not general business logic.

---

#### Q27. What is the difference between declaring a variable with and without an initializer?

**Answer:** A declaration with an initializer (`int count = 0;`) assigns a value at definition time, while a declaration without an initializer (`int count;`) leaves the variable in an uninitialized state until assigned—legal for locals only after definite assignment analysis proves use before assignment is impossible.

- Fields receive default values even without initializers (`0`, `null`, `false`).
- Locals must be assigned on every code path before read; the compiler error CS0165 catches violations.
- `const` and `readonly` require initializers (const at compile time, readonly by end of constructor).
- Initializers can call methods (`var now = DateTime.UtcNow;`) whereas `const` cannot use runtime values.

---

#### Q28. Can you use `const` with user-defined types like `DateTime` or `decimal` computed at runtime? Why or why not?

**Answer:** You cannot mark such values `const` because `const` requires a compile-time constant expression, and `DateTime.Now` or runtime-computed `decimal` values are evaluated when the program runs, not when the compiler builds the assembly.

- `const decimal Tax = 0.08m;` is valid because the literal is known at compile time.
- `readonly DateTime Created = DateTime.UtcNow;` or a static readonly field set in the static constructor is the correct pattern for runtime values.
- Attempting `const DateTime d = DateTime.Now;` produces compile error CS0133.
- See Gotcha 3 and Q9 for the const vs readonly decision table.

---

### 03. Input & Output

#### Q1. What is the difference between `Console.WriteLine`, `Console.Write`, and string interpolation for output?

**Answer:** `Console.WriteLine` prints text followed by the platform newline, `Console.Write` prints without advancing to the next line, and string interpolation (`$"{name}"`) builds the string in memory before passing it to either write method. Interpolation is a string composition technique, not a separate I/O channel.

- Use `WriteLine` for line-oriented user prompts and log-style output where each message should appear on its own row.
- Use `Write` when prompting on the same line as user input (`"Enter name: "` followed by `ReadLine`).
- Interpolation embeds expressions and format specifiers (`$"{price:C2}"`) and is usually clearer than concatenation with `+`.
- Both `Write` and `WriteLine` target `Console.Out` unless you redirect output.

---

#### Q2. What is the difference between `Console.ReadLine()` and `Console.ReadKey()`?

**Answer:** `Console.ReadLine()` blocks until the user presses Enter and returns the entire line as a `string` (without the newline), while `Console.ReadKey()` reads a single keystroke immediately (optionally intercepting it so it is not echoed) and returns a `ConsoleKeyInfo`.

- `ReadLine` suits free-text input such as names, addresses, or pasted tokens.
- `ReadKey(true)` suits "Press any key to continue" menus without requiring Enter.
- `ReadLine` can return `null` on stream end (Ctrl+Z on Windows console); handle null before parsing.
- `ReadKey` exposes whether Shift or Alt modifiers were held via `ConsoleKeyInfo`.

---

#### Q3. How do you safely parse user input (`int.TryParse`, `Parse`, `Convert`) and handle invalid input?

**Answer:** Prefer `TryParse` for interactive input because it returns `false` on failure without throwing, letting you reprompt instead of crashing. `Parse` and `Convert` throw exceptions on invalid input, which is acceptable only when bad input represents a programmer error, not a user typo.

- Pattern: loop until `int.TryParse(Console.ReadLine(), out int n)` succeeds, showing an error message on failure.
- Supply `NumberStyles` and `IFormatProvider` when input may include currency symbols or locale-specific separators.
- `Convert.ToInt32` accepts `object` and null handling rules differ slightly from `int.Parse`.
- See Gotcha 10 on production console apps vs `Parse` on unchecked user text.

---

#### Q4. How does formatted console output work (`Console.WriteLine("{0}", value)` vs interpolation)?

**Answer:** Composite formatting passes a format string with indexed placeholders `{0}`, `{1}` and a parameter list; the runtime calls `String.Format` semantics to substitute each hole. String interpolation (`$"{value}"`) is translated by the compiler into a similar `Format` call with a generated format string.

- Composite formatting supports reuse of the same index (`{0}` twice) and explicit ordering when arguments are computed expressions.
- Interpolation inlines expressions directly, which is easier to read for simple messages.
- Both support alignment and numeric formats: `{0,10:N2}` or `$"{value,10:N2}"`.
- Custom types can participate via `IFormattable.ToString(format, provider)`.

---

#### Q5. What is the difference between `CultureInfo.CurrentCulture`, `CurrentUICulture`, and `InvariantCulture`?

**Answer:** `CurrentCulture` drives formatting and parsing of numbers, dates, and currency for user-facing display. `CurrentUICulture` selects localized resource strings (satellite assemblies). `InvariantCulture` is a fixed, culture-neutral English-like format used when data must round-trip regardless of user locale.

| Property | Purpose |
|---|---|
| `CurrentCulture` | Number/date/currency format and parse for display |
| `CurrentUICulture` | Localized UI strings from `.resx` files |
| `InvariantCulture` | Stable format for logs, protocols, and file formats |

Changing `CurrentCulture` affects `ToString()` on numbers and dates when no explicit provider is passed.

---

#### Q6. When should you use `InvariantCulture` for formatting numbers and dates instead of `CurrentCulture`?

**Answer:** Use `InvariantCulture` whenever the string is stored, transmitted, or parsed later in a different locale—logs, JSON, CSV, query strings, and file names—because `CurrentCulture` varies by machine and user settings.

- User-facing labels in a GUI may use `CurrentCulture` for friendly dates, but API payloads should not.
- Explicit `culture` parameters override the thread default and document intent at the call site.
- Mixing cultures between write and read causes subtle bugs (`"1,234.56"` vs `"1.234,56"`).
- See Gotcha 9 for parse failures when comma is treated as thousands separator vs decimal point.

---

#### Q7. How do culture settings affect decimal separators, currency symbols, and date formats in console output?

**Answer:** Formatting APIs consult the culture's `NumberFormatInfo` and `DateTimeFormatInfo`, so the same `double` or `DateTime` prints differently under `en-US`, `de-DE`, or `fr-FR`. Currency format adds symbol placement and group separators defined by that culture.

- `3.14.ToString()` in `de-DE` may display `3,14` while `en-US` shows `3.14`.
- Short dates might be `MM/dd/yyyy` vs `dd.MM.yyyy`, affecting user confusion in console apps.
- `Console` output does not auto-adapt unless the thread culture is set or you pass a format provider.
- Testing console apps should set culture explicitly or use invariant formatting for deterministic assertions.

---

#### Q8. What is composite formatting (`string.Format`, `{0:N2}`, alignment `{0,10}`, `{0,-10}`)?

**Answer:** Composite formatting replaces indexed placeholders in a template string with formatted argument values, optionally specifying width, alignment, and numeric or date format strings after a colon.

- `{0:N2}` formats argument zero as a number with two decimal places and group separators per the provider.
- `{0,10}` right-aligns in width 10; `{0,-10}` left-aligns with padding spaces by default.
- `string.Format(provider, format, args)` and `Console.WriteLine(format, args)` share the same rules.
- Custom formats (`"P"`, `"C"`, `"yyyy-MM-dd"`) map to standard or type-specific format strings documented for each type.

---

#### Q9. What is the difference between `Console.InputEncoding` and `Console.OutputEncoding`, and why can mismatched encodings garble console text?

**Answer:** `Console.InputEncoding` and `Console.OutputEncoding` control how bytes from the console device map to .NET characters on read and write respectively. If the console code page or terminal encoding does not match the Unicode text you emit, characters outside that repertoire display as `?` or mojibake.

- Windows consoles historically defaulted to OEM or ANSI code pages; UTF-8 output requires matching console and `OutputEncoding`.
- Reading UTF-8 bytes with a Latin-1 interpretation corrupts multi-byte sequences.
- Modern .NET templates often set UTF-8 in project or host configuration; legacy environments may still need explicit setup.
- See Gotcha 15 on Unicode replacement characters when encodings disagree.

---

#### Q10. How do you capture console output programmatically (e.g., `StringWriter` redirected to `Console.SetOut`)?

**Answer:** Replace `Console.Out` with a `TextWriter` such as `StringWriter` via `Console.SetOut`, run code under test that writes to `Console`, then read the accumulated text from the writer. Restore the original writer in a `finally` block so later tests are not affected.

- Save `var original = Console.Out;` before redirecting and call `Console.SetOut(original)` in `finally`.
- `StringWriter.ToString()` returns all captured output after the exercise under test completes.
- The same pattern works for `Console.SetError` with a separate `StringWriter` for stderr.
- Integration tests use this to assert CLI tools print expected messages without spawning a visible console.

---

#### Q11. What is the difference between `Console.Read` and `Console.ReadLine`?

**Answer:** `Console.Read()` returns the next character as an `int` code unit (or `-1` at end of stream), blocking until one character is available, while `ReadLine()` reads until Enter and returns a full `string` including all characters typed on that line.

- `Read` is low-level and rarely used in application code except single-key scenarios without `ReadKey` features.
- `ReadLine` is the standard choice for textual user input in tutorials and simple CLIs.
- Neither trims whitespace unless you call `Trim()` on the returned string.
- `Read` does not echo behavior differences beyond platform defaults; `ReadKey` offers more control over echo and intercept.

---

#### Q12. How do you format output with alignment and padding using interpolation (`$"{value,10}"`, `$"{value:N2}"`)?

**Answer:** Interpolation supports the same alignment and format syntax as composite formatting inside the braces: a comma and width for padding, a colon and format string for numeric or date patterns.

- `$"{name,-20}"` left-aligns `name` in 20 columns, useful for columnar console tables.
- `$"{amount:N2}"` prints two decimal places with culture-aware group separators unless you pass `CultureInfo.InvariantCulture`.
- Multiple holes can mix formats: `$"{id,5} {desc,-30} {price,10:C}"`.
- Constant alignment values compile efficiently; complex expressions inside `{...}` are evaluated before formatting.

---

#### Q13. What is `IFormattable`, and how does it relate to custom formatting in `ToString(format, provider)`?

**Answer:** `IFormattable` declares `ToString(string? format, IFormatProvider? formatProvider)`, allowing types to interpret custom and standard format strings when used in composite formatting or interpolation. The runtime calls this interface when formatting an object with a non-null format specifier.

- Implement both `ToString()` and `ToString(format, provider)` consistently for public types.
- If a type lacks `IFormattable`, unknown format strings may be ignored in `ToString(format)` overrides on `object`.
- `FormattableString` and `FormattableStringFactory` expose interpolation holes for culture-invariant logging scenarios.
- Currency and numeric BCL types implement rich format support (`"C"`, `"X"`, `"E"`).

---

#### Q14. What happens if you call `int.Parse` on invalid input vs `int.TryParse` — which pattern is preferred in production console apps?

**Answer:** `int.Parse` throws `FormatException` (or `OverflowException`) when the text is not a valid integer, terminating the flow unless caught. `int.TryParse` returns `false` and sets the out parameter to default, which fits expected user input mistakes in interactive loops.

- Production console apps should treat bad input as a normal branch, not an exceptional one—see Gotcha 10.
- `Parse` remains fine for trusted configuration files validated at startup with clear error handling.
- Always consider culture: parsing `"1.234"` depends on whether `.` is decimal or thousands separator.
- Wrap parse failures with user-visible messages and reprompt rather than stack traces.

---

#### Q15. How does changing `CultureInfo.CurrentCulture` on the current thread affect subsequent formatting calls that omit an explicit provider?

**Answer:** Formatting methods without an explicit `IFormatProvider` use `CultureInfo.CurrentCulture` on the executing thread, so changing it mid-process changes how numbers, dates, and currency render from that point forward on that thread.

- ASP.NET and modern hosts often set culture per request from headers; console apps inherit OS user settings by default.
- Async continuations may flow culture depending on configuration; explicit providers avoid surprises.
- Libraries should not mutate global culture silently; accept `IFormatProvider` parameters instead.
- Tests set `CultureInfo.CurrentCulture` to invariant or a fixed culture in setup for deterministic output.

---

#### Q16. What is `NumberFormatInfo`, and how does it differ from `CultureInfo`?

**Answer:** `NumberFormatInfo` holds the specific rules for decimal separators, group sizes, negative patterns, and percent symbols, while `CultureInfo` is the broader culture object that exposes `NumberFormat`, `DateTimeFormat`, and other regional settings together.

- Access via `CultureInfo.CurrentCulture.NumberFormat` or `CultureInfo.InvariantCulture.NumberFormat`.
- Clone and modify `NumberFormatInfo` for custom numeric displays without creating a full custom culture.
- `CultureInfo` is the usual entry point; `NumberFormatInfo` is the detailed knob for number layout only.
- Parsing methods accept `NumberStyles` combined with a format provider derived from culture.

---

#### Q17. When reading numeric input from users in different locales, what pitfalls arise with comma vs period decimal separators?

**Answer:** Users type decimals according to local convention, but `TryParse` without a culture may interpret commas as thousands separators or decimal points inconsistently, producing wrong values or parse failures when the thread culture does not match the user's expectation.

- `"3,14"` is 3.14 in `de-DE` but might fail or misparse under `en-US` rules.
- Always document expected format in prompts or parse with an explicit culture matching the UI language.
- For machine-readable input, instruct users to use invariant format or accept both with custom parsing logic.
- See Gotcha 9 and Q6 on invariant culture for stored data vs localized input.

---

#### Q18. What is the purpose of `Console.ForegroundColor`, `BackgroundColor`, and resetting colors after use?

**Answer:** These properties change the console's text and background colors for subsequent output, highlighting errors, success, or sections in CLI tools. Resetting to `Console.ResetColor()` afterward prevents later unrelated output from inheriting unintended colors.

- Color support depends on terminal capabilities; Windows Terminal and modern consoles support richer palettes than legacy hosts.
- Wrap color changes in `try/finally` to restore defaults even when exceptions occur.
- Overusing color reduces accessibility; pair with explicit labels, not color alone, for errors.
- Libraries logging to shared consoles should avoid setting colors unless configured, to respect host themes.

---

### 04. Operators & Expressions

#### Q1. What are the different types of operators in C#? (Arithmetic, Relational, Logical, Bitwise, Assignment, Ternary, Null-coalescing, etc.)

**Answer:** C# groups operators by purpose: arithmetic (`+`, `-`, `*`, `/`, `%`), relational (`==`, `!=`, `<`, `>`), logical (`&&`, `||`, `!`), bitwise (`&`, `|`, `^`, `~`, shifts), assignment (`=`, `+=`, compound forms), conditional (`?:`), null operators (`?.`, `??`, `??=`), and others such as `is`, `as`, `sizeof`, and `nameof`.

- Operator precedence determines evaluation order when parentheses are omitted; unary before multiplicative before additive before relational before logical AND before OR.
- Overloaded operators apply to user-defined types when declared with `public static` signatures matching language rules.
- Some operators behave differently on floating-point vs integer types (division, remainder).
- Null-conditional and null-coalescing operators integrate with nullable reference and value type analysis.

---

#### Q2. Explain the `checked` and `unchecked` keywords in C#.

**Answer:** `checked` context causes arithmetic overflow on integral types to throw `OverflowException`, while `unchecked` (the default in most projects) silently wraps overflow using two's complement truncation. You can scope contexts with `checked { ... }` blocks or enable project-wide checked arithmetic.

- Financial and checksum code often enables `checked` for early failure instead of silent wraparound.
- `unchecked` is explicit when you rely on wrap semantics (hash mixing, low-level algorithms).
- Overflow does not apply to floating-point types—they produce infinity or NaN instead.
- See Gotcha 14 on default unchecked integer behavior.

---

#### Q3. What is the difference between `==` and `.Equals()` for value types vs reference types?

**Answer:** For value types, `==` compares values when overloaded or compares bitwise equality for primitives; `.Equals` typically matches value semantics. For reference types, `==` may use reference equality unless overloaded (as `string` does), while `.Equals` may be overridden for logical equality.

- Default reference equality: `==` and `ReferenceEquals` align unless the type overloads `==`.
- Structs should implement `IEquatable<T>` and consistent `GetHashCode` when used in collections.
- `Equals(object?)` accepts null; `==` between reference types is false if either side is null (unless both null).
- See Q14 for `ReferenceEquals` specifically.

---

#### Q4. What is integer division in C#, and how do you get a fractional result?

**Answer:** When both operands of `/` are integral types, C# performs integer division, truncating toward zero and discarding any fractional part. At least one operand must be floating-point (or cast) to obtain a fractional result.

- `10 / 3` yields `3`, not `3.333…`—see Gotcha 2.
- `10 / 3.0` or `10.0 / 3` promotes to `double` and yields approximately `3.333…`.
- `%` returns the remainder with the sign of the dividend: `-10 % 3` is `-1`.
- Use `decimal` division for money after promoting operands to `decimal`.

---

#### Q5. Explain operator precedence and associativity — why does `a + b * c` evaluate differently than `(a + b) * c`?

**Answer:** Operator precedence ranks multiplication above addition, so `a + b * c` computes `b * c` first then adds `a`. Associativity rules break ties at the same precedence level—most binary operators associate left-to-right, assignment and null-coalescing associate right-to-left.

- Parentheses override precedence explicitly and improve readability even when not strictly required.
- Misunderstanding precedence causes subtle bugs in compound conditions and arithmetic without parentheses.
- The language specification defines a complete table; IDE tooling parenthesizes subexpressions in tooltips when debugging.
- Ternary `?:` has lower precedence than most operators, which can surprise without parentheses around the condition.

---

#### Q6. What is the difference between prefix and postfix increment (`++i` vs `i++`)?

**Answer:** Prefix increment (`++i`) adds one and returns the new value; postfix increment (`i++`) returns the original value then adds one. Both mutate the variable when applied to a mutable l-value.

- In standalone statements (`i++;` as its own line), the difference is invisible.
- In expressions like `array[i++]`, postfix uses the old index then advances; prefix advances first.
- Increment on value type properties that return copies does not compile unless the property returns by ref (rare).
- Overflow follows integral checked/unchecked context like other arithmetic.

---

#### Q7. What are short-circuit logical operators (`&&`, `||`), and why do they matter beyond boolean logic?

**Answer:** `&&` and `||` evaluate the right operand only when necessary—`&&` skips the right side if the left is false, `||` skips if the left is true. This enables safe null checks and avoids side effects or expensive calls when the outcome is already determined.

- `if (obj != null && obj.Count > 0)` avoids `NullReferenceException` because `Count` is not evaluated when `obj` is null.
- Non-short-circuit `&` and `|` on `bool` always evaluate both sides—rarely needed except for bitwise booleans.
- Short-circuit behavior interacts with nullable booleans in lifted operators differently from strict evaluation.
- Side effects in the right operand must be understood as conditionally executed.

---

#### Q8. Explain the null-conditional operator (`?.`) and null-coalescing operators (`??`, `??=`).

**Answer:** The null-conditional operator `?.` accesses a member or indexer only when the receiver is not null, producing null for reference results or `Nullable<T>` for value results instead of throwing. `??` and `??=` supply defaults when an expression is null.

- `customer?.Address?.City` short-circuits at the first null in the chain.
- `?.` combined with invocation: `handler?.Invoke()` calls only if delegate is not null.
- `??=` lazily initializes fields and locals—see Q19 in Data Types.
- Null-conditional assignment extensions (`?.=`) appear in newer language versions for compound null-aware assignment.

---

#### Q9. What are bitwise operators (`&`, `|`, `^`, `~`, `<<`, `>>`), and when are they used in application code?

**Answer:** Bitwise operators manipulate individual bits of integral types: AND masks bits, OR sets bits, XOR toggles, NOT inverts, and shifts move bit patterns left or right. They appear in flags enums, permissions masks, hashing, compression, and low-level protocols.

- `[Flags]` enums combine values with `|` and test with `&`: `(mode & FileMode.Read) != 0`.
- Unsigned right shift `>>>` (C# 11+) fills with zeros regardless of sign for `int`/`uint`.
- Do not confuse bitwise `&` on integers with logical `&&` on booleans—they are different operators.
- Application business logic rarely needs bitwise ops unless modeling packed flags or interfacing with hardware formats.

---

#### Q10. What is the difference between logical AND (`&&`) and bitwise AND (`&`) when applied to `bool` operands?

**Answer:** Both combine boolean values, but `&&` short-circuits and `&` always evaluates both operands. For pure boolean logic with possible null checks or side effects on the right, `&&` is the default choice.

- `true & Foo()` always calls `Foo()`; `true && Foo()` still calls `Foo()` because the left is true—only false left skips right.
- Bitwise `&` on integers is unrelated to boolean `&&` despite similar symbols.
- Nullable bools use lifted operators with `&` and `|` producing null when either operand is null in some cases.
- Use `&` on bools only when both evaluations are required for correctness.

---

#### Q11. What is the ternary conditional operator (`?:`), and how does it differ from an `if/else` statement?

**Answer:** The ternary operator `condition ? whenTrue : whenFalse` is an expression that yields a value, while `if/else` is a statement controlling blocks of arbitrary size. Ternary fits simple choice-of-value scenarios; statements fit multi-line logic and void actions.

- Both branches must be type-compatible enough for the compiler to infer a common type.
- Nested ternaries reduce readability; prefer `if/else` or switch expressions for many branches.
- Ternary is evaluated eagerly for the chosen branch only—like `if/else`, not lazy like some functional forms.
- See Control Flow Q1 for stylistic guidance vs `if/else`.

---

#### Q12. What is the difference between `is` pattern matching and a simple boolean expression in a condition?

**Answer:** The `is` operator tests runtime type and can introduce a typed variable in the same expression (`if (obj is Order o)`), combining type check, cast, and assignment. A simple boolean might call a method but does not bind a new strongly typed variable without a separate cast.

- Pattern forms include constant, relational, property, and recursive patterns in modern C#.
- `is not null` integrates with nullable flow analysis better than `!= null` in some analyzer versions.
- Type patterns fail closed when the object is null unless you use `is null` or null checks first.
- See Control Flow Q14 for `if` vs `switch` pattern usage.

---

#### Q13. When does overflow occur for integer arithmetic, and how do `checked` blocks change behavior?

**Answer:** Overflow occurs when an integral operation's mathematical result lies outside the representable range of the type, such as `int.MaxValue + 1`. In unchecked context the value wraps; in checked context the runtime throws `OverflowException` at the overflowing operation.

- Literals and constant folding may detect overflow at compile time in checked context.
- Casting a too-large constant to a smaller type can overflow at compile time with error CS0221 in checked context.
- See Q2 and Gotcha 14 for project-level checked settings.
- `decimal` throws `OverflowException` on overflow rather than wrapping silently.

---

#### Q14. What is the difference between `==` and `ReferenceEquals` for reference types?

**Answer:** `ReferenceEquals(a, b)` always compares object identity—whether both refer to the exact same heap instance—ignoring any `==` overload on the type. `==` may delegate to an overloaded operator (strings compare by value) or default reference equality.

- Use `ReferenceEquals` when you intentionally need identity semantics despite value-based `==` overloads.
- Two distinct string objects with the same content may be `==` true but `ReferenceEquals` false unless interned.
- Value types boxed to object compare references when using `ReferenceEquals` on the boxed instances.
- See Strings chapter on interning and equality.

---

#### Q15. Can you overload operators in C# — which operators can and cannot be overloaded?

**Answer:** C# allows overloading many unary and binary operators (`+`, `-`, `==`, `!=`, implicit/explicit conversions, etc.) as static methods on the declaring type, but cannot overload operator precedence, the ternary operator, `&&`, `||`, or assignment operators like `=` and `+=` directly.

- Comparison operators `==` and `!=` must be overloaded in pairs; `true`/`false` unary operators support custom boolean logic types.
- Conversion operators must be `implicit` or `explicit` and follow clear semantics to avoid surprising casts.
- Overloads should mirror intuitive meaning; abusing `+` for unrelated operations harms readability.
- `Equals`, `GetHashCode`, and `==` should stay consistent for types used as keys.

---

#### Q16. What is the difference between compound assignment (`+=`, `-=`) and the expanded form (`x = x + y`) for value vs reference types?

**Answer:** For value types, `x += y` mutates `x` in place when `x` is a mutable variable. For reference types, `+=` on references reassigns the variable when the operator returns a new reference (strings), while mutating methods on objects (`list.Add`) change the object without reassigning the variable.

- `string s = s + "a"` and `s += "a"` both create new string instances because strings are immutable.
- `list += item` is not valid unless overloaded; use `list.Add` for mutation.
- Compound assignment evaluates the left side once, which matters if the left is a property or indexer with side effects.
- See Methods chapter on ref parameters when reassignment must affect the caller's variable.

---

#### Q17. What is the `nameof` operator, and how is it used in validation messages and refactoring-safe code?

**Answer:** `nameof` returns the unqualified identifier name of a variable, type, or member as a compile-time string without runtime reflection cost. Renaming the symbol updates `nameof` automatically, making it ideal for argument exception parameters and property change notifications.

- `throw new ArgumentNullException(nameof(customer));` stays correct if the parameter is renamed.
- `nameof` does not include namespace or full dotted paths—only the final token (`nameof(Foo.Bar)` is `"Bar"`).
- It avoids magic strings that drift from code during refactorings.
- Unlike string literals, `nameof` is resolved at compile time and has zero runtime overhead.

---

### 05. Type Conversion & Casting

#### Q1. What is the difference between the `is` and `as` operators?

**Answer:** `is` tests whether an object is compatible with a type and, with pattern matching, binds a variable when the test succeeds. `as` attempts a cast and returns `null` on failure for reference types without throwing.

- `if (obj is Customer c)` combines check and assignment in one step.
- `var c = obj as Customer; if (c != null)` is the legacy pattern before pattern matching.
- `as` does not work on non-nullable value types directly; use `is` with patterns or explicit casts for value types.
- Failed `as` returns null, which can hide bugs if you forget the null check.

---

#### Q2. What is the difference between implicit and explicit type conversion (casting)?

**Answer:** Implicit conversions compile without a cast when the conversion is guaranteed safe (widening numerics, derived to base reference). Explicit conversions require a cast operator when data might be lost or the relationship is not automatically safe (narrowing numerics, base to derived references).

- Widening (`int` to `long`) is implicit; narrowing (`long` to `int`) needs `(int)value` and may overflow at runtime.
- User-defined types can declare `implicit` or `explicit` conversion operators with clear semantics.
- Explicit reference casts throw `InvalidCastException` when the object is not actually of the target type.
- See Q5 on widening vs narrowing direction.

---

#### Q3. What is the difference between `Convert.ToInt32`, `(int)`, and `int.Parse`?

**Answer:** `(int)` is a direct cast requiring a compatible numeric type at compile time or an unboxing cast at runtime. `int.Parse` parses a string representation. `Convert.ToInt32` accepts broader inputs (`object`, strings, other numerics) with generalized conversion rules and uses `IConvertible`.

- `Parse` and `Convert` on strings throw on invalid text; prefer `TryParse` for user input.
- `Convert.ToInt32(3.9)` rounds to nearest integer (banker's rounding rules apply); `(int)3.9` truncates toward zero.
- `Convert` handles null for nullable types differently than `Parse`.
- Choose the narrowest API: cast for numeric narrowing, `TryParse` for strings, `Convert` for heterogeneous legacy APIs.

---

#### Q4. When does a cast succeed at compile time but fail at runtime?

**Answer:** Reference downcasts compile when the static type is a base type but fail at runtime if the object is not actually an instance of the target derived type. Unboxing casts compile when the static type is `object` but fail if the boxed type does not match exactly.

- `(Derived)baseRef` compiles if `baseRef` is typed as `Base` but throws if it points to another derived type.
- `(int)obj` throws `InvalidCastException` if `obj` boxes a `double`, not an `int`.
- Pattern matching with `is` avoids exceptions by testing first.
- Array covariance also compiles unsafe assignments that fail at runtime—see Gotcha 8.

---

#### Q5. What is widening vs narrowing conversion — which direction is implicit?

**Answer:** Widening conversions move to a type that can represent all values of the source without loss (for example `byte` to `int`), and C# allows them implicitly. Narrowing conversions move to a smaller range or lower precision and require explicit casts because values may be truncated or rounded.

- Floating to `decimal` is explicit because not all binary floats map exactly.
- `int` to `long` is implicit widening; `long` to `int` is explicit narrowing.
- Constant expressions may be implicitly converted when the value fits the target type even if narrowing in general.
- See Q3 for truncation vs rounding on numeric conversions.

---

#### Q6. What is the difference between `Parse`, `TryParse`, and `Convert.ChangeType`?

**Answer:** `Parse` converts strings to target types and throws on failure. `TryParse` returns a boolean and an out result without throwing for expected bad input. `Convert.ChangeType` generalizes conversion between many types via `IConvertible`, often used in reflection-based scenarios.

- `TryParse` is preferred for interactive and protocol parsing where failure is normal.
- `ChangeType` returns `object` requiring a cast and throws `InvalidCastException` when no conversion exists.
- All string parsing should specify culture when format is locale-dependent.
- Nullable value types need `TryParse` overloads or parse the underlying type then assign.

---

#### Q7. When would you use the `is` pattern with a declaration (`if (obj is int n)`) vs a traditional cast?

**Answer:** Use the declaration pattern when you need both a type test and a typed variable in the true branch without a separate cast that the compiler cannot prove safe. Traditional casts `(int)obj` are acceptable when you already verified type or will catch exceptions.

- Declaration patterns integrate with nullable analysis and switch expressions cleanly.
- Repeated `(Target)obj` casts duplicate noise and skip flow analysis benefits.
- Traditional casts fail fast with exceptions—sometimes desired in trusted internal code paths.
- Switch expressions on type patterns replace long if-else chains—see Control Flow chapter.

---

#### Q8. What exception types are commonly thrown by failed casts and parses (`FormatException`, `OverflowException`, `InvalidCastException`)?

**Answer:** `FormatException` indicates the string is not in the expected format for parse methods. `OverflowException` occurs when a numeric parse or checked arithmetic exceeds the target range. `InvalidCastException` signals an incompatible cast or unboxing operation at runtime.

- `int.Parse("abc")` throws `FormatException`; `int.Parse("999999999999999999999")` may throw `OverflowException`.
- Unboxing wrong types throws `InvalidCastException`.
- Catch specific types when recovering; let unexpected failures propagate in most application layers.
- `TryParse` avoids all three for expected failure paths on strings.

---

#### Q9. What is `TryFormat`, and how does writing into a `Span<char>` differ from calling `ToString()`?

**Answer:** `TryFormat` attempts to write a formatted representation into a caller-provided `Span<char>` buffer, returning whether the buffer was large enough. This avoids allocating a new `string` on the heap, which `ToString()` always does.

- Used internally by high-performance formatting and available on many primitive types.
- If the span is too small, `TryFormat` returns false and you can retry with a larger buffer.
- Span-based formatting fits stackalloc buffers and UTF-8 encoding pipelines in modern APIs.
- For simple logging, `ToString()` readability often outweighs micro-optimization unless profiling shows hot paths.

---

#### Q10. How does culture affect parsing and formatting during type conversion (e.g., `"1,234.56"` vs `"1.234,56"`)?

**Answer:** Parse and format methods use the supplied `IFormatProvider` or `CurrentCulture` to decide whether comma or period is the decimal separator and how groups are laid out, so the same literal characters mean different numbers in different cultures.

- Always pass `CultureInfo.InvariantCulture` for wire formats and logs.
- User input should parse with the culture matching how you prompted the user to type data.
- Mis-specified culture causes off-by orders-of-magnitude bugs in financial imports.
- See Input & Output Q5–Q7 and Gotcha 9.

---

#### Q11. What is the difference between boxing during conversion to `object` and a direct numeric cast?

**Answer:** Assigning a value type to `object` boxes it—allocating a heap wrapper—while casting between numeric types (`(int)d`) converts the value directly without creating an object wrapper when both sides are known numeric types.

- `(object)42` boxes; `(int)42.0` truncates a double without boxing.
- Unboxing from `object` back to a value type requires an exact type match.
- Generic collections avoid boxing for value types; `ArrayList` boxed every `int`.
- See Data Types Q4 and Q14 on performance impact.

---

#### Q12. When is the `as` operator preferred over a cast, and what does it return on failure?

**Answer:** Prefer `as` when casting down a reference hierarchy where failure is an expected outcome and you will branch on null, avoiding exception cost. On failure, `as` returns `null` for reference types instead of throwing `InvalidCastException`.

- Do not use `as` with value types except nullable scenarios; use `is` patterns instead in modern code.
- After `as`, always null-check before dereferencing.
- When failure should abort processing, an explicit cast or pattern match documents intent more clearly.
- Legacy codebases mix `as` with null checks; new code favors `is` patterns.

---

#### Q13. What is user-defined explicit/implicit conversion operator syntax (preview level)?

**Answer:** Types can declare `public static implicit operator TargetType(SourceType s)` or `explicit operator TargetType(SourceType s)` to allow the compiler to convert between types with defined semantics, subject to language rules requiring paired safety documentation in API design.

- Implicit conversions should be obviously safe; explicit conversions signal possible information loss.
- They participate in overload resolution like built-in conversions when applicable.
- Abuse creates hidden conversions that confuse readers—reserve for domain types with clear mappings (units, identifiers).
- They do not replace good constructor or factory methods when conversion is not natural.

---

#### Q14. What happens when you cast a `double` to `int` — is rounding or truncation applied?

**Answer:** Casting floating-point to integral types truncates toward zero, dropping the fractional part without rounding to nearest. `3.9` and `-3.9` cast to `3` and `-3` respectively.

- `Convert.ToInt32(3.9)` rounds to nearest even in default mode—different from cast truncation.
- `Math.Floor`, `Ceiling`, and `Round` express explicit rounding before casting when business rules require it.
- Overflow still possible if double magnitude exceeds int range—unchecked cast wraps in unchecked context.
- See Q3 for API differences between cast and `Convert`.

---

#### Q15. What is the difference between `default(T)` casting patterns and `Convert` methods for nullable value types?

**Answer:** `default(Nullable<int>)` is null without value, while converting null with `Convert` may yield zero for value types depending on overload. Parsing empty strings differs between `TryParse` (false) and `Convert` behaviors.

- Use `TryParse` for optional form fields representing absent numbers with `int?`.
- `Convert.ChangeType` on boxed null often returns null for nullable target types when configured appropriately.
- Keep one consistent strategy per API layer to avoid null vs zero ambiguity.
- Document whether absent numeric input maps to null or zero for consumers.

---

#### Q16. When converting between `string` and numeric types in APIs and logs, why is `InvariantCulture` often specified explicitly?

**Answer:** Explicit invariant culture makes serialized numbers and dates identical on every machine, so downstream parsers, diff tools, and aggregators do not misread separators when the server's locale differs from the developer's workstation.

- Logs consumed globally should not flip decimal commas based on OS language.
- REST and JSON often use invariant-like formats even though JSON numbers typically avoid locale separators.
- Unit tests assert expected strings without setting thread culture when invariant is specified at the call site.
- See Q10 and Input & Output culture questions for the full picture.

---

### 06. Control Flow & Loops

#### Q1. What is the difference between `if/else` and the ternary operator?

**Answer:** `if/else` is a statement that executes one of two blocks of arbitrary size and may perform multiple actions, while the ternary operator is a single expression that selects between two values. Use ternary for simple assignments; use `if/else` for multi-statement branches and side effects.

- Ternary requires both branches to be expressions compatible enough for type inference.
- Nested ternaries harm readability compared to `if/else` chains or switch expressions.
- See Operators Q11 for evaluation semantics.
- Style guides often limit ternary to one line of choice between two values.

---

#### Q2. What is the difference between traditional `switch` and switch expressions (C# 8+)?

**Answer:** Traditional `switch` is a statement with `case` labels, optional `break`, and fall-through restrictions, while switch expressions (`var result = x switch { ... }`) produce a value with expression-bodied arms separated by commas and use exhaustive pattern matching rules.

- Switch expressions discourage fall-through bugs by requiring `=>` arms and no implicit fall-through.
- Both support type, constant, and relational patterns in modern C#.
- Switch expressions must cover all inputs or include a discard `_` pattern when exhaustive.
- Prefer switch expressions for mapping enums to labels; use statements when cases need multiple statements without local functions.

---

#### Q3. When should you use `for`, `foreach`, `while`, and `do-while`?

**Answer:** Use `for` when you need an index counter and known iteration bounds, `foreach` to enumerate `IEnumerable` sequences without manual indexing, `while` when the loop condition is tested before each iteration, and `do-while` when the body must run at least once before the condition is checked.

- `foreach` is idiomatic for collections and LINQ-friendly sequences; it hides enumerator disposal via compiler-generated try/finally.
- `for` suits arrays when you need the index for parallel arrays or reverse iteration.
- `while` fits polling and read-until-done loops with unknown iteration count.
- `do-while` fits menu loops that display once before validating exit condition.

---

#### Q4. What is the difference between `break`, `continue`, and `return` inside a loop?

**Answer:** `break` exits the innermost enclosing loop or switch immediately, `continue` skips to the next iteration of the innermost loop, and `return` exits the entire method (after running any enclosing `finally` blocks) regardless of loop nesting.

- `break` in nested loops does not exit outer loops unless you use labeled break (rare) or refactor.
- `continue` re-evaluates the loop condition before the next body execution.
- `return` inside `try` still executes `finally` before the method actually returns—see Q15 and Gotcha 7.
- Misusing `break` vs `return` in search loops changes whether cleanup after the loop runs.

---

#### Q5. What are common pitfalls with nested loops and loop variable scope?

**Answer:** Nested loops multiply complexity and can hide O(n²) performance; reusing the same index variable name in inner loops shadows outer variables and confuses readers. Closure capture of loop variables in lambdas behaved differently before C# 5 for `foreach` vs `for`.

- Prefer extracting inner loops to methods when depth exceeds two levels with non-trivial logic.
- `for` loop variables are scoped to the loop; declaring the same name in nested `for` headers is legal but error-prone.
- See Q10 on foreach closure semantics in older mental models vs fixed foreach iteration variable per iteration.
- Off-by-one errors at inner boundaries often cause skipped or duplicate processing in matrices.

---

#### Q6. What is a switch expression, and how do relational and property patterns work in `switch`?

**Answer:** Switch expressions map input patterns to result expressions using `=>` arms, supporting constant patterns, type patterns, relational guards (`when`), property patterns (`{ Length: > 0 }`), and positional patterns on tuples and records.

- Relational patterns combine with constants: `var label = score switch { >= 90 => "A", >= 80 => "B", _ => "C" };`.
- Property patterns deconstruct shape: `obj switch { { Status: OrderStatus.Shipped } => true, _ => false }`.
- The compiler warns when not all enum values are handled if exhaustive analysis applies.
- Switch expressions are expression-oriented—each arm must yield a compatible type.

---

#### Q7. What is the difference between `break` in a `switch` vs `break` in a loop?

**Answer:** In both constructs `break` exits only the innermost enclosing switch or loop—it does not exit nested switches inside loops beyond one level. In modern switch expressions, `break` is replaced by arm separation without fall-through.

- Classic switch requires `break` (or `return`/`goto case`) at the end of each case unless the case ends with a jump.
- Accidentally omitting `break` in classic switch caused fall-through bugs before C# disallowed unreachable fall-through in many cases.
- `break` inside a case nested within a loop exits the switch, not the loop.
- Use `return` from a method or refactor when you need to exit both switch and loop.

---

#### Q8. When is `goto` still used in C# (e.g., `goto case`, `goto default`), and why is it generally discouraged?

**Answer:** `goto case` and `goto default` jump to another switch label when sharing logic between cases without duplicating code. General `goto` labels are discouraged because unstructured jumps make control flow hard to follow and refactor compared to loops, methods, and structured switches.

- `goto case` appears when one case falls through intentionally to shared cleanup in legacy switch statements.
- Switch expressions and local functions largely eliminate the need for arbitrary labels.
- Acceptable in generated code or performance-critical state machines; rare in application business logic.
- Excessive `goto` correlates with maintenance defects in large methods.

---

#### Q9. What is the scope of a variable declared in the initializer of a `for` loop (C# rules)?

**Answer:** A variable declared in the `for` loop initializer is scoped to the entire `for` statement and is not visible after the loop ends. Each `for` statement creates its own scope for that variable.

- You cannot reference the loop variable after the loop if it was declared in the initializer: `for (int i = 0; ...)` then `i` is out of scope.
- Declare the variable outside the loop if you need the final index value after completion.
- C# disallows assigning to the foreach iteration variable—different rule from `for` index mutation.
- Nested loops can each declare `int i` in separate `for` headers in separate scopes.

---

#### Q10. Why did C# 5 change loop variable capture semantics in lambdas, and how does that affect `foreach` vs `for`?

**Answer:** Before C# 5, a lambda closing over a `foreach` iteration variable captured the single shared variable, so all delegates saw the final value after the loop. C# 5 changed `foreach` to capture each iteration's value separately; `for` loop index capture still closes over one mutable variable unless you copy to a local inside the loop.

- Bug pattern: tasks created in `foreach` all observing the last item before the fix.
- Inside `for`, copy `int copy = i;` before async lambdas when you need per-iteration capture with mutable index semantics.
- Understanding this prevents subtle parallel and async bugs in LINQ and Task loops.
- Modern code often uses `Select((item, index) => ...)` to avoid manual capture issues.

---

#### Q11. Can you modify the collection you are iterating in a `foreach` loop — what exception results?

**Answer:** You cannot add or remove elements from most collections during `foreach` enumeration; doing so throws `InvalidOperationException` with message that collection was modified. Mutating elements in place may be allowed for some collection types but is risky if it changes structure.

- Use `for` backward over indices or build a new collection when removing items during iteration.
- `List<T>` documents that structural changes during enumeration invalidate the enumerator.
- Concurrent modification from another thread causes the same exception on many collection types.
- LINQ deferred queries may re-enumerate underlying collections that changed between operations.

---

#### Q12. What is the difference between `while` and `do-while` when the condition is false on the first check?

**Answer:** `while` evaluates the condition before the first iteration and may skip the body entirely if the condition is initially false. `do-while` always executes the body at least once before evaluating the condition at the bottom.

- Use `do-while` for input validation menus that must prompt at least once.
- Both support `break` and `continue` with the same innermost-loop rules.
- Infinite loops use `while (true)` with internal `break` when exit logic is complex.
- Condition side effects run zero times in `while` if initially false, once before check in `do-while` after first body run.

---

#### Q13. When would you prefer a `switch` over a chain of `if/else if` statements?

**Answer:** Prefer `switch` when discriminating a single expression against many constant or pattern values, especially enums, because switch expressions and statements communicate intent and enable compiler exhaustiveness checks. Long unrelated boolean conditions remain clearer as `if/else`.

- Switch on strings and enums is efficient and readable with modern pattern support.
- Switch expressions reduce temporary variables when mapping inputs to outputs.
- `if/else` fits disparate conditions that are not variations of one expression's shape.
- Performance differences are usually negligible; readability and correctness matter more.

---

#### Q14. What is pattern matching with `is` in an `if` statement vs a `switch` on type?

**Answer:** `if (obj is Type t)` handles one or two type tests inline, while `switch (obj)` with multiple type patterns scales when many types map to different handling paths without nested if ladders.

- Switch on type with arms `case Customer c:` (classic) or expression patterns consolidates dispatch.
- Both integrate with nullable flow analysis when patterns succeed.
- Prefer switch when adding a new type means adding an arm—a table-driven dispatch shape.
- Virtual methods often replace type switches for open hierarchies—see Module 02 polymorphism.

---

#### Q15. What happens if you use `return` inside a `try` block that has a `finally` — which executes first?

**Answer:** When `return` executes in `try`, the runtime runs the associated `finally` block before the method actually returns to the caller. If `finally` also contains `return`, that return value overrides the `try` return—see Gotcha 7.

- Cleanup in `finally` runs even when `try` returns normally or via exception.
- Async methods compile similarly with respect to `finally` in many patterns but add state machine complexity.
- Do not put `return` in `finally` except in generated code—it hides control flow.
- See Exception Handling chapter for interaction with exceptions during return.

---

### 07. Methods

#### Q1. What are the `out`, `ref`, and `in` parameter modifiers? Explain their usage.

**Answer:** `ref` passes an alias to an existing variable so the callee can read and write it; `out` requires the callee to assign before return and represents an extra output slot; `in` passes a readonly alias for large structs to avoid copy cost while preventing modification through the parameter.

- Callers must pass `ref` and `out` arguments with the keyword at the call site (`Method(ref x, out y)`).
- `out` variables can be declared inline at the call site in modern C# (`TryParse(s, out int n)`).
- `in` suits large readonly struct parameters in hot paths—see Q12.
- See Q11 for choosing among them.

---

#### Q2. What is the `params` keyword in method definitions?

**Answer:** `params` allows a method to accept a variable number of arguments of a specified array element type, which the compiler packs into an array at the call site. Only one `params` parameter is allowed and it must be the last parameter—see Gotcha 12.

- `void Log(params string[] messages)` enables `Log("a", "b")` without explicit array syntax.
- Passing an existing array explicitly still works without extra copying in many cases.
- Overload resolution prefers non-params overloads when an exact match exists.
- `params` with `ReadOnlySpan<T>` expands options in newer language versions for performance.

---

#### Q3. What are expression-bodied members in C#?

**Answer:** Expression-bodied members use `=>` syntax to define methods, properties, accessors, or constructors with a single expression instead of a braced block, reducing noise for trivial forwarding and computed members.

- Example: `public int Area => Width * Height;` for a read-only property.
- Expression-bodied methods can return values or void (`void M() => Console.WriteLine("hi");`).
- Complex logic should remain block-bodied for debugging and multiple statements.
- Module 02 covers expression-bodied properties in depth.

---

#### Q4. Explain named arguments and optional parameters in C#.

**Answer:** Optional parameters declare default values in the method signature so callers may omit trailing arguments. Named arguments supply parameters by name (`Method(timeout: 30, retries: 3)`) regardless of order, improving readability for long parameter lists.

- Optional parameters must come after required parameters unless all trailing parameters are optional.
- Defaults are compile-time constants embedded at call sites—see Gotcha 13 on recompile requirement when defaults change.
- Named arguments help with boolean flag clarity at call sites.
- Overload resolution considers optional parameters and may introduce ambiguity—see Constructors chapter in Module 02.

---

#### Q5. What are local functions in C#?

**Answer:** Local functions are methods declared inside another method's body, visible only within the enclosing member, useful for splitting algorithm steps without polluting the type's namespace or capturing class state unnecessarily.

- They can be static local functions to avoid accidental capture of instance members when not needed.
- Iterator methods and validation helpers commonly use local functions for clarity.
- Local functions can access outer local variables (closure) unless declared static.
- See Q17 for comparison with private instance methods.

---

#### Q6. What is the `yield` keyword and iterators in C#? *(Cross-ref: Module 03 — IEnumerable)*

**Answer:** `yield return` and `yield break` implement iterator methods that compile into state machines implementing `IEnumerable<T>` or `IEnumerator<T>`, producing elements lazily one at a time without building a full in-memory collection upfront.

- Consumers foreach over the sequence; execution resumes after each `yield return` when the next element is requested.
- Lazy evaluation saves memory for large or infinite sequences filtered by callers.
- Iterator methods cannot mix unstructured `yield` with `try/finally` patterns easily in all cases—language rules apply.
- Module 03 expands IEnumerable, LINQ, and deferred execution interactions.

---

#### Q7. Explain method overloading — what makes two methods overloads vs duplicate definitions?

**Answer:** Overloads share the same method name but differ in parameter count or types (and optionally generic arity); the return type alone cannot distinguish overloads. Duplicate signatures with only return type differing are compile errors.

- `void Print(int x)` and `void Print(string s)` are valid overloads.
- `ref` vs `out` vs value parameter modifiers participate in signature distinction.
- Generic methods overload on type parameter count and constraints.
- See Q8 for resolution when multiple overloads apply.

---

#### Q8. How does overload resolution work when multiple overloads could apply — what is the "better function member" rule?

**Answer:** The compiler picks the best match by preferring fewer conversions, better conversion kinds (implicit over explicit), and non-params candidates over expanded params forms. If no single best member exists, CS0121 ambiguity error results.

- Exact parameter type match beats conversion from `int` to `long`.
- More specific derived type beats base type when both are candidates.
- Named arguments and casts can disambiguate intentional choices.
- See Q16 for ambiguity resolution tactics.

---

#### Q9. Why can't you overload methods by return type alone?

**Answer:** Call sites often ignore return values, so the compiler cannot infer which overload to invoke from return type context alone. Method signature identity for overload purposes includes name and parameter types, not the return type.

- `int GetValue()` and `string GetValue()` with identical parameters cannot coexist.
- Async overloads differ by return type (`Task` vs `Task<int>`) only when combined with `async` pattern and different parameter lists—or the async return type is part of a distinct signature in generic scenarios with constraints.
- Explicit interface implementation can expose conflicting return types on different interfaces implemented by one class.
- Return type participates in conversion targets after an overload is already chosen.

---

#### Q10. What is the difference between call-by-value for value types vs reference types at the parameter boundary?

**Answer:** Value type parameters receive a copy of the bits; mutating the parameter variable does not affect the caller's variable unless `ref` or `out` is used. Reference type parameters copy the reference; mutating the object's fields affects the shared object, but reassigning the parameter to a new object does not change the caller's variable without `ref`.

- See Gotcha 11 on ref reassignment vs mutation.
- `readonly` fields on structs passed by value cannot be mutated through the copy.
- Large structs should use `in` or `ref readonly` to avoid copy cost when read-only.
- Reference type null can be passed; callee can assign parameter to null without affecting caller's reference.

---

#### Q11. When should you use `ref` vs `out` vs `in` for parameters?

**Answer:** Use `out` for Try-pattern results and multiple return values that are definitely assigned in the method. Use `ref` when the method must read and write an existing variable the caller already initialized. Use `in` for large structs the method reads but must not modify through the alias.

- `TryParse` is the canonical `out` pattern—see Q13.
- Swap methods and in-place sorting sometimes use `ref` on locals.
- Avoid `out` when the caller already has meaningful input in the variable that the callee should read (`ref` instead).
- `in` documents readonly intent to callers and analyzers.

---

#### Q12. What problem does the `in` modifier solve for large readonly structs?

**Answer:** Passing a large struct by value copies every field, which is expensive for big value types like 3D transforms or matrix chunks. `in` passes a readonly reference alias, eliminating the copy while preventing silent mutation through the parameter.

- Call site uses `Method(in bigStruct)` for clarity at the API boundary.
- Defensive copies may still occur if the callee stores the parameter to a field in some scenarios (struct lifetime rules).
- Prefer `readonly struct` with `in` parameters together for clarity and analyzer support.
- Small structs (Point, int pairs) often remain pass-by-value for simplicity.

---

#### Q13. What is the Try-pattern (`bool TryX(..., out T result)`), and why is it preferred over exceptions for expected failures?

**Answer:** Try-pattern methods return `false` when an operation cannot complete normally (parse failure, dictionary miss) and assign a default or meaningful `out` value, avoiding exception overhead for control flow that is common rather than exceptional.

- `Dictionary.TryGetValue`, `int.TryParse`, and `Enum.TryParse` follow this convention.
- Exceptions remain appropriate for violated invariants and unexpected environmental failures.
- Consistent naming (`Try` prefix, `out` last) makes APIs discoverable.
- See Gotcha 10 and Input & Output Q3 on user input parsing.

---

#### Q14. Can optional parameters precede required parameters — what are the ordering rules?

**Answer:** Required parameters must appear before optional ones in the parameter list unless the trailing parameters are all optional and callers use named arguments to supply required values after skipped optionals—which is confusing and should be avoided.

- Valid: `void M(int a, int b = 0, int c = 0)`.
- Invalid: `void M(int a = 0, int b)` without special call patterns.
- API design places rarely used flags at the end with defaults.
- Changing default values does not update already compiled callers—Gotcha 13.

---

#### Q15. What is the difference between `params int[]` and passing an explicit `int[]` at the call site?

**Answer:** Both end up as an array inside the method; `params` additionally allows variadic call syntax spreading individual arguments. Overload resolution treats explicit array argument as matching the array parameter directly without params expansion when an exact overload exists.

- `Method(new int[] { 1, 2 })` passes one array object; `Method(1, 2)` creates an array via params expansion.
- Null passed to params parameter can be ambiguous—prefer explicit array or overload without params.
- Performance-sensitive APIs may avoid params to reduce hidden array allocations.
- See Q2 on params placement rules.

---

#### Q16. When does overload resolution fail with ambiguity (CS0121), and how do casts or named arguments resolve it?

**Answer:** Ambiguity occurs when two overloads are equally good matches for the argument list, such as two implicit conversions of equal rank. Resolve by casting arguments to the intended parameter type, using named parameters to select an overload with fewer applicable candidates, or renaming methods to clarify intent.

- `(long)x` or `(int)x` disambiguates numeric overloads.
- Adding an overload with exact match types is the long-term API fix.
- Generic inference failures are a related but distinct compiler error family.
- Document overload sets carefully when adding optional and params overloads together.

---

#### Q17. What is the difference between a local function and a private instance method in the same class?

**Answer:** Local functions are scoped inside a single member and can access local variables and parameters of the enclosing method via closure, while private methods are class-level members callable from any instance or static method in the class depending on modifiers.

- Static local functions cannot capture instance state unless passed explicitly—encourages clearer dependencies.
- Private methods appear in API surface of the type for testing and reuse across multiple members.
- Local functions are not virtual and cannot implement interfaces.
- Choose local functions for single-use algorithm steps tightly coupled to one method's locals.

---

#### Q18. What is recursion, what is a base case, and what risk does unbounded recursion pose?

**Answer:** Recursion is when a method calls itself to solve smaller subproblems; the base case is the condition where the method returns without further recursive calls. Unbounded recursion exhausts the call stack and throws `StackOverflowException`.

- Every recursive path needs a base case and progress toward it (smaller input, closer to terminal state).
- Tail recursion is not guaranteed to optimize to iteration in C#—do not rely on it for deep stacks.
- Tree and graph traversals use recursion with clear exit conditions or switch to explicit stacks for depth safety.
- Mutual recursion between two methods requires the same discipline on both sides.

---

#### Q19. Can `out` variables be declared inline at the call site (`TryParse(text, out int n)`)?

**Answer:** Yes, C# 7+ allows declaring the type inline in the `out` argument position, scoping the new variable to the enclosing block and enabling concise Try-pattern usage without a separate declaration line.

- `if (int.TryParse(line, out int n))` uses `n` in the if block.
- Multiple inline `out` declarations in one call are supported when the method has multiple `out` parameters.
- The variable is definitely assigned when `TryParse` returns true per flow analysis.
- See Q1 and Q13 for Try-pattern context.

---

#### Q20. What is the difference between mutating an object through a reference parameter vs reassigning the parameter variable itself?

**Answer:** Mutating fields on a reference-type object through a parameter (`customer.Name = "x"`) affects the caller's object because both refer to the same instance. Reassigning the parameter (`customer = new Customer()`) only changes the local alias inside the method unless the parameter is `ref Customer`.

- Gotcha 11 states this distinction explicitly for interviews.
- Value types always copy unless `ref`/`out`/`in`; mutation on struct parameter mutates the copy only.
- `ref` reassignment lets callee replace caller's variable binding: `void Reset(ref Customer c) { c = new Customer(); }`.
- Understanding this prevents bugs when trying to "replace" objects passed without `ref`.

---

### 08. Strings

#### Q1. Explain string handling in C# (`string` vs `StringBuilder`).

**Answer:** `string` is an immutable reference type optimized for relatively stable text, while `StringBuilder` provides a mutable buffer for repeated append operations that would otherwise create many intermediate string objects. Choose `string` for simple composition; choose `StringBuilder` for many updates in loops.

- `string` methods like `Replace` and `Substring` return new instances without modifying the original.
- `StringBuilder` exposes `Append`, `Insert`, and `Remove` mutating an internal buffer with amortized growth.
- Interpolation and `string.Join` often suffice without `StringBuilder` for moderate concatenation.
- See Q9 on loop concatenation performance.

---

#### Q2. What are the different ways to format strings in C#? (`String.Format`, interpolation, composite formatting)

**Answer:** Composite formatting (`string.Format`, `Console.WriteLine` with placeholders) uses indexed holes and format providers. String interpolation (`$"..."`) embeds expressions directly. Both ultimately call formatting infrastructure with optional `IFormatProvider`.

- Interpolation is translated to `FormattableString` or `string.Format` calls at compile time.
- Culture-sensitive formatting passes `CultureInfo` explicitly or uses current culture by default.
- `StringBuilder.AppendFormat` combines mutable buffers with composite patterns.
- See Input & Output Q4 and Q8 for console-specific usage.

---

#### Q3. Are strings mutable or immutable in C#? What are the implications?

**Answer:** Strings are immutable: after construction, their character content cannot change. Any operation that appears to modify a string returns a new string instance, which simplifies threading, interning, and hash caching at the cost of allocations when building large text incrementally.

- Safe to share string references across threads without locks for content stability.
- Repeated concatenation in loops allocates O(n²) total characters without `StringBuilder`.
- Custom APIs should not expose mutable char buffers as `string`; use `StringBuilder` or `char[]` internally until finalized.
- Immutability enables the compiler and runtime to intern literal strings—see Q4.

---

#### Q4. What is string interning?

**Answer:** String interning stores one copy of each distinct literal or interned string content in a pool so multiple references can share the same object, saving memory and enabling reference equality for identical content when interned.

- Literal `"hello"` in source may be interned automatically at compile/load time.
- `string.Intern` forces lookup or insertion into the intern pool—see Q15.
- Two equal non-interned strings compare equal with `==` but may not be reference-equal—Gotcha 1.
- Interning trades memory deduplication for lifetime pinning of strings never collected while referenced from the pool.

---

#### Q5. What is the difference between `==`, `Equals`, `Compare`, and `CompareTo` for strings?

**Answer:** `==` and instance `Equals` compare string content with ordinal or overloaded semantics depending on overload; `string.Compare` returns signed ordering with explicit `StringComparison`; `CompareTo` implements `IComparable<string>` for default sort order.

- Use `StringComparison` overloads to avoid culture surprises—see Q6.
- `Compare` is static and accepts comparison type explicitly; good for sort keys.
- Reference equality differs from content equality unless interning aligns references—Q16.
- `Equals` overload without comparison uses ordinal default in modern .NET for `string.Equals(string)`.

---

#### Q6. When should you use `StringComparison.Ordinal` vs `OrdinalIgnoreCase` vs culture-sensitive comparisons?

**Answer:** Use ordinal comparisons for identifiers, file paths, protocol tokens, and dictionary keys where byte-level Unicode order is stable. Use culture-sensitive comparison for user-visible sorting and matching words in natural language. Use `OrdinalIgnoreCase` for case-insensitive identifiers like HTTP headers or enum-like names when culture rules would be wrong.

- Culture-sensitive `string.Compare("i", "I", culture)` differs between Turkish and invariant for dotted/dotless I.
- LINQ and dictionary keys for internal IDs should use ordinal comparers.
- UI sort in user's language uses `StringComparison.CurrentCulture` or explicit culture.
- Security-sensitive comparisons (passwords, tokens) use fixed rules—often ordinal or fixed-time specialized APIs.

---

#### Q7. What are verbatim string literals (`@"..."`), and when are they useful?

**Answer:** Verbatim strings prefix `@` so backslashes are literal and quotes are doubled (`""`) instead of escaped, which simplifies Windows paths, regular expression patterns, and multi-line text without doubling backslashes.

- `@"C:\Users\name"` avoids `"C:\\Users\\name"`.
- Newlines in verbatim strings are literal line breaks in source.
- Interpolation combines `$` and `@`: `$@"Hello {name}\there"`.
- Raw string literals (Q8) supersede some verbatim use cases for embedded quotes.

---

#### Q8. What are raw string literals (`"""..."""`, C# 11+), and how do they handle quotes and newlines?

**Answer:** Raw string literals delimit content with triple quotes (`"""`) and optional indentation stripping, allowing arbitrary quotes and multi-line JSON, SQL, or XML without escape proliferation.

- Opening quotes on their own line enable content that starts on the next line with dedented margins.
- More `"` characters in delimiter handle content containing triple quotes.
- Combine with `$` for interpolation inside raw strings with rules for brace placement on separate lines when needed.
- Prefer raw strings over heavily escaped verbatim strings for embedded code or markup templates.

---

#### Q9. What is the difference between `StringBuilder` and repeated string concatenation in a loop?

**Answer:** Loop concatenation with `+` or `$"{s}{item}"` creates a new string each iteration, copying all prior content repeatedly. `StringBuilder` amortizes growth across a buffer, reducing total copying to roughly linear in final length.

- For small loops or few iterations, concatenation is readable and fast enough.
- Profile before optimizing; `StringBuilder` has overhead for tiny results.
- `string.Join` and `String.Concat` with array or span inputs batch concatenation efficiently.
- See Q18 when `StringBuilder` is not ideal.

---

#### Q10. What is the difference between `string.Concat`, the `+` operator, and interpolation for combining text?

**Answer:** All produce new immutable strings; the compiler often optimizes simple chains of `+` on constants into one literal. Interpolation and `Concat` clarify intent for multiple parts; runtime behavior converges on allocation of a new string with combined content.

- Constant folding merges `"a" + "b"` at compile time.
- Interpolation evaluates expressions once into temporaries before formatting.
- `String.Concat(ReadOnlySpan<string>)` reduces allocations in modern APIs.
- Choose based on readability; micro-differences matter only in hot paths.

---

#### Q11. What is the difference between `IsNullOrEmpty`, `IsNullOrWhiteSpace`, and checking `Length == 0`?

**Answer:** `IsNullOrEmpty` is true for null or zero-length strings. `IsNullOrWhiteSpace` also treats Unicode whitespace-only strings as empty. Checking `Length == 0` requires non-null reference or throws if null.

- Use null-conditional before length: `s?.Length == 0` distinguishes null from empty if needed.
- Whitespace includes spaces, tabs, and culture-specific space characters beyond `' '`.
- Validation of user names often needs `IsNullOrWhiteSpace`; protocol tokens may allow internal spaces but not empty.
- NRT flow analysis may still require null checks before dereferencing.

---

#### Q12. What is the difference between culture-sensitive (`ToUpper()`) and invariant (`ToUpperInvariant()`) case conversion?

**Answer:** Culture-sensitive casing uses rules of `CurrentCulture` or Turkish etc., which can change dotted/dotless I behavior. Invariant casing uses fixed Unicode rules independent of user locale, preferred for identifiers and normalized keys.

- `ToUpper()` without culture uses current culture—dangerous for internal keys in global apps.
- `ToUpperInvariant()` is stable across machines for protocol identifiers.
- Security-sensitive normalization may need custom rules beyond simple casing.
- See Q6 for comparison vs conversion distinction.

---

#### Q13. What methods would you use to split, trim, replace, pad, and search within strings?

**Answer:** `Split` divides on separators; `Trim`/`TrimStart`/`TrimEnd` remove whitespace or specified chars; `Replace` substitutes substrings; `PadLeft`/`PadRight` align fixed-width fields; `Contains`, `IndexOf`, and `StartsWith`/`EndsWith` search with optional `StringComparison`.

- Span-based overloads on modern .NET reduce allocations for parsing pipelines.
- `Split` with `StringSplitOptions.RemoveEmptyEntries` cleans CSV-like input.
- Regular expressions handle complex patterns when literal methods are insufficient.
- `ReadOnlySpan<char>` slicing previews stack-friendly parsing without substring allocation in advanced scenarios.

---

#### Q14. What is UTF-16 storage in .NET strings, and how does that relate to surrogate pairs and `char`?

**Answer:** .NET `string` stores UTF-16 code units in a contiguous buffer; most characters are one `char`, but supplementary Unicode characters (emoji, rare scripts) occupy two `char` surrogate pairs. Length counts code units, not grapheme clusters users perceive as one character.

- Iterating `foreach (char c in s)` visits code units, not full Unicode scalars—use `StringInfo` or `Rune` for grapheme-aware logic.
- `char` is 16-bit UTF-16 code unit, not a full Unicode code point in all cases.
- Encoding to UTF-8 for wire formats uses `Encoding.UTF8.GetBytes` producing variable byte lengths.
- Surrogate pair corruption occurs if you manually splice strings at wrong indices.

---

#### Q15. What is the string intern pool, and what does `string.Intern` do?

**Answer:** The intern pool is a runtime table of unique string contents; `string.Intern` returns the pooled reference for the argument's content, creating an entry if absent. Literals may already be interned without explicit calls.

- Useful rarely for deduplicating massive repeated dynamic strings with identical content.
- Interned strings live for process lifetime if referenced from pool—memory trade-off.
- Do not intern unbounded user input—it can grow the pool without bound in pathological cases.
- See Gotcha 1 and Q4 for reference vs content equality implications.

---

#### Q16. Why can two strings with identical content fail `ReferenceEquals` while still passing `==`?

**Answer:** `==` for strings compares character content (ordinal by default in many overload paths), while `ReferenceEquals` checks object identity. Two separately constructed strings with the same text are equal by content but may be different heap objects unless interning aligns them.

- Literal `"hi"` reused in source may be the same reference; `new string("hi".ToCharArray())` is often not.
- Gotcha 1 is the canonical interview trap on interning.
- Do not use `ReferenceEquals` for string content comparison—use `==` or `Equals` with explicit comparison.
- Performance-sensitive deduplication sometimes interns known keys intentionally.

---

#### Q17. What is the difference between `Substring` and range/index syntax (`s[start..end]`) for slicing strings?

**Answer:** Both extract contiguous portions; `Substring(start, length)` uses start and length, while range syntax `s[start..end]` uses start inclusive and end exclusive indices with clearer intent for "from here to there."

- Negative indices from end work with `^` in ranges: `s[^3..]` last three characters.
- Both allocate new strings because strings are immutable.
- Out-of-range indices throw `ArgumentOutOfRangeException` similarly.
- Span `s.AsSpan(start, length)` avoids allocation when downstream APIs accept span.

---

#### Q18. When is `StringBuilder` not the best choice despite many append operations?

**Answer:** When the final size is known upfront, a pre-sized `char[]` or single `string.Create` call may allocate once without `StringBuilder` overhead. Very few appends (two or three) are often clearer with interpolation or `Concat`.

- `string.Create(length, state, callback)` fills a buffer in one shot for expert scenarios.
- Logging frameworks batch efficiently without manual `StringBuilder` in application code.
- Pooling `StringBuilder` instances (`StringBuilderCache` internally in BCL) is framework concern, not typical app code.
- Measure: small `StringBuilder` growth copies dominate only at scale.

---

#### Q19. How does string interpolation handle format specifiers and alignment (`$"{price:C2}"`, `$"{name,-20}"`)?

**Answer:** Interpolation holes accept format after colon (`:C2` currency two decimals) and alignment after comma (`,-20` left-align width 20), mirroring composite formatting rules inside `{expression,alignment:format}`.

- Alignment pads with spaces by default; format uses current culture unless `FormattableString.Invariant` or custom culture is applied.
- Complex formats delegate to `IFormattable` on the expression's type.
- Constant format strings enable compile-time checking in some analyzers for correctness.
- See Input & Output Q12 for console column layout examples.

---

#### Q20. What is the performance implication of calling `Replace` or `Trim` on large strings repeatedly?

**Answer:** Each call scans the full string and allocates a new string when changes occur, so chaining many passes over megabyte-scale text multiplies work and garbage. Prefer single-pass algorithms, spans, or `StringBuilder` pipelines for heavy text processing.

- `Replace` in a loop searching changing patterns can degrade badly without `StringBuilder`.
- Immutable returns mean no in-place win even when only one character changes.
- For hot paths, consider `MemoryExtensions`, regex compiled once, or streaming readers.
- Profile with realistic payload sizes before optimizing string pipelines.

---

### 09. Arrays

#### Q1. What are arrays in C#? How is memory managed for single-dimensional, multi-dimensional, and jagged arrays?

**Answer:** Arrays are reference types holding a contiguous (per dimension rules) sequence of elements with fixed rank established at creation. Single-dimensional arrays store elements in one block; rectangular multi-dimensional arrays store one block with row-major layout; jagged arrays are arrays of arrays, each row potentially different length on the heap.

- Single-dim: `int[]` object header plus element buffer on heap.
- Rectangular `int[,]` stores all cells in one array object with two lengths.
- Jagged `int[][]` has outer array referencing separate inner arrays—non-uniform row lengths possible.
- All array objects are heap-allocated; the variable is a reference—see Q5.

---

#### Q2. What is a jagged array?

**Answer:** A jagged array is an array whose elements are themselves arrays (`int[][]`), allowing each row to have a different length, unlike rectangular `int[,]` where every row shares the same column count in one matrix object.

- Useful for sparse or ragged tables where rectangular storage would waste space.
- Access is two-step: `jagged[i][j]` with separate null checks for each row array.
- Memory layout differs from rectangular 2D—see Q15.
- Initialization often loops creating each inner array explicitly.

---

#### Q3. What is the difference between `Array.Copy()`, `Clone()`, and assigning one array variable to another?

**Answer:** Assignment copies the reference only—both variables point to the same array object. `Clone()` on arrays performs shallow copy of elements into a new array object (new array, same element references for reference types). `Array.Copy` copies a range of elements from source to destination array, which may be existing or sized appropriately.

- Deep copy of elements requires looping or serialization, not `Clone()` alone for reference-type elements.
- `Copy` respects overlapping regions with defined behavior for same-array copies.
- Assignment does not duplicate elements—mutations visible through both references.
- See Q11 on shallow vs element copy semantics.

---

#### Q4. What is the difference between a single-dimensional array, a rectangular multi-dimensional array (`[,]`), and a jagged array (`[][]`)?

**Answer:** Single-dimensional arrays model vectors; rectangular arrays model fixed grid dimensions with one object; jagged arrays model rows as independent arrays allowing ragged shapes.

| Type | Syntax | Layout |
|---|---|---|
| Single-dim | `int[]` | One contiguous element block |
| Rectangular | `int[,]` | One object, `[row, col]` indexing |
| Jagged | `int[][]` | Outer array + per-row inner arrays |

Choose rectangular for dense matrices with fixed columns; jagged for variable row lengths.

---

#### Q5. Are arrays value types or reference types in C#?

**Answer:** Arrays are reference types inheriting from `System.Array`, regardless of whether their elements are value or reference types. The array variable holds a reference to the heap object containing lengths and element storage.

- `int[]` is a reference type; elements are value types stored inline in the array buffer.
- Default value of an array variable is `null`, not an empty array.
- `default(int[])` is null; use `Array.Empty<int>()` for zero-length singleton.
- Passing arrays to methods passes reference copy—see Q16.

---

#### Q6. What is array covariance for reference types, and why is `object[] arr = new string[3]; arr[0] = 42;` dangerous?

**Answer:** Covariance allows assigning a derived array to a base array reference (`string[]` to `object[]`), but writing a non-compatible element through the base reference throws `ArrayTypeMismatchException` at runtime even though the assignment compiled.

- Covariance is safe for reading when element types match variance rules; writes can fail.
- Value type arrays are not covariant to `object[]` without boxing each element in a new array.
- Gotcha 8 is the classic interview example.
- Prefer `IReadOnlyList<T>` or generics for safe heterogeneous read scenarios without write holes.

---

#### Q7. What do `Array.Resize`, `Array.Fill`, and `Array.Clear` do — which allocate new memory?

**Answer:** `Array.Resize` creates a new array of the specified size and copies elements from the old array, replacing the reference passed by ref—it allocates new memory. `Array.Fill` sets all elements to a value in an existing array without reallocating. `Array.Clear` zeroes or nulls a range in an existing array without reallocating.

- `Resize` is O(n) copy; use `List<T>` when frequent growth is needed.
- `Fill` and `Clear` mutate in place for existing buffers.
- Clearing sets value types to zero and references to null.
- Distinguish `Array.Clear` from `list.Clear()` which removes elements in dynamic lists.

---

#### Q8. What is the difference between `Length` on a single-dimensional array vs `GetLength(dimension)` on multi-dimensional arrays?

**Answer:** `Length` on single-dimensional arrays returns total element count. Multi-dimensional arrays expose `Rank` and per-dimension lengths via `GetLength(0)`, `GetLength(1)`, etc., because `Length` returns total elements across all dimensions.

- `int[,]` with 3 rows and 4 columns has `Length == 12` and `GetLength(0) == 3`, `GetLength(1) == 4`.
- Jagged outer array `Length` is row count; inner arrays have their own lengths.
- Bounds checks use these lengths on each access.
- Loop bounds should call the correct API for the array type to avoid logic errors.

---

#### Q9. How do you initialize arrays with collection initializer syntax and `new int[] { 1, 2, 3 }`?

**Answer:** Array initializer syntax lists elements in braces after `new Type[]` or with target-typed `new[] { 1, 2, 3 }` when the variable type is known. The compiler allocates an array of the correct size and assigns elements in order.

- Multi-dimensional rectangular arrays use nested brace syntax with uniform row lengths.
- Jagged arrays often combine outer initializer with per-row `new int[size]`.
- Collection expression syntax (`[1, 2, 3]`) in newer C# can target arrays and spans in some contexts.
- Initializers run before array reference is published to callers.

---

#### Q10. What is the relationship between arrays and `params` parameters in methods?

**Answer:** A `params` parameter is syntactic sugar for a single-dimensional array parameter; callers may pass individual arguments that the compiler collects into an array, or pass an array directly.

- See Methods Q2 and Q15.
- Params arrays must be last in the signature—Gotcha 12.
- Only one params parameter per method.
- Prefer `ReadOnlySpan<T>` overloads in performance APIs alongside params for flexibility.

---

#### Q11. What is the difference between shallow copy of an array reference and copying array elements?

**Answer:** Copying the reference aliases the same array object; copying elements into a new array duplicates the slot values—for reference-type elements, shallow copy duplicates references to the same nested objects, not deep clones of those objects.

- `Array.Copy` and `Clone` perform shallow element copy into new array storage.
- Deep copy requires per-element clone logic for mutable reference types.
- Assignment `b = a` shares identity; mutating `b[i]` affects `a[i]`.
- Immutability of elements simplifies reasoning after shallow array copy.

---

#### Q12. When would you use `Array.Sort` vs LINQ `OrderBy` on an array?

**Answer:** `Array.Sort` sorts in place mutating the original array with efficient memory use and optional custom comparer. LINQ `OrderBy` returns a new ordered sequence (often deferred) without requiring in-place mutation, integrating with IEnumerable pipelines.

- Sorting large arrays in memory-critical code often prefers `Array.Sort`.
- Functional style chaining uses `OrderBy` then `ToArray()` if a new array is acceptable.
- `Array.Sort` throws if comparer violates contract; stable sort behavior differs from LINQ sort guarantees in some providers.
- Partial sorts and binary search (`Array.BinarySearch`) assume sorted in-place arrays.

---

#### Q13. What bounds-checking behavior does C# provide for array indexing?

**Answer:** Every array index access is bounds-checked at runtime, throwing `IndexOutOfRangeException` when the index is less than zero or greater than or equal to the length. The Just-In-Time (JIT) compiler may optimize checks when it can prove safety in tight loops.

- Multi-dimensional indexing checks each dimension separately.
- `Span<T>` and `Memory<T>` provide similar checks with potentially better optimization paths.
- Unchecked unsafe pointer access bypasses checks—unsafe context required.
- Off-by-one errors at `Length` index are a common bug source despite checks.

---

#### Q14. What is `Span<T>`/`ReadOnlySpan<T>` in relation to arrays (preview — stack-friendly views)?

**Answer:** `Span<T>` is a stack-only ref struct view over contiguous memory such as arrays, strings (via `ReadOnlySpan<char>`), or stackalloc buffers, providing slice and parse operations without allocating subarrays.

- `array.AsSpan(start, length)` creates a window without copying elements.
- Spans enable modern high-performance APIs (`TryParse` on spans, UTF-8 processing).
- Cannot be stored on heap fields directly due to ref struct restrictions (with exceptions for ref fields in newer versions in limited scenarios).
- Module 03 and performance chapters expand span usage patterns.

---

#### Q15. How do jagged arrays differ in memory layout from rectangular 2D arrays?

**Answer:** Rectangular arrays allocate one object with all cells in a single block indexed by `[row, col]`. Jagged arrays allocate an outer array of references, each pointing to a separate inner array object that may differ in length, causing more indirection and potential cache misses but saving space for ragged data.

- Rectangular: better locality for dense fixed-size matrices.
- Jagged: flexible row lengths, extra pointer per row.
- Serialization formats may prefer one shape over the other for compatibility.
- Choose based on data shape and access patterns, not syntax preference alone.

---

#### Q16. What happens when you pass an array to a method — can the callee change the caller's array contents?

**Answer:** The callee receives a copy of the reference pointing to the same array object, so mutating elements (`arr[0] = 99`) is visible to the caller. Reassigning the parameter to a new array object does not change which array the caller's variable references unless `ref` is used on the array parameter.

- Length is fixed after creation; callee cannot resize caller's array without `ref` and `Array.Resize` on caller's variable.
- Null assignment to parameter does not null caller's reference.
- Same semantics as reference types generally—see Methods Q10 and Q20.
- Returning a new array is common when transformation changes size.

---

### 10. Exception Handling

#### Q1. Explain exception handling in C# (`try`, `catch`, `finally`, `throw`, and custom exceptions).

**Answer:** `try` wraps code that may fail; `catch` handles specific exception types; `finally` runs cleanup whether or not an exception occurred; `throw` signals failure up the stack; custom exceptions derive from `Exception` to express domain-specific errors with context.

- Catch most specific types first; general `Exception` last if used at all.
- `finally` releases unmanaged resources or resets state; pair with `using` for `IDisposable`.
- Custom exceptions should be `[Serializable]` when remoting legacy scenarios matter and include useful messages, not control flow.
- Unhandled exceptions terminate the process in console apps unless a host catches them.

---

#### Q2. What is the difference between `throw` and `throw ex`?

**Answer:** `throw;` inside a catch block rethrows the same exception object and preserves the original stack trace, while `throw ex;` throws a new exception reference that resets the stack trace to the current catch location, hiding where the failure originally occurred.

- Use `throw;` after logging to keep diagnostics intact—Gotcha 6.
- Wrap with `throw new CustomException("...", ex)` when adding context and passing inner exception explicitly.
- Never use bare `throw ex;` unless you intentionally want a fresh stack (rare).
- See examples.md minimal code sample for the pattern.

---

#### Q3. Explain the `using` statement in the context of exception handling and resource management.

**Answer:** The `using` statement compiles to try/finally that calls `Dispose()` on `IDisposable` objects when leaving scope, even if an exception occurs, ensuring files, connections, and handles release promptly instead of waiting for garbage collection.

- `using var stream = File.OpenRead(path);` disposes at end of enclosing block (using declaration).
- Nested `using` statements dispose in reverse order of acquisition.
- `Dispose` should not throw; implementers swallow secondary errors when possible.
- `using` does not catch exceptions—it guarantees disposal on exceptional paths.

---

#### Q4. What are exception filters in C#?

**Answer:** Exception filters are `when` clauses on `catch` statements that run a boolean expression after matching the exception type but before entering the catch block, allowing conditional handling without catching and rethrowing to inspect state.

- `catch (Exception ex) when (ex.HResult == specificCode)` handles only matching cases.
- Filters must not throw; throwing converts to `FailedExceptionFilterException` wrapping the filter failure.
- Useful for logging correlation without losing stack via catch-and-rethrow patterns.
- Overuse complicates control flow; prefer typed exceptions when possible.

---

#### Q5. What is the difference between catching a specific exception type vs `catch (Exception)`?

**Answer:** Catching specific types (`FormatException`, `IOException`) handles anticipated failures with targeted recovery, while catching `Exception` intercepts all managed exceptions including unexpected bugs, which often hides defects unless you rethrow after logging.

- Broad catch is acceptable at process boundaries (top-level handler) or when translating to user-safe messages before rethrow.
- Catch derived before base; unreachable catch blocks are compile errors.
- Filtering with `when` narrows broad catches without separate rethrow gymnastics.
- ASP.NET Core middleware often maps exception types to HTTP status codes selectively.

---

#### Q6. What happens if an exception is thrown inside a `finally` block?

**Answer:** If `finally` throws while unwinding from an earlier exception, the new exception typically replaces the original active exception (or is chained depending on runtime/version rules), and the original failure information may be lost unless captured in an inner exception or logged first.

- Avoid throwing from `finally`; log and swallow secondary failures during cleanup when primary error matters more.
- `Dispose` implementations should not throw if avoidable for this reason.
- Return statements in `finally` similarly override try outcomes—Gotcha 7.
- Design cleanup code defensively with try/catch inside finally for non-critical steps.

---

#### Q7. What is the base class hierarchy for exceptions in .NET (`Exception`, `SystemException`, application-specific types)?

**Answer:** All exceptions derive from `Exception`. Many BCL runtime errors derive from `SystemException` (historical distinction for system vs application). Application code typically throws `ApplicationException` subclasses or domain-specific types directly inheriting `Exception` or intermediate bases like `InvalidOperationException`.

- `ArgumentException`, `InvalidOperationException`, and `NotSupportedException` are common BCL bases for APIs.
- Do not catch `StackOverflowException` or `OutOfMemoryException` for recovery in most cases.
- `AggregateException` wraps multiple failures from parallel tasks.
- Custom hierarchies should be shallow and meaningful to callers and middleware.

---

#### Q8. When should you create a custom exception type vs using an existing BCL exception?

**Answer:** Use existing BCL exceptions when the failure mode matches their documented semantics (`ArgumentNullException` for null args, `InvalidOperationException` for wrong object state). Create custom types when callers need to distinguish domain failures programmatically or attach structured data not expressible in message alone.

- Custom exceptions need meaningful names ending in `Exception` and optional custom properties (ErrorCode, EntityId).
- Avoid deep exception inheritance trees rarely caught at different levels.
- Prefer standard types in public libraries to reduce consumer catch proliferation.
- Document which exceptions public methods throw in XML docs for API consumers.

---

#### Q9. What is the difference between `using` statement and `using` declaration (`using var`) for disposal?

**Answer:** Classic `using (var r = ...) { }` creates an explicit block scope ending with dispose at the closing brace. `using var r = ...;` disposes at the end of the enclosing scope (method or block), reducing nesting while preserving dispose-on-exit semantics.

- Both compile to equivalent dispose patterns with try/finally.
- `using var` disposal order at method end is reverse declaration order.
- Choose block form when dispose boundary is narrower than the whole method.
- Neither replaces `IAsyncDisposable` async using patterns for async disposal (`await using`).

---

#### Q10. Can you have multiple `catch` blocks — what is the order rule for catching derived vs base exceptions?

**Answer:** Multiple catch blocks are allowed for disjoint types; the compiler requires more specific types before less specific bases because the first matching catch handles the exception. A derived catch after a base catch for the same hierarchy is unreachable and errors at compile time.

- Only one catch executes per thrown exception.
- Exception filters can skip a catch block even when type matches, allowing fall-through to later catches in some designs—use carefully.
- Empty catch blocks swallow errors—avoid except at intentional boundaries with logging.
- Rethrow with `throw;` after partial handling to let upstream catch broader policy.

---

#### Q11. What is `finally` guaranteed to do, and can it prevent an exception from propagating?

**Answer:** `finally` runs when control leaves the associated try/catch via normal completion, exception, or return, making it suitable for cleanup. A `return` or uncaught exception thrown inside `finally` can override or replace pending exceptions from try, effectively changing propagation—see Gotcha 7.

- `finally` does not suppress try exceptions unless it completes normally without throwing and without return override quirks.
- Cleanup in finally should be idempotent when possible.
- Thread abort and process kill can skip finally in catastrophic scenarios—design critical durability with `try/finally` plus persistent state.
- Async finally in async methods runs as part of async state machine completion.

---

#### Q12. What is the difference between handled exceptions and unhandled exceptions in a console vs ASP.NET host?

**Answer:** Handled exceptions are caught by application catch blocks that recover or translate errors without terminating the process. Unhandled exceptions propagate until the runtime or host default handler runs— crashing console apps or returning HTTP 500 in ASP.NET Core developer/production exception middleware.

- Top-level `AppDomain.UnhandledException` and `TaskScheduler.UnobservedTaskException` are last-chance hooks.
- ASP.NET Core maps unhandled exceptions to responses via exception handler middleware and logging.
- Catching at boundary and returning exit codes is console best practice for CLI tools.
- Logging handled exceptions still matters for observability even when user sees friendly message.

---

#### Q13. When is it appropriate to catch and swallow an exception vs rethrow?

**Answer:** Swallow only when the failure is fully handled and documented (retry succeeded, optional feature unavailable) and logging captures context for diagnostics. Rethrow when callers must react, transaction must abort, or you lack authority to decide recovery—use `throw;` to preserve stack.

- Empty catch is a code smell unless idempotent probe operations (try read optional config).
- Catch-log-rethrow at layer boundaries preserves observability without losing stack via `throw;`.
- Do not swallow `OutOfMemoryException` hoping to continue reliably.
- Try-pattern preferred over catch for expected parse failures—Gotcha 10.

---

#### Q14. What is `ExceptionDispatchInfo`, and when is `throw;` insufficient?

**Answer:** `ExceptionDispatchInfo.Capture(ex)` stores an exception's stack for rethrow on another thread with `Throw()`, preserving the original stack in scenarios where bare `throw;` cannot cross async or thread boundaries cleanly.

- Useful when marshaling failures from background threads to request threads in advanced patterns.
- `throw;` only preserves stack when rethrowing on the same logical catch stack frame.
- `AggregateException` on tasks may flatten inner exceptions for reporting.
- Most application code never needs `ExceptionDispatchInfo`; know it for library and parallel code reviews.

---

#### Q15. What happens if both `try` and `finally` contain `return` statements?

**Answer:** The `finally` block executes before the method actually returns, and if `finally` contains its own `return`, that return value typically overrides the `try` return value, producing surprising results—Gotcha 7.

- Avoid `return` in `finally` in handwritten code.
- Same interaction applies when `try` throws but `finally` returns, potentially swallowing the exception.
- Refactoring to local result variables set in try and returned after finally clarifies intent.
- Control Flow Q15 cross-references this behavior.

---

#### Q16. What is the difference between `IDisposable.Dispose` and finalizers in exception-safe cleanup?

**Answer:** `Dispose` runs deterministically when `using` or explicit calls release resources promptly and should not throw. Finalizers (`~ClassName`) run non-deterministically on garbage collection as a last-chance safety net for unmanaged handles, unsuitable for timely release during normal exception flows.

- Always implement `Dispose` for unmanaged resources; suppress finalizer when dispose succeeds (`GC.SuppressFinalize`).
- Finalizers delay object collection and add GC overhead—Module 02 covers IDisposable vs finalizer depth.
- Exception during dispose should not mask original exception from try without careful logging.
- `SafeHandle` encapsulates critical finalization patterns for native resources.

---

### Gotchas — Module 01

#### Gotcha 1. String interning

**Answer:** Many candidates assume two strings with the same text always share reference identity, but only interned literals and explicit interning guarantee that; separately constructed equal strings compare equal with `==` yet may fail `ReferenceEquals`.

- Literal `"hello"` assignments often alias; `new string('h', 5)` built at runtime typically does not.
- Rely on `==` or `Equals` for content, not `ReferenceEquals`, unless testing interning explicitly.
- See Strings Q4, Q15, and Q16 for the full interning model.

---

#### Gotcha 2. Integer division

**Answer:** Developers expect `10 / 3` to yield a fractional result, but integer division truncates toward zero when both operands are integral types.

- Promote at least one operand to `double`, `decimal`, or `float` for fractional math.
- See Operators Q4 and Type Conversion Q14 for related numeric rules.
- Financial code should use `decimal` explicitly, not integer division with accidental truncation.

---

#### Gotcha 3. `const` vs runtime values

**Answer:** `const` requires compile-time constants, so values like `DateTime.Now` or computed decimals at runtime cannot be `const`; use `readonly` fields or properties set in constructors instead.

- Callers embedding optional parameter defaults capture const values at compile time—related to Gotcha 13.
- See Data Types Q8, Q9, and Q28.

---

#### Gotcha 4. Boxing silently hurts performance

**Answer:** Assigning value types to `object` or non-generic collections boxes each value on the heap, causing allocations invisible in source that accumulate in hot loops.

- Prefer `List<int>` over `ArrayList` for integers.
- Interface dispatch on structs often boxes—see Module 02 Gotcha 5.
- See Data Types Q4 and Q14.

---

#### Gotcha 5. Modifying a struct inside `foreach`

**Answer:** The foreach iteration variable is a read-only copy of each element, so mutating fields on that copy does not update the collection and fails to compile when you try to assign to the iteration variable itself.

- Use `for` with index, `Span<T>`, or refactor to mutable reference types when in-place updates are required.
- C# 5 fixed closure capture for foreach but not struct mutation rules.

---

#### Gotcha 6. `throw;` vs `throw ex;`

**Answer:** Rethrowing with `throw ex;` resets the stack trace to the catch line, destroying diagnostic context; `throw;` preserves the original failure site.

- Wrap with new exception types using inner exceptions when adding context intentionally.
- See Exception Handling Q2.

---

#### Gotcha 7. `return` in `try` vs `finally`

**Answer:** `finally` always runs before the method completes, and a `return` inside `finally` can override the value or exception pending from `try`, producing surprising control flow.

- Never `return` from `finally` in application code.
- See Control Flow Q15 and Exception Handling Q11 and Q15.

---

#### Gotcha 8. Array covariance trap

**Answer:** Assigning `string[]` to `object[]` compiles due to covariance, but storing an incompatible element like `42` throws `ArrayTypeMismatchException` at runtime on write.

- Covariance is read-safe in many scenarios; writes are the trap.
- See Arrays Q6.

---

#### Gotcha 9. Culture-sensitive parse/format

**Answer:** The same string `"3,14"` parses as 314 or 3.14 depending on whether comma is a decimal or thousands separator under the active culture, breaking logs and APIs shared across locales.

- Use `InvariantCulture` for stored and transmitted formats; use explicit culture for localized UI input.
- See Input & Output Q5–Q7 and Type Conversion Q10.

---

#### Gotcha 10. `Parse` vs `TryParse` in user input paths

**Answer:** `int.Parse` on invalid console input throws and can crash the app; `TryParse` treats failure as a normal branch suitable for reprompt loops.

- Exceptions are for exceptional conditions, not expected typos.
- See Input & Output Q3 and Q14 and Methods Q13.

---

#### Gotcha 11. `ref` reassignment vs mutation

**Answer:** Reassigning a reference parameter to a new object does not change the caller's variable unless the parameter is `ref`; mutating fields on the shared object does affect the caller.

- See Methods Q10 and Q20.

---

#### Gotcha 12. `params` must be last

**Answer:** Only one `params` array parameter is permitted and it must be the final parameter in the method signature; violating this is a compile error.

- See Methods Q2 and Q14.

---

#### Gotcha 13. Optional parameter defaults are compile-time

**Answer:** Default argument values are baked into call sites at compile time, so changing a default in the method definition does not affect callers until they recompile.

- Prefer overloads or mandatory parameters for breaking default changes in public APIs.
- See Methods Q4 and Q14.

---

#### Gotcha 14. `checked` default is context-dependent

**Answer:** Integer arithmetic wraps silently in unchecked default contexts; financial or checksum code may need explicit `checked` blocks or project settings to throw on overflow instead.

- See Operators Q2 and Q13.

---

#### Gotcha 15. Console encoding mismatch

**Answer:** Writing Unicode text when `Console.OutputEncoding` and the terminal code page disagree produces replacement characters or mojibake, especially on Windows consoles not configured for UTF-8.

- Align console, process, and font encoding for international output.
- See Input & Output Q9.

---

## Module 02. Object Oriented Programming

### 01. Classes & Objects

#### Q1. What is a class and what is an object in C#?

**Answer:** A class is a type definition—a blueprint describing fields, properties, and methods that instances will have. An object is a concrete instance of that class created at runtime with `new`, holding its own copy of instance state while sharing method implementations from the type.

- The class exists once in metadata; many objects can be instantiated from it during execution.
- Reference-type objects live on the heap; the variable stores a reference to the object.
- Static members belong to the type; instance members belong to each object—see Q12.
- Tutorial terminology: `Student` is the class; `new Student(...)` produces an object.

---

#### Q2. What is the difference between `struct` and `class` in C#?

**Answer:** `struct` is a value type copied on assignment and defaulting to zeroed fields without null (unless nullable), while `class` is a reference type identified by reference, defaulting to null, supporting inheritance and full polymorphism.

| | `struct` | `class` |
|---|---|---|
| Kind | Value type | Reference type |
| Inheritance | Interfaces only | Single base class + interfaces |
| Default | Zero bits | `null` |
| Identity | Copied; no reference identity | Reference equality by default |

Choose structs for small immutable data; classes for identity, shared mutable state, and inheritance hierarchies.

---

#### Q3. What are the different principles of OOP supported in C#?

**Answer:** C# supports encapsulation (hide state, expose controlled APIs), abstraction (essential model without implementation detail), inheritance (reuse and extend types), and polymorphism (one interface, many behaviors via virtual methods and interfaces).

- Encapsulation uses access modifiers and properties—Module 02 chapter 07.
- Abstraction uses abstract classes and interfaces—chapter 06.
- Inheritance and polymorphism—chapter 05.
- C# also emphasizes composition patterns alongside classical OOP in modern API design.

---

#### Q4. What is a partial class in C#?

**Answer:** A `partial class` splits one class definition across multiple source files, merged by the compiler into a single type. It supports designer-generated code separation (WinForms, EF) and large team workflows without one giant file.

- All parts must use the `partial` modifier and the same namespace and class name.
- Partial methods (with restrictions) allow one part to declare and another to implement.
- Cannot split across assemblies—partial is a compile-time source organization feature only.
- One partial file can hold generated code users should not edit manually.

---

#### Q5. Explain object initializers and collection initializers in C#.

**Answer:** Object initializers set public fields or properties immediately after construction with `{ Property = value }` syntax without requiring a dedicated constructor overload. Collection initializers add elements to collections implementing `Add` with `{ item1, item2 }` or `{ ["key"] = value }` for indexers.

- Object initializers call constructor first, then assign listed members in source order.
- Collection initializers desugar to repeated `Add` calls on the new collection instance.
- Init-only properties (`init`) work in object initializers until construction completes—chapter 02.
- Initializers improve readability for DTO construction and test data setup.

---

#### Q6. What is the difference between shallow copy and deep copy in C#?

**Answer:** Shallow copy duplicates the top-level object and copies field values as-is—for reference fields, both copies point to the same nested objects. Deep copy recursively clones nested objects so the clone graph is independent, requiring custom logic or serialization.

- `MemberwiseClone` on classes is protected shallow copy; structs copy by value shallowly for contained references.
- Arrays clone shallowly for elements that are reference types.
- Immutable nested objects make shallow copy safe when inner state cannot change.
- See Arrays Q11 for array copy semantics.

---

#### Q7. What is the difference between object identity and object equality?

**Answer:** Identity means two references denote the same heap object (`ReferenceEquals` true). Equality means two objects compare as equivalent by value or custom logic (`Equals`, overloaded `==`) even when they are distinct instances.

- Default class equality is reference identity unless overridden.
- Value types compare by value bitwise/default equality unless overridden.
- Equal but non-identical strings illustrate content vs reference—Module 01 Strings Q16.
- Consistent `Equals`, `GetHashCode`, and `==` matter for collections—Gotcha 3 Module 02.

---

#### Q8. What is the difference between `IDisposable` and a finalizer (`~ClassName()`)?

**Answer:** `IDisposable.Dispose` releases resources deterministically when callers use `using` or explicit dispose. A finalizer runs later during garbage collection as a safety net for missed dispose calls, not for timely cleanup of scarce resources like file handles.

- Implement dispose pattern: public `Dispose()` calling protected virtual `Dispose(bool disposing)`.
- Suppress finalizer after successful dispose with `GC.SuppressFinalize`.
- Finalizers add GC overhead and non-deterministic timing—Gotcha 13 Module 02.
- Unmanaged resources belong in dispose; managed references usually need only nulling in dispose when holding events or caches.

---

#### Q9. What happens at runtime when you execute `new MyClass()` — allocation, constructor, and reference assignment?

**Answer:** The runtime allocates memory for the object (heap for classes), initializes fields to defaults or field initializers, runs instance constructors (base then derived chain), and returns a reference assigned to the variable.

1. **Allocate** — CLR allocates object header, method table pointer, and field storage aligned for the type.
2. **Initialize fields** — Field initializers and default values run before constructor body in defined order (chapter 03).
3. **Construct** — Constructor chain executes `: base(...)` then `: this(...)` rules per inheritance.
4. **Assign** — Reference is stored in the target variable or returned to caller.

No object exists for instance methods until `new` completes successfully.

---

#### Q10. Where are class instances stored vs where are struct instances typically stored when local variables?

**Answer:** Class instances always live on the managed heap; local variables hold references. Struct locals typically reside on the stack or in registers, but structs embedded in heap objects or boxed to `object` live on the heap as part of those containers.

- Escape analysis may allocate struct locals on heap when referenced from closures surviving the method.
- Large struct locals still copy by value on assignment—performance consideration for `in` parameters.
- See Module 01 Q13 for value vs reference storage teaching model.
- `stackalloc` and `Span` scenarios use stack memory for buffers with safety rules.

---

#### Q11. What is the difference between a field, a property, and a method on a class?

**Answer:** Fields are data storage locations; properties are accessors (often with get/set) presenting controlled access to state; methods are operations that perform behavior, optionally mutating state or computing results without necessarily exposing storage.

- Public fields expose implementation directly—discouraged in public APIs (chapter 07).
- Properties can validate, compute, or defer loading while keeping field-like syntax at call sites.
- Methods express actions (`CalculateTotal`, `Save`) with arbitrary parameters and return types.
- Auto-properties blur field/property line syntactically but still generate hidden backing fields.

---

#### Q12. What is a static class vs an instance class — can you instantiate a static class?

**Answer:** A static class is sealed, cannot be instantiated, and contains only static members—it acts as a container for shared utilities. Instance classes create objects with `new` and may mix instance and static members.

- Attempting `new` on a static class is a compile error.
- Static classes cannot implement interfaces (C# rules)—chapter 04.
- Instance classes can have static helpers (`InstanceCount`) alongside instance state.
- Prefer instance services with dependency injection over static classes for testability—Gotcha 11 Module 02.

---

#### Q13. What is the `null` reference for reference types, and what is `default` for a struct vs a class?

**Answer:** Reference type variables default to `null`, meaning no object is referenced. Struct `default` is all-zero value with no null unless `Nullable<T>`. Class `default` in generics is null.

- Dereferencing null throws `NullReferenceException`.
- Nullable reference type annotations warn when null assigned to non-nullable references under `#nullable enable`.
- `default(Customer)` for a class is null; `default(Point)` for struct is (0,0) coordinates.
- Always initialize reference fields in constructors for non-nullable intent.

---

#### Q14. What is object initializer syntax, and how does it interact with constructors?

**Answer:** Object initializer syntax runs immediately after the selected constructor completes, assigning listed properties or fields in textual order. Constructor establishes invariants; initializer sets additional optional surface properties.

- You must invoke an accessible constructor—either parameterless or matched overload before `{ ... }`.
- Init-only properties accept assignments only during this construction phase in modern C#.
- Constructor cannot see initializer assignments; initializer runs after constructor body returns to caller chain.
- See Q5 and Properties chapter Q13.

---

#### Q15. What is the difference between `ReferenceEquals`, `==`, and `Equals` for classes that do not override equality?

**Answer:** Without overrides, `ReferenceEquals` and `==` (unless overloaded) compare reference identity, and `Equals` on `Object` also uses reference equality by default. Overloads may diverge if `==` is customized without matching `Equals`.

- Structs use value equality defaults; classes use reference identity defaults.
- Gotcha 14 Module 02 warns when `==` is overridden without consistent `GetHashCode`.
- For domain equality, override `Equals`, `GetHashCode`, and optionally `==` together.
- See Module 01 Operators Q3 and Q14.

---

#### Q16. When is a struct copied vs when is a reference copied when passed to a method?

**Answer:** Struct parameters copy the entire struct value into the parameter slot unless modified by `ref`, `out`, or `in`. Reference type parameters copy the reference value, aliasing the same object without copying the object itself.

- Mutating struct parameter fields mutates the copy only unless `ref`.
- Mutating object fields through reference parameter affects caller's object.
- Large structs use `in` for efficient readonly passing—Methods Q12 Module 01.
- Boxing copies struct to heap when passed as `object` or interface—Gotcha 5 Module 02.

---

#### Q17. What is the fragile base class problem at a high level?

**Answer:** The fragile base class problem occurs when a base class change (new virtual method, altered constructor sequence) breaks derived classes that relied on previous behavior, because subclasses are tightly coupled to base implementation details they do not control.

- Adding virtual calls in base constructor to overridable methods is especially dangerous—Gotcha 1 Module 02.
- Favor composition, sealed defaults, or careful virtual design to minimize surprise in derivatives.
- See Inheritance chapter Q12 for expanded discussion.
- Versioning public base classes in libraries requires extreme caution.

---

#### Q18. What is the difference between stack allocation (`stackalloc`, local structs) and heap allocation for objects?

**Answer:** `stackalloc` and local struct variables use stack or register storage scoped to the method invocation (with escape restrictions), while `new` on classes allocates on the heap with lifetime managed by garbage collection until unreachable.

- Stack memory is reclaimed when the method returns automatically—no GC.
- Heap objects survive until no references remain; finalizers run non-deterministically if present.
- `stackalloc` into `Span<T>` is idiomatic for temporary buffers in modern C#.
- Do not return references to stack memory that outlives the method—language rules prevent most cases.

---

#### Q19. What does `GC.GetTotalMemory` measure, and why is it only a rough indicator?

**Answer:** `GC.GetTotalMemory` returns an approximate number of bytes the garbage collector believes are allocated in managed heaps after optionally forcing a collection, useful for coarse diagnostics—not precise accounting of process working set or native memory.

- Passing `true` triggers collection before measure, skewing results toward post-GC state.
- Does not include unmanaged allocations, stack, or JIT code size.
- Production monitoring uses profilers and `dotnet-counters`, not ad hoc `GetTotalMemory` alone.
- Teaches that GC heap size differs from task manager process memory.

---

#### Q20. What is the difference between an anemic class (data-only) and a rich domain object?

**Answer:** An anemic class exposes data through getters and setters while behavior lives in external services, whereas a rich domain object encapsulates business rules and invariants alongside its data, enforcing valid states through methods and properties.

- Anemic models simplify CRUD and mapping layers but scatter domain logic across procedural code.
- Rich models align with encapsulation and reduce invalid state combinations if designed well.
- Neither is always wrong—reporting DTOs are intentionally anemic; core domain may be rich.
- See OOP Real-World Examples chapter Q6 on anemic domain anti-pattern.

---

### 02. Properties & Indexers

#### Q1. Explain properties and fields in C#.

**Answer:** Fields are variables declared directly on a type; properties are members with accessors that read or write backing state through methods disguised as field-like syntax. Properties enable validation, computed values, and versioning without changing public call sites.

- Auto-properties compile to hidden backing fields with trivial get/set.
- Fields cannot intercept assignment; properties can enforce invariants on set.
- Interface contracts use properties, not public fields, for consistency.
- See Classes Q11 for roles relative to methods.

---

#### Q2. What are auto-implemented properties?

**Answer:** Auto-implemented properties declare `{ get; set; }` without manual backing field code; the compiler generates a private hidden field and accessor methods automatically.

- Useful for DTOs and simple state when no validation is needed yet.
- Can use `{ get; private set; }` for restricted mutation from outside the type.
- Init-only `{ get; init; }` restricts assignment to construction phase—Q5.
- Upgrade to full property with backing field when validation becomes necessary—Q10.

---

#### Q3. What are indexers in C#?

**Answer:** Indexers are properties that accept parameters in square brackets (`this[int index]`, `this[string key]`) allowing instance syntax like `collection[i]` on custom types, implemented as get/set methods with parameters.

- Syntax mirrors arrays but defined on classes or structs implementing dictionaries, buffers, or matrices.
- Can overload on parameter types—Q8.
- Interfaces may declare indexers implemented explicitly or publicly—Q17.
- Distinct from methods named `GetByIndex` primarily by call-site syntax—Q16.

---

#### Q4. What is the difference between a `public` field and a `public` auto-property — if they behave similarly, why prefer properties?

**Answer:** At runtime both expose get/set-like access, but properties are methods in IL metadata, allowing future validation, computed backing, versioning, and data-binding conventions without breaking binary compatibility as easily as changing public fields.

- Reflection and serializers often treat properties as the public surface for serialization.
- Fields cannot be virtual; properties can be overridden with custom logic in derived classes.
- Public fields cannot intercept assignment for invariant checks without refactoring all call sites to methods.
- Encapsulation chapter expands API design rationale—chapter 07 Q11.

---

#### Q5. What are init-only properties (`get; init;`), and how do they differ from get-only and `{ get; set; }`?

**Answer:** Init-only properties allow assignment only during object construction—constructor body or object initializer—then become read-only afterward. Get-only properties without init may be set only in constructor or expression-bodied; `{ get; set; }` allows mutation any time.

- Init supports immutable object models with object initializer ergonomics.
- `{ get; private set; }` allows class methods to mutate after construction; init does not.
- Records use init properties heavily for positional semantics—Q14 preview.
- Gotcha 10 Module 02 contrasts init vs private set confusion.

---

#### Q6. What is the difference between `{ get; private set; }` and a property with only a public getter backed by a private setter method?

**Answer:** `{ get; private set; }` exposes a property whose setter is callable from any member of the declaring type. A public getter with private `SetName()` method restricts mutation to explicit methods, documenting which operations change state.

- Auto-property private set is concise for simple internal mutation from any instance method.
- Dedicated setter methods name the intent (`Promote()`, `Deactivate()`) and can carry parameters beyond single value assignment.
- Both hide public mutation; choose based on clarity of domain operations vs generic property set.
- Init-only properties differ from both—see Q5.

---

#### Q7. What are expression-bodied properties (`public string Label => $"{Title}";`)?

**Answer:** Expression-bodied properties use `=>` to define read-only properties computing a single expression without a braced get accessor block, reducing noise for derived values like formatted labels or boolean flags from other members.

- Must be read-only unless using `{ get => field; set => field = value; }` form for accessors in newer C#.
- Evaluated on each get access unless caching added in backing logic elsewhere.
- Keep expressions simple; complex logic belongs in methods or full property bodies.
- See Methods Q3 Module 01 for expression-bodied members generally.

---

#### Q8. Can indexers be overloaded — what distinguishes overloads?

**Answer:** Indexers overload by parameter signature—different parameter types, counts, or modifier combinations (`int` vs `string` key)—while sharing the `this[...]` name. Return types alone do not distinguish indexer overloads.

- Multi-dimensional indexers use multiple parameters: `this[int row, int col]`.
- Explicit interface indexers can implement interface indexer separately from public class indexer.
- Overloads must differ in parameter lists like methods.
- Compiler selects overload based on argument types at call site.

---

#### Q9. What is the syntax for an indexer (`this[int index]`, `this[string key]`)?

**Answer:** Indexers declare `public Type this[ParameterList] { get; set; }` where `this` keyword marks the indexer, parameters appear in brackets, and get/set accessors behave like property accessors with parameters.

- Parameter types define key or coordinate semantics (`string key`, `int index`).
- Read-only indexers omit set accessor.
- Default parameter values are not allowed on indexer parameters.
- Collection initializer syntax on custom types requires public `Add` or accessible indexer set.

---

#### Q10. When should you use a full property with validation vs an auto-property?

**Answer:** Use a full property with explicit backing field when assignment must validate ranges, normalize input, raise change notifications, or lazy-load expensive data. Use auto-properties when any valid value of the type is acceptable and no side effects are needed on get/set.

- Transition from auto to full property without changing public API surface beyond behavior.
- Throw `ArgumentOutOfRangeException` in set for invalid domain values.
- INotifyPropertyChanged implementations require full properties to invoke events on change.
- YAGNI: start auto, upgrade when rules appear—avoid premature validation boilerplate.

---

#### Q11. What is a computed/read-only property that derives its value from other members?

**Answer:** A read-only property calculates its return value from other fields or properties each time it is read, expressing derived state like `FullName => $"{First} {Last}"` or `IsAdult => Age >= 18` without storing redundant fields.

- Avoid side effects in getters; keep them predictable for debugging and binding.
- Cache in private field if computation is expensive and invalidation is manageable.
- Computed properties should not create inconsistent mutable state separate from source fields.
- Expression-bodied syntax common for simple computed properties—Q7.

---

#### Q12. What is the difference between `init` properties and constructor parameters for immutable objects?

**Answer:** Constructor parameters enforce required values at creation with explicit signature; init properties allow object initializer syntax and optional members while still preventing post-construction mutation. Records combine both with positional syntax.

- Constructors validate in one place; multiple constructor overloads may duplicate validation without `: this()`.
- Init properties suit many optional immutable fields with initializer ergonomics.
- Required members (C# 11+) annotate mandatory init properties compile-time.
- Choose constructor-only for small immutable types; init + initializer for many optional fields.

---

#### Q13. How do properties participate in object initializer syntax?

**Answer:** Object initializers assign to settable properties and fields after the constructor runs: `new Customer { Name = "Ada", Id = 1 }`. Init-only properties accept assignments there; get-only properties without init cannot be set in initializer.

- Order of initializer assignments follows source text; dependencies between properties should not assume order unless documented.
- Collection initializers target properties returning mutable collections or indexers.
- Constructor still establishes required invariants before initializer assignments execute.
- See Classes Q5 and Q14.

---

#### Q14. What is a preview-level understanding of `record` types and synthesized properties?

**Answer:** Records (C# 9+) are reference types (or struct records) with compiler-synthesized equality, `ToString`, and clone members, often using primary constructor parameters that become init or get-only properties for concise immutable data carriers.

- `record Person(string Name, int Age);` creates positional properties `Name` and `Age`.
- Value equality by default compares property values, not reference identity.
- `with` expressions create copies with selective property changes.
- Records suit DTOs and domain events; behavior-rich entities may remain classes.

---

#### Q15. Why might exposing a public `{ get; set; }` on a collection-typed property break encapsulation?

**Answer:** Callers can replace or mutate the internal collection without going through your type's methods, bypassing invariants like duplicate prevention, sorting, or synchronization—Gotcha 9 Module 02.

- Exposing `List<T>` allows `obj.Items.Clear()` from outside without your knowledge.
- Prefer `IReadOnlyList<T>` public get with private mutable backing list, or defensive copies on get.
- See Encapsulation chapter Q4 and Q9.
- Initialize collection properties to empty instances to avoid null reference on add.

---

#### Q16. What is the difference between an indexer and a method named `GetByIndex`?

**Answer:** Indexers use `obj[key]` syntax integrated with language indexing semantics and collection initializers; methods use `obj.GetByIndex(key)` explicit call syntax without participating in indexer language features.

- Indexers feel natural for collection-like types; methods clarify intent for non-collection lookups.
- Indexers cannot be extension members; methods can be extensions in static classes.
- Performance is equivalent; choice is API ergonomics and framework conventions (` IList<T>` uses indexer).
- Overloading rules differ slightly in discoverability for tooling.

---

#### Q17. Can interface types declare indexers, and how are they implemented?

**Answer:** Interfaces may declare indexers with get/set requirements; implementing classes provide `this[...]` accessors matching the contract, either publicly or through explicit interface implementation when name clashes occur.

- `interface IMap { string this[string key] { get; set; } }`
- Explicit implementation: `string IMap.this[string key] { get => ...; set => ...; }`
- Consumers typed as interface use indexer syntax through interface reference.
- Same explicit implementation hiding patterns as methods—Gotcha 12 Module 02.

---

#### Q18. What is the relationship between properties and data binding / serialization frameworks?

**Answer:** Data-binding (WPF, ASP.NET model binding) and serializers (System.Text.Json, XmlSerializer) typically discover public readable/writable properties by convention, ignoring fields unless configured otherwise.

- Missing public setters affects deserialization and two-way binding unless custom converters exist.
- `[JsonIgnore]` and similar attributes target properties to control serialization shape.
- Init-only properties work with serializers that support immutable object patterns in modern versions.
- Naming conventions (`Id`, `Name`) align with model binding from query strings and JSON bodies.

---

### 03. Constructors & Method Overloading

#### Q1. Explain constructors and their types in C# (default, parameterized, static, private).

**Answer:** Constructors initialize new instances; the compiler supplies a parameterless default if none is declared until you add any constructor. Parameterized constructors accept arguments; static constructors initialize type-wide static fields once per type; private constructors block external instantiation for factories or singletons.

- Instance constructors run on `new`; static constructors run before first use of the type.
- Constructor name matches class name with no return type.
- Struct parameterless constructors allowed from C# 10 with explicit rules—Q8.
- See Q10 for default constructor rules when none written.

---

#### Q2. What is a destructor/finalizer in C#?

**Answer:** A destructor (finalizer) uses `~ClassName()` syntax and runs non-deterministically when the garbage collector reclaims an object, allowing last-chance cleanup of unmanaged resources if `Dispose` was not called.

- Finalizers delay collection and add overhead—avoid unless wrapping unmanaged handles needing backup cleanup.
- Cannot call finalizer explicitly; runtime schedules it.
- Modern pattern pairs `IDisposable` with optional finalizer calling shared cleanup with `disposing` flag false.
- Gotcha 13 Module 02: do not rely on finalizers for timely release.

---

#### Q3. Explain constructor chaining in C# (`: this(...)` vs `: base(...)`).

**Answer:** `: this(...)` delegates to another constructor in the same class, running it before the current constructor body. `: base(...)` invokes a base class constructor before the derived constructor body executes, required when no default base constructor exists.

- Chain reduces duplicated initialization logic across overloads—Q12.
- Base chain must eventually reach `Object()` constructor.
- Cannot have both `: this` and `: base` on same constructor—choose one delegation path per constructor header.
- Order ensures base fields initialized before derived fields use them—Q5 and Q13.

---

#### Q4. How can you call the base class constructor from a derived class?

**Answer:** Include `: base(arguments)` in the derived constructor declaration before the body; if omitted and base has parameterless constructor, compiler inserts `: base()` implicitly.

- When base lacks parameterless constructor, every derived constructor must explicitly call a base constructor with matching arguments.
- Base constructor runs completely before derived constructor body starts.
- Cannot call base constructor from body—only constructor initializer syntax.
- See Inheritance chapter for interaction with virtual methods during base construction—Gotcha 1 Module 02.

---

#### Q5. In what order do constructors and field initializers run in an inheritance chain?

**Answer:** From root base to most derived: static constructor (once per type when needed), instance field initializers, instance constructor body at each level, with derived always completing base portion first via `: base()` chain before its own field initializers and body in the derived type's sequence after base returns.

- Simplified per type: field initializers then constructor body for that type, but base type fully constructs before derived field initializers run.
- Exact rules in Q13 enumerate static vs instance nuances.
- Virtual calls from base constructor see derived overrides before derived fields initialized—Gotcha 1.
- Static fields initialize before static constructor runs field initializers at type load.

---

#### Q6. Explain method overloading and method overriding in C#.

**Answer:** Overloading defines multiple methods with the same name but different parameter lists in the same type, resolved at compile time. Overriding replaces a base virtual method in a derived class with `override`, resolved at runtime based on actual object type.

- Overload differs by parameters, not return type alone—Module 01 Methods Q9.
- Override requires `virtual`/`abstract` base and `override` derived; `sealed override` stops further override.
- Hiding with `new` is compile-time dispatch—Gotcha 2 Module 02.
- See Q14 for compile-time vs runtime distinction.

---

#### Q7. What is a static constructor, and when does it run?

**Answer:** A static constructor (`static ClassName()`) initializes static fields before first static member access or first instance creation, executed at most once per type per application domain in a thread-safe manner by the runtime.

- No parameters or access modifiers allowed; runs before static field initializers of that type complete per spec ordering with field initializers.
- Cannot be called directly.
- Expensive static initialization delays first use of type.
- Chapter 04 expands static constructor details.

---

#### Q8. Can a struct have a parameterless constructor (C# 10+ rules vs earlier)?

**Answer:** Before C# 10, structs implicitly always had a parameterless default zeroing constructor that could not be explicitly declared. C# 10+ allows explicit parameterless struct constructors that must fully assign all fields, coexisting with the implicit default unless explicitly declared.

- Explicit parameterless struct ctor must assign all fields before any use.
- `new()` on struct still zero-initializes unless custom parameterless runs when invoked.
- Breaking change awareness when adding explicit parameterless to existing structs consumed as defaults.
- Classes differ: adding any instance constructor removes compiler-generated parameterless unless `new()` constraint patterns differ.

---

#### Q9. What is the difference between a primary constructor (C# 12 on classes/records) and traditional constructors?

**Answer:** Primary constructors declare parameters in the class/record header (`class Person(string name)`) capturing parameters into the type scope for field/property initialization and member use, reducing boilerplate compared to separate constructor body assignments.

- Parameters may become captured fields or feed explicit properties in records.
- Traditional constructors offer explicit control flow, validation throws, and multiple overload chains.
- Primary constructors interact with inheritance rules—derived types must chain appropriately in C# 12 patterns.
- Use primary constructors for concise DTOs; use traditional when complex validation or multiple overloads dominate.

---

#### Q10. What happens if you do not define any constructor — what default constructor is provided?

**Answer:** The compiler emits a public parameterless instance constructor that calls base parameterless constructor if you declare no instance constructors. Once you define any instance constructor, the default parameterless is not generated unless you declare it explicitly.

- Struct implicit parameterless zeroes fields unless custom parameterless added (C# 10+).
- Static classes never have instance constructors.
- Private parameterless can be declared for controlled factory patterns—Q11.
- Customer example in tutorial: adding parameterized constructor removed implicit default until explicit parameterless added if needed.

---

#### Q11. Why might you mark a constructor `private` (singleton, factory patterns)?

**Answer:** Private constructors prevent external `new`, forcing creation through static factory methods or singleton accessors that control instance count, caching, or substitution in tests when combined with internal instance creation paths.

- Singleton anti-pattern risks global state—Gotcha 11 prefers DI-managed singletons.
- Factory methods (`CreateFromConfig`) can return subclasses or pooled instances without exposing constructors.
- Nested class can access private constructor for builder patterns.
- Serialization and reflection may bypass private constructors with special attributes—advanced scenarios.

---

#### Q12. What is constructor overloading, and how does `: this(...)` reduce duplication?

**Answer:** Constructor overloading provides multiple signatures (`Customer()`, `Customer(string name)`) for different creation scenarios. `: this(...)` forwards common initialization to one primary constructor, keeping validation and field assignment in a single place.

- Example: parameterless calls `: this("Default")` then only one body assigns fields.
- Avoids copy-paste field assignments across overloads.
- Chain must not cycle infinitely.
- Combine with optional parameters carefully to avoid ambiguity with many overloads.

---

#### Q13. What is the exact order: static constructor, instance field initializers, instance constructor body, base constructor?

**Answer:** For first use of a type: static field initializers then static constructor; for `new Derived()`: base static init as needed, derived static init, base field initializers, base constructor body, derived field initializers, derived constructor body—with `: base()` invoking base constructor before derived field initializers on the derived type.

- Static initialization runs once before any instance of that type is created.
- Within each constructor call, field initializers of that type run before its constructor body statements after base constructor completes for derived types.
- Language specification precise ordering is interview depth; remember base-before-derived and initializers-before-body per level after base returns.
- Virtual method calls during base constructor are hazardous—Gotcha 1.

---

#### Q14. What is the difference between method overloading (compile-time) and method overriding (runtime polymorphism)?

**Answer:** Overload resolution picks the best matching signature at compile time based on argument types. Override dispatch calls the most derived override of a virtual method at runtime based on the actual object type, even when referenced through a base-typed variable.

- Overloading: `Print(int)` vs `Print(string)` chosen by compiler.
- Overriding: `Base b = new Derived(); b.VirtualMethod()` calls `Derived.VirtualMethod`.
- `new` hiding is compile-time on variable static type—distinct from override.
- See Inheritance Q14–Q15.

---

#### Q15. When does the compiler fail to pick an overload due to ambiguity involving optional parameters and `params`?

**Answer:** Ambiguity arises when two overloads are equally good matches, such as an exact array parameter overload vs a `params` expansion, or optional parameter overload vs explicit argument count matching another overload, producing CS0121.

- Adding `params` increases chance of conflict with explicit array overload—Module 01 Methods Q15–Q16.
- Cast arguments or rename methods to disambiguate public APIs.
- Optional parameters baked at compile time can shift which overload applies after recompile—Gotcha 13 Module 01.
- Prefer clear overload sets in public APIs with minimal optional/params overlap.

---

#### Q16. Can constructors be inherited — how does a derived class get a base constructor?

**Answer:** Constructors are not inherited; a derived class must declare constructors that chain to accessible base constructors with `: base(...)`. If base has only protected internal constructor, derived in another assembly may be blocked.

- Compiler does not generate derived constructor automatically except default parameterless calling base parameterless when both exist.
- Abstract base may declare protected constructor for derived use only.
- Copy patterns use `: base(copyFieldValues)` from copy constructors manually written.
- Primary constructors in C# 12 still require thoughtful base chaining in inheritance.

---

#### Q17. What validation belongs in a constructor vs a factory method?

**Answer:** Constructors should enforce invariants required for any valid instance (non-null name, positive id). Factory methods handle optional creation paths, parsing, caching, returning interface types, or failures that should not throw from constructor when TryCreate pattern preferred.

- Heavy I/O or service lookups belong in factories or async factory (`CreateAsync`), not constructors.
- `ArgumentException` in constructor fails object creation clearly.
- Static `TryCreate` mirrors parse Try-pattern for expected invalid input combinations.
- DI containers often use constructors for required dependencies exclusively.

---

#### Q18. What is the difference between calling an overloaded instance method vs a static overloaded method?

**Answer:** Both use compile-time overload resolution with the same rules, but instance overloads receive implicit `this` and require an instance reference, while static overloads are called on the type name without instance and cannot access instance members.

- Static overload resolution ignores instance type except for generic type parameters on static generic methods.
- Extension methods desugar to static calls with first parameter as instance.
- Ambiguity resolution identical—casts and named arguments apply equally.
- Cannot call instance overload without object; static overloads callable with type name even when instance exists (discouraged style if instance overload also exists hiding intent).

---

### 04. Static Members & Static Classes

#### Q1. Explain the `static` keyword in detail.

**Answer:** `static` marks members that belong to the type itself rather than any instance—shared across all objects and callable without `new`. Static fields, properties, methods, constructors, and nested types participate in type-level lifetime and initialization rules distinct from per-instance state.

- Instance methods receive implicit `this`; static methods do not and cannot access instance members without an object reference.
- Static fields initialize before first use; mutable static state requires thread-safety consideration—Q11.
- Constants are implicitly static.
- Overusing static complicates testing—Q12.

---

#### Q2. What is a static class in C#?

**Answer:** A static class is declared `static class Utilities` and is sealed, cannot be instantiated, and must contain only static members. It groups related helpers like extension method containers or math utilities without instance state.

- Compile error on `new` or instance members without static modifier.
- Cannot implement interfaces on static class in C#.
- Differs from singleton instance class—Q4 and Gotcha 11 Module 02.
- `Program` with top-level statements generates implicit static Program class in modern templates.

---

#### Q3. Why can you not override a `static` method?

**Answer:** Polymorphism applies to instance methods via virtual dispatch; static methods belong to the type and are bound at compile time based on the static type name used in the call, not the runtime object. Therefore `override` does not apply—use `new` to hide a base static method with misleading but legal syntax.

- Calling `Base.StaticM()` vs `Derived.StaticM()` resolved by compile-time type of reference used (`Derived.StaticM()` even if variable typed as Base but called as Derived method name).
- No virtual static methods in classic C# (static abstract interface members in C# 11 are different pattern for interfaces).
- Design static methods as non-polymorphic utilities with clear type ownership.
- See Inheritance Q14 on static vs instance inheritance participation.

---

#### Q4. What is the difference between a static class and the singleton pattern?

**Answer:** A static class enforces type-level-only access with no instance, while singleton pattern provides exactly one instance object often accessed via static property, allowing instance interfaces, inheritance, and lazy initialization with locking.

- Static class cannot implement interfaces or participate in instance DI as easily.
- Singleton instance can be mocked if interface-based and lifetime managed by container—Gotcha 11.
- Both represent global access points; both risk hidden dependencies in tests.
- Prefer DI-scoped or singleton services over static globals in application code.

---

#### Q5. What is a static field, and how is lifetime different from an instance field?

**Answer:** Static fields exist one per type per application domain for the process lifetime (until unload), shared by all instances. Instance fields exist per object and are garbage-collected when the object becomes unreachable.

- Static fields initialize to default before static constructor; instance fields per object on heap inside object layout.
- Mutable static fields are global variables—thread safety required for concurrent updates.
- `CustomerCount` tutorial example increments shared counter on each `new Customer()`.
- Static fields can outlive all instances and hold references preventing GC of related graphs—memory leak risk.

---

#### Q6. What is a static property and static method — what is the `this` reference inside them?

**Answer:** Static properties and methods belong to the type; there is no instance `this` inside static methods. They access only static members directly unless given an instance parameter explicitly.

- Static property wraps shared configuration or counters with get/set like instance properties but single storage.
- Extension methods are static methods with special first parameter syntax.
- Calling static method via instance reference (`obj.StaticHelper()`) compiles but is misleading style—analyze as type call.
- Static methods cannot be virtual in traditional class inheritance sense—Q3.

---

#### Q7. Why can static methods not access instance members directly?

**Answer:** Instance members require an object identity (`this`) for field access and virtual dispatch; static methods run without an instance context, so the compiler disallows direct access to instance fields or methods unless an instance is passed as a parameter.

- `static void M() { this.field; }` is compile error CS0026.
- Static method may create local instance and then access instance members on that object.
- Logical design: operations not tied to object state should be static; stateful operations should be instance methods.
- Constructors are instance members; static factory methods can call `new` to create instances.

---

#### Q8. When are static constructors executed, and how many times per AppDomain/process?

**Answer:** Static constructors run once per closed type when the type is first used—first static member access or first instance creation—before any static or instance member of that type executes, guarded by runtime for thread-safe single initialization per load context.

- In modern .NET, load context aligns closely with process for most apps; AppDomain term is historical except Framework isolation scenarios.
- Order among static constructors of unrelated types is not guaranteed.
- Infinite recursion in static ctor causes `TypeInitializationException`.
- See Constructors Q7 and Q13 for ordering with field initializers.

---

#### Q9. What is the difference between `const` (implicitly static) and `static readonly`?

**Answer:** `const` values are compile-time literals, implicitly static, embedded in metadata. `static readonly` fields assign at runtime in static constructor or inline initializer and can hold runtime-computed values.

- `const int Max = 100;` vs `static readonly DateTime Epoch = DateTime.UnixEpoch;`
- `const` cannot be `DateTime.Now`; `static readonly` can.
- Changing `const` requires recompiling consumers; changing `static readonly` value affects new runs after deploy without recompiling callers of the field (field read at runtime).
- See Module 01 Data Types Q8–Q9.

---

#### Q10. Can a static class implement interfaces?

**Answer:** No—C# disallows interface implementation on static classes because interfaces require instance methods that implement contracts via objects, and static classes cannot be instantiated.

- Use regular class with instance implementing interface if polymorphism needed.
- Static abstract interface members (C# 11) apply to instances implementing interfaces on structs/classes, not static classes as a whole.
- Extension methods provide static-class-like syntax for interface augmentation without implementing on static class.
- Utility static classes expose only concrete static helpers.

---

#### Q11. What thread-safety concerns apply to mutable static fields?

**Answer:** Mutable static fields are shared across all threads; concurrent read/write without synchronization causes race conditions, torn reads, and lost updates. Use locks, `Interlocked` operations, or immutable static initialization for safe sharing.

- Lazy initialization of static singleton may double-create without `Lazy<T>` or lock.
- Prefer immutable static data (`static readonly` initialized once) when possible.
- Static event handlers and caches cause classic memory leaks holding short-lived objects—Gotcha 8 Module 02.
- `ConcurrentDictionary` for shared caches instead of Dictionary without lock.

---

#### Q12. Why is overusing static state a testing and maintainability problem?

**Answer:** Static dependencies hide constructor injection, persist state across tests unless reset, prevent mocking interfaces, and create hidden coupling between unrelated components through global mutable fields.

- Tests run in parallel may flake when sharing mutable static counters or caches.
- Service locator static `Instance` properties hinder replacing implementations in unit tests.
- Static is fine for pure functions and true constants; application services should be instance-scoped in DI.
- Gotcha 11 contrasts static singleton vs DI singleton testability.

---

#### Q13. What is the difference between static nested classes and non-static nested classes?

**Answer:** Static nested classes do not capture an implicit reference to the outer instance; inner (non-static) nested classes hold a hidden reference to outer `this` and can access outer instance members directly.

- Static nested class behaves like namespace grouping with access to outer private members if in same outer class file scope rules apply (nested types access outer private members).
- Inner class instance requires outer instance: `outer.Inner inner = outer.new Inner();`
- Static nested useful for builder implementation details without outer reference retention—prevents leak.
- Event handler patterns sometimes use nested classes to capture context deliberately.

---

#### Q14. How do static members participate in inheritance — are they polymorphic?

**Answer:** Static members are not polymorphic; derived types can `new`-hide base static members but calls bind using the compile-time type of the qualifier used (`Base.M()` vs `Derived.M()`). Instance virtual methods remain the polymorphic mechanism.

- Inheritance of static members is naming inheritance only, not runtime override.
- Access static members through declaring type name for clarity.
- Interface static abstract methods (modern) are separate from class static inheritance model.
- See Inheritance Q3 and Q15 on hiding vs override distinction for instance methods.

---

### 05. Inheritance & Polymorphism

#### Q1. Explain inheritance in detail in C#.

**Answer:** Inheritance lets a derived class extend a base class, inheriting instance and static members (with accessibility limits) and adding or replacing behavior. C# supports single inheritance of classes plus multiple interface implementation.

- Base class can define virtual methods for override; sealed base stops further derivation.
- Protected members visible to derived classes enable extension while hiding from unrelated code.
- Constructor chaining ensures base initialization—Constructors chapter.
- Favor composition when inheritance only reuses implementation without true is-a relationship—Q13.

---

#### Q2. Explain polymorphism in C# and how it can be achieved.

**Answer:** Polymorphism allows code to operate on abstractions (base class or interface references) while runtime behavior comes from the actual derived type. C# achieves it via virtual method overriding, interface implementation, and implicit interface dispatch.

- `Animal a = new Dog(); a.Speak()` calls `Dog.Speak` if `Speak` is virtual/override.
- Interfaces enable polymorphism without shared base class implementation.
- Pattern matching and switch on type complement but do not replace virtual design for open hierarchies.
- Gotcha 7 warns against long type-check chains defeating polymorphism.

---

#### Q3. What is the difference between compile-time (static) and runtime (dynamic) polymorphism?

**Answer:** Compile-time polymorphism includes method overloading and `new` method hiding—resolved from static type at compile time. Runtime polymorphism uses `virtual`/`override` dispatch based on actual object type, and interface calls through implementing instances.

- Overload: chosen by argument types at compile time.
- Override: `Base b = new Derived(); b.V()` calls Derived at runtime.
- `dynamic` keyword adds runtime binding for member resolution beyond inheritance—Module 01 Q25.
- Default interface methods dispatch with rules for struct boxing—Gotcha 15 Module 02.

---

#### Q4. What is a sealed class in C#?

**Answer:** A `sealed class` cannot be inherited; `sealed override` on a method prevents further overrides in derived classes. Sealing documents final implementation and enables runtime devirtualization optimizations in some cases.

- `string` and many BCL types are sealed for security and invariant preservation.
- Seal classes when extension via inheritance would break invariants (security, correctness).
- Cannot derive from sealed class—Q16.
- Sealing is optional design choice vs `virtual` extensibility trade-off.

---

#### Q5. What is a virtual method in C#?

**Answer:** A `virtual` method in a base class provides a default implementation that derived classes may replace with `override`, enabling runtime dispatch to the most derived override through a base-typed reference.

- Without `virtual`, methods are non-virtual by default in C# (unlike Java instance methods).
- Virtual methods participate in inheritance chains; `abstract` virtual requires override in derived non-abstract class.
- Calling virtual methods from constructor sees derived overrides before derived initialization completes—Gotcha 1.
- Performance: JIT can devirtualize sealed or known types in optimized tiers.

---

#### Q6. What is the difference between `this` and `base` keywords?

**Answer:** `this` refers to the current instance for disambiguation and passing self; `base` accesses base class members hidden by derived declarations, especially calling `base.Method()` to run parent implementation before or after derived logic.

- `base` in constructor initializer calls base constructor—Constructors Q4.
- `base.Method()` invokes base virtual method even when derived overrides—does not dynamically dispatch to derived when explicitly qualified with `base`.
- `this()` chains constructors on same class.
- Static context has no `this`; `base` only in instance members of derived class.

---

#### Q7. What is operator overloading in C#?

**Answer:** Operator overloading defines static methods with `operator` keyword so expressions like `a + b` compile for user-defined types when overloads exist, subject to language rules on which operators are overloadable.

- Must declare public static overloads; some operators require paired overloads (`==` and `!=`).
- Cannot overload `&&` `||` as user operators though `true`/`false` unary operators enable short-circuit patterns for custom types in limited scenarios.
- Use sparingly for domain types (vectors, money) with intuitive semantics.
- Inconsistent equality operators break collections—Gotcha 14 Module 02.

---

#### Q8. Explain the difference between `virtual`, `abstract`, and `override` keywords.

**Answer:** `virtual` provides overridable default implementation in concrete base class. `abstract` on class or method requires derived non-abstract class to implement (no body on abstract method). `override` replaces inherited virtual or abstract method in derived class.

| Keyword | On class | On method |
|---|---|---|
| `virtual` | — | Optional override with default body |
| `abstract` | Class cannot instantiate | No body; must override in derived |
| `override` | — | Replaces base virtual/abstract |

Abstract class can mix concrete and abstract methods—chapter 06 Q9.

---

#### Q9. Explain the `new` keyword in the context of method hiding.

**Answer:** `new` on a derived method hides a base method with the same signature without overriding; dispatch when calling through base-typed reference uses base method unless static type of reference is derived.

- Does not participate in runtime polymorphism like `override`.
- Warning CS0108 if hiding without `new` keyword—compiler suggests `new`.
- Gotcha 2: mixing hide and override breaks expectations when calling through base type.
- Use `override` when polymorphism intended; `new` when base API should remain unchanged for base references.

---

#### Q10. Explain how C# handles multiple inheritance (using interfaces).

**Answer:** C# allows a class to inherit one base class at most but implement multiple interfaces, gaining polymorphic contracts from each interface without merging implementation from multiple class hierarchies.

- `class Worker : Person, IEmployable, IPayable` inherits Person once and implements both interfaces.
- Interface methods implemented explicitly or publicly on the class.
- Default interface methods (C# 8+) supply shared implementation on interfaces without class base duplication.
- Diamond problem for classes avoided; interfaces with default methods have resolution rules—chapter 06 Q12.

---

#### Q11. Why does C# not support multiple inheritance of classes?

**Answer:** Multiple class inheritance complicates object layout, virtual dispatch, constructor chaining, and the diamond problem where two base classes provide conflicting implementations of the same method. C# chose single inheritance plus interfaces for clarity and predictable memory layout.

- COM and CLR object model simplified with single inheritance chain.
- Composition and interfaces cover most multiple reuse scenarios without MI complexity.
- Languages with MI require complex resolution rules C# designers avoided.
- See Q13 favor composition guideline.

---

#### Q12. What is the fragile base class problem?

**Answer:** Derived classes depend on base class implementation details; changes in base (new virtual methods, altered sequence) break subclasses unexpectedly. Virtual calls from base constructors exacerbate the issue—Gotcha 1.

- Mitigate by sealing classes, minimizing virtual surface, documenting extension points, using composition.
- Framework authors treat unsealed public classes as extensibility contracts with versioning cost.
- See Classes Q17 high-level summary.
- Unit tests on derived classes may fail when base library updates silently change behavior.

---

#### Q13. Why is "favor composition over inheritance" a common guideline?

**Answer:** Composition builds types by containing helper objects and delegating behavior, avoiding tight coupling to base class implementation and fragile override chains. Inheritance exposes derived classes to base changes and deep hierarchy maintenance costs.

- Wrapper pattern (`class LoggingRepository : IRepository` delegating to inner repo) swaps behavior without subclassing concrete base.
- Inheritance suits true is-a polymorphic relationships with stable abstractions.
- Deep inheritance trees obscure where behavior originates.
- Strategy pattern uses composition with interfaces—OOP Examples Q11.

---

#### Q14. What is runtime dispatch — how does the CLR resolve `override` calls through a base reference?

**Answer:** For virtual calls, the CLR uses the method table of the actual object type at runtime to locate the most derived override, ignoring the compile-time static type of the reference for instance virtual methods.

- Each object header points to method table with slot for virtual methods; override replaces slot in derived table layout.
- Non-virtual calls bind to compile-time type method directly without virtual indirection.
- `callvirt` IL instruction enforces virtual dispatch and null check on instance.
- Sealed override enables devirtualization optimization when JIT proves final type.

---

#### Q15. What is the difference between hiding with `new` and overriding with `override` when calling through a base-typed variable?

**Answer:** With `override`, `Base b = new Derived(); b.M()` calls `Derived.M` at runtime. With `new` hiding, same call invokes `Base.M` because dispatch uses static type of reference `Base` for non-virtual hidden method.

- Gotcha 2 is core interview trap.
- Explicit cast to `Derived` calls hidden method on derived: `((Derived)b).M()`.
- Polymorphic designs should use `virtual`/`override`, not `new`.
- Interface implementation always uses runtime type of implementing object for interface calls.

---

#### Q16. Can you inherit from a sealed class?

**Answer:** No—sealed classes cannot serve as base classes; attempting to derive produces compile error CS0509.

- Sealed types include `string`, `Enum`, and many BCL security-sensitive classes.
- Seal when extension would violate invariants or security assumptions.
- Use interfaces or composition to extend behavior of sealed types.
- `sealed override` on method stops further override but class itself may still be subclassed unless class is sealed.

---

#### Q17. What is the difference between `is` type testing and casting in polymorphic code paths?

**Answer:** `is` checks compatibility and supports patterns without throwing; casting `(Derived)b` throws `InvalidCastException` on failure. In polymorphic code, prefer `is` patterns or virtual methods over repeated casts.

- `if (b is Dog d)` assigns typed variable on success.
- Cast required when you know type after guard or for value types unboxing.
- Excessive type tests suggest missing virtual abstraction—Gotcha 7.
- Switch expressions on type patterns scale better than cast chains.

---

#### Q18. What is the Liskov Substitution Principle in one sentence, and how does it relate to inheritance?

**Answer:** Liskov Substitution Principle states that objects of a derived class must be usable anywhere their base class is expected without breaking correctness—derived types must honor the base contract, not strengthen preconditions or weaken postconditions.

- Violation example: `Square`/`Rectangle` with settable width/height breaking area invariants—OOP Examples Q2.
- Inheritance implies substitutability; if derived breaks callers of base, inheritance was wrong model.
- Prefer interfaces defining minimal contracts derived types can reliably fulfill.
- Related to polymorphism safety in tests using mocks substituting real implementations.

---

#### Q19. When does `base.Method()` call the parent's implementation vs the current type's override?

**Answer:** `base.Method()` explicitly invokes the base class's method implementation for that virtual method, bypassing the derived override for that call site even though the object is derived. Normal virtual call without `base` uses most derived override.

- Useful when derived override extends rather than replaces base behavior (call base first).
- `base` qualified calls are non-virtual dispatch to immediate base implementation in the inheritance chain step.
- Differs from calling through base-typed reference with hidden `new` methods—Q15.
- Constructor cannot call overridable virtual methods safely before derived init—Gotcha 1.

---

#### Q20. What is the difference between extending behavior with inheritance vs wrapping with composition?

**Answer:** Inheritance extends by substituting a subtype that IS-A base, overriding virtual methods for changed behavior. Composition wraps an inner object HAS-A collaborator, forwarding calls and optionally intercepting without subclassing the inner type.

- Inheritance couples to base implementation; composition couples to interface of inner object swappable at runtime.
- Decorator pattern uses composition to stack behaviors.
- Inheritance depth increases fragile base risk; composition localizes changes.
- OOP Examples chapter SOLID guidance reinforces composition for extension—Q5–Q7.

---

### 06. Abstract Classes & Interfaces

#### Q1. Explain abstraction in detail in C#.

**Answer:** Abstraction exposes essential behavior and data through types while hiding implementation details behind methods, properties, and contracts. Users depend on what a type does, not how it stores state or performs algorithms internally.

- Abstract classes and interfaces define partial or full contracts without exposing every helper method.
- Public API surface is smaller than internal implementation graph (private methods, helpers).
- Abstraction enables swapping implementations in tests and production via interfaces.
- Differs from encapsulation (access control) though they work together—Q2.

---

#### Q2. What is the difference between abstraction and encapsulation?

**Answer:** Abstraction simplifies the model by showing only relevant operations; encapsulation hides internal state and requires interaction through controlled members (private fields, public properties). Abstraction is about conceptual level; encapsulation is about access boundaries.

- Interface `IRepository` abstracts persistence; class encapsulates connection string and SQL details privately.
- You can have encapsulation without high-level abstraction (simple class with private fields).
- You can abstract via interface while implementation remains well-encapsulated internally.
- Module 02 chapter 07 focuses encapsulation mechanics.

---

#### Q3. What is the difference between abstraction and polymorphism?

**Answer:** Abstraction defines the simplified contract or base shape; polymorphism is the runtime mechanism where different concrete types fulfill that contract interchangeably through virtual methods or interface dispatch.

- Abstraction answers "what operations exist"; polymorphism answers "which implementation runs for this object."
- Interface is abstraction artifact; calling through `IEnumerable` with multiple concrete enumerators is polymorphism.
- Abstract class combines shared abstraction with optional default implementation.
- Both support Open/Closed Principle extension—OOP Examples Q7.

---

#### Q4. What is the difference between an abstract class and an interface?

**Answer:** An abstract class is a single-inheritance base that can include fields, constructors, access modifiers, and mix abstract and concrete members. An interface is a contract of members (methods, properties, events, indexers) a class struct implements, traditionally without implementation until default interface methods.

| | Abstract class | Interface |
|---|---|---|
| Inheritance | One base class | Multiple interfaces |
| State | Can have fields | No instance fields (static fields limited post-C# 8) |
| Constructors | Yes | No instance constructors |
| Default implementation | Concrete methods in class | Default interface methods (C# 8+) |

Choose abstract class for shared state and base logic; interface for cross-cutting contracts.

---

#### Q5. What is the difference between an abstract class and an interface before C# 8 vs after (default interface methods)?

**Answer:** Before C# 8, interfaces declared members without bodies; implementing types supplied all code. After C# 8, interfaces may include default method bodies, static members, and access modifiers, blurring the line while classes still allow single inheritance of implementation.

- Default methods let evolve interfaces without breaking all implementers immediately.
- Implementers can override default methods explicitly when needed.
- Abstract classes still unique for protected state sharing and constructor chains.
- Diamond conflicts possible with multiple default methods—Q12.

---

#### Q6. Why do we need interfaces in C#?

**Answer:** Interfaces define polymorphic contracts decoupled from inheritance hierarchies, enabling multiple roles on one class, test doubles (mocks), and dependency injection without forcing a common base class.

- `IDisposable`, `IEnumerable<T>`, `IComparable<T>` cross-cut unrelated types.
- DI containers resolve `ILogger` to concrete logger at runtime.
- Interface segregation keeps APIs minimal—OOP Examples Q9.
- Prefer parameter types as interfaces (`IReadOnlyList<T>`) over concrete lists—Q15.

---

#### Q7. What is explicit interface implementation and when is it used?

**Answer:** Explicit interface implementation names the interface on the member (`void IDisposable.Dispose()`), making it callable primarily when cast to the interface, hiding it from public class API surface to resolve name collisions or keep interface methods internal to polymorphic use.

- `((IDisposable)obj).Dispose()` works; `obj.Dispose()` may not if explicit and class has no public Dispose.
- Useful when two interfaces define conflicting method signatures.
- Gotcha 12: public class method and explicit interface method can coexist with different behavior.
- Class can implement interface privately via explicit implementation without public exposure.

---

#### Q8. What are static abstract members in interfaces (C# 11)?

**Answer:** C# 11 allows `static abstract` methods and operators in interfaces, enabling generic algorithms constrained on interfaces to call static members on type parameters (for example generic math interfaces).

- Implementing struct or class must provide static member implementations.
- Enables pattern like `static abstract T operator +(T left, T right)` in numeric interface constraints.
- Struct implementations may box when invoking default interface instance methods—Gotcha 15.
- Advanced feature for library authors more than everyday application code.

---

#### Q9. Can an abstract class have concrete (non-abstract) methods?

**Answer:** Yes—abstract classes often provide shared concrete helper methods and default behavior while declaring abstract members derived classes must implement.

- Template method pattern: concrete base method calls abstract hooks implemented in derived classes.
- Cannot instantiate abstract class until concrete derived class exists.
- Mix reduces duplication compared to pure interface with no shared code.
- Choose abstract base when several implementers share substantial code.

---

#### Q10. Can a class implement multiple interfaces — what about an interface inheriting another interface?

**Answer:** A class implements multiple interfaces in the base list comma-separated. Interfaces can extend other interfaces, inheriting member requirements so implementer must satisfy entire chain.

- `interface IAdmin : IUser, IAuditable { }` aggregates contracts.
- Class `class Employee : Person, IUser, IEmployable` single class base plus multiple interfaces.
- Explicit implementation can map one method to multiple interface requirements when signatures align.
- No limit on interface count beyond maintainability concerns.

---

#### Q11. When would you choose an abstract base class over an interface for shared implementation?

**Answer:** Choose abstract base when implementers share significant fields, constructors, protected helpers, or non-public state, and you want one inheritance slot consumed for that shared code. Choose interface when unrelated types need the same contract without shared base implementation.

- Abstract base suits domain entity hierarchies with common identity fields.
- Interface suits repository, logging, and strategy abstractions across unrelated classes.
- Versioning abstract base changes ripple to all derivatives; interface default methods offer incremental evolution.
- Some teams use abstract base implementing interface for template + contract combo.

---

#### Q12. What is the diamond problem, and how does C# avoid it for classes but address it for interfaces with default methods?

**Answer:** Diamond problem arises when two bases provide the same method and a derived type inherits both, creating ambiguity. C# avoids class multiple inheritance entirely. For interfaces with default methods, the most specific implementing type must override conflicting defaults or explicitly resolve which interface method to call.

- Class inheritance chain is linear—no diamond at class level.
- Two interfaces with same signature default methods require class override or explicit qualification.
- Explicit interface implementation can disambiguate calls.
- Design default methods carefully to avoid conflicting defaults on common combinations.

---

#### Q13. What is explicit interface implementation — why might `((IMyInterface)obj).Method()` work when `obj.Method()` does not?

**Answer:** Explicit implementation keeps interface member off the public class surface unless cast to interface, so direct call on class typed variable fails to resolve while interface cast exposes the member.

- Supports hiding implementation details from public API while satisfying interface for polymorphic consumers.
- Also resolves name collisions between interface and class methods with same signature goals.
- See Q7 and Gotcha 12 Module 02.
- Tooling and discoverability reduced for explicit members—document interface usage.

---

#### Q14. Can interfaces declare fields, constructors, or static concrete state (pre- and post-C# 8)?

**Answer:** Classic interfaces had no fields or constructors. C# 8+ allows static fields, static methods, and default instance methods on interfaces, but still no instance fields or instance constructors on interfaces themselves.

- Static fields on interface are rare and shared per interface type semantics.
- Implementing types hold instance state, not the interface type.
- Constants in interfaces were always allowed as static abstract members conceptually.
- Constructors belong to implementing classes only.

---

#### Q15. What is the difference between `IReadOnlyList<T>` as a parameter type and `List<T>` for abstraction?

**Answer:** `IReadOnlyList<T>` documents that callee will not mutate the collection through that parameter, accepting arrays, lists, and other read-only wrappers. `List<T>` as parameter exposes mutable API surface and prevents callers from passing read-only views without copying.

- Return `IReadOnlyList<T>` or `IEnumerable<T>` from properties to protect encapsulation—Gotcha 9 Module 02.
- Callee needing add/remove should document mutability requirements explicitly or take `ICollection<T>`.
- Liskov: passing `List<T>` where read-only expected allows callee to cast and mutate—prefer minimal interface.
- LINQ often returns materialized lists; accept read-only abstraction at API boundary.

---

#### Q16. When should API surface depend on interfaces vs abstract classes?

**Answer:** Public APIs consumed by many unrelated types should depend on interfaces for flexibility and test substitution. Abstract classes fit framework extension points where shared implementation and protected hooks reduce duplication among a known family of derivatives.

- Library NuGet packages expose interfaces to avoid forcing base class inheritance choice.
- ASP.NET Core and EF extensibility mix abstract bases (ControllerBase) with interface options (middleware).
- Combine: interface as contract, optional abstract base implementing interface for convenience.
- Versioning favors interfaces with default methods over deep abstract hierarchies when evolving.

---

### 07. Encapsulation & Access Modifiers

#### Q1. Explain encapsulation in C# with examples.

**Answer:** Encapsulation bundles data with methods that enforce invariants, hiding internal representation behind private fields and exposing controlled properties or methods. Callers cannot set invalid state directly if validation lives in property setters or domain methods.

- Bank account exposes `Deposit(decimal)` instead of public balance field to enforce non-negative rules.
- Private `_connection` with public `Connect()` method hides socket details.
- Encapsulation supports changing internal storage without breaking callers if public surface stable.
- Works with access modifiers chapter Q2.

---

#### Q2. What are the different access modifiers in C#? (`private`, `protected`, `internal`, `protected internal`, `private protected`)

**Answer:** Access modifiers control visibility: `private` (declaring type only), `protected` (declaring type and derived types), `internal` (same assembly), `protected internal` (protected OR internal), `private protected` (protected AND internal—same assembly derived only).

- Top-level types default internal; members default private—Q7.
- `public` exposes everywhere; `file` (C# 11) limits to source file for types.
- Gotcha 6 Module 02 contrasts protected internal vs private protected.
- Nested types can use broader or narrower visibility relative to outer type—Q8.

---

#### Q3. What is the difference between "information hiding" and "data hiding"?

**Answer:** Data hiding restricts direct access to fields (private state); information hiding conceals broader design decisions—algorithms, collaboration between objects, and changeable implementation details—not only raw data fields.

- Private field with public property is data hiding with controlled exposure.
- Hiding that sorting uses quicksort vs mergesort behind `Sort()` is information hiding.
- Encapsulation encompasses both practices for maintainability.
- Abstraction emphasizes essential information hiding at model level—chapter 06 Q2.

---

#### Q4. Why is exposing a mutable collection through a public getter an encapsulation break?

**Answer:** Public getter returning `List<T>` lets callers mutate internal list (`Clear`, `Add`) bypassing your validation, breaking invariants and coupling external code to your concrete collection type.

- Return `IReadOnlyList<T>` or copy on get—Q9 defensive copying.
- Gotcha 9 Module 02.
- Properties chapter Q15 same theme.
- Expose methods `AddItem(item)` enforcing rules instead of raw list access when mutation needed externally.

---

#### Q5. What is the difference between `protected internal` and `private protected`?

**Answer:** `protected internal` means accessible to derived classes anywhere OR any code in the same assembly. `private protected` means accessible only to derived classes that are also in the same assembly as the base—stricter intersection, not union.

- Gotcha 6 Module 02 is common interview trap (OR vs AND).
- Choose `private protected` for assembly-local extension points not exposed to other assemblies' subclasses.
- `protected internal` wider for framework scenarios with cross-assembly inheritance.
- Table: protected internal = union; private protected = intersection of protected and internal.

---

#### Q6. What does `internal` mean in the context of assemblies and `InternalsVisibleTo`?

**Answer:** `internal` members are visible only within the same assembly, hiding implementation from referencing assemblies while sharing among types in one project output. `InternalsVisibleTo` attribute grants named friend assemblies access to internal members for testing or tightly coupled companion libraries.

- Friend assembly pattern enables unit test projects to test internal helpers—Q13.
- Security not guaranteed—friend trusts partner assembly completely.
- Public API surface stays smaller by keeping helpers internal.
- Source generators and analyzers in same compilation see internal types normally.

---

#### Q7. What is the default access level for class members if you omit an modifier?

**Answer:** Class members default to `private` if no modifier specified—accessible only within the declaring type. Top-level classes and structs default to `internal` at namespace scope.

- Explicit `public` required for API intended for external consumers.
- Interface members are implicitly public.
- Nested private class members still default private inside nested class.
- Omitting modifier on member is not "internal"—common misconception.

---

#### Q8. How do access modifiers apply to nested types vs top-level types?

**Answer:** Top-level types can be `public` or `internal` (or `file` scoped). Nested types can use any modifier including `private protected`, controlling visibility relative to outer type and assembly boundaries.

- `private nested class` visible only inside outer class—useful for implementation details.
- Public nested class visible when outer visible and nested declared public.
- Nested type can access outer private members directly.
- Depth of public nested types increases surface area—usually keep nested types private.

---

#### Q9. What is defensive copying when returning collections from properties?

**Answer:** Defensive copy returns a new collection with copied elements (shallow or deep as needed) so callers cannot mutate internal backing store through the returned reference.

- `public IReadOnlyList<T> Items => _items.AsReadOnly();` or `return _items.ToList();` on each get trades allocation for safety.
- Read-only wrapper prevents structural changes but may still allow mutating mutable elements unless deep copy.
- Choose based on threat model: trusted internal callers vs public API exposure.
- Immutable collections (`ImmutableList<T>`) avoid copy on every get if shared carefully.

---

#### Q10. What is the difference between encapsulation and immutability?

**Answer:** Encapsulation controls how state is accessed and modified; immutability means state cannot change after construction. Immutable types often use encapsulation with init-only or get-only properties and no mutators.

- Immutable object is encapsulated; encapsulated object is not necessarily immutable (`{ get; private set; }` still mutable internally).
- Records and init-only properties support immutability patterns—Properties Q5.
- Immutability simplifies threading and hashing; encapsulation simplifies invariant enforcement.
- Can combine: private set only in constructor, no public mutators.

---

#### Q11. Why are public fields discouraged in public APIs even for simple DTOs in some codebases?

**Answer:** Public fields freeze implementation, bypass validation hooks, complicate serialization versioning, and prevent future computed property substitution without breaking binary compatibility.

- Properties allow adding validation or logging in setter later without changing call syntax drastically.
- Some serializers still support fields with attributes, but conventions favor properties.
- Performance gap between field and auto-property negligible in most apps.
- Internal DTOs in performance paths occasionally use public fields deliberately—document exception.

---

#### Q12. How does `private protected` restrict visibility compared to `protected` alone?

**Answer:** `protected` alone allows derived classes in any assembly to access member. `private protected` additionally requires derived class to reside in the same assembly as base, blocking cross-assembly subclass access to that member.

- Useful for internal extension hooks not supported for third-party subclasses in other assemblies.
- Stricter than protected internal which allows same-assembly non-derived access too.
- See Q5 OR vs AND gotcha distinction.
- API design documents which extension model is supported cross-assembly.

---

#### Q13. What is a friend assembly pattern, and what are its trade-offs?

**Answer:** `InternalsVisibleTo("TestAssembly")` exposes internal members to a trusted assembly, commonly unit tests, without making helpers public production API.

- Trade-off: breaks encapsulation boundary between assemblies deliberately; friend must be trusted not to abuse internals.
- Strong-name friends require public key in attribute.
- Hides test-only surface from public consumers while enabling white-box tests.
- Overuse creates tight coupling between production and test assemblies.

---

#### Q14. How do property accessors use asymmetric access (`public get; private set;`)?

**Answer:** Asymmetric accessors expose public read while restricting write to declaring type (or assembly with `internal set`), letting consumers observe state changes through public getters while mutations occur only via type methods enforcing rules.

- Common for entity IDs set only at creation in constructor or factory.
- `init` differs by allowing object initializer writes—Properties Q5.
- `{ get; private set; }` allows any instance method in class to mutate; init does not after construction completes.
- Serialization may require private set or constructor to restore state.

---

### 08. Events

#### Q1. Explain events in C# (including event handling and publisher-subscriber pattern).

**Answer:** Events are multicast delegate fields wrapped with restricted access so subscribers register handlers (`+=`) but only the declaring type can raise them, implementing publisher-subscriber decoupling where publishers signal occurrences without knowing specific subscribers.

- Syntax: `public event EventHandler<OrderEventArgs>? OrderPlaced;`
- Handlers are methods matching delegate signature invoked when publisher calls `OrderPlaced?.Invoke(this, args)`.
- UI frameworks (WinForms, WPF) map control events to this model extensively.
- Differs from plain public delegate field—Q2.

---

#### Q2. What is the difference between an `event` and a plain public delegate field?

**Answer:** A public delegate field allows external callers to invoke (`field()`) or replace (`=`) the entire invocation list, breaking encapsulation. An `event` restricts external code to `+=` and `-=` only; only the declaring class raises the event.

- Plain delegate field: `handler = null` from outside wipes subscribers—dangerous.
- Event keyword wraps delegate with add/remove accessors compiling to thread-safe patterns optionally.
- Always expose events, not public delegate fields, for subscribe-only semantics.
- Custom event accessors possible for logging subscriptions.

---

#### Q3. Why should you unsubscribe from events, and what problem does this prevent?

**Answer:** Failing to unsubscribe keeps publisher holding references to subscriber handlers (and thus subscriber objects), preventing garbage collection of short-lived listeners attached to long-lived publishers—a classic managed memory leak.

- UI controls unsubscribe on dispose; domain services unsubscribe when scoped object ends.
- Weak event patterns exist for advanced scenarios (WPF weak events).
- Gotcha 8 Module 02 on GC vs leaks.
- `-=` with exact same delegate instance removes handler; lambdas require storing reference to remove same lambda.

---

#### Q4. What happens during multicast delegate invocation if one subscriber throws?

**Answer:** By default, when one handler in a multicast delegate throws, later handlers in the invocation list may not run, and the exception propagates to the raiser unless each handler is invoked individually in try/catch loops.

- Event raisers sometimes iterate copy of handler list invoking each in try/catch to isolate subscriber failures.
- `Invoke` on multicast stops at first exception in standard single call.
- Robust frameworks log subscriber failures without aborting other subscribers.
- Document whether your event guarantees all subscribers notified despite individual failures.

---

#### Q5. What is the standard `EventHandler` / `EventHandler<TEventArgs>` pattern?

**Answer:** `EventHandler` uses `(object? sender, EventArgs e)` signature; generic `EventHandler<TEventArgs>` where `TEventArgs : EventArgs` provides typed event data. Publishers pass `this` as sender and custom `EventArgs` subclass with event details.

- `EventHandler<T>` avoids custom delegate type proliferation.
- `EventArgs.Empty` for parameterless notifications.
- Async handlers should avoid blocking publisher thread; consider async patterns carefully for reentrancy.
- Follow naming: event name verb phrase (`Changed`, `Completed`), handler method `OnChanged` or `HandleChanged`.

---

#### Q6. How do you raise an event safely (null-check, `?.Invoke`, local copy pattern)?

**Answer:** Before raising, ensure handlers exist using null-conditional `OrderPlaced?.Invoke(this, args)` or copy delegate to local variable first to avoid race where last subscriber unsubscribes between null check and invoke on another thread.

- Local copy pattern: `var handlers = OrderPlaced; if (handlers != null) handlers(this, args);`
- Pass meaningful `EventArgs` immutable where possible.
- Avoid raising events from constructor before subscribers attach unless documented.
- Thread-safe raising may require locking around copy and invoke—Q12.

---

#### Q7. What is the difference between custom delegate types and `EventHandler` for events?

**Answer:** Custom delegate types (`public delegate void PriceChangedHandler(decimal price);`) express domain-specific signatures but increase type proliferation. Standard `EventHandler<TEventArgs>` keeps consistent `(sender, e)` shape across frameworks and tooling.

- Custom delegates useful when event args pattern awkward for simple notifications.
- Framework conventions favor `EventHandler<T>` for discoverability.
- Events can use any delegate type syntactically if declared with `event DelegateType Name;`.
- Consistency aids maintainability across large codebases.

---

#### Q8. Can interfaces declare events, and how are they implemented?

**Answer:** Interfaces may declare events; implementing class must provide accessible add/remove accessors publicly or explicitly, satisfying contract for subscribers typed against interface reference.

- Explicit interface implementation of events rare but possible for hiding.
- Interface event on class typically public add/remove forwarding to private delegate field.
- Multiple interfaces rarely declare same event name; collision requires explicit implementation.
- Mock frameworks simulate interface events in tests for publisher interfaces.

---

#### Q9. What memory-leak scenario arises when a long-lived publisher holds references to short-lived subscribers?

**Answer:** Subscriber registers handler on long-lived publisher's event; publisher's delegate chain holds reference to subscriber instance, keeping it reachable and preventing GC even after subscriber logically "finished" if unsubscribe omitted.

- Example: static event on cache singleton referencing form instance closed but not disposed properly.
- Fix: unsubscribe in dispose, use weak references in specialized patterns, or scope publisher lifetime to subscriber.
- Static events are especially dangerous—live for process lifetime.
- Gotcha 8 Module 02.

---

#### Q10. What is the difference between events and the Observer pattern / IObservable?

**Answer:** C# events are language-supported multicast delegates with restricted invocation—classic Observer localized to publisher type. `IObservable<T>`/`IObserver<T>` (.NET Reactive Extensions) generalizes push sequences with subscription lifetime and operators pipeline.

- Events fit discrete signals (button clicked, order placed).
- IObservable fits streams of values over time with composable LINQ-like operators.
- Rx `Subject<T>` bridges both models.
- Choose events for simple component notifications; Rx for complex asynchronous streams.

---

#### Q11. Can you assign to an event from outside the declaring class (`event += handler` vs `event = handler`)?

**Answer:** Outside code may only use `+=` and `-=` on events, not `=` assignment, preventing external code from clearing or replacing entire subscriber list. Only declaring class can assign directly to backing delegate in custom accessor or raise logic internally.

- `publisher.MyEvent += Handler;` legal externally.
- `publisher.MyEvent = Handler;` compile error externally.
- Inside declaring class, can assign null to clear before dispose if needed.
- Reinforces Q2 encapsulation difference from public delegate field.

---

#### Q12. What is thread-safe event raising, and when is locking required?

**Answer:** When subscribers add/remove handlers on different threads while publisher raises event, copy handler reference under lock (or use Interlocked exchange) before invoke to avoid null reference or missed handler during concurrent modification.

- `lock (gate) { handlers = MyEvent; }` then invoke outside lock to prevent deadlocks in handlers.
- `EventHandler` add/remove in custom accessor can lock internally.
- UI events often marshal to UI thread via synchronization context rather than raw lock on raise.
- Document thread affinity expectations for subscribers to avoid cross-thread UI access.

---

### 09. OOP Real-World Examples

#### Q1. Explain the SOLID principles with concrete C# examples.

**Answer:** SOLID guides maintainable OOP: Single Responsibility (one reason to change per class), Open/Closed (extend without modifying), Liskov Substitution (derived substitutable for base), Interface Segregation (small interfaces), Dependency Inversion (depend on abstractions). In C#, apply via focused classes, interfaces, virtual extension points, and constructor injection of `ILogger`-style abstractions.

- SRP: separate `OrderValidator` from `OrderRepository` instead of one god class.
- OCP: add new payment strategy implementing `IPaymentProcessor` without editing checkout class.
- LSP: derived `LimitedWithdrawAccount` must honor `Account.Withdraw` contract—Q2.
- ISP: split `IWorker` into `IWork` and `IEat` rather than fat interface—Q9.
- DIP: `OrderService(ILogger logger)` not `new FileLogger()` inside—Q3.

---

#### Q2. What is the Liskov Substitution Principle? Give a classic violation (e.g., `Square`/`Rectangle`).

**Answer:** Subtypes must be substitutable for base types without altering correctness. Classic violation: `Square` inherits `Rectangle` with setters forcing width and height equal breaks callers expecting independent width/height adjustment on `Rectangle` reference holding `Square`.

- Fix: separate types or immutable value objects without contradictory setters.
- Inheritance implies IS-A; square is not a rectangle in mutable dimension model.
- See Inheritance Q18.
- Tests failing when base-typed mock replaced with derived real type signal LSP issues.

---

#### Q3. What is Dependency Inversion, and how does constructor injection implement it?

**Answer:** High-level modules depend on abstractions (`IEmailSender`), not concrete low-level classes (`SmtpEmailSender`). Constructor injection supplies implementations at composition root (DI container), enabling substitution in tests and configuration-driven wiring.

- `public OrderService(IOrderRepository repo, ILogger<OrderService> log)` stores interfaces.
- Container registers `IOrderRepository` → `SqlOrderRepository` at startup.
- Inversion flips dependency direction: infrastructure depends on domain abstractions defined with domain.
- Contrasts Service Locator anti-pattern—Q4.

---

#### Q4. What is the difference between Dependency Injection and the Service Locator pattern?

**Answer:** Dependency Injection passes dependencies into object (constructor/parameters) from outside, making requirements explicit. Service Locator hides global registry (`ServiceLocator.Get<IRepo>()`) inside methods, obscuring dependencies and complicating testing without global setup.

- DI constructor shows all collaborators at signature glance.
- Service Locator static access hides coupling and encourages static singletons—Gotcha 11 Module 02.
- Microsoft.Extensions.DependencyInjection is DI container, not locator pattern when used with constructor injection.
- Locator acceptable only in legacy composition roots migrating to DI gradually.

---

#### Q5. What is the difference between "has-a" and "is-a" relationships? When is inheritance the wrong choice?

**Answer:** IS-A suits true subtype polymorphism (Dog is Animal with substitutable behavior). HAS-A composes behavior by containing another object (Car has Engine). Inheritance is wrong when relationship is reuse-only, not substitutability, or when subclass violates base contract.

- Prefer composition for utility reuse (`class OrderService { private readonly IValidator _validator; }`).
- Inheritance wrong for `Stack inheriting List` exposing incompatible operations historically debated.
- See Inheritance Q13 composition guideline.
- Domain modeling uses IS-A sparingly for core aggregates with stable hierarchies.

---

#### Q6. What is the anemic domain model anti-pattern?

**Answer:** Anemic domain model puts all behavior in separate service classes while domain objects are only data bags with getters/setters, losing encapsulation of business rules inside entities and scattering logic procedurally.

- Results in duplicated validation across services and harder invariant enforcement.
- Rich domain places `account.Withdraw(amount)` with rules inside `Account`.
- CRUD apps may accept anemic DTOs at boundaries but core domain benefits from behavior colocation.
- See Classes Q20.

---

#### Q7. What is the Open/Closed Principle, and how do interfaces support extension without modification?

**Answer:** Open/Closed means types open for extension (new behaviors) but closed for modification (existing code unchanged). New feature adds new implementing class or strategy registered in DI rather than editing switch statements in core service.

- Add `PayPalProcessor : IPaymentProcessor` without changing `CheckoutService` method bodies when using plugin registration.
- Switch on enum payment type violates OCP when new enum requires editing switch everywhere.
- Polymorphism and DI registration lists embody OCP in C# apps.
- Attributes and reflection-based plugins extend similarly at composition root.

---

#### Q8. What is the Single Responsibility Principle — how do you recognize a class that violates it?

**Answer:** A class violates SRP when it has multiple independent reasons to change—mixing persistence, validation, email notification, and PDF generation in one `OrderManager` class. Split when changes to email templates force retesting database code in same class.

- Smell: class name with "And" or "Manager" doing unrelated tasks.
- Each class should answer one stakeholder concern or axis of change.
- SRP is about cohesion, not literally one method per class.
- Microservices and modular monoliths apply SRP at service level too.

---

#### Q9. What is the Interface Segregation Principle — why are fat interfaces problematic?

**Answer:** Clients should not depend on methods they do not use. Fat interface `IWorker { Work(); Eat(); Sleep(); }` forces dummy implementations in classes that only work, violating cohesion and complicating mocks.

- Split into `IWorkable`, `IFeedable` so robot implements only `IWorkable`.
- Smaller interfaces simplify testing with focused mocks.
- ASP.NET Core options pattern uses many small option interfaces.
- Segregation pairs with DI registering only needed services.

---

#### Q10. What is a factory method vs a simple constructor — when do you introduce a factory?

**Answer:** Constructor creates instance when type and invariants are straightforward. Factory method (static or instance) encapsulates complex creation, returns interface type, selects subclass based on input, caches instances, or handles TryCreate failure without throwing from constructor.

- `Icon.CreateFromResource(name)` hides platform-specific subclasses.
- Abstract factory family creates related objects (UI toolkit themes).
- Constructors should not perform heavy I/O; factories can async create externally.
- Constructors chapter Q17 validation vs factory scope.

---

#### Q11. What is the Strategy pattern, and how does it map to interfaces/delegates in C#?

**Answer:** Strategy encapsulates interchangeable algorithms behind common interface (`IDiscountStrategy`) or delegate (`Func<decimal, decimal>`), letting context object delegate calculation without switch on enum types.

- Register multiple strategies in DI; select at runtime based on customer tier.
- Delegates lightweight strategy for single-method algorithms without interface ceremony.
- Open/Closed: add strategy class without editing context when injected via IEnumerable strategies.
- Functional style uses pure functions as strategies.

---

#### Q12. What is the Repository pattern at a high level, and why depend on abstractions?

**Answer:** Repository mediates between domain and data mapping layer, exposing collection-like interface (`GetById`, `Add`) hiding SQL/EF details. Depend on `IRepository<T>` so domain and tests use in-memory fake without database.

- Keeps domain free of EF `DbContext` leakage into entities when bounded context respected.
- Unit tests substitute `FakeOrderRepository` verifying service logic.
- Over-abstracting simple CRUD may be overkill—pragmatic use for complex domains.
- DIP and testability primary motivators—Q3.

---

#### Q13. How does polymorphism simplify replacing implementations in tests (mock/stub scenarios)?

**Answer:** Code depending on `ILogger` or `IRepository` accepts test doubles implementing same interface; polymorphic substitution at DI registration replaces production implementation with mock verifying interactions without changing consumer code.

- Moq/NSubstitute create dynamic proxies implementing interfaces.
- Virtual methods on concrete classes allow subclass stubs but interface injection cleaner.
- Polymorphism localizes change to composition root test setup.
- Static dependencies bypass polymorphism—hard to mock without wrappers.

---

#### Q14. What is the difference between domain modeling with rich behavior vs CRUD-style service objects?

**Answer:** Rich domain colocates rules on entities (`order.Ship()` validates state transitions). CRUD-style services expose `UpdateOrderDto` methods that set properties procedurally with validation scattered in service layer, often matching anemic models.

- Rich model reduces invalid state if methods enforce invariants consistently.
- CRUD services map quickly to REST endpoints and EF tracking for simple apps.
- Hybrid common: rich core domain, anemic DTOs at API boundary.
- Choice depends on complexity, team patterns, and bounded context boundaries.

---

### Gotchas — Module 02

#### Gotcha 1. Virtual method from base constructor

**Answer:** Calling an overridable virtual method from a base constructor dispatches to derived override before derived field initializers and constructor body run, so overridden code observes default/null derived field values.

- Avoid virtual calls in constructors; use private non-virtual initialization methods in sealed base or static factories.
- See Constructors Q13 and Inheritance Q5.

---

#### Gotcha 2. Method hiding vs overriding

**Answer:** `new` hides by compile-time static type; `override` dispatches by runtime type. Calling through base reference invokes hidden base method, not hidden derived method, breaking expected polymorphism.

- Use `override` for intentional polymorphism; `new` only when substitution not desired.
- See Inheritance Q9 and Q15.

---

#### Gotcha 3. `Equals()` without `GetHashCode()`

**Answer:** Overriding `Equals` without consistent `GetHashCode` breaks hash-based collections—objects can be stored in `Dictionary`/`HashSet` but not found after insertion or after mutating key fields.

- Override both together; keep hash stable while object used as key.
- See Classes Q15 and Gotcha 4 on mutable keys.

---

#### Gotcha 4. Mutable object as dictionary key

**Answer:** Mutating fields that participate in equality/hash after inserting object as key changes hash bucket location logically, making entry unreachable—"lost" keys at runtime.

- Use immutable key types or stable hash codes; do not mutate key properties while in dictionary.
- Value types as keys copy on insert—mutation of separate copy does not affect bucket unless reinserted.

---

#### Gotcha 5. Struct boxing via interface

**Answer:** Assigning struct to interface type boxes it; subsequent mutations to original struct do not affect boxed copy used through interface reference.

- Prefer generics `IEquatable<T>` on struct directly to avoid boxing when possible.
- See Module 01 boxing Q4 and Classes Q16.

---

#### Gotcha 6. `protected internal` vs `private protected`

**Answer:** `protected internal` is union (protected OR internal)—wider access. `private protected` is intersection (protected AND internal)—narrower, same-assembly derived only.

- See Encapsulation Q5 and Q12.

---

#### Gotcha 7. Type-checking anti-pattern

**Answer:** Long chains `if (animal is Dog) ... else if (animal is Cat)` defeat polymorphism; prefer virtual methods, visitor pattern, or switch on abstraction when closed set, or double dispatch when appropriate.

- Adding new type requires editing chain—violates Open/Closed.
- See Inheritance Q17 and Q2 polymorphism.

---

#### Gotcha 8. Memory leaks despite GC

**Answer:** Garbage collection reclaims unreachable objects, but event handlers, static caches, and timers holding references to short-lived objects keep them reachable—classic managed leaks.

- Unsubscribe events; avoid static collections keyed by objects without removal.
- See Events Q3 and Q9.

---

#### Gotcha 9. Exposing `List<T>` directly

**Answer:** Public `List<T>` property lets callers mutate internal list without invariant checks; return read-only views or defensive copies instead.

- See Properties Q15 and Encapsulation Q4.

---

#### Gotcha 10. `init` after construction

**Answer:** Init-only properties settable in object initializers and constructors cannot be assigned in ordinary methods afterward, unlike `{ get; private set; }` which class methods can mutate anytime after construction.

- Confusing when teams expect private set semantics.
- See Properties Q5 and Q6.

---

#### Gotcha 11. Static "singleton" vs DI singleton

**Answer:** Static class or static instance singleton is hard to replace in tests and hides dependencies globally. DI-managed singleton lifetime still allows interface mocking if consumers depend on abstractions injected once per container.

- Static global state persists across tests unless manually reset.
- See Static chapter Q4 and Q12.

---

#### Gotcha 12. Explicit interface hiding

**Answer:** Class can expose public method and explicit interface method with same logical operation behaving differently; callers must know whether they invoke through class or interface typed reference.

- See Abstract chapter Q7 and Q13.

---

#### Gotcha 13. Finalizer timing

**Answer:** Finalizers run on GC schedule unpredictably—do not rely on `~ClassName()` for timely release of files, sockets, or locks; use `Dispose`/`using`.

- See Classes Q8 and Constructors Q2.

---

#### Gotcha 14. Overriding `==` without consistent `Equals`/`GetHashCode`

**Answer:** Custom equality operator inconsistent with `Equals`/`GetHashCode` breaks collections, LINQ set operations, and contract expectations—always keep three aligned.

- See Operators Module 01 Q15 and Classes Q15.

---

#### Gotcha 15. Default interface methods on structs

**Answer:** Calling default interface method on struct through interface reference may box struct, losing direct struct semantics and copying behavior; performance-sensitive struct code should use generic constraints or direct struct methods when possible.

- See Abstract chapter Q8 and Module 01 boxing topics.

---

---

## Module 03. Generics & Collections

### 01. Generics

#### Q1. What are generics in C#? Why were they introduced?

**Answer:** Generics let you declare classes, methods, structs, interfaces, and delegates with type parameters (such as `T`) so one definition works for many concrete types while the compiler still enforces type safety at compile time. They were introduced in C# 2.0 to replace the old pattern of storing everything as `object` in collections, which caused runtime cast failures, lost compile-time checking, and boxing overhead for value types.

- Before generics, `ArrayList` and `Hashtable` held `object` references; inserting an `int` boxed it on the heap, and reading it back required an explicit cast that could throw `InvalidCastException` at runtime.
- With `List<int>`, the compiler knows the element type is `int`, so `Add(42)` and `int n = list[0]` require no boxing and no cast.
- Generic code is written once and reused: `Dictionary<TKey, TValue>`, `List<T>`, and your own `WarehouseSlot<T>` all share the same pattern of type parameters closed at the use site.
- The runtime retains generic type information (reification), so reflection can distinguish `List<int>` from `List<string>` unlike languages that erase generics entirely.

---

#### Q2. What is the difference between generic and non-generic collections?

**Answer:** Generic collections in `System.Collections.Generic` are typed with type parameters (`List<T>`, `Dictionary<TKey, TValue>`), while non-generic collections in `System.Collections` store `object` references and require casts at read time. Generic collections give compile-time type checking and avoid boxing for value types; non-generic collections trade safety and performance for backward compatibility with pre-C# 2.0 APIs.

| | Non-generic (`ArrayList`, `Hashtable`) | Generic (`List<T>`, `Dictionary<K,V>`) |
|---|---|---|
| Element storage | `object` | Concrete `T` |
| Value types | Boxed on insert | Stored directly |
| Type errors | Runtime (`InvalidCastException`) | Compile time |
| Modern usage | Legacy maintenance | Default for new code |

Prefer generic collections for all new C# code; reach for non-generic types only when interfacing with legacy APIs that still expose `ArrayList` or `Hashtable`.

---

#### Q3. Explain generic constraints in C# (`where` clause) with examples.

**Answer:** Generic constraints limit which types may substitute for a type parameter by appending `where` clauses after the parameter list, which tells the compiler what operations are legal on `T` inside the generic body. Without constraints, `T` is treated as an unknown type and you can only assign `default(T)` or pass `T` around — you cannot call `new T()`, compare with `<`, or access members unless you constrain `T` appropriately.

- `where T : class` — `T` must be a reference type; allows assigning `null` to `T` and enables reference-type-only APIs.
- `where T : struct` — `T` must be a non-nullable value type; excludes reference types and nullable value types unless combined with other rules.
- `where T : new()` — `T` must expose a public parameterless constructor, enabling `return new T();` inside factories.
- `where T : SomeBase` or `where T : ISomeInterface` — `T` must inherit or implement the named type, so you can call members defined on that base or interface.
- Multiple constraints combine on one parameter: `where T : class, ICloneable, new()` requires a reference type that implements `ICloneable` and has a parameterless constructor.

```csharp
public static T CreateDefault<T>() where T : new() => new T();
public static int Compare<T>(T a, T b) where T : IComparable<T> => a.CompareTo(b);
```

---

#### Q4. Explain covariance and contravariance in generics (`in` and `out` keywords).

**Answer:** Variance modifiers on generic interface type parameters control whether a generic type with a more derived type argument can be assigned to a generic type with a more base type argument. `out T` marks covariance (you only produce `T` items out of the interface), and `in T` marks contravariance (you only consume `T` items into the interface); without these modifiers, generic types like `List<T>` are invariant.

- **Covariance (`out T`):** `IEnumerable<string>` can be assigned to `IEnumerable<object>` because the interface only yields items — callers never insert a `string` where an `object` is expected in a way that breaks safety.
- **Contravariance (`in T`):** `IComparer<object>` can be assigned to `IComparer<string>` because the comparer only receives `string` instances to compare, and any comparer that handles `object` can handle `string`.
- Mutable collections like `List<T>` cannot be covariant: if `List<string>` were a `List<object>`, you could `Add(new object())` and break type safety for the original string list.
- Variance applies to delegate types as well: `Action<in T>` is contravariant; `Func<out TResult>` is covariant on its return type parameter.

---

#### Q5. Can you use `where T : Enum` or `where T : unmanaged`? What problems do these solve?

**Answer:** Yes — `where T : Enum` restricts `T` to any enum type, and `where T : unmanaged` restricts `T` to types that contain no managed references (plain value types, pointers, and structs composed only of other unmanaged fields). These constraints unlock APIs that need to treat enums or blittable structs specially without accepting arbitrary reference types.

- `where T : Enum` lets you write generic helpers such as `Enum.Parse<T>`, flag operations, or logging that works on any enum while the compiler rejects `int` or `string` at the call site.
- `where T : unmanaged` is required for `Span<T>`, stack allocation, interop marshaling, and low-level memory operations where the runtime must guarantee no GC-tracked references live inside `T`.
- `unmanaged` is stricter than `struct`: a struct containing a `string` field is a value type but not unmanaged because it holds a managed reference.
- Both constraints were added in modern C# versions to express intent that older `where T : struct` alone could not capture precisely.

---

#### Q6. What happens when you use `default(T)` on an unconstrained type parameter?

**Answer:** `default(T)` returns the zero-value for whatever type `T` represents at the closed constructed type: `0` for numeric value types, `false` for `bool`, `null` for reference types, and a zeroed struct for struct types. On an unconstrained `T`, the compiler allows the expression because every type in C# has a defined default value, even though you cannot assume `T` is nullable or comparable without additional constraints.

- For reference types, `default(T)` is always `null`; with nullable reference types enabled, assigning that to a non-nullable variable may produce a compiler warning unless you use the null-forgiving operator.
- For value types, `default(T)` produces an all-zero bit pattern — for example `default(DateTime)` is `0001-01-01`, not a "null date."
- `default(T)` is the safe way to initialize out parameters, optional slots, or generic accumulators when you cannot call `new T()` without a `new()` constraint.
- It differs from `default` alone in generic methods: `default` infers from context, while `default(T)` explicitly names the type parameter.

---

#### Q7. Why can't you write `T value = null;` unless `T` is constrained to `class`?

**Answer:** The compiler only allows `null` to be assigned to a type parameter when it knows `T` might be a reference type, which is guaranteed by a `class`, `class?`, or similar reference-type constraint. An unconstrained `T` could be instantiated as `int` or any struct, and `null` is not a valid value for non-nullable value types.

- Without a constraint, `T` might be `int`, `DateTime`, or another struct where `null` is meaningless and would be a compile error.
- Adding `where T : class` tells the compiler that only reference types will close `T`, so `T value = null;` is legal and represents a missing reference.
- Nullable value types use a different pattern: `where T : struct` combined with `T?` or `Nullable<T>` when you need an optional value type, not bare `null` assigned to `T`.
- This rule prevents silent misuse in generic algorithms that accidentally assume reference semantics on value types.

---

#### Q8. What is the difference between a generic class and a generic method?

**Answer:** A generic class declares type parameters on the class itself, so every instance and instance member shares those type arguments for the life of the object (`List<T>`, `Dictionary<TKey, TValue>`). A generic method declares its own type parameters on the method only, independent of whether the containing class is generic, and the compiler can often infer the method's type arguments from arguments at the call site.

- `class WarehouseSlot<T> { … }` — closing `T` as `string` or `decimal` creates two distinct class types with separate static fields and layouts.
- `static void Swap<T>(ref T a, ref T b)` inside a non-generic `Program` class — `T` is scoped to that method; `Swap(ref x, ref y)` infers `T` as `int`.
- A generic class can also contain generic methods with their own additional type parameters: `class Repo<TEntity> { void Map<TDto>(…) }` closes `TEntity` at the class level and `TDto` per method call.
- Choose a generic class when the stored state is tied to `T`; choose a generic method for one-off algorithms (swap, max, factory) that do not need a dedicated generic type.

---

#### Q9. What's the difference between reflection over an open generic type (`List<>`) and a closed generic type (`List<int>`)?

**Answer:** An open generic type is the type definition with unbound type parameters (for example `typeof(List<>)`), while a closed generic type is fully constructed with concrete type arguments (for example `typeof(List<int>)`). Reflection on an open type describes the generic template; reflection on a closed type describes one specific instantiation including its type arguments and layout.

- `typeof(List<>)` returns the generic type definition — useful for `MakeGenericType(typeof(int))` to build `List<int>` at runtime.
- `typeof(List<int>)` returns the closed type with `GenericTypeArguments` containing `[typeof(int)]`.
- `GetGenericTypeDefinition()` on `typeof(List<int>)` yields the open `List<>`; `IsGenericTypeDefinition` is true only on the open form.
- APIs like `Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType))` require the open definition plus runtime type arguments.

---

#### Q10. Why does `typeof(List<int>) == typeof(List<string>)` return `false`, and how do you get the shared generic type definition?

**Answer:** Each closed constructed generic type is a distinct runtime `Type` object because `List<int>` and `List<string>` have different type arguments, layouts, and static field slots even though they share the same generic definition. Equality compares the full constructed type, not just the name `List`.

- `typeof(List<int>) == typeof(List<string>)` is `false` — they are separate closed types in the Common Language Runtime (CLR).
- To obtain the shared template, call `typeof(List<int>).GetGenericTypeDefinition()`, which returns the open `List<>` type equal for all `List<T>` instantiations.
- Static fields on generic types are per closed type: `List<int>.Capacity` behavior and any `static` members on a generic class are not shared between `List<int>` and `List<string>`.
- This distinction matters for caching, serialization, and dependency injection keyed by `Type`.

---

#### Q11. What is type erasure vs reification — does C# retain generic type information at runtime?

**Answer:** Type erasure removes generic type parameters at compile time so runtime only sees raw types (Java's approach for most generics), while reification preserves generic instantiations as real runtime types. C# and the CLR use reification: `List<int>` and `List<string>` exist as distinct types in metadata and at runtime, and reflection can inspect `GenericTypeArguments`.

- IL for generic types carries type parameters and constraints; the Just-In-Time (JIT) compiler generates specialized code paths for value type arguments where needed.
- You can call `obj.GetType()` on a `List<int>` and see `System.Collections.Generic.List`1[System.Int32]`, not a erased `List`.
- Some optimizations share implementation between reference type instantiations (all reference types may share one native code body for the same generic method), but the logical type identity remains distinct.
- Contrast with languages that erase generics: in C#, `is List<int>` and casting to `List<int>` remain meaningful at runtime.

---

#### Q12. What constraints allow calling `new T()` — what does `where T : new()` enable?

**Answer:** The `new()` constraint requires that `T` has an accessible public parameterless constructor, which is the only constraint that authorizes `new T()` inside the generic body. It is often combined with `class` or an interface constraint when building factories, object pools, or default instances in generic utilities.

- `where T : new()` alone allows value types (which always have an implicit default constructor) and classes with a public parameterless ctor.
- It does not guarantee a meaningful initialized object — `new T()` on a struct yields zeroed fields; on a class it runs the parameterless constructor if one exists.
- Abstract types fail the `new()` constraint because they cannot be instantiated.
- Typical pattern: `public static T Create<T>() where T : new() => new T();` used by serializers, DI containers, and generic caches.

---

#### Q13. What is the difference between `where T : class` and `where T : struct` constraints?

**Answer:** `where T : class` restricts `T` to reference types only, allowing `null` assignments and reference equality semantics, while `where T : struct` restricts `T` to non-nullable value types, excluding ordinary classes and preventing `null` from being assigned to `T` directly. They partition the type universe into reference-only and value-only generic APIs.

| | `where T : class` | `where T : struct` |
|---|---|---|
| Allowed types | Classes, interfaces, delegates, arrays | int, DateTime, custom structs, enum |
| `null` for `T` | Allowed | Not allowed |
| Typical use | Nullable caches, reference comparers | Numeric algorithms, stack-friendly buffers |
| Nullable value types | Use `where T : struct` with `T?` separately | `T?` allowed for optional value types |

Pick `class` when the algorithm needs reference identity or null as sentinel; pick `struct` when you require stack-friendly value semantics and no null state for `T` itself.

---

#### Q14. What does `where T : notnull` mean for nullable reference type analysis?

**Answer:** The `notnull` constraint tells the nullable reference type analyzer that type arguments substituted for `T` must not be nullable reference types, which helps generic APIs like `Dictionary<TKey, TValue>` reject nullable keys at compile time when nullable reference types are enabled. It does not change runtime behavior — it strengthens compile-time warnings when someone tries to close `T` with `string?`.

- `where TKey : notnull` on `Dictionary<TKey, TValue>` means the compiler warns if you write `Dictionary<string?, int>` because keys must not be null for correct hash table behavior.
- Value types already satisfy `notnull` because they cannot be null unless wrapped in `Nullable<T>`.
- It complements runtime checks: null keys still throw at runtime, but `notnull` catches many mistakes earlier.
- Introduced to align generic collection constraints with C# 8+ nullable reference type flow analysis.

---

#### Q15. Why are generic value types separate closed types at runtime for static fields?

**Answer:** Each closed constructed generic type such as `GenericCounter<int>` and `GenericCounter<string>` is a distinct runtime type in the CLR, and static fields belong to that closed type rather than to the open generic definition alone. That is why static state is not shared across different type arguments — each instantiation gets its own static storage slot.

- `class GenericCounter<T> { public static int Count; }` — `GenericCounter<int>.Count` and `GenericCounter<string>.Count` are independent variables.
- This follows from reification: the runtime treats `GenericCounter<int>` and `GenericCounter<string>` as different types with potentially different code paths (especially when `T` is a value type).
- Generic classes with reference type arguments may share one implementation body in native code, but static fields remain per closed type identity.
- Interview pitfall: assuming one static counter for all `T` in a generic class — increments on `Cache<int>` do not affect `Cache<string>`.

---

#### Q16. What is covariance on `IEnumerable<out T>` — why can you assign `IEnumerable<string>` to `IEnumerable<object>`?

**Answer:** Covariance on `IEnumerable<out T>` means that if `string` is assignable to `object`, then `IEnumerable<string>` is assignable to `IEnumerable<object>` because the interface only produces (`out`) items of type `T` and never accepts input positions where a wrong subtype could be inserted. The `out` modifier promises callers receive at least `T` items through the interface's output members.

- `IEnumerable<T>` exposes `GetEnumerator()` yielding `T` — callers read strings but treat them as objects, which is safe because every string is an object.
- Without `out`, generic interfaces would be invariant and you would need to copy or cast elements manually to view a sequence as a less derived type.
- Covariance applies to assignment and method parameters expecting a less derived producer, not to mutable lists where items can be both read and written.
- Practical use: pass `List<string>` where `IEnumerable<object>` is expected for logging, serialization, or mixed-type processing without copying the list.

---

#### Q17. What is contravariance on `Action<in T>` / `IComparer<in T>`?

**Answer:** Contravariance on `in T` allows assigning a delegate or interface that operates on a more base type to one that operates on a more derived type — for example `Action<object>` to `Action<string>` — because the callee only passes `string` instances into parameters typed as `T`, and any handler written for `object` can accept those strings safely. The `in` modifier marks type parameters that appear only in input positions (parameters), never as return types.

- `Action<in T>`: a method that handles any `object` can handle each `string` passed to `Action<string>`.
- `IComparer<in T>`: a comparer for `object` can compare two `string` arguments without losing type safety.
- Contravariance reverses the usual inheritance direction for assignment: the parameter type goes "the other way" compared to covariance on `IEnumerable<out T>`.
- Mutable generic classes cannot be contravariant on `T` for the same reasons they cannot be covariant — input and output positions would allow unsafe writes.

---

#### Q18. Why is `List<T>` neither covariant nor contravariant on `T`?

**Answer:** `List<T>` is invariant because it exposes both input and output uses of `T`: you can read `T` items via the indexer and enumerator and write `T` items via `Add` and the indexer setter. If `List<string>` were assignable to `List<object>`, you could add a non-string `object` to what is logically still a string list, breaking type safety.

- Covariance requires that `T` appears only in output positions; `List<T>` has `Add(T item)` — an input position.
- Contravariance requires input-only positions; `List<T>` also returns `T` from `this[int index]`.
- `IReadOnlyList<T>` is still not covariant on `T` in the BCL because its definition includes both `T` input and output in the full interface hierarchy, though you often expose `IEnumerable<T>` (covariant) for read-only consumption.
- Safe pattern: expose `IEnumerable<T>` or `IReadOnlyList<T>` from APIs when callers only need to read; keep `List<T>` as the concrete mutable type internally.

---

#### Q19. What is the difference between generic specialization performance for value types vs reference types?

**Answer:** When generic methods and classes are closed over value types, the JIT compiler can generate specialized native code that uses direct machine operations on `int`, `double`, or other structs without boxing, which often matches hand-written type-specific code in hot paths. When closed over reference types, multiple instantiations (such as `List<string>` and `List<object>`) may share one JIT-compiled body because all references are pointer-sized at the machine level, though each closed type remains logically distinct.

- `List<int>` stores integers contiguously in an array with no per-element boxing; `ArrayList` boxing every `int` adds heap allocations and cache pressure.
- Value type generic arguments avoid virtual dispatch on the stored payload itself when the JIT specializes stores and loads.
- Reference type sharing reduces code bloat: one native implementation may serve `List<string>`, `List<object>`, and other reference `T` with the same IL pattern.
- Micro-benchmarks sometimes show value type generics faster than equivalent `object`-based code; the main win for everyday code is correctness and absence of casts, with performance as a bonus for value types.

---

#### Q20. Can you cast from `List<string>` to `List<object>` — what error or exception occurs?

**Answer:** No — the cast is rejected at compile time because `List<T>` is invariant, and even an explicit cast at runtime would throw `InvalidCastException` if attempted via reflection or invalid assumptions. There is no safe conversion between `List<string>` and `List<object>` despite `string` inheriting from `object`.

- Source code `List<object> bad = (List<object>)(object)stringList;` fails at compile time with an error that generic types are not covariant.
- If you need a view of strings as objects, use `IEnumerable<object>` (covariant) or project with LINQ: `stringList.Cast<object>()` which yields a new sequence without mutating the original list.
- Confusing `IEnumerable<string>` → `IEnumerable<object>` (legal) with `List<string>` → `List<object>` (illegal) is a common interview trap — see Gotcha 3 in this module.
- Mutating APIs should keep `List<string>` and expose read-only covariant interfaces when wider typing is needed.

---

### 02. ArrayList

#### Q1. What is the difference between `Array` and `ArrayList`?

**Answer:** `Array` in C# is a fixed-length, strongly typed collection whose element type is known at compile time (`int[]`, `string[]`), while `ArrayList` is a legacy growable class in `System.Collections` that stores elements as `object` references in a resizable internal buffer. Arrays offer index access and length fixed at creation; `ArrayList` offers dynamic `Add` and `Remove` at the cost of boxing value types and runtime casts.

- `int[] numbers = new int[3]` — length 3, all elements default to zero, no boxing for ints.
- `ArrayList list = new ArrayList(); list.Add(42);` — grows automatically, but the `int` is boxed to `object`.
- Arrays support multidimensional and jagged forms with CLR support; `ArrayList` is always a one-dimensional logical sequence.
- Modern code uses `T[]` when size is known or `List<T>` when growable typed storage is needed; `ArrayList` remains for legacy interop only.

---

#### Q2. What is the difference between `List<T>` and `ArrayList`?

**Answer:** `List<T>` is the generic replacement for `ArrayList`: it stores elements of type `T` in a growable array with compile-time type checking and no boxing for value types, while `ArrayList` stores `object` and requires casts on retrieval. Both use similar array-doubling growth strategies internally, but `List<T>` is the standard choice in modern C#.

- `List<int>`: `Add(1)` stores a raw int; `int x = list[0]` needs no cast.
- `ArrayList`: `Add(1)` boxes; `(int)list[0]` unboxes and can throw if the wrong type was stored.
- `List<T>` implements generic interfaces (`IList<T>`, `IEnumerable<T>`) used throughout LINQ and modern APIs; `ArrayList` implements non-generic `IList` and `IEnumerable`.
- Performance-sensitive code should always prefer `List<T>` over `ArrayList` unless a legacy API signature forces the latter.

---

#### Q3. Why is `ArrayList` considered a legacy collection in modern C#?

**Answer:** `ArrayList` predates generics and encodes the `object`-everything model that C# 2.0 deliberately replaced: it sacrifices compile-time safety, boxes value types, and pushes type errors to runtime. The Base Class Library (BCL) ships fully featured generic equivalents for every common scenario, so new code has no reason to adopt `ArrayList`.

- Microsoft added `List<T>`, `Dictionary<TKey,TValue>`, and other generic types in `System.Collections.Generic` specifically to supersede `ArrayList`, `Hashtable`, and non-generic `Queue`/`Stack`.
- Tooling, analyzers, and team style guides treat non-generic collections as compatibility shims for old frameworks rather than current design.
- Maintaining `ArrayList` in new code introduces boxing allocations and `InvalidCastException` risks that generics eliminate entirely.
- You still need to recognize `ArrayList` when reading or migrating legacy codebases, not when designing new features.

---

#### Q4. What boxing occurs when storing `int` values in an `ArrayList`?

**Answer:** Each `int` inserted into an `ArrayList` is converted to a boxed `object` on the heap: the value is copied into a heap-allocated wrapper, and the list holds a reference to that box. Reading the value back requires unboxing — extracting the `int` from the box — which adds CPU cost and can throw `InvalidCastException` if the stored object is not an `int`.

- `list.Add(42)` allocates a box for the 32-bit integer even though the logical data is four bytes.
- Ten thousand `int` inserts create ten thousand heap objects subject to garbage collection pressure.
- `List<int>` stores ints directly in its internal `T[]` buffer with no intermediate box per element.
- Decimal, bool, enum, and other value types follow the same boxing path in `ArrayList`.

---

#### Q5. What is the performance cost of repeated boxing/unboxing in hot loops using `ArrayList`?

**Answer:** Hot loops that repeatedly insert and read value types through `ArrayList` pay for heap allocation on every box, cache misses from scattered heap objects, and CPU cycles on every unbox and type check, which can dominate micro-benchmarks compared to `List<T>` on the same workload. The cost scales with iteration count and is especially visible in tight numeric or aggregation loops.

- Boxing allocates on the heap; high-frequency boxing increases Gen0 collection frequency and can cause GC pauses in latency-sensitive services.
- Unboxing validates the object's type at runtime before copying bits back to the stack — extra work `List<int>` avoids entirely.
- A loop that sums one million ints from `ArrayList` does one million unboxes; from `int[]` or `List<int>` it reads contiguous memory with no per-element type check.
- Replacing `ArrayList` with `List<T>` in hot paths is one of the cheapest performance wins when profiling shows allocation-heavy collection access.

---

#### Q6. Can you store mixed types in an `ArrayList`, and what typing risks does that create?

**Answer:** Yes — `ArrayList` stores `object`, so you can add strings, integers, custom classes, and null in the same instance, which is sometimes mistaken for flexibility but removes compile-time type discipline. Consumers must guess or document what each index holds, and an incorrect cast at read time throws `InvalidCastException` at runtime instead of failing at compile time.

- `list.Add("sku"); list.Add(42);` compiles without warning.
- Code that assumes every element is `int` breaks when a string was inserted by another maintainer or deserialization path.
- Generic `List<T>` rejects mixed types at compile time: `List<int>` cannot accept a string.
- Mixed-type scenarios that are intentional should use explicit models rather than accidental `ArrayList` soup.

---

#### Q7. What is the difference between `ArrayList.Capacity` and `Count`?

**Answer:** `Count` is the number of elements currently stored in the list, while `Capacity` is the size of the internal backing array allocated to hold elements before the next resize. Capacity is always at least `Count` and may be larger after growth or explicit preallocation to reduce reallocations during bulk adds.

- After `Add` three items, `Count` is 3; `Capacity` might be 4, 8, or another power-of-two depending on growth history.
- Setting `Capacity` explicitly preallocates space for anticipated inserts without changing `Count`.
- When `Count` exceeds `Capacity`, the internal array is replaced with a larger one and elements are copied — amortized O(1) for `Add` at the end, but O(n) copy cost at resize moments.
- The same `Count` vs `Capacity` distinction applies to `List<T>`; see List Q10.

---

#### Q8. When might you still encounter `ArrayList` in maintained legacy codebases?

**Answer:** You encounter `ArrayList` mainly in long-lived .NET Framework applications, third-party libraries written before generics, and APIs that were never updated when migrating business logic — for example old data-access layers, WinForms/WPF code-behind, or COM interop wrappers that expose `IList`/`ArrayList` to callers. Maintenance work reads and gradually replaces these usages rather than introducing new ones.

- Early ASP.NET Web Forms and ADO.NET samples often used `ArrayList` for in-memory row sets before strongly typed models.
- Some XML or reflection-driven code stored heterogeneous property bags in `ArrayList` because the element types were unknown at compile time.
- Migration strategy: wrap reads behind typed facades, replace with `List<object>` or proper domain types at boundaries, and avoid expanding `ArrayList`-based APIs.
- Interview context: recognize the type and explain why `List<T>` is the modern replacement, not advocate for new `ArrayList` usage.

---

#### Q9. What is the difference between `ArrayList` and `object[]`?

**Answer:** Both can hold references to objects of mixed runtime types, but `object[]` has a fixed length set at creation while `ArrayList` wraps a resizable internal array with `Add`, `Insert`, and `Remove` methods. An `object[]` is a CLR array type with compile-time element type `object`; `ArrayList` is a class that implements non-generic collection interfaces.

- `object[] arr = new object[10];` — length 10 forever; index out of range throws `IndexOutOfRangeException`.
- `ArrayList list = new ArrayList(); list.Add(item);` — grows as needed with similar boxing rules for value types stored as `object`.
- Arrays implement `IEnumerable` and can be used where fixed-size snapshots are needed; `ArrayList` adds mutability helpers and capacity management.
- For new mixed-reference storage with known max size, `object[]` or `List<object>` is clearer than `ArrayList`; for growable typed storage, `List<T>` wins.

---

#### Q10. What legacy non-generic collections (`Hashtable`, `Queue`, `Stack`) should you know for maintenance scenarios?

**Answer:** Alongside `ArrayList`, maintainers should recognize `Hashtable` (key/value map on `object`), non-generic `Queue` (FIFO on `object`), non-generic `Stack` (LIFO on `object`), and non-generic `SortedList` (sorted key/value pairs on `object`) in `System.Collections`. Each has a generic counterpart in `System.Collections.Generic` that should be used in new code.

| Legacy | Generic replacement |
|---|---|
| `Hashtable` | `Dictionary<TKey, TValue>` |
| `Queue` | `Queue<T>` |
| `Stack` | `Stack<T>` |
| `SortedList` | `SortedList<TKey, TValue>` / `SortedDictionary<TKey, TValue>` |

- All legacy types box value-type keys and values when stored as `object`.
- `Hashtable` allows null keys (one null key slot); `Dictionary<TKey,TValue>` with nullable reference types warns against null keys.
- Reading migration interviews: explain what each legacy type did and name the typed replacement without re-implementing legacy patterns in greenfield services.

---

### 03. List

#### Q1. Explain the internal working and performance of `List<T>` vs `LinkedList<T>`.

**Answer:** `List<T>` stores elements in a contiguous dynamic array, giving O(1) indexed access and amortized O(1) append at the end, but O(n) insert or remove in the middle because elements must shift. `LinkedList<T>` stores nodes with `Next`/`Previous` pointers, giving O(1) insert/remove when you already hold a `LinkedListNode<T>`, but O(n) indexed access because the chain must be walked from the head or tail.

| Operation | `List<T>` | `LinkedList<T>` |
|---|---|---|
| Index `list[i]` | O(1) | O(n) |
| Add at end | O(1) amortized | O(1) |
| Insert/remove middle | O(n) shift | O(1) with node reference |
| Memory | One array, dense | Extra pointer overhead per node |

Default to `List<T>` for general-purpose sequences; choose `LinkedList<T>` only when you frequently splice in the middle and already have node references, which is rare compared to array-backed lists.

---

#### Q2. What is `LinkedList<T>` and when should it be used?

**Answer:** `LinkedList<T>` is a doubly linked list in the BCL where each element lives in a `LinkedListNode<T>` that knows its predecessor and successor, and the list exposes O(1) insertion and removal relative to a node you already hold. It should be used when your algorithm requires frequent middle splicing without shifting a large array — for example some LRU cache internals — not for general random-access storage.

- There is no indexer on `LinkedList<T>`; reaching the i-th element requires O(n) traversal.
- `AddAfter(node, value)` and `Remove(node)` are constant time once you have the node reference.
- Most application code performs better with `List<T>` due to cache-friendly contiguous memory and simpler API surface.
- If you need queue-like behavior, `Queue<T>` is often simpler than managing linked nodes manually.

---

#### Q3. What is the difference between `List<T>.Sort()` stability and `OrderBy()` stability?

**Answer:** `List<T>.Sort()` uses an unstable sorting algorithm in the BCL, meaning equal elements may change relative order after the sort. LINQ `OrderBy()` is stable: equal keys retain their original sequence order from the source, which matters when you sort by one field but care about prior ordering as a tiebreaker.

- Sort by `CustomerId` with `Sort(comparer)`: two orders with the same id may swap positions compared to input order.
- `orders.OrderBy(o => o.CustomerId)` keeps the original relative order among orders sharing the same `CustomerId`.
- For unstable in-place list sort when stability matters, sort by a composite key that encodes the original index, or use `OrderBy` and `ToList()`.
- `OrderBy` uses deferred execution until enumeration; `List.Sort()` mutates the list immediately.

---

#### Q4. How does `List<T>` grow its internal buffer when capacity is exceeded?

**Answer:** When `Add` would exceed `Capacity`, `List<T>` allocates a new internal array larger than the current one (typically doubling), copies all existing elements into the new buffer, and replaces the old array reference. The old array becomes eligible for garbage collection unless another reference holds it.

- Typical growth doubles capacity (4 → 8 → 16 → …), keeping amortized O(1) cost per append at the end.
- You can set `Capacity` before a bulk insert if you know the final count, avoiding repeated resize copies during the loop.
- `Count` increases by one per successful `Add`; `Capacity` jumps only at resize events.
- `TrimExcess()` can shrink capacity down toward `Count` when you will not add many more items and want to release unused array slots.

---

#### Q5. What is the amortized cost of `Add` on `List<T>` vs `Insert` at the beginning or middle?

**Answer:** `Add` at the end is amortized O(1) because resizes are infrequent relative to appends, while `Insert(0, item)` or insert in the middle is O(n) every time because all trailing elements shift one slot right in the backing array. Front-heavy insert patterns on large lists are a common performance anti-pattern.

- One million `Add` calls on an empty list do roughly one million element copies total across all resizes — amortized constant per add.
- One thousand `Insert(0, x)` on a growing list shifts on average hundreds of elements per insert — quadratic behavior overall.
- For queue semantics with many front removals, prefer `Queue<T>` over `List<T>` with repeated `RemoveAt(0)`.
- See Gotcha 4 — frequent middle inserts on `List<T>` are O(n).

---

#### Q6. What does `List<T>.AsReadOnly()` return, and can callers still mutate the underlying list?

**Answer:** `AsReadOnly()` returns a `ReadOnlyCollection<T>` wrapper that implements `IReadOnlyList<T>` and blocks mutating methods on the wrapper itself, but the wrapper still points at the original `List<T>`. If the underlying list is modified, those changes are visible through the read-only wrapper on subsequent reads.

- The wrapper throws `NotSupportedException` if someone calls `Add` on the read-only view.
- `readOnly[0]` still reads live data from the backing list; mutating through the original reference changes what the wrapper shows.
- True immutability requires not exposing the mutable list reference or passing a copy.
- See Gotcha 9 — `AsReadOnly()` is a view, not a defensive copy.

---

#### Q7. What is the difference between `ConvertAll`, `ForEach`, and LINQ `Select` on a list?

**Answer:** `ConvertAll` maps each element to a new element and returns a new `List<TResult>` in one pass, `ForEach` executes an `Action<T>` side effect on each element without building a new list, and LINQ `Select` projects to an `IEnumerable<TResult>` with deferred execution until you enumerate or materialize.

- `list.ConvertAll(x => x * 2)` — eager, returns `List<int>` immediately.
- `list.ForEach(Console.WriteLine)` — eager side effects only; returns void.
- `list.Select(x => x * 2)` — lazy `IEnumerable<int>`; no new list until `ToList()` or foreach forces enumeration.
- Prefer LINQ when composing pipelines; use `ConvertAll` for a single eager list-to-list transform without LINQ.

---

#### Q8. What do `ToArray`, `CopyTo`, and `GetRange` do — which allocate new arrays?

**Answer:** `ToArray()` always allocates a new array of length `Count` and copies elements; `CopyTo(array)` copies into an existing array you supply; `GetRange(index, count)` allocates a new `List<T>` containing a sub-range copied from the source list, not a raw array.

- `list.ToArray()` — new `T[]` sized exactly to `Count`; common snapshot before passing to APIs expecting arrays.
- `list.CopyTo(destination, index)` — no new list object; uses an array you allocated earlier.
- `list.GetRange(2, 5)` — new `List<T>` with five elements copied from the source.
- For interop requiring arrays, `ToArray()` is typical; for sub-list views as lists, `GetRange` materializes a new list.

---

#### Q9. When would you expose `List<T>` as a return type vs `IReadOnlyList<T>` or `IEnumerable<T>`?

**Answer:** Return `IReadOnlyList<T>` or `IEnumerable<T>` from public APIs when callers should consume elements without depending on your concrete storage or mutating your internal collection, and return `List<T>` only when the caller genuinely needs list-specific mutability or you document that they may modify the returned instance.

- `IEnumerable<T>` suits streaming or lazy results and read-only forward iteration.
- `IReadOnlyList<T>` adds `Count` and an indexer for read access without `Add`/`Remove`.
- Returning the internal `List<T>` directly lets callers mutate your backing store unless you wrap or copy.
- Internal private fields stay as `List<T>` for performance; public surface narrows to read-only interfaces.

---

#### Q10. What is the difference between `List<T>.Capacity` and `Count`?

**Answer:** `Count` reports how many elements are logically in the list; `Capacity` reports how many slots are allocated in the internal array backing the list. After growth events, `Capacity` is typically greater than or equal to `Count`.

- Empty `new List<int>()` starts with `Count` 0 and a small default capacity.
- `new List<int>(1000)` sets initial capacity to 1000 while `Count` remains 0 until adds occur.
- `TrimExcess()` sets capacity to `Count` if the list has shrunk substantially.
- Same semantics as `ArrayList.Capacity` vs `Count` — see ArrayList Q7.

---

#### Q11. What happens if you mutate a list while iterating with `foreach`?

**Answer:** If you add or remove elements from a `List<T>` while a `foreach` loop is running on that same instance, the enumerator detects the version change and throws `InvalidOperationException`. This protects you from skipping elements or reading inconsistent state mid-iteration.

- The list maintains an internal version counter incremented on structural changes.
- Changing an element's fields without changing list structure does not trigger the exception — only structural changes do.
- Safe patterns: iterate a copy, collect removals separately, or use index-based loops when deleting backwards.
- See Gotcha 1 — modify while iterating.

---

#### Q12. What is `TrimExcess`, and when is it useful?

**Answer:** `TrimExcess()` sets the list's internal capacity to match its current `Count`, releasing spare array slots that were reserved during growth but are no longer needed. It is useful after a large bulk load followed by many removals, or when you will retain the list for a long time at a stable smaller size.

- After adding one million items then removing nine hundred thousand, capacity may still hold space for one million references until trimmed.
- `TrimExcess()` costs O(n) to copy elements to a smaller array — call it when memory savings outweigh the one-time copy.
- Long-lived caches that grow and shrink cyclically sometimes trim after compaction phases.

---

#### Q13. What is binary search on a list (`BinarySearch`) — what precondition must the list satisfy?

**Answer:** `List<T>.BinarySearch(T item)` performs O(log n) lookup using binary search on the internal array, but the list must already be sorted according to the same ordering the comparer uses. If the list is unsorted, the result is meaningless.

- Sort first: `list.Sort();` then `int index = list.BinarySearch(target);`
- Return value ≥ 0 means found; negative encodes a suggested insert index via `~index`.
- For custom types, provide `IComparer<T>` consistent with how the list was sorted.
- Duplicate elements: binary search returns any matching index, not necessarily the first or last.

---

#### Q14. How does `List<T>` indexer access compare to `LinkedList<T>` (no indexer)?

**Answer:** `List<T>` provides O(1) random access via `list[i]` because elements sit in a contiguous array, while `LinkedList<T>` deliberately omits an indexer because locating the i-th node requires walking the chain from a known end, which is O(n).

- Algorithms that binary search, partition by index, or swap by offset need `List<T>` or an array.
- Linked lists excel when you hold `LinkedListNode<T>` references and insert/remove around known nodes.
- Interview trap: assuming all "lists" in the BCL support `[i]` — only `IList<T>` implementations like `List<T>` and arrays do.

---

#### Q15. What is `Comparison<T>` delegate, and how does it relate to `List<T>.Sort`?

**Answer:** `Comparison<T>` is a delegate `int Comparison<T>(T x, T y)` that compares two items and returns negative, zero, or positive, and `List<T>.Sort(Comparison<T>)` uses it as the ordering rule for an in-place sort. It is a convenient alternative to implementing `IComparer<T>` when you only need a one-off sort order.

- `list.Sort((a, b) => a.Price.CompareTo(b.Price));` passes a lambda matching `Comparison<T>`.
- `list.Sort(Comparison<T>)` mutates the existing list; it does not return a new list.
- For stable ordering or deferred LINQ pipelines, use `OrderBy` instead — see List Q3.
- `Array.Sort` also accepts `Comparison<T>` for arrays with the same comparison contract.

---

### 04. Dictionary

#### Q1. What is the difference between `Dictionary<TKey, TValue>` and `Hashtable`?

**Answer:** `Dictionary<TKey, TValue>` is the generic hash table with strongly typed keys and values, compile-time type checking, and no boxing for value-type keys or values, while `Hashtable` is the legacy non-generic map in `System.Collections` that stores `object` keys and `object` values. Both use hash buckets for average O(1) lookup, but `Dictionary<TKey, TValue>` is the standard for all new C# code.

- `Dictionary<string, int>` rejects wrong key or value types at compile time; `Hashtable` accepts any object and boxes value types.
- `Dictionary` implements `IDictionary<TKey, TValue>` and integrates with LINQ; `Hashtable` implements non-generic `IDictionary`.
- Nullable reference type analysis and `notnull` constraints align with dictionary keys being non-null for correct hashing.
- Maintain `Hashtable` only in legacy code paths; migrate to `Dictionary` at boundaries when refactoring.

---

#### Q2. Explain `IDictionary<TKey, TValue>` and `IReadOnlyDictionary<TKey, TValue>`.

**Answer:** `IDictionary<TKey, TValue>` is the interface contract for mutable key/value maps with `TryGetValue`, indexer, `Add`, and `Remove`, while `IReadOnlyDictionary<TKey, TValue>` exposes read-only access with `Keys`, `Values`, `ContainsKey`, and an indexer getter without mutating methods. Public APIs often return the read-only interface to hide internal dictionary storage.

- `Dictionary<TKey, TValue>` implements both interfaces; you can cast or return as `IReadOnlyDictionary` from a property getter.
- Read-only interface prevents callers from adding keys but does not stop mutations if they still hold a reference to the underlying `Dictionary`.
- LINQ extension methods work on `IEnumerable<KeyValuePair<TKey,TValue>>` from dictionary enumeration.
- Choose `IReadOnlyDictionary` for configuration snapshots and lookup tables exposed from domain services.

---

#### Q3. How does `Dictionary<TKey, TValue>` handle hashing and collisions?

**Answer:** The dictionary computes a hash code for each key, maps that hash to a bucket index, and stores entries in that bucket; when two keys land in the same bucket (a collision), the implementation chains or probes within the bucket structure until it finds an empty slot or the matching key. Correct `GetHashCode` and `Equals` on keys keep lookups average O(1); pathological collisions degrade toward O(n).

- On insert, hash → bucket index → search chain for duplicate key or append new entry.
- On lookup, same hash → bucket → compare keys with `Equals` along the chain.
- The BCL resizes and rehashes when load factor grows, spreading entries across more buckets.
- See Gotcha 13 — constant hash codes collapse performance to linear scan.

---

#### Q4. What is the difference between `Dictionary.Add` and the indexer when the key already exists?

**Answer:** `Add(key, value)` throws `ArgumentException` if the key is already present, preserving the existing entry and signaling programmer error, while the indexer `dict[key] = value` overwrites the value for an existing key or inserts a new entry if the key was absent. Use `Add` when duplicate keys are invalid; use the indexer for upsert semantics.

- `Add` is appropriate when inserting into a map that should never collide on keys (unique ids).
- Indexer assignment implements "set or replace" in one operation — common in caches and aggregation maps.
- `TryAdd` (.NET Core / modern .NET) adds only if missing without throwing.
- See Gotcha 8 — `Add` throws; indexer overwrites silently.

---

#### Q5. What is the difference between `ContainsKey`, `TryGetValue`, and the indexer for lookup?

**Answer:** `ContainsKey` returns a bool without retrieving the value (two lookups if you then index), `TryGetValue` performs one lookup and outputs the value when found, and the indexer `dict[key]` returns the value but throws `KeyNotFoundException` when the key is missing. Prefer `TryGetValue` in hot paths and when absence is an expected case.

- `if (dict.ContainsKey(k)) { var v = dict[k]; }` — two hash lookups.
- `if (dict.TryGetValue(k, out var v)) { … }` — single lookup, idiomatic C#.
- Indexer throw is fine when a missing key is truly exceptional and should fail fast.
- `GetValueOrDefault` and nullable-friendly helpers reduce boilerplate for optional keys.

---

#### Q6. Why must keys be immutable (or stable) after insertion for correct hash table behavior?

**Answer:** The dictionary stores each entry in a bucket determined by the key's hash code at insert time; if mutable key state changes so `GetHashCode()` or `Equals` changes after insertion, later lookups search the wrong bucket and fail to find the entry even though it still occupies memory. Keys must either be immutable types or treat all equality-relevant fields as frozen after the key enters the map.

- Mutating a property on a key object used as `Customer` id changes hash → `TryGetValue` returns false for the "same" logical key.
- String and int keys are naturally immutable; mutable class keys are a design smell for dictionary keys.
- See Gotcha 2 — mutable keys cause silent lookup failures.
- Value types as keys copy on insert — mutating a struct after adding does not affect the stored copy, but reference-type keys share identity problems when mutated.

---

#### Q7. What exception is thrown when accessing a missing key via the indexer?

**Answer:** The dictionary indexer throws `KeyNotFoundException` when the key is not present in the map. This differs from `TryGetValue`, which returns false, and from `Add`, which throws `ArgumentException` on duplicate keys rather than on missing ones.

- Message includes the missing key for diagnostics in many BCL versions.
- Use `TryGetValue` or `ContainsKey` when missing keys are normal control flow, not exceptional.
- `CollectionExtensions.GetValueOrDefault` returns `default(TValue)` instead of throwing when available.

---

#### Q8. Can `null` be used as a key when `TKey` is a reference type?

**Answer:** For `Dictionary<TKey, TValue>` where `TKey` is a reference type without nullable constraints, the BCL allows one `null` key because the implementation has a dedicated path for null references in hashing. With nullable reference types enabled, using `null` as a key produces compiler warnings and is discouraged because it complicates reasoning and conflicts with `where TKey : notnull` on newer APIs.

- `Dictionary<string, int>` with NRT: `dict[null!] = 1` may warn; runtime still accepts null for reference keys in classic behavior.
- Prefer sentinel objects or `Optional<T>` patterns instead of null keys in new designs.
- `Hashtable` historically allowed one null key similarly.

---

#### Q9. What is the average vs worst-case time complexity for lookup, insert, and remove?

**Answer:** With a good hash function and balanced load, `Dictionary` lookup, insert, and remove are average O(1), but worst-case O(n) when many keys collide into the same buckets or hash codes are adversarial. Resizing and rehashing amortizes insert cost over many operations.

- Average case assumes `GetHashCode` spreads keys and `Equals` is O(1).
- Worst case: all keys hash to one bucket → linear scan like a linked list.
- Sorted dictionaries trade O(log n) guaranteed operations for different structure (tree vs hash).
- Security-sensitive code sometimes uses randomized hash seeding to mitigate hash-flooding denial of service on untrusted keys.

---

#### Q10. What is the hash code contract between `GetHashCode` and `Equals` for custom key types?

**Answer:** If two keys are equal according to `Equals`, they must produce the same hash code from `GetHashCode`; violating this contract breaks hash tables because equal objects may land in different buckets and appear missing. The converse is not required: unequal objects may share hash codes (collisions), handled by `Equals` after bucket lookup.

- Override both `Equals` and `GetHashCode` together on custom key types, or use a record which generates them consistently.
- Reference equality default on `object` is wrong for value-based keys (two different instances with same id).
- Do not use mutable fields in `GetHashCode` if those fields can change after insertion.
- `IEqualityComparer<TKey>` centralizes equality when you cannot modify the key type itself.

---

#### Q11. What is `IEqualityComparer<TKey>`, and when do you pass a custom comparer to the constructor?

**Answer:** `IEqualityComparer<TKey>` supplies custom `Equals` and `GetHashCode` for keys, and `Dictionary<TKey, TValue>` constructors accept it when keys do not implement correct equality themselves or when you need case-insensitive string keys, culture-specific rules, or projected equality (compare only part of an object).

- `new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)` — common for HTTP header or SKU maps.
- `new HashSet<Person>(new ByIdComparer())` — same comparer pattern for sets.
- Without a comparer, `EqualityComparer<TKey>.Default` uses `IEquatable<T>` or `object.Equals`.
- Pass a comparer when reusing a type you cannot modify or when multiple equality semantics are needed in different dictionaries.

---

#### Q12. When would you choose `Dictionary` over `List` for lookups by id or SKU?

**Answer:** Use a `Dictionary` when you need fast lookup, insert, and remove by a unique key (id, SKU, email) with average O(1) cost, and use a `List` when you primarily iterate in insertion order or access by numeric index without key-based retrieval. Scanning a list for each lookup is O(n); a dictionary maps key directly to value.

- Ten thousand products by SKU: `dict[sku]` vs `list.First(p => p.Sku == sku)` — dictionary wins on repeated queries.
- Lists suit ordered display, sequential processing, and small collections where linear scan is negligible.
- Combine both: list for display order, dictionary built once for id → entity index or reference.
- See Gotcha 10 — dictionary enumeration order is undefined; lists preserve insertion order unless sorted.

---

#### Q13. What happens internally when two keys hash to the same bucket?

**Answer:** The dictionary stores multiple entries in or linked from that bucket and, on lookup, compares the requested key with each candidate using `Equals` until it finds a match or exhausts the chain. Collisions are expected and normal; they only hurt performance when nearly all keys share the same bucket.

- Insert: walk bucket chain, replace if key equals existing, else append new entry.
- Remove: unlink entry from chain and may shrink table if load factor drops.
- Poor `GetHashCode` that returns a constant makes every key share one bucket — effectively a list.
- Modern dictionary implementations use optimized entry layouts (open addressing or chained entries) but the contract remains hash then equals.

---

### 05. HashSet

#### Q1. Explain `HashSet<T>` and its use cases. How is it different from `List<T>`?

**Answer:** `HashSet<T>` is an unordered collection that enforces uniqueness using hash-based membership testing with average O(1) `Contains`, while `List<T>` allows duplicates and offers O(1) indexing but O(n) membership scans. Use a hash set when you care whether an item is already present, need set algebra, or want automatic deduplication.

- `set.Add("WH-4412")` returns `false` if the SKU was already in the set; `list.Add` always appends.
- Lists preserve insertion order and support duplicates; sets ignore duplicate adds silently (returning false).
- Typical uses: visited nodes in graph traversal, distinct tag collection, permission flags as unique strings.
- Sets do not support indexing by position — use `List<T>` when order and duplicates matter.

---

#### Q2. What is the difference between `SortedSet<T>` and `HashSet<T>`?

**Answer:** `HashSet<T>` is unordered and uses hash buckets for average O(1) operations, while `SortedSet<T>` keeps elements in sorted order using a tree structure with O(log n) add, remove, and contains. Choose `HashSet` for raw uniqueness and speed; choose `SortedSet` when you need in-order enumeration or min/max without sorting a copy each time.

- `SortedSet<T>` implements `IComparer<T>`-based ordering from construction or default comparer.
- `HashSet<T>` enumeration order is undefined and may change when the set resizes.
- `SortedSet` supports `Min`, `Max`, and range views (`GetViewBetween`) efficiently.

---

#### Q3. Why does `HashSet<T>` require correct `GetHashCode()`/`Equals()` for custom types?

**Answer:** Like `Dictionary`, `HashSet` places elements in buckets using hash codes and confirms membership with `Equals`; if two distinct instances that represent the same logical value are not equal, duplicates appear in the set, and if equal objects produce different hash codes, membership tests fail unpredictably.

- Two `Person` objects with the same `Id` must be equal and share hash codes or the set stores both.
- Records with value equality work well as set elements for immutable data.
- Pass `IEqualityComparer<T>` when type authors did not override equality correctly.

---

#### Q4. What set operations does `HashSet<T>` provide (`UnionWith`, `IntersectWith`, `ExceptWith`, `SymmetricExceptWith`)?

**Answer:** These methods mutate the current set in place: `UnionWith` adds all elements from the other set, `IntersectWith` keeps only elements also in the other, `ExceptWith` removes elements present in the other, and `SymmetricExceptWith` keeps elements that appear in exactly one of the two sets.

- `a.UnionWith(b)` — a becomes a ∪ b; `a.IntersectWith(b)` — a becomes a ∩ b.
- Non-mutating equivalents exist in LINQ (`Union`, `Intersect`, `Except`) returning new sequences.

---

#### Q5. What is the difference between `Add` returning `false` on duplicate vs `List.Add` behavior?

**Answer:** `HashSet<T>.Add` returns `false` when the element was already in the set and does not change the set, while `List<T>.Add` always appends and returns void, allowing duplicate entries.

- `if (set.Add(item)) { /* first time seeing item */ }` — idiomatic distinct detection.
- List duplicate detection requires `Contains` then `Add` — O(n) per check without a set.

---

#### Q6. How do you construct a `HashSet<T>` with custom equality (`IEqualityComparer<T>`)?

**Answer:** Pass an `IEqualityComparer<T>` to the constructor: `new HashSet<string>(StringComparer.OrdinalIgnoreCase)` or `new HashSet<Product>(new ProductSkuComparer())`. The set uses that comparer's `Equals` and `GetHashCode` for all operations.

- Use the same comparer when merging sets that must agree on equality.

---

#### Q7. When would you use `HashSet<T>` for deduplication vs `Distinct()` in LINQ?

**Answer:** Use `HashSet<T>` when building a deduplicated collection incrementally in imperative code or when you need the set for later membership tests; use `Distinct()` when deduplication is one step in a LINQ pipeline over an existing sequence.

- `foreach (var x in source) set.Add(x);` — imperative multi-source dedupe.
- `source.Select(...).Distinct().ToList()` — declarative pipeline with deferred execution until materialized.

---

#### Q8. What is the difference between set membership test in `HashSet` vs scanning a `List`?

**Answer:** `HashSet.Contains` is average O(1) hash lookup, while `List.Contains` or linear search is O(n). For repeated membership queries on growing data, the set scales dramatically better.

- Small fixed lists may scan faster due to cache locality; large or growing data favors hash sets.

---

#### Q9. What is `IsSubsetOf`, `IsSupersetOf`, and `Overlaps` used for?

**Answer:** These compare two sets without mutating either: `IsSubsetOf` tests whether every element of this set is in the other, `IsSupersetOf` tests the reverse, and `Overlaps` returns whether they share at least one element.

- Example: `required.IsSubsetOf(granted)` for permission coverage checks.

---

#### Q10. Can you modify an element in a `HashSet` in place if it affects equality — what goes wrong?

**Answer:** Mutating equality-relevant fields breaks bucket placement: `Contains` may return false while the object remains in the set, and re-adding can create duplicates. Treat set elements as immutable with respect to equality while stored.

- Remove, mutate, re-add is the workaround if mutation is unavoidable.

---

### 06. Queue and Stack

#### Q1. Explain `Queue<T>` and `Stack<T>` vs their non-generic counterparts.

**Answer:** `Queue<T>` and `Stack<T>` in `System.Collections.Generic` are type-safe, end-restricted collections where you add and remove only at defined ends, while the legacy `Queue` and `Stack` in `System.Collections` store `object` references and require casts at read time. Both generic pairs encode access rules in the type itself so you cannot accidentally remove from the wrong end the way you might with an unrestricted `List<T>`.

- Generic versions avoid boxing when you store value types such as `int` or `struct` frames; non-generic versions box every value type on insert.
- `Queue<T>` implements FIFO (first in, first out); `Stack<T>` implements LIFO (last in, first out).
- Both implement `IEnumerable<T>` for forward iteration but expose no indexer — random access is intentionally unsupported.
- Prefer generic types for all new code; non-generic counterparts remain only for legacy API interop.

---

#### Q2. What is FIFO vs LIFO, and which collection maps to each?

**Answer:** FIFO means the oldest inserted item is removed first, which maps to `Queue<T>` (`Enqueue` at the back, `Dequeue` from the front). LIFO means the most recently inserted item is removed first, which maps to `Stack<T>` (`Push` and `Pop` both at the top).

- FIFO models fair waiting lines: print jobs, support tickets, breadth-first graph layers.
- LIFO models nested undo history, expression evaluation, and depth-first backtracking — the same ordering the runtime call stack uses.
- Choosing the wrong collection silently produces wrong business logic even though both types compile and run.

---

#### Q3. What operations does `Queue<T>` expose (`Enqueue`, `Dequeue`, `Peek`, `TryDequeue`, `TryPeek`)?

**Answer:** `Enqueue` adds an item to the tail, `Dequeue` removes and returns the item at the head, and `Peek` inspects the head without removing it. `TryDequeue` and `TryPeek` are non-throwing variants that return `false` when the queue is empty instead of raising `InvalidOperationException`.

- `Count` reports current size; `Clear` removes all items; `Contains` performs a linear O(n) scan.
- `ToArray` returns a snapshot in FIFO order without modifying the queue.
- Prefer `TryDequeue`/`TryPeek` in polling loops where an empty queue is normal rather than exceptional.
- Internally `Queue<T>` uses a circular buffer that resizes as needed, giving amortized O(1) enqueue and dequeue.

---

#### Q4. What operations does `Stack<T>` expose (`Push`, `Pop`, `Peek`, `TryPop`)?

**Answer:** `Push` adds an element on top, `Pop` removes and returns the top element, and `Peek` reads the top without removing it. `TryPop` and `TryPeek` mirror the queue's safe pattern by returning `false` when the stack is empty.

- `Count`, `Clear`, and `Contains` behave analogously to `Queue<T>`.
- `foreach` walks from top to bottom (most recently pushed first), matching LIFO inspection order.
- Empty `Pop()` or `Peek()` throw `InvalidOperationException` — use `TryPop` when emptiness is expected.
- The stack grows via an internal array resize strategy similar to `List<T>`.

---

#### Q5. Why do `Queue` and `Stack` not support random access by index?

**Answer:** Their contract is end-restricted access only: a queue exposes head and tail operations, and a stack exposes top operations. Allowing arbitrary indexing would break the abstraction and encourage algorithms that perform poorly on these structures, such as scanning from the middle when a `List<T>` or array is the correct choice.

- Indexing would imply O(1) middle access, but removing from the front of an array-backed queue is O(n) if implemented naively — the type hides that complexity behind `Dequeue`.
- Restricted APIs make misuse harder: you cannot `Insert(0, item)` on a queue and accidentally break FIFO ordering.
- When you need both ordering discipline and random access, use `List<T>` and enforce conventions in your own code, or pick a deque-style structure.

---

#### Q6. How is `Queue<T>` used in breadth-first search (BFS) on a graph or grid?

**Answer:** BFS enqueues the start node, then repeatedly dequeues the current node, processes it, and enqueues each unvisited neighbor until the queue is empty or the goal is found. A `HashSet<T>` (or boolean visited array) tracks nodes already enqueued or visited so you do not revisit them.

- On a grid, neighbors are typically the four or eight adjacent cells; on a graph, neighbors come from adjacency lists or edge lists.
- The queue guarantees nodes are expanded in order of increasing distance from the start when edge weights are uniform.
- `TryDequeue` fits the main loop: `while (queue.TryDequeue(out var current)) { ... }`.
- BFS memory is proportional to the frontier width, which can be large on dense graphs.

---

#### Q7. Why does BFS find shortest paths in unweighted graphs?

**Answer:** BFS discovers nodes layer by layer: all nodes at distance 1 are dequeued before any node at distance 2, and so on. The first time you dequeue the goal node, you have reached it along a path with the minimum number of edges because no shorter path could have been skipped.

- Each edge has equal cost, so path length equals edge count — BFS minimizes edge count, not weighted cost.
- Storing parent pointers during BFS lets you reconstruct the shortest path by walking backward from the goal.
- For weighted graphs, Dijkstra's algorithm or A* replaces BFS; a plain queue is no longer sufficient.

---

#### Q8. What real-world workflows map naturally to a stack (undo/redo, call stack, DFS)?

**Answer:** Any workflow where the most recent action must be reversed or completed before older ones fits a stack: text-editor undo stacks, browser history back navigation (often modeled as stacks), recursive depth-first search backtracking, and the runtime call stack itself.

- Undo pushes a snapshot before each edit and pops to restore the previous state — LIFO matches "undo last change first."
- DFS on a graph can use an explicit `Stack<T>` instead of recursion to avoid stack overflow on deep graphs.
- Expression evaluation and syntax parsing use operand and operator stacks.
- Redo stacks are often a second stack fed by undo pops.

---

#### Q9. What is the difference between non-generic `Queue`/`Stack` and generic versions regarding boxing?

**Answer:** Non-generic `Queue` and `Stack` store elements as `object`, so every value type is boxed on insert and must be unboxed (with a cast) on removal. Generic `Queue<T>` and `Stack<T>` store `T` directly, eliminating boxing overhead and providing compile-time type safety.

- Boxing allocates on the heap and adds indirection — costly in hot loops with many value-type pushes.
- Reading from a non-generic stack returns `object`, inviting `InvalidCastException` if the wrong type is assumed.
- Legacy APIs may still expose non-generic collections; wrap or migrate at boundaries when possible.

---

#### Q10. When would you use `Queue<T>` over `List<T>` with remove-from-front patterns?

**Answer:** Use `Queue<T>` when you repeatedly add at one end and remove from the other (FIFO), because `List<T>.RemoveAt(0)` shifts every remaining element and costs O(n) per removal. `Queue<T>` provides amortized O(1) enqueue and dequeue with a circular buffer designed for that access pattern.

- A ticket processor that always takes the oldest item should use a queue, not `list.RemoveAt(0)`.
- `List<T>` remains better when you need indexing, sorting, binary search, or insertion at arbitrary positions.
- If you only occasionally drain from the front on a small collection, a list may be acceptable — profile before optimizing.
- See Gotcha 14 — `Queue.Contains` is O(n); pair with `HashSet<T>` if you need fast membership checks.

---

### 07. SortedList & SortedDictionary

#### Q1. What is `SortedList<TKey, TValue>` and `SortedDictionary<TKey, TValue>`? When would you use each?

**Answer:** Both are generic sorted associative maps that keep keys in ascending order (by natural order or a supplied `IComparer<TKey>`), unlike `Dictionary<TKey, TValue>` where key order is undefined. `SortedList` is backed by two parallel sorted arrays; `SortedDictionary` uses a red-black tree — choose based on size, insert pattern, and whether you need O(1) access by sorted rank.

- Use either when you must iterate keys in sorted order without sorting a copy at read time.
- `SortedList` suits small to moderate counts with frequent indexed access to the n-th key.
- `SortedDictionary` suits larger maps with frequent inserts and deletes where array shifting would hurt.
- Both require unique keys; duplicate `Add` throws `ArgumentException`, while the indexer updates existing keys.

---

#### Q2. What interface defines ordering for sorted collections (`IComparer<TKey>` vs `IEqualityComparer<TKey>`)?

**Answer:** Sorted collections use `IComparer<TKey>` (or the key type's `IComparable<TKey>` implementation) to determine sort order — whether one key is less than, equal to, or greater than another. `IEqualityComparer<TKey>` defines equality and hash codes for hash-based structures like `Dictionary` and `HashSet`, not ordering for sorted maps.

- Pass a custom `IComparer<TKey>` to the constructor for case-insensitive string keys or domain-specific ordering.
- `Comparer<T>.Default` uses `IComparable<T>` when available.
- Using the wrong comparer interface is a compile-time error for sorted types — they do not accept `IEqualityComparer<TKey>`.
- See Gotcha 12 — confusing the two comparer types is a common mistake.

---

#### Q3. What is the difference between `SortedList` (array-backed) and `SortedDictionary` (tree-backed) performance?

**Answer:** Both offer O(log n) key lookup via binary search (SortedList) or tree traversal (SortedDictionary), but insert and remove costs differ: SortedList may shift O(n) elements in its arrays, while SortedDictionary rebalances the tree in O(log n) without shifting a large block.

- SortedList uses less memory per entry for small counts — compact arrays, no tree node overhead.
- SortedDictionary scales better under heavy churn with many entries.
- SortedList `ContainsValue` scans values linearly — O(n).
- Profile with your actual key count and insert/delete ratio rather than assuming one is always faster.

---

#### Q4. When is `SortedList` preferred over `SortedDictionary` for memory or indexed access?

**Answer:** Prefer `SortedList` when the map stays relatively small, inserts are infrequent or batched, and you need O(1) random access to the i-th key or value via `Keys[i]` and `Values[i]`. The dual-array layout is memory-efficient and exposes sorted rank directly.

- Reporting "top 10 SKUs by sorted key" can read `Keys[0]` through `Keys[9]` without iterating the whole map.
- `IndexOfKey` and `IndexOfValue` use binary search — O(log n) rank lookup.
- When the map grows large with constant inserts, shifting arrays dominates and `SortedDictionary` wins.
- `Capacity` on SortedList pre-allocates like `List<T>` to reduce resize churn.

---

#### Q5. What is the cost of inserting out-of-order keys into a sorted collection?

**Answer:** Each insert must place the key in sorted position: SortedList shifts array elements to keep keys ordered (O(n) in the worst case), and SortedDictionary walks and rebalances the tree (O(log n) per insert). Inserting keys already in ascending order is still O(log n) per key for the tree, but SortedList may benefit slightly from append-at-end patterns when keys arrive sorted.

- Bulk loading from presorted data may be cheaper with a `Dictionary` plus one sort pass if you do not need incremental sorted visibility.
- Random key arrival order on a large SortedList is the worst case for shifting cost.
- Updates to existing keys via the indexer replace values without changing key order or count.

---

#### Q6. Can you look up by index in `SortedList` — what does `Keys[index]` provide?

**Answer:** Yes — `SortedList<TKey, TValue>` exposes `IList<TKey> Keys` and `IList<TValue> Values` where index `i` refers to the i-th entry in sorted key order. `Keys[0]` is the smallest key under the current comparer, and `Values[i]` is the value paired with `Keys[i]`.

- This O(1) rank access is a major advantage over `SortedDictionary`, which has no key indexer.
- `RemoveAt(index)` removes the entry at that sorted rank and shifts remaining entries.
- Index is not the hash bucket or insertion order — it is always sort rank.
- Bounds violations throw `ArgumentOutOfRangeException` like any list indexer.

---

#### Q7. What is the difference between `SortedSet<T>` and `SortedDictionary<TKey, TValue>`?

**Answer:** `SortedSet<T>` stores unique sorted elements with no associated value payload — it is a set, not a map. `SortedDictionary<TKey, TValue>` stores unique sorted keys each mapped to a value, like a sorted version of `Dictionary`.

- Use `SortedSet` for ordered unique membership (`Min`, `Max`, set operations with ordering).
- Use `SortedDictionary` when every key carries data — prices by SKU, counts by date.
- Both use tree-based ordering with O(log n) operations; neither replaces `Dictionary` for raw lookup speed when order is irrelevant.
- `SortedSet` corresponds to `HashSet`; `SortedDictionary` corresponds to `Dictionary`.

---

#### Q8. When would you choose `SortedDictionary` over sorting keys from a `Dictionary` at read time?

**Answer:** Choose `SortedDictionary` when you need keys to stay sorted on every insert, update, or delete and you enumerate in order frequently during the object's lifetime. Sorting a `Dictionary`'s keys into a list on each read costs O(n log n) per pass and still leaves the underlying dictionary unordered for the next operation.

- A live leaderboard or calendar keyed by timestamp benefits from incremental sorted maintenance.
- One-off export where you read once and discard may be cheaper: copy keys to a list and call `Sort`.
- `SortedDictionary` avoids allocating a new sorted key list on every foreach when order matters continuously.
- If lookups dominate and order is needed only rarely, measure both approaches with realistic data sizes.

---

### 08. IEnumerable & IEnumerator

#### Q1. What is the difference between `IEnumerable<T>` and `ICollection<T>`?

**Answer:** `IEnumerable<T>` is the minimal sequence contract — it only requires `GetEnumerator()` so callers can traverse items forward. `ICollection<T>` extends `IEnumerable<T>` with mutability and size metadata: `Count`, `Add`, `Remove`, `Clear`, and `Contains`.

- You can foreach any `ICollection<T>` because it is also an `IEnumerable<T>`.
- API parameters that only read data should accept `IEnumerable<T>` to allow lazy sequences, not just concrete collections.
- `IEnumerable<T>` alone does not guarantee a materialized in-memory collection — it may be a deferred LINQ pipeline.
- `ICollection<T>` implies the implementer supports membership and structural changes, though not necessarily indexing.

---

#### Q2. What is the difference between `ICollection<T>` and `IList<T>`?

**Answer:** `IList<T>` extends `ICollection<T>` with positional access: an indexer `this[int index]`, `Insert`, and `RemoveAt`. `ICollection<T>` tells you how many items exist and lets you add or remove by value, but not access or insert at an arbitrary index.

- `List<T>` implements `IList<T>`; `HashSet<T>` implements `ICollection<T>` but not `IList<T>` because sets are unordered.
- Accept `IList<T>` when callers need index-based read or insert; accept `ICollection<T>` when only add/remove/count matters.
- Both extend `IEnumerable<T>` — foreach works on either.
- `IReadOnlyList<T>` (see Q3) splits read-only indexing from mutable `IList<T>`.

---

#### Q3. What are `IReadOnlyList<T>` and `IReadOnlyCollection<T>`?

**Answer:** These interfaces expose read-only views of collections: `IReadOnlyCollection<T>` provides `Count` and enumeration, and `IReadOnlyList<T>` adds an indexer for positional read access without `Add`, `Remove`, or `Insert`. They let APIs return or accept collections that callers must not mutate through that reference.

- `List<T>.AsReadOnly()` returns an `IReadOnlyList<T>` wrapper — mutations to the underlying list still appear through the wrapper (see Gotcha 9).
- Prefer `IReadOnlyList<T>` over `IEnumerable<T>` when count or index access is needed without materializing.
- Implementing types may still be mutable elsewhere — the interface only restricts the typed view.
- Modern BCL types like `List<T>` implement both mutable and read-only interfaces on the same instance.

---

#### Q4. What is the difference between `IEnumerator` and `IEnumerator<T>`?

**Answer:** Both are forward-only cursors with `MoveNext()` and `Current`, but `IEnumerator<T>` is strongly typed (`Current` returns `T`) and extends `IDisposable`. Non-generic `IEnumerator` returns `object` from `Current`, which may box value types.

- `GetEnumerator()` on `IEnumerable<T>` returns `IEnumerator<T>`.
- `foreach` compiles to `GetEnumerator`, a `try/finally`, and `Dispose` on the enumerator.
- Non-generic enumeration remains for legacy interop; generic code should use `IEnumerator<T>`.
- `Reset()` exists on both but is poorly supported — prefer a fresh enumerator instead of resetting.

---

#### Q5. What is the `yield` keyword, and how does iterator methods relate to `IEnumerable<T>`?

**Answer:** `yield return` and `yield break` in an iterator method cause the compiler to generate a state machine class implementing `IEnumerable<T>` and/or `IEnumerator<T>`. Each `yield return` pauses the method and produces one element when the consumer advances enumeration.

- Iterator methods can lazily generate sequences without building a full in-memory list first.
- The method body runs incrementally — work happens only as `MoveNext` is called.
- `yield break` ends iteration early, similar to `return` in a non-iterator method.
- Iterator methods cannot have `ref`/`out` parameters and have restrictions on `try/catch` around yields.

---

#### Q6. What is the difference between deferred execution and immediate execution for IEnumerable sequences?

**Answer:** Deferred execution means the query or iterator does not run until enumeration begins — each `MoveNext` pulls the next element through the pipeline. Immediate execution materializes results right away, as `ToList()`, `ToArray()`, `Count()`, or `List.Add` do, computing and storing (or counting) all elements at call time.

- LINQ operators like `Where` and `Select` are deferred; they build a description of work, not the result list.
- Calling `ToList()` forces the entire pipeline to run once and caches the output.
- Deferred sequences re-run their source logic on each full enumeration unless materialized.
- See Gotcha 11 — multiple passes over deferred LINQ can repeat expensive work.

---

#### Q7. What is the iterator pattern — what do `MoveNext`, `Current`, and `Reset` do?

**Answer:** The iterator pattern separates a collection (`IEnumerable<T>`) from the cursor (`IEnumerator<T>`) that walks it. `MoveNext()` advances the cursor to the next element and returns `false` when the sequence is exhausted; `Current` returns the element at the cursor's present position (valid only after a successful `MoveNext`).

- Before the first `MoveNext`, `Current` is undefined — do not read it.
- `Reset()` rewinds to before the first element in theory, but many implementations throw `NotSupportedException` — allocate a new enumerator instead.
- `foreach` desugars exactly to this pattern with guaranteed `Dispose` in `finally`.
- Custom collections implement `GetEnumerator()` manually or via `yield return` helper methods.

---

#### Q8. What is `yield break` vs `return` in an iterator method?

**Answer:** In an iterator method, `yield return value` produces the next element and pauses; `yield break` terminates iteration immediately with no further elements. A plain `return` (without yield) also ends the iterator but cannot supply a value — it simply stops enumeration.

- Use `yield break` after a guard clause when there is nothing to yield: `if (source == null) yield break;`.
- `return` in an iterator is legal but easy to confuse with normal methods — prefer `yield break` for clarity when ending early.
- Neither `yield break` nor `return` in an iterator returns a value to the caller like `return 42` in an ordinary method.
- After either statement, no further `yield return` in that execution path runs.

---

#### Q9. Why can multiple enumeration of the same `IEnumerable` from a LINQ query re-run the pipeline?

**Answer:** A LINQ query stored in an `IEnumerable<T>` variable is a deferred recipe, not a cached result list. Each time you foreach or call `GetEnumerator`, enumeration starts fresh from the source through every chained operator.

- If the source is a database or file, re-enumeration may re-query or re-read.
- Side effects in selectors run again on each pass — do not rely on single-run behavior unless materialized.
- `ToList()` or `ToArray()` snapshots results so later passes read memory, not the pipeline.
- See Q14 for when materialization is mandatory.

---

#### Q10. What is the difference between returning `IEnumerable<T>` from a method vs `List<T>`?

**Answer:** Returning `IEnumerable<T>` signals that callers receive a sequence they can enumerate, often lazily, without committing to a mutable list or fixed size. Returning `List<T>` (or another concrete collection) exposes a materialized, indexable, typically mutable collection with known `Count`.

- `IEnumerable<T>` as a return type hides whether results are computed on demand or preloaded — callers should not cast blindly.
- `List<T>` allows callers to index, add, and remove — a wider contract than enumeration alone.
- Iterator methods naturally return `IEnumerable<T>`; methods that build and return all results at once often use `List<T>` or `IReadOnlyList<T>`.
- Defensive APIs return `IReadOnlyList<T>` when indexing is needed without granting mutation.

---

#### Q11. What happens if you modify a collection during `foreach` — how does the enumerator detect it?

**Answer:** Mutating a collection (add or remove elements) while iterating it with `foreach` invalidates the enumerator and throws `InvalidOperationException` with a message that the collection was modified. Most BCL collections track a version stamp incremented on structural change and compare it on each `MoveNext`.

- Removing the current item during foreach is unsafe — use `for` loops backward, or rebuild the collection, or use `RemoveAll`.
- Value-type updates to existing elements (without changing count) may be allowed on some collections but are still risky if they affect enumeration.
- Concurrent modification from another thread has similar undefined behavior — use synchronization or concurrent collections.
- See Gotcha 1 — this is one of the most common collection mistakes.

---

#### Q12. What is covariance on `IEnumerable<out T>` — practical assignment examples?

**Answer:** The `out` modifier on `IEnumerable<T>` makes it covariant: you can assign `IEnumerable<string>` to `IEnumerable<object>` because the interface only produces `T` items — callers never pass a `T` in through the interface. This enables treating a sequence of derived types as a sequence of base types when you only read elements.

- Covariance applies to interfaces and delegates marked with `out T`, not to mutable types like `List<T>`.
- `List<string>` cannot be assigned to `List<object>` — you could add a non-string object and break type safety.
- `IEnumerable<Dog>` to `IEnumerable<Animal>` is safe for read-only foreach over animals.
- See Gotcha 3 — covariance on `IEnumerable` does not extend to mutable lists.

---

#### Q13. What is the difference between `foreach` and manual `while (enumerator.MoveNext())`?

**Answer:** They are equivalent in behavior when written correctly: `foreach` is compiler sugar that obtains an enumerator, loops while `MoveNext()` returns true, assigns `Current` to the loop variable, and disposes the enumerator in a `finally` block. Manual loops expose the enumerator explicitly, which is useful for interleaving two sequences or skipping elements with custom logic.

- Manual code must call `Dispose` — typically `using var e = collection.GetEnumerator();`.
- `foreach` declares the iteration variable as read-only — you cannot assign to it inside the loop.
- Manual loops can inspect `Current` without consuming in specialized merge algorithms.
- Prefer `foreach` for clarity unless you need low-level control.

---

#### Q14. What is `ToList()` materialization, and when must you materialize before multiple passes?

**Answer:** `ToList()` executes the entire deferred sequence and stores results in a new `List<T>`. You must materialize when you need to enumerate multiple times, cache expensive computation, snapshot a changing source, or need count/index without re-running the pipeline.

- After materialization, subsequent foreach passes read the list — the original pipeline does not re-execute.
- Multiple foreach over the same LINQ query without `ToList()` repeats filters, database calls, or file reads.
- Materialization trades memory for predictable single-pass cost at snapshot time.
- `ToArray()` serves a similar role with array backing instead of list resizing semantics.

---

#### Q15. What is the relationship between `IAsyncEnumerable<T>` and iterators (preview)?

**Answer:** `IAsyncEnumerable<T>` is the asynchronous counterpart to `IEnumerable<T>`: consumers use `await foreach` to pull elements, and producers implement async iterator methods with `async`/`await` and `yield return`. Each advance may involve asynchronous I/O without blocking a thread.

- `IAsyncEnumerator<T>` parallels `IEnumerator<T>` with `MoveNextAsync()` instead of synchronous `MoveNext()`.
- Async iterators suit streaming HTTP responses, database readers, and file lines read asynchronously.
- The same lazy, pull-based model applies — work runs as the consumer requests the next item.
- Full async streaming patterns are covered in later async chapters; the mental model matches synchronous `yield return`.

---

### Gotchas — Module 03

#### Gotcha 1. Modify while iterating

**Answer:** Changing a collection's structure (adding or removing elements) during `foreach` invalidates the enumerator and throws `InvalidOperationException`. The collection detects a version change between `MoveNext` calls.

- Removing items matching a condition inside `foreach` is a classic crash — collect keys to remove first, or use `RemoveAll` with a predicate.
- Even single-threaded code hits this — it is not only a concurrency issue.
- Some algorithms require iterating a copy or using index-based loops with careful index management.

---

#### Gotcha 2. Mutable keys

**Answer:** Mutating fields that participate in equality or comparison after inserting a key into a `Dictionary`, `HashSet`, or sorted map breaks lookup — the object may remain stored but `Contains` or the indexer fails to find it.

- Treat keys as immutable while they live inside hash- or tree-based collections.
- The safe pattern is remove, mutate, re-add if mutation is unavoidable.
- Reference types used as keys should override `GetHashCode` and `Equals` consistently and avoid changing those fields.

---

#### Gotcha 3. `IEnumerable<T>` covariant, `List<T>` not

**Answer:** `IEnumerable<out T>` allows assigning a sequence of derived types to a sequence of base types because callers only read elements. `List<T>` is invariant — `List<string>` cannot be treated as `List<object>` because adding to the list would break type safety.

- Covariance does not mean you can mutate through the base-typed view of an `IEnumerable`.
- APIs that accept `IEnumerable<Animal>` can receive `List<Dog>` only through the IEnumerable interface, not as `List<Animal>`.
- Use `IEnumerable<T>` or `IReadOnlyList<T>` for read-only API flexibility.

---

#### Gotcha 4. Wrong collection for the job

**Answer:** Choosing `List<T>` for frequent insertions or deletions in the middle causes O(n) shifts per operation. Choosing `Dictionary` when order matters without sorting leads to unpredictable iteration order.

- Match structure to access pattern: queue for FIFO, stack for LIFO, dictionary for key lookup, hash set for uniqueness.
- Profile hot paths — algorithmic complexity dominates at scale.
- See Q10 in Queue chapter — `RemoveAt(0)` on a list is not a queue.

---

#### Gotcha 5. Static fields on generic types

**Answer:** Each closed constructed generic type (`Generic<int>`, `Generic<string>`) gets its own static field storage. Static data is not shared across all instantiations of `Generic<T>`.

- A static counter in `Cache<T>` counts per `T`, not globally across all types.
- This enables type-specific singletons but surprises developers expecting one static slot for all generics.
- Explicit non-generic base classes can centralize shared static state when needed.

---

#### Gotcha 6. Boxing in non-generic collections

**Answer:** `ArrayList`, non-generic `Queue`, and `Hashtable` store `object`, boxing every value type on insert and requiring casts on read. This adds allocation, indirection, and runtime cast failures.

- `List<int>` stores ints directly on the heap array without boxing.
- Legacy interop may force non-generic collections — convert at boundaries.
- Boxing overhead matters in tight loops with millions of value-type elements.

---

#### Gotcha 7. Passing `List<T>` by value

**Answer:** Passing a `List<T>` to a method copies the reference, not the list contents — both caller and callee share the same underlying collection. Mutations through either reference are visible to both.

- C# passes references by value — the reference is copied, not the object graph.
- To prevent aliased mutation, pass `IReadOnlyList<T>`, clone the list, or use immutable collections.
- `ref List<T>` is rarely needed unless reassigning the reference itself.

---

#### Gotcha 8. `Dictionary.Add` vs indexer on duplicate key

**Answer:** `dictionary.Add(key, value)` throws `ArgumentException` if the key already exists. The indexer `dictionary[key] = value` inserts or overwrites silently without throwing on duplicate keys.

- Use `Add` when duplicates indicate a bug; use the indexer for upsert semantics.
- `TryAdd` (.NET Core / modern .NET) adds only if missing without throwing.
- Idempotent initialization may prefer indexer or `TryAdd`; strict invariants prefer `Add`.

---

#### Gotcha 9. `AsReadOnly()` is a view

**Answer:** `List<T>.AsReadOnly()` wraps the same underlying list — it does not copy elements. Mutations to the original list through its mutable API remain visible through the read-only wrapper.

- The wrapper prevents add/remove through `IReadOnlyList<T>` but does not stop other holders of the mutable list reference.
- For true isolation, expose a copy: `list.ToList()` or `list.AsReadOnly()` on a list you no longer mutate.
- Callers receiving `IReadOnlyList<T>` should not assume immutability of the backing store unless documented.

---

#### Gotcha 10. Assuming dictionary enumeration order

**Answer:** `Dictionary<TKey, TValue>` does not guarantee any particular key order during `foreach` — order may change when the table resizes and must not be relied upon for display or tests.

- Sort keys explicitly: `foreach (var key in dict.Keys.OrderBy(k => k))`.
- Use `SortedDictionary` or `SortedList` when sorted iteration is a requirement.
- Unit tests that assert dictionary key order are fragile across runtimes and sizes.

---

#### Gotcha 11. Multiple enumeration cost

**Answer:** Deferred `IEnumerable` sequences from LINQ or `yield return` re-execute from the source on each full enumeration. An expensive filter or I/O-bound source run twice if you foreach twice without materializing.

- Call `ToList()` or `ToArray()` once when multiple passes are required.
- Be cautious passing deferred sequences to methods that enumerate more than once internally.
- Database LINQ providers may translate each enumeration to a new SQL query.

---

#### Gotcha 12. Wrong comparer on sorted types

**Answer:** `SortedDictionary` and `SortedList` order keys with `IComparer<TKey>`, not `IEqualityComparer<TKey>`. Passing or confusing equality comparers from dictionary/hash code does not apply to sorted collections.

- Constructor overload accepts `IComparer<TKey>` or uses `Comparer<TKey>.Default`.
- Custom string sorting (case-insensitive) uses `StringComparer.OrdinalIgnoreCase` as an `IComparer<string>`.
- Equality and ordering are related but distinct contracts — sort requires total ordering rules.

---

#### Gotcha 13. Poor `GetHashCode` distribution

**Answer:** If many distinct keys return the same hash code from `GetHashCode()`, the dictionary degrades toward linear search within one bucket — O(n) lookups instead of average O(1). Constant hash codes are the worst case.

- Always combine fields in `GetHashCode` when equality uses multiple fields.
- Do not return a constant except for intentional singleton-key scenarios.
- Use `HashCode.Combine` in modern C# for consistent mixing of fields.

---

#### Gotcha 14. Queue `Contains` is O(n)

**Answer:** `Queue<T>.Contains` scans the entire queue linearly — it is not a hash lookup. Frequent membership tests on a queue are expensive.

- Maintain a parallel `HashSet<T>` of enqueued items if you need O(1) "already in queue?" checks (with careful removal to keep set in sync).
- Choose the right structure: queue for ordering, hash set for membership.
- Same applies to `Stack<T>.Contains` — linear scan from top to bottom.

---

## Module 04. Functional Style Programming

### 01. Delegates

#### Q1. What is Functional Programming, and how does C# support it without being a purely functional language?

**Answer:** Functional programming emphasizes immutable data, pure functions, and treating functions as values you pass and compose. C# is primarily object-oriented and imperative, but it supports functional techniques through delegates, lambdas, LINQ, records, and immutability features without requiring pure functional style everywhere.

- You can mix paradigms: domain models as classes with LINQ pipelines for queries.
- `Func`, `Action`, and `Predicate` make higher-order functions first-class in practice.
- Records and `init` accessors reduce accidental mutation compared to classic mutable classes.
- Enterprise codebases typically adopt selective functional patterns rather than banning all side effects.

---

#### Q2. What are the key principles of Functional Programming (immutability, pure functions, first-class functions, higher-order functions, referential transparency)?

**Answer:** Immutability means data does not change after creation; pure functions always return the same output for the same input and cause no observable side effects; first-class functions can be assigned, passed, and returned like any value; higher-order functions take or return other functions; referential transparency means you can replace a function call with its result without changing program behavior.

- Immutability simplifies reasoning in concurrent code because shared state cannot be silently modified.
- Pure functions are easier to test — no hidden database or static dependencies.
- LINQ's `Where` and `Select` are higher-order functions accepting delegate arguments.
- C# rarely enforces purity at compile time — discipline and design patterns carry the guarantee.

---

#### Q3. What is the difference between imperative and declarative programming styles? Give a C# example of each.

**Answer:** Imperative code describes how to accomplish a task step by step with explicit loops and mutations. Declarative code describes what result you want and lets libraries or the compiler determine the steps — LINQ query expressions are the familiar C# example.

- Imperative: `var results = new List<int>(); foreach (var n in nums) if (n % 2 == 0) results.Add(n * 2);`
- Declarative: `var results = nums.Where(n => n % 2 == 0).Select(n => n * 2).ToList();`
- Both may compile to similar logic, but declarative style hides loop and temporary index details.
- Declarative LINQ can defer execution until enumeration — imperative list building is typically eager.

---

#### Q4. What does it mean for functions to be first-class citizens in C#?

**Answer:** First-class functions can be stored in variables, passed as method arguments, returned from methods, and composed at runtime — delegates and lambdas give methods the same flexibility as data. Method group conversion lets you assign a method name to a delegate-typed variable without an explicit wrapper method.

- `Func<int, int> f = Math.Abs;` — method group converts to delegate instance.
- Delegates are reference types on the heap (unless optimized), holding one or more method targets.
- Events, callbacks, and strategy injection all rely on first-class function values.
- This differs from languages where function pointers exist but lack type-safe multicast and combined semantics.

---

#### Q5. What is a delegate in C#? How does it differ from a method group and from an interface with a single method?

**Answer:** A delegate is a type-safe reference type that points to one or more methods with a matching signature, invokable through `Invoke` or call syntax. A method group is the method name without parentheses before conversion to a delegate; an interface with one method defines a contract implementors satisfy, while a delegate is a callable indirection that can target any matching static or instance method.

- Delegates support multicast (`+=`, `-=`) — single-method interfaces do not chain invocations the same way.
- C# 3+ anonymous methods and lambdas convert to delegate instances inline.
- Single-method interfaces (e.g., `IComparer<T>`) fit OOP polymorphism and DI; delegates fit callbacks and LINQ.
- Delegate types are declared with the `delegate` keyword or use built-in `Func`/`Action`.

---

#### Q6. How do you declare, instantiate, and invoke a custom delegate type?

**Answer:** Declare with `public delegate ReturnType Name(ParamTypes);`, instantiate by assigning a method group or lambda matching the signature, and invoke with `instance(args)` or `instance.Invoke(args)`.

```csharp
public delegate int Transform(int x);
Transform t = x => x * 2;   // lambda instantiation
int result = t(5);            // invoke — returns 10
```

- Method group assignment: `Transform t = Math.Abs;` when signatures match exactly.
- Null delegate invocation throws `NullReferenceException` — use `t?.Invoke(5)` when optional.
- Custom delegate types clarify public API intent compared to generic `Func`/`Action`.

---

#### Q7. What is a multicast delegate? How does `+=` and `-=` work on delegate instances?

**Answer:** A multicast delegate holds an invocation list of multiple methods. `+=` appends a method to the list (or creates a new delegate combining both), and `-=` removes the last matching entry from the list. Invoking the delegate calls each subscriber in order.

- `audit += LogToConsole; audit += LogToFile;` — one invoke runs both methods.
- `-=` removes a specific method reference, not necessarily all matches if duplicated.
- `Delegate.Combine` and `Delegate.Remove` are the explicit API equivalents.
- Events build on multicast delegates with restricted invoke access from outside the declaring type.

---

#### Q8. What is the difference between single-cast and multicast delegates at invocation time?

**Answer:** A single-cast delegate invokes one target method. A multicast delegate invokes every method in its invocation list sequentially, discarding individual return values except the last (for non-void return types).

- Non-void multicast delegates return only the last handler's return value — earlier returns are lost.
- Void handlers are typical for event notification chains where return values do not matter.
- An empty invocation list after removing all handlers yields null — guard before invoke.
- `GetInvocationList()` returns separate single-cast delegates for per-handler try/catch if needed.

---

#### Q9. What happens when you invoke a multicast delegate and one subscriber throws an exception?

**Answer:** By default, invocation stops when a subscriber throws — later subscribers in the invocation list do not run. The exception propagates to the caller unless you iterate `GetInvocationList()` and invoke each handler inside its own try/catch.

- This short-circuit behavior surprises developers expecting all loggers to run even if one fails.
- Robust audit or logging chains often wrap each subscriber invocation separately.
- See Gotcha 3 in Module 04 — multicast short-circuit on exception.
- Event raisers in the BCL may document whether they catch subscriber exceptions — many do not.

---

#### Q10. What is delegate covariance and contravariance in C#?

**Answer:** Delegate variance allows assigning delegates when return types and parameter types relate by inheritance, safely. Return type covariance lets a delegate returning a derived type substitute for one returning a base type; parameter type contravariance lets a delegate accepting base parameters substitute for one accepting derived parameters.

- `Func<string>` can be assigned to `Func<object>` if return type is covariant (for reference types).
- `Action<object>` can be assigned to `Action<string>` — contravariant parameter position.
- Variance applies to reference types in delegate signatures under standard C# rules.
- Custom delegate declarations follow the same variance patterns as `Func` and `Action`.

---

#### Q11. When would you prefer a named delegate type over `Func`/`Action` in a public API?

**Answer:** A named delegate (or dedicated delegate declaration) documents domain meaning — `ShippingRule`, `OrderValidator` — instead of an anonymous `Func<decimal, bool>` whose parameters are unclear at the call site. Public libraries and long-lived APIs benefit from self-describing types.

- `Func`/`Action` suit private helpers and short-lived LINQ lambdas where context is obvious.
- Named delegates enable distinct types for identical signatures, avoiding accidental cross-wiring.
- XML documentation attaches more clearly to `delegate void PriceAlert(string msg)` than to generic `Action<string>`.
- Refactoring rename of a domain delegate propagates intent better than generic arity changes.

---

#### Q12. What are the advantages and limitations of adopting a functional style in typical enterprise C# codebases?

**Answer:** Advantages include clearer data transformations via LINQ, easier unit testing of pure helpers, reduced shared mutable state in parallel code, and expressive pipeline composition. Limitations include team familiarity, debugging deferred LINQ, closure capture pitfalls, and friction with frameworks built around mutable entities (EF change tracking, WinForms binding).

- Immutable records fit read models and DTOs; ORM entities often remain mutable by necessity.
- Over-nesting lambdas hurts readability — named methods still matter for complex logic.
- Enterprise adoption is usually pragmatic: functional where it pays (queries, validation rules), imperative elsewhere.
- Performance-sensitive hot paths may avoid delegate allocation and closure overhead.

---

### 02. Lambda Expressions

#### Q1. What is a lambda expression in C#? What problem does it solve compared to named methods?

**Answer:** A lambda expression is an inline function written with `=>` syntax that converts to a compatible delegate or expression tree. It removes the ceremony of declaring a separate named method for one-off callbacks, filters, and mappers passed directly at the call site.

- `list.Where(x => x > 0)` reads the predicate where it is used instead of scattering `IsPositive` methods.
- Lambdas are the default syntax for delegates in modern C# — shorter than anonymous methods.
- Small behavior snippets stay local, reducing namespace pollution from many single-use private methods.
- Complex multi-statement logic may still deserve a named method for testability and clarity.

---

#### Q2. What is the difference between an expression lambda and a statement lambda?

**Answer:** An expression lambda has a single expression on the right of `=>` that becomes the return value: `x => x * 2`. A statement lambda uses braces and can contain multiple statements, requiring explicit `return` for non-void delegates: `(x, y) => { var sum = x + y; return sum; }`.

- Expression lambdas cannot contain statements like `if` blocks unless rewritten as ternary expressions.
- Statement lambdas support local variables, loops, and multiple statements inside the block.
- Both forms compile to delegate instances (or expression trees when typed as such).
- `async` lambdas require statement form when using `await` with multiple statements.

---

#### Q3. When can parameter types be omitted in a lambda, and when must they be explicit?

**Answer:** Parameter types can be omitted when the compiler can infer them from the target delegate type — assignment, method argument, or cast context. Types must be explicit when inference fails: no target type, ambiguous overloads, or the compiler cannot infer individual parameter types in a tuple.

- `Func<int, bool> f = x => x > 0;` — `x` inferred as `int` from `Func<int, bool>`.
- `(int x, int y) => x + y` when passing to a method with multiple overloads may require types.
- Explicit types: `(string s) => s.Length` when target type alone does not disambiguate.
- Discards `_` can replace unused parameters: `(_, y) => y`.

---

#### Q4. What are target-typed lambdas (C# 10+)? In what contexts does the compiler infer the delegate type?

**Answer:** Target-typed lambdas let the compiler infer the delegate type from context without an explicit cast on the lambda itself — assignment to a typed variable, passing as a method argument, or returning from a method with a known return delegate type.

- `Func<int, int> square = x => x * x;` — natural target typing from variable type.
- Method parameters typed as `Action<string>` infer the lambda's parameter and return shape.
- Without a target type, a standalone `x => x * 2` may fail to compile — see Gotcha 7.
- C# 10+ improved inference for natural types in more scenarios than earlier versions.

---

#### Q5. What is the natural type of a lambda — when does the compiler infer `Func`/`Action` vs require an explicit target type?

**Answer:** In modern C#, some lambdas receive a compiler-inferred natural delegate type (often `Func` or `Action` variants) when no explicit target is required. When overload resolution or custom delegate types are involved, you still need an explicit target type or cast.

- Simple assignments may infer `Func<int, int>` for `var f = (int x) => x + 1;` in C# 10+ with natural types.
- Custom delegate types like `PriceFilter` require assignment context or cast — natural type is not the custom name.
- Expression trees require `Expression<Func<...>>` target — natural delegate inference does not apply to trees.
- Ambiguous calls need explicit types or casts to pick the correct overload.

---

#### Q6. How do lambda expressions differ from anonymous methods in syntax, capabilities, and compiler output?

**Answer:** Lambdas use concise `=>` syntax; anonymous methods use the `delegate` keyword with a block body. Both compile to similar display-class machinery when capturing variables; lambdas support expression-bodied form and integrate with expression trees more naturally in modern APIs.

- Anonymous methods can omit parameters entirely: `delegate { count++; }`.
- Lambdas cannot omit the parameter list — use `() =>` for zero parameters.
- Anonymous methods remain in legacy WinForms code; new code prefers lambdas.
- Compiler output for equivalent logic is largely similar — both generate delegate instances and closure classes when needed.

---

#### Q7. Can a lambda expression access `ref`, `out`, or `in` parameters from the enclosing method?

**Answer:** No — lambdas and anonymous methods cannot directly capture `ref`, `out`, or `in` parameters from the enclosing method. Those parameters represent special passing modes tied to call sites, not ordinary local variables the compiler can hoist into a display class.

- Workaround: copy to a local first, then capture the local if mutation of the copy suffices.
- To mutate caller state, capture a wrapper object or use a `ref struct`-free pattern outside the lambda.
- This restriction prevents undefined behavior if the lambda outlives the stack slot of a `ref` parameter.
- Regular value parameters and locals are capturable normally.

---

#### Q8. Can a lambda be converted to an expression tree? What syntax or API constraints apply?

**Answer:** Yes, when the lambda is assigned to `Expression<TDelegate>` or passed where that type is expected, the compiler builds an expression tree data structure instead of IL for immediate execution. Expression-tree lambdas cannot contain statements, assignments, or many C# constructs that lack tree node equivalents.

- `Expression<Func<int, bool>> expr = x => x > 0;` — tree for LINQ providers (EF Core, etc.).
- `Func<int, bool> func = x => x > 0;` — compiled delegate, not a tree.
- Loops, `try/catch`, and local functions inside the lambda typically disqualify expression tree conversion.
- See Gotcha 8 — expression tree vs delegate restrictions.

---

#### Q9. What is the difference between a lambda that captures no locals vs one that captures outer variables?

**Answer:** A lambda with no captures may compile to a static cached delegate instance — no hidden closure object. A lambda that references outer locals or `this` generates a display class holding captured fields, allocating heap storage that outlives the enclosing method if the delegate survives.

- `Func<int, int> f = x => x * 2;` — often no capture, no display class.
- `int offset = 10; Func<int, int> g = x => x + offset;` — captures `offset` in a compiler-generated class.
- Captured variables are shared by reference — mutations visible to all delegates sharing the slot.
- See Module 04 Closures chapter for lifetime and loop pitfalls.

---

#### Q10. How do async lambdas work (`async x => ...`)? What delegate types can they target?

**Answer:** An async lambda is a lambda marked `async` whose body can `await` — it converts to an async void, async `Action`, or async `Func` returning `Task`/`Task<T>` matching the target delegate. The compiler generates a state machine similar to async methods.

- `Func<Task<int>> work = async () => await FetchCountAsync();`
- `async void` lambdas are dangerous except for event handlers — unobserved exceptions can crash the process.
- Async lambdas cannot target `Expression<TDelegate>` — expression trees do not support async.
- Return type must match: `Func<Task>` vs `Func<Task<T>>` vs async void for `Action`-like targets.

---

#### Q11. What happens if you use a lambda where a `Expression<TDelegate>` is expected vs where a `TDelegate` is expected?

**Answer:** With `Expression<TDelegate>`, the compiler emits an expression tree describing the logic for translation (e.g., to SQL). With `TDelegate` (such as `Func<...>`), the compiler emits a real delegate for immediate invocation — same syntax, different compile-time target type.

- EF Core `Where(e => e.Price > 10)` requires `Expression<Func<...>>` on `IQueryable` to translate predicates to SQL.
- `IEnumerable.Where` takes `Func<...>` — the predicate runs in memory as IL.
- Mixing them up causes compile errors or runtime behavior differences (client evaluation vs server translation).
- Not every lambda body is valid for expression trees even when the target type is `Expression<...>`.

---

### 03. Anonymous Methods

#### Q1. What are anonymous methods in C#? Why were they introduced, and what largely replaced them?

**Answer:** Anonymous methods are inline delegate bodies written with the `delegate` keyword and a statement block, introduced in C# 2.0 so developers could pass behavior without declaring a separate named method. Lambda expressions (C# 3.0+) largely replaced them for new code because lambdas are shorter and support expression-bodied form.

- `OrderRule rule = delegate (Order o) { return o.Total > 0; };` — classic anonymous method shape.
- Legacy WinForms, WPF, and older libraries still contain `delegate { }` event handlers.
- Semantics match lambdas for capture and invocation — only syntax differs.
- Modern style guide: use lambdas unless maintaining existing anonymous-method code.

---

#### Q2. What is the syntax for an anonymous method, and how does it compare to lambda syntax?

**Answer:** Anonymous methods use `delegate (ParamTypes) { statements }` assigned to a compatible delegate variable. Lambdas replace `delegate (params) { return expr; }` with `params => expr` or `params => { statements }`.

- Full anonymous: `Func<int, int> f = delegate (int x) { return x * 2; };`
- Lambda equivalent: `Func<int, int> f = x => x * 2;`
- Both require a target delegate type — neither stands alone without assignment context.
- Anonymous methods always use block syntax; lambdas can use single-expression form without braces.

---

#### Q3. Can anonymous methods omit parameter lists? When is that useful?

**Answer:** Yes — an anonymous method can omit parameters when the delegate signature allows zero parameters or when the body does not need arguments: `Action a = delegate { count++; };`. This matches `Action` or other parameterless delegate types.

- Useful for simple event handlers that only touch instance state or outer captured locals.
- Cannot omit parameters when the delegate requires them — signature must still match at invoke time.
- Lambdas require explicit `()` for zero parameters: `Action a = () => count++;`
- Omitting parameters does not mean arbitrary arity — the delegate type fixes the contract.

---

#### Q4. What outer scope variables can anonymous methods access, and how does capture work?

**Answer:** Anonymous methods can access locals, parameters (except `ref`/`out`/`in`), and instance members from enclosing scopes — the compiler generates a display class with fields for captured variables, identical to lambda capture. Captured storage is shared by reference, so later mutations are visible when the delegate runs.

- Outer local `int threshold` captured in `delegate { return price <= threshold; }` reads live value.
- Multiple anonymous methods or lambdas sharing one local share one compiler-generated field.
- Capture extends lifetime — locals may live on the heap as long as the delegate does.
- See Module 04 Closures for loop-variable pitfalls and copy fixes.

---

#### Q5. In modern C# code, when (if ever) would you still choose an anonymous method over a lambda?

**Answer:** Rarely — lambdas are preferred for readability and expression-tree support. You might keep anonymous methods when minimally editing legacy code that already uses `delegate { }`, or when matching surrounding style in a file that has not been modernized.

- No technical advantage remains for new features — lambdas cover all anonymous-method scenarios.
- Refactoring legacy handlers to lambdas is low risk when signatures are unchanged.
- Statement lambdas replace multi-statement anonymous methods line for line.
- Greenfield code should use lambdas exclusively unless team standards say otherwise.

---

### 04. Extension Methods

#### Q1. What are extension methods in C#? How do they appear to the caller vs how they are implemented?

**Answer:** Extension methods are static methods declared in a static class with the first parameter marked `this`, so callers invoke them with instance syntax as if they were instance methods. The compiler rewrites `receiver.Method(args)` to `StaticClass.Method(receiver, args)` — no modification to the extended type occurs.

- LINQ's `Where`, `Select`, and `OrderBy` are extension methods on `IEnumerable<T>`.
- IntelliSense lists extensions when the defining namespace is imported with `using`.
- The extended type can be sealed, an interface, or a struct — extension adds API without inheritance.
- At IL level they remain static calls — no virtual dispatch on the extended type.

---

#### Q2. What are the language rules for declaring an extension method (static class, `this` parameter, accessibility)?

**Answer:** The method must live in a non-nested static class, the method itself must be static, and only the first parameter carries the `this` modifier naming the extended type. Accessibility follows normal rules — `public` extensions in an imported namespace are visible to callers.

- `public static class StringHelpers { public static int WordCount(this string s) { ... } }`
- Nested static classes cannot host extensions — CS1106.
- Additional parameters after the `this` parameter are ordinary method parameters.
- Generic static classes can contain generic extension methods with their own type parameters.

---

#### Q3. How does the compiler resolve an extension method call at compile time?

**Answer:** When instance method lookup fails, the compiler searches extension methods in scope from all `using` namespaces, matching the receiver type and argument list. It binds to the best overload by standard overload resolution rules applied to the static call form.

- Missing `using` for the static class namespace causes CS1061 — extension not found.
- See Gotcha 4 — extension not in scope is a frequent discovery issue.
- Extension resolution happens at compile time only — no runtime plugin discovery.
- Nullable reference types participate in resolution like other parameters.

---

#### Q4. What is the difference in resolution order between an instance method and an extension method with the same signature?

**Answer:** Instance methods always win — if the receiver type defines an instance method with a matching signature, the compiler binds to it and never considers the extension. Extensions apply only when no instance method matches.

- You cannot override an existing instance method with an extension — see Gotcha 5.
- Extensions add methods to types you do not own; they do not replace BCL behavior.
- Explicit static call `Ext.Method(obj)` can still invoke the extension even when an instance method exists with a different signature.
- Shadowing by accidental name collision on your own types is a design smell — rename the extension.

---

#### Q5. Can extension methods access `private` members of the extended type? Why or why not?

**Answer:** No — extension methods are static methods in a separate class and only access public (and sometimes `internal` in the same assembly via `InternalsVisibleTo`) members of the extended type. They are syntactic sugar, not privileged friends of the type.

- True encapsulation requires instance methods, nested types, or `private` members accessed from within the type itself.
- "God extensions" that need private state often indicate a missing instance method or poor type design.
- Same-assembly `internal` access works like any static helper in that assembly.
- Partial classes can add instance methods with private access — extensions cannot substitute.

---

#### Q6. What are the limitations of extension methods?

**Answer:** Extensions cannot add state, cannot override instance methods, cannot access private members, and are not polymorphic — virtual dispatch applies to instance methods on the runtime type, not to statically bound extensions. They also do not appear on the type's metadata as instance members.

- Refactoring tools may miss extension usages when moving types between namespaces.
- Extensions on interfaces can create discoverability clutter if overused.
- Cannot define fields, events backed by new storage, or properties with new backing fields through extensions.
- Resolution requires compile-time `using` — changing namespaces breaks callers.

---

#### Q7. How do extension methods work on interfaces? What are design implications (e.g., LINQ)?

**Answer:** Static extension methods can target interfaces, adding methods to every implementor without modifying implementor source — `IEnumerable<T>` gains LINQ operators this way. Callers see a rich API on interface-typed variables as long as the extension namespace is imported.

- Enables cross-cutting operations on all sequences without a common base class.
- Risk: interface plus many extensions scatters behavior — hard to find where logic lives.
- Implementors do not need to implement LINQ methods — extensions supply them centrally.
- Nullable reference typing flows through the `this` parameter like any static generic method.

---

#### Q8. What happens when two namespaces define extensions with the same name and signature for the same type?

**Answer:** Importing both namespaces causes ambiguous extension method errors at compile time unless you cast the receiver, call the static form explicitly (`NamespaceA.Helper.Method(obj)`), or remove one `using`. Overload resolution cannot pick between equally specific conflicting extensions.

- Organize extensions in one canonical namespace per type when possible.
- Explicit static invocation disambiguates without changing runtime behavior.
- Instance methods avoid this problem by living on the type itself.
- Generic extensions add complexity — both must match and conflict triggers CS0121-style ambiguity.

---

#### Q9. Can you define generic extension methods? How does type inference work at the call site?

**Answer:** Yes — extension methods can be generic with their own type parameters, inferred from argument types at the call site like ordinary generic static methods: `public static T FirstOrDefault<T>(this IEnumerable<T> source, Func<T, bool> predicate)`.

- `list.FirstOrDefault(x => x.Id == 5)` infers `T` from `list`'s element type.
- Constraints on `T` apply normally — `where T : class`, `new()`, etc.
- Generic extensions enable reusable utilities on `IEnumerable<T>`, `IReadOnlyList<T>`, and custom types.
- Explicit type arguments are available when inference fails: `items.Convert<int, string>()`.

---

#### Q10. What are anti-patterns with extension methods (god extensions, violating encapsulation)?

**Answer:** Anti-patterns include dumping dozens of unrelated helpers on `string` or `object`, reimplementing business rules that belong on domain types, and using extensions to reach toward private invariants through public surface hacks.

- Prefer instance methods on types you control; extensions suit BCL and interface types you cannot modify.
- Split extension classes by domain (`OrderExtensions`, `StringFormattingExtensions`) rather than one mega-class.
- Extensions that mutate hidden global state violate functional expectations of static helpers.
- Unit tests should target the static class directly when behavior is non-trivial.

---

### 05. Func, Action & Predicate

#### Q1. What are `Func<T>`, `Func<T, TResult>`, and the general `Func<...>` family?

**Answer:** `Func` delegates are generic templates in `System` for functions that return a value. `Func<TResult>` takes no parameters and returns `TResult`; `Func<T, TResult>` takes one `T` and returns `TResult`; the family extends up to 16 input type parameters with the last type parameter always being the return type.

- `Func<DateTime> now = () => DateTime.UtcNow;` — zero inputs.
- `Func<string, int> len = s => s.Length;` — one input, `int` result.
- Method group assignment works when signatures align: `Func<int, int> abs = Math.Abs;`
- `Func` replaces many custom `delegate` declarations for computational callbacks.

---

#### Q2. What is `Action` vs `Action<T>` vs `Action<T1, T2, ...>`?

**Answer:** `Action` delegates represent void-returning callbacks. Plain `Action` takes no parameters; `Action<T>` takes one parameter; multi-parameter forms mirror `Func` arity up to 16 parameters, all returning void.

- `Action log = () => Console.WriteLine("done");`
- `Action<string> print = msg => Console.WriteLine(msg);`
- Use `Action` when side effects matter and no return value is needed — logging, notifications, UI updates.
- `Func<T>` with return type `void` does not exist — `Action` fills that role.

---

#### Q3. What is `Predicate<T>`, and how does it relate to `Func<T, bool>`?

**Answer:** `Predicate<T>` is a legacy built-in delegate that takes one argument and returns `bool`, semantically identical to `Func<T, bool>`. Modern APIs and LINQ prefer `Func<T, bool>`, but older BCL methods like `List<T>.FindAll` still accept `Predicate<T>`.

- `Predicate<int> isEven = n => n % 2 == 0;` — same shape as `Func<int, bool>`.
- Lambdas convert to either type when the target parameter expects it.
- Choose `Func<T, bool>` in new public APIs for consistency with LINQ.
- `Array.TrueForAll` and similar methods may still name `Predicate<T>` in signatures.

---

#### Q4. When should you use `Func` vs `Action` vs `Predicate` vs a custom delegate?

**Answer:** Use `Func` when the callback returns a value, `Action` for void side effects, and `Func<T, bool>` (or `Predicate<T>` for legacy BCL) for filters. Use a custom named delegate when the signature carries domain meaning important to public API consumers.

- Private helpers and LINQ chains: generic `Func`/`Action` reduce boilerplate.
- Public plugin points like `ShippingRule`: custom delegate or dedicated type documents intent.
- `Predicate<T>` only when calling APIs that require it — otherwise prefer `Func<T, bool>`.
- Identical signatures for different concepts should not share one `Func` typedef — use distinct delegate names.

---

#### Q5. What are higher-order functions? Give C# examples using `Func` and `Action`.

**Answer:** Higher-order functions take other functions as parameters or return functions as results. C# expresses them through delegate-typed parameters and return types.

- `ApplyToAll(decimal[] prices, Func<decimal, decimal> transform)` — passes a transform function.
- `Factory` methods returning `Func<int, int>` create configured behaviors at runtime.
- LINQ `Select` and `Where` are higher-order — they accept `Func` delegates.
- Higher-order style enables strategy injection without subclass explosion.

---

#### Q6. What is function composition, and how can it be achieved in C#?

**Answer:** Function composition chains functions so the output of one becomes the input of the next: `(f ∘ g)(x) = f(g(x))`. C# has no built-in compose operator, but you implement it with a small helper or nested calls.

```csharp
Func<int, int> h = x => f(g(x));
// or: Func<Func<T,R>, Func<T,T>, Func<T,R>> Compose = (f, g) => x => f(g(x));
```

- LINQ pipelines compose declaratively: `source.Select(g).Select(f)` equivalent to mapping `f(g(x))`.
- Method chaining on fluent APIs achieves similar sequencing for object transformations.
- Pure functional libraries may supply `Compose`/`Pipe` extension methods.
- Composition preserves deferred execution when built on `IEnumerable` operators.

---

#### Q7. How many generic parameters do `Func` and `Action` support, and which parameter is always the return type for `Func`?

**Answer:** Both families support up to 16 type parameters for inputs in their longest overloads. For `Func`, the last type parameter is always `TResult` — the return type; all preceding parameters are input types. `Action` overloads have no return type parameter — all type parameters are inputs.

- `Func<T1, T2, T3, T4, TResult>` — three inputs, `TResult` return.
- `Action<T1, T2, T3, T4>` — four inputs, void return.
- Arity counting includes only type parameters, not the implicit void of `Action`.
- Very long arities exist for completeness; most code uses zero to three parameters.

---

#### Q8. How are `Func` and `Action` used in LINQ method parameters (`Select`, `Where`, etc.)?

**Answer:** LINQ extension methods on `IEnumerable<T>` take `Func` delegates: `Where` accepts `Func<T, bool>`, `Select` accepts `Func<T, TResult>`, `Aggregate` accepts funcs with varying arity. The caller supplies lambdas or method groups matching those shapes.

- `orders.Where(o => o.Total > 100)` — `Func<Order, bool>` predicate.
- `orders.Select(o => o.Customer)` — `Func<Order, string>` projector.
- `IQueryable` overloads use `Expression<Func<...>>` for translation — same logical roles, different compile target.
- Deferred execution means funcs run during enumeration, not when `Where`/`Select` are called.

---

#### Q9. When does using `Func<T, bool>` instead of `Predicate<T>` improve or hurt API clarity?

**Answer:** `Func<T, bool>` improves clarity and consistency in new code because it aligns with LINQ and modern BCL additions; `Predicate<T>` hurts cross-API uniformity when mixed arbitrarily but remains required when calling legacy methods that name `Predicate<T>` explicitly.

- Public new APIs should standardize on `Func<T, bool>` unless wrapping `List.FindAll` signatures.
- Converting between them is trivial at call sites — lambdas bind to both.
- XML docs read clearer when filter parameters use the same `Func` pattern as `Enumerable.Where`.
- Keeping `Predicate<T>` in a wrapper method isolates legacy types from new code.

---

### 06. Closures

#### Q1. What is a closure in C#?

**Answer:** A closure is the combination of a delegate and the captured environment of outer variables that the delegate can read or mutate when it runs, even after the enclosing method returns. The compiler generates a display class to hold captured fields so the delegate outlives the stack frame that created it.

- `int n = 10; Func<int> f = () => n;` — `f` closes over `n`.
- Closures enable factories, counters, and deferred LINQ predicates that reference local configuration.
- Without capture, delegates could not reference locals — only static methods and instance members.
- Closure semantics apply equally to lambdas and anonymous methods.

---

#### Q2. How does the compiler implement variable capture for lambdas and anonymous methods?

**Answer:** The compiler rewrites the enclosing method to instantiate a generated nested class (display class) with fields for each captured variable, assigns captured locals to those fields, and stores the delegate with a reference to the display instance. Invoking the delegate reads or writes the display fields.

- Multiple lambdas in one method sharing a local share one field on the same display instance.
- Capturing `this` in an instance method stores the instance reference in the display class.
- IL shows nested types like `<>c__DisplayClass0_0` in disassembly.
- No capture means the compiler may emit a static cached delegate without a display class.

---

#### Q3. What is the difference between capturing a variable vs capturing a value at closure creation time?

**Answer:** C# always captures the variable's storage slot, not a snapshot of its value at delegate creation time — when the outer local changes before invocation, the delegate observes the updated value. To freeze a value, copy it to a new local inside the loop or block and capture that copy instead.

- `int x = 1; Func<int> f = () => x; x = 2; f()` returns `2`, not `1`.
- Loop fix: `int copy = i; callbacks.Add(() => Use(copy));` — each iteration gets its own `copy` field.
- Confusing "capture by reference" with C# `ref` parameters is a common mistake — capture hoists storage to the heap.
- Value types captured are boxed or stored in display fields — mutations to the captured field affect all sharers.

---

#### Q4. What is the classic `for` loop closure bug, and how did C# 5 change loop variable capture semantics?

**Answer:** Before C# 5, all iterations of a `for` loop shared one loop variable, so lambdas deferred to later saw the final value of that variable — `for (int i = 0; i < 3; i++) list.Add(() => i)` produced three delegates all returning `3`. C# 5 changed `foreach` loop variable semantics to a fresh inner variable per iteration; `for` loops still share one `i` unless you copy per iteration.

- Classic bug output when invoked later: `3, 3, 3` instead of `0, 1, 2`.
- `foreach` after C# 5 is safe for per-iteration capture; `for` is not without a copy.
- Fix for `for`: `int copy = i;` inside the loop body before creating the lambda.
- See Gotcha 1 and Gotcha 2 in Module 04.

---

#### Q5. How does the same capture bug appear in `foreach`, LINQ, and `Task.Run` callbacks?

**Answer:** Any deferred execution that closes over a loop variable without a per-iteration copy can observe the wrong value when it finally runs. LINQ deferred queries capturing loop variables, and `Task.Run(() => Use(i))` inside a loop, exhibit the same shared-slot problem as raw delegate lists when the variable is shared.

- `tasks.Add(Task.Run(() => Process(item)))` inside `foreach` over mutable `item` without copy can race or see the last item.
- LINQ inside loops building expression trees rarely hits this; building delegate lists in loops does.
- `foreach` in modern C# creates a per-iteration variable — safer than `for` for capture.
- Always copy to a local inside the loop body when passing to async or deferred callbacks.

---

#### Q6. What problems arise when multiple closures share the same captured variable?

**Answer:** All delegates sharing one captured slot see the same live value — mutating it through one delegate affects what others read on next invocation. Unintended shared mutation causes race conditions in parallel code and surprising ordering in sequential event handlers.

- Two lambdas incrementing the same captured counter share one integer field — both see cumulative changes.
- Parallel `Task.Run` callbacks mutating one captured list without synchronization corrupt data.
- Separate intended counters require separate locals or separate display fields per closure.
- Immutable captured values (copied per iteration) eliminate cross-delegate interference.

---

#### Q7. What is a display class (compiler-generated closure type), and what performance cost does capture introduce?

**Answer:** A display class is the compiler-generated nested type holding fields for captured variables and referenced by the delegate. Capture introduces heap allocation for the display instance and possibly the delegate, plus indirection on each read of captured state — hot paths with no capture may use cached static delegates instead.

- Zero-capture lambdas are often optimized to static singleton delegates — no per-call allocation.
- Capturing one local allocates at least one display object when the enclosing method runs.
- Capturing `this` keeps the entire instance alive as long as any capturing delegate lives.
- Micro-optimization: avoid unnecessary captures in tight loops; use local functions or static lambdas when possible.

---

#### Q8. What is a pure function? Give an example in C# and explain what makes it pure.

**Answer:** A pure function returns a value that depends only on its inputs, produces no observable side effects (no I/O, no mutation of external state), and is referentially transparent — calling it twice with the same arguments yields equal results.

```csharp
static decimal ApplyDiscount(decimal price, decimal rate) => price * (1 - rate);
```

- `ApplyDiscount` does not read globals, write files, or mutate parameters.
- `Random.Next()` and `DateTime.Now` make functions impure because outputs vary without input changes.
- Pure helpers are trivially unit-testable and safe to memoize or parallelize.
- C# does not enforce purity — it is a design choice.

---

#### Q9. What is immutability, and why is it important in functional and concurrent programming?

**Answer:** Immutability means data cannot change after construction — mutations create new values instead of altering existing objects. Immutable data eliminates whole classes of bugs when multiple threads or deferred callbacks read shared state, because readers never observe partial updates.

- Records with `init` properties and `with` expressions support immutable updates in modern C#.
- Mutable shared state plus closures plus `Task.Run` is a frequent source of race conditions.
- Immutable models simplify hashing, caching, and change detection — hash codes stay stable.
- Enterprise code mixes mutable entities (ORM) with immutable DTOs for boundaries.

---

#### Q10. How can immutability be achieved in C# (`readonly`, `record`, avoiding mutable captures)?

**Answer:** Use `readonly` fields set only in constructors, `init`-only properties on records, defensive copies when exposing collections, and capture immutable snapshots (copied locals) in closures instead of mutable shared slots.

- `public record Order(int Id, string Customer);` — positional record immutability by default.
- `readonly` prevents reassignment of field references — it does not deep-freeze mutable objects pointed to.
- `IReadOnlyList<T>` return types signal callers should not mutate the underlying list through that API.
- Closure fix `var copy = item` captures an immutable per-iteration binding when `item` is reassigned each loop.

---

#### Q11. How do you avoid side effects when passing lambdas to APIs that store or invoke them later?

**Answer:** Prefer pure lambdas that depend only on parameters and immutable captured data; copy values needed at registration time; avoid capturing mutable singletons or shared collections unless synchronized; document lifetime expectations when handlers run asynchronously.

- Event subscription that captures `this` and mutates fields runs later on an unknown thread — guard shared state.
- Pass all needed data as delegate parameters instead of relying on mutable outer locals when possible.
- Snapshot configuration: `var cfg = settings; btn.Click += (_, _) => Apply(cfg);` — `cfg` reference immutable if settings object replaced atomically.
- Test stored delegates by invoking after the registering method returns to flush capture bugs.

---

#### Q12. When should you copy loop values to a local inside the loop before capturing (`var copy = item`)?

**Answer:** Copy whenever you create a delegate, task, or closure inside a `for` loop, or when `foreach` iterates reference types whose identity you need per iteration, and the delegate may run after the loop advances. The copy creates a fresh local slot the compiler hoists to its own display field per iteration.

- Required pattern for `for (int i = 0; ...)` adding lambdas to a list.
- Safer for `Task.Run(() => Process(order))` when `order` is reassigned each iteration.
- Modern `foreach` over value types often already per-iteration — reference types in mutable loops still need care.
- Helper method `void Enqueue(int value) => queue.Add(() => Console.WriteLine(value));` freezes via parameter pass-by-value.

---

#### Q13. How do local functions compare to lambdas regarding capture and allocation behavior?

**Answer:** Local functions can capture outer variables like lambdas, but static local functions (`static` modifier) cannot capture at all — they behave like static methods nested lexically. Local functions may avoid delegate allocation when called directly without converting to a delegate, and C# 9+ supports `static local function` for hot paths.

- `void Outer() { int x = 1; int Add() => x + 1; }` — local function captures `x` like a lambda.
- `static int Pure(int a) => a + 1;` inside `Outer` — no capture, no display class for that function.
- Converting a local function to a delegate (`Func<int> f = Add;`) still allocates a delegate and may capture.
- Prefer local functions over private methods when logic is used once and needs enclosing scope access.

---

### Gotchas — Module 04

#### Gotcha 1. Closure captures the variable, not the value

**Answer:** Delegates close over the storage location of outer locals, so they observe current value at invocation time, not value at delegate creation. Loop lambdas that print `i` after a `for` loop often print the final `i` three times.

- Fix: `int copy = i;` inside each iteration before creating the lambda.
- Same logic applies to any deferred execution — event handlers, task callbacks, queued work items.
- Do not assume closing over `i` freezes `i` — only a separate local per iteration freezes.

---

#### Gotcha 2. Same trap in LINQ and tasks

**Answer:** Building a list of `Task.Run(() => Work(i))` or delegates inside a loop without per-iteration copies reproduces the shared-variable bug — all tasks may see the final loop value or race on one mutable object.

- `foreach` mitigates for iteration variables but not all reference-type scenarios.
- Materializing LINQ inside loops still captures loop variables in the resulting delegates.
- Copy pattern and helper parameters are universal fixes across LINQ, tasks, and raw delegates.

---

#### Gotcha 3. Multicast delegate short-circuit on exception

**Answer:** When one subscriber in a multicast delegate throws, later subscribers are skipped unless you invoke each handler individually from `GetInvocationList()`.

- Logging pipelines should wrap per-handler try/catch so one failing sink does not silence others.
- Event raisers rarely catch subscriber exceptions — handlers should contain their own fault boundaries.
- Tests should cover failure in the middle of a multicast chain.

---

#### Gotcha 4. Extension method not in scope

**Answer:** Extension methods require a `using` directive for the namespace containing the static extension class. Without it, the compiler reports that no method with that name exists on the type (CS1061).

- LINQ requires `using System.Linq;` for `Where`, `Select`, etc.
- Fully qualify static calls when avoiding namespace imports: `System.Linq.Enumerable.Where(source, predicate)`.
- IDE quick fixes often add the missing `using` automatically.

---

#### Gotcha 5. Instance method wins over extension

**Answer:** If the instance type defines a method with the same signature as an extension, the instance method is chosen and the extension is ignored for that call.

- Extensions cannot override or hide existing instance methods.
- Adding an instance method later can silently change which method binds — breaking change for code that relied on the extension.
- Explicit static extension call still works if signatures differ slightly.

---

#### Gotcha 6. Shared captured storage

**Answer:** Multiple lambdas in the same scope that capture the same local variable share one compiler-generated field, so mutations through one lambda affect others reading that variable.

- Separate counters need separate locals or separate scopes.
- Parallel invocation of such lambdas requires synchronization if they mutate the capture.
- Functional style prefers immutable captures to avoid shared mutable slots.

---

#### Gotcha 7. Target-typed lambda ambiguity

**Answer:** A lambda without a clear target type (assignment, cast, or method parameter type) may fail overload resolution or natural type inference, producing compile errors about ambiguous or missing types.

- Assign to an explicitly typed variable: `Func<int, bool> p = x => x > 0;`
- Cast the lambda: `(Func<int, bool>)(x => x > 0)`
- Pass directly to a method parameter with a fixed delegate type — strongest inference context.

---

#### Gotcha 8. Expression tree vs delegate

**Answer:** Lambdas assigned to `Expression<TDelegate>` become expression trees with restricted syntax — no control flow statements, assignments, or many C# features available to normal delegate lambdas.

- EF Core queries fail translation when expression trees contain unsupported constructs.
- Switch to `IEnumerable` overloads with `Func` for client-side logic when trees cannot express the predicate.
- Same lambda syntax compiles differently based solely on target type — easy to miss in code review.

---

#### Gotcha 9. Capturing `this` implicitly

**Answer:** Instance lambdas and instance local functions capture `this`, keeping the entire object alive until the delegate is garbage-collected — even if the delegate only needed one field.

- Long-lived events plus capturing handlers prevent collection of the subscriber.
- Unsubscribe events when done to release captures, or use weak-event patterns for long-lived publishers.
- Static lambdas inside instance methods do not capture `this` unless they reference instance members.

---

#### Gotcha 10. Extension on null reference

**Answer:** Extension methods can be called on null receivers because they compile to static calls — the null is simply passed as the first argument. The extension method body may throw `NullReferenceException` when it dereferences the receiver.

- Instance methods on null throw immediately on dispatch; extensions enter the static method first.
- Null-check the `this` parameter at the start of extensions that must tolerate null: `if (s is null) return ...;`
- Nullable reference type annotations on the `this` parameter document null behavior.

---

---

## Module 05. Language Integrated Query

### 01. Introduction to LINQ

#### Q1. What is LINQ, and what problem does it unify across in-memory collections, databases, XML, and more?

**Answer:** Language Integrated Query (LINQ) is C#'s declarative query model for filtering, transforming, ordering, and summarizing data using a composable pipeline of operators instead of hand-written loops. It unifies the same mental model across LINQ to Objects (in-memory `IEnumerable<T>`), LINQ to Entities (Entity Framework Core translating to SQL), LINQ to XML (`XElement` trees), and other providers so you learn one query style and reuse it on different data sources.

- Before LINQ, each source required different APIs — `foreach` for lists, SQL strings for databases, XPath or DOM walks for XML — which duplicated logic and made cross-cutting rules hard to read.
- LINQ expresses operations as method chains (`Where`, `Select`, `OrderBy`) or query syntax (`from` / `where` / `select`) that compile to the same underlying calls.
- Providers plug in at the sequence type: `Enumerable` extension methods run in process; `Queryable` builds expression trees for remote translation.
- The pattern is intentionally composable — each operator returns a new sequence description you can chain until a terminal operator materializes results.

---

#### Q2. What is the difference between query syntax and method syntax? Are they equivalent?

**Answer:** Query syntax is C# sugar that looks like SQL (`from x in source where ... select ...`), while method syntax chains extension methods directly (`source.Where(...).Select(...)`). They are equivalent because the compiler rewrites query syntax into method calls, so there is no runtime difference — only readability preference.

- Query syntax shines for joins, grouping, and multi-clause queries where the keyword layout reads top-to-bottom like a report definition.
- Method syntax is often clearer for short pipelines and is required when no query-syntax keyword exists for an operator (for example `Take`, `Skip`, or `Distinct`).
- Both require `using System.Linq;` and both produce deferred sequences until you enumerate or call a terminal operator.
- You can mix styles in one solution, but teams usually pick one primary style for consistency within a file or project.

---

#### Q3. What is deferred execution in LINQ, and which operators break it?

**Answer:** Deferred execution means a LINQ query stores a recipe (the operator chain and source reference) and does not run until something consumes the sequence. Operators that force immediate execution — called terminal or immediate operators — break deferral by walking the sequence and producing a concrete result or side effect.

- Deferred operators include `Where`, `Select`, `OrderBy`, `GroupBy`, and `Join`; they return new `IEnumerable<T>` views without touching every element yet.
- Immediate operators include `ToList`, `ToArray`, `Count`, `Sum`, `Average`, `Min`, `Max`, `First`, `Single`, `Any`, `All`, and `foreach` enumeration — each triggers at least one full pass (or partial pass for short-circuiting operators).
- Because the query is lazy, changing the source collection after building the query but before enumeration can change results — the pipeline reads current data at execution time.
- Calling `ToList()` or `ToArray()` snapshots data at that moment and stops subsequent source mutations from affecting the materialized copy.

---

#### Q4. What is the difference between deferred execution and lazy evaluation?

**Answer:** Deferred execution in LINQ means the query is not run until enumeration; lazy evaluation is the broader computer-science idea that a value or computation is produced only when needed. In practice, LINQ to Objects uses deferred execution as its form of lazy evaluation — operators compute the next element on demand as the consumer pulls from the iterator.

- Deferred execution applies to the whole query chain: nothing runs until a consumer asks for elements or a terminal operator runs.
- Lazy evaluation can also describe individual elements — for example `yield return` in a custom iterator produces one item at a time rather than building a full list upfront.
- Eager evaluation is the opposite: `List<T>` constructors and immediate LINQ operators compute everything before returning.
- Both terms appear in interviews; "deferred execution" is the LINQ-specific phrase, while "lazy evaluation" explains why streaming large sequences can use bounded memory.

---

#### Q5. What is the difference between `IEnumerable<T>` and `IQueryable<T>`?

**Answer:** `IEnumerable<T>` represents an in-memory sequence you enumerate in the current process, while `IQueryable<T>` extends that contract with expression trees so a provider (such as EF Core) can translate the query into remote execution like SQL. Both support LINQ syntax, but `IQueryable<T>` is designed for out-of-process data sources where translation matters.

| | `IEnumerable<T>` | `IQueryable<T>` |
|---|---|---|
| Execution | Typically in-process (LINQ to Objects) | Provider-dependent (often remote SQL) |
| Query representation | Delegates (`Func`, `Expression` as compiled code) | Expression trees the provider inspects |
| Extension class | `System.Linq.Enumerable` | `System.Linq.Queryable` |
| Composability | Full LINQ to Objects surface | Limited to translatable expressions |

Use `IEnumerable<T>` for collections already in memory; use `IQueryable<T>` when EF Core or another provider should push filtering and projection to the database.

---

#### Q6. What is an expression tree, and why does `IQueryable` depend on it?

**Answer:** An expression tree is a data structure that represents code as a tree of objects (method calls, property accesses, constants) rather than compiled IL delegates. `IQueryable<T>` depends on expression trees because the query provider must inspect the tree, translate it to SQL or another remote language, and execute it on the server instead of running arbitrary C# lambdas in memory.

- A lambda passed to `Enumerable.Where` becomes a compiled `Func<T, bool>` that runs locally for each element.
- The same-looking lambda passed to `Queryable.Where` becomes an `Expression<Func<T, bool>>` that EF Core walks to build a `WHERE` clause.
- If the provider cannot translate a node in the tree (for example a custom instance method), translation fails or EF Core may throw in strict mode or pull data client-side in older configurations.
- Expression trees are the bridge that makes LINQ feel uniform while still allowing provider-specific optimization.

---

#### Q7. What is the difference between LINQ to Objects and LINQ to Entities (EF Core)?

**Answer:** LINQ to Objects runs LINQ operators against `IEnumerable<T>` sequences in your application's memory using compiled delegates, while LINQ to Entities runs against `IQueryable<T>` through EF Core, which translates supported operators into SQL executed by the database. The syntax looks similar, but execution location, performance, and supported operations differ sharply.

- LINQ to Objects can call any C# method in predicates; EF Core only translates expressions its provider understands.
- Database-side execution reduces network traffic and memory because filtering and projection happen before rows reach the app.
- Client evaluation — pulling rows then filtering in memory — can accidentally load entire tables if custom code appears too early in the pipeline.
- Always assume LINQ to Entities queries may run twice if you enumerate an `IQueryable` multiple times unless you materialize with `ToList()` deliberately.

---

#### Q8. What is the role of the `Enumerable` vs `Queryable` static classes?

**Answer:** Both classes hold the static extension methods that make LINQ work, but `System.Linq.Enumerable` targets `IEnumerable<T>` with delegate-based operators for in-memory execution, while `System.Linq.Queryable` targets `IQueryable<T>` with overloads that accept expression trees for provider translation.

- When you write `list.Where(x => x.Age > 18)`, the compiler picks `Enumerable.Where` because `List<T>` implements `IEnumerable<T>`.
- When you write `dbContext.Customers.Where(c => c.IsActive)`, the compiler picks `Queryable.Where` because `DbSet<T>` implements `IQueryable<T>`.
- Many operator names overlap (`Select`, `Where`, `OrderBy`), but the parameter types (`Func` vs `Expression`) determine which implementation binds.
- Provider authors implement `IQueryProvider.Execute` to interpret the expression tree built by `Queryable` methods.

---

#### Q9. What does it mean for a LINQ provider to translate a query? What happens when translation fails?

**Answer:** Translation means the provider walks the expression tree produced by `IQueryable` operators and converts supported nodes into a target language — usually SQL for EF Core — so work runs on the data store. When translation fails, the provider cannot map a method or expression to remote logic, and EF Core either throws an exception (preferred in strict configurations) or falls back to client evaluation that may pull excessive data into memory.

- Translatable patterns include property comparisons, standard string methods EF maps to SQL functions, and joins the relational model supports.
- Non-translatable patterns include arbitrary instance methods, complex object graphs built with custom constructors, and some date or culture-specific formatting.
- A failed translation is a design signal: push logic into the database with translatable expressions, precompute values, or switch to explicit SQL or raw queries.
- Logging generated SQL during development catches translation gaps before they become production performance incidents.

---

#### Q10. How do you inspect or debug the SQL generated by an `IQueryable` provider?

**Answer:** With EF Core, call `ToQueryString()` on an `IQueryable` to see the SQL the provider would send, or enable sensitive logging and log the `DbCommand` text through `LogTo` or your logging pipeline. These techniques show the translated query before it executes so you can verify filters, joins, and column projection.

- `ToQueryString()` is the quickest ad-hoc check in a debugger or unit test without running the query.
- `context.Database.Log` existed in EF6; in EF Core use `optionsBuilder.LogTo(Console.WriteLine)` or integrate with `ILogger`.
- Tools like SQL Server Profiler, Extended Events, or database-side logs confirm what actually ran in production.
- Compare LINQ changes to generated SQL after each refactor — a harmless-looking `.Select` can alter joins or introduce client evaluation.

---

#### Q11. What is the difference between chaining LINQ operators vs building queries incrementally with `if` conditions?

**Answer:** Chaining applies operators in a fixed order on one expression, while incremental building starts from a base `IQueryable` or `IEnumerable` and conditionally assigns the result of each operator to the same variable when filters are optional. Both produce equivalent expression trees when done correctly; incremental building is the standard pattern for dynamic search forms with optional criteria.

- Chaining: `source.Where(a).Where(b).Select(c)` — simple when all clauses always apply.
- Incremental: `IQueryable<T> query = dbSet; if (hasFilter) query = query.Where(...);` — each assignment extends the same deferred tree without executing it.
- Avoid executing early (`ToList()`) inside conditional branches unless you intentionally switch from remote to in-memory processing.
- For `IEnumerable<T>`, the same pattern works; for EF Core, keep the variable typed as `IQueryable<T>` as long as possible to preserve translation.

---

#### Q12. What are common signs that LINQ is hurting readability, and how do you refactor without losing composability?

**Answer:** LINQ hurts readability when a single expression nests many operators, repeats complex lambdas, mixes unrelated concerns, or hides side effects inside selectors. Refactor by extracting named methods or local functions for predicates and projections, splitting stages with well-named variables, or moving stable sub-queries into private methods that return `IQueryable<T>` or `IEnumerable<T>`.

- Deep nesting (`SelectMany` inside nested ternaries inside `GroupBy` result selectors) is a signal to break into steps with comments or method names.
- Duplicate lambda logic across `Where` and `Select` suggests a shared local function or a small domain method on the entity.
- Side effects (`SaveChanges`, logging entire sequences) inside LINQ operators violate the declarative model — execute them before or after the query pipeline.
- Composability is preserved when extracted methods accept and return sequences rather than materializing prematurely.

---

#### Q13. How does LINQ interact with nullable reference types and null propagation in projections?

**Answer:** Nullable reference type annotations flow through LINQ projections when selectors preserve nullability, but the compiler cannot always prove null-safety across arbitrary lambdas. Use null-conditional (`?.`) and null-coalescing (`??`) inside `Select` projections, guard nullable keys before `GroupBy`, and annotate Data Transfer Object (DTO) properties to match what the query can actually produce.

- A `Select` that reads `customer.Address?.City` yields a nullable string; the target DTO property should be `string?` unless you coalesce to a default.
- `FirstOrDefault` on reference types returns null when empty; nullable analysis expects you to check before dereferencing.
- EF Core translates some null checks to SQL `IS NULL` semantics; conflating null keys and missing rows affects joins and grouping.
- `#nullable enable` helps catch projections that assume non-null navigation properties when the database allows null foreign keys.

---

#### Q14. What is query rewriting (e.g., `let`, `join`, `group` in query syntax) at a high level?

**Answer:** Query rewriting is the compiler phase that transforms query syntax into method syntax by mapping each clause to a specific `Queryable` or `Enumerable` method call. Clauses like `let` introduce intermediate projected values, `join` maps to `Join` or `GroupJoin`, and `group` maps to `GroupBy`, producing a nested method chain equivalent to what you would write by hand.

- A `let` clause typically becomes a `Select` that pairs each element with a computed value, then subsequent clauses reference that value.
- `join ... on ... equals ...` desugars to `Join` with key selectors on both sides; `into` groups use `GroupJoin` followed by `SelectMany` or projection.
- `group by` produces `GroupBy`; an optional `into` clause continues querying against each group sequence.
- Understanding rewriting explains why some query-syntax orderings differ from method-syntax ordering and why both forms must follow the provider's translation rules identically.

---

### 02. Filtering & Aggregation

#### Q1. What is the difference between `.Where()` and `.Select()` in intent and output shape?

**Answer:** `Where` filters the sequence by keeping only elements that satisfy a predicate, so the output has the same element type and at most the same count as the input. `Select` projects each element into a new shape, so the output count matches the input count but the element type changes to whatever the selector returns.

- `Where` is a narrowing operation — it removes rows from the pipeline without transforming survivors.
- `Select` is a mapping operation — every input element yields exactly one output element (unless combined with operators like `SelectMany` later).
- Typical order is filter then project: reduce the set before paying the cost of building DTOs or computed fields.
- Both are deferred; neither runs until enumeration or a terminal operator executes the pipeline.

---

#### Q2. When should you filter before projecting vs project before filtering?

**Answer:** Filter before projecting when the predicate can be expressed on the source type and you want to avoid building expensive projections for rows you will discard. Project before filtering when the filter condition depends on computed fields that exist only after transformation, or when pushing the filter closer to the data source still translates correctly in EF Core.

- In LINQ to Objects, filtering first avoids allocating projection objects for excluded elements.
- In EF Core, filtering on table columns before `Select` to DTOs usually keeps predicates in SQL and reduces columns transferred.
- If the filter uses a calculated value (`Select` then `Where` on the DTO), ensure EF can still translate it or accept client evaluation cost.
- Readability sometimes favors a single `Select` that includes flags, followed by `Where` on those flags — balance clarity with translation and performance.

---

#### Q3. What is the difference between `.Count()`, `.LongCount()`, `.Sum()`, `.Average()`, `.Min()`, and `.Max()`?

**Answer:** `Count` and `LongCount` count elements (with optional predicates); `Sum`, `Average`, `Min`, and `Max` are numeric or comparable aggregations over element values or key selectors. All are terminal operators that enumerate the sequence unless the underlying collection implements optimized paths.

| Operator | Purpose | Empty sequence behavior (typical) |
|---|---|---|
| `Count` / `LongCount` | Element count | Returns `0` |
| `Sum` | Total of numeric values | Returns `0` for numeric types |
| `Average` | Mean of numeric values | **Throws** `InvalidOperationException` |
| `Min` / `Max` | Smallest / largest | **Throws** on empty sequences |

- `LongCount` exists for sequences exceeding `int.MaxValue` — rare in application code but part of the contract.
- Overloads accept a selector (`Sum(o => o.Total)`) so you aggregate a property without a separate `Select`.
- Nullable overloads propagate null semantics — for example `Average` on nullable decimals ignores null inputs but still throws if no non-null values exist.

---

#### Q4. What happens when `.Average()` or `.Sum()` is called on an empty sequence?

**Answer:** `Sum` on an empty sequence of numeric types returns the additive identity (`0`, `0m`, `0L`, etc.), while `Average` throws `InvalidOperationException` because dividing by zero elements is undefined. `Min` and `Max` also throw on empty sequences for the same reason — there is no value to return.

- Use `DefaultIfEmpty()` before `Average` if you want a defined fallback such as zero, or check `Any()` first.
- Nullable numeric `Average` ignores null elements; if all elements are null or the sequence is empty, it still throws or returns null depending on overload.
- EF Core translates these semantics to SQL (`AVG`, `SUM`) where empty groups behave per database rules — test edge cases for grouped queries.
- Defensive API code often uses `Average()` only after confirming non-empty input or catches the exception at a boundary layer.

---

#### Q5. What is `.Aggregate()`, and how does it generalize other aggregations?

**Answer:** `Aggregate` folds a sequence into a single accumulated value by applying a user-defined function across elements, starting from the first element or from an explicit seed. It generalizes `Sum`, `Count`, string concatenation, and custom reductions that do not have dedicated LINQ operators.

- Without a seed, the first element becomes the initial accumulator and the delegate combines each subsequent element — empty sequences throw.
- With a seed, the accumulator starts at the seed value and processes every element — empty sequences return the seed.
- The accumulator delegate receives the running result and the next element: `(acc, item) => ...`.
- Any aggregation expressible as repeated pairwise combination can be written with `Aggregate`, though dedicated operators are clearer when they exist.

---

#### Q6. How do seed and accumulator overloads of `.Aggregate()` work? Give use cases (running totals, building strings, merging objects).

**Answer:** The seed overload initializes the accumulator before the first element is processed, then the accumulator function combines the current accumulator with each element in order. Use a seed of `0m` for running totals, `string.Empty` or a `StringBuilder` pattern for concatenation, or a mutable builder object for merging composite results.

- Running total: `transactions.Aggregate(0m, (sum, t) => sum + t.Amount)` — same net effect as `Sum` but shows the general pattern.
- String building: prefer `string.Join` for readability, but `Aggregate("", (s, x) => s + x)` illustrates fold semantics (note efficiency cost of repeated string concat).
- Merging objects: seed a DTO and fold each row into it when no built-in operator matches your aggregation shape.
- Order matters for non-commutative operations — `Aggregate` processes left-to-right sequentially.

---

#### Q7. What is the difference between `.Aggregate()` with and without a result selector?

**Answer:** The two-parameter seed and accumulator form returns the final accumulator type directly, while the three-parameter overload adds a `resultSelector` that transforms the accumulator into a different result type after the fold completes. The result selector runs once on the final accumulated value, not per element.

- `(seed, func)` → returns `TAccumulate` after processing all elements.
- `(seed, func, resultSelector)` → returns `TResult` from `resultSelector(finalAccumulator)`.
- Example: accumulate a list internally, then return `list.Count` via `resultSelector` — useful when the fold state differs from the public result.
- Without a seed, there is no result-selector overload that avoids the empty-sequence throw — the first element seeds the fold implicitly.

---

#### Q8. What is `.Count(predicate)` vs `.Where().Count()` in terms of readability and performance?

**Answer:** Both count elements matching a condition and, for general `IEnumerable<T>`, require a single linear scan with equivalent performance. `Count(predicate)` is more concise and expresses intent directly; `Where().Count()` separates filtering from counting and composes better when you reuse the filtered sequence.

- `ICollection<T>.Count` is O(1), but `Count()` extension on a plain sequence still walks all elements unless the runtime recognizes a collection interface.
- `Count(predicate)` short-circuits only when implemented on specialized providers; LINQ to Objects scans the full sequence.
- For EF Core, both forms typically translate to `SELECT COUNT(*) ... WHERE` with no meaningful difference.
- Choose whichever reads clearer in context; avoid `Where().Count() > 0` when `Any(predicate)` communicates better (See Q9).

---

#### Q9. What is the difference between `.Any()` and `.Count() > 0` for `IEnumerable<T>` vs `ICollection<T>`?

**Answer:** Semantically both test whether at least one element exists, but `Any()` short-circuits on the first match while `Count() > 0` may enumerate the entire sequence. On `ICollection<T>`, the `.Count` property is O(1), so `collection.Count > 0` is efficient, but `Any()` remains clearer for general sequences.

- Prefer `Any()` on deferred or unknown sequence types — it stops at the first matching element.
- `Count() > 0` on a lazy infinite sequence is dangerous if no early match exists.
- For `List<T>` or arrays, `Count > 0` or `Length > 0` is fine and fast.
- `Any(predicate)` vs `Where(predicate).Any()` mirrors the same readability guidance as `Count(predicate)`.

---

#### Q10. How do nullable numeric aggregations behave in LINQ to Objects?

**Answer:** Nullable numeric aggregations (`Sum`, `Average`, `Min`, `Max` on `int?`, `decimal?`, etc.) skip null elements during computation rather than treating null as zero. If all values are null or the sequence is empty, `Sum` returns null for nullable return types, while `Average`, `Min`, and `Max` return null or throw depending on overload and content.

- `Sum` on `{ null, 2, null, 3 }` yields `5` for `int?` overloads.
- `Average` divides by the count of non-null elements present, not the total sequence length.
- This matches SQL nullable aggregation semantics closely, which helps when mirroring database reports in memory.
- When you need null treated as zero, project with coalescing before aggregating: `Select(x => x.Value ?? 0)`.

---

#### Q11. What is the difference between `.Sum()` on `int` vs `long` vs `decimal` regarding overflow?

**Answer:** `Sum` uses the numeric type of the sequence or selector, and overflow follows that type's arithmetic rules — `int` and `long` sums can silently overflow in unchecked contexts, while `decimal` has a larger practical range but still bounded precision. Choose a wider type in the selector when summing many large values.

- Summing many large `int` values can wrap past `int.MaxValue` without exception in unchecked code.
- Cast in the selector — `Sum(x => (long)x.Amount)` — promotes accumulation to a safer width before final assignment.
- Financial totals should use `decimal` end-to-end to avoid floating-point drift from `double`.
- EF Core translates `Sum` to SQL types that may overflow at the database layer — align CLR and column types deliberately.

---

#### Q12. How do aggregations translate (or fail to translate) in LINQ to Entities?

**Answer:** Standard aggregations (`Count`, `Sum`, `Average`, `Min`, `Max`) translate to SQL aggregate functions when applied directly on an `IQueryable` over mapped entities or projections the provider understands. Translation fails when the selector invokes non-translatable methods, references client-only objects, or nests aggregations the relational engine cannot express.

- Grouped aggregations become `GROUP BY` with aggregate columns in the `SELECT` list.
- Client-side aggregates after `AsEnumerable()` run in memory on materialized rows — sometimes intentional, often accidental.
- Some complex folds (`Aggregate` with arbitrary delegates) never translate — rewrite as SQL-friendly `Sum`/`Count` or compute after materialization.
- Always inspect SQL for grouped queries to ensure aggregates execute server-side.

---

#### Q13. When would you use `.Aggregate()` instead of a simple loop for custom accumulation logic?

**Answer:** Use `Aggregate` when the fold is a single expression that fits cleanly in a LINQ pipeline and you want to stay declarative alongside neighboring operators. Prefer an explicit `foreach` when the accumulation involves multiple statements, exception handling, async work, or branching that would make a lambda unreadable.

- `Aggregate` shines for functional-style one-liners in tests or report pipelines already written in LINQ.
- Loops are clearer for complex state machines, early exit, or `await` inside accumulation — `Aggregate` does not support async delegates.
- Debugging stepped accumulation is often easier with breakpoints inside a loop than inside a nested lambda.
- Neither is inherently faster; choose based on clarity and composability with the surrounding query.

---

#### Q14. What are pitfalls of aggregating floating-point values from large sequences?

**Answer:** Floating-point (`float`, `double`) aggregation accumulates rounding error because binary floating-point cannot represent all decimal fractions exactly, and repeated additions compound tiny errors. For money or precision-sensitive metrics, use `decimal`; for scientific workloads, consider compensated summation or algorithms stable at scale.

- `(0.1 + 0.2)` style surprises appear in large `Sum`/`Average` over `double` sequences.
- Parallel aggregation (`ParallelEnumerable.Sum`) reorders additions and can yield slightly different rounding than sequential sums.
- Comparing aggregated doubles with `==` is unreliable — use epsilon tolerances.
- Database `float`/`real` columns inherit the same issues when mirrored in LINQ projections.

---

### 03. Ordering

#### Q1. What is the difference between `OrderBy().ThenBy()` and calling `OrderBy()` twice?

**Answer:** `OrderBy` establishes the primary sort key, and `ThenBy` (or `ThenByDescending`) adds secondary keys that break ties from the prior level. Calling `OrderBy` a second time replaces the entire ordering with a new primary key, discarding the previous sort rather than appending to it.

- Correct multi-key sort: `OrderBy(x => x.LastName).ThenBy(x => x.FirstName)`.
- Incorrect pattern: `OrderBy(x => x.LastName).OrderBy(x => x.FirstName)` — only first name matters in the final order.
- Each `OrderBy` returns `IOrderedEnumerable<T>`, which exposes `ThenBy`; plain `IEnumerable<T>` after a non-ordered operator may require a fresh `OrderBy`.
- In query syntax, multiple `orderby` clauses compile to `ThenBy` chains when written correctly.

---

#### Q2. What is stable sort in LINQ to Objects, and why does it matter?

**Answer:** A stable sort preserves the relative order of elements that compare equal on the sort key. LINQ to Objects `OrderBy` uses a stable sort algorithm, so items with the same key remain in their original input order — important when you sort by one field but care about prior ordering as a tiebreaker.

- Stability lets you sort by priority while preserving insertion order among equal priorities without an explicit secondary key.
- Unstable sorts would shuffle equal-key items unpredictably between runs.
- When stability matters across equal keys, either rely on LINQ's stable sort or add an explicit `ThenBy` on a unique field like ID.
- Reporting and paging scenarios often depend on deterministic ordering among ties.

---

#### Q3. Does `OrderBy` guarantee stability across all .NET versions and providers?

**Answer:** LINQ to Objects documents stable ordering for `OrderBy`, but remote providers like EF Core delegate to the database engine, which may not guarantee stability unless you define it with explicit keys. Never assume stability from SQL `ORDER BY` without tie-breaker columns.

- In-memory LINQ to Objects stability is part of the documented behavior for `OrderBy`/`ThenBy`.
- SQL Server and other databases do not promise row order among equal keys unless you specify additional sort columns.
- EF Core translates `ThenBy` to comma-separated `ORDER BY` columns — the reliable cross-provider approach.
- For paginated APIs, always include a unique tie-breaker (primary key) in the sort chain.

---

#### Q4. What is the difference between `OrderBy` and `OrderByDescending` when keys compare equal?

**Answer:** When keys compare equal, neither ascending nor descending order changes relative placement among those elements — stability preserves their prior order in LINQ to Objects. The descending variant only reverses the ordering direction among keys that compare differently.

- Equal keys: stable sort keeps original relative order for both `OrderBy` and `OrderByDescending`.
- Different keys: ascending places smaller keys first; descending places larger keys first.
- Combine with `ThenBy` when equal primary keys should be ordered by a secondary field explicitly.
- Do not expect descending to invert tie order among equal primary keys.

---

#### Q5. How do `ThenBy` and `ThenByDescending` chain comparers?

**Answer:** `ThenBy` and `ThenByDescending` are only available on `IOrderedEnumerable<T>` returned by `OrderBy`/`OrderByDescending` and compose additional sort levels using the same or a custom comparer. Each call creates a new ordered sequence that sorts by the new key only where previous keys tied.

- Chain as many levels as needed: `OrderBy(...).ThenBy(...).ThenByDescending(...)`.
- Custom comparers pass to `OrderBy(comparer)` and propagate through subsequent `ThenBy` overloads accepting `IComparer<TKey>`.
- Reordering the chain changes semantics — secondary keys cannot be applied before the primary key exists.
- After a non-ordering operator (`Where`), you must call `OrderBy` again to start a new sort chain.

---

#### Q6. Can you sort by multiple keys using query syntax? How?

**Answer:** Yes — query syntax supports comma-separated keys in an `orderby` clause, where later keys compile to `ThenBy` calls on the ordered sequence. You can mix ascending and descending per key with `ascending` and `descending` keywords.

```csharp
from c in customers
orderby c.Region ascending, c.Revenue descending, c.Name
select c
```

- The compiler emits `OrderBy(Region).ThenByDescending(Revenue).ThenBy(Name)`.
- Readability for multi-column sorts is a major reason teams choose query syntax.
- Method syntax with explicit `ThenBy` is equivalent and often used in fluent EF Core pipelines.

---

#### Q7. What is the difference between sorting before `GroupBy` vs sorting groups with `OrderBy` on the outer sequence?

**Answer:** Sorting before `GroupBy` orders elements within the source stream, which may affect group member order if the grouping preserves input order (LINQ to Objects does). Sorting after `GroupBy` orders the groups themselves by a key or aggregate, not the individual members unless you sort inside each group separately.

- Pre-group sort: `source.OrderBy(x => x.Date).GroupBy(x => x.Category)` — members inside each group follow date order.
- Post-group sort: `source.GroupBy(...).OrderBy(g => g.Key)` — orders groups by key, not members.
- To sort members within each group, project with `Select(g => new { g.Key, Items = g.OrderBy(i => i.Date) })`.
- SQL translation may push these sorts to different query phases — verify generated SQL for EF Core reports.

---

#### Q8. How does `IOrderedEnumerable<T>` differ from `IEnumerable<T>`?

**Answer:** `IOrderedEnumerable<T>` extends `IEnumerable<T>` and represents a sequence that has an active sort ordering, exposing `ThenBy` and `ThenByDescending` for multi-key sorts. Once you apply a non-ordering operator that returns plain `IEnumerable<T>`, you lose `ThenBy` unless you call `OrderBy` again.

- `OrderBy` returns `IOrderedEnumerable<T>`; `Where` after it returns `IEnumerable<T>` without `ThenBy`.
- Pattern: apply all filters first, then `OrderBy`/`ThenBy`, then `Take`/`Skip` for paging.
- The interface carries no extra runtime state visible to callers — it is primarily a typing signal for composable sorting.
- Provider implementations may differ, but the contract guides correct method chaining at compile time.

---

#### Q9. When should you pass a custom `IComparer<T>` to `OrderBy`?

**Answer:** Pass a custom comparer when the default comparer for `T` does not match your business ordering — case-insensitive strings, culture-specific names, version numbers, or composite rules not expressible in a simple key selector. The comparer defines how two keys compare during the sort.

- `StringComparer.OrdinalIgnoreCase` is a common built-in comparer for case-insensitive sorts.
- Complex rules (status priority enums not aligned with numeric values) map cleanly to dedicated `IComparer<T>` implementations.
- Keep comparers pure and consistent with `Compare` contract to avoid undefined sort behavior.
- EF Core cannot translate arbitrary `IComparer` instances — use translatable key expressions or sort in memory after materialization.

---

#### Q10. How does ordering translate to SQL in EF Core (`ORDER BY`, composite keys)?

**Answer:** EF Core maps `OrderBy` and `ThenBy` to SQL `ORDER BY` columns in chain order, using ascending by default and `DESC` for descending overloads. Composite sorts become comma-separated columns, which databases use to order rows deterministically when all columns are specified.

- `OrderBy(c => c.LastName).ThenBy(c => c.FirstName)` → `ORDER BY [LastName], [FirstName]`.
- Ordering must occur before `Skip`/`Take` paging in translated queries for stable pages.
- Sorting by unindexed columns on large tables can be expensive — align with database indexes where possible.
- Client-side `OrderBy` after `AsEnumerable()` sorts in memory on fetched rows only.

---

#### Q11. What is the performance characteristic of `OrderBy` in LINQ to Objects?

**Answer:** LINQ to Objects `OrderBy` typically uses an efficient sort (often introspective sort) with O(n log n) time and O(n) extra space for the ordering buffer. It fully enumerates the source, materializes keys and elements, sorts, then yields results — it is not a streaming O(1) operation.

- Large in-memory sorts allocate buffers proportional to sequence length.
- Multiple `OrderBy`/`ThenBy` levels still culminate in a sort pass — not free chaining at runtime.
- For nearly sorted data, specialized algorithms might win, but LINQ uses general-purpose sorting.
- If you only need the top K items, consider partial selection algorithms or database-side `ORDER BY ... LIMIT` instead of sorting entire lists in memory.

---

#### Q12. What happens if the key selector throws for some elements during ordering?

**Answer:** If the key selector throws while `OrderBy` evaluates keys during the sort pass, enumeration aborts with that exception and no complete ordered sequence is returned. LINQ to Objects evaluates keys as part of sorting, so the failure surfaces at execution time, not query construction time.

- Null reference exceptions in key selectors (`x => x.Address.City` when `Address` is null) are a common source.
- Defensive projections (`x => x.Address?.City ?? ""`) prevent throws during key extraction.
- EF Core may translate some null-safe patterns to SQL `COALESCE`; others fail translation.
- Tests should include null and edge-case rows for sort keys used in production reports.

---

#### Q13. How do `OrderBy` and `Reverse()` interact — does `Reverse` undo sort stability semantics?

**Answer:** `OrderBy` produces a stably sorted forward view; `Reverse` iterates that view in reverse index order, which reverses the entire sequence including tie groups as a block — it does not re-sort by key. Applying `Reverse` after `OrderBy` yields descending-like behavior only when keys are unique; equal keys reverse as a stable group in opposite relative order.

- `OrderBy(x => x.Key).Reverse()` is not identical to `OrderByDescending(x => x.Key)` when stability and equal keys matter.
- Prefer `OrderByDescending`/`ThenByDescending` when you want descending key order with stable tie handling.
- `Reverse` on unordered sequences walks to the end if needed — costly for deferred chains without random access.
- On `IList<T>`, indexed reverse behaves differently from LINQ `Reverse` extension semantics — know your collection type.

---

#### Q14. When is in-memory sorting the wrong choice for large EF Core queries?

**Answer:** In-memory sorting is wrong when it forces materialization of large row sets before paging or when the database could sort and page indexed columns more efficiently. Calling `AsEnumerable()` then `OrderBy` pulls rows into the app process and sorts them locally, defeating server-side optimization.

- Correct pattern for large tables: keep `OrderBy`/`Skip`/`Take` on `IQueryable` so SQL handles sort and page.
- Reporting exports that truly need all rows are the exception — still stream or batch when possible.
- Sorting millions of DTOs in RAM causes memory pressure and garbage collection pauses.
- If translation requires client sort, reconsider the query shape or add computed indexed columns in the database.

---

### 04. Grouping

#### Q1. Explain `GroupBy` in LINQ to Objects — what is the shape of the result?

**Answer:** `GroupBy` partitions a source sequence into buckets keyed by a common value and returns `IEnumerable<IGrouping<TKey, TElement>>`, where each `IGrouping` exposes a `Key` and enumerates the elements in that bucket. Execution is deferred until you enumerate groups or their contents.

- Each source element appears in exactly one group determined by its key (colliding keys share a group).
- `IGrouping<TKey, TElement>` extends `IEnumerable<TElement>` — you `foreach` members inside each group.
- Nested iteration pattern: outer loop groups, inner loop items — common for console reports and UI sections.
- The original element order within a group follows input order in LINQ to Objects (stable partitioning).

---

#### Q2. What is the difference between `GroupBy(keySelector)` and `GroupBy(keySelector, elementSelector)`?

**Answer:** The single-parameter overload groups whole source elements by key, while the two-parameter overload groups projected elements — each item is transformed by `elementSelector` before it is placed in its bucket. Use the element overload when groups should contain a subset or projection of the original records.

- `GroupBy(t => t.Category)` yields `IGrouping<string, Ticket>` with full `Ticket` members.
- `GroupBy(t => t.Assignee, t => t.Title)` yields `IGrouping<string, string>` with only titles per assignee.
- Element projection reduces memory when groups need only display fields, not entire entities.
- EF Core translates both forms when selectors map to translatable columns.

---

#### Q3. What is the difference between `GroupBy` with a result selector vs post-processing grouped sequences?

**Answer:** `GroupBy` with a result selector folds each group into one result object in a single operator, while post-processing uses `GroupBy` then `Select` on groups to compute per-group summaries. Both can produce identical output; the result-selector overload avoids exposing intermediate `IGrouping` sequences explicitly.

- Result selector signature: `(key, IEnumerable<TElement> items) => TResult`.
- Post-processing: `GroupBy(...).Select(g => new { g.Key, Count = g.Count() })` — clearer when logic grows complex.
- EF Core may translate simple aggregates in either form to `GROUP BY` with aggregate columns.
- Choose based on readability — deeply nested result selectors become hard to debug.

---

#### Q4. How does `GroupBy` differ when translated to SQL (LINQ to Entities) vs in-memory?

**Answer:** In-memory `GroupBy` builds group buckets in the application after enumerating data (unless the source is already structured). EF Core translates `GroupBy` to SQL `GROUP BY` with aggregates computed server-side, returning one row per group key when the projection is translatable.

- SQL grouping requires keys and aggregates expressible relationally — arbitrary per-group imperative code does not translate.
- Client `GroupBy` after premature materialization groups in RAM — acceptable for small sets, catastrophic for large tables.
- Nested object keys may translate to multiple column groupings depending on EF version and mapping.
- Always inspect SQL when introducing grouped EF queries — accidental client grouping is a common performance bug.

---

#### Q5. What is the difference between `GroupBy` and `ToLookup()`?

**Answer:** `GroupBy` is deferred and returns mutable-enumeration group views each time you iterate, while `ToLookup()` executes immediately and returns an immutable `ILookup<TKey, TElement>` indexed by key for repeated random access. Use `GroupBy` for one-pass reporting pipelines; use `ToLookup` when you need a read-only dictionary-like structure with multiple values per key.

- `ToLookup` eagerly reads the entire source once at creation time.
- `ILookup` supports `lookup[key]` returning a sequence (possibly empty) without re-walking the source.
- `GroupBy` re-enumerates the source when you iterate groups unless the source is a materialized list and you only enumerate once.
- `ToLookup` throws no error on missing keys — it returns an empty sequence.

---

#### Q6. When should you use `ToLookup()` instead of `GroupBy().ToDictionary()`?

**Answer:** Use `ToLookup` when keys map to many elements per key and you want immutable multi-map storage without duplicate-key exceptions. `GroupBy().ToDictionary(g => g.Key, g => g.ToList())` works but requires manual list materialization and throws if duplicate keys appear in the dictionary layer (they should not if keys come from groups).

- `ToLookup` natively models one-to-many key → elements relationships.
- `ToDictionary` requires unique keys — one value per key unless values are collections you build yourself.
- Prefer `ToLookup` for join-like pre-indexing when outer keys may match multiple inner rows.
- Both execute eagerly at creation — not deferred like bare `GroupBy`.

---

#### Q7. What is `ILookup<TKey, TElement>` and how is it immutable compared to `Dictionary` of lists?

**Answer:** `ILookup<TKey, TElement>` is a read-only multi-map interface returned by `ToLookup`, exposing keys and sequences of elements per key without add/remove mutations. A `Dictionary<TKey, List<T>>` you build manually is mutable — callers can add keys or mutate lists unless you wrap or expose read-only views.

- `ILookup` enforces immutability of the lookup structure after creation — safe to share across threads if the contents are immutable.
- Dictionary-of-lists patterns require discipline to prevent concurrent mutation bugs.
- Indexing syntax differs: `lookup[key]` always succeeds with possibly empty sequence; dictionary access throws on missing keys unless you use `TryGetValue`.
- Choose `ILookup` when the grouping is a snapshot; choose dictionary when you need ongoing updates.

---

#### Q8. How do you group by composite keys (anonymous types, value tuples, custom key types)?

**Answer:** Return a composite object from the key selector — anonymous types, value tuples, or custom records with proper equality — so elements with the same combination of fields land in one group. LINQ compares keys with default equality comparers for those types.

```csharp
items.GroupBy(x => (x.Region, x.Category));
items.GroupBy(x => new { x.Region, x.Category });
```

- Value tuples and anonymous types implement value equality on their components automatically.
- Custom key types must implement `Equals` and `GetHashCode` (records do this by default).
- EF Core translates tuple and anonymous key projections to multi-column `GROUP BY` when supported.
- Composite keys make reports explicit — prefer them over string concatenation keys that hide delimiter collisions.

---

#### Q9. How does key equality affect grouping (`Equals`/`GetHashCode`, reference vs value semantics)?

**Answer:** Grouping uses equality comparers (default or explicit) to decide bucket membership — keys that compare equal merge into one group. Reference types without overridden equality group by reference identity; value types and records group by value unless a custom comparer changes behavior.

- Two different string instances with same content group together because `string` overrides equality.
- Two class instances with identical field values but no equality override land in separate groups unless you pass a custom comparer.
- Consistent `GetHashCode` with `Equals` is required for correct hash-based grouping algorithms.
- EF Core maps equality to SQL equality on projected columns — null key handling follows database semantics.

---

#### Q10. What is the difference between `group by` in query syntax and method syntax?

**Answer:** Query syntax `group element by key` desugars to `GroupBy` with the same semantics as method syntax; optional `into name` introduces a group alias for continuing the query against groups. Method syntax exposes overloads for element and result selectors directly.

```csharp
// query syntax
from t in tickets group t by t.Priority into g select new { g.Key, Count = g.Count() };

// method syntax
tickets.GroupBy(t => t.Priority, (key, items) => new { Key = key, Count = items.Count() });
```

- Both compile to equivalent expression trees for EF when translatable.
- Query syntax reads naturally for reports; method syntax fits fluent pipelines.
- Continuation after `group ... into` requires projection — you cannot silently mix ungrouped and grouped variables without scope rules.

---

#### Q11. How do you flatten or regroup nested groups efficiently?

**Answer:** Flatten nested groups with `SelectMany` on group sequences, or regroup by projecting flattened elements with a new `GroupBy` key. Avoid repeated full scans by materializing intermediate groups only when the source is expensive to re-enumerate.

- Flatten: `outerGroups.SelectMany(g => g)` or `SelectMany(g => g.Select(inner => ...))` depending on nesting shape.
- Regroup: `items.SelectMany(...).GroupBy(newKey)` after joining or nesting operations.
- For large data, prefer a single SQL query with appropriate joins and `GROUP BY` over nested in-memory regrouping.
- `SelectMany` is the idiomatic flatten operator after hierarchical `GroupBy` results.

---

#### Q12. What are common mistakes grouping large EF Core queries (client evaluation, pulling too much data)?

**Answer:** Common mistakes include materializing entities before grouping, using non-translatable per-group logic, and fetching all rows then grouping in memory when SQL could aggregate. These patterns inflate memory, network I/O, and query time silently.

- Anti-pattern: `ToList()` then `GroupBy` on millions of rows — loads entire table first.
- Anti-pattern: custom instance methods inside grouping projections EF cannot translate.
- Prefer `GroupBy` on `IQueryable` with translatable aggregates (`Count`, `Sum`) so SQL returns one row per group.
- Log and review SQL whenever adding grouped endpoints to APIs or dashboards.

---

#### Q13. How does `GroupBy` interact with ordering of elements within each group?

**Answer:** LINQ to Objects preserves source order within each group based on the order elements appeared in the input sequence. If you need a specific order inside groups, sort before grouping, or sort each group's items when projecting (`g.OrderBy(x => x.Date)`).

- SQL group member order is undefined unless you use window functions or subqueries with explicit ordering — do not rely on database order without `ORDER BY`.
- `orderby` before `group` in query syntax can influence in-memory member order.
- Reports showing "top item per group" often use ordering plus `First` per group rather than assuming group enumeration order in SQL.
- Document ordering requirements explicitly in API contracts when clients depend on item sequence within groups.

---

#### Q14. When would you prefer `Dictionary` manual grouping over `GroupBy` for performance?

**Answer:** Prefer a manual `Dictionary<TKey, List<T>>` (or `Dictionary<TKey, TAccumulate>`) when you need single-pass mutation with incremental aggregation — especially in hot loops where LINQ delegate overhead and multiple enumerations matter. `GroupBy` remains clearer for most business queries and one-off reports.

- Manual grouping shines in tight performance paths: parsing streams, building indexes while reading files once.
- Update aggregates while inserting (`dict[key].Total += amount`) avoids a second pass over groups.
- LINQ `GroupBy` allocates grouping structures and is deferred — fine for clarity, not always for micro-optimized inner loops.
- Concurrent scenarios use `ConcurrentDictionary` and manual logic rather than LINQ `GroupBy`.

---

### 05. Joins

#### Q1. What is the difference between inner join, left join, and cross join in LINQ?

**Answer:** An inner join returns pairs where keys match on both sides; a left (outer) join keeps all outer rows and fills missing inner matches with null/default; a cross join pairs every outer row with every inner row with no key predicate. LINQ expresses these with `join`, `GroupJoin` + `DefaultIfEmpty`, or `SelectMany` without a filter.

| Join type | Keeps unmatched outer rows | Keeps unmatched inner rows |
|---|---|---|
| Inner | No | No |
| Left outer | Yes (null inner) | No |
| Cross | N/A (no key) | N/A |

- Inner join is the default `join ... on ... equals ...` when both sides must match.
- Left outer join uses `GroupJoin` followed by `SelectMany` and `DefaultIfEmpty` in method syntax.
- Cross join: `from a in outer from b in inner select ...` without an `equals` clause — use carefully because cardinality multiplies.

---

#### Q2. How do you express a left outer join in method syntax vs query syntax?

**Answer:** Query syntax uses `join ... into ... from ... DefaultIfEmpty()` pattern; method syntax uses `GroupJoin` then `SelectMany` with `DefaultIfEmpty()` on the inner sequence. Both retain outer rows when no inner match exists.

```csharp
// query syntax (left outer)
from c in customers
join o in orders on c.Id equals o.CustomerId into orderGroup
from o in orderGroup.DefaultIfEmpty()
select new { c.Name, OrderId = o?.Id };

// method syntax
customers.GroupJoin(orders, c => c.Id, o => o.CustomerId, (c, og) => new { c, og })
         .SelectMany(x => x.og.DefaultIfEmpty(), (x, o) => new { x.c.Name, OrderId = o?.Id });
```

- `DefaultIfEmpty()` supplies a null/default inner element for empty groups — the hallmark of outer join semantics.
- Nullable reference annotations help when outer join yields null inner entities.
- EF Core translates this pattern to `LEFT JOIN` when the shape is supported.

---

#### Q3. What is the difference between `join` and `GroupJoin`?

**Answer:** `Join` flattens matching pairs into a single result sequence (inner join semantics by default), while `GroupJoin` preserves each outer element and attaches a group of matching inner elements as a nested sequence. `GroupJoin` is the building block for outer joins and hierarchical results before flattening.

- `Join`: one output row per match — duplicates outer rows if multiple inner matches exist.
- `GroupJoin`: one outer row with `IEnumerable<Inner>` — duplicates appear inside the group.
- Left outer join composes `GroupJoin` + `SelectMany` + `DefaultIfEmpty`.
- Choose `GroupJoin` when you need all related inner items collected per outer key without immediate flattening.

---

#### Q4. When should you use `GroupJoin` followed by `SelectMany` vs a direct `join`?

**Answer:** Use direct `join` when you want a flat inner-join result with one row per match. Use `GroupJoin` + `SelectMany` when you need outer join semantics, optional flattening, or intermediate grouping before projection — for example collecting all orders per customer then expanding or summarizing.

- Inner join flat report: `join` or `SelectMany` with equijoin predicate.
- Left outer join: always `GroupJoin` + `SelectMany` + `DefaultIfEmpty`.
- One-to-many without duplicating outer columns in flat form: sometimes keep grouped shape and project summaries without `SelectMany`.
- EF Core translation supports both patterns when keys and projections are translatable.

---

#### Q5. How do you join on composite keys using anonymous types or tuples?

**Answer:** Project composite keys on both sides with matching anonymous types or value tuples in the `equals` clause so the compiler requires structural equality on all components.

```csharp
join inner in innerSet
  on new { outer.A, outer.B } equals new { inner.A, inner.B }
```

- Tuple form: `on (outer.A, outer.B) equals (inner.A, inner.B)` in supported C# versions.
- Both sides must have the same anonymous type shape or equivalent tuple arity and types.
- EF Core maps composite keys to multi-column join conditions in SQL.
- Composite keys are clearer than concatenating fields into one string key.

---

#### Q6. What are equality requirements for join keys?

**Answer:** Join keys must be comparable with consistent equality semantics on both sides — same types or compatible types with meaningful equality, and non-null key consistency where required. LINQ uses equality comparers (default or specified) to match outer and inner keys.

- Reference types without overridden equality join by reference identity unless a custom comparer is supplied.
- Nullable value types follow lifted equality — two null keys may match depending on provider translation.
- EF Core requires translatable equality expressions — arbitrary `Equals` overrides on unmapped types may fail.
- Mismatched key types (`int` vs `long`) may not compile or may require explicit casts in the key selectors.

---

#### Q7. What is the difference between equijoin and non-equijoin — can LINQ express non-equijoins cleanly?

**Answer:** An equijoin matches rows where key values are equal; a non-equijoin uses range, inequality, or arbitrary predicates (for example "find orders within 10 days of signup"). LINQ query syntax emphasizes equijoin with `equals`, but method syntax `SelectMany` with a `where` clause expresses non-equijoins cleanly.

```csharp
from c in customers
from o in orders
where o.CustomerId == c.Id && o.Date >= c.SignupDate
select new { c, o };
```

- This cross join + filter pattern is the idiomatic non-equijoin in LINQ.
- EF Core may translate range joins to SQL with appropriate `WHERE` clauses on joins or cross apply patterns.
- Not all non-equijoin shapes perform well — database indexes and query plans matter.
- Equijoin syntax cannot replace arbitrary predicates in the `on` clause — use `where` instead.

---

#### Q8. How do joins translate to SQL in EF Core (`INNER JOIN`, `LEFT JOIN`)?

**Answer:** LINQ `join` translates to SQL `INNER JOIN` on the specified key columns; left outer patterns with `DefaultIfEmpty` translate to `LEFT JOIN`. EF Core chooses join order and types based on the expression tree, navigation properties, and mapping configuration.

- Explicit LINQ joins become SQL joins even when navigations exist — useful for shaped DTO queries.
- Navigation-based `Include` uses joins or separate queries depending on strategy — different from LINQ join syntax but related physically.
- Composite keys become multi-column join predicates.
- Review SQL when upgrading EF versions — translation improvements can change join shapes.

---

#### Q9. What is a many-to-many join pattern in LINQ?

**Answer:** Many-to-many joins traverse a linking entity or join table with two equijoins: outer to link on first key, link to inner on second key. In EF Core with skip navigations, a single navigation may hide the join table but LINQ still resolves to multiple tables in SQL.

```csharp
from m in members
join mm in memberships on m.Id equals mm.MemberId
join g in groups on mm.GroupId equals g.Id
select new { m.Name, g.Name };
```

- Explicit join table models make queries visible; convenience navigations simplify syntax.
- Duplicate pairings appear if link tables contain duplicate rows — enforce uniqueness at the database.
- Project only needed columns to avoid materializing full entities from both sides.
- Consider database views or denormalized read models for heavy many-to-many reporting.

---

#### Q10. What performance pitfalls appear when joining large in-memory sequences vs database-side joins?

**Answer:** In-memory joins on large sequences require loading both sides into RAM and typically use hash-join or nested-loop strategies in LINQ to Objects, which does not benefit from database indexes. Database-side joins filter early, use indexes, and return only projected columns across the network.

- Anti-pattern: fetch two entire tables with EF then `join` in memory after `AsEnumerable()`.
- Hash join in LINQ still allocates lookup structures proportional to inner sequence size.
- Network transfer dominates when materializing wide entities instead of projecting keys and needed fields.
- Push joins to `IQueryable` whenever both sides map to database tables.

---

#### Q11. How does `DefaultIfEmpty()` enable left outer join semantics?

**Answer:** After `GroupJoin`, each outer row has an inner sequence that may be empty when no matches exist. `DefaultIfEmpty()` on that inner sequence yields a single null/default element instead of an empty enumeration, allowing `SelectMany` to emit one outer row paired with null inner data.

- Without `DefaultIfEmpty`, outer rows with no matches disappear from flattened results — inner join behavior.
- Nullable reference types signal that inner side may be absent after outer join projection.
- Value types use default (`0`, `false`) unless you use nullable wrappers for semantic clarity.
- EF Core recognizes this pattern and maps to `LEFT JOIN` with null columns for unmatched sides.

---

#### Q12. What is the difference between a join and a correlated subquery in LINQ query syntax?

**Answer:** A join pairs two sequences explicitly on keys and returns combined projections, while a correlated subquery references an outer variable inside a nested `from` or `where` clause to filter related data per outer row. Both can express similar relationships; joins are clearer for equijoins, correlated queries for existence checks or top-N per group patterns.

- Exists pattern: `where inner.Any(i => i.CustomerId == c.Id)` correlates without flattening pairs.
- Join pattern: produces flat pairs suitable for reports needing columns from both sides.
- EF Core may translate both to SQL subqueries or joins depending on shape and optimization.
- N+1 risk rises when correlation forces per-outer queries — batch with joins or includes instead.

---

#### Q13. When is `Join` preferable to building a `Dictionary` lookup manually?

**Answer:** `Join` is preferable for declarative pipelines, EF translation, and readable equijoin reports where both sides are sequences of similar scale and you want provider optimization. Manual dictionary lookup wins for repeated lookups against a static inner table while streaming a large outer sequence once.

- LINQ `Join` integrates with deferred EF queries and composable projections.
- Dictionary lookup: build `ToLookup` or `Dictionary` on inner keys once, then probe while enumerating outer — O(n + m) with clear control.
- Hybrid: materialize small reference tables to dictionaries, join large fact tables in SQL.
- Choose based on execution location (database vs memory) and how often inner data is reused.

---

#### Q14. What happens when duplicate keys exist on the inner or outer sequence?

**Answer:** Inner duplicates produce multiple output rows for the same outer key in a flat inner join — one row per matching inner record. Outer duplicates similarly repeat with each matching inner row. `GroupJoin` collects all inner duplicates into one group per outer key without duplicating the outer element in the grouped structure.

- Inner join cardinality: `|outer matches| × |inner matches per key|` in worst case.
- If you expected one row per outer key, duplicates signal data quality issues or wrong join type.
- Use grouping or distinct operations when business rules require one inner match per outer key.
- EF Core does not deduplicate automatically — SQL join semantics apply.

---

### 06. Element Operations

#### Q1. What is the difference between `.First()`, `.FirstOrDefault()`, `.Single()`, and `.SingleOrDefault()` — when does each throw?

**Answer:** `First` and `Single` throw when the sequence is empty; `FirstOrDefault` and `SingleOrDefault` return default instead. `Single` and `SingleOrDefault` also throw when more than one element exists; `First` variants return the first match regardless of additional elements.

| Operator | Empty sequence | More than one match |
|---|---|---|
| `First` | Throws | Returns first |
| `FirstOrDefault` | Returns default | Returns first |
| `Single` | Throws | Throws |
| `SingleOrDefault` | Returns default | Throws |

- Predicate overloads apply the same rules after filtering.
- Use `Single` only when uniqueness is a business invariant you want enforced at runtime.
- Prefer `First` when any representative match suffices.

---

#### Q2. What is the difference between `.Last()` and `.LastOrDefault()` on deferred vs indexed sequences?

**Answer:** On indexed collections (`IList<T>`), `Last` can access the final element efficiently; on deferred forward-only sequences, `Last` must enumerate the entire sequence to find the last item. `LastOrDefault` returns default when empty; otherwise both return the final element after full traversal for non-indexed sources.

- `List<T>` optimizes `Last` via index; linked lists still require walking nodes.
- Deferred chains pay O(n) cost even if you only need the tail — consider storing order differently.
- EF Core may translate `Last` to SQL with `ORDER BY ... DESC LIMIT 1` when keys support it — provider-dependent.
- Never call `Last` on infinite sequences — it does not terminate.

---

#### Q3. What is the difference between `.ElementAt(index)` and indexing (`list[index]`)?

**Answer:** `ElementAt` is an extension method that walks forward from the start until it reaches the index on general sequences, while `list[index]` on `IList<T>` uses direct random access in O(1) time. Both throw (or return default with `ElementAtOrDefault`) when the index is out of range, but performance characteristics differ sharply.

- Arrays and `List<T>`: prefer indexer for clarity and speed.
- Pure `IEnumerable<T>` from LINQ pipelines: `ElementAt` may re-walk from the beginning each call — O(n) per access.
- Repeated random access on a deferred sequence suggests materializing to a list first.
- Negative indices are invalid for `ElementAt` — unlike Python-style indexing.

---

#### Q4. What is `.ElementAtOrDefault()` behavior for out-of-range indexes?

**Answer:** `ElementAtOrDefault` returns `default(T)` when the index is negative or greater than or equal to the sequence length without throwing. For reference types that is null; for value types it is zeroed bits.

- Contrast with `ElementAt`, which throws `ArgumentOutOfRangeException` for invalid indexes.
- On empty sequences, index `0` returns default — same as `FirstOrDefault` for that case.
- Still O(n) walk for non-indexed sequences — not a free random access tool.
- Validate business meaning of default — `0` may be a valid ID and confuse callers if used as sentinel.

---

#### Q5. How do element operations behave on empty sequences for each overload?

**Answer:** Strict operators (`First`, `Last`, `Single`, `ElementAt`) throw `InvalidOperationException` on empty sequences (or `ArgumentOutOfRangeException` for bad indexes). OrDefault variants return default values; predicate overloads behave the same after finding zero matches.

- Throwing operators signal violated expectations — useful when data integrity guarantees non-empty results.
- OrDefault operators suit optional lookups where absence is normal.
- `Single` on empty throws — not the same as `SingleOrDefault`.
- Document API contracts so callers know whether absence is exceptional or routine.

---

#### Q6. What is the difference between `.First(predicate)` vs `.Where(predicate).First()`?

**Answer:** Both return the first matching element and throw if none match (for non-OrDefault overloads), with equivalent linear scan behavior. `First(predicate)` is more concise; `Where(predicate).First()` composes when you reuse the filtered sequence or add more operators before taking first.

- Short-circuiting stops at the first match in both cases.
- EF Core typically translates both to SQL with `TOP 1` or `LIMIT 1` and a `WHERE` clause.
- Avoid `Where(...).First()` when `First(...)` reads cleaner — no performance gain either way for LINQ to Objects.
- OrDefault forms return default when no predicate match exists.

---

#### Q7. Why can `.Single()` be dangerous on filtered EF Core queries?

**Answer:** `Single` throws if zero or more than one row matches, which is fragile when filters are optional, data contains duplicates, or concurrency adds rows between validation and query. A report expecting exactly one ledger row can fail in production when duplicates exist or filters broaden.

- Prefer `First` when you want one row but duplicates merely mean "pick any" or ordering picks the canonical row.
- Use `Single` deliberately to enforce uniqueness constraints during development and tests.
- API layers can catch `InvalidOperationException` and translate to 409 Conflict — but fixing data or query is better.
- `SingleOrDefault` still throws on duplicates — only `FirstOrDefault` avoids the multiple-match throw.

---

#### Q8. What is the time complexity of `.ElementAt()` on a linked list vs `IList<T>`?

**Answer:** On `IList<T>` with O(1) indexing, `ElementAt` is O(1); on forward-only linked structures masquerading as `IEnumerable<T>`, each `ElementAt(n)` call is O(n) because it walks from the start. Repeated random access on linked lists is O(k × n) for k accesses — materialize to an array or list first.

- `LinkedList<T>` does not implement random access indexers — `ElementAt` walks nodes.
- `List<T>` and arrays delegate to fast index access internally when the runtime recognizes `IList<T>`.
- PLINQ and deferred LINQ chains always walk from the start unless buffered.
- Algorithm design should not call `ElementAt(i)` inside a loop over i on cold enumerables.

---

#### Q9. When should you use `.FirstOrDefault()` vs `.SingleOrDefault()` defensively in APIs?

**Answer:** Use `FirstOrDefault` when zero or one match is acceptable and multiple matches should not throw — you accept any first row if duplicates exist. Use `SingleOrDefault` when zero is acceptable but multiple matches indicate a bug you want surfaced immediately.

- Lookup by non-unique display name: `FirstOrDefault` — duplicates may exist historically.
- Lookup by primary key after validation: `SingleOrDefault` enforces uniqueness if the key truly is unique.
- Public APIs should document which semantics apply so consumers know whether duplicates throw.
- Consider ordering (`OrderBy`) before `FirstOrDefault` when "first" must be deterministic among duplicates.

---

#### Q10. How do element operators short-circuit enumeration?

**Answer:** Operators like `First`, `Any`, and `Single` (when early mismatch found) stop enumerating as soon as the answer is determined — they do not require visiting the rest of the sequence unless needed to detect multiple matches for `Single`.

- `First` stops after the first element (or first matching predicate hit).
- `Any` stops on first true predicate evaluation.
- `Single` must see zero, one, or more than one — may read two elements and stop if the second proves non-uniqueness, but empty requires full knowledge of emptiness immediately.
- Short-circuiting saves I/O when sequences are lazy streams (files, network) — unless you enumerate again later.

---

#### Q11. What exceptions are thrown vs null/default returned for reference and value types?

**Answer:** Throwing overloads raise `InvalidOperationException` for empty sequences or multiple matches where applicable; OrDefault overloads return `default(T)` which is null for reference types and zeroed values for value types. `ArgumentNullException` applies when the source sequence itself is null, not when it is empty.

- Reference type OrDefault: null means "not found" — callers must null-check.
- Value type OrDefault: `0` or `false` may be valid data — use nullable return types or `Try` patterns when ambiguous.
- Predicate overloads throw when no element satisfies the predicate for strict operators.
- Do not conflate empty sequence with null source — different exceptions and meanings.

---

#### Q12. How do `.MinBy()` / `.MaxBy()` (modern LINQ) relate to element operations conceptually?

**Answer:** `MinBy` and `MaxBy` ( .NET 6+) return the element with the minimum or maximum key according to a selector or comparer, not just the extremal key value like `Min`/`Max`. Conceptually they combine ordering/key comparison with element selection — closer to `OrderBy(key).First()` than to pure aggregation.

- `orders.MaxBy(o => o.Total)` returns the whole order object with highest total.
- Ties: return the first extremal element encountered per implementation rules — document if business needs deterministic tie-breaking.
- Empty sequences throw like `Max`/`Min` — no OrDefault variants in the base API.
- Use when you need the record itself for follow-up logic, not only the numeric extreme.

---

### 07. Set Operations

#### Q1. What is the difference between `.Distinct()`, `.Union()`, `.Intersect()`, and `.Except()`?

**Answer:** `Distinct` removes duplicates within one sequence; `Union` combines two sequences with distinct results; `Intersect` keeps elements appearing in both; `Except` keeps elements in the first sequence not present in the second. All use equality semantics to decide membership.

| Operator | Input | Result |
|---|---|---|
| `Distinct` | One sequence | Unique elements |
| `Union` | Two sequences | All unique from either |
| `Intersect` | Two sequences | In both |
| `Except` | Two sequences | In first but not second |

- Set operators in LINQ to Objects use hash-set algorithms with expected linear time after hashing.
- They do not sort — order follows implementation rules (often first-seen order preserved for Distinct).
- EF Core translates supported set operations to SQL `UNION`, `INTERSECT`, `EXCEPT`, or `DISTINCT` when available.

---

#### Q2. How does equality comparer selection work for set operations?

**Answer:** Set operators accept an optional `IEqualityComparer<T>`; otherwise they use `EqualityComparer<T>.Default`, which respects type-specific `Equals`/`GetHashCode` overrides. Reference types without overrides dedupe by reference identity, which surprises developers expecting value equality.

- Pass `StringComparer.OrdinalIgnoreCase` for case-insensitive string sets.
- Custom comparers must be consistent — violating comparer contracts causes undefined deduplication.
- EF Core cannot translate arbitrary custom comparers — equality must map to SQL expressions.
- Same comparer should be used across `Union`/`Intersect`/`Except` operands for coherent semantics.

---

#### Q3. What is `.DistinctBy()` (modern LINQ), and how does it differ from `.GroupBy().Select(g => g.First())`?

**Answer:** `DistinctBy` ( .NET 6+) returns the first element for each distinct key produced by a selector, preserving first-seen order among unique keys. `GroupBy(...).Select(g => g.First())` achieves similar results but materializes grouping structures and reads less directly for "unique by property" intent.

- `people.DistinctBy(p => p.Email)` keeps first person per email.
- GroupBy approach still works on older runtimes and when you need more than the first element per key.
- Both depend on key equality semantics for what "distinct" means.
- EF Core 7+ may translate `DistinctBy` to SQL window or distinct patterns — verify provider support.

---

#### Q4. Are `Union`/`Intersect`/`Except` multisets or sets — how are duplicate inputs handled?

**Answer:** LINQ set operators behave as sets, not multisets — duplicates collapse according to equality. `Union` does not preserve duplicate counts from inputs; `Intersect` and `Except` likewise treat membership as boolean presence.

- Duplicate inputs in the first sequence: `Distinct` semantics apply in the output of `Union`.
- For multiset semantics (bag union), count duplicates manually or use `GroupBy` with sums of counts.
- SQL `UNION` vs `UNION ALL` differs — LINQ `Union` maps to distinct union, not `UNION ALL`.
- Use `Concat` when you intentionally keep all duplicates from both lists.

---

#### Q5. What is the difference between `.Union()` and `.Concat().Distinct()`?

**Answer:** `Union` combines two sequences and removes duplicates in one operator, while `Concat` followed by `Distinct` concatenates first then deduplicates the merged stream — often equivalent results but different execution structure and readability.

- `Union` expresses intent directly — set union of two sources.
- `Concat().Distinct()` dedupes after concatenation — useful when chaining more than two sources with intermediate steps.
- Performance is similar asymptotically; both build hash-based uniqueness structures during enumeration.
- Choose based on clarity; avoid `Concat().Distinct()` when `Union` communicates the operation immediately.

---

#### Q6. How do set operations translate in EF Core?

**Answer:** EF Core translates `Distinct`, `Union`, `Intersect`, and `Except` on compatible `IQueryable` shapes to SQL set operators when the database provider supports them. Operand queries must project compatible column shapes; unsupported patterns force client evaluation or fail translation.

- SQL Server supports `UNION`, `INTERSECT`, `EXCEPT` in modern versions — provider maps accordingly.
- Complex entity graphs may require projecting to scalar/anonymous shapes before set operations translate.
- Combining set operations with ordering requires subqueries — SQL set results are unordered unless outer `ORDER BY` applies.
- Test generated SQL — provider version changes expand translatable set scenarios over time.

---

#### Q7. What is the performance of set operations on sorted vs unsorted inputs?

**Answer:** LINQ to Objects set operators generally use hash sets (expected O(n + m) time) regardless of input sort order — sorting does not eliminate hashing unless you implement merge-based algorithms yourself. Sorted inputs help only if you write custom merge logic for union/intersect on sorted enumerables.

- Do not sort solely before `Union` expecting LINQ to merge cheaply — it will not automatically use merge join.
- Database engines may choose merge or hash plans for SQL set operations based on indexes and statistics.
- Memory usage scales with distinct element counts, not raw input length alone.
- For very large on-disk sets, prefer database set operations over loading both into memory.

---

#### Q8. When would you use `HashSet<T>` manually instead of LINQ set operators?

**Answer:** Use `HashSet<T>` when you need incremental add/remove membership testing in a loop, repeated lookups while streaming data, or mutable set state shared across methods. LINQ set operators build new sequences declaratively — better for one-shot transformations in queries.

- Manual `HashSet` supports O(1) `Contains` while constructing the set once during parsing or graph walks.
- LINQ `Except`/`Intersect` recompute when you re-enumerate unless materialized.
- Thread-safe scenarios may use concurrent dictionaries or immutable hash sets instead of LINQ.
- Interop with APIs expecting `HashSet<T>` parameters requires manual construction regardless.

---

#### Q9. How do reference equality and value equality change set operation results?

**Answer:** Reference-equal types without overridden equality treat two objects with identical field values as distinct set members if they are different instances. Value types and types with proper equality overrides dedupe by value, which changes counts for `Distinct`, `Union`, and related operators.

- Two `new { X = 1 }` anonymous objects in separate lists are never equal by reference — set ops keep both unless projected to comparable keys first.
- Strings and integers dedupe by value automatically.
- Records and structs with value equality behave predictably in set operations.
- Pass custom equality comparers when domain objects need value semantics without modifying the class.

---

#### Q10. What is the difference between set operations on in-memory sequences vs `IQueryable`?

**Answer:** In-memory set operations execute locally with CLR equality and full method availability, while `IQueryable` set operations depend on EF Core translation to SQL set algebra with database equality semantics and type system rules.

- Client-side set ops after `AsEnumerable()` process potentially huge materialized lists.
- Server-side set ops reduce data movement when both operands are database queries with compatible projections.
- Null handling in SQL three-valued logic can differ subtly from CLR boolean logic in intersect/except edge cases.
- Keep operands as `IQueryable` until the set operation completes, then materialize once if needed.

---

### 08. Projection Operations

#### Q1. What is the difference between `.Select()` and `.SelectMany()`?

**Answer:** `Select` projects each input element to exactly one output element, while `SelectMany` flattens each input element to zero or more output elements by merging nested sequences into one stream. Use `SelectMany` for one-to-many expansions like orders to line items.

```csharp
orders.Select(o => o.Lines.Count);           // one int per order
orders.SelectMany(o => o.Lines);             // flat stream of all line items
```

- `Select` preserves cardinality — count in equals count out.
- `SelectMany` cardinality equals sum of inner sequence lengths.
- Both are deferred until enumeration.
- EF Core translates `SelectMany` on navigations to SQL joins or subqueries when mapped correctly.

---

#### Q2. When should you use `.SelectMany()` for one-to-many relationships?

**Answer:** Use `SelectMany` when each parent record owns a collection you want to flatten into a single sequence — line items across orders, tags across posts, or characters across strings with `SelectMany(c => c)`.

- Without `SelectMany`, `Select` yields nested `IEnumerable<T>` objects instead of flat rows.
- Join patterns can alternatively flatten related tables — choose based on whether data is hierarchical in one graph or split across sequences.
- Watch for duplicate parent data when projecting parent fields alongside many child rows.
- N+1 query problems in EF often come from loading parents then `SelectMany` over lazy collections — eager loading or explicit joins fix translation.

---

#### Q3. What is projection to anonymous types vs named DTOs — trade-offs for maintenance and testing?

**Answer:** Anonymous types are convenient for ad-hoc queries within a method because the compiler infers shape and equality, but they cannot cross assembly boundaries as public API return types. Named DTOs (records or classes) are explicit, reusable, testable, and stable contracts for APIs, mapping layers, and serialization.

- Anonymous: fast for local reports and `var` projections — refactor tools struggle with property renames across files.
- Named DTOs: version intentionally, document fields, and unit test serializers and mappers.
- EF Core projects equally well to both when properties are translatable.
- Public endpoints should return named types — anonymous types are internal implementation details.

---

#### Q4. How do you project into nested shapes or hierarchical DTOs?

**Answer:** Use `Select` to construct nested objects or records in one projection, assigning child collections from navigation properties or subqueries. Shape the DTO to match client needs rather than entity layout.

```csharp
customers.Select(c => new CustomerDto(
    c.Name,
    c.Orders.Select(o => new OrderDto(o.Id, o.Total)).ToList()));
```

- EF Core may translate nested projections to SQL with joins or split queries depending on version and configuration.
- Deep graphs risk cartesian explosion — consider multiple queries or DTOs with IDs only plus separate fetches.
- Immutable records make nested projections readable with positional syntax.
- Validate null navigations when building nested trees.

---

#### Q5. What is the difference between `.Select()` before vs after `.Where()` for EF Core translation?

**Answer:** Filtering before projection keeps predicates on entity columns when possible, often producing narrower SQL with fewer columns read. Projecting first can still translate if the filter uses projected fields expressible in SQL, but projecting wide graphs before filtering may fetch unnecessary columns or navigations.

- Preferred: `Where` on table columns, then `Select` to DTO — classic push-filter-down pattern.
- Filter on computed projection when the computation is translatable (for example string functions on columns).
- Projecting entire entities then filtering client-side defeats column slicing.
- Query plan quality depends on indexes on filtered columns regardless of LINQ order when translation succeeds.

---

#### Q6. How does `.Select()` interact with nullable reference types?

**Answer:** Projections determine nullability of DTO properties — accessing nullable navigations without guards yields nullable target properties the compiler tracks. Use null-conditional and coalescing operators in selectors to match intended DTO contracts and satisfy nullable analysis.

- `Select(c => c.Address!.City)` suppresses warnings but throws at runtime if null.
- Prefer `Select(c => c.Address == null ? null : c.Address.City)` or nullable DTO fields.
- EF translates null checks to SQL `IS NULL` patterns when possible.
- Enable nullable context on DTO definitions to catch mismatches at compile time.

---

#### Q7. What is a selector that returns `IEnumerable<T>` vs flattened `SelectMany`?

**Answer:** A `Select` selector returning `IEnumerable<T>` produces a sequence of sequences — outer enumeration yields inner enumerables you must nested-loop or flatten. `SelectMany` performs that flatten in one operator, concatenating inner sequences into a single stream.

- `Select` with inner enumerables: `[[a,b],[c]]` structure conceptually.
- `SelectMany`: `[a,b,c]` flat stream.
- Choose `Select` when you intentionally want grouped nested results without flattening yet.
- Async variants follow similar composition rules in IAsyncEnumerable pipelines.

---

#### Q8. How do you project with index using `.Select((item, index) => ...)`?

**Answer:** The indexed overload passes each element and its zero-based position during enumeration, enabling rank labels, synthetic row numbers, or alternating patterns without manual counter variables.

```csharp
items.Select((item, index) => new { Index = index + 1, item.Name });
```

- Index reflects enumeration order, not sort order unless you `OrderBy` first.
- EF Core may not translate indexed `Select` — often client-only after materialization.
- For stable row numbers in SQL, use window functions via raw SQL or provider-specific translations.
- Indexed projection pairs well with in-memory reporting after materialized lists.

---

#### Q9. What are common causes of N+1 queries related to projection in EF Core?

**Answer:** N+1 queries occur when EF loads a set of parent rows, then issues one query per parent when the projection touches unloaded navigations or executes separate queries per item during enumeration. Projecting with lazy-loaded collections or calling database methods inside loops triggers repeated round trips.

- Fix with explicit `Include`, projection joins, split queries configured appropriately, or single SQL with joins.
- `SelectMany` over navigation without translation plan may degenerate to per-parent loads.
- Use `AsSplitQuery` or shaped includes when cartesian risk exists — monitor SQL logs.
- Integration tests with query counting catch N+1 regressions early.

---

#### Q10. How does `let` in query syntax relate to projection and intermediate variables?

**Answer:** The `let` clause introduces a named intermediate value for each source element, similar to projecting into an anonymous pair then continuing the query. It improves readability when the same computed expression is reused in `where`, `orderby`, and `select` clauses.

```csharp
from o in orders
let tax = o.Total * 0.08m
where tax > 10
select new { o.Id, tax };
```

- Compiler rewrites `let` to nested `Select` calls carrying forward computed fields.
- Reuse reduces duplicate expression trees in EF translation.
- Overuse of `let` can obscure method-syntax equivalents — balance clarity.
- Translatability follows the same rules as any subexpression in projections.

---

#### Q11. When does projection cause full entity materialization vs column slicing in SQL?

**Answer:** Selecting entire tracked entities (`Select(c => c)` or returning `DbSet` entities without narrowing) materializes all mapped columns and enables change tracking. Projecting to anonymous types, DTOs, or scalar subsets allows EF to generate `SELECT col1, col2` with narrower rows and often no tracking overhead.

- DTO projections are the default pattern for read APIs and reports.
- `AsNoTracking` complements projections for read-only queries even when entities are returned.
- Including navigations expands SQL joins or additional queries — not column slicing on a single table.
- Accidental entity return from API endpoints increases payload and tracking cost.

---

#### Q12. What is the difference between projecting computed values vs mapping existing properties only?

**Answer:** Mapping existing properties translates trivially to column selections in SQL, while computed values require expressions EF can translate (operators, known functions) or force client evaluation if they invoke arbitrary C# logic.

- Computed: `Select(o => new { TotalWithTax = o.Total * 1.08m })` — usually translatable arithmetic.
- Non-translatable: `Select(o => new { Label = o.FormatCustom() })` using instance methods — client eval or error.
- Computed projections can leverage SQL server functions when mapped via EF.Functions.
- Keep business formatting in memory after fetching minimal data when translation fails.

---

#### Q13. What is `.Zip()`, and how do you combine two sequences element-by-element (including unequal lengths)?

**Answer:** `Zip` pairs elements by position from two sequences into a result sequence using a result selector, stopping when the shorter sequence is exhausted. Unequal lengths truncate silently at the shorter length — no padding unless you preprocess with `DefaultIfEmpty` or generation tricks.

```csharp
var pairs = names.Zip(scores, (name, score) => new { name, score });
```

- .NET 6+ overload can pass index as third parameter to the selector.
- For unequal lengths needing padding, align lengths first or use outer join patterns instead.
- EF Core may not translate `Zip` — typically an in-memory operator.
- Useful for parallel arrays and combining aligned streams after sorting both sides.

---

### 09. Quantifier Operations

#### Q1. What do `.All()`, `.Any()`, and `.Contains()` do, and when would you use each?

**Answer:** `Any` checks whether at least one element exists (optionally matching a predicate); `All` checks whether every element satisfies a predicate; `Contains` checks whether a specific value appears in the sequence using equality. Use `Any` for existence, `All` for universal validation rules, and `Contains` for membership of a known item.

- `Any()` without predicate tests non-emptiness — prefer over `Count() > 0` on deferred sequences.
- `All` validates rules like "every line item has positive quantity" across a cart.
- `Contains` maps cleanly to SQL `IN` for constant values or parameter lists in EF Core.
- All three short-circuit when possible — they stop enumerating once the answer is determined.

---

#### Q2. What is the difference between `.Any(predicate)` and `.Where(predicate).Any()`?

**Answer:** Both determine whether any element satisfies the predicate with equivalent linear scan and short-circuit behavior. `Any(predicate)` states intent directly; `Where(predicate).Any()` composes when you reuse the filtered sequence for additional operators.

- No meaningful performance difference for LINQ to Objects.
- EF Core translates both to SQL `EXISTS` subqueries or equivalent filters in most cases.
- Prefer `Any(predicate)` for readability in validation checks.
- Avoid materializing `Where(...).ToList()` just to call `Any`.

---

#### Q3. How does `.All()` behave on an empty sequence (vacuous truth)?

**Answer:** `All` on an empty sequence returns `true` because there are no counterexamples to the predicate — logically "all zero elements satisfy the condition." This vacuous truth surprises developers expecting false on empty inputs.

- Validate emptiness separately with `Any()` when business rules require at least one element.
- Example: `empty.All(x => x > 0)` is `true` even though no elements were checked.
- SQL `ALL` on empty sets has related three-valued logic nuances — test EF translations for edge cases.
- Document API behavior when empty collections pass `All` checks unintentionally.

---

#### Q4. What is the difference between `.Contains(item)` and `.Any(x => x.Equals(item))` with custom equality?

**Answer:** `Contains` uses `IEqualityComparer<T>.Default` or an optional comparer overload to test membership, while `Any` with `Equals` invokes the element's equality method per comparison without the collection's comparer semantics in all overload paths. For custom equality, pass an explicit comparer to `Contains` or use `Any` with a predicate reflecting your rule.

- `HashSet.Contains` uses set comparer — consistent for set-backed collections.
- Reference types without overridden equality: both may compare references unless comparer specified.
- EF Core translates `Contains` on constants to SQL `IN`; arbitrary `Any` lambdas may differ in translation.
- Choose `Contains` when testing literal membership in a list of keys for SQL `IN` patterns.

---

#### Q5. How do quantifiers short-circuit enumeration?

**Answer:** `Any` stops at the first matching element; `All` stops at the first failing element; `Contains` stops when it finds an equal item (for linear searches). None require full enumeration once the boolean result is determined unless no early exit occurs.

- Short-circuiting makes quantifiers efficient on large lazy streams and I/O-bound enumerables.
- Worst case still O(n) when all elements must be inspected (for example `All` succeeds only after the last element).
- Re-enumerating deferred queries repeats work — materialize once if multiple quantifiers run on the same expensive pipeline.
- Database engines apply similar early-exit optimizations for `EXISTS` queries.

---

#### Q6. How do quantifiers translate to SQL (`EXISTS`, `IN`, `ALL`) in EF Core?

**Answer:** `Any` with predicates typically becomes SQL `EXISTS` subqueries; `Contains` on collections of constants becomes `IN (...)`; nested `Any` on navigations correlates to `EXISTS` with join conditions. `All` may translate to `NOT EXISTS` negated predicates when expressible.

- `customers.Where(c => c.Orders.Any(o => o.Total > 1000))` → correlated exists on orders.
- Large `Contains` lists become large `IN` clauses — parameterization and table-valued parameters help on SQL Server.
- Non-translatable predicates inside quantifiers break translation.
- Review SQL for quantifiers over navigations — correct translation avoids client-side filtering of entire tables.

---

#### Q7. When is `.Contains` with a large in-memory list a performance problem for EF Core?

**Answer:** EF Core expands `Contains` over large in-memory collections into large SQL `IN` clauses or many parameters, which can exceed database parameter limits, defeat index use, or produce huge query plans. Prefer joining to temp tables, table-valued parameters, or storing filter keys in a database table for large sets.

- Hundreds of IDs may be fine; tens of thousands often are not.
- Split batches or use `WHERE id IN (SELECT id FROM @tvp)` patterns on SQL Server.
- Alternative: fetch keys into a temp staging table and join in SQL.
- Monitor query plan cache churn from constantly varying `IN` list sizes.

---

#### Q8. What is the difference between `.Any()` on `IQueryable` vs materialized collections?

**Answer:** `Any` on `IQueryable` executes a SQL existence check (often `SELECT CASE WHEN EXISTS(...)`) without necessarily retrieving all rows, while `Any` on materialized collections scans in-memory elements. Both short-circuit, but the queryable form pushes work to the database.

- `dbSet.Any()` may translate to efficient `EXISTS` without full table scans if indexes support it.
- `list.Any()` walks memory — fast for small lists, costly for huge materialized sets you could have filtered in SQL.
- Calling `ToList()` before `Any` defeats server-side optimization.
- Keep existence checks in `IQueryable` form until execution boundary.

---

#### Q9. How do quantifiers interact with null keys or null elements in sequences?

**Answer:** Null elements participate in equality checks per `EqualityComparer` rules — `Contains(null)` works if null is in the sequence. Null keys in nested quantifiers over navigations follow SQL three-valued logic when translated — a null foreign key may cause rows to be excluded from matches unexpectedly if not handled explicitly.

- Predicate forms should account for null navigation properties before comparisons.
- `All(x => x.Name != null)` differs subtly from filtering nulls first then applying business rules.
- EF translates null-safe patterns to `IS NULL` checks when written explicitly.
- Vacuous `All` on empty still returns true regardless of null concerns.

---

#### Q10. When should you prefer `.All()` vs validating with `.Count()` or exceptions?

**Answer:** Prefer `All` for declarative validation of universal rules across sequences where vacuous truth on empty is acceptable or guarded separately. Use `Count` comparisons when you need exact cardinality, and exceptions when violated invariants represent exceptional control flow rather than boolean results.

- `All` reads intent for rules like "every shipment has a tracking number."
- `Count() == expected` enforces exact sizes — stricter than `All`.
- Throwing `Single` or custom exceptions suits API boundaries enforcing hard invariants.
- Combine `Any()` guard with `All` when empty input should fail validation explicitly.

---

#### Q11. What is `.SequenceEqual()`, and how does it compare sequences with optional `IEqualityComparer<T>`?

**Answer:** `SequenceEqual` returns true if two sequences have the same length and pairwise equal elements by position, using default or custom equality comparers. It is a quantifier-style terminal operator that fully enumerates both sequences unless lengths differ early.

- Order matters — `[1,2,3]` is not sequence-equal to `[3,2,1]`.
- Custom comparers enable case-insensitive string sequence comparisons in tests.
- Short-circuits on first mismatched pair or length difference after differing length detection strategy.
- Useful in unit tests comparing expected and actual LINQ output lists.

---

### 10. Conversion Operations

#### Q1. When should you use `.ToList()`, `.ToArray()`, `.ToDictionary()`, `.ToHashSet()`, and `.AsEnumerable()`?

**Answer:** Materialize with `ToList` or `ToArray` when you need repeated enumeration, indexing, or a snapshot; use `ToDictionary` for unique key maps; `ToHashSet` for unique unordered membership tests; `AsEnumerable` to switch from `IQueryable` to LINQ to Objects without copying data yet.

| Method | When to use |
|---|---|
| `ToList` / `ToArray` | Snapshot, multiple passes, indexing |
| `ToDictionary` | One value per unique key — throws on duplicates |
| `ToHashSet` | Fast membership, deduplication |
| `AsEnumerable` | Force client-side LINQ on `IQueryable` — defers copy |

- Materialize once at the execution boundary, not after every operator.
- `ToArray` is slightly leaner for fixed-size immutable snapshots; `ToList` supports add/remove later.
- `AsEnumerable` does not hit the database by itself — subsequent operators may trigger client evaluation.

---

#### Q2. What is `.ToLookup()` and when is it preferable to `.GroupBy().ToDictionary()`?

**Answer:** `ToLookup` eagerly builds an immutable multi-map from key to sequences of elements, supporting one-to-many relationships without duplicate-key errors. Prefer it over `GroupBy().ToDictionary` when you need immediate random access by key to many values per key.

- Executes immediately — not deferred.
- Missing keys return empty sequences, not exceptions.
- `GroupBy().ToDictionary(g => g.Key, g => g.ToList())` is manual and verbose compared to `ToLookup`.
- Ideal for join optimization after loading a reference dimension table once.

---

#### Q3. What are duplicate-key behaviors for `.ToDictionary()` vs `.ToLookup()`?

**Answer:** `ToDictionary` throws `ArgumentException` when a duplicate key is encountered because dictionary keys must be unique. `ToLookup` groups duplicates under the same key as multiple elements — duplicates are expected, not errors.

- Use `ToDictionary` only when keys are guaranteed unique by domain rules.
- `ToDictionary(x => x.Id)` fails if two items share the same Id.
- `ToLookup(x => x.Category)` collects all items sharing a category.
- For duplicate handling with last-wins semantics, manual dictionary loops or `GroupBy` projections are alternatives.

---

#### Q4. Why can calling `.ToList()` too early in an EF Core query hurt performance?

**Answer:** Early `ToList()` executes the query and loads all matching rows into memory before subsequent filters, joins, or projections run client-side. That defeats server-side optimization, increases network I/O, and can accidentally pull entire tables.

- Anti-pattern: `dbContext.Orders.ToList().Where(o => o.Total > 1000)` — filters in RAM after full load.
- Correct: `dbContext.Orders.Where(o => o.Total > 1000).ToList()` — SQL applies filter first.
- Early materialization is intentional only when switching to non-translatable CLR logic deliberately.
- Log row counts during development to catch runaway materializations.

---

#### Q5. What is the difference between `.AsEnumerable()` and `.ToList()` for switching from `IQueryable` to LINQ to Objects?

**Answer:** `AsEnumerable` wraps the queryable in an `IEnumerable` facade that will execute the underlying query when enumerated, then apply subsequent operators in memory without copying results upfront. `ToList` executes immediately and stores all rows in a list snapshot.

- `AsEnumerable` defers execution until enumeration — still one database round trip when consumed.
- `ToList` forces immediate full materialization — fixed snapshot thereafter.
- Both switch later operators to LINQ to Objects semantics.
- Choose `ToList` when you need multiple in-memory passes; `AsEnumerable` when you need one streaming pass with client-side operators at the end.

---

#### Q6. What is `.Cast<T>()` vs `.OfType<T>()` — when does each throw vs filter?

**Answer:** `Cast<T>` attempts to cast every element to `T` and throws `InvalidCastException` on the first incompatible item. `OfType<T>` silently skips elements that are not assignable to `T`, returning only compatible instances.

- `Cast<T>` assumes homogenous sequences — use when type mismatch is a bug.
- `OfType<T>` suits mixed collections (for example legacy `ArrayList`) or inheritance hierarchies.
- Neither converts numeric types — `Cast<int>` on `long` values throws even though conversion exists.
- Use `Select` with explicit conversion for numeric widening or parsing.

---

#### Q7. What is `.AsQueryable()` on an in-memory sequence — what provider backs it?

**Answer:** `AsQueryable` wraps an in-memory `IEnumerable` in an `IQueryable` facade backed by `EnumerableQuery`, which reverts to LINQ to Objects execution through expression tree compilation — it does not create a remote SQL provider.

- Useful for APIs accepting `IQueryable` in tests without a database.
- Expression trees compile to delegates executed locally — no EF translation occurs.
- Passing `AsQueryable()` local data to code expecting EF can mislead if callers assume SQL translation.
- Real EF queries use `IQueryable` from `DbSet` with EF's query provider.

---

#### Q8. How do `.ToArray()` and `.ToList()` differ for subsequent mutations and memory?

**Answer:** Both materialize the sequence eagerly; `ToArray` returns a fixed-size array that cannot change length, while `ToList` returns a mutable `List<T>` supporting add, remove, and resize. Arrays avoid list overhead slightly; lists offer flexibility.

- Repeated enumeration of either is cheap — no re-walking the original source.
- Mutations affect only the copy — original deferred query unchanged if not yet executed.
- `ToArray` may allocate one contiguous block — good for interop and span-friendly scenarios.
- Choose based on whether downstream code needs list mutability.

---

#### Q9. When should you use `.ToImmutableArray()` / `.ToImmutableList()` from System.Collections.Immutable?

**Answer:** Use immutable collections when you need thread-safe sharing without locks, defensive copies for public API returns, or functional-style pipelines where subsequent code must not mutate shared state.

- Conversion snapshots current sequence into immutable structures with copy cost upfront.
- Readers never see concurrent mutation — important for cached query results served to multiple threads.
- Standard LINQ ends with mutable lists in most app code — immutability adds value at public boundaries.
- Immutable types support efficient structural sharing for some operations but still pay initial materialization cost.

---

#### Q10. What is the cost of multiple conversions in a hot path?

**Answer:** Each `ToList`, `ToArray`, or `ToDictionary` allocates new buffers and enumerates the source — repeating conversions in tight loops multiplies allocations and CPU time. Materialize once, reuse the collection, or stay deferred until the final consumer.

- Anti-pattern: calling `ToList()` inside a loop on the same query each iteration.
- Profile hot paths — conversion plus LINQ delegate overhead adds up in high-throughput services.
- Prefer streaming enumeration when single-pass processing suffices.
- Cache materialized results when the same snapshot serves many iterations within a request scope.

---

#### Q11. How does `.ToDictionary()` handle null keys?

**Answer:** `ToDictionary` throws `ArgumentNullException` if a null key is produced because dictionary keys cannot be null in standard `Dictionary<TKey, TValue>`. Guard or filter null keys before conversion or use a sentinel key pattern if domain allows.

- `ToLookup` also rejects null keys for reference type keys in standard implementations.
- Nullable value type keys (`int?`) can hold null as key where `TKey` is `int?`.
- EF materialization rarely produces dictionary keys directly — usually an in-memory step after query.
- Validate data pipelines feeding dictionary construction.

---

#### Q12. When is explicit materialization required before passing sequences across async boundaries?

**Answer:** Materialize when the underlying data source is tied to a scoped resource — EF Core `DbContext`, open data reader, or network stream — that will be disposed when the async method returns. Deferred `IQueryable` or open enumerables cannot safely be consumed after the context is gone.

- Pattern: `await query.ToListAsync()` inside the context scope, then pass the list to background work.
- Multiple enumeration of deferred queries after context disposal throws or fails unpredictably.
- Snapshot DTO lists decouple lifetime of data from database connection lifetime.
- IAsyncEnumerable streams are a separate model — consume within the async enumeration scope with proper cancellation.

---

### 11. Partitioning Operations

#### Q1. What is the difference between `.Take()`, `.Skip()`, `.TakeWhile()`, and `.SkipWhile()`?

**Answer:** `Take` and `Skip` partition by fixed count; `TakeWhile` and `SkipWhile` partition by a predicate evaluated sequentially from the start. Fixed operators use counts; conditional operators stop or start based on element tests in order.

| Operator | Behavior |
|---|---|
| `Take(n)` | First n elements |
| `Skip(n)` | All but first n elements |
| `TakeWhile(pred)` | Elements from start while pred true — stops at first false |
| `SkipWhile(pred)` | Skip while pred true, then return rest including first false |

- `TakeWhile` does not resume if later elements would match again — it only reads from the beginning.
- Combine `Skip` and `Take` for offset/limit paging after stable sort.
- All are deferred until enumeration except when followed by terminals.

---

#### Q2. How do `.Take`/`Skip` translate to SQL paging in EF Core?

**Answer:** EF Core maps `Skip(n).Take(m)` to SQL `OFFSET n ROWS FETCH NEXT m ROWS ONLY` (or provider equivalents like `LIMIT`/`OFFSET`) when ordering is defined and translation succeeds. Without `OrderBy`, paging is nondeterministic at the database level.

- Always pair paging with explicit sort keys including a unique tie-breaker.
- `Take` alone may map to `TOP` or `LIMIT` depending on provider.
- Large offsets on huge tables perform poorly — keyset pagination is the scalable alternative (See Q6).
- Parameterize skip/take values to avoid plan cache pollution when possible.

---

#### Q3. What is `.Chunk()` (modern LINQ), and how does it differ from manual batching loops?

**Answer:** `Chunk` ( .NET 6+) splits a sequence into consecutive batches of a specified size as `IEnumerable<T[]>` (or similar), handling the last partial chunk automatically. Manual loops track indices and slice arrays — more error-prone for remainder handling.

```csharp
foreach (var batch in items.Chunk(100))
{
    await ProcessBatchAsync(batch);
}
```

- Deferred — batches materialize as you iterate chunks.
- Last chunk may be smaller than chunk size — no special-case code needed.
- Useful for bulk database updates respecting parameter limits.
- EF Core does not translate `Chunk` on `IQueryable` — apply after materialization or in SQL with window functions separately.

---

#### Q4. What is the difference between `.TakeWhile`/`SkipWhile` vs `.Where` for conditional paging?

**Answer:** `TakeWhile` and `SkipWhile` depend on sequential order from the start — they stop or switch modes at the first boundary element. `Where` filters all elements matching a predicate regardless of position, which may include non-contiguous matches throughout the sequence.

- `TakeWhile(x => x.Status == Pending)` takes initial pending run only — stops at first non-pending.
- `Where(x => x.Status == Pending)` returns all pending items anywhere in the stream.
- Sorted inputs often pair with `TakeWhile`/`SkipWhile` for prefix/suffix splits.
- Choose based on whether you need contiguous segments or global filtering.

---

#### Q5. What are pitfalls of using `.Skip(n).Take(m)` without stable ordering in databases?

**Answer:** Without deterministic `OrderBy`, databases return rows in undefined order, so skipping rows on page two may repeat or omit records when data changes between requests. Paging APIs must define stable sort keys.

- Concurrent inserts and deletes shift offsets — users may see duplicates or misses even with sort.
- Unique tie-breaker (ID) prevents arbitrary reorder among equal sort keys.
- Keyset pagination reduces sensitivity to row movement compared to large offsets.
- Never expose unordered paging in public APIs.

---

#### Q6. How does keyset (seek) pagination compare to offset pagination in LINQ/EF?

**Answer:** Offset pagination uses `Skip/Take` with page numbers — simple but slow for large offsets because the database must scan skipped rows. Keyset pagination queries `WHERE sortKey > @lastSeen ORDER BY sortKey TAKE n`, using indexes to seek directly to the next page.

- Offset: `Skip(pageSize * pageIndex).Take(pageSize)` — degrades as page index grows.
- Keyset: pass last row's sort key from previous page — constant-time next page with proper indexes.
- Keyset struggles with jumping to arbitrary page numbers — UX trade-off.
- LINQ expresses keyset with `Where` on composite cursor plus `OrderBy` and `Take`.

---

#### Q7. What happens when `Skip`/`Take` arguments are negative or larger than the sequence?

**Answer:** Negative `Skip` or `Take` throw `ArgumentOutOfRangeException`. `Take` larger than the sequence length returns all available elements; `Skip` larger than length yields an empty sequence.

- `Take(0)` returns empty — valid edge case for disabling pages.
- On infinite sequences, `Take(n)` still terminates after n elements.
- Validate page parameters at API boundaries before passing to LINQ.
- EF Core passes parameters to SQL — negative values should be rejected before query execution.

---

#### Q8. When does partitioning force full enumeration vs true streaming?

**Answer:** `Skip` on non-indexed forward-only sequences must walk and discard the first n elements — O(n) before yielding — while `Take` can stop early after n items. `TakeWhile`/`SkipWhile` stream until the predicate boundary; `Skip` combined with large n on cold enumerables is expensive.

- Indexed lists can optimize some operations via element access — rare in pure LINQ extensions.
- Database `Skip/Take` does not require fetching all rows to the client — server handles offset when translated.
- Avoid huge in-memory `Skip` on linked streams — keyset or database paging instead.
- `Chunk` streams batches without loading entire source if source itself streams.

---

#### Q9. How do partitioning operators interact with deferred execution?

**Answer:** Partitioning operators return deferred sequences describing slice logic — no elements move until enumeration. Chaining `Where`, `OrderBy`, `Skip`, and `Take` builds one composed query executed in a single pass when possible.

- Multiple enumerations re-run the entire pipeline including skip/take logic.
- EF Core folds operators into one SQL statement when translatable.
- Materialize after partitioning when serving a page snapshot to clients.
- Deferred partitioning over mutable lists sees list changes if not snapshotted.

---

#### Q10. How would you batch-process a large `IEnumerable<T>` using `.Chunk()` for database updates?

**Answer:** Materialize or stream items, iterate `Chunk(batchSize)`, and issue one update per batch inside a transaction scope, passing batch arrays to bulk APIs or parameterized commands respecting database limits.

```csharp
foreach (var batch in pendingUpdates.Chunk(500))
    await repository.BulkUpdateAsync(batch, cancellationToken);
```

- Choose batch size below parameter limits and lock escalation thresholds.
- Handle partial failures with per-batch retry or compensating transactions.
- `Chunk` after `ToListAsync` when source is EF query — do not chunk unmaterialized cross-context queryables.
- Monitor memory — chunking a huge list still loaded entirely unless source streams.

---

#### Q11. What is `.TakeLast()` / `.SkipLast()`, and how do they differ from reversing then taking?

**Answer:** `TakeLast(n)` and `SkipLast(n)` (.NET Core 2.0+) yield the final n elements or all but the final n without requiring you to reverse sort order semantically. `Reverse().Take(n)` reverses the entire sequence's enumeration order, which may differ from "last by original order" on infinite or partially ordered data.

- `TakeLast` buffers up to n elements on forward-only sources — may require memory proportional to n.
- On indexed collections, implementations can optimize random access from the end.
- Prefer `TakeLast`/`SkipLast` for tail segments of finite sequences in original order.
- EF Core translation support for these operators is limited — often client-side.

---

### 12. Generation Operations

#### Q1. What are `Enumerable.Range`, `Repeat`, and `Empty` used for?

**Answer:** `Range` generates a sequence of consecutive integers; `Repeat` generates the same value a specified number of times; `Empty` returns a zero-length typed sequence. They create synthetic inputs for tests, indexing, padding, and default empty sources without allocating backing stores for full arrays upfront in deferred scenarios.

- `Enumerable.Range(1, 10)` yields 1 through 10 — end-exclusive count parameter.
- `Enumerable.Repeat("x", 5)` yields five `"x"` strings.
- `Enumerable.Empty<int>()` returns typed empty sequence — cleaner than `new int[0]` in generic contexts.
- Combine with `Select` for synthetic keys or default-filled grids.

---

#### Q2. What is the difference between `Enumerable.Repeat` and repeating elements in a collection?

**Answer:** `Enumerable.Repeat` generates a deferred sequence of repeated values without creating a physical list until consumed, while `new List<T>(Enumerable.Repeat(x, n))` or loops materialize storage. Repeat with zero count yields empty sequence without allocation of elements.

- Repeat does not clone reference types — repeated references point to the same object instance.
- Mutating a repeated reference object affects all logical slots conceptually sharing that instance.
- For independent object instances, use `Select` with a factory: `Enumerable.Range(0, n).Select(_ => new Item())`.
- Collection constructors eagerly allocate when you materialize Repeat output.

---

#### Q3. How do generation methods behave with deferred execution?

**Answer:** `Range`, `Repeat`, and `Empty` return deferred `IEnumerable<T>` implementations that yield elements on demand during enumeration — they do not pre-fill arrays except for internal iterator state tracking current index.

- No work occurs until `foreach` or terminal operators run.
- Chaining `Where`/`Select` on `Range` stays deferred as one pipeline.
- Infinite conceptual sequences are not provided by `Range`/`Repeat` — counts must be finite.
- Custom `yield return` iterators follow the same lazy model.

---

#### Q4. When is `Enumerable.Empty<T>()` preferable to `Array.Empty<T>()` or `new List<T>()`?

**Answer:** `Enumerable.Empty<T>()` returns a cached singleton empty sequence for each type in deferred LINQ pipelines where you need `IEnumerable<T>` without mutation. `Array.Empty<T>()` is ideal when you need a zero-length array instance; `new List<T>()` allocates mutable list capacity unnecessarily for empty cases.

- LINQ operators expecting sequences accept `Empty` without array allocation.
- `Array.Empty<T>()` is best for APIs requiring arrays specifically — also cached singleton.
- Avoid `new List<T>()` when immutability and zero allocation matter for empty returns.
- Choose based on return type contract of your method.

---

#### Q5. How do you generate sequences lazily without preallocating large arrays?

**Answer:** Use iterator blocks with `yield return`, `Enumerable.Range`, or chained operators that compute elements on demand rather than constructing massive arrays or lists upfront.

```csharp
IEnumerable<int> Squares() {
    for (int i = 0; ; i++) yield return i * i; // unbounded — consume with Take
}
```

- Pair unbounded generators with `Take` to bound work.
- Lazy generation keeps memory O(1) in the generator state regardless of conceptual sequence length.
- EF queries are a different kind of lazy — server-side streaming with IDataReader.
- Avoid `new int[hugeCount]` when `Range` plus pipeline expresses the same intent deferred until needed.

---

#### Q6. What are pitfalls of `Range` with large counts (memory, overflow)?

**Answer:** `Range` itself is deferred and does not allocate a huge array, but consuming it with operators that materialize everything (`ToList`, `Count` on infinite misuse) still processes every element. The start plus count can overflow `int` arithmetic if parameters are near `int.MaxValue`.

- `Range(0, int.MaxValue)` is valid but enumerating entirely is impractical.
- Terminal ops on enormous ranges take proportional time and memory when materialized.
- Validate count parameters from user input before passing to `Range` or `Repeat`.
- Overflow in `start + count - 1` is a corner case for extreme arguments.

---

#### Q7. How do generation operators combine with `.Select` to produce synthetic keys or indexes?

**Answer:** Zip `Range` with another sequence or use indexed `Select` to attach row numbers, grid coordinates, or test fixtures — generation supplies the index stream, `Select` maps to shaped objects.

```csharp
var numbered = Enumerable.Range(1, items.Count).Zip(items, (i, item) => new { i, item });
```

- Useful in tests generating predictable IDs without hard-coded arrays.
- Combine `Repeat` with `Select((_, i) => ...)` when count comes from elsewhere.
- EF may not translate generated ranges — typically in-memory for reporting prep.
- Keep generated keys stable when reproducing bugs tied to sequence order.

---

#### Q8. When would you use `yield return` in custom iterators vs `Enumerable.Range`?

**Answer:** Use `yield return` when generation logic is custom — parsing streams, tree walks, or stateful machines — not simple arithmetic progressions. Use `Range` for straightforward integer sequences where no custom state is needed.

- `yield return` integrates try/finally for resource cleanup in iterators.
- Async streams use `IAsyncEnumerable` with `yield return` and `await` — different async pattern.
- `Range` is clearer for `0..n-1` index patterns — less code than manual loops.
- Compiler generates iterator state machines for both idioms similarly under the hood.

---

#### Q9. How do infinite or unbounded sequences interact with operators like `.Count()` or `.Take()`?

**Answer:** `Take(n)` on an infinite sequence completes after n elements — safe boundary. `Count()` without bound on an infinite sequence never completes — it hangs attempting to enumerate forever.

- Always cap unbounded generators with `Take` or conditional breaks before terminals.
- `Any` may complete quickly if predicate matches early even on conceptually infinite sources.
- `First` completes after first element — safe if it exists.
- Document infinite sequences clearly — they are advanced scenarios, not database queries.

---

#### Q10. What is the difference between generating sequences in LINQ vs using `Random` or GUID factories in projections?

**Answer:** LINQ generation operators (`Range`, `Repeat`) produce deterministic, repeatable sequences suitable for tests and indexing. `Random` or `Guid.NewGuid()` in projections introduces nondeterminism — different values each enumeration if re-run, which breaks deferred execution expectations and unit tests.

- Materialize random sequences once if you need stable snapshots.
- `Random.Shared` is thread-safe for parallel scenarios — unlike legacy shared `Random` instances.
- GUID per row in EF projection translates only if provider supports it — often client-side.
- Use deterministic seeds in tests; use random generators deliberately at system boundaries, not inside re-enumerable LINQ pipelines without care.

---

### 13. LINQ to XML

#### Q1. What is the difference between LINQ to XML (`XDocument`, `XElement`) and XML serialization (`XmlSerializer`, `DataContractSerializer`)?

**Answer:** LINQ to XML provides a mutable in-memory XML tree (`XDocument`, `XElement`) you query and transform with LINQ operators, while XML serializers map CLR objects to XML and back using attributes and conventions on types. LINQ to XML suits ad-hoc parsing, shaping, and functional transforms; serializers suit stable object-graph persistence and wire formats.

- LINQ to XML is document-centric — navigate, create, and mutate nodes directly.
- `XmlSerializer` generates XML from public properties and fields on known types.
- `DataContractSerializer` uses data contract attributes for versioning and known types.
- Choose LINQ to XML for reporting pipelines and one-off transforms; serializers for API contracts and configuration files with fixed schemas.

---

#### Q2. How do you load, create, and mutate XML with `XDocument` and `XElement`?

**Answer:** Load with `XDocument.Load(path)` or `XDocument.Parse(string)`; create with object initializers (`new XElement("root", new XElement("child", "text"))`); mutate by calling `Add`, `Remove`, `SetAttributeValue`, and `SetElementValue` on elements. `XDocument` represents the whole document including declaration; `XElement` can stand alone as a subtree root.

- Functional construction builds trees without DOM factory boilerplate.
- Mutations are in-memory until you `Save` to disk or stream.
- `XDocument.Root` returns the root element for queries starting at document top.
- Namespace-aware names use `XNamespace` plus local name concatenation.

---

#### Q3. How do you query XML with LINQ (`Descendants`, `Elements`, `Attributes`, `XPath` extensions)?

**Answer:** LINQ to XML exposes axes as methods returning `IEnumerable<XElement>` or `XAttribute` — `Elements()` for direct children, `Descendants()` for all nested matches, `Attributes()` for attributes on a node. Optional `System.Xml.XPath` extensions add XPath selection when team standards prefer XPath strings.

```csharp
doc.Descendants("Order").Where(o => (decimal)o.Attribute("Total") > 100);
```

- Queries compose with standard LINQ `Where`, `Select`, and `GroupBy`.
- Attributes coerce to strings and numeric types with casts carefully — null attributes throw.
- XPath extensions bridge teams migrating XPath-heavy legacy code.
- Queries run in memory on loaded trees — not streaming XPath over unread files unless using `XPathNavigator`.

---

#### Q4. What is the difference between `Elements()` and `Descendants()`?

**Answer:** `Elements()` returns only direct child elements of the current node; `Descendants()` returns all descendant elements at every depth below the node, including nested grandchildren. Both can filter by optional `XName`.

- `root.Elements("Item")` — immediate children named Item only.
- `root.Descendants("Item")` — every Item anywhere under root.
- `DescendantsAndSelf` includes the current node if it matches the name filter.
- Choosing the wrong axis includes or excludes nodes silently — common bug in XML reports.

---

#### Q5. How do you project XML into CLR objects manually vs using deserialization?

**Answer:** Manual projection uses LINQ `Select` to map `XElement` fields to constructors or property setters, giving full control over shape mismatches and partial documents. Deserialization delegates mapping rules to serializer attributes and requires types matching the XML schema closely.

- Manual: `orders.Select(x => new Order((int)x.Element("Id"), (string)x.Element("Customer")))`.
- Serializer: `XmlSerializer(typeof(Order)).Deserialize(stream)` — less code when schema matches types.
- Manual handles irregular XML and computed fields; serializers struggle with schema drift without attributes updates.
- Combine load with LINQ projection for ETL pipelines; serializers for configuration binding.

---

#### Q6. What is the difference between `XElement` and `XAttribute` in queries?

**Answer:** `XElement` represents nested XML nodes forming tree structure; `XAttribute` represents name-value pairs on a node without nesting children. LINQ treats them as different types — attributes accessed via `Attributes()`, `Attribute(name)`, or implicit conversion in projections.

- Elements can contain child elements and text nodes; attributes cannot nest structure.
- Duplicate attribute names on one element are invalid XML — one value per name.
- Missing `Attribute` returns null — cast carefully to avoid `NullReferenceException`.
- Query syntax can join elements and attributes in the same projection anonymously.

---

#### Q7. How do namespaces affect LINQ to XML queries (`XNamespace`, `XName.Get`)?

**Answer:** XML namespaces require qualifying element and attribute names — compare using `XNamespace` plus local name, not bare strings, or queries return empty results even when visually "same" names appear in the file.

```csharp
XNamespace ns = "http://example.com/schema";
doc.Descendants(ns + "Order");
```

- `XName.Get("{uri}local")` fully qualifies names explicitly.
- Default namespaces in documents still apply — unprefixed names in C# must include namespace URI.
- Namespace ignorance is the top reason LINQ to XML queries return nothing.
- Attributes without prefixes may be in empty namespace even when elements are namespaced.

---

#### Q8. When should you use `XmlReader` streaming vs LINQ to XML DOM-style loading?

**Answer:** Use `XmlReader` for forward-only streaming of very large files where holding the full tree in memory is prohibitive. Use LINQ to XML when documents fit comfortably in RAM and you need random access, mutation, and composable LINQ transforms.

- DOM load parses entire document — memory proportional to file size.
- Streaming processes one node at a time — constant memory, harder random queries.
- Hybrid: load subtrees with LINQ after locating offsets via reader for very large files.
- Choose streaming for log ingestion pipelines; LINQ to XML for configuration and moderate message transforms.

---

#### Q9. How do you handle malformed XML and exceptions in LINQ to XML pipelines?

**Answer:** Malformed XML throws `XmlException` during `Load` or `Parse` — wrap parsing boundaries with try/catch and validate inputs before production pipelines. Schema validation optionally uses `XmlSchemaSet` before or after load depending on strategy.

- Fail fast at ingress with clear error messages and logging of source identifiers.
- Do not catch and swallow parse errors without metrics — poison messages need dead-letter handling.
- Partial recovery is rarely safe — reject malformed documents unless business rules define truncation.
- Sanitize external XML sources to mitigate billion-laughs and XXE risks when configuring readers.

---

#### Q10. What are performance and memory considerations for large XML documents with LINQ to XML?

**Answer:** LINQ to XML keeps the entire parse tree in memory with object overhead per node — large documents cause high RAM use and garbage collection pressure. Repeated queries over the same loaded document are fast; reloading repeatedly is expensive.

- Prefer streaming for multi-gigabyte feeds.
- Project early with LINQ to drop unneeded branches conceptually — though DOM still holds full tree until discarded.
- Avoid repeated string concatenation in transformations — use functional reconstruction of needed subtrees only when mutating.
- Dispose documents by dropping references promptly in batch jobs processing many files sequentially.

---

#### Q11. How do you transform XML shape with functional-style projections?

**Answer:** Project existing elements into new `XElement` trees with `Select`, optionally changing hierarchy, renaming nodes, and filtering attributes — pure functional transforms without imperative DOM walking when possible.

```csharp
var transformed = source.Descendants("Order")
    .Select(o => new XElement("Invoice",
        new XAttribute("id", (string)o.Attribute("id")),
        new XElement("Amount", (decimal)o.Element("Total"))));
```

- Compose transforms as LINQ pipelines readable top-to-bottom.
- `SelectMany` flattens nested repeats when one source node becomes many target nodes.
- Immutability pattern: build new trees rather than mutating shared documents when producing outputs for multiple consumers.
- Validate output against downstream XSD or partner contracts after transform.

---

#### Q12. What is the difference between `XDocument.Save` formatting options and writer-based output?

**Answer:** `XDocument.Save` writes the in-memory tree to a path or stream with optional `SaveOptions` (`None`, `DisableFormatting`, `OmitDuplicateNamespaces`) controlling indentation and namespace duplication. `XmlWriter` offers finer control over prefixes, async writing, and streaming output settings when documents are large or written incrementally.

- `SaveOptions.DisableFormatting` minimizes whitespace for wire transmission.
- `XmlWriter` settings manage encoding, indentation, and conformance checks explicitly.
- Writer-based output can stream without building full document first if you generate sequentially.
- Choose Save for simplicity; writers for performance tuning and custom namespace prefix behavior.

---

### Gotchas — Module 05

#### Gotcha 1. Multiple enumeration

**Answer:** Deferred LINQ queries re-execute the entire pipeline every time you `foreach`, call `Count`, or otherwise enumerate again, which is dangerous when the source is a database context, open file stream, or random number generator.

- Materialize once with `ToList` or `ToArray` when you need multiple passes over the same snapshot.
- EF Core queries against a disposed `DbContext` fail on second enumeration — fetch within scope.
- Random or time-dependent predicates produce different results on each run — not a stable report.
- Treat deferred queries as single-use recipes unless the source is a cheap in-memory collection.

---

#### Gotcha 2. `.ToList()` too early with EF Core

**Answer:** Calling `ToList()` before filters and projections executes the query prematurely and can load entire tables into memory, defeating server-side SQL optimization.

- Keep filters on `IQueryable` until the last responsible moment before materialization.
- Accidental early materialization often comes from helper methods that return `List<T>` internally.
- Log row counts when introducing new queries during code review.
- Switch to client evaluation only deliberately when translation is impossible.

---

#### Gotcha 3. Unstable paging

**Answer:** `Skip` and `Take` without a deterministic `OrderBy` produce nondeterministic pages in SQL because relational databases do not guarantee row order without an explicit sort clause.

- Always include sort keys and a unique tie-breaker such as primary key in paged APIs.
- Concurrent data changes shift offset pages — expect duplicates or gaps unless using keyset pagination.
- UI page numbers built on unstable sorts confuse users with jumping records between refreshes.
- Treat stable paging as a requirement, not an optimization.

---

#### Gotcha 4. Double `OrderBy`

**Answer:** A second `OrderBy` replaces the primary sort instead of adding a secondary key — developers expect multi-column sorts but only the last `OrderBy` applies unless they chain `ThenBy`.

- Use `OrderBy(...).ThenBy(...)` for composite sorting in method syntax.
- Query syntax comma-separated `orderby` keys compile to `ThenBy` correctly when written properly.
- Mis-sorted reports and exports are a common symptom of this mistake.
- See Ordering Q1 for the correct chaining pattern.

---

#### Gotcha 5. `.Single()` vs `.First()`

**Answer:** `Single` throws when zero or more than one element matches, while `First` returns the first match and only throws on empty sequences — using `Single` on filtered production data is fragile when duplicates exist.

- Use `Single` to enforce uniqueness invariants during development or when duplicates indicate data corruption.
- Prefer `First` with explicit ordering when you need one representative row among possible duplicates.
- EF queries returning unexpected duplicates cause runtime exceptions at API boundaries.
- Document which semantics your repository methods guarantee.

---

#### Gotcha 6. Closure over loop variable in LINQ

**Answer:** Lambdas inside loops capture variables by reference, not by iteration value — building predicates like `.Where(x => x.Id == ids[i])` inside a loop often captures the wrong index or final loop value unless copied to a local inside each iteration.

- Fix with `var id = ids[i];` inside the loop before using `id` in the lambda.
- Same pitfall affects `Task.Run` and async lambdas — not unique to LINQ but common in dynamic query builders.
- Dynamic query builders in loops are a frequent source of subtle production bugs.
- Capture intentional values into locals before closing over them in delegates.

---

#### Gotcha 7. `.Count()` cost

**Answer:** `Count()` is O(1) on `ICollection<T>` via the `.Count` property, but O(n) on deferred sequences that must be fully enumerated — assuming Count is cheap on every IEnumerable causes performance surprises.

- Check whether the source is already materialized before calling `Count()` repeatedly.
- Prefer `Any()` over `Count() > 0` when testing existence on sequences.
- Database `COUNT(*)` via EF is server-side — different from in-memory enumeration cost.
- Profile hot paths that call `Count()` inside loops on lazy pipelines.

---

#### Gotcha 8. Set operators and comparers

**Answer:** Without an explicit `IEqualityComparer`, reference types dedupe by reference identity, not by field values — `Distinct`, `Union`, and `Except` may leave visually "duplicate" objects in results.

- Pass comparers for string case rules or domain-specific equality.
- Value types and records with proper equality behave intuitively without custom comparers.
- EF Core set operations use database equality semantics on projected columns — not CLR reference identity.
- Unit tests should assert deduplication behavior with realistic comparers.

---

#### Gotcha 9. `GroupBy` vs `ToLookup` timing

**Answer:** `GroupBy` is deferred and re-enumerates the source when you iterate groups, while `ToLookup` executes immediately and returns an immutable multi-map safe for repeated key lookups.

- Use `ToLookup` when you need random access by key multiple times without re-walking the source.
- Use `GroupBy` for one-pass reporting pipelines over cheap in-memory sequences.
- Treating deferred `GroupBy` like a dictionary causes repeated database queries if enumerated multiple times.
- Materialize explicitly when lifetime and execution count are ambiguous.

---

#### Gotcha 10. Client evaluation surprises

**Answer:** Custom CLR methods in `Where` or `Select` may force EF Core to pull data client-side or fail translation in strict modes, silently changing performance characteristics or throwing at runtime.

- Keep translatable expressions on `IQueryable` until intentional switch with `AsEnumerable`.
- Enable SQL logging to detect client evaluation during development.
- Wrap non-translatable logic in methods only after materializing minimal data.
- Strict translation failures are preferable to silent full-table loads — configure and test accordingly.

---

## Module 06. Multithreading & Async Programming

### 01. Threads & Thread Lifecycle

#### Q1. Explain multithreading in C# and when it is appropriate vs async I/O or tasks.

**Answer:** Multithreading runs multiple threads of execution concurrently within one process, allowing parallel CPU work and overlapping operations. Use dedicated threads or parallel tasks for CPU-bound parallelism; prefer async I/O and `Task`-based APIs for I/O-bound work that spends time waiting, because blocking threads during waits wastes pool capacity.

- CPU-bound examples: image processing, compression, parallel aggregation over in-memory data — `Task.Run` or `Parallel.ForEach` on pool threads.
- I/O-bound examples: HTTP calls, file reads, database queries — `async`/`await` frees threads during waits.
- Raw `Thread` creation is rare in app code — thread pool and TPL cover most scenarios.
- Mixing models incorrectly (`Task.Run` around blocking I/O) steals pool threads without improving scalability.

---

#### Q2. What is a `Thread`, and how do you create and start one?

**Answer:** A `Thread` represents an operating-system thread managed by the Common Language Runtime (CLR), and you create one with `new Thread(startMethod)` or lambda delegates, then call `Start()` to schedule execution. The delegate can be `ThreadStart` (void) or `ParameterizedThreadStart` (object state).

```csharp
var thread = new Thread(() => DoWork());
thread.Start();
```

- Threads begin unstarted until `Start` is called once — calling `Start` twice throws.
- Prefer `Task.Run` for short pool-backed work instead of manual `Thread` unless you need explicit foreground/background or custom thread properties.
- Thread creation is heavier than queueing work to the pool — OS resources and stack allocation apply.
- Name threads for diagnostics with `thread.Name` when debugging multithreaded apps.

---

#### Q3. What are foreground vs background threads, and how do they affect process shutdown?

**Answer:** Foreground threads keep the process alive until they finish; background threads do not — the runtime stops them abruptly when all foreground threads end and the process exits. Set `IsBackground = true` for auxiliary work that should not block application shutdown.

- Default new threads are foreground unless configured otherwise.
- Background thread termination is nondeterministic at shutdown — no guaranteed finally blocks run on forced exit.
- Long-running services rely on graceful shutdown signaling (`CancellationToken`) rather than background alone.
- Thread pool threads are background threads.

---

#### Q4. What are the main thread states in the lifecycle (unstarted, running, wait/sleep/join, stopped)?

**Answer:** A CLR thread moves from unstarted after construction to running when started, enters wait or sleep when blocked on locks, joins, or `Thread.Sleep`, and becomes stopped when the delegate completes. `Thread.Join` blocks the caller until the target thread finishes.

- `ThreadState` flags are bitwise — combinations like `WaitSleepJoin` appear in diagnostics.
- Running does not mean executing on CPU continuously — blocked threads are not running user code.
- You cannot restart a stopped thread — create a new `Thread` instance for new work.
- Pool threads are reused across many work items with different observable lifetimes.

---

#### Q5. What is `Thread.Join()`, and what happens if you never join a foreground thread?

**Answer:** `Join` blocks the calling thread until the target thread completes, providing a synchronization point to observe completion. If a foreground worker thread is still running and never joined, the process remains alive until that thread finishes naturally even if main exits its entry point.

- Join with timeout overloads avoid indefinite blocking when workers may hang.
- Not joining background threads is acceptable for fire-and-forget auxiliary work if exceptions are handled inside the worker.
- Prefer `Task` and `await` over manual `Join` for composable completion in modern code.
- Failing to join foreground threads prevents clean process exit in console apps.

---

#### Q6. What is `Thread.Sleep()` vs spinning vs waiting — when is each appropriate?

**Answer:** `Thread.Sleep` yields the thread for a fixed time without consuming CPU, spinning repeatedly checks a condition burning CPU cycles for ultra-low latency, and waiting on synchronization objects (`Monitor.Wait`, events) blocks efficiently until signaled. Use sleep for coarse delays, waiting for coordination, and spinning only for very short expected waits on multiprocessors.

- Sleep is inappropriate in async methods — use `await Task.Delay` instead (See Async Q17).
- Spinning (`SpinWait`) suits lock-free algorithms with brief contention windows.
- Blocking waits release the thread to the scheduler — better than sleep-polling for event-driven work.
- Excessive sleep-based polling wastes threads and adds latency.

---

#### Q7. What is thread affinity, and why does it matter for UI applications?

**Answer:** Thread affinity means certain operations must run on a specific thread — UI frameworks require control updates on the UI thread that owns the window handle. Background threads must marshal UI updates through the synchronization context or dispatcher, not touch controls directly.

- WinForms and WPF associate controls with the thread that created them.
- `async`/`await` captures synchronization context by default and posts continuations back to the UI thread.
- Direct cross-thread UI access throws cross-thread operation exceptions or causes subtle corruption.
- Library code uses `ConfigureAwait(false)` to avoid capturing UI context unnecessarily.

---

#### Q8. What is the difference between creating a raw `Thread` and using thread pool threads?

**Answer:** Raw threads are explicit OS threads you create and start manually with full control over foreground status, priority, and lifetime. Thread pool threads are managed worker threads reused across many short work items queued by the runtime, amortizing creation cost and enforcing scalable concurrency limits.

- Pool threads suit bursty, short tasks — default for `Task.Run`, ASP.NET Core requests, and timer callbacks.
- Dedicated threads suit long-running loops, COM single-threaded apartment requirements, or isolation from pool starvation.
- Creating many raw threads can exhaust OS thread limits and memory for stacks.
- The pool implements hill-climbing to adjust worker count based on throughput (See ThreadPool Q3).

---

#### Q9. What are `Thread.Name`, `IsBackground`, `Priority` — which actually affect scheduling?

**Answer:** `Name` aids debugging and profiler traces but does not change scheduling. `IsBackground` determines whether the thread keeps the process alive. `Priority` hints OS scheduling priority but modern systems often ignore extreme priorities and starvation avoidance dominates — do not rely on priority for correctness.

- Set meaningful names before starting threads for dump and trace readability.
- Background flag is the reliable behavioral switch for shutdown semantics.
- Priority changes can starve other work unpredictably — use sparingly.
- Correctness requires synchronization primitives, not priority tweaks.

---

#### Q10. What is a race condition at the thread level, and how can two threads interleave unpredictably?

**Answer:** A race condition occurs when multiple threads access shared mutable state concurrently and the outcome depends on interleaving order the program does not control. Two threads updating the same variable without synchronization can lose updates or observe torn reads because instructions interleave at arbitrary points.

- `counter++` is not atomic — read, increment, and write can interleave between threads.
- Even simple checks like `if (map.ContainsKey(k)) map[k] = v` race between threads.
- Reproducing races is hard — they appear intermittently under load.
- Fix with locks, concurrent collections, or interlocked operations on narrow fields.

---

#### Q11. What is the difference between kernel threads and managed threads (conceptual model)?

**Answer:** Kernel threads are operating-system scheduling units; CLR managed threads wrap kernel threads (typically one-to-one on modern Windows and Linux) with runtime services for garbage collection hooks, exception propagation, and `Thread` API abstractions. Your C# code targets managed threads, not raw OS thread APIs directly.

- The runtime cooperates with GC safepoints on managed threads during collections.
- Thread pool maps work items to managed pool threads backed by kernel threads.
- Logical concurrency can exceed CPU cores — OS time-slices preemptive threads.
- Async I/O completion may run continuations on pool threads without one dedicated thread per operation.

---

#### Q12. Why is manually creating many threads often a scalability anti-pattern?

**Answer:** Each thread consumes kernel and user-mode resources (stack memory, scheduling structures), and context switching overhead grows with thread count. Thousands of blocked threads waiting on I/O waste memory compared to async models that reuse a small pool of threads for many concurrent operations.

- OS schedulers degrade with excessive runnable threads — thrashing and cache misses increase.
- Thread pool queues work with controlled concurrency instead of unbounded thread explosion.
- Server apps under load with per-request raw threads risk hitting thread limits.
- Scale I/O with async; scale CPU with bounded parallelism (`ParallelOptions`, partition counts).

---

#### Q13. What is `ThreadStatic`, and how does it differ from `ThreadLocal<T>`?

**Answer:** `ThreadStatic` marks static fields so each thread gets its own storage slot automatically, but initialization and generic typing are awkward. `ThreadLocal<T>` provides lazy per-thread instances with cleaner APIs and supports value factories for initialization.

- `[ThreadStatic] static int _counter;` — default zero; instance ctor does not run per thread.
- `ThreadLocal<T>` supports `Value` property and `Dispose` to clean up thread-specific resources.
- Async flows that hop threads break naive thread-static assumptions — logical call context may differ from physical thread.
- Prefer `AsyncLocal<T>` for values that follow async execution flows across thread hops.

---

#### Q14. What exceptions can occur when aborting or interrupting threads (historical vs modern guidance)?

**Answer:** `Thread.Abort` was deprecated and removed in modern .NET because it injected asynchronous exceptions unpredictably and corrupted invariants. Modern guidance uses cooperative cancellation with `CancellationToken` rather than forcibly aborting threads.

- `Thread.Interrupt` wakes sleeping threads with `ThreadInterruptedException` — rarely used in application code.
- Cooperative cancellation checks tokens in loops and exits cleanly with known state.
- Forcible termination of raw threads is an OS-level last resort outside normal managed patterns.
- `Task` cancellation registers callbacks on tokens — preferred model (See Tasks Q13).

---

#### Q15. How does the main thread exiting affect background work still running?

**Answer:** When all foreground threads complete, the process begins shutdown and terminates background threads without waiting for graceful completion — pending finally blocks and cleanup may not run. Background work must finish quickly or use cooperative shutdown signaling before main exits.

- Console apps exiting main while background workers run lose those workers mid-operation.
- Hosts like ASP.NET Core and Windows Services coordinate shutdown with `IHostApplicationLifetime` and cancellation tokens.
- Do not rely on background threads for critical flush operations without explicit join or await.
- Long-running services register shutdown handlers to drain queues before exit.

---

### 02. ThreadPool

#### Q1. What is the thread pool in .NET, and why is it preferred over creating raw threads?

**Answer:** The .NET thread pool maintains a set of worker threads that execute queued work items, reusing threads to avoid creation cost and applying global concurrency management. It is preferred because most application work consists of short tasks where pool reuse and hill-climbing tuning outperform manual thread proliferation.

- `Task.Run`, ASP.NET Core request handling, and timer callbacks use pool threads by default.
- The pool limits unbounded thread growth that would crash or thrash the OS under load.
- Work items queue when all workers are busy — latency rises instead of thread count exploding instantly.
- Dedicated threads remain appropriate for isolated long-running blocking loops outside pool semantics.

---

#### Q2. How does the thread pool manage worker threads and I/O completion threads?

**Answer:** Worker threads execute user delegates queued via `ThreadPool.QueueUserWorkItem`, `Task.Run`, and similar APIs. Separate I/O completion port threads handle asynchronous I/O completions and callback dispatch, keeping I/O completion off pure worker threads when possible — though async continuations often still run on workers.

- CPU-bound work competes for worker threads — blocking workers reduces throughput.
- I/O completion threads are fewer — misconfigured heavy callbacks on completion paths can starve I/O.
- `ThreadPool.SetMinThreads` influences how quickly the pool adds workers under sudden load.
- Async `await` continuations typically resume on thread pool workers unless a synchronization context captures them.

---

#### Q3. What is hill-climbing in the .NET thread pool (high level)?

**Answer:** Hill-climbing is the thread pool's adaptive algorithm that adjusts the number of worker threads based on measured throughput — adding threads when throughput increases with concurrency and backing off when adding threads does not help or hurts due to contention.

- The algorithm responds to queue depth and completion rates rather than fixed thread formulas.
- Sudden load spikes may briefly under-thread until hill-climbing adds workers — `SetMinThreads` can reduce ramp-up delay.
- Too many threads increase context switching without improving throughput — hill-climbing tries to find a plateau.
- CPU-bound contention on locks reduces effective parallelism — more threads do not always help.

---

#### Q4. What is `ThreadPool.QueueUserWorkItem`, and how does it relate to `Task.Run`?

**Answer:** `ThreadPool.QueueUserWorkItem` queues a delegate to the pool using the older callback API, while `Task.Run` wraps the same concept in a `Task` with status, exceptions, and composable continuations. Modern code prefers `Task.Run` for observability and integration with async patterns.

- Both schedule work on pool threads — similar underlying queue in many cases.
- `Task.Run` returns a `Task` you can await, chain, and exception-handle uniformly.
- `QueueUserWorkItem` remains for low-level scenarios not needing `Task` surface area.
- Neither should wrap async I/O that has true async APIs — use `await` instead.

---

#### Q5. What is starvation in the thread pool, and what causes it?

**Answer:** Thread pool starvation occurs when all worker threads are blocked waiting on work that itself requires pool threads to complete — for example synchronous `.Wait()` on tasks inside request handlers, or blocking I/O on every worker — so queued work waits indefinitely despite pending CPU capacity elsewhere.

- Classic ASP.NET deadlock: block on async work that needs a thread to unblock you on the same pool.
- Long synchronous I/O on pool threads prevents processing other queued items.
- Mitigate with async all the way, increase min threads only as temporary relief, or isolate blocking work.
- Monitor queue length and thread counts under load to detect starvation early.

---

#### Q6. How do synchronous blocking calls inside pool threads affect throughput?

**Answer:** Blocking calls hold a worker thread for the entire wait duration even when no CPU work occurs, reducing the number of threads available to process other queue items and shrinking effective server capacity. Under load, blocked workers cause queue growth and latency spikes.

- Replace blocking I/O with async APIs on hot paths.
- Offload unavoidable blocking to dedicated threads or `TaskCreationOptions.LongRunning` sparingly — not a default fix.
- Synchronous database and HTTP calls on ASP.NET Core thread pool threads limit requests per second dramatically.
- Profile wall-clock vs CPU time — high wait time on pool threads signals blocking problems.

---

#### Q7. What is the difference between dedicated threads and pool threads for long-running work?

**Answer:** Long-running work on pool threads occupies a worker for its entire duration, reducing pool capacity for short tasks. Dedicated threads (explicit `Thread` or `Task.Factory.StartNew` with `LongRunning`) isolate extended work so the pool is not depleted — use only when the work is genuinely long-lived and mostly CPU-active or blocking.

- `LongRunning` hints the scheduler to create a dedicated thread instead of pool reuse.
- Misusing `LongRunning` for everything defeats pool benefits.
- Background services and queue consumers often use hosted long-running tasks with cancellation.
- Async I/O long operations should not use extra threads at all — they release workers during waits.

---

#### Q8. What is `ThreadPool.SetMinThreads` / `SetMaxThreads`, and when might you tune them?

**Answer:** These APIs adjust the pool's minimum and maximum worker and I/O thread counts. Increase minimum threads when sudden bursts queue work while hill-climbing slowly adds threads — common mitigations for latency spikes at cold start or after idle periods — but tune using measurements, not guesses.

- `SetMinThreads` does not create all threads immediately — it lowers threshold for rapid injection.
- `SetMaxThreads` caps growth — rarely changed except in constrained environments.
- Over-increasing min threads raises baseline memory and context switching without guaranteed benefit.
- Fix async blocking and synchronization first — thread tuning is a secondary lever.

---

#### Q9. How does the thread pool interact with `async`/`await` continuations?

**Answer:** When an awaited `Task` completes, the runtime queues the async method continuation — often to the thread pool unless a synchronization context captures it (UI, legacy ASP.NET). I/O-bound awaits typically do not consume a thread during the wait, but continuations still need a worker briefly to resume.

- Default `await` on UI posts back to UI thread via synchronization context.
- `ConfigureAwait(false)` in libraries avoids unnecessary context captures and reduces deadlock risk.
- Thread pool throughput depends on continuations being short — long CPU work on continuation should offload explicitly.
- Async does not create one thread per operation — many concurrent awaits multiplex on few workers.

---

#### Q10. What is the danger of blocking the UI thread vs blocking a pool thread?

**Answer:** Blocking the UI thread freezes the interface and prevents message pumping, making applications unresponsive. Blocking pool threads reduces server throughput and can cause deadlocks when the blocked work waits on continuations that need the same blocked threads.

- UI: never `.Result` or `.Wait()` on UI thread for async work that resumes on UI context.
- Server: blocking pool threads caps concurrent request handling.
- Both are fixed by async I/O and offloading CPU work appropriately — different symptoms, same underlying mistake of blocking during waits.
- Use progress UI patterns with async event handlers instead of synchronous waits on UI.

---

#### Q11. How do thread pool threads relate to `Parallel.For` and PLINQ?

**Answer:** `Parallel.For` and PLINQ schedule partition work items on thread pool worker threads, using the same pool as `Task.Run` unless a custom `TaskScheduler` is supplied. Heavy parallel loops compete with other pool users for workers.

- `MaxDegreeOfParallelism` caps threads used by a parallel loop — important on shared servers.
- Nested parallel loops can multiply contention and oversubscribe CPU cores.
- PLINQ `AsParallel` defaults to pool-backed execution with partition-based stealing.
- Coordinate parallel CPU work with other pool demand in ASP.NET Core apps — avoid unbounded nested parallelism.

---

#### Q12. What diagnostics exist for thread pool queue length and thread counts (`ThreadPool.ThreadCount`, ETW)?

**Answer:** `ThreadPool.ThreadCount` and `PendingWorkItemCount` (.NET 6+) expose runtime metrics programmatically; Event Tracing for Windows (ETW) and tools like `dotnet-counters` monitor `System.Runtime` thread pool stats in production. Thread pool starvation events appear in diagnostics when work waits too long for workers.

- `dotnet-counters monitor System.Runtime` shows thread pool thread count and queue length live.
- Capture dumps under load to see blocked pool threads and lock contention stacks.
- Application Insights and OpenTelemetry can chart custom metrics from `ThreadPool` APIs.
- Correlate rising queue length with synchronous blocking code paths in profiles.

---

### 03. Tasks & Task Parallel Library

#### Q1. What is the Task Parallel Library (TPL)?

**Answer:** The Task Parallel Library is the .NET API surface centered on `Task` and `Task<T>` for representing asynchronous operations, scheduling work on the thread pool, and composing parallel and concurrent patterns. It underpins `async`/`await`, `Parallel.*`, and modern server programming models.

- Tasks represent operations that may complete now or later with status, result, and exception propagation.
- TPL adds structured parallelism beyond raw threads — continuations, cancellation, and coordination helpers.
- The compiler's async state machines build on `Task` as the primary awaitable type.
- TPL does not replace async I/O — it complements it for CPU parallelism and operation lifetime management.

---

#### Q2. Explain the difference between `Thread` and `Task` in purpose and scheduling.

**Answer:** A `Thread` is an OS-thread wrapper you create and manage explicitly, while a `Task` is a higher-level representation of work that the runtime schedules on thread pool threads (by default) with completion, cancellation, and exception semantics. Tasks are cheaper to create at scale and compose with `await`.

- One thread runs one delegate until completion unless interrupted cooperatively.
- Many tasks multiplex onto fewer pool threads over time, especially across I/O waits when async.
- Tasks expose `IsCompleted`, `Result`, `Exception`, and continuations — threads do not natively.
- Choose threads for special lifetime control; choose tasks for application concurrency patterns.

---

#### Q3. Explain `Task`, `Task<T>`, and `ValueTask<T>` — when to use each.

**Answer:** `Task` represents asynchronous work without a return value; `Task<T>` produces a result of type `T`; `ValueTask<T>` is a discriminated struct that can wrap a synchronous result, a pooled task, or an `IValueTaskSource` to reduce allocations in hot paths. Use `Task`/`Task<T>` by default; use `ValueTask<T>` when profiling shows allocation pressure and the API contract documents consumption rules.

- `Task<T>` is reference type — allocations matter in tight loops serving cached synchronous results.
- `ValueTask<T>` must be consumed once unless documented otherwise — no double await (See Gotcha 8).
- Public APIs usually return `Task`/`Task<T>` for simplicity unless optimized paths justify `ValueTask`.
- `ValueTask` without generic wraps void-like completion similarly to `Task`.

---

#### Q4. What is `Task.Run`, and when should it be used vs when it should be avoided?

**Answer:** `Task.Run` queues a delegate to the thread pool and returns a `Task` tracking that CPU-bound work. Use it to offload CPU-intensive computation from the UI or request thread; avoid it for I/O that has true async APIs, or for trivial work where queue overhead exceeds benefit.

- Good: parallelize CPU work after reading data on ASP.NET Core request thread.
- Bad: `Task.Run(() => stream.Read(...))` when `ReadAsync` exists — wastes pool threads.
- Bad: wrap already-async methods with `Task.Run` unnecessarily — adds thread hop without value.
- Prefer direct `await` on I/O-bound methods implementing async patterns.

---

#### Q5. What is `Task.Factory.StartNew`, and why is `Task.Run` usually preferred?

**Answer:** `Task.Factory.StartNew` is a lower-level scheduling API with many `TaskCreationOptions` and `TaskScheduler` parameters, defaulting to behaviors that surprise callers (custom schedulers, child task attachment). `Task.Run` is a simplified, safe default for queueing thread pool work with predictable detached task semantics.

- `StartNew` requires explicit options for common cases — easy to create attached children unintentionally.
- `Task.Run` always uses default scheduler and `DenyChildAttach` equivalent simplicity.
- Use `StartNew` only when you need custom schedulers or specialized creation flags knowingly.
- Modern guidance: default to `Task.Run`; reach for `StartNew` rarely with full option understanding.

---

#### Q6. Explain task continuations with `ContinueWith` — options, scheduling, and exception handling.

**Answer:** `ContinueWith` schedules a delegate to run when a predecessor task reaches a specified state — ran to completion, faulted, or canceled — with options for scheduling context and task attachment. Exceptions from the antecedent task propagate to the continuation task unless unwrapped manually; `await` is the preferred composition style today for clearer exception flow.

- `TaskContinuationOptions.OnlyOnFaulted` runs only when antecedent throws.
- `ExecuteSynchronously` runs inline if antecedent completes on the same thread — use carefully to avoid stack overflow or deadlocks.
- Continuation tasks fault separately if continuation delegate throws — nested exception handling complexity.
- Prefer `await antecedent` in async methods over `ContinueWith` for readability and uniform try/catch.

---

#### Q7. What is `Task.WhenAll`, `Task.WhenAny`, and how do they differ from manual continuation chaining?

**Answer:** `Task.WhenAll` returns a task that completes when all inputs complete (faulting with aggregated exceptions if any failed); `Task.WhenAny` completes when any input completes, returning the winning task. Both express batch coordination declaratively without manual continuation graphs.

- `await Task.WhenAll(t1, t2, t3)` waits for entire batch — common in parallel I/O fan-out.
- `await Task.WhenAny` implements timeout or first-responder patterns — combine with cancellation for timeouts.
- Manual `ContinueWith` chains grow complex with many tasks — `WhenAll`/`WhenAny` scale cleanly.
- `WhenAll` wraps multiple exceptions in `AggregateException` on synchronous `.Wait()` — `await` unwraps first or uses handling patterns.

---

#### Q8. What is `TaskCompletionSource<T>`, and what scenarios does it enable (bridging callbacks, manual completion)?

**Answer:** `TaskCompletionSource<T>` creates a `Task` whose completion you control externally — set result, exception, or canceled state from callback-based APIs, events, or other threads. It bridges legacy asynchronous patterns into the `Task` model awaitable by modern code.

- Wrap event-based APIs: complete the source when a `Completed` event fires.
- Implement manual async protocols where completion arrives from outside the initial delegate.
- Expose `Task` property to callers while internal code calls `TrySetResult`.
- One-shot completion — second `TrySet*` fails (See Gotcha 9).

---

#### Q9. What is the difference between completing a `TaskCompletionSource` with result, exception, or cancellation?

**Answer:** `TrySetResult` completes successfully with a value; `TrySetException` faults the task with supplied exceptions; `TrySetCanceled` transitions to canceled state optionally with a token. Awaiters observe these as normal task outcomes — return value, thrown exception, or `OperationCanceledException`.

- Only one outcome succeeds — first `TrySet*` wins; later calls return false.
- Faulted tasks throw the stored exception to awaiters — often `AggregateException` wrapping on multiple exceptions.
- Canceled tasks integrate with `CancellationToken` semantics and `ThrowIfCancellationRequested` patterns.
- Choose the outcome matching whether failure is exceptional, expected cancellation, or success.

---

#### Q10. What is `Task.FromResult`, `Task.CompletedTask`, and when are they preferable to `Task.Run`?

**Answer:** `Task.FromResult(value)` returns an already-completed `Task<T>` synchronously; `Task.CompletedTask` returns a completed void-equivalent task. Use them when work is purely synchronous and immediate — no thread hop — satisfying async method signatures without scheduling pool work.

- Cache completed tasks for repeated synchronous outcomes in hot paths.
- Async methods that sometimes hit fast paths avoid `Task.Run` allocation and scheduling overhead.
- Do not use `FromResult` to fake async I/O — callers assume completion semantics honestly.
- `ValueTask.FromResult` offers struct alternative for allocation-sensitive APIs.

---

#### Q11. What is the difference between `AggregateException` and a regular exception when tasks fail?

**Answer:** When multiple tasks fault or a task wraps multiple exceptions, `.Wait()` and `.Result` throw `AggregateException` containing an `InnerExceptions` collection. A single awaited task throws the first inner exception directly without requiring callers to unwrap `AggregateException` manually.

- `await task` unwraps one primary exception for ergonomics.
- Synchronous waits on parallel batches (`Task.WaitAll`) may surface `AggregateException` with several inner faults.
- `Flatten()` and `Handle()` process aggregate inner exceptions for logging and recovery policies.
- Continuation and `WhenAll` behavior differs between sync wait and async await — know your consumption pattern.

---

#### Q12. How do child tasks relate to parent tasks (`TaskCreationOptions`, attached vs detached)?

**Answer:** Attached child tasks (`AttachedToParent`) keep a parent task from completing until children finish — useful for structured parallel work trees. Detached children (default for `Task.Run`) complete independently; parent tasks do not wait for them unless explicitly coordinated with `WhenAll` or similar.

- Attached children nest lifetime — parent status reflects child failures when configured.
- Misuse of attachment causes surprising parent blocking or exception propagation.
- Modern code often prefers explicit `WhenAll` over attachment flags for clarity.
- `Task.Factory.StartNew` defaults historically differ from `Task.Run` — attachment surprises drove `Task.Run` preference.

---

#### Q13. What is task cancellation via `CancellationToken` registration vs `TrySetCanceled`?

**Answer:** `CancellationToken` provides cooperative cancellation — workers poll `ThrowIfCancellationRequested` or register callbacks fired when the source is canceled. `TrySetCanceled` on `TaskCompletionSource` marks the exposed task canceled directly, often linking token cancellation to task completion in bridging scenarios.

- Register `token.Register(() => tcs.TrySetCanceled())` to connect external cancellation to a TCS-backed task.
- Cooperative loops should check tokens frequently enough for responsive shutdown.
- Cancellation is not thread abort — workers must observe tokens voluntarily.
- Link tokens with `CreateLinkedTokenSource` when multiple cancel conditions apply (See Async Q15).

---

#### Q14. What are unobserved task exceptions, and how does .NET handle them?

**Answer:** If a `Task` faults and no code awaits it, accesses `.Exception`, or attaches a continuation observing failure, the exception becomes unobserved — finalized later it may raise `TaskScheduler.UnobservedTaskException`. Always await or explicitly handle task exceptions in fire-and-forget scenarios.

- `async void` event handlers rely on process-level unobserved handlers — fragile.
- Mark tasks observed by awaiting or `.ContinueWith` logging faults.
- .NET may swallow unobserved exceptions after the event in some configurations — do not depend on crashes for correctness.
- Use `Task.Run` with try/catch inside delegate when intentional background fire-and-forget is unavoidable.

---

#### Q15. What is `ValueTask` pooling/caching, and why must consumers avoid double-awaiting unless documented safe?

**Answer:** `ValueTask` can wrap pooled `IValueTaskSource` instances reset after consumption — awaiting consumes the source once. Double-awaiting or concurrent awaits on the same pooled `ValueTask` reads undefined state unless the API explicitly documents cacheable synchronous completion paths safe for multiple consumption.

- Default assumption: consume each `ValueTask` exactly once.
- Some optimized APIs return cached completed `ValueTask` singletons safe to await multiple times — documented exception.
- Library authors document consumption rules; callers follow them strictly.
- When in doubt, convert to `Task` with `AsTask()` before multiple awaits.

---

#### Q16. How do you implement a timeout around a `Task` using `CancellationTokenSource` or `WhenAny`?

**Answer:** Link a timeout token from `CancellationTokenSource.CancelAfter(ms)` into the operation's token so cooperative cancellation aborts work, or race the operation against `Task.Delay(timeout)` with `Task.WhenAny` and treat delay win as timeout if the operation lacks cancellation support.

```csharp
using var cts = CancellationTokenSource.CreateLinkedTokenSource(userToken);
cts.CancelAfter(TimeSpan.FromSeconds(30));
await operation(cts.Token);
```

- Cooperative cancellation is cleanest when the inner API honors tokens.
- `WhenAny` timeout without cancellation leaves orphan operations running — track and dispose carefully.
- Always dispose `CancellationTokenSource` instances.
- Combine linked tokens when both user cancel and timeout apply (See Async Q15).

---

### 04. Async and Await

#### Q1. Explain asynchronous programming in C# — what problem does it solve?

**Answer:** Asynchronous programming in C# lets threads serve other work while waiting on I/O or delays instead of blocking idle during network, disk, or database latency. The `async`/`await` model preserves sequential-looking code while achieving scalability on fewer threads than synchronous blocking designs.

- Synchronous blocking caps concurrent requests at roughly thread pool size under I/O load.
- Async releases the thread during awaits, improving throughput for web APIs and clients.
- Async does not automatically parallelize CPU — it optimizes waiting, not computation (See Q3).
- Composable `Task` chains integrate cancellation, exception handling, and timeouts uniformly.

---

#### Q2. Explain the `async` and `await` keywords in detail.

**Answer:** The `async` modifier on a method allows `await` inside it; the compiler rewrites the method into a state machine implementing `IAsyncStateMachine` that returns a `Task` or `Task<T>` immediately while work continues through continuations. `await` suspends the async method without blocking the current thread until the awaited operation completes, then resumes at the continuation point.

- Callers receive a `Task` representing ongoing work — await it to observe completion and results.
- Multiple awaits in one method compile to multiple state transitions with preserved locals.
- Exceptions thrown after awaits propagate to the returned task and rethrow on caller await.
- Async methods should use the `Async` suffix naming convention for discoverability.

---

#### Q3. What is the difference between CPU-bound and I/O-bound async work?

**Answer:** I/O-bound async work awaits operations that release threads during external waits — network, files, databases — using true async APIs. CPU-bound work computes on processors; offloading it with `Task.Run` uses threads during computation — `await` on CPU work does not magically parallelize unless you explicitly schedule it.

- I/O async: thread count stays modest under high concurrency of waiting operations.
- CPU async offload: `await Task.Run(() => Compute())` queues CPU work to pool — still uses a thread while computing.
- Misidentifying I/O as CPU leads to unnecessary thread hops and pool pressure.
- Server apps separate I/O pipelines from CPU-bound stages with explicit queues or `Parallel` where appropriate.

---

#### Q4. What is `ConfigureAwait(false)`, and when should library vs application code use it?

**Answer:** `ConfigureAwait(false)` tells the awaiter not to capture synchronization context for the continuation, allowing resume on any thread pool thread. Library code should use it to avoid deadlocks and unnecessary UI marshaling; application UI code usually omits it so continuations return to the UI thread for control updates.

- Default `await` captures UI or ASP.NET context when present.
- Library helpers doing no UI work call `ConfigureAwait(false)` on every await.
- ASP.NET Core has no legacy synchronization context — less critical but still good library hygiene.
- Do not use `ConfigureAwait(false)` as a performance trick in UI event handlers that touch controls after await.

---

#### Q5. How do you handle exceptions in async/await methods?

**Answer:** Exceptions thrown in async methods are stored on the returned `Task` and rethrown on the awaiting caller's thread when awaited. Use try/catch around `await` expressions the same as synchronous code; avoid `.Wait()` and `.Result` which wrap faults in `AggregateException`.

```csharp
try {
    await FetchAsync();
} catch (HttpRequestException ex) {
    Log(ex);
    throw;
}
```

- Multiple awaits can each be in one try block or separate handlers per operation.
- `finally` runs on completion path including cancellation when structured correctly.
- Global handlers supplement but do not replace local handling for recoverable errors.
- Test async exception paths with awaited assertions in unit tests.

---

#### Q6. What is the difference between `async void`, `async Task`, and `async Task<T>`?

**Answer:** `async Task` and `async Task<T>` return awaitable tasks callers can observe; `async void` is fire-and-forget with no task surface, suitable only for event handlers where the platform cannot await. `async void` exceptions crash unobservably through process-level handlers — avoid elsewhere.

- `async Task` — completion and errors observable via await.
- `async Task<T>` — await yields `T` on success.
- `async void` — only for UI/event handlers like button clicks per Microsoft guidance.
- Test and compose with `Task`-returning async methods exclusively in libraries and services.

---

#### Q7. What is an async stream (`IAsyncEnumerable<T>`) in C# 8+, and how does `await foreach` work?

**Answer:** `IAsyncEnumerable<T>` represents a sequence produced asynchronously page by page; `await foreach` consumes it with asynchronous iteration, awaiting each `MoveNextAsync` and current element retrieval without blocking threads during I/O between items.

```csharp
await foreach (var row in ReadRowsAsync(token))
    Process(row);
```

- Iterator methods use `async IAsyncEnumerable<T>` with `yield return` and `[EnumeratorCancellation]`.
- Supports streaming large datasets from EF Core, gRPC, and files with bounded memory.
- Cancellation tokens propagate through async enumeration APIs.
- Distinct from `Task<IEnumerable<T>>` which often materializes entirely before return.

---

#### Q8. How does the async state machine work under the hood (high level: `MoveNext`, `IAsyncStateMachine`)?

**Answer:** The compiler generates a struct implementing `IAsyncStateMachine` with a `MoveNext` method that jumps between states at each `await`, storing local variables in fields between suspensions. When an awaited task completes, a continuation calls `MoveNext` again to resume after the await point.

- State `-1` running, `-2` completed, non-negative states mark await resume points.
- Builder (`AsyncTaskMethodBuilder`) creates and completes the returned `Task`.
- Heap allocation of the state machine occurs when the async method runs — not at compile time only.
- Understanding explains why locals survive across awaits and why debug stepping looks nonlinear.

---

#### Q9. What is synchronization context, and how does it affect continuation marshaling?

**Answer:** `SynchronizationContext` represents a threading model for posting work back to a specific context — UI message loop or legacy ASP.NET request context. When present, default `await` posts continuations through `Post` to that context; when null or disabled with `ConfigureAwait(false)`, continuations run on thread pool threads.

- WPF/WinForms install UI synchronization contexts automatically on UI threads.
- ASP.NET Core generally lacks a capturing context — continuations use thread pool directly.
- Deadlocks occur when blocking the context thread while awaiting work that needs that thread to complete (See Q10).
- `SynchronizationContext.Current` is null in many console and modern server scenarios.

---

#### Q10. Why can `.Result`, `.Wait()`, and `.GetAwaiter().GetResult()` cause deadlocks?

**Answer:** These synchronous blocking APIs wait on the calling thread while the async operation needs a continuation on that same thread — classic on UI or legacy ASP.NET with captured synchronization context — so neither side progresses. The async method cannot resume because the blocked thread never pumps continuations.

- UI thread `.Wait()` on async method that awaits back to UI — permanent deadlock.
- Fix: `await` all the way or use `ConfigureAwait(false)` inside library code called synchronously only when context-free.
- ASP.NET Core reduces but does not eliminate all blocking pitfalls under synchronization primitives.
- Prefer async signatures at API boundaries instead of sync-over-async wrappers.

---

#### Q11. What is the difference between `await task` and `return task` from an async method (async method builder behavior)?

**Answer:** `await task` suspends the current async method until completion, unwraps exceptions, and produces the result inline. `return task` (in an async method returning `Task`/`Task<T>`) completes the async method's task when the returned inner task completes — useful for passthrough wrappers without extra state machine suspension at that layer.

- `return await task` adds a suspension point — sometimes needed for exception context or `ConfigureAwait` application.
- Passthrough methods that simply forward can `return otherTaskAsync()` without async/await if no try/catch — eliding async wrapper.
- Compiler warns on redundant `async` when method only returns one task — can remove async keyword.
- Choose based on whether you need local exception handling or context configuration around the inner task.

---

#### Q12. How do you implement retry with exponential backoff in async code?

**Answer:** Loop attempts with increasing delay between retries — often `delay = base * 2^attempt` capped at a maximum — honoring `CancellationToken` and logging failures until success or max attempts exceeded, then rethrow or return fallback.

```csharp
for (int attempt = 0; ; attempt++) {
    try { return await CallAsync(ct); }
    catch when (attempt < max && IsTransient(ex)) {
        await Task.Delay(Backoff(attempt), ct);
    }
}
```

- Transient fault detection filters which exceptions retry — not all errors should retry.
- Cap delays and attempts to prevent runaway loops (See Gotcha 15).
- Jitter reduces synchronized retry storms (See Q13).
- Polly libraries encapsulate policies declaratively (See Q14).

---

#### Q13. What is jitter in backoff strategies, and why is it used?

**Answer:** Jitter adds random variation to retry delays so many clients do not retry simultaneously after the same outage, preventing thundering herds that overwhelm recovering services. Apply jitter as a random offset or multiplier on calculated backoff delay.

- Without jitter, coordinated clients realign on identical retry timestamps.
- Full jitter picks random delay between zero and exponential cap — AWS-style patterns.
- Jitter improves stability for distributed systems and mobile clients on flaky networks.
- Combine with max delay caps and cancellation tokens for bounded total wait.

---

#### Q14. How do Polly-style resilience policies relate to manual retry loops?

**Answer:** Resilience libraries like Polly express retries, circuit breakers, timeouts, and bulkheads as reusable policies composed around delegates, replacing hand-written loops with tested implementations and consistent telemetry hooks.

- Policies separate cross-cutting resilience from business logic methods.
- Circuit breakers stop calling failing dependencies until recovery window passes.
- Composition (`Policy.Wrap`) orders timeout outside retry, etc., explicitly.
- Manual loops suffice for simple cases — libraries pay off with growing policy complexity.

---

#### Q15. What is `CancellationTokenSource.CreateLinkedTokenSource`, and when is linking tokens needed?

**Answer:** Linked sources combine multiple cancellation tokens so canceling any parent token cancels the linked token passed to operations — common pattern linking user cancel with timeout or host shutdown tokens.

```csharp
using var linked = CancellationTokenSource.CreateLinkedTokenSource(userToken, timeoutToken);
await WorkAsync(linked.Token);
```

- Dispose linked sources to unregister callbacks and free resources.
- Timeout via `CancelAfter` on a dedicated source linked into operations.
- ASP.NET Core requests expose `HttpContext.RequestAborted` linked with app shutdown tokens in hosted services.
- Propagate linked tokens into all downstream async calls accepting cancellation.

---

#### Q16. How do you propagate cancellation through layered async APIs?

**Answer:** Accept `CancellationToken` as the last parameter on public async methods — often with default — and pass it to every awaited operation, registration, and delay. Never create orphaned long-running work ignoring upstream tokens.

- Optional parameter convention: `CancellationToken cancellationToken = default`.
- Register callbacks on tokens for cleanup when cancellation fires mid-operation.
- Use `ThrowIfCancellationRequested` in CPU loops between chunks of work.
- Middleware and hosted services link host lifetime tokens automatically for request scopes.

---

#### Q17. What is `Task.Delay` vs `Thread.Sleep` in async methods?

**Answer:** `await Task.Delay` asynchronously waits without blocking the thread, allowing pool threads to serve other work during the delay. `Thread.Sleep` blocks the current thread entirely — never use it in async methods or on pool threads serving requests.

- `Task.Delay` accepts cancellation tokens — sleep aborts promptly on cancel.
- `Thread.Sleep` in async methods wastes threads and defeats scalability goals.
- Timers and periodic work use `PeriodicTimer` or delayed loops with `Task.Delay` in modern code.
- UI apps still prefer async delays over sleep even on UI thread when paired with await yielding via message pump indirectly.

---

#### Q18. What is "async all the way" — why is mixing blocking and async problematic?

**Answer:** Async all the way means public APIs and call chains use async/await consistently to the I/O boundary without inserting synchronous `.Wait()` or `.Result` wrappers. Mixing blocks threads, risks deadlocks under captured contexts, hides exceptions in aggregate form, and reduces throughput under load.

- Sync-over-async at library boundaries forces consumers into deadlock-prone patterns.
- Legacy integrations sometimes require isolated sync wrappers at the outermost edge only — document thread requirements.
- Refactor controllers and services to async end-to-end in ASP.NET Core.
- Code analyzers flag sync waits in async contexts — heed warnings in reviews.

---

#### Q19. How do you unit test async methods and time-dependent retry logic?

**Answer:** Await async methods directly in xUnit/NUnit/MSTest tests — modern frameworks support async test methods. Inject abstractions for clocks and delays (`TimeProvider`, fake delay services) to test retry without real wall-clock waits; mock dependencies throwing transient faults on first calls.

- Use `Assert.ThrowsAsync` for expected fault paths.
- Fake `Task.Delay` via wrapper interfaces to speed retry tests to milliseconds.
- Avoid `Task.Run` in tests unless verifying parallel behavior explicitly.
- `CancellationTokenSource.CancelAfter` with short intervals tests cancellation paths deterministically with controlled timeouts.

---

#### Q20. What is `IAsyncDisposable`, and how does `await using` work?

**Answer:** `IAsyncDisposable` defines `DisposeAsync` for asynchronous resource cleanup — database connections, streams, and network sessions — and `await using` compiles to calling `DisposeAsync` at scope exit, awaiting completion without blocking.

```csharp
await using var connection = await OpenConnectionAsync();
```

- Complements `IDisposable` for resources needing async teardown.
- Compiler generates try/finally ensuring disposal on exceptions.
- Implement both dispose patterns when type supports sync and async cleanup paths.
- Order of disposal reverses declaration order for multiple `await using` variables.

---

### 05. Parallel Programming

#### Q1. What is `Parallel.For` and `Parallel.ForEach`?

**Answer:** `Parallel.For` iterates an integer index range with body delegates potentially running concurrently on multiple thread pool threads. `Parallel.ForEach` applies the same model to `IEnumerable<T>` or `IList<T>` sources, partitioning elements across workers for data-parallel CPU work.

- Both block the calling thread until all partitions complete or fail — not async methods.
- Loop bodies must be thread-safe regarding shared state or use local/partition-local accumulators.
- Use for CPU-bound parallelizable work with sufficient item count to amortize scheduling overhead.
- Not a substitute for async I/O parallelism — different problem domain.

---

#### Q2. What is `ParallelOptions` (`MaxDegreeOfParallelism`, `CancellationToken`) used for?

**Answer:** `ParallelOptions` configures parallel loops — `MaxDegreeOfParallelism` caps concurrent tasks ( `-1` means default processor-based ), and `CancellationToken` enables cooperative early termination of the loop when cancellation is requested.

- Cap parallelism on shared servers to leave headroom for other requests — e.g., `MaxDegreeOfParallelism = 4`.
- Pass request abort tokens into parallel batch jobs for responsive shutdown.
- `-1` uses heuristic thread count — often logical processor count.
- Options apply per loop invocation — tune per workload characteristics.

---

#### Q3. What is a `Partitioner<TSource>`, and when would you supply a custom partitioner?

**Answer:** A `Partitioner<TSource>` splits a data source into chunks for parallel workers — default partitioners exist for arrays and lists. Custom partitioners help when elements have highly uneven processing cost or when you want fixed chunk sizes for cache locality.

- `Partitioner.Create(0, length)` range partitioners for indexable data.
- Load-balanced custom partitioners dynamically assign work to idle threads for skewed workloads.
- Incorrect partitioning can over-split tiny collections — overhead exceeds benefit.
- PLINQ uses partitioners internally when executing parallel queries.

---

#### Q4. What is the difference between range partitioning and chunk partitioning?

**Answer:** Range partitioning assigns contiguous index subranges to each worker — efficient for uniform-cost elements on arrays. Chunk partitioning hands out fixed-size blocks dynamically as threads finish — better load balance when per-element cost varies unpredictably.

- Range: thread 1 gets indices 0–999, thread 2 gets 1000–1999, etc.
- Chunk: threads grab next 64 items from a shared queue when idle.
- Skewed workloads favor chunk strategies to avoid one thread stuck on heavy tail.
- Default `Parallel.ForEach` on lists uses partitioner heuristics combining both ideas.

---

#### Q5. Explain PLINQ (`AsParallel`, `WithDegreeOfParallelism`, `WithMergeOptions`).

**Answer:** Parallel LINQ applies `AsParallel()` to run LINQ to Objects operators concurrently using partitioners and merge strategies. `WithDegreeOfParallelism` limits concurrent threads; `WithMergeOptions` controls how results merge (preserving order is costlier).

- `collection.AsParallel().Where(...).Select(...)` — parallel query over in-memory data.
- `AsOrdered` preserves input order in results at performance cost.
- Not all operators parallelize equally — aggregation and joins have merge phases.
- PLINQ runs on thread pool — competes with other server work without caps.

---

#### Q6. When is parallelization slower than sequential execution?

**Answer:** Parallelization loses when work per element is tiny, collection size is small, synchronization overhead dominates, or false sharing/cache contention erodes gains. Partitioning and thread coordination have fixed costs that sequential loops avoid for small inputs.

- Microscopic bodies (adding two ints) in loops of dozens — sequential wins.
- Heavy lock contention serializes parallel workers back to effective single-thread speed.
- Nested parallel loops multiply overhead exponentially.
- Measure with and without parallel — do not parallelize by default without profiling.

---

#### Q7. What types of workloads benefit from PLINQ vs `Parallel.ForEach`?

**Answer:** PLINQ suits declarative LINQ pipelines over in-memory collections where operator composition (filter, map, aggregate) reads naturally in parallel query form. `Parallel.ForEach` suits imperative per-item processing with side effects, complex control flow, or non-LINQ-friendly operations in the loop body.

- PLINQ: parallel analytics on large lists already expressed as LINQ chains.
- `Parallel.ForEach`: image pixel loops, file processing with heterogeneous steps per item.
- PLINQ aggregations support associative reductions with parallel merge when configured.
- Choose based on code clarity and whether LINQ operators match the computation structure.

---

#### Q8. What are thread-safe requirements when using parallel loops (shared state, locals, aggregation)?

**Answer:** Loop bodies must not unsafely mutate shared collections or variables unless protected by locks, concurrent collections, or interlocked operations. Prefer thread-local accumulators merged after the loop for aggregation; use `ParallelLoopState` for early break coordination.

- Shared `List<T>.Add` races — use concurrent bag or lock, or collect locally then merge.
- Read-only shared data is safe — multiple threads reading immutable configuration concurrently.
- `Parallel.For` thread-local finally hooks merge partition results — pattern for sum/count without lock hot spots.
- Avoid sharing non-thread-safe random or formatters — use `[ThreadStatic]`, `Random.Shared`, or locals.

---

#### Q9. How do you perform parallel aggregation with `lock`, `Interlocked`, or thread-local accumulators?

**Answer:** Thread-local accumulators compute per-partition partial results with lock-free local updates, then merge once — lowest contention. `Interlocked` suits single numeric counters; `lock` protects multi-field invariants during merge or shared structure updates.

- `Parallel.For` overload with `localInit`, `body`, `localFinally` merges sums efficiently.
- `Interlocked.Add` for global counter when merge phase is unnecessary complexity.
- `lock` around shared list merges when aggregation structure is complex.
- Choose interlocked for one field; lock for multi-step invariants (See Synchronization Q9).

---

#### Q10. What is `ParallelLoopResult`, and how do you detect partial failures?

**Answer:** `ParallelLoopResult` returns from parallel loop methods with `IsCompleted` and `LowestBreakIteration` properties indicating whether the loop finished all iterations or stopped early via `Break`/`Stop`. Exceptions from any partition throw `AggregateException` wrapping body faults — partial progress may have occurred before failure.

- `Stop` terminates fastest — sibling partitions may stop soon after.
- `Break` ensures indices below break point complete — different early-exit semantics.
- Catch `AggregateException` and inspect `InnerExceptions` for multiple partition failures.
- Idempotent or transactional designs handle partial parallel failure safely.

---

#### Q11. What are ordering guarantees in PLINQ (`AsOrdered`) and their cost?

**Answer:** Default PLINQ does not preserve source order — faster merge. `AsOrdered` forces output order to match input sequence order, requiring ordered merge buffers that increase memory and synchronization cost during parallel execution.

- Use unordered PLINQ when order irrelevant — maximum throughput.
- Use `AsOrdered` when downstream expects stable ordering matching source — pays overhead.
- `OrderBy` in PLINQ is separate concern — sorts by key possibly after parallel operators.
- Document ordering requirements in API returning parallel query results.

---

#### Q12. How does parallel LINQ decide default partition sizes?

**Answer:** PLINQ uses partitioner heuristics based on collection type, count, and processor cores — arrays and lists get range or chunk strategies aiming to balance load while minimizing delegation overhead. Very small sequences may run sequentially even after `AsParallel()`.

- Partition size adapts to element count and `WithDegreeOfParallelism`.
- Custom partitioners override defaults for domain-specific skew.
- Tiny collections fall below parallel threshold — sequential execution avoids setup cost.
- Inspect behavior with performance counters when tuning large data sets.

---

#### Q13. What exceptions are thrown from parallel loops (`AggregateException`, inner exceptions)?

**Answer:** An unhandled exception in any parallel loop body surfaces as `AggregateException` containing one or more inner exceptions from failing partitions. Other partitions may be canceled abruptly when one fails — not always all iterations complete.

- Flatten aggregates for logging all inner faults.
- Do not assume transactional all-or-nothing unless you implement compensating logic.
- `OperationCanceledException` appears when loop canceled via token.
- Prefer try/catch inside body for recoverable per-item failures when batch should continue.

---

#### Q14. How do you combine async I/O with parallel CPU work without blocking the pool?

**Answer:** Use async I/O at the edges with `await`, then optionally process CPU-bound stages with `Parallel.ForEach` on materialized data, or use `Task.WhenAll` for concurrent async I/O without blocking. Never `.Wait()` parallel or async work on pool threads during request handling.

- Pattern: `var data = await FetchAsync(); Parallel.ForEach(data, ProcessCpu);` — I/O async, CPU parallel separately.
- Concurrent I/O: multiple `await` tasks with `WhenAll` — no thread per wait.
- Avoid `Parallel.ForEach` with blocking I/O inside body — exhausts pool.
- Channel-based pipelines separate async read, parallel transform, async write stages cleanly.

---

#### Q15. What are best practices for parallel and async code in server applications?

**Answer:** Use async I/O throughout request paths, cap CPU parallelism with `MaxDegreeOfParallelism`, avoid nested parallel loops, never block on async or parallel work, propagate cancellation, and measure thread pool queue depth under load. Separate concerns — async for waiting, parallel for CPU batches with explicit bounds.

- Default ASP.NET Core actions are async — keep them non-blocking.
- Background CPU jobs run in dedicated workers or bounded `Parallel` with monitoring.
- Share read-only data safely; isolate mutable state per request scope.
- Load test combined async and parallel patterns — interactions cause production-only starvation.

---

### 06. Synchronization and Locks

#### Q1. Explain synchronization primitives: `lock`, `Monitor`, `Mutex`, and `Semaphore`/`SemaphoreSlim`.

**Answer:** `lock` is C# sugar for `Monitor.Enter/Exit` on a reference object, providing mutual exclusion in-process. `Mutex` is a named kernel mutex for cross-process synchronization. `Semaphore`/`SemaphoreSlim` limit concurrent entrants — `SemaphoreSlim` is lightweight and supports async wait for in-process throttling.

| Primitive | Scope | Typical use |
|---|---|---|
| `lock` / `Monitor` | In-process | Protect shared mutable state |
| `Mutex` | Cross-process | Single-instance apps, named locks |
| `SemaphoreSlim` | In-process | Limit N concurrent operations |

- `lock` blocks one thread at a time in critical sections.
- Mutexes are slower and OS-backed — use only when cross-process needed.
- Semaphores allow N simultaneous holders — thread pools, connection gates.

---

#### Q2. What is `ReaderWriterLockSlim`, and when is it preferable to a plain `lock`?

**Answer:** `ReaderWriterLockSlim` allows many concurrent readers OR one writer, optimizing read-heavy caches where plain `lock` serializes all access. Use it when reads dominate and write sections are short and infrequent.

- Enter read lock for cache lookups; upgrade to write lock for updates — upgrade rules are strict — avoid recursive upgrade deadlocks.
- Wrong for write-heavy workloads — writer starvation possible under constant readers in some policies.
- `ReaderWriterLock` (non-Slim) is legacy — prefer Slim variant.
- Concurrent collections sometimes replace manual RW locks for specific map scenarios.

---

#### Q3. Explain `AutoResetEvent`, `ManualResetEvent`, and `ManualResetEventSlim`.

**Answer:** Events signal threads waiting for conditions — `AutoResetEvent` releases one waiting thread and resets automatically; `ManualResetEvent` stays signaled until manually reset, releasing all waiters; `ManualResetEventSlim` is in-process optimized spin-then-block variant. Modern code often prefers `Task`, channels, or `Monitor.Wait/Pulse` patterns instead of raw events.

- AutoReset — one producer signals one consumer per pulse — queue worker wakeups.
- ManualReset — broadcast gate open until `Reset()` — startup initialization barriers.
- Events are error-prone with missed signals and race windows — document ownership carefully.
- Prefer higher-level coordination (`TaskCompletionSource`, channels) in new designs.

---

#### Q4. What is `CancellationToken`, and how do you implement cooperative cancellation?

**Answer:** `CancellationToken` propagates cancellation requests without forcibly aborting threads — workers poll `IsCancellationRequested` or call `ThrowIfCancellationRequested` at safe points, and register callbacks for cleanup when `CancellationTokenSource.Cancel` fires.

- Long loops check token each iteration or between chunks.
- I/O APIs accept tokens to abort network waits promptly where supported.
- Registration disposes with token lifetime — avoid leaking registrations.
- Cancellation is cooperative — code must observe tokens to stop promptly.

---

#### Q5. Explain deadlocks in multithreading — necessary conditions and prevention strategies.

**Answer:** Deadlock requires mutual exclusion, hold-and-wait, no preemption, and circular wait. Prevention strategies include consistent lock ordering, timeouts with `Monitor.TryEnter`, reducing lock scope, avoiding nested locks on different objects, and using async instead of blocking waits.

- Classic circular wait: thread A locks L1 waits L2; thread B locks L2 waits L1.
- Lock ordering convention: always acquire L1 before L2 globally.
- TryEnter with timeout fails fast instead of freezing indefinitely.
- Diagnostic tools capture lock chains in dumps — `dotnet-dump analyze`.

---

#### Q6. What are race conditions, and how can they be prevented?

**Answer:** Race conditions occur when concurrent threads access shared mutable state and outcomes depend on scheduling order. Prevent them with mutual exclusion (`lock`), atomic operations (`Interlocked`), immutable data structures, or confining mutable state to a single thread.

- Check-then-act races (`if (!dict.ContainsKey(k)) dict.Add(k,v)`) fail under concurrency.
- Use `ConcurrentDictionary.GetOrAdd` or lock around compound operations.
- Immutability eliminates races on shared read-only snapshots.
- Tests with stress loops and thread sanitizer-style repetition expose intermittent races.

---

#### Q7. What is the `volatile` keyword, and when does it provide visibility guarantees?

**Answer:** `volatile` reads and writes bypass certain CPU cache optimizations so all threads see the latest value of a field without stale cached copies — it provides visibility ordering for that field but not atomicity for compound operations. Use for simple flags signaling state between threads when paired with correct logic.

- Writer sets `volatile bool _ready = true`; readers observe update without lock for flag-only patterns.
- Does not make `i++` atomic on volatile int — still races (See Gotcha 12).
- Modern alternatives include `Interlocked` and `lock` for most production synchronization.
- `Volatile.Read`/`Write` methods offer fine-grained control similar to volatile semantics.

---

#### Q8. What is the difference between `volatile` and `lock` for thread safety?

**Answer:** `volatile` ensures reads/writes to a field are visible across threads but does not protect multi-step invariants involving multiple fields or read-modify-write sequences. `lock` provides mutual exclusion so critical sections execute atomically relative to other threads.

- Volatile flag stop signal — sufficient for one boolean observed by workers.
- Bank transfer between two accounts requires lock — two fields must update together.
- Lock implies full memory barrier on enter/exit — stronger than volatile alone for sections.
- Do not use volatile as a substitute for locks on complex shared structures.

---

#### Q9. What is `Interlocked` (`Increment`, `CompareExchange`, `Add`), and when is it enough without `lock`?

**Answer:** `Interlocked` methods perform atomic read-modify-write on single variables using CPU atomic instructions — sufficient for counters, reference swaps, and lock-free stack head updates when invariants involve only one location. Multi-field updates still need locks or careful compare-exchange retry loops.

- `Interlocked.Increment(ref counter)` — safe global counter without lock object.
- `CompareExchange` implements lock-free patterns — retry loops on contention.
- Not composable across unrelated fields — balance transfer needs lock or transactional pattern.
- Faster than `lock` for hot single-field metrics under moderate contention.

---

#### Q10. What is `SpinLock`, and when might low-latency spinning beat `lock`?

**Answer:** `SpinLock` spins in user mode briefly before blocking, suited for extremely short critical sections on multiprocessors where context switch cost exceeds spin wait. Misused on single-core or long sections wastes CPU — default to `lock` unless profiling proves spin benefits.

- Microscopic sections protecting a few instructions — rare in application business logic.
- Never hold SpinLock while calling unknown code — deadlock if reentered or blocks while spinning.
- `Monitor` lock yields to OS scheduler when contended — better general default.
- Use only in performance-critical infrastructure code with measured justification.

---

#### Q11. What is lock ordering, and how does it prevent deadlock?

**Answer:** Lock ordering assigns a global order to all lock objects and requires every thread acquires locks in increasing order only — preventing circular wait where thread A holds L1 waits L2 while B holds L2 waits L1. Document order for nested resources like accounts sorted by ID before transfer locks.

- Transfer between accounts: lock lower AccountId first, then higher — consistent global order.
- Breaking order when maintenance adds new lock paths reintroduces deadlock risk.
- Try-lock patterns with backoff are alternative when ordering impossible — harder to prove correct.
- Static analysis and code review check nested lock acquisition patterns.

---

#### Q12. What is the `Monitor.TryEnter` pattern, and how do timeouts help avoid indefinite blocking?

**Answer:** `Monitor.TryEnter(obj, timeout)` attempts to acquire lock within milliseconds, returning false if unavailable instead of blocking forever — enabling fallback, logging, or alternative code paths under contention.

```csharp
if (Monitor.TryEnter(_gate, TimeSpan.FromSeconds(2))) {
    try { /* work */ } finally { Monitor.Exit(_gate); }
} else { /* degrade gracefully */ }
```

- Prevents indefinite hangs when another thread dies holding lock — still leak if holder never releases.
- `lock` statement has no timeout — use explicit Monitor for timed attempts.
- Always `Exit` in `finally` when `TryEnter` succeeds.
- Combine with diagnostics logging lock wait failures for production tuning.

---

#### Q13. What is async-compatible locking (`SemaphoreSlim.WaitAsync`) vs blocking `lock` in async code?

**Answer:** `lock` cannot be awaited — blocking a thread inside async methods defeats scalability. `SemaphoreSlim.WaitAsync` asynchronously waits for exclusive or counting semaphore slots without blocking pool threads during the wait, preserving async flow.

- Pattern: `await _semaphore.WaitAsync(token); try { ... } finally { _semaphore.Release(); }`
- Protect async-critical sections updating shared state across awaits — rare; prefer per-async flow state instead.
- Do not hold locks across `await` if lock is synchronous `Monitor` — other threads blocked throughout await duration.
- `AsyncLock` community patterns wrap semaphore for mutex-like async API.

---

#### Q14. What is a priority inversion problem (conceptual), and which primitives exacerbate it?

**Answer:** Priority inversion occurs when a high-priority thread waits on a lock held by a low-priority thread, while medium-priority threads preempt the low holder — delaying completion unexpectedly. Mutexes with priority inheritance in some OSes mitigate; plain `lock` and busy waiting can worsen latency unpredictability on systems with thread priorities.

- Conceptual in managed code — most apps do not tune thread priorities.
- Long critical sections increase inversion windows regardless of priority settings.
- Keep locks short; avoid priority manipulation as primary synchronization strategy.
- Real-time systems need OS-specific mutex features — not typical ASP.NET scenarios.

---

#### Q15. How do you diagnose deadlocks and lock contention in production (dump analysis, `dotnet-sync`, counters)?

**Answer:** Capture process dumps under hang conditions and analyze thread stacks for cycles waiting on monitors — `dotnet-dump analyze` with SOS commands (`syncblk`, `clrstack`) reveals lock owners. Monitor `ContentionRate` on locks via EventCounter and profile with dotnet-trace during load tests reproducing contention.

- All pool threads blocked waiting same lock — classic symptom in dumps.
- Lock contention counters rise before throughput collapses — alert early.
- Reproduce in stress environment with concurrent integration tests.
- Fix by shrinking critical sections, removing nested locks, or restructuring to lock-free patterns where safe.

---

#### Q16. What is thread-safe lazy initialization (`Lazy<T>`, double-checked locking pitfalls)?

**Answer:** `Lazy<T>` provides thread-safe deferred initialization with publication modes ensuring exactly one factory execution. Hand-rolled double-checked locking requires `volatile` fields and careful memory model rules — easy to get wrong; prefer `Lazy<T>` unless extreme optimization demands custom code.

```csharp
private static readonly Lazy<Expensive> _instance =
    new Lazy<Expensive>(() => new Expensive(), LazyThreadSafetyMode.ExecutionAndPublication);
```

- `ExecutionAndPublication` mode prevents duplicate ctor calls under contention.
- Double-checked locking without volatile reads may publish partially constructed objects on some architectures historically — `Lazy<T>` avoids this pitfall.
- Singleton services in DI containers usually use container lifetime instead of static lazy.
- Lazy initialization delays cost until first use — good for optional heavy resources.

---

### 07. Concurrent Collections

#### Q1. What concurrent collections exist in .NET (`ConcurrentDictionary`, `ConcurrentQueue`, `ConcurrentBag`, `BlockingCollection`, etc.)?

**Answer:** .NET provides thread-safe collections including `ConcurrentDictionary`, `ConcurrentQueue`, `ConcurrentStack`, `ConcurrentBag`, `BlockingCollection` (producer-consumer wrapper), and modern `System.Threading.Channels` for async pipelines. Each targets different ordering and access patterns under concurrency.

- `ConcurrentDictionary` — thread-safe key-value map with atomic add/update helpers.
- `ConcurrentQueue` — FIFO ordering for work queues.
- `ConcurrentBag` — unordered per-thread staging optimized for parallel aggregation.
- `BlockingCollection` — bounded/unbounded blocking take/add for producer-consumer threads.

---

#### Q2. When should you use thread-safe collections instead of standard collections plus locks?

**Answer:** Use concurrent collections when multiple threads frequently add, remove, or lookup without wanting a single global lock around every operation, especially under read/write contention on shared caches or work queues. Single-threaded access or rare contention may still favor simple collections with occasional locks for clarity.

- High-throughput server caches benefit from `ConcurrentDictionary.GetOrAdd`.
- Producer-consumer pipelines use `BlockingCollection` or channels instead of locked `List<T>`.
- Low contention single-writer scenarios — locked list may be simpler and sufficient.
- Concurrent collections do not make compound business invariants atomic across keys automatically.

---

#### Q3. What is the difference between `Dictionary<TKey, TValue>` and `ConcurrentDictionary<TKey, TValue>`?

**Answer:** `Dictionary` is not thread-safe — concurrent `Add` or read-while-write races corrupt internal buckets. `ConcurrentDictionary` uses fine-grained locking or lock-free techniques for thread-safe `TryAdd`, `TryGetValue`, and atomic update helpers without external lock wrapping every call.

- Never share plain `Dictionary` across threads without synchronization.
- `ConcurrentDictionary` enumeration is snapshot-style weakly consistent — may miss concurrent adds.
- Null keys disallowed in both for reference key types standard dictionaries.
- Choose concurrent version for shared caches populated at runtime from parallel requests.

---

#### Q4. What are `AddOrUpdate`, `GetOrAdd`, and `TryUpdate` on `ConcurrentDictionary`?

**Answer:** `GetOrAdd` adds a value if key missing atomically; `AddOrUpdate` inserts or updates with factory delegates defining merge logic; `TryUpdate` replaces value only if current value matches expected comparison — supporting optimistic concurrency on cached entries.

- `GetOrAdd(key, _ => new LazyResource())` ensures single factory execution per key attempt — still race without lazy inside.
- `AddOrUpdate` handles insert vs update with different factories — useful for counters and aggregates.
- `TryUpdate` fails if another thread changed value first — retry pattern applies.
- These methods reduce lock duration compared to manual get-check-set sequences.

---

#### Q5. What is `BlockingCollection<T>`, and how does it implement producer-consumer patterns?

**Answer:** `BlockingCollection<T>` wraps an `IProducerConsumerCollection` (often concurrent queue) with optional bounding capacity, blocking `Add` and `Take` operations until space or items are available. Producers enqueue work; consumer threads dequeue — decoupling generation from processing rates.

- Bounded capacity applies backpressure — producers block when queue full.
- `GetConsumingEnumerable` yields items until collection completed and drained.
- Multiple consumers compete fairly on underlying concurrent structure.
- Modern `Channel<T>` often replaces `BlockingCollection` in async services (See Q15).

---

#### Q6. What is the difference between bounded and unbounded `BlockingCollection` behavior?

**Answer:** Unbounded collections accept unlimited adds without blocking producers — memory grows if consumers lag, risking out-of-memory failures. Bounded collections specify `boundedCapacity`, causing producers to block on `Add` when full, throttling upstream automatically.

- Unbounded suits low-volume or trusted producer rates with fast consumers.
- Bounded protects memory and applies natural flow control in ETL pipelines.
- `TryAdd` with timeout avoids indefinite producer blocking when consumers stall.
- Monitor queue depth metrics when using bounded buffers in production.

---

#### Q7. How do you use `BlockingCollection` with multiple producers and consumers?

**Answer:** Share one `BlockingCollection` instance across producer tasks calling `Add` (or `TryAdd`) and consumer tasks calling `Take` or iterating `GetConsumingEnumerable`. Call `CompleteAdding` when all producers finish so consumers eventually drain and exit after empty plus completed state.

- Producers need not coordinate among themselves beyond thread-safe `Add`.
- Consumers loop `Take` until `IsCompleted` and empty — or use consuming enumerable.
- Exception in consumer should not leave producers blocked on full bounded collection — use finally and cancellation.
- `CompleteAdding` prevents further adds — adds throw `InvalidOperationException` afterward (See Gotcha 10).

---

#### Q8. What is `ConcurrentQueue` vs `ConcurrentStack` vs `ConcurrentBag` — ordering and stealing semantics?

**Answer:** `ConcurrentQueue` is FIFO — fair ordering for work queues. `ConcurrentStack` is LIFO — last-in-first-out stacks. `ConcurrentBag` is unordered globally but optimizes parallel adds with thread-local piles and work-stealing when enumerating or merging — best when order does not matter and many threads produce items.

- Queue — order-preserving job scheduling.
- Stack — depth-first parallel tasks or undo-like patterns.
- Bag — parallel aggregation of results where order irrelevant — fastest for many producers.
- Pick structure matching ordering requirements — bag wrong for ordered log replay.

---

#### Q9. When is `ConcurrentBag` the wrong choice despite being thread-safe?

**Answer:** Avoid `ConcurrentBag` when global FIFO order matters, when you need indexed access, or when enumeration must reflect a consistent snapshot — bag ordering is undefined and optimized for per-thread staging, not sequential fairness.

- Ordered job processing requires `ConcurrentQueue`.
- Reporting events chronologically cannot use bag without sorting afterward.
- Enumeration walks thread-local piles — order appears arbitrary.
- After parallel phase, sort or drain into ordered structure if sequence required downstream.

---

#### Q10. What is `IProducerConsumerCollection<T>` and custom underlying stores for `BlockingCollection`?

**Answer:** `IProducerConsumerCollection<T>` defines producer-consumer storage (`TryAdd`, `TryTake`) implemented by concurrent queue, stack, or bag. `BlockingCollection` constructor accepts custom implementations — swap backing store to match ordering or performance needs while keeping blocking wrapper semantics.

- Inject `ConcurrentQueue` for FIFO blocking collection.
- Custom implementations must be thread-safe — rare outside specialized scenarios.
- Blocking layer adds wait signaling atop raw concurrent collection.
- Channels supersede many custom blocking wrappers in new async code.

---

#### Q11. How do concurrent collections compare to locking a `List<T>` for high-contention scenarios?

**Answer:** Locking a `List<T>` serializes every operation — readers and writers block each other on one mutex. Concurrent collections reduce contention via finer internal synchronization or lock-free segments, improving throughput when many threads access shared structure simultaneously.

- Locked list simple for low contention — easy to reason about.
- Under heavy parallel adds, single lock becomes bottleneck — concurrent bag or queue scales better.
- Compound operations spanning multiple structures still need external coordination locks.
- Measure contention — premature concurrent collection complexity may not pay off.

---

#### Q12. What enumeration semantics do concurrent collections provide (weakly consistent iterators)?

**Answer:** Enumerators snapshot elements weakly consistently — they reflect collection state at some point during enumeration but may skip or duplicate elements if concurrent adds/removes occur, and never throw concurrent modification exceptions like unsynchronized collections might corrupt.

- Do not assume enumeration is exact point-in-time snapshot unless using external locking.
- Count properties are approximate under concurrency for some structures.
- To snapshot, copy to array or list under lock or after processing completes.
- Document weak consistency for consumers iterating live caches.

---

#### Q13. How do you gracefully complete adding to a `BlockingCollection` (`CompleteAdding`)?

**Answer:** Producers signal completion with `CompleteAdding` after final item — no more adds allowed. Consumers continue `Take` until empty, then `IsCompleted` becomes true when adding completed and collection empty, allowing consumer loops to exit cleanly.

- Call `CompleteAdding` once all producer tasks finished — coordinate with `Task.WhenAll` before complete signal.
- Consumers blocked on `Take` wake when items remain after complete — drain all pending work.
- Adding after complete throws — producers must not retry add after shutdown signal misfired early.
- Pair with cancellation tokens for abort paths that may skip complete adding — handle partial drain.

---

#### Q14. What pitfalls arise when mixing concurrent collections with LINQ?

**Answer:** LINQ operators on concurrent collections often enumerate weakly consistent snapshots and may allocate intermediate sequences — not thread-safe if another thread mutates during LINQ chain unless snapshotted first. Parallel LINQ over concurrent collections still requires understanding ordering and concurrent modification during enumeration.

- Materialize copy (`ToArray`) before LINQ sorting if consistent view required.
- Do not assume `Count()` in LINQ matches exact live count under concurrent adds.
- Concurrent collection is thread-safe for its own operations — LINQ pipeline not automatically atomic end-to-end.
- Prefer dedicated methods (`GetOrAdd`) over LINQ for hot concurrent dictionary paths.

---

#### Q15. When should you use channels (`System.Threading.Channels`) instead of `BlockingCollection` in modern code?

**Answer:** Channels provide async-first producer-consumer queues with `ChannelReader`/`ChannelWriter`, `ReadAsync`/`WriteAsync`, and backpressure — integrating cleanly with `async`/`await` without blocking pool threads. Prefer channels in ASP.NET Core services and async pipelines; use `BlockingCollection` for legacy synchronous consumer threads or existing blocking code paths.

- `Channel.CreateBounded<T>` applies capacity with async wait on full channel.
- Multiple readers and writers supported with options for single-consumer optimizations.
- `Complete` on writer signals end — analogous to `CompleteAdding`.
- Channels align with modern async all-the-way server design — less thread blocking during waits.

---

### Gotchas — Module 06

#### Gotcha 1. `.Result` / `.Wait()` deadlock

**Answer:** Blocking on async work with `.Result` or `.Wait()` on a thread that owns a captured synchronization context prevents the async continuation from running on that thread, causing circular wait — classic UI and legacy ASP.NET deadlock.

- Use `await` through the call stack instead of sync-over-async at boundaries.
- Library code uses `ConfigureAwait(false)` to reduce context capture deadlock risk.
- If sync API unavoidable at edge, document thread requirements and use `GetAwaiter().GetResult()` only when context-free.
- ASP.NET Core reduces but does not eliminate blocking-related pool starvation issues.

---

#### Gotcha 2. `async void` swallows observability

**Answer:** `async void` methods cannot be awaited — exceptions propagate to `TaskScheduler.UnobservedTaskException` rather than caller try/catch, making failures hard to observe and tests unreliable.

- Reserve `async void` for UI event handlers per platform guidance.
- All service and library methods return `Task` or `Task<T>`.
- Unit tests await task-returning async methods directly.
- Fire-and-forget background work returns `Task` tracked by host or uses explicit exception logging inside delegate.

---

#### Gotcha 3. `Task.Run` for I/O

**Answer:** Offloading synchronous blocking I/O to the thread pool with `Task.Run` wastes threads during waits without improving scalability — true async I/O APIs release threads until completion.

- Replace `Task.Run(() => client.GetString(...))` with `await client.GetStringAsync(...)`.
- Pool threads blocked on I/O reduce server capacity identically to synchronous handlers.
- `Task.Run` remains valid for CPU-bound offload, not I/O wrappers.
- Legacy blocking APIs isolated at boundaries should still minimize pool usage with dedicated threads if async unavailable.

---

#### Gotcha 4. Async does not mean threaded

**Answer:** `await` on I/O operations often completes without allocating a thread for the duration of the wait — continuations may run briefly on any pool thread when data arrives. Assuming one async operation equals one dedicated thread misleads capacity planning.

- Many concurrent awaits multiplex on small pool thread counts.
- CPU work still needs threads — async is not parallelism by itself.
- Continuation scheduling depends on synchronization context and thread pool load.
- Profile thread count vs concurrent requests to see async I/O efficiency.

---

#### Gotcha 5. Unobserved task exceptions

**Answer:** Faulted tasks never awaited or observed may finalize later raising unobserved exception events — failures disappear until process-level handlers or random crashes surface them.

- Always await tasks or attach continuations logging faults.
- Avoid untracked fire-and-forget without internal try/catch.
- Use `TaskScheduler.UnobservedTaskException` only as safety net, not primary handling.
- Hosts like ASP.NET Core log unhandled exceptions on request tasks — still avoid orphan background tasks.

---

#### Gotcha 6. Race on `List<T>`/`Dictionary<,>`

**Answer:** Standard collections are not thread-safe — even `List.Add` or dictionary indexer assignments race under concurrent access, corrupting internal structures or losing updates silently.

- Use `ConcurrentDictionary`, locks, or confine mutations to single thread.
- Reading while writing without synchronization is undefined behavior for standard collections.
- LINQ over shared lists does not add thread safety.
- Defensive copies per request avoid sharing mutable lists across threads in web apps.

---

#### Gotcha 7. `ConfigureAwait(false)` in libraries

**Answer:** Library code should use `ConfigureAwait(false)` on awaits to avoid marshaling back to UI or request contexts it does not need — preventing deadlocks and unnecessary context queueing. Application UI code typically keeps default await to touch controls on UI thread after I/O.

- Reusable helpers in class libraries configure false consistently.
- ASP.NET Core library code benefits from false even without legacy context.
- Do not configure false in UI event handlers before control updates after await.
- Document when library methods require specific context for callbacks.

---

#### Gotcha 8. `ValueTask` double-await

**Answer:** Pooled `ValueTask` instances from optimized APIs may be consumed only once — double-await or concurrent await corrupts internal state unless API documents safe repeated consumption for cached synchronous paths.

- Await each `ValueTask` once or call `AsTask()` before multiple awaits.
- Library authors document consumption rules prominently.
- When unsure, convert to `Task` at boundary.
- Profiling-driven `ValueTask` optimization is not default public API pattern.

---

#### Gotcha 9. `TaskCompletionSource` set twice

**Answer:** Only the first successful `TrySetResult`, `TrySetException`, or `TrySetCanceled` wins — subsequent calls return false and dropped completion signals can leave waiters hanging if races are mishandled.

- Coordinate single completion path with locks or atomic flags if multiple threads may complete.
- Register cancellation once to call `TrySetCanceled`.
- Check return value of `TrySet*` in race scenarios and log failures.
- Expose task from TCS early; complete once when outcome known.

---

#### Gotcha 10. `BlockingCollection` after `CompleteAdding`

**Answer:** After `CompleteAdding`, further `Add` calls throw `InvalidOperationException` — producers must not attempt adds after shutdown signal even if consumers still draining.

- Signal complete only after all producers truly finished — premature complete drops expected items never added.
- Consumers must drain remaining items before exiting service loops.
- Bounded collection full plus early complete can deadlock producers still trying add — coordinate lifecycle.
- Prefer channels with completed writer in async services for clearer completion semantics.

---

#### Gotcha 11. `Interlocked` is not composable

**Answer:** `Interlocked` atomically updates one field but check-then-act across multiple fields or invariants still races — `if (balance >= amount) balance -= amount` needs lock or transactional pattern even if fields are volatile.

- Use interlocked for counters and single reference swaps only.
- Multi-field bank transfers require `lock` or database transactions.
- Compare-exchange loops implement advanced lock-free structures — expert territory.
- Do not assume volatile or interlocked fixes logical compound operations.

---

#### Gotcha 12. `volatile` does not make operations atomic

**Answer:** Marking a field `volatile` ensures visibility of reads/writes but `volatile int` increment (`i++`) is still read-modify-write non-atomic — concurrent increments lose updates without `Interlocked.Increment` or lock.

- Volatile stop flags work; volatile counters do not.
- Use `Interlocked` for arithmetic on shared numeric fields.
- Lock for multi-step mutations involving several variables.
- Code reviews should flag `volatile` counters as likely bugs.

---

#### Gotcha 13. Parallel loop over small work

**Answer:** `Parallel.ForEach` on tiny collections with negligible per-item CPU cost spends more time partitioning and scheduling than sequential loop would — parallelization hurts latency.

- Threshold collection size and body cost before enabling parallel.
- Sequential loops fine for dozens of lightweight iterations.
- Nested parallel magnifies overhead — avoid by default.
- Profile with real hardware — micro-benchmarks mislead on overhead crossover point.

---

#### Gotcha 14. Shared `Random` is not thread-safe

**Answer:** `System.Random` instance methods are not thread-safe — concurrent `Next` calls corrupt internal state or produce duplicate values. Use `Random.Shared` (.NET 6+) in parallel code or thread-local `Random` instances seeded uniquely.

- `new Random()` per call seeds poorly if created simultaneously — use `Random.Shared` or explicit seeding strategy.
- Cryptographic random uses separate thread-safe APIs (`RandomNumberGenerator`).
- Parallel simulations need independent RNG streams per partition.
- Locking around shared `Random` serializes callers — defeats parallelism benefits.

---

#### Gotcha 15. Retry without cancellation

**Answer:** Exponential backoff retry loops must honor `CancellationToken` and cap maximum attempts/delays — unbounded retries block shutdown and amplify outages during systemic failures.

- Link user cancel and host shutdown tokens into retry delays (`Task.Delay(..., token)`).
- Set max attempts and max delay ceiling with jitter.
- Stop retrying non-transient errors immediately.
- Log final failure after exhausting policy — do not loop forever silently.

---

## Module 07. File Input & Output and Streams

### 01. File & Directory Operations

#### Q1. Explain file handling in C# and the role of the `System.IO` namespace.

**Answer:** File handling in C# means reading, writing, creating, copying, moving, deleting, and inspecting files and directories through types in the `System.IO` namespace, which wraps operating-system file APIs behind a consistent managed surface. The namespace separates one-off static helpers (`File`, `Directory`), path-scoped instance wrappers (`FileInfo`, `DirectoryInfo`), byte streams (`FileStream`, `Stream`), and text adapters (`StreamReader`, `StreamWriter`).

- `System.IO` is part of the Base Class Library (BCL), so the same APIs work on Windows, Linux, and macOS when you build paths with `Path.Combine` instead of hard-coded separators.
- Static `File` and `Directory` methods are ideal for single operations such as `ReadAllText`, `Copy`, or `CreateDirectory` without holding object state between calls.
- Instance types cache metadata about one path and expose chainable operations when you touch the same file or folder repeatedly in one workflow.
- Higher-level helpers in `File` delegate to streams internally; for large files or fine-grained control you drop down to `StreamReader`/`FileStream` covered in later chapters.

---

#### Q2. What is the difference between the static `File`/`Directory` classes and the instance `FileInfo`/`DirectoryInfo` classes?

**Answer:** `File` and `Directory` expose static methods that take a path string on every call, while `FileInfo` and `DirectoryInfo` wrap a single path in an object whose properties and instance methods operate on that cached location. Both styles ultimately call the same operating-system services; the choice is about whether you need reusable state for one path or a quick one-shot operation.

| | Static (`File`, `Directory`) | Instance (`FileInfo`, `DirectoryInfo`) |
|---|---|---|
| Usage pattern | Pass path on each call | Construct once, call methods/properties many times |
| Metadata | Static getters like `File.GetLastWriteTime(path)` | Properties like `info.LastWriteTime` refresh on access |
| Chaining | Each call is independent | `info.CopyTo`, `info.MoveTo`, `info.Delete` on the same object |
| Best for | Simple scripts, one-off reads/writes | Loops, watchers, or multi-step workflows on one item |

Use static helpers when you touch a path once; use instance wrappers when the same path is queried or mutated repeatedly and you want cleaner, object-oriented code.

---

#### Q3. When would you prefer `FileInfo` over repeated `File.*` static calls on the same path?

**Answer:** Prefer `FileInfo` when your code performs multiple operations or metadata reads against the same path, because constructing one wrapper avoids repeating the path string and gives you properties such as `Length`, `CreationTime`, and `Attributes` through a single object. Each static `File.*` call re-parses and re-validates the path independently, which adds minor overhead and clutters the code when you copy, rename, and inspect the same file in sequence.

- A typical export pipeline creates a `FileInfo`, checks `Exists` and `Length`, copies with `CopyTo`, then updates timestamps — all without passing the path string to five different static methods.
- `FileInfo` properties reflect filesystem state when accessed; they are not a live watch, so refresh after external changes if accuracy matters.
- Static calls remain fine for a single `ReadAllText` or `Delete`; the instance type pays off when the path is the focus of several steps in one method.

---

#### Q4. How do `Directory.GetFiles`, `Directory.GetDirectories`, and their `Enumerate*` counterparts differ in memory behavior?

**Answer:** `GetFiles` and `GetDirectories` allocate a complete `string[]` containing every matching path before returning, while `EnumerateFiles` and `EnumerateDirectories` return a lazy `IEnumerable<string>` that yields paths one at a time as you iterate. For directories with thousands of entries, the enumerate variants keep peak memory low because they never materialize the full list upfront.

- `GetFiles(path, "*.txt")` must finish scanning (or fail) before your code receives the array, so memory grows with file count even if you only need the first match.
- `foreach` over `EnumerateFiles` can stop early — for example after finding the first file matching a predicate — without reading the rest of the directory.
- Both approaches hold directory handles during enumeration; neither is a background watcher, and both accept optional `SearchOption.AllDirectories` for recursive scans.
- Choose `Get*` when you genuinely need a random-access array of all paths; choose `Enumerate*` for large folders, streaming pipelines, or early-exit searches.

---

#### Q5. What does `Directory.CreateDirectory` do when intermediate folders already exist?

**Answer:** `Directory.CreateDirectory` creates the entire directory chain implied by the path and succeeds without error when some or all intermediate folders already exist, as long as the final path can be created or is already a directory. It returns a `DirectoryInfo` for the deepest directory in the path, whether newly created or pre-existing.

- Calling `CreateDirectory(@"C:\data\2026\reports")` creates `data`, `2026`, and `reports` if missing, or no-ops on segments that are already directories.
- If a path segment exists as a file rather than a directory, the call throws `IOException` because a file cannot serve as a parent folder.
- This idempotent behavior makes it safe to call at startup without a separate `Exists` check, though you still handle permission failures with `UnauthorizedAccessException`.

---

#### Q6. What is the difference between `File.Copy` with `overwrite: false` vs `overwrite: true`, and what exception indicates a conflict?

**Answer:** `File.Copy(source, dest, overwrite: false)` fails if the destination file already exists, while `overwrite: true` replaces the destination contents in place when it is already present. When `overwrite` is false and the destination exists, .NET throws `IOException` indicating the target file already exists.

- With `overwrite: false`, the source and destination must differ and the destination path must be absent — useful when accidental clobbering must be prevented.
- With `overwrite: true`, an existing destination file is truncated and rewritten; metadata on the destination may change depending on the operating system.
- Other failures — missing source (`FileNotFoundException`), missing parent directory (`DirectoryNotFoundException`), or permission denied (`UnauthorizedAccessException`) — are independent of the overwrite flag.

---

#### Q7. How does `File.Move` differ from copy-then-delete, and what happens to metadata and hard links?

**Answer:** `File.Move` asks the operating system to rename or relocate a file in one atomic step when source and destination are on the same volume, whereas copy-then-delete creates a new file, copies bytes, and only then removes the original — a window exists where both files exist and a crash can leave duplicates or partial copies. On the same volume, move typically preserves the file identity, inode or file index, creation time, and hard-link count; a copy creates a new file with fresh metadata.

- Cross-volume moves are implemented as copy-plus-delete internally on many systems, so they lose atomicity and may not preserve all metadata identically to an in-volume rename.
- `File.Move` fails if the destination already exists unless you use overloads that accept overwrite (available in newer .NET versions); plan for `IOException` on conflicts.
- Hard links point at the same underlying data; renaming via `Move` on the same volume keeps the link structure, while copying produces an independent file with its own link record.

---

#### Q8. What is `File.Replace`, and when is it preferable to manual backup-and-overwrite?

**Answer:** `File.Replace(sourceFileName, destinationFileName, destinationBackupFileName)` atomically replaces the contents of an existing destination file with a source file while optionally writing the previous destination to a backup path, giving you a single API for safe in-place updates. It is preferable to manual backup-and-overwrite when another process may read the destination and you want the swap to appear atomic at the filesystem level.

- A manual pattern — copy destination to `.bak`, copy source over destination, delete `.bak` on success — can leave readers seeing a half-written file if failure occurs mid-copy.
- `File.Replace` requires the destination to already exist; for brand-new targets use `File.Move` or `File.Copy` instead.
- Use it for configuration files, index files, or other shared resources where readers expect either the old or new complete version, never a torn write.

---

#### Q9. How do you safely delete a directory tree using `Directory.Delete(path, recursive: true)`?

**Answer:** Call `Directory.Delete(path, recursive: true)` only after confirming the path is correct — ideally an absolute path you constructed — and ensure no open handles (streams, editors, antivirus locks) hold files inside the tree, because the delete is immediate and irreversible. Wrap the call to handle `IOException` or `UnauthorizedAccessException` when files are locked, and prefer deleting a dedicated workspace or temp folder rather than user-supplied paths without validation.

- `recursive: false` deletes only empty directories; `recursive: true` removes all files and subfolders beneath the path.
- On Windows, an undisposed `FileStream` or `StreamWriter` inside the tree commonly causes sharing violations that surface as `IOException`.
- For user-facing features, consider moving to a recycle/trash stage or renaming to a quarantine folder before hard delete, because `Directory.Delete` bypasses the Recycle Bin.

---

#### Q10. What file metadata can you read via `File` static methods vs `FileInfo` instance properties?

**Answer:** Both surfaces expose creation time, last write time, last access time, and file attributes, but static methods take the path on each call while `FileInfo` exposes the same facts as instance properties alongside path-derived values like `Name`, `FullName`, `Extension`, `DirectoryName`, and `Length`. Functionally the timestamp and attribute data come from the same operating-system queries.

- Static: `File.GetCreationTime`, `GetLastWriteTime`, `GetLastAccessTime`, and `GetAttributes(path)` — each call hits the filesystem for that path.
- Instance: `CreationTime`, `LastWriteTime`, `LastAccessTime`, `Attributes`, and `Length` on a `FileInfo` constructed with the path.
- `FileInfo` also offers `IsReadOnly` as a convenience wrapper around the `ReadOnly` attribute flag.
- Neither API watches for external changes; refresh or construct a new `FileInfo` if another process may have modified the file since your last read.

---

#### Q11. Explain `File.GetCreationTime`, `GetLastWriteTime`, and `GetLastAccessTime` — and their `Utc` variants.

**Answer:** These methods return the operating-system timestamps for when a file was created, last modified, and last read, respectively, as `DateTime` values in local time for the non-`Utc` overloads and in Coordinated Universal Time (UTC) for the `Get*Utc` variants. Last write time is the most reliable indicator of content changes; creation time semantics vary by filesystem; last access time may be disabled or coarse-grained on some systems for performance.

- `GetLastWriteTime` / `GetLastWriteTimeUtc` — use when detecting whether a config or cache file changed and needs reloading.
- `GetCreationTime` / `GetCreationTimeUtc` — birth time on NTFS; on some Unix filesystems creation time is unavailable or approximated.
- `GetLastAccessTime` / `GetLastAccessTimeUtc` — updated when a file is read; Windows can disable access-time updates via policy, so do not rely on it for critical logic.
- Pair read and write APIs consistently — compare UTC to UTC — to avoid off-by-timezone bugs in distributed logs.

---

#### Q12. How do you set creation, last-write, and last-access timestamps programmatically?

**Answer:** Use the static setters `File.SetCreationTime`, `SetLastWriteTime`, and `SetLastAccessTime` (and their `Set*Utc` counterparts), or assign the corresponding properties on a `FileInfo` instance, passing the desired `DateTime` value. Setting timestamps does not change file contents; it only adjusts metadata records the filesystem stores alongside the file.

- `File.SetLastWriteTime(path, timestamp)` is common when extracting archived files and restoring original modification times for build-incremental tools.
- `FileInfo` allows `info.LastWriteTime = DateTime.UtcNow` with equivalent effect to the static setter for that path.
- Requires sufficient permissions; read-only files or files open with restrictive sharing may throw `UnauthorizedAccessException` or `IOException`.
- Some cloud sync or version-control tools overwrite timestamps after your set call — document that behavior for deployment scenarios.

---

#### Q13. What are `FileAttributes` (ReadOnly, Hidden, System, Archive)? How do you read and modify them?

**Answer:** `FileAttributes` is a flags enum describing filesystem metadata bits such as `ReadOnly`, `Hidden`, `System`, and `Archive`, which tell the operating system and tools how to treat a file. Read them with `File.GetAttributes(path)` or `FileInfo.Attributes`, and modify them by reading the flags, applying `|` or `& ~` to set or clear bits, then writing back with `File.SetAttributes` or assigning `FileInfo.Attributes`.

- `ReadOnly` blocks content modification through normal APIs until cleared — installers and editors respect it on Windows.
- `Hidden` and `System` affect Explorer visibility and special handling; misuse can confuse users or backup tools.
- `Archive` historically indicated backup eligibility; modern backup software may ignore it but the flag still exists on NTFS.
- Always read-modify-write flags rather than assigning a single flag blindly, or you may unintentionally clear other attribute bits the file already had.

---

#### Q14. What is the difference between `File.Exists` and attempting to open a file that may be deleted concurrently?

**Answer:** `File.Exists` performs a existence check at one instant and returns a boolean, but another thread or process can delete or create the file before your subsequent open — a time-of-check-time-of-use (TOCTOU) race — whereas opening the file attempts the operation atomically and throws `FileNotFoundException` if the file is absent at open time. For correctness under concurrency, prefer try-open over check-then-open.

- `if (File.Exists(path)) File.ReadAllText(path)` can still throw if the file disappears between the check and the read.
- Opening with `File.OpenRead(path)` or `new FileStream(..., FileMode.Open)` fails immediately with `FileNotFoundException` when the path is missing at that moment.
- `File.Exists` returning false does not distinguish "never existed" from "existed but deleted milliseconds ago" — only the open attempt reflects current reality.
- See Q19 for patterns that reduce TOCTOU risk in security-sensitive code.

---

#### Q15. What exceptions should you expect during file operations (`FileNotFoundException`, `DirectoryNotFoundException`, `IOException`, `UnauthorizedAccessException`)?

**Answer:** File I/O in .NET maps operating-system failures to a small set of managed exceptions: missing files raise `FileNotFoundException`, missing parent folders raise `DirectoryNotFoundException`, sharing conflicts and many generic I/O failures raise `IOException`, and permission or elevation problems raise `UnauthorizedAccessException`. Catch narrowly when you can recover; let unexpected failures propagate.

- `FileNotFoundException` — source path missing on open, copy, or move; also wrapped inner exceptions in some APIs.
- `DirectoryNotFoundException` — a directory segment in the path does not exist when creating or opening a file beneath it.
- `IOException` — file already exists (copy without overwrite), sharing violation (another process locked the file), disk full, or media errors.
- `UnauthorizedAccessException` — read-only media, insufficient ACL permissions, or attempting to write to a protected system path.
- `PathTooLongException` and `ArgumentException` appear for malformed or overlong paths before the OS is contacted.

---

#### Q16. How does `File.AppendAllText` differ from opening with `FileMode.Append`?

**Answer:** `File.AppendAllText` is a convenience method that opens the file, appends the string, and closes the handle in one call, internally using append mode, while opening with `FileMode.Append` via `FileStream` or `StreamWriter` gives you a long-lived handle for multiple append operations before you dispose it. Both seek to the end of the file before writing new bytes.

- `File.AppendAllText(path, text)` — best for logging a single line or small chunk without managing `using` yourself.
- `new FileStream(path, FileMode.Append, FileAccess.Write)` — use when many writes happen over time and you want to control encoding, buffering, or sharing flags explicitly.
- `StreamWriter(path, append: true)` adds text encoding and newline handling on top of an append-mode stream.
- Both create the file if it does not exist (for standard append overloads); specify encoding on `AppendAllText` overloads when UTF-8 without BOM is required.

---

#### Q17. When is `File.ReadAllBytes` / `File.WriteAllBytes` appropriate vs stream-based APIs?

**Answer:** `ReadAllBytes` and `WriteAllBytes` load or write the entire file into a single `byte[]` in one call, which is appropriate for small, bounded files such as icons, certificates, or short binary blobs, whereas stream-based APIs read or write in chunks and suit large or unbounded payloads. The all-bytes helpers allocate memory proportional to file size, so multi-gigabyte files require `FileStream` with a buffer loop.

- Use `ReadAllBytes` when you know the file fits comfortably in memory and you need a contiguous buffer for crypto, hashing, or image decoding.
- Use `FileStream` with a loop or `CopyTo` when streaming to network, hashing incrementally, or processing files larger than available RAM.
- `WriteAllBytes` replaces file contents atomically from the caller's perspective but still opens, writes, and closes — not a partial update API.
- For text, prefer `ReadAllText` or `StreamReader` instead of interpreting raw bytes manually unless the format is truly binary.

---

#### Q18. Explain `File.Create`, `File.Open`, `File.OpenRead`, and `File.OpenWrite` — what modes and access do they imply?

**Answer:** These helpers return a `FileStream` configured for common scenarios: `Create` truncates or creates with read-write access, `Open` opens an existing file with caller-specified mode and access, `OpenRead` opens existing files read-only with shared read, and `OpenWrite` opens or creates for write-only access starting at the beginning (truncating existing files). All require disposal via `using` to release locks promptly.

- `File.Create(path)` — `FileMode.Create`, `FileAccess.ReadWrite`, `FileShare.None`; truncates if the file exists.
- `File.Open(path, mode, access)` — full control; use `Open` mode for existing files, `OpenOrCreate` when either is acceptable.
- `File.OpenRead(path)` — read-only, `FileShare.Read`; throws if missing; ideal for parsers that never mutate.
- `File.OpenWrite(path)` — write-only, truncates existing content; does not open read-only existing files for read.
- Default sharing is often exclusive (`None`), which blocks other writers and sometimes readers — pass explicit `FileShare` in `File.Open` when concurrent read access is needed.

---

#### Q19. How do you handle TOCTOU (time-of-check-time-of-use) races when checking existence before read/write?

**Answer:** Avoid separate existence checks before I/O; instead attempt the open, read, or write inside try/catch and handle `FileNotFoundException` or `IOException`, or use operating-system APIs that open with create-if-missing semantics in one step. When you must validate paths from untrusted input, normalize with `Path.GetFullPath` against a known root and reject paths that escape the allowed directory before any operation.

- Replace `if (File.Exists(p)) Read` with `try { File.OpenRead(p) } catch (FileNotFoundException)`.
- Use `FileMode.CreateNew` when exclusive creation is required — the OS fails atomically if the file already exists.
- For security-sensitive temp files, create with random names via `Path.GetRandomFileName` in a private directory rather than predictable paths checked then written.
- File locking with appropriate `FileShare` reduces but does not eliminate races between processes — design idempotent operations where possible.

---

#### Q20. What is the difference between deleting a file and clearing its contents while keeping the path?

**Answer:** Deleting removes the directory entry so the path no longer resolves to a file, while clearing contents opens the file with truncate or overwrite semantics so the path remains valid but the byte length becomes zero (or a known new size). Clearing preserves open handles held by other processes on some systems and keeps inode identity for hard links; deletion breaks the path immediately for new open attempts.

- `File.Delete(path)` — path gone; subsequent `Exists` returns false.
- `File.WriteAllText(path, "")` or `new FileStream(path, FileMode.Truncate)` — path remains; metadata like creation time may be preserved depending on OS.
- Truncate-in-place is useful for log files that watchers monitor by path without re-creating the file.
- Other processes with handles open may continue reading/writing after delete on Unix until handles close — another reason to prefer truncate for shared logs when appropriate.

---

### 02. StreamReader & StreamWriter

#### Q1. Explain the `Stream` base class hierarchy and where `StreamReader`/`StreamWriter` fit.

**Answer:** `Stream` is the abstract base for byte-oriented I/O (`FileStream`, `MemoryStream`, network streams), and `StreamReader`/`StreamWriter` sit above it as text adapters that encode and decode characters through an underlying `Stream` or file path. They inherit from `TextReader` and `TextWriter`, which define the character-level read/write contract independent of storage backend.

- Byte layer: `Stream.Read`/`Write` move raw bytes; position and seeking apply when the backing stream supports it.
- Text layer: `StreamReader` converts bytes to strings using an `Encoding`; `StreamWriter` converts strings to bytes.
- You can stack `new StreamReader(fileStream, Encoding.UTF8)` to control sharing and mode on the `FileStream` while still using line-oriented text APIs.
- In-memory counterparts `StringReader`/`StringWriter` implement the same text bases without any `Stream` underneath.

---

#### Q2. What is the difference between `File.ReadAllText`, `File.ReadAllLines`, and `File.ReadLines`?

**Answer:** `ReadAllText` loads the entire file as one string, `ReadAllLines` loads every line into a `string[]` immediately, and `ReadLines` returns a lazy `IEnumerable<string>` that reads one line at a time during enumeration. The first two allocate memory proportional to full file size upfront; the third defers I/O until each line is consumed.

| Method | Returns | Memory | File handle |
|---|---|---|---|
| `ReadAllText` | Single `string` | Entire file | Open/close inside method |
| `ReadAllLines` | `string[]` | All lines at once | Open/close inside method |
| `ReadLines` | Lazy lines | One line at a time | Held until enumeration completes |

Use `ReadAllText` for small config files, `ReadAllLines` when you need random access to all lines in memory, and `ReadLines` or `StreamReader` for large logs.

---

#### Q3. Why can `File.ReadLines` hold a file lock until enumeration completes?

**Answer:** `File.ReadLines` opens the file and keeps an internal `StreamReader` alive for the lifetime of the returned enumerable, because lazy iteration must maintain an open read position between `MoveNext` calls. Until you finish the `foreach`, dispose the enumerator, or the enumerator is garbage-collected, other processes may be unable to delete or exclusively open the file on Windows.

- The lock is not held after `ReadAllLines` returns because that method reads everything and closes the handle before returning the array.
- Breaking out of a `foreach` early still requires disposal — prefer `using` patterns or fully consume lines when predictable.
- On Windows, an open reader commonly blocks `File.Delete` and rename on the same path; Linux behavior differs but concurrent replace can still fail.
- See Gotcha 1 for the contrast with `ReadAllLines`.

---

#### Q4. How do `StreamReader.ReadLine`, `ReadToEnd`, and `ReadBlock` differ for large files?

**Answer:** `ReadLine` returns the next line without a trailing newline and is ideal for line-oriented processing with bounded memory, `ReadToEnd` reads from the current position to end-of-stream into one string, and `ReadBlock` fills a caller-supplied `char[]` buffer with up to a requested count for chunked text processing. For large files, prefer `ReadLine` or `ReadBlock`; avoid `ReadToEnd` unless you accept full-file memory use.

- `ReadLine` — returns `null` at end-of-stream; each call performs I/O for the next line only.
- `ReadToEnd` — allocates one large string; equivalent to loading the remainder of the file from the current position.
- `ReadBlock` — returns the number of characters read; caller loops until zero is returned; useful for fixed-size chunk pipelines.
- `Peek` and `EndOfStream` help build parsers without over-reading.

---

#### Q5. What is the default encoding for `StreamReader` and `StreamWriter`, and why can that cause mojibake?

**Answer:** On modern .NET, `StreamReader` and `StreamWriter` default to UTF-8 without a byte order mark (BOM), but older code and some overloads still fall back to the system ANSI code page when encoding is omitted on certain constructors, which causes mojibake when the file was saved in a different encoding. Mojibake is garbled text that appears when bytes are decoded with the wrong character mapping.

- A UTF-8 file read as Windows-1252 shows accented characters as nonsense sequences.
- UTF-16 LE files without BOM detection may be misread as single-byte text.
- Always pass `Encoding.UTF8` explicitly in cross-platform apps, or detect BOM with `StreamReader` constructors that accept `detectEncodingFromByteOrderMarks: true`.
- Writer and reader encodings must match; mixing UTF-8 write with default read corrupts data silently.

---

#### Q6. How do you specify `Encoding.UTF8`, UTF-8 with BOM, and legacy encodings (`Encoding.GetEncoding`)?

**Answer:** Pass `Encoding.UTF8` to constructors for standard UTF-8 without BOM, use `new UTF8Encoding(encoderShouldEmitUTF8Identifier: true)` when consumers require a BOM prefix, and call `Encoding.GetEncoding("windows-1252")` or a code page number for legacy files. `StreamReader(path, Encoding)` and `StreamWriter(path, append, encoding)` overloads accept these explicitly.

- UTF-8 without BOM is the typical choice for JSON, logs, and Linux-oriented pipelines.
- UTF-8 with BOM helps Notepad and some Windows tools recognize encoding automatically.
- `Encoding.GetEncoding(1252)` retrieves Windows-1252; prefer registered names and handle `NotSupportedException` on platforms lacking the code page.
- `Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)` may be required on .NET Core and later for non-UTF code pages.

---

#### Q7. What does `StreamReader.DetectEncodingFromByteOrderMarks` control?

**Answer:** When `detectEncodingFromByteOrderMarks` is true (the default on common constructors), `StreamReader` inspects the first bytes for UTF-8, UTF-16 LE, or UTF-16 BE BOM markers and selects the matching encoding before decoding the remainder of the file. When false, it uses only the encoding you supplied without BOM sniffing.

- BOM detection prevents misreading UTF-16 files that contain null bytes in ASCII-looking regions.
- If no BOM is present, the reader falls back to the constructor's specified encoding.
- Disabling detection is appropriate when you must interpret BOM-less files strictly as UTF-8 regardless of accidental leading bytes.
- After detection, the reader consumes BOM bytes so they do not appear as characters in output.

---

#### Q8. Explain async read/write methods on `StreamReader`/`StreamWriter` (`ReadLineAsync`, `WriteLineAsync`, `ReadToEndAsync`).

**Answer:** Async methods such as `ReadLineAsync`, `WriteLineAsync`, and `ReadToEndAsync` perform I/O without blocking a thread pool thread while waiting for disk or network buffers, returning `Task` or `Task<string>` that completes when the operation finishes. They integrate with `async`/`await` in ASP.NET Core and other server code where thread scalability matters.

- Prefer async variants in request-handling code that reads upload streams or writes large responses.
- Async methods honor `CancellationToken` overloads where available to abort long reads during client disconnects.
- Small console tools gain little from async file I/O unless combined with other async work; synchronous APIs remain valid.
- Always `await` flush and dispose patterns — `await writer.WriteLineAsync` then `await writer.FlushAsync` before closing when inter-process readers need immediate visibility.

---

#### Q9. What is `StreamWriter.AutoFlush`, and when should you call `Flush()` explicitly?

**Answer:** `AutoFlush`, when set to true, calls `Flush` after every `Write` or `WriteLine`, pushing buffered characters to the underlying stream immediately at the cost of performance. Explicit `Flush()` is appropriate before another process must read partial output, before crash-sensitive logging, or when `AutoFlush` is false and you batch many writes for efficiency.

- Default `AutoFlush` is false; data may sit in an internal buffer until the buffer fills or the writer is disposed.
- Call `Flush()` after writing status markers that external watchdogs poll from disk.
- `FlushAsync` is the async counterpart for non-blocking pipelines.
- Disposing the writer flushes by default, but relying on finalization delays release and keeps file locks — always use `using`.

---

#### Q10. How do you append text to an existing file with `StreamWriter` (constructor overload with `append: true`)?

**Answer:** Construct `new StreamWriter(path, append: true)` or supply `append: true` with an explicit `Encoding`, which opens the file, seeks to the end, and writes new text after existing content without truncating. Each `WriteLine` appends a platform newline after the line text.

- Equivalent to `FileMode.Append` on an underlying `FileStream` with text encoding layered on top.
- Creates the file if it does not exist, matching `AppendAllText` behavior.
- Use the encoding overload when prior content was written as UTF-8 and you must stay consistent.
- Open once and write many lines in a loop rather than repeated `AppendAllText` calls when logging at high frequency — fewer open/close cycles.

---

#### Q11. What happens if you forget to dispose a `StreamWriter` — especially on Windows file locking?

**Answer:** An undisposed `StreamWriter` keeps its underlying `FileStream` handle open, which on Windows often prevents other processes from deleting, moving, or exclusively opening the file until the handle closes via dispose or garbage collection finalization. Finalizers run late under load, so locks can persist for seconds or minutes and appear as mysterious sharing violations.

- Always wrap streams in `using` statements or C# 8 `using` declarations so `Dispose` runs promptly.
- Flushing before dispose ensures buffered data reaches disk; dispose without flush may still flush depending on implementation.
- See Gotcha 2 — antivirus and indexers also hold locks, but undisposed writers are a common self-inflicted cause.
- Symptom: unit test cleanup fails to delete temp folder because a writer in the same test was not disposed.

---

#### Q12. Can you use `StreamReader`/`StreamWriter` with non-file streams (memory, network)? Give examples.

**Answer:** Yes — any `Stream` subclass can back a `StreamReader` or `StreamWriter`, so memory and network I/O use the same text APIs as files. Construct `new StreamReader(memoryStream)` or `new StreamWriter(networkStream, Encoding.UTF8)`.

```csharp
using MemoryStream ms = new MemoryStream();
using (StreamWriter w = new StreamWriter(ms, Encoding.UTF8, leaveOpen: true))
    w.WriteLine("payload");
ms.Position = 0;
using StreamReader r = new StreamReader(ms, Encoding.UTF8);
string line = r.ReadLine();
```

- `leaveOpen: true` prevents the writer from closing the shared `MemoryStream` when disposed.
- HTTP response bodies and `NamedPipeClientStream` follow the same pattern at the byte layer.
- APIs accepting `TextReader`/`TextWriter` become testable with `StringReader`/`StringWriter` without disk I/O.

---

#### Q13. What is the difference between `using` blocks and C# 8 `using` declarations for stream cleanup?

**Answer:** A `using` statement block disposes the resource at the closing brace of the block, while a C# 8 `using` declaration (`using StreamReader r = new(...)`) disposes at the end of the enclosing scope — typically the method — without extra indentation nesting. Both compile to equivalent `Dispose` calls on success and exception paths.

- `using (var r = new StreamReader(path)) { ... }` — classic, explicit scope boundary.
- `using StreamReader r = new StreamReader(path);` — less nesting for methods that read until return.
- Neither replaces `await using` for `IAsyncDisposable` types such as some async stream wrappers.
- Choose based on readability; disposal timing is end of scope in both cases.

---

#### Q14. How do you read a file line-by-line without loading it entirely into memory?

**Answer:** Use `foreach (string line in File.ReadLines(path))`, or open a `StreamReader` in a `using` and loop on `ReadLine()` until it returns null, processing each line before reading the next. Both approaches keep memory bounded to one line (plus buffers) regardless of total file size.

- `StreamReader` gives more control over encoding and underlying `FileShare` flags via a `FileStream` constructor chain.
- Process-and-discard patterns — parse, aggregate counts, write filtered output — never accumulate all lines in a `List<string>` unless required.
- For very wide lines, memory is proportional to line length; consider `ReadBlock` for fixed-width records instead.
- Combine with async `ReadLineAsync` in server scenarios processing large uploads line by line.

---

#### Q15. What is `TextReader`/`TextWriter`, and why do APIs often accept these abstractions?

**Answer:** `TextReader` and `TextWriter` are abstract base classes defining character I/O operations (`ReadLine`, `ReadToEnd`, `Write`, `WriteLine`, `Flush`) without binding to files, strings, or network streams. APIs accept them so callers can supply `StreamReader`, `StringReader`, or custom implementations interchangeably.

- A CSV parser taking `TextReader` runs against disk files in production and in-memory strings in unit tests without code changes.
- Polymorphism keeps business logic independent of storage location and eases mocking in tests.
- `StreamReader`/`StreamWriter` are the file-backed implementations; `StringReader`/`StringWriter` back in-memory scenarios.
- Binary data should use `Stream`, not `TextReader`, because text adapters apply encoding and newline semantics.

---

### 03. FileStream & Binary Files

#### Q1. What is the difference between `File`, `Stream`, and `FileStream`?

**Answer:** `File` is a static helper class for path-based convenience operations, `Stream` is the abstract byte-I/O base class shared by many backends, and `FileStream` is a concrete `Stream` implementation backed by a file on disk with explicit control over mode, access, sharing, and position. `File.ReadAllText` and similar helpers hide streams internally; `FileStream` exposes raw bytes and seeking.

- `File.Create` returns a `FileStream`; you rarely need both names in one workflow except when bridging high-level and low-level APIs.
- Other `Stream` types — `MemoryStream`, `GZipStream`, network streams — are not files but compose with the same read/write patterns.
- Choose `File` for one-shot text/byte helpers; choose `FileStream` when you need sharing flags, partial reads, or in-place patching.

---

#### Q2. Explain `FileMode` (`CreateNew`, `Create`, `Open`, `OpenOrCreate`, `Truncate`, `Append`) — when use each?

**Answer:** `FileMode` tells the operating system how to create or open the file handle: `CreateNew` fails if the file exists, `Create` truncates or creates, `Open` requires an existing file, `OpenOrCreate` opens or creates empty, `Truncate` opens and sets length to zero, and `Append` opens or creates and seeks to the end before every write.

| Mode | File missing | File exists |
|---|---|---|
| `CreateNew` | Creates | Throws `IOException` |
| `Create` | Creates | Truncates to empty |
| `Open` | Throws | Opens |
| `OpenOrCreate` | Creates | Opens |
| `Truncate` | Throws | Opens, length 0 |
| `Append` | Creates | Opens at EOF |

Use `CreateNew` for atomic exclusive creation, `Open` for read-only processing of known files, and `Append` for log writers.

---

#### Q3. Explain `FileAccess` (`Read`, `Write`, `ReadWrite`) and `FileShare` (`None`, `Read`, `Write`, `ReadWrite`, `Delete`).

**Answer:** `FileAccess` declares what your handle may do — read, write, or both — while `FileShare` declares what other concurrent handles may do while yours is open. Combining them lets readers coexist (`FileShare.Read`) or enforces exclusive access (`FileShare.None`).

- `FileAccess.Read` with `FileShare.Read` allows multiple readers, typical for config files consumed by several processes.
- `FileAccess.Write` with `FileShare.None` blocks everyone else — common for exclusive writers.
- `FileShare.Delete` (where supported) permits another process to delete the file while your handle remains open — platform-specific semantics apply.
- Mismatch between access and mode — opening with `Write` on a read-only media — fails at runtime with `UnauthorizedAccessException`.

---

#### Q4. Why does default `FileShare.None` cause sharing violations when another process needs read access?

**Answer:** `FileShare.None` requests exclusive access, so the operating system denies other processes from opening the file for read or write while your handle is open, which surfaces as `IOException` with a sharing violation message when a second process attempts access. Many `File.*` helpers and `FileStream` constructors default to exclusive sharing unless you specify otherwise.

- A log tail utility cannot read a file your service holds open with `FileShare.None`.
- Readers that should coexist need your writer to open with at least `FileShare.Read`.
- See Gotcha 3 — this is one of the most common Windows deployment issues between tools and services.
- Always dispose promptly to release shares; long-lived exclusive handles block maintenance tasks.

---

#### Q5. What are `FileStream.Position`, `Seek`, and `Length` — and when is seeking valid?

**Answer:** `Position` is the current byte offset in the stream, `Seek` moves that offset relative to `SeekOrigin.Begin`, `Current`, or `End`, and `Length` reports total stream size for seekable streams. Seeking is valid only when `CanSeek` is true — true for `FileStream` and `MemoryStream`, often false for network or pipe streams.

- After `Seek(0, SeekOrigin.Begin)`, the next `Read` starts at the first byte.
- `Length - Position` tells you how many bytes remain from the current location to end-of-file.
- Append-mode streams may ignore seeks before writes or reset position to EOF depending on platform rules — prefer explicit append constructors for logs.
- Always check `CanSeek` before random access on a `Stream` reference typed abstractly.

---

#### Q6. Explain `SeekOrigin` (`Begin`, `Current`, `End`) with a concrete read-modify-write scenario.

**Answer:** `SeekOrigin.Begin` offsets from byte zero, `Current` offsets from the present position, and `End` offsets from the end of the stream — negative offsets move backward from EOF. A read-modify-write on a binary record might read a header at `Seek(0, Begin)`, jump to a record at `Seek(recordIndex * recordSize + headerSize, Begin)`, patch bytes, then `Flush`.

- Patching a 32-bit integer at offset 100: `Seek(100, Begin)`, `Write(newBytes)`.
- `Seek(-4, SeekOrigin.End)` positions on the last four bytes for footer validation.
- `Seek(0, SeekOrigin.Current)` is a no-op; positive `Current` skips forward from the last read position.
- After seeking past EOF and writing, the file extends; bytes in the gap may be zero-filled — see Gotcha 8.

---

#### Q7. What happens if you seek on a non-seekable stream (e.g., some network streams)?

**Answer:** Calling `Seek` on a stream where `CanSeek` is false throws `NotSupportedException`, because forward-only streams such as HTTP response bodies or pipes deliver bytes sequentially and cannot rewind. You must read the data you need in one pass or buffer to a seekable `MemoryStream` first.

- Always guard with `if (stream.CanSeek)` before random access logic shared across file and network sources.
- `CopyTo` on a non-seekable input to `MemoryStream` enables subsequent seeking at the cost of memory.
- Some compressed streams allow forward-only decompression without seeking to arbitrary offsets.
- Design parsers for streaming when input may be non-seekable — do not assume `Length` is available.

---

#### Q8. What is the difference between `FileStream.Read`/`Write` and `ReadAsync`/`WriteAsync`?

**Answer:** Synchronous `Read` and `Write` block the calling thread until the operating system completes the I/O operation, while `ReadAsync` and `WriteAsync` return tasks that complete when I/O finishes without holding a thread during waits. Both may transfer fewer bytes than requested in a single call — always check return values.

- `Read` returns the count of bytes read; zero indicates end-of-stream.
- Async variants accept `CancellationToken` and `Memory<byte>` overloads on modern .NET for efficient buffer usage.
- Partial reads are normal — loop until the buffer is full or zero is returned.
- For purely local synchronous console tools, sync APIs are simpler; server and UI apps benefit from async.

---

#### Q9. What do `BinaryReader` and `BinaryWriter` add over raw `FileStream` byte operations?

**Answer:** `BinaryReader` and `BinaryWriter` wrap any `Stream` and provide typed helpers such as `ReadInt32`, `WriteDouble`, and `ReadString`, handling endianness and string length-prefix encoding so you do not manually shift bytes or track field sizes. They keep read and write order symmetric — mismatched order corrupts the file silently.

- Primitive writes use little-endian byte order on typical .NET platforms for numeric types.
- `Write(string)` / `ReadString()` use a 7-bit length prefix plus UTF-8 payload in default encoding configurations.
- `leaveOpen: true` keeps the underlying `FileStream` open when the reader/writer is disposed.
- Raw `FileStream` remains appropriate when copying opaque byte buffers without typed structure.

---

#### Q10. How does `BinaryReader` handle endianness and primitive types (`ReadInt32`, `ReadDouble`, `ReadString`)?

**Answer:** `BinaryReader` reads multi-byte numeric types in little-endian order on common platforms, converting bytes to CLR types with `BitConverter` semantics, and reads strings using the length-prefixed format written by matching `BinaryWriter` calls. `ReadDouble` consumes eight bytes as IEEE 754; `ReadInt32` consumes four bytes.

- Cross-platform binary files shared with big-endian systems require explicit endian conversion or a documented wire format.
- `ReadBoolean` reads one byte where zero is false and non-zero is true.
- `ReadString` must pair with `BinaryWriter.Write(string)` using the same `Encoding`.
- For heterogeneous files from non-.NET producers, verify layout with a hex dump rather than assuming `BinaryReader` defaults.

---

#### Q11. What is the on-disk format of `BinaryWriter.Write(string)` — and why does it matter for cross-platform files?

**Answer:** `BinaryWriter.Write(string)` writes a 7-bit-encoded length prefix followed by the string bytes in the writer's `Encoding` (UTF-8 by default on modern overloads), not a fixed-width or null-terminated C-style layout. Readers on other languages must implement the same length-prefix scheme to interoperate.

- Changing `Encoding` between write and read corrupts strings without throwing until lengths misalign.
- .NET Framework historically used UTF-8 in some overloads and Unicode in others — document the encoding per file format version.
- Fixed-width string fields for interop require manual `Write` of padded byte arrays instead of `Write(string)`.
- Version your binary format header so future readers reject incompatible string encodings early.

---

#### Q12. What is the difference between text and binary file handling in C#?

**Answer:** Text handling interprets bytes as characters through an `Encoding`, respecting newlines and human-readable formats, while binary handling reads and writes raw bytes with application-defined structure through `FileStream` and typed binary helpers. Every file is bytes on disk; the distinction is which API layer you use to interpret them.

| Aspect | Text | Binary |
|---|---|---|
| APIs | `StreamReader`, `File.ReadAllText` | `FileStream`, `BinaryReader` |
| Human readable | Yes | No |
| Numbers | Character digits `"42"` | Fixed-width binary `int32` |
| Typical use | Logs, CSV, JSON, config | Images, saves, custom protocols |

Choose text when people or line tools must read the output; choose binary for compactness, fixed layouts, or interoperability with byte protocols.

---

#### Q13. When should you use `MemoryStream` instead of `FileStream`?

**Answer:** Use `MemoryStream` when the entire payload fits in memory and you need an in-process seekable buffer — for example serializing to bytes before encryption, testing codecs without disk I/O, or buffering network chunks for random access. It backs a byte array growable on write without touching the filesystem.

- `MemoryStream.ToArray()` or `TryGetBuffer` extracts bytes after writing.
- Large payloads should stream with `FileStream` to avoid out-of-memory errors.
- `MemoryStream` is seekable (`CanSeek` true) even when wrapping a fixed buffer with `writable: false`.
- Common pattern: `JsonSerializer.SerializeAsync(memoryStream)` then upload bytes.

---

#### Q14. What is buffered I/O, and how do `FileStream` buffer size options affect performance?

**Answer:** Buffered I/O accumulates bytes in an internal buffer before issuing operating-system read or write syscalls, reducing syscall count and improving throughput for many small operations. `FileStream` constructors accept a buffer size parameter; larger buffers help sequential scans, while tiny random reads may not benefit.

- Default buffer sizes are tuned for general use; increase for bulk copy loops reading large files sequentially.
- `FileOptions.SequentialScan` hints sequential access; `RandomAccess` hints jumping — OS cache behavior may improve.
- `FileOptions.WriteThrough` bypasses OS write caching for durability-sensitive writes at a performance cost.
- `BinaryReader` adds its own buffering atop the underlying stream — avoid redundant small reads when possible.

---

#### Q15. How do you read a fixed header followed by variable-length records from a binary file?

**Answer:** Open a `FileStream`, wrap a `BinaryReader`, read the fixed header fields in documented order (magic bytes, version, record count), then loop `recordCount` times reading each record's fields — often a fixed ID followed by length-prefixed strings or nested length blocks. Validate magic and version before interpreting records to reject wrong files early.

- Magic bytes (file signature) at offset zero identify format — `"INV1"`, PNG header, etc.
- Variable-length records require reading a length integer before each payload chunk.
- Never trust file extension alone — validate header before allocation based on declared record counts (avoid denial-of-service via huge declared counts).
- Keep a schema document matching write order exactly; integration tests round-trip sample files.

---

#### Q16. What is a file signature (magic bytes), and how do you validate one without trusting the extension?

**Answer:** A file signature is a fixed byte sequence at the start of a file identifying the format, such as PNG's `89 50 4E 47` or a custom four-byte ASCII tag. Validate by reading the expected bytes with `BinaryReader.ReadBytes(n)` or `FileStream.Read` and comparing to the known pattern before parsing the remainder.

- Extensions are user-controlled and meaningless for security — `malware.exe` renamed to `.pdf` still fails PDF signature checks.
- After mismatch, throw a descriptive exception and do not attempt partial parsing.
- Some formats allow optional headers — document offset zero vs embedded signatures.
- Signature checks are cheap insurance before expensive deserialization or decompression.

---

### 04. Path & Environment Classes

#### Q1. What is the `Path` class, and why should you never hard-code `\` or `/` separators?

**Answer:** `Path` is a static helper for combining, parsing, and normalizing file path strings in an operating-system-aware way, using `Path.DirectorySeparatorChar` instead of literal backslash or forward slash. Hard-coded separators break cross-platform deployment because Linux and macOS use `/` while Windows accepts `\` and `/` in many APIs but not in all string-built paths passed to native tools.

- `Path.Combine("folder", "file.txt")` produces the correct separator for the running OS.
- Literal `"folder\\file.txt"` fails or mis-resolves when copied to Linux containers unchanged.
- UNC paths, drive roots, and tilde expansion have platform-specific rules `Path` encodes.
- See Gotcha 4 — cross-platform apps treat hard-coded separators as a defect.

---

#### Q2. How does `Path.Combine` behave with trailing slashes, rooted segments, and empty segments?

**Answer:** `Path.Combine` joins segments with the directory separator, ignoring empty segments in some overloads, and when a later segment is rooted (starts with a drive letter like `C:\` or a leading `/` on Unix), earlier segments are discarded and that rooted segment becomes the result base. Trailing slashes on intermediate segments are generally normalized but you should pass clean segment names.

- `Path.Combine("C:\\a", "b", "c.txt")` → `C:\a\b\c.txt` on Windows.
- `Path.Combine("ignored", "D:\\b")` → `D:\b` — the surprise case in Gotcha 6.
- Combining with an absolute second segment resets the path — intentional for override scenarios but dangerous if accidental.
- Empty string segments may be skipped depending on overload; avoid relying on empty parts for meaningful structure.

---

#### Q3. What is the difference between `Path.GetFullPath` and passing a relative path directly to `File.Open`?

**Answer:** `Path.GetFullPath` resolves a relative path against the current directory (or an explicit base) into an absolute path string before any file operation, while `File.Open` with a relative path lets the operating system resolve against `Environment.CurrentDirectory` implicitly at open time. `GetFullPath` lets you validate and display the resolved location first; both depend on the same current directory unless you anchor to `AppContext.BaseDirectory`.

- `GetFullPath` throws on invalid characters before I/O; useful for sanitizing user input paths.
- `File.Open("data\\file.txt")` resolves relative to current directory at open — which may differ from the executable folder.
- Prefer anchoring relative paths to `AppContext.BaseDirectory` for deployed apps, then `GetFullPath` from that root.
- See Gotcha 5 — Visual Studio sets CWD to the project folder; Windows Services often set CWD to `System32`.

---

#### Q4. Explain `Path.GetDirectoryName`, `GetFileName`, `GetFileNameWithoutExtension`, and `GetExtension`.

**Answer:** These methods parse a path string without touching the filesystem: `GetDirectoryName` returns the parent folder path, `GetFileName` returns the final segment, `GetFileNameWithoutExtension` strips the extension from that segment, and `GetExtension` returns the extension including the leading dot or empty string if none. They are pure string operations safe on non-existent paths.

- `GetFileName(@"C:\data\report.pdf")` → `"report.pdf"`.
- `GetFileNameWithoutExtension` → `"report"`.
- `GetExtension` → `".pdf"`.
- `GetDirectoryName` → `@"C:\data"`; returns `null` for root-only paths on some inputs — handle null when chaining.

---

#### Q5. What do `Path.GetTempPath`, `Path.GetTempFileName`, and `Path.GetRandomFileName` return — and what are the security implications of `GetTempFileName`?

**Answer:** `GetTempPath` returns the operating-system temp directory, `GetTempFileName` creates a uniquely named zero-byte file there and returns its full path, and `GetRandomFileName` returns a random file name without creating a file. `GetTempFileName` is predictable in older implementations and creates a world-readable file before you write secrets — prefer `GetRandomFileName` combined with `Path.Combine(GetTempPath(), name)` and exclusive creation.

- Temp directories may be shared among users on misconfigured systems — set appropriate ACLs on sensitive scratch data.
- Race attacks target predictable temp names — use `FileMode.CreateNew` after generating random names.
- `GetTempFileName` leaves empty files if your code crashes before delete — litter temp storage.
- Clean up temp files in `finally` blocks during export/import workflows.

---

#### Q6. How do `Path.IsPathRooted`, `HasExtension`, and `ChangeExtension` work?

**Answer:** `IsPathRooted` returns true when the path starts from a root (drive letter or leading directory separator), `HasExtension` checks whether the final segment contains a dot extension, and `ChangeExtension` replaces or removes the extension on the path string without verifying the file exists. They support validation and naming logic before I/O.

- Relative paths like `"data\\file.txt"` return false from `IsPathRooted` on Windows.
- `ChangeExtension("report.pdf", ".bak")` → `"report.bak"`.
- Passing `null` or empty string as new extension to `ChangeExtension` can remove the extension per API rules.
- Do not use `HasExtension` as a security MIME check — extensions are not content types.

---

#### Q7. What invalid path characters does `Path.GetInvalidPathChars` / `GetInvalidFileNameChars` expose?

**Answer:** These methods return character arrays of symbols forbidden in paths or file names on the current platform, such as `<`, `>`, `:`, `"`, `|`, `?`, `*` on Windows, helping you validate user-supplied names before creation. File-name invalid chars are a superset concern for the final segment only; path invalid chars apply to the full string.

- Reject or sanitize user filenames that contain invalid chars before `Path.Combine` with your storage root.
- Unix allows more characters than Windows — cross-platform apps should apply the stricter Windows set for compatibility.
- Control characters and trailing dots/spaces are problematic on Windows even if not in the invalid char arrays.
- Validation complements but does not replace checking for path traversal (`..` segments).

---

#### Q8. What is `Environment.SpecialFolder`, and how do you resolve `MyDocuments`, `ApplicationData`, and `LocalApplicationData`?

**Answer:** `Environment.SpecialFolder` is an enum of well-known user and system folders, resolved through `Environment.GetFolderPath` to absolute paths that differ by operating system and user profile. `MyDocuments` is the Documents folder, `ApplicationData` is roaming app data, and `LocalApplicationData` is machine-local app data that does not roam with the user profile.

- `Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)` — settings that should roam in domain environments.
- `LocalApplicationData` — caches, temp exports, machine-specific state.
- Pass `Environment.SpecialFolderOption.Create` when the folder must exist and should be created if missing.
- Never hard-code `C:\Users\...` — special folders move with folder redirection and non-English profiles.

---

#### Q9. How does `Environment.GetFolderPath` differ from hard-coding `C:\Users\...`?

**Answer:** `GetFolderPath` asks the operating system for the correct per-user, per-OS location of special folders, while hard-coded `C:\Users\...` breaks on non-Windows systems, localized profile paths, redirected folders, and service accounts without interactive profiles. It is the supported way to store user-scoped application data portably.

- Corporate folder redirection moves Documents without changing your API call — hard-coded paths break.
- Linux and macOS have different home directory layouts — special folder APIs map appropriately when available.
- Windows services running as `LOCAL SERVICE` may not have all special folders — handle empty or inaccessible paths.
- Combine with `Path.Combine` to append your company and product subfolders beneath the resolved root.

---

#### Q10. What is `Environment.CurrentDirectory`, and how can it differ from the executable's location?

**Answer:** `Environment.CurrentDirectory` is the process working directory used to resolve relative paths, which is set at launch by the host, shell, or service manager and can change at runtime via `Directory.SetCurrentDirectory`. It often differs from the folder containing the executable (`AppContext.BaseDirectory`), especially for Windows Services, scheduled tasks, and `dotnet run` from another folder.

- Visual Studio debugging typically sets current directory to the project output folder — masking deployment bugs.
- A Windows Service started from `System32` resolves relative `"config.json"` beside System32, not beside your `.exe`.
- Prefer `AppContext.BaseDirectory` or `AppDomain.CurrentDomain.BaseDirectory` for content files shipped next to the app.
- Document which paths are relative to CWD versus install directory in deployment guides.

---

#### Q11. How do you get the application base directory in modern .NET (`AppContext.BaseDirectory`, `AppDomain.CurrentDomain.BaseDirectory`)?

**Answer:** `AppContext.BaseDirectory` returns the directory containing the main application assembly or `.dll` being executed, and `AppDomain.CurrentDomain.BaseDirectory` provides a similar base path in most hosting scenarios. Use either to locate `appsettings.json`, SQLite files, or other content copied to the output directory.

- For single-file publish, base directory may extract to a temp folder — test publish layout separately from F5 debug.
- ASP.NET Core content roots use `IWebHostEnvironment.ContentRootPath` — not always identical to base directory.
- Combine with `Path.Combine(AppContext.BaseDirectory, "Data", "seed.csv")` for read-only bundled assets.
- Avoid assuming base directory equals current directory — they serve different resolution purposes.

---

#### Q12. What is the difference between absolute and relative paths in console apps vs ASP.NET Core?

**Answer:** Relative paths resolve against `Environment.CurrentDirectory`, which console hosts often set near the executable but ASP.NET Core sets to the content root, while absolute paths ignore CWD entirely. Web apps deployed to IIS or containers may have CWD and content root differ from where binaries live.

- Console: relative `"logs\\app.log"` follows CWD — set CWD explicitly or use absolute paths from base directory.
- ASP.NET Core: use `IWebHostEnvironment.ContentRootPath` or `WebRootPath` for file storage intended to survive deployment.
- Uploaded files should land in configured absolute paths outside web root when possible.
- Never serve user uploads via relative path concatenation without root validation.

---

#### Q13. How do UNC paths (`\\server\share`) interact with `Path.Combine` and `Path.GetFullPath`?

**Answer:** UNC paths are rooted network paths starting with `\\server\share`, and `Path.Combine` treats a subsequent rooted segment (including UNC) as resetting the combined result, while `GetFullPath` can normalize relative trailing segments against a UNC base when provided. UNC semantics require the share to be reachable before file APIs succeed.

- `Path.Combine(@"\\fileserver\exports", "2026", "report.csv")` builds under the share.
- Passing a drive-rooted segment after a UNC segment discards earlier parts — same rooted-segment rule as local drives.
- Authentication and firewall issues surface as `IOException` or `UnauthorizedAccessException` at open time, not at combine time.
- On Linux, UNC may require mount points rather than Windows-style `\\` paths — deployment differs by platform.

---

#### Q14. What cross-platform path differences matter when deploying the same code on Windows and Linux?

**Answer:** Windows uses drive letters and backslash separators with case-preserving but often case-insensitive lookups; Linux uses `/`-rooted paths with case-sensitive filesystems and no drive letters. Code must use `Path.Combine`, avoid hard-coded roots, test case sensitivity, and respect differing invalid character sets.

- `"Config.json"` and `"config.json"` collide on Linux but may not on Windows NTFS default.
- Leading `/` on Linux is absolute; on Windows `/` at start can mean current drive root.
- Line endings in text files differ — use `Environment.NewLine` or explicit `\n` for interchange formats.
- Docker volumes mount Linux paths — Windows-style paths inside containers fail unless explicitly mapped.

---

### 05. Working with CSV and Text Files

#### Q1. Why is there no built-in CSV parser in the BCL, and what libraries are commonly used?

**Answer:** The Base Class Library (BCL) provides general text I/O but no dedicated Comma-Separated Values (CSV) parser because CSV variants, dialects, and enterprise rules differ widely, so Microsoft left specialized parsing to NuGet packages such as CsvHelper, while you can implement quote-aware parsing for simple cases manually. JSON and XML have first-class serializers; CSV remains a bring-your-own-parser format.

- CsvHelper maps rows to objects with attributes and handles headers, culture, and bad data policies.
- Manual parsing suffices for internal tools with controlled export formats and no embedded commas in fields.
- RFC 4180 defines a common subset but real spreadsheets deviate — libraries expose configuration for delimiters and quoting.
- For large files, streaming row readers beat loading entire files into memory regardless of library.

---

#### Q2. What are RFC 4180 rules for CSV fields, delimiters, and record terminators?

**Answer:** RFC 4180 describes CSV as fields separated by commas, rows terminated by carriage return and line feed (`CRLF`), optional header rows, and fields containing commas or quotes wrapped in double quotes with internal quotes doubled. It is the interchange baseline many exporters follow, though not every spreadsheet obeys it strictly.

- Delimiter is comma; tab-separated values are a different dialect requiring different configuration.
- Record terminator is `\r\n` per RFC, though Unix `\n`-only files are common in practice.
- The last row may or may not end with a newline — parsers should tolerate both.
- MIME type `text/csv` references this family of formats for HTTP downloads.

---

#### Q3. When must a CSV field be wrapped in double quotes?

**Answer:** A field must be quoted when it contains the delimiter (comma), a double quote character, or a line break, so parsers treat the entire quoted span as one column value. Fields with only ordinary text without those characters may be unquoted.

- `"Acme, Inc"` — comma inside the company name requires quotes.
- `"Line1\nLine2"` — embedded newline inside a quoted field is one logical row continuation in full RFC parsers.
- Leading or trailing spaces may be significant inside quotes; trimming policies are dialect-specific.
- Numeric fields are usually unquoted unless your downstream tool requires quotes for type coercion.

---

#### Q4. How do you escape a literal double quote inside a quoted CSV field?

**Answer:** Double the quote character — represent one `"` as `""` inside a quoted field — so the parser knows the quote is data, not the closing delimiter. Backslash escaping (`\"`) is not RFC 4180 standard and breaks strict importers.

- `"12"" wrench"` represents the value `12" wrench`.
- When building CSV manually, replace `"` with `""` before wrapping the field in outer quotes.
- See Gotcha 10 — `\"` produces columns that shift on import into Excel or strict parsers.
- CsvHelper and similar libraries apply this rule automatically on write.

---

#### Q5. What goes wrong if you split CSV lines on `Split(',')` without a proper parser?

**Answer:** Naive comma splitting treats every comma as a column boundary, so quoted fields containing commas split into extra columns, shifting all downstream values and corrupting typed parsing. `"Smith, Jr.",42` becomes three tokens instead of two.

- Headers no longer align with data columns — deserialization maps wrong fields to properties silently or throws obscure parse errors.
- See Gotcha 9 — this is the most common CSV bug in hand-rolled importers.
- Simple `Split` also fails on embedded newlines inside quotes and on escaped quotes.
- Use a quote-aware state machine, CsvHelper, or restrict inputs to guaranteed comma-free fields only.

---

#### Q6. How do you handle embedded newlines inside quoted CSV fields?

**Answer:** A standards-aware parser stays inside quote mode across `\r` and `\n` characters until the closing quote, treating the entire multiline span as one field value. Line-based `ReadLine` loops without quote state break records prematurely at internal newlines.

- Read character-by-character or use a library that implements RFC field state machines.
- `File.ReadLines` sees physical lines, not logical CSV records — insufficient alone for multiline fields.
- Exporters must quote fields that contain newlines and double internal quotes.
- Validate record shape after assembling logical records, not after each physical line.

---

#### Q7. What issues arise with culture-specific decimal separators in CSV numeric columns?

**Answer:** Thread current culture formats decimals with locale-specific separators — for example `12,99` in German versus `12.99` in invariant English — so writing numbers with `ToString()` without culture and reading with the wrong `CultureInfo` mis parses amounts. Financial CSV interchange should standardize on `CultureInfo.InvariantCulture` for numeric columns.

- `decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)` on read.
- `price.ToString(CultureInfo.InvariantCulture)` on write.
- Excel regional settings may display imported numbers incorrectly even when bytes are correct — document expected format for partners.
- Dates have the same problem — prefer ISO 8601 strings for portable CSV date columns.

---

#### Q8. How do you write CSV headers and ensure stable column ordering for downstream consumers?

**Answer:** Emit a header row as the first record with fixed column names in a documented order, and write data rows using the same column sequence every time — often by iterating a defined column list or ordered property metadata rather than reflection order, which is unstable. Stable ordering lets downstream ETL mappings remain valid across releases.

- Define `const string Header = "Sku,Name,Quantity,UnitPrice"` matching your writer loop order.
- Adding new columns should append to the right with versioned documentation rather than reordering existing columns.
- Include a format version column when evolving schemas so consumers branch parsing logic.
- CsvHelper class maps can pin column ordinals or names explicitly.

---

#### Q9. What is the difference between `\n`, `\r\n`, and `Environment.NewLine` for text file line endings?

**Answer:** `\n` (line feed) terminates lines on Unix and modern macOS, `\r\n` (carriage return plus line feed) terminates lines on Windows, and `Environment.NewLine` returns the convention for the current platform. Mixing endings in one file confuses tools that assume a single style.

- Git `autocrlf` settings affect working tree line endings on Windows clones.
- Log aggregation systems normalize endings when indexing — still prefer consistency at write time.
- CSV RFC specifies `\r\n` between records; many Unix exports use `\n` only and remain widely accepted.
- `StreamWriter.WriteLine` uses `Environment.NewLine` automatically.

---

#### Q10. How do you normalize line endings when reading files produced on Windows vs Linux?

**Answer:** Read content and replace `\r\n` and standalone `\r` with your target newline, or split on `\r\n`, `\n`, or `\r` using `StringSplitOptions` and rejoin with the desired separator, being careful not to corrupt quoted CSV fields that legitimately contain newlines. For whole-file text loads, `ReadAllText` followed by normalization works; CSV requires parser-aware handling.

- Simple text: `content.Replace("\r\n", "\n").Replace("\r", "\n")` for Unix-normal form.
- Do not normalize inside quoted CSV fields without a CSV-aware parser.
- `StreamReader` does not strip `\r` from `\r\n` automatically in all scenarios — test with sample files.
- Store normalized form only if downstream requires it; keep originals for audit when needed.

---

#### Q11. What are best practices for large CSV ingestion (streaming vs loading all rows)?

**Answer:** Stream with `StreamReader` line-by-line or use a forward-only CSV reader, validate and parse each row, and avoid accumulating all rows in a `List<T>` unless memory bounds are known. Report row numbers in errors, skip or quarantine bad rows according to policy, and apply backpressure when writing to databases in batches.

- `ReadAllLines` on a gigabyte file allocates an enormous array — unacceptable for server imports.
- Batch insert every N validated rows to balance transaction size and memory.
- Use async I/O when reading from network streams attached to uploads.
- Monitor column count and types per row before object materialization to fail fast on malformed lines.

---

#### Q12. How do you validate CSV row shape (column count) before deserializing to objects?

**Answer:** After parsing a logical row into fields, compare `fields.Count` to the expected column count derived from the header before mapping to properties, and reject or log rows that mismatch. Header-driven validation catches shifted columns from quoting bugs or truncated lines early.

- Parse header once, store expected count and optional name-to-index map.
- `"W-100,Widget,10,9.99"` missing a trailing column yields four fields when five expected — skip with row number in log.
- Trailing comma exports produce an extra empty trailing field — decide whether your schema allows it.
- Never map by positional index alone without count check when data quality is uncertain.

---

#### Q13. When should you use fixed-width text formats instead of CSV?

**Answer:** Fixed-width columns suit legacy mainframe feeds and reports where every field occupies a known character width and delimiters are absent, eliminating comma-in-text problems at the cost of rigid padding rules. Choose fixed-width when partners specify COBOL-style layouts or when delimiter collisions are unavoidable in free text.

- Parse with `Substring` offsets and lengths documented in a layout spec.
- Pad numeric fields with leading zeros and text fields with trailing spaces to defined widths on write.
- Validation is positional — column shift is detected by length mismatch, not delimiter count.
- CSV is usually simpler for new integrations unless a legacy contract mandates fixed-width.

---

#### Q14. How do you properly dispose file resources across layered readers (`FileStream` → `StreamReader`)?

**Answer:** Wrap the outermost consumer in `using`, or dispose inner streams first by constructing `StreamReader` with `leaveOpen: false` (default) so disposing the reader closes the underlying `FileStream` automatically. Only one disposal path should own the base stream to avoid double-dispose or leaked handles.

- Typical pattern: `using FileStream fs = ...; using StreamReader sr = new StreamReader(fs);` — disposing `sr` closes `fs`.
- `leaveOpen: true` when a higher layer must outlive the reader — rare; document ownership clearly.
- Do not finalize without dispose — Windows locks persist (Gotcha 2).
- Async disposal: `await using StreamReader sr = new StreamReader(path);` on modern C#.

---

#### Q15. What logging and rotation patterns apply when appending to text log files over time?

**Answer:** Append with `StreamWriter(append: true)` or `File.AppendAllText` for small logs, and implement rotation by size or date — rename `app.log` to `app.log.2026-08-20` and start a fresh file when thresholds hit — so disks do not fill indefinitely. Use `Flush` or `AutoFlush` when external tail tools must see lines immediately after write.

- Size-based rotation: when `FileInfo.Length` exceeds N megabytes, close, rename, open new file.
- Date-based rotation: new file at midnight UTC for operational correlation.
- Structured logging frameworks (Serilog, NLog) handle sinks and rotation — prefer them over ad-hoc append in production.
- Exclusive sharing locks may block rotation while the writer is open — coordinate dispose before rename.

---

### Gotchas — Module 07

#### Gotcha 1. **`ReadAllLines` vs `ReadLines`**

**Answer:** `ReadAllLines` loads the entire file into a `string[]` before returning, while `ReadLines` defers reading until enumeration but keeps the file open until the enumerator finishes or is disposed. Treating them as interchangeable causes memory spikes or file-lock surprises.

- Large logs with `ReadAllLines` can trigger out-of-memory errors where `ReadLines` would stream safely.
- Early exit from `foreach` over `ReadLines` still requires disposal to release the lock promptly on Windows.
- Use `ReadAllLines` only when you need the full array in memory and the file size is bounded.

---

#### Gotcha 2. **Undisposed streams lock files on Windows**

**Answer:** A finalized-but-not-disposed `FileStream` or `StreamWriter` can keep an exclusive or sharing handle open, blocking deletes, renames, and sometimes reads until garbage collection finalizers run, which happens late under load.

- Symptom: test cleanup or deploy script cannot delete a temp folder because a writer in the test was not wrapped in `using`.
- Always dispose explicitly; do not rely on the finalizer as a disposal strategy.
- Antivirus scanning compounds the issue — still fix your handles first.

---

#### Gotcha 3. **`FileShare` defaults to exclusive access**

**Answer:** Opening a `FileStream` without specifying share flags often requests exclusive access, preventing concurrent readers or writers in other processes until your handle closes. Production services and desktop tools then collide with sharing violations.

- Open log files for read with `FileShare.ReadWrite` when tail tools must read while you append.
- Document sharing requirements when multiple processes access the same path.
- Disposing promptly releases shares for maintenance operations.

---

#### Gotcha 4. **Hard-coded path separators break cross-platform**

**Answer:** String literals with `\\` embed Windows separators that fail or misbehave on Linux deployments, while `Path.Combine` selects the correct separator for the running operating system. Container and CI builds on Linux expose this immediately.

- Code reviews should flag `"folder\\file"` outside platform-specific interop layers.
- `Path.Combine` and `Path.DirectorySeparatorChar` are the portable replacements.
- Forward slashes often work on Windows APIs but relying on them instead of `Path` is inconsistent with cross-platform testing.

---

#### Gotcha 5. **Relative paths depend on `CurrentDirectory`**

**Answer:** A relative path that works under Visual Studio debugging may fail as a Windows Service, scheduled task, or cron job because `Environment.CurrentDirectory` differs from the project folder. The path string is the same; the resolution base is not.

- Anchor content paths to `AppContext.BaseDirectory` or explicit configuration.
- Log both CWD and resolved full path when diagnosing deployment file-not-found errors.
- ASP.NET Core uses content root — console assumptions do not transfer unchanged.

---

#### Gotcha 6. **`Path.Combine` with an absolute second segment discards earlier parts**

**Answer:** `Path.Combine("C:\\a", "D:\\b")` yields `D:\b` because a rooted later segment resets the combination, which surprises developers expecting concatenation under the first root. Accidental absolute segments from user input can redirect output to unintended drives.

- Validate user-supplied segments with `Path.IsPathRooted` and reject or strip roots when inappropriate.
- Build trusted paths by combining a known root with sanitized relative child names only.
- Unit test combine behavior when accepting configurable subpaths.

---

#### Gotcha 7. **Encoding mismatch silently corrupts text**

**Answer:** Assuming UTF-8 when a file is Windows-1252 or UTF-16 produces mojibake without throwing, because decoders interpret bytes as characters under the wrong mapping. Explicit `Encoding` on readers and writers prevents silent corruption.

- Specify encoding on both write and read sides of long-lived file formats.
- Enable BOM detection when consumers may emit BOM-prefixed UTF-16 exports.
- Add encoding to format documentation for partner file exchanges.

---

#### Gotcha 8. **Seeking past EOF then writing extends the file with undefined gap bytes**

**Answer:** Seeking beyond the current end and writing inserts a gap that the filesystem may zero-fill or leave sparse depending on platform, producing unexpected null bytes in the middle of a binary file. Patching formats must account for hole behavior when resizing records in place.

- Prefer rewrite-from-scratch or append-only layouts when hole semantics are unacceptable.
- Document whether your format allows sparse regions for security reviewers.
- Validate file length after in-place patches in integration tests.

---

#### Gotcha 9. **CSV `Split(',')` breaks on quoted commas**

**Answer:** Splitting on commas without quote awareness turns `"Smith, Jr.",42` into three fields instead of two, shifting every subsequent column in the row. The bug often appears only on production data with company names or addresses containing commas.

- Use CsvHelper or a quote-aware parser for any externally sourced CSV.
- Add row-level column count validation against the header before mapping to objects.
- Restrict manual split to provably simple internal exports only.

---

#### Gotcha 10. **Double-quote escaping in CSV is `""` not `\"`**

**Answer:** RFC 4180 requires doubling quotes inside quoted fields; backslash-quote sequences are not standard and confuse Excel, strict parsers, and partner importers. Wrong escaping produces ragged column counts on the next import.

- When hand-building CSV, implement `value.Replace("\"", "\"\"")` before wrapping in quotes.
- Audit exporter code that uses JSON-style escaping habits on CSV output.
- Test round-trip with the same parser your consumers use.

---

## Module 08. Advanced C# Features

### 01. Serialization & Deserialization

#### Q1. What is serialization and deserialization, and what is an object graph?

**Answer:** Serialization converts an in-memory object graph — the network of objects linked by references and collections — into a storable or transmittable wire format such as JSON, XML, or bytes. Deserialization reads that payload and constructs a new object graph with fresh instances populated from the data.

- The wire format is independent of process memory layout, enabling REST APIs, message queues, and file persistence.
- An object graph includes the root object plus nested objects, lists, and dictionaries reachable from it.
- Serializers decide which members cross the boundary; unmarked or ignored members stay local.
- Round trip means serialize then deserialize and compare meaningful data — not necessarily identical object identity.

---

#### Q2. Does deserialization resurrect original object identity or create new instances?

**Answer:** Deserialization always creates new instances on the receiving side; it never resurrects the original heap objects or preserves reference identity from the sender's process. Any `ReferenceEquals` relationship from before serialization is lost unless the format explicitly preserves object references with metadata ids.

- Two references to the same object before serialization may become two separate equal objects after deserialization unless reference preservation is configured.
- Event handlers, database entity keys, and live connections cannot be faithfully restored from wire data alone.
- Identity-sensitive code must re-establish relationships after deserialization explicitly.
- See Q13 for `ReferenceHandler` options in System.Text.Json.

---

#### Q3. Compare JSON, XML, and binary as wire formats — trade-offs for APIs, config, and storage.

**Answer:** JSON is compact, human-readable, and dominant for HTTP APIs; XML is verbose but schema-rich and common in legacy enterprise integration; binary formats are smallest and fastest but opaque and tightly coupled to type layout unless you design a versioned protocol.

| Format | Readability | Schema | Typical use |
|---|---|---|---|
| JSON | High | Informal / JSON Schema | REST APIs, config, logs |
| XML | Medium | XSD, namespaces | SOAP, legacy config, documents |
| Binary | Low | Application-defined | Caches, game saves, high-throughput IPC |

JSON balances interoperability and tooling; XML when XSD validation or mixed content matters; binary when size and speed dominate and both endpoints share format rules.

---

#### Q4. Explain `System.Text.Json.JsonSerializer.Serialize` and `Deserialize` for files and streams.

**Answer:** `JsonSerializer.Serialize` converts an object to a JSON string or UTF-8 bytes, and `Deserialize<T>` parses JSON back into type `T`, with overloads accepting `Stream`, `ReadOnlySpan<byte>`, and async variants for network and file pipelines. They are the default JSON stack in modern ASP.NET Core.

- `JsonSerializer.Serialize(person)` → string; `SerializeToUtf8Bytes` avoids string allocation for HTTP bodies.
- `Deserialize<Person>(json)` throws `JsonException` on malformed JSON; validate inputs in API boundaries.
- `SerializeAsync` / `DeserializeAsync` stream to and from files without loading entire payloads as strings.
- Pass a shared `JsonSerializerOptions` instance for consistent naming and converters across calls.

---

#### Q5. What is `JsonSerializerOptions`, and which settings affect naming, indentation, and enum handling?

**Answer:** `JsonSerializerOptions` centralizes serializer behavior: `PropertyNamingPolicy` (such as camelCase), `WriteIndented` for pretty printing, `DefaultIgnoreCondition` for omitting nulls, and `Converters` for custom type handling including enums as strings. One configured instance should be reused rather than recreated per call.

- `PropertyNamingPolicy = JsonNamingPolicy.CamelCase` maps `YearOfBirth` to `yearOfBirth` on the wire.
- `WriteIndented = true` aids debugging; disable in production responses to save bytes.
- Add `JsonStringEnumConverter` to `Converters` for string enum values globally.
- `PropertyNameCaseInsensitive = true` relaxes deserialization matching for incoming JSON keys.

---

#### Q6. Why is creating a new `JsonSerializerOptions` on every call a performance problem?

**Answer:** Constructing `JsonSerializerOptions` rebuilds internal converter caches and reflection metadata each time, adding CPU and allocation overhead on hot paths such as per-request API serialization. The options object is designed to be created once and shared read-only across threads after configuration.

- ASP.NET Core registers options in dependency injection once at startup for this reason.
- Mutating a shared instance after sharing is unsafe — configure fully before first use.
- Source-generated contexts (`JsonSerializerContext`) reduce reflection cost further for known types.
- See Gotcha 7 — concurrent mutation of shared options causes race bugs.

---

#### Q7. When should you use `JsonSerializerContext` source generators vs reflection-based serialization?

**Answer:** Use source-generated `JsonSerializerContext` when you need faster startup, ahead-of-time (AOT) compatibility, or trimming-safe serialization without runtime reflection over your types. Reflection-based serialization is simpler for exploratory code and dynamically discovered types at the cost of metadata and trim warnings.

- Native AOT and trimmed apps require source generation for types you serialize — reflection may fail at runtime.
- `[JsonSerializable(typeof(Person))]` on a partial context class generates serializers at compile time.
- Reflection remains acceptable for admin tools and few-type internal utilities not published trimmed.
- Hybrid apps register source-generated contexts for hot types and fall back rarely for plugin types.

---

#### Q8. Explain `[JsonPropertyName]`, `[JsonIgnore]`, `[JsonInclude]`, and `[JsonPropertyOrder]`.

**Answer:** These attributes control wire mapping without renaming C# members: `JsonPropertyName` sets the JSON key, `JsonIgnore` omits a member, `JsonInclude` opts non-public members into serialization, and `JsonPropertyOrder` influences serialization order for readability or diff stability.

- `[JsonPropertyName("nickname")]` maps a C# `Nickname` property to `"nickname"` regardless of naming policy.
- `[JsonIgnore]` excludes secrets, computed properties, or circular navigation properties from payloads.
- `[JsonInclude]` on a private field includes it when building immutable types intentionally.
- Order attributes matter for human-readable diffs, not JSON semantic equality.

---

#### Q9. What happens when JSON contains properties not present on the C# type (extra members)?

**Answer:** By default System.Text.Json ignores extra JSON properties that have no matching CLR member, so forward-compatible API clients can send new fields older servers skip without error. Strict modes or custom converters can change that behavior when unknown members must fail validation.

- Older services remain compatible when clients add optional metadata fields first.
- To reject unknown members, implement validation with `JsonNode` or a strict schema validator.
- Typos in expected property names deserialize as missing, not as extra — see missing-member gotchas for value types.
- Document versioning strategy: additive JSON fields are safer than renaming existing keys.

---

#### Q10. What happens when JSON is missing a property mapped to a non-nullable reference type vs a value type?

**Answer:** Missing reference-type properties deserialize to `null` even when annotated as non-nullable reference types (NRT), because JSON cannot distinguish absent from null without extra validation — analyzers warn but runtime does not enforce. Missing value-type properties deserialize to default values (`0`, `false`) silently, which can hide data errors.

- `string Name` absent in JSON → `null` at runtime; NRT compile warnings do not throw.
- `int Count` absent → `0` without error — dangerous for counters and enums unless validated.
- Use `required` properties (C# 11+), `[JsonRequired]`, or custom validation for critical fields.
- See Gotcha 5 for production impact on non-nullable value types.

---

#### Q11. How do enums serialize by default in `System.Text.Json`, and what production risk does numeric enum wire format create?

**Answer:** By default enums serialize as their underlying numeric values (`0`, `1`, `2`), not as names, which breaks persisted JSON when enum members are reordered or renumbered in a later release. Clients and databases storing numeric values couple tightly to declaration order.

- Inserting a new enum member in the middle shifts subsequent numeric values and corrupts stored data.
- String enums decouple wire names from numeric backing values when names stay stable.
- Flags enums serialize as combined integers — document bitmask semantics for consumers.
- See Gotcha 6 — prefer string enums for long-lived contracts.

---

#### Q12. How do you serialize enums as strings using `JsonStringEnumConverter`?

**Answer:** Add `JsonStringEnumConverter` to `JsonSerializerOptions.Converters` or apply `[JsonConverter(typeof(JsonStringEnumConverter))]` on the enum or property, so values appear as `"Active"` instead of `0`. Combine with `[JsonPropertyName]` on enum members when wire names differ from C# identifiers.

```csharp
var options = new JsonSerializerOptions();
options.Converters.Add(new JsonStringEnumConverter());
```

- String enums improve readability in logs and manual debugging of API payloads.
- Deserialization accepts names case-sensitively by default — configure case insensitivity if needed.
- Unknown enum strings throw unless you implement custom parsing fallback.

---

#### Q13. How do you handle circular references in an object graph (`ReferenceHandler.Preserve` / `IgnoreCycles`)?

**Answer:** Object graphs with parent/child back-references cause infinite loops during naive serialization; `ReferenceHandler.IgnoreCycles` skips cyclic properties, and `ReferenceHandler.Preserve` emits `$id`/`$ref` metadata to reconstruct shared references on deserialize. Choose preserve when shared identity matters; ignore when trees are logically acyclic except for one back-link.

- `IgnoreCycles` drops repeated traversal — child.Parent may become null in JSON output.
- `Preserve` increases payload complexity but maintains graph shape for object webs.
- Redesign DTOs with id references instead of live object cycles for public APIs when possible.
- Entity Framework navigation properties often need cycle handling or `[JsonIgnore]` on back-references.

---

#### Q14. Why does serializing `Animal pet = new Dog()` sometimes drop `Dog`-only properties?

**Answer:** Statically typed serialization uses the compile-time type `Animal` unless polymorphic options include derived members, so properties declared only on `Dog` are omitted from JSON when the variable is typed as the base class. Runtime type alone does not expand the contract without configuration.

- `JsonSerializer.Serialize<Animal>(new Dog())` serializes only `Animal` members by default.
- Enable polymorphic type discriminators or serialize as `Dog` / use `object` with polymorphic options in modern System.Text.Json.
- See Gotcha 2 — this is a frequent API DTO bug with inheritance hierarchies.
- API models often flatten DTOs instead of relying on inheritance on the wire.

---

#### Q15. How do you enable polymorphic serialization in modern `System.Text.Json`?

**Answer:** .NET 7+ supports polymorphic serialization via attributes such as `[JsonDerivedType(typeof(Dog), "dog")]` on base types and global polymorphism options, emitting a type discriminator alongside base properties so deserializers instantiate the correct derived type. Earlier versions required custom converters or separate DTO shapes.

- Discriminator property name and derived type mappings must be stable across API versions.
- Untrusted polymorphic deserialization is a security risk — validate allowed derived types strictly.
- Newtonsoft.Json historically used `$type` metadata — migrate carefully when switching serializers.
- Integration tests should round-trip every derived type in the hierarchy.

---

#### Q16. What is `JsonNode`, `JsonObject`, and `JsonArray`, and when prefer them over strongly typed models?

**Answer:** `JsonNode` and its subclasses `JsonObject` and `JsonArray` model JSON as a mutable DOM you can navigate and edit without declaring fixed C# classes, useful for partially structured payloads, ad-hoc API exploration, and schema-evolving documents. Prefer strongly typed models when shape is stable and validation belongs at deserialization time.

- `JsonObject` supports `node["key"]` get/set; `JsonArray` indexes elements.
- Ideal for merging settings files, patching unknown third-party JSON, or building responses dynamically.
- Strong types give compile-time checks and clearer domain models for core business entities.
- Combine: deserialize known portions to types and keep extras in `JsonExtensionData` dictionary properties.

---

#### Q17. How do you navigate and mutate JSON with `JsonNode` without deserializing to a fixed class?

**Answer:** Parse with `JsonNode.Parse(json)` or `JsonDocument`/`JsonNode` async overloads, then cast or use `AsObject()` / `AsArray()` to read and assign properties, add keys, and remove nodes imperatively. Changes reflect in the in-memory tree until you call `ToJsonString()` to emit updated text.

- `JsonNode? nick = root?["nickname"];` — null-conditional for missing keys.
- Mutate: `((JsonObject)root!)["count"] = 42;`
- Clone subtrees when branching immutable snapshots for undo flows.
- Validate types before cast — wrong node kinds throw `InvalidOperationException`.

---

#### Q18. What is a custom `JsonConverter<T>`, and when would you implement `Read`/`Write` manually?

**Answer:** `JsonConverter<T>` plugs custom serialization logic into System.Text.Json for types the default reflection serializer handles poorly — for example `DateOnly`, discriminated unions, or legacy string formats for numbers. Override `Read` and `Write` to translate between JSON tokens and your CLR type explicitly.

- Register on the property with `[JsonConverter(typeof(MyConverter))]` or add to `JsonSerializerOptions.Converters`.
- Use when you need invariant wire formats independent of culture or special rounding rules for decimals.
- Manual converters must handle null, property names in object scenarios, and forward-compatible token skipping.
- Prefer built-in converters and attributes when they suffice — custom code is maintenance overhead.

---

#### Q19. How do `Utf8JsonReader` and `Utf8JsonWriter` differ from `JsonSerializer` helpers?

**Answer:** `Utf8JsonReader` and `Utf8JsonWriter` are low-level, forward-only UTF-8 JSON readers and writers that process tokens without building full object graphs, offering maximum control and performance for streaming pipelines. `JsonSerializer` builds on them internally for convenience deserialization to types.

- Reader loop: `while (reader.Read()) { switch (reader.TokenType) ... }`
- Writer: `WriteStartObject`, `WriteString`, `WriteEndObject` for incremental emission.
- Use low-level APIs for huge files, selective field extraction, or zero-allocation hot paths.
- Higher error handling burden — malformed JSON fails at token level with `JsonException`.

---

#### Q20. Explain `XmlSerializer` requirements (parameterless constructor, public read/write properties).

**Answer:** `XmlSerializer` can serialize public types with a public parameterless constructor and public read/write properties; it cannot serialize types lacking a default constructor or exposing only get-only properties unless you customize with attributes or `IXmlSerializable`. It generates XML from property names and collection shapes at runtime.

- Private fields and get-only auto-properties are ignored unless specially configured.
- Generic types and interfaces follow the same public property contract rules.
- Runtime failures occur when constructing the serializer if constraints are violated — compile succeeds.
- See Q22 for first-use code generation exceptions.

---

#### Q21. What do `[XmlRoot]`, `[XmlElement]`, `[XmlAttribute]`, and `[XmlIgnore]` control?

**Answer:** These attributes map CLR members to XML shape: `XmlRoot` names the document element, `XmlElement` sets element names for properties, `XmlAttribute` serializes a property as an attribute instead of child element, and `XmlIgnore` excludes members from XML output and input.

- `[XmlRoot("Person")]` on class changes outer tag from default type name.
- `[XmlAttribute("id")]` inlines simple values on the parent element.
- Collections serialize as repeated child elements unless `[XmlArray]` configures array layout.
- Order and namespace control use additional attributes (`Namespace`, `Order`) for schema compliance.

---

#### Q22. Why can `XmlSerializer` fail at runtime even when the project compiles?

**Answer:** `XmlSerializer` validates type shape and emits serialization assemblies on first use; violations such as missing parameterless constructors, interfaces as root types, or unsupported collection patterns throw `InvalidOperationException` at runtime when you first construct the serializer or serialize. The C# compiler does not analyze XML serializer constraints.

- First call may trigger csc.exe dynamic assembly generation — failures surface in production if untested.
- Circular references and `Dictionary<K,V>` historically required workarounds or custom serialization.
- Test `new XmlSerializer(typeof(MyType))` at startup or in integration tests to fail fast.
- `XmlSerializer` cannot serialize `IDictionary` implementations without custom patterns in many cases.

---

#### Q23. What is `[Serializable]` actually used for in modern .NET?

**Answer:** The `[Serializable]` attribute marked types as eligible for legacy binary formatters such as `BinaryFormatter`; modern `System.Text.Json` and `XmlSerializer` ignore it for their default code paths. It remains on some Framework-era types but is not the switch for JSON API serialization today.

- Do not add `[Serializable]` expecting JSON behavior — configure JSON attributes instead.
- See Gotcha 3 — conflating legacy binary markers with modern serializers is a common interview mistake.
- Remoting and old ASP.NET session state modes used binary serialization — largely historical.
- Prefer explicit DTO contracts for new persistence and API layers.

---

#### Q24. Explain `BinaryFormatter` — why is it obsolete, and what security risks led to its removal?

**Answer:** `BinaryFormatter` deserialized arbitrary type graphs from untrusted bytes and could be tricked into executing dangerous gadget chains, enabling remote code execution — a class of deserialization attacks. It is obsolete and disabled by default on modern .NET; Microsoft removed it as a safe default because type-name embedded payloads are inherently unsafe against malicious input.

- Never deserialize untrusted binary payloads with type-aware formatters.
- CVE history around `BinaryFormatter` drove stricter defaults and removal timelines.
- See Gotcha 4 — treating binary deserialize as harmless persistence is dangerous.
- Alternatives use explicit schemas: JSON, Protocol Buffers, MessagePack with known types.

---

#### Q25. What are recommended modern alternatives to `BinaryFormatter` for trusted internal persistence?

**Answer:** For trusted internal scenarios, use explicit formats: System.Text.Json or MessagePack for structured objects, `MemoryPack` or Protocol Buffers for performance-sensitive caches, or custom versioned binary layouts with `BinaryWriter` where you control every byte. All alternatives should whitelist types rather than embed arbitrary type names.

- JSON with source generation balances readability and AOT for app settings caches.
- MessagePack and protobuf require `.proto` or attributed models — no arbitrary type graphs.
- Encrypt and authenticate persisted blobs at rest even when trusted — defense in depth.
- Version headers on custom binary files enable migration without formatter magic.

---

#### Q26. How does `DateTime` with unspecified `Kind` behave across time zones during serialization?

**Answer:** `DateTime` with `DateTimeKind.Unspecified` carries no time-zone offset on the wire in ISO 8601 strings without `Z` or offset, so deserializing machines may interpret the instant differently depending on serializer options and local assumptions. Unspecified values are ambiguous for global systems.

- `DateTimeKind.Utc` serializes with `Z` suffix when using round-trip formats — preferred for server timestamps.
- `Local` kind ties meaning to the writer machine's time zone — poor for APIs.
- System.Text.Json defaults favor ISO 8601 strings; options control how `Unspecified` is written.
- Document whether API consumers should treat absent offset as UTC or local.

---

#### Q27. Why is `DateTimeOffset` often safer on the wire than `DateTime`?

**Answer:** `DateTimeOffset` always includes an offset from UTC alongside the clock time, preserving the intended instant across time zones without relying on unstated `Kind` semantics. It eliminates much ambiguity when clients span regions and daylight-saving transitions.

- Wire form includes `+05:30` or `Z` explicitly — consumers know the instant.
- Database storage still requires consistent UTC policy — `DateTimeOffset` maps cleanly to UTC for storage.
- Use for API contracts; convert to `DateTime` only at UI boundaries when local display requires it.
- Pair with invariant ISO formatting in serializers for stable string comparisons.

---

#### Q28. Can a type with only get-only properties serialize but fail to deserialize? Why?

**Answer:** Yes — System.Text.Json can serialize get-only properties by reading them, but deserialization requires writable members, constructor parameters matched by `[JsonConstructor]`, or `[JsonInclude]` on private setters unless using parameterized constructors configured for JSON. Immutable types need explicit constructor binding.

- Records with positional syntax generate constructor mapping; manual get-only classes do not deserialize by default.
- `[JsonInclude]` on private fields supports immutable object patterns intentionally designed for JSON.
- Serialize-only DTO projections are fine for reports; round-trip DTOs need write paths.
- Test deserialize in CI, not just serialize, for every API model.

---

#### Q29. How do `[JsonConstructor]` and parameterized constructors interact with deserialization?

**Answer:** Mark one constructor with `[JsonConstructor]` so the deserializer invokes it and binds JSON properties to parameters by name (case-insensitive by default), enabling immutable types without public setters. Parameters must align with JSON property names or `[JsonPropertyName]` mappings.

- Without `[JsonConstructor]`, multiple constructors cause ambiguity or fallback to parameterless ctor plus setters.
- Parameter names in source may require `[JsonConstructor]` and C# 9+ parameter name metadata for binding.
- Required parameters missing in JSON throw `JsonException` when options demand non-null values.
- Preferred pattern for domain models that reject invalid states at construction time.

---

#### Q30. What is the difference between `System.Text.Json` and `Newtonsoft.Json` feature sets (contract customization, references)?

**Answer:** System.Text.Json is built into modern .NET with faster defaults and ASP.NET Core integration but historically fewer knobs; Newtonsoft.Json (Json.NET) offers mature reference loop handling, `JObject` DOM, extensive contract resolvers, and broad customization at some performance cost. Many apps still reference Newtonsoft for legacy APIs or advanced scenarios.

| Area | System.Text.Json | Newtonsoft.Json |
|---|---|---|
| ASP.NET Core default | Yes | Optional package |
| Reference loops | `ReferenceHandler` options | `ReferenceLoopHandling` |
| DOM | `JsonNode` | `JObject` / `JToken` |
| Custom naming | Attributes + options | `ContractResolver` |

New greenfield ASP.NET Core APIs typically standardize on System.Text.Json unless a Newtonsoft-specific feature is required.

---

### 02. Reflection & Attributes

#### Q1. What is reflection in C#, and what problems does it solve?

**Answer:** Reflection is the ability to inspect and invoke types, members, and assemblies at runtime through metadata objects such as `Type`, `MethodInfo`, and `PropertyInfo`, instead of relying on compile-time names alone. Frameworks use it when they cannot know every user-defined class in advance — test discovery, serialization, dependency injection, and ORM mapping all depend on reading metadata embedded in assemblies.

- You enumerate properties, read attributes, and call methods when only a `Type` reference or string name is available at runtime.
- Custom attributes attach declarative data the compiler stores in IL; reflection reads that data to drive behavior without hard-coded switches per class.
- Reflection trades compile-time safety for flexibility — misspelled member names fail at runtime, not build time.
- Prefer reflection at framework boundaries; keep application business logic on strongly typed early-bound calls when possible.

---

#### Q2. What is the difference between early binding and late binding?

**Answer:** Early binding resolves types, methods, and properties at compile time, so the compiler emits direct calls and catches missing members before the program runs. Late binding resolves members at runtime through reflection or `dynamic`, deferring name and signature checks until execution.

- Early binding gives IntelliSense, refactoring support, and optimizer-friendly direct calls.
- Late binding suits plugin loaders, generic exporters, and serializers that must work with types not referenced at compile time.
- `MethodInfo.Invoke` is late binding — wrong argument types throw `TargetInvocationException` wrapping the inner error.
- Mix both: compile-time APIs for your code, reflection at integration seams where types vary.

---

#### Q3. Explain `typeof(T)` vs `obj.GetType()` — compile-time token vs runtime type.

**Answer:** `typeof(T)` is evaluated at compile time and always refers to the type token `T` in source, while `obj.GetType()` returns the actual runtime type of the object instance, which may be a derived class when the variable is typed as a base reference. They diverge whenever polymorphism is involved.

- `typeof(Animal)` is always `Animal` even if `obj` holds a `Dog`.
- `obj.GetType()` on a `Dog` instance returns `typeof(Dog)` even when stored in an `Animal` variable.
- Use `typeof` for generic type parameters and static metadata; use `GetType()` when behavior depends on the concrete instance.
- See Gotcha 1 — this distinction affects serialization, equality, and factory patterns.

---

#### Q4. Why does `typeof(List<int>) == typeof(List<string>)` return false?

**Answer:** Each closed generic instantiation such as `List<int>` and `List<string>` is a distinct constructed type at runtime with its own metadata, even though they share the same generic type definition `List<>`. Generic type parameters are part of the type identity.

- `List<int>` and `List<string>` have different method tables specialized for their type arguments.
- Compare generic definitions with `typeof(List<>).IsAssignableFrom(...)` or extract via `GetGenericTypeDefinition()`.
- Reflection APIs like `MakeGenericType` build closed types from open definitions plus argument types.
- Serialization and dependency injection containers key services by full closed generic types, not just the definition name.

---

#### Q5. How do you obtain the generic type definition from a closed generic type?

**Answer:** Call `closedType.GetGenericTypeDefinition()` on a constructed generic type such as `List<int>` to recover the open definition `List<>`, which you can then combine with new type arguments via `MakeGenericType`. If the type is not generic, `GetGenericTypeDefinition()` throws.

- Example: `typeof(Dictionary<string, int>).GetGenericTypeDefinition()` yields `typeof(Dictionary<,>)`.
- Use this when writing utilities that clone or rebind generic types with different type parameters.
- `IsGenericType` guards the call before extracting the definition.
- Framework code that registers `IRepository<T>` handlers often walks from closed service types back to open definitions.

---

#### Q6. What is the `Type` class, and what members expose metadata (methods, properties, fields, attributes)?

**Answer:** `Type` is the reflection handle for a CLR type, exposing metadata and discovery APIs for members, inheritance, generics, and custom attributes. You obtain it via `typeof`, `GetType()`, or `Type.GetType(string)`.

- `GetProperties()`, `GetMethods()`, `GetFields()`, and `GetConstructors()` enumerate members; overload with `BindingFlags` for non-public or declared-only views.
- `IsClass`, `IsValueType`, `IsAssignableFrom`, and `BaseType` describe inheritance and classification.
- `GetCustomAttribute<T>()` and `IsDefined()` read attribute metadata applied to the type or its members.
- `Assembly` on a `Type` links back to the containing assembly for broader scanning scenarios.

---

#### Q7. What is `BindingFlags`, and how do `Instance`, `Static`, `Public`, `NonPublic`, and `DeclaredOnly` combine?

**Answer:** `BindingFlags` is a bitmask that tells reflection which members to include when querying a type, combining visibility, static vs instance, and whether to walk inheritance or stop at the declaring type. Default public instance queries hide private helpers and static members unless you OR the appropriate flags.

| Flag | Effect |
|---|---|
| `Instance` | Include instance members |
| `Static` | Include static members |
| `Public` | Include public members |
| `NonPublic` | Include private and internal members |
| `DeclaredOnly` | Only members declared on this type, not inherited |

- Typical private field access: `BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly`.
- Omitting `DeclaredOnly` walks the inheritance chain, which can return multiple `ToString`-like overrides from different levels.
- Framework serializers and mappers often combine flags to honor non-public backing fields intentionally.

---

#### Q8. How do you enumerate properties, methods, fields, and constructors with reflection?

**Answer:** Obtain a `Type`, then call the appropriate `Get*` methods — `GetProperties`, `GetMethods`, `GetFields`, `GetConstructors` — optionally passing `BindingFlags` and name filters such as `GetMethod("Name", types)`. Results are arrays of `PropertyInfo`, `MethodInfo`, `FieldInfo`, or `ConstructorInfo` objects you iterate to build pipelines.

- Filter with LINQ: `type.GetProperties().Where(p => p.CanWrite && p.GetCustomAttribute<ExportableAttribute>() != null)`.
- Overloaded methods require `GetMethod(name, bindingFlags, binder, types, modifiers)` with exact parameter types.
- `GetMembers()` returns all member kinds in one array when you need unified processing.
- Cache `MemberInfo` results for hot paths — repeated reflection queries add allocation and CPU cost.

---

#### Q9. How do you invoke a method dynamically via `MethodInfo.Invoke`?

**Answer:** Obtain a `MethodInfo` for the target method, then call `method.Invoke(instance, parameters)` where `instance` is `null` for static methods and an object reference for instance methods. Arguments must match parameter types and order; mismatches throw before or during invocation.

- Wrap calls in try/catch for `TargetInvocationException` and inspect `InnerException` for the real fault from the invoked method.
- `Invoke` boxes value-type returns as `object`; cast the result to the expected type.
- Prefer strongly typed delegates created via `CreateDelegate` when the same method is invoked repeatedly for better performance.
- Optional parameters and `params` arrays must be supplied explicitly in the object[] — defaults are not applied automatically.

---

#### Q10. What is `Activator.CreateInstance`, and how do you pass constructor arguments?

**Answer:** `Activator.CreateInstance` constructs objects when you hold a `Type` reference rather than a compile-time generic parameter, optionally passing constructor arguments that match an available constructor signature. Overloads accept `Type`, `(Type, params object[] args)`, and generic `CreateInstance<T>()`.

- Missing or ambiguous constructors throw `MissingMethodException` at runtime.
- For non-public constructors, use `ConstructorInfo.Invoke` with appropriate `BindingFlags` instead of the simple activator overload.
- DI containers often wrap activator logic with constructor selection and dependency resolution.
- Prefer `Activator.CreateInstance<T>()` when the type is known at compile time — it is clearer and faster.

---

#### Q11. How do you create generic types at runtime (`MakeGenericType`) and invoke generic methods (`MakeGenericMethod`)?

**Answer:** Start from an open generic type definition such as `typeof(Dictionary<,>)`, call `MakeGenericType(typeof(string), typeof(int))` to produce `Dictionary<string,int>`, then create instances or reflect on that closed type. For generic methods, obtain `MethodInfo` from the open definition, then `MakeGenericMethod(typeof(TArg))` before `Invoke`.

- Open definitions come from `GetGenericTypeDefinition()` on an existing closed type or directly from `typeof(List<>)` syntax in C#.
- `MakeGenericMethod` requires the method's generic parameters be supplied before invocation on a closed method.
- Incorrect arity throws `ArgumentException` — count type arguments carefully for types like `Dictionary<,>`.
- ORMs and middleware builders use this pattern to construct closed repository or handler types from entity types discovered at startup.

---

#### Q12. How do you get and set property and field values through `PropertyInfo` / `FieldInfo`?

**Answer:** `PropertyInfo.GetValue(obj)` and `SetValue(obj, value)` read and write properties, respecting indexers when you pass index arguments in overloads. `FieldInfo` uses the same `GetValue`/`SetValue` pair for fields, including non-public ones when discovered with `NonPublic` binding flags.

- `CanRead` and `CanWrite` guard against get-only or init-only properties before setting.
- Value types require boxing through `object` — mutations on boxed copies do not write back unless you `SetValue` again.
- Indexed properties need the index values as additional arguments to `GetValue`/`SetValue`.
- Export and mapping utilities iterate properties and copy values without hand-written per-type assignments.

---

#### Q13. How can reflection access private members, and why is that a maintenance and security concern?

**Answer:** Reflection can retrieve private fields and methods by combining `BindingFlags.NonPublic` with `Instance` or `Static`, then invoke or mutate them despite language visibility rules. That bypasses encapsulation, hides dependencies from the type author, and breaks when internals are renamed or refactored without compile-time errors.

- Test code that reaches private members couples tests to implementation details instead of public behavior.
- Security-sensitive invariants enforced by `private` can be circumvented by fully trusted reflection callers.
- Frameworks like serializers may intentionally access non-public members with documented attributes — application code should not treat this as a general pattern.
- Prefer `InternalsVisibleTo` for test assemblies over arbitrary private access when internal seams must be tested.

---

#### Q14. Explain `Assembly`, `Module`, `MemberInfo`, `MethodInfo`, `PropertyInfo`, and `FieldInfo` relationships.

**Answer:** An `Assembly` contains one or more `Module` objects, each holding IL and metadata for types; every type member exposes a `MemberInfo` base, with specialized subclasses `MethodInfo`, `PropertyInfo`, `FieldInfo`, `ConstructorInfo`, and `EventInfo` adding type-specific APIs. Reflection walks this hierarchy from assembly scan down to individual invocations.

- `assembly.GetTypes()` lists types defined in the assembly — may throw `ReflectionTypeLoadException` if dependencies are missing.
- `MemberInfo.DeclaringType` shows where the member was defined; `ReflectedType` shows through which type the lookup occurred.
- Attributes can target assemblies, modules, types, or members — each level has corresponding reflection entry points.
- Plugin systems load assemblies, enumerate types implementing an interface, and instantiate matches via `Activator`.

---

#### Q15. What is the difference between `Assembly.Load`, `Assembly.LoadFrom`, and `AssemblyLoadContext`?

**Answer:** `Assembly.Load` binds assemblies by name through the default load context using policy and probing paths, while `Assembly.LoadFrom` loads a file path into a context that can duplicate assemblies already loaded by identity. `AssemblyLoadContext` in modern .NET provides isolated, optionally unloadable contexts for plugins with explicit dependency graphs.

- `LoadFrom` can load two copies of the same assembly identity from different paths, breaking type identity (`obj is MyType` fails across copies).
- `AssemblyLoadContext.Default` replaces many Framework `AppDomain` loading scenarios with clearer isolation.
- Plugin hosts create custom contexts, load from a plugin folder, and unload the context when the plugin is retired.
- Prefer `Load` with correct probing or explicit `AssemblyLoadContext` over ad hoc `LoadFrom` in new code.

---

#### Q16. Can you unload an assembly in .NET Framework vs .NET Core / .NET 5+?

**Answer:** In .NET Framework, assemblies loaded into an AppDomain could be unloaded only by unloading the entire AppDomain, which was heavy and rarely used in application code. Modern .NET unloads assemblies only when their `AssemblyLoadContext` is collectible and no live references remain to types or instances from that context.

- Default context assemblies stay loaded for process lifetime — same practical constraint as most Framework apps.
- Collectible contexts enable plugin scenarios: drop references, trigger GC, and reclaim generated types from the plugin.
- Reflection caches and static state in plugins can prevent unload until those references are cleared.
- Do not assume unload for hot reload of arbitrary app assemblies in production without architectural support.

---

#### Q17. How do you discover and read custom attributes at runtime (`GetCustomAttribute`, `IsDefined`)?

**Answer:** Call `member.GetCustomAttribute<T>()` to retrieve a single attribute instance, or `GetCustomAttributes()` for multiples when `AllowMultiple` is true; `IsDefined(typeof(T), inherit)` checks presence without constructing the attribute. Inheritance follows `AttributeUsage.Inherited` on the attribute class definition.

- Constructor arguments appear as read-only properties on the attribute instance returned by reflection.
- `inherit: true` walks base classes for class-level attributes marked inherited.
- Scanning all properties on a type for `[Exportable]` is a common metadata-driven export pattern in tutorials.
- Attributes are cheap to read once at startup — cache results rather than re-querying per operation.

---

#### Q18. How do you define and apply your own attribute classes (`AttributeUsage`)?

**Answer:** Derive a sealed class from `Attribute`, mark it with `[AttributeUsage(...)]` specifying valid targets, optional `AllowMultiple`, and `Inherited`, then apply it with `[MyAttribute(...)]` on permitted declarations. The compiler embeds attribute blobs into metadata for runtime readers.

- Positional constructor parameters map to required metadata; public properties supply named optional values in attribute syntax.
- Only public properties with public getters can be set as named attribute arguments.
- Seal attribute classes — unsealed attributes are a rare extensibility footgun.
- `[AttributeUsage(AttributeTargets.Property, Inherited = true)]` is typical for column or serialization hints on properties.

---

#### Q19. What are performance costs of reflection vs compiled code, and how do trimming/AOT affect it?

**Answer:** Reflection incurs metadata lookups, allocations for `MemberInfo`, and indirect invocation that is orders of magnitude slower than direct calls in hot loops, though one-time startup scans are usually acceptable. Native AOT and trimming remove or warn on reflection targets that cannot be proven at compile time, breaking apps that rely on undiscoverable dynamic type graphs.

- Cache `PropertyInfo` and `MethodInfo` when the same members are touched repeatedly.
- `CreateDelegate` and source generators replace hot-path reflection with generated code.
- Trimming requires `DynamicallyAccessedMembers` annotations or `DynamicDependency` when frameworks reflect into your types.
- Profile before micro-optimizing — many apps reflect only at startup for DI registration and routing.

---

#### Q20. What is `Reflection.Emit`, and when is dynamic IL generation justified?

**Answer:** `Reflection.Emit` builds dynamic assemblies and types by emitting IL at runtime, enabling scenarios where neither compile-time types nor simple reflection invocation suffice — for example lightweight proxy generation or specialized serializers. It is advanced, hard to debug, and justified mainly for library authors implementing proxies, mocks, or expression compilers.

- Mock frameworks and some ORM providers generate IL to override behavior efficiently.
- Emitted code runs at full speed once JIT-compiled, unlike repeated `Invoke` reflection.
- Prefer expression trees, source generators, or existing libraries before hand-rolling emit.
- Emitted assemblies participate in the same load-context and trimming constraints as reflected types.

---

#### Q21. How does reflection interact with nullable reference type annotations?

**Answer:** Nullable reference type (NRT) annotations exist only in compile-time metadata and are not enforced by reflection when reading or writing properties — `GetValue` returns `object?` and `SetValue` accepts null regardless of `string` vs `string?` declarations. Consumers of reflected APIs must apply their own null checks.

- `NullabilityInfoContext` (.NET 6+) reads NRT metadata from reflection for tools and serializers that respect nullability.
- Trimming and reflection-based serializers may treat annotated non-nullable properties as optionally absent in JSON.
- Do not assume reflection honors NRT — runtime null still occurs if payloads or setters allow it.
- Library authors exposing reflection helpers should document null behavior explicitly.

---

#### Q22. What security permissions historically gated reflection, and what changed in modern .NET?

**Answer:** .NET Framework Code Access Security could restrict reflection and private member access via permissions such as `ReflectionPermission`, tying capability to evidence and trust zones. Modern .NET consolidates on a simpler trust model: fully trusted application code reflects freely, while sandboxing relies on process isolation rather than granular reflection permissions.

- Partial trust ASP.NET and plug-in hosts were common pain points for reflection-heavy libraries on Framework.
- .NET Core and later removed most CAS infrastructure — assume full trust inside your process boundary.
- Untrusted code should not run in-process — deserialize and plugin boundaries are security concerns separate from CAS.
- Server hardening focuses on input validation and least-privilege deployment, not reflection permission attributes.

---

### 03. Regular Expressions

#### Q1. What is the purpose of the `Regex` class in C#?

**Answer:** The `Regex` class in `System.Text.RegularExpressions` implements a .NET regular expression engine that matches, searches, splits, and replaces text according to declarative patterns instead of hand-written character scanning. It centralizes pattern compilation and options so the same rule can run across validation, log parsing, and import pipelines.

- Patterns describe character classes, repetitions, anchors, and groups — the engine interprets them against input strings.
- Static helpers like `Regex.IsMatch` suit one-off checks; instance `Regex` objects reuse compiled patterns on many inputs.
- Regex complements but does not replace full parsers for nested or stateful formats like JSON or XML.
- Production code should always consider timeouts and culture options when patterns touch user-supplied text.

---

#### Q2. Explain `Regex.IsMatch`, `Match`, `Matches`, `Replace`, and `Split`.

**Answer:** `IsMatch` returns a boolean for whether the pattern matches anywhere or at an anchored position; `Match` returns the first `Match` object with success, index, length, and groups; `Matches` returns all non-overlapping matches as a `MatchCollection`. `Replace` substitutes matched substrings (with optional back-references), and `Split` tokenizes input on pattern delimiters.

- `Match.Success` gates reading `Value` — failed matches still return a `Match` object with `Success == false`.
- `Match.NextMatch()` walks subsequent matches without re-scanning from the start manually.
- `Replace` supports `$1` group back-references and `MatchEvaluator` delegates for computed replacements.
- `Split` with capturing groups includes separators in the output array — a common surprise when splitting on delimiters.

---

#### Q3. What is the difference between verbatim regex strings (`@"\d+"`) and escaped regular strings?

**Answer:** Verbatim strings prefixed with `@` treat backslashes literally, so the regex `\d+` is written as `@"\d+"` instead of `"\\d+"` in a normal string. Both produce the same pattern at runtime; verbatim form improves readability for regex-heavy patterns.

- In regular strings, every backslash must be escaped for the C# compiler before the regex engine sees it.
- Verbatim strings cannot end with a lone backslash — use `"\\"` concatenation at the end if needed.
- `Regex.Escape(userInput)` inserts backslashes before metacharacters when embedding literal user text in a pattern.
- Choose `@` patterns in code reviews for anything beyond trivial `\d` or `\w` fragments.

---

#### Q4. What are common metacharacters candidates should know (`.`, `*`, `+`, `?`, `^`, `$`, `\d`, `\w`, groups)?

**Answer:** Metacharacters give regex its expressive power: `.` matches any character (except newline by default), `*` and `+` repeat the prior atom greedily, `?` makes repetition lazy or optional, `^` and `$` anchor to start and end, `\d` and `\w` match digit and word characters, and `(...)` capture groups for extraction or back-references.

| Symbol | Typical meaning |
|---|---|
| `.` | Any char (with default options) |
| `*` / `+` / `?` | Zero-or-more / one-or-more / optional or lazy |
| `^` / `$` | Start / end anchors |
| `\d`, `\w`, `\s` | Digit, word, whitespace classes |
| `(...)` | Capturing group |

- Character classes `[A-Z0-9]` restrict matches without capturing unless grouped.
- Non-capturing `(?:...)` groups for precedence without polluting `Groups` collection.
- Anchors `^` and `$` are essential for validation patterns that must match the entire input.

---

#### Q5. What is catastrophic backtracking, and what pattern shapes trigger it?

**Answer:** Catastrophic backtracking occurs when a regex engine explores exponentially many partial matches on certain inputs because nested quantifiers retry overlapping paths, causing CPU to spike or hang. Classic shapes combine nested `*` or `+` on overlapping subexpressions, such as `(a+)+$` against a long string of `a` with a trailing mismatch.

- The engine backtracks to try alternate split points between quantifiers — failure near the end maximizes retries.
- User-controlled input plus vulnerable patterns creates denial-of-service (ReDoS) risk in web endpoints.
- Simplify patterns, use possessive or atomic groups where supported, or prefer the non-backtracking engine in .NET 7+.
- See Gotcha 11 — always set match timeouts on patterns applied to external input.

---

#### Q6. How do you mitigate ReDoS using `Regex.MatchTimeout` or the `matchTimeout` parameter in .NET?

**Answer:** Pass a `TimeSpan` match timeout to the `Regex` constructor or static methods so the engine aborts matching after a bounded interval instead of running unbounded backtracking. Centralize hot patterns in static `readonly Regex` fields constructed once with a shared timeout policy.

- Example: `new Regex(pattern, RegexOptions.None, TimeSpan.FromMilliseconds(250))`.
- Static `Regex.IsMatch(input, pattern, RegexOptions.None, matchTimeout)` applies the same guard to one-off calls.
- Choose timeouts based on expected input length — log timeouts as potential attack or pattern bugs.
- Combine timeout with input length limits for defense in depth on public APIs.

---

#### Q7. What happens when a regex times out — which exception is thrown?

**Answer:** When matching exceeds the configured timeout, .NET throws `RegexMatchTimeoutException`, a subclass of `TimeoutException`, aborting the current match attempt. Callers should catch it at validation boundaries and return a safe error rather than retrying the same pattern on the same input indefinitely.

- The exception carries pattern and timeout metadata useful for diagnostics without logging raw user secrets.
- Partial results are not returned — the match fails as a whole.
- Distinguish timeout from `Success == false` — timeout indicates engine budget exceeded, not a non-match.
- Monitor timeout rates in production to detect ReDoS attempts or overly complex patterns.

---

#### Q8. What is atomic grouping's role in preventing backtracking explosions?

**Answer:** Atomic groups (or possessive quantifiers in some engines) commit to a repetition match without allowing later backtracking into that subexpression, pruning branches that would otherwise explode. .NET exposes related ideas through `(?>...)` atomic grouping and non-backtracking mode rather than full possessive `*+` syntax everywhere.

- Once an atomic group matches, the engine never splits its internal repetition differently for later failures.
- Rewriting `(a+)+` as `(?>a+)+` removes nested backtracking paths on long inputs.
- Atomic groups can change whether a valid match exists — test carefully after rewriting.
- Prefer structural simplification first; atomic grouping is an optimization for patterns that must stay regex-based.

---

#### Q9. How does culture affect case-insensitive matching, and what does `RegexOptions.CultureInvariant` do?

**Answer:** Case-insensitive matching without invariant options uses the current thread culture's casing rules, so Turkish `I`/`ı` edge cases and other locale rules can differ from ASCII expectations. `RegexOptions.CultureInvariant` ignores culture-specific casing tables and uses invariant rules, which is usually correct for identifiers, product codes, and log tokens.

- Combine `IgnoreCase | CultureInvariant` for ASCII-centric validation like email local parts or SKU patterns.
- Culture-sensitive matching matters for natural language search UI, not for protocol identifiers.
- Static validation regexes in libraries should document whether they are culture-invariant.
- `RegexOptions.ECMAScript` is a separate compatibility switch — do not confuse it with culture settings.

---

#### Q10. When should you compile regexes with `RegexOptions.Compiled` (or source generators in .NET 7+)?

**Answer:** `Compiled` trades higher startup cost and memory for faster repeated matching by emitting IL specialized to the pattern, worthwhile when the same regex runs many times per process on hot paths. .NET 7+ source-generated regex (`[GeneratedRegex]`) improves further by moving work to compile time with trimming-friendly metadata.

- Avoid `Compiled` on one-off or rarely used patterns — JIT cost may never amortize.
- Cache `static readonly Regex` instances instead of parsing patterns on every call.
- Source generators catch some pattern errors at compile time and reduce runtime setup.
- Profile before assuming `Compiled` wins — modern regex engines are already fast for modest patterns.

---

#### Q11. What is `RegexOptions.NonBacktracking` (.NET 7+), and what trade-offs does it have?

**Answer:** `NonBacktracking` selects an alternate regex engine implementation designed to avoid catastrophic backtracking by using a different matching strategy with predictable linear-time behavior on many patterns. Some advanced backtracking features and edge-case semantics differ from the classic engine, so tests must validate both match results and performance.

- Ideal for user-supplied or evolving patterns where ReDoS risk is unacceptable.
- Not every pattern behaves identically — verify captures and zero-length matches against legacy results.
- Combine with timeouts even in non-backtracking mode for defense in depth.
- Choose explicitly per `Regex` instance — it is not the global default.

---

#### Q12. How do named capture groups work, and how do you read them from `Match.Groups`?

**Answer:** Named groups use `(?<name>...)` or `(?'name'...)` syntax; after a successful match, `match.Groups["name"]` or `match.Groups["name"].Value` retrieves the captured substring. Numbered groups remain available — group 0 is the full match, group 1 is the first capture.

- Named groups improve readability in `Replace` with `${name}` back-references.
- Duplicate group names in alternation branches require .NET's branch-specific naming rules — test multi-branch patterns.
- Check `Groups["name"].Success` when the group participated optionally in the match.
- Tutorial import scenarios extract `order id` from free text with `(?<id>\d+)` inside notes fields.

---

#### Q13. What is the difference between greedy and lazy quantifiers (`+` vs `+?`)?

**Answer:** Greedy quantifiers (`*`, `+`, `{n,m}`) consume as much input as possible while still allowing the overall pattern to succeed, while lazy quantifiers (`*?`, `+?`, `{n,m}?`) consume as little as possible before expanding. The difference changes which substring is captured when multiple valid splits exist.

- Pattern `".+"` on `"abc"` greedy-matches the whole string; lazy `".+?"` stops at the first opportunity if followed by anchors or delimiters.
- HTML scraping with greedy `.*` often overshoots closing tags — lazy quantifiers or explicit delimiters help.
- Greedy default is correct for many validations; lazy helps token extraction between known markers.
- Possessive and atomic variants further restrict backtracking beyond lazy behavior.

---

#### Q14. When should you prefer `Regex` over simple `string.Contains` / `Split` for maintainability?

**Answer:** Prefer `Regex` when the rule involves optional parts, character classes, repeated structures, or multiple delimiters that would require fragile chained string operations. Prefer `Contains`, `StartsWith`, or single-char `Split` when the check is a fixed literal with no structural variation.

- Regex centralizes a declarative rule that non-programmers can read from a named pattern constant.
- Complex `IndexOf` loops accumulate edge cases — regex with tests documents intent in one place.
- Simple checks without regex avoid culture, timeout, and ReDoS concerns entirely.
- Refactor to regex when you see three or more string hacks for the same field validation.

---

#### Q15. How do you validate input with regex without using it as a full parser (e.g., email, phone)?

**Answer:** Use regex to enforce shape and reject obvious garbage — reasonable character sets, length bounds, and required segments — then delegate deliverability or existence checks to dedicated services. Treat regex validation as a first gate, not proof that an email inbox exists or a phone number is assigned.

- Anchor patterns with `^` and `$` (or use explicit full-match APIs) so partial matches do not pass.
- Email regex in tutorials checks common structure; DNS and SMTP verification happen outside regex.
- Phone patterns vary by country — document locale assumptions or use libphonenumber-style libraries for production.
- Combine regex with length limits and normalization (trim, Unicode form) before matching.

---

### 04. Var, Dynamic & Special Keywords

#### Q1. Explain `var` — what is known at compile time vs runtime?

**Answer:** `var` instructs the compiler to infer the static type of a local variable from the initializer expression at compile time, so the variable still has a fixed strong type after compilation — there is no runtime type change. If inference fails or the initializer is missing, the compiler reports an error.

- `var s = "hi";` is compile-time `string`, not a runtime-decided type.
- `var` cannot declare fields without an initializer and cannot change type on reassignment.
- Reflection and `GetType()` at runtime see the inferred type, identical to an explicit declaration.
- Inference improves readability for long generic types while preserving full static checking.

---

#### Q2. When is `var` required (anonymous types) vs merely convenient?

**Answer:** `var` is mandatory when declaring anonymous types because the compiler generates a type name that is not expressible in source, such as `var row = new { Id = 1, Name = "A" };`. For all named types, `var` is optional convenience — explicit types remain valid and sometimes clearer for public APIs and unclear initializers.

- Anonymous projection in LINQ `select new { ... }` requires `var` or implicit typing in query syntax.
- Prefer explicit types when the initializer does not make the type obvious (`var x = GetValue();`).
- Team style guides often allow `var` when the right-hand side spells out the type clearly.
- `var` does not disable nullable warnings — inferred reference types carry NRT annotations.

---

#### Q3. Explain the `dynamic` keyword and the DLR's role.

**Answer:** `dynamic` tells the compiler to defer member resolution to runtime via the Dynamic Language Runtime (DLR), which performs binding at execution time using call sites that cache resolved members after the first successful bind. Static typing is bypassed for `dynamic` receivers, so compile-time member checking is disabled.

- The DLR coordinates overload resolution, implicit conversions, and `dynamic` invocation across languages and COM interop.
- First access pays binding cost; subsequent calls use cached rules until arguments types change materially.
- Errors like misspelled property names surface as `RuntimeBinderException` at runtime.
- Use sparingly for JSON DOMs, COM, and truly dynamic payloads — not for ordinary application logic.

---

#### Q4. What is the difference between `var`, `dynamic`, and `object`?

**Answer:** `var` is compile-time type inference with full static checking; `object` is the base type that requires explicit casts before calling specific members; `dynamic` skips compile-time member checks and resolves calls at runtime through the DLR. All three can hold references to heterogeneous data, but only `dynamic` defers binding.

| Keyword | Compile-time type | Member calls |
|---|---|---|
| `var` | Inferred concrete type | Statically checked |
| `object` | `object` | Requires cast |
| `dynamic` | `dynamic` | Runtime bound |

- Assigning a `string` to `object` needs `(string)obj` or pattern matching before `Length`.
- `dynamic` appears to call members directly but fails at runtime if the target lacks them.
- Prefer `object` with pattern matching in modern C# when shape varies but you want exhaustiveness.

---

#### Q5. When does `dynamic` defer member binding to runtime, and what errors appear only then?

**Answer:** Any member access, method call, indexer, or operator on a `dynamic` expression is resolved when that line executes, not during compilation. Missing members, wrong argument types, and ambiguous overloads throw `RuntimeBinderException` or related runtime errors that would have been compile errors on static types.

- `dynamic d = GetPayload(); d.Totla();` compiles but fails at runtime on the typo.
- Return types of dynamic calls are `dynamic` unless converted, propagating deferred binding downstream.
- Debugging is harder — IDE refactor and Find References skip dynamic sites.
- See Gotcha 8 — treat `dynamic` boundaries as explicit integration seams with tests.

---

#### Q6. What is `DynamicObject`, and when would you subclass it?

**Answer:** `DynamicObject` is a base class for types that participate in the DLR by overriding `TryGetMember`, `TrySetMember`, `TryInvokeMember`, and related hooks to define custom dynamic behavior. Subclass it when building dynamic proxies, DSL objects, or dictionary-backed models that expose members not declared as C# properties.

- `ExpandoObject` is a built-in `DynamicObject` mapping names to object values.
- Override hooks return `true` when the operation succeeds and set `result` for get/invoke paths.
- Enables Ruby-like dynamic APIs while remaining callable from C# with `dynamic` references.
- Prefer static interfaces when shape is stable — custom `DynamicObject` is for truly open-ended models.

---

#### Q7. What is `ExpandoObject`, and how does it differ from `Dictionary<string, object>`?

**Answer:** `ExpandoObject` implements `IDynamicMetaObjectProvider` and `IDictionary<string, object>`, so callers can use dynamic member syntax or dictionary APIs on the same bag of name-value pairs. A plain dictionary only supports indexer and dictionary methods unless wrapped — not dynamic member binding without extra glue.

- `dynamic bag = new ExpandoObject(); bag.Score = 10;` adds a property at runtime.
- Cast to `IDictionary<string, object>` to enumerate keys or serialize to JSON with appropriate options.
- Expando graphs suit lightweight JSON merge scenarios; typed models are safer for core domain entities.
- Thread safety is not automatic — treat expando instances like ordinary mutable dictionaries.

---

#### Q8. Explain `nameof` — how does it help refactoring and logging?

**Answer:** `nameof(expression)` resolves at compile time to the unqualified name string of a variable, type, or member, so renaming the symbol updates every `nameof` reference through the IDE refactor tools. It avoids magic strings in exceptions, logging, and property-change notifications.

- `nameof(customer.Email)` yields `"Email"` even if the expression type is broader than the member's declaring type.
- Safer than `"Email"` literals that drift during rename — compiler ties `nameof` to the symbol.
- Works on parameters, methods, and types — `nameof(OrderService.PlaceOrder)` for diagnostic messages.
- Does not evaluate runtime expressions — only simple name forms are allowed.

---

#### Q9. What is the `global::` qualifier, and when is it needed to disambiguate namespaces?

**Answer:** The `global::` prefix starts name lookup from the root namespace scope, bypassing user-defined aliases or nested namespaces that shadow system types. Use it when a project namespace such as `System` or `Email` collides with BCL types like `System.String`.

- Example: `global::System.IO.File` when your namespace hierarchy defines a conflicting `File` class.
- Aliases (`using IO = ...`) are usually enough, but `global::` is the unambiguous escape hatch.
- Generated code and analyzers occasionally emit `global::` for deterministic resolution.
- Rare in hand-written code — fix namespace naming first when collisions recur.

---

#### Q10. What is `default` literal (C# 7.1+) vs `default(T)`?

**Answer:** The `default` literal lets the compiler infer the target type from context, so `default` in `int x = default;` means `0` and in `string? s = default;` means `null` without repeating the type name. `default(T)` remains valid in generic code where the type parameter `T` is not known as a specific type at the call site.

- `default` improves readability in ternary and coalescing expressions with nullable reference types.
- For unconstrained `T`, `default` and `default(T)` both yield null for reference types and zeroed value types.
- Cannot use `default` where type inference is ambiguous — add an explicit type in those cases.
- See C# 7 Q12 for generic constraint interactions with `default`.

---

#### Q11. What is `@` verbatim identifier syntax (`@class`, `@event`) used for?

**Answer:** Prefix `@` allows identifiers that coincide with C# keywords — `@class`, `@event`, `@int` — so generated or interop code can use reserved words as names. Verbatim identifiers are the same CLR names without the `@` at runtime.

- Common for XML or database columns named `class` mapped into C# properties.
- `@` on strings (`@"C:\path"`) is a separate feature — verbatim string literals, not identifiers.
- Prefer renaming to non-keyword names in hand-written domain models when possible.
- JSON property names can still map via attributes without `@` in source identifiers.

---

#### Q12. What is unsafe code, and when are pointers justified in C#?

**Answer:** Unsafe code blocks, enabled with `/unsafe` and the `unsafe` keyword, allow pointer arithmetic and fixed buffers for interop with native APIs or extreme hot paths where spans still cannot express the required semantics. Most application code never needs unsafe — `Span<T>`, `Memory<T>`, and `stackalloc` cover many performance scenarios safely.

- Required for some legacy C library interop expecting `byte*` parameters.
- Misuse introduces buffer overruns and GC pinning hazards — code review and tests are mandatory.
- Modern BCL moves toward safe abstractions; unsafe is opt-in and excluded from some sandboxed contexts.
- Keep unsafe isolated in small vetted modules with clear documentation.

---

#### Q13. What is `stackalloc`, and how does it relate to performance-sensitive code?

**Answer:** `stackalloc` allocates a block of memory on the stack for value types, avoiding heap allocations for small temporary buffers when used with `Span<T>` in safe contexts (C# 7.2+). It suits short-lived arrays in parsing, crypto, or formatting hot paths where heap pressure matters.

- `Span<int> buf = stackalloc int[32];` keeps allocation scoped to the method frame.
- Large `stackalloc` can overflow the stack — cap sizes and spill to `ArrayPool` for big buffers.
- C# 8 integrated `stackalloc` into safe patterns alongside `Span` without requiring unsafe blocks in many cases.
- Profile before micro-optimizing — allocator improvements in the runtime already reduce small array costs.

---

#### Q14. What is `ref readonly` return, and how does it differ from returning by value?

**Answer:** A `ref readonly` return exposes a read-only reference to an existing storage location (often a large struct field or array element) without copying bytes, while preventing the caller from mutating through that reference. Returning by value copies the entire struct, which can be expensive for large value types.

- Caller receives `ref readonly T` and reads fields without taking a writable alias.
- Useful for exposing items from internal buffers while preserving encapsulation.
- Distinct from `in` parameters — `ref readonly` is about return paths.
- Large readonly struct returns in hot loops are a primary use case for this feature.

---

#### Q15. How does `dynamic` interact with extension methods (why don't they bind dynamically)?

**Answer:** Extension methods are resolved statically at compile time based on the static type of the receiver expression, so a `dynamic` receiver does not see extension methods unless you cast to the static type or invoke the extension as a static call. The DLR does not participate in extension method lookup.

- `dynamic d = ...; d.Extension();` fails at runtime even if an extension exists for the runtime type.
- Call `MyExtensions.Extension((ConcreteType)d)` or cast before invoking extensions.
- See Gotcha 9 — this surprises developers mixing LINQ-style extensions with dynamic JSON models.
- Prefer static types or wrapper methods at the dynamic boundary instead of relying on extensions.

---

### 05. C# 7 Features

#### Q1. What are tuple deconstruction and named tuple elements?

**Answer:** C# 7 tuples return multiple values from methods as `(T1, T2)` pairs or larger arities, and deconstruction assigns them to separate variables with `var (a, b) = GetPair();`. Named elements like `(int Id, string Name)` give readable field names instead of default `Item1`, `Item2`.

- Tuple types are value-type `System.ValueTuple` structs — lightweight for small composite returns.
- Names are compile-time metadata for IntelliSense; runtime field names remain `Item1`, etc., unless reflected with tuple metadata.
- Prefer small DTO records for public APIs; tuples suit private helper returns and LINQ projections.
- Deconstruction works with `out var` and discards in the same statement.

---

#### Q2. How do `out` variables declared inline in method calls work?

**Answer:** C# 7 allows declaring `out` variables directly in the argument list, such as `if (int.TryParse(text, out var n))`, scoping the variable to the enclosing block and removing a separate declaration line. Type inference applies when `var` is used with `out`.

- `out var x` must be assigned definitely in the callee before the call returns — same definite assignment rules as classic `out`.
- Inline `out` improves parse-try patterns without pre-declaring throwaway variables.
- Cannot use `out` inline variables before C# 7 — legacy code declares above the call.
- Combine with pattern matching in later C# versions for richer parsing flows.

---

#### Q3. What are discards (`_`), and where are they used (deconstruction, unused returns)?

**Answer:** A discard is a write-only placeholder named `_` (or `_` in deconstruction patterns) that explicitly ignores a value the API returns but your logic does not need. The compiler suppresses unused-variable warnings and clarifies intent.

- Deconstruct: `var (id, _, _) = GetTriple();` keeps only the first component.
- Multiple `_` discards in one scope are distinct placeholders — all are ignored.
- `out _` ignores `TryParse` out values when only success matters.
- Do not confuse discard `_` with proactive `private` field naming conventions in some styles.

---

#### Q4. Explain pattern matching enhancements in C# 7 — `is` type patterns and `switch` patterns.

**Answer:** C# 7 extends `is` to bind variables when a runtime type test succeeds (`if (obj is string s)`) and adds `switch` statement patterns that match on type, constants, and `when` guards in individual cases. This reduces sequential cast-then-check boilerplate.

- `case int i when i > 0:` combines type, binding, and guard in one label.
- Type patterns use reference or unboxing conversions — failed matches fall through.
- Later C# versions add switch expressions and relational patterns — see Cross-chapter Q9.
- Prefer pattern `switch` over long `if/else if` chains on heterogeneous types.

---

#### Q5. What are `ref` returns and `ref` locals, and what safety rules apply?

**Answer:** Methods can return `ref T` to alias existing storage such as array slots or struct fields, and locals can be declared `ref var x = ref array[i];` to mutate through aliases. Returned refs cannot point to dead stack memory — the compiler enforces that returned refs do not refer to locals that go out of scope.

- `ref` returns enable high-performance lookup tables without copying large structs.
- `ref readonly` returns add read-only aliasing for large readonly data.
- Callers assign through `ref` returns to mutate backing storage directly.
- Misuse is largely caught at compile time — do not return refs to temporaries.

---

#### Q6. What is `ref`/`in`/`out` in the context of `ReadOnlySpan`-era performance APIs (conceptual link)?

**Answer:** `ref` enables by-reference passing and returning; `out` requires the callee to assign before return; `in` passes arguments by readonly reference to avoid copying large structs while preventing callee mutation. Modern span-based APIs (`ReadOnlySpan<char>`, `Memory<byte>`) build on the same performance philosophy — minimize copies without unsafe pointers.

- `in T` is ideal for large readonly struct parameters like `in DateTime` in hot paths.
- `ref struct` types such as `Span<T>` must live on the stack — language rules enforce constraints.
- C# 7 ref locals pair with span slicing for zero-copy parsing.
- Module 06 covers threading; this link is about memory efficiency at API boundaries.

---

#### Q7. What are local functions, and how do they differ from lambdas for recursion and capture?

**Answer:** Local functions are methods declared inside another method, visible only to the containing method, and can be static or capture enclosing locals depending on modifiers. Unlike lambdas assigned to delegates, local functions can call themselves by name for recursion without declaring a delegate first.

- `static local` functions cannot capture instance or local state — clearer intent and fewer allocations.
- Local functions are not converted to delegates unless referenced as values — reducing indirection.
- Prefer local functions over private methods when helper logic is used once and keeps outer method readable.
- Lambdas remain better when passing inline callbacks to LINQ or event handlers.

---

#### Q8. What are expression-bodied members beyond properties (methods, constructors, finalizers)?

**Answer:** C# 7 allows `=>` expression bodies on methods, operators, indexers, constructors, finalizers, and property accessors when the implementation is a single expression, reducing ceremony for small members. The expression must satisfy the member's return or completion rules.

- Example: `public override string ToString() => $"{Name} ({Sku})";`
- Constructors can use `=>` only for chaining/initialization forms allowed by grammar — check compiler rules for your scenario.
- Expression-bodied members are still virtual/overridable according to member modifiers.
- Avoid cramming multi-step logic into expression bodies — use block bodies when clarity suffers.

---

#### Q9. What binary literals and digit separators (`0b1010`, `1_000_000`) improve in readability?

**Answer:** Binary literals (`0b...`) express bit masks and flags in the base they represent, and digit separators (`_`) break long numeric literals into human-readable groups without changing value. Both are compile-time constants with no runtime cost.

- `0b0000_1111` documents nibble structure in protocol code.
- Separators work in decimal, hex, and binary — `0xFFFF_0000` for color or flag words.
- Underscores cannot appear at the start or end of the literal token.
- Improves reviews of hardware and permissions constants compared to opaque decimal numbers.

---

#### Q10. What is `throw` as an expression inside ternary/null-coalescing forms?

**Answer:** C# 7 treats `throw` as an expression that can appear inside conditional or null-coalescing expressions, enabling compact validation such as `return value ?? throw new ArgumentNullException(nameof(value));`. The thrown exception propagates immediately — the overall expression does not produce a value.

- Removes separate `if (value == null) throw;` before return in guard clauses.
- Works in expression-bodied members and inline assignments.
- The thrown expression must be an exception type — not arbitrary statements.
- Prefer clear guard blocks when multiple validations precede return — readability over golf.

---

#### Q11. How do generalized async return types work (`ValueTask` as async return)?

**Answer:** C# 7 allows `async` methods to return any type that has a suitable `GetAwaiter` pattern or implements `IAsyncMethodBuilder`, not only `Task` and `Task<T>`. `ValueTask` and `ValueTask<T>` reduce allocations when async work often completes synchronously.

- Compiler selects `AsyncValueTaskMethodBuilder` for `ValueTask` returns.
- Callers still `await` the result — surface syntax is unchanged from `Task`.
- Library authors choose `ValueTask` for hot paths; public APIs document consumption rules (often single await).
- Module 06 covers task semantics; C# 7 is the language syntax enabling alternate return types.

---

#### Q12. What are `default` in generic constraints improvements in C# 7?

**Answer:** C# 7 allows `default(T)` for unconstrained type parameters and introduces the `default` literal in later point releases, making generic code easier when `T` might be reference or value type. Combined with `where T : class` or `struct` constraints, defaults initialize locals and return paths consistently.

- `T value = default;` in generic methods assigns null for reference types and zeroed structs for value types.
- Enables generic factories and caches without `Activator` for every default case.
- Nullable value types still use `default(Nullable<T>)` as null when `T` is struct.
- Pairs with `is null` / pattern matching in modern null-handling code.

---

### 06. C# 8 Features

#### Q1. Explain nullable reference types — how do they differ from `Nullable<T>` value types?

**Answer:** Nullable reference types (NRT) are compile-time annotations (`string?` vs `string`) warning when null may flow into non-nullable references, while `Nullable<T>` is a struct wrapper adding explicit null to value types like `int?`. NRT does not change CLR representation of references — only analyzer behavior.

- `int?` is `Nullable<int>` with `.HasValue`; `string?` is still `string` at runtime.
- NRT helps document API contracts and catch dereference bugs during build.
- `#nullable enable` toggles the annotation context per file or project.
- Value and reference nullability are orthogonal — `int?` and `string?` solve different problems.

---

#### Q2. What do `?`, `!`, and `#nullable` directives mean at compile time?

**Answer:** `?` on a reference type marks it as nullable annotation; the null-forgiving operator `!` suppresses a compiler warning at a specific expression telling the analyzer to treat the value as non-null; `#nullable enable/disable/restore` controls whether annotations and warnings apply in a source file region.

- `string? name` allows assignment of null with contextual warnings on dereference.
- `value!` does not change runtime behavior — it only affects static analysis.
- Overuse of `!` defeats NRT — fix flow analysis or guard with `if (value is null)`.
- Project-wide `<Nullable>enable</Nullable>` is the modern default for new SDK-style projects.

---

#### Q3. Are nullable reference annotations enforced at runtime?

**Answer:** No — NRT warnings are compile-time only; the runtime does not distinguish `string` from `string?`, and null references still throw `NullReferenceException` if dereferenced without guards. Correctness requires defensive checks, validation at boundaries, and tests — not annotations alone.

- See Gotcha 12 — enabling NRT does not inject automatic null checks into IL.
- Public API entry points should validate arguments regardless of annotations.
- Serialization and reflection can inject null into non-nullable annotated properties.
- Treat NRT as documentation plus static analysis, not a runtime safety net.

---

#### Q4. What are default interface methods, and how do they relate to the diamond problem?

**Answer:** C# 8 allows interfaces to declare method bodies with default implementations, so implementing types inherit the default unless they override. Multiple inheritance of implementation is still limited — classes inherit one base class; diamond conflicts among interface defaults are resolved by explicit implementation or most-specific override rules in the implementing class.

- Enables evolving interfaces without breaking every implementer — new members can have defaults.
- Implementers can `override` interface methods in the implementing class when the interface permits.
- Diamond ambiguity requires the class to pick which interface method to call — compiler errors guide resolution.
- Not a return to multiple class inheritance — interfaces only.

---

#### Q5. What are asynchronous streams (`IAsyncEnumerable<T>` and `await foreach`)?

**Answer:** `IAsyncEnumerable<T>` represents a sequence produced asynchronously, consumed with `await foreach` which awaits each `MoveNextAsync` and current element retrieval without blocking threads. Producers use `async IAsyncEnumerable` methods with `yield return` to emit items over time.

- Ideal for paging database results, streaming logs, or reading large files chunk by chunk.
- `ConfigureAwait` applies to awaits inside async iterators per consumer policy.
- Cancellation passes via `[EnumeratorCancellation] CancellationToken` on the async enumerable method.
- Module 06 covers async mechanics; C# 8 adds the language consumer syntax `await foreach`.

---

#### Q6. Explain null-coalescing assignment (`??=`) with examples.

**Answer:** The `??=` operator assigns the right-hand value only when the left-hand variable is currently null, combining null check and assignment in one expression: `cache ??= ComputeExpensive();`. It reduces boilerplate `if (cache == null) cache = ...` patterns.

- Works with properties with accessible setters and fields.
- Short-circuits — right side is not evaluated if left is non-null.
- Useful for lazy initialization of fields and dictionary entries.
- Distinct from `??` which does not assign — `??=` mutates the left target.

---

#### Q7. Explain range (`..`) and index (`^`) operators — how does `^1` differ from `Length - 1`?

**Answer:** The index operator `^n` counts from the end — `^1` is the last element, `^0` is one past the end (like `Length`) — while ranges `start..end` slice sequences using inclusive start and exclusive end semantics aligned with span slicing. `^1` is syntactic sugar that the compiler lowers to length-based indices.

- `array[^1]` equals `array[array.Length - 1]` when length is positive.
- Range `0..^0` spans the entire array — end is exclusive at the conceptual end index.
- Works on arrays, spans, and strings with unified syntax.
- Empty collections make `^1` invalid — same exceptions as out-of-range forward indices.

---

#### Q8. What are `using` declarations vs `using` statements for IDisposable?

**Answer:** C# 8 `using var resource = ...;` declares a disposable that stays in scope until the end of the enclosing block, disposing at block exit without an extra nested brace. Classic `using (var r = ...) { }` disposes at the end of the inner statement block — tighter lifetime when you want early disposal.

- `using var` reduces indentation for methods with one outer scope.
- Disposal order at block exit is reverse declaration order for multiple `using var` lines.
- Async disposal uses `await using var` with `IAsyncDisposable` — see Q10.
- Module 01 covers basic `using`; C# 8 adds declaration form ergonomics.

---

#### Q9. What are nullable-aware APIs in the BCL reacting to NRT (`NotNullWhen`, `MaybeNull`)?

**Answer:** Attributes such as `[NotNullWhen(true)]`, `[MaybeNull]`, `[NotNullIfNotNull]`, and `[AllowNull]` annotate BCL and user APIs so flow analysis understands null state changes across method calls. They do not affect runtime — they teach the compiler when null checks imply non-null results.

- `bool TryGetValue(..., [NotNullWhen(true)] out T? value)` tells analysis value is non-null when method returns true.
- `[MaybeNull]` on `T` return indicates generic `T` might be null even when unconstrained.
- Library authors should annotate public APIs when enabling NRT for consumers.
- Reduces false warnings without sprinkling `!` operators.

---

#### Q10. What is `IAsyncDisposable`, and how does `await using` work?

**Answer:** `IAsyncDisposable` defines `ValueTask DisposeAsync()` for resources requiring asynchronous teardown, and `await using` declares async-disposable locals disposed with awaited cleanup at block exit. It mirrors `using` for async I/O-bound disposal such as flushing network streams.

- `await using var conn = ...;` expands to try/finally calling `DisposeAsync`.
- Implement both `Dispose` and `DisposeAsync` carefully — document which callers should use.
- Nested `await using` disposes in reverse order like synchronous `using`.
- Prefer synchronous `Dispose` when cleanup is CPU-only and immediate.

---

#### Q11. What are static local functions, and why were they added?

**Answer:** `static` on a local function prevents it from capturing enclosing instance or local variables, forcing all data through parameters. This clarifies intent, avoids accidental closure allocations, and helps analyzers reason about thread safety and lifetime.

- Capture bugs in recursive helpers disappear when state is passed explicitly.
- Static local functions cannot reference outer locals — compiler error guides fixes.
- Pair with `ReadOnlySpan` parameters for parsing helpers inside methods.
- Non-static local functions still capture when needed — choose deliberately.

---

#### Q12. What is a `readonly struct`, and what mutability restrictions apply to its members?

**Answer:** A `readonly struct` promises the struct instance methods do not mutate instance state (except via `ref readonly` escapes), enabling clearer immutability and avoiding defensive copies on `in` parameters. Instance fields cannot be assigned except in constructors; `readonly` on the struct propagates to instance members implicitly in C# 8.

- Helps performance when large structs are passed with `in` — compiler skips hidden copies.
- Mutable structs are a common footgun — readonly structs document value semantics.
- Can still mutate via `ref` returns from APIs that expose interior mutability — design carefully.
- Distinct from `record struct` immutability defaults in later C# versions.

---

#### Q13. What is the `readonly` modifier on struct instance members (C# 8)?

**Answer:** Individual struct methods and properties can be marked `readonly` to promise they do not modify instance fields, even when the struct type itself is not marked `readonly struct`. Calls through `in` parameters avoid defensive copies when the member is readonly.

- Allows gradual immutability — mark mutating methods non-readonly, readers readonly.
- Compiler enforces that readonly instance members do not assign to fields.
- Improves intent in large structs like `Matrix` or custom numeric types.
- Combine with `readonly struct` for full type-level immutability when appropriate.

---

#### Q14. What are stackalloc in safe contexts and `Span<T>` integrations introduced alongside C# 8?

**Answer:** C# 8 allows `stackalloc` into `Span<T>` without an `unsafe` block when scoped correctly, integrating stack buffers with span-based APIs for parsing and formatting. Spans provide length-bounded views over stack, array, or native memory uniformly.

- `Span<char> chars = stackalloc char[128];` feeds char processing without heap arrays.
- Language rules prevent storing spans to heap fields — ref struct safety.
- Aligns with BCL methods accepting `ReadOnlySpan<char>` for zero-copy paths.
- See also C# 7 `ref` locals — together they form modern low-allocation patterns.

---

#### Q15. What is target-typed `new()` vs explicit type names?

**Answer:** C# 9 popularized target-typed `new()` where the type is inferred from context (`List<string> names = new();`), reducing repetition when the left-hand side already specifies the type. C# 8 set related groundwork in type inference improvements; explicit `new List<string>()` remains equivalent.

- Works for object creation, arrays when target-typed, and some pattern contexts in later versions.
- Improves readability when generic type arguments are long on both sides.
- Cannot infer when target type is ambiguous — add explicit type.
- Distinct from `var` — target-typed `new` requires an explicit left-hand type.

---

### Cross-chapter — Records & Pattern Matching *(C# 9–11; grouped here)*

#### Q1. What are records (C# 9), and what boilerplate do they synthesize?

**Answer:** Records are reference or value types optimized for immutable data carriers; the compiler synthesizes value-based equality, `GetHashCode`, `ToString`, and copy-with helpers depending on syntax. Positional record declarations also generate constructor parameters mapped to properties.

- `record class Person(string Name, int Age);` creates init-only properties and structural equality.
- Reduces manual `Equals`/`GetHashCode` for DTOs compared to classic classes.
- `with` expressions clone with selective overrides — see Q5.
- Choose records when identity is defined by data, not object reference alone.

---

#### Q2. What is the difference between `record class` and `record struct`?

**Answer:** `record class` declares a reference type with reference semantics and default nullability like classes, while `record struct` is a value type with copied storage and different equality boxing behavior. Both support value-based equality, but lifetime and mutability defaults differ.

- `record struct` avoids heap allocation for small immutable value bundles.
- Reference records still allocate on the heap — see Gotcha 13.
- Struct records can be readonly by declaration; class records use init accessors.
- Pick struct records for small composite keys; class records for larger shared DTO graphs.

---

#### Q3. How does value-based equality in records differ from default class equality?

**Answer:** Default classes use reference equality unless overridden; records override equality to compare values of all included data members in order, so two distinct instances with the same data compare equal. Hash codes combine member values consistently for dictionary use.

- Reference equality (`ReferenceEquals`) may still be false when value equality is true for records.
- Derived record equality includes base and derived members when properly declared.
- Serialization round-trips benefit — reconstructed DTOs equal originals by value.
- Mutable classes without overrides compare by reference — a common DTO bug records fix.

---

#### Q4. What is the difference between positional records and records with manual properties?

**Answer:** Positional syntax `record R(int Id, string Name);` declares primary constructor parameters that become init-only properties automatically, while manual records declare properties inside the body with optional custom validation or computed members. Both can be records with value equality when configured.

- Positional form is concise for flat DTOs; manual form suits complex initialization logic.
- Manual records can mix init and calculated properties not tied to constructor parameters.
- Primary constructor parameters are not always public fields — they become properties unless customized.
- Choose positional for API models; manual when encapsulation requires private setters or factories.

---

#### Q5. Explain `with` expressions — how do they relate to non-destructive mutation?

**Answer:** The `with` expression clones a record instance and overrides selected init properties, producing a new instance without mutating the original — non-destructive mutation. Syntax: `var updated = original with { Age = original.Age + 1 };`.

- Works on records with init accessors; classic mutable classes lack `with` unless customized.
- Under the hood the compiler synthesizes a copy constructor consuming member values.
- Ideal for functional-style updates in immutable domain models.
- Reference-type records still allocate new objects — not in-place field updates.

---

#### Q6. What are init-only setters (`init`), and how do they differ from `{ get; set; }` and `{ get; }`?

**Answer:** `init` accessors allow property assignment only during object initialization (object initializer, constructor, or `with`), preventing mutation after construction completes. `{ get; set; }` allows ongoing mutation; `{ get; }` without init allows assignment only in constructors declared in the type.

- Init properties support immutable DTOs deserialized from JSON when paired with constructors.
- After construction, `obj.Prop = x` fails for init-only properties.
- Records commonly use init for all data members.
- Distinct from `readonly` fields — init applies to properties with broader initializer syntax.

---

#### Q7. Can init-only properties be set inside the type's constructors after object creation semantics?

**Answer:** Init accessors are settable during the instance construction phase — including constructors and object initializers — before the object is fully constructed and exposed. Once construction completes, init properties behave like get-only from external code.

- Constructor bodies can assign init properties on `this` during construction.
- Deserializers use parameterized constructors or `[JsonInclude]` paths to satisfy init-only models.
- Do not confuse with post-construction mutation — external callers cannot re-init.
- Primary constructors in later C# versions map parameters to init properties automatically.

---

#### Q8. What is primary constructor syntax for records/classes (C# 12 preview cross-ref) vs positional records?

**Answer:** Positional records (C# 9) tie constructor parameters directly to generated properties in one declaration, while C# 12 primary constructors generalize parameter lists on any class or struct, capturing parameters into fields or properties with explicit body usage. Both reduce boilerplate but differ in generated members and inheritance rules.

- Positional `record R(T x)` always exposes property `x` unless customized.
- Primary constructors on classes may capture into private fields without auto-properties unless declared.
- Inheritance with primary constructors requires careful base constructor chaining.
- Use positional records for simple DTOs; primary constructors when mixing custom logic in the type body.

---

#### Q9. What is pattern matching in modern C# beyond C# 7 — switch expressions, relational, logical, and property patterns?

**Answer:** Modern pattern matching adds switch expressions (`var y = x switch { ... }`), property patterns matching nested shape, relational patterns comparing ordered values, and logical combinators `and`, `or`, `not`. Together they replace verbose cascade if-chains with exhaustive, expression-oriented rules.

- Switch expressions require a result expression in each arm — no fall-through statements.
- Property patterns destructure: `order is { Status: OrderStatus.Shipped, Total: > 0 }`.
- Relational patterns require types with ordering — numeric and enum cases common.
- Module 01 introduced basics; this cross-chapter covers C# 9–11 depth.

---

#### Q10. Explain property patterns (`person is { Age: > 18, Name: var n }`).

**Answer:** Property patterns match an object by inspecting nested property values and optionally binding variables from matched members. The pattern succeeds when the runtime type exposes the named properties and each nested pattern matches.

- Combines type testing, comparison, and variable binding in one expression.
- `var n` in a property pattern captures `Name` when the outer pattern matches.
- Null checks often prefix: `person is { Age: > 18 }` fails on null without throwing.
- Useful in validation pipelines and API authorization rules expressed declaratively.

---

#### Q11. What are relational patterns (`>`, `<=`) and combinator patterns (`and`, `or`, `not`)?

**Answer:** Relational patterns compare a matched value to constants using `<`, `<=`, `>`, `>=` in pattern positions, while combinator patterns join subpatterns with `and`, `or`, and `not` for boolean structure without nested ifs. They require compatible ordered types.

- Example: `x is > 0 and < 100` replaces range checks.
- `or` matches alternatives: `c is 'a' or 'e' or 'i'`.
- `not` negates a subpattern: `x is not null`.
- Compiler warnings highlight non-exhaustive combinations when enums omit cases.

---

#### Q12. What is list patterns (C# 11) — `[_, .., var last]`?

**Answer:** List patterns match sequences by structure — length, first/last elements, and slices — using syntax like `[head, .., tail]` on arrays, spans, and lists in pattern contexts. The discard `_` matches any element; `..` captures a slice subpattern.

- `[_, .., var last]` succeeds when at least two elements exist and binds `last`.
- Empty collection fails patterns requiring elements unless a separate `[]` arm exists.
- Enables concise parsing of command tokens and route segments.
- Combine with switch expressions for readable dispatch on string[] args.

---

#### Q13. What is `switch` expression vs traditional `switch` statement for exhaustiveness?

**Answer:** Switch expressions require every input to map to a resulting value with arms separated by `=>`, encouraging complete coverage of cases; traditional switch statements execute statements with fall-through controls and optional default without producing a value. Compiler exhaustiveness analysis is stronger on switch expressions over enums and tuples.

- Expression form: `var label = status switch { OrderStatus.New => "N", ... };`
- Statement form suits multi-step case bodies with local variables and loops.
- Non-exhaustive enum switch expressions warn when a case is missing — see Q14.
- Prefer expressions for mapping; statements for imperative case workflows.

---

#### Q14. What happens when a `switch` expression is not exhaustive over an enum?

**Answer:** The compiler emits a warning or error (depending on analysis level) when an enum switch expression omits a member and no discard arm catches the remainder, because a new enum value could arrive at runtime and throw `SwitchExpressionException` at execution. Adding `_ => ...` or listing all members restores exhaustiveness.

- API evolution adding enum values breaks non-exhaustive switches at runtime first.
- Treat warnings seriously in CI — they predict production exceptions on new enum members.
- Default discard arm documents intentional catch-all behavior.
- String switches cannot be exhaustively proven — enums are the primary case.

---

#### Q15. What is the difference between `is null` and `== null` when a type overloads `==`?

**Answer:** `is null` always performs a reference null check without invoking user-defined `==` overloads, while `== null` may call a static overloaded equality operator that could treat non-null instances as equal to null incorrectly. For nullable reference analysis, `is null` and `is not null` also integrate cleanly with flow tracking.

- Prefer `is null` / `is not null` for reference types with custom equality operators.
- `== null` remains common for value types and strings without surprising overloads.
- Pattern matching idioms (`if (x is not null)`) combine check and assignment.
- Unit tests should cover overloaded equality types explicitly.

---

#### Q16. What are expression trees (`Expression<T>`), and how do they differ from delegates?

**Answer:** Expression trees represent code as data structures (`Expression` nodes) that can be inspected, transformed, and compiled at runtime, whereas delegates are compiled callable targets without preserved structure. `Expression<Func<T>>` stores the lambda body as a tree; `Func<T>` stores IL to invoke directly.

- LINQ providers translate trees to SQL or other remote query languages.
- Not every C# lambda form is translatable — see Q18.
- Compile a tree once with `.Compile()` to produce a delegate for local execution.
- Trees enable dynamic predicate builders in filtering APIs.

---

#### Q17. How are expression trees used by LINQ providers (EF Core, `IQueryable`)?

**Answer:** `IQueryable` providers receive expression trees from LINQ query operators and translate member access, comparisons, and calls into provider-specific text such as SQL, executing remotely instead of in memory. The same lambda syntax compiles to a tree when the source is `IQueryable<T>` and to a delegate when the source is `IEnumerable<T>`.

- EF Core walks `Expression` nodes to build SQL with parameters — not arbitrary C# execution on the server.
- Method calls in trees must map to provider-supported functions or translations fail at runtime.
- `AsEnumerable()` switches to LINQ-to-Objects delegates — client evaluation boundary shifts.
- Debugging requires logging translated SQL, not assuming C# semantics off-process.

---

#### Q18. Why can't all C# lambdas be converted to expression trees?

**Answer:** Expression trees support only a subset of C# expressions — no statements blocks with loops, local functions, `ref` operations, or many statement-bodied patterns unless compiler can represent them as supported node types. Lambdas using unsupported constructs must compile to delegates only.

- See Gotcha 14 — EF queries fail when lambdas include unsupported calls.
- Statement-bodied lambdas with `{ ... }` often disqualify tree conversion.
- Null propagating operators and some null-forgiving forms have limited support depending on version.
- Use supported expression forms in `IQueryable` or fall back to `IEnumerable` client evaluation knowingly.

---

#### Q19. What is the difference between compile-time constant patterns and runtime type patterns?

**Answer:** Constant patterns match values known at compile time such as `case 0:` or `case "OK":`, while type patterns test runtime types and bind variables (`case Dog d:`). Constant patterns require compatible switch input type; type patterns interact with inheritance and casting rules at runtime.

- Enum cases use constant patterns with symbolic names.
- Type patterns may fail without throwing when used in `is` expressions.
- Mixing both in one switch is common in heterogeneous message dispatch.
- Switch input type determines which pattern forms are legal in each arm.

---

#### Q20. When should you prefer records over classes for DTOs and domain models?

**Answer:** Prefer records for immutable data transfer objects, event payloads, and value-centric domain concepts where equality should reflect data, not identity. Prefer classes when you need identity lifecycle, mutable aggregate behavior, or complex inheritance with reference semantics central to the model.

- API response models and message contracts fit records well with init-only properties.
- Entities with ORM change tracking and behavior-heavy aggregates often stay classes.
- Records reduce equality boilerplate — important for serialization tests and caching keys.
- Do not convert every class blindly — behavior-rich types benefit from explicit class design.

---

### Gotchas — Module 08

#### Gotcha 1. **`typeof` vs `GetType()`**

**Answer:** Developers treat `typeof(Base)` as interchangeable with `instance.GetType()` when serializing or reflecting, but `typeof` is fixed at compile time to the declared base type while `GetType()` returns the actual derived runtime type. Polymorphic scenarios then pick the wrong member set or serializer contract.

- `typeof(Animal)` never becomes `Dog` even when the variable holds a `Dog`.
- Factory and plugin code must call `GetType()` on instances for concrete behavior.
- Logging both values during bugs quickly exposes the mismatch.
- See Reflection Q3 for the full comparison.

---

#### Gotcha 2. **Serialization type loss**

**Answer:** Assigning `Animal ref = new Dog()` and serializing through the base-typed variable drops derived-only properties unless polymorphism is configured. The compile-time static type drives the default System.Text.Json contract, not the runtime object alone.

- Fix by enabling polymorphic options, serializing as the derived type, or flattening DTOs.
- Integration tests should cover every derived type in inheritance hierarchies on the wire.
- API designers often avoid deep inheritance on public JSON models because of this trap.
- See Serialization Q14–Q15.

---

#### Gotcha 3. **`[Serializable]` ignored by System.Text.Json**

**Answer:** Candidates assume the legacy `[Serializable]` attribute controls modern JSON or XML serializers, but System.Text.Json and typical XML serializers ignore it — it targeted binary formatter-era formatting. Modern contracts use JSON attributes or explicit options instead.

- `[Serializable]` does not make a type JSON-safe or include private fields automatically.
- BinaryFormatter honored `[Serializable]` — conflating the two eras causes wrong security assumptions.
- Use `[JsonPropertyName]` and related STJ attributes for JSON shape control.
- See Serialization Q23.

---

#### Gotcha 4. **`BinaryFormatter` is a security footgun**

**Answer:** Deserializing untrusted binary with `BinaryFormatter` can execute attacker-controlled object graphs, leading to remote code execution; the type is obsolete and removed or blocked on modern .NET. Teams still reach for it when porting legacy persistence without understanding the risk.

- Replace with JSON, protobuf, or other auditable formats for new persistence boundaries.
- Never accept BinaryFormatter payloads from clients or message queues.
- Migration projects should treat existing binary blobs as trusted-only internal data.
- See Serialization Q24–Q25.

---

#### Gotcha 5. **Missing JSON property on non-nullable value type**

**Answer:** When JSON omits a property mapped to a value type field, deserializers often default it to `0` or `false` without error, silently producing valid-looking but wrong business data. Reference types may become null; value types hide absence unless you add required validation.

- Use `required` members, `[JsonRequired]`, or custom validation after deserialize.
- Nullable value types (`int?`) distinguish missing from zero when configured carefully.
- Contract tests should include payloads missing optional-looking but business-critical fields.
- See Serialization Q10.

---

#### Gotcha 6. **Enum numeric wire values**

**Answer:** Default JSON enum serialization emits numeric values, so renumbering enum members in code breaks persisted documents and clients still sending old numbers. String enums trade size for stable, readable contracts across versions.

- Apply `JsonStringEnumConverter` for long-lived public APIs.
- Database-stored JSON inherits the same breakage when enums reorder.
- Document enum wire policy in API versioning guides.
- See Serialization Q11–Q12.

---

#### Gotcha 7. **`JsonSerializerOptions` not thread-safe for mutation**

**Answer:** Sharing one `JsonSerializerOptions` instance is good for performance, but mutating its properties concurrently while other threads serialize causes race conditions and subtle corruption. Configure options once, then treat them as read-only.

- Build and cache a configured static instance at startup.
- Do not add converters mid-request on a shared singleton options object.
- Clone options with `new JsonSerializerOptions(existing)` when tests need variations.
- See Serialization Q6.

---

#### Gotcha 8. **`dynamic` hides errors until runtime**

**Answer:** Code compiles when calling misspelled or nonexistent members on `dynamic`, failing only at execution with binder exceptions and blocking IDE refactor tools from updating call sites. Teams adopt `dynamic` for JSON convenience and inherit maintenance debt.

- Restrict `dynamic` to narrow interop boundaries covered by tests.
- Prefer `JsonNode` or typed DTOs for JSON when shape is known or evolvable with schema.
- Static analysis warnings disappear on dynamic flows — compensate with runtime validation.
- See Var/Dynamic Q5.

---

#### Gotcha 9. **Extension methods do not dispatch on `dynamic`**

**Answer:** Extension methods bind statically to the compile-time type, so `dynamic` receivers never see extensions even when the runtime type would match. Calls fail at runtime unless cast or invoked as static extension methods.

- `(ConcreteType)d).Extension()` or `MyExt.Extension(d)` are the escape hatches.
- LINQ-style fluent extensions on dynamic JSON models fail silently in design-time checks.
- Wrap dynamic payloads in typed adapters at boundaries instead.
- See Var/Dynamic Q15.

---

#### Gotcha 10. **Reflection string names don't refactor**

**Answer:** Code that looks up `"CalculateTotal"` by string survives compilation when the method is renamed, failing only at runtime during tests or production. Reflection-heavy pipelines need explicit tests or source generators to stay aligned with refactors.

- Prefer `nameof` for member names when APIs accept strings tied to symbols.
- Roslyn analyzers can flag magic strings in reflection calls in some setups.
- Plugin discovery by convention documents naming rules and tests assemblies at startup.
- See Reflection Q8–Q9.

---

#### Gotcha 11. **Regex without timeout on user input**

**Answer:** Patterns with nested quantifiers on attacker-controlled strings can hang the process indefinitely when no match timeout is configured. Public validation endpoints are common ReDoS targets if they compile user regex or apply complex patterns to long inputs.

- Always pass `matchTimeout` to regex used on external input.
- Cap input length before matching as a second layer.
- Consider `RegexOptions.NonBacktracking` for risky patterns in .NET 7+.
- See Regular Expressions Q5–Q7.

---

#### Gotcha 12. **Nullable reference types are annotations only**

**Answer:** Enabling `#nullable` warnings does not inject runtime null checks — null references still throw at dereference if data violates assumptions. Developers treat green builds as null-safe runtime guarantees without guards at API boundaries.

- Validate arguments and deserialize results explicitly.
- Combine NRT with `[NotNullWhen]` annotations on Try methods for flow analysis.
- Serialization can produce null into non-nullable annotated properties without compiler notice at runtime.
- See C# 8 Features Q3.

---

#### Gotcha 13. **Records are still reference types (`record class`)**

**Answer:** `record class` instances are heap objects compared by value but not by reference identity, which surprises developers expecting struct-like copying semantics or identity semantics from classic classes. `record struct` behaves differently on assignment and boxing.

- Assigning a record class copies the reference, not the data — mutate via `with` for new instances.
- Dictionary keys use value equality — two separate instances with same data collide as equal keys.
- Choose `record struct` when small immutable value semantics are required.
- See Cross-chapter Q2–Q3.

---

#### Gotcha 14. **Expression trees cannot contain statements arbitrarily**

**Answer:** Developers paste statement-heavy lambdas into EF Core or `IQueryable` queries assuming they run as C# on the server, but unsupported constructs cannot translate to SQL and throw at runtime or force client evaluation. The limitation is structural, not configurational.

- Keep query lambdas to supported expression-tree subsets.
- Inspect logged SQL to verify translation instead of assuming C# semantics remotely.
- Move complex logic to memory with `AsEnumerable()` knowingly, accepting performance cost.
- See Cross-chapter Q16–Q18.

---

## Module 09. Unit Testing

### 01. Unit Testing Basics

#### Q1. What is unit testing, and how does it differ from integration, component, and end-to-end testing?

**Answer:** Unit testing verifies a small isolated piece of logic — typically one class or method — with dependencies replaced or stubbed so failures localize to that unit. Integration tests exercise multiple real collaborators (database, file system, HTTP), component tests bound a larger slice such as a service layer, and end-to-end tests drive the full application stack as a user would.

- Unit tests run fast and in parallel because they avoid I/O and network.
- Integration tests catch wiring and configuration mistakes unit tests cannot see.
- E2E tests are fewer and slower — they validate critical user journeys.
- The test pyramid recommends many unit tests, fewer integration, fewest E2E.

---

#### Q2. Explain the AAA pattern (Arrange, Act, Assert) and why order matters psychologically for readers.

**Answer:** AAA structures each test into setup (Arrange), execution (Act), and verification (Assert) in that order so readers quickly find inputs, the operation under test, and expected outcomes. Consistent ordering reduces cognitive load even when a test is technically one line.

- Arrange creates SUT, inputs, and doubles — no assertions yet.
- Act invokes exactly one logical operation when possible — multiple acts obscure failure cause.
- Assert checks outcomes without further mutation — mixing act and assert confuses failure diagnosis.
- Blank lines or comments between phases help code reviews scan tests.

---

#### Q3. What makes a good unit test (FIRST / TRICE — fast, isolated, repeatable, self-validating, timely)?

**Answer:** Good unit tests run quickly, do not depend on external state or order, produce the same result every run, assert pass/fail automatically without manual inspection, and are written close to the code they protect (timely). FIRST (Fast, Independent, Repeatable, Self-validating, Timely) and TRICE are mnemonic variants of the same principles.

- Slow tests are skipped in local loops — fast feedback keeps developers running them.
- Isolation means no shared mutable globals or live databases in unit scope.
- Self-validating excludes tests that require a human to read console output.
- Timely TDD writes tests before or with production code, not months later.

---

#### Q4. What is the System Under Test (SUT), and how do you identify its boundaries?

**Answer:** The SUT is the class, method, or module whose behavior the test intends to prove correct; everything else is a dependency to stub, fake, or ignore. Boundaries sit at public methods you would call from production code — not private helpers tested directly.

- Name tests after SUT behavior: `CalculateSubtotal_TwoLines_ReturnsSum`.
- If a test needs many collaborators, the SUT boundary may be too large — split or integration-test instead.
- One primary SUT per test clarifies failure messages.
- Constructor-injected interfaces mark natural seams for doubles.

---

#### Q5. What is test coverage, and why can 100% line coverage still miss important bugs?

**Answer:** Coverage metrics measure which lines or branches executed during tests, but executing a line does not prove correct assertions or edge cases were tested. You can hit every line with weak asserts and still ship logic bugs on untested combinations.

- Branch coverage matters more than line coverage for conditional logic.
- Coverage ignores incorrect expected values — asserts must be meaningful.
- Generated code and trivial getters inflate line percentages without risk reduction.
- Use coverage to find gaps, not as a quality finish line.

---

#### Q6. What is mutation testing, and how does it critique coverage metrics?

**Answer:** Mutation testing automatically introduces small code mutations (change `>` to `>=`, remove calls) and checks whether tests fail — surviving mutants indicate tests that execute code but do not detect faults. It measures test effectiveness beyond mere execution counts.

- High line coverage with all mutants surviving means weak assertions.
- Mutation testing is slower and used selectively on critical modules.
- Guides adding assertions on boundary values and error paths.
- Complements coverage rather than replacing human review of test intent.

---

#### Q7. What is the difference between state-based and interaction-based testing?

**Answer:** State-based tests assert outputs or object state after acting on the SUT, while interaction-based tests verify that collaborators were called with expected arguments via mocks or spies. State-based tests survive refactors better when behavior outcome stays the same but call patterns change.

- Prefer asserting return values, persisted state, or side effects on fakes over `Verify` on every call.
- Interaction tests suit fire-and-forget collaborators like email senders when outcome is the call itself.
- Over-interaction testing couples tests to implementation — see Mocking Q11.
- Many tests combine both — assert state and one critical interaction.

---

#### Q8. What is a test fixture, and how is it different from a test case?

**Answer:** A test fixture is the shared context or environment for one or more tests — data, database seed, browser page — while a test case is a single test method executing one scenario against that context. Fixtures reduce duplication; test cases remain independent scenarios.

- xUnit `IClassFixture<T>` provides fixture instances per test class.
- A fixture setup failure can skip or fail all dependent tests in the class.
- Do not hide Act logic inside fixtures — keep scenarios visible in test methods.
- MSTest `[TestInitialize]` builds per-test fixture state — see MSTest Q4.

---

#### Q9. What are flaky tests, and what common causes (time, threading, shared state, external I/O)?

**Answer:** Flaky tests pass and fail intermittently without code changes, destroying trust in CI. Common causes include time-dependent logic (`DateTime.Now`), race conditions, shared mutable static state, reliance on external networks, and assuming test execution order.

- Fix by injecting clock abstractions, synchronizing threads, or isolating state per test.
- Random seeds must be fixed when testing probabilistic code.
- Parallel runners expose order dependencies hidden in sequential runs.
- Quarantine flaky tests temporarily but fix or delete — do not retry indefinitely in CI without investigation.

---

#### Q10. What is the test pyramid, and where do unit tests sit relative to integration tests?

**Answer:** The test pyramid visualizes many fast unit tests at the base, a moderate layer of integration tests, and a small peak of slow end-to-end tests. Unit tests provide rapid feedback on logic; upper layers validate composition and deployment reality.

- Inverted pyramids (many UI E2E, few units) slow CI and blur failure diagnosis.
- Integration tests often use real databases in containers — slower but realistic.
- Unit tests mock or fake dependencies taught in Module 09 chapter 04.
- Balance shifts slightly for infrastructure-heavy apps but the principle holds.

---

#### Q11. What is TDD (Red-Green-Refactor), and what benefits/challenges does it bring?

**Answer:** Test-driven development writes a failing test first (Red), implements minimal code to pass (Green), then refactors with tests guarding behavior. Benefits include clearer design, immediate regression safety, and executable specifications; challenges include learning curve and risk of over-mocking when driving design purely from tests.

- Red ensures the test actually detects the missing behavior.
- Green discourages speculative features not demanded by tests.
- Refactor step improves structure without changing external behavior.
- Not mandatory for all code — apply where feedback loops help most.

---

#### Q12. When should a bug fix include a regression test?

**Answer:** Every fixed production bug should include a regression test reproducing the failure scenario when feasible, so the defect cannot return silently in a future refactor. The test documents the broken case and proves the fix.

- Start from the failing inputs reported in the bug ticket.
- Assert the correct behavior, not just that an exception no longer throws without checking output.
- Link test names or comments to ticket ids for traceability when helpful.
- Skip only when reproduction requires impractical E2E infrastructure — then add the closest unit-level proxy.

---

#### Q13. What is the difference between testing public behavior vs internal implementation?

**Answer:** Public behavior tests call the same API production uses and remain stable when internals refactor; implementation tests reach private methods or verify every internal call, breaking when code is reorganized without behavior change. Tests should specify what the SUT promises, not how it fulfills the promise.

- See Gotcha 1 — testing private methods usually signals missing seams.
- Assert observable outcomes — return values, exceptions, state on fakes.
- Refactoring freedom is a main benefit of unit tests — do not sacrifice it with brittle tests.
- Extract collaborators when logic is hard to test through public API alone.

---

#### Q14. How do deterministic tests handle `DateTime.Now`, `Guid.NewGuid()`, and randomness?

**Answer:** Inject abstractions such as `TimeProvider` / `IClock`, `IGuidGenerator`, or seeded `Random` instances so tests control time and random values. Direct static calls make assertions depend on the clock at run time and introduce flakiness.

- Pass fixed `DateTime` from stub clock into SUT methods.
- Seed random for reproducible sequences while still exercising random code paths.
- Avoid sleeping in tests — use task completion sources or fakes for async timing.
- Module 09 mocking chapter shows substituting time and HTTP abstractions.

---

#### Q15. What is arrange duplication, and when is shared setup justified vs harmful?

**Answer:** Arrange duplication repeats similar setup across tests; shared setup via constructors, fixtures, or helper methods reduces noise but can hide scenario-specific inputs that make failures hard to diagnose. Share immutables and expensive resources; keep each test's unique arrange visible in the test method.

- `[TestInitialize]` and xUnit constructors run before every test — good for fresh SUT.
- Shared mutable collections in fixtures cause order-dependent failures — see Gotcha 2.
- Helper methods named `CreateValidOrder()` clarify intent when duplication is purely boilerplate.
- Avoid mega-setup that only a subset of tests need — split fixture classes instead.

---

### 02. xUnit

#### Q1. What is xUnit.net, and how does its philosophy differ from MSTest and NUnit?

**Answer:** xUnit.net is a modern .NET test framework emphasizing isolation — new test class instance per test, minimal built-in lifecycle attributes, and extensibility through fixtures and parallelization defaults. MSTest integrates tightly with Visual Studio tooling and uses different lifecycle attributes; NUnit offers a rich attribute model with longer history.

- xUnit avoids `[SetUp]`/`[TearDown]` in favor of constructors and `IAsyncLifetime`.
- Parallel test execution is on by default in xUnit — encourages independent tests.
- Many open-source .NET projects standardize on xUnit for new work.
- All three run under `dotnet test` with appropriate adapters.

---

#### Q2. Explain `[Fact]` vs `[Theory]` — when is parameterized testing appropriate?

**Answer:** `[Fact]` marks a single test with no parameters — one scenario path. `[Theory]` runs the same test logic multiple times with different inputs supplied by data attributes, ideal for validation rules, boundary values, and format checks without duplicating method bodies.

- Use `[Fact]` when setup or logic differs materially between scenarios.
- Theories keep one assert block — inputs change, expected outputs map per case.
- Each theory row is an independent test result in runners and CI reports.
- Tutorial `DiscountTheoryTests` demonstrates pricing rules with `[InlineData]`.

---

#### Q3. How do `[InlineData]`, `[MemberData]`, and `[ClassData]` supply theory inputs?

**Answer:** `[InlineData(...)]` embeds constant arguments directly on the test method for simple cases; `[MemberData(nameof(Method))]` pulls rows from a static property or method returning `IEnumerable<object[]>` for richer datasets; `[ClassData(typeof(MyData))]` delegates to a class implementing `IEnumerable<object[]>` for reusable data classes.

- Inline data must be compile-time constants compatible with attribute rules.
- MemberData can build scenarios programmatically or load from files in the member.
- ClassData separates large datasets into dedicated types for reuse across tests.
- Theory method parameters must match the arity and types of each data row.

---

#### Q4. How does xUnit create test class instances — per test or per class?

**Answer:** xUnit constructs a **new instance of the test class before each test method** runs, so instance fields start fresh and constructor injection runs per test. This differs from MSTest's default of one instance per class unless configured otherwise.

- Per-test instances prevent field leakage between tests on the same class.
- Constructor runs for every `[Fact]` and every theory row — keep constructors lightweight.
- Expensive shared setup belongs in `IClassFixture<T>`, not heavy constructors.
- See Gotcha 8 when fixtures share mutable state incorrectly.

---

#### Q5. What are `IClassFixture<T>` and `ICollectionFixture<T>`, and when use each?

**Answer:** `IClassFixture<T>` injects a shared fixture instance into all tests in one test class, created once before any test in that class runs. `ICollectionFixture<T>` shares a fixture across multiple test classes grouped in the same collection definition, amortizing expensive setup like database containers.

- Class fixture: `public MyTests(MyDatabaseFixture f)` constructor injection.
- Collection fixture: `[Collection("Db")]` on classes plus `[CollectionDefinition("Db")]` on the fixture class.
- Fixtures must be thread-safe if tests in the collection run in parallel — often serialize the collection instead.
- Tutorial `OrderCatalogFixture` demonstrates shared catalog data across tests.

---

#### Q6. How do collection definitions (`[Collection("Name")]`) serialize tests that share expensive resources?

**Answer:** Tests in the same xUnit collection run sequentially with respect to that shared resource, even when parallelization is enabled globally, preventing two tests from mutating the same non-thread-safe fixture concurrently. Define the collection once and assign tests that must not overlap.

- `[CollectionDefinition("SerialDb", DisableParallelization = true)]` on the fixture type.
- Multiple classes can join one collection name to share one fixture instance.
- Overusing collections slows CI — scope serialization to tests that truly need it.
- Prefer immutable fixtures or isolated databases when parallel speed matters.

---

#### Q7. What is `IAsyncLifetime`, and how does it replace async setup/teardown patterns?

**Answer:** Implement `IAsyncLifetime` on the test class with `InitializeAsync` and `DisposeAsync` for async setup and teardown that cannot run in a synchronous constructor. xUnit awaits these around each test instance lifecycle when the test class implements the interface.

- Use for opening async database connections or seeding via async APIs.
- `DisposeAsync` cleans resources even when tests fail — pair with try/finally patterns inside if needed.
- Do not block async setup with `.GetAwaiter().GetResult()` in constructors — use IAsyncLifetime instead.
- Class fixtures can also implement async lifetime for shared async resources.

---

#### Q8. How does xUnit handle parallel test execution by default, and how do you disable it?

**Answer:** xUnit runs tests in parallel across collections by default using thread pool workers, maximizing throughput when tests are independent. Disable parallelization with `[assembly: CollectionBehavior(DisableTestParallelization = true)]` or per-collection definitions when shared state requires serialization.

- Parallelism exposes shared static mutable state bugs — see Gotcha 2.
- `[Fact]` tests in the same class may run in parallel unless in a serial collection.
- Tune max parallelism via configuration in `xunit.runner.json` when needed.
- CI agents benefit from parallel tests if CPU cores are available.

---

#### Q9. What is `ITestOutputHelper`, and how is it injected into tests?

**Answer:** `ITestOutputHelper` captures per-test output lines visible in Visual Studio Test Explorer and `dotnet test` diagnostic logs when injected via constructor parameter. Use it instead of `Console.WriteLine` for debug traces tied to a specific test run.

- Constructor signature: `public MyTests(ITestOutputHelper output)`.
- Call `_output.WriteLine("debug: {0}", value)` during arrange or assert diagnostics.
- Output appears only when the test runs — not a substitute for assertions.
- Tutorial `OutputHelperTests` demonstrates captured lines on failure investigation.

---

#### Q10. How do `[Trait("Category", "Slow")]` attributes help filter tests in CI?

**Answer:** Traits attach key-value metadata to tests; runners filter with expressions like `Category=Slow` to include or exclude sets during CI fast lanes vs nightly builds. xUnit has no built-in `[Category]` — traits are the flexible replacement.

- Fast PR pipeline: `dotnet test --filter "Category!=Slow"`.
- Run slow integration subsets on schedule with `--filter Category=Slow`.
- Multiple traits allowed — combine with `&` and `|` in filter expressions.
- Consistent trait keys across the team matter for filters to work.

---

#### Q11. How does constructor injection of dependencies work in xUnit test classes?

**Answer:** xUnit resolves constructor parameters from fixtures (`IClassFixture<T>`, `ICollectionFixture<T>`) and `ITestOutputHelper` automatically; you declare them as constructor parameters and store in readonly fields for tests. Each test method receives a new class instance with fresh injection.

- `public OrderTests(OrderCatalogFixture catalog, ITestOutputHelper output)` is typical.
- xUnit does not auto-resolve arbitrary interfaces — only framework-provided services and fixtures you register via collections.
- Keeps tests explicit about shared dependencies compared to hidden static singletons.
- Heavy work belongs in fixture classes, not constructor body beyond assignments.

---

#### Q12. What happens if a test constructor throws — how does xUnit report it?

**Answer:** xUnit marks the test as failed during construction — the test method never runs — and reports the constructor exception as the failure reason. Every test in that class fails construction independently if the fault persists per instance.

- Misconfigured fixtures surface here before Act/Assert run.
- Keep constructors thin — validate fixture injection, do not perform full arrange.
- Async failures in constructors should move to `IAsyncLifetime.InitializeAsync`.
- Fix constructor exceptions before interpreting test logic failures.

---

#### Q13. How do you assert exceptions with `Assert.Throws<T>` vs `Assert.ThrowsAsync<T>`?

**Answer:** `Assert.Throws<T>(() => action())` executes synchronous code and expects exception type `T`. `Assert.ThrowsAsync<T>(() => task)` awaits async code and captures exceptions thrown from the returned task — must be awaited in an async test method.

- See Gotcha 10 — forgetting `await` on `ThrowsAsync` hides failures.
- Overload accepts `Func<Task>` for async lambdas returning tasks.
- Assert on exception properties after capture: `var ex = await Assert.ThrowsAsync<...>(() => ...);`
- Prefer specific exception types over base `Exception` when documenting contract.

---

#### Q14. What is the difference between returning `Task` from a test vs `async void`?

**Answer:** Async tests should return `Task` (or `Task<T>`) so the runner awaits completion and observes exceptions; `async void` is fire-and-forget — exceptions may crash the process or go unreported as test failures. xUnit supports `public async Task MyTest()`.

- See Gotcha 4 — never use `async void` for test methods.
- `[Fact] public async Task ...` is the standard pattern.
- Synchronous `[Fact] public void` remains correct for non-async tests.
- MSTest shares the same `Task` vs `async void` guidance.

---

#### Q15. How do you run xUnit tests from CLI (`dotnet test`) and filter by fully qualified name?

**Answer:** Run `dotnet test path/to/project.csproj` from the solution or test project directory; filter with `--filter FullyQualifiedName~Namespace.ClassName.MethodName` using substring or exact match syntax. Combine filters for traits, categories, and names.

- Example: `dotnet test --filter "FullyQualifiedName~CalculateSubtotal"`.
- `--logger "console;verbosity=detailed"` shows `ITestOutputHelper` lines.
- Run entire solution: `dotnet test Testing.sln` from Module 09 folder.
- CI pipelines cache packages then invoke `dotnet test` with filter expressions per stage.

---

### 03. MSTest

#### Q1. What NuGet packages compose an MSTest project (`MSTest.TestFramework`, `MSTest.TestAdapter`, `Microsoft.NET.Test.Sdk`)?

**Answer:** `MSTest.TestFramework` supplies attributes and assert APIs; `MSTest.TestAdapter` discovers and runs tests for `dotnet test` and Visual Studio; `Microsoft.NET.Test.Sdk` is the shared test host SDK required by all .NET test projects. Together they wire MSTest into the unified .NET test platform.

- SDK-style projects reference all three packages explicitly or via metapackage patterns in templates.
- Version alignment between adapter and framework avoids discovery mismatches.
- `dotnet test` invokes VSTest host which loads the MSTest adapter.
- Same Test SDK package is used by xUnit and NUnit projects.

---

#### Q2. Explain `[TestClass]`, `[TestMethod]`, and how discovery finds tests.

**Answer:** `[TestClass]` marks a class containing tests; `[TestMethod]` marks individual test methods the adapter executes. Discovery scans assemblies for public classes with `[TestClass]` and public instance methods tagged `[TestMethod]` meeting signature rules (typically parameterless or with `[DataRow]`).

- Non-public test classes or methods are ignored unless configured otherwise.
- Async test methods return `Task` for proper awaiting.
- Duplicate or invalid signatures fail at discovery with build or run warnings.
- Parallel to xUnit `[Fact]` on a class without special attributes.

---

#### Q3. What are `[DataTestMethod]` and `[DataRow]` equivalents to xUnit theories?

**Answer:** `[DataTestMethod]` marks a parameterized test method, and multiple `[DataRow(...)]` attributes supply argument sets executed as separate test cases — MSTest's parallel to xUnit `[Theory]` with `[InlineData]`. Each row appears individually in Test Explorer results.

- Rows must match method parameter count and compatible types.
- `[DynamicData]` extends beyond compile-time constants — see Q15.
- Display names can customize row labels for readability in CI output.
- Tutorial `DiscountCalculatorDataTests` uses data rows for pricing scenarios.

---

#### Q4. What is `[TestInitialize]` / `[TestCleanup]` vs `[ClassInitialize]` / `[ClassCleanup]` vs `[AssemblyInitialize]` / `[AssemblyCleanup]`?

**Answer:** `[TestInitialize]` and `[TestCleanup]` run before and after **each** test method; `[ClassInitialize]` and `[ClassCleanup]` run once per test class; `[AssemblyInitialize]` and `[AssemblyCleanup]` run once per test assembly. Scope increases up the hierarchy — choose the narrowest scope that satisfies setup needs.

- Per-test initialize gives fresh state — preferred for unit test isolation.
- Class-level setup amortizes cost but risks shared mutable state — see Gotcha 7.
- Assembly-level setup loads expensive resources once for all tests in the DLL.
- Cleanup counterparts run even when tests fail — release resources there.

---

#### Q5. Why must `[ClassInitialize]` and `[AssemblyInitialize]` be `static`?

**Answer:** Class and assembly initialize methods run before any test instance exists, so MSTest invokes them as static methods on the test class without constructing the class. Instance `[TestInitialize]` runs after the test object is created for each method.

- Signature: `public static void ClassInit(TestContext context)` with optional `TestContext` parameter.
- Static init cannot access instance fields on the test class — store shared state in static fields carefully.
- Mutable static state without per-test reset causes order-dependent failures.
- Prefer instance setup when each test needs its own SUT.

---

#### Q6. What is the `[TestContext]` property, and what runtime services does it expose?

**Answer:** MSTest injects a `TestContext` property on test classes when declared, exposing current test name, fully qualified name, deployment directory, properties from run settings, and outcome metadata. Tests use it for logging paths and conditional logic based on run configuration.

- `[TestContext] public TestContext Context { get; set; }` must be public settable property.
- `Context.TestName` helps build unique temp file names per test.
- `Context.Properties` reads `.runsettings` key-value pairs in CI.
- xUnit uses different mechanisms (`ITestOutputHelper`, fixtures) for similar needs.

---

#### Q7. How does MSTest instance lifecycle differ from xUnit's new-instance-per-test model?

**Answer:** MSTest traditionally creates **one instance of the test class per class per test run** and reuses it for all test methods in that class, calling `[TestInitialize]` before each method. xUnit creates a new instance before every test method, which reduces accidental shared instance field leakage.

- Instance fields set in one MSTest method may persist to the next on the same object unless reset in `[TestInitialize]`.
- Both call `[TestInitialize]` per test — MSTest on the same instance, xUnit on a new instance.
- Design MSTest tests to reset mutable state in `[TestInitialize]` explicitly.
- See Gotcha 7 for static class initialize pitfalls.

---

#### Q8. What are `[ExpectedException]` / `[ExpectedExceptionAttribute]` (legacy), and why is `Assert.ThrowsException` preferred?

**Answer:** `[ExpectedException(typeof(T))]` marks a test expecting an exception type during the whole method — fragile because any line may throw and satisfy the attribute incorrectly. `Assert.ThrowsException<T>(() => ...)` scopes the expected throw to a specific action with clearer intent.

- Legacy attribute is deprecated style — not repeated in xUnit chapter by design.
- `ThrowsException` fails if no exception or wrong type occurs.
- Async variant `Assert.ThrowsExceptionAsync<T>` awaits task-returning actions.
- Aligns with xUnit `Assert.Throws` patterns for consistent exception testing.

---

#### Q9. What Assert helpers exist in MSTest (`Assert.AreEqual`, `Assert.IsTrue`, `Assert.ThrowsException`, `StringAssert`)?

**Answer:** MSTest provides `Assert.AreEqual` with optional delta for doubles, reference and string comparisons, `Assert.IsTrue`/`IsFalse`, null checks, collection helpers, `Assert.ThrowsException`, and `StringAssert` for contains/start/end case variants. API names differ from xUnit but concepts match.

- `StringAssert.Contains` validates substring messages in exception or output tests.
- `CollectionAssert` compares enumerables element-wise.
- `Assert.Inconclusive` marks tests not yet implemented — use sparingly in CI.
- Tutorial `DiscountCalculatorAssertTests` demonstrates MSTest assert style.

---

#### Q10. How do you deploy test content files (`[DeploymentItem]`) — and what are modern alternatives?

**Answer:** `[DeploymentItem("source/path", "output/subdir")]` copies files to the deployment directory referenced by `TestContext.DeploymentDirectory` during test runs — common in legacy MSTest for sample CSVs and configs. Modern SDK-style projects prefer `<Content Include="..." CopyToOutputDirectory="PreserveNewest" />` in the csproj, which works across test frameworks.

- Deployment items are MSTest-specific — xUnit uses output directory content copying instead.
- Paths are relative to project structure — broken paths fail silently or at runtime.
- Embed small fixtures as resources when file system deployment is awkward.
- Container and integration tests mount volumes instead of deployment items.

---

#### Q11. What is `[Ignore]` / `[TestCategory]`, and how do you filter categories in `dotnet test`?

**Answer:** `[Ignore]` skips a test with optional message; `[TestCategory("Slow")]` tags tests for filtering. `dotnet test --filter TestCategory=Slow` runs category subsets similar to xUnit traits.

- Ignored tests report as skipped — do not leave ignored tests indefinitely without tickets.
- Multiple categories require filter expression syntax with `|` for OR.
- Exclude slow tests in PR builds: `--filter "TestCategory!=Integration"`.
- Align category names with CI pipeline stage definitions.

---

#### Q12. How does MSTest parallelization work (`Parallelize` attribute at assembly/class level)?

**Answer:** `[assembly: Parallelize]` or `[Parallelize]` on a class enables parallel test execution within scope subject to runsettings and CPU limits; `[DoNotParallelize]` forces sequential execution for tests sharing unsafe state. Parallelism is opt-in in many MSTest versions unlike xUnit default-on.

- Workers count configured in `.runsettings` `MaxCpuCount`.
- Class-level parallel still requires thread-safe fixtures and no shared mutable statics.
- See Gotcha 11 — never assume test order when parallel is enabled.
- Scope parallelization to independent unit tests only.

---

#### Q13. What is the difference between MSTest V1 and V2/V3 adapters in SDK-style projects?

**Answer:** MSTest V1 targeted legacy .csproj with different runner integration; V2/V3 adapters run on the cross-platform `Microsoft.TestPlatform` with SDK-style projects, improved discovery, and better `dotnet test` support. Modern repos use V2+ adapters exclusively.

- SDK-style `net8.0` test projects require V2/V3 adapter packages.
- V1 paths confuse migration docs — verify package names in csproj.
- V3 continues evolution of adapter with Visual Studio 2022+ integration.
- Feature parity with xUnit for async tests and data rows is solid on V2+.

---

#### Q14. When would teams choose MSTest over xUnit in greenfield .NET projects?

**Answer:** Teams deeply invested in Visual Studio Enterprise test management, Azure DevOps MSTest-centric templates, or organizational standards may pick MSTest for consistency; many open-source libraries prefer xUnit for parallel defaults and fixture ergonomics. Technical merit is close — choice is often toolchain alignment.

- MSTest familiarity in enterprise .NET Framework migrations carries forward.
- xUnit leads in cross-platform OSS examples and ASP.NET Core samples.
- Both run under `dotnet test` — switching cost is moderate.
- Pick one per repo and document patterns to avoid mixing lifecycle models confusingly.

---

#### Q15. How do MSTest data sources (`[DynamicData]`) compare to xUnit `[MemberData]`?

**Answer:** `[DynamicData(nameof(GetData), DynamicDataSourceType.Method)]` binds test methods to static methods or properties returning `IEnumerable<object[]>` — the same conceptual model as xUnit `[MemberData]`, with MSTest-specific attribute syntax and optional display name formatting per row.

- Supports property or method data sources with `DynamicDataSourceType`.
- Enables building rows from files, databases, or combinatorial generators at discovery time.
- xUnit `[ClassData]` separates data classes — MSTest can use similar patterns manually.
- Keep data methods deterministic — random rows without seeds cause flaky theories.

---

### 04. Mocking & Test Doubles

#### Q1. Define the test double taxonomy — dummy, fake, stub, spy, mock; what distinguishes each?

**Answer:** Test doubles replace real dependencies in tests: a **dummy** fills parameters without being used; a **fake** has working simplified implementation (in-memory repo); a **stub** returns canned answers; a **spy** records calls for later assertion; a **mock** (strict sense) verifies expected interactions. Informally "mock" often means any double.

| Kind | Role |
|---|---|
| Dummy | Placeholder, never exercised |
| Fake | Lightweight real behavior |
| Stub | Canned responses |
| Spy | Records interactions |
| Mock | Verifies expected calls |

- Tutorial `OrderService` uses stub inventory and fake email sender before Moq.
- Taxonomy clarifies test intent in code reviews — see Gotcha 12.
- Not every double needs a framework — hand-rolled stubs are fine.

---

#### Q2. What is a mock in the strict sense (interaction verification) vs informal "mock" meaning any fake?

**Answer:** In the strict Gerard Meszaros sense, a mock object asserts expected interactions during or after the test — failure means calls did not match the specification. Informally developers call any test substitute a "mock," blurring stub-only setups that never verify calls.

- Moq `Verify` turns a stubbed proxy into a strict interaction mock.
- Tests that only `Setup` returns without `Verify` behave as stubs despite `Mock<T>` type name.
- Over-using strict mocks produces brittle tests — see Q11.
- Name tests by behavior under proof, not by double type used.

---

#### Q3. What is a stub, and when do you configure return values without verifying calls?

**Answer:** A stub provides predetermined responses so the SUT can exercise paths without real infrastructure — configure `HasStock` to return false to test cancellation without verifying that `HasStock` was called with specific arguments unless that call is the behavior under test.

- Focus assert on `OrderResult` state, not inventory method call counts.
- Moq: `mock.Setup(x => x.HasStock("SKU", 5)).Returns(true)` without `Verify`.
- Hand-rolled `StubInventoryService` in tutorial maps SKUs to fixed quantities.
- Stubs keep tests resilient when internal call order changes.

---

#### Q4. What is a spy, and how does it record interactions for later assertion?

**Answer:** A spy wraps or subclasses a real or fake object to record method calls, arguments, and counts for assertions after Act completes. Moq mocks act as spies when you `Verify` interactions or inspect `Invocation` lists.

- Fake email sender appending to `SentMessages` list is a manual spy pattern.
- Spies help verify side effects like "email sent once" without full mock frameworks.
- Distinguish spying on collaborators from spying on SUT internals — prefer collaborator boundaries.
- Clear recorded state between tests to avoid cross-test pollution.

---

#### Q5. What is a fake (e.g., in-memory repository), and when is it preferable to mocks?

**Answer:** A fake implements enough real behavior to support tests meaningfully — an in-memory dictionary repository honors add/query rules — without mocks verifying each call. Fakes suit testing outcomes that emerge from collaborator logic, not just single return values.

- Prefer fakes when collaborator behavior grows complex — fewer fragile setups than many `Setup` chains.
- `FakeEmailSender` stores messages for assert without Moq.
- Fakes cost more code upfront but improve readability for domain-heavy tests.
- Combine fake repository with real SUT orchestration for state-based tests.

---

#### Q6. What is dependency injection's role in making code testable?

**Answer:** Constructor injection of interfaces (`IInventoryService`, `IEmailSender`) lets production wire real implementations while tests inject doubles at the same seams without changing SUT code. Dependencies become explicit instead of hidden static singletons.

- `OrderService` tutorial accepts interfaces — tests pass stubs, production passes SQL/SMTP.
- DI containers in ASP.NET Core register production types — tests bypass container with `new OrderService(stub, fake)`.
- Seams at interfaces enable substitution — see Q15.
- Code without DI often requires refactor before meaningful unit tests.

---

#### Q7. When should you use a mocking framework (Moq, NSubstitute, FakeItEasy) vs hand-written fakes?

**Answer:** Use hand-written fakes when behavior is stable, reused across many tests, and readability matters; use mocking frameworks when you need quick one-off return setups, exception simulation, or verify calls on interfaces without maintaining fake classes. Frameworks shine for narrow interaction tests; fakes shine for rich collaborator behavior.

- Moq tests live in `OrderServiceMoqTests.cs` in the tutorial project.
- Manual doubles tests in `OrderServiceManualDoubleTests.cs` compare readability.
- Framework mocks require learning API and virtual/interface rules — see Q8–Q9.
- Start simple — introduce Moq when stub classes proliferate.

---

#### Q8. What is the difference between mocking an interface vs a concrete class?

**Answer:** Mocking frameworks generate proxies for interfaces easily because all members are abstract from the proxy's view; concrete classes require mockable virtual members or interceptable interfaces — non-virtual methods cannot be overridden by proxy subclasses.

- Prefer depending on interfaces in SUT design for testability and loose coupling.
- Mocking concrete classes ties tests to implementation type and inheritance constraints.
- Sealed classes cannot be mocked except via wrapper interfaces.
- Production code benefits from interface seams even when only tests use them initially.

---

#### Q9. Why can't Moq intercept non-virtual methods on concrete classes?

**Answer:** Moq (Castle DynamicProxy) subclasses or implements types overriding virtual methods or interface members; non-virtual concrete methods dispatch directly and bypass the proxy override, so `Setup` on them has no effect. Only virtual/overridable members are interceptable on classes.

- See Gotcha 3 — mocking concrete SQL providers fails for non-virtual methods.
- `virtual` keyword enables interception at cost of inheritance design.
- Interface mocks avoid the problem entirely — preferred seam.
- Some frameworks offer different strategies — Moq follows standard proxy limits.

---

#### Q10. What is `Mock<T>.Setup`, `Returns`, `Callback`, and `Verify` in Moq terms?

**Answer:** `Setup` defines how the mock responds when a member is called; `Returns` supplies return values; `Callback` runs side effects when invoked; `Verify` asserts the member was called with expected arguments and counts after Act. Together they configure stubs and optional interaction checks.

- Example: `mock.Setup(m => m.Send(It.IsAny<Order>())).Callback(o => captured = o);`
- `Verify(m => m.Send(It.IsAny<Order>()), Times.Once)` is post-act assertion.
- `Throws` on setup simulates failures from collaborators.
- `It.Is<T>(predicate)` matches argument conditions flexibly.

---

#### Q11. What is over-specification / brittle mocking, and how does it couple tests to implementation?

**Answer:** Over-specification verifies internal call sequences, private collaborators, or every method invoked during Act, so harmless refactors break tests despite unchanged external behavior. Tests become change detectors for implementation, not guards for requirements.

- See Gotcha 5 — verify only critical interactions (email sent), not every repository query order.
- Assert final state on `OrderResult` first; add minimal `Verify` when side effect is the requirement.
- Extract new classes when tests demand many internal verifications — design smell.
- State-based tests survive reordering internal steps.

---

#### Q12. What is the difference between verifying behavior (`Verify`) vs asserting output state?

**Answer:** Behavior verification checks that collaborators were called correctly; output state assertion checks the SUT's return value or mutated state regardless of how it achieved the result. State assertions align with user-visible requirements; behavior verification documents necessary side effects like sending email.

- Successful order: assert `result.Success` and optionally `Verify` email once.
- Failed stock: assert cancellation without verifying every internal logging call.
- Over-reliance on `Verify` encodes algorithm steps, not business rules.
- Fakes with captured state bridge both — assert on fake's message list without Moq.

---

#### Q13. How do you mock `async` methods returning `Task` / `Task<T>`?

**Answer:** Moq `ReturnsAsync(value)` or `Returns(Task.FromResult(value))` stubs async methods; `ThrowsAsync(ex)` simulates async failures. The SUT awaits normally — no special test harness beyond async test methods returning `Task`.

- `Setup(x => x.LoadAsync(It.IsAny<int>())).ReturnsAsync(entity);`
- Avoid returning null task — return `Task.CompletedTask` for void async methods.
- `Verify` on async methods same as sync after awaiting SUT method in test.
- Ensure test method is `async Task` and awaits SUT async entry points.

---

#### Q14. How do you substitute `HttpClient`, `ILogger<T>`, and `DateTime` abstractions in tests?

**Answer:** Wrap `HttpClient` behind an interface or use `HttpMessageHandler` mocks with `new HttpClient(handler)` returning canned responses; substitute `ILogger<T>` with `NullLogger<T>.Instance` or a test logger collecting entries; abstract time with `TimeProvider` / `IClock` injected instead of `DateTime.UtcNow`.

- `Mock<HttpMessageHandler>` with protected `SendAsync` setup is a common HttpClient pattern.
- Log verification asserts critical errors without mocking every log call.
- Fixed clock enables deterministic expiry and scheduling tests — see Basics Q14.
- Avoid hitting real network or wall clock in unit tests.

---

#### Q15. What is a seam, and how do partial wrappers or interfaces introduce testability?

**Answer:** A seam is a place where you can substitute behavior without editing the SUT — constructor-injected interface, virtual method override, or wrapper around static system APIs. Interfaces are the cleanest seam; partial wrappers isolate static calls like `File.ReadAllText` behind injectable gateways.

- Extract `IFileSystem` when file I/O blocks unit testing.
- Seams should exist in production code paths, not `#if TEST` hacks only.
- Partial classes can wrap generated code seams sparingly — prefer explicit interfaces.
- Every seam is a design commitment — keep surface minimal.

---

#### Q16. When is integration testing with real dependencies better than mocking everything?

**Answer:** Integration tests validate SQL translations, serialization formats, HTTP middleware pipelines, and configuration wiring that mocks cannot faithfully represent. Use them for repository queries against real databases, contract tests against external APIs in sandboxes, and startup composition tests.

- Mock unit tests prove logic; integration tests prove the system connects correctly.
- Testcontainers and local SQL Express run real dependencies in CI selectively.
- See Gotcha 6 — do not label database tests as unit tests in folder structure.
- Pyramid still limits count — target high-risk integration boundaries.

---

#### Q17. What are anti-patterns: mocking concrete DB providers, verifying private collaborators, testing framework code?

**Answer:** Mocking sealed concrete database drivers, verifying calls on private methods via reflection, and asserting xUnit/MSTest behavior are wastes of effort and sources of brittle failures. Test your application code through public seams; trust framework vendors to test their frameworks.

- Mock interfaces your code owns; use real DB or container for data access integration.
- Private method tests break on extract-method refactors — test public outcomes.
- Do not unit test Moq or Assert classes — focus on SUT.
- See Gotcha 1 and Gotcha 5 for related guidance.

---

#### Q18. How do manual test doubles (hand-rolled stubs) compare to framework mocks for readability?

**Answer:** Hand-rolled stubs are explicit classes with clear fields and methods — easy for newcomers to debug — while framework mocks compress setup into fluent chains that scale for many one-off interactions but hide complexity. Teams often start manual and adopt Moq when duplication grows.

- Tutorial compares `OrderServiceManualDoubleTests` vs `OrderServiceMoqTests`.
- Manual fakes reusable across tests justify their file size.
- Moq reduces boilerplate for single-test interaction verification.
- Readability wins when tests are read more often than written — choose accordingly.

---

### Gotchas — Module 09

#### Gotcha 1. **Testing private methods directly**

**Answer:** Tests that invoke private methods through reflection or `InternalsVisibleTo` tie themselves to implementation layout, so extracting or renaming helpers breaks tests without changing user-visible behavior. The symptom is a design smell — the private logic likely belongs in a collaborator with a public seam.

- Test through public methods on the SUT or extract a small class with an interface.
- Refactoring becomes risky when private method tests outnumber behavior tests.
- `InternalsVisibleTo` is acceptable for testing internal modules in libraries — not for every private helper.
- See Basics Q13.

---

#### Gotcha 2. **Shared mutable static state**

**Answer:** Static fields mutated by one test leak into others, especially under parallel runners where order is nondeterministic. Tests pass locally in isolation but fail in CI intermittently when another test ran first and left dirty state.

- Reset static state in `[TestInitialize]` or avoid static mutable data in tests entirely.
- xUnit parallel collections serialize only when configured — statics remain dangerous globally.
- Prefer instance fields and fresh fixtures per test.
- See Basics Q9 and xUnit Q8.

---

#### Gotcha 3. **Mocking concrete classes with non-virtual members**

**Answer:** Developers mock concrete repository or service classes and wonder why `Setup` has no effect on methods — non-virtual members dispatch directly on the real type inside Moq proxies. The fix is to depend on interfaces or make members virtual deliberately for testing — rarely the best design.

- Interface extraction is cleaner than widening virtual surface on production classes.
- Sealed framework types cannot be mocked — wrap them.
- Integration tests cover concrete providers mocks cannot represent.
- See Mocking Q8–Q9.

---

#### Gotcha 4. **`async void` test methods**

**Answer:** Async test methods declared `async void` do not return a Task the runner awaits, so exceptions may terminate the process or never mark the test failed. MSTest and xUnit both expect `async Task` for async tests.

- Signature: `public async Task MyTest()` not `public async void MyTest()`.
- Event handler `async void` pattern does not apply to tests.
- See xUnit Q14.
- CI silent passes with failing async void are especially dangerous.

---

#### Gotcha 5. **Over-verifying mock calls**

**Answer:** Tests that `Verify` every repository getter and logger call fail when implementation reorders internal steps despite identical outcomes. Maintainers disable or delete those tests instead of fixing production bugs.

- Verify one or two critical side effects — email sent, payment captured.
- Assert return objects and exception types first.
- Refactoring should not require updating twenty Verify lines.
- See Mocking Q11–Q12.

---

#### Gotcha 6. **Integration tests disguised as unit tests**

**Answer:** Tests hitting real SQL Server, filesystem paths, or HTTP endpoints run slowly, flake on environment differences, and get labeled "unit" in project folders — confusing pyramid expectations and PR gate timing. Name, folder, and category should reflect scope honestly.

- Move database tests to integration projects with longer CI stages.
- Use `[Trait("Category", "Integration")]` or MSTest `[TestCategory]` filters.
- Unit test project should run in seconds on a laptop.
- See Basics Q1 and Mocking Q16.

---

#### Gotcha 7. **MSTest `[ClassInitialize]` sharing mutable state**

**Answer:** Static setup in `[ClassInitialize]` runs once per class; if tests mutate shared collections without reset in `[TestInitialize]`, later tests see corrupted data. MSTest reuses one test class instance across methods, amplifying leakage.

- Copy immutable snapshots per test or rebuild state in `[TestInitialize]`.
- Prefer instance fixtures over mutable statics when possible.
- See MSTest Q4–Q7.
- Symptom: tests pass alone, fail in full suite.

---

#### Gotcha 8. **xUnit class fixtures shared across unrelated tests**

**Answer:** `IClassFixture<T>` shares one instance per test class — if unrelated tests in the same class mutate fixture data, order-dependent failures appear because xUnit may run tests in parallel within the class. Fixtures should expose immutable or resettable state.

- Split test classes when scenarios need different fixture configurations.
- Use collection serialization when fixture is truly shared and mutable.
- See xUnit Q5–Q6.
- Constructor injection makes shared fixture visible — document mutability rules.

---

#### Gotcha 9. **Theory data referencing mutable objects**

**Answer:** `[MemberData]` returning a shared list that one test mutates corrupts rows for subsequent theory executions or other tests reading the same collection reference. Data sources should return fresh objects or immutable rows per enumeration.

- Return `new object[] { new object[] { new List<int> { 1 } } }` per yield, not one shared list.
- Defensive copy in test method if data source cannot be changed.
- See xUnit Q3.
- Symptom: theory passes first row, fails later rows with identical expected values.

---

#### Gotcha 10. **Not awaiting async assertions**

**Answer:** Calling `Assert.ThrowsAsync` without `await` starts the assertion but does not fail the test when the exception occurs later on a unobserved task. The test method completes green while the failure is lost.

- Always `await Assert.ThrowsAsync<...>(() => sut.MethodAsync());`
- Same rule for MSTest `Assert.ThrowsExceptionAsync`.
- See xUnit Q13.
- Enable analyzer warnings for unawaited tasks in test projects where possible.

---

#### Gotcha 11. **Assuming test execution order**

**Answer:** Tests must not depend on running in source file order or a previous test leaving state ready — parallel and optimized runners reorder freely. Explicit ordering attributes are exceptions, not defaults.

- Each test arranges its own prerequisites completely.
- `[TestMethod(Order = n)]` in MSTest is fragile — avoid except legacy suites.
- xUnit has no guaranteed order within a class.
- See Basics Q9 and flaky test causes.

---

#### Gotcha 12. **Confusing stub with mock**

**Answer:** Teams say "mock the database" when they only stub return values without caring about calls, then add `Verify` everywhere thinking mocks are required — mixing vocabulary leads to over-specified tests. Stubs feed data; mocks (strict) police interactions.

- Use stub setups when outcome assertion suffices.
- Add `Verify` only when interaction is part of the requirement (email must send).
- Hand-rolled fakes often replace Moq for clarity.
- See Mocking Q1–Q2 and taxonomy table.

---