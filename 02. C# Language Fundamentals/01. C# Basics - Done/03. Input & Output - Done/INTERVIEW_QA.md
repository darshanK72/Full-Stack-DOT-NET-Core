# INTERVIEW_QA — Input & Output in C#

## Foundation Questions

---

## Q1. What is the difference between Console.Write and Console.WriteLine?

**Concepts**
- Write vs WriteLine newline behavior
- cursor position after output
- composite-format overloads
- prompt pattern usage
- Console.Out backing stream

**Answer**

Console.Write sends text to standard output and leaves the cursor on the same line, while Console.WriteLine appends a platform-specific newline sequence (\r\n on Windows, \n on Unix) and moves the cursor to the next line. The practical effect is that consecutive Write calls accumulate on one row, whereas each WriteLine call starts a new row. Write is the idiomatic choice for prompts — `Console.Write("Enter name: ")` keeps the cursor adjacent to where the user will type — and for assembling a line from multiple pieces in a loop. Both methods are thin wrappers over Console.Out, which is a TextWriter backed by the process's standard output handle. Both accept the same overload family: a single object (calls ToString internally), a composite format string plus arguments (`Console.WriteLine("{0:C2}", total)`), and a format-provider variant for culture-aware output. A bare `Console.WriteLine()` with no arguments emits only the newline, which is useful for inserting blank lines in formatted tables.

---

## Q2. What are the three standard console streams and how do they map to Console class members?

**Concepts**
- standard output (Console.Out)
- standard error (Console.Error)
- standard input (Console.In)
- independent stream redirection
- OS-level piping with shell operators

**Answer**

Every console process inherits three text streams from the operating system. Standard output (fd 1) carries the program's primary output and is exposed as Console.Out; the Console.Write and Console.WriteLine methods are shortcuts that call Console.Out.Write and Console.Out.WriteLine internally. Standard error (fd 2) carries diagnostics, warnings, and error messages and is exposed as Console.Error; writing there ensures messages remain visible even when stdout is redirected to a file or pipe, because the two streams are separate handles. Standard input (fd 0) carries keyboard or piped data and is exposed as Console.In; Console.ReadLine and Console.Read are shortcuts that read from Console.In. At the shell level, operators redirect these streams independently: `dotnet run > out.txt` redirects stdout, `2> err.txt` redirects stderr, and `< data.txt` pipes a file to stdin. Understanding which stream each API targets prevents the common mistake of sending error diagnostics to stdout, which corrupts machine-readable output in pipelines.

---

## Q3. What does Console.ReadLine return, and what is the significance of its nullable return type?

**Concepts**
- string? nullable return type
- null on end-of-file (Ctrl+Z / Ctrl+D)
- empty string vs null distinction
- null-check requirement under nullable enable
- stdin closed in CI and piped contexts

**Answer**

Console.ReadLine returns `string?` — a nullable string. It returns a non-null string containing the line the user typed (without the newline terminator) when input is available, an empty string when the user presses Enter without typing, and null when the input stream is closed. The stream closes when a user presses Ctrl+Z on Windows or Ctrl+D on Linux/macOS, or when the process's stdin is connected to a pipe or file that has been fully consumed. The nullable annotation matters because in many production contexts — CI pipelines, Kubernetes jobs, and automated test harnesses — stdin is never an interactive terminal. Code that blindly assigns the result to a non-nullable `string` or calls `.Trim()` without a null-check will throw a NullReferenceException the moment input is exhausted. The correct pattern is to assign to `string?` and branch on null before any further use: `string? line = Console.ReadLine(); if (line is null) return;`. The compiler's nullable analysis enforces this under `<Nullable>enable</Nullable>`, which is the default in all new .NET 10 projects.

---

## Q4. How do Console.Read and Console.ReadKey differ from Console.ReadLine?

**Concepts**
- Console.Read returns char code as int
- Console.ReadKey returns ConsoleKeyInfo struct
- Console.ReadLine returns full line as string?
- blocking behavior of each method
- use cases for single-key, single-char, and full-line input

**Answer**

Console.ReadLine waits for the Enter key and returns the entire line as a string, making it the standard method for collecting text such as names, search terms, or numeric strings. Console.Read waits for the next character to be available in the input buffer and returns its Unicode code point as an int, returning -1 at end-of-stream. It reads only one character at a time but still buffers input in line mode, so the character is not delivered until Enter is pressed in most terminal configurations. Console.ReadKey is the lowest-level of the three: it captures exactly one keystroke as a ConsoleKeyInfo struct, which exposes the Key (a ConsoleKey enum value), the KeyChar (the char for printable keys, '\0' for function keys), and the Modifiers flags (Shift, Alt, Control). ReadKey operates in raw mode without waiting for Enter, making it the right choice for "press any key to continue" flows, single-character menu navigation, and detecting special keys like Escape or arrow keys. Unlike ReadLine, ReadKey is not well-suited for piped non-interactive input because it can block or behave unpredictably when stdin is not a terminal.

---

## Q5. What does the intercept parameter of Console.ReadKey control, and when should you use it?

**Concepts**
- intercept: true suppresses terminal echo
- password collection pattern
- ConsoleKey enum for named keys
- KeyChar for printable characters
- ConsoleModifiers flags (Shift, Alt, Control)

**Answer**

Console.ReadKey accepts an optional bool parameter named `intercept`. When `intercept` is false (the default), the pressed key is echoed to the terminal just as if the user had typed it normally. When `intercept` is true, the keypress is captured silently — nothing appears on screen. The most common use of `intercept: true` is password collection: a loop calls `ReadKey(intercept: true)`, appends the KeyChar to a StringBuilder, and stops when the user presses Enter, all without echoing any characters. The returned ConsoleKeyInfo struct carries three members: Key is a ConsoleKey enum value covering every named key on a standard keyboard including Escape, Enter, arrow keys, and function keys; KeyChar is the printable character the key produces (or '\0' for non-printing keys); and Modifiers is a ConsoleModifiers flags enum reporting whether Shift, Alt, or Ctrl was held. Without intercept suppression, a password prompt would leak each character typed to anyone viewing the screen, which is a security concern in shared-terminal and recorded-session environments.

---

## Q6. How does composite formatting work in Console.WriteLine and String.Format?

**Concepts**
- positional placeholders {index}
- format specifier after colon {0:C2}
- alignment specifier {0,width} and {0,-width}
- FormatException on invalid specifier at runtime
- IFormatProvider overload for culture control

**Answer**

Composite formatting is the mechanism behind `Console.WriteLine("{0} × {1} = {2:C2}", name, qty, total)` and the equivalent `String.Format` call. A format string contains zero or more format items in curly braces, each with the structure `{index[,alignment][:formatSpecifier]}`. The index is a zero-based integer selecting the argument following the format string — `{0}` picks the first argument, `{1}` the second. The optional alignment field, separated from the index by a comma, specifies a minimum column width: a positive number right-aligns the value (padding with spaces on the left), and a negative number left-aligns it (`{0,-20}` produces a 20-character left-aligned field). The optional format specifier, separated by a colon, is passed to the value's `ToString(string, IFormatProvider)` implementation — standard specifiers like C2 (currency), N2 (number), D4 (zero-padded integer), P0 (percent), and F2 (fixed-point) are common choices. An invalid format specifier causes a FormatException at runtime, not at compile time, because the specifier string is not compiler-validated. Console.WriteLine and String.Format both have overloads that accept an `IFormatProvider` as the first argument, which controls the culture used for numeric and date formatting.

---

## Q7. What is string interpolation ($"...") and when should you prefer composite formatting instead?

**Concepts**
- $"" interpolated string syntax
- embedded expressions with inline format specifiers
- compiler transformation to string.Format or DefaultInterpolatedStringHandler
- implicit CurrentCulture dependency
- FormattableString.Invariant for culture-safe wire formats

**Answer**

String interpolation, introduced in C# 6, lets you embed expressions directly inside a string literal by prefixing it with a dollar sign: `$"Hello {customerName}, total {lineTotal:C2}"`. The compiler transforms each interpolated string into a call to `string.Format` (or in modern C#, to `DefaultInterpolatedStringHandler` for better performance). Inline format specifiers and alignment work identically to composite formatting — `{lineTotal:C2}` applies the currency specifier, and `{name,-20}` left-aligns in 20 characters. String interpolation is generally preferred for ad hoc, developer-facing format strings because the variable names inside `{}` are far more readable than positional `{0}`. Composite formatting is preferable in two situations: when the format string is stored in a resource file for localization (positional indexes survive translation, variable names do not), and when explicit culture control is needed. Interpolated strings implicitly use `CultureInfo.CurrentCulture`, which can produce commas as decimal separators on machines configured with European locales. To force InvariantCulture, assign the interpolated string to a `FormattableString` and call `FormattableString.Invariant(...)`: `FormattableString fs = $"{price:F2}"; string wireText = FormattableString.Invariant(fs);`. This distinction matters whenever the output crosses a system boundary as a wire format.

---

## Q8. What are the most commonly used standard numeric format specifiers and what does each produce?

**Concepts**
- C (currency) with culture-dependent symbol
- N (number with thousands separator)
- F (fixed-point decimal places)
- D (zero-padded integer)
- P (percentage multiplied by 100)
- E (scientific notation)

**Answer**

Standard numeric format specifiers are single-letter codes passed to `ToString` or embedded in format items. All optionally accept a precision digit — C2, N4, F0 — controlling the number of fractional digits shown. C formats a value as currency, prepending or surrounding it with the culture's currency symbol and applying culture-specific thousands and decimal separators; `(1234.5m).ToString("C2")` yields `"$1,234.50"` in en-US and `"1.234,50 €"` in de-DE. N formats a number with thousands separators and a configurable decimal count without a currency symbol — useful for quantities and measurements. F formats a value to a fixed number of decimal places without thousands separators. D is valid only for integer types and zero-pads to the specified width: `(42).ToString("D5")` yields `"00042"`, making it useful for codes and identifiers. P multiplies the value by 100 and appends a percent sign; `(0.185m).ToString("P0")` yields `"19%"` (rounded). E formats in scientific notation: `(12345.6).ToString("E2")` yields `"1.23E+004"`. Passing an unrecognized specifier to ToString throws a FormatException at runtime, so when composing specifiers dynamically, always validate the specifier before use.

---

## Q9. What are standard date and time format specifiers, and how does culture affect their output?

**Concepts**
- short date "d" and long date "D" patterns
- short time "t" and long time "T" patterns
- round-trip "O" and sortable "s" specifiers
- CultureInfo effect via DateTimeFormatInfo
- custom tokens (yyyy, MM, dd, HH, mm, ss)

**Answer**

Standard date and time format specifiers are single-letter codes passed to `DateTime.ToString`. The "d" (short date) and "D" (long date) specifiers produce the date in the culture's conventional short and long patterns; "d" yields "8/8/2026" in en-US but "08.08.2026" in de-DE. "t" and "T" produce short and long time representations respectively, also culture-dependent. These specifiers pull their patterns from `DateTimeFormatInfo`, which is part of every `CultureInfo`; passing a different culture to `ToString("d", culture)` changes the pattern used. For data exchange, culture-independent specifiers are essential: "s" produces the ISO 8601 sortable format "2026-08-08T14:30:00" using InvariantCulture-compatible separators, and "O" (round-trip) produces "2026-08-08T14:30:00.0000000" with full fidelity including fractional seconds and timezone designator. Custom format strings combine tokens like `yyyy` (4-digit year), `MM` (2-digit month), `dd` (2-digit day), `HH` (24-hour hour), `mm` (minute), and `ss` (second). A single-character custom format string must be escaped as `%d` to avoid being interpreted as a standard specifier. Always use "O" or "yyyy-MM-ddTHH:mm:ssZ" for timestamps stored in databases or transmitted in JSON, and reserve locale-dependent specifiers for display-only strings.

---

## Q10. What is the difference between int.Parse and int.TryParse when handling user input?

**Concepts**
- Parse throws FormatException on invalid input
- TryParse returns bool, never throws
- out parameter pattern for the result
- NumberStyles and IFormatProvider overloads
- performance advantage of avoiding exceptions

**Answer**

int.Parse converts a string to an int and throws FormatException if the string is not a valid integer, or OverflowException if the value exceeds int range. Because it throws on failure, it is appropriate only when the caller can guarantee valid input — for example, reading a value from a validated configuration file or a database column constrained to integers. For user-typed input, Parse is unsuitable because ordinary typing mistakes (trailing spaces, letters, punctuation) cause uncaught exceptions if the call is not wrapped in try/catch. int.TryParse follows the Try-Parse pattern: it returns true when parsing succeeds and writes the result to an `out int` parameter, or returns false when parsing fails — never throwing. This lets validation and conversion happen in a single expression: `if (int.TryParse(input, out int qty))`. Both methods share overloads that accept NumberStyles flags (allowing or disallowing thousands separators, leading sign, etc.) and an IFormatProvider for culture-specific parsing, such as recognizing `"1.234"` as 1234 in de-DE. In tight loops, avoiding exception overhead is measurable; TryParse is preferred not just for correctness but also for throughput. The same pattern exists for every numeric type: `decimal.TryParse`, `double.TryParse`, `DateTime.TryParse`, and so on.

---

## Q11. How does the Convert class differ from Parse and TryParse for type conversion?

**Concepts**
- Convert.ToInt32, Convert.ToDecimal, Convert.ToBoolean
- null input returns default (0 for int) instead of throwing
- no Try variant — always throws on failure
- cross-type conversions (int to decimal, bool to int)
- DataRow and object-typed sources

**Answer**

The static `Convert` class provides conversion methods such as `Convert.ToInt32`, `Convert.ToDecimal`, and `Convert.ToBoolean` that convert between many base types, not just from strings. Unlike `int.Parse`, `Convert.ToInt32(null)` returns 0 instead of throwing a NullReferenceException, which can be convenient or dangerously misleading depending on context — a missing value silently becomes zero. When passed an invalid string, `Convert.ToInt32("abc")` throws FormatException just as `int.Parse` would; the Convert class does not offer a non-throwing Try variant, which is its main limitation compared to the TryParse pattern. `Convert.ToInt32(true)` returns 1, and `Convert.ToInt32(3.7)` returns 4 (rounds to nearest), behaviors that have no equivalent in Parse. The culture-aware overloads of Convert accept `IFormatProvider` but cover fewer edge cases than `int.TryParse(s, NumberStyles, IFormatProvider, out int)`. In practice, TryParse is the first choice for any string-to-numeric conversion that could fail; Convert is useful when converting between numeric primitives (int to decimal, double to int with rounding) or when processing data from a `DataRow` where values are typed as `object` and may be null.

---

## Q12. What is Console.SetOut, and how is it used to capture output for testing?

**Concepts**
- Console.SetOut(TextWriter) process-wide redirect
- StringWriter for in-memory capture
- save-and-restore the original writer
- finally block to guarantee cleanup
- TextWriter.ToString() to read captured text

**Answer**

Console.SetOut replaces the process-wide standard output TextWriter with any TextWriter the caller provides. The change is global — every subsequent call to Console.Write or Console.WriteLine writes to the new writer until SetOut is called again. The canonical use in tests is to pass a `StringWriter`: the test redirects output, exercises the code under test, then reads `stringWriter.ToString()` to assert the exact lines printed. The TextWriter that was active before the call must be saved and unconditionally restored in a `finally` block so that a test failure or exception cannot leave the process in a state where all console output silently disappears. A complete pattern: `TextWriter original = Console.Out; using var sw = new StringWriter(); try { Console.SetOut(sw); CodeUnderTest(); Assert.Contains("expected text", sw.ToString()); } finally { Console.SetOut(original); }`. StringWriter's constructor can accept a CultureInfo that governs how values are formatted into the buffer, which is relevant when the code under test uses composite formatting with numeric values. An analogous method, `Console.SetIn(TextReader)`, redirects standard input; together they allow a console application to be driven entirely from in-memory strings in a unit test.

---

## Q13. What is Console.SetIn, and how does it enable automated testing of interactive prompts?

**Concepts**
- Console.SetIn(TextReader) process-wide redirect
- StringReader for pre-scripted input lines
- save-and-restore the original reader
- simulating EOF with an exhausted StringReader
- test isolation requirements

**Answer**

Console.SetIn replaces the process-wide standard input TextReader with any TextReader the caller provides. Like SetOut, the replacement is global and must be restored in a finally block. In tests, a `StringReader` initialized with a multi-line string represents pre-scripted user input: `Console.SetIn(new StringReader("Alice\n3\n+\n"))` causes three successive ReadLine calls inside the code under test to receive "Alice", "3", and "+" respectively, as if the user had typed them. When a StringReader is exhausted, further ReadLine calls return null — this is how EOF is simulated, which is important for testing loops that must stop when input ends. Using SetIn removes the need for mock objects or dependency injection into Console-calling code, keeping simple programs testable without restructuring. For more complex scenarios where input depends on prior output, a Pipe-backed stream or a TextReader subclass may be cleaner than StringReader. In production code that must be decoupled from Console long-term, passing TextReader and TextWriter as constructor arguments is the cleaner design, but for sequential test suites the SetIn/SetOut pattern is a low-cost and effective technique.

---

## Q14. What is StreamReader/StreamWriter, and how do they relate to Console I/O?

**Concepts**
- StreamReader wraps a Stream for buffered text reading
- StreamWriter wraps a Stream for buffered text writing
- TextReader / TextWriter base class hierarchy
- Console.In as TextReader, Console.Out as TextWriter
- using declaration for deterministic Dispose and flush

**Answer**

StreamReader and StreamWriter are concrete classes in the TextReader and TextWriter hierarchy respectively, layering character encoding and buffering on top of a raw Stream. Console.In is a TextReader backed by the process's stdin byte stream; Console.Out and Console.Error are TextWriters backed by stdout and stderr respectively. When code calls `Console.ReadLine()`, it is effectively calling `Console.In.ReadLine()` — StreamReader adds no new public surface area beyond TextReader; it is simply the implementation that can be constructed around any Stream. Because Console.In is a TextReader, any TextReader — including a StringReader or a StreamReader wrapping a MemoryStream — is a valid replacement via `Console.SetIn`. When reading files in production code, `using var reader = new StreamReader("data.txt")` creates a StreamReader with UTF-8 encoding by default. `using var writer = new StreamWriter("out.csv")` creates a StreamWriter that writes text to a file. Both should be wrapped in `using` declarations so that Dispose is called and the underlying file handle is flushed and released even if an exception occurs mid-write.

---

## Q15. How do custom numeric format strings differ from standard format specifiers?

**Concepts**
- 0 required-digit placeholder (pads with zero)
- # optional-digit placeholder (suppresses leading zeros)
- comma for thousands grouping
- period for decimal-separator position
- culture-sensitive separators still applied to custom patterns

**Answer**

Standard format specifiers are single letters activating predefined behaviors — C for currency, F for fixed-point, N for number. Custom numeric format strings are patterns built from special characters describing the output digit by digit. The zero `0` is a required digit placeholder: wherever a `0` appears, a digit is always printed, padding with zero if necessary — `(42).ToString("00000")` yields "00042". The hash `#` is an optional digit placeholder: it prints a digit only if the value has one in that position, suppressing leading zeros. The comma `,` between digit placeholders activates thousands grouping; a comma placed to the left of all digit placeholders outside the integer portion is a scaling marker that divides the value by 1000 per comma. The period `.` marks where the decimal separator should appear. The separators actually emitted are still drawn from the culture's NumberFormatInfo — `"#,##0.00"` applied to `1234.5m` in de-DE produces "1.234,50" because the culture substitutes a period for the thousands separator and a comma for the decimal separator. Custom patterns are most useful for fixed-width codes, account numbers, and output shapes that the standard specifiers cannot produce directly.

---

## Gotchas

---

## Q16. What bug does using the null-forgiving operator (!) on Console.ReadLine introduce?

**Concepts**
- null-forgiving operator (!) as a compiler-only annotation
- NullReferenceException when stdin is closed
- CI pipelines and piped file exhaustion
- false satisfaction of nullable compiler analysis
- correct null-check pattern with is null

**Answer**

The null-forgiving operator `!` tells the C# compiler's nullable flow analysis to treat an expression as non-null, suppressing the CS8600/CS8602 warning. Writing `string input = Console.ReadLine()!` compiles cleanly under `<Nullable>enable</Nullable>` — but it is a lie to the compiler, not a fix. At runtime, if the process's stdin is closed or fully consumed — which happens routinely in CI pipelines, Kubernetes jobs, `dotnet run < file.txt` invocations, and test harnesses — Console.ReadLine returns null. The `!` adds no null-check; it only prevents the compiler from complaining. The next line that calls `.Trim()` or `.Length` on the result throws a NullReferenceException, typically deep in a pipeline with no console to display the error. The correct fix is to assign to `string?` and explicitly branch: `string? line = Console.ReadLine(); if (line is null) { return; }`. In a loop, `while ((string? line = Console.ReadLine()) is not null)` is idiomatic — it reads, assigns, null-checks, and enters the body only when a real line is available, terminating automatically when EOF is reached.

---

## Q17. Why does int.Parse throw even when the string looks like a valid number?

**Concepts**
- FormatException on unexpected characters (comma, symbol, space)
- OverflowException for out-of-range values
- culture-sensitive thousands separator ambiguity
- NumberStyles flags required for non-default input shapes
- TryParse as the silent-failure alternative

**Answer**

int.Parse is strict about what constitutes a valid integer string. Several common situations cause it to throw despite the string appearing numeric. Strings like `"1,234"` throw FormatException in en-US culture unless `NumberStyles.AllowThousands` is explicitly passed, because the default `NumberStyles.Integer` does not permit group separators. Strings with currency symbols or percent signs (`"$42"`) throw even though they clearly represent a numeric value, because int.Parse parses raw integers, not monetary expressions. Strings with fractional parts (`"3.0"`) throw FormatException even though the numeric value is an integer. A string valid in one culture may fail in another: `"1.234"` parses as 1234 in de-DE (where period is the thousands separator) but throws in en-US (where period is the decimal point). Values that exceed the int range throw OverflowException rather than FormatException. All of these cases are handled silently by int.TryParse, which returns false instead of throwing. When accepting input from users in locales that use non-ASCII separators, pass both `NumberStyles.Any` and the appropriate CultureInfo to TryParse rather than pre-sanitizing the string with Replace.

---

## Q18. What happens if Console.SetOut is called without saving and restoring the original TextWriter?

**Concepts**
- process-wide permanent Console.Out replacement
- output silently lost to a forgotten StringWriter
- finally block omission
- disposed StringWriter causing ObjectDisposedException
- diagnostic difficulty from silent failure

**Answer**

Console.SetOut is a process-wide mutation: after it is called, every Console.Write and Console.WriteLine — in any thread, in any class — writes to the new TextWriter until SetOut is called again. If a method redirects output to a StringWriter, reads the buffer, and returns without calling `Console.SetOut(originalOut)`, the StringWriter continues to be the destination for all subsequent output. The StringWriter's contents are never read again, and the user or terminal receives nothing. The failure mode is insidious because the code runs without exception — output is not lost to an error; it silently accumulates in a forgotten buffer. In a test suite, one leaked SetOut call can cause every subsequent test's output to disappear, making all assertion failures on printed text appear to fail because the captured string is empty. When the StringWriter was created inside a `using` block, the problem compounds: if disposal happens before SetOut is restored, any subsequent Console.Write throws ObjectDisposedException. The fix is a straightforward try/finally: save `Console.Out` before the redirect and call `Console.SetOut(saved)` in the finally block. A reusable IDisposable wrapper that restores the original writer on Dispose is a cleaner encapsulation for repeated use.

---

## Q19. Why does string interpolation $"" produce incorrect wire-format output on machines with non-en-US culture?

**Concepts**
- $"" uses CurrentCulture implicitly for all formatting
- decimal separator varies by locale (period vs comma)
- JSON and CSV corruption at service boundaries
- FormattableString.Invariant workaround
- InvariantCulture required for all wire formats

**Answer**

String interpolation is syntactic sugar over a call that uses `CultureInfo.CurrentCulture` for all formatting. On a machine or container where the thread culture is de-DE, fr-FR, or any culture that uses a comma as the decimal separator, `$"{price:F2}"` with `price = 1234.5m` produces `"1234,50"` instead of `"1234.50"`. When this string is placed into JSON (`$"{{\"total\":{price:F2}}}"`) or into a CSV row, the resulting file is syntactically invalid for any downstream parser that expects a period as the decimal separator, causing silent data corruption or parse failures in production. The bug is reliably reproducible only on affected machines, so it often escapes development (typically en-US) and surfaces only in European deployments or in Docker images that inherit a locale from the base image. The fix for wire formats is to assign the interpolated string to `FormattableString` and pass it through `FormattableString.Invariant`: `FormattableString fs = $"{price:F2}"; string wireValue = FormattableString.Invariant(fs);`. Alternatively, call `price.ToString("F2", CultureInfo.InvariantCulture)` explicitly. Reserve plain interpolation for display strings that should honor the user's locale.

---

## Q20. What goes wrong when diagnostic messages are written to Console.Out instead of Console.Error?

**Concepts**
- stdout reserved for structured program data
- stderr reserved for diagnostics and error messages
- independently redirectable stream handles
- CSV and JSON corruption by interleaved diagnostic lines
- pipeable tool design convention

**Answer**

Sending diagnostic messages — progress updates, warnings, error descriptions — to Console.Out via Console.WriteLine pollutes the program's data stream. This is benign when a human reads the terminal, but breaks every machine-driven workflow. When an operator pipes output to a file or to another program (`dotnet run > results.csv`), diagnostic lines appear in the CSV alongside data rows, making the file unparseable. Because stdout and stderr are independent handles, stderr can be redirected separately: `dotnet run > results.csv 2> errors.log` sends only data rows to the file and only diagnostics to the log. Code that uses Console.WriteLine for both data and diagnostics cannot be separately redirected at the OS level, removing this option entirely. The rule is: write to Console.Out only structured data that downstream consumers are expected to receive — CSV rows, JSON, plain numbers; write everything else — progress bars, verbose logging, warnings, validation messages, usage text — to Console.Error. This convention is the foundation of Unix pipeline design and applies equally to .NET console tools, CI jobs, and containerized workers. When adopting a structured logging framework such as Serilog or Microsoft.Extensions.Logging, configure it to write to stderr by default and let the data pipeline own stdout exclusively.

---

## Real-World Scenarios

---

## Q21. Code review: A CI job reads quantity from stdin — identify the defects and prioritize fixes.

**Concepts**
- null return from Console.ReadLine in closed-stdin context
- null-forgiving operator misuse
- int.Parse on unvalidated user input
- Console.Error for diagnostics
- stdout contamination by debug output

```csharp
// Runs in CI: dotnet PricingJob.dll < orders.txt
public static void ProcessOrder(decimal unitPrice)
{
    Console.Write("Enter quantity: ");
    string input = Console.ReadLine()!;
    int qty = int.Parse(input.Trim());
    decimal total = unitPrice * qty;
    Console.WriteLine("Total: {0:C2}", total);
    Console.WriteLine($"DEBUG: processing qty={qty}");
}
```

| Category | Problem | Impact |
|---|---|---|
| Null safety | `ReadLine()!` suppresses the null warning; returns null when stdin is exhausted | NullReferenceException on `input.Trim()` for every piped or headless run |
| Exception handling | `int.Parse` throws FormatException on any non-integer line | Uncaught exception terminates the CI job with a non-zero exit code |
| Stream misuse | `Console.WriteLine($"DEBUG: ...")` writes to stdout | Debug line appears in any `> output.txt` redirect, corrupting downstream consumers |
| Prompt placement | `Console.Write("Enter quantity: ")` prompt goes to stdout | Prompt text appears in piped output; pollutes any structured data stream |

**Fix priority:**
1. Replace `ReadLine()!` with `string? input = Console.ReadLine(); if (input is null) return;`
2. Replace `int.Parse` with `if (!int.TryParse(input.Trim(), out int qty)) { Console.Error.WriteLine("Invalid quantity"); return; }`
3. Move the DEBUG line to `Console.Error.WriteLine` so it never appears in redirected stdout
4. Move the prompt to `Console.Error.Write` to cleanly separate user-facing prompts from data output

---

## Q22. Code review: A stream-redirect receipt capture is missing cleanup — diagnose and fix.

**Concepts**
- using disposal before Console.SetOut restore
- Console.Out permanently replaced after method returns
- ObjectDisposedException on subsequent writes
- finally block required for SetOut cleanup
- correct lifecycle ordering of SetOut and StringWriter

```csharp
public static string CaptureReceipt(string customer, decimal total)
{
    TextWriter original = Console.Out;
    using StringWriter buffer =
        new StringWriter(CultureInfo.InvariantCulture);

    Console.SetOut(buffer);
    Console.WriteLine("Receipt for {0}", customer);
    Console.WriteLine("Total: {0:C2}", total);

    // using disposes buffer at the closing brace —
    // Console.SetOut(original) is never called
    return buffer.ToString();
}
```

| Category | Problem | Impact |
|---|---|---|
| Execution order | `using` disposes `buffer` at the closing brace; `Console.SetOut(original)` is never called | Console.Out remains pointed at a disposed writer for the rest of the process |
| ObjectDisposedException | Writing to a disposed StringWriter throws | Any Console.Write or Console.WriteLine after the first call crashes the process |
| Missing restore | No `finally` block to guarantee restoration on exception | An exception inside the capture block leaves Console.Out permanently redirected |
| Using scope conflict | `using` is correct for memory cleanup but competes with the SetOut lifetime | The TextWriter must remain valid until after SetOut(original) is called |

**Fix priority:**
1. Restructure with explicit `try/finally`: move `Console.SetOut(original)` into the finally block
2. Move `return buffer.ToString()` inside the try block, before the finally executes
3. Remove the `using` declaration and call `buffer.Dispose()` explicitly after restoring the original writer so the lifecycle is clear and correct

---

## Q23. A console application reads lines from stdin in a loop and must handle both interactive users and piped files gracefully. How do you structure the read loop?

**Concepts**
- while-loop with inline null check on ReadLine
- automatic loop exit at EOF
- Console.Error for prompts and diagnostics in piped contexts
- TryParse for per-line numeric conversion
- graceful termination vs abrupt exception

**Answer**

When a console application may receive input from either an interactive user or a piped file, the read loop must handle three cases: a line with valid data, a line with invalid data, and end-of-stream (null from ReadLine). The idiomatic structure in C# 10+ is `while ((string? line = Console.ReadLine()) is not null)`, which reads a line, assigns it, and enters the body only when the result is non-null; the loop exits automatically when stdin is exhausted. Inside the loop, use TryParse for any numeric conversion — `int.TryParse(line.Trim(), out int value)` — and on parse failure write a diagnostic to Console.Error to keep the data stream on stdout clean. If the application is interactive, prompts should go to Console.Error when the tool is intended to be pipeable: `Console.Error.Write("Enter quantity: ")` ensures the prompt appears on the terminal even when stdout is redirected. In a piped batch context there is typically no need for prompts at all, so the loop can simply read, parse, compute, write results to stdout, and send errors to stderr. This design makes the tool composable without any code changes: `cat orders.txt | dotnet PricingTool.dll > totals.csv 2> errors.log` works correctly, and the same binary works correctly when run interactively. The loop structure that handles null is also the correct structure for testing via Console.SetIn with a StringReader, because a StringReader returns null once exhausted.

---

## Q24. How would you build a console password prompt that collects a masked password without echoing characters?

**Concepts**
- Console.ReadKey(intercept: true) for silent capture
- ConsoleKey.Enter for loop termination
- ConsoleKey.Backspace for editing support
- StringBuilder accumulation of KeyChar values
- Console.IsInputRedirected guard for non-interactive contexts

**Answer**

Building a masked password prompt requires reading characters one at a time using `Console.ReadKey(intercept: true)` so that each keystroke is captured without appearing on screen. The loop accumulates characters in a StringBuilder, appending each printable character, and handles Backspace by removing the last character from the accumulator and erasing the placeholder asterisk from the screen using `Console.Write("\b \b")` — a backspace, a space to overwrite, and another backspace to reposition the cursor. The loop terminates when the user presses Enter, detected by `key.Key == ConsoleKey.Enter`. A complete skeleton: check `key.Key` for Enter to break, for Backspace to conditionally remove the last character, and for any other key to append `key.KeyChar` to the builder and optionally write `*` to show input progress without revealing the character. Care must be taken with modifier keys (Ctrl, Alt) and function keys that have no printable KeyChar — these should be ignored rather than appended. In production, this pattern is often extracted into a helper method `static string ReadMaskedLine(string prompt)`. This approach does not work when stdin has been redirected to a non-interactive stream; `Console.ReadKey` can throw `InvalidOperationException` in that case. The calling code should check `Console.IsInputRedirected` and fall back to `Console.ReadLine()` for non-terminal contexts, accepting that masking is unavailable in piped scenarios.

---

## Q25. A pricing service must display currency in the operator's locale but emit a fixed-format decimal for a JSON API. How do you separate these two formatting concerns?

**Concepts**
- CultureInfo.CurrentCulture for display output
- CultureInfo.InvariantCulture for all wire formats
- ToString with explicit IFormatProvider at service boundaries
- FormattableString.Invariant for interpolated wire strings
- display vs wire adapter methods

**Answer**

The display layer and the wire layer have opposite culture requirements and must never share a format call. For display, use the operator's CurrentCulture implicitly or explicitly: `price.ToString("C2", CultureInfo.CurrentCulture)` produces `"₹1,23,456.00"` in en-IN or `"1.234,50 €"` in de-DE, which is the natural representation for those operators. For any value that crosses a system boundary — a JSON payload, a CSV export, a database write, an HTTP query string — always pass InvariantCulture explicitly: `price.ToString("F2", CultureInfo.InvariantCulture)` guarantees a period decimal separator regardless of the server's locale. String interpolation must be treated with equal care: `$"{price:F2}"` silently inherits CurrentCulture and produces comma-separated decimals on European servers. To force invariant formatting through the interpolation syntax, assign to `FormattableString` first: `FormattableString fs = $"{price:F2}"; string wireValue = FormattableString.Invariant(fs);`. A practical discipline is to define two thin adapter methods — `ToWireString(decimal value)` that always passes InvariantCulture, and `ToDisplayString(decimal value, CultureInfo culture)` for UI output — and to enforce via code review that no formatting call omits its culture argument at service boundaries. This separation prevents the class of bugs where a correctly-coded European deployment intermittently corrupts JSON sent to a US-based API, a failure that is reliably hard to reproduce in development.

---

## Q26. A developer wants to unit-test a method that reads from Console and writes formatted output. How do you design the test without refactoring the production code?

**Concepts**
- Console.SetIn with StringReader for scripted input
- Console.SetOut with StringWriter to capture output
- save-and-restore both streams in finally blocks
- asserting exact output content after the call
- test isolation when suite runs sequentially

**Answer**

Testing code that directly calls Console.ReadLine and Console.WriteLine does not require refactoring into an abstraction; the Console streams can be swapped at the process level using SetIn and SetOut. Before each test, save the current Console.In and Console.Out, replace them with a StringReader loaded with simulated input and a StringWriter to capture output, exercise the method under test, and then assert on the StringWriter's content. A minimal pattern: `TextReader savedIn = Console.In; TextWriter savedOut = Console.Out; var input = new StringReader("Alice\n3\n"); var output = new StringWriter(); try { Console.SetIn(input); Console.SetOut(output); MethodUnderTest(); Assert.Contains("$749.97", output.ToString()); } finally { Console.SetIn(savedIn); Console.SetOut(savedOut); }`. The finally block is non-negotiable: if the assertion throws, the restored streams ensure later tests are unaffected. To test the null path of ReadLine — for example, verifying the loop exits cleanly on EOF — pass a StringReader whose content is exhausted before the method finishes reading, so that the last ReadLine call returns null. When a test framework runs tests in parallel, process-wide Console substitution creates a race condition; in that case, dependency injection of TextReader and TextWriter via constructor or method parameters is the correct long-term design. For sequential test suites, however, the SetIn/SetOut pattern is a low-cost and effective technique that keeps the production code simple.
