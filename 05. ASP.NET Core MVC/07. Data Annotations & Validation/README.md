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

**Concepts**
- Declarative attributes from `System.ComponentModel.DataAnnotations`
- Validation rules, display metadata, and binding hints in one place
- MVC pipeline reading annotations during binding to populate `ModelState`
- Client-side `data-val-*` attributes emitted from annotation metadata
- Annotations on ViewModels vs EF entities

**Answer**

Data Annotations are attributes you place on ViewModel or DTO properties to express validation rules, display formatting, and binding hints without writing imperative validation code. The MVC pipeline reads them during model binding and validation, accumulating errors in `ModelState` so the action can check `ModelState.IsValid` before proceeding. Common validation attributes are `[Required]`, `[StringLength]`, `[Range]`, `[EmailAddress]`, `[Compare]`, and `[RegularExpression]`. Display attributes like `[Display(Name = "...")]` and `[DataType(DataType.Password)]` shape labels and input types that Tag Helpers generate. The framework also translates annotations into `data-val-*` attributes on inputs, which jQuery Validate Unobtrusive reads for client-side feedback without any inline JavaScript. Because annotations do not replace the explicit `ModelState.IsValid` guard in MVC controllers (unlike `[ApiController]`), the check remains a required step in every POST action. Annotations belong on ViewModels rather than EF entities when the same entity is shared across persistence and UI layers.

---

## Q2. What is `ModelState` and how does it relate to validation?

**Concepts**
- `ModelState` dictionary recording binding errors and validation failures per property
- `ModelStateEntry` holding attempted value, errors, and binding exceptions
- Validation attributes, `IValidatableObject`, and custom attributes all writing into `ModelState`
- Tag helpers reading `ModelState` to render field-level error spans
- Request-scoped lifetime lost on redirect

**Answer**

`ModelState` is a dictionary on the controller (via `ControllerBase.ModelState`) keyed by property name, where each entry holds the attempted value, any binding exceptions, and a list of validation errors accumulated from all validation sources. After model binding, the validation pipeline runs annotation attributes, then `IValidatableObject.Validate`, and they all write errors into the same `ModelState` structure. The action reads `ModelState.IsValid` to decide whether to proceed or return the view. Tag helpers like `asp-validation-for="Email"` read `ModelState["Email"]` to render error messages next to inputs, which is why returning `View(model)` on failure preserves the error display — the model carries the submitted values and `ModelState` carries the errors that the Tag Helpers surface. `ModelState` is scoped to the current HTTP request and is discarded when the response is sent, so errors do not survive a `RedirectToAction` unless explicitly serialized to TempData or the form is re-validated on GET.

---

## Q3. Why must server-side validation always be performed even with client validation?

**Concepts**
- Client validation running only in the browser — bypassable with curl or Postman
- Direct HTTP POST bypassing browser JavaScript entirely
- MVC controllers not auto-rejecting invalid models without `ModelState.IsValid` check
- Remote validation not invoked during server-side POST processing
- Server validation as the authoritative security gate

**Answer**

Client-side validation runs in the user's browser and can be stripped entirely by disabling JavaScript, modifying the page source, or sending requests directly with curl, Postman, or a crafted HTTP client that never loads the form page. An attacker who POSTs `Email=notanemail&Password=abc` directly to the action endpoint bypasses every `data-val-*` attribute the server emitted. MVC controllers do not automatically reject invalid models the way `[ApiController]` does — the guard must be an explicit `if (!ModelState.IsValid) return View(model);` before any service call or database write. Remote validation with `[Remote]` is particularly deceptive: it fires an AJAX availability check in the browser but is never invoked when the server processes the POST, so a username uniqueness check enforced only via `[Remote]` is silently bypassed on direct POST. I treat client validation as a UX convenience that prevents unnecessary round trips for legitimate users, and server validation as the authoritative security boundary that must always run.

---

## Q4. What does `ModelState.IsValid` check?

**Concepts**
- `IsValid` returning true only when all entries have no validation or binding errors
- Binding failures populating errors before annotation validation runs
- Cross-property `IValidatableObject` errors contributing to `IsValid`
- `IsValid` reading results — not triggering validation
- Returning `View(model)` on failure to preserve error display

**Answer**

`ModelState.IsValid` returns `true` only when every entry in the `ModelState` dictionary has zero errors — neither binding errors (type conversion failures, missing required value types) nor validation errors from annotation attributes or `IValidatableObject`. Binding failures are recorded before annotation validation runs, so a non-parseable date string in a `DateTime` field makes `IsValid` false immediately, even before `[Required]` or `[Range]` are evaluated. Checking `IsValid` does not trigger validation — validation runs automatically during model binding; `IsValid` only reads the already-accumulated result. On failure the action must return `View(model)` with the same bound model instance so that field values are re-rendered as submitted and Tag Helpers can look up errors by the same property-name keys that were written during binding.

---

## Q5. What is the difference between `[Required]` and `[AllowNull]`?

**Concepts**
- `[Required]` enforcing runtime validation — null or empty string fails
- `[AllowNull]` as a nullable reference type compiler annotation only
- `[Required]` on `int?` — fails when field is absent and binds to null
- `[Required]` on non-nullable `int` — missing field binding to `0` satisfying the check
- `[Required(AllowEmptyStrings = true)]` for null-rejected but empty-string-accepted scenarios

**Answer**

`[Required]` is a runtime validation attribute that adds an error to `ModelState` when the property value is null or, for strings, an empty or whitespace string. `[AllowNull]` from `System.Diagnostics.CodeAnalysis` is a nullable reference type (NRT) compiler annotation that tells the C# compiler the property can legitimately be assigned `null` — it has no effect on runtime validation and MVC never reads it. The important nuance is what `[Required]` does on different types: on `string`, it fails for null and `""` (and whitespace by default); on `int?`, it fails when the field is absent and binds to `null`, since null is the "missing" signal for nullable types; on non-nullable `int`, an absent field binds to `0` which satisfies `[Required]` because `0` is a valid non-null value, so the validation silently passes even though no value was posted. Use `[Required(AllowEmptyStrings = true)]` when empty string is acceptable but null is not — such as optional text fields where the column allows empty strings but not database nulls.

---

## Q6. Why does `[Required]` not work as expected on a `bool` checkbox?

**Concepts**
- Unchecked HTML checkbox not submitting a key
- Non-nullable `bool` binding absent key to `false`
- `[Required]` checking for non-null — `false` satisfies it
- `bool?` with `[Required]` for explicit-choice consent checkboxes
- `[Range(typeof(bool), "true", "true")]` enforcing must-be-true

**Answer**

HTML checkboxes only submit their value when checked — an unchecked checkbox contributes nothing to the POST body. For non-nullable `bool`, the model binder treats an absent key as `false`, which is a perfectly valid non-null value. `[Required]` checks for null or empty, not for a specific truthy value, so `false` passes and an unchecked required consent checkbox never fails validation. The user skips the terms acceptance, the POST succeeds, and the application proceeds as if terms were accepted. The fix for a consent checkbox is to use `bool?` with `[Required]`: an absent key binds to `null`, which fails the required check, while a checked box binds to `true`, which passes. For a "must be true" assertion (not just "must not be null"), add `[Range(typeof(bool), "true", "true")]` or a custom attribute that explicitly rejects `false` — because `bool?` with `[Required]` alone would allow a crafted POST with `AcceptTerms=false` to satisfy the required check by sending an explicit false instead of nothing.

---

## Q7. What is `[Compare]` used for?

**Concepts**
- `[Compare("OtherPropertyName")]` validating cross-property equality
- String-literal property name — refactor-rename blind spot
- Client-side `data-val-equalto-other` for browser-side comparison
- Server-side compare running correctly regardless of client script
- Silent pass when named property does not exist on the type

**Answer**

`[Compare("OtherProperty")]` validates that the decorated property's value equals another named property on the same model instance — the canonical use case is `ConfirmPassword` matching `Password`. Comparison runs server-side during validation after both properties are bound, and the client-side unobtrusive adapter emits `data-val-equalto-other` so browsers show a mismatch error before submit. The critical trap is that property names are string literals: if `Password` is renamed to `NewPassword` and the attribute argument is not updated, `[Compare("Password")]` silently passes on the server because the framework cannot find the referenced property and treats the comparison as successful rather than throwing. Client-side validation also breaks because the generated `data-val-equalto-other` references the old field name, so users see no client error even when the passwords differ, and the server accepts mismatched inputs. Because `[Compare]` does not participate in the POST pipeline when invoked via direct HTTP (only the client script would catch it without server enforcement), always verify that the attribute argument matches the current property name after any rename.

---

## Q8. What are `[Range]` and `[StringLength]` used for?

**Concepts**
- `[StringLength(max)]` constraining Unicode character count
- `[Range(min, max)]` constraining numeric or date values to an inclusive interval
- Type-specific `[Range(typeof(decimal), "0.01", "9999.99")]` for non-integer bounds
- Client-side `data-val-range` and `data-val-length` attributes for browser validation
- Custom `ErrorMessage` or resource-based localization

**Answer**

`[StringLength(maximumLength)]` constrains the length of a string property, measured in characters rather than bytes, which matters for Unicode content where a single character may occupy multiple bytes but counts as one character for length purposes. An optional `MinimumLength` adds a lower bound: `[StringLength(100, MinimumLength = 3)]` rejects strings shorter than 3 or longer than 100 characters. `[Range(min, max)]` constrains numeric or date values to an inclusive interval: `[Range(1, 100)]` rejects values outside [1, 100] for integers; `[Range(typeof(decimal), "0.01", "9999.99")]` is the decimal overload since numeric literals cannot express decimals directly in attribute arguments. Both attributes generate corresponding `data-val-length` and `data-val-range` HTML attributes for unobtrusive client validation, so the browser rejects out-of-range values before the POST. Error messages are customizable via `ErrorMessage = "..."` or resource-based `ErrorMessageResourceName` and `ErrorMessageResourceType` for localization.

---

## Q9. What is `[RegularExpression]` used for?

**Concepts**
- `[RegularExpression("pattern")]` validating a string against a .NET regex
- .NET regex syntax differing slightly from JavaScript regex
- Client-side unobtrusive `data-val-regex-pattern` using JavaScript regex engine
- Pattern anchoring required to avoid partial matches
- Server validation always authoritative regardless of client regex

**Answer**

`[RegularExpression("pattern")]` validates that a string property matches a .NET regular expression from start to end — useful for phone numbers, postal codes, or usernames with specific character constraints. The pattern uses .NET regex syntax, which differs from JavaScript regex in some edge cases such as lookahead assertions, character class escapes, and the treatment of anchors, so a pattern that works in C# may behave differently in the jQuery Validate client adapter that reads `data-val-regex-pattern`. Patterns that do not anchor with `^` and `$` can match partially, so `[RegularExpression("[0-9]+")]` on a UK postal code would accept `"SW1A 2AA"` as valid because it finds digit substrings, which is incorrect; always anchor to `^` and `$`. The attribute is appropriate for simple single-field format constraints; overly complex patterns with conditional branches are better expressed in `IValidatableObject` or FluentValidation where logic is easier to test and explain in error messages. Always re-validate on the server since client-side regex is advisory.

---

## Q10. What is `IValidatableObject` and when do you use it?

**Concepts**
- `Validate(ValidationContext)` returning `IEnumerable<ValidationResult>`
- Cross-property or conditional rules that single attributes cannot express
- Running after all property-level attribute validation
- `memberNames` targeting specific properties for `asp-validation-for` display
- Server-only validation — no client-side adapter

**Answer**

`IValidatableObject` is an interface with a single `Validate(ValidationContext context)` method that returns an enumerable of `ValidationResult` objects for cross-property or conditional rules that cannot be expressed by a single attribute on one property. Examples are "EndDate must be after StartDate," "at least one of the two contact fields must be provided," or "quantity must be positive when status is Active." The framework calls `Validate` after all property-level annotation attributes have been evaluated, so when `Validate` runs, the action knows that each individual property already satisfies its own constraints. Results can specify one or more member names (`new ValidationResult("End must follow start", new[] { "EndDate" })`) which bind to `asp-validation-for` spans for inline display, or an empty member name array to produce a model-level error shown in the validation summary. Because `IValidatableObject` runs server-only and has no client-side adapter, the browser never blocks submit on its rules — the form always POSTs and the user sees the error after the round trip. For logic simple enough to express in a few lines, `IValidatableObject` on the ViewModel is the cleanest approach; elaborate rule sets belong in FluentValidation validators where they can be unit-tested independently.

---

## Q11. What is the difference between `IValidatableObject` and a custom `ValidationAttribute`?

**Concepts**
- Custom `ValidationAttribute` decorating a property and reusable across models
- `IValidatableObject.Validate` centralizing object-level rules on one type
- Property-scoped vs object-scoped rule placement
- Custom attribute enabling client-side adapter via `IClientModelValidator`
- `IValidatableObject` running server-only without extra work

**Answer**

A custom `ValidationAttribute` subclass decorates a specific property and carries its logic in `IsValid(object? value, ValidationContext context)`. The same attribute can be applied to many properties across many ViewModels with different constructor parameters, making it reusable and discoverable from the property declaration. It can also implement `IClientModelValidator` to emit `data-val-*` attributes and provide browser-side enforcement, which `IValidatableObject` cannot do without additional plumbing. `IValidatableObject` puts all cross-property rules in one `Validate` method on the model class, which means the rules are colocated with the type they validate and see all properties at once without needing `ValidationContext.ObjectInstance` casting. The trade-off is that `IValidatableObject` rules are object-scoped and not portable to other model types — they run after all property attributes, so any property-level failure stops `Validate` from running in some versions of the framework. The decision comes down to reusability: if the rule applies to `decimal` values across multiple forms (discount percent, tax rate, price), a `ValidationAttribute` is better; if the rule compares two properties that exist only on one ViewModel type, `IValidatableObject` is simpler.

---

## Q12. What is `[Remote]` validation and how does it work?

**Concepts**
- `[Remote(action, controller)]` triggering AJAX availability check during client validation
- Remote action returning JSON `true`/`false` or error string
- Server-side POST not invoking `[Remote]` — must re-enforce the rule manually
- Rate limiting and authentication required on remote endpoints
- `[Remote]` as UX convenience, not a security control

**Answer**

`[Remote(action: "CheckUsername", controller: "Account")]` generates a `data-val-remote-url` attribute that jQuery Validate uses to fire an AJAX GET to the specified action during client-side validation, checking whether the value is acceptable before the form submits — the canonical use case is verifying that an email or username is not already taken. The remote action returns `Json(true)` for a valid value or `Json("Username is taken.")` for an error, and the client displays the error inline if the server returns a string. The critical limitation is that `[Remote]` is never invoked during server-side POST processing — the framework does not fire the AJAX check when the full form POST arrives, so the uniqueness rule is not enforced on direct HTTP calls. I always duplicate the same check inside the POST action (`if (_db.Users.Any(u => u.Email == model.Email)) ModelState.AddModelError("Email", "Already taken.");`) because `[Remote]` is purely a client-side UX improvement. The remote endpoint also needs rate limiting and ideally authentication to prevent username enumeration via automated requests that bypass the form entirely.

---

## Q13. What is `[ValidateNever]` and when is it applied?

**Concepts**
- `[ValidateNever]` excluding a property from validation even if annotations are present
- Server-populated fields that were never user-submitted
- Pairing with `[BindNever]` for properties that should neither bind nor validate
- Not for skipping validation on editable user fields

**Answer**

`[ValidateNever]` from `Microsoft.AspNetCore.Mvc.ModelBinding.Validation` tells the validation pipeline to skip a property entirely — no annotation attributes, no `IValidatableObject` contributions — even if the property carries `[Required]` or other attributes. It is used on properties that are populated server-side and not submitted by the user: dropdown lists repopulated from a database on POST, server-assigned IDs, audit timestamps, or computed fields. Without `[ValidateNever]`, these properties arrive as null (since they were not in the form POST) and fail `[Required]`, producing spurious validation errors that prevent the form from submitting. Pairing `[ValidateNever]` with `[BindNever]` is the stronger combination when the property should neither be read from the request nor validated — together they fully isolate a property from inbound processing while still allowing the controller to set it before returning the view. The important boundary is not to apply `[ValidateNever]` to user-editable fields as a shortcut to suppress inconvenient validation — that creates security holes where invalid data bypasses the validation pipeline silently.

---

## Q14. Why should validation attributes not be placed on EF entities shared with MVC?

**Concepts**
- EF entity as a persistence model — validation rules differing from UI rules
- Navigation property validation triggering on related graphs during binding
- API vs MVC paths needing different rules on the same type
- ViewModel as the correct location for UI-specific validation
- EF configuration kept in Fluent API or data annotations limited to schema constraints

**Answer**

EF Core entities represent the database schema and persistence lifecycle — adding MVC validation annotations to them conflates two separate concerns with two separate lifecycles. A column that is required in the database may be optional on the first step of a multi-step wizard form; a price range valid for a web catalog may be too restrictive for a bulk import tool; a navigation property annotated with `[Required]` may trigger unexpected validation of entire related graphs during MVC model binding. More concretely, when the same entity type is used as both an MVC ViewModel and a minimal API request body, any `[Required]` or `[Range]` annotation applies to every endpoint identically, making it impossible to have different validation rules per context. API callers may send partial updates and expect different error responses than Razor form users. The clean solution is to map between a ViewModel or DTO and the entity in the controller or a mapping service — each ViewModel carries only the fields and validation rules relevant to its specific form. EF Core schema constraints should live in Fluent API (`modelBuilder.Entity<Product>().Property(p => p.Name).IsRequired().HasMaxLength(100)`) rather than as MVC annotations on the entity.

---

## Q15. What are `asp-validation-for` and `asp-validation-summary`?

**Concepts**
- `asp-validation-for` rendering a `<span>` with the field-level error message
- `asp-validation-summary` rendering a block of all or model-level errors
- Tag helpers emitting nothing when `ModelState` has no errors for the scope
- `_ValidationScriptsPartial` required for client-side message display
- Empty `<span>` injected by client scripts for immediate error visibility

**Answer**

`asp-validation-for="PropertyName"` renders a `<span>` populated with the field's error message from `ModelState` when the form is returned after a failed POST — the span is empty on GET or when the field has no error. `asp-validation-summary` renders a block of error messages for the configured scope: `ValidationSummary.All` shows every error in `ModelState`, including field-level ones; `ValidationSummary.ModelOnly` shows only errors with an empty member name (model-level errors added via `ModelState.AddModelError(string.Empty, message)`). Both tag helpers emit no HTML content when `ModelState` has no errors for their scope, so the page renders cleanly on first load. Including `_ValidationScriptsPartial` in the view loads jQuery Validate and the Unobtrusive adapter, which reads the `data-val-*` attributes emitted by `asp-for` and populates the same validation spans client-side before submit — the span elements are shared between client-side and server-side error display.

---

## Q16. What is the difference between `ValidationSummary` `ModelOnly` and `All`?

**Concepts**
- `ModelOnly` showing errors with empty member name (model-level)
- `All` showing every error including property-level ones
- Duplicate display when using `All` alongside `asp-validation-for` spans
- `ModelState.AddModelError(string.Empty, message)` targeting the summary
- Choosing scope based on whether per-field spans are present

**Answer**

`ValidationSummary.All` lists every error in `ModelState`, including property-level errors that may already appear next to their respective fields via `asp-validation-for` spans — using both on the same form produces duplicate error messages, one in the summary and one inline. `ValidationSummary.ModelOnly` shows only errors not tied to a specific property, which are added via `ModelState.AddModelError(string.Empty, message)` — this is the appropriate choice when each field already has a `asp-validation-for` span, since the summary then shows only cross-cutting or business rule errors ("This booking slot is no longer available.") without duplicating field messages. I use `ModelOnly` as the default on forms with per-field error spans and reserve `All` for compact forms like login pages that have no per-field spans and benefit from a single consolidated error list at the top.

---

## Q17. What is unobtrusive client validation?

**Concepts**
- `data-val-*` attributes emitted from annotation metadata by Tag Helpers
- jQuery Validate Unobtrusive reading attributes without inline JavaScript
- `_ValidationScriptsPartial` loading the required scripts
- Client rules mirroring server annotations where adapters exist
- `ClientValidationEnabled` view context flag for per-request disabling

**Answer**

Unobtrusive client validation is the approach where Tag Helpers with `asp-for` emit `data-val`, `data-val-required`, `data-val-range`, and similar attributes on form inputs based on the annotation metadata of the ViewModel property. jQuery Validate Unobtrusive reads these attributes at page load and registers the corresponding validation rules without any inline JavaScript in the Razor view — the view stays clean while the behavior is data-driven. Enabling it requires loading jQuery, then `jquery.validate.js`, then `jquery.validate.unobtrusive.js` — typically via `_ValidationScriptsPartial`. When enabled, the browser intercepts the submit, validates all fields against the registered rules, and shows inline errors next to the `asp-validation-for` spans without a round trip. Client adapters exist for most built-in annotations; custom `ValidationAttribute` subclasses require implementing `IClientModelValidator` to emit their own `data-val-*` attributes. `IValidatableObject` rules have no client adapter and always require a POST to surface errors.

---

## Q18. What is the difference between Data Annotations and FluentValidation?

**Concepts**
- Data Annotations as declarative attributes on the model type
- FluentValidation as `AbstractValidator<T>` classes registered in DI
- FluentValidation supporting conditional rules, async validation, and complex chains
- Data Annotations auto-driving unobtrusive client adapters
- FluentValidation requiring `AddFluentValidationAutoValidation` and extra client setup

**Answer**

Data Annotations are attributes declared directly on ViewModel properties, evaluated by the built-in MVC validation infrastructure with no extra registration. They are visible alongside the property declaration, auto-generate `data-val-*` attributes for unobtrusive client validation, and are sufficient for straightforward per-property rules. FluentValidation defines separate `AbstractValidator<T>` classes with a fluent rule-building API, registered in DI via `services.AddValidatorsFromAssembly(...)` and connected to the MVC pipeline with `AddFluentValidationAutoValidation()`. FluentValidation excels when validation logic is conditional ("when the order type is wholesale, quantity must be at least 100"), when rules reference injected services (async database checks in `MustAsync`), or when a large rule set would overwhelm the ViewModel with attributes. Both populate `ModelState` and integrate with `if (!ModelState.IsValid)`, so the controller pattern is identical. The gap is client-side: Data Annotations drive unobtrusive adapters automatically, while FluentValidation requires additional setup or manual `data-val-*` attributes for client-side parity. Testing FluentValidation validators in isolation is straightforward (`validator.TestValidate(vm)`), whereas testing annotation-based validation typically requires constructing a `ValidationContext` manually.

---

## Gotchas — Data Annotations & Validation (Interview Traps)

---

#### Gotcha 1. `[Required]` on non-nullable `bool` never failing

**Concepts**
- Non-nullable `bool` — `false` is a valid non-null value; `[Required]` always passes
- Unchecked checkbox — posts nothing; model binder sets `bool` to `false`
- `bool?` with `[Required]` — forces explicit selection by requiring non-null
- Hidden-field pattern — posts `false` for unchecked, ensuring deliberate intent

**Answer**

`[Required]` checks that a value is not null — for a non-nullable `bool`, even `false` satisfies that check. An unchecked consent checkbox posts nothing to the server and the model binder sets the `bool` property to `false`, which passes `[Required]` silently. The fix for mandatory consent is `bool? Agreed { get; set; }` with `[Required]` — now a null value (unchecked) fails validation. The hidden-field pattern (`<input type="hidden" name="Agreed" value="false" />` plus a checkbox posting `true`) is an alternative: the field always posts, so the binding always succeeds and the value reflects intent. Use `bool?` with `[Required]` for explicit consent checkboxes where unchecked must fail.

---

#### Gotcha 2. Client-side validation not wired because jQuery Validate scripts are missing or out of order

**Concepts**
- `jquery.validate.js` — core validation library, must load before unobtrusive
- `jquery.validate.unobtrusive.js` — ASP.NET Core bridge that reads `data-val-*` attributes
- Script load order — jQuery â†’ jquery.validate â†’ jquery.validate.unobtrusive
- Missing unobtrusive script — validation rules present in HTML but no client enforcement

**Answer**

ASP.NET Core MVC emits `data-val="true"` and `data-val-required` attributes on inputs, but client validation only fires when `jquery.validate.unobtrusive.js` is loaded and parses those attributes. If `jquery.validate.unobtrusive.js` is missing or loads before jQuery or before `jquery.validate.js`, the unobtrusive library fails to initialize silently — the HTML attributes are present, but no client validation fires and every POST proceeds to the server. The required script order is: jQuery, then jquery.validate, then jquery.validate.unobtrusive. Server-side validation still runs, but the UX degrades to a full page reload for every invalid submission, which is often mistaken for "validation not working."

---

#### Gotcha 3. Skipping `ModelState.IsValid` under the assumption that client validation is sufficient

**Concepts**
- Client validation — bypassable by direct POST, Postman, or disabled JavaScript
- `ModelState.IsValid` in MVC — not automatic; must be checked explicitly before any persist
- `[ApiController]` on `ControllerBase` — does auto-return 400 on invalid `ModelState`
- MVC `Controller` — does NOT auto-return 400; developer must add the guard manually

**Answer**

Unlike `[ApiController]` endpoints which automatically return 400 when `ModelState` is invalid, standard MVC controllers with `Controller` as the base class do not. If the POST action omits `if (!ModelState.IsValid) return View(model);`, invalid data reaches the service and the database even when client-side validation is enabled. An attacker using curl or Postman never runs browser JavaScript at all. The MVC server-side check is mandatory: gate every POST action with `ModelState.IsValid` before any persist, redirect, or side effect. Relying on `[ApiController]` behavior on a view-returning MVC controller is a common misconception that leads to silent validation bypass.

---

#### Gotcha 4. Custom `ValidationAttribute` not inheriting from `ValidationAttribute` base

**Concepts**
- `[AttributeUsage(AttributeTargets.Property)]` — controls where the attribute can be applied
- `ValidationAttribute.IsValid(object value)` — method to override for validation logic
- `IClientModelValidator` — interface for emitting `data-val-*` HTML attributes for client-side
- Missing base class — attribute applies but `ModelState` never populated with errors

**Answer**

A custom attribute that inherits from `Attribute` but not from `ValidationAttribute` can be applied to properties and compiles, but it is never invoked by the MVC validation pipeline. `ModelState` is populated only for attributes that inherit from `ValidationAttribute` or implement `IModelValidator`. The fix is `class NoFutureDateAttribute : ValidationAttribute` with `override bool IsValid(object? value)`. For client-side validation support, also implement `IClientModelValidator.AddValidation(ClientModelValidationContext)` to emit the `data-val-*` attributes that `jquery.validate.unobtrusive` reads. Without `IClientModelValidator`, the custom rule runs server-side only, which is acceptable but means a page reload for the error message.

---

#### Gotcha 5. `[Remote]` attribute validating on client but not enforced on POST

**Concepts**
- `[Remote("Action", "Controller")]` — fires AJAX GET check during client-side validation
- POST action — never invokes `[Remote]` endpoint; no server-side enforcement
- Uniqueness check example — email exists validation passes on AJAX but can still be bypassed
- Explicit server-side re-check — required in the POST action after `ModelState.IsValid`

**Answer**

`[Remote("CheckEmail", "Users")]` triggers an AJAX call to `CheckEmail` during client-side validation, which is useful for real-time feedback. However, the MVC validation pipeline does not call the Remote endpoint during server-side POST processing — `ModelState.IsValid` returns true even if the remote check would fail. An attacker submitting the POST directly bypasses the remote check entirely. Any uniqueness, availability, or cross-system validation that `[Remote]` covers must also be re-checked explicitly in the POST action after `ModelState.IsValid` passes. The `[Remote]` attribute improves UX; it is not a substitute for server-side business rule enforcement.

---

#### Gotcha 6. `[StringLength]` vs `[MaxLength]` — different enforcement points

**Concepts**
- `[StringLength(max)]` — validation annotation, enforced by `ModelState`; also emits client-side `data-val-length`
- `[MaxLength(max)]` — EF Core schema convention; does not add `ModelState` validation by default
- `[MaxLength]` without `[StringLength]` — database truncation or exception, no form validation error
- Both together — correct for forms where schema constraint and input validation must match

**Answer**

`[MaxLength(100)]` tells EF Core to generate a `VARCHAR(100)` column but does not add a `ModelState` validation error when a form submits a 200-character string — the string reaches the database and causes a truncation or `DbUpdateException`. `[StringLength(100)]` adds both the `ModelState` validation error and the client-side `data-val-length` attribute that jQuery Validate reads. For form-bound models, `[StringLength(100)]` is the correct annotation for user-facing length enforcement. `[MaxLength(100)]` is the correct annotation for EF Core schema generation. Applying only one means either the schema constraint or the UX validation is missing. Both annotations can coexist and serve their respective purposes.

---

#### Gotcha 7. `IValidatableObject` validation running even when individual property annotations have already failed

**Concepts**
- `IValidatableObject.Validate(context)` — always called after attribute validation, regardless of failures
- Cross-field validation — correct use case for `IValidatableObject`
- Null field access — a required field that failed binding may be null inside `Validate`
- Guard with `if (!validationContext.Members.Any())` — not reliable; check each field for null

**Answer**

`IValidatableObject.Validate` is called by the MVC validation pipeline even when property-level validation has already failed. This means a required property like `EndDate` may be null inside `Validate` if the user did not provide it — a `NullReferenceException` inside `Validate` will crash the validation pass instead of displaying the original required error. The fix is to guard each cross-field check: `if (StartDate == default || EndDate == default) yield break;` so the cross-field rule is only enforced when both fields have valid values. `IValidatableObject` is the correct place for "end date must be after start date" — not for replacing or duplicating attribute-level validation.

---

#### Gotcha 8. `[Compare]` attribute field name not matching the compared property name

**Concepts**
- `[Compare("Password")]` — string argument must match the property name exactly (case-sensitive)
- Renamed property — `Password` renamed to `NewPassword` requires updating the `Compare` argument
- Compile-time safety — no compiler check on the string argument
- `nameof(RegisterViewModel.Password)` — refactor-safe alternative to hardcoded string

**Answer**

`[Compare("Password")]` on a `ConfirmPassword` property compares the two fields during validation. The string argument `"Password"` must match the property name exactly and is case-sensitive on some platforms. If the `Password` property is renamed to `NewPassword` during refactoring, the `[Compare]` argument still says `"Password"` — the comparison silently fails to find the target property and the validation either passes (all values match null) or produces a confusing error. The refactor-safe approach is `[Compare(nameof(RegisterViewModel.Password))]` which is checked at compile time, so a rename produces a build error rather than a silent runtime mismatch.

---

#### Gotcha 9. Localized validation error messages requiring `ResourceType` configuration

**Concepts**
- `[Required(ErrorMessage = "Name is required")]` — hardcoded string, not localizable
- `[Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = "NameRequired")]` — resource-backed
- Missing `ResourceType` — resource name is treated as a literal string, not a key
- `IStringLocalizer` with `ValidationProblem` — alternative localization approach via DI

**Answer**

`[Required(ErrorMessage = "Name is required")]` embeds a hardcoded English string in the annotation. To localize validation messages, set `ErrorMessageResourceType = typeof(Messages)` (the resource class) and `ErrorMessageResourceName = "NameRequired"` (the resource key). Without `ErrorMessageResourceType`, `ErrorMessageResourceName` is treated as a literal string and displayed verbatim rather than looked up in a resource file. The resource class must have a public static property matching `ErrorMessageResourceName` or the resource lookup throws. An alternative approach is FluentValidation, which integrates with `IStringLocalizer<T>` using constructor injection and avoids attribute-based resource configuration entirely.

---

#### Gotcha 10. `ModelState` errors from API controller vs MVC controller behaving differently

**Concepts**
- `[ApiController]` + `ControllerBase` — automatically returns 400 `ProblemDetails` when `ModelState` invalid
- MVC `Controller` — does NOT auto-return 400; must check `ModelState.IsValid` manually
- `ConfigureApiBehaviorOptions.SuppressModelStateInvalidFilter` — disables auto-400 for custom handling
- Mixed project — same ViewModel with annotations, different validation behavior per controller type

**Answer**

`[ApiController]` on a `ControllerBase` subclass automatically returns a 400 response with `ValidationProblemDetails` when model binding produces an invalid `ModelState` — the action method body is never entered. MVC controllers inheriting `Controller` without `[ApiController]` do not share this behavior — the action runs regardless of `ModelState` validity, and the developer must gate it with `if (!ModelState.IsValid) return View(model);`. In a project with both MVC and API controllers using the same ViewModel, the same annotation-annotated ViewModel behaves differently: the API endpoint auto-validates and the MVC form does not. Missing this distinction is the source of "the API returns 400 but the form accepts invalid input" bugs when both endpoints share a controller class.

---
## Scenario-Based Questions (Karat Format)

---

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

**Concepts**
- Missing `ModelState.IsValid` guard before `_users.Create`
- `_ValidationScriptsPartial` enforcing validation only in the browser
- Direct POST bypassing JavaScript and reaching the action with invalid data
- MVC not auto-rejecting invalid models without the explicit guard

**Answer**

The action calls `_users.Create(model.Email, model.Password)` and immediately redirects, with no check on `ModelState.IsValid`. The `_ValidationScriptsPartial` loads jQuery Validate Unobtrusive for the GET form view, which prevents submission for invalid inputs in the browser — but disabling JavaScript (or POSTing directly with curl) skips that layer entirely and the action receives invalid data. `[EmailAddress]` and `[MinLength(8)]` ran during binding and recorded errors in `ModelState`, but the action never reads the result. `_users.Create` is called with an empty email or a 7-character password, and the invalid account is persisted.

The fix is to add the standard guard:

```csharp
[HttpPost]
public IActionResult Register(RegisterViewModel model)
{
    if (!ModelState.IsValid)
        return View(model);
    _users.Create(model.Email, model.Password);
    return RedirectToAction("Index", "Home");
}
```

`return View(model)` returns the form with the submitted values and `asp-validation-for` spans populated, so the user sees inline error messages. MVC controllers do not auto-reject invalid models — the guard is always an explicit requirement regardless of whether client scripts are present.

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

**Concepts**
- `asp-for` on `bool?` generating hidden-false companion
- Hidden `false` submitting regardless of checkbox state
- `[Required]` on `bool?` checking for null — `false` satisfies it
- `[Range(typeof(bool), "true", "true")]` as the correct must-be-true validation

**Answer**

`asp-for="AcceptTerms"` on a `bool?` property uses the checkbox tag helper, which renders a hidden `<input type="hidden" name="AcceptTerms" value="false" />` before the visible checkbox. When the checkbox is unchecked, the hidden `false` input submits, so the POST body contains `AcceptTerms=false`. `bool? AcceptTerms` binds to `false` — an explicit, non-null value. `[Required]` on `bool?` checks for null, not for `true`, so `false` satisfies it and validation passes. The user skips the terms and the order is placed.

There are two separate issues. First, the hidden-false companion converts the intended "null = not answered" semantics into an explicit `false` — unchecked always posts `false`, never `null`. Second, even if the companion were removed (so unchecked posted nothing and `AcceptTerms` bound to `null`, failing `[Required]`), a crafted POST with `AcceptTerms=false` would still bypass the check. The correct fix is:

```csharp
[Required]
[Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the terms.")]
public bool? AcceptTerms { get; set; }
```

`[Range(typeof(bool), "true", "true")]` rejects both `null` (caught by `[Required]`) and explicit `false` (caught by `[Range]`). For the hidden companion problem, write the checkbox manually without `asp-for` if tri-state is needed, or accept that `asp-for` on `bool?` always posts `false` for unchecked and rely on `[Range]` to reject it.

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

**Concepts**
- `[Compare]` string literal not updated after property rename
- Silent server-side pass when named property is not found by `[Compare]`
- Client-side `data-val-equalto-other` referencing old field id
- Production code likely still has `[Compare("Password")]` not `[Compare("NewPassword")]`
- Pen tester POST as URL-encoded form data bypassing the non-updated compare

**Answer**

The code shown has `[Compare("NewPassword")]` — that is the correct attribute argument after the rename. The scenario description says the refactor happened but the bug persists, which means the actual production code still has `[Compare("Password")]` from before the rename. When `[Compare("Password")]` is evaluated and there is no property named `Password` on `ChangePasswordViewModel`, the framework cannot find the comparison target and returns `ValidationResult.Success` rather than throwing — the validation passes silently regardless of what `ConfirmPassword` contains.

The "only in Chrome" client behavior is the same root cause: the unobtrusive adapter emitted `data-val-equalto-other="*.Password"` based on the old attribute argument, and the `#Password` field id no longer exists. jQuery Validate fails to find the target element and skips the comparison rule on most browsers, while Chrome may apply its own native form heuristics that approximate an equality check.

The fix is to update the attribute in production to `[Compare("NewPassword")]` as already shown in the review code. For the pen tester POST: the pen tester is sending JSON with `Content-Type: application/json` directly to a form action that has no `[FromBody]`. Without `[FromBody]`, the JSON body is not parsed by the form value provider — `NewPassword` and `ConfirmPassword` both bind to their initialized empty strings. `[Required]` on `""` fails, so `ModelState.IsValid` is false and the action returns `View(model)`. The pen tester cannot use a JSON POST to bypass this action — only a URL-encoded form POST with `NewPassword=x&ConfirmPassword=y` would reach the `[Compare]` check.

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

**Concepts**
- Property-level annotation attributes running before `IValidatableObject.Validate`
- `Validate` only called when all property attributes pass
- `ValidationResult` member names mapping to `asp-validation-for` spans
- Model-level (empty member name) errors appearing in summary only
- Custom `ValidationAttribute` running at property-level with single property context

**Answer**

The MVC validation pipeline runs in this order: first, all property-level annotation attributes (`[Required]`, `[Range]`, `[StringLength]`, etc.) are evaluated for each property; then, if all property attributes pass, `IValidatableObject.Validate` is called with the whole object in scope. The key consequence is that `Validate` only runs when individual properties have no errors — if `StartDate` fails a binding error, `Validate` is not called, so `EndDate >= StartDate` is never checked in that case.

For `IValidatableObject`, target errors at specific properties by including their names in the `ValidationResult`:

```csharp
public IEnumerable<ValidationResult> Validate(ValidationContext ctx)
{
    if (EndDate < StartDate)
        yield return new ValidationResult("End must be after start.", new[] { nameof(EndDate) });

    if (GuestCount > 0 && string.IsNullOrWhiteSpace(PrimaryGuestName))
        yield return new ValidationResult("Provide at least one guest name.", new[] { nameof(PrimaryGuestName) });
}
```

`nameof(EndDate)` in the member names list binds the error to `asp-validation-for="EndDate"` in the view, so the message appears next to the `EndDate` input. An empty member name (`new string[0]`) would make the error appear only in `asp-validation-summary`.

A custom `ValidationAttribute` on `EndDate` would run at property level (before `IValidatableObject`) but can only see one property unless it accesses `ValidationContext.ObjectInstance` to cast to `BookingViewModel` and read `StartDate`. For single-property rules, attributes are cleaner and reusable; for rules that require multiple properties simultaneously — as both rules here do — `IValidatableObject` is the natural fit because it receives the whole object with all bound values available. The trade-off is that `IValidatableObject` is server-only: the browser never sees these errors before a POST, since there is no client adapter for the `Validate` method.

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

**Concepts**
- `(int)value` hard cast throwing `InvalidCastException` when value is `decimal`
- Unboxing rules requiring exact type match — `decimal` cannot be unboxed to `int`
- Unit tests using `int` literals (0, 50) not exercising the `decimal` path
- `Convert.ToDecimal(value)` or pattern matching as the safe conversion
- Exception in attribute converted to server error, not a validation message

**Answer**

`(int)value` is an invalid unboxing cast when `value` is boxed as `decimal`. C# unboxing requires the exact CLR type — `(int)(object)(decimal)12.5m` throws `InvalidCastException` at runtime, not a graceful validation failure. The unit tests pass because they use integer literals: `new DiscountPercentAttribute().IsValid(50, ctx)` boxes an `int` and the cast succeeds. When the MVC form posts `DiscountPercent=12.5`, model binding creates a `decimal` property, and the validation pipeline passes the boxed `decimal` as `value` — triggering the `InvalidCastException`. Depending on the MVC version, this either surfaces as a server 500 error or is swallowed and treated as a validation failure with a generic message.

The fix is to convert safely:

```csharp
protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
{
    if (value is null) return ValidationResult.Success;
    decimal n = Convert.ToDecimal(value);
    if (n < 0 || n > 100) return new ValidationResult(ErrorMessage ?? "Invalid discount.");
    return ValidationResult.Success;
}
```

`Convert.ToDecimal` handles `int`, `long`, `double`, and `decimal` inputs correctly. Alternatively, `value is decimal d` pattern matching is type-safe and idiomatic. The broader lesson is that custom `ValidationAttribute` implementations should never assume the boxed type of `value` matches a specific primitive — use `Convert` or pattern matching, and test with the actual property type used in the ViewModel.

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

**Concepts**
- Single annotation set forced onto all contexts (MVC form, API, import)
- `RowVersion` exposed to mass assignment and rendering
- Navigation properties triggering validation of related graphs
- UI and API validation rules diverging over time with no separation point
- EF Fluent API vs data annotations for schema constraints

**Answer**

Three categories of problems emerge from this architecture.

**Over-posting and data leakage.** `RowVersion` is a concurrency token managed by EF — it should never appear in a form input or be settable from an API request body. Because `Product` is bound directly as the action parameter, `RowVersion` is exposed to model binding and can be set by a crafted POST. Conversely, if `RowVersion` is needed for optimistic concurrency in the update action, it must travel as a hidden form field, which means it is embedded in every rendered page and visible in source.

**One-size-fits-all validation.** `[Range(0, 99999)]` and `[StringLength(100)]` are locked onto the entity. A bulk import endpoint accepting prices up to 1,000,000 cannot relax that constraint without modifying the entity annotation, which also relaxes MVC form validation. An admin-only form that allows longer product names cannot extend the limit either. The annotations encode a single view of validity that must serve all callers.

**Navigation property validation.** If `Product` later gains a `Category` navigation property, `[Required]` on `Category` triggers navigation-graph validation during MVC binding, potentially requiring `Category` to be eagerly loaded for every form submission or causing spurious `ModelState` errors when only the flat product fields were submitted.

**Testing.** Unit tests for MVC-specific business rules cannot target just the ViewModel layer — they must reason about the entity class and its persistence concerns at the same time.

The correct approach is to keep EF schema constraints in Fluent API (`modelBuilder.Entity<Product>().Property(p => p.Name).IsRequired().HasMaxLength(100)`) and define separate `ProductCreateViewModel` and `ProductEditViewModel` types with annotations tailored to their forms. Mapping between the ViewModel and entity in the controller keeps the two models decoupled and allows each to evolve independently.

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

**Concepts**
- `$.validator.methods.date` override disabling client date format checking entirely
- Server `en-US` culture rejecting `dd/MM/yyyy` input
- `[DisplayFormat]` shaping display only — not changing server-side parsing culture
- `RequestLocalization` middleware or `<input type="date">` as fixes
- Client and server validation diverged by the override

**Answer**

Two problems combine here. The JavaScript override `$.validator.methods.date = function () { return true; }` disables all client-side date format validation — jQuery Validate accepts any string in the date field, so the browser never blocks `31/12/2026` or `not-a-date` or an empty string from submitting. The user sees no client error, which creates the impression that the value is valid.

On the server, `EventDate` is parsed using `CultureInfo.CurrentCulture`, which is `en-US` — expecting `M/d/yyyy`. `31/12/2026` fails because `31` is not a valid month. `ModelState` receives a type conversion error and the action returns the form with "The value '31/12/2026' is not valid for EventDate." The `[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]` controls how the date is formatted for display, not how it is parsed during model binding, so it does nothing to align the parsing culture.

The override was likely added to silence a noisy jQuery Validate `date` method that was rejecting the `dd/MM/yyyy` format the business uses. The fixes are: remove the override, add `RequestLocalization` middleware with `en-GB` as a supported culture with an `Accept-Language` or cookie-based culture provider, and configure the server culture to match the user locale. Alternatively, switch to `<input type="date">` which posts ISO `yyyy-MM-dd` format regardless of browser locale, making the server culture irrelevant for date input parsing.

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

**Concepts**
- `[Remote]` firing on every keystroke — unbounded DB queries
- `CheckUsername` callable without auth — username enumeration endpoint
- Rate limiting per IP to prevent automated bulk enumeration
- Response caching or in-process cache to reduce DB round trips
- Requiring CSRF token or auth on the remote endpoint

**Answer**

`CheckUsername` is called by the browser's jQuery Validate adapter on every input event (or debounced keystroke) as the user types. With no rate limiting, each character typed by a legitimate user triggers a database query, and a bot can call the endpoint in a tight loop to enumerate valid usernames at thousands of requests per second — both causing the DB CPU spike and the enumeration risk.

The endpoint has two missing protections. First, rate limiting: add `[EnableRateLimiting("CheckUsername")]` with a token-bucket or fixed-window policy of a few requests per second per IP using `RateLimiterMiddleware` in ASP.NET Core 7+. Second, the endpoint is publicly accessible without authentication. An unauthenticated user calling `/Account/CheckUsername?username=alice` learns whether `alice` is a registered user — valuable for phishing and credential stuffing preparation. Add `[Authorize]` if only authenticated users should check (pre-registration flows need a different approach), or at minimum add an antiforgery token requirement.

On the performance side, an in-memory or distributed cache keyed on the lowercased username can serve repeated checks for the same name without hitting the database: on the first check, query the DB and cache the result for 60 seconds. This also limits enumeration throughput. Finally, the `CheckUsername` endpoint uses `_db.Users.Any(u => u.Username == username)` — ensure `Username` is indexed in the database so the query does not perform a full table scan under load.

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

**Concepts**
- `[ValidateNever]` suppressing validation on a property not submitted by the user
- Password hash round-trip via hidden input as a security risk
- Server-side reload of sensitive fields from the database on POST
- `[BindNever]` pairing for properties that should neither bind nor validate
- `NewPassword` remaining validated since it IS user-submitted

**Answer**

The "PasswordHash field is required" error occurs because `PasswordHash` is not a nullable type — even without `[Required]`, MVC's implicit required behavior for non-nullable reference types with NRT enabled can produce a model error when the hidden field is absent or empty. `[ValidateNever]` on `PasswordHash` tells the validation pipeline to skip this property entirely, removing the spurious error:

```csharp
[ValidateNever]
[BindNever]
public string PasswordHash { get; set; } = "";
```

Pairing with `[BindNever]` ensures the property is also not read from the POST body — the controller must populate it from the database before returning the view on GET and ignore any incoming value on POST. `[BindNever]` plus `[ValidateNever]` together isolate the property from all inbound processing.

The deeper concern is that `PasswordHash` should not be in the form at all. A hidden input for the password hash embeds a bcrypt hash (or equivalent) in the rendered HTML, which is visible in source and transmitted in every form POST. An attacker who intercepts the hash can potentially use it to craft authentication bypass attempts. The correct design is to reload the hash from the database on the POST action rather than round-tripping it through the form — the server already has the original hash in the database, so there is no need to send it to the browser and back. Remove `PasswordHash` from the ViewModel entirely; load it inside the action when needed for comparison or re-encryption. `NewPassword` keeps `[Required][MinLength(8)]` because it IS user-submitted and those rules protect the password quality directly.

---

#### Q10. (D) Greenfield MVC app: team debates Data Annotations + unobtrusive client validation vs FluentValidation (`AddFluentValidationAutoValidation`). Compare maintainability, client-side parity, testing, and what happens on a direct POST with JavaScript disabled.

**Concepts**
- Data Annotations auto-generating `data-val-*` client adapters
- FluentValidation in separate `AbstractValidator<T>` classes testable in isolation
- FluentValidation requiring explicit client-side setup for parity
- Both populating `ModelState` — same action guard pattern
- Complex conditional or async rules favoring FluentValidation

**Answer**

Both approaches populate `ModelState` and integrate with `if (!ModelState.IsValid) return View(model);`, so the controller pattern and the behavior on direct POST (with or without JavaScript) are identical — server validation runs regardless.

**Data Annotations.** Rules live on the ViewModel alongside properties, which makes them visible at a glance. The framework automatically emits `data-val-*` attributes for built-in rules, so client-side validation works out of the box with `_ValidationScriptsPartial`. For simple per-property constraints (`[Required]`, `[EmailAddress]`, `[StringLength]`, `[Range]`), annotations are the fastest path. Maintainability degrades when ViewModels accumulate many attributes or when the same validation logic must be expressed differently for different contexts — annotations cannot be scoped per action or conditionally applied without custom attributes.

**FluentValidation.** Rules live in `AbstractValidator<ProductEditViewModel>` classes that are easy to unit-test with `validator.TestValidate(vm)` and `result.ShouldHaveValidationErrorFor(...)`. Complex conditional chains (`When(m => m.IsWholesale, () => RuleFor(m => m.Quantity).GreaterThanOrEqualTo(100))`), async uniqueness checks (`MustAsync`), and DI-injected services in validators are natural in FluentValidation and awkward in annotations. The gap is client-side: FluentValidation does not generate `data-val-*` attributes automatically, so client validation either requires manual attributes on ViewModels alongside the validator rules, or the team accepts that client validation is missing until the round trip. `AddFluentValidationAutoValidation` handles the server-side integration; client parity requires additional setup (FluentValidation's AspNetCore package has partial support for some rules).

**My recommendation.** Start with Data Annotations for straightforward ViewModels. Introduce FluentValidation for any ViewModel where cross-property rules, conditional logic, or async checks are needed, or where you want validator logic in isolated, testable classes. The two approaches coexist without conflict.

---

#### Q11. (M) Walk through the server-side validation order for an MVC POST to `[HttpPost] Create(ProductCreateViewModel model)` when the action has `[ValidateAntiForgeryToken]`, the ViewModel implements `IValidatableObject`, and one property has a custom `ValidationAttribute`. When does `ModelState.IsValid` become false, and when should the action short-circuit?

**Concepts**
- Antiforgery filter running before action execution and model binding
- Model binding and value provider sourcing form data
- Property-level annotation and custom attribute running per property
- `IValidatableObject.Validate` running only after all property attributes pass
- `ModelState.IsValid` reflecting the accumulated result at action entry

**Answer**

The pipeline for a POST action decorated with `[ValidateAntiForgeryToken]` on an MVC controller proceeds in this order:

**Step 1 — Antiforgery filter.** `[ValidateAntiForgeryToken]` runs as an authorization filter before any model binding occurs. If the `__RequestVerificationToken` is missing or invalid, an `AntiforgeryValidationException` is thrown and the request is rejected with 400 before the action body is reached. Model binding never runs.

**Step 2 — Model binding.** The model binder reads form fields, query strings, and route values via value providers and constructs a `ProductCreateViewModel` instance. Type conversion failures (a non-numeric string for a `decimal` property, an unparseable date) add binding errors to `ModelState` keyed by property name. `ModelState.IsValid` becomes false if any binding error occurs here.

**Step 3 — Property-level validation.** After binding, the validation pipeline evaluates all `[Required]`, `[StringLength]`, `[Range]`, and custom `ValidationAttribute` subclasses for each property. The custom attribute's `IsValid` method receives the bound value. All property attributes run regardless of order; errors accumulate in `ModelState`. `ModelState.IsValid` becomes false if any attribute returns a non-Success result.

**Step 4 — `IValidatableObject.Validate`.** Only if all property-level validation passes (no errors from steps 2 or 3) does the framework call `Validate(ValidationContext)`. Cross-property rules run here with access to all bound property values. Returned `ValidationResult` objects with member names add keyed errors to `ModelState`; results with empty member names add model-level errors. `ModelState.IsValid` becomes false if `Validate` yields any results.

**Step 5 — Action body.** By the time `Create` executes, `ModelState` reflects all accumulated errors. The action should short-circuit at the very first line: `if (!ModelState.IsValid) return View(model);`. Calling any service or database write before this check risks persisting invalid data — the antiforgery check guarantees the request originated from the rendered form, but it says nothing about data validity.

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

**Concepts**
- `Controller` base class exception handling returning HTML error pages
- Unhandled exceptions in `_orders.Add` routed to Razor error view — 200 HTML
- `BadRequest(ModelState)` returning non-`ProblemDetails` format without `[ApiController]`
- MVC exception filter overriding `BadRequest` with HTML when error page is configured
- Splitting into `[ApiController]` controller as the clean fix

**Answer**

The `ModelState.IsValid` check and `BadRequest(ModelState)` call look correct. The "200 HTML" symptom is not coming from the validation path — it is coming from unhandled exceptions downstream in `_orders.Add`. When `_orders.Add` throws (a concurrency exception, a null reference, a database timeout), the MVC exception filter catches it and renders the configured error view, typically `Views/Shared/Error.cshtml`. That error view is served with a 200 status code if the exception handling pipeline returns an HTML response without setting a non-success status code, which is a common misconfiguration.

The underlying architectural issue is that `OrdersController` derives from `Controller` and the MVC exception pipeline produces HTML for unhandled exceptions — appropriate for Razor view actions, wrong for JSON API endpoints. Without `[ApiController]`, there is no automatic `ProblemDetails` response for exceptions, and `BadRequest(ModelState)` returns a plain ModelState dictionary serialized as JSON rather than a RFC 7807 ProblemDetails object, making error handling inconsistent for the AJAX client.

The clean fix is to separate the JSON endpoint into its own controller:

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersApiController : ControllerBase
{
    [HttpPost("quickadd")]
    public IActionResult QuickAdd(QuickAddOrderViewModel model)
    {
        _orders.Add(model);
        return Ok(new { id = model.Sku });
    }
}
```

`[ApiController]` auto-validates `ModelState` and returns `ValidationProblemDetails` on failure, exception handling produces JSON `ProblemDetails`, and the `ModelState.IsValid` guard becomes implicit. The Razor view actions remain in `OrdersController : Controller` with their HTML error page behavior unchanged.
