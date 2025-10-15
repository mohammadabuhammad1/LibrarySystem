﻿using Microsoft.EntityFrameworkCore;
using LibrarySystem.Domain.Entities;
using LibrarySystem.Domain.Interfaces;
using LibrarySystem.Infrastructure.Data;

namespace LibrarySystem.Infrastructure.Repositories;

public class BorrowRecordRepository(LibraryDbContext context) : GenericRepository<BorrowRecord>(context), IBorrowRecordRepository
{
    public async Task<IEnumerable<BorrowRecord>> GetActiveBorrowsByUserAsync(string userId)
    {
        return await Context.Set<BorrowRecord>()
            .Include(br => br.Book)
            .Include(br => br.User)
            .Where(br => br.UserId == userId && !br.IsReturned)
            .ToListAsync().ConfigureAwait(false);
    }

    public async Task<IEnumerable<BorrowRecord>> GetOverdueBorrowsAsync()
    {
        return await Context.Set<BorrowRecord>()
            .Include(br => br.Book)
            .Include(br => br.User)
            .Where(br => !br.IsReturned && br.DueDate < DateTime.UtcNow)
            .ToListAsync().ConfigureAwait(false);
    }

    public async Task<BorrowRecord?> GetActiveBorrowByBookAndUserAsync(int bookId, string userId)
    {
        return await Context.Set<BorrowRecord>()
            .Include(br => br.Book)
            .Include(br => br.User)
            .FirstOrDefaultAsync(br => br.BookId == bookId && br.UserId == userId && !br.IsReturned).ConfigureAwait(false);
    }

    public async Task<IEnumerable<BorrowRecord>> GetBorrowHistoryByUserAsync(string userId)
    {
        return await Context.Set<BorrowRecord>()
            .Include(br => br.Book)
            .Include(br => br.User)
            .Where(br => br.UserId == userId)
            .OrderByDescending(br => br.BorrowDate)
            .ToListAsync().ConfigureAwait(false);
    }

    public async Task<IEnumerable<BorrowRecord>> GetBorrowHistoryByBookAsync(int bookId)
    {
        return await Context.Set<BorrowRecord>()
            .Include(br => br.Book)
            .Include(br => br.User)
            .Where(br => br.BookId == bookId)
            .OrderByDescending(br => br.BorrowDate)
            .ToListAsync().ConfigureAwait(false);
    }

    public async Task<BorrowRecord?> GetBorrowRecordWithDetailsAsync(int borrowRecordId)
    {
        return await Context.Set<BorrowRecord>()
            .Include(br => br.Book)
            .Include(br => br.User)
            .FirstOrDefaultAsync(br => br.Id == borrowRecordId).ConfigureAwait(false);
    }
}