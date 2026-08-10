/*
 * TOPIC: Encapsulated data access through properties; indexer syntax for
 *        indexed objects; overriding ToString, Equals, and GetHashCode.
 *
 * WHY IT MATTERS:
 *   Public fields expose implementation and cannot validate input. Properties
 *   are the C# idiom for controlled read/write access — auto-properties for
 *   simple data, full properties when rules apply. Indexers make custom
 *   collections feel like arrays or dictionaries. Correct Equals/GetHashCode
 *   keep collections and comparisons predictable.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Properties vs fields — overview
 *   2.  Auto-implemented properties
 *   3.  Full properties (get / set / backing field)
 *   4.  Read-only, init-only, and private-set properties
 *   5.  Expression-bodied properties
 *   6.  Indexer syntax — this[int] and overloads
 *   7.  Indexer real-world example (BookShelf)
 *   8.  Preview: const vs property
 *   9.  Override ToString, Equals, and GetHashCode
 *  10.  Preview: records (C# 9+)
 */

using System;
using System.Collections.Generic;

namespace PropertiesAndIndexers;

/*
 * SECTION 1: PROPERTIES — OVERVIEW
 *
 * A PROPERTY looks like a field from the caller's perspective (dot syntax)
 * but is implemented with get and set accessors. Fields hold raw data; properties
 * control how data is read and written.
 *
 *   Book book = new Book("CAT-001", "Clean Code", "978-0132350884");
 *   book.Title = "Clean Code, 2nd Ed.";   // property assignment
 *   string label = book.DisplayLabel;      // property read
 *
 * Common shapes you will see in this chapter:
 *
 *   Shape                         | Syntax sketch
 *   ------------------------------|------------------------------------------
 *   Auto-property                 | public string Title { get; set; }
 *   Full property                 | private field + get/set with logic
 *   Read-only                     | public string Id { get; }
 *   Init-only (C# 9+)             | public DateTime AddedOn { get; init; }
 *   Private set                   | public string Status { get; private set; }
 *   Expression-bodied (computed)  | public string Label => $"{Title}";
 *
 * Scenario for the demo: a small library catalog — books on a shelf, looked
 * up by slot or ISBN. Values are fixed so the program runs without input.
 */

/*
 * SECTION 2: THE Book TYPE — PROPERTY PATTERNS
 *
 * Book demonstrates every major property form in one cohesive domain type.
 *
 * --- 2a. Auto-implemented properties ---
 *
 * The compiler generates a hidden backing field. Use when get and set need
 * no extra logic:
 *
 *   public string Title { get; set; }
 *
 * Optional initializer at declaration: = string.Empty avoids null before assignment.
 *
 * --- 2b. Full properties (get / set / backing field) ---
 *
 * When validation, trimming, or side effects are required, declare a private
 * backing field and implement get/set explicitly:
 *
 *   private string _isbn;
 *   public string Isbn
 *   {
 *       get => _isbn;
 *       set
 *       {
 *           if (string.IsNullOrWhiteSpace(value)) throw ...
 *           _isbn = value.Trim();
 *       }
 *   }
 *
 * Invalid assignment through the setter throws ArgumentException — the backing
 * field is never updated.
 *
 * --- 2c. Read-only properties ---
 *
 * get accessor only; value fixed at construction:
 *
 *   public string CatalogId { get; }
 *
 * Set in the constructor body — no setter means callers cannot reassign after
 * the object exists.
 *
 * --- 2d. Init-only properties (C# 9+) ---
 *
 *   public DateTime AddedOn { get; init; }
 *
 * init allows assignment in the constructor or object initializer, then locks
 * the property — stricter than { get; set; } for create-once data.
 *
 * --- 2e. Private-set properties ---
 *
 *   public string LastUpdatedBy { get; private set; }
 *
 * External callers read the value; only methods inside the class may assign.
 * Useful for status fields that change through controlled operations.
 *
 * --- 2f. Expression-bodied properties ---
 *
 * Shorthand for read-only computed values with no extra storage:
 *
 *   public string DisplayLabel => $"{Title} [{Isbn}]";
 *
 * Equivalent to: public string DisplayLabel { get { return ...; } }
 *
 * --- 2g. Override ToString ---
 *
 * Object.ToString() default returns the type name ("Book"). Override for
 * logging, debugging, and Console.WriteLine output.
 *
 * --- 2h. Override Equals and GetHashCode ---
 *
 * Reference equality (== on classes) compares memory addresses. Value-style
 * equality compares meaningful fields (here: CatalogId).
 *
 * Rules:
 *   • Override both Equals and GetHashCode together.
 *   • If a.Equals(b), then a.GetHashCode() == b.GetHashCode().
 *   • Use the same fields in both methods.
 *
 * Required for Dictionary<TKey, TValue> and HashSet<T> when T is your type.
 */
public class Book
{
    public string Title { get; set; } = string.Empty;

    private string _isbn = string.Empty;

    public string Isbn
    {
        get => _isbn;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("ISBN is required.", nameof(value));
            }

            _isbn = value.Trim();
        }
    }

    public string CatalogId { get; }

    public DateTime AddedOn { get; init; }

    public string LastUpdatedBy { get; private set; } = "system";

    private int _pageCount;

    public int PageCount
    {
        get => _pageCount;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Page count must be positive.");
            }

            _pageCount = value;
        }
    }

    public string DisplayLabel => $"{Title} [{Isbn}]";

    public Book(string catalogId, string title, string isbn)
    {
        CatalogId = catalogId;
        Title = title;
        Isbn = isbn;
    }

    public void Rename(string newTitle, string updatedBy)
    {
        Title = newTitle;
        LastUpdatedBy = updatedBy;
    }

    public override string ToString() => DisplayLabel;

    public override bool Equals(object? obj)
    {
        if (obj is not Book other)
        {
            return false;
        }

        return string.Equals(CatalogId, other.CatalogId, StringComparison.Ordinal);
    }

    public override int GetHashCode() => CatalogId.GetHashCode(StringComparison.Ordinal);
}

/*
 * SECTION 3: INDEXERS — SYNTAX AND OVERLOADING
 *
 * An INDEXER lets instances use bracket notation like arrays or dictionaries:
 *
 *   public Book this[int index]
 *   {
 *       get { ... return _slots[index]; }
 *   }
 *
 * Usage:  Book first = shelf[0];
 *
 * Indexers can overload on parameter type (int vs string). They are instance
 * members — they operate on object state, not static state.
 *
 * --- 3a. Integer indexer (positional access) ---
 *
 * shelf[0], shelf[1], … by slot. This shelf is append-only, so only get is
 * exposed on the int indexer.
 *
 * Out-of-range index throws ArgumentOutOfRangeException.
 *
 * --- 3b. String indexer (keyed lookup) ---
 *
 * shelf["978-0132350884"] by ISBN. Returns null when no match — a common
 * pattern for try-style lookups without exceptions.
 *
 * --- 3c. Expression-bodied properties on BookShelf ---
 *
 * Count and Capacity are read-only computed values over internal arrays,
 * using the same => syntax as Book.DisplayLabel.
 *
 * Real-world parallels: grid[row, col], cache[key], or any custom collection
 * that hides internal storage behind bracket syntax.
 */
public class BookShelf
{
    private readonly Book[] _slots;
    private int _count;

    public int Capacity => _slots.Length;

    public int Count => _count;

    public BookShelf(int capacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be positive.");
        }

        _slots = new Book[capacity];
    }

    public void Add(Book book)
    {
        if (book is null)
        {
            throw new ArgumentNullException(nameof(book));
        }

        if (_count >= _slots.Length)
        {
            throw new InvalidOperationException("Shelf is full.");
        }

        _slots[_count] = book;
        _count++;
    }

    public Book this[int index]
    {
        get
        {
            if (index < 0 || index >= _count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return _slots[index];
        }
    }

    public Book? this[string isbn]
    {
        get
        {
            if (string.IsNullOrWhiteSpace(isbn))
            {
                return null;
            }

            string trimmed = isbn.Trim();

            for (int i = 0; i < _count; i++)
            {
                if (string.Equals(_slots[i].Isbn, trimmed, StringComparison.Ordinal))
                {
                    return _slots[i];
                }
            }

            return null;
        }
    }
}

/*
 * SECTION 4: RECORDS (C# 9+) — PREVIEW
 *
 * COVERED IN DETAIL LATER → optional dedicated Records topic
 *   (headline concepts only: positional syntax, value equality by default)
 *
 *   public record BookSummary(string Title, string Isbn);
 *
 * Records synthesize ToString, Equals, and GetHashCode for positional
 * members. Book uses manual overrides for teaching; records reduce boilerplate.
 */
public record BookSummary(string Title, string Isbn);

public class Program
{
    /*
     * SECTION 5: LIBRARY CATALOG DEMONSTRATION
     *
     * Main wires the chapter demo — create objects, exercise each property
     * pattern and indexer, then print results. Concept explanations live above
     * the types they describe; this method orchestrates the runnable example.
     */
    public static void Main(string[] args)
    {
        DateTime catalogDate = new DateTime(2024, 6, 12);

        Book cleanCode = new Book("CAT-001", "Clean Code", "978-0132350884")
        {
            AddedOn = catalogDate,
            PageCount = 464
        };

        cleanCode.Title = "Clean Code (Updated Edition)";
        string titleAfterUpdate = cleanCode.Title;

        string validIsbn = cleanCode.Isbn;
        string invalidIsbnMessage = string.Empty;

        try
        {
            cleanCode.Isbn = "   ";
        }
        catch (ArgumentException ex)
        {
            invalidIsbnMessage = ex.Message;
        }

        string catalogId = cleanCode.CatalogId;
        DateTime addedOn = cleanCode.AddedOn;

        cleanCode.Rename("Clean Code (Library Copy)", "librarian");
        string updatedBy = cleanCode.LastUpdatedBy;

        string displayLabel = cleanCode.DisplayLabel;

        Book refactoring = new Book("CAT-002", "Refactoring", "978-0201485677")
        {
            AddedOn = catalogDate,
            PageCount = 431
        };

        Book pragmatic = new Book("CAT-003", "The Pragmatic Programmer", "978-0135957059")
        {
            AddedOn = catalogDate,
            PageCount = 352
        };

        BookShelf shelf = new BookShelf(capacity: 4);
        shelf.Add(cleanCode);
        shelf.Add(refactoring);
        shelf.Add(pragmatic);

        Book bookAtSlotZero = shelf[0];
        Book bookAtSlotTwo = shelf[2];
        string outOfRangeMessage = string.Empty;

        try
        {
            _ = shelf[99];
        }
        catch (ArgumentOutOfRangeException ex)
        {
            outOfRangeMessage = ex.Message;
        }

        Book? foundByIsbn = shelf["978-0132350884"];
        Book? missingByIsbn = shelf["000-0000000000"];
        int shelfCount = shelf.Count;
        int shelfCapacity = shelf.Capacity;

        /*
         * --- 5a. const vs property (PREVIEW) ---
         *
         * COVERED IN DETAIL LATER → 01. C# Language Fundamentals (const, readonly)
         *
         * | Member kind    | Scope    | When set       | Typical use           |
         * |----------------|----------|----------------|-----------------------|
         * | const          | static   | compile time   | MaxItems = 100        |
         * | readonly field | instance | ctor / decl    | _id assigned once     |
         * | property       | instance | get/set rules  | Title, Isbn with logic|
         *
         * const cannot be a property — it is always static and fixed at compile time.
         */
        const int MaxShelfSlots = 4;
        bool shelfWithinConstLimit = shelf.Count <= MaxShelfSlots;

        string cleanCodeToString = cleanCode.ToString();
        string defaultObjectLabel = new object().ToString() ?? string.Empty;

        Book sameCatalogDifferentTitle = new Book("CAT-001", "Duplicate Entry", "978-0000000000")
        {
            AddedOn = catalogDate,
            PageCount = 1
        };

        Book differentCatalog = new Book("CAT-999", "Other Book", "978-9999999999")
        {
            AddedOn = catalogDate,
            PageCount = 100
        };

        bool equalByCatalogId = cleanCode.Equals(sameCatalogDifferentTitle);
        bool notEqualDifferentId = cleanCode.Equals(differentCatalog);
        bool referenceEquals = ReferenceEquals(cleanCode, sameCatalogDifferentTitle);
        bool hashCodesMatchForEqualBooks =
            cleanCode.GetHashCode() == sameCatalogDifferentTitle.GetHashCode();

        Dictionary<Book, string> locationByBook = new Dictionary<Book, string>
        {
            [cleanCode] = "Aisle 3, Shelf B",
            [refactoring] = "Aisle 3, Shelf C"
        };

        string locationForDuplicateKey = locationByBook[sameCatalogDifferentTitle];

        BookSummary summary = new BookSummary(cleanCode.Title, cleanCode.Isbn);
        BookSummary sameSummary = new BookSummary(cleanCode.Title, cleanCode.Isbn);
        bool recordValueEquality = summary.Equals(sameSummary);
        string recordToString = summary.ToString();

        Console.WriteLine("=== Properties and Indexers — Library Catalog ===");
        Console.WriteLine();
        Console.WriteLine($"Auto-property Title: {titleAfterUpdate}");
        Console.WriteLine($"Full property Isbn (valid): {validIsbn}");
        Console.WriteLine($"Full property Isbn (invalid): {invalidIsbnMessage}");
        Console.WriteLine();
        Console.WriteLine($"Read-only CatalogId: {catalogId} | Init-only AddedOn: {addedOn:yyyy-MM-dd}");
        Console.WriteLine($"Private-set LastUpdatedBy after Rename: {updatedBy}");
        Console.WriteLine($"Expression-bodied DisplayLabel: {displayLabel}");
        Console.WriteLine();
        Console.WriteLine($"Indexer shelf[0]: {bookAtSlotZero}");
        Console.WriteLine($"Indexer shelf[2]: {bookAtSlotTwo}");
        Console.WriteLine($"Indexer out of range: {outOfRangeMessage}");
        Console.WriteLine();
        Console.WriteLine($"ISBN lookup found: {foundByIsbn}");
        Console.WriteLine($"ISBN lookup missing: {(missingByIsbn is null ? "(null)" : missingByIsbn.ToString())}");
        Console.WriteLine($"Shelf count/capacity: {shelfCount}/{shelfCapacity}");
        Console.WriteLine();
        Console.WriteLine($"const MaxShelfSlots={MaxShelfSlots}, within limit: {shelfWithinConstLimit}");
        Console.WriteLine();
        Console.WriteLine($"ToString Book: {cleanCodeToString}");
        Console.WriteLine($"ToString object default: {defaultObjectLabel}");
        Console.WriteLine();
        Console.WriteLine($"Equals same CatalogId: {equalByCatalogId} | different id: {notEqualDifferentId}");
        Console.WriteLine($"ReferenceEquals (same id, different object): {referenceEquals}");
        Console.WriteLine($"GetHashCode match for equal books: {hashCodesMatchForEqualBooks}");
        Console.WriteLine($"Dictionary lookup by equal book: {locationForDuplicateKey}");
        Console.WriteLine();
        Console.WriteLine($"Record preview — value equality: {recordValueEquality}");
        Console.WriteLine($"Record preview — ToString: {recordToString}");
        Console.WriteLine();
        Console.WriteLine("All books on shelf:");

        for (int i = 0; i < shelf.Count; i++)
        {
            Console.WriteLine($"  [{i}] {shelf[i]}");
        }
    }
}

/*
 * QUICK REFERENCE — PROPERTIES AND INDEXERS
 *
 * --- Auto-property ---
 *
 *  public string Name { get; set; }
 *  public string Name { get; set; } = string.Empty;
 *
 * --- Full property ---
 *
 *  private string _name;
 *  public string Name
 *  {
 *      get => _name;
 *      set { _name = value ?? throw new ArgumentNullException(nameof(value)); }
 *  }
 *
 * --- Read-only / init / private set ---
 *
 *  public string Id { get; }                    // set in ctor only
 *  public DateTime Created { get; init; }       // ctor or object initializer
 *  public string Status { get; private set; }  // set only inside class
 *
 * --- Expression-bodied ---
 *
 *  public string Label => $"{First} {Last}";
 *  public int Count => _items.Length;
 *
 * --- Indexer ---
 *
 *  public T this[int i] { get { ... } set { ... } }
 *  public T? this[string key] { get { ... } }
 *
 * --- Overrides (use together) ---
 *
 *  public override string ToString() => "...";
 *  public override bool Equals(object? obj) => ...;
 *  public override int GetHashCode() => ...;
 *
 * --- Preview: const vs property ---
 *
 *  const int Max = 100;        // compile-time static constant
 *  public int Count { get; }   // per-instance, set with logic
 *
 * --- Preview: record ---
 *
 *  public record Point(int X, int Y);   // synthesized equality + ToString
 *
 * --- Tips ---
 *
 *  Pattern                          | When
 *  ---------------------------------|----------------------------------
 *  Auto-property                    | Simple pass-through storage
 *  Full property                    | Validation / lazy load / side effects
 *  get; init;                       | Immutable after construction
 *  get; private set;                | Public read, controlled internal write
 *  Expression-bodied =>             | Read-only computed values
 *  Indexer overloads                | Positional + keyed access on one type
 *  Override Equals + GetHashCode    | Dictionary/HashSet keys, value equality
 */
