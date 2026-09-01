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

**Concepts**
- jQuery Validate + unobtrusive adapter layer
- `data-val-*` attributes as metadata on input elements
- Data Annotations as single source for both server and client rules
- Client validation as UX feedback, not a security boundary
- `ClientValidationEnabled` flag controlling attribute emission

**Answer**

Client-side validation runs in the browser before or during form submission, providing immediate feedback without a server round-trip. In ASP.NET Core MVC it works through jQuery Validate reading `data-val-*` attributes that tag helpers emit from Data Annotations. The unobtrusive layer means no inline JavaScript — rules live as HTML metadata, so views stay free of hand-written validation code for standard annotation rules. Since validation only runs when a browser executes scripts correctly, it mirrors server rules purely for UX, which means it never replaces the server-side `ModelState` check that actually enforces correctness.

---

## Q2. What is unobtrusive validation?

**Concepts**
- `data-val-*` HTML attributes storing rule metadata separate from JavaScript
- `jquery.validate.unobtrusive.js` DOM parsing at page load
- Tag helper / `IHtmlGenerator` as attribute emitter at render time
- Manual `parse()` call required after AJAX partial updates
- Custom `ValidationAttribute` requiring explicit client adapters

**Answer**

Unobtrusive validation separates validation rules from JavaScript code by storing them as `data-val-*` attributes on form fields at render time. `jquery.validate.unobtrusive.js` reads those attributes when the page loads and configures jQuery Validate without inline script blocks, so views stay free of hand-written validation JavaScript for standard annotation rules. The trade-off is that custom `ValidationAttribute` subclasses need explicit client adapters, since the framework only knows how to translate built-in annotations. After AJAX partial updates that replace form HTML, I call `$.validator.unobtrusive.parse()` on the new container so rules wire up again on the fresh DOM.

---

## Q3. What scripts are required for unobtrusive client validation?

**Concepts**
- Dependency order: jQuery → `jquery.validate` → `jquery.validate.unobtrusive`
- `wwwroot/lib` convention for LibMan/npm packages
- Missing unobtrusive bridge as common silent failure cause

**Answer**

The stack requires three scripts loaded in exact order: jQuery, then `jquery.validate.min.js`, then `jquery.validate.unobtrusive.min.js`. Loading only jQuery Validate leaves `data-val-*` attributes inert because the bridge that reads them and registers rules with jQuery Validate is missing. Scripts typically live under `wwwroot/lib` and are referenced via the layout or a scripts partial. The most common cause of "validation attributes present but nothing happens" is including jQuery Validate but omitting the unobtrusive adapter file.

---

## Q4. What is `_ValidationScriptsPartial`?

**Concepts**
- Shared Razor partial centralizing validation script references
- `@section Scripts` placement for page-selective loading
- No substitute for tag-helper `data-val-*` emission on form fields

**Answer**

`_ValidationScriptsPartial` is a Razor partial that holds the jQuery Validate and jQuery Validate Unobtrusive script references in one place, so I include it in `@section Scripts` only on views that need client validation rather than duplicating paths across every form view. It renders after jQuery in the layout and before any page-specific scripts that depend on validation. Including this partial does not enable validation on its own — tag helpers still need to emit `data-val-*` attributes on form fields.

---

## Q5. What are `data-val-*` attributes and how are they generated?

**Concepts**
- `IHtmlGenerator` generating attributes from `ModelMetadata` and Data Annotations
- Standard examples: `data-val-required`, `data-val-range`, `data-val-regex`
- Hand-written HTML lacking attributes meaning no client validation
- `ClientValidationEnabled = false` suppressing emission
- `IClientModelValidator` required for custom attribute hooks

**Answer**

`data-val-*` attributes are HTML metadata on input elements describing client validation rules. Tag helpers use `IHtmlGenerator` and `ModelMetadata` to emit them from Data Annotations at render time — for example, `[Required]` produces `data-val="true"` and `data-val-required`, while `[Range]` produces `data-val-range` with min and max values. Hand-written `<input>` elements without `asp-for` carry none of these attributes, so jQuery Validate has no rules to register. Setting `ViewContext.ClientValidationEnabled = false` suppresses emission even when tag helpers are present, and custom attributes need `IClientModelValidator` adapters to add their own corresponding hooks.

---

## Q6. What is the relationship between Data Annotations and client-side validation?

**Concepts**
- Single declarative annotation source driving both server and client rules
- Standard annotation-to-unobtrusive adapter mappings built into the framework
- Custom `ValidationAttribute` requiring explicit client adapters
- ViewModels as annotation host rather than EF entities

**Answer**

Data Annotations on ViewModels define server-side validation rules and, when client validation is enabled, drive the `data-val-*` attributes that unobtrusive JavaScript consumes. The annotation is a single declarative source — the client rule is a best-effort mirror, which is why custom `ValidationAttribute` subclasses require explicit client adapters. Without one, the custom attribute enforces server-side only. Standard annotations like `[Required]`, `[StringLength]`, `[Range]`, and `[EmailAddress]` map automatically to built-in unobtrusive adapters. I annotate ViewModels rather than EF entities because the persistence layer should stay decoupled from UI validation concerns.

---

## Q7. Why is client-side validation not sufficient for security?

**Concepts**
- Client JavaScript executing entirely in the attacker's browser
- Direct HTTP POST bypassing browser scripts
- `ModelState.IsValid` as the only enforcement gate before persistence
- `[Remote]` validation also bypassable and subject to race conditions

**Answer**

Client-side validation executes in the attacker's browser and can be disabled, modified, or completely bypassed by posting directly with curl, Postman, or a custom HTTP client. Since the server has no way to know whether a request came through a browser with scripts enabled, the only enforcement boundary is server-side `ModelState` validation before any persistence, redirect, or side effect. Client validation improves UX for legitimate users, but treating it as a security control creates a false sense of safety. `[Remote]` validation is equally bypassable and subject to async race conditions, so the POST action must always re-validate the same rule regardless.

---

## Q8. What is `jquery.validate.unobtrusive.js` responsible for?

**Concepts**
- DOM parsing at page load for `data-val-*` attributes
- Rule object creation and jQuery Validate wiring
- Built-in adapter mappings: `required`, `range`, `regex`, `remote`
- Custom adapter registration via `$.validator.unobtrusive.adapters.add`
- Manual `parse()` for AJAX-loaded form fragments

**Answer**

`jquery.validate.unobtrusive.js` bridges MVC-generated `data-val-*` attributes to jQuery Validate by parsing the DOM, creating rule objects, and attaching validators to forms. It maps annotation metadata to jQuery Validate methods — `required`, `range`, `regex`, `remote` — without any inline scripts. Without this file, `data-val-*` attributes are present but no rules are ever registered. For custom business rules I add adapters via `$.validator.unobtrusive.adapters.add`, and for AJAX-loaded form fragments I call `$.validator.unobtrusive.parse()` on the container after the new HTML lands.

---

## Q9. What is `asp-validation-for` used for?

**Concepts**
- `<span>` tag helper for field-level error message display
- `data-valmsg-for` attribute wiring to jQuery Validate message placement
- CSS class toggling between `field-validation-valid` and `field-validation-error`
- Required for per-field errors when using `asp-validation-summary="ModelOnly"`

**Answer**

`asp-validation-for` renders a `<span>` for displaying validation messages for a specific model property, generating `data-valmsg-for` wired to jQuery Validate's message placement mechanism. It shows both client-side and server-side errors for the named property — when the form POSTs and returns with `ModelState` errors, this span displays the error text alongside the field. CSS classes toggle between `field-validation-valid` and `field-validation-error` based on state. It is required for visible per-field errors when the validation summary uses `"ModelOnly"`, since that mode only shows model-level errors in the summary.

---

## Q10. What is the difference between `asp-validation-summary="All"` and `"ModelOnly"`?

**Concepts**
- `"All"` rendering both property-level and model-level errors
- `"ModelOnly"` rendering only errors added via `ModelState.AddModelError(string.Empty, ...)`
- `"None"` suppressing the summary entirely
- Mismatched mode leaving property errors invisible to the user

**Answer**

`"All"` renders both property-level annotation failures and model-level errors in the summary list, so every failure appears in one place at the top of the form. `"ModelOnly"` renders only errors added explicitly with `ModelState.AddModelError(string.Empty, message)` — property errors appear only in adjacent `asp-validation-for` spans. I use `"All"` when I want a single error banner listing all failures, and `"ModelOnly"` when each field has its own span and the summary is reserved for cross-field or business-rule errors. Choosing `"ModelOnly"` without per-field spans leaves users seeing an empty summary when only property annotation rules fail.

---

## Q11. How does `[Remote]` validation work on the client?

**Concepts**
- jQuery Validate `remote` rule sending asynchronous AJAX request
- Server action returning `true` or string error message as JSON
- Async race condition on fast form submission before response returns
- Re-validation required in the POST action regardless

**Answer**

`[Remote]` generates a jQuery Validate `remote` rule that sends an AJAX request to a specified action to check field validity asynchronously — the classic example is username availability. The server action returns `true` for valid or a string error message for invalid as JSON. The call is asynchronous, which means a user who fills the field and submits quickly can submit the form before the remote response returns, so the form may POST while the check is still in flight. I always re-validate the same rule in the POST action or service layer and treat `[Remote]` as UX feedback only, never as enforcement.

---

## Q12. What is a client validation adapter for custom `ValidationAttribute`s?

**Concepts**
- `IClientModelValidator.AddValidation` emitting `data-val-*` attributes
- `ClientModelValidatorProvider` registration in DI
- Matching jQuery Validate custom method reading emitted attributes
- Server `ValidationAttribute` as authoritative rule; adapter mirrors it for UX

**Answer**

A client validation adapter implements `IClientModelValidator` and translates a custom server-side `ValidationAttribute` into `data-val-*` attributes that unobtrusive JavaScript can read. The `AddValidation(ClientModelValidationContext context)` method emits attributes like `data-val-mustbefuturedate`, and a matching jQuery Validate custom method or unobtrusive adapter reads those attributes and registers the client rule. Without this adapter, the custom attribute enforces server-side only. The server `ValidationAttribute` remains authoritative — the client adapter mirrors it for UX — and I register the adapter through `ClientModelValidatorProvider` in the `AddControllersWithViews` options.

---

## Q13. What is `ClientValidationEnabled` on `ViewContext`?

**Concepts**
- Boolean flag controlling `data-val-*` attribute emission at render time
- `_ViewImports.cshtml` as common per-app configuration point
- No effect on server-side `ModelState` validation on POST
- Environment-based disabling for Production without breaking validation correctness

**Answer**

`ClientValidationEnabled` is a boolean on `ViewContext` that controls whether tag helpers emit `data-val-*` attributes during view rendering. When `false`, inputs render without client validation metadata, so no scripts have rules to attach. I commonly set this in `_ViewImports.cshtml` or individual views, often tied to environment so it is enabled in Development for faster feedback and disabled in Production to reduce script overhead or avoid stale client rules during debugging. Disabling it has no effect on server-side `ModelState` validation, which always runs on POST regardless.

---

## Q14. Why does hand-written HTML input lose client-side validation?

**Concepts**
- `IHtmlGenerator` as render-time metadata-driven attribute emitter
- `asp-for` tag helper as the trigger for `data-val-*` emission
- Hand-written `<input>` carrying no attributes = no client rules
- `data-valmsg-for` wiring also broken without tag helpers

**Answer**

Plain HTML inputs without `asp-for` do not receive `data-val-*` attributes because the unobtrusive layer is entirely metadata-driven at render time via `IHtmlGenerator`. Without those attributes, jQuery Validate has no rules to register, so client validation silently disappears while server-side `ModelState` continues to work on POST. Using `asp-for="Quantity"` instead of a raw `<input>` is the correct fix; manually adding every required `data-val-*` attribute is error-prone. `asp-validation-for` spans also need tag helpers to generate correct `data-valmsg-for` wiring, so custom markup breaks both the input and the error display.

---

## Q15. How do you localize jQuery Validate error messages?

**Concepts**
- `.resx` files / `IValidationMetadataProvider` affecting `data-val-*-message` on rendered fields
- jQuery Validate built-in method messages remaining English by default
- Localized `messages_xx.min.js` file overriding built-in method messages
- `$.validator.messages` override as inline alternative
- `RequestLocalization` culture alignment with loaded messages script

**Answer**

Server-side localization via `.resx` files or `IValidationMetadataProvider` affects `ModelState` messages and, importantly, the `data-val-*-message` attributes rendered on form fields — so annotation-backed rules like `[Required]` show the localized message through the unobtrusive layer. But jQuery Validate's built-in method messages for `email`, `number`, and `date` remain English because they come from the library, not from server metadata. To fix those I load a localized messages file like `messages_fr.min.js` after `jquery.validate.min.js` and before the unobtrusive script parses the form, or I call `$.extend($.validator.messages, { required: "..." })`. I align `RequestLocalization` culture with the messages script loaded for the current request so both paths produce matching language.

---

## Q16. Why must server actions still check `ModelState.IsValid` when client validation is enabled?

**Concepts**
- Client validation as optional UX only for legitimate browser users
- Direct HTTP POST bypassing browser scripts entirely
- `ModelState.IsValid` as mandatory enforcement gate
- Shared ViewModel annotations enforcing same rules on both paths

**Answer**

Client validation only runs when browsers execute scripts correctly, which means any HTTP client can POST invalid or malicious payloads directly to the action, bypassing all client rules. `ModelState.IsValid` is the enforcement gate before persistence, redirects, or side effects — skipping it while relying on client validation creates a security defect. Client and server validation share the same ViewModel annotations but serve different roles: client-side for immediate UX feedback, server-side for correctness. On validation failure I return `View(model)` so the user sees the errors with the form repopulated.

---

## Q17. Why doesn't client validation fire when using a button click handler instead of form submit?

**Concepts**
- jQuery unobtrusive validation intercepting the form `submit` event
- `type="button"` bypassing the submit event entirely
- `$form.valid()` as explicit check required before AJAX send
- `type="submit"` with `preventDefault` as the safer alternative

**Answer**

jQuery unobtrusive validation intercepts the form `submit` event, so a `type="button"` click handler that calls `fetch` or `$.post` directly bypasses the validator entirely. The fix is to call `$('#form').valid()` before sending the request and abort if it returns `false`. Using `type="submit"` and preventing the default in a submit handler gives the validator a chance to run first. This pattern comes up frequently in AJAX partial-form saves where developers want to avoid a full page POST, and server-side validation remains mandatory regardless of the client guard.

---

## Q18. What is the difference between MVC unobtrusive validation and SPA/API validation?

**Concepts**
- Unobtrusive validation specific to Razor-rendered HTML forms with jQuery scripts
- `[ApiController]` automatic 400 `ValidationProblemDetails` for API paths
- SPA libraries (Zod, React Hook Form) as separate client rule implementations
- Shared ViewModel annotations enforcing server rules on both pipelines independently

**Answer**

MVC unobtrusive validation applies only to Razor-rendered HTML forms with jQuery Validate scripts — JSON API consumers never run those scripts. For SPA and API paths, server validation runs via `[ApiController]` automatic 400 responses or explicit `ModelState` checks, and the browser SPA must implement its own client-side rules separately using libraries like Zod or React Hook Form. Shared ViewModel annotations enforce the same server rules across both pipelines if each pipeline validates, but unobtrusive scripts have no effect on JSON API consumers. I don't assume that annotations on a shared DTO automatically protect every host entry point.

---

## Gotchas — ASP.NET Core MVC (Interview Traps)

#### Gotcha 1. Business logic in Razor views

**Concepts**
- Views as presentation-only layer
- Business rules in Razor bypassing unit tests
- Authorization belonging to filters and policies
- Service layer as owner of calculations and decisions

**Answer**

Placing pricing, discount, or business rules in `.cshtml` files means that logic cannot be unit tested, often duplicates the service layer, and diverges from API or batch behavior over time. Views should only render data the controller or ViewModel already computed. Authorization belongs in filters, policies, or controller checks before the view executes, not in conditional Razor blocks. I keep Razor limited to presentation formatting — any calculation or decision that affects correctness lives in services.

---

#### Gotcha 2. EF entities passed directly to views

**Concepts**
- Navigation property lazy-load triggering unexpected queries during rendering
- Over-posting via mass assignment on POST action binding
- Dedicated ViewModels as UI-contract decoupling layer
- Controller or mapping service as entity-to-ViewModel boundary

**Answer**

Binding and displaying EF Core entities exposes navigation properties that trigger unexpected lazy queries during rendering and enables mass assignment of properties users should not control — like `IsAdmin` — on POST. The fix is a dedicated ViewModel with only the fields the view needs, mapped in the controller or a mapping service before passing to the view or reading from the form.

---

#### Gotcha 3. `[FromBody]` on HTML form POST

**Concepts**
- `application/x-www-form-urlencoded` vs JSON `Content-Type` mismatch
- `[FromBody]` using JSON input formatter, leaving model at defaults on mismatch
- Silent binding failure producing no exception
- `FormData` following form binding rules, not JSON binding

**Answer**

Standard browser forms send `application/x-www-form-urlencoded` or `multipart/form-data`, not JSON. `[FromBody]` uses the JSON input formatter and leaves the model at default values when the content type doesn't match, so the action runs with empty or zero fields without any error or exception. I remove `[FromBody]` for conventional form POSTs and let model binding read form fields. This is a common cause of "my POST action receives null model" bugs that are hard to spot because the action itself returns successfully.

---

#### Gotcha 4. Skipping `ModelState.IsValid` because of client validation

**Concepts**
- Client validation bypassable by any HTTP client
- `ModelState.IsValid` as mandatory server enforcement gate
- `[Remote]` and unobtrusive rules not being security boundaries
- Missing server check as a security defect

**Answer**

Client-side validation is bypassable — attackers POST directly without running browser scripts. Server-side validation is mandatory before any persist, redirect, or side effect. I always gate POST actions with `if (!ModelState.IsValid) return View(model);`. Remote validation and unobtrusive rules are not security boundaries, so I treat missing server validation as a security defect regardless of what client scripts are present.

---

#### Gotcha 5. `return View()` after successful POST

**Concepts**
- Duplicate submission on browser refresh of a POST response
- Post-Redirect-Get (PRG) pattern separating mutation from display
- `TempData` for flash messages across redirect
- AJAX partial POSTs needing idempotent logic or disabled submit during request

**Answer**

Returning the same view after a successful POST means the browser resubmits the POST body when the user refreshes. The fix is Post-Redirect-Get: `return RedirectToAction(nameof(Index))` after successful create or update, separating the mutation from the display. Flash success messages go via `TempData` on the redirect target. AJAX partial POSTs have a similar concern — I disable the submit button during the request or use idempotent server logic.

---

#### Gotcha 6. `ModelState` after redirect

**Concepts**
- `ModelState` as request-scoped, not surviving `RedirectToAction`
- Return `View(model)` on failure, redirect only on success pattern
- `TempData` serialization as escape hatch for redirect-on-failure

**Answer**

`ModelState` is request-scoped and does not survive `RedirectToAction` — validation errors are lost unless I redisplay the form without redirecting on failure. The standard pattern is: redirect only on success; on validation failure return `View(model)` with errors displayed inline. To survive a redirect on failure, I serialize errors to `TempData`, but the simpler and more common approach avoids that entirely by never redirecting when validation fails.

---

#### Gotcha 7. TempData read twice in layout and view

**Concepts**
- `TempData` consumed on first read by default
- `Peek()` reading without consuming
- `Keep()` retaining after first read for a second consumer
- Single consumption point as simpler alternative
- Cookie-based TempData size limits

**Answer**

`TempData` is consumed on the first read by default, so if the layout reads a flash message to display it, the view sees nothing. I use `TempData.Peek("Message")` in the layout to read without consuming, or call `TempData.Keep("Message")` after the layout reads so the view can also read it. The simpler approach is a single consumption point — either the layout or a dedicated partial, not both. Cookie-based `TempData` has size limits, so I avoid storing large payloads.

---

#### Gotcha 8. Missing `[Area]` attribute on area controllers

**Concepts**
- `[Area("AreaName")]` required for area route discovery
- Area routing registered separately with `{area:exists}` constraint
- Controller treated as root controller without the attribute

**Answer**

Controllers in `Areas/Admin/Controllers` without `[Area("Admin")]` are not discovered by the areas route — they return 404 or match the wrong conventional route. Every area controller must declare `[Area("AreaName")]` matching its folder, and area routing is registered separately in `Program.cs` with the `{area:exists}` constraint. I verify area registration order so specific area routes come before catch-all default routes.

---

#### Gotcha 9. Link generation without `asp-area`

**Concepts**
- Tag helpers defaulting to current area context when generating URLs
- `asp-area` required for cross-area links
- `Url.Action` requiring `area` in route values object

**Answer**

Tag helpers default to the current area context when generating URLs, so links from a root view to an area controller need explicit `asp-area="Admin"` or they generate URLs without the area segment. From within an area, omitting `asp-area` keeps links inside the current area, which is sometimes the wrong intent. Cross-area links require both `asp-area` and `asp-controller` (and `asp-action`). The same rule applies to `Url.Action` — I pass `new { area = "Admin" }` in route values.

---

#### Gotcha 10. Checkbox `[Required]` on non-nullable `bool`

**Concepts**
- Unchecked checkbox posting nothing, model binding defaulting to `false`
- `[Required]` never failing on non-nullable `bool` since `false` is a valid value
- `bool?` with `[Required]` requiring an explicit selection
- Hidden-field pattern for deliberate `false` posting

**Answer**

An unchecked checkbox posts nothing, so model binding sets a non-nullable `bool` to `false`. `[Required]` never fails because `false` is a valid non-null value — the attribute only rejects `null`. I use `bool?` with `[Required]` when I need an explicit true selection for consent checkboxes, or the hidden-field pattern where a hidden input posts `false` and the checkbox posts `true`, so unchecked still posts a deliberate `false` rather than nothing at all.

---

#### Gotcha 11. Collection binding with gap indices

**Concepts**
- Contiguous zero-based index requirement for collection model binder
- Client-side reindexing after row deletion
- Custom `IModelBinder` as alternative for non-contiguous indices
- Partial views needing consistent index naming throughout

**Answer**

Deleting a row from a dynamic form leaving indices like `Lines[0]` and `Lines[2]` breaks model binder alignment — index 1 is missing and subsequent items may bind incorrectly or truncate. I reindex client-side after row deletion so indices are contiguous starting at zero. Alternatively, a custom `IModelBinder` can tolerate non-contiguous indices. Partial views rendering collection editors must maintain consistent index naming throughout add and delete operations.

---

#### Gotcha 12. `@Html.Raw` with user content

**Concepts**
- Razor auto-encoding preventing XSS by default
- `@Html.Raw` bypassing encoding for attacker-supplied content
- Server-side sanitization before any raw rendering
- Content-Security-Policy as defense-in-depth, not a replacement for encoding

**Answer**

Default Razor encoding prevents XSS by HTML-encoding output, so `@Html.Raw(Model.UserComment)` with unsanitized user content renders any attacker-supplied script. I never wrap raw user input in HTML — I use `@Model.UserComment` for auto-encoding, or sanitize server-side with a trusted HTML sanitizer library before any raw rendering. AJAX-loaded partials injected via `innerHTML` execute injected script exactly the same as full pages. Content-Security-Policy limits blast radius but does not replace encoding.

---

#### Gotcha 13. AJAX POST without antiforgery token

**Concepts**
- Form tag helpers emitting antiforgery tokens automatically
- `fetch`/jQuery AJAX requiring manual token inclusion
- `RequestVerificationToken` header or `__RequestVerificationToken` form field
- `[AutoValidateAntiforgeryToken]` rejecting missing tokens before action runs

**Answer**

Form tag helpers emit antiforgery tokens automatically, but `fetch` and jQuery AJAX must manually send the `RequestVerificationToken` header or `__RequestVerificationToken` form field — otherwise POSTs fail with 400 antiforgery errors. I read the hidden field value from the page and include it on every mutating AJAX request. `[AutoValidateAntiforgeryToken]` on the controller validates all unsafe methods so missing tokens fail before the action runs. The fix is always to add the token, never to disable antiforgery validation on MVC cookie-auth endpoints.

---

#### Gotcha 14. Injecting Hub into MVC controller

**Concepts**
- Hub not registered in DI for direct injection
- `IHubContext<THub>` as correct singleton proxy
- Hub requiring active connection context that DI-resolved instance lacks
- Redis backplane or Azure SignalR for multi-instance fan-out

**Answer**

Hubs are not registered in DI for direct injection into controllers — they are instantiated by SignalR per invocation with connection context. Injecting a concrete `Hub` fails activation or produces an instance without active connection state. I inject `IHubContext<THub>` instead, which is registered as a singleton proxy by `AddSignalR()`. Business notifications flow: controller → service → `IHubContext` → clients. For multi-instance deployments I pair with a Redis backplane or Azure SignalR.

---

#### Gotcha 15. SignalR scale-out without backplane

**Concepts**
- Sticky sessions routing connections but not cross-instance messages
- Redis backplane as pub/sub fan-out across all instances
- `IHubContext.Clients.User` missing connections on other instances without backplane
- Group membership and connection IDs local to each instance

**Answer**

Sticky sessions alone do not fan-out events across server instances — they route connections to the same node but do not relay messages. A controller on instance A calling `IHubContext.Clients.User(id).SendAsync` misses users connected to instance B without a backplane. Multi-node deployments need a Redis backplane via `AddStackExchangeRedis` or Azure SignalR Service so messages sent from any instance reach clients on all instances. Group membership and connection IDs are local to each instance, so the backplane carries the messaging instruction, not the connection state.

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

**Answer**

The POST action never checks `ModelState.IsValid` and always redirects, so any HTTP client can bypass browser scripts and submit invalid or malicious payloads directly to the server. The `[Required]` and `[MinLength(8)]` annotations generate `data-val-*` attributes for the browser, but those attributes have no effect on the server-side execution path — the action unconditionally redirects regardless of what was submitted. The fix is to add `if (!ModelState.IsValid) return View(model);` before any persistence or redirect. The `_ValidationScriptsPartial` stays for UX; it does not replace server checks. For API-style endpoints the equivalent is returning a `ValidationProblemDetails` 400; for MVC, re-rendering the view with `ModelState` errors.

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

**Answer**

`jquery.validate.unobtrusive.min.js` is the bridge that reads `data-val-*` attributes and registers rules with jQuery Validate. Without it, `data-val-required` and all other attributes are present in the HTML but nothing ever parses them or attaches validators to the form — so blur and submit events produce no errors. jQuery Validate alone does not know to look at `data-val-*` attributes; it needs the unobtrusive adapter to translate them. The fix is to restore `jquery.validate.unobtrusive.min.js` in the scripts section. Removing it to "reduce bundle size" saves a small number of kilobytes while silently disabling all client-side validation.

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

**Answer**

The framework only knows how to translate built-in annotations into `data-val-*` attributes — custom `ValidationAttribute` subclasses are invisible to the client pipeline until I implement `IClientModelValidator`. I add `AddValidation(ClientModelValidationContext context)` on the attribute (or via a separate adapter class) to emit a custom attribute like `data-val-mustbefuturedate="true"` and `data-val-mustbefuturedate-message="Ship date must be in the future."`. That alone is not enough — I also need a matching jQuery Validate custom method and unobtrusive adapter registered in JavaScript that reads `data-val-mustbefuturedate` and performs the comparison. The `IClientModelValidator` registration happens either by implementing the interface directly on the attribute or by registering a `ClientModelValidatorProvider` in `AddControllersWithViews` options. The server `ValidationAttribute` remains the authoritative rule; the client adapter is purely UX.

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

**Answer**

The remote validation AJAX call is asynchronous — when a user tabs through the field quickly and clicks Submit, the form submits before the remote response returns, since jQuery Validate does not block submit until all pending remote checks complete. There is no submit guard or debounce, so the race window is wide. The POST action must always re-validate uniqueness server-side regardless of what `[Remote]` reported in the browser, because `[Remote]` is UX feedback only. To narrow the race for users, I disable the submit button while a remote check is in flight and re-enable it after the response arrives, and I add a short debounce on the field's change event. The authoritative duplicate check is a unique constraint in the database or an explicit uniqueness check in the service before account creation.

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

**Answer**

`asp-validation-summary="ModelOnly"` renders only errors added explicitly with `ModelState.AddModelError(string.Empty, ...)` — it intentionally excludes property-level annotation failures. Since `[Required]` and `[Range]` are property-level rules, they appear in the `asp-validation-for` spans next to each field, not in the summary div. The summary div is blank because no model-level errors were added. Users who expect all errors in one banner should use `asp-validation-summary="All"`. The per-field spans already show the correct property errors — the only mismatch is the summary mode chosen. I use `"ModelOnly"` when I want a clean separation: the summary for business-rule messages I add manually, and the spans for annotation failures.

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

**Answer**

The `ClientValidationEnabled` assignment works correctly — setting it in `_ViewImports.cshtml` applies to every view before any tag helper emits attributes, so `data-val-*` attributes are suppressed in Production and emitted in Development. The remaining issue is that `_ValidationScriptsPartial` loads the jQuery Validate and unobtrusive scripts in all environments even when `ClientValidationEnabled` is `false`, wasting bandwidth in Production on scripts that have nothing to wire up. The fix is to also gate the partial behind the same environment check, either inline in the layout or by reading `ClientValidationEnabled` from `ViewContext`. Server-side `ModelState` validation is unaffected by this flag and continues running normally in both environments, which is the correct behavior.

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

**Answer**

`asp-for="Quantity"` triggers `IHtmlGenerator` which reads `ModelMetadata` for the `Quantity` property, finds `[Required]` and `[Range(1, 100)]`, and emits `data-val="true"`, `data-val-required`, `data-val-required-message`, `data-val-range`, `data-val-range-min="1"`, `data-val-range-max="100"`, and `data-val-range-message` as HTML attributes. The hand-written `<input>` carries none of those attributes, so `jquery.validate.unobtrusive.js` finds no rules to register for that field — client validation silently does nothing. Server-side `ModelState` validation continues to work on POST because it reads the same `ModelMetadata` independently of how the HTML was rendered. The fix for design-system inputs is either to keep `asp-for` and override only styling via CSS class, or manually add every required `data-val-*` attribute to the hand-written element — the latter is fragile because any annotation change requires updating the HTML manually.

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

**Answer**

The `data-val-required-message` attribute on the rendered `<input>` will contain the French string from the resource file, so the unobtrusive layer correctly shows the French message for annotation-backed rules. The problem is jQuery Validate's built-in method messages — `required`, `email`, `number`, `date` — which are hardcoded English strings inside `jquery.validate.min.js` and are not affected by server-side localization at all. To override them for French I load `messages_fr.min.js` (from the jQuery Validation locales package) after `jquery.validate.min.js` and before `jquery.validate.unobtrusive.min.js` parses the form, so the French strings are in place before rules are registered. Alternatively I call `$.extend($.validator.messages, { required: "Ce champ est obligatoire.", ... })` in a page script block. Without this step, annotation-backed messages show French while format validators show English.

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

**Answer**

The data annotations on `OrderViewModel` protect the server on both paths but through entirely different mechanisms. On the MVC admin path, tag helpers emit `data-val-*` attributes from the annotations and `jquery.validate.unobtrusive.js` provides client-side UX feedback before POST — then `ModelState.IsValid` enforces the same rules server-side. On the API path with `[ApiController]`, the framework automatically runs model validation on the bound `OrderViewModel` and returns a `ValidationProblemDetails` 400 before the action body executes if validation fails — no unobtrusive scripts are involved at all. The React SPA gets no client-side validation from the annotations; it must implement its own input rules using a library like Zod or React Hook Form. Those client rules are a separate implementation from the server annotations — changes to one do not automatically propagate to the other, so the team must keep both in sync deliberately.

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

---

**Answer**

The button is `type="button"`, so clicking it fires the jQuery click handler directly without triggering the form `submit` event that jQuery unobtrusive validation intercepts. The AJAX `$.post` call fires regardless of field state because the validator is never consulted. The fix is to call `$('#profile-form').valid()` before sending the request and return early if it returns `false`. A cleaner alternative is to change the button to `type="submit"`, prevent the default submit in a `submit` event handler, and call `$(this).valid()` there. Either way, the server-side action must still check `ModelState.IsValid` because the AJAX request can also be sent directly from outside the browser.

---
