# Karat — Interview Questions

> **Folder:** `07. ASP.Net Core Web API/06. Swagger & OpenAPI`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)  
> **Raw debrief:** [KARAT_DEBRIEF_SOURCE.txt](../../00. Notes & Practice/04. Citi Karat Interview Practice/KARAT_DEBRIEF_SOURCE.txt)

---

#### Q1. (R) Review this `Program.cs` from a production Web API. Swagger UI is reachable at `/swagger` in all environments; security review flagged it before go-live.

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Payments API v1"));

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

Deployment: public internet, no IP allowlist, JWT-protected endpoints documented with real example tokens in `appsettings.Development.json`.

---

#### Q2. (R) Review Swagger schema generation. Two DTOs in different namespaces share the same class name; generated OpenAPI shows merged/wrong properties and TypeScript client fails to compile.

```csharp
namespace Acme.Api.Contracts.v1 { public class ProductDto { public int Id { get; set; } public string Sku { get; set; } = ""; } }
namespace Acme.Api.Internal { public class ProductDto { public int Id { get; set; } public decimal Cost { get; set; } public string SupplierCode { get; set; } = ""; } }

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Catalog API", Version = "v1" });
});
```

Both types appear on different controller actions exposed in the same document.

---

#### Q3. (P) A PATCH DTO uses `bool?` for tri-state fields and `DateOnly?` for optional dates. Swagger UI and NSwag-generated clients show wrong nullability — required fields where omission is valid. How do you align OpenAPI schema with actual JSON binding behavior in ASP.NET Core 8?

---

#### Q4. (P) Review JWT authentication setup for Swagger UI on an internal admin API. Developers paste bearer tokens manually today; you want "Authorize" in Swagger UI without leaking production secrets in the repo.

```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Admin API", Version = "v1" });
    // no security definition yet
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(/* ... */);
```

What do you add to OpenAPI and Swagger UI, and what must stay out of source control?

---

#### Q5. (R) Review this minimal API + Swashbuckle setup. Swagger document is empty — no operations listed — but controllers in the same project document correctly when migrated.

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => Results.Ok("healthy"));

app.MapPost("/orders", (CreateOrderRequest req) =>
{
    return Results.Created($"/orders/{req.Id}", req);
});

app.Run();
```

`CreateOrderRequest` is a public record; endpoint uses lambda without `[FromBody]` attributes.

---

#### Q6. (D) A team debates exposing EF Core entity types directly in Swagger instead of separate response DTOs to "move faster." Review the API surface and documentation implications for versioning, security, and client coupling.

```csharp
[HttpGet("{id:int}")]
[ProducesResponseType(typeof(Order), 200)]
public async Task<ActionResult<Order>> Get(int id)
    => await _db.Orders.Include(o => o.LineItems).FirstOrDefaultAsync(o => o.Id == id);

// Order entity includes: InternalNotes, CostPrice, RowVersion, SupplierId
```

OpenAPI generates full `Order` schema including all mapped columns.

---

#### Q7. (M) Walk through what `AddSwaggerGen` produces at startup vs runtime, how `IncludeXmlComments` affects the document, and why `c.CustomSchemaIds(type => type.FullName)` is commonly added in larger APIs — tie to schema collision and polymorphic `$ref` behavior.

---

#### Q8. (R) Review multi-version OpenAPI setup. v1 and v2 controllers exist but Swagger UI only shows v1 operations; v2 clients get wrong schemas for shared DTO names.

```csharp
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
    options.SwaggerDoc("v2", new OpenApiInfo { Title = "API", Version = "v2" });
});

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    // v2 endpoint not registered
});

// v2 controller: [ApiVersion("2.0")] on separate controller type
```

No `DocInclusionPredicate` or `ConfigureSwaggerOptions` ties actions to documents.
