/*
 * =============================================================================
 * 04. DICTIONARY<TKEY, TVALUE> — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: Dictionary<TKey, TValue> — a generic hash-table map from unique keys
 *        to values. Covers creation, initializer syntax, core API, safe lookup,
 *        enumeration, common exceptions, null-key rules, and when to choose
 *        Dictionary over List.
 *
 * WHY IT MATTERS:
 *   Catalogs, caches, configuration maps, and lookup tables appear in almost
 *   every application. A List forces you to scan every item to find one SKU or
 *   user id; a Dictionary finds it in roughly constant time when you have a
 *   stable key — the difference shows up immediately as data grows.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Key-value pairs and Dictionary<TKey, TValue> basics
 *   2.  Creating and initializing a dictionary (collection initializer syntax)
 *   3.  Core API: Add, indexer, Remove, Count, Keys, Values
 *   4.  ContainsKey and TryGetValue (safe lookup)
 *   5.  foreach KeyValuePair<TKey, TValue> enumeration
 *   6.  Indexer pitfalls: KeyNotFoundException, duplicate Add, null keys
 *   7.  List vs Dictionary — when to use which
 *   8.  SortedDictionary preview (ordered keys — full lesson in ch.07)
 *
 * Scenario: warehouse SKU catalog — SKU code maps to product details.
 *
 * =============================================================================
 */

using System;
using System.Collections.Generic;

namespace DictionaryDemo;

/*
 * SECTION 1: KEY-VALUE PAIRS AND Dictionary<TKey, TValue>
 *
 * A DICTIONARY stores entries as KEY → VALUE pairs. Each key appears at most
 * once; the key identifies the slot, the value is what you store.
 *
 *   Dictionary<TKey, TValue>
 *
 *  Concept   | Role
 *  ----------|----------------------------------------------------------
 *  TKey      | Type of the lookup key (string, int, Guid, …)
 *  TValue    | Type of the stored value (any type, including classes)
 *  Key       | Unique identifier — must not duplicate
 *  Value     | Data associated with that key
 *
 * Internally, Dictionary uses a hash table: keys are hashed to buckets so
 * lookup by key is fast on average (O(1)), unlike scanning a List (O(n)).
 *
 * string and int already implement GetHashCode + Equals correctly — common
 * key types need no custom code. Custom key types must override both methods
 * and stay immutable after insert (see HashSet ch.05 for the hash contract).
 */

/*
 * SECTION 2: DEMO TYPE — Product stored as dictionary values
 *
 * Values can be any type. Here each catalog entry maps a SKU string to a
 * Product instance with name and price.
 */
public class Product
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }

    public override string ToString() => $"{Sku} | {Name} | {UnitPrice:C}";
}

public class Program
{
    /*
     * SECTION 3: CREATING AND INITIALIZING
     *
     * --- 3a. Empty dictionary ---
     *
     *   var catalog = new Dictionary<string, Product>();
     *
     * --- 3b. Collection initializer (at creation time) ---
     *
     *   var prices = new Dictionary<string, decimal>
     *   {
     *       ["WH-1001"] = 12.50m,          // indexer-style entry
     *       { "WH-1002", 8.99m }           // alternate Add-style pair syntax
     *   };
     *
     * --- 3c. Capacity hint (optional) ---
     *
     *   new Dictionary<string, Product>(capacity: 100)
     *
     * A capacity hint reduces internal resizing when you know approximate size.
     */
    public static Dictionary<string, Product> CreateInitialCatalog()
    {
        return new Dictionary<string, Product>(capacity: 8) // capacity hint for bulk inserts
        {
            ["WH-1001"] = new Product { Sku = "WH-1001", Name = "Steel bracket", UnitPrice = 12.50m }, // indexer-style pair
            ["WH-1002"] = new Product { Sku = "WH-1002", Name = "Rubber gasket", UnitPrice = 8.99m }
        };
    }

    /*
     * SECTION 4: CORE API — Add, indexer, Remove, Count, Keys, Values
     *
     *  Member              | Purpose
     *  --------------------|---------------------------------------------------
     *  Add(key, value)     | Insert pair; throws ArgumentException if key exists
     *  this[key]           | GET value or SET (add/replace); GET throws if missing
     *  Remove(key)         | Removes entry; returns true if key was found
     *  Count               | Number of key-value pairs
     *  Keys                | ICollection<TKey> — live view of all keys
     *  Values              | ICollection<TValue> — live view of all values
     *  Clear()             | Removes all entries
     *
     * Indexer SET always succeeds (adds new key or replaces value).
     * Indexer GET throws KeyNotFoundException when the key is absent (Section 6).
     */
    public static void DemonstrateCoreApi(Dictionary<string, Product> catalog)
    {
        catalog.Add("WH-1003", new Product
        {
            Sku = "WH-1003",
            Name = "Hex bolt M8",
            UnitPrice = 0.45m
        }); // Add throws if key already exists

        catalog["WH-1002"].UnitPrice = 9.49m; // indexer SET — add or replace value for key

        Console.WriteLine("--- Section 4: Core API ---");
        PrintCatalog(catalog);
        Console.WriteLine($"Count: {catalog.Count}");

        Console.WriteLine($"Keys ({catalog.Keys.Count}): [{string.Join(", ", catalog.Keys)}]");
        Console.WriteLine($"Values — first product name: {new List<Product>(catalog.Values)[0].Name}");

        bool removed = catalog.Remove("WH-1003");
        Console.WriteLine($"Remove WH-1003: {removed} (Count now {catalog.Count})");
        Console.WriteLine();
    }

    /*
     * SECTION 5: ContainsKey AND TryGetValue
     *
     * Never assume a key exists before reading. Two safe patterns:
     *
     * --- 5a. ContainsKey then indexer ---
     *
     *   if (catalog.ContainsKey(sku))
     *       Product p = catalog[sku];   // two hash lookups
     *
     * Works, but performs two lookups. Prefer TryGetValue when you need the value.
     *
     * --- 5b. TryGetValue (preferred) ---
     *
     *   if (catalog.TryGetValue(sku, out Product? product))
     *       // product is assigned; use it
     *
     * One lookup; out parameter receives the value when true.
     * Returns false when key missing — no exception.
     *
     * Use ContainsKey alone when you only need existence (e.g. guard before Add).
     */
    public static void DemonstrateSafeLookup(Dictionary<string, Product> catalog)
    {
        string lookupSku = "WH-1001";
        bool exists = catalog.ContainsKey(lookupSku);

        Console.WriteLine("--- Section 5: ContainsKey / TryGetValue ---");
        Console.WriteLine($"ContainsKey({lookupSku}): {exists}");

        if (catalog.TryGetValue(lookupSku, out Product? found)) // one lookup; no exception if missing
        {
            Console.WriteLine($"TryGetValue: {found}");
        }

        string missingSku = "WH-9999";
        if (!catalog.TryGetValue(missingSku, out Product? _))
        {
            Console.WriteLine($"TryGetValue({missingSku}): not in catalog (no exception thrown).");
        }

        Console.WriteLine();
    }

    /*
     * SECTION 6: FOREACH KeyValuePair<TKey, TValue>
     *
     * Dictionary implements IEnumerable<KeyValuePair<TKey, TValue>>.
     * Each iteration yields one entry with .Key and .Value properties.
     *
     *   foreach (KeyValuePair<string, Product> entry in catalog)
     *   {
     *       string sku = entry.Key;
     *       Product product = entry.Value;
     *   }
     *
     * C# also allows deconstruction in foreach:
     *   foreach (var (sku, product) in catalog) { … }
     *
     * Keys and Values are live views — mutating the dictionary during foreach
     * on Keys/Values throws InvalidOperationException. Copy first if needed.
     */
    public static void PrintCatalog(Dictionary<string, Product> catalog)
    {
        foreach (KeyValuePair<string, Product> entry in catalog)
        {
            Console.WriteLine($"  {entry.Key} → {entry.Value}");
        }
    }

    public static void DemonstrateDeconstruction(Dictionary<string, Product> catalog)
    {
        Console.WriteLine("--- Section 6: foreach KeyValuePair ---");
        foreach (KeyValuePair<string, Product> pair in catalog)
        {
            Console.WriteLine($"  KeyValuePair: {pair.Key} = {pair.Value.Name}");
        }

        Console.WriteLine("  Deconstructed foreach:");
        foreach (KeyValuePair<string, Product> entry in catalog)
        {
            (string sku, Product product) = entry; // explicit deconstruct from KeyValuePair
            Console.WriteLine($"    ({sku}, {product.UnitPrice:C})");
        }

        Console.WriteLine();
    }

    /*
     * SECTION 7: INDEXER PITFALLS — exceptions and null keys
     *
     * --- 7a. KeyNotFoundException on GET ---
     *
     *   Product p = catalog["DOES-NOT-EXIST"];  // throws KeyNotFoundException
     *
     * Fix: TryGetValue, or ContainsKey + indexer (prefer TryGetValue).
     *
     * --- 7b. ArgumentException on Add duplicate ---
     *
     *   catalog.Add("WH-1001", anotherProduct);  // key already present
     *
     * Fix: use indexer SET to replace, guard with ContainsKey, or Remove then Add.
     *
     * --- 7c. null keys (reference-type TKey only) ---
     *
     * Dictionary<string, …> rejects null keys — Add, indexer, ContainsKey, and
     * TryGetValue with null throw ArgumentNullException.
     *
     * Value types as keys (int, Guid) cannot be null unless TKey is int? etc.;
     * null nullable keys follow the same ArgumentNullException rule.
     */
    public static void DemonstratePitfalls(Dictionary<string, Product> catalog)
    {
        Console.WriteLine("--- Section 7: Indexer pitfalls ---");

        try
        {
            Product _ = catalog["WH-0000"];
        }
        catch (KeyNotFoundException ex)
        {
            Console.WriteLine($"Indexer GET missing key: KeyNotFoundException — {ex.Message}");
        }

        try
        {
            catalog.Add("WH-1001", new Product { Sku = "WH-1001", Name = "Duplicate", UnitPrice = 1m });
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Add duplicate key: ArgumentException — {ex.Message.Split('.')[0]}.");
        }

        string? nullKey = null;
        try
        {
            catalog.Add(nullKey!, new Product { Sku = "X", Name = "Null key test", UnitPrice = 0m });
        }
        catch (ArgumentNullException ex)
        {
            Console.WriteLine($"Null key on Add: ArgumentNullException — ParamName: {ex.ParamName}");
        }

        Console.WriteLine();
    }

    /*
     * SECTION 8: LIST VS DICTIONARY — when to use which
     *
     * Both hold collections of items, but they solve different problems.
     *
     *  Aspect              | List<T>                    | Dictionary<TKey,TValue>
     *  --------------------|----------------------------|---------------------------
     *  Organization        | Ordered sequence (index 0…) | Unordered map by unique key
     *  Access by position | list[i] — O(1)             | N/A (no integer index)
     *  Access by key/id    | Scan all items — O(n)      | this[key] — O(1) average
     *  Duplicates          | Allowed                    | Keys unique; values may repeat
     *  Best for            | Ordered lists, "all rows"  | Lookups, caches, indexes,
     *                      | iteration over everything  | "find by id/code/name key"
     *
     * Rule of thumb: if you repeatedly find one item by a stable identifier
     * (SKU, user id, country code), build a Dictionary keyed on that id.
     * Keep a List when order matters and you iterate everything anyway.
     *
     * (Module README: "List vs Dictionary" is grouped here in ch.04.)
     */
    public static void DemonstrateListVsDictionary()
    {
        List<Product> productList = new List<Product>
        {
            new Product { Sku = "WH-1001", Name = "Steel bracket", UnitPrice = 12.50m },
            new Product { Sku = "WH-1002", Name = "Rubber gasket", UnitPrice = 9.49m },
            new Product { Sku = "WH-1004", Name = "Cable tie 200mm", UnitPrice = 2.10m }
        };

        Dictionary<string, Product> productBySku = new Dictionary<string, Product>();
        foreach (Product item in productList)
        {
            productBySku[item.Sku] = item; // indexer SET builds SKU → Product map
        }

        string targetSku = "WH-1004";

        int listSteps = 0;
        Product? fromList = null;
        foreach (Product p in productList)
        {
            listSteps++;
            if (p.Sku == targetSku)
            {
                fromList = p;
                break;
            }
        }

        productBySku.TryGetValue(targetSku, out Product? fromDict);

        Console.WriteLine("--- Section 8: List vs Dictionary ---");
        Console.WriteLine($"Find {targetSku}:");
        Console.WriteLine($"  List scan: ~{listSteps} comparison(s) → {fromList?.Name}");
        Console.WriteLine($"  Dictionary TryGetValue: 1 lookup → {fromDict?.Name}");
        Console.WriteLine("  With thousands of SKUs, List scan grows linearly; Dictionary stays fast.");
        Console.WriteLine();
    }

    /*
     * SECTION 9: SortedDictionary PREVIEW
     *
     * Dictionary does not guarantee iteration order. Keys appear in internal hash
     * order that can change when the table resizes.
     *
     * SortedDictionary<TKey, TValue> keeps keys sorted (typically ascending) using
     * a balanced tree — O(log n) lookup/add, ordered foreach.
     *
     * COVERED IN DETAIL LATER → 07. SortedList and SortedDictionary
     *   (ordered keys, SortedList vs SortedDictionary, performance trade-offs)
     *
     * Use Dictionary for raw speed by key; use SortedDictionary when you need
     * keys in sorted order during iteration without sorting a copy yourself.
     */
    public static void DemonstrateSortedDictionaryPreview()
    {
        Console.WriteLine("--- Section 9: SortedDictionary preview ---");
        Console.WriteLine("Dictionary: fast hash lookup, no key order guarantee.");
        Console.WriteLine("SortedDictionary: keys sorted on insert — see chapter 07.");
        Console.WriteLine();
    }

    /*
     * SECTION 10: MAIN — orchestrates the chapter demo
     *
     * Main creates the catalog and calls each section method in reading order.
     */
    public static void Main(string[] args)
    {
        Console.WriteLine("=== 04. Dictionary<TKey, TValue> ===");
        Console.WriteLine();

        Dictionary<string, Product> catalog = CreateInitialCatalog();

        Console.WriteLine("--- Section 3: Initial catalog (collection initializer) ---");
        PrintCatalog(catalog);
        Console.WriteLine();

        DemonstrateCoreApi(catalog);
        DemonstrateSafeLookup(catalog);
        DemonstrateDeconstruction(catalog);
        DemonstratePitfalls(catalog);
        DemonstrateListVsDictionary();
        DemonstrateSortedDictionaryPreview();

        Console.WriteLine("=== End of Dictionary tutorial ===");
    }
}

/*
 * QUICK REFERENCE — Dictionary<TKey, TValue>
 *
 * --- Declaration ---
 *
 *   Dictionary<string, int> map = new Dictionary<string, int>();
 *   var map = new Dictionary<string, int> { ["a"] = 1, { "b", 2 } };
 *
 * --- Core operations ---
 *
 *   map.Add(key, value);           // throws ArgumentException if key exists
 *   map[key] = value;              // add or replace value
 *   TValue v = map[key];           // throws KeyNotFoundException if missing
 *   map.Remove(key);               // bool — was removed?
 *   map.Clear();
 *   int n = map.Count;
 *
 * --- Safe lookup ---
 *
 *   map.ContainsKey(key);                    // existence only
 *   map.TryGetValue(key, out TValue val);    // preferred when reading value
 *
 * --- Views and enumeration ---
 *
 *   map.Keys          // live ICollection<TKey>
 *   map.Values        // live ICollection<TValue>
 *   foreach (KeyValuePair<TKey,TValue> kv in map) { kv.Key; kv.Value; }
 *
 * --- List vs Dictionary ---
 *
 *   List       → ordered sequence, index by position, find-by-id scans all items
 *   Dictionary → unique keys, O(1) average lookup by key, no positional index
 *
 * --- Later chapters ---
 *
 *   SortedDictionary, SortedList  → 07. SortedList and SortedDictionary
 *   HashSet (unique values only)  → 05. HashSet
 *   IEnumerable iteration depth   → 08. IEnumerable and IEnumerator
 *
 * --- Common mistakes ---
 *
 *  Mistake                           | Result
 *  ----------------------------------|----------------------------------
 *  map[key] when key may be missing  | KeyNotFoundException
 *  Add with duplicate key            | ArgumentException
 *  null key (reference type TKey)    | ArgumentNullException
 *  Modify dict while foreach Keys    | InvalidOperationException
 *  ContainsKey then map[key]         | Works but two lookups — use TryGetValue
 */
