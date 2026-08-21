# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/01. C# Basics - Done/03. Input & Output - Done`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A batch pricing tool prompts for quantity over stdin in a CI pipeline (`dotnet run < empty.txt`). Review this handler — what fails at runtime, and how would you fix it?

```csharp
public static void PromptAndPrice(decimal unitPrice)
{
    Console.Write("Enter quantity: ");
    string input = Console.ReadLine();
    int qty = int.Parse(input.Trim());
    decimal total = unitPrice * qty;
    Console.WriteLine("Line total: {0:C2}", total);
}
```

---

#### Q2. (R) A containerized kiosk app runs with `CultureInfo.CurrentCulture` set to `de-DE`. Operators pipe order files from a US-based ERP. Review the parser:

```csharp
public static bool TryReadQuantity(string line, out int quantity)
{
    return int.TryParse(line.Trim(), out quantity);
}

public static bool TryReadUnitPrice(string line, out decimal price)
{
    return decimal.TryParse(line.Trim(), out price);
}
```

Sample piped line: `"Qty=3, UnitPrice=1.234,56"`. What breaks, and what would you change?

---

#### Q3. (R) A developer copies the receipt-capture pattern from this chapter's `CaptureFormattedReceipt` but omits cleanup. Review:

```csharp
public static string CaptureReceipt(string customer, decimal total)
{
    TextWriter original = Console.Out;
    using StringWriter buffer = new StringWriter(CultureInfo.InvariantCulture);
    Console.SetOut(buffer);

    Console.WriteLine("Receipt for {0}", customer);
    Console.WriteLine("Total: {0:C2}", total);
    return buffer.ToString();
}
```

After the first call, later `Console.WriteLine` calls in the same process produce no terminal output. Diagnose and fix.

---

#### Q4. (P) A .NET 8 worker deployed to Kubernetes reads config lines from stdin and writes a summary CSV to stdout. Ops runs:

```bash
kubectl run job --image=pricing-worker -- sh -c "cat orders.txt | dotnet PricingWorker.dll > /shared/export.csv 2> /shared/errors.log"
```

The job completes, but `export.csv` is empty while `errors.log` contains valid rows formatted as `"SKU,Qty,Total"`. The code mixes streams like this:

```csharp
foreach (var line in ReadLines())
{
    if (!decimal.TryParse(line, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
    {
        Console.WriteLine("Skipping bad line: {0}", line);
        continue;
    }
    Console.WriteLine("{0},{1},{2:F2}", sku, qty, amount);
}
```

Explain what went wrong and how you would structure stdout vs stderr for piped/container runs.

---

#### Q5. (M) An internal CLI formats currency for operators in Mumbai (`en-IN`) but must emit a fixed wire-format total for downstream JSON consumers. Review:

```csharp
decimal orderTotal = 1234567.89m;

Console.WriteLine("Display total: {0:C2}", orderTotal);
Console.WriteLine("Wire total: {0}", orderTotal.ToString("F2"));
File.WriteAllText("payload.json",
    $"{{\"total\":{orderTotal.ToString("F2")}}}");
```

The JSON consumer in `eu-west-1` intermittently rejects payloads. What is the culture bug, and how do you fix display vs wire formatting?

---

#### Q6. (D) A team building Express-Mart-style kiosk CLIs debates input validation strategy for numeric prompts. Two approaches:

**A — Parse (throws on bad input):**

```csharp
Console.Write("Quantity: ");
int qty = int.Parse(Console.ReadLine()!);
```

**B — TryParse loop (from this chapter's preview):**

```csharp
int qty;
while (!int.TryParse(Console.ReadLine(), NumberStyles.Integer,
    CultureInfo.InvariantCulture, out qty))
{
    Console.Error.WriteLine("Enter a whole number.");
}
```

When would you choose each in production CLIs vs interactive tutorials, and what traps remain in B?

---
