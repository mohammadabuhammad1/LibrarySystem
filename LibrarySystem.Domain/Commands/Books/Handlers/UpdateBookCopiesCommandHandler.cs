using LibrarySystem.Domain.Commands;
using LibrarySystem.Domain.Commands.Books;
using LibrarySystem.Domain.Entities;
using LibrarySystem.Domain.Interfaces;

namespace LibrarySystem.Application.Commands.Books.Handlers;

public class UpdateBookCopiesCommandHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateBookCopiesCommand>
{
    public async Task<CommandResult> HandleAsync(UpdateBookCopiesCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            Book? book = await unitOfWork.Books.GetByIdAsync(command.Id).ConfigureAwait(false);
            if (book is null)
                return CommandResult.Fail($"Book with ID {command.Id} not found");

            book.UpdateCopies(command.TotalCopies, command.Reason);
            book.UpdatedBy = command.CommandBy;

            var success = await unitOfWork.CommitAsync().ConfigureAwait(false);

            return success
                ? CommandResult.Ok(book)
                : CommandResult.Fail("Failed to update book copies");
        }
        catch (InvalidOperationException ex)
        {
            return CommandResult.Fail($"Error updating book copies: {ex.Message}");
        }
    }
}
