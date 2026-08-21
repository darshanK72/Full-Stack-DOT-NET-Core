# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `06. ASP.NET Core MVC/08. Tag Helpers`

---

#### Q1. (R) Review this strongly typed edit view. QA reports validation and wrong property updates.

```cshtml
@model Customer
<label asp-for="Email"></label>
<input asp-for="Email" />
<label asp-for="FullName"></label>
<input asp-for="FullName" />
```

**Answer:** The view declares `@model Customer` but the screen is an edit form for `EditCustomerViewModel` semantics — `asp-for` generates `name`/`id` from the **declared** model type, so validation metadata on the view model never participates and posted field names may not match what the action expects.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Model mismatch | `@model Customer` vs view-model validation attributes on `EditCustomerViewModel` | `[Required]` / `[EmailAddress]` on view model ignored — no client or server validation messages |
| Binding | `asp-for="Email"` binds to `Customer.Email` naming | May work if property names align, but `FullName` on entity vs `DisplayName` on view model diverges on real forms |
| Design | Entity used as edit surface | Over-posting risk — extra `Customer` columns bind on POST; view model should isolate editable fields |
| Validation UI | `asp-validation-for="Email"` reads `ModelState` keys from generated names | Keys follow declared model; mismatched action parameter type yields empty spans |

**Fix (priority order):**

1. Change to `@model EditCustomerViewModel` and map entity ↔ view model in the GET/POST actions (`EditCustomerViewModel` in, map to `Customer` in service layer).
2. Align property names: use `DisplayName` on the view model with `asp-for="DisplayName"`, not `FullName` on the entity.
3. Keep `[ValidateAntiForgeryToken]` on POST — form tag helper emits token when model is correct.
4. Return view with same view model on validation failure so `ModelState` keys match generated field names.

**Production takeaway:** `asp-for` is not magic — it reflects the `@model` type and expression tree. Wrong model type is the #1 Tag Helper "validation doesn't work" report in MVC migrations.

---

#### Q2. (P) Migrating MVC 5 views to Tag Helpers — strategy, global `@addTagHelper` risks, when to keep HTML Helpers.

**Answer:** Migrate incrementally: register Tag Helpers in `_ViewImports.cshtml`, convert high-churn views first, and use `!` opt-out or `@removeTagHelper` on legacy partials until refactored — HTML Helpers remain fully supported and compile side-by-side.

- **Registration:** `@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers` in `Views/_ViewImports.cshtml` (and Areas) enables built-ins project-wide; custom helpers need `@addTagHelper *, YourAssembly`.
- **Migration order:** Layout/navigation (`<environment>`, `<script asp-append-version>`) → forms (`<form>`, `asp-for`, validation spans) → anchors (`asp-controller` / `asp-action`) → leave complex `@Html.EditorFor` templates until a template helper exists.
- **Global enable risks:** Tag Helpers rewrite matching elements — e.g. `<form>` always gets antiforgery hidden field; `<a>` with `asp-*` changes `href` generation; accidental `<input class="form-control">` without `asp-for` is fine, but duplicate patterns can surprise teams expecting literal HTML.
- **Opt-out:** Prefix attribute with `!` (`<input !name="x" />`) or `@removeTagHelper` in a folder's `_ViewImports` for plugin-heavy legacy markup.
- **Keep HTML Helpers when:** `Html.EditorFor`/`DisplayFor` templates are deeply customized; dynamically built HTML in C# (`HtmlHelper` in code); or third-party attributes that conflict with Tag Helper targeting.
- **Testing:** Snapshot rendered HTML per view — Tag Helpers change `name`, `id`, `value`, and `data-val-*` attributes compared to MVC 5 output.

**Production takeaway:** Karat tests pragmatic migration judgment — coexistence and opt-out, not "delete all `Html.*` day one."

---

#### Q3. (M) Custom `HighlightTagHelper` must run after built-in helpers — review process order.

```csharp
[HtmlTargetElement("highlight")]
public class HighlightTagHelper : TagHelper { /* ... */ }
```

**Answer:** Tag Helpers on the **same element** run by ascending `Order` (lower first); built-in MVC helpers use negative orders, so a custom `[HtmlTargetElement("input")]` at default `0` runs **after** `InputTagHelper` — but this snippet also blocks on `.Result` and wraps inputs in a way that fights maintainability.

- **Order rules:** `Order` applies among helpers targeting the **same tag**. Built-in `InputTagHelper` uses a low (negative) order; a custom input helper that mutates attributes should set `Order = 1000` to run **after** built-ins finish generating `name`, `id`, and `data-val-*`.
- **Wrapper elements:** When `<highlight>` calls `GetChildContentAsync()`, child tag helpers (including `asp-for` on the inner `<input>`) execute **first** — parent order does not skip that. The bug here is calling `.Result` synchronously inside `Process` instead of overriding `ProcessAsync`.
- **Problem in snippet:** Sync-over-async via `.Result` can deadlock or starve threads under load; `SetHtmlContent` with string replace may corrupt encoded markup.
- **Better design:** Target display elements (`[HtmlTargetElement("p", Attributes = "highlight-term")]`) instead of wrapping editors, or run after built-ins only when sharing the same `input` tag.
- **Child content:** Use `await output.GetChildContentAsync()` in `ProcessAsync`.

**Fix (priority order):**

1. Implement `ProcessAsync` and `await GetChildContentAsync()` — remove `.Result`.
2. Set explicit `Order = 1000` (or document interaction) so built-in `InputTagHelper` completes first.
3. Avoid wrapping `<input asp-for>` — apply highlight to read-only display markup.

**Production takeaway:** Tag Helper **Order** is execution priority, not DOM nesting — Karat uses custom helpers to test whether you know built-ins win by default.

---

#### Q4. (R) Navigation partial — links 404, wrong controller, dropped route values.

**Answer:** Anchor Tag Helper generates URLs from **route values + current ambient values** — omitting `asp-controller` inherits the current controller; mismatched attribute-route parameters produce invalid links; plain `href` bypasses routing entirely.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Ambient values | `asp-action="Archive"` without `asp-controller` | Inherits `Orders` from current request — OK here, but on `/Home/Index` would target `HomeController.Archive` (404) |
| Missing controller | `asp-controller="Admin"` with no Admin controller/area | 404 in staging |
| Attribute routing | `Monthly` needs `{year}` and `{month}` — second link omits `month` | URL generation fails or route mismatch — 404 |
| Raw href | `href="/reports/monthly"` vs `[Route("reports/{year:int}/{month:int}")]` | Hard-coded path does not match attribute template — 404 |
| Route value types | `asp-route-year="2025"` string coerces to int | Usually OK; wrong culture/format breaks `{year:int}` |

**Fix (priority order):**

1. Use `asp-controller`, `asp-action`, and **all** required route tokens: `asp-route-year` + `asp-route-month` for attribute-routed actions.
2. Replace hard-coded `href` with Tag Helper or `Url.Action()` so link generation matches route table.
3. For areas, add `asp-area="Admin"` when applicable.
4. Add integration tests that assert generated URLs from a Razor view or `IUrlHelper`.

**Production takeaway:** `asp-action` alone is a common staging bug — ambient controller from the current page is inherited silently.

---

#### Q5. (P) Stale JavaScript after deploy — `<environment>` and `asp-append-version` in Production.

**Answer:** In Production, `asp-append-version="true"` appends a file hash query string (`?v=...`) so browsers fetch new static files after deploy; Development serves unminified files without fingerprinting for fast iteration — stale assets usually mean Production tags are not used or fingerprinting is missing on the served file.

- **`<environment include="Development">`:** Only renders when `IHostEnvironment.EnvironmentName` is Development — serves raw `site.js` without cache bust.
- **`<environment exclude="Development">`:** Renders in Staging/Production — must include `asp-append-version="true"` on script/link tags that should bust cache.
- **Wrong `ASPNETCORE_ENVIRONMENT`:** Server set to Development in prod → unversioned scripts served → long browser cache on CDN/proxy → users see old JS until hard refresh.
- **Static files middleware:** `app.UseStaticFiles()` must run; fingerprint requires file on disk — if deploy swaps files but CDN caches old query string URL, purge CDN.
- **CDN absolute URLs:** `asp-append-version` works on `src`/`href` that resolve via application static files; for absolute CDN URLs (`https://cdn.example.com/lib.js`), append-version does not hash remote files — use CDN provider versioning or copy assets locally.
- **CSS in snippet:** Layout correctly uses `asp-append-version` outside `<environment>` — good pattern for all environments.

**Production takeaway:** Environment tag helpers control **which markup exists**, not server headers — pair Production bundle tags with `asp-append-version` and verify environment variable on the host.

---

#### Q6. (R) AJAX form — Antiforgery validation failed on `fetch` POST.

**Answer:** The `<form>` Tag Helper emits a hidden `__RequestVerificationToken` field on full submit, but the JavaScript `fetch` builds `FormData` from the form **without** ensuring the token header or field is sent the way `[ValidateAntiForgeryToken]` expects — or the token field is omitted when JS constructs the body incorrectly.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Antiforgery | `fetch` POST without `RequestVerificationToken` header | 400 antiforgery failure |
| Form helper | Token rendered inside `<form>` | Present in DOM but `FormData` may omit if form incomplete or cloned incorrectly |
| AJAX pattern | Manual URL `/Tasks/Create` vs `asp-action` | Bypasses link generation; still OK if token sent |
| Validation | `[ValidateAntiForgeryToken]` on action | Correct for MVC — API-style endpoints would use different pattern |

**Fix (priority order):**

1. Include token in request: read `$('input[name="__RequestVerificationToken"]').val()` or from `FormData` and set header `RequestVerificationToken` (or ensure hidden input is inside the form `FormData` captures).
2. Prefer `@Html.AntiForgeryToken()` inside form if not using `<form>` tag helper — tag helper adds automatically for POST forms with `asp-action`.
3. For SPA-heavy pages, consider `[IgnoreAntiforgeryToken]` only with alternative CSRF defense — not default for MVC forms.
4. Use `@inject Microsoft.AspNetCore.AntiForgery.IAntiforgery` to generate token in layout for global JS helper.

```javascript
const token = document.querySelector('#createForm input[name="__RequestVerificationToken"]').value;
await fetch('/Tasks/Create', {
    method: 'POST',
    headers: { 'RequestVerificationToken': token },
    body: new FormData(e.target)
});
```

**Production takeaway:** Form Tag Helper generates the token — AJAX must **transmit** it; Karat stacks form helper behavior with JavaScript integration gaps.

---

#### Q7. (R) Duplicate `id` attributes in looped partial — accessibility and validation.

**Answer:** `asp-for="Quantity"` auto-generates `id="Quantity"` and `name` with prefix; manually setting `id="Quantity"` duplicates IDs when the partial renders multiple times — labels and client-side validation attach to the first element only.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| HTML validity | Same `id` repeated per line item | Invalid DOM — accessibility tools fail |
| Tag Helper | `asp-for` always emits `id` from expression | Manual `id="Quantity"` duplicates generated id |
| Labels | `for="Quantity"` static | Only first input associated |
| Model binding | Repeated `name="Quantity"` without index prefix | Only last value or ambiguous binding unless indexed `Lines[i].Quantity` |

**Fix (priority order):**

1. Remove manual `id` — let `asp-for` generate unique ids, or use indexed model: `asp-for="Quantity"` on `@model` with collection index in parent: `Lines[i].Quantity` in editor template.
2. Use **EditorTemplates** with `@Html.EditorFor(m => m.Lines)` or `<partial>` with indexed `ViewData.TemplateInfo.HtmlFieldPrefix`.
3. Replace static `for="Quantity"` with `<label asp-for="Quantity">` — matches generated id.
4. For collections, use `for (var i = 0; i < Model.Lines.Count; i++)` and `asp-for="Lines[i].Quantity"`.

**Production takeaway:** Never combine manual `id` with `asp-for` on the same element — Karat tests duplicate-id loops in ecommerce line editors.

---

#### Q8. (P) Tag Helpers in Razor Class Library — registration in RCL and consuming app.

**Answer:** Tag Helpers in an RCL are **not** discovered automatically by consuming apps — the RCL must expose them via `_ViewImports.cshtml` with `@addTagHelper`, and consumers must import that RCL's tag helper assembly (or rely on merged `_ViewImports` from RCL views only).

- **In RCL:** Create `Areas/Common/_ViewImports.cshtml` or root `Pages/_ViewImports.cshtml` / `Views/_ViewImports.cshtml` inside the RCL with:
  ```cshtml
  @addTagHelper *, MyCompany.Ui.Rcl
  ```
  The assembly name must match the DLL (`MyCompany.Ui.Rcl`), not only the namespace.
- **Tag helper class:** `[HtmlTargetElement("summary-card")]` public class in RCL, public parameterless ctor.
- **Consuming MVC app:** Reference RCL project; for **views defined in the app** using RCL tag helpers, add the same `@addTagHelper *, MyCompany.Ui.Rcl` to the app's `Views/_ViewImports.cshtml`.
- **Pitfall:** `_ViewImports` scope is hierarchical — RCL `_ViewImports` applies to RCL **embedded views only**, not app views. Developers reference the RCL but forget app-level `@addTagHelper`.
- **Pitfall 2:** Wrong assembly string (`MyCompany.Ui` vs `MyCompany.Ui.Rcl`) — silent no-op, raw custom tags render.
- **Discovery:** Tag helpers are compile-time registered via `@addTagHelper` directives, not runtime assembly scanning of all references.

**Production takeaway:** RCL packaging splits **view imports scope** — Karat tests whether you register helpers in the consumer, not only the library.

---

#### Q9. (M) Mixed `!` opt-out and Tag Helpers — when correct vs silent binding failure.

```cshtml
<input !type="text" !name="legacySku" !id="legacySku" value="@Model.Sku" />
<input asp-for="Sku" class="form-control" />
```

**Answer:** The `!` prefix opts **that attribute** out of Tag Helper processing so legacy `name`/`id`/`type` pass through literally — but duplicating the same logical field with both legacy and `asp-for` creates two inputs posting different names (`legacySku` vs `Sku`), and only one binds cleanly to the view model.

- **`!` opt-out:** Prevents Tag Helpers from rewriting the attribute — correct for jQuery plugins that require fixed DOM ids.
- **Double field problem:** Two inputs for one concept — model binder receives `legacySku` (parameter) and `Sku` (view model property); UI confusion about source of truth.
- **Label opt-out:** `<label !for="legacySku">` is correct when input uses literal `for`/`id` pairing outside `asp-for`.
- **When `!` is wrong:** Using `!` on `asp-for` (invalid) or opting out `name` then expecting `[Required]` on view model property — validation metadata tied to `asp-for` generated name, not legacy name.
- **Silent failure:** POST succeeds but `Sku` is empty while `legacySku` populates — business logic reading `model.Sku` sees stale data.

**Fix (priority order):**

1. Single source of truth: either legacy **or** `asp-for`, not both for the same property.
2. If plugin requires fixed id, use `asp-for="Sku"` and customize with `ViewData["htmlAttributes"]` or `IHtmlGenerator` — or one opted-out input and map `legacySku` → `Sku` in action manually (document the bridge).
3. Use `[Bind(Prefix = "")]` carefully — prefer view model property names aligned with `asp-for`.

**Production takeaway:** `!` is per-attribute escape hatch — Karat tests that you understand it disables helper rewriting, not validation or binding conventions.

---
