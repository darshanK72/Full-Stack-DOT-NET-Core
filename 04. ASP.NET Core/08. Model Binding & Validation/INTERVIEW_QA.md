# Model Binding & Validation — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is model binding in ASP.NET Core?](#q1-what-is-model-binding-in-aspnet-core)
2. [Q2. What are the binding sources in ASP.NET Core?](#q2-what-are-the-binding-sources-in-aspnet-core)
3. [Q3. What is `[ApiController]` and what automatic behaviors does it add?](#q3-what-is-apicontroller-and-what-automatic-behaviors-does-it-add)
4. [Q4. What is `ModelState` and `ModelState.IsValid`?](#q4-what-is-modelstate-and-modelstateisvalid)
5. [Q5. How do DataAnnotations work for model validation?](#q5-how-do-dataannotations-work-for-model-validation)
6. [Q6. What is `IValidatableObject`, and when should you use it?](#q6-what-is-ivalidatableobject-and-when-should-you-use-it)
7. [Q7. What is FluentValidation, and how does it compare to DataAnnotations?](#q7-what-is-fluentvalidation-and-how-does-it-compare-to-dataannotations)
8. [Q8. What is RFC 7807 ProblemDetails?](#q8-what-is-rfc-7807-problemdetails)
9. [Q9. How does `[FromBody]` work, and when does binding fail silently?](#q9-how-does-frombody-work-and-when-does-binding-fail-silently)
10. [Q10. What is the difference between `[FromQuery]` and `[FromRoute]`?](#q10-what-is-the-difference-between-fromquery-and-fromroute)
11. [Q11. When should you use `[FromForm]` vs `[FromBody]`?](#q11-when-should-you-use-fromform-vs-frombody)
12. [Q12. What is `[AsParameters]` in minimal APIs?](#q12-what-is-asparameters-in-minimal-apis)
13. [Q13. How do you configure JSON serialization settings (e.g., camelCase, nulls)?](#q13-how-do-you-configure-json-serialization-settings-eg-camelcase-nulls)
14. [Q14. How does `[ValidateNever]` work?](#q14-how-does-validatenever-work)
15. [Q15. What is custom model binding, and when do you need it?](#q15-what-is-custom-model-binding-and-when-do-you-need-it)
16. [Q16. What happens when `[FromBody]` reads a request with the wrong Content-Type?](#q16-what-happens-when-frombody-reads-a-request-with-the-wrong-content-type)
17. [Q17. How does `[ApiController]` automatic 400 response interact with custom error handling?](#q17-how-does-apicontroller-automatic-400-response-interact-with-custom-error-handling)
18. [Q18. How do you validate nested complex objects?](#q18-how-do-you-validate-nested-complex-objects)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What is model binding in ASP.NET Core?

**Concepts**
- Model binding extracting values from the HTTP request and populating action parameters
- Binding sources: route values, query string, form, body, headers, services
- Source-parameter matching by name and type
- Binding failure recorded in `ModelState` without throwing

**Answer**

Model binding is the process by which ASP.NET Core extracts values from an incoming HTTP request — route segments, query string, form fields, JSON body, and header values — and maps them to action method parameters or minimal API delegate arguments. The framework uses a provider chain to find values by name and convert them to the parameter type. For primitive types, the binder searches route data first, then query string, then form. Complex types default to `[FromBody]` for controllers and route + query for minimal APIs unless overridden. Binding failures are recorded in `ModelState` without throwing exceptions, so `ModelState.IsValid` detects type conversion or missing required value problems. `[ApiController]` short-circuits with a 400 automatically before the action body runs when `ModelState.IsValid` is false.

---

## Q2. What are the binding sources in ASP.NET Core?

**Concepts**
- `[FromRoute]` — route template values
- `[FromQuery]` — URL query string
- `[FromBody]` — deserializing request body (default for complex types in Web APIs)
- `[FromForm]` — multipart or `application/x-www-form-urlencoded`
- `[FromHeader]` — HTTP request headers
- `[FromServices]` — DI container (implicit for known service types)

**Answer**

ASP.NET Core defines six binding sources, each with an attribute to declare explicitly. `[FromRoute]` binds from path template segments, `[FromQuery]` from the query string, `[FromBody]` by deserializing the request body (typically JSON), `[FromForm]` from HTML form submissions, `[FromHeader]` from request headers, and `[FromServices]` from the DI container. With `[ApiController]`, complex types on POST/PUT/PATCH actions default to `[FromBody]` without explicit annotation. A single action can only have one `[FromBody]` parameter since the body is read once. `[FromServices]` requires minimal API parameters to be injected without appearing in the route or query, and ASP.NET Core 8 injects known services automatically in minimal APIs without explicit annotation.

---

## Q3. What is `[ApiController]` and what automatic behaviors does it add?

**Concepts**
- Automatic `ModelState.IsValid` check returning 400 `ValidationProblemDetails` before the action runs
- Implicit `[FromBody]` on complex type parameters for POST/PUT/PATCH
- Binding source inference eliminating explicit `[FromRoute]`, `[FromQuery]` repetition
- Requirement for attribute routing — conventional routing is not allowed

**Answer**

`[ApiController]` is an attribute that activates several conventions for Web API controllers. First, it runs automatic model validation before the action — if `ModelState.IsValid` is false, the framework returns a 400 `ValidationProblemDetails` response without entering the action method. Second, it infers binding sources for parameters — simple types (primitives, enums) bind from route data or query string, complex types bind from the body, and `IFormFile` binds from the form, without requiring explicit `[From*]` attributes. Third, it requires attribute routing and prohibits conventional `{controller}/{action}` routes. Fourth, it enables `ProblemDetails` formatting for error responses including 404 and 415. All four behaviors are controlled by `ApiBehaviorOptions` if you need to disable any of them selectively.

---

## Q4. What is `ModelState` and `ModelState.IsValid`?

**Concepts**
- `ModelState` accumulating binding errors, conversion failures, and validation violations
- `ModelState.IsValid` returning `true` only when no errors exist
- `[ApiController]` automating the `if (!ModelState.IsValid) return BadRequest()` check
- `AddModelError` for business-logic errors injected manually

**Answer**

`ModelState` is a dictionary-like object on `ControllerBase` that accumulates all binding errors, type conversion failures, and validation rule violations for a request. Each entry maps a property path to a `ModelStateEntry` containing the attempted value and error messages. `ModelState.IsValid` returns `true` only when no entries have errors. Without `[ApiController]`, you check this manually and return `BadRequest(ModelState)`. With `[ApiController]`, the framework checks it automatically and emits `ValidationProblemDetails` before calling the action. You can also add validation errors manually for business logic — `ModelState.AddModelError("Email", "Already in use")` — and return `BadRequest(ModelState)` from the action.

---

## Q5. How do DataAnnotations work for model validation?

**Concepts**
- DataAnnotation attributes declaring declarative validation rules on model properties
- Built-in attributes: `[Required]`, `[Range]`, `[StringLength]`, `[EmailAddress]`, `[RegularExpression]`
- `[Required]` on a non-nullable value type adding no extra benefit in C# 8+ with nullable reference types
- Error messages customizable via `ErrorMessage` parameter

**Answer**

DataAnnotations are attributes on model class properties that declare validation rules, evaluated automatically during model binding. `[Required]` marks a field mandatory, `[Range(1, 100)]` constrains numeric bounds, `[StringLength(100, MinimumLength = 3)]` constrains string length, `[EmailAddress]` validates email format, and `[RegularExpression]` validates a custom pattern. The model validator runs after binding and adds violations to `ModelState`. With `[ApiController]`, failures result in automatic 400 responses. Error messages are customizable via `ErrorMessage = "..."` on each attribute or via resource files for localization. In C# with nullable reference types enabled, non-nullable properties are implicitly required, so adding `[Required]` on `string` is redundant; however, `[Required]` is still needed on nullable value types that must not be null in the JSON payload.

---

## Q6. What is `IValidatableObject`, and when should you use it?

**Concepts**
- `IValidatableObject.Validate()` implementing cross-property validation on the model itself
- Runs after DataAnnotations pass
- No async or DI inside `Validate()` — only pure property logic
- Use when two or more properties have an invariant that a single attribute cannot express

**Answer**

`IValidatableObject` defines a `Validate(ValidationContext context)` method that runs on the model after all DataAnnotation attributes pass. It enables cross-property validation — for example, `EndDate >= StartDate` where neither attribute alone can express the relationship between two properties. The method yields `ValidationResult` objects with property names to associate errors with specific fields in `ModelState`. `Validate()` runs in the ASP.NET Core model validation pipeline, so errors appear in `ModelState` and trigger the automatic 400 response with `[ApiController]`. One limitation is that `ValidationContext` provides `GetService()` but DI is rarely used there; keep `Validate()` to pure property logic and push async service calls to the action method or a FluentValidation async validator.

---

## Q7. What is FluentValidation, and how does it compare to DataAnnotations?

**Concepts**
- FluentValidation providing a fluent API for complex validation rules
- Validators as separate classes rather than attributes decorating DTOs
- Async validation and DI access inside validator classes
- Registration with `AddFluentValidation` replacing the default model validator

**Answer**

FluentValidation is a library where validation rules are defined in separate `AbstractValidator<T>` classes using a fluent API rather than on the DTO with attributes. This separates validation logic from the data model, makes validators independently testable, supports async rules and DI injection for repository lookups, and handles complex conditional rules cleanly — `RuleFor(x => x.Discount).LessThan(x => x.Price).When(x => x.HasDiscount)`. Register validators with `builder.Services.AddFluentValidation(c => c.RegisterValidatorsFromAssemblyContaining<CreateOrderValidator>())` and they plug into the MVC model validation pipeline transparently so `[ApiController]` 400 responses still work. DataAnnotations are simpler for straightforward single-property rules and require no extra dependency, while FluentValidation is the better choice when rules depend on other properties, external services, or when the same DTO is validated differently in different contexts.

---

## Q8. What is RFC 7807 ProblemDetails?

**Concepts**
- RFC 7807 defining a standardized JSON error response format
- Fields: `type`, `title`, `status`, `detail`, `instance`
- ASP.NET Core 8 enabling `ProblemDetails` automatically with `[ApiController]`
- `ValidationProblemDetails` extending with an `errors` dictionary for field-level errors

**Answer**

RFC 7807 Problem Details for HTTP APIs defines a standardized JSON response format for errors so clients can parse error responses consistently without per-API conventions. The standard fields are `type` (URI identifying the error type), `title` (short human-readable summary), `status` (HTTP status code), `detail` (human-readable explanation specific to this occurrence), and `instance` (URI identifying the specific request). ASP.NET Core 8 with `[ApiController]` returns `ValidationProblemDetails` for 400 responses, which extends the base format with an `errors` dictionary mapping field names to arrays of error messages. Configure `AddProblemDetails()` and `UseExceptionHandler` to emit ProblemDetails for unhandled exceptions, 404s, and 405s, so all error responses from the API share a consistent contract.

---

## Q9. How does `[FromBody]` work, and when does binding fail silently?

**Concepts**
- `[FromBody]` triggering JSON deserialization of the request body
- Body read once — second `[FromBody]` on the same request errors
- Missing `Content-Type: application/json` header causing 415 Unsupported Media Type
- `[JsonRequired]` preventing silent null binding on required fields

**Answer**

`[FromBody]` reads and deserializes the request body using the registered input formatter, typically System.Text.Json for `Content-Type: application/json`. The body stream is read once, so only one `[FromBody]` parameter is allowed per action. Binding can fail silently when the body JSON contains fields with the wrong type — System.Text.Json ignores unknown properties and uses `default` for missing ones, so a required field absent from JSON binds as `null` or `0` without a validation error unless `[Required]` or `[JsonRequired]` is applied. A missing or mismatched `Content-Type` header returns 415 before binding runs. Enabling `JsonSerializerOptions.UnmappedMemberHandling = Disallow` or using `[JsonRequired]` on properties catches missing required JSON fields at deserialization time rather than relying on DataAnnotations alone.

---

## Q10. What is the difference between `[FromQuery]` and `[FromRoute]`?

**Concepts**
- `[FromRoute]` binding from a named segment in the route template
- `[FromQuery]` binding from the URL query string after `?`
- Route segment must be in the template — `[FromQuery]` can be any key
- Complex type with `[FromQuery]` binding each public property to a query key by name

**Answer**

`[FromRoute]` binds a parameter from a named segment in the route template — `{id:int}` in the template maps to a parameter named `id` decorated with `[FromRoute]`. The segment must be defined in the template; without it, `[FromRoute]` finds nothing and binds `default`. `[FromQuery]` binds from key-value pairs in the URL query string after the `?` regardless of the route template. A complex type annotated with `[FromQuery]` binds each public property to the matching query key by name — `?page=2&pageSize=25` binds to `PaginationParams { Page, PageSize }`. Route values are part of the URL path (indexed and matched by routing), while query string is unordered key-value data outside the matched path. Use `[FromRoute]` for resource identifiers and `[FromQuery]` for filters, pagination, and optional parameters.

---

## Q11. When should you use `[FromForm]` vs `[FromBody]`?

**Concepts**
- `[FromForm]` for HTML form submissions and file uploads (`IFormFile` or `IFormFileCollection`)
- `[FromBody]` for JSON and XML payloads from API clients
- Cannot combine `[FromForm]` and `[FromBody]` in the same action
- `multipart/form-data` required for `IFormFile` — `application/json` for `[FromBody]`

**Answer**

Use `[FromForm]` when the request is an HTML form submission with `application/x-www-form-urlencoded` or a multipart form upload with `multipart/form-data`. `IFormFile` and `IFormFileCollection` parameters always require `[FromForm]` and multipart encoding since they read file streams from form parts. Use `[FromBody]` for JSON or XML payloads sent by API clients with the correct `Content-Type` header. The two binding sources are incompatible in a single action because the body is read by one input formatter and the form parser uses a different reader — combining them causes binding failures or null values on one of the two parameters. For file uploads with accompanying metadata, accept metadata as query string or route parameters and the file as `IFormFile`.

---

## Q12. What is `[AsParameters]` in minimal APIs?

**Concepts**
- `[AsParameters]` binding each property of a struct or class from its individual sources
- Eliminating repetitive flat parameter lists in minimal API delegates
- Each property applying its own `[FromQuery]`, `[FromRoute]`, or `[FromHeader]` attribute
- Mirror of `[BindProperties]` in controller actions

**Answer**

`[AsParameters]` is a minimal API feature that tells the framework to bind each property of the annotated struct or class from its individual binding source — route, query string, or header — rather than treating the whole type as a single JSON body. It lets you group related route and query parameters into a typed record: `record SearchParams([FromQuery] string? Term, [FromQuery] int Page, [FromRoute] int CategoryId)`, then declare the handler as `app.MapGet("/{categoryId}", ([AsParameters] SearchParams p) => ...)`. Each property picks up its own `[From*]` attribute. This mirrors `[BindProperties]` in MVC controllers and keeps minimal API delegates concise when there are many query or route inputs.

---

## Q13. How do you configure JSON serialization settings (e.g., camelCase, nulls)?

**Concepts**
- `AddJsonOptions` configuring `JsonSerializerOptions` for MVC
- `JsonNamingPolicy.CamelCase` as the ASP.NET Core 8 default
- `DefaultIgnoreCondition` controlling `null` serialization
- `ReferenceHandler.Preserve` for circular reference handling

**Answer**

Configure JSON serialization in `Program.cs` via `builder.Services.AddControllers().AddJsonOptions(options => { ... })` for MVC, or `builder.Services.ConfigureHttpJsonOptions(options => { ... })` for minimal APIs. ASP.NET Core 8 defaults to camelCase property names. Change naming policy with `options.JsonSerializerOptions.PropertyNamingPolicy`, control null output with `options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull`, enable reference handling for circular EF graphs with `options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve`, and set `options.JsonSerializerOptions.PropertyNameCaseInsensitive = true` for case-insensitive deserialization. Per-property overrides use `[JsonPropertyName("name")]` and `[JsonIgnore]` without changing global policy.

---

## Q14. How does `[ValidateNever]` work?

**Concepts**
- `[ValidateNever]` excluding a property or parameter from model validation
- Use case: server-populated fields not present in incoming payloads
- Does not affect serialization — only skips validation pipeline
- Alternative: separate DTO without the excluded field

**Answer**

`[ValidateNever]` is a DataAnnotations attribute that instructs the model validator to skip validation for the decorated property or parameter entirely, including any `[Required]` or other constraints on that property. It is used when a property is populated server-side (from a claim, route value, or service) rather than from the incoming request body — for example, `UserId` set from `User.GetUserId()` in the action, which should never come from the client. Applying it prevents `ModelState` from reporting the property as missing even when it is absent from the JSON. A cleaner alternative is a separate input DTO that does not include server-populated fields at all, but `[ValidateNever]` is a quick opt-out when the model class is shared between different contexts.

---

## Q15. What is custom model binding, and when do you need it?

**Concepts**
- Custom `IModelBinder` handling non-standard type conversion from request values
- `IModelBinderProvider` registering custom binders by type or convention
- Use case: encrypted IDs, value objects, or comma-separated collection parameters
- Registration via `AddControllers().AddMvcOptions(o => o.ModelBinderProviders.Insert(0, ...))`

**Answer**

Custom model binding is needed when the built-in binders cannot convert an incoming value to your parameter type — for example, binding a comma-separated query string `?ids=1,2,3` into `int[]`, or converting an encrypted customer ID to a `CustomerId` value object. Implement `IModelBinder` with a `BindModelAsync` method that reads the value from `ModelBindingContext.ValueProvider` and sets `ModelBindingContext.Result`, then write an `IModelBinderProvider` that returns your binder for the target type. Register via `builder.Services.AddControllers().AddMvcOptions(o => o.ModelBinderProviders.Insert(0, new CustomerIdModelBinderProvider()))`. Keep binders focused and fast since they run for every matching request, and prefer route constraints or custom `JsonConverter` for simple type mapping.

---

## Q16. What happens when `[FromBody]` reads a request with the wrong Content-Type?

**Concepts**
- 415 Unsupported Media Type returned when no formatter accepts the `Content-Type`
- `[Consumes("application/json")]` declaring explicit media type requirements
- Missing `Content-Type` header also causing 415 with `[ApiController]`
- `SuppressConsumesConstraintForFormFileParameters` relaxing this for file uploads

**Answer**

When the request `Content-Type` header does not match any registered input formatter, ASP.NET Core returns 415 Unsupported Media Type before binding runs. For `[FromBody]`, the default formatter is System.Text.Json which accepts `application/json`. A POST without a `Content-Type` header or with `text/plain` does not match and returns 415. Add `[Consumes("application/json", "application/xml")]` to explicitly declare which media types an action accepts; this also appears in OpenAPI metadata. For `IFormFile` parameters, the framework expects `multipart/form-data`. Configure `ApiBehaviorOptions.SuppressConsumesConstraintForFormFileParameters = true` only if you need to bypass content-type checking for file endpoints.

---

## Q17. How does `[ApiController]` automatic 400 response interact with custom error handling?

**Concepts**
- `InvalidModelStateResponseFactory` customizing the 400 body shape
- Custom `ProblemDetailsFactory` for fully overriding the response
- `SuppressModelStateInvalidFilter = true` to opt out and validate manually
- Middleware-level `ProblemDetails` for non-400 errors remaining separate

**Answer**

By default, `[ApiController]` returns a `ValidationProblemDetails` JSON body with an `errors` dictionary when `ModelState.IsValid` is false. This auto-response runs before the action, so action-level error handling does not intercept it. Customize the shape by replacing `ApiBehaviorOptions.InvalidModelStateResponseFactory` — inject `ProblemDetailsFactory`, build your custom shape, and return an `ObjectResult`. For full control, set `options.SuppressModelStateInvalidFilter = true` to disable auto-400 and check `ModelState.IsValid` manually inside each action. The `AddProblemDetails()` middleware handles non-400 errors (404, 500) separately, so both integration points are needed for a fully consistent error contract.

---

## Q18. How do you validate nested complex objects?

**Concepts**
- DataAnnotations validating nested objects recursively by default
- `IValidatableObject` on the nested type for cross-property rules
- FluentValidation using `.SetValidator(new AddressValidator())` for nested types
- Depth limitations and null-reference checks on optional nested objects

**Answer**

DataAnnotations validate nested complex objects recursively — if `OrderDto.ShippingAddress` is a non-null `AddressDto` with `[Required]` and `[StringLength]` attributes, those are evaluated and errors appear in `ModelState` under keys like `ShippingAddress.Street`. `IValidatableObject` on a nested type enables cross-property rules at that level. With FluentValidation, compose validators with `.SetValidator(new AddressValidator())` on the child property rule, and use `.When(x => x.ShippingAddress != null)` to skip validation when the nested object is optional. Deeply nested validation is handled automatically in all three approaches, but null-safety on optional nested objects must be handled explicitly — a null child object skips the child's `[Required]` checks unless a `[Required]` is on the parent property itself.

---

## Gotchas — ASP.NET Core (Interview Traps)

---

#### Gotcha 1. Middleware order — routing before auth

**Concepts**
- `UseRouting` before `UseAuthentication` and `UseAuthorization`
- Endpoint metadata availability for auth middleware
- Correct pipeline order in `Program.cs`

**Answer**

In ASP.NET Core 8 endpoint routing, `UseRouting` must run before `UseAuthentication` and `UseAuthorization` so the auth middleware can inspect endpoint metadata — registering auth before routing means the endpoint has not been selected yet, which breaks endpoint-aware authorization and policy resolution. The recommended order is exception handling → forwarded headers → routing → authentication → authorization → endpoints. Symptoms of wrong order include anonymous access to protected endpoints or 401 responses without proper challenge behavior.

---

#### Gotcha 2. Scoped service in a Singleton

**Concepts**
- Captive `DbContext` living past its scope
- Stale EF change tracker accumulating unrelated entities
- `ValidateScopes` as the detection mechanism

**Answer**

Registering a scoped service such as `DbContext` into a singleton creates a captive dependency that lives for the application lifetime while the scoped instance is disposed after its first scope ends, causing stale data, thread-safety bugs, or `ObjectDisposedException`. The singleton holds one scoped instance forever instead of one per request, so EF change trackers accumulate unrelated entities across requests. Enable `ValidateScopes` in Development to catch illegal scope combinations at startup, and fix by injecting `IServiceScopeFactory` or `IDbContextFactory<T>` and creating a scope per operation.

---

#### Gotcha 3. `new HttpClient()` in a singleton

**Concepts**
- Socket exhaustion from per-use `HttpClient` instantiation
- `HttpMessageHandler` lifecycle managed by `IHttpClientFactory`
- Named and typed client registration pattern

**Answer**

Instantiating `HttpClient` with `new` inside a long-lived singleton prevents socket reuse and causes socket exhaustion under load because each instance holds its own connection pool until garbage-collected. `HttpClient` is disposable but not meant for per-use disposal — the OS connection handle is held by the handler, not the client object. `IHttpClientFactory` manages `HttpMessageHandler` lifetimes and recycles connections correctly; register named or typed clients with `builder.Services.AddHttpClient<IExternalApi, ExternalApiClient>()`.

---

#### Gotcha 4. `IOptions<T>` vs reload

**Concepts**
- `IOptions<T>` — fixed snapshot at first resolution
- `IOptionsSnapshot<T>` — per-request recalculation, scoped
- `IOptionsMonitor<T>` — singleton-safe with change notifications
- Stale configuration when `.Value` is cached in a constructor field

**Answer**

`IOptions<T>` captures a configuration snapshot at first resolution — reading `.Value` once in a singleton constructor freezes settings even when `appsettings.json` reloads with `ReloadOnChange` enabled. `IOptionsSnapshot<T>` recalculates per request scope so a singleton cannot inject it without creating a captive dependency. `IOptionsMonitor<T>` is the singleton-safe wrapper that supports change notifications via `OnChange` and exposes `CurrentValue` for the latest merged configuration.

---

#### Gotcha 5. GET with `[FromBody]`

**Concepts**
- HTTP GET body stripped by clients, proxies, and CDNs
- `[FromQuery]` with `[AsParameters]` for complex GET filters
- Silent binding failure rather than explicit error

**Answer**

Using `[FromBody]` on GET action parameters is an anti-pattern because HTTP GET semantics discourage bodies, and many clients, proxies, and caches strip or ignore GET request bodies, so binding fails silently in production. Query strings and route values are the correct binding sources for GET requests, and complex filters should use `[FromQuery]` with `[AsParameters]` or flattened query keys.

---

#### Gotcha 6. PascalCase JSON keys with default camelCase policy

**Concepts**
- Default `JsonNamingPolicy.CamelCase` in ASP.NET Core 8
- Silent binding to default values on case mismatch
- `PropertyNameCaseInsensitive` as a compatibility bridge

**Answer**

ASP.NET Core 8 Web API serializes JSON with camelCase property names by default, so incoming JSON with PascalCase keys may not bind unless case-insensitive matching is enabled. Mobile or legacy clients sending PascalCase appear to succeed but properties remain default values. Enable `JsonSerializerOptions.PropertyNameCaseInsensitive = true` as a bridge, or standardize clients on camelCase and add `[Required]` validation so silent binding failures become 400 responses instead of corrupt data.

---

#### Gotcha 7. `throw ex` vs `throw`

**Concepts**
- `throw ex` resetting the stack trace to the catch block
- `throw;` preserving the original stack trace
- `InnerException` preservation when wrapping in a new exception

**Answer**

Rethrowing with `throw ex` resets the stack trace to the catch block line, hiding the original failure location in logs and diagnostics, while bare `throw` preserves the full stack trace from where the exception was first thrown. Exception filters, middleware, and Application Insights rely on accurate stack traces for root-cause analysis, so always use `throw;` when rethrowing after logging or cleanup in a catch block. Wrap in a new exception only when adding context — `throw new OrderProcessingException("...", ex)` — to preserve `InnerException`.

---

#### Gotcha 8. Kestrel as the only production layer

**Concepts**
- Kestrel as application server vs full edge gateway
- TLS termination, WAF, and rate limiting at the reverse proxy
- `UseForwardedHeaders` required for accurate client IP and scheme

**Answer**

Running Kestrel exposed directly to the internet without a reverse proxy skips TLS termination at the edge, centralized rate limiting, WAF protection, and efficient static-file caching. Kestrel is production-grade as an application server but is not a full edge gateway — nginx, IIS, Azure Front Door, or AWS ALB commonly sit in front. Direct exposure also complicates client IP logging unless `UseForwardedHeaders` is configured with a trusted proxy.

---

#### Gotcha 9. `launchSettings.json` in production

**Concepts**
- `launchSettings.json` as development-only launch configuration
- Production host using environment variables, not launch profiles
- `ASPNETCORE_ENVIRONMENT` and `ASPNETCORE_URLS` as runtime configuration

**Answer**

Settings in `Properties/launchSettings.json` apply only when starting from Visual Studio, VS Code, or `dotnet run` with a profile; they are not deployed to production hosts. Production URLs and environment come from environment variables (`ASPNETCORE_URLS`, `ASPNETCORE_ENVIRONMENT`), container configuration, or IIS/nginx site settings. Use `appsettings.Production.json` and host-level env vars for production values.

---

#### Gotcha 10. Non-nullable `bool` for PATCH semantics

**Concepts**
- Non-nullable `bool` defaulting to `false` on JSON omission
- Three-state intent: unspecified, opt-in, opt-out
- `bool?` or enum tri-state for partial-update DTOs

**Answer**

A non-nullable `bool` property cannot distinguish "field omitted from JSON" from "explicitly set to false" because System.Text.Json deserializes missing properties to `default(false)`, corrupting partial-update semantics. PATCH endpoints need `bool?`, separate update DTOs, or enums such as `Unspecified | OptIn | OptOut` for tri-state intent. Marketing consent and feature flags are common domains where this bug causes compliance or logic errors.

---

#### Gotcha 11. Forgetting `UseForwardedHeaders` behind a proxy

**Concepts**
- `X-Forwarded-Proto` and `X-Forwarded-For` headers
- Wrong scheme causing broken HTTPS redirects and cookie secure flags
- `KnownProxies` configuration to prevent header spoofing

**Answer**

Without forwarded headers middleware configured with known proxy IPs, `HttpContext.Request.Scheme` remains `http`, `Request.Host` reflects the internal address, and client IP is the proxy — breaking HTTPS redirects, cookie secure flags, and audit logs. Call `UseForwardedHeaders()` early, before middleware that reads scheme or host. Configure `ForwardedHeadersOptions` to trust only your reverse proxy network since trusting all proxies enables header spoofing.

---

#### Gotcha 12. Static files in `wwwroot` are public

**Concepts**
- `UseStaticFiles()` serving all `wwwroot` contents unauthenticated
- Secrets and config files must stay outside the web root
- Build pipeline verification before deploy

**Answer**

Any file under `wwwroot` is served by `UseStaticFiles()` to unauthenticated clients by default, so placing secrets, `.env`, backup configs, or private keys there exposes them over HTTP. Only public assets belong in `wwwroot`, while sensitive configuration stays outside the web root and is loaded through `IConfiguration`, environment variables, or secret managers. Use build pipelines to verify web root contents before deploy.

---

#### Gotcha 13. `MapFallbackToFile` intercepting API routes

**Concepts**
- SPA fallback returning `index.html` for unmatched routes including `/api/*`
- API endpoint registration ordering before fallback
- CORS and Swagger failures masked by HTML responses

**Answer**

SPA fallback middleware registered before API endpoint mapping returns `index.html` for `/api/*` 404 responses, making API failures look like successful HTML responses to clients and breaking JSON parsers. Map API routes (`MapControllers`, minimal API groups) before `MapFallbackToFile("index.html")`, and scope fallback to non-API paths. The correct order in `Program.cs` is: API endpoints first, static files, fallback last.

---

#### Gotcha 14. Background service without scope factory

**Concepts**
- Singleton `BackgroundService` incompatible with constructor-injected scoped services
- `CreateAsyncScope()` per job to create a fresh scope
- `ValidateScopes` catching this defect at startup

**Answer**

A singleton `BackgroundService` that injects scoped services directly into its constructor fails at startup with scope validation errors or uses disposed instances after the first background iteration. Inject `IServiceScopeFactory`, create `await using var scope = factory.CreateAsyncScope()` per job, resolve scoped services inside the scope, and dispose when the job completes. Enabling `ValidateScopes` catches this defect before production deployment.

---

#### Gotcha 15. SignalR without a backplane on multiple instances

**Concepts**
- SignalR hub broadcasting to connected clients on the same instance only
- Redis or Azure Service Bus backplane for multi-node event routing
- Sticky sessions insufficient without a backplane

**Answer**

SignalR broadcasts from one server instance reach only clients connected to that instance — without a Redis or Azure Service Bus backplane, users on different nodes never receive each other's real-time events. Sticky sessions keep one client on one node but do not route events raised on other nodes to that client. Register `AddSignalR().AddStackExchangeRedis(...)` with a consistent channel prefix per application, and test scale-out with at least two instances before launch.

---

## Scenario-Based Questions (Karat Format)

---

#### Q1. (R) Two clients consume the same POST endpoint. Client A (a legacy mobile app) sends PascalCase JSON keys; Client B (a web app) sends camelCase. Neither client reports errors but several fields arrive as default values.

```json
// Client A
{ "FirstName": "Alice", "Age": 30 }

// Client B
{ "firstName": "Alice", "age": 30 }
```

```csharp
[HttpPost("register")]
public IActionResult Register([FromBody] UserRegistration model)
    => Ok(new { model.FirstName, model.Age });
```

**Concepts**
- Default camelCase policy deserializing PascalCase keys as unrecognized
- `PropertyNameCaseInsensitive = true` accepting both cases
- Silent default-value binding instead of explicit 400
- Consistent single-casing contract being the long-term fix

**Answer**

ASP.NET Core 8 uses `JsonNamingPolicy.CamelCase` by default, which means the deserializer expects `firstName` and `age`. When Client A sends `FirstName` (capital F), System.Text.Json treats it as an unrecognized property and leaves `model.FirstName` as `null`, since case sensitivity is exact by default. Client B binds correctly. Enable `PropertyNameCaseInsensitive = true` in `AddJsonOptions` to accept both casing variants as an immediate fix. The long-term fix is standardizing both clients on camelCase — documented in OpenAPI — and adding `[Required]` on mandatory properties so silent binding failures produce 400 responses instead of null values that propagate into the database. If both clients cannot be updated simultaneously, a versioned endpoint or a custom `NamingPolicy` that accepts the legacy format only on the v1 path is the cleanest migration path.

---

#### Q2. (R) A PATCH endpoint updates marketing consent. Sending `{ "marketingConsent": false }` and omitting `marketingConsent` entirely produce identical server behavior, even though the business rule requires three states: opted in, opted out, and unspecified.

```csharp
public class ConsentUpdate
{
    public bool MarketingConsent { get; set; }
}

[HttpPatch("consent")]
public IActionResult UpdateConsent([FromBody] ConsentUpdate update) { ... }
```

**Concepts**
- Non-nullable `bool` defaulting to `false` for both omitted and explicit `false`
- Three-state consent requiring `bool?`, enum, or JSON merge-patch strategy
- Business-logic implication: omitted consent not clearing stored preference
- `[JsonRequired]` preventing silent omission on non-optional fields

**Answer**

`bool` defaults to `false` when omitted from JSON, so System.Text.Json cannot distinguish "the client intentionally revoked consent" from "the client sent a PATCH that did not mention consent at all." Change `MarketingConsent` to `bool?` so the three states map to `true` (opted in), `false` (opted out), and `null` (not specified, keep existing value). In the action, apply the change only when the value is non-null: `if (update.MarketingConsent.HasValue) user.Consent = update.MarketingConsent.Value`. For PATCH with many optional fields, a JSON Merge Patch approach (`application/merge-patch+json`) is the canonical solution where the patch document omits unchanged fields. Alternatively, model the intent explicitly with an enum: `MarketingConsent: "OptIn" | "OptOut"` with no default — then omission means no change and an explicit value means a deliberate choice.

---

#### Q3. (R) A GET search endpoint uses `[FromBody]` for its filter object. In development it works, but integration tests on CI return empty results because the filter is always default values.

```csharp
[HttpGet("search")]
public IActionResult Search([FromBody] ProductFilter filter) { ... }
```

**Concepts**
- HTTP GET body stripped by many test harnesses and CI environments
- `[FromQuery]` with `[AsParameters]` as the correct replacement
- `WebApplicationFactory` not stripping GET bodies — masking the real problem
- Route or query string as the only reliable parameter source for GET

**Answer**

HTTP GET request bodies are technically allowed by the spec but are stripped by proxies, CDNs, some test harnesses, and integration-test clients. `WebApplicationFactory.CreateClient()` in ASP.NET Core does pass GET bodies, so local tests pass, but CI runs through nginx or a build agent whose `HttpClient` policy strips them. Fix by changing the binding source to `[FromQuery]` — or `[AsParameters]` with a record that groups the filter properties — so the search parameters travel in the URL query string where every HTTP client and proxy supports them reliably. The route `/search?term=shoes&minPrice=20` is also cacheable, bookmarkable, and inspectable in logs in ways that body-based GET queries are not.

---

#### Q4. (R) After removing `[ApiController]` from a controller, validation errors stopped returning JSON; the endpoint now returns a plain text "400 Bad Request" string instead of ProblemDetails.

```csharp
// [ApiController]  ← removed during refactor
[Route("api/[controller]")]
public class OrdersController : ControllerBase { ... }
```

**Concepts**
- `[ApiController]` providing automatic 400 `ValidationProblemDetails` response
- Without it, `ModelState.IsValid` must be checked manually in each action
- `ProblemDetailsFactory` and `AddProblemDetails()` for consistent error formatting
- Client SDK breaking on plain-text 400 expecting JSON

**Answer**

`[ApiController]` installs an action filter that checks `ModelState.IsValid` before the action runs and returns a `ValidationProblemDetails` JSON body. Without it, the framework does nothing automatically — the plain "400 Bad Request" comes from Kestrel or a middleware issuing a generic status response when no body has been written. Restore `[ApiController]` on the controller, or add an explicit check at the top of each action: `if (!ModelState.IsValid) return ValidationProblem(ModelState)`. For application-wide error formatting, call `builder.Services.AddProblemDetails()` and `app.UseExceptionHandler()` to emit ProblemDetails for all error categories, but this does not replace the per-action ModelState check without `[ApiController]`.

---

#### Q5. (R) A GET endpoint accepts a sorting parameter as a nested query object. The `Sort` property is always null even though the client sends `?sort.field=name&sort.direction=asc`.

```csharp
public class ProductQuery
{
    public string? Search { get; set; }
    public SortOptions? Sort { get; set; }
}

public class SortOptions
{
    public string Field { get; set; } = "";
    public string Direction { get; set; } = "asc";
}

[HttpGet]
public IActionResult List([FromQuery] ProductQuery query) { ... }
```

**Concepts**
- Complex nested `[FromQuery]` binding using dot-notation keys
- `SortOptions` being a nullable reference — binder not initializing it automatically
- Explicit constructor or default initialization resolving the null
- Alternative: flat query parameters avoiding the nesting issue

**Answer**

The dot-notation keys `sort.field` and `sort.direction` are what ASP.NET Core expects for nested `[FromQuery]` binding — the client is sending the right keys. The `Sort` property is null because the model binder creates a new `SortOptions` instance only when at least one key prefix matches, and whether it does depends on how the binder resolves the nullable reference type. Add a default initializer on `ProductQuery`: `public SortOptions Sort { get; set; } = new()`, which ensures the nested object exists before the binder populates its properties, since binding writes into the existing instance rather than constructing a new one. A simpler alternative is to flatten the query into `SortField` and `SortDirection` properties on `ProductQuery` itself, eliminating the nested binding ambiguity entirely.

---

#### Q6. (D) Describe the trade-offs between `IValidatableObject` and FluentValidation for a cross-field rule: `EndDate` must be greater than or equal to `StartDate`.

**Concepts**
- `IValidatableObject.Validate()` running after DataAnnotations, inside the model class
- FluentValidation rule as a separate class with DI access and async support
- Co-location vs separation of concerns
- Testing `IValidatableObject` requires instantiating the model; testing FluentValidation is standalone

**Answer**

`IValidatableObject.Validate()` runs after all DataAnnotations pass and adds errors to `ModelState` via `IEnumerable<ValidationResult>`. For `EndDate >= StartDate`, this is straightforward: check in `Validate()` and `yield return new ValidationResult("EndDate must be >= StartDate", new[] { nameof(EndDate) })`. The rule lives on the model class itself — convenient and discoverable, but coupling validation to the domain object and making the class less portable. FluentValidation defines the rule in a separate `AbstractValidator<EventDto>` class: `RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate)`. The validator is independently testable, supports async rules for repository lookups (e.g., checking date availability against the database), and can be composed — a complex event booking might combine five validators. The trade-off is an extra dependency and registration step. Use `IValidatableObject` for simple co-located rules with no DI, and FluentValidation for rules that need async calls, external services, or when validation logic needs to live separately from the DTO.

---

#### Q7. (R) A POST endpoint accepts a raw text body for a webhook signature payload. The binding always fails — the action receives an empty string.

```csharp
[HttpPost("webhook")]
public async Task<IActionResult> ReceiveWebhook([FromBody] string payload, CancellationToken ct) { ... }
```

**Concepts**
- `[FromBody] string` not supported — formatter expects a JSON-encoded string, not raw text
- Reading `Request.Body` directly with `StreamReader` for raw payloads
- `[Consumes("text/plain")]` insufficient without a matching input formatter
- Disabling buffering with `EnableBuffering()` for HMAC signature verification

**Answer**

`[FromBody] string` does not bind to a raw text body — it expects a JSON string (a body like `"my payload"` wrapped in quotes), not a plain byte stream. A webhook body arriving as plain bytes with no JSON encoding is not a JSON string, so the JSON input formatter skips binding and `payload` is empty. Read the body directly with `await new StreamReader(Request.Body).ReadToEndAsync(ct)`, or call `Request.EnableBuffering()` before reading if you need to read the body twice (once for HMAC signature verification, once for payload processing). Alternatively, register a plain-text input formatter and annotate with `[Consumes("text/plain")]`, but reading the body stream directly is simpler for raw webhook payloads and does not require a formatter registration.

---

#### Q8. (R) A POST request sends invalid JSON (an integer where an object is expected). With `[ApiController]`, you expect a 400 response, but instead the server returns a 500 Internal Server Error. Explain why and how to fix it.

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    [HttpPost]
    public IActionResult Create([FromBody] CreateOrderRequest request) => Ok(request);
}
```

Client sends: `Content-Type: application/json` with body `42`.

**Concepts**
- JSON deserialization exception before ModelState is populated — bypassing `[ApiController]` filter
- `JsonException` thrown by System.Text.Json during deserialization reaching the exception handler
- `AddProblemDetails()` and `UseExceptionHandler` turning 500s into 400 ProblemDetails
- Exception filter or middleware needed to map deserialization errors to 400

**Answer**

When the JSON body is malformed or the wrong shape (an integer instead of an object), System.Text.Json throws a `JsonException` during deserialization before `ModelState` is populated. The `[ApiController]` automatic model validation filter only runs after binding succeeds — it inspects `ModelState` entries, which are empty when deserialization threw an exception. The exception propagates to the exception handler and becomes a 500. Fix this in two ways: first, configure `UseExceptionHandler` with `AddProblemDetails()` to convert unhandled exceptions into structured ProblemDetails — a `JsonException` can map to a 400 status in the exception handler. Second, in ASP.NET Core 8, `builder.Services.AddProblemDetails()` combined with `app.UseExceptionHandler()` handles this scenario correctly by mapping deserialization exceptions to 400 ProblemDetails responses automatically in newer framework versions. Verify by sending a body of `42` in an integration test and asserting status 400.

---

#### Q9. (M) A stock update endpoint mixes binding sources: SKU from the route, warehouse code from a header, and new quantity from the JSON body. Describe how to wire all three correctly and what breaks if the header or body is missing.

```csharp
[HttpPut("{sku}")]
public async Task<IActionResult> UpdateStock(
    string sku,
    [FromHeader(Name = "X-Warehouse-Code")] string warehouseCode,
    [FromBody] StockUpdate update,
    CancellationToken ct) { ... }
```

**Concepts**
- `[FromRoute]`, `[FromHeader]`, and `[FromBody]` each targeting separate request sections
- `[ApiController]` validating all binding sources, not just body
- Missing required header adding a `ModelState` error and producing 400
- `string?` vs `string` on `warehouseCode` controlling whether missing header is an error

**Answer**

ASP.NET Core binds `sku` from the `{sku}` route segment, `warehouseCode` from the `X-Warehouse-Code` header, and `update` by deserializing the JSON body — all three binding sources are evaluated independently. With `[ApiController]`, if the header is missing and `warehouseCode` is a non-nullable `string`, the `[Required]`-by-inferred-nullability rule adds a `ModelState` error and returns a 400 before the action runs. If `warehouseCode` is `string?`, a missing header binds as `null` and the action receives `null` — business logic must then decide whether to use a default warehouse or reject the request explicitly. The JSON body absence causes a `JsonException` or a deserialization failure that surfaces as a 500 (or 400 via `AddProblemDetails`) rather than a `ModelState` error. Make `warehouseCode` non-nullable and add `[Required]` explicitly, ensure `update` has required properties annotated with `[Required]` or `[JsonRequired]`, and test with a missing header to confirm the 400 ProblemDetails response includes the `X-Warehouse-Code` field name in the `errors` dictionary.
