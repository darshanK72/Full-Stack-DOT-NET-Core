# Karat Interview Answer Examples

Folder-scoped samples. Headers show **folder path**, not a global section name.

---

## Example — Type (R) in async folder

**Folder:** `02. C# Language Fundamentals/06. Multithreading & Async Programming/`

#### Q1. (R) Review this controller action. What are the problems (compile-time, runtime, and scalability)?

```csharp
public IActionResult GetData()
{
    var data = _service.GetAsync().Result;
    return Ok(data);
}
```

**Answer:** This action blocks a thread by waiting synchronously on asynchronous work through `.Result`, which defeats ASP.NET Core's async I/O model and can cause thread-pool starvation or deadlocks under load.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Async | `.Result` on unfinished Task | Blocks thread pool; sync-over-async |
| API design | Non-async action signature | Cannot propagate cancellation; poor composability |
| Scalability | Sync wait per request | Reduced throughput on I/O-bound endpoints |

**Fix (priority order):**

1. Change to `public async Task<IActionResult> GetData(CancellationToken ct)` and `await _service.GetAsync(ct)`.
2. Ensure the service method is truly async end-to-end.

**Production takeaway:** Passes locally with low concurrency; fails in production when the thread pool saturates — a common Karat trap in web actions.

---

## Example — Type (P) in hosting folder

**Folder:** `05. ASP.NET Core/12. Hosting, Kestrel & Environments/`

#### Q2. (P) An API works locally over HTTPS but emits `http://` links and logs the proxy IP after deploy behind nginx. What is missing in `Program.cs`, and why does middleware order matter?

**Answer:** Forwarded headers must be applied early with trusted proxy configuration so Kestrel sees the original client scheme and IP from `X-Forwarded-*` headers instead of the immediate connection to nginx.

- Configure `ForwardedHeadersOptions` with `XForwardedFor | XForwardedProto` and restrict `KnownProxies` / `KnownNetworks` in production.
- Call `app.UseForwardedHeaders()` before HTTPS redirection, authentication, and URL generation.
- Without this, `Request.Scheme` stays `http` and `RemoteIpAddress` is the proxy — breaking redirects, cookies, and audit trails.

**Production takeaway:** Local HTTPS hides the gap; reverse-proxy deployments expose it immediately.

---

## Example — Type (D) in DI folder

**Folder:** `05. ASP.NET Core/04. Dependency Injection & Service Lifetimes/`

#### Q3. (D) A developer stores each user's shopping cart in an instance field on a **Singleton** service. What breaks, and what pattern replaces it?

**Answer:** A singleton shares one instance for all users and requests, so a single cart field mixes every user's items — a functional bug that also breaks thread-safe updates under concurrent requests.

- Register cart state as **scoped** (per request) or persist per user in a store keyed by user id.
- Keep singleton services stateless; pass user identity into scoped services that load cart data.
- In multi-instance deployments, in-memory singleton carts are not shared across pods anyway.

**Production takeaway:** Karat uses this to test DI lifetimes tied to user state — not abstract "singleton vs scoped" definitions.
