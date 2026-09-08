# Serialization & Deserialization — Interview Q&A


## Table of Contents

1. [Q1. What is serialization and what problem does it solve?](#q1-what-is-serialization-and-what-problem-does-it-solve)
2. [Q2. How does System.Text.Json differ from Newtonsoft.Json (Json.NET)?](#q2-how-does-systemtextjson-differ-from-newtonsoftjson-jsonnet)
3. [Q3. What does [JsonPropertyName] do, and when would you use it?](#q3-what-does-jsonpropertyname-do-and-when-would-you-use-it)
4. [Q4. What is JsonSerializerOptions and what are the most important settings for a production API?](#q4-what-is-jsonserializeroptions-and-what-are-the-most-important-settings-for-a-production-api)
5. [Q5. How does [JsonIgnore] work and what is [JsonInclude] for?](#q5-how-does-jsonignore-work-and-what-is-jsoninclude-for)
6. [Q6. Explain how System.Text.Json handles missing and extra JSON properties during deserialization.](#q6-explain-how-systemtextjson-handles-missing-and-extra-json-properties-during-deserialization)
7. [Q7. How do you serialize enums as strings rather than integers?](#q7-how-do-you-serialize-enums-as-strings-rather-than-integers)
8. [Q8. What is a custom JsonConverter and when would you write one?](#q8-what-is-a-custom-jsonconverter-and-when-would-you-write-one)
9. [Q9. How does ReferenceHandler.IgnoreCycles differ from ReferenceHandler.Preserve?](#q9-how-does-referencehandlerignorecycles-differ-from-referencehandlerpreserve)
10. [Q10. What are the requirements for XmlSerializer to work on a type?](#q10-what-are-the-requirements-for-xmlserializer-to-work-on-a-type)
11. [Q11. Why is BinaryFormatter obsolete and dangerous?](#q11-why-is-binaryformatter-obsolete-and-dangerous)
12. [Q12. What is JsonNode and when would you use it instead of a typed class?](#q12-what-is-jsonnode-and-when-would-you-use-it-instead-of-a-typed-class)
13. [Q13. Why does creating a new JsonSerializerOptions per call cause performance problems?](#q13-why-does-creating-a-new-jsonserializeroptions-per-call-cause-performance-problems)
14. [Q14. What is the ReferenceHandler.IgnoreCycles trap when serializing parent/child object graphs?](#q14-what-is-the-referencehandlerignorecycles-trap-when-serializing-parentchild-object-graphs)
15. [Q15. What happens when you deserialize a bool property from a payload that omits the field entirely?](#q15-what-happens-when-you-deserialize-a-bool-property-from-a-payload-that-omits-the-field-entirely)
16. [Q16. Why does WriteIndented = true hurt performance in production APIs?](#q16-why-does-writeindented-true-hurt-performance-in-production-apis)
17. [Q17. What fails when you call new XmlSerializer(typeof(T)) inside a hot loop?](#q17-what-fails-when-you-call-new-xmlserializertypeoft-inside-a-hot-loop)
18. [Q18. (Code Review) A Redis-backed session service deserializes cached JSON on every request. Review this code — what breaks under load or attack, and what do you fix first?](#q18-code-review-a-redis-backed-session-service-deserializes-cached-json-on-every-request-review-this-code-what-breaks-under-load-or-attack-and-what-do-you-fix-first)
19. [Q19. (Code Review) A partner integration writes invoice lines to XML nightly; the job fails with InvalidOperationException. Review the model and serializer usage.](#q19-code-review-a-partner-integration-writes-invoice-lines-to-xml-nightly-the-job-fails-with-invalidoperationexception-review-the-model-and-serializer-usage)
20. [Q20. A production service still reads .bin session files produced by BinaryFormatter. You must migrate to System.Text.Json without downtime. What is your rollout strategy, and why is "just flip a switch" unsafe?](#q20-a-production-service-still-reads-bin-session-files-produced-by-binaryformatter-you-must-migrate-to-systemtextjson-without-downtime-what-is-your-rollout-strategy-and-why-is-just-flip-a-switch-unsafe)
21. [Q21. A config-sync worker reads JSON settings files from a shared folder (any authenticated internal user can drop files). What runtime and security issues appear when arbitrary JSON lands in the FeatureFlags dictionary?](#q21-a-config-sync-worker-reads-json-settings-files-from-a-shared-folder-any-authenticated-internal-user-can-drop-files-what-runtime-and-security-issues-appear-when-arbitrary-json-lands-in-the-featureflags-dictionary)

---
> **Module:** 02. C# Language Fundamentals › 08. Advanced C# Features › 01. Serialization & Deserialization  
> **Stack:** .NET 10 · System.Text.Json · XmlSerializer · BinaryFormatter (legacy)

---

## Foundation Questions

---

## Q1. What is serialization and what problem does it solve?

**Concepts**
- object-graph-to-bytes conversion
- wire format (JSON, XML, binary)
- process boundary crossing
- round-trip fidelity
- deserialization creating new instances

**Answer**

Serialization transforms a live object graph into a storable or transmittable format — JSON, XML, or binary bytes — so that data can cross a process, machine, or time boundary. The reverse operation, deserialization, reads that payload and constructs new object instances populated from the encoded fields. The key insight is that deserialization never resurrects the original heap objects; it creates fresh instances, which means identity, events, and any state not captured in the payload are not preserved. REST APIs rely on serialization to convert a `Person` class into an HTTP body, caches rely on it to persist state between app restarts, and message queues rely on it to let services written in different languages share a common wire format. Choosing the wrong serializer — or configuring it incorrectly — affects security (BinaryFormatter gadget-chain attacks), versioning (silent data loss on field rename), and performance (allocation-heavy intermediate strings vs. UTF-8 byte paths).

---

## Q2. How does System.Text.Json differ from Newtonsoft.Json (Json.NET)?

**Concepts**
- built-in vs NuGet dependency
- allocation efficiency and UTF-8 first design
- stricter defaults (case-sensitive, no reference loops)
- JsonSerializerOptions vs JsonSerializerSettings
- migration surface area

**Answer**

System.Text.Json ships in-box with .NET and is designed around UTF-8 first: it avoids intermediate `string` allocations by working directly with `ReadOnlySpan<byte>` and `Utf8JsonWriter`. By contrast, Newtonsoft.Json allocates strings liberally and is more permissive by default — it accepts trailing commas, handles reference loops automatically, and deserializes case-insensitively without any configuration. System.Text.Json is stricter: property matching is case-sensitive unless `PropertyNameCaseInsensitive = true` is set, and circular references throw `JsonException` unless a `ReferenceHandler` is configured. For new .NET 10 code, System.Text.Json is the correct default because it is faster, leaner, and natively integrated with ASP.NET Core's `IResult` pipeline. Newtonsoft.Json remains relevant when a third-party package's public API exposes its types (`JsonConverter`, `JObject`), or when a project requires features like dynamic `JObject` manipulation that have no direct System.Text.Json equivalent.

---

## Q3. What does [JsonPropertyName] do, and when would you use it?

**Concepts**
- wire name decoupling from C# identifier
- camelCase vs PascalCase contracts
- overrides PropertyNamingPolicy
- JSON attribute placement on property

**Answer**

`[JsonPropertyName("wire_name")]` maps a C# property to a specific JSON key name regardless of any `PropertyNamingPolicy` configured on `JsonSerializerOptions`. It is applied directly to the property and takes highest priority — the naming policy is not consulted when the attribute is present. The primary use case is bridging a C# PascalCase naming convention to a snake_case or camelCase API contract without introducing a global naming policy, or preserving a legacy wire name after a C# rename. For example, a DTO property named `OrderId` can be serialized as `"order_id"` by annotating it `[JsonPropertyName("order_id")]`. It also controls deserialization: only the named key is matched on read, so if the incoming JSON uses a different casing, the attribute must match exactly.

---

## Q4. What is JsonSerializerOptions and what are the most important settings for a production API?

**Concepts**
- reusable options instance (singleton pattern)
- PropertyNamingPolicy (CamelCase, SnakeCaseLower)
- PropertyNameCaseInsensitive
- DefaultIgnoreCondition
- MaxDepth guard
- ReferenceHandler

**Answer**

`JsonSerializerOptions` is the configuration object passed to `JsonSerializer.Serialize` and `Deserialize`. Creating a new `JsonSerializerOptions` instance per call is expensive because it reflects over types and builds an internal cache; in production you create one instance (typically registered as a singleton or via `JsonSerializerOptions.Default`) and reuse it. The most important settings for a production API are: `PropertyNamingPolicy = JsonNamingPolicy.CamelCase` to match JavaScript conventions; `PropertyNameCaseInsensitive = true` so that `"orderId"` and `"OrderId"` both deserialize into the same property; `DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull` to keep responses lean; `MaxDepth` (default 64) to prevent stack overflow on maliciously nested JSON; and adding `JsonStringEnumConverter` to the `Converters` list so enum values round-trip as human-readable strings rather than integers. ASP.NET Core 8+ configures sensible defaults via `builder.Services.ConfigureHttpJsonOptions`, but standalone console apps must set these explicitly.

---

## Q5. How does [JsonIgnore] work and what is [JsonInclude] for?

**Concepts**
- property exclusion from serialization and deserialization
- sensitive-field protection
- computed-property omission
- [JsonInclude] for non-public members
- JsonIgnoreCondition enum

**Answer**

`[JsonIgnore]` prevents a property from appearing in JSON output and prevents it from being populated on deserialization. It is the correct way to keep secrets (password hashes, tokens), navigation properties, or purely computed values off the wire. By default `[JsonIgnore]` always ignores the member; the overload `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]` omits only null values on write while still accepting the field on read. The counterpart, `[JsonInclude]`, opts a non-public property or field into serialization — System.Text.Json only accesses public members by default, so fields with `internal` or `private` visibility require `[JsonInclude]` to participate. This is useful for immutable types that expose a private backing field. Neither attribute affects `XmlSerializer`, which has its own `[XmlIgnore]`.

---

## Q6. Explain how System.Text.Json handles missing and extra JSON properties during deserialization.

**Concepts**
- forward compatibility (extra keys ignored)
- backward compatibility (missing keys → type default)
- nullable vs non-nullable semantics
- JsonUnmappedMemberHandling
- versioning strategy

**Answer**

By default, System.Text.Json applies two tolerance rules that enable safe schema evolution. First, extra keys in the incoming JSON that have no corresponding C# property are silently ignored — this enables forward compatibility where an old client receives new fields without errors. Second, JSON properties that are absent from the incoming payload are left at the C# type's default: `null` for reference types and nullable value types, `0` for `int`, `false` for `bool`, and so on. This enables backward compatibility where a new optional field added to a DTO does not break old clients that never send it. The practical implication for versioning is to add new fields as nullable (`string?`, `bool?`) so that a missing field produces `null` rather than a misleading `false` or `0`. To detect truly missing members at the cost of forward compatibility, set `JsonUnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow` on the options.

---

## Q7. How do you serialize enums as strings rather than integers?

**Concepts**
- JsonStringEnumConverter
- [JsonConverter] attribute placement (type or property)
- [JsonPropertyName] on enum members
- global vs per-property converter registration
- stable wire name discipline

**Answer**

By default System.Text.Json writes enums as their underlying integer value, which creates fragile contracts — reordering enum members silently changes their wire representation. The fix is `JsonStringEnumConverter`, which can be applied globally by adding it to `JsonSerializerOptions.Converters`, or per-type with `[JsonConverter(typeof(JsonStringEnumConverter))]` on the enum declaration. Once applied, the C# identifier name is used as the wire string. When the identifier does not match the desired wire name — for example, `FullMember` should serialize as `"full_member"` — annotate the enum member with `[JsonPropertyName("full_member")]`. In .NET 8+ `JsonStringEnumConverter<TEnum>` is the generic form that avoids boxing. The key insight is that string enum contracts are stable across refactors only if the wire names are explicitly pinned with `[JsonPropertyName]`; relying on C# identifier names means a rename breaks all clients.

---

## Q8. What is a custom JsonConverter and when would you write one?

**Concepts**
- JsonConverter<T> base class
- Read / Write method pair
- [JsonConverter] attribute or Converters collection registration
- use cases: non-standard date formats, polymorphism, opaque types

**Answer**

A custom `JsonConverter<T>` gives complete control over how a specific type maps to and from JSON. You override `Read(ref Utf8JsonReader, Type, JsonSerializerOptions)` to parse incoming JSON tokens and return a T instance, and `Write(Utf8JsonWriter, T, JsonSerializerOptions)` to emit JSON for a T value. Common use cases include non-standard date formats (e.g., emitting `DateTime` as `"O"` ISO 8601 strings rather than the default format), reading a union type where the JSON shape indicates the runtime subtype, wrapping a domain primitive that should serialize as a plain string rather than an object, and interoperating with a partner API that uses a number for a field that is semantically a boolean. Converters are registered either globally via `JsonSerializerOptions.Converters.Add(new MyConverter())` or per-property/type with `[JsonConverter(typeof(MyConverter))]`, with the attribute taking precedence. The converter sees raw JSON tokens, so it must handle all valid states of the input.

---

## Q9. How does ReferenceHandler.IgnoreCycles differ from ReferenceHandler.Preserve?

**Concepts**
- circular reference detection
- IgnoreCycles nulls repeated references
- Preserve emits $id / $ref metadata
- client compatibility
- ORM navigation property gotcha

**Answer**

Both options handle object graphs that contain cycles, but they make different trade-offs. `ReferenceHandler.IgnoreCycles` detects when an object reference has already been visited during serialization and writes `null` in place of the repeated reference. This produces clean, readable JSON with no extra metadata, but the deserialized graph is lossy — the back-reference is gone. `ReferenceHandler.Preserve` annotates every object with a `"$id"` field on first occurrence and replaces subsequent occurrences with `{"$ref":"1"}`. The output is valid JSON but non-standard; clients must understand the `$ref` contract, which most JavaScript or mobile frameworks do not natively. The practical guidance is to use `IgnoreCycles` for one-way projection DTOs — logging snapshots, response payloads — where cycles are structural accidents rather than meaningful data. Use `Preserve` only when the full graph must round-trip within a .NET-to-.NET pipeline. For public APIs, the better solution is to break the cycle at the DTO layer rather than configuring either handler.

---

## Q10. What are the requirements for XmlSerializer to work on a type?

**Concepts**
- public parameterless constructor requirement
- public read/write properties
- supported member types
- [XmlRoot], [XmlElement], [XmlAttribute] attributes
- XmlSerializer construction cost

**Answer**

`XmlSerializer` imposes three requirements that are checked at runtime when the serializer is first constructed or when `Serialize`/`Deserialize` is called: the target type must have a public parameterless constructor, every serialized member must be a public read/write property (or public field), and collection members must be of types the serializer understands — `Dictionary<K,V>` is not natively supported and requires a wrapper. Violating these rules throws `InvalidOperationException` at runtime, not at compile time, which makes XmlSerializer failures particularly surprising in staging. Attributes control the XML shape: `[XmlRoot("RootName", Namespace = "…")]` names the document root element and its namespace; `[XmlElement("ElementName")]` renames a property in XML; `[XmlAttribute]` serializes a property as an XML attribute rather than a child element; and `[XmlIgnore]` skips a member. Constructing a new `XmlSerializer(typeof(T))` is expensive because it reflects over the type and may generate a temporary assembly; reuse one instance per type in production, typically as a `static readonly` field.

---

## Q11. Why is BinaryFormatter obsolete and dangerous?

**Concepts**
- gadget-chain deserialization attack
- SYSLIB0011 obsolete warning
- .NET 8 disabled by default
- migration alternatives (System.Text.Json, protobuf, MessagePack)
- [Serializable] as legacy marker

**Answer**

`BinaryFormatter` serialized .NET object graphs as opaque binary streams relying on `[Serializable]` as a marker. The fundamental security problem is that deserializing untrusted binary data can execute arbitrary code through gadget-chain attacks — an attacker crafts a payload that, when deserialized, invokes methods on types already loaded in the process. This class of vulnerability has been exploited extensively in .NET, Java, and Python frameworks. Microsoft marked BinaryFormatter obsolete (SYSLIB0011) in .NET 5 and disabled it by default in .NET 8; enabling it via `AppContext.SetSwitch` is supported only for rare backward-compatibility scenarios and should never be done in new code. Existing `.bin` files produced by BinaryFormatter must be treated as untrusted until migrated. Modern replacements are: System.Text.Json for REST APIs and config; XmlSerializer when an XML schema or SOAP partner is required; Protocol Buffers (`Google.Protobuf`) or MessagePack for cross-language high-performance binary; and System.Text.Json UTF-8 bytes for .NET-only performance-sensitive paths.

---

## Q12. What is JsonNode and when would you use it instead of a typed class?

**Concepts**
- dynamic JSON document model
- JsonNode / JsonObject / JsonArray / JsonValue
- mutable tree vs immutable Utf8JsonReader
- use case: unknown or heterogeneous schema
- typed DTO preferred when schema is stable

**Answer**

`JsonNode.Parse(json)` returns a mutable, navigable tree of `JsonObject`, `JsonArray`, and `JsonValue` nodes without requiring a pre-defined C# class. You access values with the indexer — `node["institute"]?.GetValue<string>()` — and can mutate the tree before re-serializing. This is useful when the JSON schema is unknown at compile time, when only a few fields need extraction from a large document, when building a JSON transformation pipeline, or when you are prototyping before defining a proper DTO. The trade-off is that there is no compile-time shape checking and no IDE autocomplete on the field names, which makes `JsonNode` a maintenance liability when overused. The correct default is always a strongly typed DTO; reach for `JsonNode` only when the schema is genuinely dynamic. In .NET 10 `JsonObject` also supports `Add`, `Remove`, and direct manipulation which makes it practical for test fixtures and proxy endpoints that forward modified JSON.

---

## Gotchas — Serialization & Deserialization (Interview Traps)

---

#### Gotcha 1. `[JsonIgnore]` vs `[JsonPropertyName]` — ignoring vs renaming serialized properties

**Concepts**
- `[JsonIgnore]` excludes the property from serialization entirely
- `[JsonPropertyName("camelCase")]` renames the property in JSON output
- both attributes live in `System.Text.Json.Serialization`
- confusion causes either missing fields or wrong field names in the JSON

**Answer**

`[JsonIgnore]` completely excludes a property from both serialization and deserialization, so the field never appears in JSON output and is never populated on deserialization. `[JsonPropertyName("name")]` keeps the property in the JSON but maps it to a different name — the C# name `FirstName` can serialize as `"first_name"` or `"firstName"` depending on convention. A common mistake is using `[JsonIgnore]` when the intent was only to rename, causing the consumer API to receive a null or missing field. The two attributes can be combined if you also need to suppress a renamed property conditionally using `[JsonIgnore(Condition = ...)]`.

---

#### Gotcha 2. Circular reference handling — System.Text.Json throws by default; ReferenceHandler.Preserve

**Concepts**
- `JsonException: A possible object cycle was detected` on circular object graphs
- `JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve`
- `$id` and `$ref` metadata in the output
- `ReferenceHandler.IgnoreCycles` silently writes null instead of the back-reference

**Answer**

By default `System.Text.Json` throws a `JsonException` when it detects a circular reference during serialization, such as `Order.Customer.Orders` pointing back to `Order`. Setting `ReferenceHandler.Preserve` resolves the cycle by emitting `$id`/`$ref` metadata tokens in the JSON — the first occurrence of an object gets `$id` and subsequent references use `$ref`. This format is understood by .NET deserializers but is non-standard and may break JavaScript clients. `ReferenceHandler.IgnoreCycles` is the softer option: it writes `null` instead of the back-reference, which round-trips incorrectly but does not throw. The cleanest production fix for public APIs is to use projection DTOs that break the cycle by design.

---

#### Gotcha 3. `[Serializable]` (BinaryFormatter) is obsolete and insecure — do not use

**Concepts**
- `BinaryFormatter` is disabled by default in .NET 5+ and throws by default
- deserialization of untrusted binary data allows remote code execution
- `[Serializable]` attribute alone does nothing for `System.Text.Json` or `XmlSerializer`
- migration path: use `System.Text.Json` or `DataContractSerializer`

**Answer**

`BinaryFormatter` was the original .NET serializer that relied on the `[Serializable]` attribute. It is disabled by default in .NET 5+ and will throw `NotSupportedException` unless explicitly re-enabled with an AppContext switch. The reason for the hard deprecation is a well-documented class of deserialization gadget-chain attacks — deserializing untrusted binary data can execute arbitrary code on the server. Adding `[Serializable]` to a class today has no effect on `System.Text.Json` or `XmlSerializer`; it is only meaningful for the legacy `BinaryFormatter`. Any project still using `BinaryFormatter` should migrate to `System.Text.Json` for general-purpose serialization or `DataContractSerializer` for WCF compatibility.

---

#### Gotcha 4. `JsonSerializer.Deserialize` returns null for null JSON — not an empty object

**Concepts**
- `JsonSerializer.Deserialize<T>("null")` returns `null` for reference types
- no automatic empty-object construction on null input
- nullable reference type annotation: `T?` return type in recent .NET
- guard pattern: null check before using the deserialized object

**Answer**

When `JsonSerializer.Deserialize<MyDto>("null")` is called, it returns `null` — the literal JSON string `"null"` deserializes to the null reference for any reference type. Similarly, passing an empty or null `string` as input throws `ArgumentNullException`. Code that does `var dto = JsonSerializer.Deserialize<OrderDto>(body); dto.Items.Count` will throw `NullReferenceException` when the body is `"null"`. The method's return type annotation in .NET 6+ is `T?`, signaling the nullable contract. Always null-check or use `?? throw new InvalidDataException(...)` immediately after deserialization to fail fast with a meaningful message rather than a cryptic downstream null dereference.

---

#### Gotcha 5. Polymorphic deserialization — base type result when derived type is not registered

**Concepts**
- `JsonSerializer.Deserialize<Animal>(json)` returns an `Animal`, not a `Dog`
- `[JsonDerivedType]` attribute (or `JsonPolymorphismOptions`) required for .NET 7+ polymorphism
- discriminator property `$type` in the JSON
- Newtonsoft.Json TypeNameHandling comparison

**Answer**

If you serialize a `Dog` instance (which inherits from `Animal`) and then deserialize the JSON back as `Animal`, you get an `Animal` instance — not a `Dog`. The derived-type metadata is lost because `System.Text.Json` does not emit type discriminators by default. To round-trip polymorphic hierarchies, annotate the base class with `[JsonDerivedType(typeof(Dog), typeDiscriminator: "dog")]` in .NET 7+. The serializer then emits a `$type: "dog"` field and uses it during deserialization to construct the correct derived type. Omitting the registration causes a silent data loss: no exception is thrown, but the deserialized object is missing all `Dog`-specific properties.

---

#### Gotcha 6. `[JsonConstructor]` — required when the type has no parameterless constructor

**Concepts**
- `System.Text.Json` calls the parameterless constructor by default
- `JsonException` thrown when no parameterless constructor exists
- `[JsonConstructor]` marks the constructor to use for deserialization
- parameter names must match JSON property names (case-insensitive)

**Answer**

`System.Text.Json` looks for a parameterless constructor when deserializing. If a class only exposes a parameterized constructor (common in immutable record-like types), deserialization throws `JsonException: Each parameter in the deserialization constructor on type must bind to an object property or field`. Applying `[JsonConstructor]` to the constructor tells the serializer to use it, and it matches constructor parameters to JSON properties by name (case-insensitive by default). Records with primary constructors automatically work without `[JsonConstructor]` in .NET 5+ because the compiler generates both the parameterized constructor and the required property declarations. For non-record classes, `[JsonConstructor]` is the explicit annotation that enables immutable value objects to round-trip correctly.

---

#### Gotcha 7. `DateTime` serialization format differences — ISO 8601 vs custom formats

**Concepts**
- `System.Text.Json` defaults to ISO 8601 (`"2025-03-15T10:30:00"`)
- time zone information: UTC Z suffix vs unspecified `DateTimeKind`
- `DateTimeOffset` is preferred over `DateTime` for unambiguous round-trips
- custom `JsonConverter<DateTime>` needed for non-standard formats

**Answer**

`System.Text.Json` serializes `DateTime` values as ISO 8601 strings by default, including a `Z` suffix when `Kind == DateTimeKind.Utc` and no suffix for `Unspecified`. A `DateTime` without kind information deserialized from `"2025-03-15T10:30:00"` produces `DateTimeKind.Unspecified`, which behaves differently from `Utc` in comparison and conversion operations. Legacy systems often produce non-ISO date formats like `"15/03/2025"`, which `System.Text.Json` cannot parse and throws on. The correct fix is to use `DateTimeOffset` throughout — it stores the offset explicitly and eliminates ambiguity. For legacy format support, implement a custom `JsonConverter<DateTime>` that uses `DateTime.ParseExact` with the known format.

---

#### Gotcha 8. `System.Text.Json` is case-insensitive only with `PropertyNameCaseInsensitive = true`

**Concepts**
- default behavior: property name matching is case-sensitive
- `"firstname"` does not map to `FirstName` without the option
- `JsonSerializerOptions.PropertyNameCaseInsensitive = true` enables relaxed matching
- performance cost of case-insensitive matching

**Answer**

By default `System.Text.Json` uses case-sensitive property matching during deserialization: a JSON payload with `"firstname": "Alice"` will NOT populate a C# property named `FirstName`. Properties that do not match are silently ignored — no exception, the property stays at its default value. This is a frequent source of "deserialized object has all null properties" bugs when the JSON uses snake_case or all-lowercase conventions. Setting `JsonSerializerOptions.PropertyNameCaseInsensitive = true` enables case-insensitive matching. ASP.NET Core's default `JsonSerializerOptions` sets this to `true`, which is why the bug often appears only in console or unit test code that constructs its own options.

---

#### Gotcha 9. XmlSerializer requires parameterless constructor and public properties

**Concepts**
- `XmlSerializer` reflects on public, settable properties
- no parameterless constructor causes `InvalidOperationException` at runtime
- read-only properties are silently skipped during deserialization
- `[XmlIgnore]` to exclude a property from XML serialization

**Answer**

`XmlSerializer` requires a public parameterless constructor — without it the deserializer throws `InvalidOperationException: There was an error reflecting type`. Unlike `System.Text.Json`, it cannot use a parameterized constructor or `[XmlConstructor]`. In addition, only public properties with both a getter and a setter are deserialized; read-only properties are serialized (output only) but silently skipped on deserialization, meaning the deserialized object will have its read-only properties at their default values even if the XML contains the corresponding elements. Immutable or record types do not work with `XmlSerializer` without workarounds. These constraints make `XmlSerializer` a poor fit for modern immutable DTO design.

---

#### Gotcha 10. `DataContractSerializer` vs `XmlSerializer` — opt-in vs opt-out model

**Concepts**
- `DataContractSerializer`: opt-in — only `[DataMember]`-annotated properties are serialized
- `XmlSerializer`: opt-out — all public properties are serialized unless `[XmlIgnore]` is applied
- `DataContractSerializer` supports private fields via `[DataMember]`
- `DataContractSerializer` is the default for WCF service contracts

**Answer**

`DataContractSerializer` follows an opt-in model: a class must be annotated with `[DataContract]` and each member with `[DataMember]`; unannotated members are excluded. `XmlSerializer` follows opt-out: all public read/write properties are included unless explicitly decorated with `[XmlIgnore]`. A common mistake when switching from `XmlSerializer` to `DataContractSerializer` is forgetting to add `[DataMember]` to all required properties, causing them to silently disappear from the serialized output. The opt-in model is safer for sensitive data — new properties are excluded by default — but requires discipline when adding new members. `DataContractSerializer` also supports private fields with `[DataMember]`, which `XmlSerializer` does not, making it more suitable for domain objects with encapsulated state.

---

## Real-World Scenarios

---

## Q18. (Code Review) A Redis-backed session service deserializes cached JSON on every request. Review this code — what breaks under load or attack, and what do you fix first?

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

**Concepts**
- null-forgiving operator suppressing cache-miss null
- sensitive field exposure in cached payload
- recursive self-referencing type
- WriteIndented in hot path
- MaxDepth missing

| Category | Problem | Impact |
|---|---|---|
| Correctness | `_redis.GetString(redisKey)!` suppresses null; a cache miss returns null and causes NRE on Deserialize | NullReferenceException under any cache eviction |
| Security | `PasswordHash` serialized into Redis cache payload | Hash leaks if cache is exfiltrated or logged |
| Security | `Nested: List<UserSession>` allows arbitrarily deep JSON | Potential DoS via stack overflow without MaxDepth |
| Performance | `WriteIndented = true` bloats every cached payload | Increased Redis storage and network cost |

**Fix priority:**
1. Check for null/empty from Redis before deserializing — null indicates cache miss, return null gracefully.
2. Apply `[JsonIgnore]` to `PasswordHash` so secrets never enter the cache serialization path.
3. Set `MaxDepth = 32` on the options to prevent deep-nesting attacks.
4. Set `WriteIndented = false` for the cache path; use a separate debug options instance if needed.
5. Reconsider whether `Nested: List<UserSession>` is an intentional design or a structural accident; if circular, use a DTO without recursive self-reference.

---

## Q19. (Code Review) A partner integration writes invoice lines to XML nightly; the job fails with InvalidOperationException. Review the model and serializer usage.

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

**Concepts**
- XmlSerializer parameterless constructor requirement
- new XmlSerializer in hot path
- IEnumerable vs concrete List serialization
- sealed class vs XmlSerializer constraints

| Category | Problem | Impact |
|---|---|---|
| Runtime failure | `InvoiceLine` has no public parameterless constructor — XmlSerializer throws InvalidOperationException immediately | Job fails on first run |
| Performance | `new XmlSerializer(...)` created on every export call | Repeated reflection cost; historical temp-assembly leak |
| Correctness | `lines.ToList()` materializes the entire enumerable before serializing | Increased memory for large export batches |

**Fix priority:**
1. Add a public parameterless constructor to `InvoiceLine` (e.g., `public InvoiceLine() {}`); the parameterized constructor can remain for convenience.
2. Declare the serializer as a `static readonly` field to avoid repeated construction.
3. Accept `List<InvoiceLine>` directly in the method signature to make the API contract explicit and avoid the `.ToList()` copy.

---

## Q20. A production service still reads .bin session files produced by BinaryFormatter. You must migrate to System.Text.Json without downtime. What is your rollout strategy, and why is "just flip a switch" unsafe?

**Concepts**
- BinaryFormatter deserialization gadget attacks
- dual-read migration pattern
- format versioning in cache keys
- canary deployment
- rollback safety

**Answer**

"Just flipping a switch" is unsafe for two independent reasons. First, existing `.bin` files contain data in a format that System.Text.Json cannot read — switching formats immediately would cause every cache read to fail (or worse, deserialize garbage), producing either NullReferenceExceptions or corrupted session state. Second, BinaryFormatter `.bin` files on disk or in a shared cache should be treated as untrusted until they are consumed and discarded, because the gadget-chain attack surface exists for any process that deserializes them. The rollout strategy is a dual-read migration: deploy version 2 that writes JSON but falls back to reading the BinaryFormatter format if the JSON key is absent; include a format version tag in the cache key (e.g., `session:v2:{userId}`). Old v1 keys expire naturally or are invalidated on first JSON write. After all v1 keys have expired — typically one session TTL later — deploy version 3 that removes the BinaryFormatter read path entirely. The canary deployment ensures that if JSON deserialization introduces a regression, it affects only a fraction of users before full rollout. Never re-enable `AppContext.SetSwitch("System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization", true)` for new flows.

---

## Q21. A config-sync worker reads JSON settings files from a shared folder (any authenticated internal user can drop files). What runtime and security issues appear when arbitrary JSON lands in the FeatureFlags dictionary?

**Concepts**
- Dictionary<string, object> JSON deserialization as JsonElement
- arbitrary object materialization risk
- untrusted input from shared file system
- JSON depth bomb
- configuration integrity

**Answer**

When `System.Text.Json` deserializes a `Dictionary<string, object>`, it materializes each value as a `JsonElement` rather than a primitive — the `object` type causes the serializer to emit a boxed `JsonElement` wrapper for every value. Calling code that casts values to `bool` or `string` will throw `InvalidCastException` at runtime for any key because the runtime type is `JsonElement`, not the expected primitive. The deeper problem is security: because any authenticated internal user can drop files into the shared folder, a malicious actor can craft a settings file with deeply nested JSON that exhausts stack space (JSON depth bomb), inject a `RetryCount` of one million to cause a denial-of-service, or set `ApiBaseUrl` to an attacker-controlled host. The correct fix is to use strongly typed deserialization — define explicit properties for each known flag, validate every deserialized value against an allowlist before caching it, set `MaxDepth` on the options, and treat the shared folder as an untrusted input boundary by applying a schema validation step (JSON Schema or manual property range checks) before the cache is updated.
