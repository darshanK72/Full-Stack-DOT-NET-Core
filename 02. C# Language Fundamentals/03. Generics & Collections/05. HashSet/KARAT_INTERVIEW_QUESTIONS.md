# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/03. Generics & Collections/05. HashSet`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A nightly tag-import job deduplicates article tags with `List<string>.Contains` before insert. Review the hot path:

```csharp
public sealed class TagImportService
{
    private readonly List<string> _knownTags = new();

    public bool TryRegisterTag(string tag)
    {
        if (_knownTags.Contains(tag, StringComparer.OrdinalIgnoreCase))
            return false;

        _knownTags.Add(tag);
        return true;
    }
}

// Startup loads 80_000 existing tags, then imports 200_000 candidate tags one-by-one.
```

The job passes unit tests (10 tags) but misses its SLA in staging. What is wrong with this design, and what collection change fixes average lookup cost?

---

#### Q2. (R) A newsletter service deduplicates subscribers by email but keeps seeing duplicate sends in logs. Review:

```csharp
public sealed class NewsletterService
{
    private readonly HashSet<Subscriber> _subscribers = new();

    public bool AddSubscriber(Subscriber sub) => _subscribers.Add(sub);

    public bool IsSubscribed(Subscriber sub) => _subscribers.Contains(sub);
}

// Two calls with different Subscriber instances, same email:
AddSubscriber(new Subscriber("Alex", "alex@example.com"));   // true
AddSubscriber(new Subscriber("Alex K.", "alex@example.com")); // true — unexpected
```

What breaks uniqueness here, and how do you align with the `SubscriberByEmailComparer` pattern from this chapter?

---

#### Q3. (R) After a profile-update feature ships, support reports "user already subscribed" errors even when lookup fails. Review:

```csharp
public class SubscriberProfile
{
    public string Email { get; set; }  // mutable — used in GetHashCode/Equals
    public string Name { get; set; }

    public override bool Equals(object? obj) =>
        obj is SubscriberProfile other &&
        string.Equals(Email, other.Email, StringComparison.OrdinalIgnoreCase);

    public override int GetHashCode() =>
        StringComparer.OrdinalIgnoreCase.GetHashCode(Email);
}

var set = new HashSet<SubscriberProfile>();
var user = new SubscriberProfile { Email = "alex@example.com", Name = "Alex" };
set.Add(user);

user.Email = "alex.k@example.com";  // user corrected typo after Add

bool found = set.Contains(user);  // false — user still "in" set but unreachable
```

What went wrong with mutability and the hash contract, and how do you fix the type for set membership?

---

#### Q4. (R) An editorial dashboard merges article tag sets for a "shared topics" widget. Case variants appear twice after deploy. Review:

```csharp
var dotnetTags = new HashSet<string>(_dotnetArticle.Tags, StringComparer.OrdinalIgnoreCase);
var linqTags = new HashSet<string>(_linqArticle.Tags, StringComparer.OrdinalIgnoreCase);

// Developer assumes LINQ Union inherits the HashSet comparer:
IEnumerable<string> allTopics = dotnetTags.Union(linqTags);

var widgetTags = new HashSet<string>(allTopics);  // default Ordinal comparer
Console.WriteLine(widgetTags.Count);              // "csharp" and "CSharp" both present
```

What comparer mismatch caused duplicate logical tags, and how do you build the union with consistent equality end-to-end?

---

#### Q5. (R) A publish pipeline accidentally wipes an editor's working tag pool. Review the merge step:

```csharp
HashSet<string> editorPool = new(StringComparer.OrdinalIgnoreCase)
{
    "csharp", "dotnet", "security"
};

HashSet<string> draftTags = new(StringComparer.OrdinalIgnoreCase)
{
    "dotnet", "api", "draft"
};

// Intent: preview tags common to BOTH pools without changing editorPool
editorPool.IntersectWith(draftTags);

Console.WriteLine(string.Join(", ", editorPool)); // only "dotnet" — pool mutated
// Later: editorPool.UnionWith(blockedList) no longer restores "csharp", "security"
```

The developer meant a non-mutating preview. What API mistake was made, and show the safe pattern that leaves `editorPool` unchanged?

---

#### Q6. (R) A custom comparer passes code review but `Remove` and `Contains` behave inconsistently. Review:

```csharp
public sealed class SubscriberByNameComparer : IEqualityComparer<Subscriber>
{
    public bool Equals(Subscriber? x, Subscriber? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        return string.Equals(x.Email, y.Email, StringComparison.OrdinalIgnoreCase);
    }

    public int GetHashCode(Subscriber obj) =>
        obj.Name.GetHashCode(StringComparison.Ordinal);  // hashes Name, not Email
}

var set = new HashSet<Subscriber>(new SubscriberByNameComparer());
var a = new Subscriber("Alex", "alex@example.com");
set.Add(a);
set.Contains(new Subscriber("Alex K.", "alex@example.com")); // sometimes false
set.Remove(new Subscriber("Alex K.", "alex@example.com"));    // sometimes false while Add returned false on duplicate
```

What contract violation breaks `HashSet<T>`, and what is the corrected comparer implementation?
