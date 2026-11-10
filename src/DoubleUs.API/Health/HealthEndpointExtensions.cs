using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DoubleUs.API.Health;

public static class HealthEndpointExtensions
{
    private const string ReadyTag = "ready";

    public static IServiceCollection AddDependencyHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DependenciesOptions>(configuration.GetSection(DependenciesOptions.SectionName));

        // Resolve options at check-creation time so configuration overrides (tests, environments) are honored.
        services.AddHealthChecks()
            .Add(new HealthCheckRegistration(
                "database",
                sp => CreateCheck(sp, o => o.Database),
                failureStatus: HealthStatus.Unhealthy,
                tags: [ReadyTag]))
            .Add(new HealthCheckRegistration(
                "redis",
                sp => CreateCheck(sp, o => o.Redis),
                failureStatus: HealthStatus.Unhealthy,
                tags: [ReadyTag]));

        return services;
    }

    /// <summary>
    /// /health/live  : process is up; checks nothing external (used for restarts).
    /// /health/ready : required dependencies reachable; returns 503 when not (used for traffic routing).
    /// Both are anonymous and return only the status word, never dependency details.
    /// </summary>
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains(ReadyTag),
        });

        return endpoints;
    }

    private static TcpEndpointHealthCheck CreateCheck(IServiceProvider services, Func<DependenciesOptions, TcpEndpointOptions> select)
    {
        var options = services.GetRequiredService<Microsoft.Extensions.Options.IOptions<DependenciesOptions>>().Value;
        return new TcpEndpointHealthCheck(select(options), TimeSpan.FromSeconds(options.ConnectTimeoutSeconds));
    }
}
