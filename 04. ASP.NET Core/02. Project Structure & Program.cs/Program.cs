/*
 * TOPIC: Project Structure & Program.cs
 *
 * WHY IT MATTERS:
 *   Program.cs is the single entry point for every ASP.NET Core application.
 *   Everything — configuration loading, dependency injection registration, the
 *   middleware pipeline, and endpoint routing — flows through here before the
 *   server starts accepting HTTP requests. Understanding how the project files
 *   relate to each other and how WebApplicationBuilder bootstraps the host is
 *   the foundation for every topic in this module.
 *
 * WHAT YOU WILL LEARN:
 *    1. The .csproj file: SDK attribute, TargetFramework, ImplicitUsings, Nullable
 *    2. Solution files (.sln): grouping multi-project solutions
 *    3. Folder conventions: wwwroot/, Properties/, bin/, obj/
 *    4. launchSettings.json: IIS Express vs. Kestrel profiles, env-var injection
 *    5. appsettings.json + appsettings.{Env}.json: layered configuration
 *    6. WebApplicationBuilder: the bootstrap container (CreateBuilder pattern)
 *    7. Generic host vs. web host — what WebApplication unifies
 *    8. builder.Services: DI service registration (PREVIEW -> 04. DI & Service Lifetimes)
 *    9. builder.Configuration: layered sources and environment-variable overrides
 *   10. builder.Logging: adding providers and setting minimum levels
 *   11. WebApplication.Build(): sealing the DI container; creating the app
 *   12. Middleware pipeline: app.Use / app.Map / app.Run (PREVIEW -> 03. Middleware Pipeline)
 *   13. Endpoint mapping: MapGet, MapControllers
 *   14. app.Run(): binding to ports and starting Kestrel
 *   15. Typed configuration sections: binding JSON to a C# record
 *
 * CHAPTER MAP (open files in this order):
 *   1. Typed config record          -> Models/AppInfo.cs
 *   2. Launch profiles              -> Properties/launchSettings.json
 *   3. App configuration (base)     -> appsettings.json
 *   4. Dev environment overrides    -> appsettings.Development.json
 *   5. Host bootstrap + pipeline    -> Program.cs  (this file, sections below)
 */

using System;                                   // Console (ImplicitUsings disabled — all imports explicit)
using Microsoft.AspNetCore.Builder;             // WebApplicationBuilder, WebApplication, app.Use*
using Microsoft.Extensions.Configuration;       // IConfiguration, GetSection, Get<T>
using Microsoft.Extensions.DependencyInjection; // AddControllers, AddAuthorization
using Microsoft.Extensions.Hosting;             // IsDevelopment() extension method
using Microsoft.Extensions.Logging;             // LogLevel, ClearProviders, AddConsole
using ProjectStructure.Models;                  // AppInfo — typed config record (Section 9)

// ─── SECTION 1: THE PROJECT FILE (.csproj) AND SOLUTION (.sln) ─────────────
/*
 * SECTION 1: THE PROJECT FILE (.csproj) AND SOLUTION (.sln)
 *
 * Open ProjectStructure.csproj to see the full file. Key elements:
 *
 *   Sdk="Microsoft.NET.Sdk.Web"
 *     Extends the base SDK with ASP.NET Core build targets and the
 *     Microsoft.AspNetCore.App shared framework reference. This gives you
 *     Kestrel, routing, middleware, DI, and all built-in ASP.NET Core packages
 *     without any explicit PackageReference entries.
 *     Sdk.Worker  -> for background services that do not serve HTTP.
 *     Sdk.Default -> for console apps and class libraries.
 *
 *   <TargetFramework>net8.0</TargetFramework>
 *     The Target Framework Moniker (TFM). net8.0 targets .NET 8 LTS.
 *     Multi-targeting: <TargetFrameworks>net6.0;net8.0</TargetFrameworks>
 *
 *   <ImplicitUsings>disable</ImplicitUsings>
 *     Prevents the SDK from generating a GlobalUsings.g.cs file in obj/.
 *     When enabled (default), namespaces like System, System.IO, and System.Linq
 *     are injected invisibly. This repo disables it so every import is explicit.
 *
 *   <Nullable>enable</Nullable>
 *     Activates C# 8+ nullable reference type analysis. The compiler emits
 *     warnings (CS8600-CS8629) when a nullable reference might be dereferenced.
 *
 *   <PackageReference Include="Foo" Version="1.0" />
 *     Adds a NuGet package. Sdk.Web bundles Microsoft.AspNetCore.App, so you
 *     only need PackageReference for third-party libraries (Serilog, AutoMapper, etc.).
 *
 * SOLUTION FILE (.sln):
 *   A .sln file groups one or more .csproj files so the CLI and IDEs can build,
 *   test, and publish related projects together:
 *     dotnet sln MySolution.sln add src/MyApi/MyApi.csproj
 *     dotnet build MySolution.sln
 *   The .sln format is proprietary text managed by tooling — treat it as
 *   infrastructure, not hand-edited source.
 *
 * FOLDER CONVENTIONS:
 *   wwwroot/          Static web assets (HTML, CSS, JS, images) served by UseStaticFiles().
 *   Properties/       launchSettings.json — developer launch profiles, not deployed.
 *   Models/           C# types that represent domain entities, DTOs, or config sections.
 *   bin/              Build output directory (gitignored).
 *   obj/              Intermediate build artifacts, generated code (gitignored).
 *   appsettings.json  Runtime configuration file — deployed with the application.
 */

// ─── SECTION 2: WEBAPPLICATIONBUILDER — THE BOOTSTRAP CONTAINER ────────────
/*
 * SECTION 2: WEBAPPLICATIONBUILDER — THE BOOTSTRAP CONTAINER
 *
 * WebApplication.CreateBuilder(args) is the modern entry point for configuring
 * an ASP.NET Core application. It returns a WebApplicationBuilder that exposes
 * all configuration surfaces in one object:
 *
 *   builder.Services        IServiceCollection  — register DI services
 *   builder.Configuration   IConfigurationManager — aggregate configuration sources
 *   builder.Logging         ILoggingBuilder — add log providers and set levels
 *   builder.Environment     IWebHostEnvironment — environment name, ContentRootPath, WebRootPath
 *   builder.Host            IHostBuilder — generic host (background services, lifetime)
 *   builder.WebHost         IWebHostBuilder — web-specific (Kestrel URLs, server options)
 *
 * GENERIC HOST vs. WEB HOST (historical context):
 *   Before .NET 6 there were two separate builders:
 *     IWebHostBuilder  — ASP.NET Core web apps (Kestrel, middleware pipeline)
 *     IHostBuilder     — generic host for background services (IHostedService)
 *   They had overlapping APIs and caused confusion when mixing both.
 *   WebApplication (.NET 6+) unifies them:
 *     - Internally wraps IHost (the generic host) for IHostedService, shutdown, DI.
 *     - Adds the Kestrel HTTP server and request pipeline on top.
 *   Use builder.Host for generic host settings; builder.WebHost for Kestrel settings.
 *   For most apps, touch neither — CreateBuilder sets sensible defaults automatically.
 *
 * DEFAULT CONFIGURATION SOURCES (loaded by CreateBuilder, in priority order — last wins):
 *   1. appsettings.json
 *   2. appsettings.{ASPNETCORE_ENVIRONMENT}.json  (e.g. appsettings.Development.json)
 *   3. User secrets (Development environment only, keyed by UserSecretsId in .csproj)
 *   4. Environment variables (ASPNETCORE_ prefix stripped for top-level keys)
 *   5. Command-line arguments (--key value or --key=value)
 *
 * LEGACY PATTERN (CreateDefaultBuilder) vs. MODERN (CreateBuilder):
 *   // Legacy — still valid; uses Startup class
 *   IHostBuilder host = Host.CreateDefaultBuilder(args)
 *       .ConfigureWebHostDefaults(web => web.UseStartup<Startup>());
 *
 *   // Modern — minimal hosting (.NET 6+)
 *   WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
 *   Both configure the same defaults; the modern pattern is preferred.
 */
WebApplicationBuilder builder = WebApplication.CreateBuilder(args); // seeds config from CLI args

// ─── SECTION 3: SERVICE REGISTRATION (PREVIEW -> 04. DI & Service Lifetimes) ─
/*
 * SECTION 3: SERVICE REGISTRATION — builder.Services
 *
 * builder.Services is an IServiceCollection — a list of ServiceDescriptor entries
 * that tell the DI container what to instantiate when a type is requested.
 * Call extension methods here before builder.Build():
 *
 *   COMMON REGISTRATIONS:
 *     AddControllers()           MVC controllers with attribute routing.
 *     AddControllersWithViews()  MVC controllers + Razor view engine.
 *     AddRazorPages()            Razor Pages (page-based routing).
 *     AddEndpointsApiExplorer()  OpenAPI/Swagger metadata for minimal API routes.
 *     AddAuthorization()         Authorization policy service.
 *     AddAuthentication()        Authentication handlers (JWT, Cookies, etc.).
 *
 *   MANUAL REGISTRATION:
 *     builder.Services.AddSingleton<IMyService, MyService>();
 *     builder.Services.AddScoped<IMyRepo, MyRepo>();
 *     builder.Services.AddTransient<IValidator, Validator>();
 *
 *   COVERED IN DETAIL -> 04. Dependency Injection & Service Lifetimes
 *   (Singleton / Scoped / Transient lifetimes, IServiceProvider, factory delegates)
 */
builder.Services.AddControllers();          // enables app.MapControllers() below
builder.Services.AddEndpointsApiExplorer(); // Swagger metadata for minimal API endpoints
builder.Services.AddAuthorization();        // required before app.UseAuthorization()

// ─── SECTION 4: CONFIGURATION — builder.Configuration ──────────────────────
/*
 * SECTION 4: CONFIGURATION — builder.Configuration
 *
 * builder.Configuration is an IConfigurationManager (implements both IConfiguration
 * and IConfigurationBuilder). It aggregates all sources into one flat key/value store
 * with colon (:) as the path separator for nested keys.
 *
 * READING VALUES:
 *   string? name    = builder.Configuration["AppInfo:Name"];           // nested path
 *   string? connStr = builder.Configuration["ConnectionStrings:Default"];
 *   int port        = builder.Configuration.GetValue<int>("Port", 5000); // typed + default
 *
 * READING A SECTION:
 *   IConfigurationSection section = builder.Configuration.GetSection("AppInfo");
 *   // Returns a non-null section object even if the key does not exist in any source.
 *
 * BINDING A SECTION TO A TYPED OBJECT:
 *   AppInfo? info = builder.Configuration.GetSection("AppInfo").Get<AppInfo>();
 *   // Null if the section has no children. See Models/AppInfo.cs for the record definition.
 *
 * ENVIRONMENT-VARIABLE OVERRIDES (override any appsettings value without file changes):
 *   Set ASPNETCORE_ENVIRONMENT=Production   -> switches the appsettings.{Env}.json layer.
 *   Override nested keys with double underscore as separator (works on all OSes):
 *     AppInfo__Name=MyApp       overrides "AppInfo": { "Name": "..." } in appsettings
 *     AppInfo__Version=2.0.0    overrides "AppInfo": { "Version": "..." }
 *   On Linux, a colon (:) also works as separator in some providers.
 *
 * ADDING CUSTOM SOURCES:
 *   builder.Configuration.AddJsonFile("secrets.json", optional: true, reloadOnChange: true);
 *   builder.Configuration.AddEnvironmentVariables(prefix: "MYAPP_");
 *   builder.Configuration.AddAzureKeyVault(new Uri("..."), new DefaultAzureCredential());
 */
// Read AppInfo at builder phase — before Build() — to demonstrate startup diagnostics.
AppInfo? startupInfo = builder.Configuration
    .GetSection("AppInfo")
    .Get<AppInfo>(); // returns null if "AppInfo" section is absent from all sources

Console.WriteLine(                                                    // builder-phase console log
    $"[Startup] {startupInfo?.Name ?? "Unknown App"} v{startupInfo?.Version ?? "?"}");

// ─── SECTION 5: LOGGING — builder.Logging ───────────────────────────────────
/*
 * SECTION 5: LOGGING — builder.Logging
 *
 * builder.Logging is an ILoggingBuilder. CreateBuilder() pre-registers Console and
 * Debug providers; ClearProviders() removes them so you have explicit control.
 *
 * BUILT-IN PROVIDERS:
 *   AddConsole()     Structured output to stdout (text or JSON format).
 *   AddDebug()       Output to the debugger output window (dev only).
 *   AddEventLog()    Windows Event Log (Windows only).
 *   AddEventSourceLogger() ETW/EventSource tracing (cross-platform).
 *
 * LOG LEVELS (ascending severity — Trace < Debug < Information < Warning < Error < Critical):
 *   LogLevel.Trace       Fine-grained diagnostic; very high volume.
 *   LogLevel.Information Normal operational messages (service started, request served).
 *   LogLevel.Warning     Unexpected situation; app continues normally.
 *   LogLevel.Error       A failure that stops the current operation.
 *   LogLevel.Critical    Application-crashing failure.
 *   LogLevel.None        Disables all logging for a category.
 *
 * CATEGORY FILTERING:
 *   The "Logging.LogLevel" section in appsettings.json applies per namespace prefix:
 *     "Microsoft.AspNetCore": "Warning"   suppress ASP.NET Core framework noise below Warning.
 *   This lets you keep your app logs at Information while silencing verbose framework logs.
 *
 * THIRD-PARTY SINKS (popular choices):
 *   builder.Host.UseSerilog(...)   Serilog — structured, multi-sink logging
 *   builder.Logging.AddNLog(...)   NLog — high-performance, rule-based
 *   builder.Logging.AddOpenTelemetry(...) OpenTelemetry — cloud-native observability
 *
 * COVERED IN DETAIL -> 06. Logging & Diagnostics
 */
builder.Logging.ClearProviders();                       // remove SDK-default providers
builder.Logging.AddConsole();                           // structured stdout
builder.Logging.SetMinimumLevel(LogLevel.Information);  // suppress Trace and Debug globally

// ─── SECTION 6: WEBAPPLICATION.BUILD() — SEALING THE CONTAINER ─────────────
/*
 * SECTION 6: WEBAPPLICATION.BUILD() — SEALING THE CONTAINER
 *
 * builder.Build() finalizes all service and configuration registrations, then
 * returns a WebApplication instance. After this call:
 *   - The DI container is sealed: no more builder.Services calls are valid.
 *   - All IConfigurationSource entries are locked in.
 *   - app.Services can be used to resolve services directly (use sparingly).
 *
 * The returned WebApplication implements multiple interfaces:
 *   IApplicationBuilder    -> app.Use*() middleware registration
 *   IEndpointRouteBuilder  -> app.MapGet(), app.MapControllers() endpoint mapping
 *   IHost                  -> app.Services (IServiceProvider), app.Lifetime, app.StartAsync()
 *   IWebHostEnvironment    -> via app.Environment (EnvironmentName, ContentRootPath, WebRootPath)
 *
 * READING A SERVICE AFTER BUILD:
 *   using IServiceScope scope = app.Services.CreateScope();
 *   var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
 *   await db.Database.MigrateAsync(); // common pattern: migrate DB at startup
 */
WebApplication app = builder.Build(); // DI container sealed after this line

// ─── SECTION 7: MIDDLEWARE PIPELINE (PREVIEW -> 03. Middleware Pipeline) ────
/*
 * SECTION 7: MIDDLEWARE PIPELINE — app.Use / app.Map / app.Run
 *
 * After Build(), you construct the HTTP request pipeline by chaining middleware.
 * Each component receives an HttpContext, can inspect/modify the request and response,
 * optionally passes control to the next component, and runs code after the next returns.
 *
 * THREE REGISTRATION VERBS:
 * +---------------------------------+--------------------------------------------+
 * | app.Use(next => async ctx => {  | Adds a middleware that can short-circuit    |
 * |   // before                     | or pass to next by calling await next(ctx). |
 * |   await next(ctx);              | Most UseXxx() helpers wrap this pattern.    |
 * |   // after                      |                                             |
 * | })                              |                                             |
 * +---------------------------------+--------------------------------------------+
 * | app.Map("/prefix", branch => {  | Branches the pipeline for a URL prefix.    |
 * |   branch.Run(ctx => { ... });   | Requests not matching the prefix skip the  |
 * | })                              | branch entirely.                            |
 * +---------------------------------+--------------------------------------------+
 * | app.Run(ctx => { ... })         | Terminal — never calls next. Ends the      |
 * |                                 | pipeline. app.Run() (no args) below is      |
 * |                                 | different — it starts the server.           |
 * +---------------------------------+--------------------------------------------+
 *
 * PIPELINE ORDER MATTERS:
 *   Middleware runs top-to-bottom on the inbound request and bottom-to-top on the
 *   outbound response. Exception handling must be first so it can catch errors from
 *   all downstream components.
 *
 * RECOMMENDED STANDARD ORDER:
 *   1. UseExceptionHandler / UseDeveloperExceptionPage   <- exception handling outermost
 *   2. UseHsts / UseHttpsRedirection
 *   3. UseStaticFiles                                    <- before routing (no auth needed)
 *   4. UseRouting                                        <- match request to endpoint
 *   5. UseCors
 *   6. UseAuthentication
 *   7. UseAuthorization                                  <- after auth established
 *   8. MapControllers / MapGet                           <- execute matched endpoint
 *
 * COVERED IN DETAIL -> 03. Middleware Pipeline
 */
if (app.Environment.IsDevelopment())          // reads ASPNETCORE_ENVIRONMENT at runtime
{
    app.UseDeveloperExceptionPage();           // detailed HTML error page with stack trace
}

app.UseStaticFiles();   // serve files from wwwroot/ without auth      PREVIEW -> 11. Static Files
app.UseRouting();       // match URL path to a registered endpoint      PREVIEW -> 07. Routing
app.UseAuthorization(); // enforce [Authorize] attributes on endpoints  PREVIEW -> future Auth topic

// ─── SECTION 8: ENDPOINT MAPPING ────────────────────────────────────────────
/*
 * SECTION 8: ENDPOINT MAPPING — MapGet, MapControllers
 *
 * Endpoint mapping connects URL patterns to handler delegates or controller actions.
 * In the minimal-API style, use MapGet/MapPost/MapPut/MapDelete directly on app.
 * In the MVC style, use MapControllers() and attribute routing on controller classes.
 *
 * MINIMAL API ROUTE REGISTRATION:
 *   app.MapGet("/path", handler)
 *   The handler is a delegate (lambda or named method). Parameters are resolved
 *   automatically ("parameter binding") from:
 *     - Route values: /items/{id} -> (int id)
 *     - Query string: /items?page=2 -> (int page)
 *     - Request body: [FromBody] or inferred for complex types with POST
 *     - DI services: IConfiguration, ILogger<T>, and any registered service
 *
 * ROUTE CONSTRAINTS:
 *   app.MapGet("/items/{id:int}", (int id) => Results.Ok(id));
 *   :int -> fails with 404 if the segment is not a valid integer.
 *   Other constraints: :guid, :alpha, :minlength(3), :range(1,100)
 *
 * RESULT TYPES:
 *   return "text"             -> text/plain 200
 *   return Results.Ok(obj)   -> application/json 200
 *   return Results.NotFound() -> 404
 *   return Results.Created("/items/1", obj) -> 201 with Location header
 *
 * COVERED IN DETAIL -> 07. Routing & Endpoints, 08. Model Binding & Validation
 */
app.MapGet("/", () => "ASP.NET Core Project Structure — Tutorial Running"); // text/plain 200

app.MapGet("/info", (IConfiguration config) =>          // IConfiguration resolved from DI
{
    AppInfo? info = config.GetSection("AppInfo").Get<AppInfo>(); // bind section to record
    return info is not null
        ? $"App: {info.Name} v{info.Version} (Env: {info.Environment})" // formatted response
        : "AppInfo section not configured";
});

app.MapControllers(); // discovers classes marked [ApiController] and their [Route] attributes

// ─── SECTION 9: app.Run() — STARTING THE SERVER ─────────────────────────────
/*
 * SECTION 9: app.Run() — STARTING THE SERVER
 *
 * app.Run() (no parameters) starts Kestrel and blocks the calling thread until a
 * shutdown signal is received (Ctrl+C in terminal, SIGTERM from OS/container, or
 * a call to app.StopAsync()).
 *
 * OVERLOADS:
 *   app.Run();                    uses URL from launchSettings / ASPNETCORE_URLS
 *   app.Run("http://+:5000");     explicit override for this invocation
 *
 * URL CONFIGURATION PRIORITY (highest wins):
 *   1. Command-line: dotnet run --urls "http://+:5001"
 *   2. Environment variable: ASPNETCORE_URLS=http://+:5001
 *   3. launchSettings.json applicationUrl (Development only, dotnet run)
 *   4. Kestrel defaults configured in appsettings.json under "Kestrel:Endpoints"
 *   5. Built-in fallback: http://localhost:5000
 *
 * GRACEFUL SHUTDOWN SEQUENCE:
 *   1. Stop accepting new connections.
 *   2. Drain in-flight requests (configurable timeout via IHostApplicationLifetime).
 *   3. Call StopAsync() on all IHostedService implementations (in reverse order).
 *   4. Dispose the DI container (Dispose() called on all Singleton/Scoped services).
 *
 * KESTREL vs. REVERSE PROXY (production deployment):
 *   In production, Kestrel typically sits behind a reverse proxy (nginx, IIS, YARP, AWS ALB).
 *   The proxy handles TLS termination, load balancing, and static asset caching;
 *   it forwards plain HTTP to Kestrel on a loopback address or Unix socket.
 *   Use app.UseForwardedHeaders() or YARP's forwarding middleware to pass client IP
 *   and original Host header from the proxy to ASP.NET Core.
 */
app.Run(); // blocks until shutdown; exits cleanly on Ctrl+C or SIGTERM

/*
 * QUICK REFERENCE
 * ─────────────────────────────────────────────────────────────────────────────
 * WebApplication.CreateBuilder(args)          Bootstrap — seeds DI, config, logging
 * builder.Services.Add*()                     Register services (before Build())
 * builder.Configuration["Section:Key"]        Flat key with colon path separator
 * builder.Configuration.GetSection("K")       IConfigurationSection (never null)
 * builder.Configuration.GetSection("K").Get<T>() Bind section to typed object; null if absent
 * builder.Configuration.GetValue<int>("K",0)  Typed read with fallback default
 * builder.Logging.ClearProviders()            Remove SDK-default log sinks
 * builder.Logging.AddConsole()                Structured stdout logging
 * builder.Logging.SetMinimumLevel(level)      Global minimum log level
 * builder.Build()                             Seal DI container; return WebApplication
 *
 * app.Environment.EnvironmentName             "Development", "Staging", "Production"
 * app.Environment.IsDevelopment()             True when ASPNETCORE_ENVIRONMENT=Development
 * app.Environment.ContentRootPath             Physical path to the project root folder
 * app.Environment.WebRootPath                 Physical path to wwwroot/
 * app.Use(next => async ctx => { })           Add pass-through middleware
 * app.UseStaticFiles()                        Serve wwwroot/ without authentication
 * app.UseRouting()                            Enable endpoint routing
 * app.UseAuthorization()                      Enforce [Authorize] attribute
 * app.MapGet("/path", handler)                Register a minimal-API GET endpoint
 * app.MapControllers()                        Enable attribute-routed MVC controllers
 * app.Run()                                   Start Kestrel; block until shutdown
 *
 * ENVIRONMENT-VARIABLE OVERRIDES:
 *   ASPNETCORE_ENVIRONMENT=Production         Switch appsettings layer
 *   ASPNETCORE_URLS=http://+:5001            Override listening address
 *   AppInfo__Name=MyApp                      Override nested key (__ = colon separator)
 *
 * CONFIG FILE LAYERING (later sources override earlier ones):
 *   appsettings.json -> appsettings.{Env}.json -> env vars -> CLI args
 * ─────────────────────────────────────────────────────────────────────────────
 */
