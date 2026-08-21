# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `07. ASP.Net Core Web API/07. API Versioning`

---

#### Q1. (D) Compare URL path vs header vs query versioning for breaking v2 rollout with 18-month v1 support.

**Answer:** URL path versioning is the most discoverable and gateway-friendly; header versioning keeps URLs clean but is invisible to caches and harder to test in a browser; query versioning is easy to add but breaks CDN cache keys and is easy for clients to omit accidentally.

| Strategy | Pros | Cons |
|---|---|---|
| **URL** (`/api/v2/products`) | Obvious in logs, proxies, OpenAPI; easy route rules | URL churn; duplicate route templates |
| **Header** (`X-Api-Version: 2.0`) | Clean URLs; same route for all versions | Hidden contract; CDN/proxy must forward header; poor browser testing |
| **Query** (`?api-version=2.0`) | Simple to add without route changes | Cache poisoning if CDN ignores query; easy to forget parameter |

- For public payments APIs with long v1 sunset, **URL + explicit default** is common — gateways route `/v1` vs `/v2` independently.
- Combine with `ReportApiVersions` response header listing supported versions.
- Document breaking changes per version in separate OpenAPI documents — not one merged doc.
- Deprecation: `Sunset` header + link to v2 migration guide regardless of strategy chosen.

**Production takeaway:** Versioning strategy is an ops and caching decision, not just routing — pick what your gateway, CDN, and SDK pipeline can enforce.

---

#### Q2. (P) URL versioning — `/api/products` returns 404; product owner expected default v1.

**Answer:** With `AssumeDefaultVersionWhenUnspecified = false`, clients must include the version segment (`/api/v1/products`) — the route template requires `{version:apiVersion}` and unmatched routes 404.

- Set `AssumeDefaultVersionWhenUnspecified = true` if unversioned calls should map to `DefaultApiVersion` (1.0).
- Alternatively add a parallel legacy route without version segment redirecting or mapping to v1 — explicit product decision.
- Enable `ReportApiVersions = true` so responses include `api-supported-versions` header — clients discover available versions.
- Register `AddApiVersioning()` before controllers; use `AddMvc().AddApiExplorer()` for Swagger integration.

```csharp
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true; // unversioned → v1
    options.ReportApiVersions = true;
});
```

**Production takeaway:** Default version behavior is contractual — `false` means "no version = 404," not "use latest."

---

#### Q3. (R) Header versioning — client sends `X-Api-Version: 2.0` but receives v1 payload.

**Answer:** When multiple controller classes share the same route template, the API version reader must disambiguate actions — missing `[MapToApiVersion]` on actions, duplicate route attributes, or incorrect controller selection causes the wrong version to execute despite the header.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Versioning | Both controllers on `api/[controller]` without action-level mapping | Ambiguous selection — v1 may win |
| ApiExplorer | Version not tied to action metadata | Wrong controller activated |
| Testing | Header not sent in some integration tests | False confidence in v2 behavior |

**Fix (priority order):**

1. Use single controller with versioned actions: `[MapToApiVersion("1.0")]` / `[MapToApiVersion("2.0")]`, or separate routes per version.
2. Confirm `HeaderApiVersionReader("X-Api-Version")` matches client header name exactly (case-insensitive value).
3. Add tests asserting v2 header returns v2 DTO shape; log `IApiVersioningFeature` in diagnostics.

**Production takeaway:** Header versioning fails silently when duplicate routes exist — structure controllers so version selection is unambiguous.

---

#### Q4. (P) Breaking vs additive changes — `name` vs `fullName` on customer GET.

**Answer:** Removing `name` or changing its meaning is a **breaking** change requiring a new major API version; adding `fullName` alongside `name` is **additive** and safe in the same version if old clients ignore unknown fields.

| Change type | Example | Client impact |
|---|---|---|
| **Breaking** | Remove `name`; rename to `fullName` only | Deserialization/parsing fails or data loss |
| **Breaking** | Change `id` from int to string | All typed clients break |
| **Additive** | Add optional `fullName` while keeping `name` | Old clients continue working |
| **Additive** | New optional query parameter | Old requests unchanged |

- Ship breaking changes only in v2; keep v1 returning `name` until sunset date.
- OpenAPI: separate documents per version — v1 schema keeps `name`; v2 documents migration.
- Release notes: explicit field mapping table (`v1.name` → `v2.fullName`).
- Consider response compatibility layer in v2 during transition if dual fields needed temporarily.

**Production takeaway:** JSON field renames are breaking — version bump or dual-field sunset period, never silent rename in place.

---

#### Q5. (P) Deprecation headers — review proposed middleware vs RFC-aligned signaling.

**Answer:** Custom `X-Deprecated: true` without a sunset date gives clients no migration deadline — use standardized `Deprecation`, `Sunset`, and `Link` headers (and matching OpenAPI `deprecated: true`) so gateways and SDKs can automate warnings.

- **`Deprecation`:** `true` or HTTP-date when deprecated (RFC 9745 semantics evolving — check current spec).
- **`Sunset`:** HTTP-date after which the endpoint may be removed (e.g., `Sunset: Sat, 01 Feb 2027 00:00:00 GMT`).
- **`Link`:** `<https://docs.example.com/api/v2/migration>; rel="successor-version"`.
- Mark operations `deprecated: true` in v1 OpenAPI document.
- Log deprecated endpoint usage metrics — drive partner outreach before removal.
- Do not remove v1 until sunset date passes and usage threshold met.

```csharp
context.Response.Headers["Deprecation"] = "true";
context.Response.Headers["Sunset"] = sunsetDate.ToString("R");
context.Response.Headers["Link"] = "</api/v2/customers/{id}>; rel=\"successor-version\"";
```

**Production takeaway:** Deprecation is a contract commitment — headers must include *when* and *where to migrate*, not a boolean flag alone.

---

#### Q6. (M) API version in OpenAPI — separate documents, `DocInclusionPredicate`, shared DTO names.

**Answer:** `Asp.Versioning.Mvc.ApiExplorer` exposes group names per version (e.g., `v1`, `v2`) — `ConfigureSwaggerOptions` implements `IConfigureOptions<SwaggerGenOptions>` and sets `DocInclusionPredicate` so each action appears only in its version's Swagger document.

- Register: `AddApiVersioning().AddApiExplorer(options => options.GroupNameFormat = "'v'VVV")`.
- `SwaggerDoc("v1", ...)` and `SwaggerDoc("v2", ...)` match explorer group names.
- `DocInclusionPredicate`: `(docName, apiDesc) => apiDesc.GroupName == docName`.
- Shared DTO short names across versions collide in OpenAPI — use `CustomSchemaIds(type => type.FullName)` or versioned namespaces (`CustomerResponseV1`, `CustomerResponseV2`).
- Swagger UI lists one dropdown entry per document — clients generate separate SDKs per version.

**Production takeaway:** Versioning without per-version OpenAPI produces unusable combined schemas — wire ApiExplorer group names to Swagger doc names explicitly.

---

#### Q7. (R) Query-string versioning behind CDN — cached v1 served to v2 clients.

**Answer:** CDNs cache by URL path by default — `GET /api/items?api-version=2.0` and `?api-version=1.0` may map to the same cache key if the CDN ignores query strings, returning stale v1 JSON to v2 clients.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Caching | CDN does not vary on `api-version` query | Wrong version body served from cache |
| Cache-Control | `public, max-age=300` on versioned responses | Amplifies cross-version pollution |
| Versioning | Query param invisible to path-based cache rules | Intermittent client bugs |

**Fix (priority order):**

1. Configure CDN to include `api-version` in cache key or disable caching for versioned endpoints.
2. Prefer URL path versioning (`/api/v2/items`) — path is always part of cache key.
3. Send `Vary: api-version` (limited CDN support) or `Cache-Control: private` for authenticated APIs.
4. Use `Cache-Control: no-store` during migration testing.

**Production takeaway:** Query-string versioning and shared URLs are hostile to edge caching — URL versioning or strict cache key rules are required.

---

#### Q8. (D) `AssumeDefaultVersionWhenUnspecified = true` with `DefaultApiVersion = 2.0` while v1 partners remain.

**Answer:** Implicit default to latest version breaks legacy partners who omit version headers — they silently receive v2 breaking payloads without opting in, causing production incidents while v1 is still deployed for others.

| Risk | Description |
|---|---|
| Silent upgrade | Clients without version info jump to v2 breaking JSON |
| Support burden | "We didn't change our code" incidents |
| Testing gap | Integrators test against default; prod default shifts on deploy |
| Observability | Hard to distinguish intentional v2 adoption vs accidental default |

- Keep `DefaultApiVersion = 1.0` until v1 sunset; set default to 2.0 only after explicit migration window closes.
- Require explicit version (URL segment or header) for public APIs — `AssumeDefaultVersionWhenUnspecified = false` forces intentional choice.
- Acceptable implicit default only for internal services with controlled client fleet and simultaneous releases.
- Monitor `api-version` usage metrics before changing default.

**Production takeaway:** Default version is the behavior for the laziest client — never default to breaking major without a published sunset and partner acknowledgment.
