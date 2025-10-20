using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LibrarySystem.Infrastructure.Data;

public class DbContextHealthCheck<TContext>(TContext dbContext) : IHealthCheck
    where TContext : DbContext
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            bool canConnect = await dbContext.Database.CanConnectAsync(cancellationToken).ConfigureAwait(false);

            return canConnect
                ? HealthCheckResult.Healthy("Database connection is healthy")
                : HealthCheckResult.Unhealthy("Cannot connect to database");
        }
        catch (DbUpdateException ex)
        {
            return HealthCheckResult.Unhealthy("Database update error", ex);
        }
        catch (OperationCanceledException ex)
        {
            return HealthCheckResult.Unhealthy("Database connection timeout", ex);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("connection", StringComparison.OrdinalIgnoreCase))
        {
            return HealthCheckResult.Unhealthy("Database connection configuration error", ex);
        }
        catch (System.Data.Common.DbException ex)
        {
            return HealthCheckResult.Unhealthy("Database provider error", ex);
        }
        catch (InvalidOperationException ex)
        {
            return HealthCheckResult.Unhealthy("Database connection failed", ex);
        }
    }
}