using LibrarySystem.Domain.Commands.Books;
using LibrarySystem.Domain.Entities;
using LibrarySystem.Domain.Interfaces;

namespace LibrarySystem.Application.Commands.Books.Handlers;

public class CreateBookCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<CreateBookCommand>
{
    public async Task<CommandResult> HandleAsync(CreateBookCommand command)
    {

        ArgumentNullException.ThrowIfNull(command);

        try
        {
            Book? existingBook = await unitOfWork.Books.GetByIsbnAsync(command.ISBN).ConfigureAwait(false);
            if (existingBook != null)
                return CommandResult.Fail($"Book with ISBN {command.ISBN} already exists");

            Book book = Book.Create(
                command.Title,
                command.Author,
                command.ISBN,
                command.PublishedYear,
                command.TotalCopies,
                command.LibraryId,
                command.Description,
                command.Genre);

            book.CreatedBy = command.CommandBy;

            Book createdBook = await unitOfWork.Books.AddAsync(book).ConfigureAwait(false);
            bool success = await unitOfWork.CommitAsync().ConfigureAwait(false);

            if (!success)
                return CommandResult.Fail("Failed to create book");

            return CommandResult.Ok(createdBook);
        }
        catch (InvalidOperationException ex)
        {
            return CommandResult.Fail($"Error creating book: {ex.Message}");
        }
    }
}