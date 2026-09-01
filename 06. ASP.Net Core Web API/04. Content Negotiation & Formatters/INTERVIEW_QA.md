# Content Negotiation & Formatters — Interview Q&A
> Back to [README](../README.md)

## Table of Contents

- [Chapter 04. Content Negotiation & Formatters](#chapter-04-content-negotiation-formatters)
  - [Q1. What is content negotiation in ASP.NET Core Web API?](#chapter-04-content-negotiation-formatters-q1)
  - [Q2. What is the Accept HTTP header used for?](#chapter-04-content-negotiation-formatters-q2)
  - [Q3. What is the Content-Type header used for in API requests?](#chapter-04-content-negotiation-formatters-q3)
  - [Q4. What is the default JSON serializer in ASP.NET Core 8?](#chapter-04-content-negotiation-formatters-q4)
  - [Q5. What is camelCase JSON naming and why is it used in Web APIs…](#chapter-04-content-negotiation-formatters-q5)
  - [Q6. What is the difference between input formatters and output f…](#chapter-04-content-negotiation-formatters-q6)
  - [Q7. What HTTP status code is returned when content negotiation f…](#chapter-04-content-negotiation-formatters-q7)
  - [Q8. What does `[Produces("application/json")]` do?](#chapter-04-content-negotiation-formatters-q8)
  - [Q9. What does `[Consumes("application/xml")]` do?](#chapter-04-content-negotiation-formatters-q9)
  - [Q10. What is the difference between `System.Text.Json` and Newton…](#chapter-04-content-negotiation-formatters-q10)
  - [Q11. How does model binding relate to input formatters?](#chapter-04-content-negotiation-formatters-q11)
  - [Q12. What is an output formatter?](#chapter-04-content-negotiation-formatters-q12)
  - [Q13. What is the difference between JSON and XML responses in Web…](#chapter-04-content-negotiation-formatters-q13)
  - [Q14. What does `[JsonPropertyName]` do?](#chapter-04-content-negotiation-formatters-q14)
  - [Q15. What is `AddJsonOptions` used for?](#chapter-04-content-negotiation-formatters-q15)
  - [Q16. When would you register a custom output formatter?](#chapter-04-content-negotiation-formatters-q16)
  - [Q17. What is the default response format for ASP.NET Core Web API…](#chapter-04-content-negotiation-formatters-q17)
  - [Q18. What is the difference between serialization and model bindi…](#chapter-04-content-negotiation-formatters-q18)
- [Gotchas](#gotchas)
- [Scenario-Based Questions](#scenario-based-questions-karat-format)

---

## Chapter 04. Content Negotiation & Formatters

### Q1. What is content negotiation in ASP.NET Core Web API? {#chapter-04-content-negotiation-formatters-q1}

What is content negotiation in ASP.NET Core Web API?

**Answer:** Content negotiation is the process by which the server selects a response format (JSON, XML, CSV) based on the client's `Accept` header and the available output formatters registered in the application. It ensures clients receive data in a representation they can process.

- Client sends `Accept: application/json` → server serializes with System.Text.Json.
- If no formatter matches and fallback is disabled, the server returns 406 Not Acceptable.
- `[Produces("application/json")]` on actions restricts or declares supported output types.
- Input content negotiation uses the `Content-Type` header to select input formatters.
- ASP.NET Core 8 Web API defaults to JSON; additional formatters must be explicitly registered.

---

### Q2. What is the Accept HTTP header used for? {#chapter-04-content-negotiation-formatters-q2}

What is the Accept HTTP header used for?

**Answer:** The Accept header tells the server which response media types the client prefers, ordered by quality values (`q=`). The server selects the best matching output formatter during content negotiation.

- Example: `Accept: application/json, application/xml;q=0.9`.
- Server picks the highest-preference type it can produce with a registered formatter.
- If the server cannot satisfy any listed type, it may return 406 or fall back to a default.
- Accept governs response format; it does not describe the request body format.
- API clients should set Accept explicitly; browsers default to `*/*`.

---

### Q3. What is the Content-Type header used for in API requests? {#chapter-04-content-negotiation-formatters-q3}

What is the Content-Type header used for in API requests?

**Answer:** The Content-Type header declares the media type of the request body so the server selects the correct input formatter for deserialization. It is required on POST, PUT, and PATCH requests that include a body.

- `Content-Type: application/json` → System.Text.Json input formatter deserializes the body.
- `Content-Type: application/xml` → XML input formatter if registered.
- Mismatch between Content-Type and actual body format causes model binding failures or 415 Unsupported Media Type.
- `[Consumes("application/json")]` on an action rejects requests with other Content-Type values.
- Multipart form uploads use `Content-Type: multipart/form-data` for file uploads.

---

### Q4. What is the default JSON serializer in ASP.NET Core 8? {#chapter-04-content-negotiation-formatters-q4}

What is the default JSON serializer in ASP.NET Core 8?

**Answer:** ASP.NET Core 8 uses `System.Text.Json` as the default JSON serializer for both input and output formatters in Web API projects. It replaces Newtonsoft.Json as the framework default, though Newtonsoft can be added optionally.

- Registered automatically when calling `AddControllers()` in the Web SDK template.
- Configured via `AddJsonOptions()` on `JsonSerializerOptions`.
- Default naming policy is camelCase for property names in JSON.
- System.Text.Json is faster and uses less memory than Newtonsoft.Json for typical API payloads.
- Add `AddNewtonsoftJson()` only when legacy features like `$type` polymorphism or specific converters are required.

---

### Q5. What is camelCase JSON naming and why is it used in Web APIs? {#chapter-04-content-negotiation-formatters-q5}

What is camelCase JSON naming and why is it used in Web APIs?

**Answer:** camelCase names JSON properties with a lowercase first letter (`customerName`) matching JavaScript and front-end conventions, while C# properties remain PascalCase (`CustomerName`). ASP.NET Core 8 applies camelCase by default via `JsonNamingPolicy.CamelCase`.

- Outbound: `CustomerName` serializes as `"customerName"` in JSON responses.
- Inbound: JSON keys must match camelCase unless `PropertyNameCaseInsensitive = true`.
- Consistency with JavaScript clients avoids manual property mapping on the front end.
- OpenAPI/Swagger schemas reflect camelCase names in generated client code.
- PascalCase JSON from legacy clients may fail to bind, causing silent null defaults.

---

### Q6. What is the difference between input formatters and output formatters? {#chapter-04-content-negotiation-formatters-q6}

What is the difference between input formatters and output formatters?

**Answer:** Input formatters deserialize the request body into action parameters during model binding; output formatters serialize the action result into the response body during result execution. Each set is selected independently based on Content-Type (input) and Accept (output).

- Input: runs before the action — `Content-Type: application/json` → `JsonInputFormatter`.
- Output: runs after the action — `Accept: application/json` → `JsonOutputFormatter`.
- Input formatters populate `[FromBody]` parameters; output formatters write `OkObjectResult` values.
- Custom formatters implement `InputFormatter` or `OutputFormatter` base classes.
- An API can accept JSON and return CSV by registering different formatters for each direction.

---

### Q7. What HTTP status code is returned when content negotiation fails? {#chapter-04-content-negotiation-formatters-q7}

What HTTP status code is returned when content negotiation fails?

**Answer:** HTTP 406 Not Acceptable is returned when the server cannot produce any representation matching the client's Accept header and no fallback formatter is configured. Without explicit 406 configuration, the framework may silently fall back to the default JSON formatter.

- Client sends `Accept: application/pdf` but only JSON formatters are registered → 406.
- Enable strict behavior via formatter options or ensure `[Produces]` matches registered formatters.
- Returning 200 with JSON when the client requested XML violates HTTP semantics.
- 415 Unsupported Media Type is the input-side equivalent — unsupported Content-Type on the request.
- Integration tests should assert 406 for unsupported Accept headers on strict APIs.

---

### Q8. What does `[Produces("application/json")]` do? {#chapter-04-content-negotiation-formatters-q8}

What does `[Produces("application/json")]` do?

**Answer:** `[Produces]` declares the content types an action can return, influencing content negotiation and OpenAPI documentation. When only JSON is listed, the formatter pipeline selects the JSON output formatter for that action.

- Metadata attribute — documents intent for Swagger/Swashbuckle schema generation.
- Can list multiple types: `[Produces("application/json", "application/xml")]` if both formatters exist.
- Declaring a type without a registered formatter misdocuments the API in OpenAPI.
- Helps restrict formatters so CSV or XML formatters do not accidentally handle JSON endpoints.
- Does not alone enforce 406 — formatter configuration controls fallback behavior.

---

### Q9. What does `[Consumes("application/xml")]` do? {#chapter-04-content-negotiation-formatters-q9}

What does `[Consumes("application/xml")]` do?

**Answer:** `[Consumes]` restricts which Content-Type values an action accepts for the request body, acting as an action constraint during endpoint selection. Requests with a non-matching Content-Type receive 415 Unsupported Media Type.

- `[Consumes("application/xml")]` — only XML body requests match this action.
- Used to isolate legacy XML endpoints while the rest of the API accepts JSON.
- Multiple types allowed: `[Consumes("application/json", "application/xml")]`.
- Requires a registered input formatter for each consumed type.
- Without a matching formatter, model binding fails even if the action is selected.

---

### Q10. What is the difference between `System.Text.Json` and Newtonsoft.Json in ASP.NET Core? {#chapter-04-content-negotiation-formatters-q10}

What is the difference between `System.Text.Json` and Newtonsoft.Json in ASP.NET Core?

**Answer:** System.Text.Json is the built-in, high-performance serializer with camelCase defaults and strict standards compliance; Newtonsoft.Json (Json.NET) is an optional third-party serializer with richer features like reference loop handling, `$type` polymorphism, and broader custom converter support.

- System.Text.Json: faster, lower allocation, default in ASP.NET Core 8 templates.
- Newtonsoft: add via `AddNewtonsoftJson()` — needed for some legacy serialization scenarios.
- Configuration differs: `AddJsonOptions()` vs `AddNewtonsoftJson(options => ...)`.
- Mixing both in one app creates inconsistent naming and behavior across endpoints.
- Choose one serializer per application unless migrating incrementally with clear boundaries.

---

### Q11. How does model binding relate to input formatters? {#chapter-04-content-negotiation-formatters-q11}

How does model binding relate to input formatters?

**Answer:** Model binding orchestrates populating action parameters from the HTTP request; for body parameters, it delegates to input formatters to deserialize the raw request stream into a CLR object. The input formatter is selected based on the request's Content-Type header.

- Route/query parameters bind directly without formatters.
- `[FromBody]` complex types trigger input formatter selection and deserialization.
- If no input formatter matches Content-Type, binding fails with 415 or empty model.
- Input formatters run before validation — Data Annotations validate the bound object afterward.
- `[ApiController]` infers `[FromBody]` for complex types without explicit attributes.

---

### Q12. What is an output formatter? {#chapter-04-content-negotiation-formatters-q12}

What is an output formatter?

**Answer:** An output formatter is a component that serializes the action result object into the HTTP response body in a specific media type. ASP.NET Core includes built-in JSON, XML, and string formatters, and supports custom formatters for CSV, PDF, or other formats.

- Selected during result execution based on Accept header, `[Produces]`, and `CanWriteType`.
- `JsonOutputFormatter` uses System.Text.Json to write JSON responses.
- Custom formatters extend `TextOutputFormatter` or `OutputFormatter` and register in `AddControllers(options => options.OutputFormatters.Add(...))`.
- `CanWriteType` must accurately declare supported CLR types — returning true for everything causes mismatches.
- Output formatters set the response Content-Type header (e.g., `application/json; charset=utf-8`).

---

### Q13. What is the difference between JSON and XML responses in Web APIs? {#chapter-04-content-negotiation-formatters-q13}

What is the difference between JSON and XML responses in Web APIs?

**Answer:** JSON is the default lightweight text format used by modern Web APIs — compact, JavaScript-native, and supported by System.Text.Json out of the box. XML is verbose, schema-heavy, and required by some legacy enterprise integrations — it needs explicit formatter registration in ASP.NET Core 8.

- JSON: `{"customerName":"Acme","creditLimit":5000}` — default in ASP.NET Core 8.
- XML: `<Customer><CustomerName>Acme</CustomerName></Customer>` — requires `AddXmlSerializerFormatters()`.
- JSON dominates mobile, SPA, and microservice communication.
- XML persists in banking, government, and SOAP-adjacent integrations.
- Supporting both requires registering both formatters and testing content negotiation for each.

---

### Q14. What does `[JsonPropertyName]` do? {#chapter-04-content-negotiation-formatters-q14}

What does `[JsonPropertyName]` do?

**Answer:** `[JsonPropertyName("custom_name")]` overrides the default naming policy for a single property, forcing a specific JSON key during serialization and deserialization. It applies when using System.Text.Json.

- Example: `[JsonPropertyName("customer_name")]` maps C# `CustomerName` to JSON `"customer_name"`.
- Overrides camelCase policy for that property only — creates a mixed naming contract.
- Must be documented in OpenAPI so generated clients use the correct key.
- Useful when integrating with external schemas that mandate specific field names.
- Inconsistent use across DTOs confuses clients and code generators.

---

### Q15. What is `AddJsonOptions` used for? {#chapter-04-content-negotiation-formatters-q15}

What is `AddJsonOptions` used for?

**Answer:** `AddJsonOptions` configures `System.Text.Json` serializer settings globally for all JSON input and output formatters in the application. It is chained on `AddControllers()` in `Program.cs`.

- Set naming policy: `options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase`.
- Enable case-insensitive deserialization: `PropertyNameCaseInsensitive = true`.
- Register custom converters: `options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())`.
- Control reference handling, default ignore conditions, and number handling.
- Changes apply application-wide — all controllers share the same JSON configuration.

---

### Q16. When would you register a custom output formatter? {#chapter-04-content-negotiation-formatters-q16}

When would you register a custom output formatter?

**Answer:** Register a custom output formatter when the API must produce a media type not supported by built-in formatters — CSV exports, PDF reports, Protocol Buffers, or proprietary binary formats. Implement `OutputFormatter`, register it in MVC options, and constrain endpoints with `[Produces]`.

- Implement `CanWriteType` to match only the intended CLR types.
- Override `WriteResponseBodyAsync` to serialize the object to the response stream.
- Register: `builder.Services.AddControllers(o => o.OutputFormatters.Add(new CsvOutputFormatter()))`.
- Pin endpoints with `[Produces("text/csv")]` to prevent formatter conflicts.
- Test with appropriate Accept headers to verify selection and output correctness.

---

### Q17. What is the default response format for ASP.NET Core Web API? {#chapter-04-content-negotiation-formatters-q17}

What is the default response format for ASP.NET Core Web API?

**Answer:** The default response format is JSON (`application/json`) serialized by System.Text.Json with camelCase property naming. No additional configuration is needed in the ASP.NET Core 8 Web API project template.

- All `Ok(dto)`, `CreatedAtAction(...)`, and implicit `ActionResult<T>` returns serialize as JSON.
- Content-Type response header is set to `application/json; charset=utf-8`.
- XML, CSV, and other formats require explicit formatter registration and `[Produces]` attributes.
- Clients omitting the Accept header receive JSON by default.
- The default applies to both minimal APIs and controller-based APIs.

---

### Q18. What is the difference between serialization and model binding? {#chapter-04-content-negotiation-formatters-q18}

What is the difference between serialization and model binding?

**Answer:** Serialization converts a CLR object to an outbound byte stream (response body) using an output formatter; model binding is the inbound process of populating action parameters from the request — route values, query strings, headers, and body deserialization via input formatters.

- Serialization: object → JSON string → HTTP response (after action executes).
- Model binding: HTTP request → deserialized object → action parameter (before action executes).
- Body deserialization is one step within model binding, performed by input formatters.
- Validation runs after model binding completes, before the action method body executes.
- Failures differ: binding failure → 400/415; serialization failure → 500 at result execution time.

---

---

---

## Gotchas — ASP.NET Core Web API (Interview Traps)

#### Gotcha 1. POST returning 200 instead of 201

**Answer:** A successful resource creation with POST should return HTTP 201 Created and tell the client where the new resource lives — returning 200 OK omits that contract and breaks REST clients that rely on status codes and the Location header.

- Use `CreatedAtAction`, `CreatedAtRoute`, or `Created` to return 201 with a Location header pointing at the new resource URL.
- Include the created representation or a minimal payload in the response body when clients need immediate data without a follow-up GET.
- Returning 200 for create operations hides the new resource URL from standard HTTP client libraries and OpenAPI-generated SDKs.

---

#### Gotcha 2. GET that mutates state

**Answer:** GET must be safe and idempotent — performing deletes or updates on GET violates HTTP semantics, breaks caching proxies, and creates security holes when URLs are prefetched, logged, or opened in email clients.

- Browsers, CDNs, and link-preview crawlers may invoke GET URLs without user intent, so side effects run unintentionally.
- Cached GET responses can replay destructive operations or stale mutations across clients.
- Use POST, PUT, PATCH, or DELETE for state changes and keep GET read-only.

---

#### Gotcha 3. `{ success: false }` with HTTP 200

**Answer:** Business failures must map to appropriate 4xx or 5xx status codes — a 200 response with an error flag forces every client to parse the body instead of using standard HTTP semantics, retries, and monitoring.

- Return `ValidationProblemDetails` or `ProblemDetails` with 400 for validation failures and 404, 409, or 422 for domain errors.
- HTTP status codes drive client retry logic, API gateways, and APM alerting; a 200 masks failures in dashboards.
- Envelope patterns like `{ success: false }` require custom handling in every consumer and break OpenAPI contract expectations.

---

#### Gotcha 4. Returning EF entities from API actions

**Answer:** EF Core entities expose navigation properties, shadow fields, and circular references that are not meant for public contracts — serialize DTOs with explicit shapes and never leak database schema to clients.

- Lazy-loaded navigations trigger N+1 queries during serialization and can pull entire object graphs into the response.
- Circular references between entities cause JSON serializer loops or require fragile reference-handling settings.
- DTOs decouple the API contract from schema migrations and let you expose only the fields clients need.

---

#### Gotcha 5. PascalCase JSON with default camelCase policy

**Answer:** ASP.NET Core 8 defaults to camelCase JSON via `System.Text.Json` — PascalCase property names from some clients bind as missing properties, leaving model properties at default values and causing silent data loss on POST and PUT.

- `[JsonPropertyName("PropertyName")]` or a custom `PropertyNamingPolicy` aligns server expectations with legacy client payloads.
- Enable `PropertyNameCaseInsensitive = true` in `AddControllers().AddJsonOptions(...)` when you must accept mixed casing.
- Silent binding failures produce 201/204 success responses with partially saved data and no validation error.

---

#### Gotcha 6. GET with `[FromBody]`

**Answer:** Many HTTP clients, proxies, and caches ignore or strip GET request bodies — filters sent as JSON in GET requests fail silently or never reach the action in ASP.NET Core 8 Web API.

- Model binding for `[FromBody]` on GET is not reliably supported across the HTTP ecosystem.
- Use query strings with `[FromQuery]` for simple filters or POST to a dedicated search endpoint for complex filter objects.
- OpenAPI tools and browser fetch also discourage or block GET bodies, making the pattern fragile in production.

---

#### Gotcha 7. CORS as server security

**Answer:** CORS is enforced by browsers only — it does not stop curl, Postman, server-to-server calls, or direct API requests; authentication and authorization still protect the API.

- CORS headers tell a browser whether JavaScript on one origin may read a cross-origin response; they do not authenticate callers.
- A public API without auth remains fully accessible to any non-browser client regardless of CORS policy.
- Register `AddCors` and `UseCors` for browser SPA access, and enforce JWT, cookies, or API keys separately for real security.

---

#### Gotcha 8. `AllowAnyOrigin` with credentials

**Answer:** Browsers reject `Access-Control-Allow-Origin: *` when the request sends cookies or authorization headers — you must specify explicit origins with `WithOrigins` and call `AllowCredentials`.

- `AllowAnyOrigin()` and `AllowCredentials()` cannot be combined; ASP.NET Core will not emit a valid CORS response for credentialed requests.
- List every trusted frontend origin explicitly, including local dev URLs and production domains.
- Credentialed cross-origin calls require both matching origins and `Access-Control-Allow-Credentials: true`.

---

#### Gotcha 9. Swagger UI exposed in Production

**Answer:** Public Swagger UI discloses the full API surface, schemas, and try-it-out access — gate it behind authentication or disable it outside Development and Staging in ASP.NET Core 8.

- `MapSwagger` and `UseSwaggerUI` in `Program.cs` should be wrapped in environment checks or authorization middleware.
- Exposed OpenAPI documents reveal internal endpoints, field names, and enum values useful for reconnaissance.
- Production APIs typically serve OpenAPI only to authenticated developers or internal tooling, not the public internet.

---

#### Gotcha 10. Missing `[ApiController]` on some controllers

**Answer:** Without `[ApiController]`, automatic 400 `ValidationProblemDetails`, binding source inference, and attribute routing behaviors differ — mixed controllers in the same Web API produce inconsistent error contracts.

- `[ApiController]` enables automatic model-state validation responses and `[FromBody]` inference for complex types.
- Controllers missing the attribute may return 200 with invalid models or require manual `ModelState` checks.
- Apply `[ApiController]` at the controller or assembly level so every endpoint shares the same API conventions.

---

#### Gotcha 11. Blocking on `.Result` in async actions

**Answer:** Blocking on `.Result` or `.Wait()` in async API actions causes thread-pool starvation and deadlocks under load — always `await` async service and database calls in ASP.NET Core 8.

- Sync-over-async ties up request threads while I/O completes, reducing throughput on Kestrel under concurrent load.
- Deadlocks occur when the blocked thread holds a synchronization context the continuation needs to resume.
- Mark controller actions `async Task<IActionResult>` and propagate `await` through the service layer to EF Core and HTTP clients.

---

#### Gotcha 12. Liveness probe includes SQL check

**Answer:** If the liveness probe fails when SQL is down, Kubernetes restarts pods that cannot fix the dependency — put SQL, Redis, and external service checks on readiness only.

- Liveness answers whether the process should be killed and restarted; a down database is not healed by restarting the app.
- Readiness removes the pod from the load balancer until dependencies recover without unnecessary restarts.
- Map `/health/live` to a lightweight self-check and `/health/ready` to `AddDbContextCheck` or custom dependency tags.

---

#### Gotcha 13. N+1 queries in list endpoints

**Answer:** Returning entities with lazy-loaded navigation properties triggers one SQL query per row — use projection with `Select`, explicit `Include`, or DTO mapping to fetch list data in a bounded number of queries.

- Serializing a list of `Order` entities with `Customer` navigation can execute 1 + N queries under default lazy loading.
- Project directly to DTOs in LINQ so EF Core generates a single query with only the columns needed.
- For graphs that must be included, use `Include`/`ThenInclude` or split queries deliberately rather than relying on lazy load during JSON output.

---

#### Gotcha 14. Unstable pagination with Skip/Take

**Answer:** Concurrent inserts and deletes between offset pages cause duplicate or skipped rows — use keyset or cursor pagination ordered by a stable, indexed key for large datasets in Web API list endpoints.

- `Skip((page - 1) * pageSize).Take(pageSize)` shifts the window when rows are added or removed between requests.
- Keyset pagination uses `WHERE id > @lastId ORDER BY id LIMIT @pageSize` with the last seen key from the previous response.
- Offset pagination remains acceptable for small, mostly static tables; expose cursor tokens in link headers or response metadata for high-churn data.

---

#### Gotcha 15. GraphQL N+1 without DataLoader

**Answer:** Field resolvers in HotChocolate or other GraphQL servers that query the database per parent row explode SQL under load — batch related loads with DataLoader or resolve joins at the root query.

- A list of 100 authors each resolving `books` individually executes 101 queries instead of one batched query.
- Register DataLoader services in DI so concurrent field resolutions within a request are grouped into single round-trips.
- Eager-load or project at the root query when the client always requests nested fields together.

---

#### Gotcha 16. gRPC in browser without gRPC-Web

**Answer:** Native gRPC uses HTTP/2 binary framing that browsers do not expose to JavaScript — browser clients need gRPC-Web middleware plus CORS configuration in ASP.NET Core 8.

- Standard `@grpc/grpc-js` in Node or .NET clients works server-to-server; Blazor WASM and SPA browsers require the gRPC-Web protocol.
- Add `AddGrpcWeb()` and `EnableGrpcWeb()` on mapped gRPC services to translate between gRPC-Web and native gRPC.
- Configure CORS for the browser origin alongside gRPC-Web, since cross-origin browser calls still enforce CORS on preflight and response headers.

---

## Gotchas — ASP.NET Core Web API (Interview Traps)

## Gotchas — ASP.NET Core Web API (Interview Traps)

---

## Scenario-Based Questions (Karat Format)

#### Q1. (R) Review serialization for a dual-client API. The JavaScript web app binds correctly; a legacy integration test sends PascalCase JSON and `CustomerName` arrives null.

```csharp
// Program.cs — template defaults only
builder.Services.AddControllers();

public class CreateCustomerRequest
{
    public string CustomerName { get; set; } = "";
    public decimal CreditLimit { get; set; }
}

[HttpPost]
public IActionResult Create([FromBody] CreateCustomerRequest request)
{
    if (string.IsNullOrEmpty(request.CustomerName))
        return BadRequest("CustomerName required");
    return Ok(_svc.Create(request));
}
```

Client body: `{ "CustomerName": "Acme", "CreditLimit": 5000 }`

---

**Answer:**

**Answer:** ASP.NET Core 8 Web API templates default `System.Text.Json` to **camelCase** property names for JSON — incoming PascalCase `CustomerName` does not bind to `CustomerName` unless case-insensitive matching is enabled or clients send camelCase. The web app works; legacy PascalCase clients silently get defaults.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Serialization | Default JSON naming is camelCase (`customerName`) | PascalCase payloads skip property binding |
| Validation | Empty name triggers manual BadRequest | False negatives for valid legacy clients |
| Contract | Undocumented breaking change from Newtonsoft defaults | Migration surprises |
| Testing | Only modern client covered in CI | Legacy partner fails in production |

**Fix (priority order):**

1. Prefer client fix: send `{ "customerName": "Acme", "creditLimit": 5000 }`.
2. Or configure: `AddJsonOptions(o => o.JsonSerializerOptions.PropertyNameCaseInsensitive = true);` during migration.
3. Document breaking naming policy in API changelog — do not rely on silent defaults.

**Production takeaway:** Debrief question **"why CustomerName becomes customerName"** — outbound camelCase; inbound also expects camelCase unless configured otherwise.

---

---

#### Q2. (R) Review this export endpoint. A partner sends `Accept: application/xml` but always receives JSON with HTTP 200.

```csharp
[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    [HttpGet("{id:int}/export")]
    public IActionResult Export(int id)
    {
        var report = _reports.Build(id);
        return Ok(report);
    }
}
```

`Program.cs` calls `AddControllers()` only — no XML formatters registered.

---

**Answer:**

**Answer:** With only `System.Text.Json` input/output formatters registered, the framework cannot satisfy `application/xml` — it falls back to JSON (or default formatter) instead of negotiating XML. Register XML formatters or restrict the endpoint to JSON and return 406 for unsupported Accept types.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Content negotiation | No XML output formatter registered | Accept header ignored |
| Contract | Partner expects XML schema validation | Downstream parsing fails despite HTTP 200 |
| API honesty | Returns JSON without `406` | Client cannot detect mismatch via status |
| Legacy | Enterprise clients often require XML | Integration blocked |

**Fix (priority order):**

1. If XML required: `builder.Services.AddControllers().AddXmlSerializerFormatters();` (or DataContract serializers).
2. If JSON-only: `[Produces("application/json")]` and enable 406 on unsupported Accept via formatter configuration.
3. Add integration test with `Accept: application/xml` asserting content type and body.

**Production takeaway:** **Accept header is a request for a representation** — without a matching formatter, you must fail explicitly or register the formatter.

---

---

#### Q3. (R) Review content negotiation failure handling. QA expects HTTP 406 when an unsupported `Accept` header is sent; API returns JSON 200.

```csharp
[HttpGet("{id:int}")]
[Produces("application/json")]
public IActionResult Get(int id)
{
    var dto = _svc.Get(id);
    return Ok(dto);
}
```

Request: `GET /api/items/1` with header `Accept: application/pdf`

---

**Answer:**

**Answer:** `[Produces("application/json")]` documents intent but does not alone return 406 — default formatter selection may still write JSON when no formatter matches the Accept header unless `ReturnHttpNotAcceptable` is enabled or no compatible formatter exists and fallback is disabled.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| HTTP semantics | Unsupported Accept should yield `406 Not Acceptable` | Clients assume PDF contract incorrectly |
| Configuration | Default may fall back to first formatter | Silent wrong content type |
| Testing | QA scenario valid for strict APIs | Compliance failures |
| Documentation | OpenAPI lists only JSON — PDF not advertised | Still should not return fake PDF |

**Fix (priority order):**

1. Configure MVC: `options.ReturnHttpNotAcceptable = true;` on formatter options (or ensure no fallback formatter handles pdf).
2. Keep `[Produces("application/json")]` accurate — remove unsupported types from `[Produces]`.
3. Return `StatusCode(406)` explicitly for known unsupported types if custom logic required.

**Production takeaway:** **406 is the honest response** when you cannot produce any acceptable representation — better than wrong-format 200.

---

---

#### Q4. (P) Explain how `System.Text.Json` camelCase naming is configured in ASP.NET Core 8 Web APIs, and why `[JsonPropertyName("customer_name")]` on one property affects the whole contract story.

---

**Answer:**

**Answer:** Web SDK sets camelCase via `JsonNamingPolicy.CamelCase` in `AddJsonOptions`; individual `[JsonPropertyName]` overrides create exceptions that clients must special-case — fine for external schema locks, dangerous when sprinkled inconsistently.

- **Default:** `builder.Services.AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);` — often already applied by template.
- **Outbound:** `CustomerName` serializes as `customerName` in JSON responses.
- **Inbound:** Deserialization expects camelCase keys matching policy unless `PropertyNameCaseInsensitive = true`.
- **`[JsonPropertyName("customer_name")]`:** Forces snake_case for one property — document in OpenAPI; mixed policies confuse code generators.
- **Newtonsoft:** If `AddNewtonsoftJson()` used, separate camelCase settings apply — do not mix serializers on same app without team agreement.

**Production takeaway:** Naming policy is a **published contract** — debrief camelCase question is about defaults plus migration, not memorizing attribute names.

---

---

#### Q5. (R) Review custom formatter registration. CSV downloads work locally but return empty bodies in staging — logs show formatter selected but model type mismatch.

```csharp
// Program.cs
builder.Services.AddControllers(options =>
{
    options.OutputFormatters.Add(new CsvOutputFormatter());
});

public class CsvOutputFormatter : TextOutputFormatter
{
    public CsvOutputFormatter()
    {
        SupportedMediaTypes.Add("text/csv");
        SupportedEncodings.Add(Encoding.UTF8);
    }

    protected override bool CanWriteType(Type? type) => true;

    public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding encoding)
    {
        var rows = (IEnumerable<OrderDto>)context.Object!;
        await context.HttpContext.Response.WriteAsync(ToCsv(rows));
    }
}

[HttpGet("export")]
[Produces("text/csv")]
public IActionResult Export() => Ok(_orders.All());
```

---

**Answer:**

**Answer:** `CanWriteType` returning `true` for everything hides that `context.Object` is not `IEnumerable<OrderDto>` — when the action returns `OkObjectResult` wrapping a different type, the invalid cast throws or writes empty output. Formatters must declare supported types and handle `ObjectResult` value types correctly.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Formatter | `CanWriteType => true` too broad | Wrong formatter selected for non-CSV responses |
| Runtime | Unsafe cast `(IEnumerable<OrderDto>)context.Object!` | InvalidCastException or empty body in staging data edge cases |
| Design | Formatter assumes collection; action may return wrapper | Local smoke test with full dataset missed null path |
| Content negotiation | CSV formatter may run for JSON Accept | Corrupt JSON endpoints |

**Fix (priority order):**

1. `CanWriteType` return `typeof(IEnumerable<OrderDto>).IsAssignableFrom(type);`
2. In `WriteResponseBodyAsync`, pattern-match `context.Object` and handle null/empty safely.
3. Pin endpoint with `[Produces("text/csv")]` and verify Accept header in tests.

**Production takeaway:** Custom formatters must be **type-safe and narrow** — `CanWriteType true` is a production incident waiting to happen.

---

---

#### Q6. (M) A bank middleware still requires SOAP/XML for one endpoint while the rest of the platform is JSON. Where do input and output formatters sit in the pipeline relative to model binding, and what does `[Consumes("application/xml")]` change?

---

**Answer:**

**Answer:** Input formatters deserialize the request body into action parameters before the action runs — model binding selects an input formatter based on Content-Type; output formatters run during result execution based on Accept and `[Produces]`. `[Consumes("application/xml")]` restricts which actions match XML Content-Type on input.

- **Input path:** Request → routing → model binding chooses input formatter by `Content-Type` → parameter object constructed → action executes.
- **Output path:** Action returns `IActionResult` → result executor asks output formatters (JSON, XML, CSV) using content negotiation.
- **`[Consumes("application/xml")]`:** Action selection filter — JSON POST to that action may return 415 Unsupported Media Type.
- **Legacy XML:** Register `AddXmlSerializerFormatters()` or custom Xml input formatter for that controller only.
- **Isolation:** Consider separate minimal controller or YARP route to XML adapter — do not force global XML for one client.

**Production takeaway:** Formatters are **pluggable serialization layers** at binding/result time — not middleware in the classic sense, but part of the MVC filter pipeline around the action.

---

---

#### Q7. (D) Leadership wants to drop XML support to reduce maintenance. One state-government client still posts `application/xml` to `POST /api/permits`. How do you decide retire vs adapter vs gateway translation?

---

**Answer:**

**Answer:** Weigh client count, contract SLA, and translation cost — often an edge XML-to-JSON gateway or dedicated adapter service preserves the modern JSON core while honoring a single legacy consumer until contractual sunset.

- **Retire XML:** Acceptable when contract allows, client can migrate, and timeline is agreed — return `415` with migration guide.
- **Adapter service:** Small sidecar converts XML ↔ DTO ↔ JSON — keeps main API JSON-only and testable.
- **Gateway translation:** APIM/nginx script transforms payload — ops owns schema mapping; watch latency and error surfaces.
- **In-app XML:** `AddXmlSerializerFormatters` on one controller — lowest infra moving parts but spreads legacy into codebase.
- **Decision inputs:** Revenue/regulatory risk, test coverage for XML schemas, team skill, deprecation clause in partner agreement.

**Production takeaway:** Production APIs **sunset representations deliberately** — not by deleting formatters on a Friday without a migration path.

---

---

#### Q8. (R) Review `[Produces]` and `[ProducesResponseType]` usage. Swagger advertises XML and JSON responses; production returns JSON only and clients cache wrong content type.

```csharp
[ApiController]
[Route("api/catalog")]
[Produces("application/json", "application/xml")]
public class CatalogController : ControllerBase
{
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    public IActionResult Get(int id) => Ok(_catalog.Get(id));
}
```

No XML serializer configured; `AddControllers()` without `AddXmlSerializerFormatters()`.

**Answer:**

**Answer:** Declaring `[Produces("application/json", "application/xml")]` without registering XML formatters misdocuments the API — Swagger shows XML clients cannot actually receive, and caches may store JSON under Accept XML expectations.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Documentation | `[Produces("application/xml")]` without formatter | False OpenAPI contract |
| Runtime | Always JSON body | Client XML parsers fail |
| Caching | `Vary: Accept` may be missing | Shared cache serves wrong representation |
| Compliance | Advertised formats must be honored or removed | Audit finding |

**Fix (priority order):**

1. Remove `application/xml` from `[Produces]` unless formatters registered and tested.
2. If XML needed, add `AddXmlSerializerFormatters()` and integration tests for both types.
3. Add `Response.Headers.Vary` for Accept when multiple representations are real.

**Production takeaway:** **`[Produces]` must match registered formatters** — OpenAPI lies become partner outages and cache poisoning.

---
