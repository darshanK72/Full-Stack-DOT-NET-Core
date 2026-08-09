/*
 * =============================================================================
 * 02. REFLECTION & ATTRIBUTES — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: Reflection — inspect and invoke types, members, and assemblies at
 *        runtime — plus custom attributes that attach declarative metadata
 *        the compiler embeds into IL and assembly manifests.
 *
 * WHY IT MATTERS:
 *   Frameworks rarely know your types at compile time. ASP.NET Core discovers
 *   controllers, EF Core maps entities to tables, serializers honor [JsonIgnore],
 *   and test runners find [Fact] methods — all through reflection and attributes.
 *   When you write a generic exporter, plugin loader, or validation pipeline,
 *   you read Type / PropertyInfo / MethodInfo and the attributes on them instead
 *   of hard-coding every class.
 *
 * WHAT YOU WILL LEARN:
 *   1.  AttributeTargets and AttributeUsage — where attributes may be applied
 *   2.  Custom attributes — define, apply, constructor vs named properties
 *   3.  Type — typeof, GetType, Type.GetType, and metadata members
 *   4.  Assembly — executing assembly, listing types, assembly-qualified names
 *   5.  PropertyInfo — GetProperties, GetValue, SetValue, BindingFlags
 *   6.  MethodInfo — GetMethod, Invoke, TargetInvocationException
 *   7.  FieldInfo and ConstructorInfo — non-public members and ctor invocation
 *   8.  Activator.CreateInstance — construct objects when only Type is known
 *   9.  Reading attributes — GetCustomAttribute, IsDefined, inheritance
 *  10.  Metadata-driven export — scan types once, honor attribute metadata
 *  11.  Reflection pitfalls — performance, null members, AOT trimming preview
 *  12.  dynamic vs reflection — preview (full comparison in ch.04)
 *
 * =============================================================================
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ReflectionAndAttributes;

/*
 * =========================================================================
 * SECTION 1: AttributeTargets AND CUSTOM ATTRIBUTE DEFINITION
 * =========================================================================
 *
 * Attributes are classes inheriting System.Attribute. Apply them in square
 * brackets on assemblies, modules, types, members, parameters, or return
 * values. The compiler stores metadata; reflection reads it at runtime.
 *
 * AttributeUsage controls valid application sites:
 *
 *   Flag              | Typical use
 *   ------------------|--------------------------------------------------
 *   Assembly          | Version info, COM visibility, license metadata
 *   Class / Struct    | ORM table mapping, serializer type hints
 *   Property / Field  | Column names, validation, export flags
 *   Method            | HTTP verbs, test markers, authorization
 *   Parameter         | Model-binding hints, optional route segments
 *   ReturnValue       | Output formatting (rare; needs special syntax)
 *
 * Other AttributeUsage knobs:
 *
 *   AllowMultiple     | false (default) — one instance per target
 *                     | true  — stack multiple attributes (e.g. [Route] chains)
 *   Inherited         | true  — subclass inherits attribute on base member
 *                     | false — attribute stays on declaring type only
 *
 * Definition pattern:
 *
 *   [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
 *   public sealed class DisplayLabelAttribute : Attribute
 *   {
 *       public DisplayLabelAttribute(string label) => Label = label; // positional ctor arg
 *       public string Label { get; }                                  // read-only metadata
 *   }
 *
 * At use site the Attribute suffix is optional: [DisplayLabel("SKU Code")]
 *
 * Rules:
 *   • Attribute class must be public (or internal in same assembly) — CS0592 if not
 *   • Only properties with public getters are valid named metadata
 *   • Locals and using aliases cannot carry attributes — CS0592
 * -------------------------------------------------------------------------
 */

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class DisplayLabelAttribute : Attribute
{
    public DisplayLabelAttribute(string label) => Label = label; // positional metadata
    public string Label { get; }                                   // exposed to reflection readers
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class ExportableAttribute : Attribute
{
    public ExportableAttribute(bool include = true) => Include = include; // default true
    public bool Include { get; }                                           // false = explicit skip
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class EntityTableAttribute : Attribute
{
    public EntityTableAttribute(string tableName) => TableName = tableName;
    public string TableName { get; }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class AuditActionAttribute : Attribute
{
    public AuditActionAttribute(string action) => Action = action;
    public string Action { get; }
}

/*
 * =========================================================================
 * SECTION 2: DECORATED DOMAIN TYPE — Product
 * =========================================================================
 *
 * Class-level [EntityTable] maps the type to a persistence table name.
 * Property-level [DisplayLabel] and [Exportable] drive report headers and
 * export columns. Method-level [AuditAction] supports multiple entries
 * because AllowMultiple = true on that attribute class.
 *
 * Product is the primary specimen for Type, PropertyInfo, MethodInfo,
 * and attribute-reading sections in Main.
 * -------------------------------------------------------------------------
 */
[EntityTable("Products")]
public sealed class Product
{
    [DisplayLabel("SKU Code")]
    [Exportable]
    public string Sku { get; set; } = string.Empty;

    [DisplayLabel("Product Name")]
    [Exportable]
    public string Name { get; set; } = string.Empty;

    [DisplayLabel("Unit Price")]
    [Exportable]
    public decimal Price { get; set; }

    [DisplayLabel("Stock Level")]
    public int Stock { get; set; } // no [Exportable] — omitted from export manifest

    [AuditAction("Calculate")]
    [AuditAction("Pricing")]
    public decimal CalculateLineTotal(int quantity) => Price * quantity; // audited pricing call

    public override string ToString() => $"{Sku}: {Name} @ {Price:C} (stock {Stock})";
}

/*
 * =========================================================================
 * SECTION 3: NON-PUBLIC MEMBERS — CostRecord (BindingFlags specimen)
 * =========================================================================
 *
 * GetProperties() and GetMethod() return public members by default.
 * BindingFlags combines visibility and static/instance scope:
 *
 *   BindingFlags.Public | BindingFlags.NonPublic
 *                       | BindingFlags.Instance | BindingFlags.Static
 *
 * Omitting a flag excludes that category — a common pitfall when hunting
 * private backing fields or internal helpers.
 * -------------------------------------------------------------------------
 */
public sealed class CostRecord
{
    private readonly decimal _costBasis; // private field — needs BindingFlags.NonPublic

    public CostRecord(decimal costBasis) => _costBasis = costBasis; // sole ctor

    public string Sku { get; set; } = string.Empty;

    public decimal RetailPrice { get; set; }

    private decimal CalculateMargin() => RetailPrice - _costBasis; // private method

    public decimal GetMarginPercent()
    {
        decimal margin = CalculateMargin(); // normal call from inside the type
        return _costBasis == 0m ? 0m : margin / _costBasis * 100m;
    }
}

/*
 * =========================================================================
 * SECTION 4: PARAMETERIZED CONSTRUCTOR — StockReceipt (Activator specimen)
 * =========================================================================
 *
 * Activator.CreateInstance(Type) requires a parameterless constructor.
 * When only a parameterized ctor exists, pass arguments:
 *
 *   Activator.CreateInstance(type, arg0, arg1, …)
 *
 * ConstructorInfo.Invoke(null, args) works for static ctors; pass the instance
 * as the first argument for instance constructors (see Section 7 in Main).
 * -------------------------------------------------------------------------
 */
public sealed class StockReceipt
{
    public StockReceipt(string sku, int quantity) // no parameterless ctor
    {
        Sku = sku;
        Quantity = quantity;
        ReceivedAt = DateTime.UtcNow;
    }

    public string Sku { get; }
    public int Quantity { get; }
    public DateTime ReceivedAt { get; }

    public override string ToString() => $"{Sku} × {Quantity} @ {ReceivedAt:u}";
}

/*
 * =========================================================================
 * SECTION 5: METADATA-DRIVEN EXPORT MANIFEST
 * =========================================================================
 *
 * Framework pattern: walk PropertyInfo once, honor [Exportable] and
 * [DisplayLabel], read values with GetValue — no hard-coded property list.
 * Mirrors CSV exporters, OpenAPI schema generators, and validation pipelines
 * that scan types at startup.
 * -------------------------------------------------------------------------
 */
public static class ExportManifestBuilder
{
    public static List<(string Header, string Value)> Build(object instance, Type type)
    {
        List<(string Header, string Value)> rows = new List<(string, string)>();

        foreach (PropertyInfo property in type.GetProperties())
        {
            ExportableAttribute? export = property.GetCustomAttribute<ExportableAttribute>();
            if (export is null || !export.Include)
            {
                continue; // skip unmarked or explicitly excluded properties
            }

            DisplayLabelAttribute? label = property.GetCustomAttribute<DisplayLabelAttribute>();
            string header = label?.Label ?? property.Name;           // fallback to property name
            object? raw = property.GetValue(instance);               // boxed for value types
            string value = raw?.ToString() ?? string.Empty;

            rows.Add((header, value));
        }

        return rows;
    }
}

public class Program
{
    /*
     * =========================================================================
     * SECTION 6: DEMONSTRATION — Main orchestrates the chapter demo
     * =========================================================================
     */
    public static void Main(string[] args)
    {
        Product sample = new Product
        {
            Sku = "KB-101",
            Name = "Mechanical Keyboard",
            Price = 89.99m,
            Stock = 42,
        };

        Console.WriteLine("=== Warehouse catalog sample ===");
        Console.WriteLine(sample);


        /*
         * --- 6a. Type — runtime type metadata ---
         *
         * Three ways to obtain System.Type:
         *
         *   typeof(Product)           compile-time; preferred when type is known
         *   sample.GetType()          exact runtime type of the instance
         *   Type.GetType("FullName")  load by string — same assembly only unless
         *                             assembly-qualified name is supplied
         *
         * Useful Type members:
         *   .Name .FullName .Namespace .IsClass .IsValueType .BaseType .Assembly
         *   .GetProperties() .GetMethods() .GetConstructors() .GetFields()
         * -------------------------------------------------------------------------
         */
        Type productType = typeof(Product); // compile-time Type token

        Console.WriteLine();
        Console.WriteLine("--- Type metadata ---");
        Console.WriteLine($"  Name:      {productType.Name}");
        Console.WriteLine($"  FullName:  {productType.FullName}");
        Console.WriteLine($"  Namespace: {productType.Namespace}");
        Console.WriteLine($"  IsClass:   {productType.IsClass}");
        Console.WriteLine($"  BaseType:  {productType.BaseType?.Name ?? "(none)"}");
        Console.WriteLine($"  Assembly:  {productType.Assembly.GetName().Name}");

        Type runtimeType = sample.GetType(); // always Product here; polymorphism would differ
        Console.WriteLine($"  GetType(): {runtimeType.Name} (same as typeof when not subclassed)");


        /*
         * --- 6b. Assembly — container of types and metadata ---
         *
         * Assembly.GetExecutingAssembly() — the running tutorial assembly.
         * assembly.GetName() → AssemblyName (Name, Version, Culture)
         * assembly.GetTypes() → every public type; can throw ReflectionTypeLoadException
         *                       if a dependency fails to load (handle .LoaderExceptions)
         *
         * Cross-assembly Type.GetType requires assembly-qualified name:
         *   "MyApp.Models.Product, MyApp, Version=…, Culture=neutral, PublicKeyToken=…"
         * Without the assembly part, GetType often returns null for external types.
         * -------------------------------------------------------------------------
         */
        Assembly executing = Assembly.GetExecutingAssembly();
        AssemblyName assemblyName = executing.GetName();

        Console.WriteLine();
        Console.WriteLine("--- Assembly metadata ---");
        Console.WriteLine($"  Location:  {executing.Location}");
        Console.WriteLine($"  Full name: {assemblyName.FullName}");

        Type[] publicTypes = executing.GetTypes()
            .Where(t => t.Namespace == typeof(Program).Namespace && t.IsPublic)
            .OrderBy(t => t.Name)
            .ToArray();

        Console.WriteLine($"  Public types in {typeof(Program).Namespace}:");
        foreach (Type t in publicTypes)
        {
            Console.WriteLine($"    {t.Name}");
        }


        /*
         * --- 6c. PropertyInfo — read and write property values ---
         *
         * type.GetProperties() — public instance properties (default binding).
         * PropertyInfo:
         *   .Name .PropertyType .CanRead .CanWrite
         *   .GetValue(instance)  — null instance for static properties
         *   .SetValue(instance, value) — value must be assignable (boxing OK)
         *
         * GetValue returns object? — unbox value types explicitly.
         * -------------------------------------------------------------------------
         */
        PropertyInfo[] properties = productType.GetProperties();

        Console.WriteLine();
        Console.WriteLine("--- PropertyInfo: public instance members ---");
        foreach (PropertyInfo property in properties)
        {
            object? value = property.GetValue(sample); // read current value
            Console.WriteLine($"  {property.Name,-10} ({property.PropertyType.Name}) = {value}");
        }

        PropertyInfo? priceProperty = productType.GetProperty(nameof(Product.Price));
        if (priceProperty is not null && priceProperty.CanWrite)
        {
            priceProperty.SetValue(sample, 79.99m); // write without compile-time reference
            Console.WriteLine();
            Console.WriteLine($"  After SetValue on Price: {sample}");
        }


        /*
         * --- 6d. BindingFlags — non-public fields and methods ---
         *
         * Default GetField/GetMethod omit private members. Combine flags explicitly.
         * -------------------------------------------------------------------------
         */
        CostRecord cost = new CostRecord(45.00m) { Sku = "KB-101", RetailPrice = 89.99m };
        Type costType = typeof(CostRecord);
        const BindingFlags instanceAny = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        FieldInfo? costBasisField = costType.GetField("_costBasis", instanceAny);
        MethodInfo? marginMethod = costType.GetMethod("CalculateMargin", instanceAny);

        Console.WriteLine();
        Console.WriteLine("--- BindingFlags: private field and method ---");
        if (costBasisField is not null)
        {
            object? basis = costBasisField.GetValue(cost); // read private _costBasis
            Console.WriteLine($"  _costBasis = {basis}");
        }

        if (marginMethod is not null)
        {
            object? margin = marginMethod.Invoke(cost, null); // call private CalculateMargin
            Console.WriteLine($"  CalculateMargin() → {margin}");
        }

        Console.WriteLine($"  Public GetMarginPercent(): {cost.GetMarginPercent():F1}%");


        /*
         * --- 6e. MethodInfo — Invoke without compile-time binding ---
         *
         * GetMethod(name, parameterTypes) locates an overload by signature.
         * Invoke(instance, args) calls it — instance null for static methods.
         *
         * Exceptions thrown INSIDE the invoked method wrap in TargetInvocationException.
         * Inspect .InnerException for the real fault (same pattern as BeginInvoke).
         *
         * Wrong name → null MethodInfo → NullReferenceException if not guarded.
         * Wrong arg count/types → ArgumentException at Invoke time.
         * -------------------------------------------------------------------------
         */
        MethodInfo? lineTotalMethod = productType.GetMethod(
            nameof(Product.CalculateLineTotal),
            new[] { typeof(int) });

        Console.WriteLine();
        Console.WriteLine("--- MethodInfo.Invoke ---");
        if (lineTotalMethod is not null)
        {
            object? invokeResult = lineTotalMethod.Invoke(sample, new object[] { 3 });
            decimal lineTotal = invokeResult is decimal amount ? amount : 0m; // unbox
            Console.WriteLine($"  {lineTotalMethod.Name}(3) → {lineTotal:C}");
        }

        MethodInfo? missingMethod = productType.GetMethod("TypoMethodName"); // pitfall: null
        Console.WriteLine($"  GetMethod typo returns: {(missingMethod is null ? "null (always check)" : "found")}");


        /*
         * --- 6f. ConstructorInfo and Activator.CreateInstance ---
         *
         * type.GetConstructors() — public instance ctors by default.
         * ctor.Invoke(new object[] { args }) — same wrapping rules as MethodInfo.
         *
         * Activator shortcuts:
         *   CreateInstance(type)              parameterless ctor required
         *   CreateInstance(type, args)        forwards to matching ctor
         *   CreateInstance<T>()               generic convenience (parameterless)
         * -------------------------------------------------------------------------
         */
        string typeName = typeof(Product).FullName!;
        Type? loadedType = Type.GetType(typeName); // works — same assembly, namespace-qualified

        Console.WriteLine();
        Console.WriteLine("--- Activator.CreateInstance (parameterless) ---");
        if (loadedType is not null)
        {
            object? created = Activator.CreateInstance(loadedType);
            if (created is Product createdProduct)
            {
                createdProduct.Sku = "MS-220";
                createdProduct.Name = "Wireless Mouse";
                createdProduct.Price = 34.50m;
                createdProduct.Stock = 120;
                Console.WriteLine($"  New {loadedType.Name}: {createdProduct}");
            }
        }

        Type receiptType = typeof(StockReceipt);
        ConstructorInfo[] ctors = receiptType.GetConstructors();
        Console.WriteLine();
        Console.WriteLine("--- ConstructorInfo.Invoke (parameterized) ---");
        Console.WriteLine($"  {receiptType.Name} has {ctors.Length} public ctor(s)");
        foreach (ConstructorInfo ctor in ctors)
        {
            ParameterInfo[] parameters = ctor.GetParameters();
            string signature = string.Join(", ", parameters.Select(p => p.ParameterType.Name));
            Console.WriteLine($"    .ctor({signature})");
        }

        object? receiptObject = Activator.CreateInstance(receiptType, "KB-101", 50);
        if (receiptObject is StockReceipt receipt)
        {
            Console.WriteLine($"  Activator with args: {receipt}");
        }


        /*
         * --- 6g. Reading custom attributes at runtime ---
         *
         * GetCustomAttribute<T>()     first T or null (respects AttributeUsage.Inherited)
         * GetCustomAttributes<T>()    all T when AllowMultiple = true
         * IsDefined(typeof(T))        presence check without allocating attribute instances
         * GetCustomAttributes(inherit) non-generic IEnumerable<Attribute>
         *
         * Class-level EntityTable has Inherited = false — subclasses do not inherit it.
         * Property DisplayLabel has Inherited = true — visible on overrides in subclasses.
         * -------------------------------------------------------------------------
         */
        EntityTableAttribute? tableAttr = productType.GetCustomAttribute<EntityTableAttribute>();

        Console.WriteLine();
        Console.WriteLine("--- Custom attributes on Product ---");
        Console.WriteLine($"  [EntityTable] → table: {tableAttr?.TableName ?? "(none)"}");
        Console.WriteLine($"  IsDefined(EntityTable): {productType.IsDefined(typeof(EntityTableAttribute))}");

        Console.WriteLine();
        Console.WriteLine("  Property attributes:");
        foreach (PropertyInfo property in properties)
        {
            DisplayLabelAttribute? label = property.GetCustomAttribute<DisplayLabelAttribute>();
            ExportableAttribute? export = property.GetCustomAttribute<ExportableAttribute>();

            string labelText = label?.Label ?? property.Name;
            string exportText = export is null ? "not marked" : export.Include ? "export" : "skip";

            Console.WriteLine($"    {property.Name,-10} label=\"{labelText}\"  export={exportText}");
        }

        MethodInfo? auditMethod = productType.GetMethod(nameof(Product.CalculateLineTotal), new[] { typeof(int) });
        if (auditMethod is not null)
        {
            AuditActionAttribute[] audits = auditMethod.GetCustomAttributes<AuditActionAttribute>().ToArray();
            Console.WriteLine();
            Console.WriteLine($"  [AuditAction] on {auditMethod.Name} (AllowMultiple):");
            foreach (AuditActionAttribute audit in audits)
            {
                Console.WriteLine($"    action=\"{audit.Action}\"");
            }
        }


        /*
         * --- 6h. Metadata-driven export manifest ---
         * -------------------------------------------------------------------------
         */
        List<(string Header, string Value)> exportRows = ExportManifestBuilder.Build(sample, productType);

        Console.WriteLine();
        Console.WriteLine("--- Metadata-driven export manifest ---");
        foreach ((string header, string value) in exportRows)
        {
            Console.WriteLine($"  {header,-16} {value}");
        }


        /*
         * --- 6i. Reflection pitfalls (summary) ---
         *
         *  Pitfall                          | Consequence / mitigation
         *  ---------------------------------|----------------------------------
         *  Stringly-typed member names       | Rename refactor breaks silently — tests + nameof
         *  Null MethodInfo/PropertyInfo      | Guard before Invoke/GetValue/SetValue
         *  TargetInvocationException         | Read .InnerException for real fault
         *  GetType without assembly name     | null Type for types in other assemblies
         *  Uncached reflection in hot paths  | Cache Type/MemberInfo in static readonly fields
         *  GetValue boxing                   | Extra allocations for value-type properties
         *  Default binding misses private    | Pass explicit BindingFlags
         *  AOT / trimming (.NET 8+)          | Reflection may strip unused members — see note
         *
         * AOT note: Native AOT and aggressive trimming require [DynamicallyAccessedMembers]
         * or source generators instead of unrestricted reflection. Framework code in ASP.NET
         * and EF handles this — custom plugin loaders must audit trim-safe patterns separately.
         * -------------------------------------------------------------------------
         */
        Console.WriteLine();
        Console.WriteLine("--- Reflection pitfalls (see section comment in Main) ---");
        Console.WriteLine("  Cache MemberInfo, guard null lookups, unwrap TargetInvocationException.");
        Console.WriteLine("  Cross-assembly Type.GetType needs assembly-qualified names.");


        /*
         * --- 6j. dynamic vs reflection — PREVIEW ---
         *
         * Both defer member binding to runtime:
         *
         *   Reflection    explicit metadata API (Type, MethodInfo, Invoke)
         *   dynamic (DLR) natural syntax; compiler emits call sites
         *
         * Reflection alone exposes attributes and full type graphs; dynamic only calls members.
         *
         * COVERED IN DETAIL LATER → 04. Var Dynamic & Special Keywords
         * -------------------------------------------------------------------------
         */
        Console.WriteLine();
        Console.WriteLine("--- Dynamic vs reflection (preview) ---");

        decimal reflectionTotal = lineTotalMethod is not null
            ? (decimal)(lineTotalMethod.Invoke(sample, new object[] { 2 }) ?? 0m)
            : 0m;

        dynamic dynamicProduct = sample; // DLR binding — no MethodInfo required
        decimal dynamicTotal = dynamicProduct.CalculateLineTotal(2);

        Console.WriteLine($"  Reflection Invoke(2): {reflectionTotal:C}");
        Console.WriteLine($"  dynamic call (2):     {dynamicTotal:C}");
        Console.WriteLine("  → Same result; reflection exposes metadata, dynamic simplifies calls.");
    }
}

/*
 * =========================================================================
 * QUICK REFERENCE — REFLECTION & ATTRIBUTES
 * =========================================================================
 *
 * --- Attribute definition ---
 *
 *   [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
 *   public sealed class MyAttr : Attribute
 *   {
 *       public MyAttr(string message) => Message = message;
 *       public string Message { get; }
 *   }
 *
 * --- AttributeTargets (common) ---
 *
 *   Assembly | Module | Class | Struct | Enum | Interface | Delegate
 *   Property | Field  | Method | Constructor | Event | Parameter | ReturnValue
 *
 * --- Obtaining Type ---
 *
 *   typeof(Product)              compile-time
 *   instance.GetType()           exact runtime type
 *   Type.GetType("Ns.TypeName")  same assembly; else use assembly-qualified name
 *
 * --- Assembly ---
 *
 *   Assembly asm = Assembly.GetExecutingAssembly();
 *   AssemblyName name = asm.GetName();
 *   Type[] types = asm.GetTypes();
 *
 * --- Properties ---
 *
 *   PropertyInfo[] props = type.GetProperties();
 *   PropertyInfo? p = type.GetProperty("Price");
 *   object? val = p?.GetValue(instance);
 *   p?.SetValue(instance, 99.99m);
 *
 * --- Methods ---
 *
 *   MethodInfo? m = type.GetMethod("Name", new[] { typeof(int) });
 *   object? result = m?.Invoke(instance, new object[] { 3 });
 *
 * --- Fields (non-public) ---
 *
 *   FieldInfo? f = type.GetField("_name",
 *       BindingFlags.Instance | BindingFlags.NonPublic);
 *   object? val = f?.GetValue(instance);
 *
 * --- Constructors ---
 *
 *   ConstructorInfo[] ctors = type.GetConstructors();
 *   object? obj = ctors[0].Invoke(new object[] { arg0, arg1 });
 *
 * --- Create instance ---
 *
 *   object? obj = Activator.CreateInstance(type);
 *   object? obj2 = Activator.CreateInstance(type, arg0, arg1);
 *   Product p = (Product)obj!;   // or pattern match
 *
 * --- Reading attributes ---
 *
 *   T? attr = member.GetCustomAttribute<T>();
 *   IEnumerable<T> all = member.GetCustomAttributes<T>();
 *   bool has = member.IsDefined(typeof(T));
 *   IEnumerable<Attribute> legacy = member.GetCustomAttributes(inherit: true);
 *
 * --- BindingFlags ---
 *
 *   type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic
 *                      | BindingFlags.Instance | BindingFlags.Static)
 *
 * --- Common mistakes ---
 *
 *  Mistake                              | Result
 *  ------------------------------------ | ----------------------------------
 *  GetMethod / GetProperty name typo      | null → NullReference if unguarded
 *  Wrong Invoke argument types            | ArgumentException / inner TargetInvocationException
 *  GetType string without assembly name   | null Type for external types
 *  Attribute class not public             | CS0592 — invalid target
 *  Attribute on local variable            | CS0592 — locals cannot have attrs
 *  Hot-path reflection without caching    | Performance degradation
 *
 * --- dynamic vs reflection ---
 *
 *   Reflection   inspect types, read attributes, Invoke any member
 *   dynamic      call members with natural syntax; no metadata API
 *   Full comparison → 04. Var Dynamic & Special Keywords
 *
 * --- Built-in attributes (framework examples) ---
 *
 *   [Obsolete] [Serializable] [Flags]     BCL metadata
 *   [JsonPropertyName] [Required]         ASP.NET / System.Text.Json
 *
 * =========================================================================
 */
