# Tag Helpers — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What are Tag Helpers in ASP.NET Core MVC?](#q1-what-are-tag-helpers-in-aspnet-core-mvc)
2. [Q2. What is the difference between Tag Helpers and HTML Helpers?](#q2-what-is-the-difference-between-tag-helpers-and-html-helpers)
3. [Q3. What does `asp-for` do on an input element?](#q3-what-does-asp-for-do-on-an-input-element)
4. [Q4. What do `asp-action` and `asp-controller` do on a form or anchor?](#q4-what-do-asp-action-and-asp-controller-do-on-a-form-or-anchor)
5. [Q5. What is `asp-validation-for`?](#q5-what-is-asp-validation-for)
6. [Q6. What is `asp-validation-summary`?](#q6-what-is-asp-validation-summary)
7. [Q7. How do Tag Helpers generate antiforgery tokens for forms?](#q7-how-do-tag-helpers-generate-antiforgery-tokens-for-forms)
8. [Q8. What is `asp-route-*` used for?](#q8-what-is-asp-route--used-for)
9. [Q9. What does `asp-append-version` do?](#q9-what-does-asp-append-version-do)
10. [Q10. What is the `<environment>` tag helper used for?](#q10-what-is-the-environment-tag-helper-used-for)
11. [Q11. How are Tag Helpers registered in `_ViewImports.cshtml`?](#q11-how-are-tag-helpers-registered-in-_viewimportscshtml)
12. [Q12. What are `@addTagHelper` and `@removeTagHelper`?](#q12-what-are-addtaghelper-and-removetaghelper)
13. [Q13. What is the difference between `<partial>` and `Html.PartialAsync` as a tag helper?](#q13-what-is-the-difference-between-partial-and-htmlpartialasync-as-a-tag-helper)
14. [Q14. What is the `!` prefix (opt-out) on Tag Helpers?](#q14-what-is-the-prefix-opt-out-on-tag-helpers)
15. [Q15. How does a custom Tag Helper work (`TagHelper` base class)?](#q15-how-does-a-custom-tag-helper-work-taghelper-base-class)
16. [Q16. What is Tag Helper processing order and why does it matter?](#q16-what-is-tag-helper-processing-order-and-why-does-it-matter)
17. [Q17. How do you register Tag Helpers from a Razor Class Library?](#q17-how-do-you-register-tag-helpers-from-a-razor-class-library)
18. [Q18. What HTML attributes do Tag Helpers emit for client-side validation (`data-val-*`)?](#q18-what-html-attributes-do-tag-helpers-emit-for-client-side-validation-data-val)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What are Tag Helpers in ASP.NET Core MVC?

What are Tag Helpers in ASP.NET Core MVC?

**Answer:** Tag Helpers are server-side components that participate in Razor view rendering by transforming HTML-like elements and attributes into valid, route-aware, model-bound markup. They replace many HTML Helpers with a more natural HTML-centric syntax in `.cshtml` files.

- They run at view compilation/render time and can add, remove, or modify tags and attributes based on `ViewContext` and model metadata.
- Built-in helpers live in `Microsoft.AspNetCore.Mvc.TagHelpers` and cover forms, links, scripts, environments, and partials.
- Tag Helpers are opt-in per element via `asp-*` attributes or target elements registered with `[HtmlTargetElement]`.
- ASP.NET Core 8 MVC continues Tag Helpers as the default approach in project templates alongside fully supported HTML Helpers.

---

## Q2. What is the difference between Tag Helpers and HTML Helpers?

What is the difference between Tag Helpers and HTML Helpers?

**Answer:** HTML Helpers are C# methods on `IHtmlHelper` (e.g., `@Html.TextBoxFor`) that return `IHtmlContent` strings. Tag Helpers are classes that process HTML elements in Razor, keeping markup readable and closer to designer-friendly HTML.

- Tag Helpers understand `asp-*` attributes; HTML Helpers use method parameters and anonymous objects for HTML attributes.
- Both generate similar output for forms and validation, but Tag Helpers integrate more cleanly with IntelliSense in HTML elements.
- HTML Helpers remain supported and are preferable for dynamic HTML built in code or heavily customized editor templates.
- Tag Helpers participate in a ordered pipeline and can wrap child content; HTML Helpers are single method calls.

---

## Q3. What does `asp-for` do on an input element?

What does `asp-for` do on an input element?

**Answer:** `asp-for="PropertyName"` binds the input to a model property expression, generating correct `name`, `id`, and `value` attributes for model binding and labeling. It also applies validation and display metadata from data annotations.

- For `EditViewModel.Title`, it emits `name="Title"`, `id="Title"`, and the current value for POST round trips.
- It adds `data-val-*` attributes when client validation is enabled and the property has validation metadata.
- Expression trees support nested paths (`asp-for="Address.City"`) and respect HTML field prefixes in partial views.
- The `@model` type must match the property expression; mismatched model types break validation key alignment.

---

## Q4. What do `asp-action` and `asp-controller` do on a form or anchor?

What do `asp-action` and `asp-controller` do on a form or anchor?

**Answer:** These attributes tell the Form or Anchor Tag Helper which controller action URL to generate using the application's route table and ambient route values. They produce correct `action` or `href` URLs without hardcoding paths.

- Omitted values default to the current controller or action from route data (ambient values).
- Combined with `asp-route-id` or other `asp-route-*` attributes, they append route parameters for link generation.
- Form Tag Helper also sets HTTP method via `method="post"` and injects antiforgery tokens for POST forms.
- For areas, `asp-area` must be set explicitly when linking across area boundaries.

---

## Q5. What is `asp-validation-for`?

What is `asp-validation-for`?

**Answer:** `asp-validation-for` is a Tag Helper that renders a `<span class="field-validation-valid">` (or error class) displaying the first validation error for the specified model property from `ModelState`.

- It uses the same expression as `asp-for` so the `ModelState` key matches the posted field name.
- On validation failure after POST, the span shows the server error message from annotations or custom validation.
- With unobtrusive scripts loaded, jQuery Validate also populates this span on the client before submit.
- Place it adjacent to the corresponding input for accessible error association.

---

## Q6. What is `asp-validation-summary`?

What is `asp-validation-summary`?

**Answer:** `asp-validation-summary` renders a `<div>` listing validation errors from `ModelState` according to the selected summary mode (`All` or `ModelOnly`).

- It is typically placed at the top of a form to show model-level or consolidated errors.
- The helper adds `validation-summary-errors` CSS class when errors exist.
- Works with both server-returned errors and client-side unobtrusive validation blocking submit.
- Avoid duplicating property errors in the summary when per-field `asp-validation-for` spans are already present unless using `ModelOnly`.

---

## Q7. How do Tag Helpers generate antiforgery tokens for forms?

How do Tag Helpers generate antiforgery tokens for forms?

**Answer:** The Form Tag Helper automatically injects a hidden `<input>` with the antiforgery token when the form uses `method="post"`. No manual token markup is required when using `<form asp-action="..." method="post">`.

- The token pairs with `[ValidateAntiForgeryToken]` or global `[AutoValidateAntiforgeryToken]` on the target action.
- GET forms do not receive tokens because GET should be idempotent and not mutate state.
- AJAX requests must manually send the token via header `RequestVerificationToken` or form field when not using the Form Tag Helper.
- Token generation uses `IAntiforgery` services configured in `AddControllersWithViews`.

---

## Q8. What is `asp-route-*` used for?

What is `asp-route-*` used for?

**Answer:** `asp-route-{parameterName}` supplies route values when generating URLs for anchors and forms, where `{parameterName}` matches a route template token or action parameter name.

- Example: `asp-route-id="@Model.ProductId"` on `<a asp-action="Details">` produces `/Products/Details/5`.
- Multiple `asp-route-*` attributes map to named segments in attribute routes or conventional routes.
- Values merge with ambient route data from the current request unless overridden.
- Unknown route values may become query string parameters depending on the route template.

---

## Q9. What does `asp-append-version` do?

What does `asp-append-version` do?

**Answer:** `asp-append-version="true"` on `<script>` or `<link>` Tag Helpers appends a cache-busting query string (file hash) to static file URLs so browsers fetch new versions after deployment.

- Works with files served through static files middleware from `wwwroot`.
- Prevents stale JavaScript or CSS after releases without manual version query strings.
- The hash changes when file content changes; unchanged files keep stable URLs for caching.
- Commonly used in layout files for bundled app scripts and styles.

---

## Q10. What is the `<environment>` tag helper used for?

What is the `<environment>` tag helper used for?

**Answer:** The `<environment>` Tag Helper conditionally renders its child content based on the hosting environment name (`Development`, `Staging`, `Production`, or custom names from `IWebHostEnvironment`).

- Use `include="Development"` to load unminified scripts or Browser Link only during local development.
- Use `exclude="Development"` to load CDN production assets with fallback tags in non-development environments.
- Multiple environment names can be comma-separated in `include` or `exclude`.
- It avoids runtime `if (env.IsDevelopment())` blocks cluttering layout markup.

---

## Q11. How are Tag Helpers registered in `_ViewImports.cshtml`?

How are Tag Helpers registered in `_ViewImports.cshtml`?

**Answer:** `_ViewImports.cshtml` at the `Views` folder (and Area views) registers Tag Helpers for all views in that folder tree using `@addTagHelper` directives.

- The standard line `@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers` enables all built-in MVC Tag Helpers.
- Custom Tag Helpers register with `@addTagHelper *, YourAssemblyName`.
- Child folders inherit imports from parent `_ViewImports.cshtml` files unless overridden.
- Registration scope means helpers apply to every view under that path without repeating directives.

---

## Q12. What are `@addTagHelper` and `@removeTagHelper`?

What are `@addTagHelper` and `@removeTagHelper`?

**Answer:** `@addTagHelper` imports Tag Helper types or assemblies into the Razor compilation scope for the current folder and descendants. `@removeTagHelper` excludes specific helpers where global registration would conflict with legacy markup.

- Syntax: `@addTagHelper [namespace.]TagHelperName, AssemblyName` or wildcard `*` for all helpers in an assembly.
- `@removeTagHelper` is useful in a subfolder's `_ViewImports` when third-party HTML must not be rewritten by a broad helper target.
- Removal is scoped to the folder tree where the directive appears.
- Order matters only insofar as remove directives must reference helpers already added by parent imports.

---

## Q13. What is the difference between `<partial>` and `Html.PartialAsync` as a tag helper?

What is the difference between `<partial>` and `Html.PartialAsync` as a tag helper?

**Answer:** Both render a partial view, but `<partial name="_Name" model="..." />` is declarative HTML-like syntax while `@await Html.PartialAsync("_Name", model)` is an explicit helper call returning `IHtmlContent`.

- The `<partial>` Tag Helper resolves the partial by name with the same view location conventions as `PartialAsync`.
- Tag Helper syntax avoids `@await` in the middle of HTML blocks and reads cleaner in designer-friendly markup.
- Both execute the partial asynchronously and do not run a controller action — they render a view fragment only.
- Pass a strongly typed model to either approach; avoid `ViewBag` for partial data when type safety matters.

---

## Q14. What is the `!` prefix (opt-out) on Tag Helpers?

What is the `!` prefix (opt-out) on Tag Helpers?

**Answer:** Prefixing an attribute with `!` (e.g., `<input !name="custom" />`) opts that element out of Tag Helper processing so the Razor engine leaves the attribute and tag unchanged.

- Useful when a literal HTML attribute would otherwise match a Tag Helper target and be rewritten unexpectedly.
- Alternative opt-out: omit all `asp-*` attributes on elements that should not be processed, or use `@removeTagHelper` for broader suppression.
- Common during migration from HTML Helpers when mixing literal markup with Tag Helper-enabled views.
- The opt-out applies per attribute, not globally to the entire element unless no `asp-*` triggers remain.

---

## Q15. How does a custom Tag Helper work (`TagHelper` base class)?

How does a custom Tag Helper work (`TagHelper` base class)?

**Answer:** A custom Tag Helper inherits `TagHelper`, is decorated with `[HtmlTargetElement(...)]` to declare which tags and attributes it targets, and overrides `Process` or `ProcessAsync` to modify `TagHelperOutput`.

- `TagHelperContext` provides element name and attribute values; `TagHelperOutput` is the mutable result tag.
- Set `output.TagName`, `Attributes`, `Content`, or `TagMode` to transform or suppress the element.
- Register the helper assembly in `_ViewImports.cshtml` with `@addTagHelper`.
- Use `Order` property to run before or after built-in helpers when sharing the same target element.

---

## Q16. What is Tag Helper processing order and why does it matter?

What is Tag Helper processing order and why does it matter?

**Answer:** When multiple Tag Helpers target the same element, they run in ascending `Order` value (lower numbers first). Built-in MVC Tag Helpers use negative orders so they typically execute before custom helpers at default order `0`.

- Custom helpers that modify attributes set by built-ins should set a higher `Order` (e.g., `1000`) to run after `InputTagHelper` finishes.
- Order applies among helpers on the **same element**, not parent-child elements in the DOM.
- Parent wrappers that call `GetChildContentAsync()` still let child element helpers run during child content rendering first.
- Incorrect order can strip or overwrite `name`, `id`, or `data-val-*` attributes generated by built-ins.

---

## Q17. How do you register Tag Helpers from a Razor Class Library?

How do you register Tag Helpers from a Razor Class Library?

**Answer:** Pack Tag Helper classes in a Razor Class Library (RCL), reference the RCL from the MVC app, and add `@addTagHelper *, YourRclAssembly` in `_ViewImports.cshtml` (root and Areas as needed).

- RCLs can ship shared partials, Tag Helpers, and static assets consumed by multiple MVC applications.
- Tag Helpers in RCLs follow the same `[HtmlTargetElement]` and `TagHelper` base class patterns as app-local helpers.
- Views embedded in the RCL resolve relative to the library's virtual path conventions.
- Version the RCL package so consuming apps pick up helper changes consistently across solutions.

---

## Q18. What HTML attributes do Tag Helpers emit for client-side validation (`data-val-*`)?

What HTML attributes do Tag Helpers emit for client-side validation (`data-val-*`)?

**Answer:** Input Tag Helpers emit unobtrusive validation attributes derived from model metadata, including `data-val="true"`, `data-val-required`, `data-val-length`, `data-val-range`, `data-val-regex`, `data-val-equalto`, and corresponding `data-val-*-msg` message attributes.

- `data-val-required` appears when `[Required]` applies; message in `data-val-required` or shared `data-valmsg-for` spans.
- `[StringLength]` maps to `data-val-length-max` / `data-val-length-min`; `[Range]` to `data-val-range-max` / `data-val-range-min`.
- `[Compare]` emits `data-val-equalto-other` referencing the paired property name.
- jQuery Validate Unobtrusive parses these attributes at runtime; hand-written `<input>` without `asp-for` lacks them unless added manually.

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

#### Q1. (R) Review this strongly typed edit view. QA reports that validation messages never appear for `Email`, and the posted form updates the wrong property. The controller and view compile cleanly.

```csharp
// EditCustomerViewModel.cs
public class EditCustomerViewModel
{
    public int Id { get; set; }
    [Required, EmailAddress]
    public string Email { get; set; } = "";
    public string DisplayName { get; set; } = "";
}

// Customer.cs (entity)
public class Customer
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public string FullName { get; set; } = "";
}

// Edit.cshtml — top of file
@model Customer

<form asp-action="Edit" method="post">
    <input type="hidden" asp-for="Id" />
    <div asp-validation-summary="ModelOnly"></div>
    <label asp-for="Email"></label>
    <input asp-for="Email" class="form-control" />
    <span asp-validation-for="Email"></span>
    <label asp-for="FullName"></label>
    <input asp-for="FullName" class="form-control" />
    <button type="submit">Save</button>
</form>
```

Controller action: `public IActionResult Edit(Customer model)` with `[ValidateAntiForgeryToken]`.

---

**Answer:**

_Answer not found._

---

#### Q2. (P) Your team is migrating a .NET Framework MVC 5 app to ASP.NET Core MVC. Hundreds of views still use `@Html.TextBoxFor`, `@Html.LabelFor`, and `@Html.ValidationMessageFor`. Product wants Tag Helpers on new screens only. What is the migration strategy, what breaks if you `@addTagHelper` globally without opt-out, and when would you keep HTML Helpers?

---

**Answer:**

_Answer not found._

---

#### Q3. (M) You ship a custom `HighlightTagHelper` that wraps matching text in `<mark>`. It must run **after** built-in helpers so it does not rewrite attributes that `asp-for` still needs to process. Review this implementation and explain process order.

```csharp
[HtmlTargetElement("highlight")]
public class HighlightTagHelper : TagHelper
{
    public string Term { get; set; } = "";

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        var content = output.GetChildContentAsync().Result.GetContent();
        output.Content.SetHtmlContent(content.Replace(Term, $"<mark>{Term}</mark>"));
    }
}

// View
<highlight term="error">
    <input asp-for="Notes" class="form-control" />
</highlight>
```

`_ViewImports.cshtml` has `@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers` and `@addTagHelper *, MyApp.Web`.

---

**Answer:**

_Answer not found._

---

#### Q4. (R) Review this navigation partial. In staging, several links 404 or hit the wrong controller; one link drops route values on POST redirect. The app uses conventional routing `{controller=Home}/{action=Index}/{id?}` plus one attribute route.

```cshtml
@* _Nav.cshtml *@
<a asp-controller="Orders" asp-action="Details" asp-route-id="@Model.OrderId">Order</a>
<a asp-controller="Admin" asp-action="Index">Admin</a>
<a asp-action="Archive" asp-route-id="@Model.OrderId">Archive</a>
<a href="/reports/monthly">Reports</a>

@* Attribute route on ReportsController: [Route("reports/{year:int}/{month:int}")] *@
<a asp-controller="Reports" asp-action="Monthly" asp-route-year="2025" asp-route-month="8">August</a>
<a asp-controller="Reports" asp-action="Monthly" asp-route-year="2025">August (broken)</a>
```

Current request: `/Orders/Edit/42` — no `Admin` area registered; `Archive` action lives on `OrdersController`.

---

**Answer:**

_Answer not found._

---

#### Q5. (P) Production users report stale JavaScript after every deploy until hard refresh. Review `_Layout.cshtml` and explain cache-busting behavior in Development vs Production, including what breaks if `ASPNETCORE_ENVIRONMENT` is wrong on the server.

```cshtml
<environment include="Development">
    <script src="~/js/site.js"></script>
    <script src="~/lib/jquery/dist/jquery.js"></script>
</environment>
<environment exclude="Development">
    <script src="~/js/site.min.js" asp-append-version="true"></script>
    <script src="~/lib/jquery/dist/jquery.min.js" asp-append-version="true"></script>
</environment>
<link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
```

CDN team asks whether `asp-append-version` works for absolute CDN URLs.

---

**Answer:**

_Answer not found._

---

#### Q6. (R) Review this AJAX create form. The POST succeeds in the full-page version but returns 400 Antiforgery validation failed when submitted via `fetch`. Same view, same controller.

```cshtml
<form asp-action="Create" asp-controller="Tasks" id="createForm">
    <input asp-for="Title" />
    <button type="submit">Create</button>
</form>

<script>
document.getElementById('createForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    const body = new FormData(e.target);
    await fetch('/Tasks/Create', { method: 'POST', body });
});
</script>
```

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Create(TaskInput model) { /* ... */ }
```

---

**Answer:**

_Answer not found._

---

#### Q7. (R) Accessibility audit flags duplicate `id` attributes and broken label associations on this editor partial rendered inside a loop.

```cshtml
@foreach (var line in Model.Lines)
{
    <partial name="_LineEditor" model="line" />
}

@* _LineEditor.cshtml *@
@model OrderLineViewModel
<label for="Quantity">Qty</label>
<input asp-for="Quantity" id="Quantity" class="form-control" />
<span asp-validation-for="Quantity"></span>
```

Browser DevTools shows multiple elements with `id="Quantity"` and labels pointing at the first one only.

---

**Answer:**

_Answer not found._

---

#### Q8. (P) A shared Razor Class Library (`MyCompany.Ui.Rcl`) ships reusable form components with custom Tag Helpers in namespace `MyCompany.Ui.TagHelpers`. Consumer MVC apps add `<ProjectReference>` but Tag Helpers never activate — views render raw `<summary-card>` elements. What registration steps are required in the RCL and consuming app, and what is a common pitfall with `_ViewImports` scope?

---

**Answer:**

_Answer not found._

---

#### Q9. (M) A legacy partial must keep explicit `name` and `id` attributes for a jQuery plugin, but the rest of the app uses Tag Helpers. Review this mixed markup and explain when `!` opt-out is correct vs when it causes silent binding failures.

```cshtml
@model ProductViewModel

<input !type="text"
       !name="legacySku"
       !id="legacySku"
       value="@Model.Sku"
       class="legacy-picker" />

<input asp-for="Sku" class="form-control" />

<label !for="legacySku">Legacy SKU</label>
<label asp-for="Sku"></label>
```

POST action binds `[Bind(Prefix = "legacySku")] string legacySku` and `ProductViewModel` with property `Sku`.

---

**Answer:**

_Answer not found._

---
