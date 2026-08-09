/*
 * =============================================================================
 * 05. C# 7 FEATURES — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: C# 7 language enhancements — out variables, pattern matching (is and
 *        switch), digit separators, binary literals, tuples and deconstruction,
 *        local functions, ref returns and ref locals, throw expressions,
 *        expression-bodied members, generalized async return types (ValueTask),
 *        async Main, and selected 7.1–7.3 additions (default literal,
 *        inferred tuple names, ref readonly, in parameters).
 *
 * WHY IT MATTERS:
 *   C# 7 (Visual Studio 2017) introduced many quality-of-life features that
 *   appear constantly in modern .NET code. Out variables remove boilerplate
 *   before TryParse; pattern matching makes type checks and switch branches
 *   expressive; tuples return multiple values without a DTO; ref returns let
 *   you mutate a slot inside a collection in place; ValueTask reduces
 *   allocations on hot async paths. Even when targeting newer runtimes, these
 *   patterns remain the idiomatic way to write C#.
 *
 * WHAT YOU WILL LEARN:
 *   1.  C# 7 overview — version milestones 7.0 through 7.3
 *   2.  Digit separators and binary literals — readable numeric constants
 *   3.  Out variables — declare at the call site (out int, out var)
 *   4.  Pattern matching with is — type test + declaration
 *   5.  Pattern matching with switch — type, constant, when guards
 *   6.  Tuples and deconstruction — multi-value returns and inferred names
 *   7.  Local functions — nested helpers scoped inside a method
 *   8.  Ref returns, ref locals, ref readonly, and in parameters
 *   9.  Generalized async return types — ValueTask on cache-hit paths
 *  10.  Expression-bodied members — => for properties and methods
 *  11.  Throw expressions — throw inside ?? and ternary branches
 *  12.  Async Main — Task-returning entry point with await
 *  13.  default literal (C# 7.1) — default(T) without naming T twice
 *
 * =============================================================================
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSharp7Features;

/*
 * =========================================================================
 * SECTION 2: DIGIT SEPARATORS AND BINARY LITERALS
 * =========================================================================
 *
 * C# 7 lets you insert underscores inside numeric literals for readability.
 * The compiler ignores them. Works on integers, floats, decimals, binary,
 * and hexadecimal literals.
 *
 *   int million = 1_000_000;
 *   decimal price = 12_345.67m;
 *   int mask = 0b1111_0000_1111_0000;   // binary literal (C# 7)
 *   int hexColor = 0xFF_EC_DC;
 *
 * Rules:
 *   • Underscore must sit between digits (not at start or end of the literal).
 *   • Binary prefix 0b / 0B is new in C# 7 (hex 0x existed before).
 *
 * Warehouse constants below feed the routing demo in Main.
 * -------------------------------------------------------------------------
 */
public static class WarehouseConstants
{
    public const int MaxCapacity = 1_000_000;
    public const decimal ExpressSurcharge = 250.00m;
    public const int LocationBitmask = 0b0000_1111_0000_0000;
    public const int DefaultAisleHex = 0x0A;
    public const decimal HighValueThreshold = 10_000m;
}

/*
 * =========================================================================
 * SECTION 10: EXPRESSION-BODIED MEMBERS — Order AND OrderPriority
 * =========================================================================
 *
 * C# 6 introduced => for properties and methods. C# 7 extends expression-
 * bodied syntax to more member kinds (constructors, finalizers, accessors —
 * see C# 7.3 notes in comments on Order).
 *
 * Single-expression members use => instead of a block body:
 *
 *   public decimal Total => Qty * Price;
 *   public override string ToString() => $"{Id}: {Name}";
 *
 * Order is the domain type for the warehouse routing demo. Expression-bodied
 * computed properties keep small calculations readable without a method call.
 * -------------------------------------------------------------------------
 */
public enum OrderPriority
{
    Standard,
    Express,
    Critical
}

public sealed class Order
{
    public int OrderId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public OrderPriority Priority { get; set; } = OrderPriority.Standard;

    // Expression-bodied property — single expression, no backing field
    public decimal LineTotal => Quantity * UnitPrice;

    // Reuses LineTotal and WarehouseConstants threshold
    public bool IsHighValue => LineTotal >= WarehouseConstants.HighValueThreshold;

    public string Summary =>
        $"#{OrderId}  {Quantity}x {Sku}  @ {UnitPrice:C}  = {LineTotal:C}  [{Priority}]";

    public override string ToString() => Summary;
}

public class Program
{
    /*
     * In-memory product name cache for the ValueTask demo (Section 9).
     * A synchronous cache hit avoids allocating a Task object on the heap.
     */
    private static readonly Dictionary<int, string> ProductNameCache = new()
    {
        [101] = "Industrial Widget",
        [202] = "Precision Gear",
    };

    /*
     * =========================================================================
     * SECTION 3: OUT VARIABLES — DECLARE AT THE CALL SITE
     * =========================================================================
     *
     * Before C# 7 you declared an out variable before the call:
     *
     *   int qty;
     *   if (int.TryParse(text, out qty)) { … }
     *
     * C# 7 allows inline declaration:
     *
     *   if (int.TryParse(text, out int qty)) { … }
     *
     * Scope: the out variable is visible in the enclosing block (if/while/
     * switch body) and in the rest of the method after the statement.
     *
     * out var — compiler infers the type from the method signature:
     *
     *   dict.TryGetValue(key, out var value);
     *
     * CS0165: reading an out variable before the call assigns it is an error.
     * TryParse only assigns out when it returns true; on false the variable
     * is still definitely assigned (default value) in C# 7+.
     * -------------------------------------------------------------------------
     */
    private static bool TryParseQuantity(string text, out int quantity)
    {
        return int.TryParse(text, out quantity);
    }

    private static void DemonstrateOutVariables(out int parsedQuantity, out int onHand)
    {
        string quantityText = "48";
        string badQuantityText = "abc";

        Console.WriteLine("--- Out variables ---");

        if (TryParseQuantity(quantityText, out int qtyFromText))
        {
            Console.WriteLine($"Parsed via helper (out forwarded): \"{quantityText}\" → {qtyFromText}");
        }

        if (!int.TryParse(badQuantityText, out int rejectedQuantity))
        {
            // rejectedQuantity is 0 — definitely assigned even when TryParse returns false
            Console.WriteLine(
                $"Could not parse \"{badQuantityText}\"; rejectedQuantity stays 0: {rejectedQuantity}");
        }

        Dictionary<string, int> stockBySku = new()
        {
            ["WIDGET-A"] = 500,
            ["GEAR-B"] = 120,
        };

        if (stockBySku.TryGetValue("WIDGET-A", out var widgetOnHand))
        {
            Console.WriteLine($"On-hand WIDGET-A (out var): {widgetOnHand}");
        }

        parsedQuantity = qtyFromText;
        onHand = widgetOnHand;
        Console.WriteLine();
    }

    /*
     * =========================================================================
     * SECTION 4: PATTERN MATCHING — is OPERATOR
     * =========================================================================
     *
     * The is operator tests a runtime type and, with a declaration, introduces
     * a new variable in the true branch:
     *
     *   if (obj is Order order) { use order.Sku }
     *
     * Replaces the older two-step pattern:
     *
     *   var order = obj as Order;
     *   if (order != null) { … }
     *
     * --- 4a. Type pattern with declaration ---
     * --- 4b. Type pattern without declaration (boolean test only) ---
     *
     * Recursive patterns, property patterns, and switch expressions are C# 8+
     * → COVERED IN DETAIL LATER → 06. C# 8 Features
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateIsPatterns(object shipmentPayload)
    {
        Console.WriteLine("--- Pattern matching: is ---");

        if (shipmentPayload is Order recognizedOrder)
        {
            Console.WriteLine($"Payload is Order: {recognizedOrder.Summary}");
        }

        if (shipmentPayload is string textPayload)
        {
            Console.WriteLine($"Payload is string: {textPayload}");
        }
        else if (shipmentPayload is Order)
        {
            Console.WriteLine("Confirmed again: payload is an Order instance.");
        }

        // Constant pattern with is (C# 7) — test value, no new variable
        if (shipmentPayload is null)
        {
            Console.WriteLine("Payload is null.");
        }

        Console.WriteLine();
    }

    /*
     * =========================================================================
     * SECTION 5: PATTERN MATCHING — switch STATEMENT
     * =========================================================================
     *
     * C# 7 extends switch to support:
     *
     *   • Type patterns       case Order o:
     *   • Constant patterns   case OrderPriority.Critical:
     *   • when guards         case Order o when o.IsHighValue:
     *   • null handling       case null:
     *
     * First matching case wins — order matters (no C-style fall-through).
     *
     * Switch expressions (=> syntax) arrive in C# 8
     * → COVERED IN DETAIL LATER → 06. C# 8 Features
     * -------------------------------------------------------------------------
     */
    private static string RouteOrder(Order order)
    {
        switch (order.Priority)
        {
            case OrderPriority.Critical:
                return "CRITICAL-LANE";
            case OrderPriority.Express when order.IsHighValue:
                return "EXPRESS-VIP";
            case OrderPriority.Express:
                return "EXPRESS-STANDARD";
            case OrderPriority.Standard when order.Quantity > 100:
                return "BULK-STANDARD";
            case OrderPriority.Standard:
                return "STANDARD";
            default:
                return "UNKNOWN";
        }
    }

    private static string ClassifyPayload(object? payload)
    {
        switch (payload)
        {
            case null:
                return "EMPTY";
            case Order o when o.IsHighValue:
                return "HIGH-VALUE-ORDER";
            case Order o:
                return $"ORDER-{o.OrderId}";
            case string s when s.Length == 0:
                return "BLANK-STRING";
            case string s:
                return $"STRING-{s.Length}-CHARS";
            default:
                return payload.GetType().Name;
        }
    }

    /*
     * =========================================================================
     * SECTION 6: TUPLES AND DECONSTRUCTION
     * =========================================================================
     *
     * ValueTuple (built into modern SDK) lets methods return multiple values
     * without a dedicated DTO:
     *
     *   (bool ok, string message) Validate(Order o) => (true, "OK");
     *
     * Deconstruction unpacks into locals:
     *
     *   (bool ok, string msg) = Validate(order);
     *   var (ok, msg) = Validate(order);
     *
     * --- 6a. Inferred tuple element names (C# 7.1) ---
     *
     * When you return (available, order.Quantity), the compiler can infer
     * element names Available and Quantity from the expressions:
     *
     *   return (available, order.Quantity);  // names: (Available, Quantity)
     *
     * --- 6b. Tuple equality (C# 7.3) ---
     *
     *   (1, 2) == (1, 2)   // true — element-wise comparison
     * -------------------------------------------------------------------------
     */
    private static (bool CanFulfill, string Note) CheckFulfillment(Order order, int available)
    {
        if (available >= order.Quantity)
        {
            return (true, $"OK — {available} on hand for {order.Quantity} requested.");
        }

        return (false, $"Short {order.Quantity - available} units.");
    }

    private static (int Available, int Requested, bool CanFulfill) BuildFulfillmentSnapshot(
        Order order,
        int available)
    {
        // C# 7.1 — inferred element names from expression identifiers
        return (available, order.Quantity, available >= order.Quantity);
    }

    /*
     * =========================================================================
     * SECTION 7: LOCAL FUNCTIONS
     * =========================================================================
     *
     * A function declared inside another method is a local function. It can
     * capture outer variables (like a lambda) but supports full method syntax
     * including ref returns, loops, and recursion.
     *
     * Use when the helper is only meaningful inside one method — keeps class
     * surface area small compared to a private static method.
     *
     * Static local functions (cannot capture outer state) arrive in C# 8
     * → COVERED IN DETAIL LATER → 06. C# 8 Features
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateLocalFunctions(Order expressOrder, Order criticalOrder, int onHand)
    {
        PrintSectionBanner("Local functions — pipeline summary");

        void PrintSectionBanner(string title)
        {
            Console.WriteLine($"===== {title} =====");
        }

        void PrintOrderPipeline(Order order, int stockLevel)
        {
            (bool ok, string note) = CheckFulfillment(order, stockLevel);
            Console.WriteLine(
                $"Pipeline: {order.Sku} → lane {RouteOrder(order)} → fulfill={ok} — {note}");
        }

        PrintOrderPipeline(expressOrder, onHand);
        PrintOrderPipeline(criticalOrder, stockLevel: 120);
        Console.WriteLine();
    }

    /*
     * =========================================================================
     * SECTION 8: REF RETURNS, REF LOCALS, ref readonly, AND in PARAMETERS
     * =========================================================================
     *
     * --- 8a. ref return and ref local (C# 7.0) ---
     *
     * ref return passes back a reference to a variable's storage location,
     * not a copy of the value. ref local holds that alias:
     *
     *   ref int slot = ref Find(ref arr, 2);
     *   slot = 99;   // mutates arr[2] in place
     *
     * Rules:
     *   • Returned ref must point to a real variable (not a temporary).
     *   • ref locals cannot appear in async methods.
     *   • CS8156 / CS8168 if you return ref to a local that goes out of scope.
     *
     * --- 8b. ref readonly returns (C# 7.2) ---
     *
     *   public ref readonly int GetSlotReadOnly(ref int[] arr, int i)
     *       => ref arr[i];
     *
     * Caller receives read-only alias — cannot mutate through it without unsafe.
     *
     * --- 8c. in parameters (C# 7.2) ---
     *
     *   void LogLargeStruct(in LargeStruct value)
     *
     * Passes by readonly reference — avoids copy for large structs without
     * allowing mutation. Different from ref (mutable) and out (must assign).
     * -------------------------------------------------------------------------
     */
    private static ref int FindInventorySlot(int[] counts, int skuIndex)
    {
        if (skuIndex < 0 || skuIndex >= counts.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(skuIndex));
        }

        return ref counts[skuIndex];
    }

    private static ref readonly int GetInventoryCountReadOnly(int[] counts, int skuIndex)
    {
        return ref counts[skuIndex];
    }

    private static void ApplyStockReservation(Order order, int[] inventoryCounts, int skuIndex)
    {
        ref int widgetSlot = ref FindInventorySlot(inventoryCounts, skuIndex);
        int before = widgetSlot;
        widgetSlot -= order.Quantity;
        int after = widgetSlot;

        ref readonly int readOnlyView = ref GetInventoryCountReadOnly(inventoryCounts, skuIndex);
        int confirmed = readOnlyView;

        Console.WriteLine("--- Ref returns and ref locals ---");
        Console.WriteLine($"WIDGET-A stock: {before} → {after} (reserved {order.Quantity})");
        Console.WriteLine($"Array confirms: inventoryCounts[{skuIndex}] = {confirmed}");
        Console.WriteLine();
    }

    private static void LogOrderSnapshot(in OrderSnapshot snapshot)
    {
        // in parameter — large struct passed by readonly reference, no copy
        Console.WriteLine(
            $"Snapshot (in param): #{snapshot.OrderId} qty={snapshot.Quantity} lane={snapshot.Lane}");
    }

    /*
     * =========================================================================
     * SECTION 9: GENERALIZED ASYNC RETURN TYPES — ValueTask
     * =========================================================================
     *
     * C# 7 allows async methods to return any type with a suitable GetAwaiter()
     * pattern — typically Task, Task<T>, ValueTask, ValueTask<T>.
     *
     * ValueTask<T> shines when the operation often completes synchronously
     * (cache hit): no Task object allocation on the fast path.
     *
     * Caveat: a consumed ValueTask must not be awaited twice unless it was
     * constructed from a Task or backed by IValueTaskSource. Document retry
     * semantics for cacheable hot paths.
     * -------------------------------------------------------------------------
     */
    private static async ValueTask<string> GetProductNameAsync(int productId)
    {
        if (ProductNameCache.TryGetValue(productId, out string? cached))
        {
            return cached; // synchronous completion — no Task allocation
        }

        await Task.Delay(10); // simulated I/O on cache miss
        string fetched = $"Product-{productId}";
        ProductNameCache[productId] = fetched;
        return fetched;
    }

    /*
     * =========================================================================
     * SECTION 11: THROW EXPRESSIONS
     * =========================================================================
     *
     * C# 7 allows throw in expression contexts — null-coalescing and ternary:
     *
     *   Order o = Find(id) ?? throw new InvalidOperationException("missing");
     *   int qty = parsed ? value : throw new FormatException("bad");
     *
     * The thrown exception propagates; the expression has no normal value.
     * Useful for guard clauses that must produce a non-null result.
     * -------------------------------------------------------------------------
     */
    private static Order RequireOrder(Order? order, string paramName)
    {
        return order ?? throw new ArgumentNullException(paramName);
    }

    private static int RequirePositiveQuantity(int quantity)
    {
        return quantity > 0
            ? quantity
            : throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");
    }

    /*
     * =========================================================================
     * SECTION 13: default LITERAL (C# 7.1)
     * =========================================================================
     *
     * default is a literal expression that produces the default value for any
     * type — reference types null, numeric types zero, bool false:
     *
     *   OrderPriority p = default;        // same as default(OrderPriority)
     *   int? n = default;                 // null
     *
     * Especially useful with generics where you do not repeat the type name.
     * -------------------------------------------------------------------------
     */
    private static OrderPriority ResolvePriority(OrderPriority? overridePriority)
    {
        return overridePriority ?? default; // default(OrderPriority) = Standard (enum 0)
    }

    /*
     * =========================================================================
     * SECTION 12: ASYNC MAIN — ENTRY POINT WITH AWAIT
     * =========================================================================
     *
     * C# 7.1 allows Main to return Task or Task<int> so top-level code can
     * await asynchronous work without blocking with .GetAwaiter().GetResult().
     *
     * Signatures supported:
     *
     *   static void Main(string[] args)
     *   static int Main(string[] args)
     *   static Task Main(string[] args)          // C# 7.1+
     *   static Task<int> Main(string[] args)     // C# 7.1+
     *
     * The compiler generates a real entry point; async Main wraps your body in
     * an async state machine. Main orchestrates the warehouse demo below.
     * -------------------------------------------------------------------------
     */
    public static async Task Main(string[] args)
    {
        /*
         * SECTION 1: C# 7 OVERVIEW
         *
         * C# 7 is a language version (not a runtime). SDK-style projects pick
         * LangVersion from the target framework unless you set <LangVersion> explicitly.
         *
         * Milestones:
         *
         *   Version | Notable additions
         *   --------|----------------------------------------------------------
         *   7.0     | out vars, tuples, is/switch patterns, ref returns,
         *           | local functions, throw expr, binary literals, digit sep
         *   7.1     | async Main, default literal, inferred tuple names
         *   7.2     | ref readonly, in params, readonly struct, span preview
         *   7.3     | tuple ==, stackalloc init, expression-bodied accessors
         *
         * C# 8 (recursive patterns, nullable reference types, ranges, etc.)
         * → COVERED IN DETAIL LATER → 06. C# 8 Features
         */

        Console.WriteLine("=== C# 7 Features — Warehouse Order Routing Demo ===");
        Console.WriteLine();

        Console.WriteLine("--- Digit separators and binary literals ---");
        Console.WriteLine($"Warehouse capacity: {WarehouseConstants.MaxCapacity:N0}");
        Console.WriteLine($"Express surcharge: {WarehouseConstants.ExpressSurcharge:C}");
        Console.WriteLine(
            $"Location bitmask (binary): {Convert.ToString(WarehouseConstants.LocationBitmask, 2).PadLeft(16, '0')}");
        Console.WriteLine($"Default aisle (hex 0x0A): {WarehouseConstants.DefaultAisleHex}");
        Console.WriteLine();

        DemonstrateOutVariables(out int parsedQuantity, out int widgetOnHand);

        Order expressOrder = new Order
        {
            OrderId = 1_000_042,
            Sku = "WIDGET-A",
            Quantity = parsedQuantity,
            UnitPrice = 125.50m,
            Priority = OrderPriority.Express,
        };

        Order criticalOrder = new Order
        {
            OrderId = 1_000_043,
            Sku = "GEAR-B",
            Quantity = 10,
            UnitPrice = 1_250.00m,
            Priority = OrderPriority.Critical,
        };

        object shipmentPayload = expressOrder;
        DemonstrateIsPatterns(shipmentPayload);

        Console.WriteLine("--- Pattern matching: switch ---");
        Console.WriteLine($"Express order lane: {RouteOrder(expressOrder)}");
        Console.WriteLine($"Critical order lane: {RouteOrder(criticalOrder)}");
        Console.WriteLine($"Payload classification: {ClassifyPayload(shipmentPayload)}");
        Console.WriteLine();

        (bool canFulfill, string fulfillmentNote) = CheckFulfillment(expressOrder, widgetOnHand);
        Console.WriteLine("--- Tuples and deconstruction ---");
        Console.WriteLine($"Fulfillment check: {canFulfill} — {fulfillmentNote}");

        var snapshot = BuildFulfillmentSnapshot(expressOrder, widgetOnHand);
        Console.WriteLine(
            $"Inferred names: Available={snapshot.Available}, Requested={snapshot.Requested}, CanFulfill={snapshot.CanFulfill}");

        (int a, int b) leftPair = (widgetOnHand, parsedQuantity);
        (int a, int b) rightPair = (widgetOnHand, parsedQuantity);
        Console.WriteLine($"Tuple equality (C# 7.3): leftPair == rightPair → {leftPair == rightPair}");
        Console.WriteLine();

        DemonstrateLocalFunctions(expressOrder, criticalOrder, widgetOnHand);

        int[] inventoryCounts = { 500, 120, 75 };
        ApplyStockReservation(expressOrder, inventoryCounts, skuIndex: 0);

        Console.WriteLine("--- ValueTask product lookup ---");
        string cachedName = await GetProductNameAsync(productId: 101);
        Console.WriteLine($"Product 101 (cache hit): {cachedName}");
        string fetchedName = await GetProductNameAsync(productId: 303);
        Console.WriteLine($"Product 303 (simulated fetch): {fetchedName}");
        Console.WriteLine();

        Console.WriteLine("--- Expression-bodied members ---");
        Console.WriteLine(expressOrder.Summary);
        Console.WriteLine(
            $"Is high value? {expressOrder.IsHighValue} (line total {expressOrder.LineTotal:C})");
        Console.WriteLine();

        Order validated = RequireOrder(expressOrder, nameof(expressOrder));
        int safeQuantity = RequirePositiveQuantity(parsedQuantity);
        Console.WriteLine("--- Throw expressions ---");
        Console.WriteLine($"Validated order #{validated.OrderId}, safe quantity {safeQuantity}");
        Console.WriteLine();

        OrderPriority resolved = ResolvePriority(overridePriority: null);
        Console.WriteLine($"--- default literal (C# 7.1) ---");
        Console.WriteLine($"Resolved priority when override is null: {resolved}");
        Console.WriteLine();

        OrderSnapshot expressSnapshot = new OrderSnapshot(
            expressOrder.OrderId,
            expressOrder.Quantity,
            RouteOrder(expressOrder));
        LogOrderSnapshot(in expressSnapshot);

        Console.WriteLine();
        Console.WriteLine("Demo complete.");
    }
}

/*
 * Readonly struct used with in parameters (Section 8c).
 * Structs larger than a pointer benefit from in to avoid copying on the stack.
 */
public readonly struct OrderSnapshot
{
    public OrderSnapshot(int orderId, int quantity, string lane)
    {
        OrderId = orderId;
        Quantity = quantity;
        Lane = lane;
    }

    public int OrderId { get; }
    public int Quantity { get; }
    public string Lane { get; }
}

/*
 * =============================================================================
 * QUICK REFERENCE — C# 7 FEATURES
 * =============================================================================
 *
 * --- Out variables ---
 *
 *   if (int.TryParse(text, out int n)) { … }
 *   dict.TryGetValue(key, out var value);
 *
 * --- Pattern matching: is ---
 *
 *   if (obj is Order o) { … }
 *   if (obj is null) { … }
 *
 * --- Pattern matching: switch ---
 *
 *   switch (x) {
 *     case Order o when o.IsHighValue: …
 *     case null: …
 *     case int i when i > 0: …
 *     default: …
 *   }
 *
 * --- Digit separators and binary literals ---
 *
 *   1_000_000    0b1111_0000    0xFF_EC_DC    12_345.67m
 *
 * --- Tuples ---
 *
 *   (bool ok, string msg) = Method();
 *   var (a, b) = Method();
 *   return (available, order.Quantity);   // C# 7.1 inferred names
 *   (1, 2) == (1, 2);                     // C# 7.3 tuple equality
 *
 * --- Local functions ---
 *
 *   void Outer() {
 *     void Inner() { … }
 *     Inner();
 *   }
 *
 * --- Ref returns / ref locals / ref readonly / in ---
 *
 *   ref int Get(int[] a, int i) => ref a[i];
 *   ref readonly int View(int[] a, int i) => ref a[i];
 *   ref int slot = ref Get(arr, 0);
 *   void M(in LargeStruct s) { … }
 *
 * --- ValueTask ---
 *
 *   async ValueTask<T> GetAsync() {
 *     if (cached) return cached;
 *     return await FetchAsync();
 *   }
 *
 * --- Expression-bodied members ---
 *
 *   public int Count => _list.Count;
 *   public override string ToString() => $"{Id}";
 *
 * --- Throw expressions ---
 *
 *   var x = Get() ?? throw new InvalidOperationException();
 *   int v = ok ? n : throw new FormatException();
 *
 * --- Async Main (C# 7.1) ---
 *
 *   static async Task Main(string[] args) {
 *     await RunAsync();
 *   }
 *
 * --- default literal (C# 7.1) ---
 *
 *   OrderPriority p = default;
 *   T value = default;
 *
 * --- Common mistakes ---
 *
 *  Mistake                               | Result
 *  --------------------------------------|----------------------------------
 *  Using out var before Try* assigns it  | CS0165 unassigned local (older rules)
 *  Awaiting same ValueTask twice         | Undefined behavior / exception
 *  ref return pointing at local temp     | CS8156 / CS8168 compile error
 *  Pattern case order wrong              | Wrong branch taken (first match)
 *
 * --- Related chapters ---
 *
 *   06. C# 8 Features          switch expressions, recursive patterns, static locals
 *   04. Var Dynamic & Special  var, dynamic, nameof (special keywords)
 *   Async chapters             Task vs ValueTask depth, ConfigureAwait
 *
 * =============================================================================
 */
