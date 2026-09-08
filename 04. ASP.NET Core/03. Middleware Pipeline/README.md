# Middleware Pipeline — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is middleware in ASP.NET Core?](#q1-what-is-middleware-in-aspnet-core)
2. [Q2. How does the middleware pipeline process an HTTP request?](#q2-how-does-the-middleware-pipeline-process-an-http-request)
3. [Q3. What is a `RequestDelegate`?](#q3-what-is-a-requestdelegate)
4. [Q4. What does calling `_next(context)` do in custom middleware?](#q4-what-does-calling-_nextcontext-do-in-custom-middleware)
5. [Q5. What happens when middleware returns without calling `_next`?](#q5-what-happens-when-middleware-returns-without-calling-_next)
6. [Q6. Why does middleware order matter?](#q6-why-does-middleware-order-matter)
7. [Q7. What is the recommended order for routing, authentication, and authorization middleware?](#q7-what-is-the-recommended-order-for-routing-authentication-and-authorization-middleware)
8. [Q8. What is the difference between `Use`, `Run`, and `Map`?](#q8-what-is-the-difference-between-use-run-and-map)
9. [Q9. What does `Map("/path", ...)` do to the pipeline?](#q9-what-does-mappath-do-to-the-pipeline)
10. [Q10. What is `MapWhen`, and when would you use it?](#q10-what-is-mapwhen-and-when-would-you-use-it)
11. [Q11. How do you register custom middleware in `Program.cs`?](#q11-how-do-you-register-custom-middleware-in-programcs)
12. [Q12. What is the difference between middleware and MVC filters?](#q12-what-is-the-difference-between-middleware-and-mvc-filters)
13. [Q13. Where should exception-handling middleware be placed in the pipeline?](#q13-where-should-exception-handling-middleware-be-placed-in-the-pipeline)
14. [Q14. What is `UseForwardedHeaders`, and why must it run early?](#q14-what-is-useforwardedheaders-and-why-must-it-run-early)
15. [Q15. What is built-in rate limiting middleware, and where does it belong in the pipeline?](#q15-what-is-built-in-rate-limiting-middleware-and-where-does-it-belong-in-the-pipeline)
16. [Q16. What does "short-circuiting the pipeline" mean?](#q16-what-does-short-circuiting-the-pipeline-mean)
17. [Q17. How does middleware differ from an endpoint filter?](#q17-how-does-middleware-differ-from-an-endpoint-filter)
18. [Q18. Can middleware access DI-registered services? How?](#q18-can-middleware-access-di-registered-services-how)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What is middleware in ASP.NET Core?

**Concepts**
- `RequestDelegate` chain — inbound and outbound execution phases
- Convention-based middleware class with `InvokeAsync` and `_next`
- Registration order in `Program.cs` determines execution order
- Short-circuit vs pass-through middleware behavior

**Answer**

Middleware in ASP.NET Core is a component that receives an `HttpContext` and chooses whether to pass control forward with `await _next(context)` or terminate the request by writing a response and returning. The pipeline is powerful because code placed before `await _next` runs on the inbound request while code after it runs on the outbound response path, so a single component can start a timer before calling next and record elapsed time after. Built-in middleware handles exception wrapping, HTTPS redirection, static files, routing, authentication, and authorization — all registered in `Program.cs` in the order they execute, which means the sequence matters as much as the components themselves.

---

## Q2. How does the middleware pipeline process an HTTP request?

**Concepts**
- Linked list of `RequestDelegate` functions built at startup
- Inbound execution order and reverse outbound execution order
- Endpoint middleware as innermost logic
- Async `Task` return to avoid thread pool blocking

**Answer**

Kestrel hands each incoming request to the first `RequestDelegate` in the pipeline, which is a linked list built when the application starts. Each component runs its pre-processing logic, then calls `await _next(context)` to invoke the next delegate in the chain — which does the same — until the innermost endpoint handler executes. On the way back, each component resumes after its `await _next` call, so response headers and body can be examined or modified. Because the pipeline runs on the async thread pool, each middleware must return `Task` from `InvokeAsync` and await I/O calls rather than blocking, since blocking under concurrent load causes thread pool starvation.

---

## Q3. What is a `RequestDelegate`?

**Concepts**
- `RequestDelegate` type — `Task RequestDelegate(HttpContext context)`
- Each middleware holds a reference to the next delegate in the chain
- Terminal delegate after all `Use` calls — endpoint or 404
- `Use`, `Run`, and `Map` all compose `RequestDelegate` instances

**Answer**

A `RequestDelegate` is the delegate type `Task RequestDelegate(HttpContext context)` that represents both the signature of middleware and the pipeline entry point. Each middleware in the chain holds a reference to the next `RequestDelegate` as `_next`, so calling it passes the request forward. The terminal delegate after all `Use` calls either executes the matched endpoint or returns 404 when nothing matched. Understanding that `Use`, `Run`, and `Map` are just ways to compose `RequestDelegate` instances explains why pipeline behavior is strictly tied to registration order.

---

## Q4. What does calling `_next(context)` do in custom middleware?

**Concepts**
- `await _next(context)` — passing control to the next pipeline stage
- Code after `await _next` runs on the response phase
- Short-circuit by omitting `_next` call
- Exception handling across `_next` call boundaries

**Answer**

Calling `await _next(context)` passes the current `HttpContext` to the next middleware in the pipeline and suspends the current component until all inner middleware and the endpoint have finished. Code written after the `await` executes on the way back out — the response phase — so I can inspect or modify the response body or headers there. Omitting the call at all is intentional when the middleware wants to short-circuit, such as returning 401 when an API key is missing. Exceptions thrown by inner middleware propagate back through `_next` and can be caught in a `try/catch` around the call, which is exactly how exception-handler middleware wraps the whole pipeline.

---

## Q5. What happens when middleware returns without calling `_next`?

**Concepts**
- Short-circuit pattern — no subsequent middleware or endpoint runs
- Middleware responsibility to set status code and response body
- Intentional vs accidental short-circuit
- Impact on auth and business logic downstream

**Answer**

When middleware returns without calling `_next`, the pipeline short-circuits and no later components — including authorization, controllers, or other middleware — run for that request. The short-circuiting middleware is fully responsible for writing a meaningful response: if it sets a 401 status but writes no body, clients receive an empty response with no error detail. This pattern is intentional for authentication failures, rate limit rejections, or IP blocklists. Accidental short-circuits — where a middleware forgets to call `_next` under certain conditions — produce mysterious 404 or empty responses for routes that would normally work, which is one of the harder pipeline bugs to diagnose.

---

## Q6. Why does middleware order matter?

**Concepts**
- Inbound execution order and reverse outbound order
- Exception handler as outermost wrapper
- `UseRouting` before auth — endpoint metadata availability
- `UseStaticFiles` position controlling file vs controller precedence

**Answer**

Middleware runs in registration order on the inbound path and in reverse on the outbound path, so each component only sees requests that prior middleware actually forwarded. Exception-handling middleware must be registered early so it wraps all inner components — placing it after routing means routing exceptions escape the handler. `UseRouting` must come before `UseAuthentication` and `UseAuthorization` because auth middleware needs the endpoint metadata that routing selection produces — without it, `[Authorize]` attributes cannot be evaluated correctly. `UseStaticFiles` positioned before routing lets the file server intercept file requests before MVC ever sees them, while positioning it after gives controllers the opportunity to handle paths that happen to match filenames.

---

## Q7. What is the recommended order for routing, authentication, and authorization middleware?

**Concepts**
- Recommended order: exception handling → forwarded headers → routing → authentication → authorization → endpoints
- Authentication populating `HttpContext.User` before authorization
- Endpoint metadata availability for authorization policy evaluation
- CORS placement relative to auth

**Answer**

The recommended pipeline order is exception handling first (outermost wrapper), then forwarded headers, then routing, then authentication, then authorization, and finally endpoint mapping with `MapControllers` or `MapGet`. Authentication must come before authorization because auth populates `HttpContext.User` from cookies or JWT tokens — authorization needs that identity to evaluate policies. Both must come after routing so the matched endpoint's `[Authorize]` attributes or `.RequireAuthorization()` policy metadata is available when auth evaluates them. CORS middleware (`UseCors`) goes after routing and before auth since preflight requests should not require authentication. In ASP.NET Core 6+ minimal hosting, implicit routing is added when you call `Map*` methods, but if you need explicit `UseRouting()` it must precede auth.

---

## Q8. What is the difference between `Use`, `Run`, and `Map`?

**Concepts**
- `Use` — middleware with access to `_next`, can call forward or short-circuit
- `Run` — terminal middleware, never calls next
- `Map` — pipeline branch on path prefix, builds independent sub-pipeline
- `MapWhen` — predicate-based branching beyond path prefix

**Answer**

`Use` adds inline middleware that receives the `next` delegate and can choose to call it or short-circuit — it is the general-purpose composition primitive. `Run` adds a terminal component that never calls next and replaces the rest of the pipeline for that branch, so anything registered after a `Run` call is unreachable. `Map` branches the pipeline by path prefix and builds a completely separate sub-pipeline for matching requests — requests that don't match the prefix skip the branch entirely and continue on the main pipeline. `MapWhen` extends this to arbitrary predicates on `HttpContext` for cases where branching logic depends on headers or query strings rather than path alone.

---

## Q9. What does `Map("/path", ...)` do to the pipeline?

**Concepts**
- Path-prefix branching creating an independent sub-pipeline
- Non-matching requests bypassing the branch entirely
- Branch isolation — separate middleware chains per branch
- Use case: different auth or rate limiting per path segment

**Answer**

`Map` creates a pipeline branch where only requests whose path starts with the given prefix enter the sub-pipeline defined in the delegate. Requests that don't match bypass the branch and continue on the main pipeline, which means the branch's middleware and endpoints are completely invisible to non-matching requests. Each branch builds its own independent middleware chain linked at the map point, so I can give the `/admin` branch its own authentication scheme and rate limiting policy without affecting the `/api` branch. The matching is prefix-based by default — `/api` matches `/api/users` and `/api` but not `/apiv2`.

---

## Q10. What is `MapWhen`, and when would you use it?

**Concepts**
- Predicate-based branching on `HttpContext` beyond path matching
- `UseWhen` — conditional middleware on main pipeline without full branch
- Header, query string, or content-type driven branching
- Combining with `Map` for layered routing logic

**Answer**

`MapWhen` branches the pipeline based on any condition expressible against `HttpContext`, not just path prefix. I use it when I want to apply a middleware chain only for requests carrying a specific header — for example, routing debug-mode requests through additional logging middleware when `context.Request.Headers.ContainsKey("X-Debug")` is true. For conditions that should apply middleware without creating a separate terminal branch, `UseWhen` is the lighter alternative that puts the conditional middleware inline on the main pipeline and still calls `_next` for the main flow after. I prefer `Map` for path-based splits because the intent is clearer, and reach for `MapWhen` for behavioral branching that doesn't fit URL structure.

---

## Q11. How do you register custom middleware in `Program.cs`?

**Concepts**
- `app.UseMiddleware<T>()` — class-based registration with DI
- Convention: constructor accepting `RequestDelegate`, `InvokeAsync` method
- Inline `app.Use(async (context, next) => { })` for simple logic
- Extension method wrapping `UseMiddleware` for readable registration

**Answer**

For class-based middleware I call `app.UseMiddleware<MyMiddleware>()`, where `MyMiddleware` has a constructor that accepts `RequestDelegate next` plus any singleton DI services, and an `InvokeAsync(HttpContext context)` method that can receive per-request scoped services as additional parameters. For simple cross-cutting logic I use the inline form `app.Use(async (context, next) => { /* before */ await next(context); /* after */ })`. In larger applications I wrap `UseMiddleware<T>` in a static extension method like `app.UseApiKeyValidation()` so `Program.cs` reads in terms of named pipeline stages rather than type names. Registration order relative to other `Use*` calls is what sets execution order.

---

## Q12. What is the difference between middleware and MVC filters?

**Concepts**
- Middleware — pipeline-wide, before MVC action selection
- MVC filters — scoped to MVC pipeline, after routing selected an action
- Minimal API endpoint filters vs MVC action filters
- Exception middleware vs exception filters — coverage difference

**Answer**

Middleware runs for every request that passes through it in the pipeline before MVC has even selected an action or validated a model. MVC filters run only after routing has chosen a controller action, so they execute within the MVC framework context and have access to action arguments, `ActionContext`, and model state. Exception middleware catches unhandled exceptions from any downstream middleware or endpoint, while exception filters only see exceptions thrown during MVC action execution — exceptions from middleware outside MVC escape exception filters entirely. For concerns that must apply universally regardless of whether the request hits MVC — logging, correlation IDs, auth — I use middleware. For concerns tied to MVC mechanics — action validation, result transformation — I use filters. Minimal APIs use endpoint filters rather than MVC action filters; middleware still applies to minimal API requests.

---

## Q13. Where should exception-handling middleware be placed in the pipeline?

**Concepts**
- Exception handler as outermost wrapper to catch all downstream exceptions
- `UseDeveloperExceptionPage` — Development only
- `UseExceptionHandler` — production error handler with ProblemDetails
- Response must not have started when exception middleware writes error body

**Answer**

Exception-handling middleware should be one of the first registrations — near the top of `Program.cs` — so it wraps all subsequent middleware and endpoint execution and can convert unhandled exceptions into proper error responses. Placing it after routing or controllers would leave exceptions from those components uncaught by the handler. In Development I use `UseDeveloperExceptionPage()`, which shows full stack traces; in production I use `UseExceptionHandler()` combined with `AddExceptionHandler<GlobalExceptionHandler>()` (ASP.NET Core 8) to emit RFC 7807 `ProblemDetails` JSON. The critical constraint is that exception middleware must write its error response before any headers are sent — once the response stream starts, status and headers cannot change, so exceptions caught after a partially-written response can only abort the connection rather than substitute a clean error body.

---

## Q14. What is `UseForwardedHeaders`, and why must it run early?

**Concepts**
- `X-Forwarded-For`, `X-Forwarded-Proto` header population from reverse proxy
- `Request.Scheme` and `RemoteIpAddress` correction before scheme-aware middleware
- `ForwardedHeadersOptions` trusted proxy network configuration
- HTTPS redirect loops without correct scheme

**Answer**

When ASP.NET Core sits behind a reverse proxy that terminates TLS, Kestrel sees plain HTTP requests and sets `Request.Scheme` to `http` and `Connection.RemoteIpAddress` to the proxy's internal IP. `UseForwardedHeaders` reads `X-Forwarded-Proto` and `X-Forwarded-For` headers that the proxy attaches and rewrites those values on `HttpContext` so downstream middleware sees the real scheme and client IP. It must run before HTTPS redirection middleware, because without it `Request.Scheme` never becomes `https` and the redirect loops, and before any rate limiting that partitions by client IP. I configure `ForwardedHeadersOptions.KnownNetworks` or `KnownProxies` to list only my actual proxy infrastructure, since trusting all headers from all sources would let attackers spoof their IP by crafting a `X-Forwarded-For` header.

---

## Q15. What is built-in rate limiting middleware, and where does it belong in the pipeline?

**Concepts**
- `AddRateLimiter` service registration and policy definition
- Fixed-window, sliding-window, token-bucket, and concurrency limiter policies
- `UseRateLimiter` placement — after routing, optionally after authentication
- 429 status with `Retry-After` header on rejection

**Answer**

ASP.NET Core 7 introduced `AddRateLimiter` for service registration and `UseRateLimiter` for pipeline registration, supporting fixed-window, sliding-window, token-bucket, and concurrency limiter policies. I register policies in `builder.Services.AddRateLimiter(options => ...)` and attach them to endpoints with `[EnableRateLimiting("policy")]` on controllers or `.RequireRateLimiting("policy")` on minimal API routes. The middleware belongs after `UseRouting` so endpoint metadata is available — this lets policies use route-specific limits — and after `UseAuthentication` when limits need to partition by authenticated user identity. Rejected requests receive 429 Too Many Requests; I configure `OnRejected` to set `Retry-After` and log the partition key for capacity planning.

---

## Q16. What does "short-circuiting the pipeline" mean?

**Concepts**
- Short-circuit — omitting `_next` call to prevent downstream execution
- Intentional short-circuit: auth failure, rate limit, cached response
- Accidental short-circuit: middleware forgetting to call `_next`
- Cannot call `_next` after writing response — would cause double execution

**Answer**

Short-circuiting means a middleware writes a response and returns without calling `_next`, so no later components execute for that request. Static file middleware short-circuits when it finds the requested file and serves it directly. Authentication middleware short-circuits with 401 when credentials are invalid or absent. The key rule is that once I write any response data, I must not then call `_next` — doing so passes a partially-written response to inner middleware and causes "headers already sent" errors. Accidental short-circuiting — where middleware omits `_next` for code paths that should proceed — produces hard-to-diagnose 404 or empty responses for routes that exist correctly further down the pipeline.

---

## Q17. How does middleware differ from an endpoint filter?

**Concepts**
- Middleware — pipeline-wide, before endpoint invocation is finalized
- Endpoint filters — per-route, run around specific Minimal API handler
- `IEndpointFilter` for Minimal APIs; action filters for MVC controllers
- Cross-cutting concerns vs handler-specific validation

**Answer**

Middleware operates at the HTTP pipeline level before the routing system has fully matched and invoked a specific endpoint handler, so it applies to every request that reaches it regardless of which endpoint eventually handles it. Endpoint filters implement `IEndpointFilter` and run as a chain immediately around a specific Minimal API route or route group handler — after routing, after model binding, right before and after the handler executes. This gives endpoint filters access to typed route handler parameters, which middleware cannot easily get. I use middleware for cross-cutting concerns that apply broadly — correlation IDs, global logging, security headers — and endpoint filters for per-handler validation, auditing, or response transformation that needs the route's typed context.

---

## Q18. Can middleware access DI-registered services? How?

**Concepts**
- Constructor injection for singleton services in convention-based middleware
- `HttpContext.RequestServices` for per-request scoped service resolution
- Captive dependency — scoped service in singleton middleware constructor
- `UseMiddleware<T>()` activating `T` from DI

**Answer**

Middleware registered with `UseMiddleware<T>()` can inject singleton and transient services through its constructor because the middleware class itself is typically activated once per application lifetime. Scoped services, however, cannot be constructor-injected into middleware because the middleware instance lives longer than any request scope — injecting `DbContext` this way creates a captive dependency that shares state across requests. Instead, I declare scoped service parameters directly on `InvokeAsync(HttpContext context, IMyService service)` — ASP.NET Core resolves them from `context.RequestServices` for each invocation. Alternatively, I call `context.RequestServices.GetRequiredService<T>()` explicitly inside `InvokeAsync` when I need fine-grained control over resolution timing.

---

## Gotchas — Middleware Pipeline (Interview Traps)

---

#### Gotcha 1. Not calling `next()` short-circuits the pipeline — response may never be written

**Concepts**
- `next()` delegate invoking the remainder of the pipeline
- Short-circuiting by not calling `next()` — intentional vs accidental
- Response committed check before writing — `HttpResponse.HasStarted`
- Terminal middleware with `app.Run()` not calling `next` by design

**Answer**

Every middleware registered with `app.Use()` receives a `next` delegate representing the rest of the pipeline. Not calling `next()` short-circuits the pipeline and prevents any subsequent middleware from running, including endpoint routing and response generation. When this happens accidentally — a missing `await next(context)` in an `if` branch — the request hangs or returns an empty response with no status code. Always verify that all code paths through a middleware either call `next()` or write a complete response, including appropriate status code and Content-Type headers. `app.Run()` is intentionally terminal and should be used when a middleware always produces a response without delegating.

---

#### Gotcha 2. Exception handling middleware must be registered first to catch all downstream exceptions

**Concepts**
- Middleware pipeline as nested delegate chain — outermost registered first
- `UseExceptionHandler` or `UseDeveloperExceptionPage` as the outermost wrapper
- Exceptions from routing, auth, and endpoints caught only if handler runs before them
- Placement order determines which exceptions are observed

**Answer**

The middleware pipeline is a nested chain of delegates — the first registered middleware is the outermost wrapper and runs first on request and last on response. Exception handling middleware catches exceptions thrown by all downstream delegates, but only if it is registered before them. An `UseExceptionHandler` registered after `UseRouting` cannot catch routing or authentication exceptions. The canonical `Program.cs` order is: `UseExceptionHandler` / `UseDeveloperExceptionPage` first, then forwarded headers, then routing, then auth, then endpoints. If your exception handler registers after any middleware that can throw, those exceptions propagate unhandled to the host.

---

#### Gotcha 3. Writing to the response after headers are sent throws `InvalidOperationException`

**Concepts**
- HTTP response headers sent with the first byte of body
- `HttpResponse.HasStarted` flag indicating committed response
- `SetStatusCode` or `Append` headers throwing after body starts
- Exception handling middleware checking `HasStarted` before handling

**Answer**

Once the HTTP response body starts being written to the client, the response headers have already been sent and cannot be modified. Calling `context.Response.StatusCode = 500` or `context.Response.Headers.Append(...)` after body bytes have been flushed throws `InvalidOperationException: Headers are read-only, response has already started`. Exception handling middleware must check `context.Response.HasStarted` before attempting to write an error response — if `HasStarted` is true, the error can only be logged and the connection should be aborted rather than trying to write a new response. This is why exception middleware must be the first registered: it needs to intercept exceptions before any body bytes are sent.

---

#### Gotcha 4. `app.Use` vs `app.Run` — `Run` terminates, `Use` continues

**Concepts**
- `app.Use(next => ...)` — receives and may invoke `next` delegate
- `app.Run(ctx => ...)` — terminal middleware, never calls `next`
- Registering middleware after `app.Run` — dead code
- `app.Map()` for conditional pipeline branching

**Answer**

`app.Run` registers a terminal middleware that never calls the `next` delegate, so any middleware registered after `app.Run` is dead code — it never executes. `app.Use` registers middleware that receives a `next` delegate and decides whether to call it. This is an easy mistake to introduce when middleware is reorganized in `Program.cs`: moving a `Run`-terminated branch before other middleware silently swallows all subsequent registrations. When adding a catch-all or fallback handler, verify it is the last registration in the pipeline, and use `app.Map` for conditional branching rather than early `Run` calls.

---

#### Gotcha 5. Branching with `app.Map` creates an isolated sub-pipeline that does not continue to the main pipeline

**Concepts**
- `app.Map("/prefix", branch => {...})` — separate pipeline for matched prefix
- Requests matched by `Map` do not flow to main pipeline after the branch
- `app.MapWhen` for predicate-based branching
- `app.UseWhen` for conditional middleware that still continues to main pipeline

**Answer**

`app.Map("/health", ...)` creates a branch pipeline that handles all requests matching `/health` and then stops — the request never flows back to the main pipeline after the branch completes. This is correct for dedicated health check endpoints, but a common mistake is using `app.Map` when `app.UseWhen` was intended: `app.UseWhen` applies middleware conditionally and then returns to the main pipeline. If exception handling or authentication middleware is registered on the main pipeline but a branch is created with `Map` before those registrations, the branch runs without them. Use `Map` for completely separate handling and `UseWhen` for conditional augmentation of the main flow.

---

#### Gotcha 6. `IMiddleware` vs conventional middleware — lifetime differences

**Concepts**
- Conventional middleware — instantiated once at startup as a singleton
- `IMiddleware` — resolved from DI per request, enabling scoped lifetime
- Constructor-injected services in conventional middleware are singleton-scoped
- `UseMiddleware<T>()` for both types

**Answer**

Conventional middleware created with `app.Use(async (ctx, next) => {...})` or using `UseMiddleware<MyMiddleware>()` with a class that takes `RequestDelegate` in its constructor is instantiated once at startup and behaves as a singleton. Any service injected into its constructor is resolved once and kept alive for the application lifetime, which makes injecting scoped services like `DbContext` a captive dependency bug. `IMiddleware` solves this: implement `IMiddleware`, register it with `AddScoped<MyMiddleware>()`, and call `app.UseMiddleware<MyMiddleware>()`. The DI container resolves a new instance per request, allowing scoped service injection. The distinction is easy to miss because both use the same `UseMiddleware<T>()` call.

---

#### Gotcha 7. Accessing `HttpContext` after the request completes causes null reference or stale data

**Concepts**
- `HttpContext` valid only during the active request's lifetime
- Background `Task.Run` capturing `HttpContext` outliving the request
- `IHttpContextAccessor.HttpContext` returning `null` after request ends
- Copying needed values before fire-and-forget background work starts

**Answer**

`HttpContext` is valid only during the scope of the active HTTP request. When middleware or action code starts a background task with `Task.Run` and captures `HttpContext`, the context may be recycled or nulled by the framework before the background task accesses it — especially `Request.Body`, which is disposed when the request scope ends. If a background operation needs data from the request, extract and copy the required values — user ID, request headers, body content — into local variables before starting the background work. Never pass `HttpContext` itself to a background thread or fire-and-forget task.

---

#### Gotcha 8. `UseStaticFiles` must be registered before `UseRouting` to prevent static paths matching as API routes

**Concepts**
- `UseStaticFiles` serving files before routing inspects the path
- Route template potentially matching static file paths as API endpoints
- Static file serving short-circuiting the pipeline without going through auth
- Order: `UseStaticFiles` → `UseRouting` → `UseAuthentication` → `UseAuthorization`

**Answer**

When `UseStaticFiles` is registered before `UseRouting`, requests for static files are served directly and the pipeline short-circuits before routing runs — which is the desired behavior. If `UseRouting` runs first, the path is evaluated as a potential API route, and if a route template matches a static file path, the route wins over the file. More critically, `UseStaticFiles` has no authentication gate — it serves files to unauthenticated clients. If authentication middleware must protect static files, a custom `StaticFileMiddleware` with an `OnPrepareResponse` callback or a different serving approach is needed. For most applications, `UseStaticFiles` before `UseRouting` is correct.

---

#### Gotcha 9. Async middleware must `await` all async calls — synchronous blocking causes thread pool starvation

**Concepts**
- `.Result` or `.Wait()` blocking async calls inside middleware
- Thread pool starvation under load from blocked threads
- `async Task` middleware delegate returning `Task`, not `void`
- `await next(context)` vs `next(context).Wait()` — always the former

**Answer**

Middleware that calls `next(context).Wait()` or `someAsyncService.DoWorkAsync().Result` blocks the thread synchronously, preventing the thread pool thread from handling other requests while the async operation completes. Under load this causes thread pool starvation where the thread pool grows slowly while request latency climbs. All middleware should be declared with `async Task` return types and `await` every async operation, including `await next(context)`. This ensures threads are returned to the pool while I/O awaits. Synchronous blocking with `.Result` or `.Wait()` also risks deadlocks in certain synchronization contexts.

---

#### Gotcha 10. Middleware registered after `UseEndpoints` or `MapControllers` never runs for matched routes

**Concepts**
- Endpoint execution short-circuiting the pipeline after the matched handler runs
- Response pipeline running in reverse — after-logic in middleware runs post-endpoint
- Middleware after endpoint mapping only reached by unmatched requests
- Logging or correlation middleware must be registered before endpoint mapping

**Answer**

Once an HTTP request is matched to an endpoint by `MapControllers()` or `MapGet()`, the endpoint handler executes and produces a response, short-circuiting the remaining "request" direction of the pipeline. Middleware registered after the endpoint mapping calls is never invoked for matched requests — only for unmatched 404 paths. This matters for cross-cutting concerns like correlation ID injection, audit logging, or response compression: these must be registered before `MapControllers()`. The response pipeline does run in reverse through already-executed middleware after the endpoint completes, which is how response compression and security headers work when placed before the endpoint mapping.

---

## Scenario-Based Questions (Karat Format)

---

## Q1. (P) How do you limit incoming client requests within a time window using ASP.NET Core's built-in rate limiting middleware? What must you register in services, how do you attach policies to endpoints, and where should `UseRateLimiter()` sit relative to routing and authentication?

**Concepts**
- `AddRateLimiter` service registration with policy definitions
- Fixed-window, sliding-window, token-bucket, concurrency limiter policy types
- `UseRateLimiter` placement — after routing, optionally after authentication
- Partition key design: IP for anonymous, user identity for authenticated limits

**Answer**

I register rate limiting policies in the service collection with `builder.Services.AddRateLimiter(options => options.AddFixedWindowLimiter("api", w => { w.Window = TimeSpan.FromMinutes(1); w.PermitLimit = 60; }))`, choosing the policy type based on the traffic shape — fixed-window for simple caps, token-bucket for burst-tolerant limits, and concurrency limiter for protecting slow operations. I call `app.UseRateLimiter()` after `UseRouting()` so endpoint metadata is available for per-endpoint policy selection, and after `UseAuthentication()` when limits need to partition by authenticated user identity rather than IP alone. I attach policies to endpoints with `.RequireRateLimiting("api")` on minimal API routes or `[EnableRateLimiting("api")]` on controllers. Rejected requests receive 429 Too Many Requests; I configure `OnRejected` to emit a `Retry-After` header and log the partition key.

```csharp
builder.Services.AddRateLimiter(o =>
{
    o.AddFixedWindowLimiter("api", w => { w.Window = TimeSpan.FromMinutes(1); w.PermitLimit = 60; });
});
app.UseRouting();
app.UseAuthentication();
app.UseRateLimiter();
app.MapControllers().RequireRateLimiting("api");
```

---

## Q2. (R) Review this API key middleware. What is wrong with behavior, headers, and security — and what happens to downstream middleware when the key is missing?

```csharp
public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    public ApiKeyMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.ContainsKey("X-Api-Key"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }
        await _next(context);
    }
}
```

*(Registration order is correct; focus on response completion, validation, and client contract.)*

**Concepts**
- `ContainsKey` only — empty header value passes validation
- Short-circuit without response body — incomplete 401
- Missing key comparison against configured secret
- Downstream middleware skipped correctly when short-circuiting

**Answer**

The middleware correctly short-circuits when the header is absent — `_next` is not called, so downstream middleware and endpoints never run, which is the right behavior for an access control component. The problems are in what it writes and what it validates. Checking only `ContainsKey` means a request with `X-Api-Key: ` (empty value) passes through, since the header exists. The 401 response has no body and no `Content-Type`, so API clients receive an empty response with no error description. Most critically, there is no comparison of the header value against a configured secret — any non-empty key would pass. I'd replace the key check with `TryGetValue` combined with a constant-time comparison against an `IOptions<ApiKeyOptions>` configured value, and I'd write a `ProblemDetails` JSON body with `Content-Type: application/problem+json` before returning:

```csharp
if (!context.Request.Headers.TryGetValue("X-Api-Key", out var key)
    || !_validator.IsValid(key))
{
    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
    context.Response.ContentType = "application/problem+json";
    await context.Response.WriteAsJsonAsync(new ProblemDetails
    {
        Status = 401, Title = "Unauthorized", Detail = "Invalid or missing API key."
    });
    return;
}
await _next(context);
```

---

## Q3. (M) An authentication middleware sets `Response.StatusCode = 401` and returns without calling `_next`. Trace one request through the pipeline — which components still run, which are skipped, and why is omitting `_next` intentional rather than a bug?

**Concepts**
- Pipeline execution up to the short-circuit point
- Components after auth — skipped entirely
- Response path through earlier middleware's post-`_next` code
- Fail-closed security — unauthenticated traffic must not reach handlers

**Answer**

Middleware registered before authentication in `Program.cs` — forwarded headers, request logging, exception handling — already ran on the inbound path before auth inspected the token. When auth sets 401 and returns without calling `_next`, everything registered after it — authorization middleware, routing, controllers, endpoint handlers, and MVC filters — is completely skipped. The response travels back up through the server and out to the client. Earlier middleware that had code after `await _next(context)` does execute its post-`_next` logic on the response path — the logging middleware can record the 401 status, for example. Omitting `_next` is intentional rather than a bug because the pipeline is designed fail-closed: unauthenticated traffic must not reach any handler that assumes an authenticated identity is present. The middleware is responsible for writing a complete response — status code, appropriate headers like `WWW-Authenticate`, and a body — before returning.

---

## Q4. (R) After deploy behind nginx, HTTPS redirects loop and `[Authorize]` sees anonymous users. Review middleware order — what is wrong?

```csharp
var app = builder.Build();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseForwardedHeaders();
app.MapControllers();
app.Run();
```

**Concepts**
- `UseForwardedHeaders` placement — must run before scheme-aware middleware
- `Request.Scheme` stuck at `http` causing redirect loops
- Cookie and JWT validation failing on wrong scheme
- Trusted proxy network configuration

**Answer**

The bug is that `UseForwardedHeaders()` runs after HTTPS redirection and authentication, so when a request arrives from nginx with `X-Forwarded-Proto: https`, the scheme is never corrected before the redirect check. HTTPS redirection middleware sees `Request.Scheme = http`, issues a 301 to the HTTPS URL, nginx receives that and forwards the HTTPS request again as HTTP — loop. Authentication middleware also sees the wrong scheme, which breaks cookie `Secure` validation and redirect URI verification in OAuth flows, so auth tokens can't be matched against the real request origin. I move `UseForwardedHeaders()` to be the first middleware call, configure `ForwardedHeadersOptions.KnownNetworks` to trust only the nginx proxy subnet, and then place redirection and auth after it:

```csharp
app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

---

## Q5. (R) Review `Run` vs `Map` branching. `/admin` returns 404 and static files leak on `/admin/config.json`. Explain pipeline behavior and fix order.

```csharp
app.UseStaticFiles();
app.Map("/admin", adminApp =>
{
    adminApp.UseRouting();
    adminApp.MapControllers();
});
app.Run(async context =>
{
    await context.Response.WriteAsync("Fallback for unmatched routes");
});
app.MapControllers();
```

**Concepts**
- `app.Run` terminal middleware — no subsequent `MapControllers` reached
- `Map` branch without auth — files and controllers exposed without protection
- `UseStaticFiles` serving files in admin path without restriction
- Correct pipeline order: Map* before terminal Run

**Answer**

The core problem is that `app.Run` is registered before `app.MapControllers()`, so the terminal fallback intercepts every request that isn't an `/admin` branch match — main app controllers are completely unreachable. The `/admin` branch itself lacks authentication middleware, so admin controllers have no auth enforcement and the static files served by the global `UseStaticFiles()` include anything under `wwwroot/admin/`, which means `config.json` is publicly downloadable. I fix this by registering all endpoint mapping before the terminal fallback, protecting the admin branch with its own auth middleware, and either scoping or removing static file access for admin paths:

```csharp
app.UseStaticFiles();
app.Map("/admin", admin =>
{
    admin.UseAuthentication();
    admin.UseAuthorization();
    admin.MapControllers();
});
app.MapControllers();
app.Run(async ctx => { ctx.Response.StatusCode = 404; });
```

---

## Q6. (P) Where should global exception-handling middleware sit relative to routing, authentication, and `UseDeveloperExceptionPage`? What breaks if exception middleware is registered too early or too late?

**Concepts**
- Exception handler as outermost wrapper catching all downstream faults
- `UseDeveloperExceptionPage` in Development, `UseExceptionHandler` in production
- Response-started constraint — exception handler cannot rewrite a begun response
- Auth short-circuits are not exceptions — separate code path

**Answer**

Exception-handling middleware should be among the very first registrations so it wraps the entire pipeline — if it appears after routing or auth, exceptions from those earlier components escape the handler entirely. In Development I call `app.UseDeveloperExceptionPage()` at the top; in production I use `app.UseExceptionHandler()` combined with a registered `IExceptionHandler` service that emits RFC 7807 `ProblemDetails` JSON. The constraint that matters most is that exception middleware cannot rewrite a response once headers have been sent — it can catch the exception and substitute an error body only if the response stream hasn't started, so handlers must call it early and never let a partial response begin before the handler has a chance to intercept. Placing it too late — after routing and controllers — means routing exceptions, static file errors, and forwarded header errors escape the handler. Authentication 401 short-circuits are not exceptions, so they always bypass exception middleware regardless of placement.

```csharp
if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
    app.UseExceptionHandler();
app.UseForwardedHeaders();
app.UseRouting();
app.UseAuthentication();
app.MapControllers();
```

---

## Q7. (R) This middleware tries to short-circuit banned clients but clients still receive full response bodies and logs show "Headers already sent." What went wrong?

```csharp
public async Task InvokeAsync(HttpContext context)
{
    await _next(context);

    if (IsBanned(context.Connection.RemoteIpAddress))
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsJsonAsync(new { error = "Forbidden" });
    }
}
```

**Concepts**
- Pre-check vs post-check placement relative to `_next`
- Response headers sealed after `_next` completes
- Short-circuit must precede `await _next` call
- Banned clients receiving full endpoint response before ban check

**Answer**

The middleware calls `await _next(context)` first, which executes the full endpoint handler and sends the response to the client, and only then checks whether the IP is banned. By that point the response headers are already committed and the body may have been sent, so attempting to set `StatusCode` and write JSON either silently fails or throws "headers already sent" — the ban check is completely ineffective. The fix is to check the ban condition before calling `_next` and short-circuit immediately if the IP is blocked, so the endpoint handler never runs and no data is sent to the banned client:

```csharp
if (IsBanned(context.Connection.RemoteIpAddress))
{
    context.Response.StatusCode = StatusCodes.Status403Forbidden;
    await context.Response.WriteAsJsonAsync(new { error = "Forbidden" });
    return;
}
await _next(context);
```

I also move this middleware as early in the pipeline as possible — before routing and auth — so banned clients don't consume routing or authentication resources.

---

## Q8. (M) Product needs request timing in logs for every endpoint including minimal APIs, plus early rejection of oversized uploads before MVC model binding. Would you use middleware, an endpoint filter, or an action filter for each concern — and why?

**Concepts**
- Middleware for pipeline-wide concerns covering all endpoint types
- `KestrelServerLimits.MaxRequestBodySize` for server-level size enforcement
- Endpoint filters for per-handler logic with typed parameters
- Action filters for MVC-specific concerns after model binding

**Answer**

Request timing belongs in middleware wrapping the full pipeline because it needs to apply to every endpoint type — minimal APIs, controllers, and any other handler — and middleware starts before any endpoint is selected. I add a `Stopwatch` before `await _next(context)` and record elapsed time after, then attach it to structured logging. Oversized upload rejection belongs partly at the Kestrel layer with `options.Limits.MaxRequestBodySize` to reject before any data is buffered, and for more granular limits, in middleware before routing that checks `Content-Length` against a configured maximum and returns 413 without invoking model binding. Action filters only run inside the MVC pipeline after model binding has already buffered the request, which is too late to prevent memory exhaustion from large bodies. Endpoint filters are the right choice for per-route validation that needs the handler's typed parameters, such as business rule validation on specific Minimal API routes — not for cross-cutting concerns like timing or upload limits.

---

## Q9. (R) Review custom correlation-ID middleware and registration. Some responses have two `X-Correlation-Id` headers and downstream services receive empty IDs. Diagnose.

```csharp
public async Task InvokeAsync(HttpContext context)
{
    var id = context.Request.Headers["X-Correlation-Id"].FirstOrDefault() ?? Guid.NewGuid().ToString();
    context.Items["CorrelationId"] = id;
    context.Response.Headers.Append("X-Correlation-Id", id);
    await _next(context);
}

// Program.cs
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>(); // registered twice by mistake
app.MapControllers();
```

**Concepts**
- Duplicate middleware registration — runs twice per request
- `Headers.Append` adding rather than setting — multiple header values
- Downstream propagation through `HttpContext.Items` vs request header
- `IHttpClientFactory` delegating handler for outbound header forwarding

**Answer**

There are two distinct bugs. First, `CorrelationIdMiddleware` is registered twice, so it runs twice on every request — the second execution calls `Headers.Append` again and adds a second `X-Correlation-Id` value to the response, producing duplicate headers that violate HTTP's single-value expectation for this header. Second, downstream services receiving empty IDs indicates that outbound HTTP calls read the correlation ID from the incoming request header rather than from `HttpContext.Items`, so when those internal calls go to services that don't forward their own headers, the chain breaks. I fix both issues: register the middleware once, use `Headers["X-Correlation-Id"] = id` rather than `Append` to prevent duplicates even if the header already exists, and wire outbound propagation through an `IHttpClientFactory` delegating handler that reads the ID from `Items` and adds it to outbound requests.

```csharp
if (!context.Response.Headers.ContainsKey("X-Correlation-Id"))
    context.Response.Headers["X-Correlation-Id"] = id;
```

---

## Q10. (D) You inherit a pipeline with 14 custom middleware components, some duplicated between `MapWhen` branches. How do you refactor without changing outward behavior — what belongs in middleware vs endpoint filters vs hosting reverse proxy?

**Concepts**
- Characterization tests before refactoring — locking current behavior
- Single ordered pipeline segment for deduplicated cross-cutting middleware
- Edge proxy for TLS, DDoS, geo-block concerns
- Endpoint metadata and policies replacing `MapWhen` auth duplication

**Answer**

The first step is writing characterization tests through `WebApplicationFactory` that assert current status codes, response headers, and body shapes for representative routes across all branches — this locks the behavior I must preserve. Then I draw the current pipeline order and mark duplicate registrations. Concerns that belong at the hosting reverse proxy — TLS termination, L7 DDoS protection, geo-blocking, coarse rate limits — I remove from the application entirely and document that the proxy handles them. What remains in middleware I consolidate into a single `UseStandardApiPipeline()` method whose body lists exactly one `UseForwardedHeaders`, one `UseExceptionHandler`, one `UseAuthentication`, one `UseRateLimiter`, and one `UseAuthorization`. The `MapWhen` copies that duplicated authentication for specific path prefixes get replaced with endpoint metadata and authorization policies — `[Authorize(Policy = "AdminOnly")]` on the admin controller or `.RequireAuthorization("AdminOnly")` on the minimal API route group — because policies express the same intent without forking the pipeline. Model validation and response transformation tied to specific routes move to endpoint filters. The result is one visible, ordered pipeline that reviewers can audit in a single scroll.
