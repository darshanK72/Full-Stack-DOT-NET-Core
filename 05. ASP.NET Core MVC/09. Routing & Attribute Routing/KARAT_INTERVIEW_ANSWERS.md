# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `06. ASP.NET Core MVC/09. Routing & Attribute Routing`

---

#### Q1. (R) Review this `ProductsController` and `Program.cs`. `GET /products/sale` shows a product named "sale" instead of the sale landing page.

```csharp
app.MapControllerRoute(
    name: "productSlug",
    pattern: "products/{id}",
    defaults: new { controller = "Products", action = "Details" });
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

public class ProductsController : Controller
{
    public IActionResult Index() => View(_repo.All());
    public IActionResult Details(string id) => View(_repo.BySlug(id));
    public IActionResult Sale() => View(_repo.SaleItems());
}
```

**Answer:** The dedicated `products/{id}` route is registered **before** the default route and maps `/products/sale` to `Details(id: "sale")` — the literal segment `sale` is a slug, not the `Sale` action. Conventional `/products/sale` would only reach `Sale()` via the default `{controller}/{action}` template, but the slug route wins first.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Route template ambiguity | Slug/`{id}` pattern vs literal action name `Sale` | Same path segment means product slug or dedicated action |
| Conventional routing | `{controller}/{action}/{id?}` encourages `/products/details/sale` for slugs | Marketing URL `/products/sale` collides with slug routing |
| API contract | No disambiguation (literal prefix vs parameter) | Wrong view/data — silent functional bug |

**Fix (priority order):**

1. Separate namespaces: `[HttpGet("sale")]` or `[Route("products/sale")]` on `Sale`, and `[HttpGet("products/{slug}")]` on `Details` — literal segments beat bare parameters in attribute routing.
2. For conventional-only apps: rename slug action route via dedicated pattern in `Program.cs`: `products/{slug}` → `Details`, register **before** default; keep `products/sale` → `Sale` via explicit `MapControllerRoute` with literal `sale`.
3. Add routing integration tests for `/products/sale`, `/products/details/sale`, and `/products/sale-items`.

**Production takeaway:** MVC slug URLs and marketing literals at the same path level need explicit route templates — see [05. ASP.NET Core/07. Routing & Endpoints](../../05.%20ASP.NET%20Core/07.%20Routing%20&%20Endpoints/KARAT_INTERVIEW_ANSWERS.md) Q4 for attribute-route specificity; the same rule applies to conventional + slug patterns.

---

#### Q2. (M) A team registers two conventional routes. Explain optional `{id?}` and defaults for `/catalog`, `/catalog/featured`, and `/catalog/list/42`.

```csharp
app.MapControllerRoute(
    name: "catalog",
    pattern: "catalog/{action=List}/{id?}",
    defaults: new { controller = "Catalog" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

**Answer:** Route matching is **first registered, best match** among named routes — the `catalog` route handles all paths starting with `catalog/` before the default route is considered.

- **`GET /catalog`** — pattern `catalog/{action=List}/{id?}` matches with `action = List` (default), `id = null` (optional omitted) → `CatalogController.List()`.
- **`GET /catalog/featured`** — `action = featured`, `id = null` → `CatalogController.Featured()` (action name from URL segment, case-insensitive by default).
- **`GET /catalog/list/42`** — `action = list`, `id = 42` → `CatalogController.List(42)` if overload exists, or model binding supplies `id` to `List(int id)`; if only parameterless `List` exists, `42` may bind to a parameter named `id` on the action.
- **Renaming `List` without updating the pattern** — `/catalog` still defaults `action=List` in the **route defaults**, not the method name; broken rename causes 404 or wrong action unless defaults and links are updated together.
- **`MapControllerRoute` order** — register more specific routes (`catalog/...`) **before** the generic `{controller}/{action}` default.

**Production takeaway:** Optional segments and inline defaults live in the **route table**, not C# optional parameters — rename actions and update `MapControllerRoute` + all `Url.Action` call sites together.

---

#### Q3. (R) Review route constraints. `GET /orders/not-a-number` returns 500 instead of not-found.

```csharp
app.MapControllerRoute(
    name: "order",
    pattern: "orders/{id:int}",
    defaults: new { controller = "Orders", action = "Details" });

public class OrdersController : Controller
{
    public IActionResult Details(int id) => View(_orders.Get(id));
}
```

**Answer:** With `{id:int}` on the **matched route**, `orders/not-a-number` should **fail route matching** (404), not reach the action — a 500 `FormatException` means a **less constrained route matched first** (e.g. default `{controller}/{action}/{id?}` binding `id` as string `"not-a-number"` then failing int conversion), or duplicate endpoints without the constraint.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Route order | Default `{controller}/{action}/{id?}` may match `orders/not-a-number` as controller/action/id | Constraint on dedicated route never evaluated |
| Route constraint | `:int` not on the endpoint that actually matched | Model binding throws on invalid int |
| Error shape | Unhandled binding exception → 500 | Clients cannot tell bad URL from server fault |

**Fix (priority order):**

1. Register the constrained `orders/{id:int}` route **before** the default catch-all conventional route.
2. Remove conflicting patterns — do not also expose `Orders/Details/{id}` without `:int` unless you handle parse failures and return 400/404.
3. Verify with endpoint debug (Development) or `EndpointDataSource` tests that only constrained template matches `/orders/{id}`.

**Production takeaway:** Route constraints reject non-matching URLs at selection time — constraint on an unreachable route is useless if a looser route wins first.

---

#### Q4. (R) Review `Program.cs` route order. Unknown URLs return Home/Index 200; `/admin/users` never reaches Admin.

```csharp
app.MapControllerRoute("fallback", "{*path}", defaults: new { controller = "Home", action = "Index" });
app.MapControllerRoute("admin", "admin/{controller=Dashboard}/{action=Index}/{id?}");
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
```

**Answer:** `MapControllerRoute` entries are tried in **registration order** — the catch-all `{*path}` registered first swallows **every** request, including `/admin/users`, before admin or default routes run.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Route order | Catch-all first | All URLs dispatch to `Home.Index` — false 200 SEO pages |
| Catch-all misuse | SPA-style fallback without `MapFallbackToController` guard | Admin and API paths never match |
| Observability | 200 instead of 404 | Broken links invisible to monitors; crawlers index duplicate home content |

**Fix (priority order):**

1. Move `{*path}` fallback **last**, after `admin`, `areas`, and `default` routes.
2. Prefer `app.MapFallbackToController("Index", "Home")` (.NET 6+) for SPA shell — clearer intent than manual catch-all defaults.
3. Restrict fallback to non-file requests if static files are involved; return 404 for unknown API paths instead of HTML shell.

**Production takeaway:** Catch-all routes are terminal — register them last or every specific route becomes dead code.

---

#### Q5. (R) Review shared layout link in Shop area. **Manage Users** 404s from `/shop/products`.

```html
<a asp-controller="Users" asp-action="Index">Manage Users</a>
```

```csharp
app.MapControllerRoute("areas", "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

[Area("Admin")]
public class UsersController : Controller { ... }
```

**Answer:** Tag Helpers generate URLs from **ambient route values** — inside the Shop area, `asp-controller`/`asp-action` inherit `area = Shop` unless overridden, producing `/shop/users/index` instead of `/admin/users/index`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Link generation | Missing `asp-area="Admin"` | Wrong area prefix in URL — 404 |
| Ambient values | Current request context leaks into `Url.Action` | Works in root, fails under areas |
| Design | Shared layout without explicit area on cross-area links | Intermittent, context-dependent bugs |

**Fix (priority order):**

1. `<a asp-area="Admin" asp-controller="Users" asp-action="Index">Manage Users</a>`.
2. For root-only links from areas, use `asp-area=""` to clear ambient area.
3. Centralize admin URL generation in a small helper or `LinkGenerator.GetPathByAction` with explicit `values: new { area = "Admin" }`.

**Production takeaway:** Conventional link generation is context-sensitive — cross-area links must name `area` explicitly; see [07. Routing & Endpoints](../../05.%20ASP.NET%20Core/07.%20Routing%20&%20Endpoints/KARAT_INTERVIEW_ANSWERS.md) for proxy/scheme issues on generated URLs.

---

#### Q6. (M) Attribute routing at controller vs action level — final URLs and token inheritance.

```csharp
[Route("reports/[controller]")]
public class ReportsController : Controller
{
    [HttpGet("monthly")]
    public IActionResult Monthly() => View();

    [HttpGet("/export/{year:int}")]
    public IActionResult Export(int year) => File(...);
}
```

**Answer:** Controller-level `[Route]` sets a prefix for all actions; action templates append unless the action template starts with `/` (root-relative, ignores controller prefix).

- **`Monthly`** — `[HttpGet("monthly")]` appends to `reports/[controller]` → **`/reports/reports/monthly`** (`[controller]` token = `Reports`, typically lowercased if `LowercaseUrls` enabled → `/reports/reports/monthly`).
- **`Export`** — `[HttpGet("/export/{year:int}")]` is **absolute from site root** → **`/export/2024`**, not under `/reports/...`.
- **Tokens:** `[controller]`, `[action]` resolve from type/method names; only actions inherit controller `[Route]` unless overridden with `~` or leading `/`.
- **When controller-level:** shared API prefix (`api/[controller]`), area-style grouping, consistent versioning.
- **When action-only:** few routed endpoints on otherwise conventional controller, or mixed conventional views + one attribute API action.

**Production takeaway:** Leading `/` on action routes opts out of controller prefix — a common surprise when `/export` 404s under `/reports/export`.

---

#### Q7. (R) Review documentation catch-all. `GET /docs/getting-started/install` returns 404.

```csharp
[Route("docs")]
public class DocsController : Controller
{
    [HttpGet("{*slug}")]
    public IActionResult Page(string slug) { ... }
}
```

**Answer:** Combined template is `/docs/{*slug}` — catch-all `{*slug}` should capture `getting-started/install` including slashes. A 404 on nested paths usually means a **more specific route matched first without catch-all**, static files middleware serves `/docs` as a physical folder, or `slug` is null/empty because the request never reaches this action.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Static files | `UseStaticFiles` serves `/docs/getting-started/install` as file path | Short-circuits before endpoint — 404 if file missing |
| Route specificity | Another route or conventional `{controller}/{action}` captures `docs` | Wrong handler or no match |
| Catch-all binding | Missing `{*slug}` or using `{slug}` (single segment only) | Only one path segment matches |

**Fix (priority order):**

1. Ensure `{*slug}` is on the action (multi-segment catch-all requires `*`).
2. Register docs routes before default conventional route; exclude `/docs/**` from static file mapping or relocate static assets.
3. Integration test: `GET /docs/a/b` binds `slug = "a/b"`.

**Production takeaway:** Catch-all parameters include `/` in the value — `{slug}` without `*` matches one segment only; CMS and doc sites need `{*slug}`.

---

#### Q8. (M) Contrast 404 vs 405 for three requests on `{controller=Home}/{action=Index}/{id?}`.

**Answer:** Endpoint routing selects an endpoint **first** (template + HTTP method), then executes — "no endpoint" and "wrong method" are different outcomes.

1. **`GET /home/index`** (action exists, GET allowed) — **200** — endpoint matches `HomeController.Index`, GET satisfies method constraint (implicit GET on conventional actions).
2. **`POST /home/index`** (only GET implemented) — **405 Method Not Allowed** — route **matches** `HomeController.Index` endpoint, but no POST method metadata — router returns 405 with `Allow` header listing permitted verbs when configured.
3. **`GET /homme/index`** (typo) — **404 Not Found** — no endpoint matches controller `Homme` — no action selection occurs.

- **Legacy IRouter:** similar distinction but middleware ordering differed; endpoint routing centralizes method metadata on `Endpoint`.
- **404** = no matching route endpoint; **405** = endpoint found, HTTP method not supported — important for API clients and security scanners.

**Production takeaway:** Do not treat 405 as "routing bug" — it often means URL is correct but verb is wrong; 404 means fix the path or register the endpoint.

---

#### Q9. (P) Lowercase URLs enabled but redirects and links still emit PascalCase paths that 404 on Linux.

```csharp
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

return Redirect("/Account/Login?returnUrl=/Products/Details/5");
```

**Answer:** `LowercaseUrls` affects **link generation** (`Url.Action`, Tag Helpers, `LinkGenerator`) — hard-coded strings bypass routing options and remain case-sensitive on Linux/Kestrel.

- Replace string redirects with `RedirectToAction("Login", "Account", new { returnUrl = ... })` or `Redirect(Url.Action(...))` so casing policy applies.
- Audit `return Redirect("/...")`, `<a href="/Products/...">`, JavaScript paths, and email templates.
- `[Route]` templates and conventional routes are matched case-insensitively by default, but **generated URLs** should be lowercase for consistency — mismatched casing in CDN caches causes duplicate URLs.
- `LowercaseQueryStrings` only applies to generated query keys/values, not manual strings.
- Integration tests on Linux CI catch PascalCase hard-coding that works on Windows dev boxes.

**Production takeaway:** Routing options are not global URL normalization middleware — every hard-coded path is a bypass.

---

#### Q10. (D) Legacy `UseMvcWithDefaultRoute()` vs `MapControllerRoute` during .NET 8 upgrade — keep both?

```csharp
app.UseMvcWithDefaultRoute();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
```

**Answer:** Do **not** run both — `UseMvcWithDefaultRoute()` is the pre-endpoint-routing model (ASP.NET Core 2.x) and is removed/incompatible with the modern `WebApplication` pipeline; duplicate registration causes double route tables, ambiguous matches, or startup failures.

- **Modern replacement:** `builder.Services.AddControllersWithViews()` + `app.MapControllerRoute(...)` (or `MapDefaultControllerRoute()`) — endpoint routing is always on in .NET 6+.
- **Legacy model:** `UseMvc` integrated routing as terminal middleware; **endpoint routing** builds a unified `EndpointDataSource` at startup, then `EndpointMiddleware` dispatches — better for minimal APIs, authorization metadata, and endpoint metadata.
- **Migration:** remove `UseMvc`/`UseMvcWithDefaultRoute`; move route templates to `MapControllerRoute`; use `MapControllers()` for attribute-routed API controllers.
- **Action selection:** endpoint routing matches once; legacy could re-run route logic in MVC pipeline — filters and link generation now share the same endpoint graph.

**Production takeaway:** One routing system per app — endpoint routing subsumes legacy IRouter middleware; see cross-ref for minimal API groups on the same host.

---

#### Q11. (R) Attribute + conventional mixing. `GET /api/reports/summary` 404s; `/Reports/Summary` works; Tag Helper links wrong.

```csharp
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

[Route("api/[controller]")]
public class ReportsController : Controller
{
    [HttpGet("summary")]
    public IActionResult Summary() => Json(_svc.Summary());
    public IActionResult Index() => View();
}
```

```html
<a asp-controller="Reports" asp-action="Summary">Summary API</a>
```

**Answer:** `Summary` is attribute-routed at **`/api/reports/summary`** only — conventional `{controller}/{action}` does not apply to attribute-routed actions. Tag Helpers use **conventional** route values by default, generating `/Reports/Summary`, which has **no endpoint** (404).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Mixed routing | API action attribute-only; Tag Helper assumes conventional | Links 404 in views |
| Link generation | `asp-controller`/`asp-action` ignore `[Route("api/[controller]")]` | Wrong path for JSON endpoint |
| Testing | Tests hit conventional URL that works only if duplicate action existed | False confidence |

**Fix (priority order):**

1. Use conventional link for MVC pages and hard-coded or `LinkGenerator` path for API: `href="/api/reports/summary"` or dedicated named route `[HttpGet("summary", Name = "ReportsSummary")]`.
2. Do not link to JSON actions from Razor with `asp-action` unless you add a conventional route name via `[HttpGet("/reports/summary")]` for HTML and separate API controller.
3. Split `ReportsApiController` (attribute) and `ReportsController` (conventional views) — clearest for Karat-style audits.

**Production takeaway:** Attribute-routed actions opt out of conventional link generation — `Url.Action("Summary","Reports")` and Tag Helpers won't infer `api/` prefix; standardize per controller type.

---
