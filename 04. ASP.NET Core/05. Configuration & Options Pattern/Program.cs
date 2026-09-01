/*
 * TOPIC: Configuration & Options Pattern
 *
 * WHY IT MATTERS:
 *   Every real application needs settings that differ between environments
 *   (development, staging, production) and can change without recompiling.
 *   ASP.NET Core's layered configuration system and the Options pattern give you:
 *     - A unified API over many sources: JSON files, environment variables, CLI args, Vault…
 *     - Strongly-typed settings classes with compile-time property names
 *     - Automatic validation at startup — fail fast on missing connection strings
 *     - Hot reload — some settings update while the app runs, without a restart
 *
 * WHAT YOU WILL LEARN:
 *    1. Configuration providers and priority order (appsettings → env vars → CLI)
 *    2. IConfiguration — GetValue<T>, GetSection, GetChildren, Bind, Get<T>
 *    3. Configuration key hierarchy and the colon (:) separator
 *    4. Strongly typed options — mapping JSON sections to C# classes
 *    5. Options validation — DataAnnotations + ValidateOnStart
 *    6. IOptions<T> vs IOptionsSnapshot<T> vs IOptionsMonitor<T>
 *    7. Named options — multiple configurations of the same type
 *    8. IConfigureOptions<T> — class-based configuration (vs lambda)
 *    9. PostConfigure — run last, override any earlier Configure call
 *   10. User secrets — keep credentials out of source control in development
 *   PREVIEW: Logging configuration (covered fully in 06. Logging & Diagnostics)
 *
 * CHAPTER MAP (open files in this order):
 *    1. appsettings.json & appsettings.Development.json  — config file structure + env override
 *    2. Strongly typed root config                       → Models/AppSettings.cs
 *    3. Nested options + DataAnnotations validation      → Models/DatabaseOptions.cs
 *    4. Feature flag options + named options setup       → Models/FeatureFlags.cs
 *    5. IOptions / IOptionsSnapshot / IOptionsMonitor    → Services/ConfigurationDemoService.cs
 *    6. Named options retrieval (.Get)                   → Services/ConfigurationDemoService.cs
 *    7. IConfigureOptions<T> class                       → Program.cs  (AppSettingsConfigurer below)
 *    8. Provider registration & IConfiguration API       → Program.cs  Main (SECTION 1 + 2)
 *    9. Options registration + validation                → Program.cs  Main (SECTIONS 3–6)
 *   10. User secrets                                     → Program.cs  Main (SECTION 7)
 */

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ConfigurationOptions.Models;
using ConfigurationOptions.Services;

namespace ConfigurationOptions;

/*
 * SECTION 7: IConfigureOptions<T> — CLASS-BASED OPTIONS CONFIGURATION
 *
 * services.Configure<T>(section) uses a lambda internally.  When the
 * configuration logic is complex, or needs other injected services, implement
 * IConfigureOptions<T> in a dedicated class instead.
 *
 * INTERFACES:
 *   IConfigureOptions<T>        — Configure(T options) — runs for the DEFAULT ("") name
 *   IConfigureNamedOptions<T>   — Configure(string? name, T options) — runs for every name
 *   IPostConfigureOptions<T>    — PostConfigure(string? name, T options) — always runs last
 *
 * EXECUTION ORDER:
 *   1. All IConfigureOptions<T> / IConfigureNamedOptions<T> registrations — in order
 *   2. All IPostConfigureOptions<T> registrations — in order, after all Configure calls
 *
 *   PostConfigure always wins, regardless of registration order relative to Configure.
 *
 * REGISTRATION:
 *   services.AddSingleton<IConfigureOptions<T>, MyConfigurer>();
 *   // Fluent equivalent (lambda form):
 *   services.AddOptions<T>().Configure<IConfiguration>((opts, cfg) => cfg.Bind(opts));
 *
 * WHEN TO USE:
 *   - Logic too complex for a one-line lambda
 *   - Need to inject other services (IHostEnvironment, ILogger, etc.)
 *   - Want the configuration logic to be unit-testable in isolation
 *   - Applying a shared configurer across multiple services
 */
internal sealed class AppSettingsConfigurer : IConfigureOptions<AppSettings>
{
    private readonly IConfiguration _configuration; // injected from DI — full provider stack

    public AppSettingsConfigurer(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /* Called by the Options framework when AppSettings is first resolved from DI. */
    public void Configure(AppSettings options)
    {
        // GetValue<T> reads a flat key; returns default(T) if absent (not an exception)
        options.AppName = _configuration.GetValue<string>("AppName") ?? "Unknown Application";
        options.Version = _configuration.GetValue<string>("Version") ?? "0.0.0";
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        IConfiguration config = builder.Configuration; // the full layered config

        /*
         * SECTION 1: CONFIGURATION PROVIDERS AND PRIORITY ORDER
         *
         * WebApplication.CreateBuilder registers these providers automatically,
         * in order from lowest to HIGHEST priority (later provider wins on conflict):
         *
         *   Priority  Provider                              Source
         *   ────────  ─────────────────────────────────────────────────────────────
         *     1 (low)  appsettings.json                    project root JSON file
         *     2        appsettings.{Environment}.json       environment-specific override
         *     3        User secrets                        %APPDATA%\...\secrets.json  (Dev only)
         *     4        Environment variables                OS / Docker / Kubernetes
         *     5 (high) Command-line arguments              --Key Value  or  Key=Value
         *   ────────  ─────────────────────────────────────────────────────────────
         *
         * PRIORITY RULE:
         *   If "AppName" appears in both appsettings.json AND an environment variable
         *   (APPNAME=Prod), the environment variable wins because it has higher priority.
         *
         * KEY HIERARCHY — THREE SEPARATOR STYLES:
         *   JSON nested key:   { "Database": { "Port": 5432 } }
         *   C# (GetValue):     "Database:Port"              (colon)
         *   Environment var:   Database__Port=5432           (double underscore)
         *   CLI argument:      --Database:Port 5432
         *
         * ADDING EXTRA PROVIDERS (after the default stack):
         *   builder.Configuration.AddJsonFile("extra.json", optional: true, reloadOnChange: true);
         *   builder.Configuration.AddIniFile("config.ini",  optional: true);
         *   builder.Configuration.AddXmlFile("config.xml",  optional: true);
         *   builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?> {
         *       ["Key"] = "Value"
         *   });
         *
         * ENVIRONMENT DETECTION:
         *   builder.Environment.EnvironmentName  — e.g. "Development", "Staging", "Production"
         *   builder.Environment.IsDevelopment()  — true when EnvironmentName == "Development"
         *   Set via: ASPNETCORE_ENVIRONMENT environment variable  (or launchSettings.json in dev)
         */

        /*
         * SECTION 2: IConfiguration — GetValue<T>, GetSection, GetChildren, Bind
         *
         * Demonstrated in PrintConfigDetails below.  The method receives
         * builder.Configuration which already holds all provider values.
         */
        PrintConfigDetails(config);

        /*
         * SECTION 3: AppSettings — IConfigureOptions<T> CLASS REGISTRATION
         *
         * AppSettingsConfigurer (defined above) reads AppName and Version from
         * IConfiguration and writes them into AppSettings.
         * AddOptions<AppSettings>() registers the Options infrastructure for the type;
         * AddSingleton<IConfigureOptions<AppSettings>, ...> hooks in the configurer.
         */
        builder.Services.AddOptions<AppSettings>(); // set up OptionsFactory<AppSettings>
        builder.Services.AddSingleton<IConfigureOptions<AppSettings>, AppSettingsConfigurer>();

        /*
         * SECTION 4: DatabaseOptions — FLUENT REGISTRATION WITH VALIDATION
         *
         * AddOptions<T>() returns OptionsBuilder<T> for chaining:
         *   .BindConfiguration(key)     — reads the named section at runtime
         *   .ValidateDataAnnotations()  — enforces [Required], [Range], etc. on properties
         *   .ValidateOnStart()          — throws OptionsValidationException at app startup
         *                                 rather than waiting for the first options access
         *
         * TIP: Always use ValidateOnStart() for critical settings (DB connections, API keys).
         *   A startup crash is far easier to diagnose than a runtime failure under load.
         */
        builder.Services
            .AddOptions<DatabaseOptions>()
            .BindConfiguration(DatabaseOptions.SectionName) // reads "Database" section
            .ValidateDataAnnotations()                       // attribute-based rules
            .ValidateOnStart();                              // fail fast on bad config

        /*
         * SECTION 5: FeatureFlags — Configure<T> WITH NAMED OPTIONS
         *
         * services.Configure<T>(section)         — registers the DEFAULT ("") configuration
         * services.Configure<T>(name, section)   — registers a NAMED configuration
         *
         * Any number of named registrations can coexist for the same type.
         * Retrieved via IOptionsMonitor<T>.Get(name) — see ConfigurationDemoService.
         *
         * Options.DefaultName == string.Empty ("")
         *   monitor.CurrentValue   is equivalent to   monitor.Get(Options.DefaultName)
         */
        builder.Services.Configure<FeatureFlags>(
            config.GetSection(FeatureFlags.SectionName));          // default ("") config

        builder.Services.Configure<FeatureFlags>(
            "Admin", config.GetSection("AdminFeatureFlags"));       // named "Admin"

        builder.Services.Configure<FeatureFlags>(
            "Api",   config.GetSection("ApiFeatureFlags"));         // named "Api"

        /*
         * SECTION 6: PostConfigure — ALWAYS RUNS LAST
         *
         * PostConfigure<T> registers a delegate that runs AFTER all IConfigureOptions<T>
         * and services.Configure<T>() calls for that type, regardless of registration order.
         *
         * Use cases:
         *   - Sanitise values: trim whitespace, normalise casing after all providers run
         *   - Apply guaranteed defaults when any Configure path may leave a property unset
         *   - Audit/log the final resolved options at startup
         *
         * Variants:
         *   services.PostConfigure<T>(opts => { })             — applies to default name only
         *   services.PostConfigure<T>(name, opts => { })        — applies to one named config
         *   services.PostConfigureAll<T>(opts => { })           — applies to ALL names (+ default)
         */
        builder.Services.PostConfigure<AppSettings>(opts =>
        {
            // Guarantee AppName is never empty — runs after AppSettingsConfigurer.Configure()
            if (string.IsNullOrWhiteSpace(opts.AppName))
                opts.AppName = "Unnamed Application";
        });

        /*
         * SECTION 7: USER SECRETS — KEEP CREDENTIALS OUT OF SOURCE CONTROL
         *
         * User Secrets store sensitive key-value pairs (API keys, passwords) in a JSON
         * file located OUTSIDE the project directory, so they are never committed to git.
         *
         *   Windows: %APPDATA%\Microsoft\UserSecrets\{UserSecretsId}\secrets.json
         *   Linux:   ~/.microsoft/usersecrets/{UserSecretsId}/secrets.json
         *
         * WebApplication.CreateBuilder already adds user secrets automatically when:
         *   1. The app runs in the "Development" environment
         *   2. The .csproj contains <UserSecretsId>...</UserSecretsId>
         *
         * CLI WORKFLOW:
         *   dotnet user-secrets init                           — adds <UserSecretsId> to .csproj
         *   dotnet user-secrets set "Database:Password" "s3cr3t" — stores a secret
         *   dotnet user-secrets list                           — lists all secrets
         *   dotnet user-secrets remove "Database:Password"     — removes one secret
         *   dotnet user-secrets clear                          — removes all secrets
         *
         * EXPLICIT REGISTRATION (usually unnecessary — shown for clarity):
         *   builder.Configuration.AddUserSecrets<Program>();
         *
         * PRIORITY:
         *   User secrets sit between appsettings.Development.json (lower) and
         *   environment variables (higher).  CI/CD env vars correctly override local secrets.
         *
         * PRODUCTION WARNING:
         *   User secrets are developer-only.  In production use:
         *   environment variables, Azure Key Vault, AWS Secrets Manager, or Vault.
         */
        if (builder.Environment.IsDevelopment())
        {
            // User secrets are already part of the default provider stack in Development.
            // Calling AddUserSecrets<T>() explicitly here is for demonstration only.
            // builder.Configuration.AddUserSecrets<Program>(); // adds provider a second time (harmless)
            Console.WriteLine("[Startup] Development environment — user secrets provider is active.");
        }

        // Register the demo service as scoped (required for IOptionsSnapshot injection)
        builder.Services.AddScoped<ConfigurationDemoService>();

        WebApplication app = builder.Build();

        /*
         * PREVIEW: LOGGING CONFIGURATION
         *   (Covered in full → 06. Logging & Diagnostics)
         *
         * The "Logging" section in appsettings.json controls which log levels are
         * emitted for each category (namespace) of code:
         *
         *   "Logging": {
         *     "LogLevel": {
         *       "Default":            "Information",  // all unnamed categories
         *       "Microsoft.AspNetCore": "Warning"     // suppress ASP.NET Core noise
         *     }
         *   }
         *
         * LOG LEVELS (lowest → highest severity):
         *   Trace → Debug → Information → Warning → Error → Critical → None
         *
         * RULE: setting a level means "log that level AND ABOVE".
         *   "Information" → logs Information, Warning, Error, Critical (not Trace/Debug).
         *   "None"        → silences all logging for that category.
         *
         * appsettings.Development.json overrides Default to "Debug" for verbose local output
         * without affecting production settings.  This is the standard pattern.
         *
         * COVERED IN FULL → 06. Logging & Diagnostics
         *   (ILogger<T>, providers, structured logging, Serilog, log scopes, performance)
         */

        // ── Minimal API endpoints (demo wiring) ─────────────────────────────

        app.MapGet("/", () => Results.Ok(
            "Configuration & Options demo. " +
            "Endpoints: /config  /options  /database  /features  /features/{name}"));

        // Demonstrates direct IConfiguration access — flat keys and nested keys
        app.MapGet("/config", (IConfiguration cfg) =>
        {
            string? appName = cfg.GetValue<string>("AppName");          // flat key
            string? version = cfg.GetValue<string>("Version");          // flat key
            int     timeout = cfg.GetValue<int>("Database:CommandTimeoutSeconds"); // nested key
            return Results.Ok(new { AppName = appName, Version = version, DbTimeout = timeout });
        });

        // Uses IOptions<AppSettings> via ConfigurationDemoService
        app.MapGet("/options", (ConfigurationDemoService svc) =>
            Results.Ok(svc.GetAppInfo()));

        // Uses IOptionsSnapshot<DatabaseOptions> — per-scope re-read
        app.MapGet("/database", (ConfigurationDemoService svc) =>
            Results.Ok(svc.GetDatabaseInfo()));

        // Uses IOptionsMonitor<FeatureFlags>.CurrentValue — live default
        app.MapGet("/features", (ConfigurationDemoService svc) =>
            Results.Ok(svc.GetFeatureInfo()));

        // Uses IOptionsMonitor<FeatureFlags>.Get(name) — named options
        app.MapGet("/features/{name}", (string name, ConfigurationDemoService svc) =>
            Results.Ok(svc.GetNamedFeatureInfo(name)));

        app.Run();
    }

    /*
     * SECTION 2 (IMPLEMENTATION): IConfiguration API
     *   GetValue<T>   — reads a single typed key (flat or colon-separated)
     *   GetSection    — returns a subtree as IConfigurationSection
     *   GetChildren   — enumerates immediate children of a section
     *   Bind          — populates an existing object from a section
     *   Get<T>        — creates and returns a new T from a section (nullable)
     *
     * NEVER THROWS on missing keys — missing keys return default(T) or null.
     * Always check section.Exists() before using a section that may be absent.
     */
    private static void PrintConfigDetails(IConfiguration config)
    {
        // GetValue<T> — typed read of a single key; null/default if key is absent
        string? appName = config.GetValue<string>("AppName");
        int     timeout = config.GetValue<int>("Database:CommandTimeoutSeconds", 30); // with fallback
        Console.WriteLine($"[GetValue] AppName: {appName ?? "(null)"}");
        Console.WriteLine($"[GetValue] DB CommandTimeoutSeconds: {timeout}");

        // GetSection — returns the "Database" subtree; never throws even if absent
        IConfigurationSection dbSection = config.GetSection("Database");

        if (dbSection.Exists()) // always check Exists() before relying on section
        {
            // Child keys accessed relative to the section root (no "Database:" prefix needed)
            string? connStr = dbSection.GetValue<string>("ConnectionString");
            string display  = connStr is null ? "(null)" : connStr[..Math.Min(30, connStr.Length)];
            Console.WriteLine($"[GetSection] ConnectionString: {display}...");

            // GetChildren — enumerates immediate child IConfigurationSection objects
            Console.WriteLine("[GetChildren] Keys inside 'Database':");
            foreach (IConfigurationSection child in dbSection.GetChildren())
            {
                // child.Key = key name (e.g. "ConnectionString"), child.Value = leaf value
                Console.WriteLine($"  {child.Key} = {child.Value ?? "(object)"}");
            }
        }

        // Bind — populates an existing instance from a section; properties keep defaults for absent keys
        DatabaseOptions boundDb = new DatabaseOptions();
        config.GetSection(DatabaseOptions.SectionName).Bind(boundDb); // writes into boundDb
        Console.WriteLine($"[Bind] MaxRetryCount: {boundDb.MaxRetryCount}");

        // Get<T> — creates a new instance and returns it; returns null if the section is empty
        AppSettings? appSettings = config.Get<AppSettings>(); // binds root keys to AppSettings
        Console.WriteLine($"[Get<T>] AppSettings.AppName: {appSettings?.AppName ?? "(null)"}");
    }
}

/*
 * QUICK REFERENCE — Configuration & Options Pattern
 * ═══════════════════════════════════════════════════════════════════════
 *
 * PROVIDER PRIORITY (highest wins):
 *   CLI args  >  Environment variables  >  User secrets (Dev)
 *   >  appsettings.{Env}.json  >  appsettings.json
 *
 * KEY SEPARATORS:
 *   JSON:      { "Database": { "Port": 5432 } }
 *   C# key:    "Database:Port"                  (colon)
 *   Env var:   Database__Port=5432              (double underscore)
 *   CLI:       --Database:Port 5432
 *
 * IConfiguration API:
 *   config["key"]                               string indexer; null if missing
 *   config.GetValue<T>("key")                   typed; default(T) if missing
 *   config.GetValue<T>("key", fallback)          typed with explicit fallback
 *   config.GetSection("key")                    subtree; always returns section
 *   config.GetSection("key").Exists()           false if section has no keys
 *   config.GetSection("key").GetChildren()      immediate child sections
 *   config.GetSection("key").Bind(obj)          populate existing object
 *   config.GetSection("key").Get<T>()           create & return T; null if empty
 *
 * OPTIONS REGISTRATION:
 *   services.Configure<T>(section)                          unnamed ("") config
 *   services.Configure<T>(name, section)                    named config
 *   services.AddOptions<T>()                                OptionsBuilder<T> fluent
 *     .BindConfiguration("Key")                             bind to section at runtime
 *     .ValidateDataAnnotations()                            enforce [Required] etc.
 *     .ValidateOnStart()                                    throw at startup if invalid
 *   services.PostConfigure<T>(opts => { })                  run last (default name)
 *   services.PostConfigureAll<T>(opts => { })               run last (all names)
 *   services.AddSingleton<IConfigureOptions<T>, Impl>()     class-based configurer
 *
 * THREE OPTIONS INTERFACES:
 *   IOptions<T>              .Value           Singleton  Fixed after startup
 *   IOptionsSnapshot<T>      .Value           Scoped     Re-read per request
 *   IOptionsSnapshot<T>      .Get(name)       Scoped     Named, per request
 *   IOptionsMonitor<T>       .CurrentValue    Singleton  Live updates
 *   IOptionsMonitor<T>       .Get(name)       Singleton  Live + named
 *   IOptionsMonitor<T>       .OnChange(cb)    —          Change callback; returns IDisposable
 *
 * USER SECRETS:
 *   dotnet user-secrets init
 *   dotnet user-secrets set "Key:Sub" "Value"
 *   dotnet user-secrets list
 *   Storage: %APPDATA%\Microsoft\UserSecrets\{id}\secrets.json  (Windows)
 *   Auto-added in Development by WebApplication.CreateBuilder.
 *   Never use in production — use env vars or a secrets vault instead.
 *
 * APPSETTINGS PARTIAL OVERRIDE RULE:
 *   Only keys present in appsettings.Development.json are overridden.
 *   Absent keys fall back to appsettings.json values.
 *   A missing object section is NOT merged — the entire section is replaced.
 */
