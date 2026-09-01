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

What is model binding in ASP.NET Core MVC?

**Answer:** Model binding is the framework mechanism that maps HTTP request data — form fields, query strings, route values, and headers — onto action method parameters and complex object properties. It runs before the action executes, populating ViewModels and primitives from the incoming request.

- Value providers supply key/value pairs; model binders convert strings to CLR types and set properties recursively on complex types.
- Validation runs during binding when Data Annotations or `IValidatableObject` are present — results accumulate in `ModelState`.
- Default binding source for MVC controller actions is form + route + query — not JSON body unless `[FromBody]` is specified.
- Custom binders implement `IModelBinder` for non-standard formats (currency parsing, composite keys).

---

## Q2. How does model binding work for HTML form POSTs?

How does model binding work for HTML form POSTs?

**Answer:** Browser forms POST `application/x-www-form-urlencoded` or `multipart/form-data` key/value pairs. The form value provider reads these keys, and the complex object model binder matches names to property paths — `ProductName` → `model.ProductName`, `Address.City` → `model.Address.City` — building the object graph.

- Tag helpers generate `name` attributes that match the ViewModel property path automatically via `asp-for`.
- Route values and query strings also participate unless restricted with `[FromForm]`.
- Binding order and culture affect parsing of dates and numbers from text inputs.
- After binding, check `ModelState.IsValid` before persisting — MVC does not auto-reject invalid models like `[ApiController]` does.

---

## Q3. What is the difference between `[FromForm]` and `[FromBody]` in MVC?

What is the difference between `[FromForm]` and `[FromBody]` in MVC?

**Answer:** `[FromForm]` binds from form fields, query, and route values — the standard HTML form path. `[FromBody]` binds from the HTTP request body using input formatters (typically JSON) — the standard Web API path. They read different parts of the request and use different parsers.

- MVC Razor form POSTs should use default binding or `[FromForm]` — not `[FromBody]`.
- `[FromBody]` requires `Content-Type: application/json` and a JSON-serializable body.
- A single action can combine `[FromRoute] int id` with `[FromForm] MyViewModel model`.
- Mixing them incorrectly produces empty models without obvious errors — the action runs with default values.

---

## Q4. Why does `[FromBody]` fail when posting a standard HTML form?

Why does `[FromBody]` fail when posting a standard HTML form?

**Answer:** Standard HTML forms submit `application/x-www-form-urlencoded` or `multipart/form-data` in the body, but `[FromBody]` expects JSON parsed by the JSON input formatter. The form value provider is bypassed, so the model binder finds no JSON body to deserialize and leaves the model empty.

- Browser `<form method="post">` never sends JSON unless JavaScript intercepts and converts the payload.
- `ModelState` may appear valid because no conversion errors occurred — properties simply stayed at defaults.
- Fix: remove `[FromBody]` for traditional MVC forms; keep it only for AJAX/API endpoints with explicit JSON clients.
- Symmetric trap: posting JSON to an action without `[FromBody]` also yields an empty model.

---

## Q5. How are collection properties bound from form fields (`Lines[0].Sku`)?

How are collection properties bound from form fields (`Lines[0].Sku`)?

**Answer:** Collection binding uses indexed field names — `Lines[0].Sku`, `Lines[0].Qty`, `Lines[1].Sku` — to populate `List<T>` or `T[]` properties. The binder creates list entries at each index and sets nested properties on each element.

- Tag helpers and editor templates generate indexed names automatically for dynamic lists.
- Indices must be **contiguous starting at zero** for reliable binding — gaps cause null/default entries at missing indices.
- Dictionary binding uses similar syntax: `Lines[SKU-123].Qty`.
- After binding, filter empty rows server-side if the UI may submit blank template rows.

---

## Q6. What happens when collection indices are non-contiguous after deleting a row?

What happens when collection indices are non-contiguous after deleting a row?

**Answer:** If the client deletes row index 1 but leaves `Lines[0]` and `Lines[2]`, the binder creates a list with a default/null element at index 1 and data at index 2. Server logic iterating all indices may process phantom rows or misalign SKUs with quantities.

- Empty `LineItemViewModel` at gap indices may pass weak validation and corrupt updates.
- Fix: re-index rows in JavaScript before submit so indices are `0, 1, 2, ...` without gaps.
- Server-side: `model.Lines.Where(l => !string.IsNullOrEmpty(l.Sku))` before processing.
- Prefer stable keys (SKU-based) with a custom binder for complex editable grids.

---

## Q7. How does model binding handle nested objects (`Address.City`)?

How does model binding handle nested objects (`Address.City`)?

**Answer:** Nested object binding uses dotted property paths in form field names — `Address.City`, `Address.PostalCode` — to populate child object properties on the parent ViewModel. The binder instantiates nested types as needed when values are present.

- Tag helpers with `asp-for="Address.City"` emit the correct prefixed names automatically.
- `Html.EditorFor(m => m.Address)` renders templates with proper prefix.
- Partial views must preserve prefix — `<partial for="Address" />` or `ViewData.TemplateInfo.HtmlFieldPrefix = "Address"`.
- Raw `name="City"` in a partial loses the `Address.` prefix and binds to a top-level `City` property instead.

---

## Q8. How do partial views affect model binding prefix for nested properties?

How do partial views affect model binding prefix for nested properties?

**Answer:** Partial views rendered without a prefix inherit only the partial's `@model` property names — `name="City"` instead of `name="Address.City"`. The model binder cannot map unprefixed fields to nested properties on the parent ViewModel.

- Use `<partial name="_AddressEditor" for="Model.Address" />` or `Html.EditorFor(m => m.Address)` to maintain prefix.
- Manually set `ViewData.TemplateInfo.HtmlFieldPrefix = "Address"` before rendering nested partials.
- The `<partial for="...">` tag helper in ASP.NET Core sets prefix correctly for nested binding.
- Misbound nested objects silently remain null/default — a common checkout and profile-form bug.

---

## Q9. What happens when optional nullable fields (`int?`, `DateTime?`) are left empty?

What happens when optional nullable fields (`int?`, `DateTime?`) are left empty?

**Answer:** Empty text inputs for nullable value types bind to `null` — the intended signal for "no value provided." This distinguishes optional fields from explicit zero or minimum-date entries.

- `int? Quantity` with cleared input → `null`, not `0`.
- `DateTime? ShipDate` with cleared `<input type="date">` → `null`, not `DateTime.MinValue`.
- Non-nullable `int`/`DateTime` on optional fields bind empty input to `0`/`0001-01-01` or add conversion errors — use nullable types for optional form fields.
- Check `ModelState` for conversion failures on required non-nullable fields separately.

---

## Q10. How does culture affect date and number binding from form fields?

How does culture affect date and number binding from form fields?

**Answer:** Model binding parses date and number strings using `CultureInfo.CurrentCulture` unless overridden — so `31/01/2026` fails on an `en-US` server expecting `1/31/2026`. Users see "The value is not valid" errors despite typing correctly for their locale.

- `RequestLocalization` middleware sets culture from cookies, query string, or `Accept-Language`.
- Prefer `<input type="date">` posting ISO `yyyy-MM-dd` — culture-invariant for dates.
- Decimal separators differ (`1,5` vs `1.5`) — culture-aware apps must configure supported cultures explicitly.
- API JSON with ISO 8601 dates avoids culture issues; traditional MVC text boxes do not.

---

## Q11. What is `RequestLocalization` and how does it relate to model binding?

What is `RequestLocalization` and how does it relate to model binding?

**Answer:** `RequestLocalization` middleware establishes `CultureInfo.CurrentCulture` and `CurrentUICulture` per request from configured providers. Model binders and Data Annotations use these cultures to parse and format dates, numbers, and currency from form fields.

- Register in `Program.cs`: `app.UseRequestLocalization(options => { options.SupportedCultures = ...; })`.
- Providers include `QueryStringRequestCultureProvider`, `CookieRequestCultureProvider`, and `AcceptLanguageHeaderRequestCultureProvider`.
- Without it, all users inherit the server's default culture — often `en-US` in cloud deployments.
- Display formatting in views and parse binding share the same culture when localization is configured correctly.

---

## Q12. How does file upload binding work (`IFormFile`)?

How does file upload binding work (`IFormFile`)?

**Answer:** `IFormFile` properties on ViewModels bind from file input fields when the form uses `enctype="multipart/form-data"`. The binder captures filename, content type, and stream for uploaded files alongside other form fields in the same POST.

- Use `<input asp-for="UploadedFile" type="file" />` inside a multipart form.
- Multiple files use `IFormFileCollection` or `List<IFormFile>`.
- Validate size (`[RequestSizeLimit]`, `MultipartBodyLengthLimit`), extension whitelist, and scan in production.
- `IFormFile` is null when no file is selected or when enctype is wrong — always null-check before processing.

---

## Q13. What `enctype` is required for file upload forms?

What `enctype` is required for file upload forms?

**Answer:** Forms that include file inputs must set `enctype="multipart/form-data"`. The default `application/x-www-form-urlencoded` cannot carry binary file content — `IFormFile` binds as null even when other text fields bind correctly.

- Tag helper: `<form asp-action="Upload" method="post" enctype="multipart/form-data">`.
- Both metadata fields and file inputs can coexist in one multipart POST.
- Large uploads may need `RequestSizeLimit` or Kestrel `MaxRequestBodySize` configuration.
- Missing enctype is the most common cause of "file upload always null" bugs.

---

## Q14. What is over-posting during model binding and how is it prevented?

What is over-posting during model binding and how is it prevented?

**Answer:** Over-posting is when attackers POST values for properties not shown in the UI — binding sets every matching key on the parameter type. Prevention requires binding to a ViewModel with only allowed properties and mapping explicitly to entities on the server.

- `[Bind(Include = "Name,Email")]` on entities is a partial fix — ViewModels are preferred.
- Never bind POST directly to EF entities with sensitive columns (`IsAdmin`, `Role`, internal pricing).
- `[BindNever]` on specific properties helps but does not replace input model whitelisting.
- Integration-test POSTs with extra fields to verify privileged properties remain unchanged.

---

## Q15. What are `[BindNever]` and `[Bind]` used for?

What are `[BindNever]` and `[Bind]` used for?

**Answer:** `[BindNever]` excludes a property from model binding — inbound POST values are ignored for that property. `[Bind(Prefix = "", Include = "A,B,C")]` or `[Bind(Exclude = "...")]` restricts binding to an explicit allow or deny list on the parameter type.

- `[BindNever]` on navigations and read-only collections prevents graph binding on entities.
- `[Bind(Include = "...")]` on action parameters whitelists bindable members — weaker than dedicated ViewModels.
- `[BindNever]` on ViewModel properties protects server-populated dropdown lists from tampering.
- Security-sensitive POST paths should use ViewModels, not broad `[Bind]` on entities.

---

## Q16. How do checkboxes bind to `bool` and `bool?` properties?

How do checkboxes bind to `bool` and `bool?` properties?

**Answer:** HTML checkboxes submit their value when checked and **omit the key entirely** when unchecked. For non-nullable `bool`, ASP.NET Core treats a missing key as `false`. Tag helpers for `bool` add a hidden `false` input so unchecked explicitly posts `false`.

- `bool AcceptedTerms`: standard helper posts hidden `false` + checkbox `true` — unchecked yields `false`.
- `bool? Newsletter`: tri-state (`null` = unchanged) breaks when the hidden-false pattern forces `false` instead of `null`.
- For tri-state nullable booleans, use manual markup without the hidden companion or an enum (`Unchanged`, `OptIn`, `OptOut`).
- `[Required]` on non-nullable `bool` checkboxes fails to work as expected — use `[Range(typeof(bool), "true", "true")]` for required consent.

---

## Q17. What is the hidden-field pattern for checkboxes?

What is the hidden-field pattern for checkboxes?

**Answer:** The checkbox tag helper renders a hidden input with `false` before the checkbox input with `true` — both share the same name. Unchecked forms submit only the hidden `false`; checked forms submit both (the checkbox value overrides), ensuring non-nullable `bool` properties always receive an explicit true or false.

- Without the hidden field, unchecked checkboxes would not appear in the POST at all — fine for `bool?` tri-state, wrong for required `bool`.
- Pattern: `<input type="hidden" name="IsActive" value="false" /><input type="checkbox" name="IsActive" value="true" />`.
- Do not apply this pattern to `bool?` when `null` means "no change" — it forces `false` on unchecked.
- Required legal consent checkboxes rely on this pattern plus server-side validation for `true`.

---

## Q18. What is a custom `IModelBinder` and when would you create one?

What is a custom `IModelBinder` and when would you create one?

**Answer:** A custom `IModelBinder` implements `BindModelAsync` to convert non-standard request data into a model property or parameter type that built-in binders cannot handle. Register it via `IModelBinderProvider` for types like custom value objects, composite keys, or culture-specific currency parsing.

- Implement `IModelBinder` with **stateless** logic — no instance fields storing request data across binds.
- Register through `ModelBinderProvider` in `AddControllersWithViews(options => options.ModelBinderProviders.Insert(0, new MyProvider()))`.
- Use cases: binding `Lines[SKU-A].Qty` dictionaries, parsing `"$1,234.56"` currency strings, combining split fields into one type.
- Avoid singleton registration of stateful binders — concurrent requests corrupt shared fields under load.

---

---

> **Target:** ASP.NET Core 8 MVC

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

---

**Answer:**

_Answer not found._

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

---

**Answer:**

_Answer not found._

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

---

**Answer:**

_Answer not found._

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

---

**Answer:**

_Answer not found._

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

---

**Answer:**

_Answer not found._

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

---

**Answer:**

_Answer not found._

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

---

**Answer:**

_Answer not found._

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

---

**Answer:**

_Answer not found._

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

---

**Answer:**

_Answer not found._

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

---

**Answer:**

_Answer not found._

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

User leaves Newsletter unchecked ( wants unchanged ) but AcceptedTerms checked. Saved newsletter becomes `false` instead of unchanged.

---

**Answer:**

_Answer not found._

---
