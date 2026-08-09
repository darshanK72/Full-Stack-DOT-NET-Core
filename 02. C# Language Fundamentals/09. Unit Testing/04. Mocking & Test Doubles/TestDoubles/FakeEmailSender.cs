using System.Collections.Generic;
using MockingAndTestDoubles.Services;

namespace MockingAndTestDoubles.TestDoubles;

/*
 * SECTION 6: FAKE — WORKING SIMPLIFIED IMPLEMENTATION
 *
 * A fake implements enough real behavior to use in tests without external systems.
 * FakeEmailSender is an in-memory IEmailSender that appends each confirmation to
 * SentMessages so you can assert state after PlaceOrder — no Verify required.
 *
 *   Stub vs fake:
 *     Stub  → returns fixed values; often no meaningful internal state
 *     Fake  → simplified but functional; inspect SentMessages.Count or contents
 *
 * Fakes shine when verifying output state is easier than verifying method calls.
 * When you only care that SendOrderConfirmation happened once with exact args,
 * Moq.Verify is often clearer — see OrderServiceMoqTests.cs.
 */
public sealed class FakeEmailSender : IEmailSender
{
    public IList<(string Email, int OrderId, decimal Total)> SentMessages { get; } = [];

    public void SendOrderConfirmation(string email, int orderId, decimal total) =>
        SentMessages.Add((email, orderId, total));
}
