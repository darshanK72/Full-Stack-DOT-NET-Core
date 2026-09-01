# Model Binding & Validation — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is model binding in ASP.NET Core?](#q1-what-is-model-binding-in-aspnet-core)
2. [Q2. How does ASP.NET Core bind route values, query strings, and request bodies to action parameters?](#q2-how-does-aspnet-core-bind-route-values-query-strings-and-request-bodies-to-action-parameters)
3. [Q3. What is the difference between `[FromBody]`, `[FromQuery]`, and `[FromRoute]`?](#q3-what-is-the-difference-between-frombody-fromquery-and-fromroute)
4. [Q4. Can a GET request have a body, and should you use `[FromBody]` on GET?](#q4-can-a-get-request-have-a-body-and-should-you-use-frombody-on-get)
5. [Q5. What does the `[ApiController]` attribute change about model validation?](#q5-what-does-the-apicontroller-attribute-change-about-model-validation)
6. [Q6. What is `ModelState`, and how is it used?](#q6-what-is-modelstate-and-how-is-it-used)
7. [Q7. What are data annotation attributes for validation?](#q7-what-are-data-annotation-attributes-for-validation)
8. [Q8. What HTTP status code does `[ApiController]` return for validation failures by default?](#q8-what-http-status-code-does-apicontroller-return-for-validation-failures-by-default)
9. [Q9. What is RFC 7807 ProblemDetails?](#q9-what-is-rfc-7807-problemdetails)
10. [Q10. How do you enable ProblemDetails responses in ASP.NET Core?](#q10-how-do-you-enable-problemdetails-responses-in-aspnet-core)
11. [Q11. What is the default JSON property naming policy in ASP.NET Core Web APIs?](#q11-what-is-the-default-json-property-naming-policy-in-aspnet-core-web-apis)
12. [Q12. How do you configure camelCase JSON serialization?](#q12-how-do-you-configure-camelcase-json-serialization)
13. [Q13. What is `IValidatableObject`?](#q13-what-is-ivalidatableobject)
14. [Q14. What is FluentValidation, and how does it differ from data annotations?](#q14-what-is-fluentvalidation-and-how-does-it-differ-from-data-annotations)
15. [Q15. How does complex type binding from query strings work (e.g., nested objects)?](#q15-how-does-complex-type-binding-from-query-strings-work-eg-nested-objects)
16. [Q16. What is the difference between model binding errors and validation errors?](#q16-what-is-the-difference-between-model-binding-errors-and-validation-errors)
17. [Q17. What is `[ValidateNever]` used for?](#q17-what-is-validatenever-used-for)
18. [Q18. How does ASP.NET Core handle invalid JSON in the request body?](#q18-how-does-aspnet-core-handle-invalid-json-in-the-request-body)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What is model binding in ASP.NET Core?

What is model binding in ASP.NET Core?

**Answer:** Model binding is the process that maps HTTP request data — route values, query strings, form fields, and JSON bodies — to action method parameters and complex types. ASP.NET Core 8 uses built-in model binders selected by `[FromRoute]`, `[FromQuery]`, `[FromBody]`, and binding source inference rules.

- Binding runs after routing selects an endpoint and before validation and the action executes.
- Simple types bind from route and query by default; complex types from body typically require POST/PUT/PATCH.
- Binding failures add entries to `ModelState` with error messages per property.
- `[ApiController]` applies opinionated binding conventions (e.g., infer `[FromBody]` for complex types on actions).

---

## Q2. How does ASP.NET Core bind route values, query strings, and request bodies to action parameters?

How does ASP.NET Core bind route values, query strings, and request bodies to action parameters?

**Answer:** The framework selects a binding source per parameter: route templates supply `[FromRoute]` values, query keys supply `[FromQuery]`, and the JSON/form body supplies `[FromBody]`. For each source, the appropriate `IModelBinder` converts strings or JSON tokens to CLR types and populates `ModelState` on failure.

- Route values come from matched endpoint parameters — `{id:int}` binds to an `int id` parameter.
- Query binding uses key names matching property names (case-insensitive by default) — `?page=2&sort=name`.
- Body binding uses `System.Text.Json` (or Newtonsoft if configured) for `[FromBody]` complex types.
- `[BindRequired]` and binding source attributes override defaults when multiple sources exist.

---

## Q3. What is the difference between `[FromBody]`, `[FromQuery]`, and `[FromRoute]`?

What is the difference between `[FromBody]`, `[FromQuery]`, and `[FromRoute]`?

**Answer:** These attributes tell model binding which part of the HTTP request supplies a parameter's value. `[FromRoute]` reads URI template values, `[FromQuery]` reads the query string, and `[FromBody]` reads the request body (typically JSON).

- `[FromRoute]` maps `{orderId}` in the template to a method parameter.
- `[FromQuery]` maps `?status=active` to a parameter or complex filter object properties.
- `[FromBody]` deserializes JSON — only one `[FromBody]` parameter is allowed per action by default.
- Using the wrong source silently produces default values (empty object, 0, null) rather than obvious errors.

---

## Q4. Can a GET request have a body, and should you use `[FromBody]` on GET?

Can a GET request have a body, and should you use `[FromBody]` on GET?

**Answer:** HTTP does not forbid a body on GET, but clients, proxies, and caches commonly ignore or strip GET bodies, and ASP.NET Core model binders do not bind `[FromBody]` on GET by default. Using `[FromBody]` on GET is an anti-pattern that fails silently in production.

- Search and filter parameters on GET should use `[FromQuery]` or a complex type bound from the query string.
- Heavy or sensitive filters belong on `POST /search` with `[FromBody]` if the payload is large.
- `[ApiController]` inference will not treat GET body parameters as reliable input.
- Integration tests against localhost may mask the issue that mobile clients and CDNs drop GET bodies.

---

## Q5. What does the `[ApiController]` attribute change about model validation?

What does the `[ApiController]` attribute change about model validation?

**Answer:** `[ApiController]` enables automatic HTTP 400 responses with `ValidationProblemDetails` when model binding or validation fails, without manual `ModelState.IsValid` checks. It also applies binding source inference — complex types from body, simple types from query/route.

- Invalid `ModelState` short-circuits before the action body runs, returning RFC 7807-compatible JSON.
- Attribute routing is required — `[ApiController]` assumes attribute-routed API controllers.
- Binding source inference reduces boilerplate `[FromQuery]` on simple GET parameters.
- Customize behavior via `ConfigureApiBehaviorOptions` (e.g., suppress automatic 400 — discouraged for public APIs).

---

## Q6. What is `ModelState`, and how is it used?

What is `ModelState`, and how is it used?

**Answer:** `ModelState` is a dictionary-like structure on `ControllerBase` that records binding and validation errors keyed by property name. Actions and filters inspect `ModelState.IsValid` to decide whether to proceed, though `[ApiController]` handles this automatically.

- Each key (e.g., `Email`) holds `ModelError` entries with error messages.
- DataAnnotations, `IValidatableObject`, and FluentValidation all contribute errors to `ModelState`.
- `ValidationProblem(ModelState)` returns a 400 ProblemDetails payload with an `errors` extension.
- Binding failures (type conversion) and validation failures (business rules) both appear in the same structure.

---

## Q7. What are data annotation attributes for validation?

What are data annotation attributes for validation?

**Answer:** Data annotations are declarative attributes on model properties — `[Required]`, `[StringLength]`, `[Range]`, `[EmailAddress]`, `[RegularExpression]` — evaluated by the validation system during model validation. They run automatically when `[ApiController]` is present and the model is bound.

- `[Required]` ensures a value was supplied — critical for non-nullable reference types with NRT enabled.
- `[Range(1, 100)]` validates numeric bounds; `[StringLength(200)]` limits string length.
- Validation runs after binding — annotations validate the bound object, not raw JSON syntax.
- Cross-field rules need `[IValidatableObject]` or FluentValidation because annotations are property-scoped.

---

## Q8. What HTTP status code does `[ApiController]` return for validation failures by default?

What HTTP status code does `[ApiController]` return for validation failures by default?

**Answer:** `[ApiController]` returns **400 Bad Request** with a `ValidationProblemDetails` body (`application/problem+json`) when model binding or validation fails. The response includes an `errors` dictionary mapping property names to message arrays.

- No action code runs when automatic validation fails — the filter returns before the method body.
- Malformed JSON (syntax errors) also typically yields 400, though with a different problem type than field validation.
- Clients should parse the `errors` object for per-field form feedback.
- Override only with strong justification — inconsistent status codes break generated SDKs and SPAs.

---

## Q9. What is RFC 7807 ProblemDetails?

What is RFC 7807 ProblemDetails?

**Answer:** RFC 7807 defines a standard JSON format for HTTP API error responses with fields like `type`, `title`, `status`, `detail`, and `instance`. ASP.NET Core 8 uses `ProblemDetails` and `ValidationProblemDetails` as the default error shape for API controllers and exception handlers.

- `ValidationProblemDetails` extends `ProblemDetails` with an `errors` dictionary for field-level failures.
- Content type is `application/problem+json` (or `application/problem+xml`).
- `type` is often a URI identifying the error category; `title` is a short human-readable summary.
- Consistent ProblemDetails let frontends and API gateways handle errors uniformly across endpoints.

---

## Q10. How do you enable ProblemDetails responses in ASP.NET Core?

How do you enable ProblemDetails responses in ASP.NET Core?

**Answer:** Register ProblemDetails services and rely on `[ApiController]` automatic validation, or call `Results.Problem()` / `TypedResults.Problem()` in minimal APIs. In ASP.NET Core 8, `builder.Services.AddProblemDetails()` configures customization, and `IExceptionHandler` implementations return ProblemDetails for unhandled exceptions.

- `[ApiController]` + invalid `ModelState` automatically produces `ValidationProblemDetails`.
- `builder.Services.AddProblemDetails(options => { ... })` customizes default titles and types.
- Exception handling: implement `IExceptionHandler` and register with `AddExceptionHandler<T>()`.
- Use `ProducesResponseType(typeof(ProblemDetails), 400)` in OpenAPI metadata for client generation.

---

## Q11. What is the default JSON property naming policy in ASP.NET Core Web APIs?

What is the default JSON property naming policy in ASP.NET Core Web APIs?

**Answer:** ASP.NET Core 8 Web APIs default to **camelCase** JSON property names via `JsonNamingPolicy.CamelCase` in `System.Text.Json`. A C# property `CustomerName` serializes as `"customerName"` and expects the same on deserialization.

- Configured in `AddControllers().AddJsonOptions(...)` or `ConfigureHttpJsonOptions` for minimal APIs.
- PascalCase JSON keys from some clients do not bind unless `PropertyNameCaseInsensitive = true` is set.
- Prefer standardizing clients on camelCase rather than relying on case-insensitive matching in production.
- OpenAPI documents should reflect camelCase to match actual wire format.

---

## Q12. How do you configure camelCase JSON serialization?

How do you configure camelCase JSON serialization?

**Answer:** ASP.NET Core 8 uses camelCase by default — explicit configuration is only needed when changing or confirming the policy. Set `JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase` in `AddJsonOptions` for controllers or `ConfigureHttpJsonOptions` for minimal APIs.

```csharp
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);
```

- `ConfigureHttpJsonOptions` applies the same policy to minimal API JSON responses and binding.
- Use `PropertyNameCaseInsensitive = true` only as a compatibility bridge, not the primary contract.
- `[JsonPropertyName("customKey")]` overrides naming for individual properties.
- Newtonsoft.Json requires separate configuration via `AddNewtonsoftJson` if used instead of System.Text.Json.

---

## Q13. What is `IValidatableObject`?

What is `IValidatableObject`?

**Answer:** `IValidatableObject` is an interface implemented on a model class to perform cross-property validation in a `Validate(ValidationContext)` method. It runs during the same validation pass as DataAnnotations and adds errors to `ModelState`.

- Use when one property's validity depends on another — e.g., `EndDate >= StartDate`.
- Return `ValidationResult` instances with optional member names to attach errors to specific properties.
- Errors surface in `ValidationProblemDetails.errors` identically to `[Required]` failures.
- Keep rules focused — large rule sets belong in FluentValidation validators instead.

---

## Q14. What is FluentValidation, and how does it differ from data annotations?

What is FluentValidation, and how does it differ from data annotations?

**Answer:** FluentValidation is a library that defines validation rules in separate `AbstractValidator<T>` classes with a fluent API, rather than attributes on DTOs. Register it with `AddFluentValidationAutoValidation()` to integrate with ASP.NET Core 8 model validation and `ModelState`.

- Rules live in validator classes — DTOs stay clean and free of validation attributes.
- Supports complex conditional logic, async rules, and reusable rule sets more comfortably than annotations.
- Errors still merge into `ModelState` and return as `ValidationProblemDetails` when `[ApiController]` is enabled.
- Do not duplicate the same rule in both DataAnnotations and FluentValidation.

---

## Q15. How does complex type binding from query strings work (e.g., nested objects)?

How does complex type binding from query strings work (e.g., nested objects)?

**Answer:** Complex types bind from query strings using prefix notation — property names become query keys like `sort.field=name&sort.descending=true` for a nested `Sort` object. ASP.NET Core 8 binds public settable properties case-insensitively by default for `[FromQuery]` types.

- Prefix binding uses dot notation for nesting — `filter.minPrice=10&filter.category=books`.
- Some clients send bracket notation (`sort[field]`) which may require custom model binders or documentation alignment.
- Deep nesting in query strings is fragile — flatten to `sortField` and `sortDescending` for public APIs.
- Test binding with integration tests — silent null nested objects are a common production bug.

---

## Q16. What is the difference between model binding errors and validation errors?

What is the difference between model binding errors and validation errors?

**Answer:** Model binding errors occur when the framework cannot convert request data to the target type — invalid JSON shape, type conversion failure, or missing required binding source. Validation errors occur when binding succeeds but DataAnnotations, `IValidatableObject`, or FluentValidation rules reject the values.

- Binding error example: `"abc"` for an `int` route parameter when no `:int` constraint exists.
- Validation error example: empty string on a property marked `[Required]` after successful bind.
- Both appear in `ModelState` but binding errors may prevent validation from running on unbound properties.
- Route constraints push some binding failures to 404 before `ModelState` is populated.

---

## Q17. What is `[ValidateNever]` used for?

What is `[ValidateNever]` used for?

**Answer:** `[ValidateNever]` excludes a property from validation — useful when a navigation property or large object graph should not be validated during binding. It prevents `[Required]` and other validators from running on that property and its children.

- Apply to EF Core navigation properties accidentally exposed on API DTOs.
- Prevents over-validation of nested graphs on PATCH endpoints that bind partial updates.
- Does not skip model binding — the property still binds if present in the request.
- Use deliberately — excluding properties can hide missing validation on sensitive fields.

---

## Q18. How does ASP.NET Core handle invalid JSON in the request body?

How does ASP.NET Core handle invalid JSON in the request body?

**Answer:** When the JSON payload is syntactically invalid or cannot be deserialized to the target type, ASP.NET Core 8 returns **400 Bad Request** with a ProblemDetails response before the action executes. This is distinct from validation failures on a successfully deserialized object.

- Malformed JSON (trailing comma, wrong token type) fails during input formatting, not DataAnnotations.
- `[ApiController]` maps formatter exceptions to 400 automatically for API controllers.
- Type mismatches (`"text"` for an `int` property) may bind as model errors or deserialization failures depending on configuration.
- Log deserialization failures server-side; return generic messages to clients without exposing internal parser details.

---

---

## Gotchas — ASP.NET Core (Interview Traps)

#### Gotcha 1. Middleware order — routing before auth

**Answer:** In ASP.NET Core 8 endpoint routing, `UseRouting` must run before `UseAuthentication` and `UseAuthorization` so the auth middleware can inspect endpoint metadata — registering auth before routing breaks endpoint-aware authorization and policy resolution.

- The recommended order is exception handling → forwarded headers → routing → authentication → authorization → endpoints (`MapControllers` / `MapGet`).
- When auth runs before routing, the endpoint has not been selected yet and `[Authorize]` metadata on minimal routes or controllers may not apply correctly.
- Symptoms include anonymous access to protected endpoints or 401 responses without proper challenge behavior.
- Always verify middleware order in `Program.cs` during code review for new services.

---

#### Gotcha 2. Scoped service in a Singleton

**Answer:** Registering a scoped service such as `DbContext` into a singleton creates a captive dependency that lives for the application lifetime while the scoped instance is disposed after its first scope ends, causing stale data, thread-safety bugs, or `ObjectDisposedException`.

- The singleton holds one scoped instance forever instead of one per request — EF change trackers accumulate unrelated entities.
- Enable `ValidateScopes` in Development/staging to catch illegal scope combinations at startup.
- Fix by injecting `IServiceScopeFactory` or `IDbContextFactory<T>` and creating a scope per operation.
- This applies equally to singleton services, hosted services, and cached delegates in Minimal APIs.

---

#### Gotcha 3. `new HttpClient()` in a singleton

**Answer:** Instantiating `HttpClient` with `new` inside a long-lived singleton prevents socket reuse and causes socket exhaustion under load because each instance holds its own connection pool until garbage-collected.

- `HttpClient` is disposable but not meant for per-use disposal — `using var client = new HttpClient()` in a singleton is an anti-pattern.
- `IHttpClientFactory` manages `HttpMessageHandler` lifetimes and recycles connections correctly.
- Register named or typed clients: `builder.Services.AddHttpClient<IExternalApi, ExternalApiClient>();`
- Symptoms include `SocketException` and timeout errors only under production traffic, not in local testing.

---

#### Gotcha 4. `IOptions<T>` vs reload

**Answer:** `IOptions<T>` captures configuration snapshot at first resolution — reading `.Value` once in a singleton constructor freezes settings even when `appsettings.json` reloads with `ReloadOnChange` enabled.

- `IOptionsSnapshot<T>` recalculates per request scope; `IOptionsMonitor<T>` supports change notifications via `OnChange`.
- Singleton services must use `IOptionsMonitor<T>` or read options inside scoped operations if they need live updates.
- Misconfiguration persists silently until process restart when `.Value` was cached at construction.
- See Chapter 05 for the full options lifetime comparison.

---

#### Gotcha 5. GET with `[FromBody]`

**Answer:** Using `[FromBody]` on GET action parameters or minimal API handlers is an anti-pattern because HTTP GET semantics discourage bodies, and many clients, proxies, and caches strip or ignore GET request bodies, so binding fails silently in production.

- Query strings and route values are the correct binding sources for GET requests.
- Complex filters should use `[FromQuery]` with `[AsParameters]` or flattened query keys.
- Failures often appear only in specific browsers or CDN layers, not in Swagger "Try it out" during development.
- REST conventions expect GET to be safe and idempotent with parameters in the URL.

---

#### Gotcha 6. PascalCase JSON keys with default camelCase policy

**Answer:** ASP.NET Core 8 Web API serializes JSON with camelCase property names by default via `JsonNamingPolicy.CamelCase`, so incoming JSON with PascalCase keys (for example `"CustomerName"`) may not bind to `CustomerName` unless case-insensitive matching is enabled.

- Mobile or legacy clients sending PascalCase appear to succeed but properties remain default values (empty string, zero).
- Prefer standardizing clients on camelCase and documenting the contract in OpenAPI.
- Optional mitigation: `AddJsonOptions(o => o.JsonSerializerOptions.PropertyNameCaseInsensitive = true)` — but explicit camelCase contracts are cleaner.
- Add validation attributes so silent binding failures become 400 responses instead of corrupt data.

---

#### Gotcha 7. `throw ex` vs `throw`

**Answer:** Rethrowing with `throw ex` resets the stack trace to the catch block line, hiding the original failure location in logs and diagnostics, while bare `throw` preserves the full stack trace from where the exception was first thrown.

- Exception filters, middleware, and Application Insights rely on accurate stack traces for root-cause analysis.
- Always use `throw;` when rethrowing after logging or cleanup in a catch block.
- Wrap in a new exception only when adding context: `throw new OrderProcessingException("...", ex)` to preserve `InnerException`.
- This trap appears in both application code and background worker error handlers.

---

#### Gotcha 8. Kestrel as the only production layer

**Answer:** Running Kestrel exposed directly to the internet without a reverse proxy skips TLS termination at the edge, centralized rate limiting, WAF protection, and efficient static-file caching that production deployments typically require.

- Kestrel is production-grade as an application server but is not a full edge gateway — nginx, IIS, Azure Front Door, or AWS ALB commonly sit in front.
- TLS certificates are easier to manage at the proxy layer with automatic renewal.
- Direct exposure also complicates client IP logging unless `UseForwardedHeaders` is configured with a trusted proxy.
- Containers often bind Kestrel to port 8080 internally while the ingress controller handles HTTPS externally.

---

#### Gotcha 9. `launchSettings.json` in production

**Answer:** Settings in `Properties/launchSettings.json` — including `applicationUrl`, environment variables, and launch profiles — apply only when starting from Visual Studio, VS Code, or `dotnet run` with a profile; they are not deployed to production hosts.

- Production URLs and environment come from environment variables (`ASPNETCORE_URLS`, `ASPNETCORE_ENVIRONMENT`), container configuration, or IIS/nginx site settings.
- Assuming `launchSettings.json` sets Production behavior leads to wrong environment or binding in deployed environments.
- The file is development ergonomics, not runtime configuration.
- Use `appsettings.Production.json` and host-level env vars for production values.

---

#### Gotcha 10. Non-nullable `bool` for PATCH semantics

**Answer:** A non-nullable `bool` property cannot distinguish "field omitted from JSON" from "explicitly set to false" because System.Text.Json deserializes missing properties to `default(false)`, corrupting partial-update semantics.

- PATCH endpoints need `bool?`, separate update DTOs, or enums such as `Unspecified | OptIn | OptOut` for tri-state intent.
- Marketing consent and feature flags are common domains where this bug causes compliance or logic errors.
- Create DTOs may use non-nullable bool when explicit values are always required on insert.
- Document nullable fields in OpenAPI so generated clients represent optional updates correctly.

---

#### Gotcha 11. Forgetting `UseForwardedHeaders` behind a proxy

**Answer:** Without forwarded headers middleware configured with known proxy IPs, `HttpContext.Request.Scheme` remains `http`, `Request.Host` reflects the internal address, and client IP is the proxy — breaking HTTPS redirects, cookie secure flags, and audit logs.

- Call `UseForwardedHeaders()` early, before middleware that reads scheme or host (HTTPS redirection, link generation, rate limiting by IP).
- Configure `ForwardedHeadersOptions` to trust only your reverse proxy network — trusting all proxies enables header spoofing.
- Headers include `X-Forwarded-For`, `X-Forwarded-Proto`, and `X-Forwarded-Host`.
- Local development without a proxy does not need this; production behind nginx/IIS/ALB does.

---

#### Gotcha 12. Static files in `wwwroot` are public

**Answer:** Any file under `wwwroot` is served by `UseStaticFiles()` to unauthenticated clients by default — placing secrets, `.env`, backup configs, or private keys there exposes them over HTTP.

- Only public assets (CSS, JS, images, public PDFs) belong in `wwwroot`.
- Sensitive configuration stays outside the web root and is loaded through `IConfiguration`, environment variables, or secret managers.
- Accidental copy of `appsettings.Production.json` into `wwwroot` is a critical security incident.
- Use build pipelines to verify web root contents before deploy.

---

#### Gotcha 13. `MapFallbackToFile` intercepting API routes

**Answer:** SPA fallback middleware registered before API endpoint mapping returns `index.html` for `/api/*` 404 responses, making API failures look like successful HTML responses to clients and breaking JSON parsers.

- Map API routes (`MapControllers`, minimal API groups) before `MapFallbackToFile("index.html")`.
- Scope fallback to non-API paths or use conditional fallback that excludes `/api` prefixes.
- Symptoms include CORS errors masked as HTML responses and Swagger fetch failures in production SPA hosting.
- Order in `Program.cs` is: API endpoints first, static files, fallback last.

---

#### Gotcha 14. Background service without scope factory

**Answer:** A singleton `BackgroundService` that injects scoped services (`DbContext`, repositories) directly into its constructor fails at startup with scope validation errors or uses disposed instances after the first background iteration.

- Hosted services live for the application lifetime — scoped dependencies must not be constructor-injected.
- Inject `IServiceScopeFactory`, create `await using var scope = factory.CreateAsyncScope()` per job, resolve scoped services inside the scope, and dispose when the job completes.
- Same rule applies to timers and `Task.Run` loops started from singletons.
- Enable `ValidateScopes` to catch this defect before production deployment.

---

#### Gotcha 15. SignalR without a backplane on multiple instances

**Answer:** SignalR broadcasts from one server instance reach only clients connected to that instance — without a Redis or Azure Service Bus backplane (or Azure SignalR Service), users on different nodes never receive each other's real-time events.

- Sticky sessions keep one client on one node but do not route events raised on other nodes to that client.
- Register `AddSignalR().AddStackExchangeRedis(...)` with a consistent channel prefix per application.
- Raw WebSocket apps need equivalent custom pub/sub — SignalR's backplane is the built-in solution.
- Test scale-out with at least two instances before launch, not single-node staging alone.

---

---

## Gotchas — ASP.NET Core (Interview Traps)

## Gotchas — ASP.NET Core (Interview Traps)

---

## Scenario-Based Questions (Karat Format)

#### Q1. (R) Review this API model and client JSON. Mobile clients send PascalCase; web clients send camelCase — some fields bind as default values instead of the intended data.

```csharp
public class CreateCustomerRequest
{
    public string CustomerName { get; set; } = "";
    public decimal CreditLimit { get; set; }
}

// Client A body: { "CustomerName": "Acme", "CreditLimit": 5000 }
// Client B body: { "customerName": "Acme", "creditLimit": 5000 }

[HttpPost]
public IActionResult Create([FromBody] CreateCustomerRequest request)
{
    if (string.IsNullOrEmpty(request.CustomerName))
        return BadRequest("CustomerName required");
    return Ok(_service.Create(request));
}
```

`Program.cs` has no `AddJsonOptions`; project targets ASP.NET Core 8 Web API template defaults.

---

**Answer:**

_Answer not found._

---

#### Q2. (D) A PATCH endpoint updates marketing preferences. Product requires three states: "not specified" (omit field), explicit opt-in (`true`), and explicit opt-out (`false`). Review the proposed model and JSON contract.

```csharp
public class UpdatePreferencesRequest
{
    public bool SendNewsletter { get; set; } // default false when omitted
}

// PATCH body omitting SendNewsletter — should mean "leave unchanged"
// PATCH body { "sendNewsletter": false } — must mean explicit opt-out
```

---

**Answer:**

_Answer not found._

---

#### Q3. (R) Review this search endpoint. The filter object always arrives empty even though query string is present in browser dev tools.

```csharp
[HttpGet("search")]
public IActionResult Search([FromBody] ProductFilter filter)
{
    var results = _catalog.Search(filter);
    return Ok(results);
}

public class ProductFilter
{
    public string? Category { get; set; }
    public decimal? MinPrice { get; set; }
}
```

Request: `GET /api/products/search?category=electronics&minPrice=10`

---

**Answer:**

_Answer not found._

---

#### Q4. (R) Review validation and error responses. Frontend expects RFC 7807 `ProblemDetails` with field errors; API returns plain text 400.

```csharp
[HttpPost]
public IActionResult Register([FromBody] RegisterRequest request)
{
    if (!ModelState.IsValid)
        return BadRequest("Validation failed");

    return Ok(_users.Register(request));
}

public class RegisterRequest
{
    [Required][EmailAddress] public string Email { get; set; } = "";
    [Required][MinLength(8)] public string Password { get; set; } = "";
}
```

Controller lacks `[ApiController]` attribute; `Program.cs` does not call `AddProblemDetails()`.

---

**Answer:**

_Answer not found._

---

#### Q5. (R) Review complex query binding. Pagination works but nested sort object is always null.

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
    public string Field { get; set; } = "name";
    public bool Descending { get; set; }
}
```

Request: `GET /api/items?page=2&sort.field=name&sort.descending=true`

---

**Answer:**

```csharp
public class PagedQuery
{
    public int Page { get; set; } = 1;
    public SortOptions? Sort { get; set; }
}
```

Request: `GET /api/items?page=2&sort.field=name&sort.descending=true`

**Answer:** Nested complex types require bracket notation by default in ASP.NET Core query binding — `sort.field` works for prefix binding; verify `[FromQuery]` prefix or use flat query parameters if clients send wrong format like `sort[field]`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Query binding | Client uses wrong nested syntax (`sort[field]` vs `sort.field`) | `Sort` stays null — default sort used silently |
| Documentation | OpenAPI does not describe nested query shape | SDKs generate wrong query strings |
| Design | Deep nesting in query strings | Hard to debug — prefer flat `sortField` / `sortDesc` |

**Fix (priority order):**

1. Document and test supported format: `?sort.field=name&sort.descending=true` or `[FromQuery(Name = "sort")]`.
2. Simplify DTO: `public string? SortField { get; set; }` and `public bool SortDescending { get; set; }` for public APIs.
3. Add model binding test with `WebApplicationFactory` asserting bound values.

**Production takeaway:** Complex `[FromQuery]` objects are fragile — Karat tests whether you know prefix conventions or flatten the contract.

---

---

#### Q6. (P) A cross-field rule requires `EndDate >= StartDate` on a booking DTO. Compare implementing `IValidatableObject` on the model vs FluentValidation in the pipeline — what runs when, and how do errors surface in ProblemDetails?

---

**Answer:**

_Answer not found._

---

#### Q7. (R) Review this POST action. Model binding silently fails and `id` is always 0.

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

```csharp
[HttpPost("orders/{orderId:int}/notes")]
public IActionResult AddNote(int orderId, [FromBody] string noteText)
```

Body: `"Please ship before Friday"` (raw JSON string).

**Answer:** `[FromBody] string` expects a JSON string token but route `orderId` must be `[FromRoute]` explicitly when mixed with body; primitive body binding is fragile — use a wrapper DTO `{ "noteText": "..." }`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Binding source | `orderId` not marked `[FromRoute]` | May bind incorrectly when multiple sources exist |
| Body primitive | Raw JSON string body | Easy to fail content negotiation; prefer wrapper type |
| Validation | `orderId == 0` conflates missing route with invalid id | Weak guard — route constraint should reject bad ids |

**Fix (priority order):**

1. Use `public record AddNoteRequest(string NoteText);` with `[FromBody] AddNoteRequest body`.
2. Mark route parameter: `[FromRoute] int orderId`.
3. Keep `{orderId:int}` route constraint — invalid routes 404 before action.

**Production takeaway:** `[FromBody]` on primitives is a common interview trap — wrapper DTOs are the production pattern.

---

---

#### Q8. (M) An `[ApiController]`-annotated controller receives invalid JSON shape. Walk through when automatic model validation runs, what status code and payload the framework returns by default in ASP.NET Core 8, and how `[ValidateNever]` or `[Required]` affect that behavior.

---

**Answer:**

_Answer not found._

---

#### Q9. (R) Review this action mixing binding sources. Route id and body quantity disagree; wrong inventory is updated under load.

```csharp
[HttpPut("inventory/{sku}")]
public async Task<IActionResult> Adjust(
    [FromRoute] string sku,
    [FromBody] InventoryAdjustRequest body,
    [FromQuery] int quantity)
{
    await _inventory.AdjustAsync(sku, quantity > 0 ? quantity : body.Quantity);
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
