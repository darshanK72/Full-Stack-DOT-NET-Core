# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/02. Object Oriented Programming/04. Static Members & Static Classes`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) An ASP.NET Core API caches the "current user's cart" in a static field so every controller can read it without DI. Under load, users report seeing each other's items. Review the code — what is wrong and how do you fix it?

```csharp
public static class CartContext
{
    private static List<CartItem> _items = new();

    public static void SetCart(List<CartItem> items) => _items = items;

    public static decimal GetTotal() => _items.Sum(i => i.Price * i.Quantity);
}

public class CheckoutController : ControllerBase
{
    [HttpPost("checkout")]
    public IActionResult Checkout([FromBody] List<CartItem> cart)
    {
        CartContext.SetCart(cart);
        var total = CartContext.GetTotal();
        return Ok(new { total });
    }
}
```

---

#### Q2. (M) A teammate adds runtime config loading to `AppSettings` and reports intermittent `TypeInitializationException` on first request. Review the static initialization — what ordering traps exist, and how would you make startup deterministic?

```csharp
public static class AppSettings
{
    public static readonly string EnvironmentName =
        Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";

    public static readonly int MaxLoginAttempts = LoadMaxAttempts();

    private static readonly string ConfigPath =
        Path.Combine(AppContext.BaseDirectory, "appsettings.json");

    static AppSettings()
    {
        Console.WriteLine($"Loading settings from {ConfigPath} for {EnvironmentName}");
    }

    private static int LoadMaxAttempts()
    {
        // reads ConfigPath from disk — throws if file missing
        return int.Parse(File.ReadAllText(ConfigPath).Trim());
    }
}
```

---

#### Q3. (R) Production logging uses the tutorial's `AuditLogger` singleton instead of `ILogger`. Tests pass locally but CI flakes and log counts are wrong under concurrent requests. Review the pattern — what's broken and what replaces it?

```csharp
public sealed class AuditLogger
{
    private static readonly AuditLogger InstanceField = new AuditLogger();
    private int _entryCount;

    private AuditLogger() { }

    public static AuditLogger Instance => InstanceField;

    public void Record(string message)
    {
        _entryCount++;
        Console.WriteLine($"[{_entryCount}] {message}");
    }
}

// Startup.cs / Program.cs
builder.Services.AddSingleton(AuditLogger.Instance);
```

---

#### Q4. (R) A developer refactors `TaxHelper` to support per-region tax profiles and adds instance state. Build fails. Review the changes — what rules did they violate, and what structure should replace a static class here?

```csharp
public static class TaxHelper
{
    public const decimal DefaultRate = 0.0825m;
    private decimal _regionRate;  // set from constructor

    public TaxHelper(decimal regionRate) => _regionRate = regionRate;

    public static decimal CalculateSalesTax(decimal amount, decimal rate)
        => Math.Round(amount * rate, 2, MidpointRounding.AwayFromZero);

    public decimal CalculateForRegion(decimal amount)
        => CalculateSalesTax(amount, _regionRate);
}
```

---

#### Q5. (R) `BankAccount` account numbers duplicate in production after traffic increases. The team uses the tutorial counter as-is. Review the static field usage — what race exists and how do you fix it without abandoning a shared sequence?

```csharp
public class BankAccount
{
    private static int _nextAccountNumber = 1000;

    public int AccountNumber { get; }

    public BankAccount(string ownerName, decimal openingDeposit)
    {
        AccountNumber = _nextAccountNumber++;  // called from many threads
        OwnerName = ownerName;
        Balance = openingDeposit;
    }

    // ...
}
```

---

#### Q6. (D) Your API team debates three approaches for shared, read-mostly configuration: `public const` literals, `static readonly` loaded at type init, and mutable `public static` properties set from middleware. Which would you allow in a multi-instance ASP.NET Core deployment, and which would you ban? Why?

---
