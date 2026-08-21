# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/02. Object Oriented Programming/09. OOP Real-World Examples`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A team ports the chapter's order-fulfillment payment flow into a service class. Support sees duplicate debits and failed rollbacks after card declines. Review:

```csharp
public sealed class OrderPaymentService
{
    public string Run(BankAccount wallet, decimal total, string orderRef)
    {
        var card = new CardPaymentProcessor();
        var walletGw = new WalletPaymentProcessor();

        if (wallet.Balance < total)
            return "Insufficient funds";

        wallet.TryWithdraw(total, out _);

        string result = card.ProcessOrderPayment(total, orderRef);
        if (result.Contains("declined", StringComparison.OrdinalIgnoreCase))
        {
            wallet.Deposit(total);
            result = walletGw.ProcessOrderPayment(total, "WLT-" + orderRef);
        }

        return result;
    }
}
```

What is wrong across encapsulation, abstraction, and correctness — and how would you fix it in priority order?

---

#### Q2. (R) A logistics API quotes delivery cost from the chapter's `Vehicle` fleet. After adding `Motorcycle` to the fleet, quotes are wrong and every new vehicle type requires editing this method. Review:

```csharp
public static decimal QuoteDelivery(Vehicle vehicle, decimal distanceKm, decimal ratePerKm)
{
    if (vehicle is Car)
        return distanceKm * ratePerKm;

    if (vehicle is Truck truck)
        return distanceKm * ratePerKm * (1.0m + truck.PayloadTons * 0.05m);

    // Fallback for anything else (Motorcycle, future types)
    return distanceKm * ratePerKm * 2.0m;
}
```

What design problems do you see, and how does the chapter's polymorphism model replace this?

---

#### Q3. (R) A PR consolidates payment, delivery, labels, notifications, and invoicing into one coordinator for "simplicity." Review:

```csharp
public sealed class OrderFulfillmentHub
{
    public BankAccount CustomerWallet { get; set; } = new("ACC-DEFAULT", 0m);

    public string Fulfill(string customer, string orderRef, decimal total)
    {
        CustomerWallet.TryWithdraw(total, out _);

        var card = new CardPaymentProcessor();
        card.ProcessOrderPayment(total, orderRef);

        var truck = new Truck("Tata", "LPT", 2021, 3.5m);
        decimal cost = truck.EstimateDeliveryCostKm(2.4m) * 12.5m;

        var circle = new Circle(3.5);
        string label = $"Label area={Math.PI * circle.Radius * circle.Radius:0.##}";

        var email = new EmailNotificationSender();
        email.Send(customer, $"Order {orderRef} for {total:C}");

        var invoice = new InvoiceDocument("INV-1", DateTime.UtcNow, customer, total);
        return invoice.Render() + $" | delivery={cost:C} | {label}";
    }
}
```

Identify stacked OOP and SOLID issues. What would you split, inject, or abstract first?

---

#### Q4. (D) Product wants **push notifications** and a shared **retry-with-backoff** helper for all channels. Two proposals land in code review:

**Option A — extend abstract base:**

```csharp
public abstract class NotificationSenderBase
{
    protected void Retry(Action sendAttempt) { /* shared retry */ }
    public abstract string Send(string recipient, string message);
}

public class PushNotificationSender : NotificationSenderBase { /* ... */ }
```

**Option B — keep chapter interface + optional helper:**

```csharp
public interface INotificationSender
{
    string ChannelName { get; }
    string Send(string recipient, string message);
}

public static class NotificationRetry
{
    public static string SendWithRetry(INotificationSender sender, string recipient, string message) { /* ... */ }
}
```

Email and SMS already implement `INotificationSender` with no common base. Which direction fits this chapter's fulfillment model, and when would you combine both?

---

#### Q5. (P) An ASP.NET Core team registers the chapter's fulfillment types in `Program.cs` for a checkout API:

```csharp
builder.Services.AddSingleton<BankAccount>();
builder.Services.AddSingleton<OrderFulfillmentCoordinator>();
builder.Services.AddTransient<CardPaymentProcessor>();
builder.Services.AddTransient<PaymentProcessor>(sp => sp.GetRequiredService<CardPaymentProcessor>());
builder.Services.AddSingleton<INotificationSender, EmailNotificationSender>();
```

Under concurrent requests, balances mix between customers and notification behavior looks "sticky." Explain what breaks at the DI lifetime layer and how you would register these abstractions for production.

---

#### Q6. (R) A developer splits `BankAccount` into partial files (as in this chapter) but adds a "fast path" for internal ops. Frozen accounts still accept money in staging. Review both fragments:

```csharp
// BankAccount.Core.cs
public partial class BankAccount
{
    private decimal _balance;

    public decimal Balance => _balance;

    public void Deposit(decimal amount)
    {
        if (amount <= 0m) throw new ArgumentOutOfRangeException(nameof(amount));
        _balance += amount;
    }
}

// BankAccount.Ops.cs
public partial class BankAccount
{
    public bool IsActive { get; set; } = true;

    public void CreditOpsAdjustment(decimal amount)
    {
        // Skips ValidateForTransaction — ops-only
        _balance += amount;
    }

    private void ValidateForTransaction()
    {
        if (!IsActive) throw new InvalidOperationException("Account is frozen.");
    }
}
```

`TryWithdraw` still calls `ValidateForTransaction`, but `Deposit` no longer does. What failed across encapsulation and invariants, and how do you fix it?

---

#### Q7. (D) You inherit a monolithic fulfillment codebase that mirrors this chapter's demo `Main` — one method creates every object, mutates wallet state, picks a truck by array index, renders shapes, sends notifications, and prints the invoice. The team has one sprint to improve production readiness without a full rewrite.

What refactor order would you choose (encapsulation fixes, introduce interfaces, extract services, events/DI), and what would you **defer**? Tie your answer to the chapter's types (`BankAccount`, `PaymentProcessor`, `Vehicle`, `Shape`, `INotificationSender`, `Document`, `OrderFulfillmentCoordinator`).

---
