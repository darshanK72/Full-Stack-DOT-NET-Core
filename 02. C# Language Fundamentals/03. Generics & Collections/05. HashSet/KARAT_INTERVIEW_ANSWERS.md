# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/03. Generics & Collections/05. HashSet`

---

#### Q1. (R) A nightly tag-import job deduplicates article tags with `List<string>.Contains` before insert. Review the hot path:

**Answer:** `List<T>.Contains` is **O(n)** per call, so importing *m* tags against *n* existing tags approaches **O(n × m)** — fine for unit tests with ten tags, catastrophic at 80k × 200k. Replace the backing store with `HashSet<string>` and the same `StringComparer` so `Add` and `Contains` are **O(1)** average.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance | Linear scan on every `Contains` | Import SLA missed; CPU spikes on large catalogs |
| Scalability | List grows; each check walks all elements | Cost compounds as `_knownTags` grows through the run |
| Collection choice | List chosen for uniqueness | Wrong tool — HashSet exists for exactly this pattern |

**Fix (priority order):**

1. Use `HashSet<string>` with `StringComparer.OrdinalIgnoreCase` as the backing store.
2. Collapse register to a single `Add` — it returns `false` when the tag is already present.

```csharp
private readonly HashSet<string> _knownTags =
    new(StringComparer.OrdinalIgnoreCase);

public bool TryRegisterTag(string tag) => _knownTags.Add(tag);
```

**Production takeaway:** Karat pairs "works in tests" with hidden **O(n²)** membership — see **Program.cs** Section 9 (HashSet vs List). Always ask lookup frequency and collection size, not just correctness on small data.

---

#### Q2. (R) A newsletter service deduplicates subscribers by email but keeps seeing duplicate sends in logs. Review:

**Answer:** `HashSet<Subscriber>` without a custom comparer uses **reference equality** for class types — two distinct `Subscriber` objects with the same email are different elements. Pass `SubscriberByEmailComparer` (or override `Equals`/`GetHashCode` on the type) so business identity drives uniqueness.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Default reference equality on reference type | Duplicate emails stored; duplicate emails sent |
| API misuse | `HashSet` assumed to compare by field values | Silent data-quality bug — `Add` returns `true` twice |
| Design | Equality rule not wired into collection | `Contains`/`Remove` also fail to find "same" subscriber |

**Fix (priority order):**

1. Construct with the chapter comparer: `new HashSet<Subscriber>(new SubscriberByEmailComparer())`.
2. Alternatively, use `SubscriberIdentity`-style immutable type with `IEquatable<T>` + consistent `GetHashCode` on `Email`.

```csharp
private readonly HashSet<Subscriber> _subscribers =
    new(new SubscriberByEmailComparer());
```

**Production takeaway:** Custom types in HashSet/Dictionary **never** dedupe by field values unless you supply equality — see **Program.cs** Section 6 vs Section 7.

---

#### Q3. (R) After a profile-update feature ships, support reports "user already subscribed" errors even when lookup fails. Review:

**Answer:** `GetHashCode` was computed from `Email` at `Add` time and placed the object in a bucket keyed to the old hash. Mutating `Email` afterward leaves the object in the **wrong bucket**, so `Contains` returns `false` even though the instance is still in the set — classic broken hash contract with mutable keys.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Mutable field participates in `GetHashCode` | Lookup/remove fail after in-place edit |
| Hash contract | Hash at insert ≠ hash at lookup | Element orphaned inside set — `Count` includes it but `Contains` misses |
| Design | Writable `Email` on set member type | Same rule as Dictionary keys — must be immutable for hashed collections |

**Fix (priority order):**

1. Make identity fields immutable (`init` or constructor-only), matching `SubscriberIdentity` in **Program.cs** Section 7.
2. If email must change, **remove** old identity from the set and **add** a new object (or rebuild the set).
3. Never mutate fields that feed `Equals`/`GetHashCode` while the instance lives inside a `HashSet` or `Dictionary`.

```csharp
public sealed class SubscriberProfile
{
    public string Email { get; }
    public string Name { get; set; }

    public SubscriberProfile(string email, string name)
    {
        Email = email;
        Name = name;
    }
    // Equals/GetHashCode on Email only
}
```

**Production takeaway:** Karat tests whether you treat HashSet elements like **Dictionary keys** — mutable hash inputs cause silent lookup failures, not exceptions.

---

#### Q4. (R) An editorial dashboard merges article tag sets for a "shared topics" widget. Case variants appear twice after deploy. Review:

**Answer:** `HashSet<T>.Union` as a LINQ extension on `IEnumerable<T>` uses **default sequence equality** (`EqualityComparer<string>.Default` → **Ordinal**, case-sensitive), **not** the HashSet's internal `StringComparer.OrdinalIgnoreCase`. Re-wrapping in `new HashSet<string>(allTopics)` without a comparer keeps Ordinal semantics, so `"csharp"` and `"CSharp"` coexist as distinct entries.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | LINQ set ops ignore HashSet's comparer | Logical duplicates in UI and analytics |
| API confusion | `Union` on HashSet still calls `Enumerable.Union` | Developer's comparer choice on construction does not flow to LINQ |
| Data quality | Default `HashSet` ctor uses Ordinal | Case variants inflate counts and break deduped filters |

**Fix (priority order):**

1. Pass the same comparer when materializing: `new HashSet<string>(dotnetTags.Union(linqTags), StringComparer.OrdinalIgnoreCase)`.
2. Or use mutating `UnionWith` on a **copy** if you need set instance semantics with the existing comparer.
3. For intersection-only widgets, same rule: `new HashSet<string>(a.Intersect(b), comparer)`.

```csharp
var widgetTags = new HashSet<string>(
    dotnetTags.Union(linqTags),
    StringComparer.OrdinalIgnoreCase);
```

**Production takeaway:** LINQ `Union`/`Intersect`/`Except` are **comparer-agnostic** — see **Program.cs** Section 3. Always thread `IEqualityComparer<T>` through the final `HashSet` constructor.

---

#### Q5. (R) A publish pipeline accidentally wipes an editor's working tag pool. Review the merge step:

**Answer:** `IntersectWith` **mutates the caller** (`editorPool`) in place, keeping only elements also in `draftTags`. The developer needed a **non-mutating** preview — the chapter's LINQ `Intersect` (Section 3) or a copy-then-`IntersectWith` pattern (Section 4).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `IntersectWith` vs intended read-only preview | `"csharp"` and `"security"` permanently removed from working pool |
| API misuse | Confused mutating (`*With`) vs LINQ extension methods | Downstream `UnionWith` cannot restore deleted tags |
| Operational | Shared `editorPool` referenced elsewhere | Other features see truncated set — data loss in session state |

**Fix (priority order):**

1. Non-mutating LINQ: `var preview = new HashSet<string>(editorPool.Intersect(draftTags), StringComparer.OrdinalIgnoreCase);`
2. Or copy first: `var preview = new HashSet<string>(editorPool, comparer); preview.IntersectWith(draftTags);`
3. Reserve `IntersectWith` for intentional in-place filtering when building a working set incrementally.

```csharp
var preview = new HashSet<string>(
    editorPool.Intersect(draftTags),
    StringComparer.OrdinalIgnoreCase);
// editorPool unchanged: csharp, dotnet, security
```

**Production takeaway:** `*With` methods return `void` and modify **this** — Karat loves swapping them with LINQ equivalents. Read method names literally before calling on shared state.

---

#### Q6. (R) A custom comparer passes code review but `Remove` and `Contains` behave inconsistently. Review:

**Answer:** `Equals` compares **Email** but `GetHashCode` hashes **Name** — violating the rule that equal objects must share the same hash code. The second subscriber lands in a different bucket, so `Contains`/`Remove` miss while duplicate `Add` behavior looks arbitrary.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `GetHashCode`/`Equals` inconsistency | Silent failures — worst kind of collection bug |
| Hash contract | Equal-by-email objects can differ by hash | `Remove` returns false for objects that "should" match |
| Code review | Comparer named `ByName` but equals on Email | Copy-paste defect easy to miss without contract tests |

**Fix (priority order):**

1. Derive hash from the **same fields** used in `Equals` — here, email case-insensitively.
2. Add unit tests: if `Equals(a,b)` then `GetHashCode(a) == GetHashCode(b)`; round-trip `Add`/`Contains`/`Remove`.

```csharp
public int GetHashCode(Subscriber obj) =>
    StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Email);
```

**Production takeaway:** HashSet and Dictionary failures from bad comparers **do not throw** — they return wrong `bool` results. Same contract as **Program.cs** Section 6 quick reference: *Equal objects → same hash code*.
