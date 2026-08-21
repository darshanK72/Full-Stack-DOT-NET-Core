# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/01. C# Basics - Done/03. Input & Output - Done`

---

#### Q1. (R) A batch pricing tool prompts for quantity over stdin in a CI pipeline (`dotnet run < empty.txt`). Review this handler — what fails at runtime, and how would you fix it?

**Answer:** `Console.ReadLine()` returns `null` when stdin is closed or empty, and `int.Parse(null)` throws `ArgumentNullException` — worse, `Parse` on malformed text throws `FormatException`. Piped batch jobs need null-safe reads and non-throwing validation with errors on stderr.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | No null check after `ReadLine()` | `ArgumentNullException` when stdin closes (empty file, Ctrl+D, broken pipe) |
| Runtime | `int.Parse` on user/piped input | `FormatException` on `"3.5"`, `"twelve"`, or blank lines — unhandled crash |
| Operability | Errors go to stdout via `WriteLine` | Pipelines that redirect stdout lose actionable failure text |
| Design | Interactive prompt pattern in non-interactive CI | Job hangs or fails instead of reading quantity from args/env |

**Fix (priority order):**

1. Null-check `ReadLine()`; treat null/whitespace as invalid input, write to `Console.Error`, exit non-zero.
2. Replace `Parse` with `int.TryParse(..., NumberStyles.Integer, CultureInfo.InvariantCulture, out qty)` for piped/batch input.
3. For CI, accept quantity via command-line args or env var — skip `ReadLine` entirely in headless runs.
4. Loop or fail fast with usage text: `"Usage: PricingTool --qty 3"` printed to stderr.

```csharp
string? input = Console.ReadLine();
if (input is null || !int.TryParse(input.Trim(), NumberStyles.Integer,
        CultureInfo.InvariantCulture, out int qty))
{
    Console.Error.WriteLine("Invalid or missing quantity.");
    Environment.Exit(1);
}
```

**Production takeaway:** ReadLine returning `null` is easy to miss in demos that use simulated strings — Karat tests whether you treat stdin as unreliable. See **Program.cs** Section 3 — null when input is closed; Section 8 — prefer TryParse for user input.

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

**Answer:** Both overloads use `CultureInfo.CurrentCulture` implicitly — under `de-DE`, `decimal.TryParse("1.234,56")` succeeds but `"1,234.56"` fails silently (`false`), so orders are skipped or misread without an exception. The sample line also mixes key-value text with locale-specific decimals, so naive `TryParse` on the whole line never succeeds.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `TryParse` without explicit `IFormatProvider` | Parsing depends on host/container culture — same string passes locally (en-US) and fails in de-DE pod |
| Correctness | No separation of `"Qty=3"` structure from numeric token | Returns `false` for entire line — silent data loss in batch import |
| Operability | `TryParse` failure looks like "bad data" | Operators cannot distinguish format mismatch vs genuinely invalid numbers |
| Design | Piped ERP exports use invariant or fixed locale | CurrentCulture in containers follows image/OS, not data source |

**Fix (priority order):**

1. Parse structured fields first (split on `,`, extract `UnitPrice=` value) — do not pass the whole line to `TryParse`.
2. Pass **`CultureInfo.InvariantCulture`** (or the ERP's known culture) explicitly: `decimal.TryParse(token, NumberStyles.Number, CultureInfo.InvariantCulture, out price)`.
3. Log culture name and rejected token to `Console.Error` when parse fails — aids cross-region debugging.
4. Document expected wire format in the CLI `--help`; reject mixed-locale files early with a clear message.

```csharp
decimal.TryParse(priceToken, NumberStyles.Number,
    CultureInfo.InvariantCulture, out price);
```

**Production takeaway:** TryParse "never throws" makes culture bugs invisible — production importers must pin culture to the **data contract**, not the thread. See **Program.cs** Section 5 — InvariantCulture for wire/API; Section 8 — TryParse with `NumberStyles` and provider.

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

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime / I/O | `Console.SetOut(buffer)` never restored | All subsequent stdout goes to discarded `StringWriter` after method returns — terminal appears "dead" |
| Correctness | `StringWriter` disposed while still set as `Console.Out` | Potential `ObjectDisposedException` on later writes depending on timing |
| Maintainability | Missing `try/finally` vs chapter pattern | First capture in a long-lived worker breaks all logging for process lifetime |
| Testing | Tests that capture output may pass once then flake | Order-dependent failures in test suites |

**Fix (priority order):**

1. Wrap body in `try/finally` and call `Console.SetOut(original)` in `finally` — match **Program.cs** Section 2a.
2. Restore **before** returning captured text (finally runs before return — safe).
3. Prefer injecting `TextWriter` or `ILogger` instead of mutating global `Console.Out` in production services.
4. Add a test that calls capture twice and asserts terminal output still works.

```csharp
try
{
    Console.SetOut(buffer);
    Console.WriteLine("Receipt for {0}", customer);
    Console.WriteLine("Total: {0:C2}", total);
    return buffer.ToString();
}
finally
{
    Console.SetOut(original);
}
```

**Production takeaway:** Global stream redirection is convenient for unit tests but dangerous in long-running processes — always restore in `finally`. See **Program.cs** `CaptureFormattedReceipt` — save, redirect, restore pattern.

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

**Answer:** Shell redirection sends **stdout** (`Console.Out`) to `export.csv` and **stderr** (`Console.Error`) to `errors.log`. Skipped-line diagnostics correctly belong on stderr, but the successful CSV rows also went to stderr — meaning the developer likely used `Console.Error.WriteLine` for data rows (or redirected streams in code). The empty CSV proves machine-readable output must use stdout exclusively; human diagnostics use stderr.

- **Stream contract:** stdout = pipeable payload (CSV, JSON lines); stderr = logs, skip messages, progress — matches **Program.cs** Section 2 (Out vs Error).
- **Likely bug:** Data rows written with `Console.Error.WriteLine` "so errors stand out" during local dev — breaks shell redirection in K8s.
- **Fix:** `Console.WriteLine` (stdout) for `{sku},{qty},{amount:F2}`; `Console.Error.WriteLine` only for `"Skipping bad line"`.
- **Container buffering:** If using `Console.Write` without newlines for progress, logs may batch — use line-delimited records and `Flush()` if needed.
- **Validation:** Integration test that runs `dotnet run < sample.txt > out.csv 2> err.log` and asserts row count in `out.csv` only.

**Production takeaway:** Stdout/stderr separation is a deployment contract — mixing them breaks every `>` / `2>` pipeline and log agents that treat stdout as data. See **Program.cs** Section 2 — redirect examples (`>`, `2>`, `<`).

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

**Answer:** `{0:C2}` correctly uses `CurrentCulture` for human display, but `ToString("F2")` without a provider uses **CurrentCulture** too — under `en-IN` or locales that use `,` as the decimal separator, the JSON file contains `"total":1234567,89`, which is invalid JSON and fails parsers expecting `.` as the decimal point.

- **Display path:** Keep `Console.WriteLine("{0:C2}", orderTotal)` or `ToString("C2", CultureInfo.GetCultureInfo("en-IN"))` for operators.
- **Wire path:** Always pass **`CultureInfo.InvariantCulture`** (or `CultureInfo.GetCultureInfo("en-US")` for numbers) on fixed-format exports: `orderTotal.ToString("F2", CultureInfo.InvariantCulture)`.
- **Safer JSON:** Use `System.Text.Json` serialization instead of manual string interpolation — avoids locale entirely for numeric fields.
- **Thread culture:** If the CLI temporarily sets `CultureInfo.CurrentCulture` for UI (see **Program.cs** Section 5e), wire writes must still pass invariant explicitly — thread culture leaks into `ToString("F2")`.

```csharp
Console.WriteLine("Display total: {0:C2}", orderTotal);
string wire = orderTotal.ToString("F2", CultureInfo.InvariantCulture);
File.WriteAllText("payload.json", $"{{\"total\":{wire}}}");
```

**Production takeaway:** "Invariant for logs and APIs, CurrentCulture for humans" — omitting the provider on `ToString` is one of the most common cross-region production bugs. See **Program.cs** Section 4 — `String.Format(InvariantCulture, …)` for export price; Section 5 — culture comparison table.

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

**Answer:** Use **TryParse loops** (B) for interactive kiosk and operator CLIs where recovery without a stack trace is required; reserve **Parse** (A) only for trusted, pre-validated config (embedded defaults, known-good args) or after schema validation — never on raw `ReadLine()` in production.

**When A is acceptable:**

- Input is already validated (regex, config file, unit test fixture) and failure should crash fast during development.
- Prototype or tutorial code where the instructor wants learners to see exception types — not shipped CLIs.

**When B is required for production:**

- Any human-typed or piped stdin — Parse turns `"3.0"` or empty Enter into an unhandled exception and exit code unrelated to business rules.
- Errors should go to `Console.Error` so stdout stays clean for scripting.

**Traps that remain in B:**

1. **`ReadLine()` returns null** — loop must break or exit when stdin closes; otherwise infinite `"Enter a whole number"` on EOF.
2. **Culture** — B pins InvariantCulture (good for wire-style input); if operators type locale-specific decimals, pass their culture or document integer-only input.
3. **No retry limit** — unattended scripts with bad stdin spin forever; cap retries or fail after N attempts with exit code 1.
4. **Silent `0`** — distinguish parse failure from legitimate zero if business rules care.

**Production takeaway:** The chapter previews TryParse precisely because user input is unreliable — Karat tests prioritization (TryParse + stderr + null + culture) over memorizing Parse signatures. See **Program.cs** Section 8 — TryParse pattern; `DemonstrateParseFailure` — errors on `Console.Error`.

---
