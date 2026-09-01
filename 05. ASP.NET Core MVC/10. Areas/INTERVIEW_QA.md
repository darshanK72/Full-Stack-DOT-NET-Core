# Areas — Interview Q&A
> Back to [README](../README.md)

## Table of Contents

- [Chapter 10. Areas](#chapter-10-areas)
  - [Q1. What are Areas in ASP.NET Core MVC?](#chapter-10-areas-q1)
  - [Q2. Why use Areas instead of controller name prefixes?](#chapter-10-areas-q2)
  - [Q3. What folder structure is required for an Area?](#chapter-10-areas-q3)
  - [Q4. What is the `[Area("Admin")]` attribute and why is it requir…](#chapter-10-areas-q4)
  - [Q5. How is area routing registered in `Program.cs`?](#chapter-10-areas-q5)
  - [Q6. What is the standard areas route pattern?](#chapter-10-areas-q6)
  - [Q7. Why does route registration order matter for Areas?](#chapter-10-areas-q7)
  - [Q8. How do you generate links to area controllers using Tag Help…](#chapter-10-areas-q8)
  - [Q9. What happens when `asp-controller` is used without `asp-area…](#chapter-10-areas-q9)
  - [Q10. What is the difference between root `Controllers` and `Areas…](#chapter-10-areas-q10)
  - [Q11. Can two controllers have the same name in different Areas?](#chapter-10-areas-q11)
  - [Q12. Where should shared partials used by multiple Areas live?](#chapter-10-areas-q12)
  - [Q13. How does layout resolution work for Area views?](#chapter-10-areas-q13)
  - [Q14. What is `Areas/{AreaName}/Views/_ViewStart.cshtml` used for?](#chapter-10-areas-q14)
  - [Q15. How do `_ViewImports` files scope between root Views and Are…](#chapter-10-areas-q15)
  - [Q16. How do you apply authorization to an entire Area?](#chapter-10-areas-q16)
  - [Q17. How do you map `/Admin` to a default dashboard action in the…](#chapter-10-areas-q17)
  - [Q18. When should you use Areas vs Razor Class Libraries vs separa…](#chapter-10-areas-q18)
- [Gotchas](#gotchas)
- [Scenario-Based Questions](#scenario-based-questions-karat-format)

---

## Chapter 10. Areas

### Q1. What are Areas in ASP.NET Core MVC? {#chapter-10-areas-q1}

What are Areas in ASP.NET Core MVC?

**Answer:** Areas are a feature that partitions a single MVC application into logical sections with their own URL prefix, controllers, and views — for example `/Admin/Users` and `/Store/Products` — while sharing the same host, DI container, and domain services.

- Each area is identified by a route segment (`Admin`, `Store`) and maps to controllers under `Areas/{AreaName}/Controllers/`.
- Areas let one deployable app serve multiple UI surfaces (public site, admin portal, support desk) without duplicating `Program.cs` or database infrastructure.
- The view engine resolves Razor files from `Areas/{AreaName}/Views/{Controller}/{Action}.cshtml` when route data includes an `area` value.
- Areas are an organizational and routing feature, not a substitute for separate microservices or network isolation boundaries.

---

### Q2. Why use Areas instead of controller name prefixes? {#chapter-10-areas-q2}

Why use Areas instead of controller name prefixes?

**Answer:** Areas provide first-class routing, view discovery, and link generation keyed by the `area` route value, whereas prefixing controller names (`AdminUsersController`) only changes type names and produces awkward URLs like `/AdminUsers/Index`.

- The standard area route pattern `{area:exists}/{controller}/{action}` produces clean, predictable URLs (`/Admin/Users/Index`) that product and QA teams can reason about.
- Tag Helpers and `RedirectToAction` accept `asp-area` and `{ area = "Admin" }` route values — prefix naming offers no equivalent metadata.
- Two areas can reuse the same controller name (`HomeController` in root and in Admin) because the `area` segment disambiguates them.
- Folder conventions (`Areas/Admin/Controllers`, `Areas/Admin/Views`) align with scaffolding, view location expanders, and IDE tooling.

---

### Q3. What folder structure is required for an Area? {#chapter-10-areas-q3}

What folder structure is required for an Area?

**Answer:** An area requires a root folder under `Areas/{AreaName}/` with at minimum a `Controllers/` subfolder for area controllers and a `Views/` subfolder mirroring the standard MVC view layout.

- Controllers live at `Areas/Admin/Controllers/UsersController.cs` — not directly under `Areas/Admin/`.
- Views follow `Areas/Admin/Views/{ControllerName}/{ActionName}.cshtml`, with optional `Areas/Admin/Views/Shared/` for area-specific layouts and partials.
- `Areas/Admin/Views/_ViewStart.cshtml` and `_ViewImports.cshtml` are strongly recommended for layout and import scoping within the area.
- The physical folder name and the string passed to `[Area("Admin")]` should match the area route segment used in URLs.

---

### Q4. What is the `[Area("Admin")]` attribute and why is it required? {#chapter-10-areas-q4}

What is the `[Area("Admin")]` attribute and why is it required?

**Answer:** `[Area("Admin")]` is a class-level attribute on a controller that registers it with MVC's area routing and view discovery system, associating the controller with the `Admin` area route segment.

- Without it, a controller in `Areas/Admin/Controllers/` is not matched by the `{area:exists}` route template and typically returns 404.
- The attribute value becomes route data `area = "Admin"`, which the view engine uses to search under `Areas/Admin/Views/` instead of `/Views/`.
- The project compiles without the attribute — the failure appears only at runtime when a request hits the area URL.
- Link generation from outside the area requires explicit `asp-area="Admin"` because ambient area values are absent on root requests.

---

### Q5. How is area routing registered in `Program.cs`? {#chapter-10-areas-q5}

How is area routing registered in `Program.cs`?

**Answer:** Area routing is registered with `app.MapControllerRoute` (or `MapAreaControllerRoute`) after `app.MapControllers()` setup, typically defining a named route with the `{area:exists}` constraint before the default route.

- Call `app.MapControllerRoute` with pattern `"{area:exists}/{controller=Home}/{action=Index}/{id?}"` and name `"areas"`.
- Register the area route **before** the default `{controller}/{action}/{id?}` route so the first URL segment is interpreted as `area`, not `controller`.
- ASP.NET Core 8 supports the same endpoint routing infrastructure used by Minimal APIs — area routes are conventional MVC routes mapped at startup.
- Optional dedicated routes (e.g., `Admin/{controller=Dashboard}/{action=Index}` with `defaults: new { area = "Admin" }`) can shorten URLs for specific areas.

---

### Q6. What is the standard areas route pattern? {#chapter-10-areas-q6}

What is the standard areas route pattern?

**Answer:** The standard pattern is `{area:exists}/{controller=Home}/{action=Index}/{id?}`, where `{area:exists}` constrains the first segment to a registered area name and supplies defaults for controller and action when omitted.

- A request to `/Admin/Users/Edit/5` yields route values `{ area = "Admin", controller = "Users", action = "Edit", id = "5" }`.
- The `:exists` constraint ensures unknown first segments do not falsely match as areas and fall through to other routes.
- Defaults allow `/Admin` or `/Admin/Users` to resolve to `Home/Index` or `Users/Index` within the area when configured.
- The pattern mirrors the default site route but prepends the area segment as the primary namespace.

---

### Q7. Why does route registration order matter for Areas? {#chapter-10-areas-q7}

Why does route registration order matter for Areas?

**Answer:** MVC evaluates routes in registration order and uses the first match — if the default route is registered before the area route, `/Admin/Users` is parsed as `controller=Admin, action=Users` instead of `area=Admin, controller=Users`.

- The area route is more specific and must be registered first so `{area:exists}` consumes the leading segment correctly.
- Misordered routes cause intermittent bugs depending on which URL shape is tested (`/Users/Index` vs `/Admin/Users/Index`).
- Named routes do not change matching priority — only registration order and template specificity matter.
- Integration tests should assert `RouteData.Values["area"]` for area URLs to catch order regressions.

---

### Q8. How do you generate links to area controllers using Tag Helpers? {#chapter-10-areas-q8}

How do you generate links to area controllers using Tag Helpers?

**Answer:** Use `asp-area`, `asp-controller`, and `asp-action` on anchor and form tag helpers to emit URLs that include the area route segment.

- From a root view linking into Admin: `<a asp-area="Admin" asp-controller="Users" asp-action="Index">Users</a>` generates `/Admin/Users`.
- `RedirectToAction` requires the same route value: `RedirectToAction(nameof(Index), new { area = "Admin" })`.
- When already inside an area, ambient values may carry the current area — cross-area links still need an explicit `asp-area` for the target area.
- Omitting `asp-area` when linking from a root layout to an area controller produces root URLs (`/Users/Index`) that miss the area prefix.

---

### Q9. What happens when `asp-controller` is used without `asp-area` from within an Area view? {#chapter-10-areas-q9}

What happens when `asp-controller` is used without `asp-area` from within an Area view?

**Answer:** Tag Helpers inherit ambient route values from the current request, so `asp-controller="Users"` from an Admin view typically generates `/Admin/Users/Index` using the current area context.

- Ambient area values flow from the executing request's route data — links within the same area often work without explicit `asp-area`.
- Linking to a **root** controller from an area view requires `asp-area=""` (empty string) to clear the ambient area and target `/Home/Index` on the root site.
- Linking to a **different** area requires explicit `asp-area="Support"` — ambient Admin context would otherwise stay in the URL.
- Redirects after POST must also pass area route values explicitly or the user may leave the area URL space.

---

### Q10. What is the difference between root `Controllers` and `Areas/Admin/Controllers`? {#chapter-10-areas-q10}

What is the difference between root `Controllers` and `Areas/Admin/Controllers`?

**Answer:** Root controllers in `/Controllers` serve the default site without an area route segment (`/Home/Index`), while area controllers in `Areas/Admin/Controllers` require the area prefix (`/Admin/Home/Index`) and carry `[Area("Admin")]`.

- Root controllers resolve views from `/Views/{Controller}/{Action}.cshtml`; area controllers resolve from `Areas/Admin/Views/{Controller}/{Action}.cshtml`.
- Both share the same DI container, middleware pipeline, and domain services — only routing and view location differ.
- The same controller class name can exist in both locations because the fully qualified type and route values differ.
- Root controllers do not use `[Area]`; area controllers must declare it for discovery and view resolution.

---

### Q11. Can two controllers have the same name in different Areas? {#chapter-10-areas-q11}

Can two controllers have the same name in different Areas?

**Answer:** Yes — MVC disambiguates by the `area` route value, so `Areas/Admin/Controllers/HomeController` and `Areas/Store/Controllers/HomeController` coexist as separate types matched by `/Admin/Home` vs `/Store/Home`.

- This is legal at compile time because they are different classes in different namespaces/folders.
- Link generation and integration tests must include the `area` route value — omitting it always targets the root controller of that name if one exists.
- Duplicate names increase navigation and testing confusion; some teams rename to `AdminHomeController` for clarity, but the framework does not require it.
- Route order and explicit area segments determine which controller handles a request — there is no automatic ambiguity resolution beyond route values.

---

### Q12. Where should shared partials used by multiple Areas live? {#chapter-10-areas-q12}

Where should shared partials used by multiple Areas live?

**Answer:** Cross-area partials belong in the application root `/Views/Shared/`, which the view engine searches after the area's own `Shared` folder when resolving partial names.

- Area-specific partials with different markup or styling stay in `Areas/{AreaName}/Views/Shared/`.
- Invoke shared partials from area views with `<partial name="_OrderSummary" model="..." />` — resolution falls back to root `Views/Shared`.
- Copying the same partial into each area causes drift when one copy is updated and others are not.
- For reuse across multiple MVC applications, extract views into a Razor Class Library with embedded resources.

---

### Q13. How does layout resolution work for Area views? {#chapter-10-areas-q13}

How does layout resolution work for Area views?

**Answer:** Area views resolve layouts through a hierarchical search starting in the area's view folders, then falling back to root `/Views/Shared/`, guided by `Areas/{AreaName}/Views/_ViewStart.cshtml`.

- `Areas/Admin/Views/_ViewStart.cshtml` typically sets `Layout = "_Layout"`, resolving to `Areas/Admin/Views/Shared/_Layout.cshtml` first.
- If the layout is not found in the area, the view engine searches `/Views/Shared/_Layout.cshtml` at the application root.
- Nested layouts work the same as root views — a child layout in the area can call `@RenderBody()` and define sections.
- Each area can maintain distinct chrome (navigation, branding) via its own `_Layout.cshtml` without affecting other areas.

---

### Q14. What is `Areas/{AreaName}/Views/_ViewStart.cshtml` used for? {#chapter-10-areas-q14}

What is `Areas/{AreaName}/Views/_ViewStart.cshtml` used for?

**Answer:** It applies layout and other view-start directives to every Razor view under that area's `Views` folder, equivalent to root `_ViewStart.cshtml` but scoped to the area subtree.

- Typical content: `@{ Layout = "_Layout"; }` pointing to the area's shared layout.
- Runs before each view in `Areas/Admin/Views/` is rendered — individual views can override with `Layout = null` or a different layout path.
- Does not affect root `/Views/` or other areas — each area has its own independent `_ViewStart` chain.
- Can also set common `ViewBag` title prefixes or other view-level defaults for the area.

---

### Q15. How do `_ViewImports` files scope between root Views and Area Views? {#chapter-10-areas-q15}

How do `_ViewImports` files scope between root Views and Area Views?

**Answer:** `_ViewImports.cshtml` applies hierarchically to views in its directory and subdirectories — root `/Views/_ViewImports.cshtml` does not import into `Areas/Admin/Views/` unless a separate area `_ViewImports` exists there.

- Each area needs its own `Areas/Admin/Views/_ViewImports.cshtml` for area-specific `@using`, `@inject`, and `@addTagHelper` directives.
- Root `_ViewImports` should contain site-wide usings only — polluting it with area-specific namespaces couples root views to Admin internals.
- Shared tag helpers (e.g., `Microsoft.AspNetCore.Mvc.TagHelpers`) are often duplicated in both root and area `_ViewImports` files.
- Think of each area's `_ViewImports` as a mini root for that view subtree with the same scoping rules.

---

### Q16. How do you apply authorization to an entire Area? {#chapter-10-areas-q16}

How do you apply authorization to an entire Area?

**Answer:** Apply authorization structurally with an area authorization convention, a base controller class all area controllers inherit, or a global filter scoped to the area namespace — not by relying on per-controller `[Authorize]` alone.

- Register a convention: `options.Conventions.Add(new AreaAuthorizationConvention("Admin", "Administrator"))` in `AddControllersWithViews`.
- Create `AdminBaseController` with `[Authorize(Policy = "Administrator")]` and inherit all Admin controllers from it.
- Use `[AllowAnonymous]` on specific controllers (e.g., `AccountController` login) to override area-wide requirements.
- Fallback policies affect the entire app — prefer targeted area conventions over a global deny-all when only some areas need protection.

---

### Q17. How do you map `/Admin` to a default dashboard action in the Admin area? {#chapter-10-areas-q17}

How do you map `/Admin` to a default dashboard action in the Admin area?

**Answer:** Register a dedicated route before the generic area route with a fixed area prefix and default controller/action values, pinning `area = "Admin"` in route defaults.

- Pattern example: `"Admin/{controller=Dashboard}/{action=Index}/{id?}"` with `defaults: new { area = "Admin" }` maps `/Admin` to `DashboardController.Index` in the Admin area.
- `DashboardController` must carry `[Area("Admin")]` and live under `Areas/Admin/Controllers/`.
- Register this route **before** the generic `{area:exists}` route and before the default site route to prevent `Admin` being captured as a root controller name.
- Alternative: `MapGet("/Admin", () => Results.Redirect("/Admin/Dashboard"))` for a simple shortcut without changing controller defaults.

---

### Q18. When should you use Areas vs Razor Class Libraries vs separate applications? {#chapter-10-areas-q18}

When should you use Areas vs Razor Class Libraries vs separate applications?

**Answer:** Use Areas for one deployable MVC app with distinct URL namespaces and shared domain logic; use Razor Class Libraries for shared UI packages across apps; use separate applications when release independence, scaling, or security isolation require different deployable boundaries.

- **Areas fit** when one team ships one container, sharing `DbContext`, authentication cookies, and services across Admin, Store, and Marketing surfaces.
- **RCLs fit** when multiple MVC hosts need identical partials, tag helpers, or embedded views without sharing business boundaries.
- **Separate apps fit** when Admin must be network-isolated (VPN-only), teams release on different cadences, or Store needs independent scale-out beyond what one process provides.
- Areas are a routing and view organization tool — they do not provide process isolation, separate databases, or independent deployment pipelines.

---

---

## Gotchas — ASP.NET Core MVC (Interview Traps)

#### Gotcha 1. Business logic in Razor views

**Answer:** Placing pricing, discount, authorization, or business rules in `.cshtml` files bypasses unit tests, duplicates service-layer logic, and makes behavior hard to change consistently.

- Views should render data the controller or ViewModel already prepared.
- Authorization belongs in filters, policies, or controller/service checks before the view executes.
- Calculations in Razor cannot be tested independently and often diverge from API or batch logic.
- Keep Razor limited to presentation formatting — not business decisions.

---

#### Gotcha 2. EF entities passed directly to views

**Answer:** Binding and displaying EF Core entities exposes navigation properties, causes over-posting on POST, and couples the UI to the database schema.

- Lazy-loaded navigations can trigger unexpected queries during rendering.
- Mass assignment can update properties the user should not control (e.g., `IsAdmin`).
- Use dedicated ViewModels with only the fields the view needs.
- Map between entities and ViewModels in the controller or a mapping service.

---

#### Gotcha 3. `[FromBody]` on HTML form POST

**Answer:** Standard browser forms send `application/x-www-form-urlencoded` or `multipart/form-data`, not JSON. `[FromBody]` uses the JSON input formatter and leaves the model empty while the action runs with default values.

- Remove `[FromBody]` for conventional form POSTs and let model binding read form fields.
- Use `[FromBody]` only when the client sends JSON with the correct Content-Type.
- Silent binding failure is a common source of "my POST action receives null model" bugs.
- AJAX forms using `FormData` follow the same form binding rules as full-page forms.

---

#### Gotcha 4. Skipping `ModelState.IsValid` because of client validation

**Answer:** Client-side validation is bypassable — attackers POST directly without browser scripts. Server-side validation is mandatory before any persist, redirect, or side effect.

- Always gate POST actions with `if (!ModelState.IsValid) return View(model);` or equivalent.
- Client validation improves UX for legitimate users only.
- Remote validation and unobtrusive rules are not security boundaries.
- Treat missing server validation as a security defect regardless of client script presence.

---

#### Gotcha 5. `return View()` after successful POST

**Answer:** Returning the same view after a successful POST causes duplicate submission when the user refreshes the page — the browser resubmits the POST body.

- Use Post-Redirect-Get: `return RedirectToAction(nameof(Index))` after successful create/update.
- PRG separates the mutation (POST) from the display (GET).
- Flash success messages via TempData on the redirect target.
- AJAX partial POSTs have a similar concern — disable submit during request or use idempotent server logic.

---

#### Gotcha 6. `ModelState` after redirect

**Answer:** `ModelState` is request-scoped and does not survive `RedirectToAction`. Validation errors are lost unless rehydrated through TempData, a second validation pass on GET, or by redisplaying the form without redirect on failure only.

- Common pattern: redirect only on success; on validation failure return `View(model)` with errors inline.
- To survive redirect on failure, serialize errors to TempData or use PRG with a form-specific error cache.
- Do not assume errors automatically follow the user after redirect.
- AJAX partial forms avoid redirect and can return the form partial with `ModelState` errors directly.

---

#### Gotcha 7. TempData read twice in layout and view

**Answer:** TempData is consumed on first read by default. If the layout reads a flash message, the view sees nothing unless you use `Peek()` or `Keep()`.

- Use `TempData.Peek("Message")` in the layout to read without consuming.
- Or call `TempData.Keep("Message")` after the layout read so the view can read it too.
- Prefer a single consumption point — typically the layout or a dedicated partial, not both.
- Cookie-based TempData has size limits; avoid storing large payloads.

---

#### Gotcha 8. Missing `[Area]` attribute on area controllers

**Answer:** Controllers in `Areas/Admin/Controllers` without `[Area("Admin")]` are not discovered by the areas route and return 404 or match the wrong conventional route.

- Every area controller must declare `[Area("AreaName")]` matching its folder.
- Area routing is registered separately in `Program.cs` with the `{area:exists}` constraint.
- Without the attribute, MVC treats the controller as a root controller.
- Verify area registration order — specific area routes before catch-all default routes.

---

#### Gotcha 9. Link generation without `asp-area`

**Answer:** Tag Helpers default to the current area context when generating URLs. Links from a root view to an area controller need explicit `asp-area="Admin"` or they generate URLs without the area segment.

- From within an area, omitting `asp-area` keeps links inside the current area — sometimes incorrectly.
- Cross-area links require both `asp-area` and `asp-controller` (and `asp-action`).
- Wrong URLs produce 404 or hit unintended controllers.
- Same rule applies to `Url.Action` — pass `new { area = "Admin" }` in route values.

---

#### Gotcha 10. Checkbox `[Required]` on non-nullable `bool`

**Answer:** A missing unchecked checkbox posts nothing and model binding sets a non-nullable `bool` to `false`. `[Required]` never fails because `false` is a valid value — not null or empty.

- Use `bool?` with `[Required]` to require an explicit true selection for consent checkboxes.
- Or use the hidden-field pattern: hidden input `false` plus checkbox `true` so unchecked still posts `false` deliberately.
- Server-side, verify explicit consent with a dedicated check rather than relying on `[Required]` alone.
- This applies to both full-page forms and AJAX form posts.

---

#### Gotcha 11. Collection binding with gap indices

**Answer:** Deleting a row from a dynamic form leaving indices such as `Lines[0]` and `Lines[2]` breaks model binder alignment — index 1 is missing and subsequent items may bind incorrectly or truncate.

- Reindex client-side after row deletion so indices are contiguous starting at zero.
- Or implement a custom `IModelBinder` that tolerates non-contiguous indices.
- Partial views rendering collection editors must maintain consistent index naming.
- Test add/delete row scenarios explicitly in complex form POSTs.

---

#### Gotcha 12. `@Html.Raw` with user content

**Answer:** Default Razor encoding prevents XSS by HTML-encoding output. `@Html.Raw(Model.UserComment)` renders attacker-supplied script if the content is not sanitized server-side.

- Encode first, then apply safe formatting — never wrap raw user input in HTML.
- AJAX-loaded partials injected via `innerHTML` execute injected script the same as full pages.
- Prefer `@Model.UserComment` (auto-encoded) or sanitize with a trusted HTML sanitizer library.
- Content-Security-Policy limits blast radius but does not replace encoding.

---

#### Gotcha 13. AJAX POST without antiforgery token

**Answer:** Form tag helpers emit antiforgery tokens automatically, but `fetch` and jQuery AJAX must manually send `RequestVerificationToken` header or `__RequestVerificationToken` form field or POSTs fail with 400 antiforgery errors.

- Read the hidden field value from the page and include it on every mutating AJAX request.
- Same-origin requests send the antiforgery cookie automatically.
- `[AutoValidateAntiforgeryToken]` on the controller validates all unsafe methods — missing tokens fail before the action runs.
- Do not disable antiforgery on MVC cookie-auth endpoints to "fix" AJAX — add the token instead.

---

#### Gotcha 14. Injecting Hub into MVC controller

**Answer:** Hubs are not registered in DI for direct injection into controllers. Use `IHubContext<THub>` to broadcast messages from controllers, services, or background jobs.

- Injecting a concrete `Hub` fails activation or produces an instance without connection context.
- `IHubContext<T>` is a singleton proxy registered by `AddSignalR()`.
- Pair with Redis backplane or Azure SignalR for multi-instance fan-out.
- Keep hubs thin; business logic stays in scoped or transient services.

---

#### Gotcha 15. SignalR scale-out without backplane

**Answer:** Sticky sessions alone do not fan-out events across server instances. Multi-node deployments need a Redis backplane or Azure SignalR Service so messages sent from any instance reach clients on all instances.

- Controller on instance A calling `IHubContext.Clients.User(id).SendAsync` misses users connected to instance B without a backplane.
- Sticky sessions route connections but do not route cross-instance messages.
- Group membership and connection IDs are local to each instance.
- Register `AddStackExchangeRedis` or `AddAzureSignalR` when scaling beyond a single node.

---

## Gotchas — ASP.NET Core MVC (Interview Traps)

## Gotchas — ASP.NET Core MVC (Interview Traps)

---

## Scenario-Based Questions (Karat Format)

#### Q1. (R) A developer creates an Admin area following folder conventions but skips the area attribute. The project builds; `GET /Admin/Dashboard` returns 404. Review the controller and routing setup — what is wrong?

```csharp
// Areas/Admin/Controllers/DashboardController.cs
namespace MyApp.Areas.Admin.Controllers;

public class DashboardController : Controller
{
    public IActionResult Index() => View();
}
```

```csharp
// Program.cs
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

---

**Answer:**

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

---

#### Q2. (R) After adding a Customer portal area, `/Portal/Orders/History` intermittently hits the wrong controller in staging. Review route registration — diagnose and fix in priority order.

```csharp
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
```

*(Assume both root `OrdersController` and `Areas/Portal/Controllers/OrdersController` exist.)*

---

**Answer:**

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

---

#### Q3. (R) QA reports Admin sidebar links land on the public site. Review the layout and a controller redirect — what breaks link generation?

```html
<!-- Areas/Admin/Views/Shared/_AdminNav.cshtml -->
<a asp-controller="Users" asp-action="Index">Users</a>
<a asp-controller="Reports" asp-action="Index">Reports</a>
```

```csharp
// Areas/Admin/Controllers/UsersController.cs
[Area("Admin")]
public class UsersController : Controller
{
    public IActionResult Create() { /* ... */ return RedirectToAction("Index"); }
}
```

*(Current request: `/Admin/Dashboard/Index`.)*

---

**Answer:**

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

---

#### Q4. (P) Product wants one `_OrderSummary.cshtml` partial reused by **Store**, **Admin**, and **Support** areas, but each area keeps its own chrome (`_Layout`). Where should shared markup live, how do area views reference it, and what breaks if each team copies the partial into their area folder?

---

**Answer:**

**Answer:** Place cross-area partials in **`/Views/Shared/`** at the application root; area views invoke them with `<partial name="_OrderSummary" model="..." />` — the view engine searches area `Shared` first, then **root `/Views/Shared`**, while `_Layout` stays per-area via each area's `_ViewStart.cshtml`.

- **Per-area chrome:** `Areas/{Area}/Views/_ViewStart.cshtml` sets `Layout = "_Layout"` resolving to `Areas/{Area}/Views/Shared/_Layout.cshtml`.
- **Shared partial:** Single `_OrderSummary.cshtml` in `/Views/Shared/` — one place for markup and bug fixes.
- **Copy per area:** Three diverging copies drift on bug fixes, CSS classes, and model types; hotfixes miss one portal.
- **Area-only partials:** Put in `Areas/{Area}/Views/Shared/` when behavior or styling truly differs by surface.
- **RCL option:** For large shared UI across apps, extract a Razor Class Library with embedded views — still one source of truth.

**Production takeaway:** Areas partition **routing and layout**, not every reusable fragment — root `Views/Shared` is the supported escape hatch for cross-area partials.

---

---

#### Q5. (R) A contractor reorganizes the Support area to "flatten" paths. Views compile in IDE but runtime throws `InvalidOperationException: The view 'Index' was not found`. Review the structure — what's wrong?

```
Areas/
  Support/
    TicketsController.cs          ← moved here
    Views/
      Ticket/
        Index.cshtml
    _ViewStart.cshtml
```

*(Expected convention: `Areas/{AreaName}/Controllers/` and `Areas/{AreaName}/Views/{Controller}/`.)*

---

**Answer:**

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

---

#### Q6. (R) Root site and Admin area both define `HomeController`. `/` works; `/Admin/Home/Index` works; but `GET /Home/Index` from an Admin page and integration tests for "admin home" fail unpredictably. Explain the collision and how routing resolves it.

```csharp
// Controllers/HomeController.cs — public marketing site
public class HomeController : Controller { public IActionResult Index() => View(); }

// Areas/Admin/Controllers/HomeController.cs
[Area("Admin")]
public class HomeController : Controller { public IActionResult Index() => View(); }
```

---

**Answer:**

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

---

#### Q7. (R) Admin views fail to compile after moving area-specific tag helpers and `@using` directives to the root `_ViewImports.cshtml` only. Review the view import scope — what is wrong and how do area-level imports interact with the root file?

```
Views/
  _ViewImports.cshtml          ← @using MyApp.Areas.Admin.ViewModels
  Shared/
Areas/
  Admin/
    Views/
      Dashboard/
        Index.cshtml           ← uses <admin-card> tag helper + AdminDashboardVm
    _ViewStart.cshtml
```

---

**Answer:**

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

---

#### Q8. (P) Security audit: the Admin area must require the **Administrator** policy; Support requires **SupportAgent**; Store stays anonymous for browsing but checkout requires authentication. Review this partial setup — what gaps remain for a new controller added tomorrow?

```csharp
// Program.cs
builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("Administrator", p => p.RequireRole("Admin"));
    o.AddPolicy("SupportAgent", p => p.RequireRole("Support"));
});

// Areas/Admin/Controllers/UsersController.cs
[Area("Admin")]
[Authorize(Policy = "Administrator")]
public class UsersController : Controller { /* ... */ }

// Areas/Support/Controllers/TicketsController.cs
[Area("Support")]
[Authorize(Policy = "SupportAgent")]
public class TicketsController : Controller { /* ... */ }
```

*(No area-wide convention or filter registration — developers add controllers ad hoc.)*

---

**Answer:**

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

---

#### Q9. (M) The team wants `/Admin` (no controller segment) to open the admin dashboard, while `/Admin/Users` still works. Review this route attempt — what matches, what 404s, and what is the correct pattern?

```csharp
app.MapControllerRoute(
    name: "admin_root",
    pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}",
    defaults: new { area = "Admin" });

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
```

*(Request: `GET /Admin` — expected `DashboardController.Index` in Admin area.)*

---

**Answer:**

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

---

#### Q10. (D) A monolith MVC app grows **Marketing**, **Store**, **Admin**, and **API** surfaces. Product asks whether to keep **Areas**, split into Razor Class Libraries, or separate deployable apps. What decision criteria matter for routing, auth boundaries, team ownership, and release cadence — and when do Areas become the wrong tool?



**Answer:**

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

---
