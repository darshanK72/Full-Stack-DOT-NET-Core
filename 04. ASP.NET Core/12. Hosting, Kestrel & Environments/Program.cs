/*
 * TOPIC: Hosting, Kestrel & Environments
 *
 * WHY IT MATTERS:
 *   Every ASP.NET Core application runs inside a host — the object that owns the
 *   DI container, configuration, logging, and the web-server lifecycle. Understanding
 *   the host model lets you control startup, configure Kestrel's listening behavior
 *   and safety limits, switch configuration per environment, and react to application
 *   lifecycle events (start-up, graceful shutdown). These skills are foundational for
 *   deploying to Linux containers, Windows IIS, and cloud PaaS environments.
 *
 * WHAT YOU WILL LEARN:
 *    1. Hosting evolution: IWebHost (legacy) -> IHost (generic) -> WebApplication.
 *    2. What WebApplication.CreateBuilder sets up automatically.
 *    3. ASPNETCORE_ENVIRONMENT -- setting and reading the environment name.
 *    4. Configuration layering: appsettings.json, env-specific files, env vars, secrets.
 *    5. IIS in-process vs out-of-process hosting modes.
 *    6. Hosting startup assemblies -- extending the host from external libraries.
 *    7. Demo wiring -- Main() connects all tutorial files.
 *    8. KestrelServerOptions -- endpoint binding (IP / port / socket).
 *    9. HTTPS / TLS endpoint configuration.
 *   10. Kestrel limits: MaxRequestBodySize, MaxConcurrentConnections, KeepAliveTimeout.
 *   11. IWebHostEnvironment -- IsDevelopment / IsProduction / IsEnvironment.
 *   12. Environment-specific configuration loading.
 *   13. Environment variables as secrets -- AddEnvironmentVariables with prefix.
 *   14. IHostedService -- background service lifecycle (StartAsync / StopAsync).
 *   15. IHostApplicationLifetime -- ApplicationStarted / Stopping / Stopped.
 *   16. Graceful shutdown -- StopApplication() and shutdown timeout.
 *
 * CHAPTER MAP:
 *    1. Hosting evolution (IWebHost -> IHost -> WebApplication)  -> Program.cs  (below)
 *    2. WebApplication.CreateBuilder defaults                    -> Program.cs  (below)
 *    3. ASPNETCORE_ENVIRONMENT -- setting and reading            -> Program.cs  (below)
 *    4. Configuration loading order                              -> Program.cs  (below)
 *    5. IIS in-process vs out-of-process                         -> Program.cs  (below)
 *    6. Hosting startup assemblies                               -> Program.cs  (below)
 *    7. Demo wiring -- Main()                                    -> Program.cs  (below)
 *    8. KestrelServerOptions -- endpoint binding                 -> Configuration/KestrelConfiguration.cs
 *    9. HTTPS / TLS endpoint configuration                       -> Configuration/KestrelConfiguration.cs
 *   10. Kestrel limits                                           -> Configuration/KestrelConfiguration.cs
 *   11. IWebHostEnvironment detection methods                    -> Configuration/EnvironmentConfiguration.cs
 *   12. Environment-specific configuration loading               -> Configuration/EnvironmentConfiguration.cs
 *   13. Environment variables for secrets                        -> Configuration/EnvironmentConfiguration.cs
 *   14. IHostedService -- StartAsync / StopAsync                 -> Lifetime/ApplicationLifetimeService.cs
 *   15. ApplicationStarted / Stopping / Stopped events           -> Lifetime/ApplicationLifetimeService.cs
 *   16. Graceful shutdown / StopApplication()                    -> Lifetime/ApplicationLifetimeService.cs
 */

using HostingKestrel.Configuration;
using HostingKestrel.Lifetime;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace HostingKestrel;

/*
 * SECTION 1: HOSTING EVOLUTION -- IWebHost -> IHost -> WebApplication
 *
 * ASP.NET Core has had three hosting models since its launch:
 *
 * +---------------------+-------------------------------------+------------+----------+
 * | Era                 | Builder API                         | Host type  | .NET ver |
 * +---------------------+-------------------------------------+------------+----------+
 * | ASP.NET Core 1-2    | WebHost.CreateDefaultBuilder(args)  | IWebHost   | 1.x-2.x  |
 * |                     |   -> IWebHostBuilder                |            |          |
 * +---------------------+-------------------------------------+------------+----------+
 * | .NET Core 3 / .NET5 | Host.CreateDefaultBuilder(args)     | IHost      | 3.x-5    |
 * |                     |   -> IHostBuilder                   | (generic)  |          |
 * |                     |   .ConfigureWebHostDefaults(...)    |            |          |
 * +---------------------+-------------------------------------+------------+----------+
 * | .NET 6+ (current)   | WebApplication.CreateBuilder(args)  | IHost      | 6+       |
 * |                     |   -> WebApplicationBuilder          | (internal) |          |
 * +---------------------+-------------------------------------+------------+----------+
 *
 * IWebHost (legacy, ASP.NET Core 1-2):
 *   IWebHostBuilder builder = WebHost.CreateDefaultBuilder(args)
 *       .UseStartup<Startup>();
 *   IWebHost host = builder.Build();
 *   host.Run();
 *
 *   IWebHost was web-only -- no support for background services or non-web workloads.
 *   The Startup class required Configure() + ConfigureServices().
 *
 * IHost / Generic Host (.NET Core 3 - .NET 5):
 *   IHost host = Host.CreateDefaultBuilder(args)
 *       .ConfigureWebHostDefaults(web => web.UseStartup<Startup>())
 *       .Build();
 *   host.Run();
 *
 *   IHost unified web + background service hosting under one DI / config / logging model.
 *   IHostApplicationLifetime (ApplicationStarted, Stopping, Stopped) was introduced here.
 *
 * WebApplication (minimal API, .NET 6+):
 *   WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
 *   WebApplication app = builder.Build();
 *   app.MapGet("/", () => "Hello");
 *   app.Run();
 *
 *   Convenience wrapper -- builder.Host is IHostBuilder, builder.WebHost is IWebHostBuilder.
 *   Eliminates the Startup class; all configuration lives in Program.cs.
 *   All three patterns ultimately produce an IHost internally.
 */

/*
 * SECTION 2: WebApplication.CreateBuilder -- DEFAULT SETUP
 *
 * WebApplication.CreateBuilder(args) wires a comprehensive set of defaults.
 *
 * +------------------+----------------------------------------------------+
 * | Area             | What is wired automatically                        |
 * +------------------+----------------------------------------------------+
 * | Configuration    | appsettings.json                                   |
 * |                  | appsettings.{Environment}.json                     |
 * |                  | User secrets (Development only)                    |
 * |                  | Environment variables                               |
 * |                  | Command-line arguments                             |
 * +------------------+----------------------------------------------------+
 * | Logging          | Console, Debug, EventSource providers               |
 * +------------------+----------------------------------------------------+
 * | DI               | IServiceCollection ready for registrations          |
 * +------------------+----------------------------------------------------+
 * | Web server       | Kestrel + IIS integration layer                    |
 * +------------------+----------------------------------------------------+
 * | Environment      | ASPNETCORE_ENVIRONMENT detection                   |
 * +------------------+----------------------------------------------------+
 *
 * Key properties on WebApplicationBuilder:
 *   builder.Host        -> IHostBuilder  (generic host, e.g. AddHostedService)
 *   builder.WebHost     -> IWebHostBuilder (Kestrel / IIS, UseKestrel, ConfigureKestrel)
 *   builder.Services    -> IServiceCollection
 *   builder.Configuration -> IConfigurationManager (read + add providers)
 *   builder.Environment -> IWebHostEnvironment (EnvironmentName, ContentRootPath)
 *
 * Pitfall: builder.Build() finalises the DI container. Register all services BEFORE
 *          calling Build(); adding services after Build() throws InvalidOperationException.
 */

/*
 * SECTION 3: ASPNETCORE_ENVIRONMENT -- SETTING AND READING
 *
 * The environment name controls:
 *   - Which appsettings.{Environment}.json is merged over appsettings.json.
 *   - Whether the developer exception page / Swagger UI is shown.
 *   - Whether user secrets are loaded (Development only).
 *   - The value of IWebHostEnvironment.EnvironmentName.
 *
 * Ways to set the environment:
 *   +-------------------------------+------------------------------------------+
 *   | Method                        | Example                                  |
 *   +-------------------------------+------------------------------------------+
 *   | OS environment variable       | ASPNETCORE_ENVIRONMENT=Production         |
 *   | launchSettings.json           | "environmentVariables": { "ASPNETCORE_   |
 *   |                               |   ENVIRONMENT": "Staging" }              |
 *   | WebApplicationOptions         | new WebApplicationOptions {               |
 *   |                               |   EnvironmentName = "Testing" }          |
 *   | IWebHostBuilder               | webHost.UseEnvironment("Staging")         |
 *   +-------------------------------+------------------------------------------+
 *
 * Built-in names (case-insensitive string comparison):
 *   "Development"  -- verbose logging, dev exception page, user secrets.
 *   "Staging"      -- production-like with additional diagnostics.
 *   "Production"   -- minimal logging, no sensitive data exposed.
 *   Custom names   -- any string; detected via env.IsEnvironment("Testing").
 *
 * Extension methods on IWebHostEnvironment (namespace: Microsoft.Extensions.Hosting):
 *   env.IsDevelopment()           -> EnvironmentName == "Development"
 *   env.IsStaging()               -> EnvironmentName == "Staging"
 *   env.IsProduction()            -> EnvironmentName == "Production"
 *   env.IsEnvironment("Testing")  -> case-insensitive match to any name
 *
 * COVERED IN DETAIL: EnvironmentConfiguration.cs (SECTIONS 11-13).
 */

/*
 * SECTION 4: CONFIGURATION LOADING ORDER -- LAYERED PROVIDERS
 *
 * ASP.NET Core configuration is built from stacked providers. Later providers
 * OVERRIDE earlier ones for the same key.
 *
 * Default order set by CreateDefaultBuilder (lowest -> highest priority):
 *   1. appsettings.json
 *   2. appsettings.{Environment}.json    (merges / overrides base file)
 *   3. User secrets                      (Development only)
 *   4. Environment variables
 *   5. Command-line arguments            (highest)
 *
 * Adding providers explicitly:
 *   builder.Configuration.AddJsonFile("extra.json", optional: true);
 *   builder.Configuration.AddEnvironmentVariables(prefix: "MYAPP_");
 *   builder.Configuration.AddCommandLine(args);
 *
 * Reading values:
 *   string? val  = builder.Configuration["Section:Key"];
 *   string? conn = builder.Configuration.GetConnectionString("Default");
 *   MyOptions o  = builder.Configuration.GetSection("MySection").Get<MyOptions>()!;
 *
 * Environment variables for secrets:
 *   Double-underscore (__) maps to section separator (:).
 *   ASPNETCORE_ConnectionStrings__Default=Server=...
 *   -> config["ConnectionStrings:Default"]
 *
 *   In production: inject via platform secret manager (Azure Key Vault,
 *   AWS Secrets Manager, Kubernetes secrets) -- never commit secrets to git.
 *
 * COVERED IN DETAIL: EnvironmentConfiguration.cs (SECTION 13).
 */

/*
 * SECTION 5: IIS IN-PROCESS vs OUT-OF-PROCESS HOSTING
 *
 * When deploying to IIS (Windows Server / Azure App Service), the ASP.NET Core
 * Module (ANCM) manages the process lifecycle.
 *
 * +-------------------+------------------------------+---------------------------+
 * | Aspect            | In-Process                   | Out-of-Process            |
 * +-------------------+------------------------------+---------------------------+
 * | Where app runs    | Inside IIS worker (w3wp.exe) | Separate Kestrel process  |
 * | HTTP handling     | IIS handles HTTP directly    | IIS reverse-proxies to    |
 * |                   | (no Kestrel socket)          | Kestrel                   |
 * | Performance       | Better -- fewer hops          | Extra hop per request     |
 * | Isolation         | Crash affects IIS            | Crash isolated from IIS   |
 * | web.config        | hostingModel="inprocess"     | hostingModel="outofprocess"|
 * | .csproj           | <AspNetCoreHostingModel>     | <AspNetCoreHostingModel>  |
 * |                   |   InProcess</...>            |   OutOfProcess</...>      |
 * +-------------------+------------------------------+---------------------------+
 *
 * In-Process is the DEFAULT since ASP.NET Core 3 and is preferred for performance.
 *
 * .csproj snippet (in-process):
 *   <PropertyGroup>
 *     <AspNetCoreHostingModel>InProcess</AspNetCoreHostingModel>
 *   </PropertyGroup>
 *
 * Pitfall: UseIIS() and UseIISIntegration() are called automatically by
 *          CreateDefaultBuilder -- do not call them manually unless overriding.
 * Pitfall: In in-process mode, Kestrel is NOT used; Kestrel-specific configuration
 *          (endpoints, limits) has no effect when running inside IIS.
 */

/*
 * SECTION 6: HOSTING STARTUP ASSEMBLIES
 *
 * IHostingStartup lets an external assembly inject services and middleware into
 * an ASP.NET Core app without modifying the application's Program.cs.
 *
 * How it works:
 *   1. An external assembly applies [assembly: HostingStartup(typeof(MyStartup))].
 *   2. MyStartup implements IHostingStartup.Configure(IWebHostBuilder).
 *   3. ASPNETCORE_HOSTINGSTARTUPASSEMBLIES env var lists assemblies to scan
 *      (semicolon-separated: "DiagnosticsLib;MonitoringLib").
 *   4. The host scans and runs each IHostingStartup during Build().
 *
 * Example (external assembly):
 *   [assembly: HostingStartup(typeof(DiagnosticsStartup))]
 *   namespace DiagnosticsLib;
 *   public class DiagnosticsStartup : IHostingStartup
 *   {
 *       public void Configure(IWebHostBuilder builder) =>
 *           builder.ConfigureServices(s =>
 *               s.AddSingleton<IDiagnostics, ConsoleDiagnostics>());
 *   }
 *
 * Preventing external startups from loading:
 *   Set ASPNETCORE_PREVENTHOSTINGSTARTUP=true   -- blocks ALL IHostingStartup.
 *   Or list specific assemblies in WebHostDefaults.HostingStartupExcludeAssembliesKey.
 *
 * Common use cases: OpenTelemetry sinks, health-check registration, shared auth middleware.
 * Pitfall: Hosting startups run BEFORE the app's code; they influence the DI container
 *          and pipeline before Program.cs gets a chance to customise them.
 */

public class Program
{
    /*
     * SECTION 7: DEMO WIRING -- Main() connects all sections in this tutorial.
     *
     * Reading order:
     *   1. Program.cs sections 1-6 above -- conceptual host model deep-dive.
     *   2. Configuration/KestrelConfiguration.cs -- sections 8-10 (Kestrel).
     *   3. Configuration/EnvironmentConfiguration.cs -- sections 11-13 (environment).
     *   4. Lifetime/ApplicationLifetimeService.cs -- sections 14-16 (lifetime events).
     *   5. appsettings*.json -- layered configuration files for each environment.
     */
    public static void Main(string[] args)
    {
        // Section 2: create builder -- wires config, logging, Kestrel, DI, environment
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Sections 8-10: configure Kestrel endpoints and limits
        KestrelConfiguration.Configure(builder.WebHost);

        // Sections 11-13: log environment state and add env-specific providers
        EnvironmentConfiguration.Configure(builder);

        // Sections 14-16: hosted service that subscribes to IHostApplicationLifetime
        builder.Services.AddHostedService<ApplicationLifetimeService>();

        // Section 2 pitfall: Build() finalises the DI container -- all registrations done
        WebApplication app = builder.Build();

        // Minimal endpoint that reports environment for demo verification
        app.MapGet("/", () =>
            $"Hosting demo | env={app.Environment.EnvironmentName} | " +
            $"root={app.Environment.ContentRootPath}");

        Console.WriteLine(
            $"[Demo] Starting in '{app.Environment.EnvironmentName}' environment. " +
            "Press Ctrl+C to trigger graceful shutdown (sections 15-16).");

        app.Run(); // Section 1: IHost.Run() -- blocks until shutdown signal
    }
}

/*
 * QUICK REFERENCE -- Hosting, Kestrel & Environments
 *
 * HOST BUILDERS
 *   WebApplication.CreateBuilder(args)      modern; WebApplicationBuilder
 *   Host.CreateDefaultBuilder(args)         generic host; needs ConfigureWebHostDefaults
 *   WebHost.CreateDefaultBuilder(args)      legacy (ASP.NET Core 1-2); avoid in new code
 *
 * BUILDER PROPERTIES (WebApplicationBuilder)
 *   builder.Host        IHostBuilder        background services, generic host options
 *   builder.WebHost     IWebHostBuilder     Kestrel, IIS, URLs
 *   builder.Services    IServiceCollection  DI registrations
 *   builder.Configuration IConfigurationManager  add/read config providers
 *   builder.Environment IWebHostEnvironment EnvironmentName, ContentRootPath, WebRootPath
 *
 * ENVIRONMENT
 *   ASPNETCORE_ENVIRONMENT=Development|Staging|Production|<custom>
 *   env.IsDevelopment()  env.IsStaging()  env.IsProduction()  env.IsEnvironment("X")
 *
 * KESTREL ENDPOINT BINDING
 *   options.ListenLocalhost(5000)            127.0.0.1:5000 HTTP
 *   options.ListenAnyIP(5001)                0.0.0.0:5001  HTTP
 *   options.Listen(IPAddress.Loopback, 5002) explicit IP
 *   listenOptions.UseHttps()                 HTTPS (dev cert or configured cert)
 *
 * KESTREL LIMITS (KestrelServerOptions.Limits)
 *   MaxRequestBodySize            long?    30 MB default; null = unlimited
 *   MaxConcurrentConnections      long?    null default (unlimited)
 *   MaxConcurrentUpgradedConnections long? null default
 *   KeepAliveTimeout              TimeSpan 130 s default
 *   RequestHeadersTimeout         TimeSpan 30 s default
 *
 * CONFIGURATION PROVIDERS (priority: last added wins)
 *   builder.Configuration.AddJsonFile("f.json", optional: true, reloadOnChange: true)
 *   builder.Configuration.AddEnvironmentVariables(prefix: "MYAPP_")
 *   builder.Configuration.AddCommandLine(args)
 *
 * IHostApplicationLifetime
 *   lifetime.ApplicationStarted   CancellationToken -- fires when host is fully up
 *   lifetime.ApplicationStopping  CancellationToken -- fires when shutdown begins
 *   lifetime.ApplicationStopped   CancellationToken -- fires after all services stopped
 *   lifetime.StopApplication()    -- programmatic graceful shutdown trigger
 *
 * GRACEFUL SHUTDOWN
 *   SIGTERM / CTRL+C              triggers automatic graceful stop
 *   builder.Services.Configure<HostOptions>(o => o.ShutdownTimeout = TimeSpan.FromSeconds(30))
 *   Default shutdown timeout: 5 seconds (increase for slow cleanup)
 *
 * IIS HOSTING MODELS (.csproj)
 *   <AspNetCoreHostingModel>InProcess</AspNetCoreHostingModel>      default, faster
 *   <AspNetCoreHostingModel>OutOfProcess</AspNetCoreHostingModel>   isolated, Kestrel used
 *
 * HOSTING STARTUP ASSEMBLIES
 *   ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=Assembly1;Assembly2
 *   ASPNETCORE_PREVENTHOSTINGSTARTUP=true   blocks all IHostingStartup
 */
