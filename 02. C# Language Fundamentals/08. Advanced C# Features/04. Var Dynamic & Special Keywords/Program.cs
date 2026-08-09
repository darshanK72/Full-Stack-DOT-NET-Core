/*
 * =============================================================================
 * 04. VAR, DYNAMIC & SPECIAL KEYWORDS — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: var (compile-time inference), dynamic (runtime binding via the DLR),
 *        nameof, default, global::, and other contextual keywords that only
 *        mean something in specific syntactic positions — plus volatile and an
 *        advanced ref/out/in recap where those keywords matter in APIs.
 *
 * WHY IT MATTERS:
 *   Warehouse and integration code mixes strongly typed stock records with
 *   plugin payloads whose shape arrives at runtime (CSV columns, JSON keys,
 *   legacy COM). var keeps locals readable without repeating long generic
 *   types; dynamic trades compile-time safety for late binding when the schema
 *   is unknown. nameof and default eliminate magic strings and clarify intent
 *   in exceptions, logging, and generic defaults. global:: disambiguates when
 *   a local type or namespace shadows the BCL.
 *
 * WHAT YOU WILL LEARN:
 *   1.  var — compile-time type inference (deeper than Data Types ch.02)
 *   2.  dynamic and the DLR — ExpandoObject, DynamicObject, runtime errors
 *   3.  var vs dynamic — static typing vs runtime dispatch
 *   4.  dynamic vs reflection — two ways to reach members at runtime
 *   5.  nameof — compile-time name strings without magic literals
 *   6.  default — default literal, default(T), structs and generics
 *   7.  global:: — qualify types when nested names shadow the BCL
 *   8.  Contextual keywords — var, dynamic, partial, where, @identifiers
 *   9.  volatile — cross-thread visibility for simple flags
 *   10. ref / out / in / ref return — advanced recap
 *   11. Named parameters — preview with forward reference
 *
 * =============================================================================
 */

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace VarDynamicAndSpecialKeywords;

/*
 * =========================================================================
 * SECTION 1: INVENTORY ITEM — STRONGLY TYPED BASELINE
 * =========================================================================
 *
 * Fixed SKU records anchor the chapter. Strong typing gives IntelliSense and
 * compile checks on Sku, Quantity, and UnitPrice — contrast with dynamic
 * import rows later.
 * -------------------------------------------------------------------------
 */
public class InventoryItem
{
    public required string Sku { get; init; }
    public required string Name { get; init; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

/*
 * =========================================================================
 * SECTION 3: CUSTOM DynamicObject — FLEXIBLE IMPORT ROW
 * =========================================================================
 *
 * Third-party and legacy APIs often expose dictionary-backed dynamic wrappers.
 * The DLR routes record.Sku and record["Sku"] through TryGetMember /
 * TrySetMember at runtime. Typos become RuntimeBinderException, not CS1061.
 * -------------------------------------------------------------------------
 */
public sealed class FlexibleInventoryRecord : DynamicObject
{
    private readonly Dictionary<string, object?> _fields = new(StringComparer.OrdinalIgnoreCase);

    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        return _fields.TryGetValue(binder.Name, out result);
    }

    public override bool TrySetMember(SetMemberBinder binder, object? value)
    {
        _fields[binder.Name] = value;
        return true;
    }

    public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object? result)
    {
        result = null;
        if (indexes.Length != 1 || indexes[0] is not string key)
        {
            return false;
        }

        return _fields.TryGetValue(key, out result);
    }

    public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object? value)
    {
        if (indexes.Length != 1 || indexes[0] is not string key)
        {
            return false;
        }

        _fields[key] = value;
        return true;
    }

    public IReadOnlyDictionary<string, object?> Snapshot() => _fields;
}

/*
 * =========================================================================
 * SECTION 4: volatile — MEMORY VISIBILITY FOR STOP FLAGS
 * =========================================================================
 *
 * volatile applies to FIELDS (not locals). It tells the JIT and CPU not to
 * cache the field in a register so another thread observes RequestStop
 * promptly. volatile is NOT a lock — pair with simple flags; use Interlocked
 * for atomic math. Full threading depth → 06. Multithreading & Async module.
 * -------------------------------------------------------------------------
 */
public sealed class PriceRefreshSignal
{
    private volatile bool _stopRequested;

    public bool StopRequested => _stopRequested;

    public void RequestStop() => _stopRequested = true;

    public int PollUntilStopped(int maxIterations)
    {
        int iterations = 0;
        while (!_stopRequested && iterations < maxIterations)
        {
            Thread.Sleep(5);
            iterations++;
        }

        return iterations;
    }
}

/*
 * =========================================================================
 * SECTION 5: READONLY STRUCT — in PARAMETER PAYLOAD
 * =========================================================================
 *
 * readonly struct used with in parameters below — avoids copying ~24+ bytes
 * on each call while preventing mutation through the parameter alias.
 * -------------------------------------------------------------------------
 */
public readonly struct InventoryDelta
{
    public InventoryDelta(string sku, int amount)
    {
        Sku = sku;
        Amount = amount;
    }

    public string Sku { get; }
    public int Amount { get; }
}

/*
 * =========================================================================
 * SECTION 6: global:: — LOCAL TYPE CAN SHADOW BCL NAMES
 * =========================================================================
 *
 * A type named Math in this namespace wins over System.Math in unqualified
 * lookup. global::System.Math forces the root BCL type. Never ship shadow
 * names in production — demo only.
 * -------------------------------------------------------------------------
 */
public static class Math
{
    public const double ApproxPi = 3.0;
}

public class Program
{
    public static void Main(string[] args)
    {
        InventoryItem[] stock = CreateInitialStock();

        Console.WriteLine("=== Warehouse stock (typed baseline) ===");
        PrintStock(stock);

        RunVarDemonstrations(stock);
        RunDynamicDemonstrations(stock);
        RunVarVsDynamicDemonstration();
        RunDynamicVsReflectionDemonstration(stock);
        RunNameofDemonstration(stock[0]);
        RunDefaultDemonstration();
        RunGlobalQualifierDemonstration();
        RunContextualKeywordsDemonstration();
        RunVolatileDemonstration();
        RunRefOutInDemonstration(stock);
        RunNamedParametersPreview();

        Console.WriteLine();
        Console.WriteLine("=== Final stock after price update ===");
        PrintStock(stock);
    }

    private static InventoryItem[] CreateInitialStock()
    {
        return
        [
            new InventoryItem { Sku = "WH-100", Name = "Pallet Jack", Quantity = 12, UnitPrice = 899.00m },
            new InventoryItem { Sku = "WH-220", Name = "Barcode Scanner", Quantity = 45, UnitPrice = 129.50m },
            new InventoryItem { Sku = "WH-330", Name = "Label Printer", Quantity = 8, UnitPrice = 349.00m },
        ];
    }

    /*
     * =========================================================================
     * SECTION 2: var — COMPILE-TIME TYPE INFERENCE
     * =========================================================================
     *
     * var is NOT a separate runtime type — the compiler infers the static type
     * from the initializer. Basics appear in 02. Data Types and Variables;
     * here we apply var where the right-hand type is obvious but verbose.
     *
     *   Rule                              | Example
     *   ----------------------------------|----------------------------------
     *   Must initialize at declaration      | var x;           → CS0818
     *   Type fixed after inference          | var n = 10; n = "x" → compile error
     *   Still statically typed              | typeof checks at compile time
     *   Anonymous types require var         | new { … } has no spellable type name
     *
     * --- 2a. foreach and LINQ with var ---
     * --- 2b. Anonymous projection — var required ---
     * -------------------------------------------------------------------------
     */
    private static void RunVarDemonstrations(InventoryItem[] stock)
    {
        var lowStock = stock.Where(item => item.Quantity < 15).OrderBy(item => item.Sku);

        Console.WriteLine();
        Console.WriteLine("--- var with LINQ (inferred: IEnumerable<InventoryItem>) ---");
        foreach (var item in lowStock)
        {
            Console.WriteLine($"  {item.Sku}: {item.Name} qty={item.Quantity}");
        }

        var valuationRows = stock
            .Select(item => new
            {
                item.Sku,
                LineValue = item.Quantity * item.UnitPrice,
            })
            .OrderByDescending(row => row.LineValue);

        Console.WriteLine();
        Console.WriteLine("--- var with anonymous type ---");
        foreach (var row in valuationRows)
        {
            Console.WriteLine($"  {row.Sku}: line value {row.LineValue:C}");
        }

        decimal totalValue = valuationRows.Sum(row => row.LineValue);
        Console.WriteLine($"  Total inventory value: {totalValue:C}");
    }

    /*
     * =========================================================================
     * SECTION 3: dynamic AND THE DLR
     * =========================================================================
     *
     * dynamic defers member resolution to RUNTIME via the Dynamic Language
     * Runtime (DLR). Call sites cache the resolved operation after first
     * successful dispatch.
     *
     * Trade-offs:
     *   + Flexibility when shape is unknown (plugin payloads, COM interop)
     *   − No compile-time checking — typos → RuntimeBinderException
     *   − Slower than static calls; harder to refactor safely
     *
     * --- 3a. ExpandoObject — dictionary-backed dynamic bag ---
     * --- 3b. FlexibleInventoryRecord — custom DynamicObject ---
     * -------------------------------------------------------------------------
     */
    private static dynamic RunDynamicDemonstrations(InventoryItem[] stock)
    {
        dynamic pluginConfig = new ExpandoObject();
        pluginConfig.WarehouseId = "WH-PUNE-01";
        pluginConfig.MaxSkus = 500;
        pluginConfig.AllowNegativeStock = false;

        Console.WriteLine();
        Console.WriteLine("--- dynamic ExpandoObject plugin config ---");
        Console.WriteLine($"  WarehouseId: {pluginConfig.WarehouseId}");
        Console.WriteLine($"  MaxSkus: {pluginConfig.MaxSkus}");

        dynamic importRow = new FlexibleInventoryRecord();
        importRow.Sku = "WH-900";
        importRow.Name = "Imported Gloves";
        importRow.Quantity = 200;

        Console.WriteLine();
        Console.WriteLine("--- dynamic FlexibleInventoryRecord ---");
        Console.WriteLine($"  Imported {importRow.Sku}: {importRow.Name} × {importRow.Quantity}");

        bool withinLimit = stock.Length <= (int)pluginConfig.MaxSkus;
        Console.WriteLine();
        Console.WriteLine($"  Plugin check: {stock.Length} SKUs within MaxSkus {pluginConfig.MaxSkus}? {withinLimit}");

        return pluginConfig;
    }

    /*
     * =========================================================================
     * SECTION 3 (continued): var vs dynamic
     * =========================================================================
     *
     *   Aspect           | var                         | dynamic
     *   -----------------|-----------------------------|----------------------------
     *   When resolved    | Compile time                | Runtime (DLR)
     *   Static type      | Inferred, then fixed        | System.Object at compile time
     *   Wrong member     | Compile error (CS1061)      | RuntimeBinderException
     *   Typical use      | Local brevity, LINQ         | Interop, late-bound config
     *
     * Mental model:
     *   var     = "compiler, figure out the type for me"
     *   dynamic = "compiler, skip checking — resolve at runtime"
     * -------------------------------------------------------------------------
     */
    private static void RunVarVsDynamicDemonstration()
    {
        var staticCount = 42;
        dynamic runtimeCount = 42;

        int doubledStatic = staticCount * 2;
        dynamic doubledRuntime = runtimeCount * 2;

        Console.WriteLine();
        Console.WriteLine("--- var vs dynamic ---");
        Console.WriteLine($"  var path: {staticCount} × 2 = {doubledStatic}");
        Console.WriteLine($"  dynamic path: {runtimeCount} × 2 = {doubledRuntime}");
        Console.WriteLine($"  staticCount.GetType(): {staticCount.GetType().Name}");
        Console.WriteLine($"  runtimeCount.GetType(): {runtimeCount.GetType().Name}");
    }

    /*
     * =========================================================================
     * SECTION 6: dynamic vs REFLECTION
     * =========================================================================
     *
     *   dynamic     DLR binders + cached call sites; natural syntax (obj.Member)
     *   reflection  System.Reflection inspects metadata; explicit and verbose
     *
     * nameof(InventoryItem.UnitPrice) feeds reflection without magic strings.
     * ExpandoObject members are NOT regular CLR properties — GetProperty returns
     * null; dynamic or dictionary access is required.
     * -------------------------------------------------------------------------
     */
    private static void RunDynamicVsReflectionDemonstration(InventoryItem[] stock)
    {
        InventoryItem probe = stock[1];
        decimal priceViaStatic = probe.UnitPrice;

        dynamic probeDynamic = probe;
        decimal priceViaDynamic = probeDynamic.UnitPrice;

        string propertyName = nameof(InventoryItem.UnitPrice);
        PropertyInfo? unitPriceProperty = typeof(InventoryItem).GetProperty(propertyName);
        decimal priceViaReflection = (decimal)unitPriceProperty!.GetValue(probe)!;

        Console.WriteLine();
        Console.WriteLine($"--- dynamic vs reflection on {probe.Sku} ---");
        Console.WriteLine($"  Static:     {priceViaStatic:C}");
        Console.WriteLine($"  Dynamic:    {priceViaDynamic:C}");
        Console.WriteLine($"  Reflection: {priceViaReflection:C} (via {propertyName})");

        dynamic pluginConfig = new ExpandoObject();
        pluginConfig.WarehouseId = "WH-DEMO";
        PropertyInfo? expandoProperty = pluginConfig.GetType().GetProperty("WarehouseId");

        Console.WriteLine();
        Console.WriteLine("--- ExpandoObject: reflection vs dynamic ---");
        Console.WriteLine($"  GetProperty(\"WarehouseId\"): {(expandoProperty is null ? "null" : expandoProperty.Name)}");
        Console.WriteLine($"  dynamic WarehouseId: {pluginConfig.WarehouseId}");
    }

    /*
     * =========================================================================
     * SECTION 7: nameof — COMPILE-TIME NAME STRINGS
     * =========================================================================
     *
     * nameof(expr) evaluates at compile time to the unqualified name of a
     * symbol — never executes expr at runtime (safe for null receivers).
     *
     *   Expression                    | Result
     *   ------------------------------|----------------------------------
     *   nameof(InventoryItem)         | "InventoryItem"
     *   nameof(InventoryItem.Sku)     | "Sku"
     *   nameof(item.Quantity)         | "Quantity"  (item need not be non-null)
     *   nameof(Program.Main)            | "Main"
     *
     * Uses: ArgumentException param names, logging, reflection, INotifyPropertyChanged.
     * Pitfall: nameof(@class) uses @ only in source — result is "class".
     * Pitfall: nameof(List<int>) → "List" not "List`1" — not a generic arity string.
     * -------------------------------------------------------------------------
     */
    private static void RunNameofDemonstration(InventoryItem item)
    {
        string typeName = nameof(InventoryItem);
        string skuMember = nameof(InventoryItem.Sku);
        string qtyFromInstance = nameof(item.Quantity);
        string mainMethod = nameof(Main);

        Console.WriteLine();
        Console.WriteLine("--- nameof ---");
        Console.WriteLine($"  {nameof(InventoryItem)} → \"{typeName}\"");
        Console.WriteLine($"  {nameof(InventoryItem.Sku)} → \"{skuMember}\"");
        Console.WriteLine($"  {nameof(item.Quantity)} → \"{qtyFromInstance}\"");
        Console.WriteLine($"  {nameof(Main)} → \"{mainMethod}\"");

        ValidatePositive(item.Quantity, nameof(item.Quantity));
    }

    private static void ValidatePositive(int value, string parameterName)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "must be non-negative");
        }

        Console.WriteLine($"  ValidatePositive({value}, {parameterName}) — OK");
    }

    /*
     * =========================================================================
     * SECTION 8: default — DEFAULT LITERAL AND default(T)
     * =========================================================================
     *
     * default is a contextual keyword:
     *
     *   Form              | Meaning
     *   ------------------|--------------------------------------------------
     *   default             | Target-typed default (C# 7.1+)
     *   default(T)          | Explicit type — common in generics
     *   default struct      | All fields zeroed / null per field rules
     *
     * Reference types → null. Numeric → 0. bool → false. Nullable<T> → null.
     * Unassigned locals still fail CS0165 — default assigns a definite value.
     *
     * --- 6a. default literal with target typing ---
     * --- 6b. default(T) inside generic helper ---
     * -------------------------------------------------------------------------
     */
    private static void RunDefaultDemonstration()
    {
        string? emptySku = default;
        int zeroQty = default;
        decimal zeroPrice = default;
        InventoryItem? nullItem = default;

        Console.WriteLine();
        Console.WriteLine("--- default literal ---");
        Console.WriteLine($"  string: {emptySku ?? "(null)"}");
        Console.WriteLine($"  int: {zeroQty}");
        Console.WriteLine($"  decimal: {zeroPrice:C}");
        Console.WriteLine($"  InventoryItem?: {(nullItem is null ? "null" : "instance")}");

        InventoryDelta unsetDelta = default;
        Console.WriteLine($"  default struct Sku: \"{unsetDelta.Sku ?? "(null)"}\", Amount: {unsetDelta.Amount}");

        int parsedOrZero = ParseQuantityOrDefault("not-a-number");
        Console.WriteLine($"  ParseQuantityOrDefault(\"not-a-number\"): {parsedOrZero}");

        int defaultInt = GetDefaultValue<int>();
        InventoryDelta defaultDelta = GetDefaultValue<InventoryDelta>();
        Console.WriteLine($"  GetDefaultValue<int>(): {defaultInt}");
        Console.WriteLine($"  GetDefaultValue<InventoryDelta>().Amount: {defaultDelta.Amount}");
    }

    private static int ParseQuantityOrDefault(string text)
    {
        if (int.TryParse(text, out int qty))
        {
            return qty;
        }

        return default;
    }

    private static T GetDefaultValue<T>() where T : struct
    {
        return default;
    }

    /*
     * =========================================================================
     * SECTION 9: global:: — ROOT-QUALIFIED TYPE NAMES
     * =========================================================================
     *
     * global:: walks to the root namespace before resolving. Required when a
     * local type named Math (see SECTION 6) would win over System.Math
     * in unqualified lookup within this namespace.
     *
     *   Math.ApproxPi                  → local helper (SECTION 6)
     *   global::System.Math.PI         → BCL Math always
     *   global::System.Console.WriteLine → BCL Console
     *
     * Also used with using aliases: global::System.IO vs project-local IO helpers.
     * -------------------------------------------------------------------------
     */
    private static void RunGlobalQualifierDemonstration()
    {
        Console.WriteLine();
        Console.WriteLine("--- global:: ---");
        Console.WriteLine($"  local Math.ApproxPi: {Math.ApproxPi}");
        double bclPi = global::System.Math.PI;
        Console.WriteLine($"  global::System.Math.PI: {bclPi}");
        global::System.Console.WriteLine("  global::System.Console.WriteLine → BCL Console (this line)");
    }

    /*
     * =========================================================================
     * SECTION 10: CONTEXTUAL KEYWORDS — MEANING ONLY IN CONTEXT
     * =========================================================================
     *
     * Contextual keywords are not reserved globally — they are tokens with
     * special meaning only in certain grammatical positions. The compiler
     * parses them as keywords when the grammar expects a keyword; otherwise
     * they can be identifiers (often with @ prefix when ambiguous).
     *
     *   Keyword    | Context in this chapter        | Elsewhere / notes
     *   -----------|--------------------------------|---------------------------
     *   var        | Local type inference             | Not a field type without init
     *   dynamic    | Runtime binding type             | —
     *   nameof     | Name string expression           | —
     *   default    | Default value expression         | —
     *   global     | Only as global:: qualifier       | —
     *   partial    | Type/method split                | → OOP chapters
     *   where      | Generic constraint               | → LINQ query where
     *   yield      | Iterator return                  | → iterators chapter
     *   add/remove | Event accessors                  | → Events chapter
     *
     * @identifier — verbatim name lets you use keyword spellings as identifiers:
     *   int @var = 10;   class @event { }   — valid where grammar allows identifiers.
     *
     * COVERED IN DETAIL LATER → LINQ (from, select, where), Async (async, await),
     *   Properties (get, set, init), Records (record, with).
     * -------------------------------------------------------------------------
     */
    private static void RunContextualKeywordsDemonstration()
    {
        int @var = 10;
        string @default = "stored as identifier via @";

        Console.WriteLine();
        Console.WriteLine("--- contextual keywords & @identifiers ---");
        Console.WriteLine($"  @var holds { @var } — not the var inference keyword here");
        Console.WriteLine($"  @{nameof(@default)} = \"{@default}\"");
        Console.WriteLine("  var inferred = @var * 2;  // var keyword in type position → int");
        var inferred = @var * 2;
        Console.WriteLine($"  inferred type: {inferred.GetType().Name}, value: {inferred}");
    }

    /*
     * =========================================================================
     * SECTION 4 (continued): volatile — WORKER THREAD DEMO
     * =========================================================================
     *
     * PriceRefreshSignal (SECTION 4) polled on a worker thread; main thread
     * calls RequestStop() — volatile ensures the worker observes the flag.
     * -------------------------------------------------------------------------
     */
    private static void RunVolatileDemonstration()
    {
        var refreshSignal = new PriceRefreshSignal();
        int pollResult = 0;

        Thread worker = new(() =>
        {
            pollResult = refreshSignal.PollUntilStopped(maxIterations: 500);
        });

        worker.Start();
        Thread.Sleep(20);
        refreshSignal.RequestStop();
        worker.Join();

        Console.WriteLine();
        Console.WriteLine("--- volatile stop flag (PriceRefreshSignal) ---");
        Console.WriteLine($"  Worker stopped after {pollResult} polls; StopRequested={refreshSignal.StopRequested}");
    }

    /*
     * =========================================================================
     * SECTION 11: ref / out / in / ref return — ADVANCED RECAP
     * =========================================================================
     *
     * Basics in 07. Methods. Patterns common in library and performance code:
     *
     *   ref       in-out alias; caller initializes
     *   out       callee assigns; TryParse pattern
     *   in        read-only alias; avoids struct copy
     *   ref return  alias to array slot or field
     *   out _     discard unwanted out values
     * -------------------------------------------------------------------------
     */
    private static void RunRefOutInDemonstration(InventoryItem[] stock)
    {
        int adjustmentDelta = 5;
        ApplyDelta(ref adjustmentDelta, 3);

        Console.WriteLine();
        Console.WriteLine("--- ref parameter ---");
        Console.WriteLine($"  adjustmentDelta after ref ApplyDelta(+3): {adjustmentDelta}");

        bool reserved = TryReserveStock(stock[2], requested: 3, out int reservedQty, out string message);

        Console.WriteLine();
        Console.WriteLine("--- out parameters ---");
        Console.WriteLine($"  Reserve 3× {stock[2].Sku}: success={reserved}, qty={reservedQty}, note={message}");

        if (TryFindIndex(stock, "WH-220", out int index))
        {
            ref InventoryItem slot = ref FindSlot(stock, index);
            slot.UnitPrice = 119.00m;

            Console.WriteLine();
            Console.WriteLine("--- ref return ---");
            Console.WriteLine($"  WH-220 price updated via ref return: {stock[index].UnitPrice:C}");
        }

        var delta = new InventoryDelta("WH-100", -2);
        string auditLine = FormatAdjustment(in delta, stock[index].UnitPrice);

        Console.WriteLine();
        Console.WriteLine("--- in parameter ---");
        Console.WriteLine($"  Audit: {auditLine}");

        bool parsed = int.TryParse("42", out _);

        Console.WriteLine();
        Console.WriteLine("--- out discard ---");
        Console.WriteLine($"  int.TryParse(\"42\", out _): {parsed}");
    }

    /*
     * =========================================================================
     * SECTION 12: NAMED PARAMETERS — PREVIEW
     * =========================================================================
     *
     * Named arguments supply parameters by name at the call site. Positional
     * arguments must precede named ones. Works with optional parameters.
     *
     * COVERED IN DETAIL LATER → 01. C# Language Fundamentals / 07. Methods
     * -------------------------------------------------------------------------
     */
    private static void RunNamedParametersPreview()
    {
        string label = PrintShippingLabel(sku: "WH-100", copies: 2, includePrice: true);

        Console.WriteLine();
        Console.WriteLine("--- named parameters (preview) ---");
        Console.WriteLine($"  Label: {label}");
    }

    private static void PrintStock(IEnumerable<InventoryItem> items)
    {
        foreach (InventoryItem item in items)
        {
            Console.WriteLine($"  {item.Sku,-8} {item.Name,-18} qty={item.Quantity,3} @ {item.UnitPrice,8:C}");
        }
    }

    private static void ApplyDelta(ref int value, int amount) => value += amount;

    private static bool TryReserveStock(InventoryItem item, int requested, out int reservedQty, out string message)
    {
        if (requested <= 0)
        {
            reservedQty = 0;
            message = "requested must be positive";
            return false;
        }

        if (item.Quantity < requested)
        {
            reservedQty = 0;
            message = $"only {item.Quantity} available";
            return false;
        }

        reservedQty = requested;
        message = "reserved";
        return true;
    }

    private static bool TryFindIndex(InventoryItem[] stock, string sku, out int index)
    {
        for (int i = 0; i < stock.Length; i++)
        {
            if (stock[i].Sku == sku)
            {
                index = i;
                return true;
            }
        }

        index = -1;
        return false;
    }

    private static ref InventoryItem FindSlot(InventoryItem[] stock, int index) => ref stock[index];

    private static string FormatAdjustment(in InventoryDelta delta, decimal unitPrice)
    {
        decimal impact = delta.Amount * unitPrice;
        return $"{delta.Sku} delta {delta.Amount:+0;-#} → {impact:C} impact";
    }

    private static string PrintShippingLabel(string sku, int copies = 1, bool includePrice = false)
    {
        string pricePart = includePrice ? " [price on label]" : string.Empty;
        return $"{sku} × {copies} label(s){pricePart}";
    }
}

/*
 * =========================================================================
 * QUICK REFERENCE — var, dynamic & SPECIAL KEYWORDS
 * =========================================================================
 *
 * --- var (compile-time) ---
 *
 *   var x = expr;              type inferred; still static
 *   var anon = new { A = 1 };  required for anonymous types
 *   var x;                      → CS0818 must initialize
 *
 * --- dynamic (runtime / DLR) ---
 *
 *   dynamic d = obj;
 *   d.Member;                   resolved at runtime
 *   RuntimeBinderException      missing member or bad types
 *
 * --- nameof ---
 *
 *   nameof(Type.Member)         compile-time string "Member"
 *   nameof(instance.Prop)       "Prop" — instance not evaluated
 *   Use for exceptions, reflection keys, logging
 *
 * --- default ---
 *
 *   T x = default;              target-typed zero/null
 *   default(T)                  explicit in generics
 *   default struct              all fields zeroed
 *
 * --- global:: ---
 *
 *   global::System.Console      root BCL namespace
 *   Disambiguates shadowing nested types/namespaces
 *
 * --- Contextual keywords ---
 *
 *   var, dynamic, nameof, default, global — this chapter
 *   partial, where, yield, async, from, … — other chapters
 *   @name                       use keyword spelling as identifier
 *
 * --- volatile ---
 *
 *   private volatile bool _flag;  field-only; cross-thread visibility
 *
 * --- ref / out / in ---
 *
 *   ref / out / in / ref return / out _ — see 07. Methods
 *
 * --- Common mistakes ---
 *
 *  Mistake                              | Result
 *  -------------------------------------|----------------------------------
 *  Treating var as runtime-typed          | Still compile-time checked
 *  dynamic typo (obj.Nmae)                | RuntimeBinderException
 *  nameof for generic arity               | "List" not "List`1"
 *  volatile on local                      | CS0106
 *  Forgetting global:: when shadowed      | Resolves wrong namespace
 *
 * --- Related chapters ---
 *
 *   02. Data Types and Variables    var introduction
 *   07. Methods                     ref/out, named & optional params
 *   02. Reflection & Attributes     reflection depth (this module)
 *   06. Multithreading & Async      locks, Interlocked
 *
 * =========================================================================
 */
