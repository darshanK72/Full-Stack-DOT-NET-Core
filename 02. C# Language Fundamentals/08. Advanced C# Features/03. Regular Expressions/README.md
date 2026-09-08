# Regular Expressions — Interview Q&A


## Table of Contents

1. [Q1. What is a regular expression and what does the Regex class provide in C#?](#q1-what-is-a-regular-expression-and-what-does-the-regex-class-provide-in-c)
2. [Q2. What are RegexOptions and which flags are most commonly needed in production?](#q2-what-are-regexoptions-and-which-flags-are-most-commonly-needed-in-production)
3. [Q3. What are capturing groups, named groups, and how do you access them?](#q3-what-are-capturing-groups-named-groups-and-how-do-you-access-them)
4. [Q4. What is catastrophic backtracking (ReDoS) and how do you prevent it?](#q4-what-is-catastrophic-backtracking-redos-and-how-do-you-prevent-it)
5. [Q5. What is [GeneratedRegex] and what are its benefits over compiled Regex?](#q5-what-is-generatedregex-and-what-are-its-benefits-over-compiled-regex)
6. [Q6. What is the difference between greedy and lazy quantifiers?](#q6-what-is-the-difference-between-greedy-and-lazy-quantifiers)
7. [Q7. How do lookahead and lookbehind assertions work?](#q7-how-do-lookahead-and-lookbehind-assertions-work)
8. [Q8. What is the difference between Regex.IsMatch static method and a static readonly Regex field?](#q8-what-is-the-difference-between-regexismatch-static-method-and-a-static-readonly-regex-field)
9. [Q9. How does RegexOptions.Multiline affect ^ and $?](#q9-how-does-regexoptionsmultiline-affect-and)
10. [Q10. How do you use Regex.Replace and when should you use a MatchEvaluator?](#q10-how-do-you-use-regexreplace-and-when-should-you-use-a-matchevaluator)
11. [Q11. Why does creating a new Regex with RegexOptions.Compiled inside a hot loop cause memory pressure?](#q11-why-does-creating-a-new-regex-with-regexoptionscompiled-inside-a-hot-loop-cause-memory-pressure)
12. [Q12. What is the ReDoS risk of the pattern (order\s+\d+)+ on a malformed input?](#q12-what-is-the-redos-risk-of-the-pattern-ordersd-on-a-malformed-input)
13. [Q13. Why does greedy matching cause HTML tag stripping to fail?](#q13-why-does-greedy-matching-cause-html-tag-stripping-to-fail)
14. [Q14. What is the Multiline / CRLF gotcha when parsing Windows-generated config files?](#q14-what-is-the-multiline-crlf-gotcha-when-parsing-windows-generated-config-files)
15. [Q15. (Code Review) A bulk-import validator creates new Regex instances per row and uses an exponentially backtracking pattern. Review and prioritize fixes.](#q15-code-review-a-bulk-import-validator-creates-new-regex-instances-per-row-and-uses-an-exponentially-backtracking-pattern-review-and-prioritize-fixes)
16. [Q16. A support portal lets agents paste a custom regex that is applied to multi-MB log files. What production risks exist and how do you harden it?](#q16-a-support-portal-lets-agents-paste-a-custom-regex-that-is-applied-to-multi-mb-log-files-what-production-risks-exist-and-how-do-you-harden-it)
17. [Q17. A notes CRM sync extracts phone numbers but area code is always empty on notes without the +1 prefix. Review the extractor.](#q17-a-notes-crm-sync-extracts-phone-numbers-but-area-code-is-always-empty-on-notes-without-the-1-prefix-review-the-extractor)

---
> **Module:** 02. C# Language Fundamentals › 08. Advanced C# Features › 03. Regular Expressions  
> **Stack:** .NET 10 · System.Text.RegularExpressions · [GeneratedRegex]

---

## Foundation Questions

---

## Q1. What is a regular expression and what does the Regex class provide in C#?

**Concepts**
- pattern-matching language
- Regex.IsMatch, Match, Matches, Replace, Split
- Match and Group objects
- RegexOptions flags
- Regex vs string.Contains for complexity trade-off

**Answer**

A regular expression is a formal pattern language that describes sets of strings. In C#, `System.Text.RegularExpressions.Regex` provides the engine. The primary operations are `IsMatch(input, pattern)` which returns a `bool`, `Match(input, pattern)` which returns the first `Match` object, `Matches(input, pattern)` which returns all `MatchCollection`, `Replace` which substitutes matches, and `Split` which divides a string at match boundaries. A `Match` carries `Value` (the matched text), `Index`, `Length`, and a `Groups` collection that provides access to capturing groups within the pattern. For simple substring checks (`string.Contains`) or prefix/suffix tests (`string.StartsWith`), Regex is the wrong tool — it adds overhead with no benefit. Regex pays off when the matching rule is a structural pattern (date formats, phone numbers, email shapes) that cannot be expressed as a simple literal comparison.

---

## Q2. What are RegexOptions and which flags are most commonly needed in production?

**Concepts**
- RegexOptions.IgnoreCase
- RegexOptions.Multiline (^ and $ per line)
- RegexOptions.Singleline (. matches newline)
- RegexOptions.Compiled (JIT compilation)
- RegexOptions.CultureInvariant
- RegexOptions.NonBacktracking (.NET 7+)

**Answer**

`RegexOptions` is a flags enum that modifies how the engine interprets the pattern and input. `IgnoreCase` makes character matching case-insensitive using the current culture unless `CultureInvariant` is also set — for API validation always combine these two to avoid culture-specific case-folding surprises (Turkish I problem). `Multiline` changes `^` and `$` to match at the start and end of each line rather than the start and end of the entire string, which is essential when scanning config files or log entries that contain embedded newlines. `Singleline` makes `.` match `\n` in addition to every other character, useful for HTML or JSON fragments that span multiple lines. `Compiled` causes the engine to JIT-compile the pattern into IL the first time the `Regex` is constructed, which speeds up subsequent matches at the cost of higher startup and memory cost — appropriate for patterns used thousands of times. `NonBacktracking` (.NET 7+) uses a linear-time NFA engine that eliminates catastrophic backtracking but does not support lookaheads or backreferences.

---

## Q3. What are capturing groups, named groups, and how do you access them?

**Concepts**
- parentheses define a group
- match.Groups[n] for positional groups
- (?<name>…) for named groups
- match.Groups["name"] access
- non-capturing group (?:…)

**Answer**

Parentheses in a regex pattern create a capturing group that captures the substring matched by the sub-pattern inside them. Groups are indexed starting at 1 (Group 0 is always the entire match). In C#, `match.Groups[2].Value` retrieves the second captured group. Named groups use the syntax `(?<areaCode>[0-9]{3})` and are accessed as `match.Groups["areaCode"].Value`, which is far more readable and resilient to reordering captures in the pattern. Non-capturing groups `(?:…)` group sub-expressions for alternation or quantifier scope without creating a group entry, which saves a small amount of memory and avoids renumbering when inserted. For patterns that extract multiple fields — phone number components, date parts, URL segments — named groups are the correct choice because they make the extraction code self-documenting and survive pattern edits that add or remove other groups.

---

## Q4. What is catastrophic backtracking (ReDoS) and how do you prevent it?

**Concepts**
- exponential backtracking on ambiguous quantifiers
- ReDoS denial-of-service attack
- MatchTimeout parameter
- RegexOptions.NonBacktracking (.NET 7+)
- pattern anchoring to reduce search space

**Answer**

Catastrophic backtracking occurs when a pattern contains nested or overlapping quantifiers that cause the engine to explore an exponential number of paths before concluding there is no match. The classic example is `(a+)+$` against a long string of `a`s followed by a non-matching character — the engine tries every possible split of the `a`s between the inner and outer groups. An attacker can supply crafted input to a web endpoint that triggers this behavior, causing the regex thread to consume 100% CPU for seconds or minutes (ReDoS — Regex Denial of Service). The primary mitigations are: always pass a `TimeSpan matchTimeout` to the `Regex` constructor or to `Regex.IsMatch` static calls to bound execution time; use `RegexOptions.NonBacktracking` for validation patterns, which runs in O(n) time regardless of input but does not support lookaheads or backreferences; anchor patterns with `^` and `$` to reduce the search space; and rewrite ambiguous patterns to eliminate overlap (e.g., possessive quantifiers via atomic groups `(?>…)` in .NET 5+).

---

## Q5. What is [GeneratedRegex] and what are its benefits over compiled Regex?

**Concepts**
- Roslyn source generator at compile time
- zero startup overhead
- Native AOT and trimming compatible
- partial method requirement
- .NET 7+ availability

**Answer**

`[GeneratedRegex("pattern", RegexOptions.IgnoreCase)]` on a `partial static Regex` method instructs the Roslyn source generator to emit C# code at compile time that implements the matching logic directly — no runtime pattern compilation, no reflection, and no JIT warm-up. The generated code is plain C# that the JIT compiles along with the rest of the application, making it fully compatible with Native AOT and the IL trimmer. At runtime, the method returns a pre-built `Regex` instance with the behavior of `RegexOptions.Compiled` but without the startup cost. The requirement is that the containing class must be `partial` and the method signature must be `private static partial Regex MethodName()`. For any new .NET 10 code that uses a statically known pattern, `[GeneratedRegex]` is the correct default: it is faster at startup, smaller in memory, and trimming-safe. Use a `static readonly Regex` field with `RegexOptions.Compiled` only when the pattern is built dynamically at runtime (from configuration), where `[GeneratedRegex]` is not applicable.

---

## Q6. What is the difference between greedy and lazy quantifiers?

**Concepts**
- greedy: * + match as much as possible
- lazy: *? +? match as little as possible
- impact on overlapping delimiters
- greedy consuming entire content between first and last delimiter
- lazy for HTML-tag-like patterns

**Answer**

Greedy quantifiers (`*`, `+`, `{n,m}`) match as many characters as possible while still allowing the overall pattern to succeed. Lazy quantifiers (`*?`, `+?`, `{n,m}?`) match as few characters as possible. The difference is visible when the input contains repeated delimiters: the greedy pattern `<.*>` applied to `<b>bold</b>` matches the entire string from the first `<` to the last `>`, consuming `<b>bold</b>` as one match. The lazy pattern `<.*?>` matches `<b>`, then `</b>` as two separate matches. For HTML and XML fragment parsing, lazy quantifiers are almost always intended because the goal is to match individual tags rather than the span from the first opening tag to the last closing tag. Even with lazy quantifiers, using regex to parse HTML or XML is fragile for arbitrarily nested content — a dedicated parser (HtmlAgilityPack, `XDocument`) is safer.

---

## Q7. How do lookahead and lookbehind assertions work?

**Concepts**
- positive lookahead (?=…)
- negative lookahead (?!…)
- positive lookbehind (?<=…)
- negative lookbehind (?<!…)
- zero-width assertions (no characters consumed)

**Answer**

Lookahead and lookbehind are zero-width assertions — they check whether a pattern exists (or does not exist) at the current position without consuming characters or contributing to the match. Positive lookahead `(?=foo)` succeeds only if `foo` follows the current position. Negative lookahead `(?!foo)` succeeds only if `foo` does not follow. Positive lookbehind `(?<=foo)` succeeds only if `foo` precedes the current position. Negative lookbehind `(?<!foo)` succeeds only if `foo` does not precede. A common use case is password validation: the pattern `(?=.*[A-Z])(?=.*\d).{8,}` requires at least one uppercase letter and one digit without prescribing their positions. Another use case is extracting a value that follows a specific keyword without including the keyword in the capture. In .NET, lookbehind assertions support variable-length patterns (unlike some other engines), which makes them more powerful but also more expensive when the lookbehind pattern is complex.

---

## Q8. What is the difference between Regex.IsMatch static method and a static readonly Regex field?

**Concepts**
- static method creates a new Regex internally per call
- static field amortizes construction cost
- RegexOptions.Compiled on the static field
- pattern not compiled with static method
- memory and JIT cost comparison

**Answer**

`Regex.IsMatch(input, pattern)` creates a new `Regex` instance internally on every call, applies a simple cache of recently used patterns (the static cache is limited to 15 entries by default), and runs the match. When the same pattern appears frequently under load the cached entry may be evicted by other patterns, causing repeated compilation. A `static readonly Regex` field with `RegexOptions.Compiled` constructs the regex once at class initialization and stores the JIT-compiled matching code permanently. Every subsequent call goes directly to the compiled IL matcher with no lookup overhead. The `[GeneratedRegex]` attribute is the .NET 10 preferred form — it produces compiled code at build time rather than runtime. The static field pattern is correct for legacy code or when the pattern is read from configuration: `private static readonly Regex EmailPattern = new Regex(@"…", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));`. Always include a `MatchTimeout` on the static field to bound ReDoS exposure.

---

## Q9. How does RegexOptions.Multiline affect ^ and $?

**Concepts**
- default: ^ = start of string, $ = end of string
- Multiline: ^ = start of any line, $ = end of any line
- \r\n vs \n line endings
- config file parsing use case
- combining with Singleline

**Answer**

By default, `^` matches only at the very start of the input string and `$` matches only at the very end (or just before a trailing newline). With `RegexOptions.Multiline`, `^` also matches at the position following every `\n` character and `$` matches at the position preceding every `\n`. This is the correct mode for parsing multiline text files where each line should be matched independently — for example, extracting key/value pairs from a config file by matching `^(\w+)=(.*)$` against the entire file content rather than splitting on newlines first. A subtle gotcha is Windows line endings: `\r\n` is two characters, and `$` in Multiline mode matches before `\n` but leaves the `\r` in the match — the captured value may have a trailing carriage return. The fix is to trim the captured group or include `\r?$` in the pattern. `Multiline` and `Singleline` can be combined but have independent effects: Multiline changes `^`/`$`, Singleline changes `.`.

---

## Q10. How do you use Regex.Replace and when should you use a MatchEvaluator?

**Concepts**
- Regex.Replace with replacement string
- $1 $2 group backreferences in replacement
- MatchEvaluator delegate for dynamic replacements
- Replace count limit overload
- common use: normalization, redaction

**Answer**

`Regex.Replace(input, pattern, replacement)` substitutes every match with the replacement string. The replacement string can reference captured groups with `$1`, `$2`, or `${name}` — for example, reformatting a date from `MM/DD/YYYY` to `YYYY-MM-DD` with `Replace(input, @"(\d{2})/(\d{2})/(\d{4})", "$3-$1-$2")`. When the replacement cannot be expressed as a static string with group references — for example, when you need to look up each matched value in a dictionary, apply a transformation, or conditionally replace based on the match content — you use a `MatchEvaluator` delegate: `Replace(input, pattern, match => lookup[match.Value])`. The evaluator is called once per match and returns the replacement string. An overload accepts a count to limit the number of substitutions. For production use, always apply a `MatchTimeout` to any `Regex` used in a Replace operation that processes user-supplied content, because unbounded Replace on adversarial input is a DoS vector.

---

## Gotchas — Regular Expressions (Interview Traps)

---

#### Gotcha 1. Catastrophic backtracking — nested quantifiers (`(a+)+`) cause exponential time on non-matching input

**Concepts**
- nested quantifiers create exponentially many backtracking paths
- partial match followed by overall failure triggers full path exploration
- `MatchTimeout` as mandatory guard for user-supplied patterns
- `RegexOptions.NonBacktracking` (.NET 7+) for linear-time guarantee

**Answer**

A pattern like `(a+)+` applied to a string `"aaaaaaaaab"` (many `a`s followed by a non-matching character) causes the backtracking engine to explore an exponential number of ways to partition the `a`s between the inner `+` and outer `+` before concluding the overall match fails. With 20 or more characters in the input, this can take seconds or minutes. The mandatory production mitigation is to set a `MatchTimeout` on every `Regex` that processes user-supplied or external content: `new Regex(pattern, RegexOptions.None, TimeSpan.FromMilliseconds(150))`. When the engine exceeds the timeout, it throws `RegexMatchTimeoutException` instead of hanging. For patterns that must be user-defined, `RegexOptions.NonBacktracking` guarantees linear time but cannot support backreferences or lookaheads.

---

#### Gotcha 2. `Regex` compiled flag — `RegexOptions.Compiled` reduces per-call overhead but increases startup time; use for hot paths

**Concepts**
- `RegexOptions.Compiled` generates IL via dynamic assembly on first use
- startup cost: significant for infrequently used patterns
- `[GeneratedRegex]` as the AOT-compatible compile-time alternative
- cached `static readonly Regex` required to amortize compilation cost

**Answer**

`RegexOptions.Compiled` causes the regex engine to JIT-compile the pattern into IL code when the `Regex` object is first constructed. This makes subsequent matches faster — typically 2-5x faster than interpreted mode — but the compilation itself takes tens to hundreds of milliseconds. If a compiled `Regex` is created inside a method that is called on every request, the startup cost is paid repeatedly and the pattern's dynamic assembly accumulates in memory without being garbage collected, creating a memory leak. The correct usage is `private static readonly Regex Pattern = new Regex(@"...", RegexOptions.Compiled)` — constructed once, reused forever. In .NET 7+ the `[GeneratedRegex]` source generator is preferred: it produces equivalent IL at build time with zero runtime startup cost and is fully AOT-compatible.

---

#### Gotcha 3. `Regex.IsMatch` vs `Regex.Match` — IsMatch stops at first match; Match returns full match details

**Concepts**
- `IsMatch` returns `bool`; stops at first successful match for efficiency
- `Match` returns a `Match` object with captured groups
- `Matches` returns all matches as a `MatchCollection`
- `IsMatch` is more efficient when only existence is needed

**Answer**

`Regex.IsMatch(input, pattern)` returns `true` as soon as the engine finds the first position where the pattern matches, making it the most efficient choice when you only need to know whether a match exists. `Regex.Match` returns the full `Match` object including captured groups, start position, length, and value — it also stops at the first match unless you call `match.NextMatch()`. `Regex.Matches` returns a lazy `MatchCollection` of all non-overlapping matches. A common mistake is using `Matches` and checking `.Count > 0` when `IsMatch` is both simpler and faster. Conversely, using `IsMatch` when the capture groups are needed results in having to call `Match` anyway, effectively evaluating the pattern twice.

---

#### Gotcha 4. Static `Regex.IsMatch` caches recently used patterns — not always faster than a `new Regex()`

**Concepts**
- static `Regex` methods cache the last 15 compiled patterns in an MRU cache
- cache eviction under high variety of patterns causes repeated re-compilation
- explicit `static readonly Regex` avoids eviction
- `Regex.CacheSize` property to tune the static cache size

**Answer**

The static methods `Regex.IsMatch(input, pattern)`, `Regex.Match(...)`, and `Regex.Replace(...)` maintain an internal MRU cache of up to 15 compiled patterns by default (configurable via `Regex.CacheSize`). When your code uses only a handful of patterns, the cache works well and the static form is convenient. But if the application uses more than 15 distinct patterns (or if patterns are constructed dynamically from user input), patterns are evicted from the cache and must be recompiled on next use. In a high-throughput application using 30+ patterns, the eviction churn causes measurable latency spikes. The reliable solution is to store each pattern in a `static readonly Regex` field, guaranteeing zero recompilation regardless of how many other patterns are in use.

---

#### Gotcha 5. Greedy vs lazy quantifiers — `.*` vs `.*?` — greedy matches as much as possible

**Concepts**
- greedy `.*` matches the longest possible string first
- lazy `.*?` matches the shortest possible string first
- greedy causes "over-matching" across multiple delimiters
- lazy does not guarantee correctness for nested or overlapping delimiters

**Answer**

The greedy quantifier `.*` consumes as many characters as possible before yielding back during backtracking. In a pattern like `<div>(.*)</div>` applied to `<div>A</div><div>B</div>`, the greedy `.*` matches from the first `A` all the way to `B` — it finds the last `</div>` in the string rather than the first one. Switching to `<div>(.*?)</div>` makes the quantifier lazy: it matches as few characters as possible, stopping at the first `</div>`. This is closer to the intended behavior for simple cases but still fails on nested tags. The general lesson is that greedy vs lazy controls the matching strategy but does not fix fundamental parser limitations — HTML and nested structures require a proper parser, not a regex tweak.

---

#### Gotcha 6. `^` and `$` match start/end of line in `Multiline` mode; start/end of string otherwise

**Concepts**
- default: `^` matches start of string, `$` matches end of string
- `RegexOptions.Multiline`: `^` and `$` match at each line boundary
- `\A` and `\Z` always anchor to string boundaries regardless of Multiline
- `RegexOptions.Singleline` makes `.` match newlines (unrelated to `^`/`$`)

**Answer**

Without `RegexOptions.Multiline`, `^` anchors to the start of the entire input string and `$` anchors to the end. A pattern like `^\d+$` applied to a multi-line string will only match if the entire string is digits. With `RegexOptions.Multiline`, `^` matches at the start of each line (after each `\n`) and `$` matches at the end of each line (before each `\n`), so `^\d+$` matches any line that contains only digits. The anchors `\A` and `\Z` are immune to `Multiline`: `\A` always matches the start of the string and `\Z` always matches the end (or before a final newline). Using `\A` and `\Z` instead of `^` and `$` is the explicit, mode-independent way to anchor to string boundaries.

---

#### Gotcha 7. Capturing groups vs non-capturing groups — `(...)` vs `(?:...)` — unnecessary captures waste memory

**Concepts**
- `(...)` captures the group and stores it in `Match.Groups`
- `(?:...)` groups without capturing — no allocation in `Groups` collection
- backreference requires a capturing group
- named groups `(?<name>...)` are also capturing groups

**Answer**

Every `(...)` in a pattern creates a capturing group, allocating a `Group` object in the `Match.Groups` collection for each match. In patterns with many groupings used only for precedence or alternation, these allocations are wasteful. Converting grouping-only constructs to non-capturing groups `(?:...)` reduces allocations and can improve performance in hot-path regex operations. Named groups `(?<year>\d{4})` are capturing groups with string keys — they are appropriate when the captured value is used by name in code or replacement strings. Pure structural groupings (e.g., `(?:foo|bar)+`) should always use `(?:...)` to signal that the capture value is irrelevant and to avoid unnecessary memory use.

---

#### Gotcha 8. Character class `[^...]` negation — `[^abc]` matches any char NOT a, b, or c, including newline

**Concepts**
- `[^abc]` negated class matches any character not in the set
- includes newline `\n` unless explicitly excluded
- `[^\n]` to exclude newline explicitly in multi-line input
- `.` does not match newline by default; negated classes always include `\n`

**Answer**

A negated character class `[^abc]` matches any single character that is NOT `a`, `b`, or `c` — including whitespace, punctuation, and, crucially, the newline character `\n`. This means a pattern like `[^,]+` on a CSV line will consume the newline at the end of the line, potentially merging data from the next line into the current field match. To explicitly exclude newline from a negated class, add `\n` to the exclusion: `[^,\n]+`. The `.` metacharacter does not match newline by default (it only does with `RegexOptions.Singleline`), but negated character classes are not affected by `Singleline` — they always include `\n` unless it is explicitly excluded.

---

#### Gotcha 9. `Regex.Replace` with a match evaluator delegate — used for dynamic replacement logic

**Concepts**
- `MatchEvaluator` delegate called once per match
- enables lookup tables, transformations, and conditional replacements
- return value replaces the entire matched substring
- `MatchTimeout` applies to the matching phase, not the evaluator execution

**Answer**

`Regex.Replace(input, pattern, match => transform(match.Value))` calls the evaluator delegate once for every match found. The delegate receives the full `Match` object — including all captured groups — and returns the replacement string. This is the idiomatic way to implement dynamic replacements that depend on the matched content, such as looking up a localized string for each matched code, applying a case transformation, or encoding special characters. The delegate itself is synchronous; for async lookups (database or cache), the pattern must pre-load the lookup table before calling `Replace`. The `MatchTimeout` set on the `Regex` governs the matching phase; the evaluator execution time is separate and is not covered by the timeout, so keep evaluator logic fast.

---

#### Gotcha 10. .NET `Regex` source generators (`[GeneratedRegex]`) in .NET 7+ — compile-time compiled patterns

**Concepts**
- `[GeneratedRegex(@"pattern", RegexOptions.X)]` on a `static partial Regex` method
- compiler generates a `Regex` subclass with the state machine baked in at build time
- zero runtime startup cost; no dynamic assembly; fully AOT-compatible
- requires `partial` class and `partial` method declaration

**Answer**

The `[GeneratedRegex]` attribute, introduced in .NET 7, instructs the Roslyn source generator to produce a `Regex`-derived class with the compiled state machine emitted as plain C# source code during build. The resulting pattern has zero runtime startup overhead and no dependence on dynamic assembly generation, making it fully compatible with Native AOT publishing. Usage requires a `static partial` method returning `Regex` inside a `partial` class. The generator produces an implementation that behaves identically to `RegexOptions.Compiled` but without the runtime cost. For any pattern used in a production application, `[GeneratedRegex]` is the preferred form over `new Regex(...)` or static method invocations.

---

## Real-World Scenarios

---

## Q15. (Code Review) A bulk-import validator creates new Regex instances per row and uses an exponentially backtracking pattern. Review and prioritize fixes.

```csharp
public sealed class ImportRowValidator
{
    public bool IsValid(string email, string phone, string notes)
    {
        const string emailPat = @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}";
        const string phonePat = @"^(\+1[-.\s]?)?(\([0-9]{3}\)|[0-9]{3})[-.\s]?[0-9]{3}[-.\s]?[0-9]{4}$";
        if (!Regex.IsMatch(email, emailPat)) return false;
        if (!Regex.IsMatch(phone, phonePat)) return false;
        var orderMatch = Regex.Match(notes, @"(order\s+\d+)+");
        return orderMatch.Success;
    }
}
```

**Concepts**
- Regex created on every call (static method with uncompiled pattern)
- (order\s+\d+)+ nested quantifier ReDoS risk
- missing anchors on email pattern
- missing MatchTimeout
- static readonly or [GeneratedRegex] as fix

| Category | Problem | Impact |
|---|---|---|
| Performance | `Regex.IsMatch` with inline pattern recreates Regex every call at 1k+ RPS | High CPU from repeated pattern compilation |
| Security | `(order\s+\d+)+` nested quantifier causes exponential backtracking on malformed notes | DoS — thread hangs on crafted input |
| Correctness | Email pattern lacks `^` and `$` anchors — `@` inside a longer string matches | False positives in validation |
| Safety | No `MatchTimeout` on any pattern | Unbounded execution time |

**Fix priority:**
1. Move all patterns to `[GeneratedRegex]` static partial methods (compile-time, zero runtime overhead, timeout not needed with NonBacktracking).
2. Rewrite `(order\s+\d+)+` as `order\s+\d+` and iterate with `Matches` — avoids nested quantifier.
3. Add `^` and `$` anchors to the email pattern.
4. If staying with `static readonly Regex`, pass `TimeSpan.FromMilliseconds(100)` as timeout.

---

## Q16. A support portal lets agents paste a custom regex that is applied to multi-MB log files. What production risks exist and how do you harden it?

```csharp
public string RedactMatches(string logContent, string userPattern)
{
    var regex = new Regex(
        userPattern,
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    return regex.Replace(logContent, "[REDACTED]");
}
```

**Concepts**
- ReDoS via user-supplied pattern
- Compiled on dynamically constructed Regex
- memory leak from abandoned dynamic assemblies
- input length limits
- pattern allowlist or validator

**Answer**

This code has three independent production risks. First, a user-supplied pattern can contain nested quantifiers designed to trigger catastrophic backtracking against large log files, pinning a CPU core and causing request timeouts or cascading failures — this is a classic ReDoS attack vector. Second, `RegexOptions.Compiled` creates a new dynamic assembly for every distinct user pattern; because agents can supply arbitrary patterns, this generates unbounded numbers of abandoned dynamic assemblies and causes a memory leak that grows until the process is recycled. Third, there is no depth or complexity limit on the user pattern — a pattern that matches every character in a 10 MB log file will produce a match per character, generating a 10 MB array of Match objects. The fixes in priority order are: remove `RegexOptions.Compiled` for dynamic patterns; add `MatchTimeout = TimeSpan.FromSeconds(2)` to bound execution; limit log file size accepted per request; run the regex in a separate process or Task with a cancellation token so a runaway match can be killed without affecting the main worker; and consider validating the pattern against a restricted grammar (e.g., disallow `(…)+` and `(.*)` patterns) before accepting it.

---

## Q17. A notes CRM sync extracts phone numbers but area code is always empty on notes without the +1 prefix. Review the extractor.

```csharp
public sealed class PhoneExtractor
{
    private static readonly Regex PhonePattern = new(
        @"(\+1[-.\s]?)?(\([0-9]{3}\)|[0-9]{3})[-.\s]?[0-9]{3}[-.\s]?[0-9]{4}",
        RegexOptions.Compiled);

    public string GetAreaCode(string notes)
    {
        Match m = PhonePattern.Match(notes);
        return m.Groups[2].Value;
    }

    public bool HasUsPhone(string notes) =>
        Regex.IsMatch(notes, PhonePattern.ToString());
}
```

**Concepts**
- Groups[2] index shifts when optional group absent
- named groups as fix
- HasUsPhone recreating Regex from ToString()
- empty string vs null for no-match
- missing null/empty input guard

| Category | Problem | Impact |
|---|---|---|
| Correctness | `Groups[2]` is the area code only when Group 1 (`+1`) is present; when `+1` is absent Group 1 still participates as an empty optional group, so area code is consistently in Group 2 — but this is fragile if the pattern changes | Subtle indexing fragility |
| Performance | `HasUsPhone` calls `PhonePattern.ToString()` which returns the raw pattern string, then `Regex.IsMatch` creates a new Regex from that string on every call — bypasses the compiled instance entirely | Full recompile per call |
| Correctness | No null or empty input check before `Match` | NullReferenceException on null input |

**Fix priority:**
1. Convert Group 2 to a named group: `(?<areaCode>\([0-9]{3}\)|[0-9]{3})` and access `m.Groups["areaCode"].Value` — robust to pattern changes.
2. Fix `HasUsPhone` to call `PhonePattern.IsMatch(notes)` (instance method on the cached compiled Regex) instead of creating a new one.
3. Guard against null/empty `notes` before calling `Match`.
