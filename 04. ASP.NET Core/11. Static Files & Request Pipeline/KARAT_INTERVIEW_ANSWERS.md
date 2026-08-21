# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `05. ASP.NET Core/11. Static Files & Request Pipeline`

---

#### Q1. (M) By default, `UseStaticFiles()` serves content from `wwwroot`. What is actually exposed to the internet if you drop `appsettings.Production.json`, a `.env` file, or source maps into `wwwroot`? How does static file serving differ from serving files from arbitrary project folders?

**Answer:** Everything under `wwwroot` is publicly reachable at the corresponding URL path with no authentication — secrets, source maps, and backup configs become directly downloadable; only files explicitly published to `wwwroot` (or additional `StaticFileOptions.FileProvider` roots) are served this way.

- `UseStaticFiles()` maps HTTP requests to files under `IWebHostEnvironment.WebRootPath` (default `wwwroot`) — `GET /appsettings.Production.json` returns the file if it exists there.
- Project folders outside `wwwroot` (e.g., `appsettings.json` at content root) are **not** served unless you misconfigure an additional `PhysicalFileProvider` pointing at them.
- Source maps (`.js.map`) expose original TypeScript/C# source structure — remove from Production builds or block at the proxy.
- Sensitive files belong in configuration providers (env vars, Key Vault), never in `wwwroot`.
- `dotnet publish` only includes intended web assets — verify CI does not copy secrets into publish output.

**Production takeaway:** Treat `wwwroot` as a public CDN root — if you would not attach it to an anonymous S3 bucket, do not put it there.

---

#### Q2. (R) Review this `Program.cs` fragment for a public-facing site. What security issues exist?

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

#### Q3. (P) You host a React SPA with ASP.NET Core as the API and static file host. Client-side routes like `/orders/123` return 404 after refresh. How do you configure static files, default files, and SPA fallback without breaking `/api` routes?

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

#### Q5. (M) A `.wasm` or custom `.dat` file downloads instead of rendering because the browser gets `application/octet-stream`. How does `StaticFileOptions` / `FileExtensionContentTypeProvider` map extensions to MIME types, and when would you use `ServeUnknownFileTypes`?

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

#### Q6. (R) Review this custom endpoint that serves user-uploaded files from disk. What attack vector exists?

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

#### Q7. (P) Production static assets (hashed `main.a1b2c3.js`) should cache aggressively; `index.html` must not be cached stale after deploy. How do you set caching headers with `StaticFileOptions.OnPrepareResponse` or response headers at the reverse proxy?

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
