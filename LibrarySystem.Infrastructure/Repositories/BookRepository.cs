using LibrarySystem.Domain.Entities;
using LibrarySystem.Infrastructure.Data;
using LibrarySystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Infrastructure.Repositories;

public class BookRepository(LibraryDbContext context) : GenericRepository<Book>(context), IBookRepository
{
    public async Task<Book?> GetByIsbnAsync(string isbn)
    {
        return await context.Set<Book>()
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.ISBN == isbn).ConfigureAwait(false);
    }

    public async Task<IEnumerable<Book>> GetAvailableBooksAsync()
    {
        return await context.Set<Book>()
            .AsNoTracking()
            .Where(b => b.CopiesAvailable > 0)
            .ToListAsync().ConfigureAwait(false);
    }

    public async Task<IEnumerable<Book>> GetBooksByLibraryAsync(int libraryId)
    {
        return await context.Set<Book>()
            .AsNoTracking()
            .Where(b => b.LibraryId == libraryId)
            .ToListAsync().ConfigureAwait(false);
    }

    public async Task<IEnumerable<Book>> GetBorrowedBooksByUserAsync(string userId)
    {
        return await context.Set<Book>()
            .AsNoTracking()
            .Where(b => b.BorrowRecords.Any(br =>
                br.UserId == userId &&
                br.ReturnDate == null)) 
            .ToListAsync().ConfigureAwait(false);
    }
}