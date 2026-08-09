/*
 * =============================================================================
 * 05. TYPE CONVERSION AND CASTING IN C#
 * =============================================================================
 *
 * TOPIC: Converting values from one type to another — implicit widening,
 *        explicit casts, Parse/TryParse, the Convert class, ToString formatting,
 *        boxing/unboxing, and safe reference conversions with is and as.
 *
 * WHY IT MATTERS:
 *   User input arrives as text; APIs and databases use different numeric sizes;
 *   polymorphic objects need safe downcasts. Wrong conversions cause silent data
 *   loss, overflow wraps, NullReferenceException, or InvalidCastException.
 *   Production code must pick the right technique and know exactly when each fails.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Implicit vs explicit numeric conversion
 *   2.  Cast syntax (type)value and truncation rules
 *   3.  Overflow when narrowing (checked / unchecked)
 *   4.  Parse, TryParse, and culture-aware parsing
 *   5.  TryFormat (number → character buffer)
 *   6.  The Convert class (To___, ChangeType, null handling)
 *   7.  ToString vs Convert.ToString
 *   8.  Boxing and unboxing (value type ↔ object)
 *   9.  Safe reference casts with is and as
 *
 * =============================================================================
 */

using System;
using System.Globalization;

namespace TypeConversionAndCasting;

/*
 * =========================================================================
 * SECTION 1: TYPE CONVERSION OVERVIEW
 * =========================================================================
 *
 * A TYPE CONVERSION changes a value from one type to another. C# offers
 * several paths depending on direction, safety, and whether the source is text:
 *
 *   Direction / source              | Typical technique
 *   --------------------------------|------------------------------------------
 *   Widening (small → large)        | Implicit — compiler inserts conversion
 *   Narrowing (large → small)       | Explicit cast (type)value
 *   string → numeric / bool / date  | Parse, TryParse, Convert.To___
 *   numeric → string                | ToString(), TryFormat, Convert.ToString
 *   value type → object             | Boxing (implicit)
 *   object → value type             | Unboxing — explicit cast to exact type
 *   reference → derived / interface | Cast, as (null on failure), is (test)
 *
 * Chapter 02 introduced basics; this chapter is the full treatment. The
 * running scenario: a warehouse order line — quantities, prices, and label
 * text — must convert cleanly for calculations, storage, and printed output.
 * -------------------------------------------------------------------------
 */
public static class OrderScenario
{
    public const int DefaultUnits = 36;
    public const decimal DefaultUnitPrice = 49.99m;
    public const string ValidQuantityText = "36";
    public const string InvalidQuantityText = "abc";
}

/*
 * =========================================================================
 * SECTION 2: IMPLICIT CONVERSION (WIDENING)
 * =========================================================================
 *
 * When the destination type can hold EVERY possible value of the source type
 * without loss, the compiler converts automatically — no cast syntax.
 *
 * Common implicit paths:
 *
 *   byte → short → int → long → float → double → decimal (partial chain)
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
 *   decimal total = 99.99m;
 *   float f = total;                // COMPILE ERROR — decimal → float needs cast
 *   int i = 3.14;                   // COMPILE ERROR — floating → int needs cast
 *
 * --- 2c. string is separate ---
 *
 *   Numbers do not implicitly become string. Use ToString(), TryFormat,
 *   or string interpolation (chapter 06).
 * -------------------------------------------------------------------------
 */
public static class ImplicitConversionDemo
{
    public static (long UnitsAsLong, double UnitsAsDouble, decimal LineSubtotal, int CasesAsInt) Run(
        int unitsOrdered,
        decimal unitPrice)
    {
        long unitsAsLong = unitsOrdered;           // int → long — widening, no cast
        double unitsAsDouble = unitsOrdered;       // int → double — implicit promotion
        decimal lineSubtotal = unitsOrdered * unitPrice; // int × decimal → decimal

        short caseCount = 3;
        int casesAsInt = caseCount;                // short → int — another widening path

        return (unitsAsLong, unitsAsDouble, lineSubtotal, casesAsInt);
    }
}

/*
 * =========================================================================
 * SECTION 3: EXPLICIT CASTING — (type)value
 * =========================================================================
 *
 * NARROWING conversions may lose precision or range. You must tell the compiler
 * with cast syntax:
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
 *   (Derived)baseReference throws InvalidCastException when the object is not
 *   actually that type. Prefer is / as for safe downcasts (section 10).
 *
 * --- 3c. Compile-time note ---
 *
 * Some casts fail at compile time when types share no conversion path.
 * -------------------------------------------------------------------------
 */
public static class ExplicitCastDemo
{
    public static (int FreightRoundedDown, int PriceWholeDollars, int DiscountPercent) Run(
        decimal unitPrice,
        decimal freightCharge,
        double discountRate)
    {
        int freightRoundedDown = (int)freightCharge;     // decimal → int — truncates fraction
        int priceWholeDollars = (int)unitPrice;          // 49.99m becomes 49, not rounded
        int discountPercent = (int)(discountRate * 100); // 0.125 × 100 → 12 (cast after multiply)

        return (freightRoundedDown, priceWholeDollars, discountPercent);
    }
}

/*
 * =========================================================================
 * SECTION 4: OVERFLOW WITH NARROWING CASTS
 * =========================================================================
 *
 * Narrowing can wrap or throw depending on checked context. Chapter 02 covered
 * checked/unchecked for arithmetic; the same rules apply when casting.
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
 *   Slot IDs, port numbers, and percentages stored in byte/short need checked
 *   casts or validation — silent wrap corrupts inventory data.
 * -------------------------------------------------------------------------
 */
public static class OverflowCastDemo
{
    public static (byte UncheckedSlot, bool OverflowCaught, byte CheckedSlot) Run(int overflowCandidate)
    {
        byte uncheckedSlot = (byte)overflowCandidate; // default unchecked — 300 wraps to 44

        bool overflowCaught = false;
        byte checkedSlot = 0;
        try
        {
            checkedSlot = checked((byte)overflowCandidate); // throws if value exceeds byte range
        }
        catch (OverflowException)
        {
            overflowCaught = true;
        }

        return (uncheckedSlot, overflowCaught, checkedSlot);
    }
}

/*
 * =========================================================================
 * SECTION 5: PARSE AND TRYPARSE (STRING → TYPED VALUE)
 * =========================================================================
 *
 * User input and file data arrive as string. Parse and TryParse convert text
 * to numeric and other scalar types.
 *
 *   Method                              | On failure
 *   ------------------------------------|------------------------------------
 *   int.Parse("42")                     | FormatException (or OverflowException)
 *   int.TryParse("abc", out int n)      | returns false; n = 0 (default)
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
 *   decimal.Parse("1.234,56", NumberStyles.Number, new CultureInfo("de-DE"))
 *
 * --- 5c. Related types ---
 *
 *   Each numeric type has Parse/TryParse; bool, DateTime, and enums too:
 *
 *   bool.TryParse("true", out bool flag) → true
 *   Enum.TryParse<ShipMethod>("Express", out ShipMethod m) → true
 *
 * FormatException: text is not valid for the type.
 * OverflowException: number is too large for the target type.
 * -------------------------------------------------------------------------
 */
public enum ShipMethod
{
    Standard,
    Express
}

public static class ParseDemo
{
    public static (
        int ParsedFromLabel,
        bool InvalidParse,
        int RejectedQuantity,
        decimal EuropeanPrice,
        bool BoolParsed,
        bool ExpressShipping,
        ShipMethod ParsedShipMethod) Run(
        string validQuantityText,
        string invalidQuantityText,
        string europeanPriceText)
    {
        int parsedFromLabel = int.Parse(validQuantityText); // throws FormatException on bad text
        bool invalidParse = int.TryParse(invalidQuantityText, out int rejectedQuantity); // false → rejectedQuantity stays 0

        decimal europeanPrice = decimal.Parse(
            europeanPriceText,
            NumberStyles.Number,
            new CultureInfo("de-DE")); // "1.234,56" — comma is decimal separator in de-DE

        bool boolParsed = bool.TryParse("true", out bool expressShipping);
        _ = Enum.TryParse("Express", ignoreCase: true, out ShipMethod shipMethod); // string → enum member

        return (parsedFromLabel, invalidParse, rejectedQuantity, europeanPrice, boolParsed, expressShipping, shipMethod);
    }
}

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
 * Returns false if the buffer is too small. On success, charsWritten is the
 * number of characters written.
 *
 * Use cases: high-performance logging, CSV rows, protocol frames.
 * For most business output, ToString() or interpolation is simpler.
 * -------------------------------------------------------------------------
 */
public static class TryFormatDemo
{
    public static (bool Success, string FormattedPrice) Run(decimal unitPrice)
    {
        Span<char> priceBuffer = stackalloc char[32]; // stack buffer — no heap string yet
        bool priceFormatted = unitPrice.TryFormat(priceBuffer, out int priceCharsWritten, "F2"); // writes into span
        string priceFromBuffer = priceBuffer.Slice(0, priceCharsWritten).ToString(); // only the written chars

        return (priceFormatted, priceFromBuffer);
    }
}

/*
 * =========================================================================
 * SECTION 7: THE CONVERT CLASS
 * =========================================================================
 *
 * System.Convert provides static To___ methods for many type pairs — handy when
 * the source type is object or string and the target varies at runtime.
 *
 *   Convert.ToInt32("50")       → 50
 *   Convert.ToInt32(null)       → 0   (null → default for value types)
 *   Convert.ToDecimal("12.5")   → 12.5m
 *   Convert.ToBoolean("true")   → true
 *
 * --- 7a. vs Parse ---
 *
 *   Convert.ToInt32(string) uses current culture by default; overloads accept
 *   IFormatProvider. Convert handles null; Parse throws on null argument.
 *
 * --- 7b. ChangeType ---
 *
 *   Convert.ChangeType(value, typeof(int)) — when the target type is known only
 *   at runtime. Returns boxed result; unbox or cast to the target type.
 *   Prefer strongly typed Parse/casts in normal application code.
 *
 * --- 7c. Base64 helpers ---
 *
 *   Convert.ToBase64String / FromBase64String — binary ↔ text (not shown here).
 * -------------------------------------------------------------------------
 */
public static class ConvertClassDemo
{
    public static (int BonusUnits, int NullAsZero, decimal ConvertedFreight, bool ConvertedFlag, int FromChangeType) Run()
    {
        int bonusUnits = Convert.ToInt32("50");
        int nullAsZero = Convert.ToInt32(null); // null → 0 for value types (Parse would throw)
        decimal convertedFreight = Convert.ToDecimal("12.75", CultureInfo.InvariantCulture);
        bool convertedFlag = Convert.ToBoolean("false");

        object boxedQuantity = "42";
        int fromChangeType = (int)Convert.ChangeType(boxedQuantity, typeof(int)); // runtime target type → unbox

        return (bonusUnits, nullAsZero, convertedFreight, convertedFlag, fromChangeType);
    }
}

/*
 * =========================================================================
 * SECTION 8: TOSTRING() AND CONVERT.TOSTRING()
 * =========================================================================
 *
 * Every type inherits object.ToString(). Value types override it to return a
 * human-readable text form.
 *
 *   int qty = 36;
 *   qty.ToString()        → "36"
 *   qty.ToString("D5")    → "00036"  (format specifier)
 *
 * --- 8a. Common format specifiers ---
 *
 *   "C"  currency      49.99m.ToString("C")  → locale currency string
 *   "F2" fixed 2 dec   49.99m.ToString("F2")  → "49.99"
 *   "N0" number no dec 1234.ToString("N0")     → "1,234" (locale separators)
 *   "P"  percent        0.125.ToString("P1")  → "12.5 %"
 *
 * --- 8b. Convert.ToString vs instance .ToString() ---
 *
 *   Situation                         | Convert.ToString     | .ToString()
 *   ----------------------------------|----------------------|------------------
 *   int 42                            | "42"                 | "42"
 *   null object reference             | ""  (empty string)   | NullReferenceException
 *   null int?                         | ""                   | ""  (Nullable override)
 *
 * Use Convert.ToString when the value might be a null object reference.
 * Use instance .ToString("C") when you need format specifiers on a known value.
 * -------------------------------------------------------------------------
 */
public static class ToStringDemo
{
    public static (
        string QuantityText,
        string PaddedSkuCount,
        string PriceFixed,
        string PriceCurrency,
        string DiscountAsPercent,
        string MissingText,
        string SafeStatusText,
        string ConvertPriceText,
        string InstancePriceText) Run(int unitsOrdered, decimal unitPrice, double discountRate)
    {
        string quantityText = unitsOrdered.ToString();
        string paddedSkuCount = unitsOrdered.ToString("D4"); // decimal format — zero-padded width 4
        string priceFixed = unitPrice.ToString("F2");         // fixed two decimal places
        string priceCurrency = unitPrice.ToString("C", CultureInfo.GetCultureInfo("en-US")); // locale currency symbol
        string discountAsPercent = discountRate.ToString("P1"); // 0.125 → "12.5 %"

        int? missingCount = null;
        string missingText = missingCount?.ToString() ?? string.Empty; // null int? → empty via ?.

        object? nullableStatus = null;
        string safeStatusText = Convert.ToString(nullableStatus) ?? string.Empty; // null object → "" not exception

        string convertPriceText = Convert.ToString(unitPrice, CultureInfo.InvariantCulture) ?? string.Empty;
        string instancePriceText = unitPrice.ToString(CultureInfo.InvariantCulture); // same culture, instance method

        return (quantityText, paddedSkuCount, priceFixed, priceCurrency, discountAsPercent,
            missingText, safeStatusText, convertPriceText, instancePriceText);
    }
}

/*
 * =========================================================================
 * SECTION 9: BOXING AND UNBOXING
 * =========================================================================
 *
 * Assigning a value type to object (or a matching interface on the value type)
 * BOXES — the runtime wraps the value in a heap object. Casting back to the
 * original value type UNBOXES — a copy of the stored value is produced.
 *
 *   object boxed = 36;              // box int → object
 *   int restored = (int)boxed;      // unbox — must match EXACT stored type
 *
 * --- 9a. Wrong unbox type ---
 *
 *   long wrong = (long)boxed;       → InvalidCastException (boxed as int, not long)
 *
 * --- 9b. Boxing through interfaces ---
 *
 *   IFormattable fmt = 42;          // box int, reference typed as IFormattable
 *   int fromInterface = (int)(object)fmt;  // unbox via object to exact type
 *
 * --- 9c. Nullable value types ---
 *
 *   int? optional = 7;  object o = optional;   // boxes the int 7
 *   int? empty = null;  object n = empty;        // boxes null reference (no value box)
 *
 * --- 9d. Performance note ---
 *
 *   Boxing allocates on the heap and copies the value. Hot paths (collections of
 *   int before generics, string concatenation with value types) historically
 *   caused pressure. Prefer generic List<int> over ArrayList of boxed ints.
 *   COVERED IN DETAIL LATER → generics and memory chapters in advanced modules.
 * -------------------------------------------------------------------------
 */
public static class BoxingDemo
{
    public static (
        int UnboxedUnits,
        bool WrongUnboxFailed,
        int UnboxedFromInterface,
        bool NullableBoxedHasValue,
        bool NullableNullBoxIsNull) Run(int unitsOrdered)
    {
        object boxedUnits = unitsOrdered;              // implicit box — int copied to heap object
        int unboxedUnits = (int)boxedUnits;            // explicit unbox — must match stored type exactly

        bool wrongUnboxFailed = false;
        try
        {
            long wrongSize = (long)boxedUnits;         // InvalidCastException — boxed as int, not long
            _ = wrongSize;
        }
        catch (InvalidCastException)
        {
            wrongUnboxFailed = true;
        }

        IFormattable formattable = unitsOrdered;       // box via interface reference
        int unboxedFromInterface = (int)(object)formattable; // unbox through object to exact int

        int? withValue = 7;
        object boxedNullable = withValue;              // nullable with value boxes the underlying int
        bool nullableBoxedHasValue = boxedNullable is int;

        int? withoutValue = null;
        object? boxedNull = withoutValue;              // null nullable → null reference, no box
        bool nullableNullBoxIsNull = boxedNull is null;

        return (unboxedUnits, wrongUnboxFailed, unboxedFromInterface, nullableBoxedHasValue, nullableNullBoxIsNull);
    }
}

/*
 * =========================================================================
 * SECTION 10: is AND as — SAFE REFERENCE TYPE CONVERSIONS
 * =========================================================================
 *
 * Reference variables often hold a base type while the runtime object is a
 * derived type. Casting blindly throws; is and as test safely first.
 *
 * --- 10a. is — compatibility test ---
 *
 *   if (item is string) { … }              // bool result
 *   if (item is string label) { … }         // pattern: test + assign
 *   if (item is not null) { … }              // null check (C# 9+)
 *
 * --- 10b. as — cast or null ---
 *
 *   string? text = obj as string;           // null if obj is not a string
 *   Derived? d = baseRef as Derived;        // null on failure — no exception
 *
 *   as works with reference types and nullable value types. For unboxing a
 *   boxed int, use is int n or (int)obj — as int is not valid on object.
 *
 * --- 10c. When to choose ---
 *
 *   is + pattern   → branch and use typed variable in one step
 *   as             → optional downcast; check for null before use
 *   (Type)obj      → you are certain; failure throws InvalidCastException
 *
 * Small type hierarchy below demonstrates warehouse line items stored in a
 * heterogeneous list typed as object.
 * -------------------------------------------------------------------------
 */
public interface IShippable
{
    int WeightGrams { get; }
}

public class PhysicalLineItem : IShippable
{
    public string Sku { get; }
    public int WeightGrams { get; }

    public PhysicalLineItem(string sku, int weightGrams)
    {
        Sku = sku;
        WeightGrams = weightGrams;
    }
}

public class DigitalLineItem
{
    public string DownloadCode { get; }

    public DigitalLineItem(string downloadCode)
    {
        DownloadCode = downloadCode;
    }
}

public static class IsAsDemo
{
    public static (
        bool IsPhysical,
        string? PhysicalSku,
        bool IsDigital,
        string? DigitalCode,
        int? ShippableWeight,
        bool IsNotNull) Inspect(object? lineItem)
    {
        bool isPhysical = lineItem is PhysicalLineItem;              // type test — no cast yet
        PhysicalLineItem? physicalLine = lineItem as PhysicalLineItem; // downcast or null on failure
        string? physicalSku = physicalLine?.Sku;                     // null-safe property read

        bool isDigital = false;
        string? digitalCode = null;
        if (lineItem is DigitalLineItem digital)                     // pattern — test + assign in one step
        {
            isDigital = true;
            digitalCode = digital.DownloadCode;
        }

        int? shippableWeight = null;
        if (lineItem is IShippable shippable)                        // interface pattern match
        {
            shippableWeight = shippable.WeightGrams;
        }

        bool isNotNull = lineItem is not null;                       // C# 9+ null check pattern

        return (isPhysical, physicalSku, isDigital, digitalCode, shippableWeight, isNotNull);
    }
}

public class Program
{
    /*
     * =========================================================================
     * SECTION 11: DEMONSTRATION — Main orchestrates the chapter demo
     * =========================================================================
     *
     * Wires every section into one warehouse order-line summary.
     * -------------------------------------------------------------------------
     */
    public static void Main(string[] args)
    {
        int unitsOrdered = OrderScenario.DefaultUnits;
        decimal unitPrice = OrderScenario.DefaultUnitPrice;
        double discountRate = 0.125;
        decimal freightCharge = 12.75m;
        int overflowCandidate = 300;

        var implicitResult = ImplicitConversionDemo.Run(unitsOrdered, unitPrice);   // SECTION 2
        var explicitResult = ExplicitCastDemo.Run(unitPrice, freightCharge, discountRate); // SECTION 3
        var overflowResult = OverflowCastDemo.Run(overflowCandidate);               // SECTION 4
        var parseResult = ParseDemo.Run(
            OrderScenario.ValidQuantityText,
            OrderScenario.InvalidQuantityText,
            "1.234,56");                                                            // SECTION 5
        var tryFormatResult = TryFormatDemo.Run(unitPrice);                         // SECTION 6
        var convertResult = ConvertClassDemo.Run();                                   // SECTION 7
        var toStringResult = ToStringDemo.Run(unitsOrdered, unitPrice, discountRate); // SECTION 8
        var boxingResult = BoxingDemo.Run(unitsOrdered);                              // SECTION 9

        object physicalItem = new PhysicalLineItem("SKU-8842", weightGrams: 850);   // stored as object for is/as
        object digitalItem = new DigitalLineItem("DL-9910-A");
        var physicalInspect = IsAsDemo.Inspect(physicalItem);                       // SECTION 10
        var digitalInspect = IsAsDemo.Inspect(digitalItem);

        decimal orderTotal = implicitResult.LineSubtotal + convertResult.ConvertedFreight; // mixed-type arithmetic

        Console.WriteLine("=== Type Conversion — Warehouse Order Line ===");
        Console.WriteLine(
            $"Implicit: {unitsOrdered} units as long={implicitResult.UnitsAsLong}, " +
            $"double={implicitResult.UnitsAsDouble}, subtotal={implicitResult.LineSubtotal:C}");
        Console.WriteLine(
            $"Explicit cast: unit price {unitPrice:C} → whole dollars={explicitResult.PriceWholeDollars}, " +
            $"freight {freightCharge} → int={explicitResult.FreightRoundedDown}, " +
            $"discount %={explicitResult.DiscountPercent}");
        Console.WriteLine(
            $"Overflow: (byte){overflowCandidate} unchecked={overflowResult.UncheckedSlot}, " +
            $"checked caught={overflowResult.OverflowCaught}, checked slot={overflowResult.CheckedSlot}");
        Console.WriteLine(
            $"Parse: label \"{OrderScenario.ValidQuantityText}\" → {parseResult.ParsedFromLabel}; " +
            $"invalid TryParse={parseResult.InvalidParse}, rejected={parseResult.RejectedQuantity}");
        Console.WriteLine(
            $"Culture parse (de-DE): \"1.234,56\" → {parseResult.EuropeanPrice}; " +
            $"bool express={parseResult.ExpressShipping} (ok={parseResult.BoolParsed}); " +
            $"enum={parseResult.ParsedShipMethod}");
        Console.WriteLine(
            $"TryFormat buffer: {tryFormatResult.FormattedPrice} (success={tryFormatResult.Success})");
        Console.WriteLine(
            $"Convert: bonus={convertResult.BonusUnits}, null→int={convertResult.NullAsZero}, " +
            $"freight={convertResult.ConvertedFreight}, flag={convertResult.ConvertedFlag}, " +
            $"ChangeType={convertResult.FromChangeType}");
        Console.WriteLine(
            $"ToString: qty={toStringResult.QuantityText}, padded={toStringResult.PaddedSkuCount}, " +
            $"price={toStringResult.PriceFixed}, {toStringResult.PriceCurrency}, " +
            $"discount={toStringResult.DiscountAsPercent}");
        Console.WriteLine(
            $"Convert.ToString(null)=\"{toStringResult.SafeStatusText}\" vs invariant " +
            $"Convert=\"{toStringResult.ConvertPriceText}\" / instance=\"{toStringResult.InstancePriceText}\"");
        Console.WriteLine(
            $"Boxing: unboxed={boxingResult.UnboxedUnits}, wrong unbox caught={boxingResult.WrongUnboxFailed}, " +
            $"via IFormattable={boxingResult.UnboxedFromInterface}, " +
            $"nullable boxed is int={boxingResult.NullableBoxedHasValue}, null box is null={boxingResult.NullableNullBoxIsNull}");
        Console.WriteLine(
            $"is/as physical: isPhysical={physicalInspect.IsPhysical}, sku={physicalInspect.PhysicalSku}, " +
            $"weight={physicalInspect.ShippableWeight}");
        Console.WriteLine(
            $"is/as digital: isDigital={digitalInspect.IsDigital}, code={digitalInspect.DigitalCode}, " +
            $"isNotNull={digitalInspect.IsNotNull}");
        Console.WriteLine(
            $"Order total: {orderTotal.ToString("C", CultureInfo.GetCultureInfo("en-US"))} | " +
            $"cases={implicitResult.CasesAsInt} | nullable text=\"{toStringResult.MissingText}\"");
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
 *  (int)decimalValue                      truncates fraction toward zero
 *  (byte)intValue                         may wrap in unchecked context
 *  checked((byte)intValue)                OverflowException if out of range
 *
 * --- string → typed value ---
 *
 *  int.Parse(text)                        throws FormatException / OverflowException
 *  int.TryParse(text, out int n)          false on failure — use for user input
 *  decimal.Parse(text, styles, culture)   locale-aware
 *  Enum.TryParse<T>(text, out T value)    enum from string
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
 *  Convert.ChangeType(value, typeof(T))      runtime target type; returns object
 *
 * --- Boxing / unboxing ---
 *
 *  object o = 42;                         box (heap allocation)
 *  int n = (int)o;                        unbox — exact type required
 *  (long)o when boxed as int               InvalidCastException
 *
 * --- is / as (reference conversions) ---
 *
 *  obj is string                          bool compatibility test
 *  obj is string s                        pattern — test + assign
 *  obj is not null                        null check
 *  obj as string                          cast or null (no exception)
 *
 * --- Common mistakes ---
 *
 *  Mistake                              | Result
 *  -------------------------------------|----------------------------------
 *  Expect rounding from (int) cast        | truncates toward zero
 *  Parse untrusted input without TryParse | FormatException crashes program
 *  nullReference.ToString()               | NullReferenceException
 *  Unbox to wrong type (long)(object)int  | InvalidCastException
 *  Assume implicit string conversion      | compile error — use ToString
 *  Use as for unboxing boxed int          | not valid — use is int or (int)obj
 *
 * --- Deferred to later chapters ---
 *
 *  User-defined conversion operators    → OOP / Advanced Types
 *  Pattern matching switch on type      → Modern C# / Control Flow
 *
 * =========================================================================
 */
