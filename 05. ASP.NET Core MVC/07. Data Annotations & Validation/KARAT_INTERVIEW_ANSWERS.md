# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `06. ASP.NET Core MVC/07. Data Annotations & Validation`

---

#### Q1. (R) Review this MVC registration flow. QA reports invalid accounts pass when JavaScript is disabled.

```csharp
[HttpPost]
public IActionResult Register(RegisterViewModel model)
{
    _users.Create(model.Email, model.Password);
    return RedirectToAction("Index", "Home");
}
```

**Answer:** Data annotations and `_ValidationScriptsPartial` only drive **client-side** jQuery unobtrusive validation — they do not block invalid POSTs by themselves. Without `if (!ModelState.IsValid) return View(model);`, the controller accepts any request, including direct HTTP POSTs with JavaScript disabled or bypassed.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Server validation | No `ModelState.IsValid` check | Invalid emails/passwords persisted |
| Security | Client-only enforcement | Pen testers bypass with curl/Postman |
| UX | No server error display path | Silent bad data or exceptions downstream |

**Fix (priority order):**

1. Guard POST actions: `if (!ModelState.IsValid) return View(model);` before `_users.Create`.
2. Keep annotations on the ViewModel — they populate `ModelState` on the server when the filter runs.
3. Add `[ValidateAntiForgeryToken]` on POST to reduce CSRF; it does not replace server validation.
4. Optionally enable automatic validation for API-style controllers with `[ApiController]` — MVC controllers require explicit checks.

**Production takeaway:** Karat tests the classic bypass — annotations on the model are not a substitute for checking `ModelState` in MVC actions.

---

#### Q2. (R) Review checkout ViewModel. Users submit without accepting terms despite a required checkbox.

```csharp
[Required] public bool? AcceptTerms { get; set; }
```

**Answer:** For nullable value types, `[Required]` means the value must be **non-null**, not "must be true." An unchecked checkbox binds as `false` (non-null), which satisfies `[Required]` — only `null` fails. Legal "must opt in" requires validating `true`, not mere presence.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Semantics | `[Required]` on `bool?` checks null, not true | Terms acceptance not enforced |
| Binding | Unchecked checkbox → `false` or omitted → default | Compliance gap |
| UX | Label says "required" but server allows skip | Regulatory risk |

**Fix (priority order):**

1. Replace with explicit rule: custom attribute, `IValidatableObject`, or `[Range(typeof(bool), "true", "true")]` / `[MustBeTrue]` pattern validating `AcceptTerms == true`.
2. Use non-nullable `bool` only when false is acceptable default; for opt-in consent, validate `true` explicitly.
3. Server-side check remains mandatory regardless of checkbox HTML5 `required`.

**Production takeaway:** `[Required]` on `bool?` is one of the most common MVC validation misconceptions — Karat uses it to test tri-state vs consent semantics.

---

#### Q3. (R) Review password change form. Server accepts mismatched passwords.

```csharp
[Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
public string ConfirmPassword { get; set; } = "";
```

**Answer:** `[Compare]` validates that `ConfirmPassword` equals the named property **`NewPassword`** on the same object at **server** validation time. If the refactor left a stale name, Compare targets a missing property and may not enforce equality. Client-side Compare depends on matching `data-val-*` attributes generated from the current property names — stale cached scripts or mismatched partial views cause browser-only errors.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Attribute | `Compare` target name must match property exactly | Server-side compare silently ineffective if wrong |
| Client/server | Unobtrusive attributes out of sync with model | Chrome shows error; POST without JS succeeds |
| Security | No server guard when Compare fails | Password changed to attacker-supplied value |

**Fix (priority order):**

1. Verify `[Compare(nameof(NewPassword))]` — use `nameof` to survive refactors.
2. Confirm POST handler checks `ModelState.IsValid` before `_users.ChangePassword`.
3. Rebuild/redeploy views so tag helpers emit updated `data-val-equalto-other`.
4. Pen-test with direct POST — client validation is never sufficient.

**Production takeaway:** Compare works on the server when names align; Karat pairs it with "JS disabled" to test whether server validation is actually invoked.

---

#### Q4. (P) Booking ViewModel cross-field rules — `IValidatableObject` vs custom `ValidationAttribute`.

**Answer:** Use **`IValidatableObject`** when rules span multiple properties (`EndDate >= StartDate`, guest name conditional on `GuestCount`). Use a **custom `ValidationAttribute`** on a single property when the rule is local and reusable (e.g., "discount 0–100"). Both run during **server-side** model validation after property-level attributes.

| Approach | Best for | MVC error mapping |
|---|---|---|
| `IValidatableObject` | Cross-field / conditional logic | `ValidationResult` with `memberNames` → `ModelState` keys match `asp-validation-for` |
| Custom `ValidationAttribute` | Single-property reusable rule | Attribute on property → automatic key = property name |
| FluentValidation (see Q10) | Many rules, complex graphs | Same `ModelState` keys when auto-validation registered |

- **Order:** Property attributes run first; then `IValidatableObject.Validate`. Failures add to `ModelState` before the action body.
- For `EndDate >= StartDate`, return `new ValidationResult("...", new[] { nameof(EndDate), nameof(StartDate) })` so both fields highlight.
- Custom attribute on `EndDate` alone can compare to `StartDate` via `ValidationContext.ObjectInstance` but gets awkward for "guest name when count > 0" — `IValidatableObject` stays clearer.

**Production takeaway:** Cross-field rules belong in `IValidatableObject` or FluentValidation rule classes — not scattered Compare hacks on the wrong properties.

---

#### Q5. (R) Review custom `DiscountPercentAttribute`. Validation fails unpredictably on form POST.

```csharp
protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
{
    if (value is null) return ValidationResult.Success;
    var n = (int)value;  // decimal 12.5 from model binder
```

**Answer:** The custom attribute **casts `decimal` to `int`**, which throws `InvalidCastException` or truncates depending on runtime — model binding supplies `decimal` from `<input type="number" step="0.01">`, not `int`. Unhandled exceptions in `IsValid` surface as 500s or failed validation inconsistently.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Type safety | `(int)value` on bound `decimal` | InvalidCastException or wrong validation |
| Null handling | `null` succeeds | May skip required check if property should be required |
| Client parity | No client adapter for custom attribute | Server-only unless `IClientModelValidator` added |

**Fix (priority order):**

1. Accept `decimal` (or use `Convert.ToDecimal(value, CultureInfo.InvariantCulture)`).
2. Validate range on decimal: `if (n < 0 || n > 100)`.
3. Implement `IClientModelValidator` (or inherit adapter base) if client-side parity is required.
4. Add unit tests with `ValidationContext` and decimal values, not just ints.

**Production takeaway:** Custom attributes must match **binder output types** — MVC forms rarely bind straight to `int` when the model property is `decimal`.

---

#### Q6. (D) EF entity annotated with validation attributes used as MVC model and API model.

**Answer:** Sharing one **`Product` entity** for persistence, Razor binding, and API contracts couples validation, exposes persistence fields (`RowVersion`, FKs), and makes API versioning painful. EF configuration belongs in Fluent API / `IEntityTypeConfiguration`; **input validation belongs on ViewModels/DTOs**.

| Concern | Entity as model | Separate ViewModel/DTO |
|---|---|---|
| Validation scope | DB constraints leak to UI/API | Per-use-case rules |
| Over-posting | Hidden fields bind `Id`, `RowVersion` | Only intended fields on form |
| API evolution | MVC `[StringLength]` breaks JSON clients | Independent contracts |
| Testing | EF + validation intertwined | Validate ViewModel without DbContext |

- Map `Product` ↔ `ProductEditViewModel` in the controller or with AutoMapper.
- Keep `[Required]`/`[Range]` on create/update DTOs; use EF `HasMaxLength` for schema.
- Never round-trip `RowVersion` in editable hidden fields without `[ValidateNever]` and careful concurrency handling — prefer server reload on POST.

**Production takeaway:** Karat tests layered architecture — annotations on entities are a shortcut that breaks MVC over-posting and API separation.

---

#### Q7. (R) Client vs server validation mismatch on date field.

```csharp
$.validator.methods.date = function () { return true; };
```

**Answer:** The legacy JS override makes **client validation always pass** for dates, while server `[DataType(DataType.Date)]` and model binding still parse with **server culture** (`en-US`). User enters `31/12/2026` (dd/MM/yyyy); client says OK, server fails binding or validation → `ModelState` error after submit.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Client | Custom validator accepts any string | False "valid" in browser |
| Culture | Display `dd/MM/yyyy` vs server `en-US` | Parse failures on POST |
| Contract | No ISO 8601 in API-style inputs | Ambiguous day/month |

**Fix (priority order):**

1. Remove the `return true` date override; use `<input asp-for="EventDate" />` with consistent format or `type="date"` (ISO yyyy-MM-dd).
2. Align culture: `RequestLocalization` middleware + consistent `DisplayFormat` / invariant API dates.
3. Treat server `ModelState` as source of truth; display errors with `asp-validation-summary`.
4. For global apps, prefer ISO dates in transport and localize only in display.

**Production takeaway:** Client/server mismatch is a support-ticket pattern — server binding culture wins when they disagree.

---

#### Q8. (R) Review `[Remote]` username check. DB CPU spikes; enumeration flagged.

```csharp
[Remote(action: "CheckUsername", controller: "Account")]
public IActionResult CheckUsername(string username)
{
    var taken = _db.Users.Any(u => u.Username == username);
    return Json(!taken);
}
```

**Answer:** `[Remote]` triggers an AJAX call **on blur/change** (and potentially frequently during typing depending on setup) — each hit runs a **synchronous DB query** with no caching, auth, or rate limiting. Attackers can enumerate usernames and load the database; marketing traffic multiplies calls.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Abuse | Unauthenticated DB lookup endpoint | CPU spike, user enumeration |
| Security | Returns boolean availability | Harvest valid usernames |
| Design | Remote validation ≠ authoritative | Race: available at check, taken at submit |

**Fix (priority order):**

1. Rate-limit `CheckUsername` (IP + session); require anti-forgery or authenticated context where appropriate.
2. Debounce client calls; avoid Remote for high-cost checks — validate uniqueness **once on POST** inside transaction.
3. Return generic messages on final submit; do not leak "username taken" on login vs register differently if that aids enumeration.
4. Cache negative results briefly if kept; prefer `[Remote]` only for low-cost format checks, not DB uniqueness.

**Production takeaway:** Remote validation is convenient for UX but is a **public endpoint** — treat it like any other abuse-prone API.

---

#### Q9. (P) Admin edit form — `PasswordHash` fails validation; explain `ValidateNever`.

```csharp
public string PasswordHash { get; set; } = "";
```

**Answer:** Non-nullable reference type `PasswordHash` gets **implicit `[Required]`** when `Nullable` context is enabled (`SuppressImplicitRequiredAttributeForNonNullableReferenceTypes` defaults false). A hidden field that arrives empty on POST adds a ModelState error. **`[ValidateNever]`** (from `Microsoft.AspNetCore.Mvc.ModelBinding.Validation`) excludes the property from validation — use for secrets, internal flags, and navigation props that must bind but not be user-validated.

| Do | Don't |
|---|---|
| `[ValidateNever]` on hash, concurrency tokens used only server-side | Round-trip password hashes in hidden inputs |
| Validate `NewPassword` with `[MinLength]` when user sets password | Trust hidden `Id`/role fields without authorization check |
| Reload sensitive entity from DB on POST | Put `[Required]` on fields users must not supply |

- **Server-side still required:** authorize admin, verify user id, hash `NewPassword` on write — `ValidateNever` skips annotation validation, not business rules.
- **Better pattern:** omit `PasswordHash` from the ViewModel entirely; load entity in POST handler by `Id` only.

**Production takeaway:** `ValidateNever` fixes implicit required on bind-only properties — but secrets should not be in forms at all.

---

#### Q10. (D) Data Annotations + unobtrusive vs FluentValidation in MVC.

**Answer:** **Annotations** colocate rules on the ViewModel and generate client metadata automatically via tag helpers — great for simple CRUD, weak for complex cross-object rules and large rule sets. **FluentValidation** centralizes rules in `AbstractValidator<T>`, tests cleanly, and scales — but **does not** provide client-side rules unless you duplicate or use FV's client adapters (limited compared to unobtrusive).

| Criterion | Data Annotations | FluentValidation |
|---|---|---|
| Client-side parity | Automatic with `_ValidationScriptsPartial` | Manual / partial adapters |
| Complex rules | Verbose (`IValidatableObject`) | `RuleFor`, `Must`, `When` |
| Unit testing | Reflect attributes or integrate pipeline | `TestValidate()` directly |
| JS disabled POST | Server checks same annotations (if `ModelState` checked) | `AddFluentValidationAutoValidation()` populates `ModelState` |
| Discoverability | On property | Separate validator class |

- Register: `services.AddFluentValidationAutoValidation(); services.AddValidatorsFromAssemblyContaining<...>();`
- Hybrid is common: annotations for simple `[Required]`/`[EmailAddress]`, FluentValidation for domain rules.
- Regardless of library, MVC actions must check `ModelState.IsValid` (unless using auto API behavior).

**Production takeaway:** Choose FluentValidation when rules outgrow attributes; never assume either library replaces explicit server checks in MVC controllers.

---

#### Q11. (M) Server-side validation order for MVC POST with anti-forgery, `IValidatableObject`, and custom attribute.

**Answer:** For `[HttpPost] Create(ProductCreateViewModel model)` with `[ValidateAntiForgeryToken]`, the approximate order is: **(1)** routing and model binding (form values → model), **(2)** anti-forgery validation (fail → 400 before action), **(3)** **`ObjectModelValidator`** runs data annotations, custom `ValidationAttribute`s, and **`IValidatableObject.Validate`**, populating `ModelState`, **(4)** action executes — developer should test `ModelState.IsValid` at the start and `return View(model)` without mutating state.

- Property-level attributes (`[Required]`, custom attribute) run before `IValidatableObject`.
- Multiple errors on one property accumulate in `ModelState[key]`.
- `[ValidateNever]` properties skip step 3 for those members.
- `[Remote]` does **not** run automatically on POST — only when the remote endpoint was called client-side; POST must enforce all rules server-side again.
- Short-circuit: immediately after binding/validation, `if (!ModelState.IsValid) return View(model);` — before file I/O, DB, or side effects.

**Production takeaway:** Anti-forgery is not validation of field rules; `ModelState` is populated before your action body — the action must honor it.

---

#### Q12. (R) `[FromBody]` POST on MVC controller returns 200 instead of validation errors.

```csharp
[HttpPost]
public IActionResult QuickAdd([FromBody] QuickAddOrderViewModel model)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);
```

**Answer:** MVC controllers default to **HTML-oriented** behavior: `[FromBody]` JSON binding may fail silently or produce empty model if content negotiation/input formatter mismatches; without `[ApiController]`, there is **no automatic 400** on invalid models. If binding fails completely, `model` may be null and annotations never run — `ModelState.IsValid` can be true for the wrong reason, or errors are not serialized as JSON the AJAX client expects.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Controller type | MVC `Controller` not `[ApiController]` | No automatic validation response |
| Binding | JSON body on MVC may need `[ApiController]` or explicit `[Consumes("application/json")]` | Empty model, skipped validation |
| Response | `BadRequest(ModelState)` shape differs from ProblemDetails | Client cannot parse errors |

**Fix (priority order):**

1. Move JSON endpoints to `ControllerBase` + `[ApiController]` or minimal APIs for consistent 400 + ProblemDetails.
2. If staying in MVC: `[Consumes("application/json")]`, null-check model, call `TryValidateModel(model)`.
3. Return `ValidationProblem(ModelState)` or unified error DTO for AJAX.
4. Keep form POSTs as `application/x-www-form-urlencoded` on ViewModels — do not mix patterns on the same action without clear contracts.

**Production takeaway:** Mixing `[FromBody]` JSON into Razor MVC controllers without `[ApiController]` is a common source of "validation doesn't work" reports — binding and response shape both differ from Web API conventions.
