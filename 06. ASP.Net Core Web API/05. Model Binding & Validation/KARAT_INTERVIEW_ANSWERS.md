# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `07. ASP.Net Core Web API/05. Model Binding & Validation`

---

#### Q1. (R) Review this Web API POST action. PascalCase vs camelCase — `CreditLimit` always 0 for one client group.

**Answer:** ASP.NET Core 8 Web API defaults to camelCase JSON property names via `JsonNamingPolicy.CamelCase` — PascalCase keys like `"CustomerName"` and `"CreditLimit"` do not bind unless case-insensitive matching is enabled, leaving default values (empty string, zero decimal).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Serialization | PascalCase JSON keys vs camelCase inbound policy | Mobile client fields silently ignored |
| Contract | Mixed client conventions without documented standard | Intermittent data loss by platform |
| Validation | No `[Required]` / range checks on bound values | Zero credit limit accepted as valid |

**Fix (priority order):**

1. Standardize all clients on camelCase `{ "customerName", "creditLimit" }` — document in OpenAPI as the canonical contract.
2. Or enable `AddJsonOptions(o => o.JsonSerializerOptions.PropertyNameCaseInsensitive = true)` — acceptable for legacy migration, not a long-term substitute for one naming policy.
3. Add validation attributes and return `ValidationProblemDetails` instead of accepting silent defaults.

**Production takeaway:** Default camelCase policy is why `CustomerName` serializes as `"customerName"` outbound and expects camelCase inbound — Karat tests binding direction, not just response formatting.

---

#### Q2. (D) PATCH notification preferences — three states required (omit, opt-in, opt-out). Review the DTO.

**Answer:** Non-nullable `bool` cannot represent three states — System.Text.Json deserializes a missing property as `default(false)`, collapsing "omit (unchanged)" and "explicit false (opt-out)" into the same value, corrupting PATCH semantics and compliance audit trails.

| Client intent | JSON | `bool` model | `bool?` model |
|---|---|---|---|
| Leave unchanged | (omit) | `false` — **wrong** | `null` |
| Explicit opt-out | `"emailAlerts": false` | `false` | `false` |
| Explicit opt-in | `"emailAlerts": true` | `true` | `true` |

- Use `bool? EmailAlerts` and `bool? SmsAlerts` on PATCH DTOs; service applies updates only when `HasValue`.
- Separate Create vs Patch DTOs — Create may require explicit non-nullable consent.
- Mark properties `nullable: true` in OpenAPI so generated clients send omission correctly.
- For regulated consent, prefer enum `Unspecified | OptIn | OptOut`.

**Production takeaway:** Wrong bool cardinality silently changes user preferences — a contract design trap, not a serializer bug.

---

#### Q3. (R) Review catalog search — query string present but `ProductFilter` binds empty.

**Answer:** GET requests must not use `[FromBody]` — browsers and HTTP clients do not reliably send bodies on GET; ASP.NET Core binds query parameters with `[FromQuery]` (default for complex types on GET when convention applies).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Binding source | `[FromBody]` on GET | Body ignored — filter is empty/default |
| HTTP semantics | Query string present but unused | Search returns unfiltered catalog |
| API design | GET with body breaks caches/proxies | Some intermediaries strip GET bodies |

**Fix (priority order):**

1. Change to `[FromQuery] ProductFilter filter` or rely on GET + complex type query binding convention.
2. Ensure query keys match properties (`category`, `minPrice`, `maxPrice`) — camelCase by default.
3. For heavy filter payloads, use POST `/search` with `[FromBody]` — not GET.

**Production takeaway:** Binding source mistakes look like "model binding is broken" — it's the wrong `[From*]` attribute.

---

#### Q4. (R) Review registration validation — frontend expects ProblemDetails; API returns plain text 400.

**Answer:** Without `[ApiController]`, automatic 400 `ValidationProblemDetails` does not run — manual `BadRequest("Validation failed")` returns plain text without per-field `errors` the SPA parser expects.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Validation | Manual ModelState check with string body | No field-level errors for forms |
| ApiController | Attribute missing on controller | No automatic 400 + ValidationProblemDetails |
| ProblemDetails | `AddProblemDetails()` not registered | Inconsistent error shape across endpoints |

**Fix (priority order):**

1. Add `[ApiController]` to controller or assembly — invalid models auto-return 400 `ValidationProblemDetails`.
2. Remove manual check; use `return ValidationProblem(ModelState);` only when customizing.
3. Register `builder.Services.AddProblemDetails();` and document 400 response in OpenAPI.

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
        => Ok(_users.Register(request)); // 400 automatic when invalid
}
```

**Production takeaway:** Uniform ProblemDetails are part of the Web API contract — string `BadRequest` breaks client parsers.

---

#### Q5. (R) Review list endpoint — pagination binds but nested `Sort` is always null.

**Answer:** ASP.NET Core query binding uses dot notation for nested objects by default (`sort.field=name`) — bracket syntax `sort[field]` (common in some frontends) does not bind to `SortOptions` without a custom model binder or flattened DTO.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Query binding | Client sends `sort[field]` vs expected `sort.field` | `Sort` stays null — default sort used silently |
| Documentation | OpenAPI does not describe nested query shape | SDKs generate wrong query strings |
| Design | Deep nesting in query strings | Hard to debug — prefer flat parameters |

**Fix (priority order):**

1. Document supported format: `?sort.field=createdAt&sort.descending=true`.
2. Simplify DTO: `public string? SortField { get; set; }` and `public bool SortDescending { get; set; }` for public APIs.
3. Add integration test with `WebApplicationFactory` asserting bound values for both syntaxes if both must be supported.

**Production takeaway:** Complex `[FromQuery]` objects are fragile — know prefix conventions or flatten the contract.

---

#### Q6. (P) Cross-field booking rules — `IValidatableObject` vs FluentValidation; ProblemDetails surfacing.

**Answer:** Both run after property-level DataAnnotations during model validation; errors merge into `ModelState` and return as `ValidationProblemDetails` when `[ApiController]` is enabled — FluentValidation scales better for many rules and keeps DTOs thin.

| Approach | Registration | When it runs |
|---|---|---|
| `IValidatableObject` | On the model class | During `ObjectModelValidator` pass — same pipeline as `[Required]` |
| FluentValidation | `AddFluentValidationAutoValidation()` + `AbstractValidator<T>` | Automatic before action; rules in separate classes |

```csharp
public class CreateBookingRequest : IValidatableObject
{
    public DateOnly Start { get; set; }
    public DateOnly End { get; set; }
    public int GuestCount { get; set; }
    public int RoomCapacity { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext ctx)
    {
        if (End < Start)
            yield return new ValidationResult("End must be on or after Start.", new[] { nameof(End) });
        if (GuestCount > RoomCapacity)
            yield return new ValidationResult("Guest count exceeds room capacity.", new[] { nameof(GuestCount) });
    }
}
```

- Errors appear under `errors.end`, `errors.guestCount` in ProblemDetails JSON — same shape as DataAnnotations.
- FluentValidation supports async rules, rule sets, and keeps transport DTOs free of validation logic.
- Do not duplicate the same rule in both places.

**Production takeaway:** Cross-field validation still flows through ModelState — clients see one consistent 400 shape regardless of validator type.

---

#### Q7. (R) Review note endpoint — `orderId` is 0 and note text is null.

**Answer:** `[FromBody] string` for raw JSON string bodies is fragile in Web APIs; mixed route and body parameters require explicit `[FromRoute]` on `orderId` — prefer a wrapper DTO for reliable binding and OpenAPI documentation.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Binding source | `orderId` not marked `[FromRoute]` | May bind from wrong source under ambiguity |
| Body primitive | Raw JSON string `"text"` | Fails or misbinds; not well described in OpenAPI |
| Validation | `orderId == 0` guard | Conflates missing route with invalid id |

**Fix (priority order):**

1. Use `public record AddNoteRequest(string NoteText);` with `[FromBody] AddNoteRequest body`.
2. Mark route parameter: `[FromRoute] int orderId`.
3. Keep `{orderId:int}` route constraint — invalid routes 404 before action.

**Production takeaway:** `[FromBody]` on primitives is a common Karat trap — wrapper DTOs are the production pattern for Web APIs.

---

#### Q8. (M) `[ApiController]` receives invalid model — automatic validation, default 400 payload, `[ValidateNever]` / `[Required]` effects.

**Answer:** For `[ApiController]` actions, invalid model state automatically short-circuits to **400 Bad Request** with `ValidationProblemDetails` (`application/problem+json`) before the action body runs — `[Required]` adds ModelState entries for missing required fields; `[ValidateNever]` excludes properties from validation (critical when EF navigation properties are accidentally bound on PATCH DTOs).

- Pipeline: model binding → DataAnnotations + `IValidatableObject` + FluentValidation → `ObjectModelValidator` → if invalid, `ApiController` filter returns `BadRequest(ModelState)` as ProblemDetails.
- Malformed JSON (syntax error) yields **400** with a different problem type — validation never runs.
- With nullable reference types enabled, `[Required]` is needed on non-nullable reference properties.
- `[ValidateNever]` on `Order.Customer` prevents validating an entire nested graph when only scalar fields should bind.
- Customize via `ConfigureApiBehaviorOptions` — e.g., `SuppressModelStateInvalidFilter` (discouraged for public APIs).

**Production takeaway:** `[ApiController]` is the switch that turns validation into consistent ProblemDetails — without it, you own every error branch manually.

---

#### Q9. (R) Review inventory adjustment — conflicting quantity from query and body.

**Answer:** The action prefers query `quantity` when `> 0`, silently using `5` instead of body `100` — dual sources for the same business field create ambiguous contracts and unpredictable updates under load.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Binding design | Same field from query and body | Undocumented precedence — query wins |
| API contract | Clients send conflicting values | Wrong inventory adjustment persisted |
| REST | PUT body should be authoritative | Query override surprises integrators |

**Fix (priority order):**

1. Single source of truth: `[FromBody] InventoryAdjustRequest` only — remove query `quantity`.
2. Document one schema in OpenAPI; reject unexpected query params.
3. Return 400 if both provided and values differ (explicit cross-field validation rule).

**Production takeaway:** Multiple binding sources for one concept is a design smell — Karat tests API clarity, not binder mechanics alone.
