# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `06. ASP.NET Core MVC/06. Model Binding in MVC`

---

#### Q1. (R) Review this MVC create action. Form fields submit but `ProductName` and `Price` arrive empty.

```csharp
[HttpPost]
public IActionResult Create([FromBody] ProductEditViewModel model)
```

**Answer:** `[FromBody]` tells the model binder to read **JSON from the request body**, not `application/x-www-form-urlencoded` form fields — a classic MVC mistake when copying Web API patterns. The binder produces an empty/default model; `ModelState` may still appear valid because no values failed conversion.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Binding source | `[FromBody]` on HTML form POST | Form keys ignored — silent empty model |
| MVC vs API | API convention applied to Razor form | Works in Postman with JSON, fails in browser |
| Validation | Empty strings/zeros may pass | Invalid products saved or weak validation only |

**Fix (priority order):**

1. Remove `[FromBody]` — for MVC `Controller` POST actions, complex types bind from **form** by default: `public IActionResult Create(ProductEditViewModel model)`.
2. Or use `[FromForm]` explicitly when mixing sources (e.g., route id + form model).
3. Keep `[FromBody]` only for JSON/AJAX endpoints — pair with `contentType: 'application/json'` on the client.

**Production takeaway:** MVC form posts and Web API JSON posts use different default binders — Karat tests whether you recognize `[FromBody]` on a `<form method="post">` as an immediate red flag.

---

#### Q2. (R) Review line-item editing. Deleting a middle row causes wrong SKU updates after POST.

**Answer:** Collection binders map indexed form keys (`Lines[0]`, `Lines[2]`) into a **list with gaps** — index 1 becomes a default/null entry, so `foreach` may process empty SKUs or shift logic that assumes contiguous indices. The UI removed row 1 but left index 2, creating a sparse collection.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Collection binding | Non-contiguous indices (`0`, `2` skip `1`) | List contains null/default `LineItemViewModel` at index 1 |
| UI sync | Client-side row delete without re-indexing | Server receives stale index scheme |
| Business logic | `foreach` over all slots | Applies qty to empty/wrong SKU |

**Fix (priority order):**

1. **Re-index on the client** before submit — JavaScript renames fields to `Lines[0]`, `Lines[1]`, … contiguously.
2. Filter server-side: `model.Lines.Where(l => !string.IsNullOrEmpty(l.Sku))` before applying updates.
3. Prefer **stable keys** instead of positional indices: `Lines[SKU-WIDGET-A].Qty` via custom binder or hidden `Lines[i].Sku` with server-side dictionary merge.
4. Use Tag Helpers / editor templates that regenerate indices when rows are added/removed dynamically.

**Production takeaway:** Collection binding assumes contiguous `prefix[index].property` names — gaps are a top MVC binding failure mode in editable grids.

---

#### Q3. (R) Review nested address binding. `ShippingCity` works; `Address.City` is always null.

**Answer:** Partial `_AddressEditor.cshtml` renders `name="City"` instead of **`name="Address.City"`** (or uses `HtmlFieldPrefix`). The model binder looks for top-level `City`, not nested `Address.City`, so the nested object stays default while the unrelated top-level field binds.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Prefix | Missing `Address.` on field names | Nested properties never bound |
| Partial views | `asp-for="City"` on child model loses parent prefix | Common EditorFor/PartialAsync trap |
| Data integrity | Shipping address saved empty | Checkout completes with missing address |

**Fix (priority order):**

1. Use **`@await Html.EditorFor(m => m.Address)`** or `<partial name="_AddressEditor" for="Address" />` so Tag Helpers emit `Address.City`, `Address.PostalCode`.
2. Or set prefix manually: `@Html.Prefix("Address")` / `ViewData.TemplateInfo.HtmlFieldPrefix = "Address"` before rendering inputs.
3. Avoid raw `name="City"` on nested partials — always prefix-aware helpers.

**Production takeaway:** Complex-type binding in MVC is **prefix-driven** — field names must reflect the object graph path, not just property names on the partial's model.

---

#### Q4. (R) Review optional numeric and date fields. Cleared inputs produce `0` and `0001-01-01` instead of null.

**Answer:** If the view model used **non-nullable** `int` and `DateTime`, empty text boxes fail conversion or bind as `default` — the question's nullable types should bind empty strings to `null`, but if developers changed to non-nullable types (or `[Required]` with empty string), they get `0` / `DateTime.MinValue`. Even with `int?`, some custom templates post `"0"` or whitespace, causing subtle bugs.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Nullability | Non-nullable value types on optional fields | Empty input → `0` or `0001-01-01` |
| Conversion | Empty string to value type | ModelState error **or** silent default depending on type |
| UX | `type="date"` cleared may omit key vs post empty | Inconsistent null vs error |

**Fix (priority order):**

1. Use **`int?`**, **`DateTime?`**, **`decimal?`** for optional form fields — empty → `null`.
2. Check `ModelState` for conversion errors on required non-nullable fields; show field-level messages.
3. For dates, prefer `<input type="date">` with nullable `DateTime?` — browser sends ISO `yyyy-MM-dd` or omits key.
4. Server-side: treat `default(DateTime)` as invalid if business rules forbid it — do not rely on "magic" dates.

**Production takeaway:** Nullable value types exist largely for **form binding semantics** — Karat distinguishes "optional field" (`null`) from "user entered zero" (`0`).

---

#### Q5. (R) Review culture-sensitive date binding. UK `31/01/2026` fails on `en-US` server.

**Answer:** Model binding uses **`CultureInfo.CurrentCulture`** (unless overridden) to parse date strings — `en-US` expects `1/31/2026`, so `31/01/2026` fails conversion and adds a ModelState error. Without `RequestLocalization`, all users inherit server culture.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Culture | `dd/MM/yyyy` posted, `en-US` parser | Binding failure — `EventDate` invalid |
| Localization | No `UseRequestLocalization()` | Every user forced to server format |
| Errors | Generic "The value '31/01/2026' is not valid" | Support tickets; users blame the form |

**Fix (priority order):**

1. Add **`RequestLocalization`** middleware with supported cultures (`en-GB`, `en-US`) and culture providers (cookie, `Accept-Language`).
2. Prefer **ISO 8601** in UI: `<input type="date">` posts `yyyy-MM-dd` — culture-invariant.
3. For text dates, use **`[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]`** + explicit format in binder or FluentValidation with culture-aware parse.
4. Never assume server culture matches user locale in multi-region deployments.

**Production takeaway:** MVC text-box date binding is culture-coupled — API JSON ISO dates avoid this; traditional forms do not.

---

#### Q6. (R) Review document upload. `UploadedFile` is always null; other fields bind.

**Answer:** Standard `<form method="post">` defaults to **`application/x-www-form-urlencoded`**, which cannot carry file bytes — `IFormFile` requires **`enctype="multipart/form-data"`**. Text fields bind from the multipart body, but without correct enctype the file part is never sent.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Form encoding | Missing `multipart/form-data` | File stream never reaches model binder |
| Binding | `IFormFile` property on view model | Always null — upload appears "broken" |
| Security | (After fix) no size/type validation | DoS or malicious uploads |

**Fix (priority order):**

1. Add **`enctype="multipart/form-data"`** — Tag Helper `<form asp-action="Upload" method="post" enctype="multipart/form-data">` or `asp-form` equivalent.
2. Validate `model.UploadedFile` — size limit via `[RequestSizeLimit]`, extension whitelist, virus scan in production.
3. Match **`name`** to property (`UploadedFile`) or use `asp-for="UploadedFile"` on `<input type="file">`.
4. For large files, consider streaming upload endpoint separate from metadata form.

**Production takeaway:** File upload binding failures are almost always **missing multipart enctype**, not `IFormFile` configuration.

---

#### Q7. (R) Review user profile update for over-posting. Non-admin can set `IsAdmin` via dev tools.

**Answer:** Binding **`User` EF entity** directly exposes every property to the form post — attackers add `IsAdmin=true` even if Razor omits the field. `[BindNever]` on `CreatedUtc` does not protect `IsAdmin`; manual copy **`user.IsAdmin = model.IsAdmin`** applies the attack. This is **over-posting / mass assignment**.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Over-posting | Entity as action parameter | Hidden fields bind attacker-controlled values |
| `[BindNever]` | Only on `CreatedUtc` | Sensitive `IsAdmin` still bindable |
| Design | Copy-all from bound entity | Privilege escalation |

**Fix (priority order):**

1. Use a **ViewModel** with only editable fields (`DisplayName`, `Email`) — never bind `IsAdmin` on user-facing forms.
2. Or **`[Bind(Prefix = "", Include = "DisplayName,Email")]`** on parameter — explicit allow-list (prefer ViewModel).
3. Map ViewModel → entity in the action; **never** assign role flags from user POST.
4. `[BindNever]` on navigation/metadata properties on entities used in other scenarios.

**Production takeaway:** MVC binding is **opt-out by default** for posted keys — security requires input models that expose only what the user may change.

---

#### Q8. (R) Review registration flow. Invalid data reaches the database despite validation attributes.

**Answer:** DataAnnotations populate **`ModelState`** during binding/validation, but the action **never checks `ModelState.IsValid`** — unlike `[ApiController]`, MVC controllers do **not** auto-return 400. Client-side validation is bypassable; server-side attributes run but are ignored by the action.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Validation gate | Missing `if (!ModelState.IsValid) return View(model);` | Invalid emails/passwords persisted |
| Client-only trust | jQuery unobtrusive validation | Disabled in browser → server must enforce |
| UX | Summary in view but action proceeds | Users see errors yet account created (race/retry) |

**Fix (priority order):**

1. Add server gate at top of every mutating POST:

```csharp
[HttpPost]
public IActionResult Register(RegisterViewModel model)
{
    if (!ModelState.IsValid)
        return View(model);
    // ...
}
```

2. Re-display field errors with `<span asp-validation-for="Email">` and summary.
3. Keep client validation for UX only — **never** as sole enforcement.
4. For APIs in same app, `[ApiController]` auto-validation differs from MVC — do not mix patterns blindly.

**Production takeaway:** **`ModelState.IsValid` ignored** is one of the most common MVC production bugs — attributes validate; the action must act on results.

---

#### Q9. (M) jQuery POST sends JSON; MVC action expects form binding — fields never bind.

**Answer:** The jQuery call sets **`contentType: 'application/json'`** and JSON body — MVC `Controller` action without `[FromBody]` uses **form value providers**, not JSON input formatter. The binder finds no form keys; model stays default. This is the inverse of Q1 (form posted to `[FromBody]`).

| Client sends | Server expects | Result |
|---|---|---|
| JSON body + `application/json` | Form fields (default MVC) | Empty model |
| `application/x-www-form-urlencoded` | `[FromBody]` DTO | Empty model |
| JSON body | `[FromBody] SiteSettingsViewModel` | Binds correctly |

**Fix (priority order):**

1. **Option A — JSON API style:** Add `[FromBody]` and ensure `AddControllers().AddJsonOptions(...)` — treat as mini-API endpoint; return `Json()` or status codes.
2. **Option B — traditional MVC:** Change client to form encoding:

```javascript
$.post('/Admin/UpdateSettings', {
    siteName: $('#siteName').val(),
    maxUsers: $('#maxUsers').val()
}); // jQuery sends x-www-form-urlencoded by default
```

3. Document binding contract per endpoint — AJAX partial updates in MVC often need explicit `[FromBody]` **or** form serialize.
4. See also: `05. ASP.NET Core/08. Model Binding & Validation` for JSON/camelCase API traps — MVC AJAX hits the same formatter rules once `[FromBody]` is used.

**Production takeaway:** jQuery vs form post is a **Content-Type and `[From*]` alignment** problem — not "model binding is broken."

---

#### Q10. (P) Custom `CurrencyModelBinder` registered as singleton with instance field — what breaks under load?

**Answer:** **`IModelBinder` implementations must be stateless`** — registering as **singleton** shares one instance across all concurrent requests. Mutating `_lastCulture` causes cross-request corruption (request A's culture read by request B's logging or logic), a data race under load.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Lifetime | Singleton binder with instance state | Race conditions; wrong culture in logs/decisions |
| Thread safety | `_lastCulture` field written per bind | Undefined behavior under concurrency |
| DI | Wrong service registration | Hard-to-reproduce intermittent bugs |

**Fix (priority order):**

1. **Remove instance state** — use only `bindingContext` and local variables inside `BindModelAsync`.
2. Register binder via **`ModelBinderProvider`** (typically transient/scoped factory pattern), not `AddSingleton<IModelBinder, ...>` for stateful binders.
3. If logging culture, log from `bindingContext.HttpContext.Features.Get<IRequestCultureFeature>()` locally — do not store on fields.
4. Follow Microsoft pattern: `IModelBinderProvider` returns new binder instances or reusable **stateless** singleton.

```csharp
public Task BindModelAsync(ModelBindingContext bindingContext)
{
    var culture = bindingContext.HttpContext.Features
        .Get<IRequestCultureFeature>()?.RequestCulture.Culture ?? CultureInfo.CurrentCulture;
    var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).FirstValue;
    // parse with local culture only — no fields
}
```

**Production takeaway:** Custom model binders run on **every form post** at scale — treat them like middleware: stateless, thread-safe, no captured request data on fields.

---

#### Q11. (R) Review checkbox pattern for `bool?` newsletter. Unchecked box saves `false` instead of unchanged (`null`).

**Answer:** HTML checkboxes **submit nothing when unchecked** — for non-nullable `bool`, absent key → `false`. For **`bool?`**, absent key also binds as **`null`** in ASP.NET Core, which is correct for tri-state — but Tag Helpers for `bool` generate a **hidden `false` input** paired with the checkbox so unchecked posts `false`. That hidden field **breaks tri-state semantics**: unchecked sends `false`, not omission → server interprets as explicit opt-out instead of unchanged.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Checkbox semantics | Hidden input + `bool?` tri-state | Unchecked → `false`, not `null` |
| PATCH-style forms | `null` = unchanged | Marketing opt-out recorded incorrectly |
| HTML limitation | Checkboxes omit key when off | Framework compensates with hidden field for `bool` |

**Fix (priority order):**

1. For **tri-state `bool?`**, do **not** use default `asp-for` checkbox helper — use manual markup: one checkbox named `Newsletter` **without** hidden false companion, and server treats missing key as `null`.
2. Or separate fields: `NewsletterChoice` enum (`Unchanged`, `OptIn`, `OptOut`) — clearer in forms than nullable bool.
3. For required **`bool AcceptedTerms`**, keep standard Tag Helper pattern (hidden false + checkbox) so unchecked is explicit `false` and validation can require `true` via `[Range(typeof(bool), "true", "true")]` or custom validation.
4. Align with API PATCH guidance in `05. ASP.NET Core/08. Model Binding & Validation` Q2 — same tri-state cardinality issue, different HTML surface.

**Production takeaway:** MVC checkbox binding **defaults to bool**, not nullable intent — tri-state flags need custom markup or enums; binding failures here are **semantic**, not parser errors.

---
