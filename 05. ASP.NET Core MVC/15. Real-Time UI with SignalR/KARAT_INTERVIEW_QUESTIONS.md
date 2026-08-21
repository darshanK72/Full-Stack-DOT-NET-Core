# Karat — Interview Questions

> **Folder:** `06. ASP.NET Core MVC/15. Real-Time UI with SignalR`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) Review this live auction hub. Bidding works in dev with one user; under load, bids from one user appear on another user's screen and `InvalidOperationException` surfaces in logs about scoped services.

```csharp
public class AuctionHub : Hub
{
    private readonly IAuctionService _auction; // registered Scoped
    private static readonly Dictionary<string, decimal> _highBids = new();

    public AuctionHub(IAuctionService auction) => _auction = auction;

    public async Task PlaceBid(string lotId, decimal amount)
    {
        _highBids[lotId] = amount;
        await _auction.RecordBidAsync(lotId, amount, Context.UserIdentifier);
        await Clients.Others.SendAsync("BidPlaced", lotId, amount);
    }
}
```

`Program.cs`: `builder.Services.AddScoped<IAuctionService, AuctionService>();` and `app.MapHub<AuctionHub>("/hubs/auction");`

---

#### Q2. (P) You deploy four MVC instances behind an Azure Application Gateway. Users on instance 2 never receive chat messages sent from a controller on instance 4. Walk through **Redis backplane** registration for SignalR — what it synchronizes, what it does not, and one production misconfiguration that silently breaks fan-out.

---

#### Q3. (D) Ops proposes **sticky sessions (session affinity)** on the load balancer instead of a SignalR backplane to save Redis cost. The app is a stock ticker dashboard with MVC Razor views and a SignalR hub. What works with sticky sessions alone, what fails on instance recycle or deploy, and when is affinity acceptable vs when is backplane or Azure SignalR mandatory?

---

#### Q4. (R) Review hub authorization. Authenticated users on the MVC site can open `/Dashboard`, but the SignalR connection succeeds as anonymous and `Context.User.Identity.Name` is null in hub methods.

```csharp
// ChatHub.cs — no attributes on class
public class ChatHub : Hub
{
    public async Task SendMessage(string room, string text)
    {
        var user = Context.User.Identity!.Name;
        await Clients.Group(room).SendAsync("ReceiveMessage", user, text);
    }
}

// Program.cs
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();
builder.Services.AddSignalR();
var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<ChatHub>("/hubs/chat");
app.MapControllerRoute(/* ... */);
```

*(Browser connects with `@microsoft/signalr` from a Razor view; no `accessTokenFactory` or cookies on negotiate.)*

---

#### Q5. (R) Review group membership wiring. Some users never receive room messages; logs show `OnConnectedAsync` completed but `Groups.AddToGroupAsync` ran after the client already called `JoinRoom`.

```csharp
public class RoomHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var room = Context.GetHttpContext()?.Request.Query["room"].ToString();
        if (!string.IsNullOrEmpty(room))
            await Groups.AddToGroupAsync(Context.ConnectionId, room);
        await base.OnConnectedAsync();
    }

    public Task JoinRoom(string room) =>
        Groups.AddToGroupAsync(Context.ConnectionId, room);
}
```

```javascript
// client.js — runs immediately after connection.start()
connection.on("ReceiveMessage", (user, msg) => render(user, msg));
connection.invoke("JoinRoom", "support-42");
connection.start();
```

---

#### Q6. (P) After a brief network blip, the MVC dashboard reconnects automatically but the user's watchlist group membership and server-side "last seen price" state are gone until they refresh the page. Explain **what SignalR resets on reconnect**, what the server must rehydrate, and a production pattern to restore group membership without a full page reload.

---

#### Q7. (R) Review broadcasting from an MVC controller after an admin action. The notification never reaches connected browsers; no exception is thrown.

```csharp
public class OrdersController : Controller
{
    private readonly OrderHub _hub; // concrete hub injected

    public OrdersController(OrderHub hub) => _hub = hub;

    [HttpPost]
    public async Task<IActionResult> Ship(int id)
    {
        await _orders.MarkShippedAsync(id);
        await _hub.Clients.All.SendAsync("OrderShipped", id);
        return RedirectToAction("Index");
    }
}

public class OrderHub : Hub { }

// Program.cs
builder.Services.AddSignalR();
// no registration for OrderHub as a service
```

---

#### Q8. (R) A separate SPA on `https://app.example.com` calls your MVC site's SignalR hub at `https://api.example.com/hubs/notifications`. Browser console shows CORS error on the negotiate request; direct WebSocket works in Postman.

```csharp
builder.Services.AddCors(o => o.AddPolicy("Spa", p =>
    p.WithOrigins("https://app.example.com")
     .AllowAnyHeader()
     .AllowAnyMethod()));

var app = builder.Build();
app.UseCors("Spa");
app.MapHub<NotificationHub>("/hubs/notifications");
```

*(SPA client uses `withUrl("https://api.example.com/hubs/notifications")` without credentials.)*

---

#### Q9. (P) Traffic grows to 50k concurrent connections across regions. Team evaluates **Azure SignalR Service** vs self-hosted hubs with Redis backplane on the MVC app. Compare connection ownership, deployment model (`AddAzureSignalR`), sticky-session requirements, and one scenario where Azure SignalR simplifies ops but changes how you broadcast from controllers.

---

#### Q10. (R) Review hub method error handling. Clients receive a generic connection drop; server logs show unhandled exceptions inside hub methods.

```csharp
public class TradeHub : Hub
{
    public async Task SubmitOrder(OrderDto order)
    {
        try
        {
            var result = await _trading.ExecuteAsync(order);
            await Clients.Caller.SendAsync("OrderAccepted", result);
        }
        catch (ValidationException ex)
        {
            throw new Exception(ex.Message); // rethrow to client
        }
        catch (InsufficientFundsException)
        {
            await Clients.Caller.SendAsync("OrderRejected", "Insufficient funds");
        }
    }
}
```

*(No `HubException`, no `IHubFilter`, default SignalR error pipeline.)*

---

#### Q11. (R) Review MVC Razor view and client wiring for a live comment feed. Connection fails in staging (HTTPS, subpath deploy) but works locally on HTTP.

```html
<!-- Views/Comments/Index.cshtml -->
@section Scripts {
    <script src="~/lib/microsoft/signalr/dist/browser/signalr.min.js"></script>
    <script>
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/hubs/comments")
            .build();

        connection.on("NewComment", (c) => appendComment(c));
        connection.start();

        document.getElementById("postBtn").onclick = () =>
            connection.invoke("PostComment", document.getElementById("text").value);
    </script>
}
```

*(App deployed at `https://intranet.corp/apps/comments/` behind a reverse proxy; `PathBase` is configured in `Program.cs`; antiforgery required on form posts elsewhere on the site.)*
