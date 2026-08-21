# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/08. Advanced C# Features/03. Regular Expressions/`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A bulk-import API validates thousands of customer rows per request. After deploy, CPU spikes and some requests time out. Review this validator and prioritize fixes.

```csharp
public sealed class ImportRowValidator
{
    public bool IsValid(string email, string phone, string notes)
    {
        const string emailPat = @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}";
        const string phonePat = @"^(\+1[-.\s]?)?(\([0-9]{3}\)|[0-9]{3})[-.\s]?[0-9]{3}[-.\s]?[0-9]{4}$";

        if (!Regex.IsMatch(email, emailPat))
            return false;
        if (!Regex.IsMatch(phone, phonePat))
            return false;

        // Extract embedded order id from free-text notes
        var orderMatch = Regex.Match(notes, @"(order\s+\d+)+");
        return orderMatch.Success;
    }
}
```

---

#### Q2. (R) A support portal lets agents paste a custom regex to search and redact matches in uploaded log files (multi-MB). Review this endpoint helper:

```csharp
public string RedactMatches(string logContent, string userPattern)
{
    var regex = new Regex(
        userPattern,
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    return regex.Replace(logContent, "[REDACTED]");
}
```

What production risks exist, and how would you harden this for untrusted input?

---

#### Q3. (R) A notes-processing job extracts phone fragments for a CRM sync. Review this extractor:

```csharp
public sealed class PhoneExtractor
{
    private static readonly Regex PhonePattern = new(
        @"(\+1[-.\s]?)?(\([0-9]{3}\)|[0-9]{3})[-.\s]?[0-9]{3}[-.\s]?[0-9]{4}",
        RegexOptions.Compiled);

    public string GetAreaCode(string notes)
    {
        Match m = PhonePattern.Match(notes);
        return m.Groups[2].Value; // area-code capture
    }

    public bool HasUsPhone(string notes) =>
        Regex.IsMatch(notes, PhonePattern.ToString());
}
```

What correctness bugs appear on edge-case inputs, and how do you fix them?

---

#### Q4. (P) Your registration API validates email on every POST (~2k RPS). A teammate proposes three options:

1. `Regex.IsMatch(email, pattern)` inline in the action  
2. `static readonly Regex` field with `RegexOptions.Compiled | RegexOptions.CultureInvariant`  
3. `[RegularExpression(@"…")]` on the DTO property  

When would you choose each, and what companion settings (timeout, anchoring, caching) are mandatory for the Regex-based approaches in production?

---

#### Q5. (D) Product wants import rejection for disposable email domains (`mailinator.com`, `tempmail.org`, …) and a regex that only allows corporate TLDs. A developer merges the blocklist into one giant pattern:

```csharp
bool ok = Regex.IsMatch(email,
    @"^(?!.*@(mailinator|tempmail)\.com$)[a-zA-Z0-9._%+-]+@(?:contoso|fabrikam)\.(?:com|org)$");
```

What breaks in maintainability, testability, and correctness compared to splitting validation layers? How would you structure this in a real import pipeline?

---

#### Q6. (M) A config-ingestion worker parses key/value lines from Windows-generated files. Keys on lines after the first never match:

```csharp
string file = "Server=prod-db\r\nPort=5432\r\nTimeout=30";
bool secondLineMatches = Regex.IsMatch(file, @"^Port=");
// secondLineMatches == false — team expects true
```

Explain why default regex behavior fails here and what minimal change fixes it without rewriting the parser as a full state machine.

---

#### Q7. (R) An internal tool "sanitizes" HTML fragments before storing them in a knowledge base:

```csharp
public string StripTags(string html)
{
    string noTags = Regex.Replace(html, @"<.*>", string.Empty);
    return Regex.Replace(noTags, @"<script.*?>.*?</script>", string.Empty);
}
```

Review on input `"<div>Title</div><script>alert(1)</script>"`. What goes wrong with matching order, greediness, and security assumptions?
