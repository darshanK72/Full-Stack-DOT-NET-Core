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

## Gotchas

---

## Q13. Why does creating a new JsonSerializerOptions per call cause performance problems?

**Concepts**
- type-metadata cache inside JsonSerializerOptions
- reflection cost on first Serialize/Deserialize per type
- static or singleton options pattern
- JsonSerializerOptions.Default (.NET 8+)

**Answer**

Every `JsonSerializerOptions` instance maintains an internal cache that maps CLR types to their serialization metadata — property names, converters, constructor parameters, and so on. This cache is populated lazily on the first call for each type via reflection, which is relatively expensive. Creating a new options instance for every call means the cache is discarded after each use, so every request pays the full reflection cost for every type in the graph. Under high load — a REST endpoint processing thousands of requests per second — this produces measurable latency and excessive garbage-collector pressure. The fix is to create one `JsonSerializerOptions` instance and reuse it, registered as a singleton in the DI container or stored in a `static readonly` field. In .NET 8+ `JsonSerializerOptions.Default` provides a pre-configured instance. ASP.NET Core's `IOptions<JsonSerializerOptions>` injects the single shared instance that the framework already configures.

---

## Q14. What is the ReferenceHandler.IgnoreCycles trap when serializing parent/child object graphs?

**Concepts**
- second-visit null substitution
- lossy round-trip
- bidirectional navigation property
- incorrect tree rendering on the client
- IgnoreCycles vs Preserve vs DTO projection

**Answer**

`ReferenceHandler.IgnoreCycles` writes `null` wherever it encounters an object reference it has already serialized in the current traversal. This means that in a parent/child graph where `Child.Parent` points back to the parent, the parent reference on the child is silently replaced with `null` in the JSON output. A mobile or browser client that renders a tree based on this JSON will see children with `null` parents, which breaks tree navigation logic. The bug is subtle because the serialization succeeds without errors and the top-level structure looks correct. The correct approaches are: project to a one-directional DTO that excludes back-references before serializing; or use `ReferenceHandler.Preserve` for .NET-to-.NET pipelines where the receiving side understands `$id`/`$ref` syntax. For public APIs, introducing a dedicated response model that explicitly controls which relationships are included is the cleanest solution.

---

## Q15. What happens when you deserialize a bool property from a payload that omits the field entirely?

**Concepts**
- missing member → type default (false for bool)
- bool vs bool? distinction
- tristate semantics (true / false / not-set)
- required keyword (.NET 7+)
- JsonUnmappedMemberHandling

**Answer**

When `System.Text.Json` deserializes a payload that does not include a field corresponding to a `bool` property, it leaves the property at its C# default: `false`. This is indistinguishable from a client that explicitly sent `"isVerified": false`. If the business logic needs to distinguish "the client explicitly set this to false" from "the client never sent this field at all," the property type must be `bool?`. A `null` result then means the field was absent, `false` means it was explicitly false, and `true` means it was explicitly true. Marking a property `required` (C# 11 / .NET 7+) causes a `JsonException` at deserialization time if the key is absent, which is appropriate for fields that are mandatory on every request. For evolving DTOs, defaulting nullable optional fields to `null` and documenting the semantic difference from `false` is the correct versioning discipline.

---

## Q16. Why does WriteIndented = true hurt performance in production APIs?

**Concepts**
- extra whitespace bytes in every response
- response payload size increase
- serialization CPU overhead
- development vs production options
- environment-specific configuration

**Answer**

`WriteIndented = true` causes `JsonSerializer` to emit newlines and spaces between every key and value to produce human-readable output. This increases the byte size of every response — sometimes by 30–50% for deeply nested objects — and increases the CPU time spent writing whitespace characters. Under high request volume this adds up to meaningful additional bandwidth and slightly higher serialization latency. In development it aids debugging and makes payloads readable in browser DevTools, so it is appropriate there. In production, `WriteIndented` should be `false`. The standard pattern is to configure options per environment, or to always use compact output and rely on browser extensions or API clients that pretty-print on the client side. ASP.NET Core's default `JsonSerializerOptions` uses compact output, so this pitfall typically appears when developers copy their debug options object into the production configuration.

---

## Q17. What fails when you call new XmlSerializer(typeof(T)) inside a hot loop?

**Concepts**
- XmlSerializer construction reflection cost
- temporary assembly generation (legacy behavior)
- static readonly serializer per type pattern
- constructor vs Serialize/Deserialize cost
- memory leak from dynamic assembly

**Answer**

Constructing a new `XmlSerializer` with a `Type` argument triggers reflection over that type's properties and, on some runtimes and for some type configurations, generates a temporary serialization assembly. This construction cost is significant — far higher than a single serialization call once the serializer exists. Calling `new XmlSerializer(typeof(T))` inside a hot loop or on every request creates a new object each time without benefiting from the cached serialization plan from previous calls, resulting in repeated reflection and, historically on .NET Framework, potential memory leaks from orphaned dynamic assemblies. The fix is to declare the serializer as a `static readonly` field on the class: `private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(T));`. The serializer is constructed once at class initialization time and reused for all subsequent calls. There is no thread-safety issue with reusing `XmlSerializer` across threads; Serialize and Deserialize are thread-safe on a single instance.

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
