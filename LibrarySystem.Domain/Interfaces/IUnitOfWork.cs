
namespace LibrarySystem.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    // Repositories
    IBookRepository Books { get; }
    IBorrowRecordRepository BorrowRecords { get; }
    ILibraryRepository Libraries { get; }
    IOrganizationUnitRepository OrganizationUnits { get; }

    // Transaction methods
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<bool> CommitAsync(CancellationToken cancellationToken = default);
    void Rollback();
}