var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.Run();

// Exposed for WebApplicationFactory in integration tests.
public partial class Program;
