# Model Binding in MVC — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is model binding in ASP.NET Core MVC?](#q1-what-is-model-binding-in-aspnet-core-mvc)
2. [Q2. How does model binding work for HTML form POSTs?](#q2-how-does-model-binding-work-for-html-form-posts)
3. [Q3. What is the difference between `[FromForm]` and `[FromBody]` in MVC?](#q3-what-is-the-difference-between-fromform-and-frombody-in-mvc)
4. [Q4. Why does `[FromBody]` fail when posting a standard HTML form?](#q4-why-does-frombody-fail-when-posting-a-standard-html-form)
5. [Q5. How are collection properties bound from form fields (`Lines[0].Sku`)?](#q5-how-are-collection-properties-bound-from-form-fields-lines0sku)
6. [Q6. What happens when collection indices are non-contiguous after deleting a row?](#q6-what-happens-when-collection-indices-are-non-contiguous-after-deleting-a-row)
7. [Q7. How does model binding handle nested objects (`Address.City`)?](#q7-how-does-model-binding-handle-nested-objects-addresscity)
8. [Q8. How do partial views affect model binding prefix for nested properties?](#q8-how-do-partial-views-affect-model-binding-prefix-for-nested-properties)
9. [Q9. What happens when optional nullable fields (`int?`, `DateTime?`) are left empty?](#q9-what-happens-when-optional-nullable-fields-int-datetime-are-left-empty)
10. [Q10. How does culture affect date and number binding from form fields?](#q10-how-does-culture-affect-date-and-number-binding-from-form-fields)
11. [Q11. What is `RequestLocalization` and how does it relate to model binding?](#q11-what-is-requestlocalization-and-how-does-it-relate-to-model-binding)
12. [Q12. How does file upload binding work (`IFormFile`)?](#q12-how-does-file-upload-binding-work-iformfile)
13. [Q13. What `enctype` is required for file upload forms?](#q13-what-enctype-is-required-for-file-upload-forms)
14. [Q14. What is over-posting during model binding and how is it prevented?](#q14-what-is-over-posting-during-model-binding-and-how-is-it-prevented)
15. [Q15. What are `[BindNever]` and `[Bind]` used for?](#q15-what-are-bindnever-and-bind-used-for)
16. [Q16. How do checkboxes bind to `bool` and `bool?` properties?](#q16-how-do-checkboxes-bind-to-bool-and-bool-properties)
17. [Q17. What is the hidden-field pattern for checkboxes?](#q17-what-is-the-hidden-field-pattern-for-checkboxes)
18. [Q18. What is a custom `IModelBinder` and when would you create one?](#q18-what-is-a-custom-imodelbinder-and-when-would-you-create-one)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What is model binding in ASP.NET Core MVC?

**Concepts**
- HTTP request data mapped onto action method parameters
- Value providers supplying key/value pairs from form, route, and query
- Model binders converting strings to CLR types recursively
- Validation running during binding via annotations and `IValidatableObject`
- Default binding source for MVC: form + route + query (not JSON body)

**Answer**

Model binding is the framework mechanism that reads HTTP request data — form fields, query strings, route values, headers, and cookies — and maps it onto action method parameters and their nested properties before the action executes. Value providers supply key/value pairs from each source; model binders then convert those string values to CLR types and set properties recursively on complex objects. Validation attributes like `[Required]` and `[Range]` run as part of this same pipeline, accumulating results in `ModelState` so the action can check `ModelState.IsValid` before proceeding. The default binding source for conventional MVC controllers covers form fields, route values, and query strings — JSON body binding requires an explicit `[FromBody]` attribute because the JSON input formatter is not in the default source set. Custom binders implement `IModelBinder` for types the built-in binders cannot handle, such as composite keys or culture-specific currency formats.

---

## Q2. How does model binding work for HTML form POSTs?

**Concepts**
- `application/x-www-form-urlencoded` key/value pairs read by form value provider
- Complex object binder matching key names to property paths
- `asp-for` tag helper generating correct `name` attributes automatically
- Route and query values participating unless restricted with `[FromForm]`
- `ModelState.IsValid` required before persisting in MVC (unlike `[ApiController]`)

**Answer**

Browser forms POST `application/x-www-form-urlencoded` or `multipart/form-data` key/value pairs, and the form value provider reads these keys from the request body. The complex object model binder then matches each key to a property path on the action parameter type — `ProductName` → `model.ProductName`, `Address.City` → `model.Address.City` — instantiating nested types as needed when values are present. Tag helpers with `asp-for` generate `name` attributes that exactly match the expected property paths, so there is no manual naming work when using ViewModels correctly. Route values and query string parameters also participate in binding alongside form fields unless the parameter is decorated with `[FromForm]` to restrict it to form data only. After binding completes, MVC does not automatically reject invalid models the way `[ApiController]` does, so checking `ModelState.IsValid` before calling any service or saving to the database is a required explicit step in every POST action.

---

## Q3. What is the difference between `[FromForm]` and `[FromBody]` in MVC?

**Concepts**
- `[FromForm]` binding from form fields, route, and query string
- `[FromBody]` binding from the HTTP body via JSON input formatter
- Combining `[FromRoute] int id` with `[FromForm] MyViewModel model`
- Incorrect mixing producing empty models without obvious errors
- MVC Razor forms using default binding or `[FromForm]`, not `[FromBody]`

**Answer**

`[FromForm]` binds from form fields, query strings, and route values — the standard HTML form path where the browser encodes data as key/value pairs. `[FromBody]` binds from the HTTP request body using input formatters, typically JSON, and requires `Content-Type: application/json` from the client. They read fundamentally different parts of the request using different parsers, so using the wrong one produces an empty model with no obvious error — the action runs with all properties at default values. MVC Razor form POSTs should use default binding (no attribute) or `[FromForm]`; `[FromBody]` is reserved for AJAX or API endpoints where the JavaScript client explicitly sends JSON. A single action can combine source attributes: `[FromRoute] int id` paired with `[FromForm] MyViewModel model` is a common and valid pattern for edit actions where the id comes from the URL path and the form data comes from the POST body.

---

## Q4. Why does `[FromBody]` fail when posting a standard HTML form?

**Concepts**
- HTML form sending `application/x-www-form-urlencoded`, not JSON
- `[FromBody]` JSON input formatter bypassing form value provider
- `ModelState` appearing valid despite empty model (no conversion error)
- Symmetric trap: posting JSON without `[FromBody]` also yields empty model
- Fix: remove `[FromBody]` for form POSTs; JavaScript must explicitly send JSON for API path

**Answer**

Standard HTML forms submit their data as `application/x-www-form-urlencoded` or `multipart/form-data` in the request body — they never send JSON unless JavaScript intercepts the submit and transforms the payload. When `[FromBody]` is on the action parameter, the framework bypasses the form value provider entirely and invokes the JSON input formatter, which finds no JSON body and leaves every property at its default value. The action executes successfully with an empty model because no conversion errors were produced — `ModelState` may appear clean, properties just quietly stayed at their defaults, and the resulting inserts save empty strings and zero values. The fix is to remove `[FromBody]` for conventional form POSTs and let the default binding source set handle the form encoding. The symmetric trap is worth noting: posting JSON to an action that lacks `[FromBody]` also produces an empty model, since the JSON body is ignored by the form value provider.

---

## Q5. How are collection properties bound from form fields (`Lines[0].Sku`)?

**Concepts**
- Indexed field names `Lines[0].Sku`, `Lines[1].Sku` populating `List<T>`
- Tag helpers and editor templates generating indexed names automatically
- Contiguous-zero-based indices required for reliable binding
- Dictionary binding with `Lines[SKU-123].Qty` syntax
- Server-side filtering of empty template rows after binding

**Answer**

Collection binding uses indexed field names to populate `List<T>` or array properties: `Lines[0].Sku`, `Lines[0].Qty`, `Lines[1].Sku`, `Lines[1].Qty` — the binder creates list entries at each index and sets nested properties on each element. Tag helpers with `asp-for` inside editor templates generate these indexed names automatically when iterating a collection model, so developers rarely need to write them by hand. The critical constraint is that indices must be contiguous starting at zero — the binder stops at the first missing index, so `Lines[0]` and `Lines[2]` with nothing at index 1 produces a list with a phantom null/default entry at position 1 and data at position 2. Dictionary properties use similar syntax with string keys: `Lines[SKU-123].Qty`. If the UI allows adding template rows (blank row stubs), the server action should filter empty rows — `model.Lines.Where(l => !string.IsNullOrEmpty(l.Sku))` — before processing, since those stubs submit empty values that become default objects after binding.

---

## Q6. What happens when collection indices are non-contiguous after deleting a row?

**Concepts**
- First-gap truncation: binder stops or inserts defaults at missing index
- Phantom null/default entries at gap positions
- SKU/quantity misalignment from shifted indices
- JavaScript re-indexing after row deletion as the fix
- Server-side null-row filtering as a secondary safety net

**Answer**

When a user deletes row index 1 from a dynamic form — leaving `Lines[0]` and `Lines[2]` without `Lines[1]` — the model binder inserts a default `LineItemViewModel` (with empty `Sku` and zero `Qty`) at position 1 and binds the real data to position 2. The resulting list has three entries: two with real data and one phantom at the gap. Server logic iterating all entries may process the phantom row (saving a blank line), or in some binder configurations the list may truncate at the first gap, depending on whether the binder treats missing indices as explicit null entries or as end-of-collection signals. Either way, SKUs and quantities can be misaligned with their original rows, corrupting the intended update. The reliable fix is to re-index rows in JavaScript immediately after any deletion — update all `name` attributes so they are `Lines[0]`, `Lines[1]`, `Lines[2]` without gaps before the form submits. A server-side filter on empty `Sku` values discards phantom rows as a safety net even if client-side re-indexing is buggy.

---

## Q7. How does model binding handle nested objects (`Address.City`)?

**Concepts**
- Dotted property path `Address.City` in field names
- Complex object binder instantiating nested types when values present
- `asp-for="Address.City"` emitting correct prefixed names
- Partial view prefix preservation for nested rendering
- Unprefixed `name="City"` in partial binding to wrong top-level property

**Answer**

Nested object binding uses dotted property paths in form field names: `Address.City`, `Address.PostalCode`, `Address.CountryCode`. The complex object binder instantiates the `Address` property on the parent ViewModel when it finds any keys with the `Address.` prefix and then sets each sub-property. Tag helpers with `asp-for="Address.City"` emit the correct prefixed names automatically, so developers using editor templates and `asp-for` for nested objects get correct binding without manual name management. The problem appears in partial views rendered with the nested object as their `@model` — if the partial uses plain `asp-for="City"` (without the parent prefix), the binder sees `name="City"` in the POST body and looks for a top-level `City` property on the parent ViewModel, finds none, and leaves `Address.City` null. The fix is to use `<partial name="_AddressEditor" for="Model.Address" />` or `Html.EditorFor(m => m.Address)`, both of which maintain the `Address.` prefix context for all fields inside the partial.

---

## Q8. How do partial views affect model binding prefix for nested properties?

**Concepts**
- Partial view `@model` receiving only the subtype without parent prefix context
- `name="City"` instead of `name="Address.City"` causing binding miss
- `<partial for="Model.Address" />` preserving prefix automatically
- `ViewData.TemplateInfo.HtmlFieldPrefix` for manual prefix control
- Silent null result when prefix is lost — a common checkout form bug

**Answer**

When a partial view is rendered with `@await Html.PartialAsync("_AddressEditor", Model.Address)`, the partial receives the `AddressModel` as its `@model` but inherits no knowledge of where that object sits in the parent's property graph. Tag helpers inside the partial generate `name="City"` rather than `name="Address.City"`, so the model binder looks for a top-level `City` on the parent ViewModel, finds nothing, and leaves `model.Address.City` at its default null. This is a silent failure — no exception, no `ModelState` error, the nested object simply stays empty. The fix is to use `<partial name="_AddressEditor" for="Model.Address" />` (the `for` attribute syntax sets the prefix from the expression) or `@Html.EditorFor(m => m.Address)` which sets `ViewData.TemplateInfo.HtmlFieldPrefix = "Address"` automatically. When neither option is available, setting `ViewData.TemplateInfo.HtmlFieldPrefix = "Address"` manually before rendering the partial achieves the same result.

---

## Q9. What happens when optional nullable fields (`int?`, `DateTime?`) are left empty?

**Concepts**
- Empty text input for nullable value type binding to `null`
- `null` distinguishing "not provided" from explicit zero or minimum date
- Non-nullable types binding empty input to `0` or `DateTime.MinValue` or adding conversion error
- `<input type="date">` submitting empty string when cleared
- Separate `ModelState` conversion error handling for required non-nullable fields

**Answer**

When a user clears an optional field and submits the form, the browser posts an empty string for that key (or omits the key entirely). For nullable value types like `int?` and `DateTime?`, an empty string or missing key binds to `null` — the intended signal for "no value provided." This correctly distinguishes between "the user cleared the field" and "the user entered zero" or "the user entered the minimum date." Non-nullable `int` and `DateTime` behave differently: for non-nullable `int`, an empty string adds a `ModelState` conversion error and the property stays at its initialized value (often zero); for non-nullable `DateTime`, the same happens and the property stays at `DateTime.MinValue` (0001-01-01). Using nullable types for optional form fields is therefore the correct design — it aligns the CLR type's null semantics with the HTML form's "absent field" semantics. When checking `ModelState.IsValid` and returning errors, the nullable approach also produces cleaner validation messages since there is no spurious type-conversion error for a legitimately cleared optional field.

---

## Q10. How does culture affect date and number binding from form fields?

**Concepts**
- Model binding using `CultureInfo.CurrentCulture` to parse strings
- Date format mismatch between user locale and server culture
- `<input type="date">` posting ISO `yyyy-MM-dd` as culture-invariant alternative
- Decimal separator differences (`1,5` vs `1.5`) across locales
- `RequestLocalization` middleware setting per-request culture

**Answer**

Model binding parses date and number strings using `CultureInfo.CurrentCulture`, so `31/01/2026` (day-first UK format) fails on a server with `en-US` culture expecting `1/31/2026`, and `1,234.56` (European decimal comma) fails on a server expecting `1234.56` with a dot. Users see "The value '31/01/2026' is not valid for EventDate" — a technically accurate but user-hostile error that comes entirely from a culture mismatch, not from invalid input. The cleanest resolution for dates is `<input type="date">` which posts ISO 8601 format (`yyyy-MM-dd`) regardless of the browser's display locale — the date picker shows in the user's format but submits in the invariant format the server can always parse. For number inputs on multi-locale apps, `RequestLocalization` middleware must be configured with explicit `SupportedCultures` and appropriate providers so the server culture matches what the user typed. JSON APIs with ISO 8601 dates and invariant numbers sidestep these issues entirely, but traditional MVC text boxes do not.

---

## Q11. What is `RequestLocalization` and how does it relate to model binding?

**Concepts**
- `RequestLocalization` middleware establishing per-request `CultureInfo`
- Culture providers: query string, cookie, `Accept-Language` header
- Model binders and validation using `CurrentCulture` for string parsing
- Cloud deployments defaulting to `en-US` without explicit configuration
- Display formatting and input parsing sharing the same culture

**Answer**

`RequestLocalization` middleware runs early in the pipeline and sets `CultureInfo.CurrentCulture` and `CurrentUICulture` for each request from configured providers. Model binders use `CurrentCulture` when parsing dates, numbers, and currency from form fields, so the per-request culture directly controls whether `31/01/2026` or `1.234,56` is parsed correctly. Common providers check a `culture` query string parameter, a culture cookie set by a language switcher, and the `Accept-Language` request header in that order. Without `RequestLocalization`, the server inherits whatever culture the host OS uses — typically `en-US` in cloud Linux containers regardless of where users connect from. Configuring it requires registering supported cultures and connecting the middleware: `app.UseRequestLocalization(options => { options.SupportedCultures = new[] { new CultureInfo("en-GB"), new CultureInfo("en-US") }; })`. The benefit is that both display formatting in views and input parsing in model binding share the same negotiated culture, so numbers and dates round-trip consistently between GET and POST.

---

## Q12. How does file upload binding work (`IFormFile`)?

**Concepts**
- `IFormFile` binding from file inputs in multipart form data
- `enctype="multipart/form-data"` required on the form
- `IFormFileCollection` or `List<IFormFile>` for multiple files
- Server-side validation of size, extension, and content type
- `IFormFile` null when no file selected or enctype is wrong

**Answer**

`IFormFile` properties on ViewModels bind from file input fields when the form uses `enctype="multipart/form-data"`. The binder captures the file's filename, content type, and a readable stream for the uploaded content alongside other form fields in the same POST body. Use `<input asp-for="UploadedFile" type="file" />` inside a multipart form; for multiple files, declare `IFormFileCollection UploadedFiles` or `List<IFormFile> UploadedFiles` on the ViewModel. Before processing, always validate the uploaded file: check size against a configured `[RequestSizeLimit]` or `Kestrel.MaxRequestBodySize`, whitelist the file extension from the client-supplied filename, and ideally inspect the content's magic bytes rather than trusting the extension. `IFormFile` is `null` when no file is selected (file input cleared) or when the form's `enctype` is wrong — always null-check before calling `.CopyToAsync()` or accessing `.Length`. Storing the raw `IFormFile` in a session or between requests is not supported; process or copy the stream within the action.

---

## Q13. What `enctype` is required for file upload forms?

**Concepts**
- `multipart/form-data` required for binary file content transmission
- Default `application/x-www-form-urlencoded` unable to carry file bytes
- `IFormFile` binding as null with wrong enctype despite other fields binding
- `<form enctype="multipart/form-data">` or tag helper equivalent
- Large file uploads requiring Kestrel `MaxRequestBodySize` configuration

**Answer**

Forms including file inputs must set `enctype="multipart/form-data"`. The default `application/x-www-form-urlencoded` encoding represents field values as percent-encoded strings and cannot carry binary file content — the browser simply omits the file bytes and `IFormFile` binds as `null` while text fields bind correctly, creating a confusing asymmetry. The tag helper syntax is `<form asp-action="Upload" method="post" enctype="multipart/form-data">`. Both metadata text fields and file inputs coexist in one multipart POST body, each as a separate MIME part. For large uploads, Kestrel's default `MaxRequestBodySize` of 30 MB may need increasing via `[RequestSizeLimit(maxBytes)]` on the action or globally in `WebHostBuilder`, and IIS has its own `maxAllowedContentLength` setting. Missing `enctype="multipart/form-data"` is the single most common cause of "file upload always arrives null" bugs and is worth checking first before investigating binder configuration.

---

## Q14. What is over-posting during model binding and how is it prevented?

**Concepts**
- Model binding setting all matching properties from POST keys
- Sensitive entity properties writable from crafted POST requests
- ViewModel property whitelist as the primary prevention
- `[BindNever]` as a weaker per-property opt-out
- Integration-testing POST with extra fields to verify protection

**Answer**

Over-posting is when attackers POST values for properties not shown in the UI — model binding sets every property on the action parameter type that matches a submitted key, regardless of whether the Razor form included an input for it. An attacker can add `IsAdmin=true` or `DiscountPercent=100` to a form POST that targets an action binding an entity or a ViewModel that includes those properties. The primary prevention is binding to a ViewModel with only the properties the form should accept — if `IsAdmin` is not a property on `ProfileEditViewModel`, the binding stage has no target for that key and ignores it silently. The server-side mapping then copies only the ViewModel's declared fields onto the tracked entity, so undeclared keys never reach persistence. `[Bind(Include = "Name,Email")]` on entity-typed parameters is a weaker alternative because it requires manually maintaining an allowlist that diverges from the entity as properties are added. `[BindNever]` protects individual properties but does not prevent an attacker from reaching others on the same type.

---

## Q15. What are `[BindNever]` and `[Bind]` used for?

**Concepts**
- `[BindNever]` excluding a single property from inbound binding
- `[Bind(Include = "...")]` allow-listing bindable properties on an action parameter
- Protecting navigation properties and server-assigned fields
- ViewModels as a stronger alternative to `[Bind]` on entities
- `[BindNever]` not removing properties from GET rendering

**Answer**

`[BindNever]` decorates a property to tell the model binder to skip it during POST binding — the property is ignored regardless of whether the request includes a matching key. This protects server-populated properties like dropdown collections, audit fields, and server-assigned IDs from being overwritten by tampered POST data. `[Bind(Prefix = "", Include = "Name,Email")]` or `[Bind(Exclude = "...")]` restricts binding on the action parameter type to an explicit allowlist or denylist, which can be applied at the action level rather than the property level. Both are weaker defenses than using a dedicated ViewModel because they require maintaining string-based property name lists that do not refactor-rename automatically when properties are renamed. `[BindNever]` on navigation properties and `ICollection<T>` members prevents the binder from attempting to deserialize graphs from form data. Importantly, `[BindNever]` does not affect GET rendering — the property still appears in Razor output normally; it only suppresses the inbound model binding step.

---

## Q16. How do checkboxes bind to `bool` and `bool?` properties?

**Concepts**
- Unchecked checkbox not submitting a key at all
- Non-nullable `bool` binding missing key to `false`
- Tag helper adding hidden `false` input to ensure explicit `false` submission
- `bool?` tri-state broken by hidden-false companion
- `[Required]` ineffective on non-nullable `bool` checkbox

**Answer**

HTML checkboxes only submit their value when checked — an unchecked checkbox contributes nothing to the POST body. For non-nullable `bool` properties, ASP.NET Core treats a missing key as `false`, so an unchecked box correctly results in `false`. The ASP.NET Core checkbox tag helper reinforces this by rendering a hidden `<input type="hidden" value="false" />` alongside the visible checkbox `<input type="checkbox" value="true" />`, both sharing the same `name`. When unchecked, only the hidden `false` submits; when checked, both submit and the binder uses the last value or true depending on how the form value provider resolves duplicates. For `bool?` tri-state properties where `null` means "user did not express a preference," this hidden-false companion is harmful — it forces the property to `false` on unchecked rather than `null`. In that case, write the checkbox manually without the tag helper's hidden companion. `[Required]` on non-nullable `bool` is also ineffective for consent checkboxes because `false` satisfies the not-null requirement.

---

## Q17. What is the hidden-field pattern for checkboxes?

**Concepts**
- Hidden `false` input submitted regardless of checkbox checked state
- Checked state overriding hidden `false` when both submitted
- Guaranteed explicit `false` for non-nullable `bool` properties
- Pattern breaking tri-state `bool?` where null means "unchanged"
- Required consent checkboxes needing additional true-assertion validation

**Answer**

The checkbox tag helper renders two inputs sharing the same `name`: `<input type="hidden" name="IsActive" value="false" />` followed by `<input type="checkbox" name="IsActive" value="true" />`. When the checkbox is unchecked, only the hidden field submits, and the binder receives `false` — ensuring non-nullable `bool` always gets an explicit value rather than binding a missing key to `false` implicitly. When checked, both inputs submit; the form value provider takes the checkbox's `true` value. The pattern exists because without it, unchecked checkboxes disappear from the POST entirely, which is technically correct for `bool` (missing → false) but fragile if the binding behavior is not understood. The pattern breaks tri-state `bool?` scenarios — for a PATCH-style update where `null` means "do not change this preference," applying the hidden-false companion converts null-intent into an explicit false. In that case, use manual `<input type="checkbox">` markup without the hidden companion and handle the missing key as `null` intentionally.

---

## Q18. What is a custom `IModelBinder` and when would you create one?

**Concepts**
- `IModelBinder.BindModelAsync` converting non-standard request data to CLR types
- `IModelBinderProvider` for registration and type-based dispatch
- Stateless binder implementation to avoid concurrent-request corruption
- Use cases: composite keys, currency string parsing, non-contiguous collection indices
- `ModelBinderProviders.Insert(0, provider)` for priority over built-in binders

**Answer**

A custom `IModelBinder` implements `BindModelAsync` to convert non-standard request data into a model property or parameter type that the built-in binders cannot handle. Common use cases are parsing `"$1,234.56"` currency strings into `decimal`, binding composite key types (`new ProductKey { Category = "A", Id = 5 }`) from two separate route or form values, or handling `Lines[SKU-123].Qty` dictionary-style bindings. The binder is registered through an `IModelBinderProvider` that inspects the requested type and returns an instance of the custom binder when appropriate, inserted at position 0 in `options.ModelBinderProviders` to run before the built-in binders. The most critical design constraint is that binders must be stateless — no instance fields that store request data across calls. The framework instantiates binders potentially once as singletons (depending on provider implementation), so storing per-request data in fields causes race conditions under concurrent load. All intermediate state must live in local variables within `BindModelAsync`.

---

## Gotchas — Model Binding in MVC (Interview Traps)

---

#### Gotcha 1. Missing `[Bind]` attribute leaving models open to overposting

**Concepts**
- Overposting — attacker adds extra fields to POST body that bind to properties the form never shows
- `[Bind("Name,Email")]` — whitelist of bindable properties; extras are silently ignored
- `[BindNever]` — per-property exclusion from binding
- Input-only ViewModel — cleanest defense; only exposes properties the form needs

**Answer**

When an EF entity or a ViewModel with many properties is bound directly from the POST body, an attacker can include additional form fields — `IsAdmin=true`, `Role=Admin`, `Balance=10000` — that bind to entity properties never shown in the form. Without `[Bind]` or a purpose-built input ViewModel, these fields silently update sensitive properties. The `[Bind("Name,Email")]` attribute on the action parameter whitelists allowed properties; any field not listed is ignored during binding. The cleaner long-term solution is a dedicated input ViewModel that exposes only the fields the form submits, making the whitelist implicit through the type itself rather than an attribute string.

---

#### Gotcha 2. Complex object prefix mismatch when using `[FromForm(Name="prefix")]`

**Concepts**
- Default prefix — matches the parameter name for nested objects
- `[FromForm(Name="")]` — removes prefix, binds top-level form fields
- Prefix mismatch — child object properties not found, all at default values
- `[Bind(Prefix="")]` — alternative prefix override on model binding

**Answer**

When a form posts `Address.City` and `Address.PostalCode` but the action parameter is `[FromForm] AddressViewModel address` without a prefix override, the model binder looks for `address.City` — the parameter name as prefix. The posted keys `Address.City` do not match and the object binds with all default values. Conversely, using `[FromForm(Name="")]` strips the prefix and expects top-level `City` and `PostalCode` keys. The fix is to ensure the form input names match the expected binder prefix: either match the parameter name in HTML (`name="address.City"`) or specify the prefix explicitly in the attribute to align with what the HTML form actually posts.

---

#### Gotcha 3. `[FromBody]` used on a standard browser form POST

**Concepts**
- HTML form submission — `application/x-www-form-urlencoded`, not JSON
- `[FromBody]` — activates the JSON input formatter; ignores form fields
- Silent binding failure — model parameter receives all default values
- Content-Type — `[FromBody]` requires `Content-Type: application/json` from the client

**Answer**

Standard browser forms send `application/x-www-form-urlencoded` or `multipart/form-data`. `[FromBody]` instructs the model binder to use the JSON input formatter, which reads the body as JSON. When a form POST arrives, the formatter finds no JSON content and the parameter defaults — every string is null, every int is zero — while the action runs silently. The fix for MVC form POSTs is to remove `[FromBody]` and let the default form value provider handle binding. `[FromBody]` belongs only on AJAX or API endpoints where the client explicitly sets `Content-Type: application/json` and sends a JSON payload.

---

#### Gotcha 4. `DateTime` binding failing due to server culture vs client locale mismatch

**Concepts**
- Model binder culture — uses server's current culture for date string parsing by default
- Client date format — browser submits `MM/DD/YYYY` but server culture expects `DD/MM/YYYY`
- Silent binding failure — `ModelState` records conversion error, property stays at default
- `[BindProperty(SupportsGet = false)]` + `IModelBinder` — correct place for custom format handling

**Answer**

A date input submitting `07/04/2025` parses as April 7 on a UK-culture server (`DD/MM/YYYY`) and as July 4 on a US-culture server (`MM/DD/YYYY`). When the server culture does not match the browser's date format, the model binder fails to parse the string, adds a conversion error to `ModelState`, and leaves the `DateTime` property at `DateTime.MinValue`. The form appears to submit successfully but stores wrong or empty dates. The correct approach is to use ISO 8601 format (`yyyy-MM-dd`) via `<input type="date">` which browsers submit in ISO format regardless of locale, or implement a custom `IModelBinder` that normalizes date strings before parsing.

---

#### Gotcha 5. File upload `IFormFile` failing because `enctype="multipart/form-data"` is missing

**Concepts**
- `<form enctype="multipart/form-data">` — required for file upload forms
- Default `enctype` — `application/x-www-form-urlencoded`, strips binary file content
- `IFormFile` parameter — always null or empty without multipart encoding
- Tag Helper — `<form asp-enctype="multipart/form-data">` or `method="post"` alone is insufficient

**Answer**

File input elements require `<form method="post" enctype="multipart/form-data">`. Without `enctype="multipart/form-data"`, the browser submits the file input as a plain filename string in the URL-encoded body, not as binary multipart data. The model binder receives no binary content and `IFormFile` is null. The `<form method="post">` default enctype is `application/x-www-form-urlencoded`, which cannot carry binary data. This is a silent failure — the action receives a null `IFormFile` with no model binding error in `ModelState`, making the problem invisible without debugging. Every form that includes a file input must specify `enctype="multipart/form-data"`.

---

#### Gotcha 6. Custom `IModelBinder` not registered causing fallback to default binder

**Concepts**
- Custom `IModelBinder` — must be registered via `[ModelBinder(typeof(MyBinder))]` or `IModelBinderProvider`
- Without registration — default binder runs; custom logic is ignored
- `IModelBinderProvider` — registered globally in `AddControllersWithViews().AddMvcOptions(...)`
- `[ModelBinder]` attribute on parameter — per-parameter registration without global change

**Answer**

Implementing `IModelBinder` and `IModelBinderProvider` is not sufficient — the provider must be inserted into `MvcOptions.ModelBinderProviders` or the `[ModelBinder(typeof(MyBinder))]` attribute must be applied to the parameter. Without registration, the framework falls back to the default complex type binder, which ignores the custom logic entirely and silently produces wrong bindings. The registration order matters: `ModelBinderProviders` is checked in order and the first provider that returns a non-null binder wins — custom providers that should override built-in behavior must be inserted at the front of the list using `Insert(0, ...)`, not added at the end with `Add(...)`.

---

#### Gotcha 7. Route constraint not matching causing 404 instead of model binding error

**Concepts**
- Route constraint `{id:int}` — fails the route match, returns 404 for non-integer
- No route constraint — route matches, model binder fails, `ModelState` has error
- 404 vs 400 — route mismatch is 404 (no route found); binding failure is 400 or model error
- Route constraint purpose — prevent wrong route from matching, not to validate business input

**Answer**

`[HttpGet("{id:int}")]` requires the `id` segment to parse as an integer at the routing layer. A request for `GET /Products/abc` does not match the route and returns 404 — the client never reaches model binding. Without the constraint (`{id}`), the route matches, the model binder tries to convert "abc" to `int`, fails, adds an error to `ModelState`, and the action receives `id = 0` (default) with an invalid `ModelState`. The behavioral difference matters for API clients: 404 signals a wrong URL while 400 signals a bad request to a valid route. Choose route constraints deliberately: `{id:int}` when only integers should match, omit when any value should reach binding for custom error messages.

---

#### Gotcha 8. `[FromQuery]` and `[FromRoute]` conflicts when both match the same name

**Concepts**
- `[FromRoute]` — binds from URL segment; higher precedence than query string
- `[FromQuery]` — binds from query string parameter
- Unattributed parameter — searched in route, then form, then query string in order
- Ambiguous binding — same name in both route and query string produces route value

**Answer**

When a route template is `[HttpGet("{id:int}")]` and the client sends `GET /Products/5?id=10`, the `id` parameter in the action receives `5` from the route — not `10` from the query string — because the route binding takes precedence. If the action has an explicit `[FromQuery] int id` attribute, it binds `10` from the query string and ignores the route segment `5`. Mixing route and query string values with the same name without explicit `[FromRoute]` or `[FromQuery]` attributes causes subtle, environment-specific binding behavior. Use explicit source attributes on every parameter whose source might be ambiguous and name them distinctly to avoid conflicts.

---

#### Gotcha 9. Binding a nested collection where the parent object also needs binding

**Concepts**
- Parent prefix — `[FromForm] OrderViewModel order` expects `order.Items[0].Qty`
- Nested collection — items posted as `Items[0].Qty` without the parent prefix bind to nothing
- HTML form naming — must match the full prefix path including parent parameter name
- `[FromBody]` for JSON — JSON array/object nesting is handled differently than form field naming

**Answer**

When an action takes `[FromForm] OrderViewModel order`, the model binder expects form fields prefixed with `order.` — so `order.Items[0].Qty`, `order.Items[0].ProductId`, and so on. A Razor form that renders `name="Items[0].Qty"` without the `order.` prefix posts fields with no matching prefix and the `Items` collection binds empty. The mismatch is invisible in `ModelState` — binding succeeds but produces an empty list. Using `[FromForm(Name="")]` on the parameter changes the expected prefix to empty, matching top-level `Items[0].Qty` keys. Every form that binds complex nested objects must align HTML `name` attributes with the model binder's expected prefix path.

---

#### Gotcha 10. `TryUpdateModelAsync` not being called when manually constructing model instances

**Concepts**
- `TryUpdateModelAsync<T>` — applies model binding and validation to an existing object instance
- Manual property assignment — bypasses model binding, no `ModelState` validation
- `IFormCollection` direct access — skips binding infrastructure, type conversion not applied
- `TryUpdateModelAsync` use case — selective update of allowed properties on a fetched entity

**Answer**

Fetching an entity from the database and then manually assigning `entity.Name = Request.Form["Name"]` bypasses model binding entirely — no type conversion, no validation attribute enforcement, and no `ModelState` population. The `TryUpdateModelAsync<TModel>(entity, prefix, includeExpressions)` method applies full model binding to an existing object instance, populates `ModelState` with validation errors, and returns false if binding fails. This is the correct pattern for "edit only allowed fields" scenarios where you load the entity first to preserve fields the user must not change, then apply binding selectively to the allowed properties using include expressions as the whitelist.

---
## Scenario-Based Questions (Karat Format)

---

#### Q1. (R) Review this MVC create action. The Razor form submits correctly in the browser (Network tab shows `application/x-www-form-urlencoded` fields), but `ProductName` and `Price` arrive empty on the server.

```csharp
public class ProductEditViewModel
{
    public string ProductName { get; set; } = "";
    public decimal Price { get; set; }
}

[HttpPost]
public IActionResult Create([FromBody] ProductEditViewModel model)
{
    if (!ModelState.IsValid)
        return View(model);
    _products.Add(model);
    return RedirectToAction(nameof(Index));
}
```

Razor form uses `<form asp-action="Create" method="post">` with `<input asp-for="ProductName" />` and `<input asp-for="Price" />` — no `enctype` override.

**Concepts**
- `[FromBody]` invoking JSON input formatter on a form-encoded body
- Form value provider bypassed by `[FromBody]`
- Silent binding failure with model at defaults
- Removing `[FromBody]` restoring default form + route + query binding

**Answer**

The `[FromBody]` attribute on the action parameter is the cause. The Razor form posts `application/x-www-form-urlencoded` — which is the correct format for a standard browser form — but `[FromBody]` tells the model binder to use the JSON input formatter. The JSON formatter reads the request body, finds form-encoded text instead of JSON, and either fails silently or leaves the model at defaults. The form value provider that would correctly parse `ProductName=Widget&Price=9.99` is never consulted. `ModelState` may not report errors because no type conversion was attempted — the model just has empty string and zero from its initializers.

The fix is to remove `[FromBody]` from the parameter entirely. With no source attribute, the model binder falls back to the default MVC binding source set — form fields, route values, and query string — which reads the `application/x-www-form-urlencoded` body correctly and populates `ProductName` and `Price`. Keep `[FromBody]` only for AJAX or API endpoints where the caller explicitly sends `Content-Type: application/json` with a JSON payload.

---

#### Q2. (R) Review line-item editing. Users delete a middle row in the UI; after POST, quantities shift and the wrong SKU is updated. Form field names use explicit indices.

```html
<!-- Rendered names after user removes row 1 -->
<input name="Lines[0].Sku" value="WIDGET-A" />
<input name="Lines[0].Qty" value="2" />
<input name="Lines[2].Sku" value="WIDGET-C" />
<input name="Lines[2].Qty" value="5" />
```

```csharp
public class OrderEditViewModel
{
    public List<LineItemViewModel> Lines { get; set; } = new();
}

public class LineItemViewModel
{
    public string Sku { get; set; } = "";
    public int Qty { get; set; }
}

[HttpPost]
public IActionResult Save(OrderEditViewModel model)
{
    foreach (var line in model.Lines)
        _orders.Apply(line.Sku, line.Qty);
    return RedirectToAction("Index");
}
```

**Concepts**
- Gap at `Lines[1]` inserting phantom default entry at index 1
- `model.Lines[1]` having empty `Sku` and zero `Qty`
- `_orders.Apply` processing phantom row between real data
- JavaScript re-indexing after deletion as the authoritative fix
- Server-side `Where` filter on non-empty `Sku` as safety net

**Answer**

The POST body contains `Lines[0]` and `Lines[2]` with no `Lines[1]`. The model binder fills position 1 with a default `LineItemViewModel` (empty `Sku`, zero `Qty`) because the collection binder expects contiguous indices and inserts a default object at each gap. The resulting `model.Lines` has three entries: WIDGET-A at index 0, an empty phantom at index 1, and WIDGET-C at index 2. The `foreach` loop then calls `_orders.Apply("", 0)` for the phantom row before processing WIDGET-C — potentially creating a blank line item or corrupting whatever `Apply` does with an empty SKU.

The fix has two layers. On the client, re-index all remaining rows in JavaScript immediately after any row is removed, so the submitted names are always `Lines[0]`, `Lines[1]`, ... without gaps. On the server, add a filter before the loop: `model.Lines.Where(l => !string.IsNullOrEmpty(l.Sku))` discards phantom rows even if the client-side re-indexing fails. For editable grids where stability under add/delete is critical, an explicit SKU-keyed custom binder handles non-contiguous indices more robustly than re-indexing.

---

#### Q3. (R) Review nested address binding on checkout. `ShippingCity` binds but nested `Address.City` is always null despite visible inputs.

```csharp
public class CheckoutViewModel
{
    public string ShippingCity { get; set; } = ""; // top-level — works
    public AddressModel Address { get; set; } = new();
}

public class AddressModel
{
    public string City { get; set; } = "";
    public string PostalCode { get; set; } = "";
}
```

Partial view `_AddressEditor.cshtml` renders:

```html
<input name="City" asp-for="City" />
<input name="PostalCode" asp-for="PostalCode" />
```

Invoked from parent as `@await Html.PartialAsync("_AddressEditor", Model.Address)`.

**Concepts**
- Partial rendered with `AddressModel` as `@model` losing the `Address.` parent prefix
- `name="City"` instead of `name="Address.City"` binding to nonexistent top-level property
- `<partial for="Model.Address" />` preserving prefix through expression binding
- `ViewData.TemplateInfo.HtmlFieldPrefix` for manual prefix control
- Silent null result for `Address.City` with no error

**Answer**

The partial is rendered with `Model.Address` as its `@model`, which means it knows only about `AddressModel` properties — it has no knowledge that this object lives at `Address.` inside the parent `CheckoutViewModel`. The `asp-for="City"` tag helper generates `name="City"` based on the partial's own model context. When the POST body contains `City=London`, the model binder looks for a top-level `City` property on `CheckoutViewModel`, finds none, and ignores the value — leaving `Address.City` null. `ShippingCity` works because it is declared directly on `CheckoutViewModel` and its name matches exactly.

The fix is to change the invocation to `<partial name="_AddressEditor" for="Model.Address" />`. The `for` attribute accepts a model expression relative to the parent view's model, and the partial tag helper uses it to set the HTML field prefix to `Address.` automatically, so `asp-for="City"` inside the partial emits `name="Address.City"`. Alternatively, pass `ViewData.TemplateInfo.HtmlFieldPrefix = "Address"` in a `ViewDataDictionary` when calling `Html.PartialAsync`, or use `@Html.EditorFor(m => m.Address)` which handles the prefix automatically through editor templates.

---

#### Q4. (R) Review optional numeric and date fields on an HR form. Empty inputs produce unexpected values instead of "not provided."

```csharp
public class LeaveRequestViewModel
{
    public int? DaysRequested { get; set; }
    public DateTime? StartDate { get; set; }
}

[HttpPost]
public IActionResult Submit(LeaveRequestViewModel model)
{
    // Expected: omitted fields → null; user reports DaysRequested = 0, StartDate = 0001-01-01
    _leave.Submit(model);
    return RedirectToAction("Index");
}
```

View uses `<input asp-for="DaysRequested" />` and `<input asp-for="StartDate" type="date" />` — user clears both fields and submits.

**Concepts**
- Nullable value types correctly binding empty inputs to `null`
- Non-nullable `int` and `DateTime` binding empty string to `0` / `DateTime.MinValue`
- The bug implying actual production code uses non-nullable types
- `<input type="date">` submitting empty string when cleared
- Checking `ModelState` for conversion errors separate from required-field errors

**Answer**

The ViewModel shown — with `int?` and `DateTime?` — is the correct design. When a user clears an `<input>` and submits, the browser posts an empty string for that field. For `int?`, the model binder parses the empty string as `null` successfully. For `DateTime?`, the same: empty string → `null`. The shown code should work as expected.

The reported symptoms (0 and 0001-01-01) indicate the actual production ViewModel uses non-nullable `int` and `DateTime` rather than the nullable types shown. For non-nullable `int`, an empty string produces a `ModelState` conversion error and the property stays at its default value `0`. For non-nullable `DateTime`, an empty string similarly fails conversion and the property stays at `DateTime.MinValue` (`0001-01-01`). If `ModelState.IsValid` is not checked before calling `_leave.Submit`, these default values are submitted as if they were real user input.

The fix is to ensure the production code uses `int?` and `DateTime?` for optional fields as shown in the review code, and to verify that the action checks `ModelState.IsValid` before proceeding. For `<input type="date">`, ASP.NET Core's date parser handles empty strings for `DateTime?` correctly, but culture-sensitive `type="text"` date inputs can fail if the server culture does not match the date format — using `type="date"` (ISO format) eliminates that variable.

---

#### Q5. (R) Review culture-sensitive date binding. UK users enter `31/01/2026`; US-hosted server culture is `en-US`. Binding fails silently and `ModelState` shows errors users do not understand.

```csharp
// Program.cs — no RequestLocalization middleware configured; server culture en-US

public class EventViewModel
{
    [Required]
    public DateTime EventDate { get; set; }
}

[HttpPost]
public IActionResult Create(EventViewModel model)
{
    if (!ModelState.IsValid)
        return View(model);
    _events.Add(model);
    return RedirectToAction("Index");
}
```

Form posts `EventDate=31/01/2026` as text (no `<input type="date">` — legacy browser support requirement).

**Concepts**
- Server `en-US` culture parsing `31/01/2026` as invalid (expects `M/d/yyyy`)
- `ModelState` error "The value is not valid for EventDate" from culture mismatch
- `RequestLocalization` middleware with `en-GB` culture provider as one fix
- `<input type="date">` posting ISO `yyyy-MM-dd` as the culture-invariant alternative
- Custom model binder for date strings when culture cannot be negotiated

**Answer**

The model binder calls `DateTime.TryParse("31/01/2026", CultureInfo.CurrentCulture, ...)` and fails because `en-US` culture expects month-first format `M/d/yyyy` — `31` is not a valid month. The `ModelState` receives a conversion error for `EventDate` and the form is returned with a message like "The value '31/01/2026' is not valid for EventDate." The user entered a valid date for their locale and sees a confusing error with no hint about the expected format.

There are two practical fixes. The first is to add `RequestLocalization` middleware configured with `en-GB` as a supported culture and a `CookieRequestCultureProvider` or `AcceptLanguageHeaderRequestCultureProvider` so UK users' requests are parsed with their locale. The second — and more robust for legacy browser support requirements — is to switch to `<input type="date">` when possible, since the browser posts ISO 8601 (`2026-01-31`) regardless of display locale, making the server culture irrelevant. When neither option is available (true legacy browser support), a custom `IModelBinder` that tries multiple date formats (`dd/MM/yyyy`, `M/d/yyyy`, ISO) provides graceful parsing. In all cases, the `Program.cs` should at minimum configure `RequestLocalization` with the supported locales to prevent silent culture mismatches in cloud deployments where the OS culture defaults to `en-US`.

---

#### Q6. (R) Review document upload on an invoice form. File and metadata should bind together; `UploadedFile` is always null while other fields bind.

```html
<form asp-action="Upload" method="post">
    <input asp-for="InvoiceNumber" />
    <input asp-for="Amount" />
    <input type="file" name="UploadedFile" />
    <button type="submit">Upload</button>
</form>
```

```csharp
public class InvoiceUploadViewModel
{
    public string InvoiceNumber { get; set; } = "";
    public decimal Amount { get; set; }
    public IFormFile? UploadedFile { get; set; }
}

[HttpPost]
public IActionResult Upload(InvoiceUploadViewModel model)
{
    if (model.UploadedFile == null)
        return View(model); // always hits this branch
    _storage.Save(model);
    return RedirectToAction("Index");
}
```

**Concepts**
- Missing `enctype="multipart/form-data"` preventing file byte transmission
- Default `application/x-www-form-urlencoded` unable to carry binary content
- `IFormFile` binding to null while text fields bind correctly with wrong enctype
- Adding `enctype="multipart/form-data"` to the form tag as the fix

**Answer**

The form is missing `enctype="multipart/form-data"`. Without it, the browser uses the default `application/x-www-form-urlencoded` encoding, which can only transmit text key/value pairs — it cannot carry binary file content. The file input is simply omitted from the POST body, so `IFormFile? UploadedFile` receives `null`. The text fields `InvoiceNumber` and `Amount` bind correctly because they are plain strings that urlencoded format handles fine, which explains the asymmetry.

The fix is to add `enctype="multipart/form-data"` to the form element:

```html
<form asp-action="Upload" method="post" enctype="multipart/form-data">
```

With multipart encoding, the browser sends each field and the file as separate MIME parts, and `IFormFile` correctly receives the file stream. After the fix, also add server-side validation: check `model.UploadedFile.Length > 0`, validate the file extension against a whitelist, and configure `[RequestSizeLimit]` or Kestrel `MaxRequestBodySize` for the expected maximum file size. Missing `enctype="multipart/form-data"` is the first thing to check whenever `IFormFile` properties are consistently null.

---

#### Q7. (R) Review user profile update for over-posting. QA reports a non-admin can set themselves admin via browser dev tools.

```csharp
public class User  // EF entity used directly as the action parameter
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = "";
    public string Email { get; set; } = "";
    public bool IsAdmin { get; set; }
    [BindNever] public DateTime CreatedUtc { get; set; }
}

[HttpPost]
public IActionResult Edit(int id, User model)
{
    var user = _db.Users.Find(id);
    user.DisplayName = model.DisplayName;
    user.Email = model.Email;
    user.IsAdmin = model.IsAdmin; // copied from bound model
    _db.SaveChanges();
    return RedirectToAction("Index");
}
```

View only shows `DisplayName` and `Email` fields — no `IsAdmin` input in Razor.

**Concepts**
- `IsAdmin` on the bound `User` entity writable from crafted POST body
- `[BindNever]` on `CreatedUtc` not protecting `IsAdmin`
- View's absence of `<input asp-for="IsAdmin">` not preventing binding
- Dedicated `ProfileEditViewModel` with only `DisplayName` and `Email`
- Explicit property copy rather than binding to the entity directly

**Answer**

`[BindNever]` is applied to `CreatedUtc` but not to `IsAdmin`, so model binding reads `IsAdmin=true` from the POST body when an attacker adds it with dev tools. The view does not render an input for `IsAdmin`, but model binding reads the request body — not the rendered HTML — so the absence of a form input provides no security. The action then explicitly copies `model.IsAdmin` onto the tracked entity, which is the proximate cause of the privilege escalation: even if `IsAdmin` were `[BindNever]`, copying it would be wrong.

The fix is to introduce a `ProfileEditViewModel` containing only `DisplayName` and `Email` — properties the user is allowed to change — and bind the action parameter to that type:

```csharp
[HttpPost]
public IActionResult Edit(int id, ProfileEditViewModel vm)
{
    if (!ModelState.IsValid) return View(vm);
    var user = _db.Users.Find(id);
    user.DisplayName = vm.DisplayName;
    user.Email = vm.Email;
    _db.SaveChanges();
    return RedirectToAction("Index");
}
```

`IsAdmin` is not declared on `ProfileEditViewModel`, so binding never sets it, and the explicit copy only transfers the two safe fields. Admin role changes belong in a separate `[Authorize(Roles = "Admin")]` action that binds a dedicated admin ViewModel.

---

#### Q8. (R) Review registration flow. Validation attributes fire (client-side shows errors), but invalid data still reaches the database after POST.

```csharp
public class RegisterViewModel
{
    [Required][EmailAddress] public string Email { get; set; } = "";
    [Required][MinLength(8)] public string Password { get; set; } = "";
}

[HttpPost]
public IActionResult Register(RegisterViewModel model)
{
    var user = _users.Create(model.Email, model.Password);
    _db.Users.Add(user);
    _db.SaveChanges();
    return RedirectToAction("Login");
}
```

View includes `_ValidationScriptsPartial` and `<div asp-validation-summary="All">`; `[HttpPost]` action never checks `ModelState`.

**Concepts**
- Missing `ModelState.IsValid` check before service call
- Client validation preventing submission only in the browser
- Direct POST bypassing browser JavaScript reaching the action
- MVC controllers not auto-rejecting invalid models (unlike `[ApiController]`)

**Answer**

The action never checks `ModelState.IsValid`, so validation attributes run and populate `ModelState` during binding — but the action ignores the result and calls `_users.Create` regardless. An invalid email or a 3-character password fails `[EmailAddress]` and `[MinLength(8)]` and MVC records those errors in `ModelState`, but the action proceeds to create the user and save to the database. An attacker who POSTs directly to `/Account/Register` with a bypassed JavaScript layer — or simply with a weak password that the client-side script did not catch — gets the invalid data persisted.

The fix is to add the standard guard at the top of the POST action:

```csharp
if (!ModelState.IsValid)
    return View(model);
```

This returns the form with validation messages when `ModelState` has errors. MVC controllers do not auto-reject invalid models the way `[ApiController]` does — the guard is always an explicit requirement. Client-side validation via `_ValidationScriptsPartial` is a UX improvement that shows errors before the round-trip, but it is not a security boundary and must always be paired with the server-side check.

---

#### Q9. (M) A legacy admin page uses jQuery to POST updates without a full form submit. Fields never bind server-side. Compare what the client sends vs what the MVC action expects.

```javascript
// Client — submit handler
$.ajax({
    url: '/Admin/UpdateSettings',
    type: 'POST',
    contentType: 'application/json',
    data: JSON.stringify({ siteName: $('#siteName').val(), maxUsers: $('#maxUsers').val() })
});
```

```csharp
public class SiteSettingsViewModel
{
    public string SiteName { get; set; } = "";
    public int MaxUsers { get; set; }
}

[HttpPost]
public IActionResult UpdateSettings(SiteSettingsViewModel model)
{
    // model.SiteName empty, MaxUsers 0
    _settings.Save(model);
    return Ok();
}
```

Controller inherits `Controller` (not `[ApiController]`). No `[FromBody]` on the parameter.

**Concepts**
- Client sending `Content-Type: application/json` with a JSON body
- MVC default binding source ignoring the JSON body (form + route + query)
- `[FromBody]` required to activate the JSON input formatter
- camelCase JSON keys requiring `JsonSerializerOptions.PropertyNameCaseInsensitive` or matching casing

**Answer**

The client sets `contentType: 'application/json'` and sends a JSON body — `{"siteName":"MyApp","maxUsers":50}`. The MVC action parameter has no `[FromBody]` attribute, so the default binding source set (form fields + route values + query string) is used. None of those sources contain the JSON payload, so `SiteName` and `MaxUsers` stay at their defaults. `ModelState` has no errors because no conversion was attempted — the properties just never received values.

The fix is to add `[FromBody]` to the action parameter:

```csharp
public IActionResult UpdateSettings([FromBody] SiteSettingsViewModel model)
```

This activates the JSON input formatter, which reads the request body and deserializes the JSON. A secondary issue is casing: the JSON body uses camelCase keys (`siteName`, `maxUsers`) while the C# property names are PascalCase. System.Text.Json serialization in ASP.NET Core defaults to camelCase in JSON output and case-insensitive matching on input, so `siteName` matches `SiteName` by default — but if a custom `JsonSerializerOptions` without `PropertyNameCaseInsensitive = true` is registered, the casing mismatch would silently leave properties at defaults again. Also note that `[AutoValidateAntiforgeryToken]` on the controller would reject this AJAX POST unless the request includes the antiforgery token header or the action is decorated with `[IgnoreAntiforgeryToken]`.

---

#### Q10. (P) A team registers a custom `CurrencyModelBinder` to parse `"$1,234.56"` from form fields. The binder stores the last parsed culture in an instance field for logging. It is registered as a singleton. What breaks under concurrent form posts, and how should binders be written?

```csharp
public class CurrencyModelBinder : IModelBinder
{
    private CultureInfo? _lastCulture;

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).FirstValue;
        _lastCulture = bindingContext.HttpContext.Features.Get<IRequestCultureFeature>()?.RequestCulture.Culture;
        if (decimal.TryParse(value, NumberStyles.Currency, _lastCulture, out var amount))
        {
            bindingContext.Result = ModelBindingResult.Success(amount);
            return Task.CompletedTask;
        }
        bindingContext.ModelState.AddModelError(bindingContext.ModelName, "Invalid currency amount.");
        bindingContext.Result = ModelBindingResult.Failed();
        return Task.CompletedTask;
    }
}

// Startup
builder.Services.AddSingleton<IModelBinder, CurrencyModelBinder>();
```

**Concepts**
- `_lastCulture` instance field shared across concurrent requests on a singleton
- Read-write race condition corrupting culture used for parsing
- Stateless binder requirement: all per-request data in local variables
- `IModelBinderProvider` as the registration mechanism, not DI singleton
- Logging via `ILogger` injected into the binder constructor, not stored in fields

**Answer**

The `_lastCulture` instance field is the problem. When two concurrent requests hit `BindModelAsync` simultaneously — say a UK user and a US user — one request writes its culture to `_lastCulture` just as the other request is reading it to parse its currency value. The result is a race condition where request B parses `"$1,234.56"` using `en-GB` culture (set by request A), which may fail or produce incorrect decimal values because the culture separators differ. Under load this produces intermittent binding failures that are nearly impossible to reproduce in single-threaded development.

The fix is to make the binder stateless: move `_lastCulture` to a local variable inside `BindModelAsync`:

```csharp
public Task BindModelAsync(ModelBindingContext bindingContext)
{
    var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).FirstValue;
    var culture = bindingContext.HttpContext.Features.Get<IRequestCultureFeature>()?.RequestCulture.Culture
                  ?? CultureInfo.CurrentCulture;
    if (decimal.TryParse(value, NumberStyles.Currency, culture, out var amount))
    {
        bindingContext.Result = ModelBindingResult.Success(amount);
        return Task.CompletedTask;
    }
    bindingContext.ModelState.AddModelError(bindingContext.ModelName, "Invalid currency amount.");
    bindingContext.Result = ModelBindingResult.Failed();
    return Task.CompletedTask;
}
```

For logging the culture, inject `ILogger<CurrencyModelBinder>` in the constructor — logging infrastructure is thread-safe — and log from the local variable. Binders should also be registered via `IModelBinderProvider` added to `options.ModelBinderProviders`, not as DI singleton services, because the framework instantiates binders through the provider pattern rather than through the DI container directly.

---

#### Q11. (R) Review checkbox and hidden-field pattern for `bool` and `bool?` opt-in flags. Marketing reports opt-out users still show as subscribed after save.

```html
<form asp-action="SavePreferences" method="post">
    <input type="hidden" asp-for="UserId" />
    <!-- Newsletter: optional tri-state — null = no change on server -->
    <input type="checkbox" asp-for="Newsletter" />
    <!-- Terms: required true to submit -->
    <input type="checkbox" asp-for="AcceptedTerms" />
    <button type="submit">Save</button>
</form>
```

```csharp
public class PreferencesViewModel
{
    public int UserId { get; set; }
    public bool? Newsletter { get; set; }   // PATCH-style: null = unchanged
    public bool AcceptedTerms { get; set; }
}

[HttpPost]
public IActionResult SavePreferences(PreferencesViewModel model)
{
    _prefs.Update(model.UserId, newsletter: model.Newsletter, terms: model.AcceptedTerms);
    return RedirectToAction("Index");
}
```

User leaves Newsletter unchecked (wants unchanged) but AcceptedTerms checked. Saved newsletter becomes `false` instead of unchanged.

**Concepts**
- `asp-for` on `bool?` generating the hidden-false companion inappropriate for tri-state
- Hidden `false` input posting even when tri-state `null` intent is correct
- Checkbox tag helper designed for non-nullable `bool`, not `bool?` PATCH semantics
- Manual `<input type="checkbox">` without hidden companion for tri-state
- `bool?` binding absent key as `null` when no hidden companion is present

**Answer**

`asp-for="Newsletter"` on a `bool?` property uses the ASP.NET Core checkbox tag helper, which automatically renders a hidden `<input type="hidden" name="Newsletter" value="false" />` before the checkbox. When the user leaves the checkbox unchecked, the hidden `false` input is submitted, and the model binder receives `Newsletter=false` — an explicit `false` rather than an absent key. `bool? Newsletter` then binds to `false` (not `null`), so the server sees an explicit opt-out and updates the subscription instead of leaving it unchanged.

The fix is to write the Newsletter checkbox manually without the hidden companion:

```html
<!-- Manual markup: absent key → null for bool? -->
<input type="checkbox" name="Newsletter" value="true" />
```

When unchecked, the field is absent from the POST body entirely, and `bool? Newsletter` binds to `null` — the intended "no change" signal. When checked, `name="Newsletter" value="true"` posts `true`. The `AcceptedTerms` checkbox can keep using `asp-for` since it is non-nullable `bool` where the hidden-false companion behavior is correct — unchecked means explicitly `false`, not null.
