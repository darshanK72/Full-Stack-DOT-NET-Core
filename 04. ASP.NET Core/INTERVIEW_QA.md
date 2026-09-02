# 04. ASP.NET Core — Cross-Topic Interview Q&A
> Back to [Full Stack .NET Core](../README.md)

## Subfolders

| # | Topic | Summary |
|---|-------|---------|
| 01 | [Introduction to ASP.NET Core](01.%20Introduction%20to%20ASP.NET%20Core/INTERVIEW_QA.md) | Framework overview, cross-platform model, Kestrel, and the unified hosting model |
| 02 | [Project Structure & Program.cs](02.%20Project%20Structure%20%26%20Program.cs/INTERVIEW_QA.md) | WebApplication builder, minimal hosting, service and middleware registration |
| 03 | [Middleware Pipeline](03.%20Middleware%20Pipeline/INTERVIEW_QA.md) | Request/response pipeline, Use/Run/Map, short-circuiting, and ordering rules |
| 04 | [Dependency Injection & Service Lifetimes](04.%20Dependency%20Injection%20%26%20Service%20Lifetimes/INTERVIEW_QA.md) | Transient/Scoped/Singleton, captive dependency, IServiceScopeFactory |
| 05 | [Configuration & Options Pattern](05.%20Configuration%20%26%20Options%20Pattern/INTERVIEW_QA.md) | appsettings.json layering, environment overrides, IOptions vs IOptionsSnapshot |
| 06 | [Logging & Diagnostics](06.%20Logging%20%26%20Diagnostics/INTERVIEW_QA.md) | ILogger, log levels, structured logging, OpenTelemetry integration |
| 07 | [Routing & Endpoints](07.%20Routing%20%26%20Endpoints/INTERVIEW_QA.md) | Endpoint routing, route templates, constraints, MapControllers vs MapGet |
| 08 | [Model Binding & Validation](08.%20Model%20Binding%20%26%20Validation/INTERVIEW_QA.md) | Source binding attributes, ModelState, DataAnnotations, FluentValidation |
| 09 | [Filters](09.%20Filters/INTERVIEW_QA.md) | Action/result/exception/authorization/resource filters, filter pipeline order |
| 10 | [Exception Handling](10.%20Exception%20Handling/INTERVIEW_QA.md) | UseExceptionHandler, ProblemDetails, exception filters vs middleware |
| 11 | [Static Files & Request Pipeline](11.%20Static%20Files%20%26%20Request%20Pipeline/INTERVIEW_QA.md) | UseStaticFiles, wwwroot, cache headers, SPA fallback |
| 12 | [Hosting, Kestrel & Environments](12.%20Hosting%2C%20Kestrel%20%26%20Environments/INTERVIEW_QA.md) | ASPNETCORE_ENVIRONMENT, Kestrel limits, IIS/nginx reverse proxy |
| 13 | [Minimal APIs](13.%20Minimal%20APIs/INTERVIEW_QA.md) | MapGet/Post/Put/Delete, endpoint filters, TypedResults, MapGroup |
| 14 | [Background & Hosted Services](14.%20Background%20%26%20Hosted%20Services/INTERVIEW_QA.md) | IHostedService, BackgroundService, hosted service lifetime and failure behavior |
| 15 | [WebSockets & Real-Time Transport](15.%20WebSockets%20%26%20Real-Time%20Transport/INTERVIEW_QA.md) | WebSocket upgrade, SignalR, long polling, Server-Sent Events |

---

## Table of Contents
- [CQ1. Middleware lifetime vs DI scoped services — the captive dependency trap](#cq1-middleware-lifetime-vs-di-scoped-services--the-captive-dependency-trap)
- [CQ2. Environment-driven configuration layering and the secrets.json production gotcha](#cq2-environment-driven-configuration-layering-and-the-secretsjson-production-gotcha)
- [CQ3. Routing, model binding, and validation — pipeline order and what breaks without ApiController](#cq3-routing-model-binding-and-validation--pipeline-order-and-what-breaks-without-apicontroller)
- [CQ4. Exception filters vs UseExceptionHandler — where each layer catches and what slips through](#cq4-exception-filters-vs-useexceptionhandler--where-each-layer-catches-and-what-slips-through)
- [CQ5. Background services, DI lifetimes, and what happens when a hosted service throws](#cq5-background-services-di-lifetimes-and-what-happens-when-a-hosted-service-throws)
- [CQ6. Minimal APIs vs controller-based APIs — routing, filter pipeline, and OpenAPI trade-offs](#cq6-minimal-apis-vs-controller-based-apis--routing-filter-pipeline-and-openapi-trade-offs)
- [CQ7. IOptions vs IOptionsSnapshot vs IOptionsMonitor — lifetime, reload, and the background-service trap](#cq7-ioptions-vs-ioptionssnapshot-vs-ioptionsmonitor--lifetime-reload-and-the-background-service-trap)
- [CQ8. Structured logging for exceptions — correlation IDs, LogError vs global exception handler](#cq8-structured-logging-for-exceptions--correlation-ids-logerror-vs-global-exception-handler)
- [CQ9. Custom exception middleware vs UseExceptionHandler vs ProblemDetails middleware in .NET 8+](#cq9-custom-exception-middleware-vs-useexceptionhandler-vs-problemdetails-middleware-in-net-8)
- [CQ10. Minimal API route conflicts caught at startup vs controller routing failures at runtime](#cq10-minimal-api-route-conflicts-caught-at-startup-vs-controller-routing-failures-at-runtime)
- [CQ11. Injecting services into filters — ServiceFilter vs TypeFilter vs IFilterFactory](#cq11-injecting-services-into-filters--servicefilter-vs-typefilter-vs-ifilterfactory)
- [CQ12. reloadOnChange for appsettings.json and the mid-request configuration race condition](#cq12-reloadonchange-for-appsettingsjson-and-the-mid-request-configuration-race-condition)
- [CQ13. Logging scope and context not flowing from an HTTP request into a background service](#cq13-logging-scope-and-context-not-flowing-from-an-http-request-into-a-background-service)
- [CQ14. Model binding exceptions vs ModelState.IsValid false — how binding errors surface differently](#cq14-model-binding-exceptions-vs-modelstateisvalid-false--how-binding-errors-surface-differently)
- [CQ15. Static files middleware ordering — UseStaticFiles before UseRouting to prevent route shadowing](#cq15-static-files-middleware-ordering--usestaticfiles-before-userouting-to-prevent-route-shadowing)
- [CQ16. WebSocket handlers and DI scopes — resolving scoped services via IServiceScopeFactory](#cq16-websocket-handlers-and-di-scopes--resolving-scoped-services-via-iservicescopefactory)
- [CQ17. Configuring Kestrel endpoints and HTTPS from appsettings.json vs code](#cq17-configuring-kestrel-endpoints-and-https-from-appsettingsjson-vs-code)
- [CQ18. Log-level filtering per category in configuration and the startup-log gotcha](#cq18-log-level-filtering-per-category-in-configuration-and-the-startup-log-gotcha)
- [CQ19. Minimal API endpoint filters vs MVC action filters — pipeline shape and attribute semantics](#cq19-minimal-api-endpoint-filters-vs-mvc-action-filters--pipeline-shape-and-attribute-semantics)

---

## CQ1. Middleware lifetime vs DI scoped services — the captive dependency trap

**Concepts**
- Middleware classes are instantiated once at startup and live for the application lifetime (effectively singleton)
- Injecting a scoped service into middleware via constructor creates a captive dependency — the scoped service is held for the entire app lifetime, defeating its per-request design intent
- `IServiceScopeFactory` is the correct pattern: call `CreateScope()` inside `InvokeAsync` to resolve a fresh scoped service per request
- `IHttpContextAccessor` internally uses the same scope-per-request approach — understanding why reinforces the factory pattern

**Answer**

Middleware components are registered in `Program.cs` and instantiated a single time during pipeline construction, giving them an effective singleton lifetime. The dependency injection system enforces this at startup: if you declare a constructor parameter of a scoped service in a middleware class, the framework will throw an `InvalidOperationException` at startup complaining about an attempt to capture a scoped service inside a singleton. Even if you work around that check, the scoped service would be held alive for the entire application lifetime, sharing state across requests and potentially causing subtle data leaks or concurrency bugs.

The correct pattern is to inject `IServiceScopeFactory` via the constructor — that interface is itself singleton-safe — and then call `serviceProvider.CreateScope()` inside the `InvokeAsync` method. This creates a new DI scope for each request, from which you can resolve the scoped service cleanly. The scope is wrapped in a `using` block so it is disposed at the end of the request, triggering `Dispose()` on every scoped object resolved within it. Entity Framework's `DbContext`, per-request caching objects, and unit-of-work patterns all follow this model. Getting this wrong is one of the most common runtime bugs in ASP.NET Core applications because the constructor injection compiles and runs but produces incorrect behaviour only under concurrent load.

---

## CQ2. Environment-driven configuration layering and the secrets.json production gotcha

**Concepts**
- `WebApplication.CreateBuilder` loads configuration sources in a defined priority order: `appsettings.json` → `appsettings.{Environment}.json` → environment variables → command-line args
- `ASPNETCORE_ENVIRONMENT` selects which environment-specific file overlays the base settings
- User secrets (`secrets.json`) are loaded only in the `Development` environment — they are deliberately excluded from production
- Environment variables and Key Vault bindings are the recommended production secret sources
- `IOptionsSnapshot<T>` re-reads configuration per request; `IOptions<T>` reads once at registration time

**Answer**

When `WebApplication.CreateBuilder` runs, it builds the configuration provider chain in a fixed order. First it reads `appsettings.json` as the baseline, then it overlays `appsettings.{Environment}.json` where the environment name is read from the `ASPNETCORE_ENVIRONMENT` environment variable. Values in the environment-specific file override matching keys in the base file, and environment variables override both. This layered design means you can keep safe defaults in `appsettings.json`, environment-specific non-secret settings in `appsettings.Production.json`, and confidential values in environment variables or Azure Key Vault at deployment time.

The production gotcha that catches developers is secrets.json. The .NET SDK auto-wires user secrets only when the environment is `Development`. If a developer tests with connection strings stored in secrets.json and then deploys to a container where `ASPNETCORE_ENVIRONMENT` is `Production`, the secrets file is never loaded and the application either fails to start or silently uses missing-value defaults. The fix is to source every secret in production from environment variables, a secrets manager, or a Key Vault-backed configuration provider. A related trap is forgetting that `IOptions<T>` captures the configuration snapshot at service-registration time; if you reload `appsettings.json` at runtime you need `IOptionsSnapshot<T>` for per-request re-reads or `IOptionsMonitor<T>` for long-lived singleton consumers.

---

## CQ3. Routing, model binding, and validation — pipeline order and what breaks without ApiController

**Concepts**
- Endpoint routing runs as a middleware pair: `UseRouting` selects the endpoint, `UseEndpoints`/`MapControllers` executes it
- After routing resolves the action, model binding sources parameter values from route data, query string, body, form, and headers
- `[ApiController]` implicitly applies `[FromBody]` inference, enables automatic `ModelState.IsValid` 400 short-circuit, and enforces attribute routing
- Without `[ApiController]`, `ModelState.IsValid` must be checked manually in every action; missing the check silently processes invalid input

**Answer**

The pipeline has three distinct stages. First, the routing middleware resolves which endpoint (controller action or minimal API handler) matches the current request path and HTTP method. No parameter values are read at this point — only the route template is matched and route tokens are captured. Second, model binding runs after the endpoint is selected; it reads route data, query string, form body, or JSON body depending on declared binding sources and populates the action method parameters. Third, validation runs against the bound model using DataAnnotations or FluentValidation, and the results are stored in `ModelState`.

The `[ApiController]` attribute changes the behaviour of this pipeline in two important ways. It activates binding source inference so that complex types are automatically treated as `[FromBody]` without requiring an explicit attribute, and it installs an implicit `ModelStateInvalidFilter` that short-circuits the request with a `400 Bad Request` and a `ValidationProblemDetails` body before the action method is ever invoked. If you omit `[ApiController]` — common in traditional MVC controllers or when migrating older code — neither inference nor automatic short-circuiting is active. Developers must add `if (!ModelState.IsValid) return BadRequest(ModelState);` at the top of every action. Forgetting this means invalid data silently reaches business logic, producing hard-to-diagnose downstream errors rather than clean client-facing validation failures.

---

## CQ4. Exception filters vs UseExceptionHandler — where each layer catches and what slips through

**Concepts**
- `IExceptionFilter` / `IAsyncExceptionFilter` run inside the MVC filter pipeline — they catch exceptions thrown by action methods, result execution, and other MVC filters
- `UseExceptionHandler` / `UseStatusCodePages` are middleware — they catch exceptions that bubble up through the entire pipeline, including exceptions thrown by other middleware
- Exception filters cannot catch exceptions thrown before or after MVC executes (e.g., in routing middleware, authentication middleware, or response-writing middleware)
- A middleware exception that bypasses filters is the most common production surprise: `UseExceptionHandler` must sit early in the pipeline to wrap everything downstream

**Answer**

The confusion stems from two separate interception points existing at different pipeline depths. Exception filters are part of the MVC filter pipeline and execute within the scope of a single controller action. They are invoked when an unhandled exception propagates from an action method body, from an action result during execution, or from another MVC filter earlier in the chain. They are well-suited for transforming domain exceptions into consistent ProblemDetails responses for a specific controller or applied globally via a filter registration. However, they are completely invisible to anything that happens outside the MVC layer.

`UseExceptionHandler` is middleware placed near the top of the `Program.cs` pipeline. Because every subsequent middleware runs inside its try/catch wrapper, it can catch exceptions thrown by authentication middleware, routing, database connection failures in other middleware, and any other component that sits downstream of it. The production gotcha occurs when teams rely solely on exception filters for global error handling and deploy code with an exception in, say, a custom authentication handler — the filter never fires because the request never reaches MVC, and the client receives an unformatted 500 with a stack trace. The robust pattern is to register `UseExceptionHandler` early as the catch-all, optionally map it to a `/error` re-execution endpoint that returns a structured `ProblemDetails` response, and use exception filters only for action-specific transformation logic layered on top.

---

## CQ5. Background services, DI lifetimes, and what happens when a hosted service throws

**Concepts**
- `IHostedService` / `BackgroundService` implementations are registered as singletons in the DI container
- Injecting scoped services (e.g., `DbContext`, repositories) via constructor is invalid — the scope is never created, so a `InvalidOperationException` is thrown at startup
- `IServiceScopeFactory` resolves scoped dependencies inside the background loop with an explicit `CreateScope()` per unit of work
- An unhandled exception in `BackgroundService.ExecuteAsync` causes the host to stop by default in .NET 6+ unless `BackgroundServiceExceptionBehavior` is overridden

**Answer**

Hosted services registered via `AddHostedService<T>` are singleton objects created once and kept alive for the application lifetime. This makes constructor injection of scoped services illegal: the DI system will either throw at startup or, if the check is skipped, silently hold the scoped service open for the entire application lifetime — the same captive dependency problem as middleware. The correct pattern is to inject `IServiceScopeFactory` and call `CreateScope()` at the beginning of each loop iteration or unit of work inside `ExecuteAsync`. The scope — and therefore the `DbContext` or repository resolved from it — is then properly disposed after each work item, preventing connection leaks and stale entity tracking.

The failure-mode behaviour changed in .NET 6 and is important to know. Prior to that version, an unhandled exception in `ExecuteAsync` was swallowed and the service stopped silently, leaving the application alive but broken. From .NET 6 onward, the default `BackgroundServiceExceptionBehavior` is `StopHost`, meaning an unhandled exception brings down the entire host process. This is safer for correctness — a dead background service is visible in container orchestration rather than silently absent — but it requires proper exception handling inside `ExecuteAsync` loops. Production-quality services wrap the core loop in a try/catch, log the exception with structured context, and decide whether to retry with back-off, transition to a faulted state, or let the host restart via the orchestrator.

---

## CQ6. Minimal APIs vs controller-based APIs — routing, filter pipeline, and OpenAPI trade-offs

**Concepts**
- Minimal APIs map routes directly in `Program.cs` using `MapGet` / `MapPost` etc.; controllers use `[Route]` attributes and `MapControllers()`
- Minimal APIs have their own endpoint filter pipeline (`IEndpointFilter`) — it is not the same as the MVC `IActionFilter` pipeline
- MVC filters (authorization, resource, action, exception, result) do not apply to minimal API endpoints
- OpenAPI metadata in minimal APIs is added via `.WithName()`, `.WithSummary()`, `.Produces<T>()`, and `.WithOpenApi()`; `[ApiController]` attribute conveniences (binding inference, auto-validation 400) are absent
- .NET 9/10 ships a built-in `Microsoft.AspNetCore.OpenApi` package that generates OpenAPI documents for both minimal APIs and controllers without Swashbuckle

**Answer**

The choice between Minimal APIs and controllers is a design judgment call with consequences across routing, extensibility, and tooling. Minimal APIs excel at low-overhead scenarios: each endpoint is a lambda or a method group wired directly to a route, compiled into a request delegate with minimal allocations. There is no controller instantiation, no action descriptor resolution, and no MVC model-binding pipeline overhead. For microservices with a small, stable surface area, this produces measurably faster cold-path throughput.

The trade-off surfaces when the API grows. Controller-based MVC provides a rich filter pipeline with well-understood ordering (authorization → resource → action → exception → result), `[ApiController]` binding inference and automatic validation short-circuiting, and mature OpenAPI support via Swashbuckle or NSwag. Minimal APIs offer endpoint filters (`IEndpointFilter`) as a replacement for action filters, but resource filters and exception filters have no equivalent — exception handling must be done inside the lambda or via a middleware wrapper. Before .NET 9, OpenAPI metadata in minimal APIs required manual `.Produces<T>()` annotations to achieve the same schema quality Swashbuckle inferred from controller return types. .NET 9 and 10 close this gap significantly with the built-in `Microsoft.AspNetCore.OpenApi` package, which generates accurate OpenAPI 3.1 documents for both styles. The practical guidance for .NET 10 projects is: prefer Minimal APIs for focused, single-responsibility services and use controllers when the team expects a large API surface, needs complex filter chains, or is porting existing MVC code.

---

## CQ7. IOptions vs IOptionsSnapshot vs IOptionsMonitor — lifetime, reload, and the background-service trap

**Concepts**
- `IOptions<T>` is a singleton — it reads configuration once at registration time and never reflects reloads
- `IOptionsSnapshot<T>` is scoped — it re-reads the current configuration value once per HTTP request, making it safe for request handlers
- `IOptionsMonitor<T>` is a singleton — it reflects configuration reloads immediately and exposes an `OnChange` callback; the correct choice for background services and other singletons
- Injecting `IOptionsSnapshot<T>` into a singleton (including `BackgroundService`) causes a captive-dependency startup exception because a scoped service cannot be resolved into a singleton scope

**Answer**

The three options interfaces exist to solve the same problem at different lifetimes. `IOptions<T>` captures a snapshot of `T` when the DI container is built and hands back that same instance on every resolution. It is the right choice when your configuration values are stable across the application lifetime and you want the lowest overhead. The moment `appsettings.json` changes on disk, `IOptions<T>` is blind to the change.

`IOptionsSnapshot<T>` solves the per-request freshness requirement. Because it is registered as scoped, the DI container creates a new instance for every HTTP request, reading the latest configuration values at that point. This is the correct choice for controllers, middleware resolved per-request, and any scoped service that needs live settings. The fatal gotcha is attempting to inject `IOptionsSnapshot<T>` into a singleton — `BackgroundService` is the most common victim. The runtime throws an `InvalidOperationException` at startup because a scoped service cannot live inside a singleton scope.

`IOptionsMonitor<T>` is the singleton-safe live-reload interface. It exposes a `CurrentValue` property that always reflects the latest configuration, and an `OnChange(Action<T, string?> listener)` callback for reacting to reloads. Background services should always use `IOptionsMonitor<T>` and read `monitor.CurrentValue` at the start of each work iteration rather than caching the value in a field, to guarantee they pick up changes without requiring a restart.

---

## CQ8. Structured logging for exceptions — correlation IDs, LogError vs global exception handler

**Concepts**
- Structured logging attaches named properties (e.g., `CorrelationId`, `UserId`) to log events rather than embedding them in message strings, enabling log aggregation and querying
- `ILogger.BeginScope` opens a named scope that adds properties to every log event emitted within its block, including from called methods
- `ILogger.LogError(exception, message, args)` is the correct overload for exceptions — it serialises the exception type, message, and stack trace as structured fields, not as a plain string
- The global exception handler (middleware or `UseExceptionHandler`) should log at `LogError` once and return a structured `ProblemDetails` response; exception filters and middleware lower in the stack should not duplicate the log entry
- Correlation IDs should be set in early middleware (e.g., reading an `X-Correlation-Id` request header) and stored in `ILogger` scope so every subsequent log line in the same request carries the ID automatically

**Answer**

Structured logging transforms exception diagnostics from opaque text searches into queryable event streams. The first step is establishing a correlation ID early in the pipeline — typically in a dedicated middleware that reads `X-Correlation-Id` from the incoming request headers, generates a new ID if absent, writes it to the response, and opens an `ILogger` scope: `using (_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))`. Because `BeginScope` is ambient for the duration of the `using` block, every subsequent `ILogger` call within that request — including calls deep inside services — automatically inherits the `CorrelationId` property without any explicit parameter threading.

For exception logging, the overload matters. `_logger.LogError(ex, "Order processing failed for OrderId {OrderId}", orderId)` serialises the `Exception` object as a first-class field (`{@Exception}` in Serilog terminology) alongside the structured properties. Logging `ex.ToString()` instead collapses the exception into a plain string, destroying queryability. The global exception handler should be the single point that logs unhandled exceptions at `Error` level. Allowing both the exception filter and the middleware to log the same exception creates duplicate entries in Kibana or Application Insights that inflate error counts and confuse on-call engineers. The clean architecture is: global handler logs once, returns `ProblemDetails`, and includes the `correlationId` in the response body so the client can reference it in a support ticket.

---

## CQ9. Custom exception middleware vs UseExceptionHandler vs ProblemDetails middleware in .NET 8+

**Concepts**
- `UseExceptionHandler` re-executes the pipeline against a specified path (e.g., `/error`) to produce a formatted response — the exception is stored in `IExceptionHandlerFeature`
- Custom exception middleware (a class implementing `InvokeAsync` with a try/catch) gives full control over the response but must be written and maintained manually
- .NET 8 introduced `IProblemDetailsService` and `AddProblemDetails()` + `UseProblemDetails()` which automatically map unhandled exceptions and status codes to RFC 9457 `ProblemDetails` responses, optionally with custom `IProblemDetailsWriter` hooks
- `UseExceptionHandler` must appear before any middleware that can throw — placing it after `UseAuthentication` or `UseRouting` leaves those layers unprotected
- Exception filters inside MVC are the last resort before the pipeline unwinds — they cannot catch errors in middleware, but they can enrich ProblemDetails before it is written

**Answer**

Before .NET 8, developers had two practical choices for global exception handling. Writing a custom middleware class with a try/catch wrapper in `InvokeAsync` gave complete control but required boilerplate for content negotiation, status code selection, and ProblemDetails serialisation. Calling `app.UseExceptionHandler("/error")` delegated response formatting to a secondary pipeline execution, but the handler action had to read `HttpContext.Features.Get<IExceptionHandlerFeature>()` to retrieve the original exception, and it only caught exceptions that bubbled past its position in the pipeline.

.NET 8 introduced a first-class solution. `builder.Services.AddProblemDetails()` registers `IProblemDetailsService` and the default JSON writer. `app.UseExceptionHandler()` — called without a path argument — now uses that service to automatically serialise unhandled exceptions as RFC 9457 `ProblemDetails` JSON, honouring the `Accept` header for content negotiation. Custom mappings are added via `options.CustomizeProblemDetails`: a callback that receives a `ProblemDetailsContext` and can set the `Status`, `Title`, `Detail`, and arbitrary extension members before the response is written. This replaces most hand-rolled exception middleware. In .NET 10, the recommended setup is `AddProblemDetails()` at service registration and `UseExceptionHandler()` as the first middleware after `app.UseHttpsRedirection()`, ensuring every downstream component including authentication, routing, and business logic is covered by the catch-all.

---

## CQ10. Minimal API route conflicts caught at startup vs controller routing failures at runtime

**Concepts**
- Minimal APIs register routes as `RouteEndpoint` objects during `WebApplication.Build()`; ambiguous routes that cannot be disambiguated by HTTP method or constraints throw an `InvalidOperationException` at startup before any request is served
- Controller routing resolves action methods dynamically at request time via the action selector; ambiguous routes only produce a `AmbiguousMatchException` (HTTP 500) when a matching request actually arrives
- Route groups (`MapGroup`) in minimal APIs allow a shared prefix and shared metadata (e.g., `RequireAuthorization()`) to be applied to a set of endpoints without per-endpoint repetition
- Route constraints (`{id:int}`, `{slug:regex(...)}`) narrow matching and participate in ambiguity resolution; minimal APIs support the same constraint syntax as attribute routing
- In .NET 10, route conflict detection at startup covers parameter-type constraints, making it safer to ship without integration tests for every route combination

**Answer**

The fundamental difference in failure timing has significant operational consequences. When you call `app.MapGet("/orders/{id}", ...)` and `app.MapGet("/orders/{id}", ...)` twice with identical templates in the same minimal API application, `WebApplication.Build()` throws at startup. The application never serves a single request, the container orchestrator marks it unhealthy, and the deployment fails visibly. This early-exit behaviour means route mistakes are caught in CI rather than discovered by a user hitting a specific path in production.

Controller routing works differently because it defers resolution to the action selector middleware. The `MapControllers()` call does not enumerate every possible route combination — it registers a single routing middleware that runs at request time. If two controller actions both match `GET /orders/{id}`, the request succeeds for every path except the ambiguous one, and the `AmbiguousMatchException` surfaces only when a real client hits that specific route. This makes integration tests for controller-based APIs more important: full route-table coverage is the only automated way to catch ambiguity before production.

Route groups in minimal APIs address the common pattern of sharing a prefix and a cross-cutting concern. `var orders = app.MapGroup("/orders").RequireAuthorization()` attaches the authorization requirement to all endpoints added to the group without requiring `.RequireAuthorization()` on each individual `MapGet`/`MapPost` call. Combined with startup-time conflict detection, this makes the minimal API routing model more predictable and testable for .NET 10 microservices.

---

## CQ11. Injecting services into filters — ServiceFilter vs TypeFilter vs IFilterFactory

**Concepts**
- `[ServiceFilter(typeof(MyFilter))]` resolves the filter from the DI container using its registered lifetime — the filter must be registered in `IServiceCollection`; otherwise a `InvalidOperationException` is thrown at runtime when the filter is activated
- `[TypeFilter(typeof(MyFilter))]` instantiates the filter via `ObjectFactory`, resolving constructor dependencies from DI without requiring the filter itself to be registered — useful for filters with arguments
- `IFilterFactory` is an interface implemented on a custom attribute; its `CreateInstance(IServiceProvider)` method is called per-request (or per-controller), giving the most control over lifetime and dependency resolution
- Constructor injection directly on a filter class only works when the filter is activated through the DI container (i.e., `ServiceFilter` or `IFilterFactory`) — a plain `[MyFilter]` attribute uses `Activator.CreateInstance`, which cannot satisfy constructor parameters

**Answer**

MVC filters are often the right place for cross-cutting concerns such as auditing, tenant resolution, or response caching, but they require careful attention to how they are constructed and how their dependencies are resolved. The simplest case — adding `[MyFilter]` as an attribute — uses the .NET `Activator.CreateInstance` path, which calls the parameterless constructor. Any constructor parameters are silently ignored, meaning the filter receives no injected dependencies and must resolve them manually via a service locator if needed. This is rarely what developers intend when they first write a filter.

`[ServiceFilter(typeof(MyFilter))]` is the correct DI-aware attribute. When the MVC framework activates the endpoint, it calls `IServiceProvider.GetRequiredService<MyFilter>()`, which resolves constructor dependencies according to the filter's registered lifetime. The filter must be registered in the container (`services.AddScoped<MyFilter>()`) or the activation throws. This approach also respects the filter's lifetime: a scoped filter gets a new instance per request, and a singleton filter is reused, making captive-dependency rules apply here just as they do in middleware.

`[TypeFilter(typeof(MyFilter))]` removes the requirement for prior registration but limits lifetime control. The framework uses `ObjectFactory` to construct the filter, resolving constructor parameters from the current scope automatically. It also accepts `Arguments` to pass literal values alongside injected ones, which is useful for parameterised filters. `IFilterFactory` is the most powerful option: the custom attribute implements `CreateInstance`, can read its own properties to produce a configured filter instance, and declares `IsReusable` to indicate whether the instance can be cached across requests, enabling singleton-like behaviour for stateless filters.

---

## CQ12. reloadOnChange for appsettings.json and the mid-request configuration race condition

**Concepts**
- `reloadOnChange: true` (the default in `WebApplication.CreateBuilder`) wires a `FileSystemWatcher` to `appsettings.json`; when a change is detected the `IConfigurationRoot` is atomically rebuilt and all `IOptionsMonitor<T>` instances are notified
- The reload is not transactional with respect to in-flight HTTP requests — two reads of the same configuration key within a single request handler can return different values if a reload occurs between them
- `IOptionsSnapshot<T>` reads a consistent snapshot once per request scope, shielding individual requests from mid-flight changes; `IOptions<T>` and `IOptionsMonitor<T>.CurrentValue` are both subject to the race
- High-frequency file saves (e.g., CI/CD rolling deploys writing appsettings) can trigger rapid successive reloads, causing transient `ObjectDisposedException` on the old configuration root

**Answer**

File-based configuration reload sounds straightforward — the file changes, the app picks up the new values. The subtlety emerges at the intersection of reload timing and request concurrency. The `IConfigurationRoot` swap is atomic at the provider level: a new root is built from scratch on a background thread and then set as the active root via a reference swap. However, a request handler that reads a feature flag from `IConfiguration["FeatureFlags:NewCheckout"]` and then, several lines later, reads a threshold from `IConfiguration["Limits:MaxItems"]` is making two separate reads against `IConfigurationRoot`. If a reload occurs between those two reads, the first read returns values from the old root and the second returns values from the new root, potentially producing an inconsistent combination that neither the old nor the new configuration alone would have produced.

`IOptionsSnapshot<T>` is the correct mitigation for request handlers. Because it binds the entire `T` options class once per request scope at the moment the scope is created, all reads within that request see a single consistent snapshot regardless of how many reloads happen during the request's lifetime. `IOptions<T>` is immune because it never reloads at all. The race applies only to code that reads `IConfiguration` directly or calls `IOptionsMonitor<T>.CurrentValue` multiple times in the same logical operation. In production, the safest pattern is to bind options to a strongly-typed class, inject `IOptionsSnapshot<T>` in request handlers, and use `IOptionsMonitor<T>` only in background services where the goal is precisely to observe live changes one iteration at a time.

---

## CQ13. Logging scope and context not flowing from an HTTP request into a background service

**Concepts**
- `ILogger.BeginScope` creates an ambient scope that is tied to the current `AsyncLocal` execution context; it does not automatically flow across thread-pool boundaries or `Task.Run` calls that escape the current request scope
- A background service queued via `IBackgroundTaskQueue` (or `Channel<T>`) runs on a different `AsyncLocal` context from the HTTP request that enqueued it; any `BeginScope` properties from the request (e.g., `CorrelationId`, `UserId`) are not present in the background work item
- The correct pattern is to explicitly capture the values from the request context and pass them as data alongside the work item, then re-open the scope inside the background service using `_logger.BeginScope`
- `ILogger<T>` in a `BackgroundService` is resolved from the singleton scope; using it correctly requires understanding that its scope stack starts empty for each `ExecuteAsync` loop iteration

**Answer**

`AsyncLocal<T>` is the mechanism underlying both `ILogger.BeginScope` and `HttpContext.TraceIdentifier`. When a controller action opens a logger scope, the scope dictionary is stored in an `AsyncLocal` slot on the current execution context. Any `await` that continues on the same logical flow inherits that context automatically — which is why structured log properties flow naturally across `await` calls within a single request handler. The problem arises when work is handed off to a background service through a queue or channel. At that point, a new task is dequeued and executed by a worker loop running in a completely separate execution context. The `AsyncLocal` dictionary that carried `CorrelationId` and `UserId` is not present in that context; it was tied to the original HTTP request's `Task` chain.

The fix is to treat correlation data as explicit message payload rather than ambient state. When enqueueing a work item, capture the values you need from the current logger scope or `HttpContext` and embed them in the work item record itself. Inside the background service, read those values from the work item and call `_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = workItem.CorrelationId })` before processing begins. This re-establishes the structured context on the worker's execution context for the duration of that item's processing. A convenience wrapper that opens the scope, processes the item, and disposes the scope within a `using` block ensures the properties do not bleed across work items on the same worker thread.

---

## CQ14. Model binding exceptions vs ModelState.IsValid false — how binding errors surface differently

**Concepts**
- `ModelState.IsValid == false` represents validation failures discovered after successful binding — the value was readable but failed DataAnnotations rules such as `[Required]` or `[Range]`
- A binding failure (e.g., `"abc"` for an `int` parameter) also results in `ModelState.IsValid == false`, but the model parameter is set to its default value, not the input — the error is stored in `ModelState` under the parameter name
- `[FromBody]` JSON deserialization errors thrown by `System.Text.Json` (e.g., malformed JSON, type mismatch) propagate as an `InputFormatterException`, which `[ApiController]` catches and converts to a 400 with `ValidationProblemDetails`; without `[ApiController]` the exception propagates to the exception handler
- Query-string and route binding errors are always surfaced as `ModelState` entries, never as thrown exceptions, regardless of whether `[ApiController]` is present

**Answer**

The distinction between a binding failure and a validation failure is subtle but matters for error handling strategy. Validation failures occur after the model binder has successfully read and converted a value — the input was parseable, but it violated a DataAnnotations constraint. Binding failures occur when the model binder cannot convert the raw input to the target type: supplying `"hello"` for an `int` parameter from a query string is a binding error. In both cases `ModelState.IsValid` returns `false`, but the model parameter's value differs: a validated-but-failed parameter holds the bound value, while a binding-failed parameter holds the type default.

The behaviour diverges sharply for `[FromBody]`. When `System.Text.Json` encounters malformed JSON or a structural mismatch (e.g., an object where an integer was expected), it throws an `InputFormatterException` during body reading, before model binding even populates `ModelState`. With `[ApiController]`, the built-in `ModelStateInvalidFilter` catches this exception and converts it to a 400 `ValidationProblemDetails` response, making the error appear as a `ModelState` entry to the caller. Without `[ApiController]`, the `InputFormatterException` propagates up the pipeline and reaches `UseExceptionHandler`, where it is typically logged and returned as a 500 unless explicitly caught. Route and query-string binders never throw; they always record failures as `ModelState` entries, making them safe to inspect after binding completes regardless of `[ApiController]` presence.

---

## CQ15. Static files middleware ordering — UseStaticFiles before UseRouting to prevent route shadowing

**Concepts**
- `UseStaticFiles` serves files from `wwwroot` by short-circuiting the pipeline — if a request path matches a file, the response is written immediately and no subsequent middleware runs
- `UseRouting` runs the endpoint routing system; placing it before `UseStaticFiles` means every request — including those for `.css`, `.js`, and image files — is evaluated against the route table before reaching the file system
- A controller route like `[Route("images/{filename}")]` can shadow `wwwroot/images/logo.png` if `UseRouting` runs first, because the router selects the controller action before the static file middleware gets a chance to respond
- `UseStaticFiles` should be placed before `UseRouting` and `UseAuthentication` in most applications, because static assets are typically public and should not require route resolution or authentication checks

**Answer**

Middleware ordering in ASP.NET Core is not automatic — each call to `Use*` appends a component to a linked list, and the order of those calls determines which component sees the request first. `UseStaticFiles` is a short-circuit middleware: when the incoming path resolves to a file in `wwwroot`, it writes the file to the response and calls neither `next` nor any subsequent middleware. This short-circuit is the performance benefit of static file serving, but it only works if `UseStaticFiles` is positioned early enough to run before routing.

If `UseRouting` is placed first, the routing engine evaluates the request path against every registered route template before `UseStaticFiles` has a chance to check the file system. A controller action with a route template that happens to match the path of a static asset — for example a `GET /images/{id}` route that was intended to serve database-stored images and a physical `wwwroot/images/logo.png` file — will cause the controller route to win, and the file will be silently unreachable. The converse is also true: `UseStaticFiles` before `UseRouting` means static files bypass authentication middleware placed after `UseRouting`, so assets in `wwwroot` are served anonymously even when the site requires login. For protected static content, the recommended .NET 10 pattern is to move the file outside `wwwroot`, keep it under application control, and serve it through a controller or minimal API endpoint that enforces authorization.

---

## CQ16. WebSocket handlers and DI scopes — resolving scoped services via IServiceScopeFactory

**Concepts**
- WebSocket connections are long-lived — the HTTP connection is upgraded and held open for the duration of the WebSocket session, which can span minutes or hours
- Unlike HTTP requests, WebSocket handling does not automatically create a DI scope; the `HttpContext.RequestServices` scope is tied to the HTTP upgrade request and is disposed when that request completes, not when the WebSocket closes
- Resolving scoped services (e.g., `DbContext`) directly from `HttpContext.RequestServices` inside a WebSocket message loop risks using a disposed scope for connections that outlive the initial request
- `IServiceScopeFactory.CreateScope()` inside the WebSocket handler creates an explicit scope whose lifetime is controlled by the handler, ensuring services are alive and not disposed for the duration of the connection

**Answer**

WebSocket handling lives in a grey zone of the DI lifetime model. The HTTP upgrade handshake is processed as a normal request, and `HttpContext.RequestServices` provides access to the scoped DI scope for that handshake. Once the upgrade succeeds, the application enters a message-receive loop: `await webSocket.ReceiveAsync(buffer, ct)` blocks until a message arrives, processes it, and loops. This loop is long-lived, but the original HTTP request's `RequestServices` scope was created for the upgrade handshake, and its disposal timing depends on how the middleware pipeline manages scope creation — in practice it may be disposed long before the WebSocket closes.

The safe pattern is to create an explicit scope at the start of the WebSocket handler using `_scopeFactory.CreateScope()` where `_scopeFactory` is an `IServiceScopeFactory` injected via constructor (it is singleton-safe). The resulting `IServiceScope` is wrapped in a `using` block that spans the entire message loop, ensuring the scope — and every service resolved from it — remains alive for the full connection lifetime. Per-message unit-of-work scenarios (e.g., saving each received message to a database) can nest an inner scope inside the receive loop: `using var msgScope = _scopeFactory.CreateScope()` creates a fresh `DbContext` per message and disposes it after the save, preventing entity-tracking accumulation that would grow unboundedly over a long-lived connection.

---

## CQ17. Configuring Kestrel endpoints and HTTPS from appsettings.json vs code

**Concepts**
- Kestrel can be configured via `appsettings.json` under the `Kestrel:Endpoints` section, allowing port and certificate bindings to be changed without recompiling
- Code-based configuration via `builder.WebHost.ConfigureKestrel(options => ...)` runs after `appsettings.json` and overrides it for any setting explicitly set in code
- HTTPS certificate binding supports three sources: a `.pfx` file path + password in configuration, a certificate store subject name, or the ASP.NET Core data protection–managed development certificate
- `reloadOnChange: true` on the Kestrel configuration section allows certificate rotation (e.g., renewed Let's Encrypt certs) without restarting the process, as long as the endpoint is bound using `KestrelServerOptions.ListenOptions.UseHttps` configured from the section
- The data protection key ring (used for cookie encryption and antiforgery tokens) is separate from the TLS certificate and must be persisted externally (Azure Blob, file share) when running multiple instances

**Answer**

Kestrel endpoint configuration from `appsettings.json` enables operational teams to change port bindings, TLS certificates, and connection limits without touching application code or rebuilding the image. The `Kestrel:Endpoints` section follows a specific schema: each named entry declares an `Url` (or `Protocols`) and an optional `Certificate` subsection with either a `Path`/`Password` pair for a `.pfx` file or a `Subject`/`Store`/`Location` triple for a certificate store lookup. When the application starts, `WebApplication.CreateBuilder` calls `server.Configure(context.Configuration.GetSection("Kestrel"))` automatically, wiring the section to `KestrelServerOptions`.

The ordering of configuration sources is important for overrides. Any `builder.WebHost.ConfigureKestrel(...)` call in `Program.cs` runs after the configuration-based setup and wins for any property it sets explicitly. This means code-based defaults can be safely paired with configuration-based overrides: set conservative connection limits in code, let operations teams expand them in `appsettings.Production.json`. For HTTPS certificate rotation, the `reloadOnChange` mechanism propagates the new `Certificate:Path` and `Certificate:Password` to Kestrel, which can perform a hot certificate swap on the endpoint without closing existing connections. This works because Kestrel monitors the configuration section for changes and re-calls the `UseHttps` setup on the listener. The data protection key ring is a separate concern — it encrypts auth cookies and antiforgery tokens and must be stored in a shared, durable location (Azure Blob Storage or a UNC share) when multiple Kestrel instances sit behind a load balancer, otherwise cookies encrypted by instance A cannot be decrypted by instance B.

---

## CQ18. Log-level filtering per category in configuration and the startup-log gotcha

**Concepts**
- `Logging:LogLevel` in `appsettings.json` filters log events by category (namespace prefix) and minimum level; `Default` covers any category not matched by a more specific key
- Provider-specific overrides (e.g., `Logging:Console:LogLevel`) apply only to that provider, allowing verbose console output in development without flooding Application Insights in production
- Log filtering from configuration applies only after the host is built — logs emitted during `WebApplication.CreateBuilder` and `builder.Build()` use a bootstrap logger configured in code, not the appsettings filter
- Serilog's `UseSerilog(ctx, cfg => cfg.ReadFrom.Configuration(ctx.Configuration))` and .NET's `ILoggingBuilder.AddConfiguration` both only attach after the host build phase; early startup exceptions may go unlogged unless a bootstrap sink is configured first

**Answer**

The `Logging` section in `appsettings.json` is the primary runtime control surface for log verbosity without code changes. Category filtering works by matching the beginning of the category name — the category for `ILogger<OrderController>` is `MyApp.Controllers.OrderController`, so a rule under `MyApp.Controllers` applies to all controllers while a rule under `MyApp` applies to the entire application namespace. Rules are evaluated longest-match-first: a specific category key always beats `Default`. Provider-specific sections (`Logging:Console:LogLevel`, `Logging:ApplicationInsights:LogLevel`) layer on top, allowing development environments to emit `Debug` to the console while the same binary sends only `Warning` and above to the telemetry backend.

The startup-log gotcha is a significant operational blind spot. `WebApplication.CreateBuilder` and `builder.Build()` both run significant code — configuration loading, service registration, DI container validation — before the host is fully constructed. Any `ILogger` obtained from `builder.Logging` during this phase uses a preliminary logger that does not yet have the appsettings filter applied. Exceptions thrown during `builder.Build()`, such as DI validation errors or Kestrel configuration failures, may be written to an early logger that goes nowhere depending on how logging is bootstrapped. The .NET 10 recommended pattern for Serilog users is to create a static bootstrap logger (`Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger()`) before calling `CreateBuilder`, then replace it with the full configuration-driven logger inside `UseSerilog`. For the built-in provider, adding `builder.Logging.AddConsole()` before `builder.Build()` ensures startup errors appear on stdout even before the appsettings filter chain is active.

---

## CQ19. Minimal API endpoint filters vs MVC action filters — pipeline shape and attribute semantics

**Concepts**
- Minimal API endpoint filters implement `IEndpointFilter` and are added via `.AddEndpointFilter<T>()` or `.AddEndpointFilter(async (ctx, next) => ...)` on individual routes or groups
- The `IEndpointFilter` pipeline is a single linear chain of `InvokeAsync(EndpointFilterInvocationContext, EndpointFilterDelegate)` calls — there is no equivalent of MVC's resource, result, or exception filter stages
- `[Authorize]` applied to a minimal API endpoint works via endpoint metadata, not via an MVC filter; the authorization middleware reads the metadata and enforces it before the endpoint delegate runs
- `[ValidateAntiForgeryToken]` is an MVC action filter and has no effect on minimal API endpoints; the equivalent is calling `IAntiforgery.ValidateRequestAsync` manually inside the handler or an endpoint filter
- `[ApiController]` automatic model-state validation and binding-source inference do not apply to minimal APIs; validation must be performed explicitly or via an endpoint filter

**Answer**

The filter pipelines of MVC and Minimal APIs share a goal — intercepting request processing before and after the core handler — but differ fundamentally in structure. MVC's filter pipeline has five ordered stages: authorization, resource, action, exception, and result. Each stage has a distinct semantic contract: resource filters can short-circuit before model binding, result filters can transform the `IActionResult` before it is executed, and exception filters can catch and transform exceptions thrown anywhere within the MVC layer. Endpoint filters have none of this staging. They form a single middleware-like chain where each filter calls `await next(context)` to proceed, and can inspect or modify the `EndpointFilterInvocationContext` before the call and the `object?` return value after it. There is no built-in exception stage; catching exceptions requires a try/catch inside the filter's `InvokeAsync`.

Authorization on minimal API endpoints works through endpoint metadata rather than filter execution. `app.MapGet("/orders", ...).RequireAuthorization("AdminPolicy")` stores a policy name in the endpoint's metadata collection. The `UseAuthorization` middleware — which runs before the endpoint delegate — reads this metadata and enforces the policy, meaning authorization still happens correctly without MVC. `[ValidateAntiForgeryToken]`, however, is an `IFilterMetadata` implementation that only the MVC filter pipeline processes; placing it on a minimal API lambda is silently ignored. Antiforgery validation must be done explicitly via `app.MapPost("/form", async (IAntiforgery af, HttpContext ctx) => { await af.ValidateRequestAsync(ctx); ... })` or extracted into a reusable `IEndpointFilter`.

---
