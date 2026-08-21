# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `06. ASP.NET Core MVC/14. Client-Side Validation`

---

#### Q1. (R) Review this registration action and Razor view. QA says the form "validates fine" in the browser, but attackers can create accounts with empty passwords.

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

#### Q2. (R) Review this layout and create form. Fields show `data-val-*` attributes but nothing happens on blur or submit.

**Answer:** Tag helpers emit `data-val-*` attributes for unobtrusive adapters, but **`jquery.validate.unobtrusive.min.js` is required** to read those attributes and attach rules to jQuery Validate — loading only `jquery.validate.min.js` leaves inert metadata in the DOM.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Script pipeline | Missing unobtrusive bridge | No client rules wired despite `data-val="true"` |
| UX | Users only see errors after full POST | Perceived "validation broken" |
| Maintenance | Partial script set | Hard-to-spot regression when trimming bundles |

**Fix (priority order):**

1. Include both scripts in order: `jquery.validate.min.js`, then `jquery.validate.unobtrusive.min.js` — use `_ValidationScriptsPartial` which loads both.
2. Ensure jQuery loads before validation scripts in `_Layout.cshtml` or the `@section Scripts`.
3. Verify with dev tools: `$('form').valid()` should return false on empty required fields.

**Production takeaway:** `data-val-*` without unobtrusive JS is a silent half-setup — common when optimizing bundles without understanding the two-library contract.

---

#### Q3. (P) Custom `[MustBeFutureDate]` works server-side but not in the browser. What is the extension point?

**Answer:** MVC client validation requires a **`IClientModelValidator` adapter** (or `ClientModelValidatorProvider`) registered in `AddMvc`/`AddControllersWithViews` options that translates your attribute into `data-val-*` attributes and optional custom adapter logic — data annotations alone do not auto-generate client rules for custom attributes.

- Implement `IClientModelValidator` for `MustBeFutureDateAttribute`, emitting attributes such as `data-val-mustbefuturedate` and `data-val-mustbefuturedate-message`.
- Register: `options.ModelBindingMessageProvider` for messages if needed; add adapter via `options.ModelValidatorProviders` or `[Attribute]` implementing `IClientModelValidator`.
- Add a small jQuery Validate **custom method** (or `$.validator.unobtrusive.adapters.add`) that reads the `data-val-*` hook and mirrors server logic.
- Server `ValidationAttribute` remains the source of truth; client rule is a best-effort mirror.

**Production takeaway:** Custom business rules need explicit client adapters — Karat distinguishes "annotation exists" from "unobtrusive pipeline wired."

---

#### Q4. (R) Review username `[Remote]` check. Duplicate accounts when users submit quickly.

**Answer:** `[Remote]` validation is **asynchronous** — jQuery Validate may allow form submit before the remote AJAX call completes, so the server receives a username that failed availability only milliseconds later.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Race | Submit before remote response | Duplicate or invalid usernames slip through |
| Server trust | Remote endpoint not re-checked on POST | Client-only availability assumption |
| UX | No pending-state on submit button | Fast typists hit the race often |

**Fix (priority order):**

1. **Always** re-validate username availability in the POST action (or service) — treat `[Remote]` as UX hint only.
2. Disable submit while remote validation is in flight: hook `$.validator` `remote` pending state or debounce blur events.
3. Consider server-side uniqueness constraint (DB unique index) as final enforcement.

**Production takeaway:** Remote validation is never a lock — same class of bug as client-only validation, with an async timing twist.

---

#### Q5. (R) Review edit form. Server returns validation errors but property errors never appear in the summary area.

**Answer:** `asp-validation-summary="ModelOnly"` renders **only model-level** errors (non-field keys), not property errors like `ProductName` or `UnitPrice` — field failures appear only in adjacent `asp-validation-for` spans, so the summary `<div>` stays empty when only property rules fail.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Summary mode | `ModelOnly` excludes property errors | Users look at empty summary banner |
| UX | Errors split between summary and spans | Confusing layout if spans are off-screen |
| Client vs server | Same summary mode affects both | Client unobtrusive also respects summary placement |

**Fix (priority order):**

1. Use `asp-validation-summary="All"` to list property errors in the summary, or keep `ModelOnly` and ensure every field has a visible `asp-validation-for` span.
2. For a single banner UX, prefer `All` plus CSS to style the list once at the top.
3. Add model-level errors with `ModelState.AddModelError(string.Empty, "...")` only when using `ModelOnly`.

**Production takeaway:** Validation summary enum (`All` | `ModelOnly` | `None`) controls aggregation — a frequent Razor misunderstanding in code review.

---

#### Q6. (P) Disable client validation in Production, enable in Development via `ClientValidationEnabled`. Review approach.

**Answer:** Setting `ViewContext.ClientValidationEnabled` from `IWebHostEnvironment` is a valid pattern — when `false`, tag helpers **omit `data-val-*` attributes**, so unobtrusive scripts have nothing to attach; server validation still runs on POST if the action checks `ModelState`.

- Place the flag in `_ViewImports.cshtml` (or a base view) so all tag helpers respect it per request.
- Loading `_ValidationScriptsPartial` in Production is harmless but wastes bytes when attributes are absent — optionally conditionally render scripts: `@if (ViewContext.ClientValidationEnabled) { ... }`.
- Do **not** disable server validation in Production — only client metadata generation.
- Alternative: keep client validation everywhere but use CDN + cache headers; disabling client validation trades UX for simpler prod debugging of server rules.

**Production takeaway:** `ClientValidationEnabled` toggles attribute emission, not server `ModelState` — ops goal is script/attribute cost and avoiding false confidence from stale client rules.

---

#### Q7. (M) Hand-written HTML replaces tag helpers — server validation works, client validation disappears. What do helpers emit?

**Answer:** `IHtmlHelper` / tag helpers (`asp-for`, `asp-validation-for`) use **`IHtmlGenerator` + model metadata** to emit inputs with unobtrusive attributes: `data-val="true"`, `data-val-required`, `data-val-required-message`, `data-val-range`, etc., derived from data annotations and `ModelMetadata.ValidatorMetadata`.

- Plain `<input name="Quantity" />` binds on POST but carries **no `data-val-*`**, so jQuery unobtrusive never registers rules.
- To keep custom markup, add attributes manually (error-prone) or use tag helpers with `css-class` overrides instead of raw HTML.
- `asp-validation-for` generates `<span class="field-validation-valid" data-valmsg-for="Quantity">` wired to jQuery Validate message placement.
- `ViewContext.ClientValidationEnabled == false` suppresses all of the above even with tag helpers.

**Production takeaway:** Karat tests mechanism — validation is metadata-driven at render time, not magic on field names alone.

---

#### Q8. (P) French server messages but English jQuery Validate defaults. What still needs wiring?

**Answer:** Localizing `[Required]` error messages via `.resx` / `IValidationMetadataProvider` affects **server** `ModelState` and optionally `data-val-*-message` on rendered fields, but **jQuery Validate's built-in method messages** (email, number, date) remain English unless you override `$.validator.messages` or load a localized `messages_fr.js` **after** jquery.validate and **before** unobtrusive parses the form.

- Ensure localized messages appear in HTML: inspect `data-val-required-message` — if French there, unobtrusive uses them for annotation-backed rules.
- Add script: `$.validator.methods` / `$.extend($.validator.messages, { required: "...", ... })` or `@Url.Content("~/lib/jquery-validation/localization/messages_fr.min.js")`.
- Align `RequestLocalization` culture with the script you load; culture switching may need per-culture script bundles.
- For API/SPA paths, localization belongs in ProblemDetails / client i18n — not jQuery Validate.

**Production takeaway:** Server localization ≠ client localization — two pipelines must be wired separately.

---

#### Q9. (D) React SPA POSTs JSON to API; team assumes `OrderViewModel` annotations protect both MVC admin and API. Compare paths.

**Answer:** MVC unobtrusive validation applies only to **Razor-rendered HTML forms** that include validation scripts — it never runs for a React SPA posting JSON to `/api/orders`; the API path needs **`[ApiController]` automatic 400 validation**, explicit `ModelState` checks, or FluentValidation in the API pipeline.

| Concern | MVC Razor + unobtrusive | SPA + Web API |
|---|---|---|
| Where rules run | Browser (optional) + server on POST if action validates | Server only (unless SPA implements its own) |
| Annotation enforcement | Tag helpers emit client metadata; server on valid POST handler | `[ApiController]` returns 400 ValidationProblemDetails when invalid |
| Shared DTO | Same class can back both | Same annotations work **server-side** on API if validation filter runs |
| Bypass | curl to MVC POST | curl/SPA always bypasses any browser script |

- Extract shared validation to a common library; enforce on API with `[ApiController]` + `ModelState` or FluentValidation.
- SPA should mirror rules for UX (React Hook Form + Zod, etc.) — duplicate of server rules, not a substitute.
- Do not rely on MVC views to protect API routes — separate controllers, separate pipelines.

**Production takeaway:** Karat tests architecture judgment — annotations on a shared model do not automatically secure every host; each entry point must validate.

---

#### Q10. (R) Review AJAX partial-form save. Request fires even when `[Required]` fields are empty.

**Answer:** jQuery unobtrusive hooks **`form` submit** events — a **`type="button"` click handler** that calls `$.post` bypasses the validator unless it explicitly calls `$form.valid()` and aborts on `false`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Event path | Custom AJAX bypasses submit interceptor | Empty `DisplayName` posted |
| Validation API | No `$form.valid()` guard | Required rule never evaluated |
| Server | POST may succeed or fail inconsistently | Bad data if server check also missing |

**Fix (priority order):**

1. Guard AJAX: `if (!$('#profile-form').valid()) return;` before `$.post`.
2. Or use `type="submit"` and bind `$('#profile-form').on('submit', function (e) { e.preventDefault(); if ($(this).valid()) { $.post(...); } });`.
3. Always validate on server in `SaveProfile` POST action.

**Production takeaway:** Unobtrusive validation is form-submit-centric — any custom AJAX save must opt into the same `$.validator` API; pairs naturally with AJAX partial-update patterns in MVC Chapter 13.
