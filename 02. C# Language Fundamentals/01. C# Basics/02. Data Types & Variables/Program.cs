/*
 * =============================================================================
 * 02. DATA TYPES AND VARIABLES IN C#
 * =============================================================================
 *
 * TOPIC: Variables (named storage) and the built-in data types every C#
 *        program uses — value types vs reference types, integers, floating-
 *        point, bool, char, string, var, const, readonly, nullable, and default.
 *
 * WHY IT MATTERS:
 *   Variables hold the data your program works with. Choosing the right type
 *   (int vs decimal for money, string for text) prevents bugs, wasted memory,
 *   and rounding errors. This topic is the foundation for every line of C#
 *   you write afterward.
 *
 * WHAT YOU WILL LEARN:
 *   1. What a variable is — declaration, initialization, assignment
 *   2. Naming rules and conventions
 *   3. Scope and lifetime
 *   4. var, const, and readonly
 *   5. Assignment operators and literals
 *   6. Integer, floating-point, bool, char, and string types
 *   7. Default values and the default keyword
 *   8. Value types vs reference types
 *   9. Nullable value types (int?) and nullable reference types (string?)
 *  10. checked / unchecked integer overflow
 *  11. Previews: type conversion, object/boxing, enum, stack vs heap
 *
 * =============================================================================
 */

using System;

namespace DataTypesAndVariables;

/*
 * =========================================================================
 * SECTION 1: enum (PREVIEW)
 * =========================================================================
 *
 * An enum names a fixed set of related constants (order status, day of week).
 *
 *   enum ShipmentStatus { Pending, Shipped, Delivered }
 *
 * Underlying integer values start at 0 unless you assign them. enum is a
 * value type — each variable stores its own copy of the enum value.
 *
 * COVERED IN DETAIL LATER → 02. Object Oriented Programming
 *   (flags enum, parsing from strings, underlying type customization)
 * -------------------------------------------------------------------------
 */
enum ShipmentStatus
{
    Pending,    // underlying int value 0 (default for first enum member)
    Shipped,    // 1
    Delivered   // 2
}

/*
 * =========================================================================
 * SECTION 2: const AND readonly
 * =========================================================================
 *
 * Both prevent changing a value after it is set — but at different stages:
 *
 *   Keyword   | When value is fixed     | Where it applies
 *   ----------|-------------------------|----------------------------------
 *   const     | Compile time            | Fields and local variables
 *   readonly  | Run time (ctor or decl) | Fields only (not local variables)
 *
 * --- const ---
 * Must be initialized at declaration. Implicitly static on class fields.
 * Use for mathematical constants, limits, and fixed rates known at compile time.
 *
 *   const decimal TaxRate = 0.18m;
 *
 * --- readonly ---
 * Assigned at declaration OR once in a constructor. Use when the value is
 * known only when the object is created (configuration, IDs from a database).
 *
 * StoreConfig below holds per-store settings set once in the constructor.
 * After construction, StoreCode and StandardTaxRate cannot be reassigned.
 * -------------------------------------------------------------------------
 */
public class StoreConfig
{
    public readonly string StoreCode;       // set once in ctor — cannot reassign after construction
    public readonly decimal StandardTaxRate;

    public StoreConfig(string storeCode, decimal standardTaxRate)
    {
        StoreCode = storeCode;               // readonly fields assigned only here (or at declaration)
        StandardTaxRate = standardTaxRate;
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        /*
         * =====================================================================
         * SECTION 3: WHAT IS A VARIABLE?
         * =====================================================================
         *
         * A variable is a NAMED storage location that holds a value.
         *
         *   type name = value;
         *   ──── ────   ─────
         *    │    │       └── literal, expression, or another variable
         *    │    └── identifier used in code
         *    └── kind of data allowed (int, string, bool, …)
         *
         * Variables give meaningful names to data, allow values to change during
         * execution, and pass information between parts of a program.
         * ---------------------------------------------------------------------
         */

        int customerAge = 25;                    // declaration + initialization in one statement
        string customerName = "Darshan";


        /*
         * =====================================================================
         * SECTION 4: DECLARATION, INITIALIZATION & ASSIGNMENT
         * =====================================================================
         *
         * DECLARATION     → tell the compiler a variable exists
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
         * ---------------------------------------------------------------------
         */

        int itemCount;              // declaration only — must assign before read
        itemCount = 2;              // first assignment
        itemCount = 3;              // reassignment — value changed from 2 to 3

        int lineNumber = 1, copyCount = 1; // multiple vars of same type on one line


        /*
         * =====================================================================
         * SECTION 5: NAMING RULES & CONVENTIONS
         * =====================================================================
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
         * CONVENTIONS (Microsoft style):
         *
         *  Context              | Style         | Example
         *  ---------------------|---------------|------------------
         *  Local variables      | camelCase     | studentName, itemCount
         *  Method parameters    | camelCase     | firstName, maxRetries
         *  Private fields       | _camelCase    | _connectionString
         *  Public properties    | PascalCase    | FullName, IsActive
         *  Constants            | PascalCase    | MaxUsers, DefaultTimeout
         * ---------------------------------------------------------------------
         */

        int studentCount = 30;
        string productName = "Wireless Mouse";


        /*
         * =====================================================================
         * SECTION 6: VARIABLE SCOPE & LIFETIME
         * =====================================================================
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
         * ---------------------------------------------------------------------
         */

        int outerScope = 10;
        {
            int innerScope = 20;                              // visible only inside this block
            itemCount = outerScope + innerScope;  // 30 — result used in summary output
        }


        /*
         * =====================================================================
         * SECTION 7: var — TYPE INFERENCE
         * =====================================================================
         *
         * var tells the compiler to infer the type from the initializer.
         * The variable is still strongly typed — var is not dynamic.
         *
         *   var city = "Pune";   → string
         *   var count = 10;      → int
         *
         * Must initialize at declaration (var x; → compile error).
         * Use when the type is obvious from the right-hand side; write the
         * explicit type when it improves readability (decimal totals, long IDs).
         * ---------------------------------------------------------------------
         */

        var warehouseCity = "Pune";   // compiler infers string from literal
        var unitsPerBox = 10;         // compiler infers int


        /*
         * =====================================================================
         * SECTION 8: const — COMPILE-TIME CONSTANTS
         * =====================================================================
         *
         * const values are fixed at compile time and never change.
         * Must initialize at declaration. Implicitly static on class fields.
         *
         *   const decimal TaxRate = 0.18m;
         *
         * readonly (Section 2) is for run-time values set in a constructor.
         * ---------------------------------------------------------------------
         */

        const decimal TaxRate = 0.18m;        // compile-time constant — same for all code
        const int MaxOrderQuantity = 9999;

        StoreConfig store = new StoreConfig("PUN-01", TaxRate); // readonly fields set in ctor


        /*
         * =====================================================================
         * SECTION 9: ASSIGNMENT OPERATORS
         * =====================================================================
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
         *
         * COVERED IN DETAIL LATER → 04. Operators and Expressions
         *   (prefix vs postfix in expressions, compound assignment edge cases)
         * ---------------------------------------------------------------------
         */

        int counter = 10;
        counter += 5;                         // 15 — same as counter = counter + 5
        counter -= 3;                         // 12
        counter *= 2;                         // 24
        counter++;                            // 25 — post-increment


        /*
         * =====================================================================
         * SECTION 9a: LITERALS — FIXED VALUES IN SOURCE CODE
         * =====================================================================
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
         *  null     | null                          | reference or nullable value only
         *
         * COMPILE ERROR: 'A' is char; "A" is string (CS1012 if you swap quotes wrongly).
         * ---------------------------------------------------------------------
         */

        int literalInt = 42;
        float literalFloat = 3.14f;      // f suffix required for float literal
        double literalDouble = 3.14;     // default floating literal is double
        decimal literalDecimal = 99.99m; // m suffix required for decimal (money)
        char literalChar = 'A';          // single quotes — char, not string
        string literalString = "hello";
        bool literalBool = true;
        int? literalNull = null;         // nullable value type — can hold null


        /*
         * =====================================================================
         * SECTION 10: INTEGER TYPES (Whole Numbers)
         * =====================================================================
         *
         * Integer types are VALUE types — each variable holds its own numeric copy.
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
         * ---------------------------------------------------------------------
         */

        sbyte temperatureOffset = -5;
        byte categoryCode = 12;
        byte accentRed = 220;

        short orderYear = 2026;
        ushort servicePort = 8080;

        int population = 1_400_000_000;              // underscore separators for readability
        uint transactionChecksum = 4_294_967_295u;   // u suffix — unsigned int

        long orderId = 9_007_199_254_740_993L;     // L suffix — 64-bit long
        ulong invoiceBytes = 18_446_744_073_709_551_615UL;


        /*
         * =====================================================================
         * SECTION 11: FLOATING-POINT TYPES (Decimal Part)
         * =====================================================================
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
         * ---------------------------------------------------------------------
         */

        float productWeightKg = 0.085f;   // f suffix — float literal
        double averageRating = 4.7;
        double distanceMeters = 1.5e6;    // scientific notation — 1.5 × 10^6

        decimal unitPrice = 799.50m;
        decimal lineSubtotal = unitPrice * itemCount;  // decimal × int — exact money math
        decimal taxAmount = lineSubtotal * TaxRate;    // decimal × decimal — no binary rounding
        decimal orderTotal = lineSubtotal + taxAmount;


        /*
         * =====================================================================
         * SECTION 11a: checked AND unchecked — INTEGER OVERFLOW
         * =====================================================================
         *
         * By default, integer arithmetic in C# is unchecked — overflow wraps silently
         * (two's complement). checked context throws OverflowException instead.
         *
         *   unchecked { int.MaxValue + 1 }  → wraps to int.MinValue
         *   checked  { int.MaxValue + 1 }  → OverflowException
         *
         * Use checked for financial counters, IDs, or anywhere wrap-around is a bug.
         * ---------------------------------------------------------------------
         */

        int overflowBase = int.MaxValue;
        bool overflowCaught = false;
        try
        {
            checked
            {
                int _ = overflowBase + 1;  // throws OverflowException in checked context
            }
        }
        catch (OverflowException)
        {
            overflowCaught = true;
        }

        int wrappedIncrement = unchecked(overflowBase + 1); // wraps to int.MinValue silently


        /*
         * =====================================================================
         * SECTION 12: bool (Boolean)
         * =====================================================================
         *
         * Stores true or false. Default: false. Used for conditions and flags.
         *
         * Logical operators (full treatment in ch.04):
         *   &&  AND  — both must be true
         *   ||  OR   — at least one must be true
         *   !   NOT  — reverses true ↔ false
         * ---------------------------------------------------------------------
         */

        bool isInStock = true;
        bool isAdult = categoryCode >= 18;              // comparison expression yields bool
        bool isBulkOrder = itemCount >= MaxOrderQuantity / 10;


        /*
         * =====================================================================
         * SECTION 13: char (Single Character)
         * =====================================================================
         *
         * Stores one UTF-16 character. Size: 2 bytes. Default: '\0'
         * Literal syntax: single quotes  'A'  (double quotes would be string)
         * ---------------------------------------------------------------------
         */

        char skuPrefix = 'M';
        char gradeLetter = (char)(skuPrefix + 1);  // char arithmetic — next Unicode code unit
        char tabChar = '\t';
        char unicodeA = '\u0041';                  // Unicode escape — same as 'A'

        ShipmentStatus shipment = ShipmentStatus.Pending; // enum member — underlying int 0


        /*
         * =====================================================================
         * SECTION 14: string (Text — PREVIEW)
         * =====================================================================
         *
         * string holds text and is a REFERENCE type (see Section 16). Immutable.
         *
         * COVERED IN DETAIL LATER → 08. Strings
         *   (interpolation, verbatim strings, String methods, StringBuilder)
         * ---------------------------------------------------------------------
         */

        string skuCode = $"{skuPrefix}{categoryCode:D2}-001"; // interpolation + D2 zero-pads to 2 digits
        string invoicePath = @"C:\Orders\invoices\order.txt"; // verbatim — backslashes are literal


        /*
         * =====================================================================
         * SECTION 15: DEFAULT VALUES
         * =====================================================================
         *
         *  Type          | Default
         *  --------------|----------
         *  int, long…    | 0
         *  float, double | 0.0
         *  bool          | false
         *  char          | '\0'
         *  decimal       | 0.0m
         *  string        | null
         *  class         | null
         *
         * default keyword or default(T) returns the default for any type.
         * Local variables must still be assigned before use — default() in an
         * assignment is fine; an unassigned local is not.
         * ---------------------------------------------------------------------
         */

        int defaultInteger = default;      // same as 0 for int
        bool defaultFlag = default;        // false for bool
        string? defaultLabel = default;    // null for reference type


        /*
         * =====================================================================
         * SECTION 16: VALUE TYPES VS REFERENCE TYPES
         * =====================================================================
         *
         * C# divides types into two storage models:
         *
         *  Category        | Examples              | Variable holds        | Default
         *  ----------------|-----------------------|-----------------------|--------
         *  Value type      | int, bool, decimal,   | The actual data       | 0/false/…
         *                  | char, enum, struct    | copied on assignment  |
         *  Reference type  | string, class, array  | A reference (address) | null
         *                  |                       | to heap data          |
         *
         * --- Value type copy ---
         * Assigning copies the bits — two independent variables:
         *
         *   int original = 10;
         *   int copy = original;
         *   copy = 99;          // original is still 10
         *
         * --- Reference type copy ---
         * Assigning copies the reference, not the object — both variables point
         * at the same instance until one is reassigned:
         *
         *   StoreConfig a = store;
         *   StoreConfig b = a;   // same object; readonly fields shared
         *
         * string is a reference type but immutable — "changing" a string creates
         * a new string object; other variables keep the old text.
         * ---------------------------------------------------------------------
         */

        int originalQuantity = itemCount;
        int copiedQuantity = originalQuantity;
        copiedQuantity = 99;               // value copy — originalQuantity still 3

        StoreConfig storeAlias = store;    // reference copy — both variables point to same object

        string primaryLabel = "Ship";
        string secondaryLabel = primaryLabel;
        secondaryLabel = "Deliver";        // new string — primaryLabel still "Ship" (immutable)


        /*
         * =====================================================================
         * SECTION 16a: STACK VS HEAP (PREVIEW)
         * =====================================================================
         *
         * Value types usually live on the stack — fast, scoped to the method.
         * Reference types store a reference on the stack pointing to data on the heap.
         *
         * COVERED IN DETAIL LATER → 00. .NET Framework Architecture (CLR)
         * COVERED IN DETAIL LATER → 08. Memory Management
         * ---------------------------------------------------------------------
         */


        /*
         * =====================================================================
         * SECTION 17: TYPE CONVERSION (PREVIEW)
         * =====================================================================
         *
         * --- Implicit (widening — automatic, safe) ---
         *   int → long, float, double, decimal
         *
         * --- Explicit cast (narrowing — may lose data) ---
         *   int i = (int)orderTotal;   → truncates fraction
         *
         * --- string ↔ number ---
         *   int.Parse("42")                  → 42  (FormatException if invalid)
         *   int.TryParse("abc", out int n)   → false, n = 0
         *
         * COVERED IN DETAIL LATER → 05. Type Conversion and Casting
         *   (Convert class, culture-specific Parse, user-defined conversions)
         * ---------------------------------------------------------------------
         */

        long itemCountAsLong = itemCount;                    // implicit widening int → long
        int totalRoundedDown = (int)orderTotal;              // explicit cast — truncates fraction
        int parsedQuantity = int.Parse("2");               // FormatException if string invalid
        bool parseFailed = int.TryParse("abc", out int badParse); // false on failure, no exception
        int convertedBonus = Convert.ToInt32("50");
        string priceText = unitPrice.ToString("F2");         // F2 — fixed two decimal places
        string countText = itemCount.ToString();


        /*
         * =====================================================================
         * SECTION 18: object AND BOXING (PREVIEW)
         * =====================================================================
         *
         * object is the root type of all types. Assigning a value type to object
         * BOXES a copy onto the heap; casting back UNBOXES it.
         *
         *   object boxed = 42;
         *   int n = (int)boxed;
         *
         * Bad unbox → InvalidCastException at runtime.
         *
         * COVERED IN DETAIL LATER → 08. Memory Management
         * ---------------------------------------------------------------------
         */

        object boxedQuantity = itemCount;           // boxing — value copied onto heap as object
        int unboxedQuantity = (int)boxedQuantity; // unboxing — cast back to int
        bool quantityIsInt = boxedQuantity is int; // pattern test — true


        /*
         * =====================================================================
         * SECTION 19: NULLABLE VALUE TYPES (int?, Nullable<T>)
         * =====================================================================
         *
         * Value types normally cannot be null. Append ? to allow null:
         *
         *   int? score = null;     same as Nullable<int>
         *
         * Members:
         *   score.HasValue  → false when null
         *   score.Value     → the int (throws if null)
         *   score ?? 0      → null-coalescing: substitute 0 when null
         *   score ??= 10    → assign 10 only if currently null
         * ---------------------------------------------------------------------
         */

        int? loyaltyPoints = null;
        int? confirmedPoints = 85;
        loyaltyPoints ??= 0;                              // assign 0 only if currently null
        int earnedPoints = loyaltyPoints ?? 0;            // null-coalesce — use 0 when null
        int totalPoints = earnedPoints + (confirmedPoints ?? 0);


        /*
         * =====================================================================
         * SECTION 20: NULLABLE REFERENCE TYPES (string?)
         * =====================================================================
         *
         * With <Nullable>enable</Nullable> in the .csproj, reference types get
         * nullability annotations:
         *
         *   string name;     → compiler expects non-null assignment before use
         *   string? note;    → explicitly allows null
         *
         * The ? on a reference type is a compile-time hint — not a different
         * runtime type. Use it for optional text fields (middle name, notes).
         *
         * Null-forgiving operator ! tells the compiler "I know this is not null":
         *   string definite = maybeNote!;   // suppress warning when you are sure
         * ---------------------------------------------------------------------
         */

        string requiredSku = skuCode;
        string? optionalGiftMessage = null;         // ? annotation — explicitly allows null
        string? resolvedMessage = optionalGiftMessage ?? "No gift message";
        string definiteMessage = resolvedMessage;


        /*
         * =====================================================================
         * SECTION 21: RUNTIME TYPE CHECKS & RANGE CONSTANTS
         * =====================================================================
         *
         *   value is int         → pattern match: true/false
         *   typeof(int)          → Type object at compile time
         *   int.MaxValue         → 2,147,483,647
         *   int.MinValue         → -2,147,483,648
         * ---------------------------------------------------------------------
         */

        Type integerType = typeof(int);  // Type metadata for int — resolved at compile time


        /*
         * =====================================================================
         * SECTION 22: ORDER SUMMARY — DEMONSTRATION
         * =====================================================================
         *
         * Prints a receipt built from every variable above. Each WriteLine uses
         * real computed values — nothing is declared only for show.
         * ---------------------------------------------------------------------
         */

        Console.WriteLine("=== Order Summary ===");
        Console.WriteLine($"Customer: {customerName} (age {customerAge}, adult: {isAdult})");
        Console.WriteLine($"Store: {store.StoreCode}  Tax rate (readonly): {store.StandardTaxRate:P0}"); // P0 — percent, no decimals
        Console.WriteLine($"Alias same store: {ReferenceEquals(store, storeAlias)}"); // true — same heap object
        Console.WriteLine($"Product:  {productName}  SKU: {requiredSku}  Grade: {gradeLetter}  Shipment: {shipment}");
        Console.WriteLine($"Year: {orderYear}  Order ID: {orderId}  Lines printed: {counter}");
        Console.WriteLine($"Qty: {unboxedQuantity} (text: {countText})  Units/box: {unitsPerBox}");
        Console.WriteLine($"Value copy — original: {originalQuantity}, copy after change: {copiedQuantity}");
        Console.WriteLine($"Labels — primary: {primaryLabel}, secondary after change: {secondaryLabel}");
        Console.WriteLine($"Unit price: {priceText} INR  Subtotal: {lineSubtotal:C}  Tax: {taxAmount:C}"); // C — currency format
        Console.WriteLine($"Order total: {orderTotal:C}  Rounded (int cast): {totalRoundedDown}");
        Console.WriteLine($"In stock: {isInStock}  Bulk order: {isBulkOrder}  Points: {totalPoints}");
        Console.WriteLine($"Weight: {productWeightKg} kg  Rating: {averageRating:F1}  Dist: {distanceMeters:N0} m"); // F1 fixed, N0 grouped
        Console.WriteLine($"Temp offset: {temperatureOffset}  RGB accent: {accentRed}  Port: {servicePort}");
        Console.WriteLine($"Population ref: {population:N0}  Checksum: {transactionChecksum}");
        Console.WriteLine($"Invoice size (bytes): {invoiceBytes}  Parsed qty: {parsedQuantity}");
        Console.WriteLine($"Bonus (Convert): {convertedBonus}  Parse failed: {parseFailed} (val={badParse})");
        Console.WriteLine($"Overflow caught: {overflowCaught}  Unchecked wrap: {wrappedIncrement}");
        Console.WriteLine($"Literals — int:{literalInt} bool:{literalBool} char:{literalChar} null?={literalNull.HasValue}"); // HasValue false when null
        Console.WriteLine($"Qty is int: {quantityIsInt}  Type: {integerType.Name}  Range: [{int.MinValue} .. {int.MaxValue}]");
        Console.WriteLine($"Implicit to long: {itemCountAsLong}");
        Console.WriteLine($"Defaults — int: {defaultInteger}, bool: {defaultFlag}, label: {defaultLabel ?? "null"}"); // ?? substitutes when null
        Console.WriteLine($"Scope sum used as qty earlier: {itemCount}  Students: {studentCount}");
        Console.WriteLine($"Warehouse: {warehouseCity}  Line/copy: {lineNumber}/{copyCount}");
        Console.WriteLine($"Chars — tab code: {(int)tabChar}, unicode A: {unicodeA}"); // (int) cast char to numeric code
        Console.WriteLine($"Path: {invoicePath}");
        Console.WriteLine($"Gift message: {definiteMessage}");
        Console.WriteLine($"Literal string sample: {literalString}  float:{literalFloat} double:{literalDouble} dec:{literalDecimal}");
    }
}

/*
 * =========================================================================
 * QUICK REFERENCE — DATA TYPES & VARIABLES
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
 *  Age, percentage 0–255            | byte
 *  Network port                      | ushort
 *  General decimal math              | double
 *  Money, prices, financial totals   | decimal
 *  Memory-sensitive float arrays     | float
 *  True/false flags                  | bool
 *  Single character                  | char
 *  Text, names, messages             | string
 *  Optional numeric field            | int?, decimal?, etc.
 *  Optional text field               | string?
 *
 * --- const vs readonly vs var ---
 *
 *  Keyword   | When set              | Typical use
 *  ----------|-----------------------|----------------------------------
 *  var       | Inferred at decl      | Obvious types: var name = "Ann";
 *  const     | Compile time          | Fixed limits, tax rates
 *  readonly  | Decl or constructor   | Per-instance config (StoreConfig)
 *
 * --- Value vs reference ---
 *
 *  Value type     → copy on assignment; stack (usually); cannot be null (unless T?)
 *  Reference type → copy the reference; heap object; default null
 *
 * --- Nullable ---
 *
 *  int? score = null;       Nullable value type
 *  string? note = null;     Nullable reference annotation (compile-time)
 *  score ?? 0               Null-coalescing
 *
 * --- Common compile errors ---
 *
 *  Mistake                    | Error
 *  ---------------------------|----------------------------------
 *  Read before assignment     | CS0165 unassigned local variable
 *  Invalid cast at runtime    | InvalidCastException on bad unbox
 *  Parse invalid string       | FormatException (use TryParse instead)
 *  Integer overflow in checked| OverflowException
 *  char vs string quotes      | CS1012 too many/few characters in literal
 *
 * =========================================================================
 */
