# Problem Details & Error Responses — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is RFC 7807 Problem Details?](#q1-what-is-rfc-7807-problem-details)
2. [Q2. What is the `ProblemDetails` class in ASP.NET Core?](#q2-what-is-the-problemdetails-class-in-aspnet-core)
3. [Q3. What is `ValidationProblemDetails`?](#q3-what-is-validationproblemdetails)
4. [Q4. What is the difference between `ProblemDetails` and a custom `{ error: "..." }` object?](#q4-what-is-the-difference-between-problemdetails-and-a-custom-error-object)
5. [Q5. What HTTP status code does `[ApiController]` return for validation failures?](#q5-what-http-status-code-does-apicontroller-return-for-validation-failures)
6. [Q6. What is centralized exception handling for Web APIs?](#q6-what-is-centralized-exception-handling-for-web-apis)
7. [Q7. What is `IExceptionHandler` in .NET 8?](#q7-what-is-iexceptionhandler-in-net-8)
8. [Q8. What should Production error responses exclude?](#q8-what-should-production-error-responses-exclude)
9. [Q9. What is the difference between 400 Bad Request and 404 Not Found for APIs?](#q9-what-is-the-difference-between-400-bad-request-and-404-not-found-for-apis)
10. [Q10. When should an API return 409 Conflict?](#q10-when-should-an-api-return-409-conflict)
11. [Q11. What is the `type` field in ProblemDetails?](#q11-what-is-the-type-field-in-problemdetails)
12. [Q12. What is the `title` field in ProblemDetails?](#q12-what-is-the-title-field-in-problemdetails)
13. [Q13. What is the `detail` field in ProblemDetails?](#q13-what-is-the-detail-field-in-problemdetails)
14. [Q14. What is the difference between Development and Production error responses?](#q14-what-is-the-difference-between-development-and-production-error-responses)
15. [Q15. What is `AddProblemDetails()` used for?](#q15-what-is-addproblemdetails-used-for)
16. [Q16. What is the `errors` dictionary in `ValidationProblemDetails`?](#q16-what-is-the-errors-dictionary-in-validationproblemdetails)
17. [Q17. What is the difference between returning `NotFound()` and a custom ProblemDetails for 404?](#q17-what-is-the-difference-between-returning-notfound-and-a-custom-problemdetails-for-404)
18. [Q18. How do API clients reliably parse validation errors?](#q18-how-do-api-clients-reliably-parse-validation-errors)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What is RFC 7807 Problem Details?

**Concepts**
- RFC 7807 standard HTTP error body contract
- `application/problem+json` content type as machine-readable signal
- Core fields: type, title, status, detail, instance
- Extension properties for custom fields like traceId
- ASP.NET Core ProblemDetails and ValidationProblemDetails mapping

**Answer**

RFC 7807 defines a standard JSON format for HTTP API error responses so every client can parse errors consistently without building a one-off deserializer per endpoint. The core body carries `type` (a URI identifying the problem category), `title` (stable short summary), `status` (the HTTP status code), `detail` (occurrence-specific explanation), and `instance` (typically the request path) — which means monitoring tools, API gateways, and partner integrations all understand the same shape. The `Content-Type` is `application/problem+json` rather than plain `application/json`, giving clients a machine-readable signal to select the problem schema rather than the success schema. Extensions are permitted, so ASP.NET Core adds `traceId` automatically and validation errors include an `errors` dictionary without violating the standard. ASP.NET Core 8 maps `ProblemDetails` and `ValidationProblemDetails` directly to this contract through `[ApiController]` conventions and `AddProblemDetails()` registration.

---

## Q2. What is the `ProblemDetails` class in ASP.NET Core?

**Concepts**
- ProblemDetails as the built-in RFC 7807 model
- Standard properties: Type, Title, Status, Detail, Instance
- Extensions dictionary for custom fields without breaking the contract
- Controller helpers and minimal API Results.Problem returning ProblemDetails
- AddProblemDetails for global customization

**Answer**

`ProblemDetails` is the built-in ASP.NET Core model for RFC 7807 error payloads, so controller helpers like `NotFound()`, `BadRequest()`, and `Results.Problem()` all serialize it to JSON with standard fields populated from the HTTP status and your message. The class exposes `Type`, `Title`, `Status`, `Detail`, and `Instance` (usually the request path), plus an `Extensions` dictionary for custom fields like `traceId` without breaking the standard shape. Both MVC controllers via `ControllerBase.Problem()` and minimal APIs via `Results.Problem()` use the same underlying class, which means OpenAPI tools and Swashbuckle can document `ProblemDetails` as the response schema for 4xx and 5xx codes consistently. In .NET 8, calling `builder.Services.AddProblemDetails()` registers services to customize defaults and environment-specific behavior globally, so the same shape applies across exception handlers and status code pages without repeating logic.

---

## Q3. What is `ValidationProblemDetails`?

**Concepts**
- ValidationProblemDetails extending ProblemDetails with an Errors dictionary
- IDictionary<string, string[]> mapping field names to message arrays
- ModelStateInvalidFilter short-circuiting before the action body runs
- Default 400 Bad Request status vs configurable 422 via InvalidModelStateResponseFactory
- camelCase key normalization in the errors dictionary

**Answer**

`ValidationProblemDetails` extends `ProblemDetails` with an `Errors` property — an `IDictionary<string, string[]>` mapping each field name to one or more validation messages — so clients can show per-field form errors without parsing a combined string. When an `[ApiController]` controller receives a request where `ModelState` is invalid, the `ModelStateInvalidFilter` runs before the action method body executes and automatically returns `ValidationProblemDetails` as a 400 response, which means validation failures never reach your business logic. Keys follow the JSON naming policy — camelCase by default — so `Email` in the C# model becomes `email` in the JSON, matching what frontend validators expect. Teams that prefer **422 Unprocessable Entity** can switch via `ConfigureApiBehaviorOptions` and `InvalidModelStateResponseFactory`. Multiple validation attributes failing on the same field all appear in the array, so `[Required]` and `[EmailAddress]` failures for one property both reach the client in one response.

---

## Q4. What is the difference between `ProblemDetails` and a custom `{ error: "..." }` object?

**Concepts**
- RFC 7807 structured contract vs ad hoc string envelope
- Machine-readable type URI for client branching logic
- Structured errors dictionary vs flat message string
- OpenAPI and API gateway integration with ProblemDetails
- traceId for production support correlation

**Answer**

`ProblemDetails` follows an industry-standard schema with typed fields, HTTP status alignment, and OpenAPI documentation support, so clients across every team and integration point share one deserializer. A custom `{ error: "..." }` object forces every consumer to implement a one-off parser, and those parsers diverge over time — which means you end up with "sometimes there is an `error` key, sometimes `errors`, sometimes a `message`" situations that produce support tickets on every new integration. Problem Details carries `status`, `title`, `type`, and `instance` rather than just a message string, so API gateways and APM tools can parse failures without hardcoding your shape. Validation errors in `ValidationProblemDetails` use a structured `errors` dictionary rather than a flat array, enabling per-field display. Custom shapes also tend to omit `traceId` or correlation identifiers, which makes production support much harder because the client-facing error cannot be tied to a server log entry.

---

## Q5. What HTTP status code does `[ApiController]` return for validation failures?

**Concepts**
- ModelStateInvalidFilter short-circuiting the pipeline on invalid ModelState
- 400 Bad Request as the default validation status
- ValidationProblemDetails response shape with errors dictionary
- ConfigureApiBehaviorOptions and InvalidModelStateResponseFactory for 422
- Validation path separate from exception middleware

**Answer**

By default, `[ApiController]` returns **400 Bad Request** with a `ValidationProblemDetails` body when model binding or data annotation validation fails, and the action method never executes because the `ModelStateInvalidFilter` short-circuits the pipeline when `ModelState.IsValid` is false. The response body is RFC 7807 JSON with an `errors` dictionary keyed by property name, so the client receives field-level messages without the controller checking `ModelState` manually. Teams that prefer **422 Unprocessable Entity** to signal that the request was parsed correctly but semantically invalid can override this via `ConfigureApiBehaviorOptions` and `InvalidModelStateResponseFactory`. This validation path is entirely separate from exception middleware — a `[Required]` annotation failure is not an unhandled exception and never reaches `IExceptionHandler` or `UseExceptionHandler`. Document the chosen status consistently in OpenAPI so client code generators produce the correct response type for validation paths.

---

## Q6. What is centralized exception handling for Web APIs?

**Concepts**
- Single handler mapping domain exceptions to HTTP status codes
- Domain exceptions thrown without HTTP knowledge in services
- Single logging point with full exception and TraceIdentifier
- IExceptionHandler and UseExceptionHandler registration in ASP.NET Core 8
- Eliminating per-controller try/catch duplication

**Answer**

Centralized exception handling catches unhandled exceptions in one place — middleware or `IExceptionHandler` in .NET 8 — maps them to appropriate HTTP status codes, and returns `ProblemDetails` JSON instead of scattering try/catch blocks in every controller. The key design principle is that domain services throw typed exceptions like `NotFoundException` or `ConflictException` without any HTTP knowledge, since HTTP mapping is a cross-cutting concern that belongs in one handler rather than replicated across the codebase. That single handler logs the full exception object with stack trace and `TraceIdentifier` in one place, so you get consistent log structure across every error path. Without centralization, controllers independently produce `{ error = ex.Message }` with wrong status codes — validation errors as 500, not-found as 400 — which breaks client retry logic and APM alerting. Register with `AddExceptionHandler<GlobalExceptionHandler>()` and `UseExceptionHandler()` early in the pipeline in ASP.NET Core 8.

---

## Q7. What is `IExceptionHandler` in .NET 8?

**Concepts**
- IExceptionHandler.TryHandleAsync contract and bool return value
- true/false meaning for multi-handler chains
- Registration via AddExceptionHandler<T>() with ordering
- Pairing with UseExceptionHandler() to activate the chain
- Writing ProblemDetails via IProblemDetailsService or WriteAsJsonAsync

**Answer**

`IExceptionHandler` is the .NET 8 interface for pluggable global exception handling — implement `TryHandleAsync(HttpContext, Exception, CancellationToken)` and return `true` when your handler wrote the response, or `false` to delegate to the next registered handler. This return-value contract is important because multiple handlers can be registered in sequence; the first one returning `true` wins and remaining handlers are skipped, which means you can have a specialized `PaymentExceptionHandler` that handles payment-specific faults first and a fallback `GlobalExceptionHandler` for everything else. Register with `builder.Services.AddExceptionHandler<GlobalExceptionHandler>()` and activate with `app.UseExceptionHandler()` — no custom path argument is required when handlers are registered this way. The handler writes `ProblemDetails` via `context.Response.WriteAsJsonAsync` or by resolving `IProblemDetailsService` from DI, giving access to the global customization pipeline including `traceId` injection. This replaces the older custom exception middleware pattern from earlier ASP.NET Core versions.

---

## Q8. What should Production error responses exclude?

**Concepts**
- Information disclosure risks: stack traces, connection strings, SQL fragments
- Client-safe generic message vs full diagnostic in server logs
- IsDevelopment() guard for conditional diagnostic extensions
- traceId in response for support correlation without exposing internals
- Sanitizing the Detail field per error category

**Answer**

Production error responses must not expose stack traces, exception type names with internal code paths, file paths, connection strings, SQL fragments, or environment-specific configuration, because an attacker can use those details to map the system and craft targeted attacks. The correct pattern is to log the full exception server-side with `LogError(ex, ...)` — including stack trace, inner exceptions, and structured data — while the response `Detail` contains only user-safe text: "An unexpected error occurred" for 500, and specific but non-sensitive text like "Customer with id 42 was not found" for 404 and 409. The `Detail` field must never echo raw `ex.Message` from infrastructure exceptions since database errors often contain table names, column names, or constraint details that reveal schema. Use `IHostEnvironment.IsDevelopment()` inside `AddProblemDetails()` customization to add diagnostic extensions locally only, never in Staging or Production. Always include `traceId` or `requestId` in the response so support teams can correlate a client error report to the right log entry without exposing any internals.

---

## Q9. What is the difference between 400 Bad Request and 404 Not Found for APIs?

**Concepts**
- 400 — invalid or malformed request the server cannot interpret
- 404 — well-formed request with no matching resource or route
- ValidationProblemDetails for 400 validation paths
- REST semantics and caching implications of wrong status codes
- ProblemDetails JSON shape consistent across both

**Answer**

**400 Bad Request** means the client sent a syntactically or semantically invalid request — bad JSON structure, failed data annotation validation, or a parameter in the wrong format — so the server cannot interpret what was requested. **404 Not Found** means the request was well-formed and the server understood it completely, but the target resource does not exist. The distinction matters because returning 404 for an invalid query parameter misleads the client into thinking the resource type is missing rather than fixing their input, while returning 400 when a resource simply does not exist breaks REST semantics and caching expectations since proxies cache 404 but not 400. In `[ApiController]` APIs, validation failures belong on 400 with `ValidationProblemDetails`, while a valid route hitting a non-existent entity id — `GET /api/orders/99999` — belongs on 404 with `ProblemDetails`. Both status codes should use the same ProblemDetails JSON shape for a consistent client experience.

---

## Q10. When should an API return 409 Conflict?

**Concepts**
- 409 for state conflicts on otherwise valid requests
- Optimistic concurrency failure mapped to 409
- Duplicate unique-key and business rule violations
- DbUpdateConcurrencyException from EF Core mapping to 409
- ProblemDetails with descriptive detail for conflict explanation

**Answer**

Return **409 Conflict** when the request is syntactically and semantically valid but cannot be applied due to a state conflict with the current resource — the input itself is fine, but the current state of the system makes the operation impossible. Common scenarios include duplicate unique-key violations on registration, an optimistic concurrency failure on PUT (another request updated the row since the client loaded it), or a business rule like "cannot cancel an already-shipped order." The conflict is distinct from 400 (invalid input) and 404 (resource missing) — the resource often exists and the request was correct, so clients know to check current state and potentially retry rather than fix their input. Map `DbUpdateConcurrencyException` from EF Core to 409 when using row versioning or `[Timestamp]` tokens. Return `ProblemDetails` with a clear `detail` explaining the conflict in non-sensitive terms and document 409 in OpenAPI so clients can implement appropriate retry or conflict-resolution UI.

---

## Q11. What is the `type` field in ProblemDetails?

**Concepts**
- type as a URI reference identifying the problem category
- Stable URI contract for client branching logic
- ASP.NET Core default type derived from status code
- Custom type URIs for domain-specific error categories
- OpenAPI documentation of problem types per endpoint

**Answer**

The `type` field is a URI reference identifying the problem category — often a stable URL pointing to documentation about that error type — which lets clients branch logic programmatically without parsing free-text messages. When a client checks `problem.type === "https://api.example.com/errors/validation"` it can route to the form error handler, while `"https://api.example.com/errors/not-found"` routes to a redirect, which means the `type` URI is the machine-readable discriminator rather than the `title` string. ASP.NET Core sets a default type based on status code when you use built-in helpers, typically pointing to RFC 7231 sections for standard codes. Clients should treat the type as an opaque identifier, not a URL users must visit, since the URI simply needs to be stable and unique per error category. Custom exception handlers can set `type` per domain exception, which is especially useful in partner integrations where clients want to handle "low inventory" differently from "payment declined" without parsing the `detail` string.

---

## Q12. What is the `title` field in ProblemDetails?

**Concepts**
- title as stable human-readable problem type summary
- title independent of specific request occurrence
- ASP.NET Core default title from status code mapping
- Localization target for multi-language API consumers
- Pairing title with status for display logic

**Answer**

The `title` field is a short, human-readable summary of the problem type that remains stable for a given error category regardless of which specific request triggered it — unlike `detail` which varies per request. This stability makes `title` safe to display in UI headers or toast notifications without sanitization checks, since "Validation Failed" looks the same whether field `email` or field `phone` was invalid. ASP.NET Core populates it from status code defaults (e.g., "Bad Request" for 400) unless your handler overrides it. Localization can target `title` independently of `detail` for multi-language API consumers since the stable string is easy to look up in a translation table. Pair it with `status` so clients can use either the HTTP status code or the title string for display logic when deserializing ProblemDetails — they carry the same semantic meaning.

---

## Q13. What is the `detail` field in ProblemDetails?

**Concepts**
- detail as occurrence-specific explanation varying per request
- Sanitizing detail in Production — generic for 500, safe-specific for 404/409
- Separation from title which is category-level and stable
- Avoiding reflected XSS by not echoing raw user input
- Logging full exception server-side while keeping detail safe

**Answer**

The `detail` field explains this specific occurrence of the problem — what went wrong for this particular request — and may include safe contextual information like "Customer with id 42 was not found" while `title` stays at "Not Found" for all 404s. For 500 errors in production, `detail` should be generic ("An unexpected error occurred") while the full exception is logged server-side with `LogError(ex, ...)`; for 404 and 409 errors, `detail` can be specific but must not contain stack traces, SQL fragments, or connection details. Avoid echoing raw user input in `detail` to prevent reflected XSS in any client that renders error text as HTML. For validation errors, field-level messages live in the `errors` dictionary rather than `detail`, which holds only a summary like "One or more validation errors occurred." The `detail` field is the most commonly over-shared field in poorly secured APIs, so treating it as "safe-to-external" by default and escalating detail level only in Development is the right default posture.

---

## Q14. What is the difference between Development and Production error responses?

**Concepts**
- Development diagnostic extensions vs Production minimal response body
- IsDevelopment() guard for stack trace and exception type extensions
- Same HTTP status code mapping in both environments
- AddProblemDetails CustomizeProblemDetails delegate for environment split
- Logging level and content separate from response body decisions

**Answer**

Development responses can include richer diagnostics — exception type names, stack traces in extension fields, or the Developer Exception Page for browser traffic — since developers need to diagnose failures quickly without consulting logs. Production responses return safe, minimal ProblemDetails with a correlation id while full diagnostics go to server logs only, because any internal detail in the response body is a potential information disclosure vector for external callers. Both environments must use the same HTTP status code for the same exception type since changing status codes between environments causes confusing test results where staging passes but production fails differently. The right place to implement this split is the `AddProblemDetails(options => options.CustomizeProblemDetails = ...)` delegate or `IExceptionHandler` where you check `IHostEnvironment.IsDevelopment()` — not `#if DEBUG` blocks scattered across controllers, which miss runtime environment changes. Logging level and content remain the same or stricter in Production; only the response body differs.

---

## Q15. What is `AddProblemDetails()` used for?

**Concepts**
- AddProblemDetails registering global ProblemDetails services
- CustomizeProblemDetails delegate for traceId and environment-specific detail
- IProblemDetailsService for consistent creation across middleware
- Complementing AddExceptionHandler<T>() with shared customization
- Unified error shape across minimal APIs and controllers

**Answer**

`AddProblemDetails()` registers the services responsible for RFC 7807 Problem Details generation globally in ASP.NET Core 8 — including default field values, customization delegates, and integration with exception handling and status code pages. I call it as `builder.Services.AddProblemDetails(options => { ... })` in `Program.cs` and supply a `CustomizeProblemDetails` delegate to add `traceId`, sanitize `detail` based on environment, or append Development-only extensions in one place. This works through `IProblemDetailsService`, which handlers and middleware can resolve from DI to create consistent ProblemDetails responses rather than constructing objects manually each time. It complements `AddExceptionHandler<T>()` — the exception handler can inject `IProblemDetailsService` and write a response that goes through the same customization pipeline. Without this registration, minimal APIs and controllers might produce slightly different error shapes for the same exception, because built-in helpers fall back to their own defaults.

---

## Q16. What is the `errors` dictionary in `ValidationProblemDetails`?

**Concepts**
- IDictionary<string, string[]> mapping field names to message arrays
- camelCase key normalization matching JSON naming policy
- Multiple messages per field when several annotations fail simultaneously
- Global/model-level errors under empty string key
- Stable per-field parsing contract vs regex on detail string

**Answer**

The `errors` property is an `IDictionary<string, string[]>` where each key is a model property or field name and each value is an array of validation messages for that field, enabling clients to show per-field inline errors rather than parsing a combined string. Keys follow the JSON naming policy — camelCase by default — so `Email` in C# becomes `email` in JSON, matching what frontend form libraries expect. Multiple validation attributes failing on the same property both appear in the array, so a field with `[Required]` and `[EmailAddress]` both failing produces two messages under the same key. Global or model-level errors not tied to a specific property may appear under an empty string key `""`. The stable client parsing contract is `response.errors.fieldName[0]` rather than regex parsing on `detail`, which means clients can depend on this structure regardless of which fields were invalid or how many errors occurred.

---

## Q17. What is the difference between returning `NotFound()` and a custom ProblemDetails for 404?

**Concepts**
- [ApiController] converting bare NotFound() to ProblemDetails JSON
- NotFound(string) returning text/plain — incompatible with JSON contract
- Explicit ProblemDetails giving control over all RFC 7807 fields
- Minimal API Results.Problem vs TypedResults.NotFound patterns
- Consistent approach across all controllers to prevent mixed content types

**Answer**

On an `[ApiController]`, `NotFound()` without arguments automatically returns a ProblemDetails body with status 404 because the `[ApiController]` convention intercepts the bare not-found result and serializes it to `application/problem+json`. However, `NotFound(string message)` returns **text/plain** with the string as the response body — same HTTP status, but an incompatible JSON contract, which breaks partner integrations parsing RFC 7807 where they expect an object. Explicit `NotFound(new ProblemDetails { Title = "...", Detail = "..." })` gives full control over all fields and ensures the response goes through your global `AddProblemDetails` customization pipeline. Without `[ApiController]`, the bare `NotFound()` behavior differs and may return an empty body, so this convention is not universal. In minimal APIs, `Results.Problem(statusCode: 404, ...)` or `TypedResults.NotFound()` serve the equivalent purpose. The practical rule is to standardize one approach across all controllers so you never have "sometimes JSON, sometimes plain text" 404s causing support tickets.

---

## Q18. How do API clients reliably parse validation errors?

**Concepts**
- Expecting 400 or team-standard 422 as the validation status
- Deserializing as ValidationProblemDetails rather than a custom shape
- Iterating errors dictionary keyed by property name
- Content-Type check as the signal to switch to the problem deserializer
- Model-level errors under empty string key handled separately

**Answer**

Clients should expect HTTP **400** (or whatever the team has standardized as — sometimes **422**), check that the `Content-Type` is `application/problem+json` or `application/json` with the ProblemDetails shape, then deserialize as `ValidationProblemDetails` and read both `status`/`title` for display and the `errors` dictionary for per-field highlighting. The `errors` property maps property names to arrays of messages, so `errors["email"][0]` gives the first validation message for the email field without any string parsing. Using code-generated clients from OpenAPI is the cleanest approach since the `ValidationProblemDetails` schema is documented at the 400 response and the client library handles deserialization automatically. Handle model-level errors under the empty string key `""` separately since those represent errors not tied to one field. If legacy code on the server returns non-ProblemDetails 400 for some endpoints, the client should fall back gracefully while tracking those as integration debt to resolve.

---

## Gotchas — ASP.NET Core Web API (Interview Traps)

---

#### Gotcha 1. POST returning 200 instead of 201

**Concepts**
- HTTP 201 Created with Location header as REST create contract
- CreatedAtAction / CreatedAtRoute for correct response
- Resource discovery via Location header
- Status code semantics for OpenAPI-generated clients

**Answer**

A successful resource creation with POST should return HTTP 201 Created and a `Location` header pointing at the new resource URL, because 200 OK carries no hint that a new resource was created or where to find it. Standard HTTP clients, API gateways, and OpenAPI-generated SDKs all look at the status code first — returning 200 means the response body is the only way to discover the new resource id, and clients that skip parsing the body miss it entirely. Use `CreatedAtAction`, `CreatedAtRoute`, or `Created` to return 201 with the Location header, and include the created representation or a minimal payload in the body when clients need immediate data without a follow-up GET.

---

#### Gotcha 2. GET that mutates state

**Concepts**
- GET as safe and idempotent per HTTP specification
- Prefetch and crawler risks from side-effecting GETs
- Caching proxy behavior replaying GET responses
- Correct HTTP verbs for state-changing operations

**Answer**

GET must be safe and idempotent per HTTP semantics — performing deletes or updates in a GET handler violates the specification, breaks caching proxies that may replay GET responses, and creates security holes when URLs are prefetched by browsers, link-preview crawlers, or email clients. The problem is that these callers invoke GET URLs without user intent, so a delete fires without anyone clicking anything. Cached GET responses can replay destructive operations across clients since the proxy treats the response as a normal cacheable resource. Use POST, PUT, PATCH, or DELETE for any operation that changes state and reserve GET strictly for reads.

---

#### Gotcha 3. `{ success: false }` with HTTP 200

**Concepts**
- HTTP status code as the universal success vs failure contract
- 200 with error flag defeating monitoring, retries, and API gateways
- ProblemDetails for consistent structured failure responses
- APM alerting and circuit breakers depending on HTTP status

**Answer**

Business failures must map to appropriate 4xx or 5xx status codes because HTTP status is the universal contract that drives client retry logic, API gateway circuit breakers, and APM alerting thresholds — a 200 response with `success: false` in the body masks every failure from every system that does not parse the body. API gateways route and throttle on status code; if every response is 200, failed calls look healthy in dashboards and no alert fires. Return `ValidationProblemDetails` or `ProblemDetails` with 400 for validation failures, 404 for missing resources, 409 for conflicts, and 422 for semantic rejections. Envelope patterns like `{ success: false }` require every consumer to implement a custom parser and break OpenAPI contract expectations.

---

#### Gotcha 4. Returning EF entities from API actions

**Concepts**
- EF entity navigation properties not suitable for public HTTP contracts
- Lazy-loading N+1 triggered during JSON serialization
- Circular reference serializer loops
- DTO decoupling API contract from persistence schema

**Answer**

EF Core entities carry navigation properties, change-tracker state, and database-internal fields that were never meant to be a public HTTP contract, so serializing them directly leaks schema details and invites circular reference errors. Lazy-loaded navigations trigger N+1 queries during serialization when the JSON serializer walks the object graph — each navigation fires a new SQL query, exhausting the connection pool under load. Circular references between related entities cause the JSON serializer to loop indefinitely or require fragile `ReferenceHandler.IgnoreCycles` settings that hide design problems. Map entities to DTOs with explicit shapes in the service layer or via EF projection so the API contract evolves independently of table schema changes.

---

#### Gotcha 5. PascalCase JSON with default camelCase policy

**Concepts**
- System.Text.Json defaulting to camelCase serialization in ASP.NET Core 8
- Silent binding failure from PascalCase client payloads
- JsonPropertyName and PropertyNamingPolicy as alignment tools
- PropertyNameCaseInsensitive for legacy mixed-casing clients

**Answer**

ASP.NET Core 8 defaults to camelCase JSON serialization via `System.Text.Json`, so PascalCase property names from legacy clients bind as missing properties because the case does not match — the model properties default to `null` or `0` rather than the values the client sent. The failure is silent: the request returns 201 or 204 with no validation error, but the persisted record has default values instead of the submitted data. Fix with `[JsonPropertyName("PropertyName")]` attributes on DTO properties or a custom `PropertyNamingPolicy` to align server expectations with legacy payloads. When accepting mixed casing from various clients, enable `PropertyNameCaseInsensitive = true` in `AddControllers().AddJsonOptions(...)`.

---

#### Gotcha 6. GET with `[FromBody]`

**Concepts**
- GET request body not reliably supported across the HTTP ecosystem
- [FromBody] on GET failing silently through proxies and caches
- [FromQuery] for simple filters as the correct alternative
- OpenAPI tools and browser fetch blocking GET bodies

**Answer**

Many HTTP clients, proxies, CDNs, and caches ignore or strip GET request bodies because the HTTP specification does not define semantics for GET bodies — filters sent as JSON in GET requests fail silently or never reach the action in ASP.NET Core 8. Model binding for `[FromBody]` on GET is therefore unreliable across the full HTTP ecosystem even if it works in direct testing. Use query strings with `[FromQuery]` for simple filter parameters, or POST to a dedicated search endpoint for complex filter objects that do not fit in a URL. Browser fetch API and OpenAPI tooling also discourage or block GET bodies, making the pattern fragile in any production environment where the full request path includes a proxy.

---

#### Gotcha 7. CORS as server security

**Concepts**
- CORS as browser-only enforcement — not server-side authentication
- Non-browser clients unaffected by CORS headers
- Authentication and authorization as actual server protection
- CORS enabling SPA browser access alongside real auth

**Answer**

CORS is enforced by browsers only — it prevents JavaScript on one origin from reading cross-origin responses, but it does nothing to stop curl, Postman, server-to-server calls, or any direct API request. The `Access-Control-Allow-Origin` header is a signal browsers check after receiving the response; a non-browser client simply ignores it and reads the data. A public API without authentication is fully accessible to any non-browser caller regardless of CORS policy, so CORS is never a substitute for JWT, API keys, or cookies. Register `AddCors` and `UseCors` to enable browser SPA access on cross-origin calls, and enforce actual authentication and authorization separately for real protection.

---

#### Gotcha 8. `AllowAnyOrigin` with credentials

**Concepts**
- Browser rejection of wildcard origin on credentialed requests
- AllowAnyOrigin and AllowCredentials as mutually exclusive
- WithOrigins for explicit trusted frontend origins
- Access-Control-Allow-Credentials header requirement

**Answer**

Browsers reject a response with `Access-Control-Allow-Origin: *` when the request includes cookies or an `Authorization` header, because the CORS specification explicitly forbids wildcard origins on credentialed cross-origin requests. `AllowAnyOrigin()` and `AllowCredentials()` cannot be combined — ASP.NET Core will not emit a valid CORS response for credentialed requests when both are set. Instead, use `WithOrigins("https://app.example.com", "https://localhost:3000")` to list every trusted frontend origin explicitly, including local development URLs and all production domains. The browser also requires `Access-Control-Allow-Credentials: true` in the response, which `AllowCredentials()` handles.

---

#### Gotcha 9. Swagger UI exposed in Production

**Concepts**
- Swagger UI disclosing full API surface and schema to public internet
- Environment checks wrapping MapSwagger and UseSwaggerUI
- OpenAPI document exposure revealing endpoint names and enum values
- Authentication or IP allowlist gating for API documentation

**Answer**

Public Swagger UI discloses the full API surface, all schemas, enum values, and try-it-out access to anyone who finds the URL — giving potential attackers a complete map of your endpoints and data structures without any effort. Gate `MapSwagger` and `UseSwaggerUI` in `Program.cs` behind environment checks so they run only in Development and Staging, or require authentication middleware before the Swagger middleware. Production APIs should serve OpenAPI documents only to authenticated developers or internal tooling, not the public internet. Exposed OpenAPI documents reveal internal endpoint names, field names, and request schemas that are directly useful for targeted reconnaissance.

---

#### Gotcha 10. Missing `[ApiController]` on some controllers

**Concepts**
- [ApiController] enabling automatic ModelStateInvalidFilter
- Binding source inference for complex types
- Mixed controllers producing inconsistent error contracts
- Assembly-level [ApiController] for uniform behavior

**Answer**

Without `[ApiController]`, automatic 400 `ValidationProblemDetails` responses, binding source inference for complex types, and attribute routing enforcement all differ from controllers that have the attribute — so mixed controllers in the same API produce inconsistent error shapes that break partner integrations. A controller missing `[ApiController]` may return 200 OK with a partially bound model when model validation fails, because `ModelStateInvalidFilter` does not run, and `[FromBody]` is not inferred for complex parameters. Apply `[ApiController]` at the controller or assembly level using `[assembly: ApiController]` in an attribute file so every endpoint shares the same conventions without per-class annotation.

---

#### Gotcha 11. Blocking on `.Result` in async actions

**Concepts**
- Sync-over-async causing thread-pool starvation under load
- Deadlock when synchronization context is held during blocking call
- async Task<IActionResult> propagating await through service layer
- Kestrel throughput reduction from blocked request threads

**Answer**

Blocking on `.Result` or `.Wait()` in async API actions causes thread-pool starvation under load because the calling thread is blocked waiting for I/O to complete while no thread is available to process the continuation. Deadlocks also occur in environments with a synchronization context when the blocked thread holds the context that the async continuation needs to resume on — the task never completes because the thread it needs is the thread that is waiting for it. Always `await` async service and database calls in controller actions, which means the action signature is `async Task<IActionResult>` and the `await` propagates through the entire service and repository layer. Kestrel processes many concurrent requests efficiently precisely because async I/O frees threads while waiting — sync-over-async defeats this design entirely.

---

#### Gotcha 12. Liveness probe includes SQL check

**Concepts**
- Liveness as process restart signal — unrelated to external dependency recovery
- Readiness as traffic drain signal for dependency failures
- Kubernetes restart loop from liveness including external checks
- Tag-based separation of liveness and readiness health checks

**Answer**

If the liveness probe includes SQL and the database goes down for maintenance, Kubernetes kills and restarts pods even though restarting cannot fix a database outage — creating a restart loop that adds startup overhead and delays recovery. Liveness answers whether the ASP.NET Core process is alive and responsive; it should return healthy as long as the process can handle an HTTP request, independent of downstream dependencies. Readiness answers whether the instance should receive traffic; SQL, Redis, and message bus checks belong here because a failing dependency means the instance will return errors. Map `/health/live` with a tag predicate selecting only the self-check and `/health/ready` with the predicate selecting `AddDbContextCheck` and other dependency checks.

---

#### Gotcha 13. N+1 queries in list endpoints

**Concepts**
- N+1 pattern: one parent query plus N child queries per row
- Lazy loading triggering extra SQL during serialization
- EF projection with Select fetching only required columns
- Include/ThenInclude for explicit eager loading in one round trip

**Answer**

N+1 occurs when a list endpoint loads a parent collection and then each item triggers an additional query for a related navigation — one query for 100 orders plus 100 queries for each order's customer. The most common cause in APIs is serializing entity objects with lazy-loaded navigation properties: the JSON serializer accesses a navigation, EF fires a SELECT, and this repeats once per row. Fix with a single translated query: project directly to DTOs using `.Select(o => new OrderDto { CustomerName = o.Customer.Name })` so EF generates one SQL JOIN, or use explicit `.Include(o => o.Customer)` before materialization. Validate with EF logging or APM to confirm list endpoints produce a fixed small number of SQL round trips regardless of result set size.

---

#### Gotcha 14. Unstable pagination with Skip/Take

**Concepts**
- Offset pagination page drift from concurrent inserts and deletes
- Skip/Take without stable OrderBy producing undefined row order
- Keyset pagination anchored to a stable indexed key
- Large OFFSET performance cost scanning and discarding preceding rows

**Answer**

Concurrent inserts and deletes shift row positions in the dataset while a client walks pages — a new row inserted at page 1 pushes all subsequent rows one position, so page 2 either repeats the last row of page 1 or skips a row entirely. `Skip((page - 1) * pageSize).Take(pageSize)` also requires the database to count and discard all preceding rows, which becomes expensive on large offsets. Keyset pagination avoids both problems by using `WHERE id > @lastSeenId ORDER BY id LIMIT @pageSize` with the last key from the previous response — no scanning skipped rows and no drift because the filter is anchored to a specific key rather than a count. Offset pagination remains acceptable for small mostly-static tables; expose cursor tokens in link headers or response metadata for high-churn datasets.

---

#### Gotcha 15. GraphQL N+1 without DataLoader

**Concepts**
- Field resolvers executing one database query per parent row
- DataLoader batching concurrent field resolutions into a single query
- 101 queries for a 100-row list without batching
- Root-level eager loading as alternative to DataLoader for static fields

**Answer**

Field resolvers in HotChocolate or other GraphQL servers execute independently per parent row — resolving `books` for each of 100 authors runs 100 separate queries plus the initial author query, totaling 101 round trips. DataLoader batches concurrent field resolutions within a single request: all 100 `books` resolver calls accumulate the author ids during the execution tick, then DataLoader fires one grouped query for all of them at once. Register DataLoader services in DI so concurrent field resolutions within a request are grouped into single round-trips automatically. For fields the client almost always requests together with the parent, eager-load or project at the root query level rather than using DataLoader.

---

#### Gotcha 16. gRPC in browser without gRPC-Web

**Concepts**
- Native gRPC HTTP/2 binary framing not accessible to browser JavaScript
- gRPC-Web protocol as browser-compatible translation layer
- AddGrpcWeb and EnableGrpcWeb for middleware setup
- CORS configuration required alongside gRPC-Web for cross-origin calls

**Answer**

Native gRPC uses HTTP/2 binary framing that browsers do not expose to JavaScript APIs — browsers cannot control trailers or binary framing at the level gRPC requires, so `@grpc/grpc-js` in the browser fails. Browser clients need the gRPC-Web protocol, which translates between the browser-accessible HTTP/1.1 or HTTP/2 fetch API and the native gRPC binary format via ASP.NET Core middleware. Add `AddGrpcWeb()` to services and call `.EnableGrpcWeb()` on each mapped gRPC service to activate the translation layer. CORS must also be configured for the browser origin because cross-origin browser calls still enforce CORS preflight and response header checks regardless of gRPC-Web. Standard .NET or Node gRPC clients communicating server-to-server continue using native gRPC without gRPC-Web.

---

## Scenario-Based Questions (Karat Format)

---

#### Q1. (R) Review this payment service called from a Web API controller. QA sees 500 responses with no correlation id in logs, and Application Insights shows the stack trace starting at the rethrow site — not the gateway call.

```csharp
public async Task ChargeAsync(Guid orderId, decimal amount, CancellationToken ct)
{
    try
    {
        await _gateway.ChargeAsync(orderId, amount, ct);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex.Message);
        throw ex;
    }
}
```

The controller has no try/catch; it expects a global handler to shape errors.

**Concepts**
- `throw ex` resetting stack trace to the catch site vs `throw;` preserving it
- `LogError(ex.Message)` losing stack trace and inner exception in structured logging
- Catch-log-rethrow duplicating global handler responsibility
- IExceptionHandler as the single logging and mapping point
- Activity/TraceIdentifier propagation for ProblemDetails traceId correlation

**Answer**

The catch block destroys observability in two ways: `_logger.LogError(ex.Message)` passes only the message string rather than the exception object, so Serilog, Application Insights, and other structured logging providers never receive the stack trace, inner exception chain, or the `Exception` structured property — the log entry is useless for diagnosis. Then `throw ex` resets the stack trace to this method's catch site, which is why Application Insights shows the rethrow location rather than the original `_gateway.ChargeAsync` call; the global handler still runs and shapes the 500 response correctly, but the log it generates from the re-thrown exception also shows the wrong origin. The fix in priority order: remove the entire try/catch and let domain or gateway exceptions bubble to `IExceptionHandler` or `UseExceptionHandler` — the service has no reason to intercept exceptions it cannot meaningfully handle. If local context must be logged, use `_logger.LogError(ex, "Payment charge failed for {OrderId}", orderId)` and then `throw;` (bare rethrow without the `ex` argument) to preserve the original stack. Ensure middleware adds or propagates `Activity`/`TraceIdentifier` so the client-facing `ProblemDetails.Extensions["traceId"]` matches server logs.

---

#### Q2. (P) You inherit a Web API with per-controller try/catch blocks returning `{ error = ex.Message }`, plain strings for 404, and occasional 500 for validation failures. How do you move to **centralized** exception handling with RFC 7807 `ProblemDetails`, consistent status codes, and different Development vs Production response bodies — without `#if DEBUG` in every controller?

**Concepts**
- IExceptionHandler mapping domain exceptions to status codes and ProblemDetails
- Domain exception types conveying intent without HTTP coupling
- AddProblemDetails CustomizeProblemDetails for environment-specific response body
- Single logging point with full exception at the global handler
- UseExceptionHandler() middleware placement activating the handler chain

**Answer**

I would migrate this in phases so the API stays deployable throughout. First, register `builder.Services.AddExceptionHandler<GlobalExceptionHandler>()` and `app.UseExceptionHandler()` near the top of the middleware pipeline — the handler maps typed domain exceptions (`NotFoundException` → 404 with `ProblemDetails`, `ValidationException` → 400 with `ValidationProblemDetails`, `DuplicateOrderException` → 409) and everything else to 500, logging with `LogError(ex, ...)` once for the 500 path. Then I call `builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = ctx => { ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier; if (env.IsDevelopment()) ctx.ProblemDetails.Extensions["stackTrace"] = ctx.Exception?.StackTrace; })` so Development responses include diagnostics while Production responses return only the trace id and a sanitized detail — all controlled in one delegate without any `#if DEBUG`. With the handler live, I strip try/catch blocks from each controller one at a time; controllers throw domain exceptions and the handler shapes HTTP. The `{ error = ex.Message }` returns disappear because no catch block catches them, and plain string 404s go away because `NotFoundException` maps to a `ProblemDetails` object in the handler. The environment split lives only in `CustomizeProblemDetails`, never in controllers.

---

#### Q3. (M) A mobile client parses error JSON using RFC 7807. Review this `[ApiController]` action and the actual response body when model validation fails. What status code and shape does the client receive, and what fields should they rely on?

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    [HttpPost]
    public IActionResult Create([FromBody] CreateOrderDto dto)
    {
        // dto.OrderLines is required but missing in request body
        return Ok(_service.Create(dto));
    }
}
```

Assume `CreateOrderDto` has `[Required]` on `OrderLines` and no custom filter overrides validation.

**Concepts**
- ModelStateInvalidFilter short-circuiting before the action body runs
- ValidationProblemDetails shape with errors dictionary keyed by field name
- 400 Bad Request as the automatic validation status
- [ApiController] convention driving the automatic response
- camelCase key normalization in the errors dictionary

**Answer**

The action body never executes for this request. When `[ApiController]` is present, the `ModelStateInvalidFilter` intercepts the request before the action runs and returns **400 Bad Request** with a `ValidationProblemDetails` body since `CreateOrderDto.OrderLines` is marked `[Required]` but missing from the request. The response shape is `{ "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1", "title": "One or more validation errors occurred.", "status": 400, "traceId": "00-abc...def-01", "errors": { "OrderLines": ["The OrderLines field is required."] } }` with `Content-Type: application/problem+json`. The client should rely on `status` (400) as the primary signal, `title` for a human-readable summary, and the `errors` dictionary for per-field messages. The `errors` keys follow the camelCase JSON naming policy by default, so the client reads `errors.orderLines` (lowercase o). The `detail` field holds a summary, not field-level messages — per-field messages are exclusively in `errors`.

---

#### Q4. (R) Review this controller catch blocks. Swagger documents 400/404/422, but production clients receive inconsistent shapes and wrong status codes.

```csharp
catch (ValidationException ex)
{
    return StatusCode(500, new { error = ex.Message, fields = ex.Errors });
}
catch (NotFoundException ex)
{
    return NotFound(ex.Message);
}
catch (DuplicateOrderException ex)
{
    return Conflict(new ProblemDetails { Title = "Duplicate", Detail = ex.Message });
}
```

**Concepts**
- ValidationException incorrectly mapped to 500 instead of 400
- NotFound(string) returning text/plain vs ProblemDetails JSON
- Inconsistent response shapes across catch branches
- Controller-level catch blocks duplicating global handler concerns
- OpenAPI documented schemas not matching actual response bodies

**Answer**

The most serious issue is `ValidationException` mapping to 500 — this treats user input errors as server faults, causing client retry loops, SLO violations, and false alerts in production monitoring since the error count inflates the 5xx error rate. The `NotFound(ex.Message)` catch returns a **plain text string** as the response body since `ControllerBase.NotFound(object)` with a string argument returns text/plain, not ProblemDetails JSON — any RFC 7807-aware client receives an unexpected string where it expects an object. The `DuplicateOrderException` catch correctly returns `ProblemDetails` for 409, but that is the only consistent branch, so partner integrations must branch per endpoint to decide whether the error is JSON or plain text. The root cause is that controller-level catches duplicate the responsibility of the global exception handler, which means new endpoints copy wrong patterns. The fix in priority order: remove all three catches and throw typed domain exceptions — `NotFoundException`, `ValidationException`, `DuplicateOrderException` — handling their HTTP mapping in `IExceptionHandler`; all branches return `ProblemDetails` or `ValidationProblemDetails` with the correct status (400/404/409); register `services.AddProblemDetails()` so the shape is consistent; and align OpenAPI response schemas to the actual `ProblemDetails` type for every documented status.

---

#### Q5. (D) Compare **Development** vs **Production** error responses for the same unhandled `NullReferenceException` in a minimal API endpoint. What should each environment return (status, `ProblemDetails` fields, stack trace), and what must never appear in an external Production JSON body?

**Concepts**
- Same HTTP 500 status in both environments
- IsDevelopment() guard for exception type and stack trace extensions
- Production response: generic detail and traceId only
- Information disclosure: stack traces revealing internal code paths
- AddProblemDetails CustomizeProblemDetails as the environment split point

**Answer**

Both environments return **500 Internal Server Error** — the status code never changes between Development and Production because changing it would produce different test results depending on environment. In Development, the response body may include `ProblemDetails` extensions like `exception.type: "System.NullReferenceException"`, `exception.stackTrace: "at OrderService.GetAsync..."`, and a `detail` containing a meaningful description of what was null — enough for a developer to diagnose locally without consulting logs. In Production, the response body contains only `{ "type": "...", "title": "An error occurred while processing your request.", "status": 500, "traceId": "00-abc...def-01" }` — no stack trace, no exception type name, no file paths, and no message that reveals internal structure. What must never appear in a Production response body: stack trace, exception type names, class or method names, file paths, connection string fragments, SQL query text, or inner exception messages from infrastructure layers. The full exception with all of that detail goes to server-side logging via `LogError(ex, ...)` and is retrievable via `traceId`. Implement this split in the `AddProblemDetails CustomizeProblemDetails` delegate or inside `IExceptionHandler.TryHandleAsync` by checking `IHostEnvironment.IsDevelopment()`.

---

#### Q6. (P) Implement global handling with **`IExceptionHandler`** (.NET 8+) for a JSON Web API. Show registration in `Program.cs`, a handler that maps `NotFoundException` → 404 and unknown exceptions → 500, and explain what `TryHandleAsync` returning `true` vs `false` does when multiple handlers are registered.

**Concepts**
- IExceptionHandler.TryHandleAsync bool return — true stops the chain, false passes to next
- Registration order determining handler precedence
- ProblemDetails serialization via WriteAsJsonAsync inside TryHandleAsync
- AddExceptionHandler<T>() and UseExceptionHandler() in Program.cs
- Logging only at the 500 path to avoid noise from expected domain exceptions

**Answer**

I implement `IExceptionHandler` by creating a class that overrides `TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)`. For `NotFoundException`, I set `context.Response.StatusCode = 404` and write a `ProblemDetails` with title "Not Found" and the exception message as `detail`, then return `true` to signal that this handler wrote the response and the chain should stop. For any other exception type, I log with `_logger.LogError(exception, "Unhandled exception")`, set status 500, write a generic `ProblemDetails`, and return `true`. Returning `false` would pass execution to the next registered handler — useful when a specialized `PaymentExceptionHandler` handles only payment-specific exceptions and returns `false` for everything else so the global fallback handles the rest. Register in `Program.cs` with `builder.Services.AddExceptionHandler<GlobalExceptionHandler>()` and activate with `app.UseExceptionHandler()`. The order of `AddExceptionHandler` registrations determines precedence — first registration runs first.

```csharp
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken ct)
    {
        var (status, title) = exception switch
        {
            NotFoundException => (404, "Not Found"),
            _ => (500, "An error occurred while processing your request.")
        };
        if (status == 500)
            _logger.LogError(exception, "Unhandled exception");

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status, Title = title,
            Detail = status == 404 ? exception.Message : null
        }, ct);
        return true;
    }
}
```

---

#### Q7. (R) Two teams merged their APIs. Support tickets report "sometimes 404 is JSON ProblemDetails, sometimes plain text." Review these endpoints for **status code and body consistency**.

```csharp
// Team A
[HttpGet("{id}")]
public IActionResult Get(int id)
{
    var item = _repo.Find(id);
    if (item is null) return NotFound(); // default ProblemDetails when ApiController
    return Ok(item);
}

// Team B — same host, different controller base
public class LegacyReportsController : ControllerBase // no [ApiController]
{
    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var report = _repo.Find(id);
        if (report is null) return NotFound($"Report {id} not found");
        return Ok(report);
    }
}
```

**Concepts**
- [ApiController] converting bare NotFound() to ProblemDetails JSON
- Missing [ApiController] causing NotFound(string) to return text/plain
- Content-Type inconsistency between controllers on the same host
- Assembly-level [ApiController] for uniform behavior across all controllers
- NotFound(new ProblemDetails {...}) as explicit fix regardless of attribute

**Answer**

Team A's controller has `[ApiController]`, so `NotFound()` without arguments returns a ProblemDetails JSON body with status 404 — the convention intercepts the bare not-found result and serializes it to `application/problem+json`. Team B's `LegacyReportsController` has no `[ApiController]` attribute, so `NotFound($"Report {id} not found")` returns a **plain string** as the response body with `Content-Type: text/plain` — the string overload of `NotFound` does not produce ProblemDetails regardless of any attribute. The result is two controllers on the same host where identical HTTP status codes (404) produce incompatible body types, which breaks any RFC 7807-aware client that calls both endpoints and expects JSON. The fix is to add `[ApiController]` to `LegacyReportsController` (or apply it assembly-wide) and replace `NotFound($"Report {id} not found")` with `NotFound(new ProblemDetails { Title = "Not Found", Detail = $"Report {id} not found" })` or with a `NotFoundException` that the global handler maps to 404. With `[ApiController]` on both controllers and explicit ProblemDetails arguments on the not-found returns, both produce consistent JSON.

---

#### Q8. (M) Your API uses `[ApiController]` automatic validation responses. A partner sends `POST /api/customers` with an invalid email. Walk through the pipeline: who produces the response, what HTTP status is used (400 vs 422), and how does **`ValidationProblemDetails`** differ from a hand-written `{ errors: [...] }` object?

**Concepts**
- ModelStateInvalidFilter producing the response before action body runs
- 400 Bad Request as the default [ApiController] validation status
- ValidationProblemDetails with RFC 7807 fields plus errors dictionary
- errors dictionary keyed per field vs flat message array
- OpenAPI-generated clients deserializing ValidationProblemDetails automatically

**Answer**

When the partner sends `POST /api/customers` with an invalid email, the `[ApiController]` attribute activates the `ModelStateInvalidFilter` — the model binder reads and validates the request body, marks `ModelState` invalid because the email fails `[EmailAddress]` validation, and the filter returns a **400 Bad Request** before the action method body ever executes. The response is `ValidationProblemDetails` serialized as `application/problem+json`, with `status: 400`, `title: "One or more validation errors occurred."`, and an `errors` dictionary like `{ "email": ["The Email field is not a valid e-mail address."] }`. The key differences from a hand-written `{ errors: [...] }` object are: the shape is standardized by RFC 7807 so any RFC 7807 client deserializes it without custom code; the `errors` dictionary is keyed per field rather than a flat array, enabling per-field inline display; the `status`, `title`, and `type` fields are present for programmatic handling; and OpenAPI documents `ValidationProblemDetails` as the 400 response schema so generated clients get typed error access automatically. A hand-written response requires the controller to check `ModelState`, build the shape manually, and document it separately — all of which diverge from team to team.

---

#### Q9. (D) Design a **status-code mapping table** for domain exceptions in a REST API (`ValidationException` → 400, `NotFoundException` → 404, duplicate → 409, forbidden business rule → 403, unexpected → 500). Where should mapping live so controllers stay thin, logs retain full exceptions, and OpenAPI documents the correct response schemas per status?

**Concepts**
- IExceptionHandler as the single HTTP mapping point for domain exceptions
- Domain exception types conveying intent without HTTP coupling in services
- Logging full exceptions only at the 500 path to avoid noise
- OpenAPI ProducesResponseType attributes on controllers aligned to handler mapping
- ValidateOnBuild and scope validation surfacing misconfiguration early

**Answer**

The mapping should live exclusively in `IExceptionHandler` — services throw typed domain exceptions without HTTP knowledge, and the handler applies the status mapping in one place so every endpoint gets identical behavior. The mapping is: `ValidationException` → 400 with `ValidationProblemDetails` (populate `errors` from the exception's field-level messages); `NotFoundException` → 404 with `ProblemDetails` and a safe `detail`; `DuplicateEntityException` or `ConflictException` → 409 with `ProblemDetails`; `ForbiddenOperationException` → 403 with `ProblemDetails`; everything else → 500 with generic detail logged at `LogError(exception, ...)` with the full exception object. Controllers catch nothing — they throw typed exceptions that bubble to `UseExceptionHandler()` where the handler runs, which means controllers stay thin on HTTP concerns. Full exception details (stack trace, inner exception, context) go to the server log only at the 500 path; 4xx handlers log at `LogWarning` since those are expected domain outcomes, not server faults. OpenAPI documents the correct schema per status by adding `[ProducesResponseType(typeof(ValidationProblemDetails), 400)]`, `[ProducesResponseType(typeof(ProblemDetails), 404)]`, and so on at the controller or action level, so generated clients get typed access to the error body for each path without custom handling.
