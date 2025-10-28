using AutoMapper;
using LibrarySystem.Application.Dtos.Books;
using LibrarySystem.Domain.Entities;
using LibrarySystem.Domain.Interfaces;
using LibrarySystem.Domain.Queries;
using LibrarySystem.Domain.ValueObjects;

namespace LibrarySystem.Application.Queries.Handlers;

public class GetBookByIsbnQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) : IQueryHandler<GetBookByIsbnQuery, BookDto?>
{
    public async Task<QueryResult<BookDto?>> HandleAsync(GetBookByIsbnQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        try
        {
            // Validate ISBN using Value Object
            if (!Isbn.TryCreate(query.ISBN, out Isbn? isbn))
            {
                return QueryResult<BookDto?>.Fail($"Invalid ISBN format: {query.ISBN}");
            }

            Book? book = await unitOfWork.Books.GetByIsbnAsync(isbn!.Value).ConfigureAwait(false);

            if (book is null)
            {
                return QueryResult<BookDto?>.Fail($"Book with ISBN {isbn.FormattedValue} not found");
            }

            BookDto bookDto = mapper.Map<BookDto>(book);
            return QueryResult<BookDto?>.Ok(bookDto);
        }
        catch (InvalidOperationException ex)
        {
            return QueryResult<BookDto?>.Fail($"Error retrieving book: {ex.Message}");
        }
    }
}