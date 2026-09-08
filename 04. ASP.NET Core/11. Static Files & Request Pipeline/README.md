# Static Files & Request Pipeline — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is the purpose of the `wwwroot` folder?](#q1-what-is-the-purpose-of-the-wwwroot-folder)
2. [Q2. What does `UseStaticFiles()` do?](#q2-what-does-usestaticfiles-do)
3. [Q3. What is the difference between `UseDefaultFiles()` and `UseStaticFiles()`?](#q3-what-is-the-difference-between-usedefaultfiles-and-usestaticfiles)
4. [Q4. What security risk does `UseDirectoryBrowser()` pose?](#q4-what-security-risk-does-usedirectorybrowser-pose)
5. [Q5. Where should static file middleware be placed in the pipeline?](#q5-where-should-static-file-middleware-be-placed-in-the-pipeline)
6. [Q6. How do you serve a Single Page Application (SPA) with ASP.NET Core?](#q6-how-do-you-serve-a-single-page-application-spa-with-aspnet-core)
7. [Q7. What is SPA fallback routing, and why is it needed?](#q7-what-is-spa-fallback-routing-and-why-is-it-needed)
8. [Q8. How do you prevent SPA fallback from intercepting API routes?](#q8-how-do-you-prevent-spa-fallback-from-intercepting-api-routes)
9. [Q9. What are MIME types, and how does ASP.NET Core determine them for static files?](#q9-what-are-mime-types-and-how-does-aspnet-core-determine-them-for-static-files)
10. [Q10. What is `StaticFileOptions`?](#q10-what-is-staticfileoptions)
11. [Q11. What is `FileExtensionContentTypeProvider`?](#q11-what-is-fileextensioncontenttypeprovider)
12. [Q12. What does `ServeUnknownFileTypes` do, and when is it risky?](#q12-what-does-serveunknownfiletypes-do-and-when-is-it-risky)
13. [Q13. What is a path traversal attack in the context of file serving?](#q13-what-is-a-path-traversal-attack-in-the-context-of-file-serving)
14. [Q14. How do you set cache-control headers for static assets?](#q14-how-do-you-set-cache-control-headers-for-static-assets)
15. [Q15. Why should hashed JS/CSS files be cached aggressively but `index.html` should not?](#q15-why-should-hashed-jscss-files-be-cached-aggressively-but-indexhtml-should-not)
16. [Q16. What is the difference between serving files from `wwwroot` vs a custom folder?](#q16-what-is-the-difference-between-serving-files-from-wwwroot-vs-a-custom-folder)
17. [Q17. Can static files be served without placing them in `wwwroot`?](#q17-can-static-files-be-served-without-placing-them-in-wwwroot)
18. [Q18. What happens if sensitive files (e.g., `.env`, `appsettings.Production.json`) are placed in `wwwroot`?](#q18-what-happens-if-sensitive-files-eg-env-appsettingsproductionjson-are-placed-in-wwwroot)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What is the purpose of the `wwwroot` folder?

**Concepts**
- IWebHostEnvironment.WebRootPath mapping
- Public static content served without controller logic
- Anonymous access by default — no authentication gate
- SPA build output landing here

**Answer**

`wwwroot` is the default web root directory whose files are served as public static content — HTML, CSS, JavaScript, images, and fonts — directly over HTTP without hitting controller logic. It maps to `IWebHostEnvironment.WebRootPath` and is the default root for `UseStaticFiles()`. Files in `wwwroot` are included in publish output and deployed to the server as-is, so anything placed there is publicly accessible to anonymous clients — there is no authentication gate by default. SPA builds from React, Angular, and Blazor WASM typically place `index.html` and bundled assets here.

---

## Q2. What does `UseStaticFiles()` do?

**Concepts**
- URL-to-filesystem mapping and short-circuit on file match
- FileExtensionContentTypeProvider for MIME type detection
- StaticFileOptions for custom roots and headers
- No application code — optimized for direct file serving

**Answer**

`UseStaticFiles()` registers middleware that intercepts HTTP requests, maps the URL path to a file under the configured web root, and returns the file with the correct content type and caching headers if it exists. When no matching file is found, the request passes to the next middleware — routing, endpoints, or other handlers. The default configuration serves from `wwwroot` with standard MIME type detection via `FileExtensionContentTypeProvider`. Customize behavior through `StaticFileOptions` — custom roots, content types, response headers, or request-path prefixes.

---

## Q3. What is the difference between `UseDefaultFiles()` and `UseStaticFiles()`?

**Concepts**
- UseDefaultFiles() rewrites path only — does not serve content
- UseStaticFiles() reads and returns file bytes
- Registration order — UseDefaultFiles must come first
- Configurable default file names via DefaultFilesOptions

**Answer**

`UseDefaultFiles()` rewrites directory requests (e.g., `/` or `/docs/`) to a default file name like `index.html` without serving any content itself — it only modifies `Request.Path` for the next middleware to act on. `UseStaticFiles()` reads and returns the file bytes. Because `UseDefaultFiles()` only rewrites and does not serve, it must be called before `UseStaticFiles()` so the rewrite happens before the file lookup. Together they enable `GET /` to return `wwwroot/index.html` without exposing `/index.html` in the URL. Default file names include `index.html`, `index.htm`, and `default.html` — configurable via `DefaultFilesOptions`.

---

## Q4. What security risk does `UseDirectoryBrowser()` pose?

**Concepts**
- HTML directory listing as information disclosure
- File and folder name enumeration
- Development-only acceptable use
- Prefer explicit static file serving over directory exposure

**Answer**

`UseDirectoryBrowser()` enables HTML directory listings, exposing file and folder names to anyone who can reach the URL — a serious information disclosure vulnerability on public sites. Attackers can enumerate backup files, source maps, upload folders, and forgotten assets without guessing paths. It is acceptable only in controlled Development environments and must never be enabled on public production hosts. When combined with misconfigured upload directories, it accelerates discovery of sensitive or executable files. Prefer explicit file serving via `UseStaticFiles()` with known file names and hide all directory structure.

---

## Q5. Where should static file middleware be placed in the pipeline?

**Concepts**
- Static files before endpoint mapping to prevent route short-circuiting issues
- API endpoints before SPA fallback
- Common pipeline order for mixed SPA + API apps

**Answer**

Place `UseDefaultFiles()` and `UseStaticFiles()` after routing is established but before SPA fallback, with API endpoints mapped before fallback so `/api/*` routes are not swallowed by static handling. The common order for a mixed SPA and API app is: exception handler → forwarded headers → HTTPS → routing → auth → endpoints (`MapControllers`) → default files → static files → SPA fallback (`MapFallbackToFile`). Static middleware short-circuits when a file matches, so its position relative to routing affects whether the endpoint or the file wins for overlapping paths. For SPAs, fallback to `index.html` must come after API endpoint mapping.

---

## Q6. How do you serve a Single Page Application (SPA) with ASP.NET Core?

**Concepts**
- SPA build output published to wwwroot
- UseDefaultFiles() + UseStaticFiles() for root and asset paths
- MapFallbackToFile for client-side deep links
- CDN for static assets in production while Kestrel handles API

**Answer**

Publish the SPA build output (from npm/vite/webpack) to `wwwroot`, serve static assets with `UseStaticFiles()`, and add a fallback route that returns `index.html` for client-side routes not matched by API endpoints or physical static files. `UseDefaultFiles()` before `UseStaticFiles()` handles `GET /` returning `wwwroot/index.html`. `app.MapFallbackToFile("index.html")` after API routes ensures deep links like `/orders/123` work on browser refresh since no server file exists at that path. In production, many teams serve static assets from a CDN while Kestrel handles only API routes and the HTML fallback.

---

## Q7. What is SPA fallback routing, and why is it needed?

**Concepts**
- index.html returned for non-file, non-endpoint paths
- Client-side router handling navigation after load
- Fallback as last-resort catch-all after all other middleware
- API 404s must not be intercepted by fallback

**Answer**

SPA fallback routing returns `index.html` for URLs that do not match a physical file or server endpoint, allowing the client-side router to handle navigation for deep links and browser refreshes. Without it, refreshing `/dashboard/settings` returns 404 because no server file exists at that path — the client router never gets a chance to render the correct view. Fallback runs only when no endpoint matched and no static file was found, so it is a genuine last resort. The client router reads the URL after `index.html` loads and renders the correct component. Fallback must not intercept valid API 404 responses — map API routes first.

---

## Q8. How do you prevent SPA fallback from intercepting API routes?

**Concepts**
- MapControllers and MapGroup before MapFallbackToFile
- Consistent /api prefix for backend routes
- Registration order as the enforcement mechanism
- Test with curl to verify JSON 404 not HTML 200

**Answer**

Register all API endpoints before `MapFallbackToFile`, since endpoint routing resolves matches in registration order. If fallback is registered first, `/api/unknown` returns `index.html` with HTTP 200, breaking API clients silently — since the status code is 200, clients do not detect the failure and attempt to parse HTML as JSON. Use a consistent `/api` prefix for all backend routes and register `MapControllers()` or `MapGroup("/api")` before the fallback delegate. Test by calling an unknown API path with `curl` and verifying JSON 404, not HTML 200.

---

## Q9. What are MIME types, and how does ASP.NET Core determine them for static files?

**Concepts**
- Content-Type header telling browsers how to interpret file bytes
- FileExtensionContentTypeProvider as the default extension-to-MIME mapping
- Wrong MIME types causing browser blocking or misrender
- Custom mappings for .wasm, .webp, and uncommon extensions

**Answer**

MIME types (Content-Type values) tell browsers how to interpret file bytes — the wrong MIME type causes browsers to block scripts under CSP, prompt downloads instead of rendering, or misinterpret content entirely. ASP.NET Core maps file extensions to MIME types via `FileExtensionContentTypeProvider` when serving static files: `.html` → `text/html`, `.css` → `text/css`, `.js` → `application/javascript`, `.png` → `image/png`. For uncommon extensions such as `.wasm`, `.webp`, or custom fonts, customize mappings in `StaticFileOptions.ContentTypeProvider` before registering `UseStaticFiles()`. `ServeUnknownFileTypes` falls back to `application/octet-stream` when enabled, but explicit registration is safer.

---

## Q10. What is `StaticFileOptions`?

**Concepts**
- FileProvider and RequestPath for custom roots at URL prefixes
- OnPrepareResponse for per-file headers and auth checks
- ContentTypeProvider for custom MIME mappings
- Serving uploaded content from a controlled directory

**Answer**

`StaticFileOptions` configures static file middleware behavior — the file provider root, request path prefix, content type mappings, default content type, response headers, and whether unknown file types are served. Pass it to `app.UseStaticFiles(new StaticFileOptions { … })` to serve files from a non-`wwwroot` folder at a URL subpath: `FileProvider = new PhysicalFileProvider(path)` combined with `RequestPath = "/images"`. `OnPrepareResponse` adds cache-control headers, security headers, or per-file authorization checks on each response without touching controller logic.

---

## Q11. What is `FileExtensionContentTypeProvider`?

**Concepts**
- Dictionary-backed extension-to-MIME mapping
- Extending default mappings for custom types
- Missing mappings causing 404 or octet-stream
- Security-sensitive types requiring explicit not guessed mappings

**Answer**

`FileExtensionContentTypeProvider` is a dictionary-backed lookup that maps file extensions to IANA MIME type strings for the `Content-Type` response header. The default provider covers common web extensions; extend with `provider.Mappings[".myext"] = "application/x-custom"` before passing it to `StaticFileOptions`. Missing mappings cause 404 (the default) or `application/octet-stream` depending on the `ServeUnknownFileTypes` setting. For security-sensitive types, keep mappings explicit — do not rely on `ServeUnknownFileTypes` to serve `.config` or `.cs` files with guessed types.

---

## Q12. What does `ServeUnknownFileTypes` do, and when is it risky?

**Concepts**
- Serving unmapped extensions as application/octet-stream
- Default is false — unknown extensions not served
- Risk of serving .config, .bak, .env as downloads
- Explicit ContentTypeProvider registration as the safer alternative

**Answer**

When `ServeUnknownFileTypes` is `true`, static files middleware serves files whose extension has no MIME mapping using `DefaultContentType` — typically `application/octet-stream` — rather than returning 404. The risk on public sites is that extensions like `.config`, `.bak`, or `.env` become downloadable if placed in the served root, since the middleware makes no distinction about file sensitivity. Browsers may also sniff content and execute HTML or JavaScript served with wrong types, enabling XSS in edge cases. The default is `false` — unknown extensions are not served — and the preferred approach is to register needed extensions explicitly in `ContentTypeProvider`.

---

## Q13. What is a path traversal attack in the context of file serving?

**Concepts**
- ../ sequences escaping the intended directory
- Path.GetFullPath + prefix check as the validation pattern
- UseStaticFiles() normalizing paths to the configured FileProvider root
- Never serving files based on raw user-supplied paths

**Answer**

Path traversal exploits insufficient path sanitization so attackers request URLs containing `../` sequences to read files outside the intended directory — for example `GET /files/../../appsettings.json`. Built-in `UseStaticFiles()` normalizes paths and restricts lookups to the configured `FileProvider` root, which makes it generally safe when not misconfigured. The danger arises in custom download endpoints that concatenate user input into file paths: the fix is to call `Path.GetFullPath` on the resolved path and verify it starts with the allowed base directory before opening the file, rejecting anything that escapes the root.

---

## Q14. How do you set cache-control headers for static assets?

**Concepts**
- StaticFileOptions.OnPrepareResponse for per-file headers
- Cache-Control: public, max-age for fingerprinted assets
- no-cache for index.html after deployments
- CDN edge caching preferred for global scale

**Answer**

Use `StaticFileOptions.OnPrepareResponse` to append `Cache-Control`, `ETag`, and `Expires` headers per file response, or configure caching at the reverse proxy or CDN layer for production static assets. In `OnPrepareResponse`, set `ctx.Context.Response.Headers.CacheControl = "public,max-age=31536000,immutable"` for fingerprinted assets and `no-cache` for `index.html` so deployments propagate quickly. Kestrel static files also support conditional requests via `Last-Modified` and `ETag` automatically for cache validation. CDN edge caching is preferred for global scale, but Kestrel headers still matter for direct-host scenarios.

---

## Q15. Why should hashed JS/CSS files be cached aggressively but `index.html` should not?

**Concepts**
- Fingerprinted asset URLs changing on every build
- max-age=31536000 immutable for hashed bundles
- Cache-Control: no-cache for index.html to pick up new bundle names
- Build tools — Vite, Webpack, Angular CLI producing hashed outputs

**Answer**

Fingerprinted assets such as `main.a1b2c3.js` have unique URLs that change on every build, since the hash is derived from file content — so a long cache lifetime is safe because a new deployment uses entirely new file names and old cached files are never served for new code paths. `index.html` references those hashed names and must be fetched fresh after each deployment so browsers load updated bundle references. Caching `index.html` aggressively causes clients to load stale bundle references after deployments, breaking the app. Set `Cache-Control: no-cache` on `index.html` so browsers revalidate on each visit, while hashed assets get `max-age=31536000, immutable`.

---

## Q16. What is the difference between serving files from `wwwroot` vs a custom folder?

**Concepts**
- wwwroot published automatically with dotnet publish
- Custom folder requiring PhysicalFileProvider and RequestPath
- Publish/copy rules needed for custom roots
- Custom roots for user-generated content with stronger validation

**Answer**

`wwwroot` is the conventional, publish-included web root served by default — files deploy automatically with `dotnet publish` and no extra configuration. A custom folder requires explicit `StaticFileOptions` with a `PhysicalFileProvider` pointing to the target directory and an optional `RequestPath` prefix mapping a URL segment to that directory. Custom folders also need publish or copy rules to ensure they are available on the server after deployment. Custom roots are useful for user-generated content, but they require stronger path validation and access control since their contents are not as tightly controlled as a build-time `wwwroot`.

---

## Q17. Can static files be served without placing them in `wwwroot`?

**Concepts**
- PhysicalFileProvider with StaticFileOptions for arbitrary directories
- Multiple UseStaticFiles calls at different URL prefixes
- Razor Class Library static assets via _content/{LibraryName}/
- Expanded attack surface from additional served roots

**Answer**

Yes — configure `UseStaticFiles` with `StaticFileOptions.FileProvider = new PhysicalFileProvider(path)` and optionally `RequestPath` to expose any directory the process can read. Multiple `UseStaticFiles` calls can serve different roots at different URL prefixes in the same app — for example uploads at `/downloads` and icons at `/icons`. Files outside `wwwroot` are not published by default so the deployment pipeline must copy or generate them on the server. Razor Class Libraries can also embed static assets served via `_content/{LibraryName}/` without copying to `wwwroot`. Every additional served root expands the attack surface, so restrict to necessary directories only.

---

## Q18. What happens if sensitive files (e.g., `.env`, `appsettings.Production.json`) are placed in `wwwroot`?

**Concepts**
- Static file middleware serving without authentication
- Connection strings and API keys exposed at their URL path
- Secrets management via environment variables and Key Vault
- Source maps also exposing internal structure

**Answer**

They become anonymously downloadable at their URL path — `GET /appsettings.Production.json` returns secrets including connection strings, API keys, and credentials to any visitor, since static file middleware has no authentication gate. Source maps (`.js.map`) similarly expose the original TypeScript or source structure to attackers. Secrets belong in environment variables, Azure Key Vault, or secret managers — never in `wwwroot` or any publicly mapped folder. CI/CD pipelines should validate publish output to verify that config or `.env` files are not copied into web roots.

---

## Gotchas — Static Files & Request Pipeline (Interview Traps)

---

#### Gotcha 1. `UseStaticFiles` must come before `UseRouting` to prevent route templates shadowing static paths

**Concepts**
- `UseStaticFiles` serving files before the routing system processes the request
- Route template potentially matching static file paths as API endpoints
- Static files served without authentication — bypasses auth middleware
- Recommended order: `UseStaticFiles` → `UseRouting` → `UseAuthentication`

**Answer**

Registering `UseStaticFiles` before `UseRouting` ensures static file requests short-circuit the pipeline and are served directly without going through the routing and action selection process. If `UseRouting` runs first, a route template like `{**path}` or a controller route might match the static file URL and try to dispatch it as an API request, resulting in 404 or unexpected behavior. The tradeoff is that static files served this way have no authentication check — `UseStaticFiles` has no auth gate. Files that need access control must not be placed in `wwwroot` and must be served through a controller action that applies `[Authorize]`.

---

#### Gotcha 2. Every file in `wwwroot` is publicly accessible — no authentication gate on static file serving

**Concepts**
- `UseStaticFiles()` serving all `wwwroot` content to unauthenticated clients
- No access control on static file requests by default
- Sensitive files accidentally placed in `wwwroot` exposed to everyone
- `appsettings.Production.json` in `wwwroot` a critical security incident

**Answer**

`UseStaticFiles()` serves every file under `wwwroot` to any HTTP client with a direct path request, with no authentication or authorization check. Placing configuration files, `.env` files, private keys, or `appsettings.Production.json` copies in `wwwroot` exposes them over HTTP to anyone who guesses or scans the path. Only truly public assets — CSS, JavaScript, images, fonts, and public PDFs — belong there. Sensitive configuration belongs outside the web root and is loaded via `IConfiguration` backed by environment variables or a secrets manager. CI/CD pipelines should validate publish output to verify that no config or credential files are copied into the web root before deploy.

---

#### Gotcha 3. `UseDefaultFiles` must be registered before `UseStaticFiles` — order matters for index file serving

**Concepts**
- `UseDefaultFiles()` rewriting the request URL before static file serving
- Registration before `UseStaticFiles` required — URL rewriting must precede file lookup
- Default files: `default.htm`, `default.html`, `index.htm`, `index.html`
- `UseFileServer()` as a convenience combining both

**Answer**

`UseDefaultFiles()` rewrites the request URL — it turns a request for `/` into a request for `/index.html` — but it does not serve the file itself. `UseStaticFiles()` performs the actual serving. `UseDefaultFiles` must be registered before `UseStaticFiles` so the URL rewriting occurs before the static file middleware looks up the file. If the order is reversed, `UseStaticFiles` receives the original URL for `/`, finds no file matching `/`, and returns 404. `UseFileServer()` is a convenience method that registers both in the correct order as well as directory browsing (disabled by default).

---

#### Gotcha 4. Directory browsing is disabled by default — enabling it accidentally exposes directory listings

**Concepts**
- `UseDirectoryBrowser()` enabling listing of directory contents
- Security risk of exposing file structure to clients
- `StaticFileOptions.ServeUnknownFileTypes` and directory browsing as separate features
- Enabling directory browsing in development vs production difference

**Answer**

ASP.NET Core disables directory browsing by default — requesting a directory URL returns 403 Forbidden without listing contents. `UseDirectoryBrowser()` enables it and can be applied to specific paths with `DirectoryBrowserOptions.RequestPath`. Enabling it globally exposes the complete structure of `wwwroot` to anyone, which is a security risk in production because it reveals all file names, subdirectory structures, and occasionally hints at application architecture. If directory browsing is genuinely needed for an internal tool, apply it to a specific path and protect it with authentication. Never call `UseDirectoryBrowser()` in production without careful thought about information disclosure.

---

#### Gotcha 5. `Cache-Control` headers are not set by default — browsers may cache or not cache unpredictably

**Concepts**
- Default static file serving with no `Cache-Control` response header
- Browser applying heuristic caching when no explicit `Cache-Control` is sent
- `StaticFileOptions.OnPrepareResponse` for setting cache headers per file
- Content-based cache-busting with versioned file names or query strings

**Answer**

`UseStaticFiles()` does not set `Cache-Control` headers by default, leaving browsers to apply heuristic caching rules — typically caching based on the file's last-modified date with an unpredictable TTL. Without explicit `Cache-Control: max-age=31536000, immutable` for versioned assets and `Cache-Control: no-cache` for files that should always be fresh, some browsers aggressively cache while others don't. Configure `StaticFileOptions.OnPrepareResponse` to set explicit headers: long max-age for cache-busted assets with content hashes in filenames, short or no-cache for files that change without a URL change.

---

#### Gotcha 6. Unknown MIME types return 404 — new file extensions need explicit MIME type registration

**Concepts**
- `UseStaticFiles` serving only recognized MIME types by default
- `StaticFileOptions.ServeUnknownFileTypes = true` to serve all file types
- `FileExtensionContentTypeProvider.Mappings` for adding specific extensions
- Security risk of enabling `ServeUnknownFileTypes` broadly

**Answer**

By default, `UseStaticFiles()` only serves files with MIME types it recognizes — files with unknown extensions return 404. This affects custom file formats, `.webmanifest` files, `.wasm` binaries, font formats like `.woff2`, and other relatively new types. The correct fix is to register specific MIME types: `var provider = new FileExtensionContentTypeProvider(); provider.Mappings[".webmanifest"] = "application/manifest+json"; app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = provider })`. Setting `ServeUnknownFileTypes = true` is a broad override that serves all files with `application/octet-stream` and is a security risk because it may serve executable or configuration files without intended restrictions.

---

#### Gotcha 7. Static files are case-sensitive on Linux — path casing must match file system exactly

**Concepts**
- Linux file system case sensitivity vs Windows insensitivity
- `GET /images/logo.PNG` returning 404 when file is `logo.png` on Linux
- Docker container deployments exposing Windows-only casing bugs
- Standardizing all static asset names to lowercase

**Answer**

On Windows, `/images/Logo.png` and `/images/logo.png` serve the same file. On Linux — including most Docker containers — they are distinct paths, and `GET /images/logo.png` returns 404 when the file is named `Logo.png`. Applications developed on Windows and deployed to Linux containers discover this only in staging or production. Standardize all static file names to lowercase and ensure all `<img src="">`, `<link href="">`, and `<script src="">` references match the lowercase file names exactly. Add a build-time file naming check or linting rule to catch case mismatches before deployment.

---

#### Gotcha 8. `MapFallbackToFile` intercepts all unmatched paths including API routes — API 404s become 200 HTML

**Concepts**
- SPA fallback returning `index.html` for all unmatched paths
- `MapFallbackToFile` registered after API endpoint mapping
- API 404s masked as HTTP 200 responses with HTML body
- Conditional exclusion of `/api` prefix from fallback

**Answer**

`MapFallbackToFile("index.html")` is designed for SPA hosting — it returns `index.html` for any path not matched by earlier endpoints, enabling deep-linked client routes to work. However, if registered before `MapControllers()` or API endpoint mappings, it intercepts requests to unknown API paths and returns `index.html` with status 200 instead of 404. JSON clients receive HTML, `fetch()` calls throw parse errors, and debugging is difficult because the response appears successful. Always register `MapControllers()` and all API endpoints before `MapFallbackToFile`, and optionally add a path exclusion that prevents the fallback from matching any path starting with `/api`.

---

#### Gotcha 9. Serving files outside `wwwroot` requires an explicit `PhysicalFileProvider` with a mapped path

**Concepts**
- `UseStaticFiles()` serving from `wwwroot` (web root) by default
- `PhysicalFileProvider` for serving from arbitrary directories
- Security implications of exposing arbitrary filesystem paths
- `RequestPath` prefixing URLs for non-wwwroot file serving

**Answer**

`UseStaticFiles()` without options serves only from the `wwwroot` folder (the web root configured at startup). Serving files from other project directories — reports, generated exports, or user-uploaded files stored outside `wwwroot` — requires explicitly configuring `StaticFileOptions` with a `PhysicalFileProvider`: `new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "Reports"))`. Always use a `RequestPath` prefix like `"/downloads"` to keep the URL namespace organized and avoid accidentally overlapping with API routes. Be extremely careful about path traversal — never construct the `PhysicalFileProvider` path from user input, and prefer serving user-uploaded content through a controller action with access control rather than static file serving.

---

#### Gotcha 10. Source maps (`.js.map`) in `wwwroot` expose TypeScript source to attackers

**Concepts**
- Source maps mapping minified JS back to original TypeScript source
- `wwwroot` serving `.js.map` files to any unauthenticated client
- Removing source maps from production `wwwroot` as a security measure
- CI/CD pipeline controlling which files are included in publish output

**Answer**

Source map files (`.js.map`) allow browsers to display the original TypeScript source when debugging minified JavaScript. When these files are present in `wwwroot`, they are served by `UseStaticFiles()` to anyone who requests them — exposing proprietary TypeScript source code, internal variable names, business logic structure, and architectural patterns that attackers can use for reconnaissance. Production builds should strip source maps from the publish output or serve them only to authenticated developers via a separate endpoint. Configure webpack, Vite, or the .NET build process to omit `.js.map` files from the production `wwwroot` entirely, or serve them from a separate non-public path accessible only to internal teams.

---

## Scenario-Based Questions (Karat Format)

#### Q1. (M) By default, `UseStaticFiles()` serves content from `wwwroot`. What is actually exposed to the internet if you drop `appsettings.Production.json`, a `.env` file, or source maps into `wwwroot`? How does static file serving differ from serving files from arbitrary project folders?

**Concepts**
- wwwroot as a public CDN root — anonymous access to all files
- Project files outside wwwroot not served unless explicitly mapped
- Source maps exposing original source structure
- dotnet publish including wwwroot but not content root files

**Answer**

Everything under `wwwroot` is publicly reachable at the corresponding URL path with no authentication — `GET /appsettings.Production.json` returns the file if it exists there, delivering connection strings and API keys to any visitor. Project folders outside `wwwroot` such as the content root where `appsettings.json` lives are not served unless you explicitly misconfigure an additional `PhysicalFileProvider` pointing at them, which is why the wwwroot boundary matters.

Source maps (`.js.map`) expose the original TypeScript or C# source structure — either remove them from production builds or block them at the reverse proxy. Sensitive files belong in configuration providers — environment variables, Key Vault — never in `wwwroot`. `dotnet publish` only includes intended web assets, so the first defensive measure is to verify CI does not copy config or `.env` files into publish output.

---

#### Q2. (R) Review this `Program.cs` fragment for a public-facing site. What security issues exist?

```csharp
var app = builder.Build();
app.UseDefaultFiles();
app.UseDirectoryBrowser();
app.UseStaticFiles();
app.MapControllers();
```

**Concepts**
- UseDirectoryBrowser() enabling file and folder enumeration
- No authentication on static file pipeline
- Static files before API endpoint mapping — minor pipeline concern
- Development-only conditional for directory browsing

**Answer**

`UseDirectoryBrowser()` is the critical issue — it enables folder listing for any directory without a default file, exposing file names, backup files, upload folders, and internal asset structure to anonymous users. On a public site, attackers enumerate everything in the served roots without guessing paths.

There is also no authentication on the static file pipeline, meaning any file placed in `wwwroot` or additional served roots is downloadable by anyone. The pipeline ordering — static files before `MapControllers` — is a minor concern in this minimal setup but atypical for mixed API and static hosting.

The priority fix is to remove `UseDirectoryBrowser()` in production and restrict it to Development only: `if (app.Environment.IsDevelopment()) app.UseDirectoryBrowser();`. For sensitive file downloads, serve them behind controller endpoints with authorization rather than raw static mapping. Add security headers — CSP, `X-Content-Type-Options` — via middleware for HTML and JavaScript assets.

---

#### Q3. (P) You host a React SPA with ASP.NET Core as the API and static file host. Client-side routes like `/orders/123` return 404 after refresh. How do you configure static files, default files, and SPA fallback without breaking `/api` routes?

**Concepts**
- API endpoints mapped before SPA fallback
- UseDefaultFiles() + UseStaticFiles() for root and asset paths
- MapFallbackToFile as last-resort catch-all
- Fallback before API mapping causing HTML 200 for unknown API routes

**Answer**

The 404 on refresh happens because the server has no file or endpoint for `/orders/123` — `index.html` must be returned so the client router handles it, but without intercepting legitimate API 404 responses. The fix is to map API routes first, then static files, then the fallback last.

`app.MapControllers()` or `app.MapGroup("/api")` must be registered before the fallback so `/api/*` routes reach controllers. `app.UseDefaultFiles()` then `app.UseStaticFiles()` serves `wwwroot/index.html` for `/`. `app.MapFallbackToFile("index.html")` runs only when no endpoint matched and no static file was found, so it handles deep links like `/orders/123` without intercepting API routes. In production, nginx or a CDN often serves static assets directly; the Kestrel fallback still handles the HTML file for the same-host SPA.

```csharp
app.UseRouting();
app.MapControllers(); // or MapGroup("/api")
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapFallbackToFile("index.html");
```

---

#### Q4. (R) API responses are correct but CSS/JS return 404 in Production. Review middleware order — what's wrong?

```csharp
var app = builder.Build();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseStaticFiles();
app.Run();
```

**Concepts**
- UseStaticFiles after MapControllers — endpoint routing terminates before static middleware runs
- Static files must be registered before endpoint mapping
- Auth running for all static file requests — unnecessary overhead
- Missing UseDefaultFiles for SPA index.html

**Answer**

`UseStaticFiles()` is registered after `MapControllers()` — in the modern endpoint routing pipeline, matched endpoints terminate processing early, so requests for static assets like `/css/site.css` that do not match a controller route fall through to `UseStaticFiles()`, but requests that do match a route never reach it. More critically, because `MapControllers()` also adds the endpoint execution middleware, the pipeline stops before reaching `UseStaticFiles` for any request not already handled by routing — so asset requests return 404.

The fix is to move `UseDefaultFiles()` and `UseStaticFiles()` before `MapControllers()` and after `UseRouting()`. This also resolves the unnecessary overhead of running authentication and authorization for every static file request. For a SPA, add `MapFallbackToFile("index.html")` after all other mappings.

```csharp
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();
app.MapFallbackToFile("index.html");
```

---

#### Q5. (M) A `.wasm` or custom `.dat` file downloads instead of rendering because the browser gets `application/octet-stream`. How does `StaticFileOptions` / `FileExtensionContentTypeProvider` map extensions to MIME types, and when would you use `ServeUnknownFileTypes`?

**Concepts**
- FileExtensionContentTypeProvider as default extension-to-MIME lookup
- Custom mappings via provider.Mappings[".ext"] = "mime/type"
- ServeUnknownFileTypes falling back to application/octet-stream
- Blazor WASM requiring correct application/wasm MIME type

**Answer**

`UseStaticFiles` uses `IContentTypeProvider` — defaulting to `FileExtensionContentTypeProvider` — to set the `Content-Type` header from the file extension. When an extension is not in the provider's dictionary, the middleware returns 404 by default. Setting the wrong or absent type causes browsers to prompt a download rather than executing or rendering the file, which is the `.wasm` symptom.

The fix is to extend the provider before registering static files: `provider.Mappings[".wasm"] = "application/wasm"` and pass it via `StaticFileOptions`. Blazor WebAssembly requires this — the browser refuses to compile WASM served with `application/octet-stream`. `ServeUnknownFileTypes = true` makes the middleware serve unmapped extensions as `application/octet-stream` rather than 404, but this is risky on public sites since it would also serve `.config` or `.bak` files silently. Prefer explicit mappings for every extension the app serves.

```csharp
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".wasm"] = "application/wasm";
app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = provider });
```

---

#### Q6. (R) Review this custom endpoint that serves user-uploaded files from disk. What attack vector exists?

```csharp
app.MapGet("/files/{*path}", (string path) =>
{
    var fullPath = Path.Combine("/uploads", path);
    return Results.File(fullPath);
});
```

**Concepts**
- Path traversal via ../ sequences in catch-all route parameter
- Path.GetFullPath + prefix check as the validation pattern
- No authentication on file access
- Opaque file IDs rather than user-supplied names in URLs

**Answer**

The `{*path}` catch-all allows path traversal sequences — an attacker requests `/files/../../etc/passwd` or `/files/../../appsettings.json` and `Path.Combine("/uploads", path)` resolves to a path outside the uploads directory. On Windows, alternate data stream syntax and backslash variants add more traversal vectors.

The fix starts with path canonicalization: resolve the full path with `Path.GetFullPath(Path.Combine(uploadRoot, path))` and verify the result starts with `uploadRoot` using a case-insensitive prefix check before opening the file. Any path that escapes the root returns 400 immediately. The endpoint also has no authorization — any anonymous visitor can download any file if they know or guess its name, so require authentication and ownership checks. For new uploads, store files under opaque server-generated IDs rather than user-supplied names in URLs to prevent enumeration entirely.

```csharp
var fullPath = Path.GetFullPath(Path.Combine(uploadRoot, path));
if (!fullPath.StartsWith(uploadRoot, StringComparison.OrdinalIgnoreCase))
    return Results.BadRequest();
return Results.File(fullPath);
```

---

#### Q7. (P) Production static assets (hashed `main.a1b2c3.js`) should cache aggressively; `index.html` must not be cached stale after deploy. How do you set caching headers with `StaticFileOptions.OnPrepareResponse` or response headers at the reverse proxy?

**Concepts**
- Cache-Control: public, max-age=31536000, immutable for fingerprinted assets
- Cache-Control: no-cache for index.html after deployments
- OnPrepareResponse hook for per-file header logic
- CDN or nginx caching rules as the preferred production approach

**Answer**

The strategy splits on whether the file name changes on each build. Fingerprinted assets have a content-derived hash in the filename, so a new deployment uses new names — `Cache-Control: public, max-age=31536000, immutable` is safe since the old cached file is never referenced again. `index.html` references the new hashed bundle names and must be revalidated after every deployment, so `Cache-Control: no-cache` or `max-age=0` forces browsers to check with the server on each visit.

Implement this in `OnPrepareResponse` by checking the file name — `.html` extension gets `no-cache`, everything else with a `.` in the path gets the immutable cache header. For production scale, configuring caching rules at nginx or a CDN is more performant since caching decisions happen at the edge. Kestrel `OnPrepareResponse` headers still matter for direct-host scenarios and for `ETag`/`Last-Modified` conditional request support on non-immutable assets.

```csharp
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        var headers = ctx.Context.Response.Headers;
        if (ctx.File.Name.EndsWith(".html"))
            headers.CacheControl = "no-cache";
        else if (ctx.Context.Request.Path.Value?.Contains('.') == true)
            headers.CacheControl = "public,max-age=31536000,immutable";
    }
});
```
