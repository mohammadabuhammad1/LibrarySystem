using LibrarySystem.Infrastructure.Data;

namespace LibrarySystem.API.Extensions;

public static class DatabaseExtensions
{
    public static async Task<WebApplication> InitializeDatabaseAsync(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        using IServiceScope scope = app.Services.CreateScope();
        IServiceProvider services = scope.ServiceProvider;

        try
        {
            DatabaseInitializer dbInitializer = services.GetRequiredService<DatabaseInitializer>();
            await dbInitializer.InitializeAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Database initialization failed", ex);
        }

        return app;
    }
}
