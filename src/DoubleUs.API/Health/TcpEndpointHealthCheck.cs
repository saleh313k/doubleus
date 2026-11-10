using System.Net.Sockets;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DoubleUs.API.Health;

/// <summary>
/// Connectivity-only readiness probe: succeeds when a TCP connection to the endpoint can be opened.
/// It does NOT authenticate or run a query. Phase 1 task 3 replaces the database probe with a real
/// EF Core query; the Redis probe is upgraded when Redis is first used by application code.
/// A failing optional endpoint reports Degraded (HTTP 200); a failing required endpoint reports Unhealthy (HTTP 503).
/// Failure details are deliberately not returned to callers.
/// </summary>
internal sealed class TcpEndpointHealthCheck(TcpEndpointOptions endpoint, TimeSpan timeout) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(timeout);

        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(endpoint.Host, endpoint.Port, timeoutSource.Token);
            return HealthCheckResult.Healthy();
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Failure("Connection attempt timed out.");
        }
        catch (SocketException)
        {
            return Failure("Connection refused or host unreachable.");
        }
    }

    private HealthCheckResult Failure(string description) =>
        new(endpoint.Required ? HealthStatus.Unhealthy : HealthStatus.Degraded, description);
}
