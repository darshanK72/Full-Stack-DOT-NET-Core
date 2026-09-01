/*
 * FILE ROLE: Custom middleware that demonstrates how to read and write
 *            HTTP cache headers — Cache-Control, ETag, and Last-Modified —
 *            and how custom middleware interacts with the static-file
 *            middleware that runs just after it in the pipeline.
 * SECTIONS IN THIS FILE:
 *   8. HTTP Cache Headers — Cache-Control, ETag, Last-Modified theory
 *   8a. CacheControlMiddleware — implementation and registration
 */

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace StaticFiles.Middleware;

/*
 * SECTION 8: HTTP CACHE HEADERS — Cache-Control, ETag, Last-Modified
 * ====================================================================
 * The browser and CDN use three response headers to decide whether to
 * fetch a fresh copy or serve from cache:
 *
 * Header            | Direction | Purpose
 * ------------------|-----------|------------------------------------------------
 * Cache-Control     | response  | Caching policy: max-age, no-cache, no-store, public/private
 * ETag              | response  | Fingerprint (hash) of the file content
 * Last-Modified     | response  | Timestamp of last change (UTC)
 * If-None-Match     | request   | Client sends its cached ETag → 304 if unchanged
 * If-Modified-Since | request   | Client sends its cached date → 304 if unchanged
 *
 * Built-in UseStaticFiles behaviour:
 *   - Sets ETag (strong, content hash) and Last-Modified automatically.
 *   - Responds 304 Not Modified when the client sends If-None-Match
 *     or If-Modified-Since that still matches the file.
 *   - Does NOT set Cache-Control by default; use OnPrepareResponse or
 *     this middleware to add it (see SECTION 3c in StaticFilesExtensions.cs).
 *
 * Cache-Control directives most used for static assets:
 *   public, max-age=2592000   → cacheable by browser AND CDN for 30 days
 *   no-cache                  → always revalidate with ETag (still fast if 304)
 *   no-store                  → never cache (PII, banking pages)
 *   immutable                 → never revalidate within max-age (hashed filenames)
 *
 * Pipeline placement note:
 *   This middleware is registered BEFORE UseStaticFiles so it can inspect
 *   the response headers AFTER static-file middleware writes them (by
 *   awaiting _next). This is a post-processing pattern — the order is:
 *     request  →  CacheControlMiddleware  →  UseStaticFiles
 *     response ←  CacheControlMiddleware  ←  UseStaticFiles
 */

/*
 * --- 8a. CacheControlMiddleware IMPLEMENTATION ---
 *
 * This middleware adds an explicit Cache-Control header to every static-
 * file response that does not already carry one. Responses to other
 * routes (API endpoints, Razor pages) are left untouched.
 *
 * Registration in Program.cs:
 *   app.UseMiddleware<CacheControlMiddleware>();  // before UseStaticFiles
 *
 * Why not OnPrepareResponse instead?
 *   OnPrepareResponse is called by UseStaticFiles for each file response,
 *   but is declared inline in StaticFileOptions — coupling the cache policy
 *   to the static-file registration.  A standalone middleware makes the
 *   caching concern reusable across multiple UseStaticFiles registrations
 *   (wwwroot, ExtraFiles, embedded, etc.) without repeating the lambda.
 *
 * Compile note:
 *   InvokeAsync must be exactly `public Task InvokeAsync(HttpContext context)`.
 *   The DI container resolves the RequestDelegate _next and any logged
 *   dependencies automatically — no factory registration needed.
 */
public sealed class CacheControlMiddleware
{
    private readonly RequestDelegate _next;      // next middleware in pipeline
    private readonly ILogger<CacheControlMiddleware> _logger;

    public CacheControlMiddleware(
        RequestDelegate next,
        ILogger<CacheControlMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // --- Pass the request down the pipeline first ---
        // After _next returns, the response headers are already written by
        // UseStaticFiles (ETag, Last-Modified, Content-Type, etc.).
        await _next(context); // static-file middleware handles the actual file

        // --- Inspect after the downstream middleware has run ---
        bool isStaticFileResponse =
            context.Response.Headers.ContainsKey("ETag") || // ETag set by UseStaticFiles
            context.Response.Headers.ContainsKey("Last-Modified"); // date set for static files

        if (!isStaticFileResponse)
            return; // not a static-file response — leave headers untouched

        // --- Add Cache-Control only if not already present ---
        // UseStaticFiles does NOT set Cache-Control by default; we add it here.
        if (!context.Response.Headers.ContainsKey("Cache-Control"))
        {
            // 30-day public cache; immutable = browser never re-fetches within max-age
            // Use only when filenames contain a content hash (e.g. site.abc123.css).
            // For unversioned filenames, use max-age without immutable so revalidation
            // still occurs after expiry.
            const string cachePolicy = "public, max-age=2592000"; // 30 days in seconds
            context.Response.Headers["Cache-Control"] = cachePolicy;

            _logger.LogDebug(
                "CacheControlMiddleware set Cache-Control: {Policy} for {Path}",
                cachePolicy,
                context.Request.Path);
        }

        // --- Log ETag for debugging conditional request flow ---
        if (context.Response.Headers.TryGetValue("ETag", out var etag))
        {
            _logger.LogDebug(
                "Static file ETag: {ETag} — client can send If-None-Match on next request",
                etag.ToString()); // ETag value already set by UseStaticFiles
        }

        // --- Conditional request outcome ---
        // HTTP 304 Not Modified is returned automatically by UseStaticFiles when:
        //   1. Client sends If-None-Match matching the current ETag, OR
        //   2. Client sends If-Modified-Since >= file's Last-Modified date.
        // In both cases _next above already returned with StatusCode = 304 and
        // an empty body — no action needed here, but we can log it:
        if (context.Response.StatusCode == StatusCodes.Status304NotModified)
        {
            _logger.LogDebug(
                "304 Not Modified for {Path} — serving from browser cache",
                context.Request.Path);
        }
    }
}
