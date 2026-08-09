/*
 * =============================================================================
 * 03. REGULAR EXPRESSIONS — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: System.Text.RegularExpressions — the Regex engine, Match and
 *        MatchCollection, capturing groups, Replace, Split, RegexOptions,
 *        compiled patterns, common validation patterns, and production pitfalls.
 *
 * WHY IT MATTERS:
 *   User input, log files, and CSV imports arrive as messy text. You need to
 *   know whether an email field looks valid, pull every order id from a blob,
 *   redact phone numbers, or split on mixed delimiters. Regex gives you a
 *   declarative pattern language instead of hand-rolled IndexOf loops. The same
 *   ideas appear in [RegularExpression] validation attributes, grep tools, and
 *   config filters across .NET applications.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Regex class — constructor, static helpers, verbatim pattern strings
 *   2.  IsMatch — quick boolean checks; anchored vs partial matching
 *   3.  Match — Success, Value, Index, Length, NextMatch
 *   4.  MatchCollection — Matches and enumeration
 *   5.  Groups — numbered, named, non-capturing, and repeated Captures
 *   6.  Replace — literal replacement, $1 back-references, MatchEvaluator
 *   7.  Split — tokenizing on flexible delimiters
 *   8.  RegexOptions — IgnoreCase, Multiline, Singleline, Compiled, and more
 *   9.  Common patterns — email, phone, product codes, dates, word boundaries
 *  10.  Performance — static Regex instances, Compiled, MatchTimeout
 *  11.  Pitfalls — false positives, ReDoS, Groups without Success
 *
 * =============================================================================
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace RegularExpressions;

/*
 * =========================================================================
 * SECTION 1: SCENARIO — CUSTOMER IMPORT BATCH
 * =========================================================================
 *
 * A support export mixes valid rows, typos, blank fields, and free-text notes.
 * Each section below uses the same data to demonstrate a different Regex API.
 * -------------------------------------------------------------------------
 */
public sealed class ImportRecord
{
    public ImportRecord(string rowId, string email, string phone, string productCode, string notes)
    {
        RowId = rowId;
        Email = email;
        Phone = phone;
        ProductCode = productCode;
        Notes = notes;
    }

    public string RowId { get; }
    public string Email { get; }
    public string Phone { get; }
    public string ProductCode { get; }
    public string Notes { get; }
}

public class Program
{
    /*
     * =========================================================================
     * SECTION 2: CREATING Regex — CONSTRUCTOR VS STATIC HELPERS
     * =========================================================================
     *
     * Two ways to run a pattern:
     *
     *   new Regex(pattern, options)   parse once; reuse on many inputs
     *   Regex.IsMatch(input, pattern) parse pattern on EVERY static call
     *
     * Pattern strings use metacharacters (. * + ? [] {} () | ^ $ \b \d …).
     * Prefix with @ in C# so backslashes are readable: @"\d+" not "\\d+".
     *
     * Regex.Escape(literal) wraps user text so special characters match literally.
     *
     * Store hot-path patterns in static readonly fields (especially with Compiled).
     * -------------------------------------------------------------------------
     */
    private static readonly Regex EmailPattern = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex PhonePattern = new(
        @"^(\+1[-.\s]?)?(\([0-9]{3}\)|[0-9]{3})[-.\s]?[0-9]{3}[-.\s]?[0-9]{4}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex ProductCodePattern = new(
        @"^[A-Z]{3}-\d{4}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex OrderIdInTextPattern = new(
        @"order\s+(?<id>\d+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex PhoneRedactPattern = new(
        @"\d{3}-\d{3}-\d{4}",
        RegexOptions.Compiled);

    public static void Main(string[] args)
    {
        List<ImportRecord> batch = CreateSampleBatch();

        Console.WriteLine("=== Customer import batch (raw) ===");
        PrintBatch(batch);

        DemonstrateRegexEscape();
        DemonstrateIsMatch(batch);
        DemonstrateMatch(batch);
        DemonstrateMatchCollection();
        DemonstrateGroups(batch);
        DemonstrateReplace(batch);
        DemonstrateSplit();
        DemonstrateCommonPatterns(batch);
        DemonstrateRegexOptions();
        DemonstratePerformance();
        DemonstratePitfalls();
        PrintValidationReport(batch);
    }

    private static List<ImportRecord> CreateSampleBatch()
    {
        return
        [
            new ImportRecord("R-1001", "alice@contoso.com", "+1 (555) 123-4567", "ABX-9001", "Contact alice@contoso.com for billing."),
            new ImportRecord("R-1002", "bob@example", "555.987.6543", "ABX-9001", "Ship to Bob ASAP."),
            new ImportRecord("R-1003", "carol@test.co.uk", "07946 0958", "ZZZ-0042", "Refund order 42 today."),
            new ImportRecord("R-1004", "", "12345", "BAD-CODE", "No email on file."),
            new ImportRecord("R-1005", "dave@mail.org", "not-a-phone", "ABX-9001", "Call Dave at 555-111-2222."),
        ];
    }

    /*
     * --- 2a. Regex.Escape — treat user input as literal text ---
     */
    private static void DemonstrateRegexEscape()
    {
        string userSearch = "C++";
        string escaped = Regex.Escape(userSearch); // turns + into \+ so it is not "one or more"
        bool found = Regex.IsMatch("Learn C++ today", escaped);

        Console.WriteLine();
        Console.WriteLine("--- Regex.Escape (literal user text) ---");
        Console.WriteLine($"  Search for \"{userSearch}\" → escaped pattern \"{escaped}\" → found={found}");
    }

    /*
     * =========================================================================
     * SECTION 3: IsMatch — BOOLEAN GATE
     * =========================================================================
     *
     *   regex.IsMatch(input)              instance — pattern already parsed
     *   Regex.IsMatch(input, pattern)     static — pattern parsed each call
     *
     * Default behavior: partial match ANYWHERE in the string unless you anchor:
     *
     *   ^     start of input (or start of line with Multiline)
     *   $     end of input (or end of line with Multiline)
     *
     * Pitfall: IsMatch("junk yes@x.co more", emailPattern) can return true without
     * ^ and $ because a substring looks like an email. Always anchor whole-field
     * validation.
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateIsMatch(List<ImportRecord> batch)
    {
        string anchoredSample = batch[0].Email;
        bool wholeFieldOk = EmailPattern.IsMatch(anchoredSample);

        string junkWrapped = "prefix bob@example suffix";
        bool partialWithoutAnchor = Regex.IsMatch(junkWrapped, @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}");
        bool wholeFieldWithAnchor = Regex.IsMatch(junkWrapped, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");

        Console.WriteLine();
        Console.WriteLine("--- Regex.IsMatch ---");
        Console.WriteLine($"  Whole field \"{anchoredSample}\" → {wholeFieldOk}");
        Console.WriteLine($"  Junk-wrapped email without ^$ → {partialWithoutAnchor} (substring match!)");
        Console.WriteLine($"  Same input with ^$ anchors     → {wholeFieldWithAnchor}");
    }

    /*
     * =========================================================================
     * SECTION 4: Match — FIRST HIT IN DETAIL
     * =========================================================================
     *
     *   regex.Match(input)        first Match (Success may be false)
     *   Regex.Match(input, pat)   static shortcut
     *
     * Match members when Success is true:
     *
     *   .Value      matched substring
     *   .Index      zero-based start in input
     *   .Length     length of matched substring
     *   .Groups     capture groups (see SECTION 6)
     *   .NextMatch() scan for the next match after this one
     *
     * Always check Success before reading Value or Groups[1].
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateMatch(List<ImportRecord> batch)
    {
        string notes = batch[4].Notes;
        Match firstPhone = Regex.Match(notes, @"\d{3}-\d{3}-\d{4}");

        Console.WriteLine();
        Console.WriteLine("--- Regex.Match (first phone in notes) ---");
        Console.WriteLine($"  Input: \"{notes}\"");
        if (firstPhone.Success)
        {
            Console.WriteLine($"  Value=\"{firstPhone.Value}\" Index={firstPhone.Index} Length={firstPhone.Length}");
        }

        string multiToken = "token A1 token B2 token C3";
        Match walker = Regex.Match(multiToken, @"token\s+(\w+)");
        Console.WriteLine();
        Console.WriteLine("--- Match.NextMatch (walk all hits manually) ---");
        while (walker.Success)
        {
            Console.WriteLine($"  \"{walker.Value}\" capture=\"{walker.Groups[1].Value}\" at index {walker.Index}");
            walker = walker.NextMatch(); // advance to next match in same input
        }
    }

    /*
     * =========================================================================
     * SECTION 5: MatchCollection — EVERY MATCH
     * =========================================================================
     *
     *   regex.Matches(input)           all matches
     *   Regex.Matches(input, pattern)  static shortcut
     *
     * Returns MatchCollection — lazy until enumerated. Supports .Count and foreach.
     * Prefer Matches when you need every hit; Match + NextMatch when you stream.
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateMatchCollection()
    {
        string logLine = "order 42 shipped; ticket 42 closed; order 99 pending";
        MatchCollection orderHits = OrderIdInTextPattern.Matches(logLine);

        Console.WriteLine();
        Console.WriteLine("--- Regex.Matches / MatchCollection ---");
        Console.WriteLine($"  Input: \"{logLine}\"");
        Console.WriteLine($"  Count: {orderHits.Count}");
        foreach (Match hit in orderHits)
        {
            Console.WriteLine($"    full=\"{hit.Value}\" id=\"{hit.Groups["id"].Value}\" at index {hit.Index}");
        }
    }

    /*
     * =========================================================================
     * SECTION 6: Groups — CAPTURES
     * =========================================================================
     *
     * Parentheses create capturing groups:
     *
     *   match.Groups[0]         entire match (same as .Value)
     *   match.Groups[1]         first (...) group
     *   match.Groups["name"]    named group from (?<name>...) or (?'name'...)
     *
     * (?:sub)   non-capturing — groups without adding a numbered capture
     *
     * When a quantifier repeats a group inside one Match, Group.Captures
     * holds every repetition; Groups[n].Value is the last capture only.
     *
     * --- 6a. Numbered groups ---
     * --- 6b. Named groups ---
     * --- 6c. Non-capturing vs capturing ---
     * --- 6d. Captures collection on repeated groups ---
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateGroups(List<ImportRecord> batch)
    {
        string phoneSample = batch[0].Phone;
        Match phoneParts = PhonePattern.Match(phoneSample);

        Console.WriteLine();
        Console.WriteLine("--- Groups: numbered (phone pattern) ---");
        Console.WriteLine($"  Input: \"{phoneSample}\"");
        Console.WriteLine($"  [0] full match : \"{phoneParts.Groups[0].Value}\"");
        Console.WriteLine($"  [1] country    : \"{phoneParts.Groups[1].Value}\"");
        Console.WriteLine($"  [2] area block : \"{phoneParts.Groups[2].Value}\"");

        string noteText = batch[0].Notes;
        Match emailParts = Regex.Match(
            noteText,
            @"(?<user>[a-zA-Z0-9._%+-]+)@(?<domain>[a-zA-Z0-9.-]+\.[a-zA-Z]{2,})");

        Console.WriteLine();
        Console.WriteLine("--- Groups: named (email in notes) ---");
        Console.WriteLine($"  user   = \"{emailParts.Groups["user"].Value}\"");
        Console.WriteLine($"  domain = \"{emailParts.Groups["domain"].Value}\"");

        Match withCapture = Regex.Match("ABC-123", @"([A-Z]+)-(\d+)");
        Match withoutCapture = Regex.Match("ABC-123", @"(?:[A-Z]+)-(\d+)"); // (?:…) skips a numbered slot for letters

        Console.WriteLine();
        Console.WriteLine("--- Groups: non-capturing (?:…) ---");
        Console.WriteLine($"  With capture    → Groups.Count={withCapture.Groups.Count}, [1]=\"{withCapture.Groups[1].Value}\", [2]=\"{withCapture.Groups[2].Value}\"");
        Console.WriteLine($"  Non-capturing   → Groups.Count={withoutCapture.Groups.Count}, [1]=\"{withoutCapture.Groups[1].Value}\" (digits only)");

        string repeated = "one two three four";
        Match repeatedInOnePass = Regex.Match(repeated, @"((\w+)\s)+"); // outer group repeats inside one Match
        Group outerRepeatingGroup = repeatedInOnePass.Groups[1];

        Console.WriteLine();
        Console.WriteLine("--- Group.Captures (group repeated in one Match) ---");
        Console.WriteLine($"  Input: \"{repeated}\"");
        Console.WriteLine($"  Groups[1].Value (last repetition) = \"{outerRepeatingGroup.Value.Trim()}\"");
        Console.Write("  Captures (every repetition): ");
        foreach (Capture capture in outerRepeatingGroup.Captures)
        {
            Console.Write($"\"{capture.Value.Trim()}\" ");
        }
        Console.WriteLine();
    }

    /*
     * =========================================================================
     * SECTION 7: Replace — TRANSFORM MATCHED TEXT
     * =========================================================================
     *
     *   regex.Replace(input, replacement)
     *   Regex.Replace(input, pattern, replacement)
     *
     * Replacement string tokens:
     *
     *   $0 or $&     entire match
     *   $1, $2 …     numbered groups
     *   ${name}      named group
     *   $$           literal $
     *
     * MatchEvaluator delegate — compute replacement per match:
     *
     *   regex.Replace(input, m => "..." + m.Groups[1].Value)
     *
     * --- 7a. Simple literal replacement ---
     * --- 7b. Group back-references in replacement ---
     * --- 7c. MatchEvaluator ---
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateReplace(List<ImportRecord> batch)
    {
        string noisy = "  hello   world  ";
        string collapsed = Regex.Replace(noisy, @"\s+", " "); // \s+ → single space

        string isoDate = "Ship on 2026-08-09 before noon.";
        string usDate = Regex.Replace(
            isoDate,
            @"(?<year>\d{4})-(?<month>\d{2})-(?<day>\d{2})",
            "${month}/${day}/${year}"); // reorder via named groups

        string notesWithPhone = batch[4].Notes;
        string redacted = PhoneRedactPattern.Replace(notesWithPhone, "[REDACTED]");

        string titleCase = Regex.Replace(
            "contact ALICE@CONTOSO.COM today",
            @"(?<user>[a-zA-Z0-9._%+-]+)@(?<domain>[a-zA-Z0-9.-]+)",
            m => $"{m.Groups["user"].Value.ToLowerInvariant()}@{m.Groups["domain"].Value.ToLowerInvariant()}",
            RegexOptions.None);

        Console.WriteLine();
        Console.WriteLine("--- Regex.Replace ---");
        Console.WriteLine($"  Collapse whitespace: \"{noisy.Trim()}\" → \"{collapsed.Trim()}\"");
        Console.WriteLine($"  ISO → US date: \"{isoDate}\" → \"{usDate}\"");
        Console.WriteLine($"  Redact phone: \"{notesWithPhone}\" → \"{redacted}\"");
        Console.WriteLine($"  MatchEvaluator lowercasing: → \"{titleCase}\"");
    }

    /*
     * =========================================================================
     * SECTION 8: Split — TOKENIZE ON PATTERNS
     * =========================================================================
     *
     *   regex.Split(input)
     *   Regex.Split(input, pattern)
     *
     * Splits on every match of the pattern (not on a literal string like
     * string.Split). Optional count limits segments; RegexOptions affect
     * what the pattern can match (e.g. Multiline for line-based splits).
     *
     * Empty entries may appear when delimiters are adjacent — filter if needed.
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateSplit()
    {
        string csvLike = "alice@contoso.com; bob@example.com, carol@test.co.uk | dave@mail.org";
        string[] emails = Regex.Split(csvLike, @"[;,|\s]+"); // one-or-more delimiters

        string multiline = "line-one\r\nline-two\r\nline-three";
        string[] lines = Regex.Split(multiline, @"\r?\n");

        Console.WriteLine();
        Console.WriteLine("--- Regex.Split ---");
        Console.WriteLine($"  Input: \"{csvLike}\"");
        Console.WriteLine($"  Tokens ({emails.Length}): {string.Join(" | ", emails)}");
        Console.WriteLine($"  Lines from multiline text: {string.Join(" / ", lines)}");
    }

    /*
     * =========================================================================
     * SECTION 9: COMMON PATTERNS — PRACTICAL CHEAT SHEET
     * =========================================================================
     *
     *   Metacharacter   Meaning
     *   --------------  -------------------------------------------------------
     *   ^  $            start / end (whole-field validation)
     *   .               any char except newline (Singleline: includes \n)
     *   \d \D           digit / non-digit
     *   \w \W           word char [A-Za-z0-9_] / non-word
     *   \s \S           whitespace / non-whitespace
     *   \b              word boundary (position, not a character)
     *   + * ?           one-or-more / zero-or-more / zero-or-one (greedy)
     *   +? *? ??        lazy (non-greedy) versions
     *   {n} {n,m}       exact / bounded repetition
     *   [...] [^...]    character class / negated class
     *
     * --- 9a. Email (practical, not RFC-exhaustive) ---
     * --- 9b. US phone with optional +1 ---
     * --- 9c. Product code ABC-1234 ---
     * --- 9d. ISO date yyyy-MM-dd ---
     * --- 9e. Word boundaries ---
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateCommonPatterns(List<ImportRecord> batch)
    {
        Console.WriteLine();
        Console.WriteLine("--- Pattern: email ---");
        foreach (ImportRecord row in batch)
        {
            bool ok = EmailPattern.IsMatch(row.Email);
            Console.WriteLine($"  {row.RowId,-8} \"{row.Email}\" → {(ok ? "valid" : "invalid")}");
        }

        Console.WriteLine();
        Console.WriteLine("--- Pattern: phone (US-style) ---");
        foreach (ImportRecord row in batch)
        {
            bool ok = PhonePattern.IsMatch(row.Phone);
            Console.WriteLine($"  {row.RowId,-8} \"{row.Phone}\" → {(ok ? "valid" : "invalid")}");
        }

        Console.WriteLine();
        Console.WriteLine("--- Pattern: product code [A-Z]{3}-\\d{4} ---");
        foreach (ImportRecord row in batch)
        {
            bool ok = ProductCodePattern.IsMatch(row.ProductCode);
            Console.WriteLine($"  {row.RowId,-8} \"{row.ProductCode}\" → {(ok ? "valid" : "invalid")}");
        }

        string report = "Events on 2026-01-15 and 2026-08-09 recorded.";
        MatchCollection dates = Regex.Matches(report, @"\b\d{4}-\d{2}-\d{2}\b");
        Console.WriteLine();
        Console.WriteLine("--- Pattern: ISO date ---");
        Console.WriteLine($"  Input: \"{report}\"");
        foreach (Match dateHit in dates)
        {
            Console.WriteLine($"    \"{dateHit.Value}\" at index {dateHit.Index}");
        }

        string sentence = "Refund order 42 today; category 42extra ignored.";
        MatchCollection wholeWord42 = Regex.Matches(sentence, @"\b42\b");
        Console.WriteLine();
        Console.WriteLine("--- Pattern: word boundary \\b42\\b ---");
        Console.WriteLine($"  Input: \"{sentence}\"");
        Console.WriteLine($"  Standalone \"42\" matches: {wholeWord42.Count}");
        foreach (Match wordHit in wholeWord42)
        {
            Console.WriteLine($"    \"{wordHit.Value}\" at index {wordHit.Index}");
        }
    }

    /*
     * =========================================================================
     * SECTION 10: RegexOptions — COMBINE WITH |
     * =========================================================================
     *
     *   IgnoreCase          case-insensitive letters
     *   Multiline           ^ and $ match line boundaries (\n-separated)
     *   Singleline          . matches newline
     *   Compiled            emit IL — faster repeat use, higher startup cost
     *   CultureInvariant    matching rules independent of thread culture
     *   NonBacktracking     (.NET 7+) linear-time engine — safer for untrusted patterns
     *   RightToLeft         search from end to start (rare)
     *
     * Pass to Regex constructor or static overloads:
     *
     *   new Regex(pat, RegexOptions.IgnoreCase | RegexOptions.Compiled)
     *   Regex.IsMatch(input, pat, RegexOptions.IgnoreCase)
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateRegexOptions()
    {
        string mixedCase = "Order ORDER order";
        bool caseSensitive = Regex.IsMatch(mixedCase, @"^order$");
        bool ignoreCase = Regex.IsMatch(mixedCase, @"^order$", RegexOptions.IgnoreCase);

        string multilineText = "first line\norder 99\nlast line";
        bool defaultAnchor = Regex.IsMatch(multilineText, @"^order");       // only start of whole string
        bool multilineAnchor = Regex.IsMatch(multilineText, @"^order", RegexOptions.Multiline); // start of any line

        string dotted = "start\nmiddle\nend";
        Match defaultDot = Regex.Match(dotted, @".+"); // stops at newline
        Match singlelineDot = Regex.Match(dotted, @".+", RegexOptions.Singleline); // spans lines

        Console.WriteLine();
        Console.WriteLine("--- RegexOptions ---");
        Console.WriteLine($"  IgnoreCase: \"{mixedCase}\" ^order$ → sensitive={caseSensitive}, ignoreCase={ignoreCase}");
        Console.WriteLine($"  Multiline: ^order in 3-line text → default={defaultAnchor}, multiline={multilineAnchor}");
        Console.WriteLine($"  Singleline: .+ match → default=\"{defaultDot.Value}\", singleline=\"{singlelineDot.Value.Replace("\n", "\\n")}\"");
        Console.WriteLine("  Compiled + CultureInvariant: used on EmailPattern / PhonePattern static fields.");
        Console.WriteLine("  NonBacktracking: use for untrusted user-supplied patterns to reduce ReDoS risk (.NET 7+).");
    }

    /*
     * =========================================================================
     * SECTION 11: PERFORMANCE — REUSE AND TIMEOUTS
     * =========================================================================
     *
     * Parse cost: every new Regex(pattern) or static Regex.IsMatch(input, pat)
     * rebuilds the automaton unless you cache the instance.
     *
     * RegexOptions.Compiled trades startup memory for faster hot-loop matching.
     *
     * MatchTimeout (constructor or static overload with TimeSpan) aborts
     * pathological backtracking — throws RegexMatchTimeoutException.
     *
     * PREVIEW — source-generated regex:
     *   [GeneratedRegex(@"pattern")] partial class + static partial method
     *   COVERED IN DETAIL LATER → 05. C# 7 Features / modern C# source generators
     * -------------------------------------------------------------------------
     */
    private static void DemonstratePerformance()
    {
        const string timingInput = "alice@contoso.com";
        const int iterations = 100_000;
        string inlinePattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

        Stopwatch compiledWatch = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            _ = EmailPattern.IsMatch(timingInput);
        }
        compiledWatch.Stop();

        Stopwatch inlineWatch = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            _ = Regex.IsMatch(timingInput, inlinePattern);
        }
        inlineWatch.Stop();

        Regex timedRegex = new(@"^(a+)+$", RegexOptions.None, TimeSpan.FromMilliseconds(100));
        bool timedOut = false;
        try
        {
            _ = timedRegex.IsMatch(new string('a', 30) + "X"); // classic nested-quantifier stress input
        }
        catch (RegexMatchTimeoutException)
        {
            timedOut = true;
        }

        Console.WriteLine();
        Console.WriteLine("--- Performance ---");
        Console.WriteLine($"  {iterations:N0} IsMatch on \"{timingInput}\"");
        Console.WriteLine($"  static Compiled EmailPattern : {compiledWatch.ElapsedMilliseconds} ms");
        Console.WriteLine($"  Regex.IsMatch (parse each call): {inlineWatch.ElapsedMilliseconds} ms");
        Console.WriteLine($"  MatchTimeout on pathological pattern → timed out={timedOut}");
        Console.WriteLine("  Takeaway: cache Regex instances on hot validation paths.");
    }

    /*
     * =========================================================================
     * SECTION 12: PITFALLS — PRODUCTION CHECKLIST
     * =========================================================================
     *
     *  Mistake                              | Result
     *  -------------------------------------|----------------------------------
     *  No ^/$ on form fields                | substring false positives
     *  Regex.IsMatch in tight loop          | re-parses pattern every call
     *  Read Groups[1] without Success       | empty strings, silent bugs
     *  Nested greedy quantifiers            | catastrophic backtracking (ReDoS)
     *  Assuming regex validates semantics   | syntax-only; still need business rules
     *
     * Greedy vs lazy: * + are greedy (match as much as possible); add ? for lazy.
     * -------------------------------------------------------------------------
     */
    private static void DemonstratePitfalls()
    {
        string field = "!!! alice@contoso.com ???";
        bool substringPass = Regex.IsMatch(field, @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}");
        bool anchoredFail = EmailPattern.IsMatch(field);

        Match failed = Regex.Match("no digits here", @"(\d+)");
        string unsafeGroup = failed.Success ? failed.Groups[1].Value : "(checked Success first)";

        string htmlSnippet = "<div>Title</div>";
        Match greedy = Regex.Match(htmlSnippet, @"<.*>");   // eats from first < to last >
        Match lazy = Regex.Match(htmlSnippet, @"<.*?>");    // stops at first >

        Console.WriteLine();
        Console.WriteLine("--- Pitfalls ---");
        Console.WriteLine($"  Substring email in junk → unanchored={substringPass}, anchored={anchoredFail}");
        Console.WriteLine($"  Groups[1] without match → \"{unsafeGroup}\"");
        Console.WriteLine($"  Greedy <.*>  → \"{greedy.Value}\"");
        Console.WriteLine($"  Lazy   <.*?> → \"{lazy.Value}\"");
    }

    /*
     * =========================================================================
     * SECTION 13: BATCH VALIDATION — PUTTING IT TOGETHER
     * =========================================================================
     *
     * Combine cached Regex instances into one pass per import row.
     * -------------------------------------------------------------------------
     */
    private static void PrintValidationReport(List<ImportRecord> batch)
    {
        Console.WriteLine();
        Console.WriteLine("=== Batch validation report ===");
        int accepted = 0;
        foreach (ImportRecord row in batch)
        {
            bool emailOk = EmailPattern.IsMatch(row.Email);
            bool phoneOk = PhonePattern.IsMatch(row.Phone);
            bool codeOk = ProductCodePattern.IsMatch(row.ProductCode);
            bool rowAccepted = emailOk && phoneOk && codeOk;
            if (rowAccepted)
            {
                accepted++;
            }

            Match embeddedPhone = Regex.Match(row.Notes, @"\d{3}-\d{3}-\d{4}");
            string embeddedPhoneText = embeddedPhone.Success ? embeddedPhone.Value : "(none)";

            Console.WriteLine($"  {row.RowId}: email={(emailOk ? "OK" : "FAIL")}, " +
                              $"phone={(phoneOk ? "OK" : "FAIL")}, " +
                              $"code={(codeOk ? "OK" : "FAIL")} → {(rowAccepted ? "ACCEPTED" : "REJECTED")}");
            Console.WriteLine($"         notes phone extract: {embeddedPhoneText}");
        }
        Console.WriteLine($"  Summary: {accepted}/{batch.Count} rows accepted");
    }

    private static void PrintBatch(List<ImportRecord> rows)
    {
        foreach (ImportRecord row in rows)
        {
            Console.WriteLine($"  {row.RowId} | email={row.Email} | phone={row.Phone} | code={row.ProductCode}");
            Console.WriteLine($"           notes: {row.Notes}");
        }
    }
}

/*
 * =========================================================================
 * QUICK REFERENCE — REGULAR EXPRESSIONS
 * =========================================================================
 *
 * --- Core types ---
 *
 *   Regex              pattern engine (instance or static helpers)
 *   Match              single result — .Success, .Value, .Index, .Length, .Groups
 *   MatchCollection    enumerable Match set — .Count, foreach
 *   Group              one capture — .Value, .Index, .Length, .Captures
 *   Capture            one sub-match within a Group
 *   CaptureCollection  all captures for a repeated group
 *
 * --- Instance methods (pattern parsed once) ---
 *
 *   regex.IsMatch(input)              bool
 *   regex.Match(input)                Match (first)
 *   regex.Matches(input)              MatchCollection
 *   regex.Replace(input, replacement) string
 *   regex.Replace(input, evaluator)   string — MatchEvaluator delegate
 *   regex.Split(input)                string[]
 *
 * --- Static shortcuts (pattern parsed every call) ---
 *
 *   Regex.IsMatch / Match / Matches / Replace / Split
 *   Regex.Escape(literal)             escape metacharacters for literal match
 *
 * --- Anchors and boundaries ---
 *
 *   ^ $       start / end of input (or line with Multiline)
 *   \b \B     word boundary / non-boundary
 *
 * --- Character classes ---
 *
 *   \d \D \w \W \s \S
 *   [abc] [^abc]   positive / negated class
 *
 * --- Quantifiers ---
 *
 *   * + ? {n} {n,m}       greedy
 *   *? +? ??              lazy (non-greedy)
 *
 * --- Groups ---
 *
 *   (sub)           capturing → Groups[1]
 *   (?<name>sub)    named     → Groups["name"]
 *   (?:sub)         non-capturing
 *
 * --- Replacement tokens ---
 *
 *   $1 $2 …    numbered groups
 *   ${name}    named group
 *   $$         literal dollar sign
 *
 * --- RegexOptions (combine with |) ---
 *
 *   IgnoreCase | Multiline | Singleline | Compiled | CultureInvariant
 *   NonBacktracking (.NET 7+) | RightToLeft
 *
 * --- Patterns in this chapter ---
 *
 *   Email:    ^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$
 *   Phone:    ^(\+1[-.\s]?)?(\([0-9]{3}\)|[0-9]{3})[-.\s]?[0-9]{3}[-.\s]?[0-9]{4}$
 *   SKU:      ^[A-Z]{3}-\d{4}$
 *   ISO date: \b\d{4}-\d{2}-\d{2}\b
 *
 * --- Performance ---
 *
 *   static readonly Regex + Compiled for hot paths
 *   MatchTimeout for untrusted or complex patterns
 *   Avoid nested greedy quantifiers — ReDoS risk
 *
 * =========================================================================
 */
