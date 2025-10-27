using LibrarySystem.Domain.Entities;
using LibrarySystem.Domain.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LibrarySystem.API.HealthChecks;

public class ApplicationHealthCheck(IUnitOfWork unitOfWork) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Test database operations
            IEnumerable<Book> books = await unitOfWork.Books
                .GetAllAsync()
                .ConfigureAwait(false);

            IEnumerable<Library> libraries = await unitOfWork.Libraries
                .GetAllAsync()
                .ConfigureAwait(false);

            IEnumerable<BorrowRecord> borrowRecords = await unitOfWork.BorrowRecords
                .GetAllAsync()
                .ConfigureAwait(false);

            // Get active borrow records
            int activeBorrows = borrowRecords.Count(br => !br.IsReturned);
            int overdueBorrows = borrowRecords.Count(br =>
                !br.IsReturned && br.DueDate < DateTime.UtcNow);

            // Calculate system metrics
            int totalBooks = books.Count();
            int availableBooks = books.Sum(b => b.CopiesAvailable);
            int totalCopies = books.Sum(b => b.TotalCopies);
            double availabilityRate = totalCopies > 0
                ? (double)availableBooks / totalCopies * 100
                : 0;

            var data = new Dictionary<string, object>
            {
                ["total_libraries"] = libraries.Count(),
                ["total_books"] = totalBooks,
                ["total_copies"] = totalCopies,
                ["available_copies"] = availableBooks,
                ["availability_rate_percent"] = Math.Round(availabilityRate, 2),
                ["active_borrows"] = activeBorrows,
                ["overdue_borrows"] = overdueBorrows,
                ["total_borrow_records"] = borrowRecords.Count(),
                ["timestamp"] = DateTime.UtcNow
            };

            // Determine health based on business rules
            if (totalBooks == 0 || !libraries.Any())
            {
                return HealthCheckResult.Degraded(
                    "Library system has no books or libraries configured",
                    data: data);
            }

            if (availabilityRate < 10)
            {
                return HealthCheckResult.Degraded(
                    $"Low book availability: {availabilityRate:F2}%",
                    data: data);
            }

            return HealthCheckResult.Healthy(
                $"Library system operational: {totalBooks} books across {libraries.Count()} libraries",
                data);
        }
        catch (InvalidOperationException ex)
        {
            return HealthCheckResult.Unhealthy(
                "Library system health check failed: Invalid operation",
                ex);
        }
        catch (TimeoutException ex)
        {
            return HealthCheckResult.Unhealthy(
                "Library system health check failed: Operation timeout",
                ex);
        }
    }
}
