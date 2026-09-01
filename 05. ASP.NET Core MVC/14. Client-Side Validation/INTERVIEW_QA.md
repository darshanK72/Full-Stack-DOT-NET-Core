# Client-Side Validation — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is client-side validation in ASP.NET Core MVC?](#q1-what-is-client-side-validation-in-aspnet-core-mvc)
2. [Q2. What is unobtrusive validation?](#q2-what-is-unobtrusive-validation)
3. [Q3. What scripts are required for unobtrusive client validation?](#q3-what-scripts-are-required-for-unobtrusive-client-validation)
4. [Q4. What is `_ValidationScriptsPartial`?](#q4-what-is-_validationscriptspartial)
5. [Q5. What are `data-val-*` attributes and how are they generated?](#q5-what-are-data-val--attributes-and-how-are-they-generated)
6. [Q6. What is the relationship between Data Annotations and client-side validation?](#q6-what-is-the-relationship-between-data-annotations-and-client-side-validation)
7. [Q7. Why is client-side validation not sufficient for security?](#q7-why-is-client-side-validation-not-sufficient-for-security)
8. [Q8. What is `jquery.validate.unobtrusive.js` responsible for?](#q8-what-is-jqueryvalidateunobtrusivejs-responsible-for)
9. [Q9. What is `asp-validation-for` used for?](#q9-what-is-asp-validation-for-used-for)
10. [Q10. What is the difference between `asp-validation-summary="All"` and `"ModelOnly"`?](#q10-what-is-the-difference-between-asp-validation-summaryall-and-modelonly)
11. [Q11. How does `[Remote]` validation work on the client?](#q11-how-does-remote-validation-work-on-the-client)
12. [Q12. What is a client validation adapter for custom `ValidationAttribute`s?](#q12-what-is-a-client-validation-adapter-for-custom-validationattributes)
13. [Q13. What is `ClientValidationEnabled` on `ViewContext`?](#q13-what-is-clientvalidationenabled-on-viewcontext)
14. [Q14. Why does hand-written HTML input lose client-side validation?](#q14-why-does-hand-written-html-input-lose-client-side-validation)
15. [Q15. How do you localize jQuery Validate error messages?](#q15-how-do-you-localize-jquery-validate-error-messages)
16. [Q16. Why must server actions still check `ModelState.IsValid` when client validation is enabled?](#q16-why-must-server-actions-still-check-modelstateisvalid-when-client-validation-is-enabled)
17. [Q17. Why doesn't client validation fire when using a button click handler instead of form submit?](#q17-why-doesnt-client-validation-fire-when-using-a-button-click-handler-instead-of-form-submit)
18. [Q18. What is the difference between MVC unobtrusive validation and SPA/API validation?](#q18-what-is-the-difference-between-mvc-unobtrusive-validation-and-spaapi-validation)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What is client-side validation in ASP.NET Core MVC?

What is client-side validation in ASP.NET Core MVC?

**Answer:** Client-side validation runs validation rules in the browser before or during form submission, providing immediate feedback without a full round-trip. In ASP.NET Core 8 MVC it is built on jQuery Validate plus the unobtrusive adapter layer reading `data-val-*` attributes emitted by tag helpers.

- It mirrors server-side Data Annotations for UX — faster feedback on required fields, ranges, lengths, and patterns.
- It does not replace server validation; attackers can bypass browser scripts entirely.
- Enabled by including validation scripts and using tag helpers (`asp-for`, `asp-validation-for`) on forms.
- Can be toggled per request via `ViewContext.ClientValidationEnabled`.

---

## Q2. What is unobtrusive validation?

What is unobtrusive validation?

**Answer:** Unobtrusive validation separates validation rules from JavaScript code by storing them as HTML `data-val-*` attributes on form fields. `jquery.validate.unobtrusive.js` reads those attributes at page load and configures jQuery Validate without inline script blocks.

- Tag helpers generate attributes from model metadata and Data Annotations at render time.
- Keeps views free of hand-written validation JavaScript for standard annotation rules.
- Custom validation attributes require explicit client adapters to participate in unobtrusive validation.
- After AJAX partial swaps, call `$.validator.unobtrusive.parse()` on the new form container.

---

## Q3. What scripts are required for unobtrusive client validation?

What scripts are required for unobtrusive client validation?

**Answer:** Load jQuery, then `jquery.validate.min.js`, then `jquery.validate.unobtrusive.min.js` in that order. All three are required — loading only jQuery Validate leaves `data-val-*` attributes inert.

- jQuery must load before the validation scripts.
- Scripts typically live in `wwwroot/lib/jquery-validation` and `wwwroot/lib/jquery-validation-unobtrusive`.
- ASP.NET Core 8 project templates reference these via LibMan or npm and bundle them in layout or a partial.
- Missing the unobtrusive bridge is a common cause of "validation attributes present but nothing happens."

---

## Q4. What is `_ValidationScriptsPartial`?

What is `_ValidationScriptsPartial`?

**Answer:** `_ValidationScriptsPartial` is a shared Razor partial that references the jQuery Validate and jQuery Validate Unobtrusive script files. Views include it in the `@section Scripts` block only on pages that need client validation.

- Keeps validation script references in one place instead of duplicating paths across every form view.
- Should be rendered after jQuery and before any page-specific scripts that depend on validation.
- Can be conditionally included when `ViewContext.ClientValidationEnabled` is true.
- Does not enable validation by itself — tag helpers must emit `data-val-*` attributes on form fields.

---

## Q5. What are `data-val-*` attributes and how are they generated?

What are `data-val-*` attributes and how are they generated?

**Answer:** `data-val-*` attributes are HTML metadata on input elements that describe client validation rules. Tag helpers use `IHtmlGenerator` and `ModelMetadata` to emit them from Data Annotations and validator metadata at render time.

- Examples: `data-val="true"`, `data-val-required`, `data-val-required-message`, `data-val-range`, `data-val-regex`.
- Hand-written `<input name="Email">` without tag helpers carries no `data-val-*` — client validation will not run.
- `ViewContext.ClientValidationEnabled = false` suppresses attribute generation even with tag helpers.
- Custom attributes need `IClientModelValidator` adapters to emit corresponding `data-val-*` hooks.

---

## Q6. What is the relationship between Data Annotations and client-side validation?

What is the relationship between Data Annotations and client-side validation?

**Answer:** Data Annotations on ViewModels define server-side validation rules and, when client validation is enabled, drive the `data-val-*` attributes that unobtrusive JavaScript consumes. The annotation is the single declarative source; the client rule is a best-effort mirror.

- Standard annotations (`[Required]`, `[StringLength]`, `[Range]`, `[EmailAddress]`) map automatically to unobtrusive adapters.
- Custom `ValidationAttribute` subclasses require explicit client adapters — annotations alone do not generate client rules.
- Server `ModelState` validation always runs from the same metadata regardless of client script presence.
- Place annotations on ViewModels, not EF entities shared across persistence and UI concerns.

---

## Q7. Why is client-side validation not sufficient for security?

Why is client-side validation not sufficient for security?

**Answer:** Client-side validation executes entirely in the attacker's browser and can be disabled, modified, or bypassed by posting directly with curl, Postman, or custom HTTP clients. Only server-side `ModelState` validation before persistence enforces rules.

- Never skip `ModelState.IsValid` because "the browser validates."
- Remote validation (`[Remote]`) is also bypassable and subject to async race conditions.
- Client validation improves UX for legitimate users; server validation provides the security boundary.
- API endpoints and SPA clients have no unobtrusive layer at all — server checks are the only enforcement.

---

## Q8. What is `jquery.validate.unobtrusive.js` responsible for?

What is `jquery.validate.unobtrusive.js` responsible for?

**Answer:** It bridges MVC-generated `data-val-*` attributes to jQuery Validate by parsing the DOM, creating rule objects, and attaching validators to forms. It also provides adapters for standard and custom validation attributes.

- Runs at page load (or after manual `parse()` calls) to wire rules without inline scripts.
- Maps annotation metadata to jQuery Validate methods (`required`, `range`, `regex`, `remote`).
- Supports adding custom adapters via `$.validator.unobtrusive.adapters.add` for custom business rules.
- Without this file, `data-val-*` attributes are present but no rules are registered.

---

## Q9. What is `asp-validation-for` used for?

What is `asp-validation-for` used for?

**Answer:** `asp-validation-for` is a tag helper that renders a `<span>` element for displaying field-level validation messages for a specific model property. It generates `data-valmsg-for` attributes wired to jQuery Validate message placement.

- Shows both client-side and server-side errors for the named property after POST.
- Typically placed adjacent to the corresponding `asp-for` input in the form.
- CSS classes toggle between `field-validation-valid` and `field-validation-error` based on state.
- Required for visible per-field errors when using `asp-validation-summary="ModelOnly"`.

---

## Q10. What is the difference between `asp-validation-summary="All"` and `"ModelOnly"`?

What is the difference between `asp-validation-summary="All"` and `"ModelOnly"`?

**Answer:** `"All"` renders property-level and model-level errors in the summary list. `"ModelOnly"` renders only model-level errors added with `ModelState.AddModelError(string.Empty, message)` — property errors appear only in adjacent `asp-validation-for` spans.

- Use `"All"` for a single error banner listing all failures at the top of the form.
- Use `"ModelOnly"` when every field has its own `asp-validation-for` span and the summary is for cross-field or business-rule errors only.
- `"None"` suppresses the summary entirely.
- Choosing `"ModelOnly"` with no per-field spans leaves users seeing an empty summary when only property rules fail.

---

## Q11. How does `[Remote]` validation work on the client?

How does `[Remote]` validation work on the client?

**Answer:** `[Remote]` generates a jQuery Validate `remote` rule that sends an AJAX GET or POST to a specified action to check field validity asynchronously — for example, username availability. The server action returns `true` (valid) or `false` (invalid) as JSON.

- Provides immediate UX feedback without a full form POST.
- The remote call is asynchronous — the form may submit before the remote response completes.
- Always re-validate the same rule in the POST action or service; treat `[Remote]` as a hint only.
- Disable submit or show pending state while remote validation is in flight to reduce race conditions.

---

## Q12. What is a client validation adapter for custom `ValidationAttribute`s?

What is a client validation adapter for custom `ValidationAttribute`s?

**Answer:** A client validation adapter implements `IClientModelValidator` and translates a custom server-side `ValidationAttribute` into `data-val-*` attributes plus optional jQuery Validate custom method registration. Without it, custom attributes validate server-side only.

- Implement `AddValidation(ClientModelValidationContext context)` to emit attributes like `data-val-mustbefuturedate`.
- Register the adapter via `ClientModelValidatorProvider` in `AddControllersWithViews` options.
- Add a matching jQuery Validate custom method or unobtrusive adapter that reads the emitted attributes.
- The server `ValidationAttribute` remains the authoritative rule; the client adapter mirrors it for UX.

---

## Q13. What is `ClientValidationEnabled` on `ViewContext`?

What is `ClientValidationEnabled` on `ViewContext`?

**Answer:** `ClientValidationEnabled` is a boolean flag on `ViewContext` that controls whether tag helpers emit `data-val-*` attributes during view rendering. When false, inputs render without client validation metadata.

- Set it in `_ViewImports.cshtml` or individual views — commonly tied to environment (enabled in Development, optionally disabled in Production).
- Disabling it does not affect server-side `ModelState` validation on POST.
- When false, including `_ValidationScriptsPartial` wastes bandwidth but causes no harm.
- Useful for reducing script payload or avoiding stale client rules during server-side debugging.

---

## Q14. Why does hand-written HTML input lose client-side validation?

Why does hand-written HTML input lose client-side validation?

**Answer:** Plain HTML inputs without tag helpers do not receive `data-val-*` attributes because unobtrusive validation is metadata-driven at render time via `IHtmlGenerator`. Without those attributes, jQuery unobtrusive has no rules to attach.

- `<input name="Quantity" type="number" />` binds on POST but validates only server-side if the action checks `ModelState`.
- Use `asp-for="Quantity"` instead of raw HTML, or manually add every required `data-val-*` attribute (error-prone).
- `asp-validation-for` spans also require tag helpers to generate correct `data-valmsg-for` wiring.
- Replacing tag helpers with custom markup is a frequent cause of "server validates but client does not."

---

## Q15. How do you localize jQuery Validate error messages?

How do you localize jQuery Validate error messages?

**Answer:** Server-side localization via `.resx` files or `IValidationMetadataProvider` affects `ModelState` messages and optionally `data-val-*-message` on rendered fields. jQuery Validate's built-in method messages (email, number, date) remain English unless you load a localized messages file or override `$.validator.messages`.

- Inspect rendered HTML — if `data-val-required-message` is French, unobtrusive uses it for annotation-backed rules.
- Load `messages_fr.min.js` after `jquery.validate.min.js` and before unobtrusive parses the form.
- Or call `$.extend($.validator.messages, { required: "...", email: "..." })` in a script block.
- Align `RequestLocalization` culture with the validation messages script loaded for the current culture.

---

## Q16. Why must server actions still check `ModelState.IsValid` when client validation is enabled?

Why must server actions still check `ModelState.IsValid` when client validation is enabled?

**Answer:** Client validation is optional UX that runs only when browsers execute your scripts correctly. Any HTTP client can POST invalid or malicious payloads directly to the action, bypassing all client rules.

- `ModelState.IsValid` is the enforcement gate before persistence, redirects, or side effects.
- Client and server validation should share the same ViewModel annotations but serve different roles.
- Failing to check `ModelState` while client validation is enabled creates a false sense of security.
- Return `View(model)` or appropriate error responses when server validation fails.

---

## Q17. Why doesn't client validation fire when using a button click handler instead of form submit?

Why doesn't client validation fire when using a button click handler instead of form submit?

**Answer:** jQuery unobtrusive validation intercepts the form `submit` event. A `type="button"` click handler that calls `fetch` or `$.post` directly bypasses the validator unless it explicitly calls `$form.valid()` and aborts when validation fails.

- Guard custom AJAX: `if (!$('#form').valid()) return;` before sending the request.
- Or use `type="submit"` and prevent default in a submit handler: `e.preventDefault(); if ($(this).valid()) { ... }`.
- This pattern is common in AJAX partial-form saves where developers avoid full page POST.
- Server-side validation in the action remains mandatory regardless of client guard.

---

## Q18. What is the difference between MVC unobtrusive validation and SPA/API validation?

What is the difference between MVC unobtrusive validation and SPA/API validation?

**Answer:** MVC unobtrusive validation applies to Razor-rendered HTML forms with jQuery Validate scripts. SPA/API validation runs server-side via `[ApiController]` automatic 400 responses, FluentValidation, or explicit `ModelState` checks — the browser SPA must implement its own client rules separately.

- Shared ViewModel annotations enforce server rules on both paths if each pipeline validates, but unobtrusive scripts never run for JSON API consumers.
- `[ApiController]` returns `ValidationProblemDetails` (400) when model binding/validation fails.
- SPAs typically use libraries like Zod or React Hook Form for UX — duplicated rules, not a substitute for server checks.
- Do not assume annotations on a shared DTO automatically protect every host entry point.

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

#### Q1. (R) Review this registration action and Razor view. QA says the form "validates fine" in the browser, but attackers can create accounts with empty passwords.

```csharp
// AccountController.cs
[HttpPost]
public IActionResult Register(RegisterViewModel model)
{
    // Client-side validation handles UX — skip server round-trip when invalid
    return RedirectToAction("Index", "Home");
}

public class RegisterViewModel
{
    [Required][EmailAddress] public string Email { get; set; } = "";
    [Required][MinLength(8)] public string Password { get; set; } = "";
}
```

```html
<!-- Views/Account/Register.cshtml -->
<form asp-action="Register" method="post">
    <input asp-for="Email" />
    <input asp-for="Password" type="password" />
    <button type="submit">Register</button>
</form>
@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

`_Layout.cshtml` includes jQuery and `_ValidationScriptsPartial` scripts. Model has data annotations.

---

**Answer:**

**Answer:** Client-side validation is UX only — the POST action never checks `ModelState.IsValid` and always redirects, so any HTTP client can bypass browser scripts and submit invalid or malicious payloads directly to the server.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | No server-side validation gate | Empty password / invalid email persisted or logic runs on bad data |
| Correctness | Unconditional redirect on POST | Invalid submissions appear "successful" |
| Defense in depth | Client scripts treated as enforcement | Trivial curl/Postman bypass |

**Fix (priority order):**

1. Add server validation: `if (!ModelState.IsValid) return View(model);` before any persistence or redirect.
2. Keep `_ValidationScriptsPartial` for UX — it does not replace server checks.
3. Return appropriate status for API-style endpoints; for MVC, re-render the view with `ModelState` errors.

**Production takeaway:** Karat tests the classic security hole — jQuery unobtrusive validation must mirror server rules, never replace them. See `07. Data Annotations & Validation` for annotation parity.

---

---

#### Q2. (R) Review this layout and create form. Fields show `data-val-*` attributes in HTML, but nothing happens on blur or submit — no inline errors.

```html
<!-- Views/Shared/_Layout.cshtml -->
<script src="~/lib/jquery/dist/jquery.min.js"></script>
@RenderSection("Scripts", required: false)

<!-- Views/Products/Create.cshtml -->
<form asp-action="Create" method="post">
    <div asp-validation-summary="ModelOnly"></div>
    <input asp-for="Sku" />
    <span asp-validation-for="Sku"></span>
    <button type="submit">Save</button>
</form>
@section Scripts {
    <script src="~/lib/jquery-validation/dist/jquery.validate.min.js"></script>
    <!-- jquery.validate.unobtrusive.min.js intentionally omitted to reduce bundle size -->
}
```

`Program.cs` uses standard MVC with Razor views; model has `[Required]` on `Sku`.

---

**Answer:**

_Answer not found._

---

#### Q3. (P) A business rule attribute `[MustBeFutureDate]` validates that `ShipDate` is after today. Server-side validation works via `IValidatableObject`, but the browser never blocks past dates before POST. What is the MVC client-validation extension point, and what must you register?

```csharp
public class MustBeFutureDateAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
        => value is DateTime d && d.Date <= DateTime.Today
            ? new ValidationResult("Ship date must be in the future.")
            : ValidationResult.Success;
}

public class OrderViewModel
{
    [MustBeFutureDate]
    public DateTime ShipDate { get; set; }
}
```

View uses `<input asp-for="ShipDate" />` and `_ValidationScriptsPartial`.

---

**Answer:**

_Answer not found._

---

#### Q4. (R) Review username availability check. Users report duplicate accounts when they tab quickly through the field and submit.

```csharp
public class RegisterViewModel
{
    [Required]
    [Remote(action: "CheckUsername", controller: "Account")]
    public string Username { get; set; } = "";
}

// AccountController
public IActionResult CheckUsername(string username)
    => Json(_users.IsAvailable(username) ? true : $"Username '{username}' is taken.");
```

```html
<form asp-action="Register" method="post" id="register-form">
    <input asp-for="Username" />
    <span asp-validation-for="Username"></span>
    <button type="submit">Register</button>
</form>
```

`_ValidationScriptsPartial` is present. No debounce or submit guard in the view.

---

**Answer:**

_Answer not found._

---

#### Q5. (R) Review this edit form. Server returns validation errors after POST, but users see a blank page above the fields — property errors never appear in the summary area they expect.

```html
<form asp-action="Edit" method="post">
    <div asp-validation-summary="ModelOnly" class="text-danger"></div>
    <input asp-for="ProductName" />
    <span asp-validation-for="ProductName" class="text-danger"></span>
    <input asp-for="UnitPrice" />
    <span asp-validation-for="UnitPrice" class="text-danger"></span>
    <button type="submit">Save</button>
</form>
```

Controller:

```csharp
[HttpPost]
public IActionResult Edit(ProductViewModel model)
{
    if (!ModelState.IsValid)
        return View(model);
    _repo.Update(model);
    return RedirectToAction(nameof(Index));
}
```

Model: `[Required]` on `ProductName`, `[Range(0.01, 9999)]` on `UnitPrice`. Client scripts loaded.

---

**Answer:**

_Answer not found._

---

#### Q6. (P) Ops wants client-side validation **disabled in Production** (rely on server validation + fewer script failures) but **enabled in Development** for faster feedback. Review the proposed `_ViewImports.cshtml` approach:

```csharp
@inject Microsoft.AspNetCore.Mvc.ViewFeatures.IHtmlHelper Html
@{
    Html.ViewContext.ClientValidationEnabled =
        Html.ViewContext.HttpContext.RequestServices
            .GetRequiredService<IWebHostEnvironment>()
            .IsDevelopment();
}
```

Layout still renders `_ValidationScriptsPartial` in all environments.

---

**Answer:**

_Answer not found._

---

#### Q7. (M) A developer replaces tag helpers with hand-written HTML to match a design system. Required and range rules still work on the server after POST, but client validation silently disappears. Explain what `IHtmlHelper` / tag helpers emit for unobtrusive validation and what breaks when you drop them.

```html
<!-- Before (worked client-side) -->
<input asp-for="Quantity" />

<!-- After (server-only) -->
<input type="number" name="Quantity" id="Quantity" class="form-control" />
```

Model:

```csharp
public class LineItemViewModel
{
    [Required]
    [Range(1, 100)]
    public int Quantity { get; set; }
}
```

---

**Answer:**

_Answer not found._

---

#### Q8. (P) A French-localized MVC app shows English jQuery Validate messages ("This field is required") while server-rendered `ModelState` errors are correctly in French. `_ViewImports` sets `@using System.ComponentModel.DataAnnotations` and resource files back `[Required]` display messages. What still needs wiring for client-side messages?

```html
<form asp-action="Create" method="post">
    <input asp-for="Nom" />
    <span asp-validation-for="Nom"></span>
</form>
@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

`Program.cs`: `RequestLocalization` configured with `fr-FR` as default culture.

---

**Answer:**

_Answer not found._

---

#### Q9. (D) A team ships a React SPA that POSTs JSON to `/api/orders` while keeping legacy MVC Razor pages for admin. Product asks: "We already have data annotations on `OrderViewModel` — does that protect the API?" Compare where MVC unobtrusive validation applies vs what the SPA path requires.

```csharp
// MVC admin — Views/Orders/Create.cshtml with tag helpers + _ValidationScriptsPartial
public class OrderViewModel
{
    [Required][StringLength(50)] public string CustomerName { get; set; } = "";
    [Range(1, 1000)] public int Quantity { get; set; }
}

// API — separate controller, same property rules desired
[ApiController]
[Route("api/orders")]
public class OrdersApiController : ControllerBase
{
    [HttpPost]
    public IActionResult Create([FromBody] OrderViewModel model) { /* ... */ }
}
```

---

**Answer:**

_Answer not found._

---

#### Q10. (R) Review this AJAX partial-form save from Chapter 13 patterns. Inline field validation works until the user clicks Save — the request fires even when `[Required]` fields are empty.

```html
<form id="profile-form" asp-action="SaveProfile" asp-controller="Account">
    <input asp-for="DisplayName" />
    <span asp-validation-for="DisplayName"></span>
    <button type="button" id="save-profile">Save</button>
</form>
@section Scripts {
    <partial name="_ValidationScriptsPartial" />
    <script>
        $('#save-profile').on('click', function () {
            $.post($('#profile-form').attr('action'), $('#profile-form').serialize());
        });
    </script>
}
```

`DisplayName` has `[Required]`. Standard unobtrusive scripts are loaded.

**Answer:**

_Answer not found._

---
