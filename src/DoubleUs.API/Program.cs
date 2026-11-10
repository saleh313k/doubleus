using DoubleUs.API.Health;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDependencyHealthChecks(builder.Configuration);

var app = builder.Build();

app.MapHealthEndpoints();

app.Run();

// Exposed for WebApplicationFactory in integration tests.
public partial class Program;
