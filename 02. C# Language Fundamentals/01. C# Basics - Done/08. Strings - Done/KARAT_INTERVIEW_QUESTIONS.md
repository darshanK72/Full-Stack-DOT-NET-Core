# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/01. C# Basics - Done/08. Strings - Done`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A nightly export job builds a CSV of 50,000 warehouse scan lines. After deployment, CPU and Gen0 GC spikes correlate with the export window. Review the row builder:

```csharp
public static string BuildExportCsv(IEnumerable<ParsedScanLine> rows)
{
    string csv = "OrderId,ServiceLevel,SkuSummary\n";
    foreach (var row in rows)
    {
        csv += $"{row.OrderId},{row.ServiceLevel},{row.SkuSummary}\n";
    }
    return csv;
}
```

What is wrong, and how would you fix it for production?

---

#### Q2. (R) A developer "normalizes" incoming scan text before lookup but duplicate orders still appear in the database. Review:

```csharp
public static string NormalizeOrderId(string rawScanLine)
{
    string cleaned = rawScanLine.Trim();
    cleaned.ToUpperInvariant(); // normalize case for lookup
    return cleaned;
}

public static bool OrderExists(string normalizedId, HashSet<string> knownOrders)
{
    return knownOrders.Contains(normalizedId);
}
```

What breaks, and what would you change?

---

#### Q3. (R) An API endpoint accepts a scan line and compares the caller's API key to a configured secret. Review:

```csharp
public bool ValidateScanRequest(string apiKeyHeader, string configuredKey, string rawScanLine)
{
    if (apiKeyHeader == configuredKey)
    {
        string orderId = rawScanLine.Trim().Split('|')[0];
        return orderId.StartsWith("ORD");
    }
    return false;
}
```

What security and correctness issues do you see, and how would you fix them?

---

#### Q4. (R) A label-printing service crashes intermittently when optional notes are omitted from the request. Review:

```csharp
public static string BuildLabel(string orderId, string notes)
{
    if (notes == "")
    {
        notes = null; // treat empty as missing
    }

    string header = $"Ship: {orderId}";
    string footer = "Notes: " + notes.Trim(); // append customer notes
    return header + Environment.NewLine + footer;
}
```

What fails at runtime, and how would you harden null/empty handling?

---

#### Q5. (P) Your team formats shipping labels with `$"Total: {orderTotal:C}"` and writes JSON audit logs on servers in `en-US`, `de-DE`, and `ja-JP`. Finance reports totals that do not reconcile across regions. Explain what is happening and what formatting approach you would standardize for display vs wire/storage.

---

#### Q6. (D) A code review proposes replacing all `StringBuilder` usage with string interpolation because "strings are simpler." The PR touches both a 3-line title builder and `LabelAssembler.BuildFullLabel`-style code that loops over hundreds of SKUs. What guidance would you give — when is `StringBuilder` worth it, and when is plain string composition enough?

---
