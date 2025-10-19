using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LibrarySystem.Infrastructure.Data;

public partial class DatabaseInitializer(IServiceProvider serviceProvider, ILogger<DatabaseInitializer> logger)
{
    // LoggerMessage delegates for high-performance logging
    private static readonly Action<ILogger, Exception?> _databaseMigrated =
        LoggerMessage.Define(LogLevel.Information, new EventId(1, "DatabaseMigrated"),
            "Database migrated successfully");

    private static readonly Action<ILogger, Exception?> _initializationCompleted =
        LoggerMessage.Define(LogLevel.Information, new EventId(2, "InitializationCompleted"),
            "Database initialization completed successfully");

    private static readonly Action<ILogger, string, Exception?> _initializationError =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(3, "InitializationError"),
            "An error occurred while initializing the database: {ErrorMessage}");

    private static readonly Action<ILogger, Exception?> _noMigrationsFound =
        LoggerMessage.Define(LogLevel.Warning, new EventId(4, "NoMigrationsFound"),
            "No migrations found. Please create migrations using 'dotnet ef migrations add InitialCreate'");

    public async Task InitializeAsync()
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        IServiceProvider scopeServiceProvider = scope.ServiceProvider;

        try
        {
            LibraryDbContext context = scopeServiceProvider.GetRequiredService<LibraryDbContext>();

            // Check if there are any pending migrations
            IEnumerable<string> pendingMigrations = await context.Database.GetPendingMigrationsAsync().ConfigureAwait(false);
            IEnumerable<string> appliedMigrations = await context.Database.GetAppliedMigrationsAsync().ConfigureAwait(false);

            // If no migrations exist at all, log a warning and return
            if (!pendingMigrations.Any() && !appliedMigrations.Any())
            {
                _noMigrationsFound(logger, null);
                throw new InvalidOperationException(
                    "No migrations found. Please create migrations using: dotnet ef migrations add InitialCreate --project LibrarySystem.Infrastructure --startup-project LibrarySystem.API");
            }

            // Apply migrations
            await context.Database.MigrateAsync().ConfigureAwait(false);
            _databaseMigrated(logger, null);

            // Seed organization units first
            await OrganizationUnitSeeder.SeedAsync(context).ConfigureAwait(false);

            // Migrate existing libraries with OU
            await UpdateLibrariesWithOu.MigrateExistingLibrariesAsync(context).ConfigureAwait(false);

            // Seed roles and super admin
            RoleSeeder roleSeeder = scopeServiceProvider.GetRequiredService<RoleSeeder>();
            await roleSeeder.SeedRolesAsync().ConfigureAwait(false);
            await roleSeeder.SeedSuperAdminAsync().ConfigureAwait(false);

            // Seed initial data
            DataSeeder dataSeeder = scopeServiceProvider.GetRequiredService<DataSeeder>();
            await dataSeeder.SeedAsync().ConfigureAwait(false);

            _initializationCompleted(logger, null);
        }
        catch (DbUpdateException dbEx)
        {
            _initializationError(logger, $"Database update failed: {dbEx.Message}", dbEx);
            throw new InvalidOperationException("Database initialization failed due to migration error", dbEx);
        }
        catch (InvalidOperationException invalidOpEx)
        {
            _initializationError(logger, $"Service resolution failed: {invalidOpEx.Message}", invalidOpEx);
            throw new InvalidOperationException("Database initialization failed due to service resolution error", invalidOpEx);
        }
        catch (Exception ex)
        {
            _initializationError(logger, $"Unexpected error: {ex.Message}", ex);
            throw new InvalidOperationException("Database initialization failed", ex);
        }
    }
}