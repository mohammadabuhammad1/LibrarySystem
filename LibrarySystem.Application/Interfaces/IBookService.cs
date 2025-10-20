using LibrarySystem.Application.Dtos.Books;

namespace LibrarySystem.Application.Interfaces;

public interface IBookService
{
    Task<BookDto?> GetBookByIdAsync(int id);
    Task<IEnumerable<BookDto>> GetAllBooksAsync();
    Task<BookDto?> GetBookByIsbnAsync(string isbn);
    Task<IEnumerable<BookDto>> GetAvailableBooksAsync();
    Task<IEnumerable<BookDto>> GetBooksByLibraryAsync(int libraryId);
    Task<IEnumerable<BookDto>> GetBorrowedBooksByUserAsync(string userId);
    Task<bool> BookExistsAsync(int id);
    Task<OverallBookStatsDto> GetOverallBookStatsAsync();



    //Task<BookDto?> CreateBookAsync(CreateBookDto createBookDto)
    //Task<BookDto?> UpdateBookAsync(int id, UpdateBookDto updateBookDto)
    //Task<bool> DeleteBookAsync(int id)



}