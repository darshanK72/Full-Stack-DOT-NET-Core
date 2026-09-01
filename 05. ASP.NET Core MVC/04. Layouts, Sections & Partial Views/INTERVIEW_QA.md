# Layouts, Sections & Partial Views — Interview Q&A
> Back to [README](../README.md)

## Table of Contents

- [Chapter 04. Layouts, Sections & Partial Views](#chapter-04-layouts-sections-partial-views)
  - [Q1. What is a layout in ASP.NET Core MVC?](#chapter-04-layouts-sections-partial-views-q1)
  - [Q2. What does `@RenderBody()` do in a layout?](#chapter-04-layouts-sections-partial-views-q2)
  - [Q3. What is a section in Razor (`@section`)?](#chapter-04-layouts-sections-partial-views-q3)
  - [Q4. What is the difference between `@RenderSection("Scripts", re…](#chapter-04-layouts-sections-partial-views-q4)
  - [Q5. What is `_ViewStart.cshtml` and how does it apply layouts?](#chapter-04-layouts-sections-partial-views-q5)
  - [Q6. What is the difference between a layout and a partial view?](#chapter-04-layouts-sections-partial-views-q6)
  - [Q7. How do nested layouts work?](#chapter-04-layouts-sections-partial-views-q7)
  - [Q8. How are `@section` definitions passed from a view to a layou…](#chapter-04-layouts-sections-partial-views-q8)
  - [Q9. Can a view define the same section name twice? What happens?](#chapter-04-layouts-sections-partial-views-q9)
  - [Q10. What is the difference between `Html.PartialAsync` and the `…](#chapter-04-layouts-sections-partial-views-q10)
  - [Q11. How does Razor locate partial views?](#chapter-04-layouts-sections-partial-views-q11)
  - [Q12. How does partial view resolution differ in Areas?](#chapter-04-layouts-sections-partial-views-q12)
  - [Q13. What is the difference between `@RenderSection` and `@await …](#chapter-04-layouts-sections-partial-views-q13)
  - [Q14. Why pass a strongly typed model to a partial instead of `Vie…](#chapter-04-layouts-sections-partial-views-q14)
  - [Q15. How does a child view override the layout assigned in `_View…](#chapter-04-layouts-sections-partial-views-q15)
  - [Q16. What happens if a required section is not defined in a view?](#chapter-04-layouts-sections-partial-views-q16)
  - [Q17. What is the `Shared` folder under `Views` used for?](#chapter-04-layouts-sections-partial-views-q17)
  - [Q18. How does `_ViewStart` layout resolution work in Areas?](#chapter-04-layouts-sections-partial-views-q18)
- [Gotchas](#gotchas)
- [Scenario-Based Questions](#scenario-based-questions-karat-format)

---

## Chapter 04. Layouts, Sections & Partial Views

### Q1. What is a layout in ASP.NET Core MVC? {#chapter-04-layouts-sections-partial-views-q1}

What is a layout in ASP.NET Core MVC?

**Answer:** A layout is a Razor view that defines the shared page shell — HTML document structure, navigation, CSS/JS references, and footer — while individual views supply only their page-specific content. Layouts eliminate duplicating chrome across every `.cshtml` file and keep site-wide markup in one place.

- Layouts live typically in `Views/Shared/` (e.g., `_Layout.cshtml`) and are applied via `_ViewStart.cshtml` or a per-view `Layout` property.
- The layout calls `@RenderBody()` where the child view's markup is injected during rendering.
- Child views can optionally define named `@section` blocks that the layout renders at specific positions (e.g., scripts at the bottom).
- A view can opt out with `Layout = null` for standalone pages such as login or print views.

---

### Q2. What does `@RenderBody()` do in a layout? {#chapter-04-layouts-sections-partial-views-q2}

What does `@RenderBody()` do in a layout?

**Answer:** `@RenderBody()` is the placeholder in a layout where the rendering engine inserts the content of the currently executing view. Without it, the child view's markup is discarded and the page appears blank even though compilation succeeds.

- It is called exactly once per layout — the primary content channel for the view pipeline.
- Content placed outside `@section` blocks in the child view is captured into the body; section content is routed separately.
- Nested layouts each have their own `@RenderBody()` — the inner layout's body receives the page view, and the outer layout's body receives the fully rendered inner layout output.
- Missing `@RenderBody()` is a common silent bug: the layout renders headers/footers but no main content.

---

### Q3. What is a section in Razor (`@section`)? {#chapter-04-layouts-sections-partial-views-q3}

What is a section in Razor (`@section`)?

**Answer:** A section is a named content block defined in a child view with `@section SectionName { ... }` and rendered in the layout with `@RenderSection("SectionName")`. Sections let pages inject optional or required fragments — such as page-specific CSS, scripts, or a hero banner — into predetermined layout slots.

- Sections are declared in the view, not the layout; the layout decides where and whether to render them.
- Common pattern: `@section Scripts { <script src="~/js/page.js"></script> }` rendered at the bottom of `_Layout.cshtml`.
- Section content is captured during view execution and deferred until the layout reaches the matching `@RenderSection` call.
- Unlike `@RenderBody()`, a view can define multiple sections with different names.

---

### Q4. What is the difference between `@RenderSection("Scripts", required: true)` and `required: false`? {#chapter-04-layouts-sections-partial-views-q4}

What is the difference between `@RenderSection("Scripts", required: true)` and `required: false`?

**Answer:** The `required` parameter controls whether the layout throws at render time when a view omits that section. `required: true` enforces a contract — every view using the layout must define the section (even if empty); `required: false` silently skips rendering when the section is undefined.

- `required: true` throws `InvalidOperationException` if the view does not contain `@section Scripts { ... }` — useful when every page must participate in a shared script pipeline.
- `required: false` is appropriate for optional analytics, SEO meta tags, or page-specific styles that only some views need.
- Use `IsSectionDefined("Scripts")` in the layout to conditionally wrap optional section output without relying on exceptions.
- Mixed policies across a site cause inconsistent behavior: some pages crash, others silently omit scripts.

---

### Q5. What is `_ViewStart.cshtml` and how does it apply layouts? {#chapter-04-layouts-sections-partial-views-q5}

What is `_ViewStart.cshtml` and how does it apply layouts?

**Answer:** `_ViewStart.cshtml` runs before every view in its folder and subfolders, setting shared view properties — most commonly `Layout = "_Layout"`. It centralizes layout assignment so individual views do not repeat the same `Layout` line.

- The file executes in order from the view's folder upward; the closest `_ViewStart.cshtml` to the view wins for properties it sets.
- Root `Views/_ViewStart.cshtml` applies a default layout to all views unless a subfolder overrides it.
- A child folder's `_ViewStart.cshtml` can set `Layout = null` or a different layout name, affecting only views under that path.
- `_ViewStart` runs at compile/render time — it is not a view users see; it configures the view execution context.

---

### Q6. What is the difference between a layout and a partial view? {#chapter-04-layouts-sections-partial-views-q6}

What is the difference between a layout and a partial view?

**Answer:** A layout wraps an entire page and defines the outer document structure with `@RenderBody()` and sections; a partial view is a reusable Razor fragment rendered inside a parent view or layout. Layouts establish the page template; partials compose smaller UI pieces within that template.

- Layouts are applied automatically via `_ViewStart` or the `Layout` property; partials are invoked explicitly with `<partial>` or `Html.PartialAsync`.
- A page has one layout (or none); it can include many partials (`_ValidationScriptsPartial`, `_LoginPartial`).
- Layouts participate in the view-start pipeline and section forwarding; partials inherit the parent's `ViewData`/`ViewBag` unless given an explicit model.
- Partials do not replace `@RenderBody()` — they render inline wherever invoked.

---

### Q7. How do nested layouts work? {#chapter-04-layouts-sections-partial-views-q7}

How do nested layouts work?

**Answer:** Nested layouts chain by setting `Layout = "_ParentLayout"` in an intermediate layout file. The innermost view fills the inner layout's `@RenderBody()`, and the rendered inner layout output fills the outer layout's `@RenderBody()` — building the page inside-out.

- Example: `_AdminLayout.cshtml` sets `Layout = "_Layout"` and provides admin sidebar chrome around `@RenderBody()`.
- Each layout level must call `@RenderBody()` once; only the outermost layout typically contains `<html>` and `<head>`.
- Sections do not automatically bubble up — intermediate layouts must forward them with `@section Scripts { @RenderSection("Scripts", required: false) }`.
- Missing section forwarding in a middle layout is a common cause of scripts defined in a page never reaching the root layout.

---

### Q8. How are `@section` definitions passed from a view to a layout? {#chapter-04-layouts-sections-partial-views-q8}

How are `@section` definitions passed from a view to a layout?

**Answer:** When a view defines `@section Scripts { ... }`, Razor captures that block during view execution and stores it until the active layout calls `@RenderSection("Scripts")`. The layout then renders the captured content at that call site — typically after shared scripts so page-specific code runs last.

- Section matching is by name — the string in `@section` must match the argument to `@RenderSection`.
- If multiple layout levels exist, each intermediate layout must explicitly forward sections to the parent layout.
- Content outside any section becomes part of `@RenderBody()`; section content is excluded from the body stream.
- Sections execute in the context of the defining view, so they can reference the view's `@model` and local variables.

---

### Q9. Can a view define the same section name twice? What happens? {#chapter-04-layouts-sections-partial-views-q9}

Can a view define the same section name twice? What happens?

**Answer:** No — Razor allows each section name only once per view. Defining `@section Scripts` twice in the same view causes a compile-time error; sections do not merge like duplicate HTML tags.

- The compiler reports a duplicate section definition before the app runs.
- To combine script blocks, merge them into a single `@section Scripts { ... }` or extract shared scripts into a partial invoked inside one section.
- This strict rule differs from calling `Html.PartialAsync` multiple times, which is allowed.
- Nested layouts may each define forwarding sections — the restriction applies per view file, not per layout chain.

---

### Q10. What is the difference between `Html.PartialAsync` and the `<partial>` tag helper? {#chapter-04-layouts-sections-partial-views-q10}

What is the difference between `Html.PartialAsync` and the `<partial>` tag helper?

**Answer:** Both render a partial view asynchronously, but the `<partial>` tag helper is the recommended ASP.NET Core 8 approach — cleaner syntax, full DI integration, and consistent with other tag helpers. `Html.PartialAsync` is the older HTML helper API; functionally similar for simple cases.

- `<partial name="_MyPartial" model="Model.Items" />` avoids `@await` in many contexts and reads clearly in markup.
- `Html.PartialAsync` returns `IHtmlContent` and requires `@await` — it remains valid for dynamic partial names computed at runtime.
- Both pass an optional model and `ViewDataDictionary`; neither runs a controller — they only render existing Razor.
- Prefer `<partial>` in new code; use `PartialAsync` when the partial name is built programmatically in C# code within the view.

---

### Q11. How does Razor locate partial views? {#chapter-04-layouts-sections-partial-views-q11}

How does Razor locate partial views?

**Answer:** Razor searches a defined set of locations relative to the calling view, starting with the current controller's view folder, then `Views/Shared/`, using the partial name (with or without leading underscore). The first matching file wins.

- For a partial named `_ProductTile`, search paths include `Views/{Controller}/_ProductTile.cshtml` and `Views/Shared/_ProductTile.cshtml`.
- Explicit paths such as `~/Views/Shared/_ProductTile.cshtml` skip ambiguous discovery.
- Partial resolution is based on the **calling view's** context, not the layout's location.
- A partial in `Views/Shared/` with the same name as one in an Area can cause unexpected binding — use distinct names or fully qualified paths.

---

### Q12. How does partial view resolution differ in Areas? {#chapter-04-layouts-sections-partial-views-q12}

How does partial view resolution differ in Areas?

**Answer:** Area views add area-specific search paths via the area view location expander. Partials invoked from an Area view are searched under `Areas/{AreaName}/Views/{Controller}/`, then `Areas/{AreaName}/Views/Shared/`, then application `Views/Shared/`.

- Area isolation is not absolute — root `Views/Shared/` remains in the search order and can shadow Area intent if names collide.
- Use `~/Areas/Billing/Views/Shared/_LineItems.cshtml` when you must guarantee the Area version.
- Layout resolution in Areas follows a similar expanded path — Area `_ViewStart` controls which layout file is selected.
- View Components use separate discovery via `[AreaViewLocationFormats]` and are often clearer for Area-specific widgets with data loading.

---

### Q13. What is the difference between `@RenderSection` and `@await Html.PartialAsync`? {#chapter-04-layouts-sections-partial-views-q13}

What is the difference between `@RenderSection` and `@await Html.PartialAsync`?

**Answer:** `@RenderSection` renders content **defined by the child view** into a layout slot — a top-down contract between view and layout. `Html.PartialAsync` renders a **separate Razor file** inline — a horizontal composition of reusable fragments, independent of the layout section pipeline.

- Sections are declared in the page view and consumed by the layout; partials are standalone files invoked from any view or layout.
- Sections support the optional/required flag; partials always render when called (or throw if not found).
- Partials accept an explicit model parameter; sections inherit the defining view's scope.
- Use sections for page-specific scripts/styles destined for layout placeholders; use partials for shared UI fragments used across many pages.

---

### Q14. Why pass a strongly typed model to a partial instead of `ViewBag`? {#chapter-04-layouts-sections-partial-views-q14}

Why pass a strongly typed model to a partial instead of `ViewBag`?

**Answer:** A strongly typed model gives compile-time checking, IntelliSense, and explicit data contracts — the partial declares `@model MyType` and receives only what it needs. `ViewBag` is dynamic, untyped, and invisible to refactoring tools, making partials fragile and hard to test.

- `model="Model.Lines"` documents exactly what the partial requires without relying on magic string keys.
- Strong typing prevents null-reference and typo errors that `ViewBag.Orders` hides until runtime.
- Unit tests and view components can construct the model directly; `ViewBag` requires dictionary setup.
- Reserve `ViewBag`/`ViewData` for rare layout-level messages — not for structured data passed to partials.

---

### Q15. How does a child view override the layout assigned in `_ViewStart`? {#chapter-04-layouts-sections-partial-views-q15}

How does a child view override the layout assigned in `_ViewStart`?

**Answer:** A view overrides `_ViewStart` by setting the `Layout` property at the top of the `.cshtml` file — for example `@{ Layout = "_PrintLayout"; }` or `@{ Layout = null; }`. The view-level assignment takes precedence over `_ViewStart` for that view only.

- Override syntax: `@{ Layout = "~/Views/Shared/_AdminLayout.cshtml"; }` using a fully qualified path when needed.
- `Layout = null` renders the view as a standalone HTML page without any layout wrapper.
- `_ViewStart` still runs first; the view's own `Layout` assignment replaces the value `_ViewStart` set.
- Per-action layout selection can also be done in the controller with `return View("Name", "_LayoutName")` overloads.

---

### Q16. What happens if a required section is not defined in a view? {#chapter-04-layouts-sections-partial-views-q16}

What happens if a required section is not defined in a view?

**Answer:** When the layout calls `@RenderSection("Scripts", required: true)` and the view omits `@section Scripts`, Razor throws `InvalidOperationException` at render time — the page fails with a 500 error. This is intentional enforcement of a layout contract.

- The error identifies the missing section name and the layout file — it only appears when that view is rendered, not at build time.
- Fix by adding `@section Scripts { }` (even empty) or changing the layout to `required: false` if the section is truly optional.
- Smoke-testing every view against its layout prevents production surprises when a new layout adds a required section.
- Alternatively, provide default scripts in the layout and use `IsSectionDefined` to append page-specific scripts optionally.

---

### Q17. What is the `Shared` folder under `Views` used for? {#chapter-04-layouts-sections-partial-views-q17}

What is the `Shared` folder under `Views` used for?

**Answer:** `Views/Shared/` holds views shared across controllers — layouts (`_Layout.cshtml`), partials (`_ValidationScriptsPartial.cshtml`), error pages, and editor templates. Razor searches here when a view or partial is not found in the controller-specific folder.

- Layout files, `_ViewImports.cshtml`, and `_ViewStart.cshtml` commonly live in or apply to Shared.
- Partials used by multiple controllers belong in Shared to avoid duplication under each controller folder.
- Editor templates and display templates for `Html.EditorFor`/`DisplayFor` also reside under `Shared/EditorTemplates/` and `Shared/DisplayTemplates/`.
- Area-specific shared views go in `Areas/{AreaName}/Views/Shared/` for Area-scoped reuse.

---

### Q18. How does `_ViewStart` layout resolution work in Areas? {#chapter-04-layouts-sections-partial-views-q18}

How does `_ViewStart` layout resolution work in Areas?

**Answer:** Area views run `Areas/{AreaName}/Views/_ViewStart.cshtml` first, then fall back to root `Views/_ViewStart.cshtml` if the Area file does not exist or does not set all properties. Layout name resolution uses area-expanded paths — `_Layout` may resolve to `Areas/Admin/Views/Shared/_Layout.cshtml` before root Shared.

- Set Area `_ViewStart` explicitly: `Layout = "~/Areas/Admin/Views/Shared/_Layout.cshtml"` to avoid accidentally picking the root site layout.
- An unused `Areas/Admin/Views/Shared/_Layout.cshtml` has no effect until `_ViewStart` references it.
- Asset paths in Area layouts should use `~/` application-root syntax — not relative `../css/admin.css` — to avoid 404s when the wrong layout renders.
- Nested `_ViewStart` files inside `Areas/Admin/Views/Dashboard/` can further override layout for that subfolder only.

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

#### Q1. (R) Review this view setup. The About page renders raw HTML with no site navigation, CSS, or footer — Home/Index and Contact look correct.

`Views/_ViewStart.cshtml`:

```cshtml
@{
    Layout = "_Layout";
}
```

`Views/Home/_ViewStart.cshtml` (added during a "Home-only experiment"):

```cshtml
@{
    Layout = null;
}
```

`Views/Home/About.cshtml`:

```cshtml
<h1>About Us</h1>
<p>Team bios and mission statement.</p>
```

`Views/Shared/_Layout.cshtml` exists and references `~/css/site.css`.

---

**Answer:**

**Answer:** A nested `Views/Home/_ViewStart.cshtml` sets `Layout = null`, which overrides the root `_ViewStart` for every view under `Views/Home/` — About inherits that null layout and renders without `_Layout.cshtml` even though other folders still use the root default.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| View pipeline | Child `_ViewStart` sets `Layout = null` | All Home views skip layout — missing chrome and CSS |
| Cascade | `_ViewStart` runs closest-folder-first | Easy to miss during code review — only Home folder affected |
| Maintainability | "Experiment" file left in repo | Intermittent layout bugs by folder path |

**Fix (priority order):**

1. Remove `Views/Home/_ViewStart.cshtml` or change it to `Layout = "_Layout"` if Home needs the same chrome.
2. If Home truly needs a different layout, set `Layout = "_HomeLayout"` explicitly — not `null` — and ensure that layout exists in `Views/Shared/`.
3. Document `_ViewStart` cascade in team conventions: only one root `_ViewStart` unless a subfolder deliberately overrides.

**Production takeaway:** `_Layout not applied` is often a `_ViewStart` cascade bug, not a missing layout file — Karat tests whether you trace the view start chain before blaming the view itself.

---

---

#### Q2. (R) Review this layout and checkout view. The page works in dev until marketing adds a new checkout step — then some pages throw at runtime and others silently omit analytics scripts.

`Views/Shared/_Layout.cshtml`:

```cshtml
<!DOCTYPE html>
<html>
<head>
    <title>@ViewData["Title"] - Shop</title>
    @RenderSection("HeadScripts", required: false)
</head>
<body>
    @await Html.PartialAsync("_Nav")
    @RenderBody()
    @RenderSection("Scripts", required: true)
</body>
</html>
```

`Views/Checkout/Confirm.cshtml`:

```cshtml
@{
    ViewData["Title"] = "Confirm Order";
}
<h1>Confirm</h1>
<p>Order #@Model.OrderId</p>
```

`Views/Checkout/Payment.cshtml` defines `@section Scripts { ... }` but Confirm does not.

---

**Answer:**

**Answer:** `@RenderSection("Scripts", required: true)` throws `InvalidOperationException` when Confirm.cshtml omits the section, while `required: false` on HeadScripts silently skips analytics with no error — the `required` flag is the trap.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | `Scripts` section marked `required: true` | Confirm page crashes when section undefined |
| Silent omission | `HeadScripts` is `required: false` | Missing analytics/SEO tags with no exception |
| Consistency | Mixed required flags across checkout flow | Some steps fail loudly, others fail quietly |

**Fix (priority order):**

1. Decide contract: if every page needs footer scripts, keep `required: true` and add `@section Scripts { }` (even empty) to every view — or default scripts in layout and use `required: false` with `@if (IsSectionDefined("Scripts"))`.
2. For optional HeadScripts, use `required: false` but document which pages must define it; consider a shared partial `_AnalyticsScripts.cshtml` included from layout instead of a section.
3. Add integration/smoke tests that hit every checkout view — section errors only appear at render time.

**Production takeaway:** `required: false` is not "optional convenience" in prod — it means content can vanish with zero signal; `required: true` means one forgotten section takes down a page.

---

---

#### Q3. (D) A product dashboard needs a reusable "Recent Orders" panel on three pages. It runs a scoped repository query, shows a loading skeleton, and must be unit-testable without spinning up the full layout pipeline. The team proposes `@await Html.PartialAsync("_RecentOrders")` with data stuffed into `ViewBag.Orders`. What would you choose instead, and why?

---

**Answer:**

**Answer:** Use a **View Component** (`RecentOrdersViewComponent` + `InvokeAsync`) — partials are synchronous rendering fragments with no invocation lifecycle, while view components support async data loading, explicit parameters, DI, and isolated testing.

- **Partial** fits static markup or data the parent view already fetched; `ViewBag` is untyped, not refactor-safe, and hides dependencies.
- **View Component** injects `IOrderRepository`, runs `InvokeAsync(int count)` async, returns a strongly typed view (`Default.cshtml`), and can be invoked via `<vc:recent-orders count="5" />` or `@await Component.InvokeAsync(...)`.
- Unit-test the component class directly; no layout or full Razor page required.
- Reserve partials for small, parent-supplied fragments (`_ValidationScriptsPartial`); use view components when the widget owns its query or state.

**Production takeaway:** Karat uses partial-vs-component to test whether you reach for the right reuse primitive — partials render; view components **execute**.

---

---

#### Q4. (M) An admin section uses nested layouts: site chrome in `_Layout.cshtml`, admin sidebar in `_AdminLayout.cshtml`, and page content in individual views. Walk through how Razor resolves `Layout`, `@RenderBody()`, and `@section` definitions across the two layout files — where does each `@RenderSection` call execute?

`Views/Shared/_Layout.cshtml`:

```cshtml
<body>
    @RenderBody()
    @RenderSection("Scripts", required: false)
</body>
```

`Views/Shared/_AdminLayout.cshtml`:

```cshtml
@{
    Layout = "_Layout";
}
<aside>@await Html.PartialAsync("_AdminNav")</aside>
<main>@RenderBody()</main>
@section Scripts {
    @RenderSection("Scripts", required: false)
}
```

`Areas/Admin/Views/Reports/Index.cshtml`:

```cshtml
@{
    Layout = "_AdminLayout";
}
<h1>Reports</h1>
@section Scripts {
    <script src="~/js/reports.js"></script>
}
```

---

**Answer:**

**Answer:** Razor builds an inside-out chain: Index content fills `_AdminLayout`'s `@RenderBody()`, then `_AdminLayout`'s output (including forwarded sections) fills `_Layout`'s `@RenderBody()` — child `@section Scripts` is captured once and forwarded by the inner layout's `@section Scripts { @RenderSection("Scripts", required: false) }`.

Execution order:

1. **Index.cshtml** — defines page markup and `@section Scripts { reports.js }`.
2. **`_AdminLayout.cshtml`** — `Layout = "_Layout"`; its `@RenderBody()` is replaced by Index body; its `@section Scripts` block **forwards** the child Scripts section upward (required pattern for nested layouts).
3. **`_Layout.cshtml`** — `@RenderBody()` receives the fully rendered admin shell (sidebar + main); `@RenderSection("Scripts")` at the bottom outputs the forwarded scripts once.

If `_AdminLayout` omits the `@section Scripts { @RenderSection(...) }` forwarder, scripts defined in Index never reach `_Layout` — a common nested-layout bug. Sections are **not** automatically bubbled through intermediate layouts.

**Production takeaway:** Nested layouts require explicit section forwarding in every intermediate layout — one missing `@RenderSection` bridge drops scripts or styles silently.

---

---

#### Q5. (R) Review this view. Build fails locally with a Razor compilation error; the developer claims "the second Scripts block should merge."

`Views/Products/Edit.cshtml`:

```cshtml
@model ProductEditViewModel

@section Scripts {
    <script src="~/lib/jquery-validation/dist/jquery.validate.min.js"></script>
}

<form asp-action="Edit" method="post">...</form>

@section Scripts {
    <script src="~/js/product-editor.js"></script>
}
```

---

**Answer:**

**Answer:** Razor allows each section name **once per view** — two `@section Scripts` blocks do not merge; the compiler reports a duplicate section definition error at build time.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `@section Scripts` defined twice in same view | Build fails — no merge semantics |
| Design | Assumption that sections accumulate like `@Html.Partial` calls | Blocks deployment until consolidated |

**Fix (priority order):**

1. Merge into one section:

```cshtml
@section Scripts {
    <script src="~/lib/jquery-validation/dist/jquery.validate.min.js"></script>
    <script src="~/js/product-editor.js"></script>
}
```

2. Or extract shared scripts to a partial `_ProductEditScripts.cshtml` and invoke once inside a single section.
3. For nested layouts, forward with one `@RenderSection` per layout level — still one definition per **view**, not per layout file.

**Production takeaway:** "Section defined twice" fails at compile time — unlike HTML `<script>` tags, Razor sections have strict single-definition rules.

---

---

#### Q6. (R) Review this Area view. The partial renders empty — no order lines — even though the controller passed a populated model. Same partial works on a non-Area page.

`Areas/Billing/Views/Invoices/Details.cshtml`:

```cshtml
@model InvoiceDetailsViewModel

<h1>Invoice @Model.InvoiceNumber</h1>
<partial name="_LineItems" model="Model.Lines" />
```

`Areas/Billing/Controllers/InvoicesController.cs` returns `View(model)` with `Lines` populated.

Project also has:

- `Views/Shared/_LineItems.cshtml` (generic stub, `@model IEnumerable<LineItem>`)
- `Areas/Billing/Views/Shared/_LineItems.cshtml` (billing-specific markup)

Developer expected Area-local partial to win automatically.

---

**Answer:**

**Answer:** `<partial name="_LineItems" />` resolves from the **calling view's** search paths — for an Area view, discovery order includes `Areas/Billing/Views/Shared/`, then `Areas/Billing/Views/Invoices/`, then **application** `Views/Shared/` — if the wrong file wins or the stub in root Shared matches first in some configurations, you get empty/wrong markup; explicit paths avoid ambiguity.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Resolution | Implicit partial name without path | May bind to `Views/Shared/_LineItems.cshtml` stub instead of Area version |
| Areas | Area view location expander adds paths but order matters | Billing-specific partial ignored — empty or generic output |
| Model | Partial receives `model="Model.Lines"` | If wrong partial `@model` type differs, binding may render nothing |

**Fix (priority order):**

1. Use explicit path: `<partial name="~/Areas/Billing/Views/Shared/_LineItems.cshtml" model="Model.Lines" />`.
2. Or rename Area partial to `_BillingLineItems.cshtml` to avoid name collision with root Shared.
3. Prefer view components for Area-specific widgets — `View()` discovery uses `[AreaViewLocationFormats]`.

Partial search order (simplified): current view folder → `{Area}/Views/Shared` → `Views/Shared`. Same name in root Shared can shadow Area intent when developers assume Area isolation is automatic.

**Production takeaway:** Partial path resolution is view-context-relative — Area pages do not magically ignore root `Views/Shared` duplicates with the same name.

---

---

#### Q7. (P) You inherit an MVC app where `Areas/Admin/Views/Shared/_Layout.cshtml` exists but Admin pages still use the root `_Layout` and break relative asset paths (`../css/admin.css` 404). Trace the layout resolution path for Area views and what to fix in `_ViewStart` / view `Layout` assignments.

`Areas/Admin/Views/_ViewStart.cshtml`:

```cshtml
@{
    Layout = "_Layout";
}
```

`Areas/Admin/Views/Dashboard/Index.cshtml`:

```cshtml
@{
    ViewData["Title"] = "Admin Dashboard";
}
<h1>Dashboard</h1>
```

Root `Views/Shared/_Layout.cshtml` links `~/css/site.css`. Admin layout links `~/css/admin.css` and includes `_AdminNav`.

---

**Answer:**

**Answer:** `Areas/Admin/Views/_ViewStart.cshtml` sets `Layout = "_Layout"`, which resolves via view location expanders to **`Views/Shared/_Layout.cshtml`** (root) — the Area-local `_Layout.cshtml` is never selected unless you use a distinct name or fully qualified path.

Resolution path for `Areas/Admin/Views/Dashboard/Index.cshtml`:

1. Run `Areas/Admin/Views/_ViewStart.cshtml` → `Layout = "_Layout"`.
2. Razor searches layout locations: `Areas/Admin/Views/Shared/_Layout.cshtml`, then `Views/Shared/_Layout.cshtml` — **first match wins**; if both exist, Area Shared should win, but identical names with wrong content or a typo in Area file causes root to match.
3. Root layout references `~/css/site.css`; admin assets in Area layout use paths that 404 when root layout renders.

Fixes:

- Set Area `_ViewStart` to `Layout = "~/Areas/Admin/Views/Shared/_Layout.cshtml"` or rename to `_AdminLayout` and reference explicitly.
- Use `~/css/admin.css` (application-root-relative) — never `../css/admin.css` in layouts.
- Verify `AddMvc()` / `AddControllersWithViews()` registers Area route `{area:exists}/...`.

**Production takeaway:** Layout in Areas requires explicit Area `_ViewStart` — an unused `Areas/Admin/Views/Shared/_Layout.cshtml` does nothing until the cascade points to it.

---

---

#### Q8. (P) A catalog page renders 40 product tiles; each tile is a partial that injects `IProductService` and calls `GetRatingAsync(productId)` — 40 service calls per page load. Review the architecture: what breaks under load, and what MVC/Razor pattern reduces work without abandoning partial reuse?

```cshtml
@* Views/Catalog/Index.cshtml *@
@model CatalogPageViewModel

<div class="grid">
@foreach (var p in Model.Products)
{
    <partial name="_ProductTile" model="p" />
}
</div>
```

```cshtml
@* Views/Shared/_ProductTile.cshtml *@
@model ProductSummary
@inject IProductService ProductService
@{
    var rating = await ProductService.GetRatingAsync(Model.Id);
}
<div class="tile">@Model.Name — @rating.Stars stars</div>
```

---

**Answer:**

**Answer:** Forty async partials each hit the database independently — classic **N+1 at the view layer** — causing latency spikes, connection-pool pressure, and thread churn; partials have no batching and duplicate DI work per invocation.

- **Symptom under load:** p95 page time scales with tile count; DB QPS = tiles × concurrent users.
- **Fix (priority):** Controller or view-model builder loads all ratings in **one query** (`GetRatingsAsync(productIds)`) and maps into `ProductSummary.Rating` before the view runs — partial becomes dumb markup only.
- **Alternative:** View Component with batched fetch keyed by ids on first invoke (still prefer controller/service layer batching).
- **Partial reuse kept:** `_ProductTile.cshtml` still renders `@Model.Stars` — data fetched upstream.
- Avoid `@inject` + async DB in partials for list items; cache ratings at service layer if hot path.

**Production takeaway:** Performance of many partials is not about Razor compilation — it is about **data access inside each partial**; Karat tests moving queries out of the view tree.

---

---

#### Q9. (R) Review this new layout copied from a static HTML template. Views compile but render blank white pages in the browser — no exception in logs.

`Views/Shared/_MarketingLayout.cshtml`:

```cshtml
<!DOCTYPE html>
<html>
<head>
    <title>@ViewData["Title"]</title>
    @RenderSection("Styles", required: false)
</head>
<body>
    @await Html.PartialAsync("_MarketingHeader")
    @RenderSection("Hero", required: false)
    @await Html.PartialAsync("_MarketingFooter")
    @RenderSection("Scripts", required: false)
</body>
</html>
```

`Views/Landing/Index.cshtml`:

```cshtml
@{
    Layout = "_MarketingLayout";
    ViewData["Title"] = "Welcome";
}
@section Hero {
    <h1>Launch offer</h1>
}
<p>Sign up today.</p>
```

---

**Answer:**

**Answer:** `_MarketingLayout.cshtml` never calls `@RenderBody()`, so view content (including the `<p>Sign up today.</p>` outside sections) is discarded — only partials and optional sections render, producing an nearly empty page with no error.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Layout contract | Missing `@RenderBody()` | Primary view markup never appears — blank main content |
| Sections only | Content outside `@section` blocks ignored | Body text in Index lost unless inside a section |
| Debugging | No exception — valid Razor | Silent functional bug, hard to spot in QA |

**Fix (priority order):**

1. Add `@RenderBody()` where main content belongs (between header and footer):

```cshtml
@await Html.PartialAsync("_MarketingHeader")
@RenderSection("Hero", required: false)
@RenderBody()
@await Html.PartialAsync("_MarketingFooter")
```

2. Move inline page copy into `@RenderBody()` placement or into a `@section Hero` / main section intentionally.
3. Smoke-test new layouts with minimal view containing non-section content to verify body renders.

**Production takeaway:** Missing `@RenderBody()` is the layout equivalent of a controller action that never returns the model — compile succeeds, output is wrong.

---

---
