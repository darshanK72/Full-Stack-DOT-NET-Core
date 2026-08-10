/*
 * TOPIC: Structured exception handling — detecting runtime failures, recovering
 *        where possible, cleaning up resources, and surfacing meaningful errors.
 *
 * WHY IT MATTERS:
 *   Payment gateways time out, users enter invalid quantities, and account
 *   balances run dry. Uncaught exceptions terminate a process; poorly handled
 *   ones hide bugs. Production code uses try/catch/finally deliberately — catch
 *   what you can recover from, log what you cannot, and always release resources.
 *
 * WHAT YOU WILL LEARN:
 *   1.  try / catch / finally structure and execution order
 *   2.  Exception hierarchy (Exception, SystemException, application types)
 *   3.  Multiple catch blocks — most specific type first
 *   4.  throw, rethrow (throw;), and inner exceptions
 *   5.  Custom exception classes with domain properties
 *   6.  Exception properties (Message, StackTrace, InnerException, Source)
 *   7.  catch when exception filters — conditional handlers
 *   8.  Anti-patterns: swallowing, throw ex;, exceptions as control flow
 */

using System;
using System.Globalization;

namespace ExceptionHandling;

/*
 * SECTION 1: CUSTOM EXCEPTION — InvalidOrderException
 *
 * Application-specific exceptions inherit from System.Exception (not
 * ApplicationException — that type is discouraged for new code).
 *
 * Provide at least:
 *   • message-only constructor        — simple validation failures
 *   • message + inner constructor   — wrap lower-level failures (Section 8)
 *
 * Add properties (OrderId here) only when callers need structured data
 * beyond Message — catch blocks stay readable: catch (InvalidOrderException ex).
 *
 * | Guideline                         | Reason                              |
 * |-----------------------------------|-------------------------------------|
 * | [Serializable] on public types    | Legacy remoting; optional today     |
 * | Parameterless ctor for serializers| Required if you mark [Serializable] |
 * | Do not throw for expected flow    | Use return / Try* patterns instead  |
 */
public class InvalidOrderException : Exception
{
    public string OrderId { get; } // domain data beyond Message — readable in catch (InvalidOrderException ex)

    public InvalidOrderException(string orderId, string message)
        : base(message) // message-only path — simple validation failures
    {
        OrderId = orderId;
    }

    public InvalidOrderException(string orderId, string message, Exception innerException)
        : base(message, innerException) // wrap lower-level failure — InnerException chain
    {
        OrderId = orderId;
    }
}

/*
 * SECTION 2: CUSTOM EXCEPTION — InsufficientFundsException
 *
 * Domain exceptions carry the data support teams need in logs and UI:
 * RequestedAmount and AvailableBalance appear in Message and as properties.
 *
 * Custom types derive from Exception directly — they are NOT SystemException.
 * SystemException marks CLR/BCL/runtime failures (NullReferenceException,
 * FormatException, etc.). Business-rule types sit on the Exception branch.
 */
public class InsufficientFundsException : Exception
{
    public decimal RequestedAmount { get; }  // structured decline data for logs and UI
    public decimal AvailableBalance { get; }

    public InsufficientFundsException(decimal requested, decimal available)
        : base($"Payment of {requested:C} declined — available balance is {available:C}.") // :C currency format in Message
    {
        RequestedAmount = requested;
        AvailableBalance = available;
    }
}

/*
 * SECTION 3: Order — VALIDATION WITH throw
 *
 * Validate() enforces business rules by throwing InvalidOrderException when
 * a rule fails. Callers decide whether to catch, log, or let the error bubble.
 *
 * throw expr; aborts the current method and unwinds the stack until a matching
 * catch runs or the process terminates.
 *
 * Prefer ArgumentException / ArgumentNullException for bad method parameters;
 * domain types (InvalidOrderException) for business-rule violations on entities.
 */
public class Order
{
    public string OrderId { get; }
    public int Quantity { get; }
    public decimal UnitPrice { get; }

    public Order(string orderId, int quantity, decimal unitPrice)
    {
        OrderId = orderId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public decimal Total => Quantity * UnitPrice; // expression-bodied property — computed on read

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(OrderId))
        {
            throw new InvalidOrderException(OrderId ?? string.Empty, "Order id is required."); // ?? — null OrderId becomes ""
        }

        if (Quantity <= 0)
        {
            throw new InvalidOrderException(OrderId, "Quantity must be greater than zero."); // domain rule — not ArgumentException
        }

        if (UnitPrice <= 0m)
        {
            throw new InvalidOrderException(OrderId, "Unit price must be greater than zero.");
        }
    }
}

/*
 * SECTION 4: AuditLogWriter — IDisposable AND finally (PREVIEW)
 *
 * COVERED IN DETAIL LATER → OOP / IDisposable / C# 8 using declarations
 *
 * using (var writer = new AuditLogWriter()) { ... } expands to try/finally
 * that calls Dispose() even when an exception escapes — same cleanup guarantee
 * as an explicit finally block without boilerplate.
 */
public class AuditLogWriter : IDisposable
{
    private bool _disposed; // guard flag — WriteEntry after Dispose throws ObjectDisposedException

    public string LastEntry { get; private set; } = string.Empty;

    public void WriteEntry(string entry)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(AuditLogWriter)); // nameof — compile-time safe type name
        }

        LastEntry = entry;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            LastEntry = LastEntry + " [closed]"; // simulated cleanup — real writer would flush/close stream
            _disposed = true;
        }
    }
}

public class Program
{
    /*
     * SECTION 5: DEMONSTRATION — Main orchestrates the chapter demo
     *
     * Fixed scenario values — no keyboard input. Each helper below maps to
     * one teaching section; output at the end proves every path ran.
     */
    public static void Main(string[] args)
    {
        string customerName = "Aisha Khan";
        decimal walletBalance = 180.00m;
        Order validOrder = new Order("ORD-4420", 2, 49.99m);
        Order invalidQuantityOrder = new Order("ORD-4421", 0, 29.99m); // Quantity 0 — triggers InvalidOrderException
        string badQuantityText = "abc"; // not parseable as int — FormatException path
        decimal chargeAmount = 199.50m; // exceeds wallet — InsufficientFundsException path

        (string validationOutcome, bool finallyOnSuccess) = RunTryCatchFinallySuccess(validOrder); // SECTION 6
        (bool customIsSystemException, bool formatIsSystemException, bool customIsExceptionRoot) =
            InspectExceptionHierarchy(); // SECTION 7 — is / SystemException vs custom types
        string invalidOrderMessage = RunMultipleCatchInvalidOrder(invalidQuantityOrder); // SECTION 8a
        string parseFailureMessage = RunMultipleCatchParse(badQuantityText); // SECTION 8b
        (string paymentDeclinedMessage, bool finallyAfterCatch) =
            RunFinallyWhenCatchRuns(walletBalance, chargeAmount); // SECTION 9 — finally after catch
        (string rethrowMessage, string rethrowStackHint) = RunRethrowPreservesStackTrace(); // SECTION 11
        (string outerPaymentMessage, string innerCauseMessage) = RunInnerExceptionWrap(validOrder.Total); // SECTION 12
        InsufficientFundsException fundsSample = new InsufficientFundsException(250m, walletBalance);
        (string sampleMessage, string sampleStackFirstLine, string sampleInner, string sampleSource) =
            ReadExceptionProperties(new InvalidOrderException("ORD-77", "Property demo")); // SECTION 13
        (string timeoutFilterResult, string amountFilterResult, string fallbackResult) =
            RunCatchWhenFilters(); // SECTION 14 — when filters
        string auditTrail = RunUsingDisposalPreview(validOrder.OrderId); // SECTION 15 — using preview
        (string swallowDemo, string flowControlGood) = RunAntiPatternDemos(invalidQuantityOrder); // SECTION 16

        Console.WriteLine("=== Exception Handling — Order & Payment ===");
        Console.WriteLine();
        Console.WriteLine($"Customer: {customerName} | Wallet: {walletBalance:C}");
        Console.WriteLine($"Valid order {validOrder.OrderId}: qty {validOrder.Quantity}, total {validOrder.Total:C}");
        Console.WriteLine($"Validation: {validationOutcome} | finally on success: {finallyOnSuccess}");
        Console.WriteLine();
        Console.WriteLine($"Hierarchy — custom is SystemException: {customIsSystemException}, " +
            $"FormatException is SystemException: {formatIsSystemException}, custom is Exception: {customIsExceptionRoot}");
        Console.WriteLine($"Invalid qty catch: {invalidOrderMessage}");
        Console.WriteLine($"Parse '{badQuantityText}' catch: {parseFailureMessage}");
        Console.WriteLine($"Payment declined: {paymentDeclinedMessage}");
        Console.WriteLine($"finally after catch: {finallyAfterCatch}");
        Console.WriteLine();
        Console.WriteLine($"Rethrow preserved message: {rethrowMessage}");
        Console.WriteLine($"Rethrow stack (first line): {rethrowStackHint}");
        Console.WriteLine($"Gateway wrap — outer: {outerPaymentMessage}");
        Console.WriteLine($"Gateway wrap — inner: {innerCauseMessage}");
        Console.WriteLine();
        Console.WriteLine($"Custom type: {fundsSample.GetType().Name} | Uncharged wallet: {walletBalance:C}");
        Console.WriteLine($"Properties — Message: {sampleMessage}");
        Console.WriteLine($"Properties — Stack (first line): {sampleStackFirstLine}");
        Console.WriteLine($"Properties — Inner: {sampleInner} | Source: {sampleSource}");
        Console.WriteLine();
        Console.WriteLine($"catch when — timeout: {timeoutFilterResult}");
        Console.WriteLine($"catch when — amount filter skipped: {amountFilterResult}");
        Console.WriteLine($"catch when — general fallback: {fallbackResult}");
        Console.WriteLine($"using preview audit: {auditTrail}");
        Console.WriteLine();
        Console.WriteLine($"Anti-pattern swallow demo returned: {swallowDemo}");
        Console.WriteLine($"Preferred validation (no throw): {flowControlGood}");
    }

    /*
     * SECTION 6: try / catch / finally — BASIC STRUCTURE
     *
     *   try     — encloses statements that might throw
     *   catch   — runs only when a matching exception type is thrown inside try
     *   finally — runs after try completes OR after catch runs (always)
     *
     * When no exception occurs, catch blocks are skipped; finally still runs.
     *
     * Execution order examples:
     *   success:  try → finally
     *   handled:  try → catch → finally
     *   unhandled: try → finally → exception propagates (catch did not match)
     */
    private static (string Outcome, bool FinallyRan) RunTryCatchFinallySuccess(Order order)
    {
        string outcome = "unknown";
        bool finallyRan = false;

        try
        {
            order.Validate(); // no throw — catch skipped
            outcome = "valid";
        }
        catch (InvalidOrderException ex) // runs only when Validate throws matching type
        {
            outcome = "invalid: " + ex.Message;
        }
        finally
        {
            finallyRan = true; // always runs — success or handled exception
        }

        return (outcome, finallyRan);
    }

    /*
     * SECTION 7: EXCEPTION HIERARCHY
     *
     * All exceptions derive from System.Exception:
     *
     *   Exception                          (root — catch-all base)
     *     ├── SystemException              (CLR / BCL runtime failures)
     *     │     ├── NullReferenceException
     *     │     ├── ArgumentException
     *     │     ├── FormatException        (Parse failures — Chapter 05)
     *     │     └── ...
     *     └── Application-defined types      (InvalidOrderException, InsufficientFundsException)
     *
     *   ex is InvalidOrderException  → true for our custom type
     *   ex is SystemException        → false for custom types
     *   ex is Exception              → true for every exception
     */
    private static (bool CustomIsSystemException, bool FormatIsSystemException, bool CustomIsExceptionRoot)
        InspectExceptionHierarchy()
    {
        Exception customAsBase = new InvalidOrderException("X", "sample");
        Exception formatAsBase = new FormatException("bad number format");

        bool customIsSystemException = customAsBase is SystemException; // false — app types are not SystemException
        bool formatIsSystemException = formatAsBase is SystemException; // true — BCL runtime failure branch
        bool customIsExceptionRoot = customAsBase is Exception;           // true for every exception type

        return (customIsSystemException, formatIsSystemException, customIsExceptionRoot);
    }

    /*
     * SECTION 8: MULTIPLE catch BLOCKS — SPECIFIC → GENERAL
     *
     * Order catch clauses from MOST SPECIFIC to MOST GENERAL. The first matching
     * type wins. A catch for a base type before a derived type causes CS0160
     * (unreachable catch) at compile time.
     *
     *   catch (InvalidOrderException ex) { }   // business rule
     *   catch (FormatException ex) { }          // parse failure
     *   catch (Exception ex) { }                // safety net — last resort
     *
     * --- 8a. Invalid order (custom exception) ---
     */
    private static string RunMultipleCatchInvalidOrder(Order order)
    {
        string message = string.Empty;

        try
        {
            order.Validate();
        }
        catch (InvalidOrderException ex) // most specific — business rule failure
        {
            message = ex.Message;
        }
        catch (FormatException ex) // unreachable for Validate — shown for ordering pattern
        {
            message = "format: " + ex.Message;
        }
        catch (Exception ex) // general safety net — must be last
        {
            message = "general: " + ex.Message;
        }

        return message;
    }

    /*
     * --- 8b. FormatException from int.Parse on bad input ---
     *
     * int.Parse throws FormatException (a SystemException) when the string
     * is not a valid integer. Prefer int.TryParse for user input (Chapter 05).
     */
    private static string RunMultipleCatchParse(string text)
    {
        string message = string.Empty;

        try
        {
            int parsedQuantity = int.Parse(text, CultureInfo.InvariantCulture); // throws FormatException when text is not an int
            _ = parsedQuantity; // discard — value only needed to exercise Parse
        }
        catch (InvalidOrderException ex)
        {
            message = ex.Message;
        }
        catch (FormatException ex) // matches int.Parse failure on "abc"
        {
            message = ex.Message;
        }
        catch (Exception ex)
        {
            message = ex.Message;
        }

        return message;
    }

    /*
     * SECTION 9: finally BLOCK BEHAVIOR
     *
     * finally executes:
     *   • after try completes normally
     *   • after a catch handles an exception
     *   • even if try or catch uses return (unless the process aborts abruptly)
     *
     * Use finally (or using / IDisposable) for cleanup: close files, release
     * locks, reset flags. Do not rely on catch for cleanup — catch may not run
     * when no exception occurs or when the exception type does not match.
     */
    private static (string DeclinedMessage, bool FinallyRan) RunFinallyWhenCatchRuns(
        decimal balance,
        decimal amount)
    {
        string declinedMessage = string.Empty;
        bool finallyRan = false;

        try
        {
            ProcessPayment(balance, amount); // throws when amount > balance
        }
        catch (InsufficientFundsException ex)
        {
            declinedMessage = ex.Message;
        }
        finally
        {
            finallyRan = true; // runs even though catch handled the exception
        }

        return (declinedMessage, finallyRan);
    }

    /*
     * SECTION 10: throw — SIGNALING ERRORS FROM CALLEES
     *
     * ProcessPayment throws InsufficientFundsException when amount exceeds balance.
     * Callers catch the specific type to show a user-friendly decline message.
     */
    private static void ProcessPayment(decimal balance, decimal amount)
    {
        if (amount > balance)
        {
            throw new InsufficientFundsException(amount, balance); // signals decline — caller catches specific type
        }
    }

    /*
     * SECTION 11: throw AND rethrow — PRESERVE STACK TRACE
     *
     * Inside catch you can:
     *
     *   throw;           — rethrows the SAME exception; stack trace preserved
     *   throw ex;        — rethrows but RESETS stack trace to this line (avoid)
     *   throw new ...    — wrap in a new exception (often with inner — Section 12)
     *
     * throw; is legal only inside a catch block — LogAndRethrow wraps that pattern.
     */
    private static (string Message, string StackFirstLine) RunRethrowPreservesStackTrace()
    {
        string message = string.Empty;
        string stackFirstLine = string.Empty;

        try
        {
            LogAndRethrow(new InvalidOrderException("ORD-9", "rethrow demo"));
        }
        catch (InvalidOrderException ex) // outer catch receives rethrown exception
        {
            message = ex.Message;
            stackFirstLine = GetStackTraceFirstLine(ex); // stack should still point at original throw site
        }

        return (message, stackFirstLine);
    }

    private static void LogAndRethrow(InvalidOrderException ex)
    {
        try
        {
            throw ex; // anti-pattern demo — throw ex; resets stack trace to this line
        }
        catch (InvalidOrderException)
        {
            _ = ex.Message; // simulate logging
            throw; // bare throw — rethrows current exception; preserves stack trace
        }
    }

    /*
     * SECTION 12: INNER EXCEPTION — WRAPPING LOWER-LEVEL FAILURES
     *
     * When a high-level operation fails because of a lower-level error, wrap
     * the original via the innerException constructor parameter:
     *
     *   throw new InvalidOrderException(id, "Payment failed.", inner);
     *
     * Consumers read ex.InnerException for the root cause (timeout, I/O, etc.).
     */
    private static (string OuterMessage, string InnerMessage) RunInnerExceptionWrap(decimal total)
    {
        string outerMessage = string.Empty;
        string innerMessage = string.Empty;

        try
        {
            ChargeViaSimulatedGateway(total);
        }
        catch (InvalidOrderException ex)
        {
            outerMessage = ex.Message;
            innerMessage = ex.InnerException?.Message ?? string.Empty; // ?. and ?? — no inner → empty string
        }

        return (outerMessage, innerMessage);
    }

    private static void ChargeViaSimulatedGateway(decimal total)
    {
        try
        {
            throw new TimeoutException("Payment gateway did not respond within 30 seconds.");
        }
        catch (TimeoutException inner)
        {
            throw new InvalidOrderException("ORD-GW", $"Could not charge {total:C}.", inner); // inner links root cause
        }
    }

    /*
     * SECTION 13: EXCEPTION PROPERTIES
     *
     *   Message         — human-readable description (safe for logs / API clients)
     *   StackTrace      — call stack at throw site (debugging only — not for end users)
     *   InnerException  — wrapped cause, if any (walk the chain for root cause)
     *   Source          — assembly name when set by the thrower
     *
     * Log StackTrace and InnerException for support engineers; never expose raw
     * stack traces in production UI (security and UX risk).
     */
    private static (string Message, string StackFirstLine, string Inner, string Source)
        ReadExceptionProperties(Exception ex)
    {
        string message = ex.Message;
        string stackFirstLine = GetStackTraceFirstLine(ex);
        string inner = ex.InnerException?.Message ?? "(none)";
        string source = ex.Source ?? "(not set)"; // assembly name when thrower sets Source

        return (message, stackFirstLine, inner, source);
    }

    private static string GetStackTraceFirstLine(Exception ex)
    {
        string? trace = ex.StackTrace;
        if (string.IsNullOrEmpty(trace))
        {
            return "(empty — optimized builds may omit)";
        }

        int newline = trace.IndexOf('\n');
        return newline >= 0 ? trace[..newline].Trim() : trace.Trim(); // range [..newline] — first line only
    }

    /*
     * SECTION 14: catch when EXCEPTION FILTERS
     *
     * Syntax: catch (ExceptionType ex) when (bool expression)
     *
     * Evaluation order for each catch clause in source order:
     *   1. Does the thrown exception match the catch type?
     *   2. If yes, evaluate the when filter — must be true to enter the handler
     *   3. If the filter is false, skip this catch and try the next clause
     *
     * Multiple catches for the SAME type differ only by when — common for HTTP
     * status codes, error codes, or message substrings without duplicating handler bodies.
     *
     * | Rule / pitfall                    | Detail                                      |
     * |-----------------------------------|---------------------------------------------|
     * | Filter can use ex                 | ex.Message, ex.HResult, custom properties   |
     * | Filter runs only if type matches  | FormatException filter not run for Timeout  |
     * | Avoid heavy work in when          | Filter runs on every matching candidate     |
     * | Do not throw from when            | Undefined / replaces original exception     |
     * | Specific type catches still first | when refines handlers for the same type     |
     *
     * --- 14a. Same Exception type, different when filters ---
     * --- 14b. when false → falls through to the next matching catch ---
     */
    private static (string TimeoutResult, string AmountResult, string FallbackResult) RunCatchWhenFilters()
    {
        string timeoutResult = RunCatchWhenOnException(
            new Exception("gateway timeout after 30s"),
            "none");

        string amountResult = RunCatchWhenOnException(
            new InsufficientFundsException(500m, 120m),
            "none");

        string fallbackResult = RunCatchWhenOnException(
            new Exception("unknown service error"),
            "none");

        return (timeoutResult, amountResult, fallbackResult);
    }

    private static string RunCatchWhenOnException(Exception toThrow, string defaultLabel)
    {
        string label = defaultLabel;

        try
        {
            throw toThrow;
        }
        catch (Exception ex) when (ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase)) // filter runs only if type matches
        {
            label = "timeout filter matched";
        }
        catch (InsufficientFundsException ex) when (ex.RequestedAmount > 1000m) // same type, stricter when — evaluated second
        {
            label = "high-amount funds filter: " + ex.RequestedAmount;
        }
        catch (InsufficientFundsException ex) // when false above — falls through to this handler
        {
            label = "standard funds handler: " + ex.AvailableBalance;
        }
        catch (Exception)
        {
            label = "general catch";
        }

        return label;
    }

    /*
     * SECTION 15: using STATEMENT — DETERMINISTIC CLEANUP (PREVIEW)
     *
     * See Section 4 (AuditLogWriter). Demonstrates Dispose running via compiler-generated finally.
     */
    private static string RunUsingDisposalPreview(string orderId)
    {
        string auditTrail = string.Empty;

        using (AuditLogWriter audit = new AuditLogWriter()) // compiler expands to try/finally calling Dispose
        {
            audit.WriteEntry($"Payment attempt for {orderId}");
            auditTrail = audit.LastEntry;
        } // Dispose runs here even if WriteEntry threw

        return auditTrail;
    }

    /*
     * SECTION 16: ANTI-PATTERNS
     *
     * --- 16a. Swallowing exceptions (empty catch) ---
     *   catch (Exception) { }    // BAD — hides failures; log and rethrow or handle
     *
     * --- 16b. Catch-all before specific ---
     *   catch (Exception) before catch (InvalidOrderException) → CS0160 unreachable
     *
     * --- 16c. Exceptions as control flow ---
     *   BAD:  throw for a missing optional item callers expect often
     *   GOOD: return null / bool TryGet / result string for expected outcomes
     *
     * --- 16d. throw ex; vs throw; ---
     *   throw ex; resets stack trace — loses original throw site (Section 11)
     */
    private static (string SwallowDemo, string FlowControlGood) RunAntiPatternDemos(Order invalidOrder)
    {
        string swallowDemo = DemonstrateSwallowAntiPattern();
        string flowControlGood = ValidateWithResult(invalidOrder);

        return (swallowDemo, flowControlGood);
    }

    private static string DemonstrateSwallowAntiPattern()
    {
        try
        {
            throw new InvalidOperationException("transient glitch");
        }
        catch (InvalidOperationException)
        {
            return "swallowed — error hidden from caller (do not do this in production)"; // empty catch body anti-pattern
        }
    }

    private static string ValidateWithResult(Order order)
    {
        if (order.Quantity <= 0)
        {
            return $"Order {order.OrderId}: quantity must be positive (no exception thrown)."; // expected flow — return, not throw
        }

        return $"Order {order.OrderId}: OK";
    }
}

/*
 * QUICK REFERENCE — EXCEPTION HANDLING
 *
 * --- Structure ---
 *
 *  try { ... }
 *  catch (SpecificEx ex) { ... }              // most specific first
 *  catch (Exception ex) when (condition) { }  // same type, filtered handler
 *  catch (Exception ex) { ... }               // general last
 *  finally { ... }                            // always runs
 *
 * --- throw / rethrow ---
 *
 *  throw new MyException("msg");              // signal failure
 *  throw;                                     // rethrow current — keeps stack trace
 *  throw ex;                                  // avoid — resets stack trace
 *
 * --- wrapping ---
 *
 *  throw new MyException("outer", innerException);
 *  ex.InnerException                          // root cause
 *
 * --- Key properties ---
 *
 *  ex.Message                                 // user/log summary
 *  ex.StackTrace                              // debug only
 *  ex.InnerException                          // wrapped cause
 *
 * --- catch when ---
 *
 *  Type must match before when is evaluated
 *  when (false) → try next catch clause
 *  Multiple catches, same type, different when → distinct handlers
 *
 * --- Anti-patterns ---
 *
 *  Mistake                          | Result
 *  ---------------------------------|----------------------------------
 *  Empty catch { }                  | Silent data loss / hidden bugs
 *  catch (Exception) before specific| CS0160 unreachable catch
 *  throw ex;                        | Lost original stack trace
 *  Exceptions for expected flow     | Slow, hard to read — use return/Try*
 *  Showing StackTrace to end users  | Security / UX risk — log internally
 */
