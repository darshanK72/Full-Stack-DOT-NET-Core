/*
 * =============================================================================
 * 03. CONSTRUCTORS AND METHOD OVERLOADING — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: Object initialization with constructors and compile-time method
 *        selection when multiple methods share the same name — including
 *        params arrays, optional parameters, and named arguments.
 *
 * WHY IT MATTERS:
 *   A newly allocated object starts with default field values — often not a
 *   valid business state. Constructors enforce invariants at creation time
 *   (required IDs, positive prices, wired dependencies). Method overloading
 *   lets callers express intent with natural argument shapes while the compiler
 *   picks the matching signature — no runtime dispatch cost.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Why constructors are needed
 *   2.  Default (parameterless) constructor
 *   3.  Parameterized constructor
 *   4.  Constructor chaining with : this(...)
 *   5.  Copy-constructor pattern (C# has no copy keyword)
 *   6.  Static constructor (type initializer)
 *   7.  Private constructor (singleton intro)
 *   8.  Preview: constructor chaining with : base(...)
 *   9.  Destructor / finalizer (~ClassName)
 *  10.  Preview: Finalize vs Dispose
 *  11.  Method overloading rules
 *  12.  params arrays — variable argument lists
 *  13.  Optional parameters — default values
 *  14.  Named arguments — skip parameters by name
 *  15.  Overload resolution and ambiguity (CS0121)
 *
 * =============================================================================
 */

using System;
using System.Globalization;

namespace ConstructorsAndMethodOverloading;

/*
 * SECTION 1: WHY CONSTRUCTORS ARE NEEDED
 *
 * Without a constructor, reference-type fields stay null and numeric fields
 * stay 0. That is rarely a usable Product in an inventory system.
 *
 * A constructor runs immediately after memory allocation and before the
 * reference is returned to the caller — your one guaranteed hook to put
 * the object into a valid, consistent state.
 *
 * Compare:
 *   Product p = new Product();              // fields: null / 0 until set
 *   Product p = new Product("Pen", 2.49m); // invariant enforced at birth
 *
 * Scenario: a small warehouse registers products, clones a bestseller,
 * prices line items with overloaded helpers, and tracks IDs via a
 * singleton registry whose static constructor seeds counters.
 *
 * --- 1a. Default (parameterless) constructor ---
 *
 * If you declare NO constructors, the compiler emits a public parameterless
 * one automatically. The moment you add ANY constructor, that default is
 * NOT generated — you must declare parameterless yourself if still needed.
 *
 * --- 1b. Parameterized constructor ---
 *
 * Accepts arguments the object needs to be meaningful. Parameters become
 * the authoritative source for initial field values.
 *
 * --- 1c. Constructor chaining — : this(...) ---
 *
 * One constructor delegates to another in the SAME class:
 *
 *   public Product() : this("Unnamed", 0m) { }
 *   public Product(string name) : this(name, 0m) { }
 *   public Product(string name, decimal price) { ... validate ... }
 *
 * Chaining must appear in the constructor header — before the body { }.
 * Only one constructor initializer per ctor (: this(...) OR : base(...)).
 *
 * --- 1d. Copy-constructor pattern ---
 *
 * C# has no "copy constructor" keyword. The pattern is a constructor that
 * accepts an instance of the same type and copies field values (often
 * producing a new SKU while reusing descriptive data).
 *
 * --- 1e. Static constructor (type initializer) ---
 *
 *   static Product() { ... }
 *
 * Runs exactly once per application domain, before any static or instance
 * member of the type is accessed, in a thread-safe manner. Use for
 * expensive one-time setup — not per-object initialization.
 *
 * Compile note: adding any instance constructor removes the compiler-generated
 * parameterless ctor — callers that still use new Product() need CS7036 fix
 * unless you add public Product() yourself.
 */
public class Product
{
    public string Name { get; }
    public decimal UnitPrice { get; }
    public int Sku { get; }

    public static int NextSku { get; private set; }

    static Product()
    {
        NextSku = 1000;
    }

    public Product()
        : this("Unnamed Product", 0m)
    {
    }

    public Product(string name)
        : this(name, 0m)
    {
    }

    public Product(string name, decimal unitPrice)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Product name is required.", nameof(name));
        }

        if (unitPrice < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(unitPrice), "Unit price cannot be negative.");
        }

        Name = name.Trim();
        UnitPrice = unitPrice;
        Sku = NextSku;
        NextSku++;
    }

    public Product(Product source)
        : this(source.Name + " (Copy)", source.UnitPrice)
    {
    }

    public string Describe()
    {
        return $"SKU {Sku}: {Name} @ {UnitPrice.ToString("F2", CultureInfo.InvariantCulture)}";
    }
}

/*
 * SECTION 2: PRIVATE CONSTRUCTOR — SINGLETON INTRO
 *
 * A private constructor prevents external code from calling new. Combined
 * with a static Instance property, only one object exists — the singleton.
 *
 * Typical shape:
 *   private InventoryRegistry() { }
 *   public static InventoryRegistry Instance => SharedInstance;
 *
 * Use sparingly; prefer dependency injection in larger apps. Here it
 * centralizes SKU registration for the demo warehouse.
 *
 * COVERED IN DETAIL LATER → 04. Static Members and Static Classes
 *   (headline concepts only: static fields, lazy initialization patterns)
 */
public sealed class InventoryRegistry
{
    public static string StartupLabel { get; }

    private static readonly InventoryRegistry SharedInstance = new InventoryRegistry();
    private int _registeredCount;
    private string _lastRegisteredName = "(none)";

    static InventoryRegistry()
    {
        StartupLabel = "InventoryRegistry static ctor ran — singleton ready";
    }

    private InventoryRegistry()
    {
    }

    public static InventoryRegistry Instance => SharedInstance;

    public int RegisteredCount => _registeredCount;

    public string LastRegisteredName => _lastRegisteredName;

    public void Register(Product product)
    {
        _registeredCount++;
        _lastRegisteredName = product.Name;
    }
}

/*
 * SECTION 3: CONSTRUCTOR CHAINING WITH : base(...) — PREVIEW
 *
 * Derived classes forward arguments to a parent constructor:
 *
 *   public DerivedEmployee(int id, string name) : base(id, name) { }
 *
 * Without : base(...), the parameterless base constructor runs (if it exists).
 * If the base has only parameterized constructors and you omit : base(...),
 * the compiler reports CS7036 (no suitable constructor).
 *
 * You cannot chain both : this(...) and : base(...) on the same constructor —
 * that causes CS2506 (only one constructor initializer allowed).
 *
 * COVERED IN DETAIL LATER → 05. Inheritance and Polymorphism
 *   (headline concepts only: inheritance syntax, base keyword, ctor order)
 */

/*
 * SECTION 4: DESTRUCTOR / FINALIZER
 *
 *   ~ClassName() { ... }
 *
 * Also called a finalizer. The runtime calls it (non-deterministically)
 * before reclaiming memory when the object becomes eligible for GC.
 *
 * Rules:
 *   - Cannot specify access modifiers or parameters
 *   - One finalizer per class (no ~ in structs)
 *   - Do NOT rely on finalizers for timely cleanup — use IDisposable
 *
 * FinalizerRunCount increments when GC collects the instance so the demo
 * can observe behavior. Production code rarely implements finalizers.
 */
public class TempFileHandle
{
    private readonly string _path;

    public static int FinalizerRunCount { get; set; }

    public TempFileHandle(string path)
    {
        _path = path;
    }

    public string OpenMessage => $"Temp handle opened for {_path}";

    ~TempFileHandle()
    {
        FinalizerRunCount++;
    }
}

/*
 * SECTION 5: FINALIZE vs DISPOSE — PREVIEW
 *
 * COVERED IN DETAIL LATER → Memory Management + IDisposable
 *   (headline concepts only: deterministic cleanup vs GC finalization)
 *
 * Finalize (~ClassName): non-deterministic; runs on GC thread; backup
 *   safety net for unmanaged resources if Dispose was forgotten.
 * Dispose (IDisposable.Dispose): deterministic; caller chooses when to
 *   release files, sockets, handles; often paired with using.
 *
 * Best practice: implement Dispose for resources; suppress finalization
 * with GC.SuppressFinalize(this) when Dispose runs successfully.
 */

/*
 * SECTION 6: METHOD OVERLOADING RULES
 *
 * Overloading = same method name, different signatures in the same scope.
 *
 * | Rule | Detail |
 * |------|--------|
 * | Parameter count | Price(a) vs Price(a,b) — valid |
 * | Parameter types | Price(int) vs Price(decimal) — valid |
 * | Parameter order | Format(a,b) vs Format(b,a) — valid |
 * | Return type alone | int Get() vs string Get() — INVALID (CS0111) |
 * | ref/out/in | ref int vs int — different signatures |
 *
 * The compiler resolves the call at compile time from argument types.
 * Ambiguous calls where two overloads match equally well → CS0121.
 *
 * --- 6a. params arrays (Section 7) ---
 *
 * --- 6b. Optional parameters (Section 8) ---
 *
 * --- 6c. Named arguments (Section 9) ---
 */
public class LineItemCalculator
{
    public decimal Price(int quantity, decimal unitPrice)
    {
        return quantity * unitPrice;
    }

    public decimal Price(int quantity, decimal unitPrice, decimal discountRate)
    {
        decimal gross = quantity * unitPrice;
        return gross - (gross * discountRate);
    }

    public decimal ApplyTax(decimal amount, decimal rate = 0.08m)
    {
        return amount + (amount * rate);
    }

    public string FormatAmount(decimal amount)
    {
        return amount.ToString("F2", CultureInfo.InvariantCulture);
    }

    public string FormatAmount(decimal amount, string currencyCode)
    {
        return $"{currencyCode} {amount.ToString("F2", CultureInfo.InvariantCulture)}";
    }

    /*
     * SECTION 7: params ARRAYS — VARIABLE ARGUMENT LISTS
     *
     * params lets the caller pass a variable number of arguments of one type.
     * The compiler wraps them in an array at the call site.
     *
     * Rules:
     *   - Only one params parameter per method
     *   - params parameter must be the last in the signature
     *   - Type must be a single-dimensional array (params int[] ok)
     *
     * Call shapes:
     *   TotalWeight(1.2m, 0.5m, 2.0m)     // three args
     *   TotalWeight()                      // zero args → empty array
     *   TotalWeight(new decimal[] { 3m })  // explicit array also valid
     *
     * Compile note: params not last → CS0231.
     */
    public decimal TotalWeight(params decimal[] itemWeights)
    {
        decimal sum = 0m;

        for (int i = 0; i < itemWeights.Length; i++)
        {
            sum += itemWeights[i];
        }

        return sum;
    }

    /*
     * SECTION 8: OPTIONAL PARAMETERS — DEFAULT VALUES
     *
     * Default values are baked in at the call site at compile time:
     *
     *   public string BuildLabel(string name, int qty = 1, string unit = "ea")
     *
     * Caller may omit trailing parameters:
     *   BuildLabel("Pen")              // qty=1, unit="ea"
     *   BuildLabel("Pen", 12)          // unit="ea"
     *
     * Changing a default value requires recompiling all callers that rely
     * on the default — unlike overloads, which stay explicit in source.
     *
     * Optional parameters cannot precede required ones.
     * Do not use optional params for values that change at runtime silently.
     */
    public string BuildLabel(string productName, int quantity = 1, string unit = "ea")
    {
        return $"{productName} x{quantity} {unit}";
    }

    /*
     * SECTION 9: NAMED ARGUMENTS
     *
     * Callers supply arguments by parameter name:
     *
     *   ApplyTax(amount, rate: 0.05m)
     *   BuildLabel("Notebook", unit: "box", quantity: 6)
     *
     * Positional arguments must come before named arguments.
     * Named arguments let you skip optional parameters in the middle without
     * passing every positional value — useful with several optional params.
     *
     * Named + optional together are common; they reduce overload count but
     * mixing carelessly with overloads can create ambiguous calls (CS0121).
     */
    public decimal ShippingFee(decimal orderTotal, decimal rate = 0.05m, decimal minimum = 2.99m)
    {
        decimal calculated = orderTotal * rate;
        return calculated < minimum ? minimum : calculated;
    }
}

public class Program
{
    /*
     * SECTION 10: WAREHOUSE INITIALIZATION DEMONSTRATION
     *
     * Main wires the chapter demo — create objects, exercise constructors
     * and overloaded helpers, then print results. Concept explanations live
     * above the types they describe; this method orchestrates the runnable example.
     */
    public static void Main(string[] args)
    {
        int argCount = args.Length;

        int skuSeedFromStaticCtor = Product.NextSku;

        Product placeholder = new Product();
        string placeholderSummary = placeholder.Describe();

        Product pen = new Product("Ballpoint Pen", 2.49m);
        string penSummary = pen.Describe();

        Product notebook = new Product("Spiral Notebook");
        string notebookSummary = notebook.Describe();

        Product penDuplicate = new Product(pen);
        string duplicateSummary = penDuplicate.Describe();

        Product stapler = new Product("Stapler", 8.99m);
        int staplerSku = stapler.Sku;
        int nextSkuAfterStapler = Product.NextSku;

        InventoryRegistry registry = InventoryRegistry.Instance;
        registry.Register(pen);
        registry.Register(stapler);
        int registeredCount = registry.RegisteredCount;
        string registryStartup = InventoryRegistry.StartupLabel;
        string lastRegistered = registry.LastRegisteredName;

        TempFileHandle.FinalizerRunCount = 0;
        CreateAndReleaseTempHandle();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        int finalizerRuns = TempFileHandle.FinalizerRunCount;

        string disposePreviewNote =
            "Preview: call Dispose (or using) for timely cleanup; finalizer is a last-resort safety net.";

        string baseChainPreviewNote =
            "Preview: derived ctors use : base(...) to forward to parent — see ch.05 Inheritance.";

        LineItemCalculator calculator = new LineItemCalculator();
        decimal lineBase = calculator.Price(3, 2.49m);
        decimal lineDiscounted = calculator.Price(3, 2.49m, 0.10m);
        decimal lineWithTax = calculator.ApplyTax(lineBase);
        decimal lineWithCustomTax = calculator.ApplyTax(lineBase, rate: 0.05m);
        string currencyLabel = calculator.FormatAmount(lineBase, "USD");
        string plainLabel = calculator.FormatAmount(lineBase);

        decimal packWeight = calculator.TotalWeight(1.2m, 0.5m, 2.0m);
        decimal emptyPackWeight = calculator.TotalWeight();

        string defaultLabel = calculator.BuildLabel("Ballpoint Pen");
        string bulkLabel = calculator.BuildLabel("Ballpoint Pen", 12);
        string namedLabel = calculator.BuildLabel("Spiral Notebook", unit: "box", quantity: 6);

        decimal defaultShipping = calculator.ShippingFee(40.00m);
        decimal namedShipping = calculator.ShippingFee(40.00m, minimum: 4.99m);

        Console.WriteLine("=== Constructors and Method Overloading — Warehouse Demo ===");
        Console.WriteLine($"CLI args received: {argCount}");
        Console.WriteLine();
        Console.WriteLine("--- Constructors ---");
        Console.WriteLine($"Default ctor:        {placeholderSummary}");
        Console.WriteLine($"Parameterized ctor:  {penSummary}");
        Console.WriteLine($"Chained ctor:        {notebookSummary}");
        Console.WriteLine($"Copy pattern:        {duplicateSummary}");
        Console.WriteLine($"Static ctor seed:    NextSku={skuSeedFromStaticCtor}, stapler SKU={staplerSku}, next={nextSkuAfterStapler}");
        Console.WriteLine($"Singleton registry:  {registryStartup}");
        Console.WriteLine($"  Registered:        {registeredCount} (last: {lastRegistered})");
        Console.WriteLine($"Finalizer runs:      {finalizerRuns} (after forced GC)");
        Console.WriteLine();
        Console.WriteLine("--- : base(...) chaining (preview) ---");
        Console.WriteLine(baseChainPreviewNote);
        Console.WriteLine();
        Console.WriteLine("--- Finalize vs Dispose (preview) ---");
        Console.WriteLine(disposePreviewNote);
        Console.WriteLine();
        Console.WriteLine("--- Method overloading ---");
        Console.WriteLine($"Price(3, 2.49):            {lineBase.ToString("F2", CultureInfo.InvariantCulture)}");
        Console.WriteLine($"Price(3, 2.49, 0.10 disc): {lineDiscounted.ToString("F2", CultureInfo.InvariantCulture)}");
        Console.WriteLine($"ApplyTax (default 8%):     {lineWithTax.ToString("F2", CultureInfo.InvariantCulture)}");
        Console.WriteLine($"ApplyTax (rate: 0.05):     {lineWithCustomTax.ToString("F2", CultureInfo.InvariantCulture)}");
        Console.WriteLine($"FormatAmount + currency:   {currencyLabel}");
        Console.WriteLine($"FormatAmount plain:        {plainLabel}");
        Console.WriteLine();
        Console.WriteLine("--- params ---");
        Console.WriteLine($"TotalWeight(1.2, 0.5, 2.0): {packWeight.ToString("F2", CultureInfo.InvariantCulture)} kg");
        Console.WriteLine($"TotalWeight():              {emptyPackWeight.ToString("F2", CultureInfo.InvariantCulture)} kg");
        Console.WriteLine();
        Console.WriteLine("--- Optional parameters ---");
        Console.WriteLine($"BuildLabel default:  {defaultLabel}");
        Console.WriteLine($"BuildLabel qty=12:   {bulkLabel}");
        Console.WriteLine($"ShippingFee default: {defaultShipping.ToString("F2", CultureInfo.InvariantCulture)}");
        Console.WriteLine();
        Console.WriteLine("--- Named arguments ---");
        Console.WriteLine($"BuildLabel named:    {namedLabel}");
        Console.WriteLine($"ShippingFee named:   {namedShipping.ToString("F2", CultureInfo.InvariantCulture)}");
    }

    private static void CreateAndReleaseTempHandle()
    {
        TempFileHandle handle = new TempFileHandle("demo-temp.log");
        Console.WriteLine(handle.OpenMessage);
    }
}

/*
 * QUICK REFERENCE — CONSTRUCTORS AND METHOD OVERLOADING
 *
 * --- Constructor forms ---
 *
 *  public Class() { }                         // default — add manually if others exist
 *  public Class(int x) { }                    // parameterized
 *  public Class() : this(0) { }               // chains to another ctor in same class
 *  public Class(Other o) { ... }              // copy pattern — manual field copy
 *  static Class() { }                         // runs once per type, before first use
 *  private Class() { }                        // blocks external new — singleton pattern
 *  ~Class() { }                               // finalizer — non-deterministic cleanup
 *  public Derived(...) : base(...) { }        // preview — see ch.05 Inheritance
 *
 * --- Why constructors ---
 *
 *  Enforce invariants at creation; avoid null/zero/default-only objects in production.
 *
 * --- Static vs instance ---
 *
 *  static ctor     → once per type (SKU seed, static caches)
 *  instance ctor   → once per new object (field initialization)
 *
 * --- Method overloading ---
 *
 *  Same name + different parameter list (count, types, or order).
 *  Return type alone does NOT create an overload (CS0111).
 *  Compiler picks best match at compile time; ambiguous call → CS0121.
 *
 * --- params ---
 *
 *  public decimal Sum(params decimal[] values) { ... }
 *  Sum(1m, 2m, 3m);                    // compiler builds array
 *  One params param only; must be last (CS0231 if not).
 *
 * --- Optional parameters ---
 *
 *  void Ship(string dest, decimal fee = 5.99m) { ... }
 *  Defaults embedded at compile time; recompile callers when defaults change.
 *
 * --- Named arguments ---
 *
 *  Ship("Boston", fee: 3.50m);
 *  Positional args first, then named; skip middle optionals by name.
 *
 * --- Preview pointers ---
 *
 *  : base(...) chaining  → 05. Inheritance and Polymorphism
 *  Finalize vs Dispose   → Memory Management + IDisposable (using, SuppressFinalize)
 *  Singleton depth       → 04. Static Members and Static Classes
 *
 * --- Common errors ---
 *
 *  Mistake                               | Result
 *  --------------------------------------|----------------------------------
 *  No parameterless ctor after adding one| CS7036 if code still calls new T()
 *  Duplicate signatures                  | CS0111 type already defines member
 *  Ambiguous overload call               | CS0121 no best match
 *  : this(...) and : base(...) together  | CS2506 only one ctor initializer
 *  params not last                       | CS0231
 */
