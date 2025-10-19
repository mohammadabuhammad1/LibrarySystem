using LibrarySystem.Domain.Interfaces;
using LibrarySystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace LibrarySystem.Infrastructure.Repositories;

public sealed class UnitOfWork(
    LibraryDbContext context,
    IBookRepository bookRepository,
    IBorrowRecordRepository borrowRecordRepository,
    ILibraryRepository libraryRepository,
    IOrganizationUnitRepository organizationUnitRepository) : IUnitOfWork
{
    private IDbContextTransaction? _transaction;

    public IBookRepository Books { get; } = bookRepository;
    public IBorrowRecordRepository BorrowRecords { get; } = borrowRecordRepository;
    public ILibraryRepository Libraries { get; } = libraryRepository;
    public IOrganizationUnitRepository OrganizationUnits { get; } = organizationUnitRepository;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> CommitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _transaction = await context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            var result = await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false) > 0;
            await _transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return result;
        }
        catch
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            }
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync().ConfigureAwait(false);
                _transaction = null;
            }
        }
    }

    public void Rollback()
    {
        _transaction?.Rollback();
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        context?.Dispose();
        GC.SuppressFinalize(this);
    }
}