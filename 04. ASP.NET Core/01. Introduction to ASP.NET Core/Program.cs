/*
 * TOPIC: Introduction to ASP.NET Core
 * ─────────────────────────────────────────────────────────────────────────────
 * WHY IT MATTERS:
 *   ASP.NET Core is the modern, cross-platform, open-source successor to
 *   classic ASP.NET Framework. Every .NET web application — REST API, MVC
 *   site, gRPC service, SignalR hub, Blazor app — is built on its unified
 *   hosting and pipeline model. Understanding how the host starts, how the
 *   pipeline is assembled, and how DI wires services together is the
 *   foundation every later ASP.NET Core chapter builds on.
 *
 * WHAT YOU WILL LEARN:
 *    1. What ASP.NET Core is and why it replaced the classic framework
 *    2. How classic ASP.NET (System.Web) differs from ASP.NET Core
 *    3. The builder pattern: WebApplicationBuilder assembles the host
 *    4. Dependency injection preview: registering services with AddSingleton
 *    5. Reading configuration from appsettings.json at startup
 *    6. Building the host: what builder.Build() returns and seals
 *    7. Environment awareness: Development, Staging, Production
 *    8. Middleware pipeline preview: ordered chain of request handlers
 *    9. Endpoints and the request/response pipeline in action
 *   10. Running the app: what app.Run() does under the hood
 *   11. Response DTO shape: returning structured JSON from an endpoint
 *   12. Service class: defining and injecting a business-logic service
 *
 * CHAPTER MAP (multi-file project — open files in this order):
 *   SECTION  1  → Program.cs (this file)         What Is ASP.NET Core
 *   SECTION  2  → Program.cs                     Classic vs Modern ASP.NET
 *   SECTION  3  → Program.cs                     Builder Pattern (WebApplicationBuilder)
 *   SECTION  4  → Program.cs                     DI Registration Preview
 *   SECTION  5  → Program.cs                     Configuration Basics
 *   SECTION  6  → Program.cs                     Building the Host (WebApplication)
 *   SECTION  7  → Program.cs                     Environment Awareness
 *   SECTION  8  → Program.cs                     Middleware Pipeline Preview
 *   SECTION  9  → Program.cs                     Endpoints & Request/Response Pipeline
 *   SECTION 10  → Program.cs                     Running the App
 *   SECTION 11  → Models/WelcomeMessage.cs        Response DTO Shape
 *   SECTION 12  → Services/GreetingService.cs     Service Class + DI Lifecycle Notes
 */

/*
 * SECTION 1: WHAT IS ASP.NET CORE
 * ─────────────────────────────────────────────────────────────────────────────
 * ASP.NET Core is a cross-platform, high-performance, open-source framework
 * for building web applications and services with .NET.
 *
 * Key characteristics:
 *   Cross-platform   — runs on Windows, Linux, and macOS via .NET (not IIS-only)
 *   Modular          — only include the middleware and services you need
 *   Unified model    — one framework for MVC, REST APIs, Razor Pages, Blazor, gRPC
 *   Built-in DI      — Dependency Injection is first-class; not a bolt-on
 *   Kestrel          — ships its own high-performance HTTP server; no IIS required
 *   Open source      — https://github.com/dotnet/aspnetcore
 *
 * Request lifecycle at a glance:
 *   Client → Kestrel → Middleware chain → Endpoint handler → Response
 *
 * ASP.NET Core ships as part of the .NET SDK — no separate installer.
 * Every project type (API, MVC, Blazor, gRPC) shares the same startup model
 * you will learn in this chapter.
 *
 * Version history (major milestones):
 *   ASP.NET Core 1.0  (2016) — initial cross-platform release
 *   ASP.NET Core 2.1  (2018) — Razor Pages, SignalR, HttpClientFactory
 *   ASP.NET Core 3.1  (2019) — LTS; Endpoint routing, Worker Services
 *   ASP.NET Core 5.0  (2020) — Swagger UI, improved JSON; dropped "Core" branding
 *   ASP.NET Core 6.0  (2021) — LTS; WebApplication / CreateBuilder introduced
 *   ASP.NET Core 7.0  (2022) — Rate limiting, Output caching, Minimal API improvements
 *   ASP.NET Core 8.0  (2023) — LTS; Blazor United, Keyed services, AOT support
 *
 * COVERED IN DETAIL LATER:
 *   Hosting & Kestrel → 12. Hosting, Kestrel & Environments
 */

/*
 * SECTION 2: CLASSIC ASP.NET (System.Web) vs ASP.NET CORE
 * ─────────────────────────────────────────────────────────────────────────────
 * Understanding the differences helps you reason about architecture choices
 * and migration considerations when moving from legacy .NET Framework codebases.
 *
 * Feature                Classic ASP.NET (.NET Framework)   ASP.NET Core (modern .NET)
 * ─────────────────────  ─────────────────────────────────  ──────────────────────────────
 * Platform               Windows only (IIS required)        Windows, Linux, macOS
 * HTTP abstraction       System.Web.HttpContext             Microsoft.AspNetCore.Http
 * DI container           Not built in (NuGet: Unity, etc.)  Built-in IServiceCollection
 * HTTP server            IIS / IIS Express only             Kestrel (+ optional reverse proxy)
 * Config model           web.config (XML)                   JSON / env vars / user secrets
 * Pipeline model         HttpModules + HttpHandlers          Middleware delegates (linear chain)
 * Performance            ~50k req/s typical (IIS)           >7M req/s (Kestrel benchmark)
 * Open source            Partially (reference source)        Fully (github.com/dotnet/aspnetcore)
 * Cross-platform         No                                 Yes
 * Self-contained deploy  No                                 Yes (single-file, Docker)
 * Startup code           Global.asax + App_Start/           Program.cs (top-level statements)
 *
 * KEY ARCHITECTURAL CHANGE:
 *   Classic: HttpModules wired via XML; tightly coupled to System.Web.
 *   Modern:  Middleware delegates registered in C# code; any order; composable.
 *
 * Migration tooling: .NET Upgrade Assistant automates many mechanical changes,
 * but architectural shifts (pipeline, DI, config) require manual rework.
 *
 * COVERED IN DETAIL LATER:
 *   .NET Framework Architecture → 01. .NET Framework Architecture / 10. Migration
 */

using IntroductionToAspNetCore.Models;
using IntroductionToAspNetCore.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

/*
 * SECTION 3: THE BUILDER PATTERN — WebApplicationBuilder
 * ─────────────────────────────────────────────────────────────────────────────
 * WebApplication.CreateBuilder(args) returns a WebApplicationBuilder — a
 * configuration accumulator that collects everything the app needs BEFORE
 * the host is started:
 *
 *   builder.Services       → the DI container (IServiceCollection)
 *   builder.Configuration  → layered config (appsettings.json, env vars, args)
 *   builder.Logging        → logging providers (Console, Debug, EventLog, …)
 *   builder.Environment    → current environment name (Development / Production)
 *   builder.Host           → host-level settings (IHostBuilder surface)
 *   builder.WebHost        → Kestrel / server-level settings (IWebHostBuilder)
 *
 * TWO-PHASE DESIGN:
 *   Phase 1 — Configure:  call builder.Services.Add*(), builder.Configuration, …
 *   Phase 2 — Run:        call builder.Build() then app.Use*() + app.Run()
 *
 * Separating configuration from running lets the runtime validate the
 * container (detect missing registrations) before the first request.
 *
 * Before .NET 6 (CreateBuilder was introduced in .NET 6), startup was:
 *   Host.CreateDefaultBuilder(args)
 *     .ConfigureWebHostDefaults(web => { web.UseStartup<Startup>(); })
 *     .Build()
 *     .Run();
 *
 * CreateBuilder() does all of that in one call with sensible defaults, and
 * eliminates the separate Startup.cs class (though Startup is still supported).
 *
 * args: command-line arguments forwarded to the host configuration. This allows
 * overriding any config value at run time: dotnet run --AppName "Prod App"
 */
var builder = WebApplication.CreateBuilder(args); // args forwarded from the CLI to the host

/*
 * SECTION 4: DEPENDENCY INJECTION REGISTRATION — PREVIEW
 * ─────────────────────────────────────────────────────────────────────────────
 * ASP.NET Core's built-in DI container (IServiceCollection) is populated during
 * the builder phase. Three lifetimes control how often a service is created:
 *
 *   Lifetime       Registration call              When a new instance is created
 *   ─────────────  ─────────────────────────────  ────────────────────────────────
 *   Singleton      AddSingleton<T>()              Once; shared for app lifetime
 *   Scoped         AddScoped<T>()                 Once per HTTP request
 *   Transient      AddTransient<T>()              Every time the type is resolved
 *
 * Lifetime choice matters for correctness and thread safety:
 *   • Singleton services must be thread-safe (shared across all concurrent requests).
 *   • Scoped services are tied to the request lifetime (DbContext is the classic example).
 *   • Transient services are cheap, stateless, and short-lived (validators, factories).
 *
 * CAPTIVE DEPENDENCY PITFALL:
 *   If a Singleton depends on a Scoped service, the Scoped service is "captured"
 *   and lives for the app lifetime — breaking request isolation. ASP.NET Core
 *   detects this in Development mode and throws InvalidOperationException.
 *
 * GreetingService is stateless → Singleton is the correct and most efficient choice.
 * See Services/GreetingService.cs (SECTION 12) for the service definition.
 *
 * COVERED IN DETAIL LATER → 04. Dependency Injection & Service Lifetimes
 */
builder.Services.AddSingleton<GreetingService>(); // one shared instance for the app lifetime

/*
 * SECTION 5: CONFIGURATION BASICS
 * ─────────────────────────────────────────────────────────────────────────────
 * builder.Configuration is a layered IConfiguration assembled from multiple
 * sources, evaluated in priority order (later sources override earlier ones):
 *
 *   Priority  Source                                 Example key
 *   ────────  ─────────────────────────────────────  ────────────────────────
 *      1      appsettings.json                       "AppName": "Intro App"
 *      2      appsettings.{Environment}.json         appsettings.Production.json
 *      3      Environment variables                  ASPNETCORE__AppName=Prod
 *      4      Command-line arguments                 --AppName "Prod App"
 *
 * The string indexer returns string? (nullable). Chaining with ?? provides a
 * non-nullable fallback so appName is typed as string, not string?.
 *
 * For structured/typed config (strongly-typed options classes), see:
 * COVERED IN DETAIL LATER → 05. Configuration & Options Pattern
 */
var appName = builder.Configuration["AppName"] ?? "ASP.NET Core App"; // string (never null)

/*
 * SECTION 6: BUILDING THE HOST — WebApplication
 * ─────────────────────────────────────────────────────────────────────────────
 * builder.Build() finalises the DI container and returns a WebApplication —
 * the running host object that implements three key interfaces at once:
 *
 *   IApplicationBuilder    — assembles the middleware pipeline (app.Use*())
 *   IEndpointRouteBuilder  — maps routes to handlers (app.MapGet(), etc.)
 *   IHost                  — manages application lifetime (Start, Stop, Run)
 *
 * AFTER Build(), you cannot add new service registrations — builder.Services
 * is sealed. Any attempt to call builder.Services.Add*() after Build() throws
 * InvalidOperationException.
 *
 * app.Services is the read-only IServiceProvider. Use it to resolve services
 * manually (scoped to a created scope) — rarely needed; prefer constructor
 * injection or handler parameter injection in normal application code.
 */
var app = builder.Build(); // DI container sealed; host is ready to configure the pipeline

/*
 * SECTION 7: ENVIRONMENT AWARENESS
 * ─────────────────────────────────────────────────────────────────────────────
 * The current environment is set by the ASPNETCORE_ENVIRONMENT environment
 * variable. When not set, the framework defaults to "Production".
 *
 * Standard environments:
 *   Development  — detailed errors; hot-reload; relaxed security; local dev
 *   Staging      — production-like config; pre-release testing; CI/CD smoke tests
 *   Production   — hardened; minimal error detail; optimised caching; live traffic
 *
 * app.Environment exposes IWebHostEnvironment:
 *
 *   Method                     Equivalent condition
 *   ─────────────────────────  ──────────────────────────────────────────────
 *   IsDevelopment()            ASPNETCORE_ENVIRONMENT == "Development"
 *   IsStaging()                ASPNETCORE_ENVIRONMENT == "Staging"
 *   IsProduction()             ASPNETCORE_ENVIRONMENT == "Production"
 *   IsEnvironment("Custom")    ASPNETCORE_ENVIRONMENT == "Custom"
 *
 * In Properties/launchSettings.json, ASPNETCORE_ENVIRONMENT is typically set
 * to "Development" for local runs so developer tooling activates automatically.
 *
 * Configuration layering respects the environment: appsettings.Development.json
 * overrides appsettings.json values only in Development — a clean way to keep
 * dev/prod secrets separate without branching in code.
 *
 * COVERED IN DETAIL LATER → 12. Hosting, Kestrel & Environments
 */
if (app.Environment.IsDevelopment()) // true when ASPNETCORE_ENVIRONMENT = "Development"
{
    app.UseDeveloperExceptionPage(); // detailed exception info; never expose in Production
}

/*
 * SECTION 8: MIDDLEWARE PIPELINE — PREVIEW
 * ─────────────────────────────────────────────────────────────────────────────
 * ASP.NET Core processes every HTTP request through a PIPELINE of middleware
 * components. Each component:
 *   1. Receives the HttpContext (request + response wrapper)
 *   2. Optionally modifies the request or response
 *   3. Either short-circuits (writes a response and returns) OR calls next()
 *      to pass control to the next component in the chain
 *
 * PIPELINE DIRECTION — middleware runs in order on the way IN and in reverse
 * order on the way OUT (like a stack of Russian dolls):
 *
 *   Request  → [A] → [B] → [C] → Endpoint handler
 *   Response ← [A] ← [B] ← [C] ←
 *
 * ORDER MATTERS:
 *   Security middleware must precede business logic.
 *   Routing must precede Authorization.
 *   Static file serving should precede routing (to short-circuit early).
 *
 * Built-in middleware reference:
 *   UseHttpsRedirection()    redirect HTTP → HTTPS
 *   UseStaticFiles()         serve files from wwwroot/
 *   UseRouting()             match URL to endpoint (implicit with MapGet in .NET 6+)
 *   UseAuthentication()      who is this user? (reads the token/cookie)
 *   UseAuthorization()       is the user allowed? (checks policies/roles)
 *   UseEndpoints()           execute the matched endpoint (implicit with MapGet)
 *
 * Custom middleware:
 *   app.Use(async (context, next) => { ...before... await next(); ...after... });
 *   app.Run(async context => { ...terminal — no next() call... });
 *
 * In Minimal API projects, UseRouting + UseEndpoints are called implicitly
 * when you call app.MapGet() / app.MapPost() etc.
 *
 * COVERED IN DETAIL LATER → 03. Middleware Pipeline
 */
app.UseHttpsRedirection(); // redirect HTTP requests to HTTPS; safe in all environments

/*
 * SECTION 9: ENDPOINTS AND THE REQUEST/RESPONSE PIPELINE
 * ─────────────────────────────────────────────────────────────────────────────
 * Endpoints are terminal middleware — they run after all other middleware and
 * produce the final HTTP response. ASP.NET Core supports two endpoint styles:
 *
 *   Minimal APIs (used here):
 *     app.MapGet("/path", handler)
 *     app.MapPost / MapPut / MapDelete / MapPatch
 *
 *   Controller-based (MVC / Web API):
 *     builder.Services.AddControllers();
 *     app.MapControllers();
 *
 * HANDLER PARAMETER BINDING — framework resolves handler parameters by:
 *
 *   Source            Match condition               Example
 *   ────────────────  ───────────────────────────  ──────────────────────────
 *   Route value       Parameter name in {pattern}  (string name) ← {name}
 *   Query string      Parameter name in ?key=val   (int? page)
 *   DI container      Type registered in Services   (GreetingService svc)
 *   Request body      [FromBody] attribute           ([FromBody] OrderDto dto)
 *
 * RETURN TYPES — handlers return IResult; Results factory produces typed responses:
 *   Results.Ok(payload)          200 OK  + JSON body
 *   Results.Created(uri, obj)    201 Created
 *   Results.NotFound()           404 Not Found
 *   Results.BadRequest(errors)   400 Bad Request
 *   Results.NoContent()          204 No Content
 *
 * REQUEST/RESPONSE CYCLE:
 *   ┌─────────────────────────────────────────────────────────────────┐
 *   │  Client → Kestrel → Middleware chain → Endpoint handler         │
 *   │       ←──────────────────────────────────────────────────────── │
 *   │  Response ← JSON serialization ← IResult ← handler return      │
 *   └─────────────────────────────────────────────────────────────────┘
 *
 * See Models/WelcomeMessage.cs (SECTION 11) for the DTO returned below.
 * See Services/GreetingService.cs (SECTION 12) for the injected service.
 *
 * COVERED IN DETAIL LATER:
 *   Routing   → 07. Routing & Endpoints
 *   Minimal APIs → 13. Minimal APIs
 */

// GET /  →  returns a WelcomeMessage DTO as JSON
app.MapGet("/", (GreetingService svc) =>  // GreetingService resolved from DI container
{
    var message = new WelcomeMessage(appName, svc.Greet("World")); // build response DTO
    return Results.Ok(message);                                     // 200 OK + JSON body
});

// GET /greet/{name}  →  personalised greeting using the {name} route segment
app.MapGet("/greet/{name}", (string name, GreetingService svc) => // name from URL segment
{
    var message = new WelcomeMessage(appName, svc.Greet(name)); // personalise the greeting
    return Results.Ok(message);
});

/*
 * SECTION 10: RUNNING THE APP
 * ─────────────────────────────────────────────────────────────────────────────
 * app.Run() starts the Kestrel server and BLOCKS the main thread until the
 * application shuts down (Ctrl+C, SIGTERM, or programmatic StopApplication()).
 *
 * What happens inside app.Run():
 *   1. Finalises the middleware pipeline (seals app.Use*() — no more additions)
 *   2. Starts Kestrel on configured URLs:
 *        Default: http://localhost:5000 and https://localhost:5001
 *        Override: ASPNETCORE_URLS env var, or builder.WebHost.UseUrls("…")
 *   3. Starts all IHostedService background services
 *   4. Enters the application loop — accepts, routes, and dispatches requests
 *   5. On shutdown signal: calls IHostedService.StopAsync(), disposes services
 *
 * Variants:
 *   app.Run()           blocking (most common for production entry points)
 *   app.RunAsync()      async non-blocking (use with await in async Main)
 *   app.Start()         non-blocking (useful in tests and hosted scenarios)
 *   app.StartAsync()    async non-blocking variant of Start
 *
 * COVERED IN DETAIL LATER:
 *   Background services → 14. Background & Hosted Services
 *   Hosting & Kestrel  → 12. Hosting, Kestrel & Environments
 */
app.Run(); // start the host; this line does not return until shutdown

/*
 * QUICK REFERENCE — Introduction to ASP.NET Core
 * ─────────────────────────────────────────────────────────────────────────────
 *
 * 1. CREATE THE BUILDER
 *    var builder = WebApplication.CreateBuilder(args);
 *
 * 2. REGISTER SERVICES (DI)
 *    builder.Services.AddSingleton<T>()    // one instance, whole app lifetime
 *    builder.Services.AddScoped<T>()       // one instance per HTTP request
 *    builder.Services.AddTransient<T>()    // new instance every resolution
 *
 * 3. READ CONFIGURATION
 *    builder.Configuration["Key"]                          // string? indexer
 *    builder.Configuration["Section:SubKey"]               // nested key path
 *    builder.Configuration.GetSection("Section").Value     // section value
 *    builder.Configuration.GetValue<int>("Port", 5000)     // typed with default
 *
 * 4. BUILD THE HOST
 *    var app = builder.Build();    // seals DI; returns WebApplication
 *
 * 5. CHECK ENVIRONMENT
 *    app.Environment.IsDevelopment()        // ASPNETCORE_ENVIRONMENT = "Development"
 *    app.Environment.IsStaging()
 *    app.Environment.IsProduction()
 *    app.Environment.IsEnvironment("Name")  // custom environment
 *
 * 6. ADD MIDDLEWARE (order matters)
 *    app.UseDeveloperExceptionPage()        // dev only — detailed error pages
 *    app.UseHttpsRedirection()              // HTTP → HTTPS redirect
 *    app.UseStaticFiles()                   // serve wwwroot/
 *    app.UseRouting()                       // match URL to endpoint (often implicit)
 *    app.UseAuthentication()                // identity: who?
 *    app.UseAuthorization()                 // access: allowed?
 *    app.Use(async (ctx, next) => { … })    // inline custom middleware
 *    app.Run(async ctx => { … })            // terminal inline middleware
 *
 * 7. MAP ENDPOINTS
 *    app.MapGet("/path", handler)
 *    app.MapPost("/path", handler)
 *    app.MapPut("/path/{id}", handler)
 *    app.MapDelete("/path/{id}", handler)
 *    app.MapControllers()                   // MVC controller routing
 *
 * 8. RETURN RESULTS FROM HANDLERS
 *    Results.Ok(payload)          // 200 + JSON body
 *    Results.Created(uri, obj)    // 201
 *    Results.NotFound()           // 404
 *    Results.BadRequest(errors)   // 400
 *    Results.NoContent()          // 204
 *    Results.Problem(detail)      // RFC 7807 problem detail
 *
 * 9. START THE HOST
 *    app.Run()             // blocking — standard entry point
 *    app.RunAsync()        // async blocking
 *    app.Start()           // non-blocking — test scenarios
 *
 * NEXT CHAPTERS:
 *   02. Project Structure & Program.cs       — csproj, launchSettings, wwwroot layout
 *   03. Middleware Pipeline                  — full middleware depth + custom middleware
 *   04. Dependency Injection & Lifetimes     — DI internals, lifetimes, scopes, IOptions<T>
 *   05. Configuration & Options Pattern      — IConfiguration, IOptions<T>, secrets
 *   07. Routing & Endpoints                  — route patterns, constraints, metadata
 *   12. Hosting, Kestrel & Environments      — Kestrel config, HTTPS, environment variables
 *   13. Minimal APIs                         — full Minimal API surface and conventions
 */
