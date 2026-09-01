using Microsoft.AspNetCore.Mvc;
using RealTimeSignalR.Services;
using System.Threading.Tasks;

/*
 * FILE ROLE: API controller that triggers server-to-client SignalR pushes in
 *            response to HTTP requests, delegating to NotificationService.
 *
 * SECTIONS IN THIS FILE:
 *   1. NotificationsController — triggering a push from an HTTP action via IHubContext
 */

namespace RealTimeSignalR.Controllers;

/*
 * SECTION 1: TRIGGERING A SIGNALR PUSH FROM A CONTROLLER ACTION
 *
 * Pattern: HTTP Request → Controller Action → Service → IHubContext → SignalR Clients
 *
 * The controller does not inject IHubContext<THub> directly; it delegates to
 * NotificationService, which owns that dependency.  Benefits:
 *   - Controller stays thin and focused on HTTP concerns
 *   - NotificationService is reusable from background jobs, webhooks, etc.
 *   - Easier to unit-test: mock NotificationService without mocking SignalR
 *
 * Alternative — inject IHubContext<THub> directly into the controller:
 *   public NotificationsController(IHubContext<NotificationHub> hub) { _hub = hub; }
 *   await _hub.Clients.All.SendAsync("ReceiveNotification", message);
 *   This is fine for simple cases; the service wrapper scales better.
 *
 * Route: [Route("api/[controller]")] → api/notifications
 *   POST api/notifications/broadcast         — push to all clients
 *   POST api/notifications/channel/{channel} — push to a named group
 */
[Route("api/[controller]")]
public sealed class NotificationsController : Controller
{
    private readonly NotificationService _notificationService;

    public NotificationsController(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    // POST api/notifications/broadcast
    // Body: "Your server announcement here"  (Content-Type: application/json)
    [HttpPost("broadcast")]
    public async Task<IActionResult> Broadcast([FromBody] string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return BadRequest("Message cannot be empty.");

        await _notificationService.BroadcastAsync(message);    // pushes to ALL SignalR clients
        return Ok(new { sent = true, message });                // 200 OK with confirmation payload
    }

    // POST api/notifications/channel/{channel}
    // Body: "Message for subscribers only"
    [HttpPost("channel/{channel}")]
    public async Task<IActionResult> SendToChannel(string channel, [FromBody] string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return BadRequest("Message cannot be empty.");

        await _notificationService.SendToChannelAsync(channel, message);   // pushes to group members only
        return Ok(new { sent = true, channel, message });
    }
}
