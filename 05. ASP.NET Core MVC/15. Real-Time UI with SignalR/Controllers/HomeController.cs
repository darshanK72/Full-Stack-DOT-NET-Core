using Microsoft.AspNetCore.Mvc;

/*
 * FILE ROLE: Serves the chat page view (Views/Home/Index.cshtml), which contains
 *            the JavaScript HubConnectionBuilder code and the live chat UI.
 *
 * SECTIONS IN THIS FILE:
 *   1. HomeController — returns the SignalR JavaScript client page
 */

namespace RealTimeSignalR.Controllers;

/*
 * SECTION 1: HOMECONTROLLER — SERVING THE SIGNALR CHAT PAGE
 *
 * In this demo the HomeController has one job: return the chat view.
 * The view (Views/Home/Index.cshtml) embeds all SignalR JavaScript client code:
 *   - HubConnectionBuilder setup
 *   - .start() / .stop() calls
 *   - .on("EventName", handler) registrations
 *   - .invoke() / .send() hub method calls
 *
 * The controller does not pass a ViewModel because the hub manages all
 * real-time data flow.  The initial page render needs no server-side data.
 *
 * If the chat page required a pre-loaded chat history, a ViewModel would
 * carry that list from a service, and the hub would then push new messages.
 * Mixing server-rendered initial data with real-time SignalR updates is a
 * common pattern for chat, live feeds, and collaborative documents.
 */
public sealed class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();  // renders Views/Home/Index.cshtml
    }
}
