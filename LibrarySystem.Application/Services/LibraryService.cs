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

        book.Borrow();

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

        book.Return();

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
            .Where(book => book.CanBorrow());

        return borrowedBooks.Select(MapToBookDto);
    }

    public async Task<IEnumerable<BookDto>> GetBooksByLibraryAsync(int libraryId)
    {
        IEnumerable<Book> books = await unitOfWork.Books
            .GetBooksByLibraryAsync(libraryId)
            .ConfigureAwait(false);

        return books.Select(MapToBookDto);
    }


    public async Task<BookDto> MarkBookAsDamagedAsync(int bookId)
    {
        Book? book = await unitOfWork.Books
            .GetByIdAsync(bookId)
            .ConfigureAwait(false);

        if (book == null)
            throw new InvalidOperationException($"Book with ID {bookId} not found.");

        book.MarkAsDamaged();

        await unitOfWork.Books
            .UpdateAsync(book)
            .ConfigureAwait(false);

        bool success = await unitOfWork
            .CommitAsync()
            .ConfigureAwait(false);

        if (!success)
            throw new InvalidOperationException("Failed to mark book as damaged.");

        return MapToBookDto(book);
    }

    public async Task<BookDto> RestockBookAsync(int bookId, int additionalCopies)
    {
        Book? book = await unitOfWork.Books
            .GetByIdAsync(bookId)
            .ConfigureAwait(false);

        if (book == null)
            throw new InvalidOperationException($"Book with ID {bookId} not found.");

        book.Restock(additionalCopies);

        await unitOfWork.Books
            .UpdateAsync(book)
            .ConfigureAwait(false);

        bool success = await unitOfWork
            .CommitAsync()
            .ConfigureAwait(false);

        if (!success)
            throw new InvalidOperationException("Failed to restock book.");

        return MapToBookDto(book);
    }

    public async Task<BookStatsDto> GetBookStatsAsync(int bookId)
    {
        Book? book = await unitOfWork.Books
            .GetByIdAsync(bookId)
            .ConfigureAwait(false);

        if (book == null)
            throw new InvalidOperationException($"Book with ID {bookId} not found.");

        return new BookStatsDto
        {
            BookId = book.Id,
            Title = book.Title,
            TotalCopies = book.TotalCopies,
            CopiesAvailable = book.CopiesAvailable,
            BorrowedCopiesCount = book.BorrowedCopiesCount,
            UtilizationRate = book.UtilizationRate,
            IsAvailable = book.IsAvailable,
            IsOutOfStock = book.IsOutOfStock
        };
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

    public async Task<OverallBookStatsDto> GetOverallBooksStats()
    {
        IEnumerable<Book> allBooks = await unitOfWork.Books.GetAllAsync().ConfigureAwait(false);
        IEnumerable<Library> allLibraries = await unitOfWork.Libraries.GetAllAsync().ConfigureAwait(false);

        var stats = new OverallBookStatsDto
        {
            TotalBooks = allBooks.Count(),
            TotalCopies = allBooks.Sum(b => b.TotalCopies),
            AvailableCopies = allBooks.Sum(b => b.CopiesAvailable),
            BorrowedCopies = allBooks.Sum(b => b.BorrowedCopiesCount),
            OutOfStockBooks = allBooks.Count(b => b.CopiesAvailable == 0),
            AvailableBooks = allBooks.Count(b => b.CopiesAvailable > 0),
            TotalLibraries = allLibraries.Count()
        };

        stats.UtilizationRate = stats.TotalCopies > 0 ?
            (decimal)stats.BorrowedCopies / stats.TotalCopies * 100 : 0;

        stats.DamagedCopies = stats.TotalCopies - (stats.AvailableCopies + stats.BorrowedCopies);

        return stats;
    }
}