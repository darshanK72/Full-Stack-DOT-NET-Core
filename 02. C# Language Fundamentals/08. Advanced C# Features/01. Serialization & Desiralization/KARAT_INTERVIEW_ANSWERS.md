# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/08. Advanced C# Features/01. Serialization & Desiralization/`

---

#### Q1. (R) A Redis-backed session service deserializes cached JSON on every request. Review this code — what breaks under load or attack, and what do you fix first?

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

#### Q4. (P) Production still reads `.bin` session files produced years ago with `BinaryFormatter`. You must migrate to System.Text.Json without taking downtime. What is your rollout strategy, and why is "just flip a switch" unsafe?

**Answer:** Deserializing legacy `.bin` files with `BinaryFormatter` on untrusted or stale blobs is a remote-code-execution risk (gadget chains); migration must read old format only in a controlled worker, write new JSON, and dual-read during cutover — never re-enable the compatibility switch on public-facing paths.

- **Phase 1 — freeze writes:** Stop creating new `BinaryFormatter` blobs; new sessions write JSON to a parallel key/path (`session.json` or versioned Redis key).
- **Phase 2 — offline migration worker:** Background job reads each `.bin` in an isolated process with minimal privileges, deserializes once with `BinaryFormatter` (suppressed obsolete warning only here), maps to a versioned DTO, and writes `System.Text.Json` UTF-8 output. Treat every input file as hostile — validate shape, size cap, no arbitrary types.
- **Phase 3 — dual-read in app:** `LoadSession` tries JSON first, falls back to `.bin` once, rewrites JSON on successful legacy read (read-repair), logs metric for remaining legacy count.
- **Phase 4 — retire binary path:** When legacy count hits zero, remove fallback and delete `.bin` artifacts.
- **Why not flip a switch:** `BinaryFormatter` is obsolete (SYSLIB0011) and disabled by default in .NET 8 (**Program.cs** Section 11). Enabling it app-wide restores an unsafe deserializer on any code path that touches binary. JSON migration also forces an explicit contract instead of opaque .NET-only graphs.

**Production takeaway:** Karat wants the security story (untrusted deserialization) plus a practical strangler migration — not "use JSON because it's newer." See **Program.cs** Section 11–12 replacement table.

---

#### Q5. (D) You are adding an optional `IsVerified` flag to a public REST DTO. v1 clients never send the field; v2 clients may send `true`, `false`, or omit it. You need to distinguish "not verified yet" from "explicitly false." Should the property be `bool` or `bool?`, and how does System.Text.Json treat a missing member for each?

**Answer:** Use `bool?` (`Nullable<bool>`) so three states are representable: missing → `null` ("unknown / not provided"), `false` → explicitly not verified, `true` → verified. Plain `bool` collapses missing and false into `false`, losing tri-state semantics.

- **Missing JSON property + `bool IsVerified`:** deserializes to `false` (default for value types) — indistinguishable from `"isVerified": false` (**Program.cs** Section 6 versioning table).
- **Missing JSON property + `bool? IsVerified`:** deserializes to `null` — signals "client did not send the field" (v1 backward compatible).
- **Extra / forward compatibility:** unknown future properties are ignored by default; adding nullable optional fields is the recommended evolution path (**Section 6**).
- **Write behavior:** combine with `DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull` if you want v2 servers to omit `null` on output and keep payloads small (**Section 5** options table).
- **API docs:** document the three states explicitly in OpenAPI (`nullable: true`) so clients do not assume false means "failed verification."

**Production takeaway:** Karat uses JSON versioning to test whether you reach for `bool?` instead of defaulting everything to false — same lesson as optional flags in ASP.NET model binding, applied to DTO evolution.

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
