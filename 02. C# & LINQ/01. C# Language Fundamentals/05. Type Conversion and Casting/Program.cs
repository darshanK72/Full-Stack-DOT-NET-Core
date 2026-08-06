/*
 * =============================================================================
 * 05. TYPE CONVERSION AND CASTING IN C# — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: Converting values from one type to another — implicit widening,
 *        explicit casts, Parse/TryParse/TryFormat, the Convert class,
 *        ToString formatting, and overflow behavior when narrowing.
 *
 * WHY IT MATTERS:
 *   User input arrives as text; databases and APIs use different numeric sizes;
 *   reports need formatted strings. Wrong conversions cause silent data loss,
 *   overflow wraps, or runtime exceptions. Production code must choose the
 *   right technique: cast, Parse, TryParse, or Convert — and know when each
 *   fails or truncates.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Implicit vs explicit conversion
 *   2.  Cast syntax (type)value
 *   3.  Overflow when narrowing (checked / unchecked)
 *   4.  Parse and TryParse (string → number)
 *   5.  TryFormat (number → character buffer)
 *   6.  Convert class methods
 *   7.  ToString() on value types
 *   8.  Convert.ToString vs instance .ToString()
 *   9.  Boxing and unboxing in conversions (preview)
 *
 * =============================================================================
 */

using System;
using System.Globalization;

namespace TypeConversionAndCasting;

public class Program
{
    public static void Main(string[] args)
    {
        /*
         * =========================================================================
         * SECTION 1: TYPE CONVERSION OVERVIEW
         * =========================================================================
         *
         * A TYPE CONVERSION (cast) changes a value from one type to another.
         * C# offers several paths depending on direction and safety:
         *
         *   Direction / source          | Typical technique
         *   ----------------------------|------------------------------------------
         *   Widening (small → large)    | Implicit — compiler inserts conversion
         *   Narrowing (large → small)   | Explicit cast (type)value
         *   string → numeric            | Parse, TryParse, Convert.To___
         *   numeric → string            | ToString(), TryFormat, Convert.ToString
         *   object ↔ value type         | Boxing / unboxing (preview below)
         *
         * Chapter 02 introduced the basics; this chapter is the full treatment.
         * The scenario: a warehouse order line — quantities, prices, and text
         * from a shipping label — must convert cleanly for calculations and output.
         * -------------------------------------------------------------------------
         */

        int unitsOrdered = 36;
        decimal unitPrice = 49.99m;
        string quantityFromLabel = "36";
        string invalidQuantityText = "abc";


        /*
         * =========================================================================
         * SECTION 2: IMPLICIT CONVERSION (WIDENING)
         * =========================================================================
         *
         * When the destination type can hold EVERY possible value of the source
         * type without loss, the compiler converts automatically — no cast syntax.
         *
         * Common implicit paths:
         *
         *   byte → short → int → long → float → double → decimal
         *   int → long, float, double, decimal
         *   char → int (Unicode code point as number)
         *
         * --- 2a. Numeric widening ---
         *
         *   int cases = 3;
         *   long casesAsLong = cases;       // OK — long holds all int values
         *   double rate = cases;            // OK — int → double
         *
         * --- 2b. What is NOT implicit ---
         *
         *   decimal total = 99.99;          // double literal
         *   float f = total;                // COMPILE ERROR — decimal → float needs cast
         *   int i = 3.14;                   // COMPILE ERROR — floating → int needs cast
         *
         * --- 2c. string is separate ---
         *
         *   Numbers do not implicitly become string. Use ToString(), TryFormat,
         *   or string interpolation — covered in later sections.
         * -------------------------------------------------------------------------
         */

        long unitsAsLong = unitsOrdered;                    // int → long
        double unitsAsDouble = unitsOrdered;                // int → double
        decimal lineSubtotal = unitsOrdered * unitPrice;    // int promoted to decimal in *

        short caseCount = 3;
        int casesAsInt = caseCount;                         // short → int


        /*
         * =========================================================================
         * SECTION 3: EXPLICIT CASTING — (type)value
         * =========================================================================
         *
         * NARROWING conversions may lose precision or range. You must tell the
         * compiler with cast syntax:
         *
         *   (targetType)expression
         *
         * Examples:
         *
         *   decimal price = 49.99m;
         *   int dollarsOnly = (int)price;     → 49  (fraction TRUNCATED, not rounded)
         *
         *   double ratio = 0.75;
         *   int percent = (int)(ratio * 100); → 75
         *
         * --- 3a. Truncation vs rounding ---
         *
         * Casting floating/decimal to integer TRUNCATES toward zero:
         *
         *   (int) 99.99   → 99
         *   (int)-99.99   → -99
         *
         * For banker's rounding use Math.Round before casting.
         *
         * --- 3b. Reference type casts ---
         *
         * Casting between class/interface types is also (Type)obj — invalid casts
         * throw InvalidCastException at runtime. Pattern matching (is / as) is
         * covered in later OOP chapters.
         *
         * --- 3c. Compile-time note ---
         *
         * Some casts fail at compile time when the types share no conversion path.
         * -------------------------------------------------------------------------
         */

        decimal freightCharge = 12.75m;
        int freightRoundedDown = (int)freightCharge;        // 12 — truncated
        int priceWholeDollars = (int)unitPrice;             // 49

        double discountRate = 0.125;
        int discountPercent = (int)(discountRate * 100);    // 12 (0.125 * 100 = 12.5 → 12)


        /*
         * =========================================================================
         * SECTION 4: OVERFLOW WITH NARROWING CASTS
         * =========================================================================
         *
         * Narrowing can wrap or throw depending on checked context.
         * Chapter 02 covered checked/unchecked for arithmetic; the same rules
         * apply when casting to a smaller type.
         *
         * --- 4a. Default (unchecked) — silent wrap ---
         *
         *   int big = 300;
         *   byte shelfSlot = (byte)big;   → 44  (300 % 256 = 44)
         *
         * --- 4b. checked cast — OverflowException ---
         *
         *   byte safeSlot = checked((byte)big);   → throws if value out of byte range
         *
         * --- 4c. When to care ---
         *
         *   Warehouse slot IDs, port numbers, and percentages stored in byte/short
         *   need checked casts or validation — silent wrap corrupts inventory data.
         *
         * Compile error note: unchecked is the default; checked applies to the
         * block or expression where it appears.
         * -------------------------------------------------------------------------
         */

        int overflowCandidate = 300;
        byte uncheckedSlot = (byte)overflowCandidate;       // wraps to 44

        bool overflowCaught = false;
        byte checkedSlot = 0;
        try
        {
            checkedSlot = checked((byte)overflowCandidate);
        }
        catch (OverflowException)
        {
            overflowCaught = true;
        }


        /*
         * =========================================================================
         * SECTION 5: PARSE AND TRYPARSE (STRING → NUMBER)
         * =========================================================================
         *
         * User input and file data arrive as string. Parse and TryParse convert
         * text to numeric types.
         *
         *   Method                         | On failure
         *   -------------------------------|------------------------------------
         *   int.Parse("42")                | FormatException (or OverflowException)
         *   int.TryParse("abc", out int n) | returns false; n = 0 (default)
         *
         * --- 5a. Prefer TryParse for external input ---
         *
         *   Parse throws — fine for trusted constants, risky for keyboard/HTTP input.
         *   TryParse returns bool — branch without exceptions in hot paths.
         *
         * --- 5b. Culture-specific parsing ---
         *
         *   "1,234.56" in en-US vs "1.234,56" in de-DE — pass IFormatProvider:
         *
         *   decimal.Parse("1,234.56", NumberStyles.Number, CultureInfo.InvariantCulture)
         *
         * --- 5c. Related types ---
         *
         *   Each numeric type has Parse/TryParse: decimal, double, bool, DateTime, etc.
         *   bool.TryParse("true", out bool flag) → true
         *
         * FormatException: text is not a valid number for the type.
         * OverflowException: number is too large for the target type.
         * -------------------------------------------------------------------------
         */

        int parsedFromLabel = int.Parse(quantityFromLabel);
        bool invalidParse = int.TryParse(invalidQuantityText, out int rejectedQuantity);

        string europeanPriceText = "1.234,56";
        decimal europeanPrice = decimal.Parse(
            europeanPriceText,
            NumberStyles.Number,
            new CultureInfo("de-DE"));

        bool boolParsed = bool.TryParse("true", out bool expressShipping);


        /*
         * =========================================================================
         * SECTION 6: TRYFORMAT (NUMBER → CHARACTER BUFFER)
         * =========================================================================
         *
         * TryFormat writes a formatted representation into a Span<char> or char[]
         * without allocating a new string on the heap (when using stack buffers).
         *
         *   Span<char> buffer = stackalloc char[32];
         *   bool ok = value.TryFormat(buffer, out int charsWritten, format);
         *
         * Returns false if the buffer is too small. On success, charsWritten is
         * the number of characters written.
         *
         * Use cases: high-performance logging, building CSV rows, protocol frames.
         * For most business output, ToString() or interpolation is simpler.
         * -------------------------------------------------------------------------
         */

        Span<char> priceBuffer = stackalloc char[32];
        bool priceFormatted = unitPrice.TryFormat(priceBuffer, out int priceCharsWritten, "F2");
        string priceFromBuffer = priceBuffer.Slice(0, priceCharsWritten).ToString();


        /*
         * =========================================================================
         * SECTION 7: THE CONVERT CLASS
         * =========================================================================
         *
         * System.Convert provides static To___ methods for many type pairs in one
         * place — handy when the source type is object or string and the target
         * varies at runtime.
         *
         *   Convert.ToInt32("50")       → 50
         *   Convert.ToInt32(null)       → 0   (null → default for value types)
         *   Convert.ToDecimal("12.5")   → 12.5m
         *   Convert.ToBoolean("true")   → true
         *
         * --- 7a. Base64 and binary ---
         *
         *   Convert.ToBase64String(bytes) / FromBase64String — not covered here.
         *
         * --- 7b. vs Parse ---
         *
         *   Convert.ToInt32(string) uses current culture by default; overloads accept
         *   IFormatProvider. Convert handles null; Parse throws on null argument.
         *
         * --- 7c. ChangeType ---
         *
         *   Convert.ChangeType(value, typeof(int)) — dynamic scenarios; prefer
         *   strongly typed Parse/casts in normal application code.
         * -------------------------------------------------------------------------
         */

        int bonusUnits = Convert.ToInt32("50");
        int nullAsZero = Convert.ToInt32(null);             // 0, not an exception
        decimal convertedFreight = Convert.ToDecimal("12.75");
        bool convertedFlag = Convert.ToBoolean("false");


        /*
         * =========================================================================
         * SECTION 8: TOSTRING() ON VALUE TYPES
         * =========================================================================
         *
         * Every type inherits object.ToString(). Value types override it to return
         * a human-readable text form of the value.
         *
         *   int qty = 36;
         *   qty.ToString()        → "36"
         *   qty.ToString("D5")    → "00036"  (format specifier)
         *
         * --- 8a. Format specifiers (common) ---
         *
         *   "C"  currency      49.99m.ToString("C")  → locale currency string
         *   "F2" fixed 2 dec   49.99m.ToString("F2")  → "49.99" (in invariant-like fixed)
         *   "N0" number no dec 1234.ToString("N0")     → "1,234" (locale separators)
         *   "P"  percent        0.125.ToString("P1")  → "12.5 %"
         *
         * --- 8b. Culture ---
         *
         *   price.ToString("C", CultureInfo.GetCultureInfo("en-IN"))
         *
         * --- 8c. Nullable value types ---
         *
         *   int? optional = null;
         *   optional.ToString()  → empty string "" (does not throw)
         * -------------------------------------------------------------------------
         */

        string quantityText = unitsOrdered.ToString();
        string paddedSkuCount = unitsOrdered.ToString("D4");
        string priceFixed = unitPrice.ToString("F2");
        string priceCurrency = unitPrice.ToString("C", CultureInfo.GetCultureInfo("en-US"));
        string discountAsPercent = discountRate.ToString("P1");

        int? missingCount = null;
        string missingText = missingCount?.ToString() ?? string.Empty;       // ""


        /*
         * =========================================================================
         * SECTION 9: CONVERT.TOSTRING VS INSTANCE .TOSTRING()
         * =========================================================================
         *
         * Syllabus highlight — both produce string output but behave differently
         * with null and with reference types.
         *
         *   Situation                         | Convert.ToString     | .ToString()
         *   ----------------------------------|----------------------|------------------
         *   int 42                            | "42"                 | "42"
         *   null object reference             | ""  (empty string)   | NullReferenceException
         *   null int?                         | ""                   | ""  (Nullable override)
         *   bool false                        | "False"              | "False"
         *
         * --- 9a. When to use Convert.ToString ---
         *
         *   Defensive logging or UI when the value might be null object:
         *
         *     object? status = GetStatus();   // might be null
         *     string text = Convert.ToString(status);   // safe → ""
         *
         * --- 9b. When to use .ToString() ---
         *
         *   Known non-null value types, or when you want format specifiers:
         *
         *     total.ToString("C")
         *
         * --- 9c. Convert.ToString overloads ---
         *
         *   Convert.ToString(value, IFormatProvider) — culture-aware for primitives.
         * -------------------------------------------------------------------------
         */

        object? nullableStatus = null;
        string safeStatusText = Convert.ToString(nullableStatus) ?? string.Empty;   // "" — no throw

        string convertPriceText = Convert.ToString(unitPrice, CultureInfo.InvariantCulture);
        string instancePriceText = unitPrice.ToString(CultureInfo.InvariantCulture);


        /*
         * =========================================================================
         * SECTION 10: BOXING AND UNBOXING (PREVIEW)
         * =========================================================================
         *
         * Assigning a value type to object (or interface) BOXES — wraps the value
         * in a heap object. Casting back to the original value type UNBOXES.
         *
         *   object boxed = unitsOrdered;        // box int → object
         *   int restored = (int)boxed;          // unbox — must match exact type
         *
         * Wrong unbox type throws InvalidCastException:
         *
         *   (long)boxed   → InvalidCastException (boxed as int, not long)
         *
         * COVERED IN DETAIL LATER → 08. Memory Management
         *   (boxing cost, when to avoid, generics eliminating boxes)
         * -------------------------------------------------------------------------
         */

        object boxedUnits = unitsOrdered;
        int unboxedUnits = (int)boxedUnits;

        bool wrongUnboxFailed = false;
        try
        {
            long wrongSize = (long)boxedUnits;              // InvalidCastException
            _ = wrongSize;
        }
        catch (InvalidCastException)
        {
            wrongUnboxFailed = true;
        }


        /*
         * =========================================================================
         * SECTION 11: ORDER LINE SUMMARY (ALL CONVERSIONS WIRED)
         * =========================================================================
         *
         * Pulls conversions from this chapter into one receipt-style summary.
         * -------------------------------------------------------------------------
         */

        decimal orderTotal = lineSubtotal + convertedFreight;

        Console.WriteLine("=== Type Conversion — Warehouse Order Line ===");
        Console.WriteLine($"Implicit: {unitsOrdered} units as long={unitsAsLong}, double={unitsAsDouble}, subtotal={lineSubtotal:C}");
        Console.WriteLine($"Explicit cast: unit price {unitPrice:C} → whole dollars={priceWholeDollars}, freight {freightCharge} → int={freightRoundedDown}");
        Console.WriteLine($"Overflow: (byte){overflowCandidate} unchecked={uncheckedSlot}, checked caught={overflowCaught}, checked slot={checkedSlot}");
        Console.WriteLine($"Parse: label \"{quantityFromLabel}\" → {parsedFromLabel}; invalid TryParse={invalidParse}, rejected={rejectedQuantity}");
        Console.WriteLine($"Culture parse (de-DE): \"{europeanPriceText}\" → {europeanPrice}");
        Console.WriteLine($"TryFormat buffer: {priceFromBuffer} (success={priceFormatted})");
        Console.WriteLine($"Convert: bonus={bonusUnits}, null→int={nullAsZero}, express={convertedFlag}");
        Console.WriteLine($"ToString: qty={quantityText}, padded={paddedSkuCount}, price={priceFixed}, {priceCurrency}, discount={discountAsPercent}");
        Console.WriteLine($"Convert.ToString(null)=\"{safeStatusText}\" vs invariant price=\"{convertPriceText}\" / instance=\"{instancePriceText}\"");
        Console.WriteLine($"Boxing: unboxed={unboxedUnits}, wrong unbox caught={wrongUnboxFailed}");
        Console.WriteLine($"Order total: {orderTotal.ToString("C", CultureInfo.GetCultureInfo("en-US"))} | cases={casesAsInt} | express parse={expressShipping} (bool ok={boolParsed})");
        Console.WriteLine($"Nullable ToString: \"{missingText}\" (empty) | discount %={discountPercent}");
    }
}

/*
 * =========================================================================
 * QUICK REFERENCE — TYPE CONVERSION AND CASTING
 * =========================================================================
 *
 * --- Widening (implicit) ---
 *
 *  int → long, float, double, decimal     no cast required
 *  short → int → long                     automatic promotion in expressions
 *
 * --- Narrowing (explicit) ---
 *
 *  (int)decimalValue                      truncates fraction
 *  (byte)intValue                         may wrap in unchecked context
 *  checked((byte)intValue)                OverflowException if out of range
 *
 * --- string → number ---
 *
 *  int.Parse(text)                        throws FormatException / OverflowException
 *  int.TryParse(text, out int n)          false on failure — use for user input
 *  decimal.Parse(text, styles, culture)   locale-aware
 *
 * --- number → string ---
 *
 *  value.ToString("F2")                   format specifiers on instance
 *  value.TryFormat(span, out written, fmt) low-allocation into buffer
 *  Convert.ToString(value)                null-safe → "" for null object
 *
 * --- Convert class ---
 *
 *  Convert.ToInt32 / ToDecimal / ToBoolean   null → default for value type
 *  Convert.ToString(obj)                     null → "" (not exception)
 *
 * --- Convert.ToString vs .ToString() ---
 *
 *  null object reference    Convert.ToString → ""     obj.ToString() → NullReferenceException
 *  value type with format     use .ToString("C")       Convert.ToString has provider overloads
 *
 * --- Boxing preview ---
 *
 *  object o = 42;                         box
 *  int n = (int)o;                        unbox — exact type required
 *
 * --- Common mistakes ---
 *
 *  Mistake                              | Result
 *  -------------------------------------|----------------------------------
 *  Expect rounding from (int) cast        | truncates toward zero
 *  Parse untrusted input without TryParse | FormatException crashes program
 *  nullReference.ToString()               | NullReferenceException
 *  Unbox to wrong type (long)(object)int  | InvalidCastException
 *  Assume implicit string conversion      | compile error — use ToString/interpolation
 *
 * --- Deferred to later chapters ---
 *
 *  User-defined conversion operators    → OOP / Advanced Types
 *  Pattern matching (is, switch type)   → Modern C# chapters
 *  Boxing performance details           → Memory Management
 *
 * =========================================================================
 */
