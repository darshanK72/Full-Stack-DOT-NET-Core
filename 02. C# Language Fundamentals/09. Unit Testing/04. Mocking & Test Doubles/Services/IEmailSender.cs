namespace MockingAndTestDoubles.Services;

/*
 * SECTION 3b: EMAIL BOUNDARY — THE NOTIFICATION SEAM
 *
 * Production might use SMTP or a cloud queue. Tests substitute FakeEmailSender
 * (inspect SentMessages) or Mock<IEmailSender> (Verify SendOrderConfirmation).
 *
 * Side-effect-only method — no return value — so interaction verification (mock/spy)
 * is the primary way to assert "email was sent" when not using a fake.
 */
public interface IEmailSender
{
    void SendOrderConfirmation(string email, int orderId, decimal total);
}
