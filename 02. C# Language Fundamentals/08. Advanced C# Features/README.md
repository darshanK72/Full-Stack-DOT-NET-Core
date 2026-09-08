# Advanced C# Features — Interview Q&A Index

Cross-topic index covering serialization, reflection, regex, type system keywords, and modern C# 7/8 language features in .NET 10.

---

## Table of Contents

| # | Topic | File |
|---|-------|------|
| 01 | Serialization & Deserialization | [INTERVIEW_QA.md](01.%20Serialization%20%26%20Desiralization/INTERVIEW_QA.md) |
| 02 | Reflection & Attributes | [INTERVIEW_QA.md](02.%20Reflection%20%26%20Attributes/INTERVIEW_QA.md) |
| 03 | Regular Expressions | [INTERVIEW_QA.md](03.%20Regular%20Expressions/INTERVIEW_QA.md) |
| 04 | Var, Dynamic & Special Keywords | [INTERVIEW_QA.md](04.%20Var%20Dynamic%20%26%20Special%20Keywords/INTERVIEW_QA.md) |
| 05 | C# 7 Features | [INTERVIEW_QA.md](05.%20C%23%207%20Features/INTERVIEW_QA.md) |
| 06 | C# 8 Features | [INTERVIEW_QA.md](06.%20C%23%208%20Features/INTERVIEW_QA.md) |

---

## Cross-Cutting Questions

---

## CQ1. How do reflection, attributes, and source generators form a spectrum for metadata-driven behavior, and when should you choose each?

**Concepts**
- attributes — declarative metadata baked into IL at compile time
- reflection — runtime inspection of types, members, and attributes
- source generators — compile-time code synthesis triggered by attributes or syntax
- performance cost: reflection allocates and bypasses JIT inlining
- `System.Text.Json` source generation as a concrete example
- AOT compatibility: reflection breaks Native AOT; source generators enable it

**Answer**

Attributes are pure metadata: they annotate types and members with structured information but do nothing on their own. Reflection reads that metadata at runtime to make decisions — serializers discover `[JsonPropertyName]`, validators read `[Required]`, ORMs inspect `[Column]`. This is expressive and requires no build step, but runtime reflection is the slowest point on the spectrum: it allocates `MemberInfo` objects, bypasses JIT optimizations, and is incompatible with .NET Native AOT trimming because the linker cannot know which types will be inspected.

Source generators sit at compile time. A generator observes your source code (often by finding specific attributes or interfaces), then emits additional C# source into the compilation. `System.Text.Json`'s `[JsonSerializable]` source generator produces a concrete `JsonSerializerContext` subclass with serialization logic woven in — no runtime reflection required, fully trimmer-safe, and measurably faster because the serialization code is real compiled methods rather than delegate chains assembled at first use.

The practical choosing rule in .NET 10: use attributes everywhere to express intent. Use reflection only in framework-internal code or tooling where the target types are truly unknown at build time. For any hot path — serialization, validation, DI container registration — prefer source generators or the T4/Roslyn analysis approach. When building a library that must support AOT, source generators are not optional; they are the architecture.

---

## CQ2. How do nullable reference types interact with `var`, `dynamic`, and special keywords like `is`, `as`, and `nameof`?

**Concepts**
- nullable reference types (NRT) — compiler-enforced null-safety annotations
- `var` — compile-time type inference, NRT flow preserved
- `dynamic` — runtime dispatch, bypasses NRT analysis entirely
- `is` pattern matching — produces a non-nullable local in the matched branch
- `as` — returns nullable reference; requires null check
- `nameof` — null-safe, evaluated at compile time

**Answer**

Nullable reference types (enabled by `<Nullable>enable</Nullable>` in .NET 10 projects by default) add a static analysis layer that tracks whether a reference can be null. `var` participates fully: when you write `var name = person.Name` the inferred type carries the nullability annotation of `Name` — if `Name` is `string?`, then `name` is `string?` and the compiler warns on unguarded dereference. `var` does not hide nullability; it propagates it.

`dynamic` is the opposite extreme. Assignments to or from `dynamic` suppress all NRT analysis because the type is resolved at runtime by the DLR. Null flowing through a `dynamic` variable produces no compiler warning, making it a blind spot in your null-safety story. Treat `dynamic` boundaries as trust boundaries: validate for null immediately when converting back to a concrete type.

`is` with a pattern binding (`if (obj is string s)`) gives you a non-nullable `s` inside the branch because the pattern only matches when the value is not null and is of the target type. This is the preferred null-and-type check in one expression. `as`, by contrast, always produces a nullable (`string? s = obj as string;`) and requires an explicit null check before use — it is useful when you expect failure to be common and want to avoid exception cost, but it offers no compiler help if you forget the check. `nameof` is orthogonal to nullability: it evaluates the identifier at compile time to a string literal and can safely be used on nullable members without a null check because no instance is ever dereferenced.

---

## CQ3. How do C# 7 and C# 8 features combine — pattern matching, switch expressions, records, and nullable — to replace older imperative code patterns?

**Concepts**
- C# 7 pattern matching — `is` type patterns, `when` guards, tuple deconstruction
- C# 8 switch expressions — expression-form exhaustive dispatch
- C# 9 records — value-equality, `with` expressions, immutability by default
- C# 8 nullable reference types — pairs with records' non-nullable init properties
- positional patterns and property patterns — structural matching without casting
- exhaustiveness checking — compiler warns on missing arms

**Answer**

These features are designed to compose. A typical pre-C# 7 pattern was a chain of `if (x is SomeType) { var t = (SomeType)x; ... }` blocks followed by a final `else throw`. C# 7 replaced the cast-check pair with `if (x is SomeType t)` and introduced `when` guards for additional conditions. C# 8 switch expressions unified this into a single expression where every arm is `pattern => result`, the compiler enforces exhaustiveness (warns on a missing discard arm `_`), and the whole construct produces a value rather than executing side effects.

Records (C# 9, heavily refined through C# 10/12) add the data layer. A `record` with positional parameters is implicitly deconstructable, so switch expression positional patterns work directly against record instances without any boilerplate. Combined with C# 8 nullable reference types, a record's properties are non-nullable by default unless annotated with `?`, making the entire data model self-documenting about null expectations and eliminating a whole class of defensive null checks inside switch arms.

In practice the combination produces code that reads like a specification: a switch expression over a record hierarchy with property patterns expresses business rules as a flat, scannable table of cases. Adding a new case to the hierarchy causes a compiler warning everywhere the switch is not updated — an architectural safety net that traditional polymorphism with virtual dispatch does not provide without extra tooling. This is why domain modeling in .NET 10 overwhelmingly prefers sealed record hierarchies with switch expressions over class hierarchies with overridden methods for value-like concepts.

---

## CQ4. How does serialization interact with reflection, attributes, `dynamic`, and C# 8+ features to affect correctness and performance?

**Concepts**
- `[JsonPropertyName]`, `[JsonIgnore]` — attribute-driven serialization contract
- reflection-based vs. source-generated `System.Text.Json`
- `dynamic` deserialization and `JsonElement`
- records as serialization DTOs — constructor-based deserialization
- nullable annotations on serialized types
- AOT trimming and `[JsonSerializable]` source generation

**Answer**

Serialization is the intersection where nearly every Advanced C# topic converges. `System.Text.Json` uses attributes to define the contract: `[JsonPropertyName]` renames a property in JSON without renaming it in C#, `[JsonIgnore]` excludes sensitive fields, and `[JsonConstructor]` directs deserialization through a specific constructor — which records exploit naturally because their primary constructor becomes the target automatically.

The reflection-based serializer reads these attributes at first use and caches the resulting metadata. This is convenient but incurs startup cost and is incompatible with .NET Native AOT because the trimmer cannot statically determine which types will be serialized. The `[JsonSerializable]` source generator eliminates this by generating the serialization logic at compile time; in .NET 10 projects this is the recommended path for any type that crosses a serialization boundary in a performance-sensitive or AOT-deployed context.

`dynamic` and `JsonElement` are the escape hatches for truly schema-unknown JSON. `JsonDocument.Parse` gives you a `JsonElement` tree that you navigate with `.GetProperty`, `.GetString`, and so on — no reflection, no runtime type binding, but also no type safety. Nullable reference types on your DTO classes integrate with source-generated serialization: a non-nullable `string Name` causes the serializer to throw on a missing JSON field rather than silently producing a null that later causes a NullReferenceException deep in business logic. Aligning nullability annotations with serialization expectations is therefore both a documentation practice and a runtime correctness guarantee.
