using LibrarySystem.Domain.Commands;
using LibrarySystem.Domain.Commands.Books;
using LibrarySystem.Domain.Entities;
using LibrarySystem.Domain.Interfaces;

namespace LibrarySystem.Application.Commands.Handlers.Books;

public class DeleteBookCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<DeleteBookCommand>
{
    public async Task<CommandResult> HandleAsync(DeleteBookCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            Book? book = await unitOfWork.Books
                .GetByIdTrackedAsync(command.Id)
                .ConfigureAwait(false);

            if (book == null)
                return CommandResult.Fail($"Book with ID {command.Id} not found");

            // Check if book has active borrows - FIXED: Method returns single record, not collection
            BorrowRecord? activeBorrow = await unitOfWork.BorrowRecords
                .GetActiveBorrowByBookAsync(command.Id)
                .ConfigureAwait(false);

            if (activeBorrow != null)
                return CommandResult.Fail("Cannot delete book with active borrows");

            // Soft delete
            book.MarkAsDeleted(command.CommandBy);

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