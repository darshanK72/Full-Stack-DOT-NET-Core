/*
 * FILE ROLE: Demonstrates custom IHealthCheck implementations, HealthCheckResult
 *            response types (Healthy/Degraded/Unhealthy), and registration patterns.
 * SECTIONS IN THIS FILE:
 *   8. IHealthCheck — implementing a custom health check with full context
 *   9. HealthCheckResult — Healthy, Degraded, and Unhealthy response types
 */

using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LoggingDiagnostics.Services;

/*
 * SECTION 8: IHealthCheck — CUSTOM HEALTH CHECK IMPLEMENTATION
 *
 * Health checks expose the operational status of an application and its dependencies
 * via an HTTP endpoint (typically /health). Load balancers, Kubernetes liveness/readiness
 * probes, and monitoring dashboards poll this endpoint automatically.
 *
 * THE INTERFACE:
 *   public interface IHealthCheck
 *   {
 *       Task<HealthCheckResult> CheckHealthAsync(
 *           HealthCheckContext context, CancellationToken cancellationToken);
 *   }
 *
 * REGISTRATION (in Program.cs):
 *   builder.Services.AddHealthChecks()
 *       .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "db", "critical" })
 *       .AddCheck<OrderQueueHealthCheck>("order-queue", tags: new[] { "queue" })
 *       .AddCheck("ping", () => HealthCheckResult.Healthy("pong")); // inline lambda
 *
 * EXPOSING THE ENDPOINT:
 *   app.MapHealthChecks("/health");                          // all checks; 200 or 503
 *   app.MapHealthChecks("/health/ready", new HealthCheckOptions
 *   {
 *       Predicate = reg => reg.Tags.Contains("critical")    // only run "critical" checks
 *   });
 *
 * HealthCheckContext properties:
 *   context.Registration.Name          — the name supplied during AddCheck ("database")
 *   context.Registration.Tags          — the tag set for selective endpoint filtering
 *   context.Registration.FailureStatus — status reported when an exception escapes
 *                                        (defaults to HealthStatus.Unhealthy)
 *
 * EXCEPTION HANDLING:
 *   If CheckHealthAsync throws, the framework catches it and returns Unhealthy.
 *   Explicit try/catch lets you attach the exception to the result for richer output.
 */
public sealed class DatabaseHealthCheck : IHealthCheck
{
    // Real implementations inject IDbConnection, IDbContextFactory, or a custom probe service
    public DatabaseHealthCheck() { } // simplified — no real DB in this tutorial

    /*
     * SECTION 9: HealthCheckResult — HEALTHY / DEGRADED / UNHEALTHY
     *
     * THREE RESULT STATUSES:
     * ┌─────────────────┬───────────┬───────────────────────────────────────────┐
     * │ Status          │ HTTP Code │ Meaning                                   │
     * ├─────────────────┼───────────┼───────────────────────────────────────────┤
     * │ Healthy         │   200     │ Dependency is fully reachable.            │
     * │ Degraded        │   200     │ Partially available; warn but don't fail. │
     * │ Unhealthy       │   503     │ Dependency is down; shed traffic.         │
     * └─────────────────┴───────────┴───────────────────────────────────────────┘
     *
     * NOTE: Both Healthy and Degraded return HTTP 200 by default.
     *   Only Unhealthy returns 503. You can override per-status HTTP codes via
     *   HealthCheckOptions.ResultStatusCodes in the endpoint registration.
     *
     * FACTORY METHODS:
     *   HealthCheckResult.Healthy(description?, data?)
     *   HealthCheckResult.Degraded(description?, exception?, data?)
     *   HealthCheckResult.Unhealthy(description?, exception?, data?)
     *
     * DATA DICTIONARY — IReadOnlyDictionary<string, object>:
     *   Attach structured diagnostics (response times, pool stats, last-checked times).
     *   Surfaced in JSON output when a detail-formatter is configured on the endpoint.
     *
     * CONSTRUCTOR FORM (alternative):
     *   new HealthCheckResult(HealthStatus.Healthy, "message", null, data)
     */
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            bool responding = SimulateDatabasePing();      // true = up; false = slow
            bool respondingFast = SimulateResponseTime();  // true = fast; false = slow

            if (!responding)
            {
                // 503 — load balancer should stop sending traffic here
                return Task.FromResult(
                    HealthCheckResult.Unhealthy(
                        "Database is not reachable",
                        data: new Dictionary<string, object> { ["last_checked"] = DateTimeOffset.UtcNow }));
            }

            if (!respondingFast)
            {
                // 200 — traffic continues but an alert should fire
                IReadOnlyDictionary<string, object> degradedData =
                    new Dictionary<string, object>
                    {
                        ["response_time_ms"] = 4800,       // diagnostic metadata attached to the result
                        ["last_checked"] = DateTimeOffset.UtcNow
                    };
                return Task.FromResult(
                    HealthCheckResult.Degraded("Database is responding slowly", data: degradedData));
            }

            // Nominal path — 200
            IReadOnlyDictionary<string, object> healthyData =
                new Dictionary<string, object>
                {
                    ["response_time_ms"] = 14,
                    ["last_checked"] = DateTimeOffset.UtcNow
                };
            return Task.FromResult(
                HealthCheckResult.Healthy("Database is reachable", data: healthyData));
        }
        catch (Exception ex)
        {
            // Explicit Unhealthy with the exception attached — visible in JSON health report
            return Task.FromResult(
                HealthCheckResult.Unhealthy(
                    "Database probe threw an exception",
                    exception: ex,
                    data: new Dictionary<string, object> { ["error"] = ex.Message }));
        }
    }

    private static bool SimulateDatabasePing() => true;     // stand-in for real connectivity probe
    private static bool SimulateResponseTime() => true;     // stand-in for latency check
}

/*
 * A second health check — demonstrates tags-based selective endpoint filtering.
 *
 * Tags allow separate readiness vs liveness endpoints:
 *   /health/live  — all checks (Kubernetes liveness probe)
 *   /health/ready — only "critical" tagged checks (Kubernetes readiness probe)
 *
 * Registration (see Program.cs Section 10):
 *   .AddCheck<OrderQueueHealthCheck>("order-queue", tags: new[] { "queue" })
 */
public sealed class OrderQueueHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        bool queueReachable = SimulateQueuePing();

        HealthCheckResult result = queueReachable
            ? HealthCheckResult.Healthy("Order queue is reachable")
            : HealthCheckResult.Unhealthy("Order queue is not reachable"); // 503

        return Task.FromResult(result);
    }

    private static bool SimulateQueuePing() => true; // stand-in
}
