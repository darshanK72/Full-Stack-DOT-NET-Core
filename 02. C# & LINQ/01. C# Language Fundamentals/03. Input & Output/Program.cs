/*
 * =============================================================================
 * 03. INPUT & OUTPUT IN C# — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: Console input and output — printing text, reading user input,
 *        formatting numbers and currency, command-line arguments, and the
 *        standard input/output/error streams.
 *
 * WHY IT MATTERS:
 *   Most beginner programs communicate through the console. You must know how
 *   to prompt users, read what they type, format professional-looking output
 *   (tables, receipts, currency), and accept launch-time arguments from the
 *   terminal. These skills appear in every CLI tool, interview kata, and
 *   backend service log you will write.
 *
 * WHAT YOU WILL LEARN:
 *   1. Console.Write vs Console.WriteLine
 *   2. Standard input, output, and error (Console.In, Out, Error)
 *   3. Reading input: ReadLine, Read, ReadKey
 *   4. String formatting — positional, named, format specifiers
 *   5. Console colors and window title
 *   6. Parsing typed input with Parse and TryParse
 *   7. Command-line arguments (args)
 *   8. Previews: Convert class, string methods
 *
 * =============================================================================
 */

using System;
using System.Globalization;

namespace InputAndOutput;

public class Program
{
    public static void Main(string[] args)
    {
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
         * Use Write when the next output should continue on the same row — prompts,
         * progress dots, or building a line from several pieces.
         *
         * Hello World introduced WriteLine; here we combine both deliberately.
         * -------------------------------------------------------------------------
         */

        Console.Write("Customer: ");
        Console.WriteLine("Ravi");

        Console.Write("Order status: ");
        Console.Write("Packing");
        Console.Write("...");
        Console.WriteLine(" done.");


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
         * -------------------------------------------------------------------------
         */

        Console.Out.WriteLine("Payment confirmed via Console.Out.");
        Console.Error.WriteLine("Warning: loyalty points service unreachable.");


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
         *
         * This tutorial uses SIMULATED input below so dotnet run completes without
         * waiting at the keyboard. Replace the string literals with ReadLine() when
         * you run interactively:
         *
         *   Console.Write("Enter your name: ");
         *   string? name = Console.ReadLine();
         *
         * ReadKey example (blocks until a key is pressed — not called in this demo):
         *
         *   Console.WriteLine("Press any key to exit...");
         *   Console.ReadKey(intercept: true);
         * -------------------------------------------------------------------------
         */

        Console.Write("Enter your name: ");
        string simulatedNameInput = "Darshan";   // replace with Console.ReadLine() when interactive
        Console.WriteLine(simulatedNameInput);

        Console.Write("Enter item quantity: ");
        string simulatedQtyInput = "3";            // replace with Console.ReadLine() when interactive
        Console.WriteLine(simulatedQtyInput);


        /*
         * =========================================================================
         * SECTION 4: STRING FORMATTING
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
         *   {0:C2}        | Format specifier after comma — currency, 2 decimal places
         *
         *   Named values in output are clearest with string interpolation:
         *   $"Customer {customerName}, total {lineTotal:C2}"
         *
         *   Common specifiers:
         *
         *   Specifier | Example input | Example output (en-US)
         *   ----------|---------------|---------------------------
         *   C or C2   | 1234.5m       | $1,234.50
         *   N2        | 1234.5        | 1,234.50
         *   P0        | 0.18          | 18%
         *   D5        | 42            | 00042
         *   F2        | 3.14159       | 3.14
         *
         * --- 4c. String interpolation ($"...") ---
         *   $"Hello, {customerName}! Total: {total:C2}"
         *   Interpolation is concise; composite formatting is explicit and locale-aware.
         *
         * --- 4d. ToString("format") ---
         *   total.ToString("C2", CultureInfo.CurrentCulture)
         *
         * COVERED IN DETAIL LATER → 06. Strings
         *   (String methods, padding, trimming, StringBuilder)
         * -------------------------------------------------------------------------
         */

        decimal unitPrice = 249.99m;
        int quantity = 3;
        decimal lineTotal = unitPrice * quantity;
        string customerName = "Ravi";

        Console.WriteLine("Item {0} × qty {1} = {2:C2}", "Wireless Mouse", quantity, lineTotal);
        Console.WriteLine($"Customer {customerName}, total {lineTotal:C2}");
        Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "Export price: {0:F2} USD", lineTotal));


        /*
         * =========================================================================
         * SECTION 5: CONSOLE COLORS AND WINDOW TITLE
         * =========================================================================
         *
         * Console.ForegroundColor and Console.BackgroundColor use the ConsoleColor
         * enum (Red, Green, Yellow, Cyan, …). Always reset after highlighting so
         * later output is not stuck in the wrong color.
         *
         * Console.Title sets the window caption (where supported by the terminal).
         * -------------------------------------------------------------------------
         */

        Console.Title = "Express Mart — Input & Output Demo";

        ConsoleColor previousColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Low stock alert: USB-C Hub (2 left)");
        Console.ForegroundColor = previousColor;


        /*
         * =========================================================================
         * SECTION 6: PARSING INPUT — PARSE VS TRYPARSE
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
         *   if (int.TryParse(simulatedQtyInput, out int parsedQty))
         *       { use parsedQty }
         *   else
         *       { show error on Console.Error }
         *
         * COVERED IN DETAIL LATER → 05. Type Conversion and Casting
         *   (Convert class, culture-specific parsing, all numeric types)
         * -------------------------------------------------------------------------
         */

        if (int.TryParse(simulatedQtyInput, out int parsedQty))
        {
            Console.WriteLine($"Parsed quantity: {parsedQty}");
        }
        else
        {
            Console.Error.WriteLine("Invalid quantity — expected a whole number.");
        }

        string badNumericText = "twelve";
        if (!int.TryParse(badNumericText, out int _))
        {
            Console.Error.WriteLine($"Could not parse '{badNumericText}' as int.");
        }


        /*
         * =========================================================================
         * SECTION 7: COMMAND-LINE ARGUMENTS
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

        string cliCustomer = args.Length > 0 ? args[0] : simulatedNameInput;
        string cliTier = args.Length > 1 ? args[1] : "standard";
        Console.WriteLine("Launch args — customer: {0}, tier: {1}", cliCustomer, cliTier);


        /*
         * =========================================================================
         * SECTION 8: CONVERT CLASS — PREVIEW
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

        int convertedSample = Convert.ToInt32("100");
        Console.WriteLine($"Convert.ToInt32 preview: {convertedSample}");


        /*
         * =========================================================================
         * SECTION 9: INTERACTIVE MINI APP — EXPRESS MART KIOSK
         * =========================================================================
         *
         * A small store kiosk: greet the shopper, show a formatted product table,
         * then run a two-number calculator from simulated (or real) input.
         * -------------------------------------------------------------------------
         */

        RunExpressMartKiosk(simulatedNameInput, parsedQty);
    }

    private static void RunExpressMartKiosk(string customerName, int cartQty)
    {
        Console.WriteLine();
        Console.WriteLine("=== Express Mart Kiosk ===");
        Console.WriteLine("Hello, {0}! You have {1} item(s) in cart.", customerName, cartQty);

        PrintProductTable();

        /*
         * Mini calculator — two operands and an operator from simulated input.
         * Replace literals with ReadLine() for an interactive session.
         */
        string firstOperandText = "10";
        string secondOperandText = "5";
        string operatorText = "+";

        if (int.TryParse(firstOperandText, out int left)
            && int.TryParse(secondOperandText, out int right))
        {
            int result = operatorText switch
            {
                "+" => left + right,
                "-" => left - right,
                "*" => left * right,
                _ => left + right
            };

            Console.WriteLine();
            Console.Write("Calculator: ");
            Console.WriteLine("{0} {1} {2} = {3}", left, operatorText, right, result);
        }
        else
        {
            Console.Error.WriteLine("Calculator aborted — operands must be integers.");
        }
    }

    private static void PrintProductTable()
    {
        /*
         * Formatted inventory table — column headers plus aligned rows.
         * PadRight keeps names readable; :C2 formats currency.
         */
        Console.WriteLine();
        Console.WriteLine("{0,-18} {1,8} {2,10}", "Product", "Stock", "Price");
        Console.WriteLine("{0,-18} {1,8} {2,10}", new string('-', 18), "--------", "----------");

        string[] productNames = { "Wireless Mouse", "USB-C Hub", "Mechanical Keyboard" };
        int[] stockLevels = { 24, 2, 11 };
        decimal[] prices = { 799.00m, 1299.00m, 4599.00m };

        for (int i = 0; i < productNames.Length; i++)
        {
            Console.WriteLine(
                "{0,-18} {1,8} {2,10:C2}",
                productNames[i],
                stockLevels[i],
                prices[i]);
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
 *   Console.Write(text)           Same line, no newline
 *   Console.WriteLine(text)       Text + newline
 *   Console.Out                   Standard output stream
 *   Console.Error                 Standard error stream
 *
 * --- Input ---
 *
 *   Console.ReadLine()            Read one line (string?)
 *   Console.Read()                Read one character code
 *   Console.ReadKey(bool)         Read one key (optional hide echo)
 *   Console.In                    Standard input stream
 *
 * --- Formatting ---
 *
 *   {0} {1}                       Positional placeholders
 *   $"{{name}}"                   Interpolation — names from variables in scope
 *   {0:C2} {1:N2} {2:P0} {3:D4}   Currency, number, percent, decimal digits
 *   $"..."                        String interpolation shorthand
 *
 * --- Parsing ---
 *
 *   int.Parse(s)                  Throws FormatException if invalid
 *   int.TryParse(s, out int n)    Returns false if invalid — prefer for user input
 *
 * --- Command line ---
 *
 *   Main(string[] args)           args[0] is first token after program name
 *   dotnet run -- arg1 arg2       Pass tokens after --
 *
 * --- Appearance ---
 *
 *   Console.ForegroundColor       Text color (reset after use)
 *   Console.BackgroundColor       Background color
 *   Console.Title                 Window title
 *
 * --- Common runtime errors ---
 *
 *   Mistake                         | Result
 *   --------------------------------|----------------------------------
 *   Parse invalid number            | FormatException
 *   ReadLine on closed input        | null return — check before use
 *   Wrong format specifier          | FormatException at runtime
 *
 * =========================================================================
 */
