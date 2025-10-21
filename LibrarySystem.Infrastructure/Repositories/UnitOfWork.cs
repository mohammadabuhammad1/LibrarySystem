using LibrarySystem.Domain.Interfaces;
using LibrarySystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq;

namespace LibrarySystem.Infrastructure.Repositories;

public sealed class UnitOfWork(LibraryDbContext context) : IUnitOfWork
{
    public IBookRepository Books { get; } = new BookRepository(context);
    public IBorrowRecordRepository BorrowRecords { get; } = new BorrowRecordRepository(context);
    public ILibraryRepository Libraries { get; } = new LibraryRepository(context);


    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> CommitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            Console.WriteLine($"Database concurrency error: {ex.Message}");
            return false;
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Database commit failed: {ex.Message}");
            return false;
        }
    }

    public void Rollback()
    {
        foreach (EntityEntry? entry in context.ChangeTracker.Entries()
            .Where(e => e.State != EntityState.Unchanged && e.State != EntityState.Detached))
        {
            entry.State = EntityState.Detached;
        }
    }

    public void Dispose()
    {
        context.Dispose();
        GC.SuppressFinalize(this);
    }
}