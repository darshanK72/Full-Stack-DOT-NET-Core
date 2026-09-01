/*
 * FILE ROLE: Configures the Kestrel web server -- endpoint binding (IP / port / socket),
 *            HTTPS / TLS setup, and connection/request limits via KestrelServerOptions.
 * SECTIONS IN THIS FILE:
 *    8. KestrelServerOptions -- endpoint listening (IP, port, socket)
 *    9. HTTPS / TLS endpoint configuration
 *   10. Kestrel limits (MaxRequestBodySize, MaxConcurrentConnections, KeepAliveTimeout)
 */

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System;
using System.Net;

namespace HostingKestrel.Configuration;

/*
 * SECTION 8: KestrelServerOptions -- ENDPOINT BINDING
 *
 * Kestrel is ASP.NET Core's built-in cross-platform HTTP server. It supports:
 *   - HTTP/1.1, HTTP/2, and HTTP/3 (with UseQuic on .NET 8+).
 *   - Binding to specific IP addresses, any IP, or Unix domain sockets.
 *   - Per-endpoint TLS and protocol configuration.
 *
 * Endpoint binding methods on KestrelServerOptions:
 *   +-------------------------------+----------------------------------------------+
 *   | Method                        | Binds to                                     |
 *   +-------------------------------+----------------------------------------------+
 *   | ListenLocalhost(port)         | 127.0.0.1:port (loopback only)               |
 *   | ListenAnyIP(port)             | 0.0.0.0:port (all IPv4 interfaces)           |
 *   | Listen(IPAddress, port)       | Explicit IP address and port                 |
 *   | ListenUnixSocket(path)        | Unix domain socket (Linux / macOS)           |
 *   +-------------------------------+----------------------------------------------+
 *
 * Each method has an overload accepting Action<ListenOptions> for per-endpoint
 * configuration (e.g., HTTPS, HTTP/2 protocol selection).
 *
 * --- 8a. Configuring Kestrel from appsettings.json ---
 * Kestrel reads the "Kestrel" section automatically during host build:
 *   {
 *     "Kestrel": {
 *       "Endpoints": {
 *         "Http":  { "Url": "http://localhost:5050" },
 *         "Https": { "Url": "https://localhost:5051" }
 *       }
 *     }
 *   }
 * JSON-driven endpoint config is preferred in production -- no recompile for URL changes.
 *
 * Pitfalls:
 *   - Calling ConfigureKestrel multiple times ADDS endpoints; it does not replace them.
 *   - ListenAnyIP on Windows covers IPv4 only. Use IPAddress.IPv6Any for IPv6.
 *   - Port 0 lets the OS assign a free port -- useful in integration tests.
 */

/*
 * SECTION 9: HTTPS / TLS ENDPOINT CONFIGURATION
 *
 * HTTPS is configured per-endpoint via the ListenOptions callback.
 *
 * UseHttps() overloads:
 *   listenOptions.UseHttps()                           uses ASP.NET Core dev certificate
 *   listenOptions.UseHttps("cert.pfx", "password")    explicit PFX file
 *   listenOptions.UseHttps(httpsOptions => { ... })    advanced: SNI, client certs
 *
 * Development certificate:
 *   Run once:  dotnet dev-certs https --trust
 *   Kestrel resolves it automatically when UseHttps() has no arguments.
 *
 * Production certificate (recommended -- never store certs in source code):
 *   Set environment variables:
 *     ASPNETCORE_Kestrel__Certificates__Default__Path=cert.pfx
 *     ASPNETCORE_Kestrel__Certificates__Default__Password=secret
 *   Or configure in appsettings.json under Kestrel:Certificates:Default.
 *
 * HTTP/2 requires HTTPS on most platforms:
 *   listenOptions.Protocols = HttpProtocols.Http1AndHttp2;  // explicit protocol enum
 *
 * Pitfall: UseHttps() applies ONLY to the endpoint it is called on. Other endpoints
 *          (e.g., ListenLocalhost) remain plain HTTP unless also configured.
 * Pitfall: Calling UseHttps() without a trusted certificate throws at runtime
 *          (InvalidOperationException), not at build time.
 */

/*
 * SECTION 10: KESTREL LIMITS
 *
 * KestrelServerOptions.Limits exposes request and connection constraints that
 * protect the server from resource exhaustion and slow-loris attacks.
 *
 * +--------------------------------------+----------+--------------------------+
 * | Property                             | Type     | Default                  |
 * +--------------------------------------+----------+--------------------------+
 * | MaxRequestBodySize                   | long?    | 30,000,000 bytes (30 MB) |
 * | MaxConcurrentConnections             | long?    | null (unlimited)         |
 * | MaxConcurrentUpgradedConnections     | long?    | null (unlimited)         |
 * | MaxRequestHeaderCount                | int      | 100                      |
 * | MaxRequestLineSize                   | int      | 8,192 bytes (8 KB)       |
 * | KeepAliveTimeout                     | TimeSpan | 130 seconds              |
 * | RequestHeadersTimeout                | TimeSpan | 30 seconds               |
 * +--------------------------------------+----------+--------------------------+
 *
 * null means unrestricted -- safe inside a trusted internal network only.
 *
 * Per-endpoint / per-action overrides:
 *   [RequestSizeLimit(50_000_000)]     attribute on a controller action (50 MB)
 *   [DisableRequestSizeLimit]          remove the limit entirely for one endpoint
 *
 * Pitfall: MaxRequestBodySize is checked BEFORE the action reads the body. If an
 *          upload endpoint legitimately exceeds the limit, apply [RequestSizeLimit]
 *          to that specific action rather than raising the global limit.
 */
public static class KestrelConfiguration
{
    /*
     * Configure is called from Program.Main, passing builder.WebHost (IWebHostBuilder).
     * ConfigureKestrel adds to any existing Kestrel config without replacing it.
     */
    public static void Configure(IWebHostBuilder webHostBuilder)
    {
        webHostBuilder.ConfigureKestrel(options =>
        {
            // --- Section 8: Endpoint Binding ---
            options.ListenLocalhost(5050);                  // HTTP 127.0.0.1:5050
            options.ListenAnyIP(5051, listenOptions =>      // all interfaces HTTP
            {
                // --- Section 9: HTTPS on this endpoint ---
                listenOptions.UseHttps(); // dev cert (dotnet dev-certs https --trust)
            });
            options.Listen(IPAddress.Loopback, 5052);       // HTTP explicit loopback IP

            // --- Section 10: Limits ---
            options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10 MB (default 30 MB)
            options.Limits.MaxConcurrentConnections = 100;         // null default = unlimited
            options.Limits.MaxConcurrentUpgradedConnections = 100; // WebSocket / HTTP/2 upgrades
            options.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(120); // 120 s (default 130 s)
            options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(20); // tighten header read
        });
    }
}
