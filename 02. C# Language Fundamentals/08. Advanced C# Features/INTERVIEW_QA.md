END MARKER NOT FOUND: ## Q3. Explain 	ypeof(T) vs obj.GetType()"

 =  # 08. Advanced C# Features — Interview Q&A
> Back to [README](../README.md)

## Table of Contents

- [01. Serialization & Deserialization](#01-serialization-deserialization)
  - [Q1. What is serialization and deserialization, and what is an object graph?](#q1-what-is-serialization-and-deserialization-and-what-is-an-object-graph)
  - [Q2. Does deserialization resurrect original object identity or create new instances?](#q2-does-deserialization-resurrect-original-object-identity-or-create-new-instances)
  - [Q3. Compare JSON, XML, and binary as wire formats — trade-offs for APIs, config, and storage.](#q3-compare-json-xml-and-binary-as-wire-formats-trade-offs-for-apis-config-and-storage)
  - [Q4. Explain `System.Text.Json.JsonSerializer.Serialize` and `Deserialize` for files and streams.](#q4-explain-systemtextjsonjsonserializerserialize-and-deserialize-for-files-and-streams)
  - [Q5. What is `JsonSerializerOptions`, and which settings affect naming, indentation, and enum handling?](#q5-what-is-jsonserializeroptions-and-which-settings-affect-naming-indentation-and-enum-handling)
  - [Q6. Why is creating a new `JsonSerializerOptions` on every call a performance problem?](#q6-why-is-creating-a-new-jsonserializeroptions-on-every-call-a-performance-problem)
  - [Q7. When should you use `JsonSerializerContext` source generators vs reflection-based serialization?](#q7-when-should-you-use-jsonserializercontext-source-generators-vs-reflection-based-serialization)
  - [Q8. Explain `[JsonPropertyName]`, `[JsonIgnore]`, `[JsonInclude]`, and `[JsonPropertyOrder]`.](#q8-explain-jsonpropertyname-jsonignore-jsoninclude-and-jsonpropertyorder)
  - [Q9. What happens when JSON contains properties not present on the C# type (extra members)?](#q9-what-happens-when-json-contains-properties-not-present-on-the-c-type-extra-members)
  - [Q10. What happens when JSON is missing a property mapped to a non-nullable reference type vs a value type?](#q10-what-happens-when-json-is-missing-a-property-mapped-to-a-non-nullable-reference-type-vs-a-value-type)
  - [Q11. How do enums serialize by default in `System.Text.Json`, and what production risk does numeric enum wire format create?](#q11-how-do-enums-serialize-by-default-in-systemtextjson-and-what-production-risk-does-numeric-enum-wire-format-create)
  - [Q12. How do you serialize enums as strings using `JsonStringEnumConverter`?](#q12-how-do-you-serialize-enums-as-strings-using-jsonstringenumconverter)
  - [Q13. How do you handle circular references in an object graph (`ReferenceHandler.Preserve` / `IgnoreCycles`)?](#q13-how-do-you-handle-circular-references-in-an-object-graph-referencehandlerpreserve-ignorecycles)
  - [Q14. Why does serializing `Animal pet = new Dog()` sometimes drop `Dog`-only properties?](#q14-why-does-serializing-animal-pet-new-dog-sometimes-drop-dog-only-properties)
  - [Q15. How do you enable polymorphic serialization in modern `System.Text.Json`?](#q15-how-do-you-enable-polymorphic-serialization-in-modern-systemtextjson)
  - [Q16. What is `JsonNode`, `JsonObject`, and `JsonArray`, and when prefer them over strongly typed models?](#q16-what-is-jsonnode-jsonobject-and-jsonarray-and-when-prefer-them-over-strongly-typed-models)
  - [Q17. How do you navigate and mutate JSON with `JsonNode` without deserializing to a fixed class?](#q17-how-do-you-navigate-and-mutate-json-with-jsonnode-without-deserializing-to-a-fixed-class)
  - [Q18. What is a custom `JsonConverter<T>`, and when would you implement `Read`/`Write` manually?](#q18-what-is-a-custom-jsonconvertert-and-when-would-you-implement-readwrite-manually)
  - [Q19. How do `Utf8JsonReader` and `Utf8JsonWriter` differ from `JsonSerializer` helpers?](#q19-how-do-utf8jsonreader-and-utf8jsonwriter-differ-from-jsonserializer-helpers)
  - [Q20. Explain `XmlSerializer` requirements (parameterless constructor, public read/write properties).](#q20-explain-xmlserializer-requirements-parameterless-constructor-public-readwrite-properties)
  - [Q21. What do `[XmlRoot]`, `[XmlElement]`, `[XmlAttribute]`, and `[XmlIgnore]` control?](#q21-what-do-xmlroot-xmlelement-xmlattribute-and-xmlignore-control)
  - [Q22. Why can `XmlSerializer` fail at runtime even when the project compiles?](#q22-why-can-xmlserializer-fail-at-runtime-even-when-the-project-compiles)
  - [Q23. What is `[Serializable]` actually used for in modern .NET?](#q23-what-is-serializable-actually-used-for-in-modern-net)
  - [Q24. Explain `BinaryFormatter` — why is it obsolete, and what security risks led to its removal?](#q24-explain-binaryformatter-why-is-it-obsolete-and-what-security-risks-led-to-its-removal)
  - [Q25. What are recommended modern alternatives to `BinaryFormatter` for trusted internal persistence?](#q25-what-are-recommended-modern-alternatives-to-binaryformatter-for-trusted-internal-persistence)
  - [Q26. How does `DateTime` with unspecified `Kind` behave across time zones during serialization?](#q26-how-does-datetime-with-unspecified-kind-behave-across-time-zones-during-serialization)
  - [Q27. Why is `DateTimeOffset` often safer on the wire than `DateTime`?](#q27-why-is-datetimeoffset-often-safer-on-the-wire-than-datetime)
  - [Q28. Can a type with only get-only properties serialize but fail to deserialize? Why?](#q28-can-a-type-with-only-get-only-properties-serialize-but-fail-to-deserialize-why)
  - [Q29. How do `[JsonConstructor]` and parameterized constructors interact with deserialization?](#q29-how-do-jsonconstructor-and-parameterized-constructors-interact-with-deserialization)
  - [Q30. What is the difference between `System.Text.Json` and `Newtonsoft.Json` feature sets (contract customization, references)?](#q30-what-is-the-difference-between-systemtextjson-and-newtonsoftjson-feature-sets-contract-customization-references)

- [02. Reflection & Attributes](#02-reflection-attributes)
  - [Q1. What is reflection in C#, and what problems does it solve?](#q1-what-is-reflection-in-c-and-what-problems-does-it-solve)
  - [Q2. What is the difference between early binding and late binding?](#q2-what-is-the-difference-between-early-binding-and-late-binding)
  - [Q3. Explain `typeof(T)` vs `obj.GetType()` — compile-time token vs runtime type.](#q3-explain-typeoft-vs-objgettype-compile-time-token-vs-runtime-type)
  - [Q4. Why does `typeof(List<int>) == typeof(List<string>)` return false?](#q4-why-does-typeoflistint-typeofliststring-return-false)
  - [Q5. How do you obtain the generic type definition from a closed generic type?](#q5-how-do-you-obtain-the-generic-type-definition-from-a-closed-generic-type)
  - [Q6. What is the `Type` class, and what members expose metadata (methods, properties, fields, attributes)?](#q6-what-is-the-type-class-and-what-members-expose-metadata-methods-properties-fields-attributes)
  - [Q7. What is `BindingFlags`, and how do `Instance`, `Static`, `Public`, `NonPublic`, and `DeclaredOnly` combine?](#q7-what-is-bindingflags-and-how-do-instance-static-public-nonpublic-and-declaredonly-combine)
  - [Q8. How do you enumerate properties, methods, fields, and constructors with reflection?](#q8-how-do-you-enumerate-properties-methods-fields-and-constructors-with-reflection)
  - [Q9. How do you invoke a method dynamically via `MethodInfo.Invoke`?](#q9-how-do-you-invoke-a-method-dynamically-via-methodinfoinvoke)
  - [Q10. What is `Activator.CreateInstance`, and how do you pass constructor arguments?](#q10-what-is-activatorcreateinstance-and-how-do-you-pass-constructor-arguments)
  - [Q11. How do you create generic types at runtime (`MakeGenericType`) and invoke generic methods (`MakeGenericMethod`)?](#q11-how-do-you-create-generic-types-at-runtime-makegenerictype-and-invoke-generic-methods-makegenericmethod)
  - [Q12. How do you get and set property and field values through `PropertyInfo` / `FieldInfo`?](#q12-how-do-you-get-and-set-property-and-field-values-through-propertyinfo-fieldinfo)
  - [Q13. How can reflection access private members, and why is that a maintenance and security concern?](#q13-how-can-reflection-access-private-members-and-why-is-that-a-maintenance-and-security-concern)
  - [Q14. Explain `Assembly`, `Module`, `MemberInfo`, `MethodInfo`, `PropertyInfo`, and `FieldInfo` relationships.](#q14-explain-assembly-module-memberinfo-methodinfo-propertyinfo-and-fieldinfo-relationships)
  - [Q15. What is the difference between `Assembly.Load`, `Assembly.LoadFrom`, and `AssemblyLoadContext`?](#q15-what-is-the-difference-between-assemblyload-assemblyloadfrom-and-assemblyloadcontext)
  - [Q16. Can you unload an assembly in .NET Framework vs .NET Core / .NET 5+?](#q16-can-you-unload-an-assembly-in-net-framework-vs-net-core-net-5)
  - [Q17. How do you discover and read custom attributes at runtime (`GetCustomAttribute`, `IsDefined`)?](#q17-how-do-you-discover-and-read-custom-attributes-at-runtime-getcustomattribute-isdefined)
  - [Q18. How do you define and apply your own attribute classes (`AttributeUsage`)?](#q18-how-do-you-define-and-apply-your-own-attribute-classes-attributeusage)
  - [Q19. What are performance costs of reflection vs compiled code, and how do trimming/AOT affect it?](#q19-what-are-performance-costs-of-reflection-vs-compiled-code-and-how-do-trimmingaot-affect-it)
  - [Q20. What is `Reflection.Emit`, and when is dynamic IL generation justified?](#q20-what-is-reflectionemit-and-when-is-dynamic-il-generation-justified)
  - [Q21. How does reflection interact with nullable reference type annotations?](#q21-how-does-reflection-interact-with-nullable-reference-type-annotations)
  - [Q22. What security permissions historically gated reflection, and what changed in modern .NET?](#q22-what-security-permissions-historically-gated-reflection-and-what-changed-in-modern-net)

- [03. Regular Expressions](#03-regular-expressions)
  - [Q1. What is the purpose of the `Regex` class in C#?](#q1-what-is-the-purpose-of-the-regex-class-in-c)
  - [Q2. Explain `Regex.IsMatch`, `Match`, `Matches`, `Replace`, and `Split`.](#q2-explain-regexismatch-match-matches-replace-and-split)
  - [Q3. What is the difference between verbatim regex strings (`@"\d+"`) and escaped regular strings?](#q3-what-is-the-difference-between-verbatim-regex-strings-d-and-escaped-regular-strings)
  - [Q4. What are common metacharacters candidates should know (`.`, `*`, `+`, `?`, `^`, `$`, `\d`, `\w`, groups)?](#q4-what-are-common-metacharacters-candidates-should-know-d-w-groups)
  - [Q5. What is catastrophic backtracking, and what pattern shapes trigger it?](#q5-what-is-catastrophic-backtracking-and-what-pattern-shapes-trigger-it)
  - [Q6. How do you mitigate ReDoS using `Regex.MatchTimeout` or the `matchTimeout` parameter in .NET?](#q6-how-do-you-mitigate-redos-using-regexmatchtimeout-or-the-matchtimeout-parameter-in-net)
  - [Q7. What happens when a regex times out — which exception is thrown?](#q7-what-happens-when-a-regex-times-out-which-exception-is-thrown)
  - [Q8. What is atomic grouping's role in preventing backtracking explosions?](#q8-what-is-atomic-groupings-role-in-preventing-backtracking-explosions)
  - [Q9. How does culture affect case-insensitive matching, and what does `RegexOptions.CultureInvariant` do?](#q9-how-does-culture-affect-case-insensitive-matching-and-what-does-regexoptionscultureinvariant-do)
  - [Q10. When should you compile regexes with `RegexOptions.Compiled` (or source generators in .NET 7+)?](#q10-when-should-you-compile-regexes-with-regexoptionscompiled-or-source-generators-in-net-7)
  - [Q11. What is `RegexOptions.NonBacktracking` (.NET 7+), and what trade-offs does it have?](#q11-what-is-regexoptionsnonbacktracking-net-7-and-what-trade-offs-does-it-have)
  - [Q12. How do named capture groups work, and how do you read them from `Match.Groups`?](#q12-how-do-named-capture-groups-work-and-how-do-you-read-them-from-matchgroups)
  - [Q13. What is the difference between greedy and lazy quantifiers (`+` vs `+?`)?](#q13-what-is-the-difference-between-greedy-and-lazy-quantifiers-vs)
  - [Q14. When should you prefer `Regex` over simple `string.Contains` / `Split` for maintainability?](#q14-when-should-you-prefer-regex-over-simple-stringcontains-split-for-maintainability)
  - [Q15. How do you validate input with regex without using it as a full parser (e.g., email, phone)?](#q15-how-do-you-validate-input-with-regex-without-using-it-as-a-full-parser-eg-email-phone)

- [04. Var, Dynamic & Special Keywords](#04-var-dynamic-special-keywords)
  - [Q1. Explain `var` — what is known at compile time vs runtime?](#q1-explain-var-what-is-known-at-compile-time-vs-runtime)
  - [Q2. When is `var` required (anonymous types) vs merely convenient?](#q2-when-is-var-required-anonymous-types-vs-merely-convenient)
  - [Q3. Explain the `dynamic` keyword and the DLR's role.](#q3-explain-the-dynamic-keyword-and-the-dlrs-role)
  - [Q4. What is the difference between `var`, `dynamic`, and `object`?](#q4-what-is-the-difference-between-var-dynamic-and-object)
  - [Q5. When does `dynamic` defer member binding to runtime, and what errors appear only then?](#q5-when-does-dynamic-defer-member-binding-to-runtime-and-what-errors-appear-only-then)
  - [Q6. What is `DynamicObject`, and when would you subclass it?](#q6-what-is-dynamicobject-and-when-would-you-subclass-it)
  - [Q7. What is `ExpandoObject`, and how does it differ from `Dictionary<string, object>`?](#q7-what-is-expandoobject-and-how-does-it-differ-from-dictionarystring-object)
  - [Q8. Explain `nameof` — how does it help refactoring and logging?](#q8-explain-nameof-how-does-it-help-refactoring-and-logging)
  - [Q9. What is the `global::` qualifier, and when is it needed to disambiguate namespaces?](#q9-what-is-the-global-qualifier-and-when-is-it-needed-to-disambiguate-namespaces)
  - [Q10. What is `default` literal (C# 7.1+) vs `default(T)`?](#q10-what-is-default-literal-c-71-vs-defaultt)
  - [Q11. What is `@` verbatim identifier syntax (`@class`, `@event`) used for?](#q11-what-is-verbatim-identifier-syntax-class-event-used-for)
  - [Q12. What is unsafe code, and when are pointers justified in C#?](#q12-what-is-unsafe-code-and-when-are-pointers-justified-in-c)
  - [Q13. What is `stackalloc`, and how does it relate to performance-sensitive code?](#q13-what-is-stackalloc-and-how-does-it-relate-to-performance-sensitive-code)
  - [Q14. What is `ref readonly` return, and how does it differ from returning by value?](#q14-what-is-ref-readonly-return-and-how-does-it-differ-from-returning-by-value)
  - [Q15. How does `dynamic` interact with extension methods (why don't they bind dynamically)?](#q15-how-does-dynamic-interact-with-extension-methods-why-dont-they-bind-dynamically)

- [05. C# 7 Features](#05-c-7-features)
  - [Q1. What are tuple deconstruction and named tuple elements?](#q1-what-are-tuple-deconstruction-and-named-tuple-elements)
  - [Q2. How do `out` variables declared inline in method calls work?](#q2-how-do-out-variables-declared-inline-in-method-calls-work)
  - [Q3. What are discards (`_`), and where are they used (deconstruction, unused returns)?](#q3-what-are-discards-_-and-where-are-they-used-deconstruction-unused-returns)
  - [Q4. Explain pattern matching enhancements in C# 7 — `is` type patterns and `switch` patterns.](#q4-explain-pattern-matching-enhancements-in-c-7-is-type-patterns-and-switch-patterns)
  - [Q5. What are `ref` returns and `ref` locals, and what safety rules apply?](#q5-what-are-ref-returns-and-ref-locals-and-what-safety-rules-apply)
  - [Q6. What is `ref`/`in`/`out` in the context of `ReadOnlySpan`-era performance APIs (conceptual link)?](#q6-what-is-refinout-in-the-context-of-readonlyspan-era-performance-apis-conceptual-link)
  - [Q7. What are local functions, and how do they differ from lambdas for recursion and capture?](#q7-what-are-local-functions-and-how-do-they-differ-from-lambdas-for-recursion-and-capture)
  - [Q8. What are expression-bodied members beyond properties (methods, constructors, finalizers)?](#q8-what-are-expression-bodied-members-beyond-properties-methods-constructors-finalizers)
  - [Q9. What binary literals and digit separators (`0b1010`, `1_000_000`) improve in readability?](#q9-what-binary-literals-and-digit-separators-0b1010-1_000_000-improve-in-readability)
  - [Q10. What is `throw` as an expression inside ternary/null-coalescing forms?](#q10-what-is-throw-as-an-expression-inside-ternarynull-coalescing-forms)
  - [Q11. How do generalized async return types work (`ValueTask` as async return)?](#q11-how-do-generalized-async-return-types-work-valuetask-as-async-return)
  - [Q12. What are `default` in generic constraints improvements in C# 7?](#q12-what-are-default-in-generic-constraints-improvements-in-c-7)

- [06. C# 8 Features](#06-c-8-features)
  - [Q1. Explain nullable reference types — how do they differ from `Nullable<T>` value types?](#q1-explain-nullable-reference-types-how-do-they-differ-from-nullablet-value-types)
  - [Q2. What do `?`, `!`, and `#nullable` directives mean at compile time?](#q2-what-do-and-nullable-directives-mean-at-compile-time)
  - [Q3. Are nullable reference annotations enforced at runtime?](#q3-are-nullable-reference-annotations-enforced-at-runtime)
  - [Q4. What are default interface methods, and how do they relate to the diamond problem?](#q4-what-are-default-interface-methods-and-how-do-they-relate-to-the-diamond-problem)
  - [Q5. What are asynchronous streams (`IAsyncEnumerable<T>` and `await foreach`)?](#q5-what-are-asynchronous-streams-iasyncenumerablet-and-await-foreach)
  - [Q6. Explain null-coalescing assignment (`??=`) with examples.](#q6-explain-null-coalescing-assignment-with-examples)
  - [Q7. Explain range (`..`) and index (`^`) operators — how does `^1` differ from `Length - 1`?](#q7-explain-range-and-index-operators-how-does-1-differ-from-length---1)
  - [Q8. What are `using` declarations vs `using` statements for IDisposable?](#q8-what-are-using-declarations-vs-using-statements-for-idisposable)
  - [Q9. What are nullable-aware APIs in the BCL reacting to NRT (`NotNullWhen`, `MaybeNull`)?](#q9-what-are-nullable-aware-apis-in-the-bcl-reacting-to-nrt-notnullwhen-maybenull)
  - [Q10. What is `IAsyncDisposable`, and how does `await using` work?](#q10-what-is-iasyncdisposable-and-how-does-await-using-work)
  - [Q11. What are static local functions, and why were they added?](#q11-what-are-static-local-functions-and-why-were-they-added)
  - [Q12. What is a `readonly struct`, and what mutability restrictions apply to its members?](#q12-what-is-a-readonly-struct-and-what-mutability-restrictions-apply-to-its-members)
  - [Q13. What is the `readonly` modifier on struct instance members (C# 8)?](#q13-what-is-the-readonly-modifier-on-struct-instance-members-c-8)
  - [Q14. What are stackalloc in safe contexts and `Span<T>` integrations introduced alongside C# 8?](#q14-what-are-stackalloc-in-safe-contexts-and-spant-integrations-introduced-alongside-c-8)
  - [Q15. What is target-typed `new()` vs explicit type names?](#q15-what-is-target-typed-new-vs-explicit-type-names)

- [Cross-chapter — Records & Pattern Matching *(C# 9–11; grouped here)*](#cross-chapter-records-pattern-matching-c-911-grouped-here)
  - [Q1. What are records (C# 9), and what boilerplate do they synthesize?](#q1-what-are-records-c-9-and-what-boilerplate-do-they-synthesize)
  - [Q2. What is the difference between `record class` and `record struct`?](#q2-what-is-the-difference-between-record-class-and-record-struct)
  - [Q3. How does value-based equality in records differ from default class equality?](#q3-how-does-value-based-equality-in-records-differ-from-default-class-equality)
  - [Q4. What is the difference between positional records and records with manual properties?](#q4-what-is-the-difference-between-positional-records-and-records-with-manual-properties)
  - [Q5. Explain `with` expressions — how do they relate to non-destructive mutation?](#q5-explain-with-expressions-how-do-they-relate-to-non-destructive-mutation)
  - [Q6. What are init-only setters (`init`), and how do they differ from `{ get; set; }` and `{ get; }`?](#q6-what-are-init-only-setters-init-and-how-do-they-differ-from-get-set-and-get)
  - [Q7. Can init-only properties be set inside the type's constructors after object creation semantics?](#q7-can-init-only-properties-be-set-inside-the-types-constructors-after-object-creation-semantics)
  - [Q8. What is primary constructor syntax for records/classes (C# 12 preview cross-ref) vs positional records?](#q8-what-is-primary-constructor-syntax-for-recordsclasses-c-12-preview-cross-ref-vs-positional-records)
  - [Q9. What is pattern matching in modern C# beyond C# 7 — switch expressions, relational, logical, and property patterns?](#q9-what-is-pattern-matching-in-modern-c-beyond-c-7-switch-expressions-relational-logical-and-property-patterns)
  - [Q10. Explain property patterns (`person is { Age: > 18, Name: var n }`).](#q10-explain-property-patterns-person-is-age-18-name-var-n)
  - [Q11. What are relational patterns (`>`, `<=`) and combinator patterns (`and`, `or`, `not`)?](#q11-what-are-relational-patterns-and-combinator-patterns-and-or-not)
  - [Q12. What is list patterns (C# 11) — `[_, .., var last]`?](#q12-what-is-list-patterns-c-11-_-var-last)
  - [Q13. What is `switch` expression vs traditional `switch` statement for exhaustiveness?](#q13-what-is-switch-expression-vs-traditional-switch-statement-for-exhaustiveness)
  - [Q14. What happens when a `switch` expression is not exhaustive over an enum?](#q14-what-happens-when-a-switch-expression-is-not-exhaustive-over-an-enum)
  - [Q15. What is the difference between `is null` and `== null` when a type overloads `==`?](#q15-what-is-the-difference-between-is-null-and-null-when-a-type-overloads)
  - [Q16. What are expression trees (`Expression<T>`), and how do they differ from delegates?](#q16-what-are-expression-trees-expressiont-and-how-do-they-differ-from-delegates)
  - [Q17. How are expression trees used by LINQ providers (EF Core, `IQueryable`)?](#q17-how-are-expression-trees-used-by-linq-providers-ef-core-iqueryable)
  - [Q18. Why can't all C# lambdas be converted to expression trees?](#q18-why-cant-all-c-lambdas-be-converted-to-expression-trees)
  - [Q19. What is the difference between compile-time constant patterns and runtime type patterns?](#q19-what-is-the-difference-between-compile-time-constant-patterns-and-runtime-type-patterns)
  - [Q20. When should you prefer records over classes for DTOs and domain models?](#q20-when-should-you-prefer-records-over-classes-for-dtos-and-domain-models)
  - [Q21. **`typeof` vs `GetType()`** — `typeof(Base)` is known at compile time; `instance.GetType()` returns the actual runtime derived type.](#q21-typeof-vs-gettype-typeofbase-is-known-at-compile-time-instancegettype-returns-the-actual-runtime-derived-type)
  - [Q22. **Serialization type loss** — Assigning `Animal ref = new Dog()` and serializing as `Animal` drops derived-only properties unless polymorphism is configured.](#q22-serialization-type-loss-assigning-animal-ref-new-dog-and-serializing-as-animal-drops-derived-only-properties-unless-polymorphism-is-configured)
  - [Q23. **`[Serializable]` ignored by System.Text.Json** — Candidates conflate legacy binary markers with modern JSON/XML serializers.](#q23-serializable-ignored-by-systemtextjson-candidates-conflate-legacy-binary-markers-with-modern-jsonxml-serializers)
  - [Q24. **`BinaryFormatter` is a security footgun** — Deserializing untrusted payloads enables remote code execution; obsolete/removed on modern .NET.](#q24-binaryformatter-is-a-security-footgun-deserializing-untrusted-payloads-enables-remote-code-execution-obsoleteremoved-on-modern-net)
  - [Q25. **Missing JSON property on non-nullable value type** — Deserialization may default the value silently; missing `required`/`[JsonRequired]` validation causes subtle bugs.](#q25-missing-json-property-on-non-nullable-value-type-deserialization-may-default-the-value-silently-missing-requiredjsonrequired-validation-causes-subtle-bugs)
  - [Q26. **Enum numeric wire values** — Renumbering enum members breaks persisted JSON; prefer string enums for long-lived contracts.](#q26-enum-numeric-wire-values-renumbering-enum-members-breaks-persisted-json-prefer-string-enums-for-long-lived-contracts)
  - [Q27. **`JsonSerializerOptions` not thread-safe for mutation** — Cache a configured instance; do not tweak shared options concurrently.](#q27-jsonserializeroptions-not-thread-safe-for-mutation-cache-a-configured-instance-do-not-tweak-shared-options-concurrently)
  - [Q28. **`dynamic` hides errors until runtime** — Misspelled members compile; also blocks many refactorings and overload resolution surprises.](#q28-dynamic-hides-errors-until-runtime-misspelled-members-compile-also-blocks-many-refactorings-and-overload-resolution-surprises)
  - [Q29. **Extension methods do not dispatch on `dynamic`** — Must cast to static type or call like static methods.](#q29-extension-methods-do-not-dispatch-on-dynamic-must-cast-to-static-type-or-call-like-static-methods)
  - [Q30. **Reflection string names don't refactor** — Renaming a property breaks reflection unless tests catch it.](#q30-reflection-string-names-dont-refactor-renaming-a-property-breaks-reflection-unless-tests-catch-it)
  - [Q31. **Regex without timeout on user input** — Crafted input can hang the process via catastrophic backtracking.](#q31-regex-without-timeout-on-user-input-crafted-input-can-hang-the-process-via-catastrophic-backtracking)
  - [Q32. **Nullable reference types are annotations only** — `#nullable enable` does not stop null at runtime without guards.](#q32-nullable-reference-types-are-annotations-only-nullable-enable-does-not-stop-null-at-runtime-without-guards)
  - [Q33. **Records are still reference types (`record class`)** — Identity semantics differ from `record struct`; boxing/equality surprises follow.](#q33-records-are-still-reference-types-record-class-identity-semantics-differ-from-record-struct-boxingequality-surprises-follow)
  - [Q34. **Expression trees cannot contain statements arbitrarily** — Many C# constructs are not translatable for EF/LINQ providers.](#q34-expression-trees-cannot-contain-statements-arbitrarily-many-c-constructs-are-not-translatable-for-eflinq-providers)

- [03. Regular Expressions](#03-regular-expressions-1)
  - [Q1. `Regex.IsMatch(email, pattern)` inline in the action](#q1-regexismatchemail-pattern-inline-in-the-action)
  - [Q2. `static readonly Regex` field with `RegexOptions.Compiled | RegexOptions.CultureInvariant`](#q2-static-readonly-regex-field-with-regexoptionscompiled-regexoptionscultureinvariant)
  - [Q3. `[RegularExpression(@"…")]` on the DTO property](#q3-regularexpression-on-the-dto-property)
  - [Q1. (R) A Redis-backed session service deserializes cached JSON on every request. Review this code — what breaks under load or attack, and what do you fix first?](#q1-r-a-redis-backed-session-service-deserializes-cached-json-on-every-request-review-this-code-what-breaks-under-load-or-attack-and-what-do-you-fix-first)
  - [Q2. (R) After deploying a new order endpoint, mobile clients get `400`/`500` on deserialize while Postman with the old payload works. Review the handler:](#q2-r-after-deploying-a-new-order-endpoint-mobile-clients-get-400500-on-deserialize-while-postman-with-the-old-payload-works-review-the-handler)
  - [Q3. (R) A partner integration writes invoice lines to XML nightly; the job fails on first deploy with `InvalidOperationException`. Review the model and serializer usage:](#q3-r-a-partner-integration-writes-invoice-lines-to-xml-nightly-the-job-fails-on-first-deploy-with-invalidoperationexception-review-the-model-and-serializer-usage)
  - [Q4. (P) Production still reads `.bin` session files produced years ago with `BinaryFormatter`. You must migrate to System.Text.Json without taking downtime. What is your rollout strategy, and why is "just flip a switch" unsafe?](#q4-p-production-still-reads-bin-session-files-produced-years-ago-with-binaryformatter-you-must-migrate-to-systemtextjson-without-taking-downtime-what-is-your-rollout-strategy-and-why-is-just-flip-a-switch-unsafe)
  - [Q5. (D) You are adding an optional `IsVerified` flag to a public REST DTO. v1 clients never send the field; v2 clients may send `true`, `false`, or omit it. You need to distinguish "not verified yet" from "explicitly false." Should the property be `bool` or `bool?`, and how does System.Text.Json treat a missing member for each?](#q5-d-you-are-adding-an-optional-isverified-flag-to-a-public-rest-dto-v1-clients-never-send-the-field-v2-clients-may-send-true-false-or-omit-it-you-need-to-distinguish-not-verified-yet-from-explicitly-false-should-the-property-be-bool-or-bool-and-how-does-systemtextjson-treat-a-missing-member-for-each)
  - [Q6. (M) An org-chart API returns departments with parent/child links wired both ways. A developer enables cycle handling and ships:](#q6-m-an-org-chart-api-returns-departments-with-parentchild-links-wired-both-ways-a-developer-enables-cycle-handling-and-ships)
  - [Q7. (R) A config-sync worker reads JSON settings files from a shared folder (any authenticated internal user can drop files). Review the ingestion path:](#q7-r-a-config-sync-worker-reads-json-settings-files-from-a-shared-folder-any-authenticated-internal-user-can-drop-files-review-the-ingestion-path)

- [02. Reflection & Attributes](#02-reflection-attributes-1)

- [02. Reflection & Attributes](#02-reflection-attributes-2)
  - [Q1. (R) A warehouse API loads pricing plugins from a separate assembly at runtime. It works on a developer machine but `LoadPlugin` always returns null in staging. Review the loader:](#q1-r-a-warehouse-api-loads-pricing-plugins-from-a-separate-assembly-at-runtime-it-works-on-a-developer-machine-but-loadplugin-always-returns-null-in-staging-review-the-loader)
  - [Q2. (R) A metadata-driven audit interceptor invokes controller actions and logs failures, but operators only see `TargetInvocationException` in Splunk — never the real fault. Review the handler:](#q2-r-a-metadata-driven-audit-interceptor-invokes-controller-actions-and-logs-failures-but-operators-only-see-targetinvocationexception-in-splunk-never-the-real-fault-review-the-handler)
  - [Q3. (R) After a rename refactor from `CalculateLineTotal` to `CalculateOrderTotal`, order totals silently become zero in production. Review the pricing service:](#q3-r-after-a-rename-refactor-from-calculatelinetotal-to-calculateordertotal-order-totals-silently-become-zero-in-production-review-the-pricing-service)
  - [Q4. (R) A margin-report job reads private cost fields from `CostRecord`-like DTOs but always gets null and skips rows. Review the extractor:](#q4-r-a-margin-report-job-reads-private-cost-fields-from-costrecord-like-dtos-but-always-gets-null-and-skips-rows-review-the-extractor)
  - [Q5. (P) An ASP.NET Core API discovers `[EntityTable]`-decorated export types by scanning `Assembly.GetExecutingAssembly().GetTypes()` at startup. After enabling `<PublishTrimmed>true</PublishTrimmed>` for a Native AOT experiment, several entity types vanish from the export manifest with no compile errors. Why does trimming break this pattern, and what production-safe alternatives exist?](#q5-p-an-aspnet-core-api-discovers-entitytable-decorated-export-types-by-scanning-assemblygetexecutingassemblygettypes-at-startup-after-enabling-publishtrimmedtruepublishtrimmed-for-a-native-aot-experiment-several-entity-types-vanish-from-the-export-manifest-with-no-compile-errors-why-does-trimming-break-this-pattern-and-what-production-safe-alternatives-exist)
  - [Q6. (D) Your team ships a CSV export endpoint. One developer scans every request with `type.GetProperties()` and `GetCustomAttribute<ExportableAttribute>()` (same pattern as `ExportManifestBuilder` in this chapter). Another caches `PropertyInfo[]` and attribute metadata in a `ConcurrentDictionary<Type, ExportColumn[]>` built once at startup. Under 500 RPS with 40 exportable properties per row, which approach do you choose and why?](#q6-d-your-team-ships-a-csv-export-endpoint-one-developer-scans-every-request-with-typegetproperties-and-getcustomattributeexportableattribute-same-pattern-as-exportmanifestbuilder-in-this-chapter-another-caches-propertyinfo-and-attribute-metadata-in-a-concurrentdictionarytype-exportcolumn-built-once-at-startup-under-500-rps-with-40-exportable-properties-per-row-which-approach-do-you-choose-and-why)
  - [Q7. (M) A `PremiumProduct : Product` subclass is added for a loyalty tier. The ORM layer reads `[EntityTable]` from the base `Product` type to resolve table names. Export works for `Product` but `PremiumProduct` rows fail with "table not mapped." Given this attribute definition from the tutorial:](#q7-m-a-premiumproduct-product-subclass-is-added-for-a-loyalty-tier-the-orm-layer-reads-entitytable-from-the-base-product-type-to-resolve-table-names-export-works-for-product-but-premiumproduct-rows-fail-with-table-not-mapped-given-this-attribute-definition-from-the-tutorial)

- [03. Regular Expressions](#03-regular-expressions-2)

- [03. Regular Expressions](#03-regular-expressions-3)
  - [Q1. (R) A bulk-import API validates thousands of customer rows per request. After deploy, CPU spikes and some requests time out. Review this validator and prioritize fixes.](#q1-r-a-bulk-import-api-validates-thousands-of-customer-rows-per-request-after-deploy-cpu-spikes-and-some-requests-time-out-review-this-validator-and-prioritize-fixes)
  - [Q2. (R) A support portal lets agents paste a custom regex to search and redact matches in uploaded log files (multi-MB). Review this endpoint helper:](#q2-r-a-support-portal-lets-agents-paste-a-custom-regex-to-search-and-redact-matches-in-uploaded-log-files-multi-mb-review-this-endpoint-helper)
  - [Q3. (R) A notes-processing job extracts phone fragments for a CRM sync. Review this extractor:](#q3-r-a-notes-processing-job-extracts-phone-fragments-for-a-crm-sync-review-this-extractor)
  - [Q4. (P) Your registration API validates email on every POST (~2k RPS). A teammate proposes three options:](#q4-p-your-registration-api-validates-email-on-every-post-2k-rps-a-teammate-proposes-three-options)
  - [Q5. (D) Product wants import rejection for disposable email domains (`mailinator.com`, `tempmail.org`, …) and a regex that only allows corporate TLDs. A developer merges the blocklist into one giant pattern:](#q5-d-product-wants-import-rejection-for-disposable-email-domains-mailinatorcom-tempmailorg-and-a-regex-that-only-allows-corporate-tlds-a-developer-merges-the-blocklist-into-one-giant-pattern)
  - [Q6. (M) A config-ingestion worker parses key/value lines from Windows-generated files. Keys on lines after the first never match:](#q6-m-a-config-ingestion-worker-parses-keyvalue-lines-from-windows-generated-files-keys-on-lines-after-the-first-never-match)
  - [Q7. (R) An internal tool "sanitizes" HTML fragments before storing them in a knowledge base:](#q7-r-an-internal-tool-sanitizes-html-fragments-before-storing-them-in-a-knowledge-base)

- [04. Var Dynamic & Special Keywords](#04-var-dynamic-special-keywords-1)

- [04. Var Dynamic & Special Keywords](#04-var-dynamic-special-keywords-2)
  - [Q1. (R) A warehouse integration service parses third-party CSV rows into `dynamic` bags before posting to inventory. It passes QA with two sample files but throws in production on the first malformed row. Review the mapper:](#q1-r-a-warehouse-integration-service-parses-third-party-csv-rows-into-dynamic-bags-before-posting-to-inventory-it-passes-qa-with-two-sample-files-but-throws-in-production-on-the-first-malformed-row-review-the-mapper)
  - [Q2. (R) A pricing dashboard uses `var` with LINQ and mutates the source collection between query definition and enumeration. Review:](#q2-r-a-pricing-dashboard-uses-var-with-linq-and-mutates-the-source-collection-between-query-definition-and-enumeration-review)
  - [Q3. (R) A generic repository uses `nameof` and `default` for reflection-based updates. After a refactor, updates silently stop working for value-type columns. Review:](#q3-r-a-generic-repository-uses-nameof-and-default-for-reflection-based-updates-after-a-refactor-updates-silently-stop-working-for-value-type-columns-review)
  - [Q4. (P) Your team ingests nightly plugin config from a legacy host that exposes JSON whose shape changes per warehouse (extra keys, missing booleans, numeric strings). A junior dev proposes `dynamic` + `ExpandoObject` for the entire pipeline; another proposes strongly typed records + `System.Text.Json` with `[JsonExtensionData]`. When is `dynamic` justified here, and what production risks push you toward typed or semi-typed models?](#q4-p-your-team-ingests-nightly-plugin-config-from-a-legacy-host-that-exposes-json-whose-shape-changes-per-warehouse-extra-keys-missing-booleans-numeric-strings-a-junior-dev-proposes-dynamic-expandoobject-for-the-entire-pipeline-another-proposes-strongly-typed-records-systemtextjson-with-jsonextensiondata-when-is-dynamic-justified-here-and-what-production-risks-push-you-toward-typed-or-semi-typed-models)
  - [Q5. (M) A background price-refresh worker should stop within seconds when ops clicks "Cancel" in the admin UI. The flag works in dev (single core, low load) but the worker occasionally runs for minutes in production. Review:](#q5-m-a-background-price-refresh-worker-should-stop-within-seconds-when-ops-clicks-cancel-in-the-admin-ui-the-flag-works-in-dev-single-core-low-load-but-the-worker-occasionally-runs-for-minutes-in-production-review)
  - [Q6. (D) Two teams share a `WarehouseAnalytics` namespace. Team A added a helper type named `Math` for domain-specific rounding; Team B assumed BCL `System.Math` in unqualified calls. Review the pricing snippet:](#q6-d-two-teams-share-a-warehouseanalytics-namespace-team-a-added-a-helper-type-named-math-for-domain-specific-rounding-team-b-assumed-bcl-systemmath-in-unqualified-calls-review-the-pricing-snippet)

- [05. C# 7 Features](#05-c-7-features-1)

- [05. C# 7 Features](#05-c-7-features-2)
  - [Q1. (R) Express VIP orders are routed to the standard express lane in production. Review this C# 7 switch with `when` guards (mirrors the warehouse routing demo):](#q1-r-express-vip-orders-are-routed-to-the-standard-express-lane-in-production-review-this-c-7-switch-with-when-guards-mirrors-the-warehouse-routing-demo)
  - [Q2. (R) A product lookup was optimized with `ValueTask<string>` for cache hits. Under retry logic, intermittent `InvalidOperationException` appears. Review:](#q2-r-a-product-lookup-was-optimized-with-valuetaskstring-for-cache-hits-under-retry-logic-intermittent-invalidoperationexception-appears-review)
  - [Q3. (R) A developer refactors inventory reservation to use C# 7 ref returns for in-place updates. The build fails; after a workaround it crashes in QA. Review:](#q3-r-a-developer-refactors-inventory-reservation-to-use-c-7-ref-returns-for-in-place-updates-the-build-fails-after-a-workaround-it-crashes-in-qa-review)
  - [Q4. (R) A CSV import pipeline uses C# 7 out variables. Finance sees rows with quantity `0` marked as successfully imported. Review:](#q4-r-a-csv-import-pipeline-uses-c-7-out-variables-finance-sees-rows-with-quantity-0-marked-as-successfully-imported-review)
  - [Q5. (P) A warehouse fulfillment microservice returns `(bool CanFulfill, string Note)` tuples from `CheckFulfillment` — the same shape as the tutorial's tuple demo. The team debates replacing tuples with a `FulfillmentResult` record before exposing the method on a public NuGet contract. When is the tuple idiomatic, and when does it break production maintainability?](#q5-p-a-warehouse-fulfillment-microservice-returns-bool-canfulfill-string-note-tuples-from-checkfulfillment-the-same-shape-as-the-tutorials-tuple-demo-the-team-debates-replacing-tuples-with-a-fulfillmentresult-record-before-exposing-the-method-on-a-public-nuget-contract-when-is-the-tuple-idiomatic-and-when-does-it-break-production-maintainability)
  - [Q6. (M) A batch job uses a local function with captured outer state to retry flaky lane assignments. Ops reports duplicate reservations on the same SKU after parallel batch splits. Review:](#q6-m-a-batch-job-uses-a-local-function-with-captured-outer-state-to-retry-flaky-lane-assignments-ops-reports-duplicate-reservations-on-the-same-sku-after-parallel-batch-splits-review)
  - [Q7. (D) Two teammates implement guard clauses for order validation. Which approach do you standardize on for a shared domain library, and why?](#q7-d-two-teammates-implement-guard-clauses-for-order-validation-which-approach-do-you-standardize-on-for-a-shared-domain-library-and-why)

- [06. C# 8 Features](#06-c-8-features-1)

- [06. C# 8 Features](#06-c-8-features-2)
  - [Q1. (R) After enabling `<Nullable>enable</Nullable>` on the document-ingest API, QA reports intermittent `NullReferenceException` on documents with no footnotes. Review the service:](#q1-r-after-enabling-nullableenablenullable-on-the-document-ingest-api-qa-reports-intermittent-nullreferenceexception-on-documents-with-no-footnotes-review-the-service)
  - [Q2. (R) A background worker streams archive pages to blob storage. Under deploy cancellation, the job keeps running for minutes and sometimes OOMs. Review the consumer and producer:](#q2-r-a-background-worker-streams-archive-pages-to-blob-storage-under-deploy-cancellation-the-job-keeps-running-for-minutes-and-sometimes-ooms-review-the-consumer-and-producer)
  - [Q3. (R) A routing microservice parses document IDs with C# 8 ranges after a format change. Production throws `ArgumentOutOfRangeException` on valid-looking IDs. Review:](#q3-r-a-routing-microservice-parses-document-ids-with-c-8-ranges-after-a-format-change-production-throws-argumentoutofrangeexception-on-valid-looking-ids-review)
  - [Q4. (R) A teammate refactors chunk parsing to overlap I/O with processing. The build fails and code review finds async/ref-struct mixing. Review:](#q4-r-a-teammate-refactors-chunk-parsing-to-overlap-io-with-processing-the-build-fails-and-code-review-finds-asyncref-struct-mixing-review)
  - [Q5. (P) Your team ships `IDocumentProcessor` as a shared NuGet package. Version 1 has `Process` and `ProcessorName`. Version 2 needs a `Describe()` helper without forcing every consumer to recompile. A consumer already implements both `IDocumentProcessor` and `IArchiveReporter`, each adding a default `Describe()`. How do you evolve the interface using C# 8 default interface methods, and what breaks if you ignore diamond ambiguity?](#q5-p-your-team-ships-idocumentprocessor-as-a-shared-nuget-package-version-1-has-process-and-processorname-version-2-needs-a-describe-helper-without-forcing-every-consumer-to-recompile-a-consumer-already-implements-both-idocumentprocessor-and-iarchivereporter-each-adding-a-default-describe-how-do-you-evolve-the-interface-using-c-8-default-interface-methods-and-what-breaks-if-you-ignore-diamond-ambiguity)
  - [Q6. (M) A nightly batch opens thousands of small files under a shared directory. After migrating to `using var`, ops reports "too many open files" and memory climbs until the job finishes. Review the loop:](#q6-m-a-nightly-batch-opens-thousands-of-small-files-under-a-shared-directory-after-migrating-to-using-var-ops-reports-too-many-open-files-and-memory-climbs-until-the-job-finishes-review-the-loop)
  - [Q7. (D) A 400-project solution enables nullable reference types repo-wide. CI surfaces 8,000 warnings; developers blanket `#nullable disable` on touched files and sprinkle `!` to merge PRs. As tech lead, what migration strategy do you recommend for a document-archive domain with heavy `string` metadata (IDs, paths, optional notes), and where do `string?`, null checks, and `[NotNullWhen]` belong versus suppressions?](#q7-d-a-400-project-solution-enables-nullable-reference-types-repo-wide-ci-surfaces-8000-warnings-developers-blanket-nullable-disable-on-touched-files-and-sprinkle-to-merge-prs-as-tech-lead-what-migration-strategy-do-you-recommend-for-a-document-archive-domain-with-heavy-string-metadata-ids-paths-optional-notes-and-where-do-string-null-checks-and-notnullwhen-belong-versus-suppressions)
- [Scenario-Based Questions](#scenario-based-questions-karat-format)

---

### 01. Serialization & Deserialization

## Q1. What is serialization and deserialization, and what is an object graph?

**Concepts**
- Object graph — root object plus all reachable nested objects and collections
- Wire format independence — JSON, XML, binary separate from process memory layout
- Serialization as object-to-wire-format conversion
- Deserialization as wire-format-to-new-instance construction
- Round-trip equivalence — value equality rather than reference identity

**Answer**

Serialization converts an in-memory object graph — the network of objects linked by references and collections — into a storable or transmittable wire format such as JSON, XML, or bytes. Deserialization reads that payload and constructs a new object graph with fresh instances populated from the data.

- The wire format is independent of process memory layout, enabling REST APIs, message queues, and file persistence.
- An object graph includes the root object plus nested objects, lists, and dictionaries reachable from it.
- Serializers decide which members cross the boundary; unmarked or ignored members stay local.
- Round trip means serialize then deserialize and compare meaningful data — not necessarily identical object identity.

---

## Q2. Does deserialization resurrect original object identity or create new instances?

**Concepts**
- Heap identity vs data identity
- Reference equality lost across serialization boundaries
- New instance construction on every deserialize call
- Object reference preservation via metadata IDs as an optional feature

**Answer**

Deserialization always creates new instances on the receiving side; it never resurrects the original heap objects or preserves reference identity from the sender's process. Any `ReferenceEquals` relationship from before serialization is lost unless the format explicitly preserves object references with metadata ids.

- Two references to the same object before serialization may become two separate equal objects after deserialization unless reference preservation is configured.
- Event handlers, database entity keys, and live connections cannot be faithfully restored from wire data alone.
- Identity-sensitive code must re-establish relationships after deserialization explicitly.

---

## Q3. Compare JSON, XML, and binary as wire formats — trade-offs for APIs, config, and storage.

**Concepts**
- JSON readability vs binary payload compactness
- XML schema validation and namespace support via XSD
- Binary format coupling to type layout and declaration order
- Wire format selection by use case — APIs vs config vs caches
- Human-readable debugging cost vs throughput

**Answer**

JSON is compact, human-readable, and dominant for HTTP APIs; XML is verbose but schema-rich and common in legacy enterprise integration; binary formats are smallest and fastest but opaque and tightly coupled to type layout unless you design a versioned protocol.

| Format | Readability | Schema | Typical use |
|---|---|---|---|
| JSON | High | Informal / JSON Schema | REST APIs, config, logs |
| XML | Medium | XSD, namespaces | SOAP, legacy config, documents |
| Binary | Low | Application-defined | Caches, game saves, high-throughput IPC |

JSON balances interoperability and tooling; XML when XSD validation or mixed content matters; binary when size and speed dominate and both endpoints share format rules.

---

## Q4. Explain `System.Text.Json.JsonSerializer.Serialize` and `Deserialize` for files and streams.

**Concepts**
- Stream-based serialization for network and file I/O pipelines
- UTF-8 bytes vs string allocation trade-off
- Async variants for large payload handling without blocking
- JsonSerializerOptions reuse for consistent behavior across calls

**Answer**

`JsonSerializer.Serialize` converts an object to a JSON string or UTF-8 bytes, and `Deserialize<T>` parses JSON back into type `T`, with overloads accepting `Stream`, `ReadOnlySpan<byte>`, and async variants for network and file pipelines. They are the default JSON stack in modern ASP.NET Core.

- `JsonSerializer.Serialize(person)` → string; `SerializeToUtf8Bytes` avoids string allocation for HTTP bodies.
- `Deserialize<Person>(json)` throws `JsonException` on malformed JSON; validate inputs in API boundaries.
- `SerializeAsync` / `DeserializeAsync` stream to and from files without loading entire payloads as strings.
- Pass a shared `JsonSerializerOptions` instance for consistent naming and converters across calls.

---

## Q5. What is `JsonSerializerOptions`, and which settings affect naming, indentation, and enum handling?

**Concepts**
- PropertyNamingPolicy — camelCase vs PascalCase on the wire
- WriteIndented — pretty-print vs compact for production
- DefaultIgnoreCondition — null omission policy
- JsonStringEnumConverter for string enum values globally
- PropertyNameCaseInsensitive for deserialization flexibility

**Answer**

`JsonSerializerOptions` centralizes serializer behavior: `PropertyNamingPolicy` (such as camelCase), `WriteIndented` for pretty printing, `DefaultIgnoreCondition` for omitting nulls, and `Converters` for custom type handling including enums as strings. One configured instance should be reused rather than recreated per call.

- `PropertyNamingPolicy = JsonNamingPolicy.CamelCase` maps `YearOfBirth` to `yearOfBirth` on the wire.
- `WriteIndented = true` aids debugging; disable in production responses to save bytes.
- Add `JsonStringEnumConverter` to `Converters` for string enum values globally.
- `PropertyNameCaseInsensitive = true` relaxes deserialization matching for incoming JSON keys.

---

## Q6. Why is creating a new `JsonSerializerOptions` on every call a performance problem?

**Concepts**
- Internal converter cache rebuilt on every construction
- Reflection metadata cost per options instance on hot paths
- Shared read-only singleton pattern for options reuse
- Per-request allocation pressure and GC cost

**Answer**

Constructing `JsonSerializerOptions` rebuilds internal converter caches and reflection metadata each time, adding CPU and allocation overhead on hot paths such as per-request API serialization. The options object is designed to be created once and shared read-only across threads after configuration.

- ASP.NET Core registers options in dependency injection once at startup for this reason.
- Mutating a shared instance after sharing is unsafe — configure fully before first use.
- Source-generated contexts (`JsonSerializerContext`) reduce reflection cost further for known types.

---

## Q7. When should you use `JsonSerializerContext` source generators vs reflection-based serialization?

**Concepts**
- Native AOT compatibility requirement for source generation
- Trimming-safe serialization without runtime reflection
- Compile-time serializer code generation via partial context class
- Source generator vs reflection trade-off on startup cost and trim warnings

**Answer**

Use source-generated `JsonSerializerContext` when you need faster startup, ahead-of-time (AOT) compatibility, or trimming-safe serialization without runtime reflection over your types. Reflection-based serialization is simpler for exploratory code and dynamically discovered types at the cost of metadata and trim warnings.

- Native AOT and trimmed apps require source generation for types you serialize — reflection may fail at runtime.
- `[JsonSerializable(typeof(Person))]` on a partial context class generates serializers at compile time.
- Reflection remains acceptable for admin tools and few-type internal utilities not published trimmed.
- Hybrid apps register source-generated contexts for hot types and fall back rarely for plugin types.

---

## Q8. Explain `[JsonPropertyName]`, `[JsonIgnore]`, `[JsonInclude]`, and `[JsonPropertyOrder]`.

**Concepts**
- Wire-name mapping independent of C# member naming
- Member exclusion for secrets and computed properties
- Non-public member opt-in for immutable type patterns
- Serialization order for readable diffs and stable output

**Answer**

These attributes control wire mapping without renaming C# members: `JsonPropertyName` sets the JSON key, `JsonIgnore` omits a member, `JsonInclude` opts non-public members into serialization, and `JsonPropertyOrder` influences serialization order for readability or diff stability.

- `[JsonPropertyName("nickname")]` maps a C# `Nickname` property to `"nickname"` regardless of naming policy.
- `[JsonIgnore]` excludes secrets, computed properties, or circular navigation properties from payloads.
- `[JsonInclude]` on a private field includes it when building immutable types intentionally.
- Order attributes matter for human-readable diffs, not JSON semantic equality.

---

## Q9. What happens when JSON contains properties not present on the C# type (extra members)?

**Concepts**
- Default ignore-unknown-members behavior in System.Text.Json
- Forward-compatible additive schema evolution
- Typo in expected name vs truly extra member
- Strict mode alternatives for unknown-property validation

**Answer**

By default System.Text.Json ignores extra JSON properties that have no matching CLR member, so forward-compatible API clients can send new fields older servers skip without error. Strict modes or custom converters can change that behavior when unknown members must fail validation.

- Older services remain compatible when clients add optional metadata fields first.
- To reject unknown members, implement validation with `JsonNode` or a strict schema validator.
- Typos in expected property names deserialize as missing, not as extra — see missing-member gotchas for value types.
- Document versioning strategy: additive JSON fields are safer than renaming existing keys.

---

## Q10. What happens when JSON is missing a property mapped to a non-nullable reference type vs a value type?

**Concepts**
- Value type default on missing JSON member — silent zero or false
- Nullable reference type runtime gap vs compile-time annotation
- required keyword and JsonRequired for validation enforcement
- Silent data corruption risk on missing business-critical fields

**Answer**

Missing reference-type properties deserialize to `null` even when annotated as non-nullable reference types (NRT), because JSON cannot distinguish absent from null without extra validation — analyzers warn but runtime does not enforce. Missing value-type properties deserialize to default values (`0`, `false`) silently, which can hide data errors.

- `string Name` absent in JSON → `null` at runtime; NRT compile warnings do not throw.
- `int Count` absent → `0` without error — dangerous for counters and enums unless validated.
- Use `required` properties (C# 11+), `[JsonRequired]`, or custom validation for critical fields.

---

## Q11. How do enums serialize by default in `System.Text.Json`, and what production risk does numeric enum wire format create?

**Concepts**
- Default numeric enum serialization in System.Text.Json
- Declaration-order coupling to persisted numeric values
- Renumbering risk when enum members are added or reordered
- Wire contract stability with string enum representation

**Answer**

By default enums serialize as their underlying numeric values (`0`, `1`, `2`), not as names, which breaks persisted JSON when enum members are reordered or renumbered in a later release. Clients and databases storing numeric values couple tightly to declaration order.

- Inserting a new enum member in the middle shifts subsequent numeric values and corrupts stored data.
- String enums decouple wire names from numeric backing values when names stay stable.
- Flags enums serialize as combined integers — document bitmask semantics for consumers.

---

## Q12. How do you serialize enums as strings using `JsonStringEnumConverter`?

**Concepts**
- JsonStringEnumConverter registration via options or attribute
- String vs numeric wire representation trade-off
- Case-sensitive deserialization by default
- Custom enum member wire names with JsonPropertyName

**Answer**

Add `JsonStringEnumConverter` to `JsonSerializerOptions.Converters` or apply `[JsonConverter(typeof(JsonStringEnumConverter))]` on the enum or property, so values appear as `"Active"` instead of `0`. Combine with `[JsonPropertyName]` on enum members when wire names differ from C# identifiers.

```csharp
var options = new JsonSerializerOptions();
options.Converters.Add(new JsonStringEnumConverter());
```

- String enums improve readability in logs and manual debugging of API payloads.
- Deserialization accepts names case-sensitively by default — configure case insensitivity if needed.
- Unknown enum strings throw unless you implement custom parsing fallback.

---

## Q13. How do you handle circular references in an object graph (`ReferenceHandler.Preserve` / `IgnoreCycles`)?

**Concepts**
- Cycle detection to prevent infinite serialization loops
- IgnoreCycles — null emitted on revisited object
- Preserve — dollar-id and dollar-ref metadata for graph reconstruction
- DTO redesign to eliminate back-references as preferred alternative

**Answer**

Object graphs with parent/child back-references cause infinite loops during naive serialization; `ReferenceHandler.IgnoreCycles` skips cyclic properties, and `ReferenceHandler.Preserve` emits `$id`/`$ref` metadata to reconstruct shared references on deserialize. Choose preserve when shared identity matters; ignore when trees are logically acyclic except for one back-link.

- `IgnoreCycles` drops repeated traversal — child.Parent may become null in JSON output.
- `Preserve` increases payload complexity but maintains graph shape for object webs.
- Redesign DTOs with id references instead of live object cycles for public APIs when possible.
- Entity Framework navigation properties often need cycle handling or `[JsonIgnore]` on back-references.

---

## Q14. Why does serializing `Animal pet = new Dog()` sometimes drop `Dog`-only properties?

**Concepts**
- Compile-time static type contract vs runtime object shape
- Default serializer contract based on declared variable type
- Polymorphic serialization configuration requirement
- Inheritance hierarchy exposure on public API wires

**Answer**

Statically typed serialization uses the compile-time type `Animal` unless polymorphic options include derived members, so properties declared only on `Dog` are omitted from JSON when the variable is typed as the base class. Runtime type alone does not expand the contract without configuration.

- `JsonSerializer.Serialize<Animal>(new Dog())` serializes only `Animal` members by default.
- Enable polymorphic type discriminators or serialize as `Dog` / use `object` with polymorphic options in modern System.Text.Json.
- API models often flatten DTOs instead of relying on inheritance on the wire.

---

## Q15. How do you enable polymorphic serialization in modern `System.Text.Json`?

**Concepts**
- JsonDerivedType attribute for derived type registration on base
- Type discriminator property in JSON output
- Runtime derived type instantiation during deserialization
- Security implications of polymorphic deserialization with untrusted input

**Answer**

.NET 7+ supports polymorphic serialization via attributes such as `[JsonDerivedType(typeof(Dog), "dog")]` on base types and global polymorphism options, emitting a type discriminator alongside base properties so deserializers instantiate the correct derived type. Earlier versions required custom converters or separate DTO shapes.

- Discriminator property name and derived type mappings must be stable across API versions.
- Untrusted polymorphic deserialization is a security risk — validate allowed derived types strictly.
- Newtonsoft.Json historically used `$type` metadata — migrate carefully when switching serializers.
- Integration tests should round-trip every derived type in the hierarchy.

---

## Q16. What is `JsonNode`, `JsonObject`, and `JsonArray`, and when prefer them over strongly typed models?

**Concepts**
- DOM-based JSON without a fixed C# class requirement
- JsonObject key-based access and in-place mutation
- JsonArray index-based element access
- Schema-evolving and partially structured payload handling
- Strongly typed model vs DOM trade-off

**Answer**

`JsonNode` and its subclasses `JsonObject` and `JsonArray` model JSON as a mutable DOM you can navigate and edit without declaring fixed C# classes, useful for partially structured payloads, ad-hoc API exploration, and schema-evolving documents. Prefer strongly typed models when shape is stable and validation belongs at deserialization time.

- `JsonObject` supports `node["key"]` get/set; `JsonArray` indexes elements.
- Ideal for merging settings files, patching unknown third-party JSON, or building responses dynamically.
- Strong types give compile-time checks and clearer domain models for core business entities.
- Combine: deserialize known portions to types and keep extras in `JsonExtensionData` dictionary properties.

---

## Q17. How do you navigate and mutate JSON with `JsonNode` without deserializing to a fixed class?

**Concepts**
- JsonNode.Parse for in-memory DOM construction
- AsObject and AsArray for type-specific navigation
- Null-conditional access for missing key safety
- ToJsonString for serialization after DOM mutation

**Answer**

Parse with `JsonNode.Parse(json)` or `JsonDocument`/`JsonNode` async overloads, then cast or use `AsObject()` / `AsArray()` to read and assign properties, add keys, and remove nodes imperatively. Changes reflect in the in-memory tree until you call `ToJsonString()` to emit updated text.

- `JsonNode? nick = root?["nickname"];` — null-conditional for missing keys.
- Mutate: `((JsonObject)root!)["count"] = 42;`
- Clone subtrees when branching immutable snapshots for undo flows.
- Validate types before cast — wrong node kinds throw `InvalidOperationException`.

---

## Q18. What is a custom `JsonConverter<T>`, and when would you implement `Read`/`Write` manually?

**Concepts**
- JsonConverter base class for custom type serialization logic
- Read and Write method override for JSON token translation
- Converter registration — attribute per property vs global options
- Null handling and forward-compatible token skipping requirements

**Answer**

`JsonConverter<T>` plugs custom serialization logic into System.Text.Json for types the default reflection serializer handles poorly — for example `DateOnly`, discriminated unions, or legacy string formats for numbers. Override `Read` and `Write` to translate between JSON tokens and your CLR type explicitly.

- Register on the property with `[JsonConverter(typeof(MyConverter))]` or add to `JsonSerializerOptions.Converters`.
- Use when you need invariant wire formats independent of culture or special rounding rules for decimals.
- Manual converters must handle null, property names in object scenarios, and forward-compatible token skipping.
- Prefer built-in converters and attributes when they suffice — custom code is maintenance overhead.

---

## Q19. How do `Utf8JsonReader` and `Utf8JsonWriter` differ from `JsonSerializer` helpers?

**Concepts**
- Forward-only token-based JSON reading without object graphs
- Incremental UTF-8 writing without intermediate allocations
- Low-level vs high-level API trade-off
- Selective field extraction for large JSON payloads

**Answer**

`Utf8JsonReader` and `Utf8JsonWriter` are low-level, forward-only UTF-8 JSON readers and writers that process tokens without building full object graphs, offering maximum control and performance for streaming pipelines. `JsonSerializer` builds on them internally for convenience deserialization to types.

- Reader loop: `while (reader.Read()) { switch (reader.TokenType) ... }`
- Writer: `WriteStartObject`, `WriteString`, `WriteEndObject` for incremental emission.
- Use low-level APIs for huge files, selective field extraction, or zero-allocation hot paths.
- Higher error handling burden — malformed JSON fails at token level with `JsonException`.

---

## Q20. Explain `XmlSerializer` requirements (parameterless constructor, public read/write properties).

**Concepts**
- Parameterless public constructor requirement for XmlSerializer
- Public read/write property requirement for member serialization
- Runtime code generation on first XmlSerializer construction
- Private member exclusion by default

**Answer**

`XmlSerializer` can serialize public types with a public parameterless constructor and public read/write properties; it cannot serialize types lacking a default constructor or exposing only get-only properties unless you customize with attributes or `IXmlSerializable`. It generates XML from property names and collection shapes at runtime.

- Private fields and get-only auto-properties are ignored unless specially configured.
- Generic types and interfaces follow the same public property contract rules.
- Runtime failures occur when constructing the serializer if constraints are violated — compile succeeds.

---

## Q21. What do `[XmlRoot]`, `[XmlElement]`, `[XmlAttribute]`, and `[XmlIgnore]` control?

**Concepts**
- Root element name control with XmlRoot
- Property-to-child-element name mapping with XmlElement
- Attribute vs child element distinction with XmlAttribute
- Member exclusion from XML output and input with XmlIgnore

**Answer**

These attributes map CLR members to XML shape: `XmlRoot` names the document element, `XmlElement` sets element names for properties, `XmlAttribute` serializes a property as an attribute instead of child element, and `XmlIgnore` excludes members from XML output and input.

- `[XmlRoot("Person")]` on class changes outer tag from default type name.
- `[XmlAttribute("id")]` inlines simple values on the parent element.
- Collections serialize as repeated child elements unless `[XmlArray]` configures array layout.
- Order and namespace control use additional attributes (`Namespace`, `Order`) for schema compliance.

---

## Q22. Why can `XmlSerializer` fail at runtime even when the project compiles?

**Concepts**
- First-use serialization assembly generation timing
- Unsupported type shape detection at runtime not compile time
- Compile-time vs serializer constraint validation gap
- Fail-fast testing at startup for serializer constructors

**Answer**

`XmlSerializer` validates type shape and emits serialization assemblies on first use; violations such as missing parameterless constructors, interfaces as root types, or unsupported collection patterns throw `InvalidOperationException` at runtime when you first construct the serializer or serialize. The C# compiler does not analyze XML serializer constraints.

- First call may trigger csc.exe dynamic assembly generation — failures surface in production if untested.
- Circular references and `Dictionary<K,V>` historically required workarounds or custom serialization.
- Test `new XmlSerializer(typeof(MyType))` at startup or in integration tests to fail fast.
- `XmlSerializer` cannot serialize `IDictionary` implementations without custom patterns in many cases.

---

## Q23. What is `[Serializable]` actually used for in modern .NET?

**Concepts**
- Legacy BinaryFormatter attribute heritage
- System.Text.Json independence from the Serializable attribute
- Modern JSON attribute contracts versus legacy binary markers
- Historical remoting and ASP.NET session state context

**Answer**

The `[Serializable]` attribute marked types as eligible for legacy binary formatters such as `BinaryFormatter`; modern `System.Text.Json` and `XmlSerializer` ignore it for their default code paths. It remains on some Framework-era types but is not the switch for JSON API serialization today.

- Do not add `[Serializable]` expecting JSON behavior — configure JSON attributes instead.
- Remoting and old ASP.NET session state modes used binary serialization — largely historical.
- Prefer explicit DTO contracts for new persistence and API layers.

---

## Q24. Explain `BinaryFormatter` — why is it obsolete, and what security risks led to its removal?

**Concepts**
- Deserialization gadget chains enabling remote code execution
- Type-aware binary serialization attack surface
- Obsolescence and default disabling in modern .NET
- Untrusted payload handling risk with arbitrary type graphs

**Answer**

`BinaryFormatter` deserialized arbitrary type graphs from untrusted bytes and could be tricked into executing dangerous gadget chains, enabling remote code execution — a class of deserialization attacks. It is obsolete and disabled by default on modern .NET; Microsoft removed it as a safe default because type-name embedded payloads are inherently unsafe against malicious input.

- Never deserialize untrusted binary payloads with type-aware formatters.
- CVE history around `BinaryFormatter` drove stricter defaults and removal timelines.
- Alternatives use explicit schemas: JSON, Protocol Buffers, MessagePack with known types.

---

## Q25. What are recommended modern alternatives to `BinaryFormatter` for trusted internal persistence?

**Concepts**
- System.Text.Json for readable structured persistence
- MessagePack and Protocol Buffers for performance-sensitive caches
- Explicit type whitelisting vs arbitrary type graph deserialization
- Versioned binary layouts with BinaryWriter for custom formats

**Answer**

For trusted internal scenarios, use explicit formats: System.Text.Json or MessagePack for structured objects, `MemoryPack` or Protocol Buffers for performance-sensitive caches, or custom versioned binary layouts with `BinaryWriter` where you control every byte. All alternatives should whitelist types rather than embed arbitrary type names.

- JSON with source generation balances readability and AOT for app settings caches.
- MessagePack and protobuf require `.proto` or attributed models — no arbitrary type graphs.
- Encrypt and authenticate persisted blobs at rest even when trusted — defense in depth.
- Version headers on custom binary files enable migration without formatter magic.

---

## Q26. How does `DateTime` with unspecified `Kind` behave across time zones during serialization?

**Concepts**
- DateTimeKind.Unspecified ambiguity on the wire
- UTC Kind serialization with Z suffix in ISO 8601
- Local Kind coupling to the writing machine's time zone
- Cross-timezone interpretation risk for global systems

**Answer**

`DateTime` with `DateTimeKind.Unspecified` carries no time-zone offset on the wire in ISO 8601 strings without `Z` or offset, so deserializing machines may interpret the instant differently depending on serializer options and local assumptions. Unspecified values are ambiguous for global systems.

- `DateTimeKind.Utc` serializes with `Z` suffix when using round-trip formats — preferred for server timestamps.
- `Local` kind ties meaning to the writer machine's time zone — poor for APIs.
- System.Text.Json defaults favor ISO 8601 strings; options control how `Unspecified` is written.
- Document whether API consumers should treat absent offset as UTC or local.

---

## Q27. Why is `DateTimeOffset` often safer on the wire than `DateTime`?

**Concepts**
- Explicit UTC offset embedded in every DateTimeOffset value
- Eliminating Kind ambiguity at API boundaries
- ISO 8601 format with offset for portable timestamps
- Daylight saving transition safety for global consumers

**Answer**

`DateTimeOffset` always includes an offset from UTC alongside the clock time, preserving the intended instant across time zones without relying on unstated `Kind` semantics. It eliminates much ambiguity when clients span regions and daylight-saving transitions.

- Wire form includes `+05:30` or `Z` explicitly — consumers know the instant.
- Database storage still requires consistent UTC policy — `DateTimeOffset` maps cleanly to UTC for storage.
- Use for API contracts; convert to `DateTime` only at UI boundaries when local display requires it.
- Pair with invariant ISO formatting in serializers for stable string comparisons.

---

## Q28. Can a type with only get-only properties serialize but fail to deserialize? Why?

**Concepts**
- Serialization requires only read access; deserialization requires write path
- JsonConstructor for constructor-based immutable type deserialization
- Init-only setters as write path for object initializer syntax
- Serialize-only DTO projection vs round-trip DTO distinction

**Answer**

Yes — System.Text.Json can serialize get-only properties by reading them, but deserialization requires writable members, constructor parameters matched by `[JsonConstructor]`, or `[JsonInclude]` on private setters unless using parameterized constructors configured for JSON. Immutable types need explicit constructor binding.

- Records with positional syntax generate constructor mapping; manual get-only classes do not deserialize by default.
- `[JsonInclude]` on private fields supports immutable object patterns intentionally designed for JSON.
- Serialize-only DTO projections are fine for reports; round-trip DTOs need write paths.
- Test deserialize in CI, not just serialize, for every API model.

---

## Q29. How do `[JsonConstructor]` and parameterized constructors interact with deserialization?

**Concepts**
- JsonConstructor attribute for explicit constructor selection
- Case-insensitive parameter name to JSON property binding
- Required parameter missing in JSON throws JsonException
- Immutable type deserialization pattern without public setters

**Answer**

Mark one constructor with `[JsonConstructor]` so the deserializer invokes it and binds JSON properties to parameters by name (case-insensitive by default), enabling immutable types without public setters. Parameters must align with JSON property names or `[JsonPropertyName]` mappings.

- Without `[JsonConstructor]`, multiple constructors cause ambiguity or fallback to parameterless ctor plus setters.
- Parameter names in source may require `[JsonConstructor]` and C# 9+ parameter name metadata for binding.
- Required parameters missing in JSON throw `JsonException` when options demand non-null values.
- Preferred pattern for domain models that reject invalid states at construction time.

---

## Q30. What is the difference between `System.Text.Json` and `Newtonsoft.Json` feature sets (contract customization, references)?

**Concepts**
- ASP.NET Core default vs optional NuGet package
- ReferenceHandler vs Newtonsoft ReferenceLoopHandling
- JsonNode DOM vs JObject and JToken DOM
- ContractResolver vs attributes and options for naming control
- Source generation support in System.Text.Json for AOT

**Answer**

System.Text.Json is built into modern .NET with faster defaults and ASP.NET Core integration but historically fewer knobs; Newtonsoft.Json (Json.NET) offers mature reference loop handling, `JObject` DOM, extensive contract resolvers, and broad customization at some performance cost. Many apps still reference Newtonsoft for legacy APIs or advanced scenarios.

| Area | System.Text.Json | Newtonsoft.Json |
|---|---|---|
| ASP.NET Core default | Yes | Optional package |
| Reference loops | `ReferenceHandler` options | `ReferenceLoopHandling` |
| DOM | `JsonNode` | `JObject` / `JToken` |
| Custom naming | Attributes + options | `ContractResolver` |

New greenfield ASP.NET Core APIs typically standardize on System.Text.Json unless a Newtonsoft-specific feature is required.

---

### 02. Reflection & Attributes

## Q1. What is reflection in C#, and what problems does it solve?

**Concepts**
- Runtime type introspection without compile-time knowledge of the type
- Late binding — resolving members by name at runtime rather than compile time
- Plugin and extensibility architectures using assembly scanning
- Type metadata access via MethodInfo, PropertyInfo, FieldInfo, and ConstructorInfo
- Performance cost of reflection vs compiled delegates

**Answer**

Reflection gives me access to type metadata at runtime, which means I can inspect, invoke, and instantiate types whose names I only know as strings rather than as compile-time symbols. The core entry point is the Type object, obtained via 	ypeof(T) for a compile-time known type or instance.GetType() for a runtime object. From there, GetMethods(), GetProperties(), GetFields(), and GetCustomAttributes() return all the member metadata.

The problems reflection solves are plugin loading (finding and creating an instance from an external assembly using a configured type name), generic framework infrastructure (ORMs mapping column names to properties, serializers reading all public members), and diagnostic tooling that enumerates types at startup. The cost is that reflection bypasses the compiler's rename-safety checks so a refactored member name causes a runtime failure rather than a build error, and the invocation overhead is higher than a direct call because member lookup involves internal cache lookups and boxing. I cache MethodInfo or PropertyInfo references when calling them in hot loops rather than retrieving them on every iteration.

## Q2. What is the difference between early binding and late binding?

**Concepts**
- Early binding — member resolution at compile time with full type checking
- Late binding — member resolution at runtime via reflection or dynamic dispatch
- Compiler error vs runtime exception trade-off
- MethodInfo.Invoke as the primary late-binding mechanism in the BCL
- Performance overhead of late binding vs JIT-optimized direct calls

**Answer**

Early binding resolves method calls and property accesses at compile time, so the compiler verifies that the member exists and has the right signature before any code runs. Renaming a method causes a build error immediately, which means the toolchain catches the mistake rather than production. Late binding defers that resolution to runtime: reflection's MethodInfo.Invoke and the dynamic keyword both look up members by name only when the line executes, so misspelled names or wrong argument types surface as RuntimeBinderException or TargetInvocationException rather than build failures.

Early binding is the right default for application code because it enables IDE refactoring, static analysis, and JIT optimization. Late binding is justified for plugin systems where the target type is not available at compile time, for COM interop, and for metadata-driven frameworks that must operate on arbitrary types. Since the invocation overhead is significantly higher, I cache the MethodInfo reference and optionally compile it to a delegate when the same method will be called repeatedly.

## Q3. Explain `typeof(T)` vs `obj.GetType()`

**Concepts**
- typeof — compile-time type token fixed at declaration
- GetType — runtime polymorphic type of the actual heap object
- Compile-time vs runtime type divergence with inheritance
- Use cases for each — generics, reflection, serialization

**Answer**

`typeof(T)` is a compile-time operator that returns a fixed `Type` object baked into the IL at the call site — the result is always the type you wrote in source, regardless of what object might be assigned to a variable at runtime. `instance.GetType()` is a virtual call on `object` that returns the actual runtime type of the heap object, so it returns `Dog` when the variable is typed as `Animal` and holds a `Dog` instance.

The distinction matters for serialization contracts (the default serializer uses the declared type, not the runtime type), for equality checks with types (	ypeof(Dog) != typeof(Animal)), and for reflection where you want the concrete type's members. I use 	ypeof when I have a known static type in generics or attribute arguments, and GetType() when I receive an object whose concrete type must be discovered at runtime — factory methods and polymorphic dispatch are common cases.

## Q4. Why does `typeof(List<int>) == typeof(List<string>)` return false?

**Concepts**
- Closed generic types as distinct runtime Type objects
- Generic type definition vs closed generic instantiation
- Type equality based on definition plus type arguments
- GetGenericTypeDefinition — extracting the open generic from a closed one

**Answer**

Generic types in .NET are parameterized with type arguments at the point of instantiation, and each unique combination of type arguments produces a distinct Type object. List<int> and List<string> share the same open generic definition (List<>) but have different type argument arrays, so the runtime creates two separate closed generic types and 	ypeof(List<int>) == typeof(List<string>) returns false.

This matters when writing reflection code that checks or routes by type: comparing 	ype.GetGenericTypeDefinition() == typeof(List<>) succeeds for both, which is the correct way to ask whether a type is any closed form of List<T>. I use this pattern in framework code that must handle arbitrary generic collections without knowing the element type at compile time.

## Q5. How do you obtain the generic type definition from a closed generic type?

**Concepts**
- GetGenericTypeDefinition — extracting the open generic List<> from List<int>
- IsGenericType and IsGenericTypeDefinition as guard checks
- GetGenericArguments to retrieve type parameters of closed generics

**Answer**

Given a closed generic type like 	ypeof(List<int>), I call 	ype.GetGenericTypeDefinition() to get back the open generic 	ypeof(List<>). Before calling it I check 	ype.IsGenericType to avoid an exception on non-generic types. The inverse direction — creating a closed generic from an open one — uses openGeneric.MakeGenericType(typeof(int)). I also use 	ype.GetGenericArguments() when I need to know the element type of a collection in a generic framework, for example to pick the right converter for a List<T> member in a custom serializer.

## Q6. What is the `Type` class, and what members expose metadata (methods, properties, fields, attributes)?

**Concepts**
- Type as the root object for runtime metadata access
- GetMethods, GetProperties, GetFields, GetConstructors enumeration
- BindingFlags filtering for public, non-public, instance, static members
- GetCustomAttributes and IsDefined for attribute inspection

**Answer**

The Type class is the central metadata object in .NET reflection, obtained via 	ypeof(T) or instance.GetType(). It exposes GetMethods(), GetProperties(), GetFields(), GetConstructors(), and GetEvents() to enumerate declared and inherited members. Each method returns an array of MemberInfo-derived objects (MethodInfo, PropertyInfo, FieldInfo, ConstructorInfo) that carry the member's name, return type, parameters, and applied attributes.

By default these methods return only public members, so to reach private or internal members I must pass BindingFlags.NonPublic | BindingFlags.Instance (or add BindingFlags.Static for class-level members). GetCustomAttributes() on any MemberInfo returns the attributes applied to that member, and IsDefined(typeof(MyAttr)) is faster when I only need a boolean check without allocating the attribute instance.

## Q7. What is `BindingFlags`, and how do `Instance`, `Static`, `Public`, `NonPublic`, and `DeclaredOnly` combine?

**Concepts**
- BindingFlags as a bitmask controlling member lookup scope
- Instance vs Static — object members vs class-level members
- Public vs NonPublic — accessibility level inclusion
- DeclaredOnly — exclude inherited members from results

**Answer**

BindingFlags is a bitmask enum passed to reflection lookup methods to control which members are returned. The four core flags combine: Instance includes members that require an object receiver, Static includes class-level members without a receiver, Public includes accessibility-public members, and NonPublic includes private and internal ones. Without providing any flags, the runtime uses its default selection which typically excludes non-public members.

A common pattern is BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly to read private fields on the exact type without climbing the inheritance chain. DeclaredOnly prevents the lookup from returning inherited members, which matters when scanning a type for its own annotations rather than those from base types. Forgetting Instance when looking for non-static fields is a frequent mistake that silently returns null rather than throwing.

## Q8. How do you enumerate properties, methods, fields, and constructors with reflection?

**Concepts**
- GetProperties, GetMethods, GetFields, GetConstructors as enumeration entry points
- BindingFlags filtering for scope and accessibility
- MemberInfo base class carrying name and declaring type
- Caching reflection results to avoid repeated metadata overhead

**Answer**

I call `type.GetProperties(BindingFlags.Instance | BindingFlags.Public)` to get all public instance properties, and similarly `GetMethods`, `GetFields`, and `GetConstructors` with whatever flags match the target scope. Each call returns an array of the corresponding `MemberInfo`-derived type: `PropertyInfo`, `MethodInfo`, `FieldInfo`, or `ConstructorInfo`. Without explicit `BindingFlags`, the methods return only public instance and static members, so I always pass explicit flags when I need non-public or static members too.

For framework code that processes many objects, I cache the returned arrays in a `ConcurrentDictionary<Type, PropertyInfo[]>` keyed by type rather than calling `GetProperties` on every invocation, since the metadata never changes after the assembly loads. I also prefer `GetDeclaredMembers` or `GetRuntimeProperties` from the `TypeInfo` API in library code targeting multiple platforms because those APIs have more predictable scoping across inheritance boundaries.

---

## Q9. How do you invoke a method dynamically via `MethodInfo.Invoke`?

**Concepts**
- MethodInfo.Invoke with target instance and argument array
- TargetInvocationException wrapping callee exceptions
- Null target for static method invocation
- Compiled delegate caching via CreateDelegate for hot paths

**Answer**

`MethodInfo.Invoke(target, args)` calls the method on `target` with the arguments in `args` as a boxed `object[]`. For static methods I pass `null` as the target. The method's return value comes back as `object` and needs casting to the expected type. Any exception thrown inside the method is wrapped in `TargetInvocationException`, so production code must catch that type specifically and unwrap the `InnerException` before logging or rethrowing — otherwise operators only see the wrapper in their logs.

When I need to call the same method many times (for example in a batch export loop), I compile the `MethodInfo` to a typed delegate once using `method.CreateDelegate(typeof(Func<T, R>), null)` and invoke the delegate from then on. This eliminates the per-call boxing overhead and the exception-wrapping cost, bringing performance close to a direct call while retaining the late-binding discovery step at startup.

---

## Q10. What is `Activator.CreateInstance`, and how do you pass constructor arguments?

**Concepts**
- Activator.CreateInstance for runtime type instantiation by Type object
- Parameterless constructor requirement for the no-args overload
- Constructor argument array for the overload accepting object params
- ConstructorInfo.Invoke as the lower-level alternative with explicit BindingFlags

**Answer**

`Activator.CreateInstance(type)` calls the public parameterless constructor of the given `Type` and returns the new instance as `object`. When the constructor requires arguments I use `Activator.CreateInstance(type, arg1, arg2)` which searches for a matching constructor signature at runtime and invokes it. If no matching constructor is found, the method throws `MissingMethodException` at runtime rather than at compile time.

For more control — for example when I need to invoke a non-public constructor — I retrieve the `ConstructorInfo` directly with `type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, paramTypes, null)` and call `ctor.Invoke(args)`. This is the right path for factory patterns in frameworks that need to bypass access modifiers in controlled circumstances, though I document and test such usage carefully since it breaks encapsulation across release boundaries.

---

## Q11. How do you create generic types at runtime (`MakeGenericType`) and invoke generic methods (`MakeGenericMethod`)?

**Concepts**
- Open generic type definition as the template
- MakeGenericType to close the definition with runtime type arguments
- GetGenericMethodDefinition to obtain the open generic method
- MakeGenericMethod to close a generic method with concrete type arguments

**Answer**

To construct `List<T>` with a type argument known only at runtime I call `typeof(List<>).MakeGenericType(elementType)`, which returns the fully closed `Type` for that specific element type. I then use `Activator.CreateInstance` on the result to get an instance. For generic methods the sequence is: get the `MethodInfo` for the open generic method, check that it `IsGenericMethodDefinition`, then call `.MakeGenericMethod(typeArg1, typeArg2)` to produce the closed version, and finally invoke it.

The most common use case is building adapter infrastructure — for example a serialization hub that needs to call `JsonSerializer.Deserialize<T>(json, options)` for a `T` resolved at runtime from configuration. I cache the open generic method and the closed method per type argument in a static dictionary, since `MakeGenericMethod` is relatively expensive and the resulting `MethodInfo` is reusable across calls for the same type argument.

---

## Q12. How do you get and set property and field values through `PropertyInfo` / `FieldInfo`?

**Concepts**
- PropertyInfo.GetValue and SetValue with target instance
- FieldInfo.GetValue and SetValue for field-level access
- CanRead and CanWrite guards before get/set calls
- Compiled property accessor delegates for hot-path performance

**Answer**

`propertyInfo.GetValue(instance)` returns the property value as `object`, and `propertyInfo.SetValue(instance, value)` assigns a boxed value. I check `CanRead` before getting and `CanWrite` before setting to avoid `MethodAccessException` on init-only or readonly properties. For fields, `fieldInfo.GetValue(instance)` and `fieldInfo.SetValue(instance, value)` work the same way, but fields accessed with `BindingFlags.NonPublic` do not need `CanWrite` since fields always accept direct assignment through reflection unless they are `readonly` — though even `readonly` fields can be set in some framework scenarios using `FieldInfo.SetValue` since the runtime does not enforce field `readonly` through reflection on older .NET versions.

When performance matters I compile a typed getter using `propertyInfo.GetMethod!.CreateDelegate<Func<T, R>>()` for a specific type, which eliminates per-call boxing. This is the pattern used in high-throughput serializers and ORMs where each property is accessed millions of times per second and the boxing overhead from `GetValue` becomes measurable.

---

## Q13. How can reflection access private members, and why is that a maintenance and security concern?

**Concepts**
- BindingFlags.NonPublic for private and internal member access
- Access modifier bypass through reflection
- Semantic version contract breakage when private members change
- Trust level and partial trust historically gating non-public reflection

**Answer**

Passing `BindingFlags.NonPublic | BindingFlags.Instance` to `GetField`, `GetProperty`, or `GetMethod` grants access to members the type author marked private or internal, bypassing the access modifier enforcement that normally prevents callers from touching implementation details. The runtime does not prevent this — it is a deliberate reflection capability for frameworks, serializers, and testing tools.

The maintenance concern is that private members carry no API contract commitment: a library author can rename, remove, or change the type of a private field in any patch release, and code that accesses it by string name will break at runtime without any compiler warning. The security concern in the .NET Framework era was that code-access security (CAS) restricted non-public reflection in partial-trust environments, but modern .NET runs entirely in full trust, so the restriction is now about design discipline rather than permission enforcement. I limit non-public reflection to testing tools and serialization infrastructure where there is no viable alternative, and I avoid it in application code where I control the types being reflected.

---

## Q14. Explain `Assembly`, `Module`, `MemberInfo`, `MethodInfo`, `PropertyInfo`, and `FieldInfo` relationships.

**Concepts**
- Assembly as the top-level deployment and metadata unit
- Module as the intermediate container within an assembly
- MemberInfo as the common base for all member metadata objects
- MethodInfo, PropertyInfo, FieldInfo as concrete MemberInfo subtypes

**Answer**

An `Assembly` is the deployment unit — a `.dll` or `.exe` — and contains one or more `Module` objects. A `Module` holds the type definitions within the assembly, though in practice most assemblies have a single module. `Type` objects are retrieved from modules (or directly from `Assembly.GetType`), and each `Type` exposes its members through the reflection API.

`MemberInfo` is the abstract base for all member metadata. `MethodInfo` describes a method (parameters, return type, generic arguments), `PropertyInfo` wraps the underlying getter and setter `MethodInfo` objects behind a property-shaped API, `FieldInfo` describes a field variable, and `ConstructorInfo` describes a constructor. All of these carry the member's name, declaring type, access modifiers, and custom attributes. The `MemberInfo.MemberType` enum distinguishes them when working with an array of mixed member types returned by `GetMembers()`.

---

## Q15. What is the difference between `Assembly.Load`, `Assembly.LoadFrom`, and `AssemblyLoadContext`?

**Concepts**
- Assembly.Load — loads by name using the runtime's binding policy
- Assembly.LoadFrom — loads from a specific file path directly
- AssemblyLoadContext — isolated load boundary for side-by-side versioning
- Type identity — types from different load contexts are not interchangeable

**Answer**

`Assembly.Load(AssemblyName)` asks the runtime's binder to find the assembly using its probing paths and fusion rules, which means the assembly must be in a location the runtime knows about (application base, GAC, or configured probing path). `Assembly.LoadFrom(path)` loads directly from an absolute or relative file path, bypassing the standard binding process — useful for plugin directories outside the application base, but types from assemblies loaded this way participate in the default load context.

`AssemblyLoadContext` (ALC) is the modern .NET approach for isolation: each ALC instance has its own resolution logic and type universe, so two ALCs can each load a different version of the same assembly without conflict. Types loaded in different ALCs are incompatible even if they have the same qualified name, which means plugin interfaces must either be in a shared ALC or communicated through a common abstraction loaded in both. ALCs can be collectible (allowing the assembly to be unloaded when the ALC is disposed) while the default context and Framework-era AppDomains cannot unload individual assemblies.

---

## Q16. Can you unload an assembly in .NET Framework vs .NET Core / .NET 5+?

**Concepts**
- AppDomain isolation as the unload boundary in .NET Framework
- Collectible AssemblyLoadContext for unload support in modern .NET
- GC root release requirement before ALC can be collected
- Implications for long-running plugin host processes

**Answer**

In .NET Framework, individual assemblies cannot be unloaded — the only way to reclaim the memory is to unload the entire `AppDomain` that contains them. Creating a dedicated `AppDomain` for each plugin with cross-domain marshaling was the historical workaround, but `AppDomains` are expensive and the marshaling boundaries are complex.

Modern .NET replaced `AppDomain` isolation with `AssemblyLoadContext`. A collectible ALC can be unloaded by calling `Unload()`, which marks the context as unloading and begins GC collection once all strong references to types and instances from that ALC are released. If any object from the ALC is still reachable — cached in a dictionary, captured by a delegate, referenced from a field — the ALC will not be collected. Debugging stuck ALC unloads requires checking all caches, static fields, and event handlers that might hold onto types from the plug-in assembly. This capability is essential for plugin host processes that need to hot-reload code without restarting the entire service.

---

## Q17. How do you discover and read custom attributes at runtime (`GetCustomAttribute`, `IsDefined`)?

**Concepts**
- MemberInfo.GetCustomAttribute<T> for typed attribute retrieval
- IsDefined for existence check without allocating the attribute instance
- GetCustomAttributes for multiple attributes of the same type (AllowMultiple)
- Attribute inheritance and the inherit parameter on reflection calls

**Answer**

Every `MemberInfo` subtype — `Type`, `MethodInfo`, `PropertyInfo`, and so on — exposes `GetCustomAttribute<T>()`, which returns the single attribute instance of type `T` applied to that member, or `null` if none is found. When I only need to know whether an attribute is present rather than reading its properties, `IsDefined(typeof(T))` is cheaper because it short-circuits without constructing the attribute object.

For attributes declared with `AllowMultiple = true`, I call `GetCustomAttributes<T>()` to get all instances as an array. The `inherit` parameter — `true` by default on the generic overloads — controls whether attributes applied to base classes or overridden methods are included. This matters when checking attributes on derived types: a `[Authorize]` attribute on a controller base class is visible on the derived class only when `inherit: true` is passed. The returned attribute instances are live objects, so any public property set in the attribute constructor or initializer is directly readable.

---

## Q18. How do you define and apply your own attribute classes (`AttributeUsage`)?

**Concepts**
- Attribute base class as the required parent
- AttributeUsage — controlling valid targets and multiplicity
- Constructor parameters vs named properties in attribute syntax
- Attribute parameter types restricted to constants (primitives, enums, Type, string)

**Answer**

A custom attribute is just a class that inherits from `System.Attribute`. I decorate the class itself with `[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]` to declare where the attribute can be applied, whether it can appear more than once on the same member, and whether it propagates through inheritance.

The attribute's constructor receives positional arguments — the values callers write first, without a name — and any public properties or fields receive named arguments, which are the `PropertyName = value` pairs callers can optionally provide. All parameter values must be compile-time constants: primitive types, strings, enums, `Type` literals, or one-dimensional arrays of those. When the attribute is applied at the call site, the compiler embeds the argument values into the assembly metadata. At runtime, reflection reconstructs the attribute instance by calling the constructor with the positional arguments and then setting any named property values that were specified.

---

## Q19. What are performance costs of reflection vs compiled code, and how do trimming/AOT affect it?

**Concepts**
- Method dispatch cost — reflection bypasses JIT-inlined, direct call overhead
- Caching MethodInfo/PropertyInfo vs repeated GetMethod/GetProperty calls
- Compiled delegate from MethodInfo.CreateDelegate as a hot-path mitigation
- IL trimming removing members not referenced at compile time
- AOT compilation replacing reflection with source-generated code

**Answer**

A reflective `MethodInfo.Invoke` call is typically 10-50x slower than a direct compiled call because it must perform type checks, marshal arguments from an object array, and cannot be inlined by the JIT. The `GetMethod` lookup itself adds overhead because it walks type metadata. The standard mitigation is to pay the lookup cost once at startup, cache the `MethodInfo`, and then either call `Invoke` in a tight loop or — for very hot paths — convert it to a compiled delegate via `MethodInfo.CreateDelegate(typeof(Action<T>), target)`, which lets the JIT optimize the call normally.

Trimming (the `PublishTrimmed` property) and NativeAOT compilation both break reflection assumptions. The trimmer removes methods, properties, and constructors that the static call graph does not reach, which means code discovered only at runtime by string-based reflection will be absent in the trimmed binary. AOT compilation goes further: it requires all types to be resolved at compile time, so arbitrary `MakeGenericType` or `Activator.CreateInstance` calls fail unless annotated with `[DynamicallyAccessedMembers]` or replaced with source generators. The modern pattern is to express "I need all public constructors of this type" via those attributes, which preserves the required metadata and makes the intent analyzable by the trimmer.

---

## Q20. What is `Reflection.Emit`, and when is dynamic IL generation justified?

**Concepts**
- System.Reflection.Emit namespace — runtime IL code generation
- DynamicMethod as lightweight single-method emission without a full assembly
- ILGenerator opcode sequence vs higher-level lambda compilation via Expression trees
- Serializer, mapper, and proxy generator use cases
- AOT incompatibility of runtime IL emission

**Answer**

`Reflection.Emit` lets me construct .NET assemblies, types, and methods at runtime by emitting CIL opcodes directly through `ILGenerator`. The entry points are `AssemblyBuilder`, `ModuleBuilder`, `TypeBuilder`, and `MethodBuilder` for persistent types, or `DynamicMethod` for a single callable method without full type scaffolding. The result is a real compiled method that runs at native speed once the JIT processes it, unlike interpreted reflection.

The typical justification is generating type-specific, allocation-free code paths that cannot be expressed in generic C# at compile time: serializers (like `Newtonsoft.Json` older versions), object-relational mappers, and dynamic proxy generators all emit IL to avoid the per-call overhead of `MethodInfo.Invoke`. In modern .NET, `Expression<T>` compilation via `Compile()` is usually a better trade-off — it is higher-level, more readable, and still produces compiled delegates — so raw `ILGenerator` is now reserved for scenarios where `Expression` cannot represent the required opcodes (e.g., by-ref locals, `tailcall` optimization). Neither approach works under NativeAOT since runtime IL emission requires the JIT, which is absent in ahead-of-time scenarios; source generators are the AOT-compatible replacement.

---

## Q21. How does reflection interact with nullable reference type annotations?

**Concepts**
- NullableAttribute — compiler-emitted metadata encoding nullability intent
- NullabilityInfoContext for reading nullability annotations via reflection
- Nullable annotations as compile-time-only static analysis, not runtime enforcement
- Generic type argument nullability vs top-level type nullability

**Answer**

Nullable reference type annotations are a compile-time construct: the `?` on a reference type produces no runtime representation in the CLR type system because reference types are always nullable at the IL level. The compiler emits a `[Nullable]` or `[NullableContext]` attribute into the assembly to record the intent, but a standard `PropertyInfo.PropertyType` returns the same `Type` object whether or not the property was declared nullable.

To actually read nullability information at runtime, .NET 6 introduced `NullabilityInfoContext`. Calling `context.Create(propertyInfo)` returns a `NullabilityInfo` with `ReadState` and `WriteState` properties that report whether the property is nullable, non-nullable, or unknown. This is what frameworks like ASP.NET Core's model binder and EF Core use to determine whether a property must be provided. For generic types, the context also exposes `GenericTypeArguments` with per-argument nullability, so `Dictionary<string, string?>` is representable. The practical implication is that reflection-based libraries — serializers, validators, ORMs — must opt into `NullabilityInfoContext` to honour nullable annotations; they cannot infer this from `PropertyType` alone.

---

## Q22. What security permissions historically gated reflection, and what changed in modern .NET?

**Concepts**
- Code Access Security (CAS) in .NET Framework — permission demands gating reflection
- ReflectionPermission — required for non-public member access in partial-trust environments
- Partial-trust AppDomains hosting untrusted code (XAML browser apps, ClickOnce)
- CAS removal in .NET Core — all code runs full-trust within the process boundary
- Modern security boundary — process isolation and OS-level sandboxing instead of CAS

**Answer**

In .NET Framework, Code Access Security used `ReflectionPermission` to gate access to non-public members. Code running in a partial-trust context — such as an XAML browser application (XBAP) or a ClickOnce sandbox — was denied the `MemberAccess` flag of `ReflectionPermission`, which meant `BindingFlags.NonPublic` reflection calls would throw `SecurityException`. Accessing private fields of other assemblies required `RestrictedMemberAccess`, which was only granted if the caller had at least the same grant set as the target assembly.

Modern .NET (Core, 5+) removed CAS entirely. There are no permission demands, no partial-trust AppDomains, and no `ReflectionPermission` type that gates behaviour. All managed code within a process runs at full trust. The security model shifted to process-level isolation: untrusted code should run in a separate process (or container) with OS-level sandboxing, not in a sandboxed AppDomain within the same process. The practical consequence is that non-public reflection — accessing private fields, invoking private methods — works without any special permission grant, which is both convenient for testing frameworks and something to be aware of when reviewing code that assumes CAS enforcement.

---

### 03. Regular Expressions

## Q1. What is the purpose of the `Regex` class in C#?

**Concepts**
- System.Text.RegularExpressions.Regex as the BCL's pattern-matching engine
- Pattern compilation — NFA-based automaton constructed from a regex string
- Static vs instance Regex — parse-once vs parse-every-call trade-off
- RegexOptions flags — Compiled, IgnoreCase, Multiline, Singleline, CultureInvariant
- MatchTimeout — guard against catastrophic backtracking on untrusted input

**Answer**

The `Regex` class wraps a compiled non-deterministic finite automaton (NFA) that searches, matches, extracts, replaces, and splits strings according to a pattern. Creating a `new Regex(pattern)` instance parses the pattern once and builds the automaton; every subsequent call reuses that structure, which means I pay the parse cost only once when I hold the instance in a `static readonly` field.

The static helpers like `Regex.IsMatch(input, pattern)` re-parse the pattern on every call and are only appropriate for cold paths. For hot paths I use instance methods on a cached object or, in .NET 7+, a source-generated regex via `[GeneratedRegex]`, which emits the automaton at compile time with zero startup cost. `RegexOptions.Compiled` adds JIT compilation on top of the automaton construction, giving faster execution but higher initialization cost — worthwhile for patterns that run millions of times but counterproductive for patterns used only occasionally.

---

## Q2. Explain `Regex.IsMatch`, `Match`, `Matches`, `Replace`, and `Split`.

**Concepts**
- IsMatch — boolean existence check, stops at first match
- Match — returns first Match object with groups and captures
- Matches — lazy enumeration of all non-overlapping matches
- Replace — substitutes matches with a literal string or MatchEvaluator delegate
- Split — splits the input on each match boundary

**Answer**

`IsMatch` is the cheapest call because the engine stops as soon as it finds one match. `Match` also finds the first match but returns a `Match` object so I can read captured groups, indices, and lengths. To find every occurrence I call `Matches`, which returns a `MatchCollection` (lazy until iterated), or in .NET 7+ I use `EnumerateMatches` for a zero-allocation value-type enumerator.

`Replace` has two forms: passing a replacement string with backreferences like `$1` for group 1, and passing a `MatchEvaluator` delegate that receives each `Match` and returns the replacement string, which is useful for context-sensitive substitutions. `Split` cuts the input at each match boundary and returns the segments between them; any capturing groups in the pattern are included in the result array as additional elements, which is a common source of surprising output when groups are added to an existing split pattern without expecting the extra entries.

---

## Q3. What is the difference between verbatim regex strings (`@"\d+"`) and escaped regular strings?

**Concepts**
- C# string escape sequences vs regex escape sequences — two layers of escaping
- Verbatim string literal — @"…" suppresses C# backslash interpretation
- Backslash doubling in non-verbatim strings for regex metacharacters
- Readability argument for verbatim strings in most regex contexts

**Answer**

A regex pattern needs backslashes to express character classes like `\d`, `\w`, and `\s`. In a standard C# string literal, backslash is the escape character, so `"\d+"` would be interpreted as the escape sequence `\d` — which is not a recognized C# escape, causing a compile warning or error depending on the context. To reach the regex engine with a literal backslash I must double it: `"\\d+"`. Verbatim strings with `@` suppress C# escape processing entirely, so `@"\d+"` passes `\d+` directly to the regex engine without doubling.

The practical rule is to default to verbatim string literals for regex patterns because patterns frequently contain backslashes and the single-backslash form is far more readable. The only case where a non-verbatim string might matter is when the pattern itself contains the double-quote character and I need to embed it as `\"`, though verbatim strings handle that with `""` instead. Mixing both layers of escaping — C# string escapes plus regex escapes — is the most common source of regex bugs when new developers hand-write patterns.

---

## Q4. What are common metacharacters candidates should know (`.`, `*`, `+`, `?`, `^`, `$`, `\d`, `\w`, groups)?

**Concepts**
- Anchors — ^ and $ for start/end of string (or line in Multiline mode)
- Quantifiers — * (zero or more), + (one or more), ? (zero or one)
- Dot — . matches any character except newline (unless Singleline)
- Character class shortcuts — \d digit, \w word char, \s whitespace
- Capturing group () vs non-capturing group (?:) vs named group (?<name>)

**Answer**

The dot `.` matches any character except a newline by default — with `RegexOptions.Singleline` it matches newlines too. `^` anchors the match to the start of the input string; with `RegexOptions.Multiline` it additionally matches after each newline. `$` mirrors that for the end of input or line.

Quantifiers attach to the preceding atom: `*` means zero or more, `+` means one or more, and `?` means zero or one. By default they are greedy — they consume as many characters as possible while still allowing the overall pattern to match. A trailing `?` makes them lazy (`*?`, `+?`), matching as few characters as possible. `\d` is shorthand for `[0-9]`, `\w` for `[a-zA-Z0-9_]`, and `\s` for any whitespace character. Parentheses `()` create capturing groups whose matched text is accessible via `Match.Groups[n]`; `(?:…)` is a non-capturing group for grouping without recording, and `(?<name>…)` creates a named group accessible via `Groups["name"]`. Named groups are preferable in any pattern complex enough to have more than two captures because the names survive pattern refactoring and read more clearly at the call site.

---

## Q5. What is catastrophic backtracking, and what pattern shapes trigger it?

**Concepts**
- NFA backtracking — engine revisits positions when a match attempt fails
- Nested quantifiers — (a+)+ causing exponential state exploration
- Ambiguous alternation — overlapping alternatives causing repeated retry
- ReDoS — Regex Denial of Service through adversarially crafted input
- Mitigations — atomic groups, possessive quantifiers, NonBacktracking engine

**Answer**

Catastrophic backtracking occurs when the regex engine's NFA explores an exponential number of possible matching paths before deciding a match is impossible. The classic trigger is nested quantifiers like `(a+)+` applied to a string that nearly matches: the outer quantifier tries each combination of how many `a`s each inner group consumes, and the number of combinations grows exponentially with input length. An input of `"aaaaab"` forces the engine to try every partitioning before failing.

Ambiguous alternation causes the same problem when alternatives share a prefix: `(cat|catch)+` against a string ending in `catca` makes the engine try both branches at every position before giving up. The practical impact is a Regex Denial of Service (ReDoS): an attacker submits a carefully crafted string to a web endpoint that applies a vulnerable pattern, hanging the thread for seconds or longer.

Mitigations include: restructuring the pattern to eliminate ambiguity (atomic groups `(?>…)` prevent backtracking into a group once it has matched), always setting a `MatchTimeout`, and using `RegexOptions.NonBacktracking` in .NET 7+ which switches to a linear-time DFA-based engine that cannot catastrophically backtrack but also cannot support backreferences or lookarounds.

---

## Q6. How do you mitigate ReDoS using `Regex.MatchTimeout` or the `matchTimeout` parameter in .NET?

**Concepts**
- MatchTimeout constructor parameter — aborts match after elapsed time
- RegexMatchTimeoutException — thrown when timeout expires
- Timeout as defense-in-depth even for fixed known-safe patterns
- RegexOptions.NonBacktracking — linear-time engine alternative to timeout

**Answer**

The `Regex` constructor accepts a `TimeSpan` argument that limits how long a single match operation may run. When the timeout elapses, the engine throws `RegexMatchTimeoutException` instead of continuing indefinitely. I catch that exception at the call site and treat it as a bad-input signal — return a 400, log the offending pattern and input length, and fail the operation rather than hanging the thread.

The timeout applies per match operation, not per character, so a 500ms timeout stops the engine after 500ms regardless of how much of the input has been processed. For patterns that run against user-supplied data — log redaction, search boxes, document processors — I set a tight timeout (200–500ms) as a first line of defence. In .NET 7+ a better first line of defence for untrusted patterns is `RegexOptions.NonBacktracking`, which switches to a linear-time engine and makes catastrophic backtracking structurally impossible at the cost of not supporting backreferences and lookarounds. For trusted fixed patterns I still set a timeout as insurance against accidental pattern edits that introduce backtracking vulnerability.

---

## Q7. What happens when a regex times out — which exception is thrown?

**Concepts**
- RegexMatchTimeoutException — the specific exception type thrown on timeout
- InfiniteMatchTimeout — default value meaning no timeout is set
- Catch-and-fail pattern — treating timeout as bad input rather than retrying
- Timeout scope — per-operation, not per-character or per-method call

**Answer**

When a match operation exceeds the `MatchTimeout` value set on the `Regex` instance, the engine throws `System.Text.RegularExpressions.RegexMatchTimeoutException`. This exception derives from `TimeoutException`, so callers can catch either type. The default timeout is `Regex.InfiniteMatchTimeout` (a negative `TimeSpan`), which means the engine runs without bound if no timeout is specified in the constructor.

The correct handling is to catch `RegexMatchTimeoutException` at the outermost boundary of the operation that applies the pattern — not to retry, since a retry would immediately trigger the timeout again. Instead I return an error to the caller (a 400 for a web endpoint, a validation failure for a data pipeline) and log the event along with the input length, which makes it easy to detect ReDoS attempts in monitoring. The timeout fires based on elapsed wall-clock time from the start of that single call to `IsMatch`, `Match`, `Matches`, or `Replace` — it does not span multiple separate calls to the same instance.

---

## Q8. What is atomic grouping's role in preventing backtracking explosions?

**Concepts**
- Atomic group (?>…) — prevents engine from re-entering a group once it has matched
- Possessive quantifiers ++, *+, ?+ — syntactic shorthand for atomic quantification
- Backtracking cut — atomic group commits to its current match, discards saved states
- Performance vs expressiveness trade-off

**Answer**

An atomic group `(?>…)` tells the engine: once this sub-expression has matched, do not backtrack into it even if a later part of the pattern fails. Normally the engine saves alternative positions inside every group so it can retry with fewer characters consumed. Atomic groups discard those saved positions immediately on success, which cuts the exponential growth of states that causes catastrophic backtracking.

Consider `(?>a+)b` against `"aaa"`. The atomic group matches all three `a`s and commits. When `b` fails at end-of-string, the engine cannot backtrack into the group to try two `a`s and see if `b` matches — it simply fails the overall match immediately. The pattern `(?>\d+)\.` is a classic example: the atomic group grabs all digits without leaving retrial points, so trying the whole pattern against a long digit-only string fails in linear time instead of quadratic. Possessive quantifiers like `\d++` are equivalent sugar in engines that support them. The trade-off is that atomic groups can cause the pattern to fail on inputs that a non-atomic version would have matched through backtracking, so they are only correct when you know the group should always match maximally.

---

## Q9. How does culture affect case-insensitive matching, and what does `RegexOptions.CultureInvariant` do?

**Concepts**
- RegexOptions.IgnoreCase — enables case-insensitive matching using current culture
- Turkish I problem — lowercase of 'I' differs between Turkish and invariant cultures
- RegexOptions.CultureInvariant — uses invariant culture rules for case folding
- Combining IgnoreCase + CultureInvariant as the safe default for non-linguistic data

**Answer**

`RegexOptions.IgnoreCase` makes the engine treat uppercase and lowercase letters as equivalent, but the definition of "equivalent" depends on the culture. The infamous Turkish I problem illustrates the risk: in Turkish locale, the lowercase of `'I'` (dotless-I) is `'ı'` not `'i'`, so `Regex.IsMatch("FILE", @"file", RegexOptions.IgnoreCase)` returns `false` on a machine with Turkish culture settings because the case-folding of `'F'`, `'I'`, `'L'`, `'E'` uses Turkish rules.

`RegexOptions.CultureInvariant` instructs the engine to use the invariant culture for all case comparisons, which gives consistent results regardless of the machine's locale. For identifiers, protocol tokens, HTTP headers, and any ASCII-range text that has no linguistic meaning, I always pair `IgnoreCase` with `CultureInvariant`. For natural-language search — finding a Turkish word in a Turkish document — I omit `CultureInvariant` so the correct linguistic rules apply. The same principle applies to the `StringComparison.OrdinalIgnoreCase` vs `StringComparison.CurrentCultureIgnoreCase` distinction in plain string comparisons.

---

## Q10. When should you compile regexes with `RegexOptions.Compiled` (or source generators in .NET 7+)?

**Concepts**
- RegexOptions.Compiled — JIT-compiles automaton to IL at construction time
- Higher startup cost vs faster per-match execution
- GeneratedRegex attribute — compile-time code generation replacing runtime compilation
- Caching strategy — static readonly instances vs per-request construction

**Answer**

`RegexOptions.Compiled` tells the runtime to JIT-compile the regex automaton into native code when the `Regex` object is constructed, which makes each match call faster but increases startup time and memory usage. The break-even point is roughly a few thousand match operations — below that, the interpreter is faster overall because it avoids the compilation overhead.

The rule of thumb is to use `Compiled` only on `static readonly` instances that will run on a hot path many times across the application's lifetime. Never create a `new Regex(..., Compiled)` per request or per call because the IL compilation cost is paid on every construction without any reuse benefit.

In .NET 7+ the better option is `[GeneratedRegex(@"^pattern$", RegexOptions.Compiled)]` on a partial method, which moves compilation entirely to build time via a source generator. The generated code has zero runtime startup cost, works correctly under NativeAOT, and is faster than `Compiled` because the C# compiler can make static optimizations the JIT cannot. For new code targeting .NET 7+ I default to `[GeneratedRegex]` for all hot-path patterns and reserve runtime `Compiled` only for dynamically constructed patterns that are nonetheless reused repeatedly.

---

## Q11. What is `RegexOptions.NonBacktracking` (.NET 7+), and what trade-offs does it have?

**Concepts**
- DFA-based engine — deterministic finite automaton, linear time complexity
- O(n) guarantee — match time proportional to input length, not pattern complexity
- Feature restrictions — no backreferences, no lookarounds, no atomic groups
- ReDoS elimination — structural impossibility of catastrophic backtracking

**Answer**

`RegexOptions.NonBacktracking` switches the regex engine from the default NFA (non-deterministic finite automaton) to a DFA (deterministic finite automaton). A DFA processes each input character exactly once, which means match time is O(n) where n is the input length regardless of how complex or adversarial the pattern is. Catastrophic backtracking is structurally impossible because the DFA has no backtracking mechanism at all.

The trade-off is feature loss: patterns using backreferences (`\1`), lookaheads (`(?=…)`), lookbehinds (`(?<=…)`), and atomic groups are not supported and will throw `ArgumentException` at construction time. This rules out a significant portion of real-world patterns. The practical use case is untrusted or user-supplied patterns applied to potentially long inputs — for example, a search feature where users supply filter expressions against large text documents. In that scenario I validate that the user-submitted pattern compiles with `NonBacktracking` and catch `ArgumentException` to reject unsupported constructs, then run the match with a short timeout as an additional guard layer.

---

## Q12. How do named capture groups work, and how do you read them from `Match.Groups`?

**Concepts**
- Named group syntax (?<name>…) in pattern
- Match.Groups["name"] vs Groups[n] for access
- Group.Success guard before reading Value
- Named groups in replacement strings with ${name} syntax

**Answer**

A named capture group is written as `(?<name>…)` in the pattern. After a successful match, I access the captured text via `match.Groups["name"].Value`. Named groups are also accessible by their numeric index — they are assigned numbers after all unnamed groups in pattern order — but I prefer name-based access because it survives pattern refactoring without requiring index updates across the codebase.

Before reading `Value` I check `group.Success`, because an optional group (surrounded by `?` or inside an alternation arm that did not match) will have `Success == false` and `Value == ""`, which is indistinguishable from a group that captured an empty string. `Group.Index` and `Group.Length` give the position and length of the captured substring in the original input, which is useful for highlighting or replacing at specific offsets. In `Replace` with a `MatchEvaluator` delegate I can read named groups from the passed `Match` object; in the replacement string syntax I reference them as `${name}`. Named groups also appear in `Regex.GetGroupNames()`, which enables generic reflection-style code that iterates all captures without hardcoding names.

---

## Q13. What is the difference between greedy and lazy quantifiers (`+` vs `+?`)?

**Concepts**
- Greedy quantifier — matches as many characters as possible, then backtracks
- Lazy quantifier — matches as few characters as possible, then expands
- Impact on match boundaries in patterns with multiple quantifiers
- Greedy dot `.*` spanning unexpected regions in markup/structured text

**Answer**

A greedy quantifier like `+` or `*` tells the engine to consume as many characters as possible while still allowing the overall pattern to succeed. It starts by matching the maximum possible extent, then backtracks one character at a time if the rest of the pattern fails. A lazy quantifier like `+?` or `*?` does the opposite: it matches as few characters as possible and expands one character at a time until the rest of the pattern succeeds.

The difference is most visible with patterns like `<.+>` applied to `"<a>text</a>"`. The greedy version matches from the first `<` to the last `>`, consuming `<a>text</a>` as a single match. The lazy version `<.+?>` matches `<a>` first, then finds `</a>` as the second match. Lazy quantifiers are not inherently faster — they still backtrack (in the expand direction) — so they can still cause catastrophic backtracking when the pattern allows exponential state exploration. The choice between greedy and lazy should reflect the semantics of what you are matching, not a performance assumption.

---

## Q14. When should you prefer `Regex` over simple `string.Contains` / `Split` for maintainability?

**Concepts**
- Structural pattern vs literal search — regex for variable-shape input
- Maintenance cost of regex — readability and fragility of complex patterns
- string.Contains/Split — faster for fixed-literal searches
- Span-based string methods — high-performance alternative for simple splits

**Answer**

I prefer `string.Contains`, `IndexOf`, or `Split` whenever the text I am searching for is a fixed literal or a simple separator, because those methods are faster, require no pattern compilation, and are immediately readable to any developer. `Regex` pays off when the shape of the text is variable — for example, matching a date in the form `dd/mm/yyyy` where day, month, and year can vary, or extracting all key=value pairs regardless of whitespace around the equals sign.

The maintenance cost of regex rises sharply with pattern length and combinatorial nesting. A 60-character pattern that validates an email address is defensible; a 300-character pattern that validates, extracts, and transforms all in one pass becomes a maintenance liability. I split complex validation into a structural regex for basic shape plus plain C# for business rules that are expressed more clearly in code than in pattern syntax. The `.NET 7+` source-generator approach shifts the "explain this pattern" burden to a comment alongside the `[GeneratedRegex]` attribute, which at least keeps the pattern and its documentation colocated. For hot paths with fixed separators, the `MemoryExtensions.Split` (or `ReadOnlySpan<char>.Split` in .NET 8) family avoids allocations entirely.

---

## Q15. How do you validate input with regex without using it as a full parser (e.g., email, phone)?

**Concepts**
- Regex as shape gate — not semantic or deliverability verification
- Anchoring — ^ and $ to prevent substring match passing full-field validation
- Layered validation — regex for format, then domain rules in plain C#
- MailAddress / PhoneNumberUtil as format-specific libraries vs hand-rolled regex

**Answer**

Regex validates the structural shape of a string but cannot verify meaning or reachability. For email, a regex confirms that the input looks like `something@something.tld` — it cannot check whether the mailbox exists, whether the domain has MX records, or whether the address belongs to a disposable provider. I use regex as the cheapest first gate that eliminates clearly invalid input before more expensive checks.

The critical rule is anchoring: `^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$` must include `^` and `$` so that `"junk alice@example.com junk"` fails rather than matching as a substring. For phone numbers, `System.Text.RegularExpressions` can enforce digit count and separator shape, but normalising international formats and validating country-code assignments is better left to a library like `libphonenumber-csharp` which embeds the carrier data the regex cannot encode. The pattern for maintainable validation is: one anchored regex for structural shape, then plain C# or a domain library for business rules, and explicit error messages per failed check rather than a single pass/fail boolean that gives callers no actionable feedback.

---

### 04. Var, Dynamic & Special Keywords

## Q1. Explain `var` — what is known at compile time vs runtime?

**Concepts**
- Compile-time type inference from the initializer expression
- Strong static typing preserved — no runtime type change
- Readability benefit for long generic type names
- Restrictions — no fields without initializer, no reassignment to different type

**Answer**

`var` instructs the compiler to infer the static type of a local variable from the initializer expression at compile time, so the variable still has a fixed strong type after compilation — there is no runtime type change. If inference fails or the initializer is missing, the compiler reports an error.

- `var s = "hi";` is compile-time `string`, not a runtime-decided type.
- `var` cannot declare fields without an initializer and cannot change type on reassignment.
- Reflection and `GetType()` at runtime see the inferred type, identical to an explicit declaration.
- Inference improves readability for long generic types while preserving full static checking.

---

## Q2. When is `var` required (anonymous types) vs merely convenient?

**Concepts**
- Anonymous type — compiler-generated unspeakable type name requiring var
- LINQ select new { } projections as the primary anonymous-type use case
- Explicit type preferred when initializer does not reveal the type

**Answer**

`var` is mandatory when declaring anonymous types because the compiler generates a type name that is not expressible in source, such as `var row = new { Id = 1, Name = "A" };`. For all named types, `var` is optional convenience — explicit types remain valid and sometimes clearer for public APIs and unclear initializers.

- Anonymous projection in LINQ `select new { ... }` requires `var` or implicit typing in query syntax.
- Prefer explicit types when the initializer does not make the type obvious (`var x = GetValue();`).
- Team style guides often allow `var` when the right-hand side spells out the type clearly.
- `var` does not disable nullable warnings — inferred reference types carry NRT annotations.

---

## Q3. Explain the `dynamic` keyword and the DLR's role.

**Concepts**
- Dynamic Language Runtime (DLR) — runtime binding infrastructure
- Call site caching — DLR caches resolved bindings for repeated calls
- Compile-time member checking disabled for dynamic receivers
- RuntimeBinderException — runtime equivalent of a compile-time member error

**Answer**

`dynamic` tells the compiler to defer member resolution to runtime via the Dynamic Language Runtime (DLR), which performs binding at execution time using call sites that cache resolved members after the first successful bind. Static typing is bypassed for `dynamic` receivers, so compile-time member checking is disabled.

- The DLR coordinates overload resolution, implicit conversions, and `dynamic` invocation across languages and COM interop.
- First access pays binding cost; subsequent calls use cached rules until arguments types change materially.
- Errors like misspelled property names surface as `RuntimeBinderException` at runtime.
- Use sparingly for JSON DOMs, COM, and truly dynamic payloads — not for ordinary application logic.

---

## Q4. What is the difference between `var`, `dynamic`, and `object`?

**Concepts**
- var — compile-time inferred static type, full member checking
- object — base type requiring explicit cast for specific members
- dynamic — runtime member binding via DLR, no compile-time checks
- Pattern matching on object vs DLR dispatch on dynamic

**Answer**

`var` is compile-time type inference with full static checking; `object` is the base type that requires explicit casts before calling specific members; `dynamic` skips compile-time member checks and resolves calls at runtime through the DLR. All three can hold references to heterogeneous data, but only `dynamic` defers binding.

| Keyword | Compile-time type | Member calls |
|---|---|---|
| `var` | Inferred concrete type | Statically checked |
| `object` | `object` | Requires cast |
| `dynamic` | `dynamic` | Runtime bound |

- Assigning a `string` to `object` needs `(string)obj` or pattern matching before `Length`.
- `dynamic` appears to call members directly but fails at runtime if the target lacks them.
- Prefer `object` with pattern matching in modern C# when shape varies but you want exhaustiveness.

---

## Q5. When does `dynamic` defer member binding to runtime, and what errors appear only then?

**Concepts**
- Dynamic expression — any member access on a dynamic-typed receiver deferred to runtime
- Propagation — return type of dynamic call is also dynamic
- RuntimeBinderException for missing member, wrong arity, or ambiguous overload
- IDE impact — no IntelliSense, no Find References at dynamic call sites

**Answer**

Any member access, method call, indexer, or operator on a `dynamic` expression is resolved when that line executes, not during compilation. Missing members, wrong argument types, and ambiguous overloads throw `RuntimeBinderException` or related runtime errors that would have been compile errors on static types.

- `dynamic d = GetPayload(); d.Totla();` compiles but fails at runtime on the typo.
- Return types of dynamic calls are `dynamic` unless converted, propagating deferred binding downstream.
- Debugging is harder — IDE refactor and Find References skip dynamic sites.

---

## Q6. What is `DynamicObject`, and when would you subclass it?

**Concepts**
- DynamicObject base class — provides override hooks for dynamic operations
- TryGetMember, TrySetMember, TryInvokeMember overrides
- Dictionary-backed property bag pattern
- ExpandoObject as the built-in DynamicObject for ad hoc bags

**Answer**

`DynamicObject` is a base class for types that participate in the DLR by overriding `TryGetMember`, `TrySetMember`, `TryInvokeMember`, and related hooks to define custom dynamic behavior. Subclass it when building dynamic proxies, DSL objects, or dictionary-backed models that expose members not declared as C# properties.

- `ExpandoObject` is a built-in `DynamicObject` mapping names to object values.
- Override hooks return `true` when the operation succeeds and set `result` for get/invoke paths.
- Enables Ruby-like dynamic APIs while remaining callable from C# with `dynamic` references.
- Prefer static interfaces when shape is stable — custom `DynamicObject` is for truly open-ended models.

---

## Q7. What is `ExpandoObject`, and how does it differ from `Dictionary<string, object>`?

**Concepts**
- ExpandoObject — IDynamicMetaObjectProvider + IDictionary<string, object>
- Dynamic member syntax vs dictionary indexer for the same underlying store
- Thread safety — neither ExpandoObject nor Dictionary are thread-safe by default
- JSON serialization compatibility with ExpandoObject

**Answer**

`ExpandoObject` implements `IDynamicMetaObjectProvider` and `IDictionary<string, object>`, so callers can use dynamic member syntax or dictionary APIs on the same bag of name-value pairs. A plain dictionary only supports indexer and dictionary methods unless wrapped — not dynamic member binding without extra glue.

- `dynamic bag = new ExpandoObject(); bag.Score = 10;` adds a property at runtime.
- Cast to `IDictionary<string, object>` to enumerate keys or serialize to JSON with appropriate options.
- Expando graphs suit lightweight JSON merge scenarios; typed models are safer for core domain entities.
- Thread safety is not automatic — treat expando instances like ordinary mutable dictionaries.

---

## Q8. Explain `nameof` — how does it help refactoring and logging?

**Concepts**
- nameof — compile-time string literal from a symbol's unqualified name
- Refactor-safe — IDE rename updates all nameof references automatically
- ArgumentNullException and INotifyPropertyChanged as primary use cases
- Restrictions — simple name forms only, no runtime expressions

**Answer**

`nameof(expression)` resolves at compile time to the unqualified name string of a variable, type, or member, so renaming the symbol updates every `nameof` reference through the IDE refactor tools. It avoids magic strings in exceptions, logging, and property-change notifications.

- `nameof(customer.Email)` yields `"Email"` even if the expression type is broader than the member's declaring type.
- Safer than `"Email"` literals that drift during rename — compiler ties `nameof` to the symbol.
- Works on parameters, methods, and types — `nameof(OrderService.PlaceOrder)` for diagnostic messages.
- Does not evaluate runtime expressions — only simple name forms are allowed.

---

## Q9. What is the `global::` qualifier, and when is it needed to disambiguate namespaces?

**Concepts**
- global:: — root namespace alias for unambiguous type lookup
- Namespace shadowing — user-defined type hiding a BCL type by the same name
- Using aliases as a lighter alternative to global::
- Generated code commonly using global:: for deterministic resolution

**Answer**

The `global::` prefix starts name lookup from the root namespace scope, bypassing user-defined aliases or nested namespaces that shadow system types. Use it when a project namespace such as `System` or `Email` collides with BCL types like `System.String`.

- Example: `global::System.IO.File` when your namespace hierarchy defines a conflicting `File` class.
- Aliases (`using IO = ...`) are usually enough, but `global::` is the unambiguous escape hatch.
- Generated code and analyzers occasionally emit `global::` for deterministic resolution.
- Rare in hand-written code — fix namespace naming first when collisions recur.

---

## Q10. What is `default` literal (C# 7.1+) vs `default(T)`?

**Concepts**
- default literal — context-inferred default value without repeating the type
- default(T) — explicit type-parameterised form, required when type is ambiguous
- Zero value semantics — 0 for numerics, null for references, zeroed fields for structs

**Answer**

The `default` literal lets the compiler infer the target type from context, so `default` in `int x = default;` means `0` and in `string? s = default;` means `null` without repeating the type name. `default(T)` remains valid in generic code where the type parameter `T` is not known as a specific type at the call site.

- `default` improves readability in ternary and coalescing expressions with nullable reference types.
- For unconstrained `T`, `default` and `default(T)` both yield null for reference types and zeroed value types.
- Cannot use `default` where type inference is ambiguous — add an explicit type in those cases.

---

## Q11. What is `@` verbatim identifier syntax (`@class`, `@event`) used for?

**Concepts**
- @ prefix escaping C# reserved keywords as identifiers
- Interop and generated code using reserved-word column or property names
- Distinct from @ verbatim string literal prefix

**Answer**

Prefix `@` allows identifiers that coincide with C# keywords — `@class`, `@event`, `@int` — so generated or interop code can use reserved words as names. Verbatim identifiers are the same CLR names without the `@` at runtime.

- Common for XML or database columns named `class` mapped into C# properties.
- `@` on strings (`@"C:\path"`) is a separate feature — verbatim string literals, not identifiers.
- Prefer renaming to non-keyword names in hand-written domain models when possible.
- JSON property names can still map via attributes without `@` in source identifiers.

---

## Q12. What is unsafe code, and when are pointers justified in C#?

**Concepts**
- unsafe keyword and /unsafe compiler flag as opt-in gates
- Pointer arithmetic bypassing GC safety checks
- fixed statement for pinning managed objects during pointer access
- Span<T> and Memory<T> as safe alternatives covering most use cases

**Answer**

Unsafe code blocks, enabled with `/unsafe` and the `unsafe` keyword, allow pointer arithmetic and fixed buffers for interop with native APIs or extreme hot paths where spans still cannot express the required semantics. Most application code never needs unsafe — `Span<T>`, `Memory<T>`, and `stackalloc` cover many performance scenarios safely.

- Required for some legacy C library interop expecting `byte*` parameters.
- Misuse introduces buffer overruns and GC pinning hazards — code review and tests are mandatory.
- Modern BCL moves toward safe abstractions; unsafe is opt-in and excluded from some sandboxed contexts.
- Keep unsafe isolated in small vetted modules with clear documentation.

---

## Q13. What is `stackalloc`, and how does it relate to performance-sensitive code?

**Concepts**
- stackalloc — allocates on the call stack, avoiding heap/GC pressure
- Span<T> integration — safe stackalloc without unsafe blocks (C# 7.2+)
- Stack overflow risk for large allocations
- ArrayPool<T> as the fallback for variable or large buffer sizes

**Answer**

`stackalloc` allocates a block of memory on the stack for value types, avoiding heap allocations for small temporary buffers when used with `Span<T>` in safe contexts (C# 7.2+). It suits short-lived arrays in parsing, crypto, or formatting hot paths where heap pressure matters.

- `Span<int> buf = stackalloc int[32];` keeps allocation scoped to the method frame.
- Large `stackalloc` can overflow the stack — cap sizes and spill to `ArrayPool` for big buffers.
- C# 8 integrated `stackalloc` into safe patterns alongside `Span` without requiring unsafe blocks in many cases.
- Profile before micro-optimizing — allocator improvements in the runtime already reduce small array costs.

---

## Q14. What is `ref readonly` return, and how does it differ from returning by value?

**Concepts**
- ref readonly return — returns alias to existing storage, no copy
- Caller restriction — cannot mutate through a ref readonly reference
- Large struct performance — avoids copying struct bytes on every call
- Distinction from in parameters — in is for parameters, ref readonly is for returns

**Answer**

A `ref readonly` return exposes a read-only reference to an existing storage location (often a large struct field or array element) without copying bytes, while preventing the caller from mutating through that reference. Returning by value copies the entire struct, which can be expensive for large value types.

- Caller receives `ref readonly T` and reads fields without taking a writable alias.
- Useful for exposing items from internal buffers while preserving encapsulation.
- Distinct from `in` parameters — `ref readonly` is about return paths.
- Large readonly struct returns in hot loops are a primary use case for this feature.

---

## Q15. How does `dynamic` interact with extension methods (why don't they bind dynamically)?

**Concepts**
- Extension methods resolved statically at compile time by the C# compiler
- DLR operates only on instance member lookup, not static import lookup
- Cast-to-concrete-type workaround for calling extensions on dynamic values
- Design implication — static extension APIs require static receiver types

**Answer**

Extension methods are resolved statically at compile time based on the static type of the receiver expression, so a `dynamic` receiver does not see extension methods unless you cast to the static type or invoke the extension as a static call. The DLR does not participate in extension method lookup.

- `dynamic d = ...; d.Extension();` fails at runtime even if an extension exists for the runtime type.
- Call `MyExtensions.Extension((ConcreteType)d)` or cast before invoking extensions.
- Prefer static types or wrapper methods at the dynamic boundary instead of relying on extensions.

---

### 05. C# 7 Features

## Q1. What are tuple deconstruction and named tuple elements?

**Concepts**
- ValueTuple<T1,T2,…> as the backing struct for C# 7 tuple literals
- Named tuple elements — syntactic names in source, not runtime properties
- Tuple deconstruction — `var (x, y) = tuple;` assignment to separate variables
- Returning multiple values without a custom struct or out parameters

**Answer**

C# 7 tuples are value types backed by `System.ValueTuple`. I write `(int x, int y) point = (3, 4);` and the compiler stores element names `x` and `y` as metadata in the assembly, but the actual fields are named `Item1` and `Item2` at the IL level — which means tuple element names are a compile-time convenience, not a runtime property. Returning `(bool Success, string Message)` from a method lets callers write `var (ok, msg) = Validate(input);` using deconstruction, which the compiler expands to `ok = result.Item1; msg = result.Item2;`.

Deconstruction also works on any type that provides a `Deconstruct` method with `out` parameters, so I can add `Deconstruct` to my own classes and use the same syntax. Named elements improve readability at call sites but carry no semantic enforcement — renaming an element between a method return and the caller's deconstruct assignment produces no error because the names do not participate in method signatures.

---

## Q2. How do `out` variables declared inline in method calls work?

**Concepts**
- Inline out variable declaration — `int.TryParse(s, out var n)` scoping n to the enclosing block
- Scoped to enclosing block, not just the if-branch
- Discard _ for unused out parameters
- Default value of out variable when the Try method returns false

**Answer**

C# 7 allows declaring the `out` variable directly in the call expression: `if (int.TryParse(s, out var n))`. The variable `n` is scoped to the enclosing block — it remains accessible after the `if` statement — but its value is meaningful only when the method returns `true`. When the method returns `false`, `n` holds the default value for its type (zero for `int`), so I always guard reads on the boolean return rather than reading the out variable unconditionally.

When I do not need the out value I can discard it with `_`: `int.TryParse(s, out _)` to check parseability without storing the result. Inline out variables work with any method that has `out` parameters, including `Dictionary.TryGetValue` and the pattern-matching `is` operator with out variables. The scoping change from C# 6 (where the variable had to be pre-declared before the call) removes the clutter of declaring a variable, setting it to default, then calling the method, which was the idiomatic but verbose pre-C# 7 form.

---

## Q3. What are discards (`_`), and where are they used (deconstruction, unused returns)?

**Concepts**
- Discard _ — write-only placeholder, communicates intentional non-use
- Use cases: unused out parameters, tuple deconstruction, standalone expression results
- Standalone discard `_ = expr;` to silence CS4014 for fire-and-forget Task calls
- Pattern matching _ as the wildcard/default arm

**Answer**

A discard `_` is a write-only placeholder that tells the compiler — and readers — that a value is intentionally discarded. In deconstruction, `var (id, _, name) = GetRow()` discards the second element. In out-parameter calls, `TryParse(s, out _)` checks parseability without storing the result. As a standalone statement, `_ = SomeTaskReturningMethod()` suppresses the "await the result" compiler warning when I deliberately fire and forget.

In switch expressions and pattern matching, `_` is the catch-all default arm — it matches any value not caught by earlier arms, making the switch exhaustive. Multiple discards `_` in a single statement are allowed because discards are not variables and do not shadow each other. The signal value of `_` is communicative: it tells the next developer that the value was considered and deliberately not stored, rather than accidentally forgotten.

---

## Q4. Explain pattern matching enhancements in C# 7 — `is` type patterns and `switch` patterns.

**Concepts**
- is type pattern — `expr is SomeType variable` combining type check and cast
- switch statement with type patterns and when guards
- First-match-wins ordering requirement in switch with type patterns
- Null exclusion — type patterns do not match null

**Answer**

C# 7 extended `is` beyond a simple type check: `if (shape is Circle c)` both checks that `shape` is a `Circle` and binds the value to `c` in scope, eliminating the separate cast. The switch statement gained type patterns with optional `when` guards: `case Circle c when c.Radius > 10` matches a `Circle` whose radius exceeds 10. The cases are evaluated top-to-bottom and the first matching case wins, so more specific guards must appear before more general ones — `case Circle c when c.Radius > 10` must precede `case Circle c`.

Type patterns never match `null`, which means I no longer need a `null` check before the `is` cast in most cases. These C# 7 patterns were the foundation for the much richer switch expressions and positional/property patterns added in C# 8 and later. The practical benefit is replacing chains of `if (x is T1) { … } else if (x is T2) { … }` with readable switch blocks that the compiler can analyse for completeness.

---

## Q5. What are `ref` returns and `ref` locals, and what safety rules apply?

**Concepts**
- ref return — returns an alias to an existing storage location rather than a copy
- ref local — local variable that aliases another storage location
- Safe-to-return rules — ref cannot outlive the storage it aliases
- async method restriction — ref locals and ref returns are not permitted in async methods

**Answer**

A `ref` return exposes a direct alias to an internal storage slot — for example an array element or a field — so callers can mutate the original location without a separate set call. `ref int slot = ref inventory[index]; slot -= qty;` mutates the array element in place. The compiler enforces safe-to-return rules: a ref return cannot alias a local variable whose lifetime ends at the method's return, and ref locals cannot escape their declaring scope. This prevents dangling reference bugs analogous to C++ dangling pointers.

The major restriction is that `ref` locals and `ref` returns are prohibited in `async` methods and iterators because those methods are compiled into state machines that store locals on the heap between suspension points — ref locals cannot be stored in heap fields, so the compiler rejects them. For synchronous hot paths such as physics simulations or data processing on large arrays, ref returns eliminate per-element copy overhead. For async code I read the value, modify it, and write it back through normal assignment.

---

## Q6. What is `ref`/`in`/`out` in the context of `ReadOnlySpan`-era performance APIs (conceptual link)?

**Concepts**
- in parameter — readonly ref pass, avoids struct copy without allowing mutation
- ref parameter — read-write alias, caller and callee share the same storage
- out parameter — write-only, callee must assign before returning
- ReadOnlySpan<T> as the practical alternative to in T for most string/buffer scenarios

**Answer**

The three modifier keywords express different ownership semantics for pass-by-reference. `out` means the callee is responsible for writing a value and the caller treats its current value as undefined; this is the pattern for `TryParse`. `ref` means both caller and callee can read and write the same storage location simultaneously; used for in-place mutation like `Interlocked.Increment(ref count)`. `in` is a readonly ref — the callee receives an alias to the caller's storage but cannot write through it, which avoids copying a large struct while preventing modification.

In the `ReadOnlySpan<T>` and `Span<T>` era, passing large buffers does not require `in T` for strings because `ReadOnlySpan<char>` already carries a reference without copying and is already readonly. The `in` modifier is most useful for large structs like matrices, vectors, or fixed-size blobs that are stack-allocated or embedded in arrays. Methods in `System.Numerics` and the BCL's cryptographic primitives use `in` extensively for this reason. The practical rule is: use `ReadOnlySpan<T>` for buffer-shaped data, `in` for large domain structs in hot paths, and plain pass-by-value for small structs where the copy cost is negligible.

---

## Q7. What are local functions, and how do they differ from lambdas for recursion and capture?

**Concepts**
- Local function — named method declared inside another method body
- Recursion support — local functions have a name to call themselves; lambdas do not
- Static local function (C# 8) — prevents accidental capture of outer variables
- Capture model — local function closes over variables by reference; lambda by reference too, but stored in a compiler-generated class

**Answer**

A local function is a named method defined inside another method. Unlike lambdas, local functions have a name and can call themselves recursively without the indirection of storing the lambda in a variable and then capturing that variable. They can also be `async`, `unsafe`, and accept `ref`/`out` parameters — constraints that lambdas either cannot express or express awkwardly.

Both local functions and lambdas capture outer variables by reference, but lambdas must be stored as delegates, which causes a heap allocation for the closure object. A local function that does not capture any outer variables is compiled as a static method with no allocation. In C# 8, marking a local function `static` explicitly prevents it from capturing any outer state, making the no-capture guarantee an enforced constraint rather than an accidental property. I use local functions when I need a helper that is logically scoped to one method, especially for recursive algorithms (tree traversal, factorial) or `async` helpers that the enclosing method composes.

---

## Q8. What are expression-bodied members beyond properties (methods, constructors, finalizers)?

**Concepts**
- Expression-bodied method — `=> expr;` replaces `{ return expr; }`
- Expression-bodied constructor — delegates to another constructor or sets a field inline
- Expression-bodied finalizer — `~T() => expr;` for single-expression cleanup
- Readability benefit for simple delegating members vs verbosity cost for complex logic

**Answer**

C# 6 introduced expression bodies for read-only properties: `public int Count => _list.Count;`. C# 7 extended the syntax to methods, constructors, finalizers, property getters and setters, and indexers. An expression-bodied method is `public string Format(int n) => n.ToString("N0");` — the arrow replaces both the braces and the `return` keyword. A constructor can use `=> field = value;` for a single-assignment body, and a finalizer can use `~ClassName() => _handle?.Dispose();` for a single cleanup expression.

The value is readability for members whose logic genuinely fits one expression. I do not use expression bodies to compress multi-step logic onto a single line; that harms debuggability because a breakpoint on the method body has nowhere specific to land. The right heuristic is: if the expression cannot be read at a glance and understood without tracing through intermediate values, it belongs in a block body with explicit variable names.

---

---

## Q9. What binary literals and digit separators (`0b1010`, `1_000_000`) improve in readability?

**Concepts**
- Binary literal 0b prefix — direct bit-pattern representation of integer constants
- Hex literal 0x prefix — the existing alternative for byte and flag values
- Digit separator _ — ignored by compiler, used to group digits visually
- Common use cases — bitmask flags, fixed-point constants, large numeric literals

**Answer**

C# 7 added two readability improvements to numeric literals. Binary literals use the `0b` prefix so I can write `const byte Mask = 0b0011_1100;` and see exactly which bits are set without mentally converting from decimal or hex. This is particularly clear for bitmask constants used with bitwise operations, where the visual alignment of bit positions carries meaning.

Digit separators `_` can appear anywhere inside a numeric literal — between any two digits — and are ignored by the compiler. `1_000_000` is the same as `1000000`, but the grouping makes the scale immediately obvious. The separator also works in hex and binary: `0xFF_FF_FF_FF` groups bytes, and `0b1111_0000_1111_0000` separates nibbles. The underscore cannot appear at the start, end, or adjacent to the `0b` or `0x` prefix, but it can otherwise be placed at any grouping boundary that aids reading.

---

---

## Q10. What is `throw` as an expression inside ternary/null-coalescing forms?

**Concepts**
- throw expression — throw usable as a value in expression contexts
- Null-coalescing throw: `value ?? throw new ArgumentNullException(...)`
- Ternary throw: `condition ? value : throw new InvalidOperationException(...)`
- Expression-bodied member guard clauses enabled by throw expressions

**Answer**

Before C# 7, `throw` was a statement and could only appear as a standalone line in a block. C# 7 promoted `throw` to an expression, meaning it can appear anywhere an expression is expected. The most common use is in null-coalescing guards: `_service = service ?? throw new ArgumentNullException(nameof(service));` which is more concise than a two-line if-block guard and works inside constructors, properties, and expression-bodied members.

In a ternary: `return count > 0 ? items[0] : throw new InvalidOperationException("empty");` — the `throw` arm satisfies the compiler's type expectation because `throw` expressions have type `T` for any `T` (they are classified as never returning). This lets expression-bodied properties perform guard checks inline: `public string Id => _id ?? throw new InvalidOperationException("not initialized");`. I use throw expressions for simple not-null and range guards; for complex validation with multiple checks and detailed error messages, block-body methods remain clearer.

---

---

## Q11. How do generalized async return types work (`ValueTask` as async return)?

**Concepts**
- Generalized async return type — any type with GetAwaiter() usable as async return
- ValueTask<T> — allocation-free async return for synchronous-completion hot paths
- Single-consumption rule — ValueTask must be awaited exactly once
- IValueTaskSource — poolable backing for non-trivial ValueTask paths

**Answer**

Before C# 7, `async` methods could only return `void`, `Task`, or `Task<T>`. C# 7 generalized this: any type that has a `GetAwaiter()` method returning an awaiter can be an async return type, provided it is annotated with `[AsyncMethodBuilder]`. This opened the door for `ValueTask<T>`, which avoids a heap allocation when the operation completes synchronously — common in cache-hit paths where the result is already in memory.

A `ValueTask<T>` is cheap to return synchronously: `return new ValueTask<T>(value);` allocates nothing. The trade-off is a strict single-consumption contract: I may only `await` the same `ValueTask` instance once, and I must not store it for later use unless I call `.AsTask()` first. This differs from `Task<T>`, which can be awaited multiple times. For very high-frequency APIs (like `PipeReader.ReadAsync`), `IValueTaskSource<T>` allows pooling the backing state machine, reducing allocations even on asynchronous completions. For ordinary application code, `Task<T>` remains the safer default and `ValueTask<T>` is adopted only after profiling shows measurable allocation pressure on a hot path.

---

---

## Q12. What are `default` in generic constraints improvements in C# 7?

**Concepts**
- default constraint disambiguation — resolving override vs explicit interface ambiguity
- C# 7.1 default literal replacing default(T) in typed contexts
- Override methods requiring default modifier when base has new default interface method

**Answer**

C# 7 and 7.1 brought two improvements around `default`. The `default` literal (C# 7.1) allows writing `default` without the explicit type in any context where the type can be inferred: `int x = default;` instead of `int x = default(int);`. This is more useful in generic code and optional parameter defaults than in concrete code, since the type is often obvious from context.

The `default` constraint improvement came alongside default interface methods in C# 8. When a base class member and a default interface method both exist for an override target, the compiler requires the `default` modifier on the override to disambiguate which baseline the override extends. This is a niche corner of the language primarily relevant to authors of frameworks that implement multiple interfaces with overlapping default methods. For everyday application code, the main benefit of these changes is cleaner generic type parameter defaults in method signatures and constructors, and slightly less repetition when initialising variables to their zero value.

---

---

### 06. C# 8 Features

## Q1. Explain nullable reference types — how do they differ from `Nullable<T>` value types?

**Concepts**
- Nullable value types Nullable<T>/T? — CLR-level struct wrapping a value with HasValue
- Nullable reference types — compiler annotation only, no CLR runtime change
- Flow analysis — compiler tracks nullability through assignments and checks
- Null-forgiving operator ! — suppresses warning without runtime guarantee

**Answer**

`Nullable<T>` (written `T?` for value types) is a CLR struct that wraps a value type with a `HasValue` flag, so `int?` occupies 8 bytes and carries actual runtime null semantics. Nullable reference types (C# 8+, enabled with `<Nullable>enable</Nullable>`) are entirely a compile-time annotation: the CLR still represents `string?` and `string` identically at runtime — both are just references that can be null. The `?` on a reference type instructs the compiler's flow analyser to warn when a potentially-null reference is dereferenced without a null check.

The null-forgiving operator `!` silences the compiler warning but does nothing at runtime — `metadata.Notes!.ToUpperInvariant()` still throws `NullReferenceException` if `Notes` is actually null. Nullable reference types are a code-correctness tool: they force null handling to be explicit and auditable, reducing the class of "forgot to check null" bugs, but only when the annotations are honest and suppressions are treated as technical debt rather than a quick way to clear CI.

---

## Q2. What do `?`, `!`, and `#nullable` directives mean at compile time?

**Concepts**
- ? on reference type — marks the type as potentially null, enabling null-safety warnings
- ! null-forgiving operator — suppresses a nullable warning without runtime effect
- #nullable enable/disable/restore — file-level or block-level nullable context control
- <Nullable> project property — sets default context for all files in the project

**Answer**

In a nullable-enabled context, `string?` means "this reference may be null and the caller must check before dereferencing". `string` (without `?`) means "this reference is expected to be non-null and the compiler will warn if it flows into a context that could be null". These annotations feed the compiler's flow analyser, which tracks assignments, conditionals, and null checks to determine whether a dereference is safe.

The `!` operator — called null-forgiving — suppresses the warning for a specific expression: `obj!.Property` tells the compiler "I know this is not null, stop warning". It is a documentation-to-compiler signal, not a runtime guard. The `#nullable enable` directive turns on nullable warnings for the remainder of the file; `#nullable disable` turns them off; `#nullable restore` returns to the project default. The `<Nullable>enable</Nullable>` MSBuild property sets the project-wide default and is the starting point for enabling NRTs across a codebase. Files still active in `disable` mode behave as C# 7 — all references are implicitly nullable-oblivious.

---

## Q3. Are nullable reference annotations enforced at runtime?

**Concepts**
- NRT as static analysis only — no IL change, no runtime enforcement
- NullReferenceException still possible despite correct NRT annotations
- [Required] attribute and model validation as separate runtime null enforcement
- Difference between NRT warnings (compile-time) and ArgumentNullException (runtime)

**Answer**

Nullable reference type annotations are purely a compile-time, static-analysis feature. The CLR's type system does not distinguish `string` from `string?` at runtime; both are represented as a reference that can hold null. Enabling `<Nullable>enable</Nullable>` does not add any runtime checks — a method annotated as returning `string` (non-nullable) can still return `null` if the implementation is wrong or comes from a nullable-unaware assembly.

The practical consequence is that NRT reduces bugs during development by catching potential null flows at compile time, but does not replace runtime guards at trust boundaries. For input coming from the network — JSON bodies, query strings, form fields — I use `ArgumentNullException.ThrowIfNull`, model validation attributes like `[Required]`, or explicit null checks after deserialization regardless of the NRT annotations in the type. The annotations help the static analyser guide code review; the guards protect against bad data at runtime. These two layers of defence complement rather than replace each other.

---

## Q4. What are default interface methods, and how do they relate to the diamond problem?

**Concepts**
- Default interface method (DIM) — interface member with a body, callable without override
- Binary-compatible interface evolution — adding DIMs without breaking existing implementors
- Diamond ambiguity — two interfaces providing conflicting DIMs for the same signature
- Explicit interface implementation as the resolution for diamond conflicts

**Answer**

C# 8 allows interfaces to declare members with bodies: `string Describe() => $"{Name} processor";`. Existing classes implementing the interface inherit the default implementation at runtime without recompiling, which makes it safe to add members to a shipped interface without a breaking change. If a class overrides the method, the override wins; if not, the interface body runs when called through an interface-typed reference.

The diamond problem arises when a class implements two interfaces that both provide a default implementation for the same signature: `class Worker : IProcessor, IReporter` where both define `string Describe()`. The compiler rejects the ambiguity — it cannot choose which default to use — and requires the class to either override `Describe()` explicitly or provide separate explicit implementations (`string IProcessor.Describe() => …; string IReporter.Describe() => …;`). The resolution must be explicit; there is no automatic "most derived wins" rule for conflicting interface defaults. DIMs are a library evolution tool, not an alternative to inheritance hierarchies.

---

## Q5. What are asynchronous streams (`IAsyncEnumerable<T>` and `await foreach`)?

**Concepts**
- IAsyncEnumerable<T> — sequence whose elements are produced asynchronously
- await foreach — iterates an async sequence, awaiting each MoveNextAsync call
- yield return in async methods — async iterator pattern
- [EnumeratorCancellation] and WithCancellation for cooperative cancellation

**Answer**

`IAsyncEnumerable<T>` is the async counterpart of `IEnumerable<T>`: instead of pulling all elements eagerly or yielding synchronously, the producer can `await` I/O or delays between yields. A method becomes an async iterator by declaring `async IAsyncEnumerable<T>` and using `yield return`:

```csharp
async IAsyncEnumerable<Document> ReadDocumentsAsync([EnumeratorCancellation] CancellationToken ct = default) {
    foreach (var batch in _batches) {
        await Task.Delay(50, ct);
        yield return await FetchAsync(batch, ct);
    }
}
```

The consumer uses `await foreach (var doc in ReadDocumentsAsync(token))`, which calls `MoveNextAsync()` on the underlying `IAsyncEnumerator<T>` and awaits each step without buffering the entire sequence. This is the correct pattern for paginated database reads, blob storage listings, and SSE feeds where loading everything into memory would cause OOM. To propagate cancellation correctly, I pass the token via `WithCancellation(token)` on the consumer side and annotate the producer's token parameter with `[EnumeratorCancellation]`, which lets the runtime merge the two token sources automatically.

---

## Q6. Explain null-coalescing assignment (`??=`) with examples.

**Concepts**
- ??= operator — assigns right-hand side only when the left-hand side is null
- Lazy initialization pattern — initialise a field or property on first access
- Short-circuit evaluation — right side not evaluated when left side is non-null
- Difference from ?? — ?? returns a value; ??= assigns and returns

**Answer**

The `??=` operator combines a null check and assignment: `field ??= new List<string>();` assigns a new list to `field` only when `field` is currently null, leaving it untouched if it already has a value. This is the idiomatic C# 8 form of the classic lazy-initialisation pattern `if (field == null) field = new List<string>();`.

The right-hand side is evaluated lazily — if the left side is non-null, the expression on the right is never executed. This makes `??=` safe to use with method calls that have side effects: `_cache ??= BuildExpensiveCache();` runs `BuildExpensiveCache` at most once. `??=` works on any nullable reference type or `Nullable<T>` value type. It does not provide thread safety — if two threads race to initialise the same field, both may see null and both may assign. For thread-safe lazy initialisation I use `Lazy<T>` or `Interlocked.CompareExchange`.

---

## Q7. Explain range (`..`) and index (`^`) operators — how does `^1` differ from `Length - 1`?

**Concepts**
- Index type ^ — end-relative index, computed as Length minus the value at access time
- Range type .. — a pair of indices defining a slice
- Array and string slicing via indexer overloads taking Index and Range
- No bounds validation at compile time — still throws ArgumentOutOfRangeException at runtime

**Answer**

The `^` operator creates an end-relative `Index`: `^1` means "one from the end", equivalent to `array.Length - 1` but without computing the length explicitly. At runtime, `array[^1]` is resolved to `array[array.Length - 1]`. The `..` operator creates a `Range` from two indices; `array[1..^1]` slices from index 1 to the last element exclusive. Both are syntactic sugar — the compiler lowers them to `Index` and `Range` struct operations that eventually produce integer offsets.

The critical difference from manually computing `Length - 1` is locality of reasoning: `^1` does not evaluate `Length` at the call site, so there is no intermediate variable. The range and index operators do not perform bounds validation at the syntax level — `array[^tailCount..]` still throws `ArgumentOutOfRangeException` when `tailCount > array.Length`. The compiler only validates that the types are compatible (the target type must have an appropriate indexer or `Slice` method), not that the values are in range. For types that implement `Index` and `Range` indexers — arrays, strings, `Span<T>`, `Memory<T>` — slicing creates a view over the same underlying memory without copying.

---

## Q8. What are `using` declarations vs `using` statements for IDisposable?

**Concepts**
- using statement — block-scoped disposal, resource disposed at closing brace
- using declaration (C# 8) — disposes at end of enclosing scope
- Disposal order — multiple using declarations dispose in reverse declaration order
- await using — async disposal via IAsyncDisposable

**Answer**

The classic `using` statement wraps a block: `using (var conn = new SqlConnection(...)) { ... }` and disposes `conn` when execution leaves the braces. C# 8 introduced the `using` declaration: `using var conn = new SqlConnection(...);` without braces, which disposes the resource when execution leaves the enclosing scope — typically the method body or the nearest containing block.

The difference matters in loops: inside a `foreach`, a `using var` declared in the loop body is scoped to the entire `foreach` block (the enclosing scope), not to each iteration. To dispose per iteration, I either use the classic `using (...)` statement or introduce an inner `{ }` block. When multiple `using var` declarations exist in the same scope, they dispose in reverse declaration order — the last declared disposes first. `await using var resource = ...` works the same way but calls `DisposeAsync()` asynchronously, which is the correct pattern for database connections, streams, and HTTP clients that implement `IAsyncDisposable`.

---

---

## Q9. What are nullable-aware APIs in the BCL reacting to NRT (`NotNullWhen`, `MaybeNull`)?

**Concepts**
- [NotNullWhen(true/false)] — annotates out parameter nullability conditional on return value
- [MaybeNull] — indicates a non-nullable return type may actually be null in some cases
- [NotNull] — promises a parameter will be non-null after the call (e.g. ThrowIfNull)
- [AllowNull] — allows callers to pass null even to a non-nullable parameter

**Answer**

The BCL's nullable-aware annotations are expressed through attributes in `System.Diagnostics.CodeAnalysis`. `[NotNullWhen(true)]` on an `out` parameter tells the compiler: "when the method returns `true`, this out parameter is guaranteed non-null". This is how `Dictionary.TryGetValue(key, out TValue? value)` works — after `if (dict.TryGetValue(k, out var v))`, the compiler knows `v` is non-null inside the `if` branch without requiring an explicit `!`.

`[MaybeNull]` marks a return type that is nominally non-nullable but may be null in practice — for example a generic method `T Find<T>(...)` where `T` might be a reference type and the result might be null. `[NotNull]` on a parameter means the method will throw if the value is null and ensures the argument is non-null after the call — used on `ArgumentNullException.ThrowIfNull`. `[AllowNull]` on a setter allows callers to set a property to null even if the property type is `string` rather than `string?`, which is useful for properties like `string Name { get; set; }` where setting to null should be allowed but getting should always return a string (with a default). These attributes enable libraries to provide accurate nullability contracts without changing their public surface types.

---

---

## Q10. What is `IAsyncDisposable`, and how does `await using` work?

**Concepts**
- IAsyncDisposable — interface with a single DisposeAsync returning ValueTask
- await using statement/declaration — awaits DisposeAsync instead of calling Dispose
- Use cases — database connections, network streams, async message consumers
- Implementing both IDisposable and IAsyncDisposable for backward compatibility

**Answer**

`IAsyncDisposable` exposes a single method `ValueTask DisposeAsync()`, which allows cleanup that involves I/O — flushing a network stream, committing a transaction, closing a connection gracefully — to run asynchronously rather than blocking a thread. `await using var conn = new AsyncConnection(...);` at the point of disposal calls `DisposeAsync()` and awaits the result, keeping the disposal non-blocking.

Without `IAsyncDisposable`, disposable resources with async cleanup either block in `Dispose` (unsafe in async code) or skip proper cleanup. The pattern is to implement both `IDisposable` and `IAsyncDisposable` when the type may be consumed from either sync or async contexts: `Dispose` performs a synchronous best-effort cleanup, and `DisposeAsync` performs the full async cleanup. `await using var x = ...;` in an async method works identically to `using var` in a sync method — the compiler desugars it to a `try/finally` that calls `DisposeAsync()` and awaits the result. For types like `DbConnection` in EF Core, the difference between `using` and `await using` is whether the underlying connection close handshake blocks a thread.

---

---

## Q11. What are static local functions, and why were they added?

**Concepts**
- static local function — local function that cannot capture outer variables
- Accidental capture prevention — compiler error if static function accesses outer state
- Performance — no closure allocation when no capture occurs
- Explicit parameter passing pattern for testability and clarity

**Answer**

C# 8 added the `static` modifier to local functions: `static int Compute(int x) => x * x;` declared inside a method. A static local function cannot reference any variables, parameters, or `this` from the enclosing scope — attempting to do so is a compile error. This turns what might be an accidental capture into a deliberate design choice: if I later add a reference to an outer variable inside the function, the compiler immediately rejects it.

The practical benefit is performance and intent documentation. A non-static local function that happens not to capture any outer variables is compiled without a closure allocation, but there is no compile-time guarantee — a future edit that adds a capture silently changes the allocation profile. Marking it `static` makes the "no capture" rule an enforced contract. I use static local functions for helper computations that are conceptually pure with respect to the containing method's state, and non-static local functions when I intentionally want access to the outer scope's variables.

---

---

## Q12. What is a `readonly struct`, and what mutability restrictions apply to its members?

**Concepts**
- readonly struct — all instance fields implicitly readonly, no mutation after construction
- Defensive copy elimination — compiler can pass readonly struct by ref without copy
- in parameter benefit — readonly struct + in avoids both copy and mutation risk
- Mutable state in readonly struct is a compile error

**Answer**

Declaring a struct as `readonly struct` tells the compiler that all instance fields are readonly — they can only be set in the constructor. This makes the entire struct immutable, which has two concrete benefits. First, it prevents accidental mutation bugs: any method that tries to modify a field will fail to compile. Second, the compiler can safely pass a `readonly struct` by reference in contexts like `in` parameters without making a defensive copy, since it knows the value cannot change.

Without `readonly struct`, when a struct is stored in a `readonly` field and a method is called on it, the compiler makes a defensive copy of the struct before invoking the method, because the method might mutate fields. This copy is invisible in code but has a real performance cost. Marking the struct `readonly` eliminates these copies. The `readonly` modifier on a struct member (C# 8 `readonly` methods) is a lighter option for structs that are not fully immutable: it marks individual methods as not modifying state, allowing the compiler to skip the defensive copy for only those member calls. Together these features allow efficient, safe value types in performance-sensitive code.

---

---

## Q13. What is the `readonly` modifier on struct instance members (C# 8)?

**Concepts**
- readonly method — instance method that promises not to modify struct fields
- Per-member readonly vs whole-struct readonly declaration
- Defensive copy elimination for readonly method calls on readonly contexts
- Auto-property readonly getter — implied readonly on get-only auto properties

**Answer**

Before C# 8, making individual struct methods "non-mutating" required the entire struct to be `readonly struct`. C# 8 added `readonly` as a modifier on individual instance methods, properties, and accessors: `public readonly int ComputeArea() => Width * Height;`. This promises the compiler that the method does not modify any fields, enabling the same defensive copy elimination for that specific method call even on a struct that has other mutable members.

If a `readonly` method accidentally modifies a field, the compiler reports an error. If it calls a non-readonly method on `this`, the compiler emits a warning (or error in strict mode) because that method might mutate. Auto-property getters on structs that have no setter are automatically treated as `readonly` because there is no setter to call. The granular approach is useful for structs that need some mutable members (counters, cached computed values) alongside genuinely read-only operations — marking the read operations `readonly` provides both the safety signal and the copy-elimination benefit without requiring the whole type to be immutable.

---

---

## Q14. What are stackalloc in safe contexts and `Span<T>` integrations introduced alongside C# 8?

**Concepts**
- stackalloc in safe context — Span<T> receiving stack-allocated buffer without unsafe keyword
- Span<T> as a ref struct wrapping a contiguous region of memory
- Stack vs heap trade-off — avoiding GC pressure for short-lived buffers
- Conditional stackalloc — fall back to array or ArrayPool for buffers that may exceed a safe size

**Answer**

C# 7.2 made `stackalloc` usable in safe code when the result is assigned to a `Span<T>`: `Span<byte> buffer = stackalloc byte[128];`. Before this, `stackalloc` required an `unsafe` block because the only way to hold the pointer was a raw `byte*`. `Span<T>` is a `ref struct` that wraps a pointer and length, so it can safely hold a stack-allocated region without exposing raw pointers.

This combination is used in parsing, cryptographic hash inputs, UTF-8 encoding buffers, and other short-lived byte manipulation that runs in tight loops. The allocation is free (it is a stack-pointer decrement) and the buffer is reclaimed automatically at method exit. The risk is stack overflow: stack space is typically 1MB per thread, so buffers larger than a few kilobytes should fall back to `ArrayPool<T>.Shared.Rent(size)` with a `try/finally` to return the array. The idiomatic pattern checks size: `Span<byte> buf = size <= 256 ? stackalloc byte[size] : new byte[size];`. Note that conditional `stackalloc` requires a constant or compile-time-constrained size in older runtimes; in modern .NET the size can be a runtime value.

---

---

## Q15. What is target-typed `new()` vs explicit type names?

**Concepts**
- Target-typed new() — type inferred from the assignment target's declared type
- Reduced repetition in field initializers and return statements
- Limitation — cannot use when target type is var or is not directly inferable
- Collection expressions (C# 12) as the natural evolution of target-typed new

**Answer**

C# 9 introduced target-typed `new`: when the compiler knows the target type from context, I can write `new()` without repeating the type name. `List<string> names = new();` is equivalent to `List<string> names = new List<string>();`. This is most useful in field declarations, property initializers, and return statements where the type is already stated nearby.

The inference only works when the type is unambiguous from the assignment target — `var x = new();` is a compile error because `var` defers type inference from the right side. Similarly, passing `new()` as an argument only works when the method's parameter type is known and unambiguous (no overload ambiguity). The benefit is reduced noise in classes with many typed fields: `private readonly Dictionary<string, List<int>> _index = new();` reads more cleanly than repeating the full generic type on both sides. Target-typed `new` composes well with object and collection initializers: `new() { Id = 1, Name = "A" }` works where the surrounding context supplies the type.

---

---

### Cross-chapter — Records & Pattern Matching *(C# 9–11; grouped here)*

## Q1. What are records (C# 9), and what boilerplate do they synthesize?

**Concepts**
- Synthesized value equality — Equals, GetHashCode, == based on all data members
- Synthesized ToString — formatted representation of all properties
- init-only properties — settable only during construction
- with expression support — compiler-generated copy constructor

**Answer**

Records are reference or value types optimized for immutable data carriers; the compiler synthesizes value-based equality, `GetHashCode`, `ToString`, and copy-with helpers depending on syntax. Positional record declarations also generate constructor parameters mapped to properties.

- `record class Person(string Name, int Age);` creates init-only properties and structural equality.
- Reduces manual `Equals`/`GetHashCode` for DTOs compared to classic classes.
- `with` expressions clone with selective overrides.
- Choose records when identity is defined by data, not object reference alone.

---

## Q2. What is the difference between `record class` and `record struct`?

**Concepts**
- record class — reference type, heap-allocated, reference equality by default overridden to value
- record struct — value type, stack-allocated, value equality same as struct but synthesized
- readonly record struct — fully immutable record struct

**Answer**

`record class` declares a reference type with reference semantics and default nullability like classes, while `record struct` is a value type with copied storage and different equality boxing behavior. Both support value-based equality, but lifetime and mutability defaults differ.

- `record struct` avoids heap allocation for small immutable value bundles.
- Reference records still allocate on the heap.
- Struct records can be readonly by declaration; class records use init accessors.
- Pick struct records for small composite keys; class records for larger shared DTO graphs.

---

## Q3. How does value-based equality in records differ from default class equality?

**Concepts**
- Reference equality — default for classes, compares object identity
- Value equality — records override Equals to compare all properties
- EqualityContract — virtual property enabling correct equality in inheritance chains
- GetHashCode synthesis — consistent with value equality for dictionary use

**Answer**

Default classes use reference equality unless overridden; records override equality to compare values of all included data members in order, so two distinct instances with the same data compare equal. Hash codes combine member values consistently for dictionary use.

- Reference equality (`ReferenceEquals`) may still be false when value equality is true for records.
- Derived record equality includes base and derived members when properly declared.
- Serialization round-trips benefit — reconstructed DTOs equal originals by value.
- Mutable classes without overrides compare by reference — a common DTO bug records fix.

---

## Q4. What is the difference between positional records and records with manual properties?

**Concepts**
- Positional record — primary constructor parameters become init-only properties automatically
- Manual property record — body-declared properties with optional custom logic
- Deconstruct synthesis — positional records get a Deconstruct method matching the primary constructor

**Answer**

Positional syntax `record R(int Id, string Name);` declares primary constructor parameters that become init-only properties automatically, while manual records declare properties inside the body with optional custom validation or computed members. Both can be records with value equality when configured.

- Positional form is concise for flat DTOs; manual form suits complex initialization logic.
- Manual records can mix init and calculated properties not tied to constructor parameters.
- Primary constructor parameters are not always public fields — they become properties unless customized.
- Choose positional for API models; manual when encapsulation requires private setters or factories.

---

## Q5. Explain `with` expressions — how do they relate to non-destructive mutation?

**Concepts**
- with expression — clones a record and overrides specified init properties
- Non-destructive mutation — original instance unchanged, new instance returned
- Copy constructor — compiler-generated protected method used by with
- Works on record structs by copy semantics

**Answer**

The `with` expression clones a record instance and overrides selected init properties, producing a new instance without mutating the original — non-destructive mutation. Syntax: `var updated = original with { Age = original.Age + 1 };`.

- Works on records with init accessors; classic mutable classes lack `with` unless customized.
- Under the hood the compiler synthesizes a copy constructor consuming member values.
- Ideal for functional-style updates in immutable domain models.
- Reference-type records still allocate new objects — not in-place field updates.

---

## Q6. What are init-only setters (`init`), and how do they differ from `{ get; set; }` and `{ get; }`?

**Concepts**
- init accessor — settable during object initializer and constructor, readonly after
- { get; set; } — fully mutable throughout object lifetime
- { get; } — settable only in the declaring class constructor
- init + required (C# 11) — required property that must be set during initialization

**Answer**

`init` accessors allow property assignment only during object initialization (object initializer, constructor, or `with`), preventing mutation after construction completes. `{ get; set; }` allows ongoing mutation; `{ get; }` without init allows assignment only in constructors declared in the type.

- Init properties support immutable DTOs deserialized from JSON when paired with constructors.
- After construction, `obj.Prop = x` fails for init-only properties.
- Records commonly use init for all data members.
- Distinct from `readonly` fields — init applies to properties with broader initializer syntax.

---

## Q7. Can init-only properties be set inside the type's constructors after object creation semantics?

**Concepts**
- Construction phase — init accessors accessible throughout constructor execution
- External post-construction — init properties cannot be set after object is constructed
- Deserializer access — JSON/XML deserializers use special runtime metadata to set init properties

**Answer**

Init accessors are settable during the instance construction phase — including constructors and object initializers — before the object is fully constructed and exposed. Once construction completes, init properties behave like get-only from external code.

- Constructor bodies can assign init properties on `this` during construction.
- Deserializers use parameterized constructors or `[JsonInclude]` paths to satisfy init-only models.
- Do not confuse with post-construction mutation — external callers cannot re-init.
- Primary constructors in later C# versions map parameters to init properties automatically.

---

## Q8. What is primary constructor syntax for records/classes (C# 12 preview cross-ref) vs positional records?

**Concepts**
- Primary constructor on classes/structs (C# 12) — parameter list on the class declaration
- Captured parameters — primary constructor parameters available throughout the class body
- Contrast with positional records — records auto-generate properties; classes do not
- Base constructor chaining with primary constructor syntax

**Answer**

Positional records (C# 9) tie constructor parameters directly to generated properties in one declaration, while C# 12 primary constructors generalize parameter lists on any class or struct, capturing parameters into fields or properties with explicit body usage. Both reduce boilerplate but differ in generated members and inheritance rules.

- Positional `record R(T x)` always exposes property `x` unless customized.
- Primary constructors on classes may capture into private fields without auto-properties unless declared.
- Inheritance with primary constructors requires careful base constructor chaining.
- Use positional records for simple DTOs; primary constructors when mixing custom logic in the type body.

---

## Q9. What is pattern matching in modern C# beyond C# 7 — switch expressions, relational, logical, and property patterns?

**Concepts**
- switch expression — expression form producing a value, exhaustiveness checked
- Property pattern — { PropertyName: pattern } matching nested shape
- Relational pattern — >, <, >=, <= comparing to constants in pattern position
- Logical combinators — and, or, not composing sub-patterns

**Answer**

Modern pattern matching adds switch expressions (`var y = x switch { ... }`), property patterns matching nested shape, relational patterns comparing ordered values, and logical combinators `and`, `or`, `not`. Together they replace verbose cascade if-chains with exhaustive, expression-oriented rules.

- Switch expressions require a result expression in each arm — no fall-through statements.
- Property patterns destructure: `order is { Status: OrderStatus.Shipped, Total: > 0 }`.
- Relational patterns require types with ordering — numeric and enum cases common.
- Module 01 introduced basics; this cross-chapter covers C# 9–11 depth.

---

## Q10. Explain property patterns (`person is { Age: > 18, Name: var n }`).

**Concepts**
- Property pattern — checks named properties match nested sub-patterns
- Nested property patterns — { Address: { City: "London" } } for deep inspection
- Variable binding inside property pattern — var n captures the matched value
- Null-safe — property pattern fails on null without throwing

**Answer**

Property patterns match an object by inspecting nested property values and optionally binding variables from matched members. The pattern succeeds when the runtime type exposes the named properties and each nested pattern matches.

- Combines type testing, comparison, and variable binding in one expression.
- `var n` in a property pattern captures `Name` when the outer pattern matches.
- Null checks often prefix: `person is { Age: > 18 }` fails on null without throwing.
- Useful in validation pipelines and API authorization rules expressed declaratively.

---

## Q11. What are relational patterns (`>`, `<=`) and combinator patterns (`and`, `or`, `not`)?

**Concepts**
- Relational pattern — operator + constant, e.g. > 0 in a pattern context
- and combinator — both sub-patterns must match
- or combinator — either sub-pattern must match
- not combinator — inverts a sub-pattern

**Answer**

Relational patterns compare a matched value to constants using `<`, `<=`, `>`, `>=` in pattern positions, while combinator patterns join subpatterns with `and`, `or`, and `not` for boolean structure without nested ifs. They require compatible ordered types.

- Example: `x is > 0 and < 100` replaces range checks.
- `or` matches alternatives: `c is 'a' or 'e' or 'i'`.
- `not` negates a subpattern: `x is not null`.
- Compiler warnings highlight non-exhaustive combinations when enums omit cases.

---

## Q12. What is list patterns (C# 11) — `[_, .., var last]`?

**Concepts**
- List pattern — matches sequences by element count and individual element patterns
- Slice pattern .. — matches zero or more middle elements, optionally binding them
- Works on arrays, List<T>, IEnumerable<T> with CountProperty, and Span<T>
- Empty list pattern [] — matches only an empty sequence

**Answer**

List patterns match sequences by structure — length, first/last elements, and slices — using syntax like `[head, .., tail]` on arrays, spans, and lists in pattern contexts. The discard `_` matches any element; `..` captures a slice subpattern.

- `[_, .., var last]` succeeds when at least two elements exist and binds `last`.
- Empty collection fails patterns requiring elements unless a separate `[]` arm exists.
- Enables concise parsing of command tokens and route segments.
- Combine with switch expressions for readable dispatch on string[] args.

---

## Q13. What is `switch` expression vs traditional `switch` statement for exhaustiveness?

**Concepts**
- switch expression — produces a value, each arm is pattern => expression
- SwitchExpressionException — thrown at runtime when no arm matches
- Exhaustiveness analysis — compiler warns on non-exhaustive enum switches
- switch statement — executes statements, allows fall-through with goto case

**Answer**

Switch expressions require every input to map to a resulting value with arms separated by `=>`, encouraging complete coverage of cases; traditional switch statements execute statements with fall-through controls and optional default without producing a value. Compiler exhaustiveness analysis is stronger on switch expressions over enums and tuples.

- Expression form: `var label = status switch { OrderStatus.New => "N", ... };`
- Statement form suits multi-step case bodies with local variables and loops.
- Non-exhaustive enum switch expressions warn when a case is missing.
- Prefer expressions for mapping; statements for imperative case workflows.

---

## Q14. What happens when a `switch` expression is not exhaustive over an enum?

**Concepts**
- CS8509 warning — compiler warning for non-exhaustive switch expression
- SwitchExpressionException at runtime — when no arm matches
- Enum extension risk — adding a new enum member breaks non-exhaustive switches at runtime
- _ discard arm as the explicit catch-all

**Answer**

The compiler emits a warning or error (depending on analysis level) when an enum switch expression omits a member and no discard arm catches the remainder, because a new enum value could arrive at runtime and throw `SwitchExpressionException` at execution. Adding `_ => ...` or listing all members restores exhaustiveness.

- API evolution adding enum values breaks non-exhaustive switches at runtime first.
- Treat warnings seriously in CI — they predict production exceptions on new enum members.
- Default discard arm documents intentional catch-all behavior.
- String switches cannot be exhaustively proven — enums are the primary case.

---

## Q15. What is the difference between `is null` and `== null` when a type overloads `==`?

**Concepts**
- is null — CLR reference equality, never invokes user-defined == operator
- == null — may invoke overloaded == operator, potentially non-null-checking behaviour
- NRT flow analysis — is null and is not null integrate with nullability warnings
- Practical recommendation — prefer is null for null checks on types with overloaded ==

**Answer**

`is null` always performs a reference null check without invoking user-defined `==` overloads, while `== null` may call a static overloaded equality operator that could treat non-null instances as equal to null incorrectly. For nullable reference analysis, `is null` and `is not null` also integrate cleanly with flow tracking.

- Prefer `is null` / `is not null` for reference types with custom equality operators.
- `== null` remains common for value types and strings without surprising overloads.
- Pattern matching idioms (`if (x is not null)`) combine check and assignment.
- Unit tests should cover overloaded equality types explicitly.

---

## Q16. What are expression trees (`Expression<T>`), and how do they differ from delegates?

**Concepts**
- Expression<T> — lambda stored as an inspectable data structure, not compiled IL
- Delegate — compiled callable, no structure accessible at runtime
- IQueryable — consumes Expression<Func<T,bool>> predicates for remote translation
- Compile() — converts Expression tree to a delegate for local execution

**Answer**

Expression trees represent code as data structures (`Expression` nodes) that can be inspected, transformed, and compiled at runtime, whereas delegates are compiled callable targets without preserved structure. `Expression<Func<T>>` stores the lambda body as a tree; `Func<T>` stores IL to invoke directly.

- LINQ providers translate trees to SQL or other remote query languages.
- Not every C# lambda form is translatable.
- Compile a tree once with `.Compile()` to produce a delegate for local execution.
- Trees enable dynamic predicate builders in filtering APIs.

---

## Q17. How are expression trees used by LINQ providers (EF Core, `IQueryable`)?

**Concepts**
- IQueryable<T> — deferred query building via expression tree accumulation
- ExpressionVisitor — walks and rewrites expression trees during query translation
- IQueryable vs IEnumerable boundary — server vs client evaluation
- Translation failure — unsupported method calls throw at runtime, not compile time

**Answer**

`IQueryable` providers receive expression trees from LINQ query operators and translate member access, comparisons, and calls into provider-specific text such as SQL, executing remotely instead of in memory. The same lambda syntax compiles to a tree when the source is `IQueryable<T>` and to a delegate when the source is `IEnumerable<T>`.

- EF Core walks `Expression` nodes to build SQL with parameters — not arbitrary C# execution on the server.
- Method calls in trees must map to provider-supported functions or translations fail at runtime.
- `AsEnumerable()` switches to LINQ-to-Objects delegates — client evaluation boundary shifts.
- Debugging requires logging translated SQL, not assuming C# semantics off-process.

---

## Q18. Why can't all C# lambdas be converted to expression trees?

**Concepts**
- Expression tree subset — only expression-bodied lambdas with supported node types
- Statement lambdas { } — not convertible to expression trees
- Unsupported features — assignment, loops, try/catch, ref, async, dynamic inside trees
- Compiler error CS0834 — statement lambda cannot be converted to expression tree

**Answer**

Expression trees support only a subset of C# expressions — no statements blocks with loops, local functions, `ref` operations, or many statement-bodied patterns unless compiler can represent them as supported node types. Lambdas using unsupported constructs must compile to delegates only.

- Statement-bodied lambdas with `{ ... }` often disqualify tree conversion.
- Null propagating operators and some null-forgiving forms have limited support depending on version.
- Use supported expression forms in `IQueryable` or fall back to `IEnumerable` client evaluation knowingly.

---

## Q19. What is the difference between compile-time constant patterns and runtime type patterns?

**Concepts**
- Constant pattern — matches a compile-time constant value (literal, enum, null, string)
- Type pattern — checks runtime type and optionally binds a typed variable
- Order dependency — more specific type patterns must precede base-type patterns in switch
- Interaction with boxing — constant int pattern on object input boxes and compares

**Answer**

Constant patterns match values known at compile time such as `case 0:` or `case "OK":`, while type patterns test runtime types and bind variables (`case Dog d:`). Constant patterns require compatible switch input type; type patterns interact with inheritance and casting rules at runtime.

- Enum cases use constant patterns with symbolic names.
- Type patterns may fail without throwing when used in `is` expressions.
- Mixing both in one switch is common in heterogeneous message dispatch.
- Switch input type determines which pattern forms are legal in each arm.

---

## Q20. When should you prefer records over classes for DTOs and domain models?

**Concepts**
- Records for value-centric data — equality by data, immutability by default
- Classes for identity-centric entities — ORM change tracking, mutable aggregate behavior
- API response / event payload fit for records — serialisation, caching keys, test assertions
- Avoiding over-use — behaviour-heavy types benefit from explicit class design

**Answer**

Prefer records for immutable data transfer objects, event payloads, and value-centric domain concepts where equality should reflect data, not identity. Prefer classes when you need identity lifecycle, mutable aggregate behavior, or complex inheritance with reference semantics central to the model.

- API response models and message contracts fit records well with init-only properties.
- Entities with ORM change tracking and behavior-heavy aggregates often stay classes.
- Records reduce equality boilerplate — important for serialization tests and caching keys.
- Do not convert every class blindly — behavior-rich types benefit from explicit class design.

---

### Gotchas — Module 08

## Gotcha 1. **`typeof` vs `GetType()`**

**Concepts**
- typeof — compile-time resolved Type for the declared type token
- GetType() — runtime resolved Type of the actual object instance
- Polymorphism mismatch — typeof(Base) != new Derived().GetType()

**Answer**

Developers treat `typeof(Base)` as interchangeable with `instance.GetType()` when serializing or reflecting, but `typeof` is fixed at compile time to the declared base type while `GetType()` returns the actual derived runtime type. Polymorphic scenarios then pick the wrong member set or serializer contract.

- `typeof(Animal)` never becomes `Dog` even when the variable holds a `Dog`.
- Factory and plugin code must call `GetType()` on instances for concrete behavior.
- Logging both values during bugs quickly exposes the mismatch.

---

## Gotcha 2. **Serialization type loss**

**Concepts**
- Static type serialization — System.Text.Json uses declared type by default, not runtime type
- Polymorphic serialization — JsonDerivedType and JsonPolymorphism attributes
- Derived-class property loss when serialising through a base-typed reference

**Answer**

Assigning `Animal ref = new Dog()` and serializing through the base-typed variable drops derived-only properties unless polymorphism is configured. The compile-time static type drives the default System.Text.Json contract, not the runtime object alone.

- Fix by enabling polymorphic options, serializing as the derived type, or flattening DTOs.
- Integration tests should cover every derived type in inheritance hierarchies on the wire.
- API designers often avoid deep inheritance on public JSON models because of this trap.

---

## Gotcha 3. **`[Serializable]` ignored by System.Text.Json**

**Concepts**
- [Serializable] — legacy BinaryFormatter era attribute, not recognised by STJ or XmlSerializer
- System.Text.Json contract — driven by JsonPropertyName, JsonIgnore, JsonInclude
- Era confusion — conflating BinaryFormatter attributes with modern serialisation attributes

**Answer**

Candidates assume the legacy `[Serializable]` attribute controls modern JSON or XML serializers, but System.Text.Json and typical XML serializers ignore it — it targeted binary formatter-era formatting. Modern contracts use JSON attributes or explicit options instead.

- `[Serializable]` does not make a type JSON-safe or include private fields automatically.
- BinaryFormatter honored `[Serializable]` — conflating the two eras causes wrong security assumptions.
- Use `[JsonPropertyName]` and related STJ attributes for JSON shape control.

---

## Gotcha 4. **`BinaryFormatter` is a security footgun**

**Concepts**
- BinaryFormatter RCE risk — arbitrary object graph execution during deserialization
- Obsolete and blocked in modern .NET — not suitable for any use
- Safe replacements — System.Text.Json, protobuf, MessagePack

**Answer**

Deserializing untrusted binary with `BinaryFormatter` can execute attacker-controlled object graphs, leading to remote code execution; the type is obsolete and removed or blocked on modern .NET. Teams still reach for it when porting legacy persistence without understanding the risk.

- Replace with JSON, protobuf, or other auditable formats for new persistence boundaries.
- Never accept BinaryFormatter payloads from clients or message queues.
- Migration projects should treat existing binary blobs as trusted-only internal data.

---

## Gotcha 5. **Missing JSON property on non-nullable value type**

**Concepts**
- Default value substitution — missing numeric JSON field silently becomes 0
- Nullable value type int? — distinguishes missing from explicit zero
- [JsonRequired] and required keyword — force the deserializer to fail on absence

**Answer**

When JSON omits a property mapped to a value type field, deserializers often default it to `0` or `false` without error, silently producing valid-looking but wrong business data. Reference types may become null; value types hide absence unless you add required validation.

- Use `required` members, `[JsonRequired]`, or custom validation after deserialize.
- Nullable value types (`int?`) distinguish missing from zero when configured carefully.
- Contract tests should include payloads missing optional-looking but business-critical fields.

---

## Gotcha 6. **Enum numeric wire values**

**Concepts**
- Numeric enum serialization — default for both STJ and Newtonsoft without converter
- Breaking change risk — renumbering enum members breaks existing serialized data
- JsonStringEnumConverter — emits and parses string names instead of numbers

**Answer**

Default JSON enum serialization emits numeric values, so renumbering enum members in code breaks persisted documents and clients still sending old numbers. String enums trade size for stable, readable contracts across versions.

- Apply `JsonStringEnumConverter` for long-lived public APIs.
- Database-stored JSON inherits the same breakage when enums reorder.
- Document enum wire policy in API versioning guides.

---

## Gotcha 7. **`JsonSerializerOptions` not thread-safe for mutation**

**Concepts**
- Shared JsonSerializerOptions — safe to read concurrently, unsafe to mutate after first use
- Options.MakeReadOnly() — .NET 8+ explicit freeze to prevent accidental mutation
- Configure-once pattern — build options at startup, treat as singleton

**Answer**

Sharing one `JsonSerializerOptions` instance is good for performance, but mutating its properties concurrently while other threads serialize causes race conditions and subtle corruption. Configure options once, then treat them as read-only.

- Build and cache a configured static instance at startup.
- Do not add converters mid-request on a shared singleton options object.
- Clone options with `new JsonSerializerOptions(existing)` when tests need variations.

---

## Gotcha 8. **`dynamic` hides errors until runtime**

**Concepts**
- RuntimeBinderException — runtime failure for missing member on dynamic receiver
- No IDE refactoring — rename does not update dynamic call sites
- Testing requirement — dynamic boundaries need explicit tests for every member accessed

**Answer**

Code compiles when calling misspelled or nonexistent members on `dynamic`, failing only at execution with binder exceptions and blocking IDE refactor tools from updating call sites. Teams adopt `dynamic` for JSON convenience and inherit maintenance debt.

- Restrict `dynamic` to narrow interop boundaries covered by tests.
- Prefer `JsonNode` or typed DTOs for JSON when shape is known or evolvable with schema.
- Static analysis warnings disappear on dynamic flows — compensate with runtime validation.

---

## Gotcha 9. **Extension methods do not dispatch on `dynamic`**

**Concepts**
- Extension method static resolution at compile time
- dynamic receiver bypasses extension method lookup entirely
- Cast-to-concrete or static invocation as the workaround

**Answer**

Extension methods bind statically to the compile-time type, so `dynamic` receivers never see extensions even when the runtime type would match. Calls fail at runtime unless cast or invoked as static extension methods.

- `(ConcreteType)d).Extension()` or `MyExt.Extension(d)` are the escape hatches.
- LINQ-style fluent extensions on dynamic JSON models fail silently in design-time checks.
- Wrap dynamic payloads in typed adapters at boundaries instead.

---

## Gotcha 10. **Reflection string names don't refactor**

**Concepts**
- Magic string member names in GetMethod/GetProperty — not updated by IDE rename
- nameof as compile-time-safe alternative where applicable
- Startup validation — verifying reflective lookups exist at application startup

**Answer**

Code that looks up `"CalculateTotal"` by string survives compilation when the method is renamed, failing only at runtime during tests or production. Reflection-heavy pipelines need explicit tests or source generators to stay aligned with refactors.

- Prefer `nameof` for member names when APIs accept strings tied to symbols.
- Roslyn analyzers can flag magic strings in reflection calls in some setups.
- Plugin discovery by convention documents naming rules and tests assemblies at startup.

---

## Gotcha 11. **Regex without timeout on user input**

**Concepts**
- ReDoS — nested quantifiers causing exponential backtracking on adversarial input
- matchTimeout parameter — stops the match after elapsed time with RegexMatchTimeoutException
- RegexOptions.NonBacktracking — linear time engine, no backtracking possible

**Answer**

Patterns with nested quantifiers on attacker-controlled strings can hang the process indefinitely when no match timeout is configured. Public validation endpoints are common ReDoS targets if they compile user regex or apply complex patterns to long inputs.

- Always pass `matchTimeout` to regex used on external input.
- Cap input length before matching as a second layer.
- Consider `RegexOptions.NonBacktracking` for risky patterns in .NET 7+.

---

## Gotcha 12. **Nullable reference types are annotations only**

**Concepts**
- NRT compile-time only — no CLR runtime enforcement
- Boundary guards still required — validate input from JSON, DB, user regardless of annotations
- ArgumentNullException.ThrowIfNull — explicit runtime guard pattern

**Answer**

Enabling `#nullable` warnings does not inject runtime null checks — null references still throw at dereference if data violates assumptions. Developers treat green builds as null-safe runtime guarantees without guards at API boundaries.

- Validate arguments and deserialize results explicitly.
- Combine NRT with `[NotNullWhen]` annotations on Try methods for flow analysis.
- Serialization can produce null into non-nullable annotated properties without compiler notice at runtime.

---

## Gotcha 13. **Records are still reference types (`record class`)**

**Concepts**
- record class heap allocation — reference is copied, not data
- value equality ≠ value semantics — two instances equal but not the same reference
- with expression for non-destructive update — not in-place mutation

**Answer**

`record class` instances are heap objects compared by value but not by reference identity, which surprises developers expecting struct-like copying semantics or identity semantics from classic classes. `record struct` behaves differently on assignment and boxing.

- Assigning a record class copies the reference, not the data — mutate via `with` for new instances.
- Dictionary keys use value equality — two separate instances with same data collide as equal keys.
- Choose `record struct` when small immutable value semantics are required.

---

## Gotcha 14. **Expression trees cannot contain statements arbitrarily**

**Concepts**
- Expression tree node subset — only expression-bodied constructs, no statement blocks
- EF Core translation failure — unsupported expression nodes throw at runtime
- AsEnumerable() boundary — switches from IQueryable server evaluation to LINQ-to-Objects

**Answer**

Developers paste statement-heavy lambdas into EF Core or `IQueryable` queries assuming they run as C# on the server, but unsupported constructs cannot translate to SQL and throw at runtime or force client evaluation. The limitation is structural, not configurational.

- Keep query lambdas to supported expression-tree subsets.
- Inspect logged SQL to verify translation instead of assuming C# semantics remotely.
- Move complex logic to memory with `AsEnumerable()` knowingly, accepting performance cost.

---

## Scenario-Based Answers (Karat Format)

---

## Q21. **`typeof` vs `GetType()`** — `typeof(Base)` is known at compile time; `instance.GetType()` returns the actual runtime derived type.

**Concepts**
- typeof — compile-time Type token, always the declared type
- GetType() — runtime Type of the actual object, reflects polymorphism
- Serializer and reflection mismatch when using wrong form

**Answer**

`typeof(Base)` is resolved at compile time and always returns the `Type` for `Base`, regardless of what the variable actually holds at runtime. `instance.GetType()` returns the actual runtime type — if `instance` is a `Dog` stored in an `Animal` reference, `instance.GetType()` returns `Dog` while `typeof(Animal)` returns `Animal`. The common bug is passing `typeof(SomeBase)` to a serializer or reflective factory expecting the concrete type, which results in missing properties or the wrong constructor being invoked. For plugin discovery and polymorphic serialization, I always call `GetType()` on instances rather than using `typeof` with the declared variable type.

---

## Q22. **Serialization type loss** — Assigning `Animal ref = new Dog()` and serializing as `Animal` drops derived-only properties unless polymorphism is configured.

**Concepts**
- Static contract serialization — STJ uses the declared compile-time type by default
- Polymorphic serialization — JsonDerivedType + JsonPolymorphism attributes
- Type discriminator — field in JSON payload identifying the concrete type on deserialization

**Answer**

When System.Text.Json serializes a variable typed as `Animal` that holds a `Dog`, it uses the `Animal` contract by default — only properties declared on `Animal` appear in the output. Properties unique to `Dog` are silently dropped. Fixing this requires opt-in polymorphism: annotate `Animal` with `[JsonPolymorphic]` and `[JsonDerivedType(typeof(Dog), "dog")]`, which adds a type discriminator field to the JSON and instructs the serializer to use the full derived type contract. Without this, round-tripping through a base-typed interface or API method loses derived data, producing corrupted domain objects on the read side.

---

## Q23. **`[Serializable]` ignored by System.Text.Json** — Candidates conflate legacy binary markers with modern JSON/XML serializers.

**Concepts**
- [Serializable] — BinaryFormatter-era attribute, no effect on STJ or XmlSerializer
- STJ contract — driven by public properties, JsonPropertyName, JsonIgnore, JsonInclude
- Era confusion — BinaryFormatter included private fields; STJ does not by default

**Answer**

`[Serializable]` was designed for `BinaryFormatter`, which used it to allow entire object graphs including private fields to be serialized to a binary stream. System.Text.Json completely ignores this attribute — it discovers serializable members by reading public properties, regardless of whether `[Serializable]` is present. Developers migrating from BinaryFormatter or DataContractSerializer sometimes assume the attribute carries over, then discover their `[Serializable]` classes are missing fields in the JSON output or are serialized with only public properties. The correct approach for STJ is to ensure properties are public (or use `[JsonInclude]` for non-public ones) and to use `[JsonPropertyName]`, `[JsonIgnore]`, and related attributes for shape control.

---

## Q24. **`BinaryFormatter` is a security footgun** — Deserializing untrusted payloads enables remote code execution; obsolete/removed on modern .NET.

**Concepts**
- Arbitrary object graph execution — BinaryFormatter can instantiate any [Serializable] type
- RCE via deserialization — attacker-crafted binary payload executes code during Deserialize
- Removed/blocked in .NET 8+ — NotSupportedException when used in default configuration
- Safe alternatives — System.Text.Json, protobuf, MessagePack for new persistence

**Answer**

`BinaryFormatter.Deserialize` can construct any `[Serializable]` type in the loaded assemblies as part of processing its payload. A crafted binary payload can trigger constructors, property setters, and finalizers of types like `Process`, leading to arbitrary code execution without any additional vulnerability. This is not a theoretical risk — working exploits against ASP.NET applications using `BinaryFormatter` with ViewState, session state, and remoting are well documented. The type is marked obsolete in .NET 5, throws by default in .NET 8+, and is disabled for all but explicitly configured contexts. Any remaining `BinaryFormatter` usage in a codebase should be replaced with a modern format before the code reaches a network boundary.

---

## Q25. **Missing JSON property on non-nullable value type** — Deserialization may default the value silently; missing `required`/`[JsonRequired]` validation causes subtle bugs.

**Concepts**
- Default value substitution — omitted int/bool/decimal property becomes 0/false/0 silently
- int? vs int — nullable makes absence distinguishable from explicit zero
- [JsonRequired] — throws JsonException when the property is absent in the payload
- required keyword (C# 11) + [SetsRequiredMembers] for constructor-based enforcement

**Answer**

When a JSON payload omits a field mapped to an `int` property, System.Text.Json leaves the property at its default value (`0`) without any warning. The application receives a valid-looking `Order` with quantity zero, which may pass validation and enter the processing pipeline. This is one of the most common subtle data integrity bugs when consuming external APIs or processing user uploads. The fixes are: use `int?` when absence is meaningful (zero and missing are different states), add `[JsonRequired]` on properties that must be present (causes a `JsonException` during deserialisation when missing), or in C# 11+ declare the property as `required` and use `[SetsRequiredMembers]` on any constructor the deserialiser calls.

---

## Q26. **Enum numeric wire values** — Renumbering enum members breaks persisted JSON; prefer string enums for long-lived contracts.

**Concepts**
- Default numeric enum serialization — STJ and Newtonsoft emit integer values by default
- JsonStringEnumConverter — serializes and deserializes enum names as strings
- Breaking change from renumbering — int-based enum members shift meaning when reordered
- Version-stable string names vs compact numeric wire format trade-off

**Answer**

By default, System.Text.Json serializes enum values as their underlying integer: `OrderStatus.Shipped` becomes `2` in JSON. If a developer later inserts a new member before `Shipped`, that member takes value `2` and `Shipped` becomes `3`, silently breaking every persisted document and every client that has not yet updated. String enum serialization (`JsonStringEnumConverter` in options or `[JsonConverter(typeof(JsonStringEnumConverter))]` on the property) emits and reads `"Shipped"` instead of `2`, which remains stable across reordering as long as the name does not change. The trade-off is a larger wire payload and case-sensitivity concerns. For long-lived APIs and database-persisted JSON, string enums are strongly preferred; numeric is acceptable only for compact binary scenarios where the schema is versioned and controlled.

---

## Q27. **`JsonSerializerOptions` not thread-safe for mutation** — Cache a configured instance; do not tweak shared options concurrently.

**Concepts**
- JsonSerializerOptions internal state — mutable until first use, then logically frozen
- Concurrent mutation race — adding converters on a shared instance while serializing corrupts state
- MakeReadOnly() (.NET 8) — explicit freeze preventing further modification
- Static singleton pattern — configure once at startup, reuse forever

**Answer**

`JsonSerializerOptions` is safe to read concurrently from multiple threads, but mutating it (adding converters, changing property naming policy) while another thread is serializing causes a data race with no synchronization in the class itself. In .NET 8, `options.MakeReadOnly()` explicitly freezes the instance — any subsequent mutation attempt throws `InvalidOperationException`. The correct pattern is to create and configure one options instance at application startup (or as a `static readonly` field), then reuse it for all serialization calls. If a test or specific path needs different options, create a separate instance for that scope rather than modifying the shared one. Cloning is safe: `new JsonSerializerOptions(existingOptions)` copies the configuration without sharing state.

---

## Q28. **`dynamic` hides errors until runtime** — Misspelled members compile; also blocks many refactorings and overload resolution surprises.

**Concepts**
- Deferred member resolution — compiler accepts any member access on dynamic without checking
- RuntimeBinderException — runtime failure equivalent to a compile-time member error
- Refactoring blindspot — IDE rename and Find References skip dynamic call sites
- Testing requirement — every dynamic member access needs an explicit test

**Answer**

When a variable is typed as `dynamic`, the compiler approves any member call on it without checking whether the member exists. `order.Totall` compiles fine even though `Totall` is a typo; the `RuntimeBinderException` only surfaces when that line executes. In a code path exercised rarely or only in edge cases, the bug may reach production. The IDE's rename-symbol and Find References operations also skip dynamic call sites, so renaming a property silently breaks any dynamic accessor referencing it by the old name. I limit `dynamic` to narrow, well-tested interop boundaries — COM interop, IronPython hosting — and prefer `JsonNode`, `JsonElement`, or typed models over `dynamic` JSON access in all application code.

---

## Q29. **Extension methods do not dispatch on `dynamic`** — Must cast to static type or call like static methods.

**Concepts**
- Extension method compile-time resolution — static type of receiver determines candidate set
- dynamic receiver — has no compile-time type for extension lookup, so none are found
- RuntimeBinderException at call site — extension appears to exist but fails at runtime
- Workaround — cast to static type or call as static extension

**Answer**

Extension methods are syntactic sugar that the compiler resolves at compile time based on the static type of the receiver and the available `using` directives. When the receiver's static type is `dynamic`, the compiler has no type to search for extension methods, so none are included in the lookup — the DLR at runtime does not participate in extension method resolution either. A call like `dynamicValue.Where(x => x > 0)` compiles without error (because any member on `dynamic` is accepted) but throws `RuntimeBinderException` at runtime because `Where` is not an instance member.

The fixes are: cast `((IEnumerable<int>)dynamicValue).Where(x => x > 0)` to restore static typing, or call the extension as a static method `Enumerable.Where(dynamicValue, x => x > 0)`. The deeper fix is to avoid `dynamic` in code that uses LINQ or other extension-heavy APIs.

---

## Q30. **Reflection string names don't refactor** — Renaming a property breaks reflection unless tests catch it.

**Concepts**
- String-based member lookup — GetProperty("Name") is untyped and not refactor-aware
- Compile-time invisibility — rename passes compilation, breaks at runtime only
- nameof as mitigation for cases where the string is tied to a local symbol
- Startup verification — validate all reflective lookups exist during application startup

**Answer**

`type.GetProperty("CalculateTotal")` compiles fine regardless of whether `CalculateTotal` exists — the string has no relationship to the symbol in the compiler's view. When the method is renamed to `ComputeTotal`, the string literal stays unchanged, and the reflection call returns `null` or throws at runtime only when that code path executes. In a code review or CI pipeline, this failure is invisible until a test or production run hits the affected path. `nameof(MyClass.CalculateTotal)` is refactor-safe for cases where the member and its string reference are in the same compilation unit, but it does not help when reflecting into external assemblies or late-loaded plugins. For plugin systems, I validate every expected member at startup with an explicit check and fail fast with a descriptive error rather than discovering the mismatch under load.

---

## Q31. **Regex without timeout on user input** — Crafted input can hang the process via catastrophic backtracking.

**Concepts**
- ReDoS — regex denial of service via adversarially crafted input string
- Nested quantifiers — the pattern shape that enables exponential backtracking
- matchTimeout — mandatory on any regex applied to user-controlled input
- RegexOptions.NonBacktracking — linear-time alternative for untrusted patterns

**Answer**

A pattern like `(a+)+$` applied to an input like `"aaaaaaaaaaaaaaab"` causes the NFA engine to explore an exponentially growing number of combinations before determining no match is possible. An attacker who controls the input string can trivially construct inputs that hang the matching thread for seconds, minutes, or indefinitely. Without a `matchTimeout` parameter, the default is `Regex.InfiniteMatchTimeout` — the engine will run forever. For any regex applied to user-controlled text — form fields, search queries, uploaded log content — I always pass a `TimeSpan` timeout to the constructor and catch `RegexMatchTimeoutException` at the call site, returning a 400 or validation error rather than letting the thread block.

---

## Q32. **Nullable reference types are annotations only** — `#nullable enable` does not stop null at runtime without guards.

**Concepts**
- Compile-time static analysis only — no IL change, no runtime enforcement
- Boundary gaps — external JSON, DB values, and legacy code can still deliver null
- ArgumentNullException.ThrowIfNull — explicit runtime guard at trust boundaries
- NRT as documentation + tooling aid, not a runtime safety net

**Answer**

`#nullable enable` and `<Nullable>enable</Nullable>` activate the compiler's flow analyser to warn when a reference that might be null is used without a null check. The CLR does not enforce these annotations at runtime — a method annotated as returning `string` can still return `null` if its implementation is incorrect or if it receives data from a nullable-unaware library. Data arriving from JSON deserialization, database queries, or external HTTP calls can be null regardless of the declared type. The annotations catch bugs at design time, not at runtime. I still guard at every trust boundary with `ArgumentNullException.ThrowIfNull` or explicit null checks, so that a null arriving from outside the annotated codebase produces a clear error at the point of entry rather than a mysterious `NullReferenceException` deep inside the application.

---

## Q33. **Records are still reference types (`record class`)** — Identity semantics differ from `record struct`; boxing/equality surprises follow.

**Concepts**
- record class — heap reference type with value-based equality, not stack-allocated
- Assignment copies reference, not data — mutating through one reference affects all
- record struct — value type, assignment copies data, equality is value-based
- Dictionary key collision — two record class instances with same data are equal as keys

**Answer**

`record class` is a reference type: assigning one to another copies the reference, not the object. Both variables point to the same heap object, so mutating a mutable record through one variable affects the other. The value-based equality synthesised by the compiler operates on the data content, not on object identity, which means two separately created `Person("Alice", 30)` instances are `==` and share the same `GetHashCode` — they collide as dictionary keys. This surprises developers who expect "record" to mean value type. `record struct` is a value type with copy-on-assignment semantics, closer to what those developers expect. I choose `record class` for reference-equality-aware sharing and `record struct` for small, truly value-typed data bundles where copy semantics are desirable.

---

## Q34. **Expression trees cannot contain statements arbitrarily** — Many C# constructs are not translatable for EF/LINQ providers.

**Concepts**
- Expression tree node subset — limited to expression-bodied constructs the compiler can represent as nodes
- IQueryable translation — EF Core walks expression tree to generate SQL, fails on unsupported nodes
- InvalidOperationException or client evaluation — runtime consequence of untranslatable expressions
- AsEnumerable() — switches to LINQ-to-Objects, accepting the full C# expression set at memory cost

**Answer**

An expression tree is a data structure representing code as nodes — `BinaryExpression`, `MethodCallExpression`, and so on. Not all C# constructs can be represented as these nodes: statement blocks, local variable declarations, loops, `try/catch`, `ref` operations, and many method calls have no corresponding expression node type. When a lambda with these constructs is passed to `IQueryable<T>`, EF Core's expression visitor encounters a node it cannot translate and either throws `InvalidOperationException` at runtime or falls back to client evaluation, loading all rows into memory before filtering.

The practical fix is to keep EF Core query lambdas to supported constructs: simple property access, comparisons, arithmetic, and BCL methods that EF Core explicitly maps to SQL functions. Complex logic — date arithmetic, string formatting, domain calculations — belongs in memory after the query returns, not inside the query expression. Logging SQL via `UseConsoleLogger` or `LogTo` reveals whether a filter was applied server-side, which is the only reliable way to confirm that an expression was successfully translated.

---

### 03. Regular Expressions

## Q1. `Regex.IsMatch(email, pattern)` inline in the action

(R) A bulk-import API validates thousands of customer rows per request. After deploy, CPU spikes and some requests time out. Review this validator and prioritize fixes.

**Concepts**
- Static `Regex` method re-parsing — `Regex.IsMatch(input, pattern)` constructs a new `Regex` on every call
- `static readonly Regex` field — compile once at startup with `RegexOptions.Compiled`; reuse the instance
- Missing anchors — an unanchored email pattern matches any substring, producing false positives
- Nested quantifiers — `(order\s+\d+)+` triggers catastrophic backtracking on long non-matching input
- `MatchTimeout` — limits backtracking time; `RegexMatchTimeoutException` is catchable and safe

**Answer**

Static `Regex.IsMatch` calls re-parse the pattern on every row, multiplying parse cost by the row count — which is why CPU climbs linearly with batch size. The email pattern lacks `^` and `$` anchors, so `"junk alice@x.co more"` passes as valid because the engine finds the email as a substring match inside the longer string. The notes pattern uses `(order\s+\d+)+` — a nested quantifier with no `MatchTimeout` — which triggers catastrophic backtracking on adversarial notes text and hangs a thread until the gateway times out. I would fix these in order: hoist all patterns into `static readonly Regex` fields with `RegexOptions.Compiled | RegexOptions.CultureInvariant` and a `TimeSpan` match timeout; add `^…$` anchors to the email pattern; rewrite the notes pattern without nested quantifiers or switch to `RegexOptions.NonBacktracking` on .NET 7+. Short-circuiting — failing on email or phone before scanning notes — reduces the blast radius further.

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


---

## Q2. `static readonly Regex` field with `RegexOptions.Compiled | RegexOptions.CultureInvariant`

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

**Concepts**
- ReDoS (Regular Expression Denial of Service) — catastrophic backtracking on adversarial input hangs a thread indefinitely
- `MatchTimeout` — `new Regex(pattern, options, timeout)` throws `RegexMatchTimeoutException` instead of running forever
- `RegexOptions.NonBacktracking` — linear-time DFA engine (.NET 7+) that cannot catastrophically backtrack
- `RegexOptions.Compiled` cost — emits IL on construction; wasteful for one-off user patterns that vary per request
- `Regex.Escape` — converts a literal string to a safe pattern that matches only that exact text, avoiding injection

**Answer**

Accepting a user-supplied pattern against multi-megabyte log files without any guard is a textbook ReDoS vector — a pattern like `(a+)+$` against a long non-matching string blocks the request thread indefinitely, pegging CPU and triggering gateway timeouts. `RegexOptions.Compiled` adds IL emission overhead for every distinct user string without helping one-off searches, and memory grows as unique patterns accumulate. The code also does not validate that the pattern is well-formed, so a malformed token throws an unhandled `ArgumentException` that surfaces as a 500. I would address security first: either reject user-supplied patterns entirely and expose parameterized search templates only, or enforce a pattern length cap and always pass a `TimeSpan` match timeout, catching `RegexMatchTimeoutException` and returning a 400. For .NET 7+, `RegexOptions.NonBacktracking` eliminates catastrophic backtracking at the cost of some feature restrictions. Drop `Compiled` for ad hoc patterns; cache only admin-defined patterns that are reused across many requests in a bounded dictionary.

```csharp
var safe = new Regex(
    userPattern,
    RegexOptions.IgnoreCase | RegexOptions.NonBacktracking,
    TimeSpan.FromSeconds(2));
```


---

## Q3. `[RegularExpression(@"…")]` on the DTO property

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

**Concepts**
- `Match.Success` check — must be verified before reading any group value; failure yields empty string, not null
- Named capture groups `(?<name>…)` — `Groups["area"]` is refactor-safe; positional index `[2]` breaks silently when the pattern changes
- Unanchored vs anchored patterns — unanchored extraction in free text is correct; whole-field validation requires `^…$`
- Static `Regex` via `.ToString()` — round-trips the pattern through a string, re-parses it, and loses the compiled instance
- Instance `Regex.IsMatch` — calling the cached instance directly avoids re-compilation

**Answer**

`GetAreaCode` reads `Groups[2].Value` without checking `m.Success` — when no phone number exists in the notes, the match fails and the group value is an empty string, which propagates to the CRM as a blank area code rather than an explicit "not found" signal. `HasUsPhone` converts the compiled pattern back to a string with `.ToString()` and passes it into a static `Regex.IsMatch` call, re-parsing the pattern on every invocation and discarding the compiled instance entirely. The group index `[2]` is a magic number tied to the optional country code group — any pattern change silently shifts it. I would add a `Success` guard before reading any group, use `PhonePattern.IsMatch(notes)` directly on the instance instead of the static round-trip, and replace positional groups with a named group `(?<area>…)` so refactors cannot shift the index silently. For whole-field phone validation, a `^…$` anchor prevents partial digit runs in free text from becoming false positives; for extraction inside notes, the unanchored pattern is intentional but should be documented.

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


---

## Scenario-Based Questions (Karat Format)

## Q1. (R) A Redis-backed session service deserializes cached JSON on every request. Review this code — what breaks under load or attack, and what do you fix first?

**Concepts**
- MaxDepth option — guards against deeply nested JSON causing stack overflow
- [JsonIgnore] — excludes sensitive fields from serialization/deserialization
- WriteIndented on hot paths — unnecessary whitespace increases payload size and allocation
- Shared JsonSerializerOptions singleton — configure once at startup, not per-use

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

**Answer**

**Answer**

The deserializer accepts arbitrarily deep nested JSON with no depth guard, and the DTO puts `PasswordHash` on the wire — a combination that enables denial-of-service via deeply nested payloads and leaks credential material if Redis is ever read or replayed.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | No `MaxDepth` on `JsonSerializerOptions` | Malicious or corrupted JSON with deep `Nested` chains can cause stack overflow or excessive CPU |
| Security / design | `PasswordHash` has no `[JsonIgnore]` | Secrets round-trip in cache payloads; any Redis dump or log leak exposes hashes |
| Runtime | `WriteIndented = true` on a hot read path | Larger payloads, extra allocations — unnecessary for machine-to-machine cache |
| Correctness | Treating Redis bytes as trusted without schema validation | Tampered session JSON deserializes blindly into live request state |

**Fix (priority order):**

1. Set `MaxDepth` (e.g. 32–64) on shared options
2. Mark secrets and internal fields with `[JsonIgnore]`.
3. Remove `WriteIndented` from production cache serialization; use compact JSON or `SerializeToUtf8Bytes` (**Section 8**).
4. Sign or encrypt session blobs, or store only opaque session ids server-side — never rehydrate security-sensitive graphs from untrusted storage without validation.
5. Register one shared `JsonSerializerOptions` instance (or `IOptions<JsonSerializerOptions>`) instead of ad hoc static copies that drift from ASP.NET defaults.


---

## Q2. (R) After deploying a new order endpoint, mobile clients get `400`/`500` on deserialize while Postman with the old payload works. Review the handler:

**Concepts**
- ConfigureHttpJsonOptions vs new JsonSerializerOptions() — framework options not inherited by ad hoc instances
- Sync-over-async — .Result blocking in an async context risks deadlock
- JsonNamingPolicy.CamelCase — case mismatch between PascalCase C# property and camelCase JSON
- Minimal API vs controller serialization options — different configuration paths

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

**Answer**

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

**Answer**

The handler bypasses the configured HTTP JSON options by passing a fresh `new JsonSerializerOptions()`, so camelCase naming and the enum string converter never apply — `orderId` and `"Shipped"` fail against PascalCase property names and numeric enum defaults.

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
5. Add `[JsonConverter(typeof(JsonStringEnumConverter))]` on `OrderStatus` or register the converter globally .


---

## Q3. (R) A partner integration writes invoice lines to XML nightly; the job fails on first deploy with `InvalidOperationException`. Review the model and serializer usage:

**Concepts**
- XmlSerializer parameterless constructor requirement — types without one throw at construction
- Public writable properties — XmlSerializer requires public read-write properties by default
- [XmlIgnore] — excludes properties from XML serialization
- XmlSerializer caching — creating instances per call leaks memory; cache per type

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

**Answer**

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

**Answer**

`XmlSerializer` requires a public parameterless constructor on every serialized type; `InvoiceLine` only exposes a parameterized ctor, so serializer construction or the first serialize call throws `InvalidOperationException`.

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


---

## Q4. (P) Production still reads `.bin` session files produced years ago with `BinaryFormatter`. You must migrate to System.Text.Json without taking downtime. What is your rollout strategy, and why is "just flip a switch" unsafe?

**Concepts**
- BinaryFormatter RCE risk — re-enabling it app-wide opens all code paths, not just the migration path
- Dual-read migration pattern — try new format first, fall back to old, rewrite on successful legacy read
- Offline migration worker — isolated process with minimal privileges for reading legacy blobs
- Read-repair pattern — rewrite to new format on successful read of old format

---

**Answer**

Deserializing legacy `.bin` files with `BinaryFormatter` on untrusted or stale blobs is a remote-code-execution risk (gadget chains); migration must read old format only in a controlled worker, write new JSON, and dual-read during cutover — never re-enable the compatibility switch on public-facing paths.

- **Phase 1 — freeze writes:** Stop creating new `BinaryFormatter` blobs; new sessions write JSON to a parallel key/path (`session.json` or versioned Redis key).
- **Phase 2 — offline migration worker:** Background job reads each `.bin` in an isolated process with minimal privileges, deserializes once with `BinaryFormatter` (suppressed obsolete warning only here), maps to a versioned DTO, and writes `System.Text.Json` UTF-8 output. Treat every input file as hostile — validate shape, size cap, no arbitrary types.
- **Phase 3 — dual-read in app:** `LoadSession` tries JSON first, falls back to `.bin` once, rewrites JSON on successful legacy read (read-repair), logs metric for remaining legacy count.
- **Phase 4 — retire binary path:** When legacy count hits zero, remove fallback and delete `.bin` artifacts.
- **Why not flip a switch:** `BinaryFormatter` is obsolete (SYSLIB0011) and disabled by default in .NET 8 . Enabling it app-wide restores an unsafe deserializer on any code path that touches binary. JSON migration also forces an explicit contract instead of opaque .NET-only graphs.


---

## Q5. (D) You are adding an optional `IsVerified` flag to a public REST DTO. v1 clients never send the field; v2 clients may send `true`, `false`, or omit it. You need to distinguish "not verified yet" from "explicitly false." Should the property be `bool` or `bool?`, and how does System.Text.Json treat a missing member for each?

**Concepts**
- bool default value — missing JSON field for bool becomes false, losing tri-state
- bool? / Nullable<bool> — missing field becomes null, distinguishable from false
- Backward compatibility — v1 clients not sending field must not corrupt domain state
- [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] — omits null from responses

---

**Answer**

**Answer**

Use `bool?` (`Nullable<bool>`) so three states are representable: missing → `null` ("unknown / not provided"), `false` → explicitly not verified, `true` → verified. Plain `bool` collapses missing and false into `false`, losing tri-state semantics.

- **Missing JSON property + `bool IsVerified`:** deserializes to `false` (default for value types) — indistinguishable from `"isVerified": false` .
- **Missing JSON property + `bool? IsVerified`:** deserializes to `null` — signals "client did not send the field" (v1 backward compatible).
- **Extra / forward compatibility:** unknown future properties are ignored by default; adding nullable optional fields is the recommended evolution path (**Section 6**).
- **Write behavior:** combine with `DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull` if you want v2 servers to omit `null` on output and keep payloads small (**Section 5** options table).
- **API docs:** document the three states explicitly in OpenAPI (`nullable: true`) so clients do not assume false means "failed verification."


---

## Q6. (M) An org-chart API returns departments with parent/child links wired both ways. A developer enables cycle handling and ships:

**Concepts**
- ReferenceHandler.IgnoreCycles — silently emits null when a cycle is detected
- ReferenceHandler.Preserve — emits $id/$ref metadata to represent shared/cyclic references
- Graph structure loss — IgnoreCycles drops back-references, breaking bidirectional navigation
- DTO flattening — projecting to DTO without cycles is the cleanest public API approach

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

**Answer**

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

**Answer**

Parent/child references form a cycle (`root → child → root`). `ReferenceHandler.IgnoreCycles` breaks the cycle by writing `null` on the second visit to an already-serialized object — so the child's `parent` becomes `null` even though the in-memory graph is fully wired .

- **What happened:** Serializer visited `child` through `root.Children`, then hit `child.Parent` pointing back to `root`, detected the cycle, and emitted `null` instead of duplicating `root` — correct for cycle breaking, wrong for clients expecting a bidirectional graph.
- **Client impact:** Tree builders that walk `parent` pointers get a disconnected node; only the downward `children` array is reliable.
- **Fix options (pick by contract):**
  - **DTO projection:** Return a read model without back-pointers — e.g. flat list or nested children only (no `Parent` on wire).
  - **`ReferenceHandler.Preserve`:** Emits `$id` / `$ref` metadata so full graphs round-trip; clients must understand JSON Reference semantics — heavier payload, needed for true graph round-trip or patch workflows.
  - **IgnoreCycles:** Acceptable for logging or snapshots where parent nulls are harmless — not for UI trees that traverse both directions.
- **Default without handler:** Serialization throws `JsonException` on first cycle — fails fast but does not silently corrupt data.


---

## Q7. (R) A config-sync worker reads JSON settings files from a shared folder (any authenticated internal user can drop files). Review the ingestion path:

**Concepts**
- Untrusted JSON deserialization — internal users can still craft malicious settings payloads
- MaxDepth and AllowTrailingCommas — limit parser resource consumption on malicious input
- Null result from Deserialize — must be checked before caching to avoid null reference in consumers
- Input validation after deserialization — schema and range checks before applying settings

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

**Answer**

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

**Answer**

Default `JsonSerializer.Deserialize` uses no depth limit, and `Dictionary<string, object>` deserializes JSON values into `JsonElement` boxes with unpredictable shapes — a writable shared folder makes this an untrusted-input path vulnerable to depth bombs, type confusion, and config tampering (e.g. repointing `ApiBaseUrl`).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | Untrusted JSON with default options (no `MaxDepth`) | Deeply nested documents can DoS the worker  |
| Security | Writable config drop folder | Attacker or misclick can redirect `ApiBaseUrl` to a hostile endpoint — SSRF / credential theft on next outbound call |
| Runtime | `Dictionary<string, object>` for `FeatureFlags` | Values deserialize as `JsonElement`; consumer code casting to `bool`/`string` throws or misbehaves at runtime |
| Design | No schema validation or versioning | Extra keys silently ignored; missing required fields default (`RetryCount` → 0 if omitted) — silent misconfiguration |
| Ops | Cached poisoned settings in `_cache` | Bad file propagates until manual cache flush — long blast radius |

**Fix (priority order):**

1. Treat input as untrusted: set `MaxDepth`, max file size, and reject unknown properties if the schema is fixed (`JsonSerializerOptions.UnmappedMemberHandling = Skip` is default; use strict mode when appropriate).
2. Replace `Dictionary<string, object>` with `Dictionary<string, bool>` or a strongly typed flags record; use `JsonNode` only for truly dynamic probes .
3. Validate after deserialize: absolute URI check on `ApiBaseUrl`, allowed host allowlist, sensible bounds on `RetryCount`.
4. Load from secured storage (signed blob, configuration provider, vault) — not a world-writable share; verify signature before apply.
5. Use a shared, named `JsonSerializerOptions` instance registered at startup — same policy as HTTP JSON.


---

### 02. Reflection & Attributes

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/08. Advanced C# Features/02. Reflection & Attributes/`

---

## Q1. (R) A warehouse API loads pricing plugins from a separate assembly at runtime. It works on a developer machine but `LoadPlugin` always returns null in staging. Review the loader:

**Concepts**
- Assembly.Load vs Assembly.LoadFrom — path-based vs name-based resolution
- Probing paths — application base and configured subdirectories searched by Load
- AssemblyLoadContext for isolated plugin loading
- Type.GetType qualified name format — AssemblyQualifiedName required for cross-assembly lookup

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

**Answer**

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

**Answer**

`Type.GetType(string)` resolves types in the **calling assembly** and mscorlib by default — not arbitrary referenced or dynamically loaded assemblies. A namespace-qualified name without an assembly qualifier returns null when the plugin lives in `Acme.Pricing.dll`, which looks like "works locally" only if everything is inlined in one project.

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


---

## Q2. (R) A metadata-driven audit interceptor invokes controller actions and logs failures, but operators only see `TargetInvocationException` in Splunk — never the real fault. Review the handler:

**Concepts**
- TargetInvocationException wrapping — MethodInfo.Invoke wraps all exceptions from the target
- InnerException access — real exception is in TargetInvocationException.InnerException
- ExceptionDispatchInfo.Capture — rethrows inner exception preserving original stack trace
- Logging pattern — log the innermost exception, not the wrapper

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

**Answer**

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

**Answer**

Exceptions thrown **inside** the invoked method are wrapped in `TargetInvocationException`. Logging and rethrowing the wrapper hides the real fault (`InvalidOperationException`, `ArgumentException`, etc.) from operators and any upstream handler that keys off exception type.

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


---

## Q3. (R) After a rename refactor from `CalculateLineTotal` to `CalculateOrderTotal`, order totals silently become zero in production. Review the pricing service:

**Concepts**
- String-based reflection lookup — unaffected by IDE rename, breaks silently at runtime
- nameof as mitigation — compile-time checked string from symbol
- Startup validation — verify all reflective lookups resolve at application startup
- Null return from GetMethod — unhandled null causes NullReferenceException or silent default

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

**Answer**

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

**Answer**

The method was renamed but the string literal was not updated — `GetMethod` returns null, and the code invokes without a null check, which throws at runtime (or would if the fallback `0m` masked a null invoke in a sloppier variant). Stringly-typed reflection bypasses the compiler's rename refactor.

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


---

## Q4. (R) A margin-report job reads private cost fields from `CostRecord`-like DTOs but always gets null and skips rows. Review the extractor:

**Concepts**
- BindingFlags.NonPublic | BindingFlags.Instance — required for private field access
- FieldInfo.GetValue vs PropertyInfo.GetValue — fields vs properties have different access patterns
- Boxing and unboxing — GetValue returns object, requires cast to the field's type
- Declared vs inherited members — GetField only searches the declaring type without DeclaredOnly flag

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

**Answer**

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

**Answer**

`GetField` without `BindingFlags` uses default binding, which returns **public** instance/static members only. `_costBasis` is a private instance field — lookup returns null, the method exits early, and rows are skipped.

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


---

## Q5. (P) An ASP.NET Core API discovers `[EntityTable]`-decorated export types by scanning `Assembly.GetExecutingAssembly().GetTypes()` at startup. After enabling `<PublishTrimmed>true</PublishTrimmed>` for a Native AOT experiment, several entity types vanish from the export manifest with no compile errors. Why does trimming break this pattern, and what production-safe alternatives exist?

**Concepts**
- IL trimming — removes types/members not statically reachable from the entry point
- Attribute-based discovery not statically analyzable — trimmer cannot see runtime GetTypes() scan
- [DynamicallyAccessedMembers] — preserves specific members for reflective access
- Source generators as AOT-compatible alternative to runtime type scanning

---

**Answer**

**Answer**

The trimmer removes types and members it cannot prove are used at compile time. Startup reflection that scans assemblies and reads custom attributes is invisible to static analysis — entity types with no direct references are linked out, so `GetTypes()` returns a smaller set and attribute-driven discovery silently drops models.

- **Why no compile error:** Trimming is a link-time optimization; reflection targets are not required call sites the compiler tracks.
- **Mitigations:** Annotate roots with `[DynamicallyAccessedMembers]` / `DynamicallyAccessedMemberTypes` on APIs that accept `Type`; use a trimmer descriptor file (`TrimmerRootAssembly` / `TrimmerRootDescriptor`) to preserve entity assemblies; register known export types explicitly in DI instead of full-assembly scan.
- **Long-term:** Replace scan-all-reflection with **source generators** that emit export manifests or EF-style mappings at compile time — same metadata (`[EntityTable]`, `[Exportable]`), no runtime graph walk.
- **Handle `ReflectionTypeLoadException`:** When dependencies are trimmed or missing, `GetTypes()` can throw — catch and log `LoaderExceptions`.


---

## Q6. (D) Your team ships a CSV export endpoint. One developer scans every request with `type.GetProperties()` and `GetCustomAttribute<ExportableAttribute>()` (same pattern as `ExportManifestBuilder` in this chapter). Another caches `PropertyInfo[]` and attribute metadata in a `ConcurrentDictionary<Type, ExportColumn[]>` built once at startup. Under 500 RPS with 40 exportable properties per row, which approach do you choose and why?

**Concepts**
- GetProperties per-request — type metadata lookup cost × RPS × properties = significant CPU overhead
- ConcurrentDictionary<Type, ExportColumn[]> — one-time reflection cost, O(1) lookup per request
- GetOrAdd pattern — thread-safe lazy population of the cache
- Compiled property accessor delegates — further optimization if GetValue boxing is measurable

---

**Answer**

**Answer**

Cache metadata at startup (or first use per `Type`) and only call `GetValue` per instance per request — reflection on `Type` and `MemberInfo` is orders of magnitude more expensive than reading pre-resolved columns from a cached `ExportColumn[]`.

- **Per-request scan:** 500 × 40 property walks × attribute lookups allocates and hits internal reflection caches repeatedly — CPU spikes, GC pressure, latency tail grows under load.
- **Cached manifest:** Startup (or lazy) build mirrors **ExportManifestBuilder** logic once per exportable type; hot path is `foreach (col in manifest) col.Getter(instance)` — optionally compile delegates with `CreateGetter` for value types to avoid boxing.
- **Thread safety:** `ConcurrentDictionary<Type, ExportColumn[]>` is safe for lazy initialization; manifest is immutable after build — no lock on read path.
- **Invalidation:** If types are loaded dynamically (plugins), register manifest on plugin load; static domain models rarely need refresh.
- **Trade-off:** Cached approach uses slightly more memory for delegate/metadata tables — acceptable vs per-request CPU at 500 RPS.


---

## Q7. (M) A `PremiumProduct : Product` subclass is added for a loyalty tier. The ORM layer reads `[EntityTable]` from the base `Product` type to resolve table names. Export works for `Product` but `PremiumProduct` rows fail with "table not mapped." Given this attribute definition from the tutorial:

**Concepts**
- Attribute inheritance — AttributeUsage.Inherited controls whether derived types inherit the attribute
- GetCustomAttribute inherit parameter — pass true to include attributes from base types
- GetType() vs typeof(Base) — must use runtime type to discover attributes on the actual type
- IsDefined with inherit: true for existence check on derived types

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

**Answer**

```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class EntityTableAttribute : Attribute { /* TableName */ }
```

Why does inheritance behave this way, and what are two correct fixes at the call site or attribute definition?

**Answer**

`Inherited = false` on `EntityTableAttribute` means the attribute is stored only on `Product` — it is **not** visible when you call `typeof(PremiumProduct).GetCustomAttribute<EntityTableAttribute>()`. The ORM resolves the runtime type of each instance, sees no attribute on the subclass, and reports "table not mapped."

- **Why designed this way:** Table-per-type hierarchies often need different tables per concrete class — inheriting `[EntityTable("Products")]` onto every subclass would be wrong when `PremiumProduct` maps to `PremiumProducts`.
- **Fix 1 (call site):** Walk the inheritance chain: check `type.GetCustomAttribute<EntityTableAttribute>(inherit: true)` is insufficient when `Inherited = false`; instead loop `type = type.BaseType` until you find the attribute, or map `PremiumProduct` explicitly in a type registry.
- **Fix 2 (attribute):** If all subclasses share one table, set `Inherited = true` on `EntityTableAttribute` and apply only on the base — then `GetCustomAttribute` on derived types returns the base metadata (verify this matches your schema).
- **Fix 3 (explicit):** Add `[EntityTable("PremiumProducts")]` on `PremiumProduct` — correct when the subclass has its own table regardless of inheritance flags.


---

### 03. Regular Expressions

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/08. Advanced C# Features/03. Regular Expressions/`

---

## Q1. (R) A bulk-import API validates thousands of customer rows per request. After deploy, CPU spikes and some requests time out. Review this validator and prioritize fixes.

**Concepts**
- Static Regex helpers re-parsing per call — pattern compiled on every invocation
- Missing anchors — substring match incorrectly validates partial email matches
- Nested quantifier without timeout — catastrophic backtracking risk on notes field
- static readonly Regex with MatchTimeout — correct caching and safety pattern

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

**Answer**

**Answer**

The validator re-parses regex patterns on every row via static helpers, accepts substring email matches because the pattern lacks anchors, and runs a nested-quantifier notes pattern with no `MatchTimeout` — together causing wasted CPU, false accepts, and potential ReDoS under adversarial notes text.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance | `Regex.IsMatch` / `Regex.Match` static calls inside per-row loop | Pattern re-parsed on every invocation — O(rows × parse cost) |
| Correctness | Email pattern missing `^` and `$` | `"junk alice@x.co more"` passes validation (substring match) |
| Runtime / security | `(order\s+\d+)+` nested quantifier with no timeout | Catastrophic backtracking on long notes → hung threads, CPU spikes |
| Design | Notes extraction mixed into boolean gate | Validation path does extra work even when email/phone already fail |

**Fix (priority order):**

1. Cache patterns in `static readonly Regex` fields (with `RegexOptions.Compiled | RegexOptions.CultureInvariant`) and call instance `.IsMatch` / `.Match`.
2. Anchor whole-field validation: `@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"`
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


---

## Q2. (R) A support portal lets agents paste a custom regex to search and redact matches in uploaded log files (multi-MB). Review this endpoint helper:

**Concepts**
- User-supplied regex as a ReDoS vector — attacker controls pattern and input
- Compiled per unique pattern — IL emission overhead without reuse benefit
- matchTimeout on user patterns — mandatory guard against indefinite blocking
- RegexOptions.NonBacktracking — linear time engine for untrusted patterns

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

**Answer**

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

**Answer**

Accepting arbitrary regex from users against large inputs is a classic ReDoS vector — nested quantifiers can hang a thread indefinitely, and `Compiled` on every unique user pattern adds startup cost without helping one-off searches.

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


---

## Q3. (R) A notes-processing job extracts phone fragments for a CRM sync. Review this extractor:

**Concepts**
- Match.Success guard — must be checked before reading Groups
- Named capture groups — Groups["name"] for maintainable group access
- Anchoring for whole-field vs substring extraction — different patterns for each use case
- Static helper re-parsing — Regex.IsMatch(text, pattern.ToString()) recreates pattern each call

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

**Answer**

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

**Answer**

`GetAreaCode` reads `Groups[2]` without checking `Success`, returning empty strings silently when no phone exists; `HasUsPhone` round-trips the pattern through `.ToString()` into a static call that re-parses every time and still performs partial matching anywhere in the notes string.

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
4. Prefer named groups `(?<area>…)` and read `Groups["area"].Value` for maintainability

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


---

## Q4. (P) Your registration API validates email on every POST (~2k RPS). A teammate proposes three options:

**Concepts**
- Inline static calls — pattern re-parsed per invocation, unsuitable for hot paths
- static readonly + Compiled — parse-once, JIT-compiled for hot path use
- [RegularExpression] data annotation — declarative validation but no caching or timeout built in
- [GeneratedRegex] (.NET 7+) — compile-time generated, zero runtime parse cost

1. `Regex.IsMatch(email, pattern)` inline in the action  
2. `static readonly Regex` field with `RegexOptions.Compiled | RegexOptions.CultureInvariant`  
3. `[RegularExpression(@"…")]` on the DTO property  

When would you choose each, and what companion settings (timeout, anchoring, caching) are mandatory for the Regex-based approaches in production?

---

**Answer**

1. `Regex.IsMatch(email, pattern)` inline in the action  
2. `static readonly Regex` field with `RegexOptions.Compiled | RegexOptions.CultureInvariant`  
3. `[RegularExpression(@"…")]` on the DTO property  

When would you choose each, and what companion settings (timeout, anchoring, caching) are mandatory for the Regex-based approaches in production?

**Answer**

At 2k RPS, inline static calls re-parse the pattern on every request and should be replaced with a cached compiled instance; the data-annotation attribute is fine for coarse API validation but still needs a well-anchored pattern and does not replace DNS or mailbox verification.

- **Option 1 — inline static:** Acceptable only for cold paths (admin tools, one-off scripts). On a hot registration endpoint it wastes CPU re-parsing the same automaton per call — replace with Option 2.
- **Option 2 — static readonly + Compiled:** Production default for hot regex validation. Pair with `^…$` anchors, `CultureInvariant`, and a constructor `TimeSpan` timeout (e.g. 200–500 ms) even for fixed patterns — defense in depth if the pattern is ever edited badly.
- **Option 3 — `[RegularExpression]`:** Good for declarative model validation in ASP.NET Core (`[ApiController]` runs it automatically). Same anchored pattern required; attribute does not add caching or timeout by itself — underlying implementation still constructs/runs regex per validation unless you also use a custom `ValidationAttribute` wrapping a shared instance.
- **Mandatory companions for any Regex approach:** whole-field anchors; treat regex as syntax-only (follow with uniqueness check, domain policy, or confirmation email); log `RegexMatchTimeoutException` as a potential attack signal.
- **Modern alternative (.NET 7+):** `[GeneratedRegex(@"^…$")]` partial method — compile-time generated, zero runtime parse, ideal for fixed hot patterns.


---

## Q5. (D) Product wants import rejection for disposable email domains (`mailinator.com`, `tempmail.org`, …) and a regex that only allows corporate TLDs. A developer merges the blocklist into one giant pattern:

**Concepts**
- Mega-pattern maintainability — blocklist in code vs configuration — hard to extend without code change
- Layered validation — syntax regex + domain policy service + DNS/MX check as separate concerns
- Negative lookahead complexity — easy to get wrong and produces opaque test failures
- Regex validates syntax only — cannot verify DNS, domain ownership, or mailbox existence

```csharp
bool ok = Regex.IsMatch(email,
    @"^(?!.*@(mailinator|tempmail)\.com$)[a-zA-Z0-9._%+-]+@(?:contoso|fabrikam)\.(?:com|org)$");
```

What breaks in maintainability, testability, and correctness compared to splitting validation layers? How would you structure this in a real import pipeline?

---

**Answer**

```csharp
bool ok = Regex.IsMatch(email,
    @"^(?!.*@(mailinator|tempmail)\.com$)[a-zA-Z0-9._%+-]+@(?:contoso|fabrikam)\.(?:com|org)$");
```

What breaks in maintainability, testability, and correctness compared to splitting validation layers? How would you structure this in a real import pipeline?

**Answer**

One mega-pattern couples RFC-ish syntax, a disposable-domain policy, and an allow-list of employers into an unreadable string that is painful to unit test, unsafe to extend (every blocklist change recompiles regex), and still cannot verify that the mailbox exists or that the domain is typosquatted.

- **Maintainability:** Blocklists belong in configuration (`IOptions<EmailPolicyOptions>`) or a database table — not inside a pattern literal. Adding `tempmail.org` should be a config deploy, not a regex edit requiring code review of lookaheads.
- **Testability:** Layered validators get focused tests: syntax regex returns pass/fail; domain service checks blocklist and allow-list independently; integration tests compose them. A single regex forces table-driven tests with opaque expected strings.
- **Correctness:** Regex validates string shape only — it cannot detect disposable subdomains, plus-address aliases (`user+tag@contoso.com`), homoglyphs, or DNS MX existence. Negative lookahead `(?!.*@mailinator…)` is easy to get wrong and still matches `user@mailinator.com.evil.net` depending on anchoring.
- **Recommended pipeline:** (1) trim/normalize input; (2) anchored syntax regex or `MailAddress` parse for basic shape; (3) extract domain segment via `Match` named groups or string split; (4) blocklist/allow-list lookup service; (5) optional async MX/DNS check off the hot path; (6) business rules (duplicate account, region lock) in plain C#.
- **When regex fits:** syntax gate only — the same practical email pattern from **Program.cs** Section 9a, anchored with `^$`.


---

## Q6. (M) A config-ingestion worker parses key/value lines from Windows-generated files. Keys on lines after the first never match:

**Concepts**
- RegexOptions.Multiline — makes ^ match after \n and $ match before \n
- Default anchor behavior — ^ and $ anchor only start/end of entire input without Multiline
- \r\n line endings — Windows CRLF vs Unix LF, affect line boundary detection
- Line-by-line splitting alternative — simpler and more debuggable for config parsing

```csharp
string file = "Server=prod-db\r\nPort=5432\r\nTimeout=30";
bool secondLineMatches = Regex.IsMatch(file, @"^Port=");
// secondLineMatches == false — team expects true
```

Explain why default regex behavior fails here and what minimal change fixes it without rewriting the parser as a full state machine.

---

**Answer**

```csharp
string file = "Server=prod-db\r\nPort=5432\r\nTimeout=30";
bool secondLineMatches = Regex.IsMatch(file, @"^Port=");
// secondLineMatches == false — team expects true
```

Explain why default regex behavior fails here and what minimal change fixes it without rewriting the parser as a full state machine.

**Answer**

By default, `^` and `$` anchor only the start and end of the entire input string, not each line — so `^Port=` looks for `Port=` at position 0 of the whole blob (`"Server=…"`), never at the start of the second line after `\r\n`.

- **Mechanism:** Without `RegexOptions.Multiline`, `\r\n` is ordinary whitespace between characters; line boundaries are invisible to `^`/`$`. With `Multiline`, `^` matches after `\n` (and `\r\n` pairs) and `$` matches before `\n`.
- **Minimal fix:** pass `RegexOptions.Multiline`: `Regex.IsMatch(file, @"^Port=", RegexOptions.Multiline)` → `true`.
- **Alternative:** split lines first with `Regex.Split(file, @"\r?\n")` or `ReadLines` and test each line — clearer when you also need comment stripping or `#` handling .
- **Related gotcha:** `RegexOptions.Singleline` makes `.` span newlines — opposite concern when extracting multiline values; do not confuse Multiline (line anchors) with Singleline (dot behavior) — **Program.cs** Section 10.
- **Production note:** For large config files, line-by-line streaming avoids loading the entire file into one string match.


---

## Q7. (R) An internal tool "sanitizes" HTML fragments before storing them in a knowledge base:

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

**Concepts**
- Greedy quantifiers — `<.*>` spans from first `<` to last `>`, consuming the entire string in one match
- Greedy vs lazy quantifiers — `<.*?>` limits each tag match to the nearest `>`, reducing over-matching
- Match-order dependency — broad tag-strip pass consuming script block before the script-specific pass runs
- Regex parsing limitations — HTML is a context-sensitive grammar that regex cannot correctly model
- Attribute-based XSS — `<img onerror=alert(1)>` bypasses tag-name-based regex removal entirely
- Allow-list sanitization — safe approach permits only known-safe tags/attributes rather than blacklisting

**Answer**

The greedy `<.*>` spans from the first `<` to the last `>` in the entire input, so `"<div>Title</div><script>alert(1)</script>"` is consumed as one match and everything is stripped in the first pass — the script-specific replace never runs because `<script>` was already consumed. Reversing the call order does not help either, because the broad greedy pass would still consume the script tags in whichever position it runs. The deeper problem is that regex is not an HTML parser — malformed tags, attribute injection (`<img onerror=alert(1)>`), and nested structures all bypass pattern-based removal, giving false confidence while stored XSS still reaches the knowledge base. I would replace this with a vetted allow-list sanitizer library such as HtmlSanitizer (NuGet) or an AngleSharp-based pipeline that permits only known-safe tags and attributes. If the requirement is only display-only tag collapse with no security implications, a lazy quantifier `<.*?>` reduces over-matching per tag, though it still cannot handle nested markup correctly.

```csharp
// Display-only collapse — NOT security:
string collapsed = Regex.Replace(html, @"<.*?>", string.Empty);
```


---

### 04. Var Dynamic & Special Keywords

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/08. Advanced C# Features/04. Var Dynamic & Special Keywords/`

---

## Q1. (R) A warehouse integration service parses third-party CSV rows into `dynamic` bags before posting to inventory. It passes QA with two sample files but throws in production on the first malformed row. Review the mapper:

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

**Concepts**
- `dynamic` keyword — late-bound member resolution via DLR, skipping compile-time type checks
- RuntimeBinderException — thrown at runtime when a dynamically dispatched member does not exist
- `var` inference from `dynamic` — locals inferred as `dynamic` when the RHS is dynamic, propagating late-binding
- Typo propagation — misspelled member names on `dynamic` compile silently and fail on first execution
- Strong DTO boundary — mapping dynamic input to a typed record at the entry point restores compile-time safety

**Answer**

The typo `Qantity` compiles because `dynamic` skips member checking, then throws `RuntimeBinderException` at runtime on the first row that lacks that misspelled member — QA samples may never hit the path. `var` locals inherit `dynamic` when the initializer is dynamic, so the entire expression chain stays late-bound with no compile-time safety.

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


---

## Q2. (R) A pricing dashboard uses `var` with LINQ and mutates the source collection between query definition and enumeration. Review:

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

**Concepts**
- Deferred LINQ execution — `Where`/`OrderBy` build a query object; the predicate runs at enumeration time
- Mutation-during-deferral — source modifications between query definition and `foreach` alter the results
- `var` masking `IEnumerable` — hides the deferred type from readers who expect an eager list
- `.ToList()` materialization — forces immediate evaluation, capturing a snapshot before any mutation
- Query definition vs enumeration ordering — method call sequence determines which data state is observed

**Answer**

`lowStock` is deferred `IEnumerable<InventoryItem>` — the `Where` predicate runs at enumeration time, after restock mutates quantities. SKUs that were low when the query was *defined* may no longer qualify (or vice versa), so alerts no longer match the "snapshot" ops thought they captured.

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


---

## Q3. (R) A generic repository uses `nameof` and `default` for reflection-based updates. After a refactor, updates silently stop working for value-type columns. Review:

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

**Concepts**
- `nameof(T)` in generic methods — yields the type parameter identifier `"T"`, not the closed type name
- `nameof(List<T>)` behavior — strips type arguments, yielding only `"List"`
- `typeof(T).Name` — returns the closed type's actual name from the runtime type object
- `default` on value types — zeroes all fields; cannot distinguish "unset" from a valid zero-value
- `Nullable<T>` or Optional pattern — explicit representation of missing vs intentionally zero-valued struct

**Answer**

`nameof(T)` inside a generic method returns the type parameter name (`"T"`), not the closed type (`"InventoryDelta"`). `nameof(List<InventoryDelta>)` yields `"List"`, not a useful storage key. `default` on a struct zeroes all fields — `Sku` is `null`, `Amount` is `0` — so `_audit` records garbage without throwing.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `nameof(List<InventoryDelta>)` → `"List"` | Wrong dictionary key; collisions and missing audit entries |
| Correctness | `nameof(T)` in generic method → `"T"` | Exception messages and metadata keys useless in logs |
| Data | `default` struct used as "unset" business object | Silent null SKU and zero amount posted to audit |
| Design | `where T : struct` + `default` conflates "missing" with valid zero delta | Cannot distinguish unset from intentional `Amount = 0` |

**Fix (priority order):**

1. Use `typeof(T).Name` or `nameof(InventoryDelta)` at call sites for keys — never `nameof(T)` when you need the closed type name` → `"List"` pitfall).
2. Replace `CreateUnset()` with explicit factory or `Nullable<T>` / `Optional<InventoryDelta>` if "unset" is a business state.
3. Validate before audit: `if (string.IsNullOrEmpty(delta.Sku)) throw ...` — do not propagate zeroed structs.
4. For reflection keys, use `nameof(InventoryDelta.Sku)` with `GetProperty`


---

## Q4. (P) Your team ingests nightly plugin config from a legacy host that exposes JSON whose shape changes per warehouse (extra keys, missing booleans, numeric strings). A junior dev proposes `dynamic` + `ExpandoObject` for the entire pipeline; another proposes strongly typed records + `System.Text.Json` with `[JsonExtensionData]`. When is `dynamic` justified here, and what production risks push you toward typed or semi-typed models?

**Concepts**
- DLR late binding — `dynamic` defers member resolution to runtime, removing compile-time safety
- RuntimeBinderException — dynamic member access on a missing or wrong-typed member fails at runtime
- `[JsonExtensionData]` attribute — captures unknown JSON properties into a dictionary, preserving forward compatibility
- Strongly typed DTO with extension data — core fields validated at deserialize time; extra keys stored safely
- `JsonDocument` and `JsonElement` — explicit zero-DLR traversal of unknown JSON shapes with clear error sites

**Answer**

Use `dynamic` only at the thin interop boundary where you truly cannot describe the contract (COM, embedded scripting, some legacy APIs). For JSON plugin config with known core fields and variable extensions, prefer typed records with `[JsonExtensionData] Dictionary<string, JsonElement>` or a dedicated options class — you keep compile checks on `WarehouseId`, `MaxSkus`, etc., while absorbing extra keys.

- **`dynamic` risks:** `RuntimeBinderException` in production on typos; no refactor support; harder unit tests; DLR overhead on hot paths; `ExpandoObject` members are not normal CLR properties — reflection returns null .
- **Typed + extension data:** Core settings validated at deserialize time; unknown keys preserved for forward compatibility; schema changes caught in CI with golden JSON fixtures.
- **When `dynamic` wins:** One-off script host, JScript/COM object, or third-party DLL that only exposes late-bound objects — wrap in an adapter and map to typed models immediately inside the adapter.
- **Middle ground:** Deserialize to `JsonDocument`, query required nodes explicitly, validate types before mapping — no DLR, explicit errors.


---

## Q5. (M) A background price-refresh worker should stop within seconds when ops clicks "Cancel" in the admin UI. The flag works in dev (single core, low load) but the worker occasionally runs for minutes in production. Review:

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

**Concepts**
- `volatile` keyword — guarantees reads and writes are not cached in CPU registers across threads
- JIT register caching — without `volatile`, the compiler may hoist a field read out of a tight loop
- CPU memory model — multi-core reordering can make a write on one thread invisible to another
- `Interlocked` operations — atomic read-modify-write for counters; `volatile` alone is not sufficient for increment
- `CancellationToken` — idiomatic .NET cooperative cancellation that propagates through async call chains

**Answer**

`_stopRequested` must be `volatile` (or guarded by `Interlocked`/lock) so the worker thread observes the UI thread's write promptly. Without `volatile`, the JIT may cache the field in a register and the loop may never exit — intermittent and load-dependent, which is why dev often passes.

- **Simple boolean stop flag:** `private volatile bool _stopRequested;`
- **Counter / statistics:** use `Interlocked.Increment` / `Interlocked.Read` — `volatile` alone does not make read-modify-write atomic.
- **Why local dev hides it:** single-core, short runs, or debugger flushes memory; production multi-core reordering exposes the bug.
- **Alternative:** `CancellationToken` from `CancellationTokenSource` — idiomatic for .NET worker services and ASP.NET hosted services; propagates through async calls better than a raw flag.


---

## Q6. (D) Two teams share a `WarehouseAnalytics` namespace. Team A added a helper type named `Math` for domain-specific rounding; Team B assumed BCL `System.Math` in unqualified calls. Review the pricing snippet:

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

**Concepts**
- Type name shadowing — a local type in the same namespace takes precedence over BCL name lookup
- CS0117 compile error — calling a method that does not exist on the shadowing type
- `global::` qualifier — forces resolution from the global namespace root, bypassing any local shadows
- `using` alias — `using BclMath = System.Math;` creates an unambiguous shorthand in one file
- Naming policy — avoiding BCL-colliding names (`Math`, `Task`, `Thread`) in domain namespaces prevents shadowing bugs

**Answer**

Unqualified `Math.Round` resolves to `WarehouseAnalytics.Pricing.Math` in that namespace — which has no `Round(decimal, int)` overload, so you get **CS0117** at compile time (not a silent wrong answer). The shadowing is still a maintenance trap: every new developer repeats the failure until they learn the local type exists.

- **Immediate fix:** `global::System.Math.Round(gross, 2)` or `System.Math.Round` with a file-level alias `using BclMath = System.Math;`.
- **Policy:** Ban type names that shadow BCL types (`Math`, `Thread`, `Task`, `Environment`) in shared namespaces — rename to `PricingMath`, `MoneyRounding`, etc. **Program.cs** Section 9 explicitly warns never ship shadow names in production.
- **Linting:** Enable analyzer rules or code review checklist for BCL name collisions; namespace-per-feature reduces accidental local `Math` helpers in global pricing code.
- **Design:** Domain rounding belongs on a clearly named static class (`CurrencyRounding.ToNickel`) so call sites document intent and never compete with `System.Math` lookup.


---

### 05. C# 7 Features

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/08. Advanced C# Features/05. C# 7 Features/`

---

## Q1. (R) Express VIP orders are routed to the standard express lane in production. Review this C# 7 switch with `when` guards (mirrors the warehouse routing demo):

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

**Concepts**
- C# 7 switch first-match-wins — cases are evaluated top to bottom; the first matching case wins
- `when` guard clauses — additional condition on a case; unreachable if a prior case matches without a guard
- Specific-before-general ordering — guarded cases must precede their unguarded counterparts
- Unreachable code — unconditional `case OrderPriority.Express` makes the guarded VIP case dead code
- Test matrix coverage — tests that only assert "express → some lane" pass even when VIP branch is dead

**Answer**

The `Express` case without a `when` guard matches every express order first, so the `Express when order.IsHighValue` branch is unreachable dead code — high-value express orders never reach VIP routing.

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


---

## Q2. (R) A product lookup was optimized with `ValueTask<string>` for cache hits. Under retry logic, intermittent `InvalidOperationException` appears. Review:

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

**Concepts**
- `ValueTask<T>` single-consumption contract — a `ValueTask` instance must be awaited at most once
- `ValueTask` caching danger — storing and reusing a `ValueTask` like a `Task` violates the API contract
- Synchronous completion path — cache-hit returns immediately without allocating a `Task`; that is the value proposition
- Double-await behavior — re-awaiting a synchronously completed `ValueTask` causes `InvalidOperationException`
- `Task<T>` for public boundaries — `Task` is safely cacheable and re-awaitable; prefer it on public APIs

**Answer**

A consumed `ValueTask` must not be awaited twice unless it wraps a `Task` or `IValueTaskSource` — the retry path re-awaits the same instance after the cache-hit path already completed it synchronously, causing undefined behavior or `InvalidOperationException`.

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


---

## Q3. (R) A developer refactors inventory reservation to use C# 7 ref returns for in-place updates. The build fails; after a workaround it crashes in QA. Review:

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

**Concepts**
- `ref` locals in async methods — compiler rejects `ref` locals across `await` suspension points (CS4012)
- Async state machine constraint — state machines move locals to the heap; `ref` locals cannot survive this
- `ref` return on `List<T>` indexer — the ref may not remain valid after list reallocation
- Parse-then-await split — extract values from `ref` locals before any `await`, then assign by index
- Transactional reservation — in-memory ref mutation is not atomic with I/O; use DB row locks for correctness

**Answer**

`ref` locals and `ref` returns cannot appear in `async` methods (state machine restriction), and `ref` returns must target stable storage — a `List<T>` indexer returns a temporary ref in many contexts, not a durable slot alias safe across growth/reallocation.

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


---

## Q4. (R) A CSV import pipeline uses C# 7 out variables. Finance sees rows with quantity `0` marked as successfully imported. Review:

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

**Concepts**
- `out var` inline declaration — C# 7 lets the `out` variable be declared at the call site for brevity
- Unchecked `TryParse` return — discarding the bool result silently accepts default(int) on failure
- Default-on-failure behavior — `int.TryParse` sets the `out` variable to `0` when parsing fails
- Silent success with bad data — `return true` after partial parsing passes invalid rows downstream
- Validation duty at parse sites — C# 7 syntax does not remove the need to check every `Try*` result

**Answer**

`int.TryParse` failure on quantity is ignored — `out var qty` defaults to `0` and import still returns `true` because only `orderId` failure short-circuits; invalid quantity silently becomes zero.

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


---

## Q5. (P) A warehouse fulfillment microservice returns `(bool CanFulfill, string Note)` tuples from `CheckFulfillment` — the same shape as the tutorial's tuple demo. The team debates replacing tuples with a `FulfillmentResult` record before exposing the method on a public NuGet contract. When is the tuple idiomatic, and when does it break production maintainability?

**Concepts**
- Named tuple element names — compile-time convenience; not part of the runtime `ValueTuple` contract
- Tuple element names across assemblies — names are not preserved in the binary contract; consumers may not see them
- Record/class for public API — named types support versioning, serialization, and documentation across assembly boundaries
- Tuple equality (C# 7.3) — structural equality on element values; useful for tests, not for domain identity
- API evolution with tuples — adding a third element is a binary-breaking change with no named fallback

**Answer**

Named tuples are fine for private or internal helpers with stable, obvious element semantics; public NuGet contracts need a named type so additions, serialization, and versioning do not break consumers silently.

- **Keep tuples** for internal methods with two or three tightly coupled values and no evolution expected — e.g. private `CheckFulfillment` inside one service class, same assembly as **Program.cs** Section 6 demo.
- **Use a record/class** when the shape crosses assembly boundaries, gains fields (`ReservedQuantity`, `BackorderSku`), needs JSON/XML mapping, or appears in logs/metrics dashboards — `(bool, string)` element names are not part of the runtime contract.
- **Inferred tuple names (C# 7.1)** help readability at the return site but do not replace API documentation for external callers.
- **Tuple equality (C# 7.3)** is useful for tests comparing snapshots; not a substitute for domain identity on persisted entities.


---

## Q6. (M) A batch job uses a local function with captured outer state to retry flaky lane assignments. Ops reports duplicate reservations on the same SKU after parallel batch splits. Review:

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

**Concepts**
- Local function closures — local functions capture outer variables by reference, sharing the same storage
- `ref` locals — alias to an array element; multiple threads aliasing the same slot race on read-modify-write
- `Parallel.ForEach` concurrency — each iteration runs on a thread-pool thread with no implicit synchronization
- Non-atomic increment — `reservationFailures++` is a read-modify-write; races with concurrent increments drop counts
- `Interlocked.Increment` — atomic increment for shared counters without a lock
- Static local functions (C# 8+) — disallow captures, making shared-state bugs visible at compile time

**Answer**

The local function captures `reservationFailures` and mutates `inventory` through `ref` aliases while `Parallel.ForEach` runs handlers concurrently — non-atomic read-modify-write on shared array slots and non-interlocked increments corrupt counts under race.

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


---

## Q7. (D) Two teammates implement guard clauses for order validation. Which approach do you standardize on for a shared domain library, and why?

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

**Concepts**
- Throw expressions (C# 7) — `throw` as an expression, usable in `??`, ternary, and expression-bodied members
- Expression-bodied members — single-expression method bodies; throw expressions fit here naturally
- `ArgumentOutOfRangeException` overloads — the three-argument form carries param name, actual value, and message
- Debuggability of expression chains — throw expressions in chained ternaries are harder to step through
- Guard clause consistency — mixing throw expressions and block guards in a NuGet library surprises callers

**Answer**

Standardize on **Option B (classic blocks)** for shared domain validation libraries, and allow **Option A (throw expressions)** only for thin one-liners where exception detail stays minimal — not as the default for public APIs that operators debug from logs.

- **Throw expressions** pair well with expression-bodied members (**Program.cs** Sections 10–11) for null-guard properties and private helpers — `order ?? throw new ArgumentNullException(nameof(order))` is clear and concise.
- **Classic blocks** win when you need overloads with `(paramName, actualValue, message)` on `ArgumentOutOfRangeException`, multiple guards, or XML doc that describes thrown types — Option B's qty check carries the offending value; Option A's ternary does not.
- **Refactor safety:** Expression-bodied throw chains are harder to breakpoint and step through in production debugging than block bodies.
- **Consistency:** Mixed styles across a NuGet domain library frustrate code review — pick block guards for public methods, throw expressions for small internal `Require*` helpers like **Program.cs** `RequireOrder` / `RequirePositiveQuantity`.


---

### 06. C# 8 Features

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/08. Advanced C# Features/06. C# 8 Features/`

---

## Q1. (R) After enabling `<Nullable>enable</Nullable>` on the document-ingest API, QA reports intermittent `NullReferenceException` on documents with no footnotes. Review the service:

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

**Concepts**
- Null-forgiving operator `!` — suppresses the compiler warning without proving non-null at runtime
- `string?` — nullable reference type annotation; calling instance methods on it without a guard throws at runtime
- `JsonSerializer.Deserialize<T>` return type — returns `T?`; the result can be null for `"null"` JSON or malformed input
- NRT validation at boundaries — `!` is a last resort for proven-safe values, not a fix for uncertain input
- Null-conditional operator `?.` — short-circuits to null instead of throwing when the receiver is null

**Answer**

Null-forgiving operators silence the compiler without proving non-null at runtime — `Notes` is still null for many documents, and `JsonSerializer.Deserialize` can return null entirely. Production NRT requires honest annotations plus guards at boundaries, not blanket `!`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| NRT misuse | `metadata.Notes.ToUpperInvariant()` on `string?` | `NullReferenceException` when `Notes` is null — matches QA report |
| NRT misuse | `metadata.Id!` without validation | Masks missing IDs; empty or null IDs slip into downstream queues |
| Correctness | `Deserialize` result used without null check | `NullReferenceException` on malformed or `"null"` JSON |
| Design | Suppressions instead of contract | Warnings return on next edit; team learns to ignore NRT |

**Fix (priority order):**

1. Handle optional notes explicitly: `var note = metadata.Notes?.ToUpperInvariant() ?? "(no notes)";`
2. Validate `metadata` and `metadata.Id` at the API boundary; throw `ArgumentException` or return `ProblemDetails` for bad input — do not use `!` on unvalidated data.
3. Guard deserialization: `var doc = JsonSerializer.Deserialize<DocumentMetadata>(json) ?? throw new JsonException("…");` or return `DocumentMetadata?` and let callers decide.
4. Configure `JsonSerializerOptions` with required-property validation (modern) or a dedicated DTO layer for ingest.
5. Treat new CS86xx warnings as build breaks in CI for touched projects — block `#nullable disable` without ticket.


---

## Q2. (R) A background worker streams archive pages to blob storage. Under deploy cancellation, the job keeps running for minutes and sometimes OOMs. Review the consumer and producer:

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

**Concepts**
- `IAsyncEnumerable<T>` — async iterator that yields elements one at a time without buffering the whole sequence
- `await foreach` — correct consumer for `IAsyncEnumerable<T>`; evaluates and processes each element lazily
- Sync-over-async antipattern — `.GetAwaiter().GetResult()` blocks a thread-pool thread, risking deadlocks
- `[EnumeratorCancellation]` — marks the parameter that receives the token passed to `WithCancellation()`
- `ToListAsync` materialize — buffers the entire async sequence before the first element is processed; defeats streaming

**Answer**

The consumer buffers the entire async stream into memory via sync-over-async `.GetResult()`, defeating `IAsyncEnumerable` streaming and ignoring cooperative cancellation during enumeration. Large archives OOM; deploy stops cannot abort the materialization phase promptly.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Async | `.GetResult()` on `ToListAsync` | Sync-over-async; thread-pool blocking; potential deadlocks in hosted contexts |
| Scalability | Materialize-all before upload | Memory proportional to total pages — OOM on large documents |
| Cancellation | `ReadPagesAsync()` called without token | `[EnumeratorCancellation]` never receives `stoppingToken`; delay loop ignores host shutdown |
| Design | Stream treated like `List<T>` | Loses backpressure; cannot start uploading until full read completes |

**Fix (priority order):**

1. Consume with streaming: `await foreach (var page in _documentStream.ReadPagesAsync(stoppingToken).WithCancellation(stoppingToken))` and upload inside the loop
2. Remove `.ToListAsync().GetResult()` entirely; keep the method `async` end-to-end.
3. Pass `stoppingToken` into `ReadPagesAsync(stoppingToken)` so cancellation propagates into `Task.Delay` and the async iterator tears down.
4. Optionally bound concurrency with a `SemaphoreSlim` if uploads overlap, but never buffer the whole sequence unless size is proven bounded.
5. Use `await using` on `AsyncDocumentStream` when the producer holds connections — **Program.cs** `IAsyncDisposable` demo.


---

## Q3. (R) A routing microservice parses document IDs with C# 8 ranges after a format change. Production throws `ArgumentOutOfRangeException` on valid-looking IDs. Review:

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

**Concepts**
- C# 8 range syntax — `a..b` creates a `Range`; throws `ArgumentOutOfRangeException` when the string is too short
- `^n` index-from-end — equivalent to `length - n`; throws when `n` exceeds the collection length
- `StartsWith` as insufficient validation — confirms prefix but does not enforce minimum length
- Magic numeric indices — hardcoded `4`, `8`, `^3` scatter the format contract across the codebase
- Defensive parsing — validate length before indexing; return `bool` from a `Try*` method rather than throwing

**Answer**

Range and index syntax does not validate length — it throws when the span is too short or when `^tailCount` exceeds the array. `StartsWith("DOC-")` is necessary but not sufficient for a 12-character ID, so truncated or legacy IDs pass validation then fail inside slice helpers.

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


---

## Q4. (R) A teammate refactors chunk parsing to overlap I/O with processing. The build fails and code review finds async/ref-struct mixing. Review:

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

**Concepts**
- `ref struct` stack-only constraint — instances must live on the stack and cannot be boxed or stored on the heap
- Async state machine heap promotion — `await` causes locals to be captured in a heap-allocated state object
- CS4012 / CS9202 compile error — `ref struct` local used across an `await` suspension point is rejected
- Parse-then-await pattern — extract needed scalars before the first `await`, then discard the `ref struct`
- `ReadOnlyMemory<char>` for async contexts — heap-safe alternative to `ReadOnlySpan<char>` across `await` calls

**Answer**

`ref struct` instances cannot survive an `await` because the async state machine may move execution to the heap — the compiler rejects storing `DocumentChunkReader` across suspension points. Disposable ref structs are stack-only and must be disposed before the first `await` in the method.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `ref struct` local used after `await` | CS4012 / CS9202 — build failure |
| Correctness | `ReadOnlySpan<char>` tied to `headerLine` lifetime | If overlap with mutation or stack pop, span could be invalid — less common with `string` but real with stack buffers |
| Design | Mixing stack-only parsing with async I/O in one scope | Forces either copy-to-string or split phases |

**Fix (priority order):**

1. **Parse-then-await split:** dispose reader and extract needed scalars (`int length = chunkReader.VisibleLength;`) **before** any `await`
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


---

## Q5. (P) Your team ships `IDocumentProcessor` as a shared NuGet package. Version 1 has `Process` and `ProcessorName`. Version 2 needs a `Describe()` helper without forcing every consumer to recompile. A consumer already implements both `IDocumentProcessor` and `IArchiveReporter`, each adding a default `Describe()`. How do you evolve the interface using C# 8 default interface methods, and what breaks if you ignore diamond ambiguity?

**Concepts**
- Default interface methods (C# 8) — interface members with a body; existing implementers inherit them without changes
- Binary compatibility — adding a DIM does not require consumers to recompile against the new version
- Diamond ambiguity — two interfaces supply the same default signature; implementing class cannot resolve implicitly
- Explicit interface implementation — `string IDocumentProcessor.Describe()` resolves ambiguity per interface
- DIM runtime dispatch — default methods are resolved against the static interface type, not the concrete type

**Answer**

Add `Describe()` as a **default interface method** on `IDocumentProcessor` so existing implementers inherit behavior at runtime without source changes, while new implementers may override selectively — but when two interfaces supply the same default signature, the implementing class must resolve ambiguity explicitly.

- **Safe evolution:** `string Describe() => $"{ProcessorName} processor";` on the interface
- **Binary compatibility:** Consumers compiled against v1 load v2 because DIMs are resolved at runtime via interface dispatch — no mandatory recompile for default-only additions.
- **Override path:** Document that implementers *may* replace `Describe()` for custom telemetry; do not require it.
- **Diamond ambiguity:** If `IArchiveReporter` also adds `string Describe() => "archive";`, `class Worker : IDocumentProcessor, IArchiveReporter` fails with CS0108/CS0539-style ambiguity when calling `Describe()` on the class without explicit qualification.
- **Resolution:** Explicit interface implementation — `string IDocumentProcessor.Describe() => …;` and `string IArchiveReporter.Describe() => …;` — or rename one method before shipping.
- **Testing:** Run contract tests against **interface-typed** references, not only concrete types — default vs override behavior differs by static type.


---

## Q6. (M) A nightly batch opens thousands of small files under a shared directory. After migrating to `using var`, ops reports "too many open files" and memory climbs until the job finishes. Review the loop:

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

**Concepts**
- `using var` scope — disposes at the end of the **enclosing block scope**, not at the end of each loop iteration
- Block-scoped `using` statement — `using (var x = ...) { }` disposes when the explicit braces close
- File handle accumulation — thousands of open `FileStream` instances exhaust OS file descriptors
- Dispose order for multiple `using var` — last declared disposes first within the same scope
- Per-iteration disposal — wrapping the loop body in an inner `{ }` block restores expected per-iteration cleanup

**Answer**

`using var` disposes at the end of the **enclosing scope** — here the entire `foreach` block — not at the end of each iteration. Every opened `FileStream` stays alive until the loop completes, exhausting file descriptors on large folders.

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


---

## Q7. (D) A 400-project solution enables nullable reference types repo-wide. CI surfaces 8,000 warnings; developers blanket `#nullable disable` on touched files and sprinkle `!` to merge PRs. As tech lead, what migration strategy do you recommend for a document-archive domain with heavy `string` metadata (IDs, paths, optional notes), and where do `string?`, null checks, and `[NotNullWhen]` belong versus suppressions?

**Concepts**
- NRT opt-in per file — `#nullable enable` / `#nullable disable` allows incremental migration without a repo-wide flag
- Warning-as-error gating — configuring `<TreatWarningsAsErrors>` only on NRT-enabled files prevents regressions
- `[NotNullWhen(true)]` — tells the compiler that the `out` parameter is non-null when the method returns `true`
- `[MaybeNull]` and `[NotNull]` — flow-state attributes for properties and return values the compiler cannot infer
- Suppression debt — each `!` is an unverified assertion; mass `!` usage defeats the value of the analysis
- Incremental rollout — enable per-assembly or per-layer (domain first, then infrastructure) to manage warning volume

**Answer**

I would never enable NRT repo-wide in one commit on a 400-project solution — that guarantees exactly the suppression spiral you are describing. The right strategy is incremental: enable `<Nullable>enable</Nullable>` one assembly at a time, starting with the core domain layer where the `string` metadata models live, since fixing the source of truth there propagates correct annotations outward. For each newly enabled project, treat the warnings as a queue to work through in code-review-sized batches, not a merge-blocker that pressure-tests developers into `!`. Every `#nullable disable` on a touched file is explicitly banned in the team contract — the rule is either leave the file unchanged or enable and fix it. For the document-archive domain, required string IDs (`DocumentId`, `FilePath`) become non-nullable `string` with validation at the deserialization boundary, while optional fields (`Notes`, `Tags`) become `string?` with null-conditional access at every use site. On `TryGet`-style methods that return a found value, `[NotNullWhen(true)]` on the `out` parameter tells the compiler the value is non-null inside the `if` block, eliminating the need for suppressions there. The `!` operator is allowed only at proven-safe interop seams — framework methods that return nullable for legacy reasons but are documented as always non-null in the used code path — and each use requires an inline comment explaining why. Once a project is fully enabled, add it to a CI step that blocks CS8600–CS8625 regressions, so new code in that assembly cannot quietly reintroduce `null` flows.

---
