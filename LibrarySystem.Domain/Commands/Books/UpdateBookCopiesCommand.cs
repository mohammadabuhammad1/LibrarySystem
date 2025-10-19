using LibrarySystem.Domain.Common;

namespace LibrarySystem.Domain.Commands.Books;

public class UpdateBookCopiesCommand : BaseCommand
{
    public int Id { get; set; }
    public int TotalCopies { get; set; }
    public string? Reason { get; set; }  // "Restock", "Lost", "Damaged", etc.
}
