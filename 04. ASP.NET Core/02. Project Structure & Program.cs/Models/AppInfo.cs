/*
 * FILE ROLE: Demonstrates a typed configuration section bound from appsettings.json.
 *            Open this file first (Chapter Map step 1) — it explains how
 *            builder.Configuration.GetSection("AppInfo").Get<AppInfo>() works
 *            and why records are a good fit for immutable configuration data.
 *
 * SECTIONS IN THIS FILE:
 *   1. AppInfo record — typed configuration section with binding rules
 */

namespace ProjectStructure.Models;

/*
 * SECTION 1: TYPED CONFIGURATION SECTIONS
 *
 * ASP.NET Core's configuration system can bind any JSON object to a C# type.
 * Define a record or POCO whose property names match the JSON keys (case-insensitive).
 * Then call:
 *
 *   AppInfo? info = builder.Configuration.GetSection("AppInfo").Get<AppInfo>();
 *
 * This matches the JSON in appsettings.json:
 *   "AppInfo": {
 *     "Name": "ProjectStructureTutorial",
 *     "Version": "1.0.0",
 *     "Environment": "Production"
 *   }
 *
 * BINDING RULES:
 *   GetSection("Key")          Returns IConfigurationSection — never null, even if absent.
 *   .Get<T>()                  Binds section to T; returns null if section has no children.
 *   .Bind(existingInstance)    Populates an existing object in place (no return value).
 *   Nested sections            Map to nested classes/records.
 *   Arrays ("Items":["a","b"]) Map to IEnumerable<string>, List<string>, string[], etc.
 *
 * LAYERED OVERRIDES:
 *   appsettings.json sets "Environment": "Production".
 *   appsettings.Development.json overrides only "Environment": "Development".
 *   After layering, when ASPNETCORE_ENVIRONMENT=Development the bound record will have
 *   Name="ProjectStructureTutorial", Version="1.0.0", Environment="Development".
 *
 * IPTIONS<T> PATTERN (PREVIEW -> 05. Configuration & Options Pattern):
 *   For services that need configuration injected, register with DI instead:
 *     builder.Services.Configure<AppInfo>(builder.Configuration.GetSection("AppInfo"));
 *   Then inject IOptions<AppInfo> (snapshot at startup), IOptionsSnapshot<AppInfo>
 *   (per-request reload), or IOptionsMonitor<AppInfo> (hot-reload via callback).
 *   .Get<T>() here is a quick builder-phase read — appropriate for startup diagnostics
 *   and simple checks, not for long-lived services.
 *
 * RECORD vs. CLASS:
 *   A record is chosen here because configuration data is immutable — values are set
 *   once at startup and never mutated. Records also provide structural equality
 *   (two AppInfo with identical values are ==), which simplifies unit testing.
 *
 * COMPILE NOTE — CS8618 (non-nullable property not initialized):
 *   Properties are given default values (= string.Empty) so the compiler does not
 *   emit CS8618 under <Nullable>enable</Nullable>. The config binder overwrites these
 *   defaults at runtime if the key is present in any configuration source.
 */
public record AppInfo
{
    public string Name { get; init; } = string.Empty;         // maps to "AppInfo.Name" in JSON
    public string Version { get; init; } = string.Empty;     // maps to "AppInfo.Version" in JSON
    public string Environment { get; init; } = string.Empty; // overridden in appsettings.Development.json
}
