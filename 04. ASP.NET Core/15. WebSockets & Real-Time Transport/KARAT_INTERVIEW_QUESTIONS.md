# Karat — Interview Questions

> **Folder:** `05. ASP.NET Core/15. WebSockets & Real-Time Transport`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)  
> **Raw debrief:** [KARAT_DEBRIEF_SOURCE.txt](../../00. Notes & Practice/04. Citi Karat Interview Practice/KARAT_DEBRIEF_SOURCE.txt)

---

#### Q1. (M) A raw WebSocket endpoint returns 404 on upgrade in production but works locally. Review middleware order — where must `UseWebSockets()` sit relative to routing, authentication, and terminal middleware?

---

#### Q2. (P) A dashboard opens a WebSocket per browser tab and keeps it open for hours. What server-side resources are tied to connection lifetime, and how do you detect stale connections and prevent unbounded memory growth?

---

#### Q3. (D) Product needs live order-status updates to web and mobile clients. Compare **SignalR** vs **raw WebSocket** for this scenario — protocol, reconnection, scale-out, and team velocity.

---

#### Q4. (P) You deploy three API instances behind a load balancer. WebSocket clients connected to instance A never receive events raised on instance B. Preview how a **SignalR backplane** (Redis/Azure Service Bus) solves this and what still breaks if you use raw WebSockets without shared state.

---

#### Q5. (R) Review this WebSocket endpoint. Anonymous clients can connect and impersonate any user id sent in the first message. What is wrong?

```csharp
app.UseWebSockets();

app.Map("/ws/orders", async context =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return;
    }

    using var socket = await context.WebSockets.AcceptWebSocketAsync();
    var buffer = new byte[4 * 1024];

    while (socket.State == WebSocketState.Open)
    {
        var result = await socket.ReceiveAsync(buffer, CancellationToken.None);
        if (result.MessageType == WebSocketMessageType.Text)
        {
            var msg = JsonSerializer.Deserialize<OrderSubscribe>(buffer.AsSpan(0, result.Count));
            _hub.Register(msg!.UserId, socket); // static registry
            await SendOpenOrdersAsync(socket, msg.UserId);
        }
    }
});
```

*(Authentication middleware is registered globally but JWT is only sent on the initial HTTP upgrade request.)*

---

#### Q6. (M) Clients send large JSON payloads over WebSocket. What limits does ASP.NET Core/Kestrel impose on request/upgrade body sizes and individual WebSocket frames, and how do you enforce application-level max message size safely?

---

#### Q7. (R) Review this broadcast loop inside a WebSocket handler. Under 2k connections, CPU spikes and slow clients block everyone. What are the problems?

```csharp
private static readonly ConcurrentDictionary<string, WebSocket> _clients = new();

while (socket.State == WebSocketState.Open)
{
    var result = await socket.ReceiveAsync(buffer, CancellationToken.None);
    if (result.MessageType == WebSocketMessageType.Text)
    {
        var tick = JsonSerializer.Deserialize<PriceTick>(buffer.AsSpan(0, result.Count));
        foreach (var kv in _clients)
        {
            var payload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(tick));
            await kv.Value.SendAsync(payload, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
```

*(Assume `_clients` adds each accepted socket on connect.)*
