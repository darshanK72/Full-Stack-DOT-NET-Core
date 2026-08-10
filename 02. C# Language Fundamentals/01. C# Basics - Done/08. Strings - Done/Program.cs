/*
 * =============================================================================
 * 08. STRINGS IN C# — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: The string type — text as a reference type, immutable sequences of
 *        UTF-16 characters — and the tools C# provides to create, inspect,
 *        transform, compare, format, and efficiently build text.
 *
 * WHY IT MATTERS:
 *   Order IDs, shipping labels, CSV exports, log messages, and API payloads
 *   are all strings. Immutability affects memory and performance; wrong
 *   comparison (case or culture) causes duplicate records; concatenating in
 *   a loop without StringBuilder wastes allocations on every iteration.
 *
 * WHAT YOU WILL LEARN:
 *   1.  String immutability, intern pool, and reference vs value equality
 *   2.  Creation: literals, constructors, Empty, Concat, char vs string (CS1012)
 *   3.  Length, indexer [i], null / empty / whitespace checks
 *   4.  Common methods: Trim, Split, Join, Replace, Substring, search helpers
 *   5.  PadLeft / PadRight, Insert, Remove, ToCharArray, case conversion
 *   6.  Comparison: ==, Equals, Compare, CompareTo, StringComparison rules
 *   7.  String interpolation ($"...") with format specifiers and alignment
 *   8.  Verbatim strings (@"...") and raw string literals ("""...""")
 *   9.  string.Format and composite placeholders
 *  10.  Culture and invariant formatting for string output and parsing
 *  11.  StringBuilder — mutable buffers for repeated building
 *
 * =============================================================================
 */

using System;
using System.Globalization;
using System.Text;

namespace Strings;

/*
 * =========================================================================
 * SECTION 2: STRING IMMUTABILITY
 * =========================================================================
 *
 * string is IMMUTABLE — once created, its character sequence cannot change.
 * Methods like Replace and Trim do NOT modify the original; they return a
 * NEW string object.
 *
 *   string s = "hello";
 *   s.ToUpper();     // returns "HELLO"; s is still "hello"
 *   s = s.ToUpper(); // reassign the variable to point at the new object
 *
 * Why immutability?
 *   • Thread-safe sharing (no one can mutate shared text)
 *   • Safe dictionary keys and interning (literal pool)
 *   • Predictable behavior when passing strings between methods
 *
 * --- 2a. Intern pool ---
 *
 * Identical string LITERALS in the same assembly may share one reference:
 *
 *   string a = "ORD-1042";
 *   string b = "ORD-1042";
 *   ReferenceEquals(a, b);   // often true for literals
 *
 *   string c = new string(new[] { 'O', 'R', 'D' });
 *   ReferenceEquals(a, c);   // false — explicit new buffer
 *
 * string.Intern(s) returns the canonical pooled instance for that sequence.
 *
 * Trade-off: repeated concatenation in loops allocates many short-lived
 * strings — use StringBuilder (Section 11) when building large text.
 * -------------------------------------------------------------------------
 */
public static class StringImmutabilityDemo
{
    public static (string Original, string Trimmed, bool SameReferenceAfterTrim) DemonstrateTrim(string raw)
    {
        string trimmed = raw.Trim(); // returns new string — original raw unchanged
        return (raw, trimmed, ReferenceEquals(raw, trimmed)); // false when Trim allocates
    }

    public static bool LiteralsShareReference(string literalA, string literalB)
    {
        return ReferenceEquals(literalA, literalB); // often true for identical compile-time literals
    }
}

/*
 * =========================================================================
 * SECTION 3: PARSED SCAN LINE — RESULT OF COMMON STRING METHODS
 * =========================================================================
 *
 * Holds the pieces extracted from a warehouse scan line. The static parser
 * below (Section 5) uses Trim, Split, Replace, Join, Substring, Contains,
 * IndexOf, StartsWith, EndsWith, PadLeft, and case conversion.
 * -------------------------------------------------------------------------
 */
public sealed class ParsedScanLine
{
    public string OrderId { get; init; } = string.Empty;
    public string OrderNumberPart { get; init; } = string.Empty;
    public string ServiceLevel { get; init; } = string.Empty;
    public string SkuSummary { get; init; } = string.Empty;
    public int SkuCount { get; init; }
    public bool HasOrdPrefix { get; init; }
    public int DashIndex { get; init; }
    public string SkuPrefix { get; init; } = string.Empty;
}

/*
 * =========================================================================
 * SECTION 5: COMMON STRING METHODS
 * =========================================================================
 *
 * All instance methods that "change" text return new strings — originals
 * stay unchanged.
 *
 * Method              | Purpose
 * --------------------|---------------------------------------------------
 * Trim / TrimStart /  | Remove leading/trailing whitespace (or custom chars)
 * TrimEnd             |
 * Split(sep)          | Break into string[] by delimiter
 * string.Join(sep,[]) | Combine array elements with separator
 * Replace(a,b)        | Swap all occurrences of a with b
 * Substring(i[, len]) | Slice from index i
 * Contains(s)         | true if substring found
 * StartsWith / EndsWith | prefix / suffix test (optional StringComparison)
 * IndexOf / LastIndexOf | zero-based index, or -1 if not found
 * PadLeft / PadRight  | Pad to total width with a char (default space)
 * Insert / Remove     | Insert at index / remove range
 * ToCharArray()       | Copy chars into char[]
 * ToUpper / ToLower   | culture-sensitive case change
 * ToUpperInvariant /  | culture-independent case (preferred for IDs)
 * ToLowerInvariant    |
 *
 * Static helpers on string:
 *   string.IsNullOrEmpty(s)       null or ""
 *   string.IsNullOrWhiteSpace(s)  null, "", or all whitespace
 *   string.Concat(a, b, …)        combine without + between every pair
 * -------------------------------------------------------------------------
 */
public static class ScanLineParser
{
    public static ParsedScanLine Parse(string rawScanLine)
    {
        string cleaned = rawScanLine.Trim(); // leading/trailing spaces removed — new string

        string[] pipeParts = cleaned.Split('|'); // pipe-delimited scan segments
        string orderSegment = pipeParts[0].Trim();
        string serviceSegment = pipeParts[1].Trim();
        string skuSegment = pipeParts[2].Trim();

        string normalizedService = serviceSegment
            .Replace("express", "Express", StringComparison.OrdinalIgnoreCase); // culture-aware replace overload

        string[] skuList = skuSegment.Split(',', StringSplitOptions.RemoveEmptyEntries);
        string skuSummary = string.Join(" + ", skuList); // static Join — combines array with separator

        int dashIndex = orderSegment.IndexOf('-'); // -1 if not found
        string orderNumberPart = orderSegment.Substring(dashIndex + 1); // slice after the dash
        string paddedNumber = orderNumberPart.PadLeft(6, '0'); // fixed-width numeric part

        string withInsertedTag = orderSegment.Insert(dashIndex, ":");
        string tagRemoved = withInsertedTag.Remove(dashIndex, 1); // Remove(start, count) — back to ORD-1042 shape

        char[] skuChars = skuSegment.ToCharArray(); // copy chars out of immutable string
        string skuFromChars = new string(skuChars, 0, 3); // ctor (char[], start, length)

        return new ParsedScanLine
        {
            OrderId = tagRemoved,
            OrderNumberPart = paddedNumber,
            ServiceLevel = normalizedService,
            SkuSummary = skuSummary,
            SkuCount = skuList.Length,
            HasOrdPrefix = orderSegment.StartsWith("ORD", StringComparison.Ordinal), // machine ID — Ordinal
            DashIndex = dashIndex,
            SkuPrefix = skuFromChars,
        };
    }

    public static string CreatePaddedCode(int width)
    {
        return new string('0', width); // repeats char — not the same as interned literal pool
    }

    public static string CreateFromCharArray(char[] chars)
    {
        return new string(chars); // explicit buffer — ReferenceEquals with literals usually false
    }

    public static string CombinePrefixAndId(string prefix, string idPart)
    {
        return string.Concat(prefix, "-", idPart); // static Concat — avoids chained + allocations
    }
}

/*
 * =========================================================================
 * SECTION 6: STRING COMPARISON
 * =========================================================================
 *
 * --- 6a. == and != ---
 *
 * For string, == compares VALUE (character sequence), not reference.
 *
 * --- 6b. StringComparison ---
 *
 *  Member                         | Use when
 *  -------------------------------|------------------------------------------
 *  Ordinal                        | IDs, file paths, HTTP headers, wire data
 *  OrdinalIgnoreCase              | Case-insensitive IDs / codes
 *  CurrentCulture                 | User-facing sort/display (locale rules)
 *  CurrentCultureIgnoreCase       | User text, case-insensitive
 *  InvariantCulture               | Stable sort/format across machines
 *  InvariantCultureIgnoreCase     | Stable case-insensitive compare
 *
 * Turkish locale pitfall: culture-sensitive ToUpper on "i" may not equal "I"
 * under Ordinal rules. For machine identifiers always prefer Ordinal* or
 * *Invariant* case conversion.
 *
 * --- 6c. Equals / Compare / CompareTo ---
 *
 *   a.Equals(b, StringComparison.OrdinalIgnoreCase)
 *   string.Compare(a, b, comparison)  → negative / zero / positive (sort key)
 *   string.CompareOrdinal(a, b)       → fastest ordinal byte compare
 * -------------------------------------------------------------------------
 */
public static class StringComparisonDemo
{
    public static (bool ExactMatch, bool IgnoreCaseMatch, string Note) CompareOrderIds(
        string scannedId,
        string expectedId)
    {
        bool exact = scannedId == expectedId; // == compares character sequence, not reference
        bool ignoreCase = scannedId.Equals(expectedId, StringComparison.OrdinalIgnoreCase);

        string note = exact
            ? "exact match"
            : ignoreCase
                ? "case-insensitive match"
                : "mismatch";

        return (exact, ignoreCase, note);
    }

    public static int CompareServiceToStandard(string serviceLevel)
    {
        return string.Compare(
            serviceLevel,
            "Standard",
            StringComparison.OrdinalIgnoreCase); // negative / zero / positive sort key
    }

    public static bool TurkishCultureCaseTrap(string lowerI)
    {
        CultureInfo turkish = CultureInfo.GetCultureInfo("tr-TR");
        string upperInTurkish = lowerI.ToUpper(turkish); // culture-sensitive — "i" may not become Ordinal "I"
        return upperInTurkish.Equals("I", StringComparison.Ordinal);
    }
}

/*
 * =========================================================================
 * SECTION 7: STRING INTERPOLATION ($"...")
 * =========================================================================
 *
 * Embed expressions inside a string literal:
 *
 *   string msg = $"Order {orderNumberPart} has {packageCount} box(es).";
 *
 * Format specifiers inside { }:
 *   {orderTotal:C}     currency (uses current culture unless :C with provider)
 *   {value,10}         minimum width 10 (right-aligned)
 *   {value,-10}        left-aligned
 *   {value:0000}       custom numeric pattern
 *
 * Interpolation is lowered to string.Format at compile time — both styles
 * appear in real codebases. Escape { as {{ and } as }} inside the literal.
 * -------------------------------------------------------------------------
 */
public static class InterpolationDemo
{
    public static (string Title, string BoxLine, string AlignedSku) BuildLines(
        string orderId,
        int packageCount,
        decimal orderTotal,
        string skuSummary)
    {
        string title = $"Ship: {orderId}";
        string boxLine = $"Packages: {packageCount} | Total: {orderTotal:C}"; // :C — currency format specifier
        string alignedSku = $"SKUs: {skuSummary,20}"; // ,20 — minimum width, right-aligned

        return (title, boxLine, alignedSku);
    }
}

/*
 * =========================================================================
 * SECTION 8: VERBATIM STRINGS (@"...")
 * =========================================================================
 *
 * The @ prefix treats backslashes and newlines literally — ideal for file
 * paths, multi-line addresses, and snippets without doubling every \.
 *
 *   string path = @"C:\Labels\out\label.txt";
 *
 * To embed a double quote inside a verbatim string, double it: "".
 * Combine with interpolation: $@"Service: ""{level}""".
 * -------------------------------------------------------------------------
 */
public static class VerbatimDemo
{
    public static (string LabelFolder, string Address, string QuotedService) BuildPaths(
        string serviceLevel)
    {
        string folder = @"C:\Warehouse\Labels"; // verbatim — backslashes literal, no \\
        string address = @"Acme Fulfillment
Dock 3 — Bay 12
Portland, OR 97201"; // multi-line verbatim — newlines preserved
        string quoted = $@"Service level: ""{serviceLevel}"""; // doubled "" embeds quote in verbatim

        return (folder, address, quoted);
    }
}

/*
 * =========================================================================
 * SECTION 9: RAW STRING LITERALS (C# 11+)
 * =========================================================================
 *
 * Delimiters of three or more quotes allow multi-line text without escapes:
 *
 *   string json = """
 *       { "orderId": "ORD-1042" }
 *       """;
 *
 * Indentation common to all lines is stripped from the output. Increase the
 * number of quote characters when the content itself contains triple quotes.
 * -------------------------------------------------------------------------
 */
public static class RawStringDemo
{
    public static string OrderJsonSnippet()
    {
        return """
            { "orderId": "ORD-1042", "skus": ["SKU-A12","SKU-B07"] }
            """;
    }
}

/*
 * =========================================================================
 * SECTION 10: STRING FORMATTING (string.Format)
 * =========================================================================
 *
 * Composite format placeholders {index[:format]}:
 *
 *   string.Format("Order {0} — {1:C}", id, total);
 *
 * Overload with IFormatProvider controls culture for every placeholder:
 *
 *   string.Format(CultureInfo.InvariantCulture, "Export {0:F2}", amount);
 *
 * Handy when the format string comes from a resource file or configuration.
 * -------------------------------------------------------------------------
 */
public static class FormatDemo
{
    public static string FormatOrderLine(
        IFormatProvider provider,
        string orderId,
        string serviceLevel,
        string skuSummary)
    {
        return string.Format(
            provider,
            "Order {0} | Service: {1} | SKUs: {2}", // composite placeholders — index-based
            orderId,
            serviceLevel,
            skuSummary);
    }
}

/*
 * =========================================================================
 * SECTION 11: CULTURE AND INVARIANT FORMATTING
 * =========================================================================
 *
 * Text operations and numeric/date formatting both depend on culture.
 *
 * CultureInfo.CurrentCulture     — thread's UI culture (user locale)
 * CultureInfo.InvariantCulture   — fixed English-like rules for logs, files,
 *                                  wire formats, and cross-machine consistency
 *
 * --- 11a. Formatting numbers into strings ---
 *
 *   amount.ToString("C", CultureInfo.CurrentCulture)    → $127.50 (en-US)
 *   amount.ToString("C", CultureInfo.InvariantCulture)  → $127.50 (invariant)
 *
 * German (de-DE) uses comma as decimal separator: 127,50 €
 *
 * --- 11b. Parsing strings back to numbers ---
 *
 * Always pass the same culture used to produce the text:
 *
 *   decimal.Parse("1.234,56", deDe)   → 1234.56
 *   decimal.Parse("1,234.56", enUs)   → 1234.56
 *
 * Prefer decimal.TryParse for user input — FormatException on bad data.
 *
 * --- 11c. String comparison with culture ---
 *
 * string.Compare(a, b, StringComparison.CurrentCulture) respects locale sort
 * rules (e.g. ä near a in some cultures). Use Ordinal for machine data.
 *
 * Console output formatting for tables was introduced in 03. Input & Output;
 * this section focuses on culture as it applies to string content itself.
 * -------------------------------------------------------------------------
 */
public static class CultureStringDemo
{
    public static (string UsCurrency, string InvariantCurrency, string GermanCurrency) FormatTotals(
        decimal orderTotal)
    {
        CultureInfo enUs = CultureInfo.GetCultureInfo("en-US");
        CultureInfo deDe = CultureInfo.GetCultureInfo("de-DE");

        string us = orderTotal.ToString("C", enUs);
        string invariant = orderTotal.ToString("C", CultureInfo.InvariantCulture); // stable across machines
        string german = orderTotal.ToString("C", deDe); // comma decimal separator in de-DE

        return (us, invariant, german);
    }

    public static (decimal ParsedGerman, bool TryParseFailed) ParseLocalizedAmounts()
    {
        CultureInfo deDe = CultureInfo.GetCultureInfo("de-DE");
        CultureInfo enUs = CultureInfo.GetCultureInfo("en-US");

        decimal parsedGerman = decimal.Parse("1.234,56", deDe); // culture must match how text was written
        bool tryFail = !decimal.TryParse("1.234,56", NumberStyles.Number, enUs, out _); // wrong culture → false

        return (parsedGerman, tryFail);
    }

    public static int CompareWordsWithCulture(string left, string right)
    {
        return string.Compare(left, right, StringComparison.CurrentCulture); // locale sort rules — not for IDs
    }
}

/*
 * =========================================================================
 * SECTION 12: STRINGBUILDER
 * =========================================================================
 *
 * System.Text.StringBuilder maintains a mutable char buffer. Append, Insert,
 * Remove, and Replace mutate the buffer in place; ToString() materializes the
 * final string once.
 *
 *  API              | Role
 *  -----------------|-------------------------------------------------------
 *  Append / AppendLine | Add text; AppendLine adds Environment.NewLine
 *  AppendFormat     | Same placeholders as string.Format
 *  Insert / Remove  | Mutate middle of buffer
 *  Replace          | Replace substring inside buffer
 *  Clear            | Remove all characters (Length → 0)
 *  Length / Capacity| Current char count / allocated buffer size
 *
 * WHEN TO USE:
 *   • Building large text in loops (log buffers, CSV rows, HTML)
 *   • Many append operations where + would allocate repeatedly
 *
 * WHEN NOT NEEDED:
 *   • A few interpolations or string.Join — plain strings are simpler.
 * -------------------------------------------------------------------------
 */
public static class LabelAssembler
{
    public static string BuildFullLabel(
        string title,
        string boxLine,
        string formatLine,
        string skuSummary)
    {
        StringBuilder builder = new StringBuilder(capacity: 256); // pre-size buffer to reduce reallocations
        builder.Append(title);
        builder.AppendLine(); // appends Environment.NewLine
        builder.Append(boxLine);
        builder.AppendLine();
        builder.Append(formatLine);
        builder.AppendLine();
        builder.AppendFormat("SKUs: {0}", skuSummary); // same {0} style as string.Format

        int skuStart = builder.ToString().IndexOf("SKUs:", StringComparison.Ordinal); // materializes snapshot for search
        builder.Insert(skuStart, "--- "); // mutates buffer in place — unlike immutable string
        builder.Replace("--- SKUs:", "SKUs:"); // Replace on StringBuilder — no new string until ToString

        int bufferLength = builder.Length;
        int bufferCapacity = builder.Capacity; // allocated char slots — may exceed Length

        builder.Clear(); // Length → 0; Capacity often retained for reuse
        builder.Append("(cleared length was ");
        builder.Append(bufferLength);
        builder.Append(", capacity ");
        builder.Append(bufferCapacity);
        builder.Append(')'); // Append(char) — single char overload

        string clearedNote = builder.ToString();

        builder.Clear();
        builder.Append(title);
        builder.AppendLine();
        builder.Append(boxLine);
        builder.AppendLine();
        builder.Append(formatLine);
        builder.AppendLine();
        builder.Append("SKUs: ");
        builder.Append(skuSummary);
        builder.Append(' ');
        builder.Append(clearedNote); // final label — one ToString at the end

        return builder.ToString();
    }
}

public class Program
{
    /*
     * =========================================================================
     * SECTION 1: SHIPPING LABEL SCENARIO
     * =========================================================================
     *
     * A warehouse prints shipping labels from raw scan data. This chapter
     * walks through parsing, cleaning, comparing, and formatting that text.
     * Values are fixed so the program runs without keyboard input; every
     * variable is used in the final summary output.
     * -------------------------------------------------------------------------
     */
    public static void Main(string[] args)
    {
        string rawScanLine = "  ORD-1042| express |SKU-A12,SKU-B07  ";
        decimal orderTotal = 127.50m;
        int packageCount = 2;
        string literalOrderId = "ORD-1042";
        string orderPrefix = "ORD";


        /*
         * =========================================================================
         * SECTION 4: LENGTH, INDEXER, char vs string
         * =========================================================================
         *
         * .Length  — number of char elements (UTF-16 code units; complex
         *            Unicode graphemes may use multiple code units).
         *
         * [i]      — zero-based indexer; returns char at position i.
         *
         *   string s = "SKU";
         *   int n = s.Length;     // 3
         *   char c = s[0];        // 'S'
         *
         * Out-of-range index throws IndexOutOfRangeException.
         *
         * --- 4a. char vs string (CS1012 / CS0029) ---
         *
         *   char letter = 'A';        // single quotes → char
         *   string word = "A";        // double quotes → string
         *   // char bad = "A";        // CS1012: cannot convert string to char
         *   // string bad2 = 'A';     // CS0029: cannot convert char to string
         *
         * Use char.ToString() when you need a one-character string.
         * -------------------------------------------------------------------------
         */

        (string original, string trimmed, bool sameRefAfterTrim) =
            StringImmutabilityDemo.DemonstrateTrim(rawScanLine);

        int scanLength = trimmed.Length;       // char count (UTF-16 code units)
        char firstChar = trimmed[0];           // zero-based indexer — returns char, not string
        char dashChar = '-';                   // single quotes → char (CS1012 if you used "...")
        string dashAsString = dashChar.ToString(); // char → one-character string

        bool literalsShareRef = StringImmutabilityDemo.LiteralsShareReference(
            literalOrderId,
            "ORD-1042"); // second literal may share intern pool with literalOrderId

        string paddedCode = ScanLineParser.CreatePaddedCode(4);
        string fromCharArray = ScanLineParser.CreateFromCharArray(new[] { 'S', 'K', 'U' });
        string emptyNotes = string.Empty; // canonical empty — not null
        string combinedId = ScanLineParser.CombinePrefixAndId(orderPrefix, "1042");

        bool notesEmpty = string.IsNullOrEmpty(emptyNotes);
        bool blankIsWhite = string.IsNullOrWhiteSpace("   \t"); // tabs/spaces count as whitespace

        ParsedScanLine parsed = ScanLineParser.Parse(rawScanLine); // Trim, Split, Join, Replace, etc.

        (bool exactMatch, bool ignoreCaseMatch, string comparisonNote) =
            StringComparisonDemo.CompareOrderIds(parsed.OrderId, literalOrderId);

        int serviceSortKey = StringComparisonDemo.CompareServiceToStandard(parsed.ServiceLevel);
        bool turkishTrap = StringComparisonDemo.TurkishCultureCaseTrap("i");

        (string labelTitle, string boxLine, string alignedSku) =
            InterpolationDemo.BuildLines(
                parsed.OrderId,
                packageCount,
                orderTotal,
                parsed.SkuSummary);

        (string labelFolder, string multiLineAddress, string quotedService) =
            VerbatimDemo.BuildPaths(parsed.ServiceLevel);

        string rawJsonSnippet = RawStringDemo.OrderJsonSnippet(); // raw """ literal — C# 11+
        int rawJsonLength = rawJsonSnippet.Trim().Length;

        string formatLine = FormatDemo.FormatOrderLine(
            CultureInfo.CurrentCulture, // thread UI culture drives {0} formatting
            parsed.OrderId,
            parsed.ServiceLevel,
            parsed.SkuSummary);

        (string usTotal, string invariantTotal, string germanTotal) =
            CultureStringDemo.FormatTotals(orderTotal);

        (decimal parsedGermanAmount, bool enUsParseFails) =
            CultureStringDemo.ParseLocalizedAmounts();

        int cultureWordCompare = CultureStringDemo.CompareWordsWithCulture("apple", "Banana");

        string fullLabel = LabelAssembler.BuildFullLabel(
            labelTitle,
            boxLine,
            formatLine,
            parsed.SkuSummary); // StringBuilder — many appends, one final string

        Console.WriteLine("=== Strings — Shipping Label Processing ===");
        Console.WriteLine();
        Console.WriteLine($"Raw scan ({scanLength} chars after trim): \"{trimmed}\"");
        Console.WriteLine($"First char: '{firstChar}' | Dash as string: \"{dashAsString}\"");
        Console.WriteLine($"Trim reused same reference? {sameRefAfterTrim}");
        Console.WriteLine($"Literal intern match? {literalsShareRef}");
        Console.WriteLine(
            $"Creation: prefix={orderPrefix}, pad={paddedCode}, chars={fromCharArray}, " +
            $"combined={combinedId}, notes empty={notesEmpty}, whitespace={blankIsWhite}");
        Console.WriteLine();
        Console.WriteLine($"Parsed order: {parsed.OrderId} (padded #: {parsed.OrderNumberPart})");
        Console.WriteLine($"Service: {parsed.ServiceLevel} | CompareTo Standard: {serviceSortKey}");
        Console.WriteLine(
            $"SKU list: {parsed.SkuSummary} ({parsed.SkuCount} items) | " +
            $"Prefix chars: {parsed.SkuPrefix} | Has ORD prefix: {parsed.HasOrdPrefix} | Dash at: {parsed.DashIndex}");
        Console.WriteLine($"ID check: {comparisonNote} (exact={exactMatch}, ignoreCase={ignoreCaseMatch})");
        Console.WriteLine($"Turkish 'i'.ToUpper(tr-TR) equals \"I\"? {turkishTrap}");
        Console.WriteLine();
        Console.WriteLine(labelTitle);
        Console.WriteLine(boxLine);
        Console.WriteLine(alignedSku);
        Console.WriteLine(formatLine);
        Console.WriteLine();
        Console.WriteLine($"Label folder: {labelFolder}");
        Console.WriteLine("Ship-to:");
        Console.WriteLine(multiLineAddress);
        Console.WriteLine(quotedService);
        Console.WriteLine();
        Console.WriteLine($"en-US: {usTotal} | Invariant: {invariantTotal} | de-DE: {germanTotal}");
        Console.WriteLine(
            $"Parsed de-DE \"1.234,56\" → {parsedGermanAmount}; en-US TryParse fails? {enUsParseFails}");
        Console.WriteLine($"CurrentCulture Compare(\"apple\",\"Banana\"): {cultureWordCompare}");
        Console.WriteLine($"Raw JSON snippet length: {rawJsonLength}");
        Console.WriteLine();
        Console.WriteLine("--- Assembled label (StringBuilder) ---");
        Console.WriteLine(fullLabel);
        Console.WriteLine();
        Console.WriteLine($"Original reference unchanged: \"{original}\""); // rawScanLine never mutated — still has padding
    }
}

/*
 * =============================================================================
 * QUICK REFERENCE — STRINGS
 * =============================================================================
 *
 * --- Core facts ---
 *
 *  string              reference type, immutable UTF-16 char sequence
 *  "text"              literal (may be interned)
 *  string.Empty        canonical empty string
 *  s.Length            char count; s[i] → char (zero-based)
 *
 * --- Common methods ---
 *
 *  Trim / TrimStart / TrimEnd       remove whitespace or custom chars
 *  Split / string.Join              break apart / combine
 *  Replace / Substring / Insert / Remove
 *  Contains / StartsWith / EndsWith
 *  IndexOf / LastIndexOf            -1 if not found
 *  PadLeft / PadRight               fixed-width columns
 *  ToUpper* / ToLower*              *Invariant for IDs and logs
 *  ToCharArray()                    copy to char[]
 *  string.IsNullOrEmpty / IsNullOrWhiteSpace
 *  string.Concat(…)                 combine without repeated +
 *
 * --- Comparison ---
 *
 *  == / !=                          value equality for strings
 *  Equals(s, StringComparison.*)    explicit rules — prefer Ordinal for IDs
 *  string.Compare / CompareOrdinal  sort keys
 *
 * --- Formatting ---
 *
 *  $"Hello {name:C}"                interpolation (inline preferred)
 *  string.Format(provider, "{0}", x)  composite / resource strings
 *  @"C:\path\file"                  verbatim — \ and newlines literal
 *  """multi line"""                  raw string literal (C# 11+)
 *
 * --- Culture ---
 *
 *  CultureInfo.CurrentCulture       user locale (UI, display)
 *  CultureInfo.InvariantCulture     stable logs, files, wire formats
 *  ToString("C", culture)           format numbers/dates into strings
 *  decimal.Parse(text, culture)     parse localized numeric strings
 *
 * --- Building text ---
 *
 *  StringBuilder sb = new StringBuilder(capacity);
 *  sb.Append("x"); sb.AppendLine(); sb.ToString();
 *
 * --- Common mistakes ---
 *
 *  Mistake                              | Result
 *  -------------------------------------|----------------------------------
 *  Expecting Trim/Replace to mutate s   | original unchanged — reassign
 *  s1 + s2 in tight loop                | many allocations — StringBuilder
 *  == on user text without culture plan | wrong matches in some locales
 *  char c = "A"                         | CS1012 (use 'A')
 *  string s = 'A'                       | CS0029 (use "A" or c.ToString())
 *  Parse without matching culture       | FormatException or wrong value
 *
 * --- Deferred ---
 *
 *  Regex → future Regular Expressions chapter
 *  ReadOnlySpan<char> → advanced performance topics
 *
 * =============================================================================
 */
