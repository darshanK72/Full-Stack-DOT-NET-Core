# Static Files & Request Pipeline — Interview Q&A
> Back to [README](../README.md)

## Table of Contents

- [Chapter 11. Static Files & Request Pipeline](#chapter-11-static-files-request-pipeline)
  - [Q1. What is the purpose of the `wwwroot` folder?](#chapter-11-static-files-request-pipeline-q1)
  - [Q2. What does `UseStaticFiles()` do?](#chapter-11-static-files-request-pipeline-q2)
  - [Q3. What is the difference between `UseDefaultFiles()` and `UseS…](#chapter-11-static-files-request-pipeline-q3)
  - [Q4. What security risk does `UseDirectoryBrowser()` pose?](#chapter-11-static-files-request-pipeline-q4)
  - [Q5. Where should static file middleware be placed in the pipelin…](#chapter-11-static-files-request-pipeline-q5)
  - [Q6. How do you serve a Single Page Application (SPA) with ASP.NE…](#chapter-11-static-files-request-pipeline-q6)
  - [Q7. What is SPA fallback routing, and why is it needed?](#chapter-11-static-files-request-pipeline-q7)
  - [Q8. How do you prevent SPA fallback from intercepting API routes…](#chapter-11-static-files-request-pipeline-q8)
  - [Q9. What are MIME types, and how does ASP.NET Core determine the…](#chapter-11-static-files-request-pipeline-q9)
  - [Q10. What is `StaticFileOptions`?](#chapter-11-static-files-request-pipeline-q10)
  - [Q11. What is `FileExtensionContentTypeProvider`?](#chapter-11-static-files-request-pipeline-q11)
  - [Q12. What does `ServeUnknownFileTypes` do, and when is it risky?](#chapter-11-static-files-request-pipeline-q12)
  - [Q13. What is a path traversal attack in the context of file servi…](#chapter-11-static-files-request-pipeline-q13)
  - [Q14. How do you set cache-control headers for static assets?](#chapter-11-static-files-request-pipeline-q14)
  - [Q15. Why should hashed JS/CSS files be cached aggressively but `i…](#chapter-11-static-files-request-pipeline-q15)
  - [Q16. What is the difference between serving files from `wwwroot` …](#chapter-11-static-files-request-pipeline-q16)
  - [Q17. Can static files be served without placing them in `wwwroot`…](#chapter-11-static-files-request-pipeline-q17)
  - [Q18. What happens if sensitive files (e.g., `.env`, `appsettings.…](#chapter-11-static-files-request-pipeline-q18)
- [Gotchas](#gotchas)
- [Scenario-Based Questions](#scenario-based-questions-karat-format)

---

## Chapter 11. Static Files & Request Pipeline

### Q1. What is the purpose of the `wwwroot` folder? {#chapter-11-static-files-request-pipeline-q1}

What is the purpose of the `wwwroot` folder?

**Answer:** `wwwroot` is the default web root directory whose files are served as public static content — HTML, CSS, JavaScript, images, and fonts — directly over HTTP without hitting controller logic.

- It maps to `IWebHostEnvironment.WebRootPath` and is the default root for `UseStaticFiles()`.
- Files in `wwwroot` are included in publish output and deployed to the server as-is.
- Treat everything under `wwwroot` as publicly accessible to anonymous clients — no authentication gate by default.
- SPA builds (React, Angular, Blazor WASM) typically place `index.html` and bundled assets here.

---

### Q2. What does `UseStaticFiles()` do? {#chapter-11-static-files-request-pipeline-q2}

What does `UseStaticFiles()` do?

**Answer:** `UseStaticFiles()` registers middleware that intercepts HTTP requests, maps the URL path to a file under the configured web root, and returns the file with the correct content type and caching headers if the file exists.

- If no matching file is found, the request passes to the next middleware (routing, endpoints).
- Default configuration serves from `wwwroot` with standard MIME type detection via `FileExtensionContentTypeProvider`.
- Customize behavior through `StaticFileOptions` — custom roots, content types, response headers, or request-path prefixes.
- Static file middleware does not execute application code — it is optimized for direct file serving from disk.

---

### Q3. What is the difference between `UseDefaultFiles()` and `UseStaticFiles()`? {#chapter-11-static-files-request-pipeline-q3}

What is the difference between `UseDefaultFiles()` and `UseStaticFiles()`?

**Answer:** `UseDefaultFiles()` rewrites directory requests (e.g., `/` or `/docs/`) to a default file name like `index.html` without serving content itself; `UseStaticFiles()` actually reads and returns the file bytes.

- Call `UseDefaultFiles()` **before** `UseStaticFiles()` so the rewrite happens before the file lookup.
- Default file names include `index.html`, `index.htm`, `default.html` — configurable via `DefaultFilesOptions`.
- `UseDefaultFiles()` alone does not serve files — it only changes `Path` on the request for the next middleware.
- Together they enable `GET /` to return `wwwroot/index.html` without exposing `/index.html` in the URL.

---

### Q4. What security risk does `UseDirectoryBrowser()` pose? {#chapter-11-static-files-request-pipeline-q4}

What security risk does `UseDirectoryBrowser()` pose?

**Answer:** `UseDirectoryBrowser()` enables HTML directory listings, exposing file and folder names to anyone who can reach the URL — a serious information disclosure vulnerability on public sites.

- Attackers enumerate backup files, source maps, upload folders, and forgotten assets without guessing paths.
- It is acceptable only in controlled Development environments — never enable on Production public hosts.
- Prefer explicit file serving via `UseStaticFiles()` with known file names; hide directory structure entirely.
- Combine with misconfigured upload directories and it accelerates discovery of sensitive or executable files.

---

### Q5. Where should static file middleware be placed in the pipeline? {#chapter-11-static-files-request-pipeline-q5}

Where should static file middleware be placed in the pipeline?

**Answer:** Place `UseDefaultFiles()` and `UseStaticFiles()` after routing is established but before endpoint execution — typically after `UseRouting()` and alongside or before `UseAuthentication()`, depending on whether static files need auth (usually they do not).

- Map API endpoints (`MapControllers`, `MapGroup`) before SPA fallback so `/api/*` routes are not swallowed by static handling.
- Common order: exception handler → forwarded headers → HTTPS → routing → auth → endpoints → default files → static files → SPA fallback.
- Static middleware short-circuits when a file matches — order relative to routing affects whether endpoint or file wins.
- For SPAs, fallback to `index.html` must come **after** API endpoint mapping.

---

### Q6. How do you serve a Single Page Application (SPA) with ASP.NET Core? {#chapter-11-static-files-request-pipeline-q6}

How do you serve a Single Page Application (SPA) with ASP.NET Core?

**Answer:** Publish the SPA build output to `wwwroot`, serve static assets with `UseStaticFiles()`, and add a fallback route that returns `index.html` for client-side routes not matched by API endpoints or static files.

- Build the SPA (npm/vite/webpack) and copy `dist/` contents into `wwwroot`.
- Use `UseDefaultFiles()` + `UseStaticFiles()` for root and asset paths.
- Add `app.MapFallbackToFile("index.html")` after API routes so deep links like `/orders/123` work on refresh.
- In Production, many teams serve static assets from a CDN while Kestrel handles API and fallback only.

---

### Q7. What is SPA fallback routing, and why is it needed? {#chapter-11-static-files-request-pipeline-q7}

What is SPA fallback routing, and why is it needed?

**Answer:** SPA fallback routing returns `index.html` for URLs that do not match a physical file or server endpoint, allowing the client-side router to handle navigation for deep links and browser refreshes.

- Without fallback, refreshing `/dashboard/settings` returns 404 because no server file exists at that path.
- Fallback runs only when no endpoint matched and no static file was found — it is a last-resort catch-all.
- The client router (React Router, Vue Router) reads the URL and renders the correct view after `index.html` loads.
- Fallback must not intercept valid API 404 responses unless intentionally unified — order API mapping first.

---

### Q8. How do you prevent SPA fallback from intercepting API routes? {#chapter-11-static-files-request-pipeline-q8}

How do you prevent SPA fallback from intercepting API routes?

**Answer:** Register all API endpoints before `MapFallbackToFile`, or scope fallback to non-API paths using conditional mapping such as `MapFallbackToFile("index.html").Add(builder => !builder.Request.Path.StartsWithSegments("/api"))`.

- `MapControllers()` or `MapGroup("/api")` must be registered before the fallback delegate runs.
- If fallback is registered first, `/api/unknown` returns `index.html` with HTTP 200 — breaking API clients silently.
- Use path prefixes consistently — `/api` for backend, everything else for SPA.
- Test with `curl` against unknown API paths to confirm JSON 404, not HTML 200.

---

### Q9. What are MIME types, and how does ASP.NET Core determine them for static files? {#chapter-11-static-files-request-pipeline-q9}

What are MIME types, and how does ASP.NET Core determine them for static files?

**Answer:** MIME types (Content-Type values) tell browsers how to interpret file bytes; ASP.NET Core maps file extensions to MIME types via `FileExtensionContentTypeProvider` when serving static files.

- `.html` → `text/html`, `.css` → `text/css`, `.js` → `application/javascript`, `.png` → `image/png`.
- Wrong MIME types cause browsers to block scripts (CSP) or misrender downloads instead of displaying pages.
- Customize mappings in `StaticFileOptions.ContentTypeProvider` for uncommon extensions (`.wasm`, `.webp`, custom fonts).
- `ServeUnknownFileTypes` falls back to `application/octet-stream` when enabled — use cautiously.

---

### Q10. What is `StaticFileOptions`? {#chapter-11-static-files-request-pipeline-q10}

What is `StaticFileOptions`?

**Answer:** `StaticFileOptions` configures static file middleware behavior — file provider root, request path prefix, content type mappings, default content type, response headers, and whether unknown file types are served.

- Pass to `app.UseStaticFiles(new StaticFileOptions { … })` or configure via `IOptions<StaticFileOptions>`.
- `FileProvider` + `RequestPath` serve files from a non-`wwwroot` folder at a URL subpath (e.g., `/images`).
- `OnPrepareResponse` adds cache-control, security headers, or auth checks per file response.
- Enables serving uploaded content from a controlled directory without exposing the entire project tree.

---

### Q11. What is `FileExtensionContentTypeProvider`? {#chapter-11-static-files-request-pipeline-q11}

What is `FileExtensionContentTypeProvider`?

**Answer:** `FileExtensionContentTypeProvider` is a dictionary-backed lookup that maps file extensions (e.g., `.json`, `.svg`) to IANA MIME type strings for the `Content-Type` response header.

- Default provider covers common web extensions; extend with `provider.Mappings[".myext"] = "application/x-custom"`.
- Used internally by static files middleware and available for manual content-type resolution in downloads.
- Missing mappings cause 404 or `application/octet-stream` depending on `ServeUnknownFileTypes` setting.
- Keep mappings explicit for security-sensitive types — do not serve `.config` or `.cs` with guessed types.

---

### Q12. What does `ServeUnknownFileTypes` do, and when is it risky? {#chapter-11-static-files-request-pipeline-q12}

What does `ServeUnknownFileTypes` do, and when is it risky?

**Answer:** When `ServeUnknownFileTypes` is `true`, static files middleware serves files whose extension has no MIME mapping using `DefaultContentType` (typically `application/octet-stream`) instead of returning 404.

- Risky on public sites — unknown extensions like `.config`, `.bak`, or `.env` may become downloadable if placed in the served root.
- Browsers may sniff content and execute HTML/JS served with wrong types, enabling XSS in edge cases.
- Prefer explicitly registering needed extensions in `ContentTypeProvider` rather than allowing all unknown types.
- Default is `false` — unknown extensions are not served, which is the safer Production default.

---

### Q13. What is a path traversal attack in the context of file serving? {#chapter-11-static-files-request-pipeline-q13}

What is a path traversal attack in the context of file serving?

**Answer:** Path traversal exploits insufficient path sanitization so attackers request URLs containing `../` sequences to read files outside the intended directory — such as `GET /files/../../appsettings.json`.

- Occurs when custom file-serving code concatenates user input into file paths without normalization and boundary checks.
- Built-in `UseStaticFiles()` normalizes paths and restricts lookups to the configured `FileProvider` root — generally safe when not misconfigured.
- Custom download endpoints must call `Path.GetFullPath` and verify the resolved path starts with the allowed base directory.
- Never serve files based on raw user-supplied paths without validation.

---

### Q14. How do you set cache-control headers for static assets? {#chapter-11-static-files-request-pipeline-q14}

How do you set cache-control headers for static assets?

**Answer:** Use `StaticFileOptions.OnPrepareResponse` to append `Cache-Control`, `ETag`, and `Expires` headers, or configure caching at the reverse proxy/CDN layer for Production static assets.

- Example: in `OnPrepareResponse`, set `ctx.Context.Response.Headers.CacheControl = "public,max-age=31536000,immutable"` for fingerprinted assets.
- Kestrel static files support conditional requests via `Last-Modified` and `ETag` automatically for cache validation.
- For `index.html`, set `no-cache` or short `max-age` so deployments propagate quickly to clients.
- CDN edge caching is preferred for global scale; Kestrel headers still matter for direct-host scenarios.

---

### Q15. Why should hashed JS/CSS files be cached aggressively but `index.html` should not? {#chapter-11-static-files-request-pipeline-q15}

Why should hashed JS/CSS files be cached aggressively but `index.html` should not?

**Answer:** Fingerprinted assets (e.g., `main.a1b2c3.js`) have unique URLs that change on every build — long cache lifetimes are safe because a new deployment uses new file names; `index.html` references those names and must be fetched fresh to pick up new bundles.

- Aggressive caching (`max-age=31536000, immutable`) for hashed files maximizes CDN and browser performance.
- Caching `index.html` aggressively causes clients to load stale bundle references after deployments, breaking the app.
- Set `Cache-Control: no-cache` on `index.html` so browsers revalidate and get updated script references.
- This cache-busting strategy is standard for Vite, Webpack, and Angular CLI production builds.

---

### Q16. What is the difference between serving files from `wwwroot` vs a custom folder? {#chapter-11-static-files-request-pipeline-q16}

What is the difference between serving files from `wwwroot` vs a custom folder?

**Answer:** `wwwroot` is the conventional, publish-included web root served by default; a custom folder requires explicit `StaticFileOptions` with a `PhysicalFileProvider` and optional `RequestPath` prefix.

- `wwwroot` files deploy automatically with `dotnet publish` — no extra configuration.
- Custom folders (e.g., `Uploads/`, `/var/app/assets`) need explicit middleware registration and correct publish/copy rules.
- `RequestPath` maps a URL prefix (`/downloads`) to a physical directory outside `wwwroot`.
- Custom roots are useful for user-generated content but require stronger path validation and access control.

---

### Q17. Can static files be served without placing them in `wwwroot`? {#chapter-11-static-files-request-pipeline-q17}

Can static files be served without placing them in `wwwroot`?

**Answer:** Yes — configure `UseStaticFiles` with `StaticFileOptions.FileProvider = new PhysicalFileProvider(path)` and optionally `RequestPath` to expose any directory the process can read.

- Multiple `UseStaticFiles` calls can serve different roots at different URL prefixes in the same app.
- Files outside `wwwroot` are not published by default — ensure deployment copies or generates them on the server.
- Razor Class Libraries can embed static assets served via `_content/{LibraryName}/` without copying to `wwwroot`.
- Every additional served root expands the attack surface — restrict to necessary directories only.

---

### Q18. What happens if sensitive files (e.g., `.env`, `appsettings.Production.json`) are placed in `wwwroot`? {#chapter-11-static-files-request-pipeline-q18}

What happens if sensitive files (e.g., `.env`, `appsettings.Production.json`) are placed in `wwwroot`?

**Answer:** They become anonymously downloadable at their URL path — `GET /appsettings.Production.json` returns secrets including connection strings, API keys, and credentials to any visitor.

- Static file middleware has no authentication — everything in the served root is public.
- Secrets belong in environment variables, Azure Key Vault, or secret managers — never in `wwwroot` or any publicly mapped folder.
- CI/CD should validate publish output does not copy config or `.env` files into web roots.
- Source maps (`.js.map`) in `wwwroot` similarly expose original source structure to attackers.

---

---

## Gotchas — ASP.NET Core (Interview Traps)

#### Gotcha 1. Middleware order — routing before auth

**Answer:** In ASP.NET Core 8 endpoint routing, `UseRouting` must run before `UseAuthentication` and `UseAuthorization` so the auth middleware can inspect endpoint metadata — registering auth before routing breaks endpoint-aware authorization and policy resolution.

- The recommended order is exception handling → forwarded headers → routing → authentication → authorization → endpoints (`MapControllers` / `MapGet`).
- When auth runs before routing, the endpoint has not been selected yet and `[Authorize]` metadata on minimal routes or controllers may not apply correctly.
- Symptoms include anonymous access to protected endpoints or 401 responses without proper challenge behavior.
- Always verify middleware order in `Program.cs` during code review for new services.

---

#### Gotcha 2. Scoped service in a Singleton

**Answer:** Registering a scoped service such as `DbContext` into a singleton creates a captive dependency that lives for the application lifetime while the scoped instance is disposed after its first scope ends, causing stale data, thread-safety bugs, or `ObjectDisposedException`.

- The singleton holds one scoped instance forever instead of one per request — EF change trackers accumulate unrelated entities.
- Enable `ValidateScopes` in Development/staging to catch illegal scope combinations at startup.
- Fix by injecting `IServiceScopeFactory` or `IDbContextFactory<T>` and creating a scope per operation.
- This applies equally to singleton services, hosted services, and cached delegates in Minimal APIs.

---

#### Gotcha 3. `new HttpClient()` in a singleton

**Answer:** Instantiating `HttpClient` with `new` inside a long-lived singleton prevents socket reuse and causes socket exhaustion under load because each instance holds its own connection pool until garbage-collected.

- `HttpClient` is disposable but not meant for per-use disposal — `using var client = new HttpClient()` in a singleton is an anti-pattern.
- `IHttpClientFactory` manages `HttpMessageHandler` lifetimes and recycles connections correctly.
- Register named or typed clients: `builder.Services.AddHttpClient<IExternalApi, ExternalApiClient>();`
- Symptoms include `SocketException` and timeout errors only under production traffic, not in local testing.

---

#### Gotcha 4. `IOptions<T>` vs reload

**Answer:** `IOptions<T>` captures configuration snapshot at first resolution — reading `.Value` once in a singleton constructor freezes settings even when `appsettings.json` reloads with `ReloadOnChange` enabled.

- `IOptionsSnapshot<T>` recalculates per request scope; `IOptionsMonitor<T>` supports change notifications via `OnChange`.
- Singleton services must use `IOptionsMonitor<T>` or read options inside scoped operations if they need live updates.
- Misconfiguration persists silently until process restart when `.Value` was cached at construction.
- See Chapter 05 for the full options lifetime comparison.

---

#### Gotcha 5. GET with `[FromBody]`

**Answer:** Using `[FromBody]` on GET action parameters or minimal API handlers is an anti-pattern because HTTP GET semantics discourage bodies, and many clients, proxies, and caches strip or ignore GET request bodies, so binding fails silently in production.

- Query strings and route values are the correct binding sources for GET requests.
- Complex filters should use `[FromQuery]` with `[AsParameters]` or flattened query keys.
- Failures often appear only in specific browsers or CDN layers, not in Swagger "Try it out" during development.
- REST conventions expect GET to be safe and idempotent with parameters in the URL.

---

#### Gotcha 6. PascalCase JSON keys with default camelCase policy

**Answer:** ASP.NET Core 8 Web API serializes JSON with camelCase property names by default via `JsonNamingPolicy.CamelCase`, so incoming JSON with PascalCase keys (for example `"CustomerName"`) may not bind to `CustomerName` unless case-insensitive matching is enabled.

- Mobile or legacy clients sending PascalCase appear to succeed but properties remain default values (empty string, zero).
- Prefer standardizing clients on camelCase and documenting the contract in OpenAPI.
- Optional mitigation: `AddJsonOptions(o => o.JsonSerializerOptions.PropertyNameCaseInsensitive = true)` — but explicit camelCase contracts are cleaner.
- Add validation attributes so silent binding failures become 400 responses instead of corrupt data.

---

#### Gotcha 7. `throw ex` vs `throw`

**Answer:** Rethrowing with `throw ex` resets the stack trace to the catch block line, hiding the original failure location in logs and diagnostics, while bare `throw` preserves the full stack trace from where the exception was first thrown.

- Exception filters, middleware, and Application Insights rely on accurate stack traces for root-cause analysis.
- Always use `throw;` when rethrowing after logging or cleanup in a catch block.
- Wrap in a new exception only when adding context: `throw new OrderProcessingException("...", ex)` to preserve `InnerException`.
- This trap appears in both application code and background worker error handlers.

---

#### Gotcha 8. Kestrel as the only production layer

**Answer:** Running Kestrel exposed directly to the internet without a reverse proxy skips TLS termination at the edge, centralized rate limiting, WAF protection, and efficient static-file caching that production deployments typically require.

- Kestrel is production-grade as an application server but is not a full edge gateway — nginx, IIS, Azure Front Door, or AWS ALB commonly sit in front.
- TLS certificates are easier to manage at the proxy layer with automatic renewal.
- Direct exposure also complicates client IP logging unless `UseForwardedHeaders` is configured with a trusted proxy.
- Containers often bind Kestrel to port 8080 internally while the ingress controller handles HTTPS externally.

---

#### Gotcha 9. `launchSettings.json` in production

**Answer:** Settings in `Properties/launchSettings.json` — including `applicationUrl`, environment variables, and launch profiles — apply only when starting from Visual Studio, VS Code, or `dotnet run` with a profile; they are not deployed to production hosts.

- Production URLs and environment come from environment variables (`ASPNETCORE_URLS`, `ASPNETCORE_ENVIRONMENT`), container configuration, or IIS/nginx site settings.
- Assuming `launchSettings.json` sets Production behavior leads to wrong environment or binding in deployed environments.
- The file is development ergonomics, not runtime configuration.
- Use `appsettings.Production.json` and host-level env vars for production values.

---

#### Gotcha 10. Non-nullable `bool` for PATCH semantics

**Answer:** A non-nullable `bool` property cannot distinguish "field omitted from JSON" from "explicitly set to false" because System.Text.Json deserializes missing properties to `default(false)`, corrupting partial-update semantics.

- PATCH endpoints need `bool?`, separate update DTOs, or enums such as `Unspecified | OptIn | OptOut` for tri-state intent.
- Marketing consent and feature flags are common domains where this bug causes compliance or logic errors.
- Create DTOs may use non-nullable bool when explicit values are always required on insert.
- Document nullable fields in OpenAPI so generated clients represent optional updates correctly.

---

#### Gotcha 11. Forgetting `UseForwardedHeaders` behind a proxy

**Answer:** Without forwarded headers middleware configured with known proxy IPs, `HttpContext.Request.Scheme` remains `http`, `Request.Host` reflects the internal address, and client IP is the proxy — breaking HTTPS redirects, cookie secure flags, and audit logs.

- Call `UseForwardedHeaders()` early, before middleware that reads scheme or host (HTTPS redirection, link generation, rate limiting by IP).
- Configure `ForwardedHeadersOptions` to trust only your reverse proxy network — trusting all proxies enables header spoofing.
- Headers include `X-Forwarded-For`, `X-Forwarded-Proto`, and `X-Forwarded-Host`.
- Local development without a proxy does not need this; production behind nginx/IIS/ALB does.

---

#### Gotcha 12. Static files in `wwwroot` are public

**Answer:** Any file under `wwwroot` is served by `UseStaticFiles()` to unauthenticated clients by default — placing secrets, `.env`, backup configs, or private keys there exposes them over HTTP.

- Only public assets (CSS, JS, images, public PDFs) belong in `wwwroot`.
- Sensitive configuration stays outside the web root and is loaded through `IConfiguration`, environment variables, or secret managers.
- Accidental copy of `appsettings.Production.json` into `wwwroot` is a critical security incident.
- Use build pipelines to verify web root contents before deploy.

---

#### Gotcha 13. `MapFallbackToFile` intercepting API routes

**Answer:** SPA fallback middleware registered before API endpoint mapping returns `index.html` for `/api/*` 404 responses, making API failures look like successful HTML responses to clients and breaking JSON parsers.

- Map API routes (`MapControllers`, minimal API groups) before `MapFallbackToFile("index.html")`.
- Scope fallback to non-API paths or use conditional fallback that excludes `/api` prefixes.
- Symptoms include CORS errors masked as HTML responses and Swagger fetch failures in production SPA hosting.
- Order in `Program.cs` is: API endpoints first, static files, fallback last.

---

#### Gotcha 14. Background service without scope factory

**Answer:** A singleton `BackgroundService` that injects scoped services (`DbContext`, repositories) directly into its constructor fails at startup with scope validation errors or uses disposed instances after the first background iteration.

- Hosted services live for the application lifetime — scoped dependencies must not be constructor-injected.
- Inject `IServiceScopeFactory`, create `await using var scope = factory.CreateAsyncScope()` per job, resolve scoped services inside the scope, and dispose when the job completes.
- Same rule applies to timers and `Task.Run` loops started from singletons.
- Enable `ValidateScopes` to catch this defect before production deployment.

---

#### Gotcha 15. SignalR without a backplane on multiple instances

**Answer:** SignalR broadcasts from one server instance reach only clients connected to that instance — without a Redis or Azure Service Bus backplane (or Azure SignalR Service), users on different nodes never receive each other's real-time events.

- Sticky sessions keep one client on one node but do not route events raised on other nodes to that client.
- Register `AddSignalR().AddStackExchangeRedis(...)` with a consistent channel prefix per application.
- Raw WebSocket apps need equivalent custom pub/sub — SignalR's backplane is the built-in solution.
- Test scale-out with at least two instances before launch, not single-node staging alone.

---

---

## Gotchas — ASP.NET Core (Interview Traps)

## Gotchas — ASP.NET Core (Interview Traps)

---

## Scenario-Based Questions (Karat Format)

#### Q1. (M) By default, `UseStaticFiles()` serves content from `wwwroot`. What is actually exposed to the internet if you drop `appsettings.Production.json`, a `.env` file, or source maps into `wwwroot`? How does static file serving differ from serving files from arbitrary project folders?

---

**Answer:**

**Answer:** Everything under `wwwroot` is publicly reachable at the corresponding URL path with no authentication — secrets, source maps, and backup configs become directly downloadable; only files explicitly published to `wwwroot` (or additional `StaticFileOptions.FileProvider` roots) are served this way.

- `UseStaticFiles()` maps HTTP requests to files under `IWebHostEnvironment.WebRootPath` (default `wwwroot`) — `GET /appsettings.Production.json` returns the file if it exists there.
- Project folders outside `wwwroot` (e.g., `appsettings.json` at content root) are **not** served unless you misconfigure an additional `PhysicalFileProvider` pointing at them.
- Source maps (`.js.map`) expose original TypeScript/C# source structure — remove from Production builds or block at the proxy.
- Sensitive files belong in configuration providers (env vars, Key Vault), never in `wwwroot`.
- `dotnet publish` only includes intended web assets — verify CI does not copy secrets into publish output.

**Production takeaway:** Treat `wwwroot` as a public CDN root — if you would not attach it to an anonymous S3 bucket, do not put it there.

---

---

#### Q2. (R) Review this `Program.cs` fragment for a public-facing site. What security issues exist?

```csharp
var app = builder.Build();
app.UseDefaultFiles();
app.UseDirectoryBrowser();
app.UseStaticFiles();
app.MapControllers();
```

---

**Answer:**

```csharp
var app = builder.Build();
app.UseDefaultFiles();
app.UseDirectoryBrowser();
app.UseStaticFiles();
app.MapControllers();
```

**Answer:** `UseDirectoryBrowser()` enables folder listing for any directory without a default file — exposing file names, backup files, and internal asset structure to anonymous users on a public site.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | `UseDirectoryBrowser()` on public site | Attackers enumerate `/js`, `/uploads`, hidden files |
| Security | No authentication on static pipeline | Any file in served roots is anonymously downloadable |
| Pipeline | Missing `UseRouting` before controllers (minor in minimal setup) | Less critical here but atypical for mixed API + static |
| Design | Default files without restricting served directories | Unexpected `index.html` from subfolders may become entry points |

**Fix (priority order):**

1. **Remove `UseDirectoryBrowser()`** in Production — restrict to Development only if ever needed: `if (app.Environment.IsDevelopment()) app.UseDirectoryBrowser();`
2. Serve only required roots; use separate file providers with `RequestPath` for isolated asset folders.
3. Place auth-sensitive downloads behind controller endpoints with authorization, not raw static mapping.
4. Add security headers (CSP, `X-Content-Type-Options`) via middleware for HTML/JS assets.

**Production takeaway:** Directory browsing is a development convenience that becomes an enumeration vulnerability the moment it ships publicly.

---

---

#### Q3. (P) You host a React SPA with ASP.NET Core as the API and static file host. Client-side routes like `/orders/123` return 404 after refresh. How do you configure static files, default files, and SPA fallback without breaking `/api` routes?

---

**Answer:**

**Answer:** Register API routes first, then static files with `UseDefaultFiles` + `UseStaticFiles`, then a fallback route that serves `index.html` for non-file, non-API paths so client-side routing handles deep links.

- Map API separately: `app.MapControllers()` or `app.MapGroup("/api")…` before SPA fallback.
- `app.UseDefaultFiles()` then `app.UseStaticFiles()` serves `wwwroot/index.html` for `/`.
- Fallback for client routes: `app.MapFallbackToFile("index.html")` (.NET 6+) — only runs when no endpoint matched and no static file found.
- Do **not** fallback before API mapping — `/api/orders` must hit controllers, not `index.html`.
- In Production, nginx/CDN often serves static assets; Kestrel fallback still needed if the same host serves SPA + API.

```csharp
app.UseRouting();
app.MapControllers(); // or MapGroup("/api")
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapFallbackToFile("index.html");
```

**Production takeaway:** SPA 404-on-refresh is a routing-order problem — API endpoints must win over the SPA catch-all.

---

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

---

**Answer:**

```csharp
var app = builder.Build();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseStaticFiles();
app.Run();
```

**Answer:** `UseStaticFiles()` is registered **after** `MapControllers()` — in the modern pipeline, mapped endpoints terminate routing early and static file middleware never runs for asset requests that do not match controller routes, so `/css/site.css` falls through to 404.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Pipeline order | `UseStaticFiles` after endpoint mapping | Static assets not served; broken UI in Production |
| Pipeline | Missing `UseDefaultFiles` before static | `/` may not serve `index.html` for SPA |
| Design | Auth middleware runs for static file requests unnecessarily | Extra overhead; may block anonymous CSS if misconfigured |

**Fix (priority order):**

1. Move `UseDefaultFiles()` and `UseStaticFiles()` **before** `MapControllers()` / after `UseRouting()` (typical pattern).
2. Optionally use `app.MapWhen` or separate branches for `/api` vs static if pipelines diverge.
3. For SPA, add `MapFallbackToFile` after static files and API maps.

```csharp
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();
app.MapFallbackToFile("index.html");
```

**Production takeaway:** Middleware order is not cosmetic — static files must run in the branch that executes before unmatched requests die.

---

---

#### Q5. (M) A `.wasm` or custom `.dat` file downloads instead of rendering because the browser gets `application/octet-stream`. How does `StaticFileOptions` / `FileExtensionContentTypeProvider` map extensions to MIME types, and when would you use `ServeUnknownFileTypes`?

---

**Answer:**

**Answer:** `StaticFileMiddleware` uses `IContentTypeProvider` (default `FileExtensionContentTypeProvider`) to set `Content-Type` from the file extension; unknown extensions fall back to `application/octet-stream`, causing browsers to download instead of execute/render.

- Customize mappings: `provider.Mappings[".wasm"] = "application/wasm";` and pass via `StaticFileOptions { ContentTypeProvider = provider }`.
- Blazor/WebAssembly requires correct WASM MIME types — missing mapping breaks client-side apps silently or triggers download.
- `ServeUnknownFileTypes = true` serves files with unknown extensions but still needs an explicit `DefaultContentType` — use sparingly; prefer explicit mappings for security (avoid serving executable types).
- Alternative: configure MIME types at nginx/IIS for performance — Kestrel mappings must still be correct when Kestrel serves directly.

```csharp
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".wasm"] = "application/wasm";
app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = provider });
```

**Production takeaway:** Wrong MIME types look like "works locally with dev server" but break when ASP.NET Core or a new proxy serves the files.

---

---

#### Q6. (R) Review this custom endpoint that serves user-uploaded files from disk. What attack vector exists?

```csharp
app.MapGet("/files/{*path}", (string path) =>
{
    var fullPath = Path.Combine("/uploads", path);
    return Results.File(fullPath);
});
```

---

**Answer:**

```csharp
app.MapGet("/files/{*path}", (string path) =>
{
    var fullPath = Path.Combine("/uploads", path);
    return Results.File(fullPath);
});
```

**Answer:** The `{*path}` catch-all allows path traversal sequences (`../`) — an attacker can request `/files/../../etc/passwd` or `..\..\appsettings.json` and escape the intended upload directory.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | Unvalidated `path` concatenated to base directory | Path traversal — read arbitrary server files |
| Security | No authentication on sensitive uploads | Any anonymous user downloads any stored file if name guessed |
| Design | `Path.Combine` with absolute base + attacker segments | `../` may resolve outside `/uploads` depending on OS normalization |

**Fix (priority order):**

1. Normalize and validate: resolve full path and ensure it starts with the upload root (`Path.GetFullPath` + prefix check).
2. Reject paths containing `..` or alternate data streams; use generated opaque file ids instead of user-supplied names in URLs.
3. Require authorization; serve via controller with ownership checks, not open static mapping.
4. Set `Content-Disposition` and correct content type; log access.

```csharp
var fullPath = Path.GetFullPath(Path.Combine(uploadRoot, path));
if (!fullPath.StartsWith(uploadRoot, StringComparison.OrdinalIgnoreCase))
    return Results.BadRequest();
return Results.File(fullPath);
```

**Production takeaway:** Never serve filesystem paths from user input without canonicalization — path traversal is a standard penetration-test finding.

---

---

#### Q7. (P) Production static assets (hashed `main.a1b2c3.js`) should cache aggressively; `index.html` must not be cached stale after deploy. How do you set caching headers with `StaticFileOptions.OnPrepareResponse` or response headers at the reverse proxy?



**Answer:**

**Answer:** Set long `Cache-Control: public, max-age=31536000, immutable` for fingerprinted assets and `no-cache` or short TTL for `index.html` — implement in `OnPrepareResponse` by file extension/name or at nginx/CDN for better performance.

- **Hashed assets** (`*.a1b2c3.js`, `*.css`): `Cache-Control: public, max-age=31536000, immutable` — safe because filename changes on content change.
- **`index.html` / entry HTML:** `Cache-Control: no-cache` or `max-age=0` — browsers must revalidate after deploy so new hashed bundles load.
- Kestrel hook:

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

- Prefer CDN/nginx caching rules for scale; use `ETag`/`Last-Modified` for conditional requests on non-immutable assets.
- Version query strings (`?v=`) without hashed filenames are weaker — filename hashing is the robust pattern.

**Production takeaway:** Aggressive caching on `index.html` causes post-deploy "stale app" incidents that are fixed only by hard refresh — split cache policy by asset type.

---
