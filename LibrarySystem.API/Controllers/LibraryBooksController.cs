using LibrarySystem.API.Errors;
using LibrarySystem.Application.Dtos.Books;
using LibrarySystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System;

namespace LibrarySystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("AuthPolicy")]
public class LibraryBooksController(ILibraryService libraryService) : ControllerBase
{
    [HttpGet("GetBooksByLibrary/{libraryId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [AllowAnonymous]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<ActionResult<IEnumerable<BookDto>>> GetBooksByLibrary(int libraryId)
    {
        IEnumerable<BookDto> books = await libraryService
            .GetBooksByLibraryAsync(libraryId)
            .ConfigureAwait(false);

        return Ok(books);
    }

    [HttpPost("AddBookToLibrary/{libraryId}/{bookId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Admin,Librarian")]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<ActionResult<BookDto>> AddBookToLibrary(int libraryId, int bookId)
    {
        try
        {
            BookDto bookDto = await libraryService
                .AddBookToLibraryAsync(libraryId, bookId)
                .ConfigureAwait(false);

            return Ok(bookDto);
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new ApiResponse(404, ex.Message));
            }
            return BadRequest(new ApiResponse(400, ex.Message));
        }
    }
}