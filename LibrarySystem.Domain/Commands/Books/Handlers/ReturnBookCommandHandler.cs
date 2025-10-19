using LibrarySystem.Domain.Commands.Books;
using LibrarySystem.Domain.Interfaces;

namespace LibrarySystem.Application.Commands.Books.Handlers;

public class ReturnBookCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<ReturnBookCommand>
{
    public async Task<CommandResult> HandleAsync(ReturnBookCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            // Validate command
            if (string.IsNullOrWhiteSpace(command.UserId))
                return CommandResult.Fail("User ID is required");

            // Get book
            Domain.Entities.Book? book = await unitOfWork.Books.GetByIdAsync(command.BookId).ConfigureAwait(false);
            if (book == null)
                return CommandResult.Fail($"Book with ID {command.BookId} not found");

            // Get active borrow record
            Domain.Entities.BorrowRecord? activeBorrow = await unitOfWork.BorrowRecords.GetActiveBorrowByBookAndUserAsync(
                command.BookId, command.UserId).ConfigureAwait(false);

            if (activeBorrow == null)
                return CommandResult.Fail("No active borrow record found for this book and user");

            // Use domain methods
            book.Return();
            book.UpdatedBy = command.CommandBy;

            activeBorrow.ReturnBook(command.FineAmount, command.Notes, command.Condition);
            activeBorrow.UpdatedBy = command.CommandBy;

            // Update entities
            await unitOfWork.Books.UpdateAsync(book).ConfigureAwait(false);
            await unitOfWork.BorrowRecords.UpdateAsync(activeBorrow).ConfigureAwait(false);

            var success = await unitOfWork.CommitAsync().ConfigureAwait(false);

            if (!success)
                return CommandResult.Fail("Failed to return book");

            return CommandResult.Ok(activeBorrow);
        }
        catch (InvalidOperationException ex)
        {
            return CommandResult.Fail($"Error returning book: {ex.Message}");
        }
        catch (ArgumentException ex)
        {
            return CommandResult.Fail($"Invalid command: {ex.Message}");
        }
    }
}