# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `06. ASP.NET Core MVC/10. Areas`

---

#### Q1. (R) A developer creates an Admin area following folder conventions but skips the area attribute. The project builds; `GET /Admin/Dashboard` returns 404. Review the controller and routing setup — what is wrong?

**Answer:** Folder placement alone does not register a controller with the area route — `DashboardController` must carry `[Area("Admin")]` so MVC associates it with the `{area:exists}` segment and resolves views under `Areas/Admin/Views/`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Routing | Missing `[Area("Admin")]` on controller | Request does not match area route to this controller — 404 |
| View discovery | No area metadata on controller action | Even if routed, view engine searches `/Views/Dashboard/` not `/Areas/Admin/Views/Dashboard/` |
| Compile vs runtime | Project builds — attribute is optional at compile time | False confidence until first HTTP request |

**Fix (priority order):**

1. Add `[Area("Admin")]` on `DashboardController` (class-level is typical).
2. Ensure view exists at `Areas/Admin/Views/Dashboard/Index.cshtml`.
3. Keep the area route registered (before default — see Q2).

```csharp
[Area("Admin")]
public class DashboardController : Controller
{
    public IActionResult Index() => View();
}
```

**Production takeaway:** Physical `Areas/` folders are a convention for humans and the view engine — routing and link generation require the `[Area]` attribute on the controller.

---

#### Q2. (R) After adding a Customer portal area, `/Portal/Orders/History` intermittently hits the wrong controller in staging. Review route registration — diagnose and fix in priority order.

**Answer:** The **default route is registered first**, so `{controller=Home}` greedily consumes `Portal` as the controller name and `Orders` as the action — the area route never gets a chance to match, sending traffic to root `OrdersController` or failing action lookup.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Route order | Default route before area route | First segment interpreted as `controller`, not `area` |
| Ambiguity | Two `OrdersController` types (root + area) | Wrong action executes or 404 depending on action names |
| Environment | "Intermittent" often means different URLs tested | `/Orders/History` vs `/Portal/Orders/History` mask the bug |

**Fix (priority order):**

1. Register the **area route before** the default route (more specific first).
2. Optionally add a dedicated named route per critical area for clarity.
3. Add integration tests that assert area URLs hit area controllers (route data `area=Portal`).

```csharp
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

**Production takeaway:** Route matching is first-wins — area routes must precede catch-all default routes, same principle as specific API routes before `{id}` templates.

---

#### Q3. (R) QA reports Admin sidebar links land on the public site. Review the layout and a controller redirect — what breaks link generation?

**Answer:** Tag helpers and `RedirectToAction` without an **area route value** assume the **current area only when ambient values exist** — from a root request or after redirect they emit URLs without `/Admin`, targeting root controllers instead.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Link generation | `asp-area` omitted on `<a>` tag helpers | URLs like `/Users/Index` hit root site, not Admin |
| Redirect | `RedirectToAction("Index")` without `new { area = "Admin" }` | After POST, user leaves Admin URL space |
| Ambient values | Area not preserved when layout rendered from wrong context | Sidebar copied to shared folder without area context worsens bug |

**Fix (priority order):**

1. Add `asp-area="Admin"` on area navigation links (or use `_ViewImports` `@addTagHelper` with a custom helper that pins area).
2. Use explicit route values on redirects: `RedirectToAction(nameof(Index), new { area = "Admin" })`.
3. Prefer `RedirectToAction` with `[Area("Admin")]` on the target controller so ambient area flows when appropriate — still be explicit on cross-area links.

```html
<a asp-area="Admin" asp-controller="Users" asp-action="Index">Users</a>
```

```csharp
return RedirectToAction(nameof(Index), new { area = "Admin" });
```

**Production takeaway:** Karat tests whether you treat **area as part of the route contract** — same as forgetting `controller` or `id` in API links.

---

#### Q4. (P) Product wants one `_OrderSummary.cshtml` partial reused by **Store**, **Admin**, and **Support** areas, but each area keeps its own chrome (`_Layout`). Where should shared markup live, how do area views reference it, and what breaks if each team copies the partial into their area folder?

**Answer:** Place cross-area partials in **`/Views/Shared/`** at the application root; area views invoke them with `<partial name="_OrderSummary" model="..." />` — the view engine searches area `Shared` first, then **root `/Views/Shared`**, while `_Layout` stays per-area via each area's `_ViewStart.cshtml`.

- **Per-area chrome:** `Areas/{Area}/Views/_ViewStart.cshtml` sets `Layout = "_Layout"` resolving to `Areas/{Area}/Views/Shared/_Layout.cshtml`.
- **Shared partial:** Single `_OrderSummary.cshtml` in `/Views/Shared/` — one place for markup and bug fixes.
- **Copy per area:** Three diverging copies drift on bug fixes, CSS classes, and model types; hotfixes miss one portal.
- **Area-only partials:** Put in `Areas/{Area}/Views/Shared/` when behavior or styling truly differs by surface.
- **RCL option:** For large shared UI across apps, extract a Razor Class Library with embedded views — still one source of truth.

**Production takeaway:** Areas partition **routing and layout**, not every reusable fragment — root `Views/Shared` is the supported escape hatch for cross-area partials.

---

#### Q5. (R) A contractor reorganizes the Support area to "flatten" paths. Views compile in IDE but runtime throws `InvalidOperationException: The view 'Index' was not found`. Review the structure — what's wrong?

**Answer:** Controllers must live under **`Areas/Support/Controllers/`** — placing `TicketsController.cs` directly under `Areas/Support/` removes it from conventional area controller discovery, and the view engine path `Areas/Support/Views/Tickets/Index.cshtml` will not pair with a mislocated controller.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Folder convention | Controller not in `Controllers/` subfolder | Controller may not be discovered or area metadata inconsistent |
| View lookup | Engine expects `Views/{ControllerName}/{Action}.cshtml` | `Index` not found at expected path |
| Tooling | IDE may still syntax-highlight `.cshtml` | Hides routing/view resolution failure until runtime |

**Fix (priority order):**

1. Move to `Areas/Support/Controllers/TicketsController.cs` with `[Area("Support")]`.
2. Keep views at `Areas/Support/Views/Tickets/Index.cshtml`.
3. Retain `Areas/Support/Views/_ViewStart.cshtml` for layout chain.

**Correct structure:**

```
Areas/
  Support/
    Controllers/
      TicketsController.cs
    Views/
      Tickets/
        Index.cshtml
      Shared/
        _Layout.cshtml
      _ViewStart.cshtml
```

**Production takeaway:** Area folder layout is not arbitrary — discovery, view location expanders, and scaffolding all assume `Controllers/` and `Views/{Controller}/`.

---

#### Q6. (R) Root site and Admin area both define `HomeController`. `/` works; `/Admin/Home/Index` works; but `GET /Home/Index` from an Admin page and integration tests for "admin home" fail unpredictably. Explain the collision and how routing resolves it.

**Answer:** Duplicate controller **names** are legal across areas because route data disambiguates — **`area` route value** selects `Areas.Admin.Controllers.HomeController` vs root `Controllers.HomeController`; links and tests that omit `area` always bind to the root controller.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Name collision | Two `HomeController` classes | Not a compile error — routing ambiguity if area missing |
| Link generation | `asp-controller="Home"` without `asp-area="Admin"` | Admin UI sends users to marketing home |
| Tests | `GET /Home/Index` without area segment | Asserts public home, not admin dashboard |

- MVC resolves by **route order + route values** — `{ area = "Admin", controller = "Home" }` selects the area type.
- Root `/` uses default route with no area → root `HomeController`.
- Prefer distinct controller names (`AdminHomeController`) only when it improves clarity — not required by framework.
- Integration tests must request `/Admin/Home/Index` or assert `RouteData.Values["area"]`.

**Production takeaway:** Same controller name in two areas is a **link-generation and testing** problem, not a type-system problem — always treat `area` as a first-class route parameter.

---

#### Q7. (R) Admin views fail to compile after moving area-specific tag helpers and `@using` directives to the root `_ViewImports.cshtml` only. Review the view import scope — what is wrong and how do area-level imports interact with the root file?

**Answer:** `_ViewImports.cshtml` applies hierarchically to views **under its directory** — root `/Views/_ViewImports.cshtml` does **not** import into `Areas/Admin/Views/`; each area needs its own `_ViewImports.cshtml` beside its views.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Import scope | Admin-only `@using` / tag helpers in root `_ViewImports` | Pollutes root views; Admin views still lack imports if removed from root |
| Compilation | `<admin-card>` and `AdminDashboardVm` unknown in area views | Razor build errors in Admin project slice |
| Coupling | Root imports reference `Areas.Admin.ViewModels` | Wrong direction — root views shouldn't depend on Admin internals |

**Fix (priority order):**

1. Create `Areas/Admin/Views/_ViewImports.cshtml` with area-specific `@using MyApp.Areas.Admin.ViewModels` and `@addTagHelper *, AdminTagHelpers`.
2. Keep root `/Views/_ViewImports.cshtml` for site-wide usings only (`@using MyApp.Models`).
3. Duplicate shared `@inject` or tag helpers in both files when truly global.

```razor
@* Areas/Admin/Views/_ViewImports.cshtml *@
@using MyApp.Areas.Admin.ViewModels
@addTagHelper *, MyApp.AdminTagHelpers
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```

**Production takeaway:** Think of area `_ViewImports` like a **mini root** for that subtree — same rules as `/Views/_ViewImports`, separate hierarchy.

---

#### Q8. (P) Security audit: the Admin area must require the **Administrator** policy; Support requires **SupportAgent**; Store stays anonymous for browsing but checkout requires authentication. Review this partial setup — what gaps remain for a new controller added tomorrow?

**Answer:** Per-controller `[Authorize]` works until someone adds a controller **without** the attribute — there is no area-wide enforcement, so authorization regressions are one omission away.

- **Area convention (recommended):** Register an authorization convention or filter for the Admin area namespace:

```csharp
builder.Services.AddControllersWithViews(options =>
{
    options.Conventions.Add(new AreaAuthorizationConvention("Admin", "Administrator"));
    options.Conventions.Add(new AreaAuthorizationConvention("Support", "SupportAgent"));
});
```

- Or apply `[Authorize(Policy = "...")]` on a **base controller** per area that all area controllers inherit.
- **Store pattern:** Allow anonymous on catalog controllers; `[Authorize]` on `CheckoutController` only — area-level deny-all would break public browsing.
- **Login exceptions:** `[AllowAnonymous]` on `AccountController` in Admin for login path — ensure convention permits override.
- **Fallback policy:** `options.FallbackPolicy` affects entire app — too blunt for mixed Store/Admin; prefer targeted conventions.
- **Smoke tests:** Scan or integration-test that all `Areas/Admin/Controllers/*` return 401/403 without role.

**Production takeaway:** Production auth boundaries need **structural defaults** (convention/base class) plus action-level exceptions — ad hoc attributes do not scale with team size.

---

#### Q9. (M) The team wants `/Admin` (no controller segment) to open the admin dashboard, while `/Admin/Users` still works. Review this route attempt — what matches, what 404s, and what is the correct pattern?

**Answer:** The template `Admin/{controller=Dashboard}/{action=Index}/{id?}` with `defaults: { area = "Admin" }` correctly maps **`/Admin` → Dashboard/Index** and **`/Admin/Users` → Users/Index** — but it must be registered **before** the generic `{area:exists}` route, and `[Area("Admin")]` must exist on `DashboardController`.

| Request | Matches | Result |
|---|---|---|
| `GET /Admin` | `admin_root` with defaults | `Admin/Dashboard/Index` if controller exists |
| `GET /Admin/Users` | `admin_root`, controller=Users | `UsersController.Index` in Admin area |
| `GET /Admin/Dashboard/Index` | Either admin or generic area route | Works if controller attributed |

- **404 causes:** Missing `[Area("Admin")]`, `DashboardController` not in `Areas/Admin/Controllers/`, or route registered after a conflicting default route that captures `Admin` as controller.
- **Generic alternative:** `{area:exists}/{controller=Dashboard}/{action=Index}/{id?}` — sets default **controller** inside any area, not default area on root site.
- **Shortcut URL:** Some teams map `GET /Admin` via `MapGet` redirect to `/Admin/Dashboard` — explicit but duplicates route knowledge.

```csharp
app.MapControllerRoute(
    name: "admin_root",
    pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}",
    defaults: new { area = "Admin" });

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
```

**Production takeaway:** "Default area routing" usually means **default controller/action within a fixed area prefix** — the `area` default is pinned in route defaults, not inferred from URL omission on the root site.

---

#### Q10. (D) A monolith MVC app grows **Marketing**, **Store**, **Admin**, and **API** surfaces. Product asks whether to keep **Areas**, split into Razor Class Libraries, or separate deployable apps. What decision criteria matter for routing, auth boundaries, team ownership, and release cadence — and when do Areas become the wrong tool?

**Answer:** **Areas** suit one deployable MVC app with shared domain services and distinct URL namespaces; separate apps or microservices fit when **scaling, security isolation, or release independence** dominate.

| Factor | Stay with Areas | RCL / shared libraries | Separate apps |
|---|---|---|---|
| Routing | Single site, `/Admin`, `/Store` prefixes | Same host, reusable UI packages | Different hosts, gateways, cookies |
| Auth | Shared cookie auth across areas works | Same | Separate identity, SSO, token exchange |
| Teams | One team or tight coupling | Shared UI kit, one deploy | Independent release cadence |
| API surface | Keep Web API out of area sprawl — use `/api` or Minimal APIs project | N/A | Dedicated API service |

- **Areas win when:** One Kestrel process, shared `DbContext`, layouts differ but domain is unified, ops wants one container.
- **RCL win when:** Multiple MVC apps need identical partials/tag helpers — extract UI, not business boundaries.
- **Separate apps win when:** Admin must be VPN-only, Store needs aggressive scale-out, compliance mandates network isolation, or teams ship on different schedules.
- **Regret signals for Areas:** Copy-paste `Program.cs` auth per area, circular references between area view models, API controllers mixed into area folders, 15+ areas with conflicting route templates.

**Production takeaway:** Areas are a **routing and view organization** feature, not a substitute for service boundaries — Karat tests whether you know when URL prefixes are enough vs when deployable boundaries are required.
