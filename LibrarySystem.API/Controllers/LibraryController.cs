using LibrarySystem.API.Errors;
using LibrarySystem.Application.Dtos.Books;
using LibrarySystem.Application.Dtos.Libraries;
using LibrarySystem.Domain.Entities;
using LibrarySystem.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using AutoMapper;

namespace LibrarySystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("AuthPolicy")]
public class LibraryController(
    ILibraryRepository libraryRepository,
    IBookRepository bookRepository,
    IMapper mapper) : ControllerBase 
{
    [HttpGet("GetAllLibraries")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [AllowAnonymous]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<ActionResult<IEnumerable<LibraryDto>>> GetAllLibraries()
    {
        IEnumerable<Library> libraries = await libraryRepository
            .GetAllAsync()
            .ConfigureAwait(false);

        IEnumerable<LibraryDto> libraryDtos = mapper.Map<IEnumerable<LibraryDto>>(libraries);

        return Ok(libraryDtos);
    }

    [HttpGet("GetLibraryById/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<ActionResult<LibraryDetailsDto>> GetLibraryById(int id)
    {
        Library? library = await libraryRepository
            .GetByIdAsync(id)
            .ConfigureAwait(false);

        if (library == null)
            return NotFound(new ApiResponse(404, $"Library with ID {id} not found"));

        // Fetch associated books to include in the LibraryDetailsDto
        IEnumerable<Book> books = await bookRepository
            .GetBooksByLibraryAsync(id)
            .ConfigureAwait(false);

        LibraryDetailsDto libraryDetailsDto = mapper.Map<LibraryDetailsDto>(library);
        libraryDetailsDto.Books = mapper.Map<IEnumerable<BookDto>>(books);

        return Ok(libraryDetailsDto);
    }

    [HttpPost("CreateLibrary")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<ActionResult<LibraryDto>> CreateLibrary([FromBody] CreateLibraryDto createLibraryDto)
    {
        ArgumentNullException.ThrowIfNull(createLibraryDto);

        Library? existingLibrary = await libraryRepository
            .GetByNameAsync(createLibraryDto.Name)
            .ConfigureAwait(false);

        if (existingLibrary != null)
            return BadRequest(new ApiResponse(400, $"Library with name '{createLibraryDto.Name}' already exists"));

        Library library = mapper.Map<Library>(createLibraryDto);

        Library createdLibrary = await libraryRepository
            .AddAsync(library)
            .ConfigureAwait(false);

        LibraryDto resultDto = mapper.Map<LibraryDto>(createdLibrary);

        return CreatedAtAction(nameof(GetLibraryById), new { id = createdLibrary.Id }, resultDto);
    }

    [HttpPut("UpdateLibrary/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Admin,Librarian")]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<ActionResult<LibraryDto>> UpdateLibrary(int id, [FromBody] UpdateLibraryDto updateLibraryDto)
    {
        ArgumentNullException.ThrowIfNull(updateLibraryDto);

        Library? library = await libraryRepository
            .GetByIdAsync(id)
            .ConfigureAwait(false);

        if (library == null)
            return NotFound(new ApiResponse(404, $"Library with ID {id} not found"));

        mapper.Map(updateLibraryDto, library);

        await libraryRepository
            .UpdateAsync(library)
            .ConfigureAwait(false);

        LibraryDto resultDto = mapper.Map<LibraryDto>(library);
        return Ok(resultDto);
    }

    [HttpDelete("DeleteLibrary/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<ActionResult> DeleteLibrary(int id)
    {
        Library? library = await libraryRepository
            .GetByIdAsync(id)
            .ConfigureAwait(false);

        if (library == null)
            return NotFound(new ApiResponse(404, $"Library with ID {id} not found"));

        // Business rule check: Ensure no books are currently assigned before deletion
        IEnumerable<Book> books = await bookRepository
            .GetBooksByLibraryAsync(id)
            .ConfigureAwait(false);

        if (books.Any())
            return BadRequest(new ApiResponse(400, "Cannot delete library that contains books"));

        await libraryRepository
            .DeleteAsync(library)
            .ConfigureAwait(false);

        return NoContent();
    }
}