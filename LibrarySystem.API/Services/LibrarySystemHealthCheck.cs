using Microsoft.Extensions.Diagnostics.HealthChecks;
using LibrarySystem.Domain.Interfaces;
using LibrarySystem.Domain.Entities;

namespace LibrarySystem.API.Services;

public class LibrarySystemHealthCheck(IUnitOfWork unitOfWork) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Test basic database operations
            IEnumerable<Book> books = await unitOfWork.Books.GetAllAsync().ConfigureAwait(false);
            IEnumerable<Library> libraries = await unitOfWork.Libraries.GetAllAsync().ConfigureAwait(false);

            var data = new Dictionary<string, object>
            {
                ["total_books"] = books.Count(),
                ["total_libraries"] = libraries.Count(),
                ["timestamp"] = DateTime.UtcNow
            };

            return HealthCheckResult.Healthy(
                $"Library system operational: {books.Count()} books, {libraries.Count()} libraries",
                data);
        }
        catch (InvalidOperationException ex)
        {
            return HealthCheckResult.Unhealthy(
                "Library system health check failed",
                ex);
        }
    }
}