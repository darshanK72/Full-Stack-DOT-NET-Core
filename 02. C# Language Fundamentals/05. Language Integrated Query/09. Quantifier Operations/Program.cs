/*
 * =============================================================================
 * 09. QUANTIFIER OPERATIONS — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: LINQ yes/no operators — Any, All, Contains — plus SequenceEqual for
 *        pairwise sequence comparison. Predicate overloads use Func<T, bool>
 *        (same shape as Predicate<T> from Functional Style). Comparer overloads
 *        accept IEqualityComparer<T> for business-key or case-insensitive rules.
 *
 * WHY IT MATTERS:
 *   Business rules often ask: "Is the cart non-empty?", "Does every line have
 *   quantity > 0?", "Is this SKU already on the pick list?", "Do these two
 *   manifests match?" Quantifiers and SequenceEqual express those checks
 *   directly, short-circuit on the first decisive element, and read better
 *   than Count() comparisons or manual foreach loops.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Quantifier overview — bool results vs Where / Count; immediate execution
 *   2.  Any() — parameterless non-empty check
 *   3.  Any(predicate) — at least one element matches
 *   4.  All(predicate) — every element matches; vacuous truth on empty
 *   5.  Contains(value) — membership with default equality
 *   6.  Contains(value, IEqualityComparer<T>) — membership by business key
 *   7.  SequenceEqual — pairwise equality; empty/length/order edge cases
 *   8.  SequenceEqual(comparer) — custom element equality across two sequences
 *   9.  Combining quantifiers — shipment dock gate scenario
 *  10.  Short-circuit intuition — Any/All/Contains vs Count
 *
 * =============================================================================
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace QuantifierOperations;

/*
 * =========================================================================
 * SECTION 1: SAMPLE DOMAIN — OrderLine AND CartonSize
 * =========================================================================
 *
 * Warehouse pick-list rows drive Any / All / Contains demos. Predicates read
 * Sku, Quantity, UnitPrice, IsHazardous, and LineTotal.
 *
 * OrderLine does NOT override Equals/GetHashCode — Contains(probe) uses
 * reference equality unless you pass a custom IEqualityComparer<OrderLine>.
 *
 * CartonSize is a readonly record struct — value equality, so Contains and
 * SequenceEqual match by Units + Label without a custom comparer.
 * -------------------------------------------------------------------------
 */
public class OrderLine
{
    public string Sku { get; set; } = string.Empty;           // warehouse SKU key
    public string ProductName { get; set; } = string.Empty;   // display name on pick list
    public int Quantity { get; set; }                         // units to pick
    public decimal UnitPrice { get; set; }                    // price per unit
    public bool IsHazardous { get; set; }                     // hazmat flag for carrier routing

    public decimal LineTotal => Quantity * UnitPrice;         // computed line amount

    public OrderLine()
    {
    }

    public OrderLine(string sku, string productName, int quantity, decimal unitPrice, bool isHazardous)
    {
        Sku = sku;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
        IsHazardous = isHazardous;
    }

    public override string ToString() =>
        $"{Sku}  {ProductName,-28}  qty {Quantity,3}  @ {UnitPrice,8:C}  haz={IsHazardous}";
}

public readonly record struct CartonSize(int Units, string Label);

/*
 * =========================================================================
 * SECTION 2: OrderLineSkuComparer — BUSINESS-KEY EQUALITY
 * =========================================================================
 *
 * IEqualityComparer<OrderLine> that treats two lines as equal when their SKU
 * matches (case-insensitive). Pair with Contains(value, comparer) and
 * SequenceEqual(second, comparer) — same comparer pattern as Distinct(comparer)
 * in 07. Set Operations.
 *
 * Contract: if Equals returns true, GetHashCode MUST return the same value.
 * -------------------------------------------------------------------------
 */
public sealed class OrderLineSkuComparer : IEqualityComparer<OrderLine>
{
    public bool Equals(OrderLine? x, OrderLine? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true; // same instance (or both null)
        }

        if (x is null || y is null)
        {
            return false;
        }

        return string.Equals(x.Sku, y.Sku, StringComparison.OrdinalIgnoreCase); // SKU-only match
    }

    public int GetHashCode(OrderLine obj) =>
        obj.Sku.ToUpperInvariant().GetHashCode(StringComparison.Ordinal); // hash aligned with Equals
}

public class Program
{
    public static void Main(string[] args)
    {
        /*
         * =========================================================================
         * SECTION 3: QUANTIFIER OVERVIEW
         * =========================================================================
         *
         * LINQ groups many operators by what they RETURN:
         *
         *  Returns          | Examples                      | Question answered
         *  -----------------|-------------------------------|----------------------------------
         *  IEnumerable<T>   | Where, Select, OrderBy        | "Give me matching items"
         *  int / decimal    | Count, Sum, Average           | "How many / how much?"
         *  bool             | Any, All, Contains,           | "Yes or no?" / "Same sequence?"
         *                   | SequenceEqual                |
         *
         * Quantifiers live in System.Linq as extension methods on IEnumerable<T>.
         * They take a predicate (Func<T, bool>) except parameterless Any(),
         * value-based Contains, and SequenceEqual (which compares two sequences).
         *
         * Immediate execution: each call walks the sequence (stopping early when
         * possible) and returns bool right away — not a deferred query.
         *
         * Scenario: a warehouse shipment batch — each OrderLine is a pick-list row.
         * Gate checks run before the batch leaves the dock; SequenceEqual compares
         * a dock manifest to a scanner re-read.
         *
         * COVERED IN DETAIL EARLIER → 02. Filtering and Aggregation
         *   Where / Count; Any/All/Contains appear there only as a short preview.
         * -------------------------------------------------------------------------
         */

        List<OrderLine> shipmentBatch =
        [
            new OrderLine("WH-4412", "Industrial Shelving Unit", 4, 49.99m, false),
            new OrderLine("WH-8890", "Heavy-Duty Pallet Jack", 1, 899.00m, false),
            new OrderLine("WH-3305", "Barcode Scanner Kit", 2, 245.00m, false),
            new OrderLine("WH-9001", "Lithium Battery Pack", 6, 89.50m, true),
        ];

        List<OrderLine> emptyBatch = []; // empty sequence — Any / All / Contains / SequenceEqual edges

        Console.WriteLine("=== Shipment batch (pick list) ===");
        PrintBatch(shipmentBatch);

        /*
         * =========================================================================
         * SECTION 4: Any() — NON-EMPTY SEQUENCE
         * =========================================================================
         *
         *   bool hasItems = sequence.Any();
         *
         * Returns true when the sequence contains AT LEAST ONE element.
         * Does NOT accept a predicate — use Any(predicate) for that.
         *
         *  Source                    | Result
         *  --------------------------|------------------
         *  Empty IEnumerable<T>      | false
         *  One or more elements      | true
         *  null reference            | ArgumentNullException
         *
         * Prefer Any() over Count() > 0: Any stops after the first element;
         * Count() must walk the entire sequence (unless the source is ICollection
         * with an O(1) Count property — still less clear at the call site).
         * -------------------------------------------------------------------------
         */

        bool batchHasLines = shipmentBatch.Any(); // true — four pick-list rows
        bool emptyHasLines = emptyBatch.Any();    // false — no elements

        Console.WriteLine();
        Console.WriteLine("--- Any() — non-empty check ---");
        Console.WriteLine($"Shipment batch has lines: {batchHasLines}");
        Console.WriteLine($"Empty batch has lines:    {emptyHasLines}");

        /*
         * =========================================================================
         * SECTION 5: Any(predicate) — AT LEAST ONE MATCH
         * =========================================================================
         *
         *   bool found = sequence.Any(item => item.Quantity > 0);
         *
         * Signature: Any(this IEnumerable<T> source, Func<T, bool> predicate)
         *
         * Evaluates the predicate for each element IN ORDER until one returns true,
         * then returns true immediately (short-circuit). If no element matches,
         * returns false. Empty source → false (no element can match).
         *
         * Equivalent intent to: sequence.Where(predicate).Any()
         * but Any(predicate) avoids building an intermediate filtered sequence.
         *
         * --- 5a. Primitive sequence (int[]) ---
         * -------------------------------------------------------------------------
         */

        int[] temperatureReadingsC = [18, 19, 22, 24, 21, 27, 23];

        bool anyAboveThreshold = temperatureReadingsC.Any(t => t >= 25); // true — 27 matches

        Console.WriteLine();
        Console.WriteLine("--- Any(predicate) ---");
        Console.WriteLine($"Any reading >= 25 °C: {anyAboveThreshold}");

        /*
         * --- 5b. Complex type — OrderLine predicates ---
         *
         * Lambdas can read multiple properties. Gate questions: hazardous on board?
         * oversized quantity? expensive line total?
         * -------------------------------------------------------------------------
         */

        bool hasHazardousMaterial = shipmentBatch.Any(line => line.IsHazardous); // WH-9001
        bool hasOversizedOrder = shipmentBatch.Any(line => line.Quantity >= 5);  // qty 6
        bool hasExpensiveLine = shipmentBatch.Any(line => line.LineTotal > 500m); // pallet jack

        Console.WriteLine($"Batch contains hazardous SKU: {hasHazardousMaterial}");
        Console.WriteLine($"Any line with quantity >= 5: {hasOversizedOrder}");
        Console.WriteLine($"Any line total over $500: {hasExpensiveLine}");

        /*
         * =========================================================================
         * SECTION 6: All(predicate) — EVERY ELEMENT MATCHES
         * =========================================================================
         *
         *   bool ok = sequence.All(item => item.Quantity > 0);
         *
         * Returns true only when EVERY element satisfies the predicate.
         * Short-circuits on the FIRST element that fails the predicate.
         *
         * *** Vacuous truth on empty sequences ***
         *
         *   new List<int>().All(x => x > 100)   // true — zero elements to falsify
         *
         * Mathematically: "all elements of the empty set satisfy P" is true.
         * In validation code, guard with Any() first when "no lines" should fail:
         *
         *   bool valid = batch.Any() && batch.All(line => line.Quantity > 0);
         *
         * Empty-sequence summary (quantifiers so far):
         *
         *  Call                         | Empty source
         *  -----------------------------|----------------
         *  Any()                        | false
         *  Any(predicate)               | false
         *  All(predicate)               | true  (vacuous)
         *  Contains(value)              | false
         *
         * --- 6a. All lines have positive quantity ---
         * -------------------------------------------------------------------------
         */

        bool allQuantitiesPositive = shipmentBatch.All(line => line.Quantity > 0);
        bool emptyVacuouslyTrue = emptyBatch.All(line => line.Quantity > 0); // vacuous truth

        Console.WriteLine();
        Console.WriteLine("--- All(predicate) ---");
        Console.WriteLine($"All shipment lines qty > 0: {allQuantitiesPositive}");
        Console.WriteLine($"Empty batch All(qty > 0):   {emptyVacuouslyTrue}  (vacuous truth)");

        /*
         * --- 6b. Composite business rules with All ---
         * -------------------------------------------------------------------------
         */

        bool allSkusPrefixed = shipmentBatch.All(line =>
            line.Sku.StartsWith("WH-", StringComparison.Ordinal));
        bool noneHazardous = shipmentBatch.All(line => !line.IsHazardous); // false — battery pack

        Console.WriteLine($"All SKUs start with WH-:     {allSkusPrefixed}");
        Console.WriteLine($"All lines non-hazardous:     {noneHazardous}");

        bool shipmentReadyForStandardCarrier =
            shipmentBatch.Any()                                       // reject empty batch
            && shipmentBatch.All(line => line.Quantity > 0)
            && shipmentBatch.All(line => line.UnitPrice > 0m);

        Console.WriteLine($"Ready for standard carrier:  {shipmentReadyForStandardCarrier}");

        /*
         * =========================================================================
         * SECTION 7: Contains(value) — DEFAULT EQUALITY
         * =========================================================================
         *
         *   bool onList = sequence.Contains(value);
         *
         * Tests MEMBERSHIP: "Is this exact value present?"
         * Uses EqualityComparer<T>.Default unless you pass a custom comparer.
         *
         * For int, string, bool — value equality works as expected.
         *
         *   new[] { 1, 2, 3 }.Contains(2)           // true
         *   new[] { "a", "b" }.Contains("A")        // false (case-sensitive default)
         *
         * Strings often need StringComparer for case-insensitive membership:
         *
         *   carriers.Contains("fedex", StringComparer.OrdinalIgnoreCase)
         *
         * Empty sequence → false. null source → ArgumentNullException.
         *
         * --- 7a. Primitive and string arrays ---
         * -------------------------------------------------------------------------
         */

        int[] allowedCartons = [12, 24, 48];
        string[] carrierCodes = ["UPS", "FEDEX", "DHL"];

        bool cartonOk = allowedCartons.Contains(24);
        bool carrierOk = carrierCodes.Contains("fedex", StringComparer.OrdinalIgnoreCase);
        bool badCarton = allowedCartons.Contains(36);
        bool emptyContains = emptyBatch.Select(line => line.Sku).Contains("WH-4412"); // false

        Console.WriteLine();
        Console.WriteLine("--- Contains(value) — primitives / strings ---");
        Console.WriteLine($"Carton size 24 allowed: {cartonOk}");
        Console.WriteLine($"Carrier FEDEX allowed:  {carrierOk}");
        Console.WriteLine($"Carton size 36 allowed: {badCarton}");
        Console.WriteLine($"Empty SKU list Contains: {emptyContains}");

        /*
         * --- 7b. Contains on List<T> for reference types (default comparer) ---
         *
         * OrderLine is a class with NO Equals/GetHashCode override.
         * EqualityComparer<OrderLine>.Default uses reference equality —
         * two different instances with the same SKU are NOT equal.
         * -------------------------------------------------------------------------
         */

        OrderLine probeSameSku = new OrderLine("WH-4412", "Industrial Shelving Unit", 4, 49.99m, false);
        OrderLine probeExactReference = shipmentBatch[0]; // same instance as list element

        bool containsByValue = shipmentBatch.Contains(probeSameSku);           // false — different refs
        bool containsByReference = shipmentBatch.Contains(probeExactReference); // true — same object

        Console.WriteLine();
        Console.WriteLine("--- Contains on OrderLine (reference equality pitfall) ---");
        Console.WriteLine($"Contains new instance (same SKU): {containsByValue}");
        Console.WriteLine($"Contains same list reference [0]: {containsByReference}");

        /*
         * =========================================================================
         * SECTION 8: Contains(value, IEqualityComparer<T>)
         * =========================================================================
         *
         *   bool found = list.Contains(probe, new OrderLineSkuComparer());
         *
         * When T is a reference type and equality should follow a BUSINESS KEY
         * (SKU, employee ID, ISBN), pass IEqualityComparer<T>.
         *
         * OrderLineSkuComparer (Section 2) compares SKU case-insensitively.
         *
         * --- 8a. Custom comparer on OrderLine ---
         * -------------------------------------------------------------------------
         */

        OrderLineSkuComparer skuComparer = new OrderLineSkuComparer();

        bool containsBySku = shipmentBatch.Contains(probeSameSku, skuComparer); // true by SKU
        bool containsUnknownSku = shipmentBatch.Contains(
            new OrderLine("WH-9999", "Unknown", 1, 1m, false),
            skuComparer); // false — SKU not on batch

        Console.WriteLine();
        Console.WriteLine("--- Contains(value, IEqualityComparer<T>) ---");
        Console.WriteLine($"Contains by SKU (comparer):     {containsBySku}");
        Console.WriteLine($"Contains unknown SKU:           {containsUnknownSku}");

        /*
         * --- 8b. Value types and records — Contains often "just works" ---
         *
         * CartonSize is a readonly record struct (Section 1) — value equality.
         * -------------------------------------------------------------------------
         */

        List<CartonSize> cartonInventory =
        [
            new CartonSize(12, "Small"),
            new CartonSize(24, "Medium"),
            new CartonSize(48, "Large"),
        ];

        bool hasMediumCarton = cartonInventory.Contains(new CartonSize(24, "Medium"));

        Console.WriteLine();
        Console.WriteLine("--- Contains on record (value equality) ---");
        Console.WriteLine($"Carton inventory has Medium (24): {hasMediumCarton}");

        /*
         * =========================================================================
         * SECTION 9: SequenceEqual — PAIRWISE SEQUENCE COMPARISON
         * =========================================================================
         *
         *   bool same = first.SequenceEqual(second);
         *
         * Returns true when BOTH sequences have the SAME LENGTH and corresponding
         * elements are equal under EqualityComparer<T>.Default (or a supplied
         * comparer — Section 10). Order matters: [1, 2] is NOT equal to [2, 1].
         *
         * Immediate execution; walks both enumerators in lockstep and short-circuits
         * on the first mismatched pair or when one sequence ends early.
         *
         *  first          | second         | Result | Why
         *  ---------------|----------------|--------|---------------------------
         *  [1, 2, 3]      | [1, 2, 3]      | true   | same length + same values
         *  [1, 2, 3]      | [1, 2, 4]      | false  | element mismatch at index 2
         *  [1, 2]         | [1, 2, 3]      | false  | different lengths
         *  [1, 2]         | [2, 1]         | false  | order matters
         *  []             | []             | true   | both empty
         *  []             | [1]            | false  | empty vs non-empty
         *  null           | anything       | throws | ArgumentNullException
         *
         * Use SequenceEqual when you need "same items in the same order," not
         * set equality (that is Union/Intersect territory in 07. Set Operations).
         *
         * --- 9a. Primitive sequences and empty edges ---
         * -------------------------------------------------------------------------
         */

        int[] dockManifestSkus = [4412, 8890, 3305, 9001];
        int[] scannerReRead = [4412, 8890, 3305, 9001];
        int[] scannerReReadOutOfOrder = [8890, 4412, 3305, 9001];
        int[] scannerMissingLine = [4412, 8890, 3305];
        int[] emptyManifest = [];
        int[] anotherEmpty = [];

        bool manifestsMatch = dockManifestSkus.SequenceEqual(scannerReRead);
        bool orderMismatch = dockManifestSkus.SequenceEqual(scannerReReadOutOfOrder);
        bool lengthMismatch = dockManifestSkus.SequenceEqual(scannerMissingLine);
        bool bothEmptyEqual = emptyManifest.SequenceEqual(anotherEmpty); // true — empty == empty
        bool emptyVsItems = emptyManifest.SequenceEqual(dockManifestSkus); // false

        Console.WriteLine();
        Console.WriteLine("--- SequenceEqual (default equality) ---");
        Console.WriteLine($"Dock vs scanner (identical):  {manifestsMatch}");
        Console.WriteLine($"Dock vs out-of-order scan:    {orderMismatch}");
        Console.WriteLine($"Dock vs missing line:         {lengthMismatch}");
        Console.WriteLine($"Empty vs empty:               {bothEmptyEqual}");
        Console.WriteLine($"Empty vs non-empty:           {emptyVsItems}");

        /*
         * --- 9b. Records — value equality element-by-element ---
         * -------------------------------------------------------------------------
         */

        CartonSize[] packedCartons =
        [
            new CartonSize(12, "Small"),
            new CartonSize(24, "Medium"),
        ];
        CartonSize[] expectedCartons =
        [
            new CartonSize(12, "Small"),
            new CartonSize(24, "Medium"),
        ];

        bool cartonPlansMatch = packedCartons.SequenceEqual(expectedCartons);

        Console.WriteLine($"Packed cartons match plan:    {cartonPlansMatch}");

        /*
         * --- 9c. Reference types without Equals — instance identity ---
         *
         * SequenceEqual on OrderLine uses reference equality by default.
         * Two lists with "same looking" lines but different instances → false
         * unless you pass a comparer (next section) or reuse the same objects.
         * -------------------------------------------------------------------------
         */

        List<OrderLine> reprintBatch =
        [
            new OrderLine("WH-4412", "Industrial Shelving Unit", 4, 49.99m, false),
            new OrderLine("WH-8890", "Heavy-Duty Pallet Jack", 1, 899.00m, false),
            new OrderLine("WH-3305", "Barcode Scanner Kit", 2, 245.00m, false),
            new OrderLine("WH-9001", "Lithium Battery Pack", 6, 89.50m, true),
        ];

        bool sameInstances = shipmentBatch.SequenceEqual(shipmentBatch); // true — same sequence object
        bool reprintLooksSame = shipmentBatch.SequenceEqual(reprintBatch); // false — new instances

        Console.WriteLine($"SequenceEqual same list:      {sameInstances}");
        Console.WriteLine($"SequenceEqual reprint rows:   {reprintLooksSame}  (ref equality)");

        /*
         * =========================================================================
         * SECTION 10: SequenceEqual(second, IEqualityComparer<T>)
         * =========================================================================
         *
         *   bool same = first.SequenceEqual(second, comparer);
         *
         * Same pairwise / same-length rules as Section 9, but each corresponding
         * pair is compared with the supplied IEqualityComparer<T>.
         *
         * Common cases:
         *   StringComparer.OrdinalIgnoreCase — case-insensitive string sequences
         *   Custom comparer — business key on reference types (SKU, Id)
         *
         * Empty vs empty still returns true (comparer is never asked to compare
         * an element when both sequences have zero length).
         * -------------------------------------------------------------------------
         */

        string[] plannedCarriers = ["UPS", "FEDEX", "DHL"];
        string[] bookedCarriers = ["ups", "FedEx", "dhl"];

        bool carriersMatchIgnoreCase =
            plannedCarriers.SequenceEqual(bookedCarriers, StringComparer.OrdinalIgnoreCase);

        bool carriersMatchDefault =
            plannedCarriers.SequenceEqual(bookedCarriers); // false — case-sensitive default

        bool reprintMatchesBySku =
            shipmentBatch.SequenceEqual(reprintBatch, skuComparer); // true — SKU order matches

        List<OrderLine> emptyReprint = [];
        bool emptyBatchesEqualBySku = emptyBatch.SequenceEqual(emptyReprint, skuComparer);

        Console.WriteLine();
        Console.WriteLine("--- SequenceEqual(second, IEqualityComparer<T>) ---");
        Console.WriteLine($"Carriers ignore case:         {carriersMatchIgnoreCase}");
        Console.WriteLine($"Carriers default equality:    {carriersMatchDefault}");
        Console.WriteLine($"Reprint matches by SKU:       {reprintMatchesBySku}");
        Console.WriteLine($"Empty batches (SKU comparer): {emptyBatchesEqualBySku}");

        /*
         * =========================================================================
         * SECTION 11: SHIPMENT GATE — COMBINING QUANTIFIERS
         * =========================================================================
         *
         * Real dock checks combine Any, All, Contains, and SequenceEqual:
         *
         *  Gate                         | Operator used
         *  -----------------------------|----------------------------------------
         *  Batch not empty              | Any()
         *  Every line valid price       | All(predicate)
         *  Hazardous present            | Any(line => line.IsHazardous)
         *  Restricted SKU on batch      | Any(line => restricted.Contains(line.Sku))
         *  Scanner matches dock plan    | SequenceEqual (SKU codes / comparer)
         *
         * Restricted SKU list uses string Contains — membership of a scalar value
         * in a small allowed/denied set.
         * -------------------------------------------------------------------------
         */

        string[] restrictedSkus = ["WH-9001", "WH-7777"];
        string[] dockSkuOrder = shipmentBatch.Select(line => line.Sku).ToArray();
        string[] scannerSkuOrder = ["WH-4412", "WH-8890", "WH-3305", "WH-9001"];

        bool batchNotEmpty = shipmentBatch.Any();
        bool allLinesPriced = shipmentBatch.All(line => line.UnitPrice > 0m);
        bool hazardousDetected = shipmentBatch.Any(line => line.IsHazardous);
        bool anyRestrictedSku = shipmentBatch.Any(line => restrictedSkus.Contains(line.Sku));
        bool scannerMatchesDock = dockSkuOrder.SequenceEqual(scannerSkuOrder);

        bool passesDockCheck =
            batchNotEmpty
            && allLinesPriced
            && !anyRestrictedSku // standard dock rejects restricted SKUs
            && scannerMatchesDock;

        Console.WriteLine();
        Console.WriteLine("=== Dock gate summary ===");
        Console.WriteLine($"  Batch not empty:        {batchNotEmpty}");
        Console.WriteLine($"  All lines priced:       {allLinesPriced}");
        Console.WriteLine($"  Hazardous detected:     {hazardousDetected}");
        Console.WriteLine($"  Restricted SKU present: {anyRestrictedSku}");
        Console.WriteLine($"  Scanner == dock SKUs:   {scannerMatchesDock}");
        Console.WriteLine($"  PASSES standard dock:   {passesDockCheck}");

        if (hazardousDetected)
        {
            Console.WriteLine("  -> Route to hazmat carrier (Any found hazardous line).");
        }

        /*
         * =========================================================================
         * SECTION 12: SHORT-CIRCUIT INTUITION — Any / All / Contains vs Count
         * =========================================================================
         *
         * Quantifiers (and SequenceEqual) stop as soon as the answer is known:
         *
         *  Expression                         | Stops early? | Notes
         *  -----------------------------------|--------------|---------------------------
         *  Any()                              | Yes          | After first element
         *  Any(predicate)                     | Yes          | First true predicate
         *  All(predicate)                     | Yes          | First false predicate
         *  Contains(value)                    | Yes          | First equal element
         *  SequenceEqual(second)              | Yes          | First mismatch / early end
         *  Count(predicate) > 0               | No*          | Count scans all matches
         *  Where(pred).Any()                  | Yes          | Extra iterator layer
         *
         * * Count with a predicate enumerates the whole sequence even when you
         *   only need "is there at least one?" Prefer Any for boolean intent.
         *
         * C# query syntax has no `any`, `all`, or `sequenceequal` keyword. You can
         * append .Any() after a query, but method syntax is idiomatic:
         *
         *   batch.Any(line => line.Quantity > 5)
         * -------------------------------------------------------------------------
         */

        bool viaMethod = shipmentBatch.Any(line => line.Quantity > 5);
        bool viaQuery = (from line in shipmentBatch
                         where line.Quantity > 5
                         select line).Any(); // query + Any — same bool intent
        int matchingCount = shipmentBatch.Count(line => line.Quantity > 5);

        Console.WriteLine();
        Console.WriteLine("--- Short-circuit: Any vs Count; query append ---");
        Console.WriteLine($"Any(qty > 5) method:  {viaMethod}");
        Console.WriteLine($"Any(qty > 5) query+:  {viaQuery}");
        Console.WriteLine($"Count(qty > 5) > 0:   {matchingCount > 0}  (count was {matchingCount})");
    }

    private static void PrintBatch(IEnumerable<OrderLine> lines)
    {
        foreach (OrderLine line in lines)
        {
            Console.WriteLine($"  {line}");
        }
    }
}

/*
 * =========================================================================
 * QUICK REFERENCE — QUANTIFIER OPERATIONS
 * =========================================================================
 *
 * --- Any ---
 *
 *   sequence.Any()                      // true if at least one element exists
 *   sequence.Any(x => condition)        // true if any element matches predicate
 *
 * --- All ---
 *
 *   sequence.All(x => condition)        // true if every element matches (empty → true)
 *
 * --- Contains ---
 *
 *   sequence.Contains(value)                          // default equality
 *   sequence.Contains(value, comparer)                // custom IEqualityComparer<T>
 *   strings.Contains(s, StringComparer.OrdinalIgnoreCase)
 *
 * --- SequenceEqual ---
 *
 *   first.SequenceEqual(second)                       // pairwise + same length
 *   first.SequenceEqual(second, comparer)             // custom element equality
 *   // empty == empty → true; order matters; different length → false
 *
 * --- Empty-sequence behavior ---
 *
 *  Any() / Any(pred) / Contains(v) → false
 *  All(pred)                       → true (vacuous truth)
 *  SequenceEqual(empty, empty)     → true
 *  SequenceEqual(empty, nonEmpty)  → false
 *
 * --- Short-circuit ---
 *
 *   Any  — stops on first match (or first element for parameterless Any)
 *   All  — stops on first failure
 *   Contains — stops on first equal element
 *   SequenceEqual — stops on first mismatch or when one sequence ends early
 *   Prefer Any over Count() > 0 when you only need a bool
 *
 * --- Predicate / comparer types ---
 *
 *   Func<T, bool>         — Any / All predicate (04. Func Action and Predicate)
 *   IEqualityComparer<T>  — Contains / SequenceEqual custom equality (07. Set Ops)
 *
 * --- Common patterns ---
 *
 *   Non-empty + all valid:
 *     batch.Any() && batch.All(line => line.Quantity > 0)
 *
 *   Membership by business key on class without Equals:
 *     list.Contains(probe, new OrderLineSkuComparer())
 *
 *   Restricted SKU check:
 *     lines.Any(line => deniedSkus.Contains(line.Sku))
 *
 *   Manifest verification:
 *     dockSkus.SequenceEqual(scannerSkus)
 *     linesA.SequenceEqual(linesB, new OrderLineSkuComparer())
 *
 * --- Common mistakes ---
 *
 *  Mistake                              | Result
 *  -------------------------------------|----------------------------------
 *  All on empty without Any() guard     | Vacuous true — empty "passes"
 *  Contains on class (no Equals)        | false unless same reference
 *  SequenceEqual on class (no Equals)   | false unless same instances/order
 *  SequenceEqual vs set equality        | Order/duplicates matter; use set ops
 *  Count() > 0 instead of Any()         | Works but scans entire sequence
 *  null sequence                        | ArgumentNullException on extension
 *
 * --- Related chapters ---
 *
 *   02. Filtering and Aggregation  — Where, Count; Any/All/Contains preview
 *   04. Func Action and Predicate  — Func<T, bool> predicate shape
 *   06. Element Operations         — First/Single when you need the item itself
 *   07. Set Operations             — IEqualityComparer with Distinct / Union
 *   08. Projection Operations      — Select before quantifier if shape changes
 *
 * =========================================================================
 */
