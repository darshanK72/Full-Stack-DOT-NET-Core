# 08. Advanced C# Features — Interview Q&A
> Back to [README](../README.md)

## Table of Contents

- [01. Serialization & Deserialization](#01-serialization-deserialization)
  - [Q1. What is serialization and deserialization, and what is an ob…](#01-serialization-deserialization-q1)
  - [Q2. Does deserialization resurrect original object identity or c…](#01-serialization-deserialization-q2)
  - [Q3. Compare JSON, XML, and binary as wire formats — trade-offs f…](#01-serialization-deserialization-q3)
  - [Q4. Explain `System.Text.Json.JsonSerializer.Serialize` and `Des…](#01-serialization-deserialization-q4)
  - [Q5. What is `JsonSerializerOptions`, and which settings affect n…](#01-serialization-deserialization-q5)
  - [Q6. Why is creating a new `JsonSerializerOptions` on every call …](#01-serialization-deserialization-q6)
  - [Q7. When should you use `JsonSerializerContext` source generator…](#01-serialization-deserialization-q7)
  - [Q8. Explain `[JsonPropertyName]`, `[JsonIgnore]`, `[JsonInclude]…](#01-serialization-deserialization-q8)
  - [Q9. What happens when JSON contains properties not present on th…](#01-serialization-deserialization-q9)
  - [Q10. What happens when JSON is missing a property mapped to a non…](#01-serialization-deserialization-q10)
  - [Q11. How do enums serialize by default in `System.Text.Json`, and…](#01-serialization-deserialization-q11)
  - [Q12. How do you serialize enums as strings using `JsonStringEnumC…](#01-serialization-deserialization-q12)
  - [Q13. How do you handle circular references in an object graph (`R…](#01-serialization-deserialization-q13)
  - [Q14. Why does serializing `Animal pet = new Dog()` sometimes drop…](#01-serialization-deserialization-q14)
  - [Q15. How do you enable polymorphic serialization in modern `Syste…](#01-serialization-deserialization-q15)
  - [Q16. What is `JsonNode`, `JsonObject`, and `JsonArray`, and when …](#01-serialization-deserialization-q16)
  - [Q17. How do you navigate and mutate JSON with `JsonNode` without …](#01-serialization-deserialization-q17)
  - [Q18. What is a custom `JsonConverter<T>`, and when would you impl…](#01-serialization-deserialization-q18)
  - [Q19. How do `Utf8JsonReader` and `Utf8JsonWriter` differ from `Js…](#01-serialization-deserialization-q19)
  - [Q20. Explain `XmlSerializer` requirements (parameterless construc…](#01-serialization-deserialization-q20)
  - [Q21. What do `[XmlRoot]`, `[XmlElement]`, `[XmlAttribute]`, and `…](#01-serialization-deserialization-q21)
  - [Q22. Why can `XmlSerializer` fail at runtime even when the projec…](#01-serialization-deserialization-q22)
  - [Q23. What is `[Serializable]` actually used for in modern .NET?](#01-serialization-deserialization-q23)
  - [Q24. Explain `BinaryFormatter` — why is it obsolete, and what sec…](#01-serialization-deserialization-q24)
  - [Q25. What are recommended modern alternatives to `BinaryFormatter…](#01-serialization-deserialization-q25)
  - [Q26. How does `DateTime` with unspecified `Kind` behave across ti…](#01-serialization-deserialization-q26)
  - [Q27. Why is `DateTimeOffset` often safer on the wire than `DateTi…](#01-serialization-deserialization-q27)
  - [Q28. Can a type with only get-only properties serialize but fail …](#01-serialization-deserialization-q28)
  - [Q29. How do `[JsonConstructor]` and parameterized constructors in…](#01-serialization-deserialization-q29)
  - [Q30. What is the difference between `System.Text.Json` and `Newto…](#01-serialization-deserialization-q30)

- [02. Reflection & Attributes](#02-reflection-attributes)
  - [Q1. What is reflection in C#, and what problems does it solve?](#02-reflection-attributes-q1)
  - [Q2. What is the difference between early binding and late bindin…](#02-reflection-attributes-q2)
  - [Q3. Explain `typeof(T)` vs `obj.GetType()` — compile-time token …](#02-reflection-attributes-q3)
  - [Q4. Why does `typeof(List<int>) == typeof(List<string>)` return …](#02-reflection-attributes-q4)
  - [Q5. How do you obtain the generic type definition from a closed …](#02-reflection-attributes-q5)
  - [Q6. What is the `Type` class, and what members expose metadata (…](#02-reflection-attributes-q6)
  - [Q7. What is `BindingFlags`, and how do `Instance`, `Static`, `Pu…](#02-reflection-attributes-q7)
  - [Q8. How do you enumerate properties, methods, fields, and constr…](#02-reflection-attributes-q8)
  - [Q9. How do you invoke a method dynamically via `MethodInfo.Invok…](#02-reflection-attributes-q9)
  - [Q10. What is `Activator.CreateInstance`, and how do you pass cons…](#02-reflection-attributes-q10)
  - [Q11. How do you create generic types at runtime (`MakeGenericType…](#02-reflection-attributes-q11)
  - [Q12. How do you get and set property and field values through `Pr…](#02-reflection-attributes-q12)
  - [Q13. How can reflection access private members, and why is that a…](#02-reflection-attributes-q13)
  - [Q14. Explain `Assembly`, `Module`, `MemberInfo`, `MethodInfo`, `P…](#02-reflection-attributes-q14)
  - [Q15. What is the difference between `Assembly.Load`, `Assembly.Lo…](#02-reflection-attributes-q15)
  - [Q16. Can you unload an assembly in .NET Framework vs .NET Core / …](#02-reflection-attributes-q16)
  - [Q17. How do you discover and read custom attributes at runtime (`…](#02-reflection-attributes-q17)
  - [Q18. How do you define and apply your own attribute classes (`Att…](#02-reflection-attributes-q18)
  - [Q19. What are performance costs of reflection vs compiled code, a…](#02-reflection-attributes-q19)
  - [Q20. What is `Reflection.Emit`, and when is dynamic IL generation…](#02-reflection-attributes-q20)
  - [Q21. How does reflection interact with nullable reference type an…](#02-reflection-attributes-q21)
  - [Q22. What security permissions historically gated reflection, and…](#02-reflection-attributes-q22)

- [03. Regular Expressions](#03-regular-expressions)
  - [Q1. What is the purpose of the `Regex` class in C#?](#03-regular-expressions-q1)
  - [Q2. Explain `Regex.IsMatch`, `Match`, `Matches`, `Replace`, and …](#03-regular-expressions-q2)
  - [Q3. What is the difference between verbatim regex strings (`@"\d…](#03-regular-expressions-q3)
  - [Q4. What are common metacharacters candidates should know (`.`, …](#03-regular-expressions-q4)
  - [Q5. What is catastrophic backtracking, and what pattern shapes t…](#03-regular-expressions-q5)
  - [Q6. How do you mitigate ReDoS using `Regex.MatchTimeout` or the …](#03-regular-expressions-q6)
  - [Q7. What happens when a regex times out — which exception is thr…](#03-regular-expressions-q7)
  - [Q8. What is atomic grouping's role in preventing backtracking ex…](#03-regular-expressions-q8)
  - [Q9. How does culture affect case-insensitive matching, and what …](#03-regular-expressions-q9)
  - [Q10. When should you compile regexes with `RegexOptions.Compiled`…](#03-regular-expressions-q10)
  - [Q11. What is `RegexOptions.NonBacktracking` (.NET 7+), and what t…](#03-regular-expressions-q11)
  - [Q12. How do named capture groups work, and how do you read them f…](#03-regular-expressions-q12)
  - [Q13. What is the difference between greedy and lazy quantifiers (…](#03-regular-expressions-q13)
  - [Q14. When should you prefer `Regex` over simple `string.Contains`…](#03-regular-expressions-q14)
  - [Q15. How do you validate input with regex without using it as a f…](#03-regular-expressions-q15)

- [04. Var, Dynamic & Special Keywords](#04-var-dynamic-special-keywords)
  - [Q1. Explain `var` — what is known at compile time vs runtime?](#04-var-dynamic-special-keywords-q1)
  - [Q2. When is `var` required (anonymous types) vs merely convenien…](#04-var-dynamic-special-keywords-q2)
  - [Q3. Explain the `dynamic` keyword and the DLR's role.](#04-var-dynamic-special-keywords-q3)
  - [Q4. What is the difference between `var`, `dynamic`, and `object…](#04-var-dynamic-special-keywords-q4)
  - [Q5. When does `dynamic` defer member binding to runtime, and wha…](#04-var-dynamic-special-keywords-q5)
  - [Q6. What is `DynamicObject`, and when would you subclass it?](#04-var-dynamic-special-keywords-q6)
  - [Q7. What is `ExpandoObject`, and how does it differ from `Dictio…](#04-var-dynamic-special-keywords-q7)
  - [Q8. Explain `nameof` — how does it help refactoring and logging?](#04-var-dynamic-special-keywords-q8)
  - [Q9. What is the `global::` qualifier, and when is it needed to d…](#04-var-dynamic-special-keywords-q9)
  - [Q10. What is `default` literal (C# 7.1+) vs `default(T)`?](#04-var-dynamic-special-keywords-q10)
  - [Q11. What is `@` verbatim identifier syntax (`@class`, `@event`) …](#04-var-dynamic-special-keywords-q11)
  - [Q12. What is unsafe code, and when are pointers justified in C#?](#04-var-dynamic-special-keywords-q12)
  - [Q13. What is `stackalloc`, and how does it relate to performance-…](#04-var-dynamic-special-keywords-q13)
  - [Q14. What is `ref readonly` return, and how does it differ from r…](#04-var-dynamic-special-keywords-q14)
  - [Q15. How does `dynamic` interact with extension methods (why don'…](#04-var-dynamic-special-keywords-q15)

- [05. C# 7 Features](#05-c-7-features)
  - [Q1. What are tuple deconstruction and named tuple elements?](#05-c-7-features-q1)
  - [Q2. How do `out` variables declared inline in method calls work?](#05-c-7-features-q2)
  - [Q3. What are discards (`_`), and where are they used (deconstruc…](#05-c-7-features-q3)
  - [Q4. Explain pattern matching enhancements in C# 7 — `is` type pa…](#05-c-7-features-q4)
  - [Q5. What are `ref` returns and `ref` locals, and what safety rul…](#05-c-7-features-q5)
  - [Q6. What is `ref`/`in`/`out` in the context of `ReadOnlySpan`-er…](#05-c-7-features-q6)
  - [Q7. What are local functions, and how do they differ from lambda…](#05-c-7-features-q7)
  - [Q8. What are expression-bodied members beyond properties (method…](#05-c-7-features-q8)
  - [Q9. What binary literals and digit separators (`0b1010`, `1_000_…](#05-c-7-features-q9)
  - [Q10. What is `throw` as an expression inside ternary/null-coalesc…](#05-c-7-features-q10)
  - [Q11. How do generalized async return types work (`ValueTask` as a…](#05-c-7-features-q11)
  - [Q12. What are `default` in generic constraints improvements in C#…](#05-c-7-features-q12)

- [06. C# 8 Features](#06-c-8-features)
  - [Q1. Explain nullable reference types — how do they differ from `…](#06-c-8-features-q1)
  - [Q2. What do `?`, `!`, and `#nullable` directives mean at compile…](#06-c-8-features-q2)
  - [Q3. Are nullable reference annotations enforced at runtime?](#06-c-8-features-q3)
  - [Q4. What are default interface methods, and how do they relate t…](#06-c-8-features-q4)
  - [Q5. What are asynchronous streams (`IAsyncEnumerable<T>` and `aw…](#06-c-8-features-q5)
  - [Q6. Explain null-coalescing assignment (`??=`) with examples.](#06-c-8-features-q6)
  - [Q7. Explain range (`..`) and index (`^`) operators — how does `^…](#06-c-8-features-q7)
  - [Q8. What are `using` declarations vs `using` statements for IDis…](#06-c-8-features-q8)
  - [Q9. What are nullable-aware APIs in the BCL reacting to NRT (`No…](#06-c-8-features-q9)
  - [Q10. What is `IAsyncDisposable`, and how does `await using` work?](#06-c-8-features-q10)
  - [Q11. What are static local functions, and why were they added?](#06-c-8-features-q11)
  - [Q12. What is a `readonly struct`, and what mutability restriction…](#06-c-8-features-q12)
  - [Q13. What is the `readonly` modifier on struct instance members (…](#06-c-8-features-q13)
  - [Q14. What are stackalloc in safe contexts and `Span<T>` integrati…](#06-c-8-features-q14)
  - [Q15. What is target-typed `new()` vs explicit type names?](#06-c-8-features-q15)

- [Cross-chapter — Records & Pattern Matching *(C# 9–11; grouped here)*](#cross-chapter-records-pattern-matching-c-911-grouped-here)
  - [Q1. What are records (C# 9), and what boilerplate do they synthe…](#cross-chapter-records-pattern-matching-c-911-grouped-here-q1)
  - [Q2. What is the difference between `record class` and `record st…](#cross-chapter-records-pattern-matching-c-911-grouped-here-q2)
  - [Q3. How does value-based equality in records differ from default…](#cross-chapter-records-pattern-matching-c-911-grouped-here-q3)
  - [Q4. What is the difference between positional records and record…](#cross-chapter-records-pattern-matching-c-911-grouped-here-q4)
  - [Q5. Explain `with` expressions — how do they relate to non-destr…](#cross-chapter-records-pattern-matching-c-911-grouped-here-q5)
  - [Q6. What are init-only setters (`init`), and how do they differ …](#cross-chapter-records-pattern-matching-c-911-grouped-here-q6)
  - [Q7. Can init-only properties be set inside the type's constructo…](#cross-chapter-records-pattern-matching-c-911-grouped-here-q7)
  - [Q8. What is primary constructor syntax for records/classes (C# 1…](#cross-chapter-records-pattern-matching-c-911-grouped-here-q8)
  - [Q9. What is pattern matching in modern C# beyond C# 7 — switch e…](#cross-chapter-records-pattern-matching-c-911-grouped-here-q9)
  - [Q10. Explain property patterns (`person is { Age: > 18, Name: var…](#cross-chapter-records-pattern-matching-c-911-grouped-here-q10)
  - [Q11. What are relational patterns (`>`, `<=`) and combinator patt…](#cross-chapter-records-pattern-matching-c-911-grouped-here-q11)
  - [Q12. What is list patterns (C# 11) — `[_, .., var last]`?](#cross-chapter-records-pattern-matching-c-911-grouped-here-q12)
  - [Q13. What is `switch` expression vs traditional `switch` statemen…](#cross-chapter-records-pattern-matching-c-911-grouped-here-q13)
  - [Q14. What happens when a `switch` expression is not exhaustive ov…](#cross-chapter-records-pattern-matching-c-911-grouped-here-q14)
  - [Q15. What is the difference between `is null` and `== null` when …](#cross-chapter-records-pattern-matching-c-911-grouped-here-q15)
  - [Q16. What are expression trees (`Expression<T>`), and how do they…](#cross-chapter-records-pattern-matching-c-911-grouped-here-q16)
  - [Q17. How are expression trees used by LINQ providers (EF Core, `I…](#cross-chapter-records-pattern-matching-c-911-grouped-here-q17)
  - [Q18. Why can't all C# lambdas be converted to expression trees?](#cross-chapter-records-pattern-matching-c-911-grouped-here-q18)
  - [Q19. What is the difference between compile-time constant pattern…](#cross-chapter-records-pattern-matching-c-911-grouped-here-q19)
  - [Q20. When should you prefer records over classes for DTOs and dom…](#cross-chapter-records-pattern-matching-c-911-grouped-here-q20)
  - [Q21. **`typeof` vs `GetType()`** — `typeof(Base)` is known at com…](#cross-chapter-records-pattern-matching-c-911-grouped-here-q21)
  - [Q22. **Serialization type loss** — Assigning `Animal ref = new Do…](#cross-chapter-records-pattern-matching-c-911-grouped-here-q22)
  - [Q23. **`[Serializable]` ignored by System.Text.Json** — Candidate…](#cross-chapter-records-pattern-matching-c-911-grouped-here-q23)
  - [Q24. **`BinaryFormatter` is a security footgun** — Deserializing …](#cross-chapter-records-pattern-matching-c-911-grouped-here-q24)
  - [Q25. **Missing JSON property on non-nullable value type** — Deser…](#cross-chapter-records-pattern-matching-c-911-grouped-here-q25)
  - [Q26. **Enum numeric wire values** — Renumbering enum members brea…](#cross-chapter-records-pattern-matching-c-911-grouped-here-q26)
  - [Q27. **`JsonSerializerOptions` not thread-safe for mutation** — C…](#cross-chapter-records-pattern-matching-c-911-grouped-here-q27)
  - [Q28. **`dynamic` hides errors until runtime** — Misspelled member…](#cross-chapter-records-pattern-matching-c-911-grouped-here-q28)
  - [Q29. **Extension methods do not dispatch on `dynamic`** — Must ca…](#cross-chapter-records-pattern-matching-c-911-grouped-here-q29)
  - [Q30. **Reflection string names don't refactor** — Renaming a prop…](#cross-chapter-records-pattern-matching-c-911-grouped-here-q30)
  - [Q31. **Regex without timeout on user input** — Crafted input can …](#cross-chapter-records-pattern-matching-c-911-grouped-here-q31)
  - [Q32. **Nullable reference types are annotations only** — `#nullab…](#cross-chapter-records-pattern-matching-c-911-grouped-here-q32)
  - [Q33. **Records are still reference types (`record class`)** — Ide…](#cross-chapter-records-pattern-matching-c-911-grouped-here-q33)
  - [Q34. **Expression trees cannot contain statements arbitrarily** —…](#cross-chapter-records-pattern-matching-c-911-grouped-here-q34)

- [03. Regular Expressions](#03-regular-expressions)
  - [Q1. `Regex.IsMatch(email, pattern)` inline in the action](#03-regular-expressions-q1)
  - [Q2. `static readonly Regex` field with `RegexOptions.Compiled | …](#03-regular-expressions-q2)
  - [Q3. `[RegularExpression(@"…")]` on the DTO property](#03-regular-expressions-q3)
- [Scenario-Based Questions](#scenario-based-questions-karat-format)

---

### 01. Serialization & Deserialization

#### Q1. What is serialization and deserialization, and what is an object graph? {#01-serialization-deserialization-q1}

What is serialization and deserialization, and what is an object graph?

**Answer:** Serialization converts an in-memory object graph — the network of objects linked by references and collections — into a storable or transmittable wire format such as JSON, XML, or bytes. Deserialization reads that payload and constructs a new object graph with fresh instances populated from the data.

- The wire format is independent of process memory layout, enabling REST APIs, message queues, and file persistence.
- An object graph includes the root object plus nested objects, lists, and dictionaries reachable from it.
- Serializers decide which members cross the boundary; unmarked or ignored members stay local.
- Round trip means serialize then deserialize and compare meaningful data — not necessarily identical object identity.

---

#### Q2. Does deserialization resurrect original object identity or create new instances? {#01-serialization-deserialization-q2}

Does deserialization resurrect original object identity or create new instances?

**Answer:** Deserialization always creates new instances on the receiving side; it never resurrects the original heap objects or preserves reference identity from the sender's process. Any `ReferenceEquals` relationship from before serialization is lost unless the format explicitly preserves object references with metadata ids.

- Two references to the same object before serialization may become two separate equal objects after deserialization unless reference preservation is configured.
- Event handlers, database entity keys, and live connections cannot be faithfully restored from wire data alone.
- Identity-sensitive code must re-establish relationships after deserialization explicitly.
- See Q13 for `ReferenceHandler` options in System.Text.Json.

---

#### Q3. Compare JSON, XML, and binary as wire formats — trade-offs for APIs, config, and storage. {#01-serialization-deserialization-q3}

Compare JSON, XML, and binary as wire formats — trade-offs for APIs, config, and storage.

**Answer:** JSON is compact, human-readable, and dominant for HTTP APIs; XML is verbose but schema-rich and common in legacy enterprise integration; binary formats are smallest and fastest but opaque and tightly coupled to type layout unless you design a versioned protocol.

| Format | Readability | Schema | Typical use |
|---|---|---|---|
| JSON | High | Informal / JSON Schema | REST APIs, config, logs |
| XML | Medium | XSD, namespaces | SOAP, legacy config, documents |
| Binary | Low | Application-defined | Caches, game saves, high-throughput IPC |

JSON balances interoperability and tooling; XML when XSD validation or mixed content matters; binary when size and speed dominate and both endpoints share format rules.

---

#### Q4. Explain `System.Text.Json.JsonSerializer.Serialize` and `Deserialize` for files and streams. {#01-serialization-deserialization-q4}

Explain `System.Text.Json.JsonSerializer.Serialize` and `Deserialize` for files and streams.

**Answer:** `JsonSerializer.Serialize` converts an object to a JSON string or UTF-8 bytes, and `Deserialize<T>` parses JSON back into type `T`, with overloads accepting `Stream`, `ReadOnlySpan<byte>`, and async variants for network and file pipelines. They are the default JSON stack in modern ASP.NET Core.

- `JsonSerializer.Serialize(person)` → string; `SerializeToUtf8Bytes` avoids string allocation for HTTP bodies.
- `Deserialize<Person>(json)` throws `JsonException` on malformed JSON; validate inputs in API boundaries.
- `SerializeAsync` / `DeserializeAsync` stream to and from files without loading entire payloads as strings.
- Pass a shared `JsonSerializerOptions` instance for consistent naming and converters across calls.

---

#### Q5. What is `JsonSerializerOptions`, and which settings affect naming, indentation, and enum handling? {#01-serialization-deserialization-q5}

What is `JsonSerializerOptions`, and which settings affect naming, indentation, and enum handling?

**Answer:** `JsonSerializerOptions` centralizes serializer behavior: `PropertyNamingPolicy` (such as camelCase), `WriteIndented` for pretty printing, `DefaultIgnoreCondition` for omitting nulls, and `Converters` for custom type handling including enums as strings. One configured instance should be reused rather than recreated per call.

- `PropertyNamingPolicy = JsonNamingPolicy.CamelCase` maps `YearOfBirth` to `yearOfBirth` on the wire.
- `WriteIndented = true` aids debugging; disable in production responses to save bytes.
- Add `JsonStringEnumConverter` to `Converters` for string enum values globally.
- `PropertyNameCaseInsensitive = true` relaxes deserialization matching for incoming JSON keys.

---

#### Q6. Why is creating a new `JsonSerializerOptions` on every call a performance problem? {#01-serialization-deserialization-q6}

Why is creating a new `JsonSerializerOptions` on every call a performance problem?

**Answer:** Constructing `JsonSerializerOptions` rebuilds internal converter caches and reflection metadata each time, adding CPU and allocation overhead on hot paths such as per-request API serialization. The options object is designed to be created once and shared read-only across threads after configuration.

- ASP.NET Core registers options in dependency injection once at startup for this reason.
- Mutating a shared instance after sharing is unsafe — configure fully before first use.
- Source-generated contexts (`JsonSerializerContext`) reduce reflection cost further for known types.
- See Gotcha 7 — concurrent mutation of shared options causes race bugs.

---

#### Q7. When should you use `JsonSerializerContext` source generators vs reflection-based serialization? {#01-serialization-deserialization-q7}

When should you use `JsonSerializerContext` source generators vs reflection-based serialization?

**Answer:** Use source-generated `JsonSerializerContext` when you need faster startup, ahead-of-time (AOT) compatibility, or trimming-safe serialization without runtime reflection over your types. Reflection-based serialization is simpler for exploratory code and dynamically discovered types at the cost of metadata and trim warnings.

- Native AOT and trimmed apps require source generation for types you serialize — reflection may fail at runtime.
- `[JsonSerializable(typeof(Person))]` on a partial context class generates serializers at compile time.
- Reflection remains acceptable for admin tools and few-type internal utilities not published trimmed.
- Hybrid apps register source-generated contexts for hot types and fall back rarely for plugin types.

---

#### Q8. Explain `[JsonPropertyName]`, `[JsonIgnore]`, `[JsonInclude]`, and `[JsonPropertyOrder]`. {#01-serialization-deserialization-q8}

Explain `[JsonPropertyName]`, `[JsonIgnore]`, `[JsonInclude]`, and `[JsonPropertyOrder]`.

**Answer:** These attributes control wire mapping without renaming C# members: `JsonPropertyName` sets the JSON key, `JsonIgnore` omits a member, `JsonInclude` opts non-public members into serialization, and `JsonPropertyOrder` influences serialization order for readability or diff stability.

- `[JsonPropertyName("nickname")]` maps a C# `Nickname` property to `"nickname"` regardless of naming policy.
- `[JsonIgnore]` excludes secrets, computed properties, or circular navigation properties from payloads.
- `[JsonInclude]` on a private field includes it when building immutable types intentionally.
- Order attributes matter for human-readable diffs, not JSON semantic equality.

---

#### Q9. What happens when JSON contains properties not present on the C# type (extra members)? {#01-serialization-deserialization-q9}

What happens when JSON contains properties not present on the C# type (extra members)?

**Answer:** By default System.Text.Json ignores extra JSON properties that have no matching CLR member, so forward-compatible API clients can send new fields older servers skip without error. Strict modes or custom converters can change that behavior when unknown members must fail validation.

- Older services remain compatible when clients add optional metadata fields first.
- To reject unknown members, implement validation with `JsonNode` or a strict schema validator.
- Typos in expected property names deserialize as missing, not as extra — see missing-member gotchas for value types.
- Document versioning strategy: additive JSON fields are safer than renaming existing keys.

---

#### Q10. What happens when JSON is missing a property mapped to a non-nullable reference type vs a value type? {#01-serialization-deserialization-q10}

What happens when JSON is missing a property mapped to a non-nullable reference type vs a value type?

**Answer:** Missing reference-type properties deserialize to `null` even when annotated as non-nullable reference types (NRT), because JSON cannot distinguish absent from null without extra validation — analyzers warn but runtime does not enforce. Missing value-type properties deserialize to default values (`0`, `false`) silently, which can hide data errors.

- `string Name` absent in JSON → `null` at runtime; NRT compile warnings do not throw.
- `int Count` absent → `0` without error — dangerous for counters and enums unless validated.
- Use `required` properties (C# 11+), `[JsonRequired]`, or custom validation for critical fields.
- See Gotcha 5 for production impact on non-nullable value types.

---

#### Q11. How do enums serialize by default in `System.Text.Json`, and what production risk does numeric enum wire format create? {#01-serialization-deserialization-q11}

How do enums serialize by default in `System.Text.Json`, and what production risk does numeric enum wire format create?

**Answer:** By default enums serialize as their underlying numeric values (`0`, `1`, `2`), not as names, which breaks persisted JSON when enum members are reordered or renumbered in a later release. Clients and databases storing numeric values couple tightly to declaration order.

- Inserting a new enum member in the middle shifts subsequent numeric values and corrupts stored data.
- String enums decouple wire names from numeric backing values when names stay stable.
- Flags enums serialize as combined integers — document bitmask semantics for consumers.
- See Gotcha 6 — prefer string enums for long-lived contracts.

---

#### Q12. How do you serialize enums as strings using `JsonStringEnumConverter`? {#01-serialization-deserialization-q12}

How do you serialize enums as strings using `JsonStringEnumConverter`?

**Answer:** Add `JsonStringEnumConverter` to `JsonSerializerOptions.Converters` or apply `[JsonConverter(typeof(JsonStringEnumConverter))]` on the enum or property, so values appear as `"Active"` instead of `0`. Combine with `[JsonPropertyName]` on enum members when wire names differ from C# identifiers.

```csharp
var options = new JsonSerializerOptions();
options.Converters.Add(new JsonStringEnumConverter());
```

- String enums improve readability in logs and manual debugging of API payloads.
- Deserialization accepts names case-sensitively by default — configure case insensitivity if needed.
- Unknown enum strings throw unless you implement custom parsing fallback.

---

#### Q13. How do you handle circular references in an object graph (`ReferenceHandler.Preserve` / `IgnoreCycles`)? {#01-serialization-deserialization-q13}

How do you handle circular references in an object graph (`ReferenceHandler.Preserve` / `IgnoreCycles`)?

**Answer:** Object graphs with parent/child back-references cause infinite loops during naive serialization; `ReferenceHandler.IgnoreCycles` skips cyclic properties, and `ReferenceHandler.Preserve` emits `$id`/`$ref` metadata to reconstruct shared references on deserialize. Choose preserve when shared identity matters; ignore when trees are logically acyclic except for one back-link.

- `IgnoreCycles` drops repeated traversal — child.Parent may become null in JSON output.
- `Preserve` increases payload complexity but maintains graph shape for object webs.
- Redesign DTOs with id references instead of live object cycles for public APIs when possible.
- Entity Framework navigation properties often need cycle handling or `[JsonIgnore]` on back-references.

---

#### Q14. Why does serializing `Animal pet = new Dog()` sometimes drop `Dog`-only properties? {#01-serialization-deserialization-q14}

Why does serializing `Animal pet = new Dog()` sometimes drop `Dog`-only properties?

**Answer:** Statically typed serialization uses the compile-time type `Animal` unless polymorphic options include derived members, so properties declared only on `Dog` are omitted from JSON when the variable is typed as the base class. Runtime type alone does not expand the contract without configuration.

- `JsonSerializer.Serialize<Animal>(new Dog())` serializes only `Animal` members by default.
- Enable polymorphic type discriminators or serialize as `Dog` / use `object` with polymorphic options in modern System.Text.Json.
- See Gotcha 2 — this is a frequent API DTO bug with inheritance hierarchies.
- API models often flatten DTOs instead of relying on inheritance on the wire.

---

#### Q15. How do you enable polymorphic serialization in modern `System.Text.Json`? {#01-serialization-deserialization-q15}

How do you enable polymorphic serialization in modern `System.Text.Json`?

**Answer:** .NET 7+ supports polymorphic serialization via attributes such as `[JsonDerivedType(typeof(Dog), "dog")]` on base types and global polymorphism options, emitting a type discriminator alongside base properties so deserializers instantiate the correct derived type. Earlier versions required custom converters or separate DTO shapes.

- Discriminator property name and derived type mappings must be stable across API versions.
- Untrusted polymorphic deserialization is a security risk — validate allowed derived types strictly.
- Newtonsoft.Json historically used `$type` metadata — migrate carefully when switching serializers.
- Integration tests should round-trip every derived type in the hierarchy.

---

#### Q16. What is `JsonNode`, `JsonObject`, and `JsonArray`, and when prefer them over strongly typed models? {#01-serialization-deserialization-q16}

What is `JsonNode`, `JsonObject`, and `JsonArray`, and when prefer them over strongly typed models?

**Answer:** `JsonNode` and its subclasses `JsonObject` and `JsonArray` model JSON as a mutable DOM you can navigate and edit without declaring fixed C# classes, useful for partially structured payloads, ad-hoc API exploration, and schema-evolving documents. Prefer strongly typed models when shape is stable and validation belongs at deserialization time.

- `JsonObject` supports `node["key"]` get/set; `JsonArray` indexes elements.
- Ideal for merging settings files, patching unknown third-party JSON, or building responses dynamically.
- Strong types give compile-time checks and clearer domain models for core business entities.
- Combine: deserialize known portions to types and keep extras in `JsonExtensionData` dictionary properties.

---

#### Q17. How do you navigate and mutate JSON with `JsonNode` without deserializing to a fixed class? {#01-serialization-deserialization-q17}

How do you navigate and mutate JSON with `JsonNode` without deserializing to a fixed class?

**Answer:** Parse with `JsonNode.Parse(json)` or `JsonDocument`/`JsonNode` async overloads, then cast or use `AsObject()` / `AsArray()` to read and assign properties, add keys, and remove nodes imperatively. Changes reflect in the in-memory tree until you call `ToJsonString()` to emit updated text.

- `JsonNode? nick = root?["nickname"];` — null-conditional for missing keys.
- Mutate: `((JsonObject)root!)["count"] = 42;`
- Clone subtrees when branching immutable snapshots for undo flows.
- Validate types before cast — wrong node kinds throw `InvalidOperationException`.

---

#### Q18. What is a custom `JsonConverter<T>`, and when would you implement `Read`/`Write` manually? {#01-serialization-deserialization-q18}

What is a custom `JsonConverter<T>`, and when would you implement `Read`/`Write` manually?

**Answer:** `JsonConverter<T>` plugs custom serialization logic into System.Text.Json for types the default reflection serializer handles poorly — for example `DateOnly`, discriminated unions, or legacy string formats for numbers. Override `Read` and `Write` to translate between JSON tokens and your CLR type explicitly.

- Register on the property with `[JsonConverter(typeof(MyConverter))]` or add to `JsonSerializerOptions.Converters`.
- Use when you need invariant wire formats independent of culture or special rounding rules for decimals.
- Manual converters must handle null, property names in object scenarios, and forward-compatible token skipping.
- Prefer built-in converters and attributes when they suffice — custom code is maintenance overhead.

---

#### Q19. How do `Utf8JsonReader` and `Utf8JsonWriter` differ from `JsonSerializer` helpers? {#01-serialization-deserialization-q19}

How do `Utf8JsonReader` and `Utf8JsonWriter` differ from `JsonSerializer` helpers?

**Answer:** `Utf8JsonReader` and `Utf8JsonWriter` are low-level, forward-only UTF-8 JSON readers and writers that process tokens without building full object graphs, offering maximum control and performance for streaming pipelines. `JsonSerializer` builds on them internally for convenience deserialization to types.

- Reader loop: `while (reader.Read()) { switch (reader.TokenType) ... }`
- Writer: `WriteStartObject`, `WriteString`, `WriteEndObject` for incremental emission.
- Use low-level APIs for huge files, selective field extraction, or zero-allocation hot paths.
- Higher error handling burden — malformed JSON fails at token level with `JsonException`.

---

#### Q20. Explain `XmlSerializer` requirements (parameterless constructor, public read/write properties). {#01-serialization-deserialization-q20}

Explain `XmlSerializer` requirements (parameterless constructor, public read/write properties).

**Answer:** `XmlSerializer` can serialize public types with a public parameterless constructor and public read/write properties; it cannot serialize types lacking a default constructor or exposing only get-only properties unless you customize with attributes or `IXmlSerializable`. It generates XML from property names and collection shapes at runtime.

- Private fields and get-only auto-properties are ignored unless specially configured.
- Generic types and interfaces follow the same public property contract rules.
- Runtime failures occur when constructing the serializer if constraints are violated — compile succeeds.
- See Q22 for first-use code generation exceptions.

---

#### Q21. What do `[XmlRoot]`, `[XmlElement]`, `[XmlAttribute]`, and `[XmlIgnore]` control? {#01-serialization-deserialization-q21}

What do `[XmlRoot]`, `[XmlElement]`, `[XmlAttribute]`, and `[XmlIgnore]` control?

**Answer:** These attributes map CLR members to XML shape: `XmlRoot` names the document element, `XmlElement` sets element names for properties, `XmlAttribute` serializes a property as an attribute instead of child element, and `XmlIgnore` excludes members from XML output and input.

- `[XmlRoot("Person")]` on class changes outer tag from default type name.
- `[XmlAttribute("id")]` inlines simple values on the parent element.
- Collections serialize as repeated child elements unless `[XmlArray]` configures array layout.
- Order and namespace control use additional attributes (`Namespace`, `Order`) for schema compliance.

---

#### Q22. Why can `XmlSerializer` fail at runtime even when the project compiles? {#01-serialization-deserialization-q22}

Why can `XmlSerializer` fail at runtime even when the project compiles?

**Answer:** `XmlSerializer` validates type shape and emits serialization assemblies on first use; violations such as missing parameterless constructors, interfaces as root types, or unsupported collection patterns throw `InvalidOperationException` at runtime when you first construct the serializer or serialize. The C# compiler does not analyze XML serializer constraints.

- First call may trigger csc.exe dynamic assembly generation — failures surface in production if untested.
- Circular references and `Dictionary<K,V>` historically required workarounds or custom serialization.
- Test `new XmlSerializer(typeof(MyType))` at startup or in integration tests to fail fast.
- `XmlSerializer` cannot serialize `IDictionary` implementations without custom patterns in many cases.

---

#### Q23. What is `[Serializable]` actually used for in modern .NET? {#01-serialization-deserialization-q23}

What is `[Serializable]` actually used for in modern .NET?

**Answer:** The `[Serializable]` attribute marked types as eligible for legacy binary formatters such as `BinaryFormatter`; modern `System.Text.Json` and `XmlSerializer` ignore it for their default code paths. It remains on some Framework-era types but is not the switch for JSON API serialization today.

- Do not add `[Serializable]` expecting JSON behavior — configure JSON attributes instead.
- See Gotcha 3 — conflating legacy binary markers with modern serializers is a common interview mistake.
- Remoting and old ASP.NET session state modes used binary serialization — largely historical.
- Prefer explicit DTO contracts for new persistence and API layers.

---

#### Q24. Explain `BinaryFormatter` — why is it obsolete, and what security risks led to its removal? {#01-serialization-deserialization-q24}

Explain `BinaryFormatter` — why is it obsolete, and what security risks led to its removal?

**Answer:** `BinaryFormatter` deserialized arbitrary type graphs from untrusted bytes and could be tricked into executing dangerous gadget chains, enabling remote code execution — a class of deserialization attacks. It is obsolete and disabled by default on modern .NET; Microsoft removed it as a safe default because type-name embedded payloads are inherently unsafe against malicious input.

- Never deserialize untrusted binary payloads with type-aware formatters.
- CVE history around `BinaryFormatter` drove stricter defaults and removal timelines.
- See Gotcha 4 — treating binary deserialize as harmless persistence is dangerous.
- Alternatives use explicit schemas: JSON, Protocol Buffers, MessagePack with known types.

---

#### Q25. What are recommended modern alternatives to `BinaryFormatter` for trusted internal persistence? {#01-serialization-deserialization-q25}

What are recommended modern alternatives to `BinaryFormatter` for trusted internal persistence?

**Answer:** For trusted internal scenarios, use explicit formats: System.Text.Json or MessagePack for structured objects, `MemoryPack` or Protocol Buffers for performance-sensitive caches, or custom versioned binary layouts with `BinaryWriter` where you control every byte. All alternatives should whitelist types rather than embed arbitrary type names.

- JSON with source generation balances readability and AOT for app settings caches.
- MessagePack and protobuf require `.proto` or attributed models — no arbitrary type graphs.
- Encrypt and authenticate persisted blobs at rest even when trusted — defense in depth.
- Version headers on custom binary files enable migration without formatter magic.

---

#### Q26. How does `DateTime` with unspecified `Kind` behave across time zones during serialization? {#01-serialization-deserialization-q26}

How does `DateTime` with unspecified `Kind` behave across time zones during serialization?

**Answer:** `DateTime` with `DateTimeKind.Unspecified` carries no time-zone offset on the wire in ISO 8601 strings without `Z` or offset, so deserializing machines may interpret the instant differently depending on serializer options and local assumptions. Unspecified values are ambiguous for global systems.

- `DateTimeKind.Utc` serializes with `Z` suffix when using round-trip formats — preferred for server timestamps.
- `Local` kind ties meaning to the writer machine's time zone — poor for APIs.
- System.Text.Json defaults favor ISO 8601 strings; options control how `Unspecified` is written.
- Document whether API consumers should treat absent offset as UTC or local.

---

#### Q27. Why is `DateTimeOffset` often safer on the wire than `DateTime`? {#01-serialization-deserialization-q27}

Why is `DateTimeOffset` often safer on the wire than `DateTime`?

**Answer:** `DateTimeOffset` always includes an offset from UTC alongside the clock time, preserving the intended instant across time zones without relying on unstated `Kind` semantics. It eliminates much ambiguity when clients span regions and daylight-saving transitions.

- Wire form includes `+05:30` or `Z` explicitly — consumers know the instant.
- Database storage still requires consistent UTC policy — `DateTimeOffset` maps cleanly to UTC for storage.
- Use for API contracts; convert to `DateTime` only at UI boundaries when local display requires it.
- Pair with invariant ISO formatting in serializers for stable string comparisons.

---

#### Q28. Can a type with only get-only properties serialize but fail to deserialize? Why? {#01-serialization-deserialization-q28}

Can a type with only get-only properties serialize but fail to deserialize? Why?

**Answer:** Yes — System.Text.Json can serialize get-only properties by reading them, but deserialization requires writable members, constructor parameters matched by `[JsonConstructor]`, or `[JsonInclude]` on private setters unless using parameterized constructors configured for JSON. Immutable types need explicit constructor binding.

- Records with positional syntax generate constructor mapping; manual get-only classes do not deserialize by default.
- `[JsonInclude]` on private fields supports immutable object patterns intentionally designed for JSON.
- Serialize-only DTO projections are fine for reports; round-trip DTOs need write paths.
- Test deserialize in CI, not just serialize, for every API model.

---

#### Q29. How do `[JsonConstructor]` and parameterized constructors interact with deserialization? {#01-serialization-deserialization-q29}

How do `[JsonConstructor]` and parameterized constructors interact with deserialization?

**Answer:** Mark one constructor with `[JsonConstructor]` so the deserializer invokes it and binds JSON properties to parameters by name (case-insensitive by default), enabling immutable types without public setters. Parameters must align with JSON property names or `[JsonPropertyName]` mappings.

- Without `[JsonConstructor]`, multiple constructors cause ambiguity or fallback to parameterless ctor plus setters.
- Parameter names in source may require `[JsonConstructor]` and C# 9+ parameter name metadata for binding.
- Required parameters missing in JSON throw `JsonException` when options demand non-null values.
- Preferred pattern for domain models that reject invalid states at construction time.

---

#### Q30. What is the difference between `System.Text.Json` and `Newtonsoft.Json` feature sets (contract customization, references)? {#01-serialization-deserialization-q30}

What is the difference between `System.Text.Json` and `Newtonsoft.Json` feature sets (contract customization, references)?

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

#### Q1. What is reflection in C#, and what problems does it solve? {#02-reflection-attributes-q1}

(R) A warehouse API loads pricing plugins from a separate assembly at runtime. It works on a developer machine but `LoadPlugin` always returns null in staging. Review the loader:

```csharp
public sealed class PluginLoader
{
    public object? LoadPlugin(string typeName)
    {
        Type? pluginType = Type.GetType(typeName); // e.g. "Acme.Pricing.VolumeDiscountPlugin"
        if (pluginType is null)
        {
            return null;
        }

        return Activator.CreateInstance(pluginType);
    }
}

// Startup config (appsettings):
// "Plugins:PricingType": "Acme.Pricing.VolumeDiscountPlugin"
```

What fails cross-assembly, and how do you fix type resolution for production plugin loading?

**Answer:** `Type.GetType(string)` resolves types in the **calling assembly** and mscorlib by default — not arbitrary referenced or dynamically loaded assemblies. A namespace-qualified name without an assembly qualifier returns null when the plugin lives in `Acme.Pricing.dll`, which looks like "works locally" only if everything is inlined in one project.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | `Type.GetType` without assembly-qualified name | Returns null for types in other assemblies — plugin silently skipped |
| Design | No assembly load step before type lookup | Plugin DLL never loaded into the AppDomain |
| Correctness | `Activator.CreateInstance` on resolved type may still fail | Parameterless ctor required; wrong ctor throws `MissingMethodException` |

**Fix (priority order):**

1. Load the plugin assembly explicitly: `Assembly.LoadFrom(path)` or `AssemblyLoadContext.LoadFromAssemblyPath`, then `assembly.GetType(typeName)`.
2. Or pass an assembly-qualified name in config: `"Acme.Pricing.VolumeDiscountPlugin, Acme.Pricing, Version=…, Culture=neutral, PublicKeyToken=…"`.
3. Fail fast when resolution returns null — log config value and searched assemblies instead of returning null quietly.
4. Validate plugin types implement a known interface (`IPricingPlugin`) before `CreateInstance`; guard null and ctor requirements.

**Production takeaway:** Cross-assembly `Type.GetType` is a classic "works in monolith, fails when split" trap — same lesson as **Program.cs** Section 6b: assembly-qualified names are required for external types.

---

#### Q2. What is the difference between early binding and late binding? {#02-reflection-attributes-q2}

(R) A metadata-driven audit interceptor invokes controller actions and logs failures, but operators only see `TargetInvocationException` in Splunk — never the real fault. Review the handler:

```csharp
public sealed class AuditInterceptor
{
    public object? InvokeWithAudit(object target, MethodInfo method, object?[] args)
    {
        AuditActionAttribute? audit = method.GetCustomAttribute<AuditActionAttribute>();
        _logger.LogInformation("Before {Action}", audit?.Action ?? method.Name);

        try
        {
            return method.Invoke(target, args);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Audit invoke failed for {Method}", method.Name);
            throw;
        }
    }
}
```

What is wrong with exception handling here, and what should be logged and rethrown?

**Answer:** Exceptions thrown **inside** the invoked method are wrapped in `TargetInvocationException`. Logging and rethrowing the wrapper hides the real fault (`InvalidOperationException`, `ArgumentException`, etc.) from operators and any upstream handler that keys off exception type.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | Logs outer `TargetInvocationException` only | Splunk shows wrapper message/stack — root cause buried in `.InnerException` |
| Observability | `throw;` rethrows wrapper unchanged | Global exception middleware may classify wrong HTTP status |
| Maintainability | No unwrap before log/rethrow | On-call cannot triage from alert text alone |

**Fix (priority order):**

1. Catch `TargetInvocationException` specifically and unwrap: `var inner = tie.InnerException ?? tie;`
2. Log `inner` (message + stack), not the wrapper — include `method.Name` and audit action metadata.
3. Rethrow `inner` with `ExceptionDispatchInfo.Capture(inner).Throw()` if you must preserve original stack without `throw ex` corruption.
4. Guard `method` for null before `Invoke` — `GetMethod` typos cause `NullReferenceException` before any wrapper is involved.

```csharp
catch (TargetInvocationException tie)
{
    Exception actual = tie.InnerException ?? tie;
    _logger.LogError(actual, "Audit invoke failed for {Method}", method.Name);
    ExceptionDispatchInfo.Capture(actual).Throw();
}
```

**Production takeaway:** `MethodInfo.Invoke` always wraps callee faults — production code must unwrap before logging and API error mapping. See **Program.cs** Section 6e and QUICK REFERENCE — TargetInvocationException.

---

#### Q3. Explain `typeof(T)` vs `obj.GetType()` — compile-time token vs runtime type. {#02-reflection-attributes-q3}

(R) After a rename refactor from `CalculateLineTotal` to `CalculateOrderTotal`, order totals silently become zero in production. Review the pricing service:

```csharp
public decimal GetLineTotal(Product product, int quantity)
{
    Type type = product.GetType();
    MethodInfo? method = type.GetMethod("CalculateLineTotal"); // string name — not nameof

    object? result = method.Invoke(product, new object[] { quantity });
    return result is decimal total ? total : 0m;
}
```

Identify the stacked problems (compile-time, runtime, maintainability) and prioritize fixes.

**Answer:** The method was renamed but the string literal was not updated — `GetMethod` returns null, and the code invokes without a null check, which throws at runtime (or would if the fallback `0m` masked a null invoke in a sloppier variant). Stringly-typed reflection bypasses the compiler's rename refactor.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Maintainability | Hard-coded `"CalculateLineTotal"` | Rename refactor does not update call sites — silent wrong behavior or NRE |
| Runtime | No null guard on `method` before `Invoke` | `NullReferenceException` in production when name drifts |
| API contract | `GetMethod(name)` without parameter types | Ambiguous if overloads exist — wrong overload or null |
| Design | Reflection for a known compile-time type | Unnecessary indirection vs direct `product.CalculateOrderTotal(quantity)` |

**Fix (priority order):**

1. Replace string with `nameof(Product.CalculateOrderTotal)` and pass parameter types: `GetMethod(nameof(...), new[] { typeof(int) })`.
2. Guard null and fail loudly (throw `InvalidOperationException`) instead of returning `0m` — silent zero corrupts billing.
3. Prefer direct call when type is known at compile time; reserve reflection for plugin/unknown-type scenarios.
4. Add a unit test that asserts invoke succeeds after renames — or eliminate reflection on this path entirely.

**Production takeaway:** Reflection + magic strings defeats IDE refactor — Karat stacks rename drift with missing null guards. See **Program.cs** Section 6e pitfall table and Section 6i — "Rename refactor breaks silently."

---

#### Q4. Why does `typeof(List<int>) == typeof(List<string>)` return false? {#02-reflection-attributes-q4}

(R) A margin-report job reads private cost fields from `CostRecord`-like DTOs but always gets null and skips rows. Review the extractor:

```csharp
public decimal? ReadCostBasis(object record)
{
    Type type = record.GetType();
    FieldInfo? field = type.GetField("_costBasis"); // default binding — public only

    if (field is null)
    {
        return null;
    }

    return (decimal?)field.GetValue(record);
}
```

What binding mistake causes this, and what flags are required?

**Answer:** `GetField` without `BindingFlags` uses default binding, which returns **public** instance/static members only. `_costBasis` is a private instance field — lookup returns null, the method exits early, and rows are skipped.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | Default binding omits non-public members | Private field never found — report data missing |
| Correctness | `(decimal?)field.GetValue(record)` on boxed value | Unboxing via nullable cast can throw if type mismatches |
| Design | Reading private fields from outside the type | Breaks encapsulation — prefer a public/internal accessor for reporting |

**Fix (priority order):**

1. Pass explicit flags matching **Program.cs** Section 6d: `BindingFlags.Instance | BindingFlags.NonPublic` (add `Public` if you want either).
2. Guard null and log type name + field name when lookup fails — distinguish "wrong type" from "wrong flags."
3. Prefer exposing `CostBasis` via an internal property or `IOffsetCostReadable` interface for the report job instead of reaching into private fields.
4. Cache `FieldInfo` per `Type` in a static readonly dictionary if this runs in a batch loop.

```csharp
const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
FieldInfo? field = type.GetField("_costBasis", flags);
```

**Production takeaway:** Default reflection binding is the first thing to check when "member not found" — same specimen as `CostRecord` in **Program.cs** Section 3 and 6d.

---

#### Q5. How do you obtain the generic type definition from a closed generic type? {#02-reflection-attributes-q5}

(P) An ASP.NET Core API discovers `[EntityTable]`-decorated export types by scanning `Assembly.GetExecutingAssembly().GetTypes()` at startup. After enabling `<PublishTrimmed>true</PublishTrimmed>` for a Native AOT experiment, several entity types vanish from the export manifest with no compile errors. Why does trimming break this pattern, and what production-safe alternatives exist?

**Answer:** The trimmer removes types and members it cannot prove are used at compile time. Startup reflection that scans assemblies and reads custom attributes is invisible to static analysis — entity types with no direct references are linked out, so `GetTypes()` returns a smaller set and attribute-driven discovery silently drops models.

- **Why no compile error:** Trimming is a link-time optimization; reflection targets are not required call sites the compiler tracks.
- **Mitigations:** Annotate roots with `[DynamicallyAccessedMembers]` / `DynamicallyAccessedMemberTypes` on APIs that accept `Type`; use a trimmer descriptor file (`TrimmerRootAssembly` / `TrimmerRootDescriptor`) to preserve entity assemblies; register known export types explicitly in DI instead of full-assembly scan.
- **Long-term:** Replace scan-all-reflection with **source generators** that emit export manifests or EF-style mappings at compile time — same metadata (`[EntityTable]`, `[Exportable]`), no runtime graph walk.
- **Handle `ReflectionTypeLoadException`:** When dependencies are trimmed or missing, `GetTypes()` can throw — catch and log `LoaderExceptions` (see **Program.cs** Section 6b).

**Production takeaway:** Attribute-driven discovery copied from EF/ASP.NET patterns fails under Native AOT and aggressive publish trimming unless you root types or generate code — preview in **Program.cs** Section 6i AOT note.

---

#### Q6. What is the `Type` class, and what members expose metadata (methods, properties, fields, attributes)? {#02-reflection-attributes-q6}

(D) Your team ships a CSV export endpoint. One developer scans every request with `type.GetProperties()` and `GetCustomAttribute<ExportableAttribute>()` (same pattern as `ExportManifestBuilder` in this chapter). Another caches `PropertyInfo[]` and attribute metadata in a `ConcurrentDictionary<Type, ExportColumn[]>` built once at startup. Under 500 RPS with 40 exportable properties per row, which approach do you choose and why?

**Answer:** Cache metadata at startup (or first use per `Type`) and only call `GetValue` per instance per request — reflection on `Type` and `MemberInfo` is orders of magnitude more expensive than reading pre-resolved columns from a cached `ExportColumn[]`.

- **Per-request scan:** 500 × 40 property walks × attribute lookups allocates and hits internal reflection caches repeatedly — CPU spikes, GC pressure, latency tail grows under load.
- **Cached manifest:** Startup (or lazy) build mirrors **ExportManifestBuilder** logic once per exportable type; hot path is `foreach (col in manifest) col.Getter(instance)` — optionally compile delegates with `CreateGetter` for value types to avoid boxing.
- **Thread safety:** `ConcurrentDictionary<Type, ExportColumn[]>` is safe for lazy initialization; manifest is immutable after build — no lock on read path.
- **Invalidation:** If types are loaded dynamically (plugins), register manifest on plugin load; static domain models rarely need refresh.
- **Trade-off:** Cached approach uses slightly more memory for delegate/metadata tables — acceptable vs per-request CPU at 500 RPS.

**Production takeaway:** Metadata-driven export is a framework pattern — scan once, read many — not scan per row. See **Program.cs** Section 5 and Section 6i — "Uncached reflection in hot paths."

---

#### Q7. What is `BindingFlags`, and how do `Instance`, `Static`, `Public`, `NonPublic`, and `DeclaredOnly` combine? {#02-reflection-attributes-q7}

(M) A `PremiumProduct : Product` subclass is added for a loyalty tier. The ORM layer reads `[EntityTable]` from the base `Product` type to resolve table names. Export works for `Product` but `PremiumProduct` rows fail with "table not mapped." Given this attribute definition from the tutorial:

```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class EntityTableAttribute : Attribute { /* TableName */ }
```

Why does inheritance behave this way, and what are two correct fixes at the call site or attribute definition?

**Answer:** `Inherited = false` on `EntityTableAttribute` means the attribute is stored only on `Product` — it is **not** visible when you call `typeof(PremiumProduct).GetCustomAttribute<EntityTableAttribute>()`. The ORM resolves the runtime type of each instance, sees no attribute on the subclass, and reports "table not mapped."

- **Why designed this way:** Table-per-type hierarchies often need different tables per concrete class — inheriting `[EntityTable("Products")]` onto every subclass would be wrong when `PremiumProduct` maps to `PremiumProducts`.
- **Fix 1 (call site):** Walk the inheritance chain: check `type.GetCustomAttribute<EntityTableAttribute>(inherit: true)` is insufficient when `Inherited = false`; instead loop `type = type.BaseType` until you find the attribute, or map `PremiumProduct` explicitly in a type registry.
- **Fix 2 (attribute):** If all subclasses share one table, set `Inherited = true` on `EntityTableAttribute` and apply only on the base — then `GetCustomAttribute` on derived types returns the base metadata (verify this matches your schema).
- **Fix 3 (explicit):** Add `[EntityTable("PremiumProducts")]` on `PremiumProduct` — correct when the subclass has its own table regardless of inheritance flags.

**Production takeaway:** Attribute inheritance is opt-in via `AttributeUsage.Inherited` — framework code must not assume subclass metadata mirrors the base. Contrast with `[DisplayLabel]` in **Program.cs** Section 1 (`Inherited = true`) vs `[EntityTable]` (`Inherited = false`).

---

#### Q8. How do you enumerate properties, methods, fields, and constructors with reflection? {#02-reflection-attributes-q8}

_Answer not found._

---

#### Q9. How do you invoke a method dynamically via `MethodInfo.Invoke`? {#02-reflection-attributes-q9}

_Answer not found._

---

#### Q10. What is `Activator.CreateInstance`, and how do you pass constructor arguments? {#02-reflection-attributes-q10}

_Answer not found._

---

#### Q11. How do you create generic types at runtime (`MakeGenericType`) and invoke generic methods (`MakeGenericMethod`)? {#02-reflection-attributes-q11}

_Answer not found._

---

#### Q12. How do you get and set property and field values through `PropertyInfo` / `FieldInfo`? {#02-reflection-attributes-q12}

_Answer not found._

---

#### Q13. How can reflection access private members, and why is that a maintenance and security concern? {#02-reflection-attributes-q13}

_Answer not found._

---

#### Q14. Explain `Assembly`, `Module`, `MemberInfo`, `MethodInfo`, `PropertyInfo`, and `FieldInfo` relationships. {#02-reflection-attributes-q14}

_Answer not found._

---

#### Q15. What is the difference between `Assembly.Load`, `Assembly.LoadFrom`, and `AssemblyLoadContext`? {#02-reflection-attributes-q15}

_Answer not found._

---

#### Q16. Can you unload an assembly in .NET Framework vs .NET Core / .NET 5+? {#02-reflection-attributes-q16}

_Answer not found._

---

#### Q17. How do you discover and read custom attributes at runtime (`GetCustomAttribute`, `IsDefined`)? {#02-reflection-attributes-q17}

_Answer not found._

---

#### Q18. How do you define and apply your own attribute classes (`AttributeUsage`)? {#02-reflection-attributes-q18}

_Answer not found._

---

#### Q19. What are performance costs of reflection vs compiled code, and how do trimming/AOT affect it? {#02-reflection-attributes-q19}

_Answer not found._

---

#### Q20. What is `Reflection.Emit`, and when is dynamic IL generation justified? {#02-reflection-attributes-q20}

_Answer not found._

---

#### Q21. How does reflection interact with nullable reference type annotations? {#02-reflection-attributes-q21}

_Answer not found._

---

#### Q22. What security permissions historically gated reflection, and what changed in modern .NET? {#02-reflection-attributes-q22}

_Answer not found._

---

### 03. Regular Expressions

#### Q1. What is the purpose of the `Regex` class in C#? {#03-regular-expressions-q1}

(R) A bulk-import API validates thousands of customer rows per request. After deploy, CPU spikes and some requests time out. Review this validator and prioritize fixes.

**Answer:** The validator re-parses regex patterns on every row via static helpers, accepts substring email matches because the pattern lacks anchors, and runs a nested-quantifier notes pattern with no `MatchTimeout` — together causing wasted CPU, false accepts, and potential ReDoS under adversarial notes text.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance | `Regex.IsMatch` / `Regex.Match` static calls inside per-row loop | Pattern re-parsed on every invocation — O(rows × parse cost) |
| Correctness | Email pattern missing `^` and `$` | `"junk alice@x.co more"` passes validation (substring match) |
| Runtime / security | `(order\s+\d+)+` nested quantifier with no timeout | Catastrophic backtracking on long notes → hung threads, CPU spikes |
| Design | Notes extraction mixed into boolean gate | Validation path does extra work even when email/phone already fail |

**Fix (priority order):**

1. Cache patterns in `static readonly Regex` fields (with `RegexOptions.Compiled | RegexOptions.CultureInvariant`) and call instance `.IsMatch` / `.Match`.
2. Anchor whole-field validation: `@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"` — matches **Program.cs** `EmailPattern`.
3. Add `TimeSpan` match timeout on the notes pattern (or use `RegexOptions.NonBacktracking` in .NET 7+ for untrusted text).
4. Rewrite the notes pattern without nested greedy quantifiers — e.g. `@"order\s+\d+"` with `Matches` when you need every hit.
5. Short-circuit: validate email and phone before scanning notes.

```csharp
private static readonly Regex EmailPattern = new(
    @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
    RegexOptions.Compiled | RegexOptions.CultureInvariant,
    TimeSpan.FromMilliseconds(250));

private static readonly Regex OrderInNotes = new(
    @"order\s+\d+",
    RegexOptions.IgnoreCase | RegexOptions.Compiled,
    TimeSpan.FromMilliseconds(250));
```

**Production takeaway:** Bulk import turns "fine in dev" regex into a hot path — cache instances, anchor fields, and timeout untrusted text. See **Program.cs** Sections 3, 11, and 12.

---

#### Q2. Explain `Regex.IsMatch`, `Match`, `Matches`, `Replace`, and `Split`. {#03-regular-expressions-q2}

(R) A support portal lets agents paste a custom regex to search and redact matches in uploaded log files (multi-MB). Review this endpoint helper:

```csharp
public string RedactMatches(string logContent, string userPattern)
{
    var regex = new Regex(
        userPattern,
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    return regex.Replace(logContent, "[REDACTED]");
}
```

What production risks exist, and how would you harden this for untrusted input?

**Answer:** Accepting arbitrary regex from users against large inputs is a classic ReDoS vector — nested quantifiers can hang a thread indefinitely, and `Compiled` on every unique user pattern adds startup cost without helping one-off searches.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | User-supplied pattern with no timeout or engine guard | ReDoS — request thread blocked, CPU pegged, gateway timeouts |
| Security | No pattern length or complexity limits | Trivial denial-of-service via `(a+)+$`-style patterns on long logs |
| Performance | `RegexOptions.Compiled` per unique user pattern | IL emission cost on every distinct pattern; memory growth if patterns vary |
| Correctness | No validation that pattern is well-formed before `Replace` | `ArgumentException` bubbles as 500; partial redaction on invalid `$` tokens |
| Design | Regex redaction on multi-MB strings in-request | Large LOH allocations; latency spikes under concurrent uploads |

**Fix (priority order):**

1. Reject or sandbox user patterns — max length, allow-list of constructs, or disallow user regex entirely (fixed internal patterns + literal search via `Regex.Escape`).
2. Always pass `TimeSpan` match timeout: `new Regex(userPattern, options, TimeSpan.FromSeconds(2))` — catch `RegexMatchTimeoutException` and return 400.
3. Prefer `RegexOptions.NonBacktracking` (.NET 7+) for untrusted patterns — linear-time engine trades some feature support for safety.
4. Drop `Compiled` for ad hoc user patterns; cache only frequently reused admin-defined patterns in a bounded dictionary.
5. Process large logs in chunks or offload to a background worker with cancellation.

```csharp
var safe = new Regex(
    userPattern,
    RegexOptions.IgnoreCase | RegexOptions.NonBacktracking,
    TimeSpan.FromSeconds(2));
```

**Production takeaway:** Never run untrusted regex on untrusted input without timeout or NonBacktracking — Karat uses this to test ReDoS awareness, not pattern syntax recall. See **Program.cs** Sections 10–11 and `DemonstratePerformance` timeout demo.

---

#### Q3. What is the difference between verbatim regex strings (`@"\d+"`) and escaped regular strings? {#03-regular-expressions-q3}

(R) A notes-processing job extracts phone fragments for a CRM sync. Review this extractor:

```csharp
public sealed class PhoneExtractor
{
    private static readonly Regex PhonePattern = new(
        @"(\+1[-.\s]?)?(\([0-9]{3}\)|[0-9]{3})[-.\s]?[0-9]{3}[-.\s]?[0-9]{4}",
        RegexOptions.Compiled);

    public string GetAreaCode(string notes)
    {
        Match m = PhonePattern.Match(notes);
        return m.Groups[2].Value; // area-code capture
    }

    public bool HasUsPhone(string notes) =>
        Regex.IsMatch(notes, PhonePattern.ToString());
}
```

What correctness bugs appear on edge-case inputs, and how do you fix them?

**Answer:** `GetAreaCode` reads `Groups[2]` without checking `Success`, returning empty strings silently when no phone exists; `HasUsPhone` round-trips the pattern through `.ToString()` into a static call that re-parses every time and still performs partial matching anywhere in the notes string.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | No `m.Success` check before `Groups[2]` | `"no phone here"` → `""` instead of explicit "not found" — CRM gets blank area codes |
| Correctness | Phone pattern not anchored with `^…$` | Matches embedded digit runs inside longer strings — false positives |
| Performance | `Regex.IsMatch(notes, PhonePattern.ToString())` | Re-parses pattern on every call; `.ToString()` is not a reliable reuse mechanism |
| Design | Group index `[2]` is magic number | Refactoring pattern breaks silently when optional country group shifts |

**Fix (priority order):**

1. Guard on `Success` before reading groups; return `null`, `Optional`, or throw a domain exception when no match.
2. Anchor for whole-field checks: prepend `^` and append `$` when validating a dedicated phone column; use unanchored instance only for extraction inside free text (document the difference).
3. Reuse the cached instance: `PhonePattern.IsMatch(notes)` instead of static + `ToString()`.
4. Prefer named groups `(?<area>…)` and read `Groups["area"].Value` for maintainability — matches **Program.cs** Section 6.

```csharp
public string? GetAreaCode(string notes)
{
    Match m = PhonePattern.Match(notes);
    if (!m.Success)
        return null;
    return m.Groups["area"].Success ? m.Groups["area"].Value : null;
}

public bool HasUsPhone(string notes) => PhonePattern.IsMatch(notes);
```

**Production takeaway:** `Groups[n]` without `Success` is a silent data bug — Karat stacks it with static-helper misuse. See **Program.cs** Section 12 pitfalls and Section 6 named groups.

---

#### Q4. What are common metacharacters candidates should know (`.`, `*`, `+`, `?`, `^`, `$`, `\d`, `\w`, groups)? {#03-regular-expressions-q4}

(P) Your registration API validates email on every POST (~2k RPS). A teammate proposes three options:

1. `Regex.IsMatch(email, pattern)` inline in the action  
2. `static readonly Regex` field with `RegexOptions.Compiled | RegexOptions.CultureInvariant`  
3. `[RegularExpression(@"…")]` on the DTO property  

When would you choose each, and what companion settings (timeout, anchoring, caching) are mandatory for the Regex-based approaches in production?

**Answer:** At 2k RPS, inline static calls re-parse the pattern on every request and should be replaced with a cached compiled instance; the data-annotation attribute is fine for coarse API validation but still needs a well-anchored pattern and does not replace DNS or mailbox verification.

- **Option 1 — inline static:** Acceptable only for cold paths (admin tools, one-off scripts). On a hot registration endpoint it wastes CPU re-parsing the same automaton per call — replace with Option 2.
- **Option 2 — static readonly + Compiled:** Production default for hot regex validation. Pair with `^…$` anchors, `CultureInvariant`, and a constructor `TimeSpan` timeout (e.g. 200–500 ms) even for fixed patterns — defense in depth if the pattern is ever edited badly.
- **Option 3 — `[RegularExpression]`:** Good for declarative model validation in ASP.NET Core (`[ApiController]` runs it automatically). Same anchored pattern required; attribute does not add caching or timeout by itself — underlying implementation still constructs/runs regex per validation unless you also use a custom `ValidationAttribute` wrapping a shared instance.
- **Mandatory companions for any Regex approach:** whole-field anchors; treat regex as syntax-only (follow with uniqueness check, domain policy, or confirmation email); log `RegexMatchTimeoutException` as a potential attack signal.
- **Modern alternative (.NET 7+):** `[GeneratedRegex(@"^…$")]` partial method — compile-time generated, zero runtime parse, ideal for fixed hot patterns (previewed in **Program.cs** Section 11).

**Production takeaway:** Karat tests whether you distinguish "works in a demo" from "safe at 2k RPS" — caching, anchoring, and timeout matter as much as the pattern itself.

---

#### Q5. What is catastrophic backtracking, and what pattern shapes trigger it? {#03-regular-expressions-q5}

(D) Product wants import rejection for disposable email domains (`mailinator.com`, `tempmail.org`, …) and a regex that only allows corporate TLDs. A developer merges the blocklist into one giant pattern:

```csharp
bool ok = Regex.IsMatch(email,
    @"^(?!.*@(mailinator|tempmail)\.com$)[a-zA-Z0-9._%+-]+@(?:contoso|fabrikam)\.(?:com|org)$");
```

What breaks in maintainability, testability, and correctness compared to splitting validation layers? How would you structure this in a real import pipeline?

**Answer:** One mega-pattern couples RFC-ish syntax, a disposable-domain policy, and an allow-list of employers into an unreadable string that is painful to unit test, unsafe to extend (every blocklist change recompiles regex), and still cannot verify that the mailbox exists or that the domain is typosquatted.

- **Maintainability:** Blocklists belong in configuration (`IOptions<EmailPolicyOptions>`) or a database table — not inside a pattern literal. Adding `tempmail.org` should be a config deploy, not a regex edit requiring code review of lookaheads.
- **Testability:** Layered validators get focused tests: syntax regex returns pass/fail; domain service checks blocklist and allow-list independently; integration tests compose them. A single regex forces table-driven tests with opaque expected strings.
- **Correctness:** Regex validates string shape only — it cannot detect disposable subdomains, plus-address aliases (`user+tag@contoso.com`), homoglyphs, or DNS MX existence. Negative lookahead `(?!.*@mailinator…)` is easy to get wrong and still matches `user@mailinator.com.evil.net` depending on anchoring.
- **Recommended pipeline:** (1) trim/normalize input; (2) anchored syntax regex or `MailAddress` parse for basic shape; (3) extract domain segment via `Match` named groups or string split; (4) blocklist/allow-list lookup service; (5) optional async MX/DNS check off the hot path; (6) business rules (duplicate account, region lock) in plain C#.
- **When regex fits:** syntax gate only — the same practical email pattern from **Program.cs** Section 9a, anchored with `^$`.

**Production takeaway:** Regex is a syntax filter, not a policy engine — Karat uses this to test layered validation judgment, not pattern authoring bravado.

---

#### Q6. How do you mitigate ReDoS using `Regex.MatchTimeout` or the `matchTimeout` parameter in .NET? {#03-regular-expressions-q6}

(M) A config-ingestion worker parses key/value lines from Windows-generated files. Keys on lines after the first never match:

```csharp
string file = "Server=prod-db\r\nPort=5432\r\nTimeout=30";
bool secondLineMatches = Regex.IsMatch(file, @"^Port=");
// secondLineMatches == false — team expects true
```

Explain why default regex behavior fails here and what minimal change fixes it without rewriting the parser as a full state machine.

**Answer:** By default, `^` and `$` anchor only the start and end of the entire input string, not each line — so `^Port=` looks for `Port=` at position 0 of the whole blob (`"Server=…"`), never at the start of the second line after `\r\n`.

- **Mechanism:** Without `RegexOptions.Multiline`, `\r\n` is ordinary whitespace between characters; line boundaries are invisible to `^`/`$`. With `Multiline`, `^` matches after `\n` (and `\r\n` pairs) and `$` matches before `\n`.
- **Minimal fix:** pass `RegexOptions.Multiline`: `Regex.IsMatch(file, @"^Port=", RegexOptions.Multiline)` → `true`.
- **Alternative:** split lines first with `Regex.Split(file, @"\r?\n")` or `ReadLines` and test each line — clearer when you also need comment stripping or `#` handling (**Program.cs** Section 8).
- **Related gotcha:** `RegexOptions.Singleline` makes `.` span newlines — opposite concern when extracting multiline values; do not confuse Multiline (line anchors) with Singleline (dot behavior) — **Program.cs** Section 10.
- **Production note:** For large config files, line-by-line streaming avoids loading the entire file into one string match.

**Production takeaway:** Multiline is the fix for `^`/`$` per line — a common Karat mechanism question tied to Windows `\r\n` exports.

---

#### Q7. What happens when a regex times out — which exception is thrown? {#03-regular-expressions-q7}

(R) An internal tool "sanitizes" HTML fragments before storing them in a knowledge base:

```csharp
public string StripTags(string html)
{
    string noTags = Regex.Replace(html, @"<.*>", string.Empty);
    return Regex.Replace(noTags, @"<script.*?>.*?</script>", string.Empty);
}
```

Review on input `"<div>Title</div><script>alert(1)</script>"`. What goes wrong with matching order, greediness, and security assumptions?

**Answer:** The greedy `<.*>` swallows from the first `<` through the last `>` in the string — removing the entire fragment including the script block in one pass — so the second replace never sees `<script>`; even with order reversed, regex is not a safe HTML sanitizer and cannot prevent attribute-based XSS.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Greedy `<.*>` spans to last `>` | Entire `"<div>…</div><script>…</script>"` becomes one match — all content deleted or mangled |
| Correctness | Script strip runs after tag strip | Script pattern never runs if greedy pass already consumed `<script>…` |
| Security | Regex-based tag removal is not HTML parsing | `<img onerror=alert(1)>`, malformed tags, and nested contexts bypass strip |
| Security | No allow-list of safe tags/attributes | "Sanitize" gives false confidence — stored XSS in knowledge base |
| Design | Two-pass replace order-dependent | Fragile refactors break redaction silently |

**Fix (priority order):**

1. Do not use regex for HTML security — use a vetted sanitizer (`HtmlSanitizer` NuGet, AngleSharp with allow-list, or store Markdown instead of raw HTML).
2. If regex is only for non-security display cleanup, use lazy quantifiers per tag: `<.*?>` — still wrong for nested `<div><div></div></div>` but matches **Program.cs** Section 12 greedy vs lazy demo.
3. Run script removal before any broad tag pass if you must stay regex-based for legacy reasons — and treat output as untrusted anyway.
4. Add integration tests with XSS payloads, not just `"<div>Title</div>"`.

```csharp
// Display-only collapse — NOT security:
string collapsed = Regex.Replace(html, @"<.*?>", string.Empty);
```

**Production takeaway:** Greedy `<.*>` is the textbook over-match bug, and Karat pairs it with "regex ≠ sanitizer" — use proper HTML parsers for user content. See **Program.cs** `DemonstratePitfalls` greedy vs lazy comparison.

---

#### Q8. What is atomic grouping's role in preventing backtracking explosions? {#03-regular-expressions-q8}

_Answer not found._

---

#### Q9. How does culture affect case-insensitive matching, and what does `RegexOptions.CultureInvariant` do? {#03-regular-expressions-q9}

_Answer not found._

---

#### Q10. When should you compile regexes with `RegexOptions.Compiled` (or source generators in .NET 7+)? {#03-regular-expressions-q10}

_Answer not found._

---

#### Q11. What is `RegexOptions.NonBacktracking` (.NET 7+), and what trade-offs does it have? {#03-regular-expressions-q11}

_Answer not found._

---

#### Q12. How do named capture groups work, and how do you read them from `Match.Groups`? {#03-regular-expressions-q12}

_Answer not found._

---

#### Q13. What is the difference between greedy and lazy quantifiers (`+` vs `+?`)? {#03-regular-expressions-q13}

_Answer not found._

---

#### Q14. When should you prefer `Regex` over simple `string.Contains` / `Split` for maintainability? {#03-regular-expressions-q14}

_Answer not found._

---

#### Q15. How do you validate input with regex without using it as a full parser (e.g., email, phone)? {#03-regular-expressions-q15}

_Answer not found._

---

### 04. Var, Dynamic & Special Keywords

#### Q1. Explain `var` — what is known at compile time vs runtime? {#04-var-dynamic-special-keywords-q1}

Explain `var` — what is known at compile time vs runtime?

**Answer:** `var` instructs the compiler to infer the static type of a local variable from the initializer expression at compile time, so the variable still has a fixed strong type after compilation — there is no runtime type change. If inference fails or the initializer is missing, the compiler reports an error.

- `var s = "hi";` is compile-time `string`, not a runtime-decided type.
- `var` cannot declare fields without an initializer and cannot change type on reassignment.
- Reflection and `GetType()` at runtime see the inferred type, identical to an explicit declaration.
- Inference improves readability for long generic types while preserving full static checking.

---

#### Q2. When is `var` required (anonymous types) vs merely convenient? {#04-var-dynamic-special-keywords-q2}

When is `var` required (anonymous types) vs merely convenient?

**Answer:** `var` is mandatory when declaring anonymous types because the compiler generates a type name that is not expressible in source, such as `var row = new { Id = 1, Name = "A" };`. For all named types, `var` is optional convenience — explicit types remain valid and sometimes clearer for public APIs and unclear initializers.

- Anonymous projection in LINQ `select new { ... }` requires `var` or implicit typing in query syntax.
- Prefer explicit types when the initializer does not make the type obvious (`var x = GetValue();`).
- Team style guides often allow `var` when the right-hand side spells out the type clearly.
- `var` does not disable nullable warnings — inferred reference types carry NRT annotations.

---

#### Q3. Explain the `dynamic` keyword and the DLR's role. {#04-var-dynamic-special-keywords-q3}

Explain the `dynamic` keyword and the DLR's role.

**Answer:** `dynamic` tells the compiler to defer member resolution to runtime via the Dynamic Language Runtime (DLR), which performs binding at execution time using call sites that cache resolved members after the first successful bind. Static typing is bypassed for `dynamic` receivers, so compile-time member checking is disabled.

- The DLR coordinates overload resolution, implicit conversions, and `dynamic` invocation across languages and COM interop.
- First access pays binding cost; subsequent calls use cached rules until arguments types change materially.
- Errors like misspelled property names surface as `RuntimeBinderException` at runtime.
- Use sparingly for JSON DOMs, COM, and truly dynamic payloads — not for ordinary application logic.

---

#### Q4. What is the difference between `var`, `dynamic`, and `object`? {#04-var-dynamic-special-keywords-q4}

What is the difference between `var`, `dynamic`, and `object`?

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

#### Q5. When does `dynamic` defer member binding to runtime, and what errors appear only then? {#04-var-dynamic-special-keywords-q5}

When does `dynamic` defer member binding to runtime, and what errors appear only then?

**Answer:** Any member access, method call, indexer, or operator on a `dynamic` expression is resolved when that line executes, not during compilation. Missing members, wrong argument types, and ambiguous overloads throw `RuntimeBinderException` or related runtime errors that would have been compile errors on static types.

- `dynamic d = GetPayload(); d.Totla();` compiles but fails at runtime on the typo.
- Return types of dynamic calls are `dynamic` unless converted, propagating deferred binding downstream.
- Debugging is harder — IDE refactor and Find References skip dynamic sites.
- See Gotcha 8 — treat `dynamic` boundaries as explicit integration seams with tests.

---

#### Q6. What is `DynamicObject`, and when would you subclass it? {#04-var-dynamic-special-keywords-q6}

What is `DynamicObject`, and when would you subclass it?

**Answer:** `DynamicObject` is a base class for types that participate in the DLR by overriding `TryGetMember`, `TrySetMember`, `TryInvokeMember`, and related hooks to define custom dynamic behavior. Subclass it when building dynamic proxies, DSL objects, or dictionary-backed models that expose members not declared as C# properties.

- `ExpandoObject` is a built-in `DynamicObject` mapping names to object values.
- Override hooks return `true` when the operation succeeds and set `result` for get/invoke paths.
- Enables Ruby-like dynamic APIs while remaining callable from C# with `dynamic` references.
- Prefer static interfaces when shape is stable — custom `DynamicObject` is for truly open-ended models.

---

#### Q7. What is `ExpandoObject`, and how does it differ from `Dictionary<string, object>`? {#04-var-dynamic-special-keywords-q7}

What is `ExpandoObject`, and how does it differ from `Dictionary<string, object>`?

**Answer:** `ExpandoObject` implements `IDynamicMetaObjectProvider` and `IDictionary<string, object>`, so callers can use dynamic member syntax or dictionary APIs on the same bag of name-value pairs. A plain dictionary only supports indexer and dictionary methods unless wrapped — not dynamic member binding without extra glue.

- `dynamic bag = new ExpandoObject(); bag.Score = 10;` adds a property at runtime.
- Cast to `IDictionary<string, object>` to enumerate keys or serialize to JSON with appropriate options.
- Expando graphs suit lightweight JSON merge scenarios; typed models are safer for core domain entities.
- Thread safety is not automatic — treat expando instances like ordinary mutable dictionaries.

---

#### Q8. Explain `nameof` — how does it help refactoring and logging? {#04-var-dynamic-special-keywords-q8}

Explain `nameof` — how does it help refactoring and logging?

**Answer:** `nameof(expression)` resolves at compile time to the unqualified name string of a variable, type, or member, so renaming the symbol updates every `nameof` reference through the IDE refactor tools. It avoids magic strings in exceptions, logging, and property-change notifications.

- `nameof(customer.Email)` yields `"Email"` even if the expression type is broader than the member's declaring type.
- Safer than `"Email"` literals that drift during rename — compiler ties `nameof` to the symbol.
- Works on parameters, methods, and types — `nameof(OrderService.PlaceOrder)` for diagnostic messages.
- Does not evaluate runtime expressions — only simple name forms are allowed.

---

#### Q9. What is the `global::` qualifier, and when is it needed to disambiguate namespaces? {#04-var-dynamic-special-keywords-q9}

What is the `global::` qualifier, and when is it needed to disambiguate namespaces?

**Answer:** The `global::` prefix starts name lookup from the root namespace scope, bypassing user-defined aliases or nested namespaces that shadow system types. Use it when a project namespace such as `System` or `Email` collides with BCL types like `System.String`.

- Example: `global::System.IO.File` when your namespace hierarchy defines a conflicting `File` class.
- Aliases (`using IO = ...`) are usually enough, but `global::` is the unambiguous escape hatch.
- Generated code and analyzers occasionally emit `global::` for deterministic resolution.
- Rare in hand-written code — fix namespace naming first when collisions recur.

---

#### Q10. What is `default` literal (C# 7.1+) vs `default(T)`? {#04-var-dynamic-special-keywords-q10}

What is `default` literal (C# 7.1+) vs `default(T)`?

**Answer:** The `default` literal lets the compiler infer the target type from context, so `default` in `int x = default;` means `0` and in `string? s = default;` means `null` without repeating the type name. `default(T)` remains valid in generic code where the type parameter `T` is not known as a specific type at the call site.

- `default` improves readability in ternary and coalescing expressions with nullable reference types.
- For unconstrained `T`, `default` and `default(T)` both yield null for reference types and zeroed value types.
- Cannot use `default` where type inference is ambiguous — add an explicit type in those cases.
- See C# 7 Q12 for generic constraint interactions with `default`.

---

#### Q11. What is `@` verbatim identifier syntax (`@class`, `@event`) used for? {#04-var-dynamic-special-keywords-q11}

What is `@` verbatim identifier syntax (`@class`, `@event`) used for?

**Answer:** Prefix `@` allows identifiers that coincide with C# keywords — `@class`, `@event`, `@int` — so generated or interop code can use reserved words as names. Verbatim identifiers are the same CLR names without the `@` at runtime.

- Common for XML or database columns named `class` mapped into C# properties.
- `@` on strings (`@"C:\path"`) is a separate feature — verbatim string literals, not identifiers.
- Prefer renaming to non-keyword names in hand-written domain models when possible.
- JSON property names can still map via attributes without `@` in source identifiers.

---

#### Q12. What is unsafe code, and when are pointers justified in C#? {#04-var-dynamic-special-keywords-q12}

What is unsafe code, and when are pointers justified in C#?

**Answer:** Unsafe code blocks, enabled with `/unsafe` and the `unsafe` keyword, allow pointer arithmetic and fixed buffers for interop with native APIs or extreme hot paths where spans still cannot express the required semantics. Most application code never needs unsafe — `Span<T>`, `Memory<T>`, and `stackalloc` cover many performance scenarios safely.

- Required for some legacy C library interop expecting `byte*` parameters.
- Misuse introduces buffer overruns and GC pinning hazards — code review and tests are mandatory.
- Modern BCL moves toward safe abstractions; unsafe is opt-in and excluded from some sandboxed contexts.
- Keep unsafe isolated in small vetted modules with clear documentation.

---

#### Q13. What is `stackalloc`, and how does it relate to performance-sensitive code? {#04-var-dynamic-special-keywords-q13}

What is `stackalloc`, and how does it relate to performance-sensitive code?

**Answer:** `stackalloc` allocates a block of memory on the stack for value types, avoiding heap allocations for small temporary buffers when used with `Span<T>` in safe contexts (C# 7.2+). It suits short-lived arrays in parsing, crypto, or formatting hot paths where heap pressure matters.

- `Span<int> buf = stackalloc int[32];` keeps allocation scoped to the method frame.
- Large `stackalloc` can overflow the stack — cap sizes and spill to `ArrayPool` for big buffers.
- C# 8 integrated `stackalloc` into safe patterns alongside `Span` without requiring unsafe blocks in many cases.
- Profile before micro-optimizing — allocator improvements in the runtime already reduce small array costs.

---

#### Q14. What is `ref readonly` return, and how does it differ from returning by value? {#04-var-dynamic-special-keywords-q14}

What is `ref readonly` return, and how does it differ from returning by value?

**Answer:** A `ref readonly` return exposes a read-only reference to an existing storage location (often a large struct field or array element) without copying bytes, while preventing the caller from mutating through that reference. Returning by value copies the entire struct, which can be expensive for large value types.

- Caller receives `ref readonly T` and reads fields without taking a writable alias.
- Useful for exposing items from internal buffers while preserving encapsulation.
- Distinct from `in` parameters — `ref readonly` is about return paths.
- Large readonly struct returns in hot loops are a primary use case for this feature.

---

#### Q15. How does `dynamic` interact with extension methods (why don't they bind dynamically)? {#04-var-dynamic-special-keywords-q15}

How does `dynamic` interact with extension methods (why don't they bind dynamically)?

**Answer:** Extension methods are resolved statically at compile time based on the static type of the receiver expression, so a `dynamic` receiver does not see extension methods unless you cast to the static type or invoke the extension as a static call. The DLR does not participate in extension method lookup.

- `dynamic d = ...; d.Extension();` fails at runtime even if an extension exists for the runtime type.
- Call `MyExtensions.Extension((ConcreteType)d)` or cast before invoking extensions.
- See Gotcha 9 — this surprises developers mixing LINQ-style extensions with dynamic JSON models.
- Prefer static types or wrapper methods at the dynamic boundary instead of relying on extensions.

---

### 05. C# 7 Features

#### Q1. What are tuple deconstruction and named tuple elements? {#05-c-7-features-q1}

(R) Express VIP orders are routed to the standard express lane in production. Review this C# 7 switch with `when` guards (mirrors the warehouse routing demo):

**Answer:** The `Express` case without a `when` guard matches every express order first, so the `Express when order.IsHighValue` branch is unreachable dead code — high-value express orders never reach VIP routing.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Unconditional `case OrderPriority.Express` precedes guarded case | VIP lane never selected; SLA breach on high-value express |
| Pattern matching | Misunderstanding first-match-wins switch semantics | Silent logic bug — compiles and runs |
| Testing | Tests that only assert "express → some express lane" pass | Mixed-batch production traffic exposes wrong lane |

**Fix (priority order):**

1. Reorder cases so **more specific patterns come first** — mirror the tutorial's `RouteOrder` in **Program.cs** Section 5:

```csharp
switch (order.Priority)
{
    case OrderPriority.Critical:
        return "CRITICAL-LANE";
    case OrderPriority.Express when order.IsHighValue:
        return "EXPRESS-VIP";
    case OrderPriority.Express:
        return "EXPRESS-STANDARD";
    case OrderPriority.Standard when order.Quantity > 100:
        return "BULK-STANDARD";
    case OrderPriority.Standard:
        return "STANDARD";
    default:
        return "UNKNOWN";
}
```

2. Add a test matrix: `(Express, high-value)`, `(Express, low-value)`, `(Standard, qty>100)` — assert distinct lanes.
3. Consider a C# 8+ switch expression later for exhaustiveness; C# 7 switch still requires manual ordering discipline.

**Production takeaway:** C# 7 switch patterns behave like ordered rule lists, not `if/else if` auto-reordering — Karat embeds this as a routing bug that compiles cleanly. See **Program.cs** QUICK REFERENCE — "Pattern case order wrong → Wrong branch taken."

---

#### Q2. How do `out` variables declared inline in method calls work? {#05-c-7-features-q2}

(R) A product lookup was optimized with `ValueTask<string>` for cache hits. Under retry logic, intermittent `InvalidOperationException` appears. Review:

**Answer:** A consumed `ValueTask` must not be awaited twice unless it wraps a `Task` or `IValueTaskSource` — the retry path re-awaits the same instance after the cache-hit path already completed it synchronously, causing undefined behavior or `InvalidOperationException`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Async / ValueTask | Double `await` on same `ValueTask` instance | Intermittent crash on cache-hit + empty-name retry |
| API contract | Caller stores and reuses `ValueTask` like `Task` | Violates ValueTask consumption rules |
| Design | Retry treats empty string as fetch failure without re-invoking | Masks catalog bugs; triggers invalid reuse |

**Fix (priority order):**

1. **Do not cache the `ValueTask`** — call `GetProductNameAsync` again on retry, or await once into a `string`:

```csharp
order.ProductName = await _lookup.GetProductNameAsync(order.ProductId);

if (string.IsNullOrEmpty(order.ProductName))
    order.ProductName = await _lookup.GetProductNameAsync(order.ProductId);
```

2. Better: return `Task<string>` on public boundaries unless profiling proves allocation pressure; keep `ValueTask` internal to hot paths documented as single-consumption.
3. Fix empty-name handling at source — distinguish "not found" from empty string instead of blind retry.
4. Add analyzer discipline: treat `ValueTask`/`ValueTask<T>` like `IDisposable` — one consumer.

**Production takeaway:** `ValueTask` wins on cache hits (**Program.cs** Section 9) but fails when callers treat it like a reusable `Task` — a common Karat stack of optimization + retry logic.

---

#### Q3. What are discards (`_`), and where are they used (deconstruction, unused returns)? {#05-c-7-features-q3}

(R) A developer refactors inventory reservation to use C# 7 ref returns for in-place updates. The build fails; after a workaround it crashes in QA. Review:

**Answer:** `ref` locals and `ref` returns cannot appear in `async` methods (state machine restriction), and `ref` returns must target stable storage — a `List<T>` indexer returns a temporary ref in many contexts, not a durable slot alias safe across growth/reallocation.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `ref` local in `async Task ReserveAsync` | CS8170 / CS4012 — async methods cannot use ref locals |
| Runtime / API | `ref counts[index]` on `List<int>` | Ref may not track list reallocation; corrupt or invalid updates |
| Design | Sync ref mutation mixed with awaited I/O | Reservation not atomic with commit; race if ever made to compile |

**Fix (priority order):**

1. Remove `ref` from async paths — compute new value after `await`, assign by index:

```csharp
public async Task ReserveAsync(Order order, int[] inventory, int skuIndex)
{
    await Task.Delay(10);
    inventory[skuIndex] -= order.Quantity;
}
```

2. Use **arrays** (as in **Program.cs** `FindInventorySlot`) for in-place ref mutation in synchronous hot paths only.
3. For `List<T>`, mutate via index assignment or lock — do not expose `ref` return on list indexer.
4. Keep `FindSlot` synchronous; if reservation needs I/O, read/modify/write as explicit steps with concurrency control (`lock`, `Interlocked`, or DB transaction).

**Production takeaway:** Ref returns suit fixed buffers (**Program.cs** Section 8); pairing them with `async` or `List<T>` is a stacked compile + lifetime trap Karat uses to test feature boundaries.

---

#### Q4. Explain pattern matching enhancements in C# 7 — `is` type patterns and `switch` patterns. {#05-c-7-features-q4}

(R) A CSV import pipeline uses C# 7 out variables. Finance sees rows with quantity `0` marked as successfully imported. Review:

**Answer:** `int.TryParse` failure on quantity is ignored — `out var qty` defaults to `0` and import still returns `true` because only `orderId` failure short-circuits; invalid quantity silently becomes zero.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Quantity parse result not checked | Bad CSV → qty 0 orders marked successful |
| Out variables | `out var` makes parse easy to "fire and forget" | C# 7 ergonomics hide missing validation |
| Data integrity | `return true` after partial parsing | Downstream fulfillment and billing act on garbage |

**Fix (priority order):**

1. Check every `TryParse` return value — C# 7 inline `out` does not remove validation duty:

```csharp
if (!int.TryParse(columns[2], out var qty) || qty <= 0)
    return false;

row.Quantity = qty;
```

2. Validate SKU with explicit failure when required — `TryGetValue` fallback to raw string may be intentional, but document it.
3. Consider `RequirePositiveQuantity` pattern from **Program.cs** Section 11 (throw expression) for internal APIs vs `Try*` for import boundaries.
4. Log row index and raw columns on `false` — ops needs rejected rows, not silent zeros.

**Production takeaway:** Out variables reduce boilerplate (**Program.cs** Section 3) but Karat tests whether you still branch on the `bool` — `out var` without checking is a production data bug.

---

#### Q5. What are `ref` returns and `ref` locals, and what safety rules apply? {#05-c-7-features-q5}

(P) A warehouse fulfillment microservice returns `(bool CanFulfill, string Note)` tuples from `CheckFulfillment` — the same shape as the tutorial's tuple demo. The team debates replacing tuples with a `FulfillmentResult` record before exposing the method on a public NuGet contract. When is the tuple idiomatic, and when does it break production maintainability?

**Answer:** Named tuples are fine for private or internal helpers with stable, obvious element semantics; public NuGet contracts need a named type so additions, serialization, and versioning do not break consumers silently.

- **Keep tuples** for internal methods with two or three tightly coupled values and no evolution expected — e.g. private `CheckFulfillment` inside one service class, same assembly as **Program.cs** Section 6 demo.
- **Use a record/class** when the shape crosses assembly boundaries, gains fields (`ReservedQuantity`, `BackorderSku`), needs JSON/XML mapping, or appears in logs/metrics dashboards — `(bool, string)` element names are not part of the runtime contract.
- **Inferred tuple names (C# 7.1)** help readability at the return site but do not replace API documentation for external callers.
- **Tuple equality (C# 7.3)** is useful for tests comparing snapshots; not a substitute for domain identity on persisted entities.

**Production takeaway:** Tuples are a local convenience feature — Karat asks you to draw the line at package/public API surfaces where contract evolution and tooling (OpenAPI, analyzers) require named types.

---

#### Q6. What is `ref`/`in`/`out` in the context of `ReadOnlySpan`-era performance APIs (conceptual link)? {#05-c-7-features-q6}

(M) A batch job uses a local function with captured outer state to retry flaky lane assignments. Ops reports duplicate reservations on the same SKU after parallel batch splits. Review:

**Answer:** The local function captures `reservationFailures` and mutates `inventory` through `ref` aliases while `Parallel.ForEach` runs handlers concurrently — non-atomic read-modify-write on shared array slots and non-interlocked increments corrupt counts under race.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Concurrency | `Parallel.ForEach` + unsynchronized `slot -= order.Quantity` | Lost updates → overselling inventory |
| Local functions | Closure over `reservationFailures++` | Race on int increment; inaccurate metrics |
| Ref locals | `ref int slot = ref inventory[skuIndex]` in parallel bodies | Each thread aliases same storage without lock |

**Fix (priority order):**

1. Do not parallelize in-place mutation on a shared array without synchronization — use `lock` per SKU, `ConcurrentDictionary`, or partition work by SKU.
2. Replace `reservationFailures++` with `Interlocked.Increment(ref reservationFailures)` if you keep shared counters.
3. Prefer **static local functions (C# 8+)** or private methods that take explicit parameters — makes captured state visible in review; see **Program.cs** Section 7 note on static locals.
4. For warehouse scale-out, reservation belongs in a transactional store (DB row lock), not a shared in-memory array across parallel threads.

**Production takeaway:** Local functions + ref locals are synchronous, single-flow tools from C# 7 — Karat stacks them with `Parallel.ForEach` to test whether you recognize closure and alias concurrency, not just syntax.

---

#### Q7. What are local functions, and how do they differ from lambdas for recursion and capture? {#05-c-7-features-q7}

(D) Two teammates implement guard clauses for order validation. Which approach do you standardize on for a shared domain library, and why?

**Answer:** Standardize on **Option B (classic blocks)** for shared domain validation libraries, and allow **Option A (throw expressions)** only for thin one-liners where exception detail stays minimal — not as the default for public APIs that operators debug from logs.

- **Throw expressions** pair well with expression-bodied members (**Program.cs** Sections 10–11) for null-guard properties and private helpers — `order ?? throw new ArgumentNullException(nameof(order))` is clear and concise.
- **Classic blocks** win when you need overloads with `(paramName, actualValue, message)` on `ArgumentOutOfRangeException`, multiple guards, or XML doc that describes thrown types — Option B's qty check carries the offending value; Option A's ternary does not.
- **Refactor safety:** Expression-bodied throw chains are harder to breakpoint and step through in production debugging than block bodies.
- **Consistency:** Mixed styles across a NuGet domain library frustrate code review — pick block guards for public methods, throw expressions for small internal `Require*` helpers like **Program.cs** `RequireOrder` / `RequirePositiveQuantity`.

**Production takeaway:** C# 7 throw expressions are idiomatic guards, not a wholesale replacement for validation methods — Karat tests judgment on expression-bodied brevity vs operability in a shared library.

---

#### Q8. What are expression-bodied members beyond properties (methods, constructors, finalizers)? {#05-c-7-features-q8}

_Answer not found._

---

#### Q9. What binary literals and digit separators (`0b1010`, `1_000_000`) improve in readability? {#05-c-7-features-q9}

_Answer not found._

---

#### Q10. What is `throw` as an expression inside ternary/null-coalescing forms? {#05-c-7-features-q10}

_Answer not found._

---

#### Q11. How do generalized async return types work (`ValueTask` as async return)? {#05-c-7-features-q11}

_Answer not found._

---

#### Q12. What are `default` in generic constraints improvements in C# 7? {#05-c-7-features-q12}

_Answer not found._

---

### 06. C# 8 Features

#### Q1. Explain nullable reference types — how do they differ from `Nullable<T>` value types? {#06-c-8-features-q1}

(R) After enabling `<Nullable>enable</Nullable>` on the document-ingest API, QA reports intermittent `NullReferenceException` on documents with no footnotes. Review the service:

```csharp
public sealed class DocumentIngestService
{
    public string BuildSummary(DocumentMetadata metadata)
    {
        string header = metadata.Id!.Trim();
        string note = metadata.Notes.ToUpperInvariant();
        return $"{header}: {note}";
    }

    public DocumentMetadata LoadFromJson(string json)
    {
        var doc = JsonSerializer.Deserialize<DocumentMetadata>(json);
        doc!.Id = doc.Id ?? "UNKNOWN";
        return doc;
    }
}

public sealed class DocumentMetadata
{
    public string Id { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
```

What problems remain after the developer "fixed" compiler warnings with `!`, and how do you harden this for production?

**Answer:** Null-forgiving operators silence the compiler without proving non-null at runtime — `Notes` is still null for many documents, and `JsonSerializer.Deserialize` can return null entirely. Production NRT requires honest annotations plus guards at boundaries, not blanket `!`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| NRT misuse | `metadata.Notes.ToUpperInvariant()` on `string?` | `NullReferenceException` when `Notes` is null — matches QA report |
| NRT misuse | `metadata.Id!` without validation | Masks missing IDs; empty or null IDs slip into downstream queues |
| Correctness | `Deserialize` result used without null check | `NullReferenceException` on malformed or `"null"` JSON |
| Design | Suppressions instead of contract | Warnings return on next edit; team learns to ignore NRT |

**Fix (priority order):**

1. Handle optional notes explicitly: `var note = metadata.Notes?.ToUpperInvariant() ?? "(no notes)";` — mirrors **Program.cs** Section 8.
2. Validate `metadata` and `metadata.Id` at the API boundary; throw `ArgumentException` or return `ProblemDetails` for bad input — do not use `!` on unvalidated data.
3. Guard deserialization: `var doc = JsonSerializer.Deserialize<DocumentMetadata>(json) ?? throw new JsonException("…");` or return `DocumentMetadata?` and let callers decide.
4. Configure `JsonSerializerOptions` with required-property validation (modern) or a dedicated DTO layer for ingest.
5. Treat new CS86xx warnings as build breaks in CI for touched projects — block `#nullable disable` without ticket.

**Production takeaway:** NRT is a **contract tool**, not a runtime checker — `!` on inbound HTTP/JSON data recreates the null bugs you enabled NRT to prevent. See **Program.cs** Section 8 — nullable annotations and `??` for optional `Notes`.

---

#### Q2. What do `?`, `!`, and `#nullable` directives mean at compile time? {#06-c-8-features-q2}

(R) A background worker streams archive pages to blob storage. Under deploy cancellation, the job keeps running for minutes and sometimes OOMs. Review the consumer and producer:

```csharp
public async Task ArchivePagesAsync(CancellationToken stoppingToken)
{
    var allPages = _documentStream.ReadPagesAsync()
        .ToListAsync(stoppingToken)
        .GetAwaiter()
        .GetResult();

    foreach (string page in allPages)
    {
        await _blobWriter.UploadPageAsync(page);
    }
}

public async IAsyncEnumerable<string> ReadPagesAsync(
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
{
    foreach (string page in _pages)
    {
        await Task.Delay(200, cancellationToken);
        yield return page;
    }
}
```

Identify stacked compile-time, runtime, and scalability issues. What is the correct streaming pattern?

**Answer:** The consumer buffers the entire async stream into memory via sync-over-async `.GetResult()`, defeating `IAsyncEnumerable` streaming and ignoring cooperative cancellation during enumeration. Large archives OOM; deploy stops cannot abort the materialization phase promptly.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Async | `.GetResult()` on `ToListAsync` | Sync-over-async; thread-pool blocking; potential deadlocks in hosted contexts |
| Scalability | Materialize-all before upload | Memory proportional to total pages — OOM on large documents |
| Cancellation | `ReadPagesAsync()` called without token | `[EnumeratorCancellation]` never receives `stoppingToken`; delay loop ignores host shutdown |
| Design | Stream treated like `List<T>` | Loses backpressure; cannot start uploading until full read completes |

**Fix (priority order):**

1. Consume with streaming: `await foreach (var page in _documentStream.ReadPagesAsync(stoppingToken).WithCancellation(stoppingToken))` and upload inside the loop — matches **Program.cs** Sections 9–10.
2. Remove `.ToListAsync().GetResult()` entirely; keep the method `async` end-to-end.
3. Pass `stoppingToken` into `ReadPagesAsync(stoppingToken)` so cancellation propagates into `Task.Delay` and the async iterator tears down.
4. Optionally bound concurrency with a `SemaphoreSlim` if uploads overlap, but never buffer the whole sequence unless size is proven bounded.
5. Use `await using` on `AsyncDocumentStream` when the producer holds connections — **Program.cs** `IAsyncDisposable` demo.

**Production takeaway:** `IAsyncEnumerable<T>` is for **incremental** async production — buffering it to a list is an anti-pattern unless you have a hard upper bound. Cancellation must flow through `WithCancellation` / `[EnumeratorCancellation]`.

---

#### Q3. Are nullable reference annotations enforced at runtime? {#06-c-8-features-q3}

(R) A routing microservice parses document IDs with C# 8 ranges after a format change. Production throws `ArgumentOutOfRangeException` on valid-looking IDs. Review:

```csharp
public string ExtractYear(string documentId)
{
    return documentId[4..8];
}

public string ExtractSequence(string documentId)
{
    return documentId[^3..];
}

public DocumentMetadata[] TakeTail(DocumentMetadata[] batch, int tailCount)
{
    return batch[^tailCount..];
}

public void ValidateId(string documentId)
{
    if (documentId.StartsWith("DOC-"))
    {
        string year = ExtractYear(documentId);
        _metrics.RecordYear(year);
    }
}
```

Expected format: `DOC-YYYY-NNN` (minimum 12 characters). What assumptions break in production, and how do you fix parsing defensively?

**Answer:** Range and index syntax does not validate length — it throws when the span is too short or when `^tailCount` exceeds the array. `StartsWith("DOC-")` is necessary but not sufficient for a 12-character ID, so truncated or legacy IDs pass validation then fail inside slice helpers.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | `documentId[4..8]` on short strings | `ArgumentOutOfRangeException` — 500s on legacy `DOC-24-001` style IDs |
| Runtime | `batch[^tailCount..]` when `tailCount > batch.Length` | Same exception when batch is smaller than requested tail |
| Correctness | Validation gate too weak | Metrics and routing run on IDs that cannot satisfy slice contracts |
| Design | Magic numeric indices scattered | Format change requires hunting literal `4`, `8`, `^3` across services |

**Fix (priority order):**

1. Centralize format validation: `if (documentId.Length < 12) return false;` or use `Regex` / `ReadOnlySpan<char>` with explicit length checks before any range.
2. Replace magic slices with named helpers or `Range` constants tied to the format spec — **Program.cs** Section 11 shows `id[4..8]` and `id[^3..]` only after a known-good sample.
3. Guard `TakeTail`: `if (tailCount <= 0) return Array.Empty<…>();` and clamp: `int start = Math.Max(0, batch.Length - tailCount); return batch[start..];`
4. Return `bool TryExtractYear(string id, out string year)` instead of throwing parsers — callers record parse failures without crashing the request pipeline.
5. Add contract tests for min-length, max-length, and migrated ID formats.

**Production takeaway:** C# 8 ranges are **syntax sugar over Index/Range** — they inherit all bounds-check behavior. Validate once at the boundary; never assume `"DOC-"` implies slice-safe length.

---

#### Q4. What are default interface methods, and how do they relate to the diamond problem? {#06-c-8-features-q4}

(R) A teammate refactors chunk parsing to overlap I/O with processing. The build fails and code review finds async/ref-struct mixing. Review:

```csharp
public async Task<int> ProcessHeaderAsync(string headerLine)
{
    ReadOnlySpan<char> idSpan = headerLine.AsSpan();

    using DocumentChunkReader chunkReader = new DocumentChunkReader(idSpan);

    await Task.Delay(10);

    return chunkReader.VisibleLength;
}

public ref struct DocumentChunkReader
{
    private ReadOnlySpan<char> _buffer;

    public DocumentChunkReader(ReadOnlySpan<char> buffer) => _buffer = buffer;

    public int VisibleLength => _buffer.Length;

    public void Dispose() => _buffer = ReadOnlySpan<char>.Empty;
}
```

What C# 8 rules are violated, and what refactor preserves both async I/O and span-based parsing?

**Answer:** `ref struct` instances cannot survive an `await` because the async state machine may move execution to the heap — the compiler rejects storing `DocumentChunkReader` across suspension points. Disposable ref structs are stack-only and must be disposed before the first `await` in the method.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `ref struct` local used after `await` | CS4012 / CS9202 — build failure |
| Correctness | `ReadOnlySpan<char>` tied to `headerLine` lifetime | If overlap with mutation or stack pop, span could be invalid — less common with `string` but real with stack buffers |
| Design | Mixing stack-only parsing with async I/O in one scope | Forces either copy-to-string or split phases |

**Fix (priority order):**

1. **Parse-then-await split:** dispose reader and extract needed scalars (`int length = chunkReader.VisibleLength;`) **before** any `await` — same pattern as **Program.cs** Section 7 synchronous `using` block.
2. Copy to owned memory when async work must interleave: `string id = headerLine;` or `char[] buffer = headerLine.ToCharArray();` then parse without ref struct across awaits.
3. Keep `DocumentChunkReader` synchronous; perform async I/O first, then run span-based parsing on the fetched `string`/`ReadOnlyMemory<char>`.
4. Do not box ref structs or store them in fields — C# 8 disallows it by design.

```csharp
public async Task<int> ProcessHeaderAsync(string headerLine)
{
    await Task.Delay(10);

    ReadOnlySpan<char> idSpan = headerLine.AsSpan();
    using var chunkReader = new DocumentChunkReader(idSpan);
    return chunkReader.VisibleLength;
}
```

**Production takeaway:** C# 8 disposable ref structs pair with **synchronous** hot paths over spans — async methods need a phase boundary before stack-only types enter the picture.

---

#### Q5. What are asynchronous streams (`IAsyncEnumerable<T>` and `await foreach`)? {#06-c-8-features-q5}

(P) Your team ships `IDocumentProcessor` as a shared NuGet package. Version 1 has `Process` and `ProcessorName`. Version 2 needs a `Describe()` helper without forcing every consumer to recompile. A consumer already implements both `IDocumentProcessor` and `IArchiveReporter`, each adding a default `Describe()`. How do you evolve the interface using C# 8 default interface methods, and what breaks if you ignore diamond ambiguity?

**Answer:** Add `Describe()` as a **default interface method** on `IDocumentProcessor` so existing implementers inherit behavior at runtime without source changes, while new implementers may override selectively — but when two interfaces supply the same default signature, the implementing class must resolve ambiguity explicitly.

- **Safe evolution:** `string Describe() => $"{ProcessorName} processor";` on the interface — matches **Program.cs** Section 3 (`TextDocumentProcessor` uses default; `MarkupDocumentProcessor` overrides).
- **Binary compatibility:** Consumers compiled against v1 load v2 because DIMs are resolved at runtime via interface dispatch — no mandatory recompile for default-only additions.
- **Override path:** Document that implementers *may* replace `Describe()` for custom telemetry; do not require it.
- **Diamond ambiguity:** If `IArchiveReporter` also adds `string Describe() => "archive";`, `class Worker : IDocumentProcessor, IArchiveReporter` fails with CS0108/CS0539-style ambiguity when calling `Describe()` on the class without explicit qualification.
- **Resolution:** Explicit interface implementation — `string IDocumentProcessor.Describe() => …;` and `string IArchiveReporter.Describe() => …;` — or rename one method before shipping.
- **Testing:** Run contract tests against **interface-typed** references, not only concrete types — default vs override behavior differs by static type.

**Production takeaway:** Default interface methods are for **additive, non-breaking** API evolution — not a free pass; colliding defaults across interfaces are a design smell that must be resolved before publish.

---

#### Q6. Explain null-coalescing assignment (`??=`) with examples. {#06-c-8-features-q6}

(M) A nightly batch opens thousands of small files under a shared directory. After migrating to `using var`, ops reports "too many open files" and memory climbs until the job finishes. Review the loop:

```csharp
public BatchResult IngestFolder(string rootPath)
{
    var result = new BatchResult();
    IEnumerable<string> paths = Directory.EnumerateFiles(rootPath, "*.meta");

    foreach (string path in paths)
    {
        using var stream = File.OpenRead(path);
        using var reader = new StreamReader(stream);

        DocumentMetadata? meta = JsonSerializer.Deserialize<DocumentMetadata>(reader.ReadToEnd());
        if (meta is not null)
        {
            result.Accept(meta);
        }
    }

    return result;
}
```

The developer expected each file handle to close after each iteration. What does C# 8 `using var` actually do here, and how do you fix it?

**Answer:** `using var` disposes at the end of the **enclosing scope** — here the entire `foreach` block — not at the end of each iteration. Every opened `FileStream` stays alive until the loop completes, exhausting file descriptors on large folders.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Resource lifetime | `using var` scope = whole `foreach` block | Thousands of concurrent open handles — "too many open files" |
| Memory | Undisposed streams and readers | Native and managed buffers accumulate until batch end |
| Misconception | Treating `using var` like block-scoped `using (...)` per iteration | Classic C# 8 migration trap — **Program.cs** Section 5 notes end-of-scope disposal |

**Fix (priority order):**

1. Restore per-iteration block scope:

```csharp
foreach (string path in paths)
{
    using (var stream = File.OpenRead(path))
    using (var reader = new StreamReader(stream))
    {
        DocumentMetadata? meta = JsonSerializer.Deserialize<DocumentMetadata>(reader.ReadToEnd());
        if (meta is not null) result.Accept(meta);
    }
}
```

2. Or wrap the body in an explicit `{ … }` block and place `using var` inside that inner block so scope ends each iteration.
3. Keep `using var` at method level only when a single resource should live for the whole method — intentional pattern in **Program.cs** `DemonstrateUsingDeclarations`.
4. Remember reverse dispose order when multiple `using var` share a scope — last declared disposes first.

**Production takeaway:** `using var` scope follows ** braces**, not developer intent — in loops, prefer classic `using (...)` or an inner block per iteration.

---

#### Q7. Explain range (`..`) and index (`^`) operators — how does `^1` differ from `Length - 1`? {#06-c-8-features-q7}

(D) A 400-project solution enables nullable reference types repo-wide. CI surfaces 8,000 warnings; developers blanket `#nullable disable` on touched files and sprinkle `!` to merge PRs. As tech lead, what migration strategy do you recommend for a document-archive domain with heavy `string` metadata (IDs, paths, optional notes), and where do `string?`, null checks, `[NotNullWhen]`, and suppressions belong versus suppressions?

**Answer:** Treat NRT as a phased contract rollout — annotate models and boundaries first, ratchet warnings to errors per project, and reserve `!` and `#nullable disable` for narrow legacy islands with tracked debt — not as the default merge strategy.

- **Phase by layer:** Start with DTOs like `DocumentMetadata` (**Program.cs** Section 8) — `string Id`, `string? Notes` — then services, then UI/API edges; bottom-up reduces noise in consumers.
- **Nullable context per project:** Enable `<Nullable>enable</Nullable>` on new projects immediately; migrate existing assemblies one csproj at a time with warning baselines that **cannot increase** on PR.
- **Domain rules:** Required archive identifiers and paths → non-nullable `string` with constructor/init validation; footnotes, tags, soft-delete reasons → `string?` with `??` defaults at read time.
- **Boundary guards:** JSON, database, and query parameters enter as possibly null — validate once, then flow non-null inward; use `[NotNullWhen(true)]` on `TryParse`-style helpers in shared utilities.
- **Suppressions policy:** Allow `!` only with adjacent comment explaining invariant (e.g., after `ArgumentNullException.ThrowIfNull`); ban file-wide `#nullable disable` except generated code or vendored files on an allowlist.
- **Tooling:** Roslyn analyzers + `dotnet format` + CI `TreatWarningsAsErrors` for CS86xx in migrated projects; Nullable Public API annotations for library packages.
- **Avoid:** Mass `#nullable disable` — it defeats the rollout and hides real null bugs in document paths where bad data is common.

**Production takeaway:** NRT migration succeeds when **annotations match business optionality** (required ID vs optional note) and CI enforces shrinking warning debt — not when teams learn to silence the compiler.

---

#### Q8. What are `using` declarations vs `using` statements for IDisposable? {#06-c-8-features-q8}

_Answer not found._

---

#### Q9. What are nullable-aware APIs in the BCL reacting to NRT (`NotNullWhen`, `MaybeNull`)? {#06-c-8-features-q9}

_Answer not found._

---

#### Q10. What is `IAsyncDisposable`, and how does `await using` work? {#06-c-8-features-q10}

_Answer not found._

---

#### Q11. What are static local functions, and why were they added? {#06-c-8-features-q11}

_Answer not found._

---

#### Q12. What is a `readonly struct`, and what mutability restrictions apply to its members? {#06-c-8-features-q12}

_Answer not found._

---

#### Q13. What is the `readonly` modifier on struct instance members (C# 8)? {#06-c-8-features-q13}

_Answer not found._

---

#### Q14. What are stackalloc in safe contexts and `Span<T>` integrations introduced alongside C# 8? {#06-c-8-features-q14}

_Answer not found._

---

#### Q15. What is target-typed `new()` vs explicit type names? {#06-c-8-features-q15}

_Answer not found._

---

### Cross-chapter — Records & Pattern Matching *(C# 9–11; grouped here)*

#### Q1. What are records (C# 9), and what boilerplate do they synthesize? {#cross-chapter-records-pattern-matching-c-911-grouped-here-q1}

What are records (C# 9), and what boilerplate do they synthesize?

**Answer:** Records are reference or value types optimized for immutable data carriers; the compiler synthesizes value-based equality, `GetHashCode`, `ToString`, and copy-with helpers depending on syntax. Positional record declarations also generate constructor parameters mapped to properties.

- `record class Person(string Name, int Age);` creates init-only properties and structural equality.
- Reduces manual `Equals`/`GetHashCode` for DTOs compared to classic classes.
- `with` expressions clone with selective overrides — see Q5.
- Choose records when identity is defined by data, not object reference alone.

---

#### Q2. What is the difference between `record class` and `record struct`? {#cross-chapter-records-pattern-matching-c-911-grouped-here-q2}

What is the difference between `record class` and `record struct`?

**Answer:** `record class` declares a reference type with reference semantics and default nullability like classes, while `record struct` is a value type with copied storage and different equality boxing behavior. Both support value-based equality, but lifetime and mutability defaults differ.

- `record struct` avoids heap allocation for small immutable value bundles.
- Reference records still allocate on the heap — see Gotcha 13.
- Struct records can be readonly by declaration; class records use init accessors.
- Pick struct records for small composite keys; class records for larger shared DTO graphs.

---

#### Q3. How does value-based equality in records differ from default class equality? {#cross-chapter-records-pattern-matching-c-911-grouped-here-q3}

How does value-based equality in records differ from default class equality?

**Answer:** Default classes use reference equality unless overridden; records override equality to compare values of all included data members in order, so two distinct instances with the same data compare equal. Hash codes combine member values consistently for dictionary use.

- Reference equality (`ReferenceEquals`) may still be false when value equality is true for records.
- Derived record equality includes base and derived members when properly declared.
- Serialization round-trips benefit — reconstructed DTOs equal originals by value.
- Mutable classes without overrides compare by reference — a common DTO bug records fix.

---

#### Q4. What is the difference between positional records and records with manual properties? {#cross-chapter-records-pattern-matching-c-911-grouped-here-q4}

What is the difference between positional records and records with manual properties?

**Answer:** Positional syntax `record R(int Id, string Name);` declares primary constructor parameters that become init-only properties automatically, while manual records declare properties inside the body with optional custom validation or computed members. Both can be records with value equality when configured.

- Positional form is concise for flat DTOs; manual form suits complex initialization logic.
- Manual records can mix init and calculated properties not tied to constructor parameters.
- Primary constructor parameters are not always public fields — they become properties unless customized.
- Choose positional for API models; manual when encapsulation requires private setters or factories.

---

#### Q5. Explain `with` expressions — how do they relate to non-destructive mutation? {#cross-chapter-records-pattern-matching-c-911-grouped-here-q5}

Explain `with` expressions — how do they relate to non-destructive mutation?

**Answer:** The `with` expression clones a record instance and overrides selected init properties, producing a new instance without mutating the original — non-destructive mutation. Syntax: `var updated = original with { Age = original.Age + 1 };`.

- Works on records with init accessors; classic mutable classes lack `with` unless customized.
- Under the hood the compiler synthesizes a copy constructor consuming member values.
- Ideal for functional-style updates in immutable domain models.
- Reference-type records still allocate new objects — not in-place field updates.

---

#### Q6. What are init-only setters (`init`), and how do they differ from `{ get; set; }` and `{ get; }`? {#cross-chapter-records-pattern-matching-c-911-grouped-here-q6}

What are init-only setters (`init`), and how do they differ from `{ get; set; }` and `{ get; }`?

**Answer:** `init` accessors allow property assignment only during object initialization (object initializer, constructor, or `with`), preventing mutation after construction completes. `{ get; set; }` allows ongoing mutation; `{ get; }` without init allows assignment only in constructors declared in the type.

- Init properties support immutable DTOs deserialized from JSON when paired with constructors.
- After construction, `obj.Prop = x` fails for init-only properties.
- Records commonly use init for all data members.
- Distinct from `readonly` fields — init applies to properties with broader initializer syntax.

---

#### Q7. Can init-only properties be set inside the type's constructors after object creation semantics? {#cross-chapter-records-pattern-matching-c-911-grouped-here-q7}

Can init-only properties be set inside the type's constructors after object creation semantics?

**Answer:** Init accessors are settable during the instance construction phase — including constructors and object initializers — before the object is fully constructed and exposed. Once construction completes, init properties behave like get-only from external code.

- Constructor bodies can assign init properties on `this` during construction.
- Deserializers use parameterized constructors or `[JsonInclude]` paths to satisfy init-only models.
- Do not confuse with post-construction mutation — external callers cannot re-init.
- Primary constructors in later C# versions map parameters to init properties automatically.

---

#### Q8. What is primary constructor syntax for records/classes (C# 12 preview cross-ref) vs positional records? {#cross-chapter-records-pattern-matching-c-911-grouped-here-q8}

What is primary constructor syntax for records/classes (C# 12 preview cross-ref) vs positional records?

**Answer:** Positional records (C# 9) tie constructor parameters directly to generated properties in one declaration, while C# 12 primary constructors generalize parameter lists on any class or struct, capturing parameters into fields or properties with explicit body usage. Both reduce boilerplate but differ in generated members and inheritance rules.

- Positional `record R(T x)` always exposes property `x` unless customized.
- Primary constructors on classes may capture into private fields without auto-properties unless declared.
- Inheritance with primary constructors requires careful base constructor chaining.
- Use positional records for simple DTOs; primary constructors when mixing custom logic in the type body.

---

#### Q9. What is pattern matching in modern C# beyond C# 7 — switch expressions, relational, logical, and property patterns? {#cross-chapter-records-pattern-matching-c-911-grouped-here-q9}

What is pattern matching in modern C# beyond C# 7 — switch expressions, relational, logical, and property patterns?

**Answer:** Modern pattern matching adds switch expressions (`var y = x switch { ... }`), property patterns matching nested shape, relational patterns comparing ordered values, and logical combinators `and`, `or`, `not`. Together they replace verbose cascade if-chains with exhaustive, expression-oriented rules.

- Switch expressions require a result expression in each arm — no fall-through statements.
- Property patterns destructure: `order is { Status: OrderStatus.Shipped, Total: > 0 }`.
- Relational patterns require types with ordering — numeric and enum cases common.
- Module 01 introduced basics; this cross-chapter covers C# 9–11 depth.

---

#### Q10. Explain property patterns (`person is { Age: > 18, Name: var n }`). {#cross-chapter-records-pattern-matching-c-911-grouped-here-q10}

Explain property patterns (`person is { Age: > 18, Name: var n }`).

**Answer:** Property patterns match an object by inspecting nested property values and optionally binding variables from matched members. The pattern succeeds when the runtime type exposes the named properties and each nested pattern matches.

- Combines type testing, comparison, and variable binding in one expression.
- `var n` in a property pattern captures `Name` when the outer pattern matches.
- Null checks often prefix: `person is { Age: > 18 }` fails on null without throwing.
- Useful in validation pipelines and API authorization rules expressed declaratively.

---

#### Q11. What are relational patterns (`>`, `<=`) and combinator patterns (`and`, `or`, `not`)? {#cross-chapter-records-pattern-matching-c-911-grouped-here-q11}

What are relational patterns (`>`, `<=`) and combinator patterns (`and`, `or`, `not`)?

**Answer:** Relational patterns compare a matched value to constants using `<`, `<=`, `>`, `>=` in pattern positions, while combinator patterns join subpatterns with `and`, `or`, and `not` for boolean structure without nested ifs. They require compatible ordered types.

- Example: `x is > 0 and < 100` replaces range checks.
- `or` matches alternatives: `c is 'a' or 'e' or 'i'`.
- `not` negates a subpattern: `x is not null`.
- Compiler warnings highlight non-exhaustive combinations when enums omit cases.

---

#### Q12. What is list patterns (C# 11) — `[_, .., var last]`? {#cross-chapter-records-pattern-matching-c-911-grouped-here-q12}

What is list patterns (C# 11) — `[_, .., var last]`?

**Answer:** List patterns match sequences by structure — length, first/last elements, and slices — using syntax like `[head, .., tail]` on arrays, spans, and lists in pattern contexts. The discard `_` matches any element; `..` captures a slice subpattern.

- `[_, .., var last]` succeeds when at least two elements exist and binds `last`.
- Empty collection fails patterns requiring elements unless a separate `[]` arm exists.
- Enables concise parsing of command tokens and route segments.
- Combine with switch expressions for readable dispatch on string[] args.

---

#### Q13. What is `switch` expression vs traditional `switch` statement for exhaustiveness? {#cross-chapter-records-pattern-matching-c-911-grouped-here-q13}

What is `switch` expression vs traditional `switch` statement for exhaustiveness?

**Answer:** Switch expressions require every input to map to a resulting value with arms separated by `=>`, encouraging complete coverage of cases; traditional switch statements execute statements with fall-through controls and optional default without producing a value. Compiler exhaustiveness analysis is stronger on switch expressions over enums and tuples.

- Expression form: `var label = status switch { OrderStatus.New => "N", ... };`
- Statement form suits multi-step case bodies with local variables and loops.
- Non-exhaustive enum switch expressions warn when a case is missing — see Q14.
- Prefer expressions for mapping; statements for imperative case workflows.

---

#### Q14. What happens when a `switch` expression is not exhaustive over an enum? {#cross-chapter-records-pattern-matching-c-911-grouped-here-q14}

What happens when a `switch` expression is not exhaustive over an enum?

**Answer:** The compiler emits a warning or error (depending on analysis level) when an enum switch expression omits a member and no discard arm catches the remainder, because a new enum value could arrive at runtime and throw `SwitchExpressionException` at execution. Adding `_ => ...` or listing all members restores exhaustiveness.

- API evolution adding enum values breaks non-exhaustive switches at runtime first.
- Treat warnings seriously in CI — they predict production exceptions on new enum members.
- Default discard arm documents intentional catch-all behavior.
- String switches cannot be exhaustively proven — enums are the primary case.

---

#### Q15. What is the difference between `is null` and `== null` when a type overloads `==`? {#cross-chapter-records-pattern-matching-c-911-grouped-here-q15}

What is the difference between `is null` and `== null` when a type overloads `==`?

**Answer:** `is null` always performs a reference null check without invoking user-defined `==` overloads, while `== null` may call a static overloaded equality operator that could treat non-null instances as equal to null incorrectly. For nullable reference analysis, `is null` and `is not null` also integrate cleanly with flow tracking.

- Prefer `is null` / `is not null` for reference types with custom equality operators.
- `== null` remains common for value types and strings without surprising overloads.
- Pattern matching idioms (`if (x is not null)`) combine check and assignment.
- Unit tests should cover overloaded equality types explicitly.

---

#### Q16. What are expression trees (`Expression<T>`), and how do they differ from delegates? {#cross-chapter-records-pattern-matching-c-911-grouped-here-q16}

What are expression trees (`Expression<T>`), and how do they differ from delegates?

**Answer:** Expression trees represent code as data structures (`Expression` nodes) that can be inspected, transformed, and compiled at runtime, whereas delegates are compiled callable targets without preserved structure. `Expression<Func<T>>` stores the lambda body as a tree; `Func<T>` stores IL to invoke directly.

- LINQ providers translate trees to SQL or other remote query languages.
- Not every C# lambda form is translatable — see Q18.
- Compile a tree once with `.Compile()` to produce a delegate for local execution.
- Trees enable dynamic predicate builders in filtering APIs.

---

#### Q17. How are expression trees used by LINQ providers (EF Core, `IQueryable`)? {#cross-chapter-records-pattern-matching-c-911-grouped-here-q17}

How are expression trees used by LINQ providers (EF Core, `IQueryable`)?

**Answer:** `IQueryable` providers receive expression trees from LINQ query operators and translate member access, comparisons, and calls into provider-specific text such as SQL, executing remotely instead of in memory. The same lambda syntax compiles to a tree when the source is `IQueryable<T>` and to a delegate when the source is `IEnumerable<T>`.

- EF Core walks `Expression` nodes to build SQL with parameters — not arbitrary C# execution on the server.
- Method calls in trees must map to provider-supported functions or translations fail at runtime.
- `AsEnumerable()` switches to LINQ-to-Objects delegates — client evaluation boundary shifts.
- Debugging requires logging translated SQL, not assuming C# semantics off-process.

---

#### Q18. Why can't all C# lambdas be converted to expression trees? {#cross-chapter-records-pattern-matching-c-911-grouped-here-q18}

Why can't all C# lambdas be converted to expression trees?

**Answer:** Expression trees support only a subset of C# expressions — no statements blocks with loops, local functions, `ref` operations, or many statement-bodied patterns unless compiler can represent them as supported node types. Lambdas using unsupported constructs must compile to delegates only.

- See Gotcha 14 — EF queries fail when lambdas include unsupported calls.
- Statement-bodied lambdas with `{ ... }` often disqualify tree conversion.
- Null propagating operators and some null-forgiving forms have limited support depending on version.
- Use supported expression forms in `IQueryable` or fall back to `IEnumerable` client evaluation knowingly.

---

#### Q19. What is the difference between compile-time constant patterns and runtime type patterns? {#cross-chapter-records-pattern-matching-c-911-grouped-here-q19}

What is the difference between compile-time constant patterns and runtime type patterns?

**Answer:** Constant patterns match values known at compile time such as `case 0:` or `case "OK":`, while type patterns test runtime types and bind variables (`case Dog d:`). Constant patterns require compatible switch input type; type patterns interact with inheritance and casting rules at runtime.

- Enum cases use constant patterns with symbolic names.
- Type patterns may fail without throwing when used in `is` expressions.
- Mixing both in one switch is common in heterogeneous message dispatch.
- Switch input type determines which pattern forms are legal in each arm.

---

#### Q20. When should you prefer records over classes for DTOs and domain models? {#cross-chapter-records-pattern-matching-c-911-grouped-here-q20}

When should you prefer records over classes for DTOs and domain models?

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

---

## Scenario-Based Answers (Karat Format)

---

#### Q21. **`typeof` vs `GetType()`** — `typeof(Base)` is known at compile time; `instance.GetType()` returns the actual runtime derived type. {#cross-chapter-records-pattern-matching-c-911-grouped-here-q21}

_Answer not found._

---

#### Q22. **Serialization type loss** — Assigning `Animal ref = new Dog()` and serializing as `Animal` drops derived-only properties unless polymorphism is configured. {#cross-chapter-records-pattern-matching-c-911-grouped-here-q22}

_Answer not found._

---

#### Q23. **`[Serializable]` ignored by System.Text.Json** — Candidates conflate legacy binary markers with modern JSON/XML serializers. {#cross-chapter-records-pattern-matching-c-911-grouped-here-q23}

_Answer not found._

---

#### Q24. **`BinaryFormatter` is a security footgun** — Deserializing untrusted payloads enables remote code execution; obsolete/removed on modern .NET. {#cross-chapter-records-pattern-matching-c-911-grouped-here-q24}

_Answer not found._

---

#### Q25. **Missing JSON property on non-nullable value type** — Deserialization may default the value silently; missing `required`/`[JsonRequired]` validation causes subtle bugs. {#cross-chapter-records-pattern-matching-c-911-grouped-here-q25}

_Answer not found._

---

#### Q26. **Enum numeric wire values** — Renumbering enum members breaks persisted JSON; prefer string enums for long-lived contracts. {#cross-chapter-records-pattern-matching-c-911-grouped-here-q26}

_Answer not found._

---

#### Q27. **`JsonSerializerOptions` not thread-safe for mutation** — Cache a configured instance; do not tweak shared options concurrently. {#cross-chapter-records-pattern-matching-c-911-grouped-here-q27}

_Answer not found._

---

#### Q28. **`dynamic` hides errors until runtime** — Misspelled members compile; also blocks many refactorings and overload resolution surprises. {#cross-chapter-records-pattern-matching-c-911-grouped-here-q28}

_Answer not found._

---

#### Q29. **Extension methods do not dispatch on `dynamic`** — Must cast to static type or call like static methods. {#cross-chapter-records-pattern-matching-c-911-grouped-here-q29}

_Answer not found._

---

#### Q30. **Reflection string names don't refactor** — Renaming a property breaks reflection unless tests catch it. {#cross-chapter-records-pattern-matching-c-911-grouped-here-q30}

_Answer not found._

---

#### Q31. **Regex without timeout on user input** — Crafted input can hang the process via catastrophic backtracking. {#cross-chapter-records-pattern-matching-c-911-grouped-here-q31}

_Answer not found._

---

#### Q32. **Nullable reference types are annotations only** — `#nullable enable` does not stop null at runtime without guards. {#cross-chapter-records-pattern-matching-c-911-grouped-here-q32}

_Answer not found._

---

#### Q33. **Records are still reference types (`record class`)** — Identity semantics differ from `record struct`; boxing/equality surprises follow. {#cross-chapter-records-pattern-matching-c-911-grouped-here-q33}

_Answer not found._

---

#### Q34. **Expression trees cannot contain statements arbitrarily** — Many C# constructs are not translatable for EF/LINQ providers. {#cross-chapter-records-pattern-matching-c-911-grouped-here-q34}

_Answer not found._

---

### 03. Regular Expressions

#### Q1. `Regex.IsMatch(email, pattern)` inline in the action   {#03-regular-expressions-q1}

(R) A bulk-import API validates thousands of customer rows per request. After deploy, CPU spikes and some requests time out. Review this validator and prioritize fixes.

**Answer:** The validator re-parses regex patterns on every row via static helpers, accepts substring email matches because the pattern lacks anchors, and runs a nested-quantifier notes pattern with no `MatchTimeout` — together causing wasted CPU, false accepts, and potential ReDoS under adversarial notes text.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance | `Regex.IsMatch` / `Regex.Match` static calls inside per-row loop | Pattern re-parsed on every invocation — O(rows × parse cost) |
| Correctness | Email pattern missing `^` and `$` | `"junk alice@x.co more"` passes validation (substring match) |
| Runtime / security | `(order\s+\d+)+` nested quantifier with no timeout | Catastrophic backtracking on long notes → hung threads, CPU spikes |
| Design | Notes extraction mixed into boolean gate | Validation path does extra work even when email/phone already fail |

**Fix (priority order):**

1. Cache patterns in `static readonly Regex` fields (with `RegexOptions.Compiled | RegexOptions.CultureInvariant`) and call instance `.IsMatch` / `.Match`.
2. Anchor whole-field validation: `@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"` — matches **Program.cs** `EmailPattern`.
3. Add `TimeSpan` match timeout on the notes pattern (or use `RegexOptions.NonBacktracking` in .NET 7+ for untrusted text).
4. Rewrite the notes pattern without nested greedy quantifiers — e.g. `@"order\s+\d+"` with `Matches` when you need every hit.
5. Short-circuit: validate email and phone before scanning notes.

```csharp
private static readonly Regex EmailPattern = new(
    @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
    RegexOptions.Compiled | RegexOptions.CultureInvariant,
    TimeSpan.FromMilliseconds(250));

private static readonly Regex OrderInNotes = new(
    @"order\s+\d+",
    RegexOptions.IgnoreCase | RegexOptions.Compiled,
    TimeSpan.FromMilliseconds(250));
```

**Production takeaway:** Bulk import turns "fine in dev" regex into a hot path — cache instances, anchor fields, and timeout untrusted text. See **Program.cs** Sections 3, 11, and 12.

---

#### Q2. `static readonly Regex` field with `RegexOptions.Compiled | RegexOptions.CultureInvariant`   {#03-regular-expressions-q2}

(R) A support portal lets agents paste a custom regex to search and redact matches in uploaded log files (multi-MB). Review this endpoint helper:

```csharp
public string RedactMatches(string logContent, string userPattern)
{
    var regex = new Regex(
        userPattern,
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    return regex.Replace(logContent, "[REDACTED]");
}
```

What production risks exist, and how would you harden this for untrusted input?

**Answer:** Accepting arbitrary regex from users against large inputs is a classic ReDoS vector — nested quantifiers can hang a thread indefinitely, and `Compiled` on every unique user pattern adds startup cost without helping one-off searches.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | User-supplied pattern with no timeout or engine guard | ReDoS — request thread blocked, CPU pegged, gateway timeouts |
| Security | No pattern length or complexity limits | Trivial denial-of-service via `(a+)+$`-style patterns on long logs |
| Performance | `RegexOptions.Compiled` per unique user pattern | IL emission cost on every distinct pattern; memory growth if patterns vary |
| Correctness | No validation that pattern is well-formed before `Replace` | `ArgumentException` bubbles as 500; partial redaction on invalid `$` tokens |
| Design | Regex redaction on multi-MB strings in-request | Large LOH allocations; latency spikes under concurrent uploads |

**Fix (priority order):**

1. Reject or sandbox user patterns — max length, allow-list of constructs, or disallow user regex entirely (fixed internal patterns + literal search via `Regex.Escape`).
2. Always pass `TimeSpan` match timeout: `new Regex(userPattern, options, TimeSpan.FromSeconds(2))` — catch `RegexMatchTimeoutException` and return 400.
3. Prefer `RegexOptions.NonBacktracking` (.NET 7+) for untrusted patterns — linear-time engine trades some feature support for safety.
4. Drop `Compiled` for ad hoc user patterns; cache only frequently reused admin-defined patterns in a bounded dictionary.
5. Process large logs in chunks or offload to a background worker with cancellation.

```csharp
var safe = new Regex(
    userPattern,
    RegexOptions.IgnoreCase | RegexOptions.NonBacktracking,
    TimeSpan.FromSeconds(2));
```

**Production takeaway:** Never run untrusted regex on untrusted input without timeout or NonBacktracking — Karat uses this to test ReDoS awareness, not pattern syntax recall. See **Program.cs** Sections 10–11 and `DemonstratePerformance` timeout demo.

---

#### Q3. `[RegularExpression(@"…")]` on the DTO property   {#03-regular-expressions-q3}

(R) A notes-processing job extracts phone fragments for a CRM sync. Review this extractor:

```csharp
public sealed class PhoneExtractor
{
    private static readonly Regex PhonePattern = new(
        @"(\+1[-.\s]?)?(\([0-9]{3}\)|[0-9]{3})[-.\s]?[0-9]{3}[-.\s]?[0-9]{4}",
        RegexOptions.Compiled);

    public string GetAreaCode(string notes)
    {
        Match m = PhonePattern.Match(notes);
        return m.Groups[2].Value; // area-code capture
    }

    public bool HasUsPhone(string notes) =>
        Regex.IsMatch(notes, PhonePattern.ToString());
}
```

What correctness bugs appear on edge-case inputs, and how do you fix them?

**Answer:** `GetAreaCode` reads `Groups[2]` without checking `Success`, returning empty strings silently when no phone exists; `HasUsPhone` round-trips the pattern through `.ToString()` into a static call that re-parses every time and still performs partial matching anywhere in the notes string.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | No `m.Success` check before `Groups[2]` | `"no phone here"` → `""` instead of explicit "not found" — CRM gets blank area codes |
| Correctness | Phone pattern not anchored with `^…$` | Matches embedded digit runs inside longer strings — false positives |
| Performance | `Regex.IsMatch(notes, PhonePattern.ToString())` | Re-parses pattern on every call; `.ToString()` is not a reliable reuse mechanism |
| Design | Group index `[2]` is magic number | Refactoring pattern breaks silently when optional country group shifts |

**Fix (priority order):**

1. Guard on `Success` before reading groups; return `null`, `Optional`, or throw a domain exception when no match.
2. Anchor for whole-field checks: prepend `^` and append `$` when validating a dedicated phone column; use unanchored instance only for extraction inside free text (document the difference).
3. Reuse the cached instance: `PhonePattern.IsMatch(notes)` instead of static + `ToString()`.
4. Prefer named groups `(?<area>…)` and read `Groups["area"].Value` for maintainability — matches **Program.cs** Section 6.

```csharp
public string? GetAreaCode(string notes)
{
    Match m = PhonePattern.Match(notes);
    if (!m.Success)
        return null;
    return m.Groups["area"].Success ? m.Groups["area"].Value : null;
}

public bool HasUsPhone(string notes) => PhonePattern.IsMatch(notes);
```

**Production takeaway:** `Groups[n]` without `Success` is a silent data bug — Karat stacks it with static-helper misuse. See **Program.cs** Section 12 pitfalls and Section 6 named groups.

---

## Scenario-Based Questions (Karat Format)

#### Q1. (R) A Redis-backed session service deserializes cached JSON on every request. Review this code — what breaks under load or attack, and what do you fix first?

```csharp
public sealed class SessionStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public UserSession? Load(string redisKey)
    {
        string json = _redis.GetString(redisKey)!;
        return JsonSerializer.Deserialize<UserSession>(json, Options);
    }
}

public sealed class UserSession
{
    public string UserId { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public List<UserSession> Nested { get; set; } = new();
}
```

---

**Answer:**

**Answer:** The deserializer accepts arbitrarily deep nested JSON with no depth guard, and the DTO puts `PasswordHash` on the wire — a combination that enables denial-of-service via deeply nested payloads and leaks credential material if Redis is ever read or replayed.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | No `MaxDepth` on `JsonSerializerOptions` | Malicious or corrupted JSON with deep `Nested` chains can cause stack overflow or excessive CPU |
| Security / design | `PasswordHash` has no `[JsonIgnore]` | Secrets round-trip in cache payloads; any Redis dump or log leak exposes hashes |
| Runtime | `WriteIndented = true` on a hot read path | Larger payloads, extra allocations — unnecessary for machine-to-machine cache |
| Correctness | Treating Redis bytes as trusted without schema validation | Tampered session JSON deserializes blindly into live request state |

**Fix (priority order):**

1. Set `MaxDepth` (e.g. 32–64) on shared options — matches **Program.cs** Section 5 / `BuildTutorialJsonOptions`.
2. Mark secrets and internal fields with `[JsonIgnore]` (see **Program.cs** Section 3 — `MemberProfile`).
3. Remove `WriteIndented` from production cache serialization; use compact JSON or `SerializeToUtf8Bytes` (**Section 8**).
4. Sign or encrypt session blobs, or store only opaque session ids server-side — never rehydrate security-sensitive graphs from untrusted storage without validation.
5. Register one shared `JsonSerializerOptions` instance (or `IOptions<JsonSerializerOptions>`) instead of ad hoc static copies that drift from ASP.NET defaults.

**Production takeaway:** System.Text.Json is safe by default only when you set depth limits and keep secrets off DTOs — Karat stacks a DoS vector with a data-leak field in one snippet. See **Program.cs** pitfall note on untrusted JSON without `MaxDepth`.

---

---

#### Q2. (R) After deploying a new order endpoint, mobile clients get `400`/`500` on deserialize while Postman with the old payload works. Review the handler:

```csharp
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

app.MapPost("/orders", (HttpRequest req) =>
{
    using var reader = new StreamReader(req.Body);
    string body = reader.ReadToEndAsync().Result;

    var dto = JsonSerializer.Deserialize<OrderDto>(body, new JsonSerializerOptions());
    return Results.Ok(dto);
});

public record OrderDto(
    [property: JsonPropertyName("order_id")] int OrderId,
    OrderStatus Status,
    string CustomerName);

public enum OrderStatus { Pending, Shipped, Cancelled }
```

Client JSON: `{ "orderId": 42, "status": "Shipped", "customerName": "Acme" }`

---

**Answer:**

```csharp
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

app.MapPost("/orders", (HttpRequest req) =>
{
    using var reader = new StreamReader(req.Body);
    string body = reader.ReadToEndAsync().Result;

    var dto = JsonSerializer.Deserialize<OrderDto>(body, new JsonSerializerOptions());
    return Results.Ok(dto);
});

public record OrderDto(
    [property: JsonPropertyName("order_id")] int OrderId,
    OrderStatus Status,
    string CustomerName);

public enum OrderStatus { Pending, Shipped, Cancelled }
```

Client JSON: `{ "orderId": 42, "status": "Shipped", "customerName": "Acme" }`

**Answer:** The handler bypasses the configured HTTP JSON options by passing a fresh `new JsonSerializerOptions()`, so camelCase naming and the enum string converter never apply — `orderId` and `"Shipped"` fail against PascalCase property names and numeric enum defaults.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| API contract | `new JsonSerializerOptions()` ignores `ConfigureHttpJsonOptions` | Client camelCase JSON does not bind; `OrderId` stays 0 |
| API contract | No `JsonStringEnumConverter` on ad hoc options | `"Shipped"` throws `JsonException`; numeric `2` would work accidentally |
| Async | `.Result` on `ReadToEndAsync()` | Thread-pool blocking under load (sync-over-async) |
| Design | Manual body read instead of `[FromBody]` / minimal API binding | Duplicates framework behavior and drifts from global JSON policy |

**Fix (priority order):**

1. Use framework binding so global options apply: `app.MapPost("/orders", (OrderDto dto) => Results.Ok(dto));` or inject `JsonSerializerOptions` from `IOptions<JsonOptions>`.
2. If manual deserialize is required, pass the configured instance: `JsonSerializer.Deserialize<OrderDto>(body, app.Services.GetRequiredService<IOptions<JsonOptions>>().Value.SerializerOptions)`.
3. Align wire names: either rely on camelCase for `orderId` and drop redundant `[JsonPropertyName("order_id")]` unless the contract truly requires snake_case.
4. Replace `.Result` with `await reader.ReadToEndAsync()` inside an async delegate.
5. Add `[JsonConverter(typeof(JsonStringEnumConverter))]` on `OrderStatus` or register the converter globally (**Program.cs** Section 4).

**Production takeaway:** Karat tests whether you know configured JSON options are not automatic — every `Deserialize` call needs the same options object the API publishes. Mismatch looks like "works in Swagger, fails on mobile."

---

---

#### Q3. (R) A partner integration writes invoice lines to XML nightly; the job fails on first deploy with `InvalidOperationException`. Review the model and serializer usage:

```csharp
public sealed class InvoiceLine
{
    public InvoiceLine(int sku, decimal unitPrice)
    {
        Sku = sku;
        UnitPrice = unitPrice;
    }

    public int Sku { get; set; }
    public decimal UnitPrice { get; set; }
}

public static void ExportLines(IEnumerable<InvoiceLine> lines, Stream target)
{
    var serializer = new XmlSerializer(typeof(List<InvoiceLine>));
    serializer.Serialize(target, lines.ToList());
}
```

---

**Answer:**

```csharp
public sealed class InvoiceLine
{
    public InvoiceLine(int sku, decimal unitPrice)
    {
        Sku = sku;
        UnitPrice = unitPrice;
    }

    public int Sku { get; set; }
    public decimal UnitPrice { get; set; }
}

public static void ExportLines(IEnumerable<InvoiceLine> lines, Stream target)
{
    var serializer = new XmlSerializer(typeof(List<InvoiceLine>));
    serializer.Serialize(target, lines.ToList());
}
```

**Answer:** `XmlSerializer` requires a public parameterless constructor on every serialized type; `InvoiceLine` only exposes a parameterized ctor, so serializer construction or the first serialize call throws `InvalidOperationException`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | No public parameterless constructor on `InvoiceLine` | Job fails at startup or first export — **Program.cs** Section 2 / Section 10 table |
| Design | Serializing `List<InvoiceLine>` directly | No named root element or metadata; partner may require a wrapper document (compare `AlumniRoster` in **Section 6**) |
| Performance | `new XmlSerializer(typeof(...))` per call in `ExportLines` | Repeated reflection; reuse one cached serializer per type in production (**Section 9** comment) |

**Fix (priority order):**

1. Add a public parameterless constructor: `public InvoiceLine() { }` (can be alongside the parameterized ctor).
2. Wrap lines in a root DTO with `[XmlRoot]` / `[XmlElement("Line")]` when the partner schema expects a document envelope.
3. Cache `XmlSerializer` instances statically per type — construction reflects over properties once.
4. Add an integration test that round-trips sample lines through serialize/deserialize before the nightly job ships.

**Production takeaway:** XmlSerializer failures are runtime, not compile-time — Karat expects you to know the parameterless ctor rule from **Program.cs** Section 2 and the exception table in Section 10.

---

---

#### Q4. (P) Production still reads `.bin` session files produced years ago with `BinaryFormatter`. You must migrate to System.Text.Json without taking downtime. What is your rollout strategy, and why is "just flip a switch" unsafe?

---

**Answer:**

**Answer:** Deserializing legacy `.bin` files with `BinaryFormatter` on untrusted or stale blobs is a remote-code-execution risk (gadget chains); migration must read old format only in a controlled worker, write new JSON, and dual-read during cutover — never re-enable the compatibility switch on public-facing paths.

- **Phase 1 — freeze writes:** Stop creating new `BinaryFormatter` blobs; new sessions write JSON to a parallel key/path (`session.json` or versioned Redis key).
- **Phase 2 — offline migration worker:** Background job reads each `.bin` in an isolated process with minimal privileges, deserializes once with `BinaryFormatter` (suppressed obsolete warning only here), maps to a versioned DTO, and writes `System.Text.Json` UTF-8 output. Treat every input file as hostile — validate shape, size cap, no arbitrary types.
- **Phase 3 — dual-read in app:** `LoadSession` tries JSON first, falls back to `.bin` once, rewrites JSON on successful legacy read (read-repair), logs metric for remaining legacy count.
- **Phase 4 — retire binary path:** When legacy count hits zero, remove fallback and delete `.bin` artifacts.
- **Why not flip a switch:** `BinaryFormatter` is obsolete (SYSLIB0011) and disabled by default in .NET 8 (**Program.cs** Section 11). Enabling it app-wide restores an unsafe deserializer on any code path that touches binary. JSON migration also forces an explicit contract instead of opaque .NET-only graphs.

**Production takeaway:** Karat wants the security story (untrusted deserialization) plus a practical strangler migration — not "use JSON because it's newer." See **Program.cs** Section 11–12 replacement table.

---

---

#### Q5. (D) You are adding an optional `IsVerified` flag to a public REST DTO. v1 clients never send the field; v2 clients may send `true`, `false`, or omit it. You need to distinguish "not verified yet" from "explicitly false." Should the property be `bool` or `bool?`, and how does System.Text.Json treat a missing member for each?

---

**Answer:**

**Answer:** Use `bool?` (`Nullable<bool>`) so three states are representable: missing → `null` ("unknown / not provided"), `false` → explicitly not verified, `true` → verified. Plain `bool` collapses missing and false into `false`, losing tri-state semantics.

- **Missing JSON property + `bool IsVerified`:** deserializes to `false` (default for value types) — indistinguishable from `"isVerified": false` (**Program.cs** Section 6 versioning table).
- **Missing JSON property + `bool? IsVerified`:** deserializes to `null` — signals "client did not send the field" (v1 backward compatible).
- **Extra / forward compatibility:** unknown future properties are ignored by default; adding nullable optional fields is the recommended evolution path (**Section 6**).
- **Write behavior:** combine with `DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull` if you want v2 servers to omit `null` on output and keep payloads small (**Section 5** options table).
- **API docs:** document the three states explicitly in OpenAPI (`nullable: true`) so clients do not assume false means "failed verification."

**Production takeaway:** Karat uses JSON versioning to test whether you reach for `bool?` instead of defaulting everything to false — same lesson as optional flags in ASP.NET model binding, applied to DTO evolution.

---

---

#### Q6. (M) An org-chart API returns departments with parent/child links wired both ways. A developer enables cycle handling and ships:

```csharp
var options = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    ReferenceHandler = ReferenceHandler.IgnoreCycles
};

string json = JsonSerializer.Serialize(rootDepartment, options);
return Results.Content(json, "application/json");
```

Sample response excerpt: `"parent": null` on a child that definitely has a parent in memory. A mobile client renders the tree incorrectly. What happened, and when would you choose `ReferenceHandler.Preserve` instead?

---

**Answer:**

```csharp
var options = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    ReferenceHandler = ReferenceHandler.IgnoreCycles
};

string json = JsonSerializer.Serialize(rootDepartment, options);
return Results.Content(json, "application/json");
```

Sample response excerpt: `"parent": null` on a child that definitely has a parent in memory. A mobile client renders the tree incorrectly. What happened, and when would you choose `ReferenceHandler.Preserve` instead?

**Answer:** Parent/child references form a cycle (`root → child → root`). `ReferenceHandler.IgnoreCycles` breaks the cycle by writing `null` on the second visit to an already-serialized object — so the child's `parent` becomes `null` even though the in-memory graph is fully wired (**Program.cs** Section 7).

- **What happened:** Serializer visited `child` through `root.Children`, then hit `child.Parent` pointing back to `root`, detected the cycle, and emitted `null` instead of duplicating `root` — correct for cycle breaking, wrong for clients expecting a bidirectional graph.
- **Client impact:** Tree builders that walk `parent` pointers get a disconnected node; only the downward `children` array is reliable.
- **Fix options (pick by contract):**
  - **DTO projection:** Return a read model without back-pointers — e.g. flat list or nested children only (no `Parent` on wire).
  - **`ReferenceHandler.Preserve`:** Emits `$id` / `$ref` metadata so full graphs round-trip; clients must understand JSON Reference semantics — heavier payload, needed for true graph round-trip or patch workflows.
  - **IgnoreCycles:** Acceptable for logging or snapshots where parent nulls are harmless — not for UI trees that traverse both directions.
- **Default without handler:** Serialization throws `JsonException` on first cycle — fails fast but does not silently corrupt data.

**Production takeaway:** Karat tests mechanism, not definition — `IgnoreCycles` silently changes meaning. Know when to reshape the DTO vs when to pay for `$ref` preservation (**Program.cs** Section 7).

---

---

#### Q7. (R) A config-sync worker reads JSON settings files from a shared folder (any authenticated internal user can drop files). Review the ingestion path:

```csharp
public AppSettings LoadSettings(string path)
{
    string json = File.ReadAllText(path);
    var settings = JsonSerializer.Deserialize<AppSettings>(json);
    _cache.Set("app-settings", settings);
    return settings;
}

public sealed class AppSettings
{
    public string ApiBaseUrl { get; set; } = string.Empty;
    public int RetryCount { get; set; } = 3;
    public Dictionary<string, object> FeatureFlags { get; set; } = new();
}
```

The team added `FeatureFlags` to support dynamic toggles. What runtime and security issues appear when arbitrary JSON lands in this path?

---

### 02. Reflection & Attributes

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/08. Advanced C# Features/02. Reflection & Attributes/`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

```csharp
public AppSettings LoadSettings(string path)
{
    string json = File.ReadAllText(path);
    var settings = JsonSerializer.Deserialize<AppSettings>(json);
    _cache.Set("app-settings", settings);
    return settings;
}

public sealed class AppSettings
{
    public string ApiBaseUrl { get; set; } = string.Empty;
    public int RetryCount { get; set; } = 3;
    public Dictionary<string, object> FeatureFlags { get; set; } = new();
}
```

The team added `FeatureFlags` to support dynamic toggles. What runtime and security issues appear when arbitrary JSON lands in this path?

**Answer:** Default `JsonSerializer.Deserialize` uses no depth limit, and `Dictionary<string, object>` deserializes JSON values into `JsonElement` boxes with unpredictable shapes — a writable shared folder makes this an untrusted-input path vulnerable to depth bombs, type confusion, and config tampering (e.g. repointing `ApiBaseUrl`).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | Untrusted JSON with default options (no `MaxDepth`) | Deeply nested documents can DoS the worker (**Program.cs** Section 5 pitfall) |
| Security | Writable config drop folder | Attacker or misclick can redirect `ApiBaseUrl` to a hostile endpoint — SSRF / credential theft on next outbound call |
| Runtime | `Dictionary<string, object>` for `FeatureFlags` | Values deserialize as `JsonElement`; consumer code casting to `bool`/`string` throws or misbehaves at runtime |
| Design | No schema validation or versioning | Extra keys silently ignored; missing required fields default (`RetryCount` → 0 if omitted) — silent misconfiguration |
| Ops | Cached poisoned settings in `_cache` | Bad file propagates until manual cache flush — long blast radius |

**Fix (priority order):**

1. Treat input as untrusted: set `MaxDepth`, max file size, and reject unknown properties if the schema is fixed (`JsonSerializerOptions.UnmappedMemberHandling = Skip` is default; use strict mode when appropriate).
2. Replace `Dictionary<string, object>` with `Dictionary<string, bool>` or a strongly typed flags record; use `JsonNode` only for truly dynamic probes (**Program.cs** Section 8c).
3. Validate after deserialize: absolute URI check on `ApiBaseUrl`, allowed host allowlist, sensible bounds on `RetryCount`.
4. Load from secured storage (signed blob, configuration provider, vault) — not a world-writable share; verify signature before apply.
5. Use a shared, named `JsonSerializerOptions` instance registered at startup — same policy as HTTP JSON.

**Production takeaway:** Serialization security is not only BinaryFormatter — any deserializer fed untrusted bytes needs depth limits, typed models, and validation. Karat stacks writable path + loose dictionary + default options into one review scenario.

---

### 02. Reflection & Attributes

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/08. Advanced C# Features/02. Reflection & Attributes/`

---

---

#### Q1. (R) A warehouse API loads pricing plugins from a separate assembly at runtime. It works on a developer machine but `LoadPlugin` always returns null in staging. Review the loader:

```csharp
public sealed class PluginLoader
{
    public object? LoadPlugin(string typeName)
    {
        Type? pluginType = Type.GetType(typeName); // e.g. "Acme.Pricing.VolumeDiscountPlugin"
        if (pluginType is null)
        {
            return null;
        }

        return Activator.CreateInstance(pluginType);
    }
}

// Startup config (appsettings):
// "Plugins:PricingType": "Acme.Pricing.VolumeDiscountPlugin"
```

What fails cross-assembly, and how do you fix type resolution for production plugin loading?

---

**Answer:**

```csharp
public sealed class PluginLoader
{
    public object? LoadPlugin(string typeName)
    {
        Type? pluginType = Type.GetType(typeName); // e.g. "Acme.Pricing.VolumeDiscountPlugin"
        if (pluginType is null)
        {
            return null;
        }

        return Activator.CreateInstance(pluginType);
    }
}

// Startup config (appsettings):
// "Plugins:PricingType": "Acme.Pricing.VolumeDiscountPlugin"
```

What fails cross-assembly, and how do you fix type resolution for production plugin loading?

**Answer:** `Type.GetType(string)` resolves types in the **calling assembly** and mscorlib by default — not arbitrary referenced or dynamically loaded assemblies. A namespace-qualified name without an assembly qualifier returns null when the plugin lives in `Acme.Pricing.dll`, which looks like "works locally" only if everything is inlined in one project.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | `Type.GetType` without assembly-qualified name | Returns null for types in other assemblies — plugin silently skipped |
| Design | No assembly load step before type lookup | Plugin DLL never loaded into the AppDomain |
| Correctness | `Activator.CreateInstance` on resolved type may still fail | Parameterless ctor required; wrong ctor throws `MissingMethodException` |

**Fix (priority order):**

1. Load the plugin assembly explicitly: `Assembly.LoadFrom(path)` or `AssemblyLoadContext.LoadFromAssemblyPath`, then `assembly.GetType(typeName)`.
2. Or pass an assembly-qualified name in config: `"Acme.Pricing.VolumeDiscountPlugin, Acme.Pricing, Version=…, Culture=neutral, PublicKeyToken=…"`.
3. Fail fast when resolution returns null — log config value and searched assemblies instead of returning null quietly.
4. Validate plugin types implement a known interface (`IPricingPlugin`) before `CreateInstance`; guard null and ctor requirements.

**Production takeaway:** Cross-assembly `Type.GetType` is a classic "works in monolith, fails when split" trap — same lesson as **Program.cs** Section 6b: assembly-qualified names are required for external types.

---

---

#### Q2. (R) A metadata-driven audit interceptor invokes controller actions and logs failures, but operators only see `TargetInvocationException` in Splunk — never the real fault. Review the handler:

```csharp
public sealed class AuditInterceptor
{
    public object? InvokeWithAudit(object target, MethodInfo method, object?[] args)
    {
        AuditActionAttribute? audit = method.GetCustomAttribute<AuditActionAttribute>();
        _logger.LogInformation("Before {Action}", audit?.Action ?? method.Name);

        try
        {
            return method.Invoke(target, args);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Audit invoke failed for {Method}", method.Name);
            throw;
        }
    }
}
```

What is wrong with exception handling here, and what should be logged and rethrown?

---

**Answer:**

```csharp
public sealed class AuditInterceptor
{
    public object? InvokeWithAudit(object target, MethodInfo method, object?[] args)
    {
        AuditActionAttribute? audit = method.GetCustomAttribute<AuditActionAttribute>();
        _logger.LogInformation("Before {Action}", audit?.Action ?? method.Name);

        try
        {
            return method.Invoke(target, args);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Audit invoke failed for {Method}", method.Name);
            throw;
        }
    }
}
```

What is wrong with exception handling here, and what should be logged and rethrown?

**Answer:** Exceptions thrown **inside** the invoked method are wrapped in `TargetInvocationException`. Logging and rethrowing the wrapper hides the real fault (`InvalidOperationException`, `ArgumentException`, etc.) from operators and any upstream handler that keys off exception type.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | Logs outer `TargetInvocationException` only | Splunk shows wrapper message/stack — root cause buried in `.InnerException` |
| Observability | `throw;` rethrows wrapper unchanged | Global exception middleware may classify wrong HTTP status |
| Maintainability | No unwrap before log/rethrow | On-call cannot triage from alert text alone |

**Fix (priority order):**

1. Catch `TargetInvocationException` specifically and unwrap: `var inner = tie.InnerException ?? tie;`
2. Log `inner` (message + stack), not the wrapper — include `method.Name` and audit action metadata.
3. Rethrow `inner` with `ExceptionDispatchInfo.Capture(inner).Throw()` if you must preserve original stack without `throw ex` corruption.
4. Guard `method` for null before `Invoke` — `GetMethod` typos cause `NullReferenceException` before any wrapper is involved.

```csharp
catch (TargetInvocationException tie)
{
    Exception actual = tie.InnerException ?? tie;
    _logger.LogError(actual, "Audit invoke failed for {Method}", method.Name);
    ExceptionDispatchInfo.Capture(actual).Throw();
}
```

**Production takeaway:** `MethodInfo.Invoke` always wraps callee faults — production code must unwrap before logging and API error mapping. See **Program.cs** Section 6e and QUICK REFERENCE — TargetInvocationException.

---

---

#### Q3. (R) After a rename refactor from `CalculateLineTotal` to `CalculateOrderTotal`, order totals silently become zero in production. Review the pricing service:

```csharp
public decimal GetLineTotal(Product product, int quantity)
{
    Type type = product.GetType();
    MethodInfo? method = type.GetMethod("CalculateLineTotal"); // string name — not nameof

    object? result = method.Invoke(product, new object[] { quantity });
    return result is decimal total ? total : 0m;
}
```

Identify the stacked problems (compile-time, runtime, maintainability) and prioritize fixes.

---

**Answer:**

```csharp
public decimal GetLineTotal(Product product, int quantity)
{
    Type type = product.GetType();
    MethodInfo? method = type.GetMethod("CalculateLineTotal"); // string name — not nameof

    object? result = method.Invoke(product, new object[] { quantity });
    return result is decimal total ? total : 0m;
}
```

Identify the stacked problems (compile-time, runtime, maintainability) and prioritize fixes.

**Answer:** The method was renamed but the string literal was not updated — `GetMethod` returns null, and the code invokes without a null check, which throws at runtime (or would if the fallback `0m` masked a null invoke in a sloppier variant). Stringly-typed reflection bypasses the compiler's rename refactor.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Maintainability | Hard-coded `"CalculateLineTotal"` | Rename refactor does not update call sites — silent wrong behavior or NRE |
| Runtime | No null guard on `method` before `Invoke` | `NullReferenceException` in production when name drifts |
| API contract | `GetMethod(name)` without parameter types | Ambiguous if overloads exist — wrong overload or null |
| Design | Reflection for a known compile-time type | Unnecessary indirection vs direct `product.CalculateOrderTotal(quantity)` |

**Fix (priority order):**

1. Replace string with `nameof(Product.CalculateOrderTotal)` and pass parameter types: `GetMethod(nameof(...), new[] { typeof(int) })`.
2. Guard null and fail loudly (throw `InvalidOperationException`) instead of returning `0m` — silent zero corrupts billing.
3. Prefer direct call when type is known at compile time; reserve reflection for plugin/unknown-type scenarios.
4. Add a unit test that asserts invoke succeeds after renames — or eliminate reflection on this path entirely.

**Production takeaway:** Reflection + magic strings defeats IDE refactor — Karat stacks rename drift with missing null guards. See **Program.cs** Section 6e pitfall table and Section 6i — "Rename refactor breaks silently."

---

---

#### Q4. (R) A margin-report job reads private cost fields from `CostRecord`-like DTOs but always gets null and skips rows. Review the extractor:

```csharp
public decimal? ReadCostBasis(object record)
{
    Type type = record.GetType();
    FieldInfo? field = type.GetField("_costBasis"); // default binding — public only

    if (field is null)
    {
        return null;
    }

    return (decimal?)field.GetValue(record);
}
```

What binding mistake causes this, and what flags are required?

---

**Answer:**

```csharp
public decimal? ReadCostBasis(object record)
{
    Type type = record.GetType();
    FieldInfo? field = type.GetField("_costBasis"); // default binding — public only

    if (field is null)
    {
        return null;
    }

    return (decimal?)field.GetValue(record);
}
```

What binding mistake causes this, and what flags are required?

**Answer:** `GetField` without `BindingFlags` uses default binding, which returns **public** instance/static members only. `_costBasis` is a private instance field — lookup returns null, the method exits early, and rows are skipped.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | Default binding omits non-public members | Private field never found — report data missing |
| Correctness | `(decimal?)field.GetValue(record)` on boxed value | Unboxing via nullable cast can throw if type mismatches |
| Design | Reading private fields from outside the type | Breaks encapsulation — prefer a public/internal accessor for reporting |

**Fix (priority order):**

1. Pass explicit flags matching **Program.cs** Section 6d: `BindingFlags.Instance | BindingFlags.NonPublic` (add `Public` if you want either).
2. Guard null and log type name + field name when lookup fails — distinguish "wrong type" from "wrong flags."
3. Prefer exposing `CostBasis` via an internal property or `IOffsetCostReadable` interface for the report job instead of reaching into private fields.
4. Cache `FieldInfo` per `Type` in a static readonly dictionary if this runs in a batch loop.

```csharp
const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
FieldInfo? field = type.GetField("_costBasis", flags);
```

**Production takeaway:** Default reflection binding is the first thing to check when "member not found" — same specimen as `CostRecord` in **Program.cs** Section 3 and 6d.

---

---

#### Q5. (P) An ASP.NET Core API discovers `[EntityTable]`-decorated export types by scanning `Assembly.GetExecutingAssembly().GetTypes()` at startup. After enabling `<PublishTrimmed>true</PublishTrimmed>` for a Native AOT experiment, several entity types vanish from the export manifest with no compile errors. Why does trimming break this pattern, and what production-safe alternatives exist?

---

**Answer:**

**Answer:** The trimmer removes types and members it cannot prove are used at compile time. Startup reflection that scans assemblies and reads custom attributes is invisible to static analysis — entity types with no direct references are linked out, so `GetTypes()` returns a smaller set and attribute-driven discovery silently drops models.

- **Why no compile error:** Trimming is a link-time optimization; reflection targets are not required call sites the compiler tracks.
- **Mitigations:** Annotate roots with `[DynamicallyAccessedMembers]` / `DynamicallyAccessedMemberTypes` on APIs that accept `Type`; use a trimmer descriptor file (`TrimmerRootAssembly` / `TrimmerRootDescriptor`) to preserve entity assemblies; register known export types explicitly in DI instead of full-assembly scan.
- **Long-term:** Replace scan-all-reflection with **source generators** that emit export manifests or EF-style mappings at compile time — same metadata (`[EntityTable]`, `[Exportable]`), no runtime graph walk.
- **Handle `ReflectionTypeLoadException`:** When dependencies are trimmed or missing, `GetTypes()` can throw — catch and log `LoaderExceptions` (see **Program.cs** Section 6b).

**Production takeaway:** Attribute-driven discovery copied from EF/ASP.NET patterns fails under Native AOT and aggressive publish trimming unless you root types or generate code — preview in **Program.cs** Section 6i AOT note.

---

---

#### Q6. (D) Your team ships a CSV export endpoint. One developer scans every request with `type.GetProperties()` and `GetCustomAttribute<ExportableAttribute>()` (same pattern as `ExportManifestBuilder` in this chapter). Another caches `PropertyInfo[]` and attribute metadata in a `ConcurrentDictionary<Type, ExportColumn[]>` built once at startup. Under 500 RPS with 40 exportable properties per row, which approach do you choose and why?

---

**Answer:**

**Answer:** Cache metadata at startup (or first use per `Type`) and only call `GetValue` per instance per request — reflection on `Type` and `MemberInfo` is orders of magnitude more expensive than reading pre-resolved columns from a cached `ExportColumn[]`.

- **Per-request scan:** 500 × 40 property walks × attribute lookups allocates and hits internal reflection caches repeatedly — CPU spikes, GC pressure, latency tail grows under load.
- **Cached manifest:** Startup (or lazy) build mirrors **ExportManifestBuilder** logic once per exportable type; hot path is `foreach (col in manifest) col.Getter(instance)` — optionally compile delegates with `CreateGetter` for value types to avoid boxing.
- **Thread safety:** `ConcurrentDictionary<Type, ExportColumn[]>` is safe for lazy initialization; manifest is immutable after build — no lock on read path.
- **Invalidation:** If types are loaded dynamically (plugins), register manifest on plugin load; static domain models rarely need refresh.
- **Trade-off:** Cached approach uses slightly more memory for delegate/metadata tables — acceptable vs per-request CPU at 500 RPS.

**Production takeaway:** Metadata-driven export is a framework pattern — scan once, read many — not scan per row. See **Program.cs** Section 5 and Section 6i — "Uncached reflection in hot paths."

---

---

#### Q7. (M) A `PremiumProduct : Product` subclass is added for a loyalty tier. The ORM layer reads `[EntityTable]` from the base `Product` type to resolve table names. Export works for `Product` but `PremiumProduct` rows fail with "table not mapped." Given this attribute definition from the tutorial:

```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class EntityTableAttribute : Attribute { /* TableName */ }
```

Why does inheritance behave this way, and what are two correct fixes at the call site or attribute definition?

---

### 03. Regular Expressions

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/08. Advanced C# Features/03. Regular Expressions/`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class EntityTableAttribute : Attribute { /* TableName */ }
```

Why does inheritance behave this way, and what are two correct fixes at the call site or attribute definition?

**Answer:** `Inherited = false` on `EntityTableAttribute` means the attribute is stored only on `Product` — it is **not** visible when you call `typeof(PremiumProduct).GetCustomAttribute<EntityTableAttribute>()`. The ORM resolves the runtime type of each instance, sees no attribute on the subclass, and reports "table not mapped."

- **Why designed this way:** Table-per-type hierarchies often need different tables per concrete class — inheriting `[EntityTable("Products")]` onto every subclass would be wrong when `PremiumProduct` maps to `PremiumProducts`.
- **Fix 1 (call site):** Walk the inheritance chain: check `type.GetCustomAttribute<EntityTableAttribute>(inherit: true)` is insufficient when `Inherited = false`; instead loop `type = type.BaseType` until you find the attribute, or map `PremiumProduct` explicitly in a type registry.
- **Fix 2 (attribute):** If all subclasses share one table, set `Inherited = true` on `EntityTableAttribute` and apply only on the base — then `GetCustomAttribute` on derived types returns the base metadata (verify this matches your schema).
- **Fix 3 (explicit):** Add `[EntityTable("PremiumProducts")]` on `PremiumProduct` — correct when the subclass has its own table regardless of inheritance flags.

**Production takeaway:** Attribute inheritance is opt-in via `AttributeUsage.Inherited` — framework code must not assume subclass metadata mirrors the base. Contrast with `[DisplayLabel]` in **Program.cs** Section 1 (`Inherited = true`) vs `[EntityTable]` (`Inherited = false`).

---

### 03. Regular Expressions

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/08. Advanced C# Features/03. Regular Expressions/`

---

---

#### Q1. (R) A bulk-import API validates thousands of customer rows per request. After deploy, CPU spikes and some requests time out. Review this validator and prioritize fixes.

```csharp
public sealed class ImportRowValidator
{
    public bool IsValid(string email, string phone, string notes)
    {
        const string emailPat = @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}";
        const string phonePat = @"^(\+1[-.\s]?)?(\([0-9]{3}\)|[0-9]{3})[-.\s]?[0-9]{3}[-.\s]?[0-9]{4}$";

        if (!Regex.IsMatch(email, emailPat))
            return false;
        if (!Regex.IsMatch(phone, phonePat))
            return false;

        // Extract embedded order id from free-text notes
        var orderMatch = Regex.Match(notes, @"(order\s+\d+)+");
        return orderMatch.Success;
    }
}
```

---

**Answer:**

**Answer:** The validator re-parses regex patterns on every row via static helpers, accepts substring email matches because the pattern lacks anchors, and runs a nested-quantifier notes pattern with no `MatchTimeout` — together causing wasted CPU, false accepts, and potential ReDoS under adversarial notes text.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance | `Regex.IsMatch` / `Regex.Match` static calls inside per-row loop | Pattern re-parsed on every invocation — O(rows × parse cost) |
| Correctness | Email pattern missing `^` and `$` | `"junk alice@x.co more"` passes validation (substring match) |
| Runtime / security | `(order\s+\d+)+` nested quantifier with no timeout | Catastrophic backtracking on long notes → hung threads, CPU spikes |
| Design | Notes extraction mixed into boolean gate | Validation path does extra work even when email/phone already fail |

**Fix (priority order):**

1. Cache patterns in `static readonly Regex` fields (with `RegexOptions.Compiled | RegexOptions.CultureInvariant`) and call instance `.IsMatch` / `.Match`.
2. Anchor whole-field validation: `@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"` — matches **Program.cs** `EmailPattern`.
3. Add `TimeSpan` match timeout on the notes pattern (or use `RegexOptions.NonBacktracking` in .NET 7+ for untrusted text).
4. Rewrite the notes pattern without nested greedy quantifiers — e.g. `@"order\s+\d+"` with `Matches` when you need every hit.
5. Short-circuit: validate email and phone before scanning notes.

```csharp
private static readonly Regex EmailPattern = new(
    @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
    RegexOptions.Compiled | RegexOptions.CultureInvariant,
    TimeSpan.FromMilliseconds(250));

private static readonly Regex OrderInNotes = new(
    @"order\s+\d+",
    RegexOptions.IgnoreCase | RegexOptions.Compiled,
    TimeSpan.FromMilliseconds(250));
```

**Production takeaway:** Bulk import turns "fine in dev" regex into a hot path — cache instances, anchor fields, and timeout untrusted text. See **Program.cs** Sections 3, 11, and 12.

---

---

#### Q2. (R) A support portal lets agents paste a custom regex to search and redact matches in uploaded log files (multi-MB). Review this endpoint helper:

```csharp
public string RedactMatches(string logContent, string userPattern)
{
    var regex = new Regex(
        userPattern,
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    return regex.Replace(logContent, "[REDACTED]");
}
```

What production risks exist, and how would you harden this for untrusted input?

---

**Answer:**

```csharp
public string RedactMatches(string logContent, string userPattern)
{
    var regex = new Regex(
        userPattern,
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    return regex.Replace(logContent, "[REDACTED]");
}
```

What production risks exist, and how would you harden this for untrusted input?

**Answer:** Accepting arbitrary regex from users against large inputs is a classic ReDoS vector — nested quantifiers can hang a thread indefinitely, and `Compiled` on every unique user pattern adds startup cost without helping one-off searches.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | User-supplied pattern with no timeout or engine guard | ReDoS — request thread blocked, CPU pegged, gateway timeouts |
| Security | No pattern length or complexity limits | Trivial denial-of-service via `(a+)+$`-style patterns on long logs |
| Performance | `RegexOptions.Compiled` per unique user pattern | IL emission cost on every distinct pattern; memory growth if patterns vary |
| Correctness | No validation that pattern is well-formed before `Replace` | `ArgumentException` bubbles as 500; partial redaction on invalid `$` tokens |
| Design | Regex redaction on multi-MB strings in-request | Large LOH allocations; latency spikes under concurrent uploads |

**Fix (priority order):**

1. Reject or sandbox user patterns — max length, allow-list of constructs, or disallow user regex entirely (fixed internal patterns + literal search via `Regex.Escape`).
2. Always pass `TimeSpan` match timeout: `new Regex(userPattern, options, TimeSpan.FromSeconds(2))` — catch `RegexMatchTimeoutException` and return 400.
3. Prefer `RegexOptions.NonBacktracking` (.NET 7+) for untrusted patterns — linear-time engine trades some feature support for safety.
4. Drop `Compiled` for ad hoc user patterns; cache only frequently reused admin-defined patterns in a bounded dictionary.
5. Process large logs in chunks or offload to a background worker with cancellation.

```csharp
var safe = new Regex(
    userPattern,
    RegexOptions.IgnoreCase | RegexOptions.NonBacktracking,
    TimeSpan.FromSeconds(2));
```

**Production takeaway:** Never run untrusted regex on untrusted input without timeout or NonBacktracking — Karat uses this to test ReDoS awareness, not pattern syntax recall. See **Program.cs** Sections 10–11 and `DemonstratePerformance` timeout demo.

---

---

#### Q3. (R) A notes-processing job extracts phone fragments for a CRM sync. Review this extractor:

```csharp
public sealed class PhoneExtractor
{
    private static readonly Regex PhonePattern = new(
        @"(\+1[-.\s]?)?(\([0-9]{3}\)|[0-9]{3})[-.\s]?[0-9]{3}[-.\s]?[0-9]{4}",
        RegexOptions.Compiled);

    public string GetAreaCode(string notes)
    {
        Match m = PhonePattern.Match(notes);
        return m.Groups[2].Value; // area-code capture
    }

    public bool HasUsPhone(string notes) =>
        Regex.IsMatch(notes, PhonePattern.ToString());
}
```

What correctness bugs appear on edge-case inputs, and how do you fix them?

---

**Answer:**

```csharp
public sealed class PhoneExtractor
{
    private static readonly Regex PhonePattern = new(
        @"(\+1[-.\s]?)?(\([0-9]{3}\)|[0-9]{3})[-.\s]?[0-9]{3}[-.\s]?[0-9]{4}",
        RegexOptions.Compiled);

    public string GetAreaCode(string notes)
    {
        Match m = PhonePattern.Match(notes);
        return m.Groups[2].Value; // area-code capture
    }

    public bool HasUsPhone(string notes) =>
        Regex.IsMatch(notes, PhonePattern.ToString());
}
```

What correctness bugs appear on edge-case inputs, and how do you fix them?

**Answer:** `GetAreaCode` reads `Groups[2]` without checking `Success`, returning empty strings silently when no phone exists; `HasUsPhone` round-trips the pattern through `.ToString()` into a static call that re-parses every time and still performs partial matching anywhere in the notes string.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | No `m.Success` check before `Groups[2]` | `"no phone here"` → `""` instead of explicit "not found" — CRM gets blank area codes |
| Correctness | Phone pattern not anchored with `^…$` | Matches embedded digit runs inside longer strings — false positives |
| Performance | `Regex.IsMatch(notes, PhonePattern.ToString())` | Re-parses pattern on every call; `.ToString()` is not a reliable reuse mechanism |
| Design | Group index `[2]` is magic number | Refactoring pattern breaks silently when optional country group shifts |

**Fix (priority order):**

1. Guard on `Success` before reading groups; return `null`, `Optional`, or throw a domain exception when no match.
2. Anchor for whole-field checks: prepend `^` and append `$` when validating a dedicated phone column; use unanchored instance only for extraction inside free text (document the difference).
3. Reuse the cached instance: `PhonePattern.IsMatch(notes)` instead of static + `ToString()`.
4. Prefer named groups `(?<area>…)` and read `Groups["area"].Value` for maintainability — matches **Program.cs** Section 6.

```csharp
public string? GetAreaCode(string notes)
{
    Match m = PhonePattern.Match(notes);
    if (!m.Success)
        return null;
    return m.Groups["area"].Success ? m.Groups["area"].Value : null;
}

public bool HasUsPhone(string notes) => PhonePattern.IsMatch(notes);
```

**Production takeaway:** `Groups[n]` without `Success` is a silent data bug — Karat stacks it with static-helper misuse. See **Program.cs** Section 12 pitfalls and Section 6 named groups.

---

---

#### Q4. (P) Your registration API validates email on every POST (~2k RPS). A teammate proposes three options:

1. `Regex.IsMatch(email, pattern)` inline in the action  
2. `static readonly Regex` field with `RegexOptions.Compiled | RegexOptions.CultureInvariant`  
3. `[RegularExpression(@"…")]` on the DTO property  

When would you choose each, and what companion settings (timeout, anchoring, caching) are mandatory for the Regex-based approaches in production?

---

**Answer:**

1. `Regex.IsMatch(email, pattern)` inline in the action  
2. `static readonly Regex` field with `RegexOptions.Compiled | RegexOptions.CultureInvariant`  
3. `[RegularExpression(@"…")]` on the DTO property  

When would you choose each, and what companion settings (timeout, anchoring, caching) are mandatory for the Regex-based approaches in production?

**Answer:** At 2k RPS, inline static calls re-parse the pattern on every request and should be replaced with a cached compiled instance; the data-annotation attribute is fine for coarse API validation but still needs a well-anchored pattern and does not replace DNS or mailbox verification.

- **Option 1 — inline static:** Acceptable only for cold paths (admin tools, one-off scripts). On a hot registration endpoint it wastes CPU re-parsing the same automaton per call — replace with Option 2.
- **Option 2 — static readonly + Compiled:** Production default for hot regex validation. Pair with `^…$` anchors, `CultureInvariant`, and a constructor `TimeSpan` timeout (e.g. 200–500 ms) even for fixed patterns — defense in depth if the pattern is ever edited badly.
- **Option 3 — `[RegularExpression]`:** Good for declarative model validation in ASP.NET Core (`[ApiController]` runs it automatically). Same anchored pattern required; attribute does not add caching or timeout by itself — underlying implementation still constructs/runs regex per validation unless you also use a custom `ValidationAttribute` wrapping a shared instance.
- **Mandatory companions for any Regex approach:** whole-field anchors; treat regex as syntax-only (follow with uniqueness check, domain policy, or confirmation email); log `RegexMatchTimeoutException` as a potential attack signal.
- **Modern alternative (.NET 7+):** `[GeneratedRegex(@"^…$")]` partial method — compile-time generated, zero runtime parse, ideal for fixed hot patterns (previewed in **Program.cs** Section 11).

**Production takeaway:** Karat tests whether you distinguish "works in a demo" from "safe at 2k RPS" — caching, anchoring, and timeout matter as much as the pattern itself.

---

---

#### Q5. (D) Product wants import rejection for disposable email domains (`mailinator.com`, `tempmail.org`, …) and a regex that only allows corporate TLDs. A developer merges the blocklist into one giant pattern:

```csharp
bool ok = Regex.IsMatch(email,
    @"^(?!.*@(mailinator|tempmail)\.com$)[a-zA-Z0-9._%+-]+@(?:contoso|fabrikam)\.(?:com|org)$");
```

What breaks in maintainability, testability, and correctness compared to splitting validation layers? How would you structure this in a real import pipeline?

---

**Answer:**

```csharp
bool ok = Regex.IsMatch(email,
    @"^(?!.*@(mailinator|tempmail)\.com$)[a-zA-Z0-9._%+-]+@(?:contoso|fabrikam)\.(?:com|org)$");
```

What breaks in maintainability, testability, and correctness compared to splitting validation layers? How would you structure this in a real import pipeline?

**Answer:** One mega-pattern couples RFC-ish syntax, a disposable-domain policy, and an allow-list of employers into an unreadable string that is painful to unit test, unsafe to extend (every blocklist change recompiles regex), and still cannot verify that the mailbox exists or that the domain is typosquatted.

- **Maintainability:** Blocklists belong in configuration (`IOptions<EmailPolicyOptions>`) or a database table — not inside a pattern literal. Adding `tempmail.org` should be a config deploy, not a regex edit requiring code review of lookaheads.
- **Testability:** Layered validators get focused tests: syntax regex returns pass/fail; domain service checks blocklist and allow-list independently; integration tests compose them. A single regex forces table-driven tests with opaque expected strings.
- **Correctness:** Regex validates string shape only — it cannot detect disposable subdomains, plus-address aliases (`user+tag@contoso.com`), homoglyphs, or DNS MX existence. Negative lookahead `(?!.*@mailinator…)` is easy to get wrong and still matches `user@mailinator.com.evil.net` depending on anchoring.
- **Recommended pipeline:** (1) trim/normalize input; (2) anchored syntax regex or `MailAddress` parse for basic shape; (3) extract domain segment via `Match` named groups or string split; (4) blocklist/allow-list lookup service; (5) optional async MX/DNS check off the hot path; (6) business rules (duplicate account, region lock) in plain C#.
- **When regex fits:** syntax gate only — the same practical email pattern from **Program.cs** Section 9a, anchored with `^$`.

**Production takeaway:** Regex is a syntax filter, not a policy engine — Karat uses this to test layered validation judgment, not pattern authoring bravado.

---

---

#### Q6. (M) A config-ingestion worker parses key/value lines from Windows-generated files. Keys on lines after the first never match:

```csharp
string file = "Server=prod-db\r\nPort=5432\r\nTimeout=30";
bool secondLineMatches = Regex.IsMatch(file, @"^Port=");
// secondLineMatches == false — team expects true
```

Explain why default regex behavior fails here and what minimal change fixes it without rewriting the parser as a full state machine.

---

**Answer:**

```csharp
string file = "Server=prod-db\r\nPort=5432\r\nTimeout=30";
bool secondLineMatches = Regex.IsMatch(file, @"^Port=");
// secondLineMatches == false — team expects true
```

Explain why default regex behavior fails here and what minimal change fixes it without rewriting the parser as a full state machine.

**Answer:** By default, `^` and `$` anchor only the start and end of the entire input string, not each line — so `^Port=` looks for `Port=` at position 0 of the whole blob (`"Server=…"`), never at the start of the second line after `\r\n`.

- **Mechanism:** Without `RegexOptions.Multiline`, `\r\n` is ordinary whitespace between characters; line boundaries are invisible to `^`/`$`. With `Multiline`, `^` matches after `\n` (and `\r\n` pairs) and `$` matches before `\n`.
- **Minimal fix:** pass `RegexOptions.Multiline`: `Regex.IsMatch(file, @"^Port=", RegexOptions.Multiline)` → `true`.
- **Alternative:** split lines first with `Regex.Split(file, @"\r?\n")` or `ReadLines` and test each line — clearer when you also need comment stripping or `#` handling (**Program.cs** Section 8).
- **Related gotcha:** `RegexOptions.Singleline` makes `.` span newlines — opposite concern when extracting multiline values; do not confuse Multiline (line anchors) with Singleline (dot behavior) — **Program.cs** Section 10.
- **Production note:** For large config files, line-by-line streaming avoids loading the entire file into one string match.

**Production takeaway:** Multiline is the fix for `^`/`$` per line — a common Karat mechanism question tied to Windows `\r\n` exports.

---

---

#### Q7. (R) An internal tool "sanitizes" HTML fragments before storing them in a knowledge base:

```csharp
public string StripTags(string html)
{
    string noTags = Regex.Replace(html, @"<.*>", string.Empty);
    return Regex.Replace(noTags, @"<script.*?>.*?</script>", string.Empty);
}
```

Review on input `"<div>Title</div><script>alert(1)</script>"`. What goes wrong with matching order, greediness, and security assumptions?

---

### 04. Var Dynamic & Special Keywords

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/08. Advanced C# Features/04. Var Dynamic & Special Keywords/`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

```csharp
public string StripTags(string html)
{
    string noTags = Regex.Replace(html, @"<.*>", string.Empty);
    return Regex.Replace(noTags, @"<script.*?>.*?</script>", string.Empty);
}
```

Review on input `"<div>Title</div><script>alert(1)</script>"`. What goes wrong with matching order, greediness, and security assumptions?

**Answer:** The greedy `<.*>` swallows from the first `<` through the last `>` in the string — removing the entire fragment including the script block in one pass — so the second replace never sees `<script>`; even with order reversed, regex is not a safe HTML sanitizer and cannot prevent attribute-based XSS.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Greedy `<.*>` spans to last `>` | Entire `"<div>…</div><script>…</script>"` becomes one match — all content deleted or mangled |
| Correctness | Script strip runs after tag strip | Script pattern never runs if greedy pass already consumed `<script>…` |
| Security | Regex-based tag removal is not HTML parsing | `<img onerror=alert(1)>`, malformed tags, and nested contexts bypass strip |
| Security | No allow-list of safe tags/attributes | "Sanitize" gives false confidence — stored XSS in knowledge base |
| Design | Two-pass replace order-dependent | Fragile refactors break redaction silently |

**Fix (priority order):**

1. Do not use regex for HTML security — use a vetted sanitizer (`HtmlSanitizer` NuGet, AngleSharp with allow-list, or store Markdown instead of raw HTML).
2. If regex is only for non-security display cleanup, use lazy quantifiers per tag: `<.*?>` — still wrong for nested `<div><div></div></div>` but matches **Program.cs** Section 12 greedy vs lazy demo.
3. Run script removal before any broad tag pass if you must stay regex-based for legacy reasons — and treat output as untrusted anyway.
4. Add integration tests with XSS payloads, not just `"<div>Title</div>"`.

```csharp
// Display-only collapse — NOT security:
string collapsed = Regex.Replace(html, @"<.*?>", string.Empty);
```

**Production takeaway:** Greedy `<.*>` is the textbook over-match bug, and Karat pairs it with "regex ≠ sanitizer" — use proper HTML parsers for user content. See **Program.cs** `DemonstratePitfalls` greedy vs lazy comparison.

---

### 04. Var Dynamic & Special Keywords

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/08. Advanced C# Features/04. Var Dynamic & Special Keywords/`

---

---

#### Q1. (R) A warehouse integration service parses third-party CSV rows into `dynamic` bags before posting to inventory. It passes QA with two sample files but throws in production on the first malformed row. Review the mapper:

```csharp
public sealed class DynamicImportMapper
{
    public decimal ComputeLineTotal(dynamic row)
    {
        var sku = row.Sku;
        var qty = row.Qantity;          // vendor column mapped at runtime
        var price = row.UnitPrice;
        return qty * price;
    }

    public void ImportBatch(IEnumerable<dynamic> rows)
    {
        foreach (dynamic row in rows)
        {
            var total = ComputeLineTotal(row);
            _ledger.Post(row.Sku, total);
        }
    }
}
```

What fails, when, and how would you harden this for production?

---

**Answer:**

```csharp
public sealed class DynamicImportMapper
{
    public decimal ComputeLineTotal(dynamic row)
    {
        var sku = row.Sku;
        var qty = row.Qantity;          // vendor column mapped at runtime
        var price = row.UnitPrice;
        return qty * price;
    }

    public void ImportBatch(IEnumerable<dynamic> rows)
    {
        foreach (dynamic row in rows)
        {
            var total = ComputeLineTotal(row);
            _ledger.Post(row.Sku, total);
        }
    }
}
```

What fails, when, and how would you harden this for production?

**Answer:** The typo `Qantity` compiles because `dynamic` skips member checking, then throws `RuntimeBinderException` at runtime on the first row that lacks that misspelled member — QA samples may never hit the path. `var` locals inherit `dynamic` when the initializer is dynamic, so the entire expression chain stays late-bound with no compile-time safety.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | Typo `row.Qantity` — no CS1061 | `RuntimeBinderException` aborts batch mid-import |
| Design | `dynamic` end-to-end for known CSV columns | Zero IntelliSense/refactor safety; schema drift undetected until prod |
| Correctness | No validation when members missing or wrong type | `qty * price` may bind wrong overload or fail on string values from CSV |
| Maintainability | `var` masks that locals are dynamically typed | Reviewers assume compile-time checking |

**Fix (priority order):**

1. Map to a strongly typed DTO (`InventoryImportRow`) with explicit property names and parse/validate at the boundary — typos become compile errors.
2. If shape is truly unknown, use `JsonDocument` / `JsonElement` or a dictionary with explicit key lookup and guard clauses instead of dot-syntax on `dynamic`.
3. Wrap per-row processing in try/catch for `RuntimeBinderException` only at the boundary if you must keep dynamic interop — log row index/SKU and continue or dead-letter, do not fail the whole batch silently.
4. Add contract tests with production-like malformed rows (missing columns, string `"12.5"` for quantity).

**Production takeaway:** `dynamic` trades compile-time safety for flexibility — Karat expects you to confine it to interop seams (COM, legacy plugins) and validate before business logic. See **Program.cs** Sections 3–4 — `RuntimeBinderException` on typos and **QUICK REFERENCE** — "dynamic typo → RuntimeBinderException."

---

---

#### Q2. (R) A pricing dashboard uses `var` with LINQ and mutates the source collection between query definition and enumeration. Review:

```csharp
public void PrintLowStockAlerts(InventoryItem[] stock)
{
    var lowStock = stock.Where(i => i.Quantity < 15).OrderBy(i => i.Sku);

    ApplyEmergencyRestock(stock);   // bumps quantities on several SKUs

    foreach (var item in lowStock)
    {
        _alerts.Send($"{item.Sku} critically low: {item.Quantity}");
    }
}

private static void ApplyEmergencyRestock(InventoryItem[] stock)
{
    foreach (var item in stock.Where(i => i.Quantity < 5))
        item.Quantity += 50;
}
```

What behavior do stakeholders see versus what they expect, and what would you change?

---

**Answer:**

```csharp
public void PrintLowStockAlerts(InventoryItem[] stock)
{
    var lowStock = stock.Where(i => i.Quantity < 15).OrderBy(i => i.Sku);

    ApplyEmergencyRestock(stock);   // bumps quantities on several SKUs

    foreach (var item in lowStock)
    {
        _alerts.Send($"{item.Sku} critically low: {item.Quantity}");
    }
}

private static void ApplyEmergencyRestock(InventoryItem[] stock)
{
    foreach (var item in stock.Where(i => i.Quantity < 5))
        item.Quantity += 50;
}
```

What behavior do stakeholders see versus what they expect, and what would you change?

**Answer:** `lowStock` is deferred `IEnumerable<InventoryItem>` — the `Where` predicate runs at enumeration time, after restock mutates quantities. SKUs that were low when the query was *defined* may no longer qualify (or vice versa), so alerts no longer match the "snapshot" ops thought they captured.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Deferred LINQ + mutation before `foreach` | Alert list reflects post-restock state, not state at query creation |
| Design | `var` hides `IEnumerable` deferral | Readers assume eager list; `var` inferred type is not `List<>` |
| Business logic | Restock before alert send in same method | Wrong ordering — alerts should fire on pre-restock snapshot or restock should run after |

**Fix (priority order):**

1. Materialize before mutation: `var lowStock = stock.Where(...).OrderBy(...).ToList();` then call `ApplyEmergencyRestock`.
2. Reorder operations: send alerts first, then restock — if business rules require alerting on original levels.
3. Use explicit type or comment when deferral matters: `IEnumerable<InventoryItem> lowStockQuery = ...` to signal lazy evaluation.
4. For reporting snapshots, project to immutable DTOs at capture time so later mutations cannot change alert content.

**Production takeaway:** `var` does not change LINQ semantics — deferred execution still bites when the underlying collection mutates. See **Program.cs** Section 2 — `var` with LINQ infers `IEnumerable<T>`, not a snapshot.

---

---

#### Q3. (R) A generic repository uses `nameof` and `default` for reflection-based updates. After a refactor, updates silently stop working for value-type columns. Review:

```csharp
public class GenericPatchHelper<T> where T : struct
{
    public static void EnsureColumnExists(string columnName)
    {
        var prop = typeof(T).GetProperty(columnName);
        if (prop is null)
            throw new InvalidOperationException($"Missing {columnName} on {nameof(T)}");
    }

    public static T CreateUnset()
    {
        return default;
    }
}

// Caller (InventoryDelta patch path):
string key = nameof(List<InventoryDelta>);   // used as dictionary / column key
var delta = GenericPatchHelper<InventoryDelta>.CreateUnset();
_audit[key] = delta.Amount;   // delta.Sku is null, Amount is 0
```

Identify the defects (compile-time, runtime, and data correctness) and prioritize fixes.

---

**Answer:**

```csharp
public class GenericPatchHelper<T> where T : struct
{
    public static void EnsureColumnExists(string columnName)
    {
        var prop = typeof(T).GetProperty(columnName);
        if (prop is null)
            throw new InvalidOperationException($"Missing {columnName} on {nameof(T)}");
    }

    public static T CreateUnset()
    {
        return default;
    }
}

// Caller (InventoryDelta patch path):
string key = nameof(List<InventoryDelta>);   // used as dictionary / column key
var delta = GenericPatchHelper<InventoryDelta>.CreateUnset();
_audit[key] = delta.Amount;   // delta.Sku is null, Amount is 0
```

Identify the defects (compile-time, runtime, and data correctness) and prioritize fixes.

**Answer:** `nameof(T)` inside a generic method returns the type parameter name (`"T"`), not the closed type (`"InventoryDelta"`). `nameof(List<InventoryDelta>)` yields `"List"`, not a useful storage key. `default` on a struct zeroes all fields — `Sku` is `null`, `Amount` is `0` — so `_audit` records garbage without throwing.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `nameof(List<InventoryDelta>)` → `"List"` | Wrong dictionary key; collisions and missing audit entries |
| Correctness | `nameof(T)` in generic method → `"T"` | Exception messages and metadata keys useless in logs |
| Data | `default` struct used as "unset" business object | Silent null SKU and zero amount posted to audit |
| Design | `where T : struct` + `default` conflates "missing" with valid zero delta | Cannot distinguish unset from intentional `Amount = 0` |

**Fix (priority order):**

1. Use `typeof(T).Name` or `nameof(InventoryDelta)` at call sites for keys — never `nameof(T)` when you need the closed type name (see **Program.cs** Section 7 — `nameof(List<int>)` → `"List"` pitfall).
2. Replace `CreateUnset()` with explicit factory or `Nullable<T>` / `Optional<InventoryDelta>` if "unset" is a business state.
3. Validate before audit: `if (string.IsNullOrEmpty(delta.Sku)) throw ...` — do not propagate zeroed structs.
4. For reflection keys, use `nameof(InventoryDelta.Sku)` with `GetProperty` — matches **Program.cs** dynamic vs reflection demo.

**Production takeaway:** `nameof` is compile-time safe for *syntax* but not semantically magic — generic arity and type-parameter names trip audit and ORM code. `default` on structs is a valid zero value, not "empty optional."

---

---

#### Q4. (P) Your team ingests nightly plugin config from a legacy host that exposes JSON whose shape changes per warehouse (extra keys, missing booleans, numeric strings). A junior dev proposes `dynamic` + `ExpandoObject` for the entire pipeline; another proposes strongly typed records + `System.Text.Json` with `[JsonExtensionData]`. When is `dynamic` justified here, and what production risks push you toward typed or semi-typed models?

---

**Answer:**

**Answer:** Use `dynamic` only at the thin interop boundary where you truly cannot describe the contract (COM, embedded scripting, some legacy APIs). For JSON plugin config with known core fields and variable extensions, prefer typed records with `[JsonExtensionData] Dictionary<string, JsonElement>` or a dedicated options class — you keep compile checks on `WarehouseId`, `MaxSkus`, etc., while absorbing extra keys.

- **`dynamic` risks:** `RuntimeBinderException` in production on typos; no refactor support; harder unit tests; DLR overhead on hot paths; `ExpandoObject` members are not normal CLR properties — reflection returns null (**Program.cs** Section 6).
- **Typed + extension data:** Core settings validated at deserialize time; unknown keys preserved for forward compatibility; schema changes caught in CI with golden JSON fixtures.
- **When `dynamic` wins:** One-off script host, JScript/COM object, or third-party DLL that only exposes late-bound objects — wrap in an adapter and map to typed models immediately inside the adapter.
- **Middle ground:** Deserialize to `JsonDocument`, query required nodes explicitly, validate types before mapping — no DLR, explicit errors.

**Production takeaway:** Karat tests judgment on *where* to stop using `dynamic` — one adapter method, not the whole pipeline. Match **Program.cs** warehouse plugin scenario: ExpandoObject for demo config, but production code maps to validated types before inventory rules run.

---

---

#### Q5. (M) A background price-refresh worker should stop within seconds when ops clicks "Cancel" in the admin UI. The flag works in dev (single core, low load) but the worker occasionally runs for minutes in production. Review:

```csharp
public sealed class PriceRefreshWorker
{
    private bool _stopRequested;

    public void RequestStop() => _stopRequested = true;

    public void RunLoop()
    {
        while (!_stopRequested)
        {
            RefreshNextSku();
            Thread.Sleep(10);
        }
    }
}
```

What mechanism is missing, why does it pass locally, and what would you use instead for a simple stop flag versus a counter you increment?

---

**Answer:**

```csharp
public sealed class PriceRefreshWorker
{
    private bool _stopRequested;

    public void RequestStop() => _stopRequested = true;

    public void RunLoop()
    {
        while (!_stopRequested)
        {
            RefreshNextSku();
            Thread.Sleep(10);
        }
    }
}
```

What mechanism is missing, why does it pass locally, and what would you use instead for a simple stop flag versus a counter you increment?

**Answer:** `_stopRequested` must be `volatile` (or guarded by `Interlocked`/lock) so the worker thread observes the UI thread's write promptly. Without `volatile`, the JIT may cache the field in a register and the loop may never exit — intermittent and load-dependent, which is why dev often passes.

- **Simple boolean stop flag:** `private volatile bool _stopRequested;` — matches **Program.cs** `PriceRefreshSignal` (Section 4). Documents intent: cross-thread visibility, not atomic compound updates.
- **Counter / statistics:** use `Interlocked.Increment` / `Interlocked.Read` — `volatile` alone does not make read-modify-write atomic.
- **Why local dev hides it:** single-core, short runs, or debugger flushes memory; production multi-core reordering exposes the bug.
- **Alternative:** `CancellationToken` from `CancellationTokenSource` — idiomatic for .NET worker services and ASP.NET hosted services; propagates through async calls better than a raw flag.

**Production takeaway:** `volatile` is field-only and not a lock — pair with simple flags only. See **Program.cs** QUICK REFERENCE — "volatile on local → CS0106" and threading depth in **06. Multithreading & Async**.

---

---

#### Q6. (D) Two teams share a `WarehouseAnalytics` namespace. Team A added a helper type named `Math` for domain-specific rounding; Team B assumed BCL `System.Math` in unqualified calls. Review the pricing snippet:

```csharp
namespace WarehouseAnalytics.Pricing;

public static class Math
{
    public static decimal RoundToNickel(decimal value)
        => global::System.Math.Round(value / 0.05m) * 0.05m;
}

public sealed class LineTotalCalculator
{
    public decimal ApplyTax(decimal net, decimal rate)
    {
        decimal gross = net * (1 + rate);
        return Math.Round(gross, 2);   // which Math?
    }
}
```

What breaks at compile time or runtime, and what naming or qualification policy prevents this in a shared codebase?

---

### 05. C# 7 Features

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/08. Advanced C# Features/05. C# 7 Features/`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

```csharp
namespace WarehouseAnalytics.Pricing;

public static class Math
{
    public static decimal RoundToNickel(decimal value)
        => global::System.Math.Round(value / 0.05m) * 0.05m;
}

public sealed class LineTotalCalculator
{
    public decimal ApplyTax(decimal net, decimal rate)
    {
        decimal gross = net * (1 + rate);
        return Math.Round(gross, 2);   // which Math?
    }
}
```

What breaks at compile time or runtime, and what naming or qualification policy prevents this in a shared codebase?

**Answer:** Unqualified `Math.Round` resolves to `WarehouseAnalytics.Pricing.Math` in that namespace — which has no `Round(decimal, int)` overload, so you get **CS0117** at compile time (not a silent wrong answer). The shadowing is still a maintenance trap: every new developer repeats the failure until they learn the local type exists.

- **Immediate fix:** `global::System.Math.Round(gross, 2)` or `System.Math.Round` with a file-level alias `using BclMath = System.Math;`.
- **Policy:** Ban type names that shadow BCL types (`Math`, `Thread`, `Task`, `Environment`) in shared namespaces — rename to `PricingMath`, `MoneyRounding`, etc. **Program.cs** Section 9 explicitly warns never ship shadow names in production.
- **Linting:** Enable analyzer rules or code review checklist for BCL name collisions; namespace-per-feature reduces accidental local `Math` helpers in global pricing code.
- **Design:** Domain rounding belongs on a clearly named static class (`CurrencyRounding.ToNickel`) so call sites document intent and never compete with `System.Math` lookup.

**Production takeaway:** `global::` fixes resolution but does not fix team confusion — Karat pairs mechanism (`global::System.Math`) with design policy (do not shadow BCL). See **Program.cs** Sections 6 and 9 — local `Math` vs `global::System.Math.PI`.

---

### 05. C# 7 Features

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/08. Advanced C# Features/05. C# 7 Features/`

---

---

#### Q1. (R) Express VIP orders are routed to the standard express lane in production. Review this C# 7 switch with `when` guards (mirrors the warehouse routing demo):

```csharp
public string RouteOrder(Order order)
{
    switch (order.Priority)
    {
        case OrderPriority.Express:
            return "EXPRESS-STANDARD";
        case OrderPriority.Express when order.IsHighValue:
            return "EXPRESS-VIP";
        case OrderPriority.Critical:
            return "CRITICAL-LANE";
        case OrderPriority.Standard when order.Quantity > 100:
            return "BULK-STANDARD";
        default:
            return "STANDARD";
    }
}
```

What is wrong, why do unit tests on isolated high-value express orders pass while mixed batches fail SLA, and how do you fix it?

---

**Answer:**

**Answer:** The `Express` case without a `when` guard matches every express order first, so the `Express when order.IsHighValue` branch is unreachable dead code — high-value express orders never reach VIP routing.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Unconditional `case OrderPriority.Express` precedes guarded case | VIP lane never selected; SLA breach on high-value express |
| Pattern matching | Misunderstanding first-match-wins switch semantics | Silent logic bug — compiles and runs |
| Testing | Tests that only assert "express → some express lane" pass | Mixed-batch production traffic exposes wrong lane |

**Fix (priority order):**

1. Reorder cases so **more specific patterns come first** — mirror the tutorial's `RouteOrder` in **Program.cs** Section 5:

```csharp
switch (order.Priority)
{
    case OrderPriority.Critical:
        return "CRITICAL-LANE";
    case OrderPriority.Express when order.IsHighValue:
        return "EXPRESS-VIP";
    case OrderPriority.Express:
        return "EXPRESS-STANDARD";
    case OrderPriority.Standard when order.Quantity > 100:
        return "BULK-STANDARD";
    case OrderPriority.Standard:
        return "STANDARD";
    default:
        return "UNKNOWN";
}
```

2. Add a test matrix: `(Express, high-value)`, `(Express, low-value)`, `(Standard, qty>100)` — assert distinct lanes.
3. Consider a C# 8+ switch expression later for exhaustiveness; C# 7 switch still requires manual ordering discipline.

**Production takeaway:** C# 7 switch patterns behave like ordered rule lists, not `if/else if` auto-reordering — Karat embeds this as a routing bug that compiles cleanly. See **Program.cs** QUICK REFERENCE — "Pattern case order wrong → Wrong branch taken."

---

---

#### Q2. (R) A product lookup was optimized with `ValueTask<string>` for cache hits. Under retry logic, intermittent `InvalidOperationException` appears. Review:

```csharp
private readonly Dictionary<int, string> _cache = new();

public async ValueTask<string> GetProductNameAsync(int productId)
{
    if (_cache.TryGetValue(productId, out string? cached))
        return cached;

    string fetched = await _catalogClient.FetchNameAsync(productId);
    _cache[productId] = fetched;
    return fetched;
}

// Caller in order enrichment:
public async Task EnrichOrderAsync(Order order)
{
    var nameTask = _lookup.GetProductNameAsync(order.ProductId);

    order.ProductName = await nameTask;

    if (string.IsNullOrEmpty(order.ProductName))
        order.ProductName = await nameTask;  // retry same ValueTask
}
```

Identify stacked issues (async semantics, caching, API contract). What production pattern replaces the double await?

---

**Answer:**

**Answer:** A consumed `ValueTask` must not be awaited twice unless it wraps a `Task` or `IValueTaskSource` — the retry path re-awaits the same instance after the cache-hit path already completed it synchronously, causing undefined behavior or `InvalidOperationException`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Async / ValueTask | Double `await` on same `ValueTask` instance | Intermittent crash on cache-hit + empty-name retry |
| API contract | Caller stores and reuses `ValueTask` like `Task` | Violates ValueTask consumption rules |
| Design | Retry treats empty string as fetch failure without re-invoking | Masks catalog bugs; triggers invalid reuse |

**Fix (priority order):**

1. **Do not cache the `ValueTask`** — call `GetProductNameAsync` again on retry, or await once into a `string`:

```csharp
order.ProductName = await _lookup.GetProductNameAsync(order.ProductId);

if (string.IsNullOrEmpty(order.ProductName))
    order.ProductName = await _lookup.GetProductNameAsync(order.ProductId);
```

2. Better: return `Task<string>` on public boundaries unless profiling proves allocation pressure; keep `ValueTask` internal to hot paths documented as single-consumption.
3. Fix empty-name handling at source — distinguish "not found" from empty string instead of blind retry.
4. Add analyzer discipline: treat `ValueTask`/`ValueTask<T>` like `IDisposable` — one consumer.

**Production takeaway:** `ValueTask` wins on cache hits (**Program.cs** Section 9) but fails when callers treat it like a reusable `Task` — a common Karat stack of optimization + retry logic.

---

---

#### Q3. (R) A developer refactors inventory reservation to use C# 7 ref returns for in-place updates. The build fails; after a workaround it crashes in QA. Review:

```csharp
public ref int FindSlot(int[] counts, int index)
{
    if (index < 0 || index >= counts.Length)
        throw new ArgumentOutOfRangeException(nameof(index));

    ref int slot = ref counts[index];
    return ref slot;  // "clearer" than returning ref counts[index]
}

public async Task ReserveAsync(Order order, int[] inventory, int skuIndex)
{
    ref int slot = ref FindSlot(inventory, skuIndex);
    await Task.Delay(10);  // simulate DB commit
    slot -= order.Quantity;
}

public ref int GetOrCreateSlot(List<int> counts, int index)
{
    while (counts.Count <= index)
        counts.Add(0);

    return ref counts[index];
}
```

What compile-time and runtime problems exist, and how should reservation mutate inventory safely in async code?

---

**Answer:**

**Answer:** `ref` locals and `ref` returns cannot appear in `async` methods (state machine restriction), and `ref` returns must target stable storage — a `List<T>` indexer returns a temporary ref in many contexts, not a durable slot alias safe across growth/reallocation.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `ref` local in `async Task ReserveAsync` | CS8170 / CS4012 — async methods cannot use ref locals |
| Runtime / API | `ref counts[index]` on `List<int>` | Ref may not track list reallocation; corrupt or invalid updates |
| Design | Sync ref mutation mixed with awaited I/O | Reservation not atomic with commit; race if ever made to compile |

**Fix (priority order):**

1. Remove `ref` from async paths — compute new value after `await`, assign by index:

```csharp
public async Task ReserveAsync(Order order, int[] inventory, int skuIndex)
{
    await Task.Delay(10);
    inventory[skuIndex] -= order.Quantity;
}
```

2. Use **arrays** (as in **Program.cs** `FindInventorySlot`) for in-place ref mutation in synchronous hot paths only.
3. For `List<T>`, mutate via index assignment or lock — do not expose `ref` return on list indexer.
4. Keep `FindSlot` synchronous; if reservation needs I/O, read/modify/write as explicit steps with concurrency control (`lock`, `Interlocked`, or DB transaction).

**Production takeaway:** Ref returns suit fixed buffers (**Program.cs** Section 8); pairing them with `async` or `List<T>` is a stacked compile + lifetime trap Karat uses to test feature boundaries.

---

---

#### Q4. (R) A CSV import pipeline uses C# 7 out variables. Finance sees rows with quantity `0` marked as successfully imported. Review:

```csharp
public bool TryImportRow(string[] columns, out Order row)
{
    row = new Order();

    if (!int.TryParse(columns[0], out var orderId))
        return false;

    row.OrderId = orderId;

    int.TryParse(columns[2], out var qty);
    row.Quantity = qty;

    if (dict.TryGetValue(columns[1], out var skuMeta))
        row.Sku = skuMeta;
    else
        row.Sku = columns[1];

    return true;
}
```

What logic bugs hide behind valid C# 7 syntax, and how do you harden parsing without reverting to pre-C# 7 style?

---

**Answer:**

**Answer:** `int.TryParse` failure on quantity is ignored — `out var qty` defaults to `0` and import still returns `true` because only `orderId` failure short-circuits; invalid quantity silently becomes zero.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Quantity parse result not checked | Bad CSV → qty 0 orders marked successful |
| Out variables | `out var` makes parse easy to "fire and forget" | C# 7 ergonomics hide missing validation |
| Data integrity | `return true` after partial parsing | Downstream fulfillment and billing act on garbage |

**Fix (priority order):**

1. Check every `TryParse` return value — C# 7 inline `out` does not remove validation duty:

```csharp
if (!int.TryParse(columns[2], out var qty) || qty <= 0)
    return false;

row.Quantity = qty;
```

2. Validate SKU with explicit failure when required — `TryGetValue` fallback to raw string may be intentional, but document it.
3. Consider `RequirePositiveQuantity` pattern from **Program.cs** Section 11 (throw expression) for internal APIs vs `Try*` for import boundaries.
4. Log row index and raw columns on `false` — ops needs rejected rows, not silent zeros.

**Production takeaway:** Out variables reduce boilerplate (**Program.cs** Section 3) but Karat tests whether you still branch on the `bool` — `out var` without checking is a production data bug.

---

---

#### Q5. (P) A warehouse fulfillment microservice returns `(bool CanFulfill, string Note)` tuples from `CheckFulfillment` — the same shape as the tutorial's tuple demo. The team debates replacing tuples with a `FulfillmentResult` record before exposing the method on a public NuGet contract. When is the tuple idiomatic, and when does it break production maintainability?

---

**Answer:**

**Answer:** Named tuples are fine for private or internal helpers with stable, obvious element semantics; public NuGet contracts need a named type so additions, serialization, and versioning do not break consumers silently.

- **Keep tuples** for internal methods with two or three tightly coupled values and no evolution expected — e.g. private `CheckFulfillment` inside one service class, same assembly as **Program.cs** Section 6 demo.
- **Use a record/class** when the shape crosses assembly boundaries, gains fields (`ReservedQuantity`, `BackorderSku`), needs JSON/XML mapping, or appears in logs/metrics dashboards — `(bool, string)` element names are not part of the runtime contract.
- **Inferred tuple names (C# 7.1)** help readability at the return site but do not replace API documentation for external callers.
- **Tuple equality (C# 7.3)** is useful for tests comparing snapshots; not a substitute for domain identity on persisted entities.

**Production takeaway:** Tuples are a local convenience feature — Karat asks you to draw the line at package/public API surfaces where contract evolution and tooling (OpenAPI, analyzers) require named types.

---

---

#### Q6. (M) A batch job uses a local function with captured outer state to retry flaky lane assignments. Ops reports duplicate reservations on the same SKU after parallel batch splits. Review:

```csharp
public void ProcessBatch(IEnumerable<Order> orders, int[] inventory)
{
    int reservationFailures = 0;

    void TryReserve(Order order, int skuIndex)
    {
        ref int slot = ref inventory[skuIndex];
        if (slot < order.Quantity)
        {
            reservationFailures++;
            return;
        }
        slot -= order.Quantity;
    }

    Parallel.ForEach(orders, order =>
    {
        int idx = _skuIndexMap[order.Sku];
        TryReserve(order, idx);
    });

    _metrics.RecordFailures(reservationFailures);
}
```

Explain mechanism-level behavior: what C# 7 features interact here, and what breaks under concurrency even though local functions and ref locals compile?

---

**Answer:**

**Answer:** The local function captures `reservationFailures` and mutates `inventory` through `ref` aliases while `Parallel.ForEach` runs handlers concurrently — non-atomic read-modify-write on shared array slots and non-interlocked increments corrupt counts under race.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Concurrency | `Parallel.ForEach` + unsynchronized `slot -= order.Quantity` | Lost updates → overselling inventory |
| Local functions | Closure over `reservationFailures++` | Race on int increment; inaccurate metrics |
| Ref locals | `ref int slot = ref inventory[skuIndex]` in parallel bodies | Each thread aliases same storage without lock |

**Fix (priority order):**

1. Do not parallelize in-place mutation on a shared array without synchronization — use `lock` per SKU, `ConcurrentDictionary`, or partition work by SKU.
2. Replace `reservationFailures++` with `Interlocked.Increment(ref reservationFailures)` if you keep shared counters.
3. Prefer **static local functions (C# 8+)** or private methods that take explicit parameters — makes captured state visible in review; see **Program.cs** Section 7 note on static locals.
4. For warehouse scale-out, reservation belongs in a transactional store (DB row lock), not a shared in-memory array across parallel threads.

**Production takeaway:** Local functions + ref locals are synchronous, single-flow tools from C# 7 — Karat stacks them with `Parallel.ForEach` to test whether you recognize closure and alias concurrency, not just syntax.

---

---

#### Q7. (D) Two teammates implement guard clauses for order validation. Which approach do you standardize on for a shared domain library, and why?

**Option A — throw expressions (C# 7):**

```csharp
public Order Accept(Order? order) =>
    order ?? throw new ArgumentNullException(nameof(order));

public int NormalizeQty(int qty) =>
    qty > 0 ? qty : throw new ArgumentOutOfRangeException(nameof(qty));
```

**Option B — classic blocks:**

```csharp
public Order Accept(Order? order)
{
    if (order is null)
        throw new ArgumentNullException(nameof(order));
    return order;
}

public int NormalizeQty(int qty)
{
    if (qty <= 0)
        throw new ArgumentOutOfRangeException(nameof(qty), qty, "Quantity must be positive.");
}
```

Consider expression-bodied members, exception detail for operators, and refactor safety in code review.

---

### 06. C# 8 Features

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/08. Advanced C# Features/06. C# 8 Features/`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

**Answer:** Standardize on **Option B (classic blocks)** for shared domain validation libraries, and allow **Option A (throw expressions)** only for thin one-liners where exception detail stays minimal — not as the default for public APIs that operators debug from logs.

- **Throw expressions** pair well with expression-bodied members (**Program.cs** Sections 10–11) for null-guard properties and private helpers — `order ?? throw new ArgumentNullException(nameof(order))` is clear and concise.
- **Classic blocks** win when you need overloads with `(paramName, actualValue, message)` on `ArgumentOutOfRangeException`, multiple guards, or XML doc that describes thrown types — Option B's qty check carries the offending value; Option A's ternary does not.
- **Refactor safety:** Expression-bodied throw chains are harder to breakpoint and step through in production debugging than block bodies.
- **Consistency:** Mixed styles across a NuGet domain library frustrate code review — pick block guards for public methods, throw expressions for small internal `Require*` helpers like **Program.cs** `RequireOrder` / `RequirePositiveQuantity`.

**Production takeaway:** C# 7 throw expressions are idiomatic guards, not a wholesale replacement for validation methods — Karat tests judgment on expression-bodied brevity vs operability in a shared library.

---

### 06. C# 8 Features

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/08. Advanced C# Features/06. C# 8 Features/`

---

---

#### Q1. (R) After enabling `<Nullable>enable</Nullable>` on the document-ingest API, QA reports intermittent `NullReferenceException` on documents with no footnotes. Review the service:

```csharp
public sealed class DocumentIngestService
{
    public string BuildSummary(DocumentMetadata metadata)
    {
        string header = metadata.Id!.Trim();
        string note = metadata.Notes.ToUpperInvariant();
        return $"{header}: {note}";
    }

    public DocumentMetadata LoadFromJson(string json)
    {
        var doc = JsonSerializer.Deserialize<DocumentMetadata>(json);
        doc!.Id = doc.Id ?? "UNKNOWN";
        return doc;
    }
}

public sealed class DocumentMetadata
{
    public string Id { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
```

What problems remain after the developer "fixed" compiler warnings with `!`, and how do you harden this for production?

---

**Answer:**

```csharp
public sealed class DocumentIngestService
{
    public string BuildSummary(DocumentMetadata metadata)
    {
        string header = metadata.Id!.Trim();
        string note = metadata.Notes.ToUpperInvariant();
        return $"{header}: {note}";
    }

    public DocumentMetadata LoadFromJson(string json)
    {
        var doc = JsonSerializer.Deserialize<DocumentMetadata>(json);
        doc!.Id = doc.Id ?? "UNKNOWN";
        return doc;
    }
}

public sealed class DocumentMetadata
{
    public string Id { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
```

What problems remain after the developer "fixed" compiler warnings with `!`, and how do you harden this for production?

**Answer:** Null-forgiving operators silence the compiler without proving non-null at runtime — `Notes` is still null for many documents, and `JsonSerializer.Deserialize` can return null entirely. Production NRT requires honest annotations plus guards at boundaries, not blanket `!`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| NRT misuse | `metadata.Notes.ToUpperInvariant()` on `string?` | `NullReferenceException` when `Notes` is null — matches QA report |
| NRT misuse | `metadata.Id!` without validation | Masks missing IDs; empty or null IDs slip into downstream queues |
| Correctness | `Deserialize` result used without null check | `NullReferenceException` on malformed or `"null"` JSON |
| Design | Suppressions instead of contract | Warnings return on next edit; team learns to ignore NRT |

**Fix (priority order):**

1. Handle optional notes explicitly: `var note = metadata.Notes?.ToUpperInvariant() ?? "(no notes)";` — mirrors **Program.cs** Section 8.
2. Validate `metadata` and `metadata.Id` at the API boundary; throw `ArgumentException` or return `ProblemDetails` for bad input — do not use `!` on unvalidated data.
3. Guard deserialization: `var doc = JsonSerializer.Deserialize<DocumentMetadata>(json) ?? throw new JsonException("…");` or return `DocumentMetadata?` and let callers decide.
4. Configure `JsonSerializerOptions` with required-property validation (modern) or a dedicated DTO layer for ingest.
5. Treat new CS86xx warnings as build breaks in CI for touched projects — block `#nullable disable` without ticket.

**Production takeaway:** NRT is a **contract tool**, not a runtime checker — `!` on inbound HTTP/JSON data recreates the null bugs you enabled NRT to prevent. See **Program.cs** Section 8 — nullable annotations and `??` for optional `Notes`.

---

---

#### Q2. (R) A background worker streams archive pages to blob storage. Under deploy cancellation, the job keeps running for minutes and sometimes OOMs. Review the consumer and producer:

```csharp
public async Task ArchivePagesAsync(CancellationToken stoppingToken)
{
    var allPages = _documentStream.ReadPagesAsync()
        .ToListAsync(stoppingToken)
        .GetAwaiter()
        .GetResult();

    foreach (string page in allPages)
    {
        await _blobWriter.UploadPageAsync(page);
    }
}

public async IAsyncEnumerable<string> ReadPagesAsync(
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
{
    foreach (string page in _pages)
    {
        await Task.Delay(200, cancellationToken);
        yield return page;
    }
}
```

Identify stacked compile-time, runtime, and scalability issues. What is the correct streaming pattern?

---

**Answer:**

```csharp
public async Task ArchivePagesAsync(CancellationToken stoppingToken)
{
    var allPages = _documentStream.ReadPagesAsync()
        .ToListAsync(stoppingToken)
        .GetAwaiter()
        .GetResult();

    foreach (string page in allPages)
    {
        await _blobWriter.UploadPageAsync(page);
    }
}

public async IAsyncEnumerable<string> ReadPagesAsync(
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
{
    foreach (string page in _pages)
    {
        await Task.Delay(200, cancellationToken);
        yield return page;
    }
}
```

Identify stacked compile-time, runtime, and scalability issues. What is the correct streaming pattern?

**Answer:** The consumer buffers the entire async stream into memory via sync-over-async `.GetResult()`, defeating `IAsyncEnumerable` streaming and ignoring cooperative cancellation during enumeration. Large archives OOM; deploy stops cannot abort the materialization phase promptly.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Async | `.GetResult()` on `ToListAsync` | Sync-over-async; thread-pool blocking; potential deadlocks in hosted contexts |
| Scalability | Materialize-all before upload | Memory proportional to total pages — OOM on large documents |
| Cancellation | `ReadPagesAsync()` called without token | `[EnumeratorCancellation]` never receives `stoppingToken`; delay loop ignores host shutdown |
| Design | Stream treated like `List<T>` | Loses backpressure; cannot start uploading until full read completes |

**Fix (priority order):**

1. Consume with streaming: `await foreach (var page in _documentStream.ReadPagesAsync(stoppingToken).WithCancellation(stoppingToken))` and upload inside the loop — matches **Program.cs** Sections 9–10.
2. Remove `.ToListAsync().GetResult()` entirely; keep the method `async` end-to-end.
3. Pass `stoppingToken` into `ReadPagesAsync(stoppingToken)` so cancellation propagates into `Task.Delay` and the async iterator tears down.
4. Optionally bound concurrency with a `SemaphoreSlim` if uploads overlap, but never buffer the whole sequence unless size is proven bounded.
5. Use `await using` on `AsyncDocumentStream` when the producer holds connections — **Program.cs** `IAsyncDisposable` demo.

**Production takeaway:** `IAsyncEnumerable<T>` is for **incremental** async production — buffering it to a list is an anti-pattern unless you have a hard upper bound. Cancellation must flow through `WithCancellation` / `[EnumeratorCancellation]`.

---

---

#### Q3. (R) A routing microservice parses document IDs with C# 8 ranges after a format change. Production throws `ArgumentOutOfRangeException` on valid-looking IDs. Review:

```csharp
public string ExtractYear(string documentId)
{
    return documentId[4..8];
}

public string ExtractSequence(string documentId)
{
    return documentId[^3..];
}

public DocumentMetadata[] TakeTail(DocumentMetadata[] batch, int tailCount)
{
    return batch[^tailCount..];
}

public void ValidateId(string documentId)
{
    if (documentId.StartsWith("DOC-"))
    {
        string year = ExtractYear(documentId);
        _metrics.RecordYear(year);
    }
}
```

Expected format: `DOC-YYYY-NNN` (minimum 12 characters). What assumptions break in production, and how do you fix parsing defensively?

---

**Answer:**

```csharp
public string ExtractYear(string documentId)
{
    return documentId[4..8];
}

public string ExtractSequence(string documentId)
{
    return documentId[^3..];
}

public DocumentMetadata[] TakeTail(DocumentMetadata[] batch, int tailCount)
{
    return batch[^tailCount..];
}

public void ValidateId(string documentId)
{
    if (documentId.StartsWith("DOC-"))
    {
        string year = ExtractYear(documentId);
        _metrics.RecordYear(year);
    }
}
```

Expected format: `DOC-YYYY-NNN` (minimum 12 characters). What assumptions break in production, and how do you fix parsing defensively?

**Answer:** Range and index syntax does not validate length — it throws when the span is too short or when `^tailCount` exceeds the array. `StartsWith("DOC-")` is necessary but not sufficient for a 12-character ID, so truncated or legacy IDs pass validation then fail inside slice helpers.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | `documentId[4..8]` on short strings | `ArgumentOutOfRangeException` — 500s on legacy `DOC-24-001` style IDs |
| Runtime | `batch[^tailCount..]` when `tailCount > batch.Length` | Same exception when batch is smaller than requested tail |
| Correctness | Validation gate too weak | Metrics and routing run on IDs that cannot satisfy slice contracts |
| Design | Magic numeric indices scattered | Format change requires hunting literal `4`, `8`, `^3` across services |

**Fix (priority order):**

1. Centralize format validation: `if (documentId.Length < 12) return false;` or use `Regex` / `ReadOnlySpan<char>` with explicit length checks before any range.
2. Replace magic slices with named helpers or `Range` constants tied to the format spec — **Program.cs** Section 11 shows `id[4..8]` and `id[^3..]` only after a known-good sample.
3. Guard `TakeTail`: `if (tailCount <= 0) return Array.Empty<…>();` and clamp: `int start = Math.Max(0, batch.Length - tailCount); return batch[start..];`
4. Return `bool TryExtractYear(string id, out string year)` instead of throwing parsers — callers record parse failures without crashing the request pipeline.
5. Add contract tests for min-length, max-length, and migrated ID formats.

**Production takeaway:** C# 8 ranges are **syntax sugar over Index/Range** — they inherit all bounds-check behavior. Validate once at the boundary; never assume `"DOC-"` implies slice-safe length.

---

---

#### Q4. (R) A teammate refactors chunk parsing to overlap I/O with processing. The build fails and code review finds async/ref-struct mixing. Review:

```csharp
public async Task<int> ProcessHeaderAsync(string headerLine)
{
    ReadOnlySpan<char> idSpan = headerLine.AsSpan();

    using DocumentChunkReader chunkReader = new DocumentChunkReader(idSpan);

    await Task.Delay(10);

    return chunkReader.VisibleLength;
}

public ref struct DocumentChunkReader
{
    private ReadOnlySpan<char> _buffer;

    public DocumentChunkReader(ReadOnlySpan<char> buffer) => _buffer = buffer;

    public int VisibleLength => _buffer.Length;

    public void Dispose() => _buffer = ReadOnlySpan<char>.Empty;
}
```

What C# 8 rules are violated, and what refactor preserves both async I/O and span-based parsing?

---

**Answer:**

```csharp
public async Task<int> ProcessHeaderAsync(string headerLine)
{
    ReadOnlySpan<char> idSpan = headerLine.AsSpan();

    using DocumentChunkReader chunkReader = new DocumentChunkReader(idSpan);

    await Task.Delay(10);

    return chunkReader.VisibleLength;
}

public ref struct DocumentChunkReader
{
    private ReadOnlySpan<char> _buffer;

    public DocumentChunkReader(ReadOnlySpan<char> buffer) => _buffer = buffer;

    public int VisibleLength => _buffer.Length;

    public void Dispose() => _buffer = ReadOnlySpan<char>.Empty;
}
```

What C# 8 rules are violated, and what refactor preserves both async I/O and span-based parsing?

**Answer:** `ref struct` instances cannot survive an `await` because the async state machine may move execution to the heap — the compiler rejects storing `DocumentChunkReader` across suspension points. Disposable ref structs are stack-only and must be disposed before the first `await` in the method.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `ref struct` local used after `await` | CS4012 / CS9202 — build failure |
| Correctness | `ReadOnlySpan<char>` tied to `headerLine` lifetime | If overlap with mutation or stack pop, span could be invalid — less common with `string` but real with stack buffers |
| Design | Mixing stack-only parsing with async I/O in one scope | Forces either copy-to-string or split phases |

**Fix (priority order):**

1. **Parse-then-await split:** dispose reader and extract needed scalars (`int length = chunkReader.VisibleLength;`) **before** any `await` — same pattern as **Program.cs** Section 7 synchronous `using` block.
2. Copy to owned memory when async work must interleave: `string id = headerLine;` or `char[] buffer = headerLine.ToCharArray();` then parse without ref struct across awaits.
3. Keep `DocumentChunkReader` synchronous; perform async I/O first, then run span-based parsing on the fetched `string`/`ReadOnlyMemory<char>`.
4. Do not box ref structs or store them in fields — C# 8 disallows it by design.

```csharp
public async Task<int> ProcessHeaderAsync(string headerLine)
{
    await Task.Delay(10);

    ReadOnlySpan<char> idSpan = headerLine.AsSpan();
    using var chunkReader = new DocumentChunkReader(idSpan);
    return chunkReader.VisibleLength;
}
```

**Production takeaway:** C# 8 disposable ref structs pair with **synchronous** hot paths over spans — async methods need a phase boundary before stack-only types enter the picture.

---

---

#### Q5. (P) Your team ships `IDocumentProcessor` as a shared NuGet package. Version 1 has `Process` and `ProcessorName`. Version 2 needs a `Describe()` helper without forcing every consumer to recompile. A consumer already implements both `IDocumentProcessor` and `IArchiveReporter`, each adding a default `Describe()`. How do you evolve the interface using C# 8 default interface methods, and what breaks if you ignore diamond ambiguity?

---

**Answer:**

**Answer:** Add `Describe()` as a **default interface method** on `IDocumentProcessor` so existing implementers inherit behavior at runtime without source changes, while new implementers may override selectively — but when two interfaces supply the same default signature, the implementing class must resolve ambiguity explicitly.

- **Safe evolution:** `string Describe() => $"{ProcessorName} processor";` on the interface — matches **Program.cs** Section 3 (`TextDocumentProcessor` uses default; `MarkupDocumentProcessor` overrides).
- **Binary compatibility:** Consumers compiled against v1 load v2 because DIMs are resolved at runtime via interface dispatch — no mandatory recompile for default-only additions.
- **Override path:** Document that implementers *may* replace `Describe()` for custom telemetry; do not require it.
- **Diamond ambiguity:** If `IArchiveReporter` also adds `string Describe() => "archive";`, `class Worker : IDocumentProcessor, IArchiveReporter` fails with CS0108/CS0539-style ambiguity when calling `Describe()` on the class without explicit qualification.
- **Resolution:** Explicit interface implementation — `string IDocumentProcessor.Describe() => …;` and `string IArchiveReporter.Describe() => …;` — or rename one method before shipping.
- **Testing:** Run contract tests against **interface-typed** references, not only concrete types — default vs override behavior differs by static type.

**Production takeaway:** Default interface methods are for **additive, non-breaking** API evolution — not a free pass; colliding defaults across interfaces are a design smell that must be resolved before publish.

---

---

#### Q6. (M) A nightly batch opens thousands of small files under a shared directory. After migrating to `using var`, ops reports "too many open files" and memory climbs until the job finishes. Review the loop:

```csharp
public BatchResult IngestFolder(string rootPath)
{
    var result = new BatchResult();
    IEnumerable<string> paths = Directory.EnumerateFiles(rootPath, "*.meta");

    foreach (string path in paths)
    {
        using var stream = File.OpenRead(path);
        using var reader = new StreamReader(stream);

        DocumentMetadata? meta = JsonSerializer.Deserialize<DocumentMetadata>(reader.ReadToEnd());
        if (meta is not null)
        {
            result.Accept(meta);
        }
    }

    return result;
}
```

The developer expected each file handle to close after each iteration. What does C# 8 `using var` actually do here, and how do you fix it?

---

**Answer:**

```csharp
public BatchResult IngestFolder(string rootPath)
{
    var result = new BatchResult();
    IEnumerable<string> paths = Directory.EnumerateFiles(rootPath, "*.meta");

    foreach (string path in paths)
    {
        using var stream = File.OpenRead(path);
        using var reader = new StreamReader(stream);

        DocumentMetadata? meta = JsonSerializer.Deserialize<DocumentMetadata>(reader.ReadToEnd());
        if (meta is not null)
        {
            result.Accept(meta);
        }
    }

    return result;
}
```

The developer expected each file handle to close after each iteration. What does C# 8 `using var` actually do here, and how do you fix it?

**Answer:** `using var` disposes at the end of the **enclosing scope** — here the entire `foreach` block — not at the end of each iteration. Every opened `FileStream` stays alive until the loop completes, exhausting file descriptors on large folders.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Resource lifetime | `using var` scope = whole `foreach` block | Thousands of concurrent open handles — "too many open files" |
| Memory | Undisposed streams and readers | Native and managed buffers accumulate until batch end |
| Misconception | Treating `using var` like block-scoped `using (...)` per iteration | Classic C# 8 migration trap — **Program.cs** Section 5 notes end-of-scope disposal |

**Fix (priority order):**

1. Restore per-iteration block scope:

```csharp
foreach (string path in paths)
{
    using (var stream = File.OpenRead(path))
    using (var reader = new StreamReader(stream))
    {
        DocumentMetadata? meta = JsonSerializer.Deserialize<DocumentMetadata>(reader.ReadToEnd());
        if (meta is not null) result.Accept(meta);
    }
}
```

2. Or wrap the body in an explicit `{ … }` block and place `using var` inside that inner block so scope ends each iteration.
3. Keep `using var` at method level only when a single resource should live for the whole method — intentional pattern in **Program.cs** `DemonstrateUsingDeclarations`.
4. Remember reverse dispose order when multiple `using var` share a scope — last declared disposes first.

**Production takeaway:** `using var` scope follows ** braces**, not developer intent — in loops, prefer classic `using (...)` or an inner block per iteration.

---

---

#### Q7. (D) A 400-project solution enables nullable reference types repo-wide. CI surfaces 8,000 warnings; developers blanket `#nullable disable` on touched files and sprinkle `!` to merge PRs. As tech lead, what migration strategy do you recommend for a document-archive domain with heavy `string` metadata (IDs, paths, optional notes), and where do `string?`, null checks, and `[NotNullWhen]` belong versus suppressions?



**Answer:**

_Answer not found._

---
