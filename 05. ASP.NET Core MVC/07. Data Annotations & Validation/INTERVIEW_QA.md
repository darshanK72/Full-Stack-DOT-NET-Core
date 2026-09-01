# Data Annotations & Validation — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What are Data Annotations in ASP.NET Core MVC?](#q1-what-are-data-annotations-in-aspnet-core-mvc)
2. [Q2. What is `ModelState` and how does it relate to validation?](#q2-what-is-modelstate-and-how-does-it-relate-to-validation)
3. [Q3. Why must server-side validation always be performed even with client validation?](#q3-why-must-server-side-validation-always-be-performed-even-with-client-validation)
4. [Q4. What does `ModelState.IsValid` check?](#q4-what-does-modelstateisvalid-check)
5. [Q5. What is the difference between `[Required]` and `[AllowNull]`?](#q5-what-is-the-difference-between-required-and-allownull)
6. [Q6. Why does `[Required]` not work as expected on a `bool` checkbox?](#q6-why-does-required-not-work-as-expected-on-a-bool-checkbox)
7. [Q7. What is `[Compare]` used for?](#q7-what-is-compare-used-for)
8. [Q8. What are `[Range]` and `[StringLength]` used for?](#q8-what-are-range-and-stringlength-used-for)
9. [Q9. What is `[RegularExpression]` used for?](#q9-what-is-regularexpression-used-for)
10. [Q10. What is `IValidatableObject` and when do you use it?](#q10-what-is-ivalidatableobject-and-when-do-you-use-it)
11. [Q11. What is the difference between `IValidatableObject` and a custom `ValidationAttribute`?](#q11-what-is-the-difference-between-ivalidatableobject-and-a-custom-validationattribute)
12. [Q12. What is `[Remote]` validation and how does it work?](#q12-what-is-remote-validation-and-how-does-it-work)
13. [Q13. What is `[ValidateNever]` and when is it applied?](#q13-what-is-validatenever-and-when-is-it-applied)
14. [Q14. Why should validation attributes not be placed on EF entities shared with MVC?](#q14-why-should-validation-attributes-not-be-placed-on-ef-entities-shared-with-mvc)
15. [Q15. What are `asp-validation-for` and `asp-validation-summary`?](#q15-what-are-asp-validation-for-and-asp-validation-summary)
16. [Q16. What is the difference between `ValidationSummary` `ModelOnly` and `All`?](#q16-what-is-the-difference-between-validationsummary-modelonly-and-all)
17. [Q17. What is unobtrusive client validation?](#q17-what-is-unobtrusive-client-validation)
18. [Q18. What is the difference between Data Annotations and FluentValidation?](#q18-what-is-the-difference-between-data-annotations-and-fluentvalidation)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What are Data Annotations in ASP.NET Core MVC?

What are Data Annotations in ASP.NET Core MVC?

**Answer:** Data Annotations are declarative attributes (from `System.ComponentModel.DataAnnotations`) placed on ViewModel or DTO properties to express validation rules, display metadata, and binding hints. MVC reads them during model binding and validation to populate `ModelState` and to emit client-side validation attributes in Razor views.

- Common validation attributes include `[Required]`, `[StringLength]`, `[Range]`, `[EmailAddress]`, `[Compare]`, and `[RegularExpression]`.
- Display attributes such as `[Display(Name = "...")]` and `[DataType(DataType.Password)]` shape labels and HTML input types in Tag Helpers.
- Annotations on the posted model are evaluated server-side by the validation pipeline when the action runs — they do not replace an explicit `ModelState.IsValid` check in MVC controllers.
- Place annotations on ViewModels rather than EF entities when the same entity is shared across persistence and UI layers.

---

## Q2. What is `ModelState` and how does it relate to validation?

What is `ModelState` and how does it relate to validation?

**Answer:** `ModelState` is a dictionary on the controller (via `ControllerBase.ModelState`) that records binding errors and validation failures for each model property. After model binding, the validation system adds entries keyed by property name; the action uses it to decide whether the submitted data is acceptable.

- Each key (e.g., `"Email"`) holds a `ModelStateEntry` with attempted value, validation errors, and binding exceptions.
- Validation attributes, `IValidatableObject`, and custom `ValidationAttribute` subclasses all write into `ModelState` through the same pipeline.
- Tag Helpers such as `asp-validation-for` read `ModelState` to render field-level error messages in the view.
- `ModelState` is scoped to the current HTTP request and is not preserved across `RedirectToAction` unless you explicitly rehydrate it.

---

## Q3. Why must server-side validation always be performed even with client validation?

Why must server-side validation always be performed even with client validation?

**Answer:** Client-side validation runs only in the browser and can be disabled, bypassed, or forged with tools like curl or Postman. Server-side validation is the authoritative gate before persisting data or performing security-sensitive operations.

- Attackers can POST directly to the action without JavaScript or with modified `data-val-*` attributes removed from the HTML.
- Client validation improves UX with immediate feedback but provides zero security guarantees on its own.
- MVC controllers do not automatically return 400 on invalid models unless you check `ModelState.IsValid` (unlike `[ApiController]` automatic validation).
- Production code must validate on the server even when unobtrusive jQuery validation is enabled in the layout.

---

## Q4. What does `ModelState.IsValid` check?

What does `ModelState.IsValid` check?

**Answer:** `ModelState.IsValid` returns `true` only when every entry in `ModelState` has no validation or binding errors. It is the standard guard in POST actions before calling services or saving to the database.

- Binding failures (type conversion, missing required value types) mark entries invalid before annotation validation runs.
- Cross-property rules from `IValidatableObject.Validate` also contribute errors that make `IsValid` false.
- Checking `IsValid` alone does not run validation — validation runs during model binding; `IsValid` only reports the outcome.
- On failure, return `View(model)` (or `PartialView`) with the same model so field names and error keys align with the form.

---

## Q5. What is the difference between `[Required]` and `[AllowNull]`?

What is the difference between `[Required]` and `[AllowNull]`?

**Answer:** `[Required]` means the value must be present and non-empty for reference types and non-nullable value types; for nullable value types it means the value must not be `null`. `[AllowNull]` (from `System.Diagnostics.CodeAnalysis`) is a nullable reference type annotation telling the compiler the property may be assigned `null` — it does not enforce runtime validation.

- `[Required]` on `string` fails for `null` or `""` (and whitespace-only by default with data annotations).
- `[Required]` on `int?` fails when the field is omitted and binds as `null`; on non-nullable `int`, a missing field may bind as `0`, which satisfies `[Required]`.
- `[AllowNull]` affects static analysis (NRT) and documentation, not MVC validation behavior.
- Use `[Required(AllowEmptyStrings = true)]` when an empty string is acceptable but `null` is not.

---

## Q6. Why does `[Required]` not work as expected on a `bool` checkbox?

Why does `[Required]` not work as expected on a `bool` checkbox?

**Answer:** A non-nullable `bool` checkbox that is unchecked does not post a value, so model binding sets the property to `false` — a valid non-null value. `[Required]` checks for presence, not for `true`, so an unchecked required consent checkbox never fails validation.

- HTML checkboxes only submit `"on"` or `"true"` when checked; absent fields bind as `false` for `bool`.
- `[Required]` on `bool?` fails only when the value is `null`, not when it is `false`.
- For "must accept terms" scenarios, validate explicitly that the value is `true` via `[Range(typeof(bool), "true", "true")]`, a custom attribute, or `IValidatableObject`.
- The hidden-field checkbox pattern (`<input type="hidden" value="false" />` plus checkbox) still posts `false` when unchecked, not `null`.

---

## Q7. What is `[Compare]` used for?

What is `[Compare]` used for?

**Answer:** `[Compare("OtherProperty")]` validates that the decorated property's value equals another property on the same model instance — commonly used for `ConfirmPassword` matching `Password`.

- Comparison runs server-side during validation after both properties are bound.
- The client-side unobtrusive adapter emits `data-val-equalto-other` so browsers can show a match error before submit.
- Property names in `[Compare]` must match exactly; typos silently weaken validation.
- `[Compare]` compares string representation; for complex types use custom validation instead.

---

## Q8. What are `[Range]` and `[StringLength]` used for?

What are `[Range]` and `[StringLength]` used for?

**Answer:** `[StringLength(maximumLength)]` constrains the length of a string property (with optional minimum). `[Range(min, max)]` constrains numeric or date values to an inclusive bounds window defined by constants or type-specific overloads.

- `[StringLength(100, MinimumLength = 3)]` validates character count, not byte size — important for Unicode text.
- `[Range(1, 100)]` on `int` rejects values outside the interval; use `[Range(typeof(decimal), "0.01", "9999.99")]` for decimal bounds.
- Both attributes generate corresponding `data-val-length` and `data-val-range` attributes for unobtrusive client validation.
- Error messages can be customized via `ErrorMessage` or resource-based `ErrorMessageResourceName`.

---

## Q9. What is `[RegularExpression]` used for?

What is `[RegularExpression]` used for?

**Answer:** `[RegularExpression("pattern")]` validates that a string property matches a .NET regular expression, useful for phone numbers, postal codes, or usernames with specific formats.

- The pattern uses .NET regex syntax, which may differ slightly from JavaScript regex used on the client.
- Unobtrusive validation maps the pattern to `data-val-regex-pattern` for jQuery Validate.
- Overly complex patterns can be hard to maintain; consider `IValidatableObject` or FluentValidation for multi-field format rules.
- Always re-validate on the server — client regex is advisory only.

---

## Q10. What is `IValidatableObject` and when do you use it?

What is `IValidatableObject` and when do you use it?

**Answer:** `IValidatableObject` is an interface with a `Validate(ValidationContext)` method that returns `IEnumerable<ValidationResult>` for cross-property or conditional rules that single attributes cannot express.

- Implement it on the ViewModel when validation depends on multiple fields (e.g., end date after start date).
- Results can target a specific member name or be model-level with an empty member name for summary display.
- It runs as part of the same MVC validation pass after property-level attributes are evaluated.
- Use it when logic is simple and colocated with the model; move elaborate rule sets to FluentValidation for maintainability.

---

## Q11. What is the difference between `IValidatableObject` and a custom `ValidationAttribute`?

What is the difference between `IValidatableObject` and a custom `ValidationAttribute`?

**Answer:** A custom `ValidationAttribute` decorates individual properties (or the class with `ValidationAttribute` usage on the type) and is reusable across models. `IValidatableObject` centralizes all object-level rules in one `Validate` method on that type.

- Attributes compose declaratively on properties; custom attributes can be reused with different parameters.
- `IValidatableObject` fits cross-field rules without creating many one-off attributes.
- Custom attributes can provide client-side adapters; `IValidatableObject` is server-only unless you add a separate client adapter.
- Both integrate with `ModelState`; choose based on reusability and whether the rule is property-scoped or object-scoped.

---

## Q12. What is `[Remote]` validation and how does it work?

What is `[Remote]` validation and how does it work?

**Answer:** `[Remote(action, controller, area)]` triggers an AJAX call during client-side validation to ask the server whether a value is acceptable — typical for checking username or email availability before form submit.

- The attribute generates `data-val-remote-url` pointing at a controller action that returns `true`/`false` or JSON indicating validity.
- The remote action must be idempotent, fast, and secured — it is callable without submitting the full form.
- Server-side validation must still enforce the same rule on POST; `[Remote]` is not invoked automatically during server validation.
- Include antiforgery or rate limiting on remote endpoints to reduce abuse in production.

---

## Q13. What is `[ValidateNever]` and when is it applied?

What is `[ValidateNever]` and when is it applied?

**Answer:** `[ValidateNever]` (from `Microsoft.AspNetCore.Mvc.ModelBinding.Validation`) excludes a property from validation even if it has data annotation attributes or would otherwise be validated. It is commonly applied to properties populated server-side that should not be validated from user input.

- Use on navigation properties, audit fields, or server-assigned IDs on ViewModels bound from forms.
- It prevents spurious validation errors on properties the user did not post.
- Pair with `[BindNever]` when the property should neither bind nor validate from the request.
- Do not use it to skip validation on user-editable fields — that creates security holes.

---

## Q14. Why should validation attributes not be placed on EF entities shared with MVC?

Why should validation attributes not be placed on EF entities shared with MVC?

**Answer:** EF Core entities represent the persistence model; UI validation rules, display names, and `[Required]` semantics often differ from database constraints. Sharing annotations couples the database schema to presentation concerns and causes incorrect validation on API or batch import paths.

- A column may be required in the database but optional on a create form step; a ViewModel expresses that per screen.
- Navigation properties on entities can trigger unintended validation of related graphs during MVC binding.
- API DTOs and MVC ViewModels need different rules than the entity; annotations on the entity force one-size-fits-all behavior.
- Keep EF configuration in Fluent API or data annotations limited to persistence; put MVC validation on ViewModels.

---

## Q15. What are `asp-validation-for` and `asp-validation-summary`?

What are `asp-validation-for` and `asp-validation-summary`?

**Answer:** These are Tag Helpers that render validation UI from `ModelState`. `asp-validation-for="PropertyName"` outputs a `<span>` with the field error message; `asp-validation-summary` renders a summary block of all errors or model-level errors only.

- `asp-validation-for` targets a single property and pairs with inputs generated by `asp-for` on the same property name.
- `asp-validation-summary` accepts `ValidationSummary.All` or `ValidationSummary.ModelOnly` to control which errors appear.
- They emit no content when `ModelState` has no errors for the requested scope.
- Require `_ValidationScriptsPartial` (jQuery Validate + unobtrusive) in the layout for client-side messages to appear before POST.

---

## Q16. What is the difference between `ValidationSummary` `ModelOnly` and `All`?

What is the difference between `ValidationSummary` `ModelOnly` and `All`?

**Answer:** `ValidationSummary.All` lists every error in `ModelState`, including property-level errors that may also appear next to fields. `ValidationSummary.ModelOnly` shows only errors not tied to a specific property (empty member name) or excludes property-level keys depending on configuration — typically used for a top-of-form alert without duplicating field messages.

- Use `ModelOnly` when each field already has `asp-validation-for` and the summary should show cross-cutting errors only.
- Use `All` when you want a single consolidated error list, often on small forms without per-field spans.
- Duplicate display occurs if you use `All` alongside `asp-validation-for` for the same properties.
- Model-level errors from `ModelState.AddModelError(string.Empty, message)` appear in `ModelOnly` summaries.

---

## Q17. What is unobtrusive client validation?

What is unobtrusive client validation?

**Answer:** Unobtrusive client validation is ASP.NET Core MVC's approach where Tag Helpers emit `data-val-*` HTML attributes from server-side metadata, and jQuery Validate Unobtrusive reads those attributes without embedding inline JavaScript in the view.

- Enabled by referencing `jquery.validate.js` and `jquery.validate.unobtrusive.js` after jQuery.
- Validation rules mirror data annotations where adapters exist; the browser blocks submit and shows messages before a round trip.
- It respects `ClientValidationEnabled` on the view context and can be disabled per request if needed.
- Server-side validation must always duplicate these rules for security regardless of unobtrusive setup.

---

## Q18. What is the difference between Data Annotations and FluentValidation?

What is the difference between Data Annotations and FluentValidation?

**Answer:** Data Annotations are attributes on properties evaluated by the built-in validation infrastructure. FluentValidation defines rule classes (`AbstractValidator<T>`) with a fluent API, registered in DI and invoked by the FluentValidation.AspNetCore integration package.

- Annotations are lightweight and visible on the ViewModel; FluentValidation centralizes complex rules in separate validator classes.
- FluentValidation excels at conditional chains, async rules, and large rule sets without polluting the model type.
- Both populate `ModelState` when integrated; FluentValidation requires explicit registration (`AddFluentValidation`, `AddValidatorsFromAssembly`).
- Annotations automatically drive unobtrusive client adapters; FluentValidation needs additional setup for client-side parity.

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

#### Q1. (R) Review this MVC registration flow. QA reports that disabling JavaScript in the browser still allows invalid accounts through in production.

```csharp
// RegisterViewModel.cs
public class RegisterViewModel
{
    [Required][EmailAddress] public string Email { get; set; } = "";
    [Required][MinLength(8)] public string Password { get; set; } = "";
}

// Views/Account/Register.cshtml (excerpt)
<form asp-action="Register" method="post">
    <input asp-for="Email" />
    <span asp-validation-for="Email"></span>
    <input asp-for="Password" type="password" />
    <button type="submit">Register</button>
</form>
@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}

// AccountController.cs
[HttpPost]
public IActionResult Register(RegisterViewModel model)
{
    _users.Create(model.Email, model.Password);
    return RedirectToAction("Index", "Home");
}
```

`Program.cs` calls `AddControllersWithViews()`; `_ValidationScriptsPartial` is present on the GET view.

---

**Answer:**

_Answer not found._

---

#### Q2. (R) Review this checkout ViewModel. Users can submit the form without accepting terms even though the UI shows a required checkbox.

```csharp
public class CheckoutViewModel
{
    [Required] public bool? AcceptTerms { get; set; }
    public string ShippingAddress { get; set; } = "";
}

// Razor excerpt
<input asp-for="AcceptTerms" type="checkbox" />
<label asp-for="AcceptTerms">I accept the terms</label>
```

POST body when checkbox is unchecked: `AcceptTerms=false` (or field omitted). Developer expected `[Required]` to block submission.

---

**Answer:**

_Answer not found._

---

#### Q3. (R) Review password change form. Server accepts mismatched passwords; client-side shows an error only in Chrome.

```csharp
public class ChangePasswordViewModel
{
    [Required][DataType(DataType.Password)]
    public string NewPassword { get; set; } = "";

    [Required][Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = "";
}

[HttpPost]
public IActionResult ChangePassword(ChangePasswordViewModel model)
{
    if (!ModelState.IsValid) return View(model);
    _users.ChangePassword(UserId, model.NewPassword);
    return RedirectToAction("Profile");
}
```

Property names were refactored from `Password` → `NewPassword`; `_ValidationScriptsPartial` is loaded. Pen testers POST `{ "NewPassword": "x", "ConfirmPassword": "y" }` directly.

---

**Answer:**

_Answer not found._

---

#### Q4. (P) A booking ViewModel needs `EndDate >= StartDate` and at least one guest name when `GuestCount > 0`. Compare putting this in `IValidatableObject.Validate` vs a custom `ValidationAttribute` on one property — what runs when in the MVC pipeline, and how do errors map to `asp-validation-for`?

```csharp
public class BookingViewModel : IValidatableObject
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int GuestCount { get; set; }
    public string? PrimaryGuestName { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext ctx) { /* ... */ }
}
```

---

**Answer:**

_Answer not found._

---

#### Q5. (R) Review this custom attribute on a discount field. Validation passes for `-50` and `999999` in unit tests but fails unpredictably in the MVC form POST.

```csharp
public sealed class DiscountPercentAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
    {
        if (value is null) return ValidationResult.Success;
        var n = (int)value;
        if (n < 0 || n > 100) return new ValidationResult(ErrorMessage ?? "Invalid discount.");
        return ValidationResult.Success;
    }
}

public class ProductEditViewModel
{
    [DiscountPercent]
    public decimal DiscountPercent { get; set; }
}
```

Form POST sends `DiscountPercent=12.5` from a `<input type="number" step="0.01">`.

---

**Answer:**

_Answer not found._

---

#### Q6. (D) A team annotates EF Core entities with `[Required]`, `[StringLength]`, and `[Range]` and passes the same entity type to MVC Razor views and to a minimal API layer. Review the architecture — what breaks, leaks, or becomes hard to test?

```csharp
public class Product  // EF entity + MVC model
{
    public int Id { get; set; }
    [Required][StringLength(100)] public string Name { get; set; } = "";
    [Range(0, 99999)] public decimal Price { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
```

---

**Answer:**

_Answer not found._

---

#### Q7. (R) Review client vs server validation mismatch. Support tickets say "the form showed valid but server rejected with a date error."

```csharp
// ViewModel — server
public class EventViewModel
{
    [Required]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
    public DateTime EventDate { get; set; }
}

// wwwroot/js/site.js (legacy override)
$.validator.methods.date = function () { return true; }; // accept any string
```

Browser locale sends `31/12/2026`; server culture is `en-US`. `_ValidationScriptsPartial` is included.

---

**Answer:**

_Answer not found._

---

#### Q8. (R) Review `[Remote]` username availability check. Production DB CPU spikes after marketing campaign; security flags possible enumeration.

```csharp
public class SignUpViewModel
{
    [Required]
    [Remote(action: "CheckUsername", controller: "Account")]
    public string Username { get; set; } = "";
}

// AccountController
[AcceptVerbs("GET", "POST")]
public IActionResult CheckUsername(string username)
{
    var taken = _db.Users.Any(u => u.Username == username);
    return Json(!taken);
}
```

Razor: `<input asp-for="Username" />` with unobtrusive validation scripts. No throttling on `CheckUsername`.

---

**Answer:**

_Answer not found._

---

#### Q9. (P) An admin edit form binds a user ViewModel that includes a password hash for round-trip hidden fields. Model validation fails on POST with "The PasswordHash field is required." Explain `ValidateNever` — where to apply it, what still must be validated server-side, and what not to put in the form at all.

```csharp
public class EditUserViewModel
{
    public int Id { get; set; }
    [Required][EmailAddress] public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = ""; // populated on GET, round-tripped in hidden input
    [Required][MinLength(8)] public string? NewPassword { get; set; }
}
```

---

**Answer:**

_Answer not found._

---

#### Q10. (D) Greenfield MVC app: team debates Data Annotations + unobtrusive client validation vs FluentValidation (`AddFluentValidationAutoValidation`). Compare maintainability, client-side parity, testing, and what happens on a direct POST with JavaScript disabled.

---

**Answer:**

_Answer not found._

---

#### Q11. (M) Walk through the server-side validation order for an MVC POST to `[HttpPost] Create(ProductCreateViewModel model)` when the action has `[ValidateAntiForgeryToken]`, the ViewModel implements `IValidatableObject`, and one property has a custom `ValidationAttribute`. When does `ModelState.IsValid` become false, and when should the action short-circuit?

---

**Answer:**

_Answer not found._

---

#### Q12. (R) Review this API-style POST added beside MVC views. Invalid models return 200 HTML instead of validation errors; `[ApiController]` is not used on this controller.

```csharp
public class OrdersController : Controller
{
    [HttpPost]
    public IActionResult QuickAdd([FromBody] QuickAddOrderViewModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _orders.Add(model);
        return Ok(new { id = model.Sku });
    }
}

public class QuickAddOrderViewModel
{
    [Required] public string Sku { get; set; } = "";
    [Range(1, 100)] public int Quantity { get; set; }
}
```

AJAX caller sends `{ "sku": "", "quantity": 0 }` with `Content-Type: application/json`. Same controller serves Razor views for `/Orders`.

**Answer:**

_Answer not found._

---
