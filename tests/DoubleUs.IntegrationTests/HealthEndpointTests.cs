using System.Globalization;
using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DoubleUs.IntegrationTests;

/// <summary>
/// Exercises /health/live and /health/ready end to end without any database installed: "available" dependencies are
/// local TCP listeners, "unavailable" ones are ports nothing listens on. (Real SQL Server-backed tests start in task 3.)
/// </summary>
public class HealthEndpointTests
{
    [Fact]
    public async Task Live_returns_200_even_when_dependencies_are_down()
    {
        using var factory = CreateFactory(databasePort: ClosedPort(), redisPort: ClosedPort());
        using var client = factory.CreateClient();

        var response = await client.GetAsync(new Uri("/health/live", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Ready_returns_503_when_the_database_is_down()
    {
        using var redis = Listen();
        using var factory = CreateFactory(databasePort: ClosedPort(), redisPort: PortOf(redis));
        using var client = factory.CreateClient();

        var response = await client.GetAsync(new Uri("/health/ready", UriKind.Relative));

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Ready_returns_200_when_all_dependencies_accept_connections()
    {
        using var database = Listen();
        using var redis = Listen();
        using var factory = CreateFactory(databasePort: PortOf(database), redisPort: PortOf(redis));
        using var client = factory.CreateClient();

        var response = await client.GetAsync(new Uri("/health/ready", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Ready_returns_200_when_only_optional_redis_is_down()
    {
        using var database = Listen();
        using var factory = CreateFactory(databasePort: PortOf(database), redisPort: ClosedPort(), redisRequired: false);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(new Uri("/health/ready", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Degraded", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Ready_returns_503_when_required_redis_is_down()
    {
        using var database = Listen();
        using var factory = CreateFactory(databasePort: PortOf(database), redisPort: ClosedPort(), redisRequired: true);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(new Uri("/health/ready", UriKind.Relative));

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Ready_response_does_not_leak_dependency_details()
    {
        using var factory = CreateFactory(databasePort: ClosedPort(), redisPort: ClosedPort());
        using var client = factory.CreateClient();

        var response = await client.GetAsync(new Uri("/health/ready", UriKind.Relative));
        var text = await response.Content.ReadAsStringAsync();

        Assert.Equal("Unhealthy", text);
        Assert.DoesNotContain("database", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("sql", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("redis", text, StringComparison.OrdinalIgnoreCase);
    }

    private static WebApplicationFactory<Program> CreateFactory(int databasePort, int redisPort, bool redisRequired = false) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Dependencies:Database:Host", "127.0.0.1");
            builder.UseSetting("Dependencies:Database:Port", databasePort.ToString(CultureInfo.InvariantCulture));
            builder.UseSetting("Dependencies:Redis:Host", "127.0.0.1");
            builder.UseSetting("Dependencies:Redis:Port", redisPort.ToString(CultureInfo.InvariantCulture));
            builder.UseSetting("Dependencies:Redis:Required", redisRequired ? "true" : "false");
            builder.UseSetting("Dependencies:ConnectTimeoutSeconds", "1");
        });

    private static TcpListener Listen()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return listener;
    }

    private static int PortOf(TcpListener listener) => ((IPEndPoint)listener.LocalEndpoint).Port;

    /// <summary>Reserves a free port and releases it, so nothing is listening there.</summary>
    private static int ClosedPort()
    {
        using var listener = Listen();
        return PortOf(listener);
    }
}
