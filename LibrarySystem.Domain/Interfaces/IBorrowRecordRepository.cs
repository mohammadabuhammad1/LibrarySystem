﻿using LibrarySystem.Domain.Entities;

namespace LibrarySystem.Domain.Interfaces;

public interface IBorrowRecordRepository : IGenericRepository<BorrowRecord>
{
    Task<IEnumerable<BorrowRecord>> GetActiveBorrowsByUserAsync(string userId);
    Task<IEnumerable<BorrowRecord>> GetOverdueBorrowsAsync();
    Task<BorrowRecord?> GetActiveBorrowByBookAndUserAsync(int bookId, string userId);
    Task<IEnumerable<BorrowRecord>> GetBorrowHistoryByUserAsync(string userId);
    Task<IEnumerable<BorrowRecord>> GetBorrowHistoryByBookAsync(int bookId);

    Task<BorrowRecord?> GetBorrowRecordWithDetailsAsync(int borrowRecordId);

}