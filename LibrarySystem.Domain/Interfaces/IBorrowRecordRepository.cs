using LibrarySystem.Domain.Entities;

namespace LibrarySystem.Domain.Interfaces;

public interface IBorrowRecordRepository : IGenericRepository<BorrowRecord>
{
    Task<BorrowRecord?> GetActiveBorrowByBookAndUserAsync(int bookId, string userId);
    Task<BorrowRecord?> GetActiveBorrowByBookAsync(int bookId);
    Task<IEnumerable<BorrowRecord>> GetBorrowHistoryByUserAsync(string userId);
    Task<IEnumerable<BorrowRecord>> GetBorrowHistoryByBookAsync(int bookId);
    Task<IEnumerable<BorrowRecord>> GetOverdueBorrowsAsync();
    Task<IEnumerable<BorrowRecord>> GetOverdueBorrowsByUserAsync(string userId);
    Task<IEnumerable<BorrowRecord>> GetActiveBorrowsByUserAsync(string userId);
    Task<BorrowRecord?> GetBorrowRecordWithDetailsAsync(int borrowRecordId);

}