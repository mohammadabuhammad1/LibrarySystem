using LibrarySystem.Domain.Common;

namespace LibrarySystem.Domain.Commands.Books;

public class CreateBookCommand : BaseCommand
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public int PublishedYear { get; set; }
    public int TotalCopies { get; set; }
    public int? LibraryId { get; set; }
    public string? Description { get; set; }
    public string? Genre { get; set; }
}