using LibrarySystem.Domain.Commands;
using LibrarySystem.Domain.Commands.Books;
using LibrarySystem.Domain.Entities;
using LibrarySystem.Domain.Interfaces;

namespace LibrarySystem.Application.Commands.Handlers.Books;

public class ReturnBookCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<ReturnBookCommand>
{
    public async Task<CommandResult> HandleAsync(ReturnBookCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            if (string.IsNullOrWhiteSpace(command.UserId))
                return CommandResult.Fail("User ID is required");

            Book? book = await unitOfWork.Books.GetByIdAsync(command.BookId).ConfigureAwait(false);
            if (book == null)
                return CommandResult.Fail($"Book with ID {command.BookId} not found");

            // Get the SPECIFIC active borrow for this book AND user
            BorrowRecord? activeBorrow = await unitOfWork.BorrowRecords
                .GetActiveBorrowByBookAndUserAsync(command.BookId, command.UserId)
                .ConfigureAwait(false);

            if (activeBorrow == null)
                return CommandResult.Fail($"No active borrow record found for this book and user");

            // Return the book
            book.Return();
            book.UpdatedBy = command.CommandBy;

            activeBorrow.ReturnBook(command.FineAmount, command.Notes, command.Condition);
            activeBorrow.UpdatedBy = command.CommandBy;

            var success = await unitOfWork.CommitAsync().ConfigureAwait(false);

            return success
                ? CommandResult.Ok(activeBorrow)
                : CommandResult.Fail("Failed to return book");
        }
        catch (InvalidOperationException ex)
        {
            return CommandResult.Fail($"Error returning book: {ex.Message}");
        }
    }
}