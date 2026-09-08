# Areas — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What are Areas in ASP.NET Core MVC?](#q1-what-are-areas-in-aspnet-core-mvc)
2. [Q2. Why use Areas instead of controller name prefixes?](#q2-why-use-areas-instead-of-controller-name-prefixes)
3. [Q3. What folder structure is required for an Area?](#q3-what-folder-structure-is-required-for-an-area)
4. [Q4. What is the `[Area("Admin")]` attribute and why is it required?](#q4-what-is-the-areaadmin-attribute-and-why-is-it-required)
5. [Q5. How is area routing registered in `Program.cs`?](#q5-how-is-area-routing-registered-in-programcs)
6. [Q6. What is the standard areas route pattern?](#q6-what-is-the-standard-areas-route-pattern)
7. [Q7. Why does route registration order matter for Areas?](#q7-why-does-route-registration-order-matter-for-areas)
8. [Q8. How do you generate links to area controllers using Tag Helpers?](#q8-how-do-you-generate-links-to-area-controllers-using-tag-helpers)
9. [Q9. What happens when `asp-controller` is used without `asp-area` from within an Area view?](#q9-what-happens-when-asp-controller-is-used-without-asp-area-from-within-an-area-view)
10. [Q10. What is the difference between root `Controllers` and `Areas/Admin/Controllers`?](#q10-what-is-the-difference-between-root-controllers-and-areasadmincontrollers)
11. [Q11. Can two controllers have the same name in different Areas?](#q11-can-two-controllers-have-the-same-name-in-different-areas)
12. [Q12. Where should shared partials used by multiple Areas live?](#q12-where-should-shared-partials-used-by-multiple-areas-live)
13. [Q13. How does layout resolution work for Area views?](#q13-how-does-layout-resolution-work-for-area-views)
14. [Q14. What is `Areas/{AreaName}/Views/_ViewStart.cshtml` used for?](#q14-what-is-areasareanameviews_viewstartcshtml-used-for)
15. [Q15. How do `_ViewImports` files scope between root Views and Area Views?](#q15-how-do-_viewimports-files-scope-between-root-views-and-area-views)
16. [Q16. How do you apply authorization to an entire Area?](#q16-how-do-you-apply-authorization-to-an-entire-area)
17. [Q17. How do you map `/Admin` to a default dashboard action in the Admin area?](#q17-how-do-you-map-admin-to-a-default-dashboard-action-in-the-admin-area)
18. [Q18. When should you use Areas vs Razor Class Libraries vs separate applications?](#q18-when-should-you-use-areas-vs-razor-class-libraries-vs-separate-applications)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What are Areas in ASP.NET Core MVC?

**Concepts**
- Areas partitioning a single MVC app into logical URL sections
- Area-specific controllers under `Areas/{AreaName}/Controllers/`
- Shared DI container and domain services across areas
- View engine resolving Razor files from area-specific view folders

**Answer**

Areas are a feature that partitions a single MVC application into logical sections with their own URL prefix, controllers, and views — for example `/Admin/Users` and `/Store/Products` — while sharing the same host, DI container, and domain services. Each area is identified by a route segment and maps to controllers under `Areas/{AreaName}/Controllers/`, so one deployable app can serve multiple UI surfaces without duplicating `Program.cs` or database infrastructure. The view engine resolves Razor files from `Areas/{AreaName}/Views/{Controller}/{Action}.cshtml` when route data includes an `area` value. Areas are an organizational and routing feature, not a substitute for separate microservices or network isolation boundaries.

---

## Q2. Why use Areas instead of controller name prefixes?

**Concepts**
- First-class routing, view discovery, and link generation keyed by the `area` route value
- Controller name prefixes producing awkward URLs and no routing metadata
- Same controller name in multiple areas disambiguated by route value
- Folder conventions aligning with scaffolding and IDE tooling

**Answer**

Areas provide first-class routing, view discovery, and link generation keyed by the `area` route value, whereas prefixing controller names (`AdminUsersController`) only changes type names and produces awkward URLs like `/AdminUsers/Index`. The standard area route pattern `{area:exists}/{controller}/{action}` produces clean, predictable URLs that product and QA teams can reason about. Tag Helpers and `RedirectToAction` accept `asp-area` and `{ area = "Admin" }` route values — prefix naming offers no equivalent metadata. Two areas can reuse the same controller name because the `area` segment disambiguates them, and the folder conventions align with scaffolding, view location expanders, and IDE tooling.

---

## Q3. What folder structure is required for an Area?

**Concepts**
- `Areas/{AreaName}/Controllers/` for area controllers
- `Areas/{AreaName}/Views/{ControllerName}/{ActionName}.cshtml` view location
- `Areas/{AreaName}/Views/Shared/` for area-specific layouts and partials
- `_ViewStart.cshtml` and `_ViewImports.cshtml` scoped to the area view folder

**Answer**

An area requires a root folder under `Areas/{AreaName}/` with a `Controllers/` subfolder for area controllers and a `Views/` subfolder mirroring the standard MVC view layout. Controllers live at `Areas/Admin/Controllers/UsersController.cs` — not directly under `Areas/Admin/`. Views follow `Areas/Admin/Views/{ControllerName}/{ActionName}.cshtml`, with optional `Areas/Admin/Views/Shared/` for area-specific layouts and partials. `Areas/Admin/Views/_ViewStart.cshtml` and `_ViewImports.cshtml` are strongly recommended for layout and import scoping within the area. The physical folder name and the string passed to `[Area("Admin")]` should match the area route segment used in URLs.

---

## Q4. What is the `[Area("Admin")]` attribute and why is it required?

**Concepts**
- `[Area("Admin")]` registering the controller with MVC area routing and view discovery
- Attribute value becoming the `area = "Admin"` route data entry
- Missing attribute causing 404 at runtime despite the project compiling
- Link generation from outside the area requiring explicit `asp-area`

**Answer**

`[Area("Admin")]` is a class-level attribute on a controller that registers it with MVC's area routing and view discovery system, associating the controller with the `Admin` area route segment. Without it, a controller in `Areas/Admin/Controllers/` is not matched by the `{area:exists}` route template and typically returns 404. The attribute value becomes route data `area = "Admin"`, which the view engine uses to search under `Areas/Admin/Views/` instead of root `/Views/`. The project compiles without the attribute — the failure appears only at runtime when a request hits the area URL. Link generation from outside the area requires explicit `asp-area="Admin"` because ambient area values are absent on root requests.

---

## Q5. How is area routing registered in `Program.cs`?

**Concepts**
- `app.MapControllerRoute` with `{area:exists}` constraint registered before the default route
- `MapAreaControllerRoute` as a convenience wrapper
- Area route before default route — segment consumed as area rather than controller
- Dedicated short-URL routes pinning `area` in route defaults

**Answer**

Area routing is registered with `app.MapControllerRoute` after `AddControllersWithViews()` setup, defining a named route with the `{area:exists}` constraint before the default route. The pattern `"{area:exists}/{controller=Home}/{action=Index}/{id?}"` with name `"areas"` is the standard registration. The area route must be registered before the default `{controller}/{action}/{id?}` route so the first URL segment is interpreted as `area`, not `controller`. Optional dedicated routes — for example `Admin/{controller=Dashboard}/{action=Index}` with `defaults: new { area = "Admin" }` — can shorten URLs for specific areas without changing the generic area pattern.

---

## Q6. What is the standard areas route pattern?

**Concepts**
- `{area:exists}/{controller=Home}/{action=Index}/{id?}` as the standard pattern
- `:exists` constraint verifying the segment against registered area names
- Defaults allowing `/Admin` or `/Admin/Users` to resolve within the area
- Pattern mirroring the default site route with an area prefix

**Answer**

The standard pattern is `{area:exists}/{controller=Home}/{action=Index}/{id?}`, where `{area:exists}` constrains the first segment to a registered area name and supplies defaults for controller and action when omitted. A request to `/Admin/Users/Edit/5` yields route values `{ area = "Admin", controller = "Users", action = "Edit", id = "5" }`. The `:exists` constraint ensures unknown first segments do not falsely match as areas and fall through to other routes. Defaults allow `/Admin` or `/Admin/Users` to resolve to `Home/Index` or `Users/Index` within the area when so configured, mirroring the default site route but with the area segment prepended.

---

## Q7. Why does route registration order matter for Areas?

**Concepts**
- Default route registered before area route misinterpreting the first segment as controller
- Area route more specific — must be registered first
- Named routes not changing matching priority
- Integration test assertions on `RouteData.Values["area"]` catching regressions

**Answer**

MVC evaluates routes in registration order and uses the first match — if the default route is registered before the area route, `/Admin/Users` is parsed as `controller=Admin, action=Users` instead of `area=Admin, controller=Users`. The area route is more specific and must be registered first so `{area:exists}` can consume the leading segment correctly. Misordered routes cause intermittent bugs depending on which URL shape is tested. Named routes do not change matching priority — only registration order and template specificity matter. Integration tests should assert `RouteData.Values["area"]` for area URLs to catch order regressions.

---

## Q8. How do you generate links to area controllers using Tag Helpers?

**Concepts**
- `asp-area`, `asp-controller`, `asp-action` for area-aware URL generation
- `RedirectToAction` requiring the same area route value
- Cross-area links needing explicit `asp-area` for the target area
- Omitting `asp-area` generating root-prefixed URLs instead

**Answer**

Use `asp-area`, `asp-controller`, and `asp-action` on anchor and form tag helpers to emit URLs that include the area route segment. From a root view linking into Admin: `<a asp-area="Admin" asp-controller="Users" asp-action="Index">Users</a>` generates `/Admin/Users`. `RedirectToAction` requires the same route value: `RedirectToAction(nameof(Index), new { area = "Admin" })`. When already inside an area, ambient values may carry the current area — cross-area links still need an explicit `asp-area` for the target area. Omitting `asp-area` when linking from a root layout to an area controller produces root URLs that miss the area prefix.

---

## Q9. What happens when `asp-controller` is used without `asp-area` from within an Area view?

**Concepts**
- Ambient area route values flowing from the executing request
- Same-area links working without explicit `asp-area`
- `asp-area=""` required to escape an area and target root controllers
- Cross-area links requiring explicit `asp-area` for the target area

**Answer**

Tag Helpers inherit ambient route values from the current request, so `asp-controller="Users"` from an Admin view typically generates `/Admin/Users/Index` using the current area context. Ambient area values flow from the executing request's route data — links within the same area often work without explicit `asp-area`. Linking to a root controller from an area view requires `asp-area=""` (empty string) to clear the ambient area and target `/Home/Index` on the root site. Linking to a different area requires explicit `asp-area="Support"` — the ambient Admin context would otherwise stay in the URL. Redirects after POST must also pass area route values explicitly or the user may leave the area URL space.

---

## Q10. What is the difference between root `Controllers` and `Areas/Admin/Controllers`?

**Concepts**
- Root controllers — no area segment, view location under `/Views/`
- Area controllers — `[Area("Admin")]` required, view location under `Areas/Admin/Views/`
- Shared DI container and middleware pipeline across both
- Same controller class name legal in both locations

**Answer**

Root controllers in `/Controllers` serve the default site without an area route segment (`/Home/Index`), while area controllers in `Areas/Admin/Controllers` require the area prefix (`/Admin/Home/Index`) and carry `[Area("Admin")]`. Root controllers resolve views from `/Views/{Controller}/{Action}.cshtml`; area controllers resolve from `Areas/Admin/Views/{Controller}/{Action}.cshtml`. Both share the same DI container, middleware pipeline, and domain services — only routing and view location differ. The same controller class name can exist in both locations because the fully qualified type and route values differ; root controllers do not use `[Area]`.

---

## Q11. Can two controllers have the same name in different Areas?

**Concepts**
- Same controller name in different areas legal at compile time
- `area` route value disambiguating same-name controllers at request time
- Link generation and tests requiring explicit area route value
- Naming clarity trade-off — distinct names vs framework support for duplicates

**Answer**

Yes — MVC disambiguates by the `area` route value, so `Areas/Admin/Controllers/HomeController` and `Areas/Store/Controllers/HomeController` coexist as separate types matched by `/Admin/Home` vs `/Store/Home`. This is legal at compile time because they are different classes in different namespaces. Link generation and integration tests must include the `area` route value — omitting it always targets the root controller of that name if one exists. Duplicate names increase navigation and testing confusion; some teams rename to `AdminHomeController` for clarity, but the framework does not require it. Route order and explicit area segments determine which controller handles a request.

---

## Q12. Where should shared partials used by multiple Areas live?

**Concepts**
- Root `/Views/Shared/` as the cross-area partial location
- View engine fallback from area `Shared` to root `Shared`
- Area-specific partials staying in `Areas/{AreaName}/Views/Shared/`
- Razor Class Library for partials reused across multiple applications

**Answer**

Cross-area partials belong in the application root `/Views/Shared/`, which the view engine searches after the area's own `Shared` folder when resolving partial names. Area-specific partials with different markup or styling stay in `Areas/{AreaName}/Views/Shared/`. Invoke shared partials from area views with `<partial name="_OrderSummary" model="..." />` — resolution falls back to root `Views/Shared`. Copying the same partial into each area causes drift when one copy is updated and others are not. For reuse across multiple MVC applications, extract views into a Razor Class Library with embedded resources.

---

## Q13. How does layout resolution work for Area views?

**Concepts**
- Hierarchical layout search starting in the area view folders
- `Areas/{AreaName}/Views/_ViewStart.cshtml` guiding layout resolution
- Fallback to root `/Views/Shared/` when layout not found in the area
- Per-area `_Layout.cshtml` for distinct chrome without affecting other areas

**Answer**

Area views resolve layouts through a hierarchical search starting in the area's view folders, then falling back to root `/Views/Shared/`, guided by `Areas/{AreaName}/Views/_ViewStart.cshtml`. That file typically sets `Layout = "_Layout"`, resolving to `Areas/Admin/Views/Shared/_Layout.cshtml` first. If the layout is not found in the area, the view engine searches `/Views/Shared/_Layout.cshtml` at the application root. Each area can maintain distinct chrome — navigation, branding — via its own `_Layout.cshtml` without affecting other areas or the root site.

---

## Q14. What is `Areas/{AreaName}/Views/_ViewStart.cshtml` used for?

**Concepts**
- Area-scoped `_ViewStart.cshtml` applying layout directives to all area views
- Runs before each view under `Areas/{AreaName}/Views/` is rendered
- Individual views overriding layout with `Layout = null` or a different path
- Independent `_ViewStart` chain per area not affecting root `/Views/`

**Answer**

It applies layout and other view-start directives to every Razor view under that area's `Views` folder, equivalent to root `_ViewStart.cshtml` but scoped to the area subtree. Typical content: `@{ Layout = "_Layout"; }` pointing to the area's shared layout. It runs before each view in `Areas/Admin/Views/` is rendered — individual views can override with `Layout = null` or a different layout path. It does not affect root `/Views/` or other areas — each area has its own independent `_ViewStart` chain. It can also set common `ViewBag` title prefixes or other view-level defaults for the area.

---

## Q15. How do `_ViewImports` files scope between root Views and Area Views?

**Concepts**
- `_ViewImports.cshtml` applying hierarchically under its own directory
- Root `_ViewImports` not importing into `Areas/{AreaName}/Views/`
- Area-specific `_ViewImports` required for area namespaces and tag helpers
- Root `_ViewImports` coupling to area internals when area usings are placed there

**Answer**

`_ViewImports.cshtml` applies hierarchically to views in its directory and subdirectories — root `/Views/_ViewImports.cshtml` does not import into `Areas/Admin/Views/` unless a separate area `_ViewImports` exists there. Each area needs its own `Areas/Admin/Views/_ViewImports.cshtml` for area-specific `@using`, `@inject`, and `@addTagHelper` directives. Root `_ViewImports` should contain site-wide usings only — polluting it with area-specific namespaces couples root views to Admin internals. Shared tag helpers like `Microsoft.AspNetCore.Mvc.TagHelpers` are often duplicated in both root and area `_ViewImports` files. Think of each area's `_ViewImports` as a mini root for that view subtree with the same scoping rules.

---

## Q16. How do you apply authorization to an entire Area?

**Concepts**
- Area authorization convention registered in `AddControllersWithViews`
- Base controller pattern with `[Authorize]` inherited by all area controllers
- Per-controller `[Authorize]` insufficient — one omission breaks the boundary
- `[AllowAnonymous]` overriding area-wide requirements for login actions

**Answer**

Apply authorization structurally with an area authorization convention, a base controller class all area controllers inherit, or a global filter scoped to the area namespace — not by relying on per-controller `[Authorize]` alone, since one omission creates an unprotected endpoint. Register a convention: `options.Conventions.Add(new AreaAuthorizationConvention("Admin", "Administrator"))` in `AddControllersWithViews`. Alternatively, create `AdminBaseController` with `[Authorize(Policy = "Administrator")]` and inherit all Admin controllers from it. Use `[AllowAnonymous]` on specific controllers, such as `AccountController` login, to override area-wide requirements. Fallback policies affect the entire app — prefer targeted area conventions over a global deny-all when only some areas need protection.

---

## Q17. How do you map `/Admin` to a default dashboard action in the Admin area?

**Concepts**
- Dedicated route with fixed `Admin` prefix and area pinned in route defaults
- `defaults: new { area = "Admin" }` supplying area without a route segment
- Registration before the generic `{area:exists}` and default routes
- `[Area("Admin")]` still required on `DashboardController`

**Answer**

Register a dedicated route before the generic area route with a fixed area prefix and default controller/action values, pinning `area = "Admin"` in route defaults. The pattern `"Admin/{controller=Dashboard}/{action=Index}/{id?}"` with `defaults: new { area = "Admin" }` maps `/Admin` to `DashboardController.Index` in the Admin area. `DashboardController` must carry `[Area("Admin")]` and live under `Areas/Admin/Controllers/`. Register this route before the generic `{area:exists}` route and before the default site route to prevent `Admin` being captured as a root controller name. A simple alternative is `MapGet("/Admin", () => Results.Redirect("/Admin/Dashboard"))` for a lightweight shortcut without changing controller defaults.

---

## Q18. When should you use Areas vs Razor Class Libraries vs separate applications?

**Concepts**
- Areas for one-deployable-unit with distinct URL namespaces and shared domain services
- Razor Class Libraries for shared UI packages across multiple MVC hosts
- Separate applications for release independence, process isolation, or independent scaling
- Areas as routing and view organization — not process or security isolation

**Answer**

Use Areas for one deployable MVC app with distinct URL namespaces and shared domain logic; use Razor Class Libraries for shared UI packages across apps; use separate applications when release independence, scaling, or security isolation require different deployable boundaries. Areas fit when one team ships one container, sharing `DbContext`, authentication cookies, and services across Admin, Store, and Marketing surfaces. RCLs fit when multiple MVC hosts need identical partials, tag helpers, or embedded views without sharing business boundaries. Separate apps fit when Admin must be network-isolated, teams release on different cadences, or Store needs independent scale-out beyond what one process provides. Areas are a routing and view organization tool — they do not provide process isolation, separate databases, or independent deployment pipelines.

---

## Gotchas — Areas (Interview Traps)

---

#### Gotcha 1. Missing `[Area]` attribute on area controller causing 404

**Concepts**
- `[Area("Admin")]` — required attribute; marks controller as belonging to the named area
- Without `[Area]` — controller discovered as root controller; area route never matches
- `{area:exists}` constraint — only matches when the area segment names a registered area
- Folder structure alone — placing controller in `Areas/Admin/Controllers/` is insufficient

**Answer**

Placing a controller in `Areas/Admin/Controllers/` does not automatically associate it with the Admin area — the `[Area("Admin")]` attribute is required on the class. Without it, the controller is treated as a root controller and the `{area:exists}` route constraint never matches URLs containing "Admin" as an area segment. The controller may accidentally match a conventional root route, or it may return 404 for all area URLs. Every controller in an area folder must declare `[Area("AreaName")]` matching both its folder name and the area route registration.

---

#### Gotcha 2. Area route not registered with `{area:exists}` constraint

**Concepts**
- `{area:exists}` — route constraint; matches only when "Admin" is a registered area name
- Without constraint — `{area}` segment matches any string, can hijack non-area URLs
- `MapAreaControllerRoute` vs `MapControllerRoute` — the area-specific overload adds the constraint
- Registration before default route — area route must precede the catch-all `MapControllerRoute`

**Answer**

`app.MapControllerRoute("admin", "{area}/{controller}/{action}")` without the `{area:exists}` constraint matches any three-segment URL even when the first segment is not a registered area. A URL like `/Reports/Summary/Index` would match with `area=Reports`, `controller=Summary`, `action=Index` — hijacking routes intended for non-area controllers. The `app.MapAreaControllerRoute("admin_default", "Admin", "Admin/{controller=Dashboard}/{action=Index}/{id?}")` overload adds the `{area:exists}` constraint automatically, ensuring the route only activates when "Admin" is a registered area name. Always use `MapAreaControllerRoute` for area routes.

---

#### Gotcha 3. Link generation without `asp-area` defaulting to wrong area or root context

**Concepts**
- Tag helpers — use ambient area value from current request when `asp-area` is omitted
- Cross-area links — require explicit `asp-area="Admin"` and `asp-controller`
- Root-to-area link — `asp-area=""` to clear ambient area, plus explicit controller and area
- `Url.Action` — must pass `new { area = "Admin" }` for cross-area links

**Answer**

Inside an area view, `<a asp-action="Index" asp-controller="Dashboard">` generates a URL within the current area because the area name is an ambient route value. Omitting `asp-area` on a cross-area or root-to-area link generates the wrong URL silently — the link appears correct in the browser but navigates to the wrong controller. From a root view linking into an area, `asp-area="Admin"` must be specified. From an area view linking to the root, `asp-area=""` clears the ambient area. The same applies to `Url.Action("Index", "Dashboard", new { area = "Admin" })` — the area must be passed explicitly in route values.

---

#### Gotcha 4. Shared `_ViewImports.cshtml` not applying to area views

**Concepts**
- Root `_ViewImports.cshtml` at `Views/` — applies to all non-area views
- Area views — under `Areas/{Area}/Views/`, require their own `_ViewImports.cshtml`
- Tag helpers not registered in area — `<partial>`, `<form asp-action>` render as literal HTML
- `@using` directives — also not inherited from root `_ViewImports` unless area has its own

**Answer**

`_ViewImports.cshtml` at `Views/` applies only to views under the `Views/` folder — it does not propagate to `Areas/Admin/Views/`. Area views that use tag helpers such as `<form asp-action="Save">` or `<partial name="_Sidebar" />` will render them as literal unknown HTML elements if `Areas/Admin/Views/_ViewImports.cshtml` is missing. Every area's view folder needs its own `_ViewImports.cshtml` containing at minimum `@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers` and any shared `@using` namespace directives. The area scaffolding in Visual Studio creates this automatically, but manually created area folders often omit it.

---

#### Gotcha 5. Area view files not found because of wrong folder structure

**Concepts**
- Expected structure — `Areas/{AreaName}/Views/{ControllerName}/{ActionName}.cshtml`
- `Areas/Admin/Views/Shared/` — area-specific shared views, layouts, and partials
- `Views/Shared/` — root shared views available to all areas and root controllers
- Wrong folder — view resolution fails with "view not found" error at runtime

**Answer**

The MVC view engine looks for area views at `Areas/{Area}/Views/{Controller}/{Action}.cshtml` and falls back to `Areas/{Area}/Views/Shared/{View}.cshtml`, then to `Views/Shared/{View}.cshtml`. Placing views at `Areas/Admin/Admin/Views/` or `Areas/Admin/{Controller}.cshtml` (missing the `Views/` folder level) causes "view not found" errors at runtime while `dotnet run` works because the template path must match the convention exactly. The area layout file should be at `Areas/Admin/Views/Shared/_AdminLayout.cshtml` — placing it at `Views/Shared/` makes it shared with the root, which may expose admin layout to non-admin pages.

---

#### Gotcha 6. Default area route catching non-area requests first

**Concepts**
- Registration order — `MapAreaControllerRoute` must come before `MapControllerRoute`
- Default route registered first — `{controller=Home}/{action=Index}/{id?}` swallows area URLs
- Area segment treated as controller name — `Admin/Users/Index` becomes `controller=Admin`
- 404 for area routes — area controller never selected because default route matched first

**Answer**

When `app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}")` is registered before `app.MapAreaControllerRoute("admin_default", "Admin", ...)`, every URL beginning with "Admin/" matches the default route first — treating "Admin" as a controller name. The MVC framework looks for `AdminController` in the root controllers folder, finds none (because `AdminUsersController` is in the area with `[Area("Admin")]`), and returns 404. The fix is always to register area routes before the default catch-all route in `Program.cs`. The `{area:exists}` constraint on the area route ensures it does not accidentally match non-area URLs even if ordering is incorrect.

---

#### Gotcha 7. `_ViewStart.cshtml` at the root applying the wrong layout to area views

**Concepts**
- Root `_ViewStart.cshtml` — sets `Layout = "_Layout"` for all views in the `Views/` tree
- Area `_ViewStart.cshtml` — overrides the root setting for views under `Areas/{Area}/Views/`
- Missing area `_ViewStart.cshtml` — area views inherit root layout, rendering admin UI in public layout
- Override — `@{ Layout = "_AdminLayout"; }` in the area `_ViewStart.cshtml`

**Answer**

`Views/_ViewStart.cshtml` applies to all views under `Views/` and does not automatically extend to area views. However, if the area folder is missing its own `_ViewStart.cshtml`, the root `_ViewStart` does NOT apply either — area views receive no layout by default. This means area views render without any layout (unstyled) unless they explicitly set `@{ Layout = "_AdminLayout"; }` at the top of each file. The correct fix is to create `Areas/Admin/Views/_ViewStart.cshtml` with `@{ Layout = "_AdminLayout"; }` — this applies the admin layout to all views in the area and can be overridden per-view with `@{ Layout = null; }` when needed.

---

#### Gotcha 8. Cross-area redirects losing the area route value

**Concepts**
- `RedirectToAction("Index", "Dashboard")` — keeps current ambient area value
- `RedirectToAction("Index", "Home", new { area = "" })` — must clear area to redirect to root
- Ambient area in redirect — silently produces wrong URL if target is root
- `RedirectToAreaAction` helper — explicit area specification prevents ambient value leakage

**Answer**

`RedirectToAction("Index", "Home")` from inside an area action keeps the ambient `area` route value, generating a URL like `/Admin/Home/Index` rather than the intended root `/Home/Index`. The home controller at the root has no `[Area("Admin")]` attribute, so the routing never matches and a 404 results. To redirect from an area to a root controller, `RedirectToAction("Index", "Home", new { area = "" })` explicitly clears the area route value. To redirect to a different area, specify `new { area = "Reports" }`. Anytime a redirect target is outside the current area, the area route value must be explicitly provided.

---

#### Gotcha 9. Naming conflict between a root controller and an area controller

**Concepts**
- Root `HomeController` and area `HomeController` with `[Area("Admin")]` — different routing contexts
- Both define `GET Index` — routing selects based on area context, not ambiguous
- Without `[Area]` on area controller — both register as root; `AmbiguousMatchException`
- `{area:exists}` constraint — ensures area route only matches area-name segments

**Answer**

An `HomeController` in the root `Controllers/` folder and an `HomeController` in `Areas/Admin/Controllers/` with `[Area("Admin")]` can coexist because they live in different routing namespaces separated by the area. The root `HomeController` is selected for `/Home/Index` and the area `HomeController` is selected for `/Admin/Home/Index`. However, if the area `HomeController` is missing `[Area("Admin")]`, both are registered as root controllers and the router throws `AmbiguousMatchException` for any `Home` route. The `[Area]` attribute is what separates otherwise identical controller names into distinct routing domains — it must be present on every area controller.

---

#### Gotcha 10. Area-specific filters not applied to area controllers without explicit registration

**Concepts**
- Global filters — applied to all controllers including area controllers
- Area-specific authorization — requires `[Authorize]` on each area controller or a policy via `MapAreaControllerRoute`
- Convention-based filters — `AddControllers().AddMvcOptions(o => o.Filters.AddForArea(...))` — does not exist
- `[Authorize(Policy = "AdminOnly")]` per controller or route group — explicit, intentional

**Answer**

There is no built-in "apply filter to all controllers in an area" registration in ASP.NET Core MVC. Global filters registered with `builder.Services.AddControllersWithViews(o => o.Filters.Add<MyFilter>())` apply to every controller in every area. To restrict a filter to one area, add it explicitly to each area controller via attribute — `[Authorize(Policy = "AdminOnly")]` — or create a base controller class for the area that carries the attribute and inherit from it. Authorization policies wired via endpoint groups in `Program.cs` using `.RequireAuthorization()` after `MapAreaControllerRoute` apply the requirement to all area routes, which is the cleaner approach for area-wide authentication requirements.

---
## Scenario-Based Questions (Karat Format)

---

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

**Concepts**
- `[Area("Admin")]` attribute required for area route matching
- Folder placement vs attribute as distinct registration mechanisms
- `{area:exists}` constraint not matching a controller without the attribute
- Runtime 404 with successful compile — attribute is optional at build time

**Answer**

Folder placement alone does not register a controller with the area route — `DashboardController` must carry `[Area("Admin")]` so MVC associates it with the `{area:exists}` segment and resolves views under `Areas/Admin/Views/`. Without the attribute, MVC treats it as a root controller, the `{area:exists}` constraint never matches it, and every request to `/Admin/Dashboard` returns 404. The project compiles without the attribute because it is optional at compile time, which creates false confidence until the first HTTP request.

Add `[Area("Admin")]` at the class level, ensure the view exists at `Areas/Admin/Views/Dashboard/Index.cshtml`, and verify the area route is registered before the default route:

```csharp
[Area("Admin")]
public class DashboardController : Controller
{
    public IActionResult Index() => View();
}
```

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

**Concepts**
- Default route registered before area route — first segment consumed as controller
- Registration order determining match priority
- `{area:exists}` constraint bypassed when the default route wins
- Intermittent failures from different URL shapes in testing masking the bug

**Answer**

The default route is registered first, so `{controller=Home}` greedily consumes `Portal` as the controller name and `Orders` as the action — the area route never gets a chance to match. Traffic goes to root `OrdersController` or fails action lookup depending on URL shape. The "intermittent" behavior comes from testing `/Orders/History` (which works via the default route) separately from `/Portal/Orders/History` (which should go to the area controller but does not).

Register the area route before the default route — more specific first:

```csharp
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

Add integration tests that assert area URLs hit area controllers by checking route data `area=Portal`.

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

**Concepts**
- `asp-area` absent — Tag Helper ambient area may not persist after certain redirects
- Redirect without area route value leaving the Admin URL space
- Cross-area link generation requiring explicit `asp-area`
- Ambient values not guaranteed when layout is rendered from a non-area context

**Answer**

Tag helpers and `RedirectToAction` without an explicit area route value depend on ambient values from the current request. When the layout is rendered from a root request context — or after a redirect that did not carry the area value — those ambient values are absent and links like `asp-controller="Users"` generate `/Users/Index` instead of `/Admin/Users/Index`, targeting root controllers.

Add `asp-area="Admin"` on all area navigation links and pass the area explicitly on redirects:

```html
<a asp-area="Admin" asp-controller="Users" asp-action="Index">Users</a>
```

```csharp
return RedirectToAction(nameof(Index), new { area = "Admin" });
```

This makes the area part of the explicit route contract rather than relying on ambient context that can disappear.

---

#### Q4. (P) Product wants one `_OrderSummary.cshtml` partial reused by **Store**, **Admin**, and **Support** areas, but each area keeps its own chrome (`_Layout`). Where should shared markup live, how do area views reference it, and what breaks if each team copies the partial into their area folder?

**Concepts**
- `/Views/Shared/` as the cross-area partial location
- View engine search order — area `Shared` first, then root `Shared`
- Per-area `_ViewStart.cshtml` keeping area-specific layout independent
- Copy-per-area causing markup drift and hotfix misses

**Answer**

Place cross-area partials in `/Views/Shared/` at the application root; area views invoke them with `<partial name="_OrderSummary" model="..." />` — the view engine searches the area's own `Shared` folder first, then falls back to root `/Views/Shared`, so the single copy is found from any area view.

Per-area chrome stays isolated: each area's `Areas/{Area}/Views/_ViewStart.cshtml` sets `Layout = "_Layout"` resolving to `Areas/{Area}/Views/Shared/_Layout.cshtml`, which is independent of the shared partial. If each team copies `_OrderSummary.cshtml` into their area folder, three diverging copies accumulate — a bug fix in one misses the other two portals, and CSS class changes applied to one break consistency across the site. For partials with genuinely different markup per area, keep them in `Areas/{AreaName}/Views/Shared/` — that is the correct place for area-specific overrides. For partials reused across multiple applications, extract to a Razor Class Library with embedded views.

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

**Concepts**
- Controller not in the `Controllers/` subfolder — MVC area discovery failure
- View lookup path `Areas/{AreaName}/Views/{ControllerName}/{Action}.cshtml`
- Area folder convention required by controller discovery and view location expanders
- Tooling building `.cshtml` without catching routing or view resolution failures

**Answer**

Controllers must live under `Areas/Support/Controllers/` — placing `TicketsController.cs` directly under `Areas/Support/` removes it from conventional area controller discovery. Even if routing somehow reaches it, the view engine expects views at `Areas/Support/Views/Tickets/Index.cshtml`, not `Areas/Support/Views/Ticket/Index.cshtml` (note the casing and the missing `s`). The IDE compiles Razor syntax successfully because view compilation is separate from route and view-location resolution, which only fails at runtime.

Restore the required structure:

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

Ensure `TicketsController` carries `[Area("Support")]`.

---

#### Q6. (R) Root site and Admin area both define `HomeController`. `/` works; `/Admin/Home/Index` works; but `GET /Home/Index` from an Admin page and integration tests for "admin home" fail unpredictably. Explain the collision and how routing resolves it.

```csharp
// Controllers/HomeController.cs — public marketing site
public class HomeController : Controller { public IActionResult Index() => View(); }

// Areas/Admin/Controllers/HomeController.cs
[Area("Admin")]
public class HomeController : Controller { public IActionResult Index() => View(); }
```

**Concepts**
- Duplicate controller class names legal across areas — route data disambiguates
- `area` route value selecting the area type vs the root type
- Link generation omitting area producing wrong URL targeting root controller
- Integration tests needing `/Admin/Home/Index` and area route data assertions

**Answer**

Duplicate controller names are legal across areas because the `area` route value disambiguates them — `{ area = "Admin", controller = "Home" }` selects `Areas.Admin.Controllers.HomeController` while no area value selects root `Controllers.HomeController`. `/` uses the default route with no area, correctly reaching the root marketing controller. The problem is links and tests that use `asp-controller="Home"` or `GET /Home/Index` without specifying `area="Admin"` — those always resolve to the root controller, not the admin dashboard.

Every link from Admin views to Admin home needs `asp-area="Admin"` explicitly:

```html
<a asp-area="Admin" asp-controller="Home" asp-action="Index">Admin Home</a>
```

Integration tests must request `/Admin/Home/Index` or assert `RouteData.Values["area"] == "Admin"` to verify they hit the correct controller. Renaming to `AdminHomeController` eliminates the ambiguity entirely but is a naming choice, not a framework requirement.

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

**Concepts**
- `_ViewImports.cshtml` scoping hierarchically under its own directory only
- Root `_ViewImports` not importing into `Areas/Admin/Views/`
- Area-specific `_ViewImports` required for area namespaces and tag helpers
- Root `_ViewImports` coupling to area internals — wrong direction

**Answer**

`_ViewImports.cshtml` applies hierarchically to views under its own directory — root `/Views/_ViewImports.cshtml` does not import into `Areas/Admin/Views/`. The `@using MyApp.Areas.Admin.ViewModels` and any `@addTagHelper` for admin-specific tag helpers are invisible to the Admin area views, which is why `AdminDashboardVm` and `<admin-card>` are unknown at compile time. Additionally, placing admin-specific usings in the root file couples root views to Admin internals in the wrong direction.

Create `Areas/Admin/Views/_ViewImports.cshtml` with the area-specific directives:

```razor
@using MyApp.Areas.Admin.ViewModels
@addTagHelper *, MyApp.AdminTagHelpers
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```

Keep root `/Views/_ViewImports.cshtml` for site-wide usings only, and duplicate shared tag helper registrations in both files when they are genuinely global.

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

**Concepts**
- Per-controller `[Authorize]` insufficient — new controller without attribute bypasses security
- Area authorization convention providing structural default enforcement
- Base controller pattern as an alternative structural enforcement
- `[AllowAnonymous]` overriding area-wide requirements for login endpoints

**Answer**

Per-controller `[Authorize]` works until someone adds a controller without the attribute — there is no area-wide enforcement, so authorization regressions are one omission away. The correct structural fix is an area authorization convention or a base controller:

```csharp
builder.Services.AddControllersWithViews(options =>
{
    options.Conventions.Add(new AreaAuthorizationConvention("Admin", "Administrator"));
    options.Conventions.Add(new AreaAuthorizationConvention("Support", "SupportAgent"));
});
```

Alternatively, create `AdminBaseController : Controller` with `[Authorize(Policy = "Administrator")]` and require all Admin controllers to inherit from it. For Store, allow anonymous on catalog controllers and apply `[Authorize]` only on `CheckoutController` — area-level deny-all would break public browsing. Use `[AllowAnonymous]` on login actions in each area to override area-wide requirements. Add integration tests that verify all controllers under `Areas/Admin/Controllers/` return 401 or 403 without the required role.

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

**Concepts**
- Dedicated route with fixed `Admin` prefix and area pinned in route defaults
- `defaults: new { area = "Admin" }` supplying area without a URL segment
- Registration order — `admin_root` must precede the generic `{area:exists}` route
- `[Area("Admin")]` still required on `DashboardController`

**Answer**

The template `Admin/{controller=Dashboard}/{action=Index}/{id?}` with `defaults: { area = "Admin" }` correctly maps `/Admin` to `Dashboard/Index` and `/Admin/Users` to `Users/Index` in the Admin area — because the `area` value is pinned in route defaults rather than inferred from a URL segment. This route must be registered before the generic `{area:exists}` route; otherwise the generic route could win first. `DashboardController` must carry `[Area("Admin")]` and live under `Areas/Admin/Controllers/`.

| Request | Route matched | Result |
|---|---|---|
| `GET /Admin` | `admin_root` with defaults | `Admin/Dashboard/Index` |
| `GET /Admin/Users` | `admin_root`, controller=Users | `UsersController.Index` in Admin area |
| `GET /Admin/Dashboard/Index` | Either admin or generic area route | Works if controller is attributed |

Common 404 causes: missing `[Area("Admin")]`, `DashboardController` not in `Areas/Admin/Controllers/`, or route registered after a conflicting default route that captures `Admin` as a controller name.

---

#### Q10. (D) A monolith MVC app grows **Marketing**, **Store**, **Admin**, and **API** surfaces. Product asks whether to keep **Areas**, split into Razor Class Libraries, or separate deployable apps. What decision criteria matter for routing, auth boundaries, team ownership, and release cadence — and when do Areas become the wrong tool?

**Concepts**
- Areas for one deployable unit with shared domain services and distinct URL namespaces
- Razor Class Libraries for shared UI packages across multiple MVC hosts
- Separate applications for process isolation, independent scaling, or release cadence differences
- Area regret signals — circular references, 15+ areas with conflicting routes, mixed API controllers

**Answer**

Areas suit one deployable MVC app with shared `DbContext`, authentication cookies, and domain services across surfaces — when one team ships one container and the main variance is URL prefix and view chrome. They are the right tool when Marketing, Store, and Admin share the same database and business services because spinning them out adds deployment complexity without benefiting from isolation.

Razor Class Libraries fit when multiple MVC hosts need identical partials, tag helpers, or embedded views. They provide a shared UI package without sharing business boundaries, so they complement areas rather than replacing them.

Separate applications fit when Admin must be VPN-only or network-isolated, teams release on different cadences, Store needs aggressive scale-out beyond one process, or compliance mandates separate audit logs or databases. Signals that areas have outgrown their role: copy-paste `Program.cs` auth conventions per area, circular references between area view models, API controllers mixed into area folders, or 15+ areas with conflicting route templates that require careful ordering. At that point, deployable boundaries provide more clarity than route prefixes.
