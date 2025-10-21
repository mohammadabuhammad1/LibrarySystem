using LibrarySystem.Domain.Entities;
using LibrarySystem.Domain.Interfaces;
using LibrarySystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace LibrarySystem.Infrastructure.Repositories;

public class BorrowRecordRepository(LibraryDbContext context) : GenericRepository<BorrowRecord>(context), IBorrowRecordRepository
{
    public async Task<IEnumerable<BorrowRecord>> GetActiveBorrowsByUserAsync(string userId)
    {
        return await context.Set<BorrowRecord>()
            .AsNoTracking() 
            .Include(br => br.Book)
            .Include(br => br.User)
            .Where(br => br.UserId == userId && !br.IsReturned)
            .ToListAsync().ConfigureAwait(false);
    }

    public async Task<IEnumerable<BorrowRecord>> GetOverdueBorrowsAsync()
    {
        return await context.Set<BorrowRecord>()
            .AsNoTracking() 
            .Include(br => br.Book)
            .Include(br => br.User)
            .Where(br => !br.IsReturned && br.DueDate < DateTime.UtcNow)
            .ToListAsync().ConfigureAwait(false);
    }

    public async Task<IEnumerable<BorrowRecord>> GetOverdueBorrowsByUserAsync(string userId)
    {
        return await context.Set<BorrowRecord>()
            .AsNoTracking() 
            .Include(br => br.Book)
            .Include(br => br.User)
            .Where(br => br.UserId == userId && !br.IsReturned && br.DueDate < DateTime.UtcNow)
            .ToListAsync().ConfigureAwait(false);
    }

    public async Task<BorrowRecord?> GetActiveBorrowByBookAndUserAsync(int bookId, string userId)
    {
        // This record might be retrieved to check borrowing status, 
        // but it is also potentially retrieved *right before* a command that
        // modifies it (like returning a book).
        // Since the pattern is CQRS (Commands modify, Queries read),
        // we'll apply AsNoTracking here too, assuming the Service layer
        // will fetch a *tracked* entity separately if modification is needed.
        return await context.Set<BorrowRecord>()
            .AsNoTracking() 
            .Include(br => br.Book)
            .Include(br => br.User)
            .FirstOrDefaultAsync(br => br.BookId == bookId && br.UserId == userId && !br.IsReturned).ConfigureAwait(false);
    }

    public async Task<BorrowRecord?> GetActiveBorrowByBookAsync(int bookId)
    {
        return await context.Set<BorrowRecord>()
            .AsNoTracking() 
            .Include(br => br.Book)
            .Include(br => br.User)
            .FirstOrDefaultAsync(br => br.BookId == bookId && !br.IsReturned).ConfigureAwait(false);
    }

    public async Task<IEnumerable<BorrowRecord>> GetBorrowHistoryByUserAsync(string userId)
    {
        return await context.Set<BorrowRecord>()
            .AsNoTracking()
            .Include(br => br.Book)
            .Include(br => br.User)
            .Where(br => br.UserId == userId)
            .OrderByDescending(br => br.BorrowDate)
            .ToListAsync().ConfigureAwait(false);
    }

    public async Task<IEnumerable<BorrowRecord>> GetBorrowHistoryByBookAsync(int bookId)
    {
        return await context.Set<BorrowRecord>()
            .AsNoTracking() 
            .Include(br => br.Book)
            .Include(br => br.User)
            .Where(br => br.BookId == bookId)
            .OrderByDescending(br => br.BorrowDate)
            .ToListAsync().ConfigureAwait(false);
    }

    public async Task<BorrowRecord?> GetBorrowRecordWithDetailsAsync(int borrowRecordId)
    {
        return await context.Set<BorrowRecord>()
            .AsNoTracking() 
            .Include(br => br.Book)
            .Include(br => br.User)
            .FirstOrDefaultAsync(br => br.Id == borrowRecordId).ConfigureAwait(false);
    }

    public async Task<bool> HasActiveBorrowForBookAsync(string userId, int bookId)
    {
        return await context.Set<BorrowRecord>()
            .AsNoTracking()
            .AnyAsync(br => br.UserId == userId &&
                           br.BookId == bookId &&
                           !br.IsReturned) 
            .ConfigureAwait(false);
    }
}