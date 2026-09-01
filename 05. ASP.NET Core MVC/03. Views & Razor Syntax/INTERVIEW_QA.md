# Views & Razor Syntax — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is a Razor view?](#q1-what-is-a-razor-view)
2. [Q2. What is the `@model` directive?](#q2-what-is-the-model-directive)
3. [Q3. What is the difference between `@` and `@@` in Razor?](#q3-what-is-the-difference-between-and-in-razor)
4. [Q4. How does Razor automatically encode output and why does it matter?](#q4-how-does-razor-automatically-encode-output-and-why-does-it-matter)
5. [Q5. What is the difference between `@Html.Raw` and default Razor output?](#q5-what-is-the-difference-between-htmlraw-and-default-razor-output)
6. [Q6. What is a code block (`@{ }`) in Razor?](#q6-what-is-a-code-block-in-razor)
7. [Q7. What is the difference between a strongly typed view and a dynamic view?](#q7-what-is-the-difference-between-a-strongly-typed-view-and-a-dynamic-view)
8. [Q8. What logic should not belong in a Razor view?](#q8-what-logic-should-not-belong-in-a-razor-view)
9. [Q9. What is `@inject` used for in Razor views?](#q9-what-is-inject-used-for-in-razor-views)
10. [Q10. What is the `@functions` block in Razor?](#q10-what-is-the-functions-block-in-razor)
11. [Q11. What is the difference between Razor runtime compilation and precompilation?](#q11-what-is-the-difference-between-razor-runtime-compilation-and-precompilation)
12. [Q12. When would you enable `AddRazorRuntimeCompilation`?](#q12-when-would-you-enable-addrazorruntimecompilation)
13. [Q13. What is a partial view and when do you use one?](#q13-what-is-a-partial-view-and-when-do-you-use-one)
14. [Q14. What is the difference between `Html.PartialAsync` and the `<partial>` tag helper?](#q14-what-is-the-difference-between-htmlpartialasync-and-the-partial-tag-helper)
15. [Q15. What is `ViewData` and how is it accessed in Razor?](#q15-what-is-viewdata-and-how-is-it-accessed-in-razor)
16. [Q16. What causes "The view 'X' was not found" errors?](#q16-what-causes-the-view-x-was-not-found-errors)
17. [Q17. What is `_ViewImports.cshtml` used for?](#q17-what-is-_viewimportscshtml-used-for)
18. [Q18. What is `_ViewStart.cshtml` used for?](#q18-what-is-_viewstartcshtml-used-for)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What is a Razor view?

**Concepts**
- Razor view — `.cshtml` file mixing HTML and C# via `@` syntax, compiled to a class
- View discovery — `Views/{Controller}/{Action}.cshtml` and `Views/Shared/` by convention
- `@model` directive — declares strongly typed model for compile-time checking
- HTML encoding — default `@` expression output, XSS protection built in
- Precompilation — views compiled to `*.Views.dll` at publish time in Release

**Answer**

A Razor view is a `.cshtml` file that mixes HTML with C# through the `@` transition character, compiled by the Razor SDK into a class that the view engine invokes to produce HTML output. The view engine locates views by convention under `Views/{ControllerName}/` or `Views/Shared/`, then optionally under area prefixes. The `@model` directive at the top declares the expected model type so the view accesses typed properties as `Model.Property` with compile-time and IntelliSense support. All `@` expressions HTML-encode their output by default, which is the primary defense against XSS in server-rendered pages. In Release builds the SDK precompiles views into a `ProjectName.Views.dll` assembly, so production serves views from DLLs without Roslyn at runtime and view syntax errors appear at build time rather than first request.

---

## Q2. What is the `@model` directive?

**Concepts**
- `@model` — declares the strongly typed model type for the view
- `Model` property — typed accessor for the passed instance
- Single `@model` per view — only one declaration allowed
- `return View(viewModel)` — controller passes the typed instance
- Partial views — also support `@model` for reusable typed fragments

**Answer**

The `@model` directive at the top of a Razor view declares the type of the data the controller passes, making it accessible as the `Model` property with full type information. I use it as `@model ProductEditViewModel` so accessing `@Model.Name` is checked at compile time and IDE tooling shows available members. Without `@model`, the view relies on untyped `ViewData` or `dynamic` where typos fail silently at runtime. Only one `@model` directive is allowed per view, and the controller passes the instance via `return View(viewModel)`. Partial views also declare `@model` so they have typed contracts when reused across multiple parent views.

---

## Q3. What is the difference between `@` and `@@` in Razor?

**Concepts**
- `@` — transitions from HTML to C# expressions, directives, and code blocks
- `@@` — renders a literal `@` character in HTML output
- `@{ }` — multi-statement code block without direct output
- CSS `@media` inside `<style>` — needs `@@media` in inline Razor styles
- Accidental Razor parsing — unescaped `@` in JS strings triggers Razor

**Answer**

A single `@` transitions from HTML to C# — it starts expressions like `@DateTime.Now`, code blocks like `@{ }`, and directives like `@model` and `@using`. `@@` escapes the transition and renders a literal `@` in the HTML output. The escape is needed in email addresses, CSS `@media` rules in inline `<style>` blocks, and CSS class names containing `@` that would otherwise be parsed as Razor transitions. Directives use a single `@` and are processed at compile time while expressions are evaluated at render time. Misusing `@` inside JavaScript string literals can accidentally invoke the Razor parser, producing compilation errors or silently dropping content.

---

## Q4. How does Razor automatically encode output and why does it matter?

**Concepts**
- HTML encoding — `<`, `>`, `"`, `&` converted to HTML entities on `@` output
- XSS prevention — encoded content displays as text, never executes as markup
- `@Html.Raw` — explicit opt-out from encoding, for trusted content only
- JavaScript encoding context — HTML encoding is not sufficient inside `<script>` or event attributes
- `IHtmlContent` — bypasses auto-encoding, already-trusted HTML wrapper

**Answer**

Razor HTML-encodes every expression written with `@` before writing to the response, converting characters like `<`, `>`, `"`, and `&` to HTML entities. This means `@Model.UserComment` containing `<script>alert(1)</script>` renders as visible text in the browser rather than executing as script — the primary automatic defense against XSS in server-rendered MVC pages. Encoding is automatic and must be explicitly overridden with `@Html.Raw()` when the content is trusted server-side HTML. The distinction matters because different output contexts require different encoders: HTML encoding protects text and attribute contexts, but content placed inside JavaScript string literals or URL parameters requires JavaScript or URL encoding respectively — `@` does not cover those. `IHtmlContent` implementations like `HtmlString` and `TagBuilder` bypass auto-encoding because they signal already-trusted markup.

---

## Q5. What is the difference between `@Html.Raw` and default Razor output?

**Concepts**
- Default `@` output — HTML-encodes values before writing to response
- `@Html.Raw` — writes string unchanged, bypassing encoding
- Stored XSS — `@Html.Raw` on unsanitized user input from database
- Allowlist HTML sanitizer — required before Raw on user-supplied rich text
- Rich text rendering — sanitize on write or on read, then Raw on sanitized output only

**Answer**

Default `@` output HTML-encodes values so `<script>` appears as literal text in the browser. `@Html.Raw(value)` writes the string unchanged directly into the response, so any HTML or script in the value is interpreted by the browser as markup. Raw should only be used on content that is either generated server-side by trusted code or sanitized through an allowlist HTML sanitizer before storage — never on raw user input from the database. Stored XSS vulnerabilities commonly come from `@Html.Raw(Model.UserContent)` on database fields, since the content passes the developer's "quick check" in local tests but carries injected payloads in production. For rich text scenarios I sanitize with a library like HtmlSanitizer on the write path or before display, then use Raw only on the sanitized output.

---

## Q6. What is a code block (`@{ }`) in Razor?

**Concepts**
- `@{ }` — arbitrary C# statements that do not directly emit output
- Variable declarations — computed once and reused across markup
- Control logic vs business logic — presentation-only `if/else` acceptable
- Excessive code blocks — signal view model or view component refactoring

**Answer**

A Razor code block wraps arbitrary C# statements that do not directly produce output — variable declarations, assignments, loop setup, and method calls. I use `@{ var count = Model.Items.Count; }` to compute a display variable once and reference it later in markup without recomputing inline. Multi-line presentation conditionals like `@if` use the same block syntax with HTML mixed inside the branches. The key discipline is that code blocks should contain presentation logic only — formatting decisions, CSS class selection, display conditions — not database queries or business rules. When `@{}` blocks accumulate business conditionals, that signals the logic belongs in a service with the controller passing a pre-computed view model, or the reusable logic belongs in a view component with its own data loading.

---

## Q7. What is the difference between a strongly typed view and a dynamic view?

**Concepts**
- `@model MyViewModel` — compile-time checked, IntelliSense-enabled
- `ViewBag` / `ViewData` — `dynamic` / `object` dictionary, no compile-time check
- Silent typo failures — `ViewBag.Titel` vs `ViewBag.Title`
- Strongly typed partials — enforce contracts when reused across pages
- Single optional cross-cutting keys — acceptable use for ViewData

**Answer**

A strongly typed view declares `@model MyViewModel` and accesses properties via `Model.PropertyName`, which the compiler checks at build time and IDEs surface with IntelliSense. A dynamic view uses `ViewBag` or `ViewData` dictionary lookups with string keys that only fail at runtime — a typo in `ViewBag.Titel` vs `ViewBag.Title` produces a blank field with no exception, which is exactly the trap in Q4. Strong typing catches property renames at compile time, makes controller-to-view contracts explicit, and enables partial views to declare typed contracts when reused across multiple parent views. Dynamic data is acceptable for isolated cross-cutting concerns like a single page title key or a layout-level flash message with a well-documented convention, but scales poorly once a view depends on more than one or two named pieces. I prefer strongly typed view models for anything complex and reserve `ViewBag` for simple layout metadata.

---

## Q8. What logic should not belong in a Razor view?

**Concepts**
- Business rules — pricing, tax, discount logic not testable in Razor
- Data access via `@inject DbContext` — bypasses service-layer testing
- Authorization checks in views — bypassable by alternate routes or API calls
- Complex `@{}` blocks — belong in application services or view model mapping
- Presentation formatting — acceptable view responsibility

**Answer**

Views should format and render data the controller has already prepared; they must not contain business rules, data access, authorization decisions, or complex calculations. Pricing and tax logic in `.cshtml` bypasses service-layer unit tests, runs invisibly in view render traces, and diverges from API, batch, and email code paths that compute the same values independently. Database queries via `@inject DbContext` or `@inject IRepository` put I/O directly in the rendering pipeline with no caching seam. Authorization checks in Razor are bypassable by alternate routes or direct API calls and belong in policies, filters, or controller logic that runs before the view. Heavy `@{}` blocks with business `if` chains signal that the logic belongs in a service, with the controller passing pre-computed values through a typed view model. Views may format dates, currencies, and CSS classes — they must not decide business outcomes.

---

## Q9. What is `@inject` used for in Razor views?

**Concepts**
- `@inject` — requests a service from DI directly in the view
- Presentation helpers — localization, feature flags, configuration read-only
- N+1 risk — repositories injected in loops cause per-row queries
- View Component — preferred for views needing independent data loading

**Answer**

The `@inject` directive declares a DI-resolved property on the generated view class, letting the view call presentation-oriented services like `IViewLocalizer` for translations or `IOptionsSnapshot<FeatureFlags>` to check feature toggles. Injected services follow DI lifetimes — scoped services align with the request. The important constraint is that `@inject` should target presentation helpers, not repositories or domain services. Injecting `IProductRepository` or `AppDbContext` directly into a view encourages per-row queries inside `@foreach` loops, creating N+1 patterns that are invisible from the controller and extremely hard to batch. When a view genuinely needs its own data-loading logic — a sidebar, a widget, a notification count — the correct pattern is a View Component with its own model and controller-style data loading, which keeps the parent view focused on layout.

---

## Q10. What is the `@functions` block in Razor?

**Concepts**
- `@functions` — methods and properties on the generated view class
- Presentation helpers — CSS class mappers, status label formatters
- Data access in `@functions` — anti-pattern, bypasses service layer
- Cross-view duplication — shared helpers belong in partials or tag helpers
- Compile-time target — generated into the view class at build or runtime

**Answer**

The `@functions` block declares C# methods and properties directly on the generated view class, callable from the markup in the same view. It suits small, pure presentation helpers such as `string StatusClass(string s) => s == "Active" ? "green" : "red";` where inlining the switch inside markup would clutter the HTML. The boundary is that `@functions` must not perform I/O, query databases, or call services — those belong in the controller or a View Component. Async data loading inside `@functions` (awaiting a repository method on each loop iteration) is the classic N+1 pattern seen in Q11. Helpers duplicated across multiple views should move to shared partials, a Tag Helper, or a static helper class rather than being copy-pasted as `@functions` blocks.

---

## Q11. What is the difference between Razor runtime compilation and precompilation?

**Concepts**
- Precompilation — `.cshtml` compiled to `*.Views.dll` at build/publish, no Roslyn at runtime
- Runtime compilation — `AddRazorRuntimeCompilation()`, reads `.cshtml` from disk on demand
- File watcher — runtime compilation recompiles on `.cshtml` change, Development-only
- Build-time errors — precompilation surfaces syntax errors before deploy
- Production default — precompilation in Release, views optional on disk

**Answer**

Precompilation compiles all `.cshtml` files into assemblies at build or publish time so production runs views from DLLs without Roslyn loaded at runtime. View syntax errors appear at build time rather than first request, startup is faster, and `.cshtml` files need not be deployed to the server. Runtime compilation (`AddRazorRuntimeCompilation()` plus the runtime compilation NuGet package) reads `.cshtml` from disk and compiles on demand, with a file watcher that recompiles when files change — the intended workflow for hot reload in Development. The performance trade-off is a CPU spike on cache miss and Roslyn overhead under load. Release publish defaults to precompiled views in `ProjectName.Views.dll`, so I use runtime compilation only in Development, guarded by `builder.Environment.IsDevelopment()`, and rely on redeploy to change views in Production.

---

## Q12. When would you enable `AddRazorRuntimeCompilation`?

**Concepts**
- Development hot reload — edit `.cshtml` without rebuilding the project
- `IsDevelopment()` guard — mandatory to prevent accidental Production use
- Production risk — compile overhead, source files on disk, potential code injection
- `Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation` — required NuGet package
- Staging/Production — precompiled artifacts, redeploy to change views

**Answer**

I enable `AddRazorRuntimeCompilation` only in Development so markup and layout changes take effect immediately during iteration without a full project rebuild — it speeds up UI work on `.cshtml` files. The registration must be conditional: `if (builder.Environment.IsDevelopment()) mvcBuilder.AddRazorRuntimeCompilation();`. Without the guard, Production and Staging also gain the ability to recompile views from disk files, which means editing `.cshtml` on the server applies immediately — a misconfiguration that allows code injection if file permissions are loose. Production should use only the precompiled assemblies from the CI publish artifact, so view changes go through redeploy. The package `Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation` should either not be referenced in Production builds or be guarded so it never activates.

---

## Q13. What is a partial view and when do you use one?

**Concepts**
- Partial view — reusable Razor fragment without layout, rendered inline
- `Views/Shared/` — conventional location, underscore-prefixed names
- `<partial name="_X" model="item" />` — passes strongly typed model
- No `_ViewStart` execution — partials do not wrap in layout
- View Component — preferred when independent data loading is needed

**Answer**

A partial view is a reusable `.cshtml` fragment that renders inline content without a layout, used to avoid copy-pasting identical HTML blocks across multiple views. Partials live in `Views/Shared/` or controller-specific folders and conventionally use underscore-prefixed names like `_ProductCard.cshtml`. I pass a strongly typed model with `<partial name="_ProductCard" model="item" />` so the partial declares its own `@model` and the contract is explicit. Unlike full views, partials do not execute `_ViewStart` so they receive no layout wrapping — they produce only their own HTML, which the parent view embeds inline. Returning `PartialView()` from an action sends an HTML fragment for AJAX replacement. When the reusable component needs its own data-loading logic rather than receiving everything from the parent, a View Component is the appropriate pattern.

---

## Q14. What is the difference between `Html.PartialAsync` and the `<partial>` tag helper?

**Concepts**
- `<partial>` tag helper — declarative, preferred in ASP.NET Core, IntelliSense-friendly
- `Html.PartialAsync` — HTML helper API, returns `Task<IHtmlContent>`, must be awaited
- `Html.Partial` (sync) — avoid, can deadlock; use async variant or tag helper
- View engine resolution — both use the same location expander

**Answer**

Both render a partial view by name, but `<partial name="_LoginPartial" model="Model.User" />` is the recommended tag helper syntax introduced with ASP.NET Core because it integrates with Razor tooling, attribute IntelliSense, and reads more naturally alongside HTML. `Html.PartialAsync` is the older HTML helper API returning `Task<IHtmlContent>` and requires explicit awaiting: `@await Html.PartialAsync("_Name", model)`. I avoid `Html.Partial` (synchronous) because it can deadlock in certain hosting scenarios and is not the preferred pattern in ASP.NET Core. Both resolve partial views by name through the same view engine location expander so discovery behavior is identical. For new code I use the `<partial>` tag helper and migrate `Html.PartialAsync` calls over time.

---

## Q15. What is `ViewData` and how is it accessed in Razor?

**Concepts**
- `ViewData` — `ViewDataDictionary`, request-scoped untyped key-value store
- `ViewBag` — `dynamic` wrapper over the same backing dictionary
- Magic string keys — silently fail on typo without compile-time check
- `TempData` — survives one redirect; `ViewData` does not
- Strongly typed view models — preferred over `ViewData` for complex data

**Answer**

`ViewData` is a `ViewDataDictionary` shared between controller and view for untyped key-value data within a single request. I set `ViewData["Title"] = "About Us"` in the controller and read it with `@ViewData["Title"]` in the view or layout. Values are typed `object` and require explicit casts that can fail silently, which is why magic-string key typos cause blank output without exceptions. `ViewBag` is a `dynamic` wrapper over the same backing dictionary, so `ViewBag.Title` and `ViewData["Title"]` reference the same entry. `ViewData` is useful for small cross-cutting values like page titles and layout metadata, but it does not survive redirects — `TempData` handles cross-request flash data. For anything complex or typed I use a strongly typed view model so renames surface at compile time.

---

## Q16. What causes "The view 'X' was not found" errors?

**Concepts**
- View discovery — `Views/{Controller}/{Action}.cshtml` convention
- Linux case sensitivity — `Index.cshtml` vs `index.cshtml` fails on Linux
- Publish artifact — missing `Views/` folder or `*.Views.dll` in output
- Area views — require `Areas/{Area}/Views/{Controller}/{View}.cshtml`
- Explicit view name typo — `return View("CustomNaem")` not found

**Answer**

The view engine cannot locate a matching `.cshtml` file or precompiled view type for the requested name, which means the conventional path does not match, the publish artifact is incomplete, or the explicit name passed to `View()` has a typo. `return View()` expects `Views/{Controller}/{Action}.cshtml` unless a different name or path is specified, so a controller named `HomeController` with action `About` requires `Views/Home/About.cshtml`. Linux deployments are case-sensitive — `Index.cshtml` and `index.cshtml` are different files, and Windows development masks this mismatch. Publish output lacking `Views/` and `ProjectName.Views.dll` produces this error after deploy despite working locally under `dotnet run`. Area views must be at `Areas/{Area}/Views/{Controller}/{View}.cshtml` and area routing must be registered with `{area:exists}`. I verify the publish artifact locally with `dotnet publish -c Release` and run from the output folder before CI promotes it.

---

## Q17. What is `_ViewImports.cshtml` used for?

**Concepts**
- `_ViewImports.cshtml` — shared directives applied to all views in the folder hierarchy
- `@using` — namespace imports without repeating per view
- `@addTagHelper` — enables tag helpers project-wide
- Nested imports — area-specific `_ViewImports` merges with parent
- No HTML output — compilation context only

**Answer**

`_ViewImports.cshtml` applies shared directives to all views under its folder and subfolders, eliminating repeated declarations across individual views. I place `@using MyApp.ViewModels` here once so every view in the project can reference view model types without a local `@using`. `@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers` enables tag helpers project-wide from one location, and `@inject IViewLocalizer Localizer` in a shared import makes localization available everywhere. Nested `_ViewImports.cshtml` files in area-specific folders merge with the root imports rather than replacing them. The file does not render any HTML — it only sets compilation context for child views. Root `Views/_ViewImports.cshtml` applies to all standard views and area imports apply additionally within their scope.

---

## Q18. What is `_ViewStart.cshtml` used for?

**Concepts**
- `_ViewStart.cshtml` — code that runs before each view renders
- `Layout = "_Layout"` — centralizes layout assignment across all views
- `Layout = null` — individual view opt-out from layout
- Hierarchical execution — root then area-specific in order
- Separate from `_ViewImports` — imports set directives; ViewStart sets runtime properties

**Answer**

`_ViewStart.cshtml` runs before each view renders and is used almost exclusively to set the `Layout` property so individual views do not repeat the layout directive. Placing `Layout = "_Layout";` in root `Views/_ViewStart.cshtml` means every view automatically wraps in the shared layout without any per-view declaration. An individual view can override with `Layout = null;` to opt out entirely or `Layout = "~/Views/Shared/_SpecialLayout.cshtml";` for a different chrome. Multiple `_ViewStart.cshtml` files execute hierarchically from root to area-specific, so areas can assign a different default layout. This is separate from `_ViewImports` — imports handle compilation directives like namespaces and tag helpers while ViewStart handles runtime view configuration. Child views focus on their content section while `_ViewStart` handles layout wrapping transparently.

---

## Gotchas — ASP.NET Core MVC (Interview Traps)

#### Gotcha 1. Business logic in Razor views

**Concepts**
- Pricing and discount calculations in `.cshtml` — no unit test coverage
- Authorization checks in Razor — bypassable by alternate routes

**Answer**

Placing pricing, discount, authorization, or business rules in `.cshtml` files bypasses unit tests, duplicates service-layer logic, and makes behavior hard to change consistently. Views should render data the controller or ViewModel already prepared. Calculations in Razor cannot be tested independently and often diverge from API or batch logic. Razor should be limited to presentation formatting, not business decisions.

---

#### Gotcha 2. EF entities passed directly to views

**Concepts**
- Lazy-loaded navigations — unexpected queries during rendering
- Over-posting — mass assignment via unlocked navigation properties

**Answer**

Binding and displaying EF Core entities exposes navigation properties, causes over-posting on POST, and couples the UI to the database schema. Lazy-loaded navigations can trigger unexpected queries during rendering and mass assignment can update properties the user should not control such as `IsAdmin`. The fix is dedicated ViewModels with only the fields the view needs.

---

#### Gotcha 3. `[FromBody]` on HTML form POST

**Concepts**
- Browser forms — `application/x-www-form-urlencoded`, not JSON
- `[FromBody]` uses JSON input formatter — leaves model empty silently

**Answer**

Standard browser forms send `application/x-www-form-urlencoded`, not JSON. `[FromBody]` uses the JSON input formatter and leaves the model empty while the action runs with default values. I remove `[FromBody]` for conventional form POSTs and use it only when the client sends JSON with the correct Content-Type.

---

#### Gotcha 4. Skipping `ModelState.IsValid` because of client validation

**Concepts**
- Client-side validation — bypassable by direct POST
- Server-side `ModelState.IsValid` — mandatory security gate

**Answer**

Client-side validation is bypassable by direct POST. Server-side validation is mandatory before any persist, redirect, or side effect. I always gate POST actions with `if (!ModelState.IsValid) return View(model);`. Missing server validation is a security defect regardless of client script presence.

---

#### Gotcha 5. `return View()` after successful POST

**Concepts**
- Browser refresh after `return View()` — resubmits POST body
- PRG — `return RedirectToAction` after successful mutation

**Answer**

Returning the same view after a successful POST causes duplicate submission when the user refreshes the page. The fix is Post-Redirect-Get: `return RedirectToAction(nameof(Index))` after successful create or update. Flash success messages go via TempData on the redirect target.

---

#### Gotcha 6. `ModelState` after redirect

**Concepts**
- `ModelState` — request-scoped, does not survive redirect
- On failure — `return View(model)` with errors inline

**Answer**

`ModelState` is request-scoped and does not survive `RedirectToAction`. The correct pattern is to redirect only on success and on validation failure return `View(model)` with errors inline. To survive redirect on failure, serialize errors to TempData or use PRG with a form-specific error cache.

---

#### Gotcha 7. TempData read twice in layout and view

**Concepts**
- TempData consumed on first read by default
- `TempData.Peek` — read without consuming

**Answer**

TempData is consumed on first read by default. If the layout reads a flash message, the view sees nothing unless `Peek()` or `Keep()` is used. I prefer a single consumption point — typically the layout or a dedicated partial, not both. Cookie-based TempData has size limits.

---

#### Gotcha 8. Missing `[Area]` attribute on area controllers

**Concepts**
- `[Area("AreaName")]` — required for area route discovery
- Without attribute — controller treated as root, returns 404

**Answer**

Controllers in `Areas/Admin/Controllers` without `[Area("Admin")]` are not discovered by the areas route and return 404 or match the wrong route. Every area controller must declare `[Area("AreaName")]` matching its folder. Area routing is registered separately with the `{area:exists}` constraint.

---

#### Gotcha 9. Link generation without `asp-area`

**Concepts**
- Tag Helpers — default to current area context
- Cross-area links — require explicit `asp-area` and `asp-controller`

**Answer**

Tag Helpers default to the current area context when generating URLs. Links from a root view to an area controller need explicit `asp-area="Admin"` or they generate URLs without the area segment. Cross-area links require both `asp-area` and `asp-controller`. The same rule applies to `Url.Action` with `new { area = "Admin" }` in route values.

---

#### Gotcha 10. Checkbox `[Required]` on non-nullable `bool`

**Concepts**
- Unchecked checkbox posts nothing — model binding sets `bool` to `false`
- `[Required]` on `bool` never fails — `false` is a valid non-null value

**Answer**

An unchecked checkbox posts nothing and model binding sets a non-nullable `bool` to `false`. `[Required]` never fails because `false` is a valid value. I use `bool?` with `[Required]` to require explicit true selection for consent checkboxes.

---

#### Gotcha 11. Collection binding with gap indices

**Concepts**
- Model binder expects contiguous zero-based indices
- Gap indices — truncation or misalignment after row deletion

**Answer**

Deleting a form row leaving indices such as `Lines[0]` and `Lines[2]` breaks model binder alignment. I reindex client-side after row deletion so indices are contiguous starting at zero, or implement a custom `IModelBinder` that tolerates non-contiguous indices.

---

#### Gotcha 12. `@Html.Raw` with user content

**Concepts**
- Default Razor `@` — HTML-encodes, prevents XSS
- `@Html.Raw` — bypasses encoding, executes injected script

**Answer**

Default Razor encoding prevents XSS. `@Html.Raw(Model.UserComment)` renders attacker-supplied script if the content is not sanitized server-side. I prefer `@Model.UserComment` (auto-encoded) for plain text or sanitize with a trusted HTML sanitizer library before using Raw.

---

#### Gotcha 13. AJAX POST without antiforgery token

**Concepts**
- Form tag helpers — emit antiforgery token automatically
- `fetch` / jQuery AJAX — must send token manually

**Answer**

Form tag helpers emit antiforgery tokens automatically but `fetch` and jQuery AJAX must manually send `RequestVerificationToken` as a header or form field. `[AutoValidateAntiforgeryToken]` on the controller validates all unsafe verb methods. I do not disable antiforgery on MVC cookie-auth endpoints — I add the token instead.

---

#### Gotcha 14. Injecting Hub into MVC controller

**Concepts**
- Hubs not registered in DI for direct injection
- `IHubContext<THub>` — singleton proxy for broadcasting

**Answer**

Hubs are not registered in DI for direct injection. Injecting a concrete `Hub` fails activation or produces an instance without connection context. The correct pattern is `IHubContext<THub>`, which is a singleton proxy registered by `AddSignalR()`.

---

#### Gotcha 15. SignalR scale-out without backplane

**Concepts**
- Sticky sessions — per-client affinity, not cross-instance event routing
- `AddStackExchangeRedis` or `AddAzureSignalR` — multi-instance fan-out

**Answer**

Sticky sessions alone do not fan-out events across server instances. A controller on instance A calling `IHubContext.Clients.User(id).SendAsync` misses users on instance B. Multi-node deployments need `AddStackExchangeRedis` or `AddAzureSignalR`.

---

## Scenario-Based Questions (Karat Format)

#### Q1. (R) A product review page renders user-submitted comments. QA passes with test data; security scan flags stored XSS. Review this Razor fragment — what is wrong and how do you fix it without breaking allowed rich text?

```cshtml
@model ProductReviewViewModel

<h2>@Model.ProductName</h2>

@foreach (var comment in Model.Comments)
{
    <div class="comment">
        @Html.Raw(comment.Body)
    </div>
}
```

*(Controller passes `comment.Body` straight from the database — no server-side sanitization.)*

**Concepts**
- `@Html.Raw` on unsanitized database content — stored XSS
- Default `@comment.Body` — HTML-encodes, prevents script execution
- Allowlist HTML sanitizer — required before Raw for intentional rich text
- Trust boundary — treat all database content as untrusted

**Answer**

`@Html.Raw` bypasses Razor's HTML encoding, so any `<script>` or event-handler markup stored in `comment.Body` executes in the victim's browser — stored XSS that affects every viewer of the page, not just the attacker. QA passes because test data is safe; the encoding is only absent when malicious input is stored.

For plain-text comments the fix is straightforward: replace `@Html.Raw(comment.Body)` with `@comment.Body` and auto-encoding prevents XSS. For intentional rich text — where formatting like bold and links is allowed — I sanitize server-side on write using an allowlist HTML sanitizer (for example HtmlSanitizer / Ganss.XSS) that strips `<script>`, `onerror` event attributes, and `javascript:` URLs, then store the sanitized result. Only the sanitized output is safe to pass to `@Html.Raw`. CSP headers and HttpOnly cookies provide defense in depth but do not replace encoding or sanitization.

---

#### Q2. (R) A teammate "fixes" XSS by switching to `@comment.Body` but adds this analytics hook. Pen testers report script execution from a display name. Diagnose the encoding-context mistake.

```cshtml
@model OrderSummaryViewModel

<button onclick="trackPurchase('@Model.CustomerName', @Model.OrderTotal)">
    Complete order
</button>

<script>
    function trackPurchase(name, total) {
        console.log('Purchase by ' + name + ': ' + total);
    }
</script>
```

**Concepts**
- HTML encoding context — does not protect inside JavaScript string literals
- JavaScript encoding context — JS string literals require JSON/JS encoder
- Inline event-handler interpolation — high-risk injection point
- `data-*` attributes — safer bridge between server data and client script

**Answer**

HTML encoding (`@Model.CustomerName` in body and attribute text context) converts `<` and `>` to entities but does not make values safe inside JavaScript string literals. A customer name like `'); alert(document.cookie);//` breaks out of the `'...'` string in the inline `onclick` handler and runs arbitrary script — the HTML encoder leaves single quotes, parentheses, and semicolons intact because they are valid HTML characters. The "fix" for XSS in body text opened a worse injection vector in inline event attributes.

The correct approach for passing server data to JavaScript is JSON serialization, which is the right encoder for JS literals: `<button data-customer="@Model.CustomerName" data-total="@Model.OrderTotal">` then read `element.dataset.customer` in an external script block, or inline `var name = @Json.Serialize(Model.CustomerName);` inside a script block where the JSON encoder handles all escaping correctly. I never concatenate user input into event-handler attributes or `<script>` blocks manually.

---

#### Q3. (R) Pricing rules live in the view because "it's just display math." After a tax-rate change, invoices are wrong in production but unit tests on the service pass. Review this `_InvoiceLine.cshtml` partial — what breaks and where should this logic live?

```cshtml
@model InvoiceLineViewModel

@{
    var discount = Model.Quantity >= 10 ? Model.UnitPrice * 0.15m : 0m;
    var subtotal = (Model.UnitPrice - discount) * Model.Quantity;
    var tax = subtotal * (Model.IsTaxExempt ? 0m : 0.0825m);
    var lineTotal = subtotal + tax;
}

<tr>
    <td>@Model.Sku</td>
    <td>@lineTotal.ToString("C")</td>
</tr>
```

**Concepts**
- Pricing and tax logic in `.cshtml` — untestable, diverges from other code paths
- Magic rate constants in view — change requires hunting every partial
- View model mapping — controller enriches `InvoiceLineViewModel` with computed values

**Answer**

Business rules in the view — volume discount thresholds, tax rates, tax exemption logic — are invisible to service-layer tests and run on a code path separate from API, PDF, email, and batch calculations. The service tests pass because they test the service, not the view, so a tax rate change in the wrong place fixes the service but leaves the partial using the hardcoded `0.0825m`. Revenue and compliance calculations must have a single authoritative code path.

The fix: move the calculation to a domain service or enrichment step in the controller — `InvoiceLineCalculator.Compute(line)` or populate `LineTotal`, `Tax`, and `Discount` on the view model before passing to `return View(model)`. The view becomes display-only: `@Model.LineTotal.ToString("C")` — the `@{}` business block is deleted entirely. Unit tests cover the calculator with table-driven cases for quantity thresholds and exemption flags. Tax rates live in `IOptions<TaxSettings>` or a configuration database, not hardcoded in Razor.

---

#### Q4. (R) A layout renders the page title from `ViewData`. Some pages show a blank `<title>` with no exception. Review the controller and layout — what fails silently and how do you prevent recurrence?

```csharp
// HomeController.cs
public IActionResult About()
{
    ViewData["Titel"] = "About Us";  // typo — intentional trap
    return View();
}
```

```cshtml
@* _Layout.cshtml *@
<title>@(ViewData["Title"] as string ?? "MyApp")</title>
```

**Concepts**
- `ViewData` magic string typo — `"Titel"` vs `"Title"` silently writes wrong key
- `as string` cast — returns null on missing key, no exception
- Compile-time check — missing for `ViewData`, present for `@model` property
- Shared constants or `[ViewData]` attribute — prevent future typo divergence

**Answer**

`ViewData["Titel"]` and `ViewData["Title"]` are unrelated dictionary entries. The controller writes `"Titel"` and the layout reads `"Title"`, so the null-coalescing `as string ?? "MyApp"` falls back to the app name on every page that uses the About action — no exception, no warning, just the wrong title silently. Magic string keys have no compile-time check, so renames or typos remain invisible until a specific page is visually inspected.

The fix with the highest long-term value is a strongly typed view model with a `Title` property so the compiler catches mismatches. A lighter-weight fix is a shared constant class `ViewDataKeys.Title` used in both controller and layout so the string is defined once. Adding `[ViewData]` on a base controller property also works. An integration test asserting that `GET /Home/About` returns a `<title>` containing "About Us" would have caught this immediately.

---

#### Q5. (D) A dashboard action currently passes six `ViewBag` properties and three `ViewData` keys to one view. The team debates a strongly typed `DashboardViewModel`. When is the refactor worth it, and when is dynamic view data still acceptable?

```csharp
public IActionResult Dashboard()
{
    ViewBag.RecentOrders = _orderService.GetRecent(5);
    ViewBag.AlertCount = _alertService.UnreadCount(User);
    ViewData["LastSync"] = _syncService.LastRunUtc;
    return View();
}
```

**Concepts**
- Strongly typed view model — compile-time safety, IntelliSense, refactor-friendly
- Six+ ViewBag properties — threshold where untyped dictionaries become tech debt
- ViewComponent — alternative for independent data loading per widget
- Acceptable dynamic data — single optional cross-cutting key with documented convention

**Answer**

At six ViewBag properties the accumulation has clearly crossed the threshold where a strongly typed `DashboardViewModel` pays back its introduction cost. Every new `ViewBag.Foo` increases coupling, forces casts in partials, and fails silently when the value is null or the key is misspelled (Q4). A typed model makes the controller-to-view contract explicit, enables IntelliSense, and lets the compiler catch property renames across refactors. Unit tests can construct the view model directly without routing through an HTTP context.

Dynamic ViewData is acceptable for isolated cross-cutting concerns: a single `ViewData["Title"]` for page titles set by convention and read by the layout, or a flash message from a filter where creating a full base view model would be architectural overhead. The line I draw is: if the view or layout reads more than one or two named values that the controller set independently, a typed model is worth it. A View Component is a better intermediate option for the `RecentOrders` widget — it has its own typed view model and data loading without inflating the controller action.

---

#### Q6. (M) After deploy to Production, first page load is fast but subsequent edits to `.cshtml` files on the server appear immediately without redeploy. Staging behaves the same. Review this `Program.cs` setup — what mechanism is active, and why is it a production risk?

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();   // "so designers can tweak CSS wrappers"
var app = builder.Build();
// ... no environment check around runtime compilation
app.Run();
```

**Concepts**
- `AddRazorRuntimeCompilation` — reads `.cshtml` from disk on demand, file watcher recompiles
- No `IsDevelopment()` guard — activates in all environments
- Security risk — unauthorized `.cshtml` edit on server executes arbitrary logic
- Precompiled views — correct Production approach, views in DLL not on disk

**Answer**

`AddRazorRuntimeCompilation()` without an `IsDevelopment()` guard activates in all environments. It watches `.cshtml` files on disk and recompiles them on change, which is why edits on the server apply immediately without redeploy. In Production this is a misconfiguration with three problems: performance (Roslyn compile overhead on cache miss), operational (view state drifts between server nodes if files differ), and security — if an attacker or unauthorized user can write to the views directory, a modified `.cshtml` runs arbitrary C# logic in the application process on the next request, equivalent to remote code execution.

The fix is to register runtime compilation only in Development:

```csharp
var mvc = builder.Services.AddControllersWithViews();
if (builder.Environment.IsDevelopment())
    mvc.AddRazorRuntimeCompilation();
```

Production and Staging rely on precompiled views from the CI publish artifact and require redeploy to change views. The `Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation` package reference can optionally be scoped to Development build configurations to enforce this at the dependency level.

---

#### Q7. (R) A developer centralizes "helper logic" in a view using `@functions` because partial views felt heavy. Under load, SQL time appears in view-render traces. Review this snippet — what is wrong?

```cshtml
@model int  @* categoryId *@

@functions {
    async Task<IEnumerable<Product>> LoadProducts(AppDbContext db, int categoryId)
    {
        return await db.Products
            .Where(p => p.CategoryId == categoryId)
            .ToListAsync();
    }
}

@inject AppDbContext Db

<ul>
@foreach (var p in await LoadProducts(Db, Model))
{
    <li>@p.Name — @p.Price.ToString("C")</li>
}
</ul>
```

**Concepts**
- `@functions` data access — EF query inside view class, bypasses service layer
- SQL in view-render traces — I/O hidden from controller profiling
- `@inject AppDbContext` in view — no caching seam, N+1 risk if partial repeated
- View Component or controller — correct location for data loading

**Answer**

`@functions` is intended for pure presentation helpers — CSS class mappers, label formatters — not database I/O. Injecting `AppDbContext` and querying inside `@functions` puts EF Core directly in the rendering pipeline, which is why SQL appears in view-render traces rather than controller traces. There is no caching seam, no batching opportunity, and no way to unit test the data loading without the view engine. If this partial is rendered multiple times in a loop, each call to `LoadProducts` fires a separate query — the classic N+1 pattern hidden behind Razor markup.

The fix: move `LoadProducts` to a service or repository; call it in the controller before `return View(model)`, and pass the loaded data as part of the view model. The view becomes `@model CategoryViewModel` with `@foreach (var p in Model.Products)` — zero service calls in Razor. Use a View Component if the product list is a reusable widget that needs its own data-loading lifecycle, which is the correct abstraction for independent data loading inside a view hierarchy.

---

#### Q8. (R) Five list views each contain the same 25-line status-badge markup with slightly different CSS classes. A bug fix in one view is missed in the others. What pattern fixes duplication, and what Razor feature misuse often causes this drift?

```cshtml
@* excerpt from Orders/Index.cshtml — duplicated in Shipments, Returns, Quotes, Invoices *@
@{
    string badgeClass = Model.Status switch
    {
        "Pending" => "badge-warning",
        "Shipped" => "badge-info",
        "Delivered" => "badge-success",
        _ => "badge-secondary"
    };
}
<span class="badge @badgeClass">@Model.Status</span>
@if (Model.Status == "Pending" && Model.AgeDays > 7)
{
    <span class="text-danger">Overdue</span>
}
```

**Concepts**
- Duplicated `@{}` blocks across views — bug divergence, no single test surface
- Partial view — centralize shared markup with typed `StatusBadgeViewModel`
- View Component — appropriate when the widget needs independent data
- Tag Helper — per-attribute customization via `asp-status-badge`

**Answer**

Copy-pasting markup across five views creates five divergent code paths where a bug fix in one does not automatically propagate to the others — the exact failure described here. The root cause is skipping partial extraction "to save time" and instead copying inline `@{}` blocks. The switch logic and overdue rule belong in one location with one test.

The fix is a partial view `_StatusBadge.cshtml` accepting a typed `StatusBadgeViewModel` with `Status` and `AgeDays` properties, replacing all five duplicated blocks with `<partial name="_StatusBadge" model="new StatusBadgeViewModel { Status = item.Status, AgeDays = item.AgeDays }" />`. The switch and overdue condition live only in the partial. A View Component is the right choice if the badge also needs to load its own data (for example a real-time staleness indicator); a Tag Helper (`asp-status-badge`) is appropriate if the customization is per-attribute and purely presentation. A snapshot test or unit test on the badge class mapping catches regressions from a single location.

---

#### Q9. (P) Production throws `InvalidOperationException: The view 'Index' was not found` for `Home/Index` after CI publish, but `dotnet run` locally finds the view. The pipeline runs `dotnet publish -c Release -o ./out` and copies only `./out` to the server. What publish or view-layout mistakes cause this, and what do you verify in the artifact?

**Concepts**
- `dotnet run` — uses project source `Views/` on disk
- Publish artifact — may contain precompiled DLL only, or incomplete view tree
- `ProjectName.Views.dll` — precompiled view assembly must be present in output
- Linux case sensitivity — view names case-sensitive on Linux, not Windows
- `.csproj` exclusion — accidental `<Content Remove="Views/**" />` strips views

**Answer**

`dotnet run` sources views from the project `Views/` directory. The publish output of a Release build typically contains only `ProjectName.Views.dll` (precompiled) — no `.cshtml` files — so if that assembly is missing or contains wrong types, or if the view name passed to `View()` does not match the action convention, the runtime produces "view not found." The mismatch is invisible locally because source is always present.

The first thing I check in `./out` is `ProjectName.Views.dll` — if it is absent, precompilation failed silently, likely due to a `.csproj` exclusion like `<Content Remove="Views/**" />` or `<None Remove="Views/**" />` that stripped views before the Razor SDK could compile them. On Linux the file system is case-sensitive: `Index.cshtml` and `index.cshtml` are different files, and Windows development masks a case mismatch. Area views must be at `Areas/{Area}/Views/{Controller}/{View}.cshtml`. An explicit `return View("CustomName")` typo also causes this. To verify before CI promotes: run `dotnet publish -c Release` locally and execute the app from `./out` against a local request — if it fails there it will fail in production.

---

#### Q10. (M) Release builds use Razor precompilation by default in modern SDK-style projects. Explain what happens to `.cshtml` files at build/publish vs first request when precompilation is enabled, and how that differs from runtime compilation.

**Concepts**
- Precompilation — `.cshtml` compiled to `*.Views.dll` at publish, Roslyn not loaded at runtime
- `RazorCompileOnPublish` — SDK default true for Release, controls precompilation
- First request (precompiled) — view engine loads compiled type, no disk `.cshtml` needed
- Runtime compilation — `AddRazorRuntimeCompilation`, file-watcher, Development workflow
- Immutable artifacts — precompiled views change only via redeploy

**Answer**

With Razor precompilation (enabled by default via `RazorCompileOnPublish=true` in Release), the Razor SDK generates C# classes from all `.cshtml` files during `dotnet publish` and compiles them into a `ProjectName.Views.dll` assembly alongside the main app DLL. View syntax errors surface at build time — not first request. At runtime the view engine locates compiled view types in the assembly; `.cshtml` files need not exist on the server, and Roslyn is not loaded. The first request is as fast as subsequent requests because no compilation happens.

Runtime compilation (`AddRazorRuntimeCompilation`) takes a different path: `.cshtml` files must exist on disk at the server, the Roslyn compiler is loaded into the process, and views are compiled on first access with a file watcher triggering recompile on save. This is the mechanism behind the Development hot-reload workflow and the Production misconfiguration in Q6. The contrast is: precompilation means views are immutable DLL artifacts changed only by redeploy; runtime compilation means views are source code that the running process can compile on demand from disk.

---

#### Q11. (R) A category page renders 200 products; each row invokes a synchronous partial that hits `_pricingService.GetTierPrice(productId)` inside the partial. TTFB spikes under concurrent users. Review this view pattern — what performance issues stack here, and what is the prioritized fix?

```cshtml
@model CategoryPageViewModel

<table>
@foreach (var product in Model.Products)
{
    <tr>
        <td>@product.Name</td>
        <td>@await Html.PartialAsync("_ProductPrice", product.Id)</td>
    </tr>
}
</table>
```

```cshtml
@* _ProductPrice.cshtml — @model int *@
@inject IPricingService Pricing
@{
    var price = Pricing.GetTierPrice(Model);  // sync DB/cache per row
}
<span>@price.ToString("C")</span>
```

**Concepts**
- N+1 I/O — 200 synchronous service calls per page render
- Sync I/O on thread pool — blocks threads under concurrent requests
- Partial per row with `@inject` service — hidden from controller profiling
- Batch pricing API — resolve all prices in one call before rendering

**Answer**

The view triggers 200 synchronous `GetTierPrice` calls — one per row via `Html.PartialAsync` rendering a partial that calls the service in a `@{}` block. Awaiting `Html.PartialAsync` does not help when the service call inside is synchronous and blocking — it awaits the partial task but the service internally blocks a thread pool thread per call. At 200 rows under concurrent requests this multiplies to thousands of blocked threads and database connections, causing TTFB spikes and thread pool starvation.

The fix is to move pricing to the controller before rendering. The controller calls `var prices = await _pricing.GetTierPricesAsync(productIds);` — a single batch query or cache round-trip — and maps into `ProductRowViewModel.DisplayPrice` before `return View(model)`. The view becomes `@product.DisplayPrice` with no partial, no injected service, and zero pricing I/O during rendering. If markup reuse requires a partial, the partial accepts a precomputed `ProductRowViewModel` with the price already set. Adding MiniProfiler confirms one pricing query per page after the fix rather than N queries.
