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
 *   1.  Arithmetic operators (+, -, *, /, %)
 *   2.  Assignment operators (reinforcement from ch.02)
 *   3.  Comparison operators (==, !=, <, >, <=, >=)
 *   4.  Logical operators (&&, ||, !) and short-circuit evaluation
 *   5.  Bitwise operators (&, |, ^, ~, <<, >>)
 *   6.  Ternary conditional (?:)
 *   7.  Null-coalescing (??, ??=)
 *   8.  Increment and decrement (++, --) — prefix vs postfix
 *   9.  Type-testing operators is and as (intro preview)
 *  10.  Operator precedence and associativity
 *
 * =============================================================================
 */

using System;

namespace OperatorsAndExpressions;

public class Program
{
    public static void Main(string[] args)
    {
        /*
         * =========================================================================
         * SECTION 1: OPERATORS AND EXPRESSIONS
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
         * calls, and operators that evaluates to a single value.
         *
         *   int lineTotal = unitPrice * quantity;   ← right side is an expression
         *
         * C# groups operators into categories by purpose. The rest of this chapter
         * walks through each category with printed results from a warehouse order
         * scenario.
         * -------------------------------------------------------------------------
         */

        int unitsPerCase = 12;
        decimal unitPrice = 49.99m;
        int caseCount = 3;


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
         * division — the fractional part is TRUNCATED (not rounded):
         *
         *   7 / 2  →  3        (not 3.5)
         *   7 / 2.0  →  3.5    (one operand is double → floating division)
         *
         * Use % (modulo) for remainders — useful for pallets, pagination, wrapping:
         *
         *   25 % 12  →  1   (25 items → 2 full cases + 1 leftover unit)
         *
         * Unary minus negates a value:  -balance
         * -------------------------------------------------------------------------
         */

        int totalUnits = caseCount * unitsPerCase;          // 36
        decimal lineSubtotal = totalUnits * unitPrice;      // 1799.64

        int looseUnits = 25;
        int fullCases = looseUnits / unitsPerCase;          // 2  (integer division)
        int leftoverUnits = looseUnits % unitsPerCase;      // 1

        double avgAsDouble = 7.0 / 2;                       // 3.5
        int avgAsInt = 7 / 2;                               // 3  (truncated)

        Console.WriteLine("--- Arithmetic ---");
        Console.WriteLine($"Cases: {caseCount} × {unitsPerCase} units = {totalUnits} units, subtotal {lineSubtotal:C}");
        Console.WriteLine($"Split {looseUnits} units → {fullCases} full cases + {leftoverUnits} loose");
        Console.WriteLine($"7 / 2 as int: {avgAsInt}, as double: {avgAsDouble}");


        /*
         * =========================================================================
         * SECTION 3: ASSIGNMENT OPERATORS (REINFORCEMENT)
         * =========================================================================
         *
         * Chapter 02 introduced basic assignment (=). Compound assignment operators
         * combine an operation with assignment — they are shorthand, not new logic:
         *
         *   x += 5;    same as    x = x + 5;
         *   x -= 2;    same as    x = x - 2;
         *   x *= 3;    same as    x = x * 3;
         *   x /= 2;    same as    x = x / 2;
         *   x %= 4;    same as    x = x % 4;
         *
         * Bitwise compound forms (&=, |=, ^=, <<=, >>=) appear in Section 6.
         * -------------------------------------------------------------------------
         */

        int pickedUnits = 10;
        pickedUnits += 5;       // 15 — five more units picked
        pickedUnits -= 3;       // 12 — three put back

        decimal runningTotal = 100.00m;
        runningTotal *= 1.18m;  // apply 18% markup in one step


        /*
         * =========================================================================
         * SECTION 4: COMPARISON OPERATORS
         * =========================================================================
         *
         * Comparison operators compare two operands and return bool (true/false).
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
         * --- 4a. Value types vs reference types ---
         *
         * Value types (int, decimal, bool, …): == compares the stored VALUES.
         *
         * Reference types (string, arrays, custom classes): == compares REFERENCES
         * by default for most types — do two variables point to the same object?
         *
         * string is special: == compares CHARACTER CONTENT (value-like behavior):
         *
         *   string a = "SKU-100";
         *   string b = "SKU-" + "100";
         *   a == b   →  true   (same text, even if different object references)
         *
         * Deep reference equality for objects is covered in OOP chapters.
         * -------------------------------------------------------------------------
         */

        int minimumOrder = 24;
        bool meetsMinimum = totalUnits >= minimumOrder;
        bool isExactCase = leftoverUnits == 0;
        bool priceChanged = unitPrice != 50.00m;

        string skuA = "WH-001";
        string skuB = "WH-" + "001";
        bool sameSkuText = skuA == skuB;

        Console.WriteLine("--- Comparison ---");
        Console.WriteLine($"Meets minimum ({minimumOrder}): {meetsMinimum}, exact case load: {isExactCase}");
        Console.WriteLine($"SKU text equal: {sameSkuText}, price differs from 50.00: {priceChanged}");


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
         * This matters when the right side has side effects (method calls, increments)
         * or could throw (null access). Use && and || for conditions; & and | (bitwise)
         * evaluate BOTH sides — rarely used for bool logic in application code.
         * -------------------------------------------------------------------------
         */

        bool inStock = true;
        bool creditOk = false;
        bool canShip = inStock && creditOk;                 // false — credit fails

        bool isWeekend = false;
        bool isHoliday = true;
        bool warehouseClosed = isWeekend || isHoliday;      // true — holiday alone closes

        bool expressAllowed = !(totalUnits > 100);          // NOT heavy bulk

        int sideEffectCounter = 0;
        bool skippedRight = false && (++sideEffectCounter > 0);   // counter stays 0
        bool skippedRightOr = true || (++sideEffectCounter > 0);   // still 0

        Console.WriteLine("--- Logical ---");
        Console.WriteLine($"Can ship (stock+credit): {canShip}, closed (weekend|holiday): {warehouseClosed}");
        Console.WriteLine($"Express allowed: {expressAllowed}, short-circuit: {skippedRight}/{skippedRightOr}, counter: {sideEffectCounter}");


        /*
         * =========================================================================
         * SECTION 6: BITWISE OPERATORS
         * =========================================================================
         *
         * Bitwise operators work on individual BITS of integer types — essential
         * for flags, permissions, encoding, and low-level math.
         *
         *  Operator | Name              | Example (5 = 0101, 3 = 0011)
         *  ---------|-------------------|----------------------------------
         *  &        | AND               | 5 & 3  →  1  (0001)
         *  |        | OR                | 5 | 3  →  7  (0111)
         *  ^        | XOR (exclusive OR)| 5 ^ 3  →  6  (0110)
         *  ~        | NOT (flip bits)   | ~0     →  all bits flipped
         *  <<       | Left shift        | 1 << 3 →  8  (0001 → 1000)
         *  >>       | Right shift       | 8 >> 1 →  4  (1000 → 0100)
         *
         * Common pattern — feature flags stored in one int:
         *
         *   const int CanRead    = 1 << 0;   // 0001  (1)
         *   const int CanWrite   = 1 << 1;   // 0010  (2)
         *   const int CanDelete  = 1 << 2;   // 0100  (4)
         *
         *   int role = CanRead | CanWrite;           // grant read + write
         *   bool canDelete = (role & CanDelete) != 0;  // test a flag
         * -------------------------------------------------------------------------
         */

        const int FlagRead = 1 << 0;    // 1
        const int FlagWrite = 1 << 1;   // 2
        const int FlagAdmin = 1 << 2;   // 4

        int pickerRole = FlagRead | FlagWrite;
        bool hasAdmin = (pickerRole & FlagAdmin) != 0;
        int toggled = pickerRole ^ FlagWrite;   // remove write via XOR

        int doubledBits = 4 << 1;       // 8
        int halvedBits = 8 >> 1;        // 4

        Console.WriteLine("--- Bitwise ---");
        Console.WriteLine($"Picker role flags: {pickerRole}, has admin: {hasAdmin}, after XOR remove write: {toggled}");
        Console.WriteLine($"Shift: 4 << 1 = {doubledBits}, 8 >> 1 = {halvedBits}");


        /*
         * =========================================================================
         * SECTION 7: TERNARY CONDITIONAL OPERATOR (?:)
         * =========================================================================
         *
         * Syntax:  condition ? valueIfTrue : valueIfFalse
         *
         * Compact alternative to if/else when assigning or returning one of two
         * expressions. Both branches must be compatible types.
         *
         *   string tier = totalUnits >= 100 ? "Bulk" : "Standard";
         *
         * Nesting is legal but hurts readability — prefer if/else for complex cases.
         * -------------------------------------------------------------------------
         */

        string shippingTier = totalUnits >= 100 ? "Bulk" : "Standard";
        decimal discountRate = meetsMinimum ? 0.05m : 0.00m;


        /*
         * =========================================================================
         * SECTION 8: NULL-COALESCING OPERATORS (?? and ??=)
         * =========================================================================
         *
         * ??  — if left operand is null, use the right operand instead:
         *
         *   string label = nickname ?? legalName ?? "Guest";
         *
         * ??= — assign only when the variable is currently null (C# 8+):
         *
         *   nickname ??= "Guest";   // same as: if (nickname == null) nickname = "Guest";
         *
         * Works with nullable value types (int?) and reference types.
         * -------------------------------------------------------------------------
         */

        string? preferredLabel = null;
        string? backupLabel = "Warehouse A";
        string displayLabel = preferredLabel ?? backupLabel ?? "Unknown";

        string? zoneCode = null;
        zoneCode ??= "Z1";              // assign default zone once


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
         * Prefer standalone x++ or x += 1 in loops; avoid chaining in complex
         * expressions — easy to misread and debug.
         * -------------------------------------------------------------------------
         */

        int pickSequence = 5;
        int nextPick = ++pickSequence;    // pickSequence = 6, nextPick = 6
        int recordedPick = pickSequence++;  // recordedPick = 6, pickSequence = 7


        /*
         * =========================================================================
         * SECTION 10: is AND as (INTRO PREVIEW)
         * =========================================================================
         *
         * COVERED IN DETAIL LATER → advanced C# / pattern matching chapters
         *   (headline concepts only: type tests, safe casts, pattern matching)
         *
         * is  — checks whether an object is compatible with a type; returns bool:
         *
         *   if (item is string) { … }
         *
         * as  — attempts a cast; returns null if the cast fails (reference types):
         *
         *   string? text = obj as string;
         *
         * Modern C# adds pattern forms:  obj is string s  (test + assign in one step).
         * -------------------------------------------------------------------------
         */

        object boxedSku = "WH-002";
        bool isText = boxedSku is string;
        string? skuText = boxedSku as string;


        /*
         * =========================================================================
         * SECTION 11: OPERATOR PRECEDENCE AND ASSOCIATIVITY
         * =========================================================================
         *
         * When multiple operators appear in one expression, precedence decides
         * which runs first. Higher in the table → evaluated earlier.
         *
         *  Precedence (high → low)     | Operators
         *  -----------------------------|----------------------------------
         *  Primary                      | x++, x--, (type)cast
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
         * Most operators associate LEFT to RIGHT; assignment (=) associates RIGHT:
         *
         *   a = b = 5;   // b becomes 5 first, then a
         * -------------------------------------------------------------------------
         */

        int withoutParens = 10 + 20 * 3;        // 70
        int withParens = (10 + 20) * 3;         // 90

        decimal discountedSubtotal = lineSubtotal - lineSubtotal * discountRate;  // * before -


        /*
         * =========================================================================
         * SECTION 12: WAREHOUSE ORDER SUMMARY (ALL OPERATORS TOGETHER)
         * =========================================================================
         *
         * Pulls variables from earlier sections into one cohesive result line.
         * -------------------------------------------------------------------------
         */

        decimal orderTotal = lineSubtotal - lineSubtotal * discountRate;

        Console.WriteLine();
        Console.WriteLine("=== Warehouse Order Summary ===");
        Console.WriteLine($"Display zone: {displayLabel} (default applied: {zoneCode})");
        Console.WriteLine($"Units picked: {pickedUnits}, picks next/recorded/final: {nextPick}/{recordedPick}/{pickSequence}");
        Console.WriteLine($"Shipping: {shippingTier}, discount: {discountRate:P0}, total: {orderTotal:C}");
        Console.WriteLine($"Precedence demo: 10+20*3={withoutParens}, (10+20)*3={withParens}");
        Console.WriteLine($"Type check on SKU object: is string={isText}, value={skuText}");
        Console.WriteLine($"Running total after markup: {runningTotal:C}");
    }
}

/*
 * =========================================================================
 * QUICK REFERENCE — OPERATORS AND EXPRESSIONS
 * =========================================================================
 *
 * --- Arithmetic ---
 *
 *  +  -  *  /  %     int/int → integer division (truncates)
 *                     mixed with float/double/decimal → floating division
 *
 * --- Comparison (returns bool) ---
 *
 *  ==  !=  <  >  <=  >=
 *  string == compares text content
 *
 * --- Logical (short-circuit) ---
 *
 *  &&  ||  !          false && x  skips x;  true || x  skips x
 *
 * --- Bitwise (integers) ---
 *
 *  &  |  ^  ~  <<  >>     flags:  role = Read | Write;  test: (role & Admin) != 0
 *
 * --- Conditional ---
 *
 *  condition ? a : b     pick a or b
 *  a ?? b                a if not null, else b
 *  a ??= b               assign b only when a is null
 *
 * --- Increment ---
 *
 *  ++x  x++  --x  x--    prefix changes before use; postfix uses then changes
 *
 * --- Type test (preview) ---
 *
 *  x is Type             bool compatibility test
 *  x as Type             cast or null
 *
 * --- Precedence mnemonic ---
 *
 *  Parentheses → Unary → Mul/Div/Mod → Add/Sub → Shift → Compare →
 *  Equality → Bitwise → && → || → ?? → ?: → Assignment
 *
 * --- Common mistakes ---
 *
 *  Mistake                         | Result
 *  --------------------------------|----------------------------------
 *  7 / 2 with int operands         | 3, not 3.5
 *  Relying on | instead of ||      | both sides always evaluated
 *  Complex a++ in same expression  | confusing postfix side effects
 *  Forgetting parentheses          | 10 + 20 * 3 is 70, not 90
 *
 * =========================================================================
 */
