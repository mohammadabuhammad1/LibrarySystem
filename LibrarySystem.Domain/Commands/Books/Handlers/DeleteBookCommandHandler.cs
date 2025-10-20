using LibrarySystem.Domain.Commands;
using LibrarySystem.Domain.Commands.Books;
using LibrarySystem.Domain.Entities;
using LibrarySystem.Domain.Interfaces;
using System.Globalization;

namespace LibrarySystem.Application.Commands.Books.Handlers;

public class DeleteBookCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<DeleteBookCommand>
{
    public async Task<CommandResult> HandleAsync(DeleteBookCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            Book? book = await unitOfWork.Books
                .GetByIdAsync(command.Id)
                .ConfigureAwait(false);
            if (book == null)
                return CommandResult.Fail($"Book with ID {command.Id} not found");

            // Check if book has active borrows
            IEnumerable<Book> borrowedBooks = await unitOfWork.Books
                .GetBorrowedBooksByUserAsync(command.Id.ToString(CultureInfo.InvariantCulture))
                .ConfigureAwait(false);

            if (borrowedBooks.Any())
                return CommandResult.Fail("Cannot delete book with active borrows");

            await unitOfWork.Books
                .DeleteAsync(book)
                .ConfigureAwait(false);

            var success = await unitOfWork
                .CommitAsync()
                .ConfigureAwait(false);

            if (!success)
                return CommandResult.Fail("Failed to delete book");

            return CommandResult.Ok(true);
        }
        catch (InvalidOperationException ex)
        {
            return CommandResult.Fail($"Error deleting book: {ex.Message}");
        }
    }
}