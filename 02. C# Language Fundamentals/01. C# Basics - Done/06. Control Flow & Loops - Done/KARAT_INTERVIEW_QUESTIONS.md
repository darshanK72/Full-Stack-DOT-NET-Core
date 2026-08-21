# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/01. C# Basics - Done/06. Control Flow & Loops - Done`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A developer ports a C-style fulfillment router into C#. The build fails with CS0163. Review the switch — what is wrong, and how would you fix it while preserving the shared "in transit" behavior?

```csharp
public static string RouteStatus(string status)
{
    switch (status)
    {
        case "Pending":
            return "Awaiting pick list";
        case "Picking":
            return "Items being collected";
        case "Packed":
            message = "Ready for carrier";
        case "Shipped":
        case "Delivered":
            return "In transit pipeline";
        default:
            return "Unknown — escalate";
    }
}
```

---

#### Q2. (R) A nightly batch job counts warehouse slots for billing. QA reports the invoice is one slot short for every aisle. Review the nested loop:

```csharp
public static int CountBillableSlots(int aisles, int shelvesPerAisle)
{
    int count = 0;
    for (int aisle = 1; aisle < aisles; aisle++)
    {
        for (int shelf = 1; shelf <= shelvesPerAisle; shelf++)
        {
            count++;
        }
    }
    return count;
}
// Called with CountBillableSlots(5, 10) — ops expects 50 slots.
```

What is wrong, and what would you change?

---

#### Q3. (R) A gate-controller service hangs in staging after a config change. Review the retry loop:

```csharp
public static bool WaitForGateOpen(int maxAttempts)
{
    int attempt = 0;
    while (attempt < maxAttempts)
    {
        if (TryOpenGate())
        {
            return true;
        }
        // forgot to increment attempt
    }
    return false;
}
```

Another team member "fixes" it with `while (true)` and a `break` inside `TryOpenGate()` that only runs on success — but `maxAttempts` is never checked. What breaks in each version, and what is the safe bounded-retry pattern?

---

#### Q4. (R) A pick-list optimizer searches a 2D bin grid for the first high-priority SKU. It finds the SKU but keeps scanning every remaining aisle. Review:

```csharp
public static (int row, int col)? FindPrioritySku(string[,] grid, string target)
{
    for (int row = 0; row < grid.GetLength(0); row++)
    {
        for (int col = 0; col < grid.GetLength(1); col++)
        {
            if (grid[row, col] == target)
            {
                continue; // found — move to next cell
            }
        }
    }
    return null;
}
```

What is wrong with `continue` here, and how would you fix it for early exit?

---

#### Q5. (R) A pricing API uses pattern matching on order payloads. Support tickets report zero-quantity lines labeled as "positive." Review:

```csharp
public static string ClassifyLine(object line)
{
    switch (line)
    {
        case int qty:
            return qty > 0 ? $"Active: {qty} units" : $"Zero qty: {qty}";
        case int units when units > 0:
            return $"Positive quantity: {units}";
        case string sku when sku.Length > 0:
            return $"SKU: {sku}";
        default:
            return "Unsupported";
    }
}
// ClassifyLine(0) returns "Active: 0 units" — product owner expected "Zero qty: 0".
```

What is wrong, and how do case order and `when` guards interact?

---

#### Q6. (R) A barcode scan worker sums active line quantities but under-reports totals. Review:

```csharp
public static int SumActiveLines(int[] quantities)
{
    int total = 0;
    foreach (int qty in quantities)
    {
        if (qty <= 0)
            break; // skip bad lines
        total += qty;
    }
    return total;
}
// Input: { 2, 0, 5, 3 } — expected 10, actual 2.
```

What is wrong, and what is the difference between `break` and `continue` in this loop?
