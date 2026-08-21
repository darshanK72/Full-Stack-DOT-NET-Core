# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/01. C# Basics - Done/05. Type Conversion & Casting - Done`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A warehouse pricing service receives quantities from an upstream JSON deserializer boxed as `object`. Review this method — what fails at runtime, and how would you fix it?

```csharp
public decimal CalculateLineTotal(object quantityBoxed, decimal unitPrice)
{
    long units = (long)quantityBoxed;   // upstream stored int.Parse result as object
    return units * unitPrice;
}

// Caller:
object qty = int.Parse("36");           // implicit box — int on heap
var total = CalculateLineTotal(qty, 49.99m);
```

---

#### Q2. (R) An ASP.NET Core order API accepts a quantity path segment. Review the action — what breaks for bad input, and what would you change?

```csharp
[HttpGet("orders/{quantity}")]
public IActionResult GetOrderLine(string quantity)
{
    int units = int.Parse(quantity);
    if (units <= 0)
        return BadRequest("Quantity must be positive.");

    var line = _orderService.BuildLine(units);
    return Ok(line);
}
```

---

#### Q3. (R) A bonus-units endpoint maps optional query text to an integer. Review both methods — which hidden behavior causes incorrect totals in production?

```csharp
public int GetBonusUnitsFromQuery(string? bonusText)
{
    return Convert.ToInt32(bonusText);   // missing query → null
}

public int GetBonusUnitsStrict(string? bonusText)
{
    return int.Parse(bonusText!);        // developer added null-forgiving
}
```

---

#### Q4. (R) Inventory assigns shelf slot IDs stored in a `byte` column. Review this service — what corrupts data silently, and how do you prevent it?

```csharp
public byte AssignShelfSlot(int inventoryLocationId)
{
    // Location IDs come from a legacy int column; values can exceed 255
    return (byte)inventoryLocationId;
}

public void PersistSlot(int locationId)
{
    byte slot = AssignShelfSlot(locationId);
    _db.Execute("UPDATE bins SET slot_id = @slot", new { slot });
}
```

---

#### Q5. (R) A shipping label builder walks a heterogeneous `List<object>` of line items. Review this code — what throws or returns wrong data?

```csharp
public string BuildLabel(object lineItem)
{
    if (lineItem is PhysicalLineItem)
        return ((PhysicalLineItem)lineItem).Sku;

    var digital = lineItem as DigitalLineItem;
    return digital.DownloadCode;   // digital is null for unknown types

    // Caller also tried: var n = lineItem as int;  // CS0039 — won't compile on object
}
```

---

#### Q6. (P) A partner integration POSTs prices as formatted strings in JSON (`"amountText": "1.234,56"`). The API runs on en-US servers. Review the handler — what fails across environments, and what contract would you enforce?

```csharp
public record PriceDto(string Sku, string AmountText);

[HttpPost("prices")]
public IActionResult ImportPrice([FromBody] PriceDto dto)
{
    var price = decimal.Parse(dto.AmountText);   // current thread culture
    _catalog.SavePrice(dto.Sku, price);
    return Ok(new { dto.Sku, price });
}
```

---
