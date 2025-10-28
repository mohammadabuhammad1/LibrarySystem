using AutoMapper;
using LibrarySystem.API.Errors;
using LibrarySystem.Application.Dtos.Books;
using LibrarySystem.Application.Interfaces;
using LibrarySystem.Application.Queries.Handlers;
using LibrarySystem.Domain.Commands;
using LibrarySystem.Domain.Commands.Books;
using LibrarySystem.Domain.Entities;
using LibrarySystem.Domain.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace LibrarySystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("AuthPolicy")]
public class BooksController(
    IBookService bookService,
    IBorrowRecordService borrowRecordService,
    ILibraryService libraryService,
    UserManager<ApplicationUser> userManager,
    ICommandDispatcher dispatcher,
    IQueryDispatcher queryDispatcher,
    IMapper mapper)
    : BaseApiController(userManager)
{
    [HttpGet("GetAllBooks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [AllowAnonymous]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<ActionResult<IEnumerable<BookDto>>> GetAllBooks()
    {
        IEnumerable<BookDto> books = await bookService
            .GetAllBooksAsync()
            .ConfigureAwait(false);

        return Ok(books);
    }

    [HttpGet("GetBookById/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    public async Task<ActionResult<BookDto>> GetBookById(int id)
    {
        BookDto? book = await bookService
            .GetBookByIdAsync(id)
            .ConfigureAwait(false);

        if (book == null)
            return NotFound(new ApiResponse(404, $"Book with ID {id} not found"));

        return Ok(book);
    }

    [HttpPost("CreateBook")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "Admin,Librarian")]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<ActionResult<BookDto>> CreateBook(CreateBookDto createBookDto)
    {
        ArgumentNullException.ThrowIfNull(createBookDto);

        ApplicationUser? currentUser = await GetCurrentUserAsync().ConfigureAwait(false);
        if (currentUser == null)
            return Unauthorized(new ApiResponse(401));

        // Pre-check outside the command handler for HTTP response clarity
        if (await bookService.GetBookByIsbnAsync(createBookDto.ISBN).ConfigureAwait(false) != null)
            return BadRequest(new ApiResponse(400, $"Book with ISBN {createBookDto.ISBN} already exists"));

        CreateBookCommand command = mapper.Map<CreateBookCommand>(createBookDto);
        command.CommandBy = currentUser.Id;

        CommandResult result = await dispatcher.DispatchAsync(command).ConfigureAwait(false);

        if (result.IsFailure)
            return BadRequest(new ApiResponse(400, result.Error));

        var createdBook = result.Value;
        BookDto bookDto = mapper.Map<BookDto>(createdBook);

        return CreatedAtAction(nameof(GetBookById), new { id = bookDto.Id }, bookDto);
    }
    [HttpPut("UpdateBook/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Admin,Librarian")]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<ActionResult<BookDto>> UpdateBook(int id, UpdateBookDto updateBookDto)
    {
        ArgumentNullException.ThrowIfNull(updateBookDto);

        ApplicationUser? currentUser = await GetCurrentUserAsync().ConfigureAwait(false);
        if (currentUser == null)
            return Unauthorized(new ApiResponse(401));

        UpdateBookCommand command = mapper.Map<UpdateBookCommand>(updateBookDto);
        command.Id = id;
        command.CommandBy = currentUser.Id;

        CommandResult result = await dispatcher.DispatchAsync(command).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result.Error switch
            {
                string error when error.Contains("not found", StringComparison.OrdinalIgnoreCase)
                    => NotFound(new ApiResponse(404, result.Error)),
                string error when error.StartsWith("Validation failed:", StringComparison.OrdinalIgnoreCase)
                    => BadRequest(new ApiResponse(400, result.Error)),
                _ => BadRequest(new ApiResponse(400, result.Error))
            };
        }

        var updatedBook = result.Value;
        BookDto bookDto = mapper.Map<BookDto>(updatedBook);

        return Ok(bookDto);
    }
    [HttpDelete("DeleteBook/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<ActionResult> DeleteBook(int id)
    {
        ApplicationUser? currentUser = await GetCurrentUserAsync().ConfigureAwait(false);
        if (currentUser == null)
            return Unauthorized(new ApiResponse(401));

        var command = new DeleteBookCommand { Id = id, CommandBy = currentUser.Id };

        CommandResult result = await dispatcher.DispatchAsync(command).ConfigureAwait(false);

        if (result.IsFailure)
        {
            if (result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(new ApiResponse(404, result.Error));

            return BadRequest(new ApiResponse(400, result.Error));
        }

        return NoContent();
    }

    [HttpGet("isbn/{isbn}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [AllowAnonymous]
    public async Task<ActionResult<BookDto>> GetBookByIsbn(string isbn)
    {
        GetBookByIsbnQuery query = new (isbn);
        QueryResult<BookDto?> result = await queryDispatcher.DispatchAsync<GetBookByIsbnQuery, BookDto?>(query).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            return result.Error.Contains("Invalid ISBN", StringComparison.OrdinalIgnoreCase)
                ? BadRequest(new ApiResponse(400, result.Error))
                : NotFound(new ApiResponse(404, result.Error));
        }

        return Ok(result.Data);
    }

    [HttpPost("restock/{bookId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<ActionResult<BookDto>> RestockBook(int bookId, [FromBody] int additionalCopies)
    {
        ApplicationUser? currentUser = await GetCurrentUserAsync().ConfigureAwait(false);
        if (currentUser == null)
            return Unauthorized(new ApiResponse(401));

        if (additionalCopies <= 0)
            return BadRequest(new ApiResponse(400, "Restock copies must be greater than zero."));

        // Create and dispatch Command
        UpdateBookCopiesCommand command = new()
        {
            Id = bookId,
            TotalCopies = additionalCopies,
            CommandBy = currentUser.Id
        };

        CommandResult result = await dispatcher.DispatchAsync(command).ConfigureAwait(false);

        if (result.IsFailure)
        {
            if (result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(new ApiResponse(404, result.Error));

            return BadRequest(new ApiResponse(400, result.Error));
        }

        // Map result entity to DTO
        var updatedBook = result.Value;
        BookDto bookDto = mapper.Map<BookDto>(updatedBook);
        return Ok(bookDto);
    }

    [HttpGet("available")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [AllowAnonymous]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<ActionResult<IEnumerable<BookDto>>> GetAvailableBooks()
    {
        IEnumerable<BookDto> books = await bookService
            .GetAvailableBooksAsync()
            .ConfigureAwait(false);

        return Ok(books);
    }

    [HttpGet("library/{libraryId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<BookDto>>> GetBooksByLibrary(int libraryId)
    {
        IEnumerable<BookDto> books = await bookService
            .GetBooksByLibraryAsync(libraryId)
            .ConfigureAwait(false);

        return Ok(books);
    }

    [HttpGet("exists/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [AllowAnonymous]
    public async Task<ActionResult<bool>> BookExists(int id)
    {
        bool exists = await bookService
            .BookExistsAsync(id)
            .ConfigureAwait(false);

        return Ok(exists);
    }
    [HttpGet("stats/{bookId:int}")] 
    [ProducesResponseType(StatusCodes.Status200OK)]
    [AllowAnonymous]
    public async Task<ActionResult<BookStatsDto>> GetBookStats(int bookId)
    {
        BookStatsDto stats = await libraryService.GetBookStatsAsync(bookId).ConfigureAwait(false);
        return Ok(stats);
    }

    [HttpGet("my-borrowed-books")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<ActionResult<IEnumerable<BookDto>>> GetMyBorrowedBooks()
    {
        string? currentUserId = GetCurrentUserId();

        if (string.IsNullOrEmpty(currentUserId))
            return Unauthorized(new ApiResponse(401));

        IEnumerable<BookDto> borrowedBooks = await bookService
            .GetBorrowedBooksByUserAsync(currentUserId)
            .ConfigureAwait(false);

        return Ok(borrowedBooks);
    }

    [HttpGet("my-active-borrows")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<ActionResult<IEnumerable<BorrowRecordDto>>> GetMyActiveBorrows()
    {
        string? currentUserId = GetCurrentUserId();

        if (string.IsNullOrEmpty(currentUserId))
            return Unauthorized(new ApiResponse(401));

        IEnumerable<BorrowRecordDto> activeBorrows = await borrowRecordService
            .GetActiveBorrowsByUserAsync(currentUserId)
            .ConfigureAwait(false);

        return Ok(activeBorrows);
    }

    [HttpPost("borrow/{bookId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<ActionResult<BorrowRecordDto>> BorrowBook(int bookId)
    {
        ApplicationUser? currentUser = await GetCurrentUserAsync().ConfigureAwait(false);
        if (currentUser == null)
            return Unauthorized(new ApiResponse(401));

        // Policy Check (Query) remains here as quick failure mechanism
        bool canBorrow = await borrowRecordService
            .CanUserBorrowAsync(currentUser.Id)
            .ConfigureAwait(false);

        if (!canBorrow)
            return BadRequest(new ApiResponse(400, "Cannot borrow book. Check if you have overdue books or reached borrowing limit."));

        // Create and dispatch Command
        var command = new BorrowBookCommand
        {
            UserId = currentUser.Id,
            BookId = bookId,
            BorrowDurationDays = 14,
            CommandBy = currentUser.Id
        };

        CommandResult result = await dispatcher.DispatchAsync(command).ConfigureAwait(false);

        if (result.IsFailure)
            return BadRequest(new ApiResponse(400, result.Error));

        var borrowRecord = result.Value;
        BorrowRecordDto recordDto = mapper.Map<BorrowRecordDto>(borrowRecord);

        return Ok(recordDto);
    }

    [HttpGet("overall-stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [AllowAnonymous]
    public async Task<ActionResult<OverallBookStatsDto>> GetOverallBooksStats()
    {
        OverallBookStatsDto stats = await bookService.GetOverallBookStatsAsync().ConfigureAwait(false);
        return Ok(stats);
    }
    //All the above bug free




    [HttpPost("{bookId:int}/return")]
    public async Task<ActionResult<BorrowRecordDto>> ReturnBook(int bookId, [FromBody] ReturnBookDto returnDto)
    {
        ArgumentNullException.ThrowIfNull(returnDto);

        ApplicationUser? currentUser = await GetCurrentUserAsync().ConfigureAwait(false);
        if (currentUser == null)
            return Unauthorized(new ApiResponse(401));

        ReturnBookCommand command = new()
        {
            BookId = bookId,
            UserId = returnDto.UserId,
            Notes = returnDto.Notes,
            Condition = returnDto.Condition,
            FineAmount = returnDto.FineAmount,
            CommandBy = currentUser.Id 
        };

        CommandResult result = await dispatcher.DispatchAsync(command).ConfigureAwait(false);

        if (result.IsFailure)
        {
            if (result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(new ApiResponse(404, result.Error));
            return BadRequest(new ApiResponse(400, result.Error));
        }

        var borrowRecord = result.Value;
        BorrowRecordDto recordDto = mapper.Map<BorrowRecordDto>(borrowRecord);
        return Ok(recordDto);
    }

    [HttpGet("my-fines")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<decimal>> GetMyTotalFines()
    {
        string? currentUserId = GetCurrentUserId();
        if (string.IsNullOrEmpty(currentUserId))
            return Unauthorized(new ApiResponse(401));

        IEnumerable<BorrowRecordDto> borrowHistory = await borrowRecordService
            .GetUserBorrowHistoryAsync(currentUserId)
            .ConfigureAwait(false);

        decimal totalFines = borrowHistory.Sum(b => b.FineAmount ?? 0);

        return Ok(totalFines);
    }

    [HttpPost("mark-damaged/{bookId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<ActionResult<BookDto>> MarkBookAsDamaged(int bookId)
    {
        ApplicationUser? currentUser = await GetCurrentUserAsync().ConfigureAwait(false);
        if (currentUser == null)
            return Unauthorized(new ApiResponse(401));

  
        UpdateBookCopiesCommand command = new()
        {
            Id = bookId,
            TotalCopies = -1, 
            Reason = "Damaged",
            CommandBy = currentUser.Id
        };

        CommandResult result = await dispatcher.DispatchAsync(command).ConfigureAwait(false);

        if (result.IsFailure)
            return BadRequest(new ApiResponse(400, result.Error));

        // Map result entity to DTO
        var updatedBook = result.Value;
        BookDto bookDto = mapper.Map<BookDto>(updatedBook);
        return Ok(bookDto);
    }
}