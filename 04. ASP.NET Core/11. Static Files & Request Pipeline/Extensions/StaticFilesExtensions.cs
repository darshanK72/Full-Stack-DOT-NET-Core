/*
 * FILE ROLE: IApplicationBuilder/WebApplication extension methods that
 *            demonstrate every static-file middleware configuration option:
 *            UseStaticFiles, StaticFileOptions, UseDefaultFiles,
 *            UseDirectoryBrowser, custom file providers, MIME mapping,
 *            and a PREVIEW of CDN integration.
 * SECTIONS IN THIS FILE:
 *   2. UseStaticFiles & wwwroot folder
 *   3. StaticFileOptions — full configuration depth
 *      3a. ContentTypeProvider & MIME mapping
 *      3b. DefaultContentType & ServeUnknownFileTypes
 *      3c. OnPrepareResponse callback
 *   4. UseDefaultFiles — default document resolution
 *   5. UseDirectoryBrowser — HTML directory listing
 *   6. Custom file providers — PhysicalFileProvider & EmbeddedFileProvider
 *      6a. PhysicalFileProvider: serving files outside wwwroot
 *      6b. EmbeddedFileProvider: serving compiled-in resources
 *   7. CDN strategies (PREVIEW)
 */

using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using StaticFiles.Models;

namespace StaticFiles.Extensions;

public static class StaticFilesExtensions
{
    /*
     * SECTION 2: UseStaticFiles & THE wwwroot FOLDER
     * ================================================
     * UseStaticFiles() is an ASP.NET Core middleware that serves files
     * from the web root directory (wwwroot by default) directly to clients
     * without passing the request to any subsequent middleware.
     *
     * Web root (wwwroot):
     *   • Configured as IWebHostEnvironment.WebRootPath.
     *   • Default path: <ContentRoot>/wwwroot (set by WebApplication.CreateBuilder).
     *   • Only files placed here are served — source files (.cs, .csproj)
     *     live under ContentRoot but NOT under WebRootPath, so they are safe.
     *   • Can be changed: builder.WebHost.UseWebRoot("public");
     *
     * URL → File mapping (default):
     *   GET /css/site.css  → wwwroot/css/site.css
     *   GET /index.html    → wwwroot/index.html
     *   GET /img/logo.png  → wwwroot/img/logo.png
     *
     * Short-circuit behaviour (crucial):
     *   When a matching file is found, UseStaticFiles writes the response
     *   and does NOT call the next middleware. This means:
     *   • Authentication/Authorization middleware NEVER runs for matched files.
     *   • Routing middleware NEVER runs for matched files.
     *   Register auth middleware BEFORE UseStaticFiles if you need to protect
     *   static files — or move protected assets outside wwwroot entirely.
     *
     * File NOT found:
     *   If the URL has no matching file, UseStaticFiles calls next() and
     *   the request continues down the pipeline to routing / endpoints.
     *
     * Pipeline position diagram:
     *   ┌──────────────────────────────────────────────────┐
     *   │  1. UseExceptionHandler (or UseDeveloperException)│
     *   │  2. UseHttpsRedirection                           │
     *   │  3. UseDefaultFiles   ← BEFORE static files      │
     *   │  4. UseStaticFiles    ← SHORT-CIRCUITS here       │
     *   │  5. UseRouting                                    │
     *   │  6. UseAuthentication                             │
     *   │  7. UseAuthorization                              │
     *   │  8. MapControllers / Map endpoints               │
     *   └──────────────────────────────────────────────────┘
     */
    public static WebApplication UseDefaultWwwroot(this WebApplication app)
    {
        // Simplest form — serves everything from wwwroot with default options.
        // MIME types, ETag, and Last-Modified are added automatically.
        app.UseStaticFiles(); // serves wwwroot/**

        return app;
    }

    /*
     * SECTION 3: StaticFileOptions — FULL CONFIGURATION
     * ===================================================
     * StaticFileOptions lets you override every aspect of how files are served.
     * Pass it to UseStaticFiles(options) instead of calling the parameterless form.
     *
     * Key properties:
     *
     * Property                | Type                        | Purpose
     * ------------------------|-----------------------------|-------------------------------
     * ContentTypeProvider     | IContentTypeProvider        | Maps extensions to MIME types
     * DefaultContentType      | string                      | Fallback MIME for unknown ext
     * ServeUnknownFileTypes   | bool                        | Serve files with no MIME match
     * OnPrepareResponse       | Action<StaticFileResponse>  | Called before headers are sent
     * FileProvider            | IFileProvider               | Source directory (default: wwwroot)
     * RequestPath             | PathString                  | URL prefix (default: "")
     */

    /*
     * --- 3a. ContentTypeProvider & MIME TYPE MAPPING ---
     *
     * IContentTypeProvider maps file extensions to MIME (Content-Type) strings.
     * The default implementation is FileExtensionContentTypeProvider which
     * ships with ~370 built-in mappings.
     *
     * Adding a custom MIME type:
     *   provider.Mappings[".webmanifest"] = "application/manifest+json";
     *
     * Removing a mapping (e.g., block .config downloads):
     *   provider.Mappings.Remove(".config");
     *
     * MIME type matters because:
     *   • Browsers use Content-Type to decide how to render/download a file.
     *   • Wrong MIME causes CSS/JS to be blocked (browser CORB / MIME sniff).
     *   • application/octet-stream triggers a download dialog.
     *
     * Common MIME types:
     *   Extension | MIME type
     *   ----------|----------------------------
     *   .html     | text/html; charset=utf-8
     *   .css      | text/css
     *   .js       | application/javascript
     *   .json     | application/json
     *   .png      | image/png
     *   .svg      | image/svg+xml
     *   .woff2    | font/woff2
     */

    /*
     * --- 3b. DefaultContentType & ServeUnknownFileTypes ---
     *
     * By default, files with no registered extension are not served at all
     * (the request falls through to the next middleware → usually 404).
     *
     * ServeUnknownFileTypes = true  → serve them using DefaultContentType.
     * DefaultContentType (default)  → "application/octet-stream" (download).
     *
     * RISK: if source files (.cs, .csproj, appsettings.json) end up under
     * wwwroot AND ServeUnknownFileTypes is true, they will be served.
     * Keep ServeUnknownFileTypes = false in production unless you control
     * exactly what lands in wwwroot.
     */

    /*
     * --- 3c. OnPrepareResponse CALLBACK ---
     *
     * Called synchronously by the middleware for every served file, just
     * before the response headers are written to the socket.
     * StaticFileResponseContext gives access to:
     *   .Context  → HttpContext  (request, response, user, services)
     *   .File     → IFileInfo   (file name, length, last modified)
     *
     * Typical uses:
     *   • Add Cache-Control header per file extension.
     *   • Add security headers (X-Content-Type-Options).
     *   • Log access to specific files.
     *   • Reject unauthenticated requests at the file level.
     *
     * NOTE: OnPrepareResponse is synchronous.  For async work (e.g. DB
     * auth check) use a proper middleware before UseStaticFiles instead.
     */
    public static WebApplication UseStaticFilesConfigured(
        this WebApplication app, FileServeOptions options)
    {
        // --- 3a: Build the content type provider with custom MIME additions ---
        var contentTypeProvider = new FileExtensionContentTypeProvider(); // 370+ built-in types

        // Add mappings not in the default set
        contentTypeProvider.Mappings[".webmanifest"] = "application/manifest+json"; // PWA manifest
        contentTypeProvider.Mappings[".br"]          = "application/x-brotli";      // pre-compressed
        contentTypeProvider.Mappings[".data"]        = "application/octet-stream";  // Unity WebGL

        // Remove sensitive extension so .config files are never served
        contentTypeProvider.Mappings.Remove(".config"); // block web.config, app.config

        var staticFileOptions = new StaticFileOptions
        {
            // --- 3a: Plug in the customised provider ---
            ContentTypeProvider = contentTypeProvider,                          // custom MIME map

            // --- 3b: Fallback for unknown extensions ---
            DefaultContentType    = options.DefaultContentType,                 // e.g. "application/octet-stream"
            ServeUnknownFileTypes = options.ServeUnknownFileTypes,             // false = secure default

            // --- 3c: OnPrepareResponse — add Cache-Control per file type ---
            OnPrepareResponse = ctx =>
            {
                // ctx.File gives IFileInfo; ctx.Context gives full HttpContext
                string extension = Path.GetExtension(ctx.File.Name).ToLowerInvariant();

                int maxAgeDays = extension switch
                {
                    ".css" or ".js"           => options.CacheMaxAgeDays,     // versioned assets
                    ".png" or ".jpg" or ".svg" => options.CacheMaxAgeDays,    // images
                    ".html"                    => 0,                           // no cache for HTML
                    _                          => 1                            // 1-day default
                };

                if (maxAgeDays == 0)
                {
                    // HTML pages: always revalidate
                    ctx.Context.Response.Headers["Cache-Control"] = "no-cache"; // revalidate each time
                }
                else
                {
                    int maxAgeSeconds = maxAgeDays * 86_400;                     // days → seconds
                    ctx.Context.Response.Headers["Cache-Control"] =
                        $"public, max-age={maxAgeSeconds}";                     // browser + CDN cache
                }

                // Security header: block browser MIME-sniffing
                ctx.Context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            }
        };

        app.UseStaticFiles(staticFileOptions); // wwwroot with full options applied

        return app;
    }

    /*
     * SECTION 4: UseDefaultFiles — DEFAULT DOCUMENT RESOLUTION
     * ==========================================================
     * UseDefaultFiles rewrites a bare-directory URL to a default filename
     * BEFORE UseStaticFiles processes it.
     *
     *   GET /          →  rewrites to  →  GET /index.html
     *   GET /docs/     →  rewrites to  →  GET /docs/index.html
     *
     * Default filenames (checked in order by DefaultFilesOptions):
     *   1. default.htm
     *   2. default.html
     *   3. index.htm
     *   4. index.html
     *
     * CRITICAL ordering rule:
     *   UseDefaultFiles MUST come BEFORE UseStaticFiles.
     *   UseDefaultFiles only rewrites the request path in memory — it does not
     *   serve the file itself. UseStaticFiles then serves the rewritten path.
     *   Wrong order = UseStaticFiles runs first, sees "/" → no matching file → falls through.
     *
     * UseFileServer() shortcut:
     *   app.UseFileServer() = UseDefaultFiles() + UseStaticFiles() in one call.
     *   Use it only when you do not need separate StaticFileOptions for each.
     */
    public static WebApplication UseDefaultDocument(this WebApplication app)
    {
        var defaultFilesOptions = new DefaultFilesOptions();
        defaultFilesOptions.DefaultFileNames.Clear();          // remove default list
        defaultFilesOptions.DefaultFileNames.Add("index.html"); // only resolve index.html
        // Could also add: defaultFilesOptions.DefaultFileNames.Add("default.html");

        app.UseDefaultFiles(defaultFilesOptions); // must precede UseStaticFiles

        return app;
    }

    /*
     * SECTION 5: UseDirectoryBrowser — HTML DIRECTORY LISTING
     * =========================================================
     * Renders a browsable HTML page listing all files and sub-folders
     * at a given URL path.
     *
     * Service registration (REQUIRED in Program.cs):
     *   builder.Services.AddDirectoryBrowser();
     *   Without this, UseDirectoryBrowser throws InvalidOperationException.
     *
     * Security warning:
     *   Directory browsing exposes the full file tree.
     *   NEVER enable for all paths in production.
     *   If you must expose it, place authentication middleware BEFORE it.
     *
     * Typical use:
     *   app.UseDirectoryBrowser(new DirectoryBrowserOptions
     *   {
     *       FileProvider = new PhysicalFileProvider(path),
     *       RequestPath  = "/browse"
     *   });
     *
     * Difference from UseStaticFiles:
     *   UseStaticFiles serves the FILE CONTENT.
     *   UseDirectoryBrowser serves an HTML PAGE listing the directory.
     *   They can coexist at the same path — the browser decides which to use.
     */
    public static WebApplication UseBrowsableFiles(
        this WebApplication app, IWebHostEnvironment env)
    {
        app.UseDirectoryBrowser(new DirectoryBrowserOptions
        {
            FileProvider = new PhysicalFileProvider(env.WebRootPath), // browse wwwroot
            RequestPath  = new PathString("/browse")                   // at /browse URL
        });

        return app;
    }

    /*
     * SECTION 6: CUSTOM FILE PROVIDERS
     * ==================================
     * UseStaticFiles is not limited to wwwroot.  You can point it at any
     * file system path or embedded resource assembly via IFileProvider.
     *
     * Built-in providers:
     *
     * Provider               | Source
     * -----------------------|--------------------------------------------
     * PhysicalFileProvider   | A directory on disk (inside or outside wwwroot)
     * EmbeddedFileProvider   | Resources compiled into the assembly (.resx / EmbeddedResource)
     * CompositeFileProvider  | Combines multiple providers — checked in order
     *
     * StaticFileOptions.FileProvider overrides the default wwwroot root.
     * StaticFileOptions.RequestPath sets the URL prefix for that provider.
     */

    /*
     * --- 6a. PhysicalFileProvider — SERVING FILES OUTSIDE wwwroot ---
     *
     * Use case: shared asset library, file upload directory, or a legacy
     * folder that should not live under wwwroot but still be served.
     *
     * The directory MUST exist on disk before UseStaticFiles processes a request.
     * If it does not exist PhysicalFileProvider throws at construction time.
     *
     * Example: serve C:\Assets at URL /assets
     *   var provider = new PhysicalFileProvider(@"C:\Assets");
     *   app.UseStaticFiles(new StaticFileOptions
     *   {
     *       FileProvider = provider,
     *       RequestPath  = "/assets"
     *   });
     *
     * Serving from inside ContentRoot (sibling to wwwroot):
     *   Path.Combine(env.ContentRootPath, "SharedAssets")
     */
    public static WebApplication UseExternalPhysicalFiles(
        this WebApplication app, IWebHostEnvironment env)
    {
        // Use wwwroot itself as the "extra" folder for this demo
        // (a real app would point to a path outside wwwroot).
        string physicalPath = env.WebRootPath; // exists — safe to pass to PhysicalFileProvider

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(physicalPath), // explicit physical root
            RequestPath  = new PathString("/extra")               // URL: /extra/site.css etc.
        });

        return app;
    }

    /*
     * --- 6b. EmbeddedFileProvider — SERVING COMPILED-IN RESOURCES ---
     *
     * Resources compiled into the assembly with <EmbeddedResource> in the
     * .csproj can be served via EmbeddedFileProvider — no disk files needed
     * at deployment time.  Useful for NuGet packages shipping admin UIs.
     *
     * Setup in .csproj:
     *   <ItemGroup>
     *     <EmbeddedResource Include="EmbeddedAssets\**" />
     *   </ItemGroup>
     *
     * Base namespace used to locate resources matches the assembly name
     * and folder path (dots replace directory separators):
     *   StaticFiles.EmbeddedAssets.logo.png
     *
     * If no embedded resources exist, EmbeddedFileProvider is constructed
     * without error — it simply returns "not found" for every request.
     */
    public static WebApplication UseEmbeddedFiles(this WebApplication app)
    {
        var embeddedProvider = new EmbeddedFileProvider(
            assembly: Assembly.GetExecutingAssembly(),   // this DLL
            baseNamespace: "StaticFiles.EmbeddedAssets"); // folder → namespace prefix

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = embeddedProvider,              // read from assembly manifest
            RequestPath  = new PathString("/embedded")   // URL: /embedded/logo.png
        });

        return app;
    }

    /*
     * SECTION 7: CDN STRATEGIES — PREVIEW
     * =====================================
     * COVERED IN DETAIL LATER → CDN and Asset Bundling (separate topic)
     *
     * Static-file middleware is intentionally simple: it serves files from
     * local disk or embedded resources.  For production-scale caching,
     * geo-distribution, and bandwidth offloading you layer a CDN in front.
     *
     * Quick orientation:
     *
     * Strategy          | How it works
     * ------------------|-------------------------------------------------------
     * Pull CDN          | CDN fetches from your origin on first request, then caches.
     *                   | Configure Cache-Control: public, max-age=... so CDN holds it.
     * Push CDN          | You upload assets to CDN storage (Azure Blob, S3).
     *                   | UseStaticFiles is bypassed; app only handles API routes.
     * URL fingerprinting| <script src="/js/app.a3f9b2.js"> — hash in filename.
     *                   | Use Cache-Control: immutable + long max-age.
     *                   | Old URLs still work; new deployment → new URL → instant cache bust.
     * asp-append-version| Razor Tag Helper appends ?v=<hash> query string automatically.
     *                   | Works without renaming files; less efficient than fingerprinting.
     *
     * In this app the Cache-Control headers set in SECTION 3c and
     * CacheControlMiddleware (SECTION 8) prepare responses for CDN pull.
     * Full CDN pipeline setup → future topic folder.
     */
}
