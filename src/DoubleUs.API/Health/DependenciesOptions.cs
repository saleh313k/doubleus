namespace DoubleUs.API.Health;

/// <summary>Network location of a backing service, bound from configuration.</summary>
public sealed class TcpEndpointOptions
{
    public string Host { get; set; } = "localhost";

    public int Port { get; set; }

    /// <summary>
    /// When true an unreachable endpoint makes /health/ready fail (503).
    /// When false it only reports Degraded (still HTTP 200), for dependencies not yet needed by the running features.
    /// </summary>
    public bool Required { get; set; } = true;
}

/// <summary>Backing services the API needs in order to serve traffic. Bound from the "Dependencies" section.</summary>
public sealed class DependenciesOptions
{
    public const string SectionName = "Dependencies";

    /// <summary>SQL Server (the application database).</summary>
    public TcpEndpointOptions Database { get; set; } = new() { Port = 1433 };

    /// <summary>Redis is not used by Phase 1 features, so it is optional until application code depends on it.</summary>
    public TcpEndpointOptions Redis { get; set; } = new() { Port = 6379, Required = false };

    public int ConnectTimeoutSeconds { get; set; } = 2;
}
