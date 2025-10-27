using System.Threading.RateLimiting;

namespace LibrarySystem.API.Extensions;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddRateLimitingConfiguration(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // Global rate limiter
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.User.Identity?.IsAuthenticated == true
                        ? $"user-{httpContext.User.Identity.Name}"
                        : $"anon-{httpContext.Connection.RemoteIpAddress}",
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1)
                    });
            });

            // Specific policy for authentication endpoints
            options.AddPolicy("AuthPolicy", httpContext =>
            {
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1)
                    });
            });

            // Policy for API endpoints
            options.AddPolicy("ApiPolicy", httpContext =>
            {
                return RateLimitPartition.GetTokenBucketLimiter(
                    partitionKey: httpContext.User.Identity?.IsAuthenticated == true
                        ? $"api-user-{httpContext.User.Identity.Name}"
                        : $"api-anon-{httpContext.Connection.RemoteIpAddress}",
                    factory: partition => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 100,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 10,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                        TokensPerPeriod = 20,
                        AutoReplenishment = true
                    });
            });

            // Configure status code for rate limited requests
            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.HttpContext.Response
                    .WriteAsync("Too many requests. Please try again later.", token)
                    .ConfigureAwait(false);
            };
        });

        return services;
    }
}