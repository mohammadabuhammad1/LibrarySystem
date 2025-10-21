using AutoMapper;
using LibrarySystem.Application.Dtos.Books;
using LibrarySystem.Application.Interfaces;
using LibrarySystem.Domain.Entities;
using LibrarySystem.Domain.Interfaces;

namespace LibrarySystem.Application.Services;

public class BookService(IUnitOfWork unitOfWork, IMapper mapper) : IBookService
{
    public async Task<BookDto?> GetBookByIdAsync(int id)
    {
        Book? book = await unitOfWork.Books
            .GetByIdAsync(id)
            .ConfigureAwait(false);

        return mapper.Map<BookDto>(book);
    }

    public async Task<IEnumerable<BookDto>> GetAllBooksAsync()
    {
        IEnumerable<Book> books = await unitOfWork.Books
            .GetAllAsync()
            .ConfigureAwait(false);

        return mapper.Map<IEnumerable<BookDto>>(books);

    }

    public async Task<BookDto?> GetBookByIsbnAsync(string isbn)
    {
        Book? book = await unitOfWork.Books
            .GetByIsbnAsync(isbn)
            .ConfigureAwait(false);

        return mapper.Map<BookDto>(book);
    }

    public async Task<IEnumerable<BookDto>> GetAvailableBooksAsync()
    {
        IEnumerable<Book> books = await unitOfWork.Books
            .GetAvailableBooksAsync()
            .ConfigureAwait(false);

        return mapper.Map<IEnumerable<BookDto>>(books);
    }

    public async Task<IEnumerable<BookDto>> GetBooksByLibraryAsync(int libraryId)
    {
        IEnumerable<Book> books = await unitOfWork.Books
            .GetBooksByLibraryAsync(libraryId)
            .ConfigureAwait(false);

        return mapper.Map<IEnumerable<BookDto>>(books);
    }

    public async Task<bool> BookExistsAsync(int id)
    {
        return await unitOfWork.Books
            .ExistsAsync(id)
            .ConfigureAwait(false);

    }

    public async Task<IEnumerable<BookDto>> GetBorrowedBooksByUserAsync(string userId)
    {
        IEnumerable<Book> books = await unitOfWork.Books
            .GetBorrowedBooksByUserAsync(userId)
            .ConfigureAwait(false);

        return mapper.Map<IEnumerable<BookDto>>(books);
    }

    public async Task<OverallBookStatsDto> GetOverallBookStatsAsync()
    {
        IEnumerable<Book> allBooks = await unitOfWork.Books.GetAllAsync().ConfigureAwait(false);

        OverallBookStatsDto stats = new OverallBookStatsDto
        {
            TotalBooks = allBooks.Count(),
            TotalCopies = allBooks.Sum(b => b.TotalCopies),
            AvailableCopies = allBooks.Sum(b => b.CopiesAvailable),
            BorrowedCopies = allBooks.Sum(b => b.BorrowedCopiesCount),
            OutOfStockBooks = allBooks.Count(b => b.CopiesAvailable == 0),
            AvailableBooks = allBooks.Count(b => b.CopiesAvailable > 0)
        };

        IEnumerable<Library> allLibraries = await unitOfWork.Libraries.GetAllAsync().ConfigureAwait(false);
        stats.TotalLibraries = allLibraries.Count();

        stats.UtilizationRate = stats.TotalCopies > 0 ?
            (decimal)stats.BorrowedCopies / stats.TotalCopies * 100 : 0;

        stats.DamagedCopies = stats.TotalCopies - (stats.AvailableCopies + stats.BorrowedCopies);

        return stats;
    }

    /*
    public async Task<BookDto?> CreateBookAsync(CreateBookDto createBookDto)
    
        if (createBookDto is null)
            return null

        Book book = Book.Create(
            createBookDto.Title,
            createBookDto.Author,
            createBookDto.ISBN,
            createBookDto.PublishedYear,
            createBookDto.TotalCopies
        )


        Book? createdBook = await unitOfWork.Books
            .AddAsync(book)
            .ConfigureAwait(false)

        bool success = await unitOfWork
            .CommitAsync()
            .ConfigureAwait(false)

        if (!success)
            throw new InvalidOperationException("Failed to create book")

        return mapper.Map<BookDto>(createdBook)
    

    public async Task<BookDto?> UpdateBookAsync(int id, UpdateBookDto updateBookDto)
    
        if (updateBookDto is null)
            return null


        Book? book = await unitOfWork.Books
            .GetByIdAsync(id)
            .ConfigureAwait(false)

        if  book = = null  retur n nu ll

        book.UpdateDetails(
            updateBookDto.Title,
            updateBookDto.Author,
            updateBookDto.ISBN,
            updateBookDto.PublishedYear,
            updateBookDto.TotalCopies
        )

        await unitOfWork.Books
            .UpdateAsync(book)
            .ConfigureAwait(false)

        bool success = await unitOfWork
            .CommitAsync()
            .ConfigureAwait(false)

        if (!success)
            throw new InvalidOperationException("Failed to update book")

        return mapper.Map<BookDto>(book)
    

    public async Task<bool> DeleteBookAsync(int id)
    
        Book? book = await unitOfWork.Books
            .GetByIdAsync(id)
            .ConfigureAwait(false)

        if book = = nu ll ret urn fa lse

        await unitOfWork.Books
            .DeleteAsync(book)
            .ConfigureAwait(false)

        bool success = await unitOfWork
            .CommitAsync()
            .ConfigureAwait(false)

        if (!success)
            throw new InvalidOperationException("Failed to delete book")

        return true
    */



}