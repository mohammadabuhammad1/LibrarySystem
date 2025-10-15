﻿using LibrarySystem.Domain.Entities;

namespace LibrarySystem.Domain.Interfaces;

public interface IBookRepository : IGenericRepository<Book>
{
    Task<Book?> GetByIsbnAsync(string isbn);
    Task<IEnumerable<Book>> GetAvailableBooksAsync();
    Task<IEnumerable<Book>> GetBooksByLibraryAsync(int libraryId);
    Task<IEnumerable<Book>> GetBorrowedBooksByUserAsync(string userId);

}
