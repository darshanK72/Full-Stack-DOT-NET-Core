# Model Binding & Validation — Interview Q&A
> Back to [README](../README.md)

## Table of Contents

- [Chapter 05. Model Binding & Validation](#chapter-05-model-binding-validation)
  - [Q1. What is model binding in ASP.NET Core Web API?](#chapter-05-model-binding-validation-q1)
  - [Q2. What does `[FromBody]` do?](#chapter-05-model-binding-validation-q2)
  - [Q3. What does `[FromQuery]` do?](#chapter-05-model-binding-validation-q3)
  - [Q4. What does `[FromRoute]` do?](#chapter-05-model-binding-validation-q4)
  - [Q5. What is the difference between `[FromBody]` and `[FromQuery]…](#chapter-05-model-binding-validation-q5)
  - [Q6. How does `[ApiController]` affect automatic model validation…](#chapter-05-model-binding-validation-q6)
  - [Q7. What HTTP status code does automatic validation failure retu…](#chapter-05-model-binding-validation-q7)
  - [Q8. What is `ValidationProblemDetails`?](#chapter-05-model-binding-validation-q8)
  - [Q9. What are data annotations for validation?](#chapter-05-model-binding-validation-q9)
  - [Q10. What is the difference between `[Required]` and optional pro…](#chapter-05-model-binding-validation-q10)
  - [Q11. What is `IValidatableObject`?](#chapter-05-model-binding-validation-q11)
  - [Q12. What is FluentValidation and how does it integrate with Web …](#chapter-05-model-binding-validation-q12)
  - [Q13. Why can't GET requests reliably use `[FromBody]`?](#chapter-05-model-binding-validation-q13)
  - [Q14. What is complex type binding from query strings?](#chapter-05-model-binding-validation-q14)
  - [Q15. What is the difference between model binding and validation?](#chapter-05-model-binding-validation-q15)
  - [Q16. What does `[ValidateNever]` do?](#chapter-05-model-binding-validation-q16)
  - [Q17. What is PATCH semantics for partial updates?](#chapter-05-model-binding-validation-q17)
  - [Q18. How does camelCase JSON map to PascalCase C# properties?](#chapter-05-model-binding-validation-q18)
- [Gotchas](#gotchas)
- [Scenario-Based Questions](#scenario-based-questions-karat-format)

---

## Chapter 05. Model Binding & Validation

### Q1. What is model binding in ASP.NET Core Web API? {#chapter-05-model-binding-validation-q1}

What is model binding in ASP.NET Core Web API?

**Answer:** Model binding is the framework mechanism that maps incoming HTTP request data — route values, query strings, headers, form fields, and JSON bodies — to action method parameters and complex types. In ASP.NET Core 8 Web API, binding runs after routing selects an endpoint and before validation and the action execute.

- The appropriate `IModelBinder` is chosen based on parameter type, HTTP verb, and binding source attributes such as `[FromBody]` and `[FromQuery]`.
- Binding failures add entries to `ModelState` with conversion or source errors keyed by property name.
- `[ApiController]` applies opinionated binding conventions — for example, inferring `[FromBody]` for complex types on POST/PUT/PATCH actions.
- Input formatters (typically `System.Text.Json`) deserialize JSON bodies; simple types bind from route and query strings as strings converted to CLR types.

---

### Q2. What does `[FromBody]` do? {#chapter-05-model-binding-validation-q2}

What does `[FromBody]` do?

**Answer:** `[FromBody]` tells model binding to deserialize the HTTP request body into the parameter, typically from JSON via `System.Text.Json`. ASP.NET Core 8 allows one `[FromBody]` parameter per action by default because the body is a single stream read once.

- Used on POST, PUT, and PATCH actions for create/update DTOs such as `CreateOrderRequest`.
- The JSON input formatter reads the body stream and maps properties to the target type using the configured naming policy (camelCase by default).
- Malformed JSON or type mismatches fail during formatting and return 400 before the action runs.
- `[ApiController]` infers `[FromBody]` for complex types on actions without an explicit attribute when no other binding source applies.

---

### Q3. What does `[FromQuery]` do? {#chapter-05-model-binding-validation-q3}

What does `[FromQuery]` do?

**Answer:** `[FromQuery]` binds a parameter from the URL query string — the key-value pairs after `?` in the request URI. Simple types and complex filter objects both bind from query keys matching property names.

- Example: `GET /api/products?page=2&category=books` binds `page` and `category` to action parameters or a filter DTO.
- Complex types use prefix notation — `sort.field=createdAt&sort.descending=true` binds nested properties.
- `[ApiController]` infers `[FromQuery]` for simple types (int, string, bool) on GET actions automatically.
- Query binding is the correct source for search, filter, and pagination parameters on GET endpoints.

---

### Q4. What does `[FromRoute]` do? {#chapter-05-model-binding-validation-q4}

What does `[FromRoute]` do?

**Answer:** `[FromRoute]` binds a parameter from values captured by the route template in the URI path. Route parameters defined in `[Route]` or `[HttpGet("{id}")]` map to method parameters marked or inferred as route-bound.

- Example: `GET /api/orders/42` with template `{id:int}` binds `id = 42` to an `int id` parameter.
- Route constraints such as `{id:int}` reject invalid values at routing time with 404 before the action executes.
- `[ApiController]` infers `[FromRoute]` for action parameters whose names match route template tokens.
- Explicit `[FromRoute]` is required when the parameter name differs from the route token or ambiguity exists.

---

### Q5. What is the difference between `[FromBody]` and `[FromQuery]`? {#chapter-05-model-binding-validation-q5}

What is the difference between `[FromBody]` and `[FromQuery]`?

**Answer:** `[FromBody]` reads the request body (typically JSON) and `[FromQuery]` reads URL query string parameters. They represent different parts of the HTTP request and suit different HTTP methods and payload sizes.

- `[FromBody]` is for request payloads on POST, PUT, and PATCH — one body stream per request.
- `[FromQuery]` is for filters, pagination, and simple parameters on GET — visible in the URL and cacheable.
- Using `[FromBody]` on GET is unreliable because clients and proxies often ignore GET bodies.
- Binding the same business field from both sources creates ambiguous contracts — choose one authoritative source per field.

---

### Q6. How does `[ApiController]` affect automatic model validation? {#chapter-05-model-binding-validation-q6}

How does `[ApiController]` affect automatic model validation?

**Answer:** `[ApiController]` enables automatic model validation that short-circuits to HTTP 400 with `ValidationProblemDetails` when `ModelState` is invalid, without manual `if (!ModelState.IsValid)` checks. It also applies binding source inference for route, query, and body parameters.

- DataAnnotations, `IValidatableObject`, and FluentValidation all run after binding and populate `ModelState` on failure.
- Invalid models never reach the action method body — a filter returns 400 before execution.
- Binding source inference reduces boilerplate `[FromQuery]` and `[FromBody]` attributes on conventional API actions.
- Customize via `ConfigureApiBehaviorOptions` — for example, `InvalidModelStateResponseFactory` for custom error shapes.

---

### Q7. What HTTP status code does automatic validation failure return? {#chapter-05-model-binding-validation-q7}

What HTTP status code does automatic validation failure return?

**Answer:** `[ApiController]` returns **400 Bad Request** when automatic model validation fails. The response body is `ValidationProblemDetails` serialized as `application/problem+json`.

- The `errors` dictionary maps property names to arrays of validation messages for client-side form feedback.
- Malformed JSON also returns 400, but with a different problem type than field-level validation failures.
- Binding conversion failures on route/query parameters may produce 400 or 404 depending on route constraints.
- Consistent 400 responses across endpoints let SPAs and generated SDKs parse errors uniformly.

---

### Q8. What is `ValidationProblemDetails`? {#chapter-05-model-binding-validation-q8}

What is `ValidationProblemDetails`?

**Answer:** `ValidationProblemDetails` extends RFC 7807 `ProblemDetails` with an `errors` dictionary for field-level validation failures. ASP.NET Core 8 uses it as the default 400 response body for invalid models on `[ApiController]` actions.

- Standard fields include `type`, `title`, `status` (400), `detail`, and `instance`.
- The `errors` object maps property names (camelCase in JSON) to string arrays of messages — e.g., `"email": ["The Email field is required."]`.
- Content type is `application/problem+json`, aligning Web API error contracts with RFC 7807.
- Clients parse `errors` for per-field UI feedback instead of custom `{ message: "..." }` wrappers.

---

### Q9. What are data annotations for validation? {#chapter-05-model-binding-validation-q9}

What are data annotations for validation?

**Answer:** Data annotations are declarative attributes on model properties — such as `[Required]`, `[StringLength]`, `[Range]`, `[EmailAddress]`, and `[RegularExpression]` — evaluated during model validation after binding succeeds.

- `[Required]` ensures a value was supplied — essential for non-nullable reference types when nullable reference types are enabled.
- `[Range(1, 100)]` and `[StringLength(200)]` enforce numeric bounds and string length limits.
- Annotations are property-scoped — cross-field rules require `IValidatableObject` or FluentValidation.
- Validation runs automatically on `[ApiController]` actions without explicit validation calls in the action method.

---

### Q10. What is the difference between `[Required]` and optional properties? {#chapter-05-model-binding-validation-q10}

What is the difference between `[Required]` and optional properties?

**Answer:** `[Required]` marks a property as mandatory — validation fails if the value is null, empty string, or missing after binding. Optional properties accept null or default values without triggering a required-field error.

- With nullable reference types enabled, non-nullable `string Name` is implicitly required; nullable `string? Nickname` is optional.
- Value types like `int` are never null — use `int?` for optional numeric fields or provide a default.
- Optional properties omitted from JSON bind as `null` (reference types) or `default` (value types) — not as validation failures unless constrained.
- `[Required]` validates after binding — a missing JSON property and an explicit `null` both fail required checks on non-nullable types.

---

### Q11. What is `IValidatableObject`? {#chapter-05-model-binding-validation-q11}

What is `IValidatableObject`?

**Answer:** `IValidatableObject` is an interface implemented on a model class to perform cross-property validation in a `Validate(ValidationContext)` method. It runs during the same validation pass as DataAnnotations and adds errors to `ModelState`.

- Use when one property's validity depends on another — for example, `EndDate >= StartDate` or `GuestCount <= RoomCapacity`.
- Return `ValidationResult` instances with optional member names to attach errors to specific properties.
- Errors appear in `ValidationProblemDetails.errors` identically to `[Required]` failures.
- Suitable for a few cross-field rules — larger rule sets belong in FluentValidation validators.

---

### Q12. What is FluentValidation and how does it integrate with Web APIs? {#chapter-05-model-binding-validation-q12}

What is FluentValidation and how does it integrate with Web APIs?

**Answer:** FluentValidation defines validation rules in separate `AbstractValidator<T>` classes using a fluent API instead of attributes on DTOs. Register it with `AddFluentValidationAutoValidation()` to integrate with ASP.NET Core 8 model validation and `ModelState`.

- Validators live in dedicated classes — DTOs stay clean without validation attributes.
- Supports conditional logic, async rules, and reusable rule sets more comfortably than DataAnnotations alone.
- Errors merge into `ModelState` and return as `ValidationProblemDetails` when `[ApiController]` is enabled.
- Do not duplicate the same rule in both DataAnnotations and FluentValidation on the same DTO.

---

### Q13. Why can't GET requests reliably use `[FromBody]`? {#chapter-05-model-binding-validation-q13}

Why can't GET requests reliably use `[FromBody]`?

**Answer:** HTTP does not forbid a body on GET, but browsers, HTTP clients, proxies, and caches commonly ignore or strip GET bodies. ASP.NET Core 8 does not bind `[FromBody]` on GET by default, so filters sent as JSON in GET requests fail silently.

- Search and filter parameters on GET should use `[FromQuery]` or a complex type bound from the query string.
- Heavy or sensitive filter payloads belong on `POST /search` with `[FromBody]` when the payload is too large for URLs.
- GET with a body breaks HTTP caching semantics — intermediaries may not forward the body to the origin server.
- Integration tests against localhost may mask the issue that production clients and CDNs drop GET bodies.

---

### Q14. What is complex type binding from query strings? {#chapter-05-model-binding-validation-q14}

What is complex type binding from query strings?

**Answer:** Complex types bind from query strings using prefix notation — property names become query keys such as `filter.category=books&filter.minPrice=10` for a nested filter object. ASP.NET Core 8 binds public settable properties case-insensitively for `[FromQuery]` types.

- Nested objects use dot notation — `sort.field=createdAt&sort.descending=true`.
- Some frontends send bracket notation (`sort[field]`) which does not bind by default without custom model binders.
- Deep nesting in query strings is fragile — flatten to `sortField` and `sortDescending` for public APIs.
- `[ApiController]` applies `[FromQuery]` inference for complex types on GET when no other source is specified.

---

### Q15. What is the difference between model binding and validation? {#chapter-05-model-binding-validation-q15}

What is the difference between model binding and validation?

**Answer:** Model binding maps raw request data to CLR types and populates action parameters. Validation checks whether bound values satisfy business rules and constraints after binding succeeds.

- Binding errors occur when data cannot be converted — invalid JSON syntax, type conversion failure, or wrong binding source.
- Validation errors occur when binding succeeds but DataAnnotations, `IValidatableObject`, or FluentValidation reject the values.
- Both contribute to `ModelState`, but binding failures on unbound properties may prevent validation from running on those properties.
- Route constraints push some binding failures to 404 before `ModelState` is populated — for example, non-integer `{id}` values.

---

### Q16. What does `[ValidateNever]` do? {#chapter-05-model-binding-validation-q16}

What does `[ValidateNever]` do?

**Answer:** `[ValidateNever]` excludes a property from the validation pipeline — validators such as `[Required]` do not run on that property or its children. The property still binds if present in the request.

- Apply to EF Core navigation properties accidentally exposed on API DTOs to prevent validating entire object graphs.
- Prevents over-validation of nested graphs on PATCH endpoints that bind partial updates.
- Useful when a property is populated server-side and should not be validated from inbound client data.
- Use deliberately — excluding sensitive fields from validation can allow invalid data through if binding still occurs.

---

### Q17. What is PATCH semantics for partial updates? {#chapter-05-model-binding-validation-q17}

What is PATCH semantics for partial updates?

**Answer:** PATCH applies partial updates — only fields present in the request should change; omitted fields remain unchanged. This requires nullable types (`bool?`, `string?`, `DateOnly?`) or dedicated patch DTOs to distinguish "omit (unchanged)" from "explicit null or false (set value)".

- Non-nullable `bool` cannot represent three states — omitted JSON deserializes as `default(false)`, conflating "unchanged" and "explicit false".
- Service logic applies updates only when patch properties `HasValue` or are explicitly present in the payload.
- `JsonPatchDocument<T>` (RFC 6902 JSON Patch) provides operation-based partial updates as an alternative to nullable DTO fields.
- OpenAPI should mark patch properties as `nullable: true` so generated clients send omission correctly.

---

### Q18. How does camelCase JSON map to PascalCase C# properties? {#chapter-05-model-binding-validation-q18}

How does camelCase JSON map to PascalCase C# properties?

**Answer:** ASP.NET Core 8 Web API defaults to `JsonNamingPolicy.CamelCase` in `System.Text.Json` — a C# property `CustomerName` serializes as `"customerName"` and expects the same key on deserialization. Property name matching is case-sensitive by default.

- Outbound responses use camelCase keys; inbound JSON must use camelCase unless configured otherwise.
- PascalCase JSON keys from legacy clients do not bind unless `PropertyNameCaseInsensitive = true` is set in `AddJsonOptions`.
- `[JsonPropertyName("custom_key")]` overrides naming for individual properties — document exceptions in OpenAPI.
- Silent data loss occurs when clients send PascalCase keys — properties bind as default values (null, 0, false) without validation errors unless `[Required]` catches empties.

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

#### Q1. (R) Review this Web API POST action. A legacy mobile client sends PascalCase JSON; the React SPA sends camelCase. `CreditLimit` is always 0 for one client group.

```csharp
[HttpPost]
public IActionResult CreateCustomer([FromBody] CreateCustomerRequest request)
{
    return CreatedAtAction(nameof(Get), new { id = request.Id }, request);
}

public class CreateCustomerRequest
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = "";
    public decimal CreditLimit { get; set; }
}

// Mobile:  { "CustomerName": "Acme", "CreditLimit": 5000 }
// Web:     { "customerName": "Acme", "creditLimit": 5000 }
```

`Program.cs` uses default ASP.NET Core 8 Web API JSON options with no custom `AddJsonOptions`.

---

**Answer:**

_Answer not found._

---

#### Q2. (D) A PATCH endpoint updates notification preferences on a user profile API. Product requires three states: omit field = unchanged, `true` = opt-in, `false` = explicit opt-out. Review the DTO and OpenAPI contract.

```csharp
public class PatchNotificationPreferences
{
    public bool EmailAlerts { get; set; }
    public bool SmsAlerts { get; set; }
}

// PATCH /api/users/{id}/preferences
// Body omitting EmailAlerts — must NOT change stored value
// Body { "emailAlerts": false } — must record explicit opt-out
```

---

**Answer:**

_Answer not found._

---

#### Q3. (R) Review this catalog search endpoint on a Web API. Query string is visible in browser dev tools but `ProductFilter` binds empty.

```csharp
[HttpGet("search")]
public IActionResult Search([FromBody] ProductFilter filter)
{
    return Ok(_catalog.Search(filter));
}

public class ProductFilter
{
    public string? Category { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}
```

Request: `GET /api/products/search?category=electronics&minPrice=10&maxPrice=500`

---

**Answer:**

_Answer not found._

---

#### Q4. (R) Review validation on a registration endpoint. The Angular client expects RFC 7807 `ValidationProblemDetails` with per-field errors; the API returns plain text 400.

```csharp
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest("Validation failed");

        return Ok(_users.Register(request));
    }
}

public class RegisterRequest
{
    [Required][EmailAddress] public string Email { get; set; } = "";
    [Required][MinLength(8)] public string Password { get; set; } = "";
}
```

Controller lacks `[ApiController]`; `Program.cs` does not call `AddProblemDetails()`.

---

**Answer:**

_Answer not found._

---

#### Q5. (R) Review this list endpoint. Pagination binds correctly but nested `Sort` is always null despite query string.

```csharp
[HttpGet]
public IActionResult List([FromQuery] PagedQuery query)
{
    return Ok(_repo.List(query));
}

public class PagedQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public SortOptions? Sort { get; set; }
}

public class SortOptions
{
    public string Field { get; set; } = "createdAt";
    public bool Descending { get; set; }
}
```

Request: `GET /api/orders?page=2&sort[field]=createdAt&sort[descending]=true`

---

**Answer:**

_Answer not found._

---

#### Q6. (P) A booking API requires `EndDate >= StartDate` and `GuestCount <= RoomCapacity` on a create-reservation DTO. Compare `IValidatableObject` on the request model vs FluentValidation with `AddFluentValidationAutoValidation()` — execution order, testability, and how errors appear in ProblemDetails for `[ApiController]` endpoints.

---

**Answer:**

_Answer not found._

---

#### Q7. (R) Review this minimal-style note endpoint. The route `orderId` binds as 0 and the note text is null under load.

```csharp
[HttpPost("orders/{orderId:int}/notes")]
public IActionResult AddNote(int orderId, [FromBody] string noteText)
{
    if (orderId == 0) return BadRequest();
    _notes.Add(orderId, noteText);
    return NoContent();
}
```

Body: `"Please ship before Friday"` (raw JSON string). Content-Type: `application/json`.

---

**Answer:**

_Answer not found._

---

#### Q8. (M) An `[ApiController]`-annotated Web API receives a POST with valid JSON syntax but missing required fields and wrong types on optional nested objects. Walk through when automatic model validation runs, the default status code and payload shape in ASP.NET Core 8, and how `[ValidateNever]` vs `[Required]` change behavior for EF navigation properties accidentally included in a DTO.

---

**Answer:**

_Answer not found._

---

#### Q9. (R) Review this inventory adjustment endpoint. Clients send conflicting quantity values; the wrong amount is persisted intermittently.

```csharp
[HttpPut("inventory/{sku}")]
public async Task<IActionResult> Adjust(
    [FromRoute] string sku,
    [FromBody] InventoryAdjustRequest body,
    [FromQuery] int quantity)
{
    var amount = quantity > 0 ? quantity : body.Quantity;
    await _inventory.AdjustAsync(sku, amount);
    return NoContent();
}

public class InventoryAdjustRequest
{
    public int Quantity { get; set; }
}
```

Request: `PUT /api/inventory/WIDGET-1?quantity=5` with body `{ "quantity": 100 }`.

**Answer:**

_Answer not found._

---
