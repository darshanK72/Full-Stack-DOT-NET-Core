/*
 * FILE ROLE: Root-level strongly typed configuration class that maps to the
 *   top-level keys in appsettings.json (AppName, Version).
 * SECTIONS IN THIS FILE:
 *   1. Strongly typed root configuration — binding patterns and property defaults
 */

namespace ConfigurationOptions.Models;

/*
 * SECTION 1: STRONGLY TYPED CONFIGURATION — BIND TO A C# CLASS
 *
 * Raw IConfiguration reads (GetValue<T>, config["key"]) are fine for one-off
 * accesses, but scatter magic strings across the codebase.  Binding a JSON
 * section to a C# class gives you compile-time safety, IntelliSense, and a
 * single place to update key names.
 *
 * FOUR BINDING APPROACHES:
 *   A) config.GetSection("key").Bind(instance)     — populates an existing object
 *   B) config.GetSection("key").Get<T>()           — creates and returns new T (nullable)
 *   C) services.Configure<T>(config.GetSection("key"))  — registers via Options (recommended)
 *   D) services.AddOptions<T>().BindConfiguration("key") — fluent; chain with validation
 *
 * HOW BINDING WORKS:
 *   - Property names are matched case-insensitively to JSON keys
 *   - Missing JSON keys leave the property at its C# default (no exception)
 *   - Nested JSON objects map to nested classes or sub-sections
 *   - Arrays map to IEnumerable<T> or T[] properties
 *
 * NULLABLE DEFAULTS:
 *   Always initialise string properties to string.Empty (or a sensible default)
 *   so nullable analysis does not warn about uninitialized non-nullable refs.
 *   If a key is absent from config the property keeps this default silently.
 *
 * appsettings.json structure for this class:
 *   {
 *     "AppName": "Configuration Demo",
 *     "Version": "1.0.0"
 *   }
 *
 * Registration in Program.cs:
 *   services.AddSingleton<IConfigureOptions<AppSettings>, AppSettingsConfigurer>();
 *   // OR:
 *   services.Configure<AppSettings>(config);   // binds all root-level keys
 */
public sealed class AppSettings
{
    public string AppName { get; set; } = string.Empty; // maps to "AppName" JSON key
    public string Version { get; set; } = string.Empty; // maps to "Version" JSON key
}
