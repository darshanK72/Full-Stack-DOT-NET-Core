namespace MockingAndTestDoubles.Models;

/*
 * SECTION 2b: ORDER RESULT — OUTCOME OF PLACE ORDER
 *
 * Factory methods keep success and failure construction consistent across tests.
 * Assert on Success + OrderId for happy path; FailureReason when stock is insufficient.
 */
public sealed class OrderResult
{
    public bool Success { get; init; }
    public int OrderId { get; init; }
    public string FailureReason { get; init; } = string.Empty;

    public static OrderResult Succeeded(int orderId) =>
        new() { Success = true, OrderId = orderId };

    public static OrderResult Failed(string reason) =>
        new() { Success = false, FailureReason = reason };
}
