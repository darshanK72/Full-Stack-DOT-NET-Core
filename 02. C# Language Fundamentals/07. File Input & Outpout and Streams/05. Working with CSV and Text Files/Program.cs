/*
 * =============================================================================
 * 05. WORKING WITH CSV AND TEXT FILES — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: Read and write comma-separated (and other delimited) text files —
 *        split columns, escape commas and quotes, build rows with StringBuilder,
 *        parse typed fields from header-driven rows, and import/export inventory
 *        data line-by-line with StreamReader and StreamWriter.
 *
 * WHY IT MATTERS:
 *   Spreadsheets, ERP exports, and partner feeds often arrive as CSV. Loading
 *   an entire multi-megabyte file with ReadAllText wastes memory and hides the
 *   line number when one row is malformed. Line-by-line StreamReader processing
 *   keeps memory flat, reports row numbers in errors, and lets you skip bad rows
 *   while importing the rest.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Manual CSV split — String.Split for simple files; basics of quoted fields
 *   2.  StringBuilder — assemble escaped CSV rows without extra string allocations
 *   3.  CSV escaping — wrap fields in quotes; double internal quotes per RFC-style rules
 *   4.  StreamReader — line-by-line ReadLine loop for large imports (depth → ch.02)
 *   5.  Header rows and typed parsing — int, decimal, and DateTime.TryParse
 *   6.  StreamWriter export — write inventory CSV with proper escaping
 *   7.  Pitfalls — culture/decimal separators, empty fields, trailing commas
 *
 * =============================================================================
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace WorkingWithCsvAndTextFiles;

/*
 * =========================================================================
 * SECTION 1: INVENTORY ROW — DOMAIN TYPE FOR IMPORT / EXPORT
 * =========================================================================
 *
 * A small record models one warehouse line after CSV columns are validated.
 * init-only properties are set once when a row passes parsing; LineValue is
 * computed from Quantity × UnitPrice.
 *
 * Column mapping (header row on disk):
 *
 *   Sku          string    required — empty SKU is a row error
 *   Name         string    product label (may contain commas when quoted on disk)
 *   Quantity     int       TryParse with InvariantCulture
 *   UnitPrice    decimal   TryParse with InvariantCulture (avoid 12,99 vs 12.99 bugs)
 *   RestockedOn  DateTime  optional — TryParse; blank field → null
 * -------------------------------------------------------------------------
 */
public sealed class InventoryItem
{
    public string Sku { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public DateTime? RestockedOn { get; init; }

    public decimal LineValue => Quantity * UnitPrice;
}

/*
 * =========================================================================
 * SECTION 2: CSV FIELD ESCAPING — COMMAS, QUOTES, NEWLINES
 * =========================================================================
 *
 * When a field contains a comma, quote, or newline, wrap the whole field in
 * double quotes and double any internal quote character:
 *
 *   Acme, Inc        →  "Acme, Inc"
 *   12" wrench       →  "12"" wrench"
 *
 * Fields without special characters are written as-is (no quotes required).
 *
 * Pitfall — culture and decimals:
 *   decimal.ToString() uses the current thread culture. German exports use
 *   "12,99" while US/Invariant use "12.99". For CSV interchange, prefer
 *   InvariantCulture when writing numbers and when parsing with TryParse.
 * -------------------------------------------------------------------------
 */
public static class CsvFormatting
{
    private static readonly char[] SpecialCharacters = { ',', '"', '\r', '\n' };

    public static string EscapeField(string value)
    {
        if (value.IndexOfAny(SpecialCharacters) >= 0)
        {
            return "\"" + value.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
        }

        return value;
    }

    /*
     * --- 2a. StringBuilder — build one CSV row from many fields ---
     *
     * StringBuilder avoids creating a new string on every field concatenation.
     * Pattern: append comma between fields; append EscapeField for each value.
     */
    public static string BuildRow(params string[] fields)
    {
        StringBuilder row = new StringBuilder();

        for (int i = 0; i < fields.Length; i++)
        {
            if (i > 0)
            {
                row.Append(',');
            }

            row.Append(EscapeField(fields[i]));
        }

        return row.ToString();
    }

    public static string BuildRow(IEnumerable<string> fields)
    {
        StringBuilder row = new StringBuilder();
        bool first = true;

        foreach (string field in fields)
        {
            if (!first)
            {
                row.Append(',');
            }

            row.Append(EscapeField(field));
            first = false;
        }

        return row.ToString();
    }
}

/*
 * =========================================================================
 * SECTION 3: MANUAL CSV READ — SPLIT VS QUOTED FIELDS
 * =========================================================================
 *
 * Simple exports (no commas inside values) work with Split:
 *
 *   string[] cols = line.Split(',', StringSplitOptions.TrimEntries);
 *
 * Limitation — Split breaks on every comma, even inside quotes:
 *
 *   "Acme, Inc",42   →  three tokens instead of two
 *
 * SplitCsvLine below implements a minimal quote-aware scan: commas inside
 * double-quoted segments are kept as part of the field. This is not a full
 * RFC 4180 parser (no multiline fields, no alternate delimiters) but covers
 * the common "company name with comma" export shape.
 *
 * Pitfall — trailing commas:
 *   "W-100,Widget,10,"  →  Split yields an extra empty trailing column.
 *   Always verify column count against the header before mapping fields.
 *
 * Pitfall — empty fields:
 *   ",Widget,10,9.99"   →  columns[0] is "" — validate required columns.
 * -------------------------------------------------------------------------
 */
public static class CsvParsing
{
    public const int InventoryColumnCount = 5;
    public const string InventoryHeader = "Sku,Name,Quantity,UnitPrice,RestockedOn";

    public static string[] SplitSimple(string line)
    {
        return line.Split(',', StringSplitOptions.TrimEntries);
    }

    public static List<string> SplitQuotedLine(string line)
    {
        List<string> fields = new List<string>();
        StringBuilder current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }

                continue;
            }

            if (c == ',' && !inQuotes)
            {
                fields.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(c);
        }

        fields.Add(current.ToString());
        return fields;
    }

    public static bool IsIgnorableLine(string line)
    {
        if (line.Length == 0)
        {
            return true;
        }

        return line[0] == '#';
    }
}

/*
 * =========================================================================
 * SECTION 4: INVENTORY CSV PARSER — TextReader, HEADER, TryParse
 * =========================================================================
 *
 * Accept TextReader so the same parser reads from StreamReader (file) or
 * StringReader (in-memory tests). COVERED IN DETAIL LATER → 02. StreamReader
 * and StreamWriter for encoding, Flush, and TextReader depth.
 *
 * Workflow per physical line:
 *   1. Skip blank lines and # comment lines
 *   2. First data line after skips → header (column names); not imported
 *   3. Split with SplitQuotedLine; verify InventoryColumnCount
 *   4. int.TryParse / decimal.TryParse with CultureInfo.InvariantCulture
 *   5. DateTime.TryParse on optional RestockedOn — blank → null
 *   6. Collect row errors with line numbers; continue loop for partial import
 * -------------------------------------------------------------------------
 */
public static class InventoryCsv
{
    public static (List<InventoryItem> Items, List<string> Errors) Import(TextReader reader)
    {
        List<InventoryItem> items = new List<InventoryItem>();
        List<string> errors = new List<string>();
        bool headerConsumed = false;
        int lineNumber = 0;

        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            lineNumber++;

            if (CsvParsing.IsIgnorableLine(line))
            {
                continue;
            }

            if (!headerConsumed)
            {
                headerConsumed = true;
                continue;
            }

            List<string> columns = CsvParsing.SplitQuotedLine(line);

            if (columns.Count != CsvParsing.InventoryColumnCount)
            {
                errors.Add(
                    $"Line {lineNumber}: expected {CsvParsing.InventoryColumnCount} columns, found {columns.Count}.");
                continue;
            }

            string sku = columns[0].Trim();
            string name = columns[1].Trim();

            if (sku.Length == 0)
            {
                errors.Add($"Line {lineNumber}: SKU is required (empty first column).");
                continue;
            }

            if (!int.TryParse(columns[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int quantity))
            {
                errors.Add($"Line {lineNumber}: invalid quantity '{columns[2]}'.");
                continue;
            }

            if (!decimal.TryParse(columns[3], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal unitPrice))
            {
                errors.Add($"Line {lineNumber}: invalid unit price '{columns[3]}'.");
                continue;
            }

            if (quantity < 0)
            {
                errors.Add($"Line {lineNumber}: quantity cannot be negative ({quantity}).");
                continue;
            }

            DateTime? restockedOn = null;
            string dateText = columns[4].Trim();

            if (dateText.Length > 0)
            {
                if (!DateTime.TryParse(dateText, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    errors.Add($"Line {lineNumber}: invalid date '{dateText}'.");
                    continue;
                }

                restockedOn = parsedDate;
            }

            items.Add(new InventoryItem
            {
                Sku = sku,
                Name = name,
                Quantity = quantity,
                UnitPrice = unitPrice,
                RestockedOn = restockedOn,
            });
        }

        return (items, errors);
    }

    /*
     * --- 4a. Export — StreamWriter + escaped rows ---
     *
     * Numbers formatted with InvariantCulture keep '.' as the decimal separator
     * regardless of the machine's regional settings.
     */
    public static void Export(string path, IEnumerable<InventoryItem> items)
    {
        UTF8Encoding utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        using (StreamWriter writer = new StreamWriter(path, append: false, utf8NoBom))
        {
            writer.WriteLine(CsvParsing.InventoryHeader);

            foreach (InventoryItem item in items)
            {
                string dateField = item.RestockedOn.HasValue
                    ? item.RestockedOn.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                    : string.Empty;

                writer.WriteLine(CsvFormatting.BuildRow(
                    item.Sku,
                    item.Name,
                    item.Quantity.ToString(CultureInfo.InvariantCulture),
                    item.UnitPrice.ToString("F2", CultureInfo.InvariantCulture),
                    dateField));
            }

            writer.Flush();
        }
    }
}

public class Program
{
    /*
     * =========================================================================
     * SECTION 5: DEMONSTRATION — IMPORT, EXPORT, AND PITFALLS
     * =========================================================================
     *
     * Creates sample CSV files under the app base directory, walks through
     * line-by-line scanning, quoted-field parsing, typed import, escaped
     * export, and common failure shapes. Path helpers → 01. File and Directory
     * Operations.
     * -------------------------------------------------------------------------
     */
    public static void Main(string[] args)
    {
        string dataFolder = Path.Combine(AppContext.BaseDirectory, "csv-demo");
        if (Directory.Exists(dataFolder))
        {
            Directory.Delete(dataFolder, recursive: true);
        }

        Directory.CreateDirectory(dataFolder);

        string cleanPath = Path.Combine(dataFolder, "inventory.csv");
        string mixedPath = Path.Combine(dataFolder, "inventory-with-errors.csv");
        string exportPath = Path.Combine(dataFolder, "inventory-export.csv");

        WriteSampleFiles(cleanPath, mixedPath);

        Console.WriteLine("=== Warehouse CSV import / export ===");
        Console.WriteLine($"Data folder: {dataFolder}");
        Console.WriteLine();

        Console.WriteLine("--- StreamReader line-by-line scan (large-file pattern) ---");
        int physicalLines = 0;
        int visibleLines = 0;

        using (StreamReader scanner = new StreamReader(cleanPath))
        {
            string? line;
            while ((line = scanner.ReadLine()) != null)
            {
                physicalLines++;

                if (!CsvParsing.IsIgnorableLine(line))
                {
                    visibleLines++;
                }
            }
        }

        Console.WriteLine($"  Physical lines: {physicalLines}; non-blank/non-comment: {visibleLines}");
        Console.WriteLine();

        Console.WriteLine("--- Simple Split vs quoted SplitCsvLine ---");
        string quotedSample = "\"Acme, Inc\",Widget,10,9.99,2026-01-15";
        string[] naive = CsvParsing.SplitSimple(quotedSample);
        List<string> quoted = CsvParsing.SplitQuotedLine(quotedSample);
        Console.WriteLine($"  Split(',') column count: {naive.Length} (wrong when name contains comma)");
        Console.WriteLine($"  SplitQuotedLine count: {quoted.Count} — first field: {quoted[0]}");
        Console.WriteLine();

        Console.WriteLine("--- Pitfall: trailing comma adds empty column ---");
        string trailingCommaRow = "W-999,Widget,5,1.00,,";
        List<string> trailingCols = CsvParsing.SplitQuotedLine(trailingCommaRow);
        Console.WriteLine(
            $"  Columns from trailing-comma row: {trailingCols.Count} (expected {CsvParsing.InventoryColumnCount}) — last field empty");
        Console.WriteLine();

        Console.WriteLine("--- Pitfall: culture affects decimal parsing ---");
        string dotDecimal = "12.99";
        string commaDecimal = "12,99";
        decimal.TryParse(dotDecimal, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal dotValue);
        bool commaInvariantOk = decimal.TryParse(
            commaDecimal, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal commaAsInvariant);
        decimal.TryParse(commaDecimal, NumberStyles.Number, new CultureInfo("de-DE"), out decimal commaGerman);
        Console.WriteLine($"  \"{dotDecimal}\" + InvariantCulture → {dotValue}");
        Console.WriteLine(
            $"  \"{commaDecimal}\" + InvariantCulture → {(commaInvariantOk ? commaAsInvariant.ToString(CultureInfo.InvariantCulture) : "fail")} (comma = thousands sep, not cents)");
        Console.WriteLine($"  \"{commaDecimal}\" + de-DE → {commaGerman}");
        Console.WriteLine();

        Console.WriteLine("--- Import clean inventory.csv ---");
        List<InventoryItem> cleanItems;
        List<string> cleanErrors;

        using (StreamReader cleanReader = new StreamReader(cleanPath))
        {
            (cleanItems, cleanErrors) = InventoryCsv.Import(cleanReader);
        }

        PrintInventory(cleanItems);
        Console.WriteLine($"  Parse errors: {cleanErrors.Count}");
        Console.WriteLine();

        Console.WriteLine("--- StringReader + same parser (TextReader polymorphism) ---");
        string inlineCsv =
            CsvParsing.InventoryHeader + Environment.NewLine +
            "T-900,Inline Part,3,4.50,2026-03-01" + Environment.NewLine;

        using (StringReader inlineReader = new StringReader(inlineCsv))
        {
            (List<InventoryItem> inlineItems, List<string> inlineErrors) = InventoryCsv.Import(inlineReader);
            Console.WriteLine($"  In-memory rows: {inlineItems.Count}; errors: {inlineErrors.Count}");
        }

        Console.WriteLine();
        Console.WriteLine("--- Export with escaped commas and quotes ---");
        InventoryCsv.Export(exportPath, cleanItems);
        Console.WriteLine($"  Wrote: {exportPath} ({new FileInfo(exportPath).Length} bytes)");
        Console.WriteLine("  First data row from export file:");

        using (StreamReader exportReader = new StreamReader(exportPath))
        {
            exportReader.ReadLine();
            Console.WriteLine($"    {exportReader.ReadLine()}");
        }

        Console.WriteLine();
        Console.WriteLine("--- Import file with malformed rows ---");

        List<InventoryItem> mixedItems;
        List<string> mixedErrors;

        using (StreamReader mixedReader = new StreamReader(mixedPath))
        {
            (mixedItems, mixedErrors) = InventoryCsv.Import(mixedReader);
        }

        Console.WriteLine($"  Accepted items: {mixedItems.Count}");
        PrintInventory(mixedItems);
        Console.WriteLine("  Row errors (import continued):");

        foreach (string error in mixedErrors)
        {
            Console.WriteLine($"    {error}");
        }

        Directory.Delete(dataFolder, recursive: true);
        Console.WriteLine();
        Console.WriteLine("Demo folder cleaned up.");
    }

    private static void WriteSampleFiles(string cleanPath, string mixedPath)
    {
        using (StreamWriter writer = new StreamWriter(cleanPath))
        {
            writer.WriteLine(CsvParsing.InventoryHeader);
            writer.WriteLine(CsvFormatting.BuildRow("W-100", "Widget A", "50", "12.99", "2026-01-10"));
            writer.WriteLine(CsvFormatting.BuildRow("W-200", "Widget B", "30", "8.50", ""));
            writer.WriteLine(CsvFormatting.BuildRow("W-300", "12\" bracket set", "10", "24.00", "2026-02-01"));
            writer.WriteLine(CsvFormatting.BuildRow("W-400", "Acme, Inc spare", "5", "19.95", "2026-02-15"));
            writer.WriteLine();
            writer.WriteLine("# end of batch");
        }

        using (StreamWriter writer = new StreamWriter(mixedPath))
        {
            writer.WriteLine(CsvParsing.InventoryHeader);
            writer.WriteLine(CsvFormatting.BuildRow("W-100", "Widget A", "50", "12.99", "2026-01-10"));
            writer.WriteLine("BAD-1,Missing columns");
            writer.WriteLine(CsvFormatting.BuildRow("BAD-2", "Gadget", "not-a-qty", "9.99", ""));
            writer.WriteLine(CsvFormatting.BuildRow("BAD-3", "Tool", "10", "not-a-price", ""));
            writer.WriteLine(CsvFormatting.BuildRow("BAD-4", "Negative", "-2", "5.00", ""));
            writer.WriteLine(CsvFormatting.BuildRow("", "Empty SKU", "5", "1.00", ""));
            writer.WriteLine(CsvFormatting.BuildRow("BAD-5", "Trailing comma row", "1", "2.00", "") + ",");
            writer.WriteLine(CsvFormatting.BuildRow("BAD-6", "Bad date", "1", "1.00", "not-a-date"));
        }
    }

    private static void PrintInventory(IEnumerable<InventoryItem> items)
    {
        foreach (InventoryItem item in items)
        {
            string restocked = item.RestockedOn.HasValue
                ? item.RestockedOn.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                : "(none)";

            Console.WriteLine(
                $"  {item.Sku,-6}  {item.Name,-18}  qty {item.Quantity,3}  @ {item.UnitPrice,6:F2}  restocked {restocked}  line {item.LineValue,8:F2}");
        }
    }
}

/*
 * =============================================================================
 * QUICK REFERENCE — CSV AND TEXT FILES
 * =============================================================================
 *
 * --- Simple split (no commas inside fields) ---
 *
 *   string[] cols = line.Split(',', StringSplitOptions.TrimEntries);
 *
 * --- Quote-aware split (minimal) ---
 *
 *   Scan char-by-char; toggle inQuotes on "; "" inside quotes → one quote char
 *
 * --- Build escaped row ---
 *
 *   StringBuilder + EscapeField per column; double internal quotes; wrap if needed
 *
 * --- Typed parsing ---
 *
 *   int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int n)
 *   decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal d)
 *   DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt)
 *
 * --- Line-by-line read (large files) ---
 *
 *   using (StreamReader reader = new StreamReader(path))
 *   {
 *       string? line;
 *       while ((line = reader.ReadLine()) != null) { … }   // null = EOF
 *   }
 *
 * --- Common mistakes ---
 *
 *  Mistake                              | Result
 *  -------------------------------------|----------------------------------
 *  Split on quoted "Acme, Inc"          | Wrong column count — use quote scan
 *  decimal.Parse without TryParse       | FormatException stops entire import
 *  Current-culture decimals in CSV      | 12,99 vs 12.99 mismatch across locales
 *  Trailing comma on row                | Extra empty column — validate count
 *  Empty required field                 | Accept "" — validate before insert
 *  ReadAllText on huge export           | High memory; no row numbers on failure
 *
 * --- Related chapters ---
 *
 *   01. File and Directory Operations    File.Exists, CreateDirectory, FileInfo
 *   02. StreamReader and StreamWriter    TextReader/TextWriter, encoding depth
 *   04. Path and Environment Classes     Combine, GetFileName, base paths
 *
 * =============================================================================
 */
