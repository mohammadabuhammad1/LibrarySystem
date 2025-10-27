using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LibrarySystem.API.HealthChecks;
public class LivenessHealthCheck : IHealthCheck
{
    private static readonly DateTime _startTime = DateTime.UtcNow;

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            TimeSpan uptime = DateTime.UtcNow - _startTime;

            var data = new Dictionary<string, object>
            {
                ["uptime_seconds"] = (int)uptime.TotalSeconds,
                ["uptime_formatted"] = FormatUptime(uptime),
                ["timestamp"] = DateTime.UtcNow,
                ["process_id"] = Environment.ProcessId
            };

            return Task.FromResult(HealthCheckResult.Healthy(
                "Application is alive and responsive",
                data));
        }
        catch (InvalidOperationException ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "Application liveness check failed: Invalid operation",
                ex));
        }
    }

    private static string FormatUptime(TimeSpan uptime)
    {
        if (uptime.TotalDays >= 1)
        {
            return $"{(int)uptime.TotalDays}d {uptime.Hours}h {uptime.Minutes}m";
        }
        if (uptime.TotalHours >= 1)
        {
            return $"{uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s";
        }
        return $"{uptime.Minutes}m {uptime.Seconds}s";
    }
}