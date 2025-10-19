using LibrarySystem.Application.Dtos.Books;
using LibrarySystem.Application.Interfaces;
using LibrarySystem.Domain.Entities;
using LibrarySystem.Domain.Interfaces;

namespace LibrarySystem.Application.Services;

public class LibraryService(IUnitOfWork unitOfWork) : ILibraryService
{
    public async Task<BookDto> BorrowBookAsync(int bookId)
    {
        Book? book = await unitOfWork.Books
            .GetByIdAsync(bookId)
            .ConfigureAwait(false);

        if (book == null)
            throw new InvalidOperationException($"Book with ID {bookId} not found.");

        if (book.CopiesAvailable <= 0)
            throw new InvalidOperationException($"No copies available for book with ID {bookId}.");

        book.CopiesAvailable--;
        await unitOfWork.Books
            .UpdateAsync(book)
            .ConfigureAwait(false);

        bool success = await unitOfWork
            .CommitAsync()
            .ConfigureAwait(false);

        if (!success)
            throw new InvalidOperationException("Failed to borrow book.");  

        return MapToBookDto(book);
    }

    public async Task<BookDto> ReturnBookAsync(int bookId)
    {
        Book? book = await unitOfWork.Books
            .GetByIdAsync(bookId)
            .ConfigureAwait(false);

        if (book == null)
            throw new InvalidOperationException($"Book with ID {bookId} not found.");

        if (book.CopiesAvailable >= book.TotalCopies)
            throw new InvalidOperationException($"'{book.Title}' is not currently borrowed.");

        book.CopiesAvailable++;

        await unitOfWork.Books
            .UpdateAsync(book)
            .ConfigureAwait(false);

        bool success = await unitOfWork
            .CommitAsync()
            .ConfigureAwait(false);

        if (!success)
            throw new InvalidOperationException("Failed to return book.");

        return MapToBookDto(book);
    }

    public async Task<IEnumerable<BookDto>> GetAvailableBooksAsync()
    {
        IEnumerable<Book> books = await unitOfWork.Books
            .GetAvailableBooksAsync()
            .ConfigureAwait(false);

        return books.Select(MapToBookDto);
    }

    public async Task<IEnumerable<BookDto>> GetBorrowedBooksAsync()
    {
        IEnumerable<Book> allBooks = await unitOfWork.Books
            .GetAllAsync()
            .ConfigureAwait(false);

        IEnumerable<Book> borrowedBooks = allBooks
            .Where(book => book.CopiesAvailable < book.TotalCopies);

        return borrowedBooks.Select(MapToBookDto);
    }

    public async Task<IEnumerable<BookDto>> GetBooksByLibraryAsync(int libraryId)
    {
        IEnumerable<Book> books = await unitOfWork.Books
            .GetBooksByLibraryAsync(libraryId)
            .ConfigureAwait(false);

        return books.Select(MapToBookDto);
    }

    private static BookDto MapToBookDto(Book book)
    {
        return new BookDto
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            ISBN = book.ISBN,
            PublishedYear = book.PublishedYear,
            TotalCopies = book.TotalCopies,
            CopiesAvailable = book.CopiesAvailable,
            LibraryId = book.LibraryId ?? 0
        };
    }
}