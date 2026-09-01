# Dependency Injection & Service Lifetimes — Interview Q&A
> Back to [README](../README.md)

## Table of Contents

- [Chapter 04. Dependency Injection & Service Lifetimes](#chapter-04-dependency-injection-service-lifetimes)
  - [Q1. What is dependency injection in ASP.NET Core?](#chapter-04-dependency-injection-service-lifetimes-q1)
  - [Q2. What is the built-in DI container in ASP.NET Core?](#chapter-04-dependency-injection-service-lifetimes-q2)
  - [Q3. What are the three service lifetimes in ASP.NET Core DI?](#chapter-04-dependency-injection-service-lifetimes-q3)
  - [Q4. What is the difference between Singleton, Scoped, and Transi…](#chapter-04-dependency-injection-service-lifetimes-q4)
  - [Q5. When should you register a service as Scoped?](#chapter-04-dependency-injection-service-lifetimes-q5)
  - [Q6. What is a "captive dependency," and why is it a problem?](#chapter-04-dependency-injection-service-lifetimes-q6)
  - [Q7. How does ASP.NET Core create a scope per HTTP request?](#chapter-04-dependency-injection-service-lifetimes-q7)
  - [Q8. What happens when you register the same interface twice?](#chapter-04-dependency-injection-service-lifetimes-q8)
  - [Q9. What is `IHttpClientFactory`, and why should you use it inst…](#chapter-04-dependency-injection-service-lifetimes-q9)
  - [Q10. What are keyed services in .NET 8?](#chapter-04-dependency-injection-service-lifetimes-q10)
  - [Q11. What is `IServiceScopeFactory`, and when do you need it?](#chapter-04-dependency-injection-service-lifetimes-q11)
  - [Q12. What is `IDbContextFactory<TContext>`, and when is it prefer…](#chapter-04-dependency-injection-service-lifetimes-q12)
  - [Q13. What do `ValidateOnBuild` and `ValidateScopes` do?](#chapter-04-dependency-injection-service-lifetimes-q13)
  - [Q14. How do you register an interface with its implementation?](#chapter-04-dependency-injection-service-lifetimes-q14)
  - [Q15. What is constructor injection?](#chapter-04-dependency-injection-service-lifetimes-q15)
  - [Q16. Can you inject a Scoped service into a Singleton? What happe…](#chapter-04-dependency-injection-service-lifetimes-q16)
  - [Q17. What is the difference between `AddSingleton`, `AddScoped`, …](#chapter-04-dependency-injection-service-lifetimes-q17)
  - [Q18. How does DI work in Minimal API route handlers?](#chapter-04-dependency-injection-service-lifetimes-q18)
- [Gotchas](#gotchas)
- [Scenario-Based Questions](#scenario-based-questions-karat-format)

---

## Chapter 04. Dependency Injection & Service Lifetimes

### Q1. What is dependency injection in ASP.NET Core? {#chapter-04-dependency-injection-service-lifetimes-q1}

What is dependency injection in ASP.NET Core?

**Answer:** Dependency injection (DI) is a design pattern where a class receives its collaborators through constructor parameters (or properties) rather than creating them with `new`. ASP.NET Core ships with a built-in DI container that registers services in `Program.cs` and resolves them automatically for controllers, middleware, Minimal API handlers, and your own types.

- The framework promotes loose coupling: application code depends on abstractions (`IOrderRepository`) while concrete types (`SqlOrderRepository`) are wired at composition root.
- Registration happens during the builder phase (`builder.Services.Add...`); resolution happens when a type is first needed at runtime.
- Constructor injection is the default and recommended style because required dependencies are explicit and the object is always in a valid state after construction.
- ASP.NET Core also supports method injection in Minimal APIs and `[FromServices]` for action parameters when constructor injection is awkward.

---

### Q2. What is the built-in DI container in ASP.NET Core? {#chapter-04-dependency-injection-service-lifetimes-q2}

What is the built-in DI container in ASP.NET Core?

**Answer:** ASP.NET Core includes a lightweight, built-in service provider (`Microsoft.Extensions.DependencyInjection`) that implements `IServiceProvider` and `IServiceScopeFactory`. It is not a full-featured container like Autofac, but it covers constructor injection, lifetimes, open generics, factory delegates, keyed services (.NET 8+), and `IEnumerable<T>` multi-registration.

- Services are described as `ServiceDescriptor` entries in an `IServiceCollection`; `builder.Build()` materializes the root `IServiceProvider`.
- The default provider is replaceable via `Host.UseServiceProviderFactory` if you need advanced features such as property injection or conditional registration.
- Most ASP.NET Core framework services (`ILogger<T>`, `IConfiguration`, `DbContext`, `IHttpClientFactory`) are registered against this same container.
- Third-party containers integrate by adapting their factory to populate the same `IServiceCollection` during host setup.

---

### Q3. What are the three service lifetimes in ASP.NET Core DI? {#chapter-04-dependency-injection-service-lifetimes-q3}

What are the three service lifetimes in ASP.NET Core DI?

**Answer:** The built-in container supports three registration lifetimes: **Transient**, **Scoped**, and **Singleton**. Each lifetime controls how often the container creates a new instance when resolving a given service type.

- **Transient** — a new instance is created every time the service is resolved from a scope or the root provider.
- **Scoped** — one instance per DI scope; in web apps the request scope is the common case.
- **Singleton** — one instance for the lifetime of the root service provider (typically the entire application process).
- Choosing the wrong lifetime is a common source of bugs involving shared mutable state, disposed dependencies, or thread-safety issues.

---

### Q4. What is the difference between Singleton, Scoped, and Transient? {#chapter-04-dependency-injection-service-lifetimes-q4}

What is the difference between Singleton, Scoped, and Transient?

**Answer:** The difference is how long an instance lives and how many instances exist when the same service is resolved multiple times. Singleton shares one instance process-wide, Scoped shares one instance per scope (usually one HTTP request), and Transient never shares instances across resolutions.

| Lifetime | Instance sharing | Typical use |
|---|---|---|
| **Singleton** | One per application | Caches, configuration facades, `IHttpClientFactory`, stateless services |
| **Scoped** | One per scope (request) | `DbContext`, unit-of-work repositories, per-user request state |
| **Transient** | New every resolve | Lightweight stateless helpers, mappers, small strategy objects |

- A Singleton must not hold mutable per-request state because concurrent requests share the same object.
- Scoped services align with Entity Framework Core's `DbContext`, which is designed to be short-lived and not thread-safe across requests.
- Transient is safest when the type is cheap to construct and carries no shared state, though excessive transient resolution in hot paths can add allocation pressure.

---

### Q5. When should you register a service as Scoped? {#chapter-04-dependency-injection-service-lifetimes-q5}

When should you register a service as Scoped?

**Answer:** Register a service as Scoped when it should live for the duration of a single unit of work — most often one HTTP request — and must not be shared across concurrent requests or the entire process. This is the default choice for data access, business services that use `DbContext`, and any type that holds request-specific state.

- Entity Framework Core `DbContext` is registered scoped because it tracks entities for one operation and is not thread-safe.
- Repositories and application services that depend on scoped `DbContext` should also be scoped so they share the same context instance within a request.
- Per-request user context (`ICurrentUserService`) belongs in scoped lifetime so each caller sees the correct identity without cross-user leakage.
- Do not register mutable scoped services as Singleton unless you deliberately create a new scope per operation with `IServiceScopeFactory`.

---

### Q6. What is a "captive dependency," and why is it a problem? {#chapter-04-dependency-injection-service-lifetimes-q6}

What is a "captive dependency," and why is it a problem?

**Answer:** A captive dependency occurs when a longer-lived service (typically a Singleton) holds a reference to a shorter-lived service (typically Scoped or Transient) beyond that dependency's intended lifetime. The long-lived consumer "captures" an instance that may be disposed, stale, or shared incorrectly across unrelated work units.

- Classic example: injecting scoped `AppDbContext` into a singleton `BackgroundService` — the context outlives its scope and throws `ObjectDisposedException` under load.
- Captive dependencies also cause subtle data bugs when a scoped object retains state from a previous request because it was resolved once at the root provider.
- ASP.NET Core can detect some cases when `ValidateScopes` is enabled on the default service provider, throwing at startup instead of failing in production.
- The fix is to match lifetimes correctly or create an explicit scope per operation with `IServiceScopeFactory.CreateScope()`.

---

### Q7. How does ASP.NET Core create a scope per HTTP request? {#chapter-04-dependency-injection-service-lifetimes-q7}

How does ASP.NET Core create a scope per HTTP request?

**Answer:** When a request enters the pipeline, the host creates a request `IServiceScope` that wraps the root service provider. Middleware, endpoint routing, controllers, and Minimal API handlers resolve scoped services from this scope so each request gets its own instances.

- The scope is established early in the pipeline and disposed when the request completes, which triggers disposal of scoped `IDisposable`/`IAsyncDisposable` services such as `DbContext`.
- `HttpContext.RequestServices` exposes the scoped provider for the current request; framework components resolve from there automatically.
- Singleton services resolved during a request still come from the root provider, but if they incorrectly depend on scoped services, the captive dependency problem appears.
- Background work triggered during a request must not capture scoped services in fire-and-forget tasks — create a new scope inside the background work instead.

---

### Q8. What happens when you register the same interface twice? {#chapter-04-dependency-injection-service-lifetimes-q8}

What happens when you register the same interface twice?

**Answer:** Multiple registrations for the same service type append descriptors to the container rather than replacing earlier entries. Resolving the interface directly returns the **last** registered implementation, while resolving `IEnumerable<TInterface>` injects all registered implementations in registration order.

- `builder.Services.AddSingleton<IMailer, SmtpMailer>();` followed by `AddSingleton<IMailer, SendGridMailer>()` means a single `IMailer` resolve yields `SendGridMailer`.
- `IEnumerable<IMailer>` activates both implementations — useful for composite handlers, plugin pipelines, or strategy chains.
- For named implementations in .NET 8+, keyed services (`AddKeyedSingleton`) avoid ambiguity when you need a specific implementation by key.
- Duplicate registration surprises teams that expect last registration to be the only one visible to `IEnumerable<T>` — both remain unless removed explicitly.

---

### Q9. What is `IHttpClientFactory`, and why should you use it instead of `new HttpClient()`? {#chapter-04-dependency-injection-service-lifetimes-q9}

What is `IHttpClientFactory`, and why should you use it instead of `new HttpClient()`?

**Answer:** `IHttpClientFactory` is a framework service that creates and manages `HttpClient` instances with correctly pooled `HttpMessageHandler` lifetimes. Using `new HttpClient()` in long-lived services causes socket exhaustion because handlers are not recycled; disposing a shared static `HttpClient` causes a different set of DNS and handler staleness problems.

- Register named or typed clients with `AddHttpClient("payments", c => c.BaseAddress = ...)` and inject `IHttpClientFactory` or a typed client interface.
- Handlers rotate on a timer (default two minutes), which avoids stale DNS entries while still reusing connections efficiently.
- Typed clients (`AddHttpClient<IPaymentGateway, PaymentGateway>()`) combine DI with sensible defaults for base address, headers, and resilience policies.
- `IHttpClientFactory` integrates with Polly for retries, circuit breakers, and timeouts without manual handler lifecycle management.

---

### Q10. What are keyed services in .NET 8? {#chapter-04-dependency-injection-service-lifetimes-q10}

What are keyed services in .NET 8?

**Answer:** Keyed services let you register multiple implementations of the same interface distinguished by an object key (string or other type) and resolve the correct one with `[FromKeyedServices("key")]` or `GetRequiredKeyedService<T>(key)`. They replace fragile "register last wins" patterns when you need named providers such as multiple payment gateways or storage backends.

- Register with `builder.Services.AddKeyedScoped<IPaymentProcessor, StripeProcessor>("stripe")` and `AddKeyedScoped<IPaymentProcessor, PayPalProcessor>("paypal")`.
- Inject in Minimal APIs or controllers: `([FromKeyedServices("stripe")] IPaymentProcessor processor)`.
- Keys are resolved from the active scope's provider, so keyed scoped services follow the same request scope rules as ordinary scoped services.
- Keyed services reduce the need for separate marker interfaces or factory classes when the only difference is which implementation to select at runtime.

---

### Q11. What is `IServiceScopeFactory`, and when do you need it? {#chapter-04-dependency-injection-service-lifetimes-q11}

What is `IServiceScopeFactory`, and when do you need it?

**Answer:** `IServiceScopeFactory` creates new DI scopes on demand from code that runs outside an existing request scope, such as singleton hosted services, queue workers, or manual background tasks. It is registered as a singleton and is the supported way to resolve scoped services from long-lived components.

- Call `using var scope = _scopeFactory.CreateScope();` then resolve scoped services from `scope.ServiceProvider`.
- Typical pattern in `BackgroundService`: one scope per queued message or timer tick so each unit of work gets a fresh `DbContext`.
- Without a scope factory, injecting scoped services into singletons creates captive dependencies or startup validation failures.
- Always dispose the scope (`using`) so scoped disposables are cleaned up promptly after the work unit finishes.

---

### Q12. What is `IDbContextFactory<TContext>`, and when is it preferred over injecting `DbContext` directly? {#chapter-04-dependency-injection-service-lifetimes-q12}

What is `IDbContextFactory<TContext>`, and when is it preferred over injecting `DbContext` directly?

**Answer:** `IDbContextFactory<TContext>` creates short-lived `DbContext` instances explicitly, which is ideal when work is not tied to a single HTTP request scope — for example, parallel operations in a singleton service, Blazor Server circuits, or background workers. Injecting `DbContext` directly remains correct for typical request-scoped web API code.

- Register with `AddDbContextFactory<AppDbContext>(options => ...)` alongside or instead of scoped `AddDbContext` depending on consumption patterns.
- Each `CreateDbContext()` (or `await CreateDbContextAsync()`) yields a context you own and must dispose, usually wrapped in `using`.
- Prefer the factory when multiple contexts are needed concurrently in one logical operation — a single scoped `DbContext` is not thread-safe for parallel queries.
- For ordinary controller actions, scoped `DbContext` injection is simpler and aligns with one context per request.

---

### Q13. What do `ValidateOnBuild` and `ValidateScopes` do? {#chapter-04-dependency-injection-service-lifetimes-q13}

What do `ValidateOnBuild` and `ValidateScopes` do?

**Answer:** These are optional checks on the default service provider that fail fast at startup instead of at runtime under load. `ValidateOnBuild` attempts to resolve the entire service graph when the provider is built, and `ValidateScopes` throws when a singleton (or root scope) tries to capture a scoped service directly.

- Enable in ASP.NET Core 8 with `builder.Host.UseDefaultServiceProvider((context, options) => { options.ValidateOnBuild = true; options.ValidateScopes = true; })`.
- `ValidateOnBuild` catches missing registrations such as an unregistered `IOptions<SmtpSettings>` before traffic arrives.
- `ValidateScopes` exposes captive dependencies like scoped `DbContext` injected into a singleton `CacheService` at root resolution.
- They add a small startup cost but are valuable in CI, staging, and local development to prevent DI lifetime mistakes from reaching production.

---

### Q14. How do you register an interface with its implementation? {#chapter-04-dependency-injection-service-lifetimes-q14}

How do you register an interface with its implementation?

**Answer:** Use extension methods on `IServiceCollection` that pair the abstraction with the concrete type and lifetime: `builder.Services.AddScoped<IOrderRepository, SqlOrderRepository>()`. The container resolves `IOrderRepository` to `SqlOrderRepository` wherever that interface is requested.

- Choose lifetime explicitly: `AddSingleton`, `AddScoped`, or `AddTransient` as appropriate for the implementation's state and dependencies.
- You can also register the concrete type alone (`AddScoped<OrderService>()`) when no interface is needed, though interface-based registration improves testability.
- Factory registration `AddScoped<IRepo>(sp => new SqlRepo(sp.GetRequiredService<...>()))` is used when construction logic is non-trivial.
- Open generic registration (`AddScoped(typeof(IRepository<>), typeof(EfRepository<>))`) supports generic repository patterns without per-entity boilerplate.

---

### Q15. What is constructor injection? {#chapter-04-dependency-injection-service-lifetimes-q15}

What is constructor injection?

**Answer:** Constructor injection supplies a class's dependencies through its public constructor parameters, and the DI container chooses an constructor and fills each parameter with registered services. It is the primary injection style in ASP.NET Core because dependencies are required, visible, and immutable after construction.

- ASP.NET Core activates controllers, Minimal API handlers, middleware, and your services by reflecting on the constructor with the most resolvable parameters.
- Multiple constructors are supported, but only one will be used — ambiguous or unsatisfiable constructors cause activation failures at runtime (or at build with validation).
- Optional dependencies can use default parameter values or `IOptions<T>` with defaults, but required services should not be resolved manually inside the constructor body.
- Property injection exists in some third-party containers but is rarely needed in ASP.NET Core's built-in provider.

---

### Q16. Can you inject a Scoped service into a Singleton? What happens? {#chapter-04-dependency-injection-service-lifetimes-q16}

Can you inject a Scoped service into a Singleton? What happens?

**Answer:** You should not inject a scoped service directly into a singleton constructor because the singleton outlives any request scope. With `ValidateScopes` enabled, the application throws at startup; without validation, the scoped instance may be created from the root provider, leading to `ObjectDisposedException`, stale state, or cross-request data leakage.

- The container treats resolving a scoped service from the root provider as a captive dependency when validation is on.
- Correct patterns: make the consumer scoped, inject `IServiceScopeFactory` and create a scope per operation, or use `IDbContextFactory<T>` for database access from singleton workers.
- Middleware is scoped per request and can safely consume scoped services; singleton hosted services cannot without explicit scoping.
- This rule applies equally to your own services and to framework abstractions such as `IOptionsSnapshot<T>` that are registered scoped.

---

### Q17. What is the difference between `AddSingleton`, `AddScoped`, and `AddTransient`? {#chapter-04-dependency-injection-service-lifetimes-q17}

What is the difference between `AddSingleton`, `AddScoped`, and `AddTransient`?

**Answer:** These three methods register the same abstraction-to-implementation mapping but assign different lifetimes to the created instance. The method name states how long the container caches and reuses the instance when the service is resolved repeatedly.

- `AddSingleton<TService, TImplementation>()` — one shared instance for the application process; thread-safe stateless services only unless you synchronize mutable state deliberately.
- `AddScoped<TService, TImplementation>()` — one instance per DI scope; in web apps this aligns with one HTTP request.
- `AddTransient<TService, TImplementation>()` — a fresh instance on every resolution, even multiple times within the same request.
- All three support overloads for instances, factories, and open generics; lifetime is the only behavioral difference among them.

---

### Q18. How does DI work in Minimal API route handlers? {#chapter-04-dependency-injection-service-lifetimes-q18}

How does DI work in Minimal API route handlers?

**Answer:** Minimal API route handlers participate in the same DI container as controllers: parameters whose types are registered services are resolved automatically from the request scope when the endpoint executes. You declare dependencies as handler parameters alongside route, query, and body bindings.

- Example: `app.MapGet("/orders/{id}", async (int id, IOrderService orders) => ...)` injects `IOrderService` from `HttpContext.RequestServices`.
- Use `[FromKeyedServices("key")]` for keyed services and `[FromServices]` when disambiguation is needed with other binding sources.
- Scoped services resolve correctly because each request creates a scope before the handler runs.
- `TypedResults`, validation filters, and endpoint filters can also receive injected services through constructor injection on filter types registered in DI.

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

#### Q1. (M) You register two payment gateways: `AddSingleton<IPaymentProcessor, StripeProcessor>()` and `AddSingleton<IPaymentProcessor, PayPalProcessor>()`. Injecting `IPaymentProcessor` resolves one implementation; injecting `IEnumerable<IPaymentProcessor>` resolves both. How does the container behave, and when does single-interface injection silently surprise you?

---

**Answer:**

**Answer:** The built-in container stores multiple descriptors for the same service type; resolving `IPaymentProcessor` returns the **last** registration, while `IEnumerable<IPaymentProcessor>` injects a composite enumerating all registrations in registration order.

- Duplicate `AddSingleton<IPaymentProcessor, T>()` calls append descriptors — not overwrite.
- Single-interface injection is **last-wins** — PayPal would win in the example unless registration order is intentional.
- `IEnumerable<IPaymentProcessor>` activates all implementations — use for plugin fan-out, composite health checks, or strategy chains.
- Alternatives when you need named impls: **keyed services** (.NET 8+), factory delegates, or separate interfaces (`IStripeProcessor`, `IPayPalProcessor`).
- Decorator pattern: register decorator as the single `TInterface` and inject inner as concrete or keyed type to avoid ambiguity.

**Production takeaway:** Multi-provider designs fail silently when developers inject `TInterface` expecting "all gateways" — Karat tests `IEnumerable<T>` vs single `T` from debrief.

---

---

#### Q2. (M) How do `ValidateScopes` and `ValidateOnBuild` on `UseDefaultServiceProvider()` catch captive dependencies and missing registrations at startup rather than under load?

---

**Answer:**

**Answer:** `ValidateOnBuild` resolves the entire service graph at startup to surface missing registrations; `ValidateScopes` throws when a singleton (or root scope) resolves a scoped service directly — exposing captive dependencies like scoped `DbContext` in a singleton hosted service.

- Configure on the host builder:

```csharp
builder.Host.UseDefaultServiceProvider((context, options) =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});
```

- `ValidateOnBuild` — first chance to catch `Unable to resolve service for type 'IOptions<SmtpOptions>'` before traffic.
- `ValidateScopes` — catches singleton constructor injecting scoped `AppDbContext` at root provider creation.
- Does not replace fixing architecture — still use `IServiceScopeFactory` per operation in background workers.
- Enable in CI/staging — small startup cost, large production save.

**Production takeaway:** Apps that "start fine" locally but fail under load often skipped these flags — debrief favorite for captive dependency detection.

---

---

#### Q3. (R) In an e-commerce app, review this cart service registered as singleton. What production failures appear under concurrent shoppers and multi-instance deployment?

```csharp
builder.Services.AddSingleton<ICartService, CartService>();

public class CartService : ICartService
{
    private readonly List<CartItem> _items = new();
    public void AddItem(CartItem item) => _items.Add(item);
    public IReadOnlyList<CartItem> GetItems() => _items;
}
```

---

**Answer:**

**Answer:** A singleton `CartService` with an instance `_items` list shares one cart across every user and request — cross-user leakage, race corruption, and inconsistent carts when scaled to multiple instances.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| DI lifetime | `AddSingleton` + mutable `_items` field | All users share one cart — privacy and billing incidents |
| Concurrency | `List<T>` mutated without synchronization | Corrupted collection or lost items under load |
| Scale-out | In-memory state is per-process | Cart differs per pod; sticky sessions fail |
| Design | No user/session key on cart data | Cannot distinguish shoppers |

**Fix (priority order):**

1. Register **scoped** `ICartService` keyed by request + user identity, or remove in-memory cart from singleton entirely.
2. Persist cart lines in Redis/SQL with `userId` / session id for multi-instance deployments.
3. Keep singleton services stateless — pass identity into scoped services that load/store cart data.

```csharp
builder.Services.AddScoped<ICartService, CartService>(); // + user id in service API
// or distributed: ICartStore backed by Redis, singleton orchestrator only
```

**Production takeaway:** E-commerce lifetime mistakes are severity-1 — wrong charges and privacy incidents, not benign test noise.

---

---

#### Q4. (R) A scoped `DbContext` is constructor-injected into a singleton `BackgroundService` that processes a queue. The app starts but throws `ObjectDisposedException` under load. Explain the captive dependency and fix it.

```csharp
public class OrderQueueWorker : BackgroundService
{
    private readonly AppDbContext _db;
    public OrderQueueWorker(AppDbContext db) => _db = db;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var order = await DequeueAsync();
            _db.Orders.Add(order);
            await _db.SaveChangesAsync(stoppingToken);
        }
    }
}
```

---

**Answer:**

**Answer:** The singleton outlives any request scope; the scoped `DbContext` is created once at root scope and disposed when that scope ends — the worker holds a disposed context (captive dependency), causing intermittent `ObjectDisposedException` on queue processing.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| DI lifetime | Scoped `DbContext` in singleton `BackgroundService` | Context disposed while worker runs |
| Runtime | `ObjectDisposedException` on deferred saves | Poison messages, data loss |
| Design | One context shared across unrelated orders | Stale change tracker, wrong commits |
| Observability | May start without `ValidateScopes` in dev | Bug reaches prod under timing |

**Fix (priority order):**

1. Inject `IServiceScopeFactory` (or `IDbContextFactory<AppDbContext>`) instead of `DbContext`.
2. Create **new scope per message** (or batch), resolve fresh context, dispose scope when done.
3. Enable `ValidateScopes` + `ValidateOnBuild` in staging.

```csharp
await using var scope = _scopeFactory.CreateAsyncScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
_db.Orders.Add(order);
await db.SaveChangesAsync(stoppingToken);
```

**Production takeaway:** Background workers own scope boundaries — request-scoped services never belong in singleton constructors (debrief captive dependency).

---

---

#### Q5. (D) A team caches per-user preferences in `static Dictionary<string, UserPrefs>` inside a singleton service. What breaks under concurrent load, parallel tests, and horizontal scale-out?

---

**Answer:**

**Answer:** Process-wide static mutable state is not thread-safe by default, not shared across instances, and survives across tests in the same process — causing corruption, stale prefs, cross-test pollution, and inconsistent reads after scale-out.

- **Concurrency:** `Dictionary<,>` throws or corrupts under concurrent writes — use `ConcurrentDictionary` or external store, but static still wrong for multi-instance.
- **Scale-out:** Each replica has its own static cache — update on node A invisible on node B until external sync.
- **Parallel tests:** xUnit parallel classes share static — flaky tests unless isolated collections or no static state.
- **Memory:** Unbounded keys → OOM — no eviction tied to session lifetime.
- **Privacy:** Key collision or missing tenant prefix → cross-user preference bleed.

**Recommended:** `IMemoryCache` with tenant-scoped keys + TTL, or Redis/SQL per user — resolve via scoped service per request.

**Production takeaway:** Static mutable caches in singletons are debrief Heisenbugs — appear under parallel tests or second replica, not solo dev runs.

---

---

#### Q6. (P) .NET 8 keyed services: you need `"primary"` and `"fallback"` implementations of `INotificationSender` in the same process. How do you register and resolve them without ambiguous `INotificationSender` injection?

---

**Answer:**

**Answer:** Use keyed DI — register with `AddKeyedSingleton<INotificationSender, EmailSender>("primary")` and resolve via `[FromKeyedServices("primary")]`, `GetRequiredKeyedService`, or keyed factory delegate — avoids last-wins single registration.

```csharp
builder.Services.AddKeyedSingleton<INotificationSender, EmailSender>("primary");
builder.Services.AddKeyedSingleton<INotificationSender, SmsSender>("fallback");

public class AlertService(
    [FromKeyedServices("primary")] INotificationSender primary,
    [FromKeyedServices("fallback")] INotificationSender fallback) { }
```

- Unkeyed `INotificationSender` injection remains ambiguous if multiple unkeyed registrations exist — prefer keyed only or single default + keyed alternates.
- Keyed services work with singleton/scoped/transient lifetimes per key.
- Alternative pre-.NET 8: separate interfaces, factory pattern, or `IServiceProvider` locator (discouraged).

**Production takeaway:** Keyed services replace hacky `IEnumerable<T>` indexing when you need **named implementations**, not all implementations.

---

---

#### Q7. (R) Review this outbound HTTP integration. Memory usage climbs until OOM and DNS changes never apply. What is wrong and what replaces `new HttpClient()`?

```csharp
public class PricingService : IPricingService
{
    public async Task<decimal> GetPriceAsync(int sku, CancellationToken ct)
    {
        using var client = new HttpClient();
        client.BaseAddress = new Uri("https://pricing.internal");
        var json = await client.GetStringAsync($"/api/prices/{sku}", ct);
        return JsonSerializer.Deserialize<PriceDto>(json)!.Amount;
    }
}
```

*(Registered as `AddScoped<IPricingService, PricingService>()` — called on every product page view.)*

---

**Answer:**

**Answer:** Instantiating `HttpClient` per call exhausts ephemeral ports and socket handles under load and ignores DNS TTL — register a typed client with `IHttpClientFactory` (or `AddHttpClient`) so handlers and connections are pooled and DNS refreshes apply.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Resource | `new HttpClient()` per request in scoped service | Socket exhaustion, SNAT port fatigue |
| DNS | Disposed client pattern still recreated each call | Stale DNS after failover |
| DI | Should use typed `HttpClient` from factory | Missing Polly, logging, correlation handlers |
| Performance | TLS handshake repeated | Latency and CPU spike on catalog pages |

**Fix (priority order):**

1. Register typed client: `builder.Services.AddHttpClient<IPricingService, PricingService>(c => c.BaseAddress = new Uri("..."));`
2. Inject `HttpClient` via constructor on `PricingService` — do not dispose injected client.
3. Add resilience handler (timeout, retry) via Polly integration if needed.

```csharp
public class PricingService(HttpClient http) : IPricingService
{
    public async Task<decimal> GetPriceAsync(int sku, CancellationToken ct)
    {
        var json = await http.GetStringAsync($"/api/prices/{sku}", ct);
        return JsonSerializer.Deserialize<PriceDto>(json)!.Amount;
    }
}
```

**Production takeaway:** `new HttpClient()` in ASP.NET Core services is a classic Karat review — factory is mandatory for production outbound calls.

---

---

#### Q8. (P) When should you inject `IServiceScopeFactory` vs `IDbContextFactory<AppDbContext>` vs a custom `Func<IServiceProvider, T>` factory? Compare a nightly batch job vs per-message queue worker vs on-demand controller action.

---

**Answer:**

**Answer:** Controllers use natural request scope for scoped services; long-running singleton workers use `IServiceScopeFactory` to create scopes per unit of work; high-throughput DbContext creation uses `IDbContextFactory` for lightweight context spin-up without full scope overhead.

| Scenario | Tool | Why |
|---|---|---|
| Controller action | Inject scoped `AppDbContext` / services directly | Request scope matches HTTP lifetime |
| Per-message queue worker | `IServiceScopeFactory.CreateAsyncScope()` | Full scoped graph per message |
| Nightly batch touching many aggregates | `IServiceScopeFactory` per batch/chunk | Dispose contexts between batches |
| High-frequency short DB reads | `IDbContextFactory<AppDbContext>` | Pool-friendly context creation |
| Complex optional dependency | Custom factory delegate registration | Encapsulate creation logic |

- `IServiceScopeFactory` resolves **any** scoped service graph — use when worker needs repository + context + unit of work together.
- `IDbContextFactory` when only EF context needed with explicit `await using var ctx = await factory.CreateDbContextAsync()`.
- Avoid `Func<IServiceProvider,T>` unless simplifying tests or multi-implementation selection — prefer typed factories.

**Production takeaway:** Factory choice is about **scope boundary granularity**, not preference — wrong choice revives captive dependency bugs.

---

---

#### Q9. (R) Review DI registration for a decorator-style audit logger. Only `ConsoleAuditLogger` ever runs; `FileAuditLogger` is never invoked. What registration mistake caused this?

```csharp
builder.Services.AddSingleton<IAuditLogger, FileAuditLogger>();
builder.Services.AddSingleton<IAuditLogger, ConsoleAuditLogger>();
builder.Services.AddSingleton<IOrderService, OrderService>();

public class OrderService(IOrderRepository repo, IAuditLogger audit) : IOrderService
{
    public async Task PlaceOrderAsync(Order order)
    {
        await repo.AddAsync(order);
        audit.Log($"Placed {order.Id}");
    }
}
```

---

**Answer:**

**Answer:** Two `AddSingleton<IAuditLogger, ...>()` registrations make single `IAuditLogger` injection **last-wins** — only `ConsoleAuditLogger` resolves; `FileAuditLogger` is orphaned unless you inject `IEnumerable<IAuditLogger>` or use a composite/decorator registration.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| DI | Duplicate interface registration — last wins | File audit never runs — compliance gap |
| Design | Expected decorator chain but registered peer implementations | Silent loss of audit sink |
| Observability | Console shows logs; file empty in prod | Incident reconstruction fails |
| Architecture | Misunderstanding of container multi-registration rules | Repeat across services |

**Fix (priority order):**

1. **Composite:** Register `IAuditLogger` as `CompositeAuditLogger(IEnumerable<IAuditLogger> loggers)` after registering concretes as themselves or keyed.
2. **Decorator:** `services.AddSingleton<IAuditLogger, FileAuditLogger>(); services.Decorate<IAuditLogger, ConsoleAuditLogger>();` (with Scrutor or manual factory).
3. **Explicit:** Inject `IEnumerable<IAuditLogger>` in `OrderService` if fan-out intended.

```csharp
builder.Services.AddSingleton<FileAuditLogger>();
builder.Services.AddSingleton<ConsoleAuditLogger>();
builder.Services.AddSingleton<IAuditLogger>(sp =>
    new CompositeAuditLogger(new IAuditLogger[]
    {
        sp.GetRequiredService<FileAuditLogger>(),
        sp.GetRequiredService<ConsoleAuditLogger>()
    }));
```

**Production takeaway:** Multiple `Add*` for same interface does not create a chain — debrief `IEnumerable<T>` vs single `T` applied to audit logging.

---

---

#### Q10. (R) Review lifetimes for a read-heavy catalog API. Intermittent wrong prices and stale inventory appear after deploy. Identify lifetime mismatches.

```csharp
builder.Services.AddSingleton<CatalogCache>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddSingleton<ICatalogService, CatalogService>();

public class CatalogService(CatalogCache cache, IProductRepository repo) : ICatalogService
{
    public async Task<Product?> GetAsync(int id) =>
        cache.Get(id) ?? cache.Store(id, await repo.GetByIdAsync(id));
}

public class CatalogCache
{
    private readonly Dictionary<int, Product> _items = new();
    public Product? Get(int id) => _items.GetValueOrDefault(id);
    public Product Store(int id, Product p) => _items[id] = p;
}
```

*( `ProductRepository` holds scoped `DbContext`; `CatalogService` is singleton. )*

**Answer:**

**Answer:** `CatalogService` is singleton but depends on scoped `IProductRepository` (captive dependency) and holds a process-wide mutable cache — stale/wrong data and disposed-context races under load.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| DI lifetime | Singleton `CatalogService` → scoped `IProductRepository` | Captive `DbContext`; disposed or stale tracker |
| State | Singleton `CatalogCache` with mutable `Dictionary` | Never evicts; wrong prices after updates |
| Concurrency | Unsynchronized dictionary writes | Corruption under parallel requests |
| Scale-out | In-memory cache per instance | Inconsistent catalog across pods |

**Fix (priority order):**

1. Make `ICatalogService` **scoped** (or singleton with `IDbContextFactory` + no scoped repo injection).
2. Replace static dictionary with `IMemoryCache` (TTL) or distributed cache for shared invalidation.
3. Enable `ValidateScopes` to catch singleton→scoped at startup.

```csharp
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
// Remove singleton CatalogService + captive cache field pattern
```

**Production takeaway:** Read-heavy APIs often "optimize" with singleton caches and create **lifetime + staleness** bugs — Karat stacks DI and caching defects in one snippet.

---

---
