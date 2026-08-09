/*
 * TOPIC: Capstone-style mini-domains that apply OOP pillars together — encapsulation,
 *        abstraction, inheritance, polymorphism, interfaces, and abstract classes —
 *        in code shaped like production features.
 *
 * WHY IT MATTERS:
 *   Textbook definitions become useful when you see them in code you would ship:
 *   a bank balance you cannot corrupt, a payment gateway you can swap, vehicles
 *   that share a base type, shapes drawn through one API, notifications on a
 *   common contract, and invoices built from a document template.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Four OOP pillars mapped to concrete C# patterns in one fulfillment scenario
 *   2.  Encapsulation — BankAccount with private state and controlled operations
 *   3.  Abstraction — PaymentProcessor hiding gateway-specific charge logic
 *   4.  Inheritance — Vehicle hierarchy with constructor chaining and overrides
 *   5.  Polymorphism — Shape drawing through base references
 *   6.  Interface — INotificationSender contract and multiple implementers
 *   7.  Abstract class — Document template method with InvoiceDocument
 *   8.  Preview: partial classes and sealed (→ 05. Inheritance and Polymorphism)
 *   9.  Preview: extension methods (→ Functional module ch.04 Extension Methods)
 *  10.  Preview: events wiring order completion (→ 08. Events)
 */

using System;
using System.Globalization;

namespace OopRealWorldExamples;

/*
 * SECTION 1: FOUR PILLARS IN ONE FULFILLMENT SCENARIO
 *
 * The four classic pillars map to concrete code patterns in this chapter:
 *
 *   Pillar          | Mechanism in C#              | Example here
 *   ----------------|------------------------------|---------------------------
 *   Encapsulation   | private fields + public API  | BankAccount balance
 *   Abstraction     | abstract base / hide detail  | PaymentProcessor
 *   Inheritance     | base class + derived types   | Vehicle → Car, Truck
 *   Polymorphism    | override + base reference    | Shape.Draw()
 *
 * Interfaces and abstract classes both support abstraction:
 *   interface         — contract without shared implementation (INotificationSender)
 *   abstract class    — shared fields + mix of concrete/abstract members (Document)
 *
 * Scenario: a customer places an order — funds leave an account, a card gateway
 * charges the order, a truck delivers, label shapes are computed, email/SMS
 * notifications fire, an invoice document renders, and an event signals completion.
 *
 * Earlier chapters own FULL depth on each building block — this chapter integrates
 * them. Look for "FULL DEPTH → NN. Topic" notes when you need the complete treatment.
 */

/*
 * SECTION 2: ENCAPSULATION — BankAccount
 *
 * ENCAPSULATION hides internal state and exposes only safe operations.
 * Callers cannot assign _balance directly — they must use Deposit or TryWithdraw,
 * which enforce business rules.
 *
 * --- 2a. Why private fields matter ---
 *
 *   public decimal Balance { get; set; }   // any caller can set any value
 *   private decimal _balance;              // only methods on this class change it
 *
 * --- 2b. Read-only surface ---
 *
 * Balance exposes current funds without a public setter — inspection only.
 * Expression-bodied read-only property: public decimal Balance => _balance;
 *
 * FULL DEPTH → 07. Encapsulation and Access Modifiers (private, invariants)
 * FULL DEPTH → 02. Properties and Indexers (auto, full, init, expression-bodied)
 * FULL DEPTH → 03. Constructors and Method Overloading (validation in ctor)
 *
 * --- 2c. partial class (preview) ---
 *
 * partial splits one class across compilation units. Teams split large types by
 * feature (core banking, validation, persistence) without one giant file.
 * Both fragments below merge into a single BankAccount type at compile time.
 * In production the second fragment might live in BankAccount.Validation.cs.
 */
public partial class BankAccount
{
    private decimal _balance;
    private readonly string _accountNumber;

    public BankAccount(string accountNumber, decimal openingBalance)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
        {
            throw new ArgumentException("Account number is required.", nameof(accountNumber));
        }

        if (openingBalance < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(openingBalance), "Opening balance cannot be negative.");
        }

        _accountNumber = accountNumber;
        _balance = openingBalance;
    }

    public string AccountNumber => _accountNumber;

    public decimal Balance => _balance;

    public void Deposit(decimal amount)
    {
        ValidateForTransaction();

        if (amount <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Deposit amount must be positive.");
        }

        _balance += amount;
    }

    public bool TryWithdraw(decimal amount, out string? errorMessage)
    {
        errorMessage = null;
        ValidateForTransaction();

        if (amount <= 0m)
        {
            errorMessage = "Withdrawal amount must be positive.";
            return false;
        }

        if (amount > _balance)
        {
            errorMessage = $"Insufficient funds. Balance is {_balance:C}.";
            return false;
        }

        _balance -= amount;
        return true;
    }
}

/*
 * --- 2d. partial fragment — account lifecycle / validation ---
 *
 * The compiler merges this with the first partial BankAccount above.
 * IsActive and ValidateForTransaction stay hidden from callers except through
 * Deposit/TryWithdraw behavior.
 */
public partial class BankAccount
{
    public bool IsActive { get; private set; } = true;

    public void Freeze()
    {
        IsActive = false;
    }

    private void ValidateForTransaction()
    {
        if (!IsActive)
        {
            throw new InvalidOperationException($"Account {_accountNumber} is frozen.");
        }
    }
}

/*
 * SECTION 3: ABSTRACTION — PaymentProcessor
 *
 * ABSTRACTION exposes *what* a component does while hiding *how*.
 * Order code depends on PaymentProcessor, not on card-network HTTP details
 * or wallet token APIs.
 *
 * --- 3a. Abstract class as abstraction boundary ---
 *
 *   PaymentProcessor.ProcessOrderPayment(...)  // stable caller API
 *        └── TryCharge(...) implemented per gateway
 *
 * --- 3b. Swapping implementations ---
 *
 * The same variable type (PaymentProcessor) holds CardPaymentProcessor or
 * WalletPaymentProcessor — choose at runtime based on customer preference.
 *
 * FULL DEPTH → 06. Abstract Classes and Interfaces (abstract members, override)
 */
public abstract class PaymentProcessor
{
    public abstract string ProcessorName { get; }

    public abstract bool TryCharge(decimal amount, string paymentReference);

    public string ProcessOrderPayment(decimal amount, string paymentReference)
    {
        bool charged = TryCharge(amount, paymentReference);
        if (charged)
        {
            return $"{ProcessorName} charged {amount:C} (ref {paymentReference}).";
        }

        return $"{ProcessorName} declined ref {paymentReference}.";
    }
}

public sealed class CardPaymentProcessor : PaymentProcessor
{
    public override string ProcessorName => "Card Gateway";

    public override bool TryCharge(decimal amount, string paymentReference)
    {
        return amount > 0m && amount <= 10_000m;
    }
}

public sealed class WalletPaymentProcessor : PaymentProcessor
{
    public override string ProcessorName => "Digital Wallet";

    public override bool TryCharge(decimal amount, string paymentReference)
    {
        return amount > 0m && paymentReference.StartsWith("WLT-", StringComparison.Ordinal);
    }
}

/*
 * SECTION 4: INHERITANCE — Vehicle hierarchy
 *
 * INHERITANCE reuses common data and behavior in a base class (Vehicle)
 * and specializes in derived types (Car, Truck, Motorcycle).
 *
 * --- 4a. is-a relationship ---
 *
 *   Car is a Vehicle — Car inherits Make, Model, Year, Describe().
 *
 * --- 4b. protected constructor chain ---
 *
 *   : base(make, model, year) runs the Vehicle constructor first.
 *
 * --- 4c. virtual override for subtype detail ---
 *
 * Truck overrides EstimateDeliveryCostKm to account for payload weight.
 *
 * --- 4d. sealed on Motorcycle (preview) ---
 *
 * sealed on a class prevents further inheritance (class SportBike : Motorcycle { }).
 * Use on leaf types that must stay final.
 *
 * FULL DEPTH → 05. Inheritance and Polymorphism (virtual, override, sealed)
 * FULL DEPTH → 03. Constructors and Method Overloading (: base(...) chaining)
 * FULL DEPTH → 07. Encapsulation and Access Modifiers (protected members)
 */
public abstract class Vehicle
{
    public string Make { get; }
    public string Model { get; }
    public int Year { get; }

    protected Vehicle(string make, string model, int year)
    {
        Make = make;
        Model = model;
        Year = year;
    }

    public abstract string VehicleType { get; }

    public virtual string Describe()
    {
        return $"{Year} {Make} {Model} ({VehicleType})";
    }

    public virtual decimal EstimateDeliveryCostKm(decimal ratePerKm)
    {
        return ratePerKm * 1.0m;
    }
}

public class Car : Vehicle
{
    public int DoorCount { get; }

    public Car(string make, string model, int year, int doorCount)
        : base(make, model, year)
    {
        DoorCount = doorCount;
    }

    public override string VehicleType => "Car";

    public override string Describe()
    {
        return base.Describe() + $", {DoorCount} doors";
    }
}

public class Truck : Vehicle
{
    public decimal PayloadTons { get; }

    public Truck(string make, string model, int year, decimal payloadTons)
        : base(make, model, year)
    {
        PayloadTons = payloadTons;
    }

    public override string VehicleType => "Truck";

    public override decimal EstimateDeliveryCostKm(decimal ratePerKm)
    {
        return ratePerKm * (1.0m + PayloadTons * 0.05m);
    }

    public override string Describe()
    {
        return base.Describe() + $", payload {PayloadTons:0.0} t";
    }
}

public sealed class Motorcycle : Vehicle
{
    public bool HasSidecar { get; }

    public Motorcycle(string make, string model, int year, bool hasSidecar)
        : base(make, model, year)
    {
        HasSidecar = hasSidecar;
    }

    public override string VehicleType => "Motorcycle";

    public override string Describe()
    {
        string sidecar = HasSidecar ? "with sidecar" : "solo";
        return base.Describe() + $", {sidecar}";
    }
}

/*
 * SECTION 5: POLYMORPHISM — Shape drawing
 *
 * POLYMORPHISM lets code treat different subtypes uniformly through a base
 * reference. At runtime the CLR dispatches to the correct override.
 *
 * --- 5a. Base reference, derived object ---
 *
 *   Shape label = new Circle(4);   // static type Shape, runtime type Circle
 *   label.Draw();                  // calls Circle.Draw()
 *
 * --- 5b. Loop without type checks ---
 *
 * RenderLabels iterates Shape[] — no if/else on typeof(Circle).
 *
 * FULL DEPTH → 05. Inheritance and Polymorphism (runtime dispatch, override)
 */
public abstract class Shape
{
    public abstract double Area { get; }

    public abstract string Draw();
}

public class Circle : Shape
{
    public double Radius { get; }

    public Circle(double radius)
    {
        if (radius <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(radius), "Radius must be positive.");
        }

        Radius = radius;
    }

    public override double Area => Math.PI * Radius * Radius;

    public override string Draw()
    {
        return $"Circle r={Radius:0.##} area={Area:0.##}";
    }
}

public class Rectangle : Shape
{
    public double Width { get; }
    public double Height { get; }

    public Rectangle(double width, double height)
    {
        if (width <= 0 || height <= 0)
        {
            throw new ArgumentOutOfRangeException("Width and height must be positive.");
        }

        Width = width;
        Height = height;
    }

    public override double Area => Width * Height;

    public override string Draw()
    {
        return $"Rectangle {Width:0.##}x{Height:0.##} area={Area:0.##}";
    }
}

/*
 * SECTION 6: INTERFACE — INotificationSender
 *
 * An INTERFACE is a contract: ChannelName and Send must exist on every
 * implementer. Unlike abstract Document, notification channels share no
 * base class — Email and SMS are unrelated types united by the contract.
 *
 * --- 6a. Programming to the interface ---
 *
 *   INotificationSender sender = new EmailNotificationSender();
 *   NotifyAll channels through INotificationSender[] — add Push later.
 *
 * FULL DEPTH → 06. Abstract Classes and Interfaces (interface vs abstract class)
 */
public interface INotificationSender
{
    string ChannelName { get; }

    string Send(string recipient, string message);
}

public class EmailNotificationSender : INotificationSender
{
    public string ChannelName => "Email";

    public string Send(string recipient, string message)
    {
        return $"[{ChannelName}] To: {recipient} | {message}";
    }
}

public class SmsNotificationSender : INotificationSender
{
    public string ChannelName => "SMS";

    public string Send(string recipient, string message)
    {
        string truncated = message.Length <= 160 ? message : message[..157] + "...";
        return $"[{ChannelName}] To: {recipient} | {truncated}";
    }
}

/*
 * SECTION 7: ABSTRACT CLASS — Document base
 *
 * An ABSTRACT CLASS can define shared state, concrete methods, and abstract
 * members subclasses must implement.
 *
 * --- 7a. Template method pattern ---
 *
 *   Render() is concrete — it calls abstract RenderBody() on the subclass.
 *   InvoiceDocument supplies invoice-specific body text.
 *
 * --- 7b. Abstract class vs interface (when to pick which) ---
 *
 *   Use abstract class when subtypes share fields + template logic (Document).
 *   Use interface when unrelated types must expose the same capability (Send).
 *
 * FULL DEPTH → 06. Abstract Classes and Interfaces
 */
public abstract class Document
{
    public string DocumentId { get; }
    public DateTime CreatedAt { get; }

    protected Document(string documentId, DateTime createdAt)
    {
        DocumentId = documentId;
        CreatedAt = createdAt;
    }

    public abstract string Title { get; }

    public abstract string RenderBody();

    public string Render()
    {
        return $"--- {Title} ({DocumentId}) {CreatedAt:yyyy-MM-dd} ---\n{RenderBody()}";
    }
}

public class InvoiceDocument : Document
{
    public string CustomerName { get; }
    public decimal Amount { get; }

    public InvoiceDocument(string documentId, DateTime createdAt, string customerName, decimal amount)
        : base(documentId, createdAt)
    {
        CustomerName = customerName;
        Amount = amount;
    }

    public override string Title => "Tax Invoice";

    public override string RenderBody()
    {
        return $"Bill to: {CustomerName}\nAmount due: {Amount.ToString("C", CultureInfo.CurrentCulture)}";
    }
}

/*
 * SECTION 8: PREVIEW — EXTENSION METHODS
 *
 * Extension methods look like instance methods but are static methods in
 * a static class with a this parameter on the first argument.
 *
 *   customerName.ToDisplayLabel()   // syntax sugar for StringDisplayExtensions.ToDisplayLabel(customerName)
 *
 * FULL DEPTH → Functional module ch.04 Extension Methods
 *   (static class rules, LINQ operators, discovery, null behavior)
 *
 * FULL DEPTH → 04. Static Members and Static Classes (static class requirements)
 */
public static class StringDisplayExtensions
{
    public static string ToDisplayLabel(this string value)
    {
        return $"[{value}]";
    }
}

/*
 * SECTION 9: PREVIEW — EVENTS (ORDER COMPLETION)
 *
 * Events decouple the object that raises a signal from objects that react.
 * OrderFulfillmentCoordinator raises OrderCompleted after wiring payment,
 * delivery, and notifications — audit/logging subscribes with +=.
 *
 * Only the declaring type invokes the event from inside (safe exposure).
 *
 * FULL DEPTH → 08. Events (subscribe +=, raise, EventHandler<T>, event vs delegate)
 */
public class OrderFulfillmentCoordinator
{
    public event Action<string>? OrderCompleted;

    public string CompleteOrder(string orderReference, string summary)
    {
        string message = $"Order {orderReference} fulfilled. {summary}";
        OrderCompleted?.Invoke(message);
        return message;
    }
}

public class Program
{
    /*
     * SECTION 10: DEMONSTRATION — Main orchestrates the fulfillment scenario
     *
     * Main creates objects, calls methods, and prints results from every section.
     * Concept explanations live above the types — not repeated here.
     */
    public static void Main(string[] args)
    {
        string customerName = "Priya Sharma";
        string orderReference = "ORD-9081";
        decimal orderTotal = 249.99m;
        decimal deliveryDistanceKm = 12.5m;
        decimal ratePerKm = 2.40m;

        BankAccount wallet = new BankAccount("ACC-7742", 500.00m);
        wallet.Deposit(50.00m);
        bool withdrawalOk = wallet.TryWithdraw(orderTotal, out string? withdrawError);
        decimal balanceAfterWithdraw = wallet.Balance;

        PaymentProcessor cardGateway = new CardPaymentProcessor();
        PaymentProcessor walletGateway = new WalletPaymentProcessor();
        string cardChargeResult = cardGateway.ProcessOrderPayment(orderTotal, orderReference);
        string walletChargeResult = walletGateway.ProcessOrderPayment(orderTotal, "WLT-" + orderReference);

        Vehicle[] fleet =
        {
            new Car("Hyundai", "i20", 2022, 4),
            new Truck("Tata", "LPT", 2021, 3.5m),
            new Motorcycle("Royal Enfield", "Classic", 2023, false)
        };

        Vehicle deliveryVehicle = fleet[1];
        string deliveryVehicleDescription = deliveryVehicle.Describe();
        decimal deliveryCost = deliveryVehicle.EstimateDeliveryCostKm(ratePerKm) * deliveryDistanceKm;
        string carDescription = fleet[0].Describe();
        string motorcycleDescription = fleet[2].Describe();

        Shape[] labelShapes =
        {
            new Circle(3.5),
            new Rectangle(10, 6),
            new Circle(1.2)
        };

        string shapeRenderOutput = RenderLabels(labelShapes);
        double totalLabelArea = SumAreas(labelShapes);

        INotificationSender[] notifiers =
        {
            new EmailNotificationSender(),
            new SmsNotificationSender()
        };

        string notificationSummary = NotifyCustomer(notifiers, customerName, orderReference, orderTotal);

        Document invoice = new InvoiceDocument(
            "INV-9081",
            new DateTime(2026, 8, 7),
            customerName,
            orderTotal);

        string invoiceText = invoice.Render();

        bool motorcycleIsSealed = typeof(Motorcycle).IsSealed;
        bool carIsSealed = typeof(Car).IsSealed;
        wallet.Freeze();
        string frozenAccountMessage = TryDepositOnFrozenAccount(wallet);

        string channelLabel = customerName.ToDisplayLabel();

        OrderFulfillmentCoordinator coordinator = new OrderFulfillmentCoordinator();
        string auditEntry = string.Empty;
        coordinator.OrderCompleted += message => auditEntry = "[Audit] " + message;
        string fulfillmentMessage = coordinator.CompleteOrder(
            orderReference,
            $"Paid {orderTotal.ToString("C", CultureInfo.CurrentCulture)}, notified {customerName}.");

        Console.WriteLine("=== OOP Real-World Examples — Order Fulfillment ===");
        Console.WriteLine();
        Console.WriteLine($"Customer: {customerName} | Order: {orderReference} | Total: {orderTotal.ToString("C", CultureInfo.CurrentCulture)}");
        Console.WriteLine();
        Console.WriteLine("--- Encapsulation (BankAccount) ---");
        Console.WriteLine($"Withdraw {orderTotal.ToString("C", CultureInfo.CurrentCulture)} succeeded: {withdrawalOk} | Error: {withdrawError ?? "(none)"}");
        Console.WriteLine($"Balance after withdraw: {balanceAfterWithdraw.ToString("C", CultureInfo.CurrentCulture)}");
        Console.WriteLine();
        Console.WriteLine("--- Abstraction (PaymentProcessor) ---");
        Console.WriteLine(cardChargeResult);
        Console.WriteLine(walletChargeResult);
        Console.WriteLine();
        Console.WriteLine("--- Inheritance (Vehicle fleet) ---");
        Console.WriteLine($"Delivery: {deliveryVehicleDescription}");
        Console.WriteLine($"Delivery cost ({deliveryDistanceKm} km): {deliveryCost.ToString("C", CultureInfo.CurrentCulture)}");
        Console.WriteLine($"Car: {carDescription}");
        Console.WriteLine($"Motorcycle: {motorcycleDescription}");
        Console.WriteLine();
        Console.WriteLine("--- Polymorphism (Shape labels) ---");
        Console.WriteLine(shapeRenderOutput);
        Console.WriteLine($"Total label area: {totalLabelArea:0.##}");
        Console.WriteLine();
        Console.WriteLine("--- Interface (INotificationSender) ---");
        Console.WriteLine(notificationSummary);
        Console.WriteLine();
        Console.WriteLine("--- Abstract class (Document) ---");
        Console.WriteLine(invoiceText);
        Console.WriteLine();
        Console.WriteLine("--- Preview: partial / sealed ---");
        Console.WriteLine($"Motorcycle sealed: {motorcycleIsSealed} | Car sealed: {carIsSealed}");
        Console.WriteLine(frozenAccountMessage);
        Console.WriteLine();
        Console.WriteLine("--- Preview: extension method ---");
        Console.WriteLine($"Display label: {channelLabel}");
        Console.WriteLine();
        Console.WriteLine("--- Preview: event (OrderCompleted) ---");
        Console.WriteLine(fulfillmentMessage);
        Console.WriteLine(auditEntry);
    }

    private static string RenderLabels(Shape[] shapes)
    {
        string[] lines = new string[shapes.Length];
        for (int i = 0; i < shapes.Length; i++)
        {
            lines[i] = shapes[i].Draw();
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static double SumAreas(Shape[] shapes)
    {
        double total = 0;
        foreach (Shape shape in shapes)
        {
            total += shape.Area;
        }

        return total;
    }

    private static string NotifyCustomer(INotificationSender[] senders, string customer, string orderRef, decimal total)
    {
        string[] receipts = new string[senders.Length];
        string message = $"Order {orderRef} confirmed for {total.ToString("C", CultureInfo.CurrentCulture)}.";

        for (int i = 0; i < senders.Length; i++)
        {
            receipts[i] = senders[i].Send(customer, message);
        }

        return string.Join(Environment.NewLine, receipts);
    }

    private static string TryDepositOnFrozenAccount(BankAccount account)
    {
        try
        {
            account.Deposit(1.00m);
            return "Deposit succeeded (unexpected — account should be frozen).";
        }
        catch (InvalidOperationException ex)
        {
            return "Frozen account blocked deposit: " + ex.Message;
        }
    }
}

/*
 * QUICK REFERENCE — OOP REAL-WORLD PATTERNS
 *
 * --- Four pillars (this chapter) ---
 *
 *  Encapsulation   private state + controlled public methods (BankAccount)
 *  Abstraction     hide implementation behind a simple API (PaymentProcessor)
 *  Inheritance     base class + derived specializations (Vehicle)
 *  Polymorphism    base reference, runtime override dispatch (Shape.Draw)
 *
 * --- Abstraction mechanisms ---
 *
 *  abstract class    shared fields + mix of concrete/abstract members (Document)
 *  interface         pure contract, multiple implementers (INotificationSender)
 *
 * --- When to use which ---
 *
 *  Situation                         | Prefer
 *  ----------------------------------|----------------------------------
 *  Protect mutable state             | Encapsulation (private + methods)
 *  Hide varying algorithm            | Abstract base or interface
 *  Shared data + template method     | abstract class
 *  Unrelated types, same capability  | interface
 *  Reuse common identity/behavior    | inheritance + virtual/override
 *  Treat subtypes uniformly          | polymorphism via base type
 *
 * --- Preview syntax ---
 *
 *  partial class Foo { }              split across files — see Section 2
 *  sealed class Leaf : Base { }      no further inheritance — Section 4
 *  static string Ext(this string s)  extension — Functional module ch.04
 *  event Action<string>? Completed   subscribe += — 08. Events
 *
 * --- Full depth in sibling chapters ---
 *
 *  01. Classes and Objects           types, fields, methods, objects
 *  02. Properties and Indexers       property patterns, indexers
 *  03. Constructors and Overloading  ctor chains, overload resolution
 *  04. Static Members                static classes (extension host)
 *  05. Inheritance and Polymorphism  virtual, override, sealed
 *  06. Abstract Classes and Interfaces
 *  07. Encapsulation and Modifiers   private, protected, invariants
 *  08. Events                        subscribe, raise, EventHandler<T>
 */
