using LibrarySystem.API.Extensions;


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configure all services
builder.Services.AddApplicationServices(builder.Configuration);

WebApplication app = builder.Build();

// Configure middleware pipeline
app.ConfigureMiddleware();

// Run database initialization
await app.InitializeDatabaseAsync().ConfigureAwait(false);

await app.RunAsync().ConfigureAwait(false);