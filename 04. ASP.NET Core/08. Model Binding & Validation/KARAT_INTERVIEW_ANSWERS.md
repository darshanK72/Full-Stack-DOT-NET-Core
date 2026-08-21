# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md) in this folder.

> **Folder:** `05. ASP.NET Core/08. Model Binding & Validation`

---

#### Q1. (R) Review this API model and client JSON. Mobile clients send PascalCase; web clients send camelCase — some fields bind as default values.

```csharp
public class CreateCustomerRequest
{
    public string CustomerName { get; set; } = "";
    public decimal CreditLimit { get; set; }
}
```

**Answer:** ASP.NET Core 8 Web API defaults to **camelCase** JSON property names via `JsonNamingPolicy.CamelCase` — PascalCase keys like `"CustomerName"` do not bind to `CustomerName` unless case-insensitive matching is enabled, leaving empty string and zero decimal.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Serialization | PascalCase JSON keys vs camelCase policy | Mobile client fields ignored — silent data loss |
| Validation | Manual `IsNullOrEmpty` only | `CreditLimit` 0 passes while intended value dropped |
| Contract | Inconsistent client payloads | Intermittent bugs by client platform |

**Fix (priority order):**

1. Standardize clients on camelCase `{ "customerName", "creditLimit" }` — document in OpenAPI.
2. Or enable case-insensitive property names: `AddJsonOptions(o => o.JsonSerializerOptions.PropertyNameCaseInsensitive = true)` — prefer camelCase contract over relying on this.
3. Add `[Required]` and range validation; return ProblemDetails instead of ad-hoc strings.

**Production takeaway:** Default camelCase policy is why `CustomerName` becomes `"customerName"` in JSON — Karat tests binding direction, not just serialization out.

---

#### Q2. (D) A PATCH endpoint updates marketing preferences. Product requires three states: omit = unchanged, `true` = opt-in, `false` = opt-out. Review the proposed model.

```csharp
public class UpdatePreferencesRequest
{
    public bool SendNewsletter { get; set; } // default false when omitted
}
```

**Answer:** Non-nullable `bool` cannot represent three states — System.Text.Json deserializes a missing property as `default(false)`, collapsing "omit" and "explicit false" into the same value, which corrupts PATCH semantics.

| Client intent | JSON | `bool` model | `bool?` model |
|---|---|---|---|
| Leave unchanged | (omit) | `false` — **wrong** | `null` |
| Explicit opt-out | `"sendNewsletter": false` | `false` | `false` |
| Explicit opt-in | `"sendNewsletter": true` | `true` | `true` |

- Use `bool? SendNewsletter` on PATCH DTOs; service layer applies updates only when `HasValue`.
- Separate **Create** vs **Update** DTOs — Create may use non-nullable with required explicit consent.
- OpenAPI: mark property `nullable: true` for generated clients.
- For legally sensitive consent, prefer enum `Unspecified | OptIn | OptOut`.

**Production takeaway:** Karat tests contract design — wrong bool cardinality silently changes marketing compliance.

---

#### Q3. (R) Review this search endpoint. The filter object always arrives empty.

```csharp
[HttpGet("search")]
public IActionResult Search([FromBody] ProductFilter filter)
```

Request: `GET /api/products/search?category=electronics&minPrice=10`

**Answer:** GET requests must not use `[FromBody]` — browsers and clients do not send bodies reliably on GET; ASP.NET Core binds query parameters with `[FromQuery]` (default for complex types in GET when convention applies).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Binding source | `[FromBody]` on GET | Body ignored — `filter` is empty/default |
| HTTP semantics | Query string present but unused | Search returns unfiltered catalog |
| API design | GET with body breaks caches/proxies | Some proxies strip GET bodies |

**Fix (priority order):**

1. Change to `[FromQuery] ProductFilter filter` or rely on `[HttpGet]` + complex type default for query binding.
2. Rename query keys to match properties (`category`, `minPrice`) — camelCase in query matches by default.
3. For heavy filters, use POST `/search` with `[FromBody]` — not GET.

**Production takeaway:** Binding source mistakes look like "model binding is broken" — it's the wrong `[From*]` attribute.

---

#### Q4. (R) Review validation and error responses. Frontend expects ProblemDetails; API returns plain text 400.

```csharp
[HttpPost]
public IActionResult Register([FromBody] RegisterRequest request)
{
    if (!ModelState.IsValid)
        return BadRequest("Validation failed");
    // ...
}
```

**Answer:** Without `[ApiController]`, automatic 400 ProblemDetails validation does not run — manual `BadRequest(string)` returns plain text without field-level `errors` extension clients need.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Validation | Manual ModelState check with string body | No per-field errors for SPA forms |
| ApiController | Attribute missing | No automatic 400 + ValidationProblemDetails |
| ProblemDetails | `AddProblemDetails()` not registered | Inconsistent error shape across endpoints |

**Fix (priority order):**

1. Add `[ApiController]` to controller or assembly — invalid models auto-return 400 `ValidationProblemDetails`.
2. Replace manual check with `return ValidationProblem(ModelState);` if custom handling needed.
3. Register `builder.Services.AddProblemDetails();` and use `ProducesResponseType(typeof(ValidationProblemDetails), 400)` in OpenAPI.

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpPost]
    public IActionResult Register([FromBody] RegisterRequest request)
        => Ok(_users.Register(request)); // 400 automatic when invalid
}
```

**Production takeaway:** Uniform ProblemDetails are part of the API contract — string `BadRequest` breaks frontend parsers.

---

#### Q5. (R) Review complex query binding. Pagination works but nested sort object is always null.

```csharp
public class PagedQuery
{
    public int Page { get; set; } = 1;
    public SortOptions? Sort { get; set; }
}
```

Request: `GET /api/items?page=2&sort.field=name&sort.descending=true`

**Answer:** Nested complex types require bracket notation by default in ASP.NET Core query binding — `sort.field` works for prefix binding; verify `[FromQuery]` prefix or use flat query parameters if clients send wrong format like `sort[field]`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Query binding | Client uses wrong nested syntax (`sort[field]` vs `sort.field`) | `Sort` stays null — default sort used silently |
| Documentation | OpenAPI does not describe nested query shape | SDKs generate wrong query strings |
| Design | Deep nesting in query strings | Hard to debug — prefer flat `sortField` / `sortDesc` |

**Fix (priority order):**

1. Document and test supported format: `?sort.field=name&sort.descending=true` or `[FromQuery(Name = "sort")]`.
2. Simplify DTO: `public string? SortField { get; set; }` and `public bool SortDescending { get; set; }` for public APIs.
3. Add model binding test with `WebApplicationFactory` asserting bound values.

**Production takeaway:** Complex `[FromQuery]` objects are fragile — Karat tests whether you know prefix conventions or flatten the contract.

---

#### Q6. (P) A cross-field rule requires `EndDate >= StartDate` on a booking DTO. Compare `IValidatableObject` vs FluentValidation — what runs when, and how do errors surface in ProblemDetails?

**Answer:** Both run after property-level DataAnnotations; errors merge into `ModelState` and return as `ValidationProblemDetails` when `[ApiController]` is enabled — FluentValidation scales better for many rules and keeps DTOs thin.

| Approach | Registration | When it runs |
|---|---|---|
| `IValidatableObject` | On the model class | During `ObjectModelValidator` pass — same as `[Required]` |
| FluentValidation | `AddFluentValidationAutoValidation()` | Automatic before action; rules in `AbstractValidator<T>` |

```csharp
public class BookingRequest : IValidatableObject
{
    public DateOnly Start { get; set; }
    public DateOnly End { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext ctx)
    {
        if (End < Start)
            yield return new ValidationResult(
                "End must be on or after Start.",
                new[] { nameof(End) });
    }
}
```

- Errors appear under `errors.End` in ProblemDetails JSON — same shape as `[Required]`.
- FluentValidation: clearer rule composition, async rules, separation from transport DTOs.
- Do not duplicate the same rule in both places.

**Production takeaway:** Cross-field validation still flows through ModelState — clients see one consistent 400 shape.

---

#### Q7. (R) Review this POST action. Model binding silently fails and `id` is always 0.

```csharp
[HttpPost("orders/{orderId:int}/notes")]
public IActionResult AddNote(int orderId, [FromBody] string noteText)
```

Body: `"Please ship before Friday"` (raw JSON string).

**Answer:** `[FromBody] string` expects a JSON string token but route `orderId` must be `[FromRoute]` explicitly when mixed with body; primitive body binding is fragile — use a wrapper DTO `{ "noteText": "..." }`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Binding source | `orderId` not marked `[FromRoute]` | May bind incorrectly when multiple sources exist |
| Body primitive | Raw JSON string body | Easy to fail content negotiation; prefer wrapper type |
| Validation | `orderId == 0` conflates missing route with invalid id | Weak guard — route constraint should reject bad ids |

**Fix (priority order):**

1. Use `public record AddNoteRequest(string NoteText);` with `[FromBody] AddNoteRequest body`.
2. Mark route parameter: `[FromRoute] int orderId`.
3. Keep `{orderId:int}` route constraint — invalid routes 404 before action.

**Production takeaway:** `[FromBody]` on primitives is a common interview trap — wrapper DTOs are the production pattern.

---

#### Q8. (M) An `[ApiController]` receives invalid JSON shape. Walk through automatic validation, default status/payload in ASP.NET Core 8, and `[ValidateNever]` / `[Required]` effects.

**Answer:** For `[ApiController]` actions, invalid model state automatically short-circuits to **400 Bad Request** with `ValidationProblemDetails` (`application/problem+json`) before your action body runs — individual `[Required]` failures add entries to `ModelState`; `[ValidateNever]` excludes properties from validation (useful for navigation properties on EF entities accidentally bound).

- Pipeline: model binding → DataAnnotations + `IValidatableObject` → `ObjectModelValidator` → if invalid, `ApiController` filter returns `BadRequest(ModelState)` as ProblemDetails.
- Malformed JSON (syntax error) typically yields **400** with different problem type — not reaching validation.
- Nullable reference types: `[Required]` needed for non-nullable reference properties when NRT context enabled.
- `[ValidateNever]` on `Order.Customer` prevents validating nested graph on PATCH binding mistakes.
- Customize via `ConfigureApiBehaviorOptions` — e.g., suppress automatic 400 for specific endpoints (discouraged for public APIs).

**Production takeaway:** `[ApiController]` is the switch that turns validation into consistent ProblemDetails — without it, you own every error branch.

---

#### Q9. (R) Review this action mixing binding sources. Route id and body quantity disagree; wrong inventory is updated.

```csharp
[HttpPut("inventory/{sku}")]
public async Task<IActionResult> Adjust(
    [FromRoute] string sku,
    [FromBody] InventoryAdjustRequest body,
    [FromQuery] int quantity)
{
    await _inventory.AdjustAsync(sku, quantity > 0 ? quantity : body.Quantity);
    return NoContent();
}
```

Request: `PUT /api/inventory/WIDGET-1?quantity=5` with body `{ "quantity": 100 }`.

**Answer:** The action prefers query `quantity` when `> 0`, silently ignoring body `100` when query is `5` — dual sources for the same business field create ambiguous contracts and race-prone updates.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Binding design | Same field from query and body | Unpredictable precedence — query wins here |
| API contract | Clients send conflicting values | Inventory adjusted by wrong amount |
| REST | PUT body should be authoritative | Query string override is undocumented |

**Fix (priority order):**

1. Single source of truth: `[FromBody] InventoryAdjustRequest` only — remove query `quantity`.
2. Document OpenAPI with one schema; reject unexpected query params with `[FromQuery]` removed.
3. Return 400 if both provided and values differ (explicit validation rule).

**Production takeaway:** Multiple binding sources for one concept is a design smell — Karat tests API clarity, not binder mechanics alone.

---
