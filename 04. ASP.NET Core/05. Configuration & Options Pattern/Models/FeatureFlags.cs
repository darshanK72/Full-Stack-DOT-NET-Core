/*
 * FILE ROLE: Feature toggle options class — structured for named options and live
 *   monitoring, demonstrating how multiple configurations of the same type coexist.
 * SECTIONS IN THIS FILE:
 *   1. Feature flag options — named options setup and IOptionsMonitor patterns
 */

namespace ConfigurationOptions.Models;

/*
 * SECTION 1: FEATURE FLAG OPTIONS — NAMED OPTIONS AND IOptionsMonitor
 *
 * Named options let you register multiple configurations of the SAME type, each
 * bound to a different JSON section.  This is ideal for feature flags that differ
 * per area, tenant, API version, or user role.
 *
 * NAMED OPTIONS REGISTRATION (Program.cs SECTION 5):
 *   services.Configure<FeatureFlags>(config.GetSection(SectionName));          // default ("")
 *   services.Configure<FeatureFlags>("Admin", config.GetSection("AdminFeatureFlags")); // named
 *   services.Configure<FeatureFlags>("Api",   config.GetSection("ApiFeatureFlags"));   // named
 *
 * RETRIEVAL — THREE INTERFACES FOR NAMED OPTIONS:
 *   Interface            Named access                 Lifetime
 *   ─────────────────────────────────────────────────────────────────────────
 *   IOptions<T>          .Value only (default "")     Singleton — never changes
 *   IOptionsSnapshot<T>  .Get(name) per scope         Scoped — per HTTP request
 *   IOptionsMonitor<T>   .Get(name) live               Singleton — updates on change
 *   ─────────────────────────────────────────────────────────────────────────
 *
 * IOptionsMonitor<T> is the PREFERRED interface for named options because:
 *   1. Singleton-safe — can be injected into any service lifetime
 *   2. .CurrentValue gives the current default snapshot (same as .Get(""))
 *   3. .Get(name) retrieves any named configuration
 *   4. .OnChange(callback) fires when config reloads
 *
 * appsettings.json structure (see appsettings.json):
 *   "FeatureFlags":      { "EnableNewDashboard": true,  "EnableBetaApi": false, "MaxUploadSizeMb": 10  }
 *   "AdminFeatureFlags": { "EnableNewDashboard": true,  "EnableBetaApi": true,  "MaxUploadSizeMb": 100 }
 *   "ApiFeatureFlags":   { "EnableNewDashboard": false, "EnableBetaApi": true,  "MaxUploadSizeMb": 25  }
 *
 * appsettings.Development.json OVERRIDES (applied after base file):
 *   "FeatureFlags": { "EnableBetaApi": true }   — only the overridden keys need to appear
 *
 * PARTIAL OVERRIDE RULE:
 *   When a key appears in appsettings.Development.json it replaces only that key.
 *   Unmentioned keys fall back to appsettings.json values.
 *   Result in Development: EnableNewDashboard stays true, EnableBetaApi becomes true.
 */
public sealed class FeatureFlags
{
    public const string SectionName = "FeatureFlags"; // default JSON section key

    public bool EnableNewDashboard { get; set; }        // false if missing from config
    public bool EnableBetaApi      { get; set; }        // false if missing from config
    public int  MaxUploadSizeMb    { get; set; } = 10;  // default upload size limit
}
