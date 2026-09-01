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

What are action filters in ASP.NET Core MVC?

**Answer:** Action filters are components that run before and after MVC action methods (and around other pipeline stages) to implement cross-cutting concerns such as logging, validation, caching, and authorization without duplicating code in every action.

- They implement interfaces like `IActionFilter`, `IAsyncActionFilter`, `IAuthorizationFilter`, `IResourceFilter`, `IResultFilter`, and `IExceptionFilter`.
- Filters receive rich MVC context objects (`ActionExecutingContext`, `ResultExecutingContext`) with route data, action arguments, and the ability to short-circuit by setting `context.Result`.
- They are registered globally, at the controller level, or on individual actions via attributes or `AddControllersWithViews` configuration.
- Built-in attributes such as `[Authorize]`, `[ValidateAntiForgeryToken]`, and `[ResponseCache]` are implemented as filters.

---

## Q2. What is the MVC filter pipeline?

What is the MVC filter pipeline?

**Answer:** The MVC filter pipeline is the ordered sequence of filter stages that wrap endpoint execution after routing selects an MVC action but before and after the action method and result execute.

- Stages run in order: authorization → resource → action → (action method) → exception handling → result.
- Within each stage, filters run by scope (global → controller → action) and then by `IOrderedFilter.Order`.
- A filter can set `context.Result` to short-circuit remaining stages — for example, returning 401 before model binding completes.
- The pipeline is distinct from middleware — filters only run for MVC controller actions and Razor Pages, not for Minimal API endpoints unless equivalent middleware is used.

---

## Q3. What are the filter stages (authorization, resource, action, exception, result)?

What are the filter stages (authorization, resource, action, exception, result)?

**Answer:** MVC defines five filter stages that execute in a fixed order around the action method and its result, each addressing a different concern in the request lifecycle.

- **Authorization filters** (`IAuthorizationFilter`) run first — enforce authentication/authorization and antiforgery validation before resource and action stages.
- **Resource filters** (`IResourceFilter`) wrap the rest of the pipeline — useful for short-circuiting before model binding or caching per request via `HttpContext.Items`.
- **Action filters** (`IActionFilter` / `IAsyncActionFilter`) run immediately before and after the action method — logging, model-state checks, and idempotency guards belong here.
- **Exception filters** (`IExceptionFilter`) handle exceptions thrown from actions or earlier filters when not caught elsewhere.
- **Result filters** (`IResultFilter`) run before and after the `IActionResult` executes — view rendering, output caching, and response header manipulation.

---

## Q4. What is the difference between action filters and middleware?

What is the difference between action filters and middleware?

**Answer:** Middleware runs for every request that reaches it in the pipeline (including static files and Minimal APIs), while action filters run only for matched MVC/Razor Page endpoints after routing and endpoint selection.

- Middleware executes before routing and has no access to action descriptors, bound models, or `ActionArguments` — filters have full MVC context.
- Short-circuiting in middleware prevents later middleware from running; filter short-circuiting skips remaining filter stages and the action but cannot undo middleware that already executed.
- Cross-cutting concerns affecting all requests (correlation IDs, HTTPS, static files) belong in middleware; per-action audit with route values belongs in action filters.
- Expensive work that must run before authentication (rate limiting all traffic) belongs in middleware placed before auth — `[Authorize]` filters run too late to prevent that cost.

---

## Q5. What are `IActionFilter` and `IAsyncActionFilter`?

What are `IActionFilter` and `IAsyncActionFilter`?

**Answer:** `IActionFilter` defines synchronous `OnActionExecuting` and `OnActionExecuted` methods; `IAsyncActionFilter` defines `OnActionExecutionAsync` for async work before and after the action without blocking thread-pool threads.

- `OnActionExecuting` runs before the action — set `context.Result` to skip the action (e.g., return `BadRequestObjectResult` for invalid model state).
- `OnActionExecuted` runs after the action — inspect or modify `context.Result`, log exceptions from `context.Exception`.
- Prefer `IAsyncActionFilter` when the filter performs I/O (database lookups, HTTP calls) — blocking with `.GetAwaiter().GetResult()` in `IActionFilter` causes thread-pool starvation under load.
- Both interfaces participate in the same ordering rules via `IOrderedFilter.Order`.

---

## Q6. What is `IAuthorizationFilter`?

What is `IAuthorizationFilter`?

**Answer:** `IAuthorizationFilter` runs in the authorization stage before resource and action filters, determining whether the caller may execute the action — the built-in `[Authorize]` attribute implements this interface.

- `OnAuthorization` receives `AuthorizationFilterContext` with `HttpContext.User`, endpoint metadata, and the ability to set `context.Result` to `ChallengeResult` or `ForbidResult`.
- `[ValidateAntiForgeryToken]` is implemented as an authorization filter (`ValidateAntiForgeryTokenAuthorizationFilter`), not an action filter — invalid tokens reject before model binding.
- Authorization filters run after authentication middleware has populated `HttpContext.User` but before the action executes.
- Multiple authorization filters all run in order; any one can short-circuit with an unauthorized result.

---

## Q7. What is `IExceptionFilter`?

What is `IExceptionFilter`?

**Answer:** `IExceptionFilter` handles exceptions thrown during action execution or earlier filter stages, allowing MVC-specific error responses such as custom views or JSON `ProblemDetails` before the exception propagates to middleware.

- `OnException` receives `ExceptionContext` with the thrown exception and can set `context.ExceptionHandled = true` and assign `context.Result`.
- Setting `ExceptionHandled = true` and then rethrowing causes double handling when global exception middleware also processes the same fault — pick one strategy.
- For API-heavy apps, centralized `IExceptionHandler` middleware often replaces exception filters; filters remain useful for returning area-specific HTML error views.
- Do not expose raw `Exception.Message` in Production responses — log server-side and return safe generic messages.

---

## Q8. What is `IResultFilter`?

What is `IResultFilter`?

**Answer:** `IResultFilter` runs before and after the `IActionResult` executes — for example before a `ViewResult` renders Razor and after the view engine produces output.

- `OnResultExecuting` can replace `context.Result` entirely (e.g., return cached `ContentResult` instead of executing a `ViewResult`) to skip view rendering.
- Writing directly to the response stream in `OnResultExecuting` without replacing `context.Result` does not prevent the original result from executing — assign a new result to short-circuit.
- `OnResultExecuted` runs after the result executes — useful for response compression hooks or logging rendered status codes.
- Result filters do not run for static file responses or Minimal API `IResult` handlers that bypass the MVC result pipeline.

---

## Q9. What is `IResourceFilter`?

What is `IResourceFilter`?

**Answer:** `IResourceFilter` wraps execution of all later filter stages and the action itself, running immediately after authorization filters and before model binding and action filters on the way in.

- `OnResourceExecuting` can set `context.Result` to short-circuit before model binding — useful for request-level caching checks via `HttpContext.Items`.
- `OnResourceExecuted` runs after the action and result complete on the way out.
- `HttpContext.Items` is the correct per-request cache for resource filters — static fields leak data across requests and tenants.
- Resource filters are less commonly authored than action filters but are the earliest stage that still has full MVC endpoint context.

---

## Q10. What is the difference between global, controller-level, and action-level filters?

What is the difference between global, controller-level, and action-level filters?

**Answer:** Filters apply at three scopes — global filters affect every MVC action, controller-level filters affect all actions on that controller, and action-level filters affect only the decorated action — with all three potentially running on a single request.

- Global filters register in `AddControllersWithViews(options => options.Filters.Add...)` or via `AddService<T>()`.
- Controller-level filters use class attributes: `[ServiceFilter(typeof(AuditFilter))]` on the controller class.
- Action-level attributes on a single method add filters only for that action.
- When multiple scopes apply, filters within the same stage run in order: global → controller → action, then by `IOrderedFilter.Order` within each scope.

---

## Q11. How does filter order (`IOrderedFilter`) work?

How does filter order (`IOrderedFilter`) work?

**Answer:** Filters implementing `IOrderedFilter` expose an `Order` property — lower values run first on the executing (before) leg and last on the executed (after) leg, mirroring middleware's onion model.

- Default `Order` is `0` when not specified — explicit negative values (e.g., `-1000`) run before default-ordered filters on the executing side.
- On the executing leg: sort ascending by `Order` — idempotency guards should use large negative values to run before validation filters.
- On the executed leg: order reverses — validation's `OnActionExecuted` runs before idempotency's post-action logic.
- Scope (global → controller → action) is applied before `Order` within the same filter type and stage.

---

## Q12. What is the difference between `[ServiceFilter]` and `[TypeFilter]`?

What is the difference between `[ServiceFilter]` and `[TypeFilter]`?

**Answer:** `[ServiceFilter(typeof(MyFilter))]` resolves the filter entirely from DI using its registered constructor dependencies, while `[TypeFilter(typeof(MyFilter), Arguments = new object[] { 60 })]` constructs the filter via a factory, supplying extra constructor arguments alongside DI-resolved services.

- `ServiceFilter` requires `builder.Services.AddScoped<MyFilter>()` (or appropriate lifetime) — all constructor parameters come from DI.
- `TypeFilter` supports passing primitive or configuration values (e.g., rate limit threshold) as `Arguments` while DI fills interfaces like `ILogger` or `IMemoryCache`.
- Both require filter dependencies to be registered in the service collection — missing registrations throw at first request activation.
- `TypeFilter` is useful when the same filter type needs different parameter values on different controllers without separate registered types.

---

## Q13. How do you register a global filter in `AddControllersWithViews`?

How do you register a global filter in `AddControllersWithViews`?

**Answer:** Register the filter type in DI and add it to `MvcOptions.Filters` inside `AddControllersWithViews`, using `AddService<T>()` for DI-managed filters or `Add<T>()` for filters without scoped dependencies.

```csharp
builder.Services.AddScoped<AuditActionFilter>();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.AddService<AuditActionFilter>();
});
```

- `AddService<T>()` resolves the filter from DI per request — required when the filter injects scoped services like `DbContext`.
- `Add<T>()` without service registration creates the filter via `ObjectFactory` and can cause captive dependency errors with scoped services.
- Global filters apply to all MVC controller actions but not to Minimal API endpoints mapped separately.
- Filter order can be controlled by implementing `IOrderedFilter` on the filter class.

---

## Q14. What is `[ValidateAntiForgeryToken]` as a filter?

What is `[ValidateAntiForgeryToken]` as a filter?

**Answer:** `[ValidateAntiForgeryToken]` is an authorization filter attribute that validates the antiforgery token on unsafe HTTP methods (POST, PUT, DELETE) before the action executes, protecting against cross-site request forgery.

- It is implemented by `ValidateAntiForgeryTokenAuthorizationFilter` — it runs in the authorization stage, not the action stage.
- Validation checks for `__RequestVerificationToken` form field or `RequestVerificationToken` header matching the cookie token issued by `IAntiforgery`.
- Invalid or missing tokens cause a 400 Bad Request before model binding and action execution — the action method never runs.
- Form Tag Helpers emit the token automatically; AJAX and `fetch` calls must include the token manually.

---

## Q15. What is `[AutoValidateAntiforgeryToken]`?

What is `[AutoValidateAntiforgeryToken]`?

**Answer:** `[AutoValidateAntiforgeryToken]` is a controller-level attribute (or global convention) that applies antiforgery validation to all unsafe HTTP methods on every action in the controller without decorating each action individually.

- Equivalent to placing `[ValidateAntiForgeryToken]` on every POST, PUT, PATCH, and DELETE action in the controller.
- Safe methods (GET, HEAD, OPTIONS) are not validated — idempotent reads remain unaffected.
- When applied globally via `AutoValidateAntiforgeryTokenAttribute` as a filter convention, all MVC POST actions require tokens unless opted out.
- AJAX JSON endpoints on the same controller inherit the requirement — clients must send the token header or receive 400 responses.

---

## Q16. What is `[IgnoreAntiforgeryToken]`?

What is `[IgnoreAntiforgeryToken]`?

**Answer:** `[IgnoreAntiforgeryToken]` opts an action or controller out of antiforgery validation, adding metadata that causes `ValidateAntiForgeryTokenAuthorizationFilter` to skip token checks for that endpoint.

- Use for webhook endpoints that authenticate via HMAC signatures or API keys instead of cookie-based CSRF tokens.
- Overrides `[AutoValidateAntiforgeryToken]` on the same controller for specific actions decorated with `[IgnoreAntiforgeryToken]`.
- Should not be applied to state-changing browser-facing forms or cookie-authenticated AJAX endpoints — it removes CSRF protection.
- Minimal API and bearer-token APIs typically do not use antiforgery at all — CSRF protection is relevant for cookie-based browser sessions.

---

## Q17. What is `[Authorize]` as an authorization filter?

What is `[Authorize]` as an authorization filter?

**Answer:** `[Authorize]` is an authorization filter attribute that enforces authentication and optional policy/role requirements before the action executes, setting `context.Result` to a challenge or forbid response when the user is not permitted.

- Runs after authentication middleware has populated `HttpContext.User` from cookies, JWT bearer, or other handlers.
- Supports `Roles`, `Policy`, and `AuthenticationSchemes` properties to target specific authorization requirements.
- `[AllowAnonymous]` on an action overrides controller-level or global `[Authorize]` for that endpoint.
- Authorization filters short-circuit the pipeline — unauthorized requests never reach model binding or the action method.

---

## Q18. When should you use a filter instead of middleware for MVC-specific concerns?

When should you use a filter instead of middleware for MVC-specific concerns?

**Answer:** Use filters when the concern requires MVC context — action name, route values, bound model arguments, or `IActionResult` manipulation — and use middleware when the concern applies to all request types or must run before routing.

- Per-action audit logging with `ActionArguments` and controller metadata belongs in an action filter — middleware only sees the URL path.
- Uniform antiforgery validation on MVC POST actions is filter-based; middleware has no built-in equivalent with the same token contract.
- Response caching of rendered views via result filters requires access to `ViewResult` — middleware cannot intercept view engine output the same way.
- Correlation IDs, HTTPS redirection, request size limits, and static file handling belong in middleware because they apply before endpoint selection or outside MVC entirely.

---

---

## Gotchas — ASP.NET Core MVC (Interview Traps)

#### Gotcha 1. Business logic in Razor views

**Answer:** Placing pricing, discount, authorization, or business rules in `.cshtml` files bypasses unit tests, duplicates service-layer logic, and makes behavior hard to change consistently.

- Views should render data the controller or ViewModel already prepared.
- Authorization belongs in filters, policies, or controller/service checks before the view executes.
- Calculations in Razor cannot be tested independently and often diverge from API or batch logic.
- Keep Razor limited to presentation formatting — not business decisions.

---

#### Gotcha 2. EF entities passed directly to views

**Answer:** Binding and displaying EF Core entities exposes navigation properties, causes over-posting on POST, and couples the UI to the database schema.

- Lazy-loaded navigations can trigger unexpected queries during rendering.
- Mass assignment can update properties the user should not control (e.g., `IsAdmin`).
- Use dedicated ViewModels with only the fields the view needs.
- Map between entities and ViewModels in the controller or a mapping service.

---

#### Gotcha 3. `[FromBody]` on HTML form POST

**Answer:** Standard browser forms send `application/x-www-form-urlencoded` or `multipart/form-data`, not JSON. `[FromBody]` uses the JSON input formatter and leaves the model empty while the action runs with default values.

- Remove `[FromBody]` for conventional form POSTs and let model binding read form fields.
- Use `[FromBody]` only when the client sends JSON with the correct Content-Type.
- Silent binding failure is a common source of "my POST action receives null model" bugs.
- AJAX forms using `FormData` follow the same form binding rules as full-page forms.

---

#### Gotcha 4. Skipping `ModelState.IsValid` because of client validation

**Answer:** Client-side validation is bypassable — attackers POST directly without browser scripts. Server-side validation is mandatory before any persist, redirect, or side effect.

- Always gate POST actions with `if (!ModelState.IsValid) return View(model);` or equivalent.
- Client validation improves UX for legitimate users only.
- Remote validation and unobtrusive rules are not security boundaries.
- Treat missing server validation as a security defect regardless of client script presence.

---

#### Gotcha 5. `return View()` after successful POST

**Answer:** Returning the same view after a successful POST causes duplicate submission when the user refreshes the page — the browser resubmits the POST body.

- Use Post-Redirect-Get: `return RedirectToAction(nameof(Index))` after successful create/update.
- PRG separates the mutation (POST) from the display (GET).
- Flash success messages via TempData on the redirect target.
- AJAX partial POSTs have a similar concern — disable submit during request or use idempotent server logic.

---

#### Gotcha 6. `ModelState` after redirect

**Answer:** `ModelState` is request-scoped and does not survive `RedirectToAction`. Validation errors are lost unless rehydrated through TempData, a second validation pass on GET, or by redisplaying the form without redirect on failure only.

- Common pattern: redirect only on success; on validation failure return `View(model)` with errors inline.
- To survive redirect on failure, serialize errors to TempData or use PRG with a form-specific error cache.
- Do not assume errors automatically follow the user after redirect.
- AJAX partial forms avoid redirect and can return the form partial with `ModelState` errors directly.

---

#### Gotcha 7. TempData read twice in layout and view

**Answer:** TempData is consumed on first read by default. If the layout reads a flash message, the view sees nothing unless you use `Peek()` or `Keep()`.

- Use `TempData.Peek("Message")` in the layout to read without consuming.
- Or call `TempData.Keep("Message")` after the layout read so the view can read it too.
- Prefer a single consumption point — typically the layout or a dedicated partial, not both.
- Cookie-based TempData has size limits; avoid storing large payloads.

---

#### Gotcha 8. Missing `[Area]` attribute on area controllers

**Answer:** Controllers in `Areas/Admin/Controllers` without `[Area("Admin")]` are not discovered by the areas route and return 404 or match the wrong conventional route.

- Every area controller must declare `[Area("AreaName")]` matching its folder.
- Area routing is registered separately in `Program.cs` with the `{area:exists}` constraint.
- Without the attribute, MVC treats the controller as a root controller.
- Verify area registration order — specific area routes before catch-all default routes.

---

#### Gotcha 9. Link generation without `asp-area`

**Answer:** Tag Helpers default to the current area context when generating URLs. Links from a root view to an area controller need explicit `asp-area="Admin"` or they generate URLs without the area segment.

- From within an area, omitting `asp-area` keeps links inside the current area — sometimes incorrectly.
- Cross-area links require both `asp-area` and `asp-controller` (and `asp-action`).
- Wrong URLs produce 404 or hit unintended controllers.
- Same rule applies to `Url.Action` — pass `new { area = "Admin" }` in route values.

---

#### Gotcha 10. Checkbox `[Required]` on non-nullable `bool`

**Answer:** A missing unchecked checkbox posts nothing and model binding sets a non-nullable `bool` to `false`. `[Required]` never fails because `false` is a valid value — not null or empty.

- Use `bool?` with `[Required]` to require an explicit true selection for consent checkboxes.
- Or use the hidden-field pattern: hidden input `false` plus checkbox `true` so unchecked still posts `false` deliberately.
- Server-side, verify explicit consent with a dedicated check rather than relying on `[Required]` alone.
- This applies to both full-page forms and AJAX form posts.

---

#### Gotcha 11. Collection binding with gap indices

**Answer:** Deleting a row from a dynamic form leaving indices such as `Lines[0]` and `Lines[2]` breaks model binder alignment — index 1 is missing and subsequent items may bind incorrectly or truncate.

- Reindex client-side after row deletion so indices are contiguous starting at zero.
- Or implement a custom `IModelBinder` that tolerates non-contiguous indices.
- Partial views rendering collection editors must maintain consistent index naming.
- Test add/delete row scenarios explicitly in complex form POSTs.

---

#### Gotcha 12. `@Html.Raw` with user content

**Answer:** Default Razor encoding prevents XSS by HTML-encoding output. `@Html.Raw(Model.UserComment)` renders attacker-supplied script if the content is not sanitized server-side.

- Encode first, then apply safe formatting — never wrap raw user input in HTML.
- AJAX-loaded partials injected via `innerHTML` execute injected script the same as full pages.
- Prefer `@Model.UserComment` (auto-encoded) or sanitize with a trusted HTML sanitizer library.
- Content-Security-Policy limits blast radius but does not replace encoding.

---

#### Gotcha 13. AJAX POST without antiforgery token

**Answer:** Form tag helpers emit antiforgery tokens automatically, but `fetch` and jQuery AJAX must manually send `RequestVerificationToken` header or `__RequestVerificationToken` form field or POSTs fail with 400 antiforgery errors.

- Read the hidden field value from the page and include it on every mutating AJAX request.
- Same-origin requests send the antiforgery cookie automatically.
- `[AutoValidateAntiforgeryToken]` on the controller validates all unsafe methods — missing tokens fail before the action runs.
- Do not disable antiforgery on MVC cookie-auth endpoints to "fix" AJAX — add the token instead.

---

#### Gotcha 14. Injecting Hub into MVC controller

**Answer:** Hubs are not registered in DI for direct injection into controllers. Use `IHubContext<THub>` to broadcast messages from controllers, services, or background jobs.

- Injecting a concrete `Hub` fails activation or produces an instance without connection context.
- `IHubContext<T>` is a singleton proxy registered by `AddSignalR()`.
- Pair with Redis backplane or Azure SignalR for multi-instance fan-out.
- Keep hubs thin; business logic stays in scoped or transient services.

---

#### Gotcha 15. SignalR scale-out without backplane

**Answer:** Sticky sessions alone do not fan-out events across server instances. Multi-node deployments need a Redis backplane or Azure SignalR Service so messages sent from any instance reach clients on all instances.

- Controller on instance A calling `IHubContext.Clients.User(id).SendAsync` misses users connected to instance B without a backplane.
- Sticky sessions route connections but do not route cross-instance messages.
- Group membership and connection IDs are local to each instance.
- Register `AddStackExchangeRedis` or `AddAzureSignalR` when scaling beyond a single node.

---

## Gotchas — ASP.NET Core MVC (Interview Traps)

## Gotchas — ASP.NET Core MVC (Interview Traps)

---

## Scenario-Based Questions (Karat Format)

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

---

**Answer:**

**Answer:** The filter blocks a request thread with sync-over-async (`GetAwaiter().GetResult()`) inside `OnActionExecuting`, which defeats ASP.NET Core's async model on a globally registered filter that runs on every MVC action — convert to `IAsyncActionFilter` and await the repository call.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Async | `.GetAwaiter().GetResult()` on `GetBySlugAsync` | Thread-pool starvation under concurrent I/O |
| Design | Sync `IActionFilter` for database I/O | Wrong interface — blocks entire action pipeline stage |
| Correctness | `slug!` null-forgiving when route value missing | Possible `ArgumentNullException` or bad query |
| Scalability | Global filter on all MVC actions | Multiplies blocking calls across every controller hit |

**Fix (priority order):**

1. Implement `IAsyncActionFilter` and `await _tenants.GetBySlugAsync(slug, context.HttpContext.RequestAborted)` before `await next()`.
2. Guard missing `tenant` route value — set `context.Result = new NotFoundResult()` and return without calling the action.
3. Keep `AddScoped` + `AddService` registration (that part is correct).

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

**Production takeaway:** `IActionFilter` is fine for CPU-only checks; any filter that touches I/O on the hot path should be `IAsyncActionFilter` — especially global MVC filters.

---

---

#### Q2. (M) A controller has three filters on one action: a global `AuditFilter` (`Order = 100`), controller-level `[ValidateAntiForgeryToken]`, and action-level `[RequireRole("Admin")]`. Describe MVC filter **scope** (global → controller → action) and **order** within the same filter stage — which runs first within authorization filters, and can a lower-ordered authorization filter still short-circuit after a higher-ordered one passes?

---

**Answer:**

**Answer:** MVC runs filters by **stage** (authorization → resource → action → exception → result), then by **scope** (global → controller → action) within each stage, then by `IOrderedFilter.Order` ascending — authorization filters run before action filters entirely, so `[RequireRole]` executes before `AuditFilter` or antiforgery action filters; multiple authorization filters all run in order and any one can set `context.Result` to short-circuit later stages.

- **Authorization stage first:** `[RequireRole("Admin")]` (authorization filter) runs before resource/action stages — antiforgery validation is an **authorization filter** (`ValidateAntiForgeryTokenAuthorizationFilter`), so it also runs in the authorization stage, not in action filters.
- **Scope within a stage:** global authorization filters → controller authorization filters → action authorization filters; same pattern for action filters (`AuditFilter` is an action filter — runs later).
- **`Order` property:** lower numbers run first within the same scope and filter type; default `Order = 0` when unspecified.
- **Short-circuit:** setting `context.Result` in an authorization filter skips the action and remaining action filters, but authorization filters with higher `Order` in the same stage may still run depending on pipeline construction — safest pattern is one authoritative auth filter with early `Order`.
- **`AuditFilter` at Order 100:** runs after default-ordered action filters unless they specify higher order.

**Production takeaway:** Teams often assume `[ValidateAntiForgeryToken]` is an action filter — it is authorization-stage, which is why invalid tokens reject before model binding and action execution.

---

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

---

**Answer:**

**Answer:** `options.Filters.Add<AuditActionFilter>()` resolves the filter through `ObjectFactory` as an effectively singleton filter instance while `AppDbContext` is scoped — register the filter as scoped and use `AddService`, or inject `IDbContextFactory<AppDbContext>`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| DI lifetime | `Filters.Add<T>()` without service registration | Filter resolved with captive scoped dependency |
| Runtime | Scope validation enabled in Production | First request throws instead of silent bug |
| Design | Synchronous audit write before action | Failed action still logged if not moved to `finally`/post-`next()` |
| Maintainability | Global filter hidden in `Program.cs` | Easy to miss in code review vs `[ServiceFilter]` on controllers |

**Fix (priority order):**

1. `builder.Services.AddScoped<AuditActionFilter>()` and `options.Filters.AddService<AuditActionFilter>()`.
2. Prefer auditing **after** `await next()` so only successful actions log, or use a try/finally pattern.
3. Alternative: inject `IDbContextFactory<AppDbContext>` if the filter must remain singleton-compatible.

```csharp
builder.Services.AddScoped<AuditActionFilter>();
builder.Services.AddControllersWithViews(o => o.Filters.AddService<AuditActionFilter>());
```

**Production takeaway:** Global MVC filters participate in DI lifetimes exactly like `[ServiceFilter]` — `Add<T>()` without scoped registration is a classic Production-only failure.

---

---

#### Q4. (D) Product wants to block unauthenticated traffic before a custom middleware that opens an expensive read replica connection on every request. A developer proposes `[Authorize]` on every MVC controller instead of moving auth earlier in the pipeline. Compare authorization-filter short-circuit (`context.Result = …`) vs middleware short-circuit (no `_next`). Which seam fixes replica churn for MVC-only apps vs mixed MVC + minimal API hosts?

---

**Answer:**

**Answer:** `[Authorize]` short-circuits at the **authorization filter** stage — after routing, endpoint selection, and all middleware registered before the MVC endpoint, including the replica middleware — so it does **not** stop replica connection churn; place authentication/authorization middleware **before** the expensive middleware, or split branches with `MapWhen`.

- **Filter short-circuit:** sets `context.Result` (401/403) — skips action and later MVC filters, but **middleware already executed** on the way in, including replica setup.
- **Middleware short-circuit:** omit `_next` before expensive work — request never reaches replica middleware; works for all endpoints on that branch.
- **MVC-only app:** still wrong if replica middleware is global — `[Authorize]` on every controller duplicates policy and still pays middleware cost; fix pipeline order: `UseAuthentication` → `UseAuthorization` → replica middleware (only if authenticated) or conditional middleware checking `context.User`.
- **Mixed MVC + minimal API:** controller `[Authorize]` leaves minimal API routes unprotected unless they also call `RequireAuthorization()`; middleware + endpoint metadata is the unified seam.
- See [Middleware Pipeline Q8](../../05.%20ASP.NET%20Core/03.%20Middleware%20Pipeline/KARAT_INTERVIEW_ANSWERS.md): "Before routing / before binding" concerns belong in middleware, not filters.

**Production takeaway:** Authorization filters enforce **who may invoke an action**; they do not rewind middleware that already ran — pipeline order is the fix for replica churn.

---

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

---

**Answer:**

**Answer:** The filter marks the exception handled **and then rethrows**, so MVC may emit JSON while exception middleware also processes the same fault — remove the rethrow, pick one strategy (filter for MVC-specific views or centralized `IExceptionHandler` for APIs), and never log sensitive `Exception.Message` in Production responses.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `ExceptionHandled = true` then `throw` | Double handling — race between filter result and middleware |
| HTTP / API | Inconsistent error shape (HTML vs JSON) | Clients cannot parse errors reliably |
| Security | `Detail = context.Exception.Message` | Information disclosure in Production |
| Observability | Duplicate logging of same exception | Alert noise and inflated error rates |

**Fix (priority order):**

1. Remove `throw context.Exception` — if handled, return cleanly from `OnException`.
2. For JSON APIs, prefer `IExceptionHandler` / exception middleware only; remove the exception filter or limit it to HTML MVC controllers returning views.
3. Map known exceptions to safe `ProblemDetails` without inner exception messages; log full detail server-side only.

```csharp
public void OnException(ExceptionContext context)
{
    _logger.LogError(context.Exception, "Action {Action} failed", context.ActionDescriptor.DisplayName);
    context.ExceptionHandled = true;
    context.Result = new ObjectResult(new ProblemDetails { Status = 500, Title = "An error occurred." })
        { StatusCode = 500 };
}
```

**Production takeaway:** Exception filters are MVC/Razor-only hooks — pairing them with global exception middleware without a single owner produces split-brain error responses.

---

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

---

**Answer:**

**Answer:** Writing to the response in `OnResultExecuting` without assigning `context.Result` does not short-circuit result execution — the original `ViewResult` still renders; set `context.Result` to a short-circuiting result (e.g., `ContentResult`) or assign `context.Cancel = true` and replace the result.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| MVC pipeline | No `context.Result` assignment | View engine still executes — double output |
| HTTP | `WriteAsync` without clearing pipeline | Layout + cached HTML concatenated or corrupted |
| Async | Fire-and-forget `WriteAsync` not awaited | Incomplete writes under load |
| Caching | Path-only cache key | Personalized or varied responses collide |

**Fix (priority order):**

1. On cache hit: `context.Result = new ContentResult { Content = html, ContentType = "text/html" };` — optionally `context.Cancel = true` (when supported) to skip remaining result filters' execution of the original result.
2. Await response writes if using manual stream writing; prefer replacing `IActionResult`.
3. Include tenant/user/version in cache key; set cache headers explicitly.

```csharp
public void OnResultExecuting(ResultExecutingContext context)
{
    var key = BuildCacheKey(context.HttpContext);
    if (_cache.TryGetValue(key, out string? html))
        context.Result = new ContentResult { Content = html, ContentType = "text/html; charset=utf-8" };
}
```

**Production takeaway:** Result-filter short-circuit means **replace `context.Result`**, not write alongside it — the view engine only skips when the pipeline is told the result changed.

---

---

#### Q7. (P) You need an audit action filter on every MVC controller but not on `/api/*` minimal routes in the same app. Show the correct global registration pattern with `AddControllersWithViews`, filter `Order`, and how `[AllowAnonymous]` / `[IgnoreAntiforgeryToken]` interact with globally registered filters.

---

**Answer:**

**Answer:** Register the audit filter globally via `AddControllersWithViews(options => options.Filters.AddService<AuditActionFilter>())`, map minimal APIs separately without MVC filter discovery, and use `[AllowAnonymous]` only for authorization — not to skip unrelated global action filters unless the audit filter explicitly checks `[AllowAnonymous]` or endpoint metadata.

- **MVC global registration:**

```csharp
builder.Services.AddScoped<AuditActionFilter>();
builder.Services.AddControllersWithViews(o => o.Filters.AddService<AuditActionFilter>());
builder.Services.AddEndpointsApiExplorer();
// Minimal APIs: app.MapGroup("/api/orders").MapGet(...).RequireAuthorization();
// — no MVC action filters run on minimal endpoints
```

- **`Order`:** set `AuditActionFilter : IAsyncActionFilter, IOrderedFilter` with `Order => 1000` to run after validation filters (`Order = 0` or negative for idempotency).
- **`[AllowAnonymous]`:** bypasses **authorization** requirements only; a global **action** filter still runs unless it checks `context.ActionDescriptor.EndpointMetadata` for `IAllowAnonymous` and no-ops.
- **`[IgnoreAntiforgeryToken]`:** adds `IgnoreAntiforgeryTokenAttribute` metadata so `ValidateAntiForgeryTokenAuthorizationFilter` skips validation — unrelated to audit action filters.
- **Selective MVC exclusion:** prefer `[ServiceFilter]` on a base `Controller` class instead of global registration, or tag controllers with `[Audit]` and register via convention.

**Production takeaway:** Global MVC filters never attach to minimal API endpoints — that separation is automatic; use global filters for cross-controller MVC concerns, middleware for cross-host concerns.

---

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

---

**Answer:**

**Answer:** `TypeFilterAttribute` constructs the filter via DI **plus** constructor arguments you supply — dependencies like `IRateLimitStore` must still be registered in DI; `ServiceFilterAttribute` resolves the **entire** filter from DI using the parameterless or DI-filled constructor registered in `Program.cs`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| DI | `IRateLimitStore` not registered | `TypeFilter` cannot resolve store at activation |
| Design | `Arguments = { 60 }` only supplies `perMinute` | First ctor param still needs DI services |
| Confusion | Copying `[ServiceFilter]` pattern onto `[TypeFilter]` | Works only when filter fully registered with all ctor params |
| API contract | Same filter, two registration styles | Inconsistent per-controller rate limits |

**Fix (priority order):**

1. Register `builder.Services.AddScoped<IRateLimitStore, RateLimitStore>()`.
2. Keep `[TypeFilter(typeof(RateLimitFilter), Arguments = new object[] { 60 })]` — DI fills `store`, argument fills `perMinute`.
3. Or register `builder.Services.AddScoped<RateLimitFilter>()` and use `[ServiceFilter(typeof(RateLimitFilter))]` with options via `IOptions<RateLimitOptions>` instead of ctor int.

```csharp
builder.Services.AddScoped<IRateLimitStore, RateLimitStore>();
// TypeFilter: DI resolves IRateLimitStore, Arguments supplies perMinute=60
[TypeFilter(typeof(RateLimitFilter), Arguments = new object[] { 60 })]
public class ReportsController : Controller { }
```

**Production takeaway:** `TypeFilter` = **factory with extra ctor args**; `ServiceFilter` = **fully DI-managed instance** — both require dependencies in the service collection.

---

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

---

**Answer:**

**Answer:** `[AutoValidateAntiforgeryToken]` applies antiforgery validation to all unsafe HTTP methods on the controller — the JSON AJAX POST lacks `RequestVerificationToken` header/form field, so `ValidateAntiForgeryTokenAuthorizationFilter` short-circuits with 400 before the action runs; fix by sending the token from the page and validating header/form field, not by `[IgnoreAntiforgeryToken]` on the main endpoint.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | AJAX POST without antiforgery token | Correct 400 — CSRF protection working |
| API design | JSON body endpoint on MVC controller with auto antiforgery | API clients need token contract, not just JSON |
| Workaround smell | `[IgnoreAntiforgeryToken]` on production action | Opens CSRF unless replaced with other auth |
| Client | No token in `$.ajax` headers | Empty 400 body — client cannot distinguish validation vs CSRF |

**Fix (priority order):**

1. Render token in the page: `@Html.AntiForgeryToken()` or `<input name="__RequestVerificationToken" ...>` from `IAntiforgery.GetAndStoreTokens`.
2. Send header on AJAX POST:

```javascript
headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() }
```

3. For pure JSON APIs, move to `ControllerBase` + minimal API with bearer auth instead of antiforgery; keep antiforgery for cookie-based MVC forms.
4. Reserve `[IgnoreAntiforgeryToken]` for webhooks with alternate signatures — not cart mutations.

**Production takeaway:** Antiforgery is an **authorization filter** — AJAX MVC endpoints must participate in the same token contract as full-page forms; `[AutoValidateAntiforgeryToken]` just removes per-action attribute boilerplate.

---

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
            var key = $"{tenantId}:{context.RouteData.Values["categoryId"]}";
            _requestCache[key] = dto;
        }
    }
}
```

---

**Answer:**

**Answer:** A `static` dictionary is process-wide, not per-request — under load it leaks catalog data across tenants and requests; use `HttpContext.Items` only (no static field) for per-request memoization, or `IMemoryCache`/distributed cache with tenant-scoped keys and TTL for cross-request caching.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Design / state | `static` `_requestCache` | Cross-request, cross-tenant data leak |
| Concurrency | Unsynchronized `Dictionary<>` | Corrupted entries under parallel requests |
| Scale-out | In-memory static per pod | Inconsistent catalog between instances |
| Naming | Comment says "per request" but static persists | Misleading abstraction — security bug |

**Fix (priority order):**

1. Remove static field; store catalog only in `HttpContext.Items["Catalog"]` within `OnResourceExecuting`/`OnResourceExecuted` for the same request.
2. Inject scoped `ICatalogService` that memoizes on first call per request scope — resource filter optional.
3. For cross-request cache: `IMemoryCache` with key `tenantId:categoryId`, sliding expiration, and never share mutable DTOs without copy.

```csharp
public void OnResourceExecuting(ResourceExecutingContext context)
{
    if (context.HttpContext.Items.ContainsKey("Catalog")) return;
    // load once into Items — visible to action filters and action in same request
}
```

**Production takeaway:** Resource filters wrap the action pipeline — **`HttpContext.Items` is the per-request cache**; static fields are cross-request singletons regardless of filter stage.

---

---

#### Q11. (D) Your MVC app needs (a) per-action audit with route values and bound model metadata, (b) uniform 413 rejection for oversized uploads before model binding, and (c) a correlation ID on every response including static files. For each concern, choose middleware, authorization filter, action filter, resource filter, or result filter — and cite one reason the wrong layer fails.

---

**Answer:**

**Answer:** (a) action filter, (b) middleware/Kestrel limits, (c) middleware — filters lack visibility into static-file middleware paths and cannot run before model binding for upload size.

| Concern | Layer | Why |
|---|---|---|
| (a) Per-action audit with route + model metadata | **Action filter** (`IAsyncActionFilter`) | Runs with `ActionExecutingContext.ActionArguments` and `ActionDescriptor` — middleware lacks bound model |
| (b) 413 before model binding | **Middleware** + `KestrelServerLimits.MaxRequestBodySize` | Action/resource filters run after routing and binding start — body may already be buffered |
| (c) Correlation ID on all responses incl. static files | **Middleware** (early) | MVC filters never run for `UseStaticFiles` short-circuit paths |

- Wrong layer for (a): middleware logs URL only — no action name or bound DTO type without reflection hacks.
- Wrong layer for (b): action filter checking size — too late; large upload already consumed memory.
- Wrong layer for (c): result filter — static files bypass MVC result pipeline entirely.

**Production takeaway:** Match the seam to the earliest point the needed context exists — see [Middleware Pipeline Q8](../../05.%20ASP.NET%20Core/03.%20Middleware%20Pipeline/KARAT_INTERVIEW_ANSWERS.md) for the same judgment framework.

---

---

#### Q12. (M) Two action filters both implement `IOrderedFilter`: `IdempotencyFilter` (`Order = -1000`, short-circuits duplicate POSTs) and `ModelStateValidationFilter` (`Order = 0`, returns `BadRequestObjectResult` when invalid). Walk through `OnActionExecutionAsync` / `OnActionExecuting` invocation order for `[ServiceFilter(typeof(IdempotencyFilter)), ServiceFilter(typeof(ModelStateValidationFilter))]` on a POST action — which runs first, and why does order matter for idempotency?



**Answer:**

**Answer:** Lower `Order` runs first on the **executing** leg — `IdempotencyFilter` (`-1000`) runs before `ModelStateValidationFilter` (`0`); on the **executed** leg, order reverses (like middleware onion), so validation's `OnActionExecuted` runs before idempotency's.

- **Executing (before action):** sort by ascending `Order` → idempotency checks duplicate `Idempotency-Key` first; if duplicate, sets `context.Result` and skips validation and action entirely.
- **Why order matters:** if validation ran first (`Order 0` before `-1000`), invalid requests would consume idempotency keys or store partial state before rejection — wrong for POST retry semantics.
- **Executed (after action):** reverse order — validation post-action hooks run before idempotency commit/persist side effects.
- **Short-circuit:** first filter to set `context.Result` on executing leg prevents action invocation; later executing filters may still run depending on filter implementation — design idempotency filter to run earliest (`Order` large negative).
- **`IAsyncActionFilter` vs `IActionFilter`:** both participate in the same ordering; async filters interleave by order value, not by interface type.

```csharp
// Executing:  Idempotency (-1000) → ModelStateValidation (0) → action
// Executed:   ModelStateValidation (0) → Idempotency (-1000)
```

**Production takeaway:** Action filter `Order` controls **which guard runs first** — idempotency and auth-style guards belong at the lowest order values on the executing side.

---
