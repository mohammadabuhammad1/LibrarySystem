using LibrarySystem.Domain.Commands;
using LibrarySystem.Domain.Commands.Books;
using LibrarySystem.Domain.Entities;
using LibrarySystem.Domain.Interfaces;

namespace LibrarySystem.Application.Commands.Handlers.Books;

public class UpdateBookCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<UpdateBookCommand>
{
    public async Task<CommandResult> HandleAsync(UpdateBookCommand command)
    {

        ArgumentNullException.ThrowIfNull(command);

        try
        {
            Book? book = await unitOfWork.Books.GetByIdTrackedAsync(command.Id).ConfigureAwait(false);
            if (book == null)
                return CommandResult.Fail($"Book with ID {command.Id} not found");

            book.Update(
                command.Title,
                command.Author,
                command.PublishedYear,
                command.TotalCopies,
                command.Description,
                command.Genre);

            book.UpdatedBy = command.CommandBy;

            var success = await unitOfWork.CommitAsync().ConfigureAwait(false);

            if (!success)
                return CommandResult.Fail("Failed to update book");

            return CommandResult.Ok(book);
        }
        catch (InvalidOperationException ex)
        {
            return CommandResult.Fail($"Error updating book: {ex.Message}");
        }
    }
}