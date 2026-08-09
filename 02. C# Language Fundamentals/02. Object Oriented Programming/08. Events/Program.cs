/*
 * TOPIC: The publisher/subscriber pattern — how a class (publisher) notifies
 *        interested code (subscribers/handlers) when something happens, using
 *        delegates constrained by the event keyword.
 *
 * WHY IT MATTERS:
 *   UI buttons, payment gateways, and domain models all need to signal change
 *   without knowing every listener in advance. Events decouple the object that
 *   detects a change from the code that reacts (logging, email, UI refresh).
 *   The event keyword prevents outsiders from faking or clearing notifications.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Roles: publisher, event, subscriber (handler)
 *   2.  delegate keyword — brief review (full lesson in Functional module)
 *   3.  event keyword and its private backing delegate
 *   4.  Subscribe (+=) and unsubscribe (-=)
 *   5.  EventHandler and EventHandler<TEventArgs>
 *   6.  Custom EventArgs for structured notification data
 *   7.  Raise patterns: ?.Invoke, On* methods, thread-safe copy
 *   8.  Events vs exposing a delegate publicly
 *   9.  Preview: real-time multi-subscriber scenarios → ch.09
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Events;

/*
 * SECTION 1: ROLES — PUBLISHER, EVENT, SUBSCRIBER (HANDLER)
 *
 *   Role          | Responsibility
 *   --------------|--------------------------------------------------------
 *   Publisher     | Owns state; detects a change; raises (invokes) event
 *   Event         | Named notification channel backed by a delegate list
 *   Subscriber    | Method (handler) registered on the event; runs when raised
 *
 * BankAccount (Section 4) is the publisher in this chapter.
 * Handlers in Program.Main are subscribers.
 *
 * Flow: publisher changes state → calls delegate?.Invoke(...) → each subscribed
 * handler runs in registration order (multicast delegate).
 */

/*
 * SECTION 2: delegate KEYWORD — REVIEW (PREVIEW)
 *
 * A DELEGATE is a type-safe reference to a method with a matching signature.
 * Events are built on delegates — the event stores a multicast delegate
 * (zero or more handlers).
 *
 *   public delegate void BalanceAlertHandler(string message);
 *
 * Any method void Handler(string message) can be assigned, combined, or subscribed.
 *
 * COVERED IN DETAIL LATER → 04. Functional Style Programming / 01. Delegates
 *   (multicast, Combine, Remove, Func/Action, and anonymous methods)
 */

/*
 * SECTION 3: CUSTOM EventArgs
 *
 * Derive from System.EventArgs to pass structured data to handlers.
 * Used with EventHandler<TEventArgs>, where TEventArgs : EventArgs.
 *
 * Handlers receive:
 *
 *   void Handler(object? sender, BalanceChangedEventArgs e)
 *
 * sender — usually the publisher instance (this)
 * e      — payload with OldBalance, NewBalance, Reason
 *
 * Raise from the publisher:
 *
 *   BalanceChanged?.Invoke(this, new BalanceChangedEventArgs(old, @new, reason));
 *
 * For events with no extra data, use the non-generic EventHandler and pass
 * EventArgs.Empty:
 *
 *   Closed?.Invoke(this, EventArgs.Empty);
 */
public class BalanceChangedEventArgs : EventArgs
{
    public decimal OldBalance { get; }
    public decimal NewBalance { get; }
    public string Reason { get; }

    public BalanceChangedEventArgs(decimal oldBalance, decimal newBalance, string reason)
    {
        OldBalance = oldBalance;
        NewBalance = newBalance;
        Reason = reason;
    }
}

/*
 * SECTION 4: PUBLISHER — delegate, event, EventHandler<T>, AND RAISE PATTERNS
 *
 * --- 4a. Custom delegate + event keyword ---
 *
 *   public delegate void BalanceAlertHandler(string message);
 *   public event BalanceAlertHandler? BalanceAlert;
 *
 * The compiler creates a PRIVATE backing delegate field. External code may only
 * += or -= handlers — not assign null to wipe subscribers, not Invoke to raise.
 * Only BankAccount calls BalanceAlert?.Invoke(...).
 *
 * Compile error from outside the declaring class:
 *   account.BalanceAlert?.Invoke("hack");  // CS0070
 *
 * --- 4b. EventHandler and EventHandler<TEventArgs> ---
 *
 * BCL standard delegates for .NET events:
 *
 *   EventHandler              → void Handler(object? sender, EventArgs e)
 *   EventHandler<TEventArgs>  → void Handler(object? sender, TEventArgs e)
 *                                 where TEventArgs : EventArgs
 *
 * BalanceChanged uses EventHandler<BalanceChangedEventArgs> — the idiomatic
 * choice when you need a typed payload.
 *
 * --- 4c. Standard raise patterns ---
 *
 * Null-conditional invoke — skips when no handlers are registered:
 *
 *   BalanceAlert?.Invoke(message);
 *
 * Protected On* method — central place to raise; derived classes can override
 * to add logic before or after notification:
 *
 *   protected virtual void OnBalanceChanged(BalanceChangedEventArgs e)
 *   {
 *       BalanceChanged?.Invoke(this, e);
 *   }
 *
 * Thread-safe raise (multi-threaded UI or services) — copy the delegate to a
 * local before invoking so += / -= on another thread cannot null the reference
 * between the null check and Invoke:
 *
 *   EventHandler<BalanceChangedEventArgs>? handler = BalanceChanged;
 *   handler?.Invoke(this, e);
 *
 * --- 4d. Custom add / remove accessors (optional) ---
 *
 * You may replace the compiler-generated event accessors with explicit add/remove
 * to synchronize on a lock object. Default auto-accessors are fine for most
 * console and single-threaded domain code.
 */
public class BankAccount
{
    public delegate void BalanceAlertHandler(string message);

    public string AccountNumber { get; }
    public decimal Balance { get; private set; }
    public decimal LowBalanceThreshold { get; }

    public event BalanceAlertHandler? BalanceAlert;

    public event EventHandler<BalanceChangedEventArgs>? BalanceChanged;

    public BankAccount(string accountNumber, decimal openingBalance, decimal lowBalanceThreshold)
    {
        AccountNumber = accountNumber;
        Balance = openingBalance;
        LowBalanceThreshold = lowBalanceThreshold;
    }

    public void Deposit(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Deposit must be positive.");
        }

        decimal oldBalance = Balance;
        Balance += amount;
        OnBalanceChanged(new BalanceChangedEventArgs(oldBalance, Balance, "Deposit"));
        CheckLowBalance();
    }

    public bool TryWithdraw(decimal amount, out string message)
    {
        if (amount <= 0)
        {
            message = "Withdrawal amount must be positive.";
            return false;
        }

        if (amount > Balance)
        {
            message = "Insufficient funds.";
            BalanceAlert?.Invoke($"Withdrawal declined for {AccountNumber}: insufficient funds.");
            return false;
        }

        decimal oldBalance = Balance;
        Balance -= amount;
        message = $"Withdrew {amount:C} from {AccountNumber}.";
        OnBalanceChanged(new BalanceChangedEventArgs(oldBalance, Balance, "Withdrawal"));
        CheckLowBalance();
        return true;
    }

    private void CheckLowBalance()
    {
        if (Balance < LowBalanceThreshold)
        {
            BalanceAlert?.Invoke(
                $"Low balance on {AccountNumber}: {Balance:C} (threshold {LowBalanceThreshold:C}).");
        }
    }

    protected virtual void OnBalanceChanged(BalanceChangedEventArgs e)
    {
        EventHandler<BalanceChangedEventArgs>? handler = BalanceChanged;
        handler?.Invoke(this, e);
    }
}

/*
 * SECTION 5: EVENTS VS PUBLIC DELEGATE EXPOSURE
 *
 *   Exposure              | Outsider can Invoke? | Outsider can = null?
 *   ----------------------|----------------------|------------------------
 *   public Action? field  | Yes — dangerous       | Yes — wipes subscribers
 *   public event Action?  | No (CS0070)           | No — only += / -=
 *
 * UnsafeNotifier exposes a public delegate field — any caller can raise fake
 * notifications or assign null and remove every subscriber without their consent.
 *
 * SafeNotifier uses event — outsiders subscribe with += / -= only; only
 * SafeNotifier.Publish raises MessageReceived from inside the type.
 */
public class UnsafeNotifier
{
    public Action<string>? OnMessage;
}

public class SafeNotifier
{
    public event Action<string>? MessageReceived;

    public void Publish(string message)
    {
        MessageReceived?.Invoke(message);
    }
}

public class Program
{
    private static readonly List<string> AuditLog = new List<string>();

    /*
     * SECTION 6: DEMONSTRATION — SUBSCRIBE, RAISE, AND COMPARE EXPOSURE
     *
     * --- 6a. Subscribe (+=) and unsubscribe (-=) ---
     *
     * += adds a handler to the multicast list. The same method registered twice
     * runs twice when the event is raised.
     * -= removes ONE matching handler per call.
     *
     * Handlers may be named methods, lambdas, or local functions — any target
     * matching the delegate signature.
     *
     * --- 6b. delegate direct invoke (review) ---
     *
     * A delegate variable can be invoked without an event — useful for callbacks
     * where the caller owns the invocation list (see Section 2 preview).
     *
     * --- 6c. EventHandler<T> handler ---
     *
     * OnBalanceChanged reads sender as BankAccount and uses BalanceChangedEventArgs.
     *
     * --- 6d. Safe vs unsafe exposure ---
     *
     * DemonstrateUnsafeDelegateClear shows public field assignment wiping handlers.
     * DemonstrateSafeEventUnsubscribe shows event -= without outsider = null.
     *
     * --- PREVIEW: production multi-handler wiring ---
     *
     * COVERED IN DETAIL LATER → 09. OOP Real-World Examples
     *   (INotificationSender, payment events, service registration at startup)
     */
    public static void Main(string[] args)
    {
        BankAccount account = new BankAccount("ACC-1001", openingBalance: 2500m, lowBalanceThreshold: 500m);

        BankAccount.BalanceAlertHandler standaloneDelegate = LogBalanceAlert;
        standaloneDelegate("Delegate review: direct invoke without an event.");

        account.BalanceAlert += LogBalanceAlert;

        BankAccount.BalanceAlertHandler smsHandler = message => AuditLog.Add("[SMS] " + message);
        account.BalanceAlert += smsHandler;
        account.BalanceAlert += LogBalanceAlert;

        int alertHandlerCountBeforeUnsubscribe = CountBalanceAlertHandlers(account);
        account.BalanceAlert -= smsHandler;
        int alertHandlerCountAfterUnsubscribe = CountBalanceAlertHandlers(account);

        account.BalanceChanged += OnBalanceChanged;

        account.Deposit(300m);

        bool withdrawalSucceeded = account.TryWithdraw(1800m, out string withdrawalMessage);
        bool declinedWithdrawal = account.TryWithdraw(2000m, out string declinedMessage);

        account.Deposit(50m);

        bool lowBalanceWithdrawal = account.TryWithdraw(600m, out string lowBalanceMessage);

        UnsafeNotifier unsafeNotifier = new UnsafeNotifier();
        SafeNotifier safeNotifier = new SafeNotifier();

        unsafeNotifier.OnMessage = message => AuditLog.Add("[Unsafe field] " + message);
        safeNotifier.MessageReceived += message => AuditLog.Add("[Safe event] " + message);

        unsafeNotifier.OnMessage?.Invoke("Outsider invoked public delegate directly.");
        safeNotifier.Publish("Publisher raised safe event from inside SafeNotifier.");
        // safeNotifier.MessageReceived?.Invoke("blocked"); // CS0070 — uncomment to see error

        bool outsiderCanClearUnsafeChain = DemonstrateUnsafeDelegateClear(unsafeNotifier);
        bool eventRequiresExplicitUnsubscribe = DemonstrateSafeEventUnsubscribe(safeNotifier);

        string realTimePreviewNote =
            "Ch.09 wires events into notification services and domain capstones.";

        StringBuilder summary = new StringBuilder();
        summary.AppendLine("=== Events Tutorial Run Summary ===");
        summary.AppendLine($"Account {account.AccountNumber} final balance: {account.Balance:C}");
        summary.AppendLine($"Withdrawal succeeded: {withdrawalSucceeded} — {withdrawalMessage}");
        summary.AppendLine($"Withdrawal declined:  {!declinedWithdrawal} — {declinedMessage}");
        summary.AppendLine($"Alert handlers before SMS removed: {alertHandlerCountBeforeUnsubscribe}");
        summary.AppendLine($"Alert handlers after SMS removed:  {alertHandlerCountAfterUnsubscribe}");
        summary.AppendLine($"Outsider cleared unsafe delegate chain: {outsiderCanClearUnsafeChain}");
        summary.AppendLine($"Event subscriber removed with -= : {eventRequiresExplicitUnsubscribe}");
        summary.AppendLine($"Low-balance withdrawal: {lowBalanceWithdrawal} — {lowBalanceMessage}");
        summary.AppendLine(realTimePreviewNote);
        summary.AppendLine("--- Audit log ---");

        foreach (string entry in AuditLog)
        {
            summary.AppendLine(entry);
        }

        Console.WriteLine(summary.ToString());
    }

    private static void LogBalanceAlert(string message)
    {
        AuditLog.Add("[Log] " + message);
    }

    private static void OnBalanceChanged(object? sender, BalanceChangedEventArgs e)
    {
        string accountNumber = sender is BankAccount account ? account.AccountNumber : "unknown";
        AuditLog.Add(
            $"[BalanceChanged] {accountNumber}: {e.OldBalance:C} → {e.NewBalance:C} ({e.Reason})");
    }

    /*
     * Reflection-based handler count for tutorial output only — not a production pattern.
     */
    private static int CountBalanceAlertHandlers(BankAccount account)
    {
        FieldInfo? field = typeof(BankAccount).GetField(
            "BalanceAlert",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        if (field == null)
        {
            return -1;
        }

        if (field.GetValue(account) is BankAccount.BalanceAlertHandler handler)
        {
            return handler.GetInvocationList().Length;
        }

        return 0;
    }

    private static bool DemonstrateUnsafeDelegateClear(UnsafeNotifier notifier)
    {
        notifier.OnMessage += _ => AuditLog.Add("[Unsafe] handler A");
        notifier.OnMessage += _ => AuditLog.Add("[Unsafe] handler B");
        int before = notifier.OnMessage?.GetInvocationList().Length ?? 0;

        notifier.OnMessage = null;

        int after = notifier.OnMessage?.GetInvocationList().Length ?? 0;
        AuditLog.Add($"[Unsafe] assignment cleared chain: {before} → {after} handlers");
        return before > 0 && after == 0;
    }

    private static bool DemonstrateSafeEventUnsubscribe(SafeNotifier notifier)
    {
        Action<string> handler = message => AuditLog.Add("[Safe] " + message);
        notifier.MessageReceived += handler;
        notifier.MessageReceived -= handler;
        AuditLog.Add("[Safe] subscriber removed with -=; no outsider = null allowed.");
        return true;
    }
}

/*
 * QUICK REFERENCE — EVENTS
 *
 * --- Roles ---
 *
 *  Publisher   raises event when state changes
 *  Event       notification channel (multicast delegate)
 *  Handler     void Method(...){ } subscribed with +=
 *
 * --- Declare & raise ---
 *
 *  public delegate void MyHandler(string msg);
 *  public event MyHandler? SomethingHappened;
 *
 *  SomethingHappened?.Invoke(data);   // inside publisher only
 *
 * --- Subscribe / unsubscribe ---
 *
 *  publisher.SomethingHappened += MyMethod;
 *  publisher.SomethingHappened += msg => { };
 *  publisher.SomethingHappened -= MyMethod;
 *
 * --- Standard BCL types ---
 *
 *  EventHandler                     (object? sender, EventArgs e)
 *  EventHandler<TEventArgs>         TEventArgs : EventArgs
 *  EventArgs.Empty                  no payload for EventHandler
 *
 * --- Custom payload ---
 *
 *  public class MyEventArgs : EventArgs { public int Id { get; init; } }
 *  public event EventHandler<MyEventArgs>? ItemProcessed;
 *
 * --- Raise patterns ---
 *
 *  Event?.Invoke(this, args);                    // null-safe
 *  protected virtual void OnEvent(EventArgs e)    // override hook
 *  { EventHandler? h = Event; h?.Invoke(this,e); }  // thread-safe copy
 *
 * --- event vs public delegate ---
 *
 *  public Action? Callback;          // outsiders can Invoke and = null
 *  public event Action? Callback;    // outsiders only += / -=  (CS0070 on Invoke)
 *
 * --- Common mistakes ---
 *
 *  Mistake                              | Result
 *  -------------------------------------|----------------------------------
 *  Forgetting ?. before Invoke          | NullReferenceException if no handlers
 *  Duplicate += same method             | Handler runs multiple times
 *  Expecting -= to remove all copies    | Removes one registration per -=
 *  Raising event from outside publisher | CS0070 compile error
 *  Public delegate instead of event     | Subscribers can be cleared or spoofed
 *
 * --- Forward references ---
 *
 *  delegate deep dive  → 04. Functional Style Programming / 01. Delegates
 *  Real-world wiring   → 09. OOP Real-World Examples
 */
