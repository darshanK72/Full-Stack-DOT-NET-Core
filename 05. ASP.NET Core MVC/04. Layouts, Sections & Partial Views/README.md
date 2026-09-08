# Layouts, Sections & Partial Views — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is a layout in ASP.NET Core MVC?](#q1-what-is-a-layout-in-aspnet-core-mvc)
2. [Q2. What does `@RenderBody()` do in a layout?](#q2-what-does-renderbody-do-in-a-layout)
3. [Q3. What is a section in Razor (`@section`)?](#q3-what-is-a-section-in-razor-section)
4. [Q4. What is the difference between `@RenderSection("Scripts", required: true)` and `required: false`?](#q4-what-is-the-difference-between-rendersectionscripts-required-true-and-required-false)
5. [Q5. What is `_ViewStart.cshtml` and how does it apply layouts?](#q5-what-is-_viewstartcshtml-and-how-does-it-apply-layouts)
6. [Q6. What is the difference between a layout and a partial view?](#q6-what-is-the-difference-between-a-layout-and-a-partial-view)
7. [Q7. How do nested layouts work?](#q7-how-do-nested-layouts-work)
8. [Q8. How are `@section` definitions passed from a view to a layout?](#q8-how-are-section-definitions-passed-from-a-view-to-a-layout)
9. [Q9. Can a view define the same section name twice? What happens?](#q9-can-a-view-define-the-same-section-name-twice-what-happens)
10. [Q10. What is the difference between `Html.PartialAsync` and the `<partial>` tag helper?](#q10-what-is-the-difference-between-htmlpartialasync-and-the-partial-tag-helper)
11. [Q11. How does Razor locate partial views?](#q11-how-does-razor-locate-partial-views)
12. [Q12. How does partial view resolution differ in Areas?](#q12-how-does-partial-view-resolution-differ-in-areas)
13. [Q13. What is the difference between `@RenderSection` and `@await Html.PartialAsync`?](#q13-what-is-the-difference-between-rendersection-and-await-htmlpartialasync)
14. [Q14. Why pass a strongly typed model to a partial instead of `ViewBag`?](#q14-why-pass-a-strongly-typed-model-to-a-partial-instead-of-viewbag)
15. [Q15. How does a child view override the layout assigned in `_ViewStart`?](#q15-how-does-a-child-view-override-the-layout-assigned-in-_viewstart)
16. [Q16. What happens if a required section is not defined in a view?](#q16-what-happens-if-a-required-section-is-not-defined-in-a-view)
17. [Q17. What is the `Shared` folder under `Views` used for?](#q17-what-is-the-shared-folder-under-views-used-for)
18. [Q18. How does `_ViewStart` layout resolution work in Areas?](#q18-how-does-_viewstart-layout-resolution-work-in-areas)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What is a layout in ASP.NET Core MVC?

**Concepts**
- Shared page shell containing HTML structure and site chrome
- `@RenderBody()` as the child view content injection point
- `_ViewStart.cshtml` for automatic layout assignment
- Named `@section` blocks for per-page slot injection
- `Layout = null` opt-out for standalone pages

**Answer**

The reason layouts exist is to eliminate duplicating HTML document structure, navigation, CSS/JS references, and footer markup across every `.cshtml` file — the layout owns the shared chrome while individual views supply only their page-specific content. Layouts typically live in `Views/Shared/` as `_Layout.cshtml` and are applied either through `_ViewStart.cshtml` (which sets `Layout = "_Layout"` for all views in the folder tree) or by a per-view `Layout` property. The layout calls `@RenderBody()` at the spot where the child view's markup should appear, and child views can additionally push named `@section` blocks to predetermined positions such as a scripts slot at the bottom of `<body>`. When a page like a login or print view should render without any shared chrome, setting `Layout = null` at the top of that view opts it out entirely.

---

## Q2. What does `@RenderBody()` do in a layout?

**Concepts**
- Primary content channel for the view pipeline
- Body vs section content routing
- Single-call constraint per layout
- Nested layout `@RenderBody()` chaining
- Silent blank page when `@RenderBody()` is missing

**Answer**

`@RenderBody()` is the placeholder in a layout where the rendering engine inserts everything the child view wrote outside of `@section` blocks — it is the primary content channel between a view and its layout. The call must appear exactly once per layout file; content placed outside sections in the child view is captured into the body stream, while section content is routed separately and consumed by the matching `@RenderSection` call. In nested layouts each level has its own `@RenderBody()`, so the page view fills the inner layout's body and the inner layout's rendered output fills the outer layout's body, building the page inside-out. The most common silent bug is omitting `@RenderBody()` from a new layout — the headers and footers render but the main content disappears with no exception and no hint in the logs.

---

## Q3. What is a section in Razor (`@section`)?

**Concepts**
- Named content block defined in a child view
- Deferred capture and render at layout call site
- `@RenderSection("Name")` consuming the captured block
- Section content excluded from `@RenderBody()` stream
- Multiple distinct sections per view

**Answer**

A section is a named content block defined in a child view with `@section SectionName { ... }` and consumed in the layout with `@RenderSection("SectionName")`. The key mechanism is deferred capture: Razor captures the section block during view execution and holds it until the layout reaches the matching `@RenderSection` call, which means page-specific scripts declared in the view can end up rendered at the bottom of `<body>` in the layout where they belong. Unlike `@RenderBody()`, a view can define multiple sections with different names, so a page might define both a `Scripts` section and a `Styles` section for different layout slots. Content inside a section is excluded from the body stream, which is why mixing section and non-section content in a view produces predictable placement without collisions.

---

## Q4. What is the difference between `@RenderSection("Scripts", required: true)` and `required: false`?

**Concepts**
- `required: true` as a mandatory layout contract
- `required: false` for optional sections with silent skip
- `InvalidOperationException` on missing required section
- `IsSectionDefined()` for conditional output without crashing
- Mixed required policies causing inconsistent page behavior

**Answer**

The `required` parameter determines whether the layout enforces that every consuming view defines the section. `required: true` means the layout and view have a hard contract — any view that omits `@section Scripts { ... }` throws `InvalidOperationException` at render time, producing a 500 response. This is appropriate when every page must participate in a shared pipeline such as injecting core scripts. `required: false` silently skips rendering when the section is absent, which suits optional slots like analytics snippets or per-page hero banners where only some views need to contribute. Rather than relying on the exception to surface omissions, I prefer using `IsSectionDefined("Scripts")` in the layout to wrap optional section output with fallback defaults, since `required: false` can silently drop content that developers assume will always appear.

---

## Q5. What is `_ViewStart.cshtml` and how does it apply layouts?

**Concepts**
- Pre-view execution for shared property initialization
- Closest-folder-wins cascade order
- Root `Views/_ViewStart.cshtml` as default for all views
- Subfolder override for area or section-specific layouts
- `_ViewStart` as a non-rendered configuration file

**Answer**

`_ViewStart.cshtml` runs before every view in its folder and subfolders, making it the right place to set shared view properties like `Layout = "_Layout"`. The cascade works closest-folder-first: if `Views/Home/_ViewStart.cshtml` exists, it runs before root `Views/_ViewStart.cshtml` for Home views, so a subfolder can set a different layout or `Layout = null` without affecting the rest of the app. Root `Views/_ViewStart.cshtml` establishes the default for all views that do not have a more specific override. Because `_ViewStart` executes at render time rather than compile time, a missing or mismatch only surfaces when those views are actually requested — so smoke-testing every subfolder after adding a new `_ViewStart` file is worth the effort.

---

## Q6. What is the difference between a layout and a partial view?

**Concepts**
- Layout as outer page template with `@RenderBody()`
- Partial view as reusable inline rendering fragment
- Automatic layout application via `_ViewStart` vs explicit partial invocation
- Single layout per page vs multiple partials
- Section forwarding in layouts vs inline partial output

**Answer**

The key distinction is that a layout wraps the entire page and defines the outer document structure — it contains `@RenderBody()` where the current view's content appears — while a partial view is a reusable Razor fragment rendered inline wherever it is invoked. A page has exactly one layout (or none) applied automatically through `_ViewStart`, but it can include many partials (`_ValidationScriptsPartial`, `_LoginPartial`) explicitly using `<partial>` or `Html.PartialAsync`. Layouts participate in the view-start pipeline and section capture; partials inherit the parent's `ViewData`/`ViewBag` by default unless given an explicit model. I think of layouts as templates that pull in the view, and partials as components the view pushes into its own markup.

---

## Q7. How do nested layouts work?

**Concepts**
- Inside-out layout chaining
- Intermediate layout's `Layout` property pointing to parent
- Each level requiring its own `@RenderBody()` call
- Section forwarding across intermediate levels
- Missing section forwarder as a common silent bug

**Answer**

Nested layouts chain by setting `Layout = "_ParentLayout"` inside an intermediate layout file. The rendering order is inside-out: the innermost view fills the intermediate layout's `@RenderBody()`, the rendered intermediate layout then fills the outer layout's `@RenderBody()`, so the outer layout ultimately holds the complete composed page. A common real-world pattern is `_AdminLayout.cshtml` setting `Layout = "_Layout"` so the admin section gets a sidebar around its pages while still sharing the root site chrome. The part most developers miss is section forwarding — sections defined in the page view do not automatically bubble through the chain. Every intermediate layout must explicitly forward them with `@section Scripts { @RenderSection("Scripts", required: false) }`, otherwise scripts declared in a page never reach the root layout's script slot and are silently discarded.

---

## Q8. How are `@section` definitions passed from a view to a layout?

**Concepts**
- Section capture during view execution
- Name-based matching between `@section` and `@RenderSection`
- Multi-level forwarding requirement in nested layouts
- Section content excluded from body stream
- Section executing in the defining view's scope

**Answer**

When a view defines `@section Scripts { ... }`, Razor captures that block during view execution and stores it indexed by name. The active layout then renders the captured content when it calls `@RenderSection("Scripts")` — the match is purely by name, so the string in `@section` must match the argument to `@RenderSection` exactly. Since sections execute in the context of the defining view, they can reference the view's `@model` and local variables. Content outside any section goes into the body stream; section content is held separately, so the layout controls where each type of content appears on the page. In a nested layout chain, each intermediate layout must explicitly forward sections to the parent, because capture and render happen per layout level rather than globally through the entire chain.

---

## Q9. Can a view define the same section name twice? What happens?

**Concepts**
- Single section name per view file rule
- Compile-time duplicate section error
- Merging script blocks into one section
- Partial invocation as an alternative for multiple inclusion points

**Answer**

No — Razor permits each section name only once per view file, and defining `@section Scripts` twice in the same view is a compile-time error, not a runtime merge. This is intentional: sections are named slots with a clear one-to-one relationship between definition and consumption, unlike HTML tags which browsers merge silently. The fix is to combine both script references into a single `@section Scripts { ... }` block, or to extract shared scripts into a partial and invoke that partial inside one section. The restriction applies per view file rather than per layout chain, so intermediate layouts may each define their own forwarding section — what is forbidden is two `@section Scripts` declarations inside the same `.cshtml` file.

---

## Q10. What is the difference between `Html.PartialAsync` and the `<partial>` tag helper?

**Concepts**
- `<partial>` tag helper as the recommended approach in new code
- `Html.PartialAsync` returning `IHtmlContent` requiring `@await`
- Both rendering asynchronously without a controller
- Dynamic partial name selection favoring `Html.PartialAsync`

**Answer**

Both render a partial view asynchronously, but `<partial name="_MyPartial" model="Model.Items" />` is the cleaner, preferred syntax for ASP.NET Core because it reads naturally in HTML markup and integrates with IntelliSense without needing `@await` inline. `Html.PartialAsync` returns `IHtmlContent` and requires `@await` — it remains the better choice when the partial name is computed at runtime in C# code within the view, since the tag helper expects a literal or simple expression for the name attribute. Functionally both approaches are equivalent: neither runs a controller action, both accept an optional model and `ViewDataDictionary`, and both use the same view location discovery rules. In new code I default to `<partial>` and reach for `PartialAsync` only when I need to programmatically build the name string.

---

## Q11. How does Razor locate partial views?

**Concepts**
- Controller-specific folder searched first
- `Views/Shared/` as the fallback location
- First-match resolution order
- Explicit path bypassing ambiguous discovery
- Calling view's context driving the resolution

**Answer**

Razor searches a defined set of locations relative to the calling view's context, not the layout's location. For a partial named `_ProductTile` called from `HomeController`, the engine checks `Views/Home/_ProductTile.cshtml` first, then `Views/Shared/_ProductTile.cshtml`, and the first match wins. Using explicit paths like `~/Views/Shared/_ProductTile.cshtml` skips discovery entirely and guarantees the correct file regardless of ambiguity. The part that surprises developers is that the calling view's controller context drives resolution, so a partial rendered from a layout is resolved against the current page's controller, not the layout's folder. A partial in `Views/Shared/` with the same name as one under an Area can cause unexpected binding — use distinct names or fully qualified paths when names might collide.

---

## Q12. How does partial view resolution differ in Areas?

**Concepts**
- Area-expanded search path prepended to the resolution order
- `Areas/{AreaName}/Views/{Controller}/` and `Areas/{AreaName}/Views/Shared/` added first
- Application `Views/Shared/` remaining in the fallback chain
- Root Shared potentially shadowing Area-local partials on name collision
- Explicit Area path for guaranteed resolution

**Answer**

Area views extend the standard resolution order by prepending area-specific paths: for a partial called from `Areas/Billing/Views/Invoices/`, the engine searches `Areas/Billing/Views/Invoices/`, then `Areas/Billing/Views/Shared/`, and finally application `Views/Shared/`. The critical point is that Area isolation is not absolute — root `Views/Shared/` remains in the chain and can shadow an Area-specific partial when both have the same name, which causes the wrong version to render silently. The reliable fix is to use an explicit path like `~/Areas/Billing/Views/Shared/_LineItems.cshtml` so resolution does not depend on folder ordering. Area layouts follow the same expanded path, which is why Area `_ViewStart` must set an explicit layout path rather than relying on a short name to pick up the Area-local version.

---

## Q13. What is the difference between `@RenderSection` and `@await Html.PartialAsync`?

**Concepts**
- `@RenderSection` rendering content defined by the child view
- `Html.PartialAsync` rendering a standalone Razor file
- Top-down layout contract vs horizontal fragment composition
- Section optional/required flag vs partial always rendering when called
- Explicit model parameter for partials vs inherited scope for sections

**Answer**

The fundamental difference is direction: `@RenderSection` is a top-down contract where the child view declares content and the layout consumes it, while `Html.PartialAsync` is a horizontal composition where any view or layout pulls in a standalone reusable file. Sections are declared in the page view and consumed by the layout; the layout controls placement and enforcement via the `required` flag. Partials are independent `.cshtml` files invoked explicitly and always render when called, though they throw `InvalidOperationException` if not found. Partials accept an explicit model parameter and can be strongly typed, while sections execute in the defining view's scope and share its model. I use sections for page-specific scripts and styles destined for layout slots, and partials for shared UI fragments like `_ValidationSummary` or product tiles that appear across many pages.

---

## Q14. Why pass a strongly typed model to a partial instead of `ViewBag`?

**Concepts**
- `@model` declaration providing compile-time property checking
- `ViewBag` as an untyped dynamic dictionary
- Explicit data contract for partial dependencies
- Refactoring safety with strong types
- Test isolation with plain POCO construction

**Answer**

Passing a strongly typed model — `model="Model.Lines"` with the partial declaring `@model IEnumerable<LineItemViewModel>` — documents exactly what data the partial needs and makes the contract visible at compile time. Any property access on `Model.Items` catches typos at build time rather than throwing at runtime. `ViewBag` is dynamic and invisible to refactoring tools: renaming a property or key anywhere in the codebase will not update the string used in `ViewBag.Orders`, so breakage only appears when the page renders. Strong typing also makes the partial independently testable — a unit test can construct the model directly and verify rendering logic without needing to populate a `ViewData` dictionary. I reserve `ViewBag` for rare layout-level notifications like flash message strings, not for structured data flowing into partials.

---

## Q15. How does a child view override the layout assigned in `_ViewStart`?

**Concepts**
- Per-view `Layout` property taking precedence over `_ViewStart`
- `Layout = null` for standalone pages without any layout
- `_ViewStart` still running but its assignment being replaced
- Fully qualified path for explicit alternate layout

**Answer**

A view overrides `_ViewStart` by setting the `Layout` property in a `@{ }` block at the top of the `.cshtml` file — for example `@{ Layout = "_PrintLayout"; }` or `@{ Layout = null; }`. The view-level assignment replaces whatever value `_ViewStart` set because `_ViewStart` still executes first, but the view's own block runs afterward and wins. Setting `Layout = null` renders the view as a standalone HTML page with no wrapper, which is the right choice for login, print, or embedded-widget pages. When the alternate layout lives in a non-default location, a fully qualified path like `~/Views/Shared/_AdminLayout.cshtml` avoids ambiguous resolution. Controllers can also select a layout via response headers or action-specific view calls, but I treat the view's own `@{ Layout = ... }` as the cleanest approach since it keeps the layout decision visible at the top of the file.

---

## Q16. What happens if a required section is not defined in a view?

**Concepts**
- `InvalidOperationException` thrown at render time
- Error identifying missing section name and layout file
- Empty section block satisfying the required contract
- `IsSectionDefined` as an alternative to required enforcement

**Answer**

When the layout calls `@RenderSection("Scripts", required: true)` and the rendering view omits `@section Scripts`, Razor throws `InvalidOperationException` at render time, producing a 500 error for that page. The error message identifies the missing section name and the layout file so the cause is findable, but it only surfaces when that specific view is rendered — not at build time and not when other views are requested. The fix is to add an empty `@section Scripts { }` to the view if the layout contract is correct, or to change the layout's `required: true` to `required: false` if the section is genuinely optional for some pages. A better approach is to use `IsSectionDefined("Scripts")` in the layout and inject a default script block when the section is absent, so pages that do not declare the section still get the shared baseline scripts.

---

## Q17. What is the `Shared` folder under `Views` used for?

**Concepts**
- Cross-controller shared views and layouts
- Fallback location in Razor's view resolution order
- Editor and display templates under `Shared/EditorTemplates/`
- `_ViewImports.cshtml` and `_ViewStart.cshtml` placement
- Area-specific `Shared` folder for area-scoped reuse

**Answer**

`Views/Shared/` is the fallback search location Razor checks when a view or partial is not found in the controller-specific folder, making it the home for everything that needs to be accessible across controllers. Layout files (`_Layout.cshtml`), shared partials (`_ValidationScriptsPartial.cshtml`, `_LoginPartial.cshtml`), and error pages all live here so they are discoverable regardless of which controller triggers them. Editor templates and display templates for `Html.EditorFor`/`DisplayFor` also reside under `Shared/EditorTemplates/` and `Shared/DisplayTemplates/` respectively. For Area views, `Areas/{AreaName}/Views/Shared/` provides area-scoped reuse so area-specific layouts and partials do not pollute the root Shared folder or create name collisions with application-wide files.

---

## Q18. How does `_ViewStart` layout resolution work in Areas?

**Concepts**
- `Areas/{AreaName}/Views/_ViewStart.cshtml` running first for area views
- Fallback to root `Views/_ViewStart.cshtml` when area file is absent
- Area-expanded layout resolution including `Areas/{AreaName}/Views/Shared/`
- Fully qualified layout path for guaranteed area-local selection
- Application-root-relative `~/` syntax for asset paths in area layouts

**Answer**

Area views run the closest `_ViewStart.cshtml` first — `Areas/{AreaName}/Views/_ViewStart.cshtml` — and fall back to the root `Views/_ViewStart.cshtml` if the area file does not exist. When the area `_ViewStart` sets `Layout = "_Layout"`, the name is resolved through area-expanded paths: the engine checks `Areas/Admin/Views/Shared/_Layout.cshtml` before root `Views/Shared/_Layout.cshtml`, so the area-local version should win if it exists. The problem is that using a short name like `"_Layout"` depends on that resolution order being correct, which is fragile — a naming collision or missing area file causes the root site layout to load silently. I always set the area `_ViewStart` with a fully qualified path, `Layout = "~/Areas/Admin/Views/Shared/_Layout.cshtml"`, to make the intent explicit. Asset paths in area layouts should also use `~/` application-root syntax rather than relative paths like `../css/admin.css`, since relative paths break the moment a different layout is selected.

---

## Gotchas — Layouts, Sections & Partial Views (Interview Traps)

---

#### Gotcha 1. `@RenderSection` required vs optional — missing section throws exception

**Concepts**
- `@RenderSection("Scripts")` — `required: true` by default; throws when section absent
- `@RenderSection("Scripts", required: false)` — silently skips when the view has no section
- Section defined in view but not rendered in layout — content silently discarded
- Runtime exception — `InvalidOperationException: Section 'Scripts' is not defined`

**Answer**

`@RenderSection("Scripts")` with no second argument defaults to `required: true`. Any view that does not define `@section Scripts { }` throws `InvalidOperationException: Section 'Scripts' is not defined` at runtime. The safe default for optional layout extension points is `@RenderSection("Scripts", required: false)`, which renders the section if present and silently skips it if absent. The inverse problem — defining `@section Sidebar { }` in a view whose layout never calls `@RenderSection("Sidebar")` — silently discards the section content with no error. Both directions cause lost content or unexpected exceptions; auditing layout/section contracts with integration tests that assert rendered HTML is the only reliable catch.

---

#### Gotcha 2. `@section` defined in a nested layout not propagating to the root layout

**Concepts**
- Nested layouts — child layout renders `@RenderBody()` from its parent layout
- Section scope — sections defined in a child layout view are not automatically visible to a parent layout
- Section propagation — each intermediate layout must re-define and re-render sections upward
- Silent content loss — script tags from child views never appear in the parent `<head>`

**Answer**

When `_AdminLayout.cshtml` uses `_Layout.cshtml` as its own layout, sections defined with `@section Scripts {}` in a view that uses `_AdminLayout` are rendered by `_AdminLayout`'s `@RenderSection("Scripts")`, not by `_Layout`. If `_Layout` also has its own `@RenderSection("Scripts", required: false)` for the page-level `<script>` block, it never receives the child view's section content because sections do not bubble up through nested layouts automatically. To propagate a section through multiple layout levels, each intermediate layout must re-define and re-render it: `@section Scripts { @RenderSection("Scripts", required: false) }`. Forgetting this produces pages where scripts from child views never appear in the root layout's script block.

---

#### Gotcha 3. `@Html.Partial` (synchronous) blocking thread pool threads under load

**Concepts**
- `@Html.Partial` — synchronous rendering, blocks the thread pool thread
- `@await Html.PartialAsync` — async, releases thread while partial renders
- `<partial name="_X" model="m" />` tag helper — always async, idiomatic ASP.NET Core
- Thread pool starvation — many simultaneous sync partial renders reduce concurrency

**Answer**

`@Html.Partial("_Widget", model)` is synchronous and blocks the thread pool thread for the entire rendering duration. Under concurrent load with many views each rendering multiple partials, threads pile up waiting for CPU or I/O, causing latency spikes and reduced throughput. `@await Html.PartialAsync("_Widget", model)` and the `<partial name="_Widget" model="model" />` tag helper are async and release the thread while rendering. The `@await` is required — `@Html.PartialAsync` without `await` discards the `Task` silently and produces empty output. Prefer the `<partial>` tag helper for new code as it is idiomatic, always async, and reads more like HTML than a Razor method call.

---

#### Gotcha 4. `@section` defined inside a partial view — silently ignored

**Concepts**
- `@section` in a partial — compiled without error but ignored at runtime
- Sections only honored in views rendered directly as action results
- Script injection from partials — must be moved to the calling view's `@section Scripts`
- View Component — correct abstraction when a widget needs its own scripts and data

**Answer**

`@section Scripts { <script src="widget.js"></script> }` inside `_Widget.cshtml` compiles without error but is silently ignored — sections are only honored in views rendered directly as action results, not in partials called via `@await Html.PartialAsync` or `<partial>`. Any scripts defined inside a partial's section block never appear in the final HTML. The fix is to move the `<script>` tag to the `@section Scripts` block of the calling view, accepting the coupling between the partial and its host. For reusable, self-contained widgets that legitimately need to inject scripts and styles, a View Component is the correct abstraction — its view is rendered directly as an action result and can define sections.

---

#### Gotcha 5. Nested layouts producing duplicate `<html>`, `<head>`, or `<body>` tags

**Concepts**
- Parent layout — declares full `<html>`, `<head>`, `<body>` skeleton
- Child layout — should contain only structural fragment and `@RenderBody()`
- Duplicate structural tags — malformed HTML breaks browser parsing and CSS cascade
- `@{ Layout = "_ParentLayout"; }` in child layout — correct composition pattern

**Answer**

When `_AdminLayout.cshtml` sets `@{ Layout = "_Layout"; }`, it should contain only the admin-specific structural fragment (sidebar, breadcrumbs, secondary nav) and a `@RenderBody()` call — not its own `<html>`, `<head>`, or `<body>` tags. A child layout that includes a full HTML skeleton produces duplicate structural elements in the rendered output, breaking browser parsing and causing stylesheet and script loading failures. The parent layout provides the skeleton; child layouts add structure within `@RenderBody()`. Validate the nested layout hierarchy by inspecting raw HTML with browser developer tools — duplicate structural tags are invisible in the visual render but cause layout and script failures.

---

#### Gotcha 6. `_ViewStart.cshtml` applying an unwanted layout to API-style partial views

**Concepts**
- `_ViewStart.cshtml` — runs before every view in its folder and all subfolders
- `Layout = "_Layout"` inheritance — applies to all views including partial-only routes
- Partial view returning full HTML page — unwanted layout wrapper added by `_ViewStart`
- `@{ Layout = null; }` in a view — explicitly disables the inherited layout

**Answer**

`_ViewStart.cshtml` in the `Views/` root sets `Layout = "_Layout"` for every view in the tree. A view intended to return only an HTML partial or a fragment for an AJAX update inherits this layout and renders an unexpected full HTML document around the partial content. The fix is to override in the specific view: `@{ Layout = null; }` as the first line disables the inherited layout for that view. For a cluster of views in a subfolder that all need a different layout or no layout, placing a `_ViewStart.cshtml` in that subfolder with its own `Layout` assignment overrides the root `_ViewStart` for the entire subfolder.

---

#### Gotcha 7. Calling `@RenderBody()` more than once in a layout

**Concepts**
- `@RenderBody()` — renders view content exactly once; single-use slot
- Second `@RenderBody()` call — throws `InvalidOperationException` at runtime
- Two-column layout — use `@RenderBody()` for main content and `@RenderSection("Sidebar")` for the second column
- `RenderBody has already been called` — only manifests at runtime, not at compile time

**Answer**

`@RenderBody()` can only be called once per layout. Calling it a second time throws `InvalidOperationException: RenderBody has already been called` — this is a runtime-only error, not caught at compile time. Developers sometimes add a second `@RenderBody()` to populate a second content area, expecting the same view content in both columns, but the framework enforces single-use. For two-column layouts, place `@RenderBody()` in the main content column and use `@RenderSection("Sidebar", required: false)` in the second column — views can then optionally inject sidebar content while the main body renders in its designated slot.

---

#### Gotcha 8. Partial view model type mismatch causing runtime cast exception

**Concepts**
- `<partial model="Model.Order">` — passes the actual runtime type to the partial
- Partial's `@model OrderDetailViewModel` — expects a specific type
- `InvalidCastException` at render time — not caught at compile time in development
- Precompilation — Razor SDK validates model types at `dotnet publish -c Release`

**Answer**

`<partial name="_OrderDetail" model="Model.Cart" />` passes a `Cart` object to a partial whose `@model OrderDetailViewModel` expects a different type. At runtime the view engine attempts to assign `Cart` to `OrderDetailViewModel` and throws `InvalidCastException`. The error only surfaces when the specific parent view renders — a missing integration test lets it ship to Production. Precompilation in a Release publish catches this at build time since the Razor SDK generates strongly typed view classes and detects the type mismatch. In development, running `dotnet publish -c Release` locally or adding an integration test that hits the parent view route surfaces the mismatch before deploy.

---

#### Gotcha 9. Layout data loaded with `@inject` service calls adding hidden queries per page render

**Concepts**
- `@inject IMenuService menus` in `_Layout.cshtml` — runs on every page render across the app
- Uncached service call per request — hidden query outside controller profiling span
- Partial rendered in loop with injected service — NÃ—M queries per page
- `IMemoryCache` inside the service, or View Component with caching — correct patterns

**Answer**

`@inject IMenuService MenuService` in `_Layout.cshtml` followed by `@await MenuService.GetMenuAsync()` runs that query on every single page render across the entire application. If the menu query takes 20ms uncached, every route adds 20ms from the layout alone, and the query appears under view rendering in profiling traces rather than under the controller action, making it hard to attribute. The correct patterns are: cache the result in `IMemoryCache` with a reasonable TTL inside the service; use a View Component for the menu widget which has its own `InvokeAsync` and can short-circuit on cache hit; or populate the data in an action filter that sets it on `ViewData` so the layout reads from a pre-populated dictionary.

---

#### Gotcha 10. Missing `@addTagHelper` directive causing `<partial>` to render as literal HTML

**Concepts**
- `<partial name="_Foo" />` — Tag Helper that requires `@addTagHelper` registration
- Missing `_ViewImports.cshtml` directive — tag helper not registered, element renders as unknown HTML
- `@Html.Partial("_Foo")` — HTML helper method, works without tag helper registration
- `@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers` — required in `_ViewImports.cshtml`

**Answer**

`<partial name="_Foo" />` is a Tag Helper that requires `@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers` in `_ViewImports.cshtml` at the appropriate folder level. If that directive is missing, the tag helper is not registered and the `<partial>` element renders as a literal unknown HTML tag with no partial view content — producing empty output silently. The synchronous `@Html.Partial("_Foo")` is an HTML helper method call via Razor and works without tag helper registration. When migrating from HTML helpers to tag helpers or when `<partial>` produces no visible output, the first check is that `_ViewImports.cshtml` at the correct folder level includes the tag helper registration directive.

---
## Scenario-Based Questions (Karat Format)

---

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

**Concepts**
- Closest-folder `_ViewStart` winning over root
- `Layout = null` stripping all layout chrome from every sibling view
- Experimental `_ViewStart` files left in source control
- Folder-scoped cascade affecting all controllers under the path

**Answer**

The bug is the `_ViewStart.cshtml` cascade: the engine runs the closest folder's `_ViewStart` first, so `Views/Home/_ViewStart.cshtml` sets `Layout = null` before the root file ever executes. Every view under `Views/Home/` — About, Index, and Contact — inherits that null layout, which is why About renders as bare HTML with no navigation or CSS, even though the root `_ViewStart` is correctly configured. Other controllers are unaffected because their views fall back to the root file directly.

The fix is to delete `Views/Home/_ViewStart.cshtml` entirely if it was left over from an experiment, since the root `Layout = "_Layout"` is correct for all Home views. If the Home section genuinely needs a different layout, set `Layout = "_HomeLayout"` explicitly with the correct layout name rather than `null`, and ensure that file exists in `Views/Shared/`. The team should also document that subfolder `_ViewStart` files are intentional overrides — an undiscovered `Layout = null` sitting in a subfolder is a silent chrome-stripping bug that only surfaces when that specific controller's views are tested.

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

**Concepts**
- `required: true` throwing `InvalidOperationException` for missing section
- `required: false` silently omitting analytics with no signal
- Inconsistent required flags producing mixed failure modes
- `IsSectionDefined` with default fallback as safer optional pattern

**Answer**

There are two independent issues here, both caused by the `required` flag. The `Scripts` section is `required: true`, so `Confirm.cshtml`, which omits `@section Scripts`, throws `InvalidOperationException` at render time and returns a 500. This is a hard crash that becomes visible immediately once the new checkout step is added. The `HeadScripts` section is `required: false`, so pages that omit it silently render without analytics or SEO tags — no exception, no warning, content simply disappears, which is a far more dangerous failure mode in production because it never triggers an alert.

The right fix depends on the intended contract. If every checkout page must inject footer scripts, keep `required: true` and add `@section Scripts { }` (even empty) to every view — or better, move shared scripts directly into the layout and use `required: false` with `IsSectionDefined("Scripts")` to append page-specific additions. For `HeadScripts`, use `required: false` but make the analytics content a first-class partial inside the layout rather than relying on a section contract, so it is always present unless explicitly suppressed. The broader lesson is that `required: false` is not "optional convenience" in production — it means content vanishes with zero signal, while `required: true` means a single forgotten section crashes a page.

---

#### Q3. (D) A product dashboard needs a reusable "Recent Orders" panel on three pages. It runs a scoped repository query, shows a loading skeleton, and must be unit-testable without spinning up the full layout pipeline. The team proposes `@await Html.PartialAsync("_RecentOrders")` with data stuffed into `ViewBag.Orders`. What would you choose instead, and why?

**Concepts**
- View Component with async `InvokeAsync` and DI support
- Partial view as a static rendering fragment with no invocation lifecycle
- `ViewBag` as untyped, non-refactor-safe data channel
- View Component as independently testable class
- `<vc:recent-orders>` tag syntax vs `Component.InvokeAsync`

**Answer**

I would use a View Component — `RecentOrdersViewComponent` with an async `InvokeAsync(int count)` method — because partials are purely rendering fragments with no invocation lifecycle, no constructor injection, and no way to run their own async queries. A partial with `ViewBag.Orders` means every calling controller must remember to populate that key before rendering, the data contract is invisible to refactoring tools, and the logic cannot be tested without exercising the full controller and layout pipeline.

The View Component injects `IOrderRepository` through its constructor, fetches recent orders in `InvokeAsync`, and returns `View(results)` pointing at `~/Views/Shared/Components/RecentOrders/Default.cshtml`. The unit test constructs the component directly, passes a mock repository, calls `InvokeAsync`, and asserts on the returned `ViewViewComponentResult` without any layout or HTTP context. The three dashboard pages invoke it with `<vc:recent-orders count="5" />` or `@await Component.InvokeAsync("RecentOrders", new { count = 5 })` — no `ViewBag` plumbing required. I reserve partials for small, parent-supplied rendering fragments where the data is already available; I use View Components when the widget owns its own data loading or has meaningful state.

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

**Concepts**
- Inside-out rendering order: page fills inner layout, inner fills outer
- Explicit section forwarding at every intermediate layout level
- Sections not auto-bubbling through the layout chain
- `@section Scripts { @RenderSection("Scripts", required: false) }` as the forwarder pattern

**Answer**

Razor builds the page inside-out. First, `Index.cshtml` executes: its body markup (`<h1>Reports</h1>`) is captured as the body stream and its `@section Scripts` block (the reports.js script tag) is captured by name. Second, `_AdminLayout.cshtml` executes: its `@RenderBody()` is replaced by the body stream from Index, producing the sidebar-plus-main structure. The `@section Scripts { @RenderSection("Scripts", required: false) }` block in `_AdminLayout` is the critical forwarder — it captures the scripts section from `_AdminLayout`'s own context (which contains the forwarded Index scripts) and re-registers it as a section for the outer layout. Third, `_Layout.cshtml` executes: its `@RenderBody()` receives the fully rendered admin shell (sidebar + main content), and its `@RenderSection("Scripts", required: false)` at the bottom outputs the forwarded scripts once.

If `_AdminLayout` omitted the `@section Scripts { @RenderSection(...) }` forwarder, the scripts defined in Index would be consumed at the `_AdminLayout` level and never reach `_Layout`. That is the most common nested layout bug: scripts or styles declared in a deep page simply vanish because an intermediate layout absorbed them without forwarding. Sections do not automatically propagate through the chain — every intermediate layout must explicitly forward each named section it does not consume itself.

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

**Concepts**
- Single section name per view compile-time rule
- No merge semantics for duplicate sections
- Merging into one block as the required fix
- Partial extraction for reusable script groups

**Answer**

Razor allows each section name exactly once per view file — two `@section Scripts` blocks do not merge, stack, or concatenate. The compiler reports a duplicate section definition error at build time, which is intentional: sections have a strict one-definition-per-view contract so the layout always knows exactly what content it will render. The developer's assumption that sections accumulate like HTML script tags is incorrect.

The fix is to merge both script references into a single `@section Scripts` block:

```cshtml
@section Scripts {
    <script src="~/lib/jquery-validation/dist/jquery.validate.min.js"></script>
    <script src="~/js/product-editor.js"></script>
}
```

If the validation scripts are shared across many edit views, extracting them into `_ValidationScriptsPartial.cshtml` and invoking `<partial name="_ValidationScriptsPartial" />` inside one section keeps the edit-specific script separate from the reusable validation setup without needing two section declarations.

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

**Concepts**
- Area view location expander extending but not replacing the search order
- Root `Views/Shared/` shadowing Area-local partial on name collision
- Explicit path as the reliable Area isolation technique
- Partial name collision between Area and root Shared

**Answer**

The resolution order for a partial named `_LineItems` called from `Areas/Billing/Views/Invoices/Details.cshtml` includes `Areas/Billing/Views/Shared/` before `Views/Shared/`, so the Area version should win in principle. The problem is that partial resolution is sensitive to the exact configuration of the view location expander, and when both a generic stub in root `Views/Shared/` and a billing-specific version in `Areas/Billing/Views/Shared/` exist with the same name, the actual resolution winner can depend on registration order or whether the expander is correctly set up. The generic stub has `@model IEnumerable<LineItem>` while the Area version expects `IEnumerable<OrderLine>` or a similar billing type — if the wrong file wins, the model type mismatch renders nothing or produces a runtime error.

The reliable fix is to use an explicit path: `<partial name="~/Areas/Billing/Views/Shared/_LineItems.cshtml" model="Model.Lines" />`. This bypasses discovery entirely and guarantees the billing-specific markup renders. Alternatively, rename the Area partial to `_BillingLineItems.cshtml` to eliminate the name collision and make the intent obvious. For Area-specific widgets that load their own data, a View Component with `[AreaViewLocationFormats]` provides stronger isolation than partial name-based discovery.

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

**Concepts**
- Short layout name resolved through area-expanded paths
- `Areas/Admin/Views/Shared/_Layout.cshtml` searched before root
- Fully qualified path as the reliable fix for Area layouts
- Application-root `~/` syntax for asset paths in layouts
- Unused area layout file until `_ViewStart` points to it

**Answer**

The resolution path for `Areas/Admin/Views/Dashboard/Index.cshtml` works like this: `Areas/Admin/Views/_ViewStart.cshtml` sets `Layout = "_Layout"`, and the view location expander searches for that name first in `Areas/Admin/Views/Shared/` and then in `Views/Shared/`. If `Areas/Admin/Views/Shared/_Layout.cshtml` exists and is found first, it should win — but in practice, if the root layout loads instead, it is likely a configuration issue with the expander or the area layout file having a different name than expected.

The most common cause is that the area layout exists but `_ViewStart` uses the short name `"_Layout"` while the root layout has the same short name and wins in some configurations. The fix is to set the area `_ViewStart` to an explicit path: `Layout = "~/Areas/Admin/Views/Shared/_Layout.cshtml"`. This removes all ambiguity and guarantees the admin-specific layout is selected. The `../css/admin.css` relative path in the layout also needs to change to `~/css/admin.css` — relative paths break the moment a different layout is used or the request URL depth changes, while `~/` is always application-root relative regardless of route structure.

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

**Concepts**
- N+1 query pattern at the view layer
- `@inject` in partials executing per invocation inside a loop
- Batch data loading upstream in the controller
- Partial as a dumb rendering fragment after upstream enrichment
- Connection pool pressure and latency under concurrent load

**Answer**

Forty async partials each making an independent `GetRatingAsync` database call is a classic N+1 at the view layer. Under load this means every page request fires 40 queries, so 100 concurrent users generate 4,000 concurrent queries — well beyond typical connection pool limits, producing pool exhaustion, queuing delays, and p95 latency that scales linearly with tile count. The `@inject` + `GetRatingAsync` pattern makes this easy to miss in development where the catalog has 5 products and queries hit local SQL.

The fix is to move all data loading to the controller. The catalog action should call `GetRatingsAsync(productIds)` once, receiving all ratings in a single query, then populate `ProductSummary.Rating` before the view runs. The `_ProductTile.cshtml` partial then simply renders `@Model.Rating.Stars` with no service injection and no async operations. The partial stays reusable — it still renders a tile from a `ProductSummary` — but it becomes a dumb rendering fragment rather than a data-loading component. If ratings are hot-path data, caching them at the service layer with a short TTL eliminates the per-request database cost entirely.

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

**Concepts**
- `@RenderBody()` absence silently discarding primary view content
- View markup outside sections routed to body stream only
- Layout compiling and serving without error despite missing `@RenderBody()`
- Smoke-testing new layouts with non-section body content

**Answer**

The layout is missing `@RenderBody()`, which means the primary content stream — everything the view writes outside of named sections — is silently discarded. The `<p>Sign up today.</p>` text in `Index.cshtml` is captured as the body stream during view execution, but the layout has nowhere to render it, so it disappears. The `@section Hero` block does render because it is consumed by `@RenderSection("Hero", required: false)`, but anything outside sections is lost. No exception is thrown because the layout is syntactically valid Razor — `@RenderBody()` is only required if you want body content to appear.

The fix is to add `@RenderBody()` between the header partial and the footer partial:

```cshtml
@await Html.PartialAsync("_MarketingHeader")
@RenderSection("Hero", required: false)
@RenderBody()
@await Html.PartialAsync("_MarketingFooter")
```

Any new layout should be smoke-tested immediately with a minimal view that contains non-section body text so the blank-page symptom surfaces before other views are built on top of it.
