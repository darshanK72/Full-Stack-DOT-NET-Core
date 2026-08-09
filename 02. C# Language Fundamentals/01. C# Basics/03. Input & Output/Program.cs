/*
 * =============================================================================
 * 03. INPUT & OUTPUT IN C# — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: Console input and output — printing text, reading user input,
 *        formatting numbers and dates, culture-aware display, command-line
 *        arguments, and the standard input/output/error streams.
 *
 * WHY IT MATTERS:
 *   Most beginner programs communicate through the console. You must know how
 *   to prompt users, read what they type, format professional-looking output
 *   (tables, receipts, currency), and accept launch-time arguments from the
 *   terminal. These skills appear in every CLI tool, interview kata, and
 *   backend service log you will write.
 *
 * WHAT YOU WILL LEARN:
 *   1. Console.Write vs Console.WriteLine and composite-format overloads
 *   2. Standard input, output, and error (Console.In, Out, Error)
 *   3. Reading input: ReadLine, Read, ReadKey
 *   4. String formatting — positional, alignment, format specifiers
 *   5. CultureInfo — CurrentCulture, InvariantCulture, thread culture
 *   6. Number and date formatting across locales
 *   7. Custom numeric format strings and IFormattable
 *   8. Console colors, encoding, cursor, and captured output
 *   9. Parsing typed input (TryParse preview)
 *  10. Command-line arguments (args)
 *  11. Previews: Convert class, string methods
 *
 * =============================================================================
 */

using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace InputAndOutput;

/*
 * =========================================================================
 * SECTION 1: CONSOLE.WRITE VS CONSOLE.WRITELINE
 * =========================================================================
 *
 * Both methods send text to standard OUTPUT (the console window).
 *
 *   Method              | Behavior
 *   --------------------|------------------------------------------------
 *   Console.WriteLine   | prints text, then moves cursor to the next line
 *   Console.Write       | prints text, cursor stays on the same line
 *
 * Overloads accept:
 *   - A single object (calls ToString() on the value)
 *   - A composite format string plus argument list: WriteLine("{0} = {1}", a, b)
 *   - A format provider plus format string (culture-aware): WriteLine(provider, "{0:C}", price)
 *
 * Use Write when the next output should continue on the same row — prompts,
 * progress dots, or building a line from several pieces.
 *
 * Hello World introduced WriteLine; here we combine both deliberately.
 * -------------------------------------------------------------------------
 */
public static class ConsoleOutputBasics
{
    public static void DemonstrateWriteVsWriteLine()
    {
        Console.Write("Customer: ");        // Write — cursor stays on same line (prompt pattern)
        Console.WriteLine("Ravi");          // WriteLine — completes the row

        Console.Write("Order status: ");
        Console.Write("Packing");           // multiple Write calls build one line
        Console.Write("...");
        Console.WriteLine(" done.");

        decimal lineTotal = 749.97m;
        Console.WriteLine("Line total (composite): {0:C2}", lineTotal); // {0:C2} = currency, 2 decimal places
    }
}

/*
 * =========================================================================
 * SECTION 2: STANDARD INPUT, OUTPUT, AND ERROR
 * =========================================================================
 *
 * A console program talks to three text streams:
 *
 *   Stream              | Property          | Typical use
 *   --------------------|-------------------|----------------------------------
 *   Standard output     | Console.Out       | Normal program text (Write/WriteLine)
 *   Standard input      | Console.In        | Keyboard input (ReadLine, Read)
 *   Standard error      | Console.Error     | Errors and diagnostics
 *
 * WriteLine is shorthand for Console.Out.WriteLine. You can redirect streams
 * when launching from a terminal:
 *
 *   dotnet run > log.txt          → stdout goes to a file
 *   dotnet run 2> errors.txt      → stderr goes to a file
 *   dotnet run < input.txt        → stdin read from a file
 *
 * Errors should go to Console.Error so they stay visible even when stdout
 * is redirected to a file.
 *
 * --- 2a. Capturing output with SetOut ---
 *
 * Console.SetOut(TextWriter) temporarily replaces where Write/WriteLine go.
 * StringWriter collects text in memory — useful for tests and log buffers.
 * Always restore the original writer in a finally block.
 * -------------------------------------------------------------------------
 */
public static class StandardStreamsDemo
{
    public static void DemonstrateOutAndError()
    {
        Console.Out.WriteLine("Payment confirmed via Console.Out.");              // explicit stdout stream
        Console.Error.WriteLine("Warning: loyalty points service unreachable."); // stderr — visible even when stdout is redirected
    }

    public static string CaptureFormattedReceipt(string customerName, decimal total)
    {
        TextWriter originalOut = Console.Out;                                       // save real console for restore
        using StringWriter buffer = new StringWriter(CultureInfo.InvariantCulture); // in-memory TextWriter collects output

        try
        {
            Console.SetOut(buffer);  // redirect Write/WriteLine into buffer instead of terminal
            Console.WriteLine("Receipt for {0}", customerName);
            Console.WriteLine("Total: {0:C2}", total);
            return buffer.ToString(); // return captured text as one string
        }
        finally
        {
            Console.SetOut(originalOut); // always restore — even if an exception occurred
        }
    }
}

/*
 * =========================================================================
 * SECTION 3: READING INPUT — READLINE, READ, READKEY
 * =========================================================================
 *
 *   Method              | Returns              | Behavior
 *   --------------------|----------------------|-------------------------------
 *   Console.ReadLine()  | string? (one line)   | Waits for Enter; line without \r\n
 *   Console.Read()      | int (char code)      | Waits for one key press
 *   Console.ReadKey()   | ConsoleKeyInfo       | Waits for one key; optional echo
 *
 * ReadLine is the workhorse for text menus and forms. Read and ReadKey are
 * for single-character or "press any key" flows.
 *
 * ReadLine returns null when input is closed (Ctrl+Z on Windows, Ctrl+D on Linux).
 * Always null-check before using the result:
 *
 *   string? line = Console.ReadLine();
 *   if (line is null) { /* input closed * / return; }
 *
 * ReadKey(intercept: true) hides the pressed key from the screen — common for
 * passwords or "press any key to continue" without echoing the key.
 *
 * This tutorial uses SIMULATED input strings so dotnet run completes without
 * waiting at the keyboard. Replace the literals with Console.ReadLine() when
 * you run interactively.
 * -------------------------------------------------------------------------
 */
public static class ConsoleInputBasics
{
    public static string SimulateReadLine(string prompt, string simulatedInput)
    {
        Console.Write(prompt);              // show prompt (no blocking in demo mode)
        Console.WriteLine(simulatedInput);  // echo simulated answer as if user typed it
        return simulatedInput;
    }

    public static void DemonstrateReadKeyStructure()
    {
        /*
         * ReadKey blocks until a key is pressed — not called in this demo.
         * Below shows the ConsoleKeyInfo members you inspect after ReadKey:
         *
         *   ConsoleKeyInfo key = Console.ReadKey(intercept: true);
         *   key.Key          → enum (Enter, A, Escape, …)
         *   key.KeyChar      → char representation (0 for function keys)
         *   key.Modifiers    → Shift, Alt, Control flags
         */
        ConsoleKeyInfo simulatedKey = new ConsoleKeyInfo('Y', ConsoleKey.Y, shift: false, alt: false, control: false); // struct ReadKey returns
        Console.WriteLine("Simulated ReadKey — Key: {0}, Char: '{1}'", simulatedKey.Key, simulatedKey.KeyChar);
    }
}

/*
 * =========================================================================
 * SECTION 4: STRING FORMATTING — PLACEHOLDERS, ALIGNMENT, SPECIFIERS
 * =========================================================================
 *
 * Four common ways to build formatted text:
 *
 * --- 4a. String concatenation ---
 *   "Total: " + amount
 *
 * --- 4b. Composite formatting (String.Format / WriteLine with {…}) ---
 *
 *   Placeholder   | Meaning
 *   --------------|--------------------------------------------------------
 *   {0}           | First argument after the format string (zero-based index)
 *   {1}           | Second argument
 *   {0,10}        | Minimum width 10 — right-aligned by default
 *   {0,-10}       | Minimum width 10 — left-aligned (minus sign)
 *   {0:C2}        | Format specifier after comma — currency, 2 decimal places
 *
 * --- 4c. String interpolation ($"...") ---
 *   $"Customer {customerName}, total {lineTotal:C2}"
 *   Interpolation is concise; composite formatting is explicit and locale-aware.
 *
 * --- 4d. ToString("format") and ToString("format", provider) ---
 *   total.ToString("C2", CultureInfo.CurrentCulture)
 *
 * Common standard specifiers (culture affects symbols and separators):
 *
 *   Specifier | Example input | Example output (en-US)
 *   ----------|---------------|---------------------------
 *   C or C2   | 1234.5m       | $1,234.50
 *   N2        | 1234.5        | 1,234.50
 *   P0        | 0.18          | 18%
 *   D5        | 42            | 00042
 *   F2        | 3.14159       | 3.14
 *   E2        | 1234.5        | 1.23E+003
 *
 * Bad format strings throw FormatException at runtime — not a compile error.
 *
 * COVERED IN DETAIL LATER → 06. Strings
 *   (String methods, padding, trimming, StringBuilder)
 * -------------------------------------------------------------------------
 */
public static class FormattingExamples
{
    public static void DemonstrateFormatting(string customerName, int quantity, decimal unitPrice)
    {
        decimal lineTotal = unitPrice * quantity;

        Console.WriteLine("Item {0} × qty {1} = {2:C2}", "Wireless Mouse", quantity, lineTotal); // composite — positional {0},{1},{2}
        Console.WriteLine($"Customer {customerName}, total {lineTotal:C2}");                   // $"" interpolation shorthand
        Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "Export price: {0:F2} USD", lineTotal)); // fixed culture for wire/API format
    }

    public static void PrintProductTable()
    {
        Console.WriteLine();
        Console.WriteLine("{0,-18} {1,8} {2,10}", "Product", "Stock", "Price"); // {-18} left-align, {8}/{10} right-align columns
        Console.WriteLine("{0,-18} {1,8} {2,10}", new string('-', 18), "--------", "----------");

        string[] productNames = { "Wireless Mouse", "USB-C Hub", "Mechanical Keyboard" };
        int[] stockLevels = { 24, 2, 11 };
        decimal[] prices = { 799.00m, 1299.00m, 4599.00m };

        for (int i = 0; i < productNames.Length; i++)
        {
            Console.WriteLine(
                "{0,-18} {1,8} {2,10:C2}", // C2 applies currency format to price column only
                productNames[i],
                stockLevels[i],
                prices[i]);
        }
    }
}

/*
 * =========================================================================
 * SECTION 5: CULTURE AND LOCALIZATION — FULL TREATMENT
 * =========================================================================
 *
 * Numeric and date output depends on CULTURE — language/region rules for
 * separators, currency symbols, month names, and calendar.
 *
 * --- 5a. CultureInfo essentials ---
 *
 *   Member / method                    | Role
 *   -----------------------------------|----------------------------------
 *   CultureInfo.CurrentCulture         | Formatting culture (numbers, dates)
 *   CultureInfo.CurrentUICulture       | Resource culture (UI strings)
 *   CultureInfo.InvariantCulture       | Fixed English-like rules — logs, APIs
 *   CultureInfo.GetCultureInfo("de-DE")| Named culture by BCP 47 tag
 *   CultureInfo.CreateSpecificCulture  | Language + default region (e.g. "en" → en-US)
 *
 * Thread.CurrentThread.CurrentCulture and CurrentUICulture can be set for
 * the duration of a request or demo — restore the original in finally.
 *
 * --- 5b. Passing culture explicitly ---
 *
 * Every formatting API that accepts IFormatProvider uses the culture you pass:
 *
 *   value.ToString("C2", germanCulture)
 *   string.Format(culture, "{0:N2}", value)
 *   Console.WriteLine(culture, "Price: {0:C}", value)
 *
 * Omitting the provider uses CultureInfo.CurrentCulture.
 *
 * --- 5c. NumberFormatInfo and DateTimeFormatInfo ---
 *
 *   culture.NumberFormat.CurrencySymbol     → "€", "$", "₹", …
 *   culture.NumberFormat.NumberDecimalSeparator → "," or "."
 *   culture.DateTimeFormat.ShortDatePattern → "dd/MM/yyyy", "M/d/yyyy", …
 *
 * --- 5d. Custom numeric format strings ---
 *
 *   Pattern | Meaning
 *   --------|----------------------------------------------------------
 *   0       | Required digit (pads with 0)
 *   #       | Optional digit
 *   ,       | Thousands separator (when between # or 0)
 *   0.00    | Two decimal places always shown
 *
 *   1234.5.ToString("#,##0.00")  → "1,234.50" in en-US
 *
 * --- 5e. IFormattable ---
 *
 * Types like decimal, DateTime, and int implement IFormattable.ToString(format, provider).
 * Your own types can implement it for consistent culture-aware output.
 * -------------------------------------------------------------------------
 */
public static class CultureFormattingDemo
{
    public static void DemonstrateCultureComparison(decimal price, DateTime orderDate)
    {
        CultureInfo unitedStates = CultureInfo.GetCultureInfo("en-US"); // BCP 47 culture tag → formatting rules
        CultureInfo germany = CultureInfo.GetCultureInfo("de-DE");
        CultureInfo india = CultureInfo.GetCultureInfo("en-IN");

        Console.WriteLine();
        Console.WriteLine("=== Same values, different cultures ===");
        Console.WriteLine("{0,12} | {1,14} | {2,12}",
            "Culture", "Currency (C2)", "Short date");

        PrintCultureRow("en-US", unitedStates, price, orderDate);
        PrintCultureRow("de-DE", germany, price, orderDate);
        PrintCultureRow("en-IN", india, price, orderDate);
        PrintCultureRow("Invariant", CultureInfo.InvariantCulture, price, orderDate);
    }

    private static void PrintCultureRow(string label, CultureInfo culture, decimal price, DateTime orderDate)
    {
        string currencyText = price.ToString("C2", culture); // ToString(format, provider) — culture-aware
        string dateText = orderDate.ToString("d", culture);  // "d" = short date pattern for that culture
        Console.WriteLine("{0,12} | {1,14} | {2,12}", label, currencyText, dateText);
    }

    public static void DemonstrateNumberFormatInfo(CultureInfo culture)
    {
        NumberFormatInfo numberFormat = culture.NumberFormat; // separators, currency symbol, etc.
        Console.WriteLine();
        Console.WriteLine("Culture {0} — CurrencySymbol: {1}, DecimalSeparator: '{2}'",
            culture.Name,
            numberFormat.CurrencySymbol,
            numberFormat.NumberDecimalSeparator);
    }

    public static void DemonstrateCustomFormatStrings(decimal amount)
    {
        Console.WriteLine();
        Console.WriteLine("Custom formats for {0} (en-US):", amount);
        Console.WriteLine("  #,##0.00   → {0}", amount.ToString("#,##0.00", CultureInfo.GetCultureInfo("en-US")));
        Console.WriteLine("  00000.00   → {0}", amount.ToString("00000.00", CultureInfo.InvariantCulture));
        Console.WriteLine("  0.###      → {0}", amount.ToString("0.###", CultureInfo.InvariantCulture));
    }

    public static void DemonstrateThreadCultureSwitch(decimal price)
    {
        CultureInfo originalCulture = CultureInfo.CurrentCulture; // save thread default
        CultureInfo french = CultureInfo.GetCultureInfo("fr-FR");

        try
        {
            CultureInfo.CurrentCulture = french; // affects all formatting without explicit provider
            Console.WriteLine();
            Console.WriteLine("Thread culture temporarily fr-FR — price: {0:C2}", price);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture; // restore so later output is not stuck in fr-FR
        }

        Console.WriteLine("Thread culture restored — price: {0:C2}", price);
    }

    public static void DemonstrateIFormattable(decimal value, DateTime timestamp)
    {
        IFormattable formattableAmount = value;     // decimal implements IFormattable
        IFormattable formattableDate = timestamp;   // DateTime implements IFormattable

        Console.WriteLine();
        Console.WriteLine("IFormattable — amount: {0}, date: {1}",
            formattableAmount.ToString("N2", CultureInfo.InvariantCulture),
            formattableDate.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));
    }
}

/*
 * =========================================================================
 * SECTION 6: CONSOLE APPEARANCE — COLORS, TITLE, CLEAR, CURSOR
 * =========================================================================
 *
 * Console.ForegroundColor and Console.BackgroundColor use the ConsoleColor
 * enum (Red, Green, Yellow, Cyan, …). Always reset after highlighting so
 * later output is not stuck in the wrong color — Console.ResetColor() restores
 * both foreground and background defaults.
 *
 * Console.Title sets the window caption (where supported by the terminal).
 * Console.Clear clears the visible buffer (terminal-dependent).
 * Console.CursorLeft / CursorTop read position; SetCursorPosition moves it.
 *
 * KeyAvailable returns true when a key was pressed but not yet read — useful
 * for non-blocking input loops (not demonstrated here to keep output stable).
 * -------------------------------------------------------------------------
 */
public static class ConsoleAppearanceDemo
{
    public static void DemonstrateColorsAndTitle()
    {
        Console.Title = "Express Mart — Input & Output Demo"; // window caption (terminal-dependent)

        ConsoleColor previousColor = Console.ForegroundColor; // save so we can restore after highlight
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Low stock alert: USB-C Hub (2 left)");
        Console.ForegroundColor = previousColor; // restore previous foreground only

        Console.BackgroundColor = ConsoleColor.DarkBlue;
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(" Express Mart — member pricing active ");
        Console.ResetColor(); // reset both foreground and background to defaults
    }
}

/*
 * =========================================================================
 * SECTION 7: CONSOLE ENCODING
 * =========================================================================
 *
 * Console.InputEncoding and Console.OutputEncoding control how bytes map to
 * characters for stdin/stdout. Terminals often use UTF-8; legacy Windows
 * consoles may default to a code page.
 *
 * Registering CodePagesEncodingProvider enables encodings beyond UTF-8 when
 * reading legacy files or piping data from older tools:
 *
 *   Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
 *
 * For new projects, prefer UTF-8 end-to-end.
 * -------------------------------------------------------------------------
 */
public static class ConsoleEncodingDemo
{
    public static void ShowCurrentEncodings()
    {
        Console.WriteLine();
        Console.WriteLine("Output encoding: {0}", Console.OutputEncoding.WebName); // e.g. utf-8 — how bytes map to chars on stdout
        Console.WriteLine("Input encoding:  {0}", Console.InputEncoding.WebName);
    }
}

/*
 * =========================================================================
 * SECTION 8: PARSING INPUT — PARSE VS TRYPARSE (PREVIEW)
 * =========================================================================
 *
 * Console.ReadLine returns string. Numeric work needs conversion.
 *
 *   Approach              | On invalid input        | When to use
 *   ----------------------|-------------------------|----------------------------
 *   int.Parse(text)       | throws FormatException  | Trusted, validated input
 *   int.TryParse(text, out)| returns false, no throw| User typing — always prefer
 *
 * TryParse pattern:
 *
 *   if (int.TryParse(input, out int parsedQty))
 *       { use parsedQty }
 *   else
 *       { show error on Console.Error }
 *
 * TryParse overloads accept NumberStyles and IFormatProvider for culture-specific
 * input (e.g. "1.234,56" in de-DE).
 *
 * COVERED IN DETAIL LATER → 05. Type Conversion and Casting
 *   (Convert class, culture-specific parsing, all numeric types)
 * -------------------------------------------------------------------------
 */
public static class InputParsingPreview
{
    public static bool TryParseQuantity(string text, out int quantity)
    {
        return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out quantity); // no throw — returns false on failure
    }

    public static void DemonstrateParseFailure(string badNumericText)
    {
        if (!int.TryParse(badNumericText, out int _)) // discard parsed value with _ when only success/fail matters
        {
            Console.Error.WriteLine("Could not parse '{0}' as int.", badNumericText); // errors go to stderr
        }
    }
}

/*
 * =========================================================================
 * SECTION 9: COMMAND-LINE ARGUMENTS
 * =========================================================================
 *
 * Main(string[] args) receives tokens from the shell after the program name.
 *
 *   dotnet run -- Alice express
 *     args[0] = "Alice"
 *     args[1] = "express"
 *
 * args.Length is zero when no extra tokens are supplied.
 * Use args for scripts, CI pipelines, and tools that should not prompt.
 * -------------------------------------------------------------------------
 */
public static class CommandLineDemo
{
    public static void ShowLaunchArguments(string[] args, string fallbackCustomer)
    {
        string cliCustomer = args.Length > 0 ? args[0] : fallbackCustomer; // args[0] = first token after program name
        string cliTier = args.Length > 1 ? args[1] : "standard";             // default when second arg omitted
        Console.WriteLine("Launch args — customer: {0}, tier: {1}", cliCustomer, cliTier);
    }
}

/*
 * =========================================================================
 * SECTION 10: CONVERT CLASS — PREVIEW
 * =========================================================================
 *
 * The static Convert class converts between many types (string ↔ int,
 * bool, DateTime, …). It throws on failure like Parse.
 *
 *   int value = Convert.ToInt32("42");
 *
 * COVERED IN DETAIL LATER → 05. Type Conversion and Casting
 *   (Convert vs Parse vs TryParse, culture, boxing edge cases)
 * -------------------------------------------------------------------------
 */
public static class ConvertPreview
{
    public static int ToIntPreview(string text)
    {
        return Convert.ToInt32(text, CultureInfo.InvariantCulture); // throws FormatException on invalid input (like Parse)
    }
}

public class Program
{
    /*
     * =========================================================================
     * SECTION 11: DEMONSTRATION — Main orchestrates the chapter demo
     * =========================================================================
     *
     * Express Mart kiosk scenario: output basics, streams, simulated input,
     * formatting tables, culture comparisons, appearance, encoding, parsing,
     * command-line args, and a mini calculator.
     * -------------------------------------------------------------------------
     */
    public static void Main(string[] args)
    {
        ConsoleOutputBasics.DemonstrateWriteVsWriteLine(); // SECTION 1 — Write vs WriteLine

        StandardStreamsDemo.DemonstrateOutAndError(); // SECTION 2 — Out and Error streams

        string capturedReceipt = StandardStreamsDemo.CaptureFormattedReceipt("Ravi", 1499.50m); // SetOut + StringWriter capture
        Console.WriteLine();
        Console.WriteLine("Captured receipt (via SetOut + StringWriter):");
        Console.Write(capturedReceipt); // Write (not WriteLine) — receipt already ends with newlines

        string simulatedNameInput = ConsoleInputBasics.SimulateReadLine("Enter your name: ", "Darshan");     // SECTION 3 — simulated ReadLine
        string simulatedQtyInput = ConsoleInputBasics.SimulateReadLine("Enter item quantity: ", "3");
        ConsoleInputBasics.DemonstrateReadKeyStructure();

        decimal unitPrice = 249.99m;
        FormattingExamples.DemonstrateFormatting(simulatedNameInput, quantity: 3, unitPrice); // SECTION 4 — composite, interpolation, String.Format

        DateTime orderDate = new DateTime(2026, 8, 8, 14, 30, 0);
        CultureFormattingDemo.DemonstrateCultureComparison(unitPrice * 3, orderDate); // SECTION 5 — culture-aware output
        CultureFormattingDemo.DemonstrateNumberFormatInfo(CultureInfo.GetCultureInfo("de-DE"));
        CultureFormattingDemo.DemonstrateCustomFormatStrings(1234.567m);
        CultureFormattingDemo.DemonstrateThreadCultureSwitch(999.99m);
        CultureFormattingDemo.DemonstrateIFormattable(42.5m, orderDate);

        ConsoleAppearanceDemo.DemonstrateColorsAndTitle(); // SECTION 6 — colors and title

        ConsoleEncodingDemo.ShowCurrentEncodings(); // SECTION 7 — stdin/stdout encoding

        if (InputParsingPreview.TryParseQuantity(simulatedQtyInput, out int parsedQty)) // SECTION 8 — TryParse (no throw)
        {
            Console.WriteLine("Parsed quantity: {0}", parsedQty);
        }
        else
        {
            Console.Error.WriteLine("Invalid quantity — expected a whole number.");
        }

        InputParsingPreview.DemonstrateParseFailure("twelve");

        CommandLineDemo.ShowLaunchArguments(args, simulatedNameInput); // SECTION 9 — args from dotnet run --

        int convertedSample = ConvertPreview.ToIntPreview("100"); // SECTION 10 — Convert preview
        Console.WriteLine("Convert.ToInt32 preview: {0}", convertedSample);

        RunExpressMartKiosk(simulatedNameInput, parsedQty); // kiosk table + mini calculator
    }

    private static void RunExpressMartKiosk(string customerName, int cartQty)
    {
        Console.WriteLine();
        Console.WriteLine("=== Express Mart Kiosk ===");
        Console.WriteLine("Hello, {0}! You have {1} item(s) in cart.", customerName, cartQty);

        FormattingExamples.PrintProductTable();

        /*
         * Mini calculator — two operands and an operator from simulated input.
         * Replace literals with ReadLine() for an interactive session.
         */
        string firstOperandText = "10";
        string secondOperandText = "5";
        string operatorText = "+";

        if (int.TryParse(firstOperandText, out int left)       // parse both operands before computing
            && int.TryParse(secondOperandText, out int right))
        {
            int result = operatorText switch // switch expression — pick operation by operator string
            {
                "+" => left + right,
                "-" => left - right,
                "*" => left * right,
                _ => left + right // default branch for unknown operator
            };

            Console.WriteLine();
            Console.Write("Calculator: "); // Write keeps label on same line as result
            Console.WriteLine("{0} {1} {2} = {3}", left, operatorText, right, result);
        }
        else
        {
            Console.Error.WriteLine("Calculator aborted — operands must be integers.");
        }
    }
}

/*
 * =========================================================================
 * QUICK REFERENCE — CONSOLE INPUT & OUTPUT
 * =========================================================================
 *
 * --- Output ---
 *
 *   Console.Write(text)              Same line, no newline
 *   Console.WriteLine(text)          Text + newline
 *   Console.WriteLine("{0:C2}", x)   Composite format with specifier
 *   Console.Out                      Standard output stream
 *   Console.Error                    Standard error stream
 *   Console.SetOut(TextWriter)       Redirect stdout (restore after use)
 *
 * --- Input ---
 *
 *   Console.ReadLine()               Read one line (string? — null if closed)
 *   Console.Read()                   Read one character code
 *   Console.ReadKey(intercept)       Read one key (hide echo when intercept: true)
 *   Console.In                       Standard input stream
 *
 * --- Formatting ---
 *
 *   {0} {1}                          Positional placeholders
 *   {0,-10} {1,8}                    Left / right alignment width
 *   {0:C2} {1:N2} {2:P0} {3:D4}      Currency, number, percent, decimal digits
 *   $"..."                           String interpolation shorthand
 *   value.ToString("F2", culture)    Explicit culture on instance
 *
 * --- Culture ---
 *
 *   CultureInfo.CurrentCulture       Thread formatting culture
 *   CultureInfo.InvariantCulture     Fixed rules for logs and wire formats
 *   GetCultureInfo("de-DE")          Named culture
 *   culture.NumberFormat             Separators and currency symbol
 *
 * --- Parsing (preview) ---
 *
 *   int.Parse(s)                     Throws FormatException if invalid
 *   int.TryParse(s, out int n)       Returns false if invalid — prefer for user input
 *
 * --- Command line ---
 *
 *   Main(string[] args)              args[0] is first token after program name
 *   dotnet run -- arg1 arg2          Pass tokens after --
 *
 * --- Appearance & encoding ---
 *
 *   Console.ForegroundColor          Text color (ResetColor after use)
 *   Console.BackgroundColor          Background color
 *   Console.Title                    Window title
 *   Console.OutputEncoding           Character encoding for stdout
 *
 * --- Common runtime errors ---
 *
 *   Mistake                          | Result
 *   ---------------------------------|----------------------------------
 *   Parse invalid number             | FormatException
 *   ReadLine on closed input         | null return — check before use
 *   Wrong format specifier           | FormatException at runtime
 *
 * =========================================================================
 */
