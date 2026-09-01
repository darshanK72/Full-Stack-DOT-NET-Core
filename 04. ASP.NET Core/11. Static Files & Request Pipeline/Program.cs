/*
 * TOPIC: Static Files & Request Pipeline
 * ========================================
 * Static files — HTML, CSS, JavaScript, images, fonts — are served directly
 * by dedicated ASP.NET Core middleware without routing or controller overhead.
 * Understanding how that middleware fits into the request pipeline is
 * essential for correctness (ordering bugs cause 404s or auth bypasses) and
 * for performance (short-circuiting, cache headers, CDN readiness).
 *
 * WHY IT MATTERS:
 *   • Every web app serves static assets; misconfigured MIME types or wrong
 *     pipeline order causes subtle bugs that only appear in production.
 *   • Static-file middleware short-circuits the pipeline — it never reaches
 *     Authentication or Authorization for matched files unless you order it correctly.
 *   • Cache headers (Cache-Control, ETag, Last-Modified) are the primary
 *     lever for client and CDN performance; UseStaticFiles sets ETag and
 *     Last-Modified automatically but leaves Cache-Control to you.
 *   • Serving files from outside wwwroot (PhysicalFileProvider) and from
 *     compiled assemblies (EmbeddedFileProvider) are patterns you will need
 *     for multi-project solutions and NuGet packages.
 *
 * WHAT YOU WILL LEARN:
 *   1. Request pipeline ordering — why order matters and how short-circuiting works
 *   2. UseStaticFiles and the wwwroot web root convention
 *   3. StaticFileOptions — ContentTypeProvider, DefaultContentType,
 *      ServeUnknownFileTypes, OnPrepareResponse
 *   4. UseDefaultFiles — default document resolution (index.html)
 *   5. UseDirectoryBrowser — HTML directory listing with security notes
 *   6. Custom file providers — PhysicalFileProvider and EmbeddedFileProvider
 *   7. CDN strategies — PREVIEW
 *   8. Cache headers — Cache-Control, ETag, Last-Modified
 *   9. Typed FileServeOptions — binding configuration to a POCO
 *
 * CHAPTER MAP:
 *   1. Request pipeline order & short-circuiting  → Program.cs  (below)
 *   2. UseStaticFiles & wwwroot                   → Extensions/StaticFilesExtensions.cs  SECTION 2
 *   3. StaticFileOptions (full depth)             → Extensions/StaticFilesExtensions.cs  SECTION 3
 *   4. UseDefaultFiles                            → Extensions/StaticFilesExtensions.cs  SECTION 4
 *   5. UseDirectoryBrowser                        → Extensions/StaticFilesExtensions.cs  SECTION 5
 *   6. Custom file providers                      → Extensions/StaticFilesExtensions.cs  SECTION 6
 *   7. CDN strategies (PREVIEW)                   → Extensions/StaticFilesExtensions.cs  SECTION 7
 *   8. Cache-Control, ETag, Last-Modified         → Middleware/CacheControlMiddleware.cs  SECTION 8
 *   9. FileServeOptions typed model               → Models/FileServeOptions.cs  SECTION 9
 */

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StaticFiles.Extensions;
using StaticFiles.Middleware;
using StaticFiles.Models;

/*
 * SECTION 1: REQUEST PIPELINE ORDERING & SHORT-CIRCUITING
 * =========================================================
 * The ASP.NET Core request pipeline is an ordered chain of middleware.
 * Each middleware can:
 *   • Process the request and call next() to continue.
 *   • Short-circuit by writing the response WITHOUT calling next().
 *
 * Static-file middleware SHORT-CIRCUITS for matching file paths:
 *   Request for /css/site.css → file found → response written → pipeline stops.
 *   Remaining middleware (routing, auth, endpoints) NEVER runs.
 *
 * This has a critical security implication:
 *   If UseAuthentication is placed BEFORE UseStaticFiles, it can gate access.
 *   If UseStaticFiles is first (default position), ALL files in wwwroot are public.
 *
 * Correct canonical order:
 *   ┌──────────────────────────────────────────────────────────────────┐
 *   │ 1. UseDeveloperExceptionPage / UseExceptionHandler               │
 *   │ 2. UseHttpsRedirection                                           │
 *   │ 3. UseDefaultFiles       ← MUST precede UseStaticFiles           │
 *   │ 4. UseMiddleware<CacheControlMiddleware>  ← pre-/post-process    │
 *   │ 5. UseStaticFiles        ← SHORT-CIRCUITS for matched files      │
 *   │ 6. UseDirectoryBrowser   ← after static files                    │
 *   │ 7. UseRouting                                                     │
 *   │ 8. UseAuthentication                                              │
 *   │ 9. UseAuthorization                                               │
 *   │10. MapControllers / Map endpoints                                 │
 *   └──────────────────────────────────────────────────────────────────┘
 *
 * UseDefaultFiles MUST precede UseStaticFiles because:
 *   UseDefaultFiles rewrites the URL in memory (/ → /index.html).
 *   UseStaticFiles then serves the rewritten path.
 *   Reversed order: UseStaticFiles sees "/" → no matching file → falls through.
 *
 * CacheControlMiddleware BEFORE UseStaticFiles:
 *   Uses a post-processing pattern — await _next() runs static files first,
 *   then the middleware adds Cache-Control to the response on the way back out.
 */

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// --- Service registrations ---
builder.Services.AddDirectoryBrowser(); // required by UseDirectoryBrowser (SECTION 5)

WebApplication app = builder.Build();

// Pipeline: development vs production branches
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // detailed error page — only in dev
}
else
{
    app.UseExceptionHandler("/error"); // generic error page — production
    app.UseHsts();                     // HTTP Strict Transport Security header
}

app.UseHttpsRedirection(); // redirect HTTP → HTTPS before any file serving

// --- SECTION 1: Canonical static-file pipeline order ---

// Step 1: UseDefaultFiles — rewrite "/" to "/index.html" BEFORE UseStaticFiles.
app.UseDefaultDocument(); // extension method → Extensions/StaticFilesExtensions.cs SECTION 4

// Step 2: CacheControlMiddleware — wraps UseStaticFiles responses on the way out.
app.UseMiddleware<CacheControlMiddleware>(); // post-processes cache headers (SECTION 8)

// Step 3: UseStaticFiles — serves wwwroot files; short-circuits for matches.
// Build FileServeOptions with sensible defaults for this demo.
var fileServeOptions = new FileServeOptions
{
    DefaultContentType    = "application/octet-stream", // download for unknown types
    ServeUnknownFileTypes = false,                      // secure: block unmapped extensions
    CacheMaxAgeDays       = 30,                         // 30-day browser cache for assets
    EnableDirectoryBrowsing = true,                     // demo only — disable in production
    EnableDefaultFiles    = true
};

app.UseStaticFilesConfigured(fileServeOptions); // extension method → SECTION 3

// Step 4: UseDirectoryBrowser — HTML listing for /browse (after static files).
app.UseBrowsableFiles(app.Environment); // extension method → SECTION 5

// Step 5: UseExternalPhysicalFiles — /extra/* served from wwwroot for demo.
app.UseExternalPhysicalFiles(app.Environment); // extension method → SECTION 6a

// Step 6: UseEmbeddedFiles — /embedded/* served from assembly manifest.
app.UseEmbeddedFiles(); // extension method → SECTION 6b

// Step 7: Routing + minimal endpoint for non-static requests.
app.UseRouting();

// Map a minimal endpoint so the app has something to do beyond static files.
app.MapGet("/health", () => "Healthy"); // simple liveness check
app.MapGet("/pipeline-order", () =>
    "Static files are served at steps 1-4 above. Routing and endpoints run here at step 7.");

app.Run();

/*
 * QUICK REFERENCE — Static Files & Request Pipeline
 * ====================================================
 *
 * Registration shortcuts:
 *   app.UseStaticFiles()                     Default wwwroot, default options
 *   app.UseStaticFiles(StaticFileOptions)    Custom MIME, cache, fallback
 *   app.UseDefaultFiles()                    Rewrite / → /index.html
 *   app.UseDirectoryBrowser()               HTML listing (needs AddDirectoryBrowser)
 *   app.UseFileServer()                      UseDefaultFiles + UseStaticFiles in one call
 *
 * StaticFileOptions key members:
 *   .ContentTypeProvider     IContentTypeProvider     MIME map (default: 370+ types)
 *   .DefaultContentType      string                   Fallback MIME type
 *   .ServeUnknownFileTypes   bool                     Serve if no MIME match (risky)
 *   .OnPrepareResponse       Action<SFResponseCtx>    Hook: add headers, log, auth
 *   .FileProvider            IFileProvider            Serve from custom directory
 *   .RequestPath             PathString               URL prefix for this registration
 *
 * File providers:
 *   new PhysicalFileProvider(path)           Disk directory (absolute path required)
 *   new EmbeddedFileProvider(asm, ns)        Compiled-in EmbeddedResource items
 *   new CompositeFileProvider(p1, p2, ...)   Try providers in order, first match wins
 *
 * Cache headers (set in OnPrepareResponse or CacheControlMiddleware):
 *   Cache-Control: public, max-age=2592000   Cache 30 days, shareable (browser + CDN)
 *   Cache-Control: no-cache                  Revalidate each request via ETag
 *   Cache-Control: no-store                  Never cache (sensitive data)
 *   ETag                                     Content hash — set automatically by middleware
 *   Last-Modified                            File timestamp — set automatically
 *   → 304 Not Modified returned when If-None-Match / If-Modified-Since matches
 *
 * Pipeline ordering rules (never deviate):
 *   UseDefaultFiles  BEFORE  UseStaticFiles
 *   UseStaticFiles   BEFORE  UseRouting
 *   UseAuthentication AFTER  UseRouting, BEFORE UseAuthorization
 *   UseAuthorization  BEFORE  MapControllers / endpoints
 *
 * Short-circuit:
 *   Static file middleware returns WITHOUT calling next() for matched files.
 *   Auth/routing middleware NEVER runs for those requests.
 *   To protect a static file: place UseAuthentication/Authorization BEFORE UseStaticFiles,
 *   or move the protected file outside wwwroot and serve via a controller action.
 *
 * CDN readiness (PREVIEW):
 *   Set Cache-Control: public, max-age=<long> so CDN nodes cache the response.
 *   Use URL fingerprinting (app.abc123.js) with immutable flag to bust cache reliably.
 *   Full CDN pipeline → future topic folder.
 */
