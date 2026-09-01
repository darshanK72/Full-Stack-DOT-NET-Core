/*
 * TOPIC: Dependency Injection & Service Lifetimes
 *
 * WHY IT MATTERS:
 *   Inversion of Control (IoC) is the design principle where objects declare what
 *   they need rather than creating their own dependencies.  Dependency Injection (DI)
 *   is its most common implementation: an external container creates and wires
 *   objects together, then "injects" them into consumers.
 *
 *   ASP.NET Core's built-in DI container is the plumbing behind every controller,
 *   middleware, service, and background task.  Getting lifetimes wrong is one of
 *   the most common bugs in ASP.NET Core: a captive dependency can cause memory
 *   leaks, stale data, or thread-safety violations that only surface under load.
 *
 * WHAT YOU WILL LEARN:
 *   1.  IoC & DI core concepts — why and how
 *   2.  IMessageService interface contract → Interfaces/IMessageService.cs
 *   3.  ICounterService interface contract → Interfaces/ICounterService.cs
 *   4.  Singleton lifetime implementation  → Services/SingletonCounterService.cs
 *   5.  Scoped lifetime implementation     → Services/ScopedMessageService.cs
 *   6.  Transient lifetime implementation  → Services/TransientTimestampService.cs
 *   7.  Constructor injection in a controller → Controllers/DemoController.cs
 *   8.  [FromServices] method injection    → Controllers/DemoController.cs
 *   9.  Manual resolution (service locator pattern) → Controllers/DemoController.cs
 *   10. IServiceCollection — the service registry
 *   11. AddSingleton / AddScoped / AddTransient — lifetime registrations
 *   12. TryAdd variants — conditional registration
 *   13. ServiceDescriptor — low-level service description
 *   14. Keyed services (.NET 8) — multiple implementations per interface
 *   15. Built-in services — ILogger, IConfiguration, IHttpClientFactory
 *   16. Captive dependency pitfall + IServiceScopeFactory
 *
 * CHAPTER MAP:
 *   SECTION  2 — IMessageService interface    → Interfaces/IMessageService.cs
 *   SECTION  3 — ICounterService interface    → Interfaces/ICounterService.cs
 *   SECTION  4 — Singleton implementation     → Services/SingletonCounterService.cs
 *   SECTION  5 — Scoped implementation        → Services/ScopedMessageService.cs
 *   SECTION  6 — Transient implementation     → Services/TransientTimestampService.cs
 *   SECTION  7 — Constructor injection        → Controllers/DemoController.cs
 *   SECTION  8 — [FromServices] injection     → Controllers/DemoController.cs
 *   SECTION  9 — Manual resolution            → Controllers/DemoController.cs
 *   SECTIONS 1, 10–16 — DI registration       → this file (Program.cs)
 */

using DependencyInjection.Interfaces;
using DependencyInjection.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions; // TryAddSingleton / TryAddScoped / TryAddTransient

/*
 * SECTION 1: IOC & DEPENDENCY INJECTION — CORE CONCEPTS
 *
 * INVERSION OF CONTROL (IoC):
 *   Traditional code: ClassA calls  new ClassB()  — ClassA controls ClassB's creation.
 *   IoC: ClassA declares "I need an IB" — an external container decides which IB to give.
 *   The control of dependency creation is INVERTED from the consumer to the container.
 *
 * DEPENDENCY INJECTION (DI):
 *   DI is the most common IoC pattern.  The container creates and "injects" dependencies
 *   into consumers, typically via the constructor, so consumers never call `new`.
 *
 * THREE INJECTION MODES:
 * ┌─────────────────┬────────────────────────────────────────────────────────────────┐
 * │ Constructor     │ Dependencies in the constructor — preferred; explicit; testable │
 * │ Method          │ [FromServices] on action parameters — for action-specific deps  │
 * │ Property        │ Not supported natively; hidden deps; avoid — see SECTION 7      │
 * └─────────────────┴────────────────────────────────────────────────────────────────┘
 *
 * CONTAINER WORKFLOW:
 *   1. Register services in IServiceCollection (builder.Services)
 *   2. Call builder.Build() — container compiles its service graph; validates in Development
 *   3. At runtime, the container resolves services on demand (lazy by default)
 *   4. At shutdown, the container disposes IDisposable services in reverse creation order
 */
var builder = WebApplication.CreateBuilder(args); // host + IServiceCollection (builder.Services)

/*
 * SECTION 10: IServiceCollection — THE SERVICE REGISTRY
 *
 * IServiceCollection is a list of ServiceDescriptor records.  Each descriptor stores:
 *   • ServiceType          — the type callers will request (usually an interface)
 *   • ImplementationType   — the concrete class the container will instantiate
 *   • Lifetime             — Singleton / Scoped / Transient
 *
 * builder.Services is the IServiceCollection for this application.
 * After builder.Build() is called, the collection is sealed — no more registrations.
 *
 * COMMON REGISTRATION SIGNATURES (AddSingleton shown; same shape for all lifetimes):
 *   services.AddSingleton<IFoo, FooImpl>()           interface → implementation
 *   services.AddSingleton<FooImpl>()                 concrete only (no interface abstraction)
 *   services.AddSingleton<IFoo>(new FooImpl())       pre-built instance (avoid for testability)
 *   services.AddSingleton<IFoo>(sp =>                factory lambda (for complex constructors)
 *       new FooImpl(sp.GetRequiredService<IBar>()))
 */

/*
 * SECTION 11: LIFETIME REGISTRATIONS — ADDSINGLETON / ADDSCOPED / ADDTRANSIENT
 *
 * RULE OF THUMB:
 *   Default to Transient (safest — no shared state).
 *   Use Scoped when the service needs request-consistent state (e.g. DbContext).
 *   Use Singleton only when the service holds app-wide state and is thread-safe.
 *
 * LIFETIME SUMMARY:
 * ┌─────────────────┬──────────────────────────────┬──────────────────────────────────────┐
 * │ Method          │ Instances created            │ Best for                             │
 * ├─────────────────┼──────────────────────────────┼──────────────────────────────────────┤
 * │ AddSingleton    │ One per application          │ Caches, counters, IHttpClientFactory │
 * │ AddScoped       │ One per request / scope      │ DbContext, unit-of-work, repositories│
 * │ AddTransient    │ One per injection point      │ Stateless helpers, validators        │
 * └─────────────────┴──────────────────────────────┴──────────────────────────────────────┘
 *
 * INJECTION SAFETY MATRIX (can Consumer inject Dependency?):
 * ┌────────────────────────┬────────────┬──────────────┬────────────┐
 * │ Consumer \ Dependency  │ Singleton  │   Scoped     │ Transient  │
 * ├────────────────────────┼────────────┼──────────────┼────────────┤
 * │ Singleton              │    ✓       │  ✗ CAPTIVE   │   ✓ *      │
 * │ Scoped                 │    ✓       │      ✓       │   ✓        │
 * │ Transient              │    ✓       │      ✓       │   ✓        │
 * └────────────────────────┴────────────┴──────────────┴────────────┘
 *   ✗ CAPTIVE: singleton captures scoped service — see SECTION 16.
 *   * Transient in singleton: allowed, but the transient lives as long as the singleton.
 */
builder.Services.AddSingleton<ICounterService, SingletonCounterService>(); // one instance, app-wide
builder.Services.AddScoped<IMessageService, ScopedMessageService>();       // one per HTTP request
builder.Services.AddTransient<TransientTimestampService>();                  // new on every injection

/*
 * SECTION 12: TRYADD VARIANTS — CONDITIONAL REGISTRATION
 *
 * TryAdd* registers a service ONLY if no registration for that ServiceType exists yet.
 * This lets library authors supply defaults that host code can override before startup.
 *
 * TryAddSingleton<T, TImpl>()    — singleton if not already registered
 * TryAddScoped<T, TImpl>()       — scoped if not already registered
 * TryAddTransient<T, TImpl>()    — transient if not already registered
 * TryAddEnumerable(descriptor)   — adds only if no descriptor with the same
 *                                    ServiceType + ImplementationType pair exists
 *
 * CONTRAST WITH Add* (without Try):
 *   services.AddSingleton<IFoo, FooV1>()
 *   services.AddSingleton<IFoo, FooV2>()   // BOTH registered; GetService<IFoo>() returns FooV2
 *                                           // but GetServices<IFoo>() returns both
 *
 * The TryAddSingleton below is a no-op because ICounterService was registered above.
 * This is intentional: it shows the conditional behavior without breaking the app.
 */
builder.Services.TryAddSingleton<ICounterService, SingletonCounterService>(); // no-op: already registered

/*
 * SECTION 13: SERVICEDESCRIPTOR — LOW-LEVEL SERVICE DESCRIPTION
 *
 * Every Add* call internally creates a ServiceDescriptor and adds it to the collection.
 * You can build descriptors manually for dynamic registration (plugins, reflection systems).
 *
 * DESCRIPTOR CONSTRUCTORS:
 *   ServiceDescriptor.Describe(typeof(IFoo), typeof(Foo), ServiceLifetime.Scoped)
 *   ServiceDescriptor.Singleton<IFoo, Foo>()      — convenience for Singleton lifetime
 *   ServiceDescriptor.Scoped<IFoo, Foo>()         — convenience for Scoped lifetime
 *   ServiceDescriptor.Transient<IFoo, Foo>()      — convenience for Transient lifetime
 *
 * DESCRIPTOR PROPERTIES:
 *   descriptor.ServiceType           → typeof(IMessageService)
 *   descriptor.ImplementationType   → typeof(ScopedMessageService)
 *   descriptor.Lifetime             → ServiceLifetime.Scoped
 *   descriptor.ImplementationFactory → null (or a Func<IServiceProvider, object>)
 *   descriptor.ImplementationInstance → null (or a pre-built singleton instance)
 *
 * SERVICES MANIPULATION:
 *   services.Add(descriptor)         — unconditional add (may create duplicate)
 *   services.Replace(descriptor)     — removes existing registration, adds new one
 *   services.RemoveAll<IFoo>()       — removes ALL registrations for IFoo
 *   services.Contains(descriptor)    — reference-equality check on the descriptor object
 *   services[index]                  — indexer access by position in the collection
 */

/*
 * SECTION 14: KEYED SERVICES — .NET 8+
 *
 * .NET 8 introduced keyed service registration: multiple implementations of the same
 * interface, each associated with a string (or object) key.
 *
 * REGISTRATION:
 *   services.AddKeyedSingleton<IFoo, FooA>("keyA")
 *   services.AddKeyedScoped<IFoo, FooB>("keyB")
 *   services.AddKeyedTransient<IFoo, FooC>("keyC")
 *
 * RESOLUTION:
 *   serviceProvider.GetRequiredKeyedService<IFoo>("keyA")
 *   serviceProvider.GetKeyedService<IFoo>("keyA")     ← returns null if key not found
 *
 * IN CONTROLLER / MINIMAL API HANDLERS:
 *   ([FromKeyedServices("keyA")] IFoo svc) => ...      ← attribute-based resolution
 *
 * USE CASES:
 *   • Strategy pattern: pick an algorithm by name at runtime
 *   • Named HTTP clients (IHttpClientFactory uses this internally)
 *   • Tenant-specific or environment-specific implementations
 *
 * NOTE: Keyed registrations are SEPARATE from non-keyed registrations.
 *   GetRequiredService<IFoo>() resolves the non-keyed registration (AddScoped above).
 *   GetRequiredKeyedService<IFoo>("primary") resolves the keyed one below.
 */
builder.Services.AddKeyedScoped<IMessageService, ScopedMessageService>("primary"); // keyed: "primary"
builder.Services.AddKeyedScoped<IMessageService, ScopedMessageService>("backup");  // keyed: "backup"

/*
 * SECTION 15: BUILT-IN SERVICES — ILogger, IConfiguration, IHttpClientFactory
 *
 * WebApplication.CreateBuilder registers a rich set of services automatically.
 * You consume them exactly like custom services: declare them in the constructor.
 *
 * ILogger<T>:
 *   Registered by AddLogging() (called by CreateBuilder internally).
 *   Inject ILogger<MyController> for structured, category-scoped logging.
 *   COVERED IN DEPTH → 06. Logging & Diagnostics
 *
 * IConfiguration:
 *   Registered by host bootstrapping from appsettings.json, environment variables,
 *   user secrets, and command-line arguments.
 *   Inject IConfiguration for raw key lookups, or use IOptions<T> for typed sections.
 *   COVERED IN DEPTH → 05. Configuration & Options Pattern
 *
 * IHttpClientFactory:
 *   Registered by AddHttpClient().  Manages HttpClient connection pools to prevent
 *   socket exhaustion — never use  new HttpClient()  per-request in production.
 *   Named clients configure base addresses and headers once at startup:
 *     services.AddHttpClient("payments", c => c.BaseAddress = new Uri("https://pay.example.com"));
 *     // consumer: _httpClientFactory.CreateClient("payments")
 *   COVERED IN DEPTH → 14. Background & Hosted Services
 *
 * IWebHostEnvironment:
 *   Provides Environment.EnvironmentName ("Development", "Production", etc.).
 *   Inject to make environment-specific decisions in services or middleware.
 *
 * IServiceScopeFactory:
 *   Registered automatically as a singleton.  Creates DI scopes from singleton or
 *   background services.  DETAILED BELOW in SECTION 16.
 */
builder.Services.AddHttpClient();     // registers IHttpClientFactory + named-client support
builder.Services.AddControllers();    // registers MVC controller infrastructure + routing

/*
 * SECTION 16: CAPTIVE DEPENDENCY PITFALL + IServiceScopeFactory
 *
 * CAPTIVE DEPENDENCY — THE MOST COMMON DI LIFETIME BUG:
 *   A singleton holds a reference to a scoped service injected in its constructor.
 *   Because the singleton lives forever, the scoped service is NEVER released —
 *   it behaves as a singleton, breaking the scoped contract (stale data, no disposal).
 *
 *   EXAMPLE — DO NOT DO THIS:
 *   ┌─────────────────────────────────────────────────────────────────────────────┐
 *   │ public class BadSingleton                                                   │
 *   │ {                                                                           │
 *   │     private readonly IMessageService _svc; // captures a scoped service!   │
 *   │     public BadSingleton(IMessageService svc) { _svc = svc; }               │
 *   │ }                                                                           │
 *   │ services.AddSingleton<BadSingleton>()                                       │
 *   │ // → host.Build() throws in Development:                                   │
 *   │ //   "Cannot consume scoped service 'IMessageService' from singleton."     │
 *   └─────────────────────────────────────────────────────────────────────────────┘
 *
 * FIX — IServiceScopeFactory:
 *   Inject IServiceScopeFactory (itself a singleton — safe) into the singleton.
 *   Create a scope when work begins, resolve the scoped service from that scope,
 *   and dispose the scope when done.  This is the correct pattern for background
 *   services (BackgroundService / IHostedService) that need a DbContext per unit of work.
 *
 *   CORRECT PATTERN:
 *   ┌─────────────────────────────────────────────────────────────────────────────┐
 *   │ public class GoodSingleton                                                  │
 *   │ {                                                                           │
 *   │     private readonly IServiceScopeFactory _factory;                        │
 *   │     public GoodSingleton(IServiceScopeFactory factory) { _factory = factory; }│
 *   │     public void DoWork()                                                    │
 *   │     {                                                                       │
 *   │         using var scope = _factory.CreateScope();                          │
 *   │         var svc = scope.ServiceProvider.GetRequiredService<IMessageService>();│
 *   │         svc.GetMessage("background");                                      │
 *   │     } // scope.Dispose() called here → IMessageService disposed            │
 *   │ }                                                                           │
 *   └─────────────────────────────────────────────────────────────────────────────┘
 */

var app = builder.Build(); // seals IServiceCollection; scope-graph validation runs in Development

app.UseRouting();   // adds routing middleware to the pipeline
app.MapControllers(); // discovers and maps DemoController → GET /api/demo/greet/{name}, etc.

/*
 * --- Minimal endpoints demonstrating IServiceScopeFactory and keyed service resolution ---
 * Minimal API handler parameters are resolved from DI automatically; all lifetime rules apply.
 */

// IServiceScopeFactory demo: create a manual scope and resolve a scoped service from it
app.MapGet("/scope-demo", (IServiceScopeFactory scopeFactory) =>
{
    using var scope = scopeFactory.CreateScope();                                           // new DI scope
    var svc = scope.ServiceProvider.GetRequiredService<IMessageService>();                  // scoped to this scope
    return Results.Ok(new { Message = svc.GetMessage("scope-demo"), InstanceId = svc.InstanceId });
    // scope.Dispose() called here → IMessageService.Dispose() called if implemented
});

// Keyed service demo: resolve the "primary" or "backup" named implementation by route key
app.MapGet("/keyed/{key}", (string key, HttpContext ctx) =>
{
    // GetKeyedService returns null for an unrecognised key; GetRequiredKeyedService throws
    var svc = ctx.RequestServices.GetKeyedService<IMessageService>(key);
    if (svc is null)
        return Results.NotFound(new { Error = $"No IMessageService registered for key '{key}'" });

    return Results.Ok(new { Key = key, Message = svc.GetMessage("keyed-user"), InstanceId = svc.InstanceId });
});

app.Run();

/*
 * QUICK REFERENCE — Dependency Injection & Service Lifetimes
 * ──────────────────────────────────────────────────────────
 *
 * REGISTRATION:
 *   builder.Services.AddSingleton<IFoo, Foo>()           one per app
 *   builder.Services.AddScoped<IFoo, Foo>()              one per request / scope
 *   builder.Services.AddTransient<IFoo, Foo>()           one per injection point
 *   builder.Services.TryAddSingleton<IFoo, Foo>()        only if not already registered
 *   builder.Services.AddKeyedScoped<IFoo, Foo>("key")    keyed by string / object (.NET 8)
 *
 * RESOLUTION:
 *   Constructor parameter  IFoo foo                      preferred
 *   Action parameter       [FromServices] IFoo foo       action-specific
 *   Keyed attribute        [FromKeyedServices("key")] IFoo foo
 *   Manual (required)      sp.GetRequiredService<IFoo>() throws if not registered
 *   Manual (optional)      sp.GetService<IFoo>()         null if not registered
 *   Keyed manual           sp.GetRequiredKeyedService<IFoo>("key")
 *   Keyed optional         sp.GetKeyedService<IFoo>("key")
 *
 * SERVICEDESCRIPTOR:
 *   ServiceDescriptor.Scoped<IFoo, Foo>()
 *   services.Add(descriptor) / services.Replace(descriptor) / services.RemoveAll<IFoo>()
 *
 * INJECTION SAFETY:
 *   Singleton → Singleton  ✓     Singleton → Scoped   ✗ CAPTIVE
 *   Scoped    → Any        ✓     Transient → Any      ✓
 *
 * CAPTIVE DEPENDENCY FIX:
 *   Inject IServiceScopeFactory (singleton-safe) into the singleton.
 *   Call factory.CreateScope() per unit of work; dispose the scope when done.
 *
 * BUILT-IN SERVICES (registered by WebApplication.CreateBuilder):
 *   ILogger<T>             structured logging — see 06. Logging & Diagnostics
 *   IConfiguration         appsettings / env vars — see 05. Configuration & Options Pattern
 *   IWebHostEnvironment    environment name (Development / Production)
 *   IServiceScopeFactory   creates DI scopes; safe to inject into singletons
 *   IHttpClientFactory     pooled HttpClient (requires AddHttpClient())
 *
 * FORWARD REFERENCES:
 *   05. Configuration & Options Pattern   IConfiguration, IOptions<T>
 *   06. Logging & Diagnostics             ILogger<T>, structured logging, sinks
 *   14. Background & Hosted Services      IServiceScopeFactory in BackgroundService
 */
