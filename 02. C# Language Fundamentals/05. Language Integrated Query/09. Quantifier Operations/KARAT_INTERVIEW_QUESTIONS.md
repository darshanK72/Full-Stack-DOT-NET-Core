# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/05. Language Integrated Query/09. Quantifier Operations`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A warehouse API loads pick-list rows from a repository that returns `IEnumerable<OrderLine>` (not materialized). A developer gates shipment release like this. Review the check — what is wrong with using `Count()` here, and what would you change?

```csharp
IEnumerable<OrderLine> batch = _pickListRepository.GetOpenLines(shipmentId);

if (batch.Count() > 0 && batch.All(line => line.Quantity > 0))
{
    await _carrierService.ReleaseAsync(shipmentId);
}
```

---

#### Q2. (R) A dock validation service treats an empty pick list as "ready to ship" in production. Review the rule:

```csharp
List<OrderLine> batch = await _repository.GetBatchAsync(batchId); // may return []

bool readyForStandardCarrier =
    batch.All(line => line.Quantity > 0)
    && batch.All(line => line.UnitPrice > 0m);

if (readyForStandardCarrier)
{
    await _dockGate.OpenAsync(batchId);
}
```

What logical bug appears when `batch` is empty, and how do you fix it without changing the business meaning of the predicates?

---

#### Q3. (R) A duplicate-SKU guard runs before merging a probe line into the live pick list. QA reports it never blocks duplicates that have the same SKU but different object instances. Review the check:

```csharp
List<OrderLine> shipmentBatch = _cache.GetBatch(shipmentId);

OrderLine incoming = new OrderLine("WH-4412", "Industrial Shelving Unit", 4, 49.99m, false);

if (shipmentBatch.Contains(incoming))
{
    throw new InvalidOperationException("SKU already on pick list.");
}

shipmentBatch.Add(incoming);
```

What is wrong, and what is the minimal fix for SKU-based membership?

---

#### Q4. (M) An audit hook logs every time a hazardous line is evaluated. The batch has one hazardous SKU at index 0 and three non-hazardous lines after it. Predict how many log lines each expression produces and whether enumeration stops early:

```csharp
int auditCalls = 0;

bool A = batch.Any(line =>
{
    auditCalls++;
    return line.IsHazardous;
});

auditCalls = 0;

bool B = batch.Count(line =>
{
    auditCalls++;
    return line.IsHazardous;
}) > 0;
```

Assume `batch` is a `List<OrderLine>` with four elements; only `batch[0].IsHazardous == true`. What are `A`, `B`, and the two `auditCalls` totals?

---

#### Q5. (R) A restricted-SKU scan uses `All` with a predicate that calls an external hazmat API per line. The second line fails the rule. Review performance and short-circuit behavior:

```csharp
bool batchClearsRestrictedList = batch.All(line =>
{
    _metrics.Increment("hazmat_api_calls");
    return !_hazmatService.IsRestrictedSku(line.Sku);
});
```

Compare this to rewriting the intent with `Any`. Which operator short-circuits on the first restricted SKU, and why does the `All` version still matter even when it returns `false` early?

---

#### Q6. (P) Carrier code validation uses `Contains` on allowed codes but the inbound scan payload varies by casing. Review both checks — which passes incorrectly in production, and what comparer belongs on the membership test?

```csharp
string[] allowedCarriers = ["UPS", "FEDEX", "DHL"];
string scannedCode = inbound.CarrierCode; // value from scanner: "fedex"

bool legacyCheck = allowedCarriers.Contains(scannedCode);
bool gateCheck = allowedCarriers.Contains(scannedCode, StringComparer.Ordinal);

if (legacyCheck || gateCheck)
{
    _gate.AllowEntry(inbound);
}
```

What breaks, and how would you align this with the string `Contains` pattern shown in **Program.cs** Section 7?
