using AutoMapper;
using LibrarySystem.Application.Dtos.Books;
using LibrarySystem.Application.Interfaces;
using LibrarySystem.Domain.Entities;
using LibrarySystem.Domain.Interfaces;

namespace LibrarySystem.Application.Services;

public class LibraryService(IUnitOfWork unitOfWork, IMapper mapper) : ILibraryService
{
    public async Task<BookDto> BorrowBookAsync(int bookId)
    {
        Book? book = await unitOfWork.Books
            .GetByIdTrackedAsync(bookId)
            .ConfigureAwait(false);

        if (book == null)
            throw new InvalidOperationException($"Book with ID {bookId} not found.");

        book.Borrow();

        // REMOVED: await unitOfWork . Books . Update Async (bo ok).C onfigur eAwait(fal se) 
        // This is redundant in EF Core if the entity is already tracked.

        bool success = await unitOfWork
            .CommitAsync()
            .ConfigureAwait(false);

        if (!success)
            throw new InvalidOperationException("Failed to borrow book.");

        return mapper.Map<BookDto>(book);
    }

    public async Task<BookDto> ReturnBookAsync(int bookId)
    {
        Book? book = await unitOfWork.Books
            .GetByIdTrackedAsync(bookId)
            .ConfigureAwait(false);

        if (book == null)
            throw new InvalidOperationException($"Book with ID {bookId} not found.");

        book.Return();

        bool success = await unitOfWork
            .CommitAsync()
            .ConfigureAwait(false);

        if (!success)
            throw new InvalidOperationException("Failed to return book.");

        return mapper.Map<BookDto>(book);
                                          
    }

    public async Task<IEnumerable<BookDto>> GetAvailableBooksAsync()
    {
        IEnumerable<Book> books = await unitOfWork.Books
            .GetAvailableBooksAsync()
            .ConfigureAwait(false);

        return mapper.Map<IEnumerable<BookDto>>(books);
    }

    public async Task<IEnumerable<BookDto>> GetBorrowedBooksAsync()
    {
        IEnumerable<Book> allBooks = await unitOfWork.Books
            .GetAllAsync()
            .ConfigureAwait(false);

        IEnumerable<Book> borrowedBooks = allBooks
                    .Where(book => book.BorrowedCopiesCount > 0);

        return mapper.Map<IEnumerable<BookDto>>(borrowedBooks); 
    }

    public async Task<IEnumerable<BookDto>> GetBooksByLibraryAsync(int libraryId)
    {
        IEnumerable<Book> books = await unitOfWork.Books
            .GetBooksByLibraryAsync(libraryId)
            .ConfigureAwait(false);

        return mapper.Map<IEnumerable<BookDto>>(books);
    }

    public async Task<BookDto> MarkBookAsDamagedAsync(int bookId)
    {
        Book? book = await unitOfWork.Books
            .GetByIdTrackedAsync(bookId)
            .ConfigureAwait(false);

        if (book == null)
            throw new InvalidOperationException($"Book with ID {bookId} not found.");

        book.MarkAsDamaged();

        bool success = await unitOfWork
            .CommitAsync()
            .ConfigureAwait(false);

        if (!success)
            throw new InvalidOperationException("Failed to mark book as damaged.");

        return mapper.Map<BookDto>(book);
    }

    public async Task<BookDto> RestockBookAsync(int bookId, int additionalCopies)
    {
        Book? book = await unitOfWork.Books
            .GetByIdTrackedAsync(bookId)
            .ConfigureAwait(false);

        if (book == null)
            throw new InvalidOperationException($"Book with ID {bookId} not found.");

        book.Restock(additionalCopies);

        bool success = await unitOfWork
            .CommitAsync()
            .ConfigureAwait(false);

        if (!success)
            throw new InvalidOperationException("Failed to restock book.");

        return mapper.Map<BookDto>(book);
    }

    public async Task<BookStatsDto> GetBookStatsAsync(int bookId)
    {
        Book? book = await unitOfWork.Books
            .GetByIdAsync(bookId)
            .ConfigureAwait(false);

        if (book == null)
            throw new InvalidOperationException($"Book with ID {bookId} not found.");

        return mapper.Map<BookStatsDto>(book);

    }

    public async Task<OverallBookStatsDto> GetOverallBooksStats()
    {
        IEnumerable<Book> allBooks = await unitOfWork.Books
            .GetAllAsync()
            .ConfigureAwait(false);

        IEnumerable<Library> allLibraries = await unitOfWork.Libraries
            .GetAllAsync()
            .ConfigureAwait(false);

        OverallBookStatsDto stats = new OverallBookStatsDto
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

    public async Task<BookDto> AddBookToLibraryAsync(int libraryId, int bookId)
    {
        Library? library = await unitOfWork.Libraries
            .GetByIdAsync(libraryId)
            .ConfigureAwait(false);

        if (library == null)
            throw new InvalidOperationException($"Library with ID {libraryId} not found");

        Book? book = await unitOfWork.Books
            .GetByIdTrackedAsync(bookId)
            .ConfigureAwait(false);

        if (book == null)
            throw new InvalidOperationException($"Book with ID {bookId} not found");

        book.AssignToLibrary(libraryId);

        bool success = await unitOfWork
            .CommitAsync()
            .ConfigureAwait(false);

        if (!success)
            throw new InvalidOperationException("Failed to add book to library");

        return mapper.Map<BookDto>(book);
    }
}