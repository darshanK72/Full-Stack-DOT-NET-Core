# Problem Details & Error Responses — Interview Q&A
> Back to [README](../README.md)

## Table of Contents

- [Chapter 09. Problem Details & Error Responses](#chapter-09-problem-details-error-responses)
  - [Q1. What is RFC 7807 Problem Details?](#chapter-09-problem-details-error-responses-q1)
  - [Q2. What is the `ProblemDetails` class in ASP.NET Core?](#chapter-09-problem-details-error-responses-q2)
  - [Q3. What is `ValidationProblemDetails`?](#chapter-09-problem-details-error-responses-q3)
  - [Q4. What is the difference between `ProblemDetails` and a custom…](#chapter-09-problem-details-error-responses-q4)
  - [Q5. What HTTP status code does `[ApiController]` return for vali…](#chapter-09-problem-details-error-responses-q5)
  - [Q6. What is centralized exception handling for Web APIs?](#chapter-09-problem-details-error-responses-q6)
  - [Q7. What is `IExceptionHandler` in .NET 8?](#chapter-09-problem-details-error-responses-q7)
  - [Q8. What should Production error responses exclude?](#chapter-09-problem-details-error-responses-q8)
  - [Q9. What is the difference between 400 Bad Request and 404 Not F…](#chapter-09-problem-details-error-responses-q9)
  - [Q10. When should an API return 409 Conflict?](#chapter-09-problem-details-error-responses-q10)
  - [Q11. What is the `type` field in ProblemDetails?](#chapter-09-problem-details-error-responses-q11)
  - [Q12. What is the `title` field in ProblemDetails?](#chapter-09-problem-details-error-responses-q12)
  - [Q13. What is the `detail` field in ProblemDetails?](#chapter-09-problem-details-error-responses-q13)
  - [Q14. What is the difference between Development and Production er…](#chapter-09-problem-details-error-responses-q14)
  - [Q15. What is `AddProblemDetails()` used for?](#chapter-09-problem-details-error-responses-q15)
  - [Q16. What is the `errors` dictionary in `ValidationProblemDetails…](#chapter-09-problem-details-error-responses-q16)
  - [Q17. What is the difference between returning `NotFound()` and a …](#chapter-09-problem-details-error-responses-q17)
  - [Q18. How do API clients reliably parse validation errors?](#chapter-09-problem-details-error-responses-q18)
- [Gotchas](#gotchas)
- [Scenario-Based Questions](#scenario-based-questions-karat-format)

---

## Chapter 09. Problem Details & Error Responses

### Q1. What is RFC 7807 Problem Details? {#chapter-09-problem-details-error-responses-q1}

What is RFC 7807 Problem Details?

**Answer:** RFC 7807 defines a standard JSON (or XML) format for HTTP API error responses — a machine-readable object with fields like `type`, `title`, `status`, `detail`, and `instance` so clients can parse errors consistently across endpoints and services.

- It replaces ad hoc shapes like `{ "error": "something went wrong" }` with a predictable contract.
- The `Content-Type` is typically `application/problem+json` (or `application/json` with the same schema).
- Extensions are allowed — ASP.NET Core adds `traceId` and validation uses an `errors` dictionary.
- Problem Details describe the error itself, not the successful resource representation.
- ASP.NET Core 8 maps `ProblemDetails` and `ValidationProblemDetails` classes directly to this standard.

---

### Q2. What is the `ProblemDetails` class in ASP.NET Core? {#chapter-09-problem-details-error-responses-q2}

What is the `ProblemDetails` class in ASP.NET Core?

**Answer:** `ProblemDetails` is the built-in ASP.NET Core model for RFC 7807 error payloads. Controller helpers like `NotFound()`, `BadRequest()`, and `Results.Problem()` serialize it to JSON with standard fields populated from the HTTP status and your message.

- Properties include `Type`, `Title`, `Status`, `Detail`, and `Instance` (usually the request path).
- `Extensions` is a dictionary for custom fields such as `traceId` without breaking the standard shape.
- Works in MVC controllers (`ControllerBase.Problem()`) and minimal APIs (`Results.Problem()`).
- OpenAPI/Swashbuckle can document `ProblemDetails` as the response schema for 4xx/5xx codes.
- Register `AddProblemDetails()` in .NET 8 to customize defaults and environment-specific behavior globally.

---

### Q3. What is `ValidationProblemDetails`? {#chapter-09-problem-details-error-responses-q3}

What is `ValidationProblemDetails`?

**Answer:** `ValidationProblemDetails` extends `ProblemDetails` with an `Errors` property — an `IDictionary<string, string[]>` mapping field names to one or more validation messages. ASP.NET Core returns it automatically when model validation fails on an `[ApiController]`.

- Inherits all RFC 7807 fields (`title`, `status`, `type`, etc.) plus field-scoped errors.
- Produced by the `ModelStateInvalidFilter` before the action method runs when `ModelState` is invalid.
- Default HTTP status is **400 Bad Request** unless you configure `InvalidModelStateResponseFactory` for 422.
- Keys match model property names (respecting JSON naming policy — camelCase by default).
- Clients iterate `errors["Email"]` to show inline form validation without custom parsing logic.

---

### Q4. What is the difference between `ProblemDetails` and a custom `{ error: "..." }` object? {#chapter-09-problem-details-error-responses-q4}

What is the difference between `ProblemDetails` and a custom `{ error: "..." }` object?

**Answer:** `ProblemDetails` follows an industry-standard schema with typed fields, HTTP status alignment, and OpenAPI documentation support. A custom `{ error: "..." }` object forces every client to implement a one-off parser and breaks consistency across endpoints.

- Problem Details include `status`, `title`, `type`, and `instance` — not just a message string.
- Validation errors use a structured `errors` dictionary instead of a flat message array.
- Standard shape works with generated API clients, API gateways, and monitoring tools.
- Custom objects often omit `traceId`/correlation, making support harder in Production.
- Mixing both shapes on one API (some endpoints ProblemDetails, others custom) is a common integration failure.

---

### Q5. What HTTP status code does `[ApiController]` return for validation failures? {#chapter-09-problem-details-error-responses-q5}

What HTTP status code does `[ApiController]` return for validation failures?

**Answer:** By default, `[ApiController]` returns **400 Bad Request** with a `ValidationProblemDetails` body when model binding or data annotation validation fails. The action method does not execute.

- The `ModelStateInvalidFilter` short-circuits the pipeline when `ModelState.IsValid` is false.
- Response body is JSON with RFC 7807 fields plus an `errors` dictionary keyed by property name.
- Teams can switch to **422 Unprocessable Entity** via `ConfigureApiBehaviorOptions` and `InvalidModelStateResponseFactory`.
- This path is separate from exception middleware — validation is not an unhandled exception.
- Document the chosen status (400 vs 422) consistently in OpenAPI for client generators.

---

### Q6. What is centralized exception handling for Web APIs? {#chapter-09-problem-details-error-responses-q6}

What is centralized exception handling for Web APIs?

**Answer:** Centralized exception handling catches unhandled exceptions in one place — middleware or `IExceptionHandler` — maps them to appropriate HTTP status codes, and returns `ProblemDetails` JSON instead of scattering try/catch blocks in every controller.

- Controllers and services throw domain exceptions (`NotFoundException`, `ConflictException`) without HTTP knowledge.
- A single handler converts exception types to 404, 409, 400, or 500 with a uniform JSON body.
- Logging with the full exception object and `TraceIdentifier` happens in the handler, not in controllers.
- Eliminates inconsistent `{ error = ex.Message }` patterns and wrong status codes (e.g., validation as 500).
- Register `AddExceptionHandler<T>()` and `UseExceptionHandler()` early in the pipeline in ASP.NET Core 8.

---

### Q7. What is `IExceptionHandler` in .NET 8? {#chapter-09-problem-details-error-responses-q7}

What is `IExceptionHandler` in .NET 8?

**Answer:** `IExceptionHandler` is the .NET 8+ interface for pluggable global exception handling. Implement `TryHandleAsync(HttpContext, Exception, CancellationToken)` — return `true` if your handler wrote the response, `false` to delegate to the next handler.

- Register with `builder.Services.AddExceptionHandler<GlobalExceptionHandler>()`.
- Pair with `app.UseExceptionHandler()` — no custom path required when handlers are registered.
- Multiple handlers can be registered; the first returning `true` wins.
- Replaces much of the custom exception middleware pattern from earlier ASP.NET Core versions.
- Handler writes `ProblemDetails` via `WriteAsJsonAsync` or `IProblemDetailsService`.

---

### Q8. What should Production error responses exclude? {#chapter-09-problem-details-error-responses-q8}

What should Production error responses exclude?

**Answer:** Production error responses must not expose stack traces, exception types with internal code paths, file paths, connection strings, SQL fragments, or environment-specific configuration. Clients get a safe generic message plus a correlation id for support lookup.

- Log full exception details server-side with `LogError(ex, ...)` including stack and inner exceptions.
- `Detail` should be user-safe — "An unexpected error occurred" for 500, specific but non-sensitive text for 404/409.
- Never return raw `ex.Message` from infrastructure exceptions (database, file system) to external callers.
- Use `IHostEnvironment.IsDevelopment()` in ProblemDetails customization for optional diagnostic extensions locally only.
- Include `traceId` or `requestId` in Production so support can correlate client reports to logs without exposing internals.

---

### Q9. What is the difference between 400 Bad Request and 404 Not Found for APIs? {#chapter-09-problem-details-error-responses-q9}

What is the difference between 400 Bad Request and 404 Not Found for APIs?

**Answer:** **400 Bad Request** means the client sent a syntactically or semantically invalid request — bad JSON, failed validation, or malformed parameters. **404 Not Found** means the request was well-formed but the target resource (or route) does not exist.

- Validation failures and invalid IDs in the wrong format typically return 400 with `ValidationProblemDetails`.
- A valid route with a non-existent entity id (e.g., `GET /api/orders/99999`) returns 404 with `ProblemDetails`.
- Returning 404 for invalid query parameters can mislead clients into thinking the resource type is missing.
- Returning 400 when a resource simply does not exist breaks REST semantics and caching expectations.
- Both should use ProblemDetails JSON on `[ApiController]` APIs for a consistent client experience.

---

### Q10. When should an API return 409 Conflict? {#chapter-09-problem-details-error-responses-q10}

When should an API return 409 Conflict?

**Answer:** Return **409 Conflict** when the request is valid but cannot be applied due to a state conflict with the current resource — duplicate unique key, optimistic concurrency failure, or a business rule violation like "order already shipped."

- Duplicate email on registration, conflicting ETag/version on PUT, or "cannot delete active subscription."
- Distinct from 400 (invalid input) and 404 (resource missing) — the resource often exists but the operation conflicts.
- Return `ProblemDetails` with a clear `title` ("Conflict") and safe `detail` explaining the conflict without internal ids.
- Map `DbUpdateConcurrencyException` from EF Core to 409 when using row versioning.
- Document 409 in OpenAPI so clients handle retries or user messaging appropriately.

---

### Q11. What is the `type` field in ProblemDetails? {#chapter-09-problem-details-error-responses-q11}

What is the `type` field in ProblemDetails?

**Answer:** The `type` field is a URI reference that identifies the problem category — often a stable URL pointing to documentation about that error type. It helps clients branch logic programmatically without parsing free-text messages.

- Example: `"type": "https://tools.ietf.org/html/rfc7231#section-6.5.1"` for a generic 400, or a company-specific URI like `https://api.example.com/errors/validation`.
- ASP.NET Core sets a default type based on status code when you use built-in helpers.
- Clients should treat it as an identifier, not a URL users must visit.
- Custom exception handlers can set `type` per domain error for machine-readable classification.
- OpenAPI can reference problem types in response documentation for partner integrations.

---

### Q12. What is the `title` field in ProblemDetails? {#chapter-09-problem-details-error-responses-q12}

What is the `title` field in ProblemDetails?

**Answer:** The `title` field is a short, human-readable summary of the problem type — independent of the specific occurrence. It should be stable for a given error category and safe to display in UI headers or toast notifications.

- Examples: "Bad Request", "Not Found", "Validation Failed", "Conflict".
- Unlike `detail`, `title` typically does not include entity-specific ids or user input.
- ASP.NET Core populates it from status code defaults or your handler mapping.
- Localization can target `title` for multi-language API consumers.
- Pair with `status` so clients can use either HTTP status or title for display logic.

---

### Q13. What is the `detail` field in ProblemDetails? {#chapter-09-problem-details-error-responses-q13}

What is the `detail` field in ProblemDetails?

**Answer:** The `detail` field explains this specific occurrence of the problem — what went wrong for this request. It may include safe contextual information such as "Customer with id 42 was not found" but must not leak stack traces or secrets in Production.

- More specific than `title`; varies per request while `title` stays constant for the error class.
- For validation errors, field-level messages live in `errors`; `detail` may summarize ("One or more validation errors occurred").
- Sanitize in Production — generic text for 500, specific but non-sensitive text for 404/409.
- Log the full exception detail server-side even when the response `detail` is generic.
- Avoid echoing raw user input in `detail` to prevent reflected XSS in clients that render error text as HTML.

---

### Q14. What is the difference between Development and Production error responses? {#chapter-09-problem-details-error-responses-q14}

What is the difference between Development and Production error responses?

**Answer:** Development responses may include richer diagnostics — exception type names, stack traces in extension fields, or Developer Exception Page for browser traffic. Production responses return safe, minimal ProblemDetails with correlation ids while full diagnostics go to logs only.

- Both environments should use the same HTTP status code mapping for the same exception type.
- Development: `ProblemDetails.Extensions["stackTrace"]` or similar behind `IsDevelopment()` checks.
- Production: generic 500 `detail`, no file paths, no connection string fragments, no inner exception chains in JSON.
- Use `AddProblemDetails()` customization or `IExceptionHandler` — not `#if DEBUG` scattered in controllers.
- Logging level and content are the same or stricter in Production; only the response body differs.

---

### Q15. What is `AddProblemDetails()` used for? {#chapter-09-problem-details-error-responses-q15}

What is `AddProblemDetails()` used for?

**Answer:** `AddProblemDetails()` registers services that configure RFC 7807 Problem Details generation globally in ASP.NET Core 8 — default field values, customization delegates, and integration with exception handling and status code pages.

- Call `builder.Services.AddProblemDetails(options => { ... })` in `Program.cs`.
- Customize `options.CustomizeProblemDetails` to add `traceId`, sanitize `detail`, or add Development-only extensions.
- Works with `IProblemDetailsService` for consistent ProblemDetails creation across middleware and endpoints.
- Complements `AddExceptionHandler<T>()` — the handler can use injected problem details services.
- Ensures minimal APIs and controllers produce the same error shape when using `Results.Problem()`.

---

### Q16. What is the `errors` dictionary in `ValidationProblemDetails`? {#chapter-09-problem-details-error-responses-q16}

What is the `errors` dictionary in `ValidationProblemDetails`?

**Answer:** The `errors` property is an `IDictionary<string, string[]>` where each key is a model property or field name and each value is an array of validation error messages for that field. It enables clients to show per-field form errors without parsing a single combined string.

- Keys follow JSON naming policy — camelCase by default (`email`, not `Email`).
- Multiple messages per field are supported (e.g., `[Required]` and `[EmailAddress]` both failing).
- Empty or missing keys mean no error on that field; clients should only highlight keys present in `errors`.
- Global/model-level errors may use an empty string key `""` for errors not tied to one property.
- Stable parsing contract: read `response.errors.fieldName[0]` rather than regex on `detail`.

---

### Q17. What is the difference between returning `NotFound()` and a custom ProblemDetails for 404? {#chapter-09-problem-details-error-responses-q17}

What is the difference between returning `NotFound()` and a custom ProblemDetails for 404?

**Answer:** On an `[ApiController]`, `NotFound()` without arguments returns a ProblemDetails body with status 404 automatically. `NotFound(string message)` returns **text/plain** with the string as body — same status, incompatible JSON contract. Explicit `NotFound(new ProblemDetails { ... })` gives full control over all fields.

- `[ApiController]` convention converts bare `NotFound()` to RFC 7807 JSON.
- Without `[ApiController]`, `NotFound()` behavior differs — often empty body or negotiated content type.
- Custom ProblemDetails lets you set `type`, `detail`, and extensions consistently with your global handler.
- Minimal APIs use `Results.Problem(statusCode: 404, ...)` or `TypedResults.NotFound()` depending on version/conventions.
- Standardize one approach across all controllers to avoid "sometimes JSON, sometimes plain text" support tickets.

---

### Q18. How do API clients reliably parse validation errors? {#chapter-09-problem-details-error-responses-q18}

How do API clients reliably parse validation errors?

**Answer:** Clients should expect HTTP **400** (or team-standard **422**), deserialize the body as `ValidationProblemDetails`, read the top-level `status` and `title`, then iterate the `errors` dictionary mapping property names to message arrays.

- Do not assume a custom `{ error: "..." }` wrapper — check `Content-Type` and ProblemDetails shape.
- Use code-generated clients from OpenAPI where the `ValidationProblemDetails` schema is documented.
- Handle model-level errors under key `""` or a documented global key.
- Fall back gracefully if an endpoint returns non-ProblemDetails 400 from legacy code — but push server teams to unify.
- Include `traceId` from extensions in client error reports for support correlation.

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

---

**Answer:**

**Answer:** The catch block destroys observability by logging only `ex.Message` (no stack trace or inner exception) and rethrowing with `throw ex`, which resets the stack trace to this catch site. The global handler still runs, but support cannot tie logs to the original gateway failure.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Observability | `LogError(ex.Message)` — exception object not passed | Serilog/App Insights lose stack, inner exceptions, structured `Exception` property |
| Diagnostics | `throw ex` resets stack trace | Root cause appears to be this service method, not `_gateway.ChargeAsync` |
| Design | Catch-log-rethrow in service layer | Duplicates global handler; message-only logs cannot be correlated with ProblemDetails `traceId` if handler logs properly but service does not |

**Fix (priority order):**

1. Remove the try/catch entirely — let domain/gateway exceptions bubble to `IExceptionHandler` or `UseExceptionHandler` (preferred for Web APIs).
2. If local handling is required: `_logger.LogError(ex, "Payment charge failed for {OrderId}", orderId);` then **`throw;`** (preserves stack).
3. Map gateway-specific faults to typed exceptions (`PaymentDeclinedException`) in one place — the global handler — not in every service.
4. Ensure middleware adds/propagates `Activity`/`TraceIdentifier` so client-facing `ProblemDetails.Extensions["traceId"]` matches server logs.

**Production takeaway:** See C# Module 01 — **`throw;` vs `throw ex`**. Web API services should throw typed domain errors; centralized handlers own HTTP shape. Debrief pattern: `LogError(ex.Message)` + `throw ex`.

---

---

#### Q2. (P) You inherit a Web API with per-controller try/catch blocks returning `{ error = ex.Message }`, plain strings for 404, and occasional 500 for validation failures. How do you move to **centralized** exception handling with RFC 7807 `ProblemDetails`, consistent status codes, and different Development vs Production response bodies — without `#if DEBUG` in every controller?

---

**Answer:**

_Answer not found._

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

---

**Answer:**

_Answer not found._

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

---

**Answer:**

**Answer:** Each catch returns a different contract and **`ValidationException` incorrectly maps to 500**. Clients cannot write one deserializer; monitoring alerts treat validation bugs as server faults.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| HTTP semantics | `ValidationException` → 500 | Client retries; SLO violations for user input errors |
| Contract | `NotFound(ex.Message)` — string body, not ProblemDetails | Breaks RFC 7807 clients expecting JSON object |
| Consistency | Conflict returns `ProblemDetails`; others do not | Partner integrations need branch per endpoint |
| Architecture | Controller-level catch duplicates global handler | New endpoints will copy wrong patterns |

**Fix (priority order):**

1. Remove catches for domain exceptions — throw `NotFoundException`, `ValidationException`, `DuplicateOrderException` and handle in `IExceptionHandler`.
2. If staying in controller: all branches return `ProblemDetails` or `ValidationProblemDetails` with correct status (400/404/409).
3. Register `services.AddProblemDetails()` and use `TypedResults.Problem()` / `Results.Problem()` in minimal APIs for the same shape.
4. Align OpenAPI response types with actual `ProblemDetails` schema for every documented status.

**Production takeaway:** Status code consistency matters as much as JSON shape — 500 on validation trains clients to retry forever.

---

---

#### Q5. (D) Compare **Development** vs **Production** error responses for the same unhandled `NullReferenceException` in a minimal API endpoint. What should each environment return (status, `ProblemDetails` fields, stack trace), and what must never appear in an external Production JSON body?

---

**Answer:**

_Answer not found._

---

#### Q6. (P) Implement global handling with **`IExceptionHandler`** (.NET 8+) for a JSON Web API. Show registration in `Program.cs`, a handler that maps `NotFoundException` → 404 and unknown exceptions → 500, and explain what `TryHandleAsync` returning `true` vs `false` does when multiple handlers are registered.

---

**Answer:**

_Answer not found._

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

---

**Answer:**

_Answer not found._

---

#### Q8. (M) Your API uses `[ApiController]` automatic validation responses. A partner sends `POST /api/customers` with an invalid email. Walk through the pipeline: who produces the response, what HTTP status is used (400 vs 422), and how does **`ValidationProblemDetails`** differ from a hand-written `{ errors: [...] }` object?

---

**Answer:**

_Answer not found._

---

#### Q9. (D) Design a **status-code mapping table** for domain exceptions in a REST API (`ValidationException` → 400, `NotFoundException` → 404, duplicate → 409, forbidden business rule → 403, unexpected → 500). Where should mapping live so controllers stay thin, logs retain full exceptions, and OpenAPI documents the correct response schemas per status?



**Answer:**

_Answer not found._

---
