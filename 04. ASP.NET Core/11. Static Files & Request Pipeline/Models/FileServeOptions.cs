/*
 * FILE ROLE: Typed configuration model that collects every custom-serving
 *            knob in one place — bound from appsettings or constructed in code.
 * SECTIONS IN THIS FILE:
 *   9. FileServeOptions — typed options class with per-property explanations
 */

namespace StaticFiles.Models;

/*
 * SECTION 9: TYPED OPTIONS MODEL — FileServeOptions
 * ===================================================
 * Instead of scattering magic strings and booleans across Program.cs,
 * group all static-file tuning into a single POCO and bind it from
 * IConfiguration or pass it directly in tests and demos.
 *
 * Binding from appsettings.json (add to Program.cs):
 *   builder.Services.Configure<FileServeOptions>(
 *       builder.Configuration.GetSection("StaticFiles"));
 *
 * Direct injection after binding:
 *   app.UseStaticFilesConfigured(
 *       app.Services.GetRequiredService<IOptions<FileServeOptions>>().Value);
 *
 * Properties map 1-to-1 to StaticFileOptions fields documented in
 * SECTION 3 (Extensions/StaticFilesExtensions.cs).
 */
public sealed class FileServeOptions
{
    // --- Content type fallbacks ---

    /// <summary>
    /// MIME type returned for files whose extension has no registered mapping.
    /// Only applied when <see cref="ServeUnknownFileTypes"/> is true.
    /// Default: "application/octet-stream" triggers a browser download.
    /// </summary>
    public string DefaultContentType { get; set; } = "application/octet-stream"; // binary download

    /// <summary>
    /// When true, files with unmapped extensions are served using
    /// <see cref="DefaultContentType"/> instead of being ignored (404).
    /// RISK: exposes source files (.cs, .csproj) if they land under wwwroot.
    /// </summary>
    public bool ServeUnknownFileTypes { get; set; } = false; // secure default

    // --- Cache headers ---

    /// <summary>
    /// Number of days used to set Cache-Control: max-age on static responses.
    /// 0 = no caching  |  365 = one year (CDN / browser cache)
    /// </summary>
    public int CacheMaxAgeDays { get; set; } = 30; // 30-day browser cache

    // --- Directory features ---

    /// <summary>
    /// Enables UseDirectoryBrowser at /browse.
    /// Requires AddDirectoryBrowser() in the DI container.
    /// WARNING: never enable on production without authentication middleware
    /// placed BEFORE UseDirectoryBrowser in the pipeline.
    /// </summary>
    public bool EnableDirectoryBrowsing { get; set; } = false; // off by default

    /// <summary>
    /// Enables UseDefaultFiles so a bare-directory request (GET /) resolves
    /// to index.html without a redirect.
    /// Must be registered BEFORE UseStaticFiles in the pipeline.
    /// </summary>
    public bool EnableDefaultFiles { get; set; } = true; // on by default

    // --- Physical root override ---

    /// <summary>
    /// Absolute path to an additional static-file directory served at /extra.
    /// Empty string = feature disabled.
    /// Used by UseExternalPhysicalFiles extension method (SECTION 6a).
    /// </summary>
    public string ExtraFilesPath { get; set; } = string.Empty; // disabled if empty
}
