# Model Binding & Validation — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is model binding in ASP.NET Core Web API?](#q1-what-is-model-binding-in-aspnet-core-web-api)
2. [Q2. What does `[FromBody]` do?](#q2-what-does-frombody-do)
3. [Q3. What does `[FromQuery]` do?](#q3-what-does-fromquery-do)
4. [Q4. What does `[FromRoute]` do?](#q4-what-does-fromroute-do)
5. [Q5. What is the difference between `[FromBody]` and `[FromQuery]`?](#q5-what-is-the-difference-between-frombody-and-fromquery)
6. [Q6. How does `[ApiController]` affect automatic model validation?](#q6-how-does-apicontroller-affect-automatic-model-validation)
7. [Q7. What HTTP status code does automatic validation failure return?](#q7-what-http-status-code-does-automatic-validation-failure-return)
8. [Q8. What is `ValidationProblemDetails`?](#q8-what-is-validationproblemdetails)
9. [Q9. What are data annotations for validation?](#q9-what-are-data-annotations-for-validation)
10. [Q10. What is the difference between `[Required]` and optional properties?](#q10-what-is-the-difference-between-required-and-optional-properties)
11. [Q11. What is `IValidatableObject`?](#q11-what-is-ivalidatableobject)
12. [Q12. What is FluentValidation and how does it integrate with Web APIs?](#q12-what-is-fluentvalidation-and-how-does-it-integrate-with-web-apis)
13. [Q13. Why can't GET requests reliably use `[FromBody]`?](#q13-why-cant-get-requests-reliably-use-frombody)
14. [Q14. What is complex type binding from query strings?](#q14-what-is-complex-type-binding-from-query-strings)
15. [Q15. What is the difference between model binding and validation?](#q15-what-is-the-difference-between-model-binding-and-validation)
16. [Q16. What does `[ValidateNever]` do?](#q16-what-does-validatenever-do)
17. [Q17. What is PATCH semantics for partial updates?](#q17-what-is-patch-semantics-for-partial-updates)
18. [Q18. How does camelCase JSON map to PascalCase C# properties?](#q18-how-does-camelcase-json-map-to-pascalcase-c-properties)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What is model binding in ASP.NET Core Web API?

**Concepts**
- Model binding — maps HTTP request data to action parameters
- Binding sources — route, query string, header, form, body
- `IModelBinder` — selected per parameter type and binding attribute
- `ModelState` — collects binding and conversion errors
- `[ApiController]` binding inference

**Answer**

Model binding is the framework mechanism that maps incoming HTTP request data — route values, query strings, headers, form fields, and JSON bodies — to action method parameters and complex types. It runs after routing selects an endpoint and before validation and the action execute, which means a binding failure prevents the action from running at all. The framework chooses the appropriate `IModelBinder` based on the parameter type, the HTTP verb, and explicit binding source attributes like `[FromBody]` or `[FromQuery]`. Binding failures add entries to `ModelState` keyed by property name. With `[ApiController]`, the framework applies opinionated conventions — complex types on POST actions get `[FromBody]` inferred automatically — which reduces the amount of explicit attribute decoration needed.

---

## Q2. What does `[FromBody]` do?

**Concepts**
- `[FromBody]` — reads and deserializes the request body stream
- Single body per request — one `[FromBody]` parameter allowed
- Input formatter selection via Content-Type
- `[ApiController]` body inference for complex types
- Malformed JSON — returns 400 before action executes

**Answer**

`[FromBody]` tells model binding to deserialize the HTTP request body into the parameter, typically from JSON via `System.Text.Json`. The request body is a single stream read once, which is why only one `[FromBody]` parameter is allowed per action — attempting to bind two `[FromBody]` parameters would require reading the stream twice. The JSON input formatter reads the body and maps properties to the target type using the configured naming policy, so camelCase keys bind to PascalCase properties by default. Malformed JSON or a type mismatch causes the formatter to fail before the action runs, producing a 400 Bad Request. With `[ApiController]`, complex types on POST, PUT, and PATCH actions get `[FromBody]` inferred when no other binding source applies, so the attribute is often implicit rather than explicit.

---

## Q3. What does `[FromQuery]` do?

**Concepts**
- `[FromQuery]` — binds from URL query string key-value pairs
- Prefix notation — dot-separated keys for complex types
- `[ApiController]` inference for simple types on GET
- Query binding — correct source for filters and pagination
- Case-insensitive property matching

**Answer**

`[FromQuery]` binds a parameter from the URL query string — the key-value pairs after `?` in the request URI. Simple types bind directly by name, so `GET /api/products?page=2&category=books` binds `page` and `category` to action parameters or a filter DTO with matching property names. Complex types use prefix notation where nested properties become dot-separated keys like `sort.field=createdAt&sort.descending=true`. The `[ApiController]` attribute infers `[FromQuery]` for simple types like `int`, `string`, and `bool` on GET actions automatically, so explicit `[FromQuery]` is mainly needed when disambiguation is required or when binding a complex filter object.

---

## Q4. What does `[FromRoute]` do?

**Concepts**
- `[FromRoute]` — binds from route template captured values
- Route constraints — `{id:int}` rejects invalid values at routing
- `[ApiController]` route inference for matching parameter names
- Explicit `[FromRoute]` — resolves parameter vs token name ambiguity

**Answer**

`[FromRoute]` binds a parameter from the values captured by the route template in the URI path. When the route template is `[HttpGet("{id:int}")]`, a request to `GET /api/orders/42` captures `id = 42` and binds it to the `int id` parameter. Route constraints like `{id:int}` reject non-integer values at routing time with 404 before model binding even starts, which is different from query string binding where a type mismatch adds a `ModelState` error. With `[ApiController]`, parameters whose names match route template tokens get `[FromRoute]` inferred automatically. Explicit `[FromRoute]` is required when the parameter name differs from the route token or when the parameter could be ambiguously resolved from multiple sources.

---

## Q5. What is the difference between `[FromBody]` and `[FromQuery]`?

**Concepts**
- `[FromBody]` — reads request body, one per action
- `[FromQuery]` — reads URL query string, multiple allowed
- GET body — unreliable across the HTTP ecosystem
- Cacheable GET — query string visible and cacheable
- HTTP verb appropriateness

**Answer**

`[FromBody]` reads the request body stream, which is a single-use resource, so only one `[FromBody]` parameter per action is supported. `[FromQuery]` reads URL query string parameters, supports multiple parameters, and is visible in the URL so browsers and CDNs can cache GET responses by URL. The key practical difference is HTTP verb appropriateness — `[FromBody]` belongs on POST, PUT, and PATCH actions that carry a payload, while `[FromQuery]` belongs on GET actions for filters, pagination, and lookup parameters. Using `[FromBody]` on GET is unreliable because many HTTP clients, proxies, and CDNs strip or ignore GET request bodies. Binding the same field from both sources simultaneously creates an ambiguous contract, so I pick one authoritative source per field.

---

## Q6. How does `[ApiController]` affect automatic model validation?

**Concepts**
- `[ApiController]` — enables automatic model validation short-circuit
- `ValidationProblemDetails` — automatic 400 response body
- `ModelState.IsValid` — no longer needs manual checking
- Binding source inference — `[FromBody]`, `[FromQuery]`, `[FromRoute]`
- `ConfigureApiBehaviorOptions` — customizes automatic behaviors

**Answer**

`[ApiController]` enables automatic model validation that short-circuits the action pipeline with HTTP 400 and a `ValidationProblemDetails` response body whenever `ModelState` is invalid, without requiring a manual `if (!ModelState.IsValid)` check in every action. DataAnnotations, `IValidatableObject`, and FluentValidation all run after binding and populate `ModelState` on failure, and invalid models never reach the action method body since a filter intercepts them first. The attribute also enables binding source inference, which reduces boilerplate by automatically applying `[FromBody]` to complex types and `[FromQuery]` to simple types based on the HTTP verb and parameter type. I customize the automatic validation behavior via `builder.Services.Configure<ApiBehaviorOptions>(o => o.InvalidModelStateResponseFactory = ...)` when I need a custom error shape.

---

## Q7. What HTTP status code does automatic validation failure return?

**Concepts**
- 400 Bad Request — automatic validation failure status
- `ValidationProblemDetails` — per-field error dictionary
- `application/problem+json` — RFC 7807 content type
- Binding conversion failures — may produce 400 or 404 depending on source
- Consistent 400 — enables uniform client-side error parsing

**Answer**

`[ApiController]` returns 400 Bad Request when automatic model validation fails, with the response body serialized as `ValidationProblemDetails` and the Content-Type set to `application/problem+json`. The `errors` dictionary in the body maps property names to arrays of validation messages keyed by camelCase property name, so a missing `email` field produces `"errors": { "email": ["The Email field is required."] }`. Malformed JSON also returns 400 but with a different problem type than field-level validation failures. Route constraint failures like a non-integer `{id}` value may return 404 at routing time rather than 400, since routing rejects the request before model binding runs. The consistent 400 shape across all endpoints lets SPAs and generated SDKs parse errors uniformly without endpoint-specific logic.

---

## Q8. What is `ValidationProblemDetails`?

**Concepts**
- `ValidationProblemDetails` — RFC 7807 ProblemDetails with errors dictionary
- `errors` property — field-to-message-array mapping
- `application/problem+json` — standardized error content type
- Standard fields — `type`, `title`, `status`, `detail`, `instance`
- Per-field error UI — enables form-level feedback

**Answer**

`ValidationProblemDetails` extends the RFC 7807 `ProblemDetails` type with an `errors` dictionary that maps property names to arrays of validation error messages. ASP.NET Core 8 uses it as the default 400 response body for invalid models on `[ApiController]` actions, so clients receive a standardized error contract across all endpoints rather than ad-hoc error strings. The standard fields include `type` (a URI identifying the problem kind), `title`, `status` (400), and optionally `detail` and `instance`. The Content-Type is `application/problem+json`, which distinguishes it from a normal JSON response and allows API gateways and clients to recognize it as an error contract. Clients parse the `errors` object for per-field UI feedback — for example, showing the error message next to the email field in a registration form.

---

## Q9. What are data annotations for validation?

**Concepts**
- Data annotations — declarative validation attributes on properties
- `[Required]` — null or empty value rejection
- `[Range]`, `[StringLength]`, `[EmailAddress]` — constraint attributes
- Property-scoped validation — cross-field rules need other mechanisms
- Automatic execution — runs without explicit calls on `[ApiController]` actions

**Answer**

Data annotations are declarative attributes applied to model properties that the validation pipeline evaluates after model binding succeeds. `[Required]` ensures a value was supplied, `[Range(1, 100)]` enforces numeric bounds, `[StringLength(200)]` limits string length, and `[EmailAddress]` validates format against an email pattern. They are property-scoped, which means each attribute validates a single property in isolation — cross-field rules like requiring `EndDate >= StartDate` cannot be expressed in annotations and need `IValidatableObject` or FluentValidation instead. On `[ApiController]` actions the framework calls validation automatically and returns 400 if any annotation fails, so I do not need to invoke validation manually or inspect `ModelState` in the action body.

---

## Q10. What is the difference between `[Required]` and optional properties?

**Concepts**
- `[Required]` — fails validation when value is null, empty, or missing
- Nullable reference types — implicit required for non-nullable `string`
- `int?` vs `int` — optional vs required numeric fields
- Missing JSON property — binds as null/default, not as validation failure without `[Required]`
- Explicit null vs omission — both fail `[Required]` on non-nullable types

**Answer**

`[Required]` marks a property as mandatory — the validation pipeline fails if the value is null, empty string, or absent after binding. Optional properties accept null or default values without triggering a required-field error. With nullable reference types enabled, a non-nullable `string Name` carries an implicit `[Required]`-equivalent warning at the language level, but the runtime validation still needs the explicit attribute to produce a `ModelState` error. For value types, `int` cannot be null so it is never absent — use `int?` to make a numeric field genuinely optional where omission means "no value provided." A missing JSON property binds as `null` for reference types or `default(T)` for value types, which only triggers `[Required]` if the annotation is present — without it, the binding succeeds silently with the default value.

---

## Q11. What is `IValidatableObject`?

**Concepts**
- `IValidatableObject` — cross-property validation interface
- `Validate(ValidationContext)` — implementation method
- `ValidationResult` with member names — attaches error to specific property
- Runs in same pass as DataAnnotations
- Suitable for a few cross-field rules; FluentValidation for larger sets

**Answer**

`IValidatableObject` is an interface implemented on a model class to perform cross-property validation inside a `Validate(ValidationContext context)` method. It runs during the same validation pass as DataAnnotations and any errors it returns are added to `ModelState` exactly like `[Required]` failures. I use it when one property's validity depends on another — for example, returning `new ValidationResult("EndDate must be after StartDate", new[] { nameof(EndDate) })` when `EndDate < StartDate`. The error attaches to the named property, so `ValidationProblemDetails` maps it to the correct field in the `errors` dictionary. For a small number of cross-field rules `IValidatableObject` is sufficient, but when a model has many complex rules or conditional logic I prefer FluentValidation validators in separate classes since they are easier to test in isolation.

---

## Q12. What is FluentValidation and how does it integrate with Web APIs?

**Concepts**
- `AbstractValidator<T>` — rule class separate from the DTO
- `AddFluentValidationAutoValidation()` — MVC pipeline integration
- `ModelState` merge — errors appear in `ValidationProblemDetails`
- Fluent rule API — conditional, async, and reusable rules
- DI registration — validators registered as services

**Answer**

FluentValidation defines validation rules in dedicated `AbstractValidator<T>` classes rather than on the DTO itself, keeping the DTO clean and making the rules testable in isolation without an HTTP context. I register validators via `builder.Services.AddFluentValidationAutoValidation()` and `AddValidatorsFromAssemblyContaining<CreateOrderValidator>()`, after which the MVC pipeline calls the matching validator during the model validation phase and merges any failures into `ModelState`. The errors appear in `ValidationProblemDetails` exactly like DataAnnotations failures, so the client receives the same 400 contract. FluentValidation supports conditional rules, async database checks, and reusable rule sets via `Include`, which makes it more expressive than DataAnnotations for domain-heavy models. I avoid duplicating the same rule in both DataAnnotations and FluentValidation on the same DTO since both execute and produce redundant errors.

---

## Q13. Why can't GET requests reliably use `[FromBody]`?

**Concepts**
- GET body — stripped by browsers, proxies, and CDNs
- HTTP caching semantics — GET body breaks cache key calculation
- `[FromQuery]` — correct source for GET filters
- `POST /search` pattern — for complex filter objects too large for URLs
- Integration test blindspot — localhost masks the issue

**Answer**

The HTTP specification does not prohibit a body on GET, but the practical ecosystem largely ignores it. Browsers and the Fetch API strip GET bodies, many HTTP client libraries warn against or silently drop them, and CDNs and API gateways typically do not forward GET bodies to the origin server. ASP.NET Core 8 does not reliably bind `[FromBody]` on GET endpoints because the request body may never arrive. Beyond client support, GET bodies break HTTP caching semantics — the cache key is based on the URL, so two GET requests to the same URL with different bodies appear identical to caches and produce stale responses. I use `[FromQuery]` for filter parameters on GET endpoints, and when the filter object is too complex for a query string I create a `POST /search` endpoint that explicitly accepts a body — this pattern is well understood and all HTTP clients handle it correctly. Integration tests running against localhost mask the issue because Kestrel does read GET bodies directly, so the bug only appears behind real proxies.

---

## Q14. What is complex type binding from query strings?

**Concepts**
- Complex type binding — public properties bound from query keys
- Dot notation — `sort.field=createdAt` for nested objects
- Bracket notation — `sort[field]` not supported by default
- Flattening — `sortField` avoids nesting complexity
- `[ApiController]` inference for complex types on GET

**Answer**

When a complex type parameter carries `[FromQuery]`, ASP.NET Core binds its public settable properties from query string keys using prefix dot notation — a parameter named `filter` with a `Category` property binds from `filter.category=books`. Nested objects extend the dot chain: `sort.field=createdAt&sort.descending=true` binds a `SortOptions` nested inside `PagedQuery`. The framework matches property names case-insensitively in this context. Some JavaScript frameworks send bracket notation — `sort[field]=createdAt` — which does not bind by default without a custom model binder, so I prefer flattening to `sortField` and `sortDescending` for public APIs to avoid client-side encoding issues. Deep nesting in query strings is also fragile for URL-encoding reasons, so I flatten anything beyond one level of nesting.

---

## Q15. What is the difference between model binding and validation?

**Concepts**
- Model binding — maps raw request data to CLR types
- Validation — checks bound values against business rules
- Binding errors — conversion failure, wrong source, 400/415
- Validation errors — rule violation after successful binding
- Route constraint failures — produce 404 before ModelState

**Answer**

Model binding maps raw request data to CLR types and populates action parameters — it converts the string `"42"` from a route segment to `int`, or deserializes a JSON body into a DTO. Validation checks whether those bound values satisfy constraints — `[Required]` ensures non-null, `[Range]` checks numeric bounds, and `IValidatableObject` enforces cross-field rules. Both contribute to `ModelState`, but they represent different failure kinds: a binding error means the data could not be converted to the expected type, while a validation error means the data was correctly typed but violates a rule. A binding failure on a missing required parameter may prevent validation from running on that property at all. Route constraints like `{id:int}` push binding failures to 404 at routing time, before `ModelState` is populated, which is a behavior difference developers sometimes find surprising.

---

## Q16. What does `[ValidateNever]` do?

**Concepts**
- `[ValidateNever]` — excludes property from validation pipeline
- Validation still binds — property value is set if present
- Navigation property skip — prevents over-validation of EF graphs
- Partial update DTOs — avoids validating server-populated fields
- Intentional use — can allow invalid data through if misapplied

**Answer**

`[ValidateNever]` excludes a property from the validation pipeline so DataAnnotations and FluentValidation rules do not run on it or its children. The property still binds from the request if the value is present — the exclusion is from validation only, not from binding. I use it to skip EF Core navigation properties accidentally included in API DTOs to prevent the validator from traversing an entire object graph, and on PATCH endpoint properties that are populated server-side and should not be validated from client input. The attribute requires deliberate use — applying it broadly to skip failing validations rather than fixing the underlying rule hides invalid data from reaching the business layer, so I treat any `[ValidateNever]` as something that should be explained by a code comment.

---

## Q17. What is PATCH semantics for partial updates?

**Concepts**
- PATCH — partial update, only present fields change
- Nullable types — distinguish "omit" from "explicit null"
- `bool?` — three-state: omitted, true, false
- `JsonPatchDocument<T>` — operation-based RFC 6902 JSON Patch
- OpenAPI nullability — `nullable: true` for optional patch fields

**Answer**

PATCH semantics require that only fields present in the request body change — fields omitted from the body must leave the stored value unchanged. This three-state requirement — omit (unchanged), explicit null (clear the field), and a real value — means non-nullable types cannot represent the "omit" state correctly. A non-nullable `bool EmailAlerts` deserializes as `false` when absent from the JSON body, conflating "the client didn't mention it" with "the client explicitly set it to false." The fix is to use `bool?`, `string?`, `DateOnly?`, and similar nullable types for all patch DTO properties, then apply updates only when the property `HasValue` or is non-null. The alternative is `JsonPatchDocument<T>` from RFC 6902, which models the patch as a list of explicit operations like `{ "op": "replace", "path": "/emailAlerts", "value": false }` so the intent is unambiguous. OpenAPI should mark patch properties as `nullable: true` so generated client SDKs can omit fields correctly.

---

## Q18. How does camelCase JSON map to PascalCase C# properties?

**Concepts**
- `JsonNamingPolicy.CamelCase` — serializes PascalCase to camelCase
- Case-sensitive deserialization default
- `PropertyNameCaseInsensitive` — inbound tolerance setting
- `[JsonPropertyName]` — per-property key override
- Silent data loss — PascalCase keys bind as default values

**Answer**

ASP.NET Core 8 Web API defaults to `JsonNamingPolicy.CamelCase` in `System.Text.Json`, so a C# property `CustomerName` serializes as `"customerName"` in outbound responses and expects the same key on inbound deserialization. The matching is case-sensitive by default, which means a legacy client sending `{ "CustomerName": "Acme" }` gets a null binding since the key casing does not match the policy. The fix is `PropertyNameCaseInsensitive = true` in `AddJsonOptions` for clients that cannot change their casing. Silent data loss occurs when clients send PascalCase keys and the property binds as null or zero — because `[Required]` checks for empty values rather than wrong-case keys, the validation may not catch it and a 201 response is returned with corrupt data. I treat camelCase as a published contract decision and document any override in the API changelog.

---

## Gotchas — Model Binding & Validation (Interview Traps)

---

#### Gotcha 1. `[FromBody]` vs `[FromForm]` for mixed file and JSON payloads

**Concepts**
- `[FromBody]` reads the raw request body as JSON/XML — incompatible with multipart
- `[FromForm]` reads `multipart/form-data` or `application/x-www-form-urlencoded`
- Combining `IFormFile` with `[FromBody]` DTO — fails because both try to read the same body
- Correct pattern: `[FromForm]` for the DTO alongside `IFormFile` in the same action

**Answer**

`[FromBody]` and `IFormFile` cannot coexist in the same action because both claim the request body stream and a request can only have one body. Sending a file alongside JSON metadata requires `multipart/form-data` encoding where the action uses `[FromForm]` on the metadata DTO and `IFormFile` on the file parameter — both bind from form fields, not the raw body. Attempting to use `[FromBody]` for the metadata alongside `IFormFile` causes either the file or the metadata to bind as null, with no binding error thrown.

---

#### Gotcha 2. Missing `[ApiController]` — `ModelState` not automatically checked

**Concepts**
- `[ApiController]` auto-returns `400 ValidationProblemDetails` when `ModelState` is invalid
- Without it, invalid models reach the action body silently
- Manual `if (!ModelState.IsValid) return ValidationProblem()` guard needed
- `[Required]` and `[Range]` annotations evaluated but not acted upon without the attribute

**Answer**

`[Required]` and `[Range]` Data Annotations are evaluated during model binding regardless of `[ApiController]`, but without it the framework does not automatically short-circuit to 400 — the request reaches the action with an invalid `ModelState`. An action that skips the manual `if (!ModelState.IsValid)` check then passes a partially-filled model to the service layer, potentially inserting corrupt data. I apply `[ApiController]` at assembly level in `Program.cs` to ensure every controller benefits from automatic model state validation.

---

#### Gotcha 3. Overposting via `[FromBody]` binding all model properties

**Concepts**
- `[FromBody]` binding every public property by default
- Client sending `IsAdmin = true` or `Id = 999` silently accepted and persisted
- `[BindNever]` / `[JsonIgnore]` preventing binding of sensitive properties
- Input DTOs (CreateXDto) as the correct pattern instead of entity types

**Answer**

Binding an EF entity or a model with privileged fields directly from `[FromBody]` allows clients to set any public property — including `IsAdmin`, `Id`, `CreatedAt`, or internal audit fields — by simply including them in the request JSON. This is a mass-assignment vulnerability. The fix is to use dedicated input DTOs (`CreateOrderDto`) that expose only the fields the client is permitted to supply, and map them to the entity in the service layer. If entity binding is unavoidable, `[BindNever]` or `[JsonIgnore]` on individual properties excludes them from binding.

---

#### Gotcha 4. `[Required]` on non-nullable value types is always valid

**Concepts**
- Non-nullable value type (`int`, `decimal`, `bool`) — always has a value by default
- `[Required]` on `int Price` — never fails because 0 satisfies the attribute
- Use `[Range(1, int.MaxValue)]` or make the type nullable `int?` to require a real value
- C# 8 nullable reference types changing semantics for reference type `[Required]`

**Answer**

`[Required]` on a non-nullable value type such as `public int Price { get; set; }` is effectively a no-op because model binding assigns the default value `0` for a missing field, and `0` satisfies `[Required]`. A client that omits `Price` entirely gets a successful response with `Price = 0` rather than a validation error. To require a real value, either make the property nullable (`public int? Price { get; set; }`) and keep `[Required]`, or add `[Range(1, int.MaxValue)]` to reject default-value submissions. For boolean fields this is especially subtle — `[Required]` on `bool Accepted` succeeds with `false`.

---

#### Gotcha 5. Complex object bound from query string without `[FromQuery]`

**Concepts**
- Complex types inferred as `[FromBody]` by `[ApiController]`
- `[FromQuery]` required for complex objects bound from query string
- `GET /api/orders?status=Pending&pageSize=10` — filter DTO needs `[FromQuery]`
- Each property mapped to a named query parameter

**Answer**

With `[ApiController]`, a complex type parameter without an explicit binding source attribute is inferred as `[FromBody]`, so a GET action with a `FilterDto` parameter tries to read the request body — which is typically empty for GET requests — instead of the query string. The DTO binds as null or with default values. The fix is `public IActionResult Search([FromQuery] FilterDto filter)`, which binds each property of `FilterDto` from a matching query parameter name. Nested complex types within a `[FromQuery]`-bound object are flattened — they do not support JSON-encoded nesting in query strings.

---

#### Gotcha 6. Array binding syntax from query string

**Concepts**
- Repeated parameter: `?ids=1&ids=2&ids=3` — standard MVC array binding
- Comma-separated: `?ids=1,2,3` — not bound as array by default, arrives as single string
- `[FromQuery] int[] ids` — binds repeated keys, not comma-separated
- Custom `ModelBinder` or string-split converter needed for comma-separated

**Answer**

`[FromQuery] int[] ids` binds from repeated query parameters — `?ids=1&ids=2&ids=3` — mapping each occurrence to an array element. It does not parse comma-separated values: `?ids=1,2,3` arrives as the string `"1,2,3"` and causes a type conversion failure or binds as a single invalid value. Clients that construct URLs with comma-separated list values must either switch to repeated parameters or the action must accept `string ids` and split manually. I document the expected query string format in OpenAPI using `[FromQuery(Name = "ids")]` and the standard repeated-key pattern.

---

#### Gotcha 7. FluentValidation not integrated into `ModelState` pipeline

**Concepts**
- FluentValidation requires explicit DI registration + `AddFluentValidation()`
- Without integration, validators never run — `ModelState` stays valid for invalid inputs
- `[ApiController]` auto-400 fires on `ModelState`, not FluentValidation result directly
- `.NET 7+` package: `FluentValidation.AspNetCore` registers validators automatically

**Answer**

Adding a FluentValidation `AbstractValidator<T>` class without calling `services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<T>())` results in validators that are never invoked — every request passes as valid. Even with the integration registered, FluentValidation runs validators and populates `ModelState`; the `[ApiController]` auto-400 then fires on that populated `ModelState`. If `FluentValidation.AspNetCore` is not installed and only the core `FluentValidation` package is present, the integration is absent and validators must be invoked manually.

---

#### Gotcha 8. Nullable reference type `string?` vs `[Required]` semantic difference

**Concepts**
- C# nullable annotations (`string?`) — compile-time nullability hint only
- `[Required]` — runtime model binding check, rejects missing/null/empty values
- `string Name` without `[Required]` — null accepted at runtime despite non-nullable annotation
- `.NET 6+` `SuppressImplicitRequiredAttributeForNonNullableReferenceTypes` option

**Answer**

C# nullable reference type annotations (`string` vs `string?`) are compile-time hints only — the model binder does not read them and will happily bind null to a `string Name` property at runtime if the field is missing in the payload. `[Required]` is the attribute that causes the model binder to reject null or missing values at runtime. In ASP.NET Core 6+, non-nullable reference type properties on model classes implicitly generate `[Required]`-like validation behavior when `SuppressImplicitRequiredAttributeForNonNullableReferenceTypes` is `false` — but this setting and its interaction with record types can surprise developers who rely on it without understanding its scope.

---

#### Gotcha 9. `[AsParameters]` in minimal APIs vs `[FromQuery]` binding behavior

**Concepts**
- Minimal API handler parameters — each declared separately or grouped via `[AsParameters]`
- `[AsParameters]` — binds an object's properties from route/query/header without `[FromBody]`
- `[AsParameters]` does not work in MVC controllers — minimal API only
- Complex type without `[AsParameters]` in minimal API — inferred as `[FromBody]`

**Answer**

In minimal API route handlers, a complex type parameter without any binding attribute is inferred as `[FromBody]`, mirroring `[ApiController]` behavior. `[AsParameters]` tells the minimal API pipeline to treat each property of the parameter type as a separate binding source (route, query, or header), which allows using a DTO to group filter parameters without sending a body. This attribute exists only for minimal APIs — it has no effect on `ControllerBase` actions, where `[FromQuery]` on the DTO is the equivalent pattern. Using `[AsParameters]` in a controller action compiles but silently does nothing.

---

#### Gotcha 10. `[BindRequired]` vs `[Required]` for query string parameters

**Concepts**
- `[Required]` — validates that a bound value is not null/empty, but missing key binds as null/default first
- `[BindRequired]` — adds a `ModelState` error when the key is absent from the request entirely
- GET with optional filter — `[Required]` accepts missing key (binds as null) without error
- `[BindRequired]` on query parameter — 400 when key not present in query string

**Answer**

`[Required]` validates the value after binding — if a query parameter key is missing entirely from the request, model binding assigns null or default first and then `[Required]` may or may not reject that default. For value types the default (0 or false) satisfies `[Required]`, so a missing key produces no error. `[BindRequired]` adds a `ModelState` error at the binding step if the key is absent from the request, regardless of value type. For mandatory query parameters where a missing key must be rejected, I use `[BindRequired]`. For optional parameters with a validation rule on the supplied value, I use `[Required]` or `[Range]` without `[BindRequired]`.

---

## Scenario-Based Questions (Karat Format)

---

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

**Concepts**
- `System.Text.Json` camelCase default — case-sensitive inbound matching
- Silent binding failure — PascalCase keys bind as default values
- `PropertyNameCaseInsensitive` — migration compatibility option
- `decimal CreditLimit` — zero is the default, masking the binding miss

**Answer**

The default `System.Text.Json` configuration uses camelCase with case-sensitive matching, so `"CreditLimit"` from the mobile client does not match the expected key `"creditLimit"` and binds as `0` — the default for `decimal`. Because `CreditLimit` has no `[Required]` attribute and zero is a valid value syntactically, no validation error fires and the action returns 201 with a silently incorrect record. The React SPA works because it sends camelCase keys that match the policy.

The immediate fix without touching the mobile client is `AddJsonOptions(o => o.JsonSerializerOptions.PropertyNameCaseInsensitive = true)` in `Program.cs`. The long-term fix is for the mobile client to adopt camelCase, documented in a migration changelog. I would also add `[Range(0.01, double.MaxValue)]` on `CreditLimit` so that a zero value from a binding miss is rejected rather than silently persisted, giving the team a validation error that surfaces the root cause during testing.

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

**Concepts**
- Three-state PATCH — omit vs false vs true cannot use non-nullable bool
- `bool?` — nullable types represent the third "omit" state
- Default value confusion — `false` is `bool` default and also a valid payload value
- `JsonIgnoreCondition.WhenWritingNull` — omit null properties from response
- OpenAPI `nullable: true` — correct schema for optional patch fields

**Answer**

The problem is that `bool EmailAlerts` has only two states — `true` and `false` — but the PATCH contract needs three: true (opt-in), false (opt-out), and absent (unchanged). When a client omits `EmailAlerts` from the JSON body, `System.Text.Json` deserializes the missing key as the default value `false`, which is indistinguishable from an explicit opt-out. The service layer cannot tell whether the client intended to clear the flag or simply did not mention it.

The fix is to change both properties to `bool? EmailAlerts` and `bool? SmsAlerts`. The service applies an update only when the property `HasValue`, ignoring null properties as "no change." The OpenAPI schema must mark these as `nullable: true` so NSwag and OpenAPI Generator produce nullable-typed client properties that can actually omit the field. I would also configure `JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull` so that response serialization does not include null fields, which keeps the PATCH response contract clean. A `JsonPatchDocument<UserPreferences>` is a valid alternative that represents each change as an explicit operation, but `bool?` DTOs are simpler for a small fixed set of properties.

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

**Concepts**
- `[FromBody]` on GET — request body stripped by proxies and browsers
- Query string vs body — different binding sources
- `[FromQuery]` — correct attribute for URL parameters
- GET body unreliability — localhost hides the issue from unit tests
- `[ApiController]` inference — would infer `[FromQuery]` for GET

**Answer**

The binding source is wrong — `[FromBody]` on a GET action attempts to read the request body, but the filter values are in the query string, not the body. Since there is no request body, the formatter finds nothing to deserialize and `filter` arrives as a default empty object. The query string values are simply ignored because no binding attribute targets them.

The fix is to replace `[FromBody]` with `[FromQuery]`, which binds each property of `ProductFilter` from the matching query string key: `category=electronics` maps to `Category`, and so on. With `[ApiController]` in place, removing the `[FromBody]` attribute entirely would cause the framework to infer `[FromQuery]` for a complex type on a GET action. I would also add an integration test with the actual query string to catch this class of error before it reaches production, since unit tests that construct the filter object directly would not reveal the binding source mismatch.

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

**Concepts**
- `[ApiController]` — enables automatic `ValidationProblemDetails` 400 response
- Manual `ModelState.IsValid` check — returns plain string instead of RFC 7807 format
- `ValidationProblem(ModelState)` — helper that returns correct `ValidationProblemDetails` body
- `application/problem+json` — content type the Angular client expects
- `AddProblemDetails()` — registers problem details services

**Answer**

Two issues combine to produce the plain-text 400. First, the controller lacks `[ApiController]`, so the automatic model validation short-circuit that returns `ValidationProblemDetails` does not activate — the action method body runs and reaches the manual check. Second, the manual check calls `BadRequest("Validation failed")`, which returns a plain string body with `Content-Type: text/plain`, not the `ValidationProblemDetails` RFC 7807 structure the Angular client parses for per-field error display.

The cleanest fix is to add `[ApiController]` to the controller and remove the manual `if (!ModelState.IsValid)` check entirely — the framework then intercepts invalid models before the action executes and returns `ValidationProblemDetails` automatically. If the manual check must stay for legacy reasons, replacing `BadRequest("Validation failed")` with `ValidationProblem(ModelState)` produces the correct RFC 7807 body. I would also add `builder.Services.AddProblemDetails()` to the service registration so the problem details middleware enriches error responses consistently.

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

**Concepts**
- Dot notation — `sort.field=createdAt` for nested complex type binding
- Bracket notation — `sort[field]` not supported by default model binder
- Query string key format mismatch — binder finds no matching keys
- Flatten to top-level — `sortField=createdAt` avoids nested binding

**Answer**

ASP.NET Core's default model binder uses dot notation — `sort.field=createdAt` — to bind nested complex types in query strings. The client is sending bracket notation — `sort[field]=createdAt` — which the default binder does not recognize, so it finds no keys matching the `Sort` property prefix and leaves it null. `Page` and `PageSize` bind correctly because they are flat top-level keys that match directly.

The simplest fix is to update the client to use dot notation: `?page=2&sort.field=createdAt&sort.descending=true`. The alternative is to flatten `SortOptions` into `PagedQuery` directly with properties `SortField` and `SortDescending`, eliminating the nested binding requirement and making the query string less ambiguous for any HTTP client. If the bracket notation is coming from a JavaScript framework that cannot change its serialization, a custom `IModelBinder` can be registered to handle bracket notation, but I prefer flattening for public APIs since it avoids that complexity.

---

#### Q6. (P) A booking API requires `EndDate >= StartDate` and `GuestCount <= RoomCapacity` on a create-reservation DTO. Compare `IValidatableObject` on the request model vs FluentValidation with `AddFluentValidationAutoValidation()` — execution order, testability, and how errors appear in ProblemDetails for `[ApiController]` endpoints.

---

**Concepts**
- `IValidatableObject.Validate` — runs after DataAnnotations in same pass
- FluentValidation `AbstractValidator<T>` — separate class, testable in isolation
- `ModelState` merge — both mechanisms produce `ValidationProblemDetails` errors
- `AddFluentValidationAutoValidation()` — hooks FluentValidation into MVC pipeline
- Cross-field rules — neither DataAnnotations nor single-property attributes cover these

**Answer**

Both approaches produce errors in `ValidationProblemDetails` and fire in the model validation phase — after binding and before the action executes — when `[ApiController]` is in place, so from the client's perspective the 400 response shape is identical.

`IValidatableObject` runs after all DataAnnotations on the model have been evaluated, within the same validation pass. I implement it by returning `ValidationResult` instances from the `Validate` method: `yield return new ValidationResult("EndDate must be after StartDate", new[] { nameof(EndDate) })`. Testing `IValidatableObject` requires instantiating the model and calling `Validator.TryValidateObject`, which works but couples the test to the DTO type.

FluentValidation separates rules into an `AbstractValidator<CreateReservationRequest>` class where I write `RuleFor(r => r.EndDate).GreaterThanOrEqualTo(r => r.StartDate).WithMessage("EndDate must be after StartDate")` and `RuleFor(r => r.GuestCount).LessThanOrEqualTo(r => r.RoomCapacity)`. The validator class is independently testable with `validator.TestValidate(request).ShouldNotHaveValidationErrorFor(...)` without any HTTP context. With `AddFluentValidationAutoValidation()` the MVC pipeline calls the validator automatically and merges errors into `ModelState` so they appear in `ValidationProblemDetails.errors` identically to DataAnnotations failures.

I use `IValidatableObject` for one or two simple cross-field checks and FluentValidation when there are multiple conditional rules, async database checks, or reusable rule sets across models.

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

**Concepts**
- `[FromBody] string` — raw JSON string body binding
- Route constraint `{orderId:int}` — rejects non-integer at routing, not at binding
- Zero orderId — default value indicates binding miss, not route failure
- `[FromRoute]` inference — `[ApiController]` infers route binding for matching parameter names
- Thread-safety under load — not relevant here, but separate reads of body

**Answer**

The `orderId` issue and the null `noteText` are likely two separate problems. The `orderId` binding as 0 under load suggests a concurrency or middleware issue where the route value is not being read correctly — but with `[ApiController]` in place and the route template containing `{orderId:int}`, the route constraint should reject non-integer values at routing time with 404 rather than binding them as 0. A more likely cause is that the route template and controller route prefix combine incorrectly, so the `orderId` segment is not being captured from the path. I would check that the controller-level `[Route]` prefix does not duplicate the `orders` segment.

For `noteText`, binding a raw JSON string to `[FromBody] string` is supported — `System.Text.Json` reads the JSON-quoted string and strips the quotes correctly when the Content-Type is `application/json`. If it arrives as null, the body stream may have been consumed by earlier middleware before reaching the formatter. Under load, if middleware like request logging or compression reads the body stream without rewinding it, subsequent `[FromBody]` binding finds an empty stream. I would check the middleware pipeline for any component that reads the request body without using `EnableBuffering()`, and consider wrapping the DTO in a request object rather than binding a raw string to make the contract explicit.

---

#### Q8. (M) An `[ApiController]`-annotated Web API receives a POST with valid JSON syntax but missing required fields and wrong types on optional nested objects. Walk through when automatic model validation runs, the default status code and payload shape in ASP.NET Core 8, and how `[ValidateNever]` vs `[Required]` change behavior for EF navigation properties accidentally included in a DTO.

---

**Concepts**
- Automatic model validation — runs after binding, before action, on `[ApiController]`
- `ValidationProblemDetails` — 400 body with `errors` dictionary
- `[Required]` — fires for null/missing values during validation
- `[ValidateNever]` — skips validation on a property and its children
- Navigation properties in DTOs — graph traversal by validator causes over-validation

**Answer**

With `[ApiController]`, the model validation pipeline runs immediately after model binding completes and before the action method body executes. For a POST with valid JSON syntax, the input formatter succeeds and the bound object is passed to the validator. The validator evaluates all DataAnnotations on every property — including nested objects — and collects failures into `ModelState`. If any failures exist, a filter short-circuits the pipeline and returns HTTP 400 with a `ValidationProblemDetails` body serialized as `application/problem+json`. The `errors` dictionary maps each failing property name in camelCase to an array of messages, for example `"email": ["The Email field is required."]`.

When an optional nested object has wrong types — say a `decimal` where an integer was expected — the binding itself fails before validation runs, producing a `ModelState` error of type `BindingError` rather than a `ValidationError`. Both appear in the same `errors` dictionary in the 400 response, so the client sees them identically.

If an EF Core navigation property is accidentally included in the DTO — say a `List<OrderLine> Lines` with DataAnnotations on `OrderLine` — the validator traverses into every element and runs annotations on them too. For a large collection this is expensive and may validate internal-only fields the API never intended to expose. Applying `[ValidateNever]` to that property tells the validator to skip it entirely, which is appropriate when the property is server-populated and should not be validated from client input. `[Required]` does the opposite — it ensures the property is present and non-null. For DTOs that should never include navigations at all, the correct fix is to remove the navigation from the DTO rather than papering over it with `[ValidateNever]`.

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

---

**Concepts**
- Dual binding source ambiguity — same business value from two sources
- Implicit priority logic — `quantity > 0` check is fragile
- Single authoritative source — one binding source per business field
- `int quantity` default — 0 when query param absent, same as "invalid"
- Contract confusion — OpenAPI must document which source wins

**Answer**

The action binds `quantity` from the query string and `body.Quantity` from the JSON body, then applies the rule `quantity > 0 ? quantity : body.Quantity`. This means the query string wins when it is present and non-zero, but falls back to the body otherwise. The problem is that 0 is also a valid query string value — if a client intentionally sends `quantity=0` to reset stock, the code treats 0 as "not provided" and uses the body value instead. Intermittent wrong amounts are almost certainly caused by clients sending both sources with different values and the priority logic behaving unexpectedly.

The fix is to pick one authoritative binding source and remove the other. For a PUT endpoint that updates a resource, the body is the natural home for all payload fields — I would remove `[FromQuery] int quantity` entirely, require the quantity in `InventoryAdjustRequest`, and add `[Range(0, int.MaxValue)]` to validate it. If the query string parameter must exist for backward compatibility, I would document the priority explicitly in both the code and the OpenAPI spec with a `[Produces]` or XML comment, and add an integration test that sends conflicting values and asserts which one wins.
