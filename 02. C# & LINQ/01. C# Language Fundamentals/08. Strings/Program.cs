/*
 * =============================================================================
 * 08. STRINGS IN C# — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: The string type — text as a reference type, immutable sequences of
 *        UTF-16 characters — and the tools C# provides to create, inspect,
 *        transform, compare, and format text.
 *
 * WHY IT MATTERS:
 *   Order IDs, shipping labels, CSV exports, log messages, and API payloads
 *   are all strings. Immutability affects memory and performance; wrong
 *   comparison (case or culture) causes duplicate records; concatenating in
 *   a loop without StringBuilder wastes allocations on every iteration.
 *
 * WHAT YOU WILL LEARN:
 *   1.  String immutability and what "changing" a string really does
 *   2.  Creation: literals, new string(...), string.Empty
 *   3.  Length and the char indexer [i]
 *   4.  Common methods: Trim, Split, Join, Replace, Substring, Contains, IndexOf
 *   5.  Comparison: ==, Equals, CompareTo, OrdinalIgnoreCase
 *   6.  String interpolation with $"..."
 *   7.  Verbatim strings with @"..."
 *   8.  Preview: raw string literals (C# 11)
 *   9.  StringBuilder for efficient repeated building
 *  10.  string.Format and composite placeholders
 *  11.  Preview: culture vs invariant formatting
 *  12.  char vs string compile errors (CS1012)
 *
 * =============================================================================
 */

using System;
using System.Globalization;
using System.Text;

namespace Strings;

public class Program
{
    public static void Main(string[] args)
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

        string rawScanLine = "  ORD-1042| express |SKU-A12,SKU-B07  ";
        decimal orderTotal = 127.50m;
        int packageCount = 2;


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
         * Trade-off: repeated concatenation in loops allocates many short-lived
         * strings — use StringBuilder (Section 11) when building large text.
         * -------------------------------------------------------------------------
         */

        string beforeTrim = rawScanLine;
        string afterTrim = rawScanLine.Trim();
        bool sameReferenceAfterTrim = ReferenceEquals(beforeTrim, afterTrim);


        /*
         * =========================================================================
         * SECTION 3: STRING CREATION
         * =========================================================================
         *
         * --- 3a. Literal ---
         *
         *   string id = "ORD-1042";
         *
         * Compiler stores string literals in an intern pool; identical literals
         * in the same assembly may share the same reference.
         *
         * --- 3b. new string(...) ---
         *
         *   string copy = new string('X', 3);   // "XXX"
         *   string fromChars = new string(new[] { 'A', 'B' });
         *
         * Explicit construction when you need a fresh char buffer or repeated char.
         *
         * --- 3c. string.Empty ---
         *
         *   string blank = string.Empty;   // preferred over ""
         *
         * Semantically identical to "" but reads clearly as "empty string".
         * -------------------------------------------------------------------------
         */

        string orderPrefix = "ORD";
        string literalOrderId = "ORD-1042";
        string paddedCode = new string('0', 4);
        string fromCharArray = new string(new[] { 'S', 'K', 'U' });
        string emptyNotes = string.Empty;


        /*
         * =========================================================================
         * SECTION 4: LENGTH AND INDEXER [i]
         * =========================================================================
         *
         * .Length  — number of char elements (UTF-16 code units, not always
         *            visible graphemes for complex Unicode).
         *
         * [i]      — zero-based indexer; returns char at position i.
         *
         *   string s = "SKU";
         *   int n = s.Length;     // 3
         *   char c = s[0];        // 'S'
         *
         * Out-of-range index throws ArgumentOutOfRangeException.
         *
         * --- 4a. char vs string (CS1012) ---
         *
         *   char letter = 'A';        // single quotes → char
         *   string word = "A";        // double quotes → string
         *   // char bad = "A";        // CS1012: cannot convert string to char
         *   // string bad2 = 'A';     // CS0029: cannot convert char to string
         *
         * Use .ToString() on a char or char.ToString() when you need a string.
         * -------------------------------------------------------------------------
         */

        int scanLength = afterTrim.Length;
        char firstChar = afterTrim[0];
        char dashChar = '-';
        string dashAsString = dashChar.ToString();


        /*
         * =========================================================================
         * SECTION 5: COMMON STRING METHODS
         * =========================================================================
         *
         * All return new strings (or other values) — originals unchanged.
         *
         * Method       | Purpose
         * -------------|--------------------------------------------------------
         * Trim()       | Remove leading/trailing whitespace (and optional chars)
         * Split(sep)   | Break into string[] by delimiter
         * Join(sep,[]) | Combine array elements with separator
         * Replace(a,b) | Swap all occurrences of a with b
         * Substring(i) | Slice from index i to end (or i, length)
         * Contains(s)  | true if substring found
         * IndexOf(s)   | zero-based start index, or -1 if not found
         * -------------------------------------------------------------------------
         */

        string cleanedLine = afterTrim;
        string[] pipeParts = cleanedLine.Split('|');
        string orderSegment = pipeParts[0].Trim();
        string serviceSegment = pipeParts[1].Trim();
        string skuSegment = pipeParts[2].Trim();

        string normalizedService = serviceSegment.Replace("express", "Express");
        string[] skuList = skuSegment.Split(',');
        string skuSummary = string.Join(" + ", skuList);

        bool hasOrdPrefix = orderSegment.Contains("ORD");
        int dashIndex = orderSegment.IndexOf('-');
        string orderNumberPart = orderSegment.Substring(dashIndex + 1);


        /*
         * =========================================================================
         * SECTION 6: STRING COMPARISON
         * =========================================================================
         *
         * --- 6a. == and != ---
         *
         * Compares VALUE (character sequence), not reference, for strings.
         *
         * --- 6b. Equals ---
         *
         *   a.Equals(b)                              default culture rules
         *   a.Equals(b, StringComparison.Ordinal)    byte-by-byte ordinal
         *   a.Equals(b, StringComparison.OrdinalIgnoreCase)
         *
         * --- 6c. CompareTo ---
         *
         *   string.Compare(a, b, comparison)  → negative / zero / positive
         *   (sort order: useful before displaying sorted lists)
         *
         * For IDs, file paths, and HTTP headers prefer Ordinal or
         * OrdinalIgnoreCase — culture rules treat "i" and "I" differently in
         * Turkish locales, which breaks naive == checks on user codes.
         * -------------------------------------------------------------------------
         */

        string scannedIdUpper = orderSegment.ToUpperInvariant();
        bool idsMatchOrdinal = orderSegment == literalOrderId;
        bool idsMatchIgnoreCase = orderSegment.Equals(
            scannedIdUpper,
            StringComparison.OrdinalIgnoreCase);

        int serviceSortKey = string.Compare(
            normalizedService,
            "Standard",
            StringComparison.OrdinalIgnoreCase);

        string comparisonNote = idsMatchOrdinal
            ? "exact match"
            : idsMatchIgnoreCase
                ? "case-insensitive match"
                : "mismatch";


        /*
         * =========================================================================
         * SECTION 7: STRING INTERPOLATION ($"...")
         * =========================================================================
         *
         * Embed expressions inside a string literal:
         *
         *   string msg = $"Order {orderNumberPart} has {packageCount} box(es).";
         *
         * Expressions in { } can call methods and use format specifiers:
         *
         *   {orderTotal:C}   currency
         *   {value,10}       alignment / padding
         *
         * Interpolation is translated to string.Format at compile time — both
         * styles appear in real codebases.
         * -------------------------------------------------------------------------
         */

        string labelTitle = $"Ship: {orderSegment}";
        string boxLine = $"Packages: {packageCount} | Total: {orderTotal:C}";


        /*
         * =========================================================================
         * SECTION 8: VERBATIM STRINGS (@"...")
         * =========================================================================
         *
         * The @ prefix treats backslashes and newlines literally — ideal for
         * file paths, multi-line addresses, and JSON/XML snippets without
         * doubling every \ character.
         *
         *   string path = @"C:\Labels\out\label.txt";
         *   string address = @"Acme Warehouse
         *   100 Dock Road
         *   Portland, OR";
         *
         * To embed a double quote inside a verbatim string, double it: "".
         * -------------------------------------------------------------------------
         */

        string labelFolder = @"C:\Warehouse\Labels";
        string multiLineAddress = @"Acme Fulfillment
Dock 3 — Bay 12
Portland, OR 97201";
        string quotedService = $@"Service level: ""{normalizedService}""";


        /*
         * =========================================================================
         * SECTION 9: RAW STRING LITERALS (PREVIEW — C# 11)
         * =========================================================================
         *
         * Triple-or-more quotes allow multi-line text without escape sequences:
         *
         *   string json = """
         *       { "orderId": "ORD-1042", "priority": "express" }
         *       """;
         *
         * Delimiter length controls how many consecutive quote chars end the
         * literal. Useful for SQL, JSON, and HTML templates.
         *
         * This section uses a minimal example wired into the label scenario.
         * -------------------------------------------------------------------------
         */

        string rawJsonSnippet = """
            { "orderId": "ORD-1042", "skus": ["SKU-A12","SKU-B07"] }
            """;
        int rawJsonLength = rawJsonSnippet.Trim().Length;


        /*
         * =========================================================================
         * SECTION 10: STRING FORMATTING (string.Format)
         * =========================================================================
         *
         * Composite format placeholders {index[:format]}:
         *
         *   string.Format("Order {0} — {1:C}", id, total);
         *
         * Same specifiers as interpolation: C currency, N number, D date, etc.
         * Handy when the format string comes from a resource file or config.
         * -------------------------------------------------------------------------
         */

        string formatLine = string.Format(
            CultureInfo.CurrentCulture,
            "Order {0} | Service: {1} | SKUs: {2}",
            orderSegment,
            normalizedService,
            skuSummary);


        /*
         * =========================================================================
         * SECTION 11: CULTURE / INVARIANT FORMATTING (PREVIEW)
         * =========================================================================
         *
         * COVERED IN DETAIL LATER → 03. Input & Output
         *   (headline concepts only: CurrentCulture, InvariantCulture, parsing)
         *
         * Numbers and dates render differently by locale:
         *   en-US  → 1,234.56
         *   de-DE  → 1.234,56
         *
         * Use CultureInfo.InvariantCulture for logs, file names, and wire formats
         * so output is identical on every machine. Use CurrentCulture for user-
         * facing UI. string.Format and :C / :N specifiers respect the culture
         * passed in (or Thread.CurrentThread.CurrentCulture by default).
         * -------------------------------------------------------------------------
         */

        string cultureTotal = orderTotal.ToString("C", CultureInfo.CurrentCulture);
        string invariantTotal = orderTotal.ToString("C", CultureInfo.InvariantCulture);


        /*
         * =========================================================================
         * SECTION 12: STRINGBUILDER
         * =========================================================================
         *
         * System.Text.StringBuilder maintains a mutable char buffer. Append and
         * Insert mutate the buffer in place; ToString() materializes the final
         * string once.
         *
         * WHEN TO USE:
         *   • Building large text in loops (log buffers, CSV rows, HTML)
         *   • Many append operations where string + string would allocate repeatedly
         *
         * WHEN NOT NEEDED:
         *   • A few interpolations or Join calls — plain strings are simpler.
         * -------------------------------------------------------------------------
         */

        StringBuilder labelBuilder = new StringBuilder();
        labelBuilder.Append(labelTitle);
        labelBuilder.AppendLine();
        labelBuilder.Append(boxLine);
        labelBuilder.AppendLine();
        labelBuilder.Append(formatLine);
        labelBuilder.AppendLine();
        labelBuilder.Append("SKUs: ");
        labelBuilder.Append(skuSummary);

        string fullLabel = labelBuilder.ToString();


        /*
         * =========================================================================
         * SECTION 13: SHIPPING LABEL SUMMARY (OUTPUT)
         * =========================================================================
         * Prints results from every section — no unused variables.
         * -------------------------------------------------------------------------
         */

        Console.WriteLine("=== Strings — Shipping Label Processing ===");
        Console.WriteLine();
        Console.WriteLine($"Raw scan ({scanLength} chars after trim): \"{cleanedLine}\"");
        Console.WriteLine($"First char: '{firstChar}' | Dash as string: \"{dashAsString}\"");
        Console.WriteLine($"Trim reused same reference? {sameReferenceAfterTrim}");
        Console.WriteLine($"Creation: prefix={orderPrefix}, pad={paddedCode}, chars={fromCharArray}, notes='{emptyNotes}'");
        Console.WriteLine();
        Console.WriteLine($"Parsed order: {orderSegment} (number part: {orderNumberPart})");
        Console.WriteLine($"Service: {normalizedService} | CompareTo Standard: {serviceSortKey}");
        Console.WriteLine($"SKU list: {skuSummary} | Has ORD prefix: {hasOrdPrefix}");
        Console.WriteLine($"ID check: {comparisonNote}");
        Console.WriteLine();
        Console.WriteLine(labelTitle);
        Console.WriteLine(boxLine);
        Console.WriteLine(formatLine);
        Console.WriteLine();
        Console.WriteLine($"Label folder: {labelFolder}");
        Console.WriteLine("Ship-to:");
        Console.WriteLine(multiLineAddress);
        Console.WriteLine(quotedService);
        Console.WriteLine();
        Console.WriteLine($"Culture total: {cultureTotal} | Invariant: {invariantTotal}");
        Console.WriteLine($"Raw JSON snippet length: {rawJsonLength}");
        Console.WriteLine();
        Console.WriteLine("--- Assembled label (StringBuilder) ---");
        Console.WriteLine(fullLabel);
    }
}

/*
 * =============================================================================
 * QUICK REFERENCE — STRINGS
 * =============================================================================
 *
 * --- Core facts ---
 *
 *  string     reference type, immutable sequence of char (UTF-16)
 *  "text"     literal (intern pool)
 *  string.Empty   canonical empty string
 *  s.Length   char count; s[i] returns char (zero-based)
 *
 * --- Common methods ---
 *
 *  Trim / TrimStart / TrimEnd     remove whitespace (or custom chars)
 *  Split(sep)                     string[] parts
 *  string.Join(sep, parts)        combine array
 *  Replace(old, new)              swap substrings
 *  Substring(start[, len])        slice
 *  Contains / StartsWith / EndsWith
 *  IndexOf / LastIndexOf          -1 if not found
 *  ToUpper / ToLower              culture-sensitive; *Invariant variants exist
 *
 * --- Comparison ---
 *
 *  == / !=                        value equality for strings
 *  Equals(s, StringComparison.OrdinalIgnoreCase)   IDs, paths
 *  string.Compare(a, b, comparison)                sort keys
 *
 * --- Formatting ---
 *
 *  $"Hello {name}"                interpolation (preferred for inline)
 *  string.Format("{0}", arg)      composite / resource strings
 *  @"C:\path\file"                verbatim — \ and newlines literal
 *  """multi
 *     line"""                     raw string literal (C# 11+)
 *
 * --- Building text ---
 *
 *  StringBuilder sb = new StringBuilder();
 *  sb.Append("part"); sb.AppendLine(); string result = sb.ToString();
 *
 * --- Common mistakes ---
 *
 *  Mistake                              | Result
 *  -------------------------------------|----------------------------------
 *  Expecting Trim/Replace to mutate s   | original unchanged — reassign
 *  s1 + s2 in tight loop                | many allocations — StringBuilder
 *  == on user text without culture plan | wrong matches in some locales
 *  char c = "A"                         | CS1012 (use 'A' or "A"[0])
 *  string s = 'A'                       | CS0029 (use "A" or c.ToString())
 *  IndexOutOfRange on s[i]              | ArgumentOutOfRangeException
 *
 * --- Deferred ---
 *
 *  Regex pattern matching → 08. Advanced C# Features / 04. Regular Expressions
 *
 * =============================================================================
 */
