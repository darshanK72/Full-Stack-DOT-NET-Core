# Karat — Interview Questions

> **Folder:** `07. ASP.Net Core Web API/05. Model Binding & Validation`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)  
> **Raw debrief:** [KARAT_DEBRIEF_SOURCE.txt](../../00. Notes & Practice/04. Citi Karat Interview Practice/KARAT_DEBRIEF_SOURCE.txt)

---

#### Q1. (R) Review this Web API POST action. A legacy mobile client sends PascalCase JSON; the React SPA sends camelCase. `CreditLimit` is always 0 for one client group.

```csharp
[HttpPost]
public IActionResult CreateCustomer([FromBody] CreateCustomerRequest request)
{
    return CreatedAtAction(nameof(Get), new { id = request.Id }, request);
}

public class CreateCustomerRequest
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = "";
    public decimal CreditLimit { get; set; }
}

// Mobile:  { "CustomerName": "Acme", "CreditLimit": 5000 }
// Web:     { "customerName": "Acme", "creditLimit": 5000 }
```

`Program.cs` uses default ASP.NET Core 8 Web API JSON options with no custom `AddJsonOptions`.

---

#### Q2. (D) A PATCH endpoint updates notification preferences on a user profile API. Product requires three states: omit field = unchanged, `true` = opt-in, `false` = explicit opt-out. Review the DTO and OpenAPI contract.

```csharp
public class PatchNotificationPreferences
{
    public bool EmailAlerts { get; set; }
    public bool SmsAlerts { get; set; }
}

// PATCH /api/users/{id}/preferences
// Body omitting EmailAlerts — must NOT change stored value
// Body { "emailAlerts": false } — must record explicit opt-out
```

---

#### Q3. (R) Review this catalog search endpoint on a Web API. Query string is visible in browser dev tools but `ProductFilter` binds empty.

```csharp
[HttpGet("search")]
public IActionResult Search([FromBody] ProductFilter filter)
{
    return Ok(_catalog.Search(filter));
}

public class ProductFilter
{
    public string? Category { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}
```

Request: `GET /api/products/search?category=electronics&minPrice=10&maxPrice=500`

---

#### Q4. (R) Review validation on a registration endpoint. The Angular client expects RFC 7807 `ValidationProblemDetails` with per-field errors; the API returns plain text 400.

```csharp
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest("Validation failed");

        return Ok(_users.Register(request));
    }
}

public class RegisterRequest
{
    [Required][EmailAddress] public string Email { get; set; } = "";
    [Required][MinLength(8)] public string Password { get; set; } = "";
}
```

Controller lacks `[ApiController]`; `Program.cs` does not call `AddProblemDetails()`.

---

#### Q5. (R) Review this list endpoint. Pagination binds correctly but nested `Sort` is always null despite query string.

```csharp
[HttpGet]
public IActionResult List([FromQuery] PagedQuery query)
{
    return Ok(_repo.List(query));
}

public class PagedQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public SortOptions? Sort { get; set; }
}

public class SortOptions
{
    public string Field { get; set; } = "createdAt";
    public bool Descending { get; set; }
}
```

Request: `GET /api/orders?page=2&sort[field]=createdAt&sort[descending]=true`

---

#### Q6. (P) A booking API requires `EndDate >= StartDate` and `GuestCount <= RoomCapacity` on a create-reservation DTO. Compare `IValidatableObject` on the request model vs FluentValidation with `AddFluentValidationAutoValidation()` — execution order, testability, and how errors appear in ProblemDetails for `[ApiController]` endpoints.

---

#### Q7. (R) Review this minimal-style note endpoint. The route `orderId` binds as 0 and the note text is null under load.

```csharp
[HttpPost("orders/{orderId:int}/notes")]
public IActionResult AddNote(int orderId, [FromBody] string noteText)
{
    if (orderId == 0) return BadRequest();
    _notes.Add(orderId, noteText);
    return NoContent();
}
```

Body: `"Please ship before Friday"` (raw JSON string). Content-Type: `application/json`.

---

#### Q8. (M) An `[ApiController]`-annotated Web API receives a POST with valid JSON syntax but missing required fields and wrong types on optional nested objects. Walk through when automatic model validation runs, the default status code and payload shape in ASP.NET Core 8, and how `[ValidateNever]` vs `[Required]` change behavior for EF navigation properties accidentally included in a DTO.

---

#### Q9. (R) Review this inventory adjustment endpoint. Clients send conflicting quantity values; the wrong amount is persisted intermittently.

```csharp
[HttpPut("inventory/{sku}")]
public async Task<IActionResult> Adjust(
    [FromRoute] string sku,
    [FromBody] InventoryAdjustRequest body,
    [FromQuery] int quantity)
{
    var amount = quantity > 0 ? quantity : body.Quantity;
    await _inventory.AdjustAsync(sku, amount);
    return NoContent();
}

public class InventoryAdjustRequest
{
    public int Quantity { get; set; }
}
```

Request: `PUT /api/inventory/WIDGET-1?quantity=5` with body `{ "quantity": 100 }`.
