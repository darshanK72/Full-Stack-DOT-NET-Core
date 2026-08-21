# Karat — Interview Questions

> **Folder:** `05. ASP.NET Core/08. Model Binding & Validation`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)  
> **Raw debrief:** [KARAT_DEBRIEF_SOURCE.txt](../../00. Notes & Practice/04. Citi Karat Interview Practice/KARAT_DEBRIEF_SOURCE.txt)

---

#### Q1. (R) Review this API model and client JSON. Mobile clients send PascalCase; web clients send camelCase — some fields bind as default values instead of the intended data.

```csharp
public class CreateCustomerRequest
{
    public string CustomerName { get; set; } = "";
    public decimal CreditLimit { get; set; }
}

// Client A body: { "CustomerName": "Acme", "CreditLimit": 5000 }
// Client B body: { "customerName": "Acme", "creditLimit": 5000 }

[HttpPost]
public IActionResult Create([FromBody] CreateCustomerRequest request)
{
    if (string.IsNullOrEmpty(request.CustomerName))
        return BadRequest("CustomerName required");
    return Ok(_service.Create(request));
}
```

`Program.cs` has no `AddJsonOptions`; project targets ASP.NET Core 8 Web API template defaults.

---

#### Q2. (D) A PATCH endpoint updates marketing preferences. Product requires three states: "not specified" (omit field), explicit opt-in (`true`), and explicit opt-out (`false`). Review the proposed model and JSON contract.

```csharp
public class UpdatePreferencesRequest
{
    public bool SendNewsletter { get; set; } // default false when omitted
}

// PATCH body omitting SendNewsletter — should mean "leave unchanged"
// PATCH body { "sendNewsletter": false } — must mean explicit opt-out
```

---

#### Q3. (R) Review this search endpoint. The filter object always arrives empty even though query string is present in browser dev tools.

```csharp
[HttpGet("search")]
public IActionResult Search([FromBody] ProductFilter filter)
{
    var results = _catalog.Search(filter);
    return Ok(results);
}

public class ProductFilter
{
    public string? Category { get; set; }
    public decimal? MinPrice { get; set; }
}
```

Request: `GET /api/products/search?category=electronics&minPrice=10`

---

#### Q4. (R) Review validation and error responses. Frontend expects RFC 7807 `ProblemDetails` with field errors; API returns plain text 400.

```csharp
[HttpPost]
public IActionResult Register([FromBody] RegisterRequest request)
{
    if (!ModelState.IsValid)
        return BadRequest("Validation failed");

    return Ok(_users.Register(request));
}

public class RegisterRequest
{
    [Required][EmailAddress] public string Email { get; set; } = "";
    [Required][MinLength(8)] public string Password { get; set; } = "";
}
```

Controller lacks `[ApiController]` attribute; `Program.cs` does not call `AddProblemDetails()`.

---

#### Q5. (R) Review complex query binding. Pagination works but nested sort object is always null.

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
    public string Field { get; set; } = "name";
    public bool Descending { get; set; }
}
```

Request: `GET /api/items?page=2&sort.field=name&sort.descending=true`

---

#### Q6. (P) A cross-field rule requires `EndDate >= StartDate` on a booking DTO. Compare implementing `IValidatableObject` on the model vs FluentValidation in the pipeline — what runs when, and how do errors surface in ProblemDetails?

---

#### Q7. (R) Review this POST action. Model binding silently fails and `id` is always 0.

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

#### Q8. (M) An `[ApiController]`-annotated controller receives invalid JSON shape. Walk through when automatic model validation runs, what status code and payload the framework returns by default in ASP.NET Core 8, and how `[ValidateNever]` or `[Required]` affect that behavior.

---

#### Q9. (R) Review this action mixing binding sources. Route id and body quantity disagree; wrong inventory is updated under load.

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

public class InventoryAdjustRequest
{
    public int Quantity { get; set; }
}
```

Request: `PUT /api/inventory/WIDGET-1?quantity=5` with body `{ "quantity": 100 }`.
