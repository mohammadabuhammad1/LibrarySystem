using LibrarySystem.Domain.Commands;
using LibrarySystem.Domain.Commands.Books;
using LibrarySystem.Domain.Entities;
using LibrarySystem.Domain.Interfaces;


namespace LibrarySystem.Application.Commands.Handlers.Books;

public class BorrowBookCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<BorrowBookCommand>
{
    public async Task<CommandResult> HandleAsync(BorrowBookCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            Book? book = await unitOfWork.Books.GetByIdTrackedAsync(command.BookId).ConfigureAwait(false);

            if (book == null)
                return CommandResult.Fail($"Book with ID {command.BookId} not found");

            if (!book.CanBorrow())
                return CommandResult.Fail("No copies available for borrowing");

            bool alreadyBorrowed = await unitOfWork.BorrowRecords
                .HasActiveBorrowForBookAsync(command.UserId, command.BookId)
                .ConfigureAwait(false);

            if (alreadyBorrowed)
                return CommandResult.Fail("User already has this book borrowed");

            var borrowRecord = BorrowRecord.Create(
                command.BookId,
                command.UserId,
                command.BorrowDurationDays,
                command.Notes);

            borrowRecord.CreatedBy = command.CommandBy;

            book.Borrow();
            book.UpdatedBy = command.CommandBy;

            // Add the new record
            await unitOfWork.BorrowRecords.AddAsync(borrowRecord).ConfigureAwait(false);

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