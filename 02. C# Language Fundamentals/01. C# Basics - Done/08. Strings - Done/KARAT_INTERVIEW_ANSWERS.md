# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/01. C# Basics - Done/08. Strings - Done`

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

**Answer:** `+=` inside the loop creates a new immutable string on every iteration — for 50,000 rows that is tens of thousands of short-lived allocations and O(n²) total copying, which drives Gen0 GC pressure and CPU spikes during the export window.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance | Repeated `csv += …` in a tight loop | O(n²) char copying; massive intermediate string garbage |
| GC | One allocation per row (often two with interpolation) | Gen0 collections spike; export window slows other services on shared nodes |
| Scalability | Entire CSV held in one `string` at the end | Large LOH pressure if export grows beyond memory budget |
| Correctness (secondary) | Unescaped commas in `SkuSummary` | Broken CSV columns — separate fix, but often discovered during export rewrites |

**Fix (priority order):**

1. Use `StringBuilder` with a reasonable initial capacity (header size + estimated row width × count) and `AppendLine` / `AppendFormat` inside the loop — one final `ToString()` or stream write at the end.
2. For very large exports, write directly to a `StreamWriter` or `PipeWriter` instead of materializing the whole file in memory.
3. Escape or quote CSV fields that may contain commas (proper CSV encoding).
4. Add a benchmark or integration test with a representative row count so regressions show up in CI.

```csharp
public static string BuildExportCsv(IReadOnlyList<ParsedScanLine> rows)
{
    var sb = new StringBuilder(capacity: 64 + rows.Count * 48);
    sb.AppendLine("OrderId,ServiceLevel,SkuSummary");
    foreach (var row in rows)
    {
        sb.Append(row.OrderId).Append(',')
          .Append(row.ServiceLevel).Append(',')
          .Append(row.SkuSummary).AppendLine();
    }
    return sb.ToString();
}
```

**Production takeaway:** String concatenation in loops is a classic immutability trap — `string` is immutable, so every `+=` allocates. See **Program.cs** Section 11 (`LabelAssembler`) and the quick-reference "s1 + s2 in tight loop" row. Use `StringBuilder` or streaming for repeated building.

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

**Answer:** `ToUpperInvariant()` returns a new string and does not mutate `cleaned` — the discarded result means lookup keys keep original casing, so `"ord-1042"` and `"ORD-1042"` are treated as different orders and duplicates slip through.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `ToUpperInvariant()` result not assigned | Normalization silently skipped — case variants create duplicate records |
| API design | Method name implies normalization but returns partially cleaned text | Misleading contract for callers and code reviewers |
| Comparison | `HashSet.Contains` uses default ordinal equality on unstabilized casing | Inconsistent with intended invariant ID rules from **ScanLineParser** |

**Fix (priority order):**

1. Reassign: `cleaned = cleaned.ToUpperInvariant();` or return in one expression: `return rawScanLine.Trim().ToUpperInvariant();`.
2. For machine identifiers prefer `ToUpperInvariant()` / `ToLowerInvariant()` over culture-sensitive `ToUpper()` — see **StringComparisonDemo.TurkishCultureCaseTrap** for the Turkish `"i"` pitfall.
3. Use `StringComparison.OrdinalIgnoreCase` at comparison boundaries if you must preserve display casing but match logically.
4. Add unit tests with mixed-case inputs asserting a single canonical key in the set.

**Production takeaway:** Immutable string methods never change the original — every transform must be reassigned or returned. This is the same class of bug as assuming `Trim()` mutates `rawScanLine` in **Program.cs** Section 2.

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

**Answer:** The API key comparison uses default string equality without a fixed comparison mode or timing-safe compare, and order-ID validation uses culture-sensitive defaults — both are risky in production authentication and machine-identifier paths.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | `apiKeyHeader == configuredKey` | Not timing-safe; default comparison mode may be culture-sensitive depending on overload resolution context |
| Security | Plain string compare for secrets | Theoretical timing side-channels; no explicit `Ordinal` / `FixedTimeEquals` |
| Correctness | `StartsWith("ORD")` without `StringComparison` | Culture-sensitive prefix rules — wrong for wire/machine IDs (see **ScanLineParser** — uses `StringComparison.Ordinal`) |
| Runtime | `Split('|')[0]` without length/null checks | `IndexOutOfRangeException` or `NullReferenceException` on malformed input |
| Input validation | No `IsNullOrWhiteSpace` on `rawScanLine` | Crashes or accepts garbage scan lines |

**Fix (priority order):**

1. Compare secrets with `CryptographicOperations.FixedTimeEquals` on UTF-8 bytes, or at minimum `string.Equals(apiKeyHeader, configuredKey, StringComparison.Ordinal)`.
2. Use `orderId.StartsWith("ORD", StringComparison.Ordinal)` for machine identifiers — never `CurrentCulture` for IDs, tokens, or paths.
3. Guard inputs: reject null/whitespace scan lines; validate segment count after `Split` (mirror **ScanLineParser.Parse** pipe-delimited structure).
4. Return `false` (or 401) without leaking which check failed — do not branch logic that reveals key vs payload validity.

**Production takeaway:** `StringComparison.Ordinal` / `OrdinalIgnoreCase` for IDs, headers, and secrets; `CurrentCulture` only for user-facing sort/display. See **Program.cs** Section 6 comparison table and Section 11 culture notes.

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

**Answer:** When the client omits `notes`, it arrives as `null`, not `""` — the empty-string check never runs, and `notes.Trim()` throws `NullReferenceException`. The method also conflates three states (null, empty, whitespace) without a consistent policy.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | `notes.Trim()` when `notes` is `null` | Intermittent crash when JSON omits optional field or deserializer passes null |
| Logic | Only normalizes `""` to null, not whitespace-only strings | `"   "` still flows through — inconsistent footer |
| API contract | Optional string not coalesced before string operations | Callers must guess whether null or empty is expected |

**Fix (priority order):**

1. Use `string.IsNullOrWhiteSpace(notes)` and branch to a safe default footer (e.g., `"Notes: (none)"`) before any instance methods.
2. Prefer null-coalescing: `var safeNotes = string.IsNullOrWhiteSpace(notes) ? "(none)" : notes.Trim();`.
3. Do not assign `notes = null` to mean missing — that increases null dereference risk; normalize to display text or `string.Empty` early.
4. Align with **Program.cs** Section 4 — `string.IsNullOrEmpty` vs `string.IsNullOrWhiteSpace` depending on whether tabs/spaces-only input should count as blank.

```csharp
public static string BuildLabel(string orderId, string? notes)
{
    string header = $"Ship: {orderId}";
    string footer = string.IsNullOrWhiteSpace(notes)
        ? "Notes: (none)"
        : "Notes: " + notes.Trim();
    return header + Environment.NewLine + footer;
}
```

**Production takeaway:** Always gate `.Trim()`, `.Split()`, and concatenation with `IsNullOrEmpty` / `IsNullOrWhiteSpace` on external input — optional API fields are usually null, not empty string.

---

#### Q5. (P) Your team formats shipping labels with `$"Total: {orderTotal:C}"` and writes JSON audit logs on servers in `en-US`, `de-DE`, and `ja-JP`. Finance reports totals that do not reconcile across regions. Explain what is happening and what formatting approach you would standardize for display vs wire/storage.

**Answer:** The `:C` format specifier uses `CultureInfo.CurrentCulture` (thread UI culture), so the same `decimal` renders as `$127.50`, `127,50 €`, or `￥127` depending on which server handled the request — audit logs and exported strings are not comparable across regions.

- **Root cause:** Interpolation `{orderTotal:C}` and `ToString("C")` without an explicit provider bind to the executing thread's culture — see **InterpolationDemo.BuildLines** and **CultureStringDemo.FormatTotals**.
- **Display (user-facing):** Format with the user's locale — `orderTotal.ToString("C", CultureInfo.CurrentCulture)` or explicit `CultureInfo.GetCultureInfo(userLocale)` in UI and printed labels.
- **Wire / storage / logs:** Use `CultureInfo.InvariantCulture` (or ISO formats like `"F2"` with invariant, or store raw `decimal` in JSON as a number) so `127.50` is identical on every machine.
- **Parsing round-trip:** If text is produced with `de-DE`, parse with the same culture — **CultureStringDemo.ParseLocalizedAmounts** shows `TryParse` failing when culture mismatches.
- **JSON specifically:** Prefer numeric JSON values for amounts; if stringified, document invariant format in the contract.

```csharp
// Label for German warehouse operator
string label = $"Total: {orderTotal.ToString("C", CultureInfo.GetCultureInfo("de-DE"))}";

// Audit log / export — stable everywhere
string audit = string.Format(CultureInfo.InvariantCulture, "Total:{0:F2}", orderTotal);
```

**Production takeaway:** Separate **display culture** from **invariant culture** — interpolation makes it easy to forget the provider. See **Program.cs** Sections 7, 10, and 11 and `FormatDemo.FormatOrderLine(IFormatProvider, …)`.

---

#### Q6. (D) A code review proposes replacing all `StringBuilder` usage with string interpolation because "strings are simpler." The PR touches both a 3-line title builder and `LabelAssembler.BuildFullLabel`-style code that loops over hundreds of SKUs. What guidance would you give — when is `StringBuilder` worth it, and when is plain string composition enough?

**Answer:** Keep interpolation and small `string.Concat` / `$"…"` for fixed, few-part text; retain `StringBuilder` (or streaming) when append count scales with data size or loop iterations — simplicity does not justify O(n²) allocations on hot paths.

**Plain strings / interpolation — sufficient when:**

- A handful of fixed parts (e.g., `$"Ship: {orderId}"` from **InterpolationDemo**).
- One-shot joins: `string.Join(" + ", skuList)` as in **ScanLineParser.Parse**.
- Static `string.Concat(prefix, "-", idPart)` for three known segments.

**StringBuilder — justified when:**

- Building text inside loops (CSV rows, log buffers, HTML tables, many SKU lines).
- Dozens of `Append` / `AppendLine` operations where chained `+` would allocate each step (**LabelAssembler.BuildFullLabel**).
- Reusing a buffer across requests (clear and re-append) to amortize capacity — `Clear()` retains capacity per Section 12 notes.

**Anti-patterns to reject in review:**

- Replacing a loop + `StringBuilder` with loop + `result += …`.
- Calling `builder.ToString()` mid-build only to search/replace unless necessary — **LabelAssembler** shows that materializing early defeats the mutable buffer; prefer `StringBuilder` search APIs or track indices.

**Production takeaway:** Choose based on allocation profile and readability, not ideology — Karat tests whether you know immutability cost, not whether you memorize `StringBuilder` API. Default to simple strings; escalate to `StringBuilder` when profiling or loop structure demands it.

---
