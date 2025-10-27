using LibrarySystem.API.HealthChecks;
using LibrarySystem.API.Models;
using LibrarySystem.Infrastructure.Data;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace LibrarySystem.API.Extensions;

public static class HealthCheckExtensions
{
    private static readonly string[] ReadyTags = ["ready"];
    private static readonly string[] DbTags = ["ready", "db"];
    private static readonly string[] ResourcesTags = ["ready", "resources"];
    private static readonly string[] BusinessTags = ["ready", "business"];
    private static readonly string[] LiveTags = ["live"];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static IServiceCollection AddComprehensiveHealthChecks(
        this IServiceCollection services)
    {
        services.AddHealthChecks()
            // Database connectivity
            .AddCheck<DbContextHealthCheck<LibraryDbContext>>(
                "database",
                tags: DbTags)

            // System resources
            .AddCheck<SystemResourcesHealthCheck>(
                "system_resources",
                tags: ResourcesTags)

            // Application business logic
            .AddCheck<ApplicationHealthCheck>(
                "application",
                tags: BusinessTags)

            // Readiness check
            .AddCheck<ReadinessHealthCheck>(
                "readiness",
                tags: ReadyTags)

            // Liveness check
            .AddCheck<LivenessHealthCheck>(
                "liveness",
                tags: LiveTags);

        return services;
    }

    public static IEndpointRouteBuilder MapComprehensiveHealthChecks(
        this IEndpointRouteBuilder endpoints)
    {
        // Main health endpoint - all checks
        endpoints.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = WriteHealthCheckResponse,
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        });

        // Readiness probe - for load balancers
        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = WriteHealthCheckResponse,
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        });

        // Liveness probe - for orchestrators
        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live"),
            ResponseWriter = WriteHealthCheckResponse,
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        });

        // Simple endpoint for quick checks
        endpoints.MapHealthChecks("/health/ping", new HealthCheckOptions
        {
            Predicate = _ => false, // No checks, just returns 200 OK
            ResponseWriter = (context, _) =>
            {
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(
                    JsonSerializer.Serialize(new { status = "pong", timestamp = DateTime.UtcNow }, JsonOptions));
            }
        });

        return endpoints;
    }

    private static Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new HealthCheckResponse
        {
            Status = report.Status.ToString(),
            Duration = report.TotalDuration,
            Timestamp = DateTime.UtcNow,
            Checks = report.Entries.ToDictionary(
                entry => entry.Key,
                entry => new HealthCheckEntry
                {
                    Status = entry.Value.Status.ToString(),
                    Description = entry.Value.Description,
                    Duration = entry.Value.Duration,
                    Data = entry.Value.Data?.Count > 0
                        ? entry.Value.Data
                        : null,
                    Exception = entry.Value.Exception?.Message
                })
        };

        return context.Response.WriteAsync(
            JsonSerializer.Serialize(response, JsonOptions));
    }
}