# Action Filters in MVC — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What are action filters in ASP.NET Core MVC?](#q1-what-are-action-filters-in-aspnet-core-mvc)
2. [Q2. What is the MVC filter pipeline?](#q2-what-is-the-mvc-filter-pipeline)
3. [Q3. What are the filter stages (authorization, resource, action, exception, result)?](#q3-what-are-the-filter-stages-authorization-resource-action-exception-result)
4. [Q4. What is the difference between action filters and middleware?](#q4-what-is-the-difference-between-action-filters-and-middleware)
5. [Q5. What are `IActionFilter` and `IAsyncActionFilter`?](#q5-what-are-iactionfilter-and-iasyncactionfilter)
6. [Q6. What is `IAuthorizationFilter`?](#q6-what-is-iauthorizationfilter)
7. [Q7. What is `IExceptionFilter`?](#q7-what-is-iexceptionfilter)
8. [Q8. What is `IResultFilter`?](#q8-what-is-iresultfilter)
9. [Q9. What is `IResourceFilter`?](#q9-what-is-iresourcefilter)
10. [Q10. What is the difference between global, controller-level, and action-level filters?](#q10-what-is-the-difference-between-global-controller-level-and-action-level-filters)
11. [Q11. How does filter order (`IOrderedFilter`) work?](#q11-how-does-filter-order-iorderedfilter-work)
12. [Q12. What is the difference between `[ServiceFilter]` and `[TypeFilter]`?](#q12-what-is-the-difference-between-servicefilter-and-typefilter)
13. [Q13. How do you register a global filter in `AddControllersWithViews`?](#q13-how-do-you-register-a-global-filter-in-addcontrollerswithviews)
14. [Q14. What is `[ValidateAntiForgeryToken]` as a filter?](#q14-what-is-validateantiforgerytoken-as-a-filter)
15. [Q15. What is `[AutoValidateAntiforgeryToken]`?](#q15-what-is-autovalidateantiforgerytoken)
16. [Q16. What is `[IgnoreAntiforgeryToken]`?](#q16-what-is-ignoreantiforgerytoken)
17. [Q17. What is `[Authorize]` as an authorization filter?](#q17-what-is-authorize-as-an-authorization-filter)
18. [Q18. When should you use a filter instead of middleware for MVC-specific concerns?](#q18-when-should-you-use-a-filter-instead-of-middleware-for-mvc-specific-concerns)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What are action filters in ASP.NET Core MVC?

**Concepts**
- Cross-cutting concerns implemented without duplicating code in every action
- Filter interfaces — `IActionFilter`, `IAsyncActionFilter`, `IAuthorizationFilter`, etc.
- Rich MVC context objects with route data, action arguments, and short-circuit capability
- Built-in filter attributes — `[Authorize]`, `[ValidateAntiForgeryToken]`, `[ResponseCache]`

**Answer**

Action filters are components that run before and after MVC action methods — and around other pipeline stages — to implement cross-cutting concerns such as logging, validation, caching, and authorization without duplicating code in every action. They implement interfaces like `IActionFilter`, `IAsyncActionFilter`, `IAuthorizationFilter`, `IResourceFilter`, `IResultFilter`, and `IExceptionFilter`. Filters receive rich MVC context objects (`ActionExecutingContext`, `ResultExecutingContext`) with route data, action arguments, and the ability to short-circuit by setting `context.Result`. They are registered globally, at the controller level, or on individual actions, and built-in attributes such as `[Authorize]` and `[ValidateAntiForgeryToken]` are implemented as filters.

---

## Q2. What is the MVC filter pipeline?

**Concepts**
- Ordered filter stage sequence wrapping endpoint execution
- Stage order — authorization, resource, action, exception, result
- Within-stage ordering — global then controller then action, then by `IOrderedFilter.Order`
- Short-circuit by setting `context.Result` skipping remaining stages
- Pipeline distinct from middleware — runs only for MVC controller actions

**Answer**

The MVC filter pipeline is the ordered sequence of filter stages that wrap endpoint execution after routing selects an MVC action, running before and after the action method and result execute. Stages run in order: authorization → resource → action → (action method) → exception → result. Within each stage, filters run by scope (global → controller → action) and then by `IOrderedFilter.Order`. A filter can set `context.Result` to short-circuit remaining stages — for example, returning 401 before model binding completes. The pipeline is distinct from middleware — filters only run for MVC controller actions and Razor Pages, not for Minimal API endpoints.

---

## Q3. What are the filter stages (authorization, resource, action, exception, result)?

**Concepts**
- Authorization filters — run first, enforce auth before any other stage
- Resource filters — wrap the full pipeline, useful before model binding
- Action filters — run immediately before and after the action method
- Exception filters — handle exceptions from actions or earlier filter stages
- Result filters — run before and after `IActionResult` executes

**Answer**

MVC defines five filter stages that execute in a fixed order around the action method and its result. Authorization filters (`IAuthorizationFilter`) run first — they enforce authentication and antiforgery validation before resource and action stages. Resource filters (`IResourceFilter`) wrap the rest of the pipeline, useful for short-circuiting before model binding or caching per request via `HttpContext.Items`. Action filters (`IActionFilter` / `IAsyncActionFilter`) run immediately before and after the action method — logging, model-state checks, and idempotency guards belong here. Exception filters (`IExceptionFilter`) handle exceptions thrown from actions or earlier filters when not caught elsewhere. Result filters (`IResultFilter`) run before and after the `IActionResult` executes — view rendering, output caching, and response header manipulation belong here.

---

## Q4. What is the difference between action filters and middleware?

**Concepts**
- Middleware running for every request regardless of endpoint type
- Action filters running only for matched MVC/Razor Page endpoints after routing
- Filter access to action descriptors, bound models, and `ActionArguments`
- Placement of cross-cutting concerns — per-action context vs all-request context

**Answer**

Middleware runs for every request that reaches it in the pipeline, including static files and Minimal APIs, while action filters run only for matched MVC or Razor Page endpoints after routing and endpoint selection. Middleware executes before routing and has no access to action descriptors, bound models, or `ActionArguments` — filters have full MVC context. Short-circuiting in middleware prevents later middleware from running; filter short-circuiting skips remaining filter stages and the action but cannot undo middleware that already executed on the way in. Cross-cutting concerns affecting all requests belong in middleware; per-action audit with route values or model data belongs in action filters.

---

## Q5. What are `IActionFilter` and `IAsyncActionFilter`?

**Concepts**
- `IActionFilter` — synchronous `OnActionExecuting` and `OnActionExecuted`
- `IAsyncActionFilter` — single `OnActionExecutionAsync` for async I/O work
- `OnActionExecuting` — set `context.Result` to skip the action
- Thread-pool starvation from sync-over-async in `IActionFilter`

**Answer**

`IActionFilter` defines synchronous `OnActionExecuting` and `OnActionExecuted` methods; `IAsyncActionFilter` defines `OnActionExecutionAsync` for async work before and after the action without blocking thread-pool threads. `OnActionExecuting` runs before the action — setting `context.Result` skips the action entirely, for example returning `BadRequestObjectResult` when model state is invalid. `OnActionExecuted` runs after the action — you can inspect or modify `context.Result` and log exceptions from `context.Exception`. I prefer `IAsyncActionFilter` when the filter performs any I/O, since blocking with `.GetAwaiter().GetResult()` in a synchronous filter causes thread-pool starvation under load.

---

## Q6. What is `IAuthorizationFilter`?

**Concepts**
- `IAuthorizationFilter` running in the authorization stage before resource and action filters
- `OnAuthorization` receiving `AuthorizationFilterContext` with the user and endpoint metadata
- `[ValidateAntiForgeryToken]` implemented as an authorization filter — runs before model binding
- Multiple authorization filters all running in order; any one can short-circuit

**Answer**

`IAuthorizationFilter` runs in the authorization stage before resource and action filters, determining whether the caller may execute the action — the built-in `[Authorize]` attribute implements this interface. `OnAuthorization` receives `AuthorizationFilterContext` with `HttpContext.User`, endpoint metadata, and the ability to set `context.Result` to `ChallengeResult` or `ForbidResult`. `[ValidateAntiForgeryToken]` is implemented as an authorization filter, not an action filter — invalid tokens reject before model binding, which is why the action method never runs and the model is never bound. Multiple authorization filters all run in order; any one can short-circuit with an unauthorized result.

---

## Q7. What is `IExceptionFilter`?

**Concepts**
- `IExceptionFilter` handling exceptions from actions or earlier filter stages
- `ExceptionContext.ExceptionHandled` flag and `context.Result` assignment for custom responses
- Setting `ExceptionHandled = true` then rethrowing — double handling with global middleware
- `IExceptionHandler` middleware as a centralized alternative for API-heavy apps

**Answer**

`IExceptionFilter` handles exceptions thrown during action execution or earlier filter stages, allowing MVC-specific error responses such as custom views or JSON `ProblemDetails` before the exception propagates to middleware. `OnException` receives `ExceptionContext` with the thrown exception and can set `context.ExceptionHandled = true` and assign `context.Result`. Setting `ExceptionHandled = true` and then rethrowing causes double handling when global exception middleware also processes the same fault — pick one strategy. For API-heavy apps, centralized `IExceptionHandler` middleware often replaces exception filters; filters remain useful for returning area-specific HTML error views. Never expose raw `Exception.Message` in production responses.

---

## Q8. What is `IResultFilter`?

**Concepts**
- `IResultFilter` running before and after `IActionResult` execution
- `OnResultExecuting` — replace `context.Result` to short-circuit view rendering
- Writing to the response without replacing `context.Result` does not prevent the original result
- Result filters not running for static files or Minimal API `IResult` handlers

**Answer**

`IResultFilter` runs before and after the `IActionResult` executes — for example before a `ViewResult` renders Razor and after the view engine produces output. `OnResultExecuting` can replace `context.Result` entirely to skip view rendering, such as returning a cached `ContentResult` instead. Writing directly to the response stream in `OnResultExecuting` without replacing `context.Result` does not prevent the original result from executing — assigning a new result is the correct short-circuit mechanism. `OnResultExecuted` runs after the result executes, useful for response compression hooks or logging rendered status codes. Result filters do not run for static file responses or Minimal API `IResult` handlers that bypass the MVC result pipeline.

---

## Q9. What is `IResourceFilter`?

**Concepts**
- `IResourceFilter` wrapping all later filter stages including model binding
- `OnResourceExecuting` for pre-binding cache checks via `HttpContext.Items`
- `HttpContext.Items` as the correct per-request cache — not static fields
- Earliest filter stage with full MVC endpoint context

**Answer**

`IResourceFilter` wraps execution of all later filter stages and the action itself, running immediately after authorization filters and before model binding and action filters on the way in. `OnResourceExecuting` can set `context.Result` to short-circuit before model binding — useful for request-level caching checks via `HttpContext.Items`. `OnResourceExecuted` runs after the action and result complete on the way out. `HttpContext.Items` is the correct per-request cache for resource filters — static fields leak data across requests and tenants. Resource filters are less commonly authored than action filters but are the earliest stage that still has full MVC endpoint context.

---

## Q10. What is the difference between global, controller-level, and action-level filters?

**Concepts**
- Global filters applying to every MVC action via `MvcOptions.Filters`
- Controller-level filters via class-level attributes
- Action-level filters on individual action methods
- Execution order — global then controller then action, then by `IOrderedFilter.Order`

**Answer**

Filters apply at three scopes — global filters affect every MVC action, controller-level filters affect all actions on that controller, and action-level filters affect only the decorated action — with all three potentially running on a single request. Global filters register in `AddControllersWithViews(options => options.Filters.Add...)` or via `AddService<T>()`. Controller-level filters use class attributes such as `[ServiceFilter(typeof(AuditFilter))]` on the controller class. Action-level attributes on a single method add filters only for that action. When multiple scopes apply, filters within the same stage run in order: global → controller → action, then by `IOrderedFilter.Order` within each scope.

---

## Q11. How does filter order (`IOrderedFilter`) work?

**Concepts**
- `IOrderedFilter.Order` — lower value runs first on executing leg
- Executing leg sorts ascending; executed leg reverses (onion model)
- Default `Order = 0` when not specified
- Scope applied before `Order` within the same filter type and stage

**Answer**

Filters implementing `IOrderedFilter` expose an `Order` property — lower values run first on the executing (before) leg and last on the executed (after) leg, mirroring middleware's onion model. Default `Order` is `0` when not specified — explicit negative values (e.g., `-1000`) run before default-ordered filters on the executing side. On the executing leg, sort ascending by `Order` — idempotency guards should use large negative values to run before validation filters. On the executed leg, order reverses — validation's `OnActionExecuted` runs before idempotency's post-action logic. Scope (global → controller → action) is applied before `Order` within the same filter type and stage.

---

## Q12. What is the difference between `[ServiceFilter]` and `[TypeFilter]`?

**Concepts**
- `[ServiceFilter]` — entire filter resolved from DI; all ctor params from container
- `[TypeFilter]` — factory construction with `Arguments` supplying primitive ctor params alongside DI
- Both requiring filter dependencies registered in the service collection
- `[TypeFilter]` enabling per-controller parameter variation without separate registered types

**Answer**

`[ServiceFilter(typeof(MyFilter))]` resolves the filter entirely from DI using its registered constructor dependencies, while `[TypeFilter(typeof(MyFilter), Arguments = new object[] { 60 })]` constructs the filter via a factory, supplying extra constructor arguments alongside DI-resolved services. `ServiceFilter` requires `builder.Services.AddScoped<MyFilter>()` — all constructor parameters come from DI. `TypeFilter` supports passing primitive or configuration values as `Arguments` while DI fills interfaces like `ILogger` or `IMemoryCache`. Both require filter dependencies to be registered in the service collection — missing registrations throw at first request activation. `TypeFilter` is useful when the same filter type needs different parameter values on different controllers without separate registered types.

---

## Q13. How do you register a global filter in `AddControllersWithViews`?

**Concepts**
- `options.Filters.AddService<T>()` for DI-managed filters
- `options.Filters.Add<T>()` — no service registration, captive dependency risk
- Scoped filter requiring `AddScoped` registration to avoid captive dependency
- Global filters applying only to MVC controller actions, not Minimal API endpoints

**Answer**

Register the filter type in DI and add it to `MvcOptions.Filters` inside `AddControllersWithViews`, using `AddService<T>()` for DI-managed filters:

```csharp
builder.Services.AddScoped<AuditActionFilter>();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.AddService<AuditActionFilter>();
});
```

`AddService<T>()` resolves the filter from DI per request — required when the filter injects scoped services like `DbContext`. `Add<T>()` without service registration creates the filter via `ObjectFactory` and can cause captive dependency errors with scoped services. Global filters apply to all MVC controller actions but not to Minimal API endpoints mapped separately. Filter order can be controlled by implementing `IOrderedFilter` on the filter class.

---

## Q14. What is `[ValidateAntiForgeryToken]` as a filter?

**Concepts**
- `[ValidateAntiForgeryToken]` implemented as an authorization filter
- Runs in authorization stage — before model binding and action execution
- Validates `__RequestVerificationToken` field or `RequestVerificationToken` header against the cookie
- Missing or invalid token yields 400 Bad Request before the action runs

**Answer**

`[ValidateAntiForgeryToken]` is an authorization filter attribute that validates the antiforgery token on unsafe HTTP methods before the action executes, protecting against cross-site request forgery. It is implemented by `ValidateAntiForgeryTokenAuthorizationFilter` — it runs in the authorization stage, not the action stage, which means invalid tokens cause a 400 Bad Request before model binding and action execution — the action method never runs. Validation checks for `__RequestVerificationToken` form field or `RequestVerificationToken` header matching the cookie token issued by `IAntiforgery`. Form Tag Helpers emit the token automatically; AJAX and `fetch` calls must include it manually.

---

## Q15. What is `[AutoValidateAntiforgeryToken]`?

**Concepts**
- `[AutoValidateAntiforgeryToken]` applying antiforgery validation to all unsafe methods
- Equivalent to `[ValidateAntiForgeryToken]` on every POST/PUT/PATCH/DELETE
- Safe methods (GET, HEAD, OPTIONS) not validated
- AJAX JSON endpoints on the same controller inheriting the token requirement

**Answer**

`[AutoValidateAntiforgeryToken]` is a controller-level attribute that applies antiforgery validation to all unsafe HTTP methods on every action in the controller without decorating each action individually. It is equivalent to placing `[ValidateAntiForgeryToken]` on every POST, PUT, PATCH, and DELETE action. Safe methods (GET, HEAD, OPTIONS) are not validated — idempotent reads remain unaffected. When applied globally via a filter convention, all MVC POST actions require tokens unless opted out. AJAX JSON endpoints on the same controller inherit the requirement — clients must send the token header or receive 400 responses.

---

## Q16. What is `[IgnoreAntiforgeryToken]`?

**Concepts**
- `[IgnoreAntiforgeryToken]` opting an action or controller out of antiforgery validation
- Overrides `[AutoValidateAntiforgeryToken]` on the same controller for specific actions
- Valid use case — webhooks with HMAC signatures or API key authentication
- Must not be applied to state-changing browser-facing cookie-authenticated endpoints

**Answer**

`[IgnoreAntiforgeryToken]` opts an action or controller out of antiforgery validation, adding metadata that causes `ValidateAntiForgeryTokenAuthorizationFilter` to skip token checks for that endpoint. The correct use case is webhook endpoints that authenticate via HMAC signatures or API keys instead of cookie-based CSRF tokens. It overrides `[AutoValidateAntiforgeryToken]` on the same controller for specific actions decorated with `[IgnoreAntiforgeryToken]`. It should not be applied to state-changing browser-facing forms or cookie-authenticated AJAX endpoints — it removes CSRF protection. Minimal API and bearer-token APIs typically do not use antiforgery at all since CSRF is only relevant for cookie-based browser sessions.

---

## Q17. What is `[Authorize]` as an authorization filter?

**Concepts**
- `[Authorize]` running after authentication middleware has populated `HttpContext.User`
- `Roles`, `Policy`, and `AuthenticationSchemes` properties for targeting requirements
- `[AllowAnonymous]` overriding controller-level or global `[Authorize]`
- Authorization filter short-circuiting before model binding and action execution

**Answer**

`[Authorize]` is an authorization filter attribute that enforces authentication and optional policy or role requirements before the action executes, setting `context.Result` to a challenge or forbid response when the user is not permitted. It runs after authentication middleware has populated `HttpContext.User` from cookies, JWT bearer, or other handlers. It supports `Roles`, `Policy`, and `AuthenticationSchemes` properties to target specific authorization requirements. `[AllowAnonymous]` on an action overrides controller-level or global `[Authorize]` for that endpoint. Authorization filters short-circuit the pipeline — unauthorized requests never reach model binding or the action method.

---

## Q18. When should you use a filter instead of middleware for MVC-specific concerns?

**Concepts**
- Filter — requires MVC context (action name, route values, bound model, `IActionResult`)
- Middleware — applies to all request types before routing and endpoint selection
- Per-action audit with `ActionArguments` and controller metadata as a filter concern
- Correlation IDs, HTTPS, and static files as middleware concerns

**Answer**

Use filters when the concern requires MVC context — action name, route values, bound model arguments, or `IActionResult` manipulation — and use middleware when the concern applies to all request types or must run before routing. Per-action audit logging with `ActionArguments` and controller metadata belongs in an action filter — middleware only sees the URL path. Uniform antiforgery validation on MVC POST actions is filter-based; middleware has no built-in equivalent with the same token contract. Response caching of rendered views via result filters requires access to `ViewResult` — middleware cannot intercept view engine output the same way. Correlation IDs, HTTPS redirection, request size limits, and static file handling belong in middleware because they apply before endpoint selection or outside MVC entirely.

---

## Gotchas — Action Filters in MVC (Interview Traps)

---

#### Gotcha 1. Global, controller, and action filter execution order

**Concepts**
- Filter pipeline order — global â†’ controller-level â†’ action-level (for OnActionExecuting)
- Reverse order for OnActionExecuted — action-level â†’ controller-level â†’ global
- Filter ordering — `IOrderedFilter.Order` controls relative execution within the same scope
- Unexpected behavior — filter at wrong level changes execution order unexpectedly

**Answer**

The MVC filter pipeline runs `OnActionExecuting` in the order global â†’ controller â†’ action and `OnActionExecuted` in the reverse order — action â†’ controller â†’ global. This means a global logging filter's `OnActionExecuting` runs before a controller-level authorization filter if both are at the default order. `IOrderedFilter.Order` (lower number runs first for `OnActionExecuting`) controls relative execution within the pipeline. When a filter at a higher scope must run after one at a lower scope, override `Order` explicitly. Misunderstanding this order causes bugs where a "pre-execution" check runs after the action already executed or where a response-setting filter is overwritten by a later filter.

---

#### Gotcha 2. Synchronous filter used for async operations, blocking thread pool

**Concepts**
- `IActionFilter` — synchronous; cannot use `await` inside `OnActionExecuting`
- `IAsyncActionFilter` — async; wraps both pre- and post-execution in `OnActionExecutionAsync`
- `await next()` — must be called to proceed to the action; omitting it short-circuits
- Thread pool blocking — sync filter with `Task.Result` or `.Wait()` causes deadlock risk

**Answer**

`IActionFilter.OnActionExecuting` is synchronous — calling an async service with `.Result` or `.GetAwaiter().GetResult()` risks deadlocking the request under ASP.NET Core's synchronization context and blocks thread pool threads. `IAsyncActionFilter.OnActionExecutionAsync(context, next)` is the correct interface for async operations inside a filter. The pattern is: perform pre-action work, `var result = await next()` to invoke the action, then perform post-action work using `result.Exception` and `result.Result`. Forgetting `await next()` short-circuits the entire request — the action never runs and the client receives the filter's response (or an empty response if none is set).

---

#### Gotcha 3. Action filter short-circuiting the action without setting a result

**Concepts**
- Short-circuit — setting `context.Result` in `OnActionExecuting` skips the action method
- No result set — action is skipped but no response is sent; client receives empty 200
- Result filters still run — short-circuit does not skip result filters
- Exception filters — do not run when short-circuit is used without an exception

**Answer**

Setting `context.Result` in `OnActionExecuting` causes the MVC pipeline to skip the action method and proceed directly to result execution. If no `Result` is set but `context.Result` is assigned `null`, the client receives an empty response with no body. A common mistake is short-circuiting for authentication by setting no result — the action is skipped but the view or redirect response is never sent. The correct pattern for blocking access is `context.Result = new ForbidResult()` or `new RedirectToActionResult("Login", "Account", null)` which produces a proper HTTP response. Result filters still run after a short-circuit; exception filters do not because no exception was thrown.

---

#### Gotcha 4. Scoped service registered as singleton inside a filter attribute

**Concepts**
- Filter attributes — instantiated once by the framework; effectively singleton lifetime
- `[TypeFilter(typeof(MyFilter))]` — creates a new filter instance per request via DI
- `[ServiceFilter(typeof(MyFilter))]` — resolves from DI container; respects registered lifetime
- Captive dependency — singleton filter holding a scoped service outlives the request scope

**Answer**

Attribute-based filters (`[MyFilter]`) are instantiated once at application start and shared across all requests, making them effectively singletons regardless of any intended lifetime. Injecting a scoped service like `AppDbContext` into a filter attribute's constructor captures the scoped service in a singleton context — the same `DbContext` instance is used across all requests, causing stale data, thread-safety issues, and change tracking corruption. `[TypeFilter(typeof(MyFilter))]` creates a new filter instance per invocation using DI, respecting the registered lifetime. `[ServiceFilter(typeof(MyFilter))]` resolves the filter from the DI container per request when registered as scoped. Always use `TypeFilter` or `ServiceFilter` when the filter has scoped dependencies.

---

#### Gotcha 5. Resource filter vs action filter — model binding execution timing

**Concepts**
- `IResourceFilter.OnResourceExecuting` — runs before model binding
- `IActionFilter.OnActionExecuting` — runs after model binding
- Resource filter use case — caching entire request/response before model binding overhead
- Action filter use case — validation, logging, modification after model binding

**Answer**

`IResourceFilter.OnResourceExecuting` runs before model binding, before the action filter pipeline, and before result caching. `IActionFilter.OnActionExecuting` runs after model binding — `ActionContext.ActionArguments` is already populated with bound parameters. If a resource filter tries to access `context.ActionArguments` in `OnResourceExecuting`, the dictionary is empty because binding has not occurred. Resource filters are the correct place for whole-response caching (short-circuit before binding overhead), while action filters are the correct place for per-action validation, audit logging of action arguments, or authorization that requires access to bound model values.

---

#### Gotcha 6. Exception filter not running when middleware catches the exception first

**Concepts**
- MVC exception filter — runs for exceptions thrown inside the MVC action pipeline
- `UseExceptionHandler` middleware — higher in the pipeline, catches unhandled exceptions first
- Exception filter scope — MVC filter pipeline only; middleware catches everything outside MVC
- `UseStatusCodePages` vs `UseExceptionHandler` — different scopes and response formats

**Answer**

`IExceptionFilter` and `IAsyncExceptionFilter` handle exceptions thrown within the MVC action/filter pipeline. If `app.UseExceptionHandler("/Error")` middleware is in the pipeline, it also catches unhandled exceptions — and middleware runs outside and around the MVC pipeline. Depending on ordering and configuration, the middleware may catch the exception before the MVC exception filter even gets a chance to handle it. For API error responses, `UseExceptionHandler` is often the right layer since it catches all unhandled exceptions. For MVC-specific error responses that need access to action context (such as redirect-to-login for a specific action), the exception filter is appropriate. Both can coexist but developers must understand which layer catches which exceptions.

---

#### Gotcha 7. `IPageFilter` on Razor Pages vs `IActionFilter` on MVC controllers

**Concepts**
- `IActionFilter` — applies to MVC controller actions
- `IPageFilter` — applies to Razor Pages `PageModel` handlers
- Global filter — `IAsyncPageFilter` required for Razor Pages when registered globally
- Cross-type filter — filters must implement the correct interface for their target

**Answer**

An `IActionFilter` registered globally with `builder.Services.AddControllersWithViews(o => o.Filters.Add<MyFilter>())` applies to MVC controllers but not to Razor Pages page handlers. Razor Pages uses a separate filter pipeline that runs `IPageFilter` implementations. A global `IActionFilter` that logs action execution will miss all Razor Pages requests. To apply a filter to both MVC controllers and Razor Pages, implement both `IActionFilter` (or `IAsyncActionFilter`) and `IPageFilter` (or `IAsyncPageFilter`) in the same class and register it globally. When a project has both MVC controllers and Razor Pages, audit global filters to confirm they cover both pipelines.

---

#### Gotcha 8. Using `OnActionExecuted` to set the response when `context.Exception` is not handled

**Concepts**
- `OnActionExecuted` — runs after the action; `context.Exception` may be non-null
- Unhandled exception — propagates if `context.ExceptionHandled` is not set to `true`
- Setting result in `OnActionExecuted` — does not suppress an unhandled exception
- Correct pattern — set `context.ExceptionHandled = true` alongside setting `context.Result`

**Answer**

If the action method throws and is not caught by an exception filter, `OnActionExecuted` still runs with `context.Exception` set. Setting `context.Result` in `OnActionExecuted` does not suppress the exception — the exception propagates to the next exception handler after the filter completes unless `context.ExceptionHandled = true` is also set. A common mistake is writing a filter that sets a custom error result in `OnActionExecuted` and assuming the exception is handled, then seeing a 500 response because the exception continued propagating. The fix: `context.ExceptionHandled = true;` followed by `context.Result = new ObjectResult(errorResponse) { StatusCode = 500 };` in the `OnActionExecuted` handler to absorb the exception and produce the custom response.

---

#### Gotcha 9. Filter registered with `[ServiceFilter]` but type not registered in DI container

**Concepts**
- `[ServiceFilter(typeof(MyFilter))]` — resolves filter from the DI container
- Not registered in DI — `InvalidOperationException` at route invocation time, not startup
- `[TypeFilter(typeof(MyFilter))]` — creates instance directly via DI activation, no explicit registration needed
- Registration — `services.AddScoped<MyFilter>()` required for `[ServiceFilter]`

**Answer**

`[ServiceFilter(typeof(MyFilter))]` resolves the filter from the DI container. If `MyFilter` is not registered with `services.AddScoped<MyFilter>()` (or `AddTransient` or `AddSingleton`), the framework throws `InvalidOperationException: No service for type 'MyFilter' has been registered` at the first request to a route that uses the attribute — not at startup. `[TypeFilter(typeof(MyFilter))]` does not require explicit DI registration because it uses the DI container's `ActivatorUtilities.CreateInstance` to instantiate the type by resolving its constructor dependencies. For filters with constructor parameters that are DI-registered types, `[TypeFilter]` is simpler. For filters that have multiple consumers and need lifetime management via DI, `[ServiceFilter]` with explicit registration is cleaner.

---

#### Gotcha 10. Result filter modifying the response after it has already started streaming

**Concepts**
- `IResultFilter.OnResultExecuted` — called after result execution starts; headers may already be sent
- HTTP response — headers sent before body; cannot change status code after first byte
- `OnResultExecuting` — correct place to modify response before execution starts
- `context.Cancelled` — true if the result was short-circuited; check before modifying

**Answer**

`IResultFilter.OnResultExecuted` runs after the result has started executing — for streaming responses or large view renders, HTTP response headers may already be sent by the time this method is called. Attempting to change the status code or add response headers in `OnResultExecuted` throws `InvalidOperationException: Headers are read-only, response has already started`. Modifying the response (adding headers, changing status code, wrapping the response body) must be done in `OnResultExecuting` before the result is executed. `OnResultExecuted` is appropriate for cleanup, logging response details, or reading `context.Exception` after execution — not for modifying the HTTP response that has already begun.

---
## Scenario-Based Questions (Karat Format)

---

#### Q1. (R) Review this custom action filter registered globally on MVC controllers. Under load, thread-pool starvation appears and Kestrel queue depth grows. What is wrong, and when should this be `IAsyncActionFilter` instead of `IActionFilter`?

```csharp
public class TenantLookupFilter : IActionFilter
{
    private readonly ITenantRepository _tenants;

    public TenantLookupFilter(ITenantRepository tenants) => _tenants = tenants;

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var slug = context.RouteData.Values["tenant"]?.ToString();
        var tenant = _tenants.GetBySlugAsync(slug!).GetAwaiter().GetResult();
        context.HttpContext.Items["Tenant"] = tenant;
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}

// Program.cs
builder.Services.AddScoped<TenantLookupFilter>();
builder.Services.AddControllersWithViews(o => o.Filters.AddService<TenantLookupFilter>());
```

**Concepts**
- Sync-over-async `.GetAwaiter().GetResult()` blocking thread-pool threads
- `IAsyncActionFilter` required for any I/O-bound filter work
- Global filter multiplying blocking I/O across every MVC action under load
- Missing route value guard producing null argument to the async method

**Answer**

The filter blocks a request thread with sync-over-async (`.GetAwaiter().GetResult()`) inside `OnActionExecuting`. Because this is a globally registered filter that runs on every MVC action, concurrent requests each block a thread waiting for the database round-trip. Under load the thread pool exhausts and Kestrel cannot service new connections. The fix is to implement `IAsyncActionFilter` and await the repository call:

```csharp
public async Task OnActionExecutionAsync(ActionExecutingContext ctx, ActionExecutionDelegate next)
{
    var slug = ctx.RouteData.Values["tenant"]?.ToString();
    if (string.IsNullOrEmpty(slug))
    {
        ctx.Result = new NotFoundResult();
        return;
    }
    ctx.HttpContext.Items["Tenant"] = await _tenants.GetBySlugAsync(slug, ctx.HttpContext.RequestAborted);
    await next();
}
```

The guard on missing `tenant` also prevents a null-forgiving exception on `slug!` when the route value is absent. Keep `AddScoped` + `AddService` — that registration is correct. Use `IActionFilter` only for CPU-only checks; any filter touching I/O on the hot path must be `IAsyncActionFilter`.

---

#### Q2. (M) A controller has three filters on one action: a global `AuditFilter` (`Order = 100`), controller-level `[ValidateAntiForgeryToken]`, and action-level `[RequireRole("Admin")]`. Describe MVC filter **scope** (global → controller → action) and **order** within the same filter stage — which runs first within authorization filters, and can a lower-ordered authorization filter still short-circuit after a higher-ordered one passes?

**Concepts**
- Filter stage order — authorization before resource before action
- Scope order within a stage — global then controller then action
- `IOrderedFilter.Order` ascending on the executing leg within the same scope
- `[ValidateAntiForgeryToken]` as an authorization-stage filter, not an action-stage filter
- Any authorization filter setting `context.Result` short-circuits later stages

**Answer**

MVC runs filters by stage first (authorization → resource → action → exception → result), then by scope (global → controller → action) within each stage, then by `IOrderedFilter.Order` ascending on the executing leg. The authorization stage runs before action filters entirely, so `[RequireRole("Admin")]` and `[ValidateAntiForgeryToken]` both execute in the authorization stage — before `AuditFilter` ever runs.

`[ValidateAntiForgeryToken]` is an authorization filter (`ValidateAntiForgeryTokenAuthorizationFilter`), which is why invalid tokens reject before model binding. `[RequireRole("Admin")]` is also an authorization filter. Both run in the authorization stage before the action stage where `AuditFilter` (Order = 100) runs.

Within the authorization stage, scope determines order — action-level `[RequireRole]` runs after any global or controller-level authorization filters within that stage. Setting `context.Result` in any authorization filter short-circuits the action and all remaining action filters — but whether later authorization filters in the same stage still run depends on the pipeline construction. The safest pattern is to have one authoritative authorization filter with the earliest `Order`, rather than relying on behavior after a prior auth filter passes.

---

#### Q3. (R) Review global filter registration and DI. Production enables scope validation; the first POST to `/Orders/Create` throws `InvalidOperationException: Cannot consume scoped service 'AppDbContext' from singleton`. Diagnose.

```csharp
builder.Services.AddDbContext<AppDbContext>();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<AuditActionFilter>(); // not AddService
});

public class AuditActionFilter : IAsyncActionFilter
{
    private readonly AppDbContext _db;
    public AuditActionFilter(AppDbContext db) => _db = db;

    public async Task OnActionExecutionAsync(ActionExecutingContext ctx, ActionExecutionDelegate next)
    {
        _db.AuditLogs.Add(new AuditLog { Action = ctx.ActionDescriptor.DisplayName });
        await _db.SaveChangesAsync();
        await next();
    }
}
```

**Concepts**
- `Filters.Add<T>()` creating a filter as an effectively singleton via `ObjectFactory`
- Scoped `AppDbContext` captive in the singleton-lifetime filter
- `Filters.AddService<T>()` with `AddScoped` registration for correct per-request lifetime
- Auditing after `await next()` to log only successful action completions

**Answer**

`options.Filters.Add<AuditActionFilter>()` resolves the filter through `ObjectFactory` as an effectively singleton filter instance. `AppDbContext` is scoped — resolved per request — so capturing it in a singleton-lifetime filter creates a captive dependency. With scope validation enabled in production, the first request throws `InvalidOperationException` rather than silently reusing a stale `DbContext` across requests.

Fix: register the filter as scoped and use `AddService`:

```csharp
builder.Services.AddScoped<AuditActionFilter>();
builder.Services.AddControllersWithViews(o => o.Filters.AddService<AuditActionFilter>());
```

Also prefer auditing after `await next()` so only successful actions log, or use a try/finally pattern to capture both outcomes. An alternative is to inject `IDbContextFactory<AppDbContext>` if the filter must remain singleton-compatible, creating a fresh context per invocation.

---

#### Q4. (D) Product wants to block unauthenticated traffic before a custom middleware that opens an expensive read replica connection on every request. A developer proposes `[Authorize]` on every MVC controller instead of moving auth earlier in the pipeline. Compare authorization-filter short-circuit (`context.Result = …`) vs middleware short-circuit (no `_next`). Which seam fixes replica churn for MVC-only apps vs mixed MVC + minimal API hosts?

**Concepts**
- Authorization filter short-circuiting after middleware pipeline has already executed on the way in
- Middleware short-circuit before the expensive operation — pipeline never continues past it
- `[Authorize]` on every controller not preventing replica middleware from running
- Mixed MVC + Minimal API — controller filters not covering Minimal endpoints

**Answer**

`[Authorize]` short-circuits at the authorization filter stage — after routing, endpoint selection, and all middleware registered before the MVC endpoint, including the replica middleware — so it does not stop replica connection churn. The middleware already executed on the way in before filters even begin.

Middleware short-circuit means not calling `_next` — the request never reaches the replica middleware further down the pipeline, since middleware executes in order and not calling `_next` ends traversal at that point.

For an MVC-only app, the fix is to place authentication and authorization middleware before the expensive replica middleware: `UseAuthentication` → `UseAuthorization` → replica middleware (conditional on the user being authenticated), or use `MapWhen` to branch authenticated-only paths before the replica middleware runs. The `[Authorize]` attributes on controllers still enforce per-action authorization semantics, but the replica cost fix requires pipeline order.

For mixed MVC + Minimal API, controller `[Authorize]` leaves Minimal API routes unprotected unless they also call `RequireAuthorization()`. Middleware-level auth or endpoint metadata is the unified seam — it applies to both endpoint types. Using `UseAuthorization` before the replica middleware, combined with a fallback policy or `RequireAuthorization` on individual endpoints, is the correct approach for a mixed host.

---

#### Q5. (R) Review this exception filter deployed to Production alongside `app.UseExceptionHandler()`. Clients sometimes receive HTML error pages and sometimes JSON `ProblemDetails` for the same exception type; logs show duplicate stack traces. What is wrong?

```csharp
public class MvcExceptionFilter : IExceptionFilter
{
    private readonly ILogger<MvcExceptionFilter> _logger;

    public MvcExceptionFilter(ILogger<MvcExceptionFilter> logger) => _logger = logger;

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "Action failed");
        context.ExceptionHandled = true;
        context.Result = new ObjectResult(new ProblemDetails
        {
            Status = 500,
            Title = "Server error",
            Detail = context.Exception.Message
        }) { StatusCode = 500 };
        throw context.Exception; // ensure middleware also sees it
    }
}
```

**Concepts**
- `ExceptionHandled = true` then rethrow — double handling by filter and middleware
- Inconsistent error shape — filter emits JSON, exception middleware emits HTML
- Information disclosure via `context.Exception.Message` in production responses
- Duplicate logging from both filter and middleware processing the same fault

**Answer**

The filter marks the exception handled and then rethrows, so MVC emits the JSON `ObjectResult` while `UseExceptionHandler` also processes the same exception and may return an HTML error page — the race between these two handlers produces inconsistent error shapes and duplicate log entries.

Remove the `throw` — if the filter marks the exception handled, it must return cleanly from `OnException` without rethrowing. Pick one strategy: either the exception filter handles MVC actions and returns JSON `ProblemDetails`, or the centralized `IExceptionHandler` middleware handles all exceptions and the exception filter is removed. Never expose `context.Exception.Message` in production responses — log the full detail server-side and return a safe generic message:

```csharp
public void OnException(ExceptionContext context)
{
    _logger.LogError(context.Exception, "Action {Action} failed", context.ActionDescriptor.DisplayName);
    context.ExceptionHandled = true;
    context.Result = new ObjectResult(new ProblemDetails { Status = 500, Title = "An error occurred." })
        { StatusCode = 500 };
}
```

---

#### Q6. (R) Review this result filter meant to skip view rendering when a cached HTML fragment exists. QA reports the view engine still runs and `_Layout.cshtml` executes twice on cache hits. Diagnose.

```csharp
public class FragmentCacheResultFilter : IResultFilter
{
    private readonly IMemoryCache _cache;

    public FragmentCacheResultFilter(IMemoryCache cache) => _cache = cache;

    public void OnResultExecuting(ResultExecutingContext context)
    {
        var key = context.HttpContext.Request.Path.Value;
        if (_cache.TryGetValue(key, out string? html))
            context.HttpContext.Response.WriteAsync(html); // write cached body
    }

    public void OnResultExecuted(ResultExecutedContext context) { }
}
```

**Concepts**
- `OnResultExecuting` requires `context.Result` assignment to short-circuit view rendering
- Writing to the response stream without replacing the result — both outputs concatenate
- Fire-and-forget `WriteAsync` not awaited — incomplete writes under load
- Cache key based on path only — personalized responses colliding

**Answer**

Writing to the response in `OnResultExecuting` without assigning `context.Result` does not short-circuit result execution — the original `ViewResult` still renders, so the layout executes and its output concatenates with or overwrites the cached fragment. The view engine only skips when it sees a different result has been assigned.

The fix on a cache hit is to assign a short-circuiting result:

```csharp
public void OnResultExecuting(ResultExecutingContext context)
{
    var key = BuildCacheKey(context.HttpContext);
    if (_cache.TryGetValue(key, out string? html))
        context.Result = new ContentResult { Content = html, ContentType = "text/html; charset=utf-8" };
}
```

Additionally, `Response.WriteAsync` is async and must be awaited — fire-and-forget in a filter produces incomplete writes under concurrent load. The cache key should include tenant, user, or version information so personalized responses do not collide, and cache headers should be set explicitly when serving cached HTML.

---

#### Q7. (P) You need an audit action filter on every MVC controller but not on `/api/*` minimal routes in the same app. Show the correct global registration pattern with `AddControllersWithViews`, filter `Order`, and how `[AllowAnonymous]` / `[IgnoreAntiforgeryToken]` interact with globally registered filters.

**Concepts**
- `AddControllersWithViews` global filters applying only to MVC controllers, not Minimal APIs
- `IOrderedFilter` controlling execution position relative to other filters
- `[AllowAnonymous]` bypassing authorization requirements — not unrelated action filters
- `[IgnoreAntiforgeryToken]` adding metadata for antiforgery to skip — unrelated to audit filters

**Answer**

Register the audit filter globally via `AddControllersWithViews` — because MVC global filters are discovered only for MVC controllers, Minimal API endpoints mapped with `app.MapGroup("/api/...")` never run them:

```csharp
builder.Services.AddScoped<AuditActionFilter>();
builder.Services.AddControllersWithViews(o => o.Filters.AddService<AuditActionFilter>());
// Minimal APIs mapped separately — no MVC action filters run on them
```

Set `Order` by implementing `IOrderedFilter`. Use `Order = 1000` to run the audit filter after validation filters (`Order = 0` or negative) so the audit log only captures requests that passed validation:

```csharp
public class AuditActionFilter : IAsyncActionFilter, IOrderedFilter
{
    public int Order => 1000;
    // ...
}
```

`[AllowAnonymous]` bypasses authorization requirements only — a global action filter still runs unless the filter explicitly checks `context.ActionDescriptor.EndpointMetadata` for `IAllowAnonymous` and no-ops. `[IgnoreAntiforgeryToken]` adds `IgnoreAntiforgeryTokenAttribute` metadata so the antiforgery authorization filter skips token validation — it has no effect on unrelated action filters like audit logging. For selective MVC exclusion, prefer a `[SkipAudit]` marker attribute checked in the filter's `OnActionExecutionAsync`, or apply via a base controller class rather than registering globally.

---

#### Q8. (R) A teammate registers a filter with `[TypeFilter(typeof(RateLimitFilter), Arguments = new object[] { 60 })]` but the app fails at first request with `InvalidOperationException` about `IRateLimitStore`. Another controller uses `[ServiceFilter(typeof(RateLimitFilter))]` successfully. Explain filter factory resolution (`TypeFilterAttribute` vs `ServiceFilterAttribute`) and fix the broken registration.

```csharp
public class RateLimitFilter : IActionFilter
{
    private readonly IRateLimitStore _store;
    private readonly int _perMinute;

    public RateLimitFilter(IRateLimitStore store, int perMinute)
    {
        _store = store;
        _perMinute = perMinute;
    }
    // OnActionExecuting checks _store ...
}

// Broken controller
[TypeFilter(typeof(RateLimitFilter), Arguments = new object[] { 60 })]
public class ReportsController : Controller { }

// Working controller — Program.cs has: builder.Services.AddScoped<RateLimitFilter>();
[ServiceFilter(typeof(RateLimitFilter))]
public class InvoicesController : Controller { }
```

**Concepts**
- `TypeFilterAttribute` — DI fills interface parameters, `Arguments` fills primitive parameters
- `ServiceFilterAttribute` — entire filter resolved from DI including all ctor params
- Both requiring all DI-resolved dependencies registered in the service collection
- `Arguments` supplying non-DI constructor values alongside DI-resolved services

**Answer**

`TypeFilterAttribute` constructs the filter using a factory that resolves interface parameters from DI and fills primitive parameters from `Arguments`. When `IRateLimitStore` is not registered in DI, the factory cannot resolve it and throws at first request activation. `ServiceFilterAttribute` resolves the entire pre-registered filter from DI — `InvoicesController` works because `RateLimitFilter` is registered as a scoped service, but that registration hardcodes whatever `perMinute` value is configured in DI (likely zero or default), not the per-controller `60`.

The fix for `ReportsController` is to register `IRateLimitStore`:

```csharp
builder.Services.AddScoped<IRateLimitStore, RateLimitStore>();
// TypeFilter: DI resolves IRateLimitStore, Arguments supplies perMinute=60
[TypeFilter(typeof(RateLimitFilter), Arguments = new object[] { 60 })]
public class ReportsController : Controller { }
```

`TypeFilter` = factory with extra constructor args; `ServiceFilter` = fully DI-managed instance. Both require all DI dependencies registered. `TypeFilter` is the right choice when you need per-decoration configuration values; `ServiceFilter` is the right choice when all configuration flows through DI (e.g., `IOptions<RateLimitOptions>`).

---

#### Q9. (R) An AJAX partial-update endpoint returns `400 Bad Request` with empty body after deploy. The action has `[ValidateAntiForgeryToken]`; the jQuery client POSTs JSON to `/Cart/AddItem` without a token header. Review the controller setup and client contract — what changed with `[AutoValidateAntiforgeryToken]`, and how do you fix AJAX without disabling CSRF?

```csharp
[AutoValidateAntiforgeryToken]
public class CartController : Controller
{
    [HttpPost]
    public IActionResult AddItem([FromBody] AddItemRequest request) { /* ... */ }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public IActionResult AddItemLegacy(AddItemRequest request) { /* ... */ }
}

// Client
$.ajax({ url: '/Cart/AddItem', method: 'POST', contentType: 'application/json',
         data: JSON.stringify({ sku: 'ABC', qty: 1 }) });
```

**Concepts**
- `[AutoValidateAntiforgeryToken]` applying to all unsafe HTTP methods on the controller
- JSON AJAX POST without `RequestVerificationToken` header — 400 before action executes
- `[IgnoreAntiforgeryToken]` valid for webhooks — not for cookie-authenticated cart endpoints
- Rendering the antiforgery token in the page for AJAX header inclusion

**Answer**

`[AutoValidateAntiforgeryToken]` applies antiforgery validation to all unsafe methods on the controller — the JSON AJAX POST to `AddItem` lacks the `RequestVerificationToken` header, so `ValidateAntiForgeryTokenAuthorizationFilter` short-circuits with 400 before the action runs. The 400 with empty body is correct behavior — CSRF protection is working.

The fix is to send the token from the page, not to disable antiforgery:

1. Render the token in the page: `@Html.AntiForgeryToken()` or extract it from an existing form's hidden field.
2. Include it in the jQuery AJAX call:

```javascript
headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() }
```

`[IgnoreAntiforgeryToken]` on `AddItemLegacy` is acceptable only if that endpoint authenticates via an alternate mechanism such as API key or HMAC — not for cookie-authenticated cart mutations, where CSRF is a real threat. For a pure JSON API surface, moving the endpoint to `ControllerBase` and requiring bearer token authentication eliminates the need for antiforgery entirely, since CSRF only applies to cookie-based sessions.

---

#### Q10. (R) Review this resource filter intended to memoize an expensive catalog lookup **once per HTTP request** across two action filters and the action. Instead, users intermittently see another tenant's catalog after scale-out to three pods. Diagnose lifetime and caching scope.

```csharp
public class CatalogResourceFilter : IResourceFilter
{
    private static readonly Dictionary<string, CatalogDto> _requestCache = new();

    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        var tenantId = context.HttpContext.User.FindFirst("tenant_id")?.Value;
        var key = $"{tenantId}:{context.RouteData.Values["categoryId"]}";
        if (_requestCache.TryGetValue(key, out var hit))
            context.HttpContext.Items["Catalog"] = hit;
    }

    public void OnResourceExecuted(ResourceExecutedContext context)
    {
        if (context.HttpContext.Items["Catalog"] is CatalogDto dto)
        {
            var tenantId = context.HttpContext.User.FindFirst("tenant_id")?.Value;
            var key = $"{tenantId}:{context.RouteData.Values["categoryId"]}`;
            _requestCache[key] = dto;
        }
    }
}
```

**Concepts**
- Static `Dictionary<>` as process-wide state — not per-request
- Cross-tenant data leak from shared static cache
- Unsynchronized `Dictionary<>` corrupted under concurrent writes
- `HttpContext.Items` as the correct per-request memoization scope

**Answer**

The `static` `_requestCache` field is process-wide, not per-request — it persists across all requests and all tenants on the same pod, leaking catalog data between tenants. Under concurrent requests, the unsynchronized `Dictionary<string, CatalogDto>` is also susceptible to corruption from race conditions. After scale-out to three pods, the static field is further per-pod, making behavior inconsistent across instances.

For per-request memoization, use `HttpContext.Items` only — no static field:

```csharp
public void OnResourceExecuting(ResourceExecutingContext context)
{
    if (context.HttpContext.Items.ContainsKey("Catalog")) return;
    // load catalog once into Items — visible to action filters and action in the same request
}
```

For cross-request caching with tenant isolation, inject `IMemoryCache` (or a distributed cache) with a key of `tenantId:categoryId`, a sliding expiration, and never share mutable DTOs without copying. The comment saying "per request" is misleading when backed by a static field — a static field is a singleton regardless of filter stage.

---

#### Q11. (D) Your MVC app needs (a) per-action audit with route values and bound model metadata, (b) uniform 413 rejection for oversized uploads before model binding, and (c) a correlation ID on every response including static files. For each concern, choose middleware, authorization filter, action filter, resource filter, or result filter — and cite one reason the wrong layer fails.

**Concepts**
- Action filter for per-action audit with `ActionArguments` and `ActionDescriptor`
- Middleware for pre-routing, cross-endpoint, and static file concerns
- Kestrel request size limits for upload rejection before model binding
- Static files middleware bypassing the MVC result pipeline entirely

**Answer**

The correct layer for each concern:

**(a) Per-action audit with route values and bound model metadata → action filter (`IAsyncActionFilter`).** The `ActionExecutingContext.ActionArguments` dictionary contains bound model values and `ActionDescriptor` carries the controller and action name. Middleware only sees the URL path — it has no access to bound model types without expensive reflection hacks.

**(b) 413 rejection for oversized uploads before model binding → middleware + `KestrelServerLimits.MaxRequestBodySize`.** Action and resource filters run after routing and model binding has started — the large body may already be buffered in memory. Setting `options.Limits.MaxRequestBodySize` in Kestrel, or `app.Use` middleware that checks `Content-Length` before binding, prevents the body from ever being consumed.

**(c) Correlation ID on every response including static files → middleware (early in the pipeline).** MVC result filters never run for `UseStaticFiles` short-circuit paths — static files are served before routing, so the MVC filter pipeline is never entered. Only middleware registered before `UseStaticFiles` runs for every response type including static file responses.

---

#### Q12. (M) Two action filters both implement `IOrderedFilter`: `IdempotencyFilter` (`Order = -1000`, short-circuits duplicate POSTs) and `ModelStateValidationFilter` (`Order = 0`, returns `BadRequestObjectResult` when invalid). Walk through `OnActionExecutionAsync` / `OnActionExecuting` invocation order for `[ServiceFilter(typeof(IdempotencyFilter)), ServiceFilter(typeof(ModelStateValidationFilter))]` on a POST action — which runs first, and why does order matter for idempotency?

**Concepts**
- Lower `Order` running first on the executing leg
- Reversed execution order on the executed (after-action) leg
- Idempotency filter at large negative `Order` to guard before validation consumes the key
- Short-circuit by first filter prevents action and later executing filters from running

**Answer**

Lower `Order` runs first on the executing (before-action) leg — `IdempotencyFilter` at `Order = -1000` runs before `ModelStateValidationFilter` at `Order = 0`. On the executed (after-action) leg the order reverses, so validation's `OnActionExecuted` runs before idempotency's post-action logic, mirroring the middleware onion model.

```
Executing:  IdempotencyFilter (-1000) → ModelStateValidationFilter (0) → action
Executed:   ModelStateValidationFilter (0) → IdempotencyFilter (-1000)
```

Order matters for idempotency because if validation ran first (`Order 0` before `-1000`), an invalid request could consume or store an idempotency key before being rejected — creating a scenario where a corrected retry is refused as a duplicate even though the original attempt was invalid and never committed. Running idempotency first checks whether the request has already been processed; if it has, it short-circuits via `context.Result` and the action never runs, which also means the validation filter's executing phase is skipped for that request (once the first filter short-circuits, later executing phases do not run). For new requests, idempotency passes, validation runs next, and on the executed leg the idempotency post-action logic persists the key only after a successful action.
