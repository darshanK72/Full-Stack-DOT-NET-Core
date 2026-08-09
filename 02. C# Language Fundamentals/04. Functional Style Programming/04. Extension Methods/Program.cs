/*
 * =============================================================================
 * 04. EXTENSION METHODS — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: Extension methods — static methods in a static class whose first
 *        parameter is marked with this, so you call them with instance-style
 *        syntax on existing types (including sealed BCL types and interfaces).
 *
 * WHY IT MATTERS:
 *   You often need a helper that "belongs" on string, DateTime, or IEnumerable
 *   without forking the framework or wrapping every value in a new class.
 *   Extension methods add discoverable API surface (IntelliSense on the type)
 *   while staying ordinary static methods under the hood. LINQ's Where, Select,
 *   and OrderBy are the most widely used extensions in .NET.
 *
 * WHAT YOU WILL LEARN:
 *   1.  What extension methods are (syntax sugar for static calls)
 *   2.  Static class rules and the this parameter
 *   3.  Extending sealed, built-in, and custom types
 *   4.  Instance-style call vs explicit static call (common confusion)
 *   5.  Namespace and using requirements (discovery / CS1061)
 *   6.  Null reference behavior vs instance methods
 *   7.  Extension vs instance method precedence
 *   8.  Chaining multiple extensions
 *   9.  Generic extension methods
 *   10. Extending interfaces (IEnumerable<T>, IReadOnlyList<T>, …)
 *   11. LINQ as IEnumerable<T> extensions (preview)
 *
 * =============================================================================
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ExtensionMethods;

/*
 * =========================================================================
 * SECTION 1: WHAT IS AN EXTENSION METHOD?
 * =========================================================================
 *
 * An EXTENSION METHOD is a static method defined in a static class where
 * the first parameter is marked with the keyword this:
 *
 *   public static string ToDisplayLabel(this string value)
 *                                 ────
 *                                 "Extends" string — value is the target
 *
 * At the call site it LOOKS like an instance method:
 *
 *   string label = customerName.ToDisplayLabel();
 *
 * The compiler rewrites that to a static call:
 *
 *   string label = StringExtensions.ToDisplayLabel(customerName);
 *
 * No IL magic on the extended type — the method lives in YOUR static class.
 * The extended type (string, DateTime, OrderLine, IEnumerable<T>, …) is never
 * modified. Section 2 below shows the full rule set on StringExtensions.
 * -------------------------------------------------------------------------
 */

/*
 * =========================================================================
 * SECTION 2: STATIC CLASS RULES AND THE this PARAMETER
 * =========================================================================
 *
 * Extension methods MUST satisfy ALL of these rules:
 *
 *  Rule                              | Why
 *  ----------------------------------|----------------------------------
 *  Declared in a static class        | Container for static helpers
 *  Method itself is static           | No instance of helper class needed
 *  First parameter uses this         | Tells compiler which type to extend
 *  Class is non-nested top-level     | Nested static classes cannot host them
 *  (Optional) generic static class OK| Same rules on the static method
 *
 * Common compile errors:
 *
 *  CS1105  Extension method must be static
 *  CS1106  Extension method must be defined in a non-nested static class
 *  CS1110  Cannot define a new extension method in a nested class
 *
 * --- 2a. The this parameter ---
 *
 * The this modifier goes ONLY on the FIRST parameter. That parameter's type
 * is the type being extended (string, DateTime?, OrderLine, …).
 *
 * Additional parameters behave like normal method parameters:
 *
 *   value.Truncate(12)  →  StringExtensions.Truncate(value, 12)
 *
 * --- 2b. Value types vs reference types ---
 *
 *  Extended type   | this receives          | Example below
 *  ----------------|------------------------|------------------------------
 *  Reference       | Reference to object    | string ToDisplayLabel
 *  Value (struct)  | Copy of struct         | DateTime IsWeekend (Section 4)
 *
 * Extension methods do NOT get special access to private members of the
 * extended type — they see only public/protected API, like any external static
 * method. You cannot "open up" a sealed class's internals.
 * -------------------------------------------------------------------------
 */
public static class StringExtensions
{
    public static string ToDisplayLabel(this string value)
    {
        return $"[{value}]"; // wraps display text in brackets
    }

    public static string Truncate(this string value, int maxLength)
    {
        if (maxLength < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxLength));
        }

        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
        {
            return value; // nothing to trim
        }

        return value.Substring(0, maxLength) + "…"; // ellipsis when shortened
    }

    public static bool IsNullOrBlank(this string? value)
    {
        return string.IsNullOrWhiteSpace(value); // null-safe — Section 7 in Main
    }
}

/*
 * =========================================================================
 * SECTION 3: DOMAIN TYPE — OrderLine (custom sealed class)
 * =========================================================================
 *
 * Sealed classes cannot be subclassed. Extension methods are the idiomatic way
 * to add formatting helpers to YOUR types without bloating the type itself.
 * OrderLineExtensions (Section 5) adds receipt formatting here.
 * -------------------------------------------------------------------------
 */
public sealed class OrderLine
{
    public OrderLine(string sku, string productName, int quantity, decimal unitPrice)
    {
        Sku = sku;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public string Sku { get; }

    public string ProductName { get; }

    public int Quantity { get; }

    public decimal UnitPrice { get; }

    public decimal LineTotal => Quantity * UnitPrice; // computed property — no setter
}

/*
 * =========================================================================
 * SECTION 4: EXTENDING SEALED AND BUILT-IN TYPES — DateTime (struct)
 * =========================================================================
 *
 * string and DateTime are framework types you do not own. string is sealed;
 * DateTime is a struct. Both can be extended the same way as custom types.
 *
 *  Type            | Sealed / struct? | Extended in this chapter
 *  ----------------|------------------|----------------------------------
 *  string          | sealed           | StringExtensions (Section 2)
 *  DateTime        | struct           | DateTimeExtensions (below)
 *  OrderLine       | sealed           | OrderLineExtensions (Section 5)
 *
 * You can define multiple static classes each extending the same type — all
 * appear in IntelliSense when their namespaces are in scope.
 * -------------------------------------------------------------------------
 */
public static class DateTimeExtensions
{
    public static string ToOrderDateLabel(this DateTime date)
    {
        return date.ToString("dddd, dd MMM yyyy", CultureInfo.CurrentCulture);
    }

    public static bool IsWeekend(this DateTime date)
    {
        return date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
    }
}

/*
 * =========================================================================
 * SECTION 5: EXTENDING CUSTOM SEALED TYPES
 * =========================================================================
 *
 * Same rules as BCL types: static class, static method, this on first param.
 * ToReceiptLine formats one line item for a text receipt.
 * -------------------------------------------------------------------------
 */
public static class OrderLineExtensions
{
    public static string ToReceiptLine(this OrderLine line)
    {
        string price = line.LineTotal.ToString("C", CultureInfo.CurrentCulture);
        return $"{line.Sku} | {line.ProductName} x{line.Quantity} = {price}";
    }
}

/*
 * =========================================================================
 * SECTION 6: GENERIC EXTENSION METHODS
 * =========================================================================
 *
 * Extension methods can be generic. The type parameter on the method (or on
 * the static class) constrains what the this parameter can be:
 *
 *   public static T Clamp<T>(this T value, T min, T max) where T : IComparable<T>
 *
 * The compiler infers T from the receiver at the call site:
 *
 *   decimal clamped = unitPrice.Clamp(0m, 999.99m);
 *
 * Generic extensions are how LINQ defines Where<T>, Select<T,TResult>, etc.
 * on IEnumerable<T> once instead of duplicating per element type.
 * -------------------------------------------------------------------------
 */
public static class ComparableExtensions
{
    public static T Clamp<T>(this T value, T min, T max)
        where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0)
        {
            return min; // below floor
        }

        if (value.CompareTo(max) > 0)
        {
            return max; // above ceiling
        }

        return value; // already in range
    }
}

/*
 * =========================================================================
 * SECTION 7: EXTENDING INTERFACES
 * =========================================================================
 *
 * The extended type can be an interface — not only classes and structs.
 * Any object that implements the interface gets the extension in IntelliSense.
 *
 *   public static decimal TotalAmount(this IEnumerable<OrderLine> lines)
 *
 * IEnumerable<T> is the classic example: one extension method applies to
 * arrays, List<T>, HashSet<T>, and any custom collection that implements
 * the interface. This is exactly how LINQ attaches to "anything enumerable."
 *
 * COVERED IN DETAIL LATER → 05. Language Integrated Query module
 * -------------------------------------------------------------------------
 */
public static class OrderLineCollectionExtensions
{
    public static decimal TotalAmount(this IEnumerable<OrderLine> lines)
    {
        return lines.Sum(line => line.LineTotal); // Sum is itself an extension (Section 11)
    }

    public static string ToReceiptBlock(this IEnumerable<OrderLine> lines)
    {
        return string.Join(Environment.NewLine, lines.Select(line => line.ToReceiptLine()));
    }

    public static IReadOnlyList<OrderLine> OverUnitPrice(this IEnumerable<OrderLine> lines, decimal threshold)
    {
        return lines.Where(line => line.UnitPrice >= threshold).ToList(); // materialize for demo output
    }
}

public class Program
{
    /*
     * =========================================================================
     * SECTION 8: DEMONSTRATION — wiring the chapter demo
     * =========================================================================
     *
     * Main orchestrates instance-style calls, static calls, chaining, generic
     * and interface extensions, null behavior, precedence, and a LINQ preview.
     * Scenario: format a small e-commerce order receipt using extensions.
     * -------------------------------------------------------------------------
     */
    public static void Main(string[] args)
    {
        string customerName = "Northwind Trading";
        string orderReference = "ORD-2026-0412";
        DateTime orderDate = new DateTime(2026, 4, 7);
        OrderLine[] lines =
        {
            new OrderLine("SKU-100", "Wireless Mouse", 2, 29.99m),
            new OrderLine("SKU-220", "USB-C Hub", 1, 49.50m),
            new OrderLine("SKU-310", "Mechanical Keyboard", 1, 129.00m),
        };

        /*
         * --- 8a. Instance-style vs explicit static call ---
         *
         * Both forms compile to the same static invocation. Use instance-style
         * syntax for readability; use explicit static form when disambiguating
         * overloads or when the extension namespace is not imported.
         *
         *  Style              | Example
         *  -------------------|---------------------------------------------------
         *  Instance-style     | customerName.ToDisplayLabel()
         *  Explicit static    | StringExtensions.ToDisplayLabel(customerName)
         *
         * COMMON CONFUSION: instance-style syntax does NOT mean an instance
         * method exists on the type. There is no virtual dispatch, no override,
         * and no polymorphism on the extension itself — always a static call.
         * -------------------------------------------------------------------------
         */
        string displayCustomer = customerName.ToDisplayLabel();
        string staticStyleLabel = StringExtensions.ToDisplayLabel(customerName);
        bool labelsMatch = displayCustomer == staticStyleLabel;

        /*
         * --- 8b. Chaining extensions ---
         *
         * Each extension returns a type; if another extension applies to that
         * return type, you chain left-to-right:
         *
         *   customerName.ToDisplayLabel().Truncate(20)
         *
         * Equivalent static form:
         *
         *   StringExtensions.Truncate(StringExtensions.ToDisplayLabel(customerName), 20)
         *
         * Order matters — ToDisplayLabel adds brackets, then Truncate shortens.
         * -------------------------------------------------------------------------
         */
        string chainedReference = orderReference.ToDisplayLabel().Truncate(16);

        /*
         * --- 8c. Namespace and using requirements ---
         *
         * Extension methods are NOT discovered by the extended type's namespace.
         * They are brought into scope by a using directive for the namespace that
         * CONTAINS the static extension class:
         *
         *   using ExtensionMethods;   ← this file's namespace (all types below)
         *
         * If extensions lived in ExtensionMethods.Extensions instead, you would
         * need using ExtensionMethods.Extensions; or a fully qualified static call:
         *
         *   ExtensionMethods.StringExtensions.ToDisplayLabel("x")
         *
         * Without the correct using, CS1061 reports the method missing on the type.
         *
         * Notes:
         *  • using static SomeClass; imports static MEMBERS, not extension methods.
         *  • Convention: place extension static classes in *.Extensions namespaces.
         *  • Same assembly or referenced assembly — both work identically.
         * -------------------------------------------------------------------------
         */
        bool extensionsInScope = customerName.ToDisplayLabel().StartsWith('[');

        /*
         * --- 8d. Null behavior (reference-type receivers) ---
         *
         * Unlike true instance methods, extension methods CAN be called when the
         * receiver reference is null — the static method still runs and receives
         * null as the first argument:
         *
         *   string? text = null;
         *   bool blank = text.IsNullOrBlank();   // OK — returns true
         *
         * A real instance method on null would throw NullReferenceException before
         * the method body runs. Design extensions that accept null explicitly when
         * the extended type is a reference type (use string? on the this parameter).
         * -------------------------------------------------------------------------
         */
        string? nullableNotes = null;
        string? blankNotes = "   ";
        bool notesMissing = nullableNotes.IsNullOrBlank();
        bool notesBlank = blankNotes.IsNullOrBlank();
        string? missingPromoCode = null;
        bool promoMissing = missingPromoCode.IsNullOrBlank();

        /*
         * --- 8e. Extension vs instance method precedence ---
         *
         * If the extended type already has an instance method with the same
         * signature, the INSTANCE method wins. Extensions never replace existing
         * instance API — they only add methods not already defined on the type.
         *
         * Example: string already has Trim(). You cannot "extend away" Trim by
         * defining public static string Trim(this string s) — the compiler binds
         * to the instance Trim on string literals and variables.
         *
         * Prefer small, well-named extension methods; avoid duplicating BCL names.
         * -------------------------------------------------------------------------
         */
        string padded = "  hub  ";
        string trimmedByInstance = padded.Trim(); // instance method on string — not an extension

        /*
         * --- 8f. Value-type and custom-type extensions ---
         */
        string orderDateLabel = orderDate.ToOrderDateLabel();
        bool shipsOnWeekend = orderDate.IsWeekend();
        decimal clampedPrice = lines[0].UnitPrice.Clamp(0m, 99.99m); // generic extension (Section 6)

        /*
         * --- 8g. Interface extensions on IEnumerable<OrderLine> ---
         */
        string receiptBody = lines.ToReceiptBlock();
        decimal orderTotal = lines.TotalAmount();
        decimal premiumThreshold = 50m;
        IReadOnlyList<OrderLine> premiumLines = lines.OverUnitPrice(premiumThreshold);

        /*
         * --- 8h. LINQ as IEnumerable<T> extensions (PREVIEW) ---
         *
         * System.Linq defines hundreds of extension methods on IEnumerable<T> and
         * IQueryable<T>. Where, Select, OrderBy, Sum, and Count look like instance
         * methods but follow the same static-class + this pattern as your own code.
         *
         * COVERED IN DETAIL LATER → 05. Language Integrated Query module
         *   (deferred execution, query vs method syntax, standard query operators)
         *
         * Below: filter lines over a price threshold and sum their totals — same
         * extension pattern as Section 7, now using built-in LINQ operators.
         * -------------------------------------------------------------------------
         */
        decimal premiumSubtotal = lines
            .Where(line => line.UnitPrice >= premiumThreshold)
            .Sum(line => line.LineTotal);

        int premiumLineCount = lines.Count(line => line.UnitPrice >= premiumThreshold);

        /*
         * --- Output ---
         */
        Console.WriteLine("=== Extension Methods — Order Receipt ===");
        Console.WriteLine();
        Console.WriteLine("--- Sections 1–2: syntax and static class rules ---");
        Console.WriteLine($"Customer label: {displayCustomer}");
        Console.WriteLine($"Chained reference: {chainedReference}");
        Console.WriteLine();
        Console.WriteLine("--- Section 4–5: DateTime and OrderLine extensions ---");
        Console.WriteLine($"Order date: {orderDateLabel} | Weekend order: {shipsOnWeekend}");
        Console.WriteLine($"Clamped unit price (SKU-100): {clampedPrice.ToString("C", CultureInfo.CurrentCulture)}");
        Console.WriteLine();
        Console.WriteLine("--- Section 8a: instance-style vs static call ---");
        Console.WriteLine($"Labels match: {labelsMatch} → {displayCustomer}");
        Console.WriteLine();
        Console.WriteLine("--- Section 7–8g: interface extensions ---");
        Console.WriteLine(receiptBody);
        Console.WriteLine($"Order total: {orderTotal.ToString("C", CultureInfo.CurrentCulture)}");
        Console.WriteLine($"Premium lines (≥ {premiumThreshold.ToString("C", CultureInfo.CurrentCulture)}): {premiumLines.Count}");
        Console.WriteLine();
        Console.WriteLine("--- Section 8c: namespace in scope ---");
        Console.WriteLine($"Extension resolved via using: {extensionsInScope}");
        Console.WriteLine();
        Console.WriteLine("--- Section 8d: null-safe extension ---");
        Console.WriteLine($"Notes missing: {notesMissing} | Notes blank: {notesBlank} | Promo missing: {promoMissing}");
        Console.WriteLine();
        Console.WriteLine("--- Section 8e: instance method precedence ---");
        Console.WriteLine($"Instance Trim result: \"{trimmedByInstance}\"");
        Console.WriteLine();
        Console.WriteLine("--- Section 8h: LINQ extensions preview ---");
        Console.WriteLine($"Premium line count: {premiumLineCount}");
        Console.WriteLine($"Premium subtotal: {premiumSubtotal.ToString("C", CultureInfo.CurrentCulture)}");
    }
}

/*
 * =============================================================================
 * QUICK REFERENCE — EXTENSION METHODS
 * =============================================================================
 *
 *  Declare (in static class):
 *    public static ReturnType MethodName(this ExtendedType receiver, …) { … }
 *
 *  Call (instance-style):
 *    receiver.MethodName(…);
 *
 *  Call (explicit static):
 *    StaticClass.MethodName(receiver, …);
 *
 *  Chain:
 *    receiver.ExtA().ExtB(arg);
 *
 *  Generic:
 *    public static T Clamp<T>(this T value, …) where T : IComparable<T>
 *
 *  Extend interface:
 *    public static decimal Total(this IEnumerable<OrderLine> lines) { … }
 *
 *  Rules:
 *    • static class + static method + this on first parameter only
 *    • namespace of static class must be in scope (using) for instance syntax
 *    • cannot access private members of extended type
 *    • instance methods on the type take precedence over extensions
 *    • null receiver allowed (reference types) — method body must handle null
 *    • not virtual — always static dispatch; no override/polymorphism
 *
 *  Common errors:
 *    CS1061  extension not found (missing using or wrong type)
 *    CS1105  extension method must be static
 *    CS1106  must be in non-nested static class
 *
 *  Related chapters:
 *    04. Functional Style Programming — delegates, lambdas (extension targets)
 *    05. Language Integrated Query — full depth on query operators
 *
 * =============================================================================
 */
