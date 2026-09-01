/*
 * FILE ROLE: Nested strongly-typed options class for the "Database" configuration
 *   section, demonstrating DataAnnotations-based options validation.
 * SECTIONS IN THIS FILE:
 *   1. Nested options with DataAnnotations validation attributes
 */

using System.ComponentModel.DataAnnotations;

namespace ConfigurationOptions.Models;

/*
 * SECTION 1: NESTED OPTIONS AND DATAANNOTATIONS VALIDATION
 *
 * Complex settings (connection strings, timeouts, retry policies) live in a
 * named JSON section and map to a dedicated C# class.  This keeps Program.cs
 * registration clean and makes the options easy to test in isolation.
 *
 * WHY VALIDATE OPTIONS?
 *   Without validation a missing or malformed connection string is discovered
 *   when the first database call fails — potentially after the app has already
 *   accepted traffic.
 *
 *   With ValidateDataAnnotations() + ValidateOnStart() the app refuses to start
 *   if validation fails, producing a clear OptionsValidationException at boot.
 *
 * DataAnnotations attributes (System.ComponentModel.DataAnnotations):
 *   [Required]                — value must not be null or empty string
 *   [Range(min, max)]         — numeric value must be within range (inclusive)
 *   [MinLength(n)]            — string length must be >= n characters
 *   [MaxLength(n)]            — string length must be <= n characters
 *   [RegularExpression(pat)]  — string must match the regex pattern
 *   [EmailAddress]            — value must be a valid e-mail format
 *
 * Registration in Program.cs (SECTION 4):
 *   services.AddOptions<DatabaseOptions>()
 *           .BindConfiguration(DatabaseOptions.SectionName) // reads "Database" section
 *           .ValidateDataAnnotations()                      // enforces attributes
 *           .ValidateOnStart();                             // fails fast at startup
 *
 * PITFALLS:
 *   - ValidateDataAnnotations does not deep-validate nested objects automatically;
 *     add [ValidateObjectMembers] (custom) or validate nested types separately.
 *   - ValidateOnStart triggers during the host startup phase — before the first
 *     request, but AFTER the app is fully built with app.Build().
 *   - Hot-reload of config files does NOT re-run validation; invalid values can
 *     be loaded at runtime via IOptionsMonitor without any exception.
 *
 * appsettings.json structure for this class:
 *   "Database": {
 *     "ConnectionString": "Server=localhost;Database=DemoDb;Trusted_Connection=True;",
 *     "CommandTimeoutSeconds": 30,
 *     "MaxRetryCount": 3
 *   }
 */
public sealed class DatabaseOptions
{
    public const string SectionName = "Database"; // key in appsettings.json

    [Required(ErrorMessage = "Database.ConnectionString is required.")]
    [MinLength(10, ErrorMessage = "ConnectionString must be at least 10 characters.")]
    public string ConnectionString { get; set; } = string.Empty; // must be provided

    [Range(1, 300, ErrorMessage = "CommandTimeoutSeconds must be 1–300.")]
    public int CommandTimeoutSeconds { get; set; } = 30; // default if key is absent

    [Range(0, 10, ErrorMessage = "MaxRetryCount must be 0–10.")]
    public int MaxRetryCount { get; set; } = 3; // default retry count
}
