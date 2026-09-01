/*
 * FILE ROLE: Demonstrates environment detection using IWebHostEnvironment and
 *            environment-specific configuration loading patterns.
 * SECTIONS IN THIS FILE:
 *   11. IWebHostEnvironment -- IsDevelopment / IsProduction / IsEnvironment
 *   12. Environment-specific configuration loading (appsettings.{env}.json pattern)
 *   13. Environment variables as secrets -- AddEnvironmentVariables with prefix
 */

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;

namespace HostingKestrel.Configuration;

/*
 * SECTION 11: IWebHostEnvironment -- ENVIRONMENT DETECTION
 *
 * IWebHostEnvironment (inherits IHostEnvironment) exposes:
 *   EnvironmentName    string  -- set by ASPNETCORE_ENVIRONMENT (e.g. "Development")
 *   ApplicationName    string  -- entry assembly name
 *   ContentRootPath    string  -- file-system path to the app's content root
 *   WebRootPath        string? -- path to wwwroot; null for API-only projects
 *   ContentRootFileProvider    -- IFileProvider for content root
 *   WebRootFileProvider        -- IFileProvider for wwwroot
 *
 * Extension methods (namespace Microsoft.Extensions.Hosting):
 *   env.IsDevelopment()           -> EnvironmentName == "Development" (case-insensitive)
 *   env.IsStaging()               -> EnvironmentName == "Staging"
 *   env.IsProduction()            -> EnvironmentName == "Production"
 *   env.IsEnvironment("Testing")  -> case-insensitive match to any custom name
 *
 * Where to access IWebHostEnvironment:
 *   In Program.cs        builder.Environment
 *   In controllers       inject IWebHostEnvironment via constructor DI
 *   In background svcs   inject IHostEnvironment (lighter; no WebRootPath needed)
 *   In middleware        inject IWebHostEnvironment into the middleware constructor
 *
 * --- 11a. IHostEnvironment vs IWebHostEnvironment ---
 *   IHostEnvironment     base type; ApplicationName, EnvironmentName, ContentRootPath.
 *   IWebHostEnvironment  extends with WebRootPath and WebRootFileProvider.
 *   Prefer IHostEnvironment in background services (avoids web dependency).
 *   Use IWebHostEnvironment in components that serve static files from wwwroot.
 *
 * Pitfall: EnvironmentName comparison is case-insensitive in the Is* helpers
 *          but the ASPNETCORE_ENVIRONMENT variable value is case-preserved in logs.
 *          Use "Development" (capitalised) by convention.
 */

/*
 * SECTION 12: ENVIRONMENT-SPECIFIC CONFIGURATION LOADING
 *
 * WebApplication.CreateBuilder already layers config providers in this order
 * (lowest -> highest priority):
 *   1. appsettings.json
 *   2. appsettings.{Environment}.json  (auto-merged by CreateDefaultBuilder)
 *   3. User secrets                    (Development only)
 *   4. Environment variables
 *   5. Command-line arguments
 *
 * The pattern below shows HOW to add providers explicitly -- matching what
 * CreateDefaultBuilder does internally. Apply this when:
 *   - Adding a third-party provider (Azure Key Vault, AWS Parameter Store).
 *   - Loading a second config file for a component-specific settings layer.
 *   - Supporting extra environments beyond the three built-in names.
 *
 * AddJsonFile parameters:
 *   path             relative path (resolved from ContentRootPath)
 *   optional         if true, silently skips missing files (good for env files)
 *   reloadOnChange   if true, IOptionsMonitor<T> subscribers see live updates
 *
 * Pitfall: Calling AddJsonFile for a file already wired by CreateDefaultBuilder
 *          causes that file to be read a second time (lower priority second copy).
 *          Use optional: true to avoid FileNotFoundException on absent files.
 */

/*
 * SECTION 13: ENVIRONMENT VARIABLES AS SECRETS
 *
 * Environment variables are the production-safe mechanism for secret injection.
 *
 * AddEnvironmentVariables(prefix: "HOSTEDAPP_"):
 *   - Only imports variables whose names start with "HOSTEDAPP_".
 *   - The prefix is stripped from the key before insertion into IConfiguration.
 *   - Double-underscore (__) maps to the section separator (:).
 *
 *   Variable: HOSTEDAPP_Database__Host=dbserver
 *   Config key: config["Database:Host"] == "dbserver"
 *
 * Environment-based secrets hierarchy:
 *   Development   use user secrets (never checked in to git):
 *                   dotnet user-secrets set "Db:Password" "dev123"
 *   CI/CD         inject via pipeline secret variables (GitHub Actions, Azure Pipelines).
 *   Production    inject via cloud secret manager env-var injection (Azure Key Vault
 *                 reference, AWS Secrets Manager env injection, K8s Secrets volume).
 *
 * Pitfall: Environment variables appear in process listings on Linux (ps aux).
 *          For very sensitive values (private keys, certificates), use mounted
 *          secret files or managed identity / workload identity instead.
 * Pitfall: Prefix matching is case-sensitive on Linux. Use a consistent ALL_CAPS
 *          prefix and document it in your deployment runbooks.
 */
public static class EnvironmentConfiguration
{
    /*
     * Configure is called from Program.Main before builder.Build().
     * Reads environment state and explicitly adds config providers for teaching.
     */
    public static void Configure(WebApplicationBuilder builder)
    {
        // Section 11: access IWebHostEnvironment from the builder
        IWebHostEnvironment env = builder.Environment;

        bool isDev    = env.IsDevelopment();          // true when env == "Development"
        bool isProd   = env.IsProduction();           // true when env == "Production"
        bool isCustom = env.IsEnvironment("Testing"); // arbitrary custom env name

        Console.WriteLine($"[EnvironmentConfig] EnvironmentName  = {env.EnvironmentName}");
        Console.WriteLine($"[EnvironmentConfig] IsDev={isDev}  IsProd={isProd}  IsCustom(Testing)={isCustom}");
        Console.WriteLine($"[EnvironmentConfig] ContentRoot       = {env.ContentRootPath}");

        // Section 12: add env-specific JSON layer (already done by CreateBuilder; shown for teaching)
        if (env.IsDevelopment())
        {
            // AddJsonFile is idempotent in priority order -- a duplicate adds a second, lower-priority layer
            builder.Configuration.AddJsonFile(
                "appsettings.Development.json", optional: true, reloadOnChange: true);
        }
        else if (env.IsProduction())
        {
            builder.Configuration.AddJsonFile(
                "appsettings.Production.json", optional: true, reloadOnChange: true);
        }

        // Section 13: env vars prefixed with HOSTEDAPP_ become config keys (prefix stripped)
        // Example: HOSTEDAPP_Database__Host=localhost  ->  config["Database:Host"]
        builder.Configuration.AddEnvironmentVariables(prefix: "HOSTEDAPP_");
    }
}
