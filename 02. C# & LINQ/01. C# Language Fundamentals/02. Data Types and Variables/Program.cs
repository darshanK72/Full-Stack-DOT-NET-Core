/*
 * =============================================================================
 * 02. DATA TYPES AND VARIABLES IN C# — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: Variables (named storage) and the built-in data types every C#
 *        program uses — integers, floating-point, bool, char, string, and
 *        how values convert between types.
 *
 * WHY IT MATTERS:
 *   Variables hold the data your program works with. Choosing the right type
 *   (int vs decimal for money, string for text) prevents bugs, wasted memory,
 *   and rounding errors. This topic is the foundation for every line of C#
 *   you write afterward.
 *
 * WHAT YOU WILL LEARN:
 *   PART 1 — VARIABLES
 *     1. What a variable is and how declaration works
 *     2. Naming rules and conventions
 *     3. Scope and lifetime
 *     4. var and const
 *     5. Assignment operators
 *   PART 2 — BUILT-IN DATA TYPES (value + reference at this level)
 *     6. Integer types (byte through ulong)
 *     7. Floating-point types (float, double, decimal)
 *     8. bool, char, and string
 *     9. Default values
 *    10. Type conversion (implicit, cast, Parse, Convert, ToString)
 *    11. checked / unchecked overflow behavior
 *    12. Nullable value types
 *    13. Previews: object, boxing, strings, enum, stack vs heap
 *
 * =============================================================================
 */

using System;
using DataTypesAndVariables.Models;

namespace DataTypesAndVariables;

public class Program
{
    public static void Main(string[] args)
    {
        /*
         * =========================================================================
         * SECTION 1: WHAT IS A VARIABLE?
         * =========================================================================
         *
         * A variable is a NAMED storage location in memory that holds a value.
         *
         *   type name = value;
         *   ──── ────   ─────
         *    │    │       └── the data stored (literal, expression, or variable)
         *    │    └── identifier used in code to access the value
         *    └── the kind of data allowed (int, string, bool, etc.)
         *
         * Variables give meaningful names to data, allow values to change during
         * execution, and pass information between parts of a program.
         * -------------------------------------------------------------------------
         */

        int customerAge = 25;
        string customerName = "Darshan";


        /*
         * =========================================================================
         * SECTION 2: DECLARATION, INITIALIZATION & ASSIGNMENT
         * =========================================================================
         *
         * DECLARATION     → tell the compiler a variable exists (reserves memory)
         * INITIALIZATION  → give it a value for the first time
         * ASSIGNMENT      → change the value after it already exists
         *
         * Combined:   int score = 100;
         * Separate:   int score;  score = 100;
         *
         * Local variables MUST be assigned before reading:
         *   int x;
         *   Console.WriteLine(x);  → COMPILE ERROR CS0165
         *
         * Multiple variables of the SAME type on one line:
         *   int a = 1, b = 2, c = 3;
         * -------------------------------------------------------------------------
         */

        int itemCount;              // declaration only
        itemCount = 2;              // first assignment
        itemCount = 3;              // reassignment — value changed from 2 to 3

        int lineNumber = 1, copyCount = 1;  // multiple declaration of same type


        /*
         * =========================================================================
         * SECTION 3: NAMING RULES & CONVENTIONS
         * =========================================================================
         *
         * RULES (compiler enforces):
         *   - Start with a letter or underscore (_)
         *   - After first character: letters, digits, underscores only
         *   - Cannot be a C# keyword (int, class, string, …)
         *   - Case-sensitive: age and Age are different
         *
         * Valid:   age, _count, userName, totalAmount2
         * Invalid: 2age, user-name, class, my variable
         *
         * CONVENTIONS (Microsoft style — not enforced, but follow them):
         *
         *  Context              | Style         | Example
         *  ---------------------|---------------|------------------
         *  Local variables      | camelCase     | studentName, itemCount
         *  Method parameters    | camelCase     | firstName, maxRetries
         *  Private fields       | _camelCase    | _connectionString
         *  Public properties    | PascalCase    | FullName, IsActive
         *  Constants            | PascalCase    | MaxUsers, DefaultTimeout
         * -------------------------------------------------------------------------
         */

        int studentCount = 30;
        string productName = "Wireless Mouse";


        /*
         * =========================================================================
         * SECTION 4: VARIABLE SCOPE & LIFETIME
         * =========================================================================
         *
         * Scope    = where a variable name is visible and usable
         * Lifetime = how long the variable exists in memory
         *
         *  Scope type     | Declared inside          | Visible where
         *  ---------------|--------------------------|----------------------------
         *  Local variable | Method or code block {}  | Only inside that block
         *  Parameter      | Method signature         | Only inside that method
         *
         * Inner blocks can access outer variables; outer blocks cannot see inner ones.
         *
         *   int outer = 10;
         *   {
         *       int inner = 20;
         *       Console.WriteLine(outer + inner);  // OK
         *   }
         *   Console.WriteLine(inner);  → ERROR — inner is out of scope
         * -------------------------------------------------------------------------
         */

        int outerScope = 10;
        {
            int innerScope = 20;
            itemCount = outerScope + innerScope;  // 30 — inner sees outer; result used below
        }


        /*
         * =========================================================================
         * SECTION 5: var AND const
         * =========================================================================
         *
         * --- var ---
         * Compiler infers the type from the initializer. Still strongly typed.
         *
         *   var city = "Pune";   → string
         *   var count = 10;      → int
         *
         * Must initialize at declaration (var x; → compile error).
         *
         * --- const ---
         * Compile-time constant. Value never changes. Must initialize at declaration.
         * Implicitly static. Use for fixed values like tax rates or limits.
         *
         *   const decimal TaxRate = 0.18m;
         *
         * --- readonly ---
         * Assigned at declaration or in a constructor only — used on class fields
         * for run-time configuration that must not change after creation.
         * See Models/StoreConfig.cs for a readonly field set in the constructor.
         * -------------------------------------------------------------------------
         */

        var warehouseCity = "Pune";           // compiler infers string
        var unitsPerBox = 10;                 // compiler infers int

        const decimal TaxRate = 0.18m;
        const int MaxOrderQuantity = 9999;

        StoreConfig store = new StoreConfig("PUN-01", TaxRate);  // readonly fields set once in ctor


        /*
         * =========================================================================
         * SECTION 6: ASSIGNMENT OPERATORS
         * =========================================================================
         *
         *  Operator | Example  | Same as
         *  ---------|----------|-------------
         *  =        | x = 5    | x = 5
         *  +=       | x += 3   | x = x + 3
         *  -=       | x -= 2   | x = x - 2
         *  *=       | x *= 4   | x = x * 4
         *  /=       | x /= 2   | x = x / 2
         *  %=       | x %= 3   | x = x % 3
         *  ++       | x++      | x = x + 1  (post-increment)
         *  --       | x--      | x = x - 1  (post-decrement)
         *
         * ++x adds 1 before the value is used; x++ uses the value first, then adds 1.
         * -------------------------------------------------------------------------
         */

        int counter = 10;
        counter += 5;                         // 15
        counter -= 3;                         // 12
        counter *= 2;                         // 24
        counter++;                            // 25 — used as display line count below


        /*
         * =========================================================================
         * SECTION 6a: LITERALS — FIXED VALUES IN SOURCE CODE
         * =========================================================================
         *
         * A literal is a value written directly in code (not computed at run time).
         *
         *  Type     | Literal examples              | Notes
         *  ---------|-------------------------------|----------------------------
         *  int      | 42, 1_000                     | underscores for readability
         *  float    | 3.14f, 0.5F                   | suffix f or F required
         *  double   | 3.14, 1.5e3                   | default for 3.14 without suffix
         *  decimal  | 99.99m, 0.1M                  | suffix m or M required for money
         *  char     | 'A', '\n', '\u0041'           | single quotes
         *  string   | "hello", ""                   | double quotes; "" is empty
         *  bool     | true, false                   | lowercase keywords
         *  null     | null                          | no value — reference or int? only
         *
         * COMPILE ERROR: 'A' is char; "A" is string (CS1012 if you swap quotes wrongly).
         * -------------------------------------------------------------------------
         */

        int literalInt = 42;
        float literalFloat = 3.14f;
        double literalDouble = 3.14;
        decimal literalDecimal = 99.99m;
        char literalChar = 'A';
        string literalString = "hello";
        bool literalBool = true;
        int? literalNull = null;


        /*
         * =========================================================================
         * SECTION 7: INTEGER TYPES (Whole Numbers)
         * =========================================================================
         *
         * Integer types are VALUE types — stored on the stack (see stack vs heap
         * preview in Section 14a).
         * They hold whole numbers with no fractional part.
         *
         *  Type   | Size   | Signed? | Range (approx.)
         *  -------|--------|---------|------------------------------------------
         *  sbyte  | 8 bit  | Yes     | -128 to 127
         *  byte   | 8 bit  | No      | 0 to 255
         *  short  | 16 bit | Yes     | -32,768 to 32,767
         *  ushort | 16 bit | No      | 0 to 65,535
         *  int    | 32 bit | Yes     | ±2.1 billion  ← default choice
         *  uint   | 32 bit | No      | 0 to ~4.3 billion
         *  long   | 64 bit | Yes     | very large signed range
         *  ulong  | 64 bit | No      | 0 to very large unsigned range
         *
         * Literal suffixes:  100u  (uint)   100L  (long)   100UL  (ulong)
         * Underscores improve readability: 1_400_000_000
         * -------------------------------------------------------------------------
         */

        sbyte temperatureOffset = -5;
        byte categoryCode = 12;               // 0–255: ages, RGB channels, raw bytes
        byte accentRed = 220;

        short orderYear = 2026;
        ushort servicePort = 8080;            // 0–65535: common for network ports

        int population = 1_400_000_000;       // most common integer type
        uint transactionChecksum = 4_294_967_295u;

        long orderId = 9_007_199_254_740_993L;
        ulong invoiceBytes = 18_446_744_073_709_551_615UL;


        /*
         * =========================================================================
         * SECTION 8: FLOATING-POINT TYPES (Decimal Part)
         * =========================================================================
         *
         *  Type    | Size   | Precision    | Suffix | Best for
         *  --------|--------|--------------|--------|----------------------------
         *  float   | 32 bit | ~6–9 digits  | f / F  | Graphics, large float arrays
         *  double  | 64 bit | ~15–16 digits| (none) | General math, science
         *  decimal | 128 bit| ~28–29 digits| m / M  | Money, financial totals
         *
         * float and double use binary floating-point → tiny rounding errors are normal.
         * decimal uses base-10 arithmetic → exact for money (0.1m + 0.2m = 0.3m).
         *
         *   (double)0.1 + (double)0.2  →  0.30000000000000004  ← wrong for finance
         *   0.1m + 0.2m                →  0.3m                  ← correct
         * -------------------------------------------------------------------------
         */

        float productWeightKg = 0.085f;       // suffix f required for float
        double averageRating = 4.7;           // default for literals like 3.14
        double distanceMeters = 1.5e6;        // scientific notation: 1,500,000

        decimal unitPrice = 799.50m;          // suffix m required for decimal
        decimal lineSubtotal = unitPrice * itemCount;
        decimal taxAmount = lineSubtotal * TaxRate;
        decimal orderTotal = lineSubtotal + taxAmount;


        /*
         * =========================================================================
         * SECTION 8a: checked AND unchecked — INTEGER OVERFLOW
         * =========================================================================
         *
         * By default, integer arithmetic in C# is unchecked — overflow wraps silently
         * (two's complement). checked context throws OverflowException instead.
         *
         *   unchecked { int.MaxValue + 1 }  → wraps to int.MinValue
         *   checked  { int.MaxValue + 1 }  → OverflowException
         *
         * Use checked for financial counters, IDs, or anywhere wrap-around is a bug.
         * -------------------------------------------------------------------------
         */

        int overflowBase = int.MaxValue;
        bool overflowCaught = false;
        try
        {
            checked
            {
                int _ = overflowBase + 1;
            }
        }
        catch (OverflowException)
        {
            overflowCaught = true;
        }

        int wrappedIncrement = unchecked(overflowBase + 1);


        /*
         * =========================================================================
         * SECTION 9: bool (Boolean)
         * =========================================================================
         *
         * Stores true or false. Default: false. Used for conditions and flags.
         *
         * Logical operators:
         *   &&  AND  — both must be true
         *   ||  OR   — at least one must be true
         *   !   NOT  — reverses true ↔ false
         * -------------------------------------------------------------------------
         */

        bool isInStock = true;
        bool isAdult = categoryCode >= 18;    // expression producing bool
        bool isBulkOrder = itemCount >= MaxOrderQuantity / 10;


        /*
         * =========================================================================
         * SECTION 10: char (Single Character)
         * =========================================================================
         *
         * Stores one UTF-16 character. Size: 2 bytes. Default: '\0'
         * Literal syntax: single quotes  'A'  (double quotes would be string)
         * -------------------------------------------------------------------------
         */

        char skuPrefix = 'M';
        char gradeLetter = (char)(skuPrefix + 1);  // char arithmetic → 'N'
        char tabChar = '\t';
        char unicodeA = '\u0041';                  // Unicode escape → 'A'


        /*
         * --- 10a. enum (preview) ---
         *
         * enum names a fixed set of related constants (order status, day of week).
         *
         *   enum ShipmentStatus { Pending, Shipped, Delivered }
         *
         * COVERED IN DETAIL LATER → 07. Methods / OOP chapters
         *   (underlying type, flags enum, parsing enum from strings)
         * -------------------------------------------------------------------------
         */

        ShipmentStatus shipment = ShipmentStatus.Pending;


        /*
         * =========================================================================
         * SECTION 11: string (Text — Preview)
         * =========================================================================
         *
         * string holds text and is a REFERENCE type (see Section 14a). Immutable.
         * Hello World introduced string literals; here we use them in the order demo.
         *
         * COVERED IN DETAIL LATER → 06. Strings
         *   (interpolation, verbatim strings, String methods, StringBuilder)
         * -------------------------------------------------------------------------
         */

        string skuCode = $"{skuPrefix}{categoryCode:D2}-001";   // minimal interpolation for SKU
        string invoicePath = @"C:\Orders\invoices\order.txt"; // verbatim path


        /*
         * =========================================================================
         * SECTION 12: DEFAULT VALUES
         * =========================================================================
         *
         *  Type          | Default
         *  --------------|----------
         *  int, long…    | 0
         *  float, double | 0.0
         *  bool          | false
         *  char          | '\0'
         *  decimal       | 0.0m
         *  string        | null
         *
         * default keyword or default(T) returns the default for any type.
         * Local variables must still be assigned before use — default() in an
         * assignment is fine; an unassigned local is not.
         * -------------------------------------------------------------------------
         */

        int defaultInteger = default;         // 0
        bool defaultFlag = default;           // false
        string? defaultLabel = default;       // null (? marks nullable reference)


        /*
         * =========================================================================
         * SECTION 13: TYPE CONVERSION
         * =========================================================================
         *
         * --- 13a. Implicit conversion (widening — automatic, safe) ---
         *   int → long, float, double, decimal
         *   byte → short → int → long → float → double
         *
         * --- 13b. Explicit conversion — casting (narrowing — may lose data) ---
         *   double d = 99.99;
         *   int i = (int)d;   → 99  (fraction truncated, not rounded)
         *
         * --- 13c. Parse & TryParse (string → numeric) ---
         *   int.Parse("42")                  → 42  (throws FormatException if invalid)
         *   int.TryParse("abc", out int n)   → false, n = 0  (safe for user input)
         *
         * --- 13d. Convert class ---
         *   Convert.ToInt32("123")  → 123
         *   Convert.ToInt32(null)     → 0
         *
         * --- 13f. Deep dive deferred ---
         *
         * COVERED IN DETAIL LATER → 05. Type Conversion and Casting
         *   (Convert overloads, culture-specific Parse, user-defined conversions)
         * -------------------------------------------------------------------------
         */

        long itemCountAsLong = itemCount;                     // implicit int → long
        int totalRoundedDown = (int)orderTotal;               // explicit decimal → int
        int parsedQuantity = int.Parse("2");
        bool parseFailed = int.TryParse("abc", out int badParse);
        int convertedBonus = Convert.ToInt32("50");
        string priceText = unitPrice.ToString("F2");
        string countText = itemCount.ToString();


        /*
         * =========================================================================
         * SECTION 14a: STACK VS HEAP (PREVIEW)
         * =========================================================================
         *
         * Value types (int, bool, decimal, etc.) usually live on the stack — fast, scoped
         * to the method. Reference types (string, classes) store a reference on
         * the stack pointing to data on the heap. Custom struct types are covered later.
         *
         * COVERED IN DETAIL LATER → 00. .NET Framework Architecture (CLR ch.06)
         * COVERED IN DETAIL LATER → 08. Memory Management
         * -------------------------------------------------------------------------
         */


        /*
         * =========================================================================
         * SECTION 14: object AND BOXING (PREVIEW)
         * =========================================================================
         *
         * object is the root type of all types. Assigning a value type to object
         * BOXES a copy onto the heap; casting back UNBOXES it.
         *
         *   object boxed = 42;
         *   int n = (int)boxed;
         *
         * COVERED IN DETAIL LATER → 08. Memory Management
         *   (boxing cost, when to avoid, pattern matching on object)
         * -------------------------------------------------------------------------
         */

        object boxedQuantity = itemCount;
        int unboxedQuantity = (int)boxedQuantity;
        bool quantityIsInt = boxedQuantity is int;


        /*
         * =========================================================================
         * SECTION 15: NULLABLE VALUE TYPES
         * =========================================================================
         *
         * Value types normally cannot be null. Append ? to allow null:
         *
         *   int? score = null;     same as Nullable<int>
         *
         *   score.HasValue  → false when null
         *   score ?? 0      → null-coalescing: substitute 0 when null
         *   score ??= 10    → assign 10 only if currently null
         * -------------------------------------------------------------------------
         */

        int? loyaltyPoints = null;
        int? confirmedPoints = 85;
        int earnedPoints = loyaltyPoints ?? 0;
        int totalPoints = earnedPoints + (confirmedPoints ?? 0);


        /*
         * =========================================================================
         * SECTION 16: RUNTIME TYPE CHECKS & RANGE CONSTANTS
         * =========================================================================
         *
         *   value is int         → pattern match: true/false
         *   typeof(int)          → Type object at compile time
         *   int.MaxValue         → 2,147,483,647
         *   int.MinValue         → -2,147,483,648
         * -------------------------------------------------------------------------
         */

        Type integerType = typeof(int);


        /*
         * =========================================================================
         * SECTION 17: ORDER SUMMARY — PUTTING IT ALL TOGETHER
         * =========================================================================
         *
         * The lines below print a receipt built from every variable above.
         * Each WriteLine uses real computed values — nothing is declared only
         * for show.
         * -------------------------------------------------------------------------
         */

        Console.WriteLine("=== Order Summary ===");
        Console.WriteLine($"Customer: {customerName} (age {customerAge}, adult: {isAdult})");
        Console.WriteLine($"Store: {store.StoreCode}  Tax rate (readonly): {store.StandardTaxRate:P0}");
        Console.WriteLine($"Product:  {productName}  SKU: {skuCode}  Grade: {gradeLetter}  Shipment: {shipment}");
        Console.WriteLine($"Year: {orderYear}  Order ID: {orderId}  Lines printed: {counter}");
        Console.WriteLine($"Qty: {unboxedQuantity} (text: {countText})  Units/box: {unitsPerBox}");
        Console.WriteLine($"Unit price: {priceText} INR  Subtotal: {lineSubtotal:C}  Tax: {taxAmount:C}");
        Console.WriteLine($"Order total: {orderTotal:C}  Rounded (int cast): {totalRoundedDown}");
        Console.WriteLine($"In stock: {isInStock}  Bulk order: {isBulkOrder}  Points: {totalPoints}");
        Console.WriteLine($"Weight: {productWeightKg} kg  Rating: {averageRating:F1}  Dist: {distanceMeters:N0} m");
        Console.WriteLine($"Temp offset: {temperatureOffset}  RGB accent: {accentRed}  Port: {servicePort}");
        Console.WriteLine($"Population ref: {population:N0}  Checksum: {transactionChecksum}");
        Console.WriteLine($"Invoice size (bytes): {invoiceBytes}  Parsed qty: {parsedQuantity}");
        Console.WriteLine($"Bonus (Convert): {convertedBonus}  Parse failed: {parseFailed} (val={badParse})");
        Console.WriteLine($"Overflow caught: {overflowCaught}  Unchecked wrap: {wrappedIncrement}");
        Console.WriteLine($"Literals — int:{literalInt} bool:{literalBool} char:{literalChar} null?={literalNull.HasValue}");
        Console.WriteLine($"Qty is int: {quantityIsInt}  Type: {integerType.Name}  Range: [{int.MinValue} .. {int.MaxValue}]");
        Console.WriteLine($"Implicit to long: {itemCountAsLong}");
        Console.WriteLine($"Defaults — int: {defaultInteger}, bool: {defaultFlag}, label: {defaultLabel ?? "null"}");
        Console.WriteLine($"Scope sum used as qty earlier: {itemCount}  Students: {studentCount}");
        Console.WriteLine($"Warehouse: {warehouseCity}  Line/copy: {lineNumber}/{copyCount}");
        Console.WriteLine($"Chars — tab code: {(int)tabChar}, unicode A: {unicodeA}");
        Console.WriteLine($"Path: {invoicePath}");
        Console.WriteLine($"Literal string sample: {literalString}  float:{literalFloat} double:{literalDouble} dec:{literalDecimal}");
    }
}

enum ShipmentStatus
{
    Pending,
    Shipped,
    Delivered
}

/*
 * =========================================================================
 * QUICK REFERENCE — PRIMITIVE DATA TYPES & VARIABLES
 * =========================================================================
 *
 * --- Integer ranges (signed unless noted) ---
 *
 *  Type   | Min (approx.)              | Max (approx.)
 *  -------|----------------------------|----------------------------
 *  sbyte  | -128                       | 127
 *  byte   | 0                          | 255
 *  short  | -32,768                    | 32,767
 *  ushort | 0                          | 65,535
 *  int    | -2,147,483,648             | 2,147,483,647
 *  uint   | 0                          | 4,294,967,295
 *  long   | -9.2 × 10^18               | 9.2 × 10^18
 *  ulong  | 0                          | 1.8 × 10^19
 *
 * --- Which type to use? ---
 *
 *  Scenario                          | Recommended Type
 *  ----------------------------------|----------------------------------
 *  General counting, indexing        | int
 *  Very large whole numbers          | long
 *  Memory-sensitive byte data        | byte
 *  Age, percentage 0–255           | byte
 *  Network port                      | ushort
 *  General decimal math              | double
 *  Money, prices, financial totals   | decimal
 *  Memory-sensitive float arrays     | float
 *  True/false flags                  | bool
 *  Single character                  | char
 *  Text, names, messages             | string
 *  Optional numeric field            | int?, decimal?, etc.
 *  Heterogeneous data                | object (prefer generics when possible)
 *
 * --- Variable naming ---
 *
 *  Local / parameter  →  camelCase     studentName
 *  Private field      →  _camelCase    _totalCount
 *  Public member      →  PascalCase    FullName
 *  Constant           →  PascalCase    MaxRetries
 *
 * --- Conversion cheat sheet ---
 *
 *  Direction            | Mechanism
 *  ---------------------|------------------------------------------
 *  Smaller → larger     | Implicit (automatic)
 *  Larger → smaller     | Explicit cast (type)value
 *  string → number      | Parse / TryParse / Convert.To___
 *  anything → string    | ToString() or string interpolation
 *
 * --- Common compile errors ---
 *
 *  Mistake                    | Error
 *  ---------------------------|----------------------------------
 *  Read before assignment     | CS0165 unassigned local variable
 *  Invalid cast at runtime      | InvalidCastException on bad unbox
 *  Parse invalid string         | FormatException (use TryParse instead)
 *  Integer overflow in checked  | OverflowException
 *
 * =========================================================================
 */
