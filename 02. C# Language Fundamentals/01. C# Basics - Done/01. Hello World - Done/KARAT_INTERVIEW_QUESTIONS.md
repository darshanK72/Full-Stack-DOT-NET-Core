# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/01. C# Basics - Done/01. Hello World - Done`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) After a merge, `dotnet build` fails with CS0017 ("Program has more than one entry point defined"). Review these two files in the same console project. What conflicted, and how do you fix it?

```csharp
// Program.cs
Console.WriteLine("Starting batch export...");
RunExport();

static void RunExport() => Console.WriteLine("Export complete.");
```

```csharp
// LegacyMain.cs
namespace HelloWorld;

public class LegacyMain
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello from classic Main!");
    }
}
```

---

#### Q2. (R) A developer copies a startup snippet into this repo's HelloWorld project (`ImplicitUsings` disabled). Build fails. What is wrong, and what would you change?

```csharp
namespace HelloWorld;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine($"Job started at {DateTime.UtcNow:O}");
        Console.Error.WriteLine("Config loaded.");
    }
}
```

---

#### Q3. (R) A deployment script runs the published console tool with no arguments:

```bash
dotnet HelloWorld.dll
```

The program crashes on startup. Review `Main`:

```csharp
public static void Main(string[] args)
{
    var environment = args[0];
    var connectionString = args[1];
    Console.WriteLine($"Running in {environment} using {connectionString[..8]}…");
}
```

What breaks, and how would you harden this for CI and production invocation?

---

#### Q4. (P) A containerized .NET 8 worker uses only `Console.Write` (no newline) for progress dots during a long loop. Locally you see live output; in Kubernetes logs appear only after the process exits or crashes. Explain why and what you would change.

---

#### Q5. (D) Your team maintains internal CLI tools and tutorial projects. Some use **top-level statements**, others use explicit `namespace` + `class Program` + `Main` (as in this chapter). What convention would you recommend for production CLIs vs learning repos, and why?

---
