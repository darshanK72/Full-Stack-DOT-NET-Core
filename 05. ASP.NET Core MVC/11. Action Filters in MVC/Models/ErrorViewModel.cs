/*
 * FILE ROLE: ErrorViewModel — the strongly-typed model used by MvcExceptionFilter
 *   to pass error information to the Views/Shared/Error.cshtml Razor view.
 *   Keeping a dedicated error model keeps the Error view strongly typed, enabling
 *   IntelliSense and compile-time validation in the view.
 *
 * SECTIONS IN THIS FILE:
 *   SECTION 10 — ErrorViewModel: RequestId, Message, ShowRequestId helper,
 *                used by MvcExceptionFilter via ViewDataDictionary<ErrorViewModel>
 */

namespace ActionFiltersMvc.Models;

/*
 * SECTION 10: ErrorViewModel — ERROR VIEW MODEL
 * ─────────────────────────────────────────────────────────────────────────────
 * This model is populated by MvcExceptionFilter.OnExceptionAsync() and passed
 * to the Error.cshtml view via ViewDataDictionary<ErrorViewModel>.
 *
 * RequestId is the ASP.NET Core distributed trace identifier (W3C TraceContext).
 * In production it links the user's error report to a specific log entry.
 * Expose it to the user only in development or when you have a support portal that
 * can look it up — in production, log it server-side and show only a generic message.
 *
 * The ShowRequestId helper property is a computed convenience so the Razor view
 * does not need to write @if(!string.IsNullOrEmpty(Model.RequestId)) inline.
 */
public sealed class ErrorViewModel
{
    // W3C TraceContext trace-id; links to structured logs via ILogger correlation
    public string? RequestId { get; set; }

    // Human-readable error description — sanitised before being shown to users
    public string Message { get; set; } = string.Empty;

    // Computed: true when RequestId should be shown (non-null/non-empty)
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
