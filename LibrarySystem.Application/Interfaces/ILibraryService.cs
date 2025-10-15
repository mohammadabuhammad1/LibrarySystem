using LibrarySystem.Application.Dtos.Books;

namespace LibrarySystem.Application.Interfaces;

public interface ILibraryService
{

    Task<BookDto> BorrowBookAsync(int bookId);
    Task<BookDto> ReturnBookAsync(int bookId);
    Task<IEnumerable<BookDto>> GetBorrowedBooksAsync();
    Task<IEnumerable<BookDto>> GetAvailableBooksAsync();


}
