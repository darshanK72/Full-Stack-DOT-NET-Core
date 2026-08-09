/*
 * =============================================================================
 * 05. HASHSET<T> — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: HashSet<T> — a generic collection that stores UNIQUE elements with
 *        fast average O(1) Add, Remove, and Contains. Set algebra (union,
 *        intersection, difference, symmetric difference), relational checks
 *        (subset, superset, SetEquals), and custom equality via
 *        IEqualityComparer<T> or overridden GetHashCode/Equals on element types.
 *
 * WHY IT MATTERS:
 *   Applications constantly need distinct values and set comparisons: unique
 *   visitor IDs, merged permission sets, overlapping product tags, deduplicated
 *   imports. HashSet expresses those rules without nested loops and gives fast
 *   membership tests as data grows.
 *
 * WHAT YOU WILL LEARN:
 *   1.  HashSet<T> — uniqueness, Add / Remove / Contains, no indexing
 *   2.  Hash tables — GetHashCode, buckets, collisions, element equality
 *   3.  Non-mutating set operations (Union, Intersect, Except, SymmetricExcept)
 *   4.  Mutating set operations (UnionWith, IntersectWith, ExceptWith, …)
 *   5.  Relational checks — IsSubsetOf, IsSupersetOf, Overlaps, SetEquals
 *   6.  Custom equality with IEqualityComparer<T>
 *   7.  Custom equality with GetHashCode and Equals on element types
 *   8.  SortedSet<T> preview — unique + sorted (full detail in ch.07)
 *   9.  Choosing HashSet vs List vs Dictionary
 *
 * =============================================================================
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace HashSetDemo;

/*
 * =========================================================================
 * SECTION 1: HASHSET<T> — UNIQUE ELEMENTS
 * =========================================================================
 *
 * HashSet<T> stores DISTINCT elements. Duplicates are rejected on Add.
 * There is NO index access (unlike List<T> or array). Order is NOT guaranteed —
 * iteration walks hash buckets, not insertion order.
 *
 *  Operation   | Average time | Notes
 *  ------------|--------------|------------------------------------------
 *  Add         | O(1)         | Returns false if element already present
 *  Contains    | O(1)         | Membership test
 *  Remove      | O(1)         | Removes if found; returns bool
 *  Count       | O(1)         | Number of unique elements
 *
 * Article holds blog post tags in a HashSet<string> so each tag appears once.
 * StringComparer.OrdinalIgnoreCase treats "LINQ" and "linq" as the same tag.
 * -------------------------------------------------------------------------
 */
public class Article
{
    public string Title { get; }

    public HashSet<string> Tags { get; }

    public Article(string title, IEnumerable<string> tags)
    {
        Title = title;
        Tags = new HashSet<string>(tags, StringComparer.OrdinalIgnoreCase); // case-insensitive uniqueness
    }

    public override string ToString() => $"{Title} [{string.Join(", ", Tags)}]";
}

/*
 * =========================================================================
 * SECTION 6: CUSTOM EQUALITY — IEqualityComparer<T>
 * =========================================================================
 *
 * Default equality for reference types uses ReferenceEquals unless T overrides
 * Equals/GetHashCode. Two Subscriber objects with the same email are DIFFERENT
 * elements in a default HashSet<Subscriber>.
 *
 * Pass IEqualityComparer<T> to the HashSet constructor to define "same element"
 * by business rules (here: email address, case-insensitive).
 *
 * Contract (same as Dictionary keys): if Equals returns true, GetHashCode MUST
 * match — violating this breaks Contains and Remove silently.
 * -------------------------------------------------------------------------
 */
public class Subscriber
{
    public string Name { get; }

    public string Email { get; }

    public Subscriber(string name, string email)
    {
        Name = name;
        Email = email;
    }

    public override string ToString() => $"{Name} <{Email}>";
}

public class SubscriberByEmailComparer : IEqualityComparer<Subscriber>
{
    public bool Equals(Subscriber? x, Subscriber? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (x is null || y is null)
        {
            return false;
        }

        return string.Equals(x.Email, y.Email, StringComparison.OrdinalIgnoreCase);
    }

    public int GetHashCode(Subscriber obj) =>
        StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Email); // hash must follow Equals rule
}

/*
 * =========================================================================
 * SECTION 7: CUSTOM EQUALITY — GetHashCode AND Equals ON THE TYPE
 * =========================================================================
 *
 * Alternative to IEqualityComparer<T>: implement IEquatable<T> and override
 * Equals(object) + GetHashCode on the element type itself. HashSet<T> uses
 * these methods when no custom comparer is supplied.
 *
 * Use immutable fields for hash inputs — if Email changed after Add, the
 * element would sit in the wrong bucket and lookup would fail.
 * -------------------------------------------------------------------------
 */
public sealed class SubscriberIdentity : IEquatable<SubscriberIdentity>
{
    public string Email { get; }

    public string DisplayName { get; }

    public SubscriberIdentity(string email, string displayName)
    {
        Email = email;
        DisplayName = displayName;
    }

    public bool Equals(SubscriberIdentity? other) =>
        other is not null &&
        string.Equals(Email, other.Email, StringComparison.OrdinalIgnoreCase);

    public override bool Equals(object? obj) => obj is SubscriberIdentity other && Equals(other);

    public override int GetHashCode() =>
        StringComparer.OrdinalIgnoreCase.GetHashCode(Email);

    public override string ToString() => $"{DisplayName} <{Email}>";
}

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("=== HashSet<T> tutorial ===");
        Console.WriteLine();

        DemonstrateBasics();
        DemonstrateHashTableInternals();
        DemonstrateNonMutatingSetOperations();
        DemonstrateMutatingSetOperations();
        DemonstrateRelationalChecks();
        DemonstrateComparerEquality();
        DemonstrateTypeEquality();
        DemonstrateSortedSetPreview();
        DemonstrateCollectionChoice();
    }

    /*
     * SECTION 1 (demo): Add, Remove, Contains on Article.Tags
     */
    private static void DemonstrateBasics()
    {
        Article dotnetArticle = new Article(
            "Generics in C#",
            new[] { "csharp", "generics", "collections", "dotnet" });

        Article linqArticle = new Article(
            "LINQ Set Operations",
            new[] { "csharp", "linq", "collections", "queries" });

        Console.WriteLine("--- Section 1: HashSet uniqueness ---");
        Console.WriteLine(dotnetArticle);
        Console.WriteLine(linqArticle);
        Console.WriteLine($"dotnet tag count: {dotnetArticle.Tags.Count}");

        bool addedNew = dotnetArticle.Tags.Add("performance");           // new tag → true
        bool addedDuplicate = dotnetArticle.Tags.Add("generics");        // duplicate → false

        Console.WriteLine($"Add \"performance\": {addedNew}");
        Console.WriteLine($"Add duplicate \"generics\": {addedDuplicate}");
        Console.WriteLine($"Contains \"GENERICS\" (case-insensitive): {dotnetArticle.Tags.Contains("GENERICS")}");

        bool removed = dotnetArticle.Tags.Remove("performance");         // present → true
        bool removedMissing = dotnetArticle.Tags.Remove("missing-tag");    // absent → false

        Console.WriteLine($"Remove \"performance\": {removed}");
        Console.WriteLine($"Remove missing tag: {removedMissing}");
        Console.WriteLine($"Tags after Remove: [{FormatTags(dotnetArticle.Tags)}]");
        Console.WriteLine();

        // Stash articles for later sections (same tag scenario throughout the chapter)
        _dotnetArticle = dotnetArticle;
        _linqArticle = linqArticle;
    }

    private static Article _dotnetArticle = null!;
    private static Article _linqArticle = null!;

    /*
     * =========================================================================
     * SECTION 2: HASH TABLES — STORAGE, HASHING, COLLISIONS
     * =========================================================================
     *
     * HashSet<T> uses a hash table (like Dictionary without separate values).
     * Uniqueness means at most one element per equality group in the table.
     *
     * --- Add / Contains path (conceptual) ---
     *
     *  Step | On tags.Add("generics") or tags.Contains("generics")
     *  -----|---------------------------------------------------------------
     *  1    | Compute hash: comparer.GetHashCode(element) or element.GetHashCode()
     *  2    | Map hash → bucket index in internal bucket array
     *  3    | Walk bucket chain; call Equals on each candidate
     *  4    | Add: insert if no Equal match; reject duplicate if match found
     *  5    | Contains: return true on first Equal match; false if chain ends
     *
     * --- Collisions ---
     *
     * Two unequal strings CAN share a bucket — normal. Equals decides membership,
     * not hash alone. If Equals says equal, GetHashCode MUST match.
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateHashTableInternals()
    {
        string tagSample = "csharp";
        int tagHash = StringComparer.OrdinalIgnoreCase.GetHashCode(tagSample);

        Console.WriteLine("--- Section 2: Hash table internals ---");
        Console.WriteLine($"Comparer hash for \"{tagSample}\": {tagHash}");
        Console.WriteLine("Add/Contains: hash → bucket → chain → Equals (collision-safe).");
        Console.WriteLine("Duplicate Add returned false because Equals found \"generics\" already.");
        Console.WriteLine();
    }

    /*
     * =========================================================================
     * SECTION 3: NON-MUTATING SET OPERATIONS
     * =========================================================================
     *
     * LINQ extension methods on IEnumerable<T> return a NEW sequence without
     * changing the original HashSet. Wrap results in HashSet if you need set
     * semantics and the same comparer.
     *
     *  Method / pattern          | Result (A op B)
     *  ---------------------------|-----------------------------------------------
     *  A.Union(B)                 | All elements in A OR B
     *  A.Intersect(B)             | Elements in BOTH A and B
     *  A.Except(B)                | Elements in A but NOT in B
     *  copy of A + SymmetricExceptWith(B) | In A or B but NOT in both
     *                                     (no Enumerable.SymmetricExcept in LINQ)
     *
     * Instance methods UnionWith / IntersectWith / … MUTATE — see Section 4.
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateNonMutatingSetOperations()
    {
        HashSet<string> unionTags = new HashSet<string>(
            _dotnetArticle.Tags.Union(_linqArticle.Tags),
            StringComparer.OrdinalIgnoreCase);

        HashSet<string> sharedTags = new HashSet<string>(
            _dotnetArticle.Tags.Intersect(_linqArticle.Tags),
            StringComparer.OrdinalIgnoreCase);

        HashSet<string> dotnetOnlyTags = new HashSet<string>(
            _dotnetArticle.Tags.Except(_linqArticle.Tags),
            StringComparer.OrdinalIgnoreCase);

        HashSet<string> exclusiveTags = new HashSet<string>(_dotnetArticle.Tags, StringComparer.OrdinalIgnoreCase);
        exclusiveTags.SymmetricExceptWith(_linqArticle.Tags); // mutates copy only

        Console.WriteLine("--- Section 3: Non-mutating set operations ---");
        Console.WriteLine($"Union (all topics):        [{FormatTags(unionTags)}]");
        Console.WriteLine($"Intersect (shared):        [{FormatTags(sharedTags)}]");
        Console.WriteLine($"Except (dotnet only):      [{FormatTags(dotnetOnlyTags)}]");
        Console.WriteLine($"SymmetricExcept (either):  [{FormatTags(exclusiveTags)}]");
        Console.WriteLine($"Original dotnet tags unchanged: [{FormatTags(_dotnetArticle.Tags)}]");
        Console.WriteLine();

        _sharedTags = sharedTags;
        _dotnetOnlyTags = dotnetOnlyTags;
    }

    private static HashSet<string> _sharedTags = null!;
    private static HashSet<string> _dotnetOnlyTags = null!;

    /*
     * =========================================================================
     * SECTION 4: MUTATING SET OPERATIONS
     * =========================================================================
     *
     * Instance methods modify THIS HashSet in place and return void.
     * Prefer them when building a working set incrementally without allocating
     * a new collection each step.
     *
     *  Method                 | Effect on caller set
     *  -----------------------|------------------------------------------------
     *  UnionWith(other)       | Add all elements from other
     *  IntersectWith(other)   | Keep only elements also in other
     *  ExceptWith(other)      | Remove elements that appear in other
     *  SymmetricExceptWith    | Keep elements in exactly one of the two sets
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateMutatingSetOperations()
    {
        HashSet<string> editorPool = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "csharp",
            "dotnet"
        };

        HashSet<string> draftTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "dotnet",
            "api",
            "security"
        };

        editorPool.UnionWith(draftTags); // merges draft tags into editor pool

        HashSet<string> allowedForPublish = new HashSet<string>(editorPool, StringComparer.OrdinalIgnoreCase);
        HashSet<string> blockedTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "draft", "wip" };
        allowedForPublish.ExceptWith(blockedTags);

        HashSet<string> candidateTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "csharp",
            "dotnet",
            "api"
        };
        candidateTags.IntersectWith(allowedForPublish);

        HashSet<string> setA = new HashSet<string> { "a", "b", "c" };
        HashSet<string> setB = new HashSet<string> { "b", "c", "d" };
        setA.SymmetricExceptWith(setB);

        Console.WriteLine("--- Section 4: Mutating set operations ---");
        Console.WriteLine($"Editor pool after UnionWith:     [{FormatTags(editorPool)}]");
        Console.WriteLine($"Publishable after ExceptWith:    [{FormatTags(allowedForPublish)}]");
        Console.WriteLine($"Candidates after IntersectWith:  [{FormatTags(candidateTags)}]");
        Console.WriteLine($"SymmetricExcept {{a,b,c}} vs {{b,c,d}}: [{FormatTags(setA)}]");
        Console.WriteLine();

        _editorPool = editorPool;
    }

    private static HashSet<string> _editorPool = null!;

    /*
     * =========================================================================
     * SECTION 5: RELATIONAL CHECKS — SUBSET, SUPERSET, SetEquals
     * =========================================================================
     *
     * HashSet<T> can compare itself to another ISet<T> or IEnumerable<T>
     * without building a new collection first.
     *
     *  Method              | True when
     *  --------------------|---------------------------------------------------
     *  IsSubsetOf(other)   | Every element in this set is in other
     *  IsProperSubsetOf    | Subset AND this.Count < other.Count
     *  IsSupersetOf(other) | Every element in other is in this set
     *  IsProperSupersetOf  | Superset AND this.Count > other.Count
     *  Overlaps(other)     | At least one shared element
     *  SetEquals(other)    | Same elements (order irrelevant)
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateRelationalChecks()
    {
        HashSet<string> allTags = new HashSet<string>(
            _dotnetArticle.Tags.Union(_linqArticle.Tags),
            StringComparer.OrdinalIgnoreCase);

        HashSet<string> beginnerSlice = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "csharp",
            "collections"
        };

        Console.WriteLine("--- Section 5: Relational checks ---");
        Console.WriteLine($"sharedTags.IsSubsetOf(allTags): {_sharedTags.IsSubsetOf(allTags)}");
        Console.WriteLine($"sharedTags.IsProperSubsetOf(allTags): {_sharedTags.IsProperSubsetOf(allTags)}");
        Console.WriteLine($"allTags.IsSupersetOf(_sharedTags): {allTags.IsSupersetOf(_sharedTags)}");
        Console.WriteLine($"_editorPool.IsSupersetOf(_sharedTags): {_editorPool.IsSupersetOf(_sharedTags)}");
        Console.WriteLine($"sharedTags.Overlaps(_dotnetOnlyTags): {_sharedTags.Overlaps(_dotnetOnlyTags)}");
        Console.WriteLine($"SetEquals shared vs copy: {_sharedTags.SetEquals(new HashSet<string>(_sharedTags, StringComparer.OrdinalIgnoreCase))}");
        Console.WriteLine($"beginnerSlice.IsSubsetOf(_dotnetArticle.Tags): {beginnerSlice.IsSubsetOf(_dotnetArticle.Tags)}");
        Console.WriteLine();
    }

    /*
     * SECTION 6 (demo): HashSet with IEqualityComparer<Subscriber>
     */
    private static void DemonstrateComparerEquality()
    {
        SubscriberByEmailComparer emailComparer = new SubscriberByEmailComparer();

        HashSet<Subscriber> subscribers = new HashSet<Subscriber>(emailComparer)
        {
            new Subscriber("Alex", "alex@example.com"),
            new Subscriber("Alex K.", "alex@example.com"), // same email → rejected
            new Subscriber("Sam", "sam@example.com")
        };

        HashSet<Subscriber> defaultEqualitySet = new HashSet<Subscriber>
        {
            new Subscriber("Alex", "alex@example.com"),
            new Subscriber("Alex", "alex@example.com") // different objects → both kept
        };

        Console.WriteLine("--- Section 6: IEqualityComparer<T> ---");
        Console.WriteLine($"Subscribers (by email): {subscribers.Count}");
        foreach (Subscriber sub in subscribers)
        {
            Console.WriteLine($"  {sub}");
        }

        Console.WriteLine($"Without comparer (reference equality): {defaultEqualitySet.Count}");
        Console.WriteLine();
    }

    /*
     * SECTION 7 (demo): HashSet using type's own GetHashCode/Equals
     */
    private static void DemonstrateTypeEquality()
    {
        HashSet<SubscriberIdentity> identities = new HashSet<SubscriberIdentity>
        {
            new SubscriberIdentity("alex@example.com", "Alex"),
            new SubscriberIdentity("ALEX@EXAMPLE.COM", "Alex K."), // same email → one entry
            new SubscriberIdentity("sam@example.com", "Sam")
        };

        Console.WriteLine("--- Section 7: GetHashCode/Equals on type ---");
        Console.WriteLine($"Unique identities by email: {identities.Count}");
        foreach (SubscriberIdentity identity in identities)
        {
            Console.WriteLine($"  {identity}");
        }

        Console.WriteLine();
    }

    /*
     * =========================================================================
     * SECTION 8: SORTEDSET<T> (PREVIEW)
     * =========================================================================
     *
     * SortedSet<T> also implements ISet<T> but keeps elements in ascending sort
     * order (red-black tree). Add/Contains/Remove are O(log n) instead of O(1)
     * average. Use when you need uniqueness AND sorted iteration or range views.
     *
     * COVERED IN DETAIL LATER → 07. SortedList and SortedDictionary (ordered
     * collections module); SortedSet shares the same "sorted unique" idea.
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateSortedSetPreview()
    {
        SortedSet<string> previewCatalog = new SortedSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "zebra",
            "alpha",
            "Beta",
            "alpha" // duplicate ignored
        };

        Console.WriteLine("--- Section 8: SortedSet<T> preview ---");
        Console.WriteLine($"Sorted unique tags: [{string.Join(", ", previewCatalog)}]");
        Console.WriteLine("HashSet = fast unordered unique; SortedSet = sorted unique at O(log n).");
        Console.WriteLine();
    }

    /*
     * =========================================================================
     * SECTION 9: CHOOSING THE RIGHT COLLECTION
     * =========================================================================
     *
     *  Need                              | Prefer
     *  ----------------------------------|----------------------------------
     *  Unique items, fast lookup         | HashSet<T>
     *  Unique + sorted order             | SortedSet<T> (preview above)
     *  Ordered by index, duplicates OK   | List<T> → 03. List
     *  Key → value lookup                | Dictionary<K,V> → 04. Dictionary
     *  FIFO / LIFO                       | Queue<T> / Stack<T> → 06. Queue and Stack
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateCollectionChoice()
    {
        Console.WriteLine("--- Section 9: Summary ---");
        Console.WriteLine($"Articles share {_sharedTags.Count} tag(s): [{FormatTags(_sharedTags)}]");
        Console.WriteLine($"dotnet-only tags: [{FormatTags(_dotnetOnlyTags)}]");
        Console.WriteLine("Article tags → HashSet; browse menus needing sort → SortedSet.");
    }

    private static string FormatTags(HashSet<string> tags) => string.Join(", ", tags);
}

/*
 * =========================================================================
 * QUICK REFERENCE — HASHSET<T>
 * =========================================================================
 *
 * --- Construction ---
 *
 *   new HashSet<T>()
 *   new HashSet<T>(collection)
 *   new HashSet<T>(collection, comparer)
 *   new HashSet<T>(comparer)
 *
 * --- Core API ---
 *
 *   Add / Remove / Contains / Count / Clear
 *   CopyTo(array) / TryGetValue — N/A (no key-value; use Contains)
 *
 * --- Hash table path ---
 *
 *   GetHashCode → bucket → chain → Equals
 *   Comparer ctor: comparer's GetHashCode/Equals for all operations
 *
 * --- Non-mutating (LINQ → new sequence / wrap in HashSet) ---
 *
 *   setA.Union(setB)
 *   setA.Intersect(setB)
 *   setA.Except(setB)
 *   new HashSet<T>(setA); copy.SymmetricExceptWith(setB)
 *
 * --- Mutating (void, modifies caller) ---
 *
 *   UnionWith / IntersectWith / ExceptWith / SymmetricExceptWith
 *
 * --- Relational ---
 *
 *   IsSubsetOf / IsProperSubsetOf / IsSupersetOf / IsProperSupersetOf
 *   Overlaps / SetEquals
 *
 * --- Custom equality ---
 *
 *   IEqualityComparer<T> in constructor
 *   OR override Equals + GetHashCode (or IEquatable<T>) on element type
 *   Rule: Equal objects → same hash code
 *
 * --- SortedSet<T> (preview) ---
 *
 *   Unique + sorted; O(log n); Min, Max, GetViewBetween
 *   Full ordered collections → 07. SortedList and SortedDictionary
 *
 * --- Related chapters ---
 *
 *   List<T>              → 03. List
 *   Dictionary<K,V>      → 04. Dictionary
 *   Queue / Stack        → 06. Queue and Stack
 *   IEnumerable / LINQ   → 08. IEnumerable and IEnumerator
 *
 * --- Common mistakes ---
 *
 *  Mistake                               | Result
 *  --------------------------------------|----------------------------------
 *  Expecting insertion order in HashSet  | Order appears random
 *  Using tags[i] on HashSet              | No indexer — use Contains/foreach
 *  Custom type without comparer/override | Duplicate "equal" objects both kept
 *  GetHashCode inconsistent with Equals  | Contains/Remove break silently
 *
 * =========================================================================
 */
