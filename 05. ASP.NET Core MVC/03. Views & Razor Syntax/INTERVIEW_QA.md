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

What is a Razor view?

**Answer:** A Razor view is a `.cshtml` file mixing HTML with C# code using the `@` syntax, compiled into a class that renders HTML output. Razor is the default view engine for ASP.NET Core MVC and supports layouts, partials, tag helpers, and strongly typed models.

- Views live under `Views/{ControllerName}/` or `Views/Shared/` by convention.
- The `@model` directive declares the expected type for compile-time checking and IntelliSense.
- Razor expressions HTML-encode output by default to mitigate XSS.
- Views are precompiled into assemblies at publish in Release builds by default.
- The view engine locates templates using conventional paths and optional area prefixes.

---

## Q2. What is the `@model` directive?

What is the `@model` directive?

**Answer:** The `@model` directive at the top of a view declares the strongly typed model type, accessible as `Model` in the markup. It enables compile-time checking, IntelliSense, and clear contracts between controller and view.

- Example: `@model ProductEditViewModel` lets the view use `@Model.Name` with type safety.
- Only one `@model` directive is allowed per view.
- The controller passes the instance via `return View(viewModel)`.
- Without `@model`, the view relies on untyped `ViewData`/`ViewBag` or `dynamic`.
- Partial views can also declare `@model` for reusable typed fragments.

---

## Q3. What is the difference between `@` and `@@` in Razor?

What is the difference between `@` and `@@` in Razor?

**Answer:** A single `@` transitions from HTML to C# code — variables, expressions, and directives. `@@` renders a literal `@` character in the HTML output, escaping the Razor transition.

- `@DateTime.Now` evaluates the expression and writes the encoded result.
- `@@` produces `@` in output — useful for email addresses or CSS `@media` in `<style>` blocks.
- Directives like `@model`, `@using`, and `@inject` also start with a single `@`.
- Code blocks use `@{ ... }` for multiple statements without inline output.
- Misusing `@` in CSS or JavaScript strings can accidentally invoke Razor parsing.

---

## Q4. How does Razor automatically encode output and why does it matter?

How does Razor automatically encode output and why does it matter?

**Answer:** Razor HTML-encodes expressions written with `@` before writing them to the response, converting characters like `<`, `>`, and `"` to HTML entities. This prevents browser interpretation of user-supplied content as active markup — the primary defense against XSS in server-rendered pages.

- Encoding applies to `@Model.UserComment` in text and most attribute contexts.
- User content rendered without encoding can execute scripts in victims' browsers.
- Encoding is automatic — developers must explicitly opt out with `@Html.Raw()` for trusted HTML.
- Different output contexts (JavaScript, URLs) may need additional encoding beyond HTML encoding.
- ASP.NET Core 8 Razor uses the same encoding pipeline for all standard `@` expressions.

---

## Q5. What is the difference between `@Html.Raw` and default Razor output?

What is the difference between `@Html.Raw` and default Razor output?

**Answer:** Default `@` output HTML-encodes values; `@Html.Raw(string)` writes the string unchanged into the response. Raw should only be used on trusted or server-sanitized HTML — never on unvalidated user input.

- Encoded output displays `<script>` as visible text; Raw executes or injects it as markup.
- Rich text scenarios require an allowlist sanitizer before Raw, not Raw alone.
- Tag helpers and `@` expressions are safe by default for typical display scenarios.
- Stored XSS vulnerabilities commonly come from `@Html.Raw(Model.UserContent)` on database fields.
- Prefer encoding unless the content is known safe or has been sanitized server-side.

---

## Q6. What is a code block (`@{ }`) in Razor?

What is a code block (`@{ }`) in Razor?

**Answer:** A Razor code block wraps arbitrary C# statements that do not directly emit output — variable declarations, loops with manual markup, conditionals, and method calls. It separates control logic from inline expressions.

- `@{ var count = Model.Items.Count; }` declares variables for later use in markup.
- Multi-line logic like `if/else` with HTML mixed inside uses `@if` or code blocks.
- Code blocks should contain presentation logic only — not database queries or business rules.
- `@{}` at the top level runs during view rendering on each request.
- Excessive logic in code blocks signals the need for view models or view components.

---

## Q7. What is the difference between a strongly typed view and a dynamic view?

What is the difference between a strongly typed view and a dynamic view?

**Answer:** A strongly typed view declares `@model MyViewModel` and accesses typed properties via `Model`. A dynamic view uses `ViewBag`, `ViewData`, or no model — relying on runtime dictionary keys or `dynamic` without compile-time checks.

- Strong typing catches property renames at compile time and enables IntelliSense in the view.
- Dynamic views fail silently on typos in `ViewBag.Title` vs `ViewBag.Titel`.
- Strongly typed partials enforce contracts when reused across pages.
- Dynamic data is acceptable for single optional messages but scales poorly on complex pages.
- Controllers pass typed models with `return View(myViewModel)` for the strongly typed approach.

---

## Q8. What logic should not belong in a Razor view?

What logic should not belong in a Razor view?

**Answer:** Views should not contain business rules, data access, authorization decisions, or complex calculations — only presentation formatting and layout. Any logic that affects correctness, security, or money belongs in services with the controller supplying ready-to-display view models.

- Database queries via `@inject DbContext` in views bypass service-layer testing.
- Pricing, tax, and discount calculations duplicated in views drift from API and batch job logic.
- Authorization checks in Razor can be bypassed by alternate routes or direct API calls.
- Heavy `@{}` blocks with business `if` chains belong in application services or view model mapping.
- Views may format dates and currencies but should not decide business outcomes.

---

## Q9. What is `@inject` used for in Razor views?

What is `@inject` used for in Razor views?

**Answer:** The `@inject` directive requests a service from DI into the view — creating a property the Razor page can use during rendering. It suits small presentation helpers like localization or configuration, not data access or business services.

- Example: `@inject IViewLocalizer Localizer` then `@Localizer["Key"]` in markup.
- Injected services follow DI lifetimes — scoped services align with the request.
- Overusing `@inject` for repositories encourages fat views and N+1 query patterns.
- View Components are the preferred pattern when a view needs its own data-loading logic.
- Services injected into views should be presentation-oriented, not domain repositories.

---

## Q10. What is the `@functions` block in Razor?

What is the `@functions` block in Razor?

**Answer:** The `@functions` block declares methods and properties on the generated view class — typically small presentation helpers like CSS class mappers or formatters. It is not intended for data access or business logic.

- Example: `@functions { string StatusClass(string s) => s == "Active" ? "green" : "red"; }`
- Functions are callable from the markup in the same view.
- Async data loading in `@functions` is an anti-pattern — load data in the controller or view component.
- Helpers duplicated across views should move to shared partials, tag helpers, or static helper classes.
- `@functions` compiles into the view's generated class at build or runtime compilation time.

---

## Q11. What is the difference between Razor runtime compilation and precompilation?

What is the difference between Razor runtime compilation and precompilation?

**Answer:** Precompilation compiles `.cshtml` files into assemblies at build or publish time, so production serves views from DLLs without Roslyn at runtime. Runtime compilation reads `.cshtml` from disk and compiles on demand with file watching — intended for Development hot reload.

- Release publish defaults to precompiled views in `ProjectName.Views.dll`.
- Runtime compilation requires `AddRazorRuntimeCompilation()` and the runtime compilation NuGet package.
- Precompilation catches view syntax errors at build time instead of first request.
- Runtime compilation adds CPU overhead and requires `.cshtml` files on the server.
- Production should use precompilation; runtime compilation should be guarded with `IsDevelopment()`.

---

## Q12. When would you enable `AddRazorRuntimeCompilation`?

When would you enable `AddRazorRuntimeCompilation`?

**Answer:** Enable `AddRazorRuntimeCompilation` during local Development so editing `.cshtml` files takes effect without rebuilding the entire project. It should not run in Production because it adds compile cost, requires source files on disk, and widens the attack surface if files can be modified.

- Register conditionally: `if (builder.Environment.IsDevelopment()) mvcBuilder.AddRazorRuntimeCompilation();`
- Useful when iterating on layout, CSS classes, and markup with fast feedback.
- Staging and Production rely on publish artifacts and redeploy to change views.
- Missing guard means view edits on a server apply immediately — a misconfiguration signal.
- Requires the `Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation` package.

---

## Q13. What is a partial view and when do you use one?

What is a partial view and when do you use one?

**Answer:** A partial view is a reusable Razor fragment without a layout, rendered into a parent view for shared markup such as form fields, cards, or pagination. Use partials to avoid copy-pasting HTML across multiple views with the same UI component.

- Partial views live in `Views/Shared/` or controller-specific folders, often prefixed with `_`.
- Pass a strongly typed model with `<partial name="_ProductCard" model="item" />`.
- Partials do not run `_ViewStart` layout wrapping — they render inline content only.
- Use partials for static markup reuse; use View Components when independent data loading is needed.
- Returning `PartialView()` from an action sends HTML fragments for AJAX replacement.

---

## Q14. What is the difference between `Html.PartialAsync` and the `<partial>` tag helper?

What is the difference between `Html.PartialAsync` and the `<partial>` tag helper?

**Answer:** Both render a partial view, but `<partial>` is the recommended tag helper syntax while `Html.PartialAsync` is the older HTML helper API. The tag helper integrates with Razor tooling and avoids some synchronous rendering pitfalls of older helper patterns.

- `<partial name="_LoginPartial" model="Model.User" />` is declarative and preferred in ASP.NET Core 8.
- `PartialAsync` returns `Task<IHtmlContent>` and must be awaited: `@await Html.PartialAsync("_Name", model)`.
- Avoid `Html.Partial` (sync) — it can deadlock in certain contexts; use `PartialAsync` or the tag helper.
- Both resolve partial views by name using the same view engine location expander.
- Tag helpers participate in Razor compilation and attribute IntelliSense.

---

## Q15. What is `ViewData` and how is it accessed in Razor?

What is `ViewData` and how is it accessed in Razor?

**Answer:** `ViewData` is a `ViewDataDictionary` passed from controller to view for untyped key-value data surviving a single request. Access values with string keys: `ViewData["Title"]` in the controller and `@ViewData["Title"]` in the view.

- Values are `object` — casts may fail silently or require `(string)ViewData["Key"]`.
- `ViewData` shares backing storage with `ViewBag` in the same request.
- Common for page titles and layout metadata when a full view model is not used.
- Magic string keys typo easily — shared constants or strongly typed models are safer.
- `ViewData` does not survive redirects; use `TempData` for cross-request flash data.

---

## Q16. What causes "The view 'X' was not found" errors?

What causes "The view 'X' was not found" errors?

**Answer:** The view engine cannot locate a matching `.cshtml` file or precompiled view for the requested name — usually due to wrong path conventions, case sensitivity on Linux, missing publish artifacts, or explicit view names that do not exist.

- `return View()` expects `Views/{Controller}/{Action}.cshtml` unless a different name is passed.
- Linux deployments are case-sensitive — `Index.cshtml` vs `index.cshtml` fails on Linux only.
- Publish output may lack `Views/` folder or `ProjectName.Views.dll` if misconfigured.
- Area views require `Areas/{Area}/Views/{Controller}/{View}.cshtml`.
- Typos in `return View("CustomName")` or wrong controller name in routing cause mismatches.

---

## Q17. What is `_ViewImports.cshtml` used for?

What is `_ViewImports.cshtml` used for?

**Answer:** `_ViewImports.cshtml` applies shared directives to all views in its folder and subfolders — `@using` namespaces, `@addTagHelper`, `@inject`, and `@model` inheritance via `@inherits` patterns. It reduces repetition across views.

- Place `@using MyApp.ViewModels` once instead of in every view.
- `@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers` enables tag helpers project-wide.
- Nested `_ViewImports.cshtml` in areas merge with parent imports.
- It does not render HTML — only sets compilation context for child views.
- Root `Views/_ViewImports.cshtml` applies to all views unless overridden deeper.

---

## Q18. What is `_ViewStart.cshtml` used for?

What is `_ViewStart.cshtml` used for?

**Answer:** `_ViewStart.cshtml` runs before each view renders, typically setting the layout with `Layout = "_Layout"`. It centralizes layout assignment so individual views do not repeat the layout directive.

- Located at `Views/_ViewStart.cshtml` and optionally in area view folders.
- Sets `Layout` property — views can override with `Layout = null` or a different layout path.
- Executes in hierarchical order from root to area-specific `_ViewStart`.
- Does not replace `_ViewImports` — imports handle namespaces; view start handles layout.
- Child views focus on content while `_ViewStart` wraps them in the shared chrome.

---

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

---

**Answer:**

**Answer:** `@Html.Raw` bypasses Razor's HTML encoding, so any `<script>` or event-handler markup stored in `comment.Body` executes in the victim's browser — classic stored XSS. Default `@comment.Body` is safe for plain text but still wrong if you intentionally allow a subset of HTML.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | `@Html.Raw` on untrusted DB content | Stored XSS — account takeover, session theft |
| Data handling | No sanitization at write or read | Malicious payload persists and affects every viewer |
| Design | Conflating "rich text" with "raw HTML" | Team thinks Razor "fixed" XSS by using Raw for formatting |

**Fix (priority order):**

1. **Plain text comments:** Render with `@comment.Body` (encoded) — never `Raw` on user input.
2. **Allowed rich text:** Sanitize server-side on save (and optionally on read) with an allowlist HTML sanitizer (e.g. Ganss.XSS / HtmlSanitizer) — strip `<script>`, `onerror=`, `javascript:` URLs; then you may use `Raw` on the **sanitized** output only.
3. Encode at the trust boundary: treat DB comment bodies as untrusted even from "internal" users.
4. Add CSP headers and HttpOnly cookies as defense in depth — not a substitute for encoding/sanitization.

**Production takeaway:** Razor `@` expressions HTML-encode by default; `@Html.Raw` is an explicit opt-out — use only on trusted or sanitized content. Karat pairs this with "QA passed" to test whether you trust happy-path test data over threat modeling.

---

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

---

**Answer:**

**Answer:** HTML encoding (`@Model.CustomerName` in attribute/text context) does not make values safe inside **JavaScript** string literals. A name like `'); alert(document.cookie);//` breaks out of the `'...'` string in `onclick` and runs arbitrary script — an encoding-context XSS.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | User data embedded in inline JS without JS encoding | XSS via `CustomerName` even when HTML encoding works elsewhere |
| Context | HTML encoder ≠ JavaScript string encoder | "Fixed" XSS in body but opened a worse vector in attributes |
| Design | Inline `onclick` with interpolated user data | Hard to audit; duplicates across views |

**Fix (priority order):**

1. Remove inline handlers; attach listeners in a script block using **`Json.Serialize(Model.CustomerName)`** (or `JsonSerializer.Serialize`) for JS string contexts — JSON encoding is correct for JS literals.
2. Prefer **`data-*` attributes** + one external script: `<button data-customer="@Model.CustomerName" data-total="@Model.OrderTotal">` and read with `element.dataset` (still encode if injecting into JS strings).
3. Never concatenate user input into `<script>` or event-handler attributes manually.
4. Use tag helpers / Content Security Policy (`script-src`) to limit inline script execution.

**Production takeaway:** Karat tests **encoding context** — HTML, JavaScript, URL, and CSS each need the right encoder. `@` in Razor is HTML-safe, not JavaScript-safe.

---

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

---

**Answer:**

**Answer:** Business rules (volume discount thresholds, tax rates, exemptions) duplicated in the view are invisible to service-layer tests and drift from API/PDF/email calculations. The view renders one number while checkout and reporting use different code paths — silent revenue and compliance bugs.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Design | Pricing/tax logic in `.cshtml` | Untested, unreusable across PDF, API, batch jobs |
| Maintainability | Magic numbers (`0.15m`, `0.0825m`) in view | Tax change requires hunting every partial |
| Correctness | `lineTotal` computed only at render time | Email template or export using service math disagrees with UI |

**Fix (priority order):**

1. Move calculation to a **domain/service** method (e.g. `InvoiceLineCalculator.Compute(line)` or enrich `InvoiceLineViewModel` in the controller/mapper with `LineTotal`, `Tax`, `Discount`).
2. View becomes display-only: `@Model.LineTotal.ToString("C")` — no `@{}` business block.
3. Unit-test the calculator with table-driven cases (quantity 9 vs 10, tax-exempt flag).
4. Single source of truth for rates: `IOptions<TaxSettings>` or database-driven rates injected into the service, not hard-coded in Razor.

**Production takeaway:** Views should **format and layout**, not decide money. Karat uses "service tests pass" to trap candidates who test only the layer they own.

---

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

---

**Answer:**

**Answer:** `ViewData["Titel"]` and `ViewData["Title"]` are unrelated keys — the typo writes a value the layout never reads, so `(ViewData["Title"] as string ?? "MyApp")` falls through to the default or empty cast without throwing. Magic strings fail at runtime with no compile-time check.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Maintainability | Typo in ViewData key `"Titel"` vs `"Title"` | Wrong or default `<title>` — SEO, tabs, accessibility |
| Type safety | `ViewData` is `object`- keyed dictionary | No IntelliSense; refactors don't rename keys |
| Observability | Silent fallback masks bug | Production pages ship with generic title |

**Fix (priority order):**

1. **Strongly typed view model** with `Title` property — compiler catches renames.
2. If keeping ViewData temporarily, **shared constants**: `ViewDataKeys.Title` used in both controller and layout.
3. `@model PageViewModel` with `[ViewData]` attribute or `_ViewStart` setting base title pattern.
4. Integration test: GET `/Home/About` asserts `<title>` contains "About Us".

**Production takeaway:** ViewData/ViewBag magic strings are a common Karat trap — prefer strongly typed models for anything that must be consistent across controller and view.

---

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

---

**Answer:**

**Answer:** Refactor to `DashboardViewModel` when the view depends on multiple named pieces of data, you need compile-time safety, or the same shape is reused across actions/tests. Dynamic `ViewBag`/`ViewData` is acceptable only for trivial, single-key throwaway pages or layout cross-cutting keys with documented conventions.

- **Worth strongly typed:** Dashboard with 6+ properties, partial views expecting specific keys, API + MVC sharing shape, frequent refactors, multiple developers — eliminates magic-string bugs (see Q4) and enables view `@model` IntelliSense.
- **Acceptable dynamic:** One-off admin diagnostic page, prototype spike, or passing a single optional banner message from a filter where a shared `LayoutViewModel` would be heavy.
- **Middle ground:** `ViewComponent` with its own view model for `RecentOrders` and `AlertCount` — keeps action slim and boundaries clear.
- **Cost of delay:** Every new `ViewBag.Foo` increases coupling; partials casting `ViewBag.RecentOrders as IEnumerable<Order>` fail silently when null.

**Production takeaway:** Karat favors judgment — not "never ViewBag," but "this dashboard crossed the threshold where untyped dictionaries are tech debt."

---

---

#### Q6. (M) After deploy to Production, first page load is fast but subsequent edits to `.cshtml` files on the server appear immediately without redeploy. Staging behaves the same. Review this `Program.cs` / project setup — what mechanism is active, and why is it a production risk?

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();   // "so designers can tweak CSS wrappers"
var app = builder.Build();
// ... no environment check around runtime compilation
app.Run();
```

*(`.csproj` has no special Razor publish settings; Release build deployed to Linux.)*

---

**Answer:**

**Answer:** `AddRazorRuntimeCompilation()` compiles `.cshtml` from disk on demand (with file watching), so hot-editing views on the server works without redeploy. That is desirable in Development only; in Production it exposes compilation overhead, file-system dependency, and a **code injection surface** if an attacker can write to the views directory.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | Runtime compile of changed `.cshtml` on server | Unauthorized view change → RCE-equivalent markup/logic in app process |
| Performance | Roslyn compile on cache miss / file change | CPU spikes, slower cold requests vs precompiled views |
| Deployment | Views expected to be immutable artifacts | Drift between nodes if one server’s files differ |
| Configuration | No `if (env.IsDevelopment())` guard | Same behavior in Staging/Production |

**Fix (priority order):**

1. Register runtime compilation **only in Development**:

   ```csharp
   var mvc = builder.Services.AddControllersWithViews();
   if (builder.Environment.IsDevelopment())
       mvc.AddRazorRuntimeCompilation();
   ```

2. Production: rely on **Razor precompilation at publish** (default in Release) and redeploy to change views.
3. Remove package `Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation` from Production deployments if unused.
4. Ensure CI publishes immutable artifacts — no manual `.cshtml` edits on servers.

**Production takeaway:** If views change without redeploy, runtime compilation is almost certainly enabled — treat that as a misconfiguration, not a feature.

---

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

---

**Answer:**

**Answer:** `@functions` blocks belong to the view class for **presentation helpers** (formatting, CSS class mapping) — not data access. Injecting `AppDbContext` and querying inside `@functions` puts I/O in the view layer, bypasses controller/service testing, and runs per render with no clear transaction boundary.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Architecture | EF query in `.cshtml` via `@functions` | Business/data layer leak; untestable without view engine |
| Performance | `ToListAsync` per category page render | DB load under traffic; no caching seam |
| DI/lifetime | DbContext in view with `@inject` | Works scoped per request but hides N+1 if partial repeats |
| Maintainability | Async logic in generated view class | Harder to mock than service interface |

**Fix (priority order):**

1. Load products in **controller or view model factory**: `return View(new CategoryPageViewModel { Products = await _productService.GetByCategoryAsync(id) });`
2. View only iterates: `@foreach (var p in Model.Products)`.
3. Use **ViewComponent** if the query is reusable widget logic with its own view model — still no DbContext in `.cshtml`.
4. Reserve `@functions` for pure helpers, e.g. `string StatusBadgeClass(string status)`.

**Production takeaway:** `@functions` abuse is a Karat code-review signal — if SQL appears in view traces, move data up one layer.

---

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

---

**Answer:**

**Answer:** Copy-paste across `.cshtml` files diverges because partial extraction or view components were skipped "to save time." Centralize markup in a **partial view** or **View Component** with a small view model (`StatusBadgeViewModel` with `Status`, `AgeDays`, optional `CssVariant`).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Maintainability | Identical switch + overdue rule in five views | Bug fixed in one place only — inconsistent UI |
| Design | Inline `@{}` logic blocks instead of shared partial | No single test surface for badge rules |
| DRY | Slightly different CSS per page copied wholesale | Fear of breaking one page blocks refactor |

**Fix (priority order):**

1. Extract `_StatusBadge.cshtml` partial or `StatusBadgeViewComponent` accepting `StatusBadgeModel`.
2. Replace duplicated blocks with `<partial name="_StatusBadge" model="..." />` or `@await Component.InvokeAsync("StatusBadge", ...)`.
3. Move `badgeClass` switch and overdue rule to one file (or tag helper `asp-status-badge`).
4. Add a snapshot/Razor test or single unit test for badge class mapping.

**Production takeaway:** Duplicate view logic is a maintenance defect — Karat expects **partial vs View Component** trade-off: partial for simple markup; View Component when independent data loading or encapsulation is needed.

---

---

#### Q9. (P) Production throws `InvalidOperationException: The view 'Index' was not found` for `Home/Index` after CI publish, but `dotnet run` locally finds the view. The pipeline runs `dotnet publish -c Release -o ./out` and copies only `./out` to the server. What publish/view-layout mistakes cause this, and what do you verify in the artifact?

---

**Answer:**

**Answer:** `dotnet run` uses project source (including `Views/` on disk); publish output may contain **precompiled assemblies only** or an incomplete view tree depending on `.csproj` settings, paths, and what the deploy step copies. The runtime searches `Views/Home/Index.cshtml` and compiled view locations — if neither exists in `./out`, you get "view not found."

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Publish | `<CopyRazorGenerateFilesToPublishDirectory>false</CopyRazorGenerateFilesToPublishDirectory>` (default) + precompilation failure | No `.cshtml` and no compiled view in output |
| Layout | Views outside `Views/` convention or wrong area path | Works locally with custom content paths; fails on server |
| Deploy | Copying trimmed artifact missing `Views` or `*.Views.dll` | Intermittent "works on my machine" |
| Case sensitivity | `Index.cshtml` vs `index.cshtml` on Linux | Windows dev OK; Linux prod 404 view |

**Fix (priority order):**

1. Inspect publish folder: presence of **`ProjectName.Views.dll`** (precompiled) **or** `Views/Home/Index.cshtml` if copying cshtml to publish.
2. Ensure `.csproj` does not exclude views: check `<Content Remove="Views/**" />`, `<None Remove=...>`, wrong `RazorCompileOnPublish`.
3. Fix view path: action returns `View()` → expects `Views/Home/Index.cshtml` (or explicit `return View("~/Views/...")`).
4. Match Linux casing; run `dotnet publish` locally and execute from `./out` before CI promote.
5. If using runtime compilation in prod (see Q6), package must include `.cshtml` files on disk.

**Production takeaway:** Always validate **`dotnet publish` output**, not `dotnet run` source tree — Karat ties MVC view errors to deployment artifacts.

---

---

#### Q10. (M) Release builds use Razor precompilation by default in modern SDK-style projects. Explain what happens to `.cshtml` files at **build/publish** vs **first request** when precompilation is enabled, and how that differs from runtime compilation.

---

**Answer:**

**Answer:** With **Razor precompilation** (`RazorCompileOnBuild` / `RazorCompileOnPublish`, default true for publish), `.cshtml` files are compiled into assemblies (e.g. `MyApp.Views.dll`) at build/publish time. At runtime the view engine loads precompiled types — **no Roslyn compile on first request**; `.cshtml` may not be deployed to the server at all.

- **Build/publish (precompiled):** Razor SDK generates C# from views → compiled into views assembly; errors surface at build time; faster startup and steady-state render.
- **First request (precompiled):** View locator finds compiled view type — disk `.cshtml` optional unless configured to copy for editing.
- **Runtime compilation (`AddRazorRuntimeCompilation`):** Original `.cshtml` on disk parsed/compiled when needed; file watcher recompiles on change — Development workflow, not Production default.
- **Contrast:** Precompilation = immutable views in DLL; runtime compilation = views are source code at runtime.

**Production takeaway:** Know which mode your pipeline uses — "change cshtml on server" (Q6) means runtime compilation; proper Release deploy means **redeploy DLL** to change views.

---

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

---

**Answer:**

**Answer:** The view triggers **O(n) synchronous service/DB calls** — one per row — via `Html.PartialAsync` rendering a partial that calls `GetTierPrice` inside `@{}`. Partials are fine for markup reuse but amplify N+1 when each invocation does I/O; async partial doesn't help if the service call is sync and repeated 200 times per request.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance | 200 × `GetTierPrice` per page | N+1 queries; thread pool blocked on sync I/O |
| Architecture | Pricing lookup inside partial | Hidden from controller profiling; duplicated if row reused |
| Razor | `@foreach` + partial per item with `@inject` service | Compiles but scales linearly with catalog size |
| Caching | No batch API | Cache misses multiply latency |

**Fix (priority order):**

1. **Batch in controller/service** before render: `var prices = await _pricing.GetTierPricesAsync(productIds);` map into `ProductRowViewModel.Price`.
2. View/partials display only: `@product.DisplayPrice` — zero service calls in `.cshtml`.
3. Replace per-row partial with inline markup or single partial taking **precomputed** `ProductRowViewModel` if markup reuse needed.
4. If pricing must stay dynamic, add **bulk endpoint + cache** (`IMemoryCache` keyed by product set hash); never sync DB in loop.
5. Measure with MiniProfiler — confirm one query (or one cache round-trip) per page after fix.

**Production takeaway:** Complex Razor performance issues are usually **N+1 I/O disguised as partial reuse** — fix data shape before micro-optimizing Razor syntax.

---

---
