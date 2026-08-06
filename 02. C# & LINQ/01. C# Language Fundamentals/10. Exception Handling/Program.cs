/*
 * =============================================================================
 * 10. EXCEPTION HANDLING IN C# — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: Structured error handling — detecting failures, responding without
 *        crashing, cleaning up resources, and surfacing meaningful errors.
 *
 * WHY IT MATTERS:
 *   Payment gateways time out, users enter invalid quantities, and account
 *   balances run dry. Uncaught exceptions terminate a process; poorly handled
 *   ones hide bugs. Production code uses try/catch/finally deliberately — catch
 *   what you can recover from, log what you cannot, and always release resources.
 *
 * WHAT YOU WILL LEARN:
 *   1.  try / catch / finally structure
 *   2.  Exception hierarchy (Exception, SystemException)
 *   3.  Multiple catch blocks — specific before general
 *   4.  finally block behavior (always runs)
 *   5.  throw and rethrow (preserve stack trace)
 *   6.  Custom exception classes (see Models/)
 *   7.  Inner exceptions — wrapping lower-level failures
 *   8.  Exception properties (Message, StackTrace, InnerException)
 *   9.  Preview: catch when filter
 *  10.  Preview: using statement for disposal
 *  11.  Exception-handling anti-patterns to avoid
 *
 * =============================================================================
 */

using System;
using System.Globalization;
using ExceptionHandling.Models;

namespace ExceptionHandling;

public class Program
{
    public static void Main(string[] args)
    {
        /*
         * =========================================================================
         * SECTION 1: EXCEPTION HANDLING OVERVIEW
         * =========================================================================
         *
         * An EXCEPTION is a runtime signal that something went wrong. Without
         * handling, it unwinds the call stack until the process crashes or a
         * top-level handler catches it.
         *
         * Structured handling wraps risky code in try, reacts in catch, and
         * cleans up in finally:
         *
         *   try
         *   {
         *       // code that may throw
         *   }
         *   catch (SpecificException ex)
         *   {
         *       // handle recoverable failure
         *   }
         *   finally
         *   {
         *       // runs whether or not an exception occurred
         *   }
         *
         * Scenario: validate an order, charge a wallet balance, and audit the
         * attempt. Values are fixed so the program runs without keyboard input.
         * -------------------------------------------------------------------------
         */

        string customerName = "Aisha Khan";
        decimal walletBalance = 180.00m;
        Order validOrder = new Order("ORD-4420", 2, 49.99m);
        Order invalidQuantityOrder = new Order("ORD-4421", 0, 29.99m);
        string badQuantityText = "abc";
        decimal chargeAmount = 199.50m;


        /*
         * =========================================================================
         * SECTION 2: try / catch / finally — BASIC STRUCTURE
         * =========================================================================
         *
         * try     — encloses statements that might throw.
         * catch   — runs only when a matching exception type is thrown inside try.
         * finally — runs after try completes OR after catch runs (always).
         *
         * --- 2a. Successful path (no exception) ---
         *
         * When no exception occurs, catch blocks are skipped; finally still runs.
         * -------------------------------------------------------------------------
         */

        string validationOutcome = "unknown";
        bool finallyRanOnSuccess = false;

        try
        {
            validOrder.Validate();
            validationOutcome = "valid";
        }
        catch (InvalidOrderException ex)
        {
            validationOutcome = "invalid: " + ex.Message;
        }
        finally
        {
            finallyRanOnSuccess = true;
        }


        /*
         * =========================================================================
         * SECTION 3: EXCEPTION HIERARCHY
         * =========================================================================
         *
         * All exceptions derive from System.Exception. Common branches:
         *
         *   Exception                          (root — catch-all base)
         *     ├── SystemException              (CLR / runtime failures)
         *     │     ├── NullReferenceException
         *     │     ├── ArgumentException
         *     │     ├── FormatException        (Parse failures)
         *     │     └── ...
         *     └── Application-defined types      (your InvalidOrderException, etc.)
         *
         * SystemException marks exceptions the runtime or BCL typically throws.
         * Custom business exceptions usually inherit directly from Exception
         * (ApplicationException exists but is discouraged for new code).
         *
         * --- 3a. Inspecting base types ---
         *
         *   ex is InvalidOrderException  → true for our custom type
         *   ex is SystemException        → false (custom types are not SystemException)
         *   ex is Exception              → true for every exception
         * -------------------------------------------------------------------------
         */

        Exception customAsBase = new InvalidOrderException("X", "sample");
        Exception formatAsBase = new FormatException("bad number format");
        bool customIsSystemException = customAsBase is SystemException;
        bool formatIsSystemException = formatAsBase is SystemException;
        bool customIsExceptionRoot = customAsBase is Exception;


        /*
         * =========================================================================
         * SECTION 4: MULTIPLE catch BLOCKS — SPECIFIC → GENERAL
         * =========================================================================
         *
         * Order catch clauses from MOST SPECIFIC to MOST GENERAL. The first
         * matching type wins; later catches for the same or base types are
         * unreachable (CS0160 compile error).
         *
         *   catch (InvalidOrderException ex) { }   // business rule
         *   catch (FormatException ex) { }          // parse failure
         *   catch (Exception ex) { }                // safety net
         *
         * --- 4a. Invalid order (custom exception) ---
         * -------------------------------------------------------------------------
         */

        string invalidOrderMessage = string.Empty;

        try
        {
            invalidQuantityOrder.Validate();
        }
        catch (InvalidOrderException ex)
        {
            invalidOrderMessage = ex.Message;
        }
        catch (FormatException ex)
        {
            invalidOrderMessage = "format: " + ex.Message;
        }
        catch (Exception ex)
        {
            invalidOrderMessage = "general: " + ex.Message;
        }


        /*
         * --- 4b. FormatException from int.Parse on bad input ---
         *
         * int.Parse throws FormatException (a SystemException) when the string
         * is not a valid integer — prefer TryParse for user input (Chapter 05).
         * -------------------------------------------------------------------------
         */

        string parseFailureMessage = string.Empty;

        try
        {
            int parsedQuantity = int.Parse(badQuantityText, CultureInfo.InvariantCulture);
            _ = parsedQuantity;
        }
        catch (InvalidOrderException ex)
        {
            parseFailureMessage = ex.Message;
        }
        catch (FormatException ex)
        {
            parseFailureMessage = ex.Message;
        }
        catch (Exception ex)
        {
            parseFailureMessage = ex.Message;
        }


        /*
         * =========================================================================
         * SECTION 5: finally BLOCK BEHAVIOR
         * =========================================================================
         *
         * finally executes:
         *   • after try completes normally
         *   • after a catch handles an exception
         *   • even if try or catch uses return (unless process aborts)
         *
         * Use finally (or using / IDisposable) for cleanup: close files, release
         * locks, reset flags. Do not rely on catch for cleanup — catch may not run.
         *
         * --- 5a. finally when catch runs ---
         * -------------------------------------------------------------------------
         */

        bool finallyAfterCatch = false;
        string paymentDeclinedMessage = string.Empty;

        try
        {
            ProcessPayment(walletBalance, chargeAmount);
        }
        catch (InsufficientFundsException ex)
        {
            paymentDeclinedMessage = ex.Message;
        }
        finally
        {
            finallyAfterCatch = true;
        }


        /*
         * =========================================================================
         * SECTION 6: throw — CREATING AND SIGNALING ERRORS
         * =========================================================================
         *
         * throw expr;  — aborts the current flow and propagates the exception.
         *
         * Use throw when a method cannot honor its contract:
         *   • validate arguments and throw ArgumentException
         *   • detect business rule violations and throw a domain exception
         *
         * ProcessPayment below throws InsufficientFundsException when balance
         * is too low — callers decide whether to catch, log, or let it bubble.
         * -------------------------------------------------------------------------
         */

        decimal remainingBalance = walletBalance;


        /*
         * =========================================================================
         * SECTION 7: throw AND rethrow — PRESERVE STACK TRACE
         * =========================================================================
         *
         * Inside catch you can:
         *
         *   throw;           — rethrows the SAME exception; stack trace preserved
         *   throw ex;        — rethrows but RESETS stack trace to this line (avoid)
         *   throw new ...    — wrap in a new exception (often with inner — Section 8)
         *
         * LogAndRethrow demonstrates throw; — the original call site stays visible
         * in StackTrace for debugging.
         * -------------------------------------------------------------------------
         */

        string rethrowSampleMessage = string.Empty;
        string rethrowStackHint = string.Empty;

        try
        {
            LogAndRethrow(new InvalidOrderException("ORD-9", "rethrow demo"));
        }
        catch (InvalidOrderException ex)
        {
            rethrowSampleMessage = ex.Message;
            rethrowStackHint = GetStackTraceFirstLine(ex);
        }


        /*
         * =========================================================================
         * SECTION 8: INNER EXCEPTION — WRAPPING LOWER-LEVEL FAILURES
         * =========================================================================
         *
         * When a high-level operation fails because of a lower-level error,
         * wrap the original in a new exception via the innerException constructor:
         *
         *   throw new InvalidOrderException(id, "Payment failed.", inner);
         *
         * Consumers read ex.InnerException for the root cause (timeout, I/O, etc.).
         * -------------------------------------------------------------------------
         */

        string innerCauseMessage = string.Empty;
        string outerPaymentMessage = string.Empty;

        try
        {
            ChargeViaSimulatedGateway(validOrder.Total);
        }
        catch (InvalidOrderException ex)
        {
            outerPaymentMessage = ex.Message;
            innerCauseMessage = ex.InnerException?.Message ?? string.Empty;
        }


        /*
         * =========================================================================
         * SECTION 9: CUSTOM EXCEPTION CLASSES
         * =========================================================================
         *
         * Domain-specific types (Models/InvalidOrderException.cs,
         * Models/InsufficientFundsException.cs) make catch blocks readable and
         * carry extra context (OrderId, amounts).
         *
         * Guidelines:
         *   • inherit from Exception (not ApplicationException for new code)
         *   • provide message-only and message+inner constructors
         *   • add properties only when callers need structured data
         *   • do not use exceptions for normal control flow (Section 12)
         * -------------------------------------------------------------------------
         */

        InsufficientFundsException fundsSample = new InsufficientFundsException(250m, walletBalance);
        string customTypeName = fundsSample.GetType().Name;


        /*
         * =========================================================================
         * SECTION 10: EXCEPTION PROPERTIES — Message, StackTrace, InnerException
         * =========================================================================
         *
         *   Message         — human-readable description
         *   StackTrace      — call stack at throw site (debugging; not for users)
         *   InnerException  — wrapped cause, if any
         *   Source          — assembly that threw (when set)
         *
         * Show Message to users or API clients; log StackTrace and InnerException
         * for support engineers — never expose raw stack traces in production UI.
         * -------------------------------------------------------------------------
         */

        Exception propertySample = new InvalidOrderException("ORD-77", "Property demo");
        string sampleMessage = propertySample.Message;
        string sampleStackFirstLine = GetStackTraceFirstLine(propertySample);
        string sampleInner = propertySample.InnerException?.Message ?? "(none)";
        string sampleSource = propertySample.Source ?? "(not set)";


        /*
         * =========================================================================
         * SECTION 11: catch when FILTER (PREVIEW)
         * =========================================================================
         *
         * COVERED IN DETAIL LATER → advanced error-handling / pattern-matching topics
         *   (headline concepts only: conditional catch without duplicating logic)
         *
         * catch (Exception ex) when (ex.Message.Contains("timeout"))
         * runs only when the filter expression is true. Specific catches still
         * come first; filters refine which handler runs for the same type.
         * -------------------------------------------------------------------------
         */

        string filterCatchLabel = "none";
        Exception filterCandidate = new Exception("gateway timeout after 30s");

        try
        {
            throw filterCandidate;
        }
        catch (Exception ex) when (ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase))
        {
            filterCatchLabel = "timeout filter matched";
        }
        catch (Exception)
        {
            filterCatchLabel = "general catch";
        }


        /*
         * =========================================================================
         * SECTION 12: using STATEMENT FOR DISPOSAL (PREVIEW)
         * =========================================================================
         *
         * COVERED IN DETAIL LATER → OOP / IDisposable / C# 8 using declarations
         *   (headline concepts only: deterministic cleanup via Dispose())
         *
         * using (var resource = new AuditLogWriter()) { ... }
         * compiles to try/finally that calls Dispose() even when an exception
         * escapes the block — same guarantee as finally, less boilerplate.
         * -------------------------------------------------------------------------
         */

        string auditTrail = string.Empty;

        using (AuditLogWriter audit = new AuditLogWriter())
        {
            audit.WriteEntry($"Payment attempt for {validOrder.OrderId}");
            auditTrail = audit.LastEntry;
        }


        /*
         * =========================================================================
         * SECTION 13: EXCEPTION HANDLING ABUSE / ANTI-PATTERNS
         * =========================================================================
         *
         * --- 13a. Swallowing exceptions (empty catch) ---
         *
         *   catch (Exception) { }    // BAD — hides failures; use logging + rethrow
         *
         * --- 13b. Catch-all too early ---
         *
         *   catch (Exception ex) before catch (InvalidOrderException) → CS0160
         *   or worse: catching Exception and never handling specific recoverable cases.
         *
         * --- 13c. Exceptions as control flow ---
         *
         *   BAD:  throw new Exception("not found") for a missing optional item
         *   GOOD: return null / bool TryGet / result pattern for expected outcomes
         *
         * --- 13d. throw ex vs throw ---
         *
         *   throw ex; resets stack trace — loses original throw site (Section 7).
         *
         * --- 13d. Correct pattern demo ---
         *
         * ValidateWithResult returns a message instead of throwing for an expected
         * validation failure — exceptions reserved for exceptional paths.
         * -------------------------------------------------------------------------
         */

        string swallowedDemo = DemonstrateSwallowAntiPattern();
        string flowControlGood = ValidateWithResult(invalidQuantityOrder);
        string flowControlBadLabel = "throws on invalid (see catch above)";


        /*
         * =========================================================================
         * SECTION 14: PAYMENT SCENARIO SUMMARY (OUTPUT)
         * =========================================================================
         * Prints results from every section — no unused variables.
         * -------------------------------------------------------------------------
         */

        Console.WriteLine("=== Exception Handling — Order & Payment ===");
        Console.WriteLine();
        Console.WriteLine($"Customer: {customerName} | Wallet: {walletBalance:C}");
        Console.WriteLine($"Valid order {validOrder.OrderId}: qty {validOrder.Quantity}, total {validOrder.Total:C}");
        Console.WriteLine($"Validation: {validationOutcome} | finally on success: {finallyRanOnSuccess}");
        Console.WriteLine();
        Console.WriteLine($"Hierarchy — custom is SystemException: {customIsSystemException}, FormatException is SystemException: {formatIsSystemException}, custom is Exception: {customIsExceptionRoot}");
        Console.WriteLine($"Invalid qty catch: {invalidOrderMessage}");
        Console.WriteLine($"Parse '{badQuantityText}' catch: {parseFailureMessage}");
        Console.WriteLine($"Payment declined: {paymentDeclinedMessage}");
        Console.WriteLine($"finally after catch: {finallyAfterCatch}");
        Console.WriteLine();
        Console.WriteLine($"Rethrow preserved message: {rethrowSampleMessage}");
        Console.WriteLine($"Rethrow stack (first line): {rethrowStackHint}");
        Console.WriteLine($"Gateway wrap — outer: {outerPaymentMessage}");
        Console.WriteLine($"Gateway wrap — inner: {innerCauseMessage}");
        Console.WriteLine();
        Console.WriteLine($"Custom type: {customTypeName} | Remaining balance (uncharged): {remainingBalance:C}");
        Console.WriteLine($"Properties — Message: {sampleMessage}");
        Console.WriteLine($"Properties — Stack (first line): {sampleStackFirstLine}");
        Console.WriteLine($"Properties — Inner: {sampleInner} | Source: {sampleSource}");
        Console.WriteLine();
        Console.WriteLine($"catch when preview: {filterCatchLabel}");
        Console.WriteLine($"using preview audit: {auditTrail}");
        Console.WriteLine();
        Console.WriteLine($"Anti-pattern swallow demo returned: {swallowedDemo}");
        Console.WriteLine($"Preferred validation (no throw): {flowControlGood}");
        Console.WriteLine($"Exception path label: {flowControlBadLabel}");
    }

    /*
     * Throws when balance is insufficient — used by Sections 5–6.
     */
    private static void ProcessPayment(decimal balance, decimal amount)
    {
        if (amount > balance)
        {
            throw new InsufficientFundsException(amount, balance);
        }
    }

    /*
     * Simulates a low-level failure wrapped in InvalidOrderException (Section 8).
     */
    private static void ChargeViaSimulatedGateway(decimal total)
    {
        try
        {
            throw new TimeoutException("Payment gateway did not respond within 30 seconds.");
        }
        catch (TimeoutException inner)
        {
            throw new InvalidOrderException("ORD-GW", $"Could not charge {total:C}.", inner);
        }
    }

    /*
     * Logs and rethrows with throw; so StackTrace stays intact (Section 7).
     * throw; is legal only inside catch — hence the try/catch wrapper here.
     */
    private static void LogAndRethrow(InvalidOrderException ex)
    {
        try
        {
            throw ex;
        }
        catch (InvalidOrderException)
        {
            _ = ex.Message;
            throw;
        }
    }

    /*
     * Returns first line of stack trace for display (truncates long output).
     */
    private static string GetStackTraceFirstLine(Exception ex)
    {
        string? trace = ex.StackTrace;
        if (string.IsNullOrEmpty(trace))
        {
            return "(empty — optimize builds may omit)";
        }

        int newline = trace.IndexOf('\n');
        return newline >= 0 ? trace[..newline].Trim() : trace.Trim();
    }

    /*
     * Anti-pattern: catch and ignore — returns a label proving catch ran (Section 13).
     */
    private static string DemonstrateSwallowAntiPattern()
    {
        try
        {
            throw new InvalidOperationException("transient glitch");
        }
        catch (InvalidOperationException)
        {
            return "swallowed — error hidden from caller (do not do this in production)";
        }
    }

    /*
     * Preferred: expected validation failure as return value, not exception.
     */
    private static string ValidateWithResult(Order order)
    {
        if (order.Quantity <= 0)
        {
            return $"Order {order.OrderId}: quantity must be positive (no exception thrown).";
        }

        return $"Order {order.OrderId}: OK";
    }
}

/*
 * =============================================================================
 * QUICK REFERENCE — EXCEPTION HANDLING
 * =============================================================================
 *
 * --- Structure ---
 *
 *  try { ... }
 *  catch (SpecificEx ex) { ... }    // most specific first
 *  catch (Exception ex) { ... }     // general last
 *  finally { ... }                  // always runs
 *
 * --- throw / rethrow ---
 *
 *  throw new MyException("msg");    // signal failure
 *  throw;                           // rethrow current — keeps stack trace
 *  throw ex;                        // avoid — resets stack trace
 *
 * --- wrapping ---
 *
 *  throw new MyException("outer", innerException);
 *  ex.InnerException                // root cause
 *
 * --- Key properties ---
 *
 *  ex.Message                       // user/log summary
 *  ex.StackTrace                    // debug only
 *  ex.InnerException                // wrapped cause
 *
 * --- Preview syntax ---
 *
 *  catch (Ex ex) when (condition)   // filter — advanced chapter
 *  using (var r = new Disposable()) // calls Dispose in finally
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
 *  
 *  Different Types of Exceptions & Their Usage/Reasons
 *
 * =============================================================================
 */
