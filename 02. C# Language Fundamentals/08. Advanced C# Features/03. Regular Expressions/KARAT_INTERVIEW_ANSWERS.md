# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/08. Advanced C# Features/03. Regular Expressions/`

---

#### Q1. (R) A bulk-import API validates thousands of customer rows per request. After deploy, CPU spikes and some requests time out. Review this validator and prioritize fixes.

**Answer:** The validator re-parses regex patterns on every row via static helpers, accepts substring email matches because the pattern lacks anchors, and runs a nested-quantifier notes pattern with no `MatchTimeout` — together causing wasted CPU, false accepts, and potential ReDoS under adversarial notes text.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance | `Regex.IsMatch` / `Regex.Match` static calls inside per-row loop | Pattern re-parsed on every invocation — O(rows × parse cost) |
| Correctness | Email pattern missing `^` and `$` | `"junk alice@x.co more"` passes validation (substring match) |
| Runtime / security | `(order\s+\d+)+` nested quantifier with no timeout | Catastrophic backtracking on long notes → hung threads, CPU spikes |
| Design | Notes extraction mixed into boolean gate | Validation path does extra work even when email/phone already fail |

**Fix (priority order):**

1. Cache patterns in `static readonly Regex` fields (with `RegexOptions.Compiled | RegexOptions.CultureInvariant`) and call instance `.IsMatch` / `.Match`.
2. Anchor whole-field validation: `@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"` — matches **Program.cs** `EmailPattern`.
3. Add `TimeSpan` match timeout on the notes pattern (or use `RegexOptions.NonBacktracking` in .NET 7+ for untrusted text).
4. Rewrite the notes pattern without nested greedy quantifiers — e.g. `@"order\s+\d+"` with `Matches` when you need every hit.
5. Short-circuit: validate email and phone before scanning notes.

```csharp
private static readonly Regex EmailPattern = new(
    @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
    RegexOptions.Compiled | RegexOptions.CultureInvariant,
    TimeSpan.FromMilliseconds(250));

private static readonly Regex OrderInNotes = new(
    @"order\s+\d+",
    RegexOptions.IgnoreCase | RegexOptions.Compiled,
    TimeSpan.FromMilliseconds(250));
```

**Production takeaway:** Bulk import turns "fine in dev" regex into a hot path — cache instances, anchor fields, and timeout untrusted text. See **Program.cs** Sections 3, 11, and 12.

---

#### Q2. (R) A support portal lets agents paste a custom regex to search and redact matches in uploaded log files (multi-MB). Review this endpoint helper:

```csharp
public string RedactMatches(string logContent, string userPattern)
{
    var regex = new Regex(
        userPattern,
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    return regex.Replace(logContent, "[REDACTED]");
}
```

What production risks exist, and how would you harden this for untrusted input?

**Answer:** Accepting arbitrary regex from users against large inputs is a classic ReDoS vector — nested quantifiers can hang a thread indefinitely, and `Compiled` on every unique user pattern adds startup cost without helping one-off searches.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | User-supplied pattern with no timeout or engine guard | ReDoS — request thread blocked, CPU pegged, gateway timeouts |
| Security | No pattern length or complexity limits | Trivial denial-of-service via `(a+)+$`-style patterns on long logs |
| Performance | `RegexOptions.Compiled` per unique user pattern | IL emission cost on every distinct pattern; memory growth if patterns vary |
| Correctness | No validation that pattern is well-formed before `Replace` | `ArgumentException` bubbles as 500; partial redaction on invalid `$` tokens |
| Design | Regex redaction on multi-MB strings in-request | Large LOH allocations; latency spikes under concurrent uploads |

**Fix (priority order):**

1. Reject or sandbox user patterns — max length, allow-list of constructs, or disallow user regex entirely (fixed internal patterns + literal search via `Regex.Escape`).
2. Always pass `TimeSpan` match timeout: `new Regex(userPattern, options, TimeSpan.FromSeconds(2))` — catch `RegexMatchTimeoutException` and return 400.
3. Prefer `RegexOptions.NonBacktracking` (.NET 7+) for untrusted patterns — linear-time engine trades some feature support for safety.
4. Drop `Compiled` for ad hoc user patterns; cache only frequently reused admin-defined patterns in a bounded dictionary.
5. Process large logs in chunks or offload to a background worker with cancellation.

```csharp
var safe = new Regex(
    userPattern,
    RegexOptions.IgnoreCase | RegexOptions.NonBacktracking,
    TimeSpan.FromSeconds(2));
```

**Production takeaway:** Never run untrusted regex on untrusted input without timeout or NonBacktracking — Karat uses this to test ReDoS awareness, not pattern syntax recall. See **Program.cs** Sections 10–11 and `DemonstratePerformance` timeout demo.

---

#### Q3. (R) A notes-processing job extracts phone fragments for a CRM sync. Review this extractor:

```csharp
public sealed class PhoneExtractor
{
    private static readonly Regex PhonePattern = new(
        @"(\+1[-.\s]?)?(\([0-9]{3}\)|[0-9]{3})[-.\s]?[0-9]{3}[-.\s]?[0-9]{4}",
        RegexOptions.Compiled);

    public string GetAreaCode(string notes)
    {
        Match m = PhonePattern.Match(notes);
        return m.Groups[2].Value; // area-code capture
    }

    public bool HasUsPhone(string notes) =>
        Regex.IsMatch(notes, PhonePattern.ToString());
}
```

What correctness bugs appear on edge-case inputs, and how do you fix them?

**Answer:** `GetAreaCode` reads `Groups[2]` without checking `Success`, returning empty strings silently when no phone exists; `HasUsPhone` round-trips the pattern through `.ToString()` into a static call that re-parses every time and still performs partial matching anywhere in the notes string.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | No `m.Success` check before `Groups[2]` | `"no phone here"` → `""` instead of explicit "not found" — CRM gets blank area codes |
| Correctness | Phone pattern not anchored with `^…$` | Matches embedded digit runs inside longer strings — false positives |
| Performance | `Regex.IsMatch(notes, PhonePattern.ToString())` | Re-parses pattern on every call; `.ToString()` is not a reliable reuse mechanism |
| Design | Group index `[2]` is magic number | Refactoring pattern breaks silently when optional country group shifts |

**Fix (priority order):**

1. Guard on `Success` before reading groups; return `null`, `Optional`, or throw a domain exception when no match.
2. Anchor for whole-field checks: prepend `^` and append `$` when validating a dedicated phone column; use unanchored instance only for extraction inside free text (document the difference).
3. Reuse the cached instance: `PhonePattern.IsMatch(notes)` instead of static + `ToString()`.
4. Prefer named groups `(?<area>…)` and read `Groups["area"].Value` for maintainability — matches **Program.cs** Section 6.

```csharp
public string? GetAreaCode(string notes)
{
    Match m = PhonePattern.Match(notes);
    if (!m.Success)
        return null;
    return m.Groups["area"].Success ? m.Groups["area"].Value : null;
}

public bool HasUsPhone(string notes) => PhonePattern.IsMatch(notes);
```

**Production takeaway:** `Groups[n]` without `Success` is a silent data bug — Karat stacks it with static-helper misuse. See **Program.cs** Section 12 pitfalls and Section 6 named groups.

---

#### Q4. (P) Your registration API validates email on every POST (~2k RPS). A teammate proposes three options:

1. `Regex.IsMatch(email, pattern)` inline in the action  
2. `static readonly Regex` field with `RegexOptions.Compiled | RegexOptions.CultureInvariant`  
3. `[RegularExpression(@"…")]` on the DTO property  

When would you choose each, and what companion settings (timeout, anchoring, caching) are mandatory for the Regex-based approaches in production?

**Answer:** At 2k RPS, inline static calls re-parse the pattern on every request and should be replaced with a cached compiled instance; the data-annotation attribute is fine for coarse API validation but still needs a well-anchored pattern and does not replace DNS or mailbox verification.

- **Option 1 — inline static:** Acceptable only for cold paths (admin tools, one-off scripts). On a hot registration endpoint it wastes CPU re-parsing the same automaton per call — replace with Option 2.
- **Option 2 — static readonly + Compiled:** Production default for hot regex validation. Pair with `^…$` anchors, `CultureInvariant`, and a constructor `TimeSpan` timeout (e.g. 200–500 ms) even for fixed patterns — defense in depth if the pattern is ever edited badly.
- **Option 3 — `[RegularExpression]`:** Good for declarative model validation in ASP.NET Core (`[ApiController]` runs it automatically). Same anchored pattern required; attribute does not add caching or timeout by itself — underlying implementation still constructs/runs regex per validation unless you also use a custom `ValidationAttribute` wrapping a shared instance.
- **Mandatory companions for any Regex approach:** whole-field anchors; treat regex as syntax-only (follow with uniqueness check, domain policy, or confirmation email); log `RegexMatchTimeoutException` as a potential attack signal.
- **Modern alternative (.NET 7+):** `[GeneratedRegex(@"^…$")]` partial method — compile-time generated, zero runtime parse, ideal for fixed hot patterns (previewed in **Program.cs** Section 11).

**Production takeaway:** Karat tests whether you distinguish "works in a demo" from "safe at 2k RPS" — caching, anchoring, and timeout matter as much as the pattern itself.

---

#### Q5. (D) Product wants import rejection for disposable email domains (`mailinator.com`, `tempmail.org`, …) and a regex that only allows corporate TLDs. A developer merges the blocklist into one giant pattern:

```csharp
bool ok = Regex.IsMatch(email,
    @"^(?!.*@(mailinator|tempmail)\.com$)[a-zA-Z0-9._%+-]+@(?:contoso|fabrikam)\.(?:com|org)$");
```

What breaks in maintainability, testability, and correctness compared to splitting validation layers? How would you structure this in a real import pipeline?

**Answer:** One mega-pattern couples RFC-ish syntax, a disposable-domain policy, and an allow-list of employers into an unreadable string that is painful to unit test, unsafe to extend (every blocklist change recompiles regex), and still cannot verify that the mailbox exists or that the domain is typosquatted.

- **Maintainability:** Blocklists belong in configuration (`IOptions<EmailPolicyOptions>`) or a database table — not inside a pattern literal. Adding `tempmail.org` should be a config deploy, not a regex edit requiring code review of lookaheads.
- **Testability:** Layered validators get focused tests: syntax regex returns pass/fail; domain service checks blocklist and allow-list independently; integration tests compose them. A single regex forces table-driven tests with opaque expected strings.
- **Correctness:** Regex validates string shape only — it cannot detect disposable subdomains, plus-address aliases (`user+tag@contoso.com`), homoglyphs, or DNS MX existence. Negative lookahead `(?!.*@mailinator…)` is easy to get wrong and still matches `user@mailinator.com.evil.net` depending on anchoring.
- **Recommended pipeline:** (1) trim/normalize input; (2) anchored syntax regex or `MailAddress` parse for basic shape; (3) extract domain segment via `Match` named groups or string split; (4) blocklist/allow-list lookup service; (5) optional async MX/DNS check off the hot path; (6) business rules (duplicate account, region lock) in plain C#.
- **When regex fits:** syntax gate only — the same practical email pattern from **Program.cs** Section 9a, anchored with `^$`.

**Production takeaway:** Regex is a syntax filter, not a policy engine — Karat uses this to test layered validation judgment, not pattern authoring bravado.

---

#### Q6. (M) A config-ingestion worker parses key/value lines from Windows-generated files. Keys on lines after the first never match:

```csharp
string file = "Server=prod-db\r\nPort=5432\r\nTimeout=30";
bool secondLineMatches = Regex.IsMatch(file, @"^Port=");
// secondLineMatches == false — team expects true
```

Explain why default regex behavior fails here and what minimal change fixes it without rewriting the parser as a full state machine.

**Answer:** By default, `^` and `$` anchor only the start and end of the entire input string, not each line — so `^Port=` looks for `Port=` at position 0 of the whole blob (`"Server=…"`), never at the start of the second line after `\r\n`.

- **Mechanism:** Without `RegexOptions.Multiline`, `\r\n` is ordinary whitespace between characters; line boundaries are invisible to `^`/`$`. With `Multiline`, `^` matches after `\n` (and `\r\n` pairs) and `$` matches before `\n`.
- **Minimal fix:** pass `RegexOptions.Multiline`: `Regex.IsMatch(file, @"^Port=", RegexOptions.Multiline)` → `true`.
- **Alternative:** split lines first with `Regex.Split(file, @"\r?\n")` or `ReadLines` and test each line — clearer when you also need comment stripping or `#` handling (**Program.cs** Section 8).
- **Related gotcha:** `RegexOptions.Singleline` makes `.` span newlines — opposite concern when extracting multiline values; do not confuse Multiline (line anchors) with Singleline (dot behavior) — **Program.cs** Section 10.
- **Production note:** For large config files, line-by-line streaming avoids loading the entire file into one string match.

**Production takeaway:** Multiline is the fix for `^`/`$` per line — a common Karat mechanism question tied to Windows `\r\n` exports.

---

#### Q7. (R) An internal tool "sanitizes" HTML fragments before storing them in a knowledge base:

```csharp
public string StripTags(string html)
{
    string noTags = Regex.Replace(html, @"<.*>", string.Empty);
    return Regex.Replace(noTags, @"<script.*?>.*?</script>", string.Empty);
}
```

Review on input `"<div>Title</div><script>alert(1)</script>"`. What goes wrong with matching order, greediness, and security assumptions?

**Answer:** The greedy `<.*>` swallows from the first `<` through the last `>` in the string — removing the entire fragment including the script block in one pass — so the second replace never sees `<script>`; even with order reversed, regex is not a safe HTML sanitizer and cannot prevent attribute-based XSS.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Greedy `<.*>` spans to last `>` | Entire `"<div>…</div><script>…</script>"` becomes one match — all content deleted or mangled |
| Correctness | Script strip runs after tag strip | Script pattern never runs if greedy pass already consumed `<script>…` |
| Security | Regex-based tag removal is not HTML parsing | `<img onerror=alert(1)>`, malformed tags, and nested contexts bypass strip |
| Security | No allow-list of safe tags/attributes | "Sanitize" gives false confidence — stored XSS in knowledge base |
| Design | Two-pass replace order-dependent | Fragile refactors break redaction silently |

**Fix (priority order):**

1. Do not use regex for HTML security — use a vetted sanitizer (`HtmlSanitizer` NuGet, AngleSharp with allow-list, or store Markdown instead of raw HTML).
2. If regex is only for non-security display cleanup, use lazy quantifiers per tag: `<.*?>` — still wrong for nested `<div><div></div></div>` but matches **Program.cs** Section 12 greedy vs lazy demo.
3. Run script removal before any broad tag pass if you must stay regex-based for legacy reasons — and treat output as untrusted anyway.
4. Add integration tests with XSS payloads, not just `"<div>Title</div>"`.

```csharp
// Display-only collapse — NOT security:
string collapsed = Regex.Replace(html, @"<.*?>", string.Empty);
```

**Production takeaway:** Greedy `<.*>` is the textbook over-match bug, and Karat pairs it with "regex ≠ sanitizer" — use proper HTML parsers for user content. See **Program.cs** `DemonstratePitfalls` greedy vs lazy comparison.
