using LibrarySystem.Domain.Common;

namespace LibrarySystem.Domain.Commands.Books;

public class DeleteBookCommand : BaseCommand
{
    public int Id { get; set; }
}
