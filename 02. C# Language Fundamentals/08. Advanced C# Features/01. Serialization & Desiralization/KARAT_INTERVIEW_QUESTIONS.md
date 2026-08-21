# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/08. Advanced C# Features/01. Serialization & Desiralization/`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A Redis-backed session service deserializes cached JSON on every request. Review this code — what breaks under load or attack, and what do you fix first?

```csharp
public sealed class SessionStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public UserSession? Load(string redisKey)
    {
        string json = _redis.GetString(redisKey)!;
        return JsonSerializer.Deserialize<UserSession>(json, Options);
    }
}

public sealed class UserSession
{
    public string UserId { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public List<UserSession> Nested { get; set; } = new();
}
```

---

#### Q2. (R) After deploying a new order endpoint, mobile clients get `400`/`500` on deserialize while Postman with the old payload works. Review the handler:

```csharp
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

app.MapPost("/orders", (HttpRequest req) =>
{
    using var reader = new StreamReader(req.Body);
    string body = reader.ReadToEndAsync().Result;

    var dto = JsonSerializer.Deserialize<OrderDto>(body, new JsonSerializerOptions());
    return Results.Ok(dto);
});

public record OrderDto(
    [property: JsonPropertyName("order_id")] int OrderId,
    OrderStatus Status,
    string CustomerName);

public enum OrderStatus { Pending, Shipped, Cancelled }
```

Client JSON: `{ "orderId": 42, "status": "Shipped", "customerName": "Acme" }`

---

#### Q3. (R) A partner integration writes invoice lines to XML nightly; the job fails on first deploy with `InvalidOperationException`. Review the model and serializer usage:

```csharp
public sealed class InvoiceLine
{
    public InvoiceLine(int sku, decimal unitPrice)
    {
        Sku = sku;
        UnitPrice = unitPrice;
    }

    public int Sku { get; set; }
    public decimal UnitPrice { get; set; }
}

public static void ExportLines(IEnumerable<InvoiceLine> lines, Stream target)
{
    var serializer = new XmlSerializer(typeof(List<InvoiceLine>));
    serializer.Serialize(target, lines.ToList());
}
```

---

#### Q4. (P) Production still reads `.bin` session files produced years ago with `BinaryFormatter`. You must migrate to System.Text.Json without taking downtime. What is your rollout strategy, and why is "just flip a switch" unsafe?

---

#### Q5. (D) You are adding an optional `IsVerified` flag to a public REST DTO. v1 clients never send the field; v2 clients may send `true`, `false`, or omit it. You need to distinguish "not verified yet" from "explicitly false." Should the property be `bool` or `bool?`, and how does System.Text.Json treat a missing member for each?

---

#### Q6. (M) An org-chart API returns departments with parent/child links wired both ways. A developer enables cycle handling and ships:

```csharp
var options = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    ReferenceHandler = ReferenceHandler.IgnoreCycles
};

string json = JsonSerializer.Serialize(rootDepartment, options);
return Results.Content(json, "application/json");
```

Sample response excerpt: `"parent": null` on a child that definitely has a parent in memory. A mobile client renders the tree incorrectly. What happened, and when would you choose `ReferenceHandler.Preserve` instead?

---

#### Q7. (R) A config-sync worker reads JSON settings files from a shared folder (any authenticated internal user can drop files). Review the ingestion path:

```csharp
public AppSettings LoadSettings(string path)
{
    string json = File.ReadAllText(path);
    var settings = JsonSerializer.Deserialize<AppSettings>(json);
    _cache.Set("app-settings", settings);
    return settings;
}

public sealed class AppSettings
{
    public string ApiBaseUrl { get; set; } = string.Empty;
    public int RetryCount { get; set; } = 3;
    public Dictionary<string, object> FeatureFlags { get; set; } = new();
}
```

The team added `FeatureFlags` to support dynamic toggles. What runtime and security issues appear when arbitrary JSON lands in this path?
