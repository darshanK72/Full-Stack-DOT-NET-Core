# Dependency Injection & Service Lifetimes — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is dependency injection in ASP.NET Core?](#q1-what-is-dependency-injection-in-aspnet-core)
2. [Q2. What is the built-in DI container in ASP.NET Core?](#q2-what-is-the-built-in-di-container-in-aspnet-core)
3. [Q3. What are the three service lifetimes in ASP.NET Core DI?](#q3-what-are-the-three-service-lifetimes-in-aspnet-core-di)
4. [Q4. What is the difference between Singleton, Scoped, and Transient?](#q4-what-is-the-difference-between-singleton-scoped-and-transient)
5. [Q5. When should you register a service as Scoped?](#q5-when-should-you-register-a-service-as-scoped)
6. [Q6. What is a "captive dependency," and why is it a problem?](#q6-what-is-a-captive-dependency-and-why-is-it-a-problem)
7. [Q7. How does ASP.NET Core create a scope per HTTP request?](#q7-how-does-aspnet-core-create-a-scope-per-http-request)
8. [Q8. What happens when you register the same interface twice?](#q8-what-happens-when-you-register-the-same-interface-twice)
9. [Q9. What is `IHttpClientFactory`, and why should you use it instead of `new HttpClient()`?](#q9-what-is-ihttpclientfactory-and-why-should-you-use-it-instead-of-new-httpclient)
10. [Q10. What are keyed services in .NET 8?](#q10-what-are-keyed-services-in-net-8)
11. [Q11. What is `IServiceScopeFactory`, and when do you need it?](#q11-what-is-iservicescopefactory-and-when-do-you-need-it)
12. [Q12. What is `IDbContextFactory<TContext>`, and when is it preferred over injecting `DbContext` directly?](#q12-what-is-idbcontextfactorytcontext-and-when-is-it-preferred-over-injecting-dbcontext-directly)
13. [Q13. What do `ValidateOnBuild` and `ValidateScopes` do?](#q13-what-do-validateonbuild-and-validatescopes-do)
14. [Q14. How do you register an interface with its implementation?](#q14-how-do-you-register-an-interface-with-its-implementation)
15. [Q15. What is constructor injection?](#q15-what-is-constructor-injection)
16. [Q16. Can you inject a Scoped service into a Singleton? What happens?](#q16-can-you-inject-a-scoped-service-into-a-singleton-what-happens)
17. [Q17. What is the difference between `AddSingleton`, `AddScoped`, and `AddTransient`?](#q17-what-is-the-difference-between-addsingleton-addscoped-and-addtransient)
18. [Q18. How does DI work in Minimal API route handlers?](#q18-how-does-di-work-in-minimal-api-route-handlers)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What is dependency injection in ASP.NET Core?

**Concepts**
- Constructor injection over object creation with `new`
- Composition root in `Program.cs`
- Abstraction-to-concrete wiring for loose coupling
- `IServiceCollection` registration and runtime resolution
- `[FromServices]` and method injection as alternatives

**Answer**

Dependency injection is a design pattern where a class receives its collaborators through constructor parameters rather than creating them with `new`, which means application code depends on abstractions (`IOrderRepository`) while concrete types (`SqlOrderRepository`) are wired at a single composition root. ASP.NET Core ships with a built-in DI container that registers services in `Program.cs` and resolves them automatically for controllers, middleware, and Minimal API handlers. Registration happens during the builder phase with `builder.Services.Add...`; resolution happens when a type is first needed at runtime. Constructor injection is the default and recommended style because required dependencies are explicit and the object is always in a valid state after construction. ASP.NET Core also supports method injection in Minimal APIs and `[FromServices]` for action parameters when constructor injection is awkward.

---

## Q2. What is the built-in DI container in ASP.NET Core?

**Concepts**
- `Microsoft.Extensions.DependencyInjection` as the default provider
- `IServiceProvider` and `IServiceScopeFactory` contracts
- `ServiceDescriptor` as the registration unit
- Keyed services and `IEnumerable<T>` multi-registration
- `UseServiceProviderFactory` for third-party container integration

**Answer**

ASP.NET Core includes a lightweight built-in service provider (`Microsoft.Extensions.DependencyInjection`) that implements `IServiceProvider` and `IServiceScopeFactory`. It is not a full-featured container like Autofac, but it covers constructor injection, lifetimes, open generics, factory delegates, keyed services (.NET 8+), and `IEnumerable<T>` multi-registration. Services are described as `ServiceDescriptor` entries in an `IServiceCollection`, and `builder.Build()` materializes the root `IServiceProvider`. The default provider is replaceable via `Host.UseServiceProviderFactory` when you need advanced features such as property injection or conditional registration. Most ASP.NET Core framework services — `ILogger<T>`, `IConfiguration`, `DbContext`, `IHttpClientFactory` — are registered against this same container, so third-party containers integrate by adapting their factory to populate the same `IServiceCollection` during host setup.

---

## Q3. What are the three service lifetimes in ASP.NET Core DI?

**Concepts**
- Transient — new instance per resolution
- Scoped — one instance per DI scope
- Singleton — one instance per application process
- Lifetime mismatches as a source of concurrency and disposal bugs

**Answer**

The built-in container supports three registration lifetimes: Transient, Scoped, and Singleton, and each controls how often the container creates a new instance when resolving a given service type. Transient creates a new instance every time the service is resolved from a scope or the root provider. Scoped creates one instance per DI scope, and in web apps the request scope is the common case. Singleton creates one instance for the lifetime of the root service provider, typically the entire application process. Choosing the wrong lifetime is a common source of bugs involving shared mutable state, disposed dependencies, or thread-safety issues.

---

## Q4. What is the difference between Singleton, Scoped, and Transient?

**Concepts**
- Instance sharing across resolutions as the key distinction
- Singleton thread-safety requirement for mutable state
- Scoped alignment with EF Core `DbContext`
- Transient allocation pressure on hot paths

**Answer**

The difference is how long an instance lives and how many instances exist when the same service is resolved multiple times. Singleton shares one instance process-wide, Scoped shares one instance per scope (usually one HTTP request), and Transient never shares instances across resolutions. A Singleton must not hold mutable per-request state because concurrent requests share the same object. Scoped services align with Entity Framework Core's `DbContext`, which is designed to be short-lived and not thread-safe across requests. Transient is safest when the type is cheap to construct and carries no shared state, though excessive transient resolution in hot paths can add allocation pressure.

| Lifetime | Instance sharing | Typical use |
|---|---|---|
| **Singleton** | One per application | Caches, configuration facades, `IHttpClientFactory`, stateless services |
| **Scoped** | One per scope (request) | `DbContext`, unit-of-work repositories, per-user request state |
| **Transient** | New every resolve | Lightweight stateless helpers, mappers, small strategy objects |

---

## Q5. When should you register a service as Scoped?

**Concepts**
- Unit-of-work lifetime per HTTP request
- EF Core `DbContext` scoped by design
- Per-request user context isolation
- Risk of mutable scoped services becoming Singleton

**Answer**

Register a service as Scoped when it should live for the duration of a single unit of work — most often one HTTP request — and must not be shared across concurrent requests or the entire process. Entity Framework Core `DbContext` is registered scoped because it tracks entities for one operation and is not thread-safe, so repositories and application services that depend on scoped `DbContext` should also be scoped so they share the same context instance within a request. Per-request user context (`ICurrentUserService`) belongs in scoped lifetime so each caller sees the correct identity without cross-user leakage. Do not register mutable scoped services as Singleton unless you deliberately create a new scope per operation with `IServiceScopeFactory`.

---

## Q6. What is a "captive dependency," and why is it a problem?

**Concepts**
- Captive dependency — long-lived service holding short-lived reference
- `ObjectDisposedException` from a disposed scoped service
- Stale state from scoped object resolved once at root provider
- `ValidateScopes` detection at startup
- `IServiceScopeFactory.CreateScope()` as the fix

**Answer**

A captive dependency occurs when a longer-lived service (typically a Singleton) holds a reference to a shorter-lived service (typically Scoped or Transient) beyond that dependency's intended lifetime. The classic example is injecting a scoped `AppDbContext` into a singleton `BackgroundService` — the context outlives its scope and throws `ObjectDisposedException` under load. Captive dependencies also cause subtle data bugs when a scoped object retains state from a previous request because it was resolved once at the root provider, which means unrelated requests share change-tracker state. ASP.NET Core can detect some cases when `ValidateScopes` is enabled on the default service provider, throwing at startup instead of failing in production. The fix is to match lifetimes correctly or create an explicit scope per operation with `IServiceScopeFactory.CreateScope()`.

---

## Q7. How does ASP.NET Core create a scope per HTTP request?

**Concepts**
- Request `IServiceScope` created by the host per incoming request
- `HttpContext.RequestServices` as the scoped provider
- Scope disposal triggering `IDisposable`/`IAsyncDisposable` cleanup
- Fire-and-forget tasks must not capture request-scoped services

**Answer**

When a request enters the pipeline, the host creates a request `IServiceScope` that wraps the root service provider. Middleware, endpoint routing, controllers, and Minimal API handlers resolve scoped services from this scope so each request gets its own instances. The scope is established early in the pipeline and disposed when the request completes, which triggers disposal of scoped `IDisposable`/`IAsyncDisposable` services such as `DbContext`. `HttpContext.RequestServices` exposes the scoped provider for the current request, and framework components resolve from there automatically. Singleton services resolved during a request still come from the root provider, but if they incorrectly depend on scoped services the captive dependency problem appears. Background work triggered during a request must not capture scoped services in fire-and-forget tasks — create a new scope inside the background work instead.

---

## Q8. What happens when you register the same interface twice?

**Concepts**
- Last-registered wins for single-interface resolution
- `IEnumerable<T>` returning all registrations in order
- Keyed services for named implementation disambiguation
- Unexpected behavior with composite or decorator patterns

**Answer**

Multiple registrations for the same service type append descriptors to the container rather than replacing earlier entries. Resolving the interface directly returns the last registered implementation, while resolving `IEnumerable<TInterface>` injects all registered implementations in registration order. For example, `AddSingleton<IMailer, SmtpMailer>()` followed by `AddSingleton<IMailer, SendGridMailer>()` means a single `IMailer` resolve yields `SendGridMailer`. `IEnumerable<IMailer>` activates both implementations, which is useful for composite handlers, plugin pipelines, or strategy chains. For named implementations in .NET 8+, keyed services avoid ambiguity when you need a specific implementation by key. Duplicate registration surprises teams that expect last registration to be the only one visible to `IEnumerable<T>` — both remain unless removed explicitly.

---

## Q9. What is `IHttpClientFactory`, and why should you use it instead of `new HttpClient()`?

**Concepts**
- Socket exhaustion from `new HttpClient()` in long-lived services
- `HttpMessageHandler` pooling and rotation on a timer
- Named and typed client registration patterns
- Polly resilience integration via delegating handlers

**Answer**

`IHttpClientFactory` is a framework service that creates and manages `HttpClient` instances with correctly pooled `HttpMessageHandler` lifetimes. Using `new HttpClient()` in long-lived services causes socket exhaustion because handlers are not recycled, and disposing a shared static `HttpClient` creates a different set of DNS and handler staleness problems. Register named or typed clients with `AddHttpClient("payments", c => c.BaseAddress = ...)` and inject `IHttpClientFactory` or a typed client interface — handlers rotate on a default two-minute timer, which avoids stale DNS entries while still reusing connections efficiently. Typed clients (`AddHttpClient<IPaymentGateway, PaymentGateway>()`) combine DI with sensible defaults for base address, headers, and resilience policies, and `IHttpClientFactory` integrates with Polly for retries, circuit breakers, and timeouts without manual handler lifecycle management.

---

## Q10. What are keyed services in .NET 8?

**Concepts**
- `AddKeyedScoped/Singleton/Transient` with a string or object key
- `[FromKeyedServices("key")]` injection attribute
- `GetRequiredKeyedService<T>(key)` for programmatic resolution
- Replacing last-wins ambiguity for named implementations

**Answer**

Keyed services let you register multiple implementations of the same interface distinguished by an object key (string or other type) and resolve the correct one with `[FromKeyedServices("key")]` or `GetRequiredKeyedService<T>(key)`, which replaces fragile last-wins patterns when you need named providers such as multiple payment gateways or storage backends. Register with `builder.Services.AddKeyedScoped<IPaymentProcessor, StripeProcessor>("stripe")` and `AddKeyedScoped<IPaymentProcessor, PayPalProcessor>("paypal")`. Inject in Minimal APIs or controllers with `([FromKeyedServices("stripe")] IPaymentProcessor processor)`. Keys are resolved from the active scope's provider, so keyed scoped services follow the same request scope rules as ordinary scoped services, which means keyed services reduce the need for separate marker interfaces or factory classes when the only difference is which implementation to select at runtime.

---

## Q11. What is `IServiceScopeFactory`, and when do you need it?

**Concepts**
- `IServiceScopeFactory` for out-of-scope resolution in singletons
- `CreateScope()` and `CreateAsyncScope()` per unit of work
- `BackgroundService` pattern — one scope per message or tick
- Disposing the scope to clean up scoped `IDisposable`s

**Answer**

`IServiceScopeFactory` creates new DI scopes on demand from code that runs outside an existing request scope, such as singleton hosted services, queue workers, or manual background tasks. It is registered as a singleton and is the supported way to resolve scoped services from long-lived components. The typical pattern in `BackgroundService` is `using var scope = _scopeFactory.CreateScope()`, then resolve scoped services from `scope.ServiceProvider`, so each queued message or timer tick gets a fresh `DbContext`. Without a scope factory, injecting scoped services into singletons creates captive dependencies or startup validation failures. Always dispose the scope with a `using` block so scoped disposables are cleaned up promptly after the work unit finishes.

---

## Q12. What is `IDbContextFactory<TContext>`, and when is it preferred over injecting `DbContext` directly?

**Concepts**
- Explicit `DbContext` ownership with `CreateDbContextAsync()`
- Parallel operations requiring multiple independent contexts
- Blazor Server and background worker use cases
- Direct scoped injection remaining correct for typical controller actions

**Answer**

`IDbContextFactory<TContext>` creates short-lived `DbContext` instances explicitly, which is ideal when work is not tied to a single HTTP request scope — for example, parallel operations in a singleton service, Blazor Server circuits, or background workers. Register with `AddDbContextFactory<AppDbContext>(options => ...)` alongside or instead of scoped `AddDbContext` depending on consumption patterns. Each `CreateDbContext()` or `await CreateDbContextAsync()` yields a context you own and must dispose, usually wrapped in `using`, since a single scoped `DbContext` is not thread-safe for parallel queries. For ordinary controller actions, scoped `DbContext` injection is simpler and aligns with one context per request, so the factory is needed only when the standard scoped pattern does not fit the lifetime or concurrency requirements.

---

## Q13. What do `ValidateOnBuild` and `ValidateScopes` do?

**Concepts**
- `ValidateOnBuild` — fail-fast on missing registrations at startup
- `ValidateScopes` — detect captive dependency at root resolution
- Small startup cost justified by preventing production DI bugs
- `UseDefaultServiceProvider` configuration in ASP.NET Core 8

**Answer**

These are optional checks on the default service provider that fail fast at startup instead of at runtime under load. `ValidateOnBuild` attempts to resolve the entire service graph when the provider is built, and `ValidateScopes` throws when a singleton or root scope tries to capture a scoped service directly. Enable them with:

```csharp
builder.Host.UseDefaultServiceProvider((context, options) => {
    options.ValidateOnBuild = true;
    options.ValidateScopes = true;
});
```

`ValidateOnBuild` catches missing registrations such as an unregistered `IOptions<SmtpSettings>` before traffic arrives, while `ValidateScopes` exposes captive dependencies like scoped `DbContext` injected into a singleton `CacheService` at root resolution. They add a small startup cost but are valuable in CI, staging, and local development to prevent DI lifetime mistakes from reaching production.

---

## Q14. How do you register an interface with its implementation?

**Concepts**
- `AddScoped<TInterface, TImpl>()` as the primary registration pattern
- Explicit lifetime selection for each service
- Factory delegate for complex construction logic
- Open generic registration for repository patterns

**Answer**

Use extension methods on `IServiceCollection` that pair the abstraction with the concrete type and lifetime: `builder.Services.AddScoped<IOrderRepository, SqlOrderRepository>()`. The container resolves `IOrderRepository` to `SqlOrderRepository` wherever that interface is requested, so choose the lifetime explicitly with `AddSingleton`, `AddScoped`, or `AddTransient` as appropriate for the implementation's state and dependencies. You can also register the concrete type alone (`AddScoped<OrderService>()`) when no interface is needed, though interface-based registration improves testability. Factory registration `AddScoped<IRepo>(sp => new SqlRepo(sp.GetRequiredService<...>()))` is used when construction logic is non-trivial, and open generic registration (`AddScoped(typeof(IRepository<>), typeof(EfRepository<>))`) supports generic repository patterns without per-entity boilerplate.

---

## Q15. What is constructor injection?

**Concepts**
- Constructor parameters as the injection point
- Most-resolvable constructor selection by the container
- Required vs optional dependency patterns
- Activation failure on unsatisfiable constructors

**Answer**

Constructor injection supplies a class's dependencies through its public constructor parameters, and the DI container chooses a constructor and fills each parameter with registered services. It is the primary injection style in ASP.NET Core because dependencies are required, visible, and immutable after construction. ASP.NET Core activates controllers, Minimal API handlers, middleware, and services by reflecting on the constructor with the most resolvable parameters. Multiple constructors are supported, but only one will be used — ambiguous or unsatisfiable constructors cause activation failures at runtime or at build with `ValidateOnBuild`. Optional dependencies can use default parameter values or `IOptions<T>` with defaults, but required services should not be resolved manually inside the constructor body since that pattern bypasses DI and hides dependencies.

---

## Q16. Can you inject a Scoped service into a Singleton? What happens?

**Concepts**
- Singleton outliving request scope causing captive dependency
- `ValidateScopes` throwing at startup when enabled
- `ObjectDisposedException` and cross-request state leakage
- `IServiceScopeFactory` and `IDbContextFactory<T>` as correct alternatives
- `IOptionsSnapshot<T>` as a scoped type subject to this rule

**Answer**

You should not inject a scoped service directly into a singleton constructor because the singleton outlives any request scope. With `ValidateScopes` enabled, the application throws at startup; without validation, the scoped instance may be created from the root provider, leading to `ObjectDisposedException`, stale state, or cross-request data leakage. The correct patterns are to make the consumer scoped, inject `IServiceScopeFactory` and create a scope per operation, or use `IDbContextFactory<T>` for database access from singleton workers. Middleware is scoped per request and can safely consume scoped services; singleton hosted services cannot without explicit scoping. This rule applies equally to your own services and to framework abstractions such as `IOptionsSnapshot<T>` that are registered scoped.

---

## Q17. What is the difference between `AddSingleton`, `AddScoped`, and `AddTransient`?

**Concepts**
- Lifetime as the only behavioral difference among the three methods
- Thread-safety requirement for `AddSingleton` with mutable state
- Request alignment for `AddScoped`
- `AddTransient` creating fresh instances on every resolution

**Answer**

These three methods register the same abstraction-to-implementation mapping but assign different lifetimes to the created instance — the method name states how long the container caches and reuses the instance. `AddSingleton<TService, TImplementation>()` creates one shared instance for the application process, so it is suitable for thread-safe stateless services only unless mutable state is explicitly synchronized. `AddScoped<TService, TImplementation>()` creates one instance per DI scope, which in web apps aligns with one HTTP request. `AddTransient<TService, TImplementation>()` creates a fresh instance on every resolution, even multiple times within the same request. All three support overloads for instances, factories, and open generics; lifetime is the only behavioral difference among them.

---

## Q18. How does DI work in Minimal API route handlers?

**Concepts**
- Handler parameter injection from the request scope
- `[FromKeyedServices("key")]` in handler parameters
- Scoped services resolving correctly per request
- Endpoint filters receiving injected services via constructor injection

**Answer**

Minimal API route handlers participate in the same DI container as controllers: parameters whose types are registered services are resolved automatically from the request scope when the endpoint executes, so you declare dependencies as handler parameters alongside route, query, and body bindings. For example, `app.MapGet("/orders/{id}", async (int id, IOrderService orders) => ...)` injects `IOrderService` from `HttpContext.RequestServices`. Use `[FromKeyedServices("key")]` for keyed services and `[FromServices]` when disambiguation is needed with other binding sources. Scoped services resolve correctly because each request creates a scope before the handler runs, and `TypedResults`, validation filters, and endpoint filters can also receive injected services through constructor injection on filter types registered in DI.

---

## Gotchas — ASP.NET Core (Interview Traps)

---

#### Gotcha 1. Middleware order — routing before auth

**Concepts**
- `UseRouting` before `UseAuthentication` and `UseAuthorization`
- Endpoint metadata availability for auth middleware
- Correct pipeline order in `Program.cs`

**Answer**

In ASP.NET Core 8 endpoint routing, `UseRouting` must run before `UseAuthentication` and `UseAuthorization` so the auth middleware can inspect endpoint metadata — registering auth before routing means the endpoint has not been selected yet, which breaks endpoint-aware authorization and policy resolution. The recommended order is exception handling → forwarded headers → routing → authentication → authorization → endpoints (`MapControllers` / `MapGet`). Symptoms of wrong order include anonymous access to protected endpoints or 401 responses without proper challenge behavior, so always verify middleware order in `Program.cs` during code review for new services.

---

#### Gotcha 2. Scoped service in a Singleton

**Concepts**
- Captive `DbContext` living past its scope
- Stale EF change tracker accumulating unrelated entities
- `ValidateScopes` as the detection mechanism

**Answer**

Registering a scoped service such as `DbContext` into a singleton creates a captive dependency that lives for the application lifetime while the scoped instance is disposed after its first scope ends, causing stale data, thread-safety bugs, or `ObjectDisposedException`. The singleton holds one scoped instance forever instead of one per request, so EF change trackers accumulate unrelated entities across requests. Enable `ValidateScopes` in Development/staging to catch illegal scope combinations at startup, and fix by injecting `IServiceScopeFactory` or `IDbContextFactory<T>` and creating a scope per operation. This applies equally to singleton services, hosted services, and cached delegates in Minimal APIs.

---

#### Gotcha 3. `new HttpClient()` in a singleton

**Concepts**
- Socket exhaustion from per-use `HttpClient` instantiation
- `HttpMessageHandler` lifecycle managed by `IHttpClientFactory`
- Named and typed client registration pattern

**Answer**

Instantiating `HttpClient` with `new` inside a long-lived singleton prevents socket reuse and causes socket exhaustion under load because each instance holds its own connection pool until garbage-collected. `HttpClient` is disposable but not meant for per-use disposal — `using var client = new HttpClient()` in a singleton is an anti-pattern because the OS connection handle is held by the handler, not the client object itself. `IHttpClientFactory` manages `HttpMessageHandler` lifetimes and recycles connections correctly; register named or typed clients with `builder.Services.AddHttpClient<IExternalApi, ExternalApiClient>()`. Symptoms include `SocketException` and timeout errors only under production traffic, not in local testing.

---

#### Gotcha 4. `IOptions<T>` vs reload

**Concepts**
- `IOptions<T>` — fixed snapshot at first resolution
- `IOptionsSnapshot<T>` — per-request recalculation, scoped
- `IOptionsMonitor<T>` — singleton-safe with change notifications
- Stale configuration when `.Value` is cached in a constructor field

**Answer**

`IOptions<T>` captures a configuration snapshot at first resolution — reading `.Value` once in a singleton constructor freezes settings even when `appsettings.json` reloads with `ReloadOnChange` enabled, because the wrapper holds the computed value without subscribing to change tokens. `IOptionsSnapshot<T>` recalculates per request scope so a singleton cannot inject it without creating a captive dependency. `IOptionsMonitor<T>` is the singleton-safe wrapper that supports change notifications via `OnChange` and exposes `CurrentValue` for the latest merged configuration. Misconfiguration persists silently until process restart when `.Value` was cached at construction, so singleton services that need live updates must use `IOptionsMonitor<T>`.

---

#### Gotcha 5. GET with `[FromBody]`

**Concepts**
- HTTP GET body stripped by clients, proxies, and CDNs
- `[FromQuery]` with `[AsParameters]` for complex GET filters
- Silent binding failure rather than explicit error

**Answer**

Using `[FromBody]` on GET action parameters or minimal API handlers is an anti-pattern because HTTP GET semantics discourage bodies, and many clients, proxies, and caches strip or ignore GET request bodies, so binding fails silently in production. Query strings and route values are the correct binding sources for GET requests, and complex filters should use `[FromQuery]` with `[AsParameters]` or flattened query keys. Failures often appear only in specific browsers or CDN layers, not in Swagger "Try it out" during development, since Swagger sends directly to local Kestrel without intermediate proxies. REST conventions expect GET to be safe and idempotent with parameters in the URL.

---

#### Gotcha 6. PascalCase JSON keys with default camelCase policy

**Concepts**
- Default `JsonNamingPolicy.CamelCase` in ASP.NET Core 8
- Silent binding to default values on case mismatch
- `PropertyNameCaseInsensitive` as a compatibility bridge

**Answer**

ASP.NET Core 8 Web API serializes JSON with camelCase property names by default via `JsonNamingPolicy.CamelCase`, so incoming JSON with PascalCase keys (for example `"CustomerName"`) may not bind to `CustomerName` unless case-insensitive matching is enabled. Mobile or legacy clients sending PascalCase appear to succeed but properties remain default values (empty string, zero) since System.Text.Json's default matching is exact-case on deserialization. Prefer standardizing clients on camelCase and documenting the contract in OpenAPI, and add validation attributes so silent binding failures become 400 responses instead of corrupt data.

---

#### Gotcha 7. `throw ex` vs `throw`

**Concepts**
- `throw ex` resetting the stack trace to the catch block
- `throw;` preserving the original stack trace
- `InnerException` preservation when wrapping in a new exception

**Answer**

Rethrowing with `throw ex` resets the stack trace to the catch block line, hiding the original failure location in logs and diagnostics, while bare `throw` preserves the full stack trace from where the exception was first thrown. Exception filters, middleware, and Application Insights rely on accurate stack traces for root-cause analysis, so always use `throw;` when rethrowing after logging or cleanup in a catch block. Wrap in a new exception only when adding context — `throw new OrderProcessingException("...", ex)` — to preserve `InnerException`. This trap appears in both application code and background worker error handlers.

---

#### Gotcha 8. Kestrel as the only production layer

**Concepts**
- Kestrel as application server vs full edge gateway
- TLS termination, WAF, and rate limiting at the reverse proxy
- `UseForwardedHeaders` required for accurate client IP and scheme

**Answer**

Running Kestrel exposed directly to the internet without a reverse proxy skips TLS termination at the edge, centralized rate limiting, WAF protection, and efficient static-file caching that production deployments typically require. Kestrel is production-grade as an application server but is not a full edge gateway — nginx, IIS, Azure Front Door, or AWS ALB commonly sit in front since TLS certificates are easier to manage at the proxy layer with automatic renewal. Direct exposure also complicates client IP logging unless `UseForwardedHeaders` is configured with a trusted proxy, and containers typically bind Kestrel to port 8080 internally while the ingress controller handles HTTPS externally.

---

#### Gotcha 9. `launchSettings.json` in production

**Concepts**
- `launchSettings.json` as development-only launch configuration
- Production host using environment variables, not launch profiles
- `ASPNETCORE_ENVIRONMENT` and `ASPNETCORE_URLS` as runtime configuration

**Answer**

Settings in `Properties/launchSettings.json` — including `applicationUrl`, environment variables, and launch profiles — apply only when starting from Visual Studio, VS Code, or `dotnet run` with a profile; they are not deployed to production hosts. Production URLs and environment come from environment variables (`ASPNETCORE_URLS`, `ASPNETCORE_ENVIRONMENT`), container configuration, or IIS/nginx site settings. Assuming `launchSettings.json` sets Production behavior leads to wrong environment or binding in deployed environments since the published application does not include or read the file. Use `appsettings.Production.json` and host-level env vars for production values.

---

#### Gotcha 10. Non-nullable `bool` for PATCH semantics

**Concepts**
- Non-nullable `bool` defaulting to `false` on JSON omission
- Three-state intent: unspecified, opt-in, opt-out
- `bool?` or enum tri-state for partial-update DTOs

**Answer**

A non-nullable `bool` property cannot distinguish "field omitted from JSON" from "explicitly set to false" because System.Text.Json deserializes missing properties to `default(false)`, corrupting partial-update semantics. PATCH endpoints need `bool?`, separate update DTOs, or enums such as `Unspecified | OptIn | OptOut` for tri-state intent, since a user omitting `sendNewsletter` should mean "leave as is" rather than "opt out". Marketing consent and feature flags are common domains where this bug causes compliance or logic errors, and nullable fields should be documented in OpenAPI so generated clients represent optional updates correctly.

---

#### Gotcha 11. Forgetting `UseForwardedHeaders` behind a proxy

**Concepts**
- `X-Forwarded-Proto` and `X-Forwarded-For` headers
- Wrong scheme causing broken HTTPS redirects and cookie secure flags
- `KnownProxies` configuration to prevent header spoofing

**Answer**

Without forwarded headers middleware configured with known proxy IPs, `HttpContext.Request.Scheme` remains `http`, `Request.Host` reflects the internal address, and client IP is the proxy — breaking HTTPS redirects, cookie secure flags, and audit logs. Call `UseForwardedHeaders()` early, before middleware that reads scheme or host such as HTTPS redirection, link generation, or rate limiting by IP. Configure `ForwardedHeadersOptions` to trust only your reverse proxy network since trusting all proxies enables header spoofing where a client can inject arbitrary `X-Forwarded-For` values. Local development without a proxy does not need this; production behind nginx/IIS/ALB does.

---

#### Gotcha 12. Static files in `wwwroot` are public

**Concepts**
- `UseStaticFiles()` serving all `wwwroot` contents unauthenticated
- Secrets and config files must stay outside the web root
- `appsettings.Production.json` in `wwwroot` as a critical security incident

**Answer**

Any file under `wwwroot` is served by `UseStaticFiles()` to unauthenticated clients by default, so placing secrets, `.env`, backup configs, or private keys there exposes them over HTTP to anyone who can guess the filename. Only public assets (CSS, JS, images, public PDFs) belong in `wwwroot`, while sensitive configuration stays outside the web root and is loaded through `IConfiguration`, environment variables, or secret managers. An accidental copy of `appsettings.Production.json` into `wwwroot` is a critical security incident since the file is served as a plain-text download. Use build pipelines to verify web root contents before deploy.

---

#### Gotcha 13. `MapFallbackToFile` intercepting API routes

**Concepts**
- SPA fallback returning `index.html` for unmatched routes including `/api/*`
- API endpoint registration ordering before fallback
- CORS and Swagger failures masked by HTML responses

**Answer**

SPA fallback middleware registered before API endpoint mapping returns `index.html` for `/api/*` 404 responses, making API failures look like successful HTML responses to clients and breaking JSON parsers. Map API routes (`MapControllers`, minimal API groups) before `MapFallbackToFile("index.html")`, and scope fallback to non-API paths or use conditional fallback that excludes `/api` prefixes. Symptoms include CORS errors masked as HTML responses and Swagger fetch failures in production SPA hosting, so the correct order in `Program.cs` is: API endpoints first, static files, fallback last.

---

#### Gotcha 14. Background service without scope factory

**Concepts**
- Singleton `BackgroundService` incompatible with constructor-injected scoped services
- `CreateAsyncScope()` per job to create a fresh scope
- `ValidateScopes` catching this defect at startup

**Answer**

A singleton `BackgroundService` that injects scoped services (`DbContext`, repositories) directly into its constructor fails at startup with scope validation errors or uses disposed instances after the first background iteration, because hosted services live for the application lifetime and scoped dependencies must not be constructor-injected. Inject `IServiceScopeFactory`, create `await using var scope = factory.CreateAsyncScope()` per job, resolve scoped services inside the scope, and dispose when the job completes. The same rule applies to timers and `Task.Run` loops started from singletons, and enabling `ValidateScopes` catches this defect before production deployment.

---

#### Gotcha 15. SignalR without a backplane on multiple instances

**Concepts**
- SignalR hub broadcasting to connected clients on the same instance only
- Redis or Azure Service Bus backplane for multi-node event routing
- Sticky sessions insufficient without a backplane

**Answer**

SignalR broadcasts from one server instance reach only clients connected to that instance — without a Redis or Azure Service Bus backplane (or Azure SignalR Service), users on different nodes never receive each other's real-time events. Sticky sessions keep one client on one node but do not route events raised on other nodes to that client, so adding a second instance without a backplane means events silently disappear for users on the wrong node. Register `AddSignalR().AddStackExchangeRedis(...)` with a consistent channel prefix per application, and test scale-out with at least two instances before launch rather than single-node staging alone.

---

## Scenario-Based Questions (Karat Format)

---

#### Q1. (M) You register two payment gateways: `AddSingleton<IPaymentProcessor, StripeProcessor>()` and `AddSingleton<IPaymentProcessor, PayPalProcessor>()`. Injecting `IPaymentProcessor` resolves one implementation; injecting `IEnumerable<IPaymentProcessor>` resolves both. How does the container behave, and when does single-interface injection silently surprise you?

**Concepts**
- Last-registered wins for single `IPaymentProcessor` resolution
- `IEnumerable<T>` returning all registrations in order
- Keyed services as the named-implementation solution in .NET 8
- Composite pattern requiring factory or explicit `IEnumerable<T>` injection

**Answer**

The built-in container stores multiple descriptors for the same service type; resolving `IPaymentProcessor` returns the last registration (PayPal in this example), while `IEnumerable<IPaymentProcessor>` injects a composite enumerating all registrations in registration order. Duplicate `AddSingleton<IPaymentProcessor, T>()` calls append descriptors rather than overwrite, which means single-interface injection is last-wins and surprises developers who expect either "only one" or "both". Use `IEnumerable<IPaymentProcessor>` when you need all implementations for a fan-out or strategy chain. When you need named implementations, keyed services (`AddKeyedSingleton`) in .NET 8 avoid ambiguity, since registering a decorator as the single `TInterface` while injecting the inner as concrete or keyed avoids the last-wins problem entirely. Alternatives before .NET 8 were separate interfaces or explicit factory delegates.

---

#### Q2. (M) How do `ValidateScopes` and `ValidateOnBuild` on `UseDefaultServiceProvider()` catch captive dependencies and missing registrations at startup rather than under load?

**Concepts**
- `ValidateOnBuild` resolving the entire service graph at host build
- `ValidateScopes` throwing on singleton capturing scoped service
- Startup cost vs production safety trade-off

**Answer**

`ValidateOnBuild` resolves the entire service graph at startup to surface missing registrations — for example `Unable to resolve service for type 'IOptions<SmtpOptions>'` — before traffic arrives. `ValidateScopes` throws when a singleton constructor injects a scoped service directly, exposing captive dependencies like scoped `AppDbContext` in a singleton hosted service. Configure both on the host builder:

```csharp
builder.Host.UseDefaultServiceProvider((context, options) =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});
```

These checks add a small startup cost but are valuable in CI and staging because apps that start fine locally but fail under load often skipped this validation. They do not replace fixing the architecture — `IServiceScopeFactory` per operation in background workers is still required once the defect is found.

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

**Concepts**
- Singleton with mutable instance field causing cross-user data leakage
- `List<T>` race condition under concurrent writes
- In-memory state inconsistent across scale-out pods
- Missing per-user identity key on cart data

**Answer**

A singleton `CartService` with an instance `_items` list shares one cart across every user and request. Under concurrent shoppers, `List<T>.Add` is not thread-safe, which means items are lost or the collection corrupts under parallel writes. More critically, every user sees every other user's cart items since the list is shared process-wide — a privacy and billing incident. When scaled to multiple instances, each pod has its own singleton so cart state differs per pod and sticky sessions alone are not enough to prevent the problem from surfacing. The fix requires registering `ICartService` as scoped with a per-user or per-session key, and persisting cart lines in Redis or SQL so all instances share the same state.

```csharp
builder.Services.AddScoped<ICartService, CartService>(); // + user id in service API
// or distributed: ICartStore backed by Redis, singleton orchestrator only
```

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

**Concepts**
- Captive `DbContext` resolved at root scope and disposed after first scope ends
- `ObjectDisposedException` on deferred saves after scope ends
- `IServiceScopeFactory` creating one scope per message as the fix
- Stale change tracker accumulating orphaned entity state

**Answer**

The singleton `BackgroundService` outlives any request scope; when the DI container resolves scoped `AppDbContext` at root, the context is created once and its owning scope ends almost immediately, leaving the worker holding a disposed context. The `ObjectDisposedException` appears under load because the timing is non-deterministic — the context may still work for early iterations before the scope is collected. Additionally, sharing one context across unrelated orders means the change tracker accumulates orphaned entity state. The fix is to inject `IServiceScopeFactory` instead of `DbContext`, create a new scope per message, and dispose it when the message is processed:

```csharp
await using var scope = _scopeFactory.CreateAsyncScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
db.Orders.Add(order);
await db.SaveChangesAsync(stoppingToken);
```

Enable `ValidateScopes` and `ValidateOnBuild` in staging to catch this class of defect before it reaches production.

---

#### Q5. (D) A team caches per-user preferences in `static Dictionary<string, UserPrefs>` inside a singleton service. What breaks under concurrent load, parallel tests, and horizontal scale-out?

**Concepts**
- `Dictionary<,>` not thread-safe under concurrent writes
- Static state shared across test classes in parallel test runs
- Per-process cache invisible to other scale-out pods
- Unbounded growth with no eviction policy

**Answer**

Process-wide static mutable state breaks in three distinct ways. Under concurrent load, `Dictionary<,>` is not thread-safe — parallel writes throw or silently corrupt the collection, requiring `ConcurrentDictionary` at minimum, though that still does not fix the scale-out problem. In parallel xUnit test runs, test classes share the static dictionary in the same process, causing flaky failures that disappear in serial runs. Across scale-out, each replica has its own static cache so an update on node A is invisible on node B. Beyond these bugs, there is no eviction tied to session lifetime, which means unbounded memory growth and potential cross-user leakage if a tenant prefix is wrong or missing. The correct approach is `IMemoryCache` with tenant-scoped keys and TTL, or Redis per user, resolved via a scoped service per request.

---

#### Q6. (P) .NET 8 keyed services: you need `"primary"` and `"fallback"` implementations of `INotificationSender` in the same process. How do you register and resolve them without ambiguous `INotificationSender` injection?

**Concepts**
- `AddKeyedSingleton<T, TImpl>("key")` registration
- `[FromKeyedServices("key")]` constructor parameter attribute
- Keyed services supporting all three lifetimes per key
- `IEnumerable<T>` as the alternative for fan-out rather than named selection

**Answer**

Use keyed DI — register with `AddKeyedSingleton<INotificationSender, EmailSender>("primary")` and resolve via `[FromKeyedServices("primary")]`, `GetRequiredKeyedService`, or a keyed factory delegate, which avoids the last-wins ambiguity of multiple unkeyed registrations.

```csharp
builder.Services.AddKeyedSingleton<INotificationSender, EmailSender>("primary");
builder.Services.AddKeyedSingleton<INotificationSender, SmsSender>("fallback");

public class AlertService(
    [FromKeyedServices("primary")] INotificationSender primary,
    [FromKeyedServices("fallback")] INotificationSender fallback) { }
```

Unkeyed `INotificationSender` injection remains ambiguous if multiple unkeyed registrations exist, so prefer keyed-only or a single default plus keyed alternates. Keyed services work with singleton, scoped, and transient lifetimes per key. Before .NET 8, alternatives were separate interfaces, a factory pattern, or `IServiceProvider` locator (which is discouraged because it hides dependencies).

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

**Concepts**
- `new HttpClient()` per call exhausting ephemeral ports and socket handles
- `HttpMessageHandler` pool not reused across disposals
- Typed client via `AddHttpClient<T>()` as the correct pattern
- TLS handshake overhead on every new client instance

**Answer**

Instantiating `HttpClient` per call exhausts ephemeral ports and socket handles under load because `using` disposes the client object but the underlying `HttpMessageHandler` (and its connection pool) is held by the OS until GC finalization, which means port exhaustion appears before memory is reclaimed. Since a new client is created on every call, DNS changes to `pricing.internal` never propagate because each connection is freshly established without the handler rotation that `IHttpClientFactory` provides. The fix is a typed client:

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

Register with `builder.Services.AddHttpClient<IPricingService, PricingService>(c => c.BaseAddress = new Uri("..."))`, inject `HttpClient` via constructor, and do not dispose the injected client — the factory owns the handler lifecycle.

---

#### Q8. (P) When should you inject `IServiceScopeFactory` vs `IDbContextFactory<AppDbContext>` vs a custom `Func<IServiceProvider, T>` factory? Compare a nightly batch job vs per-message queue worker vs on-demand controller action.

**Concepts**
- `IServiceScopeFactory` resolving the full scoped service graph per unit of work
- `IDbContextFactory` for lightweight context creation without full scope overhead
- Controller actions using the natural request scope
- Scope boundary granularity as the deciding factor

**Answer**

Controllers use the natural request scope for scoped services so no factory is needed — inject `AppDbContext` directly. For a per-message queue worker inside a singleton `BackgroundService`, use `IServiceScopeFactory.CreateAsyncScope()` per message because you need the full scoped service graph (repository, unit of work, context) together in one scope, and the scope's disposal triggers cleanup for all of them. For a nightly batch that touches many aggregates, use `IServiceScopeFactory` per batch chunk so the change tracker is reset between chunks. When only EF context is needed and you want lightweight creation without full scope overhead — high-frequency short DB reads or Blazor Server — use `IDbContextFactory<AppDbContext>` with `await using var ctx = await factory.CreateDbContextAsync()`. Avoid custom `Func<IServiceProvider, T>` factory delegates unless encapsulating multi-implementation selection or simplifying tests, since they hide dependencies from DI graph validation.

| Scenario | Tool | Why |
|---|---|---|
| Controller action | Inject scoped `AppDbContext` directly | Request scope matches HTTP lifetime |
| Per-message queue worker | `IServiceScopeFactory.CreateAsyncScope()` | Full scoped graph per message |
| Nightly batch per chunk | `IServiceScopeFactory` | Dispose contexts between chunks |
| High-frequency short DB reads | `IDbContextFactory<AppDbContext>` | Pool-friendly context creation |

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

**Concepts**
- Last-registered wins for single-interface resolution
- `FileAuditLogger` orphaned and never resolved
- Composite pattern for multi-sink audit logging
- `IEnumerable<IAuditLogger>` for fan-out intent

**Answer**

Two `AddSingleton<IAuditLogger, ...>()` registrations make single `IAuditLogger` injection last-wins — only `ConsoleAuditLogger` resolves and `FileAuditLogger` is orphaned. The container has no concept of a "chain" from multiple registrations of the same interface; it just returns the last one. The fix depends on intent: if you want both to run, register a `CompositeAuditLogger` that wraps both, or inject `IEnumerable<IAuditLogger>` in `OrderService`. If you want a decorator chain, use Scrutor's `Decorate<IAuditLogger, ConsoleAuditLogger>()` or a manual factory.

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

The compliance gap from missing file audit is silent in production — no error, no warning, just missing audit records.

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

*(`ProductRepository` holds scoped `DbContext`; `CatalogService` is singleton.)*

**Concepts**
- Singleton `CatalogService` capturing scoped `IProductRepository` — captive dependency
- Unbounded `Dictionary` cache with no eviction
- Unsynchronized dictionary writes under parallel requests
- Per-instance cache inconsistent across scale-out pods

**Answer**

`CatalogService` is singleton but depends on scoped `IProductRepository`, which means the scoped `DbContext` inside the repository is captured at root scope — a captive dependency causing stale change tracker state and eventual `ObjectDisposedException`. The `CatalogCache` never evicts entries, so product updates after deploy are invisible to anyone who read the old price before cache population. The unsynchronized `Dictionary` write in `Store` also corrupts under parallel catalog page requests. The fix is to make `ICatalogService` scoped (or use `IDbContextFactory` with a singleton service), replace the static dictionary with `IMemoryCache` with TTL for proper eviction, and enable `ValidateScopes` to catch the lifetime mismatch at startup.

```csharp
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
```
