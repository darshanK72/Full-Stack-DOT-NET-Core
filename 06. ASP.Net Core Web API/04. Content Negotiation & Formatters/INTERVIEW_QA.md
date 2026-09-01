# Content Negotiation & Formatters — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is content negotiation in ASP.NET Core Web API?](#q1-what-is-content-negotiation-in-aspnet-core-web-api)
2. [Q2. What is the Accept HTTP header used for?](#q2-what-is-the-accept-http-header-used-for)
3. [Q3. What is the Content-Type header used for in API requests?](#q3-what-is-the-content-type-header-used-for-in-api-requests)
4. [Q4. What is the default JSON serializer in ASP.NET Core 8?](#q4-what-is-the-default-json-serializer-in-aspnet-core-8)
5. [Q5. What is camelCase JSON naming and why is it used in Web APIs?](#q5-what-is-camelcase-json-naming-and-why-is-it-used-in-web-apis)
6. [Q6. What is the difference between input formatters and output formatters?](#q6-what-is-the-difference-between-input-formatters-and-output-formatters)
7. [Q7. What HTTP status code is returned when content negotiation fails?](#q7-what-http-status-code-is-returned-when-content-negotiation-fails)
8. [Q8. What does `[Produces("application/json")]` do?](#q8-what-does-producesapplicationjson-do)
9. [Q9. What does `[Consumes("application/xml")]` do?](#q9-what-does-consumesapplicationxml-do)
10. [Q10. What is the difference between `System.Text.Json` and Newtonsoft.Json in ASP.NET Core?](#q10-what-is-the-difference-between-systemtextjson-and-newtonsoftjson-in-aspnet-core)
11. [Q11. How does model binding relate to input formatters?](#q11-how-does-model-binding-relate-to-input-formatters)
12. [Q12. What is an output formatter?](#q12-what-is-an-output-formatter)
13. [Q13. What is the difference between JSON and XML responses in Web APIs?](#q13-what-is-the-difference-between-json-and-xml-responses-in-web-apis)
14. [Q14. What does `[JsonPropertyName]` do?](#q14-what-does-jsonpropertyname-do)
15. [Q15. What is `AddJsonOptions` used for?](#q15-what-is-addjsonoptions-used-for)
16. [Q16. When would you register a custom output formatter?](#q16-when-would-you-register-a-custom-output-formatter)
17. [Q17. What is the default response format for ASP.NET Core Web API?](#q17-what-is-the-default-response-format-for-aspnet-core-web-api)
18. [Q18. What is the difference between serialization and model binding?](#q18-what-is-the-difference-between-serialization-and-model-binding)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What is content negotiation in ASP.NET Core Web API?

**Concepts**
- Accept header — client media type preference
- Output formatter pipeline selection
- 406 Not Acceptable — negotiation failure
- `[Produces]` — action-level format constraint
- Input formatter selection via Content-Type

**Answer**

Content negotiation is the mechanism by which the server selects a response format based on the client's `Accept` header and the output formatters registered in the application. When a client sends `Accept: application/json`, the framework walks the registered output formatters looking for one that can produce that media type, so the server only serializes in formats it has been explicitly configured to handle. If no formatter matches and fallback is disabled, the server returns 406 Not Acceptable rather than silently defaulting to JSON. The input side works the same way in reverse — the `Content-Type` header on the request body selects the input formatter for deserialization. ASP.NET Core 8 defaults to JSON only; XML, CSV, and other formats require explicit formatter registration.

---

## Q2. What is the Accept HTTP header used for?

**Concepts**
- Accept header — client response format preference
- Quality values (`q=`) — preference ordering
- Output formatter selection at content negotiation
- 406 Not Acceptable — unsatisfied Accept
- `*/*` — wildcard Accept from browsers

**Answer**

The Accept header tells the server which response media types the client prefers, ordered by quality values such as `Accept: application/json, application/xml;q=0.9`. The framework picks the highest-preference type it can produce with a registered output formatter, so if the server has only a JSON formatter and the client lists XML first at full quality, the server either falls back to JSON or returns 406 depending on configuration. Accept governs the response format only — it says nothing about the request body format, which is the job of Content-Type. API clients should set Accept explicitly because browsers default to `*/*`, which accepts any format.

---

## Q3. What is the Content-Type header used for in API requests?

**Concepts**
- Content-Type — request body media type declaration
- Input formatter selection
- 415 Unsupported Media Type — mismatched Content-Type
- `[Consumes]` — action-level input type constraint
- `multipart/form-data` for file uploads

**Answer**

The Content-Type header declares the media type of the request body so the server can select the correct input formatter for deserialization. It is required on POST, PUT, and PATCH requests that include a body — `Content-Type: application/json` routes the body to the `System.Text.Json` input formatter, while `Content-Type: application/xml` requires a registered XML formatter. A mismatch between the Content-Type value and the actual body format causes model binding failures or a 415 Unsupported Media Type response. Applying `[Consumes("application/json")]` on an action rejects requests with other Content-Type values before binding even starts, which is useful for isolating endpoints that must only accept a specific format.

---

## Q4. What is the default JSON serializer in ASP.NET Core 8?

**Concepts**
- `System.Text.Json` — default serializer since ASP.NET Core 3+
- `AddControllers()` — registers JSON formatters automatically
- `AddJsonOptions()` — global serializer configuration
- camelCase naming policy — default in Web API templates
- `AddNewtonsoftJson()` — opt-in replacement

**Answer**

ASP.NET Core 8 uses `System.Text.Json` as the default JSON serializer for both input and output formatters, registered automatically when calling `AddControllers()`. It is configured via `AddJsonOptions()` which exposes `JsonSerializerOptions`, and the default naming policy applies camelCase to property names in all JSON responses. The reason Microsoft moved to `System.Text.Json` is performance — it is faster and allocates less memory than Newtonsoft.Json for typical API payloads since it is built on `Span<T>` and avoids boxing. I would add `AddNewtonsoftJson()` only when a project genuinely needs legacy features like `$type`-based polymorphism or specific custom converters that have no equivalent in `System.Text.Json`.

---

## Q5. What is camelCase JSON naming and why is it used in Web APIs?

**Concepts**
- `JsonNamingPolicy.CamelCase` — default naming policy
- PascalCase C# properties vs camelCase JSON keys
- `PropertyNameCaseInsensitive` — deserialization matching
- `[JsonPropertyName]` — per-property override
- OpenAPI schema naming alignment

**Answer**

camelCase naming serializes C# PascalCase property names — `CustomerName` — to lowercase-first JSON keys — `"customerName"` — which matches JavaScript and front-end conventions. ASP.NET Core 8 applies this automatically via `JsonNamingPolicy.CamelCase` so the React or Angular client can read `response.customerName` without manual mapping. The inbound side is case-sensitive by default, which means a legacy client sending `"CustomerName"` in the JSON body gets a null binding unless `PropertyNameCaseInsensitive = true` is configured in `AddJsonOptions`. OpenAPI/Swagger schemas reflect the camelCase keys, so generated TypeScript or C# clients also use camelCase — consistency all the way through avoids silent data loss at runtime.

---

## Q6. What is the difference between input formatters and output formatters?

**Concepts**
- Input formatters — request body deserialization
- Output formatters — response body serialization
- Content-Type selects input formatter
- Accept header selects output formatter
- `[FromBody]` triggers input formatter path

**Answer**

Input formatters deserialize the request body into action parameters before the action runs, selected based on the request's Content-Type header. Output formatters serialize the action result into the response body after the action executes, selected based on the Accept header and any `[Produces]` constraint. The two sets operate independently — an action can consume JSON via an input formatter and produce CSV via a custom output formatter. Custom formatters implement `InputFormatter` or `OutputFormatter` base classes respectively and are registered in `AddControllers(options => ...)`. A binding failure on the input side returns 400 or 415 before the action runs; a serialization failure on the output side produces a 500 at result execution time.

---

## Q7. What HTTP status code is returned when content negotiation fails?

**Concepts**
- 406 Not Acceptable — unsatisfied Accept header
- 415 Unsupported Media Type — unsupported Content-Type
- `ReturnHttpNotAcceptable` — opt-in strict behavior
- Default JSON fallback — framework default without configuration
- `[Produces]` — documents but does not alone enforce 406

**Answer**

HTTP 406 Not Acceptable is the correct response when the server cannot produce any representation matching the client's Accept header and no fallback formatter is configured. Without explicit configuration, the framework may silently fall back to the default JSON formatter rather than returning 406, which violates HTTP semantics and hides mismatches from clients. I would enable strict behavior via `options.ReturnHttpNotAcceptable = true` in the MVC options so clients reliably detect when they have requested an unsupported format. The input-side equivalent is 415 Unsupported Media Type, returned when the request Content-Type does not match any registered input formatter. Integration tests should assert 406 for unsupported Accept headers on strict APIs since the silent fallback is a common source of contract confusion.

---

## Q8. What does `[Produces("application/json")]` do?

**Concepts**
- `[Produces]` — metadata attribute for OpenAPI and formatter selection
- Output formatter restriction per action
- OpenAPI documentation accuracy
- Mismatch between `[Produces]` and registered formatters
- Does not alone enforce 406

**Answer**

`[Produces]` declares which content types an action can return, influencing both content negotiation and OpenAPI documentation. When only `"application/json"` is listed, the formatter selection pipeline restricts itself to the JSON output formatter for that action, so a CSV formatter registered globally will not accidentally handle that endpoint. The attribute is also metadata — Swashbuckle reads it to populate the response content type in the generated OpenAPI document, which affects the TypeScript clients generated from it. Declaring a type without a corresponding registered formatter misdocuments the API since `[Produces]` does not alone cause 406 — formatter configuration controls whether a fallback is used. I use it primarily to make the OpenAPI contract accurate and to prevent format bleed between endpoints.

---

## Q9. What does `[Consumes("application/xml")]` do?

**Concepts**
- `[Consumes]` — action selection constraint on Content-Type
- 415 Unsupported Media Type — non-matching request
- Input formatter requirement for consumed type
- Isolating legacy XML endpoints from JSON endpoints

**Answer**

`[Consumes]` acts as an action selection constraint — when a request arrives, routing checks whether the request's Content-Type matches any value listed in `[Consumes]` before selecting that action. A request with `Content-Type: application/json` sent to an action marked `[Consumes("application/xml")]` receives 415 Unsupported Media Type rather than reaching the action method. This is useful for maintaining legacy XML endpoints in an otherwise JSON-only API, since the constraint isolates which requests each action handles. The constraint only gates selection — the action still needs a registered XML input formatter to actually deserialize the body, so declaring `[Consumes("application/xml")]` without `AddXmlSerializerFormatters()` will select the action but fail during binding.

---

## Q10. What is the difference between `System.Text.Json` and Newtonsoft.Json in ASP.NET Core?

**Concepts**
- `System.Text.Json` — built-in, high-performance, default
- Newtonsoft.Json — feature-rich, third-party, optional via `AddNewtonsoftJson()`
- Configuration surface — `AddJsonOptions` vs `AddNewtonsoftJson`
- Reference loop handling and `$type` polymorphism — Newtonsoft-only features
- Mixing serializers in one app — inconsistency risk

**Answer**

`System.Text.Json` is the built-in serializer that ships with ASP.NET Core — faster, lower allocation, and strictly standards-compliant with camelCase defaults. Newtonsoft.Json is an opt-in replacement via `AddNewtonsoftJson()` that offers richer features: reference loop handling with `ReferenceLoopHandling.Ignore`, `$type`-based polymorphic deserialization, and a larger ecosystem of community converters. The configuration surfaces are completely separate — `System.Text.Json` uses `AddJsonOptions(o => o.JsonSerializerOptions...)` while Newtonsoft uses `AddNewtonsoftJson(o => o.SerializerSettings...)`, so I would never mix them in the same application since the naming and behavior differences create inconsistent contracts across endpoints. I default to `System.Text.Json` and only reach for Newtonsoft when a specific legacy feature has no equivalent.

---

## Q11. How does model binding relate to input formatters?

**Concepts**
- Model binding — orchestration layer for parameter population
- Input formatters — body deserialization component
- `[FromBody]` — triggers formatter-based binding
- Content-Type — formatter selection key
- Validation runs after binding

**Answer**

Model binding is the orchestration layer that decides where to read each action parameter from — route, query string, header, form, or body. For body parameters marked `[FromBody]`, model binding delegates to input formatters to deserialize the raw request stream into a CLR object, using the request's Content-Type header to select the right formatter. Route and query parameters bind directly without formatters, which is why a simple `int id` from a route template never touches `System.Text.Json`. If no input formatter matches the Content-Type, binding fails with 415 or leaves the model empty. Validation via DataAnnotations and `IValidatableObject` runs after binding completes, so a formatter that returns a syntactically valid but semantically wrong object will pass the formatter phase and only fail at the validation phase.

---

## Q12. What is an output formatter?

**Concepts**
- Output formatter — response body serialization component
- `CanWriteType` — CLR type eligibility check
- `WriteResponseBodyAsync` — response stream writer
- Custom formatter registration in MVC options
- Response Content-Type header set by formatter

**Answer**

An output formatter is the component responsible for serializing an action result object into the HTTP response body in a specific media type. The framework selects an output formatter during result execution by asking each registered formatter's `CanWriteType` whether it handles the result's CLR type, then matches the surviving candidates against the negotiated Accept header. The built-in `JsonOutputFormatter` writes JSON via `System.Text.Json`, and there are built-in formatters for XML and plain strings. Custom formatters extend `TextOutputFormatter` or `OutputFormatter`, override `CanWriteType` to declare which types they handle, and implement `WriteResponseBodyAsync` to write the serialized bytes to the response stream. The formatter is also responsible for setting the response `Content-Type` header — for example, `text/csv; charset=utf-8`.

---

## Q13. What is the difference between JSON and XML responses in Web APIs?

**Concepts**
- JSON — default, compact, JavaScript-native
- XML — verbose, schema-heavy, requires explicit registration
- `AddXmlSerializerFormatters()` — opt-in XML support
- Content negotiation — format selection at runtime
- Enterprise and legacy integration contexts for XML

**Answer**

JSON is the default lightweight text format for modern Web APIs — compact, natively parsed by JavaScript, and supported out of the box by `System.Text.Json` in ASP.NET Core 8. XML is the older, more verbose alternative that requires explicit registration via `AddXmlSerializerFormatters()` because it is not enabled by default. The verbosity of XML (`<CustomerName>Acme</CustomerName>` vs `"customerName":"Acme"`) makes it slower to parse and larger over the wire, which is why it has been largely displaced by JSON in mobile, SPA, and microservice communication. XML still persists in banking, government, and SOAP-adjacent integrations where schemas and namespaces provide contract guarantees. When both are needed I register both formatters and rely on content negotiation so clients declare which format they want via the Accept header.

---

## Q14. What does `[JsonPropertyName]` do?

**Concepts**
- `[JsonPropertyName]` — per-property JSON key override
- Overrides `JsonNamingPolicy` for a single property
- Mixed naming policy — documentation and client impact
- `System.Text.Json`-specific attribute
- OpenAPI schema alignment requirement

**Answer**

`[JsonPropertyName("custom_name")]` overrides the naming policy for a single property, forcing a specific JSON key for both serialization and deserialization regardless of the global `JsonNamingPolicy`. This is useful when integrating with an external schema that mandates specific field names — for example, a payment gateway that requires `"merchant_id"` rather than the camelCase default `"merchantId"`. The downside is that it creates a mixed naming contract: most properties follow camelCase while one uses snake_case, which means the OpenAPI schema must accurately document the exception so generated clients use the correct key. I use it sparingly because inconsistent naming across DTOs confuses code generators and developers who assume one policy applies everywhere.

---

## Q15. What is `AddJsonOptions` used for?

**Concepts**
- `AddJsonOptions` — global `System.Text.Json` configuration
- `JsonSerializerOptions` — serializer behavior settings
- `PropertyNamingPolicy` — naming convention
- `PropertyNameCaseInsensitive` — inbound matching
- Custom converters — `JsonStringEnumConverter` and others

**Answer**

`AddJsonOptions` configures `System.Text.Json` serializer settings globally for all JSON input and output formatters in the application, chained on `AddControllers()` in `Program.cs`. The most common uses are setting the naming policy to `JsonNamingPolicy.CamelCase`, enabling `PropertyNameCaseInsensitive = true` for legacy client compatibility, and registering custom converters such as `JsonStringEnumConverter` to serialize enums as strings rather than integers. Every controller in the application shares the same configuration, which is why I treat it as an application-wide contract decision rather than a per-endpoint tuning knob. Changes here affect inbound deserialization and outbound serialization simultaneously, so I test both directions when I modify these settings.

---

## Q16. When would you register a custom output formatter?

**Concepts**
- Custom output formatter — media type not supported by built-ins
- `OutputFormatter` base class
- `CanWriteType` — CLR type eligibility declaration
- `WriteResponseBodyAsync` — response stream serialization
- `[Produces]` — endpoint pinning to custom media type

**Answer**

I register a custom output formatter when the API must produce a media type that the built-in formatters do not support — CSV exports, PDF reports, Protocol Buffers, or proprietary binary formats. The formatter extends `TextOutputFormatter` or `OutputFormatter`, overrides `CanWriteType` to match only the intended CLR types such as `IEnumerable<ReportRow>`, and implements `WriteResponseBodyAsync` to serialize the object to the response stream. Registration goes in `AddControllers(o => o.OutputFormatters.Add(new CsvOutputFormatter()))` and the endpoint should be pinned with `[Produces("text/csv")]` so the formatter only activates when explicitly negotiated. The `CanWriteType` implementation must be narrow — returning `true` for all types is a common mistake that causes the formatter to intercept responses it cannot handle correctly.

---

## Q17. What is the default response format for ASP.NET Core Web API?

**Concepts**
- JSON default — `application/json` without configuration
- `System.Text.Json` — default serializer
- camelCase naming policy — default property naming
- `Ok(dto)` and `ActionResult<T>` — implicit JSON serialization
- Additional formats require explicit registration

**Answer**

The default response format for ASP.NET Core 8 Web API is JSON serialized by `System.Text.Json` with camelCase property naming, requiring no additional configuration beyond the standard `AddControllers()` call in the project template. Every `Ok(dto)`, `CreatedAtAction(...)`, and implicit `ActionResult<T>` return value is serialized to JSON and the response Content-Type header is set to `application/json; charset=utf-8`. Clients that omit the Accept header receive JSON since it is the first registered output formatter. XML, CSV, and any other formats must be explicitly opted into by registering additional formatters — the framework does not guess at alternative representations. This applies to both controller-based APIs and minimal APIs.

---

## Q18. What is the difference between serialization and model binding?

**Concepts**
- Serialization — CLR object to response body (outbound)
- Model binding — request data to CLR object (inbound)
- Input formatters — body deserialization within binding
- Validation — runs after binding, before action executes
- Failure modes — 400/415 for binding vs 500 for serialization

**Answer**

Serialization is an outbound operation — it converts a CLR object to a byte stream written to the HTTP response body via an output formatter after the action executes. Model binding is an inbound operation — it populates action parameters from the HTTP request before the action runs, reading from route values, query strings, headers, and the request body. Body deserialization is one step within model binding, performed by input formatters when the parameter is marked `[FromBody]`. Validation runs after model binding completes and before the action method body executes. The failure modes are distinct: a binding or deserialization failure returns 400 Bad Request or 415 Unsupported Media Type before the action runs, while a serialization failure at result execution time produces a 500 because the action has already completed.

---

## Gotchas — ASP.NET Core Web API (Interview Traps)

---

#### Gotcha 1. POST returning 200 instead of 201

**Concepts**
- HTTP 201 Created — correct status for resource creation
- Location header — URI of the new resource
- `CreatedAtAction` / `CreatedAtRoute` — helpers that set status and header
- REST contract — status codes as semantic communication

**Answer**

A successful POST that creates a resource must return 201 Created with a Location header pointing at the new resource's URI, not 200 OK. Returning 200 omits the resource location from the response contract, so HTTP client libraries and OpenAPI-generated SDKs that rely on the Location header to navigate to the created resource will silently miss it. I use `CreatedAtAction(nameof(Get), new { id = newEntity.Id }, newEntity)` rather than `Ok(newEntity)` because it sets both the correct status code and the Location header in one call. Including the created representation in the body is also useful so callers do not need an immediate follow-up GET.

---

#### Gotcha 2. GET that mutates state

**Concepts**
- HTTP GET — safe and idempotent by specification
- Browser prefetch and CDN cache replay of GET URLs
- Side effects on safe methods — security and caching violations
- Correct verbs — POST/PUT/PATCH/DELETE for mutations

**Answer**

GET must be safe and idempotent by HTTP specification, which means calling it any number of times must have no side effects. Performing deletes or updates in a GET action violates this contract in ways that cause real production issues — browsers prefetch URLs in link previews, CDNs cache and replay GET responses, and link crawlers follow URLs without user intent, so a side-effecting GET runs its mutation uncontrollably. I use POST for creates, PUT or PATCH for updates, and DELETE for deletions, keeping GET strictly read-only so the caching and safety semantics of the HTTP layer remain reliable.

---

#### Gotcha 3. `{ success: false }` with HTTP 200

**Concepts**
- HTTP status codes — semantic failure signaling
- `ProblemDetails` / `ValidationProblemDetails` — RFC 7807 error bodies
- 200 masking failures — APM and gateway blindness
- Client retry logic — driven by status codes not body flags

**Answer**

Returning HTTP 200 with `{ "success": false }` in the body forces every consumer to parse the response body to detect failure rather than using standard HTTP status code semantics. API gateways, APM tools, and retry logic in HTTP clients all key on status codes — a 200 registers as success in dashboards even when the business operation failed, which makes incidents invisible until a human reads logs. I return `ValidationProblemDetails` with 400 for validation failures, 404 for missing resources, 409 for conflicts, and 422 for domain rule violations, so the HTTP layer carries the failure signal and clients can handle errors without special-casing the body format.

---

#### Gotcha 4. Returning EF entities from API actions

**Concepts**
- EF Core navigation properties — lazy-load triggers during serialization
- Circular references — serializer loop risk
- Schema leakage — internal fields exposed to clients
- DTOs — explicit public contract shape
- N+1 query risk — navigation traversal under serialization

**Answer**

EF Core entities are not API contracts — they mirror the database schema including internal columns, shadow properties, and navigation properties that are not meant for clients. Serializing them directly causes lazy-loaded navigation properties to trigger additional SQL queries during the JSON write, potentially executing one query per row in a list response. Circular references between entities cause `System.Text.Json` to throw unless reference handling is configured, which is a fragile fix for a problem that should not exist. I always map entities to response DTOs before returning from an action, which decouples the public API shape from database schema changes, exposes only approved fields, and eliminates the lazy-load and circular reference risks.

---

#### Gotcha 5. PascalCase JSON with default camelCase policy

**Concepts**
- `System.Text.Json` camelCase default
- Silent binding failure — PascalCase keys map to null
- `PropertyNameCaseInsensitive` — migration compatibility setting
- `[JsonPropertyName]` — per-property key override

**Answer**

ASP.NET Core 8 defaults to camelCase JSON serialization via `System.Text.Json`, which means a legacy client sending `{ "CustomerName": "Acme", "CreditLimit": 5000 }` with PascalCase keys will have those properties bind as empty string and zero rather than the intended values. This produces a 201 or 204 success response with partially saved data and no validation error, which makes the defect invisible in logs. The fix depends on who owns the contract: ideally the client adopts camelCase, but during migration I enable `PropertyNameCaseInsensitive = true` in `AddJsonOptions` so the server accepts either casing. I treat the naming policy as a published contract decision — changing it after clients have integrated is a breaking change.

---

#### Gotcha 6. GET with `[FromBody]`

**Concepts**
- GET body — stripped by clients, proxies, and CDNs
- `[FromBody]` on GET — unreliable across the HTTP ecosystem
- `[FromQuery]` — correct source for GET filters
- `POST /search` pattern — for complex filter objects

**Answer**

The HTTP specification does not forbid a body on GET, but nearly every practical component in the stack — browsers, fetch API, many HTTP client libraries, CDNs, and API gateways — either ignores or strips GET request bodies. ASP.NET Core 8 does not reliably bind `[FromBody]` on GET actions, so filter objects sent as JSON in a GET body fail silently with null models. I use `[FromQuery]` for simple filter parameters on GET endpoints, and when the filter object is genuinely too complex for a query string I introduce a `POST /search` endpoint with `[FromBody]`, which is a well-understood pattern that all HTTP clients handle correctly.

---

#### Gotcha 7. CORS as server security

**Concepts**
- CORS — browser-only enforcement mechanism
- Non-browser clients — unaffected by CORS
- Authentication and authorization — real API security
- Same-Origin Policy — the browser rule CORS relaxes

**Answer**

CORS is a browser-enforced policy that controls whether JavaScript on one origin can read responses from another origin — it is not a server security mechanism. Curl, Postman, server-to-server HTTP clients, and any malicious script running outside a browser context are completely unaffected by CORS headers. I configure `AddCors` and `UseCors` to give browser SPA clients cross-origin access, but that is entirely separate from protecting the API itself — JWT authentication, cookie auth, or API key validation are what actually prevent unauthorized access regardless of the client type.

---

#### Gotcha 8. `AllowAnyOrigin` with credentials

**Concepts**
- `AllowAnyOrigin()` — sets `Access-Control-Allow-Origin: *`
- `AllowCredentials()` — requires specific origin, not wildcard
- CORS specification — forbids wildcard + credentials combination
- `WithOrigins` — explicit origin allowlist for credentialed requests

**Answer**

The CORS specification forbids combining `Access-Control-Allow-Origin: *` with `Access-Control-Allow-Credentials: true` because that combination would allow any website to make credentialed requests and read authenticated responses on behalf of the user. Browsers reject this combination and ASP.NET Core will throw or emit an invalid CORS response when both are configured. When the SPA sends cookies or an Authorization header with `credentials: 'include'`, I must use `WithOrigins("https://app.example.com")` to list each allowed origin explicitly, then chain `AllowCredentials()`. I load the allowed origins from environment-specific configuration so the local development URL and production domain are separate entries rather than hardcoded.

---

#### Gotcha 9. Swagger UI exposed in Production

**Concepts**
- Swagger UI in production — API surface reconnaissance risk
- Environment check — `IsDevelopment()` gate
- OpenAPI document — reveals endpoints, schemas, and enum values
- Authorization middleware — alternative protection for internal portals

**Answer**

Swagger UI in production exposes every endpoint, parameter, schema, and authentication scheme to anyone who can reach the URL, which is useful for reconnaissance before a targeted attack. I wrap `UseSwagger()` and `UseSwaggerUI()` in an `if (app.Environment.IsDevelopment())` block so they never activate in staging or production deployments. When internal developers need access to the OpenAPI document in production I serve it through an IP-restricted reverse proxy path or behind an authenticated portal rather than leaving it publicly accessible. The OpenAPI JSON document should also be protected since it reveals internal field names and enum values even without the interactive UI.

---

#### Gotcha 10. Missing `[ApiController]` on some controllers

**Concepts**
- `[ApiController]` — enables automatic model validation, binding inference, attribute routing
- `ValidationProblemDetails` — automatic 400 response body
- Binding source inference — `[FromBody]` for complex types
- Inconsistent error contracts — mixed controller setup

**Answer**

`[ApiController]` enables three behaviors that Web API controllers rely on: automatic 400 `ValidationProblemDetails` responses when `ModelState` is invalid, binding source inference that applies `[FromBody]` to complex types without explicit attributes, and strict attribute routing requirements. A controller missing the attribute returns 200 with an invalid model unless the action manually checks `ModelState.IsValid`, which means a typo or missing `[Required]` field produces a success response with wrong data. Mixed controllers in the same Web API produce inconsistent error contracts that are difficult for clients and test suites to handle uniformly. I apply `[ApiController]` at the assembly level in `Program.cs` to ensure every controller in the project picks it up.

---

#### Gotcha 11. Blocking on `.Result` in async actions

**Concepts**
- `.Result` / `.Wait()` — sync-over-async blocking
- Thread-pool starvation — reduced throughput under load
- Deadlock risk — synchronization context blocking
- `async Task<IActionResult>` — correct action signature

**Answer**

Blocking on `.Result` or `.Wait()` on a `Task` inside an async controller action ties up a thread-pool thread while the I/O operation completes, reducing the number of concurrent requests Kestrel can handle since each blocked thread is unavailable for new requests. Under load this creates a thread starvation spiral where the pool is exhausted waiting for completions that are themselves queued. There is also a deadlock risk when the blocked thread holds a synchronization context that the continuation needs to resume on. I mark controller actions `async Task<IActionResult>` and propagate `await` all the way through the service layer to EF Core queries and `HttpClient` calls, so threads are released to the pool during every I/O wait.

---

#### Gotcha 12. Liveness probe includes SQL check

**Concepts**
- Liveness probe — signals Kubernetes to restart the pod
- Readiness probe — removes pod from load balancer until dependencies recover
- SQL down — dependency failure, not pod failure
- `/health/live` vs `/health/ready` — separate endpoints with different checks

**Answer**

A liveness probe answers whether the process itself should be killed and restarted — a failed liveness probe causes Kubernetes to terminate and recreate the pod. If the SQL check is part of liveness and the database goes down, every pod restarts in a loop even though the application code is healthy and a restart cannot fix a database outage. The database check belongs on the readiness probe, which removes the pod from the load balancer until the dependency recovers without triggering unnecessary restarts. I map `/health/live` to a lightweight in-process check — memory, startup completion — and `/health/ready` to `AddDbContextCheck` and any other external dependency tags that indicate whether the pod can safely receive traffic.

---

#### Gotcha 13. N+1 queries in list endpoints

**Concepts**
- N+1 query problem — one query per row for related data
- Lazy loading — navigation property trigger during serialization
- `Include` / `ThenInclude` — eager load within a query
- DTO projection — single query with only needed columns
- Serialization traversal — triggers lazy loads at response time

**Answer**

The N+1 problem happens when a list endpoint returns entities with navigation properties and the serializer traverses those navigations during JSON writing, triggering one SQL query per row. A list of 100 orders serialized with their Customer navigation causes 101 queries — one for the orders and one per customer — which is invisible in unit tests but catastrophic in production with real data volumes. I fix this by projecting directly to DTOs in LINQ so EF Core generates a single query with a JOIN, fetching only the columns the response needs. When the object graph genuinely needs to be included I use `Include`/`ThenInclude` or split queries explicitly rather than relying on lazy load during serialization.

---

#### Gotcha 14. Unstable pagination with Skip/Take

**Concepts**
- Offset pagination — `Skip`/`Take` shifts on concurrent mutations
- Keyset pagination — stable cursor using indexed key
- Duplicate and skipped rows — consequence of offset instability
- `WHERE id > @lastId ORDER BY id` — keyset pattern
- Cursor tokens in response metadata

**Answer**

`Skip((page - 1) * pageSize).Take(pageSize)` calculates an offset from the beginning of the result set, which means concurrent inserts and deletes between requests shift the window — rows added before the current page push later rows into the next page, causing items to appear twice, and deletions cause items to be skipped entirely. Keyset pagination avoids this by anchoring on the last seen stable key: `WHERE id > @lastId ORDER BY id LIMIT @pageSize`, so the window moves forward relative to a value rather than a position. I expose the last key as a cursor token in the response metadata and accept it as a parameter on the next request. Offset pagination is still acceptable for small, mostly static reference data where the instability risk is low and the simplicity matters.

---

#### Gotcha 15. GraphQL N+1 without DataLoader

**Concepts**
- GraphQL field resolvers — per-parent-row execution by default
- DataLoader — batching and deduplication of sub-queries
- N+1 in GraphQL — 1 root query + N child queries
- HotChocolate DataLoader registration in DI

**Answer**

In HotChocolate and most GraphQL servers, field resolvers execute independently for each parent object by default — a list query returning 100 authors where each author's `books` field is resolved separately executes 101 queries. DataLoader solves this by collecting all the keys requested during a single execution phase and dispatching one batched query for all of them, deduplicating repeated keys automatically. I register DataLoader classes in DI so they are scoped to the request and group concurrent resolver calls into single round-trips to the database. When the client always requests nested fields together, projecting at the root query with a join is even more efficient than DataLoader since it avoids the batching overhead entirely.

---

#### Gotcha 16. gRPC in browser without gRPC-Web

**Concepts**
- Native gRPC — HTTP/2 binary framing inaccessible to browser JavaScript
- gRPC-Web — translation protocol for browser clients
- `AddGrpcWeb()` / `EnableGrpcWeb()` — ASP.NET Core middleware
- CORS — required alongside gRPC-Web for cross-origin browser calls

**Answer**

Browsers do not expose the HTTP/2 trailer and binary framing that native gRPC requires, so a Blazor WASM or SPA client cannot use the standard gRPC protocol directly. gRPC-Web is a subset protocol that wraps gRPC messages in a format browsers can use via the Fetch API, and ASP.NET Core supports it by adding `AddGrpcWeb()` to services and calling `EnableGrpcWeb()` on each mapped gRPC service. The browser client also needs the `grpc-web` package rather than the native gRPC client. Since browser calls are still subject to the Same-Origin Policy, I must configure CORS alongside gRPC-Web — the preflight on the first cross-origin call needs `Access-Control-Allow-Headers` to include the gRPC-Web headers.

---

## Scenario-Based Questions (Karat Format)

---

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

**Concepts**
- `System.Text.Json` camelCase default — case-sensitive inbound matching
- Silent binding failure — PascalCase keys arrive as null/default
- `PropertyNameCaseInsensitive` — migration compatibility option
- `[Required]` — catches missing values but not wrong-case keys
- Breaking contract change from Newtonsoft defaults

**Answer**

The root cause is that `System.Text.Json` defaults to case-sensitive matching under the camelCase policy, so the PascalCase key `"CustomerName"` does not match the expected `"customerName"` key during deserialization. The property binds as empty string, the null check fires, and the request fails with 400 even though the client sent the correct data. The JavaScript web app works because it already sends camelCase keys that match the policy.

The fastest fix without touching the client is `AddJsonOptions(o => o.JsonSerializerOptions.PropertyNameCaseInsensitive = true)` in `Program.cs`, which makes deserialization accept both `"customerName"` and `"CustomerName"`. The right long-term fix is to have the legacy client adopt camelCase, documenting the change in an API changelog rather than relying on server-side tolerance indefinitely. I would also replace the manual empty-string check with `[Required]` on `CustomerName` so the error response becomes a proper `ValidationProblemDetails` 400 rather than a plain string.

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

**Concepts**
- XML output formatter — not registered by default
- Content negotiation fallback — JSON returned when XML unavailable
- `AddXmlSerializerFormatters()` — opt-in XML support
- `ReturnHttpNotAcceptable` — strict 406 enforcement
- `[Produces]` — documenting supported formats

**Answer**

Because only `System.Text.Json` is registered, the framework has no XML output formatter to satisfy `Accept: application/xml`. Rather than returning 406 Not Acceptable, the default behavior falls back to the first available formatter — JSON — so the partner receives a JSON body with a 200 status and no indication that their preferred format was not honored. This violates HTTP semantics and breaks downstream XML parsers silently.

The fix depends on whether XML support is actually required. If the partner needs XML I add `builder.Services.AddControllers().AddXmlSerializerFormatters()` and add an integration test that sends `Accept: application/xml` and asserts the response Content-Type and body shape. If the API is JSON-only I should add `[Produces("application/json")]` and configure `options.ReturnHttpNotAcceptable = true` in MVC options so the partner receives 406 and knows to update their client rather than silently consuming wrong-format responses.

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

**Concepts**
- `[Produces]` — documents intent, does not alone enforce 406
- `ReturnHttpNotAcceptable` — MVC option for strict negotiation
- Default formatter fallback — JSON returned when no match
- 406 Not Acceptable — correct response for unsatisfied Accept

**Answer**

`[Produces("application/json")]` is a metadata attribute that documents what the action can produce and constrains formatter selection to JSON, but it does not by itself cause the framework to return 406 when the client requests a format it cannot satisfy. Without `options.ReturnHttpNotAcceptable = true` in the MVC formatter options, the pipeline falls back to the first registered formatter — JSON — and returns 200 even when the client requested PDF.

The fix is to add `builder.Services.AddControllers(options => options.ReturnHttpNotAcceptable = true)`, which tells the pipeline to return 406 rather than fall back when no registered formatter can satisfy the Accept header. I keep `[Produces("application/json")]` on the action because it accurately documents the contract and informs the OpenAPI document, and I add an integration test that sends `Accept: application/pdf` and asserts the 406 response code.

---

#### Q4. (P) Explain how `System.Text.Json` camelCase naming is configured in ASP.NET Core 8 Web APIs, and why `[JsonPropertyName("customer_name")]` on one property affects the whole contract story.

---

**Concepts**
- `JsonNamingPolicy.CamelCase` — default naming applied globally
- `[JsonPropertyName]` — per-property override of naming policy
- Mixed naming contract — some camelCase, one snake_case
- OpenAPI schema accuracy — generated clients must match actual keys
- `PropertyNameCaseInsensitive` — inbound matching tolerance

**Answer**

The Web API template configures camelCase via `builder.Services.AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase)`, which applies to every controller in the application. On the outbound side `CustomerName` serializes as `"customerName"`, and on the inbound side deserialization expects `"customerName"` unless `PropertyNameCaseInsensitive = true` relaxes that.

When `[JsonPropertyName("customer_name")]` appears on one property, it overrides the naming policy for that property only, so the JSON contract becomes a mix: `"customerName"` for most fields and `"customer_name"` for that one. The problem is that code generators reading the OpenAPI schema must document this exception accurately — if the schema shows `"customerName"` but the actual JSON key is `"customer_name"`, generated TypeScript clients will send the wrong key and get silent null bindings. I use `[JsonPropertyName]` only when integrating with external systems that mandate specific field names, and I make sure the OpenAPI document reflects the override so every generated client uses the correct key.

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

**Concepts**
- `CanWriteType` — must declare supported CLR types narrowly
- Unsafe cast in `WriteResponseBodyAsync` — runtime type mismatch
- `IEnumerable<OrderDto>` vs wrapper types — return value shape
- `context.Object` — actual value after `OkObjectResult` unwrapping
- `IsAssignableFrom` — correct type eligibility check

**Answer**

The bug is that `CanWriteType` returns `true` for every CLR type, so the formatter activates even when `context.Object` is not `IEnumerable<OrderDto>`. In staging, the action may return a slightly different type — a `List<OrderDto>`, a `PagedResult<OrderDto>`, or a wrapper — causing the hard cast to throw `InvalidCastException` or produce an empty body depending on how the exception is swallowed. Locally the test data happened to match the exact type, masking the problem.

The correct `CanWriteType` implementation is `return type != null && typeof(IEnumerable<OrderDto>).IsAssignableFrom(type)`, which correctly accepts `List<OrderDto>` as a valid subtype. Inside `WriteResponseBodyAsync` I replace the hard cast with a pattern match and add a null/empty check before calling `ToCsv`. The `[Produces("text/csv")]` on the action is correct and should stay so the formatter only activates when the client explicitly requests CSV — it prevents the formatter from intercepting JSON-accepting clients.

---

#### Q6. (M) A bank middleware still requires SOAP/XML for one endpoint while the rest of the platform is JSON. Where do input and output formatters sit in the pipeline relative to model binding, and what does `[Consumes("application/xml")]` change?

---

**Concepts**
- Input formatter — selected by Content-Type at model binding stage
- Output formatter — selected by Accept at result execution stage
- `[Consumes]` — action selection constraint, not just binding hint
- `AddXmlSerializerFormatters()` — opt-in for both input and output XML
- Formatter isolation — registering XML for specific controllers only

**Answer**

Input formatters sit within the model binding phase, which runs after routing selects an endpoint and before the action method executes. Model binding calls the input formatter selected by the request's Content-Type header to deserialize the body into the action parameter — so `Content-Type: application/xml` routes the body to the XML input formatter if one is registered. Output formatters run later, during result execution after the action returns, and select the serializer based on the Accept header and `[Produces]` constraints.

`[Consumes("application/xml")]` does more than hint at the expected format — it is an action selection constraint, meaning routing uses it to decide which action matches a request. A JSON POST to that route returns 415 Unsupported Media Type and never reaches the action method body. For the bank endpoint, I would register `AddXmlSerializerFormatters()` globally and use `[Consumes("application/xml")]` on the legacy endpoint to isolate XML intake from the JSON-only endpoints. For output, `[Produces("application/xml")]` on the same action ensures the response is XML regardless of the client's Accept header. If I want to avoid adding XML globally, I can register a custom XML formatter scoped to that controller only using a controller-level filter or a dedicated minimal controller.

---

#### Q7. (D) Leadership wants to drop XML support to reduce maintenance. One state-government client still posts `application/xml` to `POST /api/permits`. How do you decide retire vs adapter vs gateway translation?

---

**Concepts**
- Retire XML — requires contractual agreement and sunset deadline
- Adapter service — sidecar converting XML to JSON
- Gateway translation — APIM/nginx payload transformation
- In-app `AddXmlSerializerFormatters` — simplest but spreads legacy into codebase
- Deprecation signaling — `Sunset` and `Deprecation` headers

**Answer**

The decision turns on three inputs: whether the government contract has a migration clause, how long the transition period needs to be, and how much operational complexity the team can absorb. If the contract allows it and the client can migrate, I announce a sunset date via `Deprecation: true` and `Sunset: <date>` headers on the endpoint, provide a migration guide, and monitor usage until the date passes.

When the client cannot migrate on the API team's timeline, an adapter service is my preference over in-app formatters — a small sidecar converts incoming XML to a JSON DTO and calls the main API internally, keeping the JSON core clean and fully testable without the XML path in the main codebase. A gateway translation at APIM or nginx is operationally simpler to deploy but pushes XML schema knowledge into the ops layer, where schema changes are harder to test and own. Keeping `AddXmlSerializerFormatters` in the main app is the lowest-friction short-term fix but accumulates maintenance debt if XML support expands to more endpoints. I would document whichever approach is chosen in the ADR so the ownership is clear when the government client eventually migrates.

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

---

**Concepts**
- `[Produces]` mismatch — declared format without registered formatter
- False OpenAPI contract — Swagger advertises XML that cannot be produced
- `Vary: Accept` — required when multiple representations exist
- Cache poisoning — shared cache serves JSON to XML-expecting clients
- `[Produces]` accuracy — must reflect registered formatters

**Answer**

`[Produces("application/json", "application/xml")]` on the controller tells Swashbuckle to document both content types in the OpenAPI spec, so generated clients and the Swagger UI show XML as a valid response format. Since no XML formatter is registered, every request receives JSON regardless of the Accept header — clients expecting XML receive JSON, and any caching proxy that does not include `Vary: Accept` will cache the JSON body and serve it to subsequent XML-accepting clients as well.

The immediate fix is to remove `"application/xml"` from `[Produces]` so the OpenAPI contract only advertises JSON, which is what the action actually produces. If XML support is genuinely needed, I add `AddXmlSerializerFormatters()` and add an integration test with `Accept: application/xml` asserting the correct response Content-Type and body. I also add `Response.Headers["Vary"] = "Accept"` or configure it at the reverse proxy so caches use the Accept header as part of the cache key when multiple representations exist.
