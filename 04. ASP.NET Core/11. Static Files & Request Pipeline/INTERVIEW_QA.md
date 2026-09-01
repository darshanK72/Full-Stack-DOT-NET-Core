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

## Gotchas — ASP.NET Core (Interview Traps)

#### Gotcha 1. Middleware order — routing before auth

**Concepts**
- UseRouting must precede UseAuthentication and UseAuthorization
- Endpoint metadata not selected before routing runs
- Recommended pipeline order for ASP.NET Core 8

**Answer**

In ASP.NET Core endpoint routing, `UseAuthentication` and `UseAuthorization` must run after `UseRouting` so the auth middleware can read endpoint metadata — if auth runs before routing, the endpoint has not been selected yet and policy resolution for `[Authorize]` and `RequireAuthorization()` cannot inspect the correct attributes. The recommended order is exception handling → forwarded headers → routing → authentication → authorization → endpoints. Symptoms of wrong order include anonymous access to protected endpoints and 401 challenges that fire without correctly applying per-endpoint allow-anonymous overrides.

---

#### Gotcha 2. Scoped service in a Singleton

**Concepts**
- Captive dependency lifetime violation
- EF DbContext stale change tracker accumulation
- ValidateScopes detecting the problem at startup
- IServiceScopeFactory as the correct fix

**Answer**

A scoped service injected into a singleton is held for the entire application lifetime, long after the scope that created it was disposed. The most common case is `DbContext`: the change tracker accumulates entities from unrelated requests, and after the scope is torn down any access throws `ObjectDisposedException`. Enable `ValidateScopes = true` in Development and staging to catch these combinations at startup rather than under production load. The fix is to inject `IServiceScopeFactory` and create a scope per unit of work, or use `IDbContextFactory<T>` to get a short-lived context per operation.

---

#### Gotcha 3. `new HttpClient()` in a singleton

**Concepts**
- HttpMessageHandler lifetime and socket exhaustion
- IHttpClientFactory managed handler recycling
- Named and typed client registration pattern

**Answer**

Instantiating `HttpClient` with `new` in a long-lived singleton prevents socket reuse because each instance holds its own `HttpMessageHandler` and the underlying TCP connections are not returned to a pool until garbage collection. Under load this causes socket exhaustion — `SocketException` and timeout errors that do not appear in local testing with low concurrency. `IHttpClientFactory` manages handler lifetimes and recycles connections correctly, so the fix is to register named or typed clients via `builder.Services.AddHttpClient<IExternalApi, ExternalApiClient>()` and inject them rather than constructing `HttpClient` directly.

---

#### Gotcha 4. `IOptions<T>` vs reload

**Concepts**
- IOptions<T> frozen snapshot at first resolution
- IOptionsSnapshot<T> recalculates per request scope
- IOptionsMonitor<T> live change notifications for singletons
- Silent staleness until process restart

**Answer**

`IOptions<T>` resolves once and caches the configuration snapshot for the service's lifetime, so a singleton that reads `.Value` in its constructor freezes settings even when `appsettings.json` reloads with `ReloadOnChange` enabled. `IOptionsSnapshot<T>` recalculates per request scope but is only usable in scoped services. `IOptionsMonitor<T>` supports change notifications via `OnChange` and works correctly in singletons. The failure mode is silent — misconfiguration persists until process restart because `.Value` was captured at construction.

---

#### Gotcha 5. GET with `[FromBody]`

**Concepts**
- HTTP GET semantics and safe/idempotent URL parameters
- Proxies and caches stripping GET request bodies
- [FromQuery] with [AsParameters] for complex filter criteria
- Silent failures in CDN and proxy layers

**Answer**

`[FromBody]` on a GET endpoint is an anti-pattern because HTTP GET is defined as safe and idempotent with parameters in the URL — many clients, CDNs, and caching proxies strip or ignore request bodies on GET requests, so binding fails silently in production while "Try it out" in Swagger may appear to work. Use `[FromQuery]` with separate parameter names or `[AsParameters]` on a record type to aggregate complex filter criteria into a single clean parameter object.

---

#### Gotcha 6. PascalCase JSON keys with default camelCase policy

**Concepts**
- JsonNamingPolicy.CamelCase as ASP.NET Core default
- Silent binding producing default values instead of errors
- PropertyNameCaseInsensitive as a mitigation
- Validation attributes turning silent failure into 400 responses

**Answer**

ASP.NET Core Web API serializes JSON with `JsonNamingPolicy.CamelCase` by default, which means incoming JSON with PascalCase keys like `"CustomerName"` does not match the property — the model binds successfully but properties silently hold default values (null, zero, false). The preferred fix is standardizing all clients on camelCase and enforcing it through OpenAPI contracts. As a mitigation, `AddJsonOptions(o => o.JsonSerializerOptions.PropertyNameCaseInsensitive = true)` relaxes matching. Add required validation attributes so silent binding failures produce 400 responses rather than corrupt data silently stored to the database.

---

#### Gotcha 7. `throw ex` vs `throw`

**Concepts**
- throw; preserving original stack trace
- throw ex; resetting stack trace to the catch site
- InnerException preservation when intentionally wrapping
- APM and structured logging dependency on accurate stack traces

**Answer**

Rethrowing with `throw ex` resets the stack trace to the catch block line, which means Application Insights, Serilog, and `IExceptionHandler` all point at the handler rather than the code that actually failed. Bare `throw;` preserves the full original stack trace. Use `throw;` when logging and delegating upward; wrap with a new exception type only when adding context — `throw new OrderProcessingException("...", ex)` — so the original failure is preserved in `InnerException`. This rule applies identically in async code after `await`.

---

#### Gotcha 8. Kestrel as the only production layer

**Concepts**
- Kestrel as application server vs edge gateway
- TLS termination and certificate management at the reverse proxy
- WAF, rate limiting, and static file caching at the edge
- UseForwardedHeaders required for client IP logging

**Answer**

Kestrel is a production-grade application server optimized for running .NET efficiently, but directly exposing it to the internet skips TLS certificate centralization, WAF filtering, centralized rate limiting, and efficient static-file caching that reverse proxies handle. nginx, IIS, Azure Front Door, or AWS ALB typically sit in front so certificates are managed at the proxy layer with automatic renewal. If Kestrel is exposed directly, client IP logging requires `UseForwardedHeaders` configuration, and containers typically bind Kestrel to an internal port while the ingress controller handles external HTTPS.

---

#### Gotcha 9. `launchSettings.json` in production

**Concepts**
- launchSettings.json applies only to dotnet run and IDE launch
- ASPNETCORE_URLS and ASPNETCORE_ENVIRONMENT as production env vars
- appsettings.Production.json for non-secret production tuning

**Answer**

`Properties/launchSettings.json` contains URLs, environment variables, and launch profiles that are read only by `dotnet run`, Visual Studio, and VS Code — the file is not deployed to production hosts and has no effect on them. Relying on it for environment name or URL configuration leads to wrong `ASPNETCORE_ENVIRONMENT` or binding address in deployed environments. Production URLs and environment come from host-level environment variables (`ASPNETCORE_URLS`, `ASPNETCORE_ENVIRONMENT`), container configuration, or IIS/nginx site settings.

---

#### Gotcha 10. Non-nullable `bool` for PATCH semantics

**Concepts**
- default(false) for missing JSON field
- Nullable bool? for tri-state intent
- PATCH semantics requiring omitted-vs-false distinction
- Update DTO design for partial updates

**Answer**

A non-nullable `bool` property in a PATCH DTO cannot distinguish "field omitted from JSON" from "explicitly set to false" because `System.Text.Json` deserializes missing properties to `default(false)`, which corrupts partial-update semantics — a client updating only an email address accidentally resets a consent flag to false. PATCH endpoints need `bool?`, separate update DTOs that only include fields being modified, or tri-state enums like `Unspecified | OptIn | OptOut` to represent intent explicitly. Document nullable fields in OpenAPI so generated clients represent optional updates correctly.

---

#### Gotcha 11. Forgetting `UseForwardedHeaders` behind a proxy

**Concepts**
- X-Forwarded-For, X-Forwarded-Proto, X-Forwarded-Host headers
- ForwardedHeadersOptions.KnownProxies for trusted network restriction
- Pipeline position — must run before HTTPS redirection and auth
- Header spoofing risk when trusting all proxies

**Answer**

Without `UseForwardedHeaders()` configured with known proxy IPs, `HttpContext.Request.Scheme` stays `http` even when clients used HTTPS, `Request.Host` reflects the internal address, and the client IP is the proxy — breaking HTTPS redirects, secure cookie flags, and audit logs. Call `UseForwardedHeaders()` as early as possible, before HTTPS redirection, authentication, link generation, and rate limiting by IP. Configure `ForwardedHeadersOptions` to trust only your specific reverse proxy network rather than all proxies, since trusting all enables header spoofing by any client.

---

#### Gotcha 12. Static files in `wwwroot` are public

**Concepts**
- UseStaticFiles() serving without authentication
- wwwroot as a public CDN root
- Secrets management via environment variables and Key Vault
- Build pipeline verification of publish output

**Answer**

Every file in `wwwroot` is served to unauthenticated anonymous clients by `UseStaticFiles()` — there is no authentication gate by default. Placing `.env` files, `appsettings.Production.json`, private keys, or backup configs there makes them directly downloadable via their URL path. Only public assets such as CSS, JavaScript, images, and public PDFs belong in `wwwroot`. Sensitive configuration must live in environment variables, Azure Key Vault, or similar secret managers, and build pipelines should verify that publish output does not include secrets in the web root.

---

#### Gotcha 13. `MapFallbackToFile` intercepting API routes

**Concepts**
- SPA fallback order relative to API endpoint mapping
- /api/* returning index.html with HTTP 200 as a silent failure
- Endpoint-first ordering in Program.cs

**Answer**

Registering `MapFallbackToFile("index.html")` before API endpoint mapping causes any unmatched API route — including valid 404s — to return `index.html` with HTTP 200, which breaks JSON parsers on clients and masks the real failure. The correct order is to map API routes with `MapControllers()` or `MapGroup("/api")` first, then static files, then the SPA fallback last. Symptoms include CORS errors appearing as HTML responses and Swagger fetch failures in production SPA hosting.

---

#### Gotcha 14. Background service without scope factory

**Concepts**
- BackgroundService singleton lifetime
- Scoped service constructor injection causing disposal errors
- IServiceScopeFactory.CreateAsyncScope() per background job
- ValidateScopes detecting this at startup

**Answer**

A singleton `BackgroundService` cannot constructor-inject scoped services like `DbContext` because hosted services live for the application lifetime while scoped instances are disposed after their first scope ends, causing `ObjectDisposedException` or scope validation errors at startup. The fix is to inject `IServiceScopeFactory`, then inside each background job call `await using var scope = factory.CreateAsyncScope()`, resolve the scoped service from `scope.ServiceProvider`, and dispose the scope when the job finishes. Enable `ValidateScopes` in Development to catch this before production deployment.

---

#### Gotcha 15. SignalR without a backplane on multiple instances

**Concepts**
- SignalR broadcast scope — single server instance only
- Redis or Azure Service Bus backplane for multi-instance routing
- Sticky sessions vs backplane trade-offs
- Azure SignalR Service as a managed alternative

**Answer**

SignalR tracks connected clients per server instance, so a broadcast from one instance reaches only the clients connected to that instance. With multiple instances behind a load balancer, users on different nodes never receive events raised on other nodes — a critical failure for real-time chat or notifications. Sticky sessions keep one client on one node but do not route server-side events across nodes. The solution is a Redis or Azure Service Bus backplane registered with `AddSignalR().AddStackExchangeRedis(...)`, or the managed Azure SignalR Service. Test scale-out with at least two instances before launch.

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
