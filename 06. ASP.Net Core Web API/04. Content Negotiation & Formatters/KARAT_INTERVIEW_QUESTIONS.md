# Karat — Interview Questions

> **Folder:** `07. ASP.Net Core Web API/04. Content Negotiation & Formatters`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)  
> **Raw debrief:** [KARAT_DEBRIEF_SOURCE.txt](../../00. Notes & Practice/04. Citi Karat Interview Practice/KARAT_DEBRIEF_SOURCE.txt)

---

#### Q1. (R) Review serialization for a dual-client API. The JavaScript web app binds correctly; a legacy integration test sends PascalCase JSON and `CustomerName` arrives null.

```csharp
// Program.cs — template defaults only
builder.Services.AddControllers();

public class CreateCustomerRequest
{
    public string CustomerName { get; set; } = "";
    public decimal CreditLimit { get; set; }
}

[HttpPost]
public IActionResult Create([FromBody] CreateCustomerRequest request)
{
    if (string.IsNullOrEmpty(request.CustomerName))
        return BadRequest("CustomerName required");
    return Ok(_svc.Create(request));
}
```

Client body: `{ "CustomerName": "Acme", "CreditLimit": 5000 }`

---

#### Q2. (R) Review this export endpoint. A partner sends `Accept: application/xml` but always receives JSON with HTTP 200.

```csharp
[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    [HttpGet("{id:int}/export")]
    public IActionResult Export(int id)
    {
        var report = _reports.Build(id);
        return Ok(report);
    }
}
```

`Program.cs` calls `AddControllers()` only — no XML formatters registered.

---

#### Q3. (R) Review content negotiation failure handling. QA expects HTTP 406 when an unsupported `Accept` header is sent; API returns JSON 200.

```csharp
[HttpGet("{id:int}")]
[Produces("application/json")]
public IActionResult Get(int id)
{
    var dto = _svc.Get(id);
    return Ok(dto);
}
```

Request: `GET /api/items/1` with header `Accept: application/pdf`

---

#### Q4. (P) Explain how `System.Text.Json` camelCase naming is configured in ASP.NET Core 8 Web APIs, and why `[JsonPropertyName("customer_name")]` on one property affects the whole contract story.

---

#### Q5. (R) Review custom formatter registration. CSV downloads work locally but return empty bodies in staging — logs show formatter selected but model type mismatch.

```csharp
// Program.cs
builder.Services.AddControllers(options =>
{
    options.OutputFormatters.Add(new CsvOutputFormatter());
});

public class CsvOutputFormatter : TextOutputFormatter
{
    public CsvOutputFormatter()
    {
        SupportedMediaTypes.Add("text/csv");
        SupportedEncodings.Add(Encoding.UTF8);
    }

    protected override bool CanWriteType(Type? type) => true;

    public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding encoding)
    {
        var rows = (IEnumerable<OrderDto>)context.Object!;
        await context.HttpContext.Response.WriteAsync(ToCsv(rows));
    }
}

[HttpGet("export")]
[Produces("text/csv")]
public IActionResult Export() => Ok(_orders.All());
```

---

#### Q6. (M) A bank middleware still requires SOAP/XML for one endpoint while the rest of the platform is JSON. Where do input and output formatters sit in the pipeline relative to model binding, and what does `[Consumes("application/xml")]` change?

---

#### Q7. (D) Leadership wants to drop XML support to reduce maintenance. One state-government client still posts `application/xml` to `POST /api/permits`. How do you decide retire vs adapter vs gateway translation?

---

#### Q8. (R) Review `[Produces]` and `[ProducesResponseType]` usage. Swagger advertises XML and JSON responses; production returns JSON only and clients cache wrong content type.

```csharp
[ApiController]
[Route("api/catalog")]
[Produces("application/json", "application/xml")]
public class CatalogController : ControllerBase
{
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    public IActionResult Get(int id) => Ok(_catalog.Get(id));
}
```

No XML serializer configured; `AddControllers()` without `AddXmlSerializerFormatters()`.
