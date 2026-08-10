/*
 * =============================================================================
 * 04. OPERATORS AND EXPRESSIONS IN C# — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: Operators — symbols that perform actions on values (operands) — and
 *        expressions — combinations of operands and operators that evaluate
 *        to a single result.
 *
 * WHY IT MATTERS:
 *   Every calculation, comparison, and decision in C# is built from operators.
 *   Integer vs floating division, short-circuit logic, and precedence rules
 *   cause real production bugs when misunderstood. Mastering operators lets you
 *   write concise, correct business rules for pricing, permissions, and validation.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Operators, operands, and expressions
 *   2.  Arithmetic operators (+, -, *, /, %) and integer vs floating division
 *   3.  Assignment and compound assignment (=, +=, -=, …, &=, |=, …)
 *   4.  Comparison operators (==, !=, <, >, <=, >=)
 *   5.  Logical operators (&&, ||, !) and short-circuit evaluation
 *   6.  Bitwise operators (&, |, ^, ~, <<, >>) and feature flags
 *   7.  Ternary conditional (?:)
 *   8.  Null-coalescing (??, ??=)
 *   9.  Increment and decrement (++, --) — prefix vs postfix
 *  10.  Type-testing operators is and as (intro preview)
 *  11.  Operator precedence and associativity
 *
 * =============================================================================
 */

using System;

namespace OperatorsAndExpressions;

/*
 * =========================================================================
 * SECTION 1: OPERATORS, OPERANDS, AND EXPRESSIONS
 * =========================================================================
 *
 * An OPERATOR is a symbol that acts on one or more OPERANDS (values or
 * variables) and produces a result.
 *
 *   Expression:  unitPrice * quantity
 *                  ─────────   ────────
 *                  operand     operand
 *                       \       /
 *                        operator *
 *
 * An EXPRESSION is any valid combination of literals, variables, method
 * calls, and operators that evaluates to a single value:
 *
 *   int lineTotal = unitPrice * quantity;   ← right side is an expression
 *
 * C# groups operators into categories by purpose. The types and helpers below
 * demonstrate each category using a warehouse order scenario; Main prints
 * the combined results at the end.
 * -------------------------------------------------------------------------
 */

/*
 * =========================================================================
 * SECTION 2: ARITHMETIC OPERATORS
 * =========================================================================
 *
 *  Operator | Name           | Example (a=10, b=3) | Result
 *  ---------|----------------|---------------------|--------
 *  +        | Addition       | a + b               | 13
 *  -        | Subtraction    | a - b               | 7
 *  *        | Multiplication | a * b               | 30
 *  /        | Division       | a / b               | 3 (int!) or 3.333… (double)
 *  %        | Remainder      | a % b               | 1
 *
 * --- 2a. Integer vs floating division ---
 *
 * When BOTH operands are integer types (int, long, …), / performs INTEGER
 * division — the fractional part is TRUNCATED toward zero (not rounded):
 *
 *   7 / 2    →  3          (both int)
 *   7 / 2.0  →  3.5        (one operand is double → floating division)
 *   7.0m / 2 →  3.5m       (decimal division — preferred for money)
 *
 * --- 2b. Modulo (%) ---
 *
 * Returns the remainder after division — useful for pallets, pagination,
 * wrapping indices:
 *
 *   25 % 12  →  1   (25 items → 2 full cases + 1 leftover unit)
 *
 * --- 2c. Unary minus ---
 *
 * Negates a numeric value:  -balance  (same precedence group as +x)
 *
 * --- 2d. Numeric promotions ---
 *
 * Mixing int with double or decimal promotes the int to the wider type for
 * the operation. For money, keep operands decimal (49.99m) to avoid binary
 * float rounding surprises.
 * -------------------------------------------------------------------------
 */
public static class OrderArithmetic
{
    public static int ComputeTotalUnits(int caseCount, int unitsPerCase) =>
        caseCount * unitsPerCase; // multiplication — both operands int → int result

    public static decimal ComputeLineSubtotal(int totalUnits, decimal unitPrice) =>
        totalUnits * unitPrice; // int promoted to decimal for money-safe arithmetic

    public static (int FullCases, int Leftover) SplitLooseUnits(int looseUnits, int unitsPerCase)
    {
        int fullCases = looseUnits / unitsPerCase; // integer division — truncates toward zero
        int leftover = looseUnits % unitsPerCase;  // remainder after division
        return (fullCases, leftover);
    }

    public static (int AsInt, double AsDouble, decimal AsDecimal) CompareDivision()
    {
        int asInt = 7 / 2;           // both int → 3 (fraction discarded)
        double asDouble = 7.0 / 2;   // one double operand → floating division → 3.5
        decimal asDecimal = 7.0m / 2; // decimal division — preferred for currency
        return (asInt, asDouble, asDecimal);
    }

    public static decimal ApplyCredit(decimal balance, decimal payment) =>
        balance - payment; // subtraction on decimal operands
}

/*
 * =========================================================================
 * SECTION 3: ASSIGNMENT AND COMPOUND ASSIGNMENT OPERATORS
 * =========================================================================
 *
 * Simple assignment (=) stores the evaluated right-hand expression into the
 * left-hand variable. Chapter 02 introduced =; here we reinforce compound
 * forms that combine an operation with assignment:
 *
 *  Operator | Expands to     | Typical use
 *  ---------|----------------|------------------------------------------
 *  =        | x = value      | initial or replacement value
 *  +=       | x = x + y      | accumulate counts, totals
 *  -=       | x = x - y      | reduce stock, apply refunds
 *  *=       | x = x * y      | apply markup, scale quantities
 *  /=       | x = x / y      | split evenly (watch integer division)
 *  %=       | x = x % y      | wrap counters
 *
 * --- 3a. Bitwise compound assignment ---
 *
 *  &=   |=   ^=   <<=   >>=
 *
 * These apply the bitwise operator then assign — see Section 6 for examples
 * on permission flags.
 *
 * --- 3b. Assignment is an expression ---
 *
 * Assignment evaluates to the assigned value and associates RIGHT to LEFT:
 *
 *   int a, b;
 *   a = b = 5;    // b becomes 5, then a becomes 5
 *
 * Chained assignment is legal but use sparingly for readability.
 * -------------------------------------------------------------------------
 */
public static class PickingSession
{
    public static int ApplyPickAdjustments(int startingUnits)
    {
        int pickedUnits = startingUnits;
        pickedUnits += 5; // compound assignment — same as pickedUnits = pickedUnits + 5
        pickedUnits -= 3; // reduce count
        return pickedUnits;
    }

    public static decimal ApplyMarkup(decimal runningTotal, decimal multiplier)
    {
        runningTotal *= multiplier; // *= scales the running total in place
        return runningTotal;
    }

    public static int ChainAssignmentDemo()
    {
        int firstBin;
        int secondBin;
        firstBin = secondBin = 10; // assignment associates right-to-left — both become 10
        secondBin += 2;            // only secondBin changes; firstBin stays 10
        return firstBin + secondBin;
    }
}

/*
 * =========================================================================
 * SECTION 4: COMPARISON OPERATORS
 * =========================================================================
 *
 * Comparison operators compare two operands and return bool (true/false):
 *
 *  Operator | Meaning
 *  ---------|------------------
 *  ==       | equal to
 *  !=       | not equal to
 *  <        | less than
 *  >        | greater than
 *  <=       | less than or equal
 *  >=       | greater than or equal
 *
 * --- 4a. Value types ---
 *
 * For value types (int, decimal, bool, …), == and != compare stored VALUES.
 *
 * --- 4b. Reference types and string ---
 *
 * Reference types (arrays, most classes) compare REFERENCES with == by default
 * unless the type overrides Equals / operator==.
 *
 * string is special: == and != compare CHARACTER CONTENT (value-like behavior):
 *
 *   string a = "SKU-100";
 *   string b = "SKU-" + "100";
 *   a == b   →  true   (same text, even if different object references)
 *
 * --- 4c. No chained comparisons ---
 *
 * C# does NOT allow mathematical chaining like 0 < x < 100. Write:
 *
 *   x > 0 && x < 100
 *
 * Deep reference equality for custom classes is covered in OOP chapters.
 * -------------------------------------------------------------------------
 */
public static class OrderValidation
{
    public static bool MeetsMinimum(int totalUnits, int minimumOrder) =>
        totalUnits >= minimumOrder; // >= returns bool

    public static bool IsExactCaseLoad(int leftoverUnits) =>
        leftoverUnits == 0; // == compares values for int

    public static bool PriceDiffersFrom(decimal unitPrice, decimal benchmark) =>
        unitPrice != benchmark; // != — not equal

    public static bool SkuTextMatches(string skuA, string skuB) =>
        skuA == skuB; // string == compares character content, not reference

    public static bool QuantityInOpenRange(int quantity) =>
        quantity > 0 && quantity < 1000; // && — both conditions must be true (no chained 0 < x < 1000)
}

/*
 * =========================================================================
 * SECTION 5: LOGICAL OPERATORS
 * =========================================================================
 *
 *  Operator | Name        | True when
 *  ---------|-------------|------------------------------------------
 *  &&       | Logical AND | BOTH operands are true
 *  ||       | Logical OR  | AT LEAST ONE operand is true
 *  !        | Logical NOT | inverts true ↔ false
 *
 * --- 5a. Short-circuit evaluation ---
 *
 * && and || do not always evaluate the right-hand operand:
 *
 *   false && anything   →  false immediately (right side SKIPPED)
 *   true  || anything   →  true immediately (right side SKIPPED)
 *
 * Use this when the right side has side effects or could throw on null.
 *
 * --- 5b. Bitwise & and | with bool (contrast) ---
 *
 * When applied to bool operands, & and | still compute a logical result but
 * evaluate BOTH sides — no short-circuit. Rare in application code; useful
 * only when you intentionally need both sides to run.
 * -------------------------------------------------------------------------
 */
public static class ShippingRules
{
    public static bool CanShip(bool inStock, bool creditOk) =>
        inStock && creditOk; // short-circuit AND — both must be true

    public static bool WarehouseClosed(bool isWeekend, bool isHoliday) =>
        isWeekend || isHoliday; // short-circuit OR — either condition closes warehouse

    public static bool ExpressAllowed(int totalUnits) =>
        !(totalUnits > 100); // ! inverts the bool result of totalUnits > 100

    public static (bool AndShortCircuited, bool OrShortCircuited, int Counter) ShortCircuitDemo()
    {
        int sideEffectCounter = 0;
        bool andSkipped = false && (++sideEffectCounter > 0); // right side never runs — counter stays 0
        bool orSkipped = true || (++sideEffectCounter > 0);  // right side skipped — counter still 0
        return (andSkipped, orSkipped, sideEffectCounter);
    }

    public static (bool WithAnd, bool WithAmpersand, int Counter) NonShortCircuitDemo()
    {
        int counter = 0;
        bool withAnd = false && (++counter > 0); // && short-circuits — counter stays 0
        counter = 0;
        bool withAmpersand = false & (++counter > 0); // & always evaluates both sides — counter becomes 1
        return (withAnd, withAmpersand, counter);
    }
}

/*
 * =========================================================================
 * SECTION 6: BITWISE OPERATORS
 * =========================================================================
 *
 * Bitwise operators work on individual BITS of integral types (byte, int,
 * long, …) — essential for flags, permissions, encoding, and low-level math.
 *
 *  Operator | Name               | Example (5 = 0101, 3 = 0011)
 *  ---------|--------------------|----------------------------------
 *  &        | AND                | 5 & 3  →  1  (0001)
 *  |        | OR                 | 5 | 3  →  7  (0111)
 *  ^        | XOR (exclusive OR) | 5 ^ 3  →  6  (0110)
 *  ~        | NOT (flip bits)    | ~0 in int → all bits flipped
 *  <<       | Left shift         | 1 << 3 →  8  (0001 → 1000)
 *  >>       | Right shift        | 8 >> 1 →  4  (1000 → 0100)
 *
 * --- 6a. Feature flags pattern ---
 *
 *   const int CanRead    = 1 << 0;   // 0001  (1)
 *   const int CanWrite   = 1 << 1;   // 0010  (2)
 *   const int CanDelete  = 1 << 2;   // 0100  (4)
 *
 *   int role = CanRead | CanWrite;              // grant read + write
 *   bool canDelete = (role & CanDelete) != 0;   // test a flag
 *   role &= ~CanWrite;                          // remove write flag
 *
 * --- 6b. Shift notes ---
 *
 * << multiplies by 2^n (fast power-of-two scaling).
 * >> divides by 2^n for positive integers.
 * For signed types, >> performs sign-extending shift (fills with sign bit).
 *
 * --- 6c. Compound bitwise assignment ---
 *
 *   role |= CanAdmin;    // add flag
 *   role ^= CanWrite;    // toggle flag
 *   mask <<= 2;          // shift and assign
 * -------------------------------------------------------------------------
 */
public static class PermissionFlags
{
    public const int Read = 1 << 0;   // bit 0 — value 1
    public const int Write = 1 << 1;  // bit 1 — value 2
    public const int Admin = 1 << 2;  // bit 2 — value 4

    public static int GrantReadWrite() => Read | Write; // OR combines flags into one mask

    public static bool HasFlag(int role, int flag) => (role & flag) != 0; // AND tests whether flag bit is set

    public static int RemoveWriteViaXor(int role) => role ^ Write; // XOR toggles Write bit (adds if absent, removes if present)

    public static int RemoveWriteViaAndNot(int role) => role & ~Write; // ~Write inverts bits; AND clears Write flag

    public static int ApplyCompoundAssignments(int role)
    {
        role |= Admin;  // |= add Admin flag
        role ^= Write;  // ^= toggle Write flag
        role <<= 1;     // <<= shift left one bit (multiply by 2)
        role >>= 1;     // >>= shift right one bit (divide by 2)
        return role;
    }

    public static (int Doubled, int Halved) ShiftDemo(int value)
    {
        int doubled = value << 1; // left shift — multiply by 2
        int halved = value >> 1;  // right shift — divide by 2 (positive ints)
        return (doubled, halved);
    }

    public static int InvertLowByte(int value) => ~value & 0xFF; // ~ flips bits; & 0xFF keeps lowest 8 bits
}

/*
 * =========================================================================
 * SECTION 7: TERNARY CONDITIONAL OPERATOR (?:)
 * =========================================================================
 *
 * Syntax:  condition ? valueIfTrue : valueIfFalse
 *
 * Compact alternative to if/else when choosing between two expressions.
 * Both branches must be compatible types (or implicitly convertible).
 *
 *   string tier = totalUnits >= 100 ? "Bulk" : "Standard";
 *
 * --- 7a. Nested ternary ---
 *
 * Nesting is legal but hurts readability:
 *
 *   a ? b : c ? d : e    // parsed as a ? b : (c ? d : e)
 *
 * Prefer if/else when logic branches multiply.
 *
 * --- 7b. Ternary vs null-coalescing ---
 *
 * Use ?? when picking a non-null fallback; use ?: when the condition is
 * not limited to null checks.
 * -------------------------------------------------------------------------
 */
public static class PricingRules
{
    public static string ResolveShippingTier(int totalUnits) =>
        totalUnits >= 100 ? "Bulk" : "Standard"; // ternary — pick string by condition

    public static decimal ResolveDiscountRate(bool meetsMinimum) =>
        meetsMinimum ? 0.05m : 0.00m; // both branches must be compatible types (decimal here)

    public static string ResolvePriority(bool isRush, bool isBulk) =>
        isRush ? "Express" : isBulk ? "Bulk lane" : "Standard"; // nested ternary — parsed right-to-left

    public static decimal ComputeDiscountedTotal(decimal subtotal, decimal discountRate) =>
        subtotal - subtotal * discountRate; // * before - (multiplication has higher precedence)
}

/*
 * =========================================================================
 * SECTION 8: NULL-COALESCING OPERATORS (?? and ??=)
 * =========================================================================
 *
 * ??  — if the left operand is null, evaluate and use the right operand:
 *
 *   string label = nickname ?? legalName ?? "Guest";
 *
 * Chains left-to-right; stops at the first non-null value.
 *
 * ??= — assign only when the variable is currently null (C# 8+):
 *
 *   zoneCode ??= "Z1";
 *
 * Equivalent to:
 *
 *   if (zoneCode == null) zoneCode = "Z1";
 *
 * Works with nullable reference types (string?) and nullable value types
 * (int?). The right-hand side of ?? is not evaluated when the left is not null.
 * -------------------------------------------------------------------------
 */
public static class LabelDefaults
{
    public static string ResolveDisplayLabel(string? preferred, string? backup, string fallback) =>
        preferred ?? backup ?? fallback; // ?? chains left-to-right — first non-null wins

    public static string EnsureZoneCode(string? zoneCode)
    {
        zoneCode ??= "Z1"; // ??= assigns only when zoneCode is currently null
        return zoneCode;
    }

    public static int ResolvePickCount(int? requested, int defaultCount) =>
        requested ?? defaultCount; // ?? works with nullable value types (int?)
}

/*
 * =========================================================================
 * SECTION 9: INCREMENT AND DECREMENT (++, --)
 * =========================================================================
 *
 *  Form     | Name     | Behavior
 *  ---------|----------|--------------------------------------------------
 *  ++x      | Prefix   | Increment FIRST, then use the new value
 *  x++      | Postfix  | Use the CURRENT value, then increment
 *  --x      | Prefix   | Decrement first, then use
 *  x--      | Postfix  | Use current value, then decrement
 *
 * Examples starting with x = 5:
 *
 *   int a = ++x;   // x becomes 6, a is 6
 *   int b = x++;   // b is 6, then x becomes 7
 *
 * Prefer standalone x++ or x += 1 in loops; avoid embedding postfix forms in
 * complex expressions — easy to misread and debug.
 * -------------------------------------------------------------------------
 */
public static class PickSequence
{
    public static (int NextPick, int RecordedPick, int FinalSequence) PrefixPostfixDemo()
    {
        int pickSequence = 5;
        int nextPick = ++pickSequence;      // prefix — increment first, then assign (nextPick = 6)
        int recordedPick = pickSequence++;  // postfix — assign current value, then increment (recordedPick = 6, pickSequence → 7)
        return (nextPick, recordedPick, pickSequence);
    }

    public static int CountDown(int start)
    {
        int remaining = start;
        while (remaining-- > 0) // postfix decrement in condition — uses value then decrements
        {
            // loop body uses postfix decrement in condition
        }
        return remaining; // ends at -1 after start iterations
    }
}

/*
 * =========================================================================
 * SECTION 10: is AND as (INTRO PREVIEW)
 * =========================================================================
 *
 * COVERED IN DETAIL LATER → pattern matching and OOP chapters
 *
 * is  — checks whether an object is compatible with a type; returns bool:
 *
 *   if (item is string) { … }
 *   if (item is string s) { … }   // modern: test + assign
 *
 * as  — attempts a reference cast; returns null if the cast fails:
 *
 *   string? text = obj as string;
 *
 * Prefer pattern matching (is Type variable) over as + null check in modern C#.
 * -------------------------------------------------------------------------
 */
public static class SkuTypeChecks
{
    public static (bool IsText, string? SkuText) Inspect(object boxedSku)
    {
        bool isText = boxedSku is string;       // is — compatibility test, returns bool
        string? skuText = boxedSku as string;   // as — reference cast; null if incompatible
        return (isText, skuText);
    }
}

/*
 * =========================================================================
 * SECTION 11: OPERATOR PRECEDENCE AND ASSOCIATIVITY
 * =========================================================================
 *
 * When multiple operators appear in one expression, precedence decides which
 * runs first. Higher in the table → evaluated earlier.
 *
 *  Precedence (high → low)     | Operators
 *  -----------------------------|----------------------------------
 *  Primary                      | x++, x--, (type)cast, member access
 *  Unary                        | +, -, !, ~, ++x, --x
 *  Multiplicative               | *, /, %
 *  Additive                     | +, -
 *  Shift                        | <<, >>
 *  Relational                   | <, >, <=, >=, is, as
 *  Equality                     | ==, !=
 *  Logical AND                  | &
 *  Logical XOR                  | ^
 *  Logical OR                   | |
 *  Conditional AND              | &&
 *  Conditional OR               | ||
 *  Null-coalescing              | ??
 *  Ternary                      | ?:
 *  Assignment                   | =, +=, -=, …
 *
 * Parentheses ( ) override precedence — use them when clarity matters:
 *
 *   10 + 20 * 3   →  70    (multiplication first)
 *   (10 + 20) * 3 →  90    (parentheses first)
 *
 * Most binary operators associate LEFT to RIGHT; assignment (=) associates
 * RIGHT to LEFT:  a = b = 5;  // b becomes 5, then a
 *
 * Null-coalescing (??) binds looser than additive (+):
 *
 *   code?.Length ?? 0 + 1   →  parsed as  code?.Length ?? (0 + 1)
 * -------------------------------------------------------------------------
 */
public static class PrecedenceExamples
{
    public static (int WithoutParens, int WithParens) MultiplicationBeforeAddition() =>
        (10 + 20 * 3, (10 + 20) * 3); // * binds tighter than + — parentheses override

    public static decimal DiscountedSubtotal(decimal lineSubtotal, decimal discountRate) =>
        lineSubtotal - lineSubtotal * discountRate; // same precedence rule as numeric example above

    public static int NullCoalesceBeforeAddition(string? code) =>
        code?.Length ?? 0 + 1; // parsed as code?.Length ?? (0 + 1) — ?? binds looser than +
}

public class Program
{
    /*
     * =========================================================================
     * SECTION 12: DEMONSTRATION — Main orchestrates the chapter demo
     * =========================================================================
     *
     * Creates scenario inputs, calls each helper above, and prints a cohesive
     * warehouse order summary tying every operator family together.
     * -------------------------------------------------------------------------
     */
    public static void Main(string[] args)
    {
        int unitsPerCase = 12;
        decimal unitPrice = 49.99m;
        int caseCount = 3;
        int minimumOrder = 24;

        int totalUnits = OrderArithmetic.ComputeTotalUnits(caseCount, unitsPerCase); // SECTION 2 — arithmetic
        decimal lineSubtotal = OrderArithmetic.ComputeLineSubtotal(totalUnits, unitPrice);
        (int fullCases, int leftover) = OrderArithmetic.SplitLooseUnits(25, unitsPerCase); // deconstruct tuple
        (int divInt, double divDouble, decimal divDecimal) = OrderArithmetic.CompareDivision();

        int pickedUnits = PickingSession.ApplyPickAdjustments(10); // SECTION 3 — compound assignment
        decimal runningTotal = PickingSession.ApplyMarkup(100.00m, 1.18m);
        int chainSum = PickingSession.ChainAssignmentDemo();

        bool meetsMinimum = OrderValidation.MeetsMinimum(totalUnits, minimumOrder); // SECTION 4 — comparison
        bool isExactCase = OrderValidation.IsExactCaseLoad(leftover);
        bool priceChanged = OrderValidation.PriceDiffersFrom(unitPrice, 50.00m);
        bool sameSkuText = OrderValidation.SkuTextMatches("WH-001", "WH-" + "001"); // concatenated string — content still matches

        bool canShip = ShippingRules.CanShip(inStock: true, creditOk: false); // SECTION 5 — logical (short-circuit)
        bool warehouseClosed = ShippingRules.WarehouseClosed(isWeekend: false, isHoliday: true);
        bool expressAllowed = ShippingRules.ExpressAllowed(totalUnits);
        (bool andSkip, bool orSkip, int scCounter) = ShippingRules.ShortCircuitDemo();
        (bool withAnd, bool withAmp, int ampCounter) = ShippingRules.NonShortCircuitDemo();

        int pickerRole = PermissionFlags.GrantReadWrite(); // SECTION 6 — bitwise flags
        bool hasAdmin = PermissionFlags.HasFlag(pickerRole, PermissionFlags.Admin);
        int toggledRole = PermissionFlags.RemoveWriteViaXor(pickerRole);
        int andNotRole = PermissionFlags.RemoveWriteViaAndNot(pickerRole);
        int compoundRole = PermissionFlags.ApplyCompoundAssignments(pickerRole);
        (int doubled, int halved) = PermissionFlags.ShiftDemo(4);
        int invertedLow = PermissionFlags.InvertLowByte(0);

        string shippingTier = PricingRules.ResolveShippingTier(totalUnits); // SECTION 7 — ternary
        decimal discountRate = PricingRules.ResolveDiscountRate(meetsMinimum);
        string priority = PricingRules.ResolvePriority(isRush: false, isBulk: totalUnits >= 100);
        decimal orderTotal = PricingRules.ComputeDiscountedTotal(lineSubtotal, discountRate);

        string displayLabel = LabelDefaults.ResolveDisplayLabel(null, "Warehouse A", "Unknown"); // SECTION 8 — ??
        string zoneCode = LabelDefaults.EnsureZoneCode(null);
        int pickCount = LabelDefaults.ResolvePickCount(null, 1);

        (int nextPick, int recordedPick, int finalSequence) = PickSequence.PrefixPostfixDemo(); // SECTION 9 — ++/--
        int countdownEnd = PickSequence.CountDown(3);

        (bool isText, string? skuText) = SkuTypeChecks.Inspect("WH-002"); // SECTION 10 — is / as preview

        (int withoutParens, int withParens) = PrecedenceExamples.MultiplicationBeforeAddition(); // SECTION 11 — precedence
        decimal discountedSubtotal = PrecedenceExamples.DiscountedSubtotal(lineSubtotal, discountRate);
        int nullCoalesceLength = PrecedenceExamples.NullCoalesceBeforeAddition(null);
        int nullCoalesceWithCode = PrecedenceExamples.NullCoalesceBeforeAddition("WH");

        Console.WriteLine("=== 04. Operators and Expressions ===");
        Console.WriteLine();
        Console.WriteLine("--- Arithmetic ---");
        Console.WriteLine($"Cases: {caseCount} × {unitsPerCase} = {totalUnits} units, subtotal {lineSubtotal:C}");
        Console.WriteLine($"Split 25 units → {fullCases} cases + {leftover} loose");
        Console.WriteLine($"7/2 → int:{divInt}, double:{divDouble}, decimal:{divDecimal}");
        Console.WriteLine();
        Console.WriteLine("--- Assignment ---");
        Console.WriteLine($"Picked units after +/-: {pickedUnits}, markup total: {runningTotal:C}, chain sum: {chainSum}");
        Console.WriteLine();
        Console.WriteLine("--- Comparison ---");
        Console.WriteLine($"Meets min {minimumOrder}: {meetsMinimum}, exact case: {isExactCase}, SKU match: {sameSkuText}, price≠50: {priceChanged}");
        Console.WriteLine();
        Console.WriteLine("--- Logical ---");
        Console.WriteLine($"Can ship: {canShip}, closed: {warehouseClosed}, express OK: {expressAllowed}");
        Console.WriteLine($"Short-circuit &&/|| counter={scCounter} ({andSkip}/{orSkip}); & counter={ampCounter} ({withAnd}/{withAmp})");
        Console.WriteLine();
        Console.WriteLine("--- Bitwise ---");
        Console.WriteLine($"Role {pickerRole}, admin:{hasAdmin}, XOR remove write:{toggledRole}, AND-NOT:{andNotRole}, compound:{compoundRole}");
        Console.WriteLine($"Shift 4 → {doubled}/{halved}, ~0 low byte: {invertedLow}");
        Console.WriteLine();
        Console.WriteLine("--- Ternary & null-coalescing ---");
        Console.WriteLine($"Tier: {shippingTier}, priority: {priority}, discount: {discountRate:P0}, total: {orderTotal:C}");
        Console.WriteLine($"Label: {displayLabel}, zone: {zoneCode}, picks: {pickCount}");
        Console.WriteLine();
        Console.WriteLine("--- Increment / type test / precedence ---");
        Console.WriteLine($"Picks next/recorded/final: {nextPick}/{recordedPick}/{finalSequence}, countdown end: {countdownEnd}");
        Console.WriteLine($"SKU is string: {isText}, value: {skuText}");
        Console.WriteLine($"10+20*3={withoutParens}, (10+20)*3={withParens}, discounted={discountedSubtotal:C}");
        Console.WriteLine($"?? before +: null→{nullCoalesceLength}, \"WH\"→{nullCoalesceWithCode}");
    }
}

/*
 * =========================================================================
 * QUICK REFERENCE — OPERATORS AND EXPRESSIONS
 * =========================================================================
 *
 * --- Arithmetic ---
 *
 *  +  -  *  /  %     int/int → integer division (truncates toward zero)
 *                     mix with double/decimal → floating division
 *  -x                 unary negation
 *
 * --- Assignment ---
 *
 *  =  +=  -=  *=  /=  %=     arithmetic compound
 *  &=  |=  ^=  <<=  >>=      bitwise compound
 *
 * --- Comparison (returns bool) ---
 *
 *  ==  !=  <  >  <=  >=
 *  string == compares text content; no chained 0 < x < 100 — use &&
 *
 * --- Logical (short-circuit) ---
 *
 *  &&  ||  !          false && x skips x;  true || x skips x
 *  &   |   (bool)     both sides always evaluated
 *
 * --- Bitwise (integers) ---
 *
 *  &  |  ^  ~  <<  >>
 *  flags:  role = Read | Write;  test: (role & Admin) != 0;  remove: role & ~Flag
 *
 * --- Conditional ---
 *
 *  condition ? a : b     pick a or b
 *  a ?? b                a if not null, else b (chains left-to-right)
 *  a ??= b               assign b only when a is null
 *
 * --- Increment ---
 *
 *  ++x  x++  --x  x--    prefix changes before use; postfix uses then changes
 *
 * --- Type test (preview) ---
 *
 *  x is Type             bool compatibility test
 *  x as Type             cast or null (reference types)
 *
 * --- Precedence mnemonic ---
 *
 *  Parentheses → Unary → Mul/Div/Mod → Add/Sub → Shift → Compare →
 *  Equality → Bitwise & ^ | → && → || → ?? → ?: → Assignment
 *
 * --- Common mistakes ---
 *
 *  Mistake                         | Result
 *  --------------------------------|----------------------------------
 *  7 / 2 with int operands         | 3, not 3.5
 *  Using | instead of || for bool  | both sides always evaluated
 *  Complex a++ in one expression   | confusing postfix side effects
 *  Forgetting parentheses          | 10 + 20 * 3 is 70, not 90
 *  Chaining comparisons            | compile error — use && instead
 *
 * =========================================================================
 */
