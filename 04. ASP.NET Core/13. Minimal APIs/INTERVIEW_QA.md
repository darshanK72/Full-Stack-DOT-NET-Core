# Minimal APIs — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What are Minimal APIs in ASP.NET Core?](#q1-what-are-minimal-apis-in-aspnet-core)
2. [Q2. How do Minimal APIs differ from controller-based APIs?](#q2-how-do-minimal-apis-differ-from-controller-based-apis)
3. [Q3. How do you define a GET endpoint with Minimal APIs?](#q3-how-do-you-define-a-get-endpoint-with-minimal-apis)
4. [Q4. What is `MapGet`, `MapPost`, `MapPut`, `MapDelete`?](#q4-what-is-mapget-mappost-mapput-mapdelete)
5. [Q5. How does parameter binding work in Minimal API route handlers?](#q5-how-does-parameter-binding-work-in-minimal-api-route-handlers)
6. [Q6. What is the difference between `Results.Ok()` and `TypedResults.Ok()`?](#q6-what-is-the-difference-between-resultsok-and-typedresultsok)
7. [Q7. What is `IResult`, and why use it?](#q7-what-is-iresult-and-why-use-it)
8. [Q8. What are endpoint filters in Minimal APIs?](#q8-what-are-endpoint-filters-in-minimal-apis)
9. [Q9. How do you add validation to a Minimal API endpoint?](#q9-how-do-you-add-validation-to-a-minimal-api-endpoint)
10. [Q10. How do you organize Minimal APIs with `MapGroup`?](#q10-how-do-you-organize-minimal-apis-with-mapgroup)
11. [Q11. How do you apply authorization to Minimal API endpoints?](#q11-how-do-you-apply-authorization-to-minimal-api-endpoints)
12. [Q12. How does OpenAPI/Swagger discover Minimal API endpoints?](#q12-how-does-openapiswagger-discover-minimal-api-endpoints)
13. [Q13. What is `ExcludeFromDescription()` used for?](#q13-what-is-excludefromdescription-used-for)
14. [Q14. When would you choose Minimal APIs over controllers?](#q14-when-would-you-choose-minimal-apis-over-controllers)
15. [Q15. When would Minimal APIs become a poor long-term choice?](#q15-when-would-minimal-apis-become-a-poor-long-term-choice)
16. [Q16. How is DI used in Minimal API handlers?](#q16-how-is-di-used-in-minimal-api-handlers)
17. [Q17. What is `AddEndpointsApiExplorer()`?](#q17-what-is-addendpointsapiexplorer)
18. [Q18. How do you return HTTP 201 Created from a Minimal API?](#q18-how-do-you-return-http-201-created-from-a-minimal-api)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What are Minimal APIs in ASP.NET Core?

**Concepts**
- MapGet/MapPost/MapDelete/MapPut as the surface area
- Same Kestrel host, middleware pipeline, and DI container as controllers
- First-class routed endpoints participating in authorization and OpenAPI
- Lambda, local function, or static method as handler

**Answer**

Minimal APIs are a lightweight way to define HTTP endpoints directly on the application builder without creating MVC controller classes, using `MapGet`, `MapPost`, and related extension methods to bind routes to delegates or methods in `Program.cs` or extension classes. They run on the same Kestrel host, middleware pipeline, and dependency injection container as controller-based apps — there is no separate runtime, only a smaller API surface area. A minimal endpoint is a first-class routed endpoint in ASP.NET Core 8 and participates in endpoint routing, authorization metadata, OpenAPI generation, and endpoint filters. Handlers can be lambda expressions, local functions, or static and instance methods registered through extension methods such as `TodoEndpoints.Map(app)`.

---

## Q2. How do Minimal APIs differ from controller-based APIs?

**Concepts**
- No automatic model validation — must use endpoint filters or explicit validation
- IEndpointFilter replacing MVC action/authorization filters
- IResult/TypedResults vs IActionResult/ActionResult<T>
- Scale via extension methods vs controller class conventions

**Answer**

Controller-based APIs organize endpoints in classes inheriting `ControllerBase` with `[ApiController]` conventions that automatically return 400 `ValidationProblemDetails` on model failure. Minimal APIs require endpoint filters or explicit validation since there is no `[ApiController]` behavior by default. MVC uses action filters, authorization filters, and result filters; minimal APIs use endpoint filters (`IEndpointFilter`) and middleware, with authorization applied via `.RequireAuthorization()` rather than `[Authorize]` attributes. Controllers return `IActionResult` / `ActionResult<T>` processed by MVC infrastructure; minimal APIs prefer `IResult` / `TypedResults` which write responses directly. Controllers scale well for large teams with established folder conventions; minimal APIs scale when the same separation is enforced through extension methods to avoid a monolithic `Program.cs`.

---

## Q3. How do you define a GET endpoint with Minimal APIs?

**Concepts**
- app.MapGet(route, handler) after builder.Build()
- Route parameter binding by name from template
- Service parameters resolving from request scope automatically
- Named endpoints via WithName() for link generation

**Answer**

Call `app.MapGet` with a route template and a handler delegate that returns a response type or `IResult`, registering the endpoint during application configuration after `builder.Build()`. Route parameters bind by name to handler parameters when types are compatible — `{id:int}` in the template binds to an `int id` parameter. Services such as `IItemService` resolve from the request scope automatically when listed as handler parameters without any attribute. Returning a plain object implicitly produces `200 OK` with JSON serialization using the configured `System.Text.Json` options.

```csharp
app.MapGet("/weather", () => new[] { "Sunny", "Cloudy" });

app.MapGet("/items/{id:int}", (int id, IItemService items) =>
    items.Find(id) is { } item ? Results.Ok(item) : Results.NotFound());
```

---

## Q4. What is `MapGet`, `MapPost`, `MapPut`, `MapDelete`?

**Concepts**
- Extension methods on IEndpointRouteBuilder registering verb-specific routes
- RouteHandlerBuilder fluent API — Produces, RequireAuthorization, WithTags
- MapMethods and Map for custom verbs
- Endpoint objects added to route table consumed by routing middleware

**Answer**

These are extension methods on `IEndpointRouteBuilder` (typically `WebApplication`) that register HTTP endpoints for a specific verb and path template, connecting them to a handler delegate and optional metadata. `MapGet` handles GET, `MapPost` handles POST, `MapPut` handles PUT, and `MapDelete` handles DELETE — each rejects requests using other verbs for that route unless you chain additional maps. They return `RouteHandlerBuilder`, which supports fluent configuration: `.Produces<T>()`, `.RequireAuthorization()`, `.WithTags()`, `.AddEndpointFilter<T>()`, and `.WithName()`. Under endpoint routing, these calls add `Endpoint` objects to the route table consumed by the routing middleware at request time.

---

## Q5. How does parameter binding work in Minimal API route handlers?

**Concepts**
- Route template values binding by parameter name
- [FromBody] once per request, [FromQuery], [FromHeader], [FromServices]
- DI types resolving from HttpContext.RequestServices per-request scope
- [AsParameters] aggregating route, query, and body into one object

**Answer**

The minimal hosting binder resolves handler parameters from route values, query strings, headers, the request body, dependency injection, and framework types such as `HttpContext` and `CancellationToken`, using attributes to disambiguate when multiple sources could apply. Route template values bind by parameter name — `/users/{userId}` binds to `Guid userId`; built-in constraints (`:int`, `:guid`) validate format before the handler runs. Types registered in DI such as `DbContext`, repositories, and `IOptions<T>` bind from `HttpContext.RequestServices` using the per-request scope without any attribute. `[AsParameters]` on a record or class aggregates multiple bindable properties from route, query, and body into one parameter object with predictable property-level binding.

---

## Q6. What is the difference between `Results.Ok()` and `TypedResults.Ok()`?

**Concepts**
- TypedResults.Ok<T> preserving generic response type for OpenAPI inference
- Results.Ok() working at runtime but losing schema metadata
- Both executing via IResult.ExecuteAsync without MVC infrastructure
- TypedResults preferred for Native AOT and source-generated OpenAPI

**Answer**

Both return an `IResult` that writes a 200 OK response, but `TypedResults.Ok<T>(T value)` preserves the generic response type at compile time so OpenAPI tools and source generators can infer accurate response schemas. `Results.Ok(dto)` works at runtime but often appears as an untyped or loosely typed schema in Swagger/OpenAPI documents, since the type information is not carried through the `IResult` interface. Both avoid allocating MVC `ObjectResult` infrastructure — the minimal pipeline executes `IResult.ExecuteAsync` directly. Prefer `TypedResults` when client code generation, contract testing, or Native AOT trimming depends on strongly typed endpoint metadata, since those scenarios require the generic type to be visible at the call site.

---

## Q7. What is `IResult`, and why use it?

**Concepts**
- IResult encapsulating status code, headers, and body serialization
- ExecuteAsync(HttpContext) writing the response directly
- Explicit alternate response shapes — Ok<T>, NotFound, ValidationProblem
- Custom IResult implementations for specialized response formatting

**Answer**

`IResult` is the interface implemented by built-in minimal API response helpers (`Results`, `TypedResults`) that encapsulates how to write an HTTP response — status code, headers, and body — without going through MVC result executors. Implementations such as `Ok<T>`, `NotFound`, `ValidationProblem`, and `Redirect` know how to serialize themselves through `ExecuteAsync(HttpContext)`. Returning `IResult` makes alternate response shapes explicit in the handler signature (`Task<IResult>`), which improves readability compared to implicit status codes. Chaining `.Produces<T>(StatusCodes.Status200OK)` on the route adds OpenAPI metadata even when the handler returns a custom `IResult` implementation.

---

## Q8. What are endpoint filters in Minimal APIs?

**Concepts**
- IEndpointFilter with EndpointFilterInvocationContext
- Runs after binding but before the handler
- Short-circuit by returning IResult without calling next
- Group-level filters via MapGroup for shared validation

**Answer**

Endpoint filters are hooks that run immediately before and after a minimal API route handler, similar to action filters in MVC but scoped to a single endpoint or group. Register with `.AddEndpointFilter<ValidationFilter>()` on a route or `MapGroup`, or register globally through DI as `IEndpointFilter`. Filters receive `EndpointFilterInvocationContext` with bound arguments and can return a result early — for example `Results.ValidationProblem(errors)` — without calling the next delegate. They run after routing and model binding but before the handler executes, which is the idiomatic place for request validation in Minimal APIs. Unlike middleware, endpoint filters see the specific bound parameters for that route.

---

## Q9. How do you add validation to a Minimal API endpoint?

**Concepts**
- No automatic [ApiController] validation — must opt in via filters
- Endpoint filter iterating Arguments and calling validator
- Results.ValidationProblem for RFC 7807 validation errors
- [AsParameters] record types for cohesive validation of request objects

**Answer**

Minimal APIs do not automatically validate models the way `[ApiController]` does, so validation must be added explicitly through endpoint filters, third-party libraries such as FluentValidation, or built-in validation extensions. An endpoint filter can iterate `context.Arguments`, run `Validator.TryValidateObject` or FluentValidation's `ValidateAsync`, and return `Results.ValidationProblem(errors)` on failure — stopping the handler from running. Data annotations on DTOs work when a filter explicitly invokes a validator; they are not enforced automatically. Combine validation filters with `[AsParameters]` record types so route, query, and body fields validate as one cohesive request object rather than separately.

---

## Q10. How do you organize Minimal APIs with `MapGroup`?

**Concepts**
- Route prefix and shared metadata applied once to a group
- RequireAuthorization and AddEndpointFilter inherited by all child routes
- Nested groups for versioning or domain boundaries
- Static extension methods for MapGroup registration to keep Program.cs readable

**Answer**

`MapGroup` creates a route prefix and shared configuration for related endpoints, letting you apply tags, authorization, filters, and OpenAPI metadata once instead of repeating it on every route. Groups inherit fluent metadata — `.RequireAuthorization("PolicyName")` on the group protects all mapped child routes unless a specific route calls `.AllowAnonymous()`. Nested groups compose cleanly for versioning (`/api/v1`, `/api/v2`) or domain boundaries. Extract group registration into static extension methods such as `TodoEndpoints.Map(app)` to keep `Program.cs` readable as the API grows.

```csharp
var todos = app.MapGroup("/api/todos")
    .WithTags("Todos")
    .RequireAuthorization();

todos.MapGet("/", GetAll);
todos.MapPost("/", Create);
todos.MapGet("/{id:int}", GetById);
```

---

## Q11. How do you apply authorization to Minimal API endpoints?

**Concepts**
- RequireAuthorization() and RequireAuthorization("PolicyName") on routes and groups
- AddAuthorizationBuilder() defining named policies
- Middleware order — UseAuthentication before UseAuthorization before endpoints
- AllowAnonymous() for public endpoints when a fallback policy requires auth

**Answer**

Register authentication and authorization services, place `UseAuthentication()` and `UseAuthorization()` after `UseRouting()` in the middleware pipeline, define policies with `AddAuthorizationBuilder()`, and call `.RequireAuthorization()` or `.RequireAuthorization("PolicyName")` on routes or groups. Middleware order matters — authentication and authorization must run after routing so endpoint metadata is available. Public endpoints omit authorization metadata or call `.AllowAnonymous()` when a fallback policy requires authentication globally. A fallback policy (`options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()`) secures everything by default, leaving `.AllowAnonymous()` as the explicit opt-out for health checks and public routes.

---

## Q12. How does OpenAPI/Swagger discover Minimal API endpoints?

**Concepts**
- AddEndpointsApiExplorer() for Swashbuckle metadata collection
- AddOpenApi() as the built-in .NET 8 alternative
- TypedResults and .Produces<T>() for accurate schema inference
- WithName, WithTags, WithSummary for documentation metadata

**Answer**

Register `AddEndpointsApiExplorer()` for Swashbuckle or `AddOpenApi()` for .NET 8's built-in OpenAPI support, then map endpoints with discoverable metadata. Returning opaque types or untyped `object` yields empty or generic schemas — `TypedResults.Ok<CustomerDto>(dto)` fixes inference for client code generation by carrying the generic type. `WithName`, `WithTags`, `WithSummary`, and `WithDescription` attach documentation metadata consumed by OpenAPI generators. Swashbuckle requires `AddSwaggerGen()` and middleware (`UseSwagger`, `UseSwaggerUI`); .NET 8's `MapOpenApi()` serves the document directly when configured. Internal routes can opt out with `.ExcludeFromDescription()`.

---

## Q13. What is `ExcludeFromDescription()` used for?

**Concepts**
- Omitting endpoints from the public OpenAPI document
- Endpoint remains callable — exclusion is documentation-only
- Typical uses — internal admin hooks and operational routes
- Security through documentation exclusion is not real security

**Answer**

`ExcludeFromDescription()` marks a minimal API endpoint so OpenAPI/Swagger generators omit it from the published API description — which is useful for internal diagnostics, health probes, or admin-only operations not intended for external clients. The endpoint remains fully callable at runtime — exclusion affects documentation only, not routing or authorization. Typical uses include `/internal/reload-cache`, operational hooks, or duplicate routes kept for backward compatibility during migration. Combine with proper authorization on sensitive routes, since excluding an endpoint from Swagger does not secure it from direct requests.

---

## Q14. When would you choose Minimal APIs over controllers?

**Concepts**
- Low ceremony for microservices, internal tools, and prototypes
- Fewer files when cross-cutting rules are simple
- Comparable performance to controllers for typical JSON APIs
- Still supports DI, middleware, auth, validation, and OpenAPI

**Answer**

Choose Minimal APIs when the service is small, the team wants minimal ceremony, or you need fast iteration on a focused HTTP surface — microservices, internal tools, prototypes, and simple CRUD APIs are common fits. Fewer files and no controller base-class hierarchy reduce boilerplate when endpoint count is low, since the same DI, middleware, auth, validation filters, and OpenAPI support are available when configured explicitly. Performance characteristics are comparable to controllers for typical JSON APIs — the choice is primarily about structure and team conventions rather than raw throughput. Cloud-native containerized workloads benefit from a compact startup path when paired with extension-method organization.

---

## Q15. When would Minimal APIs become a poor long-term choice?

**Concepts**
- God Program.cs as the common failure mode with inline lambdas
- Large teams preferring controller discoverability
- Complex validation, versioning, and hypermedia requiring disciplined structure
- Mixing conventions when the project already standardizes on MVC

**Answer**

Minimal APIs become harder to maintain when the API grows large, many cross-cutting concerns stack up, or the team relies on MVC conventions for consistency. The common failure mode is a sprawling `Program.cs` with inline lambdas, no test seams, and no separation of concerns. Large teams often prefer controllers for discoverability — `Controllers/OrdersController.cs` is immediately navigable — consistent filter usage, and established code-review patterns. Complex validation, versioning, and hypermedia requirements need the same architectural discipline as controllers: handlers in dedicated classes, filters for validation, and DI for all infrastructure. Minimal APIs are not wrong at scale, but they require explicitly enforcing the same boundaries that MVC provides by convention.

---

## Q16. How is DI used in Minimal API handlers?

**Concepts**
- Handler parameters resolved from HttpContext.RequestServices per request
- Scoped services safe because each request creates a scope
- Captive dependency risk when closures capture services at startup
- Static or class-based handlers for lambdas needing many dependencies

**Answer**

Handler parameters are resolved from the per-request `IServiceProvider` (`HttpContext.RequestServices`), so any service registered in DI can be injected by listing its type as a parameter — constructor injection applies to named methods, not inline lambdas that capture no service provider. Scoped services such as `DbContext` and repositories work correctly because each request creates a fresh scope. The captive dependency risk arises when closures capture references to services at startup time rather than per-request — avoid storing service references in variables outside the handler. For lambdas that need many dependencies, prefer static handler methods or class-based handlers registered through DI and referenced as method groups.

---

## Q17. What is `AddEndpointsApiExplorer()`?

**Concepts**
- Registers IApiDescriptionGroupCollectionProvider
- Swashbuckle SwaggerGen consuming endpoint metadata
- .NET 8 AddOpenApi() as an alternative pipeline
- Incomplete schemas without API explorer and Produces<T>() metadata

**Answer**

`AddEndpointsApiExplorer()` registers the API explorer services that collect endpoint metadata — HTTP methods, routes, parameters, and response types — from both minimal endpoints and controllers, enabling Swashbuckle and other tools to generate OpenAPI documents. It implements `IApiDescriptionGroupCollectionProvider`, which Swashbuckle's `SwaggerGen` consumes to build schemas and operation lists. Call it in the service configuration phase alongside `AddSwaggerGen()` when using Swashbuckle. Without an API explorer and without explicit `.Produces<T>()` metadata, minimal endpoints appear in Swagger with incomplete or generic schemas, since the framework has no way to infer response types from opaque return values.

---

## Q18. How do you return HTTP 201 Created from a Minimal API?

**Concepts**
- TypedResults.Created(uri, value) for compile-time response metadata
- Location header set to the new resource URI
- WithName() and link generation for route-based URI construction
- Results.CreatedAtRoute as the named-route equivalent

**Answer**

Return `TypedResults.Created<T>(uri, value)` or `Results.Created(uri, value)` from the handler, supplying the URI of the newly created resource for the `Location` header and the response body DTO. The first argument is the resource URI (string or `Uri`) placed in `Location` — it should identify the new entity, not the collection URL. Use `.WithName("CreateOrder")` and link generation helpers when the URI should be built from a route name rather than string interpolation. `Results.CreatedAtRoute` is available when named routes are defined, analogous to MVC's `CreatedAtAction`.

```csharp
app.MapPost("/orders", async (CreateOrderRequest req, IOrderService svc) =>
{
    var order = await svc.CreateAsync(req);
    return TypedResults.Created($"/orders/{order.Id}", order);
});
```

---

## Gotchas — Minimal APIs (Interview Traps)

---

#### Gotcha 1. Scoped services injected directly into route handlers are resolved from the root scope — use `[FromServices]` or handler parameters

**Concepts**
- Minimal API route handlers as delegates resolved at startup
- Service parameters bound from DI per request via implicit `[FromServices]`
- Root-scope resolution for captured closures vs per-request for handler parameters
- `IServiceScopeFactory` for manually scoped work inside handlers

**Answer**

In Minimal API handlers, parameters of interface or class types are automatically treated as `[FromServices]` and resolved from the request's scoped DI container per request. This is correct and is the intended mechanism. However, a handler that captures a service from outside via a closure — `var svc = app.Services.GetRequiredService<IOrderService>(); app.MapGet("/", () => svc.Get())` — resolves the service from the root (singleton) container once at startup, making a scoped service behave as a singleton. Always declare scoped services as handler parameters rather than capturing them in closures to get correct per-request lifetime behavior.

---

#### Gotcha 2. `TypedResults` provides better OpenAPI metadata than `Results` — prefer it for documented APIs

**Concepts**
- `Results.Ok(data)` returning `IResult` with no type information for OpenAPI
- `TypedResults.Ok(data)` returning `Ok<T>` preserving generic type for OpenAPI metadata
- `.Produces<T>()` extension required with `Results` to annotate OpenAPI output
- `TypedResults` making response type visible without additional `.Produces<T>()` calls

**Answer**

`Results.Ok(value)` returns an `IResult` that hides the response type from OpenAPI generation — the Swagger schema shows no response body type without explicitly chaining `.Produces<T>()`. `TypedResults.Ok(value)` returns `Ok<T>`, which preserves the generic type argument and allows OpenAPI generators to automatically infer the response schema without additional annotation. For endpoints documented in Swagger, consistently use `TypedResults` so schema generation is automatic. The difference matters most for response body types; both work correctly at runtime for sending the response to the client.

---

#### Gotcha 3. Endpoint filters wrap the entire handler, not just before and after phases separately

**Concepts**
- `IEndpointFilter.InvokeAsync(context, next)` wrapping handler execution
- Before-`next` code running pre-handler, after-`next` code running post-handler
- Short-circuit by returning an `IResult` without calling `next`
- `RouteHandlerBuilder.AddEndpointFilter<T>()` for per-endpoint filters

**Answer**

Minimal API `IEndpointFilter` works like a middleware delegate: `InvokeAsync` receives the handler invocation context and a `next` delegate representing the endpoint handler (or the next filter). Code before `await next(context)` runs before the handler; code after `next` runs after. Short-circuit by returning an `IResult` without calling `next` — the handler is never invoked. This pattern handles validation, audit logging, and caching. Unlike MVC filters, there is no separate exception filter type — exceptions from endpoint filters and handlers flow to the middleware pipeline's exception handling. The first registered filter is outermost; the last is innermost (closest to the handler).

---

#### Gotcha 4. `MapGroup` requires explicit route prefix — sub-groups do not inherit the parent prefix automatically without configuration

**Concepts**
- `RouteGroupBuilder` for organizing endpoints with shared prefix and metadata
- Group prefix applied to all endpoints registered on the builder
- Nested groups building prefixes cumulatively
- `WithTags()`, `RequireAuthorization()`, and `AddEndpointFilter()` applied to all group endpoints

**Answer**

`app.MapGroup("/api/orders")` creates a group where all endpoints registered on the builder receive the `/api/orders` prefix. The group prefix is an explicit argument — sub-groups do not inherit parent prefixes automatically unless the child group is created from the parent builder: `var orderGroup = apiGroup.MapGroup("/orders")`. Calling `app.MapGroup("/orders")` independently does not connect to an existing group. Group-level metadata — `WithTags("Orders")`, `RequireAuthorization()`, `AddEndpointFilter<LoggingFilter>()` — applies to all endpoints registered within the group, which is the primary organizational benefit of using groups over individual `MapGet/Post/Put/Delete` calls.

---

#### Gotcha 5. Route conflicts in Minimal APIs are detected at startup — unlike controller attribute routing

**Concepts**
- Minimal API routes verified at `app.Build()` time
- `InvalidOperationException` thrown at startup for conflicting routes
- Controller attribute route conflicts detected at runtime on first request
- Startup route conflict detection as an advantage of Minimal API routing

**Answer**

Minimal API routes are registered and verified at `builder.Build()` time — conflicting routes with identical HTTP method and path throw `InvalidOperationException` at startup before the first request. Controller-based attribute routing delays conflict detection until runtime, throwing `AmbiguousMatchException` on the first request to the conflicting path. This startup-time detection in Minimal API is an advantage: it surfaces routing bugs in integration tests and local startup rather than in production. When migrating from controllers to Minimal API, duplicate route templates from controller actions that were never discovered as conflicts may surface as startup errors.

---

#### Gotcha 6. `[FromBody]` is implicit for complex types in Minimal APIs — explicit attribute needed when mixing sources

**Concepts**
- Complex type parameters automatically bound from JSON body in Minimal API handlers
- Route and query string parameters bound by name matching
- `[FromBody]` explicitly required when mixing body with services parameters
- No `[ApiController]` attribute on Minimal APIs — binding rules differ slightly

**Answer**

In Minimal API route handlers, a complex type parameter that is not a service, `HttpContext`, `CancellationToken`, or a route/query parameter is automatically bound from the JSON request body — equivalent to `[FromBody]` in a controller. This is implicit and does not need the attribute in most cases. However, when a handler has multiple complex type parameters or when the intent is ambiguous, adding `[FromBody]` explicitly makes the binding source unambiguous for both the runtime and readers of the code. Unlike MVC controllers, Minimal API binding source rules are simpler and more deterministic, but they still require care when the same parameter name exists in both the route template and would otherwise be bound from the body.

---

#### Gotcha 7. `Results.Json` serializes with default options — `TypedResults.Ok` uses the configured `JsonOptions`

**Concepts**
- `Results.Json(obj)` using default `JsonSerializerOptions`
- `TypedResults.Ok(obj)` respecting `AddJsonOptions` registered serializer settings
- Inconsistent serialization: camelCase from `Ok` vs PascalCase from `Json`
- Content negotiation not applied to `Results.Json`

**Answer**

`Results.Json(obj)` creates a response using default `JsonSerializerOptions` — ignoring any custom naming policy, converters, or settings configured via `AddJsonOptions()`. `TypedResults.Ok(obj)` goes through the content negotiation pipeline and respects the registered `JsonSerializerOptions`, including camelCase naming, custom converters, and `ReferenceHandler` settings. Mixing `Results.Json` for some handlers and `TypedResults.Ok` for others produces inconsistent serialization in the same API — some endpoints return camelCase, others return PascalCase — which breaks clients that rely on consistent naming. Standardize on `TypedResults.Ok` for all handlers that should respect the configured serialization settings.

---

#### Gotcha 8. `.WithName()` is required for `LinkGenerator` to generate URLs for Minimal API endpoints

**Concepts**
- `RouteHandlerBuilder.WithName("EndpointName")` assigning an endpoint name
- `LinkGenerator.GetPathByName("EndpointName")` for URL generation
- Anonymous Minimal API endpoints not referenceable by name
- Consistent naming strategy for endpoints used in redirect responses

**Answer**

Minimal API endpoints do not have a name by default — they are anonymous route registrations. `LinkGenerator.GetPathByName("GetOrderById")` returns `null` for an unnamed endpoint because the route is not registered with a name in the endpoint data source. Call `.WithName("GetOrderById")` on the `RouteHandlerBuilder` to register the endpoint under a resolvable name. This is required when using `Results.RedirectToRoute("GetOrderById", ...)` or `Results.Created(generator.GetPathByName(...), order)` in handler responses. Establish a naming convention early in the project — using action-method-style names consistent with the HTTP method and resource name prevents naming collisions in large endpoint collections.

---

#### Gotcha 9. OpenAPI metadata requires explicit `.WithSummary()`, `.WithDescription()`, `.Produces<T>()` — not inferred from code

**Concepts**
- Minimal APIs not inferring OpenAPI summaries from XML documentation comments
- `.WithSummary()`, `.WithDescription()`, `.WithTags()` for endpoint documentation
- `.Produces<T>()` for documenting non-primary response types (errors, 201, 404)
- `TypedResults` partially automating 200-response type metadata

**Answer**

Unlike controller actions, Minimal API endpoints do not use XML documentation comments for OpenAPI summaries — `/// <summary>` on the lambda does nothing. Endpoint summaries, descriptions, and tags must be added fluently: `.WithSummary("Get an order by ID").WithDescription("Returns the full order details including line items").WithTags("Orders")`. `TypedResults.Ok<T>` automatically registers the 200 response type, but additional responses (404, 400, 422) must be added with `.Produces(404).Produces<ProblemDetails>(400)`. Without this metadata, Swagger UI shows undocumented endpoints with no schema information, making the API contract impossible to understand from the generated documentation.

---

#### Gotcha 10. Minimal API handlers cannot use `[Authorize]` attribute — use `.RequireAuthorization()` instead

**Concepts**
- `[Authorize]` attribute applying to controller actions, not Minimal API lambdas
- `.RequireAuthorization("PolicyName")` for per-endpoint authorization
- `RouteGroupBuilder.RequireAuthorization()` for group-level authorization
- Anonymous Minimal API endpoints bypassing authorization by default

**Answer**

`[Authorize]` is an MVC filter attribute and has no effect on Minimal API route handler lambdas or local function handlers — placing it before a lambda compiles but is silently ignored. Minimal APIs use the fluent `.RequireAuthorization()` extension method on the `RouteHandlerBuilder` returned by `MapGet/Post/Put/Delete`. Specify a policy name with `.RequireAuthorization("AdminPolicy")` or call without arguments to require any authenticated user. For groups of endpoints that share the same authorization requirement, apply `.RequireAuthorization()` on the `RouteGroupBuilder` rather than on each endpoint individually. Without explicitly calling `.RequireAuthorization()`, endpoints are anonymous by default.

---

## Scenario-Based Questions (Karat Format)

#### Q1. (R) Review this minimal API registration. The app starts and passes smoke tests, but order totals drift under concurrent load. What is wrong?

```csharp
var orderTotals = new Dictionary<int, decimal>();

app.MapPost("/orders/{id:int}/lines", async (int id, OrderLine line, AppDbContext db) =>
{
    db.OrderLines.Add(new OrderLineEntity { OrderId = id, Sku = line.Sku, Qty = line.Qty });
    await db.SaveChangesAsync();

    if (!orderTotals.ContainsKey(id))
        orderTotals[id] = 0;
    orderTotals[id] += line.UnitPrice * line.Qty;

    return Results.Ok(new { orderId = id, runningTotal = orderTotals[id] });
});
```

*(Assume `AppDbContext` is scoped and registered correctly.)*

**Concepts**
- Delegate-captured closure behaving as singleton state
- Dictionary<> not thread-safe under concurrent reads and writes
- In-memory aggregate diverging from persisted database state
- Scale-out producing different totals per instance

**Answer**

The handler captures `orderTotals` in a closure at startup, so the dictionary is shared across all requests and all users for the lifetime of the process — it behaves exactly like a static field. Under concurrent load, two requests for the same order execute the read-modify-write sequence simultaneously, each reading the same stale total and writing conflicting updates. `Dictionary<int, decimal>` is not thread-safe under concurrent writes, so the dictionary itself can corrupt. Beyond concurrency, the in-memory aggregate diverges from the database: restarts reset the dictionary while the database retains all lines, and behind a load balancer each instance holds a different subset of totals.

The fix is to remove the dictionary entirely and compute the running total from the database — a `SUM` query or a domain service that aggregates committed lines. If caching is needed, use `IMemoryCache` or a distributed cache with explicit expiration and cache keys scoped by order ID, not a bare closure-captured dictionary. The response should reflect committed state from the database, not an in-memory aggregate that has no consistency guarantees.

---

#### Q2. (M) A teammate returns `IActionResult` from a minimal API route handler while another uses `Results.Ok()` and `TypedResults.Created()`. When does each approach fit, and what does ASP.NET Core lose when you pick the wrong one?

**Concepts**
- IResult executing directly without MVC infrastructure
- TypedResults.Ok<T> carrying compile-time type for OpenAPI inference
- IActionResult adapted via compatibility shim — weaker metadata
- Naked DTO return implicitly 200 but hiding alternate response shapes

**Answer**

`IResult` and `TypedResults` are the idiomatic minimal API response types because the minimal pipeline executes them directly via `IResult.ExecuteAsync` without invoking MVC result executors or allocating `ObjectResult` infrastructure. `TypedResults.Created<Uri, T>(uri, value)` preserves generic response types so `AddOpenApi` and Swashbuckle can emit accurate status codes and body schemas — which matters for client code generation, contract tests, and Native AOT trimming.

Returning `IActionResult` via `new OkObjectResult(dto)` works through a compatibility shim but the generic type is not visible to OpenAPI tooling, so the schema appears loosely typed or untyped in generated documents. Returning naked DTOs such as `return order` is concise and implicitly produces 200 OK, but it hides alternate response shapes — 404, 422 — from OpenAPI metadata unless `.Produces<T>()` is chained. The right habit is `TypedResults` throughout, with `.Produces<T>()` for any non-200 response paths that TypedResults cannot infer from the return type alone.

---

#### Q3. (P) You need request-body validation on a minimal API POST without MVC controllers. How do you validate a DTO and return RFC 7807 `ProblemDetails` on failure using endpoint filters?

**Concepts**
- IEndpointFilter as the minimal API validation hook
- context.Arguments iteration with Validator.TryValidateObject
- Results.ValidationProblem for field-scoped RFC 7807 errors
- [AsParameters] for cohesive validation of route, query, and body

**Answer**

Register an endpoint filter that runs after model binding, validates bound arguments using `ValidationContext` or FluentValidation, and short-circuits with `Results.ValidationProblem(errors)` before the handler executes. Add `.AddEndpointFilter<ValidationFilter>()` on the route or a `MapGroup` so it applies consistently. In the filter, iterate `context.Arguments`, skip null values, call `Validator.TryValidateObject` or `_validator.ValidateAsync`, and on failure return `Results.ValidationProblem` with a dictionary of field names to error arrays — clients receive field-scoped errors in `ValidationProblemDetails` shape, consistent with RFC 7807.

Combine with `[AsParameters]` record types so route, query, and body parameters bind into one validateable object, since the filter sees a single argument rather than having to find the right one among many. Do not rely on `[ApiController]` automatic behavior — minimal endpoints opt in explicitly.

```csharp
public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next)
{
    foreach (var arg in ctx.Arguments)
    {
        if (arg is null) continue;
        var results = new List<ValidationResult>();
        if (!Validator.TryValidateObject(arg, new ValidationContext(arg), results, true))
            return Results.ValidationProblem(results.ToDictionary(
                r => r.MemberNames.FirstOrDefault() ?? "", r => new[] { r.ErrorMessage! }));
    }
    return await next(ctx);
}
```

---

#### Q4. (R) OpenAPI/Swagger shows every minimal endpoint as `200 OK` with an empty schema, and one POST is missing from the document entirely. Review the setup — what is wrong and how do you fix it for client generation?

```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/reports", async (ReportRequest req, IReportService svc) =>
{
    await svc.EnqueueAsync(req);
    return Results.Accepted();
});

app.MapGet("/reports/{id:guid}", (Guid id, IReportService svc) =>
    svc.GetStatus(id)); // returns anonymous object or DTO

// Dev-only internal endpoint — should not appear in public OpenAPI
app.MapDelete("/admin/purge-cache", () => Results.NoContent())
   .ExcludeFromDescription();
```

*(Focus on metadata, response typing, and document completeness — not generic "install Swashbuckle" advice.)*

**Concepts**
- Untyped return from service method — empty schema in OpenAPI
- Results.Accepted() without .Produces(202) — wrong or missing status
- ExcludeFromDescription() correct for internal admin routes
- TypedResults and .Produces<T>() for accurate response contracts

**Answer**

Two problems cause the degraded OpenAPI document, and one behavior is intentional. The `MapGet` returns whatever `svc.GetStatus(id)` returns — likely an anonymous object or an untyped `object` — so the schema generator sees no concrete type and emits an empty schema. The `MapPost` returns `Results.Accepted()` without `.Produces(202)` metadata, so Swashbuckle infers a 200 response or shows no status at all. The `ExcludeFromDescription()` on the admin route is correct and intentional — that endpoint should not appear in the public document.

The fixes are type-driven. For the GET, return `TypedResults.Ok<ReportStatusDto>(dto)` or chain `.Produces<ReportStatusDto>(200)` so the schema generator has a concrete type. For the POST, chain `.Produces(StatusCodes.Status202Accepted)` or return `TypedResults.Accepted(uri, payload)` so the status code is declared. Add `.WithName()` and `.WithTags("Reports")` for discoverability and consistent OpenAPI operation IDs. Every response type and status code must be declared or inferred from `TypedResults` — there are no MVC conventions filling in gaps.

---

#### Q5. (P) How do you protect a subset of minimal API endpoints with JWT bearer auth and a named authorization policy while leaving health checks anonymous? Where do you register requirements vs apply them on routes?

**Concepts**
- AddAuthentication + AddJwtBearer for scheme setup
- AddAuthorizationBuilder().AddPolicy() for named policy definitions
- RequireAuthorization("PolicyName") on routes or groups
- AllowAnonymous() on public endpoints when a fallback policy secures by default

**Answer**

Register authentication and authorization in services, then wire them in the pipeline, then apply policy metadata per route. `builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(...)` sets up JWT validation. `AddAuthorizationBuilder().AddPolicy("OrdersWrite", p => p.RequireRole("OrderWriter"))` defines the named policy. Middleware order: `UseAuthentication()` before `UseAuthorization()` before `Map*` endpoints, and both after `UseRouting()`.

Apply to routes: `app.MapPost("/orders", Handler).RequireAuthorization("OrdersWrite")`. Groups inherit: `app.MapGroup("/api").RequireAuthorization()` protects all child routes, so specific routes that need different policies call `.RequireAuthorization("OtherPolicy")` individually. Leave health checks anonymous by either omitting authorization metadata or explicitly calling `.AllowAnonymous()` when a fallback policy requires authenticated users globally. The fallback policy approach is safer for large APIs — everything is secured by default and public routes opt out explicitly, rather than relying on developers to remember to annotate every protected route.

---

#### Q6. (D) The team wants URL-based API versioning (`/api/v1/...`, `/api/v2/...`) using `MapGroup` without duplicating middleware and OpenAPI tags. What structure would you use, and what breaks if v1 and v2 share the same route parameter names but different DTO shapes?

**Concepts**
- Nested MapGroup for versioned route prefixes
- Shared extension methods for cross-cutting filters on all versions
- Separate DTO types per major version — v1 and v2 are distinct contracts
- MapGroup organizing routes not versioning contracts

**Answer**

Structure versioned groups by nesting `MapGroup` with version-specific prefixes and tagging each for OpenAPI, then extract shared cross-cutting concerns into extension methods:

```csharp
var v1 = app.MapGroup("/api/v1/orders").WithTags("Orders v1").RequireAuthorization();
v1.MapGet("/{orderId:guid}", GetOrderV1);
var v2 = app.MapGroup("/api/v2/orders").WithTags("Orders v2").RequireAuthorization();
v2.MapGet("/{orderId:guid}", GetOrderV2);
```

Extract shared filters: `static RouteGroupBuilder AddOrderDefaults(this RouteGroupBuilder g) => g.AddEndpointFilter<AuditFilter>()` — call on both groups so audit, validation, and rate limiting apply to all versions without duplication.

What breaks when v1 and v2 share route parameter names but different DTO shapes is that the binder resolves parameters by name and type — if both versions use the same handler function with `Guid orderId`, binding succeeds, but the response shape differs. The schema mismatch is invisible until clients consuming the v2 OpenAPI document try to deserialize a v1-shaped response. The fix is separate handler methods — `GetOrderV1` and `GetOrderV2` — and separate response DTO types — `OrderV1Response` and `OrderV2Response` — even when both delegate to the same domain service internally. `MapGroup` organizes routes and applies cross-cutting concerns; it does not version contracts — explicit types and separate OpenAPI tags are required for that.

---

#### Q7. (R) Review this `Program.cs` excerpt from a production service. What maintainability, testability, and DI problems will appear as the API grows?

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var conn = builder.Configuration.GetConnectionString("Default");
var cache = new MemoryCache(new MemoryCacheOptions());
var http = new HttpClient();

app.MapGet("/customers/{id}", async (string id) =>
{
    if (cache.TryGetValue(id, out Customer? c)) return Results.Ok(c);
    await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlServer(conn).Options);
    c = await db.Customers.FindAsync(id);
    cache.Set(id, c, TimeSpan.FromMinutes(10));
    return c is null ? Results.NotFound() : Results.Ok(c);
});

app.MapPost("/customers", async (CustomerDto dto) =>
{
    var resp = await http.PostAsJsonAsync("https://legacy.example/validate", dto);
    resp.EnsureSuccessStatusCode();
    await using var db = new AppDbContext(/* same inline options */);
    db.Customers.Add(new Customer { Name = dto.Name });
    await db.SaveChangesAsync();
    return Results.Created($"/customers/{dto.Name}", dto);
});

app.Run();
```

**Concepts**
- new AppDbContext() bypassing DI — no scoped lifetime, untestable
- new HttpClient() as captured singleton — socket exhaustion
- new MemoryCache outside container — no size limits, not injectable
- Results.Created with name not ID — wrong Location header
- God Program.cs — inline logic with no test seams

**Answer**

This `Program.cs` manually constructs `AppDbContext`, `HttpClient`, and `MemoryCache` outside the DI container, bypassing lifetime management entirely. `new AppDbContext(...)` inside each handler creates a new connection per request with no connection pooling benefit from EF's built-in pooling, no scoped lifetime alignment, and no ability to substitute a test double in integration tests. The `HttpClient` is captured as a closure-level variable — a singleton that prevents socket reuse and causes socket exhaustion under load. `MemoryCache` constructed with `new` has no size limits configured, no integration with the DI health check system, and cannot be injected or replaced in tests.

`Results.Created($"/customers/{dto.Name}", dto)` sets the Location header to a path containing the customer name rather than the persisted entity ID — the REST contract is wrong and breaks client navigation. The entire handler logic is inlined in `Program.cs` with no seams for unit testing.

The fix registers everything in `builder.Services`: `AddDbContext<AppDbContext>`, `AddMemoryCache`, `AddHttpClient("legacy", ...)`, then moves handlers to static classes (`CustomerEndpoints.Map(app)`) that receive DI-injected dependencies as handler parameters. The `Created` response uses the persisted entity ID: `TypedResults.Created($"/customers/{entity.Id}", dto)`.

---

#### Q8. (M) Minimal APIs resolve route-handler parameters from route values, body, services, and `HttpContext`. A handler injects `IOptions<FeatureFlags>` and `[FromServices] IAuditLogger` alongside `[FromBody] CreateOrderRequest`. What determines injection order and failure modes when a parameter cannot be bound?

**Concepts**
- Explicit bind source attributes taking priority over inference
- [FromBody] consumed once per request — duplicate body parameters invalid
- Unregistered service parameter — 500 at invoke time not 404
- [AsParameters] for deterministic property binding order

**Answer**

The parameter binding pipeline tries explicit bind source attributes first — `[FromRoute]`, `[FromQuery]`, `[FromBody]`, `[FromHeader]`, `[FromServices]` — which override inference. After explicit sources, the binder infers: route template matches bind by name, DI-registered types resolve from `RequestServices`, and special types like `HttpContext` and `CancellationToken` are resolved by type. `[FromBody]` consumes the request body once — declaring two body parameters is invalid and causes an error at startup or first request. `[FromServices]` forces DI resolution even when a parameter name matches a route value, which is useful when name collision would otherwise cause wrong-source binding.

Failure modes: a missing required route value returns 400; an incorrect JSON body shape returns 400; an unregistered `IAuditLogger` service causes an `InvalidOperationException` at invoke time producing a 500 — not a 404 — since the binder finds no registration in `RequestServices`. `IOptions<FeatureFlags>` requires `builder.Services.AddOptions<FeatureFlags>()` to be registered, otherwise the same runtime 500 applies. Optional parameters use nullable types or default values to avoid 400 on missing input. `[AsParameters]` aggregates bindable properties into one object with deterministic property binding order, which simplifies filters that validate the whole request at once.
