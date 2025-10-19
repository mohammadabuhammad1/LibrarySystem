using LibrarySystem.Application.Dtos.Books;
using LibrarySystem.Application.Interfaces;
using LibrarySystem.Domain.Entities;
using LibrarySystem.Domain.Interfaces;
using static System.Reflection.Metadata.BlobBuilder;

namespace LibrarySystem.Application.Services;

public class BookService(IUnitOfWork unitOfWork) : IBookService
{
    public async Task<BookDto?> GetBookByIdAsync(int id)
    {
        Book? book = await unitOfWork.Books
            .GetByIdAsync(id)
            .ConfigureAwait(false);

        return book == null ? null : MapToBookDto(book);
    }

    public async Task<IEnumerable<BookDto>> GetAllBooksAsync()
    {
        IEnumerable<Book> books = await unitOfWork.Books
            .GetAllAsync()
            .ConfigureAwait(false);

        return books.Select(MapToBookDto);
    }

    public async Task<BookDto?> CreateBookAsync(CreateBookDto createBookDto)
    {
        if (createBookDto is null)
            return null;

        var book = new Book
        {
            Title = createBookDto.Title,
            Author = createBookDto.Author,
            ISBN = createBookDto.ISBN,
            PublishedYear = createBookDto.PublishedYear,
            TotalCopies = createBookDto.TotalCopies,
            CopiesAvailable = createBookDto.TotalCopies,
            CreatedAt = DateTime.UtcNow
            
        };

        Book? createdBook = await unitOfWork.Books
            .AddAsync(book)
            .ConfigureAwait(false);

        bool success = await unitOfWork
            .CommitAsync()
            .ConfigureAwait(false);

        if(!success)
            throw new InvalidOperationException("Failed to create book");

        return MapToBookDto(createdBook);
    }

    public async Task<BookDto?> UpdateBookAsync(int id, UpdateBookDto updateBookDto)
    {
        if (updateBookDto is null)
            return null;


        Book? book = await unitOfWork.Books
            .GetByIdAsync(id)
            .ConfigureAwait(false);

        if (book == null) return null;

        book.Title = updateBookDto.Title;
        book.Author = updateBookDto.Author;
        book.PublishedYear = updateBookDto.PublishedYear;
        book.TotalCopies = updateBookDto.TotalCopies;

        await unitOfWork.Books
            .UpdateAsync(book)
            .ConfigureAwait(false);

        bool success = await unitOfWork
            .CommitAsync()
            .ConfigureAwait(false);

        if(!success)
            throw new InvalidOperationException("Failed to update book");

        return MapToBookDto(book);
    }

    public async Task<bool> DeleteBookAsync(int id)
    {
        Book? book = await unitOfWork.Books
            .GetByIdAsync(id)
            .ConfigureAwait(false);

        if (book == null) return false;

        await unitOfWork.Books
            .DeleteAsync(book)
            .ConfigureAwait(false);

        bool success = await unitOfWork
            .CommitAsync()
            .ConfigureAwait(false);

        if(!success)
            throw new InvalidOperationException("Failed to delete book");

        return true;
    }

    public async Task<BookDto?> GetBookByIsbnAsync(string isbn)
    {
        Book? book = await unitOfWork.Books
            .GetByIsbnAsync(isbn)
            .ConfigureAwait(false);

        return book == null ? null : MapToBookDto(book);
    }

    public async Task<IEnumerable<BookDto>> GetAvailableBooksAsync()
    {
        IEnumerable<Book> books = await unitOfWork.Books
            .GetAvailableBooksAsync()
            .ConfigureAwait(false);

        return books.Select(MapToBookDto);
    }

    public async Task<IEnumerable<BookDto>> GetBooksByLibraryAsync(int libraryId)
    {
        IEnumerable<Book> books = await unitOfWork.Books
            .GetBooksByLibraryAsync(libraryId)
            .ConfigureAwait(false);

        return books.Select(MapToBookDto);
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
            CopiesAvailable = book.CopiesAvailable
        };
    }
}