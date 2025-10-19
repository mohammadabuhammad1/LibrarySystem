﻿using LibrarySystem.Application.Dtos.Books;

namespace LibrarySystem.Application.Interfaces;

public interface IBorrowRecordService
{
    Task<BorrowRecordDto> BorrowBookAsync(CreateBorrowRecordDto borrowDto);
    Task<BorrowRecordDto> ReturnBookAsync(ReturnBookDto returnDto);
    Task<IEnumerable<BorrowRecordDto>> GetUserBorrowHistoryAsync(string userId);
    Task<IEnumerable<BorrowRecordDto>> GetOverdueBooksAsync();
    Task<IEnumerable<BorrowRecordDto>> GetActiveBorrowsByUserAsync(string userId); 
    Task<decimal> CalculateFineAsync(int borrowRecordId);
    Task<bool> CanUserViewFineAsync(int borrowRecordId, string userId);

    Task<IEnumerable<BorrowRecordDto>> GetBorrowHistoryByBookAsync(int bookId);
    Task<BorrowRecordDto> RenewBorrowAsync(int borrowRecordId, int additionalDays, string userId);

    Task<IEnumerable<BookDto>> GetBorrowedBooksByUserAsync(string userId);
    Task<bool> CanUserBorrowAsync(string userId);
    Task<BorrowRecordDto?> GetActiveBorrowByBookAsync(int bookId);

}