# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/02. Object Oriented Programming/02. Properties & Indexers - Done`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A catalog service persists book records. A junior dev refactors `Isbn` to an auto-property "for consistency." Review the change — what breaks in production, and how should `Isbn` be implemented?

```csharp
public class Book
{
    public string CatalogId { get; }

    // Refactored from full property with validation
    public string Isbn { get; set; } = string.Empty;

    public Book(string catalogId, string title, string isbn)
    {
        CatalogId = catalogId;
        Title = title;
        Isbn = isbn;
    }

    public string Title { get; set; } = string.Empty;
}
```

```csharp
// Called from import pipeline after JSON deserialization
var book = new Book("CAT-001", "Clean Code", "978-0132350884");
book.Isbn = "   ";                    // whitespace-only "update"
await repository.SaveAsync(book);      // persists invalid ISBN
```

---

#### Q2. (R) An API team models catalog metadata with init-only properties. After code review, a developer adds a "sync" method. What is wrong, and what pattern should they use instead?

```csharp
public class CatalogEntry
{
    public string CatalogId { get; init; } = string.Empty;
    public DateTime AddedOn { get; init; }
    public string Title { get; set; } = string.Empty;
}

public class CatalogSyncService
{
    public void ApplyRemoteTimestamp(CatalogEntry entry, DateTime remoteAddedOn)
    {
        // Remote source has the authoritative AddedOn — update local copy
        entry.AddedOn = remoteAddedOn;
    }
}
```

---

#### Q3. (R) A dashboard reads `DisplayLabel` on every row render. A teammate adds "helpful" logic inside the expression-bodied getter. Review — what problems does this introduce?

```csharp
public class Book
{
    public string Title { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public int ViewCount { get; private set; }

    public string DisplayLabel
    {
        get
        {
            ViewCount++;
            LastRendered = DateTime.UtcNow;
            return $"{Title} [{Isbn}]";
        }
    }

    public DateTime LastRendered { get; private set; }
}
```

```csharp
// Grid binds to DisplayLabel — 500 rows × 3 re-renders per second
foreach (var book in books)
    row.Cells["Label"].Text = book.DisplayLabel;
```

---

#### Q4. (R) A `BookShelf` indexer passes QA with small test data, but production reports `NullReferenceException` and "empty slot" bugs. Review the indexer — what's wrong with bounds checking?

```csharp
public class BookShelf
{
    private readonly Book[] _slots;
    private int _count;

    public BookShelf(int capacity) => _slots = new Book[capacity];

    public void Add(Book book) => _slots[_count++] = book;

    public Book this[int index]
    {
        get
        {
            if (index < 0 || index >= _slots.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            return _slots[index];   // may return default/null for unused slots
        }
    }

    public int Count => _count;
}
```

```csharp
var shelf = new BookShelf(10);
shelf.Add(bookA);
shelf.Add(bookB);
var third = shelf[2];   // no exception — caller gets null
```

---

#### Q5. (R) A library module exposes the internal book list through a property so callers can "query and filter easily." Review the API surface — what can go wrong?

```csharp
public class LibrarySection
{
    private readonly List<Book> _books = new();

    public List<Book> Books => _books;

    public void AddBook(Book book) => _books.Add(book);
}
```

```csharp
var section = library.GetSection("Fiction");
section.Books.Clear();                          // bypasses AddBook / validation
section.Books.Add(new Book("", "Hack", "bad")); // no ISBN rules enforced
var snapshot = section.Books;                   // same list reference — mutates later
```

---

#### Q6. (D) You inherit a domain model mixing auto-properties, init-only metadata, expression-bodied labels, and a collection property. A PR proposes fixing all five categories above in one sprint. How do you prioritize encapsulation fixes before a catalog migration goes live?

Topics on the table: ISBN validation (Q1), init-only `AddedOn` misuse (Q2), side-effect getters (Q3), indexer bounds vs `Count` (Q4), and returning `IReadOnlyList<Book>` vs `List<Book>` (Q5). What do you fix first, what can wait, and why?

---
