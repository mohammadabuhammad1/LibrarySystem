using LibrarySystem.Domain.Commands;
using LibrarySystem.Domain.Commands.Books;
using LibrarySystem.Domain.Entities;
using LibrarySystem.Domain.Interfaces;

namespace LibrarySystem.Application.Commands.Books.Handlers;

public class BorrowBookCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<BorrowBookCommand>
{
    public async Task<CommandResult> HandleAsync(BorrowBookCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            Book? book = await unitOfWork.Books.GetByIdAsync(command.BookId).ConfigureAwait(false);
            if (book == null)
                return CommandResult.Fail($"Book with ID {command.BookId} not found");

            if (!book.CanBorrow())
                return CommandResult.Fail("No copies available for borrowing");

            // Efficiently check for existing borrow record directly on the repository if possible, 
            // but keeping current logic structure for minimal changes:
            IEnumerable<BorrowRecord> activeBorrows = await unitOfWork.BorrowRecords.GetActiveBorrowsByUserAsync(command.UserId).ConfigureAwait(false);
            if (activeBorrows.Any(br => br.BookId == command.BookId))
                return CommandResult.Fail("User already has this book borrowed");

            var borrowRecord = BorrowRecord.Create(
                command.BookId,
                command.UserId,
                command.BorrowDurationDays,
                command.Notes);

            borrowRecord.CreatedBy = command.CommandBy;

            // Apply change to the tracked entity
            book.Borrow();
            book.UpdatedBy = command.CommandBy;

            // Add the new record
            await unitOfWork.BorrowRecords.AddAsync(borrowRecord).ConfigureAwait(false);

            // REDUNDANCY REMOVED: await unitOfWork.Books.UpdateAsync(book).ConfigureAwait(false); // The change to 'book' is tracked by EF Core.

            var success = await unitOfWork.CommitAsync().ConfigureAwait(false);

            if (!success)
                return CommandResult.Fail("Failed to borrow book");

            return CommandResult.Ok(borrowRecord);
        }
        catch (InvalidOperationException ex)
        {
            return CommandResult.Fail($"Error borrowing book: {ex.Message}");
        }
    }
}