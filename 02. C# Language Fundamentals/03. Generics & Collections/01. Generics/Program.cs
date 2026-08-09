/*
 * TOPIC: Generics — type parameters (T, TKey, TValue) that let you write one
 *        class, method, struct, interface, or delegate and reuse it for many
 *        concrete types while keeping compile-time type safety and avoiding
 *        boxing for value types.
 *
 * WHY IT MATTERS:
 *   Before generics, collections stored everything as object. That meant runtime
 *   casts, InvalidCastException surprises, and boxing overhead for ints and
 *   decimals. Generics let the compiler verify types at compile time — the same
 *   List<T> pattern powers most modern .NET APIs.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Why generics exist (type safety, no boxing vs object/ArrayList)
 *   2.  Type parameters and closed constructed types
 *   3.  Generic classes
 *   4.  Generic methods
 *   5.  Generic interfaces
 *   6.  Generic structs
 *   7.  Generic delegates (custom + Action/Func/Predicate)
 *   8.  Generic constraints (class, struct, new(), base type, interface)
 *   9.  default(T) and typeof(T) vs default
 *  10.  Generic collections overview (preview)
 *  11.  Non-generic collections preview (Hashtable, Queue, Stack, SortedList)
 *  12.  Covariance and contravariance basics (in/out)
 */

using System;
using System.Collections;
using System.Collections.Generic;

namespace Generics;

/*
 * SECTION 1: WHY GENERICS EXIST — TYPE SAFETY AND NO BOXING
 *
 * Earlier chapters previewed object and boxing: storing an int in object
 * wraps it on the heap; casting back requires the exact type.
 *
 *   object box = 42;
 *   int n = (int)box;              // unbox — OK
 *   long bad = (long)box;          // InvalidCastException
 *
 * Non-generic collections (ArrayList, Hashtable) store object references.
 * Every value type inserted is boxed; every read needs a cast.
 *
 * Generics replace object with a TYPE PARAMETER:
 *
 *   List<int> counts = new List<int>();
 *   counts.Add(42);                // no boxing
 *   int value = counts[0];         // no cast
 *
 * The compiler generates (or shares) specialized code for each T you use.
 * Mistakes like adding a string to List<int> fail at compile time (CS1503),
 * not at runtime with InvalidCastException.
 *
 * Scenario: warehouse inventory — SKUs, quantities, and lookup tables.
 */
public static class LegacyCollectionProbe
{
    public static (int genericFirst, int legacyFirst, bool castTrapCaught) CompareListToArrayList()
    {
        List<int> palletCounts = new List<int>();
        palletCounts.Add(24);
        palletCounts.Add(36);
        int genericFirst = palletCounts[0]; // direct int — no cast

        ArrayList legacyCounts = new ArrayList();
        legacyCounts.Add(24); // int boxed to object on the heap
        object legacyFirstBox = legacyCounts[0] ?? throw new InvalidOperationException("Expected boxed count.");
        int legacyFirst = (int)legacyFirstBox; // unbox + cast required

        bool castTrapCaught = false;
        try
        {
            ArrayList mixedSkus = new ArrayList();
            mixedSkus.Add("WH-4412");
            object stringEntry = mixedSkus[0] ?? throw new InvalidOperationException("Expected mixed SKU entry.");
            int wrong = (int)stringEntry; // InvalidCastException — string is not int
            _ = wrong;
        }
        catch (InvalidCastException)
        {
            castTrapCaught = true;
        }

        return (genericFirst, legacyFirst, castTrapCaught);
    }
}

/*
 * SECTION 2: TYPE PARAMETERS AND CLOSED CONSTRUCTED TYPES
 *
 * A TYPE PARAMETER is a placeholder declared in angle brackets:
 *
 *   T          most common name — any single type parameter
 *   TKey       key half of a pair (Dictionary<TKey, TValue>)
 *   TValue     value half of a pair
 *   TItem      element type in a collection abstraction
 *
 * At a USE SITE you supply concrete types — that is a CLOSED CONSTRUCTED TYPE:
 *
 *   WarehouseSlot<string>   and   WarehouseSlot<decimal>
 *   are two different types — you cannot assign one to the other.
 *
 * The compiler treats each closed type separately for type checking.
 */

/*
 * SECTION 3: GENERIC CLASSES
 *
 * A GENERIC CLASS declares one or more type parameters:
 *
 *   class WarehouseSlot<T> { … }
 *
 * T stands in for a concrete type at compile time. The same class definition
 * works for string SKUs, decimal weights, or custom record types.
 *
 *   WarehouseSlot<string> skuSlot = new WarehouseSlot<string>("A-12", "WH-4412");
 *   WarehouseSlot<decimal> weightSlot = new WarehouseSlot<decimal>("B-03", 18.75m);
 */
public class WarehouseSlot<T>
{
    private readonly string _zone;
    private T _payload;

    public WarehouseSlot(string zone, T initialPayload)
    {
        _zone = zone;
        _payload = initialPayload;
    }

    public string Zone => _zone;

    public T Payload
    {
        get => _payload;
        set => _payload = value;
    }

    public string Describe()
    {
        return $"{_zone}: {(_payload?.ToString() ?? "empty")}";
    }
}

/*
 * SECTION 4: GENERIC INTERFACES
 *
 * Interfaces can be generic — common for repositories, comparers, factories,
 * and lookup abstractions:
 *
 *   interface IStockLookup<TKey, TItem> { … }
 *
 * Implementations close the type parameters once:
 *
 *   class InMemoryStockLookup<TKey, TItem> : IStockLookup<TKey, TItem>
 *
 * where TKey : notnull ensures dictionary keys are never null (nullable reference types).
 */
public interface IStockLookup<TKey, TItem> where TKey : notnull
{
    bool TryGet(TKey key, out TItem item);

    void Set(TKey key, TItem item);
}

public sealed class InMemoryStockLookup<TKey, TItem> : IStockLookup<TKey, TItem> where TKey : notnull
{
    private readonly Dictionary<TKey, TItem> _items = new Dictionary<TKey, TItem>();

    public bool TryGet(TKey key, out TItem item) => _items.TryGetValue(key, out item!);

    public void Set(TKey key, TItem item) => _items[key] = item;
}

/*
 * SECTION 5: GENERIC STRUCTS
 *
 * Structs can be generic too. Value semantics (copy on assignment) combine with
 * type parameters for lightweight typed wrappers:
 *
 *   struct Quantity<TUnit> where TUnit : struct { … }
 *
 * Generic structs follow the same constraint rules as generic classes.
 * They cannot inherit from other types (structs already implicitly inherit ValueType)
 * but can implement interfaces.
 */
public struct Quantity<TUnit> where TUnit : struct
{
    public int Count { get; set; }

    public TUnit Unit { get; set; }

    public string Format()
    {
        return $"{Count} {Unit}";
    }
}

/*
 * SECTION 6: GENERIC DELEGATES
 *
 * Delegates can carry type parameters — a typed function signature:
 *
 *   delegate TResult Mapper<TInput, TResult>(TInput input);
 *
 * The BCL ships generic delegate types you use daily:
 *
 *   Action<T>              void Method(T arg)
 *   Func<T, TResult>       TResult Method(T arg)
 *   Predicate<T>           bool Method(T arg)
 *
 * Generic methods and lambdas infer type arguments from context.
 */
public delegate TResult Mapper<TInput, TResult>(TInput input);

/*
 * SECTION 7: GENERIC CONSTRAINTS — InventoryItem SUPPORT TYPE
 *
 * Constraints limit which types may substitute for T. They appear after the
 * parameter list with the where keyword:
 *
 *   where T : class              reference type only (not struct)
 *   where T : struct             non-nullable value type
 *   where T : new()              must have public parameterless ctor
 *   where T : SomeBase           must inherit or implement SomeBase
 *   where T : IComparable<T>     must implement interface
 *
 * Multiple constraints combine on one line:
 *
 *   where T : class, ICloneable, new()
 *
 * --- 7a. Base class constraint ---
 *   where T : StockEntry   T must inherit StockEntry — access shared Sku field
 *
 * InventoryItem inherits StockEntry and implements IComparable<InventoryItem> and
 * ICloneable so constraint demos in Program can call CompareTo and Clone safely.
 */
public abstract class StockEntry
{
    public string Sku { get; set; } = string.Empty;
}

public sealed class InventoryItem : StockEntry, IComparable<InventoryItem>, ICloneable
{
    public int Units { get; set; }

    public int CompareTo(InventoryItem? other)
    {
        if (other is null)
        {
            return 1;
        }

        return string.Compare(Sku, other.Sku, StringComparison.Ordinal);
    }

    public object Clone()
    {
        return new InventoryItem { Sku = Sku, Units = Units };
    }

    public override string ToString() => $"{Sku} ({Units} units)";
}

public class Program
{
    /*
     * SECTION 8: GENERIC METHODS
     *
     * Methods declare their own type parameters, independent of the containing class:
     *
     *   static void Swap<T>(ref T a, ref T b) { … }
     *
     * The compiler infers T from arguments when possible:
     *
     *   Swap(ref x, ref y);          // T inferred as int
     *   Swap<int>(ref x, ref y);     // explicit — same result
     *
     * Generic methods are useful for algorithms that work identically on many
     * types (swap, find max, factory) without duplicating code per type.
     */
    public static void Swap<T>(ref T left, ref T right)
    {
        T temp = left;
        left = right;
        right = temp;
    }

    public static string DescribeType<T>(T value)
    {
        return $"{typeof(T).Name}: {value}";
    }

    /*
     * SECTION 9: GENERIC CONSTRAINTS — where CLAUSES ON METHODS
     *
     * --- 9a. new() — factory default ---
     *   where T : new()   allows `return new T();` inside the method body
     *
     * --- 9b. IComparable<T> — ordering ---
     *   where T : IComparable<T>   allows left.CompareTo(right)
     *
     * --- 9c. class — reference-type operations ---
     *   where T : class   T may be null; reference semantics only
     *
     * --- 9d. struct — value-type operations ---
     *   where T : struct   non-nullable value type; no null assignment to T
     *
     * --- 9e. Base class constraint ---
     *   where T : StockEntry   access Sku on any derived inventory type
     *
     * --- 9f. Multiple constraints on one parameter ---
     *   where T : class, ICloneable, new()
     *
     * Unsatisfied constraints produce CS0452 / CS0311 at compile time.
     */
    public static string DescribeStockEntry<T>(T item) where T : StockEntry
    {
        return item.Sku;
    }

    public static T CreateDefault<T>() where T : new()
    {
        return new T();
    }

    public static int CompareOrdered<T>(T left, T right) where T : IComparable<T>
    {
        return left.CompareTo(right);
    }

    public static string DescribeReference<T>(T item) where T : class
    {
        return item is null ? "null reference" : item.ToString() ?? typeof(T).Name;
    }

    public static T EchoValue<T>(T value) where T : struct
    {
        return value;
    }

    public static T CloneItem<T>(T item) where T : class, ICloneable, new()
    {
        return (T)item.Clone();
    }

    /*
     * SECTION 10: default(T) AND typeof(T)
     *
     * default(T) returns the zero-value for T:
     *   int        → 0
     *   decimal    → 0m
     *   string     → null
     *   bool       → false
     *
     * typeof(T) returns System.Type metadata for the type parameter — used for
     * reflection, logging, and generic factories. It does NOT create an instance.
     *
     *   typeof(int)     → System.Int32 (compile-time constant for closed types)
     *   typeof(T)       → resolved at runtime for the closed constructed type
     *   default(T)      → zero/null value of T
     *
     * With nullable reference types enabled, default(string) is null — annotate
     * accordingly when assigning to non-nullable string variables.
     */
    public static string DescribeDefault<T>()
    {
        T value = default!;
        Type type = typeof(T);
        string display = value is null ? "null" : value.ToString() ?? type.Name;
        return $"{type.Name} default(T) = {display}";
    }

    public static bool IsReferenceType<T>() where T : class
    {
        return !typeof(T).IsValueType;
    }

    /*
     * SECTION 11: GENERIC COLLECTIONS OVERVIEW (PREVIEW)
     *
     * The BCL ships dozens of generic collection types in System.Collections.Generic:
     *
     *   List<T>                    ordered, indexable sequence
     *   Dictionary<TKey,TValue>    key → value map
     *   HashSet<T>                 unique items, fast membership
     *   Queue<T> / Stack<T>        FIFO / LIFO
     *   SortedList / SortedDictionary   sorted by key
     *
     * They replace legacy non-generic types for new code. This module dedicates
     * separate chapters to the most common ones.
     *
     * COVERED IN DETAIL LATER → 03. List, 04. Dictionary, 05. HashSet,
     *   06. Queue and Stack, 07. SortedList and SortedDictionary
     */
    public static decimal PreviewUnitPrice()
    {
        Dictionary<string, decimal> unitPrices = new Dictionary<string, decimal>();
        unitPrices["WH-4412"] = 49.99m;
        unitPrices["WH-9901"] = 12.50m;
        return unitPrices["WH-4412"];
    }

    /*
     * SECTION 12: NON-GENERIC COLLECTIONS (PREVIEW)
     *
     * Legacy types live in System.Collections (no Generic suffix):
     *
     *   ArrayList     growable array of object references
     *   Hashtable     key/value map with object keys and values
     *   Queue         FIFO of object references
     *   Stack         LIFO of object references
     *   SortedList    sorted key/value pairs as object
     *
     * All accept any object; value types are boxed on insert. Prefer generic
     * equivalents unless an old API forces the legacy type.
     *
     * COVERED IN DETAIL LATER → 02. ArrayList (ArrayList + Hashtable depth),
     *   06. Queue and Stack, 07. SortedList and SortedDictionary
     */
    public static string PreviewLegacyCollectionNames()
    {
        return string.Join(", ", new[] { nameof(ArrayList), nameof(Hashtable), nameof(Queue), nameof(Stack), nameof(SortedList) });
    }

    /*
     * SECTION 13: COVARIANCE AND CONTRAVARIANCE (in / out)
     *
     * Usually generic types are INVARIANT — List<string> is not a List<object>.
     * Some interfaces declare variance modifiers on type parameters:
     *
     *   out T  — COVARIANT:  IEnumerable<string> assignable to IEnumerable<object>
     *   in T   — CONTRAVARIANT: IComparer<object> assignable to IComparer<string>
     *
     * Covariance (out): producer — you only GET T items out (read-only from caller view).
     * Contravariance (in): consumer — you only PUT T items in (compare, handle).
     *
     * List<T> is NOT covariant — you could Add any object if it were.
     * IEnumerable<T> is covariant because it only yields items.
     *
     * COVERED IN DETAIL LATER → 08. IEnumerable and IEnumerator (iteration + variance usage)
     */
    public static (bool covariantOk, bool contravariantOk) PreviewVariance()
    {
        List<string> skuList = new List<string> { "WH-4412", "WH-9901" };
        IEnumerable<string> skuSequence = skuList;
        IEnumerable<object> skuAsObjects = skuList; // covariant — out T on IEnumerable<T>

        IComparer<object> objectComparer = Comparer<object>.Default;
        IComparer<string> stringComparer = objectComparer; // contravariant — in T on IComparer<T>

        int objectCompare = objectComparer.Compare("a", "b");
        int stringCompare = stringComparer.Compare("WH-4412", "WH-9901");

        bool covariantOk = skuAsObjects is IEnumerable<object>;
        bool contravariantOk = objectCompare != 0 && stringCompare != 0;
        _ = skuSequence;

        return (covariantOk, contravariantOk);
    }

    /*
     * SECTION 14: DEMONSTRATION — Main orchestrates the chapter demo
     */
    public static void Main(string[] args)
    {
        (int genericFirst, int legacyFirst, bool castTrapCaught) = LegacyCollectionProbe.CompareListToArrayList();

        WarehouseSlot<string> skuSlot = new WarehouseSlot<string>("A-12", "WH-4412");
        WarehouseSlot<decimal> weightSlot = new WarehouseSlot<decimal>("B-03", 18.75m);
        skuSlot.Payload = "WH-9901";
        weightSlot.Payload = weightSlot.Payload + 2.5m;

        string skuA = "WH-1001";
        string skuB = "WH-2002";
        Swap(ref skuA, ref skuB);

        decimal priceA = 19.99m;
        decimal priceB = 29.99m;
        Swap(ref priceA, ref priceB);

        string typeLabel = DescribeType(skuSlot.Payload);

        IStockLookup<string, int> stockBySku = new InMemoryStockLookup<string, int>();
        stockBySku.Set("WH-4412", 120);
        stockBySku.Set("WH-9901", 45);
        bool found = stockBySku.TryGet("WH-4412", out int onHand);
        bool missing = stockBySku.TryGet("WH-0000", out int notFoundQty);

        Quantity<int> palletQty = new Quantity<int> { Count = 24, Unit = 1 }; // Unit = pallet size id
        string qtyLabel = palletQty.Format();

        Mapper<string, string> skuFormatter = sku => $"SKU:{sku.ToUpperInvariant()}";
        string formattedSku = skuFormatter("wh-4412");

        Func<int, string> unitsLabel = count => $"{count} units";
        string unitsText = unitsLabel(120);

        Predicate<string> isWarehouseSku = sku => sku.StartsWith("WH-", StringComparison.Ordinal);
        bool validSku = isWarehouseSku("WH-4412");

        InventoryItem defaultItem = CreateDefault<InventoryItem>();
        defaultItem.Sku = "WH-DEFAULT";

        int compareResult = CompareOrdered(
            new InventoryItem { Sku = "WH-A", Units = 10 },
            new InventoryItem { Sku = "WH-B", Units = 5 });

        string referenceReport = DescribeReference(defaultItem);
        string baseConstraintReport = DescribeStockEntry(defaultItem);

        int boxedUnits = 48;
        int processedUnits = EchoValue(boxedUnits);

        InventoryItem original = new InventoryItem { Sku = "WH-CLONE", Units = 8 };
        InventoryItem cloned = CloneItem(original);

        string intDefaultReport = DescribeDefault<int>();
        string stringDefaultReport = DescribeDefault<string>();
        bool referenceTypeCheck = IsReferenceType<InventoryItem>();

        decimal previewPrice = PreviewUnitPrice();
        string legacyNames = PreviewLegacyCollectionNames();
        (bool covariantOk, bool contravariantOk) = PreviewVariance();

        Console.WriteLine("=== Generics — Warehouse Inventory ===");
        Console.WriteLine($"Why generics: List[0]={genericFirst}, ArrayList unboxed={legacyFirst}, cast trap={castTrapCaught}");
        Console.WriteLine($"Generic class: {skuSlot.Describe()} | {weightSlot.Describe()}");
        Console.WriteLine($"After Swap: skuA={skuA}, skuB={skuB}; prices {priceA:C} / {priceB:C}");
        Console.WriteLine($"Generic method: {typeLabel}");
        Console.WriteLine($"IStockLookup: WH-4412 on hand={onHand} (found={found}), missing qty={notFoundQty} (found={missing})");
        Console.WriteLine($"Generic struct: {qtyLabel}");
        Console.WriteLine($"Generic delegate: {formattedSku}; Func={unitsText}; Predicate valid={validSku}");
        Console.WriteLine($"Constraints: default={defaultItem.Sku}, compare={compareResult}, ref={referenceReport}, base={baseConstraintReport}");
        Console.WriteLine($"Struct constraint: {processedUnits} units; clone={cloned.Sku}/{cloned.Units} from {original.Sku}");
        Console.WriteLine($"default(T): {intDefaultReport}; {stringDefaultReport}; typeof check ref={referenceTypeCheck}");
        Console.WriteLine($"Collections preview: WH-4412 price={previewPrice:C}");
        Console.WriteLine($"Legacy preview types: {legacyNames}");
        Console.WriteLine($"Variance: covariant={covariantOk}, contravariant={contravariantOk}");
    }
}

/*
 * QUICK REFERENCE — GENERICS
 *
 * --- Syntax ---
 *
 *   class Name<T> { … }                    generic class
 *   struct Name<T> { … }                   generic struct
 *   interface IName<T> { … }               generic interface
 *   delegate TR Method<T>(T arg);          generic delegate
 *   ReturnType Method<T>(T arg) { … }      generic method
 *   Name<int> instance = new Name<int>();  closed constructed type
 *
 * --- Type parameters ---
 *
 *   T, TKey, TValue, TItem                 naming conventions (T is most common)
 *   WarehouseSlot<string> vs WarehouseSlot<decimal>   distinct closed types
 *
 * --- Constraints (where) ---
 *
 *   where T : class              reference type
 *   where T : struct             value type (excludes Nullable<T> unless T?)
 *   where T : new()              parameterless constructor
 *   where T : BaseOrInterface    inheritance / implementation
 *   where T : U                  T must be or derive from type param U
 *
 * --- default(T) vs typeof(T) ---
 *
 *   default(T)                   zero/null value for T (0, false, null, …)
 *   typeof(T)                    Type metadata — reflection, logging, factories
 *
 * --- Why use generics ---
 *
 *   Compile-time type safety     no InvalidCastException from collection reads
 *   No boxing for value types    List<int> stores ints directly
 *   Reusable algorithms          one Swap<T> for all T
 *
 * --- Generic collections (preview) ---
 *
 *   List<T>, Dictionary<,>, HashSet<T>, Queue<T>, Stack<T>   System.Collections.Generic
 *
 * --- Legacy non-generic (preview) ---
 *
 *   ArrayList, Hashtable, Queue, Stack, SortedList   System.Collections — object, boxing
 *
 * --- Variance ---
 *
 *   IEnumerable<out T>           covariant — IEnumerable<string> → IEnumerable<object>
 *   IComparer<in T>              contravariant — IComparer<object> → IComparer<string>
 *   List<T>                      invariant — no List<string> to List<object>
 *
 * --- Common mistakes ---
 *
 *  Mistake                              | Result
 *  -------------------------------------|----------------------------------
 *  Wrong type in List<T>                | CS1503 at compile time
 *  Bad cast from ArrayList              | InvalidCastException at runtime
 *  Constraint not satisfied             | CS0452 / CS0311
 *  new() on type without ctor           | CS0310
 *  Assume List<string> is List<object>  | CS0029 — List<T> is invariant
 *  Confuse typeof(T) with default(T)     | typeof = metadata; default = zero/null
 *
 * --- Later chapters ---
 *
 *   ArrayList / Hashtable        → 02. ArrayList
 *   List<T> depth                → 03. List
 *   Dictionary<K,V> depth        → 04. Dictionary
 *   HashSet<T>                   → 05. HashSet
 *   Queue<T> / Stack<T>          → 06. Queue and Stack
 *   Sorted collections           → 07. SortedList and SortedDictionary
 *   IEnumerable variance usage   → 08. IEnumerable and IEnumerator
 */
