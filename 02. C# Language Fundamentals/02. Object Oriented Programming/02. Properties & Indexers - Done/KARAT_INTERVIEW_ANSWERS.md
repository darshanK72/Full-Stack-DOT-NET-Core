# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/02. Object Oriented Programming/02. Properties & Indexers - Done`

---

#### Q1. (R) A catalog service persists book records. A junior dev refactors `Isbn` to an auto-property "for consistency." Review the change — what breaks in production, and how should `Isbn` be implemented?

**Answer:** Auto-implemented properties cannot enforce invariants — whitespace-only or untrimmed ISBNs pass straight through to persistence, corrupting catalog data and breaking keyed lookups that assume normalized values.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | No validation on `Isbn` setter | `"   "` and empty strings persist; ISBN indexers return inconsistent results |
| Data integrity | No trimming/normalization | `" 978-0132350884 "` and `"978-0132350884"` may be treated as different keys |
| Design | Validation moved out of the type | Import pipeline, API controllers, and EF must duplicate rules — easy to miss one path |
| Encapsulation | Public `{ get; set; }` on invariant field | Any caller can bypass domain rules the chapter's `Book.Isbn` full property was meant to centralize |

**Fix (priority order):**

1. Restore a **full property** with a private backing field — validate in the setter (reject null/whitespace, trim before store), matching this chapter's `Book.Isbn` pattern.
2. Keep constructor assignment routed through the setter (`Isbn = isbn;`) so construction and later updates share one code path.
3. Add unit tests for invalid ISBN assignment (`ArgumentException`) and trim behavior.
4. Leave simple pass-through data (e.g., `Title`) as auto-properties — apply full properties only where invariants exist.

```csharp
private string _isbn = string.Empty;

public string Isbn
{
    get => _isbn;
    set
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("ISBN is required.", nameof(value));
        _isbn = value.Trim();
    }
}
```

**Production takeaway:** Auto-properties are for dumb data; the moment a field has validation, normalization, or authorization, use a backing field. See **Program.cs** Section 2b — full property on `Isbn`.

---

#### Q2. (R) An API team models catalog metadata with init-only properties. After code review, a developer adds a "sync" method. What is wrong, and what pattern should they use instead?

**Answer:** `init` accessors only allow assignment during object construction or an object initializer — assigning `AddedOn` after the object exists is a compile-time error (`CS8852`), and that restriction is intentional for create-once metadata.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `entry.AddedOn = remoteAddedOn` outside init context | Build fails — method as written cannot ship |
| Design | Treating init-only props like mutable `{ get; set; }` | Confusion about which fields are immutable audit metadata vs editable display data |
| Correctness (if forced) | Reflection or serialization tricks to mutate init props | Breaks immutability guarantees; audit trail timestamps become untrustworthy |
| API contract | Mixed mutability on one DTO | Callers cannot tell `AddedOn` is fixed at creation without reading every accessor |

**Fix (priority order):**

1. **Do not** mutate init-only properties after construction — if remote sync needs a new timestamp, create a **new** `CatalogEntry` (record/copy pattern) or use a dedicated mutable field (`LastSyncedOn { get; private set; }`) for operational updates.
2. Keep true creation metadata (`AddedOn`, `CatalogId`) as `{ get; init; }` or `{ get; }` set only in the constructor.
3. Use `{ get; set; }` only for fields that legitimately change (`Title`, status flags).
4. For EF/API deserialization that must hydrate init props, rely on constructor + init in one creation flow — not post-hoc setter methods.

```csharp
public CatalogEntry WithAddedOn(DateTime addedOn) =>
    new() { CatalogId = CatalogId, Title = Title, AddedOn = addedOn };
```

**Production takeaway:** `init` is stricter than `{ get; set; }` for "set once at birth" data — production models should separate immutable audit fields from mutable operational fields. See **Program.cs** Section 2d — `AddedOn { get; init; }`.

---

#### Q3. (R) A dashboard reads `DisplayLabel` on every row render. A teammate adds "helpful" logic inside the expression-bodied getter. Review — what problems does this introduce?

**Answer:** Expression-bodied and block-bodied getters must be **pure reads** — incrementing `ViewCount` and updating `LastRendered` on every property access turns an innocent label lookup into hidden mutation that breaks caching, threading, and test expectations.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Getter mutates object state | `ViewCount` grows on every UI re-bind, not on actual user views — metrics lie |
| Performance | Side effects on hot path (500 rows × 3/sec) | Unnecessary writes; defeats memoization; harder to optimize |
| Surprise / API | Property looks like a field read | Callers expect idempotent `book.DisplayLabel` — logging/analytics code may read it in loops |
| Threading | Non-atomic read + two writes in getter | Concurrent grid refresh can race on `ViewCount` / `LastRendered` without locks |
| Testing | Asserting label text changes internal counters | Tests become order-dependent; "read property" tests mutate state |

**Fix (priority order):**

1. Make `DisplayLabel` a **pure computed property**: `public string DisplayLabel => $"{Title} [{Isbn}]";` — no storage writes in the getter.
2. Move view tracking to an explicit method: `RecordView()` or an application/analytics service called once per actual view event.
3. If expensive formatting is needed, use explicit caching with a known invalidation point (when `Title`/`Isbn` change), not on every get.
4. Code-review rule: **getters do not have side effects** — same input state, same output, no hidden I/O.

**Production takeaway:** Expression-bodied `=>` properties are syntactic sugar for `get` only — they do not imply "cheap," but they must not mutate. Side effects belong in methods or event handlers. See **Program.cs** Section 2f — `DisplayLabel` as read-only computed value.

---

#### Q4. (R) A `BookShelf` indexer passes QA with small test data, but production reports `NullReferenceException` and "empty slot" bugs. Review the indexer — what's wrong with bounds checking?

**Answer:** The indexer validates against `_slots.Length` (capacity) instead of `_count` (occupied slots), so indices in the "empty tail" of the array are legal but return `null` — callers expecting a `Book` hit `NullReferenceException` downstream.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Bounds check uses `Capacity`, not `Count` | `shelf[2]` succeeds after two adds — returns `default(Book)` (null reference) |
| API contract | Indexer implies "slot i of books on shelf" | Callers cannot distinguish "out of range" from "empty reserved slot" |
| Consistency | `Add` stops at capacity; indexer allows reading unused indices | Off-by-one between logical collection size and array size |
| Defensive coding | Downstream null dereference instead of clear exception | Harder to diagnose in prod logs than `ArgumentOutOfRangeException` |

**Fix (priority order):**

1. Bound against **`_count`**, not `_slots.Length`: `if (index < 0 || index >= _count) throw new ArgumentOutOfRangeException(nameof(index));`
2. Match this chapter's `BookShelf` int indexer — positional access only over populated slots.
3. If "raw array slot" access is needed internally, keep it private; public indexer represents logical contents.
4. Add tests: after `Add` twice, index `2` must throw; index `0` and `1` return books.

```csharp
public Book this[int index]
{
    get
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));
        return _slots[index];
    }
}
```

**Production takeaway:** Indexers should enforce the same logical bounds as `Count` — array capacity is an implementation detail. See **Program.cs** Section 3a — int indexer checks `index >= _count`.

---

#### Q5. (R) A library module exposes the internal book list through a property so callers can "query and filter easily." Review the API surface — what can go wrong?

**Answer:** Returning the live `List<Book>` breaks encapsulation — callers can clear, reorder, or inject invalid books without going through `AddBook`, and holding a reference to `Books` sees every later internal mutation.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Encapsulation | Exposes mutable `_books` reference | `section.Books.Clear()` empties internal state without validation or events |
| Invariant bypass | Direct `Add` skips ISBN/page-count rules on `Book` | Invalid domain objects enter the collection |
| Aliasing | `snapshot = section.Books` shares reference | Code thinks it captured a point-in-time list; later adds/removes corrupt the "snapshot" |
| Evolution | Cannot swap backing store (array, immutable list) later | Public API locked to `List<Book>` forever |
| Thread safety | Unsynchronized shared list | Concurrent read during internal modification → `InvalidOperationException` or torn state |

**Fix (priority order):**

1. Expose **`IReadOnlyList<Book>`** (or `IEnumerable<Book>`) via a defensive copy or read-only wrapper: `public IReadOnlyList<Book> Books => _books.AsReadOnly();` or `return _books.ToList()` when callers need isolation.
2. Keep all mutations through controlled methods: `AddBook`, `RemoveBook`, `ClearSection` — enforce validation and raise change notifications if needed.
3. For LINQ-friendly querying without mutation, expose `Books.AsReadOnly()` or methods like `FindByIsbn(string)`.
4. Never return `List<T>` from a public property unless the type is explicitly a builder/mutable DTO documented as such.

```csharp
public IReadOnlyList<Book> Books => _books.AsReadOnly();
```

**Production takeaway:** Properties that expose collections should expose **views or copies**, not the backing collection — same principle as `BookShelf` hiding `_slots` behind indexers and `Count`. See foundation encapsulation — prefer controlled access over public fields/lists.

---

#### Q6. (D) You inherit a domain model mixing auto-properties, init-only metadata, expression-bodied labels, and a collection property. A PR proposes fixing all five categories above in one sprint. How do you prioritize encapsulation fixes before a catalog migration goes live?

**Answer:** Fix **data-corruption and silent-failure paths first** (ISBN validation, indexer bounds, mutable collection exposure), then **compile/design violations** (init misuse), then **observability/side-effect getters** — ship validation and bounds before migration writes bad rows into the new store.

**Priority order:**

| Priority | Fix | Why first / can wait |
|---|---|---|
| **P0 — before migration** | ISBN full property (Q1) | Invalid keys written during import are expensive to backfill; breaks ISBN indexer lookups immediately |
| **P0 — before migration** | Indexer bounds vs `_count` (Q4) | Silent nulls cause NREs in batch jobs — migration scripts often iterate by index |
| **P0 — before migration** | Stop exposing `List<Book>` (Q5) | Prevents callers from corrupting in-memory catalog during parallel migration tooling |
| **P1 — same release** | Init-only discipline (Q2) | Compile blocker if present; clarify immutable audit fields before API publishes contracts |
| **P2 — next iteration** | Pure getters / remove side effects (Q3) | Wrong metrics and perf, but rarely corrupts persisted data; fix before enabling analytics dashboards |
| **P3 — hardening** | Tests + API review checklist | Property validation tests, indexer edge cases, read-only collection contract tests |

**Trade-offs:**

- A big-bang refactor delays migration — **surgical P0 fixes** on hot types (`Book`, `BookShelf`, `LibrarySection`) unblock data move with minimal surface change.
- Auto-properties on low-risk display fields (`Title`) can stay — don't gold-plate every property in the same PR.
- Document team rules: invariants → full property; create-once → `init`; computed → pure getter; collections → `IReadOnlyList` or indexer.

**Production takeaway:** Encapsulation fixes rank by **what bad data or silent nulls cost in production**, not by line count — Karat tests prioritization, not just pattern recognition. Aligns with **Program.cs** property/indexer patterns in Sections 2–3.

---
