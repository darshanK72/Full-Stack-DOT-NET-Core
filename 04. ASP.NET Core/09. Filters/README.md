# Filters — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What are filters in ASP.NET Core MVC?](#q1-what-are-filters-in-aspnet-core-mvc)
2. [Q2. What is the MVC filter pipeline execution order?](#q2-what-is-the-mvc-filter-pipeline-execution-order)
3. [Q3. What is an authorization filter?](#q3-what-is-an-authorization-filter)
4. [Q4. What is an action filter?](#q4-what-is-an-action-filter)
5. [Q5. What is a resource filter?](#q5-what-is-a-resource-filter)
6. [Q6. What is a result filter?](#q6-what-is-a-result-filter)
7. [Q7. What is an exception filter?](#q7-what-is-an-exception-filter)
8. [Q8. What is the difference between middleware and filters?](#q8-what-is-the-difference-between-middleware-and-filters)
9. [Q9. When would you use a filter instead of middleware?](#q9-when-would-you-use-a-filter-instead-of-middleware)
10. [Q10. How do you register a global filter?](#q10-how-do-you-register-a-global-filter)
11. [Q11. How do you apply a filter to a single action or controller?](#q11-how-do-you-apply-a-filter-to-a-single-action-or-controller)
12. [Q12. What is `IAsyncActionFilter`, and how does it differ from `IActionFilter`?](#q12-what-is-iasyncactionfilter-and-how-does-it-differ-from-iactionfilter)
13. [Q13. How does `[Authorize]` relate to authorization filters?](#q13-how-does-authorize-relate-to-authorization-filters)
14. [Q14. What is the difference between authentication middleware and authorization filters?](#q14-what-is-the-difference-between-authentication-middleware-and-authorization-filters)
15. [Q15. Do Minimal APIs use MVC filters?](#q15-do-minimal-apis-use-mvc-filters)
16. [Q16. What are endpoint filters in Minimal APIs?](#q16-what-are-endpoint-filters-in-minimal-apis)
17. [Q17. How does DI work with filters?](#q17-how-does-di-work-with-filters)
18. [Q18. Can a filter short-circuit a request? How?](#q18-can-a-filter-short-circuit-a-request-how)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What are filters in ASP.NET Core MVC?

**Concepts**
- MVC filter pipeline vs middleware pipeline
- Five filter types: authorization, resource, action, exception, result
- MVC-specific context — ActionDescriptor, bound arguments
- Endpoint filters in Minimal APIs as the equivalent

**Answer**

Filters are components that run before and after MVC action execution, providing cross-cutting logic at the controller or action level. Since they operate only after routing selects an MVC controller action or Razor Page handler, they have access to MVC-specific context — `ActionDescriptor`, route values, bound arguments, and `IActionResult` — which middleware never has. ASP.NET Core supports five filter types across that pipeline. Register them globally in `AddControllers(options => ...)`, per controller via `[ServiceFilter]`, or per action with `[TypeFilter]`. Minimal APIs use endpoint filters rather than MVC filters for the same cross-cutting concerns.

---

## Q2. What is the MVC filter pipeline execution order?

**Concepts**
- Authorization filter — fail-fast 401/403
- Resource filter — wraps model binding and full action pipeline
- Action filter — wraps the action method only
- Exception filter — handles unhandled exceptions from earlier stages
- Result filter — wraps IActionResult execution
- IOrderedFilter.Order for within-stage sequencing

**Answer**

MVC filters run in a fixed order: authorization filters first so expensive work is skipped on access failure, then resource filters wrapping the remainder of the pipeline including model binding, then action filters surrounding the action method itself, then exception filters on any unhandled failure, and finally result filters wrapping `IActionResult` execution such as view rendering or JSON serialization. Within each filter type, global filters run before controller-level filters, which run before action-level filters. `IOrderedFilter.Order` provides fine-grained control when the default within-stage ordering is insufficient.

---

## Q3. What is an authorization filter?

**Concepts**
- IAuthorizationFilter / IAsyncAuthorizationFilter
- context.Result short-circuit returning 401/403
- Runs after HttpContext.User is populated by auth middleware
- Policy-based authorization via IAuthorizationService

**Answer**

An authorization filter implements `IAuthorizationFilter` or `IAsyncAuthorizationFilter` and runs before the action to determine whether the caller is allowed to proceed. The `[Authorize]` attribute is itself implemented as an authorization filter that evaluates policies via `IAuthorizationService`. Short-circuiting happens by setting `context.Result` to `UnauthorizedResult` or `ForbidResult`, which prevents the action and all subsequent filters from running. Since this filter runs after authentication middleware has already populated `HttpContext.User`, it can safely evaluate claims. Use `IAsyncAuthorizationFilter` whenever authorization logic requires async I/O, and never block with `.Result` on async calls inside synchronous filter methods.

---

## Q4. What is an action filter?

**Concepts**
- IActionFilter / IAsyncActionFilter interface
- OnActionExecuting vs OnActionExecuted callbacks
- context.Result short-circuit before the action runs
- Access to bound parameters not available in middleware

**Answer**

An action filter implements `IActionFilter` or `IAsyncActionFilter` and runs immediately before and after the action method executes. It is the right layer for action-scoped concerns such as audit logging, timing, mutating bound arguments, or validating models after binding — because unlike middleware, action filters see the actual bound parameters. In `OnActionExecuting` (or before `await next()` in the async variant), setting `context.Result` skips the action entirely. After `await next()`, `ActionExecutedContext` exposes the outcome, any exception, and elapsed time. Prefer `IAsyncActionFilter` whenever the filter performs I/O.

---

## Q5. What is a resource filter?

**Concepts**
- IResourceFilter / IAsyncResourceFilter interface
- Runs after authorization, before model binding
- Short-circuit bypasses model binding cost
- HttpContext.Items for per-request scoped data

**Answer**

A resource filter implements `IResourceFilter` or `IAsyncResourceFilter` and wraps execution of the entire remainder of the filter pipeline plus the action, running after authorization but before model binding on the way in. This position makes it earlier than action filters, so setting `context.Result` in `OnResourceExecuting` bypasses model binding cost entirely — which is useful for returning a cached `IActionResult` without running the action. For per-request reuse, store data in `HttpContext.Items` within the same request; for cross-request caching use `IMemoryCache` with TTL, since resource filters are not the right lifetime for that.

---

## Q6. What is a result filter?

**Concepts**
- IResultFilter / IAsyncResultFilter interface
- OnResultExecuting — can cancel or replace context.Result
- OnResultExecuted — inspect response status after execution
- Exception filters vs result filters — different pipeline stages

**Answer**

A result filter implements `IResultFilter` or `IAsyncResultFilter` and runs before and after `IActionResult` execution — view rendering, JSON serialization, or file delivery. `OnResultExecuting` fires before the result executes and can cancel or replace `context.Result`; `OnResultExecuted` fires after and lets you inspect the response status code and log completion. The key distinction from exception filters is stage: exception filters handle failures from the action and earlier MVC filter phases, while result filters wrap the result execution phase itself. For uniform response shaping across APIs, middleware or endpoint filters are often simpler than result filters.

---

## Q7. What is an exception filter?

**Concepts**
- IExceptionFilter / IAsyncExceptionFilter interface
- MVC-pipeline scope only — misses middleware and minimal API failures
- ExceptionHandled flag to prevent further propagation
- IExceptionHandler middleware as the preferred centralized alternative

**Answer**

An exception filter implements `IExceptionFilter` or `IAsyncExceptionFilter` and intercepts unhandled exceptions from the action or earlier MVC filter stages before the result is committed, so it can mark the exception handled and set `context.Result` to a clean error response. The key limitation is scope: exception filters only see exceptions from the MVC pipeline — failures in middleware, minimal API handlers, or infrastructure outside controller execution never reach them. For API services, the centralized `IExceptionHandler` middleware in .NET 8 is the preferred approach for uniform `ProblemDetails` responses, while exception filters remain useful for converting specific domain exceptions to particular MVC views.

---

## Q8. What is the difference between middleware and filters?

**Concepts**
- Middleware scope — every request, all endpoint types
- Filter scope — MVC/Razor Pages only, after action selection
- ActionDescriptor and bound arguments as filter-exclusive context
- Middleware order explicit in Program.cs vs MVC's fixed pipeline

**Answer**

Middleware runs for every request that reaches its position in the pipeline and applies to all endpoint types including minimal APIs, static files, and health checks. Filters run only for MVC or Razor Page requests after routing has selected a controller action, which means they have access to MVC-specific context — `ActionDescriptor`, route values, and bound parameters — that middleware never sees. Middleware order is explicit in `Program.cs`; filter order follows MVC's fixed pipeline stages with `IOrderedFilter.Order` for tie-breaking. Middleware is the right layer for authentication, forwarded headers, rate limiting, and global exception handling, while filters suit per-controller or per-action concerns like audit trails or action timing.

---

## Q9. When would you use a filter instead of middleware?

**Concepts**
- Bound parameter and action metadata access in filters
- Per-controller and per-action targeting
- Middleware for cross-endpoint concerns including minimal APIs
- API key validation as a middleware concern not a filter concern

**Answer**

I would use a filter when the logic genuinely needs MVC context — the action name, bound DTO parameters, or controller-level policies — or when the behavior should apply only to specific controllers or actions rather than every request. Middleware is the right choice for transport-wide concerns that must run before routing or must apply uniformly across minimal APIs and controllers alike. As a concrete example, action-specific audit logging that needs to inspect the bound DTO belongs in an action filter, while API key validation on all routes including minimal API health endpoints belongs in middleware, since filter short-circuit fires too late and misses non-MVC paths.

---

## Q10. How do you register a global filter?

**Concepts**
- options.Filters.Add<T>() vs AddService<T>() for DI resolution
- Scoped filter requiring AddScoped<T>() registration
- IOrderedFilter.Order for global filter sequencing

**Answer**

Add the filter type in `AddControllers` options using `options.Filters.Add<T>()` for simple filters, or `options.Filters.AddService<T>()` when the filter has scoped dependencies such as `DbContext`. The key distinction is that `AddService<T>()` requires the filter to be registered in the DI container so it is resolved with the correct lifetime per request — scoped filters for scoped services. Global filters apply to every controller action unless excluded, and `IOrderedFilter.Order` controls sequencing when multiple global filters run in the same pipeline stage.

```csharp
builder.Services.AddScoped<AuditActionFilter>();
builder.Services.AddControllers(options =>
    options.Filters.AddService<AuditActionFilter>());
```

---

## Q11. How do you apply a filter to a single action or controller?

**Concepts**
- ServiceFilter — DI resolution with proper lifetime
- TypeFilter — instantiation with constructor arguments
- Controller-level inheritance to all actions
- Multiple filters running in declaration order within a stage

**Answer**

Apply filter attributes directly on the controller class or action method. `[ServiceFilter(typeof(T))]` resolves the filter from DI, so constructor injection respects lifetimes; `[TypeFilter(typeof(T))]` instantiates the filter while allowing specified constructor arguments. Built-in attributes like `[Authorize]` work the same way. Controller-level filters inherit to all actions unless overridden — `[Authorize(Roles = "Admin")]` on one action narrows that action's authorization without affecting others. Multiple filters on one action run in declaration order within the same filter stage.

---

## Q12. What is `IAsyncActionFilter`, and how does it differ from `IActionFilter`?

**Concepts**
- Single OnActionExecutionAsync method vs two sync callbacks
- ActionExecutionDelegate next pattern with await
- Thread-pool starvation from blocking async in sync filters
- ActionExecutedContext.Exception and Result inspection after next()

**Answer**

`IAsyncActionFilter` defines a single `OnActionExecutionAsync` method that receives an `ActionExecutionDelegate` (`next`), so `await next()` surrounds the entire action execution — the result and any unhandled exception are on the returned `ActionExecutedContext`. `IActionFilter` splits the logic into two synchronous callbacks, `OnActionExecuting` and `OnActionExecuted`, with no direct async support. The critical reason this matters is that blocking async I/O inside synchronous filter methods via `.Result` or `.Wait()` causes thread-pool starvation under load, since ASP.NET Core's pipeline relies on non-blocking async throughout. Prefer `IAsyncActionFilter` whenever the filter awaits databases, HTTP calls, or file I/O.

---

## Q13. How does `[Authorize]` relate to authorization filters?

**Concepts**
- AuthorizeFilter as MVC's implementation of [Authorize]
- IAuthorizationService policy evaluation
- AllowAnonymous bypassing filter enforcement
- Same policy system backing RequireAuthorization() on minimal API routes

**Answer**

`[Authorize]` is backed by `AuthorizeFilter`, which is MVC's authorization filter implementation. When an action or controller carries `[Authorize]`, the filter calls `IAuthorizationService` with the configured policy against `HttpContext.User` populated by authentication middleware, and sets `context.Result` to 401 or 403 if the policy fails — before the action or model binding runs. `[AllowAnonymous]` on an action suppresses this filter for that endpoint. The same policy infrastructure powers both `[Authorize]` on controllers and `.RequireAuthorization()` on minimal API routes, which means policy definitions are shared while the integration point differs.

---

## Q14. What is the difference between authentication middleware and authorization filters?

**Concepts**
- Authentication middleware — identity establishment for all requests
- Authorization filters — permission decision for MVC actions only
- UseAuthentication() populating ClaimsPrincipal
- UseAuthorization() enforcing endpoint metadata including minimal routes

**Answer**

Authentication middleware (`UseAuthentication()`) runs for every request and validates credentials — JWT, cookies, API keys — to construct the `ClaimsPrincipal` on `HttpContext.User`. Authorization filters run later in the MVC pipeline and use that already-populated principal to decide whether the user may execute a specific action. The distinction is "who is this caller" versus "may this caller do this". `UseAuthorization()` middleware enforces endpoint metadata including `RequireAuthorization()` on minimal API routes and `[Authorize]` on controllers, while authorization filters are the MVC integration point for the same policy infrastructure — both evaluate policies via `IAuthorizationService`.

---

## Q15. Do Minimal APIs use MVC filters?

**Concepts**
- MVC filter pipeline does not apply to minimal API handlers
- IEndpointFilter as the minimal API equivalent
- RequireAuthorization() for authorization on minimal routes
- Consistent configuration required when mixing both

**Answer**

No — minimal API endpoints entirely bypass the MVC filter pipeline, so `IActionFilter`, `IAuthorizationFilter`, and the other MVC filter types never run for `MapGet`/`MapPost` handlers. The equivalent is `IEndpointFilter`, registered with `.AddEndpointFilter<T>()`, for validation, logging, and timing. For authorization, minimal API routes use `.RequireAuthorization()` rather than inheriting `[Authorize]` from controllers. Teams that mix controllers and minimal APIs must configure authorization at both layers — sprinkling `[Authorize]` on controllers does not protect separately registered minimal API routes.

---

## Q16. What are endpoint filters in Minimal APIs?

**Concepts**
- IEndpointFilter with InvokeAsync(HttpContext, EndpointFilterInvocationContext)
- EndpointFilterInvocationContext.Arguments for bound parameter inspection
- Short-circuit by returning IResult without calling next
- Group-level filters via MapGroup for shared validation

**Answer**

Endpoint filters implement `IEndpointFilter` and are registered with `.AddEndpointFilter<T>()` on a route or a `MapGroup`. The `InvokeAsync` signature provides access to `context.Arguments`, which holds the already-bound parameters for that specific route — so a filter can validate bound values before the handler runs, or return `Results.Problem()` to short-circuit without calling `await next(ctx)`. Applying a filter at the group level shares the same validation or timing logic across every endpoint in that `MapGroup`, which is the idiomatic way to avoid repeating filter registration on each route.

---

## Q17. How does DI work with filters?

**Concepts**
- ServiceFilter and AddService<T>() for DI-resolved filters
- Filter lifetime alignment with service lifetimes
- Captive dependency — scoped DbContext in a singleton filter
- IServiceScopeFactory or IDbContextFactory<T> for singleton filters needing a scope

**Answer**

Filters resolved through `[ServiceFilter]`, `options.Filters.AddService<T>()`, or `TypeFilterAttribute` participate in the DI container, so constructor injection works — but filter lifetimes must align with their dependencies. Injecting a scoped `DbContext` into a filter registered as singleton creates a captive dependency: the scoped instance is disposed after its first request scope while the singleton filter holds a reference forever, causing `ObjectDisposedException` or stale change trackers. Enable `ValidateScopes` in Development to catch this at startup. For singleton filters that need per-invocation database access, inject `IServiceScopeFactory` and create a scope per execution, or use `IDbContextFactory<T>` for a short-lived pooled context.

---

## Q18. Can a filter short-circuit a request? How?

**Concepts**
- Setting context.Result to skip subsequent filters and the action
- Resource filter short-circuit before model binding
- Endpoint filter returning IResult without calling next()
- Authorization filter 401/403 as the canonical short-circuit pattern

**Answer**

Yes — MVC authorization, resource, and action filters short-circuit by assigning `context.Result` to an `IActionResult` before calling `next()` (or in the `Executing` callback), which causes all subsequent filters and the action to be skipped. Endpoint filters short-circuit by returning an `IResult` directly without invoking `await next(ctx)`. Authorization filters use this pattern to return 401 or 403 before expensive model binding runs; resource filters use it to return a cached result and skip the entire action pipeline; action filters use it to return early on validation failure.

---

## Gotchas — Filters (Interview Traps)

---

#### Gotcha 1. Exception filters do not catch exceptions from result filters or from result execution

**Concepts**
- Exception filter scope: action, resource, and authorization filter exceptions
- Exceptions from result execution not caught by exception filters
- Middleware-level exception handling required for result-phase exceptions
- `OnException` vs `OnExceptionAsync` — only one should be overridden

**Answer**

Exception filters catch exceptions thrown during action execution and during the action pipeline — authorization filters, resource filters, and action filters. However, exceptions thrown during result execution (inside `IActionResult.ExecuteResultAsync`) are outside the filter pipeline and will not be caught by exception filters. These result-phase exceptions bubble up to middleware, making `UseExceptionHandler` or custom exception middleware the correct place to handle them. Teams that rely solely on exception filters for error handling may find that serialization failures, view render errors, or custom `IActionResult` execution errors bypass their exception filter entirely.

---

#### Gotcha 2. Filter execution order — authorization filters run before model binding

**Concepts**
- Filter pipeline order: authorization → resource → model binding → action → result/exception
- Authorization filter short-circuiting before model binding starts
- Action filters accessing model-bound parameters only in `OnActionExecuting`
- Resource filter wrapping model binding and action execution

**Answer**

The MVC filter pipeline runs in a strict order: authorization filters run first (before model binding), then resource filters, then model binding, then action filters (`OnActionExecuting`), then the action method, then action filters (`OnActionExecuted`), then result filters, and finally result execution. Placing logic that reads bound action parameters in an authorization filter is wrong — the parameters haven't been bound yet. Exception filters are invoked when an exception escapes any of these phases. Understanding this order is critical when deciding which filter type to use for a given cross-cutting concern.

---

#### Gotcha 3. Injecting services into filter attributes requires `ServiceFilter` or `TypeFilter` — not constructor injection

**Concepts**
- Attribute constructor arguments evaluated at compile time — no DI
- `[ServiceFilter(typeof(MyFilter))]` for DI-resolved filter
- `[TypeFilter(typeof(MyFilter), Arguments = new[] {...})]` for mixed DI and args
- `IFilterFactory` for fully custom filter factory behavior

**Answer**

Filter classes that implement `IActionFilter` can receive services through their constructor if they are instantiated by DI. However, attribute syntax like `[MyActionFilter(someValue)]` passes constructor arguments at compile time using constants, not DI. To inject runtime services, mark the attribute with `[ServiceFilter(typeof(MyFilter))]` — this resolves `MyFilter` from the DI container per request. `[TypeFilter(typeof(MyFilter), Arguments = new object[] { "value" })]` allows mixing compile-time arguments with DI-resolved services. Using `new MyFilter(service)` directly in an attribute attribute is impossible for runtime services.

---

#### Gotcha 4. `IAsyncActionFilter` and `IActionFilter` — implementing both causes the async version to be ignored in some scenarios

**Concepts**
- MVC executing `IAsyncActionFilter` preferentially when both are implemented
- `IActionFilter` methods never called when async interface is also present
- Implementing only one interface per filter class
- `ActionFilterAttribute` base class implementing both interfaces

**Answer**

When a filter class implements both `IAsyncActionFilter` and `IActionFilter`, ASP.NET Core MVC preferentially uses the async implementation and the sync methods (`OnActionExecuting` / `OnActionExecuted`) are never called. This is intentional to avoid double-execution but creates confusion when developers override both expecting both to run. `ActionFilterAttribute` implements both interfaces with no-op defaults — override only `OnActionExecutingAsync` and `OnActionExecutedAsync` for async behavior, or only `OnActionExecuting` and `OnActionExecuted` for sync. Mixing partial overrides of both can lead to subtle bugs where only the async path executes.

---

#### Gotcha 5. Short-circuiting in resource filters skips model binding AND all subsequent action/result filters

**Concepts**
- Resource filter short-circuit returning `context.Result` before model binding
- All action filters and the action method skipped when resource filter short-circuits
- Cache-aside pattern using resource filter to bypass action execution
- Result filters still execute for responses from resource filter short-circuits

**Answer**

When a resource filter assigns `context.Result` in `OnResourceExecuting`, model binding is skipped, all action filters are skipped, and the action method is never invoked. Only result filters (in the result execution phase) run after a resource filter short-circuit, because result filters wrap the result phase rather than the action phase. This makes resource filters appropriate for cache-hit returns — the cached result is set as `context.Result` and all expensive model binding and action work is bypassed. Exception filters are not invoked for resource filter short-circuits because no exception occurred, only an early result assignment.

---

#### Gotcha 6. Global filters apply to all controllers and actions — order and scope must be managed explicitly

**Concepts**
- `MvcOptions.Filters.Add()` registering global filters
- Global filters running for every action including health check and static content controllers
- `Order` property controlling relative execution among filters of the same type
- `[AllowAnonymous]` overriding authorization filter globally but not custom filters

**Answer**

Global filters registered in `MvcOptions.Filters.Add()` run for every controller and action method in the application. An audit logging filter registered globally produces a log entry for every request, including internal health check endpoints and scaffolding controllers. The `Order` property on filters controls execution sequence when multiple filters of the same type apply — lower numbers run first for before-logic and last for after-logic (innermost in the pipeline sense). Custom global filters must explicitly check for exclusion attributes or conditions when they should not apply to specific controllers, since there is no built-in "exclude from global filter" attribute mechanism equivalent to `[AllowAnonymous]` for auth.

---

#### Gotcha 7. Async result filters must `await` both before-logic and `await next()` — missing either half breaks the pipeline

**Concepts**
- `IAsyncResultFilter.OnResultExecutionAsync` wrapping result execution
- `await next()` invoking result execution (the actual IActionResult execution)
- Before-`next` code running before response is written; after-`next` code running post-response
- Response already started after `next()` — cannot modify status code or headers

**Answer**

In `IAsyncResultFilter.OnResultExecutionAsync(context, next)`, the `await next()` call is what actually executes the `IActionResult` and writes the response. Before-`next` code runs before the response is written; after-`next` code runs after response writing has started. Forgetting to `await next()` prevents the action result from executing and returns an empty response. After `next()` returns, `context.Response.HasStarted` is typically `true`, so any attempt to modify response headers or status codes after the call throws. Response manipulation must happen before `await next()`.

---

#### Gotcha 8. Exception filter vs middleware — exception filter catches only MVC pipeline exceptions

**Concepts**
- Exception filter scope limited to MVC action and filter pipeline
- Exceptions from middleware outside MVC not reaching exception filters
- `UseExceptionHandler` middleware catching all unhandled exceptions from the entire pipeline
- Complementary roles: exception filter for action-specific mapping, middleware for global handling

**Answer**

Exception filters catch exceptions that escape MVC action execution and MVC filters — they are scoped to the controller action pipeline. Exceptions thrown in middleware before or after the MVC middleware, in startup code, or during static file serving are outside the exception filter's scope and are not caught. `UseExceptionHandler` or `UseDeveloperExceptionPage` middleware is positioned outside the entire MVC stack and catches all unhandled exceptions from any middleware. In practice, both are needed: exception filters for per-controller or per-action mapping of domain exceptions to specific HTTP responses, and exception handling middleware as the global safety net for everything else.

---

#### Gotcha 9. `Order` property — lower order runs first for before-logic but last for after-logic

**Concepts**
- Filter pipeline as nested delegates — order property controls nesting depth
- `Order = -1` running before `Order = 0` in the entry phase
- `Order = -1` running after `Order = 0` in the exit/exception phase
- Outermost filter seeing both request entry and response exit

**Answer**

The `Order` property on filters follows the same nesting semantics as middleware: a filter with `Order = -1` wraps a filter with `Order = 0`, meaning it runs first during the request phase (in `OnActionExecuting`) and last during the response phase (in `OnActionExecuted`). This counterintuitive behavior surprises developers who expect lower order to always mean "runs first." For logging filters, `Order = -1` is typically correct because it wraps the entire action execution, giving the filter both the entry timing and the complete execution context including any set `Result`. Filters with the same `Order` run in registration order.

---

#### Gotcha 10. Endpoint filters (Minimal API) vs MVC action filters — different pipeline shape, no exception filter equivalent

**Concepts**
- `IEndpointFilter` wrapping Minimal API endpoint invocation
- No separate authorization/resource/exception filter types in Minimal API
- Short-circuit by returning an `IResult` from `InvokeAsync`
- `RouteGroupBuilder.AddEndpointFilter()` applying filters to a group

**Answer**

Minimal API uses `IEndpointFilter` rather than the MVC filter interfaces. An endpoint filter's `InvokeAsync` wraps the entire endpoint handler and can run before-logic, call `await next(context)` to invoke the handler, and run after-logic — similar to middleware but scoped to a single endpoint or group. There is no separate exception filter concept for Minimal API endpoints; exceptions flow to the middleware pipeline and are caught by `UseExceptionHandler`. Short-circuit by returning an `IResult` directly without calling `next`. Apply filters per-endpoint with `.AddEndpointFilter<T>()` or per-group with `RouteGroupBuilder.AddEndpointFilter<T>()`.

---

## Scenario-Based Questions (Karat Format)

#### Q1. (M) A teammate says "we'll put auth in an action filter instead of middleware." Walk through the MVC filter execution order — authorization filter, resource filter, action filter, exception filter, result filter — and explain which filter type owns authentication vs authorization vs action-specific validation.

**Concepts**
- Authentication in middleware, not any filter type
- Authorization filter — fail-fast access decision before model binding
- Action filter — action-specific validation with bound parameters
- Result filter wrapping IActionResult execution
- Filter short-circuit scope vs middleware short-circuit scope

**Answer**

MVC runs filters in a fixed pipeline around the action: authorization filters run first and short-circuit with 401/403 before model binding and the action incur cost, then resource filters wrap the remainder including model binding, then action filters run immediately around the action method, then exception filters handle unhandled exceptions from earlier stages, and finally result filters surround `IActionResult` execution. Authentication does not belong in any of these stages — it belongs in `UseAuthentication()` middleware, which runs earlier and populates `HttpContext.User` before routing even selects the action. Authorization filters then evaluate policies against that already-populated principal via `IAuthorizationService`. Action-specific validation — checking bound model state or business preconditions on the bound DTO — belongs in action filters, since they receive the bound parameters directly.

Putting authentication in an action filter runs it too late for non-MVC endpoints, bypasses it entirely for requests that never reach routing, and duplicates what the authentication middleware already standardizes across the entire pipeline.

---

#### Q2. (R) Review this custom authorization filter. What breaks at runtime or under load, and what would you change?

```csharp
public class ApiKeyAuthFilter : IAuthorizationFilter
{
    private readonly IUserRepository _users;

    public ApiKeyAuthFilter(IUserRepository users) => _users = users;

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var key = context.HttpContext.Request.Headers["X-Api-Key"];
        var user = _users.FindByApiKey(key).Result;
        if (user is null)
            context.Result = new UnauthorizedResult();
    }
}
```

*(Assume the filter is registered globally via `AddControllers(options => options.Filters.Add<ApiKeyAuthFilter>())`.)*

**Concepts**
- Sync-over-async via .Result causing thread-pool starvation
- IAsyncAuthorizationFilter as the correct interface
- StringValues header validation before database lookup
- UnauthorizedObjectResult with ProblemDetails for consistent error shape

**Answer**

The filter blocks a thread-pool thread by calling `.Result` on `FindByApiKey()` inside the synchronous `OnAuthorization` callback. Since every matched controller action runs through this global authorization filter, blocking I/O here multiplies across the entire MVC surface and causes thread-pool starvation under concurrent load. The header is also read and passed directly to the repository without checking whether it is empty or malformed — an absent `X-Api-Key` header passes a `StringValues.Empty` value to the database query. The response is a bare `UnauthorizedResult` with no body, which is inconsistent with any `ProblemDetails` contract the API maintains.

The fix starts with switching to `IAsyncAuthorizationFilter` and awaiting `FindByApiKeyAsync` with the request cancellation token. Then validate the header with `TryGetValue` before querying, and return `UnauthorizedObjectResult` with a `ProblemDetails` body rather than the empty result.

```csharp
public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
{
    if (!context.HttpContext.Request.Headers.TryGetValue("X-Api-Key", out var key)
        || !await _users.IsValidKeyAsync(key.ToString(), context.HttpContext.RequestAborted))
    {
        context.Result = new UnauthorizedObjectResult(new ProblemDetails
        {
            Status = 401, Title = "Unauthorized", Detail = "Invalid or missing API key."
        });
    }
}
```

---

#### Q3. (P) Implement cross-cutting request timing and audit logging around controller actions using `IAsyncActionFilter`. What runs in `OnActionExecutionAsync` before vs after `await next()`, and what can you still change at each stage?

**Concepts**
- ActionExecutingContext before next() — short-circuit or read bound arguments
- ActionExecutedContext after next() — outcome, exceptions, elapsed time
- IAsyncActionFilter for async I/O in filter logic
- ServiceFilter or global AddService<T>() registration

**Answer**

Register an `IAsyncActionFilter` and put setup and timing before `await next()`, then inspection and logging after. Before `await next()`, the `ActionExecutingContext` provides route values, `HttpContext.User`, and the bound arguments — at this point setting `context.Result` completely skips the action, which is useful for returning a validation failure 400 before the handler runs. After `await next()` returns, `ActionExecutedContext` carries the result type, any unhandled exception, and elapsed time for logging. Since `await next()` has already invoked the action, mutating the result at this stage is possible but unusual. Register via `[ServiceFilter(typeof(AuditActionFilter))]` for per-controller use, or globally with `options.Filters.AddService<AuditActionFilter>()` when scoped `DbContext` is needed.

```csharp
public async Task OnActionExecutionAsync(ActionExecutingContext ctx, ActionExecutionDelegate next)
{
    var sw = Stopwatch.StartNew();
    var executed = await next();
    sw.Stop();
    _logger.LogInformation("Action {Action} took {Ms}ms, status {Result}",
        ctx.ActionDescriptor.DisplayName, sw.ElapsedMilliseconds,
        executed.Result?.GetType().Name ?? "exception");
}
```

---

#### Q4. (D) Your team needs to reject requests without a valid API key before routing reaches expensive database middleware. Another developer wants an `IActionFilter` on every controller. Compare filter short-circuit (`context.Result = …`) vs middleware short-circuit (no `_next`). When is each the right seam?

**Concepts**
- Middleware short-circuit before routing and model binding
- Filter short-circuit after MVC action selection — expensive upstream middleware already ran
- Middleware applies to all endpoint types including minimal APIs
- Per-controller policies as the filter's legitimate advantage

**Answer**

Middleware short-circuit rejects requests before routing runs — saving the cost of endpoint selection, model binding, and any upstream middleware that runs after the gate. Filter short-circuit only fires after MVC has already selected a controller action, which means every expensive middleware between `UseRouting()` and the action already ran. The proposal to put an `IActionFilter` on every controller is the wrong seam for this requirement: it fires too late, misses minimal API routes and health endpoints, and duplicates authorization logic across controllers rather than centralizing it.

Middleware is the right layer when the rejection criterion is global — API key absence affects the entire HTTP surface, not one controller. Place the API key middleware before the expensive database middleware in the pipeline so rejections happen at the earliest possible point. Filters are the right layer when the logic needs MVC context: action name, bound parameters, or per-controller authorization policies. Both approaches can set a result and terminate the pipeline, but the position in the pipeline determines what has already executed when that result is written.

---

#### Q5. (M) `[Authorize(Roles = "Admin")]` is implemented as an authorization filter. How does that differ from calling `app.UseAuthentication()` / `UseAuthorization()` middleware, and what happens if authorization middleware runs but no authorization filter is reached (e.g., minimal API endpoint)?

**Concepts**
- UseAuthentication() establishing ClaimsPrincipal for all requests
- UseAuthorization() enforcing endpoint metadata after routing
- AuthorizeFilter as MVC-only integration point
- Minimal API routes requiring RequireAuthorization() not [Authorize]

**Answer**

`UseAuthentication()` runs for every request and calls authentication handlers to set the `ClaimsPrincipal` on `HttpContext.User`. `UseAuthorization()` middleware evaluates `[Authorize]`, `[AllowAnonymous]`, and `RequireAuthorization()` metadata attached to the matched endpoint — since it runs after routing, it can see endpoint-specific authorization requirements including those on minimal API routes. The MVC `[Authorize]` attribute creates an `AuthorizeFilter` as one integration point for this same policy system, but this filter runs only for controller actions.

A minimal API route registered with `.RequireAuthorization("Admin")` never reaches MVC exception or authorization filters — only the authorization middleware evaluates it. If a minimal API endpoint has no `.RequireAuthorization()` call, it is anonymous even if controllers are fully protected by `[Authorize]`. Teams mixing both surfaces must configure authorization at the middleware/metadata level — authorization filters alone cover only the MVC surface.

---

#### Q6. (R) Review this resource filter intended to cache expensive lookup data for the duration of one request. What is wrong?

```csharp
public class CatalogCacheResourceFilter : IResourceFilter
{
    private static readonly Dictionary<string, object> _cache = new();

    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        var key = context.RouteData.Values["categoryId"]?.ToString();
        if (key != null && _cache.TryGetValue(key, out var cached))
            context.HttpContext.Items["Catalog"] = cached;
    }

    public void OnResourceExecuted(ResourceExecutedContext context)
    {
        var key = context.RouteData.Values["categoryId"]?.ToString();
        if (key != null && !context.HttpContext.Items.ContainsKey("Catalog"))
            _cache[key] = context.HttpContext.Items["Catalog"]!;
    }
}
```

**Concepts**
- Static Dictionary as process-wide shared state, not per-request
- Dictionary<> thread-safety under concurrent writes
- Inverted ContainsKey logic storing null on lookup failure
- IMemoryCache with TTL for legitimate cross-request caching

**Answer**

The `static` dictionary shares catalog data across all requests and all users for the entire process lifetime, not just for one request as a resource filter normally intends. This causes three distinct problems. First, the cache never expires — entries accumulate indefinitely and stale pricing or availability data from one request persist for all subsequent ones. Second, `Dictionary<string, object>` is not thread-safe: concurrent reads and writes from parallel requests cause race conditions and potential data corruption. Third, the `OnResourceExecuted` condition is inverted — it stores when the key is absent in `HttpContext.Items`, which means it stores null when the lookup failed, poisoning the cache for subsequent requests on the same category.

For per-request reuse, store data in `HttpContext.Items` within the same request with no static field. For cross-request caching, inject `IMemoryCache` with explicit TTL and per-tenant or per-category keys. The resource filter short-circuit pattern works correctly when you set `context.Result` in `OnResourceExecuting` from a populated `IMemoryCache` entry — that is the legitimate use of a resource filter, not a static mutable dictionary.

---

#### Q7. (P) A global action filter is registered in `Program.cs` and constructor-injects `AppDbContext`. The app starts in Development but throws `Cannot consume scoped service from singleton` in Production with scope validation enabled. Explain filter DI lifetime, how global filters are resolved, and the correct fix.

```csharp
builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditActionFilter>();
});
builder.Services.AddDbContext<AppDbContext>();
```

**Concepts**
- options.Filters.Add<T>() vs AddService<T>() lifetime semantics
- ValidateScopes detecting captive dependency at startup
- Scoped filter requiring AddScoped<T>() registration
- IDbContextFactory<T> for filters that must remain singleton

**Answer**

`options.Filters.Add<AuditActionFilter>()` without a corresponding `AddScoped<AuditActionFilter>()` registration causes the framework to construct the filter without a per-request lifetime, so it effectively behaves as a singleton. When `AppDbContext` is registered as scoped, injecting it into this singleton-like filter creates a captive dependency. Development typically does not catch this immediately because `ValidateScopes` defaults to false in development templates, but enabling it — or deploying to Production where it is on by default in `WebApplication` — surfaces the `InvalidOperationException` at first use.

The fix has two parts: register `builder.Services.AddScoped<AuditActionFilter>()` so the filter gets a new instance per request scope, and switch to `options.Filters.AddService<AuditActionFilter>()` so the pipeline resolves it from DI per request. An alternative for filters that must remain singleton is to inject `IServiceScopeFactory` and create a scope inside `OnActionExecutionAsync`, or use `IDbContextFactory<AppDbContext>` which provides pooled short-lived contexts without a captive dependency.

```csharp
builder.Services.AddScoped<AuditActionFilter>();
builder.Services.AddControllers(o => o.Filters.AddService<AuditActionFilter>());
```

---

#### Q8. (D) Unhandled exceptions in a controller can be caught by an exception filter, `IExceptionHandler` middleware, or `UseExceptionHandler`. Compare scope (MVC-only vs entire pipeline), ordering, and when you would keep an exception filter vs centralizing everything in middleware.

**Concepts**
- Exception filter scope — MVC filter pipeline only
- IExceptionHandler chain — DI-registered, handles entire pipeline
- UseExceptionHandler as the outer safety net
- Minimal API exceptions bypassing MVC exception filters entirely

**Answer**

Exception filters run only for exceptions thrown from controller actions and earlier MVC filter stages — they never see exceptions from middleware before routing, from minimal API handlers, or from infrastructure code outside the MVC pipeline. `IExceptionHandler` implementations registered via `AddExceptionHandler<T>()` are invoked by `UseExceptionHandler` middleware and handle exceptions from anywhere in the downstream pipeline, including both MVC and minimal API failures.

The ordering is: `UseExceptionHandler` middleware must be registered early — typically first after `Build()` — to wrap the entire downstream pipeline. When an exception reaches it, the middleware invokes registered `IExceptionHandler` implementations in registration order until one returns `true` from `TryHandleAsync`. MVC exception filters run inside this pipeline for controller failures; if they mark the exception as handled, it never reaches the outer middleware. Only if none handle it does the default behavior apply.

Keep exception filters when converting specific MVC action failures to particular HTML views, or when third-party MVC extensions require filter-level handling. For API teams standardizing on `ProblemDetails`, centralizing everything in `IExceptionHandler` produces consistent error shapes across controllers, minimal APIs, and middleware failures — exception filters become legacy MVC-only paths.
