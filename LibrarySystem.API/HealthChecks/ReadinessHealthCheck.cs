using LibrarySystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LibrarySystem.API.HealthChecks;

public class ReadinessHealthCheck(LibraryDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check database connection
            bool canConnect = await dbContext.Database
                .CanConnectAsync(cancellationToken)
                .ConfigureAwait(false);

            if (!canConnect)
            {
                return HealthCheckResult.Unhealthy("Cannot connect to database");
            }

            // Check if migrations are applied
            IEnumerable<string> pendingMigrations = await dbContext.Database
                .GetPendingMigrationsAsync(cancellationToken)
                .ConfigureAwait(false);

            if (pendingMigrations.Any())
            {
                return HealthCheckResult.Unhealthy(
                    "Pending database migrations detected",
                    data: new Dictionary<string, object>
                    {
                        ["pending_migrations"] = pendingMigrations.ToList()
                    });
            }

            // Verify essential data exists
            bool hasLibraries = await dbContext.Libraries
                .AnyAsync(cancellationToken)
                .ConfigureAwait(false);

            var data = new Dictionary<string, object>
            {
                ["database_ready"] = true,
                ["migrations_applied"] = true,
                ["has_libraries"] = hasLibraries,
                ["timestamp"] = DateTime.UtcNow
            };

            return hasLibraries
                ? HealthCheckResult.Healthy("Application is ready to serve requests", data)
                : HealthCheckResult.Degraded("Application ready but no libraries configured", data: data);
        }
        catch (DbUpdateException ex)
        {
            return HealthCheckResult.Unhealthy(
                "Application is not ready: Database update error",
                ex);
        }
        catch (InvalidOperationException ex)
        {
            return HealthCheckResult.Unhealthy(
                "Application is not ready: Invalid operation",
                ex);
        }
        catch (TimeoutException ex)
        {
            return HealthCheckResult.Unhealthy(
                "Application is not ready: Operation timeout",
                ex);
        }
    }
}